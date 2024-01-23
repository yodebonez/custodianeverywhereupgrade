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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [ApiController]
    [Route("api/[controller]")]
    public class AirPortSearchController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<FlightAndAirPortData> _search = null;
        public AirPortSearchController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
            _search = new store<FlightAndAirPortData>();
        }

        [HttpGet("{query?}/{merchant_id?}")]
        public async Task<FlightSearch> FlightSearch(string query, string merchant_id)
        {
            log.Info("about to validate request params for Search()");
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("FlightSearch", merchant_id);
                if (!check_user_function)
                {
                    return new FlightSearch
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var search_flight = await _search.CreateQuery($@"SELECT * FROM FlightAndAirPortData WHERE AirportCode LIKE '%{query}%' OR AirportName lIKE '%{query}%' OR  City LIKE '%{query}%' OR Country LIKE '%{query}%' OR CityCountry LIKE '%{query}%'");
                if (search_flight == null || search_flight.Count == 0)
                {
                    return new FlightSearch
                    {
                        status = 402,
                        message = $"No result found for this search '{query}'"
                    };
                }

                var data = search_flight.Select(x => new FlightData
                {
                    AirportCode = x.AirportCode,
                    AirportName = x.AirportName,
                    City = x.City,
                    CityCountry = x.CityCountry,
                    Country = x.Country
                }).ToList();
                return new FlightSearch
                {
                    status = 200,
                    message = "Query was successful with results",
                    flight_search = data
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new FlightSearch
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }
    }
}
