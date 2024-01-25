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
    public class AnnuityController : ControllerBase
    {
        private Utility util = null;
        private static Logger log = LogManager.GetCurrentClassLogger();
        public AnnuityController()
        {
            util = new Utility();
        }
        [HttpGet("{dob?}/{amount?}")]
        public async Task<Annuity> GetAnnuity(DateTime dob, double amount)
        {
            try
            {
                var getdob = await util.ValidateBirthDayForLifeProduct(dob, 50, 99);
                if (getdob.status != 200)
                {
                    return new Annuity
                    {
                        status = getdob.status,
                        message = getdob.message
                    };
                }
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.CreateLifeClient("none", "none", "none", "none", getdob.dateOfBirth, "none", "none@GMAIL.COM", "none");
                    log.Info($"create client request response {request}");
                    if (string.IsNullOrEmpty(request))
                    {
                        log.Info($"Unable to create client");
                        return new Annuity
                        {
                            status = 207,
                            message = "Unable to create client"
                        };
                    }

                    var requestSerialised = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(request);
                    if (request == null)
                    {
                        log.Info($"Unable to decrypt data");
                        return new Annuity
                        {
                            status = 207,
                            message = "Unable to decrypt data"
                        };
                    }

                    string annuity = api.GetAnnuityQuote(Convert.ToInt32(requestSerialised.webTempClntCode), amount.ToString());
                    log.Info($"response from Annuity Quote computation {annuity}");
                    double computed_qoute = 0;
                    var quote = double.TryParse(annuity, out computed_qoute);
                    if (!quote)
                    {
                        return new Annuity
                        {
                            status = 208,
                            message = "Unable to compute quote"
                        };
                    }

                    return new Annuity
                    {
                        status = 200,
                        quote = computed_qoute,
                        message = "Quote computed successfully"
                    };

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new Annuity
                {
                    status = 309,
                    message = "System malfunction"
                };
            }
        }
    }
}
