using RecurringDebitService.InternalAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecurringDebitService.BLogic
{
    public enum templateTypes
    {
        ExpireCard = 1,
        FailedDebit = 2,
        SuccessDebit = 3,
        SystemError = 4,
        EndRecurring = 5
    }

    public enum Company
    {
        Life,
        General
    }
    public class EmailData
    {
        public EmailData()
        {

        }

        public string subject { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public templateTypes _templateTypes { get; set; }
        public string toAddress { get; set; }
    }

    public static class Const
    {
        public static string USERNAME { get; } = ConfigurationManager.AppSettings["USERNAME"];
        public static string PASSWORD { get; } = ConfigurationManager.AppSettings["PASSWORD"];
        public static string PAYSTACK_ENDPOINT { get; } = ConfigurationManager.AppSettings["PAYSTACK_ENDPOINT"];
        public static string PAYSTACK_KEY { get; } = ConfigurationManager.AppSettings["PAYSTACK_KEY"];
        public static string FIREBASE_ENDPOINT { get; } = ConfigurationManager.AppSettings["FIREBASE_ENDPOINT"];
        public static string FIREBASE_TOKEN { get; } = ConfigurationManager.AppSettings["FIREBASE_TOKEN"];
        public static string MERCHANT_ID { get; } = ConfigurationManager.AppSettings["MERCHANT_ID"];
        public static string SECRET_KEY { get; } = ConfigurationManager.AppSettings["SECRET_KEY"];
        public static string CUSTODIAN_ENDPOINT { get; } = ConfigurationManager.AppSettings["CUSTODIAN_ENDPOINT"];
        public static string CUSTODIAN_AUTHORIZATION { get; } = ConfigurationManager.AppSettings["CUSTODIAN_AUTHORIZATION"];
        public static int TRIGER_TIME_HOURS_GMT
        {
            get
            {
                var hours = ConfigurationManager.AppSettings["TRIGER_TIME_HOUSE_GMT"];
                return Convert.ToInt32(hours);
            }
        }
        public static int TRIGER_TIME_SECONDS_GMT
        {
            get
            {
                var seconds = ConfigurationManager.AppSettings["TRIGER_TIME_SECONDS_GMT"];
                return Convert.ToInt32(seconds);
            }
        }
    }
    public class PaystackPayload
    {
        public PaystackPayload()
        {

        }
        public string authorization_code { get; set; }
        public string email { get; set; }
        public decimal amount { get; set; }
        public dynamic metadata { get; set; }
    }

    public class PaystackChargeResponse
    {
        public PaystackChargeResponse()
        {

        }

        public templateTypes _templateTypes { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
        public PolicyDet policyDet { get; set; }
    }

    public class PostTrx
    {
        public PostTrx()
        {

        }
        public string policy_number { get; set; }
        public string subsidiary { get; set; }
        public string payment_narrtn { get; set; }
        public string reference_no { get; set; }
        public string biz_unit { get; set; }
        public decimal premium { get; set; }
        public string merchant_id { get; set; }
        public string description { get; set; }
        public string issured_name { get; set; }
        public string phone_no { get; set; }
        public string email_address { get; set; }
        public string checksum { get; set; }
        public string status { get; set; }
        public string vehicle_reg_no { get; set; }
    }

    public class Notification
    {
        public string title { get; set; }
        public string body { get; set; }
        public int badge { get; set; }
    }

    public class Firebase
    {
        public Notification notification { get; set; }
        public string to { get; set; }
    }

}
