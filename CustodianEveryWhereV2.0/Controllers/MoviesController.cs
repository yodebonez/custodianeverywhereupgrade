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
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MoviesController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private string base_url = ConfigurationManager.AppSettings["MOVIE_API"];//https://api.themoviedb.org/3
        private string movie_api_key = ConfigurationManager.AppSettings["MOVIE_API_KEY"]; //e26d2c39816a704c232faacd23e1c2cc
        private string image_url = ConfigurationManager.AppSettings["IMAGE_BASE_URL"]; //https://image.tmdb.org/t/p/w500
        public MoviesController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
        }

        [HttpGet]
        public async Task<notification_response> UpComingMovies(string hash, string merchant_id, string region = "", int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("UpComingMovies", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //https://api.themoviedb.org/3/movie/upcoming?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&page=1
                //https://image.tmdb.org/t/p/w500/
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var api = new HttpClient())
                {
                    var url = (string.IsNullOrEmpty(region)) ? base_url + $"/movie/upcoming?api_key={this.movie_api_key}&language=en-US&page={page}" :
                        base_url + $"/movie/upcoming?api_key={this.movie_api_key}&language=en-US&page={page}&region={region}";
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        if (res.results != null)
                        {
                            return new notification_response
                            {
                                message = "Movies loaded successfully",
                                status = 200,
                                data = res,
                                image_base_url = this.image_url

                            };
                        }
                        else
                        {
                            return new notification_response { message = $"No upcoming found within date range from: {res.dates.minimum} to {res.dates.maximum}", status = 401 };
                        }
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while searching for upcoming movies", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetMovieDetails(string hash, string merchant_id, int movie_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetMovieDetails", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var api = new HttpClient())
                {
                    //https://api.themoviedb.org/3/movie/287947?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&append_to_response=videos
                    var url = base_url + $"/movie/{movie_id}?api_key={this.movie_api_key}&language=en-US&append_to_response=videos";
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        return new notification_response
                        {
                            message = "Movies loaded successfully",
                            status = 200,
                            data = res,
                            image_base_url = this.image_url
                        };
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while searching for upcoming movies", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetSimilarMovies(string hash, string merchant_id, int movie_id, int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetSimilarMovies", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //https://api.themoviedb.org/3/movie/299534/similar?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&page=1
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var api = new HttpClient())
                {
                    var url = base_url + $"/movie/{movie_id}/similar?api_key={this.movie_api_key}&language=en-US&page={page}";
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        if (res.results != null)
                        {
                            return new notification_response
                            {
                                message = "Movies loaded successfully",
                                status = 200,
                                data = res,
                                image_base_url = this.image_url
                            };
                        }
                        else
                        {
                            return new notification_response { message = $"No upcoming found within date range from: {res.dates.minimum} to {res.dates.maximum}", status = 401 };
                        }
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while searching for similar movies", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetAllReleasedMovies(string hash, string merchant_id, string region = "", int page = 1)
        {

            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetAllReleasedMovies", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //https://api.themoviedb.org/3/discover/movie?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&region=NG&sort_by=release_date.desc&include_adult=false&include_video=true&page=1
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var api = new HttpClient())
                {
                    string _region = string.Empty;
                    if (!string.IsNullOrEmpty(region))
                    {
                        _region = $"&region={region}&sort_by=release_date.desc";
                    }
                    var url = base_url + $"/discover/movie?api_key={movie_api_key}&language=en-US&include_adult=false&include_video=true&page={page}" + _region.Trim();
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        if (res.results != null)
                        {
                            return new notification_response
                            {
                                message = "Movies loaded successfully",
                                status = 200,
                                data = res,
                                image_base_url = this.image_url
                            };
                        }
                        else
                        {
                            return new notification_response { message = $"No movie found", status = 401 };
                        }
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while retrieving movies", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> SearchMovies(string hash, string merchant_id, string query)
        {
            try
            {
                //https://api.themoviedb.org/3/search/movie?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&query=avenger&page=1&include_adult=false

                var check_user_function = await util.CheckForAssignedFunction("SearchMovies", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var api = new HttpClient())
                {
                    //https://api.themoviedb.org/3/search/movie?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&query=avenger&page=1&include_adult=false
                    var url = base_url + $"/search/movie?api_key={movie_api_key}&language=en-US&query={query}&page=1&include_adult=false";
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        if (res.results != null)
                        {
                            return new notification_response
                            {
                                message = "Movies loaded successfully",
                                status = 200,
                                data = res,
                                image_base_url = this.image_url
                            };
                        }
                        else
                        {
                            return new notification_response { message = $"No movie found", status = 401 };
                        }
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while searching movies", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> NowPlaying(string hash, string merchant_id, string region = "", int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("NowPlaying", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var api = new HttpClient())
                {
                    string _region = string.Empty;
                    if (!string.IsNullOrEmpty(region))
                    {
                        _region = $"&region={region}";
                    }
                    //https://api.themoviedb.org/3/movie/now_playing?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&page=1
                    var url = base_url + $"/movie/now_playing?api_key={movie_api_key}&language=en-US&page={page}" + _region.Trim();
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        if (res.results != null)
                        {
                            return new notification_response
                            {
                                message = "Movies loaded successfully",
                                status = 200,
                                data = res,
                                image_base_url = this.image_url
                            };
                        }
                        else
                        {
                            return new notification_response { message = $"No movie found", status = 401 };
                        }
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while searching movies", status = 404 };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetTVSeries(string merchant_id, string hash, int page = 1)
        {
            try
            {
                //https://api.themoviedb.org/3/tv/popular?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&page=1
                var check_user_function = await util.CheckForAssignedFunction("GetTVSeries", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var api = new HttpClient())
                {

                    //https://api.themoviedb.org/3/movie/now_playing?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&page=1
                    var url = base_url + $"/tv/popular?api_key={this.movie_api_key}&language=en-US&page={page}";
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        if (res.results != null)
                        {
                            return new notification_response
                            {
                                message = "Movies loaded successfully",
                                status = 200,
                                data = res,
                                image_base_url = this.image_url
                            };
                        }
                        else
                        {
                            return new notification_response { message = $"No movie found", status = 401 };
                        }
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet]
        public async Task<notification_response> GetTVSeriesDetails(string merchant_id, string hash, int movie_series_id, int page = 1)
        {
            try
            {
                //https://api.themoviedb.org/3/tv/popular?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&page=1
                var check_user_function = await util.CheckForAssignedFunction("GetTVSeriesDetails", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var api = new HttpClient())
                {

                    //https://api.themoviedb.org/3/tv/1399?api_key=e26d2c39816a704c232faacd23e1c2cc&language=en-US&append_to_response=videos
                    var url = base_url + $"/tv/{movie_series_id}?api_key={this.movie_api_key}&language=en-US&append_to_response=videos";
                    var req = await api.GetAsync(url);
                    if (req.IsSuccessStatusCode)
                    {
                        var res = await req.Content.ReadAsAsync<dynamic>();
                        if (res != null)
                        {
                            return new notification_response
                            {
                                message = "Movies loaded successfully",
                                status = 200,
                                data = res,
                                image_base_url = this.image_url
                            };
                        }
                        else
                        {
                            return new notification_response { message = $"No movie found", status = 401 };
                        }
                    }
                    else
                    {
                        return new notification_response { message = "Unable to connect to movies store", status = 402 };
                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
