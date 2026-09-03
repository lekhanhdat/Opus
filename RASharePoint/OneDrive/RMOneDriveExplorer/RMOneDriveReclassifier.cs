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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
//using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Cache;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.RMReport;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Utils;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.GCommon.Utility;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.SharePoint.RMExplorer.RMReclassifier;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;

namespace AvePoint.RA.SharePoint.OneDrive.RMOneDriveExplorer
{
    public class RMOneDriveReclassifier : RMReclassifierBase
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #region interface
        private ITermDao mTermDao;
        public ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }
        private IOneDriveSettingDao mOneDriveSettingDao = null;
        public IOneDriveSettingDao OneDriveSettingDao
        {
            get
            {
                if (mOneDriveSettingDao == null)
                {
                    mOneDriveSettingDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
                }
                return mOneDriveSettingDao;
            }
        }
        private ITermSetDao mTermSetDao { get; set; }
        public ITermSetDao TermSetDao
        {
            get
            {
                if (mTermSetDao == null)
                {
                    mTermSetDao = (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
                }
                return mTermSetDao;
            }
        }
        private ITermRuleAssociationDao termRuleAssociationDao;
        protected ITermRuleAssociationDao TermRuleInfos
        {
            get
            {
                if (termRuleAssociationDao == null)
                {
                    termRuleAssociationDao = (ITermRuleAssociationDao)PlatformWindsorManager.GetService(typeof(ITermRuleAssociationDao));
                }
                return termRuleAssociationDao;
            }
        }
        private IRuleManagerService mRuleManagerService;
        public IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }
        #endregion

        protected override SourceFlag Flag => SourceFlag.OneDrive;
        private List<RMOneDriveSetting> mAllSettings = new List<RMOneDriveSetting>();
        private Dictionary<Guid, Dictionary<Guid, bool>> mTermAllowToParent = new Dictionary<Guid, Dictionary<Guid, bool>>();
        private Dictionary<Guid, string> mTermPaths = new Dictionary<Guid, string>();
        private Dictionary<Guid, RMOneDriveSetting> mSiteOneDriveSettingMapping = new Dictionary<Guid, RMOneDriveSetting>();
        private Dictionary<Guid, bool> mEnableClassificationCache = new Dictionary<Guid, bool>();
        private string mCurrentSiteUrl = string.Empty;
        private IAveWeb web = null;
        private IAveList list = null;
        private RMOneDriveRetentionDataCache RetentionCache = null;
        private bool mNeedAddLabelHistory = false;
        public Dictionary<Guid, Rule> Rules { get; private set; }
        public Dictionary<Guid, RMRuleItemCollection> TermRuleMapping { get; private set; }

        public RMOneDriveReclassifier(ChangeTermDto dto) : base(dto)
        {
            mAllSettings = OneDriveSettingDao.LoadAllSetting();
            LoadRules();
            AssembleTermRuleMapping();
            RetentionCache = new RMOneDriveRetentionDataCache();
            RetentionCache.CacheTermChange(DateTime.UtcNow.Ticks);
        }
        
        public override async System.Threading.Tasks.Task ChangeTermsAsync(List<Record> records)
        {
            try
            {
                using (new RA.Common.PerformanceScope("RMExplorerUtility.ChangeTermForOneDrive"))
                {
                    logger.Info("Change term action start");
                   
                    var startTime = DateTime.Now;
                    List<Guid> failedIds = new List<Guid>();
                    List<Guid> successIds = new List<Guid>();
                    List<Record> successRecords = new List<Record>();
                    string termName = RMSPReclassifierCache.Instance.Term.Name;
                    Guid termId = RMSPReclassifierCache.Instance.Term.UniqueId;
                    var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                    var avesiteIds = recDic.Keys.ToList();
                    Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
                    if (avesiteIds.Count > 0)
                    {
                        using (new RA.Common.PerformanceScope(string.Format("change.Term.GetSites")))
                        {
                            startTime = DateTime.Now;
                            siteDic = RABrowserClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
                            logger.Warn($"[Change Term] 2. time elapsed for query from DAO {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        }
                        foreach (var kv in recDic)
                        {
                            try
                            {
                                if (siteDic.ContainsKey(kv.Key))
                                {
                                    mCurrentSiteUrl = siteDic[kv.Key].url;
                                    var site = siteDic[kv.Key];
                                    startTime = DateTime.Now;
                                    var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(site);
                                    logger.Warn($"[Declare] 3.time elapsed for GetBPOSInfo {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    startTime = DateTime.Now;
                                    var factory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                                    var spSite = factory.CreateSite();
                                    RetentionCache.CacheSPLabelInfo(spSite);
                                    logger.Warn($"[Declare] 4.1.time elapsed for CreateSite {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    foreach (var record in kv.Value)
                                    {
                                        try
                                        {
                                            if (NeedSkip(record))
                                            {
                                                logger.Info($"skip change term action.");
                                                continue;
                                            }
                                            if (base.IsProcessedFolder(record))
                                            {
                                                logger.Info($"Folder has already been processed. id:{record.Id}");
                                                continue;
                                            }
                                            startTime = DateTime.Now;
                                            if (!IsSameTermScope(record, termId, new Guid(site.parentId)))
                                            {
                                                logger.Debug("This file :{0} is not in the same term scope.", WebUtil.MakeFullUrl(mCurrentSiteUrl, record.DirPath));
                                                throw new Exception("RM_FS_FolderReclassify_FileNotInSameTermScope");
                                            }
                                            if (web == null || (web != null && web.ID != record.WebId))
                                            {
                                                web = spSite.OpenWeb(record.WebId);
                                            }
                                            if (list == null || (list != null && list.ID != record.ListId))
                                            {
                                                list = web.GetList(record.ListId);
                                            }
                                            IAveListItem aveItem = base.GetAveListItem(record, list);
                                           
                                            if (record.NodeType != (int)NodeLevel.Folder)
                                            {
                                                if (CheckisRecord(aveItem))
                                                {
                                                    logger.Debug("This file :{0} is declared.", WebUtil.MakeFullUrl(mCurrentSiteUrl, record.DirPath));
                                                    throw new Exception("RM_SS_ItemBlockEditAndDelete");
                                                }
                                                if (IsEnableClassification(record.ContainerId)) CheckRuleInfo(record, termId, aveItem);
                                                //process label
                                                Guid previousTermId = record.TermId;
                                                record.labelNotExist = UpdateLabel(aveItem, termId, record.Id, previousTermId);
                                            }
                                            var previousId = record.TermId;
                                            record.TermId = termId;
                                            record.TermName = termName;
                                            if(isNewLogicAccount && previousId != termId && IsEnableClassification(record.ContainerId)) record.RemoveManualFields();
                                            _explorerDao.AddOrUpdateRecordWithKeepManual(record, true, isKeepManualColumn: false);
                                            logger.Warn($"[Change Term] 6. time elapsed for checking term scope {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            successIds.Add(record.Id);
                                            successRecords.Add(record);
                                            base.AddProcessedFolderId(record);
                                        }
                                        catch (Exception ee)
                                        {
                                            JobDetailsStatus _status = JobDetailsStatus.Failed;
                                            if (isItemNotFoundError(ee))
                                            {
                                                _status = JobDetailsStatus.Skipped;
                                                UpdateRemoveItem(record);
                                            }
                                            else
                                            {
                                                failedIds.Add(record.Id);
                                            }                                            
                                            AddReclassifyDetailForGlobalSearch(record, _status, ee.Message, true);
                                            logger.Warn("change term action failed {0}", ee.ToString());
                                        }
                                    }
                                    try
                                    {
                                        if (spSite != null)
                                        {
                                            spSite.Dispose();
                                            spSite = null;
                                        }
                                        if (web != null)
                                        {
                                            web.Dispose();
                                            web = null;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error("Error occurred while disposing sp object {0}", e.ToString());
                                    }

                                    foreach (var record in successRecords)
                                    {
                                        if (record.labelNotExist)
                                        {
                                            AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, "RM_SPO_ApplySetting_LabelNotExist", true);
                                            mFailedCount++;
                                        }
                                        else
                                        {
                                            AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Successful, "", true);
                                        }
                                    }
                                    foreach (var tempRecord in successRecords)
                                    {
                                        ClassificationHistoryDao.Create(new RMClassificationHistory()
                                        {
                                            RecordId = tempRecord.Id,
                                            PreviousTermId = tempRecord.TermId,
                                            NewTermId = termId,
                                            OperationTime = DateTime.UtcNow.Ticks
                                        }
                                        );
                                    }
                                    successRecords.Clear();
                                    if (successIds.Count > 0)
                                    {
                                        mSucceedCount += successIds.Count;
                                        startTime = DateTime.Now;
                                        RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_Audit_Action_ChangeTerm", _jobContextDto.Comment);
                                        logger.Warn($"[Change Term] 6. time elapsed for AddReocrdHistory(succeed) to cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    }
                                    successIds.Clear();
                                }
                                else
                                {
                                    throw new Exception("RM_RDM_SCNotFound");
                                }
                            }
                            catch (Exception ee)
                            {
                                failedIds.AddRange(kv.Value.Select(t => t.Id));
                                logger.Warn("change term action failed {0}", ee.ToString());
                                foreach (var record in kv.Value)
                                {
                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, GetRealException(ee), record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                }
                            }
                        }

                        logger.Warn($"[Change Term] 5. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        if (failedIds.Count > 0)
                        {
                            mFailedCount += failedIds.Count;
                            RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("change term error:{0}", ex.ToString());
                throw ex;
            }
            finally
            {
                if (mNeedAddLabelHistory)
                {
                    await RetentionCache.AddLabelHistoryAsync();
                }
                logger.Info("Change term action finish");
            }
        }

        private bool IsEnableClassification(string containerId)
        {
            if (!Guid.TryParse(containerId, out var groupId)) return false;
            if (!mEnableClassificationCache.ContainsKey(groupId))
            {
                var groupSetting = mOneDriveSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
                mEnableClassificationCache[groupId] = groupSetting == null ? true : !groupSetting.IsNullClassificationSetting;
            }
            return mEnableClassificationCache[groupId];
        }

        private bool isItemNotFoundError(Exception e)
        {
            if (e.Message != null && e.Message.Contains("Item does not exist"))
            {
                return true;
            }
            if (e.InnerException != null)
            {
                return isItemNotFoundError(e.InnerException);
            }
            return false;
        }

        private void UpdateRemoveItem(Record removeRecordInDB)
        {
            try
            {
                if (removeRecordInDB != null)
                {
                    logger.Info("Catch item not found error, remove it from explorer.");
                    if (removeRecordInDB.RecordStatus == (int)Contract.Explorer.RMRecordStatus.Active)
                    {
                        _explorerDao.UpdateRecordState(removeRecordInDB, (int)Contract.Explorer.RMRecordStatus.RMDeleted);
                        logger.Info("update record state to 3, siteId: {0}, Unique ID: {1}, itemId: {2}", removeRecordInDB.ScopeId, removeRecordInDB.RecordsId, removeRecordInDB.ItemRowId);
                    }
                    else
                    {
                        logger.Warn("sp object already archived, siteId: {0}, Unique ID: {1}, itemId: {2}", removeRecordInDB.ScopeId, removeRecordInDB.RecordsId, removeRecordInDB.ItemRowId);
                    }
                }
                else
                {
                    logger.Warn("record is null");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        private bool IsSameTermScope(Record record, Guid targetTermId, Guid groupId)
        {
            var fullPath = WebUtil.MakeFullUrl(mCurrentSiteUrl, record.DirPath);
            RMOneDriveSetting bindSetting = mAllSettings.Where(s => s.SiteGroupId == groupId && fullPath.StartsWith(s.FullPath)).OrderBy(s => s.FullPath.Length).FirstOrDefault();
            if (bindSetting == null)
            {
                bindSetting = GetGroupLevelSetting(new Guid(record.AveSiteId));
            }

            if (bindSetting == null)
            {
                return false;
            }

            if (CheckTermValue(bindSetting, targetTermId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckTermValue(RMOneDriveSetting setting, Guid termId)
        {
            bool bindTermSet = setting.TermId == Guid.Empty;
            var parentId = bindTermSet ? setting.TermSetId : setting.TermId;
            return CheckTermValue(bindTermSet, parentId, termId);
        }

        private bool CheckTermValue(bool bindTermSet, Guid parentId, Guid termId)
        {
            string termPath = null;
            if (!mTermPaths.TryGetValue(termId, out termPath))
            {
                termPath = TermDao.GetTermIdPath(termId);
                mTermPaths[termId] = termPath;
            }

            if (string.IsNullOrEmpty(termPath))
            {
                return false;
            }

            Dictionary<Guid, bool> parentNodes = null;
            if (!mTermAllowToParent.TryGetValue(termId, out parentNodes))
            {
                parentNodes = new Dictionary<Guid, bool>();
                mTermAllowToParent[termId] = parentNodes;
            }

            string parentNodePath = null;
            bool isSubTerm = false;
            if (!parentNodes.TryGetValue(parentId, out isSubTerm))
            {
                if (bindTermSet)
                {
                    parentNodePath = (TermSetDao.GetRMTermSetByGuid(parentId)?.Id)?.ToString() + "/";
                }
                else
                {
                    parentNodePath = TermDao.GetTermIdPath(parentId) + "/";
                }
                isSubTerm = termPath.StartsWith(parentNodePath, StringComparison.OrdinalIgnoreCase);
                parentNodes[parentId] = isSubTerm;
            }
            return isSubTerm;
        }

        public bool CheckisRecord(IAveListItem item)
        {
            bool isRecord = false;
            int result = 0;
            try
            {
                object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                result = 0;
            }
            if ((result & 0x1000) != 0 || (result & 0x10) != 0 || (result & 1) != 0 || (result & 0x100) != 0)
            {
                isRecord = true;
            }
            return isRecord;
        }

        private bool UpdateLabel(IAveListItem aveItem, Guid termId, Guid recordId, Guid previousTermId)
        {
            if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
            {
                logger.Info($"Skip folder. Path:[{aveItem.FullPath()}]");
                return false;
            }
            bool labelNotExist = false;
            if (termId != Guid.Empty)
            {
                //term id改变时才操作label
                try
                {
                    TermSettingsInfo termInfo = GetTermInfo(termId);
                    if (termInfo != null)
                    {
                        if ((termInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                        {
                            labelNotExist = ApplyComplianceTag(aveItem, recordId, termInfo, termId, previousTermId);
                        }
                        else
                        {
                            if (previousTermId != termId)
                            {
                                //var previousTermInfo = GetTermInfo(previousTermId);
                                //if (previousTermInfo != null)
                                //{
                                //    if ((previousTermInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                                //    {
                                RemoveComplianceTag(aveItem, recordId);
                                //    }
                                //}
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating retention label. Item url:{0} Error:{1}", aveItem.FullPath(), e.ToString());
                }
            }
            else
            {
                //term id改变时才操作label
                if (previousTermId != Guid.Empty)
                {
                    //var previousTermInfo = GetTermInfo(previousTermId);
                    //if (previousTermInfo != null)
                    //{
                    //    if ((previousTermInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                    //    {
                    RemoveComplianceTag(aveItem, recordId);
                    //    }
                    //}
                }
            }
            return labelNotExist;
        }

        private bool ApplyComplianceTag(IAveListItem item, Guid recordId, TermSettingsInfo termInfo, Guid termId, Guid previousTermId)
        {
            bool labelNotExist = false;
            using (var performance = new PerformanceScope("SP.RMOneDriveExplorerProcessor.applyLabel"))
            {
                var processingLabelName = RetentionCache.LabelStateInfo.CurrentLabel.Name;
                AveComplianceTagInfo tagInfo = null;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName();
                logger.Info($"ApplyComplianceTag:RowId {item.ID} .currentLabel:{currentLabel}. processing lable:{processingLabelName}");
                if (NeedApplyLabel(item, termInfo, recordId, termId, previousTermId))
                {
                    if (RetentionCache.SPSiteRetentionLables.TryGetValue(processingLabelName, out tagInfo))
                    {
                        using (var performance1 = new PerformanceScope("SP.RMOneDriveExplorerProcessor.ApplyComplianceTag"))
                        {
                            //item.SetComplianceTag(tagInfo.TagName, tagInfo.BlockDelete, tagInfo.BlockEdit, tagInfo.IsEventTag, tagInfo.SuperLock);
                            item.SetComplianceTagOnBulkItems(tagInfo.TagName);
                        }
                        logger.Info($"add item label:{processingLabelName}, Item RowId:{item.ID}");
                        mNeedAddLabelHistory = true;
                    }
                    else
                    {
                        logger.Error($"SPLabel cannot be found:{processingLabelName}");
                        labelNotExist = true;
                        //AddFaildLabel(recordId);
                        //throw new Exception($"Label cannot be found, label name:{processingLabelName}");
                    }
                }
                else
                {
                    logger.Info($"skip item:Row Id {item.ID}, compliance tag:{processingLabelName} already exist.");
                }
            }
            return labelNotExist;
        }

        //以下下情况会给数据打Label
        //1.数据在cosmos db中没有记录，并且数据没有Label
        //2.数据在cosmos db中有记录，但是db中的term id和当前term id不一致
        private bool NeedApplyLabel(IAveListItem item, TermSettingsInfo termInfo, Guid recordId, Guid termId, Guid previousTermId)
        {
            bool applyLabel = false;
            var processingLabelName = RetentionCache.LabelStateInfo.CurrentLabel.Name;
            var previousLabelNames = RetentionCache.LabelStateInfo.PreviousLabelNames;
            var currentLabel = item.GetComplianceTagName().ToLower();

            if (previousTermId != termId && (!item.ExistComplianceTag()
               || (previousLabelNames.Count > 0 && previousLabelNames.Contains(currentLabel) && !currentLabel.Equals(processingLabelName, StringComparison.OrdinalIgnoreCase))))
            {
                applyLabel = true;
            }

            return applyLabel;
        }
        private void RemoveComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("SP.RMOneDriveExplorerProcessor.removeLabel"))
            {
                try
                {
                    if (item.ExistComplianceTag())
                    {
                        var previousLabelNames = RetentionCache.LabelStateInfo.PreviousLabelNames;
                        var currentLabel = item.GetComplianceTagName().ToLower();
                        var itemUrl = item.FullPath();
                        var needRemoveLabel = previousLabelNames.Contains(currentLabel);
                        logger.Info($"RemoveComplianceTag:RowId {item.ID}.currentLabel:{currentLabel}.");
                        //only remove tag of retention setting label
                        if (needRemoveLabel)
                        {
                            using (var performance1 = new PerformanceScope("SP.RMOneDriveExplorerProcessor.removeComplianceTag"))
                            {
                                //item.SetComplianceTag(null, false, false, false, false);
                                item.SetComplianceTagOnBulkItems(string.Empty);
                            }
                            logger.Info($"remove item label:{currentLabel}, ItemRowId:{item.ID}");
                        }
                        else
                        {
                            logger.Info($"skip item:RowId {item.ID}, compliance tag:current:{currentLabel}.");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Info($"An error occurred while removing label:RowId {item.ID}.error:{e.ToString()}.");
                }
            }
        }

        private TermSettingsInfo GetTermInfo(Guid termId)
        {
            TermSettingsInfo result = null;

            if (!RetentionCache.TermRetentionMapping.TryGetValue(termId, out result))
            {
                var tempTerm = TermDao.GetParentInhertSetting(termId);
                if (tempTerm != null)
                {
                    result = new TermSettingsInfo() { EnforceRetention = tempTerm.EnforceRetention, OneDriveRetentionLabel = tempTerm.OneDriveRetentionLabel };
                    RetentionCache.AddTermRetentionObj(termId, result);
                }
                else
                {
                    logger.Warn($"item term not exist in db:{termId}");
                    //throw new Exception($"term cannot be found, termId:{termId}");
                }
            }
            return result;
        }
        private void LoadRules()
        {
            logger.Debug("Begin to Load rules to cache.");
            Rules = RuleManagerService.GetRulesFromRecords().ToDictionary(rule => new Guid(rule.Id));
            logger.Debug("End to load Rules to cache");
        }
        private void AssembleTermRuleMapping()
        {
            logger.Debug("Begin to assemble term rules mappings to cache.");
            TermRuleMapping = new Dictionary<Guid, RMRuleItemCollection>();
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();

            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };

                Rule rule;
                var ruleIds = termRules[term.Id];
                int reOrder = 0;
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (Rules.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None)
                        {
                            reOrder++;
                            var ruleOBj = CloneSameRuleObject(rule);
                            commonRules.Rules.Add(reOrder, ruleOBj);
                        }
                    }
                }

                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection
                        {
                            TermId = refTerm.UniqueId,
                            TermName = refTerm.Name
                        };
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }

                    tempRC.CommonRules = commonRules;

                }
            }

            TermRuleMapping = termRuleMappings;
        }

        private Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }
        private RuleCollection RebuldSPRules(RMRuleItemCollection rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
            int reOrder = 0;
            foreach (var order in rules.CommonRules.Rules.Keys)
            {
                if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].OneDriveRule != null && rules.CommonRules.Rules[order].OneDriveRule.SOFilters != null && rules.CommonRules.Rules[order].OneDriveRule.SOFilters.Count > 0)
                {
                    reOrder++;

                    var commonRule = rules.CommonRules.Rules[order];
                    var rule = commonRule.OneDriveRule;
                    rule.Id = commonRule.Id;
                    newRules.Add(order, rule);
                }
            }

            newRuleCol.Rules = newRules;
            return newRuleCol;
        }
        private void AddReclassifyDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment, bool isDocument)
        {
            ArgumentCheck.CheckNotNull(record);
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record?.DirPath == null ? "" : string.IsNullOrWhiteSpace(mCurrentSiteUrl) ? record.DirPath : WebUtil.MakeFullUrl(mCurrentSiteUrl, record.DirPath),
                Action = "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment,
                Type = GetItemTypeI18N(record, isDocument)
            });
        }

        private RMOneDriveSetting GetGroupLevelSetting(Guid siteId)
        {
            if (mSiteOneDriveSettingMapping.ContainsKey(siteId))
            {
                return mSiteOneDriveSettingMapping[siteId];
            }
            else
            {
                var site = RABrowserClient.GetRemoteSiteCollectionById(siteId.ToString());
                if (site != null)
                {
                    var groupId = site.parentId;
                    var groupSetting = mAllSettings.Where(s => s.SiteGroupId == new Guid(groupId) && s.SiteId == Guid.Empty).FirstOrDefault();
                    if (groupSetting != null)
                    {
                        mSiteOneDriveSettingMapping.Add(siteId, groupSetting);
                        return groupSetting;
                    }
                    else
                    {
                        logger.Warn("Cannot find group setting for site, siteid:{0}", siteId);
                        return null;
                    }
                }
                else
                {
                    logger.Warn("Cannot find site, siteid:{0}", siteId);
                    return null;
                }
            }
        }

        private void CheckRuleInfo(Record record, Guid termId, IAveListItem aveItem)
        {
            RMRuleItemCollection rules = null;
            if (TermRuleMapping.TryGetValue(termId, out rules))
            {
                var newRuleCollection = RebuldSPRules(rules);
                if (newRuleCollection.Rules.Count == 0)
                {
                    record.RuleId = Guid.Empty;
                    record.RuleLevel = (int)PolicyLevel.None;
                    record.DisposalDueDate = 0;
                    record.PreviosDisposalDueDate = 0;
                    logger.Info($"No SP rules realted to the item {record?.Id}");
                }
                else
                {
                    var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                    var itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, null);
                    record.RuleId = itemRuleInfo.Rule != null ? new Guid(itemRuleInfo.Rule.Id) : Guid.Empty;
                    record.RuleLevel = itemRuleInfo.Rule != null ? (int)itemRuleInfo.Rule.PolicyLevel : 0;
                    record.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRuleInfo.DisposalAction);
                    record.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRuleInfo.DisposalAction);
                }
            }
            else
            {
                record.RuleId = Guid.Empty;
                record.RuleLevel = (int)PolicyLevel.None;
                record.DisposalDueDate = 0;
                record.PreviosDisposalDueDate = 0;
            }
        }
        public new void Dispose()
        {
            if (_explorerDao != null)
            {
                _explorerDao.Dispose();
            }

            if (RetentionCache != null)
            {
                RetentionCache = null;
            }
        }
    }

}
