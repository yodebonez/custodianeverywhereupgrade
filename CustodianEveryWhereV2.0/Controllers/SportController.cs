using CustodianEmailSMSGateway.SMS;
using CustodianEveryWhereV2._0.ActionFilters;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SportController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<News> leagues = null;
        private store<AdaptLeads> auth = null;
        private store<MyPreference> _MyPeference = null;
        public SportController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _MyPeference = new store<MyPreference>();
            leagues = new store<News>();
            auth = new store<AdaptLeads>();
        }
        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> GetAllPreference(string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetAllPreference", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}

                List<League> filtered = null;
                var getAllLeagues = await leagues.FindOneByCriteria(x => x.Id == Config.GetID);
                var MyPreference = await _MyPeference.FindOneByCriteria(x => x.Email?.Trim().ToUpper() == email?.Trim().ToUpper());
                var deserialise_preference = Newtonsoft.Json.JsonConvert.DeserializeObject<List<League>>(getAllLeagues.NewsFeed);
                if (MyPreference != null)
                {
                    filtered = new List<League>();
                    var deserialise_mypreference = Newtonsoft.Json.JsonConvert.DeserializeObject<List<League>>(MyPreference.MyPreferenceInJson);
                    foreach (var preference in deserialise_preference)
                    {
                        foreach (var mypreference in deserialise_mypreference)
                        {
                            if (preference.league_id == mypreference.league_id)
                            {
                                preference.is_my_preference = true;
                                break;
                            }
                        }
                        filtered.Add(preference);
                    }
                }

                return new notification_response
                {
                    status = 200,
                    message = "Preference retrieved successfully",
                    data = new
                    {
                        result = filtered ?? deserialise_preference
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while getting preference"
                };
            }
        }

        [HttpPost]
        [GzipCompression]
        public async Task<notification_response> AddToMyPreference(LeagueObject preference)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    string errorMessage = "";
                    foreach (var item in ModelState?.Values)
                    {
                        foreach (var error in item?.Errors)
                        {
                            errorMessage += error.ErrorMessage + ", ";
                        }
                    }
                    return new notification_response
                    {
                        status = 402,
                        message = errorMessage
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("AddToMyPreference", preference.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == preference.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {preference.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(preference.merchant_id + preference.email, config.secret_key, preference.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {preference.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == preference.email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}

                if (preference.leagues.Count() < 1)
                {
                    return new notification_response
                    {
                        status = 308,
                        message = "Please select your preference"
                    };
                }
                var MyPreference = await _MyPeference.FindOneByCriteria(x => x.Email?.Trim().ToUpper() == preference.email?.Trim().ToUpper());
                if (MyPreference == null)
                {
                    var mypreference = new MyPreference
                    {
                        CreatedAt = DateTime.Now,
                        Email = preference.email,
                        MyPreferenceInJson = Newtonsoft.Json.JsonConvert.SerializeObject(preference.leagues)
                    };
                    await _MyPeference.Save(mypreference);
                    return new notification_response
                    {
                        status = 200,
                        message = "Preference was added successfully"
                    };
                }
                MyPreference.MyPreferenceInJson = Newtonsoft.Json.JsonConvert.SerializeObject(preference.leagues);
                await _MyPeference.Update(MyPreference);
                return new notification_response
                {
                    status = 200,
                    message = "Preference was updated successfully"
                };


            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while getting preference"
                };
            }
        }
        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> GetLeagueFixtures(int league_id, string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetLeagueFixtures", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}


                using (var api = new HttpClient())
                {
                    string url = Config.BASE_URL + $"/fixtures/league/{league_id}";
                    api.DefaultRequestHeaders.Add("X-RapidAPI-Key", Config.Authorization_Header);
                    var request = await api.GetAsync(url);
                    log.Info($"Status: {request.StatusCode} end point {url}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Unable to retrieve fixtures"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<MatchFixtures>();
                    // log.Info($"Match fixtures: => {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                    if (response == null)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "Unable to decode response"
                        };
                    }

                    var current = response.api.fixtures.Where(x => (x.status.Contains("Started") || x.status.Contains("Half")) && x.event_date.Year == DateTime.Now.Year).OrderBy(x => x.event_date).ToList();

                    return new notification_response
                    {
                        status = 200,
                        message = "successful",
                        data = new
                        {
                            current = current
                        }
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
                    message = "oops!, something happend while getting fixtures"
                };
            }
        }
        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> GetMyPreference(string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetMyPreference", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}

                var preference = await _MyPeference.FindOneByCriteria(x => x.Email?.ToLower() == email?.ToLower());
                if (preference == null)
                {
                    return new notification_response
                    {
                        status = 206,
                        message = $"Sorry no preferences found with email '{email}'"
                    };
                }

                var serialise_preference = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(preference.MyPreferenceInJson);
                return new notification_response
                {
                    status = 200,
                    message = "successful",
                    data = new
                    {
                        my_preference = serialise_preference
                    }
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while getting preference"
                };
            }
        }
        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> MatchLineUp(int fixture_id, string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("MatchLineUp", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}


                using (var api = new HttpClient())
                {
                    api.DefaultRequestHeaders.Add("X-RapidAPI-Key", Config.Authorization_Header);
                    var request = await api.GetAsync(Config.BASE_URL + $"/lineups/{fixture_id}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Unable to retrieve Line up"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response == null)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "Unable to decode response"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "successful",
                        data = new
                        {
                            line_up = response,
                        }
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
                    message = "oops!, something happend while getting fixtures"
                };
            }
        }
        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> LiveScore(int league_id, string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("LiveScore", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}


                using (var api = new HttpClient())
                {
                    api.DefaultRequestHeaders.Add("X-RapidAPI-Key", Config.Authorization_Header);
                    var request = await api.GetAsync(Config.BASE_URL + $"/fixtures/live/{league_id}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Unable to retrieve live score"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response == null)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "Unable to decode response"
                        };
                    }

                    if (response.results == 0)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "No live score currently"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "successful",
                        data = new
                        {
                            live_score = response
                        }
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
                    message = "oops!, something happend while getting fixtures"
                };
            }
        }
        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> MatchPrediction(int fixture_id, string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("MatchPrediction", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}


                using (var api = new HttpClient())
                {
                    api.DefaultRequestHeaders.Add("X-RapidAPI-Key", Config.Authorization_Header);
                    var request = await api.GetAsync(Config.BASE_URL + $"/predictions/{fixture_id}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Unable to retrieve prediction"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response == null)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "Unable to decode response"
                        };
                    }

                    if (response.results == 0)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "No prediction"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "successful",
                        data = new
                        {
                            prediction = response
                        }
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
                    message = "oops!, something happend while getting fixtures"
                };
            }
        }

        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> GetLeagueFixturesHistory(int league_id, string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetLeagueFixturesHistory", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}


                using (var api = new HttpClient())
                {
                    string url = Config.BASE_URL + $"/fixtures/league/{league_id}";
                    api.DefaultRequestHeaders.Add("X-RapidAPI-Key", Config.Authorization_Header);
                    var request = await api.GetAsync(url);
                    log.Info($"Status: {request.StatusCode} end point {url}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Unable to retrieve fixtures"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<MatchFixtures>();
                    // log.Info($"Match fixtures: => {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                    if (response == null)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "Unable to decode response"
                        };
                    }

                    var history = response.api.fixtures.Where(x => x.status == "Match Finished" && x.event_date.Year == DateTime.Now.Year).OrderByDescending(x => x.event_date).ToList();

                    return new notification_response
                    {
                        status = 200,
                        message = "successful",
                        data = new
                        {
                            history = history,

                        }
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
                    message = "oops!, something happend while getting fixtures"
                };
            }
        }

        [HttpGet]
        [GzipCompression]
        public async Task<notification_response> GetLeagueTable(int league_id, string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetLeagueTable", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                //if (checkIfUserIsLoginIn == null)
                //{
                //    return new notification_response
                //    {
                //        status = 306,
                //        message = "Please login with your email to use this feature"
                //    };
                //}


                using (var api = new HttpClient())
                {
                    string url = Config.BASE_URL + $"/leagueTable/{league_id}";
                    api.DefaultRequestHeaders.Add("X-RapidAPI-Key", Config.Authorization_Header);
                    var request = await api.GetAsync(url);
                    log.Info($"Status: {request.StatusCode} end point {url}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Unable to retrieve league table"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<dynamic>();

                    if (response == null && response.api.result != 1)
                    {
                        return new notification_response
                        {
                            status = 301,
                            message = "Unable to decode response"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "successful",
                        data = new
                        {
                            table = response,

                        }
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
                    message = "oops!, something happend while getting fixtures"
                };
            }
        }

        //[HttpGet]
        //[GzipCompression]
        //public async Task<notification_response> GetMatchStatistics()
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message);
        //        log.Error(ex.StackTrace);
        //        log.Error(ex.InnerException);
        //        return new notification_response
        //        {
        //            status = 404,
        //            message = "oops!, something happend while getting match statistics"
        //        };
        //    }
        //}

    }
}
