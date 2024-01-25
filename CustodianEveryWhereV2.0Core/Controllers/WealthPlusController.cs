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
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
 
    [ApiController]
    [Route("api/[controller]")]
    public class WealthPlusController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<WealthPlus> _Buy = null;
        public WealthPlusController(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _Buy = new store<WealthPlus>();
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }


        [HttpGet("{merchant_id?}")]
        public async Task<res> GetWealthPlusDetails(string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetWealthPlusDetails", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

              
                var rootPath = _hostingEnvironment.ContentRootPath;              
                var filePath = Path.Combine(rootPath, "Cert", "WealthPlusDetails.json");
                var productDetails = System.IO.File.ReadAllText(filePath);

                 var details = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(productDetails);
                return new res
                {
                    status = 200,
                    message = "Data fetch was successful",
                    data = new
                    {
                        product = details,
                        policyTerm = Enumerable.Range(5, 21).ToList(),
                        idTypes = new List<string>() { "Voter Card", "National ID", "Driver’s License", "Int’l Passport" },
                        ageConstrian = new
                        {
                            max = 60,
                            min = 18
                        },
                        frequency = new List<string>() { "Monthly", "Quarterly", "Semi-Annually", "Annually" },
                        gender = new List<string>() { "Male", "Female" },
                        maxPercentage = GlobalConstant.GET_WEALTHPLUS_PERCENTAGE
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new res
                {
                    status = 404,
                    message = "Oops!, something happened while loading product details"
                };
            }
        }

        [HttpPost("{wealthPlus?}")]
        public async Task<res> SubmitWealthPlusRequest(WealthPlusView wealthPlus)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new res
                    {
                        status = 302,
                        message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                    };
                }
                var check_user_function = await util.CheckForAssignedFunction("SubmitWealthPlusRequest", wealthPlus.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == wealthPlus.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {wealthPlus.merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(wealthPlus.AmountToSave + wealthPlus.Frequency + wealthPlus.Email, config.secret_key, wealthPlus.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {wealthPlus.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }


                var saveData = new WealthPlus
                {
                    ImagePath = $"{new Utility().GetSerialNumber().GetAwaiter().GetResult()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{Guid.NewGuid().ToString()}.{wealthPlus.ImageFormat?.Trim().ToLower()}",
                    ImageFormat = wealthPlus.ImageFormat,
                    Email = wealthPlus.Email?.Trim(),
                    AmountToSave = wealthPlus.AmountToSave,
                    FirstName = wealthPlus.FirstName,
                    Frequency = wealthPlus.Frequency,
                    Gender = wealthPlus.Gender,
                    IndentificationNumber = wealthPlus.IndentificationNumber,
                    IndentificationType = wealthPlus.IndentificationType,
                    MiddleName = wealthPlus.MiddleName,
                    MobileNo = wealthPlus.MobileNo,
                    PolicyTerm = wealthPlus.PolicyTerm,
                    StartDate = wealthPlus.StartDate,
                    Surname = wealthPlus.Surname,
                    Title = wealthPlus.Title
                };

                var filepath = $"{System.Configuration.ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/WealthPlus/{saveData.ImagePath}";
                try
                {
                    byte[] content = Convert.FromBase64String(wealthPlus.ImageInBase64);
                    System.IO.File.WriteAllBytes(filepath, content);
                   
                }
                catch (Exception ex)
                {
                    log.Error($"Something happend while saving image to path: {saveData.ImagePath}");
                    log.Error(ex.Message);
                    log.Error(ex.StackTrace);
                    log.Error(ex.InnerException);
                }

                //send mail here

                //load template

                if (await _Buy.Save(saveData))
                {


                    var path = HttpContext.Current.Server.MapPath("~/Cert/wealthplus.html");
                    var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                    var template = System.IO.File.ReadAllText(path);
                    new Utility().SendWealthPlusMail(wealthPlus, true, template, imagepath, filepath, "");
                    return new res
                    {
                        status = 200,
                        message = "Thank you for showing interest,You'll be contacted shortly"
                    };
                }
                else
                {
                    return new res
                    {
                        status = 202,
                        message = "Sorry, Something happend while submitting your information. Please try again"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new res
                {
                    status = 404,
                    message = "Oops!, something happened while submitting request"
                };
            }
        }
    }
}
