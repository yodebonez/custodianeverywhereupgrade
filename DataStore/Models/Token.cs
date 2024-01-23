using DataStore.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class Token
    {
        public Token()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Platforms platform { get; set; }
        public DateTime datecreated { get; set; }
        public bool is_valid { get; set; }
        public bool is_used { get; set; }
        [MaxLength(6)]
        public string otp { get; set; }
        [MaxLength(20)]
        public string mobile_number { get; set; }
        [MaxLength(200)]
        public string fullname { get; set; }
        public string email { get; set; }
    }
}
