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
using Aspose.Email.Storage.Pst;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Discovery;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Newtonsoft.Json;
using RAArchiverCommon.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static AvePoint.RA.Common.Utils.SimpleLocker;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RMExplorerUtility
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMExplorerUtility));

        protected Guid RevIMClassificationColumnID
        {
            get
            {
                return new Guid("20f84bba906045b4af568ee102a52dcb");
            }
        }
        #region use wrapper method to declared records.
        protected IAveSite currentAveSite = null;
        private IAveWeb currentAveWeb = null;
        #endregion
        #region use client api to update term value ,because wrapper update item method doesn't change modify time.
        //private ClientContext currentContext = null;
        private TaxonomySession taxonomySession = null;
        private string columnName = string.Empty;
        private ClientContext currentContext = null;
        //private IAveSite currentSite = null;
        private Web currentWeb = null;
        private List currentList = null;
        private Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
        protected bool mNeedSendReport = false;
        protected ChangeTermType mChangeTermType = ChangeTermType.None;
        public int FailedCount = 0;
        protected SPOLabelUtility labelUtility = null;
        protected Dictionary<Guid, string> cacheAllTermsDic;
        #endregion
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
        //private IExplorerDao mExplorerDao;
        //public IExplorerDao ExplorerDao
        //{
        //    get
        //    {
        //        if (mExplorerDao == null)
        //        {
        //            mExplorerDao = (IExplorerDao)PlatformWindsorManager.GetService(typeof(IExplorerDao)); ;
        //        }
        //        return mExplorerDao;
        //    }
        //}
        //private IMArchiverService mArchiverService;
        //public IMArchiverService ArchiverService
        //{
        //    get
        //    {
        //        if (mArchiverService == null)
        //        {
        //            mArchiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
        //        }
        //        return mArchiverService;
        //    }
        //}
        //DAOAPIClientV1 mDocAveClient = new DAOAPIClientV1();

        private ISharePointSettingDao mSharePointSettingDao = null;
        public ISharePointSettingDao SharePointSettingDao
        {
            get
            {
                if (mSharePointSettingDao == null)
                {
                    mSharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingDao;
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
        

        private IRMSecurityTrimmingHelper mSecurityTrimmingHelper = null;
        public IRMSecurityTrimmingHelper SecurityTrimmingHelper
        {
            get
            {
                if (mSecurityTrimmingHelper == null)
                {
                    mSecurityTrimmingHelper = (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
                }
                return mSecurityTrimmingHelper;
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
        
        private IRMRemoteNodeDao mRemoteNodeDao = null;
        public IRMRemoteNodeDao RemoteNodeDao
        {
            get
            {
                mRemoteNodeDao ??= (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                return mRemoteNodeDao;
            }
        }

        private IRMKeyValueDao mKeyValueDao;
        public IRMKeyValueDao KeyValueDao
        {
            get
            {
                if (mKeyValueDao == null)
                {
                    mKeyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
                }
                return mKeyValueDao;
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

        private ISettingProfilesDao mSettingProfileDao;

        private ISettingProfilesDao SettingProfileDao
        {
            get
            {
                if(mSettingProfileDao == null)
                {
                    mSettingProfileDao = (ISettingProfilesDao)PlatformWindsorManager.GetService(typeof(ISettingProfilesDao));
                }
                return mSettingProfileDao;
            }
        }

        private string? _generalRetentionLabel = null;

        public string GeneralRetentionLabel
        {
            get
            {
                    if (_generalRetentionLabel == null)
                    {
                        try
                        {
                            _generalRetentionLabel = GetGeneralRetentionLabel();
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"An error occured when GetGeneralRetentionLabel . Ex: {ex}");
                            _generalRetentionLabel = null;
                        }
                    }
                    return _generalRetentionLabel ?? string.Empty;
            }
        }

        public RMExplorerUtility(bool initForReclassify = false, ChangeTermType changeTermType = ChangeTermType.SearchChangeTerm)
        {
            mChangeTermType = changeTermType;
            if (initForReclassify)
            {
                labelUtility = new SPOLabelUtility();
            }
        }
        public RMExplorerUtility(bool needSendReport, bool initForReclassify, ChangeTermType changeTermType = ChangeTermType.SearchChangeTerm)
        {
            mNeedSendReport = needSendReport;
            mChangeTermType = changeTermType;
            if (initForReclassify)
            {
                labelUtility = new SPOLabelUtility(mNeedSendReport);
            }
        }

        public RMExplorerUtility(RemoteSiteCollection site)
        {
            //currentContext = InitContext(site);
        }

        public RMExplorerUtility(string fullPath)
        {
            //currentContext = InitContext(fullPath);
        }

        public string GetBcsColumnName(RemoteSiteCollection site)
        {
            return SharePointSettingDao.GetBcsColumnName(site);
        }

        private string GetGeneralRetentionLabel()
        {
            SettingProfileDto profileDto = new SettingProfileDto
            {
                Type = (int)SettingProfilesType.RecordsLabelSetting,
                Name = "RecordsLabelSetting"
            };
            var dto = SettingProfileDao.Load(profileDto);
            return dto?.Settings ?? string.Empty;
        }
        #region obsolete
        //[Obsolete]
        //public void ChangeAllTerms(ChangeTermDto changeTermInfo, string tempJobId)
        //{
        //    try
        //    {
        //        logger.Info("Change term action start {0}", tempJobId);
        //        var termInfo = changeTermInfo.TermInfo;
        //        List<Record> records = new List<Record>();

        //        if (changeTermInfo.RecordIds != null && changeTermInfo.RecordIds.Count > 0)
        //        {
        //            using (new RA.Common.PerformanceScope(string.Format("change.Term.GetRecords")))
        //            {
        //                records = ExplorerDao.QueryAll(r => changeTermInfo.RecordIds.Contains(r.Id)).ToList();
        //                //records = CollectionDataDao.GetRecordByIds(changeTermInfo.RecordIds);//to do
        //            }

        //            var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
        //            var avesiteIds = recDic.Keys.ToList();
        //            Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
        //            List<Guid> failedIds = new List<Guid>();
        //            if (avesiteIds.Count > 0)
        //            {
        //                string termName = changeTermInfo.TermInfo.Name;
        //                Guid termId = changeTermInfo.TermInfo.UniqueId;
        //                using (new RA.Common.PerformanceScope(string.Format("change.Term.GetSites")))
        //                {
        //                    siteDic = mDocAveClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
        //                }

        //                foreach (var recList in recDic.Values)
        //                {
        //                    if (recList.Count > 0)
        //                    {
        //                        try
        //                        {
        //                            if (siteDic.ContainsKey(recList[0].AveSiteId))
        //                            {
        //                                var site = siteDic[recList[0].AveSiteId];
        //                                InitContext(site);
        //                                List<Guid> successIds = ChangeRecordTermAction(recList, termName, termId, ref failedIds);

        //                                //ExplorerDao.ChangeTerm(successIds, termInfo.UniqueId);
        //                                ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => 
        //                                {
        //                                    rec.TermId = termInfo.UniqueId;
        //                                    rec.TermName = termInfo.Name;
        //                                    rec.RuleId = Guid.Empty;
        //                                    rec.DisposalDueDate = I18NEntity.GetString("RM_JS_JM_EndTimePending");
        //                                    rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
        //                                });
        //                            }
        //                            else
        //                            {
        //                                List<Guid> recIds = new List<Guid>();
        //                                if (recList[0].SourceFlag == 1)
        //                                {
        //                                    throw new Exception("can't get site obj");
        //                                }
        //                                foreach (var rd in recList)
        //                                {
        //                                    if (rd.SourceFlag == 2)
        //                                    {
        //                                        recIds.Add(rd.Id);
        //                                    }
        //                                }
        //                                var term = TermDao.GetRMTermByGuId(termInfo.UniqueId);
        //                                if (term != null)
        //                                {
        //                                    ExplorerDao.UpdateAll(r => recIds.Contains(r.Id), rec => 
        //                                    {
        //                                        rec.TermId = term.UniqueId;
        //                                        rec.TermName = term.Name;
        //                                        rec.RuleId = Guid.Empty;
        //                                        rec.DisposalDueDate = I18NEntity.GetString("RM_JS_JM_EndTimePending");
        //                                        rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
        //                                    });
        //                                }

        //                            }
        //                        }
        //                        catch (Exception ee)
        //                        {
        //                            failedIds.AddRange(recList.Select(t => t.Id));
        //                            logger.Warn("change term action failed {0}", ee.ToString());
        //                        }
        //                    }
        //                }
        //            }
        //            if (failedIds.Count > 0)
        //            {
        //                string failedNames = string.Empty;
        //                foreach (var fid in failedIds)
        //                {
        //                    failedNames += records.Where(t => t.Id == fid).FirstOrDefault().LeafName + ";";
        //                }
        //                failedNames = failedNames.TrimEnd(';');
        //                //RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames);
        //                //throw new Exception("have failed record in change term action");
        //                throw new Exception(string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), failedNames));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("change term error:{0}", ex.ToString());
        //        throw ex;
        //    }
        //}
        //[Obsolete]
        //public void DeclaredRecords(List<Guid> recordIds, string tempJobId, bool isDeclare, string declaredBy)
        //{
        //    try
        //    {
        //        logger.Info("Declared Records action start {0}", tempJobId);
        //        List<Record> records = new List<Record>();
        //        if (recordIds != null && recordIds.Count > 0)
        //        {
        //            records = ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
        //               //records = CollectionDataDao.GetRecordByIds(RecordIds);//to do

        //            var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
        //            var avesiteIds = recDic.Keys.ToList();
        //            Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
        //            List<Guid> failedIds = new List<Guid>();
        //            if (avesiteIds.Count > 0)
        //            {
        //                siteDic = mDocAveClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
        //                foreach (var recList in recDic.Values)
        //                {
        //                    if (recList.Count > 0)
        //                    {
        //                        try
        //                        {
        //                            var site = siteDic[recList[0].AveSiteId];
        //                            var bposInfo = PoolUserUtil.GetBPOSInfo(site);
        //                            var factory = AveObjectModelFactory.CreateObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
        //                            var spSite = factory.CreateSite();
        //                            var IRecords = factory.CreateRecords();
        //                            EnsureRecordFeatureEnabled(spSite);

        //                            if (isDeclare)
        //                            {
        //                                List<Guid> successIds = DeclaredRecord(IRecords, spSite, recList, ref failedIds);
        //                                ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = true; rec.DeclaredBy = declaredBy; });
        //                            }
        //                            else
        //                            {
        //                                List<Guid> successIds = UnDeclaredRecord(IRecords, spSite, recList, ref failedIds);
        //                                ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = false; rec.DeclaredBy = declaredBy; });
        //                            }
        //                        }
        //                        catch (Exception ee)
        //                        {
        //                            failedIds.AddRange(recList.Select(t => t.Id));
        //                            logger.Warn("Declared Records action failed {0}", ee.ToString());
        //                        }
        //                    }
        //                }
        //                //System.Threading.Tasks.Parallel.ForEach(recDic.Values, (recList) =>
        //                //{
        //                //    if (recList.Count > 0)
        //                //    {
        //                //        try
        //                //        {
        //                //            var site = siteDic[recList[0].AveSiteId];
        //                //            var bposInfo = PoolUserUtil.GetBPOSInfo(site);
        //                //            var factory = AveObjectModelFactory.CreateObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
        //                //            var spSite = factory.CreateSite();
        //                //            var IRecords = factory.CreateRecords();
        //                //            EnsureRecordFeatureEnabled(spSite);

        //                //            if (isDeclare)
        //            {
        //                List<Guid> successIds = DeclaredRecord(IRecords, spSite, recList, ref failedIds);
        //                ExplorerDao1.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = true; rec.DeclaredBy = declaredBy; });
        //            }
        //            else
        //            {
        //                List<Guid> successIds = UnDeclaredRecord(IRecords, spSite, recList, ref failedIds);
        //                ExplorerDao1.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = false; rec.DeclaredBy = declaredBy; });
        //            }
        //        }
        //        catch (Exception ee)
        //        {
        //            failedIds.AddRange(recList.Select(t => t.Id));
        //            logger.Warn("Declared Records action failed {0}", ee.ToString());
        //        }
        //    }
        //});
        //            }
        //            if (failedIds.Count > 0)
        //            {
        //                string failedNames = string.Empty;
        //                foreach (var fid in failedIds)
        //                {
        //                    failedNames += records.Where(t => t.Id == fid).FirstOrDefault().LeafName + "; ";
        //                }
        //                if (!string.IsNullOrEmpty(failedNames))
        //                {
        //                    failedNames = failedNames.Trim().TrimEnd(';');
        //                }
        //                //RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames);
        //                throw new Exception(failedNames);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Declare record error:{0}", ex.ToString());
        //        throw ex;
        //    }
        //}
        #endregion
        public async System.Threading.Tasks.Task ChangeAllTermsAsync(ChangeTermOption changeTermInfo, string tempJobId, bool waiting4OtherSource)
        {
            try
            {
                using (new RA.Common.PerformanceScope("RMExplorerUtility.ChangeTermForSP"))
                {
                    var isNewLogicAccount = TenantService.IsNewOpusTenant();
                    logger.Info("Is new logic account is {0}", isNewLogicAccount);
                    logger.Info("Change term action start {0}", tempJobId);
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, "");
                    mRMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                    List<Record> records = new List<Record>();
                    //RecordHistoryXml xml = new RecordHistoryXml()
                    //{
                    //    HistoryList = new List<RecordHistory>()
                    //};
                    //var userName = WebUtil.LogOnUserName;
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_ChangeTerm",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    if (changeTermInfo.SourceRecordIds != null && changeTermInfo.SourceRecordIds.Count > 0)
                    {
                        var startTime = DateTime.Now;
                        using (new RA.Common.PerformanceScope(string.Format("change.Term.GetRecords")))
                        {
                            //var simpleRecords = ExplorerDao.QueryAllSimple(r => changeTermInfo.SourceRecordIds.Contains(r.Id)).ToList();
                            //logger.Warn($"0. time elapsed for query {simpleRecords.Count} simple records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                            //startTime = DateTime.Now;
                            records = ExplorerDao.QueryAll(r => changeTermInfo.SourceRecordIds.Contains(r.Id)).ToList();
                            logger.Warn($"[Change Term] 1. time elapsed for query {records.Count} records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");

                            List<Guid> allGuids = new List<Guid>();
                            allGuids.AddRange(changeTermInfo.SourceRecordIds);
                            allGuids.AddRange(changeTermInfo.SourceEXORecordIds);
                            allGuids.AddRange(changeTermInfo.SourceFSRecordIds);
                            allGuids.AddRange(changeTermInfo.SourceOneDriveRecordIds);
                            var recordsNoti = ExplorerDao.QueryAll(r => allGuids.Contains(r.Id)).ToList();
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(recordsNoti.Select(r => r.LeafName).ToList()));
                            //records = CollectionDataDao.GetRecordByIds(changeTermInfo.RecordIds);//to do
                        }
                        var trainingTerm = TrainingTermDao.GetTrainingTerm(changeTermInfo.TargetTermUniqueId);
                        if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                        {
                            var termsIds = records.Select(t => t.PredictTermId).ToList();
                            cacheAllTermsDic = (await TermDao.FindListAsync(tm => termsIds.Contains(tm.UniqueId))).ToDictionary(t => t.UniqueId, t => t.Name);
                        }
                        var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                        var avesiteIds = recDic.Keys.ToList();
                        Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
                        List<Guid> failedIds = new List<Guid>();
                        List<Guid> successIds = new List<Guid>();
                        List<Record> successRecords = new List<Record>();

                        if (mChangeTermType == ChangeTermType.AIMAChangeTerm && changeTermInfo.TargetTermId == -1) //No Term
                        {
                            foreach (var rec in records)
                            {
                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                rec.MLClassificationType = (int)RMMLClassificationType.Rejected;
                            }
                            var faileds = ExplorerDao.BatchUpdate(records, 5);
                            if (mNeedSendReport)
                            {
                                foreach (var rec in records)
                                {
                                    AddReclassifyDetailForGlobalSearch(rec, faileds.Contains(rec.Id) ? JobDetailsStatus.Failed : JobDetailsStatus.Successful, "", rec.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                }
                            }
                        }
                        else
                        {
                            if (avesiteIds.Count > 0)
                            {
                                string termName = changeTermInfo.TargetTermName;
                                Guid termId = changeTermInfo.TargetTermUniqueId;
                                using (new RA.Common.PerformanceScope(string.Format("change.Term.GetSites")))
                                {
                                    startTime = DateTime.Now;
                                    //siteDic = mDocAveClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
                                    siteDic = RABrowserClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
                                    logger.Warn($"[Change Term] 2. time elapsed for query from DAO {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                }
                                foreach (var recList in recDic.Values)
                                {
                                    if (recList.Count > 0)
                                    {
                                        try
                                        {
                                            if (siteDic.ContainsKey(recList[0].AveSiteId))
                                            {
                                                var site = siteDic[recList[0].AveSiteId];
                                                startTime = DateTime.Now;
                                                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(site);
                                                logger.Warn($"[Declare] 3.time elapsed for GetBPOSInfo {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                startTime = DateTime.Now;
                                                var factory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                                                var spSite = factory.CreateSite();
                                                labelUtility.CacheSPLabel(spSite);
                                                currentAveSite = spSite;
                                                logger.Warn($"[Declare] 4.1.time elapsed for CreateSite {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                startTime = DateTime.Now;
                                                var columnName = GetBCSColumn(site);

                                                successRecords = ChangeRecordTermAction(spSite, columnName, recList, termName, termId, factory, bposInfo, ref failedIds);
                                                successIds = successRecords.Select(a => a.Id).ToList();
                                                logger.Warn($"[Change Term] 4. time elapsed for ChangeRecordTermAction {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                //ExplorerDao.ChangeTerm(successIds, termInfo.UniqueId);
                                                startTime = DateTime.Now;
                                                if (successIds.Count > 0)
                                                {
                                                    if (mChangeTermType == ChangeTermType.AIMAChangeTerm)
                                                    {
                                                        if (trainingTerm != null && MLTermStatusHelper.ActiveTermStatus.Contains(trainingTerm.Status))
                                                        {
                                                            var previousTermId = Guid.Empty;
                                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                                            {
                                                                previousTermId = rec.TermId;
                                                                rec.TermId = termId;
                                                                rec.TermName = termName;
                                                                rec.RuleId = Guid.Empty;
                                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();

                                                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                                                rec.MLClassificationType = GetMLClassificationType();

                                                                rec.TrainingAddType = GetTrainingAddType();
                                                                rec.TrainingScope = (int)MLFileStatus.NotTrain;
                                                                rec.TrainingTermId = termId;
                                                                if(isNewLogicAccount && previousTermId != termId) rec.RemoveManualFields();
                                                            });
                                                        }
                                                        else
                                                        {
                                                            var previousTermId = Guid.Empty;
                                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                                            {
                                                                previousTermId = rec.TermId;
                                                                rec.TermId = termId;
                                                                rec.TermName = termName;
                                                                rec.RuleId = Guid.Empty;
                                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();

                                                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                                                rec.MLClassificationType = GetMLClassificationType();
                                                                if(isNewLogicAccount && previousTermId != termId) rec.RemoveManualFields();
                                                            });
                                                        }
                                                    }
                                                    else if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                                                    {
                                                        var previousTermId = Guid.Empty;
                                                        foreach (var tempSuccess in successRecords)
                                                        {
                                                            previousTermId = tempSuccess.TermId;
                                                            var tempTermName = "";
                                                            if (cacheAllTermsDic.ContainsKey(tempSuccess.PredictTermId))
                                                            {
                                                                termName = cacheAllTermsDic[tempSuccess.PredictTermId];
                                                            }
                                                            else
                                                            {
                                                                logger.Warn($"Can not found this term:{tempSuccess.PredictTermId}");
                                                            }
                                                            tempSuccess.TermId = tempSuccess.PredictTermId;
                                                            tempSuccess.TermName = tempTermName;
                                                            tempSuccess.RuleId = Guid.Empty;
                                                            tempSuccess.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            tempSuccess.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            tempSuccess.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                            tempSuccess.RecordOwner_Array = tempSuccess.RecordOwner.ExplorerSearchSplit();

                                                            tempSuccess.MLApprovalStatus = GetMLApprovalStatus();
                                                            tempSuccess.MLClassificationType = GetMLClassificationType();
                                                            if(isNewLogicAccount && previousTermId != tempSuccess.TermId) tempSuccess.RemoveManualFields();
                                                            //ExplorerDao.AddOrUpdateRecord(tempSuccess, true);
                                                        }
                                                        ExplorerDao.BatchUpdate(successRecords, 5);
                                                    }
                                                    else if (mChangeTermType == ChangeTermType.SearchChangeTerm)
                                                    {
                                                        var previousTermId = Guid.Empty;
                                                        ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                                        {
                                                            previousTermId = rec.TermId;
                                                            rec.TermId = termId;
                                                            rec.TermName = termName;
                                                            rec.RuleId = Guid.Empty;
                                                            rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                            rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
                                                            if (isNewLogicAccount && previousTermId != termId) rec.RemoveManualFields();
                                                        });
                                                    }

                                                }
                                                logger.Warn($"[Change Term] 5. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                //Add reclassify history...
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
                                                logger.Warn($"[Change Term] 6. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                if (successIds.Count > 0)
                                                {
                                                    string actionString = GetActionString();
                                                    RecordsHistoryService.AddRecordsHistory(successIds, actionString, changeTermInfo.Comment);
                                                    startTime = DateTime.Now;
                                                    logger.Warn($"[Change Term] 6. time elapsed for AddReocrdHistory(succeed) to cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                }
                                            }
                                            else
                                            {
                                                List<Guid> recIds = new List<Guid>();
                                                if (recList[0].SourceFlag == 1)
                                                {
                                                    throw new Exception("RM_RDM_SCNotFound");
                                                }
                                                #region remove fs logic
                                                //    foreach (var rd in recList)
                                                //    {
                                                //        if (rd.SourceFlag == 2)
                                                //        {
                                                //            recIds.Add(rd.Id);
                                                //        }
                                                //    }
                                                //    var term = TermDao.GetRMTermByGuId(termId);
                                                //    if (term != null)
                                                //    {
                                                //        if (mChangeTermType == ChangeTermType.SearchChangeTerm)
                                                //        {
                                                //            ExplorerDao.UpdateAll(r => recIds.Contains(r.Id), rec =>
                                                //            {
                                                //                rec.TermId = term.UniqueId;
                                                //                rec.TermName = term.Name;
                                                //                rec.RuleId = Guid.Empty;
                                                //                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                //                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                //            });
                                                //        }
                                                //        else if (mChangeTermType == ChangeTermType.AIMAChangeTerm || mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                                                //        {
                                                //            ExplorerDao.UpdateAll(r => recIds.Contains(r.Id), rec =>
                                                //            {
                                                //                rec.TermId = term.UniqueId;
                                                //                rec.TermName = term.Name;
                                                //                rec.RuleId = Guid.Empty;
                                                //                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                //                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                //                rec.TrainingAddType = GetTrainingAddType();
                                                //                rec.MLApprovalStatus = GetMLApprovalStatus();
                                                //                rec.MLClassificationType = GetMLClassificationType();
                                                //                rec.TrainingScope = (int)MLFileStatus.NotTrain;
                                                //                rec.TrainingTermId = termId;
                                                //            });
                                                //        }
                                                //    }
                                                #endregion
                                            }
                                        }
                                        catch (Exception ee)
                                        {
                                            failedIds.AddRange(recList.Select(t => t.Id));
                                            logger.Warn("change term action failed {0}", ee.ToString());
                                            if (mNeedSendReport)
                                            {
                                                foreach (var record in recList)
                                                {
                                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, getRealException(ee), record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                                }
                                            }
                                        }
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

                        if (failedIds.Count > 0)
                        {
                            if (successIds.Any())
                            {
                                string actionString = GetActionString();
                                RecordsHistoryService.AddRecordsHistory(successIds, actionString, changeTermInfo.Comment);
                                startTime = DateTime.Now;
                                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
                                logger.Warn($"[Change Term] 7. time elapsed for AddReocrdHistory(succeed) to cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                return;
                            }
                            FailedCount += failedIds.Count;
                            string failedNames = string.Empty;
                            foreach (var fid in failedIds)
                            {
                                failedNames += records.Where(t => t.Id == fid).FirstOrDefault().LeafName + ";";
                            }
                            failedNames = failedNames.TrimEnd(';');
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames, RecordsConstants.Explorer_RealTime_Failed_All);
                            //xml.HistoryList[0].Action = "RM_JS_Audit_ChangeTermErrorMessage";
                            //ExplorerDao.AddReocrdHistory(failedIds, xml);
                            RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                            //throw new Exception("have failed record in change term action");
                            if (!mNeedSendReport)
                            {
                                throw new Exception(string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), failedIds));
                            }
                        }
                        else
                        {
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
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
                if (labelUtility != null && labelUtility.LabelApplied)
                {
                    await labelUtility.AddLabelHistoryAsync();
                }
                logger.Info("Change term action finish {0}", tempJobId);
            }
        }

        protected string GetActionString()
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

        protected int GetTrainingAddType()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)TrainingAddType.Reclassify,
                _ => (int)TrainingAddType.None
            };
        }

        protected int GetMLApprovalStatus()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)RMMLApprovalStatus.Rejected,
                ChangeTermType.AIMADirectlyApprove => (int)RMMLApprovalStatus.Approved,
                _ => (int)RMMLApprovalStatus.None
            };
        }

        protected int GetMLClassificationType()
        {
            return mChangeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => (int)RMMLClassificationType.ManualClassified,
                ChangeTermType.AIMADirectlyApprove => (int)RMMLClassificationType.AutoClassfied,
                _ => (int)RMMLClassificationType.None
            };
        }

        public async System.Threading.Tasks.Task DeclaredRecordsAsync(List<Guid> recordIds, string tempJobId, bool isDeclare, string declaredBy)
        {
            try
            {
                using (new RA.Common.PerformanceScope("RMExplorerUtility.DeclaredRecordsForSP"))
                {
                    logger.Info("Declared Records action start {0}", tempJobId);
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running);
                    var startTime = DateTime.Now;
                    List<Record> records = new List<Record>();
                    var isNewLogicAccount = TenantService.IsNewOpusTenant();
                    if (recordIds != null && recordIds.Count > 0)
                    {
                        records = ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                        logger.Warn($" [Declare] 1.time elapsed for query {records.Count} records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        //records = CollectionDataDao.GetRecordByIds(RecordIds);//to do
                        RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(records.Select(r => r.LeafName).ToList()));
                        var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                        var avesiteIds = recDic.Keys.ToList();
                        Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
                        List<Guid> failedIds = new List<Guid>();
                        if (avesiteIds.Count > 0)
                        {
                            startTime = DateTime.Now;
                            //siteDic = mDocAveClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
                            siteDic = RABrowserClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
                            logger.Warn($"[Declare] 2.time elapsed for query from DAO {(DateTime.Now - startTime).TotalMilliseconds} ms");
                            foreach (var recList in recDic.Values)
                            {
                                if (recList.Count > 0)
                                {
                                    try
                                    {
                                        if (siteDic.ContainsKey(recList[0].AveSiteId))
                                        {
                                            var site = siteDic[recList[0].AveSiteId];
                                            startTime = DateTime.Now;
                                            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(site);
                                            logger.Warn($"[Declare] 3.time elapsed for GetBPOSInfo {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            startTime = DateTime.Now;
                                            var factory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                                            var spSite = factory.CreateSite();
                                            currentAveSite = spSite;
                                            logger.Warn($"[Declare] 4.1.time elapsed for CreateSite {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            startTime = DateTime.Now;
                                            var IRecords = factory.CreateRecords();
                                            logger.Warn($"[Declare] 4.2.time elapsed for CreateRecords {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            startTime = DateTime.Now;
                                            spSite.EnsureRecordFeatureEnabled(mRecordFeatureId);
                                            logger.Warn($"[Declare] 4.3.time elapsed for EnsureRecordFeatureEnabled {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            if (AccountUtility.IsSupportRecordLabel())
                                            {
                                                if (isDeclare)
                                                {
                                                    startTime = DateTime.UtcNow;
                                                    (List<Guid> successIds, failedIds) = await AddRecordLabelToItem(spSite, recList, IRecords);
                                                    logger.Warn($"[AddRecordLabel] 5.time elapsed for Add Record Label {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    startTime = DateTime.UtcNow;
                                                    ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.LockedByRecordLabel = true; rec.ApplyRecordLabelBy = declaredBy; rec.DeclaredBy = declaredBy; rec.DeclareAsRecord = false; });
                                                    logger.Warn($"[Declare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    if (successIds != null && successIds.Count > 0 && mNeedSendReport)
                                                    {
                                                        RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_AddRecordLabel");
                                                    }
                                                }
                                                else
                                                {
                                                    startTime = DateTime.Now;
                                                    (List<Guid> successIds, failedIds) = RemoveRecordLabel(spSite, recList);
                                                    logger.Warn($"[UnDeclare] 5.time elapsed for undeclare records {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    startTime = DateTime.Now;
                                                    ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.LockedByRecordLabel = false; rec.ApplyRecordLabelBy = declaredBy; rec.DeclaredBy = declaredBy; });
                                                    logger.Warn($"[UnDeclare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    if (successIds != null && successIds.Count > 0 && mNeedSendReport)
                                                    {
                                                        RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_RemoveRecordLabel");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (isDeclare)
                                                {
                                                    startTime = DateTime.Now;
                                                    (List<Guid> successIds, failedIds) = await DeclaredRecordAsync(IRecords, spSite, recList);
                                                    logger.Warn($"[Declare] 5.time elapsed for declare record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    startTime = DateTime.Now;
                                                    ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = true; rec.DeclaredBy = declaredBy; });
                                                    logger.Warn($"[Declare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");

                                                    if (successIds != null && successIds.Count > 0 && mNeedSendReport)
                                                    {
                                                        RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_DeclareAsRecord");
                                                    }
                                                }
                                                else
                                                {
                                                    startTime = DateTime.Now;
                                                    List<Guid> successIds = UnDeclaredRecord(IRecords, spSite, recList, ref failedIds);
                                                    logger.Warn($"[UnDeclare] 5.time elapsed for undeclare records {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    startTime = DateTime.Now;
                                                    ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = false; rec.DeclaredBy = declaredBy; });
                                                    logger.Warn($"[UnDeclare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                    if (successIds != null && successIds.Count > 0 && mNeedSendReport)
                                                    {
                                                        RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_UndeclareAsRecord");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            logger.Error($"Site collection not found. Id:{recList[0].AveSiteId}");
                                            throw new Exception("RM_RDM_SCNotFound");
                                        }
                                    }
                                    catch (Exception ee)
                                    {
                                        failedIds.AddRange(recList.Select(t => t.Id));
                                        logger.Warn("Declared Records action failed {0}", ee.ToString());
                                        if (mNeedSendReport)
                                        {
                                            foreach (var record in recList)
                                            {
                                                AddDeclareDetailForGlobalSearch(record, JobDetailsStatus.Failed, getRealException(ee), isDeclare, record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                            }
                                        }
                                    }
                                }
                            }
                            //System.Threading.Tasks.Parallel.ForEach(recDic.Values, (recList) =>
                            //{
                            //    if (recList.Count > 0)
                            //    {
                            //        try
                            //        {
                            //            var site = siteDic[recList[0].AveSiteId];
                            //            var bposInfo = PoolUserUtil.GetBPOSInfo(site);
                            //            var factory = AveObjectModelFactory.CreateObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                            //            var spSite = factory.CreateSite();
                            //            var IRecords = factory.CreateRecords();
                            //            EnsureRecordFeatureEnabled(spSite);

                            //            if (isDeclare)
                            //            {
                            //                List<Guid> successIds = DeclaredRecord(IRecords, spSite, recList, ref failedIds);
                            //                ExplorerDao1.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = true; rec.DeclaredBy = declaredBy; });
                            //            }
                            //            else
                            //            {
                            //                List<Guid> successIds = UnDeclaredRecord(IRecords, spSite, recList, ref failedIds);
                            //                ExplorerDao1.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = false; rec.DeclaredBy = declaredBy; });
                            //            }
                            //        }
                            //        catch (Exception ee)
                            //        {
                            //            failedIds.AddRange(recList.Select(t => t.Id));
                            //            logger.Warn("Declared Records action failed {0}", ee.ToString());
                            //        }
                            //    }
                            //});
                        }
                        if (failedIds.Count > 0)
                        {
                            FailedCount += failedIds.Count;
                            string failedNames = string.Empty;
                            foreach (var fid in failedIds)
                            {
                                failedNames += records.Where(t => t.Id == fid).FirstOrDefault()?.LeafName + "; ";
                            }
                            if (!string.IsNullOrEmpty(failedNames))
                            {
                                failedNames = failedNames.Trim().TrimEnd(';');
                            }
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames);
                            if (!mNeedSendReport)
                            {
                                throw new Exception(failedNames);
                            }
                        }
                        else
                        {
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
                logger.Error("Declare record error:{0}", ex.ToString());
                throw ex;
            }
            finally
            {
                logger.Info("Declared Records action finish {0}", tempJobId);
            }
        }

        protected virtual string GetBCSColumn(RemoteSiteCollection site)
        {
            var webApp = RABrowserClient.GetWebApplicationById(site.parentId);
            var groupLevelSetting = SharePointSettingDao.GetGroupLevelGlobalSetting(webApp.url, new Guid(webApp.id));
            var columnName = groupLevelSetting.IsUsingExistColumnName ? groupLevelSetting.ExistColumnName : groupLevelSetting.ColumnName;
            return columnName;
        }

        private void AddDeclareDetailForGlobalSearchJob(IAveListItem item, JobDetailsStatus status, string comment, bool isDeclare)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = item?.Name,
                FullPath = item?.FullPath(),
                Action = isDeclare ? "RM_BCM_History_DeclareAsRecord" : "RM_RDM_CreateRule_Options_UndeclareDocumnet",
                Status = status,
                Comment = comment,
                Type = item == null ? "" : item.File != null ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_JS_Rule_ObjectLevel_Item"
            });
        }

        private void AddRecordLabelDetailForGlobalSearchJob(IAveListItem item, JobDetailsStatus status, string comment, bool isDeclare)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = item?.Name,
                FullPath = item?.FullPath(),
                Action = isDeclare ? "RM_BCM_History_AddRecordLabel" : "RM_BCM_History_RemoveRecordLabel",
                Status = status,
                Comment = comment,
                Type = item == null ? "" : item.File != null ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_JS_Rule_ObjectLevel_Item"
            });
        }

        private void AddDeclareDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment, bool isDeclare, bool isDocument)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : currentAveSite == null ? record.DirPath : WebUtil.MakeFullUrl(currentAveSite.Url, record.DirPath),
                Action = isDeclare ? "RM_BCM_History_DeclareAsRecord" : "RM_RDM_CreateRule_Options_UndeclareDocumnet",
                Status = status,
                Comment = comment,
                Type = isDocument ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_RDM_RecordDetails_DataType_SPItem"
            });
        }

        private void AddRecordLabelDetailForGlobalSearchJob(Record record, JobDetailsStatus status, string comment, bool isDeclare, bool isDocument)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : currentAveSite == null ? record.DirPath : WebUtil.MakeFullUrl(currentAveSite.Url, record.DirPath),
                Action = isDeclare ? "RM_BCM_History_AddRecordLabel" : "RM_BCM_History_RemoveRecordLabel",
                Status = status,
                Comment = comment,
                Type = isDocument ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_RDM_RecordDetails_DataType_SPItem"
            });
        }

        protected void AddReclassifyDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment, bool isDocument)
        {
            logger.Info($"Out put details, records id: {record.Id}");
            ReportMangerFactory.Instance.ReportManager.Increase();
            var fullPath = string.Empty;
            var tempSiteUrl = string.Empty;
            if (record.DirPath != null)
            {
                if (string.IsNullOrWhiteSpace(currentAveSite?.Url))
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
                    tempSiteUrl = currentAveSite?.Url;
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
        // this method all records from same site

        public List<Record> ChangeRecordTermAction(IAveSite site, string bcsColumnName, List<Record> records, string termName, Guid termId, AveObjectModelFactory factory, AveBPOSAccountInfo bposInfo, ref List<Guid> failedIds)
        {
            List<Record> successRecords = new List<Record>();
            IAveWeb web = null;
            IAveList list = null;
            IAveTaxonomyField field = null;
            try
            {
                foreach (var record in records)
                {
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

                    logger.Info("change term action {0}:{1}", record.Id, termName);
                    bool isDocument = false;
                    try
                    {
                        switch (record.NodeType)
                        {
                            case (int)RMNodeLevel.SiteCollection:
                                ReclassifySiteCollection(site, termId, factory, bposInfo, failedIds, successRecords, record);
                                continue;
                            case (int)RMNodeLevel.Site:
                                ReclassifySite(site, web, termId, factory, bposInfo, failedIds, successRecords, record);
                                continue;
                            case (int)RMNodeLevel.List:
                            case (int)RMNodeLevel.Library:
                                ReclassifyList(site, web, termId, failedIds, successRecords, record);
                                continue;
                        }
                        if (web == null || (web != null && web.ID != record.WebId))
                        {
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                            field = GetBCSField(list, bcsColumnName);
                        }
                        if (!InSameTermScope(termId, field))
                        {
                            throw new Exception("RM_FS_FolderReclassify_FileNotInSameTermScope");
                        }
                        isDocument = list.BaseTemplate == AveListTemplateType.DocumentLibrary || list.BaseType == AveBaseType.DocumentLibrary;
                        IAveListItem item = list.GetItemByUniqueId(record.ItemId);
                        if (SPSettingsUtility.ShouldSkipArchivedItem(item))
                        {
                            UpdateArchivedItem(record);
                            if (mNeedSendReport)
                            {
                                AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Skipped, string.Empty, isDocument);
                            }
                            continue;
                        }
                        //isDocument = IsDocument(item);
                        UpdateTerm(item, field, termName, termId);
                        successRecords.Add(record);
                        bool labelNotExist = labelUtility.UpdateLabel(item, termId, record.Id, record.TermId);
                        if (mNeedSendReport)
                        {
                            AddReclassifyDetailForGlobalSearch(record, labelNotExist ? JobDetailsStatus.Failed : JobDetailsStatus.Successful, labelNotExist ? "RM_SPO_ApplySetting_LabelNotExist" : "", isDocument);
                            if (labelNotExist)
                            {
                                FailedCount++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        JobDetailsStatus _status = JobDetailsStatus.Failed;
                        if (isItemNotFoundError(e))
                        {
                            _status = JobDetailsStatus.Skipped;
                            this.UpdateRemoveItem(record);
                        }
                        else
                        {
                            failedIds.Add(record.Id);
                        }
                        if (mNeedSendReport)
                        {
                            AddReclassifyDetailForGlobalSearch(record, _status, getRealException(e), isDocument);
                        }
                        logger.Warn("update item term failed {0}:{1} error {2}", record?.Id, record.TermName, e.ToString());
                    }
                }
            }
            finally
            {
                try
                {
                    currentAveSite?.Dispose();
                    currentAveWeb?.Dispose();
                }
                catch (Exception ce)
                {
                    logger.Warn("Disposal current context error {0}", ce.ToString());
                }
            }
            return successRecords;
        }

        private void ReclassifyList(IAveSite site, IAveWeb web, Guid termId, List<Guid> failedIds, List<Record> successRecords, Record record)
        {
            try
            {
                if (web == null || (web != null && web.ID != record.WebId))
                {
                    web = site.OpenWeb(record.WebId);
                }
                var aveList = web.GetList(record.ListId);
                if (SPSettingsUtility.NeedUpdateContainer(aveList, termId))
                {
                    SPSettingsUtility.ConfigBCSProperty(aveList, termId);
                    successRecords.Add(record);
                    logger.Info($"reclassify success for list: {record?.Id}");
                }
                else
                {
                    logger.Info($"Skip reclassify for list: {record?.Id}");
                }
            }
            catch (Exception e)
            {
                failedIds.Add(record.Id);
                logger.Info($"reclassify list failed {record?.Id}:{e}");
            }
        }

        private void ReclassifySite(IAveSite site, IAveWeb web, Guid termId, AveObjectModelFactory factory, AveBPOSAccountInfo bposInfo, List<Guid> failedIds, List<Record> successRecords, Record record)
        {
            try
            {
                if (web == null || (web != null && web.ID != record.WebId))
                {
                    web = site.OpenWeb(record.WebId);
                }
                IAveTenant tenant = factory.CreateTenantCompatibleGeo(bposInfo, site.Url);
                var siteProperties = tenant.GetSitePropertiesByUrl(site.Url);
                if (SPSettingsUtility.NeedUpdateContainer(web, termId))
                {
                    SPSettingsUtility.ConfigBCSProperty(siteProperties, web.Site.Url, web, termId);
                    successRecords.Add(record);
                    logger.Info($"reclassify success for site: {record?.Id}");
                }
                else
                {
                    logger.Info($"Skip reclassify for site: {record?.Id}");
                }
            }
            catch (Exception e)
            {
                logger.Info($"reclassify site failed {record?.Id}:{e}");
                failedIds.Add(record.Id);
            }
        }

        private void ReclassifySiteCollection(IAveSite site, Guid termId, AveObjectModelFactory factory, AveBPOSAccountInfo bposInfo, List<Guid> failedIds, List<Record> successRecords, Record record)
        {
            try
            {
                IAveTenant tenant = factory.CreateTenantCompatibleGeo(bposInfo, record.DirPath);
                var siteProperties = tenant.GetSitePropertiesByUrl(record.DirPath);
                if (SPSettingsUtility.NeedUpdateContainer(site.RootWeb, termId))
                {
                    SPSettingsUtility.ConfigBCSProperty(siteProperties, site.Url, site.RootWeb, termId);
                    successRecords.Add(record);
                    logger.Info($"reclassify success for site collection: {record?.Id}");
                }
                else
                {
                    logger.Info($"Skip reclassify for site collection: {record?.Id}");
                }
            }
            catch (Exception e)
            {
                logger.Info($"reclassify site collection failed {record?.Id}:{e}");
                failedIds.Add(record.Id);
            }
        }

        private void UpdateRemoveItem(Record removeRecordInDB)
        {
            try
            {
                if (removeRecordInDB != null)
                {
                    logger.Info("Catch item not found error, remove it from explorer.");
                    if (removeRecordInDB.RecordStatus == (int)Contract.Explorer.RMRecordStatus.Active || removeRecordInDB.RecordStatus == (int)Contract.Explorer.RMRecordStatus.TrainingManualSync)
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

        private void UpdateArchivedItem(Record archivedRecordInDB)
        {
            try
            {
                if (archivedRecordInDB == null)
                {
                    logger.Warn("record is null");
                    return;
                }

                if (archivedRecordInDB.RecordStatus != (int)Contract.Explorer.RMRecordStatus.RMDeleted)
                {
                    ExplorerDao.UpdateRecordState(archivedRecordInDB, (int)Contract.Explorer.RMRecordStatus.RMDeleted);
                    logger.Info("item is archived in SharePoint, update record state to 3. siteId: {0}, Unique ID: {1}, itemId: {2}", archivedRecordInDB.ScopeId, archivedRecordInDB.RecordsId, archivedRecordInDB.ItemRowId);
                }
                else
                {
                    logger.Info("item is archived in SharePoint and record state is already 3. siteId: {0}, Unique ID: {1}, itemId: {2}", archivedRecordInDB.ScopeId, archivedRecordInDB.RecordsId, archivedRecordInDB.ItemRowId);
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

        protected string getRealException(Exception e)
        {
            if(e == null)
            {
                return null;
            }
            if(e is System.Reflection.TargetInvocationException && e.InnerException != null)
            {
                return getRealException(e.InnerException);
            }
            return e.Message;
        }

        private bool InSameTermScope(Guid termId, IAveTaxonomyField field)
        {
            try
            {
                if (field.AnchorId == Guid.Empty)
                {
                    //term scope is termset
                    var sourceTermSet = currentAveSite.AveSPTaxonomySession.GetTerm(termId).TermSet;
                    return sourceTermSet.ID.Equals(field.TermSetId) ? true : false;
                }
                else
                {
                    //term scope is term
                    var destinationTerm = currentAveSite.AveSPTaxonomySession.GetTerm(field.AnchorId);
                    if (destinationTerm == null)
                    {
                        return false;
                    }
                    //check if in the same termset
                    var sourceTerm = currentAveSite.AveSPTaxonomySession.GetTerm(termId);
                    if (!destinationTerm.TermSet.ID.Equals(sourceTerm.TermSet.ID))
                    {
                        return false;
                    }

                    //check path of term
                    return sourceTerm.PathOfTerm.StartsWith(destinationTerm.PathOfTerm + ";") ? true : false;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while checking same term group. Error{e.ToString()}");
            }
            return false;
        }
        private IAveTaxonomyField GetBCSField(IAveList list, string columnName)
        {
            IAveTaxonomyField taxField = null;
            var tempField = list.Fields.GetRecordTaxonomyField(columnName);
            if (tempField != null)
            {
                taxField = tempField;
            }
            else
            {
                var bcsColumn = list.Fields.GetFieldById(RevIMClassificationColumnID, false);
                if (bcsColumn != null)
                {
                    taxField = bcsColumn as IAveTaxonomyField;
                }
            }
            return taxField;
        }

        #region Wrapper update terms
        [Obsolete]
        public void UpdateTerm(IAveListItem item, IAveTaxonomyField taxField, string termName, Guid termId)
        {

            IAveTaxonomyFieldValue taxValue = taxField.TaxonomyFieldValue;
            taxValue.TermGuid = termId.ToString();
            taxValue.Label = termName;
            item[taxField.ID] = taxValue;
            item[taxField.TextField] = taxValue.ToString();
            item.SystemUpdateForRecords();
        }
        #endregion

        private async Task<(List<Guid> successIds, List<Guid> failedIds)> AddRecordLabelToItem(IAveSite site, List<Record> records, IAveORecords IRecords)
        {
            List<Guid> successIds = new List<Guid>();
            List<Guid> failedIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            AveComplianceTagInfo sharePointRetentionLabel;
            try
            {
                try
                {
                    var availableTags = site.GetAvailableTagsForSite();
                    sharePointRetentionLabel = availableTags.FirstOrDefault(_ => _.TagName.Equals(GeneralRetentionLabel, StringComparison.OrdinalIgnoreCase));
                    if(sharePointRetentionLabel == null)
                    {
                        throw new Exception($"Can not find {GeneralRetentionLabel} in current site {site?.Url}");
                    }
                    if(!(sharePointRetentionLabel.BlockDelete && sharePointRetentionLabel.BlockEdit))
                    {
                        throw new Exception("StorageOptimization_SOARCurrentLabelIsNotRecordLabel");
                    }
                }
                catch (Exception e)
                {
                    failedIds = records != null ? records.Select(r => r.Id).ToList() : failedIds;
                    logger.Error($"Error occurred while get site retention label. Site Url:{site?.Url} Error:{e.ToString()}");
                    if (mNeedSendReport)
                    {
                        ArgumentNullException.ThrowIfNull(records);
                        foreach (var record in records)
                        {
                            AddRecordLabelDetailForGlobalSearchJob(record, JobDetailsStatus.Failed, e.Message, true, record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                        }
                    }
                    throw;
                }
                foreach (var record in records)
                {
                    logger.Info($"Add record label to file {record.Id}");
                    try
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
                        if (CheckisRecord(item))
                        {
                           IRecords.UndeclareItemAsRecord(item);
                        }
                        item.LockRecordItem();
                        item.SetComplianceTag(sharePointRetentionLabel.TagName, true, true, false, false);
                        successIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddRecordLabelDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, true);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Add record label to items failed {0},{1}", WebUtil.MakeFullUrl(site.Url, record.DirPath), e.ToString());
                        failedIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddRecordLabelDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, e.Message, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Add record label to item failed {0}", e.ToString());
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }

            return (successIds, failedIds);
        }

        private (List<Guid> successIds, List<Guid> failedIds) RemoveRecordLabel(IAveSite site, List<Record> records)
        {
            List<Guid> successIds = new List<Guid>();
            List<Guid> failedIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            Dictionary<string, AveComplianceTagInfo> sharePointRetentionLabels;
            try
            {
                try
                {
                    var availableTags = site.GetAvailableTagsForSite();
                    sharePointRetentionLabels = availableTags.ToDictionary(_ => _.TagName);
                }
                catch(Exception e)
                {
                    logger.Warn($"Init retention label for site {site?.Url} has errors: {e.Message}");
                    sharePointRetentionLabels = new();
                }

                foreach(var record in records)
                {
                    try
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
                        var retentionLabelNameOfItem = item.GetComplianceTagName();
                        if(sharePointRetentionLabels.TryGetValue(retentionLabelNameOfItem, out var tagInfo))
                        {
                            if(tagInfo.BlockDelete && tagInfo.BlockEdit)
                            {
                                logger.Info($"remove record label of file {WebUtil.MakeFullUrl(site.Url, record.DirPath)}");
                                item.SetComplianceTagOnBulkItems("");
                                successIds.Add(record.Id);
                                if (mNeedSendReport)
                                {
                                    AddRecordLabelDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, false);
                                }
                            }
                        }
                        else
                        {
                            logger.Warn($"Can not find current retention label {retentionLabelNameOfItem} of file in current site {WebUtil.MakeFullUrl(site.Url, record.DirPath)}");
                        }
                        
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Remove record label to items failed {0},{1}", WebUtil.MakeFullUrl(site.Url, record.DirPath), e.ToString());
                        failedIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddRecordLabelDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, e.Message, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
                if (mNeedSendReport)
                {
                    AddRecordLabelDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, ex.Message, false);
                }
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return (successIds, failedIds);
        }

        /// <summary>
        /// Declared SharePoint Records.
        /// </summary>
        /// <param name="records"></param>
        public async Task<(List<Guid>, List<Guid>)> DeclaredRecordAsync(IAveORecords IRecords, IAveSite site, List<Record> records)
        {
            List<Guid> successIds = new List<Guid>();
            List<Guid> failedIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            try
            {
                try
                {
                    if (!site.CheckDeclarationSettingIsBlockEditAndDelete() && !site.IsOD4BSite())
                    {
                        //all records in one site
                        //for performance, check site once.
                        var testRecord = records.First();
                        //var remoteSite = mDocAveClient.GetRemoteSiteCollectionsByIdList(new List<string> { testRecord.AveSiteId }).FirstOrDefault();
                        var remoteSite = RABrowserClient.GetRemoteSiteCollectionsByIdList(new List<string> { testRecord.AveSiteId }).FirstOrDefault();
                        var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
                        var factory = MultiAppUtil.CreateAveObjectModelFactory(site.Url, bposInfo, AveContextKind.ClientObjectModel);
                        IAveSiteProperties siteProperties = null;
                        try
                        {
                            IAveTenant tenant = factory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, site.Url));
                            siteProperties = tenant.GetSitePropertiesByUrl(site.Url);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Init site properties failed {site.Url}:{e}");
                        }
                        site.EnsureWebDeclarationSetting();
                    }
                }
                catch (Exception e)
                {
                    failedIds = records != null ? records.Select(r => r.Id).ToList() : failedIds;
                    logger.Error($"Error occurred while DisableDenyAddAndCustomizePages. Site Url:{site?.Url} Error:{e.ToString()}");
                    if (mNeedSendReport)
                    {
                        ArgumentNullException.ThrowIfNull(records);
                        foreach (var record in records)
                        {
                            AddDeclareDetailForGlobalSearch(record, JobDetailsStatus.Failed, e.Message, true, record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                        }
                    }
                    throw;
                }

                //TODO  --ywhe order by path could help?
                foreach (var record in records)
                {
                    logger.Info("Declared Records {0}", record.Id);
                    try
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
                        if (!SPSettingsUtility.IsBlockEditAndDeleteRecord(item))
                        {
                            if (item.FieldValues.ContainsKey("CheckoutUser") &&
                                item.FieldValues["CheckoutUser"] != null && !string.IsNullOrEmpty(item.FieldValues["CheckoutUser"].ToString()))
                            {
                                logger.Warn("The file is in Checked out status, cannot be declared now. File UniqueId: {0} RowId:{1}", item.UniqueId, item.ID);
                                failedIds.Add(record.Id);
                                if (mNeedSendReport)
                                {
                                    AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, "RM_JM_GlobalSearch_DeclareCheckout", true);
                                }
                            }
                            else
                            {
                                if (CheckisRecord(item))
                                {
                                    IRecords.UndeclareItemAsRecord(item);
                                }
                                var lockerKey = web.Site.ID.ToString();
                                bool lockStatus = false;
                                try
                                {
                                    lockStatus = await RMGlobalLocker.GetRecordsLockerAsync(lockerKey);
                                    site.EnsureWebDeclarationSetting();
                                    IRecords.DeclareItemAsRecord(item);
                                    successIds.Add(record.Id);
                                    if (mNeedSendReport)
                                    {
                                        AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failedIds.Add(record.Id);
                                    if (mNeedSendReport)
                                    {
                                        AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, ex.Message, true);
                                    }
                                    logger.Error("error occurred while process items,ERROR:{0}", ex.ToString());
                                }
                                finally
                                {
                                    if (lockStatus && !string.IsNullOrEmpty(lockerKey))
                                    {
                                        await RMGlobalLocker.ReleaseRecordsLockerAsync(lockerKey);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Declared Records failed {0},{1}", WebUtil.MakeFullUrl(site.Url, record.DirPath), e.ToString());
                        failedIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, e.Message, true);
                        }
                    }
                }//for each end
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
                //if (mNeedSendReport)
                //{
                //    AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, ex.Message, true);
                //}
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return (successIds, failedIds);
        }

        /// <summary>
        /// UnDeclared SharePoint Records
        /// </summary>
        /// <param name="records"></param>
        /// <param name="failedIds"></param>
        /// <returns></returns>
        public List<Guid> UnDeclaredRecord(IAveORecords IRecords, IAveSite site, List<Record> records, ref List<Guid> failedIds)
        {
            List<Guid> successIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            try
            {
                //TODO  --ywhe order by path could help?
                foreach (var record in records)
                {
                    logger.Info("UnDeclared Records {0}", record.Id);
                    IAveListItem item = null;
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {  //memory leak?
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                        }
                        item = list.GetItemByUniqueId(record.ItemId);
                        if (CheckisRecord(item))
                        {
                            IRecords.UndeclareItemAsRecord(item);
                            successIds.Add(record.Id);
                            if (mNeedSendReport)
                            {
                                AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, false);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Declared Records failed {0},{1}", record.FullPath, e.ToString());
                        failedIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, e.Message, false);
                        }
                    }
                }
                //CollectionDataDao.UpdateUnDeclaredRecords(successIds);
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
                if (mNeedSendReport)
                {
                    AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, ex.Message, false);
                }
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return successIds;
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
        //public IAveListItem GetItem(RMBaseRecord record)//replace to client api
        //{
        //    if (currentAveWeb == null || (currentAveWeb != null && currentAveWeb.ID != record.WebId))
        //    {
        //        currentAveWeb = currentAveSite.OpenWeb(record.WebId);
        //    }
        //    if (currentAveList == null || (currentAveList != null && currentAveList.ID != record.ListId))
        //    {
        //        currentAveList = currentAveWeb.GetList(record.ListId);
        //    }
        //    IAveListItem item = currentAveList.GetItemByUniqueId(record.ItemId);
        //    return item;
        //}

        public RemoteSiteCollection GetSiteNode(string fullPath)
        {
            //return mDocAveClient.GetSiteNode(fullPath);
            return RABrowserClient.GetSiteNode(fullPath);
        }
        public RemoteSiteCollection GetSiteNode(Guid aveId)
        {
            //return mDocAveClient.GetSiteNode(aveId);
            return RABrowserClient.GetSiteNode(aveId);
        }
        //private IAveSite InitCurrentSite(RemoteSiteCollection site)
        //{
        //    SharePointSettingUtility SPUtility = new SharePointSettingUtility();
        //    var bposInfo = PoolUserUtil.GetBPOSInfo(site);

        //    aveObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
        //    currentAveSite = aveObjectModelFactory.CreateSite();
        //    return currentAveSite;
        //}
        //private IAveORecords Record
        //{
        //    get
        //    {
        //        if (mRecord == null)
        //        {
        //            mRecord = aveObjectModelFactory.CreateRecords();
        //        }
        //        return mRecord;
        //    }
        //}
        //public ClientContext InitContext(string siteUrl)
        //{
        //    siteCollection = GetSiteNode(siteUrl);//from cache
        //    columnName = GetBcsColumnName(siteCollection);
        //    CommonClientContext clientContext = new CommonClientContext();
        //    ClientContext context = clientContext.InitClientContext(siteCollection);
        //    currentSite = context.Site;
        //    return context;
        //}
        //public ClientContext InitContext(Guid siteId)
        //{
        //    siteCollection = GetSiteNode(siteId);//from cache
        //    columnName = GetBcsColumnName(siteCollection);
        //    CommonClientContext clientContext = new CommonClientContext();
        //    ClientContext context = clientContext.InitClientContext(siteCollection);
        //    currentSite = context.Site;
        //    return context;
        //}
        //private ClientContext InitContext(RemoteSiteCollection site)
        //{
        //    var startTime = DateTime.Now;
        //    siteCollection = site;
        //    columnName = GetBcsColumnName(siteCollection);
        //    logger.Info("column name:{0}, parentId:{1}", columnName, site.parentId);
        //    logger.Warn($"[Change Term] 3.1. time elapsed for initContext(GetBcsColumnName)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    CommonClientContext clientContext = new CommonClientContext();
        //    currentContext = clientContext.InitClientContext(siteCollection);
        //    logger.Warn($"[Change Term] 3.2. time elapsed for initContext(InitClientContext)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    currentSite = currentContext.Site;
        //    taxonomySession = TaxonomySession.GetTaxonomySession(currentContext);
        //    logger.Warn($"[Change Term] 3.3. time elapsed for initContext(GetTaxonomySession)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    currentContext.Load(currentSite);
        //    logger.Warn($"[Change Term] 3.4. time elapsed for initContext(load)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    currentContext.ExecuteQuery();
        //    logger.Warn($"[Change Term] 3.5. time elapsed for initContext(ExecuteQuery)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    currentWeb = null;//reset current web
        //    return currentContext;
        //}
        #region client context update method
        public void UpdateTerm(ListItem item, string ternName, Guid termId)
        {
            Field textField = null;
            TaxonomyField taxField = GetTaxonomyField(item.ParentList, ref textField);
            Guid termScopeId = taxField.AnchorId;
            var valueTerm = taxonomySession.GetTerm(termId);
            currentContext.Load(valueTerm, t => t.PathOfTerm);
            if (termScopeId != Guid.Empty)
            {
                var scopeTerm = taxonomySession.GetTerm(termScopeId);
                currentContext.Load(scopeTerm, t => t.PathOfTerm);
                currentContext.ExecuteQuery();
                if (!valueTerm.PathOfTerm.StartsWith(scopeTerm.PathOfTerm + ";"))
                {
                    logger.Warn("Scope{0} : TermValue{1}", scopeTerm.PathOfTerm, valueTerm.PathOfTerm);
                    throw new Exception("Term in invalidate scope");
                }
            }
            else
            {
                var termSetId = taxField.TermSetId;
                currentContext.Load(valueTerm, t => t.TermSet);
                currentContext.ExecuteQuery();
                if (valueTerm.TermSet.Id != termSetId)
                {
                    logger.Warn("Scope termSet {0} : Term Set Value{1}", termSetId, valueTerm.TermSet.Id);
                    throw new Exception("Term in invalidate scope");
                }
            }
            taxField.ValidateSetValue(item, ternName + "|" + termId);
            //var textFieldName = textField.InternalName;
            //item[taxField.InternalName] = ternName + "|" + termId.ToString();
            //item[textFieldName] = ternName + "|" + termId.ToString();
            ////这个方式是最新版的client dll才包含的，注意测试local站点是否好使
            ////item.SystemUpdate();
            //item.Update();
            item.SystemUpdate();
            currentContext.ExecuteQuery();
        }
        public TaxonomyField GetTaxonomyField(List list, ref Field textField)
        {
            currentContext.Load(list, l => l.Fields);
            currentContext.ExecuteQuery();
            var field = list.Fields.GetByTitle(columnName);
            currentContext.Load(field);
            currentContext.ExecuteQuery();
            TaxonomyField taxField = currentContext.CastTo<TaxonomyField>(field);
            //TaxonomyField taxField = field as TaxonomyField;
            currentContext.Load(taxField);
            currentContext.ExecuteQuery();

            textField = list.Fields.GetById(taxField.TextField);
            currentContext.Load(textField);
            currentContext.ExecuteQuery();
            return taxField;
        }

        public ListItem GetListItem(Record record)//replace to client api
        {
            if (currentWeb == null || currentWeb.Id != record.WebId)
            {
                currentWeb = currentContext.Site.OpenWebById(record.WebId);
                currentContext.Load(currentWeb, w => w.Lists, w => w.Id);
                currentContext.ExecuteQuery();
            }
            if (currentList == null || currentList.Id != record.ListId)
            {
                currentList = currentWeb.Lists.Where(l => l.Id == record.ListId).FirstOrDefault();
            }
            currentContext.Load(currentList);
            currentContext.ExecuteQuery();
            ListItem item = currentList?.GetItemById(record.ItemRowId);
            if(item  != null)
            {
                currentContext.Load(item);
                currentContext.Load(item.File);
                currentContext.ExecuteQuery();
            }
            return item;
        }



        #endregion

        #region Check Move And Move Rule Location Path
        public async Task<CheckLocationObject> ValidationDestUrlForRAAsync(string url)
        {
            CheckLocationObject checkObject = new CheckLocationObject();
            bool isLibraryInRA = true;
            url = HttpUtility.UrlDecode(url);
            try
            {
                logger.Info("Start check location url for ra.");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                int listTemplate = 0;
                var siteCollectionUrl = GetSiteCollectionUrlFromListUrl(url);
                RemoteSiteCollection site = RemoteNodeDao.GetRemoteSiteCollectionByUrl(siteCollectionUrl);
                logger.Info($"Site is null: {site == null}");
                checkObject.ContainerId = site.parentId;
                Guid teamsContainerId = Guid.Empty;
                bool isTeamsNode = false;
                if(KeyValueDao.HasUpgradeTeams() && !site.TeamId.IsNullOrEmpty())
                {
                    var (teamsNode, listSiteNode) = RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(site.TeamId);
                    logger.Info($"teams is null: {teamsNode == null}");
                    if (teamsNode == null) return null;
                    checkObject.ContainerId = teamsNode.parentId;
                    teamsContainerId = new Guid(teamsNode.parentId);
                    isTeamsNode = true;
                }
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                logger.Info($"Account is null: {account == null}");
                if (!(await IsAdminAsync(account.UserId, site.NodeType)))
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(isTeamsNode ? teamsContainerId : new Guid(site.parentId), userAndGroupUserIds))
                    {
                        logger.Info($"Current user doesn't have permission on container. Container Id:{site.parentId}.DesUrl:{url}.");
                        return null;
                    }
                }
                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(site);
                var mFactory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                logger.Info($"Factory is null: {mFactory == null}");
                using (IAveSite mSite = mFactory.CreateSite(site.url))
                {
                    var webUrls = GetWebServerRelativeUrl(mSite, url);
                    foreach (var webUrl in webUrls)
                    {
                        var (succeed, isLibraryInRAAfterValidating) = ValidateWebUrl(mSite, checkObject, webUrl, url, bposInfo, site.id);
                        isLibraryInRA = isLibraryInRAAfterValidating;
                        if(succeed)
                        {
                            break;
                        }
                    }
                }
                watch.Stop();
                logger.Info("End check location url for ra,Take Milliseconds:{0} ms.", watch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                isLibraryInRA = false;
                logger.Info("Failed check location url for ra, [{0}],error message:{1}", url, ex.Message);
            }
            if (!isLibraryInRA)
            {
                checkObject = null;
            }
            return checkObject;
        }

        private (bool, bool) ValidateWebUrl(IAveSite mSite, CheckLocationObject checkObject, string webUrl, string url, AveBPOSAccountInfo bposInfo, string siteId)
        {
            try
            {
                using IAveWeb web = mSite.OpenWeb(webUrl);
                logger.Info($"web is null:{web == null}. web url:{webUrl}");
                IAveList list = null;
                if (url.Contains("#/"))
                {
                    list = web.GetListFromUrl(url.Substring(url.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                }
                else
                {
                    list = web.GetList(url);
                }

                logger.Info($"list is null:{list == null}.");
                if (!url.TrimEnd('/').EndsWith(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    if (!url.TrimEnd('/').EndsWith(list.DefaultViewUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info($"Current url [{url}] does not end with list relative url");
                        return (false, false);
                    }
                }

                var listTemplate = Convert.ToInt32(list.BaseTemplate);
                //判断是不是library
                if (listTemplate == 101 || listTemplate == 1302 || listTemplate == 700)
                {
                    logger.Info("This is a library, List Template is [{0}], List path is {1}", listTemplate, url);
                    checkObject.DestRootPath = url;
                    checkObject.AveSiteId = new Guid(siteId);
                }
                else
                {
                    logger.Info("This is not a library, List Template is [{0}], List path is {1}", listTemplate, url);
                    return (true, false);
                }

                checkObject.UserInfoName = bposInfo.UserName;
                if (!bposInfo.Password.IsNullOrEmpty())
                {
                    checkObject.UserInfoKey =
                        Convert.ToBase64String(CspCommunicationWrapper.WrapKey(bposInfo.Password));
                }
                return (true, true);
            }
            catch (Exception ex)
            {
                logger.Info("Failed validating location url for ra, [{0}],error message:{1}", url, ex.Message);
                return (false, false);
            }
        }

        private async Task<bool> IsAdminAsync(string userId, RemoveNodeType nodeType)
        {
            bool isAdmin = false;
            if (nodeType == RemoveNodeType.SkyDrivePro)
            {
                isAdmin = await IsOneDriveAdminAsync(userId) || await IsSOOneDriveAdminAsync(userId);
            }
            else
            {
                isAdmin = await IsSPAdminAsync(userId) || await IsSOSPAdminAsync(userId);
            }
            return isAdmin;
        }
        private Task<bool> IsSPAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () => {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
                });
        }
        private Task<bool> IsOneDriveAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () => {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveAdmin);
                });
        }

        private Task<bool> IsSOSPAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () => {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
                });
        }
        private Task<bool> IsSOOneDriveAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () => {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.OneDriveAdmin);
                });
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByListUrl(string listUrl)
        {
            //var client = new DAOAPIClientV1();
            //return mDocAveClient.GetRemoteSiteCollectionByListUrl(listUrl);
            return RABrowserClient.GetRemoteSiteCollectionByListUrl(listUrl);
        }
        public List<string> GetWebServerRelativeUrl(IAveSite site, string listUrl)
        {
            return site.GetListWebServerRelativeUrl(listUrl);
        }

        private string GetSiteCollectionUrlFromListUrl(string url)
        {
            var uri = new Uri(url);
            var siteCollectionUrl = string.Empty;
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            string[] siteTypes = { "sites", "personal", "teams" };
            int idx = Array.FindIndex(segments, s => siteTypes.Contains(s, StringComparer.OrdinalIgnoreCase));

            if (idx >= 0 && idx < segments.Length - 1)
            {
                var sitePath = "/" + string.Join("/", segments.Take(idx + 2));
                siteCollectionUrl = $"{uri.Scheme}://{uri.Host}{sitePath}";
            }
            else
            {
                siteCollectionUrl = $"{uri.Scheme}://{uri.Host}";
            }
            logger.Info($"The parsed site URL is [{siteCollectionUrl}], the parameter is [{url}]");
            return siteCollectionUrl;
        }
        #endregion

        #region Check SPO Location Lib or Folder Path

        public async Task<DestinationSPOLocationInfo> ValidationDestUrlForRestore(string url, bool isSupportSiteLevel = false)
        {
            DestinationSPOLocationInfo checkObject = new DestinationSPOLocationInfo();
            checkObject.FullUrl = url;
            bool isLibraryInRA = true;
            url = HttpUtility.UrlDecode(url);
            try
            {
                logger.Info("Start check location url for restore.");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var siteCollectionUrl = GetSiteCollectionUrlFromListUrl1(url);
                RemoteSiteCollection site = RemoteNodeDao.GetRemoteSiteCollectionByUrl(siteCollectionUrl);
                logger.Info($"Site is null: {site == null}");
                if (site == null) return null;
                checkObject.ContainerId = site.parentId;
                checkObject.TenantId = site.TenantId;
                Guid teamsContainerId = Guid.Empty;
                bool isTeamsNode = false;
                if (KeyValueDao.HasUpgradeTeams() && !site.TeamId.IsNullOrEmpty())
                {
                    var (teamsNode, _) = RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(site.TeamId);
                    logger.Info($"teams is null: {teamsNode == null}");
                    if (teamsNode == null) return null;
                    checkObject.ContainerId = teamsNode.parentId;
                    checkObject.TenantId = teamsNode.TenantId;
                    teamsContainerId = new Guid(teamsNode.parentId);
                    isTeamsNode = true;
                }
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                logger.Info($"Account is null: {account == null}");
                if (account != null && !(await IsAdminAsync(account.UserId, site.NodeType)))
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(isTeamsNode ? teamsContainerId : new Guid(site.parentId), userAndGroupUserIds))
                    {
                        logger.Info($"Current user doesn't have permission on container. Container Id:{site.parentId}.DesUrl:{url}.");
                        return null;
                    }
                }
                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(site);
                var mFactory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                logger.Info($"Factory is null: {mFactory == null}");
                if (mFactory == null) return null;
                using var _ = new AvePoint.RA.RACommonUtility.Common.SiteStateTransitionScopeUtility(siteCollectionUrl, SiteState.ReadOnly, true);
                using (IAveSite mSite = mFactory.CreateSite(site.url))
                {
                    checkObject.SiteCollectionUrl = site.url;
                    var webUrls = GetWebServerRelativeUrl(mSite, url);

                    if (webUrls == null || webUrls.Count == 0)
                    {
                        if (isSupportSiteLevel)
                        {
                            return checkObject;
                        }
                        logger.Info($"Cannot find any web url for the list url: {url}");
                        return null;
                    }

                    foreach (var webUrl in webUrls)
                    {
                        var (succeed, isLibraryInRAAfterValidating) = ValidateWebUrlForRestore(mSite, checkObject, webUrl, url, bposInfo, site.id, isSupportSiteLevel);
                        isLibraryInRA = isLibraryInRAAfterValidating;
                        if (succeed)
                        {
                            checkObject.WebPath = webUrl;
                            break;
                        }
                    }
                }
                watch.Stop();
                logger.Info("End check location url for ra,Take Milliseconds:{0} ms.", watch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                isLibraryInRA = false;
                logger.Info("Failed check location url for ra, [{0}],error message:{1}", url, ex.Message);
            }
            if (!isLibraryInRA)
            {
                checkObject = null;
            }
            return checkObject;
        }

        public string GetSiteCollectionUrlFromListUrl1(string url)
        {
            var uri = new Uri(url);
            var siteCollectionUrl = string.Empty;
            //var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            string pathForParsing = HttpUtility.UrlDecode(uri.AbsolutePath);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            var idParam = queryParams["id"];

            if (!string.IsNullOrEmpty(idParam) && idParam.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                pathForParsing = idParam;
            }

            var segments = pathForParsing.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string[] siteTypes = { "sites", "personal", "teams" };
            int idx = Array.FindIndex(segments, s => siteTypes.Contains(s, StringComparer.OrdinalIgnoreCase));

            if (idx >= 0 && idx < segments.Length - 1)
            {
                var sitePath = "/" + string.Join("/", segments.Take(idx + 2));
                siteCollectionUrl = $"{uri.Scheme}://{uri.Host}{sitePath}";
            }
            else
            {
                siteCollectionUrl = $"{uri.Scheme}://{uri.Host}";
            }
            logger.Info($"The parsed site URL is [{siteCollectionUrl}], the parameter is [{url}]");
            return siteCollectionUrl;
        }

        private (bool isSuccess, bool isValidDestination) ValidateWebUrlForRestore(IAveSite mSite, DestinationSPOLocationInfo checkObject, string webUrl, string rawUrl, AveBPOSAccountInfo bposInfo, string siteId, bool isSupportSiteLevel = false)
        {
            try
            {
                using IAveWeb web = mSite.OpenWeb(webUrl);
                logger.Info($"[Folder Validate] Opened web. webUrl: {webUrl}");

                string targetServerRelativePath = NormalizeTargetServerRelativePath(
                    ExtractTargetServerRelativePath(rawUrl),
                    web.ServerRelativeUrl);
                logger.Info($"Parsed target path: {targetServerRelativePath}");

                string cleanWebPath = web.ServerRelativeUrl.TrimEnd('/');
                string cleanTargetPathForWebCheck = targetServerRelativePath.TrimEnd('/');

                // The provided URL points to the web (sub-site) itself, not a library/folder.
                if (isSupportSiteLevel && cleanTargetPathForWebCheck.Equals(cleanWebPath, StringComparison.OrdinalIgnoreCase))
                {
                    checkObject.WebPath = web.ServerRelativeUrl;
                    checkObject.IsRootWeb = web.IsRootWeb;
                    checkObject.WebName = web.IsRootWeb ? "." : web.Name;
                    checkObject.FullPath = WebUtil.MakeFullUrl(mSite.Url, targetServerRelativePath);
                    logger.Info($"[Site Validate] Target is a sub-site. WebPath: {checkObject.WebPath}");
                    return (true, true);
                }

                IAveList list = null;
                if (rawUrl.Contains("#/"))
                {
                    list = web.GetListFromUrl(rawUrl.Substring(rawUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                }
                else
                {
                    list = web.GetList(targetServerRelativePath);
                }

                if (list == null)
                {
                    logger.Info($"[Folder Validate] Cannot find list from url: {targetServerRelativePath}");
                    return (false, false);
                }

                var listTemplate = Convert.ToInt32(list.BaseTemplate);
                if (listTemplate != 101 && listTemplate != 1302 && listTemplate != 700 && listTemplate != 109)
                {
                    logger.Info($"[Folder Validate] Not a supported library. List Template: {listTemplate}");
                    return (true, false);
                }

                string rootFolderUrl = list.RootFolder.ServerRelativeUrl.TrimEnd('/');
                checkObject.ListPath = rootFolderUrl;
                string cleanTargetPath = targetServerRelativePath.TrimEnd('/');

                if (cleanTargetPath.Equals(rootFolderUrl, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info("Target is Root Library. Path: {0}", rootFolderUrl);
                    //checkObject.FolderPath = rootFolderUrl;
                }
                else if (cleanTargetPath.StartsWith(rootFolderUrl + "/", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var folder = web.GetFolder(cleanTargetPath);
                        if (folder == null || !folder.Exists)
                        {
                            logger.Info("Folder does not exist at path: {0}", cleanTargetPath);
                            return (false, false);
                        }
                        logger.Info("Target is Sub-Folder. Path: {0}", cleanTargetPath);
                        checkObject.FolderPath = folder.ServerRelativeUrl;
                        checkObject.FolderName = folder.Name;
                        //checkObject.DestRootPath = cleanTargetPath;
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Error verifying folder existence. Path: {0}, Error: {1}", cleanTargetPath, ex.Message);
                        return (false, false);
                    }
                }
                else
                {
                    logger.Info($"Current target [{cleanTargetPath}] does not match root folder path.");
                    return (false, false);
                }

                //checkObject.AveSiteId = new Guid(siteId);
                //checkObject.UserInfoName = bposInfo.UserName;
                //if (!bposInfo.Password.IsNullOrEmpty())
                //{
                //    checkObject.UserInfoKey = Convert.ToBase64String(CspCommunicationWrapper.WrapKey(bposInfo.Password));
                //}
                checkObject.IsRootWeb = web.IsRootWeb;
                checkObject.WebName = web.IsRootWeb ? "." : web.Name;
                checkObject.ListName = list.Title;
                checkObject.FullPath = WebUtil.MakeFullUrl(mSite.Url, targetServerRelativePath);

                return (true, true);
            }
            catch (Exception ex)
            {
                logger.Error($"[Folder Validate] Failed validating location url: {rawUrl}. Error: {ex.Message}");
                return (false, false);
            }
        }

        private string ExtractTargetServerRelativePath(string rawUrl)
        {
            string decodedUrl = HttpUtility.UrlDecode(rawUrl);

            Uri uri;
            if (!Uri.TryCreate(decodedUrl, UriKind.Absolute, out uri))
            {
                throw new Exception($"Invalid URL format: {rawUrl}");
            }

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(queryParams["id"]))
            {
                logger.Info($"URL contains 'id' query parameter, it should be a folder url. Extracting path from 'id': {queryParams["id"]}");
                return queryParams["id"];
            }

            string path = HttpUtility.UrlDecode(uri.AbsolutePath);

            int formsIndex = path.IndexOf("/Forms/", StringComparison.OrdinalIgnoreCase);
            if (formsIndex > 0 && path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, formsIndex);
            }

            return path;
        }

        private string NormalizeTargetServerRelativePath(string targetPath, string webServerRelativeUrl)
        {
            string normalizedTargetPath = HttpUtility.UrlDecode(targetPath);
            if (Uri.TryCreate(normalizedTargetPath, UriKind.Absolute, out var targetUri))
            {
                normalizedTargetPath = targetUri.AbsolutePath;
            }

            normalizedTargetPath = "/" + normalizedTargetPath.Trim('/');
            string normalizedWebPath = webServerRelativeUrl.TrimEnd('/');
            if (normalizedWebPath.Length > 0
                && !normalizedTargetPath.Equals(normalizedWebPath, StringComparison.OrdinalIgnoreCase)
                && !normalizedTargetPath.StartsWith(normalizedWebPath + "/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedTargetPath = normalizedWebPath + normalizedTargetPath;
            }

            return HttpUtility.UrlDecode(normalizedTargetPath);
        }

        #endregion
    }
}
