using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class FitfamplusDeals
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }
        [MaxLength(200)]
        public string firstname { get; set; }
        [MaxLength(200)]
        public string lastname { get; set; }
        public DateTime dob { get; set; }
        [MaxLength(10)]
        public string gender { get; set; }
        [MaxLength(200)]
        public string email { get; set; }
        [MaxLength(200)]
        public string address { get; set; }
        [MaxLength(20)]
        public string mobile { get; set; }
        [MaxLength(200)]
        public string marital_status { get; set; }
        public DateTime? anniversary { get; set; }
        public int membership_id { get; set; }
        public decimal discounted_price { get; set; }
        public decimal discount_percent { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public decimal price { get; set; }
        [MaxLength(50)]
        public string gym { get; set; }
        public int package_id { get; set; }
        [MaxLength(200)]
        [Key]
        [Column(Order = 2)]
        public string reference { get; set; }
        public string deal_description { get; set; }
        public DateTime? transactionDate { get; set; }
    }
}
