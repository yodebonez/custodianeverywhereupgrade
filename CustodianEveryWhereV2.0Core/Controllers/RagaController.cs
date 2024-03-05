using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CustodianEveryWhereV2._0.Controllers
{
    /// <summary>
    /// Post to raga end api
    /// </summary>
 
    [ApiController]
    [Route("api/[controller]")]
    public class RagaController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private Utility util = null;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// 
        /// </summary>
        public RagaController(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        [HttpPost("{ragaRequest?}")]
        public async Task<dynamic> PostToRaga(RagaRequest ragaRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new
                    {
                        status = 308,
                        message = "Some parameters missing from request"
                    };
                }
                //var check_user_function = await util.CheckForAssignedFunction("PostToRaga", ragaRequest.merchant_id);
                //check_user_function = true; // remove before going live
                //if (!check_user_function)
                //{
                //    return new
                //    {
                //        status = 401,
                //        message = "Permission denied from accessing this feature",
                //    };
                //}
                //var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == ragaRequest.merchant_id.Trim());
                //if (config == null)
                //{
                //    log.Info($"Invalid merchant Id {ragaRequest.merchant_id}");
                //    return new
                //    {
                //        status = 402,
                //        message = "Invalid merchant Id"
                //    };
                //}
              
                // Map path for JSON file
                string jsonFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "TravelCategoryJSON", "RagaConfig.json");

                // Read the contents of the file
                string file = System.IO.File.ReadAllText(jsonFilePath);

                var configRaga = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RagaConfig>>(file);
                RagaConfig selectConfig = null;
                int area;
                if (ragaRequest.zone.ToLower().Contains("area"))
                {
                    string newarea = "AREA_1";
                    if (ragaRequest.zone.ToLower().Contains("2"))
                    {
                        newarea = "AREA_2";
                    }
                    selectConfig = configRaga[1];
                    area = selectConfig.zones.FirstOrDefault(x => x.type == newarea).value;
                }
                else
                {
                    selectConfig = configRaga[0];
                    area = selectConfig.zones.FirstOrDefault(x => x.type.ToLower() == ragaRequest.zone.ToLower()).value;
                }
                string dateFormat = "yyyy-mm-dd";
                var pushToRaga = new Raga
                {
                    area = area,
                    convention_id = selectConfig.convention_id,
                    country_destination = ragaRequest.country_destination,
                    country_residence = ragaRequest.country_residence,
                    date_birth = ragaRequest.date_birth.Date.ToString(dateFormat),
                    email = ragaRequest.email,
                    end_date = ragaRequest.end_date.Date.ToString(dateFormat),
                    first_name = ragaRequest.first_name,
                    last_name = ragaRequest.last_name,
                    md5 = selectConfig.hash,
                    nationality = (ragaRequest.nationality.ToLower() == "nigerian") ? "NIGERIAN NIGERIA" : ragaRequest.nationality,
                    num_passport = ragaRequest.num_passport,
                    num_police_ass = DateTime.Now.Date.ToString(dateFormat),
                    password = selectConfig.password,
                    program = selectConfig.program,
                    start_date = ragaRequest.start_date.Date.ToString(dateFormat),
                    user_name = selectConfig.username,
                    num_group = "0"
                };
                var toxml = util.ToXML(pushToRaga);
                log.Info($"Generated xml {toxml}");
              //  StringBuilder ragaSchema = new StringBuilder(System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/RagaSchema.xml")));
                string xmlFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "TravelCategoryJSON", "RagaSchema.xml");
                string xmlContent = System.IO.File.ReadAllText(xmlFilePath);
                StringBuilder ragaSchema = new StringBuilder(xmlContent);
               // XDocument xDocument = XDocument.Parse(xmlContent);
               

                log.Info($"Request to raga {Newtonsoft.Json.JsonConvert.SerializeObject(pushToRaga)}");
                ragaSchema.Replace("#CONTENT#", toxml);
                log.Info($"Full xml request to raga {ragaSchema.ToString()}");
                string RagaUrl = _configuration["AppSettings:RagaUrl"];
                using (var api = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
                    var httpContent = new StringContent(ragaSchema.ToString(), Encoding.UTF8, "application/xml");
                    var request = await api.PostAsync(RagaUrl, httpContent);
                    log.Info($"Status code Raga {request.StatusCode}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new
                        {
                            status = 205,
                            message = "Request was not successful"
                        };
                    }

                    var content = await request.Content.ReadAsStringAsync();
                    log.Info($"response from raga {content}");
                    return new
                    {
                        status = 200,
                        message = "Record was pushed successfully to Raga"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return new
                {
                    status = 209,
                    message = "System malfunction"
                };
            }
        }
    }
}
