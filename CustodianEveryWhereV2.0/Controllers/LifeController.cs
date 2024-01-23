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
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LifeController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<LifeInsurance> _Buy = null;
        public LifeController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _Buy = new store<LifeInsurance>();
        }

        [HttpPost]
        public async Task<notification_response> GetQuote(LifeQuoteObject quote)
        {
            try
            {
                log.Info($"Raw quote form Life {Newtonsoft.Json.JsonConvert.SerializeObject(quote)}");
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 400,
                        message = "Oops!, some paramters missing from request"
                    };
                }


                var check_user_function = await util.CheckForAssignedFunction("GetQuote", quote.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == quote.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {quote.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(quote.date_of_birth + quote.amount + quote.frequency.ToString(), config.secret_key, quote.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {quote.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                DateTime dateparsed;
                var parse_date = DateTime.TryParse(quote.date_of_birth, out dateparsed);
                if (!parse_date)
                {
                    return new notification_response
                    {
                        status = 405,
                        message = "Oops!, Invalid date"
                    };
                }

                #region
                int today = DateTime.Now.Year;
                DateTime dob = dateparsed;
                var passdate = dob.Year;

                if ((today - passdate) == 18)
                {
                    var month = DateTime.Now.Month;
                    var passedmonth = dob.Month;
                    var day = DateTime.Now.Day;
                    var passedday = dob.Day;
                    if (passedmonth == month)
                    {
                        if (day >= passedday)
                        {
                            if (dob.Month < DateTime.Now.Month)
                            {
                                dob.AddYears(1);
                            }
                            else if (dob.Month == DateTime.Now.Month && dob.Day >= DateTime.Now.Day)
                            {
                                dob.AddYears(1);
                            }
                        }
                        else
                        {
                            log.Info($"First one ==> Customer is less than 18 years of aga {dob}");
                            return new notification_response
                            {
                                status = 406,
                                message = "Sorry, you are ineligible to buy this policy"
                            };


                        }
                    }
                    else if (passedmonth > month)
                    {
                        if (dob.Month < DateTime.Now.Month)
                        {
                            dob.AddYears(1);
                        }
                        else if (dob.Month == DateTime.Now.Month && dob.Day >= DateTime.Now.Day)
                        {
                            dob.AddYears(1);
                        }
                    }
                    else
                    {
                        //Your age is less than 18. You're too young to take up this policy. Kindly confirm you've inputed the right date
                        log.Info($"Your age is less than 18. You're too young to take up this policy. Kindly confirm you've inputed the right date {dob}");
                        return new notification_response
                        {
                            status = 407,
                            message = "Sorry, you are ineligible to buy this policy"
                        };

                    }
                }
                else if ((today - passdate) > 18)
                {

                    if (dob.Month < DateTime.Now.Month)
                    {
                        dob.AddYears(1);
                    }
                    else if (dob.Month == DateTime.Now.Month && dob.Day >= DateTime.Now.Day)
                    {
                        dob.AddYears(1);
                    }
                }
                else
                {
                    log.Info($"First ThirdOne ==> Customer is less than 18 years of aga {dob}");
                    return new notification_response
                    {
                        status = 407,
                        message = "Sorry, you are ineligible to buy this policy"
                    };
                }
                #endregion

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    if (quote.policy_type != PolicyType.TermAssurance)
                    {
                        if (quote.amount < 2000)
                        {
                            return new notification_response
                            {
                                message = "Minimum amount to save for this policy type is (N2,000)",
                                status = 203
                            };
                        }
                    }
                    var clientCode = api.CreateLifeClient("None", "None", "None", "None", dob, "None", "none@gmail.com", "0800000000001");
                    log.Info($"create client response from api {clientCode}");
                    if (string.IsNullOrEmpty(clientCode))
                    {
                        log.Info($"Error generating client code");
                        return new notification_response
                        {
                            status = 408,
                            message = "Sorry, something went wrong while computing premium. Try again"
                        };
                    }
                    var clientobj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(clientCode);
                    if (quote.policy_type.ToString().ToUpper() == PolicyType.CapitalBuilder.ToString().ToUpper())
                    {
                        if (quote.terms < 5 || quote.terms > 25)
                        {
                            return new notification_response
                            {
                                message = "Invalid policy term: the number of term for this policy ranges from 5years  to 25years",
                                status = 203
                            };
                        }
                        var capital = api.GetLifeQuote(Convert.ToInt32(clientobj.webTempClntCode), quote.amount.ToString(), "", 1, await util.Transposer(quote.frequency.ToString().Replace("_", "-").ToLower()), quote.terms, "");
                        if (!string.IsNullOrEmpty(capital))
                        {
                            var quot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(capital);

                            log.Info($"Success computing(CapitalBuilder) premium from service response {capital}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation was successful",
                                data = quot,
                                sum_insured = Convert.ToDecimal(quot.sumInsured),
                                policyterms = Enumerable.Range(5, 21).ToList()
                            };
                            //sumassured = quote.sumInsured.ToString();

                            //placeholder = "CUSTODIAN CAPITAL BUILDER PLAN";
                        }
                        else
                        {
                            log.Info($"Error computing(CapitalBuilder) premium from service response{Newtonsoft.Json.JsonConvert.SerializeObject(capital)}");
                            return new notification_response
                            {
                                status = 204,
                                message = "Sorry, something went wrong while computing premium. Try again",
                            };
                        }
                    }

                    else if (quote.policy_type.ToString().ToUpper() == PolicyType.LifeTimeHarvest.ToString().ToUpper())
                    {
                        int[] terms = { 6, 9, 12, 15, 18, 21, 24 };
                        if (!terms.Any(x => x == Convert.ToInt32(quote.terms)))
                        {

                            return new notification_response
                            {
                                message = "Invalid policy term: the number of terms for this policy are( 6, 9, 12, 15, 18, 21, 24 ) years",
                                status = 203
                            };
                        }
                        var lifetime = api.GetLifeQuote(Convert.ToInt32(clientobj.webTempClntCode), quote.amount.ToString(), "", 24, await util.Transposer(quote.frequency.ToString().Replace("_", "-").ToLower()), quote.terms, "");
                        if (!string.IsNullOrEmpty(lifetime))
                        {
                            var quot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(lifetime);
                            //sumassured = quote.sumInsured.ToString();
                            decimal suminsured = 0;

                            foreach (var item in quot.coverTypeAllocations)
                            {
                                if (item.cvtShtDesc == "LTH")
                                {
                                    suminsured = Convert.ToDecimal(item.cvtSa);
                                }
                            }

                            log.Info($"Success computing(LifeTimeHarvest) premium from service response{Newtonsoft.Json.JsonConvert.SerializeObject(lifetime)}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation was successful",
                                data = quot,
                                sum_insured = suminsured,
                                policyterms = terms
                            };
                            // placeholder = "CUSTODIAN LIFE TIME HARVEST";
                        }
                        else
                        {
                            log.Info($"Error computing(LifeTimeHarvest) premium from service response{lifetime} ");
                            return new notification_response
                            {
                                status = 204,
                                message = "Sorry, something went wrong while computing premium. Try again",
                            };
                        }
                    }
                    else if (quote.policy_type.ToString().ToUpper() == PolicyType.EsusuShield.ToString().ToUpper())
                    {
                        var terms = new List<int>() { 1, 2, 3, 4, 5 };
                        if (!terms.Any(x => x == Convert.ToInt32(quote.terms)))
                        {

                            return new notification_response
                            {
                                message = "Invalid policy term: the number of terms for this policy are(1, 2, 3, 4, 5) years",
                                status = 203
                            };
                        }
                        var esusushiled = api.GetLifeQuote(Convert.ToInt32(clientobj.webTempClntCode), quote.amount.ToString(), "", 14, await util.Transposer(quote.frequency.ToString().Replace("_", "-").ToLower()), quote.terms, "");
                        if (!string.IsNullOrEmpty(esusushiled))
                        {
                            var quot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(esusushiled);
                            //sumassured = quote.sumInsured.ToString();
                            log.Info($"Success computing(Esusu Shield) premium from service response{Newtonsoft.Json.JsonConvert.SerializeObject(esusushiled)}");
                            //placeholder = "CUSTODIAN ESUSU SHIELD";

                            return new notification_response
                            {
                                status = 200,
                                message = "operation was successful",
                                data = quot,
                                sum_insured = Convert.ToDecimal(quot.sumInsured),
                                policyterms = terms
                            };
                        }
                        else
                        {
                            log.Info($"Error computing(Esusu Shield) premium from service response{Newtonsoft.Json.JsonConvert.SerializeObject(esusushiled)}");
                            return new notification_response
                            {
                                status = 204,
                                message = "Sorry, something went wrong while computing premium. Try again",
                            };
                        }
                    }
                    else if (quote.policy_type.ToString().ToUpper() == PolicyType.TermAssurance.ToString().ToUpper())
                    {
                        if (quote.terms > 1)
                        {
                            log.Info($"Defaut value used durations was set to 1year amount {quote.amount.ToString()}");
                            quote.terms = 1;
                        }
                        var TermAssurance = api.GetLifeQuote(Convert.ToInt32(clientobj.webTempClntCode), quote.amount.ToString(), "", 40, "F", Convert.ToInt32(quote.terms), "");

                        if (!string.IsNullOrEmpty(TermAssurance))
                        {
                            var quot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(TermAssurance);
                            // var parseHtml = new HtmlDocument();

                            return new notification_response
                            {
                                status = 200,
                                message = "operation was successful",
                                data = quot,
                                sum_insured = Convert.ToDecimal(quot.sumInsured)
                            };
                        }
                        else
                        {
                            log.Info($"Error computing(Term Assurance) premium from service response{Newtonsoft.Json.JsonConvert.SerializeObject(TermAssurance)}");
                            return new notification_response
                            {
                                status = 204,
                                message = "Sorry, something went wrong while computing premium. Try again",
                            };
                        }
                    }
                    else if (quote.policy_type.ToString().ToUpper() == PolicyType.WealthPlus.ToString().ToUpper())
                    {
                        if ((today - passdate) > 60)
                        {
                            return new notification_response
                            {
                                status = 204,
                                message = "Sorry, you are ineligible to buy this policy min age: 18 max age: 60",
                            };
                        }

                        if (quote.sum_assured <= 0)
                        {
                            return new notification_response
                            {
                                status = 204,
                                message = "WPP(Wealth Plus Plan) requires sum assured",
                            };
                        }


                        var validate2 = await util.ValidateWealthPlusCoverLimits(quote.sum_assured, quote.frequency, quote.amount, quote.terms);
                        if (validate2 != null && validate2.status != 200)
                        {
                            return new notification_response
                            {
                                status = 302,
                                message = validate2.message
                            };
                        }

                        string wealthplus = api.GetLifeQuote(Convert.ToInt32(clientobj.webTempClntCode), quote.amount.ToString(), quote.sum_assured.ToString(), 41, await util.Transposer(quote.frequency.ToString().Replace("_", "-").ToLower()), quote.terms, "");
                        log.Info($"Raw response: {wealthplus}");
                        if (!string.IsNullOrEmpty(wealthplus))
                        {
                            var _quote = Newtonsoft.Json.JsonConvert.DeserializeObject<WealthPlusResponse>(wealthplus);
                            // var parseHtml = new HtmlDocument();
                            log.Info($"Value from QT:{_quote.investmentAmount}");
                            //decimal investmentAmount = 0m;
                            // var investmentAmount = Convert.ToDouble($"{_quote.investmentAmount}");

                            var validate = await util.ValidateWealthPlusCoverLimits(quote.sum_assured, quote.frequency, _quote.investmentAmount, quote.terms);
                            if (validate != null && validate.status != 200)
                            {
                                return new notification_response
                                {
                                    status = 302,
                                    message = validate.message
                                };
                            }

                            return new notification_response
                            {
                                status = 200,
                                message = "operation was successful",
                                data = _quote,
                                sum_insured = validate.projectedAmount
                            };
                        }
                        else
                        {
                            log.Info($"Error computing(wealthplus) premium from service response{Newtonsoft.Json.JsonConvert.SerializeObject(wealthplus)}");
                            return new notification_response
                            {
                                status = 204,
                                message = "Sorry, something went wrong while computing premium. Try again",
                            };
                        }
                    }
                }

                return new notification_response
                {
                    status = 209,
                    message = "Invalid policy type",
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "Oops!, something happened while calculating premium"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> BuyLifeInsurance(LifePolicy BuyLife)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 400,
                        message = "Oops!, some paramters missing from request"
                    };
                }


                var check_user_function = await util.CheckForAssignedFunction("BuyLifeInsurance", BuyLife.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == BuyLife.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {BuyLife.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(BuyLife.date_of_birth + BuyLife.premium + BuyLife.frequency.ToString() + BuyLife.phonenumber + BuyLife.policytype.ToString(), config.secret_key, BuyLife.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {BuyLife.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //CUSTODIAN CAPITAL BUILDER PLAN  CUSTODIAN ESUSU SHIELD CUSTODIAN LIFE TIME HARVEST
                string PlaceHolder = string.Empty;
                if (BuyLife.policytype.ToString().ToUpper() == PolicyType.CapitalBuilder.ToString().ToUpper())
                {
                    PlaceHolder = "CUSTODIAN CAPITAL BUILDER";
                }
                else if (BuyLife.policytype.ToString().ToUpper() == PolicyType.EsusuShield.ToString().ToUpper())
                {
                    PlaceHolder = "CUSTODIAN ESUSU SHIELD";
                }
                else if (BuyLife.policytype.ToString().ToUpper() == PolicyType.LifeTimeHarvest.ToString().ToUpper())
                {
                    PlaceHolder = "CUSTODIAN LIFE TIME HARVEST";
                }
                else if (BuyLife.policytype.ToString().ToUpper() == PolicyType.TermAssurance.ToString().ToUpper())
                {
                    PlaceHolder = "CUSTODIAN TERM ASSURANCE";
                }
                else if (BuyLife.policytype.ToString().ToUpper() == PolicyType.WealthPlus.ToString().ToUpper())
                {
                    PlaceHolder = "CUSTODIAN WEALTH PLUS";
                }
                var checkme = await _Buy.FindOneByCriteria(x => x.reference.ToLower() == BuyLife.payment_reference.ToLower());
                if (checkme != null)
                {
                    log.Info($"duplicate request {BuyLife.merchant_id}");
                    return new notification_response
                    {
                        status = 300,
                        message = "Duplicate request"
                    };
                }

                var newBuyer = new LifeInsurance
                {
                    address = BuyLife.address,
                    indentity_type = BuyLife.indentity_type,
                    computed_premium = BuyLife.computed_premium,
                    date_of_birth = Convert.ToDateTime(BuyLife.date_of_birth),
                    emailaddress = BuyLife.emailaddress,
                    frequency = BuyLife.frequency.ToString().Replace("_", "-").ToUpper(),
                    gender = BuyLife.gender.ToString().ToUpper(),
                    insured_name = BuyLife.insured_name,
                    occupation = BuyLife.occupation,
                    phonenumber = BuyLife.phonenumber,
                    premium = BuyLife.premium,
                    terms = BuyLife.terms,
                    policytype = PlaceHolder,
                    id_number = BuyLife.id_number,
                    reference = BuyLife.payment_reference,
                    merchant_id = BuyLife.merchant_id,
                    referralCode = BuyLife.referralCode
                };

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.SubmitPaymentRecord(GlobalConstant.merchant_id, GlobalConstant.password, "NA",
                        "Life", $"NewBusniness|{BuyLife.payment_reference}", Convert.ToDateTime(BuyLife.date_of_birth), DateTime.Now, BuyLife.payment_reference, BuyLife.insured_name, "", "", BuyLife.address, BuyLife.phonenumber,
                        BuyLife.emailaddress, BuyLife.terms.ToString(), BuyLife.frequency.ToString().Replace("_", "-"), "", PlaceHolder, BuyLife.premium, BuyLife.computed_premium, "ADAPT", "NB", BuyLife.referralCode ?? "", "");
                    log.Info("RAW Response from api" + request);
                    if (!string.IsNullOrEmpty(request))
                    {
                        if (!string.IsNullOrEmpty(BuyLife.base64Image))
                        {
                            var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{BuyLife.payment_reference}.{BuyLife.base64ImageFormat}";
                            var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Life/{nameurl}";
                            byte[] content = Convert.FromBase64String(BuyLife.base64Image);
                            File.WriteAllBytes(filepath, content);
                            newBuyer.pathname = nameurl;
                            newBuyer.base64ImageFormat = BuyLife.base64ImageFormat;
                            log.Info($"Raw object {Newtonsoft.Json.JsonConvert.SerializeObject(newBuyer)}");
                        }
                        await _Buy.Save(newBuyer);
                        var shorturl = (GlobalConstant.Reciept_url + $"FinalReceipt.aspx?mUser=CUST_WEB&mCert={BuyLife.payment_reference}&mCert2={BuyLife.payment_reference}");
                        return new notification_response
                        {
                            status = 200,
                            message = "operation was successful",
                            reciept_url = shorturl
                        };
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 407,
                            message = "Oops!, something happened while processing request"
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
                    message = "Oops!, something happened while calculating premium"
                };
            }
        }
    }
}
