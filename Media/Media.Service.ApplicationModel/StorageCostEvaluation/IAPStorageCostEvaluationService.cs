using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace AvePoint.Media.Service
{
    public interface IAPStorageCostEvaluationService : IAsyncDisposable
    {
        void Open(APStorageCostEvaluationJobInfo jobInfo, Action<JMArchiverRententionJobDetails> reportAction);
        Task EvaluateAsync(string jobId, JobType jobType);
    }
}
