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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using Newtonsoft.Json;
using RAGoogle.JobProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Google;

namespace AvePoint.RA.SharePoint.RMSharePointTaxnomy
{
    public class RMSyncTermProcessor: IDisposable
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMSyncTermProcessor));
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        #region dao
        private ITermGroupDao mTermGroupDAO { get; set; }
        public ITermGroupDao TermGroupDAO
        {
            get
            {
                if (mTermGroupDAO == null)
                {
                    mTermGroupDAO = (ITermGroupDao)PlatformWindsorManager.GetService(typeof(ITermGroupDao));
                }
                return mTermGroupDAO;
            }
        }
        private ITermGroupMembershipDao mTermGroupMembershipDao { get; set; }
        public ITermGroupMembershipDao TermGroupMembershipDao
        {
            get
            {
                if (mTermGroupMembershipDao == null)
                {
                    mTermGroupMembershipDao = (ITermGroupMembershipDao)PlatformWindsorManager.GetService(typeof(ITermGroupMembershipDao));
                }
                return mTermGroupMembershipDao;
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

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private IRMGoogleJobService GoogleJobService => PlatformWindsorManager.GetService<IRMGoogleJobService>();

        #endregion

        #region private property  
        private List<RemoteSiteCollection> mRemoteSiteCollectionsInDAO;
        private RMSharePointTaxonomy mRMSharePointTaxonomy;
        private List<Guid> mTermStoreIdsForGroup;
        private List<RMTermGroup> mAllTermGroups;
        private List<Guid> mAllGoogleTermGroupIds = [];
        private bool mHasError;
        private int mFinishCount;
        private string mErrorMessage;
        private List<JPMCTenantConfig> mJPMCTenantConfigs;
        public List<Guid> NoDeleteTermids = new List<Guid>();
        public string _jobId = string.Empty;
        private bool _fromGoogleOne = false;
        public JobStatus jobTermGoogleFinishStatus = JobStatus.Finished;
        #endregion

        public RMSyncTermProcessor(string jobId, JobType jobType, bool fromGoogleOne)
        {
            this._jobId = jobId;
            this._fromGoogleOne = fromGoogleOne;
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager.IncreaseBase(100);
            ReportManager.Increase(1);
            ReportManager.StartUpdateJobProgress(60);
        }

        public RMSyncTermProcessor()
        {
        }

        public async Task SyncTermAsync()
        {
            try
            {
                Initialize();
                foreach (var group in mAllTermGroups)
                {
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        if(!_fromGoogleOne && group.M365TermSyncOption != TermSyncOption.None && TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL))
                        {
                            await SyncTermGroupAsync(group);
                        }
                        if(group.GoogleTermSyncOption != TermSyncOption.None)
                        {
                            mAllGoogleTermGroupIds.Add(group.UniqueId);
                        }
                    }
                }
                var hasGControlLicense = await TenantService.HasInitGControlPlatForm();
                if (TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusGoogle) || hasGControlLicense)
                {
                    jobTermGoogleFinishStatus = await SyncTermGroupGoogleAsync(mAllGoogleTermGroupIds);
                }

                //assemble term rule 
                InitJPMC();
                if (mJPMCTenantConfigs != null && mJPMCTenantConfigs.Count > 0)
                {
                    HashSet<string> processTenant = [];
                    foreach (var current365TenantSettings in mJPMCTenantConfigs)
                    {
                        if (!string.IsNullOrEmpty(current365TenantSettings.M365TenantId))
                        {
                            if (!processTenant.Contains(current365TenantSettings.M365TenantId))
                            {
                                processTenant.Add(current365TenantSettings.M365TenantId);
                                var syncCustomization4JPMC = new RMSyncCustomization4JPMC(current365TenantSettings);
                                var jpmcHasError = syncCustomization4JPMC.JPMCCustomizationSync();
                                if (jpmcHasError)
                                {
                                    mHasError = true;
                                }
                            }
                            else
                            {
                                logger.Warn($"This tenant has been executed. {current365TenantSettings.ConfigSiteUrl}");
                            }
                        }
                        else
                        {
                            ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                            {
                                Term = "RM_JS_Common_Pending",
                                Action = @"N/A",
                                MMSApplication = "RM_JS_Common_Pending",
                                Status = JobDetailsStatus.Failed,
                                Comment = $"RM_TM_Action_CustomizationAppSync_ConfigSiteNeedSyncToOpus{I18NEntity.Separator}{current365TenantSettings.ConfigSiteUrl}"
                            });
                            mHasError = true;
                        }
                    }
                }
                else
                {
                    logger.Info("There is no [JPMCTenantConfigs]");
                }
                Finish();

                if(_fromGoogleOne && (jobTermGoogleFinishStatus == JobStatus.Finished || jobTermGoogleFinishStatus == JobStatus.FinishWithException))
                {
                    GoogleJobService.ApplySettings(JobRunBy.Schedule, false, RunApplySettingMethod.AllScope);
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while running term sync job. Error:{e.ToString()}");
                mHasError = true;
                Finish();
            }
        }

        #region private method
        private void Initialize()
        {
            mAllTermGroups = TermGroupDAO.LoadNeedSyncTermGroups([SiteType.Online, SiteType.Google]);
            if (mAllTermGroups.Count == 0)
            {
                mErrorMessage = I18NEntity.GetString("RM_TS_SS_Summary");
                throw new Exception("There is no term groups need to sync in records.");
            }

            try
            {
                GetRemoteSiteCollectionsFromDAO();
            }
            catch (Exception ex)
            {
                logger.Error("browse register sites error,error message {0}", ex.ToString());
                mErrorMessage = string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), ex.Message);
                throw;
            }
        }

        private void InitJPMC()
        {
            var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization");
            try
            {
                List<JPMCTenantConfig> configs = null;
                if (jsonConfig != null)
                {
                    configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig.Value);
                    var remoteSites = RemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(configs.Select(c => c.ConfigSiteUrl).ToList());
                    configs.ForEach(c =>
                            {
                            var remoteSite = remoteSites.FirstOrDefault(s => s.url == c.ConfigSiteUrl);
                            if (remoteSite != null)
                            {
                            c.ConfigSite = remoteSite;
                            c.M365TenantId = remoteSite.TenantId;
                            }
                            else
                            {
                            logger.Warn($"Can not get this site:{c.ConfigSiteUrl}");
                            }
                            });
                    mJPMCTenantConfigs = configs;
                }
            }
            catch (JsonException e)
            {
                logger.Warn($"JPMC_Customization value is not a valid JSON tenant config (it may only be a feature flag). Value: [{jsonConfig.Value}]. Error: {e.Message}");
            }
        }

        private async Task SyncTermGroupAsync(RMTermGroup termGroup)
        {
            try
            {
                mRMSharePointTaxonomy = new RMSharePointTaxonomy(termGroup);
                mRMSharePointTaxonomy.NoDeleteTermIds = NoDeleteTermids;
                await RealSyncTermGroupAsync(termGroup);
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                mErrorMessage = "RM_SYNC_InitException";
                logger.Warn("Process sync termTree in termGroup {0} error , ID {1}, detail message {2}", termGroup.Name, termGroup.UniqueId, ex.ToString());
                ReportManager.SendJobDetail(new JMTermSyncJobDetails() { Term = "RM_JS_Common_Pending",  Action = @"N/A", MMSApplication = "RM_JS_Common_Pending", Status = JobDetailsStatus.Failed, Comment = "RM_SYNC_InitException" });
                //SiteCollectionURL = termGroup.Name,
                mHasError = true;
            }
            finally
            {
                mHasError |= mRMSharePointTaxonomy.JobHasError;
                mFinishCount += mRMSharePointTaxonomy.FinsihCount;
                if (mTermStoreIdsForGroup != null)
                {
                    mTermStoreIdsForGroup.Clear();
                }
            }
        }
        private async Task<JobStatus> SyncTermGroupGoogleAsync(List<Guid> termGroupIds)
        {
            var syncGoogleTermProcessor = new SyncTermToGoogleProcessor(_jobId, JobType.TermSynchronization, termGroupIds);
            await syncGoogleTermProcessor.KickOffAsync();
            return syncGoogleTermProcessor.jobFinishStatus;
        }
        private void Finish()
        {
            try
            {
                var jobTermFinishStatus = DetermineJobStatus(mHasError, mFinishCount, mErrorMessage);
                if (jobTermFinishStatus == JobStatus.Finished && jobTermGoogleFinishStatus == JobStatus.Finished)
                {
                    ReportManager.SetJobFinished(JobStatus.Finished);
                }
                else if (jobTermFinishStatus == JobStatus.Failed && jobTermGoogleFinishStatus == JobStatus.Failed)
                {
                    ReportManager.SetJobFinished(JobStatus.Failed, string.IsNullOrWhiteSpace(mErrorMessage) ? "RM_TS_SS_Summary" : mErrorMessage);
                }
                else
                {
                    ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_TS_SS_Summary");
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while updating job status. Eror:{e.ToString()}");
            }
        }
        private JobStatus DetermineJobStatus(bool hasError, int finishCount, string? errorMessage)
        {
            if (!hasError)
            {
                return JobStatus.Finished;
            }
            else if (hasError && finishCount > 0)
            {
                return JobStatus.FinishWithException;
            }
            else
            {
                return JobStatus.Failed;
            }
        }

        private bool IsDeletedSiteException(Exception ex)
        {
            if (ex == null) return false;
            if (ex is ArgumentNullException argEx && argEx.ParamName == "context")
                return true;
            if (ex is System.Net.WebException webEx && webEx.Message.Contains("404"))
                return true;
            if (ex.Message != null &&
                ex.Message.Contains("Site may have been deleted from SharePoint"))
                return true;
            if (ex.InnerException != null)
                return IsDeletedSiteException(ex.InnerException);
            return false;
        }

        private async Task RealSyncTermGroupAsync(RMTermGroup termGroup)
        {
            var sites = GetNeedSyncSites(termGroup.UniqueId, termGroup.UsingMMSSpecified);
            if (sites.Count == 0 && termGroup.UsingMMSSpecified)
            {
                mHasError = true;
                return;
            }
            foreach (var site in sites)
            {
                using (CheckJobStopScope stopScope = new CheckJobStopScope())
                {
                    try
                    {
                        if (new Uri(site.url).Host.ToLower().Split(".").First().EndsWith("-my"))
                        {
                            logger.Warn($"Current site [{site.url}] is OneDrive site. Skipped it.");
                            continue;
                        }
                        await SyncTermForTenantAsync(site.TenantId, termGroup, site);
                    }
                    catch (JobStopException)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception e)
                    {
                        if (IsDeletedSiteException(e))
                        {
                            logger.Warn($"Site [{site?.url}] has been deleted from SharePoint, skipping this site.");
                            continue;
                        }
                        logger.Warn("Process sync termTree in site {0} error , farmName {1}, detail message {2}", site?.url, site?.AdminUrl, e.ToString());
                        ReportManager.SendJobDetail(new JMTermSyncJobDetails() { Term = "RM_JS_Common_Pending", Action = @"N/A", MMSApplication = site?.AdminUrl.TrimEnd('/') ?? "RM_JS_Common_Pending", Status = JobDetailsStatus.Failed, Comment = "RM_SYNC_InitException" });
                        mHasError = true;
                    }
                }
            }
        }



        private async Task SyncTermForTenantAsync(string tenantId, RMTermGroup group, RemoteSiteCollection site)
        {
            logger.Info($"Synchronize term to tenant, termGroupName:[{group.Name}], tenantId:[{tenantId}],init context use site url:[{site.url}], admin url:[{site.AdminUrl}]");
            mRMSharePointTaxonomy.CurrentAdminUrl = site?.AdminUrl?.TrimEnd('/');
            if (string.IsNullOrEmpty(mRMSharePointTaxonomy.CurrentAdminUrl))
            {
                logger.Warn($"AdminUrl is null or empty, get the admin url.");
                mRMSharePointTaxonomy.CurrentAdminUrl = WebUtil.GetSPAdminUrl(site.url, site.TenantId);
            }
            using (PerformanceScope iScope = new PerformanceScope("Term Sync InitClientContext"))
            {
                mRMSharePointTaxonomy.InitClientContext(site);
                mRMSharePointTaxonomy.CurrentSiteUrl = site.url;
            }

            if (mTermStoreIdsForGroup != null && mTermStoreIdsForGroup.Count > 0)
            {
                var curTermStoreId = mRMSharePointTaxonomy.GetDefaultTermStoreId();
                if (!mTermStoreIdsForGroup.Contains(curTermStoreId))
                {
                    logger.Warn($"Cannot find current store id: {curTermStoreId} from term group membership.");
                    return;
                }
            }
            await mRMSharePointTaxonomy.SyncTermToSharePointAsync();
        }

        private List<RemoteSiteCollection> GetNeedSyncSites(Guid termGroupId, bool usingMMSSpecified)
        {
            if(!usingMMSSpecified)
            {
                var result = mRemoteSiteCollectionsInDAO
                    .Where(item => item.NodeType != RemoveNodeType.SkyDrivePro)
                    .GroupBy(item => item.TenantId, item => item)
                    .ToDictionary(
                        item => item.Key,
                        item => item.GroupBy(item => new Uri(item.url).Host.ToLower(), item => item)
                                .ToDictionary(item => item.Key, item => item.FirstOrDefault()).Values.ToList()
                     ).Values.SelectMany(item => item).ToList();
                logger.Info($"Current no using MMSS pecified, need process site : {string.Join(',', result.Select(item => item.url))}");
                return result;
            }

            mTermStoreIdsForGroup = TermGroupMembershipDao.GetTermStoreIdsByTermGroupId(termGroupId, SiteType.Online);
            mRMSharePointTaxonomy.TermStoreIds = mTermStoreIdsForGroup;
            List<RMTermGroupMembership> termGroupMembership = TermGroupMembershipDao.GetTermGroupMembershipByTermGroupId(termGroupId, SiteType.Online);
            var remoteNodes = mRemoteSiteCollectionsInDAO
                .Where(item => termGroupMembership.Any(s => s.SiteUrl.Equals(item.url, StringComparison.OrdinalIgnoreCase)) && item.NodeType != RemoveNodeType.SkyDrivePro)
                .ToList();
            
            var res = remoteNodes.GroupBy(item => new Uri(item.url).Host.ToLower(), item => item)
                .ToDictionary(item => item.Key, item => item.FirstOrDefault())
                .Values.ToList();

            if (!res.Any())
            {
                foreach (var groupMembership in termGroupMembership)
                {
                    var detail = new JMTermSyncJobDetails() { Term = "RM_JS_Common_Pending", Action = @"N/A", MMSApplication = "RM_JS_Common_Pending", Status = JobDetailsStatus.Failed, Comment = "RM_TS_SCNotRegister" };
                    // SiteCollectionURL = groupMembership.SiteUrl,
                    ReportManager.SendJobDetail(detail);
                }
            }

            return res;
        }

        /*private List<string> GetNeedSyncTenantIds(Guid termGroupId, bool usingMMSSpecified)
        {
            if (usingMMSSpecified)
            {
                mTermStoreIdsForGroup = TermGroupMembershipDao.GetTermStoreIdsByTermGroupId(termGroupId, SiteType.Online);
                mRMSharePointTaxonomy.TermStoreIds = mTermStoreIdsForGroup;
                List<RMTermGroupMembership> termGroupMembership = TermGroupMembershipDao.GetTermGroupMembershipByTermGroupId(termGroupId, SiteType.Online);
                var tenantIds = mRemoteSiteCollectionsInDAO.Where(a => termGroupMembership.Any(s => s.SiteUrl.Equals(a.url))).Select(s => s.TenantId).Distinct().ToList();
                if (tenantIds.Count == 0)
                {
                    foreach (var groupMembership in termGroupMembership)
                    {
                        var detail = new JMTermSyncJobDetails() { Term = "RM_JS_Common_Pending", Action = @"N/A", MMSApplication = "RM_JS_Common_Pending", Status = JobDetailsStatus.Failed, Comment = "RM_TS_SCNotRegister" };
                        // SiteCollectionURL = groupMembership.SiteUrl,
                        ReportManager.SendJobDetail(detail);
                    }
                }
                return tenantIds;
            }
            else
            {
                return mRemoteSiteCollectionsInDAO.Select(s => s.TenantId).Distinct().ToList();
            }
        }*/

        private void GetRemoteSiteCollectionsFromDAO()
        {
            if (mAllTermGroups.Any(o => !o.UsingMMSSpecified))
            {
                using PerformanceScope scope = new("Get all sites");
                mRemoteSiteCollectionsInDAO = RABrowserClient.GetAuthorisedRemoteSiteCollectionsByUser();
            }
            else
            {
                using PerformanceScope scope = new("Get all specified sites");
                var siteUrls = TermGroupMembershipDao.GetAllSpecifiedSites(SiteType.Online);
                mRemoteSiteCollectionsInDAO = RemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(siteUrls);
            }
        }
        #endregion

        #region public method for other function
        public async Task<List<RMTermSet>> BuildRMTermSetTreeAsync(TermSetType termSetType, Guid termGroupId, bool includeDeletedTerm = true)
        {
            try
            {
                logger.Info("begin BuildRMTermSetTree:{0}", termGroupId);
                List<RMTermSet> allRMTermSet = await TermSetDAO.LoadTermSetAsync(termSetType, termGroupId);
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

        public async Task<List<RMTermSet>> BuildRMTermSetTreeByGroupNameAsync(TermSetType termSetType, string groupName, bool includeDeletedTerm = true)
        {
            List<RMTermSet> allRMTermSet = null;
            List<RMTermGroup> rmTermGroups = TermGroupDAO.LoadTermGroup();
            foreach (RMTermGroup group in rmTermGroups)
            {
                if (string.Equals(group.Name, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    allRMTermSet = await BuildRMTermSetTreeAsync(termSetType, group.UniqueId, includeDeletedTerm);
                    break;
                }
            }
            return allRMTermSet;
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

        public void Dispose()
        {
            if(mRMSharePointTaxonomy != null)
            {
                mRMSharePointTaxonomy.Dispose();
            }
        }
        #endregion
    }
}
