using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService1.NewsAPIJob
{
    public class NewsJob : IJob
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public void Execute(IJobExecutionContext context)
        {
            log.Info("about to start New Jobs");
            NewsProcessor.GetNews();
            GetAllLeague.GetLeague();
            log.Info("Job completed");
        }
    }


    public class LeagueJob : IJob
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public void Execute(IJobExecutionContext context)
        {
            log.Info("about to start Football league get ALL league Jobs");
            GetAllLeague.GetLeague();
            log.Info("Job completed");
        }
    }

}
