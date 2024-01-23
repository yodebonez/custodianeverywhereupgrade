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
    public class WealthManagementController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        public WealthManagementController()
        {
            util = new Utility();
        }

        [HttpPost]
        public async Task<res> ChakaAuthentication()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }
    }
}
