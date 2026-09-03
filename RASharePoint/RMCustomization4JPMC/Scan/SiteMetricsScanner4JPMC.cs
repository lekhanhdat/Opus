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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Extension;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Base;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Implement;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Interface;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan
{
    internal class SiteMetricsScanner4JPMC : SiteMetricsScanner4JPMCBase
    {
        protected static readonly RALogger mLog = RALogger.GetInstance(typeof(SiteMetricsScanner4JPMC));
        #region add for sp query  
        private CAMLManager mCAMLManager = null;
        private int mMaxItemIdInLibrary = 0;
        private const string SP_ID = "ID";
        private IAveList mSPQueryList = null;
        private RuleItemCollection RuleItemCollection = null;
        internal SPOFolder SPORootFolder = null;
        #endregion
        
        private JPMCTenantConfig mJPMCTenantConfig = null;
        private List<string> DesignLists = new List<string>();

        private IDiscoverNodeWorker mDiscoverWorker = null;
        public override IDiscoverNodeWorker discoverWorker
        {
            get
            {
                if (mDiscoverWorker == null)
                {
                    mDiscoverWorker = new JPMCScanDiscovrerNodeWorker(jobSettings, mConfiguration, mDependencyObjs, mJPMCTenantConfig);
                }
                return mDiscoverWorker;
            }
            set { }
        }
        public SiteMetricsScanner4JPMC(ScanJobSettings scanJobSettings, JPMCTenantConfig jpmcConfig, string siteUrl = "") : base(scanJobSettings)
        {
            scanJobSettings.Configuration.DiscoverWithSPQuery = CheckNeedDiscoverBySPQuery(scanJobSettings.Configuration.RuleCollection.Values.ToList());
            DesignLists = GetDesignLists();
            mJPMCTenantConfig = jpmcConfig;
        }

        public override bool ListSkipCheck(ArchiverNodeItem listNode)
        {
            try
            {
                var discoverList = (AveDiscoverList)listNode.DiscoverSPObject;
                if (discoverList != null)
                {
                    if (CheckIsDesignList(discoverList.Name + discoverList.ListTemplate.ToString()))
                    {
                        mLog.Info("Skip the design list {0}", discoverList.Name);
                        return true;
                    }
                    if (listNode.SPList != null && NeedSkipGenericList(listNode.SPList))
                    {
                        mLog.Info("Skip general list. List url: {0} .", listNode.FullPath);
                        return true;
                    }
                    foreach (var rule in mConfiguration.RuleCollection)
                    {
                        foreach (var filter in rule.Value.Filters)
                        {
                            if (filter.RuleType == PolicyRuleType.Column)
                            {
                                var exist = false;
                                var columnName = filter.Rule.Value1;
                                if (columnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && columnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                                {
                                    var internalName = columnName.Trim(['[', ']']);
                                    exist = (listNode?.SPList?.Fields?.ContainsFieldWithInternalName(internalName)).GetValueOrDefault();
                                }
                                else
                                {
                                    exist = (listNode?.SPList?.Fields?.ContainsField(columnName)).GetValueOrDefault();
                                }
                                if (!exist)
                                {
                                    mLog.Info($"Skip this list, because column {columnName} is not exist, list URL:{listNode.FullPath}");
                                    return true;
                                }
                            }
                        }
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
            }
            return false;
        }

        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (DesignLists.Contains(listInfo))
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

        private List<string> GetDesignLists()
        {
            return WebUtil.GetDesignLists(false);
        }

        private bool NeedSkipGenericList(IAveList list)
        {
            return list.BaseType == AveBaseType.GenericList;
        }

        public override async Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope()) { }
                    if (ListSkipCheck(list))
                    {
                        return;
                    }

                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, list);
                    }

                    if (await discoverWorker.ProcessContainerAsync(list, ProcessType.NeedProcess) == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                    AveDiscoverFolder rootFolder = null;
                    var discoverList = list.DiscoverSPObject as AveDiscoverList;
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
                    else
                    {
                        rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
                    }
                    ArchiverNodeItem foldernode = list.GenerateFolderNodeItem(rootFolder, GCommon.Contract.Tree.Object.NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
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
                    throw;
                }
                finally
                {
                    mConfiguration.ProgressDto.UpdateProgress();
                }
            }
        }

        protected override void DownloadScanDbIfExists(string blobPath, string localFilePath)
        {
            mLog.Info("Full scan mode skips downloading existing scan db.");
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
                var parentFoldersName = frUrl.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
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

            var documentRules = rule.Where(r => r.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document).ToList();
            var nonDocumentsRules = rule.Where(r => r.PolicyLevel != GCommon.Contract.CommonFilter.PolicyLevel.Document).ToList();
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

        public override void Dispose()
        {
            base.Dispose();
            SPORootFolder?.Dispose();
        }
    }
}
