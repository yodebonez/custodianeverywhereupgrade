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
    public class MealPlan
    {
        public MealPlan()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(100)]
        public string daysOfWeek { get; set; }
        [MaxLength(100)]
        public string mealType { get; set; }
        [MaxLength(100)]
        public string quantity { get; set; }
        [MaxLength(100)]
        public string time { get; set; }
        [MaxLength(100)]
        public string food { get; set; }
        [MaxLength(100)]
        public string preference { get; set; }
        [MaxLength(100)]
        public string mealPlanCategory { get; set; }
        [MaxLength(300)]
        public string youTubeUrl { get; set; }
        [MaxLength(100)]
        public string common { get; set; }
        [MaxLength(100)]
        public string target { get; set; }
        [MaxLength(100)]
        public  string image { get; set; }
    }
}
