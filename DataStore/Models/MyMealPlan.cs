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
    public class MyMealPlan
    {
        public MyMealPlan()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public MealPlanCategory target { get; set; }
        public Preference preference { get; set; }
        public string ageRange { get; set; }
        public GivenBirth givenBirth { get; set; }
        public MaritalStatus maritalStatus { get; set; }
        public Gender gender { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public virtual List<SelectedMealPlan> SelectedMealPlan { get; set; }
        public DateTime datecreated { get; set; }
        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledDate { get; set; }
    }
}
