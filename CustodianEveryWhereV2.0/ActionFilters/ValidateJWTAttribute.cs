using DataStore.Models;
using DataStore.repository;
using DataStore.ViewModels;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using JWT.Serializers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace CustodianEveryWhereV2._0.ActionFilters
{
    public class ValidateJWTAttribute : ActionFilterAttribute
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                await base.OnActionExecutingAsync(actionContext, cancellationToken);
                string merchant_id = "";
                foreach (var item in actionContext.ActionArguments.Keys)
                {
                    if (item == "merchant_id")
                    {
                        merchant_id = (string)actionContext.ActionArguments[item];
                        break;
                    }
                    else
                    {
                        var propName = actionContext.ActionArguments[item].GetType().GetProperty("merchant_id");
                        merchant_id = (string)propName.GetValue(actionContext.ActionArguments[item], null);
                        break;
                    }
                }
                if (await ValidateMerchant(merchant_id))
                {
                    IEnumerable<string> authorization = null;
                    IEnumerable<string> sessionid = null;
                    actionContext.Request.Headers.TryGetValues("Authorization", out authorization);
                    actionContext.Request.Headers.TryGetValues("sessionid", out sessionid);
                    if (authorization == null || sessionid == null)
                    {
                        var resp = new
                        {
                            status = (int)HttpStatusCode.Unauthorized,
                            message = "Authorization denied(Invalid session Id or authorization code)"
                        };
                        log.Info("Authorization denied(Invalid session Id or authorization code)");
                        actionContext.Response = new HttpResponseMessage
                        {
                            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(resp), Encoding.UTF8, "application/json"),
                            StatusCode = HttpStatusCode.Unauthorized
                        };
                    }
                    else
                    {
                        if (authorization.ToList()[0].Split(' ').First() != "Bearer")
                        {
                            var resp = new
                            {
                                status = (int)HttpStatusCode.Unauthorized,
                                message = "Invalid authorization format: Bearer <token value> "
                            };
                            log.Info("Invalid authorization format: Bearer <token value>");
                            actionContext.Response = new HttpResponseMessage
                            {
                                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(resp), Encoding.UTF8, "application/json"),
                                StatusCode = HttpStatusCode.Unauthorized
                            };
                        }
                        log.Info($"Authorization {authorization.ToList()[0].Split(' ').Last()}");
                        log.Info($"Sessionid {sessionid.ToList().First()}");
                        var validateJWT = await JWTValidator(authorization.ToList()[0].Split(' ').Last(), sessionid.ToList().First());
                        if (!validateJWT.isvalid)
                        {
                            log.Info(validateJWT.message);
                            var resp = new
                            {
                                status = (int)HttpStatusCode.Unauthorized,
                                message = validateJWT.message
                            };
                            actionContext.Response = new HttpResponseMessage
                            {
                                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(resp), Encoding.UTF8, "application/json"),
                                StatusCode = HttpStatusCode.Unauthorized
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info(ex.Message);
                log.Info(ex.StackTrace);
            }


        }
        private async Task<validationMsg> JWTValidator(string token, string sessionid)
        {
            try
            {
                store<SessionTokenTracker> session = new store<SessionTokenTracker>();
                string secret = GlobalConstant.JWT_SECRET;
                var getSessionId = await session.FindOneByCriteria(x => x.sessionid == sessionid && x.isactive == true);
                if (getSessionId == null)
                {
                    log.Info("Invalid session Id");
                    return new validationMsg
                    {
                        message = "Invalid session Id",
                        isvalid = false
                    };
                }

                if (!getSessionId.jwt.Equals(token))
                {
                    log.Info("Token mismatched");
                    return new validationMsg
                    {
                        message = "Token mismatched",
                        isvalid = false
                    };
                }
                //check time
                int total = (int)getSessionId.expiresin.Subtract(DateTime.Now).TotalMinutes;

                if (total > GlobalConstant.JWT_ACTIVE_TIME || total < 0)
                {
                    getSessionId.isactive = false;
                    await session.Update(getSessionId);
                    log.Info("Token has expired");
                    return new validationMsg
                    {

                        message = "Token has expired",
                        isvalid = false
                    };

                }
                await Task.Factory.StartNew(() =>
                {
                    IJsonSerializer serializer = new JsonNetSerializer();
                    var provider = new UtcDateTimeProvider();
                    IJwtValidator validator = new JwtValidator(serializer, provider);
                    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                    IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
                    IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                    var decoded = decoder.Decode(token, secret, verify: true);
                    //referesh token
                });

                getSessionId.expiresin = DateTime.Now.AddMinutes(GlobalConstant.JWT_ACTIVE_TIME);
                getSessionId.refreshat = DateTime.Now;
                await session.Update(getSessionId);
                return new validationMsg
                {
                    message = "Token is valid",
                    isvalid = true
                };

            }
            catch (TokenExpiredException ex)
            {
                return new validationMsg
                {
                    message = "Token has expired",
                    isvalid = false
                };

            }
            catch (SignatureVerificationException ex)
            {
                return new validationMsg
                {
                    message = "Token has invalid signature",
                    isvalid = false
                };
            }
            catch (Exception ex)
            {
                return new validationMsg
                {
                    message = ex.Message,
                    isvalid = false
                };
            }
        }

        private async Task<bool> ValidateMerchant(string merchant_id)
        {
            try
            {
                if (string.IsNullOrEmpty(merchant_id))
                {
                    return false;
                }
                store<ApiConfiguration> _apiconfig = new store<ApiConfiguration>();
                var merchant = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id && x.is_active == true);
                if (merchant == null)
                {
                    return false;
                }
                if (!merchant.EnableBearerAuthorization)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public class validationMsg
    {
        public validationMsg()
        {

        }
        public string message { get; set; }
        public bool isvalid { get; set; }
    }
}