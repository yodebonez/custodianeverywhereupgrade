using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
   public class WealthPlus
    {
        public WealthPlus()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(10)]
        public string Title { get; set; }
        [MaxLength(200)]
        public string FirstName { get; set; }
        [MaxLength(200)]
        public string Surname { get; set; }
        [MaxLength(200)]
        public string MiddleName { get; set; }
        [MaxLength(10)]
        public string Gender { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(14)]
        public string MobileNo { get; set; }
        public DateTime StartDate { get; set; }
        [MaxLength(50)]
        public string IndentificationType { get; set; }
        [MaxLength(20)]
        public string IndentificationNumber { get; set; }
        public string ImagePath { get; set; }
        [MaxLength(6)]
        public string ImageFormat { get; set; }
        public decimal AmountToSave { get; set; }
        [MaxLength(20)]
        public string Frequency { get; set; }
        public int PolicyTerm { get; set; }

        public string Address { get; set; }
    }
}
