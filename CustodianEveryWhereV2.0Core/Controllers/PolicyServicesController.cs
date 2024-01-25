using CustodianEmailSMSGateway.Email;
using CustodianEmailSMSGateway.SMS;
using DapperLayer.Dapper.Core;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using ActionFilter;

namespace CustodianEveryWhereV2._0.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyServicesController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private store<PolicyServicesDetails> policyService = null;
        private Core<policyInfo> _policyinfo = null;
        public PolicyServicesController(IWebHostEnvironment hostingEnvironment)
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
            policyService = new store<PolicyServicesDetails>();
            _policyinfo = new Core<policyInfo>();
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("{policy?}")]
        [ValidateJWT]
        public async Task<res> Setup(setup policy)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return new res { message = "provide poilcy number", status = (int)HttpStatusCode.ExpectationFailed };
                }

                var check_user_function = await util.CheckForAssignedFunction("PolicyServicesSetup", policy.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == policy.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {policy.merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(policy.policynumber, config.secret_key, policy.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {policy.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var lookup = await _policyinfo.GetPolicyServices(policy.policynumber);
                if (lookup == null || lookup.Count() <= 0)
                {
                    return new res
                    {
                        status = 207,
                        message = "Policy not found (Please contact custodian carecentre)"
                    };
                }

                var email = lookup.FirstOrDefault(x => util.IsValid(x.email?.Trim()) == true)?.email;
                var phone = util.numberin234(lookup.FirstOrDefault(x => util.isValidPhone(x.phone?.Trim()) == true)?.phone);

                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone))
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.BadRequest,
                        message = "No valid phone number or email attached to provided policy (Please contact custodian carecentre to update your record)"
                    };
                }
                var obj = lookup.First();

                var validate = await policyService.FindOneByCriteria(x => x.customerid == obj.customerid.ToString() && x.is_setup_completed == true);
                if (validate != null)
                {
                    return new res
                    {
                        status = 201,
                        message = "User has already been setup",
                        data = new
                        {
                            customer_id = validate.customerid,
                            email = validate.email,
                            phonenumber = validate.phonenumber
                        }
                    };
                }

                var generate_otp = await util.GenerateOTP(false, email?.Trim() ?? phone?.Trim(), "POLICYSERVICE", Platforms.ADAPT);
                string messageBody = $"Adapt Policy Services authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";


                var rootPath = _hostingEnvironment.ContentRootPath;
                var filePath = Path.Combine(rootPath, "Cert", "Adapt.html");
                var template = System.IO.File.ReadAllText(filePath);
               
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#CONTENT#", messageBody);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));

                var imagepath = Path.Combine(rootPath, "Images", "adapt_logo.png");
             
                List<string> cc = new List<string>();
                //cc.Add("technology@custodianplc.com.ng");
                //use handfire
                if (!string.IsNullOrEmpty(email))
                {
                    string test_email = "oscardybabaphd@gmail.com";
                    //email.email
                    var test = Config.isDemo ? "Test" : null;
                    var _email = Config.isDemo ? test_email : email;
                    new SendEmail().Send_Email(_email,
                               $"Adapt-PolicyServices Authentication {test}",
                               sb.ToString(), $"PolicyServices Authentication {test}",
                               true, imagepath, null, null, null);
                }

                if (!Config.isDemo)
                {
                    if (!string.IsNullOrEmpty(phone))
                    {
                        //phone = "2348069314541";
                        await new SendSMS().Send_SMS($"Adapt OTP: {generate_otp}", phone);
                    }
                }
                dynamic pol = new ExpandoObject();


                //save record and set status inactive
                var check_setup_has_started = await policyService.FindOneByCriteria(x => x.customerid == obj.customerid.ToString().Trim());
                if (check_setup_has_started == null)
                {
                    var savePolicy = new PolicyServicesDetails
                    {
                        createdat = DateTime.UtcNow,
                        customerid = obj.customerid.ToString().Trim(),
                        deviceimei = policy.imei,
                        devicename = policy.devicename,
                        email = obj.email?.ToLower(),
                        is_setup_completed = false,
                        phonenumber = util.numberin234(obj.phone?.Trim()),
                        policynumber = obj.policyno?.Trim().ToUpper(),
                        os = policy.os,

                    };

                    if (await policyService.Save(savePolicy))
                    {
                        return new res
                        {
                            message = $"OTP has been sent to email {obj.email} and phone {obj.phone} attached to your policy",
                            status = (int)HttpStatusCode.OK,
                            data = new
                            {
                                customer_id = obj.customerid,
                                email = obj.email?.Trim(),
                                phonenumber = util.numberin234(obj.phone?.Trim())
                            }
                        };
                    }
                    else
                    {
                        return new res
                        {
                            message = "Something happend while processing your information",
                            status = (int)HttpStatusCode.InternalServerError,
                        };
                    }
                }
                else
                {
                    //var savePolicy = new PolicyServicesDetails
                    //{
                    //    createdat = DateTime.UtcNow,
                    //    customerid = obj.customerid.ToString().Trim(),
                    //    deviceimei = policy.imei,
                    //    devicename = policy.devicename,
                    //    email = obj.email?.ToLower(),
                    //    is_setup_completed = false,
                    //    phonenumber = obj.phone,
                    //    policynumber = obj.policyno?.Trim().ToUpper(),
                    //    os = policy.os,

                    //};

                    //if(check_setup_has_started.email !=)
                    return new res
                    {

                        message = $"OTP has been sent to email {obj.email} and phone {obj.phone} attached to your policy",
                        status = (int)HttpStatusCode.OK,
                        data = new
                        {
                            customer_id = check_setup_has_started.customerid,
                            email = check_setup_has_started.email,
                            phonenumber = util.numberin234(check_setup_has_started.phonenumber)
                        }
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpPost("{validate?}")]
        [ValidateJWT]
        public async Task<res> ValidateOTP(ValidatePolicy validate)
        {
            try
            {

                var check_user_function = await util.CheckForAssignedFunction("PolicyServicesSetupValidateOTP", validate.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == validate.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {validate.merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }
                var getRecord = await policyService.FindOneByCriteria(x => x.customerid == validate.customerid);
                if (getRecord != null && getRecord.is_setup_completed)
                {
                    log.Info($"valid merchant Id {validate.merchant_id} setup completed");
                    return new res
                    {
                        status = 201,
                        message = "User has finished setting up profile kindly login to access your policy services"
                    };
                }

                var checkhash = await util.ValidateHash2(validate.otp + validate.customerid, config.secret_key, validate.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {validate.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var validateOTP = await util.ValidateOTP(validate.otp, validate.email.ToLower());
                if (!validateOTP)
                {
                    log.Info($"invalid OTP for {validate.merchant_id} email {validate.email}");
                    return new res
                    {
                        status = 405,
                        message = "Invalid OTP"
                    };
                }
                else
                {
                    if (getRecord == null)
                    {
                        return new res
                        {
                            status = 407,
                            message = "Invalid customer id"
                        };
                    }
                    getRecord.updatedat = DateTime.UtcNow;
                    getRecord.is_setup_completed = true;
                    getRecord.pin = util.Sha256(validate.pin);
                    await policyService.Update(getRecord);
                    return new res
                    {
                        status = 200,
                        message = "OTP Validated successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}/{pin?}/{customer_id?}/{hash?}")]
        [ValidateJWT]
        public async Task<res> GetPolicies(string merchant_id, string pin, string customer_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(customer_id + pin, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var _pin = util.Sha256(pin);
                var check_setup = await policyService.FindOneByCriteria(x => x.customerid == customer_id.Trim() && x.pin == _pin);

                if (check_setup == null)
                {
                    return new res
                    {
                        status = 405,
                        message = "Invalid customer PIN"
                    };
                }

                var lookup = await _policyinfo.GetPolicyServices(check_setup.policynumber);
                if (lookup == null || lookup.Count() <= 0)
                {
                    return new res
                    {
                        status = 207,
                        message = "Policy not found (Please contact custodain care centre)"
                    };
                }
                var build = lookup.Select(x => new
                {
                    PolicyNo = x.policyno?.Trim(),
                    StartDate = x.startdate.ToShortDateString(),
                    EndDate = x.enddate.ToShortDateString(),
                    Source = (x.datasource == "ABS") ? "General" : "Life",
                    ProductName = x.productdesc,
                    ProductType = x.productsubdesc ?? x.productdesc,
                    PolicyNumber = x.policyno?.Trim(),
                    Status = x.status?.Trim().ToUpper(),
                    PolicyType = (x.datasource == "ABS") ? util.PolicyType(x.policyno?.Trim().ToUpper()) : "Life",
                    ClaimTypes = (x.datasource == "ABS") ? util.GeneralClaimType(x.policyno?.Trim().ToUpper()) : util.LifeClaimTypes(x.productdesc?.Trim().ToLower()),
                    Division = (x.datasource == "ABS") ? util.GetGeneralDivision(x.policyno?.Trim().ToLower()) :
                    new DivisonsCode
                    {
                        name = "LIFE",
                        code = "LIFE"
                    }

                }).ToList();

                dynamic pol = new ExpandoObject();
                var obj = lookup.First();
                pol.FullName = obj.fullname;
                pol.CustomerId = obj.customerid;
                pol.Phone = obj.phone;
                pol.Email = obj.email;
                pol.PolicyList = build;

                log.Info($"Policies fetched succesully for {obj.email}");
                return new res
                {
                    status = 200,
                    message = "Policies fetched succesully",
                    data = pol
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}/{newpin?}/{otp?}/{customer_id?}/{hash?}")]
        [ValidateJWT]
        public async Task<res> ResetPIN(string merchant_id, string newpin, string otp, string customer_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ResetPINPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(customer_id + newpin + otp, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }


                var getCustomer = await policyService.FindOneByCriteria(x => x.customerid == customer_id);


                if (getCustomer == null)
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var validateOTP = await util.ValidateOTP(otp, getCustomer.email?.ToLower() ?? getCustomer.phonenumber);
                if (!validateOTP)
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid OTP"
                    };
                }
                var _pin = util.Sha256(newpin);
                getCustomer.pin = _pin;
                getCustomer.updatedat = DateTime.UtcNow;
                if (await policyService.Update(getCustomer))
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.OK,
                        message = "Pin update successfully"
                    };
                }
                else
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.NotAcceptable,
                        message = "Updated was not successful"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}/{source?}/{pin?}/{policynumber?}/{customer_id?}/{hash?}")]
        [ValidateJWT]
        public async Task<res> GetPolicyDetails(string merchant_id, string source, string pin, string policynumber, string customer_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPolicyDetailsPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(policynumber + pin, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var encryptPin = util.Sha256(pin);
                var checkuser = await policyService.FindOneByCriteria(x => x.pin == encryptPin && x.customerid == customer_id?.Trim());
                if (checkuser == null)
                {
                    log.Info($"Pin authentication failed {policynumber}");
                    return new res
                    {
                        status = 409,
                        message = $"Invalid PIN for '{policynumber}'"
                    };
                }
                var lookup = await _policyinfo.GetPolicyServices(policynumber);

                if (lookup.Count() > 0)
                {
                    if (customer_id.Trim() != lookup.First().customerid.ToString().Trim())
                    {
                        return new res
                        {
                            status = 309,
                            message = $"Customer ID does not match login information"
                        };
                    }
                }
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetMorePolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, source, policynumber);
                    if (request == null)
                    {
                        log.Info($"Unable to fetch policy with policynumber {policynumber}");
                        return new res
                        {
                            status = 409,
                            message = $"Unable to fetch policy with policynumber '{policynumber}'"
                        };
                    }

                    List<string> motor = new List<string>() { "car", "third", "vehicle" };
                    return new res
                    {
                        status = 200,
                        message = "fetch was successful",
                        data = new
                        {
                            AgenctName = request.AgenctName?.Trim(),
                            BizBranch = request.BizBranch?.Trim(),
                            AgenctNum = request.AgenctNum?.Trim(),
                            BizUnit = request.BizUnit?.Trim(),
                            Enddate = request.Enddate,
                            InsAddr1 = request.InsAddr1?.Trim(),
                            InsAddr2 = request.InsAddr2?.Trim(),
                            InsAddr3 = request.InsAddr3?.Trim(),
                            InsLGA = request.InsLGA?.Trim(),
                            InsOccup = request.InsOccup?.Trim(),
                            InsState = request.InsState?.Trim(),
                            InstPremium = request.InstPremium,
                            InsuredEmail = (Config.isDemo) ? "demo@gmail.com" : (request.InsuredEmail?.Trim() == null || string.IsNullOrEmpty(request.InsuredEmail?.Trim()) || request.InsuredEmail?.Trim() == "NULL") ? $"{Guid.NewGuid().ToString().Split('-')[0]}@gmail.com" : request.InsuredEmail?.Trim(),
                            InsuredName = request.InsuredName?.Trim(),
                            InsuredNum = request.InsuredNum?.Trim(),
                            InsuredOthName = request.InsuredOthName?.Trim(),
                            InsuredTelNum = request.InsuredTelNum?.Trim(),
                            PolicyEBusiness = request.PolicyEBusiness?.Trim(),
                            PolicyNo = request.PolicyNo?.Trim(),
                            Startdate = request.Startdate,
                            SumIns = request.SumIns,
                            TelNum = request.TelNum?.Trim(),
                            OutPremium = request.OutPremium,
                            mPremium = request.mPremium
                        },
                        extra_data = new
                        {
                            Category = (motor.Any(x => request.BizUnit.ToLower().Contains(x.ToLower()))) ? "MOTOR" : "NON_MOTOR",
                        }

                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{customer_id?}/{merchant_id?}/{hash?}")]
        [ValidateJWT]
        public async Task<res> GenerateOTP(string customer_id, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GenerateOTPPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(customer_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {customer_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var checkuser = await policyService.FindOneByCriteria(x => x.customerid == customer_id);
                if (checkuser == null)
                {
                    log.Info($"Invalid customer id {customer_id}");
                    return new res
                    {
                        status = 409,
                        message = $"Customer id is not valid"
                    };
                }

                var generate_otp = await util.GenerateOTP(false, checkuser.email?.ToLower() ?? checkuser.phonenumber, "POLICYSERVICE", Platforms.ADAPT);
                string messageBody = $"Adapt Policy Services authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";


            
                var rootPath = _hostingEnvironment.ContentRootPath;           
                var filePath = Path.Combine(rootPath, "Cert", "Adapt.html");              
                var template = System.IO.File.ReadAllText(filePath);
                
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#CONTENT#", messageBody);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
               
                var imagepath = Path.Combine(rootPath,  "Images", "adapt_logo.png");


                List<string> bcc = new List<string>();
                // bcc.Add("technology@custodianplc.com.ng");
                //use handfire
                if (!string.IsNullOrEmpty(checkuser.email))
                {
                    string test_email = "oscardybabaphd@gmail.com";
                    //email.email
                    var test = Config.isDemo ? "Test" : null;
                    var email = Config.isDemo ? test_email : checkuser.email;
                    new SendEmail().Send_Email(email,
                          $"Adapt-PolicyServices PIN Reset {test}",
                          sb.ToString(), $"Adapt-PolicyServices PIN Reset {test}",
                          true, imagepath, null, null, null);
                }

                if (!Config.isDemo)
                {
                    if (!string.IsNullOrEmpty(checkuser.phonenumber))
                    {
                        //phone = "2348069314541";
                        await new SendSMS().Send_SMS($"Adapt OTP: {generate_otp}", checkuser.phonenumber);
                    }
                }
                return new res
                {

                    message = $"OTP has been sent to email {checkuser.email} and phone {checkuser.phonenumber} attached to your policy",
                    status = (int)HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}/{policy_number?}/{hash?}")]
        [ValidateJWT]
        public async Task<res> GetLifeTransactions(string merchant_id, string policy_number, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetLifeTransactions", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(policy_number, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {policy_number}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var lookup = await _policyinfo.GetPolicyServices(policy_number);

                //if (lookup.Count() > 0)
                //{
                //    if (customer_id.Trim() != lookup.First().customerid.ToString().Trim())
                //    {
                //        return new res
                //        {
                //            status = 309,
                //            message = $"Customer ID does not match login information"
                //        };
                //    }
                //}

                var getTranscation = await util.GetTransactionFromTQ(policy_number);
                if (getTranscation == null)
                {
                    return new res
                    {
                        status = 200,
                        message = "No record found"
                    };
                }

                return new res
                {
                    status = 200,
                    message = "Transaction Fetched successfully",
                    data = getTranscation
                };


            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}/{policy_number?}/{hash?}")]
        [ValidateJWT]
        public async Task<res> GetVehicleList(string merchant_id, string policy_number, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetVehicleList", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(policy_number, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {policy_number}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetMotorPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, policy_number);
                    log.Info($"Raw response from GetMotorPolicyDetails {request}");
                    if (request == null || request.Length == 0)
                    {
                        return new res
                        {
                            status = (int)HttpStatusCode.Forbidden,
                            message = "Unable to fetch vehicle(s) details"
                        };
                    }

                    var response = request.Select(x => new
                    {
                        RegNumber = x.mVehReg?.Trim(),
                        ChasisNumber = x.mChasisNum?.Trim(),
                        EngineNumber = x.mENGINENUM?.Trim(),
                        ExpiryDate = Convert.ToDateTime(x.mEnddate?.Trim()).ToString("dd-MMM-yyyy"),
                        StartDate = Convert.ToDateTime(x.mStartdate?.Trim()).ToString("dd-MMM-yyyy"),
                        Color = x.mVEHCOLOR?.Trim(),
                        Make = x.mVEHMAKE?.Trim(),
                        Premium = Convert.ToDouble(x.mVEHPREMIUM?.Trim()),
                        Value = Convert.ToDouble(x.mVEHVALUE?.Trim()),
                        InsuredName = x.mInsuredname?.Trim(),
                        Status = x.Status?.Trim(),
                        EngineCapacity = x.mHPCAPACITY?.Trim()

                    }).ToList();

                    return new res
                    {
                        status = (int)HttpStatusCode.OK,
                        message = "Vehicle(s) details fetch successfully",
                        data = response
                    };

                }


            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }
    }
}
