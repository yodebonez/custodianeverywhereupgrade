using DataStore.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class AutoInsurance
    {
        public AutoInsurance()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }
        [MaxLength(200)]
        public string customer_name { get; set; }
        [MaxLength(400)]
        public string address { get; set; }
        [MaxLength(20)]
        public string phone_number { get; set; }
        public string email { get; set; }
        [MaxLength(100)]
        public string engine_number { get; set; }
        [Required]
        public TypeOfCover insurance_type { get; set; }
        [Required]
        public decimal premium { get; set; }
       // [Required]
        public decimal sum_insured { get; set; }
        [Required]
        public string chassis_number { get; set; }
        [MaxLength(200)]
        public string registration_number { get; set; }
        [MaxLength(200)]
        public string vehicle_model { get; set; }
        [MaxLength(200)]
        public string vehicle_category { get; set; }
        [MaxLength(200)]
        public string vehicle_color { get; set; }
        [MaxLength(200)]
        public string vehicle_type { get; set; }
        [MaxLength(200)]
        public string vehicle_year { get; set; }
        [MaxLength(200)]
        [Key]
        [Column(Order = 2)]
        [Required]
        public string reference_no { get; set; }
        [MaxLength(200)]
        public string id_type { get; set; }
        public string occupation { get; set; }
        [MaxLength(200)]
        public string id_number { get; set; }
        public DateTime create_at { get; set; }
        public DateTime? dob { get; set; }
        public string attachemt { get; set; }
        [MaxLength(200)]
        public string extension_type { get; set; }
        public string payment_option { get; set; }
        public string excess { get; set; }
        public string tracking { get; set; }
        public string flood { get; set; }
        public string srcc { get; set; }
        public DateTime? start_date { get; set; }
        public string merchant_id { get; set; }
        public string referralCode { get; set; }
        public string policyNumber { get; set; }
    }
}
