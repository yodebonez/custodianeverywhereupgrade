using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class RequestDocument
    {
        public RequestDocument()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string DocType { get; set; }
        public string Email { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime DateRequested { get; set; }
        public string Division { get; set; }
        public string DivisionEmail { get; set; }
    }

    public class DivisonsCode
    {
        public DivisonsCode()
        {

        }

        public string name { get; set; }
        public string code { get; set; }
    }
}
