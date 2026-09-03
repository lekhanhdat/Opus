using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IJobProgressDao : IBaseDao<RMJobProgress>
    {
        public Task<int> GetJobProgressCountAsync(string conditionFilter, BaseJobDto jobInfo);
        public Task<IEnumerable<RMJobProgress>> GetJobProgressesAsync(int pageSize, int pageNumber, string conditionFilter, BaseJobDto jobInfo);
        public Task<RMJobProgress> GetJobProgressBySubJobIdAsync(string subJobId);
        public IAsyncEnumerable<IEnumerable<RMJobProgress>> GetJobProgressesByMainJobIdAsync(string mainJobId);
        public Task<bool> AddJobProgressAsync(RMJobProgress jobProgress);
        public Task<bool> UpdateJobProgressAsync(RMJobProgress jobProgress);
        public Task<int> ClearJobProgressesByJobIdAsync(string mainJobId);
        public Task<int> UpdateRemainingSubJobStatusAsync(string mainJobId, HashSet<int> originalStatuses, int newStatus);

        public Task<bool> BatchAddJobProgressesBySubJobsAsync(IEnumerable<RMSubJob> subJobs);
    }
}
