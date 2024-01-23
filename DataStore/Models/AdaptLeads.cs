using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class AdaptLeads
    {
        public AdaptLeads()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [DataType(DataType.EmailAddress)]
        [MaxLength(100)]
        [Required]
        public string email { get; set; }
        [MaxLength(100)]
        [Required]
        public string fullname { get; set; }
        [Required]
        public DateTime created_at { get; set; }
        [MaxLength(200)]
        [Required]
        public string UUID { get; set; }
        public string fcm_token { get; set; }
        [MaxLength(50)]
        public string app_version { get; set; }
        [MaxLength(50)]
        public string platform { get; set; }
        public DateTime? updatedAt { get; set; }
        public DateTime? createdAt { get; set; }
    }
}
