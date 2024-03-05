using CustodianEmailSMSGateway.Email;
using CustodianEmailSMSGateway.SMS;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CustodianEveryWhereV2._0.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<Token> _otp = null;
        private store<RequestDocument> document = null;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public NotificationsController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _otp = new store<Token>();
            document = new store<RequestDocument>();
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("{email?}")]
        public async Task<notification_response> SendEmail(Email email)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        message = "Some required parameters are missing from request. please check Api docs for all required params",
                        status = 203,
                        type = DataStore.ViewModels.Type.EMAIL.ToString()
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SendEmail", email.Merchant_Id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.EMAIL.ToString()
                    };
                }

               
            
                string htmlFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "Cert", "Notification.html");
                string template = System.IO.File.ReadAllText(htmlFilePath);

                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#CONTENT#", email.htmlBody);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
               

                // Map path for image file
                string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "logo-white.png");
                await  Task.Factory.StartNew(() =>
                {
                    new SendEmail().Send_Email(email.ToAddress, email.Subject, sb.ToString(), email.Title, true, imagePath, email.CC, email.Bcc, null);
                });

                if (!string.IsNullOrEmpty(email.ExtraHtmlBody) && email.CCUnit != null && email.CCUnit.Count() > 0)
                {
                      // Map path for HTML file
                    string htmlFilePathtwo = Path.Combine(_hostingEnvironment.WebRootPath, "Cert", "Notification.html");

                    // Read the contents of the file
                    string template2 = System.IO.File.ReadAllText(htmlFilePathtwo);

                    StringBuilder sb2 = new StringBuilder(template);
                    sb2.Replace("#CONTENT#", email.ExtraHtmlBody);
                    sb2.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                    int i = 0;
                    List<string> newCC = new List<string>();
                    foreach (var item in email.CCUnit)
                    {
                        if (i == 0)
                        {
                            ++i;
                            continue;

                        }
                        else
                        {
                            newCC.Add(item);
                        }
                        ++i;
                    }

                   await Task.Factory.StartNew(() =>
                    {
                        new SendEmail().Send_Email(email.CCUnit[0], email.Subject, sb2.ToString(), email.Title, true, imagePath, newCC, null, null);
                    });
                }

                return new notification_response
                {
                    status = 200,
                    message = "Email was sent successfully",
                    type = DataStore.ViewModels.Type.EMAIL.ToString()
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response
                {
                    message = "Error occured while sending email",
                    status = 203,
                    type = DataStore.ViewModels.Type.EMAIL.ToString()
                };
            }
        }

        [HttpPost("{sms?}")]
        public async Task<notification_response> SendSMS(SMS sms)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        message = "Some required parameters are missing from request. please check Api docs for all required params",
                        status = 203,
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SendSMS", sms.Merchant_Id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                await new SendSMS().Send_SMS(sms.Message, sms.PhoneNumber);

                return new notification_response
                {
                    status = 200,
                    message = "Sms was sent successfully",
                    type = DataStore.ViewModels.Type.SMS.ToString()
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new notification_response
                {
                    message = "Error occured while sending email",
                    status = 203,
                    type = DataStore.ViewModels.Type.SMS.ToString()
                };
            }
        }

        [HttpPost("{send?}")]
        public async Task<notification_response> SendOTP(user_otp send)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "Some parameters missing",
                        type = DataStore.ViewModels.Type.OTP.ToString()
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SendOTP", send.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.OTP.ToString()
                    };
                }

                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == send.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {send.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(send.mobile + send.fullname, config.secret_key, send.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {send.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                log.Info($"about to send otp to number {send.mobile}");
                var old_otp = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && x.mobile_number == send.mobile);
                if (old_otp != null)
                {
                    log.Info($"deactivating previous un-used otp for mobile: {send.mobile}");
                    old_otp.is_used = true;
                    old_otp.is_valid = false;
                    await _otp.Update(old_otp);
                }
                log.Info($"creating new opt for user: {send.mobile}");
                var new_otp = new Token
                {
                    datecreated = DateTime.Now,
                    fullname = send.fullname,
                    is_used = false,
                    is_valid = true,
                    mobile_number = send.mobile,
                    platform = send.platform,
                    otp = (GlobalConstant.IsDemoMode) ? "123456" : Security.Transactions.UID.Codes.TransactionCodes.GenTransactionCodes(6)
                };
                await _otp.Save(new_otp);
                log.Info($"otp was saved successfully: {send.mobile}");
                var sms = new SendSMS();
                string body = $"Authentication code: {new_otp.otp}";
                string number = string.Empty;
                if (new_otp.mobile_number.StartsWith("0"))
                {
                    number = "234" + new_otp.mobile_number.Remove(0, 1);
                }
                else
                {
                    number = new_otp.mobile_number;
                }
                if (!GlobalConstant.IsDemoMode)
                {
                    var response = await sms.Send_SMS(body, number);
                    if (response == "SUCCESS")
                    {
                        log.Info($"otp was sent successfully to mobile: {send.mobile}");
                        return new notification_response
                        {
                            status = 200,
                            message = "OTP sent successfully",
                            type = DataStore.ViewModels.Type.OTP.ToString()
                        };

                    }
                    else
                    {
                        log.Info($"error sending otp to : {send.mobile}");
                        return new notification_response
                        {
                            status = 207,
                            message = "Oops! we couldn't send OTP to the provided number",
                            type = DataStore.ViewModels.Type.OTP.ToString()
                        };
                    }
                }
                else
                {
                    log.Info($"otp was sent successfully to mobile: {send.mobile}");
                    return new notification_response
                    {
                        status = 200,
                        message = "OTP sent successfully",
                        type = DataStore.ViewModels.Type.OTP.ToString()
                    };

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new notification_response
                {
                    message = "Error generating token",
                    status = 404,
                };
            }
        }

        [HttpGet("{token?}/{mobile?}/{merchant_id?}/{hash?}")]
        public async Task<notification_response> ValidateOTP(string token, string mobile, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ValidateOTP", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                //check api config
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
                var checkhash = await util.ValidateHash2(token + mobile, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                if (token.Length < 6 || token.Length > 6)
                {
                    log.Info($"you have provided an invalid otp {mobile}");
                    return new notification_response
                    {
                        status = 304,
                        message = "you have provided an invalid OTP"
                    };
                }

                var validate = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && x.mobile_number == mobile && x.otp == token);
                if (validate != null)
                {
                    log.Info($"you have provided an valid otp {mobile}");
                    return new notification_response
                    {
                        status = 200,
                        message = "you have provided a valid OTP"
                    };
                }
                else
                {
                    log.Info($"you have provided an invalid otp {mobile}");
                    return new notification_response
                    {
                        status = 201,
                        message = "you have provided an invalid OTP"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new notification_response
                {
                    message = "Error validating token",
                    status = 203,
                };
            }
        }

        [HttpPost("{documents?}")]
        public async Task<notification_response> RequestForDocuments(fDocuments documents)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        message = "Parameters missing from payload",
                        status = 201,
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("RequestForDocuments", documents.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == documents.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {documents.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(documents.docType + documents.email, config.secret_key, documents.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {documents.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var get_division = util.GetGeneralDivision(documents.policyNumber?.Trim().ToLower());
                log.Info($"return val {Newtonsoft.Json.JsonConvert.SerializeObject(get_division)}");
                if (get_division == null)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "Unable to fetch policy division. Please contact custodian care centre"
                    };
                }
                DivisionEmail email_division = null;
               

              
                string jsonFilePath = Path.Combine("Cert", "json.json"); // Adjust the path accordingly        
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                List<DivisionEmail> divisionn_obj = JsonSerializer.Deserialize<List<DivisionEmail>>(jsonContent);



                if (documents.subsidiary == subsidiary.General)
                {
                    if (get_division.name == "BRANCH")
                    {
                        email_division = divisionn_obj.FirstOrDefault(x => x.Key.ToUpper() == get_division.code);
                    }
                    else
                    {
                        email_division = divisionn_obj.FirstOrDefault(x => x.Code.ToUpper() == get_division.name);
                    }
                }
                else
                {
                    return new notification_response
                    {
                        message = "Sorry, this service temporarily unavaible. Please visit https://life.custodianplc.com.ng to download your policy document",
                        status = 204,
                    };
                    // email_division = divisionn_obj.FirstOrDefault(x => x.Code.ToUpper() == "LIFE");
                }
              

                // Map path for HTML file
                string htmlFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Cert", "Notification.html");

                // Read the contents of the file
                string template = System.IO.File.ReadAllText(htmlFilePath);

                StringBuilder sb = new StringBuilder(template);
                StringBuilder table = new StringBuilder();
                table.Append("<table>");
                table.Append($"<tr><td>Request Type</td><td>{documents.docType}</td></tr>");
                table.Append($"<tr><td>Policy Number</td><td>{documents.policyNumber.ToUpper()}</td></tr>");
                if (documents.from.HasValue)
                {
                    if (!documents.to.HasValue)
                    {
                        documents.to = documents.from;
                    }
                    table.Append($"<tr><td>Start Date</td><td>{documents.from.Value.ToShortDateString()}</td></tr>");
                }

                if (documents.to.HasValue)
                {
                    if (!documents.from.HasValue)
                    {
                        documents.from = documents.to;
                    }
                    table.Append($"<tr><td>End Date</td><td>{documents.to.Value.ToShortDateString()}</td></tr>");
                }
                table.Append($"<tr><td>Email Address</td><td>{documents.email}</td></tr>");
                table.Append("</table>");
                sb.Replace("#CONTENT#", table.ToString());
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                // Map path for image file
                string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Images", "logo-white.png");
                var cc = _configuration["AppSettings:Notification"]?.Split('|')?.ToList();
                new SendEmail().Send_Email(email_division.Email, $"Request for {documents.docType}", sb.ToString(), $"Request for {documents.docType}", true, imagePath, cc, null, null);
                var save = new RequestDocument
                {
                    DateRequested = DateTime.Now,
                    DocType = documents.docType,
                    Email = documents.email,
                    Division = email_division.Code,
                    DivisionEmail = email_division.Email,
                    From = documents.from,
                    PolicyNumber = documents.policyNumber,
                    To = documents.to
                };
                await document.Save(save);
                return new notification_response
                {
                    status = 200,
                    message = "Request was sent successfully"
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new notification_response
                {
                    message = "System malfunction",
                    status = 404,
                };
            }
        }
    }
}
