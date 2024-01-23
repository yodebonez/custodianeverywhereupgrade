using CustodianEmailSMSGateway.Email;
using NLog;
using RecurringDebitService.DbModels;
using RecurringDebitService.InternalAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RecurringDebitService.BLogic
{
    public class CardProcessor
    {
        private static connectionStr conn = new connectionStr();
        private static Logger log = LogManager.GetCurrentClassLogger();
        public CardProcessor()
        {

        }

        /// <summary>
        /// This method does the debit from Paystack and is the entry method 
        /// </summary>
        public void RecurringEngine()
        {
            try
            {
                log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ ");
                log.Info("Paystack Key: ", Const.PAYSTACK_KEY);
                log.Info("Paystack EndPoint: ", Const.PAYSTACK_ENDPOINT);
                log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ ");
                // Do some house cleaning chores before process start
                var canIProceed = UpdateExpiredCard();
                if (!canIProceed)
                {
                    // House cleaning chores method crashed prcoess should be stoped and send mail to the software team to look at the exception and cause
                    log.Info("House cleaning chores method crashed prcoess should be stoped and send mail to the software team to look at the exception and cause");
                    string message = "Please check the log file for recurring payment..House keeping method ran into and exception";
                    SendMail(null, templateTypes.SystemError, message);
                    // Break process
                    return;
                }
                DateTime today = DateTime.Now;
                //var getTodaysPayment = conn.PaystackRecurringCharges.Where(x => x.recurring_start_month.Day == today.Day && x.recurring_start_month.Month == today.Month
                //&& x.reocurrance_state == "SCHEDULED").ToList();

                //var getStartedTransaction = conn.PaystackRecurringCharges.Where(x => x.recurring_start_month.Day == today.Day && x.reocurrance_state == "STARTED").ToList();
                //var totalTrans = getStartedTransaction.Concat(getTodaysPayment);
                List<PaystackRecurringCharge> getCards = null;
                getCards = conn.PaystackRecurringCharges.SqlQuery($@"select * from [PaystackRecurringCharges]  where (month(recurring_start_month) = {today.Month} and  day(recurring_start_month) = {today.Day} and reocurrance_state = 'SCHEDULED') 
                                                                        or (day(recurring_start_month) = {today.Day} and reocurrance_state = 'STARTED')").ToList();
                if (getCards.Count() == 0)
                {
                    log.Info($"No recurring payment for today: {today}");
                    return;
                }
                //var last_run_today = DateTime.Now.Date;
                foreach (var item in getCards)
                {
                    if ((item.last_run_date.HasValue && today.Date != item.last_run_date.Value.Date) || !item.last_run_date.HasValue)
                    {
                        log.Info("================Debit Process started====================");
                        var debit = DebitCard(item);
                        if (debit != null)
                        {
                            item.last_attempt_date = DateTime.Now;
                            item.last_run_date = DateTime.Now;
                            if (item.reocurrance_state == "SCHEDULED")
                            {
                                item.reocurrance_state = "STARTED";
                            }
                            if (debit._templateTypes != templateTypes.SuccessDebit)
                            {
                                item.number_of_attempt_failed += 1;
                            }
                            else
                            {
                                item.number_of_attempt_success += 1;
                            }
                            //Update records
                            conn.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            conn.SaveChanges();


                            if (debit._templateTypes == templateTypes.SuccessDebit)
                            {
                                string _ref = $"{ debit.data.data.reference }";
                                PosTransaction(item, debit.policyDet, _ref);
                            }

                        }
                    }
                    else
                    {
                        log.Info($"Recurring payment has ran for this transaction for today {item.customer_email}---{item.product_name}---{item.customer_name}--{item.last_run_date}");
                    }


                }
                log.Info("================Debit Process finished====================");

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
            }
        }

        /// <summary>
        /// Set expired card status to EXPIRED
        /// </summary>
        private bool UpdateExpiredCard()
        {
            try
            {
                string month = DateTime.Now.Month.ToString();
                string year = DateTime.Now.Year.ToString();
                var getExpiredCards = conn.PaystackRecurringCharges.Where(x => x.exp_month == month && x.exp_year == year).ToList();
                if (getExpiredCards.Count() == 0)
                {
                    log.Info($"No card expiry for today {DateTime.Now.ToShortDateString()}");
                    return true;
                }
                foreach (var item in getExpiredCards)
                {
                    item.reocurrance_state = "EXPIRED";
                    item.card_cancel_date = DateTime.Now;
                    conn.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    conn.SaveChanges();
                    log.Info($"The following card withh toke {item.card_unique_token} has expired date {DateTime.Now}");
                    SendMail(item, templateTypes.ExpireCard);
                }
                // check for end recurring payment for the previous month
                CheckRecurringEndDate();
                return true;
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
                return false;
            }
        }
        /// <summary>
        /// Send Mail base on actions
        /// </summary>
        /// <param name="details"></param>
        /// <param name="_templateTypes"></param>
        /// <param name="errorMessage"></param>
        public void SendMail(PaystackRecurringCharge details, templateTypes _templateTypes, string errorMessage = "")
        {
            try
            {
                log.Info("about to start sending email");
                var send = new SendEmail();
                var imagepath = $"{AppDomain.CurrentDomain.BaseDirectory}/Image/logo-white.png";
                string emailTemplate = $"{AppDomain.CurrentDomain.BaseDirectory}/EmailTemplate/Email.html";
                var template = new StringBuilder(System.IO.File.ReadAllText(emailTemplate));

                if (details != null)
                {
                    var bcc = ConfigurationManager.AppSettings["Notification"].Split('|').ToList();
                    var mail = GetTemplateType(details, _templateTypes, errorMessage);
                    if (mail != null)
                    {
                        log.Info($"About to send message to customer email {mail.toAddress}");
                        template.Replace("#CONTENT#", mail.body);
                        var state = send.Send_Email(mail.toAddress, mail.subject, template.ToString(), mail.title, true, imagepath, null, bcc, null, true);
                        log.Info($"Message sending to {mail.toAddress} was {state}");
                    }
                }
                else
                {
                    var address = ConfigurationManager.AppSettings["ErrorNotifcation"].Split('|').ToList();
                    string message = errorMessage;
                    template.Replace("#CONTENT#", message);
                    var state = send.Send_Email(address[0], "Exception Notification-Recurring Payment", template.ToString(), "Exception Notification-Recurring Payment", true, imagepath, address, null, null, true);

                    log.Info($"Message sending to {address[0]} was {state}");
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
            }
        }
        /// <summary>
        /// Get actions template
        /// </summary>
        /// <param name="details"></param>
        /// <param name="_templateTypes"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private EmailData GetTemplateType(PaystackRecurringCharge details, templateTypes _templateTypes, string errorMessage = "")
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                if (_templateTypes == templateTypes.ExpireCard)
                {
                    sb.AppendLine("<p>Dear Valued Customer, </p>");
                    sb.AppendLine("<p>We noticed that one of your card settup for recurring payment has expired. Kindly update your card deatils. </p>");
                    sb.AppendLine("<p>Thank you.</p>");
                    string month = (Convert.ToInt32(details.exp_month) < 10) ? $"0{details.exp_month}" : details.exp_month;
                    sb.AppendLine($"<table  border= '0' width='600' cellpadding='0' cellspacing='0' class='container'><tr><td>Card Type</td><td>{details.card_type}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Year</td><td>{details.exp_year}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Month</td><td>{month}</td></tr>");
                    sb.AppendLine($"<tr><td>Bank</td><td>{details.bank}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Product</td><td>{details.product_name}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Policy</td><td>{details.policy_number}</td></tr>");
                    sb.AppendLine("</table>");
                    return new EmailData
                    {
                        body = sb.ToString(),
                        subject = $"Expired Card Alert {(details.product_name)} -- {details.policy_number}",
                        title = $"Expired Card Alert {(details.product_name)} -- {details.policy_number}",
                        _templateTypes = _templateTypes,
                        toAddress = details.customer_email
                    };
                }
                else if (_templateTypes == templateTypes.FailedDebit)
                {
                    sb.AppendLine("<p>Dear Valued Customer, </p>");
                    sb.AppendLine($"<p>This is to notify you that a debit failed on card details below. Reasons for failure is <strong>'{errorMessage}'</strong> </p>");
                    sb.AppendLine("<p>Thank you.</p>");
                    string month = (Convert.ToInt32(details.exp_month) < 10) ? $"0{details.exp_month}" : details.exp_month;
                    sb.AppendLine($"<table  border= '0' width='600' cellpadding='0' cellspacing='0' class='container'><tr><td>Card Type</td><td>{details.card_type}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Year</td><td>{details.exp_year}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Month</td><td>{month}</td></tr>");
                    sb.AppendLine($"<tr><td>Bank</td><td>{details.bank}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Product</td><td>{details.product_name}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Policy</td><td>{details.policy_number}</td></tr>");
                    sb.AppendLine($"<tr><td>Premium</td><td>{details.Amount}</td></tr>");
                    sb.AppendLine("</table>");
                    return new EmailData
                    {
                        body = sb.ToString(),
                        subject = $"Failed Debit Alert {(details.product_name)} -- {details.policy_number}",
                        title = $"Failed Debit Alert {(details.product_name)} -- {details.policy_number}",
                        _templateTypes = _templateTypes,
                        toAddress = details.customer_email
                    };
                }
                else if (_templateTypes == templateTypes.SuccessDebit)
                {
                    sb.AppendLine("<p>Dear Valued Customer, </p>");
                    sb.AppendLine($"<p>We wish to inform you that a <strong>Successful Debit</strong> has occurred on the card details below</p>");
                    sb.AppendLine("<p>Thank you.</p>");
                    string month = (Convert.ToInt32(details.exp_month) < 10) ? $"0{details.exp_month}" : details.exp_month;
                    sb.AppendLine($"<table  border= '0' width='600' cellpadding='0' cellspacing='0' class='container'><tr><td>Card Type</td><td>{details.card_type}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Year</td><td>{details.exp_year}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Month</td><td>{month}</td></tr>");
                    sb.AppendLine($"<tr><td>Bank</td><td>{details.bank}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Product</td><td>{details.product_name}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Policy</td><td>{details.policy_number}</td></tr>");
                    sb.AppendLine($"<tr><td>Premium</td><td>{details.Amount}</td></tr>");
                    sb.AppendLine("</table>");
                    return new EmailData
                    {
                        body = sb.ToString(),
                        subject = $"Successful Debit Alert {(details.product_name)} -- {details.policy_number}",
                        title = $"Successful Debit Alert {(details.product_name)} -- {details.policy_number}",
                        _templateTypes = _templateTypes,
                        toAddress = details.customer_email
                    };
                }
                else if (_templateTypes == templateTypes.SystemError)
                {
                    sb.AppendLine("<p>Dear Valued Customer, </p>");
                    sb.AppendLine($"<p>We wish to inform you that we're unable to perform debit on card details below</p>");
                    sb.AppendLine("<p>Thank you.</p>");
                    string month = (Convert.ToInt32(details.exp_month) < 10) ? $"0{details.exp_month}" : details.exp_month;
                    sb.AppendLine($"<table  border= '0' width='600' cellpadding='0' cellspacing='0' class='container'>");
                    sb.AppendLine($"<tr><td>Card Type</td><td>{details.card_type}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Year</td><td>{details.exp_year}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Month</td><td>{month}</td></tr>");
                    sb.AppendLine($"<tr><td>Bank</td><td>{details.bank}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Product</td><td>{details.product_name}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Policy</td><td>{details.policy_number}</td></tr>");
                    sb.AppendLine($"<tr><td>Premium</td><td>{details.Amount}</td></tr>");
                    sb.AppendLine("</table>");
                    return new EmailData
                    {
                        body = sb.ToString(),
                        subject = $"Unable to Debit Card {(details.product_name)} -- {details.policy_number}",
                        title = $"Unable to Debit Card {(details.product_name)} -- {details.policy_number}",
                        _templateTypes = _templateTypes,
                        toAddress = details.customer_email
                    };
                }
                else
                {
                    sb.AppendLine("<p>Dear Valued Customer, </p>");
                    sb.AppendLine($"<p>We wish to inform you that you recurring payment has ended on <strong>{details.recurring_end_month.ToShortDateString()}</strong></p>");
                    sb.AppendLine("<p>Thank you.</p>");
                    string month = (Convert.ToInt32(details.exp_month) < 10) ? $"0{details.exp_month}" : details.exp_month;
                    sb.AppendLine($"<table  border= '0' width='600' cellpadding='0' cellspacing='0' class='container'><tr><td>Card Type</td><td>{details.card_type}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Year</td><td>{details.exp_year}</td></tr>");
                    sb.AppendLine($"<tr><td>Expiry Month</td><td>{month}</td></tr>");
                    sb.AppendLine($"<tr><td>Bank</td><td>{details.bank}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Product</td><td>{details.product_name}</td></tr>");
                    sb.AppendLine($"<tr><td>Attached Policy</td><td>{details.policy_number}</td></tr>");
                    sb.AppendLine($"<tr><td>Premium</td><td>{details.Amount}</td></tr>");
                    sb.AppendLine("</table>");
                    return new EmailData
                    {
                        body = sb.ToString(),
                        subject = $"Recurring Payment Stopped {(details.product_name)} -- {details.policy_number}",
                        title = $"Recurring Payment Stopped {(details.product_name)} -- {details.policy_number}",
                        _templateTypes = _templateTypes,
                        toAddress = details.customer_email
                    };
                }
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
                return null;
            }
        }
        /// <summary>
        /// Check for end recuuing payment and flag to COMPLETED
        /// </summary>
        private void CheckRecurringEndDate()
        {
            try
            {
                var getExpiredCards = conn.PaystackRecurringCharges.Where(x => x.recurring_end_month.Month < DateTime.Now.Month
                && x.reocurrance_state == "STARTED" && x.recurring_end_month.Year == DateTime.Now.Year).ToList();
                if (getExpiredCards.Count() == 0)
                {
                    log.Info($"No card recurring end date for today: date {DateTime.Now.ToShortDateString()}");
                }

                foreach (var item in getExpiredCards)
                {
                    log.Info($"recurring payment stopped for email : {item.customer_email}");
                    item.reocurrance_state = "COMPLETED";
                    item.card_cancel_date = DateTime.Now;
                    conn.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    conn.SaveChanges();
                    SendMail(item, templateTypes.EndRecurring);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
            }
        }

        /// <summary>
        /// Debit customer card from paystack payment gateway
        /// </summary>
        /// <param name="paystackRecurringCharge"></param>
        /// <returns></returns>
        private PaystackChargeResponse DebitCard(PaystackRecurringCharge paystackRecurringCharge)
        {
            try
            {
                if (paystackRecurringCharge == null)
                {
                    log.Info("Process terminated at DebitCard because paystackRecurringCharge was null");
                    return null;
                }
                var company = (Company)Enum.Parse(typeof(Company), paystackRecurringCharge.subsidiary, false);
                var lookUp = PolicyLookUp(paystackRecurringCharge.policy_number?.Trim(), company);
                if (lookUp == null)
                {
                    log.Info("Process terminated at DebitCard becausse PolicyLookUp was null");
                    return null;
                }

                var chargeCardFromPayStack = ChargeCardFromPayStack(paystackRecurringCharge, lookUp);

                if (chargeCardFromPayStack._templateTypes != templateTypes.SuccessDebit)
                {
                    //Send message to client
                    SendMail(paystackRecurringCharge, chargeCardFromPayStack._templateTypes, chargeCardFromPayStack.message);
                    SendFirebaseNotification(paystackRecurringCharge.customer_email, templateTypes.FailedDebit);
                    return chargeCardFromPayStack;
                }
                chargeCardFromPayStack.policyDet = lookUp;
                return chargeCardFromPayStack;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
                return null;
            }
        }
        /// <summary>
        /// Charge customer at paystack payment gateways 
        /// </summary>
        /// <param name="paystackRecurringCharge"></param>
        /// <param name="policyDetails"></param>
        private PaystackChargeResponse ChargeCardFromPayStack(PaystackRecurringCharge paystackRecurringCharge, PolicyDet policyDetails)
        {
            try
            {
                if (paystackRecurringCharge == null || policyDetails == null)
                {
                    log.Info("Process terminated because paystackRecurringCharge object or policyDetails value is NULL");
                    return null;
                }
                using (var api = new HttpClient())
                {
                    api.DefaultRequestHeaders.Add("Authorization", $"Bearer {Const.PAYSTACK_KEY}");
                    log.Info("Paystack key: ", Const.PAYSTACK_KEY);
                    log.Info("Header set: ", api.DefaultRequestHeaders?.GetValues("Authorization"));
                    //   api.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    List<dynamic> custom_fields = new List<dynamic>();
                    custom_fields.Add(new
                    {
                        display_name = "PolicyNumber",
                        variable_name = "PolicyNumber",
                        value = paystackRecurringCharge.policy_number
                    });
                    custom_fields.Add(new
                    {
                        display_name = "Channel",
                        variable_name = "Channel",
                        value = "Adapt"
                    });
                    custom_fields.Add(new
                    {
                        display_name = "CustomerName",
                        variable_name = "CustomerName",
                        value = paystackRecurringCharge.customer_name
                    });
                    custom_fields.Add(new
                    {
                        display_name = "ProductType",
                        variable_name = "ProductType",
                        value = paystackRecurringCharge.product_name
                    });
                    custom_fields.Add(new
                    {
                        display_name = "Mode",
                        variable_name = "Mode",
                        value = "Recurring Payment"
                    });
                    custom_fields.Add(new
                    {
                        display_name = "DataSource",
                        variable_name = "DataSource",
                        value = paystackRecurringCharge.subsidiary
                    });
                    var payload = new PaystackPayload
                    {
                        amount = paystackRecurringCharge.Amount * 100,
                        authorization_code = paystackRecurringCharge.authorization_code,
                        email = paystackRecurringCharge.customer_email,
                        metadata = new Dictionary<string, List<dynamic>>()
                        {
                            {"custom_fields", custom_fields }
                        }
                    };
                    bool isError = false;
                    bool status = false;
                    string message = "";
                    string errorDump = "";
                    log.Info("Payload to paystack", Newtonsoft.Json.JsonConvert.SerializeObject(payload));
                    var request = api.PostAsJsonAsync(Const.PAYSTACK_ENDPOINT, payload).GetAwaiter().GetResult();
                    if (!request.IsSuccessStatusCode)
                    {
                        var errorMsg = request.Content.ReadAsAsync<dynamic>().GetAwaiter().GetResult();
                        status = (bool)errorMsg["status"];
                        message = errorMsg["message"];
                        isError = true;
                        errorDump = Newtonsoft.Json.JsonConvert.SerializeObject(errorMsg);
                        //log.Info($"Unable to connect to paystack to collect payment: {error}");
                        //return new PaystackChargeResponse
                        //{
                        //    _templateTypes = templateTypes.SystemError,
                        //    message = "Unable to connect to paystack to collect payment"
                        //};
                    }

                    var response = request.Content.ReadAsAsync<dynamic>().GetAwaiter().GetResult();
                    status = (isError) ? status : (bool)response["status"];
                    message = (isError) ? message : response["message"];
                    var dumpData = new PaystackRecurringDump
                    {
                        coresystememail = policyDetails.InsuredEmail?.Trim(),
                        dumpdate = DateTime.Now,
                        dumpmessage = message,
                        dumpstate = status,
                        logonemail = paystackRecurringCharge.customer_email,
                        policynumber = paystackRecurringCharge.policy_number,
                        productname = paystackRecurringCharge.product_name,
                        paystackrawdump = (isError) ? errorDump : Newtonsoft.Json.JsonConvert.SerializeObject(response)
                    };

                    conn.PaystackRecurringDumps.Add(dumpData);
                    conn.SaveChanges();
                    if (!status)
                    {
                        log.Info($"Debit failed for policy number {paystackRecurringCharge.policy_number} reasons {message}");
                        return new PaystackChargeResponse
                        {
                            message = message,
                            _templateTypes = templateTypes.FailedDebit
                        };
                    }

                    log.Info($"Debit successful for policy number {paystackRecurringCharge.policy_number} reasons {message}");
                    return new PaystackChargeResponse
                    {
                        data = response,
                        message = message,
                        _templateTypes = templateTypes.SuccessDebit
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
                return new PaystackChargeResponse
                {
                    _templateTypes = templateTypes.SystemError,
                    message = "System Error"
                };
            }
        }

        /// <summary>
        /// Look up policy from core system
        /// </summary>
        /// <param name="policyno"></param>
        /// <param name="company"></param>
        private PolicyDet PolicyLookUp(string policyno, Company company)
        {
            try
            {
                using (var api = new PolicyServicesSoapClient())
                {
                    log.Info($"About to look up policy from the core syetem. policy number {policyno}");
                    var request = api.GetPolicyDetails(Const.USERNAME, Const.PASSWORD, company.ToString(), policyno);
                    if (request != null && request.InsuredName?.Trim() != "NULL")
                    {
                        log.Info($"Policy details was found for policyno: {policyno}: data: {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
                        return request;
                    }
                    log.Info($"No record found for policyno {policyno}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Check email if it valid(this is majorly for the core system email address)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private bool IsMailValid(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                MailAddress m = new MailAddress(email);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send firebase notification to Adapt mobile app
        /// </summary>
        /// <param name="email"></param>
        /// <param name="_templateTypes"></param>
        private void SendFirebaseNotification(string email, templateTypes _templateTypes)
        {
            try
            {
                string mesg = "";
                string title = "";
                if (_templateTypes != templateTypes.SuccessDebit)
                {
                    mesg = $"Unable to debit card for recurring payment on {DateTime.Now.ToShortDateString()}";
                    title = "Failed Transaction Alert!!";
                }
                else
                {
                    mesg = $"Recurring debit was successfull on {DateTime.Now.ToShortDateString()}";
                    title = $"Successful Transaction Alert!!";
                }

                var getFcmToken = conn.AdaptLeads.FirstOrDefault(x => x.email.ToLower().Trim() == email.ToLower().Trim());

                if (getFcmToken == null)
                {
                    log.Info($"Unable to send notification to email '{email}' becasue user email was not found on the authentication table");
                    return;
                }
                if (string.IsNullOrEmpty(getFcmToken.fcm_token))
                {
                    log.Info($"Unable to send notification to email '{email}' FCM token is empty");
                    return;
                }
                var payload = new Firebase
                {
                    notification = new Notification
                    {
                        badge = 1,
                        body = mesg,
                        title = title
                    },
                    to = getFcmToken.fcm_token
                };
                using (var firebase = new HttpClient())
                {
                    firebase.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={Const.FIREBASE_TOKEN}");
                    var request = firebase.PostAsJsonAsync(Const.FIREBASE_ENDPOINT, payload).GetAwaiter().GetResult();
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info($"Unable to send notification to firebase request status: {request.StatusCode}");
                    }
                    var response = request.Content.ReadAsAsync<dynamic>().GetAwaiter().GetResult();

                    log.Info($"Notification was successfully send to Firebase: response obj: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
            }
        }

        /// <summary>
        /// Post Transaction to core system using Custodian everywhere api 2.0
        /// </summary>
        /// <param name="paystackChargeResponse"></param>
        /// <param name="policyDet"></param>
        private bool PosTransaction(PaystackRecurringCharge paystackRecurringCharge, PolicyDet policyDet, string tranxRef)
        {
            try
            {
                if (paystackRecurringCharge == null || policyDet == null)
                {
                    log.Info("Paystack  object or policy details object is null");
                    return false;
                }
                var payload = new PostTrx
                {
                    biz_unit = policyDet.BizUnit?.Trim(),
                    email_address = (string.IsNullOrEmpty(policyDet.InsuredEmail?.Trim()) || policyDet.InsuredEmail?.Trim() == "NULL") ?
                    paystackRecurringCharge.customer_email?.Trim() : policyDet.InsuredEmail?.Trim(),
                    issured_name = paystackRecurringCharge.customer_name,
                    merchant_id = Const.MERCHANT_ID,
                    phone_no = policyDet.InsuredTelNum,
                    status = "SUCCESS",
                    description = $"payment for {policyDet.BizUnit?.Trim()}",
                    policy_number = paystackRecurringCharge.policy_number,
                    premium = paystackRecurringCharge.Amount,
                    // vehicle_reg_no = paystackRecurringCharge.vehicle_reg
                    subsidiary = paystackRecurringCharge.subsidiary,
                    reference_no = tranxRef,
                };

                using (var api = new HttpClient())
                {
                    api.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"{Const.CUSTODIAN_AUTHORIZATION}");
                    var request = api.PostAsJsonAsync(Const.CUSTODIAN_ENDPOINT, payload).GetAwaiter().GetResult();
                    if (!request.IsSuccessStatusCode)
                    {
                        //send email to the dev team notifying that post transaction to the core system failed
                        var message = $"Unable to post transaction to custodian everywhere 2.0: status code {request.StatusCode}";
                        log.Info(message);
                        SendMail(null, templateTypes.SystemError, message);
                        return false;
                    }
                    var response = request.Content.ReadAsAsync<dynamic>().GetAwaiter().GetResult();
                    int status = (int)response["status"];
                    if (status != 200)
                    {

                        var message = $"Unable to post transaction to custodian everywhere 2.0: response {Newtonsoft.Json.JsonConvert.SerializeObject(response)}";
                        log.Info(message);
                        SendMail(null, templateTypes.SystemError, message);
                        return false;
                    }
                    else
                    {
                        log.Info("Transaction was posted successfully to core api");
                        //send firebase notifcation to customer
                        SendFirebaseNotification(paystackRecurringCharge.customer_email, templateTypes.SuccessDebit);
                        SendMail(paystackRecurringCharge, templateTypes.SuccessDebit);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                SendMail(null, templateTypes.SystemError, $"System crashed at: {ex.StackTrace} ----- {ex.InnerException?.ToString()}");
                return false;
            }
        }
    }
}
