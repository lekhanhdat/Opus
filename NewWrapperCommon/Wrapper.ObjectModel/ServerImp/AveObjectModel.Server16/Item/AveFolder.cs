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



using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Collections;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Linq;
using AvePoint.GCommon.Contract.CodeReview;
using System.Text;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    class AveFolder : AveServerObject, IAveFolder, IAveEnableCache, IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public bool EnableCache { get; set; }
        private SPFolder mFolder;
        private AveFolder mParentFolder;
        private AveFileCollection mFiles;
        private AveFolderCollection mFolders;
        private AveWeb mWeb;
        private AveListItem mItem;
        private AveFolderCollection mSubFolders;
        private IList<IAveContentType> mUniqueContentTypeOrder;
        private AveList m_ParentList;
        private AveSite mSite;
        private List<AveHiddenFileInfo> mHiddenFiles;
        private AveAudit mAudit;
        private AveDataCacheManager dataCacheManager;

        public AveFolder(AveWeb web, SPFolder folder)
        {
            mWeb = web;
            mFolder = folder;
            mSite = mWeb.Site as AveSite;
            this.dataCacheManager = new AveDataCacheManager(this);
        }

        internal SPFolder Folder
        {
            get { return mFolder; }
        }

        #region IAveFolder Members

        public IAveFileCollection Files
        {
            get
            {
                if (mFiles == null)
                {
                    mFiles = new AveFileCollection(this, mFolder.Files);
                }
                return mFiles;
            }
        }

        public IAveFolderCollection Folders
        {
            get
            {
                if (mFolders == null)
                {
                    mFolders = new AveFolderCollection(mWeb, mFolder.SubFolders);
                }
                return mFolders;
            }
        }

        public int ItemCount
        {
            get
            {
                return mFolder.ItemCount;
            }
        }

        public string Name
        {
            get { return mFolder.Name; }
        }

        public void MoveTo(string newUrl)
        {
            mFolder.MoveTo(newUrl);
        }

        public IAveFolder ParentFolder
        {
            get
            {
                if (mParentFolder == null)
                {
                    mParentFolder = new AveFolder(mWeb, mFolder.ParentFolder);
                }
                return mParentFolder;
            }
        }

        public string ServerRelativeUrl
        {
            get { return mFolder.ServerRelativeUrl; }
        }

        public string WelcomePage
        {
            get
            {
                return mFolder.WelcomePage;
            }
            set
            {
                mFolder.WelcomePage = value;
            }
        }

        public void Update()
        {
            mFolder.Update();
        }

        public string Url
        {
            get { return mFolder.Url; }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                return mWeb;
            }
        }

        public bool Exists
        {
            get { return mFolder.Exists; }
        }

        public IAveListItem Item
        {
            get
            {
                if (mItem == null)
                {
                    if (!Guid.Empty.Equals(mFolder.ParentListId) && !Guid.Empty.Equals(mFolder.ParentFolder.ParentListId))
                    {
                        SPListItem item = mFolder.Item;
                        if (item != null)
                        {
                            mItem = new AveListItem(ParentList as AveList, item);
                        }
                    }
                }
                return mItem;
            }
        }

        public Guid ParentListId
        {
            get { return mFolder.ParentListId; }
        }

        public IAveFolderCollection SubFolders
        {
            get
            {
                if (mSubFolders == null)
                {
                    mSubFolders = new AveFolderCollection(mWeb, mFolder.SubFolders);
                }
                return mSubFolders;
            }
        }

        public Hashtable Properties
        {
            get { return mFolder.Properties; }
        }

        public Guid UniqueId
        {
            get { return mFolder.UniqueId; }
        }

        public IList<IAveContentType> ContentTypeOrder
        {
            get
            {
                IList<SPContentType> contentTypes = mFolder.ContentTypeOrder;
                if (contentTypes != null)
                {
                    IList<IAveContentType> tempContentTypeOrder = new List<IAveContentType>();
                    foreach (SPContentType contentType in contentTypes)
                    {
                        tempContentTypeOrder.Add(new AveContentType(mWeb.ContentTypes as AveContentTypeCollection, contentType));
                    }
                    return tempContentTypeOrder;
                }
                else
                {
                    return null;
                }
            }
        }

        public IList<IAveContentType> UniqueContentTypeOrder
        {
            get
            {
                if (mUniqueContentTypeOrder == null)
                {
                    IList<SPContentType> contentTypes = mFolder.UniqueContentTypeOrder;
                    if (contentTypes != null)
                    {
                        mUniqueContentTypeOrder = new List<IAveContentType>();
                        foreach (SPContentType contentType in contentTypes)
                        {
                            mUniqueContentTypeOrder.Add(new AveContentType(mWeb.ContentTypes as AveContentTypeCollection, contentType));
                        }
                    }
                }
                return mUniqueContentTypeOrder;
            }
            set
            {
                mUniqueContentTypeOrder = value;
                if (mUniqueContentTypeOrder != null)
                {
                    List<SPContentType> uniqueContentTypeOrder = new List<SPContentType>();
                    foreach (IAveContentType aveContentType in mUniqueContentTypeOrder)
                    {
                        uniqueContentTypeOrder.Add((aveContentType as AveContentType).ContentType);
                    }
                    mFolder.UniqueContentTypeOrder = uniqueContentTypeOrder;
                }
                else
                {
                    mFolder.UniqueContentTypeOrder = null;
                }
            }
        }

        public void Delete()
        {
            mFolder.Delete();
        }

        public IAveList ParentList
        {
            get
            {
                if (m_ParentList == null)
                {
                    if (this.ParentListId == Guid.Empty)
                    {
                        return null;
                    }
                    m_ParentList = mWeb.Lists[this.ParentListId] as AveList;
                }
                return m_ParentList;
            }
            set
            {
                m_ParentList = value as AveList;
                if (m_ParentList != null)
                {
                    AveAssemblyUtility.SetFieldValue(mFolder, "m_parentList", m_ParentList.List);
                    m_ParentList.LoadFieldMap();
                }
            }
        }

        public List<AveHiddenFileInfo> HiddenFiles
        {
            get
            {
                if (mHiddenFiles == null)
                {
                    mHiddenFiles = mSite.QueryService.GetHiddenFiles(mSite.ID, mWeb.ID, ParentListId, mFolder.UniqueId);
                    mSite.DisposeConnection();  //TODOLMM temp modification for CM
                }
                return mHiddenFiles;
            }
        }

        public Guid Recycle()
        {
            return mFolder.Recycle();
        }

        public IAveAudit Audit
        {
            get
            {
                if (mAudit == null)
                {
                    mAudit = new AveAudit(mFolder.Audit);
                }
                return mAudit;
            }
        }

        public void Reload(bool force = true)
        {
            try
            {
                if (mFolder != null && (force || mFolder.ParentWeb != mWeb.Web))
                {
                    mFolder = mWeb.Web.GetFolder(mFolder.UniqueId);
                    m_ParentList = null;
                    mAudit = null;
                    mFiles = null;
                    mFolders = null;
                    mHiddenFiles = null;
                    mItem = null;
                    mParentFolder = null;
                    mSubFolders = null;
                    mUniqueContentTypeOrder = null;
                    //mUserDataJunctionCache = null;
                    //mUserDataJunctionCacheInited = false;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.ReloadFolderError, e.ToString());
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mItem != null)
            {
                mItem.Dispose();
                mItem = null;
            }
            if (this.dataCacheManager != null)
            {
                this.dataCacheManager.Dispose();
                this.dataCacheManager = null;
            }
        }

        #endregion

        #region For performance

        string maxSubLeafName;
        internal string MaxSubLeafName
        {
            get
            {
                if (maxSubLeafName == null)
                {
                    maxSubLeafName = mSite.QueryService.GetMaxSubLeafName(mSite.ID, this.UniqueId);
                }
                return maxSubLeafName;
            }
            set { maxSubLeafName = value; }
        }

        #endregion

        public List<int> GetItemsByColumnValue(string columnDisplayName, string value)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolder.GetItemsByColumnValue"))
            {

                if (m_ParentList.Fields.ContainsField(columnDisplayName))
                {
                    List<int> itemRowIds = new List<int>();
                    AveField field = m_ParentList.Fields[columnDisplayName] as AveField;
                    if (field.Type == AveFieldType.Text || field.Type == AveFieldType.User)
                    {
                        itemRowIds = mSite.QueryService.GetItemsByColumnValue(mSite.ID, this.m_ParentList.Id, this.UniqueId, field.ColName, value);
                    }
                    else
                    {
                        itemRowIds = null;
                    }
                    return itemRowIds;
                }
                return null;

            }

        }

        public IAveDocumentSet DocumentSet
        {
            get
            {
                return new AveDocumentSet(this);
            }
        }

        #region Data Cache

        public Dictionary<string, object> GetDocDataFromCache(AveBaseItemInfo itemInfo)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectServer.AveFolder.GetDocDataFromCache"))
            {

                try
                {
                    if (NeedCache(itemInfo))
                    {
                        return dataCacheManager.GetDocData(itemInfo, m_ParentList.ColNameCollection);
                    }
                    return null;
                }
                catch (AvePoint.Wrapper.Common.AveQueryException e)
                {
                    if (e.ErrorCode == -2)
                    {
                        this.EnableCache = false;
                    }
                    log.Warn("An error occurred while getting data from cache. Error: {0}", e);
                    return null;
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while getting data from cache. Error: {0}", ex);
                    return null;
                }

            }

        }

        public List<Dictionary<string, object>> GetUserDataFromCache(AveBaseItemInfo itemInfo)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectServer.AveFolder.GetUserDataFromCache"))
            {

                try
                {
                    if (NeedCache(itemInfo))
                    {
                        return dataCacheManager.GetUserData(itemInfo, m_ParentList.ColNameCollection);
                    }
                    return null;
                }
                catch (AvePoint.Wrapper.Common.AveQueryException e)
                {
                    if (e.ErrorCode == -2)
                    {
                        this.EnableCache = false;
                    }
                    log.Warn("An error occurred while getting data from cache. Error: {0}", e);
                    return null;
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while getting data from cache. Error: {0}", ex);
                    return null;
                }

            }

        }

        public Dictionary<string, object> GetVersionDataFromCache(AveBaseItemInfo itemInfo)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectServer.AveFolder.GetVersionDataFromCache"))
            {

                try
                {
                    if (NeedCache(itemInfo))
                    {
                        return dataCacheManager.GetVersionData(itemInfo, m_ParentList.ColNameCollection);
                    }
                    return null;
                }
                catch (AvePoint.Wrapper.Common.AveQueryException e)
                {
                    if (e.ErrorCode == -2)
                    {
                        this.EnableCache = false;
                    }
                    log.Warn("An error occurred while getting data from cache. Error: {0}", e);
                    return null;
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while getting data from cache. Error: {0}", ex);
                    return null;
                }

            }

        }

        public Dictionary<string, object> GetCurrentVersionDocDataFromCache(AveBaseItemInfo itemInfo)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectServer.AveFolder.GetCurrentVersionDocDataFromCache"))
            {

                try
                {
                    if (NeedCache(itemInfo))
                    {
                        return dataCacheManager.GetCurrentVersionDocData(itemInfo, m_ParentList.ColNameCollection);
                    }
                    return null;
                }
                catch (AvePoint.Wrapper.Common.AveQueryException e)
                {
                    if (e.ErrorCode == -2)
                    {
                        this.EnableCache = false;
                    }
                    log.Warn("An error occurred while getting data from cache. Error: {0}", e);
                    return null;
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while getting data from cache. Error: {0}", ex);
                    return null;
                }

            }

        }

        public List<int> GetDocVersionsFromCache(AveBaseItemInfo itemInfo)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectServer.AveFolder.GetDocVersionsFromCache"))
            {

                try
                {
                    if (NeedCache(itemInfo))
                    {
                        return dataCacheManager.GetDocVersions(itemInfo, m_ParentList.ColNameCollection);
                    }
                    return null;
                }
                catch (AvePoint.Wrapper.Common.AveQueryException e)
                {
                    if (e.ErrorCode == -2)
                    {
                        this.EnableCache = false;
                    }
                    log.Warn("An error occurred while getting data from cache. Error: {0}", e);
                    return null;
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while getting data from cache. Error: {0}", ex);
                    return null;
                }

            }

        }

        private bool NeedCache(AveBaseItemInfo itemInfo)
        {
            return this.EnableCache && itemInfo.RowId > 0 &&
                (itemInfo.ItemType == AveItemType.Document || itemInfo.ItemType == AveItemType.ListItem);
        }

        #region DataCacheManager
        [AveCodeReview("2012/5/20", "Sid.you@avepoint.com", "Qinglong.luo@avepoint.com",
            new string[] { "CHECK_LIST_ID_FA_1" }, "ADO-27432", true)]
        internal class AveDataCacheManager : IDisposable
        {
            #region=================Private Member=================
            private const int CAPACITY = 1000;
            private const int DODOUDLECOUNT = 6;
            #region Cache Data Structure
            private InternalCache docCache;
            private InternalCache versionCache;
            private Key currentKey;
            #endregion

            #region For Query
            protected Guid folderId;
            protected Guid siteId;
            private AveBaseType listBaseType;
            protected AveSite site;
            #endregion
            #endregion

            #region=================Constructor=================
            public AveDataCacheManager(AveFolder folder)
            {
                if (folder == null)
                {
                    throw new ArgumentNullException("folder");
                }
                if (folder.Exists)
                {
                    this.folderId = folder.UniqueId;
                    this.site = folder.mSite;
                    this.siteId = folder.mSite.ID;
                    this.listBaseType = folder.ParentListId == Guid.Empty ? AveBaseType.DocumentLibrary : folder.ParentList.BaseType;
                    this.docCache = new InternalCache("AllDocs");
                    this.versionCache = new InternalCache("AllDocVersions");
                }
                else
                {
                    folder.EnableCache = false;
                }
            }
            #endregion

            #region=================Interface Method=================
            public Dictionary<string, object> GetDocData(AveBaseItemInfo itemInfo, string ColNameCollection)
            {
                var dataCache = TryGetDataCache(itemInfo.RowId, itemInfo.Version, this.docCache, ColNameCollection);
                if (dataCache != null)
                {
                    return dataCache.DocData;
                }
                return null;
            }

            public Dictionary<string, object> GetVersionData(AveBaseItemInfo itemInfo, string ColNameCollection)
            {
                var dataCache = TryGetDataCache(itemInfo.RowId, itemInfo.Version, this.versionCache, ColNameCollection);
                if (dataCache != null)
                {
                    return dataCache.DocData;
                }
                return null;
            }

            public List<Dictionary<string, object>> GetUserData(AveBaseItemInfo itemInfo, string ColNameCollection)
            {
                var dataCache = TryGetDataCache(itemInfo.RowId, itemInfo.Version, itemInfo.PageVersion ? this.versionCache : this.docCache, ColNameCollection);
                if (dataCache != null)
                {
                    return dataCache.UserData;
                }
                return null;
            }

            public Dictionary<string, object> GetCurrentVersionDocData(AveBaseItemInfo itemInfo, string ColNameCollection)
            {
                Func<AveDataCollection, bool> predicate = (dataCollection) =>
                        dataCollection.DocData != null
                        && dataCollection.DocData.ContainsKey("IsCurrentVersion")
                        && string.Equals(bool.TrueString, dataCollection.DocData["IsCurrentVersion"].ToString(), StringComparison.Ordinal);
                var dataCache = TryGetDataCache(itemInfo.RowId, this.docCache, predicate, ColNameCollection);
                if (dataCache != null)
                {
                    return dataCache.DocData;
                }
                return null;
            }

            public List<int> GetDocVersions(AveBaseItemInfo itemInfo, string ColNameCollection)
            {
                InitDataCache(itemInfo.RowId, this.docCache, ColNameCollection);
                InitDataCache(itemInfo.RowId, this.versionCache, ColNameCollection);
                var list = this.docCache.GetKeysUnderRowId(itemInfo.RowId);
                list.AddRange(this.versionCache.GetKeysUnderRowId(itemInfo.RowId));
                return list;
            }
            #endregion

            #region=================Private Method=================
            private AveDataCollection TryGetDataCache(int rowId, int uiVersion, InternalCache internalCache, string ColNameCollection)
            {
                InitDataCache(rowId, internalCache, ColNameCollection);
                var key = new Key(rowId, uiVersion);
                AveDataCollection currentDataEntity = null;
                if (internalCache.TryGetValue(key, out currentDataEntity))
                {
                    Remove(internalCache, key);
                }
                return currentDataEntity;
            }

            private AveDataCollection TryGetDataCache(int rowId, InternalCache internalCache, Func<AveDataCollection, bool> predicate, string ColNameCollection)
            {
                InitDataCache(rowId, internalCache, ColNameCollection);
                return internalCache.TryGetValue(rowId, predicate);
            }

            private void Remove(InternalCache internalCache, Key key)
            {
                if (this.currentKey != key)
                {
                    internalCache.Remove(this.currentKey);
                    this.currentKey = key;//多次读取数据
                }
            }

            private void InitDataCache(int rowId, InternalCache internalCache, string ColNameCollection)
            {
                if (internalCache.NeedToQuery)
                {
                    if (internalCache.ItemCount == 0 || rowId >= internalCache.CurrentMaxIndex.RowId)//重新查询最后一个Item,查失败的item不会继续查
                    {
                        GetNextCacheBlock(internalCache, ColNameCollection, rowId);
                    }
                }
            }

            private void GetNextCacheBlock(InternalCache dataCache, string ColNameCollection, int rowId)
            {
                var count = CAPACITY / 2;
                Key currentMaxIndex;
                List<Dictionary<string, object>> collections;
                var smallestRowId = dataCache.CurrentMaxIndex.RowId < rowId ? rowId : dataCache.CurrentMaxIndex.RowId;//现在逻辑肯定是rowId>=dataCache.CurrentMaxIndex.RowId
                var doDoubleCount = DODOUDLECOUNT;//控制翻倍查找的次数，暂定6,则最高可查32000记录。
                var needContinue = false;
                do
                {
                    count += count;
                    collections = QueryDocAndUserData(smallestRowId, dataCache.TableName, count, ColNameCollection);
                    currentMaxIndex = AssemblyDataCacheCollection(collections, dataCache);
                    doDoubleCount--;
                    needContinue = collections.Count == count && currentMaxIndex.RowId == dataCache.CurrentMaxIndex.RowId;
                    if (needContinue && doDoubleCount == 0)
                    {
                        currentMaxIndex.RowId++;//+1 表示当前id的item获取失败，需从下一个id开始Cache数据。
                        dataCache.Clear();
                        break;
                    }
                }//currentMaxIndex=null, if collections.count =0
                while (needContinue);
                //如果collections.Count<count 或 两次查询的currentMaxIndex相等，就意味着数据库中的所有条目已经查询出来了
                dataCache.NeedToQuery = collections.Count == count && (currentMaxIndex != dataCache.CurrentMaxIndex);
                if (currentMaxIndex != null)
                {
                    dataCache.CurrentMaxIndex = currentMaxIndex;
                }
            }

            private Key AssemblyDataCacheCollection(List<Dictionary<string, object>> collections, InternalCache internalCache)
            {
                Key currentIndex = null;
                internalCache.Clear();
                foreach (var row in collections)
                {
                    var docData = new Dictionary<string, object>();
                    var userData = new Dictionary<string, object>();
                    currentIndex = GetRowData(row, docData, userData, internalCache.TableName);
                    internalCache.AddRowDataToCache(currentIndex, docData, userData);
                }
                return currentIndex;
            }

            private Key GetRowData(Dictionary<string, object> row, Dictionary<string, object> docData, Dictionary<string, object> userData, string tableName)
            {
                bool isDocTable = string.Equals(tableName, "AllDocs", StringComparison.OrdinalIgnoreCase);
                foreach (var column in row)
                {
                    string key = column.Key;
                    if (isDocTable && key.StartsWith("DOC#", StringComparison.OrdinalIgnoreCase))
                    {
                        docData.Add(key.Remove(0, "DOC#".Length), column.Value);
                    }
                    else if (!isDocTable && key.StartsWith("VER#", StringComparison.OrdinalIgnoreCase))
                    {
                        docData.Add(key.Remove(0, "VER#".Length), column.Value);
                    }
                    else if (key.StartsWith("UD#", StringComparison.OrdinalIgnoreCase))
                    {
                        userData.Add(key.Remove(0, "UD#".Length), column.Value);
                    }
                }
                return GenerateKey(row, isDocTable);
            }

            private List<Dictionary<string, object>> QueryDocAndUserData(int smallestRowId, string tableName, int count, string ColNameCollection)
            {
                switch (tableName)
                {
                    case "AllDocs":
                        log.Info("Get the data cache info. Initialize NO.1 cache. Smallest Id is {0}, count is {1}.", smallestRowId, count);
                        return this.site.QueryService.GetDocAndUserInfo(this.siteId, this.folderId, smallestRowId, count, ColNameCollection);
                    case "AllDocVersions":
                        log.Info("Get the data cache info. Initialize NO.2 cache. Smallest Id is {0}, count is {1}.", smallestRowId, count);
                        return this.site.QueryService.GetVersionAndUserInfo(this.siteId, this.folderId, smallestRowId, count, ColNameCollection);
                    default:
                        log.Info("Initialize no data cache.");
                        return new List<Dictionary<string, object>>();
                }
            }

            private Key GenerateKey(Dictionary<string, object> data, bool isDocTable)
            {
                int rowId = data.ContainsKey("DOC#DoclibRowId") ? (int)data["DOC#DoclibRowId"] : 0;
                if (isDocTable)
                {
                    return new Key(rowId, (int)data["DOC#UIVersion"]);
                }
                else
                {
                    return new Key(rowId, (int)data["VER#UIVersion"]);
                }
            }
            #endregion

            public void Dispose()
            {
                if (this.docCache != null)
                {
                    this.docCache.Dispose();
                    this.docCache = null;
                }
                if (this.versionCache != null)
                {
                    this.versionCache.Dispose();
                    this.versionCache = null;
                }
            }
        }

        [AveCodeReview("2012/5/20", "Sid.you@avepoint.com", "Qinglong.luo@avepoint.com",
            new string[] { "CHECK_LIST_ID_FA_1" }, "ADO-27432", true)]
        internal class InternalCache : IDisposable
        {
            public string TableName { get; private set; }
            public Key CurrentMaxIndex { get; set; }
            /// <summary>
            /// False if the previous query has got all the data entity.
            /// In order to hit preformance
            /// </summary>
            public bool NeedToQuery { get; set; }
            //private AveVolatileCache<Key, AveDataCollection> dataCache;
            private Dictionary<int, Dictionary<int, AveDataCollection>> dataCache;
            public int ItemCount
            {
                get { return this.dataCache.Count; }
            }
            public int Count
            {
                get { return this.dataCache.Values.Select(value => value.Count).Sum(); }
            }

            public List<int> Keys
            {
                get { return this.dataCache.Keys.ToList(); }
            }

            public List<int> GetKeysUnderRowId(int rowId)
            {
                using (var scope = new AvePerformanceScope("ObjectServer.AveFolder.InternalCache.GetKeysUnderRowId"))
                {
                    if (this.dataCache.Keys.Contains(rowId))
                    {
                        return this.dataCache[rowId].Keys.ToList();
                    }
                    return new List<int>();
                }
            }

            public InternalCache(string tableName)
            {
                //this.dataCache = new AveVolatileCache<Key, AveDataCollection>(tableName);
                this.dataCache = new Dictionary<int, Dictionary<int, AveDataCollection>>();
                this.CurrentMaxIndex = Key.MinValue;
                this.TableName = tableName;
                this.NeedToQuery = true;
            }

            public Key AddRowDataToCache(Key key, Dictionary<string, object> docData, Dictionary<string, object> userData)
            {
                if (!this.dataCache.ContainsKey(key.RowId))
                {
                    this.dataCache.Add(key.RowId, new Dictionary<int, AveDataCollection>());
                }
                var rowIds = this.dataCache[key.RowId];
                if (!rowIds.ContainsKey(key.UIVersion))
                {
                    rowIds.Add(key.UIVersion, new AveDataCollection()
                    {
                        DocData = docData,
                    });
                }
                //一条AllDocs里面的记录可能对应多条AllUserData中的记录，因此AllDocs的记录可能是重复的
                if (userData.Count > 0)
                {
                    rowIds[key.UIVersion].UserData.Add(userData);
                }
                return key;
            }

            public bool TryGetValue(Key key, out AveDataCollection dataCollection)
            {
                dataCollection = null;
                if (key == null)
                {
                    return false;
                }
                if (this.dataCache.ContainsKey(key.RowId) && this.dataCache[key.RowId].ContainsKey(key.UIVersion))
                {
                    dataCollection = this.dataCache[key.RowId][key.UIVersion];
                    return true;
                }
                return false;
                //return this.dataCache.TryGetValue(key, out dataCollection);
            }

            public AveDataCollection TryGetValue(int rowId, Func<AveDataCollection, bool> predicate)
            {
                if (this.dataCache.ContainsKey(rowId))
                {
                    return this.dataCache[rowId].Values.Where(value => predicate(value)).FirstOrDefault();
                }
                return null;
            }

            public bool Remove(Key key)
            {
                if (key == null)
                {
                    return false;
                }
                if (this.dataCache.ContainsKey(key.RowId))
                {
                    try
                    {
                        return this.dataCache[key.RowId].Remove(key.UIVersion);
                    }
                    finally
                    {
                        if (this.dataCache[key.RowId].Count == 0)
                        {
                            this.dataCache.Remove(key.RowId);
                        }
                    }
                }
                return false;

            }

            public void Clear()
            {
                if (this.dataCache != null && this.dataCache.Count > 0)
                {
                    this.dataCache.Clear();
                }
            }

            public void Dispose()
            {
                if (this.dataCache != null)
                {
                    this.dataCache.Clear();
                    this.dataCache = null;
                }
            }
        }

        internal class AveDataCollection
        {
            public Dictionary<string, object> DocData { get; set; }
            public List<Dictionary<string, object>> UserData { get; set; }

            public AveDataCollection()
            {
                UserData = new List<Dictionary<string, object>>();
            }
        }

        internal class Key
        {
            //public byte Type { get; private set; }
            public int RowId { get; set; }
            //public string LeafName { get; private set; }//for doc lib:LeafName, for list: DocLibRowId
            public int UIVersion { get; private set; }
            public static Key MinValue
            {
                get
                {
                    return new Key(0, 0);
                }
            }

            public Key(int rowId, int uiVersion)
            {
                //this.Type = type;
                this.RowId = rowId;
                //this.LeafName = leafName;
                this.UIVersion = uiVersion;
            }

            public override bool Equals(object obj)
            {
                var keyObject = obj as Key;
                if (keyObject == null)
                {
                    return false;
                }
                return //this.Type == keyObject.Type &&
                    this.RowId == keyObject.RowId &&
                    //string.Equals(this.LeafName, keyObject.LeafName) &&
                    this.UIVersion == keyObject.UIVersion;
            }

            public override int GetHashCode()
            {
                return //(this.Type.GetHashCode() & 0xFF << 24) |
                    (this.RowId.GetHashCode() & 0xFFFF << 16) |
                    //(this.LeafName.GetHashCode() & 0xFF << 8) |
                    (this.UIVersion.GetHashCode() & 0xFFFF);
            }

            #region operator
            public static bool operator ==(Key k1, Key k2)
            {
                if (object.Equals(k1, null))
                {
                    return object.Equals(k2, null);
                }
                return k1.Equals(k2);
            }

            public static bool operator !=(Key k1, Key k2)
            {
                return !(k1 == k2);
            }

            public static bool operator >(Key k1, Key k2)
            {
                if (k1 == null)
                {
                    return false;
                }
                if (k2 == null)
                {
                    return true;
                }
                if (k1.RowId == k2.RowId)
                {
                    return k1.UIVersion > k2.UIVersion;

                }
                return k1.RowId > k2.RowId;
            }

            public static bool operator >=(Key k1, Key k2)
            {
                return !(k1 < k2);
            }

            public static bool operator <(Key k1, Key k2)
            {
                return k2 > k1;
            }

            public static bool operator <=(Key k1, Key k2)
            {
                return !(k1 > k2);
            }
            #endregion

        }
        public bool mUserDataJunctionCacheInited;
        private int mUserDataJunctionCacheMaxRow = 1000;
        public int UserDataJunctionCacheMaxRow
        {
            get
            {
                return mUserDataJunctionCacheMaxRow;
            }
            set
            {
                if (value > 0)
                {
                    mUserDataJunctionCacheMaxRow = value;
                }
            }
        }
        Dictionary<Guid, Dictionary<int, List<Dictionary<string, object>>>> mUserDataJunctionCache;
        internal List<Dictionary<string, object>> GetUserDataJunctionFromCache(AveBaseItemInfo baseItemInfo)
        {
            if (m_ParentList != null && m_ParentList.List != null && (m_ParentList.Fields as AveFieldCollection).ContainsLookupField)
            {
                if (!mUserDataJunctionCacheInited)
                {
                    lock (this)
                    {
                        if (!mUserDataJunctionCacheInited)
                        {
                            mUserDataJunctionCacheInited = true;
                            try
                            {
                                mUserDataJunctionCache = mSite.QueryService.GetFolderItemsUserDataJunctions(this.mSite.ID, this.UniqueId, mUserDataJunctionCacheMaxRow);
                            }
                            catch (Exception ex)
                            {
                                log.Warn("An error occurred when getting user data junction cache for folder: {0}, Reason: {1}.", this.ServerRelativeUrl, ex);
                            }
                        }
                    }
                }
                Dictionary<int, List<Dictionary<string, object>>> itemData;
                if (mUserDataJunctionCache != null && mUserDataJunctionCache.TryGetValue(baseItemInfo.GUID, out itemData))
                {
                    List<Dictionary<string, object>> result;
                    if (itemData.TryGetValue(baseItemInfo.Version, out result))
                    {
                        return result;
                    }
                }
                return mSite.QueryService.GetUserDataJunction(baseItemInfo);
            }
            return null;
        }
        #endregion
        #endregion
    }
}
