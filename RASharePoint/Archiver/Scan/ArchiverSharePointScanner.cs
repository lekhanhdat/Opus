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
using AvePoint.Api.Contract.Job;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.RA.I18N.Core;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using AvePoint.Wrapper.Discovery;
using Cloud.Sdk.Data.Dao;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class ArchiverSharePointScanner : SharePointScannerBase
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ArchiverSharePointScanner));
        #region add for sp query  
        private CAMLManager mCAMLManager = null;
        private int mMaxItemIdInLibrary = 0;
        private const string SP_ID = "ID";
        private const int MaxItemIdThreshold = 1000000;
        private const double IdAndCountRatioThreshold = 200;
        private IAveList mSPQueryList = null;
        private RuleItemCollection RuleItemCollection = null;
        internal SPOFolder SPORootFolder = null;
        #endregion
        public List<Guid> rmLocationIds = new List<Guid>();
        public List<String> rmSiteUrl = new List<String>();

        private IDiscoverNodeWorker mDiscoverWorker = null;
        public override IDiscoverNodeWorker discoverWorker
        {
            get
            {
                if (mDiscoverWorker == null)
                {
                    mDiscoverWorker = new ScanDiscovrerNodeWorker(jobSettings, mConfiguration, mDependencyObjs, false);
                }
                return mDiscoverWorker;
            }
            set { }
        }
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public ArchiverSharePointScanner(ScanJobSettings scanJobSettings) : base(scanJobSettings)
        {
            scanJobSettings.Configuration.DiscoverWithSPQuery = CheckNeedDiscoverBySPQuery(scanJobSettings.Configuration.RuleCollection.Values.ToList());
            scanJobSettings.Configuration.DiscoverWithSPQueryForVersion = CheckNeedDiscoverBySPQueryForVersion(scanJobSettings.Configuration.RuleCollection.Values.ToList());
            CheckHasLastAccessTimeRule(scanJobSettings.Configuration.RuleCollection.Values.ToList());
            //scanJobSettings.Configuration.SkipDiscoverItemForFolderLevelRule = CheckSkipDiscoverItemForFolderLevelRule(scanJobSettings.Configuration.RuleCollection);
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

        public override bool ListSkipCheck(ArchiverNodeItem listNode)
        {
            if (CheckIsRMLocation(listNode.ID, listNode.FullPath) && CheckIsRMSite(listNode.SiteUrl))
            {
                mLog.Info("The list is record manager's destination location, skip it.  List url: {0} .", listNode.FullPath);
                return true;
            }
            try
            {
                var discoverList = ((AveDiscoverList)listNode.DiscoverSPObject);
                if (discoverList != null)
                {
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
                    //Document Version Rule & Document Rule skip general list
                    if (listNode.SPList != null && NeedSkipGenericList(listNode.SPList))
                    {
                        mLog.Info("Current rule is document/document version rule and current list is general list so skip process.  List url: {0} .", listNode.FullPath);
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

        private bool NeedSkipGenericList(IAveList list)
        {
            bool skipGenericList = false;
            try
            {
                if (!mConfiguration.IsILMode)
                {
                    mLog.Info($"NeedSkipGenericList:{list.Title} : {list.BaseTemplate} : {list.BaseType}");
                    if (list.BaseType == AveBaseType.GenericList && OnlyHasDocumentOrDocumentVersionRule())
                    {
                        skipGenericList = true;
                    }
                    else
                    {
                        skipGenericList = false;
                    }
                }
                else
                {
                    skipGenericList = false;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred while NeedSkipGenericList. Error:{e}");
            }
            return skipGenericList;
        }

        private bool OnlyHasDocumentOrDocumentVersionRule()
        {
            bool onlyHasDocumentOrDocumentVersionRule = false;
            try
            {
                if (mConfiguration.RuleCollection != null
                    && mConfiguration.RuleCollection.Values != null
                    && mConfiguration.RuleCollection.Values.Count > 0)
                {
                    bool hasOtherLevelRule = false;
                    foreach (var rule in mConfiguration.RuleCollection.Values)
                    {
                        if (rule.PolicyLevel != GCommon.Contract.CommonFilter.PolicyLevel.Document
                            && rule.PolicyLevel != GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion)
                        {
                            hasOtherLevelRule = true;
                            break;
                        }
                    }
                    if (!hasOtherLevelRule)
                    {
                        onlyHasDocumentOrDocumentVersionRule = true;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"An error occurred while CheckOnlyHasDocumentOrDocumentVersionRule. Error:{ex}");
            }
            return onlyHasDocumentOrDocumentVersionRule;
        }

        public override async System.Threading.Tasks.Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
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
                    else
                    {
                        if (IrmLeaveStubListSkipHelper.TryGetListLevelMatchedRule(mConfiguration, list.SPList, out var matchedRule))
                        {
                            mLog.Info(
                                "Skip list for leave-stub IRM restriction after container rule check. ListTitle:{0}, RuleId:{1}, RuleName:{2}, KeepDataOption:{3}, PolicyLevel:{4}, IrmEnabled:{5}, IrmReject:{6}.",
                                list.Title,
                                matchedRule?.Id,
                                matchedRule?.Name,
                                matchedRule?.KeepDataOption,
                                matchedRule?.PolicyLevel,
                                list.SPList?.IrmEnabled,
                                list.SPList?.IrmReject);

                            mConfiguration.JobReportDto.AddScanReport(
                                list.FullPath,
                                0,
                                (int)CacheNodeType.List,
                                string.Empty,
                                Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped,
                                IrmLeaveStubListSkipHelper.SkipReportMessageKey);
                            return;
                        }
                    }
                    AveDiscoverFolder rootFolder = null;
                    if (NeedDiscoverWithSPQuery(list.SPList))
                    {
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
                        }
                    }
                    else if (NeedDiscoverWithSPQueryForVersionRule(list.SPList))
                    {
                        try
                        {
                            mLog.Info("List Begin SPQuery to filter data for version rule. Path:{0}.", list.FullPath);
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
                        }
                    }
                    else
                    {
                        rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
                    }
                    ArchiverNodeItem foldernode = list.GenerateFolderNodeItem(rootFolder, GCommon.Contract.Tree.Object.NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
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
                    if ((e.InnerException is ServerUnauthorizedAccessException) && (list.DiscoverSPObject as AveDiscoverList)?.ListTemplate == (int)AveListTemplateType.UserInformation)
                    {
                        mLog.Info("Skip the user info list {0}", list.FullPath);
                        mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, e.InnerException.Message);
                        mConfiguration.JobReportDto.HasErrorNode = true;
                        throw;
                    }
                    else
                    {
                        mLog.Error("An unexpected error occurred while processing list node.Path:{0}.Message:{1}.", list.FullPath, e.ToString());
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
                }
            }
        }

        private bool CheckIsRMLocation(Guid listId, string url)
        {
            if (listId == Guid.Empty)
            {
                mLog.Info("CheckIsRMLocation: The listid is null. Url: {0}. ", url);
                return false;
            }
            else if (rmLocationIds.Contains(listId))
            {
                mLog.Info("CheckIsRMLocation: The rmLocationIds Contains listId. listUrl: {0}, listID: {1}. ", url, listId);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CheckIsRMSite(string siteUrl)
        {
            bool flag = false;

            if (rmSiteUrl.Contains(siteUrl))
            {
                flag = true;
            }
            return flag;
        }

        private bool ShouldSkipSpQueryByIdAndCount(IAveList list)
        {
            try
            {
                if (list == null || list.ItemCount <= 0)
                {
                    return false;
                }

                double ratioThreshold = IdAndCountRatioThreshold;
                var setting = KeyValueDao.GetValueByKey(KeyNameCollection.SharepointidAndCountRatio);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    if (!double.TryParse(setting.Value, out ratioThreshold) || ratioThreshold <= 0)
                    {
                        mLog.Info($"Key {KeyNameCollection.SharepointidAndCountRatio} exists but value is invalid:{setting.Value}. Use default threshold:{IdAndCountRatioThreshold}.");
                        ratioThreshold = IdAndCountRatioThreshold;
                    }
                }

                var maxItemId = GetLastItemId(list, list.RootFolder.ServerRelativeUrl);
                if (maxItemId <= 0 || maxItemId < MaxItemIdThreshold)
                {
                    return false;
                }

                var ratio = maxItemId / (double)list.ItemCount;
                if (ratio > ratioThreshold)
                {
                    mLog.Info($"Skip SPQuery by ID/count ratio. List:{list.Title}, ItemCount:{list.ItemCount}, MaxItemId:{maxItemId}, Ratio:{ratio}, Threshold:{ratioThreshold}.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Check ShouldSkipSpQueryByIdAndCount failed. Error:{ex}.");
            }

            return false;
        }

        internal bool NeedDiscoverWithSPQuery(IAveList list)
        {
            try
            {
                if (list == null)
                {
                    return false;
                }
                mLog.Info($"NeedDiscoverWithSPQuery {list.Title} : {list.BaseTemplate} : {list.BaseType}");
                if (mConfiguration.DiscoverWithSPQuery
                    && (list.BaseTemplate == AveListTemplateType.DocumentLibrary || list.BaseType == AveBaseType.DocumentLibrary)
                    )
                {
                    if (ShouldSkipSpQueryByIdAndCount(list))
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred while getting discover method. Error:{e.ToString()}");
                return false;
            }
        }

        internal bool NeedDiscoverWithSPQueryForVersionRule(IAveList list)
        {
            try
            {
                if (list == null)
                {
                    return false;
                }
                mLog.Info($"{list.Title} : {list.BaseTemplate} : {list.BaseType}");
                if (mConfiguration.DiscoverWithSPQueryForVersion
                    && (list.BaseTemplate == AveListTemplateType.DocumentLibrary || list.BaseType == AveBaseType.DocumentLibrary)
                    )
                {
                    if (ShouldSkipSpQueryByIdAndCount(list))
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred while getting discover method. Error:{e.ToString()}");
                return false;
            }
        }

        internal void InitForSPQueryDiscover(IAveList list)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitForSPQueryDiscover"))
            {
                mConfiguration.mUseQueryDiscover = true;
                mSPQueryList = list;
                CamlScan cs = new CamlScan();
                mCAMLManager = cs.InitCamlQuery(list, list.Fields, RuleItemCollection, DateTime.UtcNow, true);
                mMaxItemIdInLibrary = GetLastItemId(list, list.RootFolder.ServerRelativeUrl);
                mLog.Info($"Using spquery for list:{list.Title} Max item id:{mMaxItemIdInLibrary}");
            }
        }

        internal void InitForSPQueryDiscoverForVersionRule(IAveList list)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitForSPQueryDiscover"))
            {
                mConfiguration.mUseQueryDiscover = true;
                mSPQueryList = list;
                CamlScan cs = new CamlScan();
                mCAMLManager = cs.InitCamlQuery(list, list.Fields, RuleItemCollection, DateTime.UtcNow, true);
                mMaxItemIdInLibrary = GetLastItemId(list, list.RootFolder.ServerRelativeUrl);
                mLog.Info($"Using spquery for list:{list.Title} Max item id:{mMaxItemIdInLibrary}");
            }
        }

        internal void ReleaseForSPQueryDiscover()
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
                mLog.Error($"something when ReleaseForSPQueryDiscover,error:{e}");
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

        private int GetSPQueryRowLimit()
        {
            const int defaultRowLimit = 2000;

            try
            {
                var keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                var setting = keyValueDao?.GetValueByKey(KeyNameCollection.SPQueryRowLimit);
                if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
                {
                    return defaultRowLimit;
                }

                if (int.TryParse(setting.Value, out var configuredRowLimit) && configuredRowLimit > 0)
                {
                    mLog.Info($"Use configured SP query row limit for archiver. Key:{KeyNameCollection.SPQueryRowLimit}, Value:{configuredRowLimit}.");
                    return configuredRowLimit;
                }

                mLog.Warn($"Ignore invalid archiver SP query row limit setting. Key:{KeyNameCollection.SPQueryRowLimit}, Value:{setting.Value}. Use default row limit:{defaultRowLimit}.");
            }
            catch (Exception ex)
            {
                mLog.Warn($"Read archiver SP query row limit failed. Use default row limit:{defaultRowLimit}. Error:{ex}");
            }

            return defaultRowLimit;
        }

        /// <summary>
        /// 第一次Query获取所有Item信息并拼接SPFolder/Item结构
        /// </summary>
        /// <param name="rootFolderServerRelativeUrl"></param>
        internal void InitArchiverSPQueryRootFolder(string rootFolderServerRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryRootFolder"))
            {
                int startIndex = 0;
                int endIndex = 0;
                int totaltemsCount = 0;
                int rowLimit = GetSPQueryRowLimit();
                SPORootFolder?.Dispose();
                SPORootFolder = SPOFolder.BuildRootFolder(new CacheDBOperator<SPOItem>(), new CacheDBOperator<SPOFolder>(), rootFolderServerRelativeUrl);
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
                            mLog.Info("InitArchiverSPQueryRootFolder xml {0}:{1}.", rootFolderServerRelativeUrl, queryXml);
                            IAveListItemCollection items = mSPQueryList.GetItems(query);
                            executeCount++;
                            totaltemsCount = totaltemsCount + items.Count;
                            mLog.Info("InitArchiverSPQueryRootFolder {0}, query execute count:{1}. folder items count:{2}.", rootFolderServerRelativeUrl, executeCount, items.Count);
                            AnalyzeListItems(items, SPORootFolder);
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

        /// <summary>
        /// 第二次Query，Query List下所有Folder，拼接Folder信息，主要是获取FolderID
        /// </summary>
        /// <param name="rootFolderServerRelativeUrl"></param>
        internal void InitArchiverSPQueryFolderStructure(string rootFolderServerRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryFolderStructure"))
            {
                int startIndex = 0;
                int endIndex = 0;
                int totaltemsCount = 0;
                int rowLimit = GetSPQueryRowLimit();
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

        /// <summary>
        /// 拼接Folder/Item结构
        /// </summary>
        /// <param name="items"></param>
        /// <param name="rootFolder"></param>
        void AnalyzeListItems(IAveListItemCollection items, SPOFolder rootFolder)
        {
            foreach (var item in items)
            {
                var serverRelativeUrl = (string)item.FieldValues["FileRef"];
                var name = (string)item.FieldValues["FileLeafRef"];

                var parentFolder = rootFolder;
                var frUrl = serverRelativeUrl.Substring(rootFolder.Name.Length, serverRelativeUrl.Length - rootFolder.Name.Length - name.Length - 1);
                mLog.Info($"AnalyzeListItems. ObjectId:{item.ID}.ObjectServerRelativeUrl:{frUrl}.");
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
            bool useSPQuery = false;

            var documentRules = rule.Where(r => r.PolicyLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document).ToList();
            var nonDocumentsRules = rule.Where(r => r.PolicyLevel != AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document).ToList();
            if (documentRules.Count > 0 && nonDocumentsRules.Count == 0)
            {
                RuleItemCollection = CamlUtil.GetRuleItemCollection(DateTime.UtcNow, rule.Where(r => r.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document).ToList());
                if (!RuleItemCollection.HasUnCamlQueryableCondition)
                {
                    useSPQuery = true;
                }
            }
            else
            {
                mLog.Info($"Document rule count:{documentRules.Count}, non document rule count:{nonDocumentsRules.Count}.");
            }

            mLog.Info($"Use spquery to discover:{useSPQuery}.");
            return useSPQuery;
        }

        private bool CheckNeedDiscoverBySPQueryForVersion(List<Rule> rule)
        {
            bool useSPQuery = false;

            var documentVersionRules = rule.Where(r => r.PolicyLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion).ToList();
            var nonDocumentVersionRules = rule.Where(r => r.PolicyLevel != AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion).ToList();
            if (documentVersionRules.Count > 0 && nonDocumentVersionRules.Count == 0)
            {
                RuleItemCollection = CamlUtil.GetRuleItemCollectionForVersionRule(DateTime.UtcNow, rule.Where(r => r.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion).ToList());
                if (!RuleItemCollection.HasUnCamlQueryableCondition)
                {
                    useSPQuery = true;
                }
            }
            else
            {
                mLog.Info($"Document version rule count:{nonDocumentVersionRules.Count}, non document version rule count:{nonDocumentVersionRules.Count}.");
            }

            mLog.Info($"Use spquery version size to discover:{useSPQuery}.");
            return useSPQuery;
        }

        public override void Dispose()
        {
            base.Dispose();
            SPORootFolder?.Dispose();
        }

        /// <summary>
        /// Scan Job，只有Folder Level Rule时不Discover Item，只处理Folder本身，提升效率.
        /// </summary>
        /// <param name="ruleCollection"></param>
        /// <returns></returns>
        /*private bool CheckSkipDiscoverItemForFolderLevelRule(Dictionary<int, Rule> ruleCollection)
        {
            bool skipDiscoverItemForFolderLevelRule = false;

            var mRuleEngine = new RuleManagement(ruleCollection);
            if (mRuleEngine.HasFolderCondition && !mRuleEngine.HasLowerLevelRule((int)CacheNodeType.Folder))
            {
                mLog.Info("Current rule collection has folder rule and does not have low level rule so skip discover item.");
                skipDiscoverItemForFolderLevelRule = true;
            }

            mLog.Info($"Skip Discover Item For Folder Level Rule:{skipDiscoverItemForFolderLevelRule}.");
            return skipDiscoverItemForFolderLevelRule;
        }*/
    }
}
