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
    public class PensionController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public PensionController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpGet]
        public async Task<notification_response> GetRSABalanace(string rsa_pin, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetRSABalanace", merchant_id);
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
                var checkhash = await util.ValidateHash2(rsa_pin, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //call service
                log.Info($"About to get RSA for {rsa_pin}");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                var api = new RSA.CalWebServiceImplService();
                var pin = rsa_pin.Trim().ToUpper();
                if (!rsa_pin.Trim().ToUpper().StartsWith("PEN"))
                {
                    rsa_pin = "PEN" + rsa_pin.ToUpper();
                }
                var request = api.GetBenefitInfo(rsa_pin.Trim().ToUpper());

                log.Info($"response from {rsa_pin} data {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
                if (request.t24Connection == "200")
                {
                    return new notification_response
                    {
                        status = 200,
                        message = "RSA pin is valid",
                        data = request
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 402,
                        message = "RSA pin is invalid"
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
                    message = "oops!, something happend while searching for details"
                };
            }
        }
    }
}
