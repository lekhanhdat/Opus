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
using AngleSharp.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.Archiver.Scan.DiscorverScan;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Discover.InsightsEngine;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using AvePoint.Wrapper.Discovery;
using Azure.Storage.Blobs;
using Cloud.Sdk.Data.IE;
using DataOrchestration.Tag.Sdk;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Analyzer;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using DocumentFormat.OpenXml.Wordprocessing;
using HsmBackup.Shared;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using RAArchiverCommon.DiscoveryArchiveJob;
using RAArchiverCommon.HSMArchiver;
using SPDisposeCheck;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.Archiver.Scan
{
    using AvePoint.RA.Common.Configurations;
    using AvePoint.RA.Contract.Common;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.RA.DB.Model;
    using AvePoint.RA.I18N.Core;
    using AvePoint.RA.RACommonUtility.Common;
    using LiteDB;
    using System.IO;
    using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;

    public class HSMXmlArchiverScanner : ISharePointScanner
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(HSMXmlArchiverScanner));
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        private readonly object locker = new();
        private IScanDataReader mScanDataReader = null;
        private long totalScanCount = 0;
        protected HSMItemManager itemManager;
        public static IEDataOptimizationService discoverDBService;
        internal AveDiscoverSite mDiscoverSite = null;
        internal IBackwardDependencyNodeCache<object> mDependencyObjs;
        internal ScanJobSettings jobSettings = null;
        internal ScheduleConfiguration mConfiguration = null;
        internal Guid scopeId = Guid.Empty;
        internal Guid groupId = Guid.Empty;
        internal AveObjectModelFactory mFactory = null;

        private IAveList mSPQueryList = null;
        private int mMaxItemIdInLibrary = 0;
        private SPOFolder SPORootFolder = null;
        public const string SP_ID = "ID";
        public const string SP_UniqueID = "UniqueId";
        private CAMLManager mCAMLManager = null;
        private string mWebAppName = string.Empty;
        private string mSiteUrl = string.Empty;
        public bool siteStorageLimit;
        public AveDiscoverFolder mInitNodeEntityRelatedInfoDiscoverRootFolder = null;
        public Guid mInitNodeEntityRelatedInfoRootFolderId = Guid.Empty;
        private HSMScanWorker mDiscoverWorker = null;
        private List<string> DesignLists = new List<string>();
        private readonly ConcurrentDictionary<string, ManifestDiscoveredItem> manifestItemCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<Guid, List<ManifestDiscoveredItem>> manifestEntriesByListId = new();
        private static readonly ConcurrentDictionary<string, Dictionary<string, List<AveRoleAssignmentInfo>>> roleAssignmentsByManifestPath = new(StringComparer.OrdinalIgnoreCase);
        private const string DeploymentManifestNamespace = "urn:deployment-manifest-schema";
        private static readonly IReadOnlyList<AveRoleAssignmentInfo> EmptyRoleAssignments = Array.Empty<AveRoleAssignmentInfo>();

        public IDiscoverNodeWorker discoverWorker
        {
            get
            {
                if (mDiscoverWorker == null)
                {
                    mDiscoverWorker = new HSMScanWorker(jobSettings, mConfiguration, mDependencyObjs);
                }
                return mDiscoverWorker;
            }
            set { }
        }
        public static IEDataOptimizationService Instance
        {
            get
            {
                return discoverDBService;
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

        public PCContainer<ArchiveApproveReport> GetPCContainer()
        {
            if (mDiscoverWorker == null)
            {
                return (discoverWorker as AnalysisOptimizationDiscoverNodeWorker).pcContainer;
            }
            else
            {
                return mDiscoverWorker.pcContainer;
            }

        }
        protected IAveSite Site { get; set; }
        internal string WebAppName
        {
            get
            {
                if (string.IsNullOrEmpty(mWebAppName))
                {
                    int indexOfSlash = mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
                    mWebAppName = mSiteUrl;
                    if (indexOfSlash != -1)
                    {
                        mWebAppName = mSiteUrl.Substring(0, mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
                    }
                }
                return mWebAppName;
            }
        }

        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private RMDiscoveryOptimizeDataSettingDto mRMDiscoveryOptimizeDataSettingDto = null;
        private RMDiscoveryAOSPOptimizeDataSettingDto mRMDiscoveryAOSPOptimizeDataSettingDto = null;
        private List<DiscoverTagRule> discoverTagRules = new List<DiscoverTagRule>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private string StorageConnectionString;
        private string ContentStorageConnectionString;
        private string ContainerName;
        public HSMXmlArchiverScanner(ScanJobSettings scanJobSettings)
        {
            mDependencyObjs = new BackwardDependenceNodeCache<object>();
            jobSettings = scanJobSettings;
            mConfiguration = scanJobSettings.Configuration;
            mScanDataReader = new ScanDataReader(mConfiguration);
            this.DesignLists = GetDesignLists();
            var storageInfo = StorageDeviceService.GetStorageDeviceById(scanJobSettings.SourceDataStorageId);
            if (storageInfo != null && storageInfo.Status == RMConstants.STORAGE_OLD_DATA_TYPE)
            {
                throw new Exception("This storage has been removed from Opus!");
            }
            if (!string.IsNullOrEmpty(scanJobSettings.DataContentStorageId))
            {
                mLog.Info("this HSM archiver job is use other content storage");
                var contentStorageInfo = StorageDeviceService.GetStorageDeviceById(scanJobSettings.DataContentStorageId);
                if (contentStorageInfo != null && contentStorageInfo.Status == RMConstants.STORAGE_OLD_DATA_TYPE)
                {
                    throw new Exception("This storage has been removed from Opus!");
                }
                ContentStorageConnectionString = contentStorageInfo.ConnectionString;
            }

            if (scanJobSettings.SourceDataStorageId.Equals(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase) || storageInfo.Type != (int)AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType.CloudAzure)
            {
                throw new Exception("Souce data storage only support byos azure storage");
            }
            StorageConnectionString = storageInfo.ConnectionString;
            WrapperConfiguration.SpecifyReportStorageXRIString = StorageConnectionString;
            ContainerName = storageInfo.mCurrentXRI.Params["containername"];

        }

        public async Task RealRun()
        {
            try
            {
                using (new CheckJobStopScope()) { }
                await RunHSMXmlOptimizationAsync();
            }
            catch (JobStopException)
            {
                mLog.Error($"Job was stop, stop scan");
            }
            catch (AveExceedStorageLimitException)
            {
                siteStorageLimit = true;
                mConfiguration.JobReportDto.AddScanReport(mSiteUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_JM_SiteStorageLimit_ErrorMessage");
                mLog.Error($"This site has exceeded its maximum file storage limit,set siteStorageLimit = true");
            }
            catch (Exception e)
            {
                mLog.Error($"some thing went wrong when RunDiscoverOptimization ,error :{e.ToString()}");
            }
        }
        public async System.Threading.Tasks.Task RunHSMXmlOptimizationAsync()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SharePointScanner.Run"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings.TreeNode);
                    var ruleNode = ConvertHSMTreeNodeToRuleNodeConfig(node, RuleNodeType.Archiver);
                    ArchiverNodeItem selectNodeItem = new ArchiverNodeItem(ruleNode);
                    JobExecutionProcessStatisticExecutor.Instance.StartCalculateRuleAndSummary(selectNodeItem.SPNodeLevel.ToString(), selectNodeItem.FullPath);
                    try
                    {
                        selectNodeItem.SiteId = new Guid(node.SPObjectId);
                        selectNodeItem.Name = node.FullUrl;
                        mSiteUrl = node.FullUrl;
                        var count = CaculateListCount(selectNodeItem);
                        mLog.Info($"Scan caculate list count is {count}");
                        mConfiguration.ProgressDto.SetBaseCount4Phase(count);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"Scan caculate list count error {e}");
                    }
                    await ProcessSiteCollectionAsync(selectNodeItem);
                    mDependencyObjs.Flush();
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveExceedStorageLimitException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while scanning error {0}", e.ToString());
                }
                finally
                {
                    discoverWorker.Flush();
                    mDiscoverWorker.pcContainer.EndProduce();
                    JobExecutionProcessStatisticExecutor.Instance.EndCalculateRuleAndScanSummary(totalScanCount, Site);
                }
            }
        }


        public async System.Threading.Tasks.Task RunAsync()
        {
            await RealRun();
        }

        public virtual List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId = "")
        {
            return ArchiverSettingDao.LoadBreakInheritNodeUrls(scopeUrl, siteObjectId, mConfiguration.IsTeams);
        }


        public IScanDataReader GetScanDataReader()
        {
            return mScanDataReader;
        }

        public virtual async System.Threading.Tasks.Task ProcessSiteCollectionAsync(ArchiverNodeItem sitecollection)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    try
                    {
                        using (new CheckJobStopScope()) { }
                        //初始化Site对应的一些信息。
                        await InitialSPObjectInfoAsync(discoverWorker, sitecollection);
                        //If the rootWeb has defined a unique rule, we should skip all the site collection.
                        //URL of RootWeb is same as sitecollection's
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;

                        //对这个Site检查Rule.
                        ProcessResult result = (await discoverWorker.ProcessContainerAsync(sitecollection, ProcessType.NeedProcess));
                        if (result == ProcessResult.SkipCurrentNode)
                        {
                            mLog.Info("skip current Node {0}", sitecollection.FullPath);
                            return;
                        }

                        using (AveDiscoverSite discoverySite = sitecollection.DiscoverSPObject as AveDiscoverSite)
                        {
                            using (AveDiscoverWeb rootWeb = discoverySite.GetRootWeb())
                            {
                                using (ArchiverNodeItem webnode = sitecollection.GenerateSiteNodeItem(rootWeb, mConfiguration, true))
                                {
                                    string rootWebSiteLogoDescription = rootWeb.AveWeb.SiteLogoDescription;//通过调用SiteLogoDescription自动创建出Site Assets List 
                                    await ProcessWebAsync(webnode);
                                }
                            }
                        }

                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (AveExceedStorageLimitException)
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
            catch (JobStopException)
            {
                throw;
            }
            catch (AveExceedStorageLimitException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLog.Error("Process sitecollection error {0}", e.ToString());
                //TO DO Add Detail
                //TO DO I18N
                //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty,
                //    string.Empty, string.Empty, JobReportDetailStatus.Failed, e.Message);
            }
            finally
            {
                mDiscoverSite = null;
            }
        }

        [SPDisposeCheckIgnoreAttribute(SPDisposeCheckID._120, "Ignore")]
        public async virtual System.Threading.Tasks.Task ProcessWebAsync(ArchiverNodeItem web, bool needInitInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWeb"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }

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
                    //TODO:Disable language mapping
                    //ScheduleLanguageMapping.ProcessLanguageMapping(tmpWeb);
                    mDependencyObjs.PutIn(tmpWeb, (int)CacheNodeType.Web, false);
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(web, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//web 级别 符合 web rule
                    {
                        return;
                    }
                    await ProcessListCollectionAsync(web);
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

        /// <summary>
        /// Process all the items under list for initialization
        /// </summary>
        /// <param name="list"></param>
        public virtual async System.Threading.Tasks.Task ProcessListAsync(ArchiverNodeItem list)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if ((await discoverWorker.ProcessContainerAsync(list, ProcessType.NeedProcess)) == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                    AveDiscoverFolder rootFolder = null;
                    try
                    {
                        mLog.Info("List Begin SPQuery to filter data. Path:{0}.", list.FullPath);
                        InitForSPQueryDiscover(list.SPList);
                        InitArchiverSPQueryRootFolder(list.SPList.RootFolder.ServerRelativeUrl);
                        if (SPORootFolder != null && SPORootFolder.SubFolders != null && SPORootFolder.SubFolders.Count > 0)
                        {
                            InitArchiverSPQueryFolderStructure(list.SPList.RootFolder.ServerRelativeUrl);
                        }
                        rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolderForArchiverSPQuery(SPORootFolder);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Can not use SPQuery to filter data and change query to Full Scan. Path:{0}. Message:{1}.", list.FullPath, ex.ToString());
                        ReleaseForSPQueryDiscover();
                        rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
                        //DB Scan如果SP Query 跪了，则直接抛异常，不走Full Discover
                        throw;
                    }
                    ArchiverNodeItem foldernode = list.GenerateFolderNodeItem(rootFolder, NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
                    await ProcessFolderAsync(foldernode);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("List UserName Or Password Incorrect. Path:{0}. Message:{1}.", list.FullPath, IUPEx.ToString());
                    throw;
                }

                catch (SPObjectReadOnlyException sroe)
                {
                    mLog.Info("List is ReadOnly. Path:{0}. Message:{1}.", list.FullPath, sroe.ToString());

                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("List is Locked. Path:{0}. Message:{1}.", list.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("List Not Found. Path:{0}. Message:{1}.", list.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing list node.Path:{0}.Message:{1}.", list.FullPath, e.ToString());
                    mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, e.Message);
                    mConfiguration.JobReportDto.HasErrorNode = true;
                    throw;
                }
                finally
                {
                    mConfiguration.ProgressDto.UpdateProgress();
                }
            }
        }
        private void ReleaseForSPQueryDiscover()
        {
            try
            {
                mConfiguration.mUseQueryDiscover = false;
                mSPQueryList = null;
                mCAMLManager = null;
                mMaxItemIdInLibrary = 0;
            }
            catch (Exception e)
            {
                mLog.Error($"error occured when ReleaseForSPQueryDiscover,error:{e}");
            }
        }
        private int GetLastItemId(IAveList list, string folderUrl)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(list, folderUrl, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(list, folderUrl, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }
        private void InitForSPQueryDiscover(IAveList list)
        {
            mConfiguration.mUseQueryDiscover = true;
            mSPQueryList = list;
            CamlScan cs = new CamlScan();
            //mCAMLManager = cs.InitCamlQuery(list, list.Fields, mConfiguration.RuleItemCollection, DateTime.UtcNow, true);
            mCAMLManager = new CAMLManager();
            mMaxItemIdInLibrary = GetLastItemId(list, list.RootFolder.ServerRelativeUrl);
            mLog.Info($"Using spquery for list:{list.Title} Max item id:{mMaxItemIdInLibrary}");
        }
        private void InitArchiverSPQueryFolderStructure(string rootFolderServerRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryFolderStructure"))
            {
                int startIndex = 0;
                int endIndex = 0;
                int totaltemsCount = 0;
                int rowLimit = 2000;
                try
                {
                    if (mMaxItemIdInLibrary > 0)
                    {
                        AveCamlQuery query = new AveCamlQuery();
                        mCAMLManager.ScopeType = Types.ScopeTypes.RecursiveAll;
                        mCAMLManager.RowLimit = rowLimit;
                        query.DatesInUtc = true;
                        query.FolderServerRelativeUrl = rootFolderServerRelativeUrl;
                        int executeCount = 0;
                        mLog.Info($"Start to query InitArchiverSPQueryFolderStructure in :{rootFolderServerRelativeUrl}.");
                        List<SPFolderReducedInfo> AllFolderReducedInfos = new List<SPFolderReducedInfo>();
                        AllFolderReducedInfos.Add(new SPFolderReducedInfo() { ServerRelativeUrl = rootFolderServerRelativeUrl, ID = 0 });
                        do
                        {
                            endIndex = startIndex + rowLimit > mMaxItemIdInLibrary ? mMaxItemIdInLibrary : startIndex + rowLimit;
                            mCAMLManager.QueryGroup.Groups.Clear();
                            mCAMLManager.QueryGroup.Conditions.Clear();
                            mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIndex.ToString()));
                            mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, endIndex.ToString()));
                            mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, "FSObjType", Types.FieldTypes.Integer, Types.QueryTypes.Eq, ((int)AveFileSystemObjectType.Folder).ToString()));
                            string queryXml = mCAMLManager.GetFullCAML();
                            query.ViewXml = queryXml;
                            mLog.Info("InitArchiverSPQueryFolderStructure xml {0}:{1}.", rootFolderServerRelativeUrl, queryXml);
                            IAveListItemCollection items = mSPQueryList.GetItems(query);
                            executeCount++;
                            totaltemsCount = totaltemsCount + items.Count;
                            mLog.Info("InitArchiverSPQueryFolderStructure {0}, query execute count:{1}. folder items count:{2}.", rootFolderServerRelativeUrl, executeCount, items.Count);
                            var folderItems = items.Where(x => x.FileSystemObjectType == AveFileSystemObjectType.Folder).ToList();
                            var partialReducedInfos = GetFolderReducedInfos(folderItems);
                            //AnalyzeFolderStructureV3(items, SPORootFolder);
                            AllFolderReducedInfos.AddRange(partialReducedInfos);
                            items = null;
                            mLog.Info("InitArchiverSPQueryFolderStructure ProcessDataWithSPQuery finished:{0}.execute count:{1}.", rootFolderServerRelativeUrl, executeCount);
                            if (startIndex + rowLimit < mMaxItemIdInLibrary)
                            {
                                startIndex = startIndex + rowLimit;
                            }
                            else if (startIndex + rowLimit > mMaxItemIdInLibrary && endIndex < mMaxItemIdInLibrary)
                            {
                                startIndex = mMaxItemIdInLibrary - endIndex;
                            }
                            else
                            {
                                break;
                            }
                        }
                        while (true);

                        AnalyzeFolderStructureV3(AllFolderReducedInfos, SPORootFolder);
                        mLog.Info("InitArchiverSPQueryFolderStructure xml {0}:{1}, query execute count:{2} totaltemsCount:{3}.", rootFolderServerRelativeUrl, mCAMLManager.GetFullCAML(), executeCount, totaltemsCount);
                    }
                    else
                    {
                        mLog.Info($"No item in this library, folder url:{rootFolderServerRelativeUrl} max item id:{mMaxItemIdInLibrary}.");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfoldersV2.Path:{0}.Message:{1}.", rootFolderServerRelativeUrl, ex.ToString());
                    throw;
                }
            }
        }
        void AnalyzeFolderStructureV3(List<SPFolderReducedInfo> folderItems, SPOFolder rootFolder)
        {
            var realRootFolder = folderItems.FirstOrDefault(x => string.Equals(x.ServerRelativeUrl.ToString(), rootFolder.Name, StringComparison.OrdinalIgnoreCase));
            if (realRootFolder != null)
            {
                rootFolder.Id = realRootFolder.ID;
            }
            else
            {
                mLog.Error($"Cannot find root folder id by url {rootFolder.Name}");
                throw new Exception($"Cannot find root folder id by url {rootFolder.Name}");
            }
            List<int> results = new List<int>();
            if (rootFolder.SubFolders != null)
            {
                foreach (SPOFolder folder in rootFolder.SubFolders)
                {
                    var failedItemsId = AssignFolderId(folder, rootFolder.Name, folderItems);

                    if (failedItemsId == null)
                    {
                        mLog.Info($"Folder:{folder?.Name} items id is null");
                        continue;
                    }

                    List<int> ids = new List<int>();

                    foreach (int id in failedItemsId)
                    {
                        ids.Add(id);
                        if (ids.Count() > 1000)
                        {
                            RealAnalyzeFolderStructureV3(ids);
                            ids = new List<int>();
                        }
                    }

                    if (ids.Any())
                    {
                        RealAnalyzeFolderStructureV3(ids);
                    }
                }
            }
        }

        public void RealAnalyzeFolderStructureV3(List<int> ids)
        {
            List<RMDiscoveryFileDataInfo> fileInfos = itemManager.SelectValuesFromDBByItemIds(ids.ToArray());
            foreach (var i in ids)
            {
                try
                {
                    var info = fileInfos.Where(f => f.ItemId.Equals(i)).FirstOrDefault();
                    if (info != null)
                    {
                        //mConfiguration.ProgressDto.HasErrorNode = true;
                        string siteIDString = mConfiguration.SiteCollectionID.ToString();
                        string siteUrlString = mConfiguration.SiteCollectionUrl;
                        mLog.Error($"Cannot find ID:{info.ItemId}.Name:{info.FullUrl} when AnalyzeFolderStructureV3.");
                        mConfiguration.JobReportDto.AddScanReport(info.FullUrl, 0, (int)CacheNodeType.Item, "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, "StorageOptimization_SOARRecordManagerFileNotExist");
                        //CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).UpdateStatus(siteIDString, info.ItemId, BackupRestoreStatus.Failed);
                        //mConfiguration.JobReportDto.AddReport(info.url, 0, BackupRestoreStatus.Failed, (int)CacheNodeType.Item, mConfiguration.JobId, "", "", "StorageOptimization_SOARCGDBWrongFilePathError");
                    }
                }
                catch (Exception e)
                {
                    mLog.Error($"Cannot add to report id {i} error {e}");
                }
            }
        }


        private IEnumerable<int> AssignFolderId(SPOFolder folder, string parentFolderServerRelativePath, List<SPFolderReducedInfo> realFolders)
        {
            var currentFolderServerRelativePath = parentFolderServerRelativePath + "/" + folder.Name;
            var realCurrentFolder = realFolders.FirstOrDefault(x => string.Equals(x.ServerRelativeUrl, currentFolderServerRelativePath, StringComparison.OrdinalIgnoreCase));
            if (realCurrentFolder != null)
            {
                folder.Id = realCurrentFolder.ID;
            }
            else
            {
                IEnumerable<int> ids = null;
                //log can't find the folder from SP
                mLog.Error($"Cannot find folder id by url {currentFolderServerRelativePath}");
                try
                {
                    ids = GetItemRowIdUnderFolder(folder);
                }
                catch (Exception e)
                {
                    mLog.Warn($"An error occurred while GetItemRowIdUnderFolder {e.ToString()}");
                }
                if (ids != null)
                {
                    foreach (int id in ids)
                    {
                        yield return id;
                    }
                }
            }

            if (folder.SubFolders != null)
            {
                foreach (var subfolder in folder.SubFolders)
                {
                    var ids = AssignFolderId(subfolder, currentFolderServerRelativePath, realFolders);
                    foreach (int id in ids)
                    {
                        yield return id;
                    }
                }
            }
        }
        private IEnumerable<int> GetItemRowIdUnderFolder(SPOFolder folder)
        {
            if (folder != null)
            {
                if (folder.Items.Count != 0)
                {
                    foreach (var itemId in folder.Items.Select(item => item.Id))
                    {
                        yield return itemId;
                    }
                }
                if (folder.SubFolders != null)
                {
                    foreach (var subFolder in folder.SubFolders)
                    {
                        var idsUnderFolder = GetItemRowIdUnderFolder(subFolder);
                        foreach (int id in idsUnderFolder)
                        {
                            yield return id;
                        }
                    }
                }
            }
        }
        private List<SPFolderReducedInfo> GetFolderReducedInfos(List<IAveListItem> folders)
        {
            List<SPFolderReducedInfo> foldersReducedInfos = new List<SPFolderReducedInfo>();
            foreach (var folder in folders)
            {
                SPFolderReducedInfo info = new SPFolderReducedInfo();
                info.ID = folder.ID;
                info.ServerRelativeUrl = folder.FieldValues["FileRef"].ToString();
                foldersReducedInfos.Add(info);
                mLog.Info($"GetFolderReducedInfos. Folder Id:{info.ID}.Folder ServerRelativeUrl:{info.ServerRelativeUrl}.");
            }
            return foldersReducedInfos;
        }
        private int InnerGetLastItemId(IAveList list, string folderUrl, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folderUrl;
            query.ViewXml = queryXml;
            var itemCollection = list.GetItems(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }
        private void InitArchiverSPQueryRootFolder(string rootFolderServerRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryRootFolder"))
            {
                long fileInfoCount = itemManager.GetCoutOfData();
                SPORootFolder?.Dispose();
                SPORootFolder = SPOFolder.BuildRootFolder(new CacheDBOperator<SPOItem>(), new CacheDBOperator<SPOFolder>(), rootFolderServerRelativeUrl);
                try
                {
                    if (mMaxItemIdInLibrary > 0)
                    {
                        mLog.Info($"Start to query InitArchiverSPQueryRootFolder in :{rootFolderServerRelativeUrl}.");
                        //totaltemsCount = totaltemsCount + items.Count;
                        mLog.Info("InitArchiverSPQueryRootFolder.");
                        AnalyzeListItems(SPORootFolder);
                        mLog.Info("InitArchiverSPQueryRootFolder finished.");
                    }
                    else
                    {
                        mLog.Info($"No item in this library, folder url:{rootFolderServerRelativeUrl} max item id:{mMaxItemIdInLibrary}.");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while InitArchiverSPQueryRootFolder.Path:{0}.Message:{1}.", rootFolderServerRelativeUrl, ex.ToString());
                    throw;
                }
            }
        }
        void AnalyzeListItems(SPOFolder rootFolder)
        {
            int page = 0;
            int size = 1000;
            List<RMDiscoveryFileDataInfo> items;
            do
            {
                items = itemManager.PageSelectValuesFromDB(page++, size);
                foreach (var item in items)
                {
                    int index = item.FullUrl.LastIndexOf('/');
                    if (index <= 0)
                    { continue; }
                    //mLog.Info($"AnalyzeListItems. DBFileInfo,item full url:{item.FullUrl},webappName:{WebAppName},  msiteUrl:{mSiteUrl}");
                    var serverRelativeUrl = item.FullUrl.Substring(WebAppName.Length);
                    var name = item.FullUrl.Substring(index + 1);
                    mLog.Info($"AnalyzeListItems. DBFileInfo Id:{item.Id}.itemId:{item.ItemId}.listId:{item.ListId}.webId:{item.WebId}.ItemParentPath:{item.FullUrl.Substring(0, index)}.");
                    var parentFolder = rootFolder;
                    //mLog.Warn($"AnalyzeListItems. DBFileInfo serverRelativeUrl:{serverRelativeUrl} lenth:{serverRelativeUrl.Length},rootFolder.Name:{rootFolder.Name} lenth:{rootFolder.Name.Length}.");
                    var frUrl = serverRelativeUrl.Substring(rootFolder.Name.Length, serverRelativeUrl.Length - rootFolder.Name.Length - name.Length - 1);
                    var parentFoldersName = frUrl.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < parentFoldersName.Length; i++)
                    {
                        var folderName = parentFoldersName[i];
                        SPOFolder tempFolder = null;
                        tempFolder = parentFolder.SubFolders.GetByName(folderName);

                        if (tempFolder == null)
                        {
                            tempFolder = SPOFolder.BuildUnRootFolder(parentFolder, folderName, -1);
                            parentFolder.SubFolders.Add(tempFolder);
                        }
                        parentFolder = tempFolder;
                    }

                    var id = item.ItemId;
                    if (item.FileSize > 0)
                    {
                        var spoItem = new SPOItem()
                        {
                            Id = id,
                            Name = name
                        };
                        parentFolder.Items.Add(spoItem);
                    }
                    else
                    {
                        mLog.Info($"The Object {item.FullUrl} size less 0 and update CGDBStatus .");
                        string siteIDString = mConfiguration.SiteCollectionID.ToString();
                        string siteUrlString = mConfiguration.SiteCollectionUrl;
                        //CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).UpdateStatus(siteIDString, item.itemId, BackupRestoreStatus.Skipped);
                    }
                }
            } while (items.Count() == size);
        }

        private string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";

            return result;
        }
        private string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";
            return result;
        }
        /// <summary>
        /// Process folder for initialization
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="needInitInfo"></param>
        public async virtual System.Threading.Tasks.Task ProcessFolderAsync(ArchiverNodeItem folder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessFolder"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(folder, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//add for RevIM RECO-84
                    {
                        return;
                    }
                    await ProcessItemsAndSubfoldersAsync(folder, folder.Cache_NodeType);

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

        public async virtual System.Threading.Tasks.Task ProcessItemAsync(ArchiverNodeItem nodeItem, IDiscoverNodeWorker discoverWorker, bool needInitInfo = false)
        {
            this.discoverWorker = discoverWorker;
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessItem"))
            {
                ArchiverNodeItem folderItem = null;
                int tempIdx = nodeItem.FullPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                // if method up can not get IAveDiscoverFolder  ,we can new  an  NodeItem as Folder 
                folderItem = new ArchiverNodeItem()
                {
                    WebApplicationId = nodeItem.WebApplicationId,
                    WebApplicationUrl = nodeItem.WebApplicationUrl,
                    SiteId = nodeItem.SiteId,
                    WebId = nodeItem.WebId,
                    ListId = nodeItem.ListId,
                    SiteUrl = nodeItem.SiteUrl,
                    FullPath = tempIdx != -1 ? nodeItem.FullPath.Substring(0, tempIdx) : nodeItem.FullPath,
                    ID = nodeItem.FolderId,
                    Cache_NodeType = (int)CacheNodeType.Folder,
                    SPNodeLevel = nodeItem.IsRootFolder ? NodeLevel.RootFolder : NodeLevel.Folder
                };
                if (needInitInfo)
                {
                    await InitialSPObjectInfoAsync(discoverWorker, folderItem);
                }
                AveDiscoverItem item = null;
                AveDiscoverFolder folder = (AveDiscoverFolder)folderItem.DiscoverSPObject;
                folderItem.Name = folder.ItemName;
                folderItem.FullPath = folder.FullUrl;
                ProcessResult result = await discoverWorker.ProcessContainerAsync(folderItem, ProcessType.NeedProcess);
                //item = ((IAveDiscoverFolder)folderItem.DiscoverSPObject).GetItemById(nodeItem.ItemId);
                List<AveDiscoverItem> discoverItems = null;
                int retryTime = 0;
                while (retryTime < 10)
                {
                    try
                    {
                        discoverItems = ((AveDiscoverFolder)folderItem.DiscoverSPObject).GetItems();
                        mLog.Info("GetItems success in ProcessItem");
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryTime++;
                        discoverItems = new List<AveDiscoverItem>();
                        mLog.Warn("GetItems Failed in ProcessItem and retry.RetryTime:{0}.Message:{1}.", retryTime, ex.ToString());
                        await System.Threading.Tasks.Task.Delay(5 * 1000);
                    }
                }
                if (discoverItems != null)
                {
                    foreach (AveDiscoverItem aveDiscoverItem in discoverItems)
                    {
                        if (aveDiscoverItem.DocID == nodeItem.ItemId)
                        {
                            item = aveDiscoverItem;
                            await ProcessVersionAndAttachmentsAsync(item, (AveDiscoverFolder)folderItem.DiscoverSPObject, folderItem, discoverWorker);
                            break;
                        }
                    }
                }
                if (item == null)
                {
                    throw new Exception("The Item In This Library Or List Do Not Found");
                }
            }
        }

        public async virtual System.Threading.Tasks.Task ProcessItemsAndSubfoldersAsync(ArchiverNodeItem folderNode, int folderLevel)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RealProcessItemsAndSubfolders"))
            {
                AveDiscoverFolder rootFolder = (folderNode.DiscoverSPObject as AveDiscoverFolder);
                #region process items/documents
                try
                {
                    if (TryGetManifestEntriesForList(folderNode.ListId, out var manifestEntries))
                    {
                        mLog.Info("Processing manifest entries with folder-first order for list {0}.", folderNode.ListId);
                        var manifestEntriesByFolderPath = BuildManifestEntriesByFolderPath(manifestEntries, folderNode);
                        var processedManifestEntries = new HashSet<ManifestDiscoveredItem>();
                        await ProcessManifestFolderAsync(rootFolder, folderNode, manifestEntries, manifestEntriesByFolderPath, processedManifestEntries, discoverWorker);

                        var remainingManifestEntries = manifestEntries.Count - processedManifestEntries.Count;
                        if (remainingManifestEntries > 0)
                        {
                            mLog.Warn("Manifest contains {0} entries whose folders were not found in SharePoint.", remainingManifestEntries);
                        }
                    }
                    else
                    {
                        await ProcessDataAsync(rootFolder, folderNode, discoverWorker);
                        await ProcessFolderHierarchyAsync(rootFolder, folderNode, discoverWorker);
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

                // Manifest entries are grouped by folder to preserve hierarchy for restore; SharePoint folder traversal now walks folders to keep backup completeness.
                if (rootFolder != null)
                {
                    rootFolder.Dispose();
                }
            }
        }

        public virtual async System.Threading.Tasks.Task InitialSPObjectInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitialSPObjectInfo"))
            {

                mDiscoverSite = InitDiscoverSite(node); //tmpDiscoverSite;
                //初始化Site对象的一些信息。  
                Uri uri = new Uri(node.SiteUrl);
                mConfiguration.mInitialTime = DateTime.Now;
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
                        //ProcessResult result = await discoverWork.ProcessDiscoverOptimizationContainerAsync(node.GenerateWebappNodeItem(), ProcessType.NeedProcess);
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
                                node.Cache_NodeType = ((int)CacheNodeType.Item) / 2;
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

        internal async System.Threading.Tasks.Task ProcessDataAsync(AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessData"))
            {
                mLog.Error("Manifest entries missing for list {0} ({1}). Skipping SharePoint fallback by configuration.", folderNode.ListId, folderNode.FullPath);
            }
        }

        private async System.Threading.Tasks.Task ProcessManifestFolderAsync(
            AveDiscoverFolder currentFolder,
            ArchiverNodeItem currentFolderNode,
            List<ManifestDiscoveredItem> manifestEntries,
            IReadOnlyDictionary<string, List<ManifestDiscoveredItem>> manifestEntriesByFolderPath,
            ISet<ManifestDiscoveredItem> processedManifestEntries,
            IDiscoverNodeWorker discoverWorker)
        {
            if (currentFolder == null
                || currentFolderNode == null
                || manifestEntries == null
                || manifestEntries.Count == 0
                || manifestEntriesByFolderPath == null
                || processedManifestEntries == null)
            {
                return;
            }

            var list = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
            var folderPath = currentFolder.FullUrl;
            var normalizedCurrentPath = NormalizeFolderPathForList(folderPath, list).TrimEnd('/');
            IEnumerable<ManifestDiscoveredItem> entriesInCurrent = manifestEntriesByFolderPath.TryGetValue(normalizedCurrentPath, out var matchedEntries)
                ? matchedEntries
                : Array.Empty<ManifestDiscoveredItem>();

            var processResult = await discoverWorker.ProcessContainerAsync(currentFolderNode, ProcessType.NeedProcess);
            if (processResult != ProcessResult.SkipCurrentNode)
            {
                foreach (var entry in entriesInCurrent)
                {
                    if (!processedManifestEntries.Add(entry))
                    {
                        continue;
                    }

                    using (new CheckJobStopScope()) { }
                    try
                    {
                        await ProcessManifestEntryAsync(entry, currentFolderNode, discoverWorker);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Error("Error while processing manifest entry FullUrl:{0}.Message:{1}.", entry.Metadata?.FullUrl, ex.ToString());
                    }
                }
            }

            var subFolders = currentFolder.GetSubFolders();
            if (subFolders == null)
            {
                return;
            }

            foreach (var subFolder in subFolders)
            {
                using (new CheckJobStopScope()) { }
                using (ArchiverNodeItem subFolderNode = currentFolderNode.GenerateFolderNodeItem(subFolder, NodeLevel.Folder, mDiscoverSite?.Site?.Url ?? currentFolderNode.SiteUrl, mConfiguration))
                {
                    await ProcessManifestFolderAsync(subFolder, subFolderNode, manifestEntries, manifestEntriesByFolderPath, processedManifestEntries, discoverWorker);
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessFolderHierarchyAsync(AveDiscoverFolder currentFolder, ArchiverNodeItem currentFolderNode, IDiscoverNodeWorker discoverWorker)
        {
            if (currentFolder == null || currentFolderNode == null)
            {
                return;
            }

            var subFolders = currentFolder.GetSubFolders();
            if (subFolders == null)
            {
                return;
            }

            foreach (var subFolder in subFolders)
            {
                using (new CheckJobStopScope()) { }
                using (ArchiverNodeItem subFolderNode = currentFolderNode.GenerateFolderNodeItem(subFolder, NodeLevel.Folder, mDiscoverSite?.Site?.Url ?? currentFolderNode.SiteUrl, mConfiguration))
                {
                    var result = await discoverWorker.ProcessContainerAsync(subFolderNode, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)
                    {
                        continue;
                    }

                    await ProcessFolderHierarchyAsync(subFolder, subFolderNode, discoverWorker);
                }
            }
        }

        internal async virtual System.Threading.Tasks.Task ProcessVersionAndAttachmentsAsync(AveDiscoverItem item, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersionAndAttachments"))
            {
                var manifestEntry = ResolveManifestEntry(item);
                using (ArchiverNodeItem itemNode = folderNode.GenerateItemNodeItem(item, rootFolder, mConfiguration))
                {
                    ApplyManifestMetadata(itemNode, manifestEntry?.Metadata);

                    var fileRecord = itemManager.SelectValuesFromDBByItemUniqueIds(itemNode.ID.ToString()).FirstOrDefault();

                    if (fileRecord != null && fileRecord.ModifiedTime.Ticks + TimeSpan.FromMinutes(1).Ticks < itemNode.Modified)
                    {
                        mLog.Warn($"this file:{fileRecord.FullUrl} has beed modified,item id:{itemNode.ID},fileRecord:{fileRecord.ModifiedTime.Ticks},itemModified{itemNode.Modified}");
                        return;
                    }
                    ProcessResult result = await discoverWorker.ProcessItemAsync(itemNode, folderNode);
                    if (result == ProcessResult.CurrentVersionHasApprove || result == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                    Stopwatch watch = Stopwatch.StartNew();

                    var manifestVersions = GetManifestVersions(manifestEntry);
                    IReadOnlyList<AveVersionObject> versions = manifestVersions.Count > 0 ? manifestVersions : item.GetVersions();

                    if (versions.Count > 1)
                    {
                        foreach (AveVersionObject version in versions)
                        {
                            if (ShouldSkipVersion(item, version))
                            {
                                continue;
                            }
                            try
                            {
                                await ProcessVersionsAsync(itemNode, version, folderNode, discoverWorker);
                            }
                            catch (Exception ex)
                            {
                                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanProcessItemVersionsError + ex.ToString());
                            }
                        }
                    }
                    watch.Stop();
                    mLog.Info("ProcessVersionAndAttachments GetAttachments GetVersions costs: {0}.", watch.Elapsed);
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessManifestDataAsync(List<ManifestDiscoveredItem> manifestEntries, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            var manifestEntriesByFolderPath = BuildManifestEntriesByFolderPath(manifestEntries, folderNode);
            await ProcessManifestFolderAsync(
                folderNode?.DiscoverSPObject as AveDiscoverFolder,
                folderNode,
                manifestEntries,
                manifestEntriesByFolderPath,
                new HashSet<ManifestDiscoveredItem>(),
                discoverWorker);
        }

        private async System.Threading.Tasks.Task ProcessManifestEntryAsync(ManifestDiscoveredItem entry, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            if (entry?.Metadata == null)
            {
                return;
            }

            using (ArchiverNodeItem itemNode = BuildManifestItemNode(entry, folderNode))
            {
                var fileRecord = itemManager.SelectValuesFromDBByItemUniqueIds(itemNode.ID.ToString()).FirstOrDefault();
                if (fileRecord != null && fileRecord.ModifiedTime.Ticks + TimeSpan.FromMinutes(1).Ticks < itemNode.Modified)
                {
                    mLog.Warn($"this file:{fileRecord.FullUrl} has beed modified,item id:{itemNode.ID},fileRecord:{fileRecord.ModifiedTime.Ticks},itemModified{itemNode.Modified}");
                    return;
                }
                var versions = GetManifestVersions(entry);
                var includeCurrentVersions = GetManifestVersionsForSize(entry);
                itemNode.DocumentSize = GetFileSize(includeCurrentVersions, entry, itemNode);
                var result = await discoverWorker.ProcessItemAsync(itemNode, folderNode);
                if (result == ProcessResult.CurrentVersionHasApprove || result == ProcessResult.SkipCurrentNode)
                {
                    return;
                }

                if (versions.Count > 1)
                {
                    foreach (var version in versions)
                    {
                        if (version == null || version.Uiversion == 0)
                        {
                            continue;
                        }

                        if (version.IsCurrentVersion || (entry.Metadata != null && !string.IsNullOrWhiteSpace(entry.Metadata.CurrentVersion) && string.Equals(version.UiVersionString, entry.Metadata.CurrentVersion, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }
                        try
                        {
                            using (ArchiverNodeItem versionNode = BuildManifestVersionNode(itemNode, version, entry))
                            {
                                await discoverWorker.ProcessItemAsync(versionNode, itemNode);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanProcessItemVersionsError + ex.ToString());
                        }
                    }
                }
            }
        }
        private long GetFileSize(List<AveVersionObject> versionObj, ManifestDiscoveredItem entry, ArchiverNodeItem itemNode)
        {
            long totalSize = 0;
            if (versionObj.Count > 0)
            {
                foreach (var version in versionObj)
                {
                    try
                    {
                        totalSize += BuildManifestVersionSize(itemNode, version, entry);
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanProcessItemVersionsError + ex.ToString());
                    }
                }
            }
            return totalSize;
        }
        internal async virtual System.Threading.Tasks.Task ProcessVersionsAsync(ArchiverNodeItem item, AveVersionObject version, ArchiverNodeItem folder, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersions"))
            {
                ArchiverNodeItem versionNode = item.GenerateItemVersionNodeItem(version, item, mConfiguration);
                var result = await discoverWorker.ProcessItemAsync(versionNode, item);
            }
        }

        internal async virtual System.Threading.Tasks.Task ProcessAttachmentsAsync(ArchiverNodeItem folderNode, ArchiverNodeItem item, AveItemObject attachment, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessAttachments"))
            {
                ProcessResult result = ProcessResult.Default;
                try
                {
                    using (new CheckJobStopScope()) { }
                    ArchiverNodeItem attachmentNode = null;
                    switch (item.ItemType)
                    {
                        case ArchiverCommon.ItemType.ITEM_TYPE:
                            attachmentNode = item.GenerateAttachmentNodeItem(attachment, (AveDiscoverFolder)folderNode.DiscoverSPObject);
                            result = await discoverWorker.ProcessItemAsync(attachmentNode, item);
                            break;
                        default:
                            attachmentNode = item.GenerateAttachmentNodeFolder(attachment, (AveDiscoverFolder)item.DiscoverSPObject);
                            result = await discoverWorker.ProcessItemAsync(attachmentNode, item);
                            break;
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while processing attachments.Path:{0}.Message:{1}.", item.FullPath, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Convert tree node to RuleNodeContract.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RuleNodeContract ConvertTreeNodeToRuleNodeConfig(SPTreeNodeDto node, RuleNodeType type, RMDiscoverOptimizationNode discoverNode)
        {
            if (node == null)
            {
                return null;
            }
            RuleNodeContract result = new RuleNodeContract();
            result.Id = Guid.NewGuid().ToString();
            result.NodeId = discoverNode.SiteId.ToString();
            //result.NodeName = node.;
            result.DisplayName = node.DisplayName;
            result.ManagerTreeId = node.ID;
            result.FullPath = discoverNode.SiteUrl;
            //result.FarmId = node.FarmID;
            //result.SPType = node.SPType;
            //if (node.NodeExtension != null && node.NodeExtension.BposInfo != null)
            //{
            //    result.BposInfo = node.NodeExtension.BposInfo;
            //}
            //if (node.Parent != null)  //Farm 级别没有Parent
            //{
            //    if (node.Parent.Level == NodeLevel.Sites || node.Parent.Level == NodeLevel.Lists || node.Parent.Level == NodeLevel.Folders)
            //    {
            //        result.ParentNodeId = node.Parent.Parent == null ? null : node.Parent.Parent.SPObjectId;
            //        result.ParentNodeName = node.Parent.Parent == null ? null : node.Parent.Parent.Name;
            //    }
            //    else
            //    {
            //        result.ParentNodeId = node.Parent.SPObjectId;
            //        result.ParentNodeName = node.Parent.Name;
            //    }
            //}
            result.NodeLevel = NodeLevel.SiteCollection;
            //result.SPVersion = node.SPVersion;
            result.Type = type;
            AssignSPObjectId(node, ref result);
            //在处理index的时候需要转换children
            result.BreakInheritNodesEncryptBySha1 = new Dictionary<string, RuleNodeContract>();
            //var breakInheritUrl = LoadBreakInheritNodeUrls(node.FullPath);
            //foreach (var b in breakInheritUrl)
            //{
            //    var sh1 = ArchiverCommonStaticMethod.GetBreakInheritSHA1String(b);
            //    result.BreakInheritNodesEncryptBySha1[sh1] = null;
            //}
            return result;
        }
        private RuleNodeContract ConvertHSMTreeNodeToRuleNodeConfig(SPTreeNodeDto node, RuleNodeType type)
        {
            if (node == null)
            {
                return null;
            }
            RuleNodeContract result = new RuleNodeContract();
            result.Id = Guid.NewGuid().ToString();
            result.NodeId = node.SPObjectId.ToString();
            //result.NodeName = node.;
            result.DisplayName = node.DisplayName;
            result.ManagerTreeId = node.ID;
            result.FullPath = node.FullPath;
            result.NodeLevel = NodeLevel.SiteCollection;
            //result.SPVersion = node.SPVersion;
            result.Type = type;
            AssignSPObjectId(node, ref result);
            //在处理index的时候需要转换children
            result.BreakInheritNodesEncryptBySha1 = new Dictionary<string, RuleNodeContract>();
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
                    try
                    {
                        long storageMaximumLevel = Site.Quota.StorageMaximumLevel * 1024L * 1024L;
                        mLog.Info($"Current Site:{Site.Url} StorageMaximumLevel is:{Site.Quota.StorageMaximumLevel}.Storage is:{Site.Usage.Storage}.ByteStorageMaximumLevel:{storageMaximumLevel}.");
                        if (Site.Quota.StorageMaximumLevel == 0)
                        {
                            //special env,special site does not permission to get this value, so skip this check when size is 0.
                            mLog.Info($"CheckAveExceedStorageLimit.Current Site:{Site.Url} StorageMaximumLevel is 0, skip check current site storage limit.");
                        }
                        else if (Site.Usage.Storage >= storageMaximumLevel)
                        {
                            mConfiguration.JobReportDto.summaryComments = "RM_JM_SiteStorageLimit_ErrorMessage";
                            throw new AveExceedStorageLimitException("This site has exceeded its maximum file storage limit.");
                        }
                    }
                    catch (AveExceedStorageLimitException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"Get Site Storage StorageMaximumLevel Error.error:{e}");
                    }
                }
                catch (AveExceedStorageLimitException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    var we = e.InnerException as WebException;
                    if (we != null)
                    {
                        if (we.Status == WebExceptionStatus.ProtocolError)
                        {
                            var httpResp = (we.Response as HttpWebResponse);
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
                    throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessSiteSPObjectNotFoundException, "SiteCollection", node.FullPath); ;
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
                AveDiscoverSite tmpDiscoverSite = new AveDiscoverSite(Site, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);

                return tmpDiscoverSite;
            }
        }

        private bool GetEnableRemoveReadOnlyState()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableRemoveReadOnlyState");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        

        internal async System.Threading.Tasks.Task ProcessListCollectionAsync(ArchiverNodeItem web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessListCollection"))
            {
                Dictionary<Guid, AveDiscoverList> discoveryLists;
                discoveryLists = (web.DiscoverSPObject as AveDiscoverWeb).GetLists();
                foreach (AveDiscoverList list in discoveryLists.Values)
                {
                    using (new CheckJobStopScope()) { }
                    if (CheckIsDesignList(list.Name + list.ListTemplate.ToString()))
                    {
                        mLog.Info($"ProcessListCollectionAsync:Skip the design list.ListTitle:{list.Title}.ListURL:{list.RootFolderUrl}.");
                        continue;
                    }
                    if (CheckIsDesignList(CombineListUrlAndTemplate(list)))
                    {
                        mLog.Info($"ProcessListCollectionAsync:Skip the design list by URL and Template.ListTitle:{list.Title}.ListURL:{list.RootFolderUrl}.");
                        continue;
                    }
                    if (list.Hidden.HasValue && list.Hidden.Value)
                    {
                        mLog.Info($"ProcessListCollectionAsync:discover optimization current list is Hidden.ListURL:{list.RootFolderUrl}.Title:{list.Title}.");
                        continue;
                    }
                    if (list.Title.Equals("{System Folder}"))
                    {
                        mLog.Info("Current list is System Folder when discover list collection, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                        continue;
                    }
                    try
                    {
                        using (itemManager = HSMItemManager.GetInstance())
                        {
                            IAveWeb tmpWeb = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as IAveWeb;
                            IAveList tmpList = tmpWeb.GetList(list.RootFolderUrl);
                            mLog.Info("Current list [{0}] ItemCount [{1}].", list.RootFolderUrl, tmpList.ItemCount);
                            if (tmpList != null && tmpList.BaseType == AveBaseType.GenericList)
                            {
                                mLog.Info($"ProcessListCollectionAsync.Current list is general list so skip process.List Title: {tmpList.Title}.");
                                continue;
                            }
                            string siteIDString = mConfiguration.SiteCollectionID.ToString();
                            string siteUrlString = this.WebAppName + web.FullPath;
                            bool hasManifestEntries = false;
                            await foreach (var manifestEntries in LoadAzureManifestFileDataAsync(StorageConnectionString, RMConstants.ImportArchiveDataFolderName, jobSettings.TraceId, web.WebId, list.ListId, siteIDString, siteUrlString, ContentStorageConnectionString))
                            {
                                if (manifestEntries == null || manifestEntries.Count == 0)
                                {
                                    continue;
                                }

                                hasManifestEntries = true;
                                ProcessManifestEntries(itemManager, manifestEntries, tmpList);
                            }

                            if (!hasManifestEntries)
                            {
                                mLog.Info($"Local manifest does not contain files for webId={web.WebId}, listId={list.ListId}.");
                                continue;
                            }

                            itemManager.WaitInsertFinish();
                            long count = itemManager.GetCoutOfData();
                            mLog.Info($"discover optimization current list file count is:{count}");
                            if (count > 0)
                            {
                                mLog.Info("Begin discover list, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                                ArchiverNodeItem ListNode = web.GenerateListNodeItem(list, tmpList);
                                mDependencyObjs.PutIn(tmpList, (int)CacheNodeType.List, false);
                                using (ListNode)
                                {
                                    await ProcessListAsync(ListNode);
                                }
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
                    }
                    finally
                    {
                        CleanupManifestEntries(list.ListId);
                    }
                }
            }
        }

        private void ProcessManifestEntries(HSMItemManager itemDbManager, List<ManifestDiscoveredItem> manifestEntries, IAveList aveList)
        {
            if (manifestEntries == null || manifestEntries.Count == 0)
            {
                return;
            }

            foreach (var entry in manifestEntries)
            {
                if (!string.IsNullOrWhiteSpace(entry.Metadata.ItemUniqueId))
                {
                    manifestItemCache[entry.Metadata.ItemUniqueId] = entry;
                }
            }

            itemDbManager.InsertValue(manifestEntries.Select(e => e.Metadata).ToList());

            var listKey = manifestEntries[0].ListId;
            if (manifestEntriesByListId.TryGetValue(listKey, out var existingEntries) && existingEntries != null)
            {
                existingEntries.AddRange(manifestEntries);
            }
            else
            {
                manifestEntriesByListId[listKey] = new List<ManifestDiscoveredItem>(manifestEntries);
            }
        }

        private bool TryGetManifestEntry(string uniqueId, [NotNullWhen(true)] out ManifestDiscoveredItem entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                return false;
            }

            return manifestItemCache.TryGetValue(uniqueId, out entry);
        }

        private bool TryGetManifestEntriesForList(Guid listId, out List<ManifestDiscoveredItem> entries)
        {
            entries = null;
            if (listId == Guid.Empty)
            {
                return false;
            }

            if (!manifestEntriesByListId.TryGetValue(listId, out var manifestEntries) || manifestEntries == null || manifestEntries.Count == 0)
            {
                return false;
            }

            entries = manifestEntries;
            return true;
        }

        private void CleanupManifestEntries(Guid listId)
        {
            if (listId == Guid.Empty)
            {
                return;
            }

            if (manifestEntriesByListId.TryRemove(listId, out var entries) && entries != null)
            {
                foreach (var entry in entries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Metadata.ItemUniqueId))
                    {
                        manifestItemCache.TryRemove(entry.Metadata.ItemUniqueId, out _);
                    }
                }
            }
        }

#nullable enable

        private ManifestDiscoveredItem? ResolveManifestEntry(AveDiscoverItem item)
        {
            if (item == null)
            {
                return null;
            }

            if (TryGetManifestEntry(item.DocID.ToString("D"), out var entry))
            {
                return entry;
            }

            if (item.tp_GUID != Guid.Empty && TryGetManifestEntry(item.tp_GUID.ToString("D"), out entry))
            {
                return entry;
            }

            return null;
        }

        private static void ApplyManifestMetadata(ArchiverNodeItem node, RMDiscoveryFileDataInfo? metadata)
        {
            if (node == null || metadata == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(metadata.FullUrl))
            {
                node.FullPath = metadata.FullUrl;
            }

            node.DocumentSize = metadata.FileSize;
            node.Modified = metadata.ModifiedTime.Ticks;
            node.Created = metadata.CreatedTime.Ticks;
        }

        private static List<AveVersionObject> GetManifestVersions(ManifestDiscoveredItem? entry)
        {
            if (entry?.Metadata?.Versions == null || entry.Metadata.Versions.Count <= 1)
            {
                return new List<AveVersionObject>();
            }
            return GetAllVersions(entry);

        }
        private static List<AveVersionObject> GetManifestVersionsForSize(ManifestDiscoveredItem? entry)
        {
            if (entry?.Metadata?.Versions == null || entry.Metadata.Versions.Count <= 0)
            {
                return new List<AveVersionObject>();
            }
            return GetAllVersions(entry);

        }
        private static List<AveVersionObject> GetAllVersions(ManifestDiscoveredItem? entry)
        {
            var versions = new List<AveVersionObject>(entry.Metadata.Versions.Count);
            foreach (var manifestVersion in entry.Metadata.Versions)
            {
                var versionNumber = ConvertVersionLabelToUiversion(manifestVersion.Version);
                versions.Add(new AveVersionObject
                {
                    Uiversion = versionNumber,
                    UiVersionString = manifestVersion.Version ?? string.Empty,
                    TimeLastModified = manifestVersion.ModifiedTime,
                    Size = manifestVersion.VersionSize,
                    IsCurrentVersion = string.Equals(manifestVersion.Version, entry.Metadata.CurrentVersion, StringComparison.OrdinalIgnoreCase),
                    FileValue = manifestVersion.FileValue,
                });
            }

            return versions;
        }

        private static string NormalizeFolderPathForList(string folderPath, IAveList list)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return list.RootFolder.ServerRelativeUrl;
            }

            var path = folderPath;
            if (folderPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || folderPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var schemeIndex = folderPath.IndexOf("://", StringComparison.Ordinal);
                var pathStartIndex = schemeIndex >= 0 ? folderPath.IndexOf('/', schemeIndex + 3) : -1;
                path = pathStartIndex >= 0 ? folderPath.Substring(pathStartIndex) : "/";
            }

            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                var webPrefix = list.ParentWeb?.ServerRelativeUrl ?? string.Empty;
                path = string.IsNullOrWhiteSpace(webPrefix)
                    ? "/" + path.TrimStart('/')
                    : string.Format(CultureInfo.InvariantCulture, "{0}/{1}", webPrefix.TrimEnd('/'), path.TrimStart('/'));
            }

            path = Uri.UnescapeDataString(path);

            return path;
        }

        private IReadOnlyDictionary<string, List<ManifestDiscoveredItem>> BuildManifestEntriesByFolderPath(
            List<ManifestDiscoveredItem> manifestEntries,
            ArchiverNodeItem baseFolder)
        {
            var lookup = new Dictionary<string, List<ManifestDiscoveredItem>>(StringComparer.OrdinalIgnoreCase);
            if (manifestEntries == null || manifestEntries.Count == 0 || baseFolder == null)
            {
                return lookup;
            }

            var list = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
            var normalizedListRoot = list != null
                ? NormalizeFolderPathForList(list.RootFolder.ServerRelativeUrl, list).TrimEnd('/')
                : (baseFolder.FullPath ?? string.Empty).Replace("\\", "/", StringComparison.Ordinal).TrimEnd('/');

            foreach (var entry in manifestEntries)
            {
                var normalizedEntryPath = GetNormalizedManifestFolderPath(entry, baseFolder, list, normalizedListRoot);
                if (!lookup.TryGetValue(normalizedEntryPath, out var entriesInFolder))
                {
                    entriesInFolder = new List<ManifestDiscoveredItem>();
                    lookup[normalizedEntryPath] = entriesInFolder;
                }

                entriesInFolder.Add(entry);
            }

            return lookup;
        }

        private static string GetNormalizedManifestFolderPath(
            ManifestDiscoveredItem entry,
            ArchiverNodeItem baseFolder,
            IAveList? list,
            string normalizedListRoot)
        {
            var manifestFolderFullPath = ResolveManifestFolderFullPath(entry, baseFolder);
            var normalizedEntryPath = list != null
                ? NormalizeFolderPathForList(manifestFolderFullPath, list).TrimEnd('/')
                : manifestFolderFullPath?.Replace("\\", "/", StringComparison.Ordinal).TrimEnd('/') ?? string.Empty;

            return string.IsNullOrWhiteSpace(normalizedEntryPath)
                ? normalizedListRoot
                : normalizedEntryPath;
        }

        private static string ResolveManifestFolderFullPath(ManifestDiscoveredItem entry, ArchiverNodeItem baseFolder)
        {
            var manifestFullUrl = entry.Metadata?.FullUrl;
            if (!string.IsNullOrWhiteSpace(manifestFullUrl))
            {
                var lastSlashIndex = manifestFullUrl.LastIndexOf("/", StringComparison.Ordinal);
                return lastSlashIndex > 0 ? manifestFullUrl.Substring(0, lastSlashIndex) : manifestFullUrl;
            }

            var siteUrl = entry.Metadata?.SiteUrl ?? baseFolder.SiteUrl ?? string.Empty;
            var folderRelative = entry.Metadata?.FolderRelativeUrl;
            if (!string.IsNullOrWhiteSpace(folderRelative))
            {
                return string.IsNullOrWhiteSpace(siteUrl)
                    ? folderRelative.Replace("\\", "/", StringComparison.Ordinal)
                    : string.Format(CultureInfo.InvariantCulture, "{0}/{1}", siteUrl.TrimEnd('/'), folderRelative.TrimStart('/'));
            }

            var serverRelative = BuildServerRelativeUrl(entry);
            if (!string.IsNullOrWhiteSpace(serverRelative))
            {
                var folderRelativePath = Path.GetDirectoryName(serverRelative)?.Replace("\\", "/", StringComparison.Ordinal) ?? string.Empty;
                return string.IsNullOrWhiteSpace(siteUrl)
                    ? folderRelativePath
                    : string.Format(CultureInfo.InvariantCulture, "{0}/{1}", siteUrl.TrimEnd('/'), folderRelativePath.TrimStart('/'));
            }

            return baseFolder.FullPath ?? string.Empty;
        }

        private ArchiverNodeItem BuildManifestFolderNode(ArchiverNodeItem parentFolderNode, string folderFullPath)
        {
            var normalizedFullPath = (folderFullPath ?? parentFolderNode.FullPath ?? string.Empty).Replace("\\", "/", StringComparison.Ordinal).TrimEnd('/');
            var folderName = normalizedFullPath.Split('/')?.LastOrDefault() ?? parentFolderNode.Name;

            return new ArchiverNodeItem
            {
                ID = Guid.NewGuid(),
                Name = folderName,
                FullPath = string.IsNullOrWhiteSpace(normalizedFullPath) ? parentFolderNode.FullPath : normalizedFullPath,
                SPNodeLevel = NodeLevel.Folder,
                ItemType = parentFolderNode.ItemType,
                DiscoverSPObject = parentFolderNode.DiscoverSPObject,
                ListType = parentFolderNode.ListType,
                Cache_NodeType = parentFolderNode.Cache_NodeType,
                Parent = parentFolderNode,
                IsSystemObject = parentFolderNode.IsSystemObject,
                LibRowID = parentFolderNode.LibRowID,
                Modified = parentFolderNode.Modified,
                Created = parentFolderNode.Created,
                SiteUrl = parentFolderNode.SiteUrl,
                SiteId = parentFolderNode.SiteId,
                WebId = parentFolderNode.WebId,
                ListId = parentFolderNode.ListId,
                IsAppData = parentFolderNode.IsAppData,
                DocumentSize = parentFolderNode.DocumentSize,
                Author = parentFolderNode.Author,
                Editor = parentFolderNode.Editor,
                ShouldDoArchive = parentFolderNode.ShouldDoArchive,
                RuleId = parentFolderNode.RuleId,
                RuleName = parentFolderNode.RuleName,
                RuleArchiverAction = parentFolderNode.RuleArchiverAction,
                WebApplicationId = parentFolderNode.WebApplicationId,
                WebApplicationUrl = parentFolderNode.WebApplicationUrl,
                ItemIDs = parentFolderNode.ItemIDs
            };
        }

        private ArchiverNodeItem BuildManifestItemNode(ManifestDiscoveredItem entry, ArchiverNodeItem folderNode)
        {
            var metadata = entry.Metadata;
            Guid.TryParse(metadata.ItemUniqueId, out var uniqueId);
            Guid.TryParse(metadata.SiteId, out var siteId);
            Guid.TryParse(metadata.WebId, out var webId);
            Guid.TryParse(metadata.ListId, out var listId);

            var fullPath = !string.IsNullOrWhiteSpace(metadata.FullUrl)
                ? metadata.FullUrl
                : string.Format("{0}/{1}", folderNode.FullPath?.TrimEnd('/') ?? string.Empty, metadata.Name).Replace("//", "/");

            var node = new ArchiverNodeItem
            {
                ID = uniqueId == Guid.Empty ? Guid.NewGuid() : uniqueId,
                Name = metadata.Name,
                FullPath = fullPath,
                SPNodeLevel = NodeLevel.Item,
                ItemType = ArchiverCommon.ItemType.DOCUMENT,
                DiscoverSPObject = entry,
                ListType = folderNode.ListType,
                Cache_NodeType = (int)CacheNodeType.HSMItem,
                Parent = folderNode,
                IsSystemObject = false,
                LibRowID = metadata.ItemId,
                Modified = metadata.ModifiedTime.Ticks,
                Created = metadata.CreatedTime.Ticks,
                SiteUrl = metadata.SiteUrl ?? folderNode.SiteUrl,
                SiteId = siteId == Guid.Empty ? folderNode.SiteId : siteId,
                WebId = webId == Guid.Empty ? folderNode.WebId : webId,
                ListId = listId == Guid.Empty ? folderNode.ListId : listId,
                IsAppData = folderNode.IsAppData,
                DocumentSize = metadata.FileSize,
                Author = metadata.AuthorId.ToString(CultureInfo.InvariantCulture),
                Editor = metadata.EditorId.ToString(CultureInfo.InvariantCulture),
                ShouldDoArchive = folderNode.ShouldDoArchive,
                RuleId = folderNode.RuleId,
                RuleName = folderNode.RuleName,
                RuleArchiverAction = folderNode.RuleArchiverAction,
                WebApplicationId = folderNode.WebApplicationId,
                WebApplicationUrl = folderNode.WebApplicationUrl,
                ItemIDs = new List<int> { metadata.ItemId }
            };

            if (folderNode.IsAppData)
            {
                node.AppDataName = folderNode.AppDataName;
            }

            node.ManifestSnapshot = BuildManifestSnapshot(entry, folderNode, false, metadata.CurrentVersion, null, null);
            if (node.ManifestSnapshot != null && node.ManifestSnapshot.DocumentSize > 0)
            {
                entry.Metadata.FileSize = node.ManifestSnapshot.DocumentSize;
                node.DocumentSize = node.ManifestSnapshot.DocumentSize;
            }

            return node;
        }

        private ArchiverNodeItem BuildManifestVersionNode(ArchiverNodeItem itemNode, AveVersionObject version, ManifestDiscoveredItem manifestEntry)
        {
            var versionLabel = BuildVersionLabel(version);
            var versionNode = new ArchiverNodeItem
            {
                ID = itemNode.ID,
                Name = string.Format("{0}:{1}", itemNode.Name, versionLabel),
                FullPath = string.Format("{0}:{1}", itemNode.FullPath, versionLabel),
                SPNodeLevel = NodeLevel.Item,
                ItemType = ArchiverCommon.ItemType.DOCUMENT_VER,
                DiscoverSPObject = version,
                ListType = itemNode.ListType,
                Cache_NodeType = (int)CacheNodeType.HSMItemVersion,
                Parent = itemNode,
                IsSystemObject = itemNode.IsSystemObject,
                LibRowID = itemNode.LibRowID,
                Modified = version.TimeLastModified.Ticks,
                Created = itemNode.Created,
                SiteUrl = itemNode.SiteUrl,
                SiteId = itemNode.SiteId,
                WebId = itemNode.WebId,
                ListId = itemNode.ListId,
                IsAppData = itemNode.IsAppData,
                DocumentSize = version.Size,
                Author = itemNode.Author,
                Editor = itemNode.Editor,
                ShouldDoArchive = itemNode.ShouldDoArchive,
                RuleId = itemNode.RuleId,
                RuleName = itemNode.RuleName,
                RuleArchiverAction = itemNode.RuleArchiverAction,
                WebApplicationId = itemNode.WebApplicationId,
                WebApplicationUrl = itemNode.WebApplicationUrl
            };

            if (itemNode.IsAppData)
            {
                versionNode.AppDataName = itemNode.AppDataName;
            }

            if (manifestEntry != null)
            {
                var manifestVersionKey = !string.IsNullOrWhiteSpace(version.UiVersionString) ? version.UiVersionString : versionLabel;
                var fileVersion = manifestEntry.File?.Versions?.FirstOrDefault(v => string.Equals(v.Version, manifestVersionKey, StringComparison.OrdinalIgnoreCase));
                var listItemVersion = manifestEntry.ListItem?.Versions?.FirstOrDefault(v => string.Equals(v.Version, manifestVersionKey, StringComparison.OrdinalIgnoreCase));
                versionNode.ManifestSnapshot = BuildManifestSnapshot(manifestEntry, itemNode, true, manifestVersionKey, fileVersion, listItemVersion);
                if (versionNode.ManifestSnapshot != null && versionNode.ManifestSnapshot.DocumentSize > 0)
                {
                    versionNode.DocumentSize = versionNode.ManifestSnapshot.DocumentSize;
                }
            }

            return versionNode;
        }
        private long BuildManifestVersionSize(ArchiverNodeItem itemNode, AveVersionObject version, ManifestDiscoveredItem manifestEntry)
        {
            long size = 0;
            if (manifestEntry != null && !string.IsNullOrEmpty(manifestEntry.ContentStorageConnectionString))
            {
                size = manifestEntry.Metadata.FileSize;
                var containerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(manifestEntry.ContentStorageConnectionString);
                var blobClient = containerClient.GetBlobClient(version.FileValue);
                var exists = blobClient.Exists();
                if (exists.Value)
                {
                    var prop = blobClient.GetProperties();
                    size = prop.Value.ContentLength;
                }
            }
            return size;
        }
        private ManifestDocumentSnapshot BuildManifestSnapshot(ManifestDiscoveredItem entry, ArchiverNodeItem contextNode, bool isVersion, string versionLabel, HsmManifestFileVersion? fileVersion, HsmManifestListItemVersion? listItemVersion)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var metadata = entry.Metadata ?? throw new InvalidOperationException("Manifest entry metadata is required.");
            var normalizedVersionLabel = string.IsNullOrWhiteSpace(versionLabel) ? metadata.CurrentVersion ?? string.Empty : versionLabel;
            var columnValues = BuildManifestColumnValues(entry, fileVersion, listItemVersion);
            RemoveNullOrEmptyColumnValues(columnValues);
            var writableColumns = FilterWritableColumns(columnValues);
            var serverRelativeUrl = BuildServerRelativeUrl(entry);
            var fileValue = fileVersion?.FileValue ?? entry.File?.FileValue;
            var contentFilePath = ResolveManifestContentPath(entry, fileValue);
            long resolvedSize = metadata.FileSize;


            if (resolvedSize < 0)
            {
                resolvedSize = 0;
            }

            metadata.FileSize = resolvedSize;

            var tagOverrides = BuildTagInfoOverrides(metadata.Tags);
            var metadataEntries = BuildManifestMetadataEntries(writableColumns, entry, isVersion, normalizedVersionLabel, fileVersion, listItemVersion);
            var baseAccessUrl = string.IsNullOrWhiteSpace(metadata.FullUrl) ? serverRelativeUrl : metadata.FullUrl;

            var authorFieldValue = GetUserFieldValue(entry, fileVersion, listItemVersion, "Created_x0020_By");
            var authorValue = ResolveAuthor(entry, fileVersion, listItemVersion);
            var authorStringSource = string.IsNullOrWhiteSpace(authorFieldValue) ? authorValue : authorFieldValue;

            var editorFieldValue = GetUserFieldValue(entry, fileVersion, listItemVersion, "Modified_x0020_By");
            var editorValue = ResolveEditor(entry, fileVersion, listItemVersion);
            var editorStringSource = string.IsNullOrWhiteSpace(editorFieldValue) ? editorValue : editorFieldValue;
            var _ContentFilePath = contentFilePath ?? fileValue ?? string.Empty;
            var _ContentBlobPrefix = entry.ContentBlobPrefix;
            mLog.Info($"build current item snapshot,content file path:{_ContentFilePath},content blob prefix:{_ContentBlobPrefix}");
            return new ManifestDocumentSnapshot
            {
                Site = BuildManifestSiteInfo(entry),
                List = BuildManifestListInfo(entry, contextNode),
                Folder = BuildManifestFolderInfo(entry, contextNode, serverRelativeUrl),
                DocumentAccessUrl = BuildDocumentAccessUrl(baseAccessUrl, isVersion, normalizedVersionLabel),
                DocumentServerRelativeUrl = serverRelativeUrl,
                FileServerRelativeUrl = serverRelativeUrl,
                IsSystemFile = contextNode.IsSystemObject,
                HasUniqueRoleAssignments = TryGetBooleanColumn(writableColumns, "_HasUniqueRoleAssignments") || TryGetBooleanColumn(writableColumns, "HasUniqueRoleAssignments"),
                IsVersion = isVersion,
                DocumentSize = resolvedSize,
                FileTitle = ResolveFileTitle(metadata, writableColumns),
                ColumnValues = writableColumns,
                TagInfoOverrides = tagOverrides,
                MetadataEntries = metadataEntries,
                ContentFilePath = _ContentFilePath,
                RecordsRelatedValue = contextNode.RelatedRecordInfo,
                EnableHsm = true,
                PathMd5 = ComputeMd5(baseAccessUrl) ?? string.Empty,
                ScopeUrl = contextNode.FullPath,
                CreatedTime = fileVersion?.Created ?? listItemVersion?.Created ?? metadata.CreatedTime,
                ModifiedTime = fileVersion?.Modified ?? listItemVersion?.Modified ?? metadata.ModifiedTime,
                Author = authorValue,
                AuthorString = ExtractUserIdentifier(authorStringSource),
                Editor = editorValue,
                EditorString = ExtractUserIdentifier(editorStringSource),
                AuthorId = ToIntId(metadata.AuthorId),
                EditorId = ToIntId(metadata.EditorId),
                ContentBlobPrefix = _ContentBlobPrefix,
                StorageContainerName = entry.StorageContainerName,
                StorageConnectionString = entry.StorageConnectionString,
                Version = metadata.CurrentVersion ?? string.Empty,
                ContentStorageConnectionString = entry.ContentStorageConnectionString
            };
        }

        private static ManifestSiteInfo BuildManifestSiteInfo(ManifestDiscoveredItem entry)
        {
            return new ManifestSiteInfo
            {
                Url = entry.Metadata?.SiteUrl ?? entry.SiteUrl ?? string.Empty,
                ServerRelativeUrl = ExtractServerRelativeUrl(entry.Metadata?.SiteUrl)
            };
        }

        private static ManifestListInfo BuildManifestListInfo(ManifestDiscoveredItem entry, ArchiverNodeItem contextNode)
        {
            return new ManifestListInfo
            {
                Id = entry.ListId,
                BaseTemplate = contextNode.ListType,
                Hidden = false,
                IsCatalog = false
            };
        }

        private static ManifestFolderInfo BuildManifestFolderInfo(ManifestDiscoveredItem entry, ArchiverNodeItem contextNode, string fileServerRelativeUrl)
        {
            var folderRelative = !string.IsNullOrWhiteSpace(entry.Metadata?.FolderRelativeUrl)
                ? entry.Metadata.FolderRelativeUrl
                : Path.GetDirectoryName(fileServerRelativeUrl)?.Replace("\\", "/", StringComparison.Ordinal) ?? string.Empty;

            return new ManifestFolderInfo
            {
                ServerRelativeUrl = NormalizeServerRelativePath(folderRelative ?? string.Empty),
                Path = contextNode.FullPath ?? string.Empty
            };
        }

        private static string BuildDocumentAccessUrl(string baseUrl, bool isVersion, string versionLabel)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return string.Empty;
            }

            if (isVersion && !string.IsNullOrWhiteSpace(versionLabel))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", baseUrl, versionLabel);
            }

            return baseUrl;
        }

        private static string BuildServerRelativeUrl(ManifestDiscoveredItem entry)
        {
            var relative = entry.ListItem?.Url;
            if (string.IsNullOrWhiteSpace(relative))
            {
                relative = entry.File?.Url;
            }

            if (string.IsNullOrWhiteSpace(relative))
            {
                return entry.Metadata?.FullUrl ?? string.Empty;
            }

            if (relative.StartsWith("/", StringComparison.Ordinal))
            {
                return relative;
            }

            var siteRelative = ExtractServerRelativeUrl(entry.Metadata?.SiteUrl);
            if (string.IsNullOrWhiteSpace(siteRelative))
            {
                return "/" + relative.TrimStart('/');
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", siteRelative.TrimEnd('/'), relative.TrimStart('/'));
        }

        private static string NormalizeServerRelativePath(string relative)
        {
            if (string.IsNullOrWhiteSpace(relative))
            {
                return string.Empty;
            }

            var normalized = relative.Replace("\\", "/", StringComparison.Ordinal);
            return normalized.StartsWith("/", StringComparison.Ordinal) ? normalized : "/" + normalized.TrimStart('/');
        }

        private static string ExtractServerRelativeUrl(string? siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(siteUrl, UriKind.Absolute, out var uri))
            {
                return string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;
            }

            return siteUrl.StartsWith("/", StringComparison.Ordinal) ? siteUrl : "/" + siteUrl.TrimStart('/');
        }

        private static readonly HashSet<string> SkippedColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // 之前为了避免 Lookup 解析崩溃临时屏蔽
            "MetaInfo",
            "CheckedOutTitle",

            // 与还原端重复或已在 DocProperty 提供，避免被当作 Lookup 解析
            "UniqueId",
            "FileRef",
            "FileDirRef",

            // 常见内置/系统字段，值为空或非数字时会触发 Lookup/DateTime 解析异常
            "TaxCatchAll",
            "Last_x0020_Modified",
            "Created_x0020_Date",
            "SortBehavior",
            "IsCheckedoutToLocal",
            "ParentUniqueId",
            "ParentVersionString",
            "ParentLeafName",
            "SyncClientId",
            "ProgId",
            "ScopeId",
            "ItemChildCount",
            "FolderChildCount",
            "Restricted",
            "OriginatorId",
            "NoExecute",
            "ContentVersion",
            "DocConcurrencyNumber",
            "StreamHash",
            "AccessPolicy",
            "VirusStatus",
            "_VirusStatus",
            "_VirusVendorID",
            "_VirusInfo",

            // 合规/标签类
            "_ComplianceFlags",
            "_ComplianceTag",
            "_ComplianceTagWrittenTime",
            "_ComplianceTagUserId",
            "_IpLabelId",
            "_IpLabelAssignmentMethod",
            "_IpLabelMetaInfo",
            "_IpLabelHash",
            "_IpLabelPromotionCtagVersion",
            "_DisplayName",
            "_DraftOwnerId",

            // 其它系统/计数/扩展元数据
            "BSN",
            "_ListSchemaVersion",
            "_Dirty",
            "_Parsable",
            "_StubFile",
            "_HasEncryptedContent",
            "_HasUserDefinedProtection",
            "_RansomwareAnomalyMetaInfo",
            "_CommentFlags",
            "_CommentCount",
            "_LikeCount",
            "_RmsTemplateId",
            "_ExpirationDate",
            "_AdditionalStreamSize",
            "_StreamScenarioIds",
            "_FileArchiveStatus",
            "_ExtractedMetadata",

            // SM 开头的一组同步统计字段
            "SMTotalSize",
            "SMLastModifiedDate",
            "SMTotalFileStreamSize",
            "SMTotalFileCount",

            // 留空：自定义列不在此处屏蔽，避免丢失业务字段
        };

        // Blacklist of read-only/system columns that must not be backed up to avoid restore failures.
        private static readonly HashSet<string> ReadOnlyColumnBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ID","GUID","UniqueId","FileRef","FileDirRef","FileLeafRef","FSObjType","ContentTypeId","ContentType",
            "_UIVersion","_UIVersionString","_Level","_ModerationStatus","_CheckinComment","WorkflowVersion","WorkflowInstanceID",
            "_HasCopyDestinations","_CopySource","AppAuthor","AppEditor","PermMask",
            "DocIcon","FileSizeDisplay","SortBehavior","MetaInfo","File_x0020_Size","EncodedAbsUrl","ServerUrl","BaseName",
            "_IsCurrentVersion","_IsRecord","InstanceID","_ModerationComments","Modified_x0020_By","Created_x0020_By","File_x0020_Type",
            "HTML_x0020_File_x0020_Type","_SourceUrl","_SharedFileIndex","_ColorHex","_ColorTag","_Emoji","MediaGeneratedMetadata",
            "MediaUserMetadata","ExtractedMetadataComputed","ComplianceAssetId","TemplateUrl","xd_ProgID","xd_Signature","_EffectiveIpLabelDisplayName",
            "_ShortcutUrl","_ShortcutSiteId","_ShortcutWebId","_ShortcutUniqueId","_ExtendedDescription","TriggerFlowInfo","PrincipalCount",
            "LinkCheckedOutTitle","_EditMenuTableStart","_EditMenuTableStart2","_EditMenuTableEnd","A2ODMountCount","MainLinkSettings",
            "SelectTitle","SelectFilename","Combine","RepairDocument","PolicyDisabledUICapabilities"
        };

        private static string? ResolveManifestContentPath(ManifestDiscoveredItem? entry, string? fileValue)
        {
            if (entry == null)
            {
                mLog.Warn("ResolveManifestContentPath returned null because manifest entry is null.");
                return null;
            }

            mLog.Info(
                "ResolveManifestContentPath started. RelativePath:{0}, ContentDirectory:{1}, ManifestDirectory:{2}, HasContentStorageConnection:{3}.",
                DescribePathForLog(fileValue),
                DescribePathForLog(entry.ContentDirectoryPath),
                DescribePathForLog(entry.ManifestDirectoryPath),
                !string.IsNullOrWhiteSpace(entry.ContentStorageConnectionString));

            // Lazy download from Azure when content is missing locally.
            if (!string.IsNullOrWhiteSpace(entry.ContentStorageConnectionString))
            {
                if (string.IsNullOrWhiteSpace(fileValue))
                {
                    mLog.Info("ResolveManifestContentPath skipped Azure lazy-download branch because the relative path is empty.");
                }
                else
                {
                    try
                    {
                        var normalizedBlobName = fileValue.Replace("\\", "/", StringComparison.Ordinal).Replace("//", "/", StringComparison.Ordinal);
                        mLog.Info("ResolveManifestContentPath entered Azure lazy-download branch. BlobPath:{0}.", DescribePathForLog(normalizedBlobName));
                        return normalizedBlobName;

                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("ResolveManifestContentPath Azure lazy-download branch failed. BlobPath:{0}.", DescribePathForLog(fileValue), ex);
                    }
                }
            }
            else
            {
                mLog.Info("ResolveManifestContentPath skipped Azure lazy-download branch because content storage connection is empty.");
            }

            if (string.IsNullOrWhiteSpace(fileValue))
            {
                mLog.Info("ResolveManifestContentPath returned null because the relative path is empty and no earlier branch resolved it.");
                return null;
            }

            if (Path.IsPathRooted(fileValue))
            {
                if (File.Exists(fileValue))
                {
                    mLog.Info("ResolveManifestContentPath resolved by rooted local path branch. LocalDirectory:{0}.", DescribePathForLog(fileValue));
                    return fileValue;
                }

                mLog.Info("ResolveManifestContentPath rooted local path branch did not find the file. LocalDirectory:{0}.", DescribePathForLog(fileValue));
            }
            else
            {
                mLog.Info("ResolveManifestContentPath skipped rooted local path branch because the relative path is not rooted. RelativePath:{0}.", DescribePathForLog(fileValue));
            }

            if (!string.IsNullOrWhiteSpace(entry.ContentDirectoryPath))
            {
                var candidate = Path.Combine(entry.ContentDirectoryPath, fileValue);
                if (File.Exists(candidate))
                {
                    mLog.Info("ResolveManifestContentPath resolved by content directory branch. ContentDirectory:{0}, CandidateDirectory:{1}.", DescribePathForLog(entry.ContentDirectoryPath), DescribePathForLog(candidate));
                    return candidate;
                }

                mLog.Info("ResolveManifestContentPath content directory branch did not find the file. ContentDirectory:{0}, CandidateDirectory:{1}.", DescribePathForLog(entry.ContentDirectoryPath), DescribePathForLog(candidate));
            }
            else
            {
                mLog.Info("ResolveManifestContentPath skipped content directory branch because content directory is empty.");
            }

            if (!string.IsNullOrWhiteSpace(entry.ManifestDirectoryPath))
            {
                var candidate = Path.Combine(entry.ManifestDirectoryPath, fileValue);
                if (File.Exists(candidate))
                {
                    mLog.Info("ResolveManifestContentPath resolved by manifest directory branch. ManifestDirectory:{0}, CandidateDirectory:{1}.", DescribePathForLog(entry.ManifestDirectoryPath), DescribePathForLog(candidate));
                    return candidate;
                }

                mLog.Info("ResolveManifestContentPath manifest directory branch did not find the file. ManifestDirectory:{0}, CandidateDirectory:{1}.", DescribePathForLog(entry.ManifestDirectoryPath), DescribePathForLog(candidate));
            }
            else
            {
                mLog.Info("ResolveManifestContentPath skipped manifest directory branch because manifest directory is empty.");
            }

            mLog.Info(
                "ResolveManifestContentPath returned null after all branches. RelativePath:{0}, ContentDirectory:{1}, ManifestDirectory:{2}.",
                DescribePathForLog(fileValue),
                DescribePathForLog(entry.ContentDirectoryPath),
                DescribePathForLog(entry.ManifestDirectoryPath));
            return null;
        }

        private static string DescribePathForLog(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "(empty)";
            }

            var normalizedPath = path.Replace("\\", "/", StringComparison.Ordinal).TrimEnd('/');
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return "/";
            }

            if (normalizedPath.EndsWith(":", StringComparison.Ordinal))
            {
                return normalizedPath + "/";
            }

            var directory = Path.GetDirectoryName(normalizedPath)?.Replace("\\", "/", StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                return directory;
            }

            return "(root)";
        }

        private static Dictionary<string, object> BuildManifestColumnValues(ManifestDiscoveredItem entry, HsmManifestFileVersion? fileVersion, HsmManifestListItemVersion? listItemVersion)
        {
            var columns = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (entry.ListItem?.Fields != null)
            {
                AppendFieldValues(columns, entry.ListItem.Fields);
            }
            else if (entry.File?.Fields != null)
            {
                AppendFieldValues(columns, entry.File.Fields);
            }

            if (listItemVersion?.Fields != null)
            {
                AppendFieldValues(columns, listItemVersion.Fields);
            }

            if (fileVersion?.Fields != null)
            {
                AppendFieldValues(columns, fileVersion.Fields);
            }

            if (!columns.ContainsKey("FileLeafRef"))
            {
                columns["FileLeafRef"] = entry.Metadata?.Name ?? string.Empty;
            }
            if (!columns.ContainsKey("FileDirRef"))
            {
                columns["FileDirRef"] = entry.Metadata?.FolderRelativeUrl ?? string.Empty;
            }
            if (!columns.ContainsKey("UniqueId"))
            {
                columns["UniqueId"] = entry.Metadata?.ItemUniqueId ?? string.Empty;
            }
            if (!columns.ContainsKey("FileRef"))
            {
                columns["FileRef"] = entry.Metadata?.FullUrl ?? string.Empty;
            }

            if (!columns.ContainsKey("HasStream"))
            {
                columns["HasStream"] = (entry.File != null || (entry.Metadata?.FileSize ?? 0) > 0) ? 1 : 0;
            }

            // 补充常用只读用户字段，便于还原端写回 Author/Editor（保留为整型 ID）
            if (!columns.ContainsKey("Author"))
            {
                columns["Author"] = entry.Metadata?.AuthorId ?? 0;
            }
            if (!columns.ContainsKey("Editor"))
            {
                columns["Editor"] = entry.Metadata?.EditorId ?? 0;
            }

            NormalizeLookupFriendlyValues(columns);

            return columns;
        }

        private static void RemoveNullOrEmptyColumnValues(IDictionary<string, object> columns)
        {
            if (columns == null || columns.Count == 0)
            {
                return;
            }

            var keysToRemove = new List<string>();
            foreach (var kv in columns)
            {
                if (kv.Value == null)
                {
                    keysToRemove.Add(kv.Key);
                    continue;
                }

                if (kv.Value is string s && string.IsNullOrWhiteSpace(s))
                {
                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                columns.Remove(key);
            }
        }

        private static Dictionary<string, object> FilterWritableColumns(Dictionary<string, object> source)
        {
            if (source == null || source.Count == 0)
            {
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            var filtered = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in source)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key) && !ReadOnlyColumnBlacklist.Contains(kv.Key))
                {
                    filtered[kv.Key] = kv.Value;
                }
            }

            return filtered;
        }

        private static void NormalizeLookupFriendlyValues(IDictionary<string, object> columns)
        {
            if (columns == null || columns.Count == 0)
            {
                return;
            }

            foreach (var key in columns.Keys.ToList())
            {
                if (columns[key] is not string value || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                // 仅针对可能的 Lookup/Person 形态做格式修正，避免误改普通文本。
                // 1) 数字开头的 "id;value" 补 ";#"（例如 "10;UserInfo" -> "10;#UserInfo"）。
                if (value.Contains(';') && !value.Contains(";#", StringComparison.Ordinal))
                {
                    var firstSegment = value.Split(';')[0];
                    if (int.TryParse(firstSegment, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        var idx = value.IndexOf(';');
                        if (idx >= 0 && idx < value.Length - 1)
                        {
                            columns[key] = value.Insert(idx + 1, "#");
                            continue;
                        }
                    }
                }

                // 2) 已知的路径/GUID字段（UniqueId, FileRef）如果缺少分号格式，包装为 "0;#value" 以兼容 lookup 解析。
                if ((key.Equals("UniqueId", StringComparison.OrdinalIgnoreCase)
                        || key.Equals("FileRef", StringComparison.OrdinalIgnoreCase))
                    && !value.Contains(";#", StringComparison.Ordinal))
                {
                    columns[key] = string.Format(CultureInfo.InvariantCulture, "0;#{0}", value);
                }
            }
        }

        private static void AppendFieldValues(IDictionary<string, object> target, IReadOnlyDictionary<string, HsmManifestField>? fields)
        {
            if (fields == null)
            {
                return;
            }

            foreach (var field in fields.Values)
            {
                if (field == null || string.IsNullOrWhiteSpace(field.Name))
                {
                    continue;
                }

                if (SkippedColumnNames.Contains(field.Name))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(field.Value) && !string.IsNullOrEmpty(field.Value2) && field.Value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    target[field.Name] = field.Value;
                    target[field.Name+"#2"] = field.Value2;
                }
                else
                {
                    target[field.Name] = NormalizeFieldValue(field);
                }
            }
        }

        private static object NormalizeFieldValue(HsmManifestField field)
        {
            if (!string.IsNullOrWhiteSpace(field.Value) && !string.IsNullOrWhiteSpace(field.Value2))
            {
                if (field.Name.Equals("_dlc_DocIdUrl", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", field.Value, field.Value2);
                }
                return string.Format(CultureInfo.InvariantCulture, "{0};#{1}", field.Value, field.Value2);
            }

            if (!string.IsNullOrWhiteSpace(field.Value))
            {
                return field.Value;
            }

            if (!string.IsNullOrWhiteSpace(field.Value2))
            {
                return field.Value2;
            }

            return string.Empty;
        }

        private static IList<ManifestMetadataEntry> BuildManifestMetadataEntries(Dictionary<string, object> columnValues, ManifestDiscoveredItem entry, bool isVersion, string versionLabel, HsmManifestFileVersion? fileVersion, HsmManifestListItemVersion? listItemVersion)
        {
            var entries = new List<ManifestMetadataEntry>();

            var versionNumber = ConvertVersionLabelToUiversion(versionLabel);

            bool hasStream = entry.File != null || (entry.Metadata?.FileSize ?? 0) > 0;

            var docProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["UniqueId"] = entry.Metadata?.ItemUniqueId ?? string.Empty,
                ["LeafName"] = entry.Metadata?.Name ?? string.Empty,
                ["FullUrl"] = entry.Metadata?.FullUrl ?? string.Empty,
                ["IsUserDocVersion"] = isVersion,
                ["DoclibRowId"] = entry.Metadata?.ItemId ?? 0,
                ["UIVersion"] = versionNumber.ToString(CultureInfo.InvariantCulture),
                ["TimeCreated"] = fileVersion?.Created ?? listItemVersion?.Created ?? entry.Metadata?.CreatedTime ?? DateTime.MinValue,
                ["TimeLastModified"] = fileVersion?.Modified ?? listItemVersion?.Modified ?? entry.Metadata?.ModifiedTime ?? DateTime.MinValue,
                ["HasStream"] = hasStream ? 1 : 0
            };

            entries.Add(new ManifestMetadataEntry
            {
                Type = AveMetadataType.DocProperty.ToString(),
                Data = docProperties
            });

            // DocData 需要紧跟在 DocProperty 之后，方便还原侧 TryReadMetadata 顺序读取。
            if (columnValues.Count > 0)
            {
                entries.Add(new ManifestMetadataEntry
                {
                    Type = AveMetadataType.DocData.ToString(),
                    Data = new Dictionary<string, object>(columnValues, StringComparer.OrdinalIgnoreCase)
                });
            }

            if (!isVersion && entry.RoleAssignments != null && entry.RoleAssignments.Count > 0)
            {
                var roleAssignments = entry.RoleAssignments
                    .Select(r => new AveRoleAssignmentInfo
                    {
                        RoleId = r.RoleId,
                        PrincipalId = r.PrincipalId,
                        RoleName = r.RoleName,
                        MemberLoginName = r.MemberLoginName,
                        MemberType = r.MemberType
                    })
                    .ToList();

                if (roleAssignments.Count > 0)
                {
                    entries.Add(new ManifestMetadataEntry
                    {
                        Type = AveMetadataType.RoleAssignment.ToString(),
                        Data = roleAssignments
                    });
                }
            }

            return entries;
        }

        private static List<TagInfoCollection> BuildTagInfoOverrides(Dictionary<string, object>? tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return new List<TagInfoCollection>();
            }

            var overrides = new List<TagInfoCollection>(tags.Count);
            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag.Key))
                {
                    continue;
                }

                overrides.Add(new TagInfoCollection
                {
                    Key = tag.Key,
                    Value = tag.Value ?? string.Empty
                });
            }

            return overrides;
        }

        private static bool TryGetBooleanColumn(Dictionary<string, object> columns, string columnName)
        {
            if (!columns.TryGetValue(columnName, out var raw) || raw == null)
            {
                return false;
            }

            if (raw is bool booleanValue)
            {
                return booleanValue;
            }

            if (raw is string stringValue)
            {
                if (bool.TryParse(stringValue, out var parsedBool))
                {
                    return parsedBool;
                }

                if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt))
                {
                    return parsedInt != 0;
                }
            }

            return false;
        }

        private static string ResolveFileTitle(RMDiscoveryFileDataInfo metadata, Dictionary<string, object> columns)
        {
            if (columns.TryGetValue("Title", out var titleValue) && titleValue != null)
            {
                return titleValue.ToString() ?? string.Empty;
            }

            return metadata?.Name ?? string.Empty;
        }

        private static string ResolveAuthor(ManifestDiscoveredItem entry, HsmManifestFileVersion? fileVersion, HsmManifestListItemVersion? listItemVersion)
        {
            if (!string.IsNullOrWhiteSpace(fileVersion?.Author))
            {
                return fileVersion.Author;
            }

            if (!string.IsNullOrWhiteSpace(entry.File?.Author))
            {
                return entry.File.Author;
            }

            if (!string.IsNullOrWhiteSpace(entry.ListItem?.Author))
            {
                return entry.ListItem.Author;
            }

            return entry.Metadata?.AuthorId.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string ResolveEditor(ManifestDiscoveredItem entry, HsmManifestFileVersion? fileVersion, HsmManifestListItemVersion? listItemVersion)
        {
            if (!string.IsNullOrWhiteSpace(fileVersion?.ModifiedBy))
            {
                return fileVersion.ModifiedBy;
            }

            if (!string.IsNullOrWhiteSpace(entry.File?.ModifiedBy))
            {
                return entry.File.ModifiedBy;
            }

            if (!string.IsNullOrWhiteSpace(entry.ListItem?.ModifiedBy))
            {
                return entry.ListItem.ModifiedBy;
            }

            return entry.Metadata?.EditorId.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static string ExtractUserIdentifier(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var candidate = raw;

            if (candidate.Contains(";#", StringComparison.Ordinal))
            {
                var parts = candidate.Split(new[] { ";#" }, StringSplitOptions.None);
                candidate = parts.Length > 1 ? parts[^1] : parts[0];
            }

            if (candidate.Contains("|", StringComparison.Ordinal))
            {
                var parts = candidate.Split('|');
                candidate = parts[^1];
            }

            return candidate.Trim();
        }

        private static string GetUserFieldValue(ManifestDiscoveredItem entry, HsmManifestFileVersion? fileVersion, HsmManifestListItemVersion? listItemVersion, string fieldKey)
        {
            var value = TryGetFieldValue(fileVersion?.Fields, fieldKey)
                       ?? TryGetFieldValue(listItemVersion?.Fields, fieldKey)
                       ?? TryGetFieldValue(entry.File?.Fields, fieldKey)
                       ?? TryGetFieldValue(entry.ListItem?.Fields, fieldKey);

            return value ?? string.Empty;
        }

        private static string? TryGetFieldValue(IReadOnlyDictionary<string, HsmManifestField>? fields, string fieldKey)
        {
            if (fields == null || string.IsNullOrWhiteSpace(fieldKey))
            {
                return null;
            }

            if (fields.TryGetValue(fieldKey, out var field) && field != null)
            {
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    return field.Value;
                }

                if (!string.IsNullOrWhiteSpace(field.Value2))
                {
                    return field.Value2;
                }
            }

            return null;
        }

        private static int ToIntId(long id)
        {
            if (id > int.MaxValue || id < int.MinValue)
            {
                return 0;
            }

            return (int)id;
        }

        private static string? ComputeMd5(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        private static string BuildVersionLabel(AveVersionObject version)
        {
            if (!string.IsNullOrWhiteSpace(version?.UiVersionString))
            {
                return version.UiVersionString;
            }

            if (version == null || version.Uiversion <= 0)
            {
                return "1.0";
            }

            var major = version.Uiversion / 512;
            var minor = version.Uiversion % 512;
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", major, minor);
        }

        private static int ConvertVersionLabelToUiversion(string? versionLabel)
        {
            if (string.IsNullOrWhiteSpace(versionLabel))
            {
                return 0;
            }

            var segments = versionLabel.Split('.');
            if (segments.Length == 2
                && int.TryParse(segments[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var major)
                && int.TryParse(segments[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor))
            {
                return (major * 512) + minor;
            }

            return int.TryParse(versionLabel, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;
        }

        private static bool ShouldSkipVersion(AveDiscoverItem item, AveVersionObject version)
        {
            if (item == null || version == null)
            {
                return true;
            }

            return version.Uiversion == item.Uiversion || version.Uiversion == 0;
        }

        private List<ManifestDiscoveredItem> LoadLocalManifestFileData(Guid webId, Guid listId, string siteId, string siteUrl)
        {
            var result = new List<ManifestDiscoveredItem>();
            var packageRoot = ResolveLocalPackageRoot(jobSettings.TraceId);
            if (string.IsNullOrWhiteSpace(packageRoot))
            {
                mLog.Warn("Local package root path cannot be resolved.");
                return result;
            }

            if (!Directory.Exists(packageRoot))
            {
                mLog.Warn($"Local package root directory not found at path {packageRoot}.");
                return result;
            }

            var siteFolderName = BuildSiteFolderName(siteUrl);
            var siteFolderPath = Path.Combine(packageRoot, siteFolderName);
            var listFolderName = listId.ToString("D", CultureInfo.InvariantCulture);
            var listFolderPath = Path.Combine(siteFolderPath, listFolderName);

            var manifestBundles = new List<(string manifestPath, string manifestDirectory, string contentDirectory)>();

            if (Directory.Exists(listFolderPath))
            {
                foreach (var packageDirectory in Directory.EnumerateDirectories(listFolderPath))
                {
                    var manifestDirectory = Path.Combine(packageDirectory, "MetaData");
                    var manifestPath = Path.Combine(manifestDirectory, "Manifest.xml");
                    if (!File.Exists(manifestPath))
                    {
                        continue;
                    }

                    var contentDirectory = Path.Combine(packageDirectory, "Content");
                    manifestBundles.Add((manifestPath, manifestDirectory, contentDirectory));
                }
            }

            if (manifestBundles.Count == 0 && Directory.Exists(siteFolderPath))
            {
                foreach (var packageDirectory in Directory.EnumerateDirectories(siteFolderPath))
                {
                    var manifestDirectory = Path.Combine(packageDirectory, "MetaData");
                    var manifestPath = Path.Combine(manifestDirectory, "Manifest.xml");
                    if (!File.Exists(manifestPath))
                    {
                        continue;
                    }

                    var contentDirectory = Path.Combine(packageDirectory, "Content");
                    manifestBundles.Add((manifestPath, manifestDirectory, contentDirectory));
                }
            }

            if (manifestBundles.Count == 0)
            {
                // fallback to legacy single-folder structure for backward compatibility
                var legacyManifestDirectory = Path.Combine(packageRoot, "MetaData");
                var legacyManifestPath = Path.Combine(legacyManifestDirectory, "Manifest.xml");
                var legacyContentDirectory = Path.Combine(packageRoot, "Content");
                if (File.Exists(legacyManifestPath))
                {
                    manifestBundles.Add((legacyManifestPath, legacyManifestDirectory, legacyContentDirectory));
                }
                else
                {
                    mLog.Warn($"No manifest files found under {listFolderPath}, {siteFolderPath} or legacy path {legacyManifestPath}.");
                    return result;
                }
            }

            var dedupKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var bundle in manifestBundles)
            {
                foreach (var entryPage in LoadManifestEntriesFromFile(bundle.manifestPath, bundle.manifestDirectory, bundle.contentDirectory, webId, listId, siteId, siteUrl))
                {
                    foreach (var entry in entryPage)
                    {
                        var uniqueKey = entry.Metadata?.ItemUniqueId;
                        if (!string.IsNullOrWhiteSpace(uniqueKey) && !dedupKeys.Add(uniqueKey))
                        {
                            continue;
                        }

                        result.Add(entry);
                    }
                }
            }

            return result;
        }

        private async IAsyncEnumerable<List<ManifestDiscoveredItem>> LoadAzureManifestFileDataAsync(
            string connectionString,
            string? rootPrefix,
            string traceId,
            Guid webId,
            Guid listId,
            string siteId,
            string siteUrl,
            string contentConnectionString,
            [global::System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default)
        {
            const int pageSize = 200;
            var tempWorkspaces = new List<string>();
            var manifestBundles = new List<(string manifestPath, string manifestDirectory, string contentDirectory, string contentBlobPrefix)>();
            var safeSiteUrl = siteUrl ?? string.Empty;
            try
            {
                var containerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(connectionString);
                var normalizedRootPrefix = string.IsNullOrWhiteSpace(rootPrefix)
                    ? RMConstants.GetImportArchiveDataFolderName(traceId)
                    : RMConstants.GetImportArchiveDataFolderName(traceId, rootPrefix.Trim().TrimEnd('/').Replace("\\", "/", StringComparison.Ordinal));

                var siteFolderName = BuildSiteFolderName(mConfiguration.SiteCollectionUrl);
                var webFolderName = webId.ToString("D", CultureInfo.InvariantCulture);
                var listFolderName = listId.ToString("D", CultureInfo.InvariantCulture);
                var listScopedPrefix = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}/{3}/", normalizedRootPrefix, siteFolderName, webFolderName, listFolderName);
                var sitePrefix = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/", normalizedRootPrefix, siteFolderName);
                var listScopeCandidates = 0;
                var siteScopeCandidates = 0;

                mLog.Info("LoadAzureManifestFileDataAsync started. RootPrefix:{0}, SitePrefix:{1}, ListPrefix:{2}, SiteUrl:{3}, WebId:{4}, ListId:{5}.", normalizedRootPrefix, sitePrefix, listScopedPrefix, safeSiteUrl, webId, listId);

                await foreach (var item in containerClient.GetBlobsByHierarchyAsync(default, default, prefix: listScopedPrefix, delimiter: "/", cancellationToken: cancellationToken))
                {
                    if (!item.IsPrefix || string.IsNullOrWhiteSpace(item.Prefix))
                    {
                        continue;
                    }

                    listScopeCandidates++;

                    var manifestBlobPath = string.Format(CultureInfo.InvariantCulture, "{0}MetaData/Manifest.xml", item.Prefix);
                    var localWorkspace = CreateTempWorkspaceForBlob(manifestBlobPath);
                    tempWorkspaces.Add(localWorkspace);
                    var localManifestPath = await DownloadBlobAsync(containerClient, manifestBlobPath, localWorkspace, cancellationToken).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(localManifestPath))
                    {
                        continue;
                    }

                    var manifestDirectory = Path.GetDirectoryName(localManifestPath) ?? localWorkspace;
                    var contentDirectory = Path.Combine(localWorkspace, "Content");
                    var contentBlobPrefix = string.Format(CultureInfo.InvariantCulture, "{0}Content/", item.Prefix);
                    manifestBundles.Add((localManifestPath, manifestDirectory, contentDirectory, contentBlobPrefix));
                    mLog.Info("Manifest bundle discovered in list scope. Prefix:{0}, BundleCount:{1}.", item.Prefix, manifestBundles.Count);
                }

                if (manifestBundles.Count == 0)
                {
                    mLog.Info("No manifest bundle found in list scope. Falling back to site scope. SitePrefix:{0}.", sitePrefix);

                    await foreach (var item in containerClient.GetBlobsByHierarchyAsync(default, default, prefix: sitePrefix, delimiter: "/", cancellationToken: cancellationToken))
                    {
                        if (!item.IsPrefix || string.IsNullOrWhiteSpace(item.Prefix))
                        {
                            continue;
                        }

                        siteScopeCandidates++;

                        var manifestBlobPath = string.Format(CultureInfo.InvariantCulture, "{0}MetaData/Manifest.xml", item.Prefix);
                        var localWorkspace = CreateTempWorkspaceForBlob(manifestBlobPath);
                        tempWorkspaces.Add(localWorkspace);
                        var localManifestPath = await DownloadBlobAsync(containerClient, manifestBlobPath, localWorkspace, cancellationToken).ConfigureAwait(false);
                        if (string.IsNullOrWhiteSpace(localManifestPath))
                        {
                            continue;
                        }

                        var manifestDirectory = Path.GetDirectoryName(localManifestPath) ?? localWorkspace;
                        var contentDirectory = Path.Combine(localWorkspace, "Content");
                        var contentBlobPrefix = string.Format(CultureInfo.InvariantCulture, "{0}Content/", item.Prefix);
                        manifestBundles.Add((localManifestPath, manifestDirectory, contentDirectory, contentBlobPrefix));
                        mLog.Info("Manifest bundle discovered in site scope. Prefix:{0}, BundleCount:{1}.", item.Prefix, manifestBundles.Count);
                    }
                }

                if (manifestBundles.Count == 0)
                {
                    var legacyPrefix = string.Format(CultureInfo.InvariantCulture, "{0}/", normalizedRootPrefix);
                    mLog.Info("No manifest bundle found in site scope. Falling back to legacy scope. LegacyPrefix:{0}.", legacyPrefix);

                    var legacyManifestBlobPath = string.Format(CultureInfo.InvariantCulture, "{0}MetaData/Manifest.xml", legacyPrefix);
                    var localWorkspace = CreateTempWorkspaceForBlob(legacyManifestBlobPath);
                    tempWorkspaces.Add(localWorkspace);
                    var localManifestPath = await DownloadBlobAsync(containerClient, legacyManifestBlobPath, localWorkspace, cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(localManifestPath))
                    {
                        var manifestDirectory = Path.GetDirectoryName(localManifestPath) ?? localWorkspace;
                        var contentDirectory = Path.Combine(localWorkspace, "Content");
                        var contentBlobPrefix = string.Format(CultureInfo.InvariantCulture, "{0}Content/", legacyPrefix);
                        manifestBundles.Add((localManifestPath, manifestDirectory, contentDirectory, contentBlobPrefix));
                    }
                    else
                    {
                        mLog.Warn($"No manifest files found under Azure prefix {listScopedPrefix}, site prefix {sitePrefix} or legacy prefix {legacyPrefix}.");
                    }
                }

                mLog.Info("Manifest discovery completed. BundleCount:{0}, TempWorkspaceCount:{1}, ListScopeCandidates:{2}, SiteScopeCandidates:{3}.", manifestBundles.Count, tempWorkspaces.Count, listScopeCandidates, siteScopeCandidates);

            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to load manifest data from Azure Storage for site {safeSiteUrl}.", ex);
                yield break;
            }

            try
            {
                var dedupKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var page = new List<ManifestDiscoveredItem>(pageSize);
                var bundleIndex = 0;
                var acceptedEntryCount = 0;
                var duplicateEntryCount = 0;
                var yieldedPageCount = 0;

                foreach (var bundle in manifestBundles)
                {
                    bundleIndex++;
                    mLog.Info("Parsing manifest bundle {0}/{1}. ContentPrefix:{2}.", bundleIndex, manifestBundles.Count, bundle.contentBlobPrefix ?? string.Empty);

                    foreach (var entryPage in LoadManifestEntriesFromFile(bundle.manifestPath, bundle.manifestDirectory, bundle.contentDirectory, webId, listId, siteId, safeSiteUrl, bundle.contentBlobPrefix, connectionString, contentConnectionString))
                    {
                        mLog.Info("Manifest bundle {0}/{1} produced a page with {2} entries.", bundleIndex, manifestBundles.Count, entryPage.Count);

                        foreach (var entry in entryPage)
                        {
                            var uniqueKey = entry.Metadata?.ItemUniqueId;
                            if (!string.IsNullOrWhiteSpace(uniqueKey) && !dedupKeys.Add(uniqueKey))
                            {
                                duplicateEntryCount++;
                                continue;
                            }

                            page.Add(entry);
                            acceptedEntryCount++;
                            if (page.Count == pageSize)
                            {
                                yieldedPageCount++;
                                mLog.Info("Yielding manifest page {0} with {1} entries.", yieldedPageCount, page.Count);
                                yield return page;
                                page = new List<ManifestDiscoveredItem>(pageSize);
                            }
                        }
                    }
                }

                if (page.Count > 0)
                {
                    yieldedPageCount++;
                    mLog.Info("Yielding final manifest page {0} with {1} entries.", yieldedPageCount, page.Count);
                    yield return page;
                }

                mLog.Info("LoadAzureManifestFileDataAsync completed. BundleCount:{0}, AcceptedEntries:{1}, DuplicateEntries:{2}, YieldedPages:{3}.", manifestBundles.Count, acceptedEntryCount, duplicateEntryCount, yieldedPageCount);
            }
            finally
            {
                foreach (var workspace in tempWorkspaces)
                {
                    DeleteDirectorySafe(workspace);
                }
            }

        }

        private static string ResolveLocalPackageRoot(string traceId)
        {
            var importArchiveDataFolderName = RMConstants.GetImportArchiveDataFolderName(traceId);
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var backupRoot = Path.Combine(repoRoot, "reco", "Common", "Wrapper", "Wrapper.Backup", importArchiveDataFolderName);
            if (Directory.Exists(backupRoot))
            {
                return backupRoot;
            }

            var fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Common", "Wrapper", "Wrapper.Backup", importArchiveDataFolderName);
            return fallback;
        }

        private static string CreateTempWorkspaceForBlob(string blobPath)
        {
            var safeName = ComputeMd5(blobPath) ?? Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var workspace = Path.Combine(Path.GetTempPath(), "HSMManifest", safeName);
            Directory.CreateDirectory(workspace);
            return workspace;
        }

        private static async Task<string?> DownloadBlobAsync(BlobContainerClient containerClient, string blobPath, string targetRoot, CancellationToken cancellationToken)
        {
            try
            {
                var blobClient = containerClient.GetBlobClient(blobPath);
                var exists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
                if (!exists)
                {
                    return null;
                }

                var localPath = Path.Combine(targetRoot, blobPath.Replace('/', Path.DirectorySeparatorChar));
                var localDirectory = Path.GetDirectoryName(localPath) ?? targetRoot;
                Directory.CreateDirectory(localDirectory);

                using (var stream = File.Create(localPath))
                {
                    await blobClient.DownloadToAsync(stream, cancellationToken).ConfigureAwait(false);
                }

                return localPath;
            }
            catch (Exception ex)
            {
                mLog.Warn("Failed to download a blob from Azure Storage.", ex);
                return null;
            }
        }

        private static void DeleteDirectorySafe(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to delete temp workspace {path}.", ex);
            }
        }

        private static string BuildSiteFolderName(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                return "UnknownSite";
            }

            // Use a fixed invalid set so the generated path is consistent across OS (Windows replaces ':' while Linux would not).
            var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars()) { ':', '\\', '/' };
            invalidChars.UnionWith(new[] { '?', '*', '"', '<', '>', '|' });

            var buffer = siteUrl.ToCharArray();
            for (var i = 0; i < buffer.Length; i++)
            {
                var ch = buffer[i];
                buffer[i] = invalidChars.Contains(ch) ? '#' : ch;
            }

            var candidate = new string(buffer).Trim('#');
            return string.IsNullOrWhiteSpace(candidate) ? "Site" : candidate;
        }

        private IEnumerable<List<ManifestDiscoveredItem>> LoadManifestEntriesFromFile(
            string manifestPath,
            string manifestDirectory,
            string contentDirectory,
            Guid webId,
            Guid listId,
            string siteId,
            string siteUrl,
            string? contentBlobPrefix = null,
            string? connectionString = null,
            string? contentConnectionString = null
            )
        {
            const int pageSize = 200;
            if (!File.Exists(manifestPath))
            {
                yield break;
            }

            IReadOnlyDictionary<string, List<AveRoleAssignmentInfo>> roleAssignmentsByObjectId = new Dictionary<string, List<AveRoleAssignmentInfo>>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<HsmManifestFile> manifestFiles = Enumerable.Empty<HsmManifestFile>();

            try
            {
                roleAssignmentsByObjectId = GetRoleAssignmentsByObjectId(manifestPath);

                manifestFiles = HsmManifestParser.EnumerateFilesByWebAndListId(manifestPath, webId, listId);

            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to load manifest data for webId={webId}, listId={listId}.", ex);
                yield break;
            }

            try
            {
                var page = new List<ManifestDiscoveredItem>(pageSize);
                foreach (var manifestFile in manifestFiles)
                {
                    if (manifestFile is not HsmManifestFile currentFile)
                    {
                        continue;
                    }

                    var currentListItem = currentFile.ListItem;
                    var metadata = ConvertManifestEntry(currentFile, currentListItem, siteId, siteUrl, webId, listId);
                    var roleAssignments = ResolveRoleAssignments(metadata, roleAssignmentsByObjectId);
                    page.Add(new ManifestDiscoveredItem(webId, listId, siteId, siteUrl, currentFile, currentListItem, metadata, manifestDirectory, contentDirectory, contentBlobPrefix, roleAssignments, connectionString, contentConnectionString));

                    if (page.Count == pageSize)
                    {
                        yield return page;
                        page = new List<ManifestDiscoveredItem>(pageSize);
                    }
                }

                if (page.Count > 0)
                {
                    yield return page;
                }
            }
            finally
            {
                CleanupRoleAssignmentsCache(manifestPath);
            }
        }

        private RMDiscoveryFileDataInfo ConvertManifestEntry(HsmManifestFile file, HsmManifestListItem? listItem, string siteId, string siteUrl, Guid webId, Guid listId)
        {
            var uniqueId = file.Id != Guid.Empty
                ? file.Id
                : listItem != null && listItem.DocId != Guid.Empty
                    ? listItem.DocId
                    : listItem != null && listItem.Id.HasValue
                        ? listItem.Id.Value
                        : Guid.NewGuid();

            var name = !string.IsNullOrWhiteSpace(file.Name)
                ? file.Name
                : listItem != null && !string.IsNullOrWhiteSpace(listItem.Name)
                    ? listItem.Name
                    : string.Empty;

            var relativeUrl = listItem != null ? listItem.Url : null;
            if (string.IsNullOrWhiteSpace(relativeUrl) && listItem != null)
            {
                relativeUrl = file.Url ?? listItem.FileUrl ?? listItem.DirName ?? string.Empty;
            }

            var folderRelativeUrl = listItem != null && !string.IsNullOrWhiteSpace(listItem.DirName)
                ? listItem.DirName
                : !string.IsNullOrWhiteSpace(file.Url)
                    ? file.Url
                    : string.Empty;

            var latestFileVersion = file.Versions != null && file.Versions.Count > 0
                ? file.Versions.LastOrDefault()
                : null;

            var created = file.TimeCreated
                ?? listItem?.Created
                ?? latestFileVersion?.Created
                ?? DateTime.UtcNow;

            var modified = file.TimeLastModified
                ?? listItem?.Modified
                ?? latestFileVersion?.Modified
                ?? created;

            var currentVersion = !string.IsNullOrWhiteSpace(file.Version)
                ? file.Version
                : latestFileVersion != null && !string.IsNullOrWhiteSpace(latestFileVersion.Version)
                    ? latestFileVersion.Version
                    : listItem?.Version ?? string.Empty;

            return new RMDiscoveryFileDataInfo
            {
                Id = uniqueId.ToString(),
                Name = name,
                SiteUrl = siteUrl,
                FullUrl = BuildAbsoluteUrl(siteUrl, relativeUrl ?? string.Empty),
                FolderRelativeUrl = folderRelativeUrl,
                SiteId = siteId,
                WebId = webId.ToString(),
                ListId = listId.ToString(),
                FolderId = listItem != null && listItem.ParentFolderId.HasValue ? listItem.ParentFolderId.Value.ToString() : string.Empty,
                ItemId = listItem?.IntId ?? 0,
                ItemUniqueId = uniqueId.ToString(),
                FileExtension = Path.GetExtension(name) ?? string.Empty,
                FileSize = 0,
                CurrentVersion = currentVersion,
                HistoryVersionsCount = CalculateVersionCount(file, listItem),
                HistoryVersionsSize = 0,
                AuthorId = ParseLong(listItem?.Author),
                EditorId = ParseLong(listItem?.ModifiedBy),
                CreatedTime = created,
                ModifiedTime = modified,
                Versions = BuildVersionData(file, listItem),
                Tags = new Dictionary<string, object>()
            };
        }

        private static List<RMDiscoveryFileVersionDataInfo> BuildVersionData(HsmManifestFile? file, HsmManifestListItem? listItem)
        {
            if (file != null && file.Versions != null && file.Versions.Count > 0)
            {
                return file.Versions.Select(v => new RMDiscoveryFileVersionDataInfo
                {
                    Version = v.Version,
                    VersionSize = 0,
                    CreatedTime = v.Created ?? DateTime.UtcNow,
                    ModifiedTime = v.Modified ?? v.Created ?? DateTime.UtcNow,
                    FileValue = v.FileValue
                }).ToList();
            }

            if (listItem != null && listItem.Versions != null && listItem.Versions.Count > 0)
            {
                return listItem.Versions.Select(v => new RMDiscoveryFileVersionDataInfo
                {
                    Version = v.Version,
                    VersionSize = 0,
                    CreatedTime = DateTime.UtcNow,
                    ModifiedTime = DateTime.UtcNow
                }).ToList();
            }

            return new List<RMDiscoveryFileVersionDataInfo>();
        }

        private static IReadOnlyDictionary<string, List<AveRoleAssignmentInfo>> GetRoleAssignmentsByObjectId(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            {
                return new Dictionary<string, List<AveRoleAssignmentInfo>>(0, StringComparer.OrdinalIgnoreCase);
            }

            var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", manifestPath, File.GetLastWriteTimeUtc(manifestPath).Ticks);
            if (roleAssignmentsByManifestPath.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var parsed = LoadRoleAssignmentsByObjectId(manifestPath);
            roleAssignmentsByManifestPath[cacheKey] = parsed;
            return parsed;
        }

        private static void CleanupRoleAssignmentsCache(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            {
                return;
            }

            var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", manifestPath, File.GetLastWriteTimeUtc(manifestPath).Ticks);
            roleAssignmentsByManifestPath.TryRemove(cacheKey, out _);
        }

        private static Dictionary<string, List<AveRoleAssignmentInfo>> LoadRoleAssignmentsByObjectId(string manifestPath)
        {
            var result = new Dictionary<string, List<AveRoleAssignmentInfo>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var settings = new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                    DtdProcessing = DtdProcessing.Ignore,
                    CloseInput = true
                };

                using var reader = XmlReader.Create(manifestPath, settings);
                reader.MoveToContent();

                while (!reader.EOF)
                {
                    if (reader.NodeType != XmlNodeType.Element || !string.Equals(reader.LocalName, "SPObject", StringComparison.Ordinal))
                    {
                        reader.Read();
                        continue;
                    }

                    var objectType = reader.GetAttribute("ObjectType");
                    if (!string.Equals(objectType, "DeploymentRoleAssignments", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Skip();
                        continue;
                    }

                    if (XElement.ReadFrom(reader) is not XElement element)
                    {
                        continue;
                    }

                    var roleAssignmentsElement = element.Element(XName.Get("RoleAssignments", DeploymentManifestNamespace));
                    if (roleAssignmentsElement == null)
                    {
                        continue;
                    }

                    foreach (var roleAssignmentElement in roleAssignmentsElement.Elements(XName.Get("RoleAssignment", DeploymentManifestNamespace)))
                    {
                        var objectId = NormalizeRoleAssignmentKey(roleAssignmentElement.Attribute("ObjectId")?.Value);
                        if (string.IsNullOrWhiteSpace(objectId))
                        {
                            continue;
                        }

                        var assignments = result.TryGetValue(objectId, out var existing)
                            ? existing
                            : (result[objectId] = new List<AveRoleAssignmentInfo>());

                        foreach (var assignmentElement in roleAssignmentElement.Elements(XName.Get("Assignment", DeploymentManifestNamespace)))
                        {
                            var roleIdRaw = assignmentElement.Attribute("RoleId")?.Value;
                            var principalIdRaw = assignmentElement.Attribute("PrincipalId")?.Value;

                            if (!int.TryParse(roleIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var roleId))
                            {
                                continue;
                            }

                            if (!int.TryParse(principalIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var principalId))
                            {
                                continue;
                            }

                            assignments.Add(new AveRoleAssignmentInfo
                            {
                                RoleId = roleId,
                                PrincipalId = principalId,
                                RoleName = string.Empty,
                                MemberLoginName = string.Empty,
                                MemberType = string.Empty
                            });
                        }

                        if (assignments.Count == 0)
                        {
                            result.Remove(objectId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to parse role assignments from manifest {manifestPath}. Error:{ex}");
            }

            return result;
        }

        private static IReadOnlyList<AveRoleAssignmentInfo> ResolveRoleAssignments(RMDiscoveryFileDataInfo metadata, IReadOnlyDictionary<string, List<AveRoleAssignmentInfo>> roleAssignmentsByObjectId)
        {
            if (metadata == null || roleAssignmentsByObjectId == null || roleAssignmentsByObjectId.Count == 0)
            {
                return EmptyRoleAssignments;
            }

            var key = NormalizeRoleAssignmentKey(metadata.ItemUniqueId);
            if (string.IsNullOrWhiteSpace(key))
            {
                return EmptyRoleAssignments;
            }

            return roleAssignmentsByObjectId.TryGetValue(key, out var assignments) && assignments != null && assignments.Count > 0
                ? assignments
                : EmptyRoleAssignments;
        }

        private static string NormalizeRoleAssignmentKey(string? objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId))
            {
                return string.Empty;
            }

            return Guid.TryParse(objectId, out var parsed)
                ? parsed.ToString("D", CultureInfo.InvariantCulture)
                : objectId.Trim();
        }

        private static ManifestEntryAggregate GetOrAddAggregate(Dictionary<string, ManifestEntryAggregate> aggregates, string key)
        {
            if (!aggregates.TryGetValue(key, out var aggregate))
            {
                aggregate = new ManifestEntryAggregate();
                aggregates[key] = aggregate;
            }

            return aggregate;
        }

        private static string BuildManifestAggregateKey(Guid? id, string url)
        {
            if (id.HasValue && id.Value != Guid.Empty)
            {
                return id.Value.ToString("D");
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                return url.Trim().ToLowerInvariant();
            }

            return Guid.NewGuid().ToString("D");
        }

        private static string BuildAbsoluteUrl(string siteUrl, string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
            {
                return siteUrl;
            }

            var normalizedRelative = relativeUrl.Replace('\\', '/');

            if (normalizedRelative.StartsWith("//", StringComparison.Ordinal))
            {
                normalizedRelative = "/" + normalizedRelative.TrimStart('/');
            }

            var isAbsoluteHttpUrl = normalizedRelative.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || normalizedRelative.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (isAbsoluteHttpUrl && Uri.TryCreate(normalizedRelative, UriKind.Absolute, out var absolute))
            {
                if (string.Equals(absolute.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(absolute.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    return absolute.OriginalString;
                }

                if (!string.IsNullOrWhiteSpace(absolute.AbsolutePath)
                    && absolute.AbsolutePath.StartsWith("/", StringComparison.Ordinal))
                {
                    normalizedRelative = absolute.AbsolutePath;
                }
            }

            if (!Uri.TryCreate(siteUrl, UriKind.Absolute, out var siteUri))
            {
                return normalizedRelative;
            }

            var serverRelative = normalizedRelative.StartsWith("/", StringComparison.Ordinal)
                ? normalizedRelative
                : "/" + normalizedRelative.TrimStart('/');

            return string.Concat(siteUri.GetLeftPart(UriPartial.Authority), serverRelative);
        }

        private static int CalculateVersionCount(HsmManifestFile? file, HsmManifestListItem? listItem)
        {
            if (file != null && file.Versions != null)
            {
                return file.Versions.Count;
            }

            if (listItem != null && listItem.Versions != null)
            {
                return listItem.Versions.Count;
            }

            return 0;
        }

        private static long ParseLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        }

        private sealed class ManifestEntryAggregate
        {
            public HsmManifestFile? File { get; set; }
            public HsmManifestListItem? ListItem { get; set; }
        }

        private sealed class ManifestDiscoveredItem
        {
            public ManifestDiscoveredItem(
                Guid webId,
                Guid listId,
                string siteId,
                string siteUrl,
                HsmManifestFile? file,
                HsmManifestListItem? listItem,
                RMDiscoveryFileDataInfo metadata,
                string manifestDirectoryPath,
                string contentDirectoryPath,
                string? contentBlobPrefix,
                IReadOnlyList<AveRoleAssignmentInfo> roleAssignments,
                string? connectionString,
                string? contentConnectionString)
            {
                WebId = webId;
                ListId = listId;
                SiteId = siteId;
                SiteUrl = siteUrl;
                File = file;
                ListItem = listItem;
                Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
                ManifestDirectoryPath = manifestDirectoryPath ?? string.Empty;
                ContentDirectoryPath = contentDirectoryPath ?? string.Empty;
                ContentBlobPrefix = contentBlobPrefix ?? string.Empty;
                StorageContainerName = string.Empty;
                StorageConnectionString = connectionString ?? string.Empty;
                ContentStorageConnectionString = contentConnectionString ?? string.Empty;
                RoleAssignments = roleAssignments?.ToList() ?? new List<AveRoleAssignmentInfo>();
            }

            public Guid WebId { get; }
            public Guid ListId { get; }
            public string SiteId { get; }
            public string SiteUrl { get; }
            public HsmManifestFile? File { get; }
            public HsmManifestListItem? ListItem { get; }
            public RMDiscoveryFileDataInfo Metadata { get; }
            public string ManifestDirectoryPath { get; }
            public string ContentDirectoryPath { get; }
            public string ContentBlobPrefix { get; }
            public string StorageContainerName { get; }
            public string StorageConnectionString { get; }
            public string ContentStorageConnectionString { get; }
            public IReadOnlyList<AveRoleAssignmentInfo> RoleAssignments { get; }
        }

#nullable restore

        private bool CheckIsDesignList(string listInfo)
        {
            return false;
        }

        private string CombineListUrlAndTemplate(AveDiscoverList discoverList)
        {
            string combineUrlTemplate = string.Empty;
            string listUrl = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(discoverList.RootFolderUrl))
                {
                    int listUrlIndex = discoverList.RootFolderUrl.LastIndexOf("/");
                    //Root Site RootFolderURL like /SitePages
                    if (listUrlIndex >= 0)
                    {
                        listUrl = discoverList.RootFolderUrl.Substring(listUrlIndex + 1);
                    }
                    else
                    {
                        listUrl = discoverList.Name;
                    }
                    combineUrlTemplate = listUrl + discoverList.ListTemplate.ToString();
                    mLog.Info($"CombineListUrlAndTemplate combineUrlTemplate is {combineUrlTemplate}.discoverList.RootFolderUrl:{discoverList.RootFolderUrl}.");
                }
                else
                {
                    mLog.Info("CombineListUrlAndTemplate discoverList.RootFolderUrl is IsNullOrEmpty.");
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"CombineListUrlAndTemplate error: ({ex})");
                combineUrlTemplate = string.Empty;
            }
            return combineUrlTemplate;
        }

        private List<string> GetDesignLists()
        {
            return WebUtil.GetDesignLists(TenantService.IsCSDTenant());
        }

        internal async System.Threading.Tasks.Task ProcessWebCollectionAsync(ArchiverNodeItem web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWebCollection"))
            {
                Dictionary<Guid, AveDiscoverWeb> discoverWebs = new Dictionary<Guid, AveDiscoverWeb>();
                if (mConfiguration.Procedure == ScheduleProcedure.Scan)
                {
                    discoverWebs = ((AveDiscoverWeb)web.DiscoverSPObject).GetSubWebs(true);
                }
                else
                {
                    discoverWebs = ((AveDiscoverWeb)web.DiscoverSPObject).GetSubWebs();
                }
                foreach (AveDiscoverWeb tmp in discoverWebs.Values)
                {
                    using (new CheckJobStopScope()) { }
                    try
                    {
                        using (ArchiverNodeItem webnode = web.GenerateSiteNodeItem(tmp, mConfiguration, web.Parent.SPNodeLevel == NodeLevel.SiteCollection))
                        {

                            using (IAveWeb iweb = tmp.AveWeb)
                            {
                                try
                                {
                                    //SAAS-20894 在Get Web Properties中获取，这里不需要判断了。
                                    //else if (string.Equals(iweb.WebTemplate, "CMSPUBLISHING") || string.Equals(iweb.WebTemplate, "BLANKINTERNET") || string.Equals(iweb.WebTemplate, "ENTERWIKI"))
                                    //{
                                    //    //SAAS-11588 添加判断条件判断需要执行此步骤的webtemplate(通过调用SiteLogoUrl自动创建出Site Assets List)
                                    //    string subSiteLogoUrl = iweb.SiteLogoUrl;
                                    //}
                                    string subSiteLogoDescription = iweb.SiteLogoDescription;//通过调用SiteLogoDescription自动创建出Site Assets List
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("Get Web Properties Error{0},WebName is {1}", e.ToString(), web.Name);
                                }

                            }
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
                        { }
                        ;
                    }
                }
            }
        }


        private async System.Threading.Tasks.Task<ArchiverNodeItem> InitNodeEntityRelatedInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node, bool autoApproval, bool firstCall = false)
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
                                if (mConfiguration.IsILMode && (discoverWorker is RecordsOneDriveScanDiscovrerNodeWorker))
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
                            string tempFolderPath = "/" + node.FullPath.TrimStart('/').TrimEnd('/');
                            string parentFolderPath = AveUrlUtility.GetParentUrl(tempFolderPath);
                            mLog.Info("InitNodeEntityRelatedInfo parentFolderPath:{0}.", parentFolderPath);
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
                                AveDiscoverFolder discoverFolde = ((AveDiscoverFolder)parent.DiscoverSPObject).GetSubFolders().FirstOrDefault<AveDiscoverFolder>(tmp => tmp.DocID.Equals(tempNodeId));
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

        /// <summary>
        /// Process web content in iteration for initialization
        /// </summary>
        /// <param name="web"></param>
        private int CaculateWebListCount(ArchiverNodeItem web, bool needInitInfo, AveDiscoverWeb discoverWeb)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.CaculateWebListCount"))
            {
                int result = 0;
                try
                {
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
                                if (IsWebBreakInherit(web, subWeb))
                                {
                                    continue;
                                }
                                discoverLists = subWeb.GetLists();
                                result += discoverLists.Count;
                                //处理SubSite下面的Subsite
                                CaculateWebListCount(web, needInitInfo, subWeb);
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
                                try
                                {
                                    if (IsWebBreakInherit(web, subWeb))
                                    {
                                        continue;
                                    }
                                    discoverLists = subWeb.GetLists();
                                    result += discoverLists.Count;
                                    CaculateWebListCount(web, needInitInfo, subWeb);
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

        protected bool IsWebBreakInherit(ArchiverNodeItem web, AveDiscoverWeb subWeb)
        {
            bool skipCheckBreakInherit = false;
            if (web != null && !string.IsNullOrEmpty(web.RuleId) && web.DoDelete)
            {
                skipCheckBreakInherit = true;
            }
            if (!mConfiguration.UseArchiverImportFile && !skipCheckBreakInherit && !string.IsNullOrEmpty(web.SiteUrl) && (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(web.SiteUrl, subWeb.FullUrl))))
            {
                return true;
            }
            return false;
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
        public void Dispose()
        {
            discoverWorker.Dispose();
            SPORootFolder?.Dispose();
        }

        public bool CheckSiteCollectionIsHold()
        {
            throw new NotImplementedException();
        }
    }
}
