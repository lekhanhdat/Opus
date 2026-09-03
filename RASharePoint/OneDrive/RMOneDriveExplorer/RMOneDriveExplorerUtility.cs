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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Cache;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Utils;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace AvePoint.RA.SharePoint.OneDrive.RMOneDriveExplorer
{
    public class RMOneDriveExplorerUtility : IDisposable
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMOneDriveExplorerUtility));
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
        private IRMRecordsUpdateTempDao mRMRecordsUpdateTempDao;
        public IRMRecordsUpdateTempDao RMRecordsUpdateTempDao
        {
            get
            {
                if (mRMRecordsUpdateTempDao == null)
                {
                    mRMRecordsUpdateTempDao = (IRMRecordsUpdateTempDao)PlatformWindsorManager.GetService(typeof(IRMRecordsUpdateTempDao)); ;
                }
                return mRMRecordsUpdateTempDao;
            }
        }
        private IRMScopeRoleAssignmentDao mRMScopeRoleAssignmentDao = null;
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao
        {
            get
            {
                if (mRMScopeRoleAssignmentDao == null)
                {
                    mRMScopeRoleAssignmentDao = (IRMScopeRoleAssignmentDao)PlatformWindsorManager.GetService(typeof(IRMScopeRoleAssignmentDao));
                }
                return mRMScopeRoleAssignmentDao;
            }
        }
        private IUserService mUserService = null;
        public IUserService UserService
        {
            get
            {
                if (mUserService == null)
                {
                    mUserService = (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
                }
                return mUserService;
            }
        }
        private IAccountDao mAccountDao = null;
        public IAccountDao AccountDao
        {
            get
            {
                if (mAccountDao == null)
                {
                    mAccountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return mAccountDao;
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
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IRMClassificationHistoryDao mClassificationHistoryDao;
        protected IRMClassificationHistoryDao ClassificationHistoryDao
        {
            get
            {
                if (mClassificationHistoryDao == null)
                {
                    mClassificationHistoryDao = (IRMClassificationHistoryDao)PlatformWindsorManager.GetService(typeof(IRMClassificationHistoryDao));
                }
                return mClassificationHistoryDao;
            }

        }

        private IRecordsHistoryService mRecordsHistoryService = null;
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

        private IRMTermUsageDao mRMTermUsageDao;
        protected IRMTermUsageDao RMTermUsageDao
        {
            get
            {
                if (mRMTermUsageDao == null)
                {
                    mRMTermUsageDao = (IRMTermUsageDao)PlatformWindsorManager.GetService(typeof(IRMTermUsageDao));
                }
                return mRMTermUsageDao;
            }
        }
        private IRMMLTermDao mlTermDao = null;
        public IRMMLTermDao TrainingTermDao
        {
            get
            {
                if (mlTermDao == null)
                {
                    mlTermDao = (IRMMLTermDao)PlatformWindsorManager.GetService(typeof(IRMMLTermDao));
                }
                return mlTermDao;
            }
        }

        private IRMEXOLabelDao _labelDao;
        public IRMEXOLabelDao LabelDao
        {
            get { return _labelDao ?? (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao)); }
            set { _labelDao = value; }
        }

        private IRMEXOLabelDao _RetentionLabelDao;
        public IRMEXOLabelDao RetentionLabelDao
        {
            get { return _RetentionLabelDao ?? (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao)); }
            set { _RetentionLabelDao = value; }
        }

        private IRMRemoteNodeDao mRemoteNodeDao = null;
        public IRMRemoteNodeDao RemoteNodeDao
        {
            get
            {
                mRemoteNodeDao ??= (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                return mRemoteNodeDao;
            }
        }

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
        #endregion
        private bool mIsGlobalSearch = false;
        private ChangeTermType mChangeTermType = ChangeTermType.None;
        private List<RMOneDriveSetting> mAllSettings = new List<RMOneDriveSetting>();
        private Dictionary<Guid, Dictionary<Guid, bool>> mTermAllowToParent = new Dictionary<Guid, Dictionary<Guid, bool>>();
        private Dictionary<Guid, string> mTermPaths = new Dictionary<Guid, string>();
        private Dictionary<Guid, RMOneDriveSetting> mSiteOneDriveSettingMapping = new Dictionary<Guid, RMOneDriveSetting>();
        private string mCurrentSiteUrl = string.Empty;
        private IAveWeb web = null;
        private IAveList list = null;
        private ConcurrentDictionary<Guid, long> mTermChangedDic { get; set; }
        private RMOneDriveRetentionDataCache RetentionCache = null;
        private bool mNeedAddLabelHistory = false;
        public Dictionary<Guid, Rule> Rules { get; private set; }
        public Dictionary<Guid, RMRuleItemCollection> TermRuleMapping { get; private set; }

        public int FailedCount = 0;
        private Dictionary<Guid, string> cacheAllTermsDic;


        public RMOneDriveExplorerUtility(bool isGlobalSearch, ChangeTermType changeTermType = ChangeTermType.SearchChangeTerm)
        {
            mTermChangedDic = new ConcurrentDictionary<Guid, long>();
            mIsGlobalSearch = isGlobalSearch;
            mChangeTermType = changeTermType;
            mAllSettings = OneDriveSettingDao.LoadAllSetting();
            LoadRules();
            AssembleTermRuleMapping();
            RetentionCache = new RMOneDriveRetentionDataCache();
            RetentionCache.CacheTermChange(DateTime.UtcNow.Ticks);
        }

        public async System.Threading.Tasks.Task ChangeAllTermsForOneDriveAsync(ChangeTermOption changeTermInfo, string tempJobId)
        {
            try
            {
                using (new RA.Common.PerformanceScope("RMExplorerUtility.ChangeTermForOneDrive"))
                {
                    var isNewLogicAccount = TenantService.IsNewOpusTenant();
                    logger.Info("Is new logic account is {0}", isNewLogicAccount);
                    logger.Info("Change term action start {0}", tempJobId);
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, "");
                    mRMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, false);
                    List<Record> records = new List<Record>();
                    if (changeTermInfo.SourceOneDriveRecordIds != null && changeTermInfo.SourceOneDriveRecordIds.Count > 0)
                    {
                        var startTime = DateTime.Now;
                        using (new RA.Common.PerformanceScope(string.Format("change.Term.GetRecords")))
                        {
                            records = ExplorerDao.QueryAll(r => changeTermInfo.SourceOneDriveRecordIds.Contains(r.Id)).ToList();
                            logger.Warn($"[Change Term] 1. time elapsed for query {records.Count} records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                            List<Guid> allGuids = new List<Guid>();
                            allGuids.AddRange(changeTermInfo.SourceRecordIds);
                            allGuids.AddRange(changeTermInfo.SourceOneDriveRecordIds);
                            var recordsNoti = ExplorerDao.QueryAll(r => allGuids.Contains(r.Id)).ToList();
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(recordsNoti.Select(r => r.LeafName).ToList()));
                        }

                        List<Guid> failedIds = new List<Guid>();
                        List<Guid> successIds = new List<Guid>();
                        List<Record> successRecords = new List<Record>();

                        var trainingTerm = TrainingTermDao.GetTrainingTerm(changeTermInfo.TargetTermUniqueId);
                        if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                        {
                            var termsIds = records.Select(t => t.PredictTermId).ToList();
                            cacheAllTermsDic = (await TermDao.FindListAsync(tm => termsIds.Contains(tm.UniqueId))).ToDictionary(t => t.UniqueId, t => t.Name);
                        }
                        string termName = changeTermInfo.TargetTermName;
                        Guid termId = changeTermInfo.TargetTermUniqueId;
                        var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                        var avesiteIds = recDic.Keys.ToList();
                        Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();

                        if (mChangeTermType == ChangeTermType.AIMAChangeTerm && changeTermInfo.TargetTermId == -1) //No Term
                        {
                            foreach (var rec in records)
                            {
                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                rec.MLClassificationType = (int)RMMLClassificationType.Rejected;
                            }
                            var faileds = ExplorerDao.BatchUpdate(records, 5);
                            if (mIsGlobalSearch)
                            {
                                foreach (var rec in records)
                                {
                                    AddReclassifyDetailForGlobalSearch(rec, faileds.Contains(rec.Id) ? JobDetailsStatus.Failed : JobDetailsStatus.Successful, "", true);
                                }
                            }
                        }
                        else
                        {
                            if (avesiteIds.Count > 0)
                            {
                                using (new RA.Common.PerformanceScope(string.Format("change.Term.GetSites")))
                                {
                                    startTime = DateTime.Now;
                                    //siteDic = mDocAveClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
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
                                            web = null;
                                            list = null;
                                            RetentionCache.CacheSPLabelInfo(spSite);
                                            logger.Warn($"[Declare] 4.1.time elapsed for CreateSite {(DateTime.Now - startTime).TotalMilliseconds} ms");

                                            foreach (var record in kv.Value)
                                            {
                                                try
                                                {
                                                    Guid previousTermId = record.TermId;

                                                    if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                                                    {
                                                        termId = record.PredictTermId;
                                                        if (cacheAllTermsDic.ContainsKey(termId))
                                                        {
                                                            termName = cacheAllTermsDic[termId];
                                                        }
                                                        else
                                                        {
                                                            logger.Warn($"Can not found this term:{termId}");
                                                        }
                                                    }

                                                    startTime = DateTime.Now;
                                                    if (!IsSameTermScope(record, termId, new Guid(site.parentId)))
                                                    {
                                                        logger.Debug("This file :{0} is not in the same term scope.", WebUtil.MakeFullUrl(mCurrentSiteUrl, record.DirPath));
                                                        throw new Exception("RM_FS_FolderReclassify_FileNotInSameTermScope");
                                                    }
                                                    IAveListItem aveItem;
                                                    if (IsDeclared(record, spSite, out aveItem))
                                                    {
                                                        logger.Debug("This file :{0} is declared.", WebUtil.MakeFullUrl(mCurrentSiteUrl, record.DirPath));
                                                        throw new Exception("RM_SS_ItemBlockEditAndDelete");
                                                    }
                                                    
                                                    if (IsContainerEnableClassification(record.ContainerId))
                                                    {
                                                        if (isNewLogicAccount && previousTermId != termId)
                                                        {
                                                            record.RemoveManualFields();
                                                        }

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

                                                    record.TermId = termId;
                                                    record.TermName = termName;

                                                    if (mChangeTermType == ChangeTermType.AIMAChangeTerm)
                                                    {
                                                        record.MLApprovalStatus = GetMLApprovalStatus();
                                                        record.MLClassificationType = GetMLClassificationType();

                                                        if (trainingTerm != null && MLTermStatusHelper.ActiveTermStatus.Contains(trainingTerm.Status))
                                                        {
                                                            record.TrainingAddType = GetTrainingAddType();
                                                            record.TrainingScope = (int)MLFileStatus.NotTrain;
                                                            record.TrainingTermId = termId;
                                                        }
                                                    }
                                                    if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                                                    {
                                                        record.MLApprovalStatus = GetMLApprovalStatus();
                                                        record.MLClassificationType = GetMLClassificationType();
                                                    }

                                                    //process label
                                                    record.labelNotExist = UpdateLabel(aveItem, termId, record.Id, previousTermId);

                                                    ExplorerDao.BatchUpdate(new List<Record>() { record }, 1);
                                                    //add term usage for dashboard
                                                    //ModifiedTermUsages(previousTermId, record.TermId, record.Id);
                                                    logger.Warn($"[Change Term] 6. time elapsed for checking term scope {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    successIds.Add(record.Id);
                                                    successRecords.Add(record);
                                                }
                                                catch (Exception ee)
                                                {
                                                    JobDetailsStatus _status = JobDetailsStatus.Failed;
                                                    if (isItemNotFoundError(ee))
                                                    {
                                                        _status = JobDetailsStatus.Skipped;
                                                        this.UpdateRemoveItem(record);
                                                    }
                                                    else
                                                    {
                                                        failedIds.Add(record.Id);
                                                    }

                                                    if (mIsGlobalSearch)
                                                    {
                                                        AddReclassifyDetailForGlobalSearch(record, _status, ee.Message, true);
                                                    }
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

                                            //if (successIds.Count > 0)
                                            //{
                                            //    ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                            //    {
                                            //        rec.TermId = termId;
                                            //        rec.TermName = termName;
                                            //        rec.RuleId = Guid.Empty;
                                            //        rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                            //        rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                            //        rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
                                            //    });
                                            //}

                                            if (mIsGlobalSearch)
                                            {
                                                foreach (var record in successRecords)
                                                {
                                                    if (record.labelNotExist)
                                                    {
                                                        AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, "RM_SPO_ApplySetting_LabelNotExist", true);
                                                        FailedCount++;
                                                    }
                                                    else
                                                    {
                                                        AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Successful, "", true);
                                                    }
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
                                                startTime = DateTime.Now;
                                                string actionString = GetActionString();
                                                RecordsHistoryService.AddRecordsHistory(successIds, actionString, changeTermInfo.Comment);
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
                                        if (mIsGlobalSearch)
                                        {
                                            foreach (var record in kv.Value)
                                            {
                                                AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, getRealException(ee), record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                            }
                                        }
                                    }
                                }

                                logger.Warn($"[Change Term] 5. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                //Add reclassify history...

                                if (failedIds.Count > 0)
                                {
                                    FailedCount += failedIds.Count;
                                    string failedNames = string.Empty;
                                    foreach (var fid in failedIds)
                                    {
                                        failedNames += records.Where(t => t.Id == fid).FirstOrDefault().LeafName + ";";
                                    }
                                    failedNames = failedNames.TrimEnd(';');

                                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames);
                                    RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                                    if (!mIsGlobalSearch)
                                    {
                                        throw new Exception(string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), failedIds));
                                    }
                                }
                                else
                                {
                                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
                                }
                            }

                            if (mChangeTermType == ChangeTermType.AIMAChangeTerm)
                            {
                                var trainingScopeCount = ExplorerDao.QueryCount(r => r.TrainingTermId == termId);
                                var updateTrainingTerm = TrainingTermDao.Find(t => t.Id == termId);
                                if (updateTrainingTerm != null && MLTermStatusHelper.ActiveTermIntStatus.Contains(updateTrainingTerm.Status))
                                {
                                    updateTrainingTerm.TrainingScopeCount = trainingScopeCount;
                                    await TrainingTermDao.UpdateAsync(updateTrainingTerm);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_All);
                logger.Error("change term error:{0}", ex.ToString());
                throw ex;
            }
            finally
            {
                //UpdateTermUsage();
                if (mNeedAddLabelHistory)
                {
                    await RetentionCache.AddLabelHistoryAsync();
                }
                logger.Info("Change term action finish {0}", tempJobId);
            }
        }

        private bool IsContainerEnableClassification(string containerId)
        {
            if (!Guid.TryParse(containerId, out var guidId)) return false;
            var containerSetting = mAllSettings.FirstOrDefault(s => s.SiteGroupId == guidId && s.SiteId == Guid.Empty);
            return containerSetting == null || !containerSetting.IsNullClassificationSetting;
        }

        private string GetActionString()
        {
            //xml.HistoryList[0].Action = "RM_BCM_Audit_Action_ChangeTerm";
            //ExplorerDao.AddReocrdHistory(successIds, xml);

            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => "RM_BCM_Audit_Action_AIChangeTerm",
                ChangeTermType.AIMADirectlyApprove => "RM_BCM_Audit_Action_AIApprove",
                ChangeTermType.SearchChangeTerm => "RM_BCM_Audit_Action_ChangeTerm",
                _ => ""
            };
        }

        private int GetTrainingAddType()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)TrainingAddType.Reclassify,
                _ => (int)TrainingAddType.None
            };
        }

        private int GetMLApprovalStatus()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)RMMLApprovalStatus.Rejected,
                ChangeTermType.AIMADirectlyApprove => (int)RMMLApprovalStatus.Approved,
                _ => (int)RMMLApprovalStatus.None
            };
        }

        private int GetMLClassificationType()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)RMMLClassificationType.ManualClassified,
                ChangeTermType.AIMADirectlyApprove => (int)RMMLClassificationType.AutoClassfied,
                _ => (int)RMMLClassificationType.None
            };
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
                        ExplorerDao.UpdateRecordState(removeRecordInDB, (int)Contract.Explorer.RMRecordStatus.RMDeleted);
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

        private string getRealException(Exception e)
        {
            if (e == null)
            {
                return null;
            }
            if (e is System.Reflection.TargetInvocationException && e.InnerException != null)
            {
                return getRealException(e.InnerException);
            }
            return e.Message;
        }

        private bool UpdateLabel(IAveListItem aveItem, Guid termId, Guid recordId, Guid previousTermId)
        {
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

                //bool needApplyLabel = (!string.IsNullOrEmpty(previousLabelName) && currentLabel == previousLabelName && currentLabel != processingLabelName);


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
                        //using (var performance2 = new PerformanceScope("SP.RMEnforceRetentionProcesser.sendReport"))
                        //{
                        //    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        //    {
                        //        ObjectName = item.GetObjectName(),
                        //        SourceURL = itemUrl,
                        //        Action = "RM_EXO_EnforceRetention_TagLabel",
                        //        Status = JobDetailsStatus.Successful,
                        //    });
                        //}
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

        /*private async System.Threading.Tasks.Task ModifiedTermUsagesAsync(Guid previousTermId, Guid currentTermId, Guid recordId)
        {
            //需要判断TermId是否发生了变化...
            //当前TermId == DB中的TermId, 需要判断本次收集Job时， Record是否存在History Reclassify的操作. 若存在说明有做过Reclassify相关操作, 需要找到原始Term并-1处理, 不存在则不处理
            //若当前TermId != DB中的TermId, 需要到Reclassify操作对应的临时表中找最原始的TermId，并将该TermId的Size - 1, 当前Record关联的TermId的Size + 1, 
            //若Reclassify操作对应的临时表没有该记录相关信息, 则将该Record在Explorer DB中的TermId的Size - 1.
            //找到后删除该Record相关的Reclassify操作记录;

            var tempHistories = await ClassificationHistoryDao.FindListAsync(d => d.RecordId == recordId);
            var tempHistory = tempHistories.OrderBy(j => j.OperationTime).FirstOrDefault();
            if (tempHistory != null)
            {
                previousTermId = tempHistory.PreviousTermId;
                //Delete Classification History
                ClassificationHistoryDao.BatchDelete(tempHistories);
            }

            if (previousTermId != currentTermId)
            {
                logger.Debug($"previousTermId:{previousTermId}, currentTermId:{currentTermId}");
                if (previousTermId != Guid.Empty)
                {
                    //Previous Term - 1
                    AddTermChange(previousTermId, -1);
                }
                if (currentTermId != Guid.Empty)
                {
                    //Current Term + 1
                    AddTermChange(currentTermId, 1);
                }
            }
        }*/


        public void AddTermChange(Guid termId, long count)
        {
            if (mTermChangedDic.ContainsKey(termId))
            {
                mTermChangedDic[termId] += count;
            }
            else
            {
                mTermChangedDic.TryAdd(termId, count);
            }
        }

     

        private bool IsDeclared(Record record, IAveSite site, out IAveListItem aveItem)
        {
            if (web == null || (web != null && web.ID != record.WebId))
            {
                web = site.OpenWeb(record.WebId);
            }
            if (list == null || (list != null && list.ID != record.ListId))
            {
                list = web.GetList(record.ListId);
            }
            IAveListItem item = list.GetItemByUniqueId(record.ItemId);
            aveItem = item;
            return CheckisRecord(item);
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
                    //var DAUtil = new DAUtil();
                    //DAUtil.AddMoveToFilter(rule);
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(order, rule);
                }
            }

            newRuleCol.Rules = newRules;
            return newRuleCol;
        }

        private void AddReclassifyDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment, bool isDocument)
        {
            ArgumentCheck.CheckNotNull(record);
            ReportMangerFactory.Instance.ReportManager.Increase();
            var fullPath = string.Empty;
            var tempSiteUrl = string.Empty;
            if (record.DirPath != null)
            {
                if (string.IsNullOrWhiteSpace(mCurrentSiteUrl))
                {
                    var remoteNode = RemoteNodeDao.GetRemoteSiteCollectionById(record.AveSiteId);
                    if (remoteNode == null)
                    {
                        fullPath = record.DirPath;
                    }
                    else
                    {
                        tempSiteUrl = remoteNode.url;
                    }
                }
                else
                {
                    tempSiteUrl = mCurrentSiteUrl;
                }
                if (!string.IsNullOrEmpty(tempSiteUrl))
                {
                    fullPath = WebUtil.MakeFullUrl(tempSiteUrl, record.DirPath);
                }
            }
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = fullPath,
                Action = mChangeTermType == ChangeTermType.AIMADirectlyApprove ? "RM_MA_Approve" : "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment,
                Type = isDocument ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_RDM_RecordDetails_DataType_SPItem"
            });
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

        public void Dispose()
        {
            if (RetentionCache != null) 
            {
                RetentionCache = null;
            }
        }
    }
}
