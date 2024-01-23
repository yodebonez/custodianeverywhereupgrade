using DataStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DataStore.Utilities
{
    public class TravelUtility
    {
        public TravelUtility()
        {

        }

        public async Task<List<WorkAroundQuote>> GetAroundQuoteCountryAsync()
        {
            try
            {
                var country_file = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/Country.json"));
                var country_list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorkAroundQuote>>(country_file);
                return country_list;
            }
            catch (Exception)
            {

                return null;
            }
        }
    }
}
