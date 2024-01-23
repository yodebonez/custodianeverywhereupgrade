using CustodianEmailSMSGateway.Email;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [ApiController]
    [Route("api/[controller]")]
    public class CarTrackingController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<BuyTrackerDevice> track = null;
        private store<TelematicsUsers> telematics_user = null;
        public CarTrackingController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            track = new store<BuyTrackerDevice>();
            telematics_user = new store<TelematicsUsers>();
        }

        [HttpGet("{imei?}/{merchant_id?}/{hash?}/{email?}/{password?}")]
        public async Task<notification_response> GetCarLastStatus(string imei, string merchant_id, string hash, string email, string password)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetCarLastStatus", merchant_id);
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                log.Info($"User password {password}");
                //var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                //var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                //var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];
                var passwd = util.Sha256(password);
                var auth = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == email.ToLower() && x.password == passwd && x.IsActive == true);

                if (auth == null)
                {
                    log.Info($"Invalid Username or Password {merchant_id}");
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid Username or Password"
                    };
                }

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(GlobalConstant.base_url + $"getLastStatus?email={GlobalConstant.auth_email}&passcode={GlobalConstant.passcode}&imei={imei}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"Raw response from api {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
                        {
                            log.Info($"status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation successful",
                                data = response
                            };
                        }
                        else
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 206,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        log.Info($"unable to get status imei for user imei {imei}");
                        return new notification_response
                        {
                            status = 205,
                            message = "Unable to get vehicle last status"
                        };
                    }
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

        [HttpGet("{imei?}/{start_date_time?}/{end_date_time?}/{merchant_id?}/{hash?}/{email?}/{password?}/{page?}")]
        public async Task<notification_response> GetCarStatusHistory(string imei, string start_date_time, string end_date_time, string merchant_id, string hash, string email, string password, int page = 1)
        {
            try
            {
                log.Info(start_date_time + " " + end_date_time);
                var check_user_function = await util.CheckForAssignedFunction("GetCarStatusHistory", merchant_id);
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                //var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                //var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];

                var passwd = util.Sha256(password);
                var auth = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == email.ToLower() && x.password == passwd && x.IsActive == true);

                if (auth == null)
                {
                    log.Info($"Invalid Username or Password {merchant_id}");
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid Username or Password"
                    };
                }

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(GlobalConstant.base_url + $"getStatusHistory?email={GlobalConstant.auth_email}&passcode={GlobalConstant.passcode}&imei={imei}&start_date_time={start_date_time}&end_date_time={end_date_time}&pageNo={page}&pageSize=10");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"Raw response from api {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation successful",
                                data = response
                            };
                        }
                        else
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 206,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        log.Info($"unable to get status imei for user imei {imei}");
                        return new notification_response
                        {
                            status = 205,
                            message = "Unable to get vehicle last status"
                        };
                    }
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

        [HttpGet("{imei?}/{lng?}/{lat?}/{merchant_id?}/{email?}/{password?}/{hash?}")]
        public async Task<notification_response> GetCarAddress(string imei, string lng, string lat, string merchant_id, string email, string password, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetCarAddress", merchant_id);
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                //var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                //var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];
                var passwd = util.Sha256(password);
                var auth = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == email.ToLower() && x.password == passwd && x.IsActive == true);

                if (auth == null)
                {
                    log.Info($"Invalid Username or Password {merchant_id}");
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid Username or Password"
                    };
                }
                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(GlobalConstant.base_url + $"getAddress?email={GlobalConstant.auth_email}&passcode={GlobalConstant.passcode}&latlng={lat},{lng}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"Raw response from api {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation successful",
                                data = response
                            };
                        }
                        else
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 206,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        log.Info($"unable to get status imei for user imei {imei}");
                        return new notification_response
                        {
                            status = 205,
                            message = "Unable to get vehicle last status"
                        };
                    }
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

        [HttpPost("{tracker?}")]
        public async Task<notification_response> BuyTracker(BuyTracker tracker)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 406,
                        message = "Some required parameters missing from request payload"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("BuyTracker", tracker.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == tracker.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {tracker.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                //validate hash
                var checkhash = await util.ValidateHash2(tracker.address + tracker.customer_email + tracker.reference, config.secret_key, tracker.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {tracker.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                DateTime install_date;
                var parse_date = DateTime.TryParse(tracker.installation_date_time, out install_date);
                if (!parse_date)
                {
                    return new notification_response
                    {
                        status = 407,
                        message = "Invalid Installation date"
                    };
                }

                var checkme = await track.FindOneByCriteria(x => x.reference.ToLower() == tracker.reference.ToLower());
                if (checkme != null)
                {
                    log.Info($"duplicate request {tracker.merchant_id}");
                    return new notification_response
                    {
                        status = 300,
                        message = "Duplicate request"
                    };
                }

                using (var apicall = new HttpClient())
                {
                    var request = await apicall.PostAsJsonAsync(GlobalConstant.base_url + "submitRequest", new BuyTrackerPost
                    {
                        address = tracker.address,
                        contact_person = tracker.contact_person,
                        customer_email = tracker.customer_email,
                        customer_name = tracker.customer_name,
                        installation_date_time = install_date,
                        mobile_number = tracker.mobile_number,
                        plate_number = tracker.plate_number,
                        tracker_type_id = tracker.tracker_type_id,
                        user_email = GlobalConstant.auth_email,
                        user_passcode = GlobalConstant.passcode,
                        vehicle_year = tracker.vehicle_year,
                        vehicle_make = tracker.vehicle_make,
                        vehicle_model = tracker.vehicle_model
                    });
                    log.Info($"response from halogen buytracker device is {request.StatusCode}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"response object from halogen buytracker device is {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
                        {
                            var savenew_request = new BuyTrackerDevice
                            {
                                address = tracker.address,
                                annual_subscription = tracker.annual_subscription,
                                contact_person = tracker.contact_person,
                                customer_email = tracker.customer_email,
                                customer_name = tracker.customer_name,
                                date_created = DateTime.Now,
                                device_description = tracker.device_description,
                                installation_date_time = install_date,
                                mobile_number = tracker.mobile_number,
                                plate_number = tracker.plate_number,
                                price = tracker.price,
                                tracker_type_id = tracker.tracker_type_id,
                                vehicle_year = tracker.vehicle_year,
                                vehicle_make = tracker.vehicle_make,
                                vehicle_model = tracker.vehicle_model,
                                reference = tracker.reference
                            };

                            log.Info($"about to save to database");
                            if (await track.Save(savenew_request))
                            {
                                log.Info($"data commited to database");
                                return new notification_response
                                {
                                    status = 200,
                                    message = "Your device has been successfully booked"
                                };
                            }
                            else
                            {
                                log.Info($"something happend while committing data check stack trace");
                                return new notification_response
                                {
                                    status = 203,
                                    message = "Oops!, something went wrong while processing your request"
                                };
                            }
                        }
                        else
                        {
                            return new notification_response
                            {
                                status = 205,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 409,
                            message = "Tracker request was not successful"
                        };
                    }
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
                    message = "oops!, something happend while booking your request"
                };
            }
        }

        [HttpGet("{year?}/{merchant_id?}/{hash?}/{car_value?}")]
        public async Task<notification_response> GetCompactibleVehicle(int year, string merchant_id, string hash, decimal car_value = 0)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetCompactibleVehicle", merchant_id);
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
                var checkhash = await util.ValidateHash2(year.ToString(), config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                bool can_buy_comprehensive = false;
                if (car_value > 0 && car_value >= GlobalConstant.DeviceComprehensiveTracker)
                {
                    can_buy_comprehensive = true;
                }
                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"getCompatibleTrackerTypes?vehicle_year={year}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<DevicePricesResponse>();
                        if (response.response_code == "00")
                        {
                            log.Info("loaded successfully");
                            var cnt = response.data;
                            if (cnt.Count > 0)
                            {

                                response.data[0].discount = GlobalConstant.DiscountPriceHalogen;
                                response.data[0].actual_price = Convert.ToDecimal(GlobalConstant.HalogenDefaultPrice);
                                response.data[0].label = GlobalConstant.LabelHalogen;
                                response.data[0].price = GlobalConstant.HardCodedHalogenPrice + Convert.ToDecimal(GlobalConstant.LoadingPrice);
                                //response.can_buy_comprehensive = can_buy_comprehensive;
                                return new notification_response
                                {
                                    status = 200,
                                    message = "operation successful",
                                    data = response.data,
                                    can_buy_comprehensive = can_buy_comprehensive
                                };
                            }
                            else
                            {
                                return new notification_response
                                {
                                    status = 209,
                                    message = "Vehicle not yet supported",
                                };
                            }
                            //return new notification_response
                            //{
                            //    status = 200,
                            //    message = "operation successful",
                            //    data = response.data
                            //};
                        }
                        else
                        {
                            log.Info("no vehicle found for selected year");
                            return new notification_response
                            {
                                status = 409,
                                message = "Sorry no device for your car model and year"
                            };
                        }
                    }
                    else
                    {
                        log.Info("error loading compactible vehicle with year");
                        return new notification_response
                        {
                            status = 405,
                            message = "oops!, something happend while getting vehicle"
                        };
                    }

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
                    message = "oops!, something happend while getting vehicle"
                };
            }
        }

        [HttpGet("{imei?}/{state?}/{email?}/{password?}/{merchant_id?}/{hash?}")]
        public async Task<notification_response> StopStartCar(string imei, Car state, string email, string password, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("StopStartCar", merchant_id);
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var passwd = util.Sha256(password);
                var auth = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == email.ToLower() && x.password == passwd && x.IsActive == true);

                if (auth == null)
                {
                    log.Info($"Invalid Username or Password {merchant_id}");
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid Username or Password"
                    };
                }
                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"{((state == Car.Start) ? "startCar" : "stopCar")}?email={GlobalConstant.auth_email}&passcode={GlobalConstant.passcode}&imei={imei}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        if (response.response_code == "00")
                        {
                            return new notification_response
                            {
                                status = 200,
                                message = ((state == Car.Start) ? "Start" : "Stop") + " command was successful"
                            };
                        }
                        else
                        {
                            return new notification_response
                            {
                                status = 401,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 401,
                            message = "Unable to start/stop car at the moment, Please try again"
                        };
                    }
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
                    message = "Error: Unable to start/stop car at the moment"
                };
            }
        }

        [HttpGet("{imei?}/{sos_number?}/{merchant_id?}/{hash?}/{email?}/{password?}")]
        public async Task<notification_response> ListenIn(string imei, string sos_number, string merchant_id, string hash, string email, string password)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ListenIn", merchant_id);
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
                var checkhash = await util.ValidateHash2(imei + sos_number, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var passwd = util.Sha256(password);
                var auth = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == email.ToLower() && x.password == passwd && x.IsActive == true);

                if (auth == null)
                {
                    log.Info($"Invalid Username or Password {merchant_id}");
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid Username or Password"
                    };
                }
                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"listenIn?email={GlobalConstant.auth_email}&passcode={GlobalConstant.passcode}&imei={imei}&sos_number={sos_number}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        if (response.response_code == "00")
                        {
                            return new notification_response
                            {
                                status = 200,
                                message = $"Command was successful, kindly call {sos_number} with your mobile phone and listen in"
                            };
                        }
                        else
                        {
                            return new notification_response
                            {
                                status = 401,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 401,
                            message = "Unable to bind number to device, Please try again"
                        };
                    }
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
                    message = "Error: Unable to bind number to device"
                };
            }
        }

        [HttpPost("{user?}")]
        public async Task<notification_response> SetupUser(SetTeleUser user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Some required fields missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SetupUser", user.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == user.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {user.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(user.Email + user.OTP + user.Newpassword, config.secret_key, user.hash);
                checkhash = true; //TODO: this should be remove after mejide has fix the bug on the front end
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {user.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                // check if user exist
                var is_profile_setup = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == user.Email.ToLower());
                if (is_profile_setup != null)
                {
                    log.Info($"user has been setup already {user.Email}");
                    return new notification_response
                    {
                        status = 402,
                        message = "User profile is already setup"
                    };
                }
                // if user exist at halogen ends
                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"GetImeiByEmail?email={user.Email}");
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info($"verifying from halogen {user.Email}");
                        return new notification_response
                        {
                            status = 409,
                            message = "Secondary verification failed"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response.response_code != "00" || response.data == null)
                    {
                        log.Info($"verifying from halogen failed {user.Email}");
                        return new notification_response
                        {
                            status = 409,
                            message = response.response_message
                        };
                    }

                    var validate_otp = await util.ValidateOTP(user.OTP, user.Email);
                    if (!validate_otp)
                    {
                        log.Info($"Invalid otp{user.Email}");
                        return new notification_response
                        {
                            status = 402,
                            message = "Invalid OTP provided"
                        };
                    }
                    //var request2 = await apicall.PostAsJsonAsync(GlobalConstant.base_url + "SetNewPwd",
                    //    new Dictionary<string, string>()
                    //    {
                    //        { "email", user.Email.Trim().ToLower() },
                    //        { "passcode", user.Newpassword }
                    //    });

                    //if (!request2.IsSuccessStatusCode)
                    //{
                    //    log.Info($"unable to set password {user.Email}");
                    //    return new notification_response
                    //    {
                    //        status = 406,
                    //        message = "Host not responding to request, Try Again"
                    //    };
                    //}
                    //var response2 = await request2.Content.ReadAsAsync<dynamic>();
                    //if (response2.response_code != "00")
                    //{
                    //    log.Info($"unable to set password/pssword reset failed {user.Email}");
                    //    return new notification_response
                    //    {
                    //        status = 406,
                    //        message = "Unable setup user credential, Try Again"
                    //    };
                    //}

                    if (validate_otp)
                    {
                        var new_setup = new TelematicsUsers
                        {
                            CreatedAt = DateTime.Now,
                            email = user.Email.ToLower(),
                            Gender = user.Gender,
                            IsActive = true,
                            IsFromCustodian = true,
                            LoginLocation = user.LoginLocation,
                            OwnerName = user.OwnerName,
                            LastLoginDate = DateTime.Now,
                            password = util.Sha256(user.Newpassword)
                        };
                        await telematics_user.Save(new_setup);
                        return new notification_response
                        {
                            status = 200,
                            message = "User profile setup successfully"
                        };
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 401,
                            message = "Invalid OTP provided"
                        };
                    }
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
                    message = "Error: Unable to setup user, Try Again"
                };
            }
        }

        [HttpGet("{email?}/{merchant_id?}/{hash?}")]
        public async Task<notification_response> SendSecureCode(string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("SendSecureCode", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
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
                var checkhash = await util.ValidateHash2(email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //validate email

                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"GetImeiByEmail?email={email?.ToLower().Trim()}");
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info("Unable to verify email, Try Again");
                        return new notification_response
                        {
                            status = 409,
                            message = "Unable to verify email, Try Again"
                        };
                    }

                    var response = await request.Content.ReadAsAsync<dynamic>();
                    log.Info($"new log {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                    if (response.response_code != "00")
                    {
                        log.Info("Unable to verify email, Try Again");
                        return new notification_response
                        {
                            status = 407,
                            message = "Email address not associated with any tracking device"
                        };
                    }

                    var generate_otp = await util.GenerateOTP(false, email.ToLower(), "TELEMATICS", Platforms.ADAPT);
                    if (string.IsNullOrEmpty(generate_otp))
                    {
                        log.Info("Unable to generate OTP, Try Again");
                        return new notification_response
                        {
                            status = 408,
                            message = "Unable to generate OTP, Try Again"
                        };
                    }

                    // send OTP to email address
                    string messageBody = $"Adapt Telematics authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";
                    var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                    StringBuilder sb = new StringBuilder(template);
                    sb.Replace("#CONTENT#", messageBody);
                    sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                    var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                    await Task.Factory.StartNew(() =>
                    {
                        //  List<string> cc = new List<string>();
                        // cc.Add("technology@custodianplc.com.ng");
                        new SendEmail().Send_Email(email, "Adapt-Telematics Authentication", sb.ToString(), "Telematics Authentication", true, imagepath, null, null, null);
                    });

                    log.Info($"Otp was sent successfully to {email}");
                    return new notification_response
                    {
                        status = 200,
                        message = $"OTP was sent successfully to {email}"
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
                    message = "Error: Unable to verify user email, Try Again"
                };
            }
        }

        [HttpPost("{userAuth?}")]
        public async Task<notification_response> AuthUser(AuthTeleUser userAuth)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid request: some parameters missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("AuthUser", userAuth.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == userAuth.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {userAuth.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(userAuth.email + userAuth.password, config.secret_key, userAuth.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {userAuth.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //check if user has been setup first
                var password = util.Sha256(userAuth.password);
                var is_user_setup = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == userAuth.email.ToLower() && x.IsActive == true && x.password == password);
                if (is_user_setup == null)
                {
                    log.Info("User account has not been mapped. kindly use the setup option");
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid Username or Password",
                    };
                }

                //authenticate from halogen
                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"GetImeiObjectsByEmail?email={GlobalConstant.auth_email}&passcode={GlobalConstant.passcode}&customer_email={userAuth.email}");
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info("Host is not reachable, Try Again");
                        return new notification_response
                        {
                            status = 402,
                            message = "Host is not reachable, Try Again",
                        };
                    }

                    //read from  stream
                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response.response_code != "00")
                    {
                        log.Info("Authentication failed: Hint(Invalid email or password)");
                        return new notification_response
                        {
                            status = 401,
                            message = "Authentication failed: Hint(Invalid email or password)",
                        };
                    }

                    log.Info($"Login successful {userAuth.email}");
                    string messageBody = $"<h3>Authentication was successful on {DateTime.Now.ToString("dddd, dd MMMM yyyy")}</h3>";
                    var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                    StringBuilder sb = new StringBuilder(template);
                    sb.Replace("#CONTENT#", messageBody);
                    sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                    var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                    await Task.Factory.StartNew(() =>
                     {
                         List<string> bcc = new List<string>();
                         // bcc.Add("technology@custodianplc.com.ng");
                         new SendEmail().Send_Email(userAuth.email, "Adapt-Telematics Authentication successful", sb.ToString(), "Telematics Authentication successful", true, imagepath, null, null, null);
                     });

                    // successful process
                    return new notification_response
                    {
                        status = 200,
                        message = "Authentication successful",
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
                    message = "Error: User authenication failed, Try Again"
                };
            }
        }

        [HttpPost("{reset?}")]
        public async Task<notification_response> TelematicsPasswordReset(TelemaricResetPassword reset)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Some required fields missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("TelematicsPasswordReset", reset.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == reset.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {reset.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(reset.email + reset.OTP + reset.password, config.secret_key, reset.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {reset.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                // check if user exist
                var is_profile_setup = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == reset.email.ToLower());

                if (is_profile_setup == null)
                {
                    log.Info($"User profile has not been setup {reset.email}");
                    return new notification_response
                    {
                        status = 402,
                        message = "User profile has not been setup"
                    };
                }
                //check if user exist at halogen ends
                using (var apicall = new HttpClient())
                {
                    // validate OTP
                    var validate_otp = await util.ValidateOTP(reset.OTP, reset.email.ToLower());
                    if (!validate_otp)
                    {
                        log.Info($"Invalid otp{reset.email}");
                        return new notification_response
                        {
                            status = 402,
                            message = "Invalid OTP provided"
                        };
                    }

                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"GetImeiByEmail?email={reset.email}");
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info($"verifying from halogen {reset.email}");
                        return new notification_response
                        {
                            status = 409,
                            message = "Secondary verification failed"
                        };
                    }

                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response.response_code != "00")
                    {
                        log.Info($"verifying from halogen failed {reset.email}");
                        return new notification_response
                        {
                            status = 409,
                            message = response.response_message
                        };
                    }

                    //var request2 = await apicall.PostAsJsonAsync(GlobalConstant.base_url + "SetNewPwd",
                    //    new Dictionary<string, string>()
                    //    {
                    //        { "email", reset.email.Trim().ToLower() },
                    //        { "passcode", reset.password }
                    //    });

                    //if (!request2.IsSuccessStatusCode)
                    //{
                    //    log.Info($"unable to set password {reset.email}");
                    //    return new notification_response
                    //    {
                    //        status = 406,
                    //        message = "Host not responding to request, Try Again"
                    //    };
                    //}
                    //var response2 = await request2.Content.ReadAsAsync<dynamic>();
                    is_profile_setup.password = util.Sha256(reset.password);

                    var response2 = await telematics_user.Update(is_profile_setup);
                    if (!response2)
                    {
                        log.Info($"unable to set password/pssword reset failed {reset.email}");
                        return new notification_response
                        {
                            status = 406,
                            message = "Unable setup user credential, Try Again"
                        };
                    }
                    else
                    {
                        log.Info($"password reset was successful {reset.email}");
                        string messageBody = $"<h2>Your password reset was successful at {DateTime.Now.ToString("dddd, dd MMMM yyyy")}</h2>";
                        var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                        StringBuilder sb = new StringBuilder(template);
                        sb.Replace("#CONTENT#", messageBody);
                        sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                        var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                        await Task.Factory.StartNew(() =>
                        {
                            List<string> cc = new List<string>();
                            // cc.Add("technology@custodianplc.com.ng");
                            new SendEmail().Send_Email(reset.email, "Adapt-Telematics Password Reset", sb.ToString(), "Password Reset", true, imagepath, null, null, null);
                        });

                        return new notification_response
                        {
                            status = 200,
                            message = "Password reset was successful"
                        };
                    }
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
                    message = "Error: Unable to reset password"
                };
            }
        }
    }
}
