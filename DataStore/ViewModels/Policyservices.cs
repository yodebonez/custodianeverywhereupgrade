using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class setup
    {
        public setup()
        {

        }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string policynumber { get; set; }
        [Required]
        public string hash { get; set; }
 
        public string imei { get; set; }
        //[Required]
        public string email { get; set; }
        public string devicename { get; set; }
        public string os { get; set; }
    }

    public class policyInfo
    {
        public policyInfo()
        {

        }
        public int customerid { get; set; }
        public string fullname { get; set; }
        public string policyno { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string datasource { get; set; }
        public DateTime createddate { get; set; }
        public string productdesc { get; set; }
        public string productsubdesc { get; set; }
        public string status { get; set; }
    }

    public class ValidatePolicy
    {
        public ValidatePolicy()
        {

        }
        public string merchant_id { get; set; }
        public string email { get; set; } 
        public string otp { get; set; }
        public string hash { get; set; }
        public string pin { get; set; }
        public string customerid { get; set; }
    }

    public class ValidateAgent
    {
        public ValidateAgent()
        {

        }
        public string merchant_id { get; set; }
        public string validationValue { get; set; }
        public string otp { get; set; }
        public string hash { get; set; }
        public string pin { get; set; }
        public string agent_ref_id { get; set; }
        public string validationKey { get; set; }
    }
}
