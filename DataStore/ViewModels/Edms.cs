using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class GeneralPayload : EDMSCore
    {
        public GeneralPayload()
        {

        }

        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "PhoneNumber is required")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "PolicyNumber is required")]
        public string PolicyNumber { get; set; }
        [Required(ErrorMessage = "IncidenceDescription is required")]
        public string IncidenceDescription { get; set; }
        [Required(ErrorMessage = "IncidenceDate is required")]
        public DateTime IncidenceDate { get; set; }
        public string VehicleReg { get; set; }

        [Required(ErrorMessage = "ClaimsAmount is required")]
        public decimal ClaimsAmount { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

    }

    public class GeneralClaimStatus: EDMSCore
    {
        public GeneralClaimStatus()
        {

        }
        [Required(ErrorMessage = "CliamNumber is required")]
        public string CliamNumber { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string StatusCode { get; set; }
    }

    public class EDMSCore
    {
        public EDMSCore()
        {

        }
        [Required(ErrorMessage = "MerchantID is required")]
        public string merchant_id { get; set; }
        [Required(ErrorMessage = "Hash is required")]
        public string hash { get; set; }
    }
}
