using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using Hangfire;
using System.Configuration;
using NLog;
using DataStore.Utilities;
using Hangfire.Console;
using Hangfire.Server;

[assembly: OwinStartup(typeof(CustodianEveryWhereV2._0.Startup))]

namespace CustodianEveryWhereV2._0
{
    public partial class Startup
    {
        // private static Logger log = LogManager.GetCurrentClassLogger();
        public void Configuration(IAppBuilder app)
        {
            //GlobalConfiguration.Configuration
            //       .UseSqlServerStorage(ConfigurationManager.ConnectionStrings["CustApi"].ConnectionString)
            //       .UseConsole();
            //app.UseHangfireDashboard();
            //app.UseHangfireServer();
            //GlobalConfiguration.Configuration.UseNLogLogProvider();
             ConfigureAuth(app);
            //RecurringJob.AddOrUpdate(() => new cron().logTimer(null), Cron.Minutely());
        }
    }
}
