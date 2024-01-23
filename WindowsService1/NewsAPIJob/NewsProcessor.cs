using DataStore.Models;
using DataStore.repository;
using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1.NewsAPIJob
{
    public static class NewsProcessor
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static void GetNews()
        {
            try
            {
                store<News> newupdate = new store<News>();
                log.Info("about to get latest new from news api");
                var newsApiClient = new NewsApiClient("f7b2cbdfbd3a4abc89cb81d0f03f1555");
                var articlesResponse = newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.NG,
                    PageSize = 100
                });

                if (articlesResponse.Status == Statuses.Ok)
                {
                    log.Info("new was fetched successfully");
                    var content = Newtonsoft.Json.JsonConvert.SerializeObject(articlesResponse.Articles);
                    var update = newupdate.FindOneByCriteria(x => x.Id == 1).GetAwaiter().GetResult();
                    if (update != null)
                    {
                        update.UpdateAt = DateTime.Now;
                        update.NewsFeed = content;
                        newupdate.Update(update).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                throw;
            }
        }
    }
}
