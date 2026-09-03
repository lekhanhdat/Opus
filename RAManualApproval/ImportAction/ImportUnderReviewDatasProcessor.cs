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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RAManualApproval.ImportAction
{
    public class ImportUnderReviewDatasProcessor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ImportUnderReviewDatasProcessor));

        private static readonly IRMWorkflowDefinitionDao RMWorkflowDefinitionDao = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private static readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly IRMEmailItemDao EmailItemDao = PlatformWindsorManager.GetService<IRMEmailItemDao>();

        private static readonly IWorkflowInstanceDao WorkflowInstanceDao = PlatformWindsorManager.GetService<IWorkflowInstanceDao>();

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly IUserService UserService = PlatformWindsorManager.GetService<IUserService>();

        private static readonly IRMSecurityTrimmingHelper SecurityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static readonly IRMFunctionSettingDao RMFunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;

        private static readonly ITenantService s_tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private static ManualApprovalRecordRepository Repository => new();

        private static HistoryAddAction AddAction => new();

        private int TotalCount { get; set; }

        private int CurrentTotalCount { get; set; }

        private readonly string FilePath;

        private readonly RMAccount ApprovalAccount;

        private readonly string approve = I18NEntity.GetString("RM_MA_Approve");

        private readonly string reject = I18NEntity.GetString("RM_MA_Reject");

        private readonly ConcurrentDictionary<Guid, RMManualApproveHistoryTableEntity> RecordIdHistoryMapping = new();

        private readonly ConcurrentDictionary<Guid, string[]> RecordIdReviewerMapping = new();

        private static readonly RMWorkflowProcessor _workflowProcessor = new();

        private readonly SyncItemArchiverStatusAction _syncArchiverStatusAction = new();

        private readonly bool _hasFSLiscense;

        private readonly bool _hasLSPLiscense;

        public ImportUnderReviewDatasProcessor(string jobId, string blobPath, string logonUserId)
        {
            ImportUnderReviewDatasManager.Init(jobId);
            if (!string.IsNullOrEmpty(logonUserId))
            {
                ApprovalAccount = AccountDao.Find(item => item.UserId == logonUserId && item.IsRemoved == 0);
            }

            try
            {
                FilePath = JobReportUtility.GetImportJobCSVFile(blobPath);
            }
            catch (Exception e)
            {
                Logger.Error("can not download file,error:{0}", e.ToString());
                throw;
            }

            _hasFSLiscense = s_tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            _hasLSPLiscense = s_tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
        }

        public async System.Threading.Tasks.Task RunAsync()
        {

            ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessItemSucceedAsync, ProcessItemFailed);
            try
            {
                await ReadAllCellValuesAndProcessForCsvAsync();
                Logger.Info($"Process total count is {TotalCount}");

            }
            catch (Exception e)
            {
                Logger.Error($"Read and Process records failed, error {e}");
            }

            if (TotalCount == 0)
            {
                ImportUnderReviewDatasManager.JobComment = "RM_DAM_ManualImport_NoDataCanAction";
            }

            ManualApprovalDataSyncManager.WaitComplete();
            ImportUnderReviewDatasManager.SetJobFinished();
            PerformanceMonitor.WritePerformanceResult();
        }

        private async System.Threading.Tasks.Task ReadAllCellValuesAndProcessForCsvAsync()
        {
            using (new PerformanceScope("Read csv file and process items", "", true))
            {
                using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var recordsList = new List<RecordItem>();

                //Skip header
                var line = string.Empty;
                var special = false;
                var rowStr = string.Empty;
                while (!string.IsNullOrWhiteSpace((line = await reader.ReadLineAsync())))
                {
                    rowStr += line;
                    int remainder = (line.Split(new char[] { '"' }, StringSplitOptions.None).Length - 1) % 2;
                    if (remainder != 0)
                    {
                        if (special)
                        {
                            special = false;
                        }
                        else
                        {
                            rowStr += System.Environment.NewLine;
                            special = true;
                            continue;
                        }
                    }
                    else
                    {
                        if (special)
                        {
                            rowStr += System.Environment.NewLine;
                            continue;
                        }
                    }

                    var itemFields = CSVHelper.AnalyseCSVRow2ArrayForManualImport(rowStr);
                    rowStr = null;
                    if (!itemFields.Any() || itemFields.All(string.IsNullOrEmpty))
                    {
                        continue;
                    }

                    var record = GetRecordItem(itemFields);
                    if (record != null)
                    {
                        recordsList.Add(record);
                    }

                    if (recordsList.Count != 1000)
                    {
                        continue;
                    }

                    var items = await QueryItemsAsync(recordsList);
                    if (items.Any())
                    {
                        await ProcessRecordsAsync(recordsList, items);
                        TotalCount += items.Count;
                    }
                    recordsList = new List<RecordItem>();
                }

                if (recordsList.Any())
                {
                    var items = await QueryItemsAsync(recordsList);
                    if (items.Any())
                    {
                        await ProcessRecordsAsync(recordsList, items);
                        TotalCount += items.Count;
                    }
                }
            }         
        }

        private RecordItem GetRecordItem(string[] itemFields)
        {
            try
            {
                var record = new RecordItem
                {
                    ApprovalStatus = itemFields[0]?.Trim().ToLower(),
                    ReviewNames = itemFields[15]?.ToLower(),
                    CollectionTime = itemFields[21]?.ToLower(),
                    Id = itemFields[22]?.ToLower(),
                    ActionTime = itemFields[23]?.ToLower(),
                    QuickReason = itemFields[2] ?? string.Empty,
                    ManualApprovalComment = itemFields[3] ?? string.Empty,
                    ExtendDisposalDate = itemFields[4] ?? string.Empty,
                };

                return record;
            }
            catch (Exception ex)
            {
                Logger.Error($"itemFields.Count:{itemFields?.Length}, itemFields.Content: {string.Join('|', itemFields)}, error: {ex}");
                throw;
            }
        }

        private async System.Threading.Tasks.Task ProcessItemSucceedAsync(Record item)
        {
            try
            {
                var reviewers = GetReviewers(item.ManualReviewer);
                var approveStatus = item.ManualApprovedStatus;
                if (RecordIdReviewerMapping.ContainsKey(item.Id))
                {
                    reviewers = RecordIdReviewerMapping[item.Id];
                }

                if (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved ||
                    item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                {

                    await _syncArchiverStatusAction.UpdateItemArchiverStatusAsync(item);
                }

                if (RecordIdHistoryMapping.ContainsKey(item.Id))
                {
                    var historyData = RecordIdHistoryMapping[item.Id];
                    await AddAction.AddAsync(historyData);
                    approveStatus = historyData.ApprovedStatus;
                    Logger.Info($"Succeed insert item [{item.Id}] to history table.");
                }
                ImportUnderReviewDatasManager.AddSucceedJobDetail(item, approveStatus, reviewers);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while process history item succeed. Error: {e}");
                ManualApprovalJobManager.AddFailedJobDetail(item, ManualApprovalAction.Import, e.Message);
            }
            RecordIdHistoryMapping.Remove(item.Id, out var history);
            RecordIdReviewerMapping.Remove(item.Id, out var reviewer);
        }

        private void ProcessItemFailed(Record item, string errorMessage)
        {
            if (RecordIdHistoryMapping.ContainsKey(item.Id))
            {
                RecordIdHistoryMapping.Remove(item.Id, out var value);
                Logger.Info($"Process item failed, remove history data {value.RowKey}");
            }
            if (RecordIdReviewerMapping.ContainsKey(item.Id))
            {
                RecordIdReviewerMapping.Remove(item.Id, out var value);
                Logger.Info($"Process item failed, remove record reviewer");
            }
            ManualApprovalJobManager.AddFailedJobDetail(item, ManualApprovalAction.Import, errorMessage);
        }


        private async System.Threading.Tasks.Task ProcessRecordsAsync(List<RecordItem> excelDatas, List<ManualApprovalRecord> items)
        {
            using (new PerformanceScope("Process Records", $"records count {items.Count}", true))
            {
                var ManualSettingInfoJson = RMFunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult();
                var ManualSettingInfoes = SerializerHelper.DeserializeByJsonConvert<ManualApprovalSettings>(ManualSettingInfoJson);
                var maxDisposalExtendCount = ManualSettingInfoes.DisposalExtentionSetting.MaxDelayTimes;
                var idApprovalStatesMapping = new Dictionary<string, string>();
                try 
                {
                    idApprovalStatesMapping = excelDatas.ToDictionary(data => data.Id, data => data.ApprovalStatus);
                }
                catch(Exception ex) 
                {
                    var theSameItemIds = excelDatas.GroupBy(data => data.Id).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
                    if (theSameItemIds.Any())
                    {
                        var matchingItems = items.Where(item => theSameItemIds.Contains(item.Id.ToString()));

                        foreach (var matchingItem in matchingItems)
                        {
                            ImportUnderReviewDatasManager.AddFailedJobDetail(matchingItem, (int)SOApproveDBStatus.Failed, GetReviewers(matchingItem.ManualReviewer), "RM_DAM_ManualImport_ImportHastheSameItemIds");
                        }
                        excelDatas.RemoveAll(item => theSameItemIds.Contains(item.Id.ToString()));
                        items.RemoveAll(item => theSameItemIds.Contains(item.Id.ToString()));
                    }
                    if (excelDatas.Any())
                    {
                        idApprovalStatesMapping = excelDatas.ToDictionary(data => data.Id, data => data.ApprovalStatus);
                    }
                }

                CurrentTotalCount += items.Count;
                foreach (var item in items)
                {
                    var approvalStatus = idApprovalStatesMapping[item.Id.ToString()].ToLowerInvariant() == approve.ToLowerInvariant() ? 3 : 4;
                    var reviewers = GetReviewers(item.ManualReviewer);
                    if (idApprovalStatesMapping[item.Id.ToString()].ToLowerInvariant() != approve.ToLowerInvariant()
                        && idApprovalStatesMapping[item.Id.ToString()].ToLowerInvariant() != reject.ToLowerInvariant())
                    {
                        ImportUnderReviewDatasManager.AddSkippedJobDetail(item, item.ManualApprovedStatus, GetReviewers(item.ManualReviewer), "RM_DAM_ManualImport_ActionNotRight");
                        continue;
                    }

                    if (!_hasFSLiscense && item.SourceFlag == (int)SourceFlag.FileSystem)
                    {
                        ImportUnderReviewDatasManager.AddFailedJobDetail(item, approvalStatus, reviewers, "RM_MA_NoLicense");
                        continue;
                    }

                    if (!_hasLSPLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.SharePointOnPrem))
                    {
                        ImportUnderReviewDatasManager.AddFailedJobDetail(item, approvalStatus, reviewers, "RM_MA_NoLicense");
                        continue;
                    }

                    using (new PerformanceScope("Process records", $"approval status is {approvalStatus}", true))
                    {
                        if (await CheckRecordsIsChanged(item, excelDatas, maxDisposalExtendCount))
                        {
                            if (item.ManualWorkflowInstanceId != Guid.Empty || (item.ManualWorkflowDefinitionId != Guid.Empty && item.ManualWorkflowStepId != Guid.Empty))
                            {
                                foreach (var excelData in excelDatas)
                                {
                                    if (excelData.Id == item.Id.ToString())
                                    {
                                        await ProcessWorkflowRecordsAsync(item, approvalStatus, reviewers, excelData);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var excelData in excelDatas)
                                {
                                    if (excelData.Id == item.Id.ToString())
                                    {
                                       await ProcessWaitingRecords(item, approvalStatus, reviewers,excelData);
                                    }
                                }   
                            }
                        }
                    }
                }
                if (CurrentTotalCount >= 10000)
                {
                    ManualApprovalDataSyncManager.Commit();
                    ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessItemSucceedAsync, ProcessItemFailed);
                    CurrentTotalCount = 0;
                }
            }
        }

        private static async Task<List<ManualApprovalRecord>> QueryItemsAsync(List<RecordItem> excelDatas)
        {
            using (new PerformanceScope("Query item from cosmos db", $"Query count is {excelDatas.Count}", true))
            {
                var repository = Repository;
                var recordIds = excelDatas.Select(item => item.Id).ToList();
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
                var userHasPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
                // to do 
                var items = await repository.QueryItemsAsync(record => recordIds.Contains(record.Id.ToString()));
                if (!isAdmin)
                {
                    var currentItems = items.Where(item => item.ManualReviewer.ToList().Any(i => userHasPermissionIntIds.Contains(i))).ToList();
                    var otherItems = items.Except(currentItems).ToList();
                    foreach (var item in otherItems)
                    {
                        var reviewerNames = GetReviewers(item.ManualReviewer);
                        ImportUnderReviewDatasManager.AddSkippedJobDetail(item, item.ManualApprovedStatus, reviewerNames, "RM_DAM_ManualImport_NoCurrentUserData");
                    }
                    return currentItems;
                }
                return items;
            }
        }

        private static async Task<bool> CheckRecordsIsChanged(ManualApprovalRecord item, List<RecordItem> dataList,int maxDisposalExtendCount)
        {
            using (new PerformanceScope("Check record is changed"))
            {
                var reviewerNames = GetReviewers(item.ManualReviewer);
                var fileData = dataList.Where(data => data.Id == item.Id.ToString()).FirstOrDefault();
                if(fileData == null)
                {
                    Logger.Info($"Can not find item in import file item id : {item.Id}");
                    return false;
                }
                //判断Quick Reason  type是6 
                var QuickReasonSettingInfo = RMFunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalCommentSetting).GetAwaiter().GetResult();
                var QucikInfo = SerializerHelper.DeserializeByJsonConvert<ManualApprovalCommentSetting>(QuickReasonSettingInfo);
                if (!string.IsNullOrEmpty(fileData.QuickReason.Trim()) && fileData.ApprovalStatus.Equals("reject"))
                {
                    if (!QucikInfo.ManualApprovalQuickReasonInfo.NeedQuickReason)
                    {
                        ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Failed, reviewerNames, "RM_DAM_ManualImport_NotConfigrationQuickReason");
                        return false;
                    }
                    if (fileData.QuickReason.Trim().Length > 255 )
                    {
                        ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Failed, reviewerNames, "RM_TM_CustomProperties_QuickReasonTooLong");
                        return false;
                    }
                    if (!QucikInfo.ManualApprovalQuickReasonInfo.QuickReasonInfo.Contains(fileData.QuickReason.Trim()))
                    {
                        ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Failed, reviewerNames, "RM_DAM_ManualImport_NotFieldWithQuickReason");
                        return false;
                    }
                    if (QucikInfo.ManualApprovalQuickReasonInfo?.IncativeRejectBool != null) 
                    {
                        List<string> quickReasonInfoList = QucikInfo.ManualApprovalQuickReasonInfo.QuickReasonInfo.ToList();
                        List<bool> incativeRejectBoolList = QucikInfo.ManualApprovalQuickReasonInfo.IncativeRejectBool.ToList();
                        List<string> result = new List<string>();
                        for (int i = 0; i < incativeRejectBoolList.Count; i++)
                        {
                            if (!incativeRejectBoolList[i])
                            {
                                result.Add(quickReasonInfoList[i]);
                            }
                        }
                        if (!result.Contains(fileData.QuickReason.Trim())) 
                        {
                            ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Failed, reviewerNames, "RM_DAM_ManualImport_QuickReasonDisable");
                            return false;
                        }
                    }
                }
                   
               if (string.IsNullOrEmpty(fileData.QuickReason.Trim()) && QucikInfo.ManualApprovalQuickReasonInfo.NeedQuickReason && fileData.ApprovalStatus.Equals("reject"))
               {
                    ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Failed, reviewerNames, "RM_DAM_ManualImport_PleaseFillYourQuickReason");
                    return false;
               }
                //判断extend disposal date  type是1             
                if (fileData.ApprovalStatus.Equals("reject") && item.ManualExtendCount >= maxDisposalExtendCount) 
                {
                    ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Failed, reviewerNames, "RM_MA_MaxRejectExtendDisposalDate");
                    return false;    
                }        
                
                var ManualApproveSettingInfo = RMFunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalCommentOption).GetAwaiter().GetResult();
                //  1.Approve Reject 必須     2.Approve  必須    3.Reject必須    4. 都可以填或者不填
                if (ManualApproveSettingInfo.Equals("1") && string.IsNullOrEmpty(fileData.ManualApprovalComment.Trim()))
                {
                    if (fileData.ApprovalStatus.Equals("approve"))
                    {
                        ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Approved, reviewerNames, "RM_DAM_ManualImport_NotConfigration");
                        return false;
                    }
                    else
                    {
                        ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Rejected, reviewerNames, "RM_DAM_ManualImport_NotConfigration");
                        return false;
                    }
                }
                if (ManualApproveSettingInfo.Equals("2") && fileData.ApprovalStatus.Equals("approve") && string.IsNullOrEmpty(fileData.ManualApprovalComment.Trim()))
                {
                    ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Approved, reviewerNames, "RM_DAM_ManualImport_NotConfigration");
                    return false;
                }
                if (ManualApproveSettingInfo.Equals("3") && fileData.ApprovalStatus.Equals("reject") && string.IsNullOrEmpty(fileData.ManualApprovalComment.Trim()))
                {
                    ImportUnderReviewDatasManager.AddFailedJobDetail(item, (int)SOApproveDBStatus.Rejected, reviewerNames, "RM_DAM_ManualImport_NotConfigration");
                    return false;
                }
                var idActionTimeMapping = dataList.ToDictionary(data => data.Id, data => data.ActionTime);
                var actionTime = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualActionTime, true).SimplifyFormatTime;
                if (actionTime.ToLowerInvariant() != idActionTimeMapping[item.Id.ToString()].ToLowerInvariant())
                {
                    if(item.ManualActionTime != 0 && idActionTimeMapping[item.Id.ToString()] != "0")
                    {
                        ImportUnderReviewDatasManager.AddSkippedJobDetail(item, item.ManualApprovedStatus, reviewerNames, "RM_DAM_ManualImport_HasBeenActioned");
                        Logger.Info($"Current record [{item.Id}] has already been processed, processed by : {GetReviewers(new int[] { item.ManualApprovedBy }).FirstOrDefault()}");
                        return false;
                    }
                    else if (item.ManualActionTime != 0 && idActionTimeMapping[item.Id.ToString()] == "0")
                    {
                        ImportUnderReviewDatasManager.AddSkippedJobDetail(item, item.ManualApprovedStatus, reviewerNames, "RM_DAM_ManualImport_HasBeenActioned");
                        Logger.Info($"Current record [{item.Id}] has already been processed, processed by : {GetReviewers(new int[] { item.ManualApprovedBy }).FirstOrDefault()}");
                        return false;
                    }
                }
                var collectTime = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualCollectionTime, true).SimplifyFormatTime;
                if (collectTime.ToLowerInvariant() != fileData.CollectionTime.ToLowerInvariant())
                {
                    ImportUnderReviewDatasManager.AddSkippedJobDetail(item, item.ManualApprovedStatus, reviewerNames, "RM_DAM_ManualImport_ReCollected");
                    return false;
                }
                if (item.ManualApprovedStatus != (int)SOApproveDBStatus.WaitingApprove)
                {
                    ImportUnderReviewDatasManager.AddSkippedJobDetail(item, item.ManualApprovedStatus, reviewerNames, "RM_DAM_ManualImport_ApprovalFinished");
                    return false;
                }
                if (item.ManualExtendTime > DateTime.UtcNow.Ticks)
                {
                    ImportUnderReviewDatasManager.AddSkippedJobDetail(item, item.ManualApprovedStatus, reviewerNames, "RM_DAM_ManualImport_HasBeenExtend");
                    return false;
                }
                return true;
            }
        }

        private async System.Threading.Tasks.Task ProcessWorkflowRecordsAsync(ManualApprovalRecord item, int approvalStatus, string[] reviewerNames,RecordItem excelData)
        {
            try
            {
                await ApprovalOrRejectForWorkflowNewAsync(item, approvalStatus, reviewerNames, excelData);
            }
            catch (Exception e)
            {
                Logger.Error($"Approve or reject for process in progress datas failed , error : {e}");
                ImportUnderReviewDatasManager.AddFailedJobDetail(item, approvalStatus, reviewerNames, e.Message);
            }
        }

        private async System.Threading.Tasks.Task ProcessWaitingRecords(ManualApprovalRecord item, int approvalStatus, string[] reviewerNames,RecordItem excelData)
        {
            using (new PerformanceScope("Add item to Data Sync Manager", $"item id : {item.Id} ", true))
            {
                try
                {
                    item.ManualApprovalComment = excelData.ManualApprovalComment;
                    item.QuickReason = approvalStatus == (int)SOApproveDBStatus.Rejected ? excelData.QuickReason : string.Empty;
                    item.ManualLastReasonForRejection = item.QuickReason;
             
                    if (item.SourceFlag >= 1000)
                    {
                        var historyData = AddAction.Convert(item, (SOApproveDBStatus)approvalStatus, ApprovalAccount.Id);
                        RecordIdHistoryMapping[item.Id] = historyData;
                    }

                    item.ManualInternalApprovedStatus = approvalStatus;
                    item.ManualApprovedStatus = approvalStatus;
                    item.ManualApprovedBy = ApprovalAccount.Id;
                    item.ManualActionTime = DateTime.UtcNow.Ticks;
                    item.ManualLastApproveRejectComment = excelData.ManualApprovalComment;
                    item.ManualLastReviewedBy = ApprovalAccount.DisplayName;
                    item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
                    if (item.SourceFlag == (int)SourceFlag.Physical || item.SourceFlag > (int)SourceFlag.Connector)
                    {
                        item.DisposalStatus = approvalStatus;
                    }

                    var (extendType,extendNumber) = CheckExtendDisposalTime(excelData.ExtendDisposalDate.Trim());
                    var customDateTime = DateTime.UtcNow;
                    if (extendType == ManualApprovalExtendType.Custom)
                    {
                        var addDay = int.Parse(excelData.ExtendDisposalDate);
                        customDateTime = customDateTime.AddDays(addDay);
                    }
                    if (approvalStatus == (int)SOApproveDBStatus.Rejected)
                    {           
                        item.ManualExtendTime = await CalculationExtendTimeAsync(extendType, extendNumber, customDateTime);
                        item.ManualExtendComment = string.Empty;
                        item.ManualExtendCount += 1;
                    }

                    await ManualApprovalAzureTableManager.RebuildAuditsAsync(item, (SOApproveDBStatus)approvalStatus, ApprovalAccount, extendType, extendNumber, customDateTime);

                    ManualApprovalDataSyncManager.Add(item);
                }
                catch (Exception e)
                {
                    Logger.Error($"Approve or reject For waiting for approval datas failed, error : {e}");
                    ImportUnderReviewDatasManager.AddFailedJobDetail(item, approvalStatus, reviewerNames, e.Message);
                }
            }
        }

        private async System.Threading.Tasks.Task ApprovalOrRejectForWorkflowNewAsync(ManualApprovalRecord item, int approvalStatus, string[] reviewers, RecordItem excelData)
        {
            var repository = Repository;
            var workflowDefinitionId = item.ManualWorkflowDefinitionId;
            var workflowStepId = item.ManualWorkflowStepId;
            if (item.ManualWorkflowInstanceId != Guid.Empty && item.ManualWorkflowDefinitionId == Guid.Empty && item.ManualWorkflowStepId == Guid.Empty)
            {
                var instance = await RMWorkflowDefinitionDao.GetWorkflowInstanceAsync(item.ManualWorkflowInstanceId);
                workflowDefinitionId = instance.DefinitionId;
                workflowStepId = new Guid(instance.CurStepId);

                item.ManualWorkflowInstanceId = Guid.Empty;
                item.ManualWorkflowDefinitionId = workflowDefinitionId;

                await WorkflowInstanceDao.UpdateStatusAsync(instance.Id, RMWorkflowStatus.Completed);
            }

            var workflowInstance = await _workflowProcessor.LoadAsync(workflowDefinitionId);
            var currentStep = workflowInstance.LoadStep(workflowStepId);

            var nextStep = currentStep;
            if (approvalStatus == (int)SOApproveDBStatus.Approved)
            {
                nextStep = currentStep.Approve();
            }
            else
            {
                nextStep = currentStep.Reject();
            }
            item.QuickReason = excelData.QuickReason;
            item.ManualLastReasonForRejection = string.Empty;
            item.ManualApprovalComment = excelData.ManualApprovalComment;
            var historyData = AddAction.Convert(item, (SOApproveDBStatus)approvalStatus, ApprovalAccount.Id);
            RecordIdReviewerMapping[item.Id] = reviewers;

            item.ManualWorkflowStepId = nextStep.Id;
            item.ManualApprovedBy = ApprovalAccount.Id;
            item.ManualActionTime = DateTime.UtcNow.Ticks;
            item.ManualIsAutoReassigned = false;
            item.ManualEmailNotificationCount = 0;
            item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
            item.ManualEscalateFrom = 0;
            item.ManualEscalatedComment = string.Empty;
            item.ManualLastApproveRejectComment = excelData.ManualApprovalComment;
            item.ManualLastReviewedBy = ApprovalAccount.DisplayName;
            item.ManualLastlReviewTime = DateTime.UtcNow.Ticks;
            var (extendType, extendNumber) = CheckExtendDisposalTime(excelData.ExtendDisposalDate.Trim());
            var customDateTime = DateTime.UtcNow;
            if (extendType == ManualApprovalExtendType.Custom)
            {
                var addDay = int.Parse(excelData.ExtendDisposalDate);
                customDateTime = customDateTime.AddDays(addDay);
            }

            await ManualApprovalAzureTableManager.RebuildAuditsAsync(item, (SOApproveDBStatus)approvalStatus, ApprovalAccount, extendType, extendNumber , customDateTime);
            if (!nextStep.IsEnd)
            {
                item.ManualReviewer = (await nextStep.GetReviewersAsync(item.ScopeId)).Select(item => item.RMUserId).ToArray();
                if(item.ManualNeedEmailNotification)
                {
                    var emailItem = new RMEmailItem
                    {
                        Id = item.Id,
                        Status = RMSendEmailStatus.WaittingSendEmail,
                        ModifyTime = DateTime.UtcNow
                    };
                    await EmailItemDao.AddWorkflowManualItemAsync(emailItem);
                }
                item.ManualLastExtendType = extendType;
                item.ManualLastCustomeExtendDate = customDateTime;
                if (approvalStatus == (int)SOApproveDBStatus.Approved)
                {
                    item.ManualLastExtendType = ManualApprovalExtendType.After1Month;
                }
            }

            if (nextStep.IsEnd)
            {
                item.ManualApprovedStatus = approvalStatus;
                item.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                if (approvalStatus == (int)SOApproveDBStatus.Rejected) 
                {
                    item.ManualLastReasonForRejection = item.QuickReason;
                }
                Logger.Info($"next step is not review step, update next step to final step: {item.ManualWorkflowStepId} for item {item.Id}");
                if (item.SourceFlag == (int)SourceFlag.Physical || item.SourceFlag > (int)SourceFlag.Connector)
                {
                    item.DisposalStatus = approvalStatus;
                }
                if (approvalStatus == (int)SOApproveDBStatus.Rejected)
                {
                    var extendTime = await CalculationExtendTimeAsync(extendType, extendNumber , customDateTime);
                    item.ManualLastReasonForRejection = item.QuickReason;
                    item.ManualExtendTime = extendTime;
                    item.ManualExtendComment = string.Empty;
                    item.ManualExtendCount += 1;
                }
            }

            if (!nextStep.IsEnd || item.SourceFlag > (int)SourceFlag.Connector)
            {
                RecordIdHistoryMapping[item.Id] = historyData;
            }

            ManualApprovalDataSyncManager.Add(item);
        }

        private static (ManualApprovalExtendType, int) CheckExtendDisposalTime(string extendDisposalDate)
        {
            var DefaultExtendDisposalDate = ManualApprovalExtendType.Custom;

            if (string.IsNullOrEmpty(extendDisposalDate))
            {
                return (ManualApprovalExtendType.After1Month, 0);
            }
            if (!int.TryParse(extendDisposalDate, out var number) || number <= 0)
            {
                return (ManualApprovalExtendType.After1Month, 0);
            }
            var ManualSettingInfoJson = RMFunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult();
            var ManualSettingInfoes = SerializerHelper.DeserializeByJsonConvert<ManualApprovalSettings>(ManualSettingInfoJson);
            var maxSelectDayEmnu = ManualSettingInfoes.DisposalExtentionSetting.LatestExtendType;
            var maxSelectDayNumber = ManualSettingInfoes.DisposalExtentionSetting.LatestExtendNumber;
            var addMonth = maxSelectDayEmnu switch
            {
                ManualApprovalExtendType.Month => 1,
                ManualApprovalExtendType.Year => 12,
                _ => 0
            };

            if (DateTime.UtcNow.AddDays(number) >= DateTime.UtcNow.AddMonths(addMonth * maxSelectDayNumber))
            {
                return (maxSelectDayEmnu, maxSelectDayNumber);
            }
            
            return (DefaultExtendDisposalDate,0);
        }
        private static async Task<long> CalculationExtendTimeAsync(ManualApprovalExtendType extendType, int extendTime ,DateTime customeExtendDate)
        {
            var now = DateTime.UtcNow;
            if (extendType == ManualApprovalExtendType.Custom)
            {
                return customeExtendDate.Ticks;
            }
            else if (extendType == ManualApprovalExtendType.After1Month)
            {
                return now.AddMonths(1).Ticks;
            }
            else if (extendType == ManualApprovalExtendType.Month)
            {
                return now.AddMonths(extendTime).Ticks;
            }
            else if (extendType == ManualApprovalExtendType.Year)
            {
                return now.AddYears(extendTime).Ticks;
            }

            return 0;
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
                Logger.Error($"Get owner display names failed,{e}");
                return reviewerNames;
            }
        }
    }

    public class RecordItem
    {
        public string Id { get; set; }

        public string ApprovalStatus { get; set; }

        public string ReviewNames { get; set; }

        public string CollectionTime { get; set; }

        public string ActionTime { get; set; }

        public string ManualApprovalComment { get; set; }

        public string QuickReason { get; set; }

        public string ExtendDisposalDate { get; set; }
    }
}
