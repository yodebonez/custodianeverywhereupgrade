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
    public class MealPlanController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private store<MyMealPlan> myMealPlan = null;
        private store<MealPlan> MealPlan = null;
        public MealPlanController()
        {
            myMealPlan = new store<MyMealPlan>();
            MealPlan = new store<MealPlan>();
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meal"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<notification_response> CreateMealPlanNew(_MealPlan meal)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("CreateMealPlan", meal.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == meal.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {meal.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(meal.givenBirth.ToString() + meal.gender + meal.maritalStatus.ToString(), config.secret_key, meal.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {meal.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var check_if_meal_plan_exist = await myMealPlan.FindOneByCriteria(x => x.email.ToUpper().Trim() == meal.email.ToUpper().Trim() && x.IsCancelled == false);
                if (check_if_meal_plan_exist != null)
                {
                    return new notification_response
                    {
                        status = 407,
                        message = $"Meal plan already exist for email '{meal.email}'"
                    };
                }
                List<MealPlan> getMeal = null;
                getMeal = await MealPlan.FindMany(x => x.target == meal.target.ToString() && x.preference == meal.preference.ToString());
                if (getMeal != null && getMeal.Count > 0)
                {
                    //var group_plan = getMeal.GroupBy(x => x.daysOfWeek);
                    // dic = new Dictionary<string, Dictionary<string, List<temp>>>();
                    var final = new List<object>();
                    var temphistory = new List<Dictionary<string, object>>();
                    var dicWeek = new List<Dictionary<string, Dictionary<string, List<temp>>>>();

                    #region old code
                    //var list_meal = new List<temp>();
                    //var meallist = item.GroupBy(x => x.mealType);
                    //var day = new Dictionary<string, List<temp>>();
                    //foreach (var subitem in meallist)
                    //{
                    //    day.Add(subitem.First().mealType, subitem.Select(x => new temp
                    //    {
                    //        food = x.food,
                    //        quantity = x.quantity,
                    //        time = x.time,
                    //        youtubeurl = x.youTubeUrl,
                    //        image_path = $"{ConfigurationManager.AppSettings["MEAL_PLAN_IMAGES"]}/{x.image}",
                    //    }).ToList());
                    //}

                    //dic.Add(item.First().daysOfWeek.Trim(), day);
                    // final.Add(dic);
                    //var final = new List<object>();
                    // var mealhistory = new List<List<Dictionary<string, object>>>();

                    //var groupByWeek = item_in_my_meal.SelectedMealPlan.GroupBy(x=>x.)
                    #endregion

                    var groupByWeek = getMeal.GroupBy(x => x.mealPlanCategory);
                    foreach (var itemWeek in groupByWeek)
                    {
                        var dic = new Dictionary<string, Dictionary<string, List<temp>>>();
                        var group_plan = itemWeek.GroupBy(x => x.daysOfWeek);
                        foreach (var item in group_plan)
                        {
                            var list_meal = new List<temp>();
                            var meallist = item.GroupBy(x => x.mealType);
                            var day = new Dictionary<string, List<temp>>();
                            foreach (var subitem in meallist)
                            {
                                day.Add(subitem.First().mealType, subitem.Select(x => new temp
                                {
                                    food = x.food,
                                    quantity = x.quantity,
                                    time = x.time,
                                    youtubeurl = x.youTubeUrl,
                                    image_path = $"{ConfigurationManager.AppSettings["MEAL_PLAN_IMAGES"]}/{x.image}"
                                }).ToList());
                            }
                            dic.Add(item.First().daysOfWeek.Trim(), day);
                        }
                        dicWeek.Add(dic);
                    }
                    var newMyMeal = new MyMealPlan
                    {
                        ageRange = meal.ageRange,
                        email = meal.email.ToLower(),
                        gender = meal.gender,
                        givenBirth = meal.givenBirth,
                        maritalStatus = meal.maritalStatus,
                        phoneNumber = meal.phoneNumber,
                        preference = meal.preference,
                        target = meal.target,
                        datecreated = DateTime.Now,
                        IsCancelled = false,
                        SelectedMealPlan = getMeal.Select(x => new SelectedMealPlan
                        {
                            dateCreated = DateTime.Now,
                            mealPlanID = x.Id
                        }).ToList()
                    };

                    await myMealPlan.Save(newMyMeal);

                    return new notification_response
                    {
                        data = dicWeek,
                        message = "Meal created successfully",
                        status = 200
                    };
                }
                else
                {
                    return new notification_response { message = "Unable to create meal plan", status = 409 };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while creating meal plan", status = 404 };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailOrphone"></param>
        /// <param name="merchant_id"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<notification_response> GetMyMealPlanNew(string emailOrphone, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetMyMealPlan", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
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
                var checkhash = await util.ValidateHash2(emailOrphone, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var getMyMeal = await myMealPlan.FindOneByCriteria(x => x.email.ToUpper().Trim() == emailOrphone.ToUpper().Trim() && x.IsCancelled == false);
                if (getMyMeal != null)
                {
                    var final = new List<object>();
                    // var mealhistory = new List<List<Dictionary<string, object>>>();
                    var temphistory = new List<Dictionary<string, object>>();
                    var dicWeek = new List<Dictionary<string, Dictionary<string, List<temp>>>>();
                    //var groupByWeek = item_in_my_meal.SelectedMealPlan.GroupBy(x=>x.)
                    var groupByWeek = getMyMeal.SelectedMealPlan.GroupBy(x => x.MealPlan.mealPlanCategory);
                    foreach (var itemWeek in groupByWeek)
                    {
                        var dic = new Dictionary<string, Dictionary<string, List<temp>>>();
                        var group_plan = itemWeek.GroupBy(x => x.MealPlan.daysOfWeek);
                        foreach (var item in group_plan)
                        {
                            var list_meal = new List<temp>();
                            var meallist = item.GroupBy(x => x.MealPlan.mealType);
                            var day = new Dictionary<string, List<temp>>();
                            foreach (var subitem in meallist)
                            {
                                day.Add(subitem.First().MealPlan.mealType, subitem.Select(x => new temp
                                {
                                    food = x.MealPlan.food,
                                    quantity = x.MealPlan.quantity,
                                    time = x.MealPlan.time,
                                    youtubeurl = x.MealPlan.youTubeUrl,
                                    image_path = $"{ConfigurationManager.AppSettings["MEAL_PLAN_IMAGES"]}/{x.MealPlan.image}"
                                }).ToList());
                            }
                            dic.Add(item.First().MealPlan.daysOfWeek.Trim(), day);
                        }
                        dicWeek.Add(dic);
                    }

                    temphistory.Add(new Dictionary<string, object>
                        {
                            { "preference", getMyMeal.preference.ToString() },
                            { "target", getMyMeal.target.ToString() },
                            { "emailorphone", getMyMeal.email.ToString() },
                            { "datecreated", (getMyMeal.datecreated != null)?getMyMeal.datecreated.ToShortDateString(): null },
                            { "mealhistory",dicWeek }
                        });


                    // mealhistory.Add(temphistory);
                    return new notification_response
                    {
                        message = "Meal hsitory loaded successfully",
                        status = 200,
                        data = temphistory
                    };
                }
                else
                {
                    return new notification_response { message = "No meal plan found", status = 403 };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while loading your meal plan", status = 404 };
            }
        }
        /// <summary>
        ///  Cancel meal plan
        /// </summary>
        /// <param name="emailOrphone"></param>
        /// <param name="merchant_id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<notification_response> CancelMealPlan(string emailOrphone, string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("CancelMealPlan", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
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

                var _myMealPlan = await myMealPlan.FindOneByCriteria(x => x.email?.ToLower().Trim() == emailOrphone?.ToLower().Trim() && x.IsCancelled == false);
                if (_myMealPlan == null)
                {
                    return new notification_response
                    {
                        status = 405,
                        message = "User email not tied to any meal plan"
                    };
                }

                _myMealPlan.IsCancelled = true;
                _myMealPlan.CancelledDate = DateTime.Now;
                await myMealPlan.Update(_myMealPlan);
                return new notification_response
                {
                    status = 200,
                    message = "You've cancelled your meal plan"
                };
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return new notification_response { message = "Something happend while deleting your meal plan", status = 404 };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meal"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<notification_response> CreateMealPlan(_MealPlan meal)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("CreateMealPlan", meal.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == meal.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {meal.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(meal.givenBirth.ToString() + meal.gender + meal.maritalStatus.ToString(), config.secret_key, meal.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {meal.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getMeal = await MealPlan.FindMany(x => x.target == meal.target.ToString() && x.preference == meal.preference.ToString());
                if (getMeal != null && getMeal.Count > 0)
                {
                    var group_plan = getMeal.GroupBy(x => x.daysOfWeek);
                    var dic = new Dictionary<string, Dictionary<string, List<temp>>>();
                    var final = new List<object>();
                    foreach (var item in group_plan)
                    {

                        var list_meal = new List<temp>();
                        var meallist = item.GroupBy(x => x.mealType);
                        var day = new Dictionary<string, List<temp>>();
                        foreach (var subitem in meallist)
                        {
                            day.Add(subitem.First().mealType, subitem.Select(x => new temp
                            {
                                food = x.food,
                                quantity = x.quantity,
                                time = x.time,
                                youtubeurl = x.youTubeUrl,
                                image_path = $"{ConfigurationManager.AppSettings["MEAL_PLAN_IMAGES"]}/{x.image}",
                            }).ToList());
                        }

                        dic.Add(item.First().daysOfWeek.Trim(), day);
                        // final.Add(dic);
                    }

                    var newMyMeal = new MyMealPlan
                    {
                        ageRange = meal.ageRange,
                        email = meal.email,
                        gender = meal.gender,
                        givenBirth = meal.givenBirth,
                        maritalStatus = meal.maritalStatus,
                        phoneNumber = meal.phoneNumber,
                        preference = meal.preference,
                        target = meal.target,
                        datecreated = DateTime.Now,
                        SelectedMealPlan = getMeal.Select(x => new SelectedMealPlan
                        {
                            dateCreated = DateTime.Now,
                            mealPlanID = x.Id
                        }).ToList()
                    };

                    await myMealPlan.Save(newMyMeal);

                    return new notification_response
                    {
                        data = dic,
                        message = "Meal created successfully",
                        status = 200
                    };
                }
                else
                {
                    return new notification_response { message = "Unable to create meal plan", status = 409 };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while creating meal plan", status = 404 };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailOrphone"></param>
        /// <param name="merchant_id"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<notification_response> GetMyMealPlan(string emailOrphone, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetMyMealPlan", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
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
                var checkhash = await util.ValidateHash2(emailOrphone, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var getMyMeal = await myMealPlan.FindMany(x => x.email.ToUpper().Trim() == emailOrphone.ToUpper().Trim());
                if (getMyMeal != null && getMyMeal.Count > 0)
                {
                    var final = new List<object>();
                    var mealhistory = new List<List<Dictionary<string, object>>>();
                    var temphistory = new List<Dictionary<string, object>>();
                    foreach (var item_in_my_meal in getMyMeal.OrderByDescending(x => x.Id))
                    {
                        var group_plan = item_in_my_meal.SelectedMealPlan.GroupBy(x => x.MealPlan.daysOfWeek);
                        var dic = new Dictionary<string, Dictionary<string, List<temp>>>();
                        foreach (var item in group_plan)
                        {
                            var list_meal = new List<temp>();
                            var meallist = item.GroupBy(x => x.MealPlan.mealType);
                            var day = new Dictionary<string, List<temp>>();
                            foreach (var subitem in meallist)
                            {
                                day.Add(subitem.First().MealPlan.mealType, subitem.Select(x => new temp
                                {
                                    food = x.MealPlan.food,
                                    quantity = x.MealPlan.quantity,
                                    time = x.MealPlan.time,
                                    youtubeurl = x.MealPlan.youTubeUrl,
                                    image_path = $"{ConfigurationManager.AppSettings["MEAL_PLAN_IMAGES"]}/{x.MealPlan.image}"
                                }).ToList());
                            }
                            dic.Add(item.First().MealPlan.daysOfWeek.Trim(), day);
                            //final.Add(dic);
                        }
                        temphistory.Add(new Dictionary<string, object>
                        {
                            { "preference", item_in_my_meal.preference.ToString() },
                            { "target", item_in_my_meal.target.ToString() },
                            { "emailorphone", item_in_my_meal.email.ToString() },
                            { "datecreated", (item_in_my_meal.datecreated != null)?item_in_my_meal.datecreated.ToShortDateString(): null },
                            { "mealhistory",dic }
                        });

                    }
                    // mealhistory.Add(temphistory);
                    return new notification_response
                    {
                        message = "Meal hsitory loaded successfully",
                        status = 200,
                        data = temphistory
                    };
                }
                else
                {
                    return new notification_response { message = "No meal plan found", status = 403 };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response { message = "Something happend while loading your meal plan", status = 404 };
            }
        }
    }
}
