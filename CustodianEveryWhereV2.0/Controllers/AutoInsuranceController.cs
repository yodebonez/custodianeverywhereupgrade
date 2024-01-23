using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml.Serialization;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AutoInsuranceController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private store<AutoInsurance> auto = null;
        public AutoInsuranceController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
            auto = new store<AutoInsurance>();
        }

        [HttpGet]
        public async Task<res> AutoReg(string regno, string merchant_id)
        {
            try
            {
                if (string.IsNullOrEmpty(regno) || string.IsNullOrEmpty(merchant_id))
                {
                    return new res { message = $"Invalid Registration Number or Merchant Id ({regno})", status = 405 };
                }

                var check_user_function = await util.CheckForAssignedFunction("AutoReg", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }


                return new res
                {
                    status = 405,
                    message = "Features temporary disabled"
                };
                #region old api version-- depricated by auto reg
                //var reg = new AutoReg.VehicleCheckSoapClient();
                //var resp = await reg.WS_LicenseInfoByRegNoAsync(regno, "", "", "", "", "", "6492CUS15");
                //if (resp.Contains("</LicenseInfo>"))
                //{
                //    var index = resp.IndexOf("<Response>");
                //    var remove = resp.Remove(index, resp.Length - resp.Substring(0, index).Length);
                //    XmlSerializer serializer = new XmlSerializer(typeof(LicenseInfo), new XmlRootAttribute("LicenseInfo"));
                //    StringReader stringReader = new StringReader(remove);
                //    LicenseInfo details = (LicenseInfo)serializer.Deserialize(stringReader);
                //    return new res
                //    {
                //        message = "Registration Number Is Valid",
                //        status = 200,
                //        data = details
                //    };
                //}
                //else
                //{
                //    XmlSerializer serializer = new XmlSerializer(typeof(response), new XmlRootAttribute("Response"));
                //    StringReader stringReader = new StringReader(resp);
                //    response details = (response)serializer.Deserialize(stringReader);
                //    return new res
                //    {
                //        message = details.ResponseMessage,
                //        status = 402
                //    };
                //}
                #endregion


                using (var api = new HttpClient())
                {
                    regno = Regex.Replace(regno, @"\s", "");// remove spaces between text
                    var api_key = ConfigurationManager.AppSettings["AutoRegAPIKey"];
                    var url = ConfigurationManager.AppSettings["AutoRegAPIUrl"];
                    var request = await api.GetAsync($"{url}/{regno}/{api_key}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new res
                        {
                            message = "Unable to fetch vehicle information",
                            status = (int)request.StatusCode
                        };
                    }
                    var response = await request.Content.ReadAsStringAsync();
                    var details = Newtonsoft.Json.JsonConvert.DeserializeObject<LicenseInfo>(response);
                    return new res
                    {
                        message = "Registration Number Is Valid",
                        status = 200,
                        data = details
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpPost]
        public async Task<res> GetAutoQuote(AutoQuoute quote)
        {
            try
            {
                log.Info($"request from {Newtonsoft.Json.JsonConvert.SerializeObject(quote)}");

                if (!ModelState.IsValid)
                {
                    return new res
                    {
                        status = 409,
                        message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    };
                }
                // check if user has access to function
                var check_user_function = await util.CheckForAssignedFunction("GetAutoQuote", quote.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == quote.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {quote.merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash

                var checkhash = await util.ValidateHash2(quote.vehicle_value + quote.cover_type.ToString() + quote.tracking + quote.excess + quote.srcc + quote.flood, config.secret_key, quote.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {quote.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    log.Info($"cover type {quote.cover_type.ToString()}");
                    var request = api.GetMotorQuote(quote.cover_type.ToString().Replace("_", " "),
                         quote.vehicle_category, quote.vehicle_value.ToString(),
                        !string.IsNullOrEmpty(quote.payment_option) ? quote.payment_option : "",
                        quote.excess, quote.tracking, quote.flood, quote.srcc);
                    log.Info($"Raw quote computed from {quote.merchant_id} is {request}");
                    if (!string.IsNullOrEmpty(request))
                    {
                        log.Info($"quote computed successfully {quote.merchant_id}");
                        decimal amount = 0;
                        var parse_amount = decimal.TryParse(request, out amount);
                        return new res
                        {
                            status = (amount > 0) ? 200 : 407,
                            message = (amount > 0) ? "Quote computed successfully" : "Quote computation error",
                            data = (amount > 0) ? new Dictionary<string, decimal> { { "quote_amount", amount } } : null
                        };
                    }
                    else
                    {
                        return new res { message = "Quote computation not successful", status = 405 };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpPost]
        public async Task<res> BuyAutoInsurance(Auto Auto)
        {
            try
            {
                log.Info($"request from {Newtonsoft.Json.JsonConvert.SerializeObject(Auto)}");
                if (!ModelState.IsValid)
                {
                    return new res
                    {
                        status = 409,
                        message = "Some required parameters missing from request =>" + string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    };
                }
                // check if user has access to function

                var check_user_function = await util.CheckForAssignedFunction("BuyAutoInsurance", Auto.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == Auto.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {Auto.merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(Auto.premium.ToString() + Auto.sum_insured.ToString() + Auto.insurance_type.ToString() + Auto.reference_no, config.secret_key, Auto.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {Auto.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var checkme = await auto.FindOneByCriteria(x => x.reference_no.ToLower() == Auto.reference_no.ToLower());
                if (checkme != null)
                {
                    log.Info($"duplicate request {Auto.merchant_id}");
                    return new res
                    {
                        status = 300,
                        message = "Duplicate request"
                    };
                }
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    string request = null;
                    string request2 = null;
                    var source = (config.merchant_name.ToLower().Contains("adapt")) ? "ADAPT" : "API";
                    if (Auto.insurance_type == TypeOfCover.Comprehensive)
                    {
                        var count = await auto.GetAll();//TODO: this is not scaleable , just get count only instead of loading the entire record to get count 
                        var resp = await util.SendQuote(Auto, count.Count() + 1);
                        request = api.SubmitPaymentRecord(GlobalConstant.merchant_id,
                            GlobalConstant.password, "", "", "", Auto.dob ?? DateTime.Now, DateTime.Now, "",
                            Auto.customer_name, "", "", Auto.address, Auto.phone_number, Auto.email, Auto.payment_option, "", "",
                            Auto.insurance_type.ToString().Replace("_", " ").Replace("And", "&"), Auto.premium, Auto.sum_insured, "ADAPT", "NB", Auto.referralCode, Auto.registration_number);

                        if (resp.status == 200)
                        {
                            request2 = "success";
                        }
                        else
                        {
                            request2 = "failed";
                        }

                    }
                    else
                    {
                        request = api.POSTMotorRec(GlobalConstant.merchant_id, GlobalConstant.password,
                           Auto.customer_name, Auto.address ?? "", Auto.phone_number, Auto.email ?? "", Auto.engine_number,
                           Auto.insurance_type.ToString().Replace("_", " ").Replace("And", "&"), Auto.premium, Auto.sum_insured
                           , Auto.chassis_number, Auto.registration_number, Auto.vehicle_model,
                           Auto.vehicle_model, Auto.vehicle_color, Auto.vehicle_model, Auto.vehicle_type, Auto.vehicle_year,
                           DateTime.Now, DateTime.Now, DateTime.Now.AddMonths(12), Auto.reference_no, "", source, Auto.referralCode ?? "", "", "");
                    }

                    log.Info($"Response from Api {request}");

                    //HO/V/29/G0000529E|17294
                    if (!string.IsNullOrEmpty(request) || request.ToLower() == "success" || request2 == "success")
                    {

                        var save_new = new AutoInsurance
                        {
                            address = Auto.address,
                            chassis_number = Auto.chassis_number,
                            create_at = DateTime.Now,
                            customer_name = Auto.customer_name,
                            dob = Auto.dob ?? null,
                            email = Auto.email,
                            engine_number = Auto.engine_number,
                            extension_type = Auto.extension_type,
                            id_number = Auto.id_number,
                            id_type = Auto.id_type,
                            insurance_type = Auto.insurance_type,
                            occupation = Auto.occupation,
                            phone_number = Auto.phone_number,
                            premium = Auto.premium,
                            reference_no = Auto.reference_no,
                            registration_number = Auto.registration_number,
                            sum_insured = Auto.sum_insured,
                            vehicle_category = Auto.vehicle_category,
                            vehicle_color = Auto.vehicle_color,
                            vehicle_model = Auto.vehicle_model,
                            vehicle_type = Auto.vehicle_type,
                            vehicle_year = Auto.vehicle_year,
                            excess = Auto.excess,
                            payment_option = Auto.payment_option,
                            flood = Auto.flood,
                            tracking = Auto.tracking,
                            start_date = Auto.start_date ?? DateTime.Now,
                            srcc = Auto.srcc,
                            merchant_id = Auto.merchant_id,
                            referralCode = Auto.referralCode

                        };
                        string cert_url = "";
                        string cert_number = Guid.NewGuid().ToString();
                        if (Auto.insurance_type != TypeOfCover.Comprehensive)
                        {
                            var cert_code = request.Replace("**", "|")?.Split('|')[1];
                            var policy_number = request.Replace("**", "|")?.Split('|')[0];
                            var reciept_base_url = ConfigurationManager.AppSettings["Reciept_Base_Url"];
                            cert_number = cert_code;
                            save_new.policyNumber = policy_number;
                            cert_url = $"{reciept_base_url}mUser=CUST_WEB&mCert={cert_code}&mCert2={cert_code}";
                        }
                        if (!string.IsNullOrEmpty(Auto.attachment))
                        {

                            try
                            {
                                var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{cert_number}.{Auto.extension_type}";
                                var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Auto/{nameurl}";
                                byte[] content = Convert.FromBase64String(Auto.attachment);
                                File.WriteAllBytes(filepath, content);
                                save_new.attachemt = nameurl;
                                save_new.extension_type = Auto.extension_type;
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex.Message);
                                log.Error(ex.StackTrace);
                                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                                return new res
                                {
                                    status = 308,
                                    message = "Unable to decode attachment"
                                };
                            }

                        }

                        await auto.Save(save_new);
                        return new res
                        {
                            status = 200,
                            message = "Transaction was successful",
                            data = new Dictionary<string, string>
                            {
                                {"cert_url", cert_url},
                                {"policyNo",save_new.policyNumber }
                            }
                        };
                    }
                    else
                    {
                        return new res
                        {
                            status = 308,
                            message = "Transaction was not successful"
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }
        [HttpGet]
        public async Task<dynamic> USSDThridPartyProductCategory(string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetUSSDProductCategory", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                return new
                {
                    status = 200,
                    message = "operation successful",
                    data = new List<dynamic>()
                    {
                        new
                        {
                            type = "Private car",
                            premium = 5000,
                            display = "Private car N5,000"
                        },
                         new
                        {
                            type = "Own goods and Staff bus",
                            premium = 7500,
                            display = "Own goods n Staff bus N7,500"
                        },
                           new
                        {
                            type = "Trucks and Cartage",
                            premium = 25000,
                            display = "Trucks n Cartage N25,000"
                        },
                          new
                        {
                            type = "Ambulance",
                            premium = 5000,
                            display = "Ambulance N5000"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }
    }
}
