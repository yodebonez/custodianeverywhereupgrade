using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class AgentServices
    {
        public AgentServices()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }
        public string email { get; set; }
        public string agentname { get; set; }
        public string phonenumber { get; set; }
        public string agent_ref_id { get; set; }
        [JsonIgnore]
        public DateTime createdat { get; set; }
        [JsonIgnore]
        public DateTime? updatedat { get; set; }
        [JsonIgnore]
        public string os { get; set; }
        public string devicename { get; set; }
        [JsonIgnore]
        public string deviceimei { get; set; }
        [JsonIgnore]
        public string pin { get; set; }
        [JsonIgnore]
        public bool is_setup_completed { get; set; }
    }
}
