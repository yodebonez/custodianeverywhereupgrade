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
    public class GoogleNewsController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<News> news = null;
        private store<AdaptLeads> auth = null;
        private store<PinnedNews> pin = null;
        public GoogleNewsController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            news = new store<News>();
            auth = new store<AdaptLeads>();
            pin = new store<PinnedNews>();
        }
        [HttpGet]
        public async Task<notification_response> GetLatestHeadLines(string merchant_id, string email, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetLatestHeadLines", merchant_id);
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

                var get_news = await news.FindOneByCriteria(x => x.Id == 1);
                if (get_news != null)
                {
                    log.Info($"new was fetched successfully {merchant_id}");
                    var get_pinned_new = await pin.FindMany(x => x.AdaptLeads.email.ToLower() == email.ToLower());
                    if (get_pinned_new != null)
                    {
                        log.Info($"was here");
                        List<dynamic> my_pinned = new List<dynamic>();
                        foreach (var item in get_pinned_new)
                        {
                            if (item.JsonNewsObject != null)
                            {
                                my_pinned.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.JsonNewsObject));
                            }
                        }
                        var original_news = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(get_news.NewsFeed);
                        var remove_pinned = original_news.Where(x => !my_pinned.Any(y => y.Url == x.Url)).ToList();
                        return new notification_response
                        {
                            status = 200,
                            message = "New was fetched successfully from Google",
                            data = remove_pinned
                        };
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 200,
                            message = "New was fetched successfully from Google",
                            data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(get_news.NewsFeed)
                        };
                    }
                }
                else
                {
                    log.Info($"No news {merchant_id}");
                    return new notification_response
                    {
                        status = 407,
                        message = "Can't fetch current news headlines, Try again in few minutes"
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
                    message = "oops!, something happend while get news headlines"
                };
            }
        }
        [HttpPost]
        public async Task<notification_response> PinNews(Pinned_News pinned)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("PinNews", pinned.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == pinned.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {pinned.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                //validate hash
                var checkhash = await util.ValidateHash2(pinned.merchant_id + pinned.email, config.secret_key, pinned.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {pinned.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var user_details = await auth.FindOneByCriteria(x => x.email.ToLower() == pinned.email.ToLower());
                if (user_details == null)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "user has not logged in"
                    };
                }

                var check_if_max_is_reached = await pin.FindMany(x => x.AdaptLeads.email.ToLower() == pinned.email.ToLower());
                if (check_if_max_is_reached.Count() == 10)
                {
                    return new notification_response
                    {
                        status = 406,
                        message = "Max pinned cannot exceed 10"
                    };
                }

                //check if user has click pinned button
                var get_pinned_new = await pin.FindMany(x => x.AdaptLeads.email.ToLower() == pinned.email.ToLower());
                if (get_pinned_new != null && get_pinned_new.Count > 0)
                {
                    for (int i = 0; i <= get_pinned_new.Count() - 1; ++i)
                    {
                        var obj1 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(get_pinned_new[i].JsonNewsObject);
                        if (obj1.Url == pinned.jsonbase64string.Url)
                        {
                            //new already pinned
                            return new notification_response
                            {
                                status = 205,
                                message = "This news has been pinned already"
                            };

                        }
                    }
                }
                var decode_base64 = Newtonsoft.Json.JsonConvert.SerializeObject(pinned.jsonbase64string);
                log.Info($"decoded string {decode_base64}");
                var new_pinned_news = new PinnedNews
                {
                    CreatedAt = DateTime.Now,
                    JsonNewsObject = decode_base64,
                    UserID = user_details.Id
                };
                await pin.Save(new_pinned_news);
                return new notification_response
                {
                    status = 200,
                    message = "News has been pinned"
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
                    message = "oops!, something happend while pinning news"
                };
            }
        }
        [HttpGet]
        public async Task<notification_response> GetPinnedNew(string merchant_id, string email, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPinnedNew", merchant_id);
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
                    log.Info($"Invalid merchant Id {email}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                //validate hash
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {email}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var get_pinned_new = await pin.FindMany(x => x.AdaptLeads.email.ToLower() == email.ToLower());
                if (get_pinned_new == null)
                {

                    log.Info($"You have not pinned any news {email}");
                    return new notification_response
                    {
                        status = 207,
                        message = "You have not pinned any news"
                    };
                }
                var order_news = get_pinned_new.OrderByDescending(x => x.Id).ToList();
                List<NewList> news_colections = new List<NewList>();
                foreach (var item in order_news)
                {
                    if (item.JsonNewsObject != null)
                    {
                        news_colections.Add(new NewList
                        {
                            news = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.JsonNewsObject),
                            Id = item.Id
                        });
                    }

                }

                return new notification_response
                {
                    status = 200,
                    message = "Pinned news lodded successfully",
                    data = news_colections
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
                    message = "oops!, something happend while getting pinned news"
                };
            }
        }
        [HttpGet]
        public async Task<notification_response> DeletePinnedNews(string merchant_id, int Id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("DeletePinnedNews", merchant_id);
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
                    log.Info($"Invalid merchant Id {Id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                //validate hash
                var checkhash = await util.ValidateHash2(merchant_id + Id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {Id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var get_pinned_news = await pin.FindOneByCriteria(x => x.Id == Id);
                if (get_pinned_news == null)
                {
                    return new notification_response
                    {
                        status = 417,
                        message = "This news doesnot exist"
                    };
                }
                var is_deleted = await pin.Delete(get_pinned_news);
                if (is_deleted)
                {
                    return new notification_response
                    {
                        status = 200,
                        message = "Pinned news has been deleted successfully"
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 207,
                        message = "Item deletion failed, Please try again"
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
                    message = "oops!, something happend while deleting pinning news"
                };
            }
        }
    }
}