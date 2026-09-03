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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.EnforceRetention.Cache;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using AvePoint.Wrapper.Discovery;
using AvePoint.Wrapper.Restore;
using Cloud.Sdk.Telemetry.Data.Alita;
using Google.Apis.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class RecordsSharePointScanner : SharePointScannerBase
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(RecordsSharePointScanner));

        #region add for sp query  
        //private CAMLManager mCAMLManager = null;
        private List<CAMLManager> mCAMLManagers = null;
        private int mMaxItemIdInLibrary = 0;
        private const string SP_ID = "ID";
        private IAveList mSPQueryList = null;
        private SPOFolder SPORootFolder = null;
        private List<TermTreeNode> mGroupTermTreeNodes = null;
        private Dictionary<Guid, RuleItemCollection> mTermAndRulesMapping = null;
        private Dictionary<Guid, int> mTermWssidMappingsOfSite = null;
        private const int mQueryConditionMaxCount = 100;
        public const int mCamlInConditionArrayValuesMaxCount = 100;
        private List<Guid> mTermIds = new List<Guid>();
        #endregion

        private IDiscoverNodeWorker mDiscoverWorker = null;
        private static IRMJobService RMJobService => PlatformWindsorManager.GetService<IRMJobService>();
        private static IRMReportService RMReportService => PlatformWindsorManager.GetService<IRMReportService>();
        private static ITermRuleAssociationDao TermRuleInfos => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private static ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private DB.Explorer.Dao.IExplorerDao explorerDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        public RecordsSharePointScanner(ScanJobSettings scanJobSettings) : base(scanJobSettings)
        {
            scanJobSettings.Configuration.DiscoverWithSPQuery = CheckNeedDiscoverBySPQuery(scanJobSettings.Configuration.RuleCollection.Values.ToList()) && !WrapperConfiguration.IsProcessApprovalDatasOnly;
            CheckHasLastAccessTimeRule(scanJobSettings.Configuration.RuleCollection.Values.ToList());
            WrapperConfiguration.IsRecheckRule = true;
            if (WrapperConfiguration.IsProcessApprovalDatasOnly)
            {
                var isRecheckRule = FunctionSettingDao.GetSettingInfo(FunctionSettingType.IsRecheckRule).GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(isRecheckRule))
                {
                    bool result = Convert.ToBoolean(isRecheckRule);
                    WrapperConfiguration.IsRecheckRule = result;
                }
                else
                {
                    WrapperConfiguration.IsRecheckRule = true;//the old setting need to check rule
                }
                mLog.Info($"current is recheck rule status is :{WrapperConfiguration.IsRecheckRule}");
                GetDatasFromCosmosAndSaveToSqlite(scanJobSettings.Configuration.AveSiteId);
            }
            //scanJobSettings.Configuration.DiscoverWithSPQuery = true;
        }
        private void GetDatasFromCosmosAndSaveToSqlite(string siteId)
        {
            try
            {
                var currentSiteApproveResult = explorerDao.QueryAll(r => r.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved
                && r.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.None
                && r.RecordStatus != (int)RMRecordStatus.Destroyed && r.RecordStatus != (int)RMRecordStatus.RMDeleted
                && r.AveSiteId==siteId.ToString()).ToList();

                ApprovedDatasSqliteHelper.SaveApprovedDatasToSqlite(new Guid(siteId), currentSiteApproveResult, mConfiguration.ArchiveTemp, mConfiguration.JobId);
                mLog.Info($"Save approved data from Cosmos to SQLite finished. SiteId:{siteId}, Count:{currentSiteApproveResult.Count}.");
            }
            catch (Exception ex)
            {
                mLog.Error($"Save approved data from Cosmos to SQLite failed. SiteId:{siteId}, Error:{ex}.");
                throw;
            }
        }
        private void CheckHasLastAccessTimeRule(List<Rule> rules)
        {
            try
            {
                foreach (var rule in rules)
                {
                    foreach (var filter in rule.SOFilters)
                    {
                        if (filter.Rule is AvePoint.GCommon.Contract.CommonFilter.StubLastAccessTimeRule || filter.Rule is AvePoint.GCommon.Contract.CommonFilter.StubLastActiveTimeRule)
                        {
                            mLog.Info($"CheckHasLastAccessTimeRule.Has LastAccessedTime/LastActiveTime, rule type:{filter.Rule} rule name:{rule.Name}.");
                            WrapperConfiguration.WrapperConfigurationForBPOS.HasLATRule = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Info($"CheckHasLastAccessTimeRule failed.Message:{ex}.");
            }
        }

        public override IDiscoverNodeWorker discoverWorker
        {
            get
            {
                if (mDiscoverWorker == null)
                {
                    if (mConfiguration.IsOneDriverSite)
                    {
                        mDiscoverWorker = new RecordsOneDriveScanDiscovrerNodeWorker(jobSettings, mConfiguration, mDependencyObjs);
                    }
                    else
                    {
                        mDiscoverWorker = new RecordsSharePointScanDiscoverNodeWorker(jobSettings, mConfiguration, mDependencyObjs, false);
                    }
                }
                return mDiscoverWorker;
            }
            set { }
        }

        public override List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId = "")
        {
            List<string> mBreakInheritNodeUrls = new List<string>();
            if (mConfiguration.jobtype == Contract.JobMonitor.JobType.ApprovalProcessArchive)
            {
                mLog.Info("this job is ApprovalProcessArchive,so not break nodes");
            }
            else
            {
                var node = RMJobService.BuildBreakTreeNode(jobSettings.TreeNode);
                foreach (RuleNodeContract archiverConfig in node)
                {
                    mBreakInheritNodeUrls.Add(GetFullPathByRuleNode(archiverConfig));
                }
            }
            return mBreakInheritNodeUrls;
        }

        public override bool ListSkipCheck(ArchiverNodeItem listNode)
        {
            try
            {
                var discoverList = ((AveDiscoverList)listNode.DiscoverSPObject);
                if (discoverList != null)
                {
                    if (listNode.SPNodeLevel == NodeLevel.List
                        && !mConfiguration.IsLifecycleManagementEnabledForList(listNode.ID, listNode.Parent?.ID ?? Guid.Empty))
                    {
                        mLog.Info($"Skip list because lifecycle management is disabled. List: {listNode.FullPath}");
                        return true;
                    }

                    if ((!string.IsNullOrEmpty(mConfiguration.ForceFitTeamsRuleID) || !string.IsNullOrEmpty(listNode.Parent.RuleId)))
                    {
                        // No need to skip the list with TeamsGroup or SC or Web level rule.
                        mLog.Info("No need to skip the list {0} with TeamsGroup or SC or Web level rule.", discoverList.Name);
                        return false;
                    }
                    if (CheckIsDesignList(discoverList.Name + discoverList.ListTemplate.ToString()))
                    {
                        mLog.Info("Skip the design list {0}", discoverList.Name);
                        return true;
                    }
                    if (CheckIsDesignList(discoverList))
                    {
                        mLog.Info($"Skip the design list by URL and Template: {discoverList.Name}.");
                        return true;
                    }
                }
                else
                {
                    mLog.Info("CheckIsDesignList discoverList is null");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"CheckIsDesignList error: ({e})");
                return true;
            }
            return false;
        }

        public override async System.Threading.Tasks.Task InitialSPObjectInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node)
        {
            await base.InitialSPObjectInfoAsync(discoverWork, node);
            Guid bcsColumnID = Guid.Empty;
            string bcsColumnName = GetBSCColumnName();
            string internalName = GetBCSColumnInternalName(base.Site, bcsColumnName, ref bcsColumnID);
            ScanDataCache.Instance.SetSiteLevelCache(internalName, bcsColumnName, bcsColumnID, base.groupId.ToString());
        }

        public override System.Threading.Tasks.Task ProcessSiteCollectionAsync(ArchiverNodeItem sitecollection)
        {
            mTermWssidMappingsOfSite = new();
            return base.ProcessSiteCollectionAsync(sitecollection);
        }

        public override async System.Threading.Tasks.Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            mTermWssidMappingsOfSite ??= new();
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    //添加Job Stop相关逻辑
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
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

                        if (!mConfiguration.IsOneDriverSite && !CheckHasRecordsClassificationColumn(list))
                        {
                            mLog.Info("Current list does not contains RecordsClassificationColumn.  List url: {0} .", list.FullPath);
                            return;
                        }
                        AveDiscoverFolder rootFolder = null;

                        var listMatchDeleteRule = false;
                        if (!string.IsNullOrEmpty(list.RuleId) && list.DoDelete)
                        {
                            mLog.Info($"This list match rule and do delete, list url: {list.FullPath}");
                            listMatchDeleteRule = true;
                        }

                        // set is inherit parent term flag here to reduce db call for list
                        list.SetInheritContainerTerm4CurrentList(mConfiguration, needInitInfo);

                        if (!listMatchDeleteRule && NeedDiscoverWithSPQuery(list))
                        {
                            mLog.Info($"[CamlQuery4TermRule]Use SPQuery to discover, Path:[{list.FullPath}].");
                            try
                            {
                                InitForSPQueryDiscover(list.SPList);
                                if (list.IsInheritContainerTerm)
                                {
                                    mLog.Info($"[CamlQuery4TermRule]Use SPQuery to discover unclassfication docs, Path:[{list.FullPath}].");
                                    InitForSPQueryDiscoverUnclassification(list);
                                }
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
                            }
                        }
                        else
                        {
                            mLog.Info($"[CamlQuery4TermRule]Can NOT use SPQuery to discover, Path:[{list.FullPath}].");
                            rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
                        }

                        //mConfiguration.InitOneDriveItemTermInfoByListId(list.ID);
                        if (mDiscoverWorker is RecordsOneDriveScanDiscovrerNodeWorker)
                        {
                            ((RecordsOneDriveScanDiscovrerNodeWorker)mDiscoverWorker).InitOneDriveItemTermInfoByListId(mConfiguration.SiteCollectionID, list.ID);
                        }

                        ArchiverNodeItem foldernode = list.GenerateFolderNodeItem(rootFolder, NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
                        await ProcessFolderAsync(foldernode);
                    }
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
                        mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, e.Message);
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
                    ReleaseForSPQueryDiscover();
                }
            }
        }

        private bool CheckHasRecordsClassificationColumn(ArchiverNodeItem list)
        {
            return CheckHasRecordsClassificationColumn(list.SPList);
        }

        /// <summary>
        /// check bcs column, reset internal name(existing column)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool CheckHasRecordsClassificationColumn(IAveList list)
        {
            bool listContainsBCSColumn = true;
            try
            {
                //web does exit BCS column and List level BCS column set true.
                if (string.IsNullOrEmpty(ScanDataCache.Instance.SiteLevelCache.BCSColumnInternalName)
                    && list.Fields.ContainsField(ScanDataCache.Instance.SiteLevelCache.BCSColumnDisplayName)
                    && mConfiguration.UseListLevelBCSColumn)
                {
                    mLog.Info($"Web does not exist bcs column:{ScanDataCache.Instance.SiteLevelCache.BCSColumnDisplayName} and try use list bcs column:{list.RootFolder?.ServerRelativeUrl}.");
                    var bcsColumn = list.Fields.GetField(ScanDataCache.Instance.SiteLevelCache.BCSColumnDisplayName);
                    if (mConfiguration.IsUseSPQueryOneByOne)
                    {
                        WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName = bcsColumn.InternalName;
                    }
                    if (bcsColumn != null)
                    {
                        mLog.Info($"Web does not exist bcs column and list contains bcs column, list:{list.RootFolder?.ServerRelativeUrl}, column name:{bcsColumn.InternalName}.");
                    }
                    else
                    {
                        listContainsBCSColumn = false;
                    }
                }
                //Default logic(use web column)
                else if (!list.Fields.ContainsFieldWithInternalName(ScanDataCache.Instance.SiteLevelCache.BCSColumnInternalName))
                {
                    if (RetentionDataCache.Instance.BCSColumnInternalName != RcordsBuiltInColumn.ITEM_BCS_NAME)
                    {
                        //existing column reset internal name
                        var bcsColumn = list.Fields.GetFieldById(ScanDataCache.Instance.SiteLevelCache.BCSColumnID, false);
                        if (bcsColumn != null)
                        {
                            RetentionDataCache.Instance.BCSColumnInternalName = bcsColumn.InternalName;
                            if (mConfiguration.IsUseSPQueryOneByOne)
                            {
                                WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName = bcsColumn.InternalName;
                            }
                            mLog.Info($"reset list bcs column, list:{list.RootFolder?.ServerRelativeUrl}, column name:{RetentionDataCache.Instance.BCSColumnInternalName}");
                        }
                        else
                        {
                            listContainsBCSColumn = false;
                        }
                    }
                    else
                    {
                        listContainsBCSColumn = false;
                    }

                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Get list bcs column error:{ex.ToString()}");
            }
            return listContainsBCSColumn;
        }

        private string GetBSCColumnName()
        {
            string columnName = string.Empty;
            if (mConfiguration.IsTeams)
            {
                RMSPExplorerDataCache.SourceFlag = Contract.Explorer.SourceFlag.Teams;
                columnName = new SharePointSettingUtility().GetTeamsMedataColumn(base.groupId);
            }
            else
            {
                columnName = new SharePointSettingUtility().GetMedataColumn(base.groupId);
            }
            return columnName;
        }

        private string GetBCSColumnInternalName(IAveSite aveSite, string columnName, ref Guid bcsColumnID)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetSPSetting"))
            {
                var internalName = string.Empty;
                if (!string.IsNullOrEmpty(columnName))
                {
                    mLog.Info("Column name on group:{0}, groupId {1}", columnName, base.groupId);
                    var field = GetTaxonomyField(aveSite.RootWeb.Fields, columnName);
                    if (field != null)
                    {
                        internalName = field.InternalName;
                        bcsColumnID = field.ID;
                    }

                }
                return internalName;
            }
        }

        protected IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            return fields.GetRecordTaxonomyField(rmFieldTitle);
        }

        private string GetFullPathByRuleNode(RuleNodeContract config)
        {
            string result = null;
            if (config != null)
            {
                //web application level , site collection level和web level存的就是绝对path
                result = GetFullPathByRuleNode(config.NodeLevel, config.NodeName, config.FullPath, config.SiteUrl);
            }
            return result;
        }

        private string GetFullPathByRuleNode(NodeLevel nodeLevel, string nodeName, string fullPath, string siteUrl)
        {
            string result = null;
            switch (nodeLevel)
            {
                case NodeLevel.Farm:
                case NodeLevel.ContentDB:
                    result = nodeName;
                    break;
                case NodeLevel.WebApplication:
                case NodeLevel.SiteCollection:
                case NodeLevel.SkyDrivePro:
                case NodeLevel.SkyDriveProGroup:
                case NodeLevel.O365GroupSites:
                case NodeLevel.O365GroupSitesGroup:
                case NodeLevel.PrivateChannel:
                case NodeLevel.PrivateChannelGroup:
                case NodeLevel.Site:
                    result = fullPath;
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                case NodeLevel.Folder:
                case NodeLevel.RootFolder:
                    result = GetFullPathByPathAndSiteUrl(siteUrl, fullPath);
                    break;
                default:
                    break;
            }
            return result;
        }

        private string GetFullPathByPathAndSiteUrl(string siteUrl, string relativePath)
        {
            string fullPath = string.Empty;
            if (siteUrl == null)
            {
                siteUrl = string.Empty;
            }
            if (relativePath == null)
            {
                relativePath = string.Empty;
            }
            //根据list及folder的相对fullPath和site collection url取得绝对的fullPath
            //去掉list url中的site collection部分的url
            string temFullPath = relativePath;
            int index = temFullPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            while (index > 0)
            {
                temFullPath = temFullPath.Substring(0, index);
                if (siteUrl.EndsWith(temFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath.Substring(index);
                    break;
                }
                else
                {
                    index = temFullPath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                }
            }
            fullPath = siteUrl + relativePath;
            return fullPath;
        }


        private List<Guid> GetTermIds(IAveTaxonomyField taxonomyField)
        {
            List<Guid> subTermIds;
            Guid anchordGuid;
            string anchordId = taxonomyField.GetProperty("AnchorId");
            if (!string.IsNullOrEmpty(anchordId) && anchordId != "00000000-0000-0000-0000-000000000000")
            {
                anchordGuid = new Guid(anchordId);
                subTermIds = GetSubTermIds(anchordGuid);
            }
            else
            {
                subTermIds = GetSubTermIds(taxonomyField.TermSetId);
            }

            return subTermIds;
        }

        private List<Guid> GetSubTermIds(Guid termNodeId)
        {
            List<Guid> termIds = new List<Guid>();
            TermTreeNode termNode = null;

            mGroupTermTreeNodes ??= RMReportService.GetRATermTreeNodesAsync().GetAwaiter().GetResult();
            foreach (var groupNode in mGroupTermTreeNodes)
            {
                termNode = GetTermTreeNode(groupNode, termNodeId);
                if (termNode != null)
                {
                    break;
                }
            }
            GetSubTermIds(termNode, ref termIds);
            return termIds;
        }

        private void GetSubTermIds(TermTreeNode termNode, ref List<Guid> termIds)
        {
            if (termNode != null)
            {
                foreach (var item in termNode.Children)
                {
                    termIds.Add(item.Key);
                    GetSubTermIds(item.Value, ref termIds);
                }
            }
        }

        private TermTreeNode GetTermTreeNode(TermTreeNode sourceNode, Guid termNodeId)
        {
            TermTreeNode tempNode = null;
            if (sourceNode != null && sourceNode.ID != termNodeId)
            {
                if (sourceNode.Children != null && sourceNode.Children.Count > 0 && !sourceNode.Children.TryGetValue(termNodeId, out tempNode))
                {
                    foreach (var node in sourceNode.Children.Values)
                    {
                        tempNode = GetTermTreeNode(node, termNodeId);
                        if (tempNode != null)
                        {
                            break;
                        }
                    }
                }
            }
            return tempNode;
        }

        private bool NeedDiscoverWithSPQuery(ArchiverNodeItem listNode)
        {
            try
            {
                IAveList list = listNode.SPList;
                mLog.Info($"NeedDiscoverWithSPQuery {list.Title} : {list.BaseTemplate} : {list.BaseType}");
                if (mConfiguration.DiscoverWithSPQuery && (list.BaseTemplate == AveListTemplateType.DocumentLibrary || list.BaseType == AveBaseType.DocumentLibrary))
                {
                    IAveTaxonomyField taxonomyField = list.Fields.GetFieldById(ScanDataCache.Instance.SiteLevelCache.BCSColumnID, false) as IAveTaxonomyField;
                    using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GetTermWssids"))
                    {
                        mTermIds = GetTermIds(taxonomyField);
                    }
                    mLog.Info("List Begin SPQuery to filter data. Path:{0}.", listNode.FullPath);
                    Dictionary<Guid, int> allWssIds = GetTermWssids(list);
//#if DEBUG
//                    var tempAllTermIds = new List<Guid>(mTermIds);
//#endif
                    //filter term by wssid
                    mTermIds = mTermIds.Where(t => allWssIds.ContainsKey(t)).ToList();

//#if DEBUG
//                    mTermIds = tempAllTermIds;
//#endif


                    var ruleCollForList = mTermAndRulesMapping
                        .Where(m => mTermIds.Contains(m.Key) 
                            || (listNode.IsInheritContainerTerm && listNode.ContainerLevelTermId == m.Key))
                        .Select(m => m.Value).ToList();

                    mLog.Info($"[CamlQuery4TermRule]Get rules by terms, rules: [{string.Join(", ", ruleCollForList.SelectMany(ruleColl => ruleColl.Rules.Select(rule => rule.RuleId)).Distinct())}]");
                    foreach (var ruleColl in ruleCollForList)
                    {
                        foreach (var rule in ruleColl.CommonRules.Rules ?? new())
                        {
                            if (rule.Value.PolicyLevel != PolicyLevel.Document)
                            {
                                mLog.Info("[CamlQuery4TermRule]Has non document level, can not use caml scan.");
                                return false;
                            }
                        }
                    }

                    return !ruleCollForList.Any(c => c.HasUnCamlQueryableCondition);
                }
                else
                {
                    mLog.Info("[CamlQuery4TermRule]Can not use caml scan.");
                    return false;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred while getting discover method. Error:{e.ToString()}");
                return false;
            }
        }

        private void InitForSPQueryDiscover(IAveList list)
        {
            mCAMLManagers = new();
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitForSPQueryDiscover"))
            {
                mConfiguration.mUseQueryDiscover = true;
                mSPQueryList = list;
                CamlScan cs = new();
                Dictionary<RuleItemCollection, List<int>> termGroupByRules = new();
                Dictionary<int, int> termGroupByRulesIndexMap = new();
                foreach (var termId in mTermIds)
                {
//#if DEBUG
//                    if (!mTermWssidMappingsOfSite.ContainsKey(termId)) {
//                        mTermWssidMappingsOfSite.Add(termId, new Random().Next(512, 1024));
//                    }
//#endif

                    if (mTermWssidMappingsOfSite.TryGetValue(termId, out int wssid))
                    {
                        if (mTermAndRulesMapping.TryGetValue(termId, out var ruleColl))
                        {
                            if (termGroupByRules.ContainsKey(ruleColl))
                            {
                                if (!termGroupByRulesIndexMap.TryGetValue(ruleColl.GetHashCode(), out int groupIndex))
                                {
                                    groupIndex = 0;
                                    termGroupByRulesIndexMap.Add(ruleColl.GetHashCode(), groupIndex);
                                }
                                var newRuleColl = CloneRuleColl(ruleColl);
                                newRuleColl.Index4TermGroup = groupIndex;
                                if (termGroupByRules[newRuleColl].Count >= mCamlInConditionArrayValuesMaxCount)
                                {
                                    groupIndex++;
                                    termGroupByRulesIndexMap.AddOrReplace(ruleColl.GetHashCode(), groupIndex);
                                    newRuleColl.Index4TermGroup = groupIndex;
                                    termGroupByRules[newRuleColl] = new() { wssid };
                                }
                                else
                                {
                                    termGroupByRules[newRuleColl].Add(wssid);
                                }
                            }
                            else
                            {
                                var newRuleColl = CloneRuleColl(ruleColl);
                                termGroupByRules[newRuleColl] = new List<int> { wssid };
                            }
                        }
                    }

                }

                //termGroupByRules Take 100 
                int index = 0;
                while (termGroupByRules.Skip(index).Take(mQueryConditionMaxCount) != null && termGroupByRules.Skip(index).Take(mQueryConditionMaxCount).Any())
                {
                    var termRuleQuerys = termGroupByRules.Skip(index).Take(mQueryConditionMaxCount).ToList();
                    index += mQueryConditionMaxCount;
                    if (termRuleQuerys.Count != 0)
                    {
                        var cm = cs.InitCamlQuery(list, mConfiguration.ArchiverUNCTime, termRuleQuerys);
                        if (cm != null)
                        {
                            mCAMLManagers.Add(cm);
                        }
                    }
                }

                mMaxItemIdInLibrary = GetLastItemId(list, list.RootFolder.ServerRelativeUrl);
                mLog.Info($"Using spquery for list:{list.Title} Max item id:{mMaxItemIdInLibrary}");
            }
        }

        private void InitForSPQueryDiscoverUnclassification(ArchiverNodeItem listNode)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitForSPQueryDiscoverUnclassificationDocs"))
            {
                var list = listNode.SPList;
                mConfiguration.mUseQueryDiscover = true;
                mSPQueryList ??= list;
                CamlScan cs = new();
                RuleItemCollection ruleCollection = new();
                Dictionary<int, int> termGroupByRulesIndexMap = new();

                if (listNode.ContainerLevelTermId == Guid.Empty)
                {
                    mLog.Info($"List {list.RootFolder.Url} use inherit parent term but does not have container level term. Skip init caml query");
                    return; // not query unclassification rule without any container level term
                }

                // Use termId from properties even if it's not in TaxonomyHiddenList so mTermWssidMappingsOfSite may not contain it.
                //mTermWssidMappingsOfSite.TryGetValue(listNode.ContainerLevelTermId, out _) 
                if (mTermAndRulesMapping.TryGetValue(listNode.ContainerLevelTermId, out var ruleColl))
                {
                    var newRuleColl = CloneRuleColl(ruleColl);
                    ruleCollection = newRuleColl;
                }

                if (ruleCollection.Rules == null || ruleCollection.Rules.Count == 0)
                {
                    mLog.Info($"List {list.RootFolder.Url} use container level term but cannot found any rules mapping, termId: {listNode.ContainerLevelTermId}");
                    return; // not query unclassification rule without any container level term
                }

                HashSet<int> criterias = [];
                foreach (var rule in ruleCollection.Rules)
                    foreach (var rf in rule.RuleFilters)
                        criterias.Add((int)rf.RuleType);

                var indexedFieldstaticNames = list.Fields.Where(f => f.Indexed).Select(f => f.StaticName);

                mLog.Info($"List {list.RootFolder.Url} has criterias: {string.Join(',', criterias)} . indexedFields: {string.Join(',', indexedFieldstaticNames)}");

                ruleCollection.HasUnCamlQueryableCondition |= SPCommonUtility.FilterIndexedIncludeCriteria(indexedFieldstaticNames, criterias);
                
                var timepoint = indexedFieldstaticNames.Contains(SPColumnConstants.SP_Created) ? DateTime.MinValue : mConfiguration.ArchiverUNCTime;

                var cm = cs.InitCamlQuery4Unclassification(list, timepoint, ruleCollection);
                if (cm != null)
                {
                    cm.IsUnclassificationQuery = true;
                    (mCAMLManagers ?? []).Add(cm);
                    mLog.Info($"finish init spquery for Unclassification for list:{list.Title}");
                }
                
                //mMaxItemIdInLibrary = GetLastItemId(list, list.RootFolder.ServerRelativeUrl);
                //mLog.Info($"Using spquery for list:{list.Title} Max item id:{mMaxItemIdInLibrary}");
            }
        }

        private RuleItemCollection CloneRuleColl(RuleItemCollection coll)
        {
            return new RuleItemCollection()
            {

                HasUnCamlQueryableCondition = coll.HasUnCamlQueryableCondition,
                TermId = coll.TermId,
                TermName = coll.TermName,
                Index4TermGroup = coll.Index4TermGroup,
                CommonRules = coll.CommonRules,
                Rules = coll.Rules,
            };
        }

        private void ReleaseForSPQueryDiscover()
        {
            try
            {
                mConfiguration.mUseQueryDiscover = false;
                mSPQueryList = null;
                mCAMLManagers = null;
                mMaxItemIdInLibrary = 0;
            }
            catch(Exception e)
            {
                mLog.Error($"error occured when ReleaseForSPQueryDiscover,error:{e}");
            }
        }

        private Dictionary<Guid, int> GetTermWssids(IAveList list)
        {
            var allWssIds = CAMLManagerUtil.GetTaxonomyHiddenListTerms(list.ParentWeb.Site);
            foreach (var termId in allWssIds.Keys)
            {
                if (!mTermWssidMappingsOfSite.TryGetValue(termId, out int wssid))
                {
                    try
                    {
                        if (allWssIds.TryGetValue(termId, out wssid))
                        {
                            mTermWssidMappingsOfSite.Add(termId, wssid);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Get TermId And WssId Mapping failed! Term id: {0}. Error message: {1}.", termId, ex.ToString());
                    }
                }
            }
            return allWssIds;
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
        /// 第一次Query获取所有Item信息并拼接SPFolder/Item结构
        /// </summary>
        /// <param name="rootFolderServerRelativeUrl"></param>
        private void InitArchiverSPQueryRootFolder(string rootFolderServerRelativeUrl)
        {
            int rowLimit = 2000;
            SPORootFolder?.Dispose();
            SPORootFolder = SPOFolder.BuildRootFolder(new CacheDBOperator<SPOItem>(), new CacheDBOperator<SPOFolder>(), rootFolderServerRelativeUrl);
            foreach (var mCAMLManager in mCAMLManagers)
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryRootFolder"))
                {
                    int startIndex = 0;
                    int endIndex = 0;
                    int totaltemsCount = 0;
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
                            mLog.Info($"Start to query InitArchiverSPQueryRootFolder in :{rootFolderServerRelativeUrl}.");
                            do
                            {
                                endIndex = startIndex + rowLimit > mMaxItemIdInLibrary ? mMaxItemIdInLibrary : startIndex + rowLimit;
                                mCAMLManager.QueryGroup.Conditions.RemoveAll(g => g.Query.Field == SP_ID);
                                mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIndex.ToString()));
                                mCAMLManager.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, endIndex.ToString()));
                                string queryXml = mCAMLManager.GetFullCAML();
                                query.ViewXml = queryXml;
                                mLog.Info("InitArchiverSPQueryRootFolder xml {0}:{1}. query length:{2}", rootFolderServerRelativeUrl, queryXml, queryXml.Length);
                                IAveListItemCollection items = mSPQueryList.GetItems(query);
                                executeCount++;
                                totaltemsCount = totaltemsCount + items.Count;
                                mLog.Info("InitArchiverSPQueryRootFolder {0}, query execute count:{1}. folder items count:{2}.", rootFolderServerRelativeUrl, executeCount, items.Count);
                                AnalyzeListItems(items, SPORootFolder, mCAMLManager.IsUnclassificationQuery);
                                items = null;
                                mLog.Info("InitArchiverSPQueryRootFolder ProcessDataWithSPQuery finished:{0}.execute count:{1}.", rootFolderServerRelativeUrl, executeCount);
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
                            mLog.Info("InitArchiverSPQueryRootFolder xml {0}:{1}, query execute count:{2} totaltemsCount:{3}.", rootFolderServerRelativeUrl, mCAMLManager.GetFullCAML(), executeCount, totaltemsCount);
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
        }

        /// <summary>
        /// 第二次Query，Query List下所有Folder，拼接Folder信息，主要是获取FolderID
        /// </summary>
        /// <param name="rootFolderServerRelativeUrl"></param>
        private void InitArchiverSPQueryFolderStructure(string rootFolderServerRelativeUrl)
        {
            foreach (var mCAMLManager in mCAMLManagers)
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
        }

        /// <summary>
        /// 拼接Folder/Item结构
        /// </summary>
        /// <param name="items"></param>
        /// <param name="rootFolder"></param>
        void AnalyzeListItems(IAveListItemCollection items, SPOFolder rootFolder, bool isUnclassificationQuery)
        {
            bool? listCreatedIndexed = null;
            foreach (var item in items)
            {
                var serverRelativeUrl = (string)item.FieldValues["FileRef"];
                var name = (string)item.FieldValues["FileLeafRef"];

                var parentFolder = rootFolder;
                var frUrl = serverRelativeUrl.Substring(rootFolder.Name.Length, serverRelativeUrl.Length - rootFolder.Name.Length - name.Length - 1);
                
                if (isUnclassificationQuery)
                {
                    listCreatedIndexed = (listCreatedIndexed.HasValue && listCreatedIndexed.Value) 
                        || item.ParentList.Fields[SPColumnConstants.SP_Created].Indexed;

                    var TimeCreated = item.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? item.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0;
                    if (listCreatedIndexed.Value && TimeCreated >= mConfiguration.ArchiverUNCTime.Ticks)
                    {
                        mLog.Info($"Skip item created after ArchiverUNCTime: {mConfiguration.ArchiverUNCTime}. ObjectId:{item.ID}.ObjectServerRelativeUrl:{frUrl}.");
                        continue;
                    }
                }

                mLog.Info($"AnalyzeListItems. ObjectId:{item.ID}.ObjectServerRelativeUrl:{frUrl}.");
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

                var id = item.ID;
                if (item.FileSystemObjectType == AveFileSystemObjectType.File)
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
                    var spoFolder = parentFolder.SubFolders.GetByName(name);
                    if (spoFolder == null)
                    {
                        spoFolder = SPOFolder.BuildUnRootFolder(parentFolder, name, id);
                        parentFolder.SubFolders.Add(spoFolder);
                    }
                    else
                    {
                        spoFolder.Id = id;
                    }
                }
            }
        }
        private class SPOItemComparer : IEqualityComparer<SPOItem>
        {
            public bool Equals(SPOItem x, SPOItem y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                {
                    return false;
                }
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(SPOItem obj)
            {
                return obj.GetHashCode();
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
                mLog.Error($"Cannot find root folder id by url: {rootFolder.Name}");
                throw new Exception($"Cannot find root folder id by url {rootFolder.Name}");
            }
            if (rootFolder.SubFolders != null)
            {
                foreach (SPOFolder folder in rootFolder.SubFolders)
                {
                    AssignFolderId(folder, rootFolder.Name, folderItems);
                }
            }
        }

        private void AssignFolderId(SPOFolder folder, string parentFolderServerRelativePath, List<SPFolderReducedInfo> realFolders)
        {
            var currentFolderServerRelativePath = parentFolderServerRelativePath + "/" + folder.Name;
            var realCurrentFolder = realFolders.FirstOrDefault(x => string.Equals(x.ServerRelativeUrl, currentFolderServerRelativePath, StringComparison.OrdinalIgnoreCase));
            if (realCurrentFolder != null)
            {
                folder.Id = realCurrentFolder.ID;
                mLog.Info($"AssignFolderId. FolderId:{folder.Id}.ObjectServerRelativeUrl:{currentFolderServerRelativePath}.");
            }
            else
            {
                //log can't find the folder from SP
                mLog.Error($"Cannot find folder id by url: {currentFolderServerRelativePath}");
            }
            if (folder.SubFolders != null)
            {
                foreach (var subfolder in folder.SubFolders)
                {
                    AssignFolderId(subfolder, currentFolderServerRelativePath, realFolders);
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

        private bool CheckNeedDiscoverBySPQuery(List<Rule> rule)
        {
            bool useSPQuery = true;
            if (mConfiguration.IsOneDriverSite)
            {
                useSPQuery = false;
                return useSPQuery;
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GetTermAndRuleMappings"))
            {
                mTermAndRulesMapping = GetTermAndRuleMappings(DateTime.UtcNow, rule.ToList());
            }

            mLog.Info($"Use spquery to discover:{useSPQuery}, QueryConditionMaxCount: {mQueryConditionMaxCount},CamlInConditionArrayValuesMaxCount: {mCamlInConditionArrayValuesMaxCount}.");
            return useSPQuery;
        }

        public Dictionary<Guid, RuleItemCollection> GetTermAndRuleMappings(DateTime timePoint, List<Rule> daRules)
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RuleItemCollection>();
            Dictionary<Guid, Rule> allRules = daRules.ToDictionary(r => new Guid(r.Id));
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RuleItem> rmRules = new List<RuleItem>();
                bool hasUnCamlQueryableCondition = false;
                Rule rule;
                var ruleIds = termRules[term.Id];
                int reOrder = 0;
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None && rule.SOFilters != null && rule.SOFilters.Count > 0)
                        {
                            reOrder++;
                            var ruleOBj = CamlUtil.CloneSameRuleObject(rule);
                            commonRules.Rules.Add(reOrder, ruleOBj);
                            if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                            {
                                rmRules.Add(CamlUtil.ConvertRuleChecker(ruleOBj, timePoint));
                            }
                            else
                            {
                                CamlUtil.ModifyRuleChecker(ruleOBj, timePoint);
                            }
                        }

                    }
                }
                if (rmRules.Count > 0)
                {
                    if (rmRules.Exists(rc => rc.HasUnCamlQueryableCondition))
                    {
                        hasUnCamlQueryableCondition = true;
                    }
                }
                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms, timePoint.Ticks);
                foreach (var refTerm in refTerms)
                {
                    RuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RuleItemCollection();
                        tempRC.TermId = refTerm.UniqueId;
                        tempRC.TermName = refTerm.Name;
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }
                    tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;
                }
            }

            return termRuleMappings;
        }

        public override void Dispose()
        {
            base.Dispose();
            SPORootFolder?.Dispose();
        }

    }
}
