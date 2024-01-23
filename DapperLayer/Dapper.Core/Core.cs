using DapperLayer.utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Dynamic;
using DataStore.ViewModels;
using DataStore.Models;
using NLog;
using System.IO;
using System.Configuration;

namespace DapperLayer.Dapper.Core
{
    public class Core<T> where T : class
    {
        public SqlTransaction TransactionState = null;
        private static Logger log = LogManager.GetCurrentClassLogger();
        public Core()
        {

        }
        public async Task<dynamic> GetAllbyPagination(int limit, string sql)
        {
            IList<int> count = null;
            IList<T> results = null;
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var p = new { limit = limit };
                using (var multiple = await cnn.QueryMultipleAsync(sql.Trim(), p, commandTimeout: 120))
                {
                    count = multiple.Read<int>().ToList();
                    results = multiple.Read<T>().ToList();
                }
            };
            dynamic ret = new ExpandoObject();
            ret.results = results;
            ret.count = count[0];
            return ret;
        }
        public async Task<dynamic> SearchPager(int page, int skip, string terms, string sql)
        {
            IList<T> results = null;
            dynamic obj = new ExpandoObject();
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var p = new { search = terms };
                var result = await cnn.QueryAsync<T>(sql.Trim(), p);
                obj.count = result.Count();
                results = result.Skip(skip).Take(page).ToList();
            };

            obj.results = results;
            return obj;
        }
        public async Task<dynamic> GetPredictionByCustomerID(int customer_id, string sql)
        {
            IList<T> current = null;
            IList<T> recommended = null;
            dynamic obj = new ExpandoObject();
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var p = new { customer_id = customer_id };
                using (var multiple = await cnn.QueryMultipleAsync(sql.Trim(), p, commandTimeout: 120))
                {
                    recommended = multiple.Read<T>().ToList();
                    current = multiple.Read<T>().ToList();

                }
            };
            obj.current = current;
            obj.recommended = recommended;
            return obj;
        }
        public async Task<IEnumerable<T>> GetNewCustomerDetails()
        {
            try
            {
                //var p = new { start_date = start_date, end_date = end_date };
                using (var cnn = new SqlConnection(connectionManager.connectionString("CustApi2")))
                {
                    var result = await cnn.QueryAsync<T>(connectionManager._getNewCustomerSP, commandType: CommandType.StoredProcedure, commandTimeout: 120);
                    return result;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return null;
            }
        }
        public async Task<List<T>> GetRenewalRatio(string sql)
        {

            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var multiple = await cnn.QueryAsync<T>(sql.Trim(), commandTimeout: 120);
                return multiple?.ToList();
            };
        }
        public async Task<NextRenewalResult> GetRenewalNext(string query, string query_filter, string condition = "", string condition_where = "", int offset = 0, int next = 100)
        {
            try
            {
                log.Info("I got here from GetRenewalNext");
                IList<int> count = null;
                IList<int> overallcount = null;
                IList<NextRenewal> results = null;
                var result = new NextRenewalResult();
                var new_query = $@"SELECT * FROM [dbo].[Renewals_staging] where {query_filter}  {condition}
                            ORDER BY EndDate
                            OFFSET {offset} ROWS FETCH NEXT {next} ROWS ONLY OPTION(RECOMPILE);

                            SELECT Count(*) as Total FROM [dbo].[Renewals_staging]
                            where {query_filter} {condition};

                            SELECT Count(*) as Total FROM [dbo].[Renewals_staging]
                            where  {query_filter}  {condition};";


                // var new_query = string.Format(query_filter,query, condition, offset, query_filter, next, condition_where);
                log.Info($"Query to get Record: {new_query}");
                using (var cnn = new SqlConnection(connectionManager.connectionString()))
                {
                    using (var multiple = await cnn.QueryMultipleAsync(new_query, commandTimeout: 240))
                    {
                        results = (await multiple.ReadAsync<NextRenewal>()).ToList();
                        count = (await multiple.ReadAsync<int>()).ToList();
                        overallcount = (await multiple.ReadAsync<int>()).ToList();
                        result.TotalPages = count[0];
                        result.OverAllCount = overallcount[0];
                        result.Results = results.ToList();
                    }
                };
                return result;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                throw;
            }
        }
        public async Task<bool> BulkInsert(List<TravelInsurance> buyTravels)
        {
            var getProps = new TravelInsurance().GetType().GetProperties();
            List<string> col = new List<string>();
            List<string> colData = new List<string>();
            foreach (var item in getProps)
            {
                if (item.Name != "Id")
                {
                    col.Add(item.Name);
                    colData.Add("@" + item.Name);
                }
            }
            string query = $@"INSERT INTO TravelInsurance ({string.Join(",", col)}) Values ({string.Join(",", colData)})";
            bool IsSuccessful = false;
            try
            {
                var cnn = new SqlConnection(connectionManager.connectionString("CustApi2"));
                await cnn.OpenAsync();
                TransactionState = cnn.BeginTransaction();
                int affectedRow = await cnn.ExecuteAsync(query, buyTravels, TransactionState, commandType: CommandType.Text);
                if (affectedRow == buyTravels.Count())
                {
                    IsSuccessful = true;
                }

                return IsSuccessful;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return IsSuccessful;

            }
        }
        public async Task<bool> BulkUpdate(List<TravelInsurance> buyTravels)
        {
            string query = $@"Update TravelInsurance SET policy_number = @policy_number,certificate_number=@certificate_number,transaction_ref=@transaction_ref,status=@status where Id = @Id";
            var param = buyTravels.Select(x => new
            {
                policy_number = x.policy_number,
                certificate_number = x.certificate_number,
                transaction_ref = x.transaction_ref,
                Id = x.Id,
                status = x.status
            }).ToList();
            bool IsSuccessful = false;
            try
            {
                var cnn = new SqlConnection(connectionManager.connectionString("CustApi2"));
                await cnn.OpenAsync();
                TransactionState = cnn.BeginTransaction();
                int affectedRow = await cnn.ExecuteAsync(query, param, TransactionState, commandType: CommandType.Text);
                if (affectedRow == buyTravels.Count())
                {
                    IsSuccessful = true;
                }

                return IsSuccessful;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return IsSuccessful;

            }
        }
        public async Task<bool> BulkInsert(List<TravelBroker> buyTravels)
        {
            var getProps = new TravelInsurance().GetType().GetProperties();
            List<string> col = new List<string>();
            List<string> colData = new List<string>();
            foreach (var item in getProps)
            {
                if (item.Name != "Id")
                {
                    col.Add(item.Name);
                    colData.Add("@" + item.Name);
                }
            }
            string query = $@"INSERT INTO TravelInsurance ({string.Join(",", col)}) Values ({string.Join(",", colData)})";
            bool IsSuccessful = false;
            try
            {
                var cnn = new SqlConnection(connectionManager.connectionString("CustApi2"));
                await cnn.OpenAsync();
                TransactionState = cnn.BeginTransaction();
                int affectedRow = await cnn.ExecuteAsync(query, buyTravels, TransactionState, commandType: CommandType.Text);
                if (affectedRow == buyTravels.Count())
                {
                    IsSuccessful = true;
                }

                return IsSuccessful;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return IsSuccessful;

            }
        }
        public async Task<T> GetConfigTravel(string _token)
        {
            try
            {
                string query = $@"select usr.id as UserID,lin.Token,
                                                          cp.CompanyName,cp.CompanyProfileID,cp.Logo,
                                                          cp.BrokerCode,
                                                          cp.SubAccountID,cp.SubAccountID_2,cp.Category,
                                                          cp.TravelRate,cp.TravelRate_2 from [AspNetUsers] usr
                                                          inner join [TravelExternalLinks] lin
                                                          on usr.Id = lin.UserID and lin.Token = '{_token}'
                                                          inner join [CompanyProfiles] cp
                                                          on cp.CompanyProfileID = usr.CompanyProfileID";

                using (var conn = new SqlConnection(connectionManager.connectionString("Travel")))
                {
                    var p = new { token = _token };
                    var result = await conn.QueryFirstAsync<T>(query, p, commandType: CommandType.Text);
                    return result;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return null;
            }
        }
        public async Task<IEnumerable<ReferralModel>> ValidateReferralCode(string code)
        {
            try
            {
                string sql = $@"SELECT * FROM [AgentRefCode]  where AgntRefID = (select TOP(1) AgntRefID from [AgentRefCode]  WHERE LTRIM(RTRIM(AgntRefID)) = '{code}' OR LTRIM(RTRIM(Agnt_Num)) = '{code}')";
                log.Info($"query to exe {sql}");
                using (var cnn = new SqlConnection(connectionManager.connectionString("ReferralDB")))
                {
                    var result = await cnn.QueryAsync<ReferralModel>(sql.Trim());
                    return result;
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return null;
            }
        }
        public async Task<bool> DeleteRecord(string referrenceKey)
        {
            try
            {
                string query = $"DELETE FROM [TravelInsurance] WHERE group_reference = '{referrenceKey}'";
                var cnn = new SqlConnection(connectionManager.connectionString("CustApi2"));
                await cnn.OpenAsync();
                if (TransactionState == null)
                {
                    TransactionState = cnn.BeginTransaction();
                }
                int affectedRow = await cnn.ExecuteAsync(query, null, TransactionState, commandType: CommandType.Text);
                string dir = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Travel/{referrenceKey}";
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    TransactionState.Commit();
                    return true;
                }
                else
                {
                    TransactionState.Commit();
                    return true;
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                TransactionState.Rollback();
                return false;
            }
        }
        public async Task<IEnumerable<T>> GetPolicyServices(string policynumber)
        {
            try
            {
                var p = new { policy = policynumber };
                using (var cnn = new SqlConnection(connectionManager.connectionString("ReferralDB")))
                {
                    var result = await cnn.QueryAsync<T>(connectionManager.GetPolicy, param: p, commandType: CommandType.StoredProcedure);
                    return result;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return null;
            }
        }

        public async Task<AgentData> ConfirmAgentCode(string agentCode)
        {
            try
            {
                var p = new { Agnt_Num = agentCode };
                using (var cnn = new SqlConnection(connectionManager.connectionString("ReferralDB")))
                {
                    var result = await cnn.QueryFirstAsync<AgentData>(connectionManager.ConfirmAgent, param: p, commandType: CommandType.StoredProcedure);
                    return result;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return null;
            }
        }
        public async Task<IEnumerable<AgentPolicies>> GetAgentPolicies(string agent_ref_id)
        {
            try
            {
                var p = new { AgntRefID = agent_ref_id };
                using (var cnn = new SqlConnection(connectionManager.connectionString("ReferralDB")))
                {
                    var result = await cnn.QueryAsync<AgentPolicies>(connectionManager.GetAgentPolicies, param: p, commandType: CommandType.StoredProcedure);
                    return result;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return null;
            }
        }
    }
}
