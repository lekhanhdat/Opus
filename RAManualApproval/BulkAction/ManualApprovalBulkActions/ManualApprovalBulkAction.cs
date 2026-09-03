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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using RAManualApproval.EmailSchedule;
using RAManualApproval.ImportAction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AvePoint.RA.Contract.ControlPlus;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Contract.GoogleOne;
using AvePoint.RA.Contract.RMWeb.Account;
using RAGoogle.Common;

namespace RAManualApproval.BulkAction.ManualApprovalBulkActions
{
    public abstract class ManualApprovalBulkAction
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ManualApprovalBulkAction));

        private static readonly ConcurrentDictionary<Guid, string[]> _reviewerNamesCache = new();

        private static readonly ConcurrentDictionary<ManualApprovalFilterOptions, IFilter> _filterCollection = new();

        private static readonly ManualApprovalRecordRepository _repository = new();

        private static readonly IRMSecurityTrimmingHelper _securityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static readonly IRMFunctionSettingDao _functionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private static readonly IUserService _userService = PlatformWindsorManager.GetService<IUserService>();

        private static readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private static readonly IAccountDao _accountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        
        private readonly HashSet<GControlWorkflowDto> _gControlWorkflowDtos = new();
        
        private static IGControlTaskAssigneeService GControlTaskAssigneeService => PlatformWindsorManager.GetService<IGControlTaskAssigneeService>();

        private static readonly string _commonErrorMessage = "RM_TS_SS_Summary";

        protected static readonly Dictionary<ManualApprovalBulkActionType, string> _manualAppovalActionI18N = new()
        {
            {ManualApprovalBulkActionType.Approve, "RM_MA_Approve" },
            {ManualApprovalBulkActionType.Reject, "RM_MA_Reject" },
            {ManualApprovalBulkActionType.RestartProcess, "RM_MA_ResetManualWorkflow" },
        };

        private static readonly int _pageSize = 500;

        private static List<Guid> _unCheckItemIds = [];
        
        private readonly IPeoplePickerService _peoplePickerService = new PeoplePickerService();
        
        private static ILnkUserGroupDao LnkUserGroupDao => PlatformWindsorManager.GetService<ILnkUserGroupDao>();

        public abstract ManualApprovalBulkActionType ActionType { get; }

        protected abstract Task SucceedAction(Record item, string[] reviewers);

        protected abstract Task ProcessAction(Record item);

        protected abstract List<ManualApprovalFilterDefinition> FilterDefinitions { get; }
        protected abstract List<ManualApprovalFilterDefinition> FilterGControlDefinitions { get; }

        protected abstract List<ManualApprovalRecord> GenerateItems(List<ManualApprovalRecord> Items);

        protected bool FromGControl { get; set; }

        protected string Continuation { get; set; }

        protected int CurrentTotalCount { get; set; }

        protected RMAccount ApprovalAccount { get; set; }

        protected RMSubJob SubJob { get; set; }

        protected ManualApprovalJobParam ManualApprovalInfos { get; set; }

        protected bool HasFSLiscense { get; set; }

        protected bool HasLSPLiscense { get; set; }

        protected bool HasGControlLicense { get; set; }
        
        protected readonly ConcurrentBag<string> _accountCache = new();

        public async Task RunAsync(string jobId, string userId, bool fromGControl)
        {
            _logger.Info($"Begin to executor {ActionType} action");
            FromGControl = fromGControl;

            ManualApprovalBulkActionManager.Init(jobId);

            InitFilterCollection();

            SubJob = _subJobDao.GetSubJob(jobId, true);

            ManualApprovalInfos = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalJobParam>(SubJob.JobContext.Content);

            if (!string.IsNullOrEmpty(userId))
            {
                ApprovalAccount = _accountDao.Find(item => item.UserId == userId && item.IsRemoved == 0);
            }

            TenantLocalValue.LogonUserId = userId;
            
            TenantLocalValue.RequesterType = ManualApprovalInfos.RequesterType;

            HasFSLiscense = _tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);

            HasLSPLiscense = _tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);

            HasGControlLicense = await _tenantService.HasInitGControlPlatForm();

            ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessRecordSucceedAsync, ProcessRecordFailed);

            var queryExpression = await BuildQueryDefinitionAsync(ManualApprovalInfos.QueryDefintion);

            _unCheckItemIds = ManualApprovalInfos.UncheckedItemIds;

            try
            {
                var ManualSettingInfoJson = _functionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult();
                var ManualSettingInfoes = SerializerHelper.DeserializeByJsonConvert<ManualApprovalSettings>(ManualSettingInfoJson);
                var maxDisposalExtendCount = ManualSettingInfoes.DisposalExtentionSetting.MaxDelayTimes;
                do
                {
                    using CheckJobStopScope jScope = new();
                    List<ManualApprovalRecord> items = [];
                    using (new PerformanceScope("Query Datas by query definition"))
                    {
                        items = await QueryItems(queryExpression);
                        CurrentTotalCount += items.Count;
                    }
                    using (new PerformanceScope($"Excute Action", $"Action Type :[{ActionType}], item count: {items.Count}", true))
                    {
                        foreach (var item in items)
                        {
                            if (_unCheckItemIds.Contains(item.Id))
                            {
                                continue;
                            }
                            if (item.ManualExtendTime >= DateTime.UtcNow.Ticks) 
                            {
                                _logger.Error($"extend items  cant  approval/reject by system");
                                continue;
                            }

                            var reviewers = ManualApprovalInfos.QueryDefintion.FromGControl ? GetReviewers([item.GControlCurrentApproverId]).Union(GetReviewers(item.GControlManualReviewers ?? [])).ToArray() : GetReviewers(item.ManualReviewer);

                            if (reviewers.Length == 0)
                            {
                                ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_NoOwner");
                                _logger.Error($"Current item [{item.Id}] not found owners.");
                                continue;
                            }

                            try
                            {
                                if (!HasFSLiscense && item.SourceFlag == (int)SourceFlag.FileSystem)
                                {
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_NoLicense");
                                    continue;
                                }

                                if (!HasLSPLiscense && item.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                                {
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_NoLicense");
                                    continue;
                                }

                                if (!HasGControlLicense && item.IsGControlRecord)
                                {
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_NoLicense");
                                    continue;
                                }

                                if ((SOApproveDBStatus)ActionType == SOApproveDBStatus.Rejected 
                                    && item.ManualExtendCount >= maxDisposalExtendCount
                                    && !IsRequestFromGControl(fromGControl, item.IsGControlRecord))
                                {
                                    throw new Exception("RM_MA_MaxRejectExtendDisposalDate");
                                }

                                await ProcessAction(item);
                                ManualApprovalDataSyncManager.Add(item);
                                _reviewerNamesCache.TryAdd(item.Id, reviewers);
                            }
                            catch (Exception e)
                            {
                                if (e.Message.Contains("RM_JS_MA_ItemDisposal"))
                                {
                                    _logger.Info($"Item has been disposal, can not restart workflow. e: {e.Message} ");
                                    ManualApprovalBulkActionManager.AddSkippedJobDetail(item, 0, _manualAppovalActionI18N[ActionType], reviewers, "RM_JS_MA_ItemDisposal");
                                }
                                else if (e.Message.Contains("RM_MA_MaxRejectExtendDisposalDate"))
                                {
                                    _logger.Info($"Item max disposal date , can not reject. e: {e.Message} ");
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_MaxRejectExtendDisposalDate");
                                }
                                else if (e.Message.Contains("RM_MA_ExtendDisposalTime_Valid_EarlierThanNow"))
                                {
                                    _logger.Info($"Item extend time <= date time ticks. e: {e.Message} ");
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_ExtendDisposalTime_Valid_EarlierThanNow");
                                }
                                else if (e.Message.Contains("Sequence contains no elements") || e.Message.Contains($"The item does not exist"))
                                {
                                    _logger.Info($"No elements or No exist - >  e: {e.Message} ");
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_WF_NoElements");
                                }
                                else if (e.Message.Contains($"Workflow [{item.ManualWorkflowDefinitionId}] not exists."))
                                {
                                    _logger.Info($"No elementsbus1 - >  e: {e.Message} ");
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], "RM_MA_WF_NoWorkflow");
                                }
                                else
                                {
                                    _logger.Error($"An error occurred process item, item id : [{item.Id}], action : [{ActionType}]. Error: {e}");
                                    ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewers, _manualAppovalActionI18N[ActionType], e.Message);
                                }
                            }
                        }
                    }
                    if (CurrentTotalCount >= 10000)
                    {
                        ManualApprovalDataSyncManager.Commit();
                        ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessRecordSucceedAsync, ProcessRecordFailed);
                        CurrentTotalCount = 0;
                    }
                } while (!string.IsNullOrEmpty(Continuation));

                ManualApprovalDataSyncManager.WaitComplete();
                ManualApprovalBulkActionManager.SetJobFinished();
                await GControlTaskAssigneeService.BatchAddAsync(_gControlWorkflowDtos.ToList());
                PerformanceMonitor.WritePerformanceResult();
            }
            catch (JobStopException ex)
            {
                ManualApprovalBulkActionManager.SetJobStopped(ex.Message);
                _logger.Info("Stop job success!");
            }
            catch (Exception e)
            {
                ManualApprovalBulkActionManager.SetJobFailed(_commonErrorMessage);
                _logger.Error($"An error occurred while process job. Error: {e}");
            }

        }

        private bool IsRequestFromGControl(bool isGControlRequest, bool isGControlRecord)
        {
            return isGControlRecord && isGControlRequest;
        }

        protected void AddNewWorkflowDto(Guid workflowDefinitionId, Guid workflowStepId)
        {
            if (FromGControl)
            {
                _gControlWorkflowDtos.Add(new GControlWorkflowDto
                {
                    WorkflowId = workflowDefinitionId,
                    StageId = workflowStepId,
                    Status = (int)ActionType switch
                    {
                        (int) SOApproveDBStatus.Approved => ApprovalProcessStatus.Approved,
                        _ => ApprovalProcessStatus.Rejected
                    }
                });
            }
        }
        
                
        protected async Task UpdateGoogleUserAsync(string userId)
        {
            var (account, members) = await _peoplePickerService.GetDirectoryAndUsersInGroupTypeDirectoryAsync(userId);
            if (account != null)
            {
                var neededAddAccounts = new List<AccountDto>() { account };
                if (account.ObjectType == RMActiveDirectoryObjectType.Group && members.IsNotNullOrEmpty())
                {
                    neededAddAccounts.AddRange(members);
                    await LnkUserGroupDao.AddUsersInGroupAsync(members.Select(item => item.UserId), account.UserId);
                }
                await _userService.BatchAddAccountsAsync(neededAddAccounts);
                _userService.SaveUsersToBuiltInGroup([userId]);
            }
        }

        protected async Task<bool> CheckGGUserExistenceInDB(string userId)
        {
            return await _userService.GetGoogleUserAsync(userId) != null;
        }

        protected bool NeedApprovalOrRejectForWorkflow(Record item)
        {
            if (FromGControl)
            {
                return item.GControlApprovalProcessId != Guid.Empty.ToString() && item.GControlCurrentStageId != Guid.Empty.ToString();
            }
            return item.ManualWorkflowInstanceId != Guid.Empty || (item.ManualWorkflowDefinitionId != Guid.Empty && item.ManualWorkflowStepId != Guid.Empty);
        }

        private async Task<List<ManualApprovalRecord>> QueryItems(List<Expression<Func<ManualApprovalRecord, bool>>> filterExpresions)
        {
            var repository = _repository;

            var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
            {
                PageSize = _pageSize,
                Continuation = Continuation,
                Predicates = filterExpresions,
            };

            var explorerQueryResult = await repository.QueryItemsWithPaginationAsync(explorerQueryDefinition);
            Continuation = explorerQueryResult.Continuation;
            return GenerateItems(explorerQueryResult.Items);
        }

        private async Task ProcessRecordSucceedAsync(Record item)
        {
            using (new PerformanceScope("Update Archiver and Add history"))
            {
                await SucceedAction(item, _reviewerNamesCache[item.Id]);
                _reviewerNamesCache.Remove(item.Id, out var reviewer);
            }
        }

        private void ProcessRecordFailed(Record item, string message)
        {
            var reviewerNames = _reviewerNamesCache[item.Id];
            ManualApprovalBulkActionManager.AddFailedJobDetail(item, (int)ActionType, reviewerNames, _manualAppovalActionI18N[ActionType], message);
            _reviewerNamesCache.Remove(item.Id, out var reviewer);
        }

        private async Task<List<Expression<Func<ManualApprovalRecord, bool>>>> BuildQueryDefinitionAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            queryDefinition.PageSize = _pageSize;

            queryDefinition.NeedCalculationCount = false;

            if(queryDefinition.FromGControl)
            {
                queryDefinition.Filters.AddRange(FilterGControlDefinitions);
            }
            else
            {
                queryDefinition.Filters.AddRange(FilterDefinitions);
            }


            await PrePermissionValidateAsync(queryDefinition);

            return await BuildCosmosDBFilterAsync(queryDefinition);
        }

        private static async Task<List<Expression<Func<ManualApprovalRecord, bool>>>> BuildCosmosDBFilterAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var result = new List<Expression<Func<ManualApprovalRecord, bool>>>();
            foreach (var filterDefinition in queryDefinition.Filters)
            {
                var filterOption = filterDefinition.FilterOption;
                var filter = _filterCollection[filterOption];
                var expression = await filter.GetCosmosDBFilterExpressionAsync(filterDefinition.Value);
                result.Add(expression);
            }

            return result;
        }

        private async Task PrePermissionValidateAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var isAdmin = await _securityTrimmingHelper.DoesUserHasThisPermissionAsync(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            if (isAdmin)
            {
                return;
            }
            if (FromGControl)
            {
                if (TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
                {
                    return;
                }
                var googleReviewerFilter = new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.GControlReviewer,
                    Value = "[]"
                };
                queryDefinition.Filters.Add(googleReviewerFilter);

                var googleUserHasPermissionIntIds = _userService.GetUserWithRemovedAndGroupIds(ApprovalAccount.UserId);
                googleReviewerFilter.Value = JsonConvert.SerializeObject(googleUserHasPermissionIntIds);
                return;
            }
            var reviewerFilter = new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.Reviewer,
                Value = "[]"
            };
            queryDefinition.Filters.Add(reviewerFilter);

            var userHasPermissionIntIds = _userService.GetUserWithRemovedAndGroupIds(ApprovalAccount.UserId);
            reviewerFilter.Value = JsonConvert.SerializeObject(userHasPermissionIntIds);
        }

        private static void InitFilterCollection()
        {
            try
            {
                var filterType = typeof(IFilter);
                var assembly = Assembly.GetAssembly(filterType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(filterType))
                    {
                        var instance = Activator.CreateInstance(type) as IFilter;
                        _filterCollection.TryAdd(instance.FilterOption, instance);
                    }
                }
                _logger.Info($"Succeed init filter collection.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init filter collection. Error: {e}");
                throw;
            }
        }

        private static string[] GetReviewers(int[] reviewerIds)
        {
            var reviewerNames = Array.Empty<string>();
            try
            {
                reviewerNames = ManualApprovalOwnerManager.GetOwnerDisplayNames(reviewerIds).ToArray();
                return reviewerNames;
            }
            catch (Exception e)
            {
                _logger.Error($"Get owner display names failed,{e}");
                return reviewerNames;
            }
        }
        private static string[] GetReviewers(List<string> reviewerIds)
        {
            var reviewerNames = Array.Empty<string>();
            try
            {
                reviewerNames = ManualApprovalOwnerManager.GetOwnerDisplayNamesByUserIds(reviewerIds).ToArray();
                return reviewerNames;
            }
            catch (Exception e)
            {
                _logger.Error($"Get owner display names failed,{e}");
                return reviewerNames;
            }
        }
    }
}
