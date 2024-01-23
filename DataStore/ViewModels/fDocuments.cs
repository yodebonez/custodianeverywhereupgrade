using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class fDocuments : CoreModels
    {
        [Required]
        public string docType { get; set; }
        [Required]
        public string email { get; set; }
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
        [Required]
        public string policyNumber { get; set; }
        [Required]
        public subsidiary subsidiary { get; set; }
    }
}
