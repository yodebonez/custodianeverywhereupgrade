using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class InterStateStocks
    {
        public InterStateStocks()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string email { get; set; }
        public DateTime createdAt { get; set; }
        public bool pushStatus { get; set; }
        public string data { get; set; }
        public DateTime? updatedAt { get; set; }
        public int stage { get; set; }
        public string accountId { get; set; }
        public string requestId { get; set; }
    }
}
