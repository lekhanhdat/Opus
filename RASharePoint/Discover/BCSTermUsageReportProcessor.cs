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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Discover
{
    public class BCSTermUsageReportProcessor : RMReportProcessor
    {

        private IExplorerDao ExplorerDao = new ExplorerDao();
        private Dictionary<Guid, RMTermIdentity> mUsageTermInfo;
        private Dictionary<Guid, List<RMTermIdentity>> mSharePointOrphanTermsInfo;
        private List<Guid> mUsageTermId;
        private bool isOrphanedTermReport;

        private Guid CurrentContainerNodeId;
        private Guid CurrentSiteCollectionNodeId;

        #region For retired Term
        private bool mIsRetiredTermReport;
        private Dictionary<Guid, List<RMTermIdentity>> mSharePointRetiredTermsInfo;
        private List<Guid> spRetiredTermIds = new List<Guid>();
        #endregion

        private Guid curTermStoreId;
        private RMSharePointTaxonomy mRMSharePointTax;
        private ITermDao TermDao;

        private List<Guid> spOrphanedTermIds = new List<Guid>();
        //private string homeLocationName;
        //private string lifecycleStatusName;
        //private string boxName;
        //private string availabilityName;
        //private string currentlyHeldByName;
        private Dictionary<Guid, List<int>> mWssidsInWeb = new Dictionary<Guid, List<int>>();
        //protected const string CONTENT_TYPE_PHYSICAL_FILE_NAME = "Physical File";
        public BCSTermUsageReportProcessor(string jobId, string profileId, bool IsOrphanedTermReport, bool isRetiredTermReport)
            : base(jobId, (int)JobType.BCSTermUsageReport, IsOrphanedTermReport)
        {
            RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
            isOrphanedTermReport = IsOrphanedTermReport;
            mIsRetiredTermReport = isRetiredTermReport;
            if (IsOrphanedTermReport)
            {
                mUsageTermInfo = ReportService.GetOrphanedTermsOfRMAsync().Result;
            }
            else if (isRetiredTermReport)
            {
                mUsageTermInfo = ReportService.GetRetiredTermsOfRMAsync().Result;
            }
            else
            {
                mUsageTermInfo = ReportService.GetTermIDsFromBCSTermTreeAsync(profile.Extension1).Result;
            }
            mUsageTermId = mUsageTermInfo.Select(_ => _.Key).ToList();
            //mUsageTermInfo = IsOrphanedTermReport ? ReportService.GetOrphanedTermsOfRM() : ReportService.GetTermIDsFromBCSTermTree(profile.Extension1);
            //mSharePointOrphanTermsInfo = IsOrphanedTermReport ? RMSharePointTaxonomyService.GetOrphanedTermsOfSharePoint() : null;
            TermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            SendJobReportSummary();
        }

        public override async System.Threading.Tasks.Task RunReportJobAsync()
        {
            if (mUsageTermInfo == null || mUsageTermInfo.Count == 0)
            {
                ReportManager.SetJobFinished(JobStatus.Failed, "RM_RC_TUR_NoTermForReport");
                return;
            }
            try
            {
                if (isOrphanedTermReport || mIsRetiredTermReport)
                {
                    using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.InitSPTaxnomy"))
                    {
                        InitSPTaxnomy();
                    }
                }
                foreach (var SiteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    await ProcessAsync(SiteCollectionNodeItem);
                }
            }
            catch (JobStopException ex)
            {
                mJobHasStopped = true;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (mJobHasException)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (mJobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                ReportManager.SetJobFinished(finalStatus);
            }
        }

        private void InitSPTaxnomy()
        {
            var allTerms = TermDao.GetAllTerms();
            mRMSharePointTax = new RMSharePointTaxonomy(null, null, allTerms);
            mSharePointOrphanTermsInfo = new Dictionary<Guid, List<RMTermIdentity>>();
            mSharePointRetiredTermsInfo = new Dictionary<Guid, List<RMTermIdentity>>();
        }

        private async System.Threading.Tasks.Task GetRetiredTermOfSPSiteAsync(RMSPTreeNode siteNode)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("BCSTermUsageReportProcessor.GetRetiredTermOfSPSite"))
            {
                try
                {
                    Guid termStoreId = Guid.Empty;
                    mLog.Info("Processing Site Collection of RetiredTermReport url:{0}", siteNode.FullPath);
                    mRMSharePointTax.InitClientContext(siteNode);
                    termStoreId = mRMSharePointTax.GetDefaultTermStoreId();

                    if (mSharePointRetiredTermsInfo.ContainsKey(termStoreId))
                    {
                        mLog.Info("The term store has aleady been checked.");
                    }
                    else
                    {
                        List<RMTerm> retiredTermsOfSP = await mRMSharePointTax.GetRetiredTermOfSharePointAsync(JobInfo.Id);
                        List<RMTermIdentity> termIdentitylist = new List<RMTermIdentity>();
                        if (retiredTermsOfSP != null && retiredTermsOfSP.Count > 0)
                        {
                            termIdentitylist = retiredTermsOfSP.ConvertAll<RMTermIdentity>
                                                                 ((term) => (new RMTermIdentity
                                                                 {
                                                                     UniqueId = term.UniqueId,
                                                                     Name = term.Name,
                                                                     FullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId),
                                                                     Status = GetRmTermStatus(term)
                                                                 }));
                        }
                        if (!mSharePointRetiredTermsInfo.ContainsKey(termStoreId))
                        {
                            mSharePointRetiredTermsInfo.Add(termStoreId, termIdentitylist);
                        }
                    }
                    mLog.Info("Processing site collection complete.");
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while GetRetiredTermsOfSharePoint,error message: {0}", e.ToString());
                }
            }
        }

        private async System.Threading.Tasks.Task GetOrphanTermOfSPSiteAsync(RMSPTreeNode siteNode)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("BCSTermUsageReportProcessor.GetOrphanTermOfSPSite"))
            {
                try
                {
                    mLog.Info("Processing Site Collection of OrphanedtermReport url:{0}", siteNode.FullPath);

                    mRMSharePointTax.InitClientContext(siteNode);
                    var termStoreId = mRMSharePointTax.GetDefaultTermStoreId();
                    if (mSharePointOrphanTermsInfo.ContainsKey(termStoreId))
                    {
                        mLog.Info("The term store has aleady been checked.");
                    }
                    else
                    {
                        List<RMTerm> orphanedTermsOfSP = await mRMSharePointTax.GetOrphanedTermOfSharePointAsync(JobInfo.Id);

                        List<RMTermIdentity> termIdentitylist = new List<RMTermIdentity>();
                        if (orphanedTermsOfSP != null && orphanedTermsOfSP.Count > 0)
                        {
                            termIdentitylist = orphanedTermsOfSP.ConvertAll<RMTermIdentity>
                                                                 ((term) => (new RMTermIdentity
                                                                 {
                                                                     UniqueId = term.UniqueId,
                                                                     Name = term.Name,
                                                                     FullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId),
                                                                     Status = GetRmTermStatus(term)
                                                                 }));
                        }
                        if (!mSharePointOrphanTermsInfo.ContainsKey(termStoreId))
                        {
                            mSharePointOrphanTermsInfo.Add(termStoreId, termIdentitylist);
                        }
                    }
                    mLog.Info("Processing site collection complete.");
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while GetOrphanedTermsOfSharePoint,error message: {0}", e.ToString());
                }
            }
        }

        public RMTermStatus GetRmTermStatus(RMTerm term)
        {
            if (term.IsSPRemoved)
            {
                return RMTermStatus.Removed;
            }
            else if (term.IsSPDeprecated)
            {
                return RMTermStatus.Retired;
            }
            else
            {
                if (term.IsRemoved)
                {
                    return RMTermStatus.Removed;
                }
                else
                {
                    return RMTermStatus.Retired;
                }
            }
        }

        private void SendJobReportSummary()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            //ReportManager.IncreaseBase(mUsageTermInfo.Values.Count);
            foreach (var term in mUsageTermInfo.Values)
            {
                //ReportManager.Increase();
                details.Add(new JMTermSelection()
                {
                    Term = term.Name,
                    TermFullPath = term.FullPath
                });
            }
            //if (isOrphanedTermReport && mSharePointOrphanTermsInfo.ContainsKey(curTermStoreId))
            //{
            //    foreach (var term in mSharePointOrphanTermsInfo[curTermStoreId])
            //    {
            //        details.Add(new JMTermSelection()
            //        {
            //            Term = term.Name,
            //            TermFullPath = term.FullPath
            //        });
            //    }
            //}
            ReportManager.BatchSendJobDetail(details);
        }

        private void SendJobReportSummaryOfSPOrphanTerm()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            if (mSharePointOrphanTermsInfo.ContainsKey(curTermStoreId))
            {
                foreach (var term in mSharePointOrphanTermsInfo[curTermStoreId])
                {
                    if (!mUsageTermInfo.Values.Contains(term) && !spOrphanedTermIds.Contains(term.UniqueId))
                    {
                        details.Add(new JMTermSelection()
                        {
                            Term = term.Name,
                            TermFullPath = term.FullPath
                        });
                        spOrphanedTermIds.Add(term.UniqueId);
                    }
                }
            }
            ReportManager.BatchSendJobDetail(details);
        }

        private void SendJobReportSummaryOfSPRetiredTerm()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            if (mSharePointRetiredTermsInfo.ContainsKey(curTermStoreId))
            {
                foreach (var term in mSharePointRetiredTermsInfo[curTermStoreId])
                {
                    if (!mUsageTermInfo.Values.Contains(term) && !spRetiredTermIds.Contains(term.UniqueId))
                    {
                        details.Add(new JMTermSelection()
                        {
                            Term = term.Name,
                            TermFullPath = term.FullPath
                        });
                        spRetiredTermIds.Add(term.UniqueId);
                    }
                }
            }
            ReportManager.BatchSendJobDetail(details);
        }

        protected override CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds)
        {
            CAMLManager cm = new CAMLManager();
            List<int> termWssIds = new List<int>();
            int tempCounter = 0;
            foreach (var termId in termIds)
            {
                List<int> wssids;
                if (isOrphanedTermReport)
                {
                    //RMTermIdentity spOrphanTerm = mSharePointOrphanTermsInfo.ContainsKey(curTermStoreId) ? mSharePointOrphanTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;
                    if ((mUsageTermInfo.ContainsKey(termId)) && GetWssidOfTermInTaxList(termId, out wssids))
                    {
                        //QueryCondition condition = QueryConditionFactory.GetTaxonomyQueryCondition(BCSColumnInternalName, new int[] { wssid }, Types.JoinTypes.Or);
                        //cm.QueryGroup.AddCondition(condition);
                        foreach (var wssid in wssids)
                        {
                            if (!termWssIds.Contains(wssid))
                            {
                                termWssIds.Add(wssid);
                            }
                        }
                    }
                }
                else if (mIsRetiredTermReport)
                {
                    // RMTermIdentity spRetiredTerm = mSharePointRetiredTermsInfo.ContainsKey(curTermStoreId) ? mSharePointRetiredTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;

                    if ((mUsageTermInfo.ContainsKey(termId)) && GetWssidOfTermInTaxList(termId, out wssids))
                    {
                        foreach (var wssid in wssids)
                        {
                            if (!termWssIds.Contains(wssid))
                            {
                                termWssIds.Add(wssid);
                            }
                        }
                    }

                }
                else
                {
                    if (mUsageTermInfo.ContainsKey(termId) && GetWssidOfTermInTaxList(termId, out wssids))
                    {
                        //QueryCondition condition = QueryConditionFactory.GetTaxonomyQueryCondition(BCSColumnInternalName, new int[] { wssid }, Types.JoinTypes.Or);
                        //cm.QueryGroup.AddCondition(condition);
                        foreach (var wssid in wssids)
                        {
                            if (!termWssIds.Contains(wssid))
                            {
                                termWssIds.Add(wssid);
                            }
                        }
                    }
                }

                tempCounter++;
                if (tempCounter >= 100)
                {
                    //UpdateJobWithoutProgressChange();
                    mLog.Info("Update job progress");
                    tempCounter = 0;
                }
            }

            if (termWssIds.Count > 0)
            {
                QueryCondition condition = QueryConditionFactory.GetTaxonomyQueryCondition(BCSColumnInternalName, termWssIds.ToArray(), Types.JoinTypes.Or);
                cm.QueryGroup.AddGroup(new QueryGroup(Types.JoinTypes.And, null, new List<QueryCondition> { condition }));
            }
            else
            {
                cm = null;
            }
            return cm;
        }
        protected override CAMLManager InitUnclassificationCamlQuery(IAveFieldCollection listFields, IAveWeb web, IAveList list, RMReportExtension reportExt)
        {
            return null;
        }
        private void GetTaxonomyHiddenListTerms(IAveWeb web)
        {
            if (!web.IsRootWeb)
            {
                return;
            }
            mWssidsInWeb.Clear();
            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.GetTaxonomyHiddenListTerms"))
            {
                try
                {
                    var taxonomyList = web.GetListByTitle("TaxonomyHiddenList");
                    string camlQueryForTermViewXml = CamlQuery.CreateAllItemsQuery().ViewXml;
                    var query = new AveCamlQuery() { QueryXml = camlQueryForTermViewXml };
                    AveItemCollectionPosition position = null;
                    do
                    {
                        query.ListItemCollectionPosition = position;
                        var termItems = taxonomyList.GetItems(query);
                        foreach (var termItem in termItems)
                        {
                            if (termItem["Title"] == null)
                            {
                                mLog.Warn("Term Title in TaxonomyHiddenList is null.TermGuid:[{0}] TermSetId:[{1}]", termItem["IdForTerm"].ToString(), termItem["IdForTermSet"]);
                                continue;
                            }
                            var idForTerm = new Guid(termItem["IdForTerm"].ToString());
                            int tempTermId = int.Parse(termItem["ID"].ToString());
                            if (mWssidsInWeb.ContainsKey(idForTerm))
                            {
                                if (!mWssidsInWeb[idForTerm].Contains(tempTermId))
                                {
                                    mWssidsInWeb[idForTerm].Add(tempTermId);
                                }
                            }
                            else
                            {
                                mWssidsInWeb.Add(idForTerm, new List<int>() { tempTermId });
                            }
                        }
                        position = termItems.ListItemCollectionPosition == null ? null : new AveItemCollectionPosition() { PagingInfo = termItems.ListItemCollectionPosition.PagingInfo };
                    } while (query.ListItemCollectionPosition != null);
                }
                catch (Exception e1)
                {
                    mLog.Warn("get wwsid for term error: {0}", e1.ToString());
                }
            }
        }

        private bool GetWssidOfTermInTaxList(Guid termId, out List<int> wssids)
        {
            return mWssidsInWeb.TryGetValue(termId, out wssids);
        }

        protected override int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items)
        {
            int results = 0;
            if (items != null && items.Count > 0)
            {
                ReportManager.IncreaseBase(items.Count);
                using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessItems"))
                {
                    int objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                    List<BaseReport> reports = new List<BaseReport>();
                    foreach (var discoverItem in items)
                    {
                        var item = discoverItem.CurrentItem;
                        ReportManager.Increase();
                        using (PerformanceScope scope0 = new PerformanceScope("BCSTermUsageReportProcessor.ProcessItem"))
                        {
                            mLog.Info("Process Item {0}", item.UniqueId);
                            BCSTermUsageReport report = new BCSTermUsageReport();
                            var isAddReport = true;
                            try
                            {
                                if (item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                                {
                                    objectLevel = (int)RMReportObjectLevel.Folder;
                                }
                                else
                                {
                                    objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                                }
                                using (CheckJobStopScope jScope = new CheckJobStopScope())
                                {
                                    report.TitleOrName = this.GetListItemName(item);
                                    report.Url = list.BaseType == AveBaseType.DocumentLibrary ? MakeFullUrl(web.Url, item.Url) : WebUtil.GetListItemRealPath(web.Url, list.RootFolder.ServerRelativeUrl, item.Url);
                                    report.ObjectLevel = objectLevel;
                                    report.CreatedBy = GetSingleUserFieldValue(item, "Author");
                                    report.CreatedTime = DateTime.Parse(item["Created"].ToString()).Ticks;
                                    //GetDateTimeFieldValue(item, "Created").Ticks;
                                    report.LastModifiedBy = GetSingleUserFieldValue(item, "Editor");
                                    report.LastModifiedTime = DateTime.Parse(item["Modified"].ToString()).Ticks;
                                    //GetDateTimeFieldValue(item, "Modified").Ticks;
                                    report.SPWebTimeZoneName = SPWebTimeZone.Description;

                                    Guid termId;
                                    string termName;
                                    if (GetSingleTaxonomyFieldValue(item, BCSColumnInternalName, out termId, out termName))
                                    {
                                        report.BCSTermId = termId.ToString();
                                        report.BCSTermName = termName;
                                        if (mUsageTermInfo.ContainsKey(termId))
                                        {
                                            report.TermStatus = mUsageTermInfo[termId].Status;
                                            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                                        }
                                        else
                                        {
                                            isAddReport = false;
                                        }
                                    }
                                }
                            }
                            catch (JobStopException ex)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Report item failed. item url: {0}, error message: {1}.", item.Url, ex.ToString());
                            }
                            finally
                            {
                                if (!CheckJobStatusUtility.isStopping)
                                {
                                    if (isAddReport)
                                    {
                                        ReportManager.SendJobReport(report);
                                        //reports.Add(report);
                                        results++;
                                    }
                                }
                                else
                                {
                                    ReportManager.SendJobReport(report);
                                    //SendJobReport(reports);
                                }
                            }
                        }
                    }
                    //SendJobReport(reports);
                }
            }
            return results;
        }

        protected override async System.Threading.Tasks.Task ProcessWebAppAsync(NodeItem webapp)
        {
            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessWebApp"))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        CheckNodeLevel(webapp, NodeLevel.WebApplication);
                        mLog.Info("Start web app process. fullPath: [{0}], isIncludeNew : [{1}].", webapp.FullPath, webapp.IncludeNew);
                        SetEvent();
                        mBCSColumnName = ReportService.GetMetaDataColumnName(mWebApplicationId);
                        if (string.IsNullOrEmpty(mBCSColumnName))
                        {
                            mLog.Warn("Web application metadate column is null or empty. Web app fullPath: [{0}].", webapp.FullPath);
                            return;
                        }
                        List<RMSPTreeNode> sites = await SPTreeService.BrowseAsync(webapp.TreeNode);
                        foreach (var site in sites)
                        {
                            NodeItem tempSite;
                            Guid siteId = new Guid(site.Id);
                            if (webapp.Children.TryGetValue(siteId, out tempSite))
                            {
                                if (AreThereProcessedChildren(tempSite))
                                {
                                    //if (isOrphanedTermReport)
                                    //{
                                    //    GetOrphanTermOfSPSite(site);
                                    //}
                                    //if (mIsRetiredTermReport)
                                    //{
                                    //    GetRetiredTermOfSPSite(site);
                                    //}
                                    //UpdateJobWithoutProgressChange();//更新job进度，防止因为数据量太大导致job超时

                                    await ProcessSiteAsync(tempSite);
                                    //UpdateJobProgress();
                                }
                                else if (tempSite.IsChecked)
                                {
                                    SendJobReportDetails(tempSite, JobDetailsStatus.Successful);
                                }
                                webapp.Children.Remove(siteId);
                            }
                            else if (webapp.IncludeNew)
                            {
                                //if (isOrphanedTermReport)
                                //{
                                //    GetOrphanTermOfSPSite(site);
                                //}
                                //if (mIsRetiredTermReport)
                                //{
                                //    GetRetiredTermOfSPSite(site);
                                //}
                                //UpdateJobWithoutProgressChange();//更新job进度，防止因为数据量太大导致job超时

                                site.CheckNumber = 1;
                                site.IncludeNew = 1;
                                await ProcessSiteAsync(new NodeItem(site, webapp));
                                //UpdateJobProgress();
                            }
                        }

                        if (webapp.Children.Count > 0)
                        {
                            foreach (var node in webapp.Children.Values)
                            {
                                if (node.IsChecked)
                                {
                                    mJobHasException = true;
                                    SendJobReportDetails(node, JobDetailsStatus.Failed, "RM_JM_Details_Failed_NodeRemovedFromGroup");
                                }
                            }
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    mLog.Error("An error occurred while prosess webapplication, fullPath is :{0}, error message: {1}.", webapp.FullPath, e.ToString());
                }
                finally
                {
                    ClearChildren(webapp);//Release children
                }
            }
        }

        protected override async System.Threading.Tasks.Task ProcessSiteAsync(NodeItem site)
        {


            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessSite"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    try
                    {
                        CurrentContainerNodeId = GetContainerNode(site)?.Id ?? Guid.Empty;
                        var remoteSite = RABrowserClient.GetSiteNode(site.FullPath);
                        if (!Guid.TryParse(remoteSite.ObjectId, out CurrentSiteCollectionNodeId))
                        {
                            mLog.Warn($"Can not convert site collection id to guid {remoteSite.ObjectId}");
                        }
                        if (site.IsChecked)
                        {
                            var siteCollectionRecord = ExplorerDao.QueryByPage(_ => _.ContainerId == CurrentContainerNodeId.ToString()
                               && _.ScopeId == CurrentSiteCollectionNodeId
                               && _.RecordStatus == (int)RMRecordStatus.Active && _.NodeType == (int)NodeLevel.SiteCollection && _.SourceFlag == (int)SourceFlag.SharePoint, 1, string.Empty).Item1.FirstOrDefault();
                            var termId = siteCollectionRecord?.TermId ?? Guid.Empty;
                            BCSTermUsageReport report = new BCSTermUsageReport();
                            bool sendReport = false;
                            if (mUsageTermInfo.ContainsKey(termId))
                            {
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = mUsageTermInfo[termId].Name;
                                report.TermStatus = mUsageTermInfo[termId].Status;
                                report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                                sendReport = true;
                            }
                            if (sendReport)
                            {
                                report.TitleOrName = siteCollectionRecord.LeafName;
                                report.Url = siteCollectionRecord.DirPath;
                                report.ObjectLevel = (int)RMReportObjectLevel.SiteCollection;
                                report.CreatedBy = siteCollectionRecord.CreatedBy;
                                report.CreatedTime = siteCollectionRecord.TimeCreated;
                                report.LastModifiedTime = siteCollectionRecord.TimeModified;
                                ReportManager.SendJobReport(report);
                            }

                        }
                        await base.ProcessSiteAsync(site);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("An error occurred while get DefaultSiteCollectionTermStore in orphanedTermReport,site fullPath is :{0}, error message: {1}.", site.FullPath, e.ToString());
                    }
                }
            }
        }

        private NodeItem GetContainerNode(NodeItem node)
        {
            if (node.NodeLevel == NodeLevel.WebApplication)
            {
                return node;
            }
            return GetContainerNode(node.Parent);
        }

        protected override async System.Threading.Tasks.Task ProcessWebAsync(NodeItem web, bool IsProcessLists)
        {
            if (!SharePointSettingDao.GetSettingEnableInfoByScope(groupId, siteId, web.Id))
            {
                SendJobReportDetails(web, JobDetailsStatus.Skipped, "RM_JS_JMD_DisableRecordManagement");
                mLog.Info("Process web sharepoint setting is disable {0}", web.FullPath);
                return;
            }
            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessWeb"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverWeb = web.DiscoverObj as IAveWeb;
                    if (discoverWeb.IsRootWeb)
                    {
                        mLog.Info("No need to report root web {0}", discoverWeb.Url);
                    }
                    //discoverWeb.Properties.ContainsKey("RevIM") &&
                    else if (web.IsChecked)
                    {
                        var webRecord = ExplorerDao.QueryByPage(_ => _.ContainerId == CurrentContainerNodeId.ToString()
                            && _.ScopeId == CurrentSiteCollectionNodeId && _.WebId == web.Id
                            && _.RecordStatus == (int)RMRecordStatus.Active && _.NodeType == (int)NodeLevel.Site && _.SourceFlag == (int)SourceFlag.SharePoint, 1, string.Empty).Item1.FirstOrDefault();
                        var termId = webRecord?.TermId ?? Guid.Empty;
                        BCSTermUsageReport report = new BCSTermUsageReport();
                        bool sendReport = false;
                        if (mUsageTermInfo.ContainsKey(termId))
                        {
                            report.BCSTermId = termId.ToString();
                            report.BCSTermName = mUsageTermInfo[termId].Name;
                            report.TermStatus = mUsageTermInfo[termId].Status;
                            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                            sendReport = true;
                        }
                        if (sendReport && webRecord != null)
                        {
                            report.TitleOrName = webRecord.LeafName;
                            report.Url = webRecord.DirPath;
                            report.ObjectLevel = (int)RMReportObjectLevel.Site;
                            report.CreatedBy = webRecord.CreatedBy;
                            report.CreatedTime = webRecord.TimeCreated;
                            report.LastModifiedTime = webRecord.TimeModified;
                            ReportManager.SendJobReport(report);
                        }
                    }
                    await base.ProcessWebAsync(web, IsProcessLists);
                }

            }
        }
        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem list)
        {
            if (!SharePointSettingDao.GetSettingEnableInfoByScope(groupId, siteId, list.Id))
            {
                SendJobReportDetails(list, JobDetailsStatus.Skipped, "RM_JS_JMD_DisableRecordManagement");
                mLog.Info("Process list sharepoint setting is disable {0}", list.FullPath);
                return;
            }
            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessList"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverList = list.DiscoverObj as IAveList;
                    Guid webId = discoverList?.ParentWeb?.ID ?? Guid.Empty;
                    Guid listId = discoverList?.ID ?? Guid.Empty;
                    var listRecord = ExplorerDao.QueryByPage(_ => _.ContainerId == CurrentContainerNodeId.ToString()
                            && _.ScopeId == CurrentSiteCollectionNodeId && _.WebId == webId && _.ListId == listId
                            && _.RecordStatus == (int)RMRecordStatus.Active && _.NodeType == (int)NodeLevel.List && _.SourceFlag == (int)SourceFlag.SharePoint, 1, string.Empty).Item1.FirstOrDefault();
                    if (list.IsChecked)
                    {
                        var termId = listRecord?.TermId ?? Guid.Empty;
                        BCSTermUsageReport report = new BCSTermUsageReport();
                        bool sendReport = false;
                        if (mUsageTermInfo.ContainsKey(termId))
                        {
                            report.BCSTermId = termId.ToString();
                            report.BCSTermName = mUsageTermInfo[termId].Name;
                            report.TermStatus = mUsageTermInfo[termId].Status;
                            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                            sendReport = true;
                        }
                        if (sendReport && listRecord != null)
                        {
                            report.TitleOrName = listRecord.LeafName;
                            report.Url = MakeFullUrl(discoverList.ParentWeb.Url, discoverList.RootFolder.Url);
                            report.ObjectLevel = (int)RMReportObjectLevel.List;
                            report.CreatedBy = listRecord.CreatedBy;
                            report.CreatedTime = listRecord.TimeCreated;
                            report.LastModifiedTime = listRecord.TimeModified;
                            ReportManager.SendJobReport(report);
                        }
                    }
                    list.NameOrTitle = listRecord?.LeafName ?? discoverList.Title;
                    await ProcessItemsUnderList(list);
                }
            }
        }

        private async Task ProcessItemsUnderList(NodeItem list)
        {
            using (PerformanceScope scope0 = new PerformanceScope("BCSTermUsageReportProcessor.ProcessItemsUnderList", $"BCSTermUsageReportProcessor.ProcessItemsUnderList.[{list.NameOrTitle}]", addToStatistics: true))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        CheckNodeLevel(list, NodeLevel.List);
                        var discoverList = list.DiscoverObj as IAveList;
                        var rootFolder = discoverList.RootFolder;
                        var discoverWeb = discoverList.ParentWeb;
                        list.FullPath = MakeFullUrl(discoverWeb.Url, rootFolder.Url);

                        var total = await ReportItemUnderTheList(list);

                        SendJobReportDetails(list, JobDetailsStatus.Successful, total > 0 ? "" : "RM_JM_Details_Sucess_NoMachedList");
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    SendJobReportDetails(list, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    mLog.Error("An error occurred while processing list level node: {0}, error message: {1}.", list.FullPath, e.ToString());
                }
                finally
                {
                    SafeDisposeObject(list.DiscoverObj);
                    ClearChildren(list);
                }
            }
        }

        private async Task<int> ReportItemUnderTheList(NodeItem list)
        {
            string pageIndex = string.Empty;
            var discoverList = list.DiscoverObj as IAveList;
            Guid webId = discoverList?.ParentWeb?.ID ?? Guid.Empty;
            Guid listId = discoverList?.ID ?? Guid.Empty;
            var total = 0;
            Tuple<IEnumerable<Record>, string> queryResult = null;
            do
            {
                queryResult = ExplorerDao.QueryByPage(_ => _.ContainerId == CurrentContainerNodeId.ToString()
                    && _.ScopeId == CurrentSiteCollectionNodeId && _.WebId == webId && _.ListId == listId && mUsageTermId.Contains(_.TermId)
                    && _.RecordStatus == (int)RMRecordStatus.Active && _.SourceFlag == (int)SourceFlag.SharePoint && (_.NodeType == (int)NodeLevel.Folder || _.NodeType == (int)NodeLevel.Item), 1000, pageIndex);
                total += ProcessItems(queryResult.Item1.ToList(), discoverList);
                pageIndex = queryResult.Item2;
            } while (!string.IsNullOrEmpty(queryResult.Item2));
            return total;
        }

        protected int ProcessItems(List<Record> items, IAveList list)
        {
            int results = 0;
            var discoverWeb = list.ParentWeb;
            var rootUrl = discoverWeb.ServerRelativeUrl.Equals("/") ? discoverWeb.Url : discoverWeb.Url.Substring(0, discoverWeb.Url.Length - discoverWeb.ServerRelativeUrl.Length);
            if (items != null && items.Count > 0)
            {
                ReportManager.IncreaseBase(items.Count);
                using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessItems"))
                {
                    int objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                    List<BaseReport> reports = new List<BaseReport>();
                    foreach (var record in items)
                    {
                        ReportManager.Increase();
                        using (PerformanceScope scope0 = new PerformanceScope("BCSTermUsageReportProcessor.ProcessItem"))
                        {
                            mLog.Info("Process Item {0}", record.Id);
                            BCSTermUsageReport report = new BCSTermUsageReport();
                            var isAddReport = true;
                            try
                            {
                                if (record.NodeType == (int)RMNodeLevel.Folder)
                                {
                                    objectLevel = (int)RMReportObjectLevel.Folder;
                                }
                                else
                                {
                                    objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                                }
                                using (CheckJobStopScope jScope = new CheckJobStopScope())
                                {
                                    report.TitleOrName = record.LeafName;
                                    report.Url = list.BaseType == AveBaseType.DocumentLibrary ? MakeFullUrl(rootUrl, record.DirPath) : WebUtil.GetListItemRealPath(rootUrl, list.RootFolder.ServerRelativeUrl, record.DirPath);
                                    report.ObjectLevel = objectLevel;
                                    report.CreatedBy = record.CreatedBy;
                                    report.CreatedTime = record.TimeCreated;
                                    report.LastModifiedBy = record.ModifiedBy;
                                    report.LastModifiedTime = record.TimeModified;

                                    Guid termId = record.TermId;
                                    report.BCSTermId = termId.ToString();
                                    if (mUsageTermInfo.ContainsKey(termId))
                                    {
                                        report.BCSTermName = mUsageTermInfo[termId].Name;
                                        report.TermStatus = mUsageTermInfo[termId].Status;
                                        report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                                    }
                                    else
                                    {
                                        isAddReport = false;
                                    }
                                }
                            }
                            catch (JobStopException ex)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Report item failed. item url: {0}, error message: {1}.", record.DirPath, ex.ToString());
                            }
                            finally
                            {

                                if (!CheckJobStatusUtility.isStopping)
                                {
                                    if (isAddReport)
                                    {
                                        ReportManager.SendJobReport(report);
                                        results++;
                                    }
                                }
                                else
                                {
                                    ReportManager.SendJobReport(report);
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }
    }
}
