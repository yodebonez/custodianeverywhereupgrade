using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class CoreModels
    {
        public CoreModels()
        {

        }
       // [Required]
        public string merchant_id { get; set; }
      //  [Required]
        public string hash { get; set; }
    }
}
