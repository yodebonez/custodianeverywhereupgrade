using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class TravelInsurance
    {
        public TravelInsurance()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }
        public string title { get; set; }
        [Required]
        [MaxLength(100)]
        public string surname { get; set; }
        [Required]
        [MaxLength(100)]
        public string firstname { get; set; }
        public DateTime date_of_birth { get; set; }
        [MaxLength(10)]
        [Required]
        public string gender { get; set; }
        [MaxLength(100)]
        [Required]
        public string nationality { get; set; }
        [MaxLength(20)]
        [Required]
        public string passport_number { get; set; }
        [MaxLength(100)]
        [Required]
        public string occupation { get; set; }
        [MaxLength(20)]
        [Required]
        public string phone_number { get; set; }
        [MaxLength(500)]
        [Required]
        public string address { get; set; }
        [MaxLength(50)]
        [Required]
        public string zone { get; set; }
        [MaxLength(100)]
        [Required]
        public string destination { get; set; }
        [MaxLength(100)]
        [Required]
        public string purpose_of_trip { get; set; }
        [Required]
        public DateTime depature_date { get; set; }
        [Required]
        public DateTime return_date { get; set; }
        [Required]
        public decimal premium { get; set; }
        public string transaction_ref { get; set; }
        public string multiple_destination { get; set; }
        [MaxLength(100)]
        [Required]
        public string Email { get; set; }
        [MaxLength(100)]
        [Required]
        public string merchant_id { get; set; }
        [MaxLength(100)]
        [Required]
        public string merchant_name { get; set; }
        [Required]
        [MaxLength(20)]
        public string Image_extension_type { get; set; }
        public string file_path { get; set; }
        public string group_reference { get; set; }
        public DateTime createdat { get; set; }
        public int group_count { get; set; }
        public string type { get; set; }
        public string policy_number { get; set; }
        public string certificate_number { get; set; }
        public string status { get; set; }
        public bool IsGroupLeader { get; set; }
        public bool IsGroup { get; set; }
        public string referalCode { get; set; }
    }
}
