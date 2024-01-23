using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SafetyPlusController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<SafetyPlus> safetyplus = null;

        public SafetyPlusController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            safetyplus = new store<SafetyPlus>();
        }

        [HttpGet]
        public async Task<notification_response> GetUnits(int NoOfUnits, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetUnits", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                if (NoOfUnits <= 0)
                {
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid units provided"
                    };
                }
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    string request = api.GetSafetyplusQuote(NoOfUnits);
                    if (!string.IsNullOrEmpty(request))
                    {
                        if (config.merchant_name.ToLower() != "adapt")
                        {
                            return new notification_response
                            {
                                status = 200,
                                message = "Units calculated successfully",
                                data = new
                                {
                                    quote = request
                                }
                            };
                        }
                        else
                        {
                            return new notification_response
                            {
                                status = 200,
                                message = "Units calculated successfully",
                                data = request
                            };
                        }
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "Unable to get units"
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happened while calculating units"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> BuySafetyPlus(SafetyRequest safe)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 202,
                        message = "Some parameters missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("BuySafetyPlus", safe.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == safe.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {safe.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(safe.merchant_id + safe.NoOfUnit + safe.Email + safe.Reference, config.secret_key, safe.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {safe.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var checkme = await safetyplus.FindOneByCriteria(x => x.Reference.ToLower() == safe.Reference.ToLower());
                if (checkme != null)
                {
                    log.Info($"duplicate request {safe.merchant_id}");
                    return new notification_response
                    {
                        status = 300,
                        message = "Duplicate request"
                    };
                }
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.PostSafetyPlus(GlobalConstant.merchant_id,
                        GlobalConstant.password, safe.CustomerName, safe.Address,
                        safe.PhoneNumber, safe.Email, safe.Occupation, safe.Premium, safe.NoOfUnit, DateTime.Now, DateTime.Now,
                        DateTime.Now.AddMonths(12), safe.Reference, safe.Description,
                        safe.BeneficiaryName, safe.BeneficiarySex.ToString(), safe.BeneficiaryDOB, safe.BeneficiaryRelatn, "API", safe.referralCode ?? "", "", "");

                    log.Info($"Response from api {request}");
                    if (!string.IsNullOrEmpty(request))
                    {
                        var safety_plus = new SafetyPlus
                        {
                            Activedate = DateTime.Now,
                            Reference = safe.Reference,
                            Address = safe.Address,
                            BeneficiaryDOB = safe.BeneficiaryDOB,
                            BeneficiaryName = safe.BeneficiaryName,
                            BeneficiaryRelatn = safe.BeneficiaryRelatn,
                            BeneficiarySex = safe.BeneficiarySex,
                            DateCreated = DateTime.Now,
                            Description = safe.Description,
                            Email = safe.Email,
                            ExpiryDate = DateTime.Now.AddMonths(12),
                            IdentificationType = safe.IdentificationType,
                            NoOfUnit = safe.NoOfUnit,
                            Occupation = safe.Occupation,
                            PhoneNumber = safe.PhoneNumber,
                            Premium = safe.Premium,
                            CustomerDOB = safe.CustomerDOB,
                            IndetificationNUmber = safe.IdentificationNumber,
                            Merchant_Id = safe.merchant_id,
                            referralCode = safe.referralCode
                        };
                        var cert_code = request.Replace("**", "|")?.Split('|')[1];
                        var policy_number = request.Replace("**", "|")?.Split('|')[0];
                        safety_plus.policyNumber = policy_number;
                        var reciept_base_url = ConfigurationManager.AppSettings["Reciept_Base_Url"];
                        if (!string.IsNullOrEmpty(safe.ImageBase64))
                        {
                            var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{safe.Reference}.{safe.ImageFormat}";
                            var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/General/{nameurl}";
                            byte[] content = Convert.FromBase64String(safe.ImageBase64);
                            File.WriteAllBytes(filepath, content);
                            safety_plus.ImagePath = nameurl;
                        }
                        //log.Info($"Raw data from safety plus {Newtonsoft.Json.JsonConvert.SerializeObject(safety_plus)}");
                        await safetyplus.Save(safety_plus);
                        return new notification_response
                        {
                            status = 200,
                            message = "Payment processing was successful",
                            data = new Dictionary<string, string>
                            {
                                {"cert_url",reciept_base_url+$"mUser=CUST_WEB&mCert={cert_code}&mCert2={cert_code}" },
                                {"policy_number",safety_plus.policyNumber }
                            }
                        };

                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 401,
                            message = "Unable to push transaction please"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happened while submitting details"
                };
            }
        }
    }
}
