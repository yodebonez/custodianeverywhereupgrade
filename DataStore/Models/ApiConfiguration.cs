using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class ApiConfiguration
    {
        public ApiConfiguration()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(20)]
        public string secret_key { get; set; }
        [MaxLength(20)]
        public string merchant_id { get; set; }
        [MaxLength(30)]
        public string merchant_name { get; set; }
        public DateTime datecreated { get; set; }
        public bool is_active { get; set; }
        public string assigned_function { get; set; }
        public string AD_client_secret { get; set; }
        public string AD_client_id { get; set; }
        public string AD_redirect_uri { get; set; }
        public bool EnableBearerAuthorization { get; set; }
    }
}

//AD_client_secret
//AD_client_id
//AD_redirect_uri
