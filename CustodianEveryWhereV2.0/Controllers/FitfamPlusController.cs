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
using System.Net.Http.Formatting;
using System.Text;
using CustodianEmailSMSGateway.Email;
using System.IO;
using DataStore.ExtensionMethods;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FitfamPlusController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<FitfamplusDeals> _deals = null;
        private store<ListOfGyms> _gymList = null;
        public FitfamPlusController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _deals = new store<FitfamplusDeals>();
            _gymList = new store<ListOfGyms>();
        }

        [HttpGet]
        public async Task<Wallet> WelletBalance()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new Wallet
                {
                    status = 404,
                    message = "oops!, something happend while getting your balance"
                };
            }
        }

        /// <summary>
        /// Get deals from Fitfam plus api
        /// </summary>
        /// <param name="merchant_id"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<notification_response> GetDeals(string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetDeals", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                var endpoint = ConfigurationManager.AppSettings["ENDPOINTS"].Split('|');
                Dictionary<string, List<dynamic>> deals = new Dictionary<string, List<dynamic>>();
                List<dynamic> dy_deals = new List<dynamic>();
                using (var api = new HttpClient())
                {
                    foreach (var item in endpoint)
                    {
                        try
                        {
                            var request = await api.GetAsync(item + "deals");
                            if (request.IsSuccessStatusCode)
                            {
                                var response = await request.Content.ReadAsAsync<dynamic>();
                                if (response.status == "success")
                                {
                                    foreach (var subitems in response.deals)
                                    {
                                        dy_deals.Add(subitems);
                                    }
                                }
                            }
                            // dy_deals.sh
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message);
                            log.Error(ex.StackTrace);
                            log.Error(ex.InnerException);
                        }
                    }
                }

                if (dy_deals != null && dy_deals.Count > 0)
                {
                    var ordered_list = dy_deals.OrderByDescending(x => Convert.ToDecimal(x.discounted_pric)).ToList();
                    var addHttpstTourl = ordered_list.Select(x => new
                    {
                        package_id = x.package_id,
                        package = x.package,
                        price = x.price,
                        status = x.status,
                        discounted_price = x.discounted_price,
                        discounted_percent = x.discounted_percent,
                        description = x.description,
                        duration = x.duration,
                        gym = x.gym,
                        image_url = ($"{x.image_url}".StartsWith("http")) ? $"{x.image_url}".Replace("http", "https") : x.image_url,
                        membership_id = x.membership_id,
                        period = x.period
                    }).ToList<dynamic>();
                    deals.Add("deals", addHttpstTourl);
                    return new notification_response
                    {
                        status = 200,
                        message = "deals loaded sucessfully",
                        data = deals
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 205,
                        message = "No deal avaliable yet",
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
                    message = "oops!, something happend while getting deals"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> CheckIn(string phone, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("CheckIn", merchant_id);
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
                var checkhash = await util.ValidateHash2(phone, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var endpoint = ConfigurationManager.AppSettings["ENDPOINTS"].Split('|');
                //var geturl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/config.json")));
                Dictionary<string, List<Dictionary<string, dynamic>>> deals = new Dictionary<string, List<Dictionary<string, dynamic>>>();
                List<dynamic> dy_deals = new List<dynamic>();
                using (var api = new HttpClient())
                {
                    foreach (var item in endpoint)
                    {
                        try
                        {
                            var request = await api.GetAsync(item + $"customer/info/{phone}");
                            if (request.IsSuccessStatusCode)
                            {
                                var response = await request.Content.ReadAsAsync<dynamic>();
                                if (response.status == "success")
                                {
                                    dy_deals.Add(response.member);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message);
                            log.Error(ex.StackTrace);
                            log.Error(ex.InnerException);
                        }
                    }
                }

                if (dy_deals != null && dy_deals.Count > 0)
                {
                    var ordered_list = dy_deals.OrderByDescending(x => Convert.ToDecimal(x.discounted_pric)).ToList();
                    var addHttpstTourl = ordered_list.Select(x => new Dictionary<string, dynamic>
                    {
                        { "id",x.id },
                        { "firstname", x.firstname },
                        { "lastname",x.lastname },
                        { "dob",x.dob },
                        { "gender", x.gender },
                        { "email",x.email },
                        { "address", x.address },
                        { "mobile",x.mobile },
                        { "joindate",x.joindate },
                        { "image_url" , ($"{x.image_url}".StartsWith("http")) ? $"{x.image_url}".Replace("http", "https") : x.image_url },
                        { "weight", x.weight },
                        { "height", x.height },
                        { "maritalstatus",  x.maritalstatus },
                        { "anniversary", x.anniversary },
                        { "gym" , x.gym },
                        { "memberships", x.memberships },
                        { "check-in", x["check-in"] }
                    }).ToList();
                    deals.Add("membership", addHttpstTourl);
                    return new notification_response
                    {
                        status = 200,
                        message = "details loaded sucessfully",
                        data = deals
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 205,
                        message = "User does not exist",
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
                    message = "oops!, something happend while searching for details"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> BuyDeal(deal deal)
        {
            log.Info($"raw data fitfam {Newtonsoft.Json.JsonConvert.SerializeObject(deal)}");
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Some parameters are missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("BuyDeal", deal.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == deal.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {deal.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(deal.email + deal.reference + deal.gym, config.secret_key, deal.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {deal.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var checkme = await _deals.FindOneByCriteria(x => x.reference.ToLower() == deal.reference.ToLower());
                if (checkme != null)
                {
                    log.Info($"duplicate request {deal.merchant_id}");
                    return new notification_response
                    {
                        status = 300,
                        message = "Duplicate request"
                    };
                }

                DateTime versary;
                DateTime expiry;
                var newdeal = new FitfamplusDeals
                {
                    address = deal.address,
                    anniversary = DateTime.Now,
                    discount_percent = deal.discounted_percent,
                    discounted_price = deal.discounted_price,
                    dob = (!string.IsNullOrEmpty(deal.dob) && DateTime.TryParse(deal.dob, out versary)) ? versary.Date : default(DateTime).Date,
                    email = deal.email,
                    firstname = deal.firstname,
                    gender = deal.gender,
                    gym = deal.gym,
                    lastname = deal.lastname,
                    marital_status = deal.marital_status,
                    membership_id = deal.membership,
                    mobile = deal.mobile,
                    package_id = deal.package_id,
                    price = deal.price,
                    reference = deal.reference,
                    deal_description = deal.description,
                    transactionDate = DateTime.Now,
                    start_date = (!string.IsNullOrEmpty(deal.start_date) && DateTime.TryParse(deal.start_date, out versary)) ? versary.Date : default(DateTime).Date
                };
                if (deal.period.ToLower() == "day" || deal.period.ToLower() == "week")
                {
                    expiry = newdeal.start_date.AddDays(deal.duration);
                }
                else
                {
                    expiry = newdeal.start_date.AddMonths(deal.duration);
                }
                newdeal.end_date = expiry;
                log.Info($"Date time {DateTime.Now}");
                log.Info($"Fitfam adapt deals request2:  {Newtonsoft.Json.JsonConvert.SerializeObject(newdeal)}");
                var geturl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/config.json"))).FirstOrDefault(x => x.gymname == deal.gym.ToUpper().Trim());
                if (geturl != null)
                {
                    using (var _api = new HttpClient())
                    {
                        string url = geturl.url;
                        log.Info($"endpoint to push {url}");
                        var request = await _api.PostAsJsonAsync(url, newdeal);
                        log.Info($"response code for fitfam {request.StatusCode}");
                        if (request.IsSuccessStatusCode)
                        {

                            var response = await request.Content.ReadAsAsync<dynamic>();
                            log.Info($"response from fitfam {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                            if (response != null && response.status != "fail")
                            {
                                await _deals.Save(newdeal);
                                //send mail
                                string messageBody = $@"Dear {newdeal.firstname + " " + newdeal.lastname }<br/><br/>Congratulations on your purchase on Adapt. Please find summary below: <br/><br/>
                                                <table border = '1' width='100%' style = 'border-collapse: collapse;' >
                                                <tr><td style = 'font-weight:bold'>Gym Name</td><td>{geturl.fullname}</td></tr>
                                                <tr><td style = 'font-weight:bold'>Deal</td><td>{newdeal.deal_description}</td></tr>
                                                <tr><td style = 'font-weight:bold'>Price</td><td>NGN {string.Format("{0:N}", newdeal.price)}</td></tr>
                                                <tr><td style = 'font-weight:bold'>Start Date</td><td>{newdeal.start_date.ToString("dddd, dd MMMM yyyy")}</td></tr>
                                                <tr><td style = 'font-weight:bold'>End Date</td><td>{newdeal.end_date.ToString("dddd, dd MMMM yyyy")}</td></tr>
                                                <tr><td style = 'font-weight:bold'>Transction Date</td><td>{newdeal.transactionDate.Value.ToString("dddd, dd MMMM yyyy")}</td></tr>
                                                <tr><td style = 'font-weight:bold'>Payment Reference</td><td>{newdeal.reference}</td></tr>
                                                </table>";
                                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                                StringBuilder sb = new StringBuilder(template);
                                sb.Replace("#CONTENT#", messageBody);
                                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                                var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                                Task.Factory.StartNew(() =>
                                {
                                    List<string> cc = new List<string>();
                                    //cc.Add("technology@custodianplc.com.ng");
                                    new SendEmail().Send_Email(deal.email, "Adapt-Deal", sb.ToString(), "Adapt-Deal", true, imagepath, null, null, null);
                                });

                                log.Info($"Gym processing to api success");
                                return new notification_response
                                {
                                    status = 200,
                                    message = "Transaction was successful"
                                };
                            }
                            else
                            {
                                log.Info($"Gym processing to api failed");
                                return new notification_response
                                {
                                    status = 405,
                                    message = "Unable to push deal to gym. Try Agin"
                                };
                            }
                        }
                        else
                        {
                            log.Info($"Gym processing to api failed");
                            return new notification_response
                            {
                                status = 402,
                                message = "Unable to push deal to gym. Try Again"
                            };
                        }
                    }
                }
                else
                {
                    log.Info($"Invalid config settings no config found on json config file");
                    return new notification_response
                    {
                        status = 407,
                        message = "Incorrect gym name"
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
                    message = "oops!, something happend while searching for details"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetListOfGyms(string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetListOfGyms", merchant_id);
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

                var gymList = await _gymList.GetAll();

                return new notification_response
                {
                    status = 200,
                    message = "Gyms loaded successfully",
                    data = gymList.Select(x => new
                    {
                        gymName = x.GymName,
                        Id = x.Id
                    }).ToList()
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
                    message = "oops!, something happend while searching for details"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> GymLogin(GymLogin gymLogin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Some parameters are missing from request",
                    };
                }
                var check_user_function = await util.CheckForAssignedFunction("GymLogin", gymLogin.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }


                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == gymLogin.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {gymLogin.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(gymLogin.password + gymLogin.username + gymLogin.gym_id, config.secret_key, gymLogin.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {gymLogin.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getGym = await _gymList.FindOneByCriteria(x => x.Id == gymLogin.gym_id);
                if (getGym == null)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "Requested gym does not exist"
                    };
                }

                using (var api = new HttpClient())
                {
                    api.BaseAddress = new Uri(getGym.LoginEndPoint?.Trim());
                    var request = await api.PostAsJsonAsync<dynamic>("", new
                    {
                        Email = gymLogin.username,
                        Password = gymLogin.password
                    });

                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 403,
                            message = "Authentication failed, third-part application not responding"
                        };
                    }

                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response == null && response.status != "success")
                    {
                        return new notification_response
                        {
                            status = 208,
                            message = "Authentication Failed -- Invalid Username or Password"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "Authentication successful"
                    };

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
                    message = "oops!, something happend while authenticating user"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> MarkAttendance(MarkAttendance markAttendance)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Some parameters are missing from request",
                    };
                }
                var check_user_function = await util.CheckForAssignedFunction("MarkAttendance", markAttendance.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }


                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == markAttendance.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {markAttendance.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(markAttendance.user_id.ToString() + markAttendance.gym_id.ToString(), config.secret_key, markAttendance.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {markAttendance.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getGym = await _gymList.FindOneByCriteria(x => x.Id == markAttendance.gym_id);
                if (getGym == null)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "Requested gym does not exist"
                    };
                }

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(getGym.CheckInEndPoint + $"/{markAttendance.user_id}");
                    if (!request.IsSuccessStatusCode)
                    {
                        return new notification_response
                        {
                            status = 407,
                            message = "Checking failed for user, third-party not responding"
                        };
                    }

                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response == null || response.status != "success")
                    {
                        return new notification_response
                        {
                            status = 407,
                            message = "Checking failed for user, Try again"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "Check-In successful",
                        data = response.data
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
                    message = "oops!, something happend while authenticating user"
                };
            }
        }
    }
}