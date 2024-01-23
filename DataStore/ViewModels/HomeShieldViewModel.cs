using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class HomeShieldViewModel : CoreModels
    {
        [Required]
        public string CustomerFullName { get; set; }
        [Required]
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Occupation { get; set; }
        [Required]
        public decimal Premium { get; set; }
        [Required]
        public int NoOfUnit { get; set; }
        [Required]
        public DateTime ActivationDate { get; set; }
        [Required]
        public List<ItemsList> ItemsDescription { get; set; }
        [Required]
        public string AttachementFormat { get; set; }
        [Required]
        public string AttachementInBase64 { get; set; }
        [Required]
        public IndetificationTypes IdentificationType { get; set; }
        [Required]
        public string TransactionReference { get; set; }
        public string referralCode { get; set; }
    }

    public enum IndetificationTypes
    {
        InternationalPassport = 1,
        DriversLicense = 2,
        VotersIdCard = 3,
    }

    public class ItemsList
    {
        public ItemsList()
        {

        }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public decimal ItemValue { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
