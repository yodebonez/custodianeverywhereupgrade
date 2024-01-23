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
using DataStore.ExtensionMethods;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class JokesController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<JokesList> jokelist = null;
        private store<WatchedJokes> watched = null;
        private store<AdaptLeads> userlead = null;
        public JokesController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            jokelist = new store<JokesList>();
            watched = new store<WatchedJokes>();
            userlead = new store<AdaptLeads>();
        }

        [HttpGet]
        public async Task<notification_response> GetJokes(string merchant_id, string email, string hash, int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetJokes", merchant_id);
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

                if (page <= 0)
                {
                    return new notification_response
                    {
                        status = 203,
                        message = "page index start at 1"
                    };
                }

                var getallwatched = await watched.FindMany(x => x.AdaptLeads.email.ToLower() == email.ToLower());
                var filter = getallwatched.Select(x => new JokesList
                {
                    Id = x.JokeList.Id,
                    credit = x.JokeList.credit,
                    created = x.JokeList.created,
                    title = x.JokeList.title,
                    youtube_link = x.JokeList.youtube_link
                }).ToList();
                log.Info($"my watched jokes {Newtonsoft.Json.JsonConvert.SerializeObject(filter)}");
                log.Info($"my email {email}");
                var getjokes = (filter.Count > 0) ? await jokelist.FindMany(x => !filter.Any(y => y.Id == x.Id)) : await jokelist.GetAll();
                int pagesize = 10;
                int skip = (page == 1) ? 0 : pagesize * (page - 1);
                decimal total = Convert.ToDecimal(getjokes.Count) / Convert.ToDecimal(pagesize);
                int totalpage = (int)Math.Ceiling(total);
                var sortjoke = getjokes.OrderBy(x => Guid.NewGuid()).Skip(skip).Take(pagesize).Select(y => new joke
                {
                    youtube_url = y.youtube_link,
                    credit = y.credit,
                    thumbnail_image = string.Format("https://img.youtube.com/vi/{0}/0.jpg", y.youtube_link.Split('=')[1].Trim()),
                    title = y.title,
                    JokeId = y.Id
                }).ToList();
                //sortjoke.Shuffle();
                return new notification_response
                {
                    status = 200,
                    data = new Dictionary<string, object>() {
                        { "page",page },
                        { "navigation", $"{page} of {totalpage}" },
                        { "total_pages",totalpage },
                        { "jokelist", sortjoke}
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
                    message = "oops!, something happend while searching for jokes"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetWatchedJokes(string merchant_id, string email, string hash, int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetWatchedJokes", merchant_id);
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
                if (page <= 0)
                {
                    return new notification_response
                    {
                        status = 203,
                        message = "page index start at 1"
                    };
                }
                var getjokes = await watched.FindMany(x => x.AdaptLeads.email == email);
                if (getjokes.Count == 0)
                {
                    return new notification_response
                    {
                        status = 209,
                        message = "You have not watched any joke"
                    };
                }
                int pagesize = 10;
                int skip = (page == 1) ? 0 : pagesize * (page - 1);
                decimal total = Convert.ToDecimal(getjokes.Count) / Convert.ToDecimal(pagesize);
                int totalpage = (int)Math.Ceiling(total);
                var sortjoke = getjokes.OrderByDescending(x => x.Id).Skip(skip).Take(pagesize).Select(y => new joke
                {
                    youtube_url = y.JokeList.youtube_link,
                    credit = y.JokeList.credit,
                    thumbnail_image = string.Format("https://img.youtube.com/vi/{0}/0.jpg", y.JokeList.youtube_link.Split('=')[1].Trim()),
                    title = y.JokeList.title,
                    JokeId = y.JokesID
                }).ToList();

                return new notification_response
                {
                    status = 200,
                    data = new Dictionary<string, object>() {
                        { "page",page },
                        { "navigation", $"{page} of {totalpage}" },
                        { "total_pages",totalpage },
                        { "jokelist", sortjoke}
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
                    message = "oops!, something happend while searching for jokes"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> UpdateToWatched(string merchant_id, string email, int joke_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("UpdateToWatched", merchant_id);
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

                var getuserbyemail = await userlead.FindOneByCriteria(x => x.email.ToLower() == email);
                if (getuserbyemail == null)
                {
                    log.Info($"User not found {email}");
                    return new notification_response
                    {
                        status = 407,
                        message = "User authentication details not found"
                    };
                }

                var hasbeenwatched = await watched.FindOneByCriteria(x => x.JokesID == joke_id && x.UserID == getuserbyemail.Id);
                if (hasbeenwatched != null)
                {
                    log.Info($"Joke has been watched already {email}");
                    return new notification_response
                    {
                        status = 407,
                        message = "This joke has been moved to watched list before"
                    };
                }
                var new_watched = new WatchedJokes
                {
                    CreatedAt = DateTime.Now,
                    JokesID = joke_id,
                    UserID = getuserbyemail.Id
                };

                await watched.Save(new_watched);
                return new notification_response
                {
                    status = 200,
                    message = "Updated to watched"
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
                    message = "oops!, something happend while searching for jokes"
                };
            }
        }
    }
}
