using DataStore.Models;
using DataStore.repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WindowsService1.Models;
using WindowsService1.NewsAPIJob;

namespace WindowsService1.NewsAPIJob
{
    public static class GetAllLeague
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void GetLeague()
        {
            try
            {
                log.Info("about to get league");
                store<News> newupdate = new store<News>();
                using (var api = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(Config.Authorization_Header))
                    {
                        api.DefaultRequestHeaders.Add("X-RapidAPI-Key", Config.Authorization_Header);
                        var request = api.GetAsync(Config.BASE_URL + "/leagues").GetAwaiter().GetResult();
                        if (request.IsSuccessStatusCode)
                        {
                            string[] exclude = { "England", "World", "Spain", "Italy", "Germany", "France", "Africa" };
                            var result = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var filter = Newtonsoft.Json.JsonConvert.DeserializeObject<_api>(result);
                            var select = filter.api.leagues.Where(x => x.is_current == true && x.season == DateTime.Now.Year).ToList();
                            var update = newupdate.FindOneByCriteria(x => x.Id == Config.GetID).GetAwaiter().GetResult();
                            if (update != null)
                            {
                                update.UpdateAt = DateTime.Now;
                                update.NewsFeed = Newtonsoft.Json.JsonConvert.SerializeObject(select.Where(x => exclude.Any(y => y.ToLower().Contains(x.country?.ToLower().Trim()))));
                                newupdate.Update(update).GetAwaiter().GetResult();
                            }
                        }
                    }
                    else
                    {
                        log.Info("API authorization key is null, please check your condiguration file");
                    }
                    log.Info("finished  getting league");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                // throw;
            }
        }
    }
}
