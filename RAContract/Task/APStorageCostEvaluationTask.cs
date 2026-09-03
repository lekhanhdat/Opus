using System;

namespace AvePoint.RA.Contract.Task
{
    public class APStorageCostEvaluationTask : TaskBase
    {
        internal override TaskBase AssembleDefaultTask()
        {
            return new APStorageCostEvaluationTask
            {
                Id = GenerateId(),
                Schedule = new TaskSchedule()
                {
                    Id = GenerateId(),
                    Interval = 1,
                    IntervalType = TaskIntervalType.Daily,
                },
                DisallowConcurrentExecution = true,
                NextRunTime = DateTime.UtcNow.Ticks,
                Type = TaskType.APStorageCostEvaluation,
            };
        }
    }
}
