using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DalServices;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl.PlanProfile;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Plan;
using AvePoint.RA.Service.RMTasks;
using Cloud.Sdk.Data.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryDalJobMonitor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMDiscoveryDalJobMonitor));
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IDalService DalService => PlatformWindsorManager.GetService<IDalService>();
        private IRMDiscoveryPlanDalJobDao PlanDalJobDao = new RMDiscoveryPlanDalJobDao();

        private static readonly HashSet<RMDalJobStatus> TerminalStatuses =
        [
            RMDalJobStatus.Completed,
            RMDalJobStatus.Failed,
            RMDalJobStatus.FinishedWithExceptions
        ];

        public async Task MonitorAsync()
        {
            try
            {
                var jobRunnings = JobMonitorDao.GetRunningJobs(new List<Contract.JobMonitor.JobType> { Contract.JobMonitor.JobType.DiscoveryDalJob });
                if (jobRunnings.Count == 0)
                {
                    logger.Info("No running RM discovery DAL jobs found.");
                    return;
                }
                if (jobRunnings.FirstOrDefault().Status == (int)JobStatus.InProgress)
                {
                    await MonitorDiscoveryDalJobMonitor(jobRunnings.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while monitoring RM discovery DAL jobs: {ex}");
            }
        }

        private async Task MonitorDiscoveryDalJobMonitor(RMJobMonitor rMJobMonitor)
        {
            var mainJobId = rMJobMonitor.Id;
            var subJobs = await PlanDalJobDao.GetJobsByMainJobIdAsync(mainJobId);

            if (subJobs == null || subJobs.Count == 0)
            {
                logger.Info($"[{nameof(MonitorDiscoveryDalJobMonitor)}] No sub-jobs found for JobMonitor:{rMJobMonitor.Id}.");
                return;
            }

            foreach (var subJob in subJobs)
            {
                await ProcessSubJobAsync(rMJobMonitor.Id, subJob);
            }

            int totalCount = subJobs.Count;
            int terminalCount = subJobs.Count(j => TerminalStatuses.Contains(j.Status));
            int progress = terminalCount * 100 / totalCount;

            JobMonitorService.UpdateJobProgress(rMJobMonitor.Id, progress);

            if (terminalCount == totalCount)
            {
                JobStatus finalStatus = GetJobMonitorStatus(subJobs);
                if (finalStatus == JobStatus.Failed || finalStatus == JobStatus.FinishWithException)
                {
                    JobMonitorService.UpdateJobStatus(rMJobMonitor.Id, finalStatus, "RM_JM_Summary_DiscoveryDalJob");
                }
                else
                {
                    JobMonitorService.UpdateJobStatus(rMJobMonitor.Id, finalStatus);
                }
                logger.Info($"[{nameof(MonitorDiscoveryDalJobMonitor)}] All sub-jobs terminal. JobMonitor:{rMJobMonitor.Id}, FinalStatus:{finalStatus}.");
            }
        }

        private async Task ProcessSubJobAsync(string jobMonitorId, RMDiscoveryPlanDalJob subJob)
        {
            if (TerminalStatuses.Contains(subJob.Status))
            {
                return;
            }

            try
            {
                JobHistoryModel dalJobHistory = await DalService.GetJobStatusAsync(subJob.DalJobId);
                logger.Info($"[{nameof(ProcessSubJobAsync)}] Retrieved DAL job status. JobMonitor:{jobMonitorId}, SubJob:{subJob.Id}, DalJobId:{subJob.DalJobId}, DalStatus:{dalJobHistory.Status}.");
                RMDalJobStatus? mappedStatus = MapDalStatus(dalJobHistory.Status);

                if (mappedStatus == null || mappedStatus == subJob.Status)
                {
                    return;
                }

                subJob.Status = mappedStatus.Value;
                if (TerminalStatuses.Contains(mappedStatus.Value))
                {
                    subJob.EndTime = DateTime.UtcNow.Ticks;
                }

                await PlanDalJobDao.AddOrUpdateJobAsync(subJob);
                logger.Info($"[{nameof(ProcessSubJobAsync)}] Updated sub-job:{subJob.Id}, DalJobId:{subJob.DalJobId}, NewStatus:{mappedStatus.Value}.");
            }
            catch (Exception ex)
            {
                logger.Error($"[{nameof(ProcessSubJobAsync)}] Failed to check DAL status. JobMonitor:{jobMonitorId}, SubJob:{subJob.Id}, DalJobId:{subJob.DalJobId}. Exception: {ex}");
            }
        }

        private static RMDalJobStatus? MapDalStatus(Cloud.Sdk.Data.Dal.JobStatus dalStatus)
        {
            return dalStatus switch
            {
                Cloud.Sdk.Data.Dal.JobStatus.Completed => RMDalJobStatus.Completed,
                Cloud.Sdk.Data.Dal.JobStatus.Failed => RMDalJobStatus.Failed,
                Cloud.Sdk.Data.Dal.JobStatus.FinishedWithExceptions => RMDalJobStatus.FinishedWithExceptions,
                Cloud.Sdk.Data.Dal.JobStatus.Timeout => RMDalJobStatus.Timeout,
                _ => null
            };
        }

        private static JobStatus GetJobMonitorStatus(IEnumerable<RMDiscoveryPlanDalJob> subJobs)
        {
            bool hasCompleted = subJobs.Any(j => j.Status == RMDalJobStatus.Completed);
            bool hasFailed = subJobs.Any(j => j.Status is RMDalJobStatus.Failed or RMDalJobStatus.Timeout);
            bool hasExceptions = subJobs.Any(j => j.Status == RMDalJobStatus.FinishedWithExceptions);

            if (hasExceptions || (hasCompleted && hasFailed))
            {
                return JobStatus.FinishWithException;
            }

            if (hasFailed && !hasCompleted)
            {
                return JobStatus.Failed;
            }

            return JobStatus.Finished;
        }
    }
}
