using NLog;
using Quartz;
using RecurringDebitService.BLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecurringDebitService.Quartz
{
    public class PayStackDebitJob : IJob
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public void Execute(IJobExecutionContext context)
        {
            log.Info("about to start paystack debit Jobs");
            new CardProcessor().RecurringEngine();
            log.Info("Paystack debit Jobs Job completed");
        }
    }
}
