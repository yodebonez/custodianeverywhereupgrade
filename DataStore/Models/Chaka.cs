using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class Chaka
    {
        public Chaka()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string userId { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string clientId { get; set; }
        public string mobileNumber { get; set; }
        public string firstName { get; set; }
        public string chakaId { get; set; }
        public string lastName { get; set; }
        public string role { get; set; }
        public string admin { get; set; }
        public string verified { get; set; }
        public string superAdmin { get; set; }
        public DateTime? createdAt { get; set; }
        public DateTime? modifiedAt { get; set; }
        public string password { get; set; }
        public bool isActive { get; set; }
    }
}