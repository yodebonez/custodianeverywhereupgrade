using DataStore.Irepository;
using DataStore.Models;
using DataStore.repository;
using DataStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Spire.Doc;
using CustodianEmailSMSGateway.Email;
using System.Configuration;
using System.Data;
using System.Web.ModelBinding;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Hangfire.Server;
using Hangfire.Console;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Net;
using System.Security.Authentication;
using Oracle.ManagedDataAccess.Client;
using System.DirectoryServices;

namespace DataStore.Utilities
{
    public class Utility
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<PremiumCalculatorMapping> _premium_map = null;
        private store<Token> _otp = null;
        private store<Chaka> chaka = null;
        public Utility()
        {
            _apiconfig = new store<ApiConfiguration>();
            _premium_map = new store<PremiumCalculatorMapping>();
            _otp = new store<Token>();
            chaka = new store<Chaka>();
        }
        public async Task<bool> ValidateHash(decimal value_of_goods, string secret, string _hash)
        {
            bool res = false;
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(Convert.ToInt32(value_of_goods) + secret));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            if (Sb.ToString().ToUpper().Equals(_hash.ToUpper()))
            {
                res = true;
            }
            return res;
        }
        public string Sha256(string pattern)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(pattern));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        public async Task<string> Sha512(string pattern)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA512 hash = SHA512.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(pattern));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        public async Task<bool> ValidateGTBankUsers(string userhash, string computedhash)
        {
            if (userhash.ToUpper().Equals(computedhash.ToUpper()))
            {
                log.Info("valid hash for GTB users");
                return true;
            }
            else
            {
                log.Info("Invalid hash for GTB users");
                return false;
            }
        }
        public async Task<bool> ValidateHash2(string pattern, string secret, string _hash)
        {
            log.Info($"Passed hash {_hash.ToUpper()}");
            log.Info($"my hash partten {pattern + secret}");
            bool res = false;
            StringBuilder Sb = new StringBuilder();
            using (MD5 hash = MD5.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(pattern + secret));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            log.Info($"Computed hash {Sb.ToString().ToUpper()}");
            if (Sb.ToString().ToUpper().Equals(_hash.ToUpper()))
            {
                res = true;
            }
            return res;
        }
        public async Task<string> MD_5(string pattern)
        {
            StringBuilder Sb = new StringBuilder();
            using (MD5 hash = MD5.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(pattern));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        public async Task<bool> CheckForAssignedFunction(string methodName, string merchant_id)
        {
            var apiconfig = new store<ApiConfiguration>();
            var getconfig = await apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim() && x.is_active == true);
            if (getconfig == null || string.IsNullOrEmpty(getconfig.assigned_function))
            {
                return false;
            }
            if (!getconfig.assigned_function.Split('|').Any(x => x.Trim().ToLower().Equals(methodName.ToLower())))
            {
                return false;
            }
            else
            {
                var method_status = new store<ApiMethods>().FindOneByCriteria(x => x.method_name.Trim().ToLower().Equals(methodName.Trim().ToLower()) && x.is_active == true);
                if (method_status != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public string base64Decode(string data)
        {
            try
            {
                UTF8Encoding encoder = new UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                return null;
                //throw new Exception("Error in base64Decode" + e.Message);
            }
        }
        public async Task<bool> ValidateHeaders(string sentHeader, string merchant_id)
        {
            var apiconfig = new store<ApiConfiguration>();
            var getconfig = await apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim() && x.is_active == true);
            if (getconfig == null || string.IsNullOrEmpty(getconfig.assigned_function))
            {
                return false;
            }
            var formhash = Sha256(getconfig.secret_key + getconfig.merchant_id);
            var formbase64headers = base64Decode(sentHeader);
            if (formhash.ToUpper().Equals(formbase64headers.ToUpper()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        public async Task<req_response> GenerateCertificate(GenerateCert cert)
        {
            req_response response = new req_response();
            try
            {

                Document doc = new Document();
                string newrtfPath = HttpContext.Current.Server.MapPath("~/Cert/template.rtf");
                doc.LoadFromFile(newrtfPath, FileFormat.Rtf);
                doc.Replace("#POLICY_NO#", cert.policy_no, false, true);
                doc.Replace("#NAME#", cert.name, false, true);
                doc.Replace("#ADDRESS#", cert.address, false, true);
                doc.Replace("#FROM_DATE#", cert.from_date, false, true);
                //doc.Replace("#TO_DATE#", cert.to_date, false, true);
                doc.Replace("#VREG_NO#", cert.vehicle_reg_no, false, true);
                doc.Replace("#INTEREST#", cert.interest.Trim(), false, true);
                doc.Replace("#VALUE_OF_GOODS#", string.Format("N{0:N}", Convert.ToDecimal(cert.value_of_goods)), false, true);
                doc.Replace("#FROM_LOC#", cert.from_location, false, true);
                doc.Replace("#PREMIUM#", string.Format("N{0:N}", Convert.ToDecimal(cert.premium)), false, true);
                doc.Replace("#TO_LOC#", cert.to_location, false, true);//#SERIAL_NUMBER#
                doc.Replace("#SERIAL_NUMBER#", cert.serial_number, false, true);
                string pdfPath = HttpContext.Current.Server.MapPath($"~/Cert/GeneratedCert/{cert.serial_number}.pdf");
                if (System.IO.File.Exists(pdfPath))
                {
                    System.IO.File.Delete(pdfPath);
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }
                else
                {
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }

                cert.cert_path = pdfPath;
                //dont wait for certificate to send continue process and let ths OS thread do the sending without slowing down the application
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                string imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                Task.Factory.StartNew(() =>
                {
                    this.SendEmail(cert, template, imagepath);
                });

                response.status = 200;
                response.message = $"Insurance purchase was successful, a copy of your insurance document has been sent to this ({cert.email_address}) email";
                response.policy_details = new policy_details
                {
                    certificate_no = cert.serial_number,
                    policy_number = cert.policy_no,
                    download_link = $"https://api.custodianplc.com.ng/CustodianApiv2.0/download/{cert.serial_number}"
                };

            }
            catch (Exception ex)
            {
                response.status = 401;
                response.message = $"System malfunction";
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }

            return response;
        }
        public async Task<req_response> GenerateCertificateOneOff(GenerateCert cert)
        {
            req_response response = new req_response();
            try
            {

                Document doc = new Document();
                string newrtfPath = HttpContext.Current.Server.MapPath("~/Cert/templateOneoff.rtf");
                doc.LoadFromFile(newrtfPath, FileFormat.Rtf);
                doc.Replace("#POLICY_NO#", cert.policy_no, false, true);
                doc.Replace("#NAME#", cert.name, false, true);
                doc.Replace("#ADDRESS#", cert.address, false, true);
                doc.Replace("#FROM_DATE#", cert.from_date, false, true);
                doc.Replace("#TO_DATE#", cert.to_date, false, true);
                doc.Replace("#VREG_NO#", cert.vehicle_reg_no, false, true);
                doc.Replace("#SERIAL_NUMBER#", cert.serial_number, false, true);
                string pdfPath = HttpContext.Current.Server.MapPath($"~/Cert/GeneratedCert/{cert.serial_number}_NB.pdf");
                if (System.IO.File.Exists(pdfPath))
                {
                    System.IO.File.Delete(pdfPath);
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }
                else
                {
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }

                cert.cert_path = pdfPath;
                //dont wait for certificate to send continue process and let ths OS thread do the sending without slowing down the application
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                string imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                Task.Factory.StartNew(() =>
                {
                    this.SendEmail(cert, template, imagepath);
                });

                response.status = 200;
                response.message = $"Insurance purchase was successful, a copy of your insurance document has been sent to this ({cert.email_address}) email";
                response.policy_details = new policy_details
                {
                    certificate_no = cert.serial_number,
                    policy_number = cert.policy_no,
                };

            }
            catch (Exception ex)
            {
                response.status = 401;
                response.message = $"System malfunction";
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }

            return response;
        }
        public async Task<string> GetSerialNumber()
        {
            Guid serialGuid = Guid.NewGuid();
            string uniqueSerial = serialGuid.ToString("N");
            string uniqueSerialLength = uniqueSerial.Substring(0, 28).ToUpper();
            char[] serialArray = uniqueSerialLength.ToCharArray();
            string finalSerialNumber = "";
            int j = 0;
            for (int i = 0; i < 28; i++)
            {
                for (j = i; j < 4 + i; j++)
                {
                    finalSerialNumber += serialArray[j];
                }
                if (j == 28)
                {
                    break;
                }
                else
                {
                    i = (j) - 1;
                    finalSerialNumber += "-";
                }
            }

            return finalSerialNumber;
        }
        public void SendEmail(GenerateCert sendmail, string temp, string Imagepath, string new_biz = "")
        {
            try
            {
                StringBuilder template = new StringBuilder(temp);
                string body = $"Dear {sendmail.name},<br/><br/>Please find attached your GIT Insurance documents.<br/><br/> Thank you.<br/><br/>";
                template.Replace("#CONTENT#", body);
                template.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                List<string> attach = new List<string>();
                attach.Add(sendmail.cert_path);

                var send = new SendEmail().Send_Email(sendmail.email_address, $"{new_biz} GIT Insurance Document", template.ToString(), $"{new_biz}  GIT Insurance Document", true, Imagepath, null, null, attach);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public void SendQuotationEmail(Auto sendmail, string temp, string Imagepath, string cert_path, string temp2, string quotation_number)
        {
            try
            {
                StringBuilder template = new StringBuilder(temp);
                string body = $"Dear {sendmail.customer_name},<br/><br/>Please find attached your quotation.<br/><br/> Thank you.<br/><br/>";
                template.Replace("#CONTENT#", body);
                template.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                List<string> attach = new List<string>();
                attach.Add(cert_path);
                var send = new SendEmail().Send_Email(sendmail.email, $"{sendmail.insurance_type.ToString()}-QUOTATION", template.ToString(), $"Custodian", true, Imagepath, null, null, attach);
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Notification"]))
                {
                    var emailList = ConfigurationManager.AppSettings["Notification"].Split('|');
                    var newlist = emailList;
                    if (emailList.Length > 1)
                    {
                        StringBuilder template2 = new StringBuilder(temp2);
                        string body2 = $"Dear Team,<br/><br/>A customer just requested for a quote, find attached for follow-up<br/><br/> Thank you.<br/><br/>";
                        template2.Replace("#CONTENT#", body2);
                        template2.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                        List<string> cc = new List<string>();
                        int i = 1;
                        foreach (var item in newlist)
                        {
                            if (i != 1)
                            {
                                cc.Add(item);
                            }
                            ++i;
                        }
                        var send2 = new SendEmail().Send_Email(emailList[0], $"{sendmail.insurance_type.ToString()}-QUOTATION--No.{quotation_number}", template2.ToString(), $"{sendmail.insurance_type.ToString()}-QUOTATION--No.{quotation_number}", true, Imagepath, cc, null, attach);
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public async Task<string> GeneratePolicyNO(int val)
        {
            string final = "";
            if (val <= 9)
            {
                final = "000000" + val;
            }
            else if (val.ToString().Length < 7)
            {
                int loop = 7 - val.ToString().Length;
                string zeros = "";
                for (int i = 0; i < loop; i++)
                {
                    zeros += "0";
                }
                final = zeros + val;
            }
            else
            {
                final = val.ToString();
            }

            return "HO/A/07/T" + final + "E";
        }
        public async Task<req_response> Validator(string method_name, string merchant_id, string _category, decimal value_of_goods, string hash)
        {
            try
            {
                var check_user_function = await CheckForAssignedFunction(method_name, merchant_id);
                if (!check_user_function)
                {
                    return new req_response
                    {
                        status = 406,
                        message = "Access denied. user not configured for this service "
                    };
                }
                Category category;
                if (!Enum.TryParse(_category, out category))
                {
                    return new req_response
                    {
                        status = 401,
                        message = "Invalid category type, kindly check the api  documentation for the category types"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    return new req_response
                    {
                        status = 405,
                        message = "Invalid merchant Id"
                    };
                }
                var can_proceed = await ValidateHash(value_of_goods, config.secret_key, hash);
                if (!can_proceed)
                {
                    return new req_response
                    {
                        status = 403,
                        message = "Security violation: hash value mismatch"
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
            return null;
        }
        public void SendMail(Life_Claims mail, bool IsCustodian, string template, string imagepath, string division_email = null)
        {
            try
            {
                string test = Config.isDemo ? "Test" : "";
                log.Info($"About to send email to {mail.email_address}");
                StringBuilder sb = new StringBuilder(template);
                log.Info($"About to send temp to here");
                sb.Replace("#NAME#", mail.policy_holder_name);
                sb.Replace("#POLICYNUMBER#", mail.policy_number);
                sb.Replace("#POLICYTYPE#", mail.policy_type);
                sb.Replace("#CLAIMSTYPE#", mail.claim_request_type);
                sb.Replace("#EMAILADDRESS#", mail.email_address);
                sb.Replace("#PHONENUMBER#", mail.phone_number);
                sb.Replace("#CLAIMNUMBER#", mail.claim_number);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                log.Info($"About to send param to all");
                var image_path = imagepath;
                if (IsCustodian)
                {
                    string msg_1 = @"Dear Team,<br/> A claim has been logged succesfully and require your attention for further processing";
                    sb.Replace("#FOOTER#", "");
                    sb.Replace("#CONTENT#", msg_1);
                    var email = ConfigurationManager.AppSettings["Notification"];
                    var list = email.Split('|');
                    string emailaddress = "";
                    List<string> cc = new List<string>();
                    if (list.Count() > 1)
                    {
                        int i = 0;
                        emailaddress = list[0];
                        foreach (var item in list)
                        {
                            if (i == 0)
                            {
                                ++i;
                                continue;
                            }
                            else
                            {
                                cc.Add(item);
                                ++i;
                            }

                        }
                    }
                    else
                    {
                        emailaddress = list[0];
                    }

                    if (!string.IsNullOrEmpty(division_email))
                    {
                        emailaddress = division_email;
                        cc.Add(list[0]);
                    }
                    var send = new SendEmail().Send_Email(emailaddress, $"Claim Request {test}", sb.ToString(), $"Claim Request {test}", true, image_path, cc, null, null);
                }
                else
                {

                    string msg_1 = $@"Your claim has been submitted successfully. Your claim number is <strong>{mail.claim_number}</strong>. 
                                You can check your claim status on our website or call(+234)12774000 - 9";
                    sb.Replace("#CONTENT#", msg_1);
                    sb.Replace("#FOOTER#", "");
                    var send = new SendEmail().Send_Email(mail.email_address, $"Claim Request {test}", sb.ToString(), $"Claim Request {test}", true, image_path, null, null, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public void SendMail(ViewModelNonLife mail, bool IsCustodian, string template, string imagepath, string divisionemail = "")
        {
            try
            {
                string test = Config.isDemo ? "Test" : "";
                log.Info($"About to send email to {mail.email_address}");
                StringBuilder sb = new StringBuilder(template);
                log.Info($"About to send temp to here");
                sb.Replace("#NAME#", mail.policy_holder_name);
                sb.Replace("#POLICYNUMBER#", mail.policy_number);
                sb.Replace("#POLICYTYPE#", mail.policy_type);
                sb.Replace("#CLAIMSTYPE#", mail.claim_request_type);
                sb.Replace("#EMAILADDRESS#", mail.email_address);
                sb.Replace("#PHONENUMBER#", mail.phone_number);
                sb.Replace("#CLAIMNUMBER#", mail.claims_number);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                log.Info($"About to send param to all");
                var image_path = imagepath;
                if (IsCustodian)
                {
                    sb.Replace("#FOOTER#", "");
                    string msg_1 = @"Dear Team,<br/><br/>A claim has been logged successfully and require your attention for further processing";
                    sb.Replace("#CONTENT#", msg_1);
                    var email = ConfigurationManager.AppSettings["Notification"];
                    var list = email.Split('|');
                    string emailaddress = "";
                    List<string> cc = new List<string>();
                    if (list.Count() > 1)
                    {
                        int i = 0;
                        if (!string.IsNullOrEmpty(divisionemail))
                        {
                            emailaddress = divisionemail;
                        }
                        else
                        {
                            emailaddress = list[0];
                        }

                        foreach (var item in list)
                        {
                            if (!string.IsNullOrEmpty(divisionemail))
                            {
                                cc.Add(item);
                                ++i;
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    ++i;
                                    continue;
                                }
                                else
                                {
                                    cc.Add(item);
                                    ++i;
                                }
                            }
                        }
                    }
                    else
                    {
                        //emailaddress = list[0];
                        if (!string.IsNullOrEmpty(divisionemail))
                        {
                            emailaddress = divisionemail;
                            cc.Add(list[0]);
                        }
                        else
                        {
                            emailaddress = list[0];
                        }
                    }
                    var send = new SendEmail().Send_Email(emailaddress, $"Claim Request {test}", sb.ToString(), $"Claim Request {test}", true, image_path, cc, null, null);
                }
                else
                {
                    sb.Replace("#FOOTER#", @"Please visit our website to confirm the status of your claim.<br /><br />
                                    If you did not initiate this process, please contact us on (+234)12774008-9 or carecentre@custodianinsurance.com");
                    string msg_1 = @"Dear Valued Customer,<br/><br/>Your claim with the below details has been submitted successfully.";
                    sb.Replace("#CONTENT#", msg_1);
                    var send = new SendEmail().Send_Email(mail.email_address, $"Claim Request {test}", sb.ToString(), $"Claim Request {test}", true, image_path, null, null, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public async Task<string> SendGITMail(GITInsurance git, string status, string merchant_id)
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath("~/Cert/Gitnotification.html");
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                var template = System.IO.File.ReadAllText(path);
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#NAME#", git.insured_name);
                sb.Replace("#POLICYNUMBER#", git.policy_no);
                sb.Replace("#VALUE_OF_GOODS#", string.Format("N {0:N}", git.value_of_goods));
                sb.Replace("#DESCRIPTION#", git.goods_description);
                sb.Replace("#EMAILADDRESS#", git.email_address);
                sb.Replace("#PHONENUMBER#", git.phone_number);
                sb.Replace("#PREMIUM#", string.Format("N {0:N}", git.premium));
                sb.Replace("#STATUS#", status);
                sb.Replace("#START#", git.from_date.ToShortDateString());

                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                var email = ConfigurationManager.AppSettings["Notification"];
                var list = email.Split('|');
                string emailaddress = "";
                List<string> cc = new List<string>();
                if (list.Count() > 1)
                {
                    int i = 0;
                    emailaddress = list[0];
                    foreach (var item in list)
                    {
                        if (i == 0)
                        {
                            ++i;
                            continue;
                        }
                        else
                        {
                            cc.Add(item);
                            ++i;
                        }

                    }
                }
                else
                {
                    emailaddress = list[0];
                }
                string title = "Custodian GIT Start Trip Notification";
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (status == "NO")
                {
                    sb.Replace("#HEADER#", "Git Start Trip Notification");
                    sb.Replace("#CONTENT#", $"Dear Team,<br/><br/>A customer has bought GIT Insurance from {config.merchant_name}");
                    title = "Custodian GIT Start Trip Notification";
                    sb.Replace("#END#", "---");
                }
                else
                {
                    title = "Custodian GIT End Trip Notification";
                    sb.Replace("#HEADER#", "Git End Trip Notification");
                    sb.Replace("#CONTENT#", $"Dear Team,<br/><br/>A customer has delievered his goods successfully from {config.merchant_name}");
                    sb.Replace("#END#", git.to_date.Value.ToShortDateString());
                }

                Task.Factory.StartNew(() =>
                {
                    var send = new SendEmail().Send_Email(emailaddress, title, sb.ToString(), title, true, imagepath, cc, null, null);
                });
                return "True";
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<string> SendGITMail(GITInsurance git, string merchant_id)
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath("~/Cert/Gitnotification.html");
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                var template = System.IO.File.ReadAllText(path);
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#NAME#", git.insured_name);
                sb.Replace("#POLICYNUMBER#", git.policy_no);
                sb.Replace("#VALUE_OF_GOODS#", "---");
                sb.Replace("#DESCRIPTION#", git.category);
                sb.Replace("#EMAILADDRESS#", git.email_address);
                sb.Replace("#PHONENUMBER#", git.phone_number);
                sb.Replace("#PREMIUM#", string.Format("N {0:N}", git.premium));
                sb.Replace("#STATUS#", "---");
                sb.Replace("#START#", git.from_date.ToShortDateString());
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                sb.Replace("#END#", git.to_date.Value.ToShortDateString());
                var email = ConfigurationManager.AppSettings["Notification"];
                var list = email.Split('|');
                string emailaddress = "";
                List<string> cc = new List<string>();
                if (list.Count() > 1)
                {
                    int i = 0;
                    emailaddress = list[0];
                    foreach (var item in list)
                    {
                        if (i == 0)
                        {
                            ++i;
                            continue;
                        }
                        else
                        {
                            cc.Add(item);
                            ++i;
                        }

                    }
                }
                else
                {
                    emailaddress = list[0];
                }
                string title = "New Business Custodian GIT Insurance";
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());

                sb.Replace("#HEADER#", " ");
                sb.Replace("#CONTENT#", $"Dear Team,<br/><br/>A customer just bought GIT Insurance from {config.merchant_name}");
                Task.Factory.StartNew(() =>
                {
                    var send = new SendEmail().Send_Email(emailaddress, title, sb.ToString(), title, true, imagepath, cc, null, null);
                });
                return "True";
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<LifeClaimStatus> SubmitLifeClaims(string policy_no, string claim_type)
        {
            try
            {
                string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cn.Open();
                    cmd.CommandText = "cust_max_mgt.create_claim";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = policy_no;
                    cmd.Parameters.Add("p_type_code", OracleDbType.Varchar2).Value = claim_type;
                    cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    string response = cmd.Parameters["v_data"].Value.ToString();
                    log.Info($"response from turnquest {response}");
                    if (string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var transpose = Newtonsoft.Json.JsonConvert.DeserializeObject<LifeClaimStatus>(response);
                    return transpose;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                throw ex;
            }
        }
        public async Task<string> ClaimCode(string claim_type)
        {
            if (claim_type.ToLower().Trim() == "surrender")
            {
                return "SUR";
            }
            else if (claim_type.ToLower().Trim() == "death")
            {
                return "DTH";
            }
            else if (claim_type.ToLower().Trim() == "termination")
            {
                //return "TEM";
                return "SUR";
            }
            else if (claim_type.ToLower().Trim().StartsWith("parmanet"))
            {
                return "DIS";
            }
            else
            {
                return "PROCEED";
            }
        }
        public async Task<LifeClaimStatus> CheckClaimStatus(string claim_number)
        {
            try
            {
                string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cn.Open();
                    cmd.CommandText = "cust_max_mgt.check_claim_status";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_claim_no", OracleDbType.Varchar2).Value = claim_number;
                    cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    string response = cmd.Parameters["v_data"].Value.ToString();
                    log.Info($"response from turnquest claims status {response}");
                    if (string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var transpose = Newtonsoft.Json.JsonConvert.DeserializeObject<LifeClaimStatus>(response);
                    return transpose;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<dynamic> GetTransactionFromTQ(string policynumber)
        {
            try
            {
                string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
                string query = $@"SELECT DECODE(OPR_DRCR,'D',-1,1)*OPR_AMT OPR_AMT,TO_CHAR(OPR_RECEIPT_DATE,'DD/MM/RRRR')OPR_DATE,OPR_RECEIPT_NO,OPR_DRCR
                                FROM LMS_ORD_PREM_RECEIPTS WHERE OPR_POL_POLICY_NO = '{policynumber}' ORDER BY OPR_RECEIPT_DATE ASC";
                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    OracleCommand cmd = new OracleCommand();
                    await cn.OpenAsync();
                    cmd.Connection = cn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    var rows = await cmd.ExecuteReaderAsync();
                    List<dynamic> tranx = new List<dynamic>();
                    while (await rows.ReadAsync())
                    {
                        var single = new
                        {
                            Amount = Convert.ToDecimal(rows["OPR_AMT"]?.ToString()),
                            TransactionDate = rows["OPR_DATE"]?.ToString(),
                            RecieptNumber = rows["OPR_RECEIPT_NO"]?.ToString(),
                            Status = (rows["OPR_DRCR"]?.ToString().ToUpper() == "D") ? "DR" : "CR",
                        };

                        tranx.Add(single);
                    }

                    return tranx;
                }
            }
            catch (Exception ex)
            {
                log.Info("Exception was throwed");
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                throw ex;
            }
        }
        public async Task<claims_details> GetLifeClaimsDetails(ClaimsDetails claim_detail)
        {
            try
            {
                log.Info($"raw response from portal {Newtonsoft.Json.JsonConvert.SerializeObject(claim_detail)}");
                string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString;
                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cn.Open();
                    cmd.CommandText = "cust_max_mgt.get_claim_policy_info";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = claim_detail.p_policy_no;
                    cmd.Parameters.Add("p_type", OracleDbType.Varchar2).Value = claim_detail.p_type;
                    cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_claim_type", OracleDbType.Varchar2).Value = claim_detail.p_claim_type;
                    cmd.ExecuteNonQuery();
                    string response = cmd.Parameters["v_data"].Value.ToString();
                    log.Info($"response from turnquest claims status {response}");
                    if (string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var transpose = Newtonsoft.Json.JsonConvert.DeserializeObject<claims_details>(response);
                    return transpose;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<string> Transposer(string frequency)
        {
            string frq = "";
            if (frequency.ToLower() == "annually")
            {
                frq = "A";
            }
            else if (frequency.ToLower() == "quarterly")
            {
                frq = "Q";
            }
            else if (frequency.ToLower() == "bi-annually")
            {
                frq = "S";
            }
            else if (frequency.ToLower() == "monthly")
            {
                frq = "M";
            }
            else if (frequency.ToLower() == "semi-annually")
            {
                frq = "S";
            }
            else
            {
                frq = "F";
            }

            return frq;
        }
        public async Task<string> GenerateOTP(bool isSms, string toaddress, string fullname, Platforms source)
        {
            try
            {
                var old_otp = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && (x.mobile_number == toaddress || x.email == toaddress));
                if (old_otp != null)
                {
                    log.Info($"deactivating previous un-used otp for mobile: {toaddress}");
                    old_otp.is_used = true;
                    old_otp.is_valid = false;
                    await _otp.Update(old_otp);
                }
                log.Info($"creating new opt for user: {toaddress}");
                var new_otp = new Token
                {
                    datecreated = DateTime.Now,
                    fullname = fullname,
                    is_used = false,
                    is_valid = true,
                    mobile_number = (isSms) ? toaddress : null,
                    platform = source,
                    email = (!isSms) ? toaddress : null,
                    otp = (Config.isDemo) ? "123456" : Security.Transactions.UID.Codes.TransactionCodes.GenTransactionCodes(6)
                };
                await _otp.Save(new_otp);

                return new_otp.otp;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return null;
            }
        }
        public async Task<bool> ValidateOTP(string token, string emailorphone)
        {
            try
            {
                var validate = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && (x.mobile_number == emailorphone?.ToLower().Trim() || x.email?.ToLower().Trim() == emailorphone?.ToLower().Trim()) && x.otp == token);
                if (validate != null)
                {
                    log.Info($"you have provided an valid otp {emailorphone}");
                    validate.is_used = true;
                    validate.is_valid = false;
                    await _otp.Update(validate);
                    return true;
                }
                else
                {
                    log.Info($"you have provided an invalid otp {emailorphone}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return false;
            }
        }
        public async Task<List<double>> GetDiscountByAge(List<int> _Age, double BasePremium)
        {
            List<double> rates = new List<double>();
            foreach (var Age in _Age)
            {
                if (Age < 18)
                {
                    rates.Add(BasePremium * 0.7);
                }
                else if (Age >= 66 && Age <= 70)
                {
                    rates.Add(BasePremium * 1.5);
                }
                else if (Age >= 71 && Age <= 76)
                {
                    rates.Add(BasePremium * 2);
                }
                else
                {
                    rates.Add(BasePremium);
                }
            }

            return rates;
        }
        public async Task<List<RateCategory>> GetTravelRate(int numbersOfDays, TravelCategory region)
        {
            #region
            string category = "";
            if (numbersOfDays >= 1 && numbersOfDays <= 7)
            {
                category = "A";
            }
            else if (numbersOfDays >= 8 && numbersOfDays <= 15)
            {
                category = "B";
            }
            else if (numbersOfDays >= 16 && numbersOfDays <= 32)
            {
                category = "C";
            }
            else if (numbersOfDays >= 33 && numbersOfDays <= 62)
            {
                category = "D";
            }
            else if (numbersOfDays >= 63 && numbersOfDays <= 93)
            {
                category = "E";
            }
            else if (numbersOfDays >= 94 && numbersOfDays <= 180)
            {
                category = "F";
            }
            else
            {
                category = "G";
            }
            #endregion
            var getRateFile = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/RateTable.json"));
            var getRate = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TravelRate>>(getRateFile);
            var getCategory = getRate.FirstOrDefault(x => x._class == category);
            if (region == TravelCategory.WorldWide2 || region == TravelCategory.WorldWide)
            {
                List<string> worldwide = new List<string>() { "GOLD", "SILVER", "ECONOMY" };
                return getCategory.category.Where(x => worldwide.Any(y => y == x.type)).ToList();
            }
            else
            {
                return getCategory.category.Where(x => x.type == region.ToString().ToUpper()).ToList();
            }

        }
        public List<Package> GetPackageDetails(TravelCategory region, out List<string> benefits)
        {
            var getFile = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/Category.json"));
            var package = Newtonsoft.Json.JsonConvert.DeserializeObject<PackageList>(getFile);
            int _region = (int)region;
            if (_region == 5)
            {
                _region = 1;
            }
            var getRegion = package.category.Where(x => x.region == _region).ToList();
            benefits = package.benefits;
            List<Package> packages = new List<Package>();
            foreach (var item in getRegion)
            {
                foreach (var _item in item.package)
                {
                    packages.Add(_item);
                }
            }
            log.Info($" Data from Text file: {Newtonsoft.Json.JsonConvert.SerializeObject(packages)}");
            return packages;
        }
        public async Task<int> RoundValueToNearst100(double value)
        {
            int result = (int)Math.Ceiling(value / 100);// + 1;
            if (value > 0 && result == 0)
            {
                result = 1;
            }
            return (int)result * 100;
        }
        public string ToXML(Object oObject)
        {
            var getProps = oObject.GetType().GetProperties();
            string xml = "";
            foreach (var prop in getProps)
            {
                xml += $"<{prop.Name}>{prop.GetValue(oObject)}</{prop.Name}>";
            }
            return xml;
            #region
            //XmlDocument xmlDoc = new XmlDocument();
            //XmlSerializer xmlSerializer = new XmlSerializer(oObject.GetType());
            //using (MemoryStream xmlStream = new MemoryStream())
            //{
            //    xmlSerializer.Serialize(xmlStream, oObject);
            //    xmlStream.Position = 0;
            //    xmlDoc.Load(xmlStream);
            //    return xmlDoc.InnerXml.;
            //}
            #endregion
        }
        public async Task<Annuity> ValidateBirthDayForLifeProduct(DateTime DateOfBirth, int MinAge, int MaxAge)
        {
            try
            {

                int today = DateTime.Now.Year;
                int passdate = DateOfBirth.Year;
                int currentAge = today - passdate;
                if (currentAge >= MinAge && currentAge <= MaxAge)
                {
                    var month = DateTime.Now.Month;
                    var dobmonth = DateOfBirth.Month;
                    var day = DateTime.Now.Day;
                    var dobday = DateOfBirth.Day;
                    if (dobmonth == month)
                    {
                        if (dobday > day)
                        {
                            DateOfBirth = DateOfBirth.AddYears(-1);
                        }
                    }
                    else if (month < dobmonth)
                    {
                        int ageNext = Math.Abs(month - DateOfBirth.Month);
                        if (ageNext <= 6)
                        {
                            DateOfBirth = DateOfBirth.AddYears(-1);
                        }
                    }
                    log.Info($"Date of Birth ==>{DateOfBirth}");
                    return new Annuity
                    {
                        status = 200,
                        dateOfBirth = DateOfBirth,
                        message = "operation was successful"
                    };
                }
                else
                {
                    return new Annuity
                    {
                        status = 202,
                        message = "Your are not eligeable"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Info(ex.StackTrace);
                log.Info(ex.Message);
                return new Annuity
                {
                    status = 209,
                    message = "We could not validate your age"
                };
            }
        }
        public async Task<req_response> SendQuote(Auto cert, int quote_number)
        {
            req_response response = new req_response();
            try
            {
                Document doc = new Document();
                string newrtfPath = HttpContext.Current.Server.MapPath("~/Cert/QuoteTemplate.rtf");
                doc.LoadFromFile(newrtfPath, FileFormat.Rtf);
                string excess = (cert.excess?.ToLower() == "y") ? "Excess" : "";
                string tracking = (cert.tracking?.ToLower() == "y") ? "Tracking" : "";
                string flood = (cert.flood?.ToLower() == "y") ? "Flood" : "";
                string srcc = (cert.srcc?.ToLower() == "y") ? "SRCC" : "";
                string quote_no = await GenerateQuoteNumber(quote_number);
                doc.Replace("#customer_name#", cert.customer_name, false, true);
                doc.Replace("#address#", cert.address, false, true);
                doc.Replace("#phone_number#", cert.phone_number, false, true);
                doc.Replace("#email#", cert.email, false, true);
                doc.Replace("#insurance_type#", cert.insurance_type.ToString(), false, true);
                doc.Replace("#occupation#", cert.occupation?.ToString().Trim(), false, true);
                doc.Replace("#vehicle_category#", cert.vehicle_category?.ToString().Trim(), false, true);
                doc.Replace("#vehicle_model#", cert.vehicle_model?.ToString().Trim(), false, true);
                doc.Replace("#vehicle_year#", cert.vehicle_year?.ToString().Trim(), false, true);
                doc.Replace("#vehicle_color#", cert.vehicle_color, false, true);
                doc.Replace("#registration_number#", cert.registration_number, false, true);//#SERIAL_NUMBER#
                doc.Replace("#engine_number#", cert.engine_number, false, true);
                doc.Replace("#chassis_number#", cert.chassis_number, false, true);
                doc.Replace("#sum_insured#", string.Format("N{0:N}", cert.sum_insured), false, true);
                doc.Replace("#premium#", string.Format("N{0:N}", cert.premium), false, true);
                doc.Replace("#payment_option#", cert.payment_option, false, true);
                doc.Replace("#extensions#", $"{excess} {tracking} {flood} {srcc}", false, true);
                doc.Replace("#quotation_number#", quote_no, false, true);
                doc.Replace("#create_at#", DateTime.Now.ToString(), false, true);
                string pdfPath = HttpContext.Current.Server.MapPath($"~/Cert/GeneratedCert/{quote_no}.pdf");
                if (System.IO.File.Exists(pdfPath))
                {
                    System.IO.File.Delete(pdfPath);
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }
                else
                {
                    doc.SaveToFile(pdfPath, FileFormat.PDF);
                }
                //dont wait for certificate to send continue process and let ths OS thread do the sending without slowing down the application
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                string imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                Task.Factory.StartNew(() =>
                {
                    this.SendQuotationEmail(cert, template, imagepath, pdfPath, template, quote_no);
                });

                response.status = 200;
            }
            catch (Exception ex)
            {
                response.status = 401;
                response.message = $"System malfunction";
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }

            return response;
        }
        public async Task<string> GenerateQuoteNumber(int val)
        {
            string final = "";
            if (val <= 9)
            {
                final = "000000" + val;
            }
            else if (val.ToString().Length < 7)
            {
                int loop = 7 - val.ToString().Length;
                string zeros = "";
                for (int i = 0; i < loop; i++)
                {
                    zeros += "0";
                }
                final = zeros + val;
            }
            else
            {
                final = val.ToString();
            }

            return final;
        }
        public bool IsValid(string emailaddress)
        {
            try
            {
                if (string.IsNullOrEmpty(emailaddress))
                {
                    return false;
                }
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public bool isValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }
            if (phone?.Trim().Length == 11)
            {
                return true;
            }
            if (phone?.Trim().Length == 14)
            {
                return true;
            }

            return false;
        }
        public string numberin234(string number)
        {
            if (string.IsNullOrEmpty(number))
                return null;

            if (number.StartsWith("+"))
                return number.Remove(0, 1);
            else if (number.StartsWith("0"))
                return $"234{number.Remove(0, 1)}";

            return number;
        }
        public string PolicyType(string policy_number)
        {
            string policy_type = "";

            #region -- Regex matching for policy numbers
            if (new Regex(@"(\/V\/29\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PRIVATE CAR INSURANCE";
            }
            else if (new Regex(@"(\/V\/29A\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PRIVATE CAR INSURANCE (E-PLATEFORM)";
            }
            else if (new Regex(@"(\/V\/30\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "COMMERCIAL VEHICLE (OWN GOODS)";
            }
            else if (new Regex(@"(\/ V\/ 30A\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "COMMERCIAL VEHICLE (E-PLATEFORM)";
            }
            else if (new Regex(@"(\/ V\/ 31\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MOTORCYCLE INSURANCE";
            }
            else if (new Regex(@"(\/ V\/ 32\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MOTOR TRADE INSURANCE (COMBINED)";
            }
            else if (new Regex(@"(\/ V\/ 33\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MOTOR TRADE (ROAD RISKS)";
            }
            else if (new Regex(@"(\/ V\/ 34\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MOTOR TRADE (INTERNAL RISK)";
            }
            else if (new Regex(@"(\/ V\/ 59\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "COMMERCIAL VEHICLE (GENERAL CARTAGE)";
            }
            else if (new Regex(@"(\/ V\/ 60\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "COMMERCIAL VEHICLE  (SPECIAL TYPE)";
            }
            else if (new Regex(@"(\/ V\/ 61\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "COMMERCIAL VEHICLE  (STAFF BUS)";
            }
            else if (new Regex(@"(\/ M\/ 27\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MARINE CARGO";
            }
            else if (new Regex(@"(\/ M\/ 28\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MARINE HULL";
            }
            else if (new Regex(@"(\/ F\/ 01\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "FIRE/SPECIAL PERILS";
            }
            else if (new Regex(@"(\/ F\/ 01A\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "TERRORISM & POLITICAL RISKS";
            }
            else if (new Regex(@"(\/ F\/ 02\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "CONSEQUENTIAL LOSS";
            }
            else if (new Regex(@"(\/ E\/ 24\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "ELECTRONICS EQUIPMENT INSURANCE";
            }
            else if (new Regex(@"(\/ E\/ 25\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MACHINERY BREAKDOWN INSURANCE";
            }
            else if (new Regex(@"(\/ E\/ 26\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "BOILER & PRESSURE VESSEL INSURANCE";
            }
            else if (new Regex(@"(\/ E\/ 36\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "BOILER";
            }
            else if (new Regex(@"(\/ E\/ 40\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MACHINERY BREAKDOWN CONSEQUENTIAL LOSS INSURANCE";
            }
            else if (new Regex(@"(\/ E\/ 41\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "ADVANCE PROFIT INSURANCE POLICY";
            }
            else if (new Regex(@"(\/ E\/ 21\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "CONTRACTORS ALL RISKS";
            }
            else if (new Regex(@"(\/ E\/ 22\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "ERECTION ALL RISKS";
            }
            else if (new Regex(@"(\/ E\/ 23\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PLANT ALL RISKS";
            }
            else if (new Regex(@"(\/ B\/ 38\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "CREDIT BOND";
            }
            else if (new Regex(@"(\/ B\/ 17\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "TENDER BOND";
            }
            else if (new Regex(@"(\/ B\/ 18\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PERFORMANCE BOND";
            }
            else if (new Regex(@"(\/ B\/ 19\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "ADVANCE PAYMENT BOND";
            }
            else if (new Regex(@"(\/ B\/ 20\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "CUSTOM BOND";
            }
            else if (new Regex(@"(\/ H\/ 39\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "AVIATION INSURANCE (HULL - ALL - RISKS)";
            }
            else if (new Regex(@"(\/ Z\/ 46\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY(OPERATIONAL RISK)";
            }
            else if (new Regex(@"(\/ Z\/ 47\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY (CONSTRUCTION RISK)";
            }
            else if (new Regex(@"(\/ Z\/ 49\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY(OPER. RISK) - THIRD PARY LIABILITY";
            }
            else if (new Regex(@"(\/ Z\/ 50\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY(OPER. RISK) - THIRD PARY LIABILITY";
            }
            else if (new Regex(@"(\/ Z\/ 52\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY(OPER. RISK)";
            }
            else if (new Regex(@"(\/ Z\/ 53\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY (CONSTRUCTION RISK)";
            }
            else if (new Regex(@"(\/ Z\/ 54\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY(OPER. RISK) - THIRD PARY LIABILITY";
            }
            else if (new Regex(@"(\/ Z\/ 55\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OIL AND ENERGY(OPER. RISK) - THIRD PARY LIABILITY";
            }
            else if (new Regex(@"(\/ P\/ 62\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PACKAGE/COMBINED/IAR";
            }
            else if (new Regex(@"(\/ S\/ 48\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "WARRANTY INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 03\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "BURGLARY BUSINESS PREMISES";
            }
            else if (new Regex(@"(\/ A\/ 04\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "BURGLARY PRIVATE PREMISES";
            }
            else if (new Regex(@"(\/ A\/ 04\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "BURGLARY PRIVATE PREMISES";
            }
            else if (new Regex(@"(\/ A\/ 044\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "DIRECTORS LIABILITY INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 05\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "CASH-IN-TRANSIT/MONEY";
            }
            else if (new Regex(@"(\/ A\/ 06\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "CASH-IN-SAFE";
            }
            else if (new Regex(@"(\/ A\/ 07\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "GOODS IN TRANSIT";
            }
            else if (new Regex(@"(\/ A\/ 08\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PROFESSIONAL INDEMNITY";
            }
            else if (new Regex(@"(\/ A\/ 09\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "GROUP PERSONAL ACCIDENT";
            }
            else if (new Regex(@"(\/ A\/ 10\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PERSONAL ACCIDENT";
            }
            else if (new Regex(@"(\/ A\/ 11\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PUBLIC LIABILITY";
            }
            else if (new Regex(@"(\/ A\/ 12\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "PRODUCT LIABILITY";
            }
            else if (new Regex(@"(\/ A\/ 13\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "ALL RISKS";
            }
            else if (new Regex(@"(\/ A\/ 14\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "FIDELITY GUARANTEE";
            }
            else if (new Regex(@"(\/ A\/ 15\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "EMPLOYEES' ACCIDENT BENEFIT INSURANCE POLICY";
            }
            else if (new Regex(@"(\/ A\/ 16\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "COMBINED HOUSEHOLDERS & HOUSEOWNERS";
            }
            else if (new Regex(@"(\/ A\/ 35\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "GOLFERS POLICY";
            }
            else if (new Regex(@"(\/ A\/ 37\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "COMBINED ASSETS INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 42\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "GLASS INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 43\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "MOBILE PHONE INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 44\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "TRAVEL INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 45\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "AIRPORT OWNERS AND OPERATORS LIABILITY INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 51\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "BUSINESS OWNERS POLICY";
            }
            else if (new Regex(@"(\/ A\/ 57\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "OCCUPIERS LIABILITY INSURANCE";
            }
            else if (new Regex(@"(\/ A\/ 58\/){ 1}").IsMatch(policy_number))
            {
                policy_type = "HEALTH CARE PROFESSIONAL INDEMNITY INS.";
            }
            #endregion

            return policy_type;
        }
        public List<string> GeneralClaimType(string policy_number)
        {
            List<string> claimType = new List<string>();
            var loss = new List<string>() { "V/29", "V/30", "V/31" };
            foreach (var item in loss)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Partial Loss");
                    claimType.Add("Total Loss");
                    claimType.Add("Theft");
                    claimType.Add("Vandalization");
                }
            }
            var accidental = new List<string>() { "A/16", "A/15", "P/62" };
            foreach (var item in accidental)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Accidental Injury");

                }
            }

            var death = new List<string>() { "F/01", "P/62", "A/09", "A/15", "V/29", "V/30", "V/31" };
            foreach (var item in death)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Death");

                }
            }

            var burglary = new List<string>() { "A/03", "F/01", "P/62", "A/16" };
            foreach (var item in burglary)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Burglary");

                }
            }

            var theft = new List<string>() { "A/03", "F/01", "P/62", "A/16" };
            foreach (var item in theft)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Theft");

                }
            }

            var flood = new List<string>() { "F/01", "P/62", "A/16", "V/29", "V/30", "V/31" };
            foreach (var item in flood)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Flood");
                    claimType.Add("Fire");

                }
            }

            var damageofgoods = new List<string>() { "F/01", "P/62", "A/07" };
            foreach (var item in damageofgoods)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Damage to goods");
                }
            }

            var guarantee = new List<string>() { "F/01", "A/14", "P/62" };
            foreach (var item in guarantee)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Fidelity guarantee");
                }
            }

            var money = new List<string>() { "F/01", "P/62", "A/05" };
            foreach (var item in money)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Money");
                }
            }

            var liability = new List<string>() { "F/01", "P/62", "A/11" };
            foreach (var item in liability)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Liability");
                }
            }

            var employee = new List<string>() { "F/01", "P/62", "A/15" };
            foreach (var item in employee)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Employee accident");
                }
            }

            return claimType;
        }
        public List<string> GeneralClaimTypeUpdated(string policy_number, out List<dynamic> damageType)
        {
            List<string> claimType = new List<string>();
            damageType = new List<dynamic>();
            List<string> types = null;
            var loss = new List<string>() { "V/29", "V/30", "V/31" };
            foreach (var item in loss)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    types = new List<string>();
                    var to = new List<string>() { "Fire", "Accident", "Flood" };
                    types.Add("Partial Loss");
                    types.Add("Total Loss");
                    damageType.Add(new
                    {
                        //Category = "MOTOR",
                        DamageTypes = types,
                        AppliesTo = to
                    });

                    claimType.Add("Theft");
                    claimType.Add("Vandalization");
                    claimType.Add("Accident");
                }
            }
            var accidental = new List<string>() { "A/16", "A/15", "P/62" };
            foreach (var item in accidental)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {

                    claimType.Add("Accidental Injury");

                }
            }

            var death = new List<string>() { "F/01", "P/62", "A/09", "A/15", "V/29", "V/30", "V/31" };
            foreach (var item in death)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    types = new List<string>();
                    var to = new List<string>() { "3rd Party" };
                    types.Add("Death");
                    types.Add("Property");
                    types.Add("Bodily Injury");
                    damageType.Add(new
                    {
                        //Category = "MOTOR",
                        DamageTypes = types,
                        AppliesTo = to
                    });

                    claimType.Add("3rd Party");

                }
            }

            var burglary = new List<string>() { "A/03", "F/01", "P/62", "A/16" };
            foreach (var item in burglary)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Burglary");

                }
            }

            var theft = new List<string>() { "A/03", "F/01", "P/62", "A/16" };
            foreach (var item in theft)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Theft");

                }
            }

            var flood = new List<string>() { "F/01", "P/62", "A/16", "V/29", "V/30", "V/31" };
            foreach (var item in flood)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Flood");
                    claimType.Add("Fire");

                }
            }

            var damageofgoods = new List<string>() { "F/01", "P/62", "A/07" };
            foreach (var item in damageofgoods)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Damage to goods");
                }
            }

            var guarantee = new List<string>() { "F/01", "A/14", "P/62" };
            foreach (var item in guarantee)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Fidelity guarantee");
                }
            }

            var money = new List<string>() { "F/01", "P/62", "A/05" };
            foreach (var item in money)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Money");
                }
            }

            var liability = new List<string>() { "F/01", "P/62", "A/11" };
            foreach (var item in liability)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    claimType.Add("Liability");
                }
            }

            var employee = new List<string>() { "F/01", "P/62", "A/15" };
            foreach (var item in employee)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    types = new List<string>();
                    var to = new List<string>() { "Employee accident" };
                    types.Add("Accidental Injury");
                    types.Add("Death");
                    claimType.Add("Employee accident");
                    damageType.Add(new
                    {
                        // Category = "NON_MOTOR",
                        DamageTypes = types,
                        AppliesTo = to
                    });
                }
            }

            return claimType;
        }

        public string GeneralClaimCategory(string policy_number)
        {

            var loss = new List<string>() { "V/29", "V/30", "V/31" };
            foreach (var item in loss)
            {
                return "MOTOR";
            }
            var accidental = new List<string>() { "A/16", "A/15", "P/62" };
            foreach (var item in accidental)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {

                    return "NON_MOTOR";

                }
            }

            var death = new List<string>() { "F/01", "P/62", "A/09", "A/15", "V/29", "V/30", "V/31" };
            foreach (var item in death)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "MOTOR";

                }
            }

            var burglary = new List<string>() { "A/03", "F/01", "P/62", "A/16" };
            foreach (var item in burglary)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";

                }
            }

            var theft = new List<string>() { "A/03", "F/01", "P/62", "A/16" };
            foreach (var item in theft)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";

                }
            }

            var flood = new List<string>() { "F/01", "P/62", "A/16", "V/29", "V/30", "V/31" };
            foreach (var item in flood)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";

                }
            }

            var damageofgoods = new List<string>() { "F/01", "P/62", "A/07" };
            foreach (var item in damageofgoods)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";
                }
            }

            var guarantee = new List<string>() { "F/01", "A/14", "P/62" };
            foreach (var item in guarantee)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";
                }
            }

            var money = new List<string>() { "F/01", "P/62", "A/05" };
            foreach (var item in money)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";
                }
            }

            var liability = new List<string>() { "F/01", "P/62", "A/11" };
            foreach (var item in liability)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";
                }
            }

            var employee = new List<string>() { "F/01", "P/62", "A/15" };
            foreach (var item in employee)
            {
                if (new Regex($@".{item}").IsMatch(policy_number))
                {
                    return "NON_MOTOR";
                }
            }

            return "NON_MOTOR";
        }
        public List<string> LifeClaimTypes(string productName)
        {
            var termination = new List<string>() { "esusu", "capital",
                "provident",
                "investment", "ordinary", "educational", "harvest","dignity","funeral",
                "tuition","assurance","whole","mortgage","credit"};
            var fullmaturity = new List<string>() { "esusu", "capital", "provident", "investment", "ordinary", "educational", "harvest" };
            var surender = new List<string>() { "esusu", "capital", "provident", "investment", "ordinary", "educational", "harvest", "deferred" };
            var personalaccident = new List<string>() { "esusu" };
            var maturityAndMedical = new List<string>() { "harvest" };
            var loan = new List<string>() { "esusu", "capital", "provident", "investment", "ordinary", "educational", "harvest" };
            var death = new List<string>() { "esusu", "capital",
                "provident",
                "investment", "ordinary", "educational", "harvest","dignity","funeral",
                "tuition","assurance","whole","mortgage","credit","Immediate","Retiree","laspec","despeb","tuition" };
            var permanentDisability = new List<string>() { "dignity", "funeral", "tuition", "assurance", "whole" };

            var critical_illness = new List<string>() { "critical" };

            List<string> claimsType = new List<string>();
            if (termination.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Termination");
            }
            if (permanentDisability.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Permanent Disability");
            }

            if (critical_illness.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Diagnosis");
                claimsType.Add("Death with Diagnosis");
            }

            if (fullmaturity.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Full Maturity");
            }

            if (surender.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Surrender");
            }

            if (personalaccident.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Personal Accident");
            }

            if (maturityAndMedical.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Partial Maturity");
                claimsType.Add("Medical Expenses");
            }

            if (loan.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Loan");
            }

            if (death.Any(x => productName.Contains(x)))
            {
                claimsType.Add("Death");
            }

            return claimsType;
        }
        public DivisonsCode GetGeneralDivision(string policy_number)
        {
            DivisonsCode division;
            string code = policy_number?.Trim().Split('/')[0]?.ToUpper();

            if (new Regex($@".t").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "TRADING",
                    code = code
                };
            }
            else if (new Regex($@".b").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "RETAILBANCASSURANCE",
                    code = code
                };
            }
            else if (new Regex($@".g").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "PERSONALLINES",
                    code = code
                };
            }
            else if (new Regex($@".f").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "FINANCIALINSTITUTIONS",
                    code = code
                };
            }
            else if (new Regex($@".m").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "MANUFACTURING",
                    code = code
                };
            }
            else if (new Regex($@".e").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "ENGINEERINGTELECOMS",
                    code = code
                };
            }
            else if (new Regex($@".p").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "PUBLICSECTOR",
                    code = code
                };
            }
            else if (new Regex($@"[asz]").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "OILANDGAS",
                    code = code
                };
            }
            else if (new Regex($@".l").IsMatch(policy_number))
            {
                division = new DivisonsCode
                {
                    name = "EBUSINESS",
                    code = code
                };
            }
            else
            {
                division = new DivisonsCode
                {
                    name = "BRANCH",
                    code = code
                };
            }
            return division;
        }
        public async Task<dynamic> GetChakaOauthToken(string userId)
        {
            try
            {
                string url = $"{Config.CHAKA_BASE_URL}/oauth/token";
                var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.CHAKA_BASIC_AUTH));
                log.Info($"About to get token from chaka for userId: {userId}");


                using (var http = new HttpClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.DefaultConnectionLimit = 9999;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    http.DefaultRequestHeaders.Add("Authorization", $"Basic {basicAuth}");
                    StringContent content = new StringContent("data=scope=profile&grant_type=client_credentials",
                        Encoding.UTF8, "application/x-www-form-urlencoded");
                    HttpResponseMessage response = await http.PostAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var msg = await response.Content.ReadAsStringAsync();
                        log.Info($"Session is invalid. Please login again {userId}:{response.StatusCode}: => {msg}");
                        return new
                        {
                            status = (int)response.StatusCode,
                            message = "Session is invalid. Please login again"
                        };
                    }
                    log.Info($"Session loaded successfully {userId}");
                    var processResponse = await response.Content.ReadAsStringAsync();
                    log.Info($"Session loaded successfully data: {processResponse}");
                    return new
                    {
                        status = 200,
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(processResponse)
                    };
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public async Task<dynamic> ChakaSignUp(ChakaSignUp signUp)
        {
            try
            {
                var checkformail = await chaka.FindOneByCriteria(x => x.email.ToLower() == signUp.email.ToLower() && x.isActive == true);
                if (checkformail != null)
                {
                    return new
                    {
                        status = 204,
                        message = "Account already exist. please login to access your profile"
                    };
                }

                var validOTP = await ValidateOTP(signUp.otp, signUp.mobileno);
                if (!validOTP)
                {
                    return new
                    {
                        status = 202,
                        message = "Invalid OTP"
                    };
                }
                var getToken = await GetChakaOauthToken(signUp.email);
                if (getToken.status != 200)
                {
                    return new
                    {
                        status = getToken.status,
                        message = getToken.message
                    };
                }
                else
                {

                    using (var http = new HttpClient())
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.DefaultConnectionLimit = 9999;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        string url = $"{Config.CHAKA_BASE_URL}/api/v1/users/signup";
                        http.DefaultRequestHeaders.Clear();
                        http.DefaultRequestHeaders.Add("Authorization", $"Bearer {getToken.data.access_token}");
                        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var request = await http.PostAsJsonAsync<ChakaSignUp>(url, signUp);
                        if (!request.IsSuccessStatusCode)
                        {
                            var response = await request.Content.ReadAsAsync<dynamic>();
                            return new
                            {
                                status = 409,
                                message = response.message
                            };
                        }
                        else
                        {
                            var response = await request.Content.ReadAsAsync<dynamic>();
                            var saveData = new Chaka
                            {
                                admin = response.data.admin,
                                chakaId = response.data.chakaId,
                                clientId = response.data.clientId,
                                createdAt = DateTime.Now,
                                email = response.data.email,
                                firstName = signUp.firstName,
                                isActive = true,
                                lastName = signUp.lastName,
                                mobileNumber = signUp.mobileno,
                                password = await Sha512(signUp.password),
                                role = response.data.role,
                                superAdmin = response.superAdmin,
                                userId = response.data.id,
                                username = response.data.username,
                                modifiedAt = DateTime.Now,
                                verified = response.data.verified
                            };
                            if (!await chaka.Save(saveData))
                            {
                                return new
                                {
                                    status = 207,
                                    data = "Unable to finish operation"
                                };
                            }
                            return new
                            {
                                status = 200,
                                data = "Onboarding successful"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<dynamic> AuthenticateChakaUser(string email, string password)
        {
            try
            {
                string passwordHash = await Sha512(password);
                var authenticateUser = await chaka.FindOneByCriteria(x => x.email.ToLower() == email.ToLower() && x.password == passwordHash && x.isActive == true);
                if (authenticateUser == null)
                {
                    return new
                    {
                        status = 204,
                        message = "Invalid username/password"
                    };
                }
                string url = $"{Config.CHAKA_BASE_URL}/oauth/token";
                //var basicAuth = base64Decode(Config.CHAKA_BASIC_AUTH);
                var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.CHAKA_BASIC_AUTH));
                log.Info($"About to get token from chaka for userId: {email}");

                using (var http = new HttpClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.DefaultConnectionLimit = 9999;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    http.DefaultRequestHeaders.Clear();
                    http.DefaultRequestHeaders.Add("Authorization", $"Basic {basicAuth}");
                    http.DefaultRequestHeaders.Add("userId", authenticateUser.userId);
                    StringContent content = new StringContent($"data=scope=profile&grant_type=client_credentials",
                        Encoding.UTF8, "application/x-www-form-urlencoded");
                    HttpResponseMessage response = await http.PostAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        log.Info($"Session is invalid. Please login again {email}");
                        return new
                        {
                            status = 203,
                            message = "Session is invalid. Please login again"
                        };
                    }
                    log.Info($"Session loaded successfully {email}");
                    var processResponse = await response.Content.ReadAsAsync<dynamic>();
                    log.Info($"Session loaded successfully data: {processResponse}");
                    string mode = Config.isDemo ? "TEST" : "LIVE";
                    return new
                    {
                        status = 200,
                        // chaka_url = $"{Config.CHAKA_APP_URL}/{mode}/{processResponse.access_token}",
                        token = processResponse.access_token,
                        mode = mode,
                        script_url = Config.CHAKA_SCRIPT_URL
                    };

                    //http://sdk.chakaent.com/${mode}/${token} `
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<dynamic> ChakaResetPassword(string email, string password, string otp)
        {
            try
            {
                var checkformail = await chaka.FindOneByCriteria(x => x.email.Trim().ToLower() == email.Trim().ToLower() && x.isActive == true);
                if (checkformail == null)
                {
                    return new
                    {
                        status = 204,
                        message = "User does not exist"
                    };
                }

                var validOTP = await ValidateOTP(otp, checkformail.mobileNumber.Trim());
                if (!validOTP)
                {
                    return new
                    {
                        status = 202,
                        message = "Invalid OTP"
                    };
                }
                checkformail.password = await Sha512(password);
                checkformail.modifiedAt = DateTime.Now;
                if (!await chaka.Update(checkformail))
                {
                    return new
                    {
                        status = 209,
                        message = "Password change was not successful"
                    };
                }

                return new
                {
                    status = 200,
                    message = "Operation was successful"
                };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void SendWealthPlusMail(WealthPlusView mail, bool IsCustodian, string template, string imagepath, string attachmentpath, string divisionemail = "")
        {
            try
            {
                string test = Config.isDemo ? "Test" : "";
                log.Info($"About to send email to {mail.Email}");
                StringBuilder sb = new StringBuilder(template);
                log.Info($"About to send temp to here");
                sb.Replace("#NAME#", $"{mail.Title} {mail.Surname} {mail.MiddleName}  {mail.FirstName}");
                sb.Replace("#EMAILADDRESS#", mail.Email?.ToLower());
                sb.Replace("#PHONENUMBER#", mail.MobileNo);
                sb.Replace("#ADDRESS#", mail.address);
                sb.Replace("#DOB#", mail.DOB.ToShortDateString());
                sb.Replace("#GENDER#", mail.Gender);
                sb.Replace("#TERM#", mail.PolicyTerm.ToString());
                sb.Replace("#FREQUENCY#", mail.Frequency);
                sb.Replace("#AMOUNT#", string.Format("{0:N}", mail.AmountToSave));
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                log.Info($"About to send param to all");
                var image_path = imagepath;
                if (IsCustodian)
                {
                    sb.Replace("#FOOTER#", "");
                    string msg_1 = @"Dear Team,<br/><br/> A customer with details below requested for quotation";
                    sb.Replace("#CONTENT#", msg_1);
                    var email = ConfigurationManager.AppSettings["Notification"];
                    var list = email.Split('|');
                    string emailaddress = "";
                    List<string> cc = new List<string>();
                    if (list.Count() > 1)
                    {
                        int i = 0;
                        if (!string.IsNullOrEmpty(divisionemail))
                        {
                            emailaddress = divisionemail;
                        }
                        else
                        {
                            emailaddress = list[0];
                        }

                        foreach (var item in list)
                        {
                            if (!string.IsNullOrEmpty(divisionemail))
                            {
                                cc.Add(item);
                                ++i;
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    ++i;
                                    continue;
                                }
                                else
                                {
                                    cc.Add(item);
                                    ++i;
                                }
                            }
                        }
                    }
                    else
                    {
                        //emailaddress = list[0];
                        if (!string.IsNullOrEmpty(divisionemail))
                        {
                            emailaddress = divisionemail;
                            cc.Add(list[0]);
                        }
                        else
                        {
                            emailaddress = list[0];
                        }
                    }
                    List<string> attach = null;
                    if (!string.IsNullOrEmpty(attachmentpath))
                    {
                        attach = new List<string>();
                        attach.Add(attachmentpath);
                    }
                    var send = new SendEmail().Send_Email(emailaddress, $"Quote Request Custodian WealthPlus {test}", sb.ToString(), $"Quote Request Custodian WealthPlus {test}", true, image_path, cc, null, attach);
                }
                else
                {
                    //sb.Replace("#FOOTER#", @"Please visit our website to confirm the status of your claim.<br /><br />
                    //                If you did not initiate this process, please contact us on (+234)12774008-9 or carecentre@custodianinsurance.com");
                    //string msg_1 = @"Dear Valued Customer,<br/><br/>Your claim with the below details has been submitted successfully.";
                    //sb.Replace("#CONTENT#", msg_1);
                    //var send = new SendEmail().Send_Email(mail.email_address, $"Claim Request {test}", sb.ToString(), $"Claim Request {test}", true, image_path, null, null, null);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        public async Task<dynamic> SendFirebaseNotification(string title, string body, string email)
        {
            try
            {
                var context = new store<AdaptLeads>();
                var get_fcm_token = await context.FindOneByCriteria(x => x.email?.ToLower().Trim() == email?.ToLower().Trim());
                if (get_fcm_token == null)
                {
                    return new
                    {
                        status = 206,
                        message = "Customer Adapt email not tied to policy"
                    };
                }
                using (var api = new HttpClient())
                {
                    api.DefaultRequestHeaders.Add("Authorization", $"key={Config.FIREBASE_AUTHORIZATION}");
                    api.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    var payload = new Firebase
                    {
                        to = get_fcm_token.fcm_token,
                        notification = new fNotification
                        {
                            badge = 1,
                            body = body,
                            title = title
                        }
                    };

                    var request = await api.PostAsJsonAsync(Config.FIREBASE_URL, payload);
                    if (!request.IsSuccessStatusCode)
                    {
                        return new
                        {
                            status = 209,
                            message = "Unable to send push notification to Adapt"
                        };
                    }

                    var response = await request.Content.ReadAsAsync<dynamic>();
                    return new
                    {
                        status = 200,
                        data = response
                    };
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<WPPCal> ValidateWealthPlusCoverLimits(decimal sumAssured, Frequency frequency, decimal premium, int terms)
        {
            int multiplyer = 0;
            decimal projectAmount = 0;
            var rate = GlobalConstant.WPP_RATE / 100m;


            if (frequency == Frequency.Monthly)
            {
                multiplyer = 12; //10,000 * (1 + 0.05/1) ^ (10*1)

            }
            else if (frequency == Frequency.Annually)
            {
                multiplyer = 1;
            }
            else if (frequency == Frequency.Quarterly)
            {
                multiplyer = 4;
            }
            else if (frequency == Frequency.Semi_Annually)
            {
                multiplyer = 2;
            }

            if (multiplyer == 0)
            {
                //return new
                //{
                //    message = "Invalid Frequency type",
                //    status = 203
                //};
                return new WPPCal
                {
                    status = 206,
                    message = "Invalid Frequency type"
                };
            }

            var targetAmount = (terms * multiplyer) * premium;
            var getMaxPercentage = (targetAmount * GlobalConstant.GET_WEALTHPLUS_PERCENTAGE) / 100m;
            if (sumAssured < getMaxPercentage)
            {
                //return $"Life cover Sum Assured {string.Format("{0:N}", sumAssured)} Cannot be greater than Investment Sum Assured {string.Format("{0:N}", targetAmount)}";
                return new WPPCal
                {
                    // message = $"Life cover Sum Assured {string.Format("{0:N}", sumAssured)} Cannot be greater than Investment Sum Assured {string.Format("{0:N}", targetAmount)}",
                    status = 203,
                    message = $"Sum assured of {string.Format("N {0:N}", sumAssured)} amount cannot be less than 30% of your Total contribution of {string.Format("N {0:N}", targetAmount)} amount"
                };
            }

            decimal total = 0;
            if (frequency != Frequency.Annually)
            {
                decimal _rate = 1 + (rate / multiplyer);
                decimal cal = DecimalMath.DecimalEx.Pow(_rate, multiplyer);
                decimal power = 1 / Convert.ToDecimal(multiplyer);
                decimal interest_rate = DecimalMath.DecimalEx.Pow(cal, power) - 1;
                for (int i = 1; i <= terms; ++i)
                {
                    for (int k = 1; k <= multiplyer; k++)
                    {
                        var p = (premium + total) * interest_rate;
                        total += premium + p;
                    }

                }
            }
            else
            {
                decimal interest_rate = 1 + (rate / multiplyer) - 1;
                for (int k = 1; k <= terms; k++)
                {
                    var p = (premium + total) * interest_rate;
                    total += premium + p;
                }
            }

            return new WPPCal
            {
                message = "validation passed",
                status = 200,
                projectedAmount = Convert.ToDecimal(string.Format("{0:N}", total))
            };
            // return null;
        }
        public async Task<dynamic> AuthenticateLDAP(string username, string password)
        {
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry($"LDAP://{GlobalConstant.AD_CREDENTAILS}", username, password);
                directoryEntry.AuthenticationType = AuthenticationTypes.Secure;
                DirectorySearcher ds = new DirectorySearcher(directoryEntry);
                ds.PropertiesToLoad.Add("name");
                ds.PropertiesToLoad.Add("mail");
                ds.PropertiesToLoad.Add("givenname");
                ds.PropertiesToLoad.Add("sn");
                ds.PropertiesToLoad.Add("userPrincipalName");
                ds.PropertiesToLoad.Add("distinguishedName");
                //ds.Filter = $"(&(objectCategory=User)(objectClass=person)(name={username}))";
                var result = ds.FindOne();
                //var name = result.Properties;
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public static class Config
    {
        private const string DEFAULT_BASE_URL = "https://api-football-v1.p.rapidapi.com/v2";
        public static string BASE_URL
        {
            get
            {
                string base_url = ConfigurationManager.AppSettings["BASE_URL"];
                if (!string.IsNullOrEmpty(base_url?.Trim()))
                {
                    if (!base_url.Trim().EndsWith("/"))
                    {
                        return base_url;
                    }
                    else
                    {
                        return base_url.Remove(base_url.Length - 1, 1);
                    }
                }
                else
                {
                    return DEFAULT_BASE_URL;
                }
            }
        }
        public static string Authorization_Header
        {
            get
            {
                string header = ConfigurationManager.AppSettings["AUTH_HEADER"];
                if (!string.IsNullOrEmpty(header))
                {
                    return header;
                }
                else
                {
                    return null;
                }

            }
        }
        public static int GetID
        {
            get
            {
                string Id = ConfigurationManager.AppSettings["LeagueID"];
                if (!string.IsNullOrEmpty(Id))
                {
                    return Convert.ToInt32(Id);
                }
                else
                {
                    return 3;
                }
            }
        }
        public static bool isDemo
        {
            get
            {
                string demo = ConfigurationManager.AppSettings["IsDemo"];
                if (bool.TryParse(demo, out bool result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
        }

        private const string DEFAULT_CHAKA_URL = "http://auth.chakaent.com";
        public static string CHAKA_BASE_URL
        {
            get
            {
                string base_url = ConfigurationManager.AppSettings["CHAKA_BASE_URL"];
                if (!string.IsNullOrEmpty(base_url?.Trim()))
                {
                    if (!base_url.Trim().EndsWith("/"))
                    {
                        return base_url;
                    }
                    else
                    {
                        return base_url.Remove(base_url.Length - 1, 1);
                    }
                }
                else
                {
                    return DEFAULT_BASE_URL;
                }
            }
        }

        public static string CHAKA_BASIC_AUTH
        {
            get
            {
                string base_url = ConfigurationManager.AppSettings["CHAKA_BASIC_AUTH"];
                if (string.IsNullOrEmpty(base_url))
                    return null;
                return base_url;
            }
        }

        public static string CHAKA_APP_URL
        {
            get
            {
                string base_url = ConfigurationManager.AppSettings["CHAKA_APP_URL"];
                if (!string.IsNullOrEmpty(base_url))
                {
                    return base_url;
                }
                else
                {
                    return "http://sdk.chakaent.com";
                }
            }
        }
        public static string CHAKA_SCRIPT_URL
        {
            get
            {
                string script_url = ConfigurationManager.AppSettings["CHAKA_SCRIPT_URL"];
                if (!string.IsNullOrEmpty(script_url))
                {
                    return script_url;
                }
                else
                {
                    return "https://self.chakaent.com/assets/js/chakasdk.js";//https://sdk.chakaent.com/assets/js/chakasdk.js 
                }
            }
        }

        private const string DEFAULT_INTER_STATE_URL = "https://online.interstatesecurities.com/customers/signup/api";
        public static string INTER_STATE_URL
        {
            get
            {
                string url = ConfigurationManager.AppSettings["INTER_STATE_URL"];
                if (!string.IsNullOrEmpty(url))
                {
                    return url;
                }
                else
                {
                    return INTER_STATE_URL;
                }
            }
        }
        public static string INTER_STATE_TERMINALID { get; } = ConfigurationManager.AppSettings["INTER_STATE_TERMINALID"];
        public static string FIREBASE_AUTHORIZATION { get; } = ConfigurationManager.AppSettings["FIREBASE_AUTHORIZATION"];
        public static string FIREBASE_URL
        {
            get
            {
                string base_url = ConfigurationManager.AppSettings["FIREBASE_URL"];
                return base_url?.Trim();
            }
        }
    }
    public class cron
    {
        public cron()
        {

        }

        public void logTimer(PerformContext log)
        {
            log.WriteLine($"Log me time ==================== {DateTime.Now} ===========================");
        }
    }
}
