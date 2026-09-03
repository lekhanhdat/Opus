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
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
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
    internal class CGArchiverSharePointScanner : SharePointCGScannerBase
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ArchiverSharePointScanner));
        #region add for sp query  
        private CAMLManager mCAMLManager = null;
        private int mMaxItemIdInLibrary = 0;
        private const string SP_ID = "ID";
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
                    IBackwardDependencyNodeCache<ArchiveApproveReport> mBackupNodeCache = new BackwardDependenceNodeCache<ArchiveApproveReport>(new BackupNodeCache(pcContainer));
                    mDiscoverWorker = new DBScanDiscoverNodeWorker(mBackupNodeCache,jobSettings, mConfiguration, mDependencyObjs);
                }
                return mDiscoverWorker;
            }
            set { }
        }

        public CGArchiverSharePointScanner(ScanJobSettings scanJobSettings) : base(scanJobSettings)
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
            return false;
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

        internal bool NeedDiscoverWithSPQuery(IAveList list)
        {
            try
            {
                mLog.Info($"NeedDiscoverWithSPQuery {list.Title} : {list.BaseTemplate} : {list.BaseType}");
                if (mConfiguration.DiscoverWithSPQuery
                    && (list.BaseTemplate == AveListTemplateType.DocumentLibrary || list.BaseType == AveBaseType.DocumentLibrary)
                    )
                {
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
                mLog.Info($"{list.Title} : {list.BaseTemplate} : {list.BaseType}");
                if (mConfiguration.DiscoverWithSPQueryForVersion
                    && (list.BaseTemplate == AveListTemplateType.DocumentLibrary || list.BaseType == AveBaseType.DocumentLibrary)
                    )
                {
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
        internal void InitArchiverSPQueryRootFolder(string rootFolderServerRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.InitArchiverSPQueryRootFolder"))
            {
                int startIndex = 0;
                int endIndex = 0;
                int totaltemsCount = 0;
                int rowLimit = 2000;
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

    }
}
