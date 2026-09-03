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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Extension;
using AvePoint.RA.Common.Threads;
using System.Collections.Concurrent;
using System.Threading;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Common.SystemSetting;

namespace AvePoint.RA.SharePoint.OneDrive.Discover.Base
{
    public delegate void ProcessOneDriveWebApplicationHandler(Guid webAppId);

    /// <summary>
    /// 目前不是线程安全的，不能支持多线程
    /// </summary>
    public abstract class RMOneDriveReportProcessor : IDisposable
    {
        protected static readonly RALogger mLog = RALogger.GetInstance(typeof(RMOneDriveReportProcessor));
        private List<string> DesignLists = new List<string>();
        private const string SITES = "Sites";
        private const string LISTS = "Lists";
        protected string homeLocationName;
        protected string lifecycleStatusName;
        protected string boxName;
        protected string availabilityName;
        protected string currentlyHeldByName;
        //public Dictionary<Guid, string> mBCSColumnNameDics = new Dictionary<Guid, string>();
        protected string mBCSColumnName = string.Empty;
        protected Guid mWebApplicationId = Guid.Empty;
        protected List<NodeItem> SiteCollectionNodeItems = new List<NodeItem>();
        protected Guid siteId;
        protected Guid groupId;
        private ConcurrentDictionary<Guid, int> mTermWssidMappingsOfSite;
        /// <summary>
        /// when intial mTermWssidMappingsOfSite in multi thread, need to lock
        /// </summary>
        private List<TermTreeNode> mGroupTermTreeNodes;
        public event ProcessOneDriveWebApplicationHandler ProcessingWebApplication;
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
        //private int mRestProgressRatio = 100;   //剩余的进度 0~100
        //private int mTotalSiteCollections = 0;
        //private int mProcessedSiteCollections = 0;  //已处理的SiteCollection数
        //private int mProcessedLists = 0;
        //private int mTotalLists = 0;
        protected bool mJobHasException = false;
        protected bool mJobHasStopped = false;
        private ISPSettingTreeService mSPTreeService;
        private IRMReportService mReportService;     
        private readonly static RASimpleLocker _simpleLocker = new RASimpleLocker();

        protected SharePointSettingUtility SPSettingUtility = new SharePointSettingUtility();
        protected AveObjectModelFactory mFactory = null;

        private IRMSubJobDao SubJobDao { set; get; }
        protected IOneDriveSettingDao OneDriveSettingDao { get; set; }
        private IExplorerQueryService mExplorerQueryService;
        public IExplorerQueryService ExplorerQueryService
        {
            get
            {
                if (mExplorerQueryService == null)
                {
                    mExplorerQueryService = (IExplorerQueryService)PlatformWindsorManager.GetService(typeof(IExplorerQueryService));
                }
                return mExplorerQueryService;
            }
        }

        private IJobMonitorDao mJobMonitorDao;
        public IJobMonitorDao JobMonitorDao
        {
            get
            {
                if (mJobMonitorDao == null)
                {
                    mJobMonitorDao = (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));
                }
                return mJobMonitorDao;
            }
        }
        protected virtual bool IsProcessListInParallel => false;

        protected RMOneDriveReportProcessor(string jobId, int jobType, bool IsOrphanedTermReport)
        {
            JobInfo = new Contract.JobMonitor.BaseJobDto() { Id = jobId, JobType = jobType };
            //ReportMangerFactory.Instance.Init(jobId, (JobType)jobType, true);
            mGroupTermTreeNodes = IsOrphanedTermReport ?  ReportService.GetRATermTreeNodeOfOrphanedTermAsync().Result :  ReportService.GetRATermTreeNodesAsync().Result;
            //计算当前的Job进度
            //ReportManager.Increase(1);
            ReportManager.StartUpdateJobProgress();
            //int currentRatio = new Random().Next(11, 39);
            //mRestProgressRatio = 100 - currentRatio;
            //ReportManager.Increase(currentRatio);
            DesignLists = WebUtil.GetDesignLists(JobContext.IsCSDTenant);
            SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
            OneDriveSettingDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
            var mainJob = JobMonitorDao.GetJob(subJobWithContext.ParentId);
           // var account = AccountDao.GetActiveUserByName(mainJob.UserName);
            TenantLocalValue.LogonUserId = mainJob.ContainerId;

            List<RMSPTreeNode> tempSiteCollections = string.IsNullOrWhiteSpace(subJobWithContext?.JobContext?.Settings) ? new List<RMSPTreeNode>() : SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJobWithContext.JobContext.Settings);
            foreach (var tempSiteCollection in tempSiteCollections)
            {
                if (tempSiteCollection.NodeType == (int)NodeType.SkyDriveProSitesGroup)
                {
                    mLog.Info("skip onedrive node{0}", tempSiteCollection.FullPath);
                }
                var siteSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(tempSiteCollection.SPObjectId), new Guid(tempSiteCollection.SPObjectId));
                if (siteSetting == null)
                {
                    siteSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(tempSiteCollection.Parent.SPObjectId), Guid.Empty);
                }
                if (siteSetting != null && siteSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    SiteCollectionNodeItems.Add(new NodeItem(tempSiteCollection, new NodeItem(tempSiteCollection.GetGroupNode())));
                }
                else
                {
                    AddDisabledReportDetail(new NodeItem(tempSiteCollection));
                    mLog.Info("node is disable {0}", tempSiteCollection.FullPath);
                }
            }
            //if (tempSiteCollections != null && tempSiteCollections.Count > 0)
            //{
            //    mBCSColumnNameDics = InitBCSColumnNames(tempSiteCollections);
            //}
        }

        //private Dictionary<Guid, string> InitBCSColumnNames(List<RMSPTreeNode> tempSiteCollections)
        //{
        //    var result = new Dictionary<Guid, string>();
        //    foreach (var site in tempSiteCollections)
        //    {
        //        if (!site.IsEnableHoldPhyical)
        //        {
        //            var tempColumnName = SPSettingUtility.GetMedataColumn(new Guid((site.Parent.SPObjectId)));
        //            if (!string.IsNullOrEmpty(tempColumnName))
        //            {
        //                try
        //                {
        //                    result.Add(new Guid(site.Id), tempColumnName);
        //                }
        //                catch (Exception ex)
        //                {
        //                    mLog.Error("InitBCSColumnNames error, detail: ", ex.ToString());
        //                }
        //            }
        //        }
        //    }
        //    return result;
        //}

        protected ISPSettingTreeService SPTreeService
        {
            get
            {
                if (mSPTreeService == null)
                {
                    mSPTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
                }
                return mSPTreeService;
            }
        }
        protected IRMReportService ReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }      
        protected bool JobHasExceptions
        {
            set
            {
                mJobHasException = value;
            }
        }
        protected IAveTimeZone SPWebTimeZone { get; set; }
        protected RegionalSettings RegionalSetting { get; set; }
        protected ClientContext context { get; set; }
        //protected CookieContainer cookie { get; set; }
        protected string BCSColumnInternalName { get; set; }
        protected Contract.JobMonitor.BaseJobDto JobInfo { get; set; }
        protected IAveSite Site { get; set; }
        protected IAveWeb Web { get; set; }
        protected Dictionary<Guid, IAveList> PhyListDict { get; set; }
        protected List<Guid> FitRuleFoldersInDisposalJob = new List<Guid>();

        /// <summary>
        /// Check if current node type is the excepted one
        /// </summary>
        /// <param name="real">current node type</param>
        /// <param name="expected">expected node type</param>
        /// <returns></returns>
        protected void CheckNodeLevel(NodeItem node, NodeLevel expected)
        {
            if (!node.NodeLevel.Equals(expected))
            {
                throw new Exception(string.Format("Node expected level is {0}, but current node type is {1}. Node full path: {2}.", expected.ToString(), node.NodeLevel.ToString(), node.FullPath));
            }
        }
        /// <summary>
        /// 判断是否有需要Process的子节点，
        /// 判断依据：NodeItem节点有子节点是勾选的 或者节点需要IncludeNew 或者 NodeItem节点被勾选且没有展开
        /// </summary>
        protected bool AreThereProcessedChildren(NodeItem node)
        {
            return node.HasCheckedChildren || node.IncludeNew || (node.IsChecked && node.Children.Count == 0);
        }
        protected void ClearChildren(NodeItem node)
        {
            node.Children.Clear();
        }
        protected bool GetSingleTaxonomyFieldValue(IAveListItem item, string fieldName, out Guid termId, out string termName)
        {
            return item.GetSingleTaxonomyFieldValue(fieldName, out termId, out termName);
        }
        protected DateTime GetDateTimeValue(DateTime dt)
        {
            try
            {
                if (RegionalSetting != null)
                {
                    var utcTime = RegionalSetting.TimeZone.UTCToLocalTime(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified));
                    context.ExecuteQuery();
                    return utcTime.Value;
                }
                else
                {
                    //dt = DateTime.Parse(item[fieldName].ToString());
                    return SPWebTimeZone.UTCToLocalTime(dt);
                }
            }

            catch (Exception ex)
            {
                mLog.Warn("Get datetime field value failed", ex.ToString());
                try
                {
                    return SPWebTimeZone.UTCToLocalTime(dt);
                }
                catch (Exception e1)
                {
                    mLog.Warn("Get datetime field value failed", e1.ToString());
                }
            }
            return new DateTime();
        }
        protected DateTime GetDateTimeFieldValue(IAveListItem item, string fieldName)
        {
            return item.GetDateTimeFieldValue(SPWebTimeZone, fieldName);
        }

        protected DateTime GetDateTimeFromUtc(long ticks, IAveWeb web)
        {
            var dt = new DateTime(ticks);

            try
            {
                TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(web.RegionalSettings.TimeZone.ID));
                var utcDateTime = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                var dt0 = dt + cstZone.GetUtcOffset(utcDateTime);
                return dt0;
            }

            catch (Exception ex)
            {
                mLog.Warn("Get datetime from utc time failed! ticks:{0} error message: {1}.", ticks, ex.ToString());
                try
                {
                    return web.RegionalSettings.TimeZone.UTCToLocalTime(dt);
                }
                catch (Exception e1)
                {
                    mLog.Warn("Get datetime from utc time failed! ticks:{0} error message: {1}.", ticks, e1.ToString());
                }
            }
            return new DateTime();
        }
        protected string GetSingleUserFieldValue(IAveListItem item, string fieldName)
        {
            return item.GetSingleUserFieldValue(fieldName);
        }
        protected string MakeFullUrl(string webUrl, string relativeUrl)
        {
            if (webUrl == null)
            {
                throw new ArgumentNullException("webUrl");
            }
            if (relativeUrl == null)
            {
                throw new ArgumentNullException("relativeUrl");
            }
            relativeUrl = relativeUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (relativeUrl.StartsWith("/"))
            {
                stringBuilder.Append(webUrl);
                stringBuilder.Append(relativeUrl);
            }
            else
            {
                stringBuilder.Append(webUrl);
                if (relativeUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(relativeUrl);
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        protected virtual void AddDisabledReportDetail(NodeItem item)
        {
            SendJobReportDetails(item, JobDetailsStatus.Skipped, "RM_JS_JMD_DisableRecordManagement");
        }

        protected void SendJobReportDetails(NodeItem item, JobDetailsStatus status, string comments = "")
        {
            if (this is OneDriveCreationAndDestroyedReportProcessor)
            {
                return;
            }
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = JobReportUtility.ConvertItemTypeForDetails(item.NodeLevel);
            detail.TitleOrName = item.NameOrTitle;
            detail.Url = item.FullPath;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }

        protected void SendJobReport(List<BaseReport> reports)
        {
            RASimpleLocker.Locker locker = _simpleLocker.GetLocker(JobInfo.Id);

            lock (locker)
            {
                try
                {
                    if (reports != null)
                    {
                        ReportManager.BatchSendJobReport(reports);
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("Add Report Error {0} {1}", JobInfo.Id, e.ToString());
                }
                finally
                {
                    _simpleLocker.FreeLocker(locker.Key);
                }
            }
        }

        private void DiscoseContext()
        {
            try
            {
                if (this.context != null)
                {
                    context.Dispose();
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Dispose context error {0}", e.ToString());
            }
        }
        private static void SafeDisposeObject(object obj)
        {
            if (obj == null)
            {
                return;
            }

            var disposeObj = (obj as IDisposable);

            if (disposeObj != null)
            {
                disposeObj.Dispose();
            }
        }
        public virtual async System.Threading.Tasks.Task ProcessAsync(NodeItem node)
        {
            groupId = node.Parent.Id;
            siteId = node.Id;
            try
            {
                if (node.NodeLevel == NodeLevel.SiteCollection)
                {
                    await ProcessSiteAsync(node);
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                mLog.Error("An error occurred while farm process. fullPath: [{0}], error message : {1}.", node.FullPath, ex.ToString());
                throw;
            }
        }
        protected void SetEvent()
        {
            if (ProcessingWebApplication != null)
            {
                ProcessingWebApplication(mWebApplicationId);
            }
        }
        protected virtual async System.Threading.Tasks.Task ProcessWebAppAsync(NodeItem webapp)
        {
            using (PerformanceScope scope = new PerformanceScope("RMReportProcessor.ProcessWebApp"))
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
        protected virtual async System.Threading.Tasks.Task ProcessSiteAsync(NodeItem site)
        {
            using (PerformanceScope scope = new PerformanceScope($"RMReportProcessor.ProcessSite.[{site.NameOrTitle}]"))
            {
                IAveWeb discoverWeb = null;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        CheckNodeLevel(site, NodeLevel.SiteCollection);
                        mLog.Info("Start Site process. fullPath: [{0}], isIncludeNew : [{1}].", site.FullPath, site.IncludeNew);
                        var remoteSite = RABrowserClient.GetSiteNode(site.FullPath);
                        var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
                        mFactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                        Site = mFactory.CreateSite(site.FullPath);

                        var mTotalWebs = Site.AllWebs.Count;
                        ReportManager.IncreaseBase(mTotalWebs);

                        try
                        {
                            CommonClientContext commonContext = new CommonClientContext();
                            this.context = commonContext.InitClientContext(new RMSPTreeNode()
                            {
                                BposInfo = site.BposInfo,
                                FullPath = site.FullPath,
                                Level = (int)NodeLevel.SiteCollection
                            }, bposInfo);
                        }
                        catch (Exception ce)
                        {
                            mLog.Warn("Get Context Error {0}", ce.ToString());
                        }
                        mTermWssidMappingsOfSite = new ConcurrentDictionary<Guid, int>();
                        discoverWeb = Site.RootWeb;
                        site.DiscoverObj = Site;
                        site.NameOrTitle = discoverWeb.Title;
                        SPWebTimeZone = discoverWeb.RegionalSettings.TimeZone;
                        NodeItem rootWebNode;
                        //Sitecollection 节点有子节点是勾选的
                        if (site.HasCheckedChildren)
                        {
                            rootWebNode = site.Children.Values[0];
                            rootWebNode.DiscoverObj = discoverWeb;
                            rootWebNode.NameOrTitle = discoverWeb.Title;
                        }
                        else  //if (site.IsChecked && site.Children.Count == 0)   Sitecollection 节点被勾选了，但是没有展开
                        {
                            rootWebNode = new NodeItem()
                            {
                                Id = discoverWeb.ID,
                                NameOrTitle = discoverWeb.Title,
                                DiscoverObj = discoverWeb,
                                FullPath = site.FullPath,
                                NodeLevel = NodeLevel.Site,
                                Parent = site,
                                IncludeNew = true,
                                IsChecked = true
                            };
                        }

                        SendJobReportDetails(site, JobDetailsStatus.Successful);
                        await ProcessWebAsync(rootWebNode);
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (WebException we)
                {
                    mJobHasException = true;
                    //当Site已被删除或者User对Site权限不足时，Response的HttpStatusCode都是Unauthorized，所以没办法区分这两种情况
                    //var httpResp = (we.Response as HttpWebResponse);
                    //if (httpResp != null && httpResp.StatusCode == HttpStatusCode.Unauthorized)
                    //{
                    //    SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_AccessDenied");
                    //}
                    //else
                    //{
                    //    SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    //}
                    SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    mLog.Error("An error occurred while prosess sitecollection, fullPath is :{0}, error message: {1}.", site.FullPath, we.ToString());
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    SendJobReportDetails(site, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    mLog.Error("An error occurred while prosess sitecollection, fullPath is :{0}, error message: {1}.", site.FullPath, e.ToString());
                }
                finally
                {
                    SafeDisposeObject(Site);
                    ClearChildren(site);
                    DiscoseContext();
                }
            }
        }
        public RegionalSettings GetRegionalSetting(string webServerRelativeUrl)
        {
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            context.Load(web);
            RegionalSettings regionalSettings = web.RegionalSettings;
            context.ExecuteQuery();
            return regionalSettings;
        }
        public void clientContext_ExecutingWebRequest(object sender, WebRequestEventArgs e)
        {
            try
            {
                e.WebRequestExecutor.WebRequest.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
            }
            catch (Exception ex)
            {
                mLog.Warn("add form based auth error {0}", ex.ToString());
            }
        }
        protected virtual async System.Threading.Tasks.Task ProcessWebAsync(NodeItem web, bool IsProcessLists = true)
        {
            using (PerformanceScope scope = new PerformanceScope($"RMReportProcessor.ProcessWeb.[{web.NameOrTitle}]"))
            {
                try
                {
                    ReportManager.Increase();
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        CheckNodeLevel(web, NodeLevel.Site);
                        SendJobReportDetails(web, JobDetailsStatus.Successful);

                        SPWebTimeZone = (web.DiscoverObj as IAveWeb).RegionalSettings.TimeZone;
                        try
                        {
                            RegionalSetting = GetRegionalSetting((web.DiscoverObj as IAveWeb).ServerRelativeUrl);
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Get Regional settings error {0}", ex.ToString());
                        }
                        if (web.Children.Count == 0)
                        {
                            //Lists节点
                            if (IsProcessLists)
                            {
                                var treeNodeLists = new NodeItem()
                                {
                                    NodeLevel = NodeLevel.Lists,
                                    IsChecked = true,
                                    FullPath = LISTS,
                                    NameOrTitle = LISTS,
                                    Parent = web,
                                    DiscoverObj = web.DiscoverObj
                                };
                                await ProcessListsAsync(treeNodeLists);
                            }

                            //Sites节点
                            var treeNodeSites = new NodeItem()
                            {
                                NodeLevel = NodeLevel.Sites,
                                IsChecked = true,
                                FullPath = SITES,
                                NameOrTitle = SITES,
                                Parent = web,
                                DiscoverObj = web.DiscoverObj
                            };
                            await ProcessWebsAsync(treeNodeSites);
                        }
                        else
                        {
                            foreach (var childNode in web.Children.Values.OrderBy(n => n.NodeLevel))
                            {
                                if (AreThereProcessedChildren(childNode))
                                {
                                    childNode.DiscoverObj = web.DiscoverObj;
                                    switch (childNode.NodeLevel)
                                    {
                                        case NodeLevel.Lists:
                                            if (IsProcessLists)
                                            {
                                                await ProcessListsAsync(childNode);
                                            }
                                            break;
                                        case NodeLevel.Sites:
                                            await ProcessWebsAsync(childNode);
                                            break;

                                        default:
                                            break;
                                    }
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
                    mLog.Error("An error occurred while processing web: {0}, error message: {1}.", web.FullPath, e.ToString());
                }
                finally
                {
                    SafeDisposeObject(web.DiscoverObj);
                    ClearChildren(web);
                }
            }
        }
        protected async System.Threading.Tasks.Task ProcessWebsAsync(NodeItem sitesNode)
        {
            using (PerformanceScope scope = new PerformanceScope($"RMReportProcessor.ProcessWebs.[{sitesNode.NameOrTitle}]"))
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
                                    await ProcessWebAsync(tempWebNode);
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
                                    IsChecked = true
                                };
                                await ProcessWebAsync(tempWebNode);
                            }
                        }
                    }

                    if (sitesNode.Children.Count > 0)
                    {
                        foreach (var node in sitesNode.Children.Values)
                        {
                            if (node.IsChecked)
                            {
                                mJobHasException = true;
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
                    mJobHasException = true;
                    mLog.Error("An error occurred while processing sites level node, error message: {0}.", e.ToString());
                }
                finally
                {
                    ClearChildren(sitesNode);
                }
            }
        }
        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (this.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
            }
            return isDesignList;
        }

        private async System.Threading.Tasks.Task ProcessListAsync(NodeItem listsNode, IAveWeb parentWeb, IAveList discoverList, CancellationTokenSource cts = null)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    ReportManager.Increase();
                    NodeItem tempListNode;
                    mLog.Info("list rootfolder url {0}", discoverList.RootFolder.Name);
                    int listTemplate = (int)discoverList.BaseTemplate;
                    if (listTemplate == 600)
                    {
                        mLog.Info("Skip external list {0}", discoverList.RootFolder.Name);
                        return;
                    }
                    if (discoverList.BaseType != AveBaseType.DocumentLibrary && (int)discoverList.BaseTemplate != 700)
                    {
                        mLog.Info("This is not a document library, skip it. Path: {0}", discoverList.RootFolder.Name);
                        return;
                    }
                    //Skip the system list & custom list
                    if (CheckIsDesignList(discoverList.RootFolder.Name + listTemplate.ToString()) || discoverList.Hidden)
                    {
                        mLog.Info("Skip the design list & system list{0}", discoverList.RootFolder.Name);
                        return;
                    }

                    
                    if (listsNode.Children.TryGetValue(discoverList.ID, out tempListNode) && tempListNode.IsChecked)
                    {
                        tempListNode.DiscoverObj = discoverList;
                        await ProcessListAsync(tempListNode);
                        //mProcessedLists++;
                        //UpdateJobProgressByList();
                        listsNode.Children.SafeRemove(discoverList.ID);
                    }
                    else if (listsNode.IsChecked)
                    {
                        if (!listsNode.Children.TryGetValue(discoverList.ID, out tempListNode))
                        {
                            tempListNode = new NodeItem()
                            {
                                NodeLevel = NodeLevel.List,
                                Id = discoverList.ID,
                                NameOrTitle = discoverList.Title,
                                FullPath = MakeFullUrl(parentWeb.Url, discoverList.RootFolder.Url),  //discoverList.RootFolder.ServerRelativeUrl,
                                NodeType = discoverList.BaseType == AveBaseType.DocumentLibrary ? NodeType.DocumentLibrary : NodeType.GenericList,
                                DiscoverObj = discoverList,
                                Parent = listsNode,
                                IsChecked = true
                            };
                            if (tempListNode.NameOrTitle.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
                            {
                                tempListNode.NodeType = NodeType.DocumentLibrary;
                            }
                            if (tempListNode != null && tempListNode.IsChecked)
                            {
                                await ProcessListAsync(tempListNode);
                            }
                            else
                            {
                                mLog.Warn("Temp list node is null.");
                            }
                        }
                        //mProcessedLists++;
                        //UpdateJobProgressByList();
                    }
                }
            }
            catch (JobStopException ex)
            {
                cts?.Cancel();
                throw ex;
            }
        }

        private async System.Threading.Tasks.Task ProcessListsAsync(IAveWeb parentWeb, NodeItem listsNode)
        {
            if (parentWeb.Lists.Count == 0) return;
            if (IsProcessListInParallel)
            {
                var cts = new System.Threading.CancellationTokenSource();
                AveTenantTasks.RunAndWaitTasks(
                    parentWeb.Lists,
                    cts,
                    async discoverList =>
                    {
                        try
                        {
                            await ProcessListAsync(listsNode, parentWeb, discoverList, cts);
                        }
                        catch(Exception e)
                        {
                            mLog.Warn(e.ToString());
                        }
                    }
                );
            }
            else
            {
                foreach (var discoverList in parentWeb.Lists)
                {
                    await ProcessListAsync(listsNode, parentWeb, discoverList);
                }
            }
        }
        protected async System.Threading.Tasks.Task ProcessListsAsync(NodeItem listsNode)
        {
            using (PerformanceScope scope = new PerformanceScope($"RMReportProcessor.ProcessLists.[{listsNode.NameOrTitle}]"))
            {
                try
                {
                    CheckNodeLevel(listsNode, NodeLevel.Lists);
                    var parentWeb = listsNode.DiscoverObj as IAveWeb;
                    //NodeItem tempListNode;
                    ReportManager.IncreaseBase(parentWeb.Lists.Count);
                    await ProcessListsAsync(parentWeb, listsNode);
                    if (listsNode.Children.Count > 0)
                    {
                        foreach (var node in listsNode.Children.Values)
                        {
                            if (node.IsChecked)
                            {
                                string webRelativeUrl = parentWeb.ServerRelativeUrl;
                                if (node.FullPath.StartsWith(webRelativeUrl, StringComparison.OrdinalIgnoreCase))
                                {
                                    node.FullPath = node.FullPath.Substring(webRelativeUrl.Length);
                                    node.FullPath = MakeFullUrl(parentWeb.Url, node.FullPath);
                                }
                                mJobHasException = true;
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
                    mJobHasException = true;
                    mLog.Error("An error occurred while processing lists level node, error message: {0}.", e.ToString());
                }
                finally
                {
                    ClearChildren(listsNode);
                }
            }
        }

        protected virtual ExplorerQueryV2Dto GetFilterOption(Guid scopeId, Guid listId)
        {
            return RMOneDriveQueryHelper.GetListQueryDto(scopeId, listId);
        }

        protected virtual async System.Threading.Tasks.Task ProcessListAsync(NodeItem list)
        {
            using (PerformanceScope scope0 = new PerformanceScope("RMReportProcessor.ProcessList"))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        //UpdateJobWithoutProgressChange();//更新job进度，防止因为数据量太大导致job超时
                        CheckNodeLevel(list, NodeLevel.List);
                        var discoverList = list.DiscoverObj as IAveList;
                        var rootFolder = discoverList.RootFolder;
                        var discoverWeb = discoverList.ParentWeb;

                        list.FullPath = MakeFullUrl(discoverWeb.Url, rootFolder.Url);
                        list.NameOrTitle = discoverList.Title;
                        long total = 0;
                        ReportManager.Increase();
                        mLog.Debug($"begin to query list:{list.FullPath}");

                        var explorerQueryV2Dto = GetFilterOption(discoverList.ParentWeb.Site.ID, discoverList.ID);
                        ExplorerPagingInfo pageInfo;
                        do
                        {
                            var result = await ExplorerQueryService.QueryDataListWithoutTotalAsync(explorerQueryV2Dto);
                            if (result != null && result.Datas != null && result.Datas.Count > 0)
                            {
                                total += ProcessItems(discoverWeb, discoverList, result.Datas);
                            }
                            pageInfo = result?.PagingInfo;
                        }
                        while (pageInfo != null && pageInfo.HasNextPage);

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
        /// <returns>返回真正符合条件的记录数</returns>
        protected abstract int ProcessItems(IAveWeb web, IAveList list, List<BaseRecordDto> items);
        // protected abstract CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds);
        public abstract System.Threading.Tasks.Task RunReportJobAsync();
        public virtual void Dispose()
        {
        }
        //private List<CAMLManager> GetCAMLManagerList(IAveFieldCollection listFields, IAveTaxonomyField mmsField, List<Guid> termIds)
        //{
        //    List<CAMLManager> cms = new List<CAMLManager>();
        //    if (termIds.Count < mQueryConditionMaxCount)
        //    {
        //        CAMLManager cm = InitCamlQuery(listFields, mmsField, termIds);
        //        if (cm != null)
        //        {
        //            cms.Add(cm);
        //        }
        //    }
        //    else
        //    {
        //        int index = 0;
        //        while (termIds.Skip(index).Take(mQueryConditionMaxCount) != null && termIds.Skip(index).Take(mQueryConditionMaxCount).Count() != 0)
        //        {
        //            var queryIds = termIds.Skip(index).Take(mQueryConditionMaxCount).ToList();
        //            index += mQueryConditionMaxCount;
        //            if (queryIds.Count() != 0)
        //            {
        //                CAMLManager cm = InitCamlQuery(listFields, mmsField, queryIds);
        //                if (cm != null)
        //                {
        //                    cms.Add(cm);
        //                }
        //            }
        //        }
        //    }
        //    return cms;
        //}

        protected string GetItemFieldValue(IAveListItem item, string fieldName)
        {
            return item.GetItemFieldValue(fieldName);
        }
        protected string GetWssIDForTerm(Guid termId)
        {
            try
            {
                string result = "-1";
                List taxonomyList = this.context.Web.Lists.GetByTitle("TaxonomyHiddenList");
                CamlQuery camlQueryForTerm = new CamlQuery();
                camlQueryForTerm.ViewXml = @"
<View>
    <Query>
        <Where>
            <Eq>
                <FieldRef Name='IdForTerm' />
                <Value Type='Text'>" + termId + @"</Value>
            </Eq>
        </Where>
    </Query>       
</View>";
                ListItemCollection termItems = taxonomyList.GetItems(camlQueryForTerm);
                this.context.Load(termItems);
                this.context.ExecuteQuery();

                //foreach (var termItem in termItems)
                //{
                //    return termItem["ID"].ToString();
                //}
                if (termItems?.FirstOrDefault() != null)
                {
                    result = termItems?.First()["ID"].ToString();
                }
                return result;
            }
            catch (Exception e1)
            {
                mLog.Warn("get wwsid for term error: {0}", e1.ToString());
                return "-1";
            }
        }

        protected string GetListItemName(IAveListItem item)
        {
            var itemName = item.Name;
            if (!string.IsNullOrEmpty(itemName))
            {
                return itemName;
            }
            switch (item.ParentList.BaseTemplate)
            {
                case AveListTemplateType.DocumentLibrary:
                case AveListTemplateType.RecordLib:
                    itemName = item.Name;
                    break;
                case AveListTemplateType.Links:
                    if (AveListTemplateType.Links == item.ParentList.BaseTemplate)
                    {
                        //IAveFieldUrlValue filedUrlValue = item.FieldValues["URL"] as IAveFieldUrlValue;
                        //new AveFieldUrlValue()
                        IAveFieldUrlValue filedUrlValue = mFactory.CreateFieldUrlValue(item.FieldValues["URL"].ToString());
                        itemName = filedUrlValue.Url;
                    }
                    break;
                default:
                    itemName = item.Title;
                    break;
            }
            return itemName;
        }
    }
}
