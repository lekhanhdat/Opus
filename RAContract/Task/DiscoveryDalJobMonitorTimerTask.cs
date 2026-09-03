using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Contract.Task
{
    public class DiscoveryDalJobMonitorTimerTask : TaskBase
    {
        internal override TaskBase AssembleDefaultTask()
        {
            return new DiscoveryDalJobMonitorTimerTask
            {
                Id = GenerateId(),
                Schedule = new TaskSchedule()
                {
                    Id = GenerateId(),
                    Interval = 10,
                    IntervalType = TaskIntervalType.Minutes,
                },
                DisallowConcurrentExecution = true,
                NextRunTime = DateTime.UtcNow.Ticks,
                Type = TaskType.DiscoveryDalJobMonitorTimer,
            };
        }
    }
}
