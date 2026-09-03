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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Runtime.Remoting.Messaging;
using AvePoint.ObjectModel.Common;
using AvePoint.Wrapper;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/02/29", "gqsun@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    [AveCodeReview("2012/03/08", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    internal class AveDiscoveryQuery : IAveDiscoveryQuery
    {
        private AveBPOSAccountInfo mUserAccountInfo;
        private string mSiteUrl;
        private string mSPVersion;
        private readonly DateTime mStartTime;
        private readonly DateTime mEndTime;
        private IAveRequest mRequest;
        private AveRequestParameter mRequestParameter;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveDiscoveryQuery));
        public bool SupportIB { get; set; }
        private Dictionary<string, object> mChangeCache = new Dictionary<string, object>();
        private Dictionary<Guid, object> mWebParts = new Dictionary<Guid, object>();
        private bool mQuerySite = false;
        //[ADO-25849]Replicator BPOS-S运行时间过长，添加缓存，Dictionary的count不大于1
        private Dictionary<AveQueryCacheForReplicator, Dictionary<string, object>> mItemsAndFoldersCacheForReplicator = null;

        public IAveRequest Request
        {
            get
            {
                return mRequest;
            }
        }

        public void Dispose()
        {
            AveRequestInterceptor.DisposeAvailableRequest(mRequestParameter, mSiteUrl, mUserAccountInfo.GetAccountName());
            mChangeCache.Clear();
        }

        #region Construct Function
        public AveDiscoveryQuery()
        {
            SupportIB = false;
        }

        public AveDiscoveryQuery(string siteUrl, AveBPOSAccountInfo account, DateTime startTime, DateTime endTime)
            : this(siteUrl, null, account, startTime, endTime, false, new Dictionary<string, object>())
        { }

        public AveDiscoveryQuery(string siteUrl, AveBPOSAccountInfo account)
            : this(siteUrl, null, account, DateTime.MinValue, DateTime.MinValue, false, new Dictionary<string, object>())
        { }

        public AveDiscoveryQuery(IAveSite site)
            : this(site.Url, (site as AveSite).RequestParameter, (site as AveSite).UserAccountInfo, DateTime.MinValue, DateTime.MinValue, false, new Dictionary<string, object>())
        { }

        public AveDiscoveryQuery(IAveSite site, AveBPOSAccountInfo account)
            : this(site.Url, (site as AveSite).RequestParameter, account, DateTime.MinValue, DateTime.MinValue, false, new Dictionary<string, object>())
        { }

        public AveDiscoveryQuery(IAveSite site, DateTime startTime, DateTime endTime)
            : this(site.Url, (site as AveSite).RequestParameter, (site as AveSite).UserAccountInfo, startTime, endTime, false, new Dictionary<string, object>())
        { }

        public AveDiscoveryQuery(IAveSite site, AveBPOSAccountInfo account, DateTime startTime, DateTime endTime)
            : this(site.Url, (site as AveSite).RequestParameter, account, startTime, endTime, false, new Dictionary<string, object>())
        { }

        private AveDiscoveryQuery(string siteUrl, AveRequestParameter requestParameter, AveBPOSAccountInfo account, DateTime startTime, DateTime endTime, bool supportIB, Dictionary<string, object> changes)
        {
            mSiteUrl = siteUrl;
            mUserAccountInfo = account;
            mStartTime = startTime;
            mEndTime = endTime;

            mRequestParameter = requestParameter;
            if (mRequestParameter == null || mRequestParameter.AveRequest == null)
            {
                InitRequest();
                mRequestParameter = new AveRequestParameter(mRequest, mSPVersion);
            }
            else
            {
                mRequest = requestParameter.AveRequest;
            }
            SupportIB = supportIB;
            mChangeCache = changes;
        }
        #endregion

        #region Init Request

        private void InitRequest()
        {
            AveRequestInterceptor request = new AveRequestInterceptor(mSiteUrl, mUserAccountInfo);
            mRequest = request.Proxy;
            mSPVersion = request.SPVersion;
        }
        #endregion

        #region Init List/Web
        public void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.InitDiscoverWeb"))
            {
                string webServerRelativeUrl = string.IsNullOrEmpty(webObj.FullUrl) ? webObj.FullUrl : "/" + webObj.FullUrl.TrimStart('/');
                IAveWeb web = webCache.AveSite.OpenWeb(webServerRelativeUrl);
                this.InitWebObject(webObj, webCache, web);
            }
        }

        public void InitDiscoverList(AveListCache listCache, AveListObject listObj)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.InitDiscoverList"))
            {
                IAveList list = listCache.AveWeb.GetList(listObj.RootFolderUrl);
                this.InitListObject(listObj, listCache, list);
            }
        }

        public void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj,ref AveListObject parentListObject)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.InitDiscoverFolder"))
            {
                IAveFolder folder = folderCache.AveWeb.GetFolder(folderObj.FullUrl);
                this.InitFolderObject(folderObj, folderCache, folder);
            }
        }
        #endregion

        #region Site Level
        public Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryWebForIB"))
            {
                Dictionary<Guid, AveWebObject> webs = new Dictionary<Guid, AveWebObject>();
                //需要加载，否则无法找到site下面变化的web数据
                if (!mQuerySite)
                {
                    GetSiteChangedForIB(siteId);
                    mQuerySite = true;
                }
                if (mChangeCache.ContainsKey("ChangedWebCache"))
                {
                    Dictionary<Guid, object> webProperties = mRequest.QueryWebForIB(mChangeCache["ChangedWebCache"] as Dictionary<Guid, object>);
                    ConvertWebObjects(webs, webProperties);
                }
                return webs;
            }
        }

        public Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QuerySiteSecurityForIB"))
            {
                Dictionary<int, AveSiteMemberObject> members = new Dictionary<int, AveSiteMemberObject>();
                Dictionary<int, object> memberProperties = mRequest.QuerySiteSecurityForIB(siteId, mStartTime, mEndTime);
                if (memberProperties != null)
                {
                    ConvertSiteMemberObjects(members, memberProperties);
                }
                return members;
            }
        }
        #endregion

        #region Web Level
        public void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryWebRootFolder"))
            {
                Dictionary<string, object> folder = mRequest.QueryWebRootFolder(listCache.WebId);
                ConvertItemObject(rootFolderObject, folder);
                rootFolderObject.ObjType = ItemType.Folder;
                rootFolderObject.DirName = folder["DirName"].ToString();
                rootFolderObject.FullUrl = folder["FullUrl"].ToString();
            }
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            return mRequest.GetListChangedItems(webId, listId, startTime, endTime);
        }
        public Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryListForIB"))
            {
                Dictionary<Guid, AveListObject> changedLists = new Dictionary<Guid, AveListObject>();
                if (mChangeCache.ContainsKey("ChangedListCache"))
                {
                    Dictionary<Guid, object> listsProp = mRequest.QueryListForIB(webId, mChangeCache["ChangedListCache"] as Dictionary<Guid, object>);
                    ConvertListObjects(changedLists, listsProp);
                }
                return changedLists;
            }
        }

        public AveWebObject QueryRootWeb(Guid siteId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryRootWeb"))
            {
                Dictionary<string, object> webProperty = mRequest.QueryRootWeb(siteId);
                AveWebObject web = new AveWebObject();
                ConvertWebObject(web, webProperty);
                return web;
            }
        }

        public Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.GetSubWebs"))
            {
                Dictionary<Guid, AveWebObject> webs = new Dictionary<Guid, AveWebObject>();
                Dictionary<Guid, object> webProperties = mRequest.GetSubWebs(siteId, parentWebId);
                ConvertWebObjects(webs, webProperties);
                return webs;
            }
        }
        #endregion

        #region list level
        public void QueryListRootFolder(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryListRootFolder"))
            {
                Dictionary<string, object> folder = mRequest.QueryListRootFolder(listCache.SiteId, listCache.WebId, listCache.ListId);
                ConvertItemObject(rootFolderObject, folder);
                rootFolderObject.ObjType = ItemType.Folder;
                rootFolderObject.DirName = folder["DirName"].ToString();
                rootFolderObject.FullUrl = folder["FullUrl"].ToString();
            }
        }

        public Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            QuerySystemListItemForIB(folderCache, folderObject, extraItems);
        }
        [Obsolete("no use now, will remove later")]
        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DiscoverModeForSOIB discoverMode,
            List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            throw new NotImplementedException();
        }

        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryListItemForIB"))
            {
                Dictionary<string, object> changedItems = new Dictionary<string, object>();
                changedItems = mRequest.QueryListItemForIB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, mChangeCache["ChangedItemsCache"] as Dictionary<string, object>);
                //bool isUnderWebRootFolder = folderCache.ParentList.ListID == Guid.Empty ? true : false;
                bool isUnderWebRootFolder = folderCache.ListId == Guid.Empty ? true : false;
                FillFolderObject(changedItems, folderObject, isUnderWebRootFolder);
            }
        }
        [Obsolete("no use now, will remove later")]
        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject,
            DiscoverModeForSOIB discoverMode, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            QueryListItemForIB(folderCache, folderObject,listObject,extraItems);
        }

        public void QueryChangedListItemFromExtraItemList(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject,
            List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid siteId, Guid webId, Guid listId, int itemId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QuerySiteWebForFB"))
            {
                Dictionary<Guid, AveWebObject> allWebs = new Dictionary<Guid, AveWebObject>();
                Dictionary<string, object> allWebProperties = mRequest.GetAllWebs();
                List<Dictionary<string, object>> webPropertiesList = (List<Dictionary<string, object>>)allWebProperties[AveObjectModelConstant.ChildrenProperties];
                foreach (Dictionary<string, object> webProperties in webPropertiesList)
                {
                    AveWebObject web = new AveWebObject();
                    web.WebID = (Guid)webProperties["Id"];
                    string name = webProperties["Name"].ToString();
                    if (string.IsNullOrEmpty(name))
                    {
                        name = ".";
                    }
                    web.Name = name;
                    web.Title = webProperties["Title"].ToString();
                    web.FullUrl = webProperties["ServerRelativeUrl"].ToString().TrimStart('/');
                    object value;
                    if (webProperties.TryGetValue("IsAppWeb", out value))
                    {
                        web.IsAppWeb = (bool)value;
                    }
                    if (webProperties.TryGetValue("AppInstanceId", out value))
                    {
                        web.AppInstanceId = (Guid)value;
                    }

                    allWebs.Add((Guid)webProperties["Id"], web);
                }
                return allWebs;
            }
        }

        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId)
        {
            return QueryWebListForFB(siteId, webId, false);
        }

        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool includeRecycleBin)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryWebListForFB"))
            {
                Dictionary<Guid, object> listProperties = mRequest.QueryWebListForFB(siteId, webId);
                Dictionary<Guid, AveListObject> lists = new Dictionary<Guid, AveListObject>();
                ConvertListObjects(lists, listProperties);
                return lists;
            }
        }

        public Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryListViewForFB"))
            {
                Dictionary<Guid, AveViewObject> views = new Dictionary<Guid, AveViewObject>();
                Dictionary<Guid, object> viewProperties = mRequest.QueryListViewForFB(siteId, webId, listId);
                ConvertViewObjects(views, viewProperties);
                return views;
            }
        }
        #endregion

        #region Item Level
        public int GetSiteChangedForIB(Guid siteId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.GetSiteChangedForIB"))
            {
                int changeType = mRequest.GetSiteChangedForIB(siteId, mStartTime, mEndTime, mChangeCache);
                return changeType;
            }
        }

        /// <summary>
        /// 获取Site本身的改变信息，还有User 以及Group的改变
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="siteCollectionChangeType"></param>
        /// <param name="userChangeType"></param>
        /// <param name="groupChangeType"></param>
        /// <returns></returns>
        public bool GetSiteChangedForIB(Guid siteId, ref ChangeType siteCollectionChangeType, ref ChangeType userChangeType, ref ChangeType groupChangeType)
        {
            bool changed = false;

            if (!mQuerySite)
            {
                GetSiteChangedForIB(siteId);
                mQuerySite = true;
            }

            object tempObj = null;

            if (mChangeCache != null && mChangeCache.TryGetValue("ChangedSiteCache", out tempObj))
            {
                Dictionary<Guid, object> siteCaches = (Dictionary<Guid, object>)tempObj;

                if (siteCaches != null && siteCaches.TryGetValue(siteId, out tempObj))
                {
                    Dictionary<string, object> siteCache = (Dictionary<string, object>)tempObj;
                    siteCollectionChangeType = (ChangeType)siteCache["ChangeType"];

                    if (siteCollectionChangeType != ChangeType.None)
                    {
                        changed = true;
                    }

                    if (siteCache.TryGetValue("UserChangeType", out tempObj))
                    {
                        userChangeType = (ChangeType)tempObj;
                    }
                    if (siteCache.TryGetValue("GroupChangeType", out tempObj))
                    {
                        groupChangeType = (ChangeType)tempObj;
                    }
                }
            }

            return changed;
        }
        /// <summary>
        /// Only Used in discover.
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="parentListObject"></param>
        /// <param name="includeRecycleBin"></param>
        /// <param name="includeSystemFolder"></param>
        public void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin, bool includeSystemFolder, bool includeVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryListItemForFB"))
            {
                bool needContineQuery = folderObject.FolderChildCount.HasValue ? folderObject.FolderChildCount.Value > 0 : true;
                needContineQuery |= folderObject.ItemChildCount.HasValue ? folderObject.ItemChildCount.Value > 0 : true;
                if (needContineQuery)
                {
                    Dictionary<string, object> folderProperty = mRequest.QueryListItemForFB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, !WrapperConfiguration.BPOS_S.QueryAllPropertiesInDiscver, includeSystemFolder);
                    bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
                    FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
                }
            }
        }

        public void DiscoverAllListContent(AveListCache listCache, AveItemObject rootFolderObj, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.DiscoverAllListContent"))
            {
                Dictionary<string, object> allObjects = null;
                try
                {
                    allObjects = mRequest.DiscoverAllListContent(listCache.SiteId, listCache.WebId, listCache.ListId, maxItemCount, includeRecycleBin, includeSystemFolder);
                    bool isUnderWebRootFolder = listCache.ListId.Equals(Guid.Empty) ? true : false;
                    FillFolderObject(allObjects, rootFolderObj, isUnderWebRootFolder, true);
                    MarkDiscoverFolder(rootFolderObj);
                }
                catch(Exception e)
                {
                    mLogger.Warn("Query all list content failed. Error: {0}", e);
                }
            }
        }
        public void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin)
        {
            throw new NotImplementedException();
        }

        public int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject, AveListObject ParentListObject, bool includeRecycleBin = false)
        {
            throw new NotImplementedException();
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.QueryWebContentTypeForFB"))
            {
                Dictionary<byte[], AveContentTypeObject> contentTypes = new Dictionary<byte[], AveContentTypeObject>();
                Dictionary<byte[], object> contentTypeProperties = mRequest.QueryWebContentTypeForFB(siteId, webId);
                ConvertContentTypeObjects(contentTypes, contentTypeProperties);
                return contentTypes;
            }
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl)
        {
            throw new NotImplementedException();
        }
        
        public string GetListContentTypes(Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, int? maxMajorwithMinorVersionCount)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.GetItemExist"))
            {
                AveItemObject itemObject = new AveItemObject();
                //[ADO-25849]从缓存中获取Item的信息
                Dictionary<string, object> item = null;
                #region get content from buffer if cached
                Dictionary<string, object> foldersAndItems;
                AveQueryCacheForReplicator cacheInfo = GetAveQueryCacheForReplicator(SiteId, webId, listId, Guid.Empty, dirName);
                if (mItemsAndFoldersCacheForReplicator != null && mItemsAndFoldersCacheForReplicator.TryGetValue(cacheInfo, out foldersAndItems))
                {
                    object folders;
                    object items;
                    if (id != Guid.Empty && isListItem)
                    {
                        if (foldersAndItems.TryGetValue("Items", out items))
                        {
                            foreach (Dictionary<string, object> itemDic in items as List<Dictionary<string, object>>)
                            {
                                if (itemDic.ContainsKey("DocID") && (Guid)itemDic["DocID"] == id)
                                {
                                    item = itemDic;
                                    break;
                                }
                            }
                        }
                    }
                    else if (!isListItem)
                    {
                        if (foldersAndItems.TryGetValue("Items", out items))
                        {
                            foreach (Dictionary<string, object> itemDic in items as List<Dictionary<string, object>>)
                            {
                                if (itemDic.ContainsKey("FileDirRef") && itemDic.ContainsKey("Name") && dirName.Equals(itemDic["FileDirRef"].ToString().TrimStart(new char[] { '/' }), StringComparison.Ordinal) && leafName.Equals(itemDic["Name"].ToString(), StringComparison.Ordinal))
                                {
                                    item = itemDic;
                                    break;
                                }
                            }
                        }
                        if (foldersAndItems.TryGetValue("Folders", out folders))
                        {
                            foreach (Dictionary<string, object> folderDic in folders as List<Dictionary<string, object>>)
                            {
                                if (folderDic.ContainsKey("FileDirRef") && folderDic.ContainsKey("Name") && dirName.Equals(folderDic["FileDirRef"].ToString().TrimStart(new char[] { '/' }), StringComparison.Ordinal) && leafName.Equals(folderDic["Name"].ToString(), StringComparison.Ordinal))
                                {
                                    item = folderDic;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (item == null && mItemsAndFoldersCacheForReplicator != null && mItemsAndFoldersCacheForReplicator.ContainsKey(cacheInfo))
                {
                    return null;
                }
                #endregion
                if (item == null)
                {
                    item = mRequest.GetItemExist(SiteId, webId, listId, id, dirName, leafName, isListItem);
                }
                if (item != null)
                {
                    ConvertItemObject(itemObject, item);
                    if (itemObject.ID.HasValue)
                    {
                        object tempItemObj;
                        if (item.TryGetValue("Versions", out tempItemObj))
                        {
                            foreach (Dictionary<string, object> dicVersion in tempItemObj as List<Dictionary<string, object>>)
                            {
                                var version = new AveVersionObject();
                                ConvertVersionObject(version, dicVersion);
                                itemObject.VersionObjs.Add(version);
                            }
                        }
                        //[ADO-25849]初始化itemObject的AttachmentObjs属性
                        if (item.TryGetValue("Attachments", out tempItemObj))
                        {
                            foreach (Dictionary<string, object> dicAttachment in tempItemObj as List<Dictionary<string, object>>)
                            {
                                var attachment = new AveItemObject();
                                ConvertAttachmentObject(attachment, dicAttachment);
                                itemObject.AttachmentObjs.Add(attachment);
                            }
                        }
                    }
                }
                else
                {
                    itemObject = null;
                }
                return itemObject;
            }
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.GetItemLastModifiedTimeForLeafName"))
            {
                return mRequest.GetItemLastModifiedTime(siteId, webId, listId, dirName, leafName, ref docId);
            }
        }

        public AveItemObject GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId)
        {
            throw new NotImplementedException();
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.GetDocIdByTp_Guid"))
            {
                return mRequest.GetDocIdByTp_Guid(siteId, webId, listId, parentId, tp_Guid, rowId);
            }
        }

        public Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl)
        {
            
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.GetTPGUIDAndDocIdMapping"))
            {
                var itemsMapping = new Dictionary<Guid, Guid>();
                try
                {
                    Dictionary<string, object> folderProperty = mRequest.QueryListItemForFB(siteId, webId, listId, parentId, folderUrl, false, false);
                    //由于现在wrapper的逻辑是restore的时候不应该有discover，如果有discover会导致无法refreshcontext，先暂时用这个方法处理
                    mRequest.Dispose(true);
                    object folders;
                    object items;
          
                    if (folderProperty.TryGetValue("Folders", out folders))
                    {
                        foreach (Dictionary<string, object> folderDic in folders as List<Dictionary<string, object>>)
                        {
                            itemsMapping[(Guid)folderDic["GUID"]] = (Guid)folderDic["DocID"];
                        }
                    }
                    if (folderProperty.TryGetValue("Items", out items))
                    {
                        foreach (Dictionary<string, object> dicItem in items as List<Dictionary<string, object>>)
                        {
                            itemsMapping[(Guid)dicItem["GUID"]] = (Guid)dicItem["DocID"];
                        }
                    }
                    //[ADO-25849]Replicator BPOS-S运行时间过长，缓存QueryListItemForFB的返回值
                    AveQueryCacheForReplicator cacheInfo = GetAveQueryCacheForReplicator(siteId, webId, listId, Guid.Empty, folderUrl);
                    Dictionary<AveQueryCacheForReplicator, Dictionary<string, object>> folderPropertyCache = new Dictionary<AveQueryCacheForReplicator, Dictionary<string, object>>();
                    folderPropertyCache.Add(cacheInfo, folderProperty);
                    mItemsAndFoldersCacheForReplicator = folderPropertyCache;
                }
                catch (Exception e)
                {
                    mRequest.Dispose(true);
                    mLogger.Error(e.ToString());
                }
                return itemsMapping;
            }        
            //bool isUnderWebRootFolder = folderCache.ParentList.ListID.Equals(Guid.Empty) || folderCache.ParentList == null ? true : false;
            //FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
            //throw new NotImplementedException();
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId)
        {
            throw new NotImplementedException();
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {
            throw new NotImplementedException();
        }

        public bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.IsHaveSameName"))
            {
                return mRequest.IsHaveSameName(webId, listId, dirName, leafName);
            }
        }

        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.IsListItemHaveSameName"))
            {
                return mRequest.IsListItemHaveSameName(siteId, webId, tpGuid, listId, rowId);
            }
        }

        /// <summary>
        /// 获取ListId对应list下所有的view文件对应的webparts属性；
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemDocId"></param>
        /// <returns></returns>
        public List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.ObjectModel.Common.AveDiscoveryQuery.GetItemWebParts"))
            {
                List<AveWebPartObject> webparts = new List<AveWebPartObject>();
                Dictionary<string, object> webpartsProperties = new Dictionary<string, object>();
                if (mWebParts.ContainsKey(listId))
                {
                    webpartsProperties = mWebParts[listId] as Dictionary<string, object>;
                }
                else
                {
                    mWebParts.Clear();//以后如果需要都保留就不clear;
                    webpartsProperties = mRequest.GetItemWebParts(siteId, webId, listId, itemDocId);
                    mWebParts.Add(listId, webpartsProperties);
                }
                convertWebparts(webparts, webpartsProperties.ContainsKey(itemDocId.ToString()) ? webpartsProperties[itemDocId.ToString()] as List<Dictionary<string, object>> : new List<Dictionary<string, object>>());
                return webparts;
            }
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 根据接收到的信息初始化webparts属性值
        /// </summary>
        /// <param name="webparts"></param>
        /// <param name="webpartsProperties"></param>
        public void convertWebparts(List<AveWebPartObject> webparts, List<Dictionary<string, object>> webpartsProperties)
        {
            foreach (Dictionary<string, object> webpartProperty in webpartsProperties)
            {
                var webpart = new AveWebPartObject();
                webpart.Id = (Guid)webpartProperty["Id"];
                webpart.DisplayName = webpartProperty["DisplayName"].ToString();
                webpart.Flags = (int)webpartProperty["Flags"];
                webpart.ZoneId = webpartProperty["ZoneId"] == null ? webpartProperty["ZoneId"].ToString() : null;
                webpart.AllUsersProperties = webpartProperty["AllUsersProperties"] != null ? (byte[])webpartProperty["AllUsersProperties"] : null;
                webpart.PerUserProperties = webpartProperty["PerUserProperties"] != null ? (byte[])webpartProperty["PerUserProperties"] : null;
                webpart.IsIncluded = (bool)webpartProperty["IsIncluded"];
                webpart.PartOrder = (int)webpartProperty["PartOrder"];
                webpart.View = webpartProperty["View"] != null ? (byte[])webpartProperty["View"] : null;
                webparts.Add(webpart);
            }
        }

        public void QueryAttachmentByItemObj(Guid siteId, string listRootFolderUrl, AveItemObject itemObj, IAveWeb web, Guid listId)
        {
            return;
        }
        #endregion

        #region Private Method
        private void ConvertWebObjects(Dictionary<Guid, AveWebObject> webs, Dictionary<Guid, object> webProperties)
        {
            foreach (KeyValuePair<Guid, object> webProperty in webProperties)
            {
                AveWebObject web = new AveWebObject();
                ConvertWebObject(web, (Dictionary<string, object>)webProperty.Value);
                webs.Add(webProperty.Key, web);
            }
        }

        private void ConvertWebObject(AveWebObject web, Dictionary<string, object> webProperty)
        {
            web.WebID = webProperty.ContainsKey("WebID") ? (Guid)webProperty["WebID"] : Guid.Empty;
            web.Name = webProperty.ContainsKey("Name") ? webProperty["Name"].ToString() : string.Empty;
            web.FullUrl = webProperty.ContainsKey("FullUrl") ? webProperty["FullUrl"].ToString().TrimStart('/') : string.Empty;
            web.EventTime = webProperty.ContainsKey("EventTime") ? (DateTime)webProperty["EventTime"] : DateTime.MinValue;
            web.Title = webProperty.ContainsKey("Title") ? webProperty["Title"].ToString() : string.Empty;
            web.NavigationChanged = webProperty.ContainsKey("NavigationChanged") ? (bool)webProperty["NavigationChanged"] : false;
            web.ChangeType = webProperty.ContainsKey("ChangeType") ? (ChangeType)webProperty["ChangeType"] : ChangeType.None;
            web.ContentTypeChangeType = webProperty.ContainsKey("ContentTypeChangeType") ? (ChangeType)webProperty["ContentTypeChangeType"] : ChangeType.None;
            web.ColumnChangeType = webProperty.ContainsKey("ColumnChangeType") ? (ChangeType)webProperty["ColumnChangeType"] : ChangeType.None;
            web.NavigationChangeType = web.NavigationChanged ? ChangeType.Edit : ChangeType.None;
            web.PermissionLevelChangeType = webProperty.ContainsKey("PermissionLevelChangeType") ? (ChangeType)webProperty["PermissionLevelChangeType"] : ChangeType.None;
            web.RoleAssignmentsChangeType = webProperty.ContainsKey("RoleAssignmentsChangeType") ? (ChangeType)webProperty["RoleAssignmentsChangeType"] : ChangeType.None;
            web.IsAppWeb = webProperty.ContainsKey("IsAppWeb") ? (bool)webProperty["IsAppWeb"] : false;
            web.AppInstanceId = webProperty.ContainsKey("AppInstanceId") ? (Guid)webProperty["AppInstanceId"] : Guid.Empty;
        }

        private void ConvertSiteMemberObjects(Dictionary<int, AveSiteMemberObject> members, Dictionary<int, object> memberProperties)
        {
            foreach (KeyValuePair<int, object> memberProperty in memberProperties)
            {
                AveSiteMemberObject memberOjbect = new AveSiteMemberObject();
                ConvertSiteMemberObject(memberOjbect, (Dictionary<string, object>)memberProperty.Value);
                members.Add(memberProperty.Key, memberOjbect);
            }
        }

        private void ConvertSiteMemberObject(AveSiteMemberObject memberObj, Dictionary<string, object> memberProperty)
        {
            memberObj.PrincipleId = (int)memberProperty["PrincipleId"];
            object member = false;
            memberProperty.TryGetValue("IsUser", out member);
            memberObj.IsUser = (bool)member;
            memberObj.IsGroup = !(bool)member;
            memberObj.ChangeType = (ChangeType)memberProperty["ChangeType"];
            memberObj.EventTime = (DateTime)memberProperty["Time"];
            object addedMemberIds;
            object deletedMemberIds;
            if (memberProperty.TryGetValue("AddedMemberIds", out addedMemberIds))
            {
                foreach (Dictionary<string, object> addedMember in (addedMemberIds as Dictionary<int, object>).Values)
                {
                    AveSiteMemberObject user = new AveSiteMemberObject { PrincipleId = (int)addedMember["PrincipleId"], IsUser = true };
                    user.IsDomainGroup = (bool)addedMember["IsDomainGroup"];
                    user.Login = addedMember["Login"].ToString();
                    user.Title = addedMember["Title"].ToString();
                    memberObj.AddedMemberIds.Add(user.PrincipleId, user);
                }
            }
            if (memberProperty.TryGetValue("DeletedMemberIds", out deletedMemberIds))
            {
                foreach (Dictionary<string, object> deletedMember in (memberProperty["DeletedMemberIds"] as Dictionary<int, object>).Values)
                {
                    AveSiteMemberObject user = new AveSiteMemberObject { PrincipleId = (int)deletedMember["PrincipleId"], IsUser = true };
                    user.IsDomainGroup = (bool)deletedMember["IsDomainGroup"];
                    user.Login = deletedMember["Login"].ToString();
                    user.Title = deletedMember["Title"].ToString();
                    memberObj.DeletedMemberIds.Add(user.PrincipleId, user);
                    if (memberObj.AddedMemberIds.ContainsKey(user.PrincipleId))
                    {
                        memberObj.AddedMemberIds.Remove(user.PrincipleId);
                    }
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of keys")]
        private void ConvertItemObject(AveItemObject item, Dictionary<string, object> itemProperty)
        {
            item.DocID = itemProperty.ContainsKey("DocID") ? (Guid)itemProperty["DocID"] : (itemProperty.ContainsKey("UniqueId") ? (Guid)itemProperty["UniqueId"] : Guid.Empty);
            if (item.DocID.Equals(Guid.Empty))
            {
                item.DocID = itemProperty.ContainsKey("UniqueId") ? (Guid)itemProperty["UniqueId"] : Guid.Empty;
            }
            item.LeafName = itemProperty.ContainsKey("LeafName") ? itemProperty["LeafName"].ToString() : (itemProperty.ContainsKey("Name") ? itemProperty["Name"].ToString() : string.Empty);
            item.ItemName = item.LeafName;
            item.SourceName = item.LeafName;
            item.TimeLastModified = itemProperty.ContainsKey("TimeLastModified") ? (DateTime)itemProperty["TimeLastModified"] : (itemProperty.ContainsKey("Modified") ? (DateTime)itemProperty["Modified"] : DateTime.MinValue);
            object fieldValues;
            if (itemProperty.TryGetValue("FieldValues", out fieldValues) && fieldValues != null)
            {
                var values = (Dictionary<string, object>)fieldValues;
                item.Uiversion = values.ContainsKey("_UIVersion") ? (int)values["_UIVersion"] : 512;
            }
            else
            {
                item.Uiversion = itemProperty.ContainsKey("UIVersion") ? (int)itemProperty["UIVersion"] : 512;
            }

            item.ID = itemProperty.ContainsKey("DoclibRowId") ? (int?)itemProperty["DoclibRowId"] : (itemProperty.ContainsKey("Id") ? (int?)itemProperty["Id"] : null);
            item.FullUrl = itemProperty.ContainsKey("FullUrl") ? itemProperty["FullUrl"].ToString() : (itemProperty.ContainsKey("FileRef") ? itemProperty["FileRef"].ToString() : string.Empty);
            item.DirName = itemProperty.ContainsKey("DirName") ? itemProperty["DirName"].ToString() : (itemProperty.ContainsKey("FileDirRef") ? itemProperty["FileDirRef"].ToString() : string.Empty);
            item.DocFlags = itemProperty.ContainsKey("DocFlags") ? (int?)itemProperty["DocFlags"] : null;
            item.Hidden = itemProperty.ContainsKey("Hidden") ? (bool?)itemProperty["Hidden"] : null;
            item.ParentID = itemProperty.ContainsKey("ParentID") && itemProperty["ParentID"] is Guid ? (Guid)itemProperty["ParentID"] : Guid.Empty;
            item.CheckoutUserId = itemProperty.ContainsKey("CheckoutUserId") ? (int?)itemProperty["CheckoutUserId"] : null;
            item.Level = itemProperty.ContainsKey("Level") ? (byte)itemProperty["Level"] : Byte.MinValue;
            item.Type = itemProperty.ContainsKey("Type") ? Convert.ToByte(itemProperty["Type"]) : byte.MinValue;
            item.Size = itemProperty.ContainsKey("Size") ? long.Parse(itemProperty["Size"].ToString()) : 0;
            item.IsSystemObject = itemProperty.ContainsKey("IsSystemFile") ? (bool)itemProperty["IsSystemFile"] : false;
            item.HasStream = itemProperty.ContainsKey("HasStream") ? Convert.ToBoolean(itemProperty["HasStream"]) : false;
            item.QueryType = itemProperty.ContainsKey("QueryType") ? (int)itemProperty["QueryType"] : 2;
            item.ServerRelativeUrl = itemProperty.ContainsKey("ServerRelativeUrl") ? itemProperty["ServerRelativeUrl"].ToString() : string.Empty;
            if (itemProperty.ContainsKey("ChangeType"))
            {
                var changeType = ChangeType.None;
                Enum.TryParse(itemProperty["ChangeType"].ToString(), out changeType);
                item.ChangeType = changeType;
            } 
            item.isRename = itemProperty.ContainsKey("IsRenamed") ? (bool)itemProperty["IsRenamed"] : false;
            item.tp_GUID = itemProperty.ContainsKey("tp_GUID") ? (Guid)itemProperty["tp_GUID"] : (itemProperty.ContainsKey("GUID") ? (Guid)itemProperty["GUID"] : Guid.Empty);
            item.EventTime = itemProperty.ContainsKey("ChangeTime") ? (DateTime)itemProperty["ChangeTime"] : DateTime.MinValue;
            item.RoleAssignmentsChangeType = itemProperty.ContainsKey("RoleAssignmentsChangeType") ? (ChangeType)itemProperty["RoleAssignmentsChangeType"] : ChangeType.None;
            item.ModifyBy = itemProperty.ContainsKey("Modified_x0020_By") ? itemProperty["Modified_x0020_By"].ToString() : string.Empty;
            item.CreatedBy = itemProperty.ContainsKey("Created_x0020_By") ? itemProperty["Created_x0020_By"].ToString() : string.Empty;
            item.ItemChildCount = itemProperty.ContainsKey("ItemChildCount") ? (int?)(Convert.ToInt32(itemProperty["ItemChildCount"])) : null;
            item.FolderChildCount = itemProperty.ContainsKey("FolderChildCount") ? (int?)(Convert.ToInt32(itemProperty["FolderChildCount"])) : null;
        }

        private void ConvertItemObjectForSystemFolder(AveItemObject item, Dictionary<string, object> itemProperty, bool isFolder)
        {
            item.LeafName = itemProperty.ContainsKey("LeafName") ? itemProperty["LeafName"].ToString() : (itemProperty.ContainsKey("Name") ? itemProperty["Name"].ToString() : string.Empty);
            item.ItemName = item.LeafName;
            if (itemProperty.ContainsKey("ServerRelativeUrl"))
            {
                string serverRelativeUrl = itemProperty["ServerRelativeUrl"].ToString();
                string dirName = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'));
                item.DirName = dirName;
                item.FullUrl = serverRelativeUrl;
            }
            item.ID = itemProperty.ContainsKey("DoclibRowId") ? (int?)itemProperty["DoclibRowId"] : (itemProperty.ContainsKey("Id") ? (int?)itemProperty["Id"] : null);
            item.DocID = itemProperty.ContainsKey("DocID") ? (Guid)itemProperty["DocID"] : (itemProperty.ContainsKey("UniqueId") ? (Guid)itemProperty["UniqueId"] : Guid.Empty);
            item.Hidden = true;//(bool?)properties["Hidden"];
            item.QueryType = 2;//(int)properties["QueryType"];
            if (isFolder)
            {
                item.HasStream = false;
                return;
            }
            item.HasStream = true;//(int)properties["HasStream"] == 1 ? true : false;
            item.Uiversion = itemProperty.ContainsKey("UIVersion") ? (int)itemProperty["UIVersion"] : 512;
            item.TimeLastModified = itemProperty.ContainsKey("TimeLastModified") ? (DateTime)itemProperty["TimeLastModified"] : DateTime.MinValue;
            item.Level = itemProperty.ContainsKey("Level") ? (byte)itemProperty["Level"] : Byte.MinValue;
            //itemProperty.ContainsKey("DocLibRowId") ? (int)itemProperty["DocLibRowId"] : default(int);
            item.EventTime = itemProperty.ContainsKey("ChangeTime") ? (DateTime)itemProperty["ChangeTime"] : DateTime.MinValue;
            item.ObjType = ItemType.Document;
            item.ModifyBy = itemProperty.ContainsKey("Modified_x0020_By") ? itemProperty["Modified_x0020_By"].ToString() : string.Empty;
            item.CreatedBy = itemProperty.ContainsKey("Created_x0020_By") ? itemProperty["Created_x0020_By"].ToString() : string.Empty;
            #region BPOS can not support at now
            //itemObject.DocFlags = (int?)properties["DocFlags"]; //item has ETag property
            //itemObject.ParentID = (Guid)properties["ParentID"];
            //itemObject.ID = (int?)properties["ID"];
            //itemObject.Type = Convert.ToByte(properties["Type"]);
            //itemObject.Size = (int)properties["Size"];
            //if (properties.ContainsKey("RbsId"))
            //{
            //    itemObject.RbsId = (byte[])properties["RbsId"];
            //}
            //itemObject.IsCurrentVersion = (bool)properties["IsCurrentVersion"];
            //if (properties.ContainsKey("tp_GUID"))
            //{
            //    itemObject.tp_GUID = (Guid)properties["tp_GUID"];
            //}
            //if (properties.ContainsKey("CheckoutUserId"))
            //{
            //    itemObject.CheckoutUserId = (int?)properties["CheckoutUserId"];
            //}
            #endregion
        }

        private void ConvertListObjects(Dictionary<Guid, AveListObject> lists, Dictionary<Guid, object> listProperties)
        {
            foreach (KeyValuePair<Guid, object> listProperty in listProperties)
            {
                AveListObject list = new AveListObject();
                ConvertListObject(list, (Dictionary<string, object>)listProperty.Value);
                lists.Add(listProperty.Key, list);
            }
        }

        private void ConvertListObject(AveListObject list, Dictionary<string, object> listProperty)
        {
            list.ListId = listProperty.ContainsKey("ListId") ? (Guid)listProperty["ListId"] : Guid.Empty;
            list.RootFolderId = listProperty.ContainsKey("RootFolderId") ? (Guid)listProperty["RootFolderId"] : Guid.Empty;
            list.Name = listProperty.ContainsKey("Name") ? listProperty["Name"].ToString() : string.Empty;
            list.Title = listProperty.ContainsKey("Title") ? listProperty["Title"].ToString() : string.Empty;
            list.Type = listProperty.ContainsKey("Type") ? (int)listProperty["Type"] : 0;
            list.RootFolderUrl = listProperty.ContainsKey("RootFolderUrl") ? listProperty["RootFolderUrl"].ToString() : string.Empty;
            list.Flag = listProperty.ContainsKey("Flag") ? listProperty["Flag"] : null;
            list.ServerTemplate = listProperty.ContainsKey("ServerTemplate") ? (int?)listProperty["ServerTemplate"] : 0;
            list.Hidden = listProperty.ContainsKey("Hidden") ? (bool?)listProperty["Hidden"] : false;
            list.ChangeType = listProperty.ContainsKey("ChangeType") ? (ChangeType)listProperty["ChangeType"] : ChangeType.None;
            list.RoleAssignmentsChangeType = listProperty.ContainsKey("RoleAssignmentsChangeType") ? (ChangeType)listProperty["RoleAssignmentsChangeType"] : ChangeType.None;
        }

        private void ConvertViewObjects(Dictionary<Guid, AveViewObject> views, Dictionary<Guid, object> viewProperties)
        {
            foreach (KeyValuePair<Guid, object> viewProperty in viewProperties)
            {
                AveViewObject view = new AveViewObject();
                ConvertViewObject(view, (Dictionary<string, object>)viewProperty.Value);
                views.Add(viewProperty.Key, view);
            }
        }

        private void ConvertViewObject(AveViewObject view, Dictionary<string, object> viewProperties)
        {
            view.ViewID = (Guid)viewProperties["ViewID"];
            view.ViewType = (int)viewProperties["ViewType"];
            view.IsPersonalView = (bool)viewProperties["IsPersonalView"];
            view.BaseViewId = (byte)viewProperties["BaseViewId"];
            view.ViewTitle = viewProperties["ViewTitle"].ToString();
            view.PageUrlID = (Guid)viewProperties["PageUrlID"];
            view.ViewUserID = (int?)viewProperties["ViewUserID"];
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        private void FillFolderObject(Dictionary<string, object> curFolder, AveItemObject rootFolder, bool isUnderWebRootFolder, bool needClearCache = false)
        {
            object folders;
            object items;
            object attachements;
            object stubAttachments;
            if (curFolder.TryGetValue("Folders", out folders))
            {
                foreach (Dictionary<string, object> folderDic in folders as List<Dictionary<string, object>>)
                {
                    AveItemObject folder = null;
                    foreach (var folderObject in rootFolder.SubFolderObjs)
                    {
                        if (isUnderWebRootFolder)
                        {
                            if (folderObject.LeafName.Equals(folderDic["Name"].ToString()))
                            {
                                folder = folderObject;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (folderObject.LeafName.Equals(folderDic["LeafName"].ToString()))
                        {
                            folder = folderObject;
                            break;
                        }

                    }
                    if (folder == null)
                    {
                        string serverRelativeUrl = folderDic.ContainsKey("ServerRelativeUrl") ? folderDic["ServerRelativeUrl"].ToString() : folderDic.ContainsKey("FullUrl") ? folderDic["FullUrl"].ToString() : string.Empty;
                        var dirName = serverRelativeUrl.LastIndexOf('/') > 0
                                    ? serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'))
                                    : serverRelativeUrl;
                        AveItemObject parentFolder = GetParentFolder(dirName, rootFolder);
                        folder = GetCurrentFolder(parentFolder, serverRelativeUrl);
                    }

                    if (folder.DocID.Equals(Guid.Empty))
                    {
                        if (isUnderWebRootFolder)
                        {
                            ConvertItemObjectForSystemFolder(folder, folderDic, true);
                        }
                        else
                        {
                            ConvertItemObject(folder, folderDic);
                        }
                        if (string.IsNullOrEmpty(folder.FullUrl))
                        {
                            folder.FullUrl = folder.ServerRelativeUrl.Trim('/');
                        }
                        folder.ObjType = ItemType.Folder;
                        if (folder.ID.HasValue && folderDic.ContainsKey("Versions"))
                        {
                            foreach (Dictionary<string, object> dicVersion in folderDic["Versions"] as List<Dictionary<string, object>>)
                            {
                                var version = new AveVersionObject();
                                ConvertVersionObject(version, dicVersion);
                                folder.VersionObjs.Add(version);
                            }
                        }

                        if (folderDic.TryGetValue("Attachments", out attachements))
                        {
                            foreach (Dictionary<string, object> attachment in attachements as List<Dictionary<string, object>>)
                            {
                                var attachmentObject = new AveItemObject();
                                attachmentObject.ObjType = ItemType.Document;
                                ConvertAttachmentObject(attachmentObject, attachment);
                                folder.AttachmentObjs.Add(attachmentObject);
                            }
                        }

                        if (folderDic.TryGetValue("StubAttachments", out stubAttachments))
                        {
                            foreach (Dictionary<string, object> attachment in stubAttachments as List<Dictionary<string, object>>)
                            {
                                var attachmentObject = new AveItemObject();
                                ConvertAttachmentObject(attachmentObject, attachment);
                                folder.StubAttachmentObjs.Add(attachmentObject);
                            }
                        }
                    }
                    if (needClearCache) folderDic.Clear();
                }
            }

            if (curFolder.TryGetValue("Items", out items))
            {
                foreach (Dictionary<string, object> dicItem in items as List<Dictionary<string, object>>)
                {
                    var item = new AveItemObject();
                    if (isUnderWebRootFolder)
                    {
                        ConvertItemObjectForSystemFolder(item, dicItem, false);
                    }
                    else
                    {
                        ConvertItemObject(item, dicItem);
                        item.ObjType = (ItemType)dicItem["ObjType"];
                    }
                    string serverRelativeUrl = dicItem.ContainsKey("ServerRelativeUrl") ? dicItem["ServerRelativeUrl"].ToString() : dicItem.ContainsKey("FullUrl") ? dicItem["FullUrl"].ToString() : string.Empty;
                    var dirName = serverRelativeUrl.LastIndexOf('/') >= 0
                                    ? serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'))
                                    : serverRelativeUrl;
                    AveItemObject parentFolder = GetParentFolder(dirName, rootFolder);
                    if (parentFolder == null) //If a changed page in another list has this list's Listview webPart, we can't get its parent folder.
                    {
                        continue;
                    }
                    parentFolder.SubItemObjs.Add(item);
                    //if (item.ID.HasValue)
                    //{
                    foreach (Dictionary<string, object> dicVersion in dicItem["Versions"] as List<Dictionary<string, object>>)
                    {
                        var version = new AveVersionObject();
                        ConvertVersionObject(version, dicVersion);
                        item.VersionObjs.Add(version);
                    }
                    //}
                    if (dicItem.TryGetValue("Attachments", out attachements))
                    {
                        foreach (Dictionary<string, object> attachment in attachements as List<Dictionary<string, object>>)
                        {
                            var attachmentObject = new AveItemObject();
                            attachmentObject.ObjType = ItemType.Document;
                            ConvertAttachmentObject(attachmentObject, attachment);
                            item.AttachmentObjs.Add(attachmentObject);
                        }
                    }
                    if (dicItem.TryGetValue("StubAttachments", out stubAttachments))
                    {
                        foreach (Dictionary<string, object> attachment in stubAttachments as List<Dictionary<string, object>>)
                        {
                            var attachmentObject = new AveItemObject();
                            ConvertAttachmentObject(attachmentObject, attachment);
                            item.StubAttachmentObjs.Add(attachmentObject);
                        }
                    }
                    if (needClearCache) dicItem.Clear();
                }
            }
            if (needClearCache) curFolder.Clear();
        }

        private AveItemObject GetParentFolder(string dirName, AveItemObject rootFolder)
        {
            string listRootFolderUrl = rootFolder.FullUrl;

            if (dirName.Trim('/').Equals(listRootFolderUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
            {
                return rootFolder;
            }
            if (!dirName.Trim('/').Contains(listRootFolderUrl.Trim('/')))
            {
                return null;
            }
            string foldersDirName = dirName.Trim('/').Substring(listRootFolderUrl.Trim('/').Length).Trim('/');

            AveItemObject tempFolder = rootFolder;
            AveItemObject tempParentFolder = rootFolder;
            foreach (string str in foldersDirName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (FolderExist(ref tempParentFolder, str))
                {
                    continue;
                }
                else
                {
                    tempFolder = new AveItemObject
                    {
                        LeafName = str,
                        DirName = tempParentFolder.FullUrl.Trim('/'),
                        ObjType = ItemType.Folder
                    };
                    tempFolder.FullUrl = (tempFolder.DirName + "/" + tempFolder.LeafName).Trim('/');
                    if (tempParentFolder.SubFolderObjs == null)
                    {
                        tempParentFolder.SubFolderObjs = new List<AveItemObject>();
                    }
                    tempParentFolder.SubFolderObjs.Add(tempFolder);
                    //mNoPropertyFolders.Add(tempFolder.FullUrl, tempFolder);
                    tempParentFolder = tempFolder;
                }
            }
            return tempParentFolder;
        }

        private AveItemObject GetCurrentFolder(AveItemObject parent, string fullUrl)
        {
            AveItemObject folder = null;

            foreach (AveItemObject afc in parent.SubFolderObjs)
            {
                if (afc.FullUrl.Trim('/').Equals(fullUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                {
                    folder = afc;
                    break;
                }
            }

            if (folder == null)
            {
                folder = new AveItemObject();
                parent.SubFolderObjs.Add(folder);
            }
            return folder;
        }

        private bool FolderExist(ref AveItemObject tempParentFolder, string str)
        {
            foreach (AveItemObject folder in tempParentFolder.SubFolderObjs)
            {
                if (folder.LeafName.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    tempParentFolder = folder;
                    return true;
                }
            }
            return false;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        private void ConvertVersionObject(AveVersionObject version, Dictionary<string, object> versionProperty)
        {
            version.Size = long.Parse(versionProperty["Size"].ToString());
            version.TimeLastModified = (DateTime)versionProperty["TimeLastModified"];
            version.Uiversion = (int)versionProperty["UIVersion"];
            version.Level = (byte)versionProperty["Level"];
            version.IsCurrentVersion = (bool)versionProperty["IsCurrentVersion"];
            if (versionProperty.ContainsKey("UserDataGuid"))
            {
                version.UserDataGuid = (Guid)versionProperty["UserDataGuid"];
            }
            version.ObjType = (ItemType)versionProperty["ObjType"];
            //version.QueryType = (int)versionProperty["QueryType"];
        }

        /// <summary>
        /// 将discover获取的attachment属性进行封装向外开放
        /// </summary>
        /// <param name="attachmentObject"></param>
        /// <param name="attachment"></param>
        private void ConvertAttachmentObject(AveItemObject attachmentObject, Dictionary<string, object> attachment)
        {
            attachmentObject.DocID = (Guid)attachment["DocID"];
            attachmentObject.DirName = attachment["DirName"].ToString();
            attachmentObject.LeafName = attachmentObject.ItemName = attachment["LeafName"].ToString();
            attachmentObject.Uiversion = (int)attachment["UIVersion"];//key应该是UIVersion；
            attachmentObject.DocFlags = (int?)attachment["DocFlags"];
            attachmentObject.TimeLastModified = (DateTime)attachment["TimeLastModified"];
            attachmentObject.Level = (byte)attachment["Level"];
            attachmentObject.Type = (byte)attachment["Type"];
            attachmentObject.Size = long.Parse(attachment["Size"].ToString());
            attachmentObject.ParentID = (Guid)attachment["ParentID"];
            attachmentObject.RbsId = (byte[])attachment["RbsId"];
            attachmentObject.FullUrl = attachment["FullUrl"].ToString();
            attachmentObject.CheckoutUserId = (int?)attachment["CheckoutUserId"];
            attachmentObject.HasStream = (bool)attachment["HasStream"];
            attachmentObject.ServerRelativeUrl = attachment["ServerRelativeUrl"].ToString();
            attachmentObject.ID = (int?)attachment["ID"];
        }

        private void ConvertContentTypeObjects(Dictionary<byte[], AveContentTypeObject> contentTypes, Dictionary<byte[], object> contentTypeProperties)
        {
            foreach (KeyValuePair<byte[], object> contentTypeProperty in contentTypeProperties)
            {
                AveContentTypeObject contentType = new AveContentTypeObject();
                ConvertContentTypeObject(contentType, (Dictionary<string, object>)contentTypeProperty.Value);
                contentTypes.Add(contentTypeProperty.Key, contentType);
            }
        }
        private void MarkDiscoverFolder(AveItemObject parentFolder)
        {
            parentFolder.AllListContentAdded = true;
            foreach(var subFolder in parentFolder.SubFolderObjs)
            {
                MarkDiscoverFolder(subFolder);
            }
        }

        private void ConvertContentTypeObject(AveContentTypeObject contentType, Dictionary<string, object> contentTypeProperty)
        {
            contentType.ContentTypeId = (byte[])contentTypeProperty["ContentTypeId"];
            contentType.Name = contentTypeProperty["Name"].ToString();
            contentType.Scope = contentTypeProperty["Scope"].ToString();
            contentType.SchemaXml = contentTypeProperty["SchemaXml"].ToString();
        }

        private void InitWebObject(AveWebObject webObj, AveWebCache webCache, IAveWeb web)
        {
            webObj.WebID = web.ID;
            webObj.Title = web.Title;
            webObj.Name = web.IsRootWeb ? "." : web.Name;
        }

        private void InitListObject(AveListObject listObj, AveListCache listCache, IAveList list)
        {
            //listCache.ParentWeb.WebID = list.ParentWeb.ID;
            //listCache.ListID = listObj.ListId = list.ID;
            listObj.ListId = list.ID;
            listObj.Title = listObj.Name = list.Title;
            listObj.RootFolderId = list.RootFolder.UniqueId;
            listObj.Type = (int)list.BaseType;
            listObj.Flag = list.Flags;
            listObj.RootFolderUrl = listObj.RootFolderUrl.Trim('/');
            //listObj.ServerTemplate = (int?)sr["tp_ServerTemplate"];
            listObj.Hidden = list.Hidden;
        }

        private void InitFolderObject(AveItemObject folderObj, AveFolderCache folderCache, IAveFolder folder)
        {
            //folderCache.WebId = folder.ParentWeb.Id;
            ////folderCache.ParentList.ListID = folder.ParentList.ID;
            ////folderCache.ParentList.ParentWeb.WebID = folderCache.ParentWeb.WebID;
            //folderCache.ListId = folder.ParentListId;
            //if (folderCache.ListId != Guid.Empty)
            //{
            //    folderCache.ListUrl = folder.ParentList.RootFolder.Url;
            //}
            folderCache.InitBasicProperties(folder.ParentWeb.ID, folder.ParentListId, folderCache.ListId != Guid.Empty ? folder.ParentList.RootFolder.Url : string.Empty);
        }
        #endregion

        public IAveDiscoveryQuery CloneObjWithNewRequest(object aveRequest)
        {
            if (aveRequest != null && aveRequest is AveRequestParameter)
            {
                AveRequestParameter requestParameter = aveRequest as AveRequestParameter;
                if (requestParameter.AveRequest != null && !requestParameter.AveRequest.Url.Equals(this.mSiteUrl, StringComparison.OrdinalIgnoreCase))
                { // here we need to check the url, otherwise we may use wrong bpos account info.
                    throw new ArgumentException("AveRequestParameter's url is not acceptable.");
                }

                AveDiscoveryQuery newQuery = new AveDiscoveryQuery(this.mSiteUrl, requestParameter, this.mUserAccountInfo,
                    this.mStartTime, this.mEndTime, this.SupportIB, this.mChangeCache);
                return newQuery;
            }
            return this;
        }

        public long GetItemSizeAndUserInfo(Guid siteId, Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {
            throw new NotImplementedException();
        }

        public int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid docId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// [ADO-25849]Replicator BPOS-S运行时间过长，添加结构体记录Folder的基本信息
        /// </summary>
        public struct AveQueryCacheForReplicator
        {
            public Guid SiteId;
            public Guid WebId;
            public Guid ListId;
            public Guid DocID;
            public string FullUrl;
        }

        /// <summary>
        /// [ADO-25849]Replicator BPOS-S运行时间过长，该方法根据参数返回一个结构体变量
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="docId"></param>
        /// <param name="fullUrl"></param>
        /// <returns></returns>
        public AveQueryCacheForReplicator GetAveQueryCacheForReplicator(Guid siteId, Guid webId, Guid listId, Guid docId, string fullUrl)
        {
            AveQueryCacheForReplicator queryCache = new AveQueryCacheForReplicator();
            queryCache.SiteId = siteId;
            queryCache.WebId = webId;
            queryCache.ListId = listId;
            queryCache.DocID = docId;
            queryCache.FullUrl = fullUrl;
            return queryCache;
        }

        #region IAveDiscoveryQuery Members


        public long GetWebSize(Guid siteId, Guid webId)
        {
            throw new NotImplementedException();
        }

        public long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime)
        {
            throw new NotImplementedException();
        }

        public long GetListSize(Guid siteId, Guid webId, Guid listid)
        {
            throw new NotImplementedException();
        }

        public long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl)
        {
            throw new NotImplementedException();
        }

        #endregion


        public void QueryVersionsByItemObj(AveItemCache itemCache, AveItemObject itemObject)
        {
        }


        public void QueryAttachmentByItemObj(IAveWeb web, Guid listId, AveItemObject itemObject)
        {
        }
    }
}
