using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class PaystackRecurringDump
    {
        public PaystackRecurringDump()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string policynumber { get; set; }
        public string productname { get; set; }
        public string logonemail { get; set; }
        public string coresystememail { get; set; }
        public string paystackrawdump { get; set; }
        public DateTime dumpdate { get; set; }
        public bool dumpstate { get; set; }
        public string dumpmessage { get; set; }
    }
}
