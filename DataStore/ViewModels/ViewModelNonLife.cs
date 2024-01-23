using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class ViewModelNonLife
    {
        public ViewModelNonLife()
        {

        }
       
        [Required]
        public string policy_holder_name { get; set; }
        
        [Required]
        public string policy_number { get; set; }
        
        [Required]
        public string policy_type { get; set; }
      
        public string vehicle_reg_number { get; set; }
      
        [Required]
        public string email_address { get; set; }
       
        [Required]
        public string phone_number { get; set; }
       
        [Required]
        public string claim_request_type { get; set; }
       
        public string damage_type { get; set; }
        public DateTime? incident_date_time { get; set; }
        
        public string incident_description { get; set; }
        
        public string witness_available { get; set; }
        
        public string witness_name { get; set; }
        
        public string witness_contact_info { get; set; }
        
        public string police_informed { get; set; }
        public DateTime? police_report_date_time { get; set; }
        
        public string police_station_address { get; set; }
       
        public string thirdparty_involved { get; set; }
       
        public string third_party_info { get; set; }
        public decimal claim_amount { get; set; }
       
        public string data_source { get; set; }
        
        public string fire_service_informed { get; set; }
      
        public string fire_service_report_available { get; set; }
      
        public string fire_service_station_address { get; set; }
        
        public string list_of_damaged_items { get; set; }
        public DateTime? last_occupied_date { get; set; }
        
        public string list_of_stolen_items { get; set; }
       
        public string premise_occupied { get; set; }
       
        public string describe_permanent_disability { get; set; }
      
        public string doctor_name { get; set; }
       
        public string hospital_address { get; set; }
       
        public string hospital_name { get; set; }
       
        public string injury_sustained_description { get; set; }
       
        public string is_insured_employee { get; set; }
      
        public string permanent_disability { get; set; }
       
        public string taken_to_hospital { get; set; }
       
        public string inspection_location { get; set; }
        
        public string list_of_items_lost { get; set; }
      
        public string mode_of_conveyance { get; set; }
     
        public string vehicle_details { get; set; }
        public List<document> documents { get; set; }
        public string merchant_id { get; set; }
        public string claims_number { get; set; }
        public string division { get; set; }
        public string branch { get; set; }
        public string logon_email { get; set; }
        public int? tempId { get; set; }
    }


    public class document
    {
        public document()
        {

        }
        [MaxLength(30)]
        public string extension { get; set; }
        [MaxLength(100)]
        public string name { get; set; }
        public string data { get; set; }
    }
}
