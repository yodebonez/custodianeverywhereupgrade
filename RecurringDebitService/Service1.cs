using NLog;
using RecurringDebitService.BLogic;
using RecurringDebitService.Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RecurringDebitService
{
    public partial class Service1 : ServiceBase
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log.Info("========Paystack recurring debit has been turn on===========");
            JobScheduler.Start();
        }

        protected override void OnStop()
        {
            log.Info("===========Paystack recurring debit has been turn off ===============");
            new CardProcessor().SendMail(null, templateTypes.FailedDebit, "Paystack recurring service has been turned off from the windows service dialog");
            JobScheduler.Stop();
        }
    }
}
