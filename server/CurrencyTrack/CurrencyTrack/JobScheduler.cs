using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;// For Schedule Task
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace CurrencyTrack
{
    public class JobScheduler : ScheduledTask
    {
        /// <summary>
        /// The scheduleTask is called at an regular interval of 30 seconds.
        /// </summary>

        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail job = JobBuilder.Create<ScheduledTask>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                                    .WithIdentity("trigger1", "group1")
                                    .StartNow()
                                    .WithSimpleSchedule
                                        (s =>
                                            s.WithIntervalInMinutes(60)
                                            .RepeatForever()
                                         ).Build();
            scheduler.ScheduleJob(job, trigger);
        }// End method Start()
    }// End JobScheduler
}