using DapperLayer.Dapper.Core;
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
    public class BrokerPortalController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private Core<dynamic> dapper_core = null;
        public BrokerPortalController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            dapper_core = new Core<dynamic>();
        }

        [HttpGet]
        public async Task<dynamic> GetConfig(string token, string merchant_id)
        {
            try
            {
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                var request = await dapper_core.GetConfigTravel(token);
                if (request == null)
                {
                    return new notification_response
                    {
                        status = 309,
                        message = "Invalid merchant key"
                    };
                }
                return new notification_response
                {
                    data = request,
                    message = "Operation successful",
                    status = 200
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return new notification_response
                {
                    status = 400,
                    message = "System error"
                };
            }
        }
    }
}
