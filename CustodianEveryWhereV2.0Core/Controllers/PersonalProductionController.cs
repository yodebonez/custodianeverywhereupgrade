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
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    public class PersonalProductionController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public PersonalProductionController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpGet("{policy_number?}/{sub?}/{merchant_id?}/{hash?}")]
        public async Task<notification_response> GetPPDetails(string policy_number, subsidiary sub, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPPDetails", merchant_id);
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
                var checkhash = await util.ValidateHash2(policy_number, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var _sub = (sub == subsidiary.General) ? "General" : "Life";
                    var request = api.GetPPTDetails(_sub, policy_number);
                    if (request == null || request.Count() <= 0)
                    {
                        return new notification_response
                        {
                            status = 402,
                            message = "Oops!. No record found"
                        };
                    }
                    return new notification_response
                    {
                        status = 200,
                        message = "record was found",
                        data = request
                    };
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
                    message = "Error: Unable to fetch details from core system: Try Again"
                };
            }
        }
    }
}
