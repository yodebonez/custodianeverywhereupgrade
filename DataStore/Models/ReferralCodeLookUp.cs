using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class ReferralCodeLookUp
    {
        public ReferralCodeLookUp()
        {
            TransactionDate = DateTime.Now;
        }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ProductName { get; set; }
        public double Amount { get; set; }
        public string CustomerName { get; set; }
        public string AgentCode { get; set; }
        public string TransactionRef { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}

