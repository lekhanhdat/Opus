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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Interface;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using SPDisposeCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using WrapperChangeType = AvePoint.Wrapper.Common.ChangeType;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Base
{
    public abstract class SiteMetricsScanner4JPMCBase : ISharePointScanner4JPMC
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SiteMetricsScanner4JPMCBase));
        private Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        private readonly object locker = new();
        private IScanDataReader4JPMC mScanDataReader = null;
        private long totalScanCount = 0;

        internal AveDiscoverSite mDiscoverSite = null;
        internal IBackwardDependencyNodeCache<object> mDependencyObjs;
        internal ScanJobSettings jobSettings = null;
        internal ScheduleConfiguration mConfiguration = null;
        internal Guid scopeId = Guid.Empty;
        internal AveObjectModelFactory mFactory = null;
        protected bool UseIncrementalDiscover => mConfiguration?.UseIncrementalDiscover ?? false;
        protected string ScanDbBlobPath { get; set; } = string.Empty;
        protected string ScanDbLocalPath { get; set; } = string.Empty;

        public AveDiscoverFolder mInitNodeEntityRelatedInfoDiscoverRootFolder = null;
        public Guid mInitNodeEntityRelatedInfoRootFolderId = Guid.Empty;
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
        protected IAveSite Site { get; set; }
        private IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));

        public abstract IDiscoverNodeWorker discoverWorker
        {
            get;
            set;
        }

        public SiteMetricsScanner4JPMCBase(ScanJobSettings scanJobSettings)
        {
            mDependencyObjs = new BackwardDependenceNodeCache<object>();
            jobSettings = scanJobSettings;
            mConfiguration = scanJobSettings.Configuration;
            mScanDataReader = new ScanDataReader4JPMC(mConfiguration);
            InitializeScanDbIfNeeded();
        }

        /// <summary>
        /// Allows subclasses to skip list count calculation when a full pass is not needed.
        /// </summary>
        protected virtual bool ShouldCalculateListCount()
        {
            return true;
        }

        /// <summary>
        /// Initializes scan database paths and lets subclasses handle download logic.
        /// </summary>
        protected virtual void InitializeScanDbIfNeeded()
        {
            ScanDbLocalPath = SecurityUtils.SafeCombinePath(mConfiguration.ArchiveTemp, mConfiguration.ScanDBName);
            var siteUrl = GetSiteUrlForScanDb();
            ScanDbBlobPath = BuildScanDbBlobPath(siteUrl, mConfiguration.ScanDBName);
            DownloadScanDbIfExists(ScanDbBlobPath, ScanDbLocalPath);
        }

        /// <summary>
        /// Provides the site url used to build the scan db path. Subclasses can override to supply a fallback.
        /// </summary>
        /// <returns>Resolved site url.</returns>
        protected virtual string GetSiteUrlForScanDb()
        {
            if (!string.IsNullOrWhiteSpace(mDiscoverSite?.Site?.Url))
            {
                return mDiscoverSite.Site.Url;
            }

            var siteCollectionUrl = jobSettings?.Configuration?.SiteCollectionUrl;
            if (!string.IsNullOrWhiteSpace(siteCollectionUrl))
            {
                return siteCollectionUrl;
            }

            var treeNodeFullPath = jobSettings?.TreeNode?.FullPath;
            if (!string.IsNullOrWhiteSpace(treeNodeFullPath))
            {
                return treeNodeFullPath;
            }

            mLog.Warn("Unable to resolve site url for scan db; returning empty string.");
            return string.Empty;
        }

        /// <summary>
        /// Downloads an existing scan database from storage. Subclasses override to customize behavior.
        /// </summary>
        protected virtual void DownloadScanDbIfExists(string blobPath, string localFilePath)
        {
            // Default implementation intentionally left blank; subclasses handle as needed.
        }

        protected string BuildScanDbBlobPath(string siteUrl, string scanDbName)
        {
            if (string.IsNullOrWhiteSpace(siteUrl) || string.IsNullOrWhiteSpace(scanDbName))
            {
                mLog.Warn("Skip building scan db blob path because siteUrl or scanDbName is empty.");
                return string.Empty;
            }

            try
            {
                var tenantGroupId = TenantLocalValue.LogonGroupId;
                if (string.IsNullOrWhiteSpace(tenantGroupId))
                {
                    mLog.Warn("Skip building scan db blob path because tenant group id is empty.");
                    return string.Empty;
                }

                var encodedSitePath = EncodeSiteUrlForStorage(siteUrl);
                if (string.IsNullOrEmpty(encodedSitePath))
                {
                    return string.Empty;
                }

                var normalizedPrefix = "report_db".TrimEnd('/', '\\');
                var builder = new StringBuilder();
                builder.Append(normalizedPrefix);
                builder.Append('/');
                builder.Append(tenantGroupId.Trim('/'));
                builder.Append('/');
                builder.Append(encodedSitePath.Trim('/'));
                builder.Append('/');
                builder.Append(scanDbName);
                return builder.ToString();
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to build scan db blob path for site: {siteUrl}. Error:{ex}");
                return string.Empty;
            }
        }

        protected static string EncodeSiteUrlForStorage(string siteUrl)
        {
            ParseSiteUrlSegments(siteUrl, out var webAppName, out var siteName);
            var builder = new StringBuilder();
            builder.Append(webAppName.Trim('/'));
            if (!string.IsNullOrEmpty(siteName))
            {
                builder.Append('/');
                builder.Append(siteName.Trim('/'));
            }
            return builder.ToString().Trim('/');
        }

        protected static void ParseSiteUrlSegments(string siteUrl, out string webAppName, out string siteName)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                throw new ArgumentException("siteUrl cannot be null or empty.", nameof(siteUrl));
            }

            var builder = new StringBuilder();
            var index = siteUrl.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (index < 0 || siteUrl.Length <= index + 3)
            {
                throw new ArgumentException($"Invalid site url: {siteUrl}", nameof(siteUrl));
            }

            builder.Append(siteUrl.Substring(0, index)).Append("#");
            var temp = siteUrl.Substring(index + 3);
            index = temp.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                builder.Append(80).Append("#");
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    builder.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    builder.Append(temp);
                    temp = string.Empty;
                }
            }
            else
            {
                var machineName = temp.Substring(0, index);
                temp = temp.Substring(index + 1);
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    builder.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    builder.Append(temp);
                    temp = string.Empty;
                }
                builder.Append("#").Append(machineName);
            }

            webAppName = builder.ToString();

            builder.Clear();
            builder.Append("#");
            if (!string.IsNullOrEmpty(temp))
            {
                temp = temp.Replace(';', '#');
                builder.Append(temp.Replace('/', '#'));
            }

            siteName = builder.ToString();
        }

        /// <summary>
        /// Hook for subclasses to upload scan db or execute post-run work.
        /// </summary>
        protected virtual void UploadScanDbToStorage()
        {
            if (string.IsNullOrWhiteSpace(ScanDbBlobPath) || string.IsNullOrWhiteSpace(ScanDbLocalPath))
            {
                return;
            }

            try
            {
                if (!File.Exists(ScanDbLocalPath))
                {
                    mLog.Info($"Skip uploading scan db because the local file does not exist. Path:{ScanDbLocalPath}");
                    return;
                }

                RAStorageUtil.UploadReportBlob(ScanDbBlobPath, ScanDbLocalPath);
                mLog.Info($"Uploaded scan db to storage path {ScanDbBlobPath}.");
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to upload scan db to storage path {ScanDbBlobPath}. Error:{ex}");
            }
        }

        /// <summary>
        /// Hook for subclasses to run logic after a successful scan pass completes.
        /// </summary>
        /// <returns>A completed task by default.</returns>
        protected virtual Task OnAfterRunAsync()
        {
            UploadScanDbToStorage();
            UpdateSiteMetricsNodeFlag();
            return Task.CompletedTask;
        }

        private void UpdateSiteMetricsNodeFlag()
        {
            try
            {
                var rmNodeFlagDao = (IRMNodeFlagDao)PlatformWindsorManager.GetService(typeof(IRMNodeFlagDao));
                if (rmNodeFlagDao == null)
                {
                    mLog.Warn("RMNodeFlagDao is null, skip updating site metrics node flag.");
                    return;
                }

                var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings?.TreeNode);
                if (node == null || node.Level != NodeLevel.SiteCollection)
                {
                    mLog.Warn("Site metrics node flag update skipped because current node is not a site collection.");
                    return;
                }

                if (!Guid.TryParse(node.SPObjectId, out var siteId))
                {
                    mLog.Warn($"Invalid site collection id for node {node.FullPath}, skip updating site metrics node flag.");
                    return;
                }

                var groupNode = SPTreeNodeManagement.GetGroupNode(node);
                if (groupNode == null || !Guid.TryParse(groupNode.SPObjectId, out var groupId))
                {
                    mLog.Warn("Group id is empty, skip updating site metrics node flag.");
                    return;
                }

                var collectionTicks = mConfiguration?.IncrementalDiscoverEndTimeTicks ?? DateTime.MinValue.Ticks;
                if (collectionTicks <= DateTime.MinValue.Ticks)
                {
                    collectionTicks = DateTime.UtcNow.Ticks;
                    mLog.Info("Incremental discover end time is missing; fallback to current UTC time for site metrics node flag.");
                }

                var title = string.IsNullOrWhiteSpace(node.Name) ? node.FullPath : node.Name;

                rmNodeFlagDao.AddSiteFlagInfo(new RMNodeFlag
                {
                    NodeId = siteId,
                    GroupId = groupId,
                    Title = title,
                    FullPath = node.FullPath,
                    CollectionTime = collectionTicks,
                    NodeFlagType = (int)NodeFlagType.SiteMetrics,
                    IsRemoved = false
                });
            }
            catch (Exception ex)
            {
                mLog.Warn($"Update site metrics node flag failed, site:{jobSettings?.TreeNode?.FullPath}. Error:{ex}");
            }
        }

        public virtual async Task RunAsync()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SharePointScanner.Run"))
            {
                var runCompleted = false;
                try
                {
                    mLog.Info($"SharePointScanner start...");
                    var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings.TreeNode);
                    scopeId = Guid.Parse(node.SPObjectId);
                    var ruleNode = ConvertTreeNodeToRuleNodeConfig(node, RuleNodeType.Archiver);
                    discoverWorker.Init(ruleNode);
                    ArchiverNodeItem selectNodeItem = new ArchiverNodeItem(ruleNode);

                    if (ShouldCalculateListCount())
                    {
                        try
                        {
                            var count = CaculateListCount(selectNodeItem);
                            mLog.Info($"Scan caculate list count is {count}");
                            mConfiguration.ProgressDto.SetBaseCount4Phase(count);
                        }
                        catch (JobStopException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            mLog.Warn($"Scan caculate list count error {e}");
                        }
                    }
                    else
                    {
                        mLog.Info("Skip list count calculation by scanner configuration.");
                    }

                    switch (selectNodeItem.SPNodeLevel)
                    {
                        case NodeLevel.SiteCollection:
                            {
                                await ProcessSiteCollectionAsync(selectNodeItem);
                                break;
                            }
                        case NodeLevel.Site:
                            {
                                await ProcessWebAsync(selectNodeItem, true);
                                break;
                            }
                        case NodeLevel.List:
                        case NodeLevel.Library:
                            {
                                await ProcessListAsync(selectNodeItem, true);
                                break;
                            }
                        case NodeLevel.RootFolder:
                        case NodeLevel.FSFolder:
                        case NodeLevel.Folder:
                            {
                                await ProcessFolderAsync(selectNodeItem, true, selectNodeItem.ItemIDs);//node.itemIDs for endUser ribbon
                                break;
                            }
                        case NodeLevel.Item:
                            {
                                //For EndUserArchive
                                //ProcessItem(selectNodeItem, true);
                                break;
                            }
                        default:
                            throw new Exception(LOGRESOURCE.StorageOptimization13_SOARScanScanException);
                    }
                    mDependencyObjs.Flush();
                    runCompleted = true;
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while scanning error {0}", e.ToString());
                    throw;
                }
                finally
                {
                    discoverWorker.Flush();
                    mLog.Info($"SharePointScanner end...");
                }

                if (runCompleted)
                {
                    try
                    {
                        await OnAfterRunAsync();
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        mLog.Error("An unexpected error occurred while running post-scan actions error {0}", e.ToString());
                        throw;
                    }
                }
            }
        }

        public IScanDataReader4JPMC GetScanDataReader()
        {
            return mScanDataReader;
        }

        public virtual async Task ProcessSiteCollectionAsync(ArchiverNodeItem sitecollection)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    try
                    {
                        //初始化Site对应的一些信息。
                        await InitialSPObjectInfoAsync(discoverWorker, sitecollection);
                        //If the rootWeb has defined a unique rule, we should skip all the site collection.
                        //URL of RootWeb is same as sitecollection's
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;

                        //对这个Site检查Rule.
                        ProcessResult result = await discoverWorker.ProcessContainerAsync(sitecollection, ProcessType.NeedProcess);
                        if (result == ProcessResult.SkipCurrentNode)
                        {
                            mLog.Info("skip current Node {0}", sitecollection.FullPath);
                            return;
                        }

                        using (AveDiscoverSite discoverySite = sitecollection.DiscoverSPObject as AveDiscoverSite)
                        {
                            var discoverWebs = GetWebsForSiteCollection(discoverySite);
                            if (discoverWebs == null || discoverWebs.Count == 0)
                            {
                                mLog.Warn($"No webs found to process for site collection {sitecollection.FullPath}.");
                                return;
                            }

                            foreach (var discoverWeb in discoverWebs.Values)
                            {
                                using (discoverWeb)
                                {
                                    if (ShouldHandleDeletedWeb(discoverWeb))
                                    {
                                        await HandleDeletedWebAsync(sitecollection, discoverWeb);
                                        continue;
                                    }

                                    using (ArchiverNodeItem webNode = sitecollection.GenerateSiteNodeItem(discoverWeb, mConfiguration, true))
                                    {
                                        await ProcessWebAsync(webNode);
                                    }
                                }
                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (AveWrapperI18NException IUPEx)
                    {
                        mLog.Info("Site Collection UserName Or Password Incorrect. Path:{0}. Message:{1}.", sitecollection.FullPath, IUPEx.ToString());
                        throw;
                    }
                    catch (SPObjectReadOnlyException snfe)
                    {
                        mLog.Info("Site Collection is ReadOnly. Path:{0}. Message:{1}.", sitecollection.FullPath, snfe.ToString());

                        throw;
                    }
                    catch (SPObjectLockedException sle)
                    {
                        mLog.Info("Site Collection is Locked. Path:{0}. Message:{1}.", sitecollection.FullPath, sle.ToString());

                        throw;
                    }
                    catch (SPObjectNotFoundException ex)
                    {
                        mLog.Info("Site Collection Not Found. Path:{0}. Message:{1}.", sitecollection.FullPath, ex.ToString());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null && ex.InnerException.Message.Contains("The site do not meet the conditions."))
                        {
                            mLog.Error(string.Format("AveLATMgtApiNotEnabledException in Backup Site Collection :{0}.Site Collection Path:{1}.", ex.ToString(), sitecollection.FullPath));
                        }
                        else
                        {
                            mLog.Error("An unexpected error occurred while processing site collection node.Path:{0}.Message:{1}.", sitecollection.FullPath, ex);
                        }
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("Process sitecollection error {0}", e.ToString());
                throw;
                //TO DO Add Detail
                //TO DO I18N
                //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty,
                //    string.Empty, string.Empty, JobReportDetailStatus.Failed, e.Message);
            }
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._120, "Ignore")]
        public async virtual Task ProcessWebAsync(ArchiverNodeItem web, bool needInitInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWeb"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, web);
                    }
                    else
                    {
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                        if (mConfiguration.mInitialTime.AddHours(23) < DateTime.Now)
                        {
                            mLog.Info("The SPSite id Time out, New it again");
                            string mSiteUrl = tmpSite.Url;
                            tmpSite.Dispose();
                            mConfiguration.mInitialTime = DateTime.Now;
                            //tmpSite = new SPSite(mSiteUrl);
                            AveObjectModelFactory factory = mConfiguration.aveObjectModelFactory;

                            tmpSite = factory.CreateSite(mSiteUrl);
                            mDependencyObjs.PutIn(tmpSite, (int)CacheNodeType.SiteCollection, false);
                        }
                        IAveWeb tmpWeb = tmpSite.OpenWeb(web.ID);
                        if (tmpWeb == null)
                        {
                            throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessWebSPObjectNotFoundException, "Site", web.FullPath);
                        }
                        mDependencyObjs.PutIn(tmpWeb, (int)CacheNodeType.Web, false);
                    }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(web, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//web 级别 符合 web rule
                    {
                        return;
                    }

                    if (result != ProcessResult.SkipListNode)
                    {
                        await ProcessListCollectionAsync(web);
                    }
                    //Process web
                    await ProcessWebCollectionAsync(web);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("Web UserName Or Password Incorrect. Path:{0}. Message:{1}.", web.FullPath, IUPEx.ToString());
                    throw;
                }
                catch (SPObjectReadOnlyException snfe)
                {
                    mLog.Info("Web is ReadOnly. Path:{0}. Message:{1}.", web.FullPath, snfe.ToString());
                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("Web is Locked. Path:{0}. Message:{1}.", web.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("Web Not Found. Path:{0}. Message:{1}.", web.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing web node.Path:{0}. Message:{1}.", web.FullPath, e.ToString());
                    throw;
                }
            }
        }


        public virtual async Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            await Task.FromResult(0);
        }
        
        /// <summary>
        /// Process folder for initialization
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="needInitInfo"></param>
        public async virtual Task ProcessFolderAsync(ArchiverNodeItem folder, bool needInitInfo = false, List<int> itemIDs = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessFolder"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (folder.Parent != null && ListSkipCheck(folder.Parent))
                    {
                        return;
                    }

                    //Initialize parent node
                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, folder);
                    }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(folder, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//add for RevIM RECO-84
                    {
                        return;
                    }
                    await ProcessItemsAndSubfoldersAsync(folder, folder.Cache_NodeType, itemIDs);

                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("Folder UserName Or Password Incorrect. Path:{0}. Message:{1}.", folder.FullPath, IUPEx.ToString());
                    throw;
                }
                catch (SPObjectReadOnlyException sroe)
                {
                    mLog.Info("Folder is ReadOnly. Path:{0}. Message:{1}.", folder.FullPath, sroe.ToString());
                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("Folder is Locked. Path:{0}. Message:{1}.", folder.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("Folder Not Found. Path:{0}. Message:{1}.", folder.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing folder node.Path:{0}.Message:{1}.", folder.FullPath, e.ToString());
                    //throw; 非特定异常Folder Scan失败，不应该影响整体Job状态，Folder失败即可。SAAS-38055
                }
            }
        }
        
        public async virtual Task ProcessItemsAndSubfoldersAsync(ArchiverNodeItem folderNode, int folderLevel, List<int> itemIDs = null)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RealProcessItemsAndSubfolders"))
            {
                AveDiscoverFolder rootFolder = folderNode.DiscoverSPObject as AveDiscoverFolder;
                if (!ShouldProcessFolderContents(folderNode))
                {
                    return;
                }

                #region process items/documents
                try
                {
                    if (mConfiguration.SkipDiscoverItemForFolderLevelRule)
                    {
                        mLog.Info("Current rule is folder rule and skip discover folder sub items.Path:{0}.", folderNode.FullPath);
                    }
                    else
                    {
                        foreach (var items in rootFolder.GetItemsWithStructureForArchiver())
                        {
                            mLog.Info("Current GetItemsWithStructureForArchiver Items Count:{0}.", items.Count);
                            await ProcessDataAsync(items, itemIDs, rootFolder, folderNode, discoverWorker);
                            rootFolder.ClearSubItemsCache();
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.", folderNode.FullPath, ex.ToString());
                }
                #endregion

                #region process folders
                try
                {
                    foreach (var folders in rootFolder.GetFoldersWithStructure(true))
                    {
                        mLog.Info("Curent GetFoldersWithStructure folders Count:{0}.", folders.Count);
                        var folderIds = folders.Where(x => x.ID != null).Count() != 0 ? folders.Where(x => x.ID != null).Select(x => x.ID.Value).ToList() : new List<int>();
                        await ProcessDataAsync(folders, itemIDs, folderNode, discoverWorker);
                        rootFolder.ClearSubFoldersCache();
                        //Remove IAveFolder Cache.每次Query出的Folder外围处理结束后，清除当次Query缓存的IAveFolder，避免造成内存问题.
                        mLog.Info("Begin remove folder cache GetFoldersWithStructurForArchiver.RemomveCount:{0}.FullPath:{1}.", folderIds.Count, folderNode.FullPath);
                        rootFolder.RemoveFolderCache(folderIds);
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.", folderNode.FullPath, ex.ToString());
                }
                #endregion
                if (rootFolder != null)
                {
                    rootFolder.Dispose();
                }
            }
        }

        public abstract bool ListSkipCheck(ArchiverNodeItem list);

        public virtual async Task InitialSPObjectInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitialSPObjectInfo"))
            {

                mDiscoverSite = InitDiscoverSite(node); //tmpDiscoverSite;
                //初始化Site对象的一些信息。  
                Uri uri = new Uri(node.SiteUrl);
                mConfiguration.mInitialTime = DateTime.Now;
                InitKeyValueBoolean();

                if (mDiscoverSite.Site == null)
                {
                    throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessSiteSPObjectNotFoundException, "SiteCollection", node.FullPath);
                }
                mDependencyObjs.PutIn(mDiscoverSite.Site, (int)CacheNodeType.SiteCollection, false);
                if (node.SPNodeLevel == NodeLevel.SiteCollection)
                {
                    node.DiscoverSPObject = mDiscoverSite;
                    if (discoverWork != null)
                    {
                        ProcessResult result = await discoverWork.ProcessContainerAsync(node.GenerateWebappNodeItem(), ProcessType.NeedProcess);
                    }
                    return;
                }
                switch (node.SPNodeLevel)
                {
                    case NodeLevel.Site:
                        {
                            //ADO-189107 Folder/Site Rule，Backup Discover过程中不需要再赋值默认CacheNodeType。
                            if (mConfiguration.ObjectCache.ContainsKey(node.ID) && mConfiguration.ObjectCache[node.ID] == node.Cache_NodeType)
                            {
                                //node.Cache_NodeType = node.Cache_NodeType;
                            }
                            else if (node.Cache_NodeType <= (int)CacheNodeType.List / 2)
                            {
                                node.Cache_NodeType = (int)CacheNodeType.List / 2;
                            }
                            node = await InitNodeEntityRelatedInfoAsync(discoverWork, node, mConfiguration.AutoApproval, true);
                            //node.Name = (node.DiscoverSPObject as AveDiscoverWeb).Name;//防止取node name时只取site 的name，这里要取site 的相对name。
                            break;
                        }
                    case NodeLevel.Library:
                    case NodeLevel.List:
                        {
                            node.IsSystemObject = false;
                            node = await InitNodeEntityRelatedInfoAsync(discoverWork, node, mConfiguration.AutoApproval, true);
                            break;
                        }
                    case NodeLevel.RootFolder:
                    case NodeLevel.Folder:
                        {
                            IAveWeb spweb = null;
                            IAveList splist = null;
                            try
                            {
                                spweb = mDiscoverSite.Site.OpenWeb(node.WebId);
                            }
                            catch (Exception exc)
                            {
                                mLog.Info("Init Folder Level SPWeb" + exc.ToString());
                                spweb = mDiscoverSite.Site.OpenWeb();
                            }
                            mDependencyObjs.PutIn(spweb, (int)CacheNodeType.Web, false);
                            splist = spweb.GetList(node.FullPath);
                            mDependencyObjs.PutIn(splist, (int)CacheNodeType.List, false);
                            //当Folder Level大于5000时用原本的CacheNodeType，以保证添加到PC Container中 ADO-183775
                            if (node.Cache_NodeType <= (int)CacheNodeType.Item / 2)
                            {
                                node.Cache_NodeType = (int)CacheNodeType.Item / 2;
                            }
                            node.IsSystemObject = false;
                            node = await InitNodeEntityRelatedInfoAsync(discoverWork, node, mConfiguration.AutoApproval, true);
                            break;
                        }
                    default: break;
                }

                IAveWeb web = null;
                IAveList list = null;

                if (node.SPNodeLevel > NodeLevel.SiteCollection)
                {
                    try
                    {
                        web = mDiscoverSite.Site.OpenWeb(node.WebId);
                    }
                    catch (Exception exce)
                    {
                        mLog.Info("Get Final SPWeb" + exce.ToString());
                        web = mDiscoverSite.Site.OpenWeb();
                    }
                    mDependencyObjs.PutIn(web, (int)CacheNodeType.Web, false);
                }
                if (node.SPNodeLevel > NodeLevel.Site && web != null)
                {
                    list = web.GetList(node.FullPath);
                    mLog.Info("Current list [{0}] ItemCount [{1}].", node.FullPath, list.ItemCount);
                    mDependencyObjs.PutIn(list, (int)CacheNodeType.List, false);
                }
            }
        }
        private void InitKeyValueBoolean()
        {
            var bcsColumnValue = RMKeyValueDao.GetValueByKey("UseListLevelBCSColumn");
            var spQueryOneByOne = RMKeyValueDao.GetValueByKey("IsUseSPQueryOneByOne");
            if (bcsColumnValue != null)
            {
                mConfiguration.UseListLevelBCSColumn = Convert.ToBoolean(bcsColumnValue.Value);
            }
            if (spQueryOneByOne != null)
            {
                mConfiguration.IsUseSPQueryOneByOne = Convert.ToBoolean(spQueryOneByOne.Value);
            }
        }
        public static void AssignSPObjectId(SPTreeNodeDto node, ref RuleNodeContract config)
        {
            if (node.Level != NodeLevel.O365GroupSitesGroup
                && node.Level != NodeLevel.PrivateChannelGroup
                && node.Level != NodeLevel.SkyDriveProGroup
                && (node.Level >= NodeLevel.Folder || node.Level == NodeLevel.Sites || node.Level == NodeLevel.Lists))
            {
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.List)
            {
                config.ListId = node.SPObjectId;
                config.ListTitle = node.Name;
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.Site)
            {
                if (string.IsNullOrEmpty(config.WebId))
                {
                    config.WebId = node.SPObjectId;
                }
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.SiteCollection)
            {
                config.SiteId = node.ID;
                config.SiteUrl = node.Url;
                if (node.Parent != null)
                {
                    AssignSPObjectId(node.Parent, ref config);
                }
            }
            if (node.Level == NodeLevel.WebApplication
              || node.Level == NodeLevel.O365GroupSitesGroup
              || node.Level == NodeLevel.SkyDriveProGroup
              || node.Level == NodeLevel.PrivateChannelGroup)
            {
                config.WebAppId = node.SPObjectId;
                config.WebAppUrl = node.FullPath;
            }
        }

        #region private methods

        internal async Task ProcessDataAsync(List<AveDiscoverItem> items, List<int> itemIDs, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessData"))
            {
                foreach (AveDiscoverItem item in items)
                {
                    string itemFullUrl = item.FullUrl;
                    try
                    {
                        if (itemIDs != null && itemIDs.Count != 0)
                        {
                            if (!itemIDs.Contains(Convert.ToInt32(item.ID)))
                            {
                                continue;
                            }
                        }

                        if (LinkFileCommon.StubFileNameSuffixList.Contains(System.IO.Path.GetExtension(item.LeafName)) && item.CurrentItem != null
                            && item.CurrentItem.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                            && item.CurrentItem.FieldValues[LinkFileCommon.LinkFileFieldName] != null
                            && item.CurrentItem.FieldValues[LinkFileCommon.LinkFileFieldName].ToString().Length > 0)
                        {
                            mLog.Info($"skip stub file:{item.ID}");
                            continue;
                        }
                        await ProcessVersionAndAttachmentsAsync(item, rootFolder, folderNode, discoverWorker);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception exc)
                    {
                        mLog.Error(string.Format("Error in Backup Single Item :{0}.ItemFullPath:{1}.", exc.ToString(), itemFullUrl));
                    }
                    item.Dispose();
                }
            }
        }

        internal async virtual Task ProcessVersionAndAttachmentsAsync(AveDiscoverItem item, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersionAndAttachments"))
            {
                using (ArchiverNodeItem itemNode = folderNode.GenerateItemNodeItem(item, rootFolder, mConfiguration))
                {
                    ProcessResult result = await discoverWorker.ProcessItemAsync(itemNode, folderNode);
                    if (result == ProcessResult.CurrentVersionHasApprove)
                    {
                        return;
                    }

                    //Stopwatch watch = Stopwatch.StartNew();
                    ////Progress attachments 
                    //if (item.GetAttachments().Count > 0)
                    //{
                    //    foreach (AveItemObject attachment in item.GetAttachments())
                    //    {
                    //        await ProcessAttachmentsAsync(folderNode, itemNode, attachment, discoverWorker);
                    //    }
                    //}
                    ////Progress item versions
                    //if (item.GetVersions().Count > 1)
                    //{
                    //    foreach (AveVersionObject version in item.GetVersions())
                    //    {
                    //        if (version.Uiversion == item.Uiversion || version.Uiversion == 0)
                    //        {
                    //            continue;
                    //        }
                    //        try
                    //        {
                    //            await ProcessVersionsAsync(itemNode, version, folderNode, discoverWorker);
                    //        }
                    //        catch (JobStopException)
                    //        {
                    //            watch.Stop();
                    //            throw;
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanProcessItemVersionsError + ex.ToString());
                    //        }
                    //    }
                    //}

                    //watch.Stop();
                    //mLog.Info("ProcessVersionAndAttachments GetAttachments GetVersions costs: {0}.", watch.Elapsed);
                }
            }
        }

        internal async Task ProcessDataAsync(List<AveDiscoverFolder> folders, List<int> itemIDs, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            foreach (AveDiscoverFolder folder in folders)
            {
                using (new CheckJobStopScope()) { }
                if (itemIDs != null)
                {
                    if (!itemIDs.Contains(Convert.ToInt32(folder.ID)))
                    {
                        continue;
                    }
                }

                ArchiverNodeItem subFolderNode = folderNode.GenerateFolderNodeItem(folder, NodeLevel.Folder, mDiscoverSite.Site.Url, mConfiguration);
                ProcessResult result = await discoverWorker.ProcessContainerAsync(subFolderNode, ProcessType.NeedProcess);
                if (result == ProcessResult.SkipCurrentNode)
                {
                    continue;
                }

                await ProcessItemsAndSubfoldersAsync(subFolderNode, subFolderNode.Cache_NodeType);
                folder.Dispose();
            }
        }

        /// <summary>
        /// Convert tree node to RuleNodeContract.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RuleNodeContract ConvertTreeNodeToRuleNodeConfig(SPTreeNodeDto node, RuleNodeType type)
        {
            if (node == null)
            {
                return null;
            }
            RuleNodeContract result = new RuleNodeContract();
            result.Id = Guid.NewGuid().ToString();
            result.NodeId = node.SPObjectId;
            result.NodeName = node.Name;
            result.DisplayName = node.DisplayName;
            result.ManagerTreeId = node.ID;
            result.FullPath = node.FullPath;
            result.FarmId = node.FarmID;
            //result.SPType = node.SPType;
            if (node.NodeExtension != null && node.NodeExtension.BposInfo != null)
            {
                result.BposInfo = node.NodeExtension.BposInfo;
            }
            if (node.Parent != null)  //Farm 级别没有Parent
            {
                if (node.Parent.Level == NodeLevel.Sites || node.Parent.Level == NodeLevel.Lists || node.Parent.Level == NodeLevel.Folders)
                {
                    result.ParentNodeId = node.Parent.Parent == null ? null : node.Parent.Parent.SPObjectId;
                    result.ParentNodeName = node.Parent.Parent == null ? null : node.Parent.Parent.Name;
                }
                else
                {
                    result.ParentNodeId = node.Parent.SPObjectId;
                    result.ParentNodeName = node.Parent.Name;
                }
            }
            result.NodeLevel = node.Level;
            result.SPVersion = node.SPVersion;
            result.Type = type;
            AssignSPObjectId(node, ref result);
            //在处理index的时候需要转换children
            if (node.Children != null && node.Children.Count > 0 && type == RuleNodeType.IndexDevice)
            {
                result.Children = new List<RuleNodeContract>();
                foreach (SPTreeNodeDto child in node.Children)
                {
                    RuleNodeContract childRuleNode = new RuleNodeContract();
                    childRuleNode = ConvertTreeNodeToRuleNodeConfig(child, type);
                    if (childRuleNode != null)
                    {
                        childRuleNode.ParentNode = result;
                        result.Children.Add(childRuleNode);
                    }
                }
            }
            return result;
        }

        private AveDiscoverSite InitDiscoverSite(ArchiverNodeItem node)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitDiscoverSite"))
            {
                if (mDiscoverSite != null && string.Compare(mDiscoverSite.Site.Url, node.SiteUrl, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return mDiscoverSite;
                }

                if (mDiscoverSite != null)
                {
                    mDiscoverSite.Dispose();
                }
                var bposInfo = GetBposInfoBySite(node.SiteUrl);
                mFactory = MultiAppUtil.CreateAveObjectModelFactory(node.SiteUrl, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.

                try
                {
                    Site = mFactory.CreateSite(node.SiteUrl);
                }
                catch (Exception e)
                {
                    var we = e.InnerException as WebException;
                    if (we != null)
                    {
                        if (we.Status == WebExceptionStatus.ProtocolError)
                        {
                            var httpResp = we.Response as HttpWebResponse;
                            if (httpResp != null)
                            {
                                if (httpResp.StatusCode == HttpStatusCode.NotFound)
                                {
                                    mLog.Error("[DirtyData] SiteCollection {0} is deleted, error: {1}", node.FullPath, e.ToString());
                                    //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty, string.Empty, string.Empty, JobReportDetailStatus.Failed, "RM_SS_SiteRemovedFromDAO");
                                    throw;
                                }
                            }
                        }
                    }
                    if (bposInfo.ExsitAppProfile)
                    {
                        throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessSiteSPObjectNotFoundException, "SiteCollection", node.FullPath); ;
                    }
                    else
                    {
                        throw new Exception("RM_JM_AppProfile_NotFoundError");
                    }
                }
                #region RevIM job获取自定义属性
                try
                {
                    if (mConfiguration.IsILMode && mConfiguration.RuleCollection != null && mConfiguration.RuleCollection.Values.FirstOrDefault()?.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)
                    {
                        try
                        {
                            mLog.Info("Current rule is ArchiverRuleForRevIM job and need get site collection information, site collection url is :{0}.", node.FullPath);
                            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            try
                            {
                                node.SiteTitle = Site.RootWeb.Title;
                                columnCollectionOfDisplayName["author"] = Site.Owner.Name;
                                columnCollectionOfDisplayName["editor"] = Site.RootWeb.CurrentUser.Name;
                                node.ItemDisplayColumns = columnCollectionOfDisplayName;
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("Get Version Properties Error{0}", e.ToString());
                            }
                        }
                        catch (Exception exp)
                        {
                            mLog.Warn("Error in Get item columns : " + exp.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("Get RevIM Binding Column Errror, message: {0}", e.ToString());
                }
                #endregion
                AveDiscoverSite tmpDiscoverSite = TryGetIncrementalDiscoverRange(out var changeStart, out var changeEnd)
                    ? new AveDiscoverSite(Site, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, changeStart, changeEnd)
                    : new AveDiscoverSite(Site, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);

                return tmpDiscoverSite;
            }
        }
        private bool TryGetIncrementalDiscoverRange(out DateTime startTime, out DateTime endTime)
        {
            startTime = DateTime.MinValue;
            endTime = DateTime.MinValue;

            if (!UseIncrementalDiscover || mConfiguration == null)
            {
                return false;
            }

            var startTicks = mConfiguration.IncrementalDiscoverStartTimeTicks;
            var endTicks = mConfiguration.IncrementalDiscoverEndTimeTicks;
            if (startTicks <= DateTime.MinValue.Ticks || endTicks <= DateTime.MinValue.Ticks || startTicks >= endTicks)
            {
                return false;
            }

            startTime = new DateTime(startTicks, DateTimeKind.Utc);
            endTime = new DateTime(endTicks, DateTimeKind.Utc);
            return true;
        }

        internal async Task ProcessListCollectionAsync(ArchiverNodeItem web)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessListCollection"))
            {
                AveDiscoverWeb discoverWeb = web.DiscoverSPObject as AveDiscoverWeb;
                Dictionary<Guid, AveDiscoverList> discoveryLists = GetListsForWeb(discoverWeb);
                foreach (AveDiscoverList list in discoveryLists.Values)
                {
                    if (ShouldHandleDeletedList(list))
                    {
                        await HandleDeletedListAsync(web, list);
                        continue;
                    }
                    mLog.Info("Begin discover list, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);

                    try
                    {
                        //arthur: need complete this {system folder} logical later. add to scandiscoverNodeWorker
                        if (list.Title.Equals("{System Folder}"))
                        {
                            mLog.Info("Current list is System Folder when discover list collection, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                            ArchiverNodeItem listnode = web.GenerateListNodeItem(list, null);
                            listnode.FullPath = listnode.Parent.FullPath;
                            await ProcessListAsync(listnode);
                        }
                        else
                        {
                            IAveWeb tmpWeb = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as IAveWeb;

                            IAveList tmpList = tmpWeb.GetList(list.RootFolderUrl);
                            mLog.Info("Current list [{0}] ItemCount [{1}].", list.RootFolderUrl, tmpList.ItemCount);
                            ArchiverNodeItem listNode = web.GenerateListNodeItem(list, tmpList);
                            if (ListSkipCheck(listNode))
                            {
                                continue;
                            }
                            mDependencyObjs.PutIn(tmpList, (int)CacheNodeType.List, false);
                            using (listNode)
                            {
                                await ProcessListAsync(listNode);
                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARScanProcessListCollectionError, list.Title, ex.ToString());
                        throw;
                    }
                }
            }
        }

        internal async Task ProcessWebCollectionAsync(ArchiverNodeItem web)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWebCollection"))
            {
                Dictionary<Guid, AveDiscoverWeb> discoverWebs = GetSubWebs((AveDiscoverWeb)web.DiscoverSPObject);
                foreach (AveDiscoverWeb tmp in discoverWebs.Values)
                {
                    try
                    {
                        if (ShouldHandleDeletedWeb(tmp))
                        {
                            await HandleDeletedWebAsync(web, tmp);
                            continue;
                        }
                        using (ArchiverNodeItem webnode = web.GenerateSiteNodeItem(tmp, mConfiguration, web.Parent.SPNodeLevel == NodeLevel.SiteCollection))
                        {
                            await ProcessWebAsync(webnode);
                        }
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (SPObjectNotFoundException e1)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARScanProcessWebProcressSubWeb, e1);
                    }
                    finally
                    {
                        using (tmp)
                        { };
                    }
                }
            }
        }

        protected virtual async Task HandleDeletedWebAsync(ArchiverNodeItem parentWebNode, AveDiscoverWeb deletedWeb)
        {
            if (deletedWeb == null)
            {
                return;
            }

            if (discoverWorker is ISiteMetricsDeletionHandler deletionHandler)
            {
                await deletionHandler.RemoveWebDataAsync(deletedWeb.WebID, deletedWeb.FullUrl);
            }
            else
            {
                mLog.Warn($"Detected deleted web {deletedWeb.FullUrl}, but no deletion handler is available; cached data might become stale.");
            }
        }

        protected virtual Dictionary<Guid, AveDiscoverWeb> GetWebsForSiteCollection(AveDiscoverSite discoverySite)
        {
            return discoverySite?.GetWebs() ?? new Dictionary<Guid, AveDiscoverWeb>();
        }

        protected virtual bool ShouldProcessFolderContents(ArchiverNodeItem folderNode)
        {
            return true;
        }

        protected virtual Dictionary<Guid, AveDiscoverList> GetListsForWeb(AveDiscoverWeb discoverWeb)
        {
            return discoverWeb?.GetLists() ?? new Dictionary<Guid, AveDiscoverList>();
        }

        protected virtual bool ShouldHandleDeletedList(AveDiscoverList list)
        {
            return false;
        }

        protected virtual Dictionary<Guid, AveDiscoverWeb> GetSubWebs(AveDiscoverWeb discoverWeb)
        {
            if (discoverWeb == null)
            {
                return new Dictionary<Guid, AveDiscoverWeb>();
            }

            return mConfiguration.Procedure == ScheduleProcedure.Scan
                ? discoverWeb.GetSubWebs(true)
                : discoverWeb.GetSubWebs();
        }

        protected virtual bool ShouldHandleDeletedWeb(AveDiscoverWeb discoverWeb)
        {
            return false;
        }

        protected virtual async Task HandleDeletedListAsync(ArchiverNodeItem parentWebNode, AveDiscoverList deletedList)
        {
            if (deletedList == null)
            {
                return;
            }

            var webId = parentWebNode?.WebId ?? Guid.Empty;
            var listIdentifier = string.IsNullOrWhiteSpace(deletedList.RootFolderUrl)
                ? deletedList.ListId.ToString("D")
                : deletedList.RootFolderUrl;

            if (discoverWorker is ISiteMetricsDeletionHandler deletionHandler)
            {
                await deletionHandler.RemoveListDataAsync(webId, deletedList.ListId, listIdentifier);
            }
            else
            {
                mLog.Warn($"Detected deleted list {listIdentifier}, but no deletion handler is available; cached data might become stale.");
            }
        }

        private async Task<ArchiverNodeItem> InitNodeEntityRelatedInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node, bool autoApproval, bool firstCall = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitNodeEntityRelatedInfo"))
            {
                ArchiverNodeItem parent = new ArchiverNodeItem();
                //parent.ID = node.ID;
                parent.WebApplicationUrl = node.WebApplicationUrl;
                parent.SiteId = node.SiteId;
                parent.WebId = node.WebId;
                parent.ListId = node.ListId;
                parent.FullPath = node.FullPath;
                parent.SiteUrl = node.SiteUrl;
                switch (node.SPNodeLevel)
                {
                    case NodeLevel.SiteCollection:
                        {
                            IAveSite site = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                            node = parent.GenerateSiteCollectionNodeItem(site, mConfiguration);
                            await InitialSPObjectInfoAsync(discoverWork, node);
                            break;
                        }
                    case NodeLevel.Site:
                        {
                            IAveSite site = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                            bool isRootWeb = false;
                            bool parentIsRootWeb = false;
                            //Merge Code for CI ADO-94697
                            try
                            {
                                using (IAveWeb web = site.OpenWeb(node.ID))
                                {
                                    if (web.IsRootWeb)
                                    {
                                        parent.SPNodeLevel = NodeLevel.SiteCollection;
                                        isRootWeb = true;
                                    }
                                    else
                                    {
                                        parent.SPNodeLevel = NodeLevel.Site;
                                        parent.ID = web.ParentWebId;
                                        parentIsRootWeb = web.ParentWebId.Equals(site.RootWeb.ID);
                                    }
                                    #region records web property
                                    if (mConfiguration.RuleCollection != null && mConfiguration.RuleCollection[1].ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM)
                                    {
                                        try
                                        {
                                            mLog.Info("Current rule is ArchiverRuleForRevIM job and need get site information, site collection url is :{0}.", node.FullPath);
                                            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                                            try
                                            {
                                                if (web.IsRootWeb)
                                                {
                                                    columnCollectionOfDisplayName["author"] = site.Owner.LoginName;
                                                }
                                                else
                                                {
                                                    columnCollectionOfDisplayName["author"] = web.Author.LoginName;
                                                }
                                                columnCollectionOfDisplayName["editor"] = web.CurrentUser.Name;
                                                node.ItemDisplayColumns = columnCollectionOfDisplayName;
                                                node.SiteTitle = web.Title;
                                            }
                                            catch (Exception e)
                                            {
                                                mLog.Warn("Get Version Properties Error{0}", e.ToString());
                                            }
                                        }
                                        catch (Exception exp)
                                        {
                                            mLog.Warn("Error in Get item columns : " + exp.ToString());
                                        }
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("Open web with id error, node id {0},{1}", node.ID, e.ToString());
                                mLog.Info("Will Open web with url again, node url {0}", node.FullPath);
                                using (IAveWeb web = site.OpenWeb(node.FullPath))
                                {
                                    if (web != null && web.Exists)
                                    {
                                        node.ID = web.ID;
                                        if (web.IsRootWeb)
                                        {
                                            parent.SPNodeLevel = NodeLevel.SiteCollection;
                                            isRootWeb = true;
                                        }
                                        else
                                        {
                                            parent.SPNodeLevel = NodeLevel.Site;
                                            parent.ID = web.ParentWebId;
                                            parentIsRootWeb = web.ParentWebId.Equals(site.RootWeb.ID);
                                        }
                                    }
                                    else
                                    {
                                        if (I18NEntity.HasKey(e.Message))
                                        {
                                            throw;
                                        }
                                        throw new SPObjectNotFoundException("StorageOptimization_SOARScanProcessWebSPObjectNotFoundException");
                                    }
                                }
                            }
                            if (isRootWeb)
                            {
                                node.Name = ".";
                            }
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            AveDiscoverWeb discoverWeb = null;
                            if (parent.DiscoverSPObject is AveDiscoverSite)
                            {
                                discoverWeb = ((AveDiscoverSite)parent.DiscoverSPObject).GetRootWeb();
                            }
                            else
                            {
                                discoverWeb = ((AveDiscoverWeb)parent.DiscoverSPObject).GetSubWebs()[node.ID];
                            }

                            if (!firstCall)
                            {
                                node = parent.GenerateSiteNodeItem(discoverWeb, mConfiguration, isRootWeb | parentIsRootWeb);
                            }
                            if (node.DiscoverSPObject == null)
                            {
                                node.DiscoverSPObject = discoverWeb;
                            }

                            break;
                        }
                    case NodeLevel.Library:
                    case NodeLevel.List:
                        {
                            parent.SPNodeLevel = NodeLevel.Site;
                            parent.ID = node.WebId;
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            using (IAveWeb webs = mDiscoverSite.Site.OpenWeb(node.WebId))
                            {
                                IAveList lists = webs.GetList(node.FullPath);
                                if (null == lists)
                                {
                                    mLog.Warn(string.Format("List {0} Is Null", node.FullPath));
                                    throw new SPObjectNotFoundException("RM_JM_GlobalSearch_CannotFindExchangeItem");
                                }
                                mLog.Info("Current list [{0}] ItemCount [{1}].", node.FullPath, null == lists ? 0 : lists.ItemCount);
                                node.ID = lists.ID;
                                node.Name = lists.Title;
                                node.SPList = lists;
                                if (node.DiscoverSPObject == null)
                                {
                                    AveDiscoverList discoverList = ((AveDiscoverWeb)parent.DiscoverSPObject).GetLists()[node.ID];
                                    if (!firstCall)
                                    {
                                        node = parent.GenerateListNodeItem(discoverList, lists);
                                    }
                                    else
                                    {
                                        node.IsRecord = ArchiverCommonStaticMethod.CheckListRecord(lists);
                                    }
                                    node.DiscoverSPObject = discoverList;
                                    node.ListType = discoverList.Type;
                                }
                                if (mConfiguration.IsILMode && discoverWorker is RecordsOneDriveScanDiscovrerNodeWorker)
                                {
                                    ((RecordsOneDriveScanDiscovrerNodeWorker)discoverWorker).InitOneDriveItemTermInfoByListId(mConfiguration.SiteCollectionID, lists.ID);
                                }
                            }
                            break;
                        }
                    case NodeLevel.RootFolder:
                        {
                            parent.SPNodeLevel = NodeLevel.List;
                            parent.ID = node.ListId;
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            if (node.DiscoverSPObject == null)
                            {
                                AveDiscoverFolder discoverRootFolder = null;
                                //一个List下的Root Folder，只实例化一次即可。对于同一个List下的多个Subfolder符合rule，只需要实例化一次，减少性能浪费.
                                if (mInitNodeEntityRelatedInfoDiscoverRootFolder == null || mInitNodeEntityRelatedInfoRootFolderId != node.ID)
                                {
                                    mLog.Info("Init rootfolder for InitNodeEntityRelatedInfo.Url:{0}.FolderID:{1}.", parent.FullPath, node.ID);
                                    discoverRootFolder = ((AveDiscoverList)parent.DiscoverSPObject).GetRootFolder(true);
                                    mInitNodeEntityRelatedInfoDiscoverRootFolder = discoverRootFolder;
                                    mInitNodeEntityRelatedInfoRootFolderId = node.ID;
                                }
                                else
                                {
                                    mLog.Info("Current list already init rootfolder for InitNodeEntityRelatedInfo.Url:{0}.", parent.FullPath);
                                    discoverRootFolder = mInitNodeEntityRelatedInfoDiscoverRootFolder;
                                }
                                if (!firstCall)
                                {
                                    node = parent.GenerateFolderNodeItem(discoverRootFolder, NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
                                }
                                else
                                {
                                    node.IsRecord = parent.IsRecord;
                                }
                                node.DiscoverSPObject = discoverRootFolder;
                                //rootFolder listType 是 1 需要0 取list的listType
                                node.ListType = ((AveDiscoverList)parent.DiscoverSPObject).Type;
                            }
                            break;
                        }
                    case NodeLevel.Folder:
                        {
                            IAveList list = (IAveList)mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List);
                            #region Old Logic
                            //Wrapper list.GetItemByUniqueId(node.ID).Folder方法和nodeItemFolder.ParentFolder逻辑有问题，外围不再调用
                            //IAveFolder nodeItemFolder;
                            //try
                            //{
                            //    //nodeItemFolder = list.GetItemByUniqueId(node.ID).Folder;
                            //    nodeItemFolder = list.GetFolder(node.FullPath.TrimEnd('/'));
                            //}
                            //catch (Exception exception)
                            //{
                            //    mLog.Info("Error in Folder Level, Get Folder:{0}.", exception.ToString());
                            //    nodeItemFolder = list.GetFolder(node.WebApplicationUrl + node.FullPath);
                            //    node.ID = nodeItemFolder.UniqueId;
                            //}
                            //IAveFolder nodeItemFolder = list.GetItemByUniqueId(node.ID).Folder;
                            //parent.ID = nodeItemFolder.ParentFolder.UniqueId;
                            #endregion
                            string tempFolderPath = "/" + node.FullPath.TrimStart('/').TrimEnd('/');
                            string parentFolderPath = AveUrlUtility.GetParentUrl(tempFolderPath);
                            mLog.Info("InitNodeEntityRelatedInfo parentFolderPath:{0}.", parentFolderPath);

                            try
                            {
                                if (mConfiguration.IsILMode)
                                {
                                    if (!parentFolderPath.StartsWith(list.ParentWeb.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
                                    {
                                        parentFolderPath = list.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + parentFolderPath.TrimStart('/');
                                        mLog.Info("InitNodeEntityRelatedInfo need combine folder server relative url, parentFolderPath:{0}.", parentFolderPath);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                mLog.Warn($"Combine folder server relative url error:{e}");
                            }

                            parent.ID = list.GetFolder(parentFolderPath).UniqueId;
                            mLog.Info("InitNodeEntityRelatedInfo parentFolderID:{0}.", parent.ID);
                            parent.FullPath = parentFolderPath;
                            if (parent.ID.Equals(list.RootFolder.UniqueId))
                            {
                                parent.SPNodeLevel = NodeLevel.RootFolder;
                                parent.ListId = list.ID;
                            }
                            else
                            {
                                parent.SPNodeLevel = NodeLevel.Folder;
                            }
                            parent = await InitNodeEntityRelatedInfoAsync(discoverWork, parent, autoApproval);
                            if (node.DiscoverSPObject == null)
                            {
                                Guid tempNodeId = node.ID;
                                AveDiscoverFolder discoverFolde = ((AveDiscoverFolder)parent.DiscoverSPObject).GetSubFolders().FirstOrDefault(tmp => tmp.DocID.Equals(tempNodeId));
                                if (discoverFolde == null)
                                {
                                    throw new SPObjectNotFoundException("RM_JM_GlobalSearch_CannotFindExchangeItem");
                                }
                                if (!firstCall)
                                {
                                    node = parent.GenerateFolderNodeItem(discoverFolde, NodeLevel.Folder, mDiscoverSite.Site.Url, mConfiguration);
                                }
                                else
                                {
                                    node.ListType = parent.ListType;
                                    node.IsRecord = parent.IsRecord;
                                    //ADO-165559 folder node export rule,LibRowID需要手动赋值
                                    node.LibRowID = discoverFolde.ID == null ? -1 : discoverFolde.ID.Value;
                                    //node.IsMicroFeedList = parent.IsMicroFeedList;
                                    node.Modified = discoverFolde.TimeLastModified.Ticks;
                                }
                                node.DiscoverSPObject = discoverFolde;
                            }
                            break;
                        }
                    default:
                        break;
                }
                node.Parent = parent;
                if (node.SPNodeLevel != NodeLevel.SiteCollection)//防止冲掉webapp
                {
                    //discoverWork.ProcessContainerLevelNodeWithRule(parent);
                    //递归的时候不checkRule
                    ProcessResult result = await discoverWork.ProcessContainerAsync(parent, ProcessType.NoNeedProcess);
                }
                return node;
            }
        }

        private AveBPOSAccountInfo GetBposInfoBySite(string siteUrl)
        {
            lock (locker)
            {
                if (_bposCache.ContainsKey(siteUrl))
                {
                    return _bposCache[siteUrl];
                }
                else
                {
                    GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    AveBPOSAccountInfo aveBPOSAccountInfo = PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection).Result;
                    _bposCache.Add(siteUrl, aveBPOSAccountInfo);
                    return aveBPOSAccountInfo;
                }
            }
        }

        public int CaculateListCount(ArchiverNodeItem node)
        {
            int result = 0;
            switch (node.SPNodeLevel)
            {
                case NodeLevel.SiteCollection:
                    result = CaculateSiteListCount(node);
                    break;
                case NodeLevel.Site:
                    result = CaculateWebListCount(node, true, null);
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    result++;
                    break;
                case NodeLevel.RootFolder:
                case NodeLevel.FSFolder:
                case NodeLevel.Folder:
                case NodeLevel.Item:
                    result++;
                    break;
                default:
                    throw new Exception("Unknown Level");
            }
            mLog.Info($"CaculateSiteCollectionItemCount.TotalItemCount:{totalScanCount}.");
            return result;

        }

        private int CaculateSiteListCount(ArchiverNodeItem site)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.CaculateSiteListCount"))
            {
                int result = 0;
                try
                {
                    using (new CheckJobStopScope()) { }
                    //初始化Site对应的一些信息。
                    AveObjectModelFactory factory = mConfiguration.aveObjectModelFactory;
                    IAveSite aveSite = factory.CreateSite(site.SiteUrl);
                    if (aveSite == null)
                    {
                        throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanCaculateSiteListCount, "SiteCollection", site.FullPath);
                    }
                    AveDiscoverSite discoverSite = new AveDiscoverSite(factory.CreateSite(), null, AveDiscoveryKind.API, DiscoverModule.Archive);

                    #region//去掉不用的方法
                    //InitialSPObjectInfo(null, ref site);
                    ////If the rootWeb has defined a unique rule, we should skip all the site collection.
                    ////URL of RootWeb is same as sitecollection's
                    //IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                    #endregion

                    using (discoverSite)
                    {
                        using (var web = discoverSite.GetRootWeb())
                        {
                            using (ArchiverNodeItem webnode = site.GenerateSiteNodeItem(web, mConfiguration, true))
                            {
                                result = CaculateWebListCount(webnode, false, web);
                            }
                        }
                    }
                    if (aveSite != null)
                    {
                        aveSite.Dispose();
                        aveSite = null;
                    }
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverCalculateCountSuccess, result);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (SPObjectNotFoundException snfe)
                {
                    mLog.Info("Site Collection Not Found " + snfe.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanCaculateSiteListCount, e);
                }
                return result;
            }
        }

        private int CaculateWebListCount(ArchiverNodeItem web, bool needInitInfo, AveDiscoverWeb discoverWeb)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.CaculateWebListCount"))
            {
                int result = 0;
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (needInitInfo)
                    {
                        mConfiguration.mInitialTime = DateTime.Now;
                    }

                    if (discoverWeb != null)
                    {
                        var discoverLists = discoverWeb.GetLists();
                        OutPutListItemCount(discoverLists);
                        var subWebs = discoverWeb.GetSubWebs();
                        result = discoverLists.Count;
                        foreach (var subWeb in subWebs.Values)
                        {
                            try
                            {
                                discoverLists = subWeb.GetLists();
                                result += discoverLists.Count;
                                //处理SubSite下面的Subsite
                                CaculateWebListCount(web, needInitInfo, subWeb);
                            }
                            catch (JobStopException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverGetSubSiteListCountError, ex.Message);
                            }
                            finally
                            {
                                web.Dispose();
                                subWeb.Dispose();
                            }
                        }
                    }
                    else
                    {
                        AveObjectModelFactory factory = null;
                        factory = mConfiguration.aveObjectModelFactory;
                        using (IAveSite site = factory.CreateSite())
                        {
                            //TODO: Need to take Explicit inclusion into consideration
                            string webUrl = AveUrlUtility.GetServerRelativeUrl(web.FullPath);
                            //string webUrl = web.FullPath.Substring(site.WebApplication.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString().Length);
                            discoverWeb = new AveDiscoverWeb(site, webUrl, DiscoverModule.Archive, factory);
                            var discoverLists = discoverWeb.GetLists();
                            OutPutListItemCount(discoverLists);
                            result = discoverLists.Count;
                            var subWebs = discoverWeb.GetSubWebs();
                            foreach (var subWeb in subWebs.Values)
                            {
                                using (new CheckJobStopScope()) { }
                                try
                                {
                                    discoverLists = subWeb.GetLists();
                                    result += discoverLists.Count;
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverGetSubWebListCountError, ex.Message);
                                }
                                finally
                                {
                                    subWeb.Dispose();
                                }
                            }
                            if (discoverWeb != null)
                            {
                                web.Dispose();
                                discoverWeb.Dispose();
                            }
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (SPObjectNotFoundException snfe)
                {
                    mLog.Info("Site Collection Not Found " + snfe.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanCaculateWebListCount, e);
                }
                finally
                {
                }
                return result;
            }
        }
        private void OutPutListItemCount(Dictionary<Guid, AveDiscoverList> discoverLists)
        {
            foreach (var list in discoverLists)
            {
                try
                {
                    if (list.Value != null)
                    {
                        totalScanCount += list.Value.ItemCount;
                        mLog.Info($"CaculateWebListCount.ListUrl:{list.Value.RootFolderUrl}.ListTotalCount:{list.Value.ItemCount}.");
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn($"An error occurred while calculate list count. {e}.");
                }
            }
        }
        #endregion
        public virtual void Dispose()
        {
            discoverWorker.Dispose();
        }

    }
}