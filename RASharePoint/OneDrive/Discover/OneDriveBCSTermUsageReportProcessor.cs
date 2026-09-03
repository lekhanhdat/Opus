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
using AvePoint.RA.SharePoint.Discover.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.Wrapper.Common;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.GCommon.Contract.Tree.Object;
using System.Reflection;
using System.Diagnostics;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.RA.SharePoint.Common;
using Microsoft.SharePoint.Client;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.RA.SharePoint.OneDrive.Discover.Base;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.SharePoint.Discover
{
    public class OneDriveBCSTermUsageReportProcessor : RMOneDriveReportProcessor
    {
        private Dictionary<Guid, RMTermIdentity> mUsageTermInfo;
        private bool isOrphanedTermReport;

        #region For retired Term
        private bool mIsRetiredTermReport;
        //private Dictionary<Guid, List<RMTermIdentity>> mSharePointRetiredTermsInfo;
        //private List<Guid> spRetiredTermIds = new List<Guid>();
        #endregion

        private ITermDao TermDao;

        public OneDriveBCSTermUsageReportProcessor(string jobId, string profileId, bool IsOrphanedTermReport, bool isRetiredTermReport)
            : base(jobId, (int)JobType.OneDriveTermUsageReport, IsOrphanedTermReport)
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
                //if (isOrphanedTermReport || mIsRetiredTermReport)
                //{
                //    //using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.InitSPTaxnomy"))
                //    //{
                //    //    InitSPTaxnomy();
                //    //}
                //}
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

        //private void SendJobReportSummaryOfSPOrphanTerm()
        //{
        //    List<JMJobDetails> details = new List<JMJobDetails>();
        //    if (mSharePointOrphanTermsInfo.ContainsKey(curTermStoreId))
        //    {
        //        foreach (var term in mSharePointOrphanTermsInfo[curTermStoreId])
        //        {
        //            if (!mUsageTermInfo.Values.Contains(term) && !spOrphanedTermIds.Contains(term.UniqueId))
        //            {
        //                details.Add(new JMTermSelection()
        //                {
        //                    Term = term.Name,
        //                    TermFullPath = term.FullPath
        //                });
        //                spOrphanedTermIds.Add(term.UniqueId);
        //            }
        //        }
        //    }
        //    ReportManager.BatchSendJobDetail(details);
        //}

        //private void SendJobReportSummaryOfSPRetiredTerm()
        //{
        //    List<JMJobDetails> details = new List<JMJobDetails>();
        //    if (mSharePointRetiredTermsInfo.ContainsKey(curTermStoreId))
        //    {
        //        foreach (var term in mSharePointRetiredTermsInfo[curTermStoreId])
        //        {
        //            if (!mUsageTermInfo.Values.Contains(term) && !spRetiredTermIds.Contains(term.UniqueId))
        //            {
        //                details.Add(new JMTermSelection()
        //                {
        //                    Term = term.Name,
        //                    TermFullPath = term.FullPath
        //                });
        //                spRetiredTermIds.Add(term.UniqueId);
        //            }
        //        }
        //    }
        //    ReportManager.BatchSendJobDetail(details);
        //}


        //private void GetTaxonomyHiddenListTerms(IAveWeb web)
        //{
        //    if (!web.IsRootWeb)
        //    {
        //        return;
        //    }
        //    mWssidsInWeb.Clear();
        //    using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.GetTaxonomyHiddenListTerms"))
        //    {
        //        try
        //        {
        //            var taxonomyList = web.GetListByTitle("TaxonomyHiddenList");
        //            string camlQueryForTermViewXml = CamlQuery.CreateAllItemsQuery().ViewXml;
        //            var query = new AveCamlQuery() { QueryXml = camlQueryForTermViewXml };
        //            AveItemCollectionPosition position = null;
        //            do
        //            {
        //                query.ListItemCollectionPosition = position;
        //                var termItems = taxonomyList.GetItems(query);
        //                foreach (var termItem in termItems)
        //                {
        //                    if (termItem["Title"] == null)
        //                    {
        //                        mLog.Warn("Term Title in TaxonomyHiddenList is null.TermGuid:[{0}] TermSetId:[{1}]", termItem["IdForTerm"].ToString(), termItem["IdForTermSet"]);
        //                        continue;
        //                    }
        //                    var idForTerm = new Guid(termItem["IdForTerm"].ToString());
        //                    if (!mWssidsInWeb.ContainsKey(idForTerm))
        //                    {
        //                        mWssidsInWeb.Add(idForTerm, int.Parse(termItem["ID"].ToString()));
        //                    }
        //                }
        //                position = termItems.ListItemCollectionPosition == null ? null : new AveItemCollectionPosition() { PagingInfo = termItems.ListItemCollectionPosition.PagingInfo };
        //            } while (query.ListItemCollectionPosition != null);
        //        }
        //        catch (Exception e1)
        //        {
        //            mLog.Warn("get wwsid for term error: {0}", e1.ToString());
        //        }
        //    }
        //}

        //private bool GetWssidOfTermInTaxList(Guid termId, out int wssid)
        //{
        //    return mWssidsInWeb.TryGetValue(termId, out wssid);
        //}

        protected override int ProcessItems(IAveWeb web, IAveList list, List<BaseRecordDto> items)
        {
            int results = 0;
            if (items != null && items.Count > 0)
            {
                ReportManager.IncreaseBase(items.Count);
                using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessItems"))
                {
                    int objectLevel = (int)RMReportObjectLevel.Document;
                    List<BaseReport> reports = new List<BaseReport>();
                    foreach (var discoverItem in items)
                    {
                        ReportManager.Increase();
                        using (PerformanceScope scope0 = new PerformanceScope("BCSTermUsageReportProcessor.ProcessItem"))
                        {
                            mLog.Info("Process Item {0}", discoverItem.ItemRowId);
                            BCSTermUsageReport report = new BCSTermUsageReport();
                            var isAddReport = true;
                            try
                            {
                                using (CheckJobStopScope jScope = new CheckJobStopScope())
                                {
                                    report.TitleOrName = discoverItem.LeafName;
                                    report.Url = WebUtil.MakeFullUrl(web.Url, discoverItem.DirPath);
                                    report.ObjectLevel = objectLevel;
                                    report.CreatedBy = discoverItem.CreatedBy;
                                    report.CreatedTime = GetDateTimeFromUtc(discoverItem.TimeCreated, web).Ticks;
                                    report.LastModifiedBy = discoverItem.ModifiedBy;
                                    report.LastModifiedTime = GetDateTimeFromUtc(discoverItem.TimeLastModified, web).Ticks;
                                    report.SPWebTimeZoneName = SPWebTimeZone.Description;

                                    Guid termId;
                                    string termName;
                                    if (discoverItem.TermId != Guid.Empty)
                                    {
                                        termId = discoverItem.TermId;
                                        termName = discoverItem.TermName;
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
                                mLog.Warn("Report item failed. item id: {0}, error message: {1}.", discoverItem.ItemRowId, ex.ToString());
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
            //if (!SharePointSettingDao.GetSettingEnableInfoByScope(groupId, siteId, site.Id))
            //{
            //    SendJobReportDetails(site, JobDetailsStatus.Skipped, I18N.Core.I18NEntity.GetString("RM_JS_JMD_DisableRecordManagement"));
            //    mLog.Info("Process site sharepoint setting is disable {0}", site.FullPath);
            //    return;
            //}
            //site collection判断在父类构造方法，添加SiteCollectionNodeItems逻辑中

            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessSite"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    try
                    {
                        try
                        {
                            var mfactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, PoolUserUtil.GetAveBPOSAccountInfo(site.BposInfo, site.FullPath), AveContextKind.ClientObjectModel);
                            Site = mfactory.CreateSite(site.FullPath);
                            //IAveTaxonomySession session = Site.AveSPTaxonomySession;
                            //if (Site.AveSPTaxonomySession == null)
                            //{
                            //    throw new Exception("session is null");
                            //}
                            //IAveTermStore termStore = session.DefaultSiteCollectionTermStore;
                            //if (termStore == null)
                            //{
                            //    termStore = session.TermStores[0];
                            //    if (termStore == null)
                            //    {
                            //        mLog.Info("termstore is null");
                            //    }
                            //}
                            //curTermStoreId = termStore.ID;
                            //if (isOrphanedTermReport)
                            //{
                            //   // GetOrphanTermOfSPSite(site.TreeNode);
                            //    SendJobReportSummaryOfSPOrphanTerm();
                            //}
                            //if (mIsRetiredTermReport)
                            //{
                            //   // GetRetiredTermOfSPSite(site.TreeNode);
                            //    SendJobReportSummaryOfSPRetiredTerm();
                            //}
                        }
                        catch (Exception e)
                        {
                            mLog.Error("Outer process site error:" + e.ToString());
                        }

                        //if (Site.RootWeb.Properties.ContainsKey("RevIM") && site.IsChecked)
                        //{
                        //    var termId = new Guid(Site.RootWeb.Properties["RevIM"].ToString());
                        //    BCSTermUsageReport report = new BCSTermUsageReport();
                        //    bool sendReport = false;
                        //    if (isOrphanedTermReport)
                        //    {
                        //        //RMTermIdentity spOrphanTerm = mSharePointOrphanTermsInfo.ContainsKey(curTermStoreId) ? mSharePointOrphanTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;
                        //        //if (spOrphanTerm != null)
                        //        //{
                        //        //    report.BCSTermId = spOrphanTerm.UniqueId.ToString();
                        //        //    report.BCSTermName = spOrphanTerm.Name;
                        //        //    report.TermStatus = spOrphanTerm.Status;
                        //        //    report.BCSTermFullPath = spOrphanTerm.FullPath;
                        //        //    sendReport = true;
                        //        //}
                        //        //else
                        //        //{
                        //        if (mUsageTermInfo.ContainsKey(termId))
                        //        {
                        //            report.BCSTermId = termId.ToString();
                        //            report.BCSTermName = mUsageTermInfo[termId].Name;
                        //            report.TermStatus = mUsageTermInfo[termId].Status;
                        //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                        //            sendReport = true;
                        //        }
                        //        // }
                        //    }
                        //    else if (mIsRetiredTermReport)
                        //    {
                        //        //RMTermIdentity spRetiredTerm = mSharePointRetiredTermsInfo.ContainsKey(curTermStoreId) ? mSharePointRetiredTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;
                        //        //if (spRetiredTerm != null)
                        //        //{
                        //        //    report.BCSTermId = spRetiredTerm.UniqueId.ToString();
                        //        //    report.BCSTermName = spRetiredTerm.Name;
                        //        //    report.TermStatus = spRetiredTerm.Status;
                        //        //    report.BCSTermFullPath = spRetiredTerm.FullPath;
                        //        //    sendReport = true;
                        //        //}
                        //        //else
                        //        //{
                        //        if (mUsageTermInfo.ContainsKey(termId))
                        //        {
                        //            report.BCSTermId = termId.ToString();
                        //            report.BCSTermName = mUsageTermInfo[termId].Name;
                        //            report.TermStatus = mUsageTermInfo[termId].Status;
                        //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                        //            sendReport = true;
                        //        }
                        //        //}
                        //    }
                        //    else
                        //    {
                        //        if (mUsageTermInfo.ContainsKey(termId))
                        //        {
                        //            report.BCSTermId = termId.ToString();
                        //            report.BCSTermName = mUsageTermInfo[termId].Name;
                        //            report.TermStatus = mUsageTermInfo[termId].Status;
                        //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                        //            sendReport = true;
                        //        }
                        //    }
                        //    if (sendReport)
                        //    {
                        //        SPWebTimeZone = Site.RootWeb.RegionalSettings.TimeZone;
                        //        report.TitleOrName = Site.RootWeb.Title;
                        //        report.Url = Site.Url;
                        //        report.ObjectLevel = (int)RMReportObjectLevel.SiteCollection;

                        //        //if (Site.RootWeb.Author != null)
                        //        //{
                        //        //    report.CreatedBy = Site.RootWeb.Author.Name;
                        //        //}

                        //        if (Site.Owner.PrincipalType != AvePrincipalType.User)
                        //        {
                        //            try
                        //            {
                        //                report.CreatedBy = Site.Owner.Email.ToLowerInvariant();
                        //            }
                        //            catch (Exception e)
                        //            {
                        //                report.CreatedBy = Site.Owner.Name;
                        //                mLog.Warn($"get owner.Email error, use Name Property. Exception: {e}");
                        //            }
                        //        }
                        //        else
                        //        {
                        //            Int32 index = Site.Owner.NoPrefixLoginName.IndexOf("|");
                        //            if (index != -1)
                        //            {
                        //                report.CreatedBy = Site.Owner.NoPrefixLoginName.Substring(index + 1);
                        //            }
                        //        }
                        //        report.CreatedTime = GetDateTimeValue(Site.RootWeb.Created).Ticks;
                        //        //report.LastModifiedBy = Site.RootWeb.e;
                        //        report.LastModifiedTime = GetDateTimeValue(Site.RootWeb.LastItemModifiedDate).Ticks;
                        //        report.SPWebTimeZoneName = SPWebTimeZone.Description;
                        //        ReportManager.SendJobReport(report);
                        //        //List<BaseReport> reports = new List<BaseReport>();
                        //        //reports.Add(report);
                        //        //SendJobReport(reports);
                        //    }

                        //}
                        await base.ProcessSiteAsync(site);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("An error occurred while get DefaultSiteCollectionTermStore in orphanedTermReport,site fullPath is :{0}, error message: {1}.", site.FullPath, e.ToString());
                    }
                }
            }
        }
        protected override async System.Threading.Tasks.Task ProcessWebAsync(NodeItem web, bool IsProcessLists)
        {
            if (!OneDriveSettingDao.GetSettingEnableInfoByScope(groupId, siteId, web.Id))
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
                    //if (discoverWeb.IsRootWeb)
                    //{
                    //    mLog.Info("No need to report root web {0}", discoverWeb.Url);
                    //}
                    //else if (discoverWeb.Properties.ContainsKey("RevIM") && web.IsChecked)
                    //{
                    //    var termId = new Guid(discoverWeb.Properties["RevIM"].ToString());
                    //    BCSTermUsageReport report = new BCSTermUsageReport();
                    //    bool sendReport = false;
                    //    if (isOrphanedTermReport)
                    //    {
                    //        //RMTermIdentity spOrphanTerm = mSharePointOrphanTermsInfo.ContainsKey(curTermStoreId) ? mSharePointOrphanTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;
                    //        //if (spOrphanTerm != null)
                    //        //{
                    //        //    report.BCSTermId = spOrphanTerm.UniqueId.ToString();
                    //        //    report.BCSTermName = spOrphanTerm.Name;
                    //        //    report.TermStatus = spOrphanTerm.Status;
                    //        //    report.BCSTermFullPath = spOrphanTerm.FullPath;
                    //        //    sendReport = true;
                    //        //}
                    //        //else
                    //        //{
                    //        if (mUsageTermInfo.ContainsKey(termId))
                    //        {
                    //            report.BCSTermId = termId.ToString();
                    //            report.BCSTermName = mUsageTermInfo[termId].Name;
                    //            report.TermStatus = mUsageTermInfo[termId].Status;
                    //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                    //            sendReport = true;
                    //        }
                    //        //}
                    //    }
                    //    else if (mIsRetiredTermReport)
                    //    {
                    //        //RMTermIdentity spRetiredTerm = mSharePointRetiredTermsInfo.ContainsKey(curTermStoreId) ? mSharePointRetiredTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;
                    //        //if (spRetiredTerm != null)
                    //        //{
                    //        //    report.BCSTermId = spRetiredTerm.UniqueId.ToString();
                    //        //    report.BCSTermName = spRetiredTerm.Name;
                    //        //    report.TermStatus = spRetiredTerm.Status;
                    //        //    report.BCSTermFullPath = spRetiredTerm.FullPath;
                    //        //    sendReport = true;
                    //        //}
                    //        //else
                    //        //{
                    //        if (mUsageTermInfo.ContainsKey(termId))
                    //        {
                    //            report.BCSTermId = termId.ToString();
                    //            report.BCSTermName = mUsageTermInfo[termId].Name;
                    //            report.TermStatus = mUsageTermInfo[termId].Status;
                    //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                    //            sendReport = true;
                    //        }
                    //        //}
                    //    }
                    //    else
                    //    {
                    //        if (mUsageTermInfo.ContainsKey(termId))
                    //        {
                    //            report.BCSTermId = termId.ToString();
                    //            report.BCSTermName = mUsageTermInfo[termId].Name;
                    //            report.TermStatus = mUsageTermInfo[termId].Status;
                    //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                    //            sendReport = true;
                    //        }
                    //    }
                    //    if (sendReport)
                    //    {
                    //        SPWebTimeZone = discoverWeb.RegionalSettings.TimeZone;
                    //        report.TitleOrName = discoverWeb.Title;
                    //        report.Url = discoverWeb.Url;
                    //        report.ObjectLevel = (int)RMReportObjectLevel.Site;
                    //        if (discoverWeb.Author != null)
                    //        {
                    //            report.CreatedBy = discoverWeb.Author.Name;
                    //        }

                    //        report.CreatedTime = GetDateTimeValue(discoverWeb.Created).Ticks;
                    //        //report.LastModifiedBy = Site.RootWeb.e;
                    //        report.LastModifiedTime = GetDateTimeValue(discoverWeb.LastItemModifiedDate).Ticks;
                    //        report.SPWebTimeZoneName = SPWebTimeZone.Description;
                    //        ReportManager.SendJobReport(report);
                    //        //List<BaseReport> reports = new List<BaseReport>();
                    //        //reports.Add(report);
                    //        //SendJobReport(reports);
                    //    }
                    //}
                    //GetTaxonomyHiddenListTerms(discoverWeb);
                    await base.ProcessWebAsync(web, IsProcessLists);
                }

            }
        }
        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem list)
        {
            if (!OneDriveSettingDao.GetSettingEnableInfoByScope(groupId, siteId, list.Id))
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
                    //if (discoverList.RootFolder.Properties.ContainsKey("RevIM") && list.IsChecked)
                    //{
                    //    var termId = new Guid(discoverList.RootFolder.Properties["RevIM"].ToString());
                    //    BCSTermUsageReport report = new BCSTermUsageReport();
                    //    bool sendReport = false;
                    //    if (isOrphanedTermReport)
                    //    {
                    //        //RMTermIdentity spOrphanTerm = mSharePointOrphanTermsInfo.ContainsKey(curTermStoreId) ? mSharePointOrphanTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;
                    //        //if (spOrphanTerm != null)
                    //        //{
                    //        //    report.BCSTermId = spOrphanTerm.UniqueId.ToString();
                    //        //    report.BCSTermName = spOrphanTerm.Name;
                    //        //    report.TermStatus = spOrphanTerm.Status;
                    //        //    report.BCSTermFullPath = spOrphanTerm.FullPath;
                    //        //    sendReport = true;
                    //        //}
                    //        //else
                    //        //{
                    //        if (mUsageTermInfo.ContainsKey(termId))
                    //        {
                    //            report.BCSTermId = termId.ToString();
                    //            report.BCSTermName = mUsageTermInfo[termId].Name;
                    //            report.TermStatus = mUsageTermInfo[termId].Status;
                    //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                    //            sendReport = true;
                    //        }
                    //        //}
                    //    }
                    //    else if (mIsRetiredTermReport)
                    //    {
                    //        //RMTermIdentity spRetiredTerm = mSharePointRetiredTermsInfo.ContainsKey(curTermStoreId) ? mSharePointRetiredTermsInfo[curTermStoreId].SingleOrDefault(l => l.UniqueId.Equals(termId)) : null;
                    //        //if (spRetiredTerm != null)
                    //        //{
                    //        //    report.BCSTermId = spRetiredTerm.UniqueId.ToString();
                    //        //    report.BCSTermName = spRetiredTerm.Name;
                    //        //    report.TermStatus = spRetiredTerm.Status;
                    //        //    report.BCSTermFullPath = spRetiredTerm.FullPath;
                    //        //    sendReport = true;
                    //        //}
                    //        //else
                    //        //{
                    //        if (mUsageTermInfo.ContainsKey(termId))
                    //        {
                    //            report.BCSTermId = termId.ToString();
                    //            report.BCSTermName = mUsageTermInfo[termId].Name;
                    //            report.TermStatus = mUsageTermInfo[termId].Status;
                    //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                    //            sendReport = true;
                    //        }
                    //        //}
                    //    }
                    //    else
                    //    {
                    //        if (mUsageTermInfo.ContainsKey(termId))
                    //        {
                    //            report.BCSTermId = termId.ToString();
                    //            report.BCSTermName = mUsageTermInfo[termId].Name;
                    //            report.TermStatus = mUsageTermInfo[termId].Status;
                    //            report.BCSTermFullPath = mUsageTermInfo[termId].FullPath;
                    //            sendReport = true;
                    //        }
                    //    }
                    //    if (sendReport)
                    //    {
                    //        report.TitleOrName = discoverList.Title;
                    //        report.Url = MakeFullUrl(discoverList.ParentWeb.Url, discoverList.RootFolder.Url);
                    //        report.ObjectLevel = (int)RMReportObjectLevel.List;
                    //        if (discoverList.Author != null)
                    //        {
                    //            report.CreatedBy = discoverList.Author.Name;
                    //        }
                    //        report.CreatedTime = GetDateTimeValue(discoverList.Created).Ticks;
                    //        //report.LastModifiedBy = Site.RootWeb.e;
                    //        report.LastModifiedTime = GetDateTimeValue(discoverList.LastItemModifiedDate).Ticks;
                    //        report.SPWebTimeZoneName = SPWebTimeZone.Description;
                    //        ReportManager.SendJobReport(report);
                    //        //List<BaseReport> reports = new List<BaseReport>();
                    //        //reports.Add(report);
                    //        //SendJobReport(reports);
                    //    }
                    //}
                    await base.ProcessListAsync(list);
                }
            }
        }
    }
}
