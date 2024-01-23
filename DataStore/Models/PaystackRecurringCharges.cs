using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class PaystackRecurringCharges
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string authorization_code { get; set; }
        public string card_type { get; set; }
        public string last4 { get; set; }
        public string exp_month { get; set; }
        public string exp_year { get; set; }
        public string bin { get; set; }
        public string bank { get; set; }
        public string channel { get; set; }
        public string signature { get; set; }
        public bool reusable { get; set; }
        public string country_code { get; set; }
        public DateTime date_added { get; set; }
        public bool is_active { get; set; }
        public string policy_number { get; set; }
        public string product_name { get; set; }
        public string recurring_freqency { get; set; }
        public DateTime recurring_start_month { get; set; }
        public string customer_email { get; set; }
        public string customer_name { get; set; }
        public string card_unique_token { get; set; }
        public string merchant_id { get; set; }
        public DateTime? card_cancel_date { get; set; }
        public DateTime recurring_end_month { get; set; }
        public int number_of_attempt_success { get; set; }
        public int number_of_attempt_failed { get; set; }
        public DateTime? last_attempt_date { get; set; }
        public string reocurrance_state { get; set; }
        public decimal Amount { get; set; }
        public string subsidiary { get; set; }
        public string vehicle_reg { get; set; }
        public  DateTime? last_run_date { get; set; }
    }
}
