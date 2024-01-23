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
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [ApiController]
    [Route("api/[controller]")]
    public class AddonsController : ControllerBase
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        public AddonsController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();

        }

        [HttpGet("{lat?}/{lon?}/{merchant_id?}")]
        public async Task<res> GetWeatherForecast(string lat, string lon, string merchant_id)
        {
            try
            {
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    Log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetWeatherForecast", merchant_id);
                if (!check_user_function)
                {
                    Log.Info($"Permission denied from accessing this feature for policy search {merchant_id}");
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync($"{GlobalConstant.WeatherBaseURL}?lat={lat}&lon={lon}&appid={GlobalConstant.GetWeatherAPIKey}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new res
                        {
                            message = "Unable to retrieve weather forecast for current location",
                            status = (int)HttpStatusCode.GatewayTimeout
                        };
                    }

                    var response = await request.Content.ReadAsStringAsync();
                    dynamic weather = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);
                    return new res
                    {
                        message = "operation was successfull",
                        status = (int)HttpStatusCode.OK,
                        data = new
                        {
                            weather_data = weather,
                            icon = $"{GlobalConstant.WeatherIconBaseURL}/{weather.weather[0].icon}@2x.png",

                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                Log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}")]
        public async Task<res> GetAdds(string merchant_id)
        {
            try
            {
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    Log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetAdds", merchant_id);
                if (!check_user_function)
                {
                    Log.Info($"Permission denied from accessing this feature for policy search {merchant_id}");
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                string getFile = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/adds.json"));
                var adds = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(getFile);
                return new res
                {
                    status = (int)HttpStatusCode.OK,
                    message = "fetch was successful",
                    data = adds
                };
            }
            catch (Exception ex)
            {

                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                Log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet("{merchant_id?}")]
        public async Task<res> GetDisplayProduct(string merchant_id)
        {
            try
            {
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    Log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetDisplayProduct", merchant_id);
                if (!check_user_function)
                {
                    Log.Info($"Permission denied from accessing this feature for policy search {merchant_id}");
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                string getFile = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/product.json"));
                var adds = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(getFile);
                return new res
                {
                    status = (int)HttpStatusCode.OK,
                    message = "fetch was successful",
                    data = adds
                };
            }
            catch (Exception ex)
            {

                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                Log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }
    }
}
