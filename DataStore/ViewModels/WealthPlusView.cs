using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class WealthPlusView : CoreModels
    {
        [MaxLength(10)]
        public string Title { get; set; }
        [MaxLength(200)]
        [Required]
        public string FirstName { get; set; }
        [MaxLength(200)]
        [Required]
        public string Surname { get; set; }
        [MaxLength(200)]
        [Required]
        public string MiddleName { get; set; }
        [MaxLength(10)]
        [Required]
        public string Gender { get; set; }
        [MaxLength(100)]
        [Required]
        public string Email { get; set; }
        [MaxLength(14)]
        [Required]
        public string MobileNo { get; set; }
        public DateTime StartDate { get; set; }
        [MaxLength(50)]
        [Required]
        public string IndentificationType { get; set; }
        [MaxLength(20)]
        [Required]
        public string IndentificationNumber { get; set; }
        public string ImageInBase64 { get; set; }
        [MaxLength(6)]
        public string ImageFormat { get; set; }
        [Required]
        public decimal AmountToSave { get; set; }
        [MaxLength(20)]
        [Required]
        public string Frequency { get; set; }
        [Required]
        public int PolicyTerm { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public DateTime DOB { get; set; }
    }

    public class WPPCal
    {
        public WPPCal()
        {

        }

        public int status { get; set; }
        public string message { get; set; }
        public decimal projectedAmount { get; set; }
    }


    public class CoverTypeAllocation
    {
        public string cvtShtDesc { get; set; }
        public int cvtCode { get; set; }
        public int cvtSa { get; set; }
        public string cvtDesc { get; set; }
    }

    public class WealthPlusResponse
    {
        public int policyTerm { get; set; }
        public int premium { get; set; }
        public int sumInsured { get; set; }
        public int quoCode { get; set; }
        public List<CoverTypeAllocation> coverTypeAllocations { get; set; }
        public List<int> lifeCoverSumAssureds { get; set; }
        public decimal lifeCoverPremium { get; set; }
        public decimal investmentAmount { get; set; }
    }

}
