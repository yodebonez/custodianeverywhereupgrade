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
    public class EnquiryController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public EnquiryController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpPost("{policy?}")]
        public async Task<Policy> LifePolicy(Enquiry policy)
        {
            try
            {
                log.Info("about to validate request params for LifePolicy()");
                log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(policy));
                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            log.Error(error.ErrorMessage);
                        }
                    }
                    return new Policy
                    {
                        status = 401,
                        message = "Missing parameters from request"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("LifePolicy", policy.merchant_id);
                if (!check_user_function)
                {
                    return new Policy
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, "Life", policy.policy_no);
                    if (string.IsNullOrEmpty(request.InsState) && request.InsState.ToLower().Contains("wrong"))
                    {
                        return new Policy
                        {
                            status = 302,
                            message = "Invalid policy number"
                        };
                    }
                    else
                    {
                        return new Policy
                        {
                            status = 200,
                            message = "policy number is valid",
                            details = request
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new Policy
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost("{policy?}")]
        public async Task<Policy> NonLifePolicy(Enquiry policy)
        {
            try
            {
                log.Info("about to validate request params for NonLifePolicy()");
                log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(policy));
                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            log.Error(error.ErrorMessage);
                        }
                    }
                    return new Policy
                    {
                        status = 401,
                        message = "Missing parameters from request"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("LifePolicy", policy.merchant_id);
                if (!check_user_function)
                {
                    return new Policy
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, "General", policy.policy_no);
                    if (string.IsNullOrEmpty(request.InsState) && request.InsState.ToLower().Contains("wrong"))
                    {
                        return new Policy
                        {
                            status = 302,
                            message = "Invalid policy number"
                        };
                    }
                    else
                    {
                        return new Policy
                        {
                            status = 200,
                            message = "policy number is valid",
                            details = request
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new Policy
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }
    }
}
