using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings
{
    [Audit]
    public class RMFileSystemSettingsCreateSubJobService: BaseContentRepositorySettingsService, IRMFileSystemSettingsCreateSubJobService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMFileSystemSettingsCreateSubJobService));
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        public string CreateAndExecuteSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool canExecuteNow, string fullPath, string settingData)
        {
            string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, canExecuteNow, fullPath, settingData);

            if (canExecuteNow)
            {
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType, subJobId),
                });
            }

            return subJobId;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSApplyClassCodeJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.ApplyClassCodeSettings4FS, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public string CreateAndExecuteSubJobWithAudit(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool canExecuteNow, string fullPath, string settingData, out string subJobId)
        {
            subJobId = CreateAndExecuteSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, canExecuteNow, fullPath, settingData);

            return jobId;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.FSMyhub, Action = AuditAction.MyhubClassify, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.MyhubClassify, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public string CreateAndExecuteMyhubSubJobWithAudit(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool canExecuteNow, string fullPath, string settingData, out string subJobId)
        {
            subJobId = CreateAndExecuteSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, canExecuteNow, fullPath, settingData);

            return jobId;
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool sendNow, string fullPath, string jobContent = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, String1 = fullPath };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList), Content = jobContent };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
    }
}
