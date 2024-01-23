using NLog;
using Quartz;
using Quartz.Impl;
using RecurringDebitService.BLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecurringDebitService.Quartz
{
    [DisallowConcurrentExecution]
    public class JobScheduler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail Tranxjob = JobBuilder.Create<PayStackDebitJob>().Build();
            ITrigger Ttrigger = TriggerBuilder.Create()
             .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24).OnEveryDay()
               .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(Const.TRIGER_TIME_HOURS_GMT, Const.TRIGER_TIME_SECONDS_GMT))).Build();
             //.WithDailyTimeIntervalSchedule(s => s.OnEveryDay()
             //.StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(Const.TRIGER_TIME_HOURS_GMT, Const.TRIGER_TIME_SECONDS_GMT))).Build();
            // .WithSimpleSchedule(s => s.WithIntervalInMinutes(30).RepeatForever()).Build();
            //scheduler.ScheduleJob(Tranxjob, Ttrigger);
        }
        public static void Stop()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Shutdown(false);
        }
    }
}
