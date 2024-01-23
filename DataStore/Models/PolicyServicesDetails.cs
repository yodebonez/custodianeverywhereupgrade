using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class PolicyServicesDetails
    {
        public PolicyServicesDetails()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string email { get; set; }
        public string phonenumber { get; set; }
        public string customerid { get; set; }
        public DateTime createdat { get; set; }
        public DateTime? updatedat { get; set; }
        public string os { get; set; }
        public string devicename { get; set; }
        public string deviceimei { get; set; }
        public string pin { get; set; }
        public bool is_setup_completed { get; set; }
        public string policynumber { get; set; }
    }
}
