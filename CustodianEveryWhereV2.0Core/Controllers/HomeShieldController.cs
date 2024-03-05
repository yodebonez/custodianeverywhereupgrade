using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    public class HomeShieldController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<HomeShield> _homeShield = null;
        private readonly IConfiguration _configuration;
        public HomeShieldController(IConfiguration configuration)
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _homeShield = new store<HomeShield>();
            _configuration = configuration;
        }


        [HttpGet("{merchant_id?}/{units?}/{hash?}")]
        public async Task<notification_response> GetHomeShieldQuote(string merchant_id, int units, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetHomeShieldQuote", merchant_id);
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
                var checkhash = await util.ValidateHash2(units.ToString(), config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetHomeShieldQuote(units);
                    decimal amount;
                    if (!Decimal.TryParse(request, out amount))
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Unable to compute unit at the moment"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "Computation was successful",
                        data = new
                        {
                            units = units,
                            premium = amount,
                            tandc = ""
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
                    message = "oops!, something happend while computing unit"
                };
            }
        }

        [HttpPost("{homeShield?}")]
        public async Task<notification_response> BuyHomeShieldInsurance(HomeShieldViewModel homeShield)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 201,
                        message = "Some params missing from request",
                    };
                }
                var check_user_function = await util.CheckForAssignedFunction("BuyHomeShieldInsurance", homeShield.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == homeShield.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id homeShield{homeShield.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(homeShield.NoOfUnit.ToString() + homeShield.Premium + homeShield.PhoneNumber + homeShield.TransactionReference, config.secret_key, homeShield.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {homeShield.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                if (homeShield.ItemsDescription.Count() < 1)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "Please specify the items you want to insure"
                    };
                }
                string request;
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    string items = "";
                    foreach (var item in homeShield.ItemsDescription)
                    {
                        items += $"Name: {item.ItemName}{Environment.NewLine}Value:{item.ItemValue}{Environment.NewLine}Quantity:{item.Quantity}|";
                    }
                    request = api.PostHomeShield(GlobalConstant.merchant_id, GlobalConstant.password,
                       homeShield.CustomerFullName, homeShield.Address, homeShield.PhoneNumber, homeShield.Email,
                       homeShield.Occupation, homeShield.Premium, homeShield.NoOfUnit, DateTime.Now, homeShield.ActivationDate, homeShield.ActivationDate.AddMonths(12), "", items, "API", homeShield.referralCode ?? "", "", "");
                    log.Info($"response from api {request}");
                }

                var transposed = new HomeShield
                {
                    ActiveDate = homeShield.ActivationDate,
                    Address = homeShield.Address,
                    CustomerFullNAme = homeShield.CustomerFullName,
                    DateCreated = DateTime.Now,
                    Email = homeShield.Email,
                    ExpiryDate = homeShield.ActivationDate.AddMonths(12),
                    IdentificationType = homeShield.IdentificationType.ToString(),
                    NoOfUnit = homeShield.NoOfUnit,
                    Occupation = homeShield.Occupation,
                    PhoneNumber = homeShield.PhoneNumber,
                    Premium = homeShield.Premium,
                    TransactionReeference = homeShield.TransactionReference,
                    Description = Newtonsoft.Json.JsonConvert.SerializeObject(homeShield.ItemsDescription),
                    ResponseFromAPI = request,
                    referralCode = homeShield.referralCode
                };

                //write upload to text files
                var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_HomeShield.{homeShield.AttachementFormat}";
                var filepath = $"{_configuration["Settings:DOC_PATH"]}/Documents/HomeShield/{nameurl}";
                byte[] content = Convert.FromBase64String(homeShield.AttachementInBase64);
               System.IO.File.WriteAllBytes(filepath, content);
                transposed.ImagePath = nameurl;
                transposed.FileFormat = homeShield.AttachementFormat;
                await _homeShield.Save(transposed);

                return new notification_response
                {
                    status = 200,
                    message = "Transaction was successful"
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
                    message = "oops!, something happend while processing transaction"
                };
            }
        }
    }
}
