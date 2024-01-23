using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class LocalTravel
    {
        public LocalTravel()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string MobileNumber { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Address { get; set; }
        public string FromDestination { get; set; }
        public string ToDestionation { get; set; }
        public string VehicleReg { get; set; }
        public string NextofKinMoble { get; set; }
        public DateTime DepartureDate { get; set; } = DateTime.Now;
        public decimal Premium { get; set; }
        public string TransactionReference { get; set; }
        public string Status { get; set; }
        public string Narration { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string merchant_Id { get; set; }
        public string Email { get; set; }
        public string referralCode { get; set; }
    }
}
