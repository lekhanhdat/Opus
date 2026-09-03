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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.MachineLearningManualApproval.Actions;
using AvePoint.RA.Service.Services.MachineLearningManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.MachineLearningManualApproval.Queriers;
//using AvePoint.RA.Contract.MachineLearning;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Contract.CloudService;
using Microsoft.Extensions.Azure;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.MachineLearningManualApproval
{
    [AsyncAudit]
    public class RMMLManualApprovalService : RMServiceBase, IRMMLManualApprovalService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static MLManualApprovalRecordRepository Repository => new MLManualApprovalRecordRepository();

        private static readonly IRMManualApprovalDao ManualApprovalDao = new RMManualApprovalDao();

        private ITermDao TermDao = PlatformWindsorManager.GetService<ITermDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IJobMonitorService jobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        public static IRMSubJobDao subJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        public async Task<ManualApprovalPaginateResult> UnderReviewQueryAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            try
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.MLApprovalStatus,
                    Value = JsonConvert.SerializeObject(new List<RMMLApprovalStatus> { RMMLApprovalStatus.WaitingApprove })
                });
                return await MLManualApprovalQuerier.CosmosDBQueryAsync(queryDefinition);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute ml under review panel query. Error: {e}");
                return new ManualApprovalPaginateResult();
            }
        }

        public async Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptionsAsync()
        {
            try
            {
                return await MLManualApprovalQuerier.GetFilterDefaultOptionsAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute get ml filter default value. Error: {e}");
                return new List<ManualApprovalDefaultOptionDefinition>();
            }
        }

        public async Task<ManualApprovalWorkspacePaginateResult> QueryWorkspacesAsync(ManualApprovalWorkspaceQueryDefinition queryDefinition)
        {
            try
            {
                var (Items, Count, SearchCount) = queryDefinition.ContentSource switch
                {
                    SourceFlag.SharePoint => await ManualApprovalDao.GetWorkspacesForSharePointOnline(queryDefinition.PageIndex, queryDefinition.PageSize, queryDefinition.SearchValue),
                    SourceFlag.OneDrive => await ManualApprovalDao.GetWorkspacesForOneDrive(queryDefinition.PageIndex, queryDefinition.PageSize, queryDefinition.SearchValue),
                    SourceFlag.Teams => await ManualApprovalDao.GetWorkspacesForTeams(queryDefinition.PageIndex, queryDefinition.PageSize, queryDefinition.SearchValue),
                    var contentSource when (int)contentSource >= 1000 => (new(), 0, 0),
                    _ => (new(), 0, 0),
                };
                return new ManualApprovalWorkspacePaginateResult
                {
                    WorkspaceItems = Items,
                    WorkspaceCount = Count,
                    SearchResultCount = SearchCount
                };
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while query ml [{queryDefinition.ContentSource}] workspaces. Error: {e}");
                return new ManualApprovalWorkspacePaginateResult();
            }
        }

        [AsyncAudit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.MLReassign, IAsyncAfterHandler = typeof(MLManualApprovalAfterAuditHandler))]
        public Task<ManualApprovalActionResult> ReassignAsync(ManualAprovalEscalateDefinition definition)
        {
            var action = new ReassignAction(Repository);
            return action.Reassign(definition);
        }

        [AsyncAudit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.MLChangeTermJob, IAsyncAfterHandler = typeof(MLManualApprovalAfterAuditHandler))]
        public Task<string> RealRunChangeTermJobAsync(string param, JobType jobType)
        {
            return RealRunActionJobAsync(param, jobType);
        }

        [AsyncAudit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.MLApproveJob, IAsyncAfterHandler = typeof(MLManualApprovalAfterAuditHandler))]
        public Task<string> RealRunApproveJobAsync(string param, JobType jobType)
        {
            return RealRunActionJobAsync(param, jobType);
        }

        private async Task<string> RealRunActionJobAsync(string param, JobType jobType)
        {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            string logonUserId = SerializerHelper.DeserializeByJsonSerializer<ChangeTermDto>(param).UserId;
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = jobMonitorService.CreateJob(jobType, jobRunByUser, account.UserId);
                subJobDao.UpdateSubJobCount(jobId, 1);
                string subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);
                List<string> runningJobs = jobMonitorService.GetRunningJobs(JobType.MachineLearningReviewApprove);
                runningJobs.AddRange(jobMonitorService.GetRunningJobs(JobType.MachineLearningReviewReclassify));
                bool isSkip = runningJobs.Any(j => j != jobId);
                if (!isSkip)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1} {2}", jobType, subJobId, logonUserId),
                    });
                }
                else
                {
                    Logger.Info(I18NEntity.GetString("RM_SYNC_JobSkip"));
                    jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in RealRunApprovedOrRejectedJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        public RAReturnMessage ChangeTerm(RealTimeAction action, ChangeTermDto changeTermInfo)
        {
            RAReturnMessage msg = new RAReturnMessage();
            //If changeTermInfo.TermInfo == null, approve action
            Logger.Info($"Change term action: {action}, records: [{(changeTermInfo.RecordIds == null ? "null" : string.Join(", ", changeTermInfo.RecordIds))}]");
            if (changeTermInfo.TermInfo != null && changeTermInfo.TermInfo.Id != -1)//No Term
            {
                RMTerm selectedTerm = TermDao.GetRMTermByUniqueId(changeTermInfo.TermInfo.UniqueId, false);
                if (selectedTerm.IsDeprecated || selectedTerm.IsExpired)
                {
                    string message = I18N.Core.I18NEntity.GetString("RM_JS_JMD_Comment_Auto_TermNotAvailable");
                    msg.ErrorMessage = message;
                    msg.MessageType = RAMessageType.Failed;
                    return msg;
                }
                //var activeStatus = new MLTermStatus[] { MLTermStatus.NotTrain, MLTermStatus.Training, MLTermStatus.Trained };
                //if (!TrainingTermDao.GetAllMLTerm().Any(t => activeStatus.Contains(t.Status) && t.Id == changeTermInfo.TermInfo.UniqueId))
                //{
                //    string message = I18N.Core.I18NEntity.GetString("RM_MachineLearning_NotAiTerm");
                //    msg.ErrorMessage = message;
                //    msg.MessageType = RAMessageType.Failed;
                //    return msg;
                //}
            }
            string jobId = string.Empty;
            int updateResult;
            IExplorerDao explorerDao = new ExplorerDao();

            if (action == RealTimeAction.MLReviewApprove && changeTermInfo.QueryDefintion == null && changeTermInfo.RecordIds != null)
            {
                var allRecords = explorerDao.QueryAll(r => changeTermInfo.RecordIds.Contains(r.Id)).ToList();
                Logger.Info($"Get records from db, records: [{string.Join(",", allRecords.Select(r => r.Id))}]");

                changeTermInfo.RecordIds = allRecords.Where(r => r.SourceFlag == (int)SourceFlag.SharePoint).Select(r => r.Id).ToList();
                Logger.Info($"Get SPO records from db, records: [{string.Join(",", changeTermInfo.RecordIds)}]");

                changeTermInfo.OneDriveRecordIds = allRecords.Where(r => r.SourceFlag == (int)SourceFlag.OneDrive).Select(r => r.Id).ToList();
                Logger.Info($"Get SPO records from db, records: [{string.Join(",", changeTermInfo.RecordIds)}]");

                changeTermInfo.TeamsRecordIds = allRecords.Where(r => r.SourceFlag == (int)SourceFlag.Teams).Select(r => r.Id).ToList();
                Logger.Info($"Get Teams records from db, records: [{string.Join(",", changeTermInfo.TeamsRecordIds)}]");

                changeTermInfo.GoogleDriveRecordIds = allRecords.Where(r => r.SourceFlag == (int)SourceFlag.Google).Select(r => r.Id).ToList();
                Logger.Info($"Get Google records from db, records: [{string.Join(",", changeTermInfo.GoogleDriveRecordIds)}]");
            }

            #region Start Job
            //if (changeTermInfo.QueryDefinition != null && changeTermInfo.QueryDefinition.Filters != null && changeTermInfo.QueryDefinition.Filters.Count > 0)
            if (changeTermInfo.QueryDefintion != null)
            {
                RAReturnMessage returnMessage = new();
                string id = string.Empty;
                try
                {
                    var groupId = TenantLocalValue.LogonGroupId;
                    var loginName = TenantLocalValue.LogonUserEmail;
                    changeTermInfo.UserId = TenantLocalValue.LogonUserId;
                    changeTermInfo.RequesterType = TenantLocalValue.RequesterType;
                    JobQueueDto jqDto = new()
                    {
                        JobType = changeTermInfo.TermInfo != null ? JobType.MachineLearningReviewReclassify : JobType.MachineLearningReviewApprove,
                        Parameters = SerializerHelper.SerializeByDataContractSerializer(changeTermInfo),
                        JobRunType = JobRunBy.Control,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName
                    };
                    returnMessage.MessageType = RAMessageType.Successful;
                    returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
                }
                catch (Exception ex)
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = ex.Message;
                }
                return returnMessage;

            }

            #endregion

            updateResult = UpdateTerms(action, changeTermInfo, ref jobId);
            msg.Extension = jobId;
            try
            {
                List<Guid> allGuids = new List<Guid>();
                allGuids.AddRange(changeTermInfo.RecordIds?.ToList() ?? []);
                allGuids.AddRange(changeTermInfo.OneDriveRecordIds?.ToList() ?? []);
                msg.Extsion1 = JsonConvert.SerializeObject(explorerDao.GetRecordByIds(allGuids).Select(r => r.LeafName).ToList());
            }
            catch (Exception e)
            {
                Logger.Warn($"get records name error: {e}");
            }
            return msg;
        }
        private int UpdateTerms(RealTimeAction action, ChangeTermDto changeTermInfo, ref string updateTermTempJobId)
        {
            updateTermTempJobId = "UT" + Guid.NewGuid().ToString();
            RecordsRealTimeMessage jobMessage = new RecordsRealTimeMessage();
            jobMessage.JobId = updateTermTempJobId;
            jobMessage.Action = action;//RealTimeAction.ChangeTerm;
            jobMessage.ChangeTermOption = new ChangeTermOption()
            {
                SourceRecordIds = changeTermInfo.RecordIds ?? [],
                SourceOneDriveRecordIds = changeTermInfo.OneDriveRecordIds ?? [],
                SourceTeamsRecordIds = changeTermInfo.TeamsRecordIds ?? [],
                GoogleDriveRecordIds = changeTermInfo.GoogleDriveRecordIds ?? [],
                Comment = changeTermInfo.Comment,

                SourceFSRecordIds = new(),
                SourceEXORecordIds = new(),
                SourcePhyRecordIds = new(),
                SourceAzureFileShareRecordIds = new(),
                SourceCustomizeConnectorRecordIds = new(),
            };
            if (changeTermInfo.TermInfo != null)
            {
                jobMessage.ChangeTermOption.TargetTermId = changeTermInfo.TermInfo.Id;
                jobMessage.ChangeTermOption.TargetTermName = changeTermInfo.TermInfo.Name;
                jobMessage.ChangeTermOption.TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId;
            }
            else
            {
                //Approval
                jobMessage.ChangeTermOption.TargetTermId = -1;
            }
            jobMessage.LogonGroupId = TenantLocalValue.LogonGroupId;
            jobMessage.CurrentUserName = TenantLocalValue.LogonUserEmail;

            try
            {
                SendMessageAsync(jobMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                return RecordsConstants.Explorer_RealTime_Failed_All;
            }
            return RecordsConstants.Explorer_RealTime_Success;
        }
        private void SendMessageAsync(RecordsRealTimeMessage jobMessage)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment && string.IsNullOrEmpty(RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SERVICE_BUS_CONNECTION_STRING]))
                {
                    Logger.Info($"Send  message to service bus failed. Make sure config SERVICE_BUS_CONNECTION_STRING");
                }
                else
                {
                    Logger.Info($"Send  message to service bus. LogonGroupId : {jobMessage.LogonGroupId}, Action: {jobMessage.Action.ToString()}, JobId:  {jobMessage.JobId}");
                    SendMessageToCloud(jobMessage);
                }
            });

        }

        private void SendMessageToCloud(RecordsRealTimeMessage jobMessage)
        {
            var maxRetryTimes = 3;
            var retryTimes = 0;
            while (retryTimes < maxRetryTimes)
            {
                try
                {
                    QueueMessageUtilFactory.GetUtil(QueueMessageType.RealTime).SendMessage(jobMessage);
                    break;
                }
                catch (Exception e)
                {
                    Logger.Info($"Will retry to send real time action message to cloud, max retry times : {maxRetryTimes}, current retry times : {++retryTimes}");
                    System.Threading.Thread.Sleep(1000);
                }
            }


        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, JobStatus jobState, int subJobCount, string jobMessage, string string1 = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)jobState,
                Weight = 100d / subJobCount,
                String1 = string1,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                Runable = jobState == JobStatus.InProgress ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
            };
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            subJobDao.CreateJob(subJob);
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }
    }
}
