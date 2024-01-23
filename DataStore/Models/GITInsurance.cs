using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class GITInsurance
    {
        public GITInsurance()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(100)]
        public string insured_name { get; set; }
        public decimal premium { get; set; }
        [MaxLength(20)]
        public string category { get; set; }
        public decimal value_of_goods { get; set; }
        [MaxLength(150)]
        public string address { get; set; }
        [MaxLength(50)]
        public string email_address { get; set; }
        [MaxLength(20)]
        public string phone_number { get; set; }
        [MaxLength(150)]
        public string goods_description { get; set; }
        [MaxLength(20)]
        public string vehicle_registration_no { get; set; }
        public decimal rate_used { get; set; }
        [MaxLength(40)]
        public string policy_no { get; set; }
        [MaxLength(200)]
        public string from_location { get; set; }
        [MaxLength(200)]
        public string to_location { get; set; }
        public DateTime from_date { get; set; }
        public DateTime? to_date { get; set; }
        [MaxLength(50)]
        public string certificate_serial { get; set; }
        public DateTime date_created { get; set; }
        [MaxLength(20)]
        public string trip_completed { get; set; }
        [MaxLength(20)]
        public string Type { get; set; }

    }
}
