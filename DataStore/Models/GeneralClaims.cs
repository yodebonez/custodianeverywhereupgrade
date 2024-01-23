using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class GeneralClaims
    {
        public GeneralClaims()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(200)]
        [Required]
        public string policy_holder_name { get; set; }
        [MaxLength(300)]
        [Required]
        public string policy_number { get; set; }
        [MaxLength(300)]
        [Required]
        public string policy_type { get; set; }
        [MaxLength(50)]
        public string vehicle_reg_number { get; set; }
        [MaxLength(100)]
        [Required]
        public string email_address { get; set; }
        [MaxLength(20)]
        [Required]
        public string phone_number { get; set; }
        [MaxLength(300)]
        [Required]
        public string claim_request_type { get; set; }
        [MaxLength(300)]
        public string damage_type { get; set; }
        public DateTime? incident_date_time { get; set; }
       
        public string incident_description { get; set; }
        [MaxLength(5)]
        public string witness_available { get; set; }
        [MaxLength(100)]
        public string witness_name { get; set; }
        [MaxLength(300)]
        public string witness_contact_info { get; set; }
        [MaxLength(5)]
        public string police_informed { get; set; }
        public DateTime? police_report_date_time { get; set; }
        [MaxLength(190)]
        public string police_station_address { get; set; }
        [MaxLength(30)]
        public string thirdparty_involved { get; set; }
        [MaxLength(160)]
        public string third_party_info { get; set; }
        public decimal claim_amount { get; set; }
        [MaxLength(20)]
        public string data_source { get; set; }
        [MaxLength(5)]
        public string fire_service_informed { get; set; }
        [MaxLength(5)]
        public string fire_service_report_available { get; set; }
        [MaxLength(100)]
        public string fire_service_station_address { get; set; }
        [MaxLength(250)]
        public string list_of_damaged_items { get; set; }
        public DateTime? last_occupied_date { get; set; }
        [MaxLength(200)]
        public string list_of_stolen_items { get; set; }
        [MaxLength(5)]
        public string premise_occupied { get; set; }
        [MaxLength(200)]
        public string describe_permanent_disability { get; set; }
        [MaxLength(100)]
        public string doctor_name { get; set; }
        [MaxLength(200)]
        public string hospital_address { get; set; }
        [MaxLength(100)]
        public string hospital_name { get; set; }
        [MaxLength(100)]
        public string injury_sustained_description { get; set; }
        [MaxLength(5)]
        public string is_insured_employee { get; set; }
        [MaxLength(5)]
        public string permanent_disability { get; set; }
        [MaxLength(5)]
        public string taken_to_hospital { get; set; }
        [MaxLength(200)]
        public string inspection_location { get; set; }
      
        public string list_of_items_lost { get; set; }
        [MaxLength(50)]
        public string mode_of_conveyance { get; set; }
        [MaxLength(50)]
        public string vehicle_details { get; set; }
        public virtual IList<NonLifeClaimsDocument> NonLifeDocument { get; set; }
        [MaxLength(30)]
        public string claims_number { get; set; }
        [MaxLength(30)]
        public string division { get; set; }
        public DateTime datecreated { get; set; }
        [MaxLength(10)]
        public string BranchCode { get; set; }
        public decimal adjustedamount { get; set; }
        [MaxLength(10)]
        public string Status { get; set; }
        [MaxLength(100)]
        public string merchant_id { get; set; }
    }
}
