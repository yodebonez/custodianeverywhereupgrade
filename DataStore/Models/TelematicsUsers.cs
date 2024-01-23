using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.ViewModels;
namespace DataStore.Models
{
    public class TelematicsUsers
    {
        public TelematicsUsers()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string OwnerName { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public bool IsFromCustodian { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime? LastLoginDate { get; set; }
        public string LoginLocation { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public string password { get; set; }
    }
}
