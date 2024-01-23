using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class LifeQuoteObject
    {
        public LifeQuoteObject()
        {

        }

        [Required]
        public decimal amount { get; set; }
        [Required]
        public Frequency frequency { get; set; }
        [Required]
        public string date_of_birth { get; set; }
        [Required]
        public int terms { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public PolicyType policy_type { get; set; }
        public decimal sum_assured { get; set; }
    }

    public class LifePolicy
    {
        public LifePolicy()
        {

        }

        [Required]
        public decimal premium { get; set; }
        [Required]
        public decimal computed_premium { get; set; }
        [Required]
        public Frequency frequency { get; set; }
        [Required]
        public string date_of_birth { get; set; }
        [Required]
        public int terms { get; set; }
        [Required]
        public string insured_name { get; set; }
        [Required]
        public string address { get; set; }
        //[Required]
        public string base64Image { get; set; }
        //[Required]
        public string base64ImageFormat { get; set; }
        [Required]
        public Gender gender { get; set; }
       // [Required]
        public string indentity_type { get; set; }
        [Required]
        public string emailaddress { get; set; }
        [Required]
        public string phonenumber { get; set; }
       // [Required]
        public string occupation { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public PolicyType policytype { get; set; }
        [Required]
        public string payment_reference { get; set; }
       // [Required]
        public string id_number { get; set; }

        public string referralCode { get; set; }

    }
}
