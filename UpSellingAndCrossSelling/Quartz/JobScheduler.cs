using NLog;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpSellingAndCrossSelling.Quartz
{
    [DisallowConcurrentExecution]
    public class JobScheduler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static void Start()
        {
            log.Info("about to start scheduler");
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            //News Scheduler
            IJobDetail Tranxjob = JobBuilder.Create<RecommendationJob>().Build();
            ITrigger Ttrigger = TriggerBuilder.Create().WithCronSchedule("0 30 00 14,27 1/1 ? *").Build();
            scheduler.ScheduleJob(Tranxjob, Ttrigger);
            //Football League Scheduler
            //IJobDetail League = JobBuilder.Create<LeagueJob>().Build();
            //ITrigger TLeague = TriggerBuilder.Create()
            //    .WithSimpleSchedule(s => s.WithIntervalInMinutes(60).RepeatForever()).Build();
            //scheduler.ScheduleJob(League, TLeague);
            log.Info("scheduler started successfully");
        }
        public static void Stop()
        {
            log.Info("about to stop scheduler");
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Shutdown(false);
            log.Info("scheduler started successfully");
        }
    }
}
