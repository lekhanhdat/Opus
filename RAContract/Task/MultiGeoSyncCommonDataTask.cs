using System;

namespace AvePoint.RA.Contract.Task
{
    internal class MultiGeoSyncCommonDataTask : TaskBase
    {
        internal override TaskBase AssembleDefaultTask()
        {
            return new MultiGeoSyncCommonDataTask()
            {
                Id = GenerateId(),
                Schedule = new TaskSchedule
                {
                    Id = GenerateId(),
                    Interval = 1,
                    IntervalType = TaskIntervalType.Daily,
                },
                NextRunTime = DateTime.UtcNow.Ticks,
                DisallowConcurrentExecution = true,
                Type = TaskType.MultiGeoSyncCommonData,
            };
        }
    }
}
