using DapperLayer.Dapper.Core;
using DapperLayer.utilities;
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
using CustodianEveryWhereV2._0.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [ApiController]
    [Route("api/[controller]")]
    public class CRMController : ControllerBase
    {

        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private Core<dynamic> dapper_core = null;
        public CRMController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
            dapper_core = new Core<dynamic>();
        }

        [HttpGet("{merchant_id?}/{page?}")]
        [GzipCompression]
        public async Task<dynamic> GetPrediction(string merchant_id, int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPrediction", merchant_id);
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

                int pagesize = 30;
                int skip = (page == 1) ? 0 : pagesize * (page - 1);
                int limit = skip + pagesize;
                var result = await dapper_core.GetAllbyPagination(skip, string.Format(connectionManager.sp_getall_new, skip));
                if (result == null)
                {
                    return new
                    {
                        status = 401,
                        message = "No predicted result found"
                    };
                }
                decimal total = Convert.ToDecimal(result.count) / Convert.ToDecimal(pagesize);
                int totalpage = (int)Math.Ceiling(total);
                var props = new
                {
                    pageSize = pagesize,
                    totalPages = totalpage,
                    statusCode = 200,
                    navigation = $"{page} of {totalpage}"
                };
                List<dynamic> mylist = new List<dynamic>();
                foreach (dynamic item in result.results)
                {
                    mylist.Add(new
                    {
                        CustomerID = item.CustomerID,
                        FullName = item.FullName,
                        Email = item.Email,
                        Phone = item.Phone,
                        Occupation = item.Occupation,
                        NoOfProductRecommended = item.NoOfProductRecommended,
                        Source = item.Data_source,
                        currentProdCount = item.currentProdCount,
                        AvgProbabiltyToBuy = item.AvgProb
                    });
                }
                return new { pageProps = props, dataSets = mylist };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "Error: while getting predicted product"
                };
            }
        }

        [HttpGet("{query?}/{merchant_id?}/{page?}")]
        [GzipCompression]
        public async Task<dynamic> SearchPrediction(string query, string merchant_id, int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("SearchPrediction", merchant_id);
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

                int pagesize = 30;
                int skip = (page == 1) ? 0 : pagesize * (page - 1);
                var result = await dapper_core.SearchPager(pagesize, skip, query, connectionManager.search_query);
                if (result == null)
                {
                    return new
                    {
                        status = 401,
                        message = "No predicted result found"
                    };
                }
                decimal total = Convert.ToDecimal(result.count) / Convert.ToDecimal(pagesize);
                int totalpage = (int)Math.Ceiling(total);
                var props = new
                {
                    pageSize = pagesize,
                    totalPages = totalpage,
                    statusCode = 200,
                    navigation = $"{page} of {totalpage}"
                };
                List<dynamic> mylist = new List<dynamic>();
                foreach (dynamic item in result.results)
                {
                    mylist.Add(new
                    {
                        CustomerID = item.CustomerID,
                        FullName = item.FullName,
                        Email = item.Email,
                        Phone = item.Phone,
                        Occupation = item.Occupation,
                        NoOfProductRecommended = item.NoOfProductRecommended,
                        Source = item.Data_source,
                        currentProdCount = item.currentProdCount,
                        AvgProbabiltyToBuy = item.AvgProb
                    });
                }

                return new { pageProps = props, dataSets = mylist };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "Error: while getting predicted product"
                };
            }
        }

        [HttpGet("{customer_id?}/{merchant_id?}")]
        [GzipCompression]
        public async Task<dynamic> GetCurrentAndRecommendedProducts(int customer_id, string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetCurrentAndRecommendedProd", merchant_id);
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

                var result = await dapper_core.GetPredictionByCustomerID(customer_id, connectionManager.recomendation);
                if (result == null)
                {
                    return new
                    {
                        status = 401,
                        message = "No predicted result found"
                    };
                }

                var p = new
                {
                    customer_id = customer_id,
                    currentProduct = result.current,
                    recommendedProduct = result.recommended
                };

                return p;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "Error: while getting predicted product"
                };
            }
        }

        [HttpPost("{renewalRatio?}")]
        [GzipCompression]
        public async Task<dynamic> GetRenewalRatio(RenewalRatio renewalRatio)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetRenewalRatio", renewalRatio.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == renewalRatio.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {renewalRatio.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                Core<RenewRatio> _dapper_core = new Core<RenewRatio>();
                string condition = new helpers().QueryResolver(renewalRatio);
                log.Info($"{((string.IsNullOrEmpty(condition)) ? "Admin Access" : condition)}");
                var result = await _dapper_core.GetRenewalRatio(string.Format(connectionManager.renewalsRatio, condition));
                if (result.Count() == 0)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "No result found"
                    };
                }

                var grouped_item = new helpers().Grouper(result, renewalRatio.from, renewalRatio.to);

                return new notification_response
                {
                    status = 200,
                    message = "Successful",
                    data = grouped_item
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
                    message = "Error: while getting predicted product"
                };
            }
        }

        [HttpPost("{renewalRatio?}")]
        [GzipCompression]
        public async Task<dynamic> GetNextRenewal(RenewalRatio renewalRatio)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetNextRenewal", renewalRatio.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == renewalRatio.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {renewalRatio.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                Core<NextRenewalResult> _dapper_core = new Core<NextRenewalResult>();
                string condition = new helpers().QueryResolver(renewalRatio);
                string default_query = (renewalRatio.from.HasValue && renewalRatio.to.HasValue) ? $"(enddate between '{renewalRatio.from.Value}' and '{renewalRatio.to.Value}')" : "month(enddate)=month(getdate()) and year(enddate)=year(getdate())";
                log.Info($"{((string.IsNullOrEmpty(condition)) ? "Admin Access" : condition)}");
                var condition_where = !string.IsNullOrEmpty(condition) ? $" {condition}" : " ";
                int pagesize = 100;
                int skip = (renewalRatio.page == 1) ? 0 : pagesize * (renewalRatio.page - 1);
                var result = await _dapper_core.GetRenewalNext(connectionManager.NexRenewal, default_query, condition, condition_where, skip, pagesize);
                decimal total = Convert.ToDecimal(result.TotalPages) / Convert.ToDecimal(pagesize);
                int totalpage = (int)Math.Ceiling(total);

                if (result.Results.Count() == 0)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "No result found"
                    };
                }

                var grouped_item = new helpers().Grouper2(result.Results);

                return new
                {
                    status = 200,
                    message = "Successful",
                    pageSize = pagesize,
                    totalPages = totalpage,
                    totalrecord = result.OverAllCount,
                    navigation = $"{renewalRatio.page} of {totalpage}",
                    data = grouped_item
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
                    message = "Error: while getting predicted product"
                };
            }
        }
    }
}
