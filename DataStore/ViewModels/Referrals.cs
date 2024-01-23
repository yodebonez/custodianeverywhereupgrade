using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class Referrals
    {
        public Referrals()
        {

        }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string AgentCode { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string TransactionRef { get; set; }
    }
}
