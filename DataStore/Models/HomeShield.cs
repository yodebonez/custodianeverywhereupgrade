using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class HomeShield
    {
        public HomeShield()
        {

        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CustomerFullNAme { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Occupation { get; set; }
        public decimal Premium { get; set; }
        public int NoOfUnit { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime ActiveDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string IdentificationType { get; set; }
        public string TransactionReeference { get; set; }
        public string FileFormat { get; set; }
        public string ResponseFromAPI { get; set; }

        public string referralCode { get; set; }

    }
}
