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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Enums;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Collections;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.Dashboard;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using AvePoint.RA.Service.Services.ManualApproval.ApproveJob;
using AvePoint.RA.Service.Services.ManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using AvePoint.RA.Service.Services.ManualApproval.Setting;
using AvePoint.RA.Service.Services.Myhub.Actions;
using AvePoint.RA.Service.Services.PermissionManagement;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.SharePointSetting;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Cloud.Sdk.Data.Aos.CloudInsights;
using CommonModel.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using Polly;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimeZoneConverter;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.Service.Services.ManualApproval
{
    [AsyncAudit]
    public class RMManualApprovalService : RMServiceBase, IRMManualApprovalService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMManualApprovalService));
        private static readonly IUserService UserService = PlatformWindsorManager.GetService<IUserService>();
        private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private static ManualApprovalRecordRepository Repository => new ManualApprovalRecordRepository();

        private static IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private static IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private static IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        private static readonly IRMManualApprovalDao ManualApprovalDao = new RMManualApprovalDao();

        private static IRMTenantUpgradeInfoDao TenantUpgradeInfoDao => PlatformWindsorManager.GetService<IRMTenantUpgradeInfoDao>();
        public ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static IRMCacheManager CacheManager => PlatformWindsorManager.GetService<IRMCacheManager>();
        private static IMyhubReportJobDao MyhubReportJobDao => PlatformWindsorManager.GetService<IMyhubReportJobDao>();
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IFSConnectionDao FSConnectionDao = PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private const string DISABLE_ESCALATE_KEY = "MANUAL_DISABLE_ESCALATE";

        private const string MANUAL_TASK_ID = "7e7f2f3d-4f4b-438e-8d5c-5395e6e2b3f8";
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMKeyValueDao _RMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        public async Task<ManualApprovalPaginateResult> UnderReviewQueryAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            try
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = queryDefinition.FromGControl ? ManualApprovalFilterOptions.GControlApprovalStatus : ManualApprovalFilterOptions.ApprovalStatus,
                    Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
                });
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
                FilterPermission(queryDefinition);
                return await ManualApprovalQuerier.CosmosDBQueryAsync(queryDefinition);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute under review panel query. Error: {e}");
                return new ManualApprovalPaginateResult();
            }
        }

        public async Task<ManualApprovalPaginateResult> UnderReviewFolderViewQueryAsync(ManualApprovalQueryDefinition queryDefinition, string timeZoneId, bool isDaylight)
        {
            try
            {
                return await ManualApprovalQuerier.CosmosDBFolderViewQueryAsync(queryDefinition, timeZoneId, isDaylight);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute under review panel query. Error: {e}");
                return new ManualApprovalPaginateResult();
            }
        }

        public async Task<ManualApprovalPaginateResult> RelatedRecordsQueryAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            try
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                    Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
                });
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.IsRelatedRecords,
                    Value = "true"
                });
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
                FilterPermission(queryDefinition);
                return await ManualApprovalQuerier.CosmosDBQueryAsync(queryDefinition);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute related records panel query. Error: {e}");
                return new ManualApprovalPaginateResult();
            }
        }

        public async Task<ManualApprovalPaginateResult> ExtendQueryAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            try
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = queryDefinition.FromGControl ? ManualApprovalFilterOptions.GControlApprovalStatus : ManualApprovalFilterOptions.ApprovalStatus,
                    Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Approved, SOApproveDBStatus.Rejected, SOApproveDBStatus.WaitingApprove })
                });
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "true"
                });
                FilterPermission(queryDefinition);
                return await ManualApprovalQuerier.CosmosDBQueryAsync(queryDefinition);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute extend panel query. Error: {e}");
                return new ManualApprovalPaginateResult();
            }
        }

        public async Task<ManualApprovalPaginateResult> WaitDiposalQueryAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            try
            {
                if (!queryDefinition.Filters.Any(item => item.FilterOption == ManualApprovalFilterOptions.ApprovalStatus))
                {
                    queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                    {
                        FilterOption = queryDefinition.FromGControl ? ManualApprovalFilterOptions.GControlApprovalStatus : ManualApprovalFilterOptions.ApprovalStatus,
                        Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Approved, SOApproveDBStatus.Rejected })
                    });
                }
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
                FilterPermission(queryDefinition);
                return await ManualApprovalQuerier.CosmosDBQueryAsync(queryDefinition);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute wait disposal panel query. Error: {e}");
                return new ManualApprovalPaginateResult();
            }
        }

        public async Task<List<ManualApprovalItem>> HistoryAzureTableQueryAsync()
        {
            try
            {
                return await ManualApprovalAzureTableHistoryQuerier.HistoryQueryAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute history panel from azure table query. Error: {e}");
                return new();
            }
        }

        public async Task<List<ManualApprovalItem>> HistoryAzureTableQueryForGControlAsync()
        {
            try
            {
                return await ManualApprovalAzureTableHistoryQuerier.HistoryQueryForGControlAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute history panel from azure table query. Error: {e}");
                return new();
            }
        }

        public async Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptionsAsync()
        {
            try
            {
                return await ManualApprovalQuerier.GetFilterDefaultOptionsAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute get filter default value. Error: {e}");
                return new List<ManualApprovalDefaultOptionDefinition>();
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToApproved, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<ManualApprovalActionResult> ApproveAsync(ManualApprovalActionParams approveParameters, bool isFromMyhub = false)
        {
            try
            {
                var approvalAction = new ApprovalAction(Repository, SOApproveDBStatus.Approved);
                await approvalAction.InitAsync();
                return await approvalAction.ApproveOrReject(approveParameters);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while approve data. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = ""
                };
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToRejected, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<ManualApprovalActionResult> RejectAsync(ManualApprovalActionParams approveParameters, bool isFromMyhub = false)
        {
            var needRejectIds = approveParameters.NeedActionIds;
            try
            {
                Logger.Info("reject id count {0}", needRejectIds?.Count);
                var approvalAction = new ApprovalAction(Repository, SOApproveDBStatus.Rejected);
                await approvalAction.InitAsync();
                return await approvalAction.ApproveOrReject(approveParameters);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while reject datas: [{string.Join(",", needRejectIds)}]. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = ""
                };
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.EscalateTo, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public Task<ManualApprovalActionResult> EscalateAsync(ManualAprovalEscalateDefinition definition)
        {
            var action = new EscalateAction(Repository);
            return action.Escalate(definition);
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.ReassignTo, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public Task<ManualApprovalActionResult> ReassignAsync(ManualAprovalEscalateDefinition definition)
        {
            var action = new EscalateAction(Repository);
            return action.Reassign(definition);
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToExtend, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public Task<ManualApprovalActionResult> Extend(ManualApprovalExtendDefinition definition)
        {
            var action = new ExtendAction(Repository);
            return action.Extend(definition);
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RestoreExtend, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public Task<ManualApprovalActionResult> RestoreExtended(List<Guid> itemIds)
        {
            var action = new ExtendAction(Repository);
            return action.Restore(itemIds);
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.ChangeAction, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public Task<ManualApprovalActionResult> ChangeDiposalAction(ManualApprovalRelatedRecordsDisposalDefinition definition)
        {
            var action = new RelatedRecordsAction(Repository);
            return action.ChangeDisposalAction(definition);
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.ResetManualWorkflow, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<ManualApprovalActionResult> ResetManualReviewForWorkflow(List<Guid> itemIds, bool isFromGControl = false)
        {
            try
            {
                var action = new ReManualReviewAction(Repository, isFromGControl);
                return await action.ResetWorkflow(itemIds);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while reset manual status. Items: [{string.Join(",", itemIds)}] Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = ""
                };
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.WorkflowManagement, Action = AuditAction.ManualApprovalConfigSetting, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public Task<bool> UpdateManualApprovalSetting(ManualApprovalSettings setting)
        {
            var manager = new ManualApprovalSettingManager();
            return manager.Update(setting);
        }

        public Task<ManualApprovalSettings> GetManualApprovalSettingsAsync()
        {
            var manager = new ManualApprovalSettingManager();
            return manager.Get();
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.ManualApprovalSettingTimer, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public Task<string> RealRunEmailScheduleJobAsync(JobRunBy runBy)
        {
            Logger.Info("Start run email schedule job.");
            var jobId = string.Empty;

            try
            {
                var username = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.ManualApprovalEmailSchedule) > 0;
                jobId = JobMonitorService.CreateJob(JobType.ManualApprovalEmailSchedule, username);
                if (hasRunningJob)
                {
                    Logger.Warn("A running email schedule job already exists.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return System.Threading.Tasks.Task.FromResult(jobId);
                }

                Logger.Info($"Real run dashboard job: [{jobId}]");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.ManualApprovalEmailSchedule,
                    CommandLine = $"{JobType.ManualApprovalEmailSchedule} {jobId}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run email schedule job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return System.Threading.Tasks.Task.FromResult(jobId);
        }

        public bool SchduleRunEmailScheduleJob(JobRunBy runBy)
        {
            var id = string.Empty;
            var runJobUserName = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";

            try
            {
                var queue = new JobQueueDto
                {
                    JobType = JobType.ManualApprovalEmailSchedule,
                    JobRunType = runBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = runJobUserName,
                    Parameters = null
                };

                id = JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run email schedule job. Error: {e}");
            }
            return !string.IsNullOrEmpty(id);
        }

        public MAReturnMessage RunBulkActionJob(ManualApprovalJobParam param)
        {
            MAReturnMessage returnMessage = new MAReturnMessage();
            try
            {
                if (param.ApprovalAction == (int)SOApproveDBStatus.Approved)
                {
                    param.QuickReason = string.Empty;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                param.UserId = TenantLocalValue.LogonUserId;
                param.RequesterType = TenantLocalValue.RequesterType;

                if (param.IsFromMyhub == true && param.IsJpmc == false)
                {
                    ManualApprovalFilterDefinition filter = new ManualApprovalFilterDefinition();
                    filter.FilterOption = ManualApprovalFilterOptions.SourceNoFs;
                    filter.Value = "";
                    if (param.QueryDefintion == null)
                    {
                        ManualApprovalQueryDefinition queryDefintion = new ManualApprovalQueryDefinition();
                        param.QueryDefintion = queryDefintion;
                    }
                    if (param.QueryDefintion.Filters == null || param.QueryDefintion.Filters.Count < 1)
                    {
                        List<ManualApprovalFilterDefinition> filters = new List<ManualApprovalFilterDefinition>();
                        param.QueryDefintion.Filters = filters;
                    }
                    param.QueryDefintion.Filters.Add(filter);
                }

                var parameter = SerializerHelper.SerializeByJsonSerializer(param);
                JobQueueDto jqDto = new()
                {
                    JobType = JobType.ManualApprovalOrRejectJob,
                    Parameters = parameter,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                Logger.Error($"error occurred while run bulk action Job, action [{param.ApprovalAction}] ,ERROR:{ex}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RunManualApproveOrReject, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler))]
        public async Task<string> RealRunBulkActionJobAsync(string param)
        {
            var jobId = string.Empty;
            var jobRunByUser = TenantLocalValue.LogonUserEmail;
            var manualActionInfos = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalJobParam>(param);
            try
            {
                var jobType = JobType.ManualApprovalOrRejectJob;
                var account = AccountDao.Find(item => item.UserId == manualActionInfos.UserId && item.IsRemoved == 0);
                jobId = JobMonitorService.CreateJob(jobType, account.UserPrincipalName, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);
                var runningJobs = JobMonitorService.GetRunningJobs(JobType.ManualApprovalOrRejectJob);
                var isSkip = runningJobs.Any(j => j != jobId);
                if (!isSkip)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1} {2} {3} {4}", jobType, subJobId, account.UserId, manualActionInfos.ApprovalAction.ToString(), manualActionInfos.QueryDefintion.FromGControl),
                    });
                }
                else
                {
                    Logger.Info(I18NEntity.GetString("RM_SYNC_JobSkip"));
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in real run bulk action job, action [{manualActionInfos.ApprovalAction}], error : {ex}.");
            }
            return jobId;
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
                    SourceFlag.Google => await ManualApprovalDao.GetWorkspacesForGoogle(queryDefinition.PageIndex, queryDefinition.PageSize, queryDefinition.SearchValue),
                    var contentSource when (int)contentSource >= 1000 => (new(), 0, 0),
                    _ => (new(), 0, 0),
                };
                return new ManualApprovalWorkspacePaginateResult
                {
                    WorkspaceItems = Items,
                    WorkspaceCount = Count,
                    SearchResultCount = SearchCount,
                };
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while query [{queryDefinition.ContentSource}] workspaces. Error: {e}");
                return new ManualApprovalWorkspacePaginateResult();
            }
        }

        public async Task<ManualApprovalFilterFolderPathResult> QueryFolderPathAsync(ManualApprovalFolderPathQueryDefinition queryDefinition)
        {
            var result = new ManualApprovalFilterFolderPathResult()
            {
                FolderPathResults = new HashSet<string>(),
                Continuation = queryDefinition.Continuation
            };
            try
            {

                var repository = Repository;
                var pageIndex = queryDefinition.PageIndex;
                var workSpaceSource = queryDefinition.WorkSpaceSource;
                var searchValue = queryDefinition.SearchValue;
                var pageSize = queryDefinition.PageSize;
                var (isAdmin, userPermissionIds) = await CheckUserAdminPermission();
                var (predicate, notAdminpredicate) = await BuildFilterAsync(queryDefinition.ManualApprovalTab, result, repository, pageIndex, workSpaceSource, searchValue, isAdmin, userPermissionIds);
                return await ManualApprovalDao.GetFolderPathResults(result, repository, predicate, notAdminpredicate, pageSize);

            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while query [{queryDefinition.WorkSpaceSource}]-Folder Path. Error: {e}");
                return result;
            }
        }

        public static async Task<(bool isAdmin, List<int> userPermissionIds)> CheckUserAdminPermission()
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);

            if (isAdmin)
            {
                return (true, new List<int>());
            }

            var userPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            return (false, userPermissionIntIds);
        }

        public static async Task<(Expression<Func<ManualApprovalRecord, bool>> predicate, Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate)> BuildFilterAsync(
            ManualApprovalTab manualApprovalTab, ManualApprovalFilterFolderPathResult result, ManualApprovalRecordRepository repository,
            int pageIndex, List<string> workSpaceSource, string searchValue, bool isAdmin, List<int> userPermissionIds)
        {

            Expression<Func<ManualApprovalRecord, bool>> predicate = null;
            Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate = item => true;
            Expression<Func<ManualApprovalRecord, string>> selector = item => item.ManualFolderPath;
            if (manualApprovalTab == ManualApprovalTab.UnderReview)   //UnderReview
            {
                predicate = item =>
                    item.SourceFlag == (int)SourceFlag.OneDrive &&
                    item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove &&
                    item.ManualExtendTime < DateTime.UtcNow.Ticks &&
                    (workSpaceSource.Count == 0 || workSpaceSource.Contains(item.ManualSiteUrl)) &&
                    item.ManualFolderPath.Contains(searchValue.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    item.IsManualSynced && item.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd &&
                    item.RecordStatus != (int)RMRecordStatus.Hidden && item.RecordStatus != (int)RMRecordStatus.RMDeleted;

            }
            else if (manualApprovalTab == ManualApprovalTab.WaitDisposal)  //WaitingforDisposal
            {
                predicate = item =>
                        item.SourceFlag == (int)SourceFlag.OneDrive &&
                        (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved || item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected) &&
                        (workSpaceSource.Count == 0 || workSpaceSource.Contains(item.ManualSiteUrl)) &&
                         item.ManualExtendTime < DateTime.UtcNow.Ticks &&
                        item.IsManualSynced && item.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd &&
                        item.RecordStatus != (int)RMRecordStatus.Hidden && item.RecordStatus != (int)RMRecordStatus.RMDeleted &&
                        item.ManualFolderPath.Contains(searchValue.Trim(), StringComparison.OrdinalIgnoreCase);

            }
            else if (manualApprovalTab == ManualApprovalTab.Extend)
            {
                predicate = item =>
                           item.SourceFlag == (int)SourceFlag.OneDrive &&
                            (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved || item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected) &&
                            (workSpaceSource.Count == 0 || workSpaceSource.Contains(item.ManualSiteUrl)) &&
                            item.ManualFolderPath.Contains(searchValue.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            item.ManualExtendTime >= DateTime.UtcNow.Ticks &&
                            item.IsManualSynced && item.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd &&
                            item.RecordStatus != (int)RMRecordStatus.Hidden && item.RecordStatus != (int)RMRecordStatus.RMDeleted;

            }
            if (!isAdmin)
            {
                notAdminpredicate = await GetCosmosDBFilterExpressionAsync(userPermissionIds);
            }

            if (pageIndex == 0)
            {
                result.FolderPathResultsCount = await repository.CountAsyncForRecordItemDistinct(predicate, notAdminpredicate, selector);
            }

            return (predicate, notAdminpredicate);
        }

        public static async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(List<int> userPermissionIds)
        {

            var expressions = new List<Expression>();
            ParameterExpression parameter = Expression.Parameter(typeof(ManualApprovalRecord), "root");

            foreach (var reviewr in userPermissionIds)
            {
                Expression<Func<ManualApprovalRecord, bool>> expression =
                    (root) => root.ManualReviewer.Contains(reviewr);
                expressions.Add(expression.Body);
            }
            var body = expressions.AsEnumerable().Aggregate(Expression.OrElse);
            return Expression.Lambda<Func<ManualApprovalRecord, bool>>(body, parameter);
        }

        public Task<bool> EnableFolderPathForDeloitte()
        {
            try
            {
                var setting = KeyValueDao.GetValueByKey("EnableFolderPath");
                return System.Threading.Tasks.Task.FromResult(setting != null && Convert.ToBoolean(setting.Value));
            }
            catch
            {
                Logger.Error($"An error occurred while check escalate function is disabled.");
                return System.Threading.Tasks.Task.FromResult(false);
            }
        }

        public async Task<(bool isOnlyOneLocation, string manualSiteUrl)> EnableFolderPathForDeloitteOnlyOneLocation()
        {
            try
            {
                var repository = Repository;
                var (isAdmin, userPermissionIds) = await CheckUserAdminPermission();
                Expression<Func<ManualApprovalRecord, bool>> predicate = item =>
                    item.SourceFlag == (int)SourceFlag.OneDrive &&
                    item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove &&
                    item.ManualExtendTime < DateTime.UtcNow.Ticks &&
                    item.IsManualSynced && item.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd &&
                    item.RecordStatus != (int)RMRecordStatus.Hidden && item.RecordStatus != (int)RMRecordStatus.RMDeleted;

                Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate = item => true;
                Expression<Func<ManualApprovalRecord, string>> selector = item => item.ManualSiteUrl;
                if (!isAdmin)
                {
                    notAdminpredicate = await GetCosmosDBFilterExpressionAsync(userPermissionIds);
                }

                return await repository.FindisOneLocation(predicate, notAdminpredicate, selector);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while check escalate function is disabled.error: {e}");
                return (false, string.Empty);
            }
        }

        public async Task<ManualApprovalSpecialReviewerResult> SpecialReviewerResult()
        {
            try
            {
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ControlPanelAdmin);
                if (isAdmin)
                {
                    return new();
                }

                var isDeloitteReviewer = await EnableFolderPathForDeloitte();
                if (!isDeloitteReviewer)
                {
                    return new();
                }

                var result = await EnableFolderPathForDeloitteOnlyOneLocation();
                var isExistFolderViewItem = await IsExistFolderViewItem();
                return new()
                {
                    IsDeloitteReviewer = isDeloitteReviewer,
                    IsOnlyOneLocation = result.isOnlyOneLocation,
                    ManualSiteUrl = result.manualSiteUrl,
                    IsExsitFolderViewItem = isExistFolderViewItem,
                };
            }
            catch (Exception e)
            {
                Logger.Error($"Get Deloitte reviewer failed, error : {e}");
                return new();
            }

        }

        public async Task<ManualApprovalTaskInfos> GetManualApprovalTaskInfo(string timeZoneId, bool isDaylight)
        {
            try
            {
                var queryDefinition = new ManualApprovalQueryDefinition
                {
                    OrderBy = ManualApprovalOrderOptions.CollectioinTime,
                    PageSize = 1,
                    IsEnableFolderView = false,
                };
                var result = await UnderReviewFolderViewQueryAsync(queryDefinition, timeZoneId, isDaylight);
                var item = result.Items.First();
                if (item != null)
                {
                    var collectionTime = item.CollectionDateTime;
                    var taskDuration = (await GetApprovalCommentOptionAsync()).Duration;
                    var taskDueDate = collectionTime.AddDays(taskDuration);
                    return new()
                    {
                        Id = MANUAL_TASK_ID,
                        Status = 0,
                        DueDate = taskDueDate,
                        CreatedTime = collectionTime,
                        Title = I18NEntity.GetString("RM_MA_Myhub_OpusTask"),
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get manual approval task information , custom id : {TenantLocalValue.LogonGroupId}, user id : {TenantLocalValue.LogonUserId}, error : {e}.");
            }

            return null;
        }

        public Task<bool> DisabledEscalateAsync()
        {
            try
            {
                var setting = KeyValueDao.GetValueByKey(DISABLE_ESCALATE_KEY);
                return System.Threading.Tasks.Task.FromResult(setting != null && Convert.ToBoolean(setting.Value));
            }
            catch
            {
                Logger.Error($"An error occurred while check escalate function is disabled.");
                return System.Threading.Tasks.Task.FromResult(false);
            }
        }

        public void SendUpgradeJobMessage()
        {
            try
            {
                var count = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.ManualHistoriesUpgrade);
                if (count > 0)
                {
                    Logger.Warn($"A upgrade job meessage already exists.");
                }
                var queue = new JobQueueDto
                {
                    JobType = JobType.ManualHistoriesUpgrade,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = null
                };

                JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while current tenant send upgrade job message. Error: {e}");
            }
        }

        public string RealRunUpgradeJob()
        {
            Logger.Info("Start run tenant upgrade job.");
            var jobId = string.Empty;

            try
            {
                var username = "RM_TS_RunSchedule";
                var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.ManualHistoriesUpgrade) > 0;
                jobId = JobMonitorService.CreateJob(JobType.ManualHistoriesUpgrade, username);
                if (hasRunningJob)
                {
                    Logger.Warn("A running upgrade job already exists.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return jobId;
                }

                Logger.Info($"Real run upgrade job: [{jobId}]");
                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.ManualHistoriesUpgrade,
                    CommandLine = $"{JobType.ManualHistoriesUpgrade} {jobId}",
                });


                TenantUpgradeInfoDao.UpdateTenantUpgradeInfoToRunning(TenantLocalValue.LogonGroupId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run upgrade job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        public RAReturnMessage RunExportHistoryDatasJob(string serviceUrl, ManualApprovalHistoryOption historyOption)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var logonUserId = TenantLocalValue.LogonUserId;
                historyOption.LogonUserId = logonUserId;

                //Handle Display name
                if (!string.IsNullOrEmpty(historyOption.FullPath))
                {
                    if (historyOption.LatestExportType == 4 && historyOption.CustomDate != null)
                    {
                        var customDate = historyOption.CustomDate;

                        if (string.IsNullOrEmpty(customDate.TimeZoneId))
                        {
                            var generalSetting = GeneralSettingService.GetGeneralSettingAsync().Result;
                            customDate.TimeZoneId = generalSetting?.TimeZoneId ?? TimeZoneInfo.Local.Id;
                            customDate.IsDaylight = generalSetting?.DayLight ?? false;
                        }
                        else
                        {
                            customDate.TimeZoneId = DateTimeUtil.AllTimeZones[Convert.ToInt32(customDate.TimeZoneId)];
                        }


                        TimeZoneInfo timeZone;
                        try
                        {
                            timeZone = TZConvert.GetTimeZoneInfo(historyOption.CustomDate.TimeZoneId);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn($"Failed to load TimeZone for '{historyOption.CustomDate.TimeZoneId}'. Falling back to UTC. Error: {ex.Message}");
                            timeZone = TimeZoneInfo.Utc;
                            historyOption.CustomDate.TimeZoneId = timeZone.Id;
                        }

                        var localStart = customDate.StartDateTime;
                        var localEnd = customDate.EndDateTime;

                        var localEnd59 = new DateTime(localEnd.Year, localEnd.Month, localEnd.Day, localEnd.Hour, localEnd.Minute, 59, DateTimeKind.Unspecified);
                        var localStartUnspecified = DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified);

                        customDate.StartDateTimeTicks = TimeZoneInfo.ConvertTimeToUtc(localStartUnspecified, timeZone).Ticks;
                        customDate.EndDateTimeTicks = TimeZoneInfo.ConvertTimeToUtc(localEnd59, timeZone).Ticks;

                        if (customDate.StartDateTimeTicks > customDate.EndDateTimeTicks)
                        {
                            Logger.Warn("Invalid time range for RCC report. Start:{0} End:{1}", customDate.StartDateTime, customDate.EndDateTime);
                            returnMessage.MessageType = RAMessageType.Failed;
                            returnMessage.ErrorMessage = "The start date cannot be later than the end date.";
                            return returnMessage;
                        }

                        //DateTime currentUtcDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).Date;
                        //var startUtc = DateTime.SpecifyKind(new DateTime(customDate.StartDateTimeTicks), DateTimeKind.Utc);
                        //var endUtc = DateTime.SpecifyKind(new DateTime(customDate.EndDateTimeTicks), DateTimeKind.Utc);

                        //if (startUtc.Date > currentUtcDate || endUtc.Date > currentUtcDate)
                        //{
                        //    Logger.Warn("Invalid time range for RCC report. Start:{0} End:{1}", customDate.StartDateTime, customDate.EndDateTime);
                        //    returnMessage.MessageType = RAMessageType.Failed;
                        //    returnMessage.ErrorMessage = "The start date and end date cannot be later than the current date.";
                        //    return returnMessage;
                        //}
                    }
                    var validMsg = ValidExportDisposalHistoryParam(historyOption);
                    if (validMsg.MessageType != RAMessageType.Successful)
                    {
                        return validMsg;
                    }
                    var connection = Guid.TryParse(historyOption.PartitionKeyId, out Guid connectionId)
                                ? FSConnectionDao.GetConnectionById(connectionId)
                                : null;
                    if (connection != null)
                    {
                        historyOption.DisplayName = ResolvedHistoryDisplayName(connection);
                    }
                }

                var historyOptionStr = SerializerHelper.SerializeByJsonSerializer(historyOption);
                var jqDto = new JobQueueDto
                {
                    JobType = JobType.ManualExportHistoryDatasJob,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = $"{historyOptionStr}",
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while current tenant send upgrade job message. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RunExportHistoryJob, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        [FSAudit(AuditType = FSAuditType.GenerateDisposalHistory, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunExportHistoryDatasJobAsync(string historyOptionStr)
        {
            Logger.Info("Start run export history data job.");
            var jobId = string.Empty;
            var historyOption = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalHistoryOption>(historyOptionStr);
            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

                var historyOptionObj = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(historyOptionStr);

                jobId = !string.IsNullOrEmpty(historyOptionObj.Id) ? JobMonitorService.CreateJob(JobType.ManualExportHistoryDatasJob, username, account.UserId, historyOptionObj.Id)
                                                : JobMonitorService.CreateJob(JobType.ManualExportHistoryDatasJob, username, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                string subJobId = CreateSubJob(jobId, 0, JobType.ManualExportHistoryDatasJob, JobStatus.InProgress, 1, historyOptionStr);
                string fileName = string.IsNullOrEmpty(historyOptionObj.FullPath) ? (jobId + ".zip") : historyOptionObj.DisplayName;
                var downloadDataInfo = new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = historyOption.LogonUserId,
                    Name = fileName,
                    DownloadType = DownloadContentType.HistoryContent,
                    ExtendString1 = historyOptionStr,
                };

                DownloadDataInfoDao.Create(downloadDataInfo);
                if (!string.IsNullOrEmpty(historyOptionObj.FullPath)) await MyhubReportJobDao.CreateJobReports(downloadDataInfo);

                Logger.Info($"Real run dashboard job: [{jobId}]");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = subJobId,
                    JobType = JobType.ManualExportHistoryDatasJob,
                    CommandLine = string.Format("{0} {1} {2}", JobType.ManualExportHistoryDatasJob, subJobId, account.UserId),
                    Extension = historyOptionStr,
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run email schedule job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        private string ResolvedHistoryDisplayName(FSConnection connection)
        {
            if (connection == null)
            {
                Logger.Warn("Connection not found.");
                return string.Empty;
            }

            string jpmcId = SanitizeFileName(connection.JPMCConnectionId);
            string connName = SanitizeFileName(connection.Name ?? connection.UNCPath);

            if (string.IsNullOrWhiteSpace(jpmcId) && string.IsNullOrWhiteSpace(connName))
            {
                Logger.Warn("Connection produced an empty file name after sanitization.");
                return string.Empty;
            }

            jpmcId = TruncateString(jpmcId, 100);
            connName = TruncateString(connName, 100);
            return $"Disposal_history_report_{jpmcId}_{connName}.zip";
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length > maxLength ? text[..maxLength] : text;
        }

        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                sb.Append(Array.IndexOf(invalidChars, c) >= 0 ? '_' : c);
            }

            string sanitized = sb.ToString();
            sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9\s\.\-]{2,}", "_");
            sanitized = Regex.Replace(sanitized, @"_{2,}", "_");

            return sanitized.Trim();
        }

        private RAReturnMessage ValidExportDisposalHistoryParam(ManualApprovalHistoryOption param)
        {
            var result = new RAReturnMessage();
            if (string.IsNullOrWhiteSpace(param.Id))
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "Id should not be null or empty.";
                return result;
            }
            if (string.IsNullOrWhiteSpace(param.PartitionKeyId))
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "PartitionKeyId should not be null or empty.";
                return result;
            }
            return result;
        }
        public RAReturnMessage RunDeleteInvalidRecordsJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.DeleteInvalidRecords,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred whie DeleteInvalidRecords,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return msg;
        }
        public string RealRunDeleteInvalidRecordsJob()
        {
            Logger.Info("Start delete invalid records job.");
            string jobId = string.Empty;
            try
            {
                var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.DeleteInvalidRecords) > 0;
                jobId = JobMonitorService.CreateJob(JobType.DeleteInvalidRecords, "RM_TS_RunSchedule");
                if (hasRunningJob)
                {
                    Logger.Warn("A Delete Invalid Records Job already exists.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return jobId;
                }
                Logger.Info($"Real run DeleteInvalidRecords job: [{jobId}]");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.DeleteInvalidRecords,
                    CommandLine = $"{JobType.DeleteInvalidRecords} {jobId}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run DeleteInvalidRecords job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }
            return jobId;
        }
        public async Task<RAReturnMessage> RunExportRecordsForReviewDatasJobAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (!await PreCheckHasRecordsAsync(queryDefinition, queryDefinition.ManualApprovalTab))
                {
                    throw new Exception(I18NEntity.GetString("RM_RDM_MA_ExportNoData"));
                }
                var loginName = TenantLocalValue.LogonUserEmail;
                var definitionStr = SerializerHelper.SerializeByJsonConvert(queryDefinition);
                var jqDto = new JobQueueDto
                {
                    JobType = JobType.ManualExportRecordsForReviewDatasJob,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = $"{definitionStr}",
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run export {queryDefinition.ManualApprovalTab} job message. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;
        }
        
        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RunExportRecordsForReviewJob, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<string> RealRunExportRecordsForReviewDatasJobAsync(string queryDefinitionStr)
        {
            Logger.Info("Start run export RecordsForReview data job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = JobMonitorService.CreateJob(JobType.ManualExportRecordsForReviewDatasJob, username, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                string subJobId = CreateSubJob(jobId, 0, JobType.ManualExportRecordsForReviewDatasJob, JobStatus.InProgress, 1, queryDefinitionStr);

                var queryDefinition = JsonConvert.DeserializeObject<ManualApprovalQueryDefinition>(queryDefinitionStr);

                var downloadType = queryDefinition.ManualApprovalTab switch
                {
                    ManualApprovalTab.UnderReview => DownloadContentType.UnderReviewContent,
                    ManualApprovalTab.WaitDisposal => DownloadContentType.WaitingForDisposalContent,
                    ManualApprovalTab.Extend => DownloadContentType.DisposalExtendContent,
                    ManualApprovalTab.RelatedRecords => DownloadContentType.RelatedRecordsContent,
                };

                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = downloadType,
                });

                Logger.Info($"Real run export RecordsForReview job: [{jobId}].");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = subJobId,
                    JobType = JobType.ManualExportRecordsForReviewDatasJob,
                    CommandLine = $"{JobType.ManualExportRecordsForReviewDatasJob} {subJobId} {jobId} {(int)queryDefinition.ManualApprovalTab}",
                });

            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run export RecordsForReview job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }
        private static async Task<bool> PreCheckHasRecordsAsync(ManualApprovalQueryDefinition queryDefinition, ManualApprovalTab manualApprovalTab)
        {
            var statusList = new List<SOApproveDBStatus>() { };

            if (manualApprovalTab == ManualApprovalTab.UnderReview)
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
                statusList.Add(SOApproveDBStatus.WaitingApprove);
            }
            else if (manualApprovalTab == ManualApprovalTab.WaitDisposal)
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
                statusList.Add(SOApproveDBStatus.Rejected);
                statusList.Add(SOApproveDBStatus.Approved);
            }
            else if (manualApprovalTab == ManualApprovalTab.Extend)
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "true"
                });
                statusList.Add(SOApproveDBStatus.WaitingApprove);
                statusList.Add(SOApproveDBStatus.Rejected);
                statusList.Add(SOApproveDBStatus.Approved);
            }
            else if (manualApprovalTab == ManualApprovalTab.RelatedRecords)
            {
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.IsRelatedRecords,
                    Value = "true"
                });
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
                statusList.Add(SOApproveDBStatus.WaitingApprove);
            }

            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(statusList)
            });
            queryDefinition.PageSize = 1;
            var count = await ManualApprovalQuerier.CountAsync(queryDefinition);
            return (count > 0);
        }

        public RAReturnMessage RunImportUnderReviewDatasJob(string fileName, Stream fileStream)
        {
            {
                Logger.Debug("start Import under review datas");
                RAReturnMessage returnMessage = new();

                try
                {
                    CheckFile(fileName, fileStream, FileExtension.CSV);
                    DateTime dt = DateTime.Now;
                    string uploadFileName = "ImportUnderReviewData_" + dt.Ticks.ToString() + ".csv";
                    var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, uploadFileName);
                    RAStorageUtil.UploadReportBlob(blobName, fileStream);
                    Logger.Info("save file success.");
                    var groupId = TenantLocalValue.LogonGroupId;
                    var loginName = TenantLocalValue.LogonUserEmail;
                    var importParam = new ManualApprovalImportParams();
                    importParam.BlobName = blobName;
                    importParam.FileName = fileName;
                    var importParamStr = SerializerHelper.SerializeByJsonConvert(importParam);
                    JobQueueDto jqDto = new()
                    {
                        JobType = JobType.ManualImportUnderReviewDatasJob,
                        JobRunType = JobRunBy.Control,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName,
                        Parameters = importParamStr,
                    };
                    returnMessage.MessageType = RAMessageType.Successful;
                    returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
                    if (string.IsNullOrEmpty(returnMessage.Extension))
                    {
                        returnMessage = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while run import under review datas job message. Error: {e}");
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = e.Message;
                }

                return returnMessage;
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RunImportUnderReviewJob, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<string> RealRunImportUnderReviewDatasJobAsync(string importParamStr)
        {
            var importParam = SerializerHelper.DeserializeByJsonConvert<ManualApprovalImportParams>(importParamStr);
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            string jobId = JobMonitorService.CreateJob(JobType.ManualImportUnderReviewDatasJob, TenantLocalValue.LogonUserEmail, account.UserId);
            var hasRunningManualJob = JobMonitorService.CheckHasRunningManualJob();
            var hasCurrentUserJob = JobMonitorService.CheckCurrentUserHasRunningJob(account.UserId, jobId);
            var skip = hasCurrentUserJob || hasRunningManualJob;
            if (!skip)
            {
                Logger.Info("Start to import manual under review datas job");
                string content = "\"" + importParam.BlobName + "\"";
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.ManualImportUnderReviewDatasJob,
                    CommandLine = string.Format("{0} {1} {2} {3} {4}", JobType.ManualImportUnderReviewDatasJob, jobId, ".xlsx", content, account.UserId),
                });
                return jobId;
            }
            else
            {
                if (hasRunningManualJob)
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DAM_ManualImport_HasRunningManualJob");
                    return string.Empty;
                }
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DAM_ManualImport_HasRunningManualJob");
                return string.Empty;
            }
        }

        public MAReturnMessage RunFolderViewActionJob(ManualApprovalActionParams actionParameters)
        {
            {
                Logger.Debug("Start run folder view action job");
                MAReturnMessage returnMessage = new();
                try
                {
                    if (actionParameters.ExtendType == ManualApprovalExtendType.Custom)
                    {
                        var extendTime = actionParameters.CustomeExtendDate.Ticks;
                        if (extendTime <= DateTime.UtcNow.Ticks)
                        {
                            throw new Exception(I18NEntity.GetString("RM_MA_ExtendDisposalTime_Valid_EarlierThanNow"));
                        }
                    }
                    var groupId = TenantLocalValue.LogonGroupId;
                    var loginName = TenantLocalValue.LogonUserEmail;
                    var parameter = SerializerHelper.SerializeByJsonSerializer(actionParameters);
                    JobQueueDto jqDto = new()
                    {
                        JobType = JobType.ManualFolderViewActions,
                        Parameters = parameter,
                        JobRunType = JobRunBy.Control,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName
                    };
                    returnMessage.MessageType = RAMessageType.Successful;
                    returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while run folder view action job message. Error: {e}");
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = e.Message;
                }

                return returnMessage;
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RunFolderViewActionJob, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler), IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<string> RealRunFolderViewActionJobAsync(string parameterStr)
        {
            var jobId = string.Empty;
            var jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                var jobType = JobType.ManualFolderViewActions;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = JobMonitorService.CreateJob(jobType, account.UserPrincipalName, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, parameterStr);
                var runningJobs = JobMonitorService.GetRunningJobs(JobType.ManualFolderViewActions);
                var isSkip = runningJobs.Any(j => j != jobId);
                if (!isSkip)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1} {2}", jobType, subJobId, account.UserId),
                    });
                }
                else
                {
                    Logger.Info(I18NEntity.GetString("RM_SYNC_JobSkip"));
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run folder view action job. Error: {e}");
                throw;
            }
            return jobId;
        }

        #region File System manual data upgrade

        public void SendFileSystemManualDataUpgradeJobMessage()
        {
            try
            {
                var count = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.ManualFileSystemUpgrade);
                if (count > 0)
                {
                    Logger.Warn("Manual file system upgrade job already exists.");
                }

                var queue = new JobQueueDto
                {
                    JobType = JobType.ManualFileSystemUpgrade,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = null,
                };

                JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run manual file system upgrade job. Error: {e}");
            }
        }

        public string RealRunFileSystemManualDataUpgradeJob()
        {
            var jobId = string.Empty;
            try
            {
                var username = "RM_TS_RunSchedule";
                var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.ManualFileSystemUpgrade) > 0;
                jobId = JobMonitorService.CreateJob(JobType.ManualFileSystemUpgrade, username);
                if (hasRunningJob)
                {
                    Logger.Warn("A running file system manual data upgrade job already exists.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return jobId;
                }

                Logger.Info($"Real run file system manual data upgrade job: [{jobId}]");
                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.ManualFileSystemUpgrade,
                    CommandLine = $"{JobType.ManualFileSystemUpgrade} {jobId}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run file system manual data upgrade job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        #endregion

        public ManualApprovalCountResult ReadUploadFile(string fileName, Stream fileStream)
        {
            try
            {
                CheckFile(fileName, fileStream, FileExtension.CSV);
                var uploadFileDatas = ExcelUtil.ReadExcelRowCount(fileStream);
                return uploadFileDatas;
            }
            catch (Exception e)
            {
                Logger.Error($"Read Upload file failed,error {e}");
                return new ManualApprovalCountResult();
            }
        }

        public static void CheckFile(string fileName, Stream fileStream, FileExtension fileExtension)
        {
            try
            {
                string extension = fileName.Substring(fileName.LastIndexOf(".") + 1);
                var allowFileExts = fileExtension == FileExtension.CSV ? new List<FileExtension> { FileExtension.CSV } : new List<FileExtension> { FileExtension.XLSX };
                WebUtil.CheckFileExtension(extension, allowFileExts);
                WebUtil.CheckFileHeadCode(fileStream, allowFileExts);
            }
            catch (Exception e)
            {
                Logger.Error($"Check files failed,error :{e}");
                throw;
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ApprovalProcesses, Action = AuditAction.ManualApprovalSetting, IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<bool> SaveApprovalSettingAsync(ManualApprovalSettingInfo settingInfo)
        {
            try
            {
                var commentInfo = settingInfo.CommentSettingInfo;
                var optionResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalCommentOption, ((int)commentInfo.Option).ToString());
                commentInfo.CommentSetting.ManualApprovalQuickReasonInfo.QuickReasonInfo = commentInfo.CommentSetting.ManualApprovalQuickReasonInfo.QuickReasonInfo.Select(qr => qr.Trim()).ToList();
                var commentSettingStr = SerializerHelper.SerializeByJsonConvert(commentInfo.CommentSetting);
                var commentSettingResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalCommentSetting, commentSettingStr);

                var modifyButtonNameJsonStr = SerializerHelper.SerializeByJsonConvert(commentInfo.ModifyButtonName);
                var modifyButtonNameResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalButtonName, modifyButtonNameJsonStr);

                var duration = commentInfo.Duration.ToString();
                var durationResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalDuration, duration);

                var stayManualReviewOptionJsonStr = SerializerHelper.SerializeByJsonConvert(commentInfo.StayManualReviewOption);
                var stayManualReviewOptionResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.StayManualReviewOption, stayManualReviewOptionJsonStr);

                if (commentInfo.EnableAutoApprovedProcess || commentInfo.isRecheckRule)
                {
                    if (!LicenseHelperService.IsNewOpus().GetAwaiter().GetResult())
                    {
                        Logger.Error("old logic can not save auto process,return false");
                        throw new Exception("old logic can not save auto process");
                    }
                }
                var approvalResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.EnableAutoApprovedProcess, (commentInfo.EnableAutoApprovedProcess).ToString());
                var isRecheckRuleResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.IsRecheckRule, (commentInfo.isRecheckRule).ToString());
                var enableDeleteInvalidRecordsResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.EnableDeleteInvalidRecords, (commentInfo.EnableDeleteInvalidRecords).ToString());
                var manager = new ManualApprovalSettingManager();
                var approvalProcessSettingResult = await manager.Update(settingInfo.ApprovalProcessSetting);
                return optionResult && commentSettingResult && modifyButtonNameResult && stayManualReviewOptionResult && approvalResult && approvalProcessSettingResult && isRecheckRuleResult && enableDeleteInvalidRecordsResult;
            }
            catch (Exception e)
            {
                Logger.Error($"Save approval setting info failed, error :{e}");
                return false;
            }
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.SaveApprovalCommentOption, IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<bool> SaveApprovalCommentOptionAsync(ManualApprovalCommentInfos infos)
        {
            try
            {
                //Comment 
                var optionResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalCommentOption, ((int)infos.Option).ToString());
                //qucik reason
                infos.CommentSetting.ManualApprovalQuickReasonInfo.QuickReasonInfo = infos.CommentSetting.ManualApprovalQuickReasonInfo.QuickReasonInfo.Select(qr => qr.Trim()).ToList();
                var commentSettingStr = SerializerHelper.SerializeByJsonConvert(infos.CommentSetting);
                var commentSettingResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalCommentSetting, commentSettingStr);
                //Modify Button Name
                var ModifyButtonNameJsonStr = SerializerHelper.SerializeByJsonConvert(infos.ModifyButtonName);
                var ModifyButtonNameResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalButtonName, ModifyButtonNameJsonStr);
                //Duration
                var duration = infos.Duration.ToString();
                var durationResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalDuration, duration);
                //Stay Manual Review Option
                var stayManualReviewOptionJsonStr = SerializerHelper.SerializeByJsonConvert(infos.StayManualReviewOption);
                var stayManualReviewOptionResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.StayManualReviewOption, stayManualReviewOptionJsonStr);
                //EnableAutoApprovedProcess
                if (infos.EnableAutoApprovedProcess)
                {
                    if (!LicenseHelperService.IsNewOpus().GetAwaiter().GetResult())
                    {
                        Logger.Error("old logic can not save auto process,return false");
                        throw new Exception("old logic can not save auto process");
                    }
                }
                var approvalResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.EnableAutoApprovedProcess, (infos.EnableAutoApprovedProcess).ToString());
                var enableDeleteInvalidRecordsStr = SerializerHelper.SerializeByJsonConvert(infos.EnableDeleteInvalidRecords);
                var enableDeleteInvalidRecordsResult = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.EnableDeleteInvalidRecords, enableDeleteInvalidRecordsStr);
                return optionResult && commentSettingResult && ModifyButtonNameResult && stayManualReviewOptionResult && approvalResult;

            }
            catch (Exception e)
            {
                Logger.Error($"Save approval comment info failed, error :{e}");
                return false;
            }
        }


        public async Task<ManualApprovalCommentInfos> GetApprovalCommentOptionAsync()
        {
            try
            {
                await FunctionSettingDao.NotExistCreateIt(FunctionSettingType.ManualApprovalCommentOption, ((int)ManualApprovalCommentOptions.Optional).ToString());
                var option = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalCommentOption);
                var approvalOption = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.EnableAutoApprovedProcess);
                var recheckRuleOption = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.IsRecheckRule);
                var enableDeleteInvalidRecordsOption = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.EnableDeleteInvalidRecords);
                //qucik reason
                var defaultManualApprovalCommentSetting = new ManualApprovalCommentSetting()
                {
                    ManualApprovalQuickReasonInfo = new ManualApprovalQuickReasonInfo()
                    {
                        NeedQuickReason = false,
                        QuickReasonInfo = new List<string> { "" },
                        IncativeRejectBool = new List<bool>() { },
                    }
                };
                var defaultManualApprovalCommentSettingStr = SerializerHelper.SerializeByJsonConvert(defaultManualApprovalCommentSetting);
                await FunctionSettingDao.NotExistCreateIt(FunctionSettingType.ManualApprovalCommentSetting, defaultManualApprovalCommentSettingStr);
                var manualApprovalCommentSettingStr = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalCommentSetting);
                var manualApprovalCommentSetting = SerializerHelper.DeserializeByJsonConvert<ManualApprovalCommentSetting>(manualApprovalCommentSettingStr);
                //Modify Button Name
                var defaultManualApprovalButtonName = new ManualApprovalModifyName()
                {
                    ManualApprovalModifyButton = new ManualApprovalModifyButtonName()
                    {
                        EnableModifyButtonName = false,
                        ModifiedButtonNames =
                        [
                            new()
                            {
                                EnglishName = I18NEntity.GetString("RM_MA_Approve", CultureInfo.GetCultureInfo(1033)),
                                JapaneseName = I18NEntity.GetString("RM_MA_Approve", CultureInfo.GetCultureInfo(1041)),
                                ChineseName = I18NEntity.GetString("RM_MA_Approve", CultureInfo.GetCultureInfo(2052)),
                                Korean = I18NEntity.GetString("RM_MA_Approve", CultureInfo.GetCultureInfo(1042)),
                            },
                            new()
                            {
                                EnglishName = I18NEntity.GetString("RM_MA_Reject", CultureInfo.GetCultureInfo(1033)),
                                JapaneseName = I18NEntity.GetString("RM_MA_Reject", CultureInfo.GetCultureInfo(1041)),
                                ChineseName = I18NEntity.GetString("RM_MA_Reject", CultureInfo.GetCultureInfo(2052)),
                                Korean = I18NEntity.GetString("RM_MA_Reject", CultureInfo.GetCultureInfo(1042)),
                            }
                        ]
                    }
                };
                var defaultManualApprovalButtonNameJsonStr = SerializerHelper.SerializeByJsonConvert(defaultManualApprovalButtonName);
                await FunctionSettingDao.NotExistCreateIt(FunctionSettingType.ManualApprovalButtonName, defaultManualApprovalButtonNameJsonStr);
                var manualApprovalButtonNameJsonStrFromDb = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalButtonName);
                //old state needs to be updated.
                var oldmanualApprovalButtonName = SerializerHelper.DeserializeByJsonConvert<ManualApprovalModifyName>(manualApprovalButtonNameJsonStrFromDb);
                if (string.IsNullOrEmpty(oldmanualApprovalButtonName.ManualApprovalModifyButton.ModifiedButtonNames[0]?.Korean ?? null))
                {
                    oldmanualApprovalButtonName.ManualApprovalModifyButton.ModifiedButtonNames[0].Korean = I18NEntity.GetString("RM_MA_Approve", CultureInfo.GetCultureInfo(1042));
                    oldmanualApprovalButtonName.ManualApprovalModifyButton.ModifiedButtonNames[1].Korean = I18NEntity.GetString("RM_MA_Reject", CultureInfo.GetCultureInfo(1042));
                    manualApprovalButtonNameJsonStrFromDb = SerializerHelper.SerializeByJsonConvert(oldmanualApprovalButtonName);
                    await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualApprovalButtonName, manualApprovalButtonNameJsonStrFromDb);
                }


                var manualApprovalButtonNameFromDb = SerializerHelper.DeserializeByJsonConvert<ManualApprovalModifyName>(manualApprovalButtonNameJsonStrFromDb);

                var defaultManualApprovalDuration = "10";
                await FunctionSettingDao.NotExistCreateIt(FunctionSettingType.ManualApprovalDuration, defaultManualApprovalDuration);
                var manualApprovalDuration = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalDuration);

                await FunctionSettingDao.NotExistCreateIt(FunctionSettingType.StayManualReviewOption, SerializerHelper.SerializeByJsonConvert(ManualApprovalStayManualReviewOption.Stay));
                var stayManualReviewOptionJsonStrFromDb = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.StayManualReviewOption);
                var stayManualReviewOptionFromDb = SerializerHelper.DeserializeByJsonConvert<ManualApprovalStayManualReviewOption>(stayManualReviewOptionJsonStrFromDb);

                return new ManualApprovalCommentInfos()
                {
                    Option = (ManualApprovalCommentOptions)int.Parse(option),
                    CommentSetting = manualApprovalCommentSetting,
                    ModifyButtonName = manualApprovalButtonNameFromDb,
                    Duration = Convert.ToInt32(manualApprovalDuration),
                    StayManualReviewOption = stayManualReviewOptionFromDb,
                    EnableAutoApprovedProcess = string.IsNullOrEmpty(approvalOption) ? false : Convert.ToBoolean(approvalOption),
                    isRecheckRule = string.IsNullOrEmpty(recheckRuleOption) ? true : Convert.ToBoolean(recheckRuleOption),
                    EnableDeleteInvalidRecords = string.IsNullOrEmpty(enableDeleteInvalidRecordsOption) ? true : Convert.ToBoolean(enableDeleteInvalidRecordsOption)
                };
            }
            catch (Exception e)
            {
                Logger.Error($"Get approval setting failed, error : {e}");
                return new();
            }
        }

        public Task<bool> IsHideReclassifyBtnInManualApproval()
        {
            const string cacheKey = "ManualApproval_IsHideReclassifyBtn";

            return CacheManager.Cache.TryGetAsync(
                cacheKey,
                () =>
                {
                    var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization");
                    var shouldHide = jsonConfig != null && !string.IsNullOrEmpty(jsonConfig.Value);
                    return Task.FromResult(shouldHide);
                },
                TimeSpan.FromMinutes(15));
        }

        private async Task<bool> IsExistFolderViewItem()
        {
            var repository = Repository;
            var (isAdmin, userPermissionIds) = await CheckUserAdminPermission();
            Expression<Func<ManualApprovalRecord, bool>> predicate = item =>
                item.SourceFlag == (int)SourceFlag.OneDrive &&
                item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove &&
                item.ManualExtendTime < DateTime.UtcNow.Ticks &&
                item.IsManualSynced && item.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd &&
                item.RecordStatus != (int)RMRecordStatus.Hidden && item.RecordStatus != (int)RMRecordStatus.RMDeleted
                && item.ParentId != Guid.Empty;

            Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate = item => true;
            Expression<Func<ManualApprovalRecord, string>> selector = item => item.LeafName;
            if (!isAdmin)
            {
                notAdminpredicate = await GetCosmosDBFilterExpressionAsync(userPermissionIds);
            }
            var result = await repository.CountAsyncForRecordItemDistinct(predicate, notAdminpredicate, selector);
            return result > 0;
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
            SubJobDao.CreateJob(subJob);
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }

        #region for migration
        public async Task<(bool, string)> NeedRunManualApproveJob()
        {
            var timerSchedule = (await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.ManualApprovalScheduleTimer)).FirstOrDefault();
            if (timerSchedule == null)
            {
                return (false, "");
            }
            else
            {
                var jobInfo = JobMonitorService.GetRunningJobs(JobType.ManualApprovalTimer);
                if (jobInfo?.Count > 0)
                {
                    return (false, jobInfo.FirstOrDefault());
                }

                var queueCount = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.ManualApprovalTimer);
                if (queueCount > 0)
                {
                    return (false, "");
                }
            }
            var loginName = TenantLocalValue.LogonUserEmail;
            var jobId = JobMonitorService.CreateJob(JobType.ManualApprovalTimer, loginName);
            return (true, jobId);
        }
        #endregion

        #region Permission
        private List<SourceFlag> GetUserPermissionAsync()
        {
            var isSPOLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            var isBoxLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Box);
            var isGoogleLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            var isFSLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            var isILLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);
            var isGControlLicense = TenantService.HasInitGControlPlatForm().Result;
            List<SourceFlag> sources = new List<SourceFlag>();
            if (!isSPOLicense)
            {
                sources.Add(SourceFlag.SharePointOnPrem);
            }
            if (!isBoxLicense)
            {
                sources.Add(SourceFlag.Box);
            }
            if (!isFSLicense)
            {
                sources.Add(SourceFlag.FileSystem);
            }
            if (!isGoogleLicense && !isGControlLicense)
            {
                sources.Add(SourceFlag.Google);
            }
            if (!isILLicense)
            {
                sources.AddRange([SourceFlag.SharePoint, SourceFlag.OneDrive, SourceFlag.Exchange, SourceFlag.Teams]);
            }
            return sources;
        }
        private void FilterPermission(ManualApprovalQueryDefinition queryDefinition)
        {
            var sources = GetUserPermissionAsync();
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.Permission,
                Value = JsonConvert.SerializeObject(sources)
            });

        }
        #endregion

        public bool IsJpmc(bool isJpmc)
        {
            bool EnableJPMCFileSystemFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            if (isJpmc && EnableJPMCFileSystemFeature)
            {
                return true;
            }
            return false;
        }

    }
}
