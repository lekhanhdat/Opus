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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Microsoft.Azure.Amqp.Framing;
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
    public abstract class SharePointScannerBase : ISharePointScanner
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SharePointScannerBase));
        private Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        private readonly object locker = new();
        private IScanDataReader mScanDataReader = null;
        private long totalScanCount = 0;

        internal AveDiscoverSite mDiscoverSite = null;
        internal IBackwardDependencyNodeCache<object> mDependencyObjs;
        internal ScanJobSettings jobSettings = null;
        internal ScheduleConfiguration mConfiguration = null;
        internal Guid scopeId = Guid.Empty;
        internal Guid groupId = Guid.Empty;
        internal AveObjectModelFactory mFactory = null;
        private List<string> mDesignLists = null;

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

        private List<string> DesignLists
        {
            get
            {
                if (mDesignLists == null)
                {
                    mDesignLists = GetDesignLists();
                }
                return mDesignLists;
            }
        }

        protected IAveSite Site { get; set; }
        private IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        public abstract IDiscoverNodeWorker discoverWorker
        {
            get;
            set;
        }

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public IDiscoverNodeWorker RelativeDataDiscoverWorker { get; set; }

        public SharePointScannerBase(ScanJobSettings scanJobSettings)
        {
            mDependencyObjs = new BackwardDependenceNodeCache<object>();
            jobSettings = scanJobSettings;
            mConfiguration = scanJobSettings.Configuration;
            mScanDataReader = new ScanDataReader(mConfiguration);
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

        public virtual async System.Threading.Tasks.Task RunAsync()
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
                    else if (mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB)
                    {
                        mLog.Info($"virtual will use splited DB, job id:{mConfiguration?.JobId}");
                        return;
                    }

                    bool siteIsHold = false;
                    if (ArchiveJobLimitCollection.ShouldCheckSiteHoldJobTypeSet.Contains(mConfiguration.jobtype))
                    {
                        try
                        {
                            siteIsHold = CheckSiteCollectionIsHold();
                        }
                        catch (Exception ex) when (
                            ex.Message.Contains("403") ||
                            ex.Message.Contains("Forbidden") ||
                            (ex.InnerException != null && (
                                ex.InnerException.Message.Contains("403") ||
                                ex.InnerException.Message.Contains("Forbidden"))))
                        {
                            mLog.Warn($"HTTP 403 on CheckSiteCollectionIsHold for {mConfiguration.SiteCollectionUrl}, treating as Skipped.");
                            mConfiguration.JobReportDto.AddScanReport(
                                mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection,
                                string.Empty, JobDetailsStatus.Skipped, "RM_ArchiveSCBy365_Detail_Skip");
                            return;
                        }
                    }

                    if (siteIsHold)
                    {
                        mLog.Info($"site was hold");
                        bool hasNotMicrosoftArchiveAction = true;
                        if (!mConfiguration.IsILMode)
                        {
                            foreach (var r in mConfiguration?.RuleCollection?.Values)
                            {
                                hasNotMicrosoftArchiveAction = (r.KeepDataOption & (int)KeepDataOption.TriggerMicrosoft365Archiving) != (int)KeepDataOption.TriggerMicrosoft365Archiving;
                                if (hasNotMicrosoftArchiveAction)
                                {
                                    mLog.Info($"this job not only include Microsoft365Archiving action");
                                    break;
                                }
                            }
                        }
                        if (hasNotMicrosoftArchiveAction && mConfiguration.RuleCollection.Any(rule => RuleHelper.CheckIsWillDeleteDataAction(rule.Value)))
                        {
                            string ruleName = "";
                            if (!string.IsNullOrWhiteSpace(mConfiguration.ForceFitTeamsRuleID))
                            {
                                ruleName = mConfiguration.RuleCollection.Values?.FirstOrDefault(r => r.Id.ToString() == mConfiguration.ForceFitTeamsRuleID)?.Name;
                            }
                            mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, ruleName, JobDetailsStatus.Skipped, "RM_JM_SiteCollectionHoldAndHaveDeletRule_ErrorMessage");
                            return;
                        }
                    }

                    var node = RMDtoConverter.ConvertRMTree2SPTree(jobSettings.TreeNode);
                    CheckSCLevelRuleUseByUnOphenNodeInODSource(node);
                    scopeId = Guid.Parse(node.SPObjectId);
                    groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(node).SPObjectId);
                    var ruleNode = ConvertTreeNodeToRuleNodeConfig(node, RuleNodeType.Archiver);
                    discoverWorker.Init(ruleNode);
                    ArchiverNodeItem selectNodeItem = new ArchiverNodeItem(ruleNode);

                    var (hasSetting, isInheritParentTerm) = mConfiguration.TryGetIsEnableInheritTerm(selectNodeItem.ID, node.Level, jobSettings.TreeNode);
                    if (hasSetting && isInheritParentTerm)
                    {
                        selectNodeItem.IsInheritContainerTerm = true;
                    }
                    mLog.Info($"Get inherit term flag for select node: {selectNodeItem.ID}, level: {node.Level}, IsInheritContainerTerm: {selectNodeItem.IsInheritContainerTerm}");
                    JobExecutionProcessStatisticExecutor.Instance.StartCalculateRuleAndSummary(selectNodeItem.SPNodeLevel.ToString(), selectNodeItem.FullPath);

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
                                await ProcessFolderAsync(selectNodeItem, true, selectNodeItem.ItemIDs);
                                break;
                            }
                        case NodeLevel.Item:
                            {
                                break;
                            }
                        default:
                            throw new Exception(LOGRESOURCE.StorageOptimization13_SOARScanScanException);
                    }
                    mDependencyObjs.Flush();
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
                    if (mConfiguration?.ArchiveJobSplitedDBInfo?.IsUseSplitedDB == false)
                    {
                        JobExecutionProcessStatisticExecutor.Instance.EndCalculateRuleAndScanSummary(totalScanCount, Site);
                    }
                }
            }
        }
        private void CheckSCLevelRuleUseByUnOphenNodeInODSource(SPTreeNodeDto node)
        {
            if ((node.Type == GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup || node.Type == GCommon.Contract.Tree.Object.NodeType.SkyDriveProSites) &&
                        mConfiguration.RuleCollection.Count(ruleEntity => ruleEntity.Value.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection) > 0)
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
            using (new CheckJobStopScope()) { }
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
                if (relativeDataArchiveObject == null)
                {
                    mLog.Error("End user tree node string is null or empty cannot get end user archive object");
                    throw new Exception("End user tree node string is null or empty cannot get end user archive object");
                }
                // mPcContainer.StartProduce();
                ArchiveApproveReport report = relativeDataArchiveObject.Approve;
                IBackwardDependencyNodeCache<ArchiveApproveReport> mBackupNodeCache = new BackwardDependenceNodeCache<ArchiveApproveReport>(new ApprovalReportService(mConfiguration));
                {
                    BackwardDependenceNodeCache<object> dependencyObjs = new BackwardDependenceNodeCache<object>();
                    RelativeDataBackupDiscoverNodeWork worker = new RelativeDataBackupDiscoverNodeWork(mBackupNodeCache, dependencyObjs, mConfiguration);
                    RelativeDataDiscoverWorker = worker;
                    {
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
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARCLReaderDiscoverNode, ex.ToString());
                mLog.Warn("[RunRelativeDataScanAsync][Exception]Set the HasErrorNode true.");
                mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARCLReaderDiscoverError;
                mConfiguration.ProgressDto.HasErrorNode = true;
            }
            finally
            {
                RelativeDataDiscoverWorker.Flush();
            }
            //JobExecutionProcessStatisticExecutor.Instance.EndCalculateRuleAndScanSummary(totalScanCount, Site);
        }

        public virtual List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId)
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
                        await InitialSPObjectInfoAsync(discoverWorker, sitecollection);
                        //If the rootWeb has defined a unique rule, we should skip all the site collection.
                        //URL of RootWeb is same as sitecollection's
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;

                        ProcessResult result =(await  discoverWorker.ProcessContainerAsync(sitecollection, ProcessType.NeedProcess));
                        if (result == ProcessResult.SkipCurrentNode)
                        {
                            mLog.Info("skip current Node {0}", sitecollection.FullPath);
                            return;
                        }

                        using (AveDiscoverSite discoverySite = sitecollection.DiscoverSPObject as AveDiscoverSite)
                        {
                            using (AveDiscoverWeb rootWeb = discoverySite.GetRootWeb())
                            {
                                if (!mConfiguration.UseArchiverImportFile && discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(sitecollection.FullPath)))
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
                    //catch (AveWrapperI18NException IUPEx)
                    //{
                    //    mLog.Info("Site Collection UserName Or Password Incorrect. Path:{0}. Message:{1}.", sitecollection.FullPath, IUPEx.ToString());
                    //    throw;
                    //}
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
                    catch (AveWrapperI18NException IUPEx)
                    {
                        mLog.Info("Site Collection UserName Or Password Incorrect. Path:{0}. Message:{1}.", sitecollection.FullPath, IUPEx.ToString());

                        if (IUPEx is AveSkipLockSiteException)
                        {
                            mLog.Info($"skip locked site collection: {sitecollection.FullPath}");
                            mConfiguration.JobReportDto.AddScanReport(
                                sitecollection.SiteUrl, 0, (int)CacheNodeType.SiteCollection,
                                string.Empty, JobDetailsStatus.Skipped, "RM_ArchiveSCBy365_Detail_Skip");
                            return;
                        }

                        throw;
                    }
                    catch (SPObjectNotFoundException ex) when (
    ex.Message.Contains("unable access") ||
    (ex.InnerException != null && (
        ex.InnerException.Message.Contains("403") ||
        ex.InnerException.Message.Contains("Forbidden"))))
                    {
                        mLog.Warn($"SPObjectNotFoundException likely caused by 403 for {sitecollection.FullPath}, treating as Skipped.");
                        mConfiguration.JobReportDto.AddScanReport(
                            sitecollection.SiteUrl, 0, (int)CacheNodeType.SiteCollection,
                            string.Empty, JobDetailsStatus.Skipped, "RM_ArchiveSCBy365_Detail_Skip");
                        return;
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
            catch (Exception e)
            {
                mLog.Error("Process sitecollection error {0}", e.ToString());
                if (e is AveExceedStorageLimitException)
                {
                    mConfiguration.JobReportDto.AddScanReport(sitecollection.SiteUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_JM_SiteStorageLimit_ErrorMessage");
                }
                throw;
                //TO DO Add Detail
                //TO DO I18N
                //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty,
                //    string.Empty, string.Empty, JobReportDetailStatus.Failed, e.Message);
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
                    if (result == ProcessResult.FitRule || (web.Parent != null && !string.IsNullOrEmpty(web.Parent.RuleId)))
                    {
                        await ProcessAppCollectionAsync(web); //添加支持APP的Backup  
                    }
                    await ProcessListCollectionAsync(web, result == ProcessResult.SkipListNode);
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
                    using (new CheckJobStopScope()) { }
                    if (ListSkipCheck(list))
                    {
                        return;
                    }

                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, list);
                        OutPutListItemCount(new()
                        {
                            { list.ListId, list.DiscoverSPObject as AveDiscoverList }
                        });
                    }

                    CheckAccessableForUserInfoList(list);

                    if ((await discoverWorker.ProcessContainerAsync(list, ProcessType.NeedProcess)) == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                    AveDiscoverFolder rootFolder = null;
                    rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
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
                    if ((e.InnerException is ServerUnauthorizedAccessException) && (list.DiscoverSPObject as AveDiscoverList)?.ListTemplate == (int)AveListTemplateType.UserInformation)
                    {
                        mLog.Info("[ProcessListAsync][InnerException][ServerUnauthorizedAccessException]Skip the user info list {0}", list.FullPath);
                        mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, e.InnerException.Message);
                        mConfiguration.JobReportDto.HasErrorNode = true;
                        throw;
                    }
                    else
                    {
                        mLog.Error("[ProcessListAsync][Exception]An unexpected error occurred while processing list node.Path:{0}.Message:{1}.", list.FullPath, e.ToString());
                        mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, "", JobDetailsStatus.Failed, e.Message);
                        mConfiguration.JobReportDto.HasErrorNode = true;
                        throw;
                    }
                }
                finally
                {
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles((list.DiscoverSPObject as AveDiscoverList).ItemCount);
                    }
                    mConfiguration.ProgressDto.UpdateProgress();
                }
            }
        }

        /// <summary>
        /// Process folder for initialization
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="needInitInfo"></param>
        /// 承接上层列表 / 父文件夹的调用，完成文件夹级别的初始化、规则校验、拦截过滤，最终向下分发处理当前文件夹内的文档项 (Item) 和子文件夹，形成完整的文件夹树递归扫描
        public async virtual System.Threading.Tasks.Task ProcessFolderAsync(ArchiverNodeItem folder, bool needInitInfo = false, List<int> itemIDs = null)
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
                        folder.SetInheritContainerTerm4CurrentList(mConfiguration, needInitInfo);
                        mLog.Info($"Get inherit term flag for folder node: {folder.ID} from parent list. IsInheritContainerTerm: {folder.IsInheritContainerTerm}, ContainerLevelTermId: {folder.ContainerLevelTermId}");
                    }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(folder, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//add for RevIM RECO-84
                    {
                        return;
                    }
                    await ProcessItemsAndSubfoldersAsync(folder, folder.Cache_NodeType, itemIDs, needInitInfo);

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
            using (new CheckJobStopScope()) { }
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
            using (new CheckJobStopScope()) { }
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
                catch (JobStopException)
                {
                    throw;
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

        protected bool CheckIsDesignList(string listInfo)
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

        protected bool CheckIsDesignList(AveDiscoverList discoverList)
        {
            return CheckIsDesignList(CombineListUrlAndTemplate(discoverList));
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

        protected void CheckAccessableForUserInfoList(ArchiverNodeItem node)
        {
            var list = (node.DiscoverSPObject as AveDiscoverList);
            if (list?.ListTemplate == (int)AveListTemplateType.UserInformation)
            {
                list.GetListTitle();
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
                            node.SPList ??= splist;
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

        internal async System.Threading.Tasks.Task ProcessDataAsync(List<AveDiscoverItem> items, List<int> itemIDs, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
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
                        if (item.CurrentItem != null
                            && item.CurrentItem.FieldValues != null
                            && item.CurrentItem.FieldValues.ContainsKey("_FileArchiveStatus")
                            && item.CurrentItem.FieldValues["_FileArchiveStatus"] != null
                            && !string.IsNullOrEmpty(item.CurrentItem.FieldValues["_FileArchiveStatus"].ToString()))
                        {
                            mConfiguration.JobReportDto.AddScanReport(itemFullUrl, 0, (int)CacheNodeType.Item, string.Empty, JobDetailsStatus.Skipped, "RM_ArchiveBy365_Detail_Skip");
                            mLog.Info($"skip fully archived file:{item.ID}");
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

        

        internal async virtual System.Threading.Tasks.Task ProcessVersionAndAttachmentsAsync(AveDiscoverItem item, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
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
                            catch (JobStopException)
                            {
                                watch.Stop();
                                throw;
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
            using (new CheckJobStopScope()) { }
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
                using (new CheckJobStopScope()) { }
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
                else if (!mConfiguration.UseArchiverImportFile && discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(Site.Url, folder.FullUrl)))
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
            using (new CheckJobStopScope()) { }
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
            using (new CheckJobStopScope()) { }
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
            string siteObjectId = GetSiteSPObjectId(node);
            var breakInheritUrl = LoadBreakInheritNodeUrls(node.Level == NodeLevel.Folder?string.IsNullOrEmpty(node.FullUrl)?node.FullPath: node.FullUrl : node.FullPath, siteObjectId);
            foreach (var b in breakInheritUrl)
            {
                var sh1 = ArchiverCommonStaticMethod.GetBreakInheritSHA1String(b);
                result.BreakInheritNodesEncryptBySha1[sh1] = null;
            }
            return result;
        }
        private string GetSiteSPObjectId(SPTreeNodeDto node)
        {
            string result = string.Empty;
            if (node.Level == NodeLevel.SiteCollection)
            {
                result = node.SPObjectId;
            }
            else if (node.Level > NodeLevel.SiteCollection)
            {
                var tempNode = node;
                while (tempNode != null)
                {
                    tempNode = tempNode.Parent;
                    if (tempNode != null && tempNode.Level == NodeLevel.SiteCollection)
                    {
                        result = tempNode.SPObjectId;
                        break;
                    }
                }
            }
            NodeLevel level = node.Level;
            mLog.Info($"the Get Site SPObjectId result is {result},{level}");
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
                    catch (AveSkipLockSiteException) 
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"Get StorageMaximumLevel Error.Message:{e}.");
                    }
                    
                    OutPutWebRoleDefinitions(Site);
                }
                catch (AveExceedStorageLimitException aex)
                {
                    throw;
                }

                catch (Exception e)
                {
                    mLog.Error($"An unexpected error occurred while processing site node.Message:{e}.");
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

        private void OutPutWebRoleDefinitions(IAveSite aveSite)
        {
            try
            {
                var roleDefinitions = aveSite.RootWeb.RoleDefinitions;
                foreach (IAveRoleDefinition roleDefinition in roleDefinitions)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    var permissions = roleDefinition.BasePermissions;
                    foreach (var permission in Enum.GetValues(typeof(AveBasePermissions)))
                    {
                        if (permissions.Has((AveBasePermissions)permission))
                        {
                            stringBuilder.Append(permission.ToString()+";");
                        }
                    }
                    mLog.Info($"OutPutWebRoleDefinitions.RoleName:{roleDefinition.Name}.RoleDescription:{roleDefinition.Description}.BasePermissions:{stringBuilder.ToString()}.");
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"OutPutWebRoleDefinitions Error.Message:{ex}.");
            }
        }

        internal async System.Threading.Tasks.Task ProcessListCollectionAsync(ArchiverNodeItem web, bool isSkipListNode = false)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessListCollection"))
            {
                Dictionary<Guid, AveDiscoverList> discoveryLists;
                discoveryLists = (web.DiscoverSPObject as AveDiscoverWeb).GetLists();
                foreach (AveDiscoverList list in discoveryLists.Values)
                {
                    mLog.Info("Begin discover list, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);

                    try
                    {
                        if (isSkipListNode)
                        {
                            mLog.Info("Skip discover list, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                            continue;
                        }
                        bool skipCheckBreakInherit = false;
                        if (web != null && !string.IsNullOrEmpty(web.RuleId) && web.DoDelete)
                        {
                            skipCheckBreakInherit = true;
                            mLog.Info("List parent is match rule, so skip BreakInherit check.{0}", list.Name);
                        }
                        //arthur: need complete this {system folder} logical later. add to scandiscoverNodeWorker
                        if (list.Title.Equals("{System Folder}"))
                        {
                            mLog.Info("Current list is System Folder when discover list collection, url is :{0},title is: {1}.", list.RootFolderUrl, list.Title);
                            ArchiverNodeItem listnode = web.GenerateListNodeItem(list, null);
                            listnode.FullPath = listnode.Parent.FullPath;
                            await ProcessListAsync(listnode);
                        }
                        else if (!mConfiguration.UseArchiverImportFile && !skipCheckBreakInherit && (list == null || discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(Site.Url, list.RootFolderUrl))))
                        {
                            if (list != null)
                            {
                                mLog.Warn("List {0} is break inherit or is null", list.Name);
                            }
                            else
                            {
                                mLog.Info("Current list is null when discover list collection.");
                            }
                            continue;
                        }
                        else
                        {
                            IAveWeb tmpWeb = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as IAveWeb;

                            IAveList tmpList = tmpWeb.GetList(list.RootFolderUrl);
                            mLog.Info("Current list [{0}] ItemCount [{1}].", list.RootFolderUrl, tmpList.ItemCount);
                            ArchiverNodeItem ListNode = web.GenerateListNodeItem(list, tmpList);
                            if (ListSkipCheck(ListNode))
                            {
                                continue;
                            }
                            mDependencyObjs.PutIn(tmpList, (int)CacheNodeType.List, false);
                            using (ListNode)
                            {
                                await ProcessListAsync(ListNode);
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
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles(list.ItemCount);
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.List, 0);
                    }
                }
            }
        }

        internal async System.Threading.Tasks.Task ProcessWebCollectionAsync(ArchiverNodeItem web)
        {
            using (new CheckJobStopScope()) { }
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
                        bool skipCheckBreakInherit = false;
                        if (web != null && !string.IsNullOrEmpty(web.RuleId) && web.DoDelete)
                        {
                            skipCheckBreakInherit = true;
                            mLog.Warn("web parent is match rule, so skip BreakInherit check.{0}", tmp.FullUrl);
                        }
                        if (!mConfiguration.UseArchiverImportFile && !skipCheckBreakInherit && (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(Site.Url, tmp.FullUrl))))
                        {
                            mLog.Warn("{0} is break inherit from parent", tmp.FullUrl);
                            continue;
                        }
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
                        { };
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessAppCollectionAsync(ArchiverNodeItem web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWebAppCollection"))
            {
                Dictionary<Guid, AveDiscoverAppDefinition> discoveryApps;
                discoveryApps = (web.DiscoverSPObject as AveDiscoverWeb).GetAppDefinitions();
                foreach (AveDiscoverAppDefinition appDefinition in discoveryApps.Values)
                {
                    using (new CheckJobStopScope()) { }
                    try
                    {
                        IAveWeb tmpWeb = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as IAveWeb;
                        IAveAppInstance tmpAppInstance = tmpWeb.GetAppInstanceById(appDefinition.InstanceId);
                        mDependencyObjs.PutIn(tmpAppInstance, (int)CacheNodeType.APP, false);

                        using (ArchiverNodeItem app = web.GenerateWebAppDefinitionNodeItem(appDefinition, tmpAppInstance))
                        {
                            ProcessResult result = await discoverWorker.ProcessContainerAsync(app, ProcessType.NeedProcess);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error("[ProcessAppCollectionAsync][Exception]Get AppInstance has error,message:{0}", ex);
                        string fitRuleName = mConfiguration.RuleCollection?.Values?.FirstOrDefault(rule => rule?.Id == web?.RuleId)?.Name;
                        mConfiguration.JobReportDto.AddScanReport(web.FullPath + '/' + appDefinition.Name, 0, (int)CacheNodeType.APP, fitRuleName);
                        mConfiguration.JobReportDto.HasErrorNode = true;
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

        protected AveBPOSAccountInfo GetBposInfoBySite(string siteUrl)
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
                                if (IsWebBreakInherit(web, subWeb))
                                {
                                    continue;
                                }
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

        protected void OutPutListItemCount(Dictionary<Guid, AveDiscoverList> discoverLists)
        {
            foreach (var list in discoverLists)
            {
                try
                {
                    if (list.Value != null)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseTotalFiles(list.Value.ItemCount);
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
            RelativeDataDiscoverWorker?.Dispose();
            discoverWorker.Dispose();
        }
    }
}