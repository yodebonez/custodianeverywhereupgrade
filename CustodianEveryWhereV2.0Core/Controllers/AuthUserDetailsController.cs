using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using JWT.Algorithms;
using JWT.Builder;
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
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthUserDetailsController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<AdaptLeads> auth_user = null;
        private store<SessionTokenTracker> session = null;
        public AuthUserDetailsController(IWebHostEnvironment hostingEnvironment)
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            auth_user = new store<AdaptLeads>();
            session = new store<SessionTokenTracker>();
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("{auth_deatils?}")]
        public async Task<notification_response> AuthUser(UserAuthDetails auth_deatils)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    log.Info($"All request parameters are mandatory for email {auth_deatils.email}");
                    return new notification_response
                    {
                        status = 203,
                        message = "All request parameters are mandatory"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("AuthUser", auth_deatils.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == auth_deatils.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {auth_deatils.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }


                // validate hash
                var checkhash = await util.ValidateHash2(auth_deatils.UUID + auth_deatils.email, config.secret_key, auth_deatils.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {auth_deatils.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var check_for_existence = await auth_user.FindOneByCriteria(x => x.email.ToLower() == auth_deatils.email.ToLower());

                bool dontpass = false;
                if (check_for_existence != null && (string.IsNullOrEmpty(check_for_existence.app_version) || string.IsNullOrEmpty(check_for_existence.fcm_token)
                    || string.IsNullOrEmpty(check_for_existence.platform)))
                {
                    check_for_existence.platform = auth_deatils.platform;
                    check_for_existence.fcm_token = auth_deatils.fcm_token;
                    check_for_existence.app_version = auth_deatils.app_version;
                    check_for_existence.updatedAt = DateTime.Now;
                    await auth_user.Update(check_for_existence);
                    dontpass = true;
                }

                if (!dontpass && check_for_existence != null && (!auth_deatils.fcm_token.Trim().Equals(check_for_existence.fcm_token)
                    || auth_deatils.UUID.Trim().Equals(check_for_existence.UUID)))
                {
                    check_for_existence.platform = auth_deatils.platform;
                    check_for_existence.fcm_token = auth_deatils.fcm_token;
                    check_for_existence.app_version = auth_deatils.app_version;
                    check_for_existence.updatedAt = DateTime.Now;
                    await auth_user.Update(check_for_existence);
                }

                updates update = null;
                try
                {

                    // Combine the base path with the relative path to the file
                    string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "TravelCategoryJSON", "AppUpdate.json");

                    // Read the contents of the file
                    string getFile = System.IO.File.ReadAllText(filePath);

                  //  string getFile = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/AppUpdate.json"));
                    update = Newtonsoft.Json.JsonConvert.DeserializeObject<updates>(getFile);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    log.Error(ex.InnerException);
                    log.Error(ex.StackTrace);
                }

                if (check_for_existence != null && !string.IsNullOrEmpty(check_for_existence.app_version) && !string.IsNullOrEmpty(check_for_existence.fcm_token) && !string.IsNullOrEmpty(check_for_existence.platform))
                {
                    log.Info($"User profile already exist {auth_deatils.email}");
                    return new notification_response
                    {
                        status = 200,
                        message = "User profile is active",
                        data = check_for_existence,
                        app_updates = update
                    };
                }


                if (check_for_existence == null)
                {
                    var new_user = new AdaptLeads
                    {
                        created_at = DateTime.Now,
                        email = auth_deatils.email.ToLower(),
                        fullname = auth_deatils.fullname,
                        UUID = auth_deatils.UUID,
                        app_version = auth_deatils.app_version,
                        fcm_token = auth_deatils.fcm_token,
                        platform = auth_deatils.platform,
                        createdAt = DateTime.Now
                    };

                    await auth_user.Save(new_user);
                }


                log.Info($"User profile hash been saved {auth_deatils.email}");
                return new notification_response
                {
                    status = 200,
                    message = $"User profile has been created for '{auth_deatils.email}'",
                    data = auth_deatils,
                    app_updates = update
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
                    message = "oops!, something happend while saving authentication details"
                };
            }
        }

        [HttpGet("{Appverion?}/{platforms?}/{merchant_id?}")]
        public async Task<notification_response> CheckForAppUpdate(string Appverion, AppPlatform platforms, string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("CheckForAppUpdate", merchant_id);
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
               // string getFile = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/AppUpdate.json"));

                // Combine the base path with the relative path to the file
                string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "TravelCategoryJSON", "AppUpdate.json");

                // Read the contents of the file
                string getFile = System.IO.File.ReadAllText(filePath);


                var update = Newtonsoft.Json.JsonConvert.DeserializeObject<updates>(getFile);
                if (platforms == AppPlatform.Andriod)
                {
                    if (!update.android_version.Trim().Equals(Appverion.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return new notification_response
                        {
                            app_updates = update,
                            status = 200,
                            message = "Update avaliable"
                        };
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 201,
                            message = "Your application is upto date"
                        };
                    }
                }
                else
                {
                    if (!update.ios_version.Trim().Equals(Appverion.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return new notification_response
                        {
                            app_updates = update,
                            status = 200,
                            message = "Update avaliable"
                        };
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 201,
                            message = "Your application is upto date"
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
                    message = "oops!, something happend while saving authentication details"
                };
            }
        }

        [HttpPost("{azureAD?}")]
        public async Task<notification_response> ADAuthenticate(AzureAD azureAD)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ADAuthenticate", azureAD.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == azureAD.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {azureAD.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(azureAD.merchant_id, config.secret_key, azureAD.hash);
                checkhash = true;// remove before pushing to production
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {azureAD.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                string user_email = "";
                using (var api = new HttpClient())
                {
                    var param = new Dictionary<string, string>
                    {
                        {"client_id", config.AD_client_id},
                        {"client_secret", config.AD_client_secret},
                        {"grant_type", "authorization_code"},
                        {"redirect_uri", config.AD_redirect_uri},
                        {"code", azureAD.AzureAuthCode},
                        {"scope", "User.Read" }
                    };

                    HttpContent content = new FormUrlEncodedContent(param);
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var request = await api.PostAsync(GlobalConstant.AD_AUTHENTICATE, content);
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                        using (var api2 = new HttpClient())
                        {
                            log.Info($"Resposne from Azure authentication {response}");
                            api2.DefaultRequestHeaders.Add("Authorization", $"Bearer  {response["access_token"]}");
                            var request2 = await api2.GetAsync(GlobalConstant.AD_GRAPH);
                            if (request2.IsSuccessStatusCode)
                            {
                                var response2 = await request2.Content.ReadFromJsonAsync<dynamic>();
                                user_email = response2["mail"];
                                return new notification_response
                                {
                                    status = 200,
                                    message = "AUthentication was successful",
                                    data = new
                                    {
                                        user_email = user_email,
                                        access_token = response["access_token"],
                                        profileData = response2
                                    }
                                };
                            }
                            else
                            {
                                return new notification_response
                                {
                                    status = 406,
                                    message = "Unable to read user information from AD"
                                };
                            }

                        }
                    }
                    else
                    {
                        var response = await request.Content.ReadFromJsonAsync<dynamic>();
                        return new notification_response
                        {
                            status = 406,
                            message = "Authentication failed",
                            data = response
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
                    message = "Oops!, Unable to authenticate user with active directory"
                };
            }
        }

        [HttpGet("{merchant_id?}/{hash?}")]
        public async Task<notification_response> GenerateJWT(string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GenerateJWT", merchant_id);
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

                string secret = GlobalConstant.JWT_SECRET; //"GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
                string sessionId = await util.MD_5(Guid.NewGuid().ToString() + DateTime.Now.Ticks);
                string token = JwtBuilder.Create()
                      .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                      .WithSecret(secret)
                      .AddClaim("exp", new DateTimeOffset(DateTime.UtcNow.AddHours(5), TimeSpan.Zero).ToUnixTimeSeconds())
                      .AddClaim("claim2", sessionId)
                      .Encode();
                var track = new SessionTokenTracker
                {
                    createdat = DateTime.Now,
                    expiresin = DateTime.Now.AddMinutes(GlobalConstant.JWT_ACTIVE_TIME),
                    isactive = true,
                    jwt = token,
                    refreshat = DateTime.Now,
                    sessionid = sessionId
                };
                await session.Save(track);
                return new notification_response
                {
                    status = 200,
                    data = new
                    {
                        authorization = track.jwt,
                        sessionId = track.sessionid,
                        createdat = DateTime.Now,
                        message = $"Inactivity period is set to {GlobalConstant.JWT_ACTIVE_TIME} mins"
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
                    message = "Oops!, something happend while generating token"
                };
            }
        }

        [HttpGet("{merchant_id?}/{sessionid?}/{hash?}")]
        public async Task<notification_response> DestroyJWT(string merchant_id, string sessionid, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("DestroyJWT", merchant_id);
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

                var checkhash = await util.ValidateHash2(merchant_id + sessionid, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getToken = await session.FindOneByCriteria(x => x.sessionid == sessionid && x.isactive == true);
                if (getToken == null)
                {
                    return new notification_response
                    {
                        status = 403,
                        message = "Token doesnt exist"
                    };
                }

                getToken.isactive = false;
                getToken.refreshat = DateTime.Now;
                await session.Update(getToken);
                return new notification_response
                {
                    status = 200,
                    message = "Token was deactivated successfully"
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
                    message = "Oops!, something happend while destroying token"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> LDAPAuthentication()
        {
            try
            {
                //System.DirectoryServices.
                //DirectoryEntry directoryEntry = new DirectoryEntry(GlobalConstant.AD_CREDENTAILS, "username", "password");
                return null;
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "Oops!, something happend while destroying token"
                };
            }
        }
    }
}
