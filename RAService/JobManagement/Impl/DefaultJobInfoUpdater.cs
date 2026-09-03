/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using Aspose.Page.XPS.XpsMetadata;
using Aspose.Pdf.Operators;
using Aspose.Words.XAttr;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AzureFileShare;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model.QueryRequest;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMEmail;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Util;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.DisposalStubDao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using Castle.Core.Resource;
using Cloud.Sdk.Data.Cop.SMP;
using DocumentFormat.OpenXml.Wordprocessing;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Service.JobManagement.Impl
{
    public class DefaultJobInfoUpdater : IJobInfoUpdater
    {
        private RALogger logger = RALogger.GetInstance(typeof(DefaultJobInfoUpdater));
        public Dictionary<int, IJobStateHandler> HandlerDic { set; get; }
        private IJobMonitorDao JobDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        private IJobDetailService JobDetailService => PlatformWindsorManager.GetService<IJobDetailService>();

        private IRMEmailManagementService EmailManagementService => PlatformWindsorManager.GetService<IRMEmailManagementService>();

        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IFSConnectionRelatedJobInfoDao FSConnectionRelatedJobInfoDao => PlatformWindsorManager.GetService<IFSConnectionRelatedJobInfoDao>();
        private IRMAzureFileSettingsService AzureFileSettingsService => PlatformWindsorManager.GetService<IRMAzureFileSettingsService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        private IRMAzureFileShareConnectionGroupService AzureFileShareConnectionGroupService => PlatformWindsorManager.GetService<IRMAzureFileShareConnectionGroupService>();
        private IExportSettingsDao ExportSettingsDao => (IExportSettingsDao)PlatformWindsorManager.GetService(typeof(IExportSettingsDao));
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IMultiGeoSettingService _multiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        private IRMStubFileRecordDao StubFileRecordDao => PlatformWindsorManager.GetService<IRMStubFileRecordDao>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateJobProgress(string jobId, double progress)
        {
            try
            {
                logger.Info("Handle update job {0}, progress {1}, memory used {2}", jobId, progress, ProcessUtil.GetProcessMemoryMB());
            }
            catch (Exception e)
            {
                logger.Warn($"Get memory used failed, error {e.ToString()}");
                logger.Info("Handle update job {0}, progress {1}", jobId, progress);
            }
            if (JobServiceUtility.IsSubJob(jobId))
            {
                HandleSubJobProgress(jobId, progress);
            }
            else
            {
                HandelIndependentJobProgress(jobId, progress);
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateJobState(string jobId, int jobState, string comments = null)
        {
            logger.Info("Handle update job {0}, state {1}", jobId, jobState);
            if (JobServiceUtility.IsSubJob(jobId))
            {
                HandleSubJobStateAsync(jobId, jobState, comments).Wait();
            }
            else 
            {
                HandleIndependentJobState(jobId, jobState, comments);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateJobTime(string jobId, bool isStartTime)
        {
            logger.Info("Handle update job time {0}, isStart time {1}", jobId, isStartTime);
            SubJobDao.UpdateJobTime(jobId, isStartTime);
        }

        private void HandleSubJobProgress(string jobId, double progress) 
        {
            RMSubJob subJob = SubJobDao.GetSubJob(jobId);
            bool success = false;
            double increment;
            if (subJob == null)
            {
                return;
            }
            if (progress >= 100)
            {
                progress = 99;
            }
            if (JobServiceUtility.IsFinalState(subJob.Status))
            {
                logger.Info($"job {jobId} status is final state {subJob.Status}, no need to update progress.");
                success = SubJobDao.UpdateProgress(jobId, 100, DateTime.UtcNow.Ticks);
                //当subjob结束，需要用100减去DB里面当前的subjob进度外的incremental更新到主job进度里面，这样无论subjob进度是否准确，主job进度是没问题的.例如subjob进度原本是35%结束，我们需要把(100-35)65%的进度更新到主job上面.
                increment = JobServiceUtility.CalcMainJobProgressIncrement(subJob.Weight, 100 - subJob.Progress);
            }
            else
            {
                success = SubJobDao.UpdateProgress(jobId, progress, DateTime.UtcNow.Ticks);
                increment = JobServiceUtility.CalcMainJobProgressIncrement(subJob.Weight, progress - subJob.Progress);
            }
            //先直接更新子job的进度和时间戳

            logger.Info("update sub job {0} progress, success ? {1} ", jobId, success);
            if (success)
            {
                //级联更新主job的进度和时间戳
                //todo
                logger.Info("sub job weight {0}, main job progress increment {1}", subJob.Weight, increment);
                RMJobMonitor mainJob = JobDao.GetJob(subJob.ParentId);
                if (JobServiceUtility.IsFinalState(mainJob.Status))
                {
                    logger.Info("Main job {0} status is {1}, no need to update progress.", mainJob.Id, mainJob.Status);
                    return;
                }
                double mainJobProgress = mainJob.DoubleProgress + increment;
                int intProgress = Convert.ToInt32(mainJobProgress);
                if (intProgress >= 100)
                {
                    intProgress = 99;
                }
                bool isOK = SubJobDao.CascatMainJobProgress(subJob.ParentId, intProgress, mainJobProgress);
                logger.Info("Cascate main job {0} progress {1}, success ? {2} ", mainJob.Id, intProgress, isOK);
            }
            else
            {
                logger.Warn("update progress failed, no db row affect.");
            }
        }

        private void HandelIndependentJobProgress(string jobId, double progress) 
        {
            JobDao.UpdateJob(jobId, (int)progress);
        }

        private int ValidateSubJobState(int newState, RMSubJob subJob)
        {

            IStatesObject oldState = JobServiceUtility.GetStateObject(subJob.Status);
            int valiateResult = oldState.validateState(newState);
            logger.Info("Comming state {0}, old state {1}; valiate result:{2}", newState, subJob.Status, valiateResult);
            return valiateResult;
        }

        private async System.Threading.Tasks.Task HandleSubJobStateAsync(string jobId, int jobState, string comments = null) 
        {
            RMSubJob subJob = SubJobDao.GetSubJob(jobId);
            if (subJob == null)
            {
                return;
            }
            if (JobServiceUtility.IsFinalState(subJob.Status))
            {
                logger.Info("job {0} status is final state {1}, no need to update status.");
                return;
            }
            int stateValidateResult = ValidateSubJobState(jobState, subJob);
            //Currently, subjob comment max length is 1000, add the temp logic before upgrade.
            int commentMaxDBLength = 1000;
            if (!string.IsNullOrEmpty(comments) && comments.Length >= commentMaxDBLength)
            {
                logger.Info($"HandleSubJobStateAsync.Comment Length large than max db length so Substring.Comment:{comments}.");
                comments = comments.Substring(0, commentMaxDBLength);
            }
            //TODO subjob.Status == stateValidateResult时候，是否需要继续执行
            long currentTicks = DateTime.UtcNow.Ticks;
            bool success = SubJobDao.UpdateStatus(jobId, stateValidateResult, currentTicks, comments);
            if (success)
            {
                // subjob status changed, so can calculate main job status here
                int mainjobState = CalculateParentJobState(subJob.ParentId);
                subJob.Status = stateValidateResult;

                bool isVEOrule = false;
                bool isVeoEnabled = false;
                string veoParam = string.Empty;
                if (JobServiceUtility.IsFinalState(mainjobState) 
                    && JobTypeConstants.NeedToCheckVEORuleJobType.Contains((JobType)subJob.JobType))
                {
                    logger.Info($"main job should be final state: {mainjobState}, Initialize Merge Veo Info");
                    InitializeMergeVeoInfo(subJob, ref veoParam, ref isVEOrule, ref isVeoEnabled);
                }
                if (JobServiceUtility.IsFinalState(stateValidateResult))
                {
                    logger.Info($"Sub job {jobId} has final state {stateValidateResult}, merge details for sub job.");
                    JobReportShardHelper.MergeDetailsForSubJob(subJob.Id, subJob.JobType);
                    logger.Info($"Sub job {jobId} merge details finished, check if need to write job details for new job details job.");

                    if (JobServiceUtility.NewJobDetailsJobs.Contains(subJob.JobType) && JobDao.GetJobById(subJob.ParentId).JobVersion == JobVersion.UnMerged)
                    {
                        var mainJobInfo = new BaseJobDto() { Id = subJob.ParentId, JobType = subJob.JobType };
                        var subJobInfo = new BaseJobDto()
                        {
                            Id = subJob.Id,
                            JobType = subJob.JobType,
                            Status = subJob.Status,
                            ScopeId = subJob.String1,
                            Comment = subJob.Comment,
                        };
                        logger.Info($"Start to write job details for sub job {subJob.Id} with skipped merge.");
                        JobDetailService.InsertMainJobDetails(subJobInfo, mainJobInfo);
                        logger.Info($"End to write job details for sub job {subJob.Id} with skipped merge.");
                    }
                }

                bool temp = await cascadeMainJobStateAsync(subJob, mainjobState);

                if (JobServiceUtility.IsFinalState(stateValidateResult))
                {
                    logger.Info($"Sub job {jobId} has final state {stateValidateResult}, check if need to remove stub records.");
                    RemoveStubRecords();
                }

                //if (JobServiceUtility.IsFinalState(stateValidateResult) && !JobServiceUtility.IsFinalState(mainjobState) && mainjobState != (int)JobStatus.Calculating)
                //{
                //主job没完成, 有可能还有子job在Waiting, 查询有结果的话, 更成Runnable
                //移除此逻辑，在加入Job priority 后，需要WaitingSubJobTaskExecutor来处理等待的subjob
                // StartNextWaitingSubJob(subJob.ParentId, jobId, stateValidateResult);
                //string nextSubjobId = SubJobDao.GetOneWaitingSubJobId(subJob.ParentId);
                //if (nextSubjobId != null)
                //{
                //    logger.Info($"Sub job {jobId}, state {stateValidateResult}; Get next runnable sub job {nextSubjobId}");
                //    bool canRun = SubJobDao.UpdateRunable(nextSubjobId, RecordsConstants.SubJob_Runnable_CanRun);
                //}
                //}
                if (temp && JobServiceUtility.IsFinalState(mainjobState))
                {
                    if (subJob.JobType == (int)JobType.PhysicalSetPermission)
                    {
                        var runningJobIds = SubJobDao.GetRunningSetPermissionJobIds(subJob.Id);
                        if (runningJobIds != null && runningJobIds.Count > 0)
                        {
                            logger.Info("already has another permission job running, job id {0}", string.Join(",", runningJobIds));
                        }
                        else
                        {
                            string otherJobId = SubJobDao.GetOtherOneWaitingPermissionSubJobId(subJob.ParentId);
                            if (otherJobId != null)
                            {
                                bool canRun = SubJobDao.UpdateRunable(otherJobId, RecordsConstants.SubJob_Runnable_CanRun);
                                logger.Info("notify other permission job to run, job id {0}", otherJobId);
                            }
                            else
                            {

                            }
                            logger.Info("Permission job finish, no other waiting job to run.");
                        }
                    }
                    logger.Info($"jobtype is:{subJob.JobType}");

                    #region run VEO Merge job
                    // current logic still VeoMerge job when this 2 flags is true even if veoParam is empty so no need check veoParam for now
                    if (isVEOrule && isVeoEnabled /* && !string.IsNullOrEmpty(veoParam)*/)
                    {
                        logger.Info("Start to enqueue VEO Merge job");
                        try
                        {
                            JobQueueDto jqDto = new JobQueueDto()
                            {
                                JobType = JobType.VeoMerge,
                                JobRunType = JobRunBy.Schedule,
                                TenantGroupId = TenantLocalValue.LogonGroupId,
                                JobRunByUser = "RM_TS_RunSchedule",
                                Parameters = veoParam,
                            };

                            string id = JobQueueService.AddToDBJobQueue(jqDto);
                            if (string.IsNullOrEmpty(id))
                            {
                                logger.Warn("VEOMerge failed ,because id is null");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("error occurred while running VEO job,ERROR:{0}", ex.ToString());
                        }
                    }
                    else
                    {
                        logger.Info("Skip VEOMerge enqueue, IsVEOrule:{0}, IsEnabledVEO:{1}", isVEOrule, isVeoEnabled);
                    }
                    #endregion

                    if (EmailManagementService.CheckJobNeedSendEmail(subJob.ParentId))
                    {
                        EmailManagementService.SendEmailJobMessageToQueue(subJob.ParentId);
                    }

                    await SendMLManualEmailAsync(subJob.ParentId);

                    logger.Info("Job {0} has final state, process handler", subJob.ParentId);
                }
            }
            else
            {
                logger.Warn("update sub job satus failed, no db row affect.");
            }
        }

        private int CalculateParentJobState(string parentId)
        {
            int mainjobState;
            var mainJob = JobDao.GetJobById(parentId);
            var currentMainJobStatus = mainJob?.Status ?? -1;
            logger.Info($"Main job {parentId} current status is {currentMainJobStatus} before cascading status.");
            bool hasInProgressSubJob = SubJobDao.HasInProgressSubJobByParent(parentId);
            if (hasInProgressSubJob && currentMainJobStatus != (int)JobStatus.Stopping)
            {
                mainjobState = (int)JobStatus.InProgress;
            }
            else
            {
                List<int> allStatus = SubJobDao.GetAllStatesByParent(parentId);
                mainjobState = CalcMainJobState(allStatus);
            }

            return mainjobState;
        }

        private void InitializeMergeVeoInfo(RMSubJob subJob, ref string veoParam, ref bool isVEOrule, ref bool isVeoEnabled)
        {
            isVEOrule = RMRunningJobRuleMappingDao.HasVEORule(TenantLocalValue.LogonGroupId, subJob.ParentId);
            if (!isVEOrule) return; // no need to check VEO enabled if no VEO rule

            isVeoEnabled = IsEnabledVEO();
            if (!isVeoEnabled) return; // no need to get subjob ids if VEO is not enabled

            if (subJob.JobType == (int)JobType.TeamsArchiverBackup || subJob.JobType == (int)JobType.TeamsRecordsDisposal || subJob.JobType == (int)JobType.SpecifyTeamsArchiverBackup)
            {
                // only get virtual subjobs for teams job
                veoParam = SerializerHelper.SerializeByDataContractSerializer(SubJobDao.GetAllExcludeSubJobIds(subJob.ParentId, null));
            }
            else
            {
                veoParam = SerializerHelper.SerializeByDataContractSerializer(SubJobDao.GetAllSubJobIds(subJob.ParentId, null));
            }
        }

        private bool IsEnabledVEO()
        {
            bool enabled = false;
            try
            {
                if (VEOV3CommonMethod.HasUpgradedVEOV3()) 
                {
                    //Not support merge VEO for v3.
                    return false;
                }
                var condition = (Func<RMCPExportSetting, bool>)(s => s.VEOContent == null && s.VEOHistory == null && s.ArchiverSetting != null); 
                var exportSetting = ExportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(condition); //VEO v2 export setting.
                if (exportSetting != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(exportSetting.ArchiverSetting);
                    enabled = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                }
                else
                {
                    var unZipFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "VEO Configuration Files");
                    using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverSettings.config"), FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(sr.ReadToEnd());
                            enabled = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("check enable VEO error {0}", e.ToString());
            }
            return enabled;
        }
        private void StartNextWaitingSubJob(string mainJobId, string jobId, int stateValidateResult, int retryTime = 0)
        {
            var nextSubJob = SubJobDao.GetOneWaitingSubJob(mainJobId);
            if(nextSubJob == null)
            {
                return;
            }

            if(RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.Contains((JobType)nextSubJob.JobType)
                && !string.IsNullOrWhiteSpace(nextSubJob.O365TenantId))
            {
                logger.Info($"Current job [{(JobType)nextSubJob.JobType}] used o365 sub job control logic.");
                return;
            }

            if((JobType)nextSubJob.JobType == JobType.DiscoveryJob)
            {
                logger.Info($"Current job [{JobType.DiscoveryJob}] used self job control logic.");
                return;
            }

            logger.Info($"Sub job {jobId}, state {stateValidateResult}; Get next runnable sub job {nextSubJob.Id}");
            bool canRun = SubJobDao.UpdateRunable(nextSubJob.Id, RecordsConstants.SubJob_Runnable_CanRun);
            if (!canRun && retryTime < 2)
            {
                logger.Info($"Update waiting sub job failed, sub job id:{nextSubJob.Id}, retry to update next job, retry count:{retryTime}");
                StartNextWaitingSubJob(mainJobId, jobId, stateValidateResult, ++retryTime);
            };
        }

        private void HandleIndependentJobState(string jobId, int jobState, string comments = null) 
        {
            JobDao.UpdateJob(jobId, (JobStatus)jobState, comments);

            if(JobServiceUtility.IsFinalState(jobState))
            {
                TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.RunJob, new List<object> { jobId });
                TelemetryContext.FlushAsync().GetAwaiter().GetResult();
            }
        }

        private async Task<bool> cascadeMainJobStateAsync(RMSubJob subJob, int mainjobState)
        {
            bool cascatMainJoStatus = false;
            string parentId = subJob.ParentId;
            int jobType = subJob.JobType;
            string subJobId = subJob.Id;
            try
            {
                // need process site metrics export file before update main job to final state
                if (jobType == (int)JobType.ExportSiteMetrics && JobServiceUtility.IsFinalState(mainjobState))
                {
                    await HandleSiteMetricsExportAsync(parentId);
                }
                else if (mainjobState == (int)JobStatus.Finished || mainjobState == (int)JobStatus.FinishWithException)
                {
                    if (jobType == (int)JobType.AzureFileShareDataSynchronisation)
                    {
                        try
                        {
                            var scopeId = JobDao.GetJob(parentId).ScopeId;
                            await AzureFileSettingsService.ResetSyncSettingAsync(new Guid(scopeId));
                        }
                        catch(Exception e)
                        {
                            logger.Error($"An error occurred while reset azure file sync job [{parentId}] node setting. Error: {e}");
                        }
                    }
                    else if(jobType == (int)JobType.AzureFileShareDataSynchronisationSchedule)
                    {
                        try
                        {
                            var groupIds = (await AzureFileShareConnectionGroupService.GetAllAsync()).Select(item => item.Id).ToList();
                            await groupIds.ForEachAsync(item => AzureFileSettingsService.ResetSyncSettingAsync(item));
                        }
                        catch(Exception e)
                        {
                            logger.Error($"An error occurred while reset azure file sync schedule job [{parentId}] node setting. Error: {e}");
                        }
                    }

                    if (jobType == (int)JobType.FSDataSynchronization || jobType == (int)JobType.ImportFSSetting || jobType == (int)JobType.ApplyClassCode)
                    {
                        try
                        {
                            var result = RMFileSystemSettingsService.ResetApplyExistingOptionForRealTimeJob(subJobId);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurre while ResetApplyExistingOption for real time job. Id: {0} Error:{1}", subJobId, e.ToString());
                        }
                    }
                    else if (jobType == (int)JobType.FSDataSynchronizationSchedule)
                    {
                        try
                        {
                            var result = await RMFileSystemSettingsService.ResetApplyExistingOptionForScheduleJobAsync(subJobId);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurre while ResetApplyExistingOption for schedule job. Id: {0} Error:{1}", subJobId, e.ToString());
                        }
                    }
                    if (jobType == (int)JobType.BCSTermUsageReport
                        || jobType == (int)JobType.EXOTermUsageReport
                        || jobType == (int)JobType.CreateAndDestroyedFileReport
                        || jobType == (int)JobType.EXOCreateAndDestroyedFileReport
                        || jobType == (int)JobType.RetiredTermReport
                        || jobType == (int)JobType.EXORetiredTermUsageReport
                        || jobType == (int)JobType.OrphanedTermReport
                        || jobType == (int)JobType.EXOOrphanedTermUsageReport
                        || jobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                        || jobType == (int)JobType.TeamsCreateAndDestroyedFileReport
                        || jobType == (int)JobType.TeamsBCSTermUsageReport
                        || jobType == (int)JobType.TeamsOrphanedTermUsageReport
                        || jobType == (int)JobType.TeamsRetiredTermUsageReport)
                    {
                        if (!JobTypeConstants.EXOReportTypes.Contains(jobType))
                        {
                            cascatMainJoStatus = SubJobDao.CascatMainJoStatus(parentId, (int)JobStatus.Calculating);
                            if (cascatMainJoStatus)
                            {
                                IJobStateHandler handler = GetStateHandler(jobType);
                                handler.BeforeHandleState(parentId, jobType, mainjobState);
                            }
                            await HandleFinalStateAsync(mainjobState, jobType, parentId, cascatMainJoStatus);
                            return cascatMainJoStatus;
                        }
                    }
                    if (jobType == (int)JobType.APStorageCostEvaluation)
                    {
                        cascatMainJoStatus = SubJobDao.CascatMainJoStatus(parentId, mainjobState);
                        await HandleFinalStateAsync(mainjobState, jobType, parentId, cascatMainJoStatus);
                        return cascatMainJoStatus;
                    }
                }
                cascatMainJoStatus = true;
                await UpdateFSConnectionRelatedJobInfoAsync(subJob);
                await HandleFinalStateAsync(mainjobState, jobType, parentId, cascatMainJoStatus);
                bool temp = SubJobDao.CascatMainJoStatus(parentId, mainjobState);
                return temp;
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                mainjobState = -1;
                return false;
            }
            finally
            {
                if (cascatMainJoStatus && JobServiceUtility.IsFinalState(mainjobState))
                {
                    logger.Info($"flushing Telemetry records to cloud telemetry when job: {parentId} in final state: {(JobStatus)mainjobState}");
                    await TelemetryContext.FlushAsync();
                }
            }
        }

        private async Task UpdateFSConnectionRelatedJobInfoAsync(RMSubJob subJob)
        {
            try
            {
                if (!FSHighPerformanceUtility.IsEnabledJPMCFileSystemFeature()) return;

                if (!JobServiceUtility.FSConnectionRelatedJobTypes.Contains(subJob.JobType)) return;

                var mainJobId = subJob.ParentId;

                if (JobServiceUtility.IsFinalFailureState(subJob.Status))
                {
                    (bool isJobRunAtConnectionLevel, FSConnection fsConnection) = await ResolveConnection(subJob);
                    if(fsConnection == null)
                    {
                        logger.Warn($"Can not find related FS connection for sub job {subJob.Id}, job type {subJob.JobType}, job status {subJob.Status}.");
                        return;
                    }
                    var FSConnectionRelatedJobInfo = new FSConnectionRelatedJobInfo
                    {
                        FolderPath = isJobRunAtConnectionLevel ? string.Empty : subJob.String1,
                        ConnectionId = fsConnection.Id,
                        ConnectionPath = fsConnection.UNCPath,
                        ConnectionGroupId = fsConnection.GroupId,
                        ConnectionGroupName = fsConnection.GroupName,
                        JobId = subJob.ParentId,
                        JobType = subJob.JobType,
                        Status = subJob.Status,
                        Comment = subJob.Comment,
                    };
                    await UpdateFSRelatedFailureJobAsync(FSConnectionRelatedJobInfo);
                }

                if (JobServiceUtility.IsFinalState(subJob.Status))
                {
                    if (await ResolveConnection(subJob) is (_, FSConnection fsConnection) && fsConnection != null &&
                        (subJob.JobType == (int)JobType.FSDataSynchronization 
                        || subJob.JobType == (int)JobType.FSDataSynchronizationSchedule))
                    {
                        logger.Info($"Update FS connection last sync time: connection id {fsConnection.Id}, connection name {fsConnection.Name}, job status {(JobStatus)subJob.Status} ,last sync time {subJob.LastUpdateTime}.");
                        if (await _multiGeoSettingService.IsEnableMultiGeoFeature())
                        {
                            logger.Info($"Starting update last sync time for mainDC and otherDC.");
                            await RAMultiGeoClient.PostCommonDataToMainDcAsync<UpdateLastSyncTimeRequest, bool>(new UpdateLastSyncTimeRequest { ConnectionId = fsConnection.Id, LastSyncTime = subJob.LastUpdateTime },
                            MultiGeoOperationType.UpdateLastSyncTimeFSConnection,
                            MultiGeoOperationType.UpdateLastSyncTimeFSConnection,
                            request => FSConnectionDao.UpdateLastSyncTimeAsync(request.ConnectionId, request.LastSyncTime));
                        }
                        else
                        {
                            await FSConnectionDao.UpdateLastSyncTimeAsync(fsConnection.Id, subJob.LastUpdateTime);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while updating FS connection related job info for job {subJob.ParentId}, error: {e}");
            }
        }

        private async Task UpdateFSRelatedFailureJobAsync(FSConnectionRelatedJobInfo relatedInfo)
        {
            if (relatedInfo == null )
            {
                logger.Warn($"Related job info is null, no need to update.");
                return;
            }
            await FSConnectionRelatedJobInfoDao.AddOrUpdateRelatedJobAsync(relatedInfo);
            logger.Info($"Updated FS connection related job info for job {relatedInfo.JobId}, job status {relatedInfo.Status}, connection id {relatedInfo.ConnectionId}.");
        }

        private async Task<(bool isJobRunAtConnectionLevel, FSConnection connection)> ResolveConnection(RMSubJob subJob)
        {
            if (subJob == null) return (false, null);

            if (string.IsNullOrEmpty(subJob.String1)) return (false, null); ;

            var selectedNodePath = subJob.String1;

            var connection = FSConnectionDao.GetConnectionByUNCPath(selectedNodePath);
            if (connection != null) return (true, connection);

            var jobContext = SubJobDao.GetJobContextSettingByJobId(subJob.Id);
            if (string.IsNullOrEmpty(jobContext)) return (false, null);

            var fsTreeNodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(jobContext);
            var selectedNode = fsTreeNodes?.FirstOrDefault();
            if (selectedNode == null) return (false, null);

            var connectionLevelNode = RMFileSystemSettingsService.FindConnectionLevelNode(selectedNode);
            if (connectionLevelNode == null) return (false, null);
            connection = FSConnectionDao.GetConnectionByUNCPath(connectionLevelNode.FullPath);

            if (connection == null) return (false, null);

            return (false, connection);
        }
        private async Task HandleSiteMetricsExportAsync(string parentId)
        {
            try
            {
                var customId = TenantLocalValue.LogonGroupId;
                var blobPrefixName = SecurityUtils.SafeCombinePath(customId, "MergeExcel", parentId);
                var tempMergeFolder = SecurityUtils.SafeCombinePath(WebUtil.GetInstallPath(), "Temp", "MergeExcel", parentId);
                var tempMergeTargetFolder = SecurityUtils.SafeCombinePath(WebUtil.GetInstallPath(), "Temp", "MergeExcelTarget", parentId);

                if (!Directory.Exists(tempMergeFolder)) { Directory.CreateDirectory(tempMergeFolder); }
                if (!Directory.Exists(tempMergeTargetFolder)) { Directory.CreateDirectory(tempMergeTargetFolder); }

                var mergeTargetFile = SecurityUtils.SafeCombinePath(tempMergeTargetFolder, $"{parentId}.xlsx");
                blobPrefixName = blobPrefixName.Replace("\\", "/").TrimEnd('/') + "/";
                logger.Info($"blobPrefixName:{blobPrefixName}");
                RAStorageUtil.DownloadAllArchivedContentFiles(blobPrefixName, tempMergeFolder);
                //ExcelUtil.MergeExcelFiles(Directory.GetFiles(tempMergeFolder).ToList(), mergeTargetFile);
                Dictionary<string, List<string[]>> mergedResult = new Dictionary<string, List<string[]>>();
                var firstSourcFile = true;
                foreach (var filePath in Directory.GetFiles(tempMergeFolder))
                {
                    using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    Dictionary<string, List<string[]>> dic;
                    if (firstSourcFile)
                    {
                        dic = ExcelUtil.ReadExcelWithHeader(fs);
                        firstSourcFile = false;

                    }
                    else
                    {
                        dic = ExcelUtil.ReadExcel(fs);
                    }
                    foreach (var d in dic)
                    {
                        if (!mergedResult.ContainsKey(d.Key))
                        {
                            mergedResult.Add(d.Key, new List<string[]>());
                        }
                        mergedResult[d.Key].AddRange(d.Value);
                    }
                }

                var isFirstSheet = true;
                foreach (var sheet in mergedResult)
                {
                    if (isFirstSheet)
                    {
                        ReportUtil.CreateExcel(mergeTargetFile, sheet.Key, [.. sheet.Value]);
                        isFirstSheet = false;
                    }
                    else
                    {
                        ReportUtil.InsertWorksheet(mergeTargetFile, sheet.Key, [.. sheet.Value]);
                    }
                }

                ZipUtil.ZipFolder(tempMergeTargetFolder, tempMergeTargetFolder + ".zip", Encoding.UTF8);
                var blobName = SecurityUtils.SafeCombinePath(customId, parentId + ".zip");
                blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, tempMergeTargetFolder + ".zip");
                var downCenterInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus([(int)DownloadContentJobStatus.Wait, (int)DownloadContentJobStatus.InProgress]).FirstOrDefault(item => item.JobId == parentId);

                var fileInfo = new FileInfo(tempMergeTargetFolder + ".zip");
                downCenterInfo.FileSize = fileInfo.Length;
                downCenterInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                downCenterInfo.JobStatus = (int)DownloadContentJobStatus.Finished;
                if (!SPOExportUtility.IsExportToSPODocumentLibrary)
                {
                    logger.Info("No need to export report to SPO Document Library");
                }
                else
                {
                    var isUploadSuccess = SPOExportUtility.UploadToSPODocumentLibrary(fileInfo.FullName);
                    if (!isUploadSuccess)
                    {
                        throw new Exception($"Upload to SPO lib failed");
                    }
                    logger.Info($"Upload to SPO lib succeeded");
                }

                DownloadDataInfoDao.UpdateDownloadInfo(downCenterInfo);
            }
            catch (Exception e)
            {
                logger.Error($"Merge excel error: {e}");
            }
        }

        private async Task HandleFinalStateAsync(int mainjobstate,int jobType,string parentId,bool cascatMainJoStatus)
        {
            try
            {
                if (cascatMainJoStatus && JobServiceUtility.IsFinalState(mainjobstate))
                {
                    logger.Info($"the main job is final state,state:{mainjobstate}");
                    IJobStateHandler handler = GetStateHandler(jobType);
                    await handler.HanldeFinalStateAsync(parentId, mainjobstate);
                }
                else
                {
                    logger.Info($"the main job is not final state,state:{mainjobstate}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"merge report failed,error:{e}");
            }
        }

        private void RemoveStubRecords()
        {
            try
            {
                StubFileRecordDao.FlushDeleteCache(TenantLocalValue.LogonGroupId);
                logger.Info("Finished flushing delete cache of stub file record for tenant {0}.", TenantLocalValue.LogonGroupId);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while flushing delete cache of stub file record. Error: {e}");
            }
        }

        private IJobStateHandler GetStateHandler(int jobType)
        {
            if (HandlerDic.ContainsKey(jobType))
            {
                return HandlerDic[jobType];
            }
            else
            {
                return HandlerDic[-1];
            }
        }

        private int CalcMainJobState(List<int> allStatus)
        {
            int result = allStatus[0];
            IStatesObject state = JobServiceUtility.GetStateObject(allStatus[0]);
            bool isFirst = true;
            foreach(int status in allStatus)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }
                result = state.coalesceState(status);
                state = JobServiceUtility.GetStateObject(result);
            }
            return result;
        }

        public System.Threading.Tasks.Task MonitorExeptionAsync(string jobId, MonitorExceptionType exceptionType)
        {
            return JobDao.UpdateJobWithMonitorExceptionAsync(jobId, exceptionType);
        }

        private async System.Threading.Tasks.Task SendMLManualEmailAsync(string mainJobId)
        {
            var job = JobDao.GetJob(mainJobId);
            var jobTypes = GetSendEmailJobTypes();
            if (jobTypes.Contains(job.JobType))
            {
                await RMMLManualApprovalEmailSender.SendAsync(mainJobId);
            }
        }
        private static List<int> GetSendEmailJobTypes()
        {
            return new List<int> {
                (int)JobType.ApplySharePointSettings,
                (int)JobType.SharePointScheduleSetting,
                (int)JobType.OneDriveDataSynchronisation,
                (int)JobType.OneDriveDataSynchronisationSchedule,
                (int)JobType.ApplyTeamsSettings,
                (int)JobType.TeamsScheduleSetting,
                (int)JobType.GoogleApplySettings
            };
        }
    }
}
