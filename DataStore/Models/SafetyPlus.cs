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
    public class SafetyPlus
    {
        public SafetyPlus()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime CustomerDOB { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Occupation { get; set; }
        [Required]
        public decimal Premium { get; set; }
        [Required]
        public int NoOfUnit { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        [Required]
        public DateTime Activedate { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        [Required]
        public string Reference { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string BeneficiaryName { get; set; }
        [Required]
        public Gender BeneficiarySex { get; set; }
        [Required]
        public DateTime BeneficiaryDOB { get; set; }
        [Required]
        public string BeneficiaryRelatn { get; set; }
        //[Required]
        public string ImagePath { get; set; }
        //[Required]
        public string IdentificationType { get; set; }
       // [Required]
        public string IndetificationNUmber { get; set; }
        public string Merchant_Id { get; set; }
        public string referralCode { get; set; }

        public string policyNumber { get; set; }
    }
}
