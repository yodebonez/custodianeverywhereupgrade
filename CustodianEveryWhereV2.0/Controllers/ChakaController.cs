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
    public class ChakaController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<Chaka> chaka = null;
        private Utility util = null;
        public ChakaController()
        {
            _apiconfig = new store<ApiConfiguration>();
            chaka = new store<Chaka>();
            util = new Utility();
        }

        [HttpPost]
        public async Task<dynamic> ChakaOnboarding(ChakaSignUp signUp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //log.Error(Newtonsoft.Json.JsonConvert.SerializeObject(ModelState.Values));
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            log.Error(error.ErrorMessage);
                        }
                    }
                    return new claims_response
                    {
                        status = 401,
                        message = "Missing parameters from request"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("ChakaOnboarding", signUp.merchant_id);
                if (!check_user_function)
                {
                    return new claims_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == signUp.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {signUp.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(signUp.email + signUp.password, config.secret_key, signUp.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {signUp.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var request = await util.ChakaSignUp(signUp);
                return request;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpGet]
        public async Task<dynamic> AuthenticateChaka(string email, string password, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("AuthenticateChaka", merchant_id);
                if (!check_user_function)
                {
                    return new claims_response
                    {
                        status = 401,
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

                var checkhash = await util.ValidateHash2(email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var request = await util.AuthenticateChakaUser(email, password);
                return request;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpGet]
        public async Task<dynamic> ResetChakaPassword(string email, string newpassword, string otp, string merchant_id, string hash)
        {
            try
            {
                log.Info($"Data:::  {email}, {otp}");
                var check_user_function = await util.CheckForAssignedFunction("ResetChakaPassword", merchant_id);
                if (!check_user_function)
                {
                    return new claims_response
                    {
                        status = 401,
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

                var checkhash = await util.ValidateHash2(email + newpassword, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var request = await util.ChakaResetPassword(email, newpassword, otp);
                return request;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
