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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Google;
using Google.Apis.DriveLabels.v2.Data;
using Newtonsoft.Json;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Models;
using RAGoogle.Services;
using RAGoogle.Util;

namespace RAGoogle.GoogleExplorer
{
    public class GoogleReclassifyBaseProcessor : IDisposable
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(GoogleReclassifyBaseProcessor));
        protected RuleManager RuleManager { get; set; }
        protected RecordManager RecordManager { get; set; }
        protected IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();
        private IRMGoogleSettingDao RmGoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();

        protected List<Record> SucceedItems = new List<Record>();
        protected List<Record> FailedItems;
        protected List<Record> CannotOverwriteLabelRecords = new List<Record>();
        protected Dictionary<string, List<Record>> InvalidRecordCache;
        private List<GoogleAppsDriveLabelsV2Label> _labelGoogleCache = [];

        protected Dictionary<Guid, List<Record>> AllFolderFiles = new Dictionary<Guid, List<Record>>();
        private Dictionary<Guid, bool> IsEnableClassificationContainerCache = new();

        protected AutoJobOption AutoJobOption { get; set; } = AutoJobOption.None;
        private IExplorerDao _explorerDao { get; set; }
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private ITermDao _termDao { get; set; }
        protected ITermDao TermDao
        {
            get
            {
                if (_termDao == null)
                {
                    _termDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return _termDao;
            }
        }

        private IRecordsHistoryService mRecordsHistoryService { get; set; }
        public IRecordsHistoryService RecordsHistoryService
        {
            get
            {
                if (mRecordsHistoryService == null)
                {
                    mRecordsHistoryService = (IRecordsHistoryService)PlatformWindsorManager.GetService(typeof(IRecordsHistoryService));
                }
                return mRecordsHistoryService;
            }
        }

        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        private static IRMClassificationHistoryDao ClassificationHistoryDao => PlatformWindsorManager.GetService<IRMClassificationHistoryDao>();

        private ITenantService mTenantService;
        public ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }

        private IRMGoogleSettingDao mRMGoogleSettingDao;

        public IRMGoogleSettingDao RMGoogleSettingDao
        {
            get
            {
                if (mRMGoogleSettingDao == null)
                {
                    mRMGoogleSettingDao = (IRMGoogleSettingDao)PlatformWindsorManager.GetService(typeof(IRMGoogleSettingDao));
                }
                return mRMGoogleSettingDao;
            }
        }

        protected RMGoogleDiscoverBase RMGoogleDiscoverBase { get; set; }
        protected RMTerm RMTerm { get; set; }
        protected RMGoogleLabelInfo GLabelInfo { get; set; }
        private string? labelGoogleId { get; set; }
        protected ChangeTermOption ChangeTermInfo { get; set; }
        protected ChangeTermDto ChangeLabelDtoInfo { get; set; }
        protected bool isNewLogicAccount;

        protected Dictionary<string, RMGoogleDiscoverBase> TenantIdAppProfileDic { get; set; }
        protected string _jobId;
        public GoogleReclassifyBaseProcessor()
        {
            SucceedItems = new List<Record>();
            FailedItems = new List<Record>();
            InvalidRecordCache = new();
            AllFolderFiles = new Dictionary<Guid, List<Record>>();
            isNewLogicAccount = TenantService.IsNewOpusTenant();
        }

        public int FailedCount { get; protected set; }
        public int SucceedCount { get; protected set; }

        public async Task InitAsync(string labelUniqueId)
        {
            RuleManager = new RuleManager();
            RecordManager = new RecordManager();
            var allGoogleProfiles = RMAosApiClient.GetAllAppProfilesGoogleTenants(TenantLocalValue.LogonGroupId);
            TenantIdAppProfileDic = new Dictionary<string, RMGoogleDiscoverBase>();
            foreach (var profile in allGoogleProfiles)
            {
                var rmGoogleDicoverBase = new RMGoogleDiscoverBase(null);
                rmGoogleDicoverBase.Init(null, profile);
                TenantIdAppProfileDic.Add(profile.TenantId, rmGoogleDicoverBase);
                using GoogleLabelService service = new(profile);
                try
                {
                    _labelGoogleCache.AddRange(await service.ListLabelsPublishedAsync(true));
                }
                catch (Exception ex)
                {
                    logger.Error($"[Change Label] Failed to list labels for TenantId {profile.TenantId}, error: {ex.Message}.");
                    _labelGoogleCache.AddRange([]);
                }
            }
        }

        public void SetJobStatus()
        {
            if (FailedCount == 0)
            {
                ReportManager.SetJobFinished(JobStatus.Finished);
            }
            if (SucceedCount > 0 && FailedCount > 0)
            {
                ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_SS_CommonErrorMessage");
            }
            if (SucceedCount == 0 && FailedCount > 0)
            {
                ReportManager.SetJobFinished(JobStatus.Failed, "RM_SS_CommonErrorMessage");
            }
        }

        protected bool IsContainerEnableClassification(string containerId)
        {
            if (Guid.TryParse(containerId, out Guid containerGuid))
            {
                if (IsEnableClassificationContainerCache.ContainsKey(containerGuid))
                {
                    return IsEnableClassificationContainerCache[containerGuid];
                }
                else
                {
                    var googleSetting = RMGoogleSettingDao.Find(s => s.ContainerId == containerGuid && s.ScopeId == containerGuid);
                    bool isEnable = googleSetting == null || !googleSetting.IsNullClassificationSetting;
                    IsEnableClassificationContainerCache[containerGuid] = isEnable;
                    return isEnable;
                }
            }
            return false;
        }

        public async Task HandleRecords(List<Record> records, Guid targetTermUniqueId, string targetTermName)
        {
            var recordsGroupByScope = records.GroupBy(r => r.ScopeId);
            logger.Info($"[Change Label] Start to apply label on {records.Count} records, group count:{recordsGroupByScope.Count()}.");
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };

            var recordsInOneDrive = new List<Record>();

            foreach (var recordList in recordsGroupByScope)
            {
                try
                {
                    var scopeId = recordList.Key;
                    recordsInOneDrive = recordList.ToList();
                    var treeNode = await RemoteGoogleNodeService.GetRemoteNodeByDriveIdAsync(scopeId.ToString());
                    var tenantId = treeNode.GoogleTenantId;
                    await CheckAndAssignLabelInfo(targetTermUniqueId.ToString(), tenantId);

                    if (!TenantIdAppProfileDic.TryGetValue(tenantId, out var googleDiscoverBase))
                    {
                        FailedCount++;
                        logger.Warn($"[Change Label] Failed to find Google Discoverbase for TenantId {tenantId}. Skipping records in this scope.");
                        foreach (var record in recordsInOneDrive)
                        {
                            await JobReportFailedAction(record, new KeyNotFoundException($"AppInfo not found for TenantId: {tenantId}."));
                        }
                        continue;
                    }


                    if (treeNode == null)
                    {
                        FailedCount++;
                        logger.Warn($"[Change Label] Failed to retrieve tree node for scope ID {scopeId}. Skipping records in this scope.");
                        var exception = new NullReferenceException($"Tree node for scope ID {scopeId} is null.");
                        foreach (var record in recordsInOneDrive)
                        {
                            await JobReportFailedAction(record, exception);
                        }
                        continue;
                    }
                    logger.Info($"[Change Label] Start to handle drive, id {scopeId}, count:{recordsInOneDrive.Count()}.");
                    var driveId = treeNode.Level == (int)NodeLevel.GoogleSharedDrive ? treeNode.ObjectId : treeNode.DisplayName;
                    using (var googleDrive = await googleDiscoverBase.GetDriveService(driveId))
                    {
                        await Parallel.ForEachAsync(recordsInOneDrive, parallelOptions, async (record, _) =>
                        {
                            try
                            {
                                logger.Info($"[Change Label] Start to handle record, id {record.Id}.");
                                if (record.NodeType != (int)RMNodeLevel.GoogleFile) return;

                                var itemInfo = JsonConvert.DeserializeObject<GoogleItemMetaInfo>(record.MetaInfo);

                                var tenantId = itemInfo!.TenantId;
                                var fileStatus = await googleDrive.GetFileStatusAsync(itemInfo.DocId);
                                if (fileStatus.IsTrashed)
                                {
                                    FailedCount++;
                                    logger.Warn($"[Change Label] File {record.Id} is trashed. Skipping label application.");
                                    throw new TrashException($"File {record.Id} is trashed.");
                                }

                                List<string> labelCurrents = record.TermId == Guid.Empty ? new() : [(await TermDao.GetGoogleTermInfoByUniqueId(record.TermId.ToString(), tenantId)).LabelId];

                                await RetryExecuteApplyLabelAsync(googleDrive, targetTermUniqueId, targetTermName, itemInfo, record, labelCurrents);
                            }
                            catch (UnexpectedErrorException uex)
                            {
                                logger.Warn($"[Change Label] An unexpected error occurred with record : {record.Id}, error: {uex.Message}");
                                FailedCount++;
                                await JobReportFailedAction(record, uex);
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"[Change Label] Failed to handle record {record.Id}, error: {e.Message}");
                                FailedCount++;
                                await JobReportFailedAction(record, e);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    foreach (var record in recordsInOneDrive)
                    {
                        FailedCount++;
                        await JobReportFailedAction(record, ex);
                    }

                    logger.Warn($"[Change Label] Skipped applying label on scope {recordList.Key} for record  because {ex.Message} is null.");
                }
            }
        }

        public async Task<List<Record>> HandleRecordsWithResult(List<Record> records, Guid targetTermUniqueId, string targetTermName)
        {
            List<Record> successRecords = new List<Record>();
            var recordsGroupByScope = records.GroupBy(r => r.ScopeId);
            logger.Info($"[Change Label] Start to apply label on {records.Count} records, group count:{recordsGroupByScope.Count()}.");
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };

            var recordsInGoogle = new List<Record>();

            foreach (var recordList in recordsGroupByScope)
            {
                try
                {
                    var scopeId = recordList.Key;
                    recordsInGoogle = recordList.ToList();
                    var treeNode = await RemoteGoogleNodeService.GetRemoteNodeByDriveIdAsync(scopeId.ToString());
                    var tenantId = treeNode.GoogleTenantId;
                    await CheckAndAssignLabelInfo(targetTermUniqueId.ToString(), tenantId);

                    if (!TenantIdAppProfileDic.TryGetValue(tenantId, out var googleDiscoverBase))
                    {
                        FailedCount++;
                        logger.Warn($"[Change Label] Failed to find Google Discoverbase for TenantId {tenantId}. Skipping records in this scope.");
                        foreach (var record in recordsInGoogle)
                        {
                            await JobReportFailedAction(record, new KeyNotFoundException($"AppInfo not found for TenantId: {tenantId}."));
                        }
                        continue;
                    }


                    if (treeNode == null)
                    {
                        FailedCount++;
                        logger.Warn($"[Change Label] Failed to retrieve tree node for scope ID {scopeId}. Skipping records in this scope.");
                        var exception = new NullReferenceException($"Tree node for scope ID {scopeId} is null.");
                        foreach (var record in recordsInGoogle)
                        {
                            await JobReportFailedAction(record, exception);
                        }
                        continue;
                    }
                    logger.Info($"[Change Label] Start to handle drive, id {scopeId}, count:{recordsInGoogle.Count()}.");
                    var driveId = treeNode.Level == (int)NodeLevel.GoogleSharedDrive ? treeNode.ObjectId : treeNode.DisplayName;
                    using (var googleDrive = await googleDiscoverBase.GetDriveService(driveId))
                    {
                        await Parallel.ForEachAsync(recordsInGoogle, parallelOptions, async (record, _) =>
                        {
                            try
                            {
                                logger.Info($"[Change Label] Start to handle record, id {record.Id}.");
                                if (record.NodeType != (int)RMNodeLevel.GoogleFile) return;

                                var itemInfo = JsonConvert.DeserializeObject<GoogleItemMetaInfo>(record.MetaInfo);

                                var tenantId = itemInfo!.TenantId;
                                var fileStatus = await googleDrive.GetFileStatusAsync(itemInfo.DocId);
                                if (fileStatus.IsTrashed)
                                {
                                    FailedCount++;
                                    logger.Warn($"[Change Label] File {record.Id} is trashed. Skipping label application.");
                                    await JobReportFailedAction(record, new TrashException($"File {record.Id} is trashed."));
                                    return;
                                }

                                List<string> labelCurrents = new();
                                if (itemInfo?.Labels != null && itemInfo.Labels.Count > 0) 
                                {
                                    foreach(var label in itemInfo.Labels)
                                    {
                                        labelCurrents.Add(label.Id);
                                    }
                                }
                                var googleSetting = RmGoogleSettingDao.GetSettingInfoByScope(new Guid(record.ContainerId), record.ScopeId, record.ScopeId);
                                if (googleSetting == null)
                                {
                                    logger.Info($"[Change Label] Try got container setting.");
                                    googleSetting = RmGoogleSettingDao.GetSettingInfoByScope(new Guid(record.ContainerId), new Guid(record.ContainerId), Guid.Empty);
                                }

                                await RetryExecuteApplyLabelAsync(googleDrive, targetTermUniqueId, targetTermName, itemInfo, record, labelCurrents, googleSetting);
                                successRecords.Add(record);
                            }
                            catch (UnexpectedErrorException uex)
                            {
                                logger.Warn($"[Change Label] An unexpected error occurred with record : {record.Id}, error: {uex.Message}");
                                FailedCount++;
                                await JobReportFailedAction(record, uex);
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"[Change Label] Failed to handle record {record.Id}, error: {e.Message}");
                                FailedCount++;
                                await JobReportFailedAction(record, e);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    foreach (var record in recordsInGoogle)
                    {
                        FailedCount++;
                        await JobReportFailedAction(record, ex);
                    }

                    logger.Warn($"[Change Label] Skipped applying label on scope {recordList.Key} for record  because {ex.Message} is null.");
                }
            }
            return successRecords;

        }
        public virtual async Task JobReportSuccessfulAction(Record record, Guid sourceTermId)
        {
            await Task.CompletedTask;
        }
        public virtual async Task JobReportFailedAction(Record record, Exception ex)
        {
            await Task.CompletedTask;
        }
        public virtual async Task JobReportSkipAction(Record record, Exception ex)
        {
            await Task.CompletedTask;
        }

        public void AddProcessReclassifyItemsToHistory(ChangeTermDto dto)
        {
            try
            {
                var overwriteSuccessful = SucceedItems.Except(CannotOverwriteLabelRecords).ToList();
                var partialSuccessful = CannotOverwriteLabelRecords.ToList();

                if (FailedCount == 0)
                {
                    if (overwriteSuccessful.Any())
                    {
                        RecordsHistoryService.AddRecordsHistory(overwriteSuccessful.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_ChangeLabel", dto.Comment);
                    }
                    if (partialSuccessful.Any())
                    {
                        RecordsHistoryService.AddRecordsHistory(partialSuccessful.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_SuccessNoOverwrite", dto.Comment);
                    }
                }
                if (FailedCount > 0 && SucceedCount > 0)
                {
                    if (overwriteSuccessful.Any())
                    {
                        RecordsHistoryService.AddRecordsHistory(overwriteSuccessful.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_ChangeLabel", dto.Comment);
                    }
                    if (partialSuccessful.Any())
                    {
                        RecordsHistoryService.AddRecordsHistory(partialSuccessful.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_SuccessNoOverwrite", dto.Comment);
                    }
                    RecordsHistoryService.AddRecordsHistory(FailedItems.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeLabelErrorMessage");
                }
                if (SucceedCount == 0 && FailedCount > 0)
                {
                    RecordsHistoryService.AddRecordsHistory(FailedItems.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeLabelAllErrorMessage");
                }
                logger.Info($"Succeed add process reclassify files to history.");

                if (_jobId.Contains("GSA") && AllFolderFiles.Keys.ToList().Any())
                {
                    foreach (var folder in AllFolderFiles)
                    {
                        var folderId = folder.Key;
                        var filesInFolder = folder.Value;

                        if (!filesInFolder.Any())
                        {
                            RecordsHistoryService.AddRecordsHistory(new List<Guid> { folderId }, "RM_JS_Audit_ChangeLabelNoFileErrorMessage");
                            logger.Warn($"Folder [{folderId}] reclassify action failed because it has no files.");
                            continue;
                        }

                        var succeedFiles = filesInFolder.Where(file => SucceedItems.Contains(file)).ToList();
                        var failedFiles = filesInFolder.Where(file => FailedItems.Contains(file)).ToList();

                        if (succeedFiles.Count == filesInFolder.Count)
                        {
                            RecordsHistoryService.AddRecordsHistory(new List<Guid> { folderId }, "RM_BCM_Audit_Action_ChangeLabel", dto.Comment);
                            logger.Info($"Folder [{folderId}] reclassify action succeeded.");
                        }
                        else if (failedFiles.Count == filesInFolder.Count)
                        {
                            RecordsHistoryService.AddRecordsHistory(new List<Guid> { folderId }, "RM_JS_Audit_ChangeLabelAllErrorMessage");
                            logger.Warn($"Folder [{folderId}] reclassify action failed.");
                        }
                        else
                        {
                            RecordsHistoryService.AddRecordsHistory(new List<Guid> { folderId }, "RM_JS_Audit_ChangeLabelFolderWithException", dto.Comment);
                            logger.Warn($"Folder [{folderId}] reclassify action has mixed results (success and failure).");
                        }
                    }

                    logger.Info($"Succeed add process reclassify folder to history.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while add process reclassify items to history. Error: {e}");
            }
        }

        private async Task RetryExecuteApplyLabelAsync(GoogleDriveService googleDrive, Guid targetLabelUniqueId, string targetTermName, GoogleItemMetaInfo itemInfo, Record record, List<string>? currentLabels = null, RMGoogleSetting? googleSetting = null)
        {
            int retryCount = 0;
            do
            {
                retryCount++;
                try
                {
                    if (googleSetting != null && googleSetting.DeployLabelMethod != (int)DeployLabelMethod.UseManualClassification)
                    {
                        if(currentLabels != null && currentLabels!.Count > 0 && googleSetting.AutoJobOption == (int)AutoJobOption.SkipAndKeep)
                        {
                            AutoJobOption = AutoJobOption.SkipAndKeep;
                            logger.Info("Skip and Keep existing label on file. FileName:{0}", record.LeafName);
                            break;
                        }
                        if (currentLabels != null && currentLabels!.Count > 0 && googleSetting.AutoJobOption == (int)AutoJobOption.Override)
                        {
                            await googleDrive.BatchRemoveLabelsOnFileAsync(currentLabels, itemInfo.DocId);
                            logger.Info($"The Label on File {itemInfo.DocId} has been replace");
                            currentLabels.Clear();
                        }
                    }
                    else if (googleSetting == null)
                    {
                        if (currentLabels != null && currentLabels!.Count > 0)
                        {
                            await googleDrive.BatchRemoveLabelsOnFileAsync(currentLabels, itemInfo.DocId);
                            logger.Info($"The Label on File {itemInfo.DocId} has been replace");
                            currentLabels.Clear();
                        }
                    }
                    await googleDrive.AppliedLabelOnFileAsync(labelGoogleId, itemInfo.DocId);

                    var sourceTermId = record.TermId;
                    record.TermId = targetLabelUniqueId;
                    record.TermName = targetTermName;
                    if(IsContainerEnableClassification(record.ContainerId))
                    {
                        if (isNewLogicAccount && sourceTermId != targetLabelUniqueId)
                        {
                            record.RemoveManualProperties();
                        }
                        await RuleManager.ApplyRuleInfo(record);
                    }
                    RecordManager.UpdateManualProperties(record, true);
                    ExplorerDao.Upsert(record);
                    logger.Info($"[Change Label] Successfully updated Cosmos DB for record {record.Id}.");
                    await JobReportSuccessfulAction(record, sourceTermId);
                    SucceedCount++;
                    logger.Info($"[Change Label] Successful to handle record, id {record.Id}.");
                    break;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains(I18NResource.LabelInvalidOverwritePermissionException))
                    {
                        logger.Warn($"[Change Label] Failed to overwrite labels from file {record.Id}. Labels to remove: {string.Join(", ", currentLabels ?? [""])}");
                        CannotOverwriteLabelRecords.Add(record);
                        break;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            while (retryCount <= 3);
        }


        private async Task CheckAndAssignLabelInfo(string termUniqueId, string tenantId)
        {
            RMTerm = TermDao.GetRMTermByUniqueId(new Guid(termUniqueId), true);
            if (RMTerm == null)
            {
                FailedCount++;
                logger.Error($"[Change Label] The opus term [{termUniqueId}] is not exist.");
                throw new TermNotFoundException($"RM_JM_JD_LabelInvalidError");
            }
            var gLabelInfoInDB = await TermDao.GetGoogleTermInfoByUniqueId(termUniqueId, tenantId);
            if (!string.IsNullOrEmpty(gLabelInfoInDB?.LabelId))
            {
                labelGoogleId = _labelGoogleCache.FirstOrDefault(x => x.Id == gLabelInfoInDB.LabelId)?.Id;
            }

            if (string.IsNullOrEmpty(labelGoogleId))
            {
                logger.Error($"[Change Label] The google term [{termUniqueId}] is not exist.");
                throw new TermNotFoundException($"RM_JM_JD_LabelInvalidError");
            }
        }

        public void Dispose()
        {

        }

        public void AddSucceedDetail(Record record, Guid previousTermId)
        {
            try
            {
                ClassificationHistoryDao.Create(new RMClassificationHistory
                {
                    RecordId = record.Id,
                    PreviousTermId = previousTermId,
                    NewTermId = record.TermId,
                    OperationTime = DateTime.UtcNow.Ticks
                });

                ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                {
                    ObjectName = record.LeafName,
                    FullPath = GetFullPath(record),
                    Action = "RM_JS_BCM_Explorer_ChangeLabel",
                    Status = JobDetailsStatus.Successful,
                    Comment = string.Empty,
                    Type = "RM_JS_Rule_ObjectLevel_Document"
                });

                logger.Info($"Successfully added item [{record.Id}] reclassify action to history.");
            }
            catch (Exception e)
            {
                logger.Error($"Error adding succeed detail for item [{record.Id}]. Error: {e}");
            }
        }

        public void AddFailedDetail(Record record, Exception ex)
        {
            try
            {
                string message = string.Empty;
                if (ex.Message.Contains("LabelNoPermission"))
                {
                    message = "RM_JM_JD_LabelNoPermission";
                }
                else if (ex is NullReferenceException)
                {
                    message = "RM_JS_DAM_RetrieveTreeNodeFailed";
                }
                else if (ex is TrashException)
                {
                    message = "RM_JS_JM_FileIsInTrash";
                }
                else if (ex is LockException)
                {
                    message = "RM_JS_JM_ItemIsLocked";
                }
                else if (ex is UnexpectedErrorException)
                {
                    message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
                }
                else if (ex is LimitTermApplyException)
                {
                    message = "RM_JM_JD_LabelLimitApplied";
                }
                else if (ex is TermNotFoundException)
                {
                    message = "RM_JM_JD_LabelInvalidError";
                }
                else if (ex is GoogleApiException gex)
                {
                    if (gex?.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        message = "RM_JM_JD_ItemNotFoundError";
                    }
                    if (gex?.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
                    }
                }

                var action = _jobId switch
                {
                    var id when id.StartsWith("MAAP") => "RM_MA_Approve",
                    var id when id.StartsWith("MARE") => "RM_JS_BCM_Explorer_ChangeLabel",
                    var id when id.StartsWith("GSA") => "RM_JS_BCM_Explorer_ChangeLabel",
                    _ => "RM_JS_Common_None"
                };

                ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                {
                    ObjectName = record.LeafName,
                    FullPath = GetFullPath(record),
                    Action = action,
                    Status = JobDetailsStatus.Failed,
                    Comment = message,
                    Type = "RM_JS_Rule_ObjectLevel_Document"
                });
                logger.Info($"Failed detail added for item [{record.Id}].");
            }
            catch (Exception e)
            {
                logger.Error($"Error adding failed detail for item [{record.Id}]. Error: {e}");
            }
        }

        public void AddSkipDetail(Record record, Exception ex)
        {
            try
            {
                string message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
                if (ex is NullReferenceException)
                {
                    message = "RM_JS_DAM_RetrieveTreeNodeFailed";
                }

                ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                {
                    ObjectName = record.LeafName,
                    FullPath = GetFullPath(record),
                    Action = "RM_JS_BCM_Explorer_ChangeLabel",
                    Status = JobDetailsStatus.Skipped,
                    Comment = message,
                    Type = "RM_JS_Rule_ObjectLevel_Document"
                });
                logger.Info($"Failed detail added for item [{record.Id}].");
            }
            catch (Exception e)
            {
                logger.Error($"Error adding failed detail for item [{record.Id}]. Error: {e}");
            }
        }

        public string GetFullPath(Record record)
        {
            string FullPath = SecurityUtils.SafeCombinePath(record.DirPath);
            return FullPath.Replace('/', '\\');
        }

        public void CacheInvalidRecord(Exception ex, Record record)
        {
            string message = string.Empty;
            if (ex.Message.Contains("LabelNoPermission"))
            {
                message = "RM_JM_JD_LabelNoPermission";
            }
            else if (ex is NullReferenceException)
            {
                message = "RM_JS_DAM_RetrieveTreeNodeFailed";
            }
            else if (ex is TrashException)
            {
                message = "RM_JS_JM_FileIsInTrash";
            }
            else if (ex is LockException)
            {
                message = "RM_JS_JM_ItemIsLocked";
            }
            else if (ex is LimitTermApplyException)
            {
                message = "RM_JM_JD_LabelLimitApplied";
            }
            else if (ex is UnexpectedErrorException)
            {
                message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
            }
            else if (ex is TermNotFoundException)
            {
                message = "RM_JM_JD_LabelInvalidError";
            }
            else if (ex is GoogleApiException gex)
            {
                if (gex?.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    message = "RM_JM_JD_ItemNotFoundError";
                }
                if (gex?.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    message = "RM_RDM_Explorer_ChangeLabel_All_Failed";
                }
            }
            if (InvalidRecordCache.ContainsKey(message))
            {
                InvalidRecordCache[message].Add(record);
            }
            else
            {
                InvalidRecordCache.Add(message, [record]);
            }
        }
    }

    [Serializable]
    public class TrashException : Exception
    {
        public TrashException(string message) : base(message) { }
    }

    [Serializable]
    public class LockException : Exception
    {
        public LockException(string message) : base(message) { }
    }

    [Serializable]
    public class UnexpectedErrorException : Exception
    {
        public UnexpectedErrorException() : base("An unexpected error occurred.") { }

        public UnexpectedErrorException(string message) : base(message) { }

        public UnexpectedErrorException(string message, Exception innerException) : base(message, innerException) { }
    }

    [Serializable]
    public class LimitTermApplyException : Exception
    {
        public LimitTermApplyException(string message) : base(message) { }
    }

    [Serializable]
    public class TermNotFoundException : Exception
    {
        public TermNotFoundException(string message) : base(message) { }
    }
}
