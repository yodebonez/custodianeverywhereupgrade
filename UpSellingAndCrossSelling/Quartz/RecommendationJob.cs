using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpSellingAndCrossSelling.CrossSelling;

namespace UpSellingAndCrossSelling.Quartz
{

    public class RecommendationJob : IJob
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public void Execute(IJobExecutionContext context)
        {
            log.Info("about to start New RecommendationJob Jobs");
            new CrossSellingEngine().EngineProcessor();
            log.Info("Job completed");
        }
    }
}
