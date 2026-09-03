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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.Archiver.Common;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using AvePoint.Wrapper.Discovery;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using SPDisposeCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.Archiver
{
    public abstract class SharePointCGScannerBase : ISharePointScanner
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SharePointCGScannerBase));
        private Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly object locker = new();
        private IScanDataReader mScanDataReader = null;
        private long totalScanCount = 0;
        public bool siteStorageSizeLimit;
        internal AveDiscoverSite mDiscoverSite = null;
        internal IBackwardDependencyNodeCache<object> mDependencyObjs;
        internal ScanJobSettings jobSettings = null;
        internal ScheduleConfiguration mConfiguration = null;
        internal Guid scopeId = Guid.Empty;
        internal Guid groupId = Guid.Empty;
        internal AveObjectModelFactory mFactory = null;
        public List<Guid> listIds = new List<Guid>();
        public List<DBFileInfo> fileInfos = new List<DBFileInfo>();
        public List<Guid> dbWebList = new List<Guid>();
        private IAveList mSPQueryList = null;
        private int mMaxItemIdInLibrary = 0;
        private SPOFolder SPORootFolder = null;
        public const string SP_ID = "ID";
        public const string SP_UniqueID = "UniqueId";
        private CAMLManager mCAMLManager = null;
        private string mWebAppName = string.Empty;
        private string mSiteUrl = string.Empty;
        public AveDiscoverFolder mInitNodeEntityRelatedInfoDiscoverRootFolder = null;
        public Guid mInitNodeEntityRelatedInfoRootFolderId = Guid.Empty;
        public PCContainer<ArchiveApproveReport> pcContainer = new PCContainer<ArchiveApproveReport>(1000);
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

        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();

        public abstract IDiscoverNodeWorker discoverWorker
        {
            get;
            set;
        }
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
        public SharePointCGScannerBase(ScanJobSettings scanJobSettings)
        {
            mDependencyObjs = new BackwardDependenceNodeCache<object>();
            jobSettings = scanJobSettings;
            mConfiguration = scanJobSettings.Configuration;
            mScanDataReader = new CGDBScanDataReader(mConfiguration);
            mSiteUrl = mConfiguration.SiteCollectionUrl;
        }
        public void RealRun()
        {
            try
            {
                RunCGAsync().GetAwaiter().GetResult();
            }
            catch (AveExceedStorageLimitException e)
            {
                siteStorageSizeLimit = true;
                mConfiguration.JobReportDto.AddScanReport(mSiteUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_JM_SiteStorageLimit_ErrorMessage");
                mLog.Error($"AveExceedStorageLimitException some thing went wrong when RunCGArchiver ,error :{e.ToString()}");
            }
            catch (Exception e)
            {
                mLog.Error($"some thing went wrong when RunCGArchiver ,error :{e.ToString()}");
            }
            
        }

        public bool CheckSiteCollectionIsHold()
        {
            var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings.TreeNode);
            RuleNodeContract nodeContract = new RuleNodeContract();
            AssignSPObjectId(node, ref nodeContract);
            AveObjectModelFactory factory = mConfiguration.aveObjectModelFactory;
            IAveSite aveSite = factory.CreateSite(nodeContract.SiteUrl);
            if (aveSite == null)
            {
                throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanCaculateSiteListCount, "SiteCollection", nodeContract.SiteUrl);
            }
            if (aveSite.HasHolds)
            {
                mLog.Warn($"Site collection {nodeContract.SiteUrl} is on hold, skip it.");
                if (RMKeyValueDao.EnableArchiveHoldSiteCollection())
                {
                    mLog.Warn($"Archive hold site collection is enabled, will not skip site collection.");
                }
                else
                {
                    mLog.Warn($"Archive hold site collection is un enabled, skip site collection.");
                    return true;
                }

            }
            return false;
        }

        public async System.Threading.Tasks.Task RunCGAsync()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SharePointScanner.Run"))
            {
                try
                {
                    if (mConfiguration.IsRelativeDataJob)
                    {
                        await RunRelativeDataScanAsync();
                        return;
                    }
                    if (ArchiveJobLimitCollection.ShouldCheckSiteHoldJobTypeSet.Contains(mConfiguration.jobtype) && CheckSiteCollectionIsHold())
                    {
                        mLog.Info($"site was hold");
                        if (mConfiguration.RuleCollection.Any(rule => RuleHelper.CheckIsWillDeleteDataAction(rule.Value)))
                        {
                            mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Skipped, "RM_JM_SiteCollectionHoldAndHaveDeletRule_ErrorMessage");
                            return;
                        }
                    }
                    var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings.TreeNode);
                    scopeId = Guid.Parse(node.SPObjectId);
                    groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(node).SPObjectId);
                    var ruleNode = ConvertTreeNodeToRuleNodeConfig(node, RuleNodeType.Archiver);
                    discoverWorker.Init(ruleNode);
                    ArchiverNodeItem selectNodeItem = new ArchiverNodeItem(ruleNode);
                    JobExecutionProcessStatisticExecutor.Instance.StartCalculateRuleAndSummary(selectNodeItem.SPNodeLevel.ToString(), selectNodeItem.FullPath);
                    try
                    {
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
                catch (AveExceedStorageLimitException e)
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
                    pcContainer.EndProduce();
                    JobExecutionProcessStatisticExecutor.Instance.EndCalculateRuleAndScanSummary(totalScanCount, Site);
                }
            }
        }

        public async System.Threading.Tasks.Task RunAsync()
        {
            var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings.TreeNode);
            CheckSCLevelRuleUseByUnOphenNodeInODSource(node);


        }

        private void CheckSCLevelRuleUseByUnOphenNodeInODSource(SPTreeNodeDto node)
        {
            if ((node.Type == GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup || node.Type == GCommon.Contract.Tree.Object.NodeType.SkyDriveProSites) &&
                        mConfiguration.RuleCollection.Count(ruleEntity => ruleEntity.Value.PolicyLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection) > 0)
            {
                RemoteSiteCollection remoteNode = RemoteNodeDao.GetRemoteSiteCollectionById(jobSettings.TreeNode.Id);
                if (remoteNode != null && remoteNode.Name != null && remoteNode.NodeType == RemoveNodeType.SkyDrivePro)
                {
                    mLog.Info("OD source, only orphen onedrive can use site collection level rule, will remove site collection level rule");
                    mConfiguration.RuleCollection = mConfiguration.RuleCollection.Where(
                        ruleEntity => ruleEntity.Value.PolicyLevel != GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection).ToDictionary();
                }
            }
        }

        public async System.Threading.Tasks.Task RunRelativeDataScanAsync()
        {

            //AveLogger.SetThreadJobId(mConfiguration.JobId, bool.Parse(SOGlobalProperty.Configuration[SOCommonObjects.ConfigurationOption.SeparateLogFileForEachJob]));
            RelativeDataArchiveObject relativeDataArchiveObject = null;
            if (!string.IsNullOrEmpty(mConfiguration.relativeDataTreeNodeString))
            {
                relativeDataArchiveObject = new RelativeDataArchiveObject(mConfiguration.relativeDataTreeNodeString);
                JobExecutionProcessStatisticExecutor.Instance.StartCalculateRuleAndSummary(relativeDataArchiveObject.mCurrentlevel, relativeDataArchiveObject.mSORelativeDataRequest.Path);
                bool exist = false;
                IAveListItem checkDeclareItem = null;
                try
                {
                    //获取正确的DocLibRowId，由于RA Related Document 可以备份还原，可能出现很多ID 不准确的case，所以此处添加重新获取的逻辑
                    var site = mConfiguration.aveObjectModelFactory.CreateSite(relativeDataArchiveObject.mSORelativeDataRequest.SiteCollectionUrl);
                    relativeDataArchiveObject.mSORelativeDataRequest.SiteCollectionId = site.ID.ToString();
                    var web = site.OpenWeb(relativeDataArchiveObject.mSORelativeDataRequest.WebServerRelatedUrl);
                    if (web.Exists)
                    {
                        relativeDataArchiveObject.mSORelativeDataRequest.WebId = web.ID.ToString();
                        if (relativeDataArchiveObject.mSORelativeDataRequest.CurrentLevel.Equals(SORelativeDataArchiverNodeLevel.Document.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var file = web.GetFile(relativeDataArchiveObject.mSORelativeDataRequest.Path);
                            if (file.Exists)
                            {
                                exist = true;
                                //Update container level id for the backup/restore case
                                relativeDataArchiveObject.mSORelativeDataRequest.ListId = file.ParentFolder.ParentListId.ToString();
                                relativeDataArchiveObject.mSORelativeDataRequest.FolderId = file.ParentFolder.UniqueId.ToString();
                                relativeDataArchiveObject.mSORelativeDataRequest.ItemId = file.UniqueId.ToString();
                                relativeDataArchiveObject.mSORelativeDataRequest.DocLibRowId = file.Item.ID;
                                checkDeclareItem = file.Item;
                            }
                            else
                            {
                                mLog.Error(string.Format("Cannot get file by url : {0}.", relativeDataArchiveObject.mSORelativeDataRequest.Path));
                            }
                        }
                        else if (relativeDataArchiveObject.mSORelativeDataRequest.CurrentLevel.Equals(SORelativeDataArchiverNodeLevel.Item.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            IAveListItem listItem = null;
                            try
                            {
                                listItem = web.GetListItem(relativeDataArchiveObject.mSORelativeDataRequest.Path, new Guid(relativeDataArchiveObject.mSORelativeDataRequest.ListId), relativeDataArchiveObject.mSORelativeDataRequest.DocLibRowId);
                            }
                            catch
                            {
                                listItem = web.GetListItem(relativeDataArchiveObject.mSORelativeDataRequest.Path, new Guid(relativeDataArchiveObject.mSORelativeDataRequest.ListId), new Guid(relativeDataArchiveObject.mSORelativeDataRequest.ItemId));
                            }
                            exist = listItem != null;
                            checkDeclareItem = listItem;
                        }
                    }
                    else
                    {
                        mLog.Warn(string.Format("Cannot get web by url : {0}", relativeDataArchiveObject.mSORelativeDataRequest.WebServerRelatedUrl));
                    }
                }
                catch (Exception exception)
                {
                    mLog.Warn(string.Format("Cannot get the ids, reason: {0}.", exception.ToString()));
                }
                if (!exist)
                {
                    mLog.Warn("Item: {0} does not exist in SharePoint.", relativeDataArchiveObject.mSORelativeDataRequest.Path);
                    //Add skip detail for end user job
                    //JobDetail backupDetail = new JobDetail() { SubJobId = mConfiguration.JobId, Type = SPNodeLevel.Item.ToString(), SrcURL = relativeDataArchiveObject.mSORelativeDataRequest.Path, Size = 0, Status = (int)BackupRestoreStatus.Skipped, Remark12 = "Backup", };
                    //mConfiguration.relativeDataJobReportOperation.AddDetail(backupDetail);
                    //JobDetail deleteDetail = new JobDetail() { SubJobId = mConfiguration.JobId, Type = SPNodeLevel.Item.ToString(), SrcURL = relativeDataArchiveObject.mSORelativeDataRequest.Path, Size = 0, Status = (int)BackupRestoreStatus.Skipped, Remark12 = "Delete", };
                    //mConfiguration.relativeDataJobReportOperation.AddDetail(deleteDetail);
                    //mPcContainer.StartProduce();
                    //mPcContainer.EndProduce();
                    return;
                }
                else if (mConfiguration.currentRule != null && !mConfiguration.currentRule.DeleteRecords 
                    && !RuleHelper.CheckArchiveOnlyRule(mConfiguration.currentRule) && ArchiverCommonStaticMethod.CheckisRecord(checkDeclareItem))
                {
                    mLog.Warn("Item: {0} is Declare Status.", relativeDataArchiveObject.mSORelativeDataRequest.Path);
                    AddRelativeDataDetail(0, checkDeclareItem?.Name, checkDeclareItem?.FullPath(), 10000, JobDetailsStatus.Skipped, "RM_SS_ItemBlockEditAndDelete");
                    //throw new Exception("RM_SS_ItemBlockEditAndDelete");

                    //Add skip detail for end user job
                    //JobDetail backupDetail = new JobDetail() { SubJobId = mConfiguration.JobId, Type = SPNodeLevel.Item.ToString(), SrcURL = relativeDataArchiveObject.mSORelativeDataRequest.Path, Size = 0, Status = (int)BackupRestoreStatus.Skipped, Remark12 = "Backup", };
                    //mConfiguration.relativeDataJobReportOperation.AddDetail(backupDetail);
                    //JobDetail deleteDetail = new JobDetail() { SubJobId = mConfiguration.JobId, Type = SPNodeLevel.Item.ToString(), SrcURL = relativeDataArchiveObject.mSORelativeDataRequest.Path, Size = 0, Status = (int)BackupRestoreStatus.Skipped, Remark12 = "Delete", };
                    //mConfiguration.relativeDataJobReportOperation.AddDetail(deleteDetail);
                    //mPcContainer.StartProduce();
                    //mPcContainer.EndProduce();
                    return;
                }
                relativeDataArchiveObject.InitDiscoverObject();

                List<TagInfoCollection> tagInfo = relativeDataArchiveObject.TagValue();
                if (tagInfo != null)
                {
                    mConfiguration.tagInfoCollection.AddRange(tagInfo);
                }
            }
            try
            {
                // mPcContainer.StartProduce();
                if(relativeDataArchiveObject == null)
                {
                    mLog.Error("End user tree node string is null or empty cannot get end user archive object");
                    throw new Exception("End user tree node string is null or empty cannot get end user archive object");
                }
                ArchiveApproveReport report = relativeDataArchiveObject.Approve;
                using (IBackwardDependencyNodeCache<ArchiveApproveReport> mBackupNodeCache = new BackwardDependenceNodeCache<ArchiveApproveReport>(new ApprovalReportService(mConfiguration)))
                {
                    BackwardDependenceNodeCache<object> dependencyObjs = new BackwardDependenceNodeCache<object>();
                    using (RelativeDataBackupDiscoverNodeWork worker = new RelativeDataBackupDiscoverNodeWork(mBackupNodeCache, dependencyObjs, mConfiguration))
                    {
                        //如果传递过来的是Folder，并且有Item ID 的集合，表示选择Ribbon 按钮操作，并且选择的是Folder 下的数据。这时候，符合Rule 的CacheNodeType 必定比传递过来的Folder 小，即使是下层Folder，也最起码要 +1.
                        if ((NodeLevel)report.SPNodeLevel == NodeLevel.Folder && report.ItemIDs != null && report.ItemIDs.Count > 0)
                        {
                            worker.DiscoverCacheNodeType = report.CacheNodeType + 1;
                        }
                        else
                        {
                            worker.DiscoverCacheNodeType = report.CacheNodeType;
                        }
                        //using (ScanJob scanJob = new ScanJob(dependencyObjs, mConfiguration))
                        {
                            ArchiverNodeItem item = new ArchiverNodeItem(report) { WebApplicationId = relativeDataArchiveObject.mWebappId, WebApplicationUrl = relativeDataArchiveObject.mWebappUrl, SiteId = relativeDataArchiveObject.mSiteId, WebId = relativeDataArchiveObject.mWebId, ListId = relativeDataArchiveObject.mListId, SiteUrl = relativeDataArchiveObject.mSiteUrl, FolderId = relativeDataArchiveObject.mFolderId, ItemId = relativeDataArchiveObject.mItemId };
                            //if (item.SPNodeLevel == NodeLevel.SiteCollection || item.SPNodeLevel == NodeLevel.Site)
                            //{
                            //    mConfiguration.ProgressDto.TotalCount = scanJob.CaculateListCount(item);
                            //}
                            //else
                            //{
                            //    //在List level以下的level，Total Count均赋值为4。原因是backup sitecollection、web、list、item节点的时候，都会给current count加1，
                            //    //所以当current为4时，能够代表已经备份到了item级别，而item最为最后一个备份的level，也能够表示job即将结束。
                            //    mConfiguration.ProgressDto.TotalCount = 4;
                            //}
                            item.ShouldDoArchive = true;
                            item.IsRootFolder = relativeDataArchiveObject.mIsRootFolder;
                            //scanJob.Scan(item, worker);
                            await ProcessItemAsync(item, worker, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARCLReaderDiscoverNode, ex.ToString());
                mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARCLReaderDiscoverError;
                mConfiguration.ProgressDto.HasErrorNode = true;
            }
            finally
            {
                //mPcContainer.EndProduce();
            }
            //JobExecutionProcessStatisticExecutor.Instance.EndCalculateRuleAndScanSummary(totalScanCount, Site);
        }

        public virtual List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId = "")
        {
            return ArchiverSettingDao.LoadBreakInheritNodeUrls(scopeUrl, siteObjectId, mConfiguration.IsTeams);
        }

        private void AddRelativeDataDetail(long size, string name, string fullPath, int type, JobDetailsStatus status, string comment)
        {
            if (mConfiguration.RelativeDataJobSourceFlag == (int)SourceFlag.Physical)
            {
                //RM_JS_JM_Related_DeleteRelatedFailed                
                SendPhysicalJobDetail(name, fullPath, PhysicalDisposalActionType.Disposal, String.Empty, ArchiverTypeConvert.ConvertNodeLevelToI18n(type), status, comment);
            }
            else
            {
                SendSPJobDetail(size, fullPath, type, status, comment);
            }
        }

        public void SendPhysicalJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                ActionType = GetI18NActionType(action),
                DestinationPath = destinationPath,
                ItemType = ItemType,
                Status = status,
                Comment = comment
            });
        }

        private string GetI18NActionType(PhysicalDisposalActionType action)
        {
            string result = string.Empty;
            switch (action)
            {
                case PhysicalDisposalActionType.Pending:
                    result = "RM_JMD_PD_DisposalAction_Pending";
                    break;
                case PhysicalDisposalActionType.Disposal:
                    result = "RM_JMD_PD_DisposalAction_Dispose";
                    break;
                case PhysicalDisposalActionType.Move:
                    result = "RM_JMD_PD_DisposalAction_Move";
                    break;
                default:
                    result = action.ToString();
                    break;
            }
            return result;
        }
        public void SendSPJobDetail(long nodeSize, string originPath, int cacheNodeType, JobDetailsStatus status, string comment = "")
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = originPath;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = mConfiguration.currentRule.Name;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Backup;
            //mArchiverActionJobDetails.Action = "Delete";
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = comment;
            JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(mConfiguration.currentRule, nodeSize, cacheNodeType, status);
            ReportManager.SendJobDetail(mArchiverActionJobDetails);
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
                                if (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(sitecollection.FullPath)))
                                {
                                    var setting = ArchiverSettingDao.LoadArchiverSetting(rootWeb.WebID, sitecollection.ID);
                                    if (setting != null)
                                    {
                                        mLog.Warn("root web {0} is break inherit from parent", rootWeb.FullUrl);
                                        return;
                                    }
                                }
                                using (ArchiverNodeItem webnode = sitecollection.GenerateSiteNodeItem(rootWeb, mConfiguration, true))
                                {
                                    dbWebList.Clear();
                                    string siteIDString = mConfiguration.SiteCollectionID.ToString();
                                    string siteUrlString = mConfiguration.SiteCollectionUrl;
                                    dbWebList = CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).GetWebIds(mConfiguration.SiteCollectionID, mConfiguration.currentRule.Order);
                                    string rootWebSiteLogoDescription = rootWeb.AveWeb.SiteLogoDescription;//通过调用SiteLogoDescription自动创建出Site Assets List 
                                    await ProcessWebAsync(webnode);
                                }
                            }
                        }

                    }
                    catch (AveExceedStorageLimitException e)
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
                    finally
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.SiteCollection, 0);
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (AveExceedStorageLimitException e)
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
                    listIds.Clear();
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
                        //TODO:Disable language mapping
                        //ScheduleLanguageMapping.ProcessLanguageMapping(tmpWeb);
                        mDependencyObjs.PutIn(tmpWeb, (int)CacheNodeType.Web, false);
                    }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(web, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//web 级别 符合 web rule
                    {
                        return;
                    }
                    if (dbWebList.Contains(web.WebId))
                    {
                        string siteIDString = mConfiguration.SiteCollectionID.ToString();
                        string siteUrlString = mConfiguration.SiteCollectionUrl;
                        listIds = CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).GetListIds(new Guid(siteIDString), web.WebId, mConfiguration.currentRule.Order);
                        //check if the webid in the db web cache.
                        if (listIds != null && listIds.Count > 0)
                        {
                            await ProcessListCollectionAsync(web);
                        }
                    }
                    //Process web
                    await ProcessWebCollectionAsync(web);
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
                finally
                {
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Web, 0);
                }
            }
        }

        /// <summary>
        /// Process all the items under list for initialization
        /// </summary>
        /// <param name="list"></param>
        public virtual async System.Threading.Tasks.Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, list);
                        OutPutListItemCount(new()
                        {
                            { list.ListId, list.DiscoverSPObject as AveDiscoverList }
                        });
                    }

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
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles((list.DiscoverSPObject as AveDiscoverList).ItemCount);
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.List, 0);
                    }
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
            catch(Exception e)
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
                    if(failedItemsId == null)
                    {
                        mLog.Info($"Folder:{folder?.Name} items id is null");
                        continue;
                    }
                    foreach(int id in failedItemsId)
                    {
                        try
                        {
                            var info = fileInfos.Where(f => f.ID.Equals(id)).FirstOrDefault();
                            if (info != null)
                            {
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                string siteIDString = mConfiguration.SiteCollectionID.ToString();
                                string siteUrlString = mConfiguration.SiteCollectionUrl;
                                mLog.Error($"Cannot find ID:{info.itemId}.Name:{info.fullPath} when AnalyzeFolderStructureV3.");
                                CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).UpdateStatus(siteIDString, info.itemId, BackupRestoreStatus.Failed);
                                mConfiguration.JobReportDto.AddReport(info.url, 0, JobDetailsStatus.Exception, (int)CacheNodeType.Item, mConfiguration.JobId, "", "", "StorageOptimization_SOARRecordManagerFileNotExist");
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Cannot add to report id {id} error {e}");
                        }
                    }
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
                if(ids != null)
                {
                    foreach(int id in ids)
                    {
                        yield return id;
                    }
                }
            }

            if (folder.SubFolders != null)
            {
                foreach (var subfolder in folder.SubFolders)
                {
                    foreach(int id in AssignFolderId(subfolder, currentFolderServerRelativePath, realFolders))
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
                        foreach(int id in GetItemRowIdUnderFolder(subFolder))
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
                int fileInfoCount = fileInfos.Count();
                SPORootFolder?.Dispose();
                SPORootFolder = SPOFolder.BuildRootFolder(new CacheDBOperator<SPOItem>(), new CacheDBOperator<SPOFolder>(), rootFolderServerRelativeUrl);
                try
                {
                    if (mMaxItemIdInLibrary > 0)
                    {
                        mLog.Info($"Start to query InitArchiverSPQueryRootFolder in :{rootFolderServerRelativeUrl}.");
                        //totaltemsCount = totaltemsCount + items.Count;
                        mLog.Info("InitArchiverSPQueryRootFolder.");
                        AnalyzeListItems(fileInfos, SPORootFolder);
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
        void AnalyzeListItems(List<DBFileInfo> items, SPOFolder rootFolder)
        {
            foreach (var item in items)
            {
                string decodeFullPath = Uri.UnescapeDataString(item.fullPath);
                int index = decodeFullPath.LastIndexOf('/');
                if (index <= 0)
                { continue; }

                var serverRelativeUrl = decodeFullPath.Substring(WebAppName.Length);
                var name = decodeFullPath.Substring(index + 1);
                mLog.Info($"AnalyzeListItems. DBFileInfo Id:{item.ID}.itemId:{item.itemId}.listId:{item.listId}.webId:{item.webId}.ItemParentPath:{decodeFullPath.Substring(0, index)}.");
                var parentFolder = rootFolder;
                var frUrl = serverRelativeUrl.Substring(rootFolder.Name.Length, serverRelativeUrl.Length - rootFolder.Name.Length - name.Length - 1);
                var parentFoldersName = frUrl.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parentFoldersName.Length; i++)
                {
                    var folderName = parentFoldersName[i];
                    SPOFolder tempFolder = parentFolder.SubFolders.GetByName(folderName);

                    if (tempFolder == null)
                    {
                        tempFolder = SPOFolder.BuildUnRootFolder(parentFolder, folderName, -1);
                        parentFolder.SubFolders.Add(tempFolder);
                    }
                    parentFolder = tempFolder;
                }

                var id = item.ID;
                if (item.Size > 0 || item.StorageSize > 0)
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
                    mLog.Info($"The Object {item.fullPath} size less 0 and update CGDBStatus .");
                    string siteIDString = mConfiguration.SiteCollectionID.ToString();
                    string siteUrlString = mConfiguration.SiteCollectionUrl;
                    CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).UpdateStatus(siteIDString, item.itemId, BackupRestoreStatus.Skipped);
                }
            }
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
        public async virtual System.Threading.Tasks.Task ProcessFolderAsync(ArchiverNodeItem folder, bool needInitInfo = false, List<int> itemIDs = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessFolder"))
            {
                try
                {
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
                    await InitialSPObjectInfoAsync(discoverWorker,folderItem);
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

        public async virtual System.Threading.Tasks.Task ProcessItemsAndSubfoldersAsync(ArchiverNodeItem folderNode, int folderLevel, List<int> itemIDs = null, bool needInitInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RealProcessItemsAndSubfolders"))
            {
                AveDiscoverFolder rootFolder = (folderNode.DiscoverSPObject as AveDiscoverFolder);
                #region process items/documents
                int totalItemCount = rootFolder.GetItemCount();
                try
                {
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseTotalFiles(totalItemCount);
                    }
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
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.", folderNode.FullPath, ex.ToString());
                }
                finally
                {
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Folder, 0);
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles(totalItemCount);
                    }
                }
                #endregion

                #region process folders
                try
                {
                    foreach (var folders in rootFolder.GetFoldersWithStructure(true))
                    {
                        mLog.Info("Curent GetFoldersWithStructure folders Count:{0}.", folders.Count);
                        var folderIds = folders.Where(x => x.ID != null).Count() != 0 ? folders.Where(x => x.ID != null).Select(x => x.ID.Value).ToList() : new List<int>();
                        await ProcessDataAsync(folders, itemIDs, folderNode, discoverWorker, needInitInfo);
                        rootFolder.ClearSubFoldersCache();
                        //Remove IAveFolder Cache.每次Query出的Folder外围处理结束后，清除当次Query缓存的IAveFolder，避免造成内存问题.
                        mLog.Info("Begin remove folder cache GetFoldersWithStructurForArchiver.RemomveCount:{0}.FullPath:{1}.", folderIds.Count, folderNode.FullPath);
                        rootFolder.RemoveFolderCache(folderIds);
                    }
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
                        ProcessResult result = await discoverWork.ProcessContainerAsync(node.GenerateWebappNodeItem(), ProcessType.NeedProcess);
                    }
                    return;
                }
                JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.SiteCollection, 0);
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
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Web, 0);
                }
                if (node.SPNodeLevel > NodeLevel.Site && web != null)
                {
                    list = web.GetList(node.FullPath);
                    mLog.Info("Current list [{0}] ItemCount [{1}].", node.FullPath, list.ItemCount);
                    mDependencyObjs.PutIn(list, (int)CacheNodeType.List, false);
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.List, 0);
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

        internal async System.Threading.Tasks.Task ProcessDataAsync(List<AveDiscoverItem> items, List<int> itemIDs, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
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
                    catch (Exception exc)
                    {
                        mLog.Error(string.Format("Error in Backup Single Item :{0}.ItemFullPath:{1}.", exc.ToString(), itemFullUrl));
                    }
                    item.Dispose();
                }
            }
        }

        internal async virtual System.Threading.Tasks.Task ProcessVersionAndAttachmentsAsync(AveDiscoverItem item, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersionAndAttachments"))
            {
                using (ArchiverNodeItem itemNode = folderNode.GenerateItemNodeItem(item, rootFolder, mConfiguration))
                {
                    ProcessResult result = await discoverWorker.ProcessItemAsync(itemNode, folderNode);
                    if (result == ProcessResult.CurrentVersionHasApprove)
                    {
                        return;
                    }

                    Stopwatch watch = Stopwatch.StartNew();
                    //Progress attachments 
                    if (item.GetAttachments().Count > 0)
                    {
                        foreach (AveItemObject attachment in item.GetAttachments())
                        {
                            await ProcessAttachmentsAsync(folderNode, itemNode, attachment, discoverWorker);
                        }
                    }
                    //Progress item versions
                    if (item.GetVersions().Count > 1)
                    {
                        foreach (AveVersionObject version in item.GetVersions())
                        {
                            if ((version.Uiversion == item.Uiversion) || (version.Uiversion == 0))
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

        internal async virtual System.Threading.Tasks.Task ProcessVersionsAsync(ArchiverNodeItem item, AveVersionObject version, ArchiverNodeItem folder, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersions"))
            {
                ArchiverNodeItem versionNode = item.GenerateItemVersionNodeItem(version, item, mConfiguration);
                var result = await discoverWorker.ProcessItemAsync(versionNode, item);
            }
        }

        internal async System.Threading.Tasks.Task ProcessDataAsync(List<AveDiscoverFolder> folders, List<int> itemIDs, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker, bool needInitInfo = false)
        {
            foreach (AveDiscoverFolder folder in folders)
            {
                if (itemIDs != null)
                {
                    if (!itemIDs.Contains(Convert.ToInt32(folder.ID)))
                    {
                        continue;
                    }
                }
                if (folderNode.Parent != null && !string.IsNullOrEmpty(folderNode.Parent.RuleId) && folderNode.Parent.DoDelete)
                {
                    mLog.Warn("Folder parent is match rule, so skip BreakInherit check.{0}", folder.FullUrl);
                }
                else if (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(Site.Url, folder.FullUrl)))
                {
                    mLog.Warn("Folder {0} is break inherit or is null", folder.FullUrl);
                    continue;
                }
                

                ArchiverNodeItem subFolderNode = folderNode.GenerateFolderNodeItem(folder, NodeLevel.Folder, mDiscoverSite.Site.Url, mConfiguration);
                ProcessResult result = await discoverWorker.ProcessContainerAsync(subFolderNode, ProcessType.NeedProcess);
                if (result == ProcessResult.SkipCurrentNode)
                {
                    continue;
                }
                //add folder attachment
                if (folder.GetAttachments().Count > 0)
                {
                    foreach (AveItemObject attachment in folder.GetAttachments())
                    {
                        await ProcessAttachmentsAsync(folderNode, subFolderNode, attachment, discoverWorker);

                    }
                }
                if (folder.GetVersions().Count > 1)
                {
                    foreach (AveVersionObject version in folder.GetVersions())
                    {
                        if ((version.Uiversion == folder.Uiversion) || (version.Uiversion == 0))
                        {
                            continue;
                        }
                        await ProcessFolderVersionsAsync(version, subFolderNode, folder, discoverWorker);
                    }
                }
                await ProcessItemsAndSubfoldersAsync(subFolderNode, subFolderNode.Cache_NodeType, needInitInfo: needInitInfo);
                folder.Dispose();
            }
        }

        private async System.Threading.Tasks.Task ProcessFolderVersionsAsync(AveVersionObject version, ArchiverNodeItem folder, AveDiscoverFolder disFolder, IDiscoverNodeWorker discoverWorker)
        //for folder's version
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessFolderVersions"))
            {
                int subId = 0;
                if (disFolder.ID != null)
                {
                    subId = (int)disFolder.ID;
                }

                ArchiverNodeItem folderVersionNode = folder.GenerateFolderVersionNodeItem(version, NodeLevel.Folder, disFolder);
                ProcessResult result = await discoverWorker.ProcessContainerAsync(folderVersionNode, ProcessType.NeedProcess);
            }
        }

        internal async virtual System.Threading.Tasks.Task ProcessAttachmentsAsync(ArchiverNodeItem folderNode, ArchiverNodeItem item, AveItemObject attachment, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessAttachments"))
            {
                ProcessResult result = ProcessResult.Default;
                try
                {
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
            result.BreakInheritNodesEncryptBySha1 = new Dictionary<string, RuleNodeContract>();
            var breakInheritUrl = LoadBreakInheritNodeUrls(node.FullPath);
            foreach (var b in breakInheritUrl)
            {
                var sh1 = ArchiverCommonStaticMethod.GetBreakInheritSHA1String(b);
                result.BreakInheritNodesEncryptBySha1[sh1] = null;
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
                    catch (AveExceedStorageLimitException e)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Get Site Storage StorageMaximumLevel Error{0}", e.ToString());
                    }
                    
                }
                catch (AveExceedStorageLimitException e)
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

                ArchiverJobManagementService archiverJobManagementService = new ArchiverJobManagementService();
                if (mConfiguration.ArchiverExtendSetting != null
                    && mConfiguration.ArchiverExtendSetting.IsCGDiscovery
                    && archiverJobManagementService.EnableFixFullPathForCGScan())
                {
                    try
                    {
                        mLog.Info($"DBDiscover will fix full path for this site: {Site.Url}");
                        var dbReader = CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, Site.ID.ToString(), Site.Url);
                        List<DBFileInfo> dBFileInfos = dbReader.GetFilesInfoForFixCGFullPath(Site.ID);
                        CheckFileWithSharePoint(dBFileInfos, Site, dbReader);
                        mLog.Info($"DBDiscover finished fix full path for this site: {Site.Url}");
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"DBDiscover failed fix full path for this site: {Site.Url}.Message:{ex}.");
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

        private void CheckFileWithSharePoint(List<DBFileInfo> dBFileInfos, IAveSite aveSite, CGDBReader cgDBReader)
        {
            try
            {
                //根据 Web ID分组
                var webInfoList = dBFileInfos.GroupBy(x => x.webId)
                .Select(group => new DBWebInfo
                {
                    webId = group.Key,
                    DBFileInfos = group.ToList()
                }).ToList();
                mLog.Info($"CheckFileWithSharePoint Web ID Count:{webInfoList.Count()}");
                foreach (DBWebInfo webInfo in webInfoList)
                {
                    IAveWeb web = aveSite.OpenWeb(webInfo.webId);
                    mLog.Info($"CheckFileWithSharePoint. Start processing Web ID:{webInfo.webId}.URL:{web.ServerRelativeUrl}.");
                    foreach (DBFileInfo dBFileInfo in webInfo.DBFileInfos)
                    {
                        if (dBFileInfo.fileType.EqualIgnoreCase("folder"))
                        {
                            mLog.Info($"CheckFileWithSharePoint. Skip folder level approve data.FolderID:{dBFileInfo.ID}.FolderName:{dBFileInfo.fileName}.FolderPath:{dBFileInfo.fullPath}.");
                            continue;
                        }
                        int condition = 0;
                        string fullPath = string.Empty;
                        int listItemId = 0;
                        Guid listId = Guid.Empty;
                        try
                        {
                            IAveFile file = null;
                            try
                            {
                                file = web.GetFile(dBFileInfo.itemId);
                            }
                            catch (Exception e)
                            {
                                file = null;
                                mLog.Info($"CheckFileWithSharePoint.Can not get file by itemId.itemId:{dBFileInfo.itemId}.itemName:{dBFileInfo.fileName}.itemPath:{dBFileInfo.fullPath}.");
                            }
                            if (file != null)
                            {
                                fullPath = GetNodeFullPath(file.ServerRelativeUrl, aveSite.Url);
                                listItemId = file.Item.ID;
                                listId = file.ParentFolder.ParentListId;
                                mLog.Info($"CheckFileWithSharePoint.File Path:{fullPath}.File Id:{listItemId}.listId:{listId}.");
                            }
                            //文件被删除  ||  folder整体删除
                            if (file == null || !file.Exists)
                            {
                                //update IsArchive=’99’
                                condition = 1;
                                cgDBReader.UpdateCGDBUnCorrectData(dBFileInfo.CGDBID, listItemId, condition, dBFileInfo.fullPath, dBFileInfo.fileName, dBFileInfo.listId.ToString());
                                mLog.Info($"CheckFileWithSharePoint.File does not exist in SharePoint and update status to 99. Path:{dBFileInfo.fullPath}.File Id:{listItemId}.itemId:{dBFileInfo.itemId}.");
                                continue;
                            }
                            //文件modified time改变，近三个月改状态为90，超过三个月不处理
                            //if ((DateTime.Now - file.TimeLastModified).Days < 90)
                            //{
                            //    //update IsArchive=’90’
                            //    condition = 2;
                            //    mLog.Info($"CheckFileWithSharePoint.File were changed within three months with an updated status of 90. Path:{dBFileInfo.fullPath}.File Id:{listItemId}.ModifiedTime:{file.TimeLastModified}.");
                            //    //文件被rename特殊处理
                            //    if (!file.Name.Equals(dBFileInfo.fileName))
                            //    {
                            //        condition = 4;
                            //        mLog.Info($"CheckFileWithSharePoint.File name were changed. Path:{dBFileInfo.fullPath}.File Id:{listItemId}.NewFileName:{file.Name}.DBFileName:{dBFileInfo.fileName}.");
                            //    }
                            //    cgDBReader.UpdateCGDBUnCorrectData(dBFileInfo.CGDBID, listItemId, condition, fullPath, file.Name, dBFileInfo.listId.ToString());
                            //    continue;
                            //}
                            //fullpath invalid  ||  文件被rename  ||  文件被move走(Move到同一个site collection下)  || folder整体move走(Move到同一个site collection下)
                            if (!fullPath.Equals(dBFileInfo.fullPath) || !file.Name.Equals(dBFileInfo.fileName))
                            {
                                condition = 3;
                                mLog.Info($"CheckFileWithSharePoint.File fullpath or name also changed.DBPath:{dBFileInfo.fullPath}.File Id:{listItemId}.NewPath:{fullPath}.DBFileName:{dBFileInfo.fileName}.NewFileName:{file.Name}.itemId:{dBFileInfo.itemId}.");
                                cgDBReader.UpdateCGDBUnCorrectData(dBFileInfo.CGDBID, listItemId, condition, fullPath, file.Name, listId.ToString());
                                continue;
                            }
                            //ListItemId为零
                            if (dBFileInfo.ID == 0)
                            {
                                condition = 5;
                                mLog.Info($"CheckFileWithSharePoint.The listitemid of the file is 0.DBPath:{dBFileInfo.fullPath}.File Id:{listItemId}.itemId:{dBFileInfo.itemId}.");
                                cgDBReader.UpdateCGDBUnCorrectData(dBFileInfo.CGDBID, listItemId, condition, fullPath, file.Name, listId.ToString());
                                continue;
                            }
                            mLog.Info($"CheckFileWithSharePoint.There are no problems with the current file, file Path:{fullPath}.DBFilePath:{dBFileInfo.fullPath}.");
                        }
                        catch (Exception e)
                        {
                            mLog.Warn($"CheckFileWithSharePoint.Failed process file:{dBFileInfo.fileName}.fullpath:{dBFileInfo.fullPath}. fileId:{dBFileInfo.ID}.message:{e}.");
                        }
                    }
                    mLog.Info($"CheckFileWithSharePoint.End processing Web ID:{webInfo.webId}.");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"CheckFileWithSharePoint.Exception:{e}.");
            }
            finally
            {
                mLog.Info($"CheckFileWithSharePoint.End modifying data.......");
            }
        }

        private string GetNodeFullPath(string nodePath, String siteUrl)
        {
            string nodeFullPath = string.Empty;
            string siteUrlSchemeAndHost = new Uri(siteUrl).Scheme + @"://" + new Uri(siteUrl).Authority;

            if (nodePath.StartsWith(siteUrlSchemeAndHost, StringComparison.OrdinalIgnoreCase))
            {
                nodeFullPath = nodePath;
            }
            else
            {
                nodeFullPath = siteUrlSchemeAndHost + "/" + nodePath.TrimStart('/');
            }
            return nodeFullPath;
        }


        internal async System.Threading.Tasks.Task ProcessListCollectionAsync(ArchiverNodeItem web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessListCollection"))
            {
                Dictionary<Guid, AveDiscoverList> discoveryLists;
                discoveryLists = (web.DiscoverSPObject as AveDiscoverWeb).GetLists();
                foreach (AveDiscoverList list in discoveryLists.Values)
                {
                    fileInfos.Clear();
                    if (!listIds.Contains(list.ListId))
                    {
                        continue;
                    }
                    string siteIDString = mConfiguration.SiteCollectionID.ToString();
                    string siteUrlString = mConfiguration.SiteCollectionUrl;
                    fileInfos = CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).GetFilesInfo(new Guid(siteIDString), web.WebId, list.ListId, mConfiguration.currentRule.Order);
                    mLog.Info("Begin discover list, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                    fileInfos = fileInfos.GroupBy(f => f.itemId).Select(s => s.FirstOrDefault()).ToList();
                    mLog.Info("Remove duplicate items, list count:{0}", fileInfos.Count);
                    try
                    {
                        bool skipCheckBreakInherit = false;
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
                            ArchiverNodeItem ListNode = web.GenerateListNodeItem(list, tmpList);    
                            mDependencyObjs.PutIn(tmpList, (int)CacheNodeType.List, false);
                            using (ListNode)
                            {
                                await ProcessListAsync(ListNode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARScanProcessListCollectionError, list.Title, ex.ToString());
                    }
                    finally
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles(list.ItemCount);
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.List, 0);
                    }
                }
            }
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
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseTotalFiles(list.Value.ItemCount);
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
            SPORootFolder?.Dispose();
        }
    }
    internal class BackupNodeCache : IScheduleContainer<ArchiveApproveReport>
    {
        PCContainer<ArchiveApproveReport> mContainer;
        public BackupNodeCache(PCContainer<ArchiveApproveReport> pcContainer)
        {
            mContainer = pcContainer;
        }

        public void Store(ArchiveApproveReport node, bool hasReported)
        {
            mContainer.Produce(node);
        }

        public void AddReport(ArchiveApproveReport report) { }

        public void Flush() { }

        public BackwardDependenceNode<ArchiveApproveReport> FetchNext() { return null; }

        public void Dispose() { }
    }
}