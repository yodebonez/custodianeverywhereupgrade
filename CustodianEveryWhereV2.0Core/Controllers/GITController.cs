using DataStore.repository;
using DataStore.Models;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
    /// <summary>
    /// The Controller manages all GIT insurance
    /// </summary>
  
    [ApiController]
    [Route("api/[controller]")]
    public class GITController : ControllerBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<PremiumCalculatorMapping> _premium_map = null;
        private Utility util = null;
        private store<GITInsurance> _git = null;
        public GITController()
        {
            _apiconfig = new store<ApiConfiguration>();
            _premium_map = new store<PremiumCalculatorMapping>();
            util = new Utility();
            _git = new store<GITInsurance>();
        }
        /// <summary>
        /// The method get quote for Goods in transit(GIT)
        /// </summary>
        /// <param name="Quote"></param>
        /// <returns></returns>
        [HttpPost("{Quote?}")]
        public async Task<req_response> GetGITQuote(GetQuote Quote)
        {
            try
            {
                log.Info("about to validate request params for GetQuote()");
                if (!ModelState.IsValid)
                {
                    return new req_response
                    {
                        status = 404,
                        message = "Some required parameters missing from request"
                    };
                }
                //check if user has access to this function before processing request
                var response = await util.Validator("GetGITQuote", Quote.merchant_id, Quote.category, Quote.value_of_goods, Quote.hash);
                if (response != null)
                {
                    return response;
                }
                decimal premium = 0;
                var premConfig = await _premium_map.FindOneByCriteria(x => x.category.ToLower() == Quote.category.ToLower().Trim().Trim());
                if (premConfig == null)
                {
                    return new req_response
                    {
                        status = 407,
                        message = "Unable to compute premium, please try again"
                    };
                }
                premium = (Quote.value_of_goods * premConfig.rate) / 100;

                return new req_response
                {
                    status = 200,
                    value_of_goods = Quote.value_of_goods,
                    category = Quote.category,
                    premium = premium.ToString(),
                    message = "Premium computation was successful"
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new req_response
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        /// <summary>
        /// The method is for purchase of Good in transit 
        /// </summary>
        /// <param name="BuyGIT"></param>
        /// <returns></returns>
        [HttpPost("{BuyGIT?}")]
        public async Task<req_response> BuyGITInsurance(BuyGITInsurance BuyGIT)
        {
            try
            {
                log.Info($"new request {Newtonsoft.Json.JsonConvert.SerializeObject(BuyGIT)}");
                if (!ModelState.IsValid)
                {
                    return new req_response
                    {
                        status = 404,
                        message = "Some required parameters missing from request"
                    };
                }
                //check user access and hash and other validations
                var response = await util.Validator("BuyGITInsurance", BuyGIT.merchant_id, BuyGIT.category, BuyGIT.value_of_goods, BuyGIT.hash);
                if (response != null)
                {
                    return response;
                }

                decimal premium = 0;
                var premConfig = await _premium_map.FindOneByCriteria(x => x.category.ToLower() == BuyGIT.category.ToLower().Trim().Trim());
                if (premConfig == null)
                {
                    return new req_response
                    {
                        status = 407,
                        message = "Unable to re-compute premium, please try again"
                    };
                }

                premium = (BuyGIT.value_of_goods * premConfig.rate) / 100;
                var save_git_insurance = new GITInsurance
                {
                    address = BuyGIT.address,
                    category = BuyGIT.category.ToUpper(),
                    email_address = BuyGIT.email_address,
                    goods_description = BuyGIT.goods_description,
                    insured_name = BuyGIT.insured_name,
                    phone_number = BuyGIT.phone_number,
                    vehicle_registration_no = BuyGIT.vehicle_registration_no,
                    value_of_goods = BuyGIT.value_of_goods,
                    premium = premium,
                    rate_used = premConfig.rate,
                    from_date = BuyGIT.cover_period.start_date,
                    //to_date = BuyGIT.cover_period.end_date,
                    from_location = BuyGIT.movement.from,
                    to_location = BuyGIT.movement.to,
                    certificate_serial = await util.GetSerialNumber(),
                    date_created = DateTime.Now,
                    trip_completed = "NO",
                    Type = Types.Continuous.ToString()
                };
                await _git.Save(save_git_insurance);
                string generate_policy_no = await util.GeneratePolicyNO(save_git_insurance.Id);
                save_git_insurance.policy_no = generate_policy_no;

                //get object 
                var getobject = await _git.FindOneByCriteria(x => x.Id == save_git_insurance.Id);
                getobject.policy_no = generate_policy_no;
                await _git.Update(getobject);
                // await _git.CreateQuery($"UPDATE [CustodianEveryWhereV2.0].[dbo].[GITInsurance] SET policy_no = '{generate_policy_no}' WHERE Id = {save_git_insurance.Id}");

                var SendAndSaveCert = await util.GenerateCertificate(new GenerateCert
                {
                    address = save_git_insurance.address,
                    from_date = save_git_insurance.from_date.ToShortDateString(),
                    from_location = save_git_insurance.from_location,
                    interest = save_git_insurance.goods_description,
                    name = save_git_insurance.insured_name,
                    policy_no = save_git_insurance.policy_no,
                    //to_date = save_git_insurance.to_date.ToShortDateString(),
                    to_location = save_git_insurance.to_location,
                    value_of_goods = save_git_insurance.value_of_goods.ToString(),
                    vehicle_reg_no = save_git_insurance.vehicle_registration_no,
                    premium = save_git_insurance.premium.ToString(),
                    serial_number = save_git_insurance.certificate_serial,
                    email_address = save_git_insurance.email_address
                });

                //Send GIT Mail
                var resp = await util.SendGITMail(getobject, "NO", BuyGIT.merchant_id.Trim());
                return SendAndSaveCert;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new req_response
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpGet("{policy_no?}/{hash?}/{merchant_id?}")]
        public async Task<req_response> EndTrip(string policy_no, string hash, string merchant_id)
        {
            try
            {
                log.Info($"Endtrip param from {policy_no} {hash} {merchant_id}");
                if (string.IsNullOrEmpty(policy_no) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(merchant_id))
                {
                    log.Info("parameters missing from request");
                    return new req_response
                    {
                        status = 405,
                        message = "Some parameters missing from request: hint(all params are required)"
                    };
                }
                var get_tranx = await _git.FindOneByCriteria(x => x.policy_no == policy_no.Trim());
                if (get_tranx == null)
                {
                    log.Info("the certificate number is not valid");
                    return new req_response
                    {
                        status = 402,
                        message = "The certificate number is not valid"
                    };
                }
                var response = await util.Validator("EndTrip", merchant_id, get_tranx.category, get_tranx.value_of_goods, hash);

                if (response != null)
                {
                    return response;
                }

                get_tranx.trip_completed = "Yes";
                get_tranx.to_date = DateTime.Now;
                await _git.Update(get_tranx);
                log.Info($"Trip has ended success from {merchant_id} and certificate {policy_no}");

                var resp = await util.SendGITMail(get_tranx, "YES", merchant_id.Trim());

                return new req_response
                {
                    status = 200,
                    message = "Trip has ended successfully",
                    category = get_tranx.category,
                    premium = get_tranx.premium.ToString(),
                    value_of_goods = get_tranx.value_of_goods
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new req_response
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost("{OneOffGit?}")]
        public async Task<req_response> BuyGITOneOffInsurance(BuyOneOffGITInsurance OneOffGit)
        {
            try
            {
                log.Info($"new request {Newtonsoft.Json.JsonConvert.SerializeObject(OneOffGit)}");
                if (!ModelState.IsValid)
                {
                    return new req_response
                    {
                        status = 404,
                        message = "Some required parameters missing from request"
                    };
                }
                //check user access and hash and other validations
                var response = await util.Validator("BuyGITOneOffInsurance", OneOffGit.merchant_id, Category.BREAKABLES.ToString(), OneOffGit.sum_insured, OneOffGit.hash);
                if (response != null)
                {
                    return response;
                }

                var save_git_insurance = new GITInsurance
                {
                    address = OneOffGit.address,
                    category = Category.GENERAL_GOODS.ToString(),
                    email_address = OneOffGit.email_address,
                    insured_name = OneOffGit.insured_name,
                    phone_number = OneOffGit.phone_number,
                    vehicle_registration_no = OneOffGit.vehicle_registration_no,
                    premium = OneOffGit.sum_insured,
                    from_date = DateTime.Now,
                    to_date = DateTime.Now.AddMonths(12),
                    certificate_serial = await util.GetSerialNumber(),
                    date_created = DateTime.Now,
                    trip_completed = "NO",
                    Type = Types.OneOff.ToString()
                };

                await _git.Save(save_git_insurance);
                string generate_policy_no = await util.GeneratePolicyNO(save_git_insurance.Id);
                save_git_insurance.policy_no = generate_policy_no;
                var getobject = await _git.FindOneByCriteria(x => x.Id == save_git_insurance.Id);
                getobject.policy_no = generate_policy_no;
                await _git.Update(getobject);


                var SendAndSaveCert = await util.GenerateCertificateOneOff(new GenerateCert
                {
                    address = save_git_insurance.address,
                    from_date = save_git_insurance.from_date.ToShortDateString(),
                    name = save_git_insurance.insured_name,
                    policy_no = save_git_insurance.policy_no,
                    to_date = save_git_insurance.to_date.Value.ToShortDateString(),
                    vehicle_reg_no = save_git_insurance.vehicle_registration_no,
                    serial_number = save_git_insurance.certificate_serial,
                    email_address = save_git_insurance.email_address
                });

                var resp = await util.SendGITMail(getobject, OneOffGit.merchant_id.Trim());
                return SendAndSaveCert;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new req_response
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }
    }
}
