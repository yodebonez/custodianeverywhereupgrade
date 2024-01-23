using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class ListOfGyms
    {
        public ListOfGyms()
        {

        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string GymName { get; set; }
        public string GynKey { get; set; }
        public string LoginEndPoint { get; set; }
        public string CheckInEndPoint { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
