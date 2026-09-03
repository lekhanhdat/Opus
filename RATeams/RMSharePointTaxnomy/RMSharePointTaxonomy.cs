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

using System.Text.RegularExpressions;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

namespace RATeams.RMSharePointTaxnomy
{

    public class RMSharePointTaxonomy : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMSharePointTaxonomy));
        #region private property
        private ClientContext mClientcontext { get; set; }
        private TermStore mTermStore { get; set; }
        private TaxonomySession mTaxonomySession { get; set; }
        private int mLcid;
        private List<RMTermSet> mAllTermSet { get; set; }
        private RMTermGroup mRmTermGroup { get; set; }
        private List<RMTerm> mAllTerm { get; set; }
        private List<Guid> mSyncTermStoreGuids = new List<Guid>();
        private List<Guid> mOrphanTermStoreGuids = new List<Guid>();
        private List<Guid> mRetiredTermStoreGuids = new List<Guid>();
        private string mTermStoreName = string.Empty;
        private ITenantInfoDao TenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();
        private readonly bool isCsdTenant;
        #endregion

        #region dao       
        private IJobMonitorService mJobMonitorService { get; set; }
        public IJobMonitorService JobService
        {
            get
            {
                if (mJobMonitorService == null)
                {
                    mJobMonitorService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                }
                return mJobMonitorService;
            }
        }
        private ITenantService mTenantService { get; set; }
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
        private ITermDao mTermDAO { get; set; }
        public ITermDao TermDAO
        {
            get
            {
                if (mTermDAO == null)
                {
                    mTermDAO = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDAO;
            }
        }
        private ITermSetDao mTermSetDAO { get; set; }
        public ITermSetDao TermSetDAO
        {
            get
            {
                if (mTermSetDAO == null)
                {
                    mTermSetDAO = (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
                }
                return mTermSetDAO;
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

        private Task<GeneralSettingModel> mGeneralSetting = null;
        private Task<GeneralSettingModel> GeneralSetting
        {
            get
            {
                if (mGeneralSetting == null)
                {
                    mGeneralSetting = ((IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService))).GetGeneralSettingAsync();
                }
                return mGeneralSetting;
            }
        }
        #endregion


        #region public property
        public bool IsAppAuth { get; set; }
        public string CurrentSiteUrl = string.Empty;
        public string CurrentAdminUrl = string.Empty;
        public List<Guid> NoDeleteTermIds = new List<Guid>();
        public List<Guid> TermStoreIds = new List<Guid>();
        public bool JobHasError;
        public int FinsihCount;
        #endregion

        public RMSharePointTaxonomy(RMTermGroup rmTermGroup)
        {
            mAllTermSet = BuildRMTermSetTreeAsync(TermSetType.BusinessTerm, rmTermGroup.UniqueId).Result;
            mRmTermGroup = rmTermGroup;
            isCsdTenant = TenantInfoDao.IsEnableCSD(TenantLocalValue.LogonGroupId);
        }
        public RMSharePointTaxonomy(List<RMTermSet> allTermSet, RMTermGroup rmTermGroup, List<RMTerm> allTerm)
        {
            mAllTerm = allTerm;
        }
        public RMSharePointTaxonomy(ClientContext context)
        {
            mClientcontext = context;
            mTaxonomySession = TaxonomySession.GetTaxonomySession(context);
            mTermStore = GetDefaultTermStore();
            mClientcontext.Load(mTaxonomySession);
            mClientcontext.ExecuteQuery();
        }

        public RMSharePointTaxonomy()
        {
        }

        public void InitClientContext(RMSPTreeNode node)
        {
            CommonClientContext commonClientContext = new CommonClientContext();
            mClientcontext = commonClientContext.InitClientContext(node);
            mTaxonomySession = TaxonomySession.GetTaxonomySession(mClientcontext);
            mClientcontext.Load(mTaxonomySession);
            mClientcontext.ExecuteQuery();
        }

        public TermSetCollection GetTermSet(string name, int lcid)
        {
            return mTaxonomySession.GetTermSetsByName(name, lcid);
        }

        public TermStoreCollection LoadTermStore()
        {
            mClientcontext.Load(mTaxonomySession, s => s.TermStores);
            mClientcontext.ExecuteQuery();
            return mTaxonomySession.TermStores;
        }

        public void InitClientContext(RemoteSiteCollection site)
        {
            CommonClientContext commonClientContext = new CommonClientContext();
            mClientcontext = commonClientContext.InitClientContext(site);
            IsAppAuth = (site.AuthType == AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken ||
                site.AuthType == AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern);
            if (mClientcontext == null)
            {
                throw new Exception($"Failed to initialize ClientContext for site [{site?.url}]. Site may have been deleted from SharePoint.");
            }
            mTaxonomySession = TaxonomySession.GetTaxonomySession(mClientcontext);
            mClientcontext.Load(mTaxonomySession);
            mClientcontext.ExecuteQuery();
        }


        public TermStore GetDefaultTermStore()
        {
            TermStoreCollection col = LoadTermStore();
            //可能由于一些未知的原因（可能是网络）导致load不到TermStore，所以如果没有load到，就sleep一下，然后retry  RECO-451
            if (col.Count == 0)
            {
                //Thread.Sleep(1000);
                logger.Info("failed to get default term store");
                col = LoadTermStore();
                if (col.Count == 0)
                {
                    return null;
                }
                else
                {
                    mClientcontext.Load(col);
                    return col[0];
                }
            }
            else
            {
                mClientcontext.Load(col);
                return col[0];
            }
        }

        public Guid GetDefaultTermStoreId()
        {
            TermStoreCollection col = LoadTermStore();
            mClientcontext.Load(col);
            Guid termStoreId = col[0].Id;
            //this.m_clientcontext.ExecuteQuery();
            return termStoreId;
        }

        public string GetDefaultTermStoreName()
        {
            TermStoreCollection col = LoadTermStore();
            mClientcontext.Load(col);
            //this.m_clientcontext.ExecuteQuery();
            return col[0].Name;
        }

        public Term GetTermFromMMS(string termId)
        {
            if (mTermStore == null)
            {
                logger.Warn("This site collection do not have default termStore.");
                return null;
            }
            mClientcontext.Load(mTermStore);
            try
            {
                if (!string.IsNullOrEmpty(termId) && new Guid(termId) != Guid.Empty)
                {
                    var term = mTermStore.GetTerm(new Guid(termId));
                    mClientcontext.Load(term);
                    mClientcontext.ExecuteQuery();
                    return term;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Validate term error :{0}", e.ToString());
            }
            return null;
        }
        public bool ValidateTermId(Guid termId)
        {
            if (mTermStore == null)
            {
                logger.Warn("This site collection do not have default termStore.");
                return false;
            }
            try
            {
                if (termId != Guid.Empty)
                {
                    var term = mTermStore.GetTerm(termId);
                    mClientcontext.Load(term);
                    mClientcontext.ExecuteQuery();
                    string termName = term.Name;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Validate term error :{0}", e.ToString());
                return false;
            }
            return true;
        }
        public bool ValidateTermIds(Guid termSetId, string termId = null, string defalutTermId = null)
        {
            if (mTermStore == null)
            {
                logger.Warn("This site collection do not have default termStore.");
                return false;
            }
            mClientcontext.Load(mTermStore);
            try
            {
                var termSet = mTermStore.GetTermSet(termSetId);
                mClientcontext.Load(termSet);
                mClientcontext.ExecuteQuery();
                string termSetName = termSet.Name;
                if (!string.IsNullOrEmpty(termId) && new Guid(termId) != Guid.Empty)
                {
                    var term = mTermStore.GetTerm(new Guid(termId));
                    mClientcontext.Load(term);
                    mClientcontext.ExecuteQuery();
                    string termName = term.Name;
                }
                if (!string.IsNullOrEmpty(defalutTermId) && new Guid(defalutTermId) != Guid.Empty)
                {
                    var defaultTerm = mTermStore.GetTerm(new Guid(defalutTermId));
                    mClientcontext.Load(defaultTerm);
                    mClientcontext.ExecuteQuery();
                    string defaultTermName = defaultTerm.Name;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Validate term error :{0}", e.ToString());
                return false;
            }
            return true;
        }


        public TermGroupCollection LoadGroup(TermStore store)
        {
            mClientcontext.Load(store, s => s.Groups);
            mClientcontext.ExecuteQuery();
            return store.Groups;
        }

        public TermSetCollection LoadTermSet(TermGroup group)
        {
            mClientcontext.Load(group, g => g.TermSets);
            mClientcontext.ExecuteQuery();
            return group.TermSets;
        }

        public TermCollection LoadTerm(TermSet termSet)
        {
            mClientcontext.Load(termSet, ts => ts.Terms);
            mClientcontext.ExecuteQuery();
            return termSet.Terms;

        }

        public List<Guid> GetAllTermStoreIds()
        {
            TermStoreCollection termSC = LoadTermStore();
            List<Guid> ids = new List<Guid>();
            foreach (TermStore termStore in termSC)
            {
                ids.Add(termStore.Id);
            }
            return ids;
        }
        /// <summary>
        /// Sync SharePoint TermSet Tree.
        /// </summary>
        public async System.Threading.Tasks.Task SyncTermToSharePointAsync()
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    TermStoreCollection termSC = LoadTermStore();
                    if (termSC.Count == 0)
                    {
                        AddJobDetail("RM_JS_Common_Pending", "RM_JS_Common_Pending", JobDetailsStatus.Failed, "RM_TS_NoMMS");
                        return;
                    }
                    foreach (TermStore termStore in termSC)
                    {
                        try
                        {
                            if (CheckTermStoreNeedSkip(termSC, termStore))
                            {
                                continue;
                            }

                            LoadBasicInfoForTermStore(termStore);

                            await SyncTermGroupAsync(termStore);
                        }
                        finally
                        {
                            if (!mSyncTermStoreGuids.Contains(termStore.Id))
                            {
                                mSyncTermStoreGuids.Add(termStore.Id);
                            }
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error("Some problems encountered in the process of dealing with sync SharePoint term tree {0}", e.ToString());
                throw;
            }
        }

        private async System.Threading.Tasks.Task SyncTermGroupAsync(TermStore termStore)
        {
            try
            {
                TermGroup termGroup = GetTermGroupIfExist(termStore);
                if (termGroup != null)
                {
                    try
                    {
                        UpdateTermGroup(termGroup);
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"An error occurred while updating term group. Name:{mRmTermGroup.Name} Error:{e.ToString()}");
                        return;
                    }
                    TermSetCollection termSetCollection = LoadTermSet(termGroup);
                    logger.Info("Load group {0}", termGroup.Name);
                    foreach (RMTermSet rmTermSet in mAllTermSet)
                    {
                        await SyncTermSetAsync(rmTermSet, termGroup, termSetCollection);
                    }
                }
                else
                {
                    logger.Info("Need create termgroup {0}", mRmTermGroup.Name);
                    await CreateTermGroupAsync(termStore, mRmTermGroup);
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Warn("Some problems encountered in the process of dealing with this rmtermgroup {0},error {1}", mRmTermGroup.Name, e.ToString());
                AddJobDetail(mRmTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, GetExceptionMessage(e));
            }
        }

        private async Task<List<RMTermSet>> BuildRMTermSetTreeAsync(TermSetType termSetType, Guid termGroupId, bool includeDeletedTerm = true)
        {
            try
            {
                logger.Info("begin BuildRMTermSetTree:{0}", termGroupId);
                List<RMTermSet> allRMTermSet = await TermSetDAO.LoadTermSetWithDeletedItemsAsync(termSetType, termGroupId);
                if (allRMTermSet.Count == 0)
                {
                    logger.Warn("Term set not found,tgroupId:{0}", termGroupId);
                    return allRMTermSet;
                }
                //assembly TermSet with term
                foreach (RMTermSet termSet in allRMTermSet)
                {
                    List<RMTerm> allTerm = includeDeletedTerm ? TermDAO.GetTermFromTermSet(termSet.Id, true) : TermDAO.GetTermFromTermSetWithoutDeletedTerm(termSet.Id);
                    if (allTerm.Count != 0)
                    {
                        termSet.RMTerms = allTerm;
                        foreach (RMTerm rmTerm in allTerm)
                        {
                            BuildRMTerm(rmTerm, includeDeletedTerm);
                        }
                    }
                }
                logger.Info("BuildRMTermSetTree Complete.");
                return allRMTermSet;
            }
            catch (Exception e)
            {
                logger.Error("There are some error in buildRMTermSetTree {0}", e.ToString());
                return new List<RMTermSet>();
            }
        }

        private void BuildRMTerm(RMTerm rmTerm, bool includeDeletedTerm = true)
        {
            List<RMTerm> allSubTerm = includeDeletedTerm ? TermDAO.GetTermFromParentTerm(rmTerm) : TermDAO.GetTermFromParentTermWithoutDeletedTerm(rmTerm.Id);
            if (allSubTerm.Count != 0)
            {
                rmTerm.subTerms = allSubTerm;
                foreach (RMTerm subTerm in allSubTerm)
                {
                    BuildRMTerm(subTerm, includeDeletedTerm);
                }
            }
        }

        private void LoadBasicInfoForTermStore(TermStore termStore)
        {
            mTermStoreName = termStore.Name;
            mClientcontext.Load(termStore, t => t.WorkingLanguage, s => s.DefaultLanguage);
            mLcid = isCsdTenant ? termStore.DefaultLanguage : termStore.WorkingLanguage;
            logger.Info("Current TermStore. Termstore name:{0}, lcid:{1}", mTermStoreName, mLcid);
        }

        private bool CheckTermStoreNeedSkip(TermStoreCollection termSC, TermStore termStore)
        {
            bool needSkip = false;
            if (TermStoreIds != null && TermStoreIds.Count > 0)
            {
                //syn default term store
                if (!termStore.Id.Equals(termSC[0].Id))
                {
                    needSkip = true;
                }
            }
            if (mSyncTermStoreGuids.Contains(termStore.Id))
            {
                AddJobDetail(mTermStoreName, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_SkipToSyncMMS");
                logger.Info("This termStore has been synchronized. Termstore name,{0}", mTermStoreName);
                needSkip = true;
            }
            return needSkip;
        }

        private TermGroup GetTermGroupIfExist(TermStore termStore)
        {
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.GetTermGroupIfExist", $"GetTermGroupIfExist, term group name:{mRmTermGroup.Name}", true))
            {
                TermGroup mTermGroup = null;
                try
                {
                    mTermGroup = termStore.Groups.GetById(mRmTermGroup.UniqueId);
                    mClientcontext.Load(mTermGroup, t => t.Name, t => t.Description);
                    mClientcontext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    //此处报异常说明这个term group在term store中不存在，所以需要新创建
                    logger.Info($"term group doesn't exist. term group name:{mRmTermGroup.Name} error:{e.ToString()}");
                    mTermGroup = null;
                }
                return mTermGroup;
            }
        }

        private void UpdateTermGroup(TermGroup termGroup)
        {
            if (CheckIsChanged(termGroup.Name, mRmTermGroup.Name, termGroup.Description, mRmTermGroup.Description))
            {
                try
                {
                    logger.Info("current term group name or description is changed.");
                    using (var scope = new PerformanceScope("RMSharePointTaxonomy.UpdateTermGroup", $"UpdateTermGroup, term group name:{mRmTermGroup.Name}", true))
                    {
                        termGroup.Name = mRmTermGroup.Name;
                        termGroup.Description = mRmTermGroup.Description;
                        mClientcontext.ExecuteQuery();
                    }
                    AddJobDetail(mRmTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful, null);
                }
                catch (Exception e)
                {
                    if (e.Message.ToString().Contains("Group names must be unique."))
                    {
                        logger.Warn("Group names must be unique.", e.ToString());
                        AddJobDetail(mRmTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_RepeatOrDeny_GroupUnique");
                    }
                    else if (e.Message.ToString().Contains("The description"))
                    {
                        logger.Warn("The description", e.ToString());
                        AddJobDetail(mRmTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_RepeatOrDeny_Description");
                    }
                    else 
                    {
                        logger.Warn("Sync current rm term group has some error, group name {0} , error detail, {1}", mRmTermGroup.Name, e.ToString());
                        AddJobDetail(mRmTermGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_RepeatOrDeny");
                    }
                    throw;
                }
            }
            else
            {
                AddJobDetail(mRmTermGroup.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_NoChangeTermGroup");
            }
        }

        private string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                if (te.InnerException != null)
                {
                    comment = te.InnerException.Message;
                }
            }
            return comment;
        }

        private static bool IsTermSetLimitReached(ServerException se)
        {
            if (se == null)
            {
                return false;
            }

            string detail = $"{se.Message} {se.ServerErrorTypeName} {se.ServerErrorValue}".ToLowerInvariant();
            bool hasTermSetToken = detail.Contains("termset") || detail.Contains("term set");

            return (hasTermSetToken && (detail.Contains("maximum") || detail.Contains("limit") || detail.Contains("quota")))
                || (hasTermSetToken && detail.Contains("too many"));
        }

        private static string BuildServerExceptionDiagnostic(Exception e)
        {
            ServerException se = e as ServerException;
            if (se == null)
            {
                return string.Empty;
            }

            return $"ServerErrorTypeName:{se.ServerErrorTypeName}, ServerErrorCode:{se.ServerErrorCode}, ServerErrorValue:{se.ServerErrorValue}, CorrelationId:{se.ServerErrorTraceCorrelationId}";
        }

        private async System.Threading.Tasks.Task SyncTermSetAsync(RMTermSet rmTermSet, TermGroup defaultGroup, TermSetCollection termSetCollection)
        {
            logger.Info("Begin process termset,{0}", rmTermSet.Name);
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    TermSet editTermSet = GetTermSetIfExist(termSetCollection, rmTermSet);
                    if (editTermSet != null)
                    {
                        if (rmTermSet.IsRemoved)
                        {
                            //if termset is deleted in records, delete the corresponding termset in SP
                            RemoveTermSetInSP(editTermSet, rmTermSet);
                            return;
                        }
                        if (CheckIsChanged(editTermSet.Name, rmTermSet.Name, editTermSet.Description, rmTermSet.Description))
                        {
                            UpdateTermSet(editTermSet, rmTermSet);
                        }
                        else
                        {
                            AddJobDetail(rmTermSet.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_NoChangeTermSet");
                        }
                        if (rmTermSet.RMTerms != null && rmTermSet.RMTerms.Count != 0)
                        {
                            await SyncTermsAsync(editTermSet.Terms, rmTermSet.RMTerms, editTermSet, null);
                        }
                    }
                    else
                    {
                        if (rmTermSet.IsRemoved)
                        {
                            logger.Info($"Termset:{rmTermSet.Name} was deleted in records, no need to create new termset in SharePoint.");
                        }
                        else
                        {
                            logger.Info("SharePoint do not contains termset, need create {0}", rmTermSet.Name);
                            await CreateTermSetAsync(defaultGroup, rmTermSet);
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Warn("Some problems encountered in the process of dealing edit rmTermSet {0},detail message {1}", rmTermSet.Name, e.ToString());
                AddJobDetailForFailedTerm(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, GetExceptionMessage(e), rmTermSet.subTerms);
            }
        }

        private TermSet GetTermSetIfExist(TermSetCollection termSetCollection, RMTermSet rmTermSet)
        {
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.GetTermSetIfExist",$"GetTermSetIfExist, termset name:{rmTermSet.Name}", true))
            {
                TermSet mEditTermSet = null;
                try
                {
                    logger.Info("SharePoint contains termset,{0}", rmTermSet.Name);
                    mEditTermSet = termSetCollection.GetById(rmTermSet.UniqueId);
                    mClientcontext.Load(mEditTermSet, t => t.Name, t => t.Description);
                    mClientcontext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    //此处报异常说明这个term set在term group中不存在，所以需要新创建
                    logger.Info($"Termset not exists, name:{ rmTermSet.Name} error:{ e.ToString()}");
                    mEditTermSet = null;
                }
                return mEditTermSet;
            }
        }

        private void RemoveTermSetInSP(TermSet termSet, RMTermSet rmTermSet)
        {
            logger.Info("Delete TermSet,{0}", rmTermSet.Name);
            try
            {
                termSet.DeleteObject();
                mClientcontext.ExecuteQuery();
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_Delete", JobDetailsStatus.Successful, null);
            }
            catch (Exception ex)
            {
                logger.Warn("Delete term error, term name {0},message detail {1}", rmTermSet.Name, ex.ToString());
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_Delete", JobDetailsStatus.Failed, "RM_TS_TermSetDeny");
            }
        }

        private void UpdateTermSet(TermSet editTermSet, RMTermSet rmTermSet)
        {
            try
            {
                using (var scope = new PerformanceScope("RMSharePointTaxonomy.UpdateTermSet", $"UpdateTermSet, termset name:{rmTermSet.Name}", true))
                {
                    editTermSet.Name = rmTermSet.Name;
                    editTermSet.Description = rmTermSet.Description;
                    mClientcontext.ExecuteQuery();
                }
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful, null);
            }
            catch (Exception e)
            {
                logger.Warn("Sync current rm term set has some error, set name {0} , error detail, {1}", rmTermSet.Name, e.ToString());
                ServerException se = e as ServerException;
                if (se != null && e.Message.ToString().Contains("The description"))
                {
                    AddJobDetail(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSetRepeatOrDeny_Description");
                }
                else 
                {
                    AddJobDetail(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSetDeny");
                }
            }
        }

        private async System.Threading.Tasks.Task CreateTermGroupAsync(TermStore termStore, RMTermGroup rmTermGroup)
        {
            try
            {
                TermGroup newTermGroup = RealCreateTermGroup(termStore, rmTermGroup);
                AddJobDetail(rmTermGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, "");
                //因为这个term group是新创建的，所以它下面的term set也需要新创建
                foreach (RMTermSet rmTermSet in mAllTermSet)
                {
                    if (!rmTermSet.IsRemoved)
                    {
                        await CreateTermSetAsync(newTermGroup, rmTermSet);
                    }
                    else
                    {
                        logger.Info($"Termset:{rmTermSet.Name} was deleted in records, no need to create termset in SharePoint.");
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToString().Contains("Group names must be unique."))
                {
                    logger.Warn("Group names must be unique.", e.ToString());
                    AddJobDetailForFailedTermSet(rmTermGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_RepeatOrDeny_GroupUnique", mAllTermSet);
                }
                else if (e.Message.ToString().Contains("The description"))
                {
                    logger.Warn("The description", e.ToString());
                    AddJobDetailForFailedTermSet(rmTermGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_RepeatOrDeny_Description", mAllTermSet);
                }
                else 
                {
                    logger.Warn("Some problems encountered in the process of dealing with create termgroup, detail message {0}", e.ToString());
                    ServerException se = e as ServerException;
                    string message = string.Empty;
                    if (se != null && se.ServerErrorTypeName == "System.UnauthorizedAccessException" && IsAppAuth)
                    {
                        message = "RM_TS_AppScanDeny";
                    }
                    else
                    {
                        //在SharePoint中已有同名且id不同的term group，或者当前用户没有权限操作这个metadata management service
                        message = "RM_TS_RepeatOrDeny";
                    }
                    AddJobDetailForFailedTermSet(rmTermGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, message, mAllTermSet);
                }
            }
        }

        private TermGroup RealCreateTermGroup(TermStore termStore, RMTermGroup rmTermGroup)
        {
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.RealCreateTermGroup", $"RealCreateTermGroup, term group name:{rmTermGroup.Name}", true))
            {
                TermGroup newTermGroup;
                mClientcontext.Load(termStore, ts => ts.Groups);
                mClientcontext.ExecuteQuery();
                newTermGroup = termStore.CreateGroup(rmTermGroup.Name, rmTermGroup.UniqueId);
                newTermGroup.Description = rmTermGroup.Description;
                mClientcontext.ExecuteQuery();
                mClientcontext.Load(newTermGroup, ts => ts.TermSets);
                mClientcontext.ExecuteQuery();
                logger.Info("Create TermGroup {0}", rmTermGroup.Name);
                return newTermGroup;
            }
        }

        private async System.Threading.Tasks.Task CreateTermSetAsync(TermGroup defaultGroup, RMTermSet rmTermSet)
        {
            TermSet newTermSet = null;
            try
            {
                newTermSet = RealCreateTermSet(defaultGroup, rmTermSet);
                AddJobDetail(rmTermSet.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, null);
            }
            catch (Exception e)
            {
                logger.Warn("Create termset failed. Detail message {0}. {1}", e.ToString(), BuildServerExceptionDiagnostic(e));
                ServerException se = e as ServerException;
                if (IsTermSetLimitReached(se))
                {
                    AddJobDetailForFailedTerm(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSetMaxLimitReached", rmTermSet.subTerms);
                }
                else if (se != null && e.Message.ToString().Contains("The description"))
                {
                    AddJobDetailForFailedTerm(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSetRepeatOrDeny_Description", rmTermSet.subTerms);
                }
                else
                {
                    AddJobDetailForFailedTerm(rmTermSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSetRepeatOrDeny", rmTermSet.subTerms);
                }
                return;
            }
            //因为这个term set是新创建的，所以它下面的term也需要新创建
            if (rmTermSet.RMTerms != null && rmTermSet.RMTerms.Count != 0)
            {
                ReportManager.IncreaseBase(rmTermSet.RMTerms.Count);
                foreach (RMTerm rmTerm in rmTermSet.RMTerms)
                {
                    try
                    {
                        if (await CheckTermNeedSkipAsync(rmTerm))
                        {
                            continue;
                        }
                        await CreateTermUnderTermSetAsync(newTermSet, rmTerm);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Create term error term`s name {0},detail message {1}", rmTerm.Name, e.ToString());
                        AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, GetExceptionMessage(e), rmTerm.subTerms);
                    }
                    finally
                    {
                        ReportManager.Increase(1);
                    }
                }
            }
        }

        private TermSet RealCreateTermSet(TermGroup defaultGroup, RMTermSet rmTermSet)
        {
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.RealCreateTermSet", $"RealCreateTermSet, termset name:{rmTermSet.Name}", true))
            {
                TermSet newTermSet;
                mClientcontext.Load(defaultGroup, ts => ts.TermSets);
                mClientcontext.ExecuteQuery();
                logger.Info("Begin create TermSet name, {0} ; lcid, {1} ", rmTermSet.Name, mLcid);
                newTermSet = defaultGroup.CreateTermSet(rmTermSet.Name, rmTermSet.UniqueId, mLcid);
                newTermSet.Description = rmTermSet.Description;
                mClientcontext.ExecuteQuery();
                mClientcontext.Load(newTermSet, ts => ts.Terms);
                mClientcontext.ExecuteQuery();
                logger.Info("End create TermSet,", rmTermSet.Name);
                return newTermSet;
            }
        }

        private async System.Threading.Tasks.Task CreateTermUnderTermSetAsync(TermSet newTermSet, RMTerm rmTerm)
        {
            Term subTerm = null;
            try
            {
                subTerm = RealCreateTermUnderTermSet(newTermSet, rmTerm);
                AddJobDetail(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, null);
            }
            catch (Exception e)
            {

                //在SharePoint中已有同名且id不同的term
                logger.Warn("Some problems encountered in the process of dealing create term {0},detail message {1}", rmTerm.Name, e.ToString());
                ServerException se = e as ServerException;
                if (se != null && e.Message.ToString().Contains("The description"))
                {
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_TermRepeatOrDeny_Description", rmTerm.subTerms);
                }
                else
                {
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_TermRepeatOrDeny", rmTerm.subTerms);
                }
                return;
            }
            if (rmTerm.subTerms != null && rmTerm.subTerms.Count != 0)
            {
                ReportManager.IncreaseBase(rmTerm.subTerms.Count);
                foreach (RMTerm subRMTerm in rmTerm.subTerms)
                {
                    try
                    {
                        if (await CheckTermNeedSkipAsync(subRMTerm))
                        {
                            continue;
                        }
                        await CreateTermUnderTermAsync(subTerm, subRMTerm, true);
                    }
                    finally
                    {
                        ReportManager.Increase(1);
                    }
                }
            }
        }

        private Term RealCreateTermUnderTermSet(TermSet newTermSet, RMTerm rmTerm)
        {
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.RealCreateTermUnderTermSet", $"RealCreateTermUnderTermSet, term name:{rmTerm.Name}", true))
            {
                Term subTerm = newTermSet.CreateTerm(rmTerm.Name, mLcid, rmTerm.UniqueId);
                mClientcontext.Load(subTerm);
                subTerm.Deprecate(rmTerm.IsDeprecated);
                subTerm.SetDescription(rmTerm.Description, mLcid);
                mClientcontext.ExecuteQuery();
                logger.Info("Create Term {0} under TermSet,", rmTerm.Name);
                return subTerm;
            }
        }

        private async System.Threading.Tasks.Task CreateTermUnderTermAsync(Term newTerm, RMTerm rmTerm, bool isCreateChildTerm = false)
        {
            Term subTerm = null;
            try
            {
                subTerm = RealCreateTermUnderTerm(newTerm, rmTerm);
                AddJobDetail(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Successful, null);
            }
            catch (Exception e)
            {
                //在SharePoint中已有同名且id不同的term set
                logger.Warn("Some problems encountered in the process of dealing create term {0},detail message {1}", rmTerm.Name, e.ToString());
                ServerException se = e as ServerException;
                if (se != null && e.Message.ToString().Contains("The description"))
                {
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_TermRepeatOrDeny_Description", rmTerm.subTerms);
                }
                else
                {
                    //isCreateChildTerm这个term不是termset下的第一级term
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, isCreateChildTerm ? "RM_TS_TermRepeatOrDeny" : "RM_TS_TermRepeat", rmTerm.subTerms);
                }
                return;
            }
            if (rmTerm.subTerms != null && rmTerm.subTerms.Count != 0)
            {
                //CreateSubTerm(subTerm, rmTerm);
                ReportManager.IncreaseBase(rmTerm.subTerms.Count);
                foreach (RMTerm subRMTerm in rmTerm.subTerms)
                {
                    try
                    {
                        if (await CheckTermNeedSkipAsync(subRMTerm))
                        {
                            continue;
                        }
                        await CreateTermUnderTermAsync(subTerm, subRMTerm, true);
                    }
                    finally
                    {
                        ReportManager.Increase(1);
                    }
                }
            }
        }

        private async Task<bool> CheckTermNeedSkipAsync(RMTerm subRMTerm)
        {
            bool needSkip = false;
            //过滤掉已经在term management删除的term
            if (subRMTerm.IsRemoved)
            {
                logger.Info("Term is skipped,because it has been removed . {0}", subRMTerm.Name);
                needSkip = true;
            }
            //判断这个term是否在生效时间内，如果不在生效时间内，则不会创建这个term
            else if (!IsInTime(subRMTerm.TermExpirationFrom, subRMTerm.TermExpirationTo, (await GeneralSetting).TimeZoneId))
            {
                logger.Info("Term is skipped,because it is not within the valid time span. {0}", subRMTerm.Name);
                AddJobDetail(subRMTerm.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_TermOutTime");
                needSkip = true;
            }
            return needSkip;
        }

        private Term RealCreateTermUnderTerm(Term newTerm, RMTerm rmTerm)
        {
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.RealCreateTermUnderTerm", $"RealCreateTermUnderTerm, term name:{rmTerm.Name}", true))
            {
                Term subTerm = newTerm.CreateTerm(rmTerm.Name, mLcid, rmTerm.UniqueId);
                mClientcontext.Load(subTerm);
                subTerm.Deprecate(rmTerm.IsDeprecated);
                subTerm.SetDescription(rmTerm.Description, mLcid);
                logger.Info("Create Term {0} ", rmTerm.Name);
                mClientcontext.ExecuteQuery();
                return subTerm;
            }
        }
        //private void CreateSubTerm(Term subTerm, RMTerm rmTerm)
        //{
        //    if (rmTerm.subTerms != null && rmTerm.subTerms.Count != 0)
        //    {
        //        ReportManager.IncreaseBase(rmTerm.subTerms.Count);
        //        foreach (RMTerm subRMTerm in rmTerm.subTerms)
        //        {
        //            try
        //            {
        //                //过滤掉已经在term management删除的term
        //                if (subRMTerm.IsRemoved)
        //                {
        //                    logger.Info("Term is skipped,because it has been removed . {0}", subRMTerm.Name);
        //                    continue;
        //                }
        //                //判断这个term是否在生效时间内，如果不在生效时间内，则不会创建这个term
        //                else if (!IsInTime(subRMTerm.TermExpirationFrom, subRMTerm.TermExpirationTo, subRMTerm.TimeZoneId))
        //                {
        //                    logger.Info("Term is skipped,because it is not within the valid time span. {0}", subRMTerm.Name);
        //                    AddJobDetail(subRMTerm.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_TermOutTime");
        //                    continue;
        //                }
        //                CreateTermUnderTerm(subTerm, subRMTerm, true);
        //            }
        //            finally
        //            {
        //                ReportManager.Increase(1);
        //            }
        //        }
        //    }
        //}

        private async System.Threading.Tasks.Task SyncTermsAsync(TermCollection editTermCollection, List<RMTerm> rmTermList, TermSet termSet, Term ParentTerm)
        {
            ReportManager.IncreaseBase(rmTermList.Count);
            foreach (RMTerm rmTerm in rmTermList)
            {
                logger.Info("Process RMTerm,{0}", rmTerm.Name);
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        Term term = GetTermIfExist(rmTerm, editTermCollection);
                        if (term != null)
                        {
                            await SyncTermAsync(term, rmTerm);
                        }
                        else
                        {
                            await CreateTermAsync(term, rmTerm, termSet, ParentTerm);
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Warn("Some problems encountered in the process of dealing with this term,{0}, detail message {1}", rmTerm.Name, e.ToString());
                    //this.AddDetailToList(rmTerm.Name, "RM_TS_Action_Update", "Failed", e.Message);
                    AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, GetExceptionMessage(e), rmTerm.subTerms);
                }
                finally
                {
                    ReportManager.Increase(1);
                }
            }
        }

        private async System.Threading.Tasks.Task SyncTermAsync(Term term, RMTerm rmTerm)
        {
            if (rmTerm.IsRemoved)
            {
                //如果这个term在SharePoint中存在，并且在term management中已经被删除了，需要在SharePoint上也删除
                RemoveTermInSP(term, rmTerm);
                return;
            }
            logger.Info("Edit Term,{0}", rmTerm.Name);
            bool originalDeprecatedStatusInSP = term.IsDeprecated;
            bool currentDeprecatedStatusInSP = false;
            try
            {
                //修改这个term的Deprecated属性，即是否禁用，如果这个term已不在生效时间内，则禁用
                currentDeprecatedStatusInSP = await DeprecateTermInSPAsync(term, rmTerm);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while deprecating term, term name:{rmTerm.Name} Error:{e.ToString()}");
                return;
            }

            try
            {
                //check if need to update term
                UpdateTerm(term, rmTerm, originalDeprecatedStatusInSP != currentDeprecatedStatusInSP);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while updating term. Term name:{rmTerm.Name} Error:{e.ToString()}");
                return;
            }
            if (rmTerm.subTerms != null && rmTerm.subTerms.Count != 0)
            {
                await SyncTermsAsync(term.Terms, rmTerm.subTerms, null, term);
            }
        }

        private async System.Threading.Tasks.Task CreateTermAsync(Term term, RMTerm rmTerm, TermSet termSet, Term ParentTerm)
        {
            if (await CheckTermNeedSkipAsync(rmTerm))
            {
                return;
            }

            if (termSet == null)
            {
                logger.Info($"Create term:{rmTerm.Name}.");
                await CreateTermUnderTermAsync(ParentTerm, rmTerm);
            }
            else
            {
                logger.Info($"Create term:{rmTerm.Name} under termset.");
                await CreateTermUnderTermSetAsync(termSet, rmTerm);
            }
        }

        private Term GetTermIfExist(RMTerm rmTerm, TermCollection editTermCollection)
        {
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.GetTermIfExist", $"GetTermIfExist, name:{rmTerm.Name}", true))
            {
                Term mTerm = null;
                try
                {
                    mTerm = editTermCollection.GetById(rmTerm.UniqueId);
                    mClientcontext.Load(mTerm, ts => ts.Name, ts => ts.Description, ts => ts.IsDeprecated);
                    mClientcontext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    logger.Info($"Term not exists. Term name:{rmTerm.Name} Error:{e.ToString()}");
                    mTerm = null;
                }
                return mTerm;
            }
        }

        private void RemoveTermInSP(Term term, RMTerm rmTerm)
        {
            logger.Info("Delete Term,{0}", rmTerm.Name);
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.RemoveTermInSP", $"RemoveTermInSP, term name:{rmTerm.Name}", true))
            {
                try
                {
                    if (!NoDeleteTermIds.Contains(rmTerm.UniqueId))
                    {
                        term.DeleteObject();
                        mClientcontext.ExecuteQuery();
                        AddJobDetail(rmTerm.Name, "RM_TS_Action_Delete", JobDetailsStatus.Successful, null);
                    }
                    else
                    {
                        AddJobDetail(rmTerm.Name, "RM_TS_Action_Delete", JobDetailsStatus.Skipped, "Location Term Used");
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Delete term error, term name {0},message detail {1}", rmTerm.Name, ex.ToString());
                    AddJobDetail(rmTerm.Name, "RM_TS_Action_Delete", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny");
                }
            }
        }

        private async Task<bool> DeprecateTermInSPAsync(Term term, RMTerm rmTerm)
        {
            bool dbTermDeprecate = false;
            using (var scope = new PerformanceScope("RMSharePointTaxonomy.DeprecateTermInSP", $"DeprecateTermInSP, term name:{rmTerm.Name}", true))
            {
                if (!IsInTime(rmTerm.TermExpirationFrom, rmTerm.TermExpirationTo, (await GeneralSetting).TimeZoneId))
                {
                    try
                    {
                        if (!term.IsDeprecated)
                        {
                            term.Deprecate(true);
                            mClientcontext.Load(term, ts => ts.Name, ts => ts.Description, ts => ts.IsDeprecated);
                            mClientcontext.ExecuteQuery();
                        }
                        dbTermDeprecate = true;
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Update term error, term name {0},message detail {1}", rmTerm.Name, e.ToString());
                        AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny", rmTerm.subTerms);
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        term.Deprecate(rmTerm.IsDeprecated);
                        dbTermDeprecate = rmTerm.IsDeprecated;
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Update term error, term name {0},message detail {1}", rmTerm.Name, e.ToString());
                        AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny", rmTerm.subTerms);
                        throw;
                    }
                }
            }
            return dbTermDeprecate;
        }

        private void UpdateTerm(Term term, RMTerm rmTerm, bool needChange)
        {
            string termDes = string.Empty;
            Label defaultLabel = null;
            string termName = string.Empty;
            if (isCsdTenant)
            {
                var description = term.GetDescription(mLcid);
                mClientcontext.Load(term.Labels);
                mClientcontext.ExecuteQuery();
                termDes = description.Value;
                defaultLabel = term.Labels.FirstOrDefault((obj) => (obj.Language == mLcid) && obj.IsDefaultForLanguage);
                termName = defaultLabel?.Value;
            }
            else
            {
                termDes = term.Description;
                termName = term.Name;
            }
            //判断这个term和之前同步的相比是否有变化
            if (CheckIsChanged(termName, rmTerm.Name, termDes, rmTerm.Description) || needChange)
            {
                try
                {
                    using (var scope = new PerformanceScope("RMSharePointTaxonomy.UpdateTerm", $"UpdateTerm, term name:{rmTerm.Name}", true))
                    {
                        if (isCsdTenant)
                        {
                            AvePoint.GCommon.Utility.ArgumentCheck.NotNull(defaultLabel, nameof(defaultLabel));
                            if (!defaultLabel.Value.Equals(rmTerm.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                defaultLabel.Value = rmTerm.Name;
                            }
                        }
                        else
                        {
                            term.Name = rmTerm.Name;
                        }
                        term.SetDescription(rmTerm.Description, mLcid);
                        mClientcontext.ExecuteQuery();
                    }
                    AddJobDetail(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful, null);
                }
                catch (Exception e)
                {
                    logger.Warn("Update term error, term name {0},message detail {1}", rmTerm.Name, e.ToString());
                    ServerException se = e as ServerException;
                    if (se != null && e.Message.ToString().Contains("The description"))
                    {
                        AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermRepeatOrDeny_Description", rmTerm.subTerms);
                    }
                    else
                    {
                        AddJobDetailForFailedTerm(rmTerm.Name, "RM_TS_Action_Update", JobDetailsStatus.Failed, "RM_TS_TermSyncDeny", rmTerm.subTerms);
                    }
                }
            }
            else
            {
                AddJobDetail(rmTerm.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_NoChangeTerm");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TermExpirationFrom"></param>
        /// <param name="TermExpirationTo"></param>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        private bool IsInTime(long TermExpirationFrom, long TermExpirationTo, string timeZoneId)
        {
            if (TermExpirationFrom == 0 && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom == 0 && TermExpirationTo > DateTime.UtcNow.Ticks)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && DateTime.UtcNow.Ticks <= TermExpirationTo)
            {
                return true;
            }
            return false;
        }

        private bool CheckIsChanged(string name, string newName, string description, string newDescription)
        {
            if ((string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(newDescription)) || (!string.IsNullOrEmpty(description) && string.IsNullOrEmpty(newDescription)))
            {
                return true;
            }

            var legalName = GetLegalName(newName);
            if (!name.Equals(legalName) || (!string.IsNullOrEmpty(description) && !description.Equals(newDescription)))
            {
                return true;
            }
            return false;
        }


        private string GetLegalName(string newName)
        {
            string legalName = string.Empty;
            //移除多余空格
            if (!string.IsNullOrWhiteSpace(newName))
            {
                newName = newName.Trim();
                string[] strArray = newName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                legalName = string.Join(" ", strArray);
            }

            //半角替换全角
            return ReplaceStr(legalName);
        }

        private string ReplaceStr(string sourceStr)
        {
            string resultStr = "";
            if (!string.IsNullOrEmpty(sourceStr))
            {
                Regex reg = new Regex(@"[;<>|]+");
                sourceStr = reg.Replace(sourceStr.Trim(), "");
                if (!string.IsNullOrEmpty(sourceStr) && (sourceStr.Contains("&") || sourceStr.Contains("\"")))
                {
                    //替换成全角的
                    resultStr = sourceStr.Replace('&', '＆').Replace('"', '＂');
                }
                else
                {
                    resultStr = sourceStr;
                }
            }
            return resultStr;
        }
        private void AddJobDetail(string termName, string action, JobDetailsStatus status, string message)
        {
            if (status == JobDetailsStatus.Failed)
            {
                JobHasError = true;
            }
            else if (status == JobDetailsStatus.Successful || status == JobDetailsStatus.Skipped)
            {
                FinsihCount++;
            }
            JMTermSyncJobDetails detail = new JMTermSyncJobDetails();
            detail.MMSApplication = CurrentAdminUrl;
            detail.Term = termName;
            detail.Action = action;
            //detail.SiteCollectionURL = CurrentSiteUrl;
            detail.Status = status;
            detail.Comment = message;
            ReportManager.SendJobDetail(detail);
        }
        /// <summary>
        /// //如果一个父亲级别的term同步失败了，则需要把它下面的所有层级的子term的detail打出来
        /// </summary>
        /// <param name="termName"></param>
        /// <param name="action"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <param name="rmTerms"></param>
        private void AddJobDetailForFailedTerm(string termName, string action, JobDetailsStatus status, string message, List<RMTerm> rmTerms)
        {
            try
            {
                if (!string.IsNullOrEmpty(termName))
                {
                    if (status == JobDetailsStatus.Failed)
                    {
                        JobHasError = true;
                    }
                    else if (status == JobDetailsStatus.Successful)
                    {
                        FinsihCount++;
                    }
                    JMTermSyncJobDetails detail = new JMTermSyncJobDetails();
                    detail.MMSApplication = CurrentAdminUrl;
                    detail.Term = termName;
                    detail.Action = action;
                    detail.Status = status;
                    detail.Comment = message;
                    ReportManager.SendJobDetail(detail);
                }

                if (rmTerms != null && rmTerms.Count != 0)
                {
                    foreach (RMTerm rmTerm in rmTerms)
                    {
                        if (rmTerm.IsRemoved)
                        {
                            continue;
                        }
                        JMTermSyncJobDetails childDetail = new JMTermSyncJobDetails();
                        childDetail.MMSApplication = CurrentAdminUrl;
                        childDetail.Term = rmTerm.Name;
                        childDetail.Action = "RM_TS_Action_Skip";
                        childDetail.Status = JobDetailsStatus.Skipped;
                        childDetail.Comment = "RM_TS_ParentSyncFail";
                        ReportManager.SendJobDetail(childDetail);
                        AddJobDetailForFailedTerm(null, null, JobDetailsStatus.None, null, rmTerm.subTerms);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while AddJobDetailForFailedTerm, Error:{0}", e.ToString());
            }
        }
        /// <summary>
        /// //如果一个term set同步失败了，则需要把它下面的所有层级的子term的detail打出来
        /// </summary>
        /// <param name="termName"></param>
        /// <param name="action"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <param name="rmTermSets"></param>
        private void AddJobDetailForFailedTermSet(string termName, string action, JobDetailsStatus status, string message, List<RMTermSet> rmTermSets)
        {
            try
            {
                if (!string.IsNullOrEmpty(termName))
                {
                    if (status == JobDetailsStatus.Failed)
                    {
                        JobHasError = true;
                    }
                    else if (status == JobDetailsStatus.Successful)
                    {
                        FinsihCount++;
                    }
                    JMTermSyncJobDetails detail = new JMTermSyncJobDetails();
                    detail.MMSApplication = CurrentAdminUrl;
                    detail.Term = termName;
                    detail.Action = action;
                    detail.Status = status;
                    detail.Comment = message;
                    ReportManager.SendJobDetail(detail);
                }
                if (rmTermSets != null && rmTermSets.Count != 0)
                {
                    foreach (RMTermSet rmTermSet in rmTermSets)
                    {
                        JMTermSyncJobDetails detail = new JMTermSyncJobDetails();
                        detail.MMSApplication = CurrentAdminUrl;
                        detail.Term = rmTermSet.Name;
                        detail.Action = "RM_TS_Action_Skip";
                        detail.Status = JobDetailsStatus.Skipped;
                        detail.Comment = "RM_TS_ParentSyncFail";
                        ReportManager.SendJobDetail(detail);
                        if (rmTermSet.RMTerms != null && rmTermSet.RMTerms.Count != 0)
                        {
                            AddJobDetailForFailedTerm(null, null, JobDetailsStatus.None, null, rmTermSet.RMTerms);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while AddJobDetailForFailedTermSet, Error{0}", e.ToString());
            }
        }

        public void Dispose()
        {
            try
            {
                mClientcontext.Dispose();
            }
            catch (Exception e)
            {
                logger.Warn("Dispose the SharePoint Taxonomy clientcontext error {0}", e.ToString());
            }
        }

        #region provide for other function
        /// <summary>
        ///拿RA中的AllTerm与SharePoint TermStore中的Term比较，获取其中在SharePoint中进行删除操作的Term
        /// </summary>
        /// <returns></returns>
        public async Task<List<RMTerm>> GetOrphanedTermOfSharePointAsync(string jobId)
        {
            try
            {
                int tempcounter = 0;

                List<RMTerm> orphanedTermsOfSP = new List<RMTerm>();
                //加载这个site collection所在的Web App关联的所有metadata management service
                TermStoreCollection termSC = LoadTermStore();//default termstore
                if (termSC.Count == 0)
                {
                    //如果没有termStore，则说明这个site collection所在的Web App没有关联metadata management service
                    return orphanedTermsOfSP;
                }
                var termStore = termSC[0];
                //当前这个termStore是否遍历过
                bool isSuccess = true;
                if (mOrphanTermStoreGuids.Contains(termStore.Id))
                {
                    return null;
                }
                foreach (var rmTerm in mAllTerm)
                {
                    try
                    {
                        Term term = null;
                        try
                        {
                            term = termStore.GetTerm(rmTerm.UniqueId);
                            mClientcontext.Load(term, ts => ts.Name, ts => ts.IsDeprecated);
                            mClientcontext.ExecuteQuery();
                            if (term.IsDeprecated && !rmTerm.IsDeprecated)
                            {
                                //REC-2668 在sharepoint中禁用的term，不算做Orphan的term
                                //在SP中是被禁用的 ，而在RM中没有被禁用
                                //orphanedTermsOfSP.Add(new RMTerm() { Name = rmTerm.Name, UniqueId = rmTerm.UniqueId, IsRemoved = rmTerm.IsRemoved, IsDeprecated = rmTerm.IsDeprecated, IsSPDeprecated = true, IsSPRemoved = false });
                                //logger.Info("term name : {0} in RA is Available but in sp is Deprecated", rmTerm.Name);
                            }
                        }
                        catch (Exception e)
                        {
                            //此处报异常说明这个term在termstore中不存在
                            if (!rmTerm.IsRemoved)
                            {
                                //在SP中不存在，在RM中存在
                                orphanedTermsOfSP.Add(new RMTerm() { Name = rmTerm.Name, UniqueId = rmTerm.UniqueId, IsRemoved = rmTerm.IsRemoved, IsDeprecated = rmTerm.IsDeprecated, IsSPRemoved = true, IsSPDeprecated = false });
                                logger.Info("term name : {0} in RA is Available but in sp is Deleted", rmTerm.Name);
                            }
                        }

                        tempcounter++;
                        if (tempcounter >= 100)
                        {
                            await JobService.UpdateJobWithoutProgressChangeAsync(jobId);
                            tempcounter = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while OrphanedTerm in RA compare with SharPoint,term name: {0}, error message: {1}", rmTerm.Name, e.ToString());
                    }
                }
                //记录一下已经遍历过的term store
                if (isSuccess)
                {
                    mOrphanTermStoreGuids.Add(termStore.Id);
                }
                return orphanedTermsOfSP;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while GetOrphanedTermOfSharePoint in orphanedTermReport,error message: {0}", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 拿RA中的AllTerm与SharePoint TermStore中的Term比较，获取其中在SharePoint中禁用的Term
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public virtual async Task<List<RMTerm>> GetRetiredTermOfSharePointAsync(string jobId)
        {
            try
            {
                int tempcounter = 0;

                List<RMTerm> retiredTermsOfSP = new List<RMTerm>();
                //加载这个site collection所在的Web App关联的所有metadata management service
                TermStoreCollection termSC = LoadTermStore();//default termstore
                if (termSC.Count == 0)
                {
                    //如果没有termStore，则说明这个site collection所在的Web App没有关联metadata management service
                    return retiredTermsOfSP;
                }
                var termStore = termSC[0];
                //当前这个termStore是否遍历过
                bool isSuccess = true;
                if (mRetiredTermStoreGuids.Contains(termStore.Id))
                {
                    return null;
                }
                foreach (var rmTerm in mAllTerm)
                {
                    try
                    {
                        Term term = null;
                        try
                        {
                            term = termStore.GetTerm(rmTerm.UniqueId);
                            mClientcontext.Load(term, ts => ts.Name, ts => ts.IsDeprecated);
                            mClientcontext.ExecuteQuery();

                            if (term.IsDeprecated && !rmTerm.IsDeprecated)
                            {
                                //在SP中是被禁用的 ，而在RM中没有被禁用
                                retiredTermsOfSP.Add(new RMTerm() { Name = rmTerm.Name, UniqueId = rmTerm.UniqueId, IsRemoved = rmTerm.IsRemoved, IsDeprecated = rmTerm.IsDeprecated, IsSPDeprecated = true, IsSPRemoved = false });
                                logger.Info("term name : {0} in RA is Available but in sp is Deprecated", rmTerm.Name);
                            }
                        }
                        catch (Exception e)
                        {
                            //此处报异常说明这个term在termstore中不存在
                            if (!rmTerm.IsRemoved)
                            {
                                //在SP中不存在，在RM中存在
                                logger.Info("RetiredTermReport.Term name : [{0}] in RA is Available but in sp is Deleted", rmTerm.Name);
                            }
                        }

                        tempcounter++;
                        if (tempcounter >= 100)
                        {
                            await JobService.UpdateJobWithoutProgressChangeAsync(jobId);
                            tempcounter = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while RetiredTerm in RA compare with SharPoint,term name: {0}, error message: {1}", rmTerm.Name, e.ToString());
                    }
                }
                //记录一下已经遍历过的term store
                if (isSuccess)
                {
                    mRetiredTermStoreGuids.Add(termStore.Id);
                }
                return retiredTermsOfSP;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while GetRetiredTermOfSharePoint in RetiredTermReport,error message: {0}", e.ToString());
                throw;
            }
        }
        #endregion
    }

}
