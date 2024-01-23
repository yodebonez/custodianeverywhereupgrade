using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class TestController : ApiController
    {
        //[HttpGet]
        //public async Task<dynamic> GetTransactionFromTQ(string policynumber)
        //{
        //    try
        //    {
        //        string ConnectionString = ConfigurationManager.ConnectionStrings["OracleConnectionString2"].ConnectionString;
        //        string query = $@"SELECT DECODE(OPR_DRCR,'D',-1,1)*OPR_AMT OPR_AMT,TO_CHAR(OPR_RECEIPT_DATE,'DD/MM/RRRR')OPR_DATE,OPR_RECEIPT_NO,OPR_DRCR
        //                        FROM LMS_ORD_PREM_RECEIPTS WHERE OPR_POL_POLICY_NO = '{policynumber}' ORDER BY OPR_RECEIPT_DATE ASC";
        //        using (Oracle.ManagedDataAccess.Client.OracleConnection cn = new Oracle.ManagedDataAccess.Client.OracleConnection(ConnectionString))
        //        {
        //            Oracle.ManagedDataAccess.Client.OracleCommand cmd = new Oracle.ManagedDataAccess.Client.OracleCommand();
        //            await cn.OpenAsync();
        //            cmd.Connection = cn;
        //            cmd.CommandType = CommandType.Text;
        //            cmd.CommandText = query;
        //            var rows = await cmd.ExecuteReaderAsync();
        //            List<dynamic> tranx = new List<dynamic>();
        //            while (await rows.ReadAsync())
        //            {
        //                var single = new
        //                {
        //                    Amount = Convert.ToDecimal(rows["OPR_AMT"]?.ToString()),
        //                    TransactionDate = rows["OPR_DATE"]?.ToString(),
        //                    RecieptNumber = rows["OPR_RECEIPT_NO"]?.ToString(),
        //                    Status = (rows["OPR_DRCR"]?.ToString().ToUpper() == "D") ? "DR" : "CR",
        //                };

        //                tranx.Add(single);
        //            }

        //            return tranx;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //log.Info("Exception was throwed");
        //        //log.Error(ex.Message);
        //        //log.Error(ex.StackTrace);
        //        //log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
        //        //throw ex;
        //        return new
        //        {
        //            message = ex.Message,
        //            stacktrace = ex.StackTrace,
        //            innerexception = ex.InnerException?.ToString()
        //        };
        //    }
        //}
    }
}
