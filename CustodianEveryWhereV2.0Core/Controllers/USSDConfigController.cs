using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class USSDConfigController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public USSDConfigController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpGet("{merchant_id?}")]
        public async Task<dynamic> GetConfiguration(string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetConfiguration", merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }
                var config = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/productdescription.json"));
                var info = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(config);
                return new
                {
                    status = 200,
                    message = "Configuration fetch was successful",
                    configuration = info
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new { message = "System error, Try Again", status = 404 };
            }
        }
    }
}
