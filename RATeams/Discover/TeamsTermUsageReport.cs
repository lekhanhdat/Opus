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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.CAMLHelper.CAML;
using AvePoint.RA.RACommonUtility.CAMLHelper.General;
using AvePoint.RA.RACommonUtility.Model;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Protobuf.Collections;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using RATeams.Discover.Base;
using RATeams.RMSharePointTaxnomy;
using static Google.Cloud.AIPlatform.V1.GroundingChunk.Types;

namespace AvePoint.RA.SharePoint.Teams.Discover
{
    public class TeamsBCSTermUsageReportProcessor : RMTeamsReportProcessor
    {
        private IExplorerDao ExplorerDao = new ExplorerDao();
        private Dictionary<Guid, RMTermIdentity> _usageTermInfo;
        private List<Guid> _usageTermIds;
        private Dictionary<Guid, List<RMTermIdentity>> _teamsOrphanTermsInfo;
        private bool isTeamsOrphanedTermReport;

        #region For retired Term
        private bool isTeamsRetiredTermReport;
        private Dictionary<Guid, List<RMTermIdentity>> _teamsRetiredTermsInfo;
        private List<Guid> teamsRetiredTermIds = new List<Guid>();
        #endregion

        private Guid curTermStoreId;
        private RMSharePointTaxonomy _RMTeamsTax;
        private ITermDao TermDao;

        private List<Guid> teamsOrphanedTermIds = new List<Guid>();
        private Dictionary<Guid, List<int>> mWssidsInWeb = new Dictionary<Guid, List<int>>();

        private Guid CurrentContainerNodeId;
        private Guid CurrentSiteCollectionNodeId;

        public TeamsBCSTermUsageReportProcessor(string jobId, string profileId, bool IsOrphanedTermReport, bool isRetiredTermReport) :
            base(jobId, JobType.TeamsBCSTermUsageReport, IsOrphanedTermReport)
        {
            RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
            isTeamsOrphanedTermReport = IsOrphanedTermReport;
            isTeamsRetiredTermReport = isRetiredTermReport;
            if (IsOrphanedTermReport)
            {
                _usageTermInfo = ReportService.GetOrphanedTermsOfRMAsync().Result;
            }
            else if (isRetiredTermReport)
            {
                _usageTermInfo = ReportService.GetRetiredTermsOfRMAsync().Result;
            }
            else
            {
                _usageTermInfo = ReportService.GetTermIDsFromBCSTermTreeAsync(profile.Extension1).Result;
            }
            _usageTermIds = _usageTermInfo.Select(_ => _.Key).ToList();
            TermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            SendJobReportSummary();
        }
        private void SendJobReportSummary()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            foreach (var term in _usageTermInfo.Values)
            {
                details.Add(new JMTermSelection()
                {
                    Term = term.Name,
                    TermFullPath = term.FullPath
                });
            }
            ReportManager.BatchSendJobDetail(details);
        }
        private void SendJobReportSummaryOfSPOrphanTerm()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            if (_teamsOrphanTermsInfo.ContainsKey(curTermStoreId))
            {
                foreach (var term in _teamsOrphanTermsInfo[curTermStoreId])
                {
                    if (!_usageTermInfo.Values.Contains(term) && !teamsOrphanedTermIds.Contains(term.UniqueId))
                    {
                        details.Add(new JMTermSelection()
                        {
                            Term = term.Name,
                            TermFullPath = term.FullPath
                        });
                        teamsOrphanedTermIds.Add(term.UniqueId);
                    }
                }
            }
            ReportManager.BatchSendJobDetail(details);
        }
        private void SendJobReportSummaryOfSPRetiredTerm()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            if (_teamsRetiredTermsInfo.ContainsKey(curTermStoreId))
            {
                foreach (var term in _teamsRetiredTermsInfo[curTermStoreId])
                {
                    if (!_usageTermInfo.Values.Contains(term) && !teamsRetiredTermIds.Contains(term.UniqueId))
                    {
                        details.Add(new JMTermSelection()
                        {
                            Term = term.Name,
                            TermFullPath = term.FullPath
                        });
                        teamsRetiredTermIds.Add(term.UniqueId);
                    }
                }
            }
            ReportManager.BatchSendJobDetail(details);
        }
        protected override CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds, IAveWeb web, IAveList list)
        {
            CAMLManager cm = new CAMLManager();
            List<int> termWssIds = new List<int>();
            int tempCounter = 0;
            foreach (var termId in termIds)
            {
                List<int> wssids;
                if (isTeamsOrphanedTermReport)
                {
                    if ((_usageTermInfo.ContainsKey(termId)) && GetWssidOfTermInTaxList(termId, out wssids))
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
                else if (isTeamsRetiredTermReport)
                {
                    if ((_usageTermInfo.ContainsKey(termId)) && GetWssidOfTermInTaxList(termId, out wssids))
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
                    if (_usageTermInfo.ContainsKey(termId) && GetWssidOfTermInTaxList(termId, out wssids))
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

                tempCounter++;
                if (tempCounter >= 100)
                {
                    Logger.Info("Update job progress");
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
        public override async System.Threading.Tasks.Task RunAsync()
        {
            if (_usageTermInfo == null || _usageTermInfo.Count == 0)
            {
                ReportManager.SetJobFinished(JobStatus.Failed, "RM_RC_TUR_NoTermForReport");
                return;
            }
            try
            {
                if (isTeamsOrphanedTermReport || isTeamsRetiredTermReport)
                {
                    using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.InitSPTaxnomy"))
                    {
                        InitSPTaxnomy();
                    }
                }

                foreach (var siteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    await ProcessAsync(siteCollectionNodeItem);
                }
            }
            catch (JobStopException ex)
            {
                JobHasStopped = true;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (JobHasStopped)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (JobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                ReportManager.SetJobFinished(finalStatus);
            }
        }

        protected override async System.Threading.Tasks.Task ProcessSiteAsync(NodeItem site)
        {
            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessSite"))
            {
                using (var jScope = new CheckJobStopScope())
                {
                    try
                    {
                        CurrentContainerNodeId = GetContainerNode(site)?.Id ?? Guid.Empty;
                        var remoteSite = RABrowserClient.GetSiteNode(site.FullPath);
                        if (!Guid.TryParse(remoteSite.ObjectId, out CurrentSiteCollectionNodeId)) 
                        {
                            Logger.Warn($"Can not convert site collection id to guid {remoteSite.ObjectId}");
                        }
                        if (site.IsChecked)
                        {
                            var siteCollectionRecord = ExplorerDao.QueryByPage(_ => _.ContainerId == CurrentContainerNodeId.ToString() 
                                && _.ScopeId == CurrentSiteCollectionNodeId 
                                && _.RecordStatus == (int)RMRecordStatus.Active && _.NodeType == (int)NodeLevel.SiteCollection && _.SourceFlag == (int)SourceFlag.Teams, 1, string.Empty).Item1.FirstOrDefault();
                            var termId = siteCollectionRecord?.TermId ?? Guid.Empty;
                            BCSTermUsageReport report = new BCSTermUsageReport();
                            bool sendReport = false;
                            if (_usageTermInfo.ContainsKey(termId))
                            {
                                report.BCSTermId = termId.ToString();
                                report.BCSTermName = _usageTermInfo[termId].Name;
                                report.TermStatus = _usageTermInfo[termId].Status;
                                report.BCSTermFullPath = _usageTermInfo[termId].FullPath;
                                sendReport = true;
                            }
                            if (sendReport && siteCollectionRecord != null)
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
                        Logger.Error("An error occurred while get DefaultSiteCollectionTermStore in orphanedTermReport,site fullPath is :{0}, error message: {1}.", site.FullPath, e.ToString());
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
            //var teamsNode = RMRemoteNodeDao.GetTeamsNodeBySiteUrl(web.FullPath);

            //Guid teamId = new Guid(teamsNode.TeamId);
            //if (!_teamsSettingDao.GetSettingEnableInfoByScope(groupId, teamId, siteId, web.Id))
            //{
            //    RMSPTreeNode mSPTreeNode = new RMSPTreeNode() { BposInfo = web.BposInfo, FullPath = web.FullPath, Level = (int)NodeLevel.SiteCollection };
            //    SendJobReportDetails(mSPTreeNode, JobDetailsStatus.Skipped, "RM_JS_JMD_DisableRecordManagement");
            //    Logger.Info("Process web sharepoint setting is disable {0}", web.FullPath);
            //    return;
            //}
            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessWeb"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverWeb = web.DiscoverObj as IAveWeb;
                    if (discoverWeb?.IsRootWeb ?? false)
                    {
                        Logger.Info("No need to report root web {0}", web.FullPath);
                    }
                    else if (web.IsChecked)
                    {
                        var webRecord = ExplorerDao.QueryByPage(_ => _.ContainerId == CurrentContainerNodeId.ToString() 
                            && _.ScopeId == CurrentSiteCollectionNodeId && _.WebId == web.Id 
                            && _.RecordStatus == (int)RMRecordStatus.Active && _.NodeType == (int)NodeLevel.Site && _.SourceFlag == (int)SourceFlag.Teams, 1, string.Empty).Item1.FirstOrDefault();
                        var termId = webRecord?.TermId ?? Guid.Empty;
                        BCSTermUsageReport report = new BCSTermUsageReport();
                        bool sendReport = false;
                        if (_usageTermInfo.ContainsKey(termId))
                        {
                            report.BCSTermId = termId.ToString();
                            report.BCSTermName = _usageTermInfo[termId].Name;
                            report.TermStatus = _usageTermInfo[termId].Status;
                            report.BCSTermFullPath = _usageTermInfo[termId].FullPath;
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

        protected override async Task ProcessWebsAsync(NodeItem sitesNode)
        {
            using (PerformanceScope scope = new PerformanceScope("RMTeamsReportProcessor.ProcessWebs", $"RMTeamsReportProcessor.ProcessWebs.[{sitesNode.NameOrTitle}]", addToStatistics: true))
            {
                try
                {
                    CheckNodeLevel(sitesNode, NodeLevel.Sites);
                    var parentWeb = sitesNode.DiscoverObj as IAveWeb;
                    NodeItem tempWebNode;
                    foreach (var subWeb in parentWeb.Webs)
                    {
                        ReportManager.Increase();
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            if (sitesNode.Children.TryGetValue(subWeb.ID, out tempWebNode))
                            {
                                tempWebNode.DiscoverObj = subWeb;
                                if (AreThereProcessedChildren(tempWebNode))
                                {
                                    tempWebNode.TeamsName = sitesNode.TeamsName;
                                    await ProcessWebAsync(tempWebNode, true);
                                }
                                else if (tempWebNode.IsChecked)
                                {
                                    SendJobReportDetails(tempWebNode, JobDetailsStatus.Successful);
                                }
                                sitesNode.Children.Remove(subWeb.ID);
                            }
                            else if (sitesNode.IsChecked)
                            {
                                tempWebNode = new NodeItem()
                                {
                                    Id = subWeb.ID,
                                    NameOrTitle = subWeb.Name,
                                    DiscoverObj = subWeb,
                                    FullPath = subWeb.Url,
                                    NodeLevel = NodeLevel.Site,
                                    Parent = sitesNode,
                                    IsChecked = true,
                                    TeamsName = sitesNode.TeamsName,
                                };
                                await ProcessWebAsync(tempWebNode, true);
                            }
                        }
                    }

                    if (sitesNode.Children.Count > 0)
                    {
                        foreach (var node in sitesNode.Children.Values)
                        {
                            if (node.IsChecked)
                            {
                                JobHasException = true;
                                SendJobReportDetails(node, JobDetailsStatus.Failed, "RM_JM_Details_Failed_NodeDeleted");
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
                    JobHasException = true;
                    Logger.Error("An error occurred while processing sites level node, error message: {0}.", e.ToString());
                }
                finally
                {
                    ClearChildren(sitesNode);
                }
            }
        }

        protected override int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items, string teamsName)
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
                            Logger.Info("Process Item {0}", item.UniqueId);
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
                                    report.LastModifiedBy = GetSingleUserFieldValue(item, "Editor");
                                    report.LastModifiedTime = DateTime.Parse(item["Modified"].ToString()).Ticks;
                                    report.SPWebTimeZoneName = ClientContextHelper.SPWebTimeZone.Description;

                                    Guid termId;
                                    string termName;
                                    if (GetSingleTaxonomyFieldValue(item, BCSColumnInternalName, out termId, out termName))
                                    {
                                        report.BCSTermId = termId.ToString();
                                        report.BCSTermName = termName;
                                        if (_usageTermInfo.ContainsKey(termId))
                                        {
                                            report.TermStatus = _usageTermInfo[termId].Status;
                                            report.BCSTermFullPath = _usageTermInfo[termId].FullPath;
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
                                Logger.Warn("Report item failed. item url: {0}, error message: {1}.", item.Url, ex.ToString());
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
        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem list)
        {
            using (PerformanceScope scope = new PerformanceScope("BCSTermUsageReportProcessor.ProcessList"))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var discoverList = list.DiscoverObj as IAveList;
                    Guid webId = discoverList?.ParentWeb?.ID ?? Guid.Empty;
                    Guid listId = discoverList?.ID ?? Guid.Empty;
                    var listRecord = ExplorerDao.QueryByPage(_ => _.ContainerId == CurrentContainerNodeId.ToString()
                            && _.ScopeId == CurrentSiteCollectionNodeId && _.WebId == webId && _.ListId == listId
                            && _.RecordStatus == (int)RMRecordStatus.Active && _.NodeType == (int)NodeLevel.List && _.SourceFlag == (int)SourceFlag.Teams, 1, string.Empty).Item1.FirstOrDefault();
                    if (list.IsChecked)
                    {
                        var termId = listRecord?.TermId ?? Guid.Empty;
                        BCSTermUsageReport report = new BCSTermUsageReport();
                        bool sendReport = false;
                        if (_usageTermInfo.ContainsKey(termId))
                        {
                            report.BCSTermId = termId.ToString();
                            report.BCSTermName = _usageTermInfo[termId].Name;
                            report.TermStatus = _usageTermInfo[termId].Status;
                            report.BCSTermFullPath = _usageTermInfo[termId].FullPath;
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


        private  async Task ProcessItemsUnderList(NodeItem list)
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
                    JobHasException = true;
                    SendJobReportDetails(list, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    Logger.Error("An error occurred while processing list level node: {0}, error message: {1}.", list.FullPath, e.ToString());
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
                    && _.ScopeId == CurrentSiteCollectionNodeId && _.WebId == webId && _.ListId == listId && _usageTermIds.Contains(_.TermId)
                    && _.RecordStatus == (int)RMRecordStatus.Active && _.SourceFlag == (int)SourceFlag.Teams && (_.NodeType == (int)NodeLevel.Folder || _.NodeType == (int)NodeLevel.Item), 1000, pageIndex);
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
                            Logger.Info("Process Item {0}", record.Id);
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
                                    string termName;
                                    report.BCSTermId = termId.ToString();
                                    if (_usageTermInfo.ContainsKey(termId))
                                    {
                                        report.BCSTermName = _usageTermInfo[termId].Name;
                                        report.TermStatus = _usageTermInfo[termId].Status;
                                        report.BCSTermFullPath = _usageTermInfo[termId].FullPath;
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
                                Logger.Warn("Report item failed. item url: {0}, error message: {1}.", record.DirPath, ex.ToString());
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

        private async System.Threading.Tasks.Task GetRetiredTermOfTeamsSiteAsync(RMSPTreeNode siteNode)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("BCSTermUsageReportProcessor.GetRetiredTermOfSPSite"))
            {
                try
                {
                    Guid termStoreId = Guid.Empty;
                    Logger.Info("Processing Site Collection of TeamsRetiredTermReport url:{0}", siteNode.FullPath);
                    _RMTeamsTax.InitClientContext(siteNode);
                    termStoreId = _RMTeamsTax.GetDefaultTermStoreId();

                    if (_teamsRetiredTermsInfo.ContainsKey(termStoreId))
                    {
                        Logger.Info("The term store has aleady been checked.");
                    }
                    else
                    {
                        List<RMTerm> retiredTermsOfSP = await _RMTeamsTax.GetRetiredTermOfSharePointAsync(BaseJobDto.Id);
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
                        if (!_teamsRetiredTermsInfo.ContainsKey(termStoreId))
                        {
                            _teamsRetiredTermsInfo.Add(termStoreId, termIdentitylist);
                        }
                    }
                    Logger.Info("Processing site collection complete.");
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred while GetRetiredTermsOfTeams,error message: {0}", e.ToString());
                }
            }
        }
        private async System.Threading.Tasks.Task GetOrphanTermOfTeamsSiteAsync(RMSPTreeNode siteNode)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("BCSTermUsageReportProcessor.GetOrphanTermOfSPSite"))
            {
                try
                {
                    Logger.Info("Processing Site Collection of OrphanedtermReport url:{0}", siteNode.FullPath);

                    _RMTeamsTax.InitClientContext(siteNode);
                    var termStoreId = _RMTeamsTax.GetDefaultTermStoreId();
                    if (_teamsOrphanTermsInfo.ContainsKey(termStoreId))
                    {
                        Logger.Info("The term store has aleady been checked.");
                    }
                    else
                    {
                        List<RMTerm> orphanedTermsOfSP = await _RMTeamsTax.GetOrphanedTermOfSharePointAsync(BaseJobDto.Id);

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
                        if (!_teamsOrphanTermsInfo.ContainsKey(termStoreId))
                        {
                            _teamsOrphanTermsInfo.Add(termStoreId, termIdentitylist);
                        }
                    }
                    Logger.Info("Processing site collection complete.");
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred while GetOrphanedTermsOfSharePoint,error message: {0}", e.ToString());
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
        private void InitSPTaxnomy()
        {
            var allTerms = TermDao.GetAllTerms();
            _RMTeamsTax = new RMSharePointTaxonomy(null, null, allTerms);
            _teamsOrphanTermsInfo = new Dictionary<Guid, List<RMTermIdentity>>();
            _teamsRetiredTermsInfo = new Dictionary<Guid, List<RMTermIdentity>>();
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
                                Logger.Warn("Term Title in TaxonomyHiddenList is null.TermGuid:[{0}] TermSetId:[{1}]", termItem["IdForTerm"].ToString(), termItem["IdForTermSet"]);
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
                    Logger.Warn("get wwsid for term error: {0}", e1.ToString());
                }
            }
        }
        private bool GetWssidOfTermInTaxList(Guid termId, out List<int> wssids)
        {
            return mWssidsInWeb.TryGetValue(termId, out wssids);
        }

    }
}