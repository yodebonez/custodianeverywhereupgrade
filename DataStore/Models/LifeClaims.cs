using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class LifeClaims
    {
        public LifeClaims()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(20)]
        public string policy_number { get; set; }
        [MaxLength(200)]
        public string policy_holder_name { get; set; }
        [MaxLength(200)]
        public string policy_type { get; set; }
        [MaxLength(50)]
        [DataType(DataType.EmailAddress)]
        public string email_address { get; set; }
        [MaxLength(16)]
        [DataType(DataType.PhoneNumber)]
        public string phone_number { get; set; }
        [MaxLength(200)]
        public string claim_request_type { get; set; }
        public decimal claim_amount { get; set; }
        [MaxLength(160)]
        public string cause_of_death { get; set; }
        [MaxLength(200)]
        public string last_residential_address { get; set; }
        [MaxLength(200)]
        public string burial_location { get; set; }
        [MaxLength(100)]
        public string claimant_name { get; set; }
        [MaxLength(30)]
        public string claimant_relationship { get; set; }
        public bool status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public virtual IList<LifeDocument> LifeDocument { get; set; }
        [MaxLength(30)]
        public string Claim_No { get; set; }
        [MaxLength(30)]
        public string division { get; set; }
        public string BranchCode { get; set; }
        [MaxLength(10)]
        public string State { get; set; }
        [MaxLength(100)]
        public string merchant_id { get; set; }
    }
}
