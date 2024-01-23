using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class TempClaimData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string data { get; set; }
        public DateTime createdat { get; set; }
        public string reference_id { get; set; }
        public string status { get; set; }
        public DateTime updatedat { get; set; } 
        public string email { get; set; }
    }
}
