using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class SessionTokenTracker
    {
        public SessionTokenTracker()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int uuid { get; set; }
        public string jwt { get; set; }
        public DateTime createdat { get; set; }
        public DateTime expiresin { get; set; }
        public DateTime refreshat { get; set; }
        public bool isactive { get; set; }

        public string sessionid { get; set; }
    }
}
