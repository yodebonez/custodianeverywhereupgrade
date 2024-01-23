using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class SelectedMealPlan
    {
        public SelectedMealPlan()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int mealPlanID { get; set; }
        [ForeignKey("mealPlanID")]
        public virtual MealPlan MealPlan { get; set; }
        public DateTime dateCreated { get; set; }
        public string MyMeal { get; set; }
    }
}
