using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class LifeInsurance
    {
        public LifeInsurance()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }
        [Required]
        public decimal premium { get; set; }
        [Required]
        public decimal computed_premium { get; set; }
        [Required]
        public string frequency { get; set; }
        [Required]
        public DateTime date_of_birth { get; set; }
        [Required]
        public int terms { get; set; }
        [Required]
        public string insured_name { get; set; }
        [Required]
        public string address { get; set; }
        //[Required]
        public string pathname { get; set; }
       // [Required]
        public string base64ImageFormat { get; set; }
        [Required]
        public string gender { get; set; }
       // [Required]
        public string indentity_type { get; set; }
        [Required]
        public string emailaddress { get; set; }
        [Required]
        public string phonenumber { get; set; }
       // [Required]
        public string occupation { get; set; }
        [Required]
        public string policytype { get; set; }
       // [Required]
        public string id_number { get; set; }
        [Required]
        [Key]
        [Column(Order = 2)]
        public string reference { get; set; }

        public string merchant_id { get; set; }

        public string referralCode { get; set; }
    }
}
