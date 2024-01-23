using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PaystackRecurringChargesController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<PaystackRecurringCharges> charges = null;
        public PaystackRecurringChargesController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            charges = new store<PaystackRecurringCharges>();
        }

        [HttpPost]
        public async Task<notification_response> SetUpRecurringPayment(PaystackCharges paystack)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                        status = 401
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SetUpRecurringPayment", paystack.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }


                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == paystack.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {paystack.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(paystack.authorization_code + paystack.policy_number + paystack.customer_email + paystack.signature, config.secret_key, paystack.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {paystack.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var cardhash = util.Sha256(paystack.bank?.Trim() + paystack.bin.Trim() + paystack.card_type?.Trim() + paystack.exp_month?.Trim() + paystack.exp_year?.Trim() + paystack.policy_number?.Trim().ToUpper() + paystack.last4?.Trim() + Guid.NewGuid().ToString());
                var checkIfCardAlreadyAddToProduct = await charges.FindOneByCriteria(x => x.card_unique_token == cardhash && x.is_active == true);
                if (checkIfCardAlreadyAddToProduct != null)
                {
                    return new notification_response
                    {
                        message = "Card already tied to this policy previously",
                        status = 406
                    };
                }

                var setupCard = new PaystackRecurringCharges
                {
                    signature = paystack.signature,
                    authorization_code = paystack.authorization_code,
                    card_unique_token = cardhash,
                    card_type = paystack.card_type?.Trim(),
                    bank = paystack.bank?.Trim(),
                    bin = paystack.bin?.Trim(),
                    channel = paystack.channel,
                    country_code = paystack.country_code,
                    customer_email = paystack.customer_email?.Trim().ToLower(),
                    customer_name = paystack.customer_name,
                    date_added = DateTime.Now,
                    exp_month = paystack.exp_month?.Trim(),
                    exp_year = paystack.exp_year?.Trim(),
                    is_active = true,
                    last4 = paystack.last4?.Trim(),
                    policy_number = paystack.policy_number?.Trim().ToUpper(),
                    product_name = paystack.product_name?.Trim().ToUpper(),
                    recurring_start_month = paystack.recurring_start_month,
                    reusable = paystack.reusable,
                    recurring_freqency = paystack.recurring_freqency.ToString(),
                    merchant_id = paystack.merchant_id,
                    reocurrance_state = ReOccurranceState.SCHEDULED.ToString(),
                    Amount = paystack.amount,
                    recurring_end_month = paystack.recurring_end_month,
                    subsidiary = paystack.subsidiary
                };


                if (await charges.Save(setupCard))
                {
                    return new notification_response
                    {
                        message = "Your card has been setup for recurring payment",
                        status = 200
                    };
                }
                else
                {
                    return new notification_response
                    {
                        message = "Unable to setup card for recurring payment",
                        status = 305
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetAllRecurringSetupsByEmail(string merchant_id, string email, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetAllRecurringSetupsByEmail", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
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
                var checkhash = await util.ValidateHash2(email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var recurring_list = await charges.FindMany(x => x.customer_email == email?.Trim().ToLower() && x.is_active == true);
                return new notification_response
                {
                    status = 200,
                    message = "Fetch was successful",
                    data = recurring_list
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> CancelCard(string merchant_id, string card_unique_token, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("CancelCard", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
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
                var checkhash = await util.ValidateHash2(card_unique_token, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var recurring = await charges.FindOneByCriteria(x => x.card_unique_token == card_unique_token?.Trim().ToLower() && x.is_active == true);
                if (recurring == null)
                {
                    return new notification_response
                    {
                        status = 206,
                        message = "Card token not found",
                    };
                }
                recurring.is_active = false;
                recurring.card_cancel_date = DateTime.Now;
                recurring.reocurrance_state = ReOccurranceState.CANCELLED.ToString();
                recurring.card_cancel_date = DateTime.Now;
                if (await charges.Update(recurring))
                {
                    return new notification_response
                    {
                        status = 200,
                        message = "Your recurring has been cancelled successfully"
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 200,
                        message = "Unable to cancel recurring payment"
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpPost]
        public async Task<notification_response> UpdateCard(CardUpdate cardUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                        status = 401
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SetUpRecurringPayment", cardUpdate.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 301,
                        message = "Permission denied from accessing this feature"
                    };
                }


                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == cardUpdate.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {cardUpdate.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(cardUpdate.card_unique_token , config.secret_key, cardUpdate.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {cardUpdate.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getCard = await charges.FindOneByCriteria(x => x.card_unique_token == cardUpdate.card_unique_token.Trim() && x.is_active == true);
                if (getCard == null)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Card details not found"
                    };
                }

                var today = DateTime.Now;
                if (today.Day == getCard.recurring_start_month.Day && today.Month == getCard.recurring_start_month.Month)
                {
                    return new notification_response
                    {
                        status = 309,
                        message = "This card cannot be updated because reoccuring date is set to today"
                    };
                }

                getCard.recurring_start_month = cardUpdate.recurring_start_month;
                getCard.recurring_freqency = cardUpdate.recurring_freqency.ToString();
                getCard.recurring_end_month = cardUpdate.recurring_end_month;
                if (await charges.Update(getCard))
                {
                    return new notification_response
                    {
                        status = 200,
                        message = "Card reoccuring information was updated successfully"
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 205,
                        message = "Sorry, there was a problem while updating your information, Try again"
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
