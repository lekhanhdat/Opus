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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    public class AveDiscoverQueryForAPI : IAveDiscoveryQuery
    {
        public bool SupportIB
        {
            set;
            get;
        }

        private static AveLogger log = AveLogger.GetInstance(typeof(AveDiscoverQueryForAPI));

        #region cache property

        private IAveSite site;

        private IAveWeb web;
        private IAveTimeZone timeZone;
        private Dictionary<string, Guid> listFields = null;
        private AveBaseType listBaseType = AveBaseType.UnspecifiedBaseType;

        private IAveQuery FBQuery;

        private IAveDiscoverQueryService queryService;

        private AveDiscoverReader mDiscoverReader;

        private AveObjectModelFactory factory;

        private DateTime startTime;

        private DateTime endTime;

        private bool QueryVersionByNative;

        //WebId,Caches
        private Dictionary<Guid, List<IAveRecycleBinItem>> mFirstStageRecycleBinItemCache = new Dictionary<Guid, List<IAveRecycleBinItem>>();
        private List<IAveRecycleBinItem> mSecondStageRecycleBinItemCache = new List<IAveRecycleBinItem>();
        private bool hasQueryFirstStageRecycleBin;
        private bool hasQuerySecondStageRecycleBin;


        /// <summary>
        /// 1.特殊List下的Folder.item为，如果直接调用API会抛错，属于SharePoint API问题，直接返回Null即可。
        /// 2.这种List不需要备份List Item。
        /// 目前只发现External List。
        /// </summary>
        private static readonly List<AveListTemplateType> SpecialListTemplates = new List<AveListTemplateType>
        {
            AveListTemplateType.ExternalList
        };

        #endregion

        public AveDiscoverQueryForAPI(IAveSite site, AveObjectModelFactory factory, DateTime startTime, DateTime endTime, DiscoverModule module)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.Constructor"))
            {
                this.site = site;
                this.factory = factory;
                this.FBQuery = factory.CreateQuery();
                this.startTime = startTime;
                this.endTime = endTime;
                queryService = factory.CreateQueryService<IAveDiscoverQueryService>(site);

                mDiscoverReader = factory.CreateDiscoverReader(module);
                QueryVersionByNative = WrapperConfiguration.QueryVersionByNative;
                log.Info("Discover data by API.");
            }
        }

        private void SetWebIfChanged(Guid parentWebId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.SetWebIfChanged"))
            {
                if (this.web == null)
                {
                    this.web = site.OpenWeb(parentWebId);
                    this.timeZone = web.RegionalSettings != null ? web.RegionalSettings.TimeZone : null;
                }
                else if (this.web.ID != parentWebId)
                {
                    this.web.Dispose();
                    this.web = site.OpenWeb(parentWebId);
                    this.timeZone = web.RegionalSettings != null ? web.RegionalSettings.TimeZone : null;
                }
            }
        }

        private IAveChangeQuery GetQueryForIB(bool allObject, bool allChangeType, AveCollectionScope scope, Guid scopeId)
        {
            using (var scope2 = new AvePerformanceScope("AveDiscoverQueryForAPI.GetQueryForIB"))
            {
                var ibQuery = factory.CreateChangeQuery(allObject, allChangeType);
                ibQuery.ChangeTokenStart = factory.CreateChangeToken(scope, scopeId, this.startTime);
                ibQuery.ChangeTokenEnd = factory.CreateChangeToken(scope, scopeId, this.endTime);
                ibQuery.IgnoreStartTokenNotFoundError = true;
                if (factory.ContextKind != AveContextKind.Server07ObjectModel)
                {
                    ibQuery.FetchLimit = 2000;
                }
                return ibQuery;
            }
        }

        public void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitDiscoverWeb"))
            {
                var serverRelativeUrl = "/" + webObj.FullUrl.TrimStart('/');
                using (var web = site.OpenWeb(serverRelativeUrl))
                {
                    webObj.WebID = web.ID;
                    webObj.Name = web.IsRootWeb ? "." : web.Name;
                    webObj.Title = web.Title;
                }
            }
        }

        public void InitDiscoverList(AveListCache listCache, AveListObject listObj)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitDiscoverList"))
            {
                var list = listCache.AveWeb.GetList(listObj.RootFolderUrl);
                listObj.ListId = list.ID;
                listObj.Title = list.Title;
                listObj.Name = list.Title;
                listObj.RootFolderId = list.RootFolder.UniqueId;
                listObj.Type = (int)list.BaseType;
                //Flag这个值在SharePoint DB中是以bigint存的，而在SPList中这个属性的类型是ulong，
                //因此不太可能出现超过long.Max的情况，当前为了与QueryService保持一致，全部记录为long型。
                listObj.Flag = (long)list.Flags;
                listObj.RootFolderUrl = listObj.RootFolderUrl.Trim('/');
                listObj.ServerTemplate = (int?)list.BaseTemplate;
                listObj.Hidden = list.Hidden;
            }
        }

        public void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj, ref AveListObject parentListObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitDiscoverFolder"))
            {
                var web = folderCache.AveWeb;
                var folderUrl = folderObj.FullUrl;
                if (!string.IsNullOrEmpty(folderUrl) && !folderUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    folderUrl = "/" + folderUrl;
                }
                var folder = web.GetFolder(folderUrl);
                if (folder.Exists)
                {
                    folderObj.DocID = folder.UniqueId;
                    folderCache.InitBasicProperties(folderCache.AveWebCacheParameter.WebId, folder.ParentListId, string.Empty);
                    if (folderCache.ListId != Guid.Empty)
                    {
                        parentListObject = new AveListObject()
                        {
                            ListId = folder.ParentList.ID,
                            RootFolderId = folder.ParentList.RootFolder.UniqueId,
                            Name = folder.ParentList.Title,
                            Title = folder.ParentList.Title,
                            Type = (int)folder.ParentList.BaseType,
                            RootFolderUrl = folder.ParentList.RootFolder.ServerRelativeUrl.Trim('/'),
                            Flag = long.Parse(folder.ParentList.Flags.ToString()),
                            ServerTemplate = (int)folder.ParentList.BaseTemplate,
                            Hidden = folder.ParentList.Hidden
                        };
                    }
                }
                else
                {
                    log.Debug("The folder does not exist while init discover folder .URL:{0}", folderObj.FullUrl);
                }
            }
        }

        private static void GenerateRootFolderProperties(AveItemObject rootFolderObject, IAveFolder rootFolder)
        {
            rootFolderObject.ObjType = ItemType.Folder;
            rootFolderObject.DocID = rootFolder.UniqueId;
            rootFolderObject.Hidden = true;
            rootFolderObject.Uiversion = 512;
            rootFolderObject.Level = 1;
            rootFolderObject.Type = 1;
            rootFolderObject.QueryType = 2;
            rootFolderObject.IsCurrentVersion = true;
            rootFolderObject.DocFlags = 0;
            if (rootFolder.Properties.ContainsKey("vti_timelastmodified"))
            {
                rootFolderObject.TimeLastModified = (DateTime)rootFolder.Properties["vti_timelastmodified"];
            }
        }

        #region Site Level

        public int GetSiteChangedForIB(Guid siteId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetSiteChangedForIB.siteId"))
            {
                var ibQuery = GetQueryForIB(false, true, AveCollectionScope.Site, site.ID);
                ibQuery.Site = true;
                var type = ChangeType.None;
                var items = site.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        ChangeType changeType = DiscoverUtility.GetChangeType((NativeChangeType)item.Rows[(int)DiscoverRowName.EventType]);
                        if (changeType == ChangeType.Delete)
                        {
                            type = changeType;
                            break;
                        }
                        else
                        {
                            if (type != ChangeType.Add)
                            {
                                type = changeType;
                            }
                        }
                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = site.GetChanges(ibQuery);
                }
                return (int)type;
            }
        }

        public bool GetSiteChangedForIB(Guid siteId, ref ChangeType siteCollectionChangeType, ref ChangeType userChangeType, ref ChangeType groupChangeType)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetSiteChangedForIB.siteId.siteCollectionChangeType.userChangeType.groupChangeType"))
            {
                var ibQuery = GetQueryForIB(false, true, AveCollectionScope.Site, site.ID);
                ibQuery.User = true;
                ibQuery.Group = true;
                ibQuery.Site = true;
                bool changed = false;
                var items = site.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        changed = true;
                        ChangeType changeType = DiscoverUtility.GetChangeType((NativeChangeType)item.Rows[(int)DiscoverRowName.EventType]);
                        ChangeObjectType objectType = (ChangeObjectType)item.Rows[(int)DiscoverRowName.ObjectType];
                        switch (objectType)
                        {
                            case ChangeObjectType.Site:
                                if (siteCollectionChangeType == ChangeType.Add)
                                {
                                    if (changeType == ChangeType.Delete)
                                    {
                                        siteCollectionChangeType = changeType;
                                    }
                                }
                                else
                                {
                                    siteCollectionChangeType = changeType;
                                }
                                break;
                            case ChangeObjectType.Group:
                                groupChangeType |= changeType;
                                break;
                            case ChangeObjectType.User:
                                userChangeType |= changeType;
                                break;
                            default:
                                break;
                        }
                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = site.GetChanges(ibQuery);
                }
                return changed;
            }
        }

        public Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryWebForIB"))
            {
                List<Guid> notAvailableWebs = new List<Guid>();
                var ibQuery = GetQueryForIB(true, true, AveCollectionScope.Site, site.ID);
                //ibQuery.Web = true;
                var changeWebObjs = new Dictionary<Guid, AveWebObject>();
                var items = site.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        Guid webId = item.Rows[(int)DiscoverRowName.WebId] is DBNull ? Guid.Empty : (Guid)item.Rows[(int)DiscoverRowName.WebId];
                        if (webId == Guid.Empty)
                        {
                            continue;
                        }
                        var nativeChangeType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                        var ObjType = (ChangeObjectType)item.Rows[(int)DiscoverRowName.ObjectType];
                        AveWebObject webObj = null;
                        if (changeWebObjs.ContainsKey(webId))
                        {
                            webObj = changeWebObjs[webId];
                        }
                        else
                        {
                            if (ObjType == ChangeObjectType.Web && item.ChangeType == AveChangeType.Delete)
                            {
                                var deletedWeb = GetItemInRecycleBinForIB(site, item);
                                if (deletedWeb != null)
                                {
                                    webObj = new AveWebObject
                                    {
                                        WebID = webId,
                                        Name = deletedWeb.LeafName,
                                        FullUrl = string.IsNullOrEmpty(deletedWeb.DirName) ? deletedWeb.LeafName : deletedWeb.DirName + "/" + deletedWeb.LeafName,
                                        Title = deletedWeb.Title
                                    };
                                }
                                else
                                {
                                    webObj = new AveWebObject
                                    {
                                        WebID = webId,
                                        FullUrl = item.Rows[(int)DiscoverRowName.ItemFullUrl].ToString()
                                    };
                                }
                            }
                            else
                            {
                                if (notAvailableWebs.Contains(webId))
                                {
                                    continue;
                                }
                                try
                                {
                                    using (var tempWeb = this.site.OpenWeb(webId))
                                    {
                                        webObj = new AveWebObject
                                        {
                                            WebID = tempWeb.ID,
                                            Name = tempWeb.Name,
                                            FullUrl = tempWeb.ServerRelativeUrl,
                                            Title = tempWeb.Title
                                        };
                                    }
                                }
                                catch (Exception e)
                                {
                                    notAvailableWebs.Add(webId);
                                    log.Warn("An error occurred while getting this web. Url: {0}. Error: {1}", webId, e);
                                    continue;
                                }
                            }
                            changeWebObjs.Add(webId, webObj);
                        }

                        if (ObjType == ChangeObjectType.Web)
                        {
                            InitchangeWeb(item, nativeChangeType, webObj, changeWebObjs, webId);
                        }
                        else if (ObjType == ChangeObjectType.Field)
                        {
                            webObj.ColumnChangeType |= DiscoverUtility.GetChangeType(nativeChangeType);
                        }
                        else if (ObjType == ChangeObjectType.ContentType)
                        {
                            webObj.ContentTypeChangeType |= DiscoverUtility.GetChangeType(nativeChangeType);
                        }
                    }

                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = site.GetChanges(ibQuery);
                }
                return changeWebObjs;
            }
        }

        #region RecycleBin Methods
        private IAveRecycleBinItemCollection GetRecycleBinItems(IAveSite site, AveRecycleBinItemState recycleBinItemState)
        {
            var query = factory.CreateRecycleBinQuery();
            query.ItemState = recycleBinItemState;
            query.RowLimit = int.MaxValue - 1;
            return site.GetRecycleBinItems(query);

        }

        //一个Site Collection只查询一次Second Stage。
        private List<IAveRecycleBinItem> GetRecycleBinItemsInSecondStageForIB()
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetRecycleBinItemsInSecondStageForIB"))
            {
                lock (mSecondStageRecycleBinItemCache)
                {
                    if (!hasQuerySecondStageRecycleBin)
                    {
                        var secondStageCollection = GetRecycleBinItems(site, AveRecycleBinItemState.SecondStageRecycleBin);
                        if (secondStageCollection != null)
                        {
                            mSecondStageRecycleBinItemCache = secondStageCollection.Where(item =>
                                 item.DeletedDate >= this.startTime
                                 && item.DeletedDate <= this.endTime).ToList();
                        }
                    }
                    hasQuerySecondStageRecycleBin = true;
                    return mSecondStageRecycleBinItemCache;
                }
            }
        }

        private List<IAveRecycleBinItem> GetRecycleBinItemsInFirstStageForIB()
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetRecycleBinItemsInFirstStageForIB"))
            {
                lock (mFirstStageRecycleBinItemCache)
                {
                    List<IAveRecycleBinItem> result;
                    if (!mFirstStageRecycleBinItemCache.TryGetValue(web.ID, out result))
                    {
                        if (!hasQueryFirstStageRecycleBin)
                        {
                            var firstStageCollection = GetRecycleBinItems(web.Site, AveRecycleBinItemState.FirstStageRecycleBin);
                            if (firstStageCollection != null)
                            {
                                firstStageCollection.Where(item =>
                                        item.Web != null
                                        && item.DeletedDate >= this.startTime
                                        && item.DeletedDate <= this.endTime).GroupBy(item => item.Web.ID).All(
                                        item =>
                                        {
                                            mFirstStageRecycleBinItemCache[item.Key] = item.ToList();
                                            return true;
                                        });
                            }
                            mFirstStageRecycleBinItemCache.TryGetValue(web.ID, out result);
                            hasQueryFirstStageRecycleBin = true;
                        }
                    }
                    return result ?? new List<IAveRecycleBinItem>();
                }
            }
        }

        private IAveRecycleBinItem GetItemInRecycleBinForIB(IAveWeb web, IAveChange change)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetItemInRecycleBinForIB1"))
            {
                IAveRecycleBinItem result = null;
                if (!(change.Rows[(int)DiscoverRowName.ItemFullUrl] is DBNull))
                {
                    var url = change.Rows[(int)DiscoverRowName.ItemFullUrl].ToString();
                    //通过delete time和URL唯一确定。
                    result = GetRecycleBinItemsInFirstStageForIB().Find(item => CompareTime(item.DeletedDate, (DateTime)change.Rows[(int)DiscoverRowName.TimeLastModified])
                        && url.Equals(string.IsNullOrEmpty(item.DirName) ? item.LeafName : item.DirName + "/" + item.LeafName, StringComparison.OrdinalIgnoreCase));
                    if (result == null)
                    {
                        result = GetRecycleBinItemsInSecondStageForIB().Find(item => CompareTime(item.DeletedDate, (DateTime)change.Rows[(int)DiscoverRowName.TimeLastModified])
                        && url.Equals(string.IsNullOrEmpty(item.DirName) ? item.LeafName : item.DirName + "/" + item.LeafName, StringComparison.OrdinalIgnoreCase));
                    }
                }
                return result;
            }
        }

        //只有GetChange web调用，只获取一次。
        private IAveRecycleBinItem GetItemInRecycleBinForIB(IAveSite site, IAveChange change)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetItemInRecycleBinForIB2"))
            {
                var recycleBinItems = GetRecycleBinItemsInSecondStageForIB();
                if (!(change.Rows[(int)DiscoverRowName.ItemFullUrl] is DBNull))
                {
                    var url = change.Rows[(int)DiscoverRowName.ItemFullUrl].ToString();
                    //通过delete time和URL唯一确定。
                    return recycleBinItems.Find(item => CompareTime(item.DeletedDate, (DateTime)change.Rows[(int)DiscoverRowName.TimeLastModified])
                        && url.Equals(string.IsNullOrEmpty(item.DirName) ? item.LeafName : item.DirName + "/" + item.LeafName, StringComparison.OrdinalIgnoreCase));
                }
                return null;
            }
        }

        /// <summary>
        /// 比较Delete Time和LastModified用到。这里模拟SP的做法：Delete Time保留到Second，Millisecond级别四舍五入。
        /// </summary>
        /// <param name="deletedDate"></param>
        /// <param name="timeLastModified"></param>
        /// <returns></returns>
        private bool CompareTime(DateTime deletedDate, DateTime timeLastModified)
        {
            return Math.Abs((deletedDate - timeLastModified).TotalMilliseconds) <= 500;
        }

        private List<IAveRecycleBinItem> GetRecycleBinItemsInFirstStage()
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetRecycleBinItemsInFirstStage"))
            {
                lock (mFirstStageRecycleBinItemCache)
                {
                    List<IAveRecycleBinItem> result;
                    if (!mFirstStageRecycleBinItemCache.TryGetValue(web.ID, out result))
                    {
                        if (!hasQueryFirstStageRecycleBin)
                        {
                            var firstStageCollection = GetRecycleBinItems(site, AveRecycleBinItemState.FirstStageRecycleBin);
                            if (firstStageCollection != null)
                            {
                                firstStageCollection.Where(item => item.Web != null).GroupBy(item => item.Web.ID).All(
                                        item =>
                                        {
                                            mFirstStageRecycleBinItemCache[item.Key] = item.ToList();
                                            return true;
                                        });
                            }
                            mFirstStageRecycleBinItemCache.TryGetValue(web.ID, out result);
                            hasQueryFirstStageRecycleBin = true;
                        }
                    }
                    return result ?? new List<IAveRecycleBinItem>();
                }
            }
        }

        private List<IAveRecycleBinItem> GetRecycleBinItemsInSecondStage()
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetRecycleBinItemsInSecondStage"))
            {
                lock (mSecondStageRecycleBinItemCache)
                {
                    if (!hasQuerySecondStageRecycleBin)
                    {
                        var secondStageCollection = GetRecycleBinItems(site, AveRecycleBinItemState.SecondStageRecycleBin);
                        if (secondStageCollection != null)
                        {
                            mSecondStageRecycleBinItemCache = secondStageCollection.ToList();
                        }
                        hasQuerySecondStageRecycleBin = true;
                    }
                    return mSecondStageRecycleBinItemCache;
                }
            }
        }

        private void GetWebsInRecycleBin(Dictionary<Guid, AveWebObject> webs)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetWebsInRecycleBin"))
            {
                foreach (var item in GetRecycleBinItemsInSecondStage())
                {
                    if (item.ItemType == AveRecycleBinItemType.Web && item.DirName.Equals(web.ServerRelativeUrl.TrimStart('/'), StringComparison.Ordinal))
                    {
                        AveWebObject webObj = new AveWebObject
                        {
                            WebID = item.ID,
                            Name = item.Title,
                            Title = item.Title,
                            //API取不到RecycleBin中数据的DeleteTransactionId，先标记为0x1
                            DeleteTransactionId = new byte[] { 0x1 }
                        };
                        webs.Add(webObj.WebID, webObj);
                    }
                }
            }
        }

        private void GetListsInRecycleBin(Dictionary<Guid, AveListObject> listObjs)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetListsInRecycleBin"))
            {
                foreach (var item in GetRecycleBinItemsInFirstStage())
                {
                    InitAndAddListObjectInRecycleBin(item, listObjs);
                }
                foreach (var item in GetRecycleBinItemsInSecondStage())
                {
                    InitAndAddListObjectInRecycleBin(item, listObjs);
                }
            }
        }

        private void GetItemsInRecycleBin(IAveFolder parentFolder, AveItemObject folderObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetItemsInRecycleBin"))
            {
                foreach (var item in GetRecycleBinItemsInFirstStage())
                {
                    InitAndAddItemObjectInRecycleBin(parentFolder, folderObject, item);
                }
                foreach (var item in GetRecycleBinItemsInSecondStage())
                {
                    InitAndAddItemObjectInRecycleBin(parentFolder, folderObject, item);
                }
            }
        }

        private void GetAttachmentsInRecycleBin(IAveList parentList, AveItemObject itemObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetAttachmentsInRecycleBin"))
            {
                foreach (var item in GetRecycleBinItemsInFirstStage())
                {
                    InitAndAddAttachmentObjectInRecycleBin(itemObject, item, parentList);
                }
                foreach (var item in GetRecycleBinItemsInSecondStage())
                {
                    InitAndAddAttachmentObjectInRecycleBin(itemObject, item, parentList);
                }
            }
        }

        private void InitAndAddListObjectInRecycleBin(IAveRecycleBinItem item, Dictionary<Guid, AveListObject> listObjs)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitAndAddListObjectInRecycleBin"))
            {
                //List DirName:sites/ABC/Lists   Library DirName:sites/ABC
                if (item.ItemType == AveRecycleBinItemType.List && item.DirName.StartsWith(web.ServerRelativeUrl.TrimStart('/'), StringComparison.Ordinal))
                {
                    AveListObject listObj = new AveListObject
                    {
                        ListId = item.ID,
                        Name = item.Title,
                        Title = item.Title,
                        //API取不到RecycleBin中数据的DeleteTransactionId，先标记为0x1
                        DeleteTransactionId = new byte[] { 0x1 }
                    };
                    listObjs.Add(listObj.ListId, listObj);
                }
            }
        }

        private void InitAndAddItemObjectInRecycleBin(IAveFolder parentFolder, AveItemObject folderObject, IAveRecycleBinItem item)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitAndAddItemObjectInRecycleBin"))
            {
                if (item.DirName.Equals(parentFolder.ServerRelativeUrl.TrimStart('/'), StringComparison.Ordinal))
                {
                    AveItemObject itemObject;
                    switch (item.ItemType)
                    {
                        case AveRecycleBinItemType.ListItem:
                            itemObject = InitRecycleBinItemBasicProperty(item);
                            itemObject.ObjType = ItemType.Item;
                            folderObject.SubItemObjs.Add(itemObject);
                            break;
                        case AveRecycleBinItemType.File:
                            itemObject = InitRecycleBinItemBasicProperty(item);
                            itemObject.ObjType = ItemType.Document;
                            itemObject.Size = item.Size;
                            folderObject.SubItemObjs.Add(itemObject);
                            break;
                        case AveRecycleBinItemType.Folder:
                            itemObject = InitRecycleBinItemBasicProperty(item);
                            itemObject.ObjType = ItemType.Folder;
                            folderObject.SubFolderObjs.Add(itemObject);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void InitAndAddAttachmentObjectInRecycleBin(AveItemObject itemObject, IAveRecycleBinItem item, IAveList parentList)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitAndAddAttachmentObjectInRecycleBin"))
            {
                //Attachment DirName:sites/ABC/Lists/ListA/Attachment/1
                if (item.ItemType == AveRecycleBinItemType.Attachment
                && item.DirName.Equals(parentList.RootFolder.ServerRelativeUrl.TrimStart('/') + "/Attachments/" + itemObject.LeafName.Substring(0, itemObject.LeafName.LastIndexOf('_')), StringComparison.Ordinal))
                {
                    AveItemObject attachment = InitRecycleBinItemBasicProperty(item);
                    attachment.ObjType = ItemType.Document;
                    attachment.Size = item.Size;
                    itemObject.AttachmentObjs.Add(attachment);
                }
            }
        }

        private AveItemObject InitRecycleBinItemBasicProperty(IAveRecycleBinItem item)
        {
            AveItemObject itemObject = new AveItemObject()
            {
                DocID = item.ID,
                SourceName = item.LeafName,
                LeafName = item.LeafName,
                ItemName = item.LeafName,
                FullUrl = item.DirName + '/' + item.LeafName,
                DirName = item.DirName,
                Type = item.ItemType == AveRecycleBinItemType.Folder ? (byte)1 : (byte)0,
                TimeLastModified = item.DeletedDate,
                //API取不到RecycleBin中数据的DeleteTransactionId，先标记为0x1
                DeleteTransactionId = new byte[] { 0x1 }
            };
            return itemObject;
        }
        #endregion

        private void InitchangeWeb(IAveChange item, NativeChangeType nativeChangeType, AveWebObject webObj, Dictionary<Guid, AveWebObject> changeWebObjs, Guid webId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitChangeWeb"))
            {
                ChangeType preChange = webObj.ChangeType;
                ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                webObj.EventTime = (DateTime)item.Rows[(int)DiscoverRowName.EventTime];
                var url = item.Rows[(int)DiscoverRowName.ItemFullUrl];
                if (changeType == ChangeType.Delete && !(url is DBNull))
                {
                    webObj.FullUrl = url.ToString();
                }
                if (preChange == ChangeType.Add || preChange == ChangeType.Restore)
                {
                    if (changeType == ChangeType.Delete)
                    {
                        webObj.ChangeTypeBeforeDelete = webObj.ChangeType;
                        webObj.ChangeType = ChangeType.Delete;
                    }
                }
                else
                {
                    if (preChange == ChangeType.Delete && changeType == ChangeType.Restore)
                    {
                        webObj.ChangeType = webObj.ChangeTypeBeforeDelete;
                        if (webObj.ChangeType == ChangeType.None)
                        {
                            changeWebObjs.Remove(webId);
                        }
                    }
                    else
                    {
                        if (changeType == ChangeType.Delete)
                        {
                            webObj.ChangeTypeBeforeDelete = webObj.ChangeType;
                        }
                        webObj.ChangeType = changeType;
                    }
                }
                //提取web上删除Role与RoleAssignment事件的信息

                switch (nativeChangeType)
                {
                    case NativeChangeType.AssignmentAdd:
                    case NativeChangeType.ScopeAdd:
                        webObj.RoleAssignmentsChangeType |= ChangeType.Add;
                        break;
                    case NativeChangeType.AssignmentDelete:
                    case NativeChangeType.ScopeDelete:
                        webObj.RoleAssignmentsChangeType |= ChangeType.Delete;
                        break;
                    case NativeChangeType.RoleAdd:
                        webObj.PermissionLevelChangeType |= ChangeType.Add;
                        break;
                    case NativeChangeType.RoleUpdate:
                        webObj.PermissionLevelChangeType |= ChangeType.Edit;
                        break;
                    case NativeChangeType.RoleDelete:
                        webObj.PermissionLevelChangeType |= ChangeType.Delete;
                        break;
                    case NativeChangeType.Navigation:
                        webObj.NavigationChanged = true;
                        webObj.NavigationChangeType = ChangeType.Edit;
                        break;
                    default:
                        break;

                }

                if (nativeChangeType == NativeChangeType.RoleDelete || nativeChangeType == NativeChangeType.AssignmentDelete)
                {
                    var PrincipleId = item.Rows[(int)DiscoverRowName.Int0];
                    var roleId = item.Rows[(int)DiscoverRowName.Int1];
                    if (!(PrincipleId is DBNull) && !(roleId is DBNull))//&& !sr.IsDBNull(11))
                    {
                        AveSecurityObject deleteSecurity = new AveSecurityObject();
                        deleteSecurity.PrincipleId = -1;
                        deleteSecurity.RoleId = (int)roleId;
                        //deleteSecurity.RoleName = sr.GetString(11);  取不到 ！！！！！
                        deleteSecurity.ObjectType = SecurityType.Role;
                        deleteSecurity.EventTime = webObj.EventTime;
                        webObj.DeleteSecurities.Add(deleteSecurity);
                    }
                    if (!(PrincipleId is DBNull))//&& sr.IsDBNull(11))
                    {
                        AveSecurityObject deleteSecurity = new AveSecurityObject();
                        deleteSecurity.PrincipleId = (int)PrincipleId;
                        deleteSecurity.RoleId = roleId is DBNull ? -1 : (int)roleId;
                        deleteSecurity.ObjectType = SecurityType.Assignment;
                        deleteSecurity.EventTime = webObj.EventTime;
                        webObj.DeleteSecurities.Add(deleteSecurity);
                    }
                }
            }
        }

        public Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QuerySiteSecurityForIB"))
            {
                var ibQuery = GetQueryForIB(false, true, AveCollectionScope.Site, site.ID);
                ibQuery.User = true;
                ibQuery.Group = true;
                Dictionary<int, AveSiteMemberObject> memberChanges = new Dictionary<int, AveSiteMemberObject>();
                Dictionary<int, AveSiteMemberObject> userChanges = new Dictionary<int, AveSiteMemberObject>();
                Dictionary<int, AveSiteMemberObject> groupChanges = new Dictionary<int, AveSiteMemberObject>();
                var items = site.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var eventTime = (DateTime)item.Rows[(int)DiscoverRowName.EventTime];
                        int principalId = (int)item.Rows[(int)DiscoverRowName.Int0];
                        var eventType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                        var changeObjectType = (ChangeObjectType)item.Rows[(int)DiscoverRowName.ObjectType];
                        var member = item.Rows[(int)DiscoverRowName.ItemId];
                        AveSiteMemberObject memberChange = null;

                        if (!(userChanges.TryGetValue(principalId, out memberChange) || groupChanges.TryGetValue(principalId, out memberChange)))
                        {
                            memberChange = new AveSiteMemberObject()
                            {
                                PrincipleId = principalId,
                            };
                            if (changeObjectType == ChangeObjectType.Group)
                            {
                                memberChange.IsGroup = true;
                                groupChanges.Add(principalId, memberChange);
                            }
                            else
                            {
                                memberChange.IsUser = true;
                                userChanges.Add(principalId, memberChange);
                            }
                            //memberChanges.Add(principalId, memberChange);
                        }
                        memberChange.EventTime = eventTime;

                        //string title = sr.IsDBNull(3) ? string.Empty : sr.GetString(3);
                        //if (string.IsNullOrEmpty(memberChange.Title) || !memberChange.Title.Equals(title))
                        //{
                        //    memberChange.Title = title;
                        //}

                        ChangeType changeType = DiscoverUtility.GetChangeType(eventType);
                        if (memberChange.ChangeType == ChangeType.Add)
                        {
                            if (changeType == ChangeType.Delete)
                            {
                                memberChange.ChangeType = ChangeType.Delete;
                                continue;
                            }
                        }
                        else
                        {
                            memberChange.ChangeType = changeType;
                        }
                        #region Get group members
                        if (changeObjectType == ChangeObjectType.Group && !(member is DBNull))
                        {
                            int userId = (int)member;
                            var user = groupChanges.ContainsKey(userId) ? groupChanges[userId] : new AveSiteMemberObject
                            {
                                PrincipleId = userId,
                                IsUser = true,
                                EventTime = eventTime,

                            };
                            user.EventTime = eventTime;
                            if (eventType == NativeChangeType.MemberAdd)
                            {
                                if (memberChange.AddedMemberIds == null)
                                {
                                    memberChange.AddedMemberIds = new Dictionary<int, AveSiteMemberObject>();
                                }
                                memberChange.AddedMemberIds.Add(userId, user);
                            }
                            else if (eventType == NativeChangeType.MemberDelete)
                            {
                                if (memberChange.DeletedMemberIds == null)
                                {
                                    memberChange.DeletedMemberIds = new Dictionary<int, AveSiteMemberObject>();
                                }
                                memberChange.DeletedMemberIds.Add(userId, user);
                                if (memberChange.AddedMemberIds != null && memberChange.AddedMemberIds.ContainsKey(userId))
                                {
                                    memberChange.AddedMemberIds.Remove(userId);
                                }
                            }
                        }
                        #endregion

                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;


                    items = site.GetChanges(ibQuery);
                }
                //此处为给user和Group添加Login等属性的方法。
                queryService.QueryUserOrGroupProperty(userChanges, siteId, ChangeObjectType.User);
                queryService.QueryUserOrGroupProperty(groupChanges, siteId, ChangeObjectType.Group);
                memberChanges = userChanges.Concat(groupChanges).ToDictionary(k => k.Key, v => v.Value);
                return memberChanges;
            }
        }

        #endregion

        #region Web Level

        public void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryWebRootFolder"))
            {
                SetWebIfChanged(listCache.WebId);
                var rootFolder = this.web.GetFolder("");
                var folderServerRelativeUrl = rootFolder.ServerRelativeUrl.Trim('/');
                string dirName;
                string leafName;
                AveUrlUtility.SplitUrl(folderServerRelativeUrl, out dirName, out leafName);
                rootFolderObject.DirName = dirName;
                rootFolderObject.SourceName = rootFolderObject.LeafName = rootFolderObject.ItemName = leafName;
                rootFolderObject.FullUrl = folderServerRelativeUrl;
                GenerateRootFolderProperties(rootFolderObject, rootFolder);
            }
        }

        public Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryWebSecurityForIB"))
            {
                SetWebIfChanged(webId);
                var ibQuery = GetQueryForIB(false, false, AveCollectionScope.Web, this.web.ID);
                ibQuery.Web = true;
                ibQuery.RoleAssignmentAdd = true;
                ibQuery.RoleAssignmentDelete = true;
                ibQuery.RoleDefinitionAdd = true;
                ibQuery.RoleDefinitionDelete = true;
                ibQuery.RoleDefinitionUpdate = true;
                Dictionary<int, List<AveSecurityObject>> webSecurityChanges = new Dictionary<int, List<AveSecurityObject>>();
                var items = site.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var nativeChangeType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                        SecurityType securityType = DiscoverUtility.GetSecurityObjectType(nativeChangeType);
                        ChangeType changeType = DiscoverUtility.GetSecurityChangeType(nativeChangeType);

                        switch (securityType)
                        {
                            case SecurityType.Role:
                                try
                                {
                                    RoleSecurityChange(changeType, item, webSecurityChanges);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebSecurityForIB.SecurityType.Role. EventTime:{0}.  ErrorMessage:{1}.", item.Rows[(int)DiscoverRowName.EventTime].ToString(), e.ToString());
                                }
                                break;
                            case SecurityType.Assignment:
                                try
                                {
                                    AssignmentSecurityChange(changeType, item, webSecurityChanges);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", item.Rows[(int)DiscoverRowName.EventTime].ToString(), e.ToString());
                                }
                                break;
                            case SecurityType.Scope: //break inherate
                                try
                                {
                                    ScopeSecurityChange(changeType, item, webSecurityChanges);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebSecurityForIB.SecurityType.Scope.EventTime:{0}.  ErrorMessage:{1}", item.Rows[(int)DiscoverRowName.EventTime].ToString(), e.ToString());
                                }
                                break;
                            case SecurityType.None:
                                break;
                            default:
                                break;
                        }
                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = site.GetChanges(ibQuery);
                }
                return webSecurityChanges;
            }
        }

        //copy
        private void RoleSecurityChange(ChangeType changeType, IAveChange item, Dictionary<int, List<AveSecurityObject>> securityChanges)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.RoleSecurityChange"))
            {
                int roleId = item.Rows[(int)DiscoverRowName.Int1] is DBNull ? -1 : (int)item.Rows[(int)DiscoverRowName.Int1];
                List<AveSecurityObject> roleSecuritys = null;
                securityChanges.TryGetValue(AveSecurityObject.RoleChangeId, out roleSecuritys);
                if (roleSecuritys == null)
                {
                    roleSecuritys = new List<AveSecurityObject>();
                    securityChanges.Add(AveSecurityObject.RoleChangeId, roleSecuritys);
                }

                AveSecurityObject security = TryGetRoleSecurity(roleSecuritys, roleId);

                if (security.ChangeType == ChangeType.Add)
                {
                    if (changeType == ChangeType.Delete)
                    {
                        roleSecuritys.Remove(security);
                        DeleteAllRelatedRole(securityChanges, roleId);
                        return;
                    }
                }
                else
                {
                    security.ChangeType = changeType;
                }
                if (security.ChangeType == ChangeType.Delete)
                {
                    DeleteAllRelatedRole(securityChanges, roleId);
                    return;
                }
                security.ScopeId = (Guid)(item.Rows[(int)DiscoverRowName.Guid0] ?? Guid.Empty);
            }
        }

        //copy
        private AveSecurityObject TryGetRoleSecurity(List<AveSecurityObject> securitys, int roleId)
        {
            foreach (AveSecurityObject asc in securitys)
            {
                if (asc.RoleId == roleId)
                {
                    return asc;
                }
            }
            AveSecurityObject security = new AveSecurityObject
            {
                RoleId = roleId,
                ObjectType = SecurityType.Role
            };
            securitys.Add(security);
            return security;
        }

        //copy
        private void DeleteAllRelatedRole(Dictionary<int, List<AveSecurityObject>> securityChanges, int roleId)
        {
            foreach (var kvp in securityChanges)
            {
                if (kvp.Key != AveSecurityObject.RoleChangeId && kvp.Key != AveSecurityObject.ScopeChangeId)
                { // we shoud delete scope and principle relate current role
                    foreach (AveSecurityObject asc in kvp.Value)
                    {
                        if (asc.RoleId == roleId)
                        {
                            kvp.Value.Remove(asc);
                        }
                    }
                }
            }
        }

        //copy
        private void AssignmentSecurityChange(ChangeType changeType, IAveChange item, Dictionary<int, List<AveSecurityObject>> securityChanges)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.AssignmentSecurityChange"))
            {
                int principleId = (int)item.Rows[(int)DiscoverRowName.Guid0];
                int roleId = item.Rows[(int)DiscoverRowName.Int1] is DBNull ? -1 : (int)item.Rows[(int)DiscoverRowName.Int1];
                if (principleId == null || roleId == null) //assign role to principle 
                {
                    //有assignment事件的时候，可定时关联RoleId和PrincipleId
                    return;
                }
                List<AveSecurityObject> securitys = null;
                AveSecurityObject security = new AveSecurityObject();
                securityChanges.TryGetValue(principleId, out securitys);
                if (securitys == null)
                {
                    securitys = new List<AveSecurityObject>();
                    securityChanges.Add(principleId, securitys);
                }
                security = TryGetAssignmentSecurity(securitys, roleId);
                if (security.ChangeType == ChangeType.Add)
                {
                    if (security.ChangeType == ChangeType.Delete)
                    {
                        securitys.Remove(security);
                        DeleteAllRelatedRole(securityChanges, roleId);
                        return;
                    }
                }
                else
                {
                    security.ChangeType = changeType;
                }
                if (security.ChangeType == ChangeType.Delete)
                {
                    DeleteAllRelatedRole(securityChanges, roleId);
                    return;
                }
                security.ScopeId = (Guid)(item.Rows[(int)DiscoverRowName.Guid0] ?? Guid.Empty);
            }
        }

        //copy
        private AveSecurityObject TryGetAssignmentSecurity(List<AveSecurityObject> Securitys, int roleId)
        {
            AveSecurityObject security = new AveSecurityObject();
            foreach (AveSecurityObject asc in Securitys)
            {
                if (asc.RoleId == roleId)
                {
                    return asc;
                }
            }
            security.RoleId = roleId;
            security.ObjectType = SecurityType.Assignment;
            Securitys.Add(security);
            return security;
        }

        //copy
        private void ScopeSecurityChange(ChangeType changeType, IAveChange item, Dictionary<int, List<AveSecurityObject>> mSecurityChanges)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.ScopeSecurityChange"))
            {
                Guid scopeId = (Guid)item.Rows[(int)DiscoverRowName.Guid0];
                int scopeRoleId = item.Rows[(int)DiscoverRowName.Int1] is DBNull ? -1 : (int)item.Rows[(int)DiscoverRowName.Int1];
                List<AveSecurityObject> scopeSecuritys = null;
                mSecurityChanges.TryGetValue(AveSecurityObject.ScopeChangeId, out scopeSecuritys);
                if (scopeSecuritys == null)
                {
                    scopeSecuritys = new List<AveSecurityObject>();
                    mSecurityChanges.Add(AveSecurityObject.ScopeChangeId, scopeSecuritys);
                }

                AveSecurityObject scopeSecurity = TryGetScopeSecurity(scopeSecuritys, scopeId);

                if (scopeSecurity.ChangeType == ChangeType.Add)
                {
                    if (changeType == ChangeType.Delete)
                    {
                        scopeSecuritys.Remove(scopeSecurity);
                        return;
                    }
                }
                else
                {
                    scopeSecurity.ChangeType = changeType;
                }
                if (scopeSecurity.ChangeType == ChangeType.Delete)
                {
                    return;
                }
                scopeSecurity.RoleId = scopeRoleId;
            }
        }

        //copy
        private AveSecurityObject TryGetScopeSecurity(List<AveSecurityObject> securitys, Guid scopeId)
        {
            foreach (var asc in securitys)
            {
                if (asc.ScopeId == scopeId)
                {
                    return asc;
                }
            }
            AveSecurityObject security = new AveSecurityObject
            {
                ScopeId = scopeId,
                ObjectType = SecurityType.Scope
            };
            securitys.Add(security);
            return security;
        }

        public Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListForIB"))
            {
                SetWebIfChanged(webId);
                var getListFailedCache = new List<Guid>();
                var ibQuery = GetQueryForIB(false, true, AveCollectionScope.Web, webId);
                ibQuery.List = true;
                ibQuery.Folder = true;
                ibQuery.File = true;
                ibQuery.Item = true;
                ibQuery.View = true;
                ibQuery.Alert = true;
                var listObjs = new Dictionary<Guid, AveListObject>();
                var items = web.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        try
                        {
                            Guid listId = item.Rows[(int)DiscoverRowName.ListId] is DBNull ? Guid.Empty : (Guid)item.Rows[(int)DiscoverRowName.ListId];
                            if (listId == Guid.Empty)
                            {
                                //ToDo  system folder
                                continue;
                            }
                            var ObjType = (ChangeObjectType)item.Rows[(int)DiscoverRowName.ObjectType];
                            NativeChangeType nativeChangeType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                            ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);

                            AveListObject listObj = null;
                            if (!listObjs.ContainsKey(listId))
                            {
                                listObj = new AveListObject
                                {
                                    ListId = listId
                                };
                                var rootFolderUrl = item.Rows[(int)DiscoverRowName.ItemFullUrl];
                                if (ObjType == ChangeObjectType.List && changeType == ChangeType.Delete)
                                {
                                    var deletedList = GetItemInRecycleBinForIB(web, item);
                                    if (deletedList != null)
                                    {
                                        listObj.RootFolderUrl = rootFolderUrl.ToString();
                                        listObj.Name = deletedList.Title;//rootFolderUrl.ToString().Contains("/") ? rootFolderUrl.ToString().Substring(rootFolderUrl.ToString().LastIndexOf('/') + 1) : rootFolderUrl.ToString();
                                        listObj.Title = deletedList.Title;
                                        listObj.ModifiedTime = deletedList.DeletedDate;
                                        listObj.ModifiedBy = deletedList.DeletedBy.LoginName;
                                        //API取不到被删除List的 Flag
                                    }
                                    else//在回收站中没找到。
                                    {
                                        listObj.RootFolderUrl = rootFolderUrl.ToString();
                                    }
                                    listObjs.Add(listId, listObj);
                                }
                                else
                                {
                                    if (getListFailedCache.Contains(listId))
                                    {
                                        continue;
                                    }
                                    try
                                    {
                                        var list = this.web.GetList(listId);
                                        listObj.RootFolderUrl = list.RootFolder.ServerRelativeUrl;
                                        var index = listObj.RootFolderUrl.LastIndexOf('/');
                                        listObj.Name = index >= 0 ? listObj.RootFolderUrl.Substring(index + 1) : listObj.RootFolderUrl;
                                        listObj.Title = listObj.Name;
                                        listObj.Flag = Convert.ToInt64(list.Flags);
                                        listObjs.Add(listId, listObj);
                                    }
                                    catch (Exception e)
                                    {
                                        getListFailedCache.Add(listId);
                                        log.Warn("An error occurred while getting list by Id. Web: {0}, List Id: {1},  Error: {2}", this.web.Url, listId, e);
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                listObj = listObjs[listId];
                                if (listObj.Flag == null)
                                {
                                    if (getListFailedCache.Contains(listId))
                                    {
                                        continue;
                                    }
                                    try
                                    {
                                        var list = this.web.GetList(listId);
                                        listObj.Flag = Convert.ToInt64(list.Flags);
                                    }
                                    catch (Exception e)
                                    {
                                        getListFailedCache.Add(listId);
                                        log.Warn("An error occurred while getting list flag Id. Web: {0}, List Id: {1},  Error: {2}", this.web.Url, listId, e);
                                    }
                                }
                            }

                            if (ObjType == ChangeObjectType.List)
                            {
                                listObj.ModifiedTime = (DateTime)item.Rows[(int)DiscoverRowName.EventTime];
                                ChangeType currentType = listObj.ChangeType;
                                if (currentType == ChangeType.Add ||
                                    currentType == ChangeType.Restore)
                                {
                                    if (changeType == ChangeType.Delete)
                                    {
                                        var deletedList = GetItemInRecycleBinForIB(web, item);
                                        if (deletedList != null)
                                        {
                                            listObj.ModifiedBy = deletedList.DeletedBy.Name;
                                        }
                                        listObj.ChangeTypeBeforeDelete = listObj.ChangeType;
                                        listObj.ChangeType = ChangeType.Delete;
                                    }
                                    //otherwise not change.
                                }
                                else //"None or Edit", change to "Edit or Delete".
                                {
                                    if (currentType == ChangeType.Delete &&
                                        changeType == ChangeType.Restore)
                                    {
                                        //currentList.ListCache.ChangeType = currentList.ListCache.ChangeTypeBeforeDelete;
                                        listObj.ChangeType = listObj.ChangeTypeBeforeDelete;
                                        if (listObj.ChangeType == ChangeType.None)
                                        {
                                            listObjs.Remove(listId);
                                        }
                                    }
                                    else
                                    {
                                        if (changeType == ChangeType.Delete)
                                        {
                                            listObj.ChangeTypeBeforeDelete = listObj.ChangeType;
                                            listObj.ChangeType = changeType;
                                            var deletedList = GetItemInRecycleBinForIB(web, item);
                                            if (deletedList != null)
                                            {
                                                listObj.ModifiedBy = deletedList.DeletedBy.Name;
                                            }
                                        }
                                        else if (changeType != ChangeType.None)
                                        {
                                            listObj.ChangeType = changeType;
                                        }
                                    }
                                }
                                //提取list上删除RoleAssignment事件的信息

                                switch (nativeChangeType)
                                {
                                    case NativeChangeType.AssignmentDelete:
                                    case NativeChangeType.AssignmentAdd:
                                    case NativeChangeType.ScopeDelete:
                                    case NativeChangeType.ScopeAdd:
                                        listObj.RoleAssignmentsChangeType = ChangeType.Edit;
                                        break;
                                    default:
                                        break;
                                }

                                if (nativeChangeType == NativeChangeType.AssignmentDelete)
                                {
                                    if (!(item.Rows[(int)DiscoverRowName.Int0] is DBNull))
                                    {
                                        AveSecurityObject deleteRoleAssignment = new AveSecurityObject();
                                        // 删除RoleAssignmet时，第13个字段为int0,第14个字段为int1
                                        // int0存放principalID,int1存放RoleID
                                        deleteRoleAssignment.ObjectType = SecurityType.Assignment;
                                        deleteRoleAssignment.PrincipleId = (int)item.Rows[(int)DiscoverRowName.Int0];
                                        if (!(item.Rows[(int)DiscoverRowName.Int1] is DBNull))
                                        {
                                            deleteRoleAssignment.RoleId = (int)item.Rows[(int)DiscoverRowName.Int1];
                                        }
                                        //如果int1为Null，说明把该user/group的权限全部移除了
                                        else
                                        {
                                            deleteRoleAssignment.RoleId = -1;
                                        }
                                        deleteRoleAssignment.EventTime = listObj.ModifiedTime;
                                        listObj.DeleteRoleAssignments.Add(deleteRoleAssignment);
                                    }
                                }
                            }
                            else if (ObjType == ChangeObjectType.Alert && listId != Guid.Empty && !(item.Rows[(int)DiscoverRowName.DocId] is DBNull))
                            {
                                listObj.AlertChangeType = ChangeType.Edit;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.WARN, "Error occur while Get Change List From EventCache Table. ErrorMessage:{0}", ex.ToString());
                        }
                    }

                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = web.GetChanges(ibQuery);
                }
                return listObjs;
            }
        }

        //copy
        private bool IsContainContentTypeId(Dictionary<byte[], AveContentTypeObject> contentTypeChanges, byte[] contentTypeId, out AveContentTypeObject contentTypeChange)
        {
            foreach (var kvp in contentTypeChanges)
            {
                byte[] bs = kvp.Key;
                if (bs.Length != contentTypeId.Length)
                {
                    continue;
                }
                else
                {
                    int i = 0;
                    for (; i < bs.Length; i++)
                    {
                        if (bs[i] != contentTypeId[i])
                        {
                            break;
                        }
                    }
                    if (i == bs.Length)
                    {
                        contentTypeChange = kvp.Value;
                        return true;
                    }
                }
            }
            contentTypeChange = null;
            return false;
        }

        //copy
        private void RemoveContentType(Dictionary<byte[], AveContentTypeObject> ContentTypeChanges, byte[] contentTypeId)
        {
            foreach (var kvp in ContentTypeChanges)
            {
                byte[] bs = kvp.Key;
                if (bs.Length != contentTypeId.Length)
                {
                    continue;
                }
                else
                {
                    int i = 0;
                    for (; i < bs.Length; i++)
                    {
                        if (bs[i] != contentTypeId[i])
                        {
                            break;
                        }
                    }
                    if (i == bs.Length)
                    {
                        ContentTypeChanges.Remove(kvp.Key);
                        return;
                    }
                }
            }
        }

        #endregion

        #region List Level

        public void QueryListRootFolder(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListRootFolder"))
            {
                SetWebIfChanged(listCache.WebId);
                var list = this.web.GetList(listCache.ListId);
                var rootFolder = list.RootFolder;
                rootFolderObject.SourceName = rootFolderObject.LeafName = rootFolderObject.ItemName = rootFolder.Name;
                rootFolderObject.DirName = rootFolder.ParentFolder.ServerRelativeUrl.Trim('/');
                rootFolderObject.FullUrl = string.Format("{0}/{1}", rootFolderObject.DirName.Trim('/'), rootFolderObject.LeafName).Trim('/');
                GenerateRootFolderProperties(rootFolderObject, rootFolder);
            }
        }

        public Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListAlertForIB"))
            {
                var changeAlerts = new Dictionary<Guid, AveAlertObject>();
                SetWebIfChanged(webId);
                var list = this.web.GetList(listId);
                var ibQuery = GetQueryForIB(false, true, AveCollectionScope.List, listId);
                ibQuery.Alert = true;
                var items = list.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var nativeChangeType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                        ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                        Guid alertId = (Guid)item.Rows[(int)DiscoverRowName.Guid0];
                        //AveAlertObject alert = null;
                        AveAlertObject alert = null;
                        if (changeAlerts.ContainsKey(alertId))
                        {
                            alert = changeAlerts[alertId];
                            if (alert.ChangeType == ChangeType.Add)
                            {
                                if (changeType == ChangeType.Delete)
                                {
                                    changeAlerts.Remove(alertId);
                                }
                            }
                        }
                        else
                        {
                            //if (sr.IsDBNull(6) && sr.IsDBNull(7) ||
                            //    sr.GetString(4).ToLower(CultureInfo.InvariantCulture).Contains("filterpath") ||
                            //    sr.GetString(5).ToLower(CultureInfo.InvariantCulture).Contains("filterpath"))
                            //{
                            //    //this alert is delete we can't know the alert belong to this list or folder
                            //    //or it is folder alert
                            //    continue;
                            //}
                            //API 取list 的alert 变化，不应该存在此问题，暂时去掉??????????
                            alert = new AveAlertObject
                            {
                                Id = alertId,
                                ChangeType = changeType
                            };
                            changeAlerts.Add(alertId, alert);
                        }
                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = list.GetChanges(ibQuery);
                }
                return changeAlerts;
            }
        }

        public Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListViewForIB"))
            {
                SetWebIfChanged(webId);
                var list = this.web.GetList(listId);
                var ibQuery = GetQueryForIB(false, true, AveCollectionScope.List, listId);
                ibQuery.View = true;
                var changeViews = new Dictionary<Guid, AveViewObject>();
                var items = list.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        ChangeType changeType = DiscoverUtility.GetChangeType((NativeChangeType)item.Rows[(int)DiscoverRowName.EventType]);
                        Guid viewId = (Guid)item.Rows[(int)DiscoverRowName.Guid0];
                        AveViewObject viewChange = null;
                        if (!changeViews.ContainsKey(viewId))
                        {
                            viewChange = new AveViewObject();
                            try
                            {
                                var view = list.GetView(viewId);
                                viewChange.ViewID = viewId;
                                viewChange.ViewType = (int)view.Flag;
                                viewChange.IsPersonalView = (view.Flag & 262144) == 262144 ? true : false;
                                viewChange.BaseViewId = Byte.Parse(view.BaseViewId);
                                viewChange.ViewTitle = view.Title;
                                viewChange.PageUrlID = this.web.GetFile(view.ServerRelativeUrl).UniqueId;
                                //viewChange.ViewUserID      暂时没有找到API 取personal view 的user id   ?????????????
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, "The view may be deleted by custom,because it con not be found in list.viewId:{0},listTitle:{1}. Error message: {2}", viewId, list.Title, e.ToString());
                            }
                            changeViews.Add(viewId, viewChange);
                        }
                        viewChange = changeViews[viewId];
                        if (viewChange.ChangeType == ChangeType.Add)
                        {
                            if (changeType == ChangeType.Delete)
                            {
                                changeViews.Remove(viewId);
                            }
                        }
                        else
                        {
                            viewChange.ChangeType = changeType;
                        }
                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = list.GetChanges(ibQuery);
                }
                return changeViews;
            }
        }

        public Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListSecurityForIB"))
            {
                SetWebIfChanged(webId);
                var list = this.web.GetList(listId);
                var ibQuery = GetQueryForIB(false, false, AveCollectionScope.List, listId);
                ibQuery.List = true;
                ibQuery.RoleAssignmentAdd = true;
                ibQuery.RoleAssignmentDelete = true;
                var securityChanges = new Dictionary<int, List<AveSecurityObject>>();
                var items = list.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var nativeChangeType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                        SecurityType securityType = DiscoverUtility.GetSecurityObjectType(nativeChangeType);
                        ChangeType changeType = DiscoverUtility.GetSecurityChangeType(nativeChangeType);

                        switch (securityType)
                        {
                            case SecurityType.Assignment:
                                try
                                {
                                    AssignmentSecurityChange(changeType, item, securityChanges);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occur while access data from QueryListSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", item.Rows[(int)DiscoverRowName.EventTime].ToString(), e.ToString());
                                }
                                break;
                            case SecurityType.Scope: //break inherate
                                try
                                {
                                    ScopeSecurityChange(changeType, item, securityChanges);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occur while access data from QueryListSecurityForIB.SecurityType.Scope. EventTime:{0}.  ErrorMessage:{1}", item.Rows[(int)DiscoverRowName.EventTime].ToString(), e.ToString());
                                }
                                break;
                            case SecurityType.None:
                                break;
                            default:
                                break;
                        }
                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = list.GetChanges(ibQuery);
                }
                return securityChanges;
            }
        }

        public Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListContentTypeForIB"))
            {
                SetWebIfChanged(webId);
                var list = this.web.GetList(listId);
                var ibQuery = GetQueryForIB(false, true, AveCollectionScope.List, listId);
                ibQuery.List = true;
                ibQuery.ContentType = true;
                var contentTypeChanges = new Dictionary<byte[], AveContentTypeObject>();
                var items = list.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        try
                        {
                            var nativeChangeType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                            ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                            if (nativeChangeType == NativeChangeType.ListContenTypeAdd)
                            {
                                changeType = ChangeType.Add;
                            }
                            else if (nativeChangeType == NativeChangeType.ListContenTypeDelete)
                            {
                                changeType = ChangeType.Delete;
                            }

                            var objType = (ChangeObjectType)item.Rows[(int)DiscoverRowName.ObjectType];
                            var contentTypeId = (byte[])item.Rows[(int)DiscoverRowName.ContentTypeId];
                            AveContentTypeObject contentTypeChange = null;

                            if (!IsContainContentTypeId(contentTypeChanges, contentTypeId, out contentTypeChange))
                            {
                                //contentTypeChange = new AveContentTypeObject { ContentTypeId = contentTypeId };
                                contentTypeChange = new AveContentTypeObject
                                {
                                    ContentTypeId = contentTypeId
                                };
                                contentTypeChanges.Add(contentTypeId, contentTypeChange);
                            }
                            if (contentTypeChange.ChangeType == ChangeType.Add)
                            {
                                if (changeType == ChangeType.Delete)
                                {
                                    RemoveContentType(contentTypeChanges, contentTypeId);
                                }
                            }

                            else
                            {
                                contentTypeChange.ChangeType = changeType;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while getting changed contentTypes in list:{0},Error:{1}", list.Title, e);
                        }
                    }
                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = list.GetChanges(ibQuery);
                }
                return contentTypeChanges;
            }
        }

        //TODO
        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            SetWebIfChanged(folderCache.WebId);
            var ibQuery = GetQueryForIB(false, true, AveCollectionScope.Web, folderCache.WebId);
            ibQuery.File = true;
            ibQuery.Folder = true;
        }
        [Obsolete("no use now, will remove later")]
        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DiscoverModeForSOIB discoverMode, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            QuerySystemListItemForIB(folderCache, folderObject, extraItems);
        }

        private void EnsureListInfo(IAveList list)
        {
            if (listFields == null)
            {
                listFields = new Dictionary<string, Guid>(list.Fields.Count);
                if (list != null)
                {
                    log.Debug("Ensure List Info. List Title:{0}", list.Title);
                    listFields = list.Fields.ToDictionary(field => field.InternalName, field => field.ID);
                    listBaseType = list.BaseType;
                }
            }
        }

        private bool CheckVersions(AveItemObject itemObject)
        {
            if (listBaseType == AveBaseType.Survey
                || listBaseType == AveBaseType.UnspecifiedBaseType
                || listBaseType == AveBaseType.Unused)
                return false;
            if (itemObject.Uiversion <= 0) return false;

            if (listBaseType == AveBaseType.DocumentLibrary)
            {
                if (itemObject.Uiversion == 1) return false;
            }
            else
            {
                if (itemObject.Uiversion == 512) return false;
            }

            return true;
        }

        private bool FieldExist(string internalName)
        {
            return listFields.ContainsKey(internalName);
        }

        private void DisposeCache()
        {
            if (listFields != null)
            {
                listFields.Clear();
                listFields = null;
            }
            listBaseType = AveBaseType.UnspecifiedBaseType;
        }

        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListItemForIB"))
            {
                try
                {
                    SetWebIfChanged(folderCache.WebId);
                    var list = this.web.GetList(folderCache.ListId);
                    EnsureListInfo(list);
                    var ibQuery = GetQueryForIB(false, true, AveCollectionScope.List, folderCache.ListId);
                    ibQuery.File = true;
                    ibQuery.Folder = true;
                    ibQuery.Item = true;
                    ibQuery.View = true;
                    var items = new Dictionary<int, AveItemObject>();
                    var systemItems = new Dictionary<Guid, AveItemObject>();
                    var attachments = new Dictionary<int, List<AveItemObject>>();
                    var viewIdAndFileMapping = new Dictionary<Guid, IAveFile>();
                    Dictionary<string, AveItemObject> noPropertyFolders = new Dictionary<string, AveItemObject>();
                    var attParentFolderUrl = list.RootFolder.ServerRelativeUrl.Trim('/') + "/Attachments/";
                    var changeItems = list.GetChanges(ibQuery);
                    while (changeItems.Count > 0)
                    {
                        foreach (var currentItem in changeItems)
                        {
                            DateTime eventTime = (DateTime)currentItem.Rows[(int)DiscoverRowName.EventTime];
                            var nativeChangeType = (NativeChangeType)currentItem.Rows[(int)DiscoverRowName.EventType];
                            ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                            var objectType = (ChangeObjectType)currentItem.Rows[(int)DiscoverRowName.ObjectType];
                            switch (objectType)
                            {
                                case ChangeObjectType.View:
                                    Guid viewId = (Guid)currentItem.Rows[(int)DiscoverRowName.Guid0];
                                    IAveFile viewFile = GetViewFileByViewId(list, viewId, viewIdAndFileMapping);
                                    if (viewFile != null)
                                    {
                                        FillSystemFileProperty(folderObject, systemItems, list, viewFile, currentItem, eventTime, nativeChangeType, changeType, objectType, noPropertyFolders);
                                    }
                                    break;
                                case ChangeObjectType.File:
                                    var docId = (Guid)currentItem.Rows[(int)DiscoverRowName.DocId];
                                    var attFullUrl = currentItem.Rows[(int)DiscoverRowName.ItemFullUrl].ToString();
                                    if (attFullUrl.IndexOf(attParentFolderUrl, StringComparison.OrdinalIgnoreCase) >= 0 && list.BaseType != AveBaseType.DocumentLibrary)
                                    {
                                        FillAttachmentProperty(items, docId, list, attachments, changeType, attFullUrl);
                                        break;
                                    }
                                    IAveFile aveFile;

                                    if (IsSystemFile(docId, out aveFile))
                                    {
                                        FillSystemFileProperty(folderObject, systemItems, list, aveFile, currentItem, eventTime, nativeChangeType, changeType, objectType, noPropertyFolders);
                                    }
                                    else//ChangeObjectType是File的话，这个条件会走？
                                    {
                                        FillProperty(folderObject, list, items, currentItem, eventTime, nativeChangeType, changeType, objectType, noPropertyFolders);
                                    }
                                    break;
                                case ChangeObjectType.Item:
                                case ChangeObjectType.Folder:
                                    FillProperty(folderObject, list, items, currentItem, eventTime, nativeChangeType, changeType, objectType, noPropertyFolders);
                                    break;
                            }
                        }
                        ibQuery.ChangeTokenStart = changeItems.LastChangeToken;
                        changeItems = list.GetChanges(ibQuery);
                    }
                    try
                    {
                        HandleExtraItem(folderCache, folderObject, listObject, extraItems, items, systemItems, attParentFolderUrl, attachments, noPropertyFolders);
                    }
                    catch (Exception extra)
                    {
                        log.Error("An error occurred while get Extra Item. Error message:{0}", extra);
                    }
                    if (QueryVersionByNative)
                    {
                        queryService.QueryItemVersionsForAPI(items, listObject, mDiscoverReader);
                    }
                    FillNoPropertyFolders(noPropertyFolders, listObject);
                    FillAttachment(items, attachments);
                    List<AveItemObject> allItems = items.Values.ToList();
                    foreach (var systemItem in systemItems)
                    {
                        if (allItems.FirstOrDefault(item => item.DocID == systemItem.Key) == null)
                        {
                            allItems.Add(systemItem.Value);
                        }
                    }
                    queryService.SetItemStubInfo(allItems, folderCache.SiteId);
                }
                finally
                {
                    DisposeCache();
                }
            }
        }
        [Obsolete("no use now, will remove later")]
        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, DiscoverModeForSOIB discoverMode, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            QueryListItemForIB(folderCache, folderObject, listObject, extraItems);
        }

        private void HandleExtraItem(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems, Dictionary<int, AveItemObject> items, Dictionary<Guid, AveItemObject> systemItems, string attParentFolderUrl, Dictionary<int, List<AveItemObject>> attachments, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            if (extraItems == null || extraItems.Count == 0) return;
            SetWebIfChanged(folderCache.WebId);
            var list = this.web.GetList(folderCache.ListId);
            EnsureListInfo(list);
            foreach (var extraItem in extraItems)
            {
                var docId = extraItem.Id;
                switch (extraItem.ObjectType)
                {
                    case ChangeObjectType.File:
                        var attFullUrl = extraItem.Url;
                        if (attFullUrl.IndexOf(attParentFolderUrl, StringComparison.OrdinalIgnoreCase) >= 0 && list.BaseType != AveBaseType.DocumentLibrary)
                        {
                            FillAttachmentProperty(items, docId, list, attachments, ChangeType.Edit, attFullUrl);
                            break;
                        }
                        IAveFile aveFile;

                        if (IsSystemFile(docId, out aveFile))
                        {
                            FillExtraSystemFileProperty(folderObject, systemItems, list, aveFile, noPropertyFolders);
                        }
                        else//ChangeObjectType是File的话，这个条件会走？
                        {
                            FillExtraItemProperty(extraItem, list, folderObject, folderCache, items, noPropertyFolders);
                        }
                        break;
                    case ChangeObjectType.Item:
                    case ChangeObjectType.Folder:
                        FillExtraItemProperty(extraItem, list, folderObject, folderCache, items, noPropertyFolders);
                        break;
                }
            }
        }
        private void FillExtraSystemFileProperty(AveItemObject folderObject, Dictionary<Guid, AveItemObject> systemItems, IAveList list, IAveFile aveFile, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.FillSystemFileProperty"))
            {
                if (!aveFile.Exists)
                {
                    return;
                }
                AveItemObject parentFolder = null;
                string fullName = aveFile.ServerRelativeUrl.TrimStart('/');
                string itemName = aveFile.Name;
                string dirName = aveFile.ParentFolder.ServerRelativeUrl.TrimStart('/');
                if ((parentFolder = GetParentFolder(dirName, folderObject, noPropertyFolders)) == null)
                {
                    return;
                }
                AveItemObject itemOrFolder = GernerateSystemFile(list, systemItems, aveFile, fullName, itemName, dirName, parentFolder);

            }
        }
        private void FillExtraItemProperty(AveDiscoverExtraItemBaseInfo extraItem, IAveList list, AveItemObject folderObject, AveFolderCache folderCache, Dictionary<int, AveItemObject> items, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            IAveListItem aveItem = null;
            string fullName = string.Empty;
            string itemName;
            string dirName;
            IAveSite checkoutSite = null;
            IAveWeb checkoutWeb = null;
            AveItemObject parentFolder = null;
            try
            {
                AveListTemplateType listType;
                try
                {
                    aveItem = GetListItemByDocLibRowId(extraItem.RowId, list, out checkoutSite, out checkoutWeb, out listType);
                }
                catch (Exception e)
                {
                    //TODO   暂时不处理找不到的item
                    log.Warn("Can not get item by guid. item row id: {0}, List Id: {1},  error message: {2}", extraItem.RowId, list.ID, e);
                    var aveFolder = GetFolderByUniqutId(list, extraItem.Id);
                    if ((aveFolder != null) && GetParentFolder(aveFolder.ParentFolder.ServerRelativeUrl.Trim('/'), folderObject, noPropertyFolders) == null)
                    {
                        if (extraItem.ObjectType == ChangeObjectType.Folder && extraItem.Id.Equals(folderObject.DocID))
                        {
                            folderObject.ChangeType = ChangeType.Edit;
                        }
                    }
                }
                if (aveItem != null)
                {
                    fullName = list.RootFolder.ServerRelativeUrl.Substring(1, list.RootFolder.ServerRelativeUrl.Length - list.RootFolder.Url.Length - 1) + aveItem.Url;
                    //此处对于listItem，itemName必须得用fullName来截，不能用aveItem.Name
                    itemName = fullName.Substring(fullName.LastIndexOf('/') + 1);
                    //要注意rootSC存在fullName.equals(itemName)的情况
                    dirName = fullName.Substring(0, fullName.Length - itemName.Length).TrimEnd('/');
                    if ((parentFolder = GetParentFolder(dirName, folderObject, noPropertyFolders)) == null)
                    {
                        if (extraItem.ObjectType == ChangeObjectType.Folder && extraItem.Id == folderObject.DocID)
                        {
                            folderObject.ChangeType = ChangeType.Edit;
                        }
                        return;
                    }
                    AveItemObject itemOrFolder;
                    if (aveItem.Folder != null)
                    {
                        itemOrFolder = GenerateFolder(items, extraItem.Id, aveItem, fullName, itemName, dirName, parentFolder);
                    }
                    else //ListItem or Document
                    {
                        itemOrFolder = GernerateItem(list, items, aveItem, fullName, itemName, dirName, parentFolder);
                    }
                }
            }
            finally
            {
                DisposeCheckoutSPObjct(checkoutSite, checkoutWeb);
            }
        }
        public void QueryChangedListItemFromExtraItemList(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryChangedListItemFromRCDB"))
            {
                try
                {
                    if (extraItems == null || extraItems.Count == 0) return;
                    SetWebIfChanged(folderCache.WebId);
                    var list = this.web.GetList(folderCache.ListId);
                    EnsureListInfo(list);
                    var ibQuery = GetQueryForIB(false, true, AveCollectionScope.List, folderCache.ListId);
                    ibQuery.File = true;
                    ibQuery.Folder = true;
                    ibQuery.Item = true;
                    ibQuery.View = true;
                    var items = new Dictionary<int, AveItemObject>();
                    var systemItems = new Dictionary<Guid, AveItemObject>();
                    var attachments = new Dictionary<int, List<AveItemObject>>();
                    var viewIdAndFileMapping = new Dictionary<Guid, IAveFile>();
                    Dictionary<string, AveItemObject> noPropertyFolders = new Dictionary<string, AveItemObject>();
                    var attParentFolderUrl = list.RootFolder.ServerRelativeUrl.Trim('/') + "/Attachments/";
                    try
                    {
                        HandleExtraItem(folderCache, folderObject, listObject, extraItems, items, systemItems, attParentFolderUrl, attachments, noPropertyFolders);
                    }
                    catch (Exception extra)
                    {
                        log.Error("An error occurred while get Extra Item. Error message:{0}", extra);
                    }
                    if (QueryVersionByNative)
                    {
                        queryService.QueryItemVersionsForAPI(items, listObject, mDiscoverReader);
                    }
                    FillNoPropertyFolders(noPropertyFolders, listObject);
                    FillAttachment(items, attachments);
                    var allitems = systemItems.Values.Union(items.Values).ToList();
                    queryService.SetItemStubInfo(allitems, folderCache.SiteId);
                }
                finally
                {
                    DisposeCache();
                }
            }
        }

        private IAveFile GetViewFileByViewId(IAveList list, Guid viewId, Dictionary<Guid, IAveFile> viewIdAndFileMapping)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetViewFileByViewId"))
            {
                IAveFile viewFile = null;
                try
                {
                    if (!viewIdAndFileMapping.TryGetValue(viewId, out viewFile))
                    {
                        var view = list.GetView(viewId);
                        viewFile = this.web.GetFile(view.ServerRelativeUrl);
                        if (viewFile.Exists)
                        {
                            viewIdAndFileMapping.Add(viewId, viewFile);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while get list view file by viewId. listId:{0}, viewId:{1}, error:{2}.", list.ID, viewId, e);
                }
                return viewFile;
            }
        }

        private void FillAttachmentProperty(Dictionary<int, AveItemObject> items, Guid docId, IAveList list, Dictionary<int, List<AveItemObject>> attachments, ChangeType changeType, string attachmentFullUrl)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.FillAttachmentProperty"))
            {
                string[] strs = attachmentFullUrl.Split('/');
                var itemId = Convert.ToInt32(strs[strs.Length - 2], CultureInfo.InvariantCulture);
                if (items.ContainsKey(itemId))
                {
                    var itemOject = items[itemId];
                    itemOject.ID = itemId;
                    var attObject = GetAttachmentObject(itemOject, docId, list, attachments, attachmentFullUrl, changeType);
                }
            }
        }
        private static void FillAttachment(Dictionary<int, AveItemObject> items, Dictionary<int, List<AveItemObject>> attachments)
        {
            foreach (var attachmentOneCache in attachments)
            {
                var id = attachmentOneCache.Key;
                if (items.ContainsKey(id))
                {
                    items[id].AttachmentObjs = attachmentOneCache.Value;
                }
            }
        }

        private void FillNoPropertyFolders(Dictionary<string, AveItemObject> noPropertyFolders, AveListObject listObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.FillNoPropertyFolders"))
            {
                var folderCollectionForNativeQuery = new Dictionary<int, AveItemObject>();
                foreach (var kv in noPropertyFolders)
                {
                    try
                    {
                        var folder = kv.Value;
                        var tempFolder = this.web.GetFolder("/" + folder.FullUrl.Trim('/'));
                        folder.IsCurrentVersion = true;
                        if (!tempFolder.Exists)
                        {
                            kv.Value.ChangeType = ChangeType.Delete;
                        }
                        else
                        {
                            folder.DocID = tempFolder.UniqueId;
                            //folder.PropertyAdded = true;
                            folder.ObjType = ItemType.Folder;
                            if (tempFolder.Properties.ContainsKey("vti_level"))
                            {
                                folder.Level = Byte.Parse(tempFolder.Properties["vti_level"].ToString());
                            }
                            if (tempFolder.Properties.ContainsKey("vti_timelastmodified"))
                            {
                                folder.TimeLastModified = (DateTime)tempFolder.Properties["vti_timelastmodified"];
                            }
                            if (tempFolder.Item != null)
                            {
                                folder.ID = tempFolder.Item.ID;
                                QueryVersionOrCacheVersionItem(folderCollectionForNativeQuery, folder, tempFolder.Item);
                            }
                            folder.Uiversion = GetFolderUIVersion(tempFolder);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while generating the noProperty folder.Url:{0},Error:{1}", kv.Value.FullUrl, e.ToString());
                    }
                }
                if (QueryVersionByNative)
                {
                    queryService.QueryItemVersionsForAPI(folderCollectionForNativeQuery, listObject, mDiscoverReader);
                }
            }
        }

        private void QueryVersionOrCacheVersionItem(Dictionary<int, AveItemObject> folderCollectionForNativeQuery, AveItemObject folder, IAveListItem item)
        {
            if (!QueryVersionByNative)
            {
                GenerateVersion(item, folder);
            }
            else
            {
                if (folder.ID.HasValue && folder.ID != 0)
                {
                    folderCollectionForNativeQuery[folder.ID.Value] = folder;
                }
            }
        }

        private AveItemObject GetAttachmentObject(AveItemObject itemOject, Guid docId, IAveList list, Dictionary<int, List<AveItemObject>> attachments, string fullUrl, ChangeType changeType)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetAttachmentObject"))
            {
                try
                {
                    var itemId = itemOject.ID.Value;
                    if (!attachments.ContainsKey(itemId))
                    {
                        attachments[itemId] = new List<AveItemObject>();
                    }
                    var parentItem = list.GetItemByUniqueId(itemOject.DocID);
                    var attObject = GetAttachment(attachments[itemId], docId, parentItem, fullUrl);

                    if (attObject.ChangeType == ChangeType.Add || attObject.ChangeType == ChangeType.Restore)
                    {
                        if (changeType == ChangeType.Delete)
                        {
                            attachments[itemId].Remove(attObject);
                        }
                    }
                    else
                    {
                        if (attObject.ChangeType == ChangeType.Delete && changeType == ChangeType.Restore)
                        {
                            attachments[itemId].Remove(attObject);
                        }
                        else
                        {
                            attObject.ChangeType = changeType;
                        }
                    }
                    return attObject;

                }
                catch (Exception e)
                {
                    log.Warn("Can not get attachment info, error message: {0}.", e.ToString());
                }
                return new AveItemObject();
            }
        }

        private int GetFolderUIVersion(IAveFolder folder)
        {
            var version = 512;
            if (folder.ParentListId != Guid.Empty)
            {
                if (folder.Item != null)
                {
                    if (FieldExist("_UIVersion"))
                    {
                        version = (int)folder.Item["_UIVersion"];
                    }
                }
                else
                {
                    if (IsPublishWeb(folder.ParentWeb)
                       && ((folder.Name.Equals("_w", StringComparison.OrdinalIgnoreCase) || folder.Name.Equals("_t", StringComparison.OrdinalIgnoreCase))
                       && folder.ParentList.BaseTemplate == AveListTemplateType.ImagesLibrary))

                    {
                        version = 1;
                    }
                }
            }
            return version;
        }
        private bool IsPublishWeb(IAveWeb web)
        {
            try
            {
                var webTemplate = web.WebTemplate + "#" + web.Configuration;
                return webTemplate.StartsWith("CMSPUBLISHING", StringComparison.OrdinalIgnoreCase)
                    || webTemplate.Equals("SPS#0", StringComparison.OrdinalIgnoreCase)
                    || webTemplate.StartsWith("BLANKINTERNET", StringComparison.OrdinalIgnoreCase)
                    || webTemplate.Equals("SPSSITES#0", StringComparison.OrdinalIgnoreCase)
                    || webTemplate.Equals("SRCHCEN#0", StringComparison.OrdinalIgnoreCase)
                    || webTemplate.Equals("SPSREPORTCENTER#0", StringComparison.OrdinalIgnoreCase)
                    || webTemplate.StartsWith("ENTERWIKI", StringComparison.OrdinalIgnoreCase)
                    || webTemplate.Equals("SRCHCENTERFAST#0", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                log.Warn("Failed to determine whether this web is publish web. Web: {0} : {1}.", web.Url, e);
                return false;
            }
        }

        private AveItemObject GetAttachment(List<AveItemObject> attachments, Guid docId, IAveListItem item, string fullUrl)
        {
            foreach (AveItemObject attach in attachments)
            {
                if (attach.DocID == docId)
                {
                    return attach;
                }
            }

            AveItemObject attachmentObject = new AveItemObject
            {
                DocID = docId
            };
            attachmentObject.FullUrl = fullUrl;
            attachmentObject.DirName = fullUrl.Remove(fullUrl.LastIndexOf('/'));
            attachmentObject.SourceName = fullUrl.Substring(fullUrl.LastIndexOf('/')).TrimStart('/');
            attachmentObject.LeafName = attachmentObject.SourceName;
            attachmentObject.ItemName = attachmentObject.SourceName;
            //TODO
            //attachmentObject.Uiversion
            //attachmentObject.TimeLastModified
            //attachmentObject.Size
            //attachmentObject.DocFlags

            attachments.Add(attachmentObject);
            return attachmentObject;
        }

        private bool IsSystemFile(Guid docId, out IAveFile aveFile)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.IsSystemFile"))
            {

                aveFile = this.web.GetFile(docId);
                if (!aveFile.Exists)
                {
                    log.Warn("File is not exists, file id is {0}, parent web url is {1}.", docId, this.web.Url);
                    return true;//返回true，FillSystemFileProperty中会再次判断
                }
                if (aveFile.Item != null)
                {
                    return false;
                }
                return true;
            }
        }
        private void FillSystemFileProperty(AveItemObject folderObject, Dictionary<Guid, AveItemObject> systemItems, IAveList list, IAveFile aveFile, IAveChange changeItem, DateTime eventTime, NativeChangeType nativeChangeType, ChangeType changeType, ChangeObjectType objectType, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.FillSystemFileProperty"))
            {
                if (!aveFile.Exists)
                {
                    return;
                }
                AveItemObject parentFolder = null;
                string fullName = aveFile.ServerRelativeUrl.TrimStart('/');
                string itemName = aveFile.Name;
                string dirName = aveFile.ParentFolder.ServerRelativeUrl.TrimStart('/');
                if ((parentFolder = GetParentFolder(dirName, folderObject, noPropertyFolders)) == null)
                {
                    return;
                }
                AveItemObject itemOrFolder = GernerateSystemFile(list, systemItems, aveFile, fullName, itemName, dirName, parentFolder);
                AnalyseItemEvent(parentFolder, itemOrFolder, nativeChangeType, changeType, fullName, null);
                itemOrFolder.EventTime = eventTime;
                //把document与listItem的RoleAssignment删除记录load出来
                if (changeItem != null && nativeChangeType == NativeChangeType.AssignmentDelete)
                {
                    LoadDeleteAssignmentData(changeItem, itemOrFolder, eventTime);
                }
            }
        }

        private IAveFolder GetFolderByUniqutId(IAveList list, Guid docId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.FillSystemFolderProperty"))
            {
                var parentWeb = list.ParentWeb;

                IAveFolder aveFolder = parentWeb.GetFolder(docId);
                if (!aveFolder.Exists)
                {
                    //docId不一定是folder,可能是在IB job前被delete的item
                    log.Warn("This object is not exists, object id is {0}, parent web url is {1}.", docId, parentWeb.Url);
                    return null;
                }
                return aveFolder;

            }
        }
        private void FillSystemFolderProperty(IAveFolder aveFolder, AveItemObject folderObject, IAveChange changeItem, Guid docId, NativeChangeType nativeChangeType, ChangeType changeType, ChangeObjectType objectType, DateTime eventTime, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            AveItemObject parentFolder = null;
            string fullName = aveFolder.ServerRelativeUrl.TrimStart('/');
            string itemName = aveFolder.Name;
            //list下rootFolder的ParentFolder.ServerRelativeUrl前后都有'/'
            string dirName = aveFolder.ParentFolder.ServerRelativeUrl.Trim('/');
            if ((parentFolder = GetParentFolder(aveFolder.ParentFolder.ServerRelativeUrl.Trim('/'), folderObject, noPropertyFolders)) == null)
            {
                if (objectType == ChangeObjectType.Folder && changeType != ChangeType.None && docId.Equals(folderObject.DocID))
                {
                    folderObject.ChangeType = ChangeType.Edit;
                }
                return;
            }
            var itemOrFolder = GernerateSystemFolder(changeItem, eventTime, nativeChangeType, changeType, docId, aveFolder, fullName, itemName, dirName, parentFolder);

            //把document与listItem的RoleAssignment删除记录load出来
            if (nativeChangeType == NativeChangeType.AssignmentDelete)
            {
                if (!(changeItem.Rows[(int)DiscoverRowName.Int0] is DBNull))
                {
                    LoadDeleteAssignmentData(changeItem, itemOrFolder, eventTime);
                }
            }
        }
        private void FillProperty(AveItemObject folderObject, IAveList list, Dictionary<int, AveItemObject> items, IAveChange changeItem, DateTime eventTime, NativeChangeType nativeChangeType, ChangeType changeType, ChangeObjectType objectType, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.FillProperty"))
            {
                var docId = (Guid)changeItem.Rows[(int)DiscoverRowName.DocId];
                var ObjType = (ChangeObjectType)changeItem.Rows[(int)DiscoverRowName.ObjectType];
                if (changeItem.ChangeType == AveChangeType.Delete && (ObjType == ChangeObjectType.Item || ObjType == ChangeObjectType.File || ObjType == ChangeObjectType.Folder))
                {
                    AnalyzeDeleteEvent(folderObject, items, changeItem, eventTime, nativeChangeType, changeType, objectType, noPropertyFolders);
                    return;
                }
                int rowId;

                if (!changeItem.TryGetRowValue<int>(DiscoverRowName.ItemId, out rowId))
                {
                    log.Info("Row id is null, the object is system folder.");

                    var aveFolder = GetFolderByUniqutId(list, docId);
                    if (aveFolder != null)
                        FillSystemFolderProperty(aveFolder, folderObject, changeItem, docId, nativeChangeType, changeType, objectType, eventTime, noPropertyFolders);
                    return;
                }

                IAveListItem aveItem = null;
                string fullName = string.Empty;
                string itemName;
                string dirName;
                IAveSite checkoutSite = null;
                IAveWeb checkoutWeb = null;
                AveItemObject parentFolder = null;
                try
                {

                    try
                    {
                        AveListTemplateType listType;
                        aveItem = GetListItemByDocLibRowId(rowId, list, out checkoutSite, out checkoutWeb, out listType);
                    }
                    catch (ArgumentException)
                    {
                        IAveCheckedOutFile checkedOutFile;
                        var lib = list as IAveDocumentLibrary;
                        if (lib != null && TryGetCheckedOutFile(lib, changeItem, out checkedOutFile))
                        {
                            aveItem = GetCheckoutFileItem(checkedOutFile, out checkoutSite, out checkoutWeb);
                        }
                        else
                        {
                            var aveFolder = GetFolderByUniqutId(list, docId);
                            if (aveFolder != null)
                                FillSystemFolderProperty(aveFolder, folderObject, changeItem, docId, nativeChangeType, changeType, objectType, eventTime, noPropertyFolders);
                        }
                    }
                    catch (Exception e)
                    {
                        //TODO   暂时不处理找不到的item
                        log.Warn("Can not get item by guid. item row id: {0}, List Id: {1},  error message: {2}", rowId, list.ID, e);
                        var aveFolder = GetFolderByUniqutId(list, docId);
                        if (aveFolder != null)
                            FillSystemFolderProperty(aveFolder, folderObject, changeItem, docId, nativeChangeType, changeType, objectType, eventTime, noPropertyFolders);
                    }
                    if (aveItem != null)
                    {
                        fullName = list.RootFolder.ServerRelativeUrl.Substring(1, list.RootFolder.ServerRelativeUrl.Length - list.RootFolder.Url.Length - 1) + aveItem.Url;
                        //此处对于listItem，itemName必须得用fullName来截，不能用aveItem.Name
                        itemName = fullName.Substring(fullName.LastIndexOf('/') + 1);
                        //要注意rootSC存在fullName.equals(itemName)的情况
                        dirName = fullName.Substring(0, fullName.Length - itemName.Length).TrimEnd('/');
                        if ((parentFolder = GetParentFolder(dirName, folderObject, noPropertyFolders)) == null)
                        {
                            if (objectType == ChangeObjectType.Folder && changeType != ChangeType.None
                                && docId == folderObject.DocID)
                            {
                                folderObject.ChangeType = ChangeType.Edit;
                            }
                            return;
                        }
                        AveItemObject itemOrFolder;
                        string itemFullName = fullName;
                        if (aveItem.Folder != null)
                        {
                            itemOrFolder = GenerateFolder(items, docId, aveItem, fullName, itemName, dirName, parentFolder);
                            itemFullName = (dirName + "/" + fullName).Trim('/');
                        }
                        else //ListItem or Document
                        {
                            itemOrFolder = GernerateItem(list, items, aveItem, fullName, itemName, dirName, parentFolder);
                        }
                        AnalyseItemEvent(parentFolder, itemOrFolder, nativeChangeType, changeType, fullName, items);
                        itemOrFolder.EventTime = eventTime;
                        //把document与listItem的RoleAssignment删除记录load出来
                        if (nativeChangeType == NativeChangeType.AssignmentDelete)
                        {
                            LoadDeleteAssignmentData(changeItem, itemOrFolder, eventTime);
                        }
                    }
                }
                finally
                {
                    DisposeCheckoutSPObjct(checkoutSite, checkoutWeb);
                }
            }
        }

        private static bool TryGetCheckedOutFile(IAveDocumentLibrary lib, IAveChange changeItem, out IAveCheckedOutFile checkedOutFile)
        {
            checkedOutFile = null;
            checkedOutFile = GetCheckedOutFileById(lib, changeItem);
            if (checkedOutFile == null && changeItem.ChangeType != AveChangeType.Rename)//remame changeItem中为修改前的Url，因此不能使用这个URL作条件
            {
                checkedOutFile = GetCheckedOutFileByUrl(lib, changeItem);
            }
            return checkedOutFile != null;
        }

        private static IAveCheckedOutFile GetCheckedOutFileByUrl(IAveDocumentLibrary lib, IAveChange changeItem)
        {
            try
            {
                string url;
                if (changeItem.TryGetRowValue(DiscoverRowName.ItemFullUrl, out url))
                {
                    return lib.CheckedOutFiles.FirstOrDefault(f => string.Equals(f.Url, url, StringComparison.CurrentCultureIgnoreCase));
                }
            }
            catch (UnauthorizedAccessException e)
            {
                //当站点被置为Read Only状态时，因user没有CancelCheckout权限，调用lib.CheckedOutFiles API抛异常
                log.Warn("An error occurred while getting library's checkedOut files, current user may not have CancelCheckout permmission to library. Library title: {0}. Error:{1}", lib.Title, e);
            }
            return null;
        }

        private static IAveCheckedOutFile GetCheckedOutFileById(IAveDocumentLibrary lib, IAveChange changeItem)
        {
            try
            {
                int rowId;
                if (changeItem.TryGetRowValue(DiscoverRowName.ItemId, out rowId))
                {
                    return lib.CheckedOutFiles.FirstOrDefault(f => f.ListItemId == rowId);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                //当站点被置为Read Only状态时，因user没有CancelCheckout权限，调用lib.CheckedOutFiles API抛异常
                log.Warn("An error occurred while getting library's checkedOut files, current user may not have CancelCheckout permmission to library. Library title: {0}. Error:{1}", lib.Title, e);
            }
            return null;
        }

        private void AnalyzeDeleteEvent(AveItemObject folderObject, Dictionary<int, AveItemObject> items, IAveChange changeItem, DateTime eventTime, NativeChangeType nativeChangeType, ChangeType changeType, ChangeObjectType objectType, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.AnalyzeDeleteEvent"))
            {
                string fullName = string.Empty;
                AveItemObject parentFolder = null;
                int id;
                try
                {
                    if (changeItem.Rows[(int)DiscoverRowName.ItemId] is DBNull)
                    {
                        return;
                    }
                    id = Convert.ToInt32(changeItem.Rows[(int)DiscoverRowName.ItemId], CultureInfo.InvariantCulture);
                    if (id == 0)
                    {
                        return;
                    }
                    AveItemObject item = null;
                    if (items.TryGetValue(id, out item))
                    {
                        item.ChangeTypeBeforeDelete = item.ChangeType;
                        item.ChangeType = ChangeType.Delete;
                        return;
                    }
                    var docId = (Guid)changeItem.Rows[(int)DiscoverRowName.DocId];
                    var recycleBinItem = GetItemInRecycleBinForIB(this.web, changeItem);

                    if (recycleBinItem == null)
                    {
                        string fullUrl = changeItem.Rows[(int)DiscoverRowName.ItemFullUrl].ToString();
                        int index = fullUrl.LastIndexOf('/');
                        string dirName = index > 0 ? fullUrl.Substring(0, index) : String.Empty;
                        string leafName = fullUrl.Substring(index + 1);
                        if ((parentFolder = GetParentFolder(dirName, folderObject, noPropertyFolders)) != null)
                        {
                            item = new AveItemObject();
                            item.DocID = docId;
                            item.ID = id;
                            item.FullUrl = fullUrl;
                            item.LeafName = leafName;
                            item.SourceName = leafName;
                            item.ItemName = leafName;
                            item.EventTime = changeItem.Time;
                            item.TimeLastModified = Convert.ToDateTime(changeItem.Rows[(int)DiscoverRowName.TimeLastModified]);
                            item.ChangeTypeBeforeDelete = item.ChangeType;
                            item.ChangeType = ChangeType.Delete;
                            items.Add(id, item);
                            parentFolder.SubItemObjs.Add(item);
                        }
                    }
                    else
                    {
                        if ((parentFolder = GetParentFolder(recycleBinItem.DirName, folderObject, noPropertyFolders)) != null)
                        {
                            fullName = recycleBinItem.DirName.TrimStart('/') + "/" + recycleBinItem.LeafName;

                            if (recycleBinItem.ItemType == AveRecycleBinItemType.ListItem || recycleBinItem.ItemType == AveRecycleBinItemType.File)
                            {
                                item = new AveItemObject();
                                item.ID = id;
                                item.DocID = docId;
                                item.HasStream = true;
                                item.IsCurrentVersion = true;
                                item.DirName = recycleBinItem.DirName;
                                item.SourceName = recycleBinItem.LeafName;
                                item.ItemName = recycleBinItem.LeafName;
                                item.LeafName = recycleBinItem.LeafName;
                                item.ServerRelativeUrl = string.IsNullOrEmpty(recycleBinItem.DirName) ? recycleBinItem.LeafName : recycleBinItem.DirName + "/" + recycleBinItem.LeafName;
                                item.Type = (byte)0;
                                item.TimeLastModified = recycleBinItem.DeletedDate;
                                item.ModifyBy = recycleBinItem.DeletedBy.LoginName;
                                item.EventTime = changeItem.Time;
                                item.ChangeType = ChangeType.Delete;
                                item.FullUrl = fullName;
                                item.Uiversion = 512;//被删除，获取不到version值。赋默认值。
                                item.ObjType = recycleBinItem.ItemType == AveRecycleBinItemType.ListItem ? ItemType.Item : ItemType.Document;

                                AveVersionObject tempVersion = new AveVersionObject()
                                {
                                    Uiversion = 512,
                                    IsCurrentVersion = true,
                                    HasStream = true,
                                };
                                item.VersionObjs.Add(tempVersion);//将Current version添加到VersionObjs中。
                                items.Add(id, item);
                                parentFolder.SubItemObjs.Add(item);
                                AnalyseItemEvent(parentFolder, item, nativeChangeType, changeType, fullName, items);
                            }
                            else
                            {
                                AveItemObject folder = GetCurrentFolder(parentFolder, fullName);
                                folder.ID = id;
                                folder.DocID = docId;
                                folder.DirName = recycleBinItem.DirName;
                                folder.LeafName = recycleBinItem.LeafName;
                                folder.ServerRelativeUrl = string.IsNullOrEmpty(recycleBinItem.DirName) ? recycleBinItem.LeafName : recycleBinItem.DirName + "/" + recycleBinItem.LeafName;
                                folder.Type = (byte)1;
                                folder.TimeLastModified = recycleBinItem.DeletedDate;
                                folder.ModifyBy = recycleBinItem.DeletedBy.LoginName;
                                folder.EventTime = changeItem.Time;
                                folder.ChangeType = ChangeType.Delete;
                                folder.FullUrl = fullName;
                                items.Add(id, folder);
                                AnalyseFolderEvent(parentFolder, folder, nativeChangeType, changeType, fullName, items);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while analyzing an delete event. Url: {0}, Error: {1}", fullName, e);
                }
            }
        }

        /// <summary>
        /// 把document与listItem的RoleAssignment删除记录load出来
        /// </summary>
        /// <param name="changeItem"></param>
        /// <param name="itemOrFolder"></param>
        private void LoadDeleteAssignmentData(IAveChange changeItem, AveItemObject itemOrFolder, DateTime eventTime)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.LoadDeleteAssignmentData"))
            {
                if (!(changeItem.Rows[(int)DiscoverRowName.Int0] is DBNull))
                {
                    AveSecurityObject deleteRoleAssignment = new AveSecurityObject();
                    // 删除RoleAssignmet时，
                    // int0存放principalID,int1存放RoleID
                    deleteRoleAssignment.ObjectType = SecurityType.Assignment;
                    deleteRoleAssignment.PrincipleId = (int)changeItem.Rows[(int)DiscoverRowName.Int0];
                    if (!(changeItem.Rows[(int)DiscoverRowName.Int1] is DBNull))
                    {
                        deleteRoleAssignment.RoleId = (int)changeItem.Rows[(int)DiscoverRowName.Int1];
                    }
                    //如果int1为Null，说明把该user/group的权限全部移除了
                    else
                    {
                        deleteRoleAssignment.RoleId = -1;
                    }
                    deleteRoleAssignment.EventTime = eventTime;
                    itemOrFolder.DeleteRoleAssignments.Add(deleteRoleAssignment);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "file property")]
        private AveItemObject GernerateItem(IAveList list, Dictionary<int, AveItemObject> items, IAveListItem aveItem, string fullName, string itemName, string dirName, AveItemObject parentFolder)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GenerateItem"))
            {
                AveItemObject item = null;
                if (!items.ContainsKey(aveItem.ID))
                {
                    item = InitItemObjBasicProperty(aveItem, dirName, itemName, fullName);
                    //discoverReader.ReadItemContentForIB(item, sr);          
                    item.IsCurrentVersion = true;
                    //item.EventTime = eventTime;
                    if (FieldExist("Modified By"))
                    {
                        item.ModifyBy = aveItem["Modified By"].ToString();
                        var index = item.ModifyBy.IndexOf(";#", StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            item.ModifyBy = item.ModifyBy.Substring(index + 2);
                        }
                    }
                    if (aveItem.File != null)
                    {
                        item.Size = aveItem.File.Length;
                        item.HasStream = aveItem.File.HasStream();
                    }

                    //Add item versions
                    if (!QueryVersionByNative)
                    {
                        log.Debug("Query version with native method");
                        GenerateVersion(aveItem, item);
                    }
                    parentFolder.SubItemObjs.Add(item);
                    items.Add(aveItem.ID, item);
                }
                else
                {
                    item = items[aveItem.ID];
                }
                //AnalyseItemEvent(parentFolder, item, nativeChangeType, changeType, fullName, items);
                return item;
            }
        }

        private AveItemObject GernerateSystemFile(IAveList list, Dictionary<Guid, AveItemObject> systemItems, IAveFile aveFile, string fullName, string itemName, string dirName, AveItemObject parentFolder)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GenerateSystemFile"))
            {
                AveItemObject file = null;
                if (!systemItems.ContainsKey(aveFile.UniqueId))
                {
                    file = new AveItemObject();
                    //discoverReader.ReadItemContentForIB(item, sr);
                    if (list.BaseType == AveBaseType.DocumentLibrary)
                    {
                        file.ObjType = ItemType.Document;
                        file.SourceName = itemName;
                    }
                    else
                    {
                        file.ObjType = ItemType.Item;
                    }
                    file.DocID = aveFile.UniqueId;
                    file.DirName = dirName;
                    file.FullUrl = fullName;
                    file.IsCurrentVersion = true;
                    file.SourceName = file.LeafName = file.ItemName = itemName;
                    //file.EventTime = eventTime;
                    if (aveFile.Properties.ContainsKey("vti_timelastmodified"))
                    {
                        file.TimeLastModified = Convert.ToDateTime(aveFile.Properties["vti_timelastmodified"].ToString());
                    }
                    if (aveFile.Properties.ContainsKey("vti_modifiedby"))
                    {
                        file.ModifyBy = aveFile.Properties["vti_modifiedby"].ToString();
                    }
                    file.Uiversion = aveFile.UIVersion;
                    file.Level = (byte)aveFile.Level;
                    file.HasStream = aveFile.HasStream();
                    //ToDo
                    //item.Type            
                    //item.CheckoutUserId

                    parentFolder.SubItemObjs.Add(file);
                    systemItems.Add(aveFile.UniqueId, file);
                }
                else
                {
                    file = systemItems[aveFile.UniqueId];
                }
               
                return file;
            }
        }

        private AveItemObject GernerateSystemFolder(IAveChange item, DateTime eventTime, NativeChangeType nativeChangeType, ChangeType changeType, Guid docId, IAveFolder aveFolder, string fullName, string itemName, string dirName, AveItemObject parentFolder)
        {
            AveItemObject folder = GetCurrentFolder(parentFolder, fullName);
            //if (!folder.PropertyAdded)
            //{
            folder.DocID = docId;
            folder.IsCurrentVersion = true;
            folder.FullUrl = fullName;
            folder.ItemName = itemName;
            folder.SourceName = itemName;
            folder.LeafName = fullName.Substring(dirName.Length + 1);
            folder.DirName = dirName;
            //folder.PropertyAdded = true;
            folder.ObjType = ItemType.Folder;
            folder.Uiversion = GetFolderUIVersion(aveFolder);
            if (aveFolder.Properties.ContainsKey("vti_level"))
            {
                folder.Level = byte.Parse(aveFolder.Properties["vti_level"].ToString());
            }
            //}
            folder.EventTime = eventTime;
            //if (!(sr["ModifiedBy"] is DBNull))
            //{
            //    folder.ModifyBy = (string)sr["ModifiedBy"];
            //}
            //把folder的RoleAssignment删除记录load出来
            AnalyseFolderEvent(parentFolder, folder, nativeChangeType, changeType, (dirName + "/" + itemName).Trim('/'), null);
            return folder;
        }

        private AveItemObject GenerateFolder(Dictionary<int, AveItemObject> items, Guid docId, IAveListItem aveItem, string fullName, string itemName, string dirName, AveItemObject parentFolder)
        {
            AveItemObject folder = null;
            if (!items.ContainsKey(aveItem.ID))
            {
                folder = GetCurrentFolder(parentFolder, fullName);
                //if (!folder.PropertyAdded)
                //{
                folder.DocID = docId;
                folder.ID = aveItem.ID;
                folder.IsCurrentVersion = true;
                folder.FullUrl = fullName;
                folder.ItemName = itemName;
                folder.SourceName = itemName;
                folder.LeafName = fullName.Substring(dirName.Length + 1);
                folder.DirName = dirName;
                //folder.PropertyAdded = true;
                folder.ObjType = ItemType.Folder;
                folder.Level = (byte)aveItem.Level;
                folder.tp_GUID = FieldExist("GUID") ? new Guid((string)aveItem["GUID"]) : default(Guid);
                if (FieldExist("Modified"))
                {
                    if (aveItem.ParentList.ParentWeb.RegionalSettings != null)
                    {
                        folder.TimeLastModified = aveItem.ParentList.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveItem["Modified"]);
                    }
                    else
                    {
                        folder.TimeLastModified = ((DateTime)aveItem["Modified"]).ToUniversalTime();
                    }
                }
                if (FieldExist("_UIVersion"))
                {
                    folder.Uiversion = (int)aveItem["_UIVersion"];
                }
                //folder.EventTime = eventTime;
                //}
                //if (!(sr["ModifiedBy"] is DBNull))
                //{
                //    folder.ModifyBy = (string)sr["ModifiedBy"];
                //}          
                items.Add(aveItem.ID, folder);
            }
            else
            {
                folder = items[aveItem.ID];
            }
            //把folder的RoleAssignment删除记录load出来
            //AnalyseFolderEvent(parentFolder, folder, nativeChangeType, changeType, (dirName + "/" + itemName).Trim('/'), items);
            return folder;
        }

        private AveItemObject GetParentFolder(string dirName, AveItemObject rootFolder, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            string listRootFolderUrl = rootFolder.FullUrl;

            if (dirName.TrimEnd('/').Equals(listRootFolderUrl))
            {
                return rootFolder;
            }
            if (!dirName.Contains(listRootFolderUrl))
            {
                return null;
            }
            string foldersDirName = dirName.Substring(listRootFolderUrl.Length).Trim('/');

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
                    noPropertyFolders.Add(tempFolder.FullUrl, tempFolder);
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
                if (afc.FullUrl.Equals(fullUrl, StringComparison.OrdinalIgnoreCase))
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

        private void AnalyseFolderEvent(AveItemObject parentFolder, AveItemObject folder, NativeChangeType nativeChageType, ChangeType changeType, string sourceFullUrl, Dictionary<int, AveItemObject> items)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.AnalyseFolderEvent"))
            {
                if (nativeChageType.HasFlag(NativeChangeType.Rename))
                {
                    //当用Sharepoint Designer在同一个list下去move一个folder的时候，触发的事件即为rename，在这里标记为true。
                    folder.isRename = true;//For replicator
                    foreach (AveItemObject afc in parentFolder.SubFolderObjs)
                    {
                        if (afc.FullUrl.Equals(sourceFullUrl, StringComparison.OrdinalIgnoreCase) && !folder.FullUrl.EndsWith(afc.ItemName, StringComparison.OrdinalIgnoreCase))
                        {
                            afc.ChangeType = ChangeType.Edit; //we regard rename as edit
                            afc.FullUrl = folder.FullUrl;
                            afc.ItemName = folder.ItemName;
                            afc.LeafName = folder.LeafName;
                            afc.DirName = folder.DirName;
                            afc.ModifyBy = folder.ModifyBy;
                            afc.EventTime = folder.EventTime;
                            parentFolder.SubFolderObjs.Remove(folder);
                            afc.ObjType = ItemType.Folder;
                            return;
                        }
                    }
                    return;
                }
                //当用Sharepoint designer去跨list move一个folder的时候，触发的事件为move into,也让其走rename逻辑。
                if (nativeChageType == NativeChangeType.MoveInto)
                {
                    folder.isRename = true;
                }
                if (nativeChageType == NativeChangeType.AssignmentAdd || nativeChageType == NativeChangeType.AssignmentDelete || nativeChageType == (NativeChangeType.RoleAdd | NativeChangeType.AssignmentAdd))
                {
                    folder.ItemPermissionChanged = true;
                    folder.RoleAssignmentsChangeType = ChangeType.Edit;
                    return;
                }
                else
                {
                    folder.ItemPermissionChanged = false;
                }
                if (folder.ChangeType == ChangeType.Add || folder.ChangeType == ChangeType.Restore)
                {
                    if (changeType == ChangeType.Delete)
                    {
                        folder.ChangeTypeBeforeDelete = folder.ChangeType;
                        folder.ChangeType = ChangeType.Delete;
                    }
                }
                else
                {
                    if (folder.ChangeType == ChangeType.Delete && changeType == ChangeType.Restore)
                    {
                        folder.ChangeType = folder.ChangeTypeBeforeDelete;
                        if (folder.ChangeType == ChangeType.None)
                        {
                            parentFolder.SubFolderObjs.Remove(folder);
                            if (items != null && folder.ID.HasValue)
                            {
                                items.Remove(folder.ID.Value);
                            }
                        }
                    }
                    else
                    {
                        if (changeType == ChangeType.Delete)
                        {
                            folder.ChangeTypeBeforeDelete = folder.ChangeType;
                        }
                        folder.ChangeType = changeType;
                    }
                }
            }
        }

        private void AnalyseItemEvent(AveItemObject parentFolder, AveItemObject item, NativeChangeType nativeChageType, ChangeType changeType, string fullName, Dictionary<int, AveItemObject> items)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.AnalyseItemEvent"))
            {
                //当用Sharepoint designer在同一个list下move一个document的时候，触发的事件为rename。
                //当用Sharepoint designer去跨list move一个document的时候，触发的事件为move into,也让其走rename逻辑。
                if (nativeChageType.HasFlag(NativeChangeType.Rename) || nativeChageType == NativeChangeType.MoveInto)
                {
                    item.isRename = true;
                    item.ChangeType = ChangeType.Edit; //we regard rename as edit
                    item.ItemName = fullName.Substring(fullName.LastIndexOf('/') + 1);
                    item.FullUrl = fullName;
                    return;
                }
                if (nativeChageType == NativeChangeType.AssignmentAdd || nativeChageType == NativeChangeType.AssignmentDelete || nativeChageType == (NativeChangeType.RoleAdd | NativeChangeType.AssignmentAdd))
                {
                    item.RoleAssignmentsChangeType = ChangeType.Edit;
                    item.ItemPermissionChanged = true;
                    return;
                }
                //当先checkout,之后change permission,然后discard checkout时，下面的else代码将itempermissionchanged属性给覆盖了，所以将其注释
                //else
                //{
                //    item.ItemPermissionChanged = false;
                //}
                if (item.ChangeType == ChangeType.Add || item.ChangeType == ChangeType.Restore)
                {
                    if (changeType == ChangeType.Delete)
                    {
                        item.ChangeTypeBeforeDelete = item.ChangeType;
                        item.ChangeType = ChangeType.Delete;
                    }
                }
                else
                {
                    if (item.ChangeType == ChangeType.Delete && changeType == ChangeType.Restore)
                    {
                        item.ChangeType = item.ChangeTypeBeforeDelete;
                        if (item.ChangeType == ChangeType.None)
                        {
                            parentFolder.SubItemObjs.Remove(item);
                            if (items != null && item.ID.HasValue)
                            {
                                items.Remove(item.ID.Value);
                            }
                        }
                    }
                    else
                    {
                        if (changeType == ChangeType.Delete)
                        {
                            item.ChangeTypeBeforeDelete = item.ChangeType;
                        }
                        item.ChangeType = changeType;
                    }
                }
            }
        }

        #endregion

        #region Item Level

        public Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid siteId, Guid webId, Guid listId, int itemId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryItemSecurityForIB"))
            {
                SetWebIfChanged(webId);
                var list = this.web.GetList(listId);
                var ibQuery = GetQueryForIB(false, false, AveCollectionScope.List, listId);
                ibQuery.Item = true;
                ibQuery.RoleAssignmentAdd = true;
                ibQuery.RoleAssignmentDelete = true;

                var securityChanges = new Dictionary<int, List<AveSecurityObject>>();
                var items = list.GetChanges(ibQuery);
                while (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var nativeChangeType = (NativeChangeType)item.Rows[(int)DiscoverRowName.EventType];
                        SecurityType securityType = DiscoverUtility.GetSecurityObjectType(nativeChangeType);
                        ChangeType changeType = DiscoverUtility.GetSecurityChangeType(nativeChangeType);
                        switch (securityType)
                        {
                            case SecurityType.Assignment:
                                try
                                {
                                    AssignmentSecurityChange(changeType, item, securityChanges);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", item.Rows[(int)DiscoverRowName.EventTime].ToString(), e.ToString());
                                }
                                break;
                            case SecurityType.Scope: //break inherate
                                try
                                {
                                    ScopeSecurityChange(changeType, item, securityChanges);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Scope.EventTime:{0}.  ErrorMessage:{1}", item.Rows[(int)DiscoverRowName.EventTime].ToString(), e.ToString());
                                }
                                break;
                            case SecurityType.None:
                            default:
                                break;
                        }
                    }

                    ibQuery.ChangeTokenStart = items.LastChangeToken;
                    items = list.GetChanges(ibQuery);
                }
                return securityChanges;
            }
        }
        public void QueryAttachmentByItemObj(Guid siteId, string listRootFolderUrl, AveItemObject itemObj, IAveWeb web, Guid listId)
        {
            this.QueryAttachmentByItemObj(web, listId, itemObj);
        }

        #endregion

        #region FB

        private Dictionary<Guid, AveWebObject> GetWebsInCollection(IAveWebCollection webs, string rootWebUrl)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetWebsInCollection"))
            {
                var webObjs = new Dictionary<Guid, AveWebObject>();
                foreach (var web in webs)
                {
                    AveWebObject tempWeb = new AveWebObject();
                    try
                    {
                        tempWeb.WebID = web.ID;
                        tempWeb.Title = web.Title;
                        tempWeb.FullUrl = web.ServerRelativeUrl.TrimStart('/');
                        if (web.IsRootWeb)
                        {
                            tempWeb.Name = ".";
                        }
                        else
                        {
                            //注意考虑root sitecollection的情况，与sql保持一致
                            tempWeb.Name = tempWeb.FullUrl.Substring(rootWebUrl.Length).TrimStart('/');
                        }
                        tempWeb.IsAppWeb = web.IsAppWeb;
                        tempWeb.AppInstanceId = web.AppInstanceId;
                        webObjs.Add(tempWeb.WebID, tempWeb);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while discovering webs.WebURL:{0},webID:{1},Error:{2}", tempWeb.FullUrl, tempWeb.WebID, e.ToString());
                    }
                    finally
                    {
                        web.Dispose();
                    }
                }
                return webObjs;
            }
        }

        public Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId)
        {
            return GetWebsInCollection(site.AllWebs, site.RootWeb.ServerRelativeUrl.TrimStart('/'));
        }

        public AveWebObject QueryRootWeb(Guid siteId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryRootWeb"))
            {
                try
                {
                    using (var web = site.OpenWeb())
                    {

                        return new AveWebObject()
                        {
                            WebID = web.ID,
                            Name = ".",
                            FullUrl = web.ServerRelativeUrl.TrimStart('/'),
                            Title = web.Title
                        };

                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while discovering webs.Error:{0}",  e);
                    return null;
                }
            }
        }

        public Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin)
        {
            var rootWeb = QueryRootWeb(siteId);
            SetWebIfChanged(parentWebId);
            var webs = GetWebsInCollection(this.web.Webs, rootWeb.FullUrl);
            if (includeRecycleBin)
            {
                GetWebsInRecycleBin(webs);
            }
            return webs;
        }

        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryWebListForFB1"))
            {
                SetWebIfChanged(webId);
                var listObjs = new Dictionary<Guid, AveListObject>();
                foreach (var list in this.web.Lists)
                {
                    if(listObjs.ContainsKey(list.ID))
                    {
                        continue;
                    }
                    AveListObject listObj = new AveListObject
                    {
                        ListId = list.ID,
                        RootFolderId = list.RootFolder.UniqueId,
                        Name = list.Title,
                        Title = list.Title,
                        Type = (int)list.BaseType,
                        RootFolderUrl = list.RootFolder.ServerRelativeUrl.Trim('/'),
                        Flag = long.Parse(list.Flags.ToString()),
                        ServerTemplate = (int)list.BaseTemplate,
                        Hidden = list.Hidden
                    };
                    listObjs.Add(listObj.ListId, listObj);
                }
                return listObjs;
            }
        }

        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool includeRecycleBin)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryWebListForFB2"))
            {
                SetWebIfChanged(webId);
                var listObjs = new Dictionary<Guid, AveListObject>();
                foreach (var list in this.web.Lists)
                {
                    if (listObjs.ContainsKey(list.ID))
                    {
                        continue;
                    }
                    AveListObject listObj = new AveListObject
                    {
                        ListId = list.ID,
                        RootFolderId = list.RootFolder.UniqueId,
                        Name = list.Title,
                        Title = list.Title,
                        Type = (int)list.BaseType,
                        RootFolderUrl = list.RootFolder.ServerRelativeUrl.Trim('/'),
                        Flag = long.Parse(list.Flags.ToString()),
                        ServerTemplate = (int)list.BaseTemplate,
                        Hidden = list.Hidden
                    };
                    listObjs.Add(listObj.ListId, listObj);
                }
                if (includeRecycleBin)
                {
                    GetListsInRecycleBin(listObjs);
                }
                return listObjs;
            }
        }

        public Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListViewForFB"))
            {
                SetWebIfChanged(webId);
                var views = new Dictionary<Guid, AveViewObject>();
                var list = this.web.GetList(listId);
                foreach (var view in list.Views)
                {
                    try
                    {
                        byte bt;
                        byte.TryParse(view.BaseViewId, out bt);
                        AveViewObject tempView = new AveViewObject()
                        {
                            ViewID = view.ID,
                            ViewType = (int)view.Flag,
                            IsPersonalView = view.PersonalView,
                            BaseViewId = bt,
                            ViewTitle = view.Title,
                            PageUrlID = this.web.GetFile(view.ServerRelativeUrl).UniqueId
                        };
                        //TODO   userId
                        views.Add(view.ID, tempView);
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while discovering the list view. listId: {0}, viewTitle: {1}, exception message: {2}", listId, view.Title, e);
                    }
                }
                return views;
            }
        }

        //List
        private void QuerySubFoldersForFB(AveFolderCache folderCache, AveListObject parentListObject, AveItemObject itemObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QuerySubFoldersForFB"))
            {
                SetWebIfChanged(folderCache.WebId);
                var parentFolder = this.web.GetFolder(itemObject.DocID);
                foreach (var folder in parentFolder.SubFolders)
                {
                    //skip the list root folders and no-use folders.
                    var item = GetFolderListItem(folder);
                    if (parentFolder.ParentListId == Guid.Empty && folder.ParentListId != Guid.Empty ||
                        item == null && string.Compare(folder.Name, "Attachments", StringComparison.OrdinalIgnoreCase) == 0 ||
                        parentFolder.ParentListId == Guid.Empty && mDiscoverReader.IsUnusedFolder(folder.Name, true))
                    {
                        continue;
                    }
                    var tempFolder = new AveItemObject()
                    {
                        DocID = folder.UniqueId,
                        SourceName = folder.Name,
                        LeafName = folder.Name,
                        ItemName = folder.Name,
                        Level = 1, //暂时先赋值，下面会修改此属性
                        Type = 1,
                        IsCurrentVersion = true,
                        DirName = itemObject.FullUrl,
                        FullUrl = folder.ServerRelativeUrl.Trim('/'),
                        ObjType = ItemType.Folder,
                    };
                    if (folder.Properties.ContainsKey("vti_level"))
                    {
                        tempFolder.Level = Byte.Parse(folder.Properties["vti_level"].ToString());
                    }
                    if (folder.Properties.ContainsKey("vti_timelastmodified"))
                    {
                        tempFolder.TimeLastModified = (DateTime)folder.Properties["vti_timelastmodified"];
                    }
                    if (item != null)
                    {
                        tempFolder.ID = item.ID;
                        tempFolder.Hidden = false;
                        if (item.Fields.ContainsField("_UIVersion"))
                        {
                            tempFolder.Uiversion = (int)item["_UIVersion"];
                        }
                        if (!QueryVersionByNative)
                        {
                            GenerateVersion(item, tempFolder);
                        }
                    }
                    else
                    {
                        tempFolder.Uiversion = GetFolderUIVersion(folder);
                        tempFolder.Hidden = true;
                    }
                    itemObject.SubFolderObjs.Add(tempFolder);

                }
                if (QueryVersionByNative)
                {
                    queryService.QueryItemVersionsForAPIFB(folderCache.SiteId, itemObject.DocID, itemObject.SubFolderObjs, parentListObject, mDiscoverReader);
                }
            }
        }

        private IAveListItem GetFolderListItem(IAveFolder folder)
        {
            if (folder.ParentList != null)  // null  is  web folder 系统folder
            {
                if (SpecialListTemplates.Contains(folder.ParentList.BaseTemplate))
                {
                    return null;
                }
            }
            return folder.Item;
        }

        private IAveUser GetCheckOutUser(IAveListItem item)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetCheckedOutUser"))
            {
                if (this.factory.ContextKind == AveContextKind.ClientObjectModel
                    || item.File == null
                    || item.File.CheckOutType == AveCheckOutType.None)
                {
                    return null;
                }
                return item.File.CheckedOutByUser;
            }
        }

        private void GenerateVersion(IAveListItem item, AveItemObject tempItem)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GenerateVersion"))
            {
                if (CheckVersions(tempItem))
                {
                    //tempItem的Uiversion,ModifyTime属性都移到该方法前通过item的Fields来获取，
                    //暂时不知道Fields中什么时候不包含Uiversion,ModifyTime属性。如遇不包含情况，
                    //再在此处通过最新version来获取。
                    int versionCount = 0;
                    using (var versionScope = new AvePerformanceScope("SP.SPListItem.Versions.Count"))
                    {
                        versionCount = item.Versions.Count;
                    }
                    foreach (var version in item.Versions)
                    {
                        AveVersionObject tempVersion = new AveVersionObject()
                        {
                            UserDataGuid = tempItem.tp_GUID,
                            Uiversion = version.VersionId,
                            Level = (byte)version.Level,
                            IsCurrentVersion = version.VersionId == tempItem.Uiversion,
                            HasStream = tempItem.HasStream,
                            TimeLastModified = version.Created   //暂时用此属性代替，version 的modify by 应该就是此属性
                        };
                        if (item.File != null)
                        {
                            IAveFileVersion fileVersion = null;
                            using (var vs = new AvePerformanceScope("SP.VersionCollection.GetVersionFromID"))
                            {
                                fileVersion = item.File.Versions.GetVersionFromID(version.VersionId);
                            }
                            if (fileVersion != null)
                            {
                                tempVersion.Size = fileVersion.Size;
                            }
                            else
                            {
                                tempVersion.Size = tempItem.Size;
                            }
                        }
                        tempItem.VersionObjs.Add(tempVersion);
                    }
                }
                else //出于效率方面考虑，只有一个version的时候没有必要遍历versions，减少API的调用
                {
                    AddCurrentVersionObj(tempItem);
                }
                //从数据库中获取version的StubInfo属性
                queryService.SetVersionsStubInfo(tempItem.VersionObjs, item.Web.Site.ID, item.UniqueId, mDiscoverReader);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ls is a variable")]
        public void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin, bool includeSystemFolder, bool includeVersion)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryListItemForFB"))
            {
                try
                {
                    QuerySubFoldersForFB(folderCache, parentListObject, folderObject);
                    var parentList = folderCache.ListId == Guid.Empty ? null : this.web.GetList(folderCache.ListId);
                    var parentFolder = this.web.GetFolder(folderObject.DocID);
                    if (folderCache.ListId == Guid.Empty)
                    {
                        foreach (var file in parentFolder.Files)
                        {
                            AveItemObject tempItem = new AveItemObject()
                            {
                                DocID = file.UniqueId,
                                DirName = file.ServerRelativeUrl.Replace(file.Name, "").Trim('/'),
                                SourceName = file.Name,
                                LeafName = file.Name,
                                ItemName = file.Name,
                                FullUrl = file.ServerRelativeUrl.Trim('/'),
                                Uiversion = file.UIVersion,
                                TimeLastModified = file.TimeLastModified,
                                Size = file.Length,
                                ObjType = ItemType.Document,
                                Type = 0,
                                HasStream = file.HasStream(),
                                Level = (byte)file.Level,
                            };
#if DEBUG
                            log.Debug("Discover system file, url: {0}, has stream: {1}, level:{2}", tempItem.FullUrl, tempItem.HasStream, tempItem.Level);
#endif


                            AddCurrentVersionObj(tempItem);
                            folderObject.SubItemObjs.Add(tempItem);
                        }
                        if (includeRecycleBin)
                        {
                            GetItemsInRecycleBin(parentFolder, folderObject);
                        }
                        return;
                    }

                    if (!SpecialListTemplates.Contains(parentList.BaseTemplate))
                    {

                        FBQuery.Query =
                             @"
<Query>
    <OrderBy>
        <FieldRef Name='ID'/>
    </OrderBy>
    <Where>
        <Eq>
            <FieldRef Name='FSObjType'/>
            <Value Type='Lookup'>0</Value>
        </Eq>
    </Where>
</Query>
";
                        FBQuery.Folder = parentFolder;
                        EnsureListInfo(parentList);
                        foreach (var item in parentList.GetItems(FBQuery))
                        {
                            if (item.FileSystemObjectType != AveFileSystemObjectType.Folder)
                            {
                                try
                                {
                                    AveItemObject tempItem = GetItemObject(item, folderObject);

                                    #region get versions
                                    if (includeVersion && !QueryVersionByNative)
                                    {
                                        GenerateVersion(item, tempItem);
                                    }
                                    #endregion

                                    #region get attachments
                                    try
                                    {
                                        if (item != null && item.File == null)
                                        {
                                            if (parentList.EnableAttachments && FieldExist("Attachments") && (bool)item["Attachments"])  //出于效率考虑，Attachments column为true时才遍历
                                            {
                                                using (var attach = new AvePerformanceScope("AveDiscoverQueryForAPI.GetAttachments"))
                                                {
                                                    foreach (IAveAttachment attachment in item.Attachments)
                                                    {
                                                        AveItemObject attachmentObj = InitAttachment(attachment, tempItem, parentList);
                                                        tempItem.AttachmentObjs.Add(attachmentObj);
                                                    }
                                                }
                                            }
                                            queryService.SetAttachmentsStubInfo(tempItem.AttachmentObjs, web.Site.ID, mDiscoverReader);

                                            if (includeRecycleBin && parentList.EnableAttachments)
                                            {
                                                GetAttachmentsInRecycleBin(parentList, tempItem);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Debug("An error occurred while get attachment. Item ID:{0}, parent list url:{1}, error message:{2}", item.ID, parentList.DefaultDisplayFormUrl, e);
                                    }
                                    #endregion
                                    folderObject.SubItemObjs.Add(tempItem);
                                }
                                catch (Exception e)
                                {
                                    var fullUrl = GetItemFullUrl(item, folderObject);
                                    log.Error("An error occurred while discover this item. Item: {0}, Error: {1}", fullUrl, e);
                                }
                            }
                        }
                        if (includeVersion && QueryVersionByNative)
                        {
                            queryService.QueryItemVersionsForAPIFB(folderCache.SiteId, folderObject.DocID, folderObject.SubItemObjs, parentListObject, mDiscoverReader);
                        }
                    }
                    if (parentList.BaseType == AveBaseType.DocumentLibrary)
                    {
                        AddCheckOutFile(parentFolder, parentList, web, folderObject);
                    }
                    if (parentList.BaseType == AveBaseType.Survey)
                    {
                        AddCheckOutListItem(folderCache, folderObject, parentListObject);
                    }

                    foreach (var item in parentFolder.HiddenFiles)
                    {
                        AveItemObject itemObject = InitHiddenFiles(item, folderObject);
                        AddCurrentVersionObj(itemObject);
                        folderObject.SubItemObjs.Add(itemObject);
                    }
                    if (includeRecycleBin)
                    {
                        GetItemsInRecycleBin(parentFolder, folderObject);
                    }
                    var allItems = new List<AveItemObject>(folderObject.SubItemObjs);
                    if (mDiscoverReader.NeedGetItemStubInfo())
                    {
                        //ADO-149781:只获取item的stub信息。
                        //API GetItems暂不支持includeRecycleBin,待以后想办法支持。先在DB查询item的stub信息方法中添加includeRecycleBin option。
                        queryService.SetItemStubInfo(allItems, folderCache.SiteId, includeRecycleBin);
                    }
                }
                finally
                {
                    DisposeCache();
                }
            }
        }

        private void AddCheckOutListItem(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObj)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.AddCheckOutListItem"))
            {
                var checkoutItem = queryService.GetCheckoutListItems(folderCache, listObj);
                if (checkoutItem.Count > 0)
                {
                    foreach (var itemInfo in checkoutItem)
                    {
                        if (itemInfo.ContainsKey("ItemId") && itemInfo.ContainsKey("RowId") && itemInfo.ContainsKey("UserId"))
                        {
                            try
                            {
                                var itemId = (Guid)itemInfo["ItemId"];
                                var itemRowId = (Int32)itemInfo["RowId"];
                                var userId = (Int32)itemInfo["UserId"];
                                if (userId == folderCache.AveSite.RootWeb.CurrentUser.ID)
                                {
                                    continue;
                                }
                                var user = folderCache.AveWeb.SiteUsers.GetByID(userId);
                                using (var web = folderCache.AveSite.GetCheckoutWeb(folderCache.SiteId, folderCache.AveWeb, null, user, itemId, false))
                                {
                                    var item = web.Lists.GetById(folderCache.ListId).GetItemById(itemRowId);
                                    var itemObject = InitItem(item, folderObject);
                                    itemObject.CheckoutUserId = userId;
                                    folderObject.SubItemObjs.Add(itemObject);
                                }
                            }

                            catch (Exception e)
                            {
                                log.Error("An error occurred while adding the check out ListItem. Error message: {0}", e);
                            }
                        }
                    }
                }
            }
        }
        private void AddCheckOutFile(IAveFolder parentFolder, IAveList parentList, IAveWeb parentWeb, AveItemObject folderObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.AddCheckoutFile"))
            {
                var documentLibrary = parentList as IAveDocumentLibrary;
                if (documentLibrary != null)
                {
                    var parentFolderUrl = parentFolder.ServerRelativeUrl.Trim(new char[] { '/' });
                    try
                    {
                        foreach (var checkedOutFile in documentLibrary.CheckedOutFiles)
                        {
                            IAveSite checkoutSite = null;
                            IAveWeb checkoutWeb = null;
                            IAveListItem item = null;
                            try
                            {
                                //在最小权限user的情况下，user的Sid为空,固改成用ID进行比较
                                if (parentFolderUrl.Equals(checkedOutFile.DirName, StringComparison.OrdinalIgnoreCase) && checkedOutFile.CheckedOutBy.ID != parentWeb.CurrentUser.ID)
                                {
                                    item = GetCheckoutFileItem(checkedOutFile, out checkoutSite, out checkoutWeb);
                                    var itemObject = InitItem(item, folderObject);
                                    folderObject.SubItemObjs.Add(itemObject);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error("An error occurred while adding the check out file. File: {0}. Error message: {1}", checkedOutFile.Url, e);
                            }
                            finally
                            {
                                DisposeCheckoutSPObjct(checkoutSite, checkoutWeb);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        //当站点被置为Read Only状态时，因user没有CancelCheckout权限，调用lib.CheckedOutFiles API抛异常
                        log.Warn("An error occurred while getting library's checkedOut files, current user may not have CancelCheckout permmission to ibrary. Library title: {0}. Error:{1}", documentLibrary.Title, e);
                    }
                }
            }
        }

        private AveItemObject GetItemObject(IAveListItem item, AveItemObject folderObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetItemObject"))
            {
                IAveSite checkoutSite = null;
                IAveWeb checkoutWeb = null;
                try
                {
                    var checkoutItem = GetListItemCheckOutVersion(item, out checkoutSite, out checkoutWeb);
                    if (checkoutItem != null)
                    {
                        return InitItem(checkoutItem, folderObject);
                    }
                }
                catch (Exception e)//如果discover checked out version失败，那么这个document是否算discover失败？
                {
                    var fullUrl = GetItemFullUrl(item, folderObject);
                    log.Error("An error occurred while getting the checked out version. Item: {0}. Error: {1}", fullUrl, e);
                }
                finally
                {
                    DisposeCheckoutSPObjct(checkoutSite, checkoutWeb);
                }
                return InitItem(item, folderObject);
            }
        }

        private string GetItemFullUrl(IAveListItem item, AveItemObject folderObject)
        {
            var leafName = item.Url.Substring(item.Url.LastIndexOf('/') + 1);
            return string.Format("{0}/{1}", folderObject.FullUrl, leafName).Trim('/');
        }


        private AveItemObject InitAttachment(IAveAttachment attachment, AveItemObject tempItem, IAveList parentList)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitAttachment"))
            {
                var name = attachment.FileName;
                var dirName = parentList.RootFolder.ServerRelativeUrl.Trim('/') + '/' + "Attachments/" + tempItem.ID.Value.ToString();
                AveItemObject tempAttachment = new AveItemObject()
                {
                    DocID = attachment.ROWID,
                    SourceName = name,
                    LeafName = name,
                    ItemName = name,
                    DirName = dirName,
                    FullUrl = string.Format("{0}/{1}", dirName, name).Trim('/'),
                };

                //TODO  
                //tempAttachment.TimeLastModified          
                //tempAttachment.Uiversion 
                return tempAttachment;
            }
        }

        private AveItemObject InitHiddenFiles(AveHiddenFileInfo item, AveItemObject folderObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitHiddenFiles"))
            {
                AveItemObject tempItem = new AveItemObject()
                {
                    DocID = new Guid(item.ID),
                    SourceName = item.Name,
                    LeafName = item.Name,
                    ItemName = item.Name,
                    Type = 0,
                    Level = (byte)item.Level,
                    DirName = folderObject.FullUrl,
                    FullUrl = string.Format("{0}/{1}", folderObject.FullUrl, item.Name).Trim('/'),
                    ObjType = ItemType.Document,
                    HasStream = item.HasStream,
                    Size = item.Size,
                };
                tempItem.TimeLastModified = item.TimeLastModified;
                tempItem.Uiversion = item.Version;
                return tempItem;
            }
        }

        private AveItemObject InitItem(IAveListItem item, AveItemObject folderObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitItem"))
            {
                var leafName = item.Url.Substring(item.Url.LastIndexOf('/') + 1);
                var fullUrl = string.Format("{0}/{1}", folderObject.FullUrl, leafName).Trim('/');
                var tempItem = InitItemObjBasicProperty(item, folderObject.FullUrl, leafName, fullUrl);

                if (item.File != null)
                {
                    tempItem.Size = item.File.Length;
                    tempItem.HasStream = item.File.HasStream();
                }
                //GenerateVersion(item, tempItem, parentList.ParentWeb);//此处注意注销掉既可防止GetItems API直接返回带Version的结果。修改后需要直接调用GenerateVersion方法，需要观察item的参数运用情况（防止空指针）。
                return tempItem;
            }
        }

        public void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryStubItemForFB"))
            {
                SetWebIfChanged(folderCache.WebId);
                var folder = this.web.GetFolder(folderObject.DocID);
                if (folder.ParentList == null)
                {
                    queryService.QueryStubItemForFB(folderCache, folderObject, null, mDiscoverReader, includeRecycleBin);
                }
                else
                {
                    var list = folder.ParentList;
                    AveListObject listObj = new AveListObject
                    {
                        ListId = list.ID,
                        RootFolderId = list.RootFolder.UniqueId,
                        Name = list.Title,
                        Title = list.Title,
                        Type = (int)list.BaseType,
                        RootFolderUrl = list.RootFolder.ServerRelativeUrl.Trim('/'),
                        Flag = long.Parse(list.Flags.ToString()),
                        ServerTemplate = (int)list.BaseTemplate,
                        Hidden = list.Hidden
                    };
                    queryService.QueryStubItemForFB(folderCache, folderObject, listObj, mDiscoverReader, includeRecycleBin);
                }
            }
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            SetWebIfChanged(webId);
            return GenerateContentTypes(this.web.ContentTypes);
        }

        private Dictionary<byte[], AveContentTypeObject> GenerateContentTypes(IAveContentTypeCollection contentTypes)
        {
            var results = new Dictionary<byte[], AveContentTypeObject>();
            foreach (var ct in contentTypes)
            {
                AveContentTypeObject contentType = new AveContentTypeObject
                {
                    ContentTypeId = ct.ID.ToByteArray(),
                    SchemaXml = ct.SchemaXml,
                    Name = ct.Name,
                    Scope = ct.Scope.TrimStart('/')
                };
                results.Add(contentType.ContentTypeId, contentType);
            }
            return results;
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl)
        {
            var tempWeb = this.site.OpenWeb(serverRelativeUrl);
            return QueryWebContentTypeForFB(siteId, tempWeb.ID);
        }

        #endregion

        #region For Replicator

        public int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject, AveListObject ParentListObject, bool includeRecycleBin = false)
        {
            //TODO replicator no use
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dirName">Exapmle:"sites/WebTest/Lists/Task/Folder"</param>
        /// <param name="leafName"></param>
        /// <param name="fullUrl">Exapmle:"sites/WebTest/Lists/Task/Folder/5_.000"</param>
        /// <param name="isListItem"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "InitItemObjBasicProperty is the method name")]
        private AveItemObject InitItemObjBasicProperty(IAveListItem item, string dirName, string leafName, string fullUrl)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.InitItemObjBasicProperty"))
            {
                int? checkedOutUserId = null;
                try
                {
                    var checkoutUser = GetCheckOutUser(item);
                    if (checkoutUser != null)
                    {
                        checkedOutUserId = checkoutUser.ID;
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while getting checkout user for this file. Url: {0}, Error: {1}", fullUrl, e);
                }
                ItemType objType = GetItemObjType(item);
                var tempItem = new AveItemObject()
                {
                    DocID = item.UniqueId,
                    SourceName = leafName,
                    LeafName = leafName,
                    ItemName = leafName,
                    ID = item.ID,
                    Type = (objType == ItemType.Folder) ? (byte)1 : (byte)0,
                    Level = (byte)item.Level,
                    FullUrl = fullUrl,
                    DirName = dirName,
                    ObjType = objType,
                    CheckoutUserId = checkedOutUserId,
                    tp_GUID = FieldExist("GUID") ? new Guid((string)item["GUID"]) : default(Guid),
                    Uiversion = FieldExist("_UIVersion") ? (int)item["_UIVersion"] : 0,
                    TimeLastModified = GetItemLastModifiedTime(item),
                };
                return tempItem;
            }
        }

        /// <summary>
        /// 初始化SystermFile基本属性
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="fullUrl"></param>
        /// <returns></returns>
        private AveItemObject InitSystemFileObjBasicProperty(IAveFile file, string dirName, string leafName, string fullUrl)
        {
            var tempItem = new AveItemObject()
            {
                DocID = file.UniqueId,
                SourceName = leafName,
                LeafName = leafName,
                ItemName = leafName,
                Type = 0,
                Level = (byte)file.Level,
                FullUrl = fullUrl,
                DirName = dirName,
                ObjType = ItemType.Document,
                Uiversion = file.UIVersion,
                TimeLastModified = file.Properties.ContainsKey("vti_timelastmodified") ? (DateTime)file.Properties["vti_timelastmodified"] : default(DateTime)
            };
            return tempItem;
        }

        /// <summary>
        /// 初始化SystermFolder基本属性
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="fullUrl"></param>
        /// <returns></returns>
        private AveItemObject InitSystemFolderObjBasicProperty(IAveFolder folder, string dirName, string leafName, string fullUrl)
        {
            var tempItem = new AveItemObject()
            {
                DocID = folder.UniqueId,
                SourceName = leafName,
                LeafName = leafName,
                ItemName = leafName,
                Type = 1,
                FullUrl = fullUrl,
                DirName = dirName,
                ObjType = ItemType.Folder,
                Uiversion = GetFolderUIVersion(folder),
                Level = folder.Properties.ContainsKey("vti_level") ? Byte.Parse(folder.Properties["vti_level"].ToString()) : default(Byte),
                TimeLastModified = folder.Properties.ContainsKey("vti_timelastmodified") ? (DateTime)folder.Properties["vti_timelastmodified"] : default(DateTime)
            };
            return tempItem;
        }

        private DateTime GetItemLastModifiedTime(IAveListItem item)
        {
            DateTime itemLastModifyTime = default(DateTime);
            if (FieldExist("Modified"))
            {
                if (this.timeZone != null)
                {
                    itemLastModifyTime = this.timeZone.LocalTimeToUTC((DateTime)item["Modified"]);
                }
                else
                {
                    itemLastModifyTime = ((DateTime)item["Modified"]).ToUniversalTime();
                }
            }
            return itemLastModifyTime;
        }

        public AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, int? maxMajorwithMinorVersionCount)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetItemExist"))
            {
                IAveSite checkoutSite = null;
                IAveWeb checkoutWeb = null;
                AveItemObject tempItem = null;
                IAveListItem item;
                string serverRelativeUrl = String.Format("/{0}/{1}", dirName, leafName);
                string fullUrl = serverRelativeUrl.TrimStart('/');
                SetWebIfChanged(webId);
                try
                {
                    if (isListItem)
                    {
                        item = GetListItemByUniqueId(listId, id, out checkoutSite, out checkoutWeb);
                        //Attachments discover 过程中会出现url多一个'/'的问题。
                        leafName = item.Url.Substring(item.Url.LastIndexOf('/') + 1);
                        fullUrl = string.Format("/{0}/{1}", dirName.Trim('/'), leafName);
                    }
                    else //folder or document
                    {
                        Object obj = GetFileOrFolderByServerRelativeUrl(serverRelativeUrl, out checkoutSite, out checkoutWeb);
                        var folderObj = obj as IAveFolder;
                        if (folderObj != null)
                        {
                            return InitSystemFolderObjBasicProperty(folderObj, dirName, leafName, fullUrl);
                        }
                        var fileObj = obj as IAveFile;
                        if (fileObj != null)
                        {
                            return InitSystemFileObjBasicProperty(fileObj, dirName, leafName, fullUrl);
                        }
                        item = obj as IAveListItem;
                    }
                    EnsureListInfo(item.ParentList);
                    tempItem = InitItemObjBasicProperty(item, dirName, leafName, fullUrl);
                    tempItem.ParentID = parentId;
                    QueryOneItemVersions(tempItem, item);
                    DisposeCache();
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting item exist. List ID: {0}, Item ID: {1}, Error: {2}", listId, id, e);
                }
                finally
                {
                    DisposeCheckoutSPObjct(checkoutSite, checkoutWeb);
                }
                return tempItem;
            }
        }

        private void QueryOneItemVersions(AveItemObject tempItem, IAveListItem item)
        {
            if (!QueryVersionByNative)
            {
                GenerateVersion(item, tempItem);
            }
            else
            {
                if (item.ID != 0)
                {
                    queryService.QueryItemVersionsForAPI(new Dictionary<int, AveItemObject> { { item.ID, tempItem } }, null, mDiscoverReader);
                }
            }
        }

        private ItemType GetItemObjType(IAveListItem item)
        {
            ItemType objType = ItemType.Item;
            if (item.File != null)
            {
                objType = ItemType.Document;
            }
            else if (item.Folder != null)
            {
                objType = ItemType.Folder;
            }
            return objType;
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            var modifiedTime = DateTime.MinValue;
            SetWebIfChanged(webId);
            var file = this.web.GetFile("/" + dirName.Trim('/') + "/" + leafName);
            if (file.Exists)
            {
                docId = file.Item.UniqueId;
                modifiedTime = file.TimeLastModified;
            }
            return modifiedTime;
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId)
        {
            return queryService.GetItemLastModifiedTime(siteId, listId, rowId);
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {
            return queryService.GetItemLastModifiedTime(siteId, itemId);
        }

        public AveItemObject GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetItemVersions"))
            {
                SetWebIfChanged(webId);
                IAveSite checkoutSite = null;
                IAveWeb checkoutWeb = null;
                AveListTemplateType listTemplateType;
                AveItemObject tempItem = null;
                try
                {
                    var list = web.GetList(listId);
                    var item = GetListItemByDocLibRowId(docLibRowId, list, out checkoutSite, out checkoutWeb, out listTemplateType);
                    //  GetListItem(
                    string fullUrl = string.Format("{0}/{1}", this.web.ServerRelativeUrl, item.Url).Trim('/');
                    string dirName = fullUrl.Substring(0, fullUrl.Length - item.Name.Length - 1);
                    EnsureListInfo(list);
                    tempItem = InitItemObjBasicProperty(item, dirName, item.Name, fullUrl);
                    QueryOneItemVersions(tempItem, item);
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting item versions. List ID: {0}, DocLibRowId: {1}, Error: {2}", listId, docLibRowId, e);
                }
                finally
                {
                    DisposeCheckoutSPObjct(checkoutSite, checkoutWeb);
                }
                return tempItem;
            }
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return queryService.GetDocIdByTp_Guid(siteId, parentId, tp_Guid);
        }

        public Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl)
        {
            return queryService.GetTPGUIDAndDocIdMapping(siteId, parentId);
        }

        public bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName)
        {
            SetWebIfChanged(webId);
            var file = this.web.GetFile("/" + dirName.Trim('/') + "/" + leafName);
            return file.Exists;
        }

        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            SetWebIfChanged(webId);
            var list = this.web.GetList(listId);
            try
            {
                var item = list.GetItemById(rowId);
                return item != null;
            }
            catch (Exception e)
            {
                log.Debug("The item does not exist.WebUrl:{0},List title:{1},RowId:{2},Error:{3}", this.web.Url, list.Title, rowId, e.ToString());
                return false;
            }
        }

        public List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            return queryService.GetItemWebParts(siteId, webId, listId, itemDocId);
        }

        public long GetItemSizeAndUserInfo(Guid siteId, Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {
            return queryService.GetItemSizeAndUserInfo(siteId, webId, listId, docId, level, ref createdBy, ref modifiedBy);
        }

        #endregion

        #region Support Extender

        public int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid docId)
        {
            return queryService.GetCurrentUIVersion(siteId, parentId, docId);
        }

        #endregion

        public void Dispose()
        {
            if (this.web != null)
            {
                this.web.Dispose();
                this.web = null;
            }
            if (queryService != null)
            {
                queryService.Dispose();
                queryService = null;
            }
        }

        public IAveDiscoveryQuery CloneObjWithNewRequest(object aveRequest)
        {
            return this;
        }

        #region IAveDiscoveryQuery Members

        public long GetWebSize(Guid siteId, Guid webId)
        {
            SetWebIfChanged(webId);
            return this.web.Size;
        }

        public long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime)
        {
            SetWebIfChanged(webId);
            return queryService.GetObjectChangedSize(siteId, webId, listId, folderPath, beginTime);
        }

        public long GetListSize(Guid siteId, Guid webId, Guid listid)
        {
            SetWebIfChanged(webId);
            return queryService.GetListSize(siteId, webId, listid);
        }

        public long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl)
        {
            SetWebIfChanged(webId);
            return queryService.GetFolderSize(siteId, webId, listId, folderUrl);
        }

        #endregion

        [Obsolete("Unsed method")]
        public void QueryVersionsByItemObj(AveItemCache itemCache, AveItemObject itemObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryVersionsByItemObject"))
            {
                itemObject.VersionObjs.Clear();
                if ((itemCache.ListId != Guid.Empty ? web.GetList(itemCache.ListId).EnableVersioning : false)
                    && (itemCache.AveWeb != null && itemObject.ID != null))
                {
                    IAveSite checkedOutSite = null;
                    IAveWeb checkedOutWeb = null;
                    try
                    {
                        IAveListItem item = null;
                        if (((itemCache.ItemId.HasValue && itemCache.ItemId > 0) || (itemObject.ID.HasValue && itemObject.ID > 0)) && itemCache.ListId != Guid.Empty)
                        { //出于效率方面考虑，使用row id获取item效率更高
                            AveListTemplateType listTemplateType = AveListTemplateType.NoListTemplate;
                            var list = web.GetList(itemCache.ListId);
                            int rowId = (itemCache.ItemId > 0 ? itemCache.ItemId : itemObject.ID).Value;
                            item = GetListItemByDocLibRowId(rowId, list, out checkedOutSite, out checkedOutWeb, out listTemplateType);
                        }
                        else
                        {
                            item = GetListItem(itemCache, itemObject, out checkedOutSite, out checkedOutWeb);
                        }
                        GenerateVersion(item, itemObject);
                    }
                    catch (Exception e)
                    {
                        log.Error("Get item versions failed. Item Url: {0}, Error: {1}", itemObject.FullUrl, e);
                    }
                    finally
                    {
                        DisposeCheckoutSPObjct(checkedOutSite, checkedOutWeb);
                    }
                }
                else//System File或者list本身没开version的 需要将Item本身添加到VersionObjects中。
                {
                    AddCurrentVersionObj(itemObject);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AddCurrentVersionObj is the method name")]
        private void AddCurrentVersionObj(AveItemObject itemObj)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.AddCurrentVersionObj"))
            {
                itemObj.VersionObjs.Add(new AveVersionObject
                {
                    UserDataGuid = itemObj.tp_GUID,
                    Uiversion = itemObj.Uiversion,
                    Level = (byte)itemObj.Level,
                    IsCurrentVersion = true,
                    HasStream = itemObj.HasStream,
                    TimeLastModified = itemObj.TimeLastModified,
                    Size = itemObj.Size
                });
            }
        }


        private IAveListItem GetListItem(AveItemCache itemCache, AveItemObject itemObject, out IAveSite checkoutSite, out IAveWeb checkoutWeb)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetListItem"))
            {
                checkoutSite = null;
                checkoutWeb = null;
                IAveListItem item = null;

                try
                {
                    IAveUser checkoutUser = null;
                    if (itemObject.CheckoutUserId.HasValue)
                    {
                        try
                        {
                            checkoutUser = this.site.RootWeb.SiteUsers.GetByID((int)itemObject.CheckoutUserId);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Can not found this user. User: {0}, Error: {1}", itemObject.CheckoutUserId, ex);
                        }
                    }
                    item = GetListItemByFullUrl(itemObject.FullUrl, itemCache.ListId, itemCache.AveWeb, itemObject.DocID, checkoutUser, out checkoutSite, out checkoutWeb);
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while get item. Item Url: {0}, Error: {1}", itemObject.FullUrl, e);
                }
                return item;
            }
        }

        /// <summary>
        /// Only for checkout file.
        /// </summary>
        /// <param name="docLibRowId"></param>
        /// <param name="checkoutUser"></param>
        /// <param name="checkoutSite">Must been disposed by caller.</param>
        /// <param name="checkoutWeb">Must been disposed by caller.</param>
        /// <returns></returns>
        private IAveListItem GetCheckoutFileItem(IAveCheckedOutFile checkoutFile, out IAveSite checkoutSite, out IAveWeb checkoutWeb)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetCheckoutFileItem"))
            {
                checkoutSite = null;
                checkoutWeb = null;
                IAveListItem item = null;
                try
                {
                    var serverRelativeUrl = checkoutFile.Url;
                    if (!serverRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        serverRelativeUrl = "/" + serverRelativeUrl;
                    }
                    //在最小权限user的情况下，user的Sid为空,固改成用ID进行比较
                    if (checkoutFile.CheckedOutBy.ID != site.RootWeb.CurrentUser.ID)
                    {
                        checkoutSite = factory.CreateSite(site.Url, checkoutFile.CheckedOutBy.UserToken);
                        checkoutWeb = checkoutSite.OpenWeb(web.ID);
                        item = checkoutWeb.GetFile(serverRelativeUrl).Item;
                    }
                    else//web.currentuser就是checkout user，直接用当前web get item即可。
                    {
                        item = web.GetFile(serverRelativeUrl).Item;
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting checkout item. Item: {0}, User: {1}, Error: {2}", checkoutFile.Url, checkoutFile.CheckedOutByName, e);
                }
                return item;
            }
        }

        /// <summary>
        /// Replicator和IB专用。通过DocID获取Item
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="checkoutSite">Must been disposed by caller.</param>
        /// <param name="checkoutWeb">Must been disposed by caller.</param>
        /// <returns></returns>
        private IAveListItem GetListItemByUniqueId(Guid listId, Guid uniqueId, out IAveSite checkoutSite, out IAveWeb checkoutWeb)
        {
            using (var scope = new AvePerformanceScope("DiscoverQueryForAPI.GetListItemByUniqueId"))
            {
                checkoutSite = null;
                checkoutWeb = null;
                var list = this.web.GetList(listId);
                IAveListItem item = null;
                using (var spscope = new AvePerformanceScope("SP.DiscoverQueryForAPI.GetListItemByUniqueId"))
                {
                    item = list.GetItemByUniqueId(uniqueId);
                }
                try
                {
                    var checkoutItem = GetListItemCheckOutVersion(item, out checkoutSite, out checkoutWeb);
                    if (checkoutItem != null)
                    {
                        return checkoutItem;
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting checkout version. Item: {0}, Error: {1}", item.Url, e);
                }
                return item;
            }
        }

        /// <summary>
        /// 根据serverRelativeUrl来获取file或folder
        /// web.GetObject方法获取的SP对象，如果为IAveListItem类型，则可能为listItem,普通folder,普通file;如果为IAveFolder类型，则为SystemFolder；如果为IAveFile类型，则为SystemFile.
        /// </summary>
        /// <param name="serverRelativeUrl"></param>
        /// <param name="checkoutSite"></param>
        /// <param name="checkoutWeb"></param>
        /// <returns></returns>
        private Object GetFileOrFolderByServerRelativeUrl(string serverRelativeUrl, out IAveSite checkoutSite, out IAveWeb checkoutWeb)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetFileOrFolderByServerRelativeUrl"))
            {
                checkoutSite = null;
                checkoutWeb = null;
                Object obj = web.GetObject(serverRelativeUrl);
                var item = obj as IAveListItem;
                try
                {
                    //普通的file，要返回其checkoutVersion.
                    if (item != null && item.File != null)
                    {
                        var checkoutItem = GetListItemCheckOutVersion(item, out checkoutSite, out checkoutWeb);
                        if (checkoutItem != null)
                        {
                            return checkoutItem;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting checkout version. Item: {0}, Error: {1}", item.Url, e);
                }
                return obj;
            }
        }

        /// <summary>
        /// 用已经取到的Item获取它的Checkout Item。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="listId"></param>
        /// <param name="checkoutSite"></param>
        /// <param name="checkoutWeb"></param>
        /// <returns>如果Item被checkout，返回checkout item。否则null</returns>
        private IAveListItem GetListItemCheckOutVersion(IAveListItem item, out IAveSite checkoutSite, out IAveWeb checkoutWeb)
        {
            using (var scope = new AvePerformanceScope("DiscoverQueryForAPI.GetListItemCheckoutVersion"))
            {
                checkoutSite = null;
                checkoutWeb = null;
                IAveUser checkoutUser = GetCheckOutUser(item);
                //在最小权限user的情况下，user的Sid为空,固改成用ID进行比较
                if (checkoutUser == null || checkoutUser.ID == site.RootWeb.CurrentUser.ID)
                {
                    return null;
                }
                log.Debug("Get item checkout version");
                checkoutSite = factory.CreateSite(site.Url, checkoutUser.UserToken);
                checkoutWeb = checkoutSite.OpenWeb(web.ID);
                return checkoutWeb.GetFile(item.File.ServerRelativeUrl).Item;
            }
        }

        /// <summary>
        /// Replicator专用。通过DocLibRowId获取Item。
        /// </summary>
        /// <param name="docLibRowId"></param>
        /// <param name="listId"></param>
        /// <param name="checkoutSite">Must been disposed by caller.</param>
        /// <param name="checkoutWeb">Must been disposed by caller.</param>
        /// <param name="listType"></param>
        /// <returns></returns>
        private IAveListItem GetListItemByDocLibRowId(int docLibRowId, IAveList list, out IAveSite checkoutSite, out IAveWeb checkoutWeb, out AveListTemplateType listType)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.GetListItemByRowId"))
            {
                checkoutSite = null;
                checkoutWeb = null;

                IAveListItem item = null;
                using (var spscope = new AvePerformanceScope("SP.List.GetItemById"))
                {
                    item = list.GetItemById(docLibRowId);
                }
                listType = list.BaseTemplate;
                try
                {
                    var checkoutItem = GetListItemCheckOutVersion(item, out checkoutSite, out checkoutWeb);
                    if (checkoutItem != null)
                    {
                        return checkoutItem;
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting checkout  item. Item: {0}, Error: {1}", item.Url, e);
                }
                return item;
            }
        }

        /// <summary>
        /// Get Item之前已经知道文件是否被checkout。单独Discover Version 用到此方法.
        /// </summary>
        /// <param name="fullUrl"></param>
        /// <param name="listId"></param>
        /// <param name="web"></param>
        /// <param name="docId"></param>
        /// <param name="checkoutUser"></param>
        /// <param name="checkoutSite">Must been disposed by caller.</param>
        /// <param name="checkoutWeb">Must been disposed by caller.</param>
        /// <returns></returns>
        private IAveListItem GetListItemByFullUrl(string fullUrl, Guid listId, IAveWeb web, Guid docId, IAveUser checkoutUser, out IAveSite checkoutSite, out IAveWeb checkoutWeb)
        {
            checkoutSite = null;
            checkoutWeb = null;
            try
            {
                //在最小权限user的情况下,user的Sid为空,固改成用ID进行比较
                if (checkoutUser != null && checkoutUser.ID != site.RootWeb.CurrentUser.ID)
                {
                    checkoutSite = factory.CreateSite(site.Url, checkoutUser.UserToken);
                    checkoutWeb = checkoutSite.OpenWeb(web.ID);
                    return checkoutWeb.GetListItem(fullUrl, listId, docId);
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting checkout item. Item: {0}, User: {1}, Error: {2}", fullUrl, checkoutUser.LoginName, e);
            }
            return web.GetListItem(fullUrl, listId, docId);
        }

        private void DisposeCheckoutSPObjct(params IDisposable[] spObjects)
        {
            if (spObjects == null)
            {
                return;
            }
            for (int index = 0; index < spObjects.Count(); index++)
            {
                var spObject = spObjects[index];
                if (spObject != null)
                {
                    spObject.Dispose();
                    spObject = null;
                }
            }
        }

        public void QueryAttachmentByItemObj(IAveWeb web, Guid listId, AveItemObject itemObject)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQueryForAPI.QueryAttachmentByItemObject"))
            {
                itemObject.AttachmentObjs.Clear();
                if (web != null && listId != Guid.Empty && itemObject.ID.HasValue)
                {
                    //SetWebIfChanged(itemCache.WebId);
                    var parentList = web.GetList(listId);
                    if (!parentList.EnableAttachments)
                    {
                        return;
                    }
                    //var item = parentList.GetItemById(itemObject.ID.Value);
                    IAveListItem item = null;
                    if (parentList != null && itemObject.ID > 0)
                    {
                        item = parentList.GetItemById(itemObject.ID.Value);
                    }
                    else
                    {
                        item = web.GetListItem(itemObject.FullUrl, listId, itemObject.DocID);
                    }
                    try
                    {
                        if (item != null && item.File == null)
                        {
                            if (item.Fields.ContainsField("Attachments") && (bool)item["Attachments"])  //出于效率考虑，Attachments column为true时才遍历
                            {
                                foreach (IAveAttachment attachment in item.Attachments)
                                {
                                    AveItemObject attachmentObj = InitAttachment(attachment, itemObject, parentList);
                                    itemObject.AttachmentObjs.Add(attachmentObj);
                                }
                            }
                            queryService.SetAttachmentsStubInfo(itemObject.AttachmentObjs, web.Site.ID, mDiscoverReader);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Debug("An error occurred while get attachment. Item ID:{0}, parent list url:{1}, error message:{2}", item.ID, parentList.DefaultDisplayFormUrl, e);
                    }
                }
            }
        }

        public void DiscoverAllListContent(AveListCache listCache, AveItemObject rootFolderObj, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        #region IAveDiscoveryQuery Members






        #endregion
    }

    internal enum DiscoverRowName
    {
        EventTime,
        Id,
        SiteId,
        WebId,
        ListId,
        ItemId,
        DocId,
        Guid0,
        Int0,
        ContentTypeId,
        ItemFullUrl,
        EventType,
        ObjectType,
        TimeLastModified,
        Int1,
        DocClientId
    }
}
