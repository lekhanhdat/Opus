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
using AvePoint.ObjectModel.Common;
using AvePoint.Wrapper;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/02/29", "gqsun@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    [AveCodeReview("2012/03/08", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    internal class AveQuery : IAveDiscoveryQuery
    {
        private AveBPOSAccountInfo mUserAccountInfo;
        private string mSiteUrl;
        private readonly DateTime mStartTime;
        private readonly DateTime mEndTime;
        private IAveRequest mRequest;
        //private IAveDiscoverRequest mDiscoverRequest;
        private AveRequestParameter mRequestParameter;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveQuery));
        public bool SupportIB { get; set; }
        public bool CacheItemProperties { get; set; }
        private Dictionary<string, object> mChangeCache = new Dictionary<string, object>();
        private Dictionary<Guid, object> mWebParts = new Dictionary<Guid, object>();
        private bool mQuerySite = false;
        //[ADO-25849]Replicator BPOS-S运行时间过长，添加缓存，Dictionary的count不大于1
        private Dictionary<AveQueryCacheForReplicator, Dictionary<string, object>> mItemsAndFoldersCacheForReplicator = null;

        internal IAveRequest Request
        {
            get
            {
                return mRequest;
            }
        }

        protected IAveRequest DiscoverRequest
        {
            get
            {
                //if (mDiscoverRequest == null&& mRequest is IAveDiscoverRequest)
                //{
                //    mDiscoverRequest = mRequest as IAveDiscoverRequest;
                //}
                return mRequest;
            }
        }

        public void Dispose()
        {
            AveRequestInterceptor.DisposeAvailableRequest(mRequestParameter, mSiteUrl);
            mChangeCache.Clear();
        }

        #region Construct Function
        public AveQuery()
        {
            SupportIB = false;
        }

        public AveQuery(string siteUrl, AveBPOSAccountInfo account, DateTime startTime, DateTime endTime)
            : this(siteUrl, null, account, startTime, endTime, false, new Dictionary<string, object>())
        { }

        public AveQuery(string siteUrl, AveBPOSAccountInfo account)
            : this(siteUrl, null, account, DateTime.MinValue, DateTime.MinValue, false, new Dictionary<string, object>())
        { }

        public AveQuery(IAveSite site)
            : this(site.Url, (site as AveSite).RequestParameter, (site as AveSite).UserAccountInfo, DateTime.MinValue, DateTime.MinValue, false, new Dictionary<string, object>())
        { }

        public AveQuery(IAveSite site, AveBPOSAccountInfo account)
            : this(site.Url, (site as AveSite).RequestParameter, account, DateTime.MinValue, DateTime.MinValue, false, new Dictionary<string, object>())
        { }

        public AveQuery(IAveSite site, DateTime startTime, DateTime endTime)
            : this(site.Url, (site as AveSite).RequestParameter, (site as AveSite).UserAccountInfo, startTime, endTime, false, new Dictionary<string, object>())
        { }

        public AveQuery(IAveSite site, AveBPOSAccountInfo account, DateTime startTime, DateTime endTime)
            : this(site.Url, (site as AveSite).RequestParameter, account, startTime, endTime, false, new Dictionary<string, object>())
        { }

        private AveQuery(string siteUrl, AveRequestParameter requestParameter, AveBPOSAccountInfo account, DateTime startTime, DateTime endTime, bool supportIB, Dictionary<string, object> changes)
        {
            mSiteUrl = siteUrl;
            mUserAccountInfo = account;
            mStartTime = startTime;
            mEndTime = endTime;

            mRequestParameter = requestParameter;
            if (mRequestParameter == null || mRequestParameter.AveRequest == null)
            {
                InitRequest();
                mRequestParameter = new AveRequestParameter(mRequest, account);
            }
            else
            {
                mRequest = requestParameter.AveRequest;
            }
            mSiteUrl = mRequest.Url;
            SupportIB = supportIB;
            mChangeCache = changes;
        }
        #endregion

        #region Init Request

        private void InitRequest()
        {
#if PerformaceLog
            AveRequestInterceptor request = new AveRequestInterceptor(mSiteUrl, mUserAccountInfo);
            mRequest = request.Proxy;
#else
            AveClientRequest request = new AveClientRequest(mSiteUrl, mUserAccountInfo);
            mRequest = request.InitRequest();
#endif
        }
        #endregion

        #region Init List/Web
        public void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj)
        {
            IAveWeb web = webCache.AveSite.OpenWeb(webObj.FullUrl);
            this.InitWebObject(webObj, webCache, web);
        }

        public void InitDiscoverList(AveListCache listCache, AveListObject listObj)
        {
            IAveList list = listCache.AveWeb.GetList(listObj.RootFolderUrl);
            this.InitListObject(listObj, listCache, list);
        }

        public void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj)
        {
            IAveFolder folder = folderCache.AveWeb.GetFolder(folderObj.FullUrl);
            this.InitFolderObject(folderObj, folderCache, folder);
        }
        #endregion

        #region Site Level
        public Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId)
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
                Dictionary<Guid, object> webProperties = DiscoverRequest.QueryWebForIB(mChangeCache["ChangedWebCache"] as Dictionary<Guid, object>);
                ConvertWebObjects(webs, webProperties);
            }
            return webs;
        }

        public Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId)
        {
            Dictionary<int, AveSiteMemberObject> members = new Dictionary<int, AveSiteMemberObject>();
            Dictionary<int, object> memberProperties = DiscoverRequest.QuerySiteSecurityForIB(siteId, mStartTime, mEndTime);
            if (memberProperties != null)
            {
                ConvertSiteMemberObjects(members, memberProperties);
            }
            return members;
        }
        #endregion

        #region Web Level
        public void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject)
        {
            Dictionary<string, object> folder = DiscoverRequest.QueryWebRootFolder(listCache.WebId);
            ConvertItemObject(rootFolderObject, folder);
            rootFolderObject.ObjType = ItemType.Folder;
            rootFolderObject.DirName = folder["DirName"].ToString();
            rootFolderObject.FullUrl = folder["FullUrl"].ToString();
        }

        public Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId)
        {
            Dictionary<Guid, AveListObject> changedLists = new Dictionary<Guid, AveListObject>();
            if (mChangeCache.ContainsKey("ChangedListCache"))
            {
                //Dictionary<Guid, object> listsProp = mRequest.QueryListForIB(webId, mChangeCache["ChangedListCache"] as Dictionary<Guid, object>);
                Dictionary<Guid, object> listsProp = DiscoverRequest.QueryListForIB(webId, mChangeCache, mStartTime, mEndTime);
                ConvertListObjects(changedLists, listsProp);
            }
            return changedLists;
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForIB(Guid siteId, Guid webId)
        {
            throw new NotImplementedException();
        }

        public AveWebObject QueryRootWeb(Guid siteId)
        {
            Dictionary<string, object> webProperty = DiscoverRequest.QueryRootWeb(siteId);
            AveWebObject web = new AveWebObject();
            ConvertWebObject(web, webProperty);
            return web;
        }


        public AveWebObject QueryWeb(Guid webId)
        {
            Dictionary<string, object> webProperty = DiscoverRequest.QueryWeb(webId);
            AveWebObject web = new AveWebObject();
            ConvertWebObject(web, webProperty);
            return web;
        }

        public Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId)
        {
            Dictionary<Guid, AveWebObject> webs = new Dictionary<Guid, AveWebObject>();
            Dictionary<Guid, object> webProperties = DiscoverRequest.GetSubWebs(siteId, parentWebId);
            ConvertWebObjects(webs, webProperties);
            return webs;
        }

        public List<AveProjectObject> QueryProjects()
        {
            var projects = new List<AveProjectObject>();
            var rawData = mRequest.QueryProjects(false);
            var temp = ConvertProjectObjects(rawData);
            if (this.mStartTime == DateTime.MinValue)
            {
                projects = temp;
            }
            else
            {
                foreach (var item in temp)
                {
                    if ((item.LastPublishedDate.Ticks > this.mStartTime.Ticks && item.LastPublishedDate.Ticks <= this.mEndTime.Ticks)
                        || (item.LastSavedDate.Ticks > this.mStartTime.Ticks && item.LastSavedDate.Ticks <= this.mEndTime.Ticks))
                    {
                        projects.Add(item);
                    }
                }
            }
            return projects;
        }
        #endregion

        #region list level
        public void QueryListRootFolder(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject)
        {
            Dictionary<string, object> folder = DiscoverRequest.QueryListRootFolder(listCache.SiteId, listCache.WebId, listCache.ListId);
            ConvertItemObject(rootFolderObject, folder);
            rootFolderObject.ObjType = ItemType.Folder;
            rootFolderObject.DirName = folder["DirName"].ToString();
            rootFolderObject.FullUrl = folder["FullUrl"].ToString();
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
            throw new NotImplementedException();
        }

        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            Dictionary<string, object> changedItems = new Dictionary<string, object>();
            changedItems = DiscoverRequest.QueryListItemForIB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, mChangeCache["ChangedItemsCache"] as Dictionary<string, object>);
            //bool isUnderWebRootFolder = folderCache.ParentList.ListID == Guid.Empty ? true : false;
            bool isUnderWebRootFolder = folderCache.ListId == Guid.Empty ? true : false;
            FillFolderObject(changedItems, folderObject, isUnderWebRootFolder);
        }

        public Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid webId, Guid listId, int itemId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId, bool includeAppWeb = false)
        {
            Dictionary<Guid, AveWebObject> allWebs = new Dictionary<Guid, AveWebObject>();
            Dictionary<string, object> allWebProperties = mRequest.GetAllWebs();
            List<IDictionary<string, object>> webPropertiesList = allWebProperties.GetChildren();
            foreach (var webProperties in webPropertiesList)
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
                web.FullUrl = webProperties["Url"].ToString();
                web.IsAppWeb = webProperties.ContainsKey("AppInstanceId") ? (Guid)webProperties["AppInstanceId"] != Guid.Empty : false;
                web.AppInstanceId = webProperties.ContainsKey("AppInstanceId") ? (Guid)webProperties["AppInstanceId"] : Guid.Empty;
                if (web.IsAppWeb && !includeAppWeb)
                {
                    continue;
                }
                allWebs.Add((Guid)webProperties["Id"], web);
            }
            return allWebs;
        }

        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool throwException = false)
        {
            Dictionary<Guid, object> listProperties = DiscoverRequest.QueryWebListForFB(siteId, webId, throwException);
            Dictionary<Guid, AveListObject> lists = new Dictionary<Guid, AveListObject>();
            ConvertListObjects(lists, listProperties);
            return lists;
        }

        public Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            Dictionary<Guid, AveViewObject> views = new Dictionary<Guid, AveViewObject>();
            Dictionary<Guid, object> viewProperties = DiscoverRequest.QueryListViewForFB(siteId, webId, listId);
            ConvertViewObjects(views, viewProperties);
            return views;
        }
        #endregion

        #region Item Level
        public int GetSiteChangedForIB(Guid siteId)
        {
            int changeType = DiscoverRequest.GetSiteChangedForIB(siteId, mStartTime, mEndTime, mChangeCache);
            mQuerySite = true;
            return changeType;
        }
        public int GetListChangedForRecords(Guid webId, Guid listId)
        {
            int changeType = DiscoverRequest.GetListChangedForRecords(webId, listId, mStartTime, mEndTime, mChangeCache);
            //mQuerySite = true;
            return changeType;
        }
        public int GetListChangedCount(Guid webId, Guid listId)
        {
            int listChangeCount = DiscoverRequest.GetListChangedCount(webId, listId, mStartTime, mEndTime);
            return listChangeCount;
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId)
        {
            Dictionary<string, object> listChangeItems = DiscoverRequest.GetListChangedItems(webId, listId, mStartTime, mEndTime);
            return listChangeItems;
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            Dictionary<string, object> listChangeItems = DiscoverRequest.GetListChangedItems(webId, listId, startTime, endTime);
            return listChangeItems;
        }

        public Dictionary<string, object> GetListDeletedItems(Guid webId, Guid listId)
        {
            Dictionary<string, object> listDeletedItems = DiscoverRequest.GetListDeletedItems(webId, listId, mStartTime, mEndTime);
            return listDeletedItems;
        }

        public Dictionary<string, object> GetFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime)
        {
            Dictionary<string, object> folderChangeItems = DiscoverRequest.GetFolderChangedItems(webId, listId, folderId, startTime, endTime);
            return folderChangeItems;
        }

        public Dictionary<string, object> GetFolderAndSubFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime)
        {
            Dictionary<string, object> folderChangeItems = DiscoverRequest.GetFolderAndSubFolderChangedItems(webId, listId, folderId, startTime, endTime);
            return folderChangeItems;
        }


        public void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject folderObject)
        {
            Dictionary<string, object> folderProperty = DiscoverRequest.QueryListItemForFB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, true, true, false);
            //bool isUnderWebRootFolder = folderCache.ParentList.ListID.Equals(Guid.Empty) || folderCache.ParentList == null ? true : false;
            bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
            FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
        }

        public void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeRecycleBin, bool includeSystemFolder)
        {
            Dictionary<string, object> folderProperty = DiscoverRequest.QueryListItemForFB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, true, false, includeSystemFolder);
            bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
            FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
        }

        public void QueryAttachment(AveFolderCache folderCache, AveItemObject folderObject)
        {
        }

        public void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeRecycleBin, bool includeSystemFolder)
        {
            Dictionary<string, object> folderProperty = DiscoverRequest.QueryListItemForFB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, true, true, includeSystemFolder);
            bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
            FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            Dictionary<byte[], AveContentTypeObject> contentTypes = new Dictionary<byte[], AveContentTypeObject>();
            Dictionary<byte[], object> contentTypeProperties = DiscoverRequest.QueryWebContentTypeForFB(siteId, webId);
            ConvertContentTypeObjects(contentTypes, contentTypeProperties);
            return contentTypes;
        }

        public Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForFB(Guid siteId, Guid webId, Guid listId, string rootFolderUrl, int listType, object flag)
        {
            Dictionary<byte[], AveContentTypeObject> contentTypes = new Dictionary<byte[], AveContentTypeObject>();
            Dictionary<byte[], object> contentTypeProperties = DiscoverRequest.QueryListContentTypeForFB(siteId, webId, listId);
            ConvertContentTypeObjects(contentTypes, contentTypeProperties);
            return contentTypes;
        }

        public string GetListTitle(Guid siteId, Guid webId, Guid listId)
        {
            return DiscoverRequest.GetListTitle(siteId, webId, listId);
        }

        public string GetListContentTypes(Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public ItemType GetItemObjectTypeForReplciator(bool isListItem, Dictionary<string, object> itemProperties)
        {
            var itemType = ItemType.UnKnow;
            if (isListItem)
            {
                itemType = ItemType.Item;
            }
            else if ((int)itemProperties["FileSystemObjectType"] == 1)
            {
                itemType = ItemType.Folder;
            }
            else
            {
                itemType = ItemType.Document;
            }
            return itemType;
        }

        public AveItemObject GetItemExistForListener(string webServerRelativeUrl, System.Globalization.CultureInfo culture, Guid listId, Guid tpGuid, string dirName, string leafName, bool isListItem)
        {
            var item = new AveItemObject();
            //var currentItemProperties = new Dictionary<string, object>();
            //if (isViewFile)
            //{
            //    currentItemProperties = GetViewFile(webServerRelativeUrl, listId, viewId);
            //    ConvertItemObject(item, currentItemProperties);
            //    item.ObjType = ItemType.View;
            //}
            //else
            //{
            var camlQueryNode = GetCamlQueryNodeForRP(tpGuid, dirName, leafName, isListItem);
            mLogger.Info("Start to query item under web: {0}", webServerRelativeUrl);
            var itemObjs = DiscoverRequest.GetItemsByCamlQueryWithAttachments(webServerRelativeUrl, listId, camlQueryNode);
            var tempCurrentItemProperties = itemObjs.GetChildren().FirstOrDefault();
            //}
            if (tempCurrentItemProperties == null || tempCurrentItemProperties.Count == 0)
            {
                return null;
            }
            var currentItemProperties = new Dictionary<string, object>();
            currentItemProperties.AddRange(tempCurrentItemProperties);
            var itemId = Convert.ToInt32(currentItemProperties["ID"]);
            ConvertItemObject(item, currentItemProperties);

            item.ObjType = GetItemObjectTypeForReplciator(isListItem, currentItemProperties);

            //Get attachments
            object haveAttachments = false;
            if (currentItemProperties.TryGetValue("Attachments" + AveObjectModelConstant.ObjectPropertySuffix, out haveAttachments))
            {
                if (Convert.ToBoolean(haveAttachments))
                {
                    List<Dictionary<string, object>> attachments = currentItemProperties["Attachments"] as List<Dictionary<string, object>>;
                    foreach (Dictionary<string, object> dicAttachment in attachments)
                    {
                        var attachment = new AveItemObject();
                        ConvertAttachmentObject(attachment, dicAttachment);
                        item.AttachmentObjs.Add(attachment);
                    }
                }
            }

            Dictionary<string, object> listItemVersionsProperties = mRequest.GetItemVersions(webServerRelativeUrl, string.Empty, listId.ToString(), itemId, string.Empty, culture, NeedLoadFieldsForReplicator(), true);

            if (!listItemVersionsProperties.ContainsKey("HasVersion") || (Boolean)listItemVersionsProperties["HasVersion"])
            {
                foreach (var listItemVersionProperties in listItemVersionsProperties.GetChildren())
                {
                    var version = new AveVersionObject();
                    ConvertVersionObjectForReplicator(version, listItemVersionProperties);
                    version.ID = itemId;
                    version.UserDataGuid = tpGuid;
                    item.VersionObjs.Add(version);
                }
            }
            mLogger.Info("Finish to query item under web: {0}", webServerRelativeUrl);
            return item;
        }

        private Dictionary<string, string> NeedLoadFieldsForReplicator()
        {
            return new Dictionary<string, string>
            {
                {"_UIVersion","Integer"},
                {"_UIVersionString","Text"},
                {"_Level","Integer"},
                {"_IsCurrentVersion","Boolean"},
                {"GUID", "Guid"},
                //{"Created_x0020_By","Text"},
                //{"FileRef","Lookup"},
                //{"File_x0020_Size","Lookup"},
            };
        }

        private void ConvertVersionObjectForReplicator(AveVersionObject version, IDictionary<string, object> versionProperties)
        {
            version.TimeLastModified = (DateTime)versionProperties["Modified"];
            version.Uiversion = (int)versionProperties["VersionId"];
            version.UiVersionString = versionProperties["VersionLabel"].ToString();
            version.Level = (byte)versionProperties["Level"];
            version.IsCurrentVersion = Convert.ToBoolean(versionProperties["IsCurrentVersion"]);
            //version.UserDataGuid = new Guid(versionProperties["GUID"].ToString());
        }

        private string[] GetCamlQueryNodeForRP(Guid tpGuid, string dirName, string leafName, bool isListItem)
        {
            AveCamlQuery query = new AveCamlQuery();
            if (isListItem)
            {
                query.ViewXml = string.Format(
                "<View Scope='RecursiveAll'>" +
                "<Query><Where>" +
                "<Eq><FieldRef Name=\"GUID\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                "</Where></Query></View>",
                tpGuid.ToString());
            }
            else
            {
                query.ViewXml = string.Format(
                                              "<View Scope=\"Default\">" +
                                              "<Query><Where><And>" +
                                              "<Eq><FieldRef Name=\"FileDirRef\"/><Value Type=\"Lookup\">{0}</Value></Eq>" +
                                              "<Eq><FieldRef Name=\"FileLeafRef\"/><Value Type=\"Lookup\">{1}</Value></Eq>" +
                                              "</And></Where></Query></View>",
                                              dirName, leafName);
                query.FolderServerRelativeUrl = "/" + dirName;
            }
            return query.ToStringArray();
        }

        public AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem)
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
                            if ((itemDic.ContainsKey("DocID") && (Guid)itemDic["DocID"] == id) || (itemDic.ContainsKey("DocId") && (Guid)itemDic["DocId"] == id))
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
                item = DiscoverRequest.GetItemExist(SiteId, webId, listId, id, dirName, leafName, isListItem);
            }
            if (item != null)
            {
                ConvertItemObject(itemObject, item);
                if (itemObject.ID.HasValue)
                {
                    if (item.ContainsKey("Versions"))
                    {
                        foreach (Dictionary<string, object> dicVersion in item["Versions"] as List<Dictionary<string, object>>)
                        {
                            var version = new AveVersionObject();
                            ConvertVersionObject(version, dicVersion);
                            itemObject.VersionObjs.Add(version);
                        }
                    }
                    //[ADO-25849]初始化itemObject的AttachmentObjs属性
                    object attachments;
                    if (item.TryGetValue("Attachments", out attachments))
                    {
                        foreach (Dictionary<string, object> dicAttachment in attachments as List<Dictionary<string, object>>)
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

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
        {
            return DiscoverRequest.GetItemLastModifiedTime(siteId, webId, listId, id, hasDocLibRowId);
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            return DiscoverRequest.GetItemLastModifiedTime(siteId, webId, listId, dirName, leafName, ref docId);
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId)
        {
            return DiscoverRequest.GetItemLastModifiedTime(siteId, webId, listId, tp_Guid, ref docId);
        }

        public AveItemObject GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId)
        {
            throw new NotImplementedException();
        }

        public Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId)
        {
            return DiscoverRequest.GetListItemGuid(webId, listId, tp_Guid, rowId);
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid)
        {
            throw new NotImplementedException();
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return DiscoverRequest.GetDocIdByTp_Guid(siteId, webId, listId, parentId, tp_Guid, rowId);
        }

        public bool GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl, ref Dictionary<Guid, Guid> itemsMapping, ref Dictionary<Guid, Guid> foldersMapping)
        {
            try
            {
                Dictionary<string, object> folderProperty = DiscoverRequest.QueryListItemForFB(siteId, webId, listId, parentId, folderUrl, true, true, false);
                //由于现在wrapper的逻辑是restore的时候不应该有discover，如果有discover会导致无法refreshcontext，先暂时用这个方法处理
                mRequest.Dispose(true);
                object folders;
                object items;
                itemsMapping = new Dictionary<Guid, Guid>();
                foldersMapping = new Dictionary<Guid, Guid>();
                if (folderProperty.TryGetValue("Folders", out folders))
                {
                    foreach (Dictionary<string, object> folderDic in folders as List<Dictionary<string, object>>)
                    {
                        Guid ItemDocId = folderDic.ContainsKey("DocID") ? (Guid)folderDic["DocID"] : (folderDic.ContainsKey("DocId") ? (Guid)folderDic["DocId"] : Guid.Empty);
                        Guid ItemGuid = folderDic.ContainsKey("GUID") ? (Guid)folderDic["GUID"] : Guid.Empty;
                        if (ItemDocId == Guid.Empty || ItemGuid == Guid.Empty)
                        {
                            continue;
                        }
                        foldersMapping[(Guid)folderDic["GUID"]] = ItemDocId;
                    }
                }
                if (folderProperty.TryGetValue("Items", out items))
                {
                    foreach (Dictionary<string, object> dicItem in items as List<Dictionary<string, object>>)
                    {
                        Guid ItemDocId = dicItem.ContainsKey("DocID") ? (Guid)dicItem["DocID"] : (dicItem.ContainsKey("DocId") ? (Guid)dicItem["DocId"] : Guid.Empty);
                        Guid ItemGuid = dicItem.ContainsKey("GUID") ? (Guid)dicItem["GUID"] : Guid.Empty;
                        if (ItemDocId == Guid.Empty || ItemGuid == Guid.Empty)
                        {
                            continue;
                        }
                        itemsMapping[(Guid)dicItem["GUID"]] = ItemDocId;
                    }
                }
                //[ADO-25849]Replicator BPOS-S运行时间过长，缓存QueryListItemForFB的返回值
                AveQueryCacheForReplicator cacheInfo = GetAveQueryCacheForReplicator(siteId, webId, listId, Guid.Empty, folderUrl);
                Dictionary<AveQueryCacheForReplicator, Dictionary<string, object>> folderPropertyCache = new Dictionary<AveQueryCacheForReplicator, Dictionary<string, object>>();
                folderPropertyCache.Add(cacheInfo, folderProperty);
                mItemsAndFoldersCacheForReplicator = folderPropertyCache;
                return true;
            }
            catch (Exception e)
            {
                mRequest.Dispose(true);
                mLogger.Error(e.ToString());
            }
            return false;
            //bool isUnderWebRootFolder = folderCache.ParentList.ListID.Equals(Guid.Empty) || folderCache.ParentList == null ? true : false;
            //FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
            //throw new NotImplementedException();
        }

        public DateTime GetItemLastModifiedTime(Guid listId, int rowId)
        {
            throw new NotImplementedException();
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {
            throw new NotImplementedException();
        }

        public bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName)
        {
            return DiscoverRequest.IsHaveSameName(webId, listId, dirName, leafName);
        }

        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            return DiscoverRequest.IsListItemHaveSameName(siteId, webId, tpGuid, listId, rowId);
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
            List<AveWebPartObject> webparts = new List<AveWebPartObject>();
            Dictionary<string, object> webpartsProperties = new Dictionary<string, object>();
            if (mWebParts.ContainsKey(listId))
            {
                webpartsProperties = mWebParts[listId] as Dictionary<string, object>;
            }
            else
            {
                mWebParts.Clear();//以后如果需要都保留就不clear;
                webpartsProperties = DiscoverRequest.GetItemWebParts(siteId, webId, listId, itemDocId);
                mWebParts.Add(listId, webpartsProperties);
            }
            convertWebparts(webparts, webpartsProperties.ContainsKey(itemDocId.ToString()) ? webpartsProperties[itemDocId.ToString()] as List<Dictionary<string, object>> : new List<Dictionary<string, object>>());
            return webparts;
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

        public void ClearItemCache()
        {
            DiscoverRequest.ClearItemCache();
        }

        public void RemoveFolderCache(List<int> folderIds)
        {
            DiscoverRequest.RemoveFolderCache(folderIds);
        }

        public void QueryAttachmentByItemObj(Guid siteId, string listRootUrl, AveItemObject itemObj)
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

        private List<AveProjectObject> ConvertProjectObjects(List<Dictionary<string, object>> projectsProperties)
        {
            var results = new List<AveProjectObject>(projectsProperties.Count);
            foreach (var props in projectsProperties)
            {
                AveProjectObject proObj = new AveProjectObject();
                proObj.Name = props.ContainsKey("Name") ? props["Name"].ToString() : string.Empty;
                proObj.Id = props.ContainsKey("Id") ? (Guid)props["Id"] : Guid.Empty;
                proObj.IsEnterpriseProject = props.ContainsKey("IsEnterpriseProject") ? (bool)props["IsEnterpriseProject"] : false;
                proObj.CreatedDate = props.ContainsKey("CreatedDate") ? (DateTime)props["CreatedDate"] : DateTime.MinValue;
                proObj.LastSavedDate = props.ContainsKey("LastSavedDate") ? (DateTime)props["LastSavedDate"] : DateTime.MinValue;
                proObj.LastPublishedDate = props.ContainsKey("LastPublishedDate") ? (DateTime)props["LastPublishedDate"] : DateTime.MinValue;
                
                if (props.ContainsKey("ProjectSiteUrl") && props["ProjectSiteUrl"] != null)
                {
                    proObj.ProjectSiteUrl = props["ProjectSiteUrl"].ToString();
                }
                else
                {
                    proObj.ProjectSiteUrl = string.Empty;
                }
                results.Add(proObj);
            }
            return results;
        }

        private void ConvertWebObject(AveWebObject web, Dictionary<string, object> webProperty)
        {
            web.WebID = webProperty.ContainsKey("WebID") ? (Guid)webProperty["WebID"] : Guid.Empty;
            web.Name = webProperty.ContainsKey("Name") ? webProperty["Name"].ToString() : string.Empty;
            web.FullUrl = webProperty.ContainsKey("FullUrl") ? webProperty["FullUrl"].ToString() : string.Empty;
            web.EventTime = webProperty.ContainsKey("EventTime") ? (DateTime)webProperty["EventTime"] : DateTime.MinValue;
            web.Title = webProperty.ContainsKey("Title") ? webProperty["Title"].ToString() : string.Empty;
            web.NavigationChanged = webProperty.ContainsKey("NavigationChanged") ? (bool)webProperty["NavigationChanged"] : false;
            web.ChangeType = webProperty.ContainsKey("ChangeType") ? (ChangeType)webProperty["ChangeType"] : ChangeType.None;
            web.AppInstanceId = webProperty.ContainsKey("AppInstanceId") ? (Guid)webProperty["AppInstanceId"] : Guid.Empty;
            web.IsAppWeb = webProperty.ContainsKey("AppInstanceId") ? (Guid)webProperty["AppInstanceId"] != Guid.Empty : false;
        }

        private void ConvertSiteMemberObjects(Dictionary<int, AveSiteMemberObject> members, Dictionary<int, object> memberProperties)
        {
            foreach (KeyValuePair<int, object> memberProperty in memberProperties)
            {
                AveSiteMemberObject memberOjbect = new AveSiteMemberObject
                {
                    AddedMemberIds = new Dictionary<int, AveSiteMemberObject>(),
                    DeletedMemberIds = new Dictionary<int, AveSiteMemberObject>()
                };
                ConvertSiteMemberObject(memberOjbect, (Dictionary<string, object>)memberProperty.Value);
                members.Add(memberProperty.Key, memberOjbect);
            }
        }

        private void ConvertSiteMemberObject(AveSiteMemberObject memberObj, Dictionary<string, object> memberProperty)
        {
            memberObj.PrincipleId = (int)memberProperty["PrincipleId"];
            object member;
            if (memberProperty.TryGetValue("IsGroup", out member))
            {
                memberObj.IsGroup = (bool)member;
            }
            if (memberProperty.TryGetValue("IsUser", out member))
            {
                memberObj.IsUser = (bool)member;
                memberObj.IsDomainGroup = (bool)memberProperty["IsDomainGroup"];
                memberObj.Login = memberProperty["Login"].ToString();
            }
            memberObj.ChangeType = (ChangeType)memberProperty["ChangeType"];
            memberObj.EventTime = (DateTime)memberProperty["EventTime"];
            memberObj.Title = memberProperty["Title"].ToString();
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
            //add for SAAS-27045 在scan的时候将file的基础属性都load出来
            object fieldValuesObject;
            if (itemProperty.TryGetValue("FieldValues", out fieldValuesObject))
            {
                Dictionary<string, object> fieldValues = (Dictionary<string, object>)fieldValuesObject;
                item.CreatedBy = fieldValues.ContainsKey("Created_x0020_By") && fieldValues["Created_x0020_By"] != null && fieldValues["Created_x0020_By"].ToString() != string.Empty ? fieldValues["Created_x0020_By"].ToString() : (fieldValues.ContainsKey("Author") && fieldValues["Author"] != null && fieldValues["Author"].ToString() != string.Empty ? fieldValues["Author"].ToString() : string.Empty);
                item.ModifyBy = fieldValues.ContainsKey("Modified_x0020_By") && fieldValues["Modified_x0020_By"] != null && fieldValues["Modified_x0020_By"].ToString() != string.Empty ? fieldValues["Modified_x0020_By"].ToString() : (fieldValues.ContainsKey("Editor") && fieldValues["Editor"] != null && fieldValues["Editor"].ToString() != string.Empty ? fieldValues["Editor"].ToString() : string.Empty);
                item.Author = fieldValues.ContainsKey("Author") && fieldValues["Author"] != null && fieldValues["Author"].ToString() != string.Empty ? fieldValues["Author"].ToString() : string.Empty;
                item.Editor = fieldValues.ContainsKey("Editor") && fieldValues["Editor"] != null && fieldValues["Editor"].ToString() != string.Empty ? fieldValues["Editor"].ToString() : string.Empty;
            }
            //如果itemProperty["Length"]是int32类型，使用long进行转换会报错，所以这里使用Convert.ToInt64进行转换
            item.Length = itemProperty.ContainsKey("Length") ? Convert.ToInt64(itemProperty["Length"]) : 0; 
            item.TimeCreated = itemProperty.ContainsKey("TimeCreated") ? (DateTime)itemProperty["TimeCreated"] : (itemProperty.ContainsKey("Created") ? (DateTime)itemProperty["Created"] : DateTime.MinValue);

            item.TimeLastModified = itemProperty.ContainsKey("TimeLastModified") ? (DateTime)itemProperty["TimeLastModified"] : (itemProperty.ContainsKey("Modified") ? (DateTime)itemProperty["Modified"] : DateTime.MinValue);
            item.Uiversion = itemProperty.ContainsKey("UIVersion") ? (int)itemProperty["UIVersion"] : 512;
            item.ID = itemProperty.ContainsKey("DoclibRowId") ? (int?)itemProperty["DoclibRowId"] : (itemProperty.ContainsKey("Id") ? (int?)itemProperty["Id"] : null);
            item.FullUrl = itemProperty.ContainsKey("FullUrl") ? itemProperty["FullUrl"].ToString() : (itemProperty.ContainsKey("FileRef") ? itemProperty["FileRef"].ToString() : string.Empty);
            item.DirName = itemProperty.ContainsKey("DirName") ? itemProperty["DirName"].ToString() : (itemProperty.ContainsKey("FileDirRef") ? itemProperty["FileDirRef"].ToString() : string.Empty);
            item.DocFlags = itemProperty.ContainsKey("DocFlags") ? (int?)itemProperty["DocFlags"] : null;
            item.Hidden = itemProperty.ContainsKey("Hidden") ? (bool?)itemProperty["Hidden"] : null;
            item.ParentID = itemProperty.ContainsKey("ParentID") && itemProperty["ParentID"] is Guid ? (Guid)itemProperty["ParentID"] : Guid.Empty;
            item.CheckoutUserId = itemProperty.ContainsKey("CheckoutUserId") ? (int?)itemProperty["CheckoutUserId"] : null;
            item.Level = itemProperty.ContainsKey("Level") ? (byte)itemProperty["Level"] : Byte.MinValue;
            item.Type = itemProperty.ContainsKey("Type") ? Convert.ToByte(itemProperty["Type"]) : byte.MinValue;
            item.Size = itemProperty.ContainsKey("Size") ? (int)itemProperty["Size"] : 0;
            item.HasStream = itemProperty.ContainsKey("HasStream") ? Convert.ToBoolean(itemProperty["HasStream"]) : false;
            item.QueryType = itemProperty.ContainsKey("QueryType") ? (int)itemProperty["QueryType"] : 2;
            item.ServerRelativeUrl = itemProperty.ContainsKey("ServerRelativeUrl") ? itemProperty["ServerRelativeUrl"].ToString() : string.Empty;
            item.ChangeType = itemProperty.ContainsKey("ChangeType") ? (ChangeType)itemProperty["ChangeType"] : ChangeType.None;
            item.tp_GUID = itemProperty.ContainsKey("tp_GUID") ? (Guid)itemProperty["tp_GUID"] : (itemProperty.ContainsKey("GUID") ? (Guid)itemProperty["GUID"] : Guid.Empty);
            item.EventTime = itemProperty.ContainsKey("ChangeTime") ? (DateTime)itemProperty["ChangeTime"] : DateTime.MinValue;
            item.SPChangeType = itemProperty.ContainsKey("SPChangeType") ? itemProperty["SPChangeType"].ToString() : String.Empty;
            item.ViewId = itemProperty.ContainsKey("ViewId") ? (Guid)itemProperty["ViewId"] : Guid.Empty;
            item.IsSystemObject = itemProperty.ContainsKey("IsSystemFile") ? (bool)itemProperty["IsSystemFile"] : false;
            if (itemProperty.ContainsKey("ObjType"))
            {
                if ((int)itemProperty["ObjType"] == 1)
                {
                    item.ObjType = ItemType.Item;
                }
                else if ((int)itemProperty["ObjType"] == 2)
                {
                    item.ObjType = ItemType.Document;
                }
                else if ((int)itemProperty["ObjType"] == 4)
                {
                    item.ObjType = ItemType.Folder;
                }
            }
            item.HasGetLAT = itemProperty.ContainsKey("HasGetLAT") ? (bool)itemProperty["HasGetLAT"] : false;
            item.LastAccessTime = itemProperty.ContainsKey("LastAccessTime") ? (DateTime)itemProperty["LastAccessTime"] : DateTime.MinValue;
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
            item.ID = default(int);//itemProperty.ContainsKey("DocLibRowId") ? (int)itemProperty["DocLibRowId"] : default(int);
            item.EventTime = itemProperty.ContainsKey("ChangeTime") ? (DateTime)itemProperty["ChangeTime"] : DateTime.MinValue;
            item.SPChangeType = itemProperty.ContainsKey("SPChangeType") ? itemProperty["SPChangeType"].ToString() : "None";
            item.ObjType = ItemType.Document;
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
            list.ListTemplate = listProperty.ContainsKey("ListTemplate") ? (int)listProperty["ListTemplate"] : 0;
            list.ItemCount = listProperty.ContainsKey("ItemCount") ? (int)listProperty["ItemCount"] : -1;
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
        private void FillFolderObject(Dictionary<string, object> curFolder, AveItemObject rootFolder, bool isUnderWebRootFolder)
        {
            object folders;
            object items;
            object attachements;
            object versions;
            if (curFolder.TryGetValue("Folders", out folders))
            {
                foreach (Dictionary<string, object> folderDic in folders as List<Dictionary<string, object>>)
                {
                    AveItemObject folder = null;
                    string serverRelativeUrl = folderDic.ContainsKey("ServerRelativeUrl") ? folderDic["ServerRelativeUrl"].ToString() : folderDic.ContainsKey("FullUrl") ? folderDic["FullUrl"].ToString() : string.Empty;
                    mLogger.Info("folder server relative url:{0}", serverRelativeUrl);
                    string dirName = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'));
                    AveItemObject parentFolder = GetParentFolder(dirName, rootFolder);
                    foreach (var folderObject in parentFolder.SubFolderObjs)
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
                        if (folderDic["LeafName"] == null)
                        {
                            mLogger.Error("folderDic[\"LeafName\"]");
                        }
                        if (folderObject.LeafName.Equals(folderDic["LeafName"].ToString()))
                        {
                            folder = folderObject;
                            break;
                        }

                    }
                    if (folder == null)
                    {
                        folder = new AveItemObject();
                        if ((!folderDic.ContainsKey("IsSystemFile") || !Convert.ToBoolean(folderDic["IsSystemFile"]))
                            && folderDic.ContainsKey("LeafName"))
                        {
                            //make sure it is not systemfile
                            folder.FolderStructure = GetFolderStructureFromParent(folderDic["LeafName"].ToString(), parentFolder.FolderStructure);
                        }
                        parentFolder.SubFolderObjs.Add(folder);
                    }
                    if (folder.DocID.Equals(Guid.Empty))
                    {
                        if (isUnderWebRootFolder)
                        {
                            ConvertItemObjectForSystemFolder(folder, folderDic, true);
                        }
                        else
                        {
                            if (CacheItemProperties)
                            {
                                folder.ItemProperties = folderDic;
                            }
                            ConvertItemObject(folder, folderDic);
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
                    }
                }
            }

            if (curFolder.TryGetValue("Items", out items))
            {
                foreach (Dictionary<string, object> dicItem in items as List<Dictionary<string, object>>)
                {
                    if (dicItem.ContainsKey("FileSystemObjectType") 
                        && Convert.ToInt32(dicItem["FileSystemObjectType"]) ==(int)AveFileSystemObjectType.Folder)
                    {
                        continue;
                    }
                    var item = new AveItemObject();
                    if (isUnderWebRootFolder)
                    {
                        ConvertItemObjectForSystemFolder(item, dicItem, false);
                    }
                    else
                    {
                        if (CacheItemProperties)
                        {
                            item.ItemProperties = dicItem;
                        }
                        ConvertItemObject(item, dicItem);
                        if (dicItem.TryGetValue("ObjType", out object objType))
                        {
                            item.ObjType = (ItemType)objType;
                        }
                    }
                    string serverRelativeUrl = dicItem.ContainsKey("ServerRelativeUrl") ? dicItem["ServerRelativeUrl"].ToString() : dicItem.ContainsKey("FullUrl") ? dicItem["FullUrl"].ToString() : string.Empty;
                    string dirName = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'));
                    AveItemObject parentFolder = GetParentFolder(dirName, rootFolder);
                    if (parentFolder == null)
                    {
                        mLogger.Error("item server relative url:{0}", serverRelativeUrl);
                    }
                    parentFolder?.SubItemObjs.Add(item);
                    //if (item.ID.HasValue)
                    //{
                    List<IDictionary<string, object>> newVersions;
                    if(TryGetVersions(dicItem,out newVersions))
                    {
                        foreach (var dicVersion in newVersions)
                        {
                            var version = new AveVersionObject();
                            ConvertVersionObjectV1(version, dicVersion);
                            item.VersionObjs.Add(version);
                        }
                    }
                    else if (dicItem.TryGetValue("Versions", out versions))
                    {
                        foreach (Dictionary<string, object> dicVersion in versions as List<Dictionary<string, object>>)
                        {
                            var version = new AveVersionObject();
                            ConvertVersionObject(version, dicVersion);
                            item.VersionObjs.Add(version);
                        }
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
                }
            }
        }

        private void FillFolderObjectForArchiver(Dictionary<string, object> curFolder, AveItemObject rootFolder, bool isUnderWebRootFolder)
        {
            object folders;
            object items;
            object attachements;
            object versions;
            if (curFolder.TryGetValue("Folders", out folders))
            {
                foreach (Dictionary<string, object> folderDic in folders as List<Dictionary<string, object>>)
                {
                    AveItemObject folder = null;
                    string serverRelativeUrl = folderDic.ContainsKey("ServerRelativeUrl") ? folderDic["ServerRelativeUrl"].ToString() : folderDic.ContainsKey("FullUrl") ? folderDic["FullUrl"].ToString() : string.Empty;
                    mLogger.Info("folder server relative url:{0}", serverRelativeUrl);
                    string dirName = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'));
                    AveItemObject parentFolder = GetParentFolder(dirName, rootFolder);
                    foreach (var folderObject in parentFolder.SubFolderObjs)
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
                        if (folderDic["LeafName"] == null)
                        {
                            mLogger.Error("folderDic[\"LeafName\"]");
                        }
                        if (folderObject.LeafName.Equals(folderDic["LeafName"].ToString()))
                        {
                            folder = folderObject;
                            break;
                        }

                    }
                    if (folder == null)
                    {
                        folder = new AveItemObject();
                        if ((!folderDic.ContainsKey("IsSystemFile") || !Convert.ToBoolean(folderDic["IsSystemFile"]))
                            && folderDic.ContainsKey("LeafName"))
                        {
                            //make sure it is not systemfile
                            folder.FolderStructure = GetFolderStructureFromParent(folderDic["LeafName"].ToString(), parentFolder.FolderStructure);
                        }
                        parentFolder.SubFolderObjs.Add(folder);
                    }
                    if (folder.DocID.Equals(Guid.Empty))
                    {
                        if (isUnderWebRootFolder)
                        {
                            ConvertItemObjectForSystemFolder(folder, folderDic, true);
                        }
                        else
                        {
                            if (CacheItemProperties)
                            {
                                folder.ItemProperties = folderDic;
                            }
                            ConvertItemObject(folder, folderDic);
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
                                ConvertAttachmentObjectIncludeLength(attachmentObject, attachment);
                                folder.AttachmentObjs.Add(attachmentObject);
                            }
                        }
                    }
                }
            }

            if (curFolder.TryGetValue("Items", out items))
            {
                foreach (Dictionary<string, object> dicItem in items as List<Dictionary<string, object>>)
                {
                    if (dicItem.ContainsKey("FileSystemObjectType")
                        && Convert.ToInt32(dicItem["FileSystemObjectType"]) == (int)AveFileSystemObjectType.Folder)
                    {
                        continue;
                    }
                    var item = new AveItemObject();
                    if (isUnderWebRootFolder)
                    {
                        ConvertItemObjectForSystemFolder(item, dicItem, false);
                    }
                    else
                    {
                        if (CacheItemProperties)
                        {
                            item.ItemProperties = dicItem;
                        }
                        ConvertItemObject(item, dicItem);
                        if (dicItem.TryGetValue("ObjType", out object objType))
                        {
                            item.ObjType = (ItemType)objType;
                        }
                    }
                    string serverRelativeUrl = dicItem.ContainsKey("ServerRelativeUrl") ? dicItem["ServerRelativeUrl"].ToString() : dicItem.ContainsKey("FullUrl") ? dicItem["FullUrl"].ToString() : string.Empty;
                    string dirName = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/'));
                    AveItemObject parentFolder = GetParentFolder(dirName, rootFolder);
                    if (parentFolder == null)
                    {
                        mLogger.Error("item server relative url:{0}", serverRelativeUrl);
                    }
                    parentFolder.SubItemObjs.Add(item);
                    //if (item.ID.HasValue)
                    //{
                    List<IDictionary<string, object>> newVersions;
                    if (TryGetVersions(dicItem, out newVersions))
                    {
                        foreach (var dicVersion in newVersions)
                        {
                            var version = new AveVersionObject();
                            ConvertVersionObjectV1(version, dicVersion);
                            item.VersionObjs.Add(version);
                        }
                    }
                    else if (dicItem.TryGetValue("Versions", out versions))
                    {
                        foreach (Dictionary<string, object> dicVersion in versions as List<Dictionary<string, object>>)
                        {
                            var version = new AveVersionObject();
                            ConvertVersionObject(version, dicVersion);
                            item.VersionObjs.Add(version);
                        }
                    }
                    //}
                    if (dicItem.TryGetValue("Attachments", out attachements))
                    {
                        foreach (Dictionary<string, object> attachment in attachements as List<Dictionary<string, object>>)
                        {
                            var attachmentObject = new AveItemObject();
                            attachmentObject.ObjType = ItemType.Document;
                            ConvertAttachmentObjectIncludeLength(attachmentObject, attachment);
                            item.AttachmentObjs.Add(attachmentObject);
                        }
                    }
                }
            }
        }

        private bool TryGetVersions(Dictionary<string, object> itemProperties, out List<IDictionary<string,object>> versions)
        {
            object versionObject;
            if (itemProperties.TryGetValue("Versions" + AveObjectModelConstant.ObjectPropertySuffix, out versionObject))
            {
                versions = (versionObject as Dictionary<string, object>).GetChildren();
                if (versions != null)
                {
                    return true;
                }
            }
            versions = default(List<IDictionary<string, object>>);
            return false;
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
                mLogger.Info("dirName:{0},List root folder url:{1}", dirName, listRootFolderUrl);
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
                        ObjType = ItemType.Folder,
                        FolderStructure = GetFolderStructureFromParent(str, tempParentFolder.FolderStructure)
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
        private void ConvertVersionObject(AveVersionObject version, IDictionary<string, object> versionProperty)
        {
            long size = 0;
            if (long.TryParse(versionProperty["Size"].ToString(), out size))
            {
                version.Size = size;
            }
            version.TimeLastModified = (DateTime)versionProperty["TimeLastModified"];
            version.Uiversion = (int)versionProperty["UIVersion"];
            version.Level = (byte)versionProperty["Level"];
            version.IsCurrentVersion = (bool)versionProperty["IsCurrentVersion"];
            if (versionProperty.ContainsKey("UserDataGuid"))
            {
                version.UserDataGuid = (Guid)versionProperty["UserDataGuid"];
            }
            object obj_Type;
            if (versionProperty.TryGetValue("ObjType", out obj_Type))
            {
                version.ObjType = (ItemType)obj_Type;
            }
            //version.QueryType = (int)versionProperty["QueryType"];
        }

        private void ConvertVersionObjectV1(AveVersionObject version, IDictionary<string, object> versionProperty)
        {
            version.TimeLastModified = (DateTime)versionProperty["Modified"];
            version.Uiversion = (int)versionProperty["VersionId"];
            version.Level = (byte)versionProperty["Level"];
            version.IsCurrentVersion = (bool)versionProperty["IsCurrentVersion"];
            object lengthObj;
            if (versionProperty.TryGetValue("Length", out lengthObj))
            {
                version.Size = Convert.ToInt64(lengthObj);
            }
            object tp_Guid;
            if (versionProperty.TryGetValue("GUID", out tp_Guid))
            {
                version.UserDataGuid = (Guid)tp_Guid;
            }
            object obj_Type;
            if (versionProperty.TryGetValue("ObjType", out obj_Type))
            {
                version.ObjType = (ItemType)obj_Type;
            }
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
            attachmentObject.Size = (int)attachment["Size"];
            attachmentObject.ParentID = (Guid)attachment["ParentID"];
            attachmentObject.RbsId = (byte[])attachment["RbsId"];
            attachmentObject.FullUrl = attachment["FullUrl"].ToString();
            attachmentObject.CheckoutUserId = (int?)attachment["CheckoutUserId"];
            attachmentObject.HasStream = (bool)attachment["HasStream"];
            attachmentObject.ServerRelativeUrl = attachment["ServerRelativeUrl"].ToString();
            attachmentObject.ID = (int?)attachment["ID"];
            if (attachment.TryGetValue("AuthorObject", out object authorObj) && authorObj != null)
            {
                attachmentObject.Author = authorObj.ToString();
            }
            else
            {
                attachmentObject.Author = string.Empty;
            }
            attachmentObject.TimeCreated = (DateTime)attachment["TimeCreated"];
        }

        private void ConvertAttachmentObjectIncludeLength(AveItemObject attachmentObject, Dictionary<string, object> attachment)
        {
            ConvertAttachmentObject(attachmentObject, attachment);
            attachmentObject.Length = (long)attachment["Length"];
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
            listObj.ListTemplate = (int)list.BaseTemplate;
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
            if (folderObj != null)
            {
                folderObj.DocID = folder.UniqueId;
                folderObj.LeafName = folder.Name;
                folderObj.ID = folder.ID;
                folderObj.FullUrl = folderObj.FullUrl ?? folder.ServerRelativeUrl;
                folderObj.TimeCreated = folder.TimeCreated ?? default;
                folderObj.TimeLastModified = folder.TimeLastModified ?? default;
            }
            folderCache.InitBasicProperties(folder.ParentWeb.ID, folder.ParentListId, folderCache.ListId != Guid.Empty ? folder.ParentList.RootFolder.Url : string.Empty);
        }
        #endregion

        public IAveDiscoveryQuery CloneObjWithNewRequest(object aveRequest)
        {
            if (aveRequest != null && aveRequest is AveRequestParameter)
            {
                AveRequestParameter requestParameter = aveRequest as AveRequestParameter;
                if (requestParameter.AveRequest != null && !requestParameter.AveRequest.Url.TrimEnd('/').Equals(this.mSiteUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                { // here we need to check the url, otherwise we may use wrong bpos account info.
                    throw new ArgumentException("AveRequestParameter's url is not acceptable");
                }

                AveQuery newQuery = new AveQuery(this.mSiteUrl, requestParameter, this.mUserAccountInfo,
                    this.mStartTime, this.mEndTime, this.SupportIB, this.mChangeCache);
                return newQuery;
            }
            return this;
        }

        public int GetItemSize(Guid siteId, Guid webId, Guid listId, Guid docId, ref string createdBy, ref string modifiedBy)
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

        public void RemoveFolderCache(string folderServerRelativeUrl)
        {
            DiscoverRequest.RemoveFolderCache(folderServerRelativeUrl);
        }

        public void RemoveItemCache(int itemId)
        {
            DiscoverRequest.RemoveItemCache(itemId);
        }


        #region

        public void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder)
        {
            Dictionary<string, object> folderProperty = DiscoverRequest.QueryFolderForFB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, includeSystemFolder);
            bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
            FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
        }

        public void QuerySubItemsForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder, ref string pageInfo)
        {
            Dictionary<string, object> folderProperty = DiscoverRequest.QueryItemForFB(folderCache.SiteId, folderCache.WebId, folderCache.ListId, folderObject.DocID, folderObject.FullUrl, ref pageInfo, includeSystemFolder);
            bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
            FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
        }

        #endregion

        public PreDiscoverDesignListResult PreDiscoverDesignList(string siteUrl, Guid webId, Guid listId, bool includeGhostFile = false, bool includeEmptyFolder = false)
        {
            if (DiscoverRequest is IAveDMDiscoverRequest)
            {
                return (DiscoverRequest as IAveDMDiscoverRequest).PreDiscoverDesignList(siteUrl, webId, listId, includeGhostFile, includeEmptyFolder);
            }
            mLogger.Warn("DiscoverRequest is not IAveDMDiscoverRequest，will return default value.");
            return null;
        }

        private SPOFolder GetFolderStructureFromParent(string folderName, SPOFolder parentStructure)
        {
            if (parentStructure == null ||
                parentStructure.SubFolders == null || parentStructure.SubFolders.Count <= 0)
            {
                return null;
            }

            foreach (var folder in parentStructure.SubFolders)
            {
                if (string.Equals(folderName, folder.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return folder;
                }
            }
            return null;
        }

        public void QueryListRootFolderWithStructure(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject)
        {
            Dictionary<string, object> folder = DiscoverRequest.QueryListRootFolderWithStructureCache(listCache.SiteId, listCache.WebId, listCache.ListId);
            ConvertItemObject(rootFolderObject, folder);
            rootFolderObject.FolderStructure = folder["ListStructure"] as SPOFolder;
            rootFolderObject.ObjType = ItemType.Folder;
            rootFolderObject.DirName = folder["DirName"].ToString();
            rootFolderObject.FullUrl = folder["FullUrl"].ToString();
        }

        public void QueryListRootFolderWithStructureForArchiver(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject, SPOFolder SPOFolder)
        {
            Dictionary<string, object> folder = DiscoverRequest.QueryListRootFolder(listCache.SiteId, listCache.WebId, listCache.ListId);
            ConvertItemObject(rootFolderObject, folder);
            rootFolderObject.FolderStructure = SPOFolder;
            rootFolderObject.ObjType = ItemType.Folder;
            rootFolderObject.DirName = folder["DirName"].ToString();
            rootFolderObject.FullUrl = folder["FullUrl"].ToString();
        }

        public void QueryListRootFolderForFullDiscover(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject)
        {
            Dictionary<string, object> folder = DiscoverRequest.QueryListRootFolderForFullDiscover(listCache.SiteId, listCache.WebId, listCache.ListId);
            ConvertItemObject(rootFolderObject, folder);
            rootFolderObject.FolderStructure = folder["ListStructure"] as SPOFolder;
            rootFolderObject.ObjType = ItemType.Folder;
            rootFolderObject.DirName = folder["DirName"].ToString();
            rootFolderObject.FullUrl = folder["FullUrl"].ToString();
        }

        public IEnumerable<int> QuerySubFoldersWithStructureForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder)
        {
            List<int> foldersId = null;
            if (folderObject.FolderStructure != null && folderObject.FolderStructure.SubFolders != null &&
                folderObject.FolderStructure.SubFolders.Count > 0)
            {
                foldersId = folderObject.FolderStructure.SubFolders.Select(f => f.Id).ToList();
            }

            var listId = folderCache.ListId;
            Dictionary<string, string> needLoadFields = null;
            if (listId != Guid.Empty)
            {
                var list = folderCache.AveWeb.Lists[listId] as AveList;
                needLoadFields = list.NeedLoadFields;
            }
            foreach (Dictionary<string, object> folderProperty in mRequest.QueryFolderWithStructureForFB(
                folderCache.WebId, listId, folderObject.FullUrl, foldersId, needLoadFields, includeSystemFolder))
            {
                if (folderProperty == null)
                {
                    folderObject.SubFolderObjs.Clear();
                    if (foldersId != null)
                    {
                        foldersId.Clear();
                    }
                    yield return -1;
                }
                bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
                FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
                yield return folderObject.SubFolderObjs.Count;
                folderObject.SubFolderObjs.Clear();
            }

            if (foldersId != null)
            {
                foldersId.Clear();
                //TODO have any effective way to set subfolders free?
                //make sure it can get subfolders of each subfolders in later logic
            }
        }

        public IEnumerable<int> QuerySubItemsWithStructureForFB(AveFolderCache folderCache, AveItemObject folderObject)
        {
            IEnumerable<int> itemsId = null;
            if (folderObject.FolderStructure != null && folderObject.FolderStructure.Items.Count > 0)
            {
                itemsId = folderObject.FolderStructure.Items.Select(item => item.Id);
            }

            var listId = folderCache.ListId;
            Dictionary<string, string> needLoadFields = null;
            if (listId != Guid.Empty)
            {
                var list = folderCache.AveWeb.Lists[listId] as AveList;
                needLoadFields = list.NeedLoadFields;
            }
            foreach (Dictionary<string, object> folderProperty in mRequest.QueryItemWithStructureForFB(
                folderCache.WebId, listId, folderObject.FullUrl, itemsId, needLoadFields))
            {
                bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
                FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
                yield return folderObject.SubItemObjs.Count;
                folderObject.SubItemObjs.Clear();
            }

            if (itemsId != null)
            {
                folderObject.FolderStructure.Items.Clear();
            }
        }

        public IEnumerable<int> QuerySubItemsWithStructureForArchiverFB(AveFolderCache folderCache, AveItemObject folderObject)
        {
            IEnumerable<int> itemsId = null;
            if (folderObject.FolderStructure != null && folderObject.FolderStructure.Items != null &&
                folderObject.FolderStructure.Items.Count > 0)
            {
                itemsId = folderObject.FolderStructure.Items.Select(item => item.Id);
            }

            var listId = folderCache.ListId;
            Dictionary<string, string> needLoadFields = null;
            if (listId != Guid.Empty)
            {
                var list = folderCache.AveWeb.Lists[listId] as AveList;
                needLoadFields = list.NeedLoadFields;
            }
            foreach (Dictionary<string, object> folderProperty in (mRequest as IAveRequest).QueryItemWithStructureForArchiverFB(
                folderCache.WebId, listId, folderObject.FullUrl, itemsId, needLoadFields))
            {
                bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
                FillFolderObjectForArchiver(folderProperty, folderObject, isUnderWebRootFolder);
                yield return folderObject.SubItemObjs.Count;
                folderObject.SubItemObjs.Clear();
            }

            if (itemsId != null)
            {
                folderObject.FolderStructure.Items.Clear();
            }
        }


        public IEnumerable<int> QuerySubItemsWithStructureForRecordsFB(AveFolderCache folderCache, AveItemObject folderObject)
        {
            List<int> itemsId = null;
            if (folderObject.FolderStructure != null && folderObject.FolderStructure.Items != null &&
                folderObject.FolderStructure.Items.Count > 0)
            {
                return folderObject.FolderStructure.Items.Select(item => item.Id);
            }
            return null;
            //var listId = folderCache.ListId;
            //Dictionary<string, string> needLoadFields = null;
            //if (listId != Guid.Empty)
            //{
            //    var list = folderCache.AveWeb.Lists[listId] as AveList;
            //    needLoadFields = list.NeedLoadFields;
            //}
            //foreach (Dictionary<string, object> folderProperty in (mRequest as IAve2013Request).QueryItemWithStructureForArchiverFB(
            //    folderCache.WebId, listId, folderObject.FullUrl, itemsId, needLoadFields))
            //{
            //    bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
            //    FillFolderObjectForArchiver(folderProperty, folderObject, isUnderWebRootFolder);
            //    yield return folderObject.SubItemObjs.Count;
            //    folderObject.SubItemObjs.Clear();
            //}

            //if (itemsId != null)
            //{
            //    itemsId.Clear();
            //    folderObject.FolderStructure.Items.Clear();
            //}
        }
        /*public IEnumerable<int> QuerySubFoldersWithStructureForArchiverFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder)
        {
            List<int> foldersId = null;
            if (folderObject.FolderStructure != null && folderObject.FolderStructure.SubFolders != null &&
                folderObject.FolderStructure.SubFolders.Count > 0)
            {
                foldersId = folderObject.FolderStructure.SubFolders.Select(f => f.Id).ToList();
            }

            var listId = folderCache.ListId;
            Dictionary<string, string> needLoadFields = null;
            if (listId != Guid.Empty)
            {
                var list = folderCache.AveWeb.Lists[listId] as AveList;
                needLoadFields = list.NeedLoadFields;
            }
            foreach (Dictionary<string, object> folderProperty in (mRequest as IAveRequest).QueryFolderWithStructureForArchiverFB(
                folderCache.WebId, listId, folderObject.FullUrl, foldersId, needLoadFields, includeSystemFolder))
            {
                if (folderProperty == null)
                {
                    folderObject.SubFolderObjs.Clear();
                    if (foldersId != null)
                    {
                        foldersId.Clear();
                    }
                    yield return -1;
                }
                bool isUnderWebRootFolder = folderCache.ListId.Equals(Guid.Empty) ? true : false;
                FillFolderObject(folderProperty, folderObject, isUnderWebRootFolder);
                yield return folderObject.SubFolderObjs.Count;
                folderObject.SubFolderObjs.Clear();
            }

            if (foldersId != null)
            {
                foldersId.Clear();
                //TODO have any effective way to set subfolders free?
                //make sure it can get subfolders of each subfolders in later logic
            }
        }*/
    }
}
