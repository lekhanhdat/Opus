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
namespace AvePoint.Wrapper.QueryService
{
    using System;
    using System.Data;
    using System.Collections.Generic;
    using AvePoint.Wrapper.Common;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon;
    using System.Globalization;
    using static SP2016SelectQueryString;
    using static SP2016DiscoverQueryStringSelect;


    internal partial class AveQueryService : IAveDiscoverQueryService
    {

        private BusinessLayerForDiscover discoverCommon = new BusinessLayerForDiscover();
        private const int DocList = 1;

        #region private methods

        /// <summary>
        /// 初始化ParentListObject的信息，包括system folder
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="noPropertyFolders"></param>
        /// <param name="discoverReader"></param>
        /// <returns></returns>
        private AveListObject InitParentList(AveFolderCache folderCache, Dictionary<string, AveItemObject> noPropertyFolders, AveDiscoverReader discoverReader)
        {
            var listObj = QuerySingleListProperty(folderCache);
            if (listObj != null)
            {
                folderCache.ListUrl = listObj.RootFolderUrl; // 只能在这设置，cache 层取不到 listObject 对象
            }
            QueryFolderProperty(folderCache, noPropertyFolders, discoverReader, listObj);
            return listObj;
        }

        private Guid GetListIdByRootFolderUrl(string rootFolderUrl, Guid siteId)
        {
            string dirName;
            string leafName;
            AveUrlUtility.SplitUrl(rootFolderUrl, out dirName, out leafName);
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@DirName", dirName);
            mQueryWorker.AddParameter("@LeafName", leafName);
            return (Guid)mQueryWorker.ExecuteScalar(GetListIdByDirNameLeafName_Select_AllDocs);
        }

        /// <summary>
        /// 查询单个List的属性
        /// </summary>
        /// <param name="folderCache"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "tp_MaxMajorwithMinorVersionCount")]
        private AveListObject QuerySingleListProperty(AveFolderCache folderCache)
        {
            AveListObject listObject = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
                mQueryWorker.AddParameter("@WebId", folderCache.WebId);
                mQueryWorker.AddParameter("@ListId", folderCache.ListId);
                using (var sr = mQueryWorker.ExecuteReader(GetListWithRootFolderById_Select_AllLists_AllDocs))
                {
                    if (sr.Read())
                    {
                        try
                        {
                            listObject = new AveListObject
                            {
                                ListId = folderCache.ListId,
                                RootFolderUrl = (string)sr["DirName"] + "/" + (string)sr["LeafName"]
                            };
                            AveDiscoverSqlUtility.InitListObjBasicPropertiesByReader(listObject, sr);
                            if (!Convert.IsDBNull(sr["tp_MaxMajorwithMinorVersionCount"]))
                            {
                                listObject.MaxMajorwithMinorVersionCount = (int)sr["tp_MaxMajorwithMinorVersionCount"];
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from InitDiscoverFolder.GetListInfo.QuerySingleListProperty. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
            return listObject;
        }

        [QueryReview("2012/05/21", "Oliver Luo", false, "在调用方法中Review")]
        private void QueryAttachmentForFB(string commText, Dictionary<int, AveItemObject> attachmentItems, IAveDiscoverReader discoverReader)
        {
            ExceptionHandlingScope(() =>
            {
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var dirName = (string)sr["DirName"];
                            var pos = dirName.LastIndexOf('/');
                            if (pos < 0)
                            {
                                continue;
                            }
                            int subId;
                            AveItemObject item;
                            if (int.TryParse(dirName.Substring(pos + 1), out subId)
                                && attachmentItems.TryGetValue(subId, out item))
                            {
                                var attachment = new AveItemObject();
                                discoverReader.ReadAttachmentContent(attachment, sr);
                                item.AttachmentObjs.Add(attachment);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "An error occurred while getting data from method QueryAttachmentForFB. ErrorMessage:{0}", e);
                        }
                    }
                }
                //deal stub Attachment Extender  
                if (discoverReader is AveExtenderDiscoverReader)
                {
                    GetAttanchemntStub(attachmentItems);
                }
            });
        }
        private void GetAttanchemntStub(Dictionary<int, AveItemObject> attachmentItems)
        {
            var result = from pair in attachmentItems orderby pair.Key select pair;
            foreach (KeyValuePair<int, AveItemObject> pair in result)
            {
                foreach (var att in pair.Value.AttachmentObjs)
                {
                    mQueryWorker.AddParameter("@DocId", att.DocID);
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveQueryString16.Sp16ContentOrStub))
                    {
                        if (sr.HasRows)
                        {
                            pair.Value.StubAttachmentObjs.Add(att);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 此处是查询某些只有URL 的folder 的属性信息，包括versions
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="folderCache"></param>
        /// <param name="noPropertyFolders"></param>
        /// <param name="discoverReader"></param>
        /// <param name="listObject"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Urls is a variable")]
        private void QueryFolderProperty(AveFolderCache folderCache, Dictionary<string, AveItemObject> noPropertyFolders, AveDiscoverReader discoverReader, AveListObject listObject)
        {
            if (noPropertyFolders.Count <= 0)
            {
                return;
            }
            ExceptionHandlingScope(() =>
            {
                var needSearchFolders = noPropertyFolders.Values.ToList();

                var index = 0;
                AveItemObject folder = null;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteId", folderCache.SiteId);
                while (index < needSearchFolders.Count)
                {
                    var needSearchVersions = new Dictionary<Guid, AveItemObject>();

                    #region  拼接sql

                    var commText = GetFolderPropertiesByUrls_Select_AllDocs(discoverReader, needSearchFolders, ref index, 400);

                    #endregion  拼接sql

                    var idsbuilder = new StringBuilder();
                    using (var sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                var id = (Guid)sr["Id"];
                                if (!needSearchVersions.ContainsKey(id))
                                {
                                    idsbuilder.AppendFormat("'{0}',", id);
                                    //rootSC,dirName可能为empty.
                                    var fullName = $"{sr["DirName"]}/{sr["LeafName"]}".TrimStart('/');
                                    folder = noPropertyFolders[fullName];
                                    needSearchVersions[id] = folder;
                                    discoverReader.ReadItemContent(folder, sr);
                                    folder.DirName = (string)sr["DirName"];
                                    folder.FullUrl = fullName;
                                    folder.ObjType = ItemType.Folder;
                                    folder.ParentID = (Guid)sr["ParentId"];
                                    noPropertyFolders.Remove(fullName);
                                }
                                var version = new AveVersionObject();
                                discoverReader.ReadVersionContent(version, sr);
                                AddVersion(version, folder, sr, discoverReader);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from query in QueryFolderProperty. ErrorMessage:{0}.", e);
                            }
                        }
                    }
                    if (idsbuilder.Length > 0)
                    {
                        idsbuilder.Length--;
                        var condition = string.Format(discoverReader.GetItemVersionsWithDocIdsCondition(), idsbuilder.ToString());
                        var allUserDataQueryCommandText = GetItemVersions_Select_AllUserData(discoverReader, condition);
                        QueryItemVersions(allUserDataQueryCommandText, needSearchVersions, discoverReader, listObject);
                    }
                }
            });
        }

        /// <summary>
        /// Add the interface for discover API. if there is any changes, it doesn't effect the native  method.
        /// </summary>
        /// <param name="itemCollection"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        public void QueryItemVersionsForAPI(Dictionary<int, AveItemObject> itemCollection, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryItemVersionsForAPI"))
            {
                AddVersionToItems(itemCollection, listObject, discoverReader);
            }
        }

        public void QueryItemVersionsForAPIFB(Guid siteId, Guid parentId, List<AveItemObject> itemObjs, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryItemVersionsForAPIFB"))
            {
                bool includeRecycleBin = false;
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                var queryCommandText = string.Empty;
                if (listObject == null)//System Folder
                {
                    queryCommandText = GetWebItemVersionsByParentId_Select_AllUserDataORAllVersions(includeRecycleBin, discoverReader);
                    mQueryWorker.AddParameter("@ListId", Guid.Empty);
                }
                else
                {
                    queryCommandText = GetListItemVersionsByParentId_Select_AllUserDataORAllVersions(includeRecycleBin, discoverReader);
                    mQueryWorker.AddParameter("@ListId", listObject.ListId);
                }
                QueryItemVersions(queryCommandText, itemObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject);
            }
        }

        private void QueryItemVersions(string allUserDataQueryCommandText, Dictionary<Guid, AveItemObject> items, IAveDiscoverReader discoverReader, AveListObject listObject)
        {
            ExceptionHandlingScope(() =>
            {
                if (items.Count > 0) //查到Item 情况才需要查version
                {
                    var isSpecialLibrary = listObject != null && listObject.Type == 1 && listObject.MaxMajorwithMinorVersionCount.HasValue;
                    if (isSpecialLibrary && !(discoverReader is AveExtenderDiscoverReader))
                    {
                        QueryItemVersionsInUDForVersionLimitedLibrary(items, discoverReader, allUserDataQueryCommandText);
                    }
                    else
                    {
                        QueryItemVersionsInUDBasic(items, discoverReader, allUserDataQueryCommandText);
                    }
                }
            });
        }

        /// <summary>
        /// 查询List和没有version limitations的library中item的version
        /// </summary>
        /// <param name="collections"></param>
        /// <param name="discoverReader"></param>
        /// <param name="userDataCommandText"></param>
        private void QueryItemVersionsInUDBasic(Dictionary<Guid, AveItemObject> collections, IAveDiscoverReader discoverReader, string userDataCommandText)
        {
            AveItemObject previousItem = null;
            using (var sr = mQueryWorker.ExecuteReader(userDataCommandText))
            {
                while (sr.Read())
                {
                    var docId = (Guid)sr["tp_DocId"];
                    if (previousItem == null || previousItem.DocID != docId)
                    {
                        if (!collections.TryGetValue(docId, out previousItem))
                        {
                            continue;
                        }
                    }
                    var version = new AveVersionObject();
                    discoverReader.ReadVersionContentWithDeleteState(version, sr);
                    AddVersion(version, previousItem, sr, discoverReader);
                }
            }
        }

        /// <summary>
        /// 查询version limitated library中的item version
        /// </summary>
        /// <param name="collections"></param>
        /// <param name="discoverReader"></param>
        /// <param name="userDataCommandText"></param>
        private void QueryItemVersionsInUDForVersionLimitedLibrary(Dictionary<Guid, AveItemObject> collections, IAveDiscoverReader discoverReader, string userDataCommandText)
        {
            AveItemObject previousItem = null;
            var allItems = collections.Select(item => item.Value.DocID).ToArray();
            int index = 0;
            var allDocVersionsCache = new Dictionary<Guid, List<int>>();
            while (index < allItems.Length)
            {
                List<Guid> queryItemDocIds = new List<Guid>();
                //SQL command text limited 64k
                for (var idCount = 0; idCount < 800; ++idCount)
                {
                    queryItemDocIds.Add(allItems[index++]);
                    if (index >= allItems.Length)
                    {
                        break;
                    }
                }
                var allDocVersionsCommand = AveQueryUtility.GetAllDocVersionsForSpecialLibrary_Select_AllDocVersions(queryItemDocIds);
                using (var sr = mQueryWorker.ExecuteReader(allDocVersionsCommand))
                {
                    while (sr.Read())
                    {
                        var allDocVersionId = (Guid)sr["Id"];
                        var uiVersion = (int)sr["UIVersion"];
                        if (allDocVersionsCache.ContainsKey(allDocVersionId))
                        {
                            allDocVersionsCache[allDocVersionId].Add(uiVersion);
                        }
                        else
                        {
                            var uiList = new List<int> { uiVersion };
                            allDocVersionsCache.Add(allDocVersionId, uiList);
                        }
                    }
                }
            }
            using (var sr = mQueryWorker.ExecuteReader(userDataCommandText))
            {
                while (sr.Read())
                {
                    var audDocId = (Guid)sr["tp_DocId"];
                    var audCalculatedVersion = Convert.ToInt32(sr["tp_CalculatedVersion"]);
                    var audTPCurrentVersion = Convert.ToInt32(sr["tp_IsCurrentVersion"]);
                    if (audTPCurrentVersion == 1 || (allDocVersionsCache.ContainsKey(audDocId) && allDocVersionsCache[audDocId].Exists(uiversion => uiversion == audCalculatedVersion)))
                    {
                        try
                        {
                            var docId = (Guid)sr["tp_DocId"];
                            if (previousItem == null || previousItem.DocID != docId)
                            {
                                if (!collections.TryGetValue(docId, out previousItem))
                                {
                                    continue;
                                }
                            }
                            var version = new AveVersionObject();
                            discoverReader.ReadVersionContentWithDeleteState(version, sr);
                            AddVersion(version, previousItem, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from AddVersionToItems. ErrorMessage:{0}", e);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 向VersionObjs集合中添加一个version,原则上覆盖同version number的数据，保证插入的数据是从大到小排序
        /// </summary>
        /// <param name="version"></param>
        /// <param name="currentItem"></param>
        /// <param name="sr"></param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        private void AddVersion(AveVersionObject version, AveItemObject currentItem, SqlDataReader sr, IAveDiscoverReader discoverReader)
        {
            //UD 表覆盖alldoc 表中数据。  currentItem.Uiversion  在所有引用地方都已经赋值。
            if (currentItem.Uiversion == version.Uiversion)
            {
                discoverReader.OverriteProperties(sr, currentItem);
            }
            discoverCommon.AddVersionToOrderedItemVersions(version, currentItem);
        }

        /// <summary>
        /// 查询单个Item的attachments
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="listRootFolder"></param>
        /// <param name="discoverReader"></param>
        /// <param name="item"></param>
        private void QueryAttachmentsForSingleItem(Guid siteId, Guid webId, Guid listId, string listRootFolder, IAveDiscoverReader discoverReader, AveItemObject item)
        {
            if (!item.ID.HasValue)
            {
                return;
            }
            var itemEntities = new Dictionary<int, AveItemObject> { { (int)item.ID, item } };
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ItemId", item.ID);
            mQueryWorker.AddParameter("@AttachmentUrl", (listRootFolder).Trim('/') + "/" + "Attachments");
            var queryString = GetSingleItemAttachments_Select_AllDocs(discoverReader);
            QueryAttachmentForFB(queryString, itemEntities, discoverReader);
        }

        #endregion private methods

        /// <summary>
        /// 初始化DiscoverFolder
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObj"></param>
        /// <param name="noPropertyFolders"></param>
        /// <param name="listObject"> 缓存在APIDiscover 层，设计有问题，需要修改</param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/05/21", "Oliver Luo", true, "AllDocs增加Level索引")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "tp_MaxMajorwithMinorVersionCount is a part of Keys")]
        public void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj, Dictionary<string, AveItemObject> noPropertyFolders, ref AveListObject listObject, AveDiscoverReader discoverReader)
        {
            var listObj = listObject;
            ExceptionHandlingScope(() =>
            {
                noPropertyFolders[folderObj.FullUrl] = folderObj;
                if (folderCache.ListId == Guid.Empty)
                {
                    listObj = null;
                    QueryFolderProperty(folderCache, noPropertyFolders, discoverReader, listObj);
                }
                else
                {
                    listObj = InitParentList(folderCache, noPropertyFolders, discoverReader);
                    if (listObj != null && listObj.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObj.Flag))
                    {
                        QueryAttachmentsForSingleItem(folderCache.SiteId, folderCache.WebId, folderCache.ListId, listObj.RootFolderUrl, discoverReader, folderObj);
                    }
                }
            });
            listObject = listObj;
        }

        /// <summary>
        /// 初始化DiscoverList
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="listObj"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        public void InitDiscoverList(AveListCache listCache, AveListObject listObj)
        {
            ExceptionHandlingScope(() =>
            {
                var listId = GetListIdByRootFolderUrl(listObj.RootFolderUrl, listCache.SiteId);

                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@SiteId", listCache.SiteId);
                mQueryWorker.AddParameter("@WebId", listCache.WebId);
                using (var sr = mQueryWorker.ExecuteReader(GetListById_Select_AllLists))
                {
                    if (sr.Read())
                    {
                        try
                        {
                            listObj.ListId = listId;
                            listObj.RootFolderUrl = listObj.RootFolderUrl.Trim('/');
                            AveDiscoverSqlUtility.InitListObjBasicPropertiesByReader(listObj, sr);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from InitDiscoverList.SiteId:{0}. RootFolderUrl:{1}.  ErrorMessage:{2}", listCache.SiteId, listObj.RootFolderUrl, e);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 根据FullUrl和SiteId 查询,初始化 DiscoverWeb 的基本信息
        /// </summary>
        /// <param name="webCache"></param>
        /// <param name="webObj"></param>
        public void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj)
        {
            ExceptionHandlingScope(() =>
            {
                var len = -1;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", webCache.SiteId);
                mQueryWorker.AddParameter("@FullUrl", webObj.FullUrl);
                using (var sr = mQueryWorker.ExecuteReader(GetWebByFullUrlAndSiteId_Select_AllWebs))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var webId = sr.GetGuid(0);
                            var fullUrl = sr.GetString(1);
                            var title = sr.GetString(2);

                            if (fullUrl.Equals(webObj.FullUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                webObj.WebID = webId;
                                webObj.FullUrl = fullUrl;
                                webObj.Title = title;
                                //full url - root web url, if len <=0, this should be a root web
                                webObj.Name = len > 0 ? fullUrl.Substring(len).TrimStart('/') : ".";
                            }
                            else //must be root web
                            {
                                len = sr.GetString(1).Length;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from method InitDiscoverWeb.SiteId:{0}. WebId:{1}.  ErrorMessage:{2}", webCache.SiteId, webCache.WebId, e);
                        }
                    }
                }
            });
        }

        #region for replicator

        #region for replicator private methods

        /// <summary>
        /// 根据DocId获取ListItem的信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="discoverReader"></param>
        /// <returns></returns>
        private AveItemObject GetListItemInfoFromDocs(Guid siteId, Guid parentId, Guid id, IAveDiscoverReader discoverReader)
        {
            AveItemObject item = null;
            ExceptionHandlingScope(() =>
            {
                var commText = GetListItemInfoByDocId_Select_AllDocs(discoverReader);
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@Id", id);

                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            if (item == null)
                            {
                                item = new AveItemObject
                                {
                                    ObjType = ((byte)sr["Type"] == 1) ? ItemType.Folder : ItemType.Item,
                                    DirName = (string)sr["DirName"],
                                    LeafName = (string)sr["LeafName"],
                                    ParentID = parentId
                                };
                                item.FullUrl = AveUrlUtility.CombineUrl(item.DirName, item.LeafName);
                                discoverReader.ReadItemContent(item, sr);
                            }
                            var version = new AveVersionObject();
                            discoverReader.ReadVersionContent(version, sr);
                            AddVersion(version, item, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from GetItemExist.GetItemInfoFromDocs. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
            return item;
        }

        /// <summary>
        /// 根据DirName,LeafName获取Document的信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="discoverReader"></param>
        /// <returns></returns>
        private AveItemObject GetDocumentInfoFromDocs(Guid siteId, string dirName, string leafName, IAveDiscoverReader discoverReader)
        {
            AveItemObject item = null;
            ExceptionHandlingScope(() =>
            {
                var commText = GetDocumentInfoByName_Select_AllDocs(discoverReader);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@LeafName", leafName);
                mQueryWorker.AddParameter("@DirName", dirName);
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            if (item == null)
                            {
                                item = new AveItemObject
                                {
                                    DirName = dirName,
                                    LeafName = leafName,
                                    ParentID = (Guid)sr["ParentId"],
                                    ObjType = ((byte)sr["Type"] == 1) ? ItemType.Folder : ItemType.Document
                                };
                                discoverReader.ReadItemContent(item, sr);
                                item.FullUrl = AveUrlUtility.CombineUrl(item.DirName, item.LeafName);
                            }
                            var version = new AveVersionObject();
                            discoverReader.ReadVersionContent(version, sr);
                            AddVersion(version, item, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from GetItemExist.GetItemInfoFromDocs. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
            return item;
        }

        /// <summary>
        /// 根据DirName,LeafName获取一个Item的Id，Modified，DocId信息,查询AllDocs表
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <returns>AveItemObject，只有DocID，TimeLastModified，ID三个属性初始化了</returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        private AveItemObject GetItemIdDocIdLastModifiedTimeFromDocs(Guid siteId, string dirName, string leafName)
        {
            var item = new AveItemObject();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DirName", dirName);
                mQueryWorker.AddParameter("@LeafName", leafName);
                using (var sr = mQueryWorker.ExecuteReader(GetItemIdModifiedInfoByName_Select_AllDocs))
                {
                    if (sr.Read())
                    {
                        item.DocID = sr.GetGuid(0);
                        item.TimeLastModified = sr.GetDateTime(1);
                        item.ID = sr.IsDBNull(2) ? 0 : sr.GetInt32(2);
                    }
                }
            });
            return item;
        }

        private void GetCreateByModifiedByUserInfo(Guid siteId, Guid parentId, Guid docId, out string createdBy, out string modifiedBy)
        {
            var createdByUser = string.Empty;
            var modifiedByUser = string.Empty;
            ExceptionHandlingScope(() =>
            {
                int createdUserId;
                int modifiedUserId;
                GetAuthorEditorIdByDocId(siteId, parentId, docId, out createdUserId, out modifiedUserId);
                createdByUser = createdUserId != 0 ? GetUserTitleById(siteId, createdUserId) : string.Empty;
                modifiedByUser = modifiedUserId != 0 ? GetUserTitleById(siteId, modifiedUserId) : string.Empty;
            });
            createdBy = createdByUser;
            modifiedBy = modifiedByUser;
        }

        private string GetUserTitleById(Guid siteId, int createdUserId)
        {
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@UserId", createdUserId);
            return (string)mQueryWorker.ExecuteScalar(GetUserTitleById_Select_UserInfo);
        }

        private void GetAuthorEditorIdByDocId(Guid siteId, Guid parentId, Guid docId, out int createdUserId, out int modifiedUserId)
        {
            createdUserId = 0;
            modifiedUserId = 0;
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@DocId", docId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            using (var sr = mQueryWorker.ExecuteReader(GetAuthorAndEditorByDocIdParentId_Select_AllUserData))
            {
                if (sr.Read())
                {
                    createdUserId = sr["tp_Author"] is DBNull ? 0 : (int)sr["tp_Author"];
                    modifiedUserId = sr["tp_Editor"] is DBNull ? 0 : (int)sr["tp_Editor"];
                }
            }
        }

        #endregion for replicator private methods

        /// <summary>
        /// 根据Id或name check item是否存在，存在返回对应的Item信息
        /// </summary>
        /// <param name="SiteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="listRootFolder"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="isListItem"></param>
        /// <param name="discoverReader"></param>
        /// <param name="maxMajorwithMinorVersionCount"></param>
        /// <returns></returns>
        public AveItemObject GetItemExist(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, AveDiscoverReader discoverReader, int? maxMajorwithMinorVersionCount)
        {
            AveItemObject item = null;
            ExceptionHandlingScope(() =>
            {
                //todo:wbhu，1.方法拆分成两个，需要review和测试  2.item.ID属性看逻辑没有初始化，需要再确认下是否会有问题，因为下面用到这个属性了
                item = isListItem ? GetListItemInfoFromDocs(siteId, parentId, id, discoverReader) : GetDocumentInfoFromDocs(siteId, dirName, leafName, discoverReader);
                if (item != null)
                {
                    AveListObject listObject = null;
                    //Special Library.
                    if (!isListItem && item.ID.HasValue && item.ID.Value > 0 && maxMajorwithMinorVersionCount.HasValue)
                    {
                        listObject = new AveListObject
                        {
                            Type = 1,
                            MaxMajorwithMinorVersionCount = maxMajorwithMinorVersionCount
                        };
                    }
                    mQueryWorker.ResetCommand(CommandType.Text);
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@ParentId", item.ParentID);
                    mQueryWorker.AddParameter("@Id", item.DocID);
                    var condition = discoverReader.GetItemVersionsWithDocIdCondition();
                    var allUserDataQueryCommandText = GetItemVersions_Select_AllUserData(discoverReader, condition);
                    QueryItemVersions(allUserDataQueryCommandText, new Dictionary<Guid, AveItemObject> { { item.DocID, item } }, discoverReader, listObject);

                    if (isListItem && !string.IsNullOrEmpty(listRootFolder))
                    {
                        QueryAttachmentsForSingleItem(siteId, webId, listId, listRootFolder, discoverReader, item);
                    }
                }

            });
            return item;
        }

        /// <summary>
        /// 根据DirName,LeafName获取Item/Document的LastModifiedTime(system file查Doc表，其他item查询UD表)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            var item = GetItemIdDocIdLastModifiedTimeFromDocs(siteId, dirName, leafName);
            if (item.ID.HasValue && item.ID.Value > 0)
            {
                item.TimeLastModified = GetItemLastModifiedTime(siteId, listId, item.ID.Value);
            }
            return item.TimeLastModified;
        }

        /// <summary>
        /// 根据Item RowId获取Item/Document的LastModifiedTime,查询AllUserData表
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="rowId">must be above 0</param>
        /// <returns></returns>
        public DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId)
        {
            var result = DateTime.MinValue;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@Id", rowId);
                using (var sr = mQueryWorker.ExecuteReader(GetItemLastModifiedTimeByRowId_Select_AllUserData))
                {
                    if (sr.Read())
                    {
                        result = sr.GetDateTime(0);
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 通过DocId获取Item/Document的LastModifiedTime,AllDocs表
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo", false, "AveDiscoverQueryString.ItemLastModifiedTimeWithDoclibRowId中AllUserData表索引使用不全。")]
        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {

            var result = DateTime.MinValue;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@Id", itemId);
                using (var sr = mQueryWorker.ExecuteReader(GetItemLastModifiedTimeByDocId_Select_AllDocs))
                {
                    if (sr.Read())
                    {
                        result = sr.GetDateTime(0);
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 根据DoclibRowId查找该Item下的所有Versions
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="docLibRowId"></param>
        /// <returns>返回的 AveItemObject 本身属性不全</returns>
        [QueryReview("2012/05/09", "Oliver Luo", true, "AveDiscoverQueryString.ItemVersions中AllUserData表索引使用不全，增加tp_IsCurrentVersion。")]
        public AveItemObject GetItemVersions(Guid siteId, Guid listId, int docLibRowId)
        {
            var item = new AveItemObject { ID = docLibRowId };
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@docLibId", docLibRowId);
                using (var sr = mQueryWorker.ExecuteReader(GetItemVersionsByRowId_Select_AllUserData))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var version = new AveVersionObject
                            {
                                Uiversion = sr.GetInt32(0),
                                TimeLastModified = sr.GetDateTime(1),
                                IsCurrentVersion = sr.GetBoolean(2),
                                UserDataGuid = sr.GetGuid(3),
                                ID = sr.GetInt32(4),
                                UiVersionString = sr.GetString(5),
                                Level = sr.GetByte(6),
                                Size = long.Parse(sr[7].ToString()),
                                Tp_IsCurrentVersion = sr.GetBoolean(8),
                            };
                            item.VersionObjs.Add(version);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from GetItemVersions. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
            return item;
        }

        /// <summary>
        /// 根据tp_Guid去查询Item的DocId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid)
        {
            var docId = Guid.Empty;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@parentId", parentId);
                mQueryWorker.AddParameter("@tp_Guid", tp_Guid);
                var result = mQueryWorker.ExecuteScalar(GetDocIdByParentIdAndGuid_Select_AllUserData);
                if (result != null)
                {
                    docId = (Guid)result;
                }
            });
            return docId;
        }

        /// <summary>
        /// 根据parentId获取Document的tp_Guid-tp_DocId,DocId-type的Mapping
        /// 效率考虑，有API实现
        /// </summary> 
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemsMapping"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid parentId)
        {
            var idAndGUIDMappings = new Dictionary<Guid, Guid>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                using (var sr = mQueryWorker.ExecuteReader(GetItemTpGuidAndIdMapping_Select_AllUserData))
                {
                    while (sr.Read())
                    {
                        idAndGUIDMappings[sr.GetGuid(1)] = sr.GetGuid(0);
                    }
                }
            });
            return idAndGUIDMappings;
        }

        /// <summary>
        /// 根据Leafname去数据库中查询是否有相同记录
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName)
        {
            var hasTheSame = false;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@dirName", dirName);
                mQueryWorker.AddParameter("@LeafName", leafName);
                using (var sr = mQueryWorker.ExecuteReader(GetItemOrVersionCountByLeafname_Select_AllDocs))
                {
                    hasTheSame = sr.Read() && sr.GetInt32(0) > 0;
                }
            });
            return hasTheSame;
        }

        /// <summary>
        /// 根据tp_Guid去查询数据库中是否有相同记录
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="tpGuid"></param>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            var hasTheSame = false;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@tp_Guid", tpGuid);
                using (var sr = mQueryWorker.ExecuteReader(GetItemOrVersionCountByTpGuid_Select_AllUserData))
                {
                    hasTheSame = sr.Read() && sr.GetInt32(0) > 0;
                }
            });
            return hasTheSame;
        }

        /// <summary>
        /// 查询Item上的WebParts
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemDocId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo", true, "AveDiscoverQueryString.ItemWebParts，AllWebParts没有使用索引，增加tp_IsCurrentVersion")]
        public List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            var webParts = new List<AveWebPartObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DocId", itemDocId);
                using (var sr = mQueryWorker.ExecuteReader(GetItemCurrentVersionWebParts_Select_AllWebParts))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var webPart = new AveWebPartObject
                            {
                                Id = sr.GetGuid(0),
                                IsIncluded = (bool)sr.GetValue(5)
                            };
                            if (!sr.IsDBNull(1))
                            {
                                webPart.Flags = (int)sr.GetValue(1);
                            }
                            if (!sr.IsDBNull(2))
                            {
                                webPart.DisplayName = (string)sr.GetValue(2);
                            }
                            if (!sr.IsDBNull(3))
                            {
                                webPart.PartOrder = (int)sr.GetValue(3);
                            }
                            if (!sr.IsDBNull(4))
                            {
                                webPart.ZoneId = (string)sr.GetValue(4);
                            }

                            if (!sr.IsDBNull(6))
                            {
                                webPart.View = (byte[])sr.GetValue(6);
                            }
                            if (!sr.IsDBNull(7))
                            {
                                webPart.AllUsersProperties = (byte[])sr.GetValue(7);
                            }
                            if (!sr.IsDBNull(8))
                            {
                                webPart.PerUserProperties = (byte[])sr.GetValue(8);
                            }
                            webParts.Add(webPart);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from GetItemWebParts. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
            return webParts;
        }


        /// <summary>
        /// 获取Item的size和ModifiedBy，CreatedBy属性
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="docId"></param>
        /// <param name="level"></param>
        /// <param name="createdBy"></param>
        /// <param name="modifiedBy"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public long GetItemSizeAndUserInfo(Guid siteId, Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {
            long size = 0;
            var parentId = Guid.Empty;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DocId", docId);
                mQueryWorker.AddParameter(@"Level", level);

                using (var sr = mQueryWorker.ExecuteReader(GetItemSizeAndParentIdByDocId_Select_AllDocs))
                {
                    if (sr.Read())
                    {
                        var sizeObj = sr["Size"]; //size会出现空的情况，需要判断
                        if (sizeObj != null && sizeObj != DBNull.Value)
                        {
                            size = long.Parse(sizeObj.ToString());
                        }
                        parentId = (Guid)sr["ParentId"];
                    }
                }
            });
            GetCreateByModifiedByUserInfo(siteId, parentId, docId, out createdBy, out modifiedBy);
            return size;
        }

        /// <summary>
        /// 获取Attachments
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listRootUrl"></param>
        /// <param name="itemObj"></param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/12/10", "Austin Han")]
        public void QueryItemAttachment(Guid siteId, string listRootUrl, AveItemObject itemObj, AveDiscoverReader discoverReader)
        {
            ExceptionHandlingScope(() =>
            {
                var attachItemObj = new Dictionary<int, AveItemObject>();
                if (itemObj.ID.HasValue)
                {
                    var itemObjId = (int)itemObj.ID;
                    attachItemObj.Add(itemObjId, itemObj);
                    itemObj.AttachmentObjs.Clear();
                    mQueryWorker.ResetCommand(CommandType.Text);
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@ItemId", itemObj.ID.Value.ToString());
                    mQueryWorker.AddParameter("@AttachmentUrl", listRootUrl + "/" + "Attachments");
                    var queryString = GetSingleItemAttachments_Select_AllDocs(discoverReader);
                    QueryAttachmentForFB(queryString, attachItemObj, discoverReader);
                }
            });
        }

        #endregion for replicator

        #region API Discover extension

        /// <summary>
        /// 获取某个item的所有version的stub信息
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <param name="discoverReader"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "docver")]
        public void SetVersionsStubInfo(List<AveVersionObject> versions, Guid siteId, Guid itemId, AveDiscoverReader discoverReader)
        {

            var commText = GetItemVersionsStubInfo_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams(discoverReader);
            if (string.IsNullOrEmpty(commText))
            {
                return;
            }
            ExceptionHandlingScope(() =>
            {
                var versionsKeyValues = versions.ToDictionary(key => key.Uiversion, value => value);
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@Id", itemId);
                mQueryWorker.AddParameter("@SiteId", siteId);
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            AveVersionObject currentVersionObj;
                            if (versionsKeyValues.TryGetValue((int)sr["UIVersion"], out currentVersionObj))
                            {
                                discoverReader.ReadVersionStubInfo(sr, currentVersionObj);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Error occurred while getting data from SetVersionsStubInfo. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// For Extender,set attachments stubInfo，补全一个Item上所有attachment的stub信息
        /// </summary>
        /// <param name="attachments">同一个Item或folder上的attachment集合</param>
        /// <param name="siteId"></param>
        /// <param name="discoverReader"></param>
        public void SetAttachmentsStubInfo(List<AveItemObject> attachments, Guid siteId, AveDiscoverReader discoverReader)
        {
            var commText = GetSingleItemAttachmentsStubInfo_Select_AllDocs_DocsToStreams_DocStreams(discoverReader);
            //只有Extender模块commText不为空
            if (string.IsNullOrEmpty(commText) || attachments.Count == 0)
            {
                return;
            }
            ExceptionHandlingScope(() =>
            {
                var attachmentsKeyValues = attachments.ToDictionary(key => key.DocID, value => value);
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                //一个Item上所有attachment的DirName都相同，取第一个元素的DirName做为parameter
                mQueryWorker.AddParameter("@DirName", attachments[0].DirName);
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            AveItemObject currentAttachmentObj;
                            if (attachmentsKeyValues.TryGetValue((Guid)sr["Id"], out currentAttachmentObj))
                            {
                                discoverReader.ReadAttachmentStubInfo(sr, currentAttachmentObj);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occurred while getting data from SetAttachmentsStubInfo. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Just for API Discover
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="listObj"></param>
        /// <returns></returns>
        public List<Dictionary<String, Object>> GetCheckoutListItems(AveFolderCache folderCache, AveListObject listObj)
        {
            var checkoutItemInfoList = new List<Dictionary<string, object>>();
            if (folderCache == null || folderCache.SiteId == Guid.Empty || folderCache.WebId == Guid.Empty ||
                folderCache.ListId == Guid.Empty || folderCache.AveSite == null || folderCache.AveWeb == null)
            {
                return checkoutItemInfoList;
            }
            ExceptionHandlingScope(() =>
            {
                var commandSQLString = GetCheckoutItemsInList_Select_AllDocs;
                mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
                mQueryWorker.AddParameter("@WebId", folderCache.WebId);
                mQueryWorker.AddParameter("@ListId", folderCache.ListId);
                using (var sr = mQueryWorker.ExecuteReader(commandSQLString))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var userId = Convert.ToInt32(sr["CheckoutUserId"]);
                            var rowId = Convert.ToInt32(sr["DoclibRowId"]);
                            var itemId = (Guid)sr["Id"];
                            var checkoutItemInfo = new Dictionary<string, object>
                            {
                                {"UserId", userId},
                                {"RowId", rowId},
                                {"ItemId", itemId}
                            };
                            checkoutItemInfoList.Add(checkoutItemInfo);
                        }
                        catch (Exception e)
                        {
                            logger.Error("An error occurred while getting checkout ListItem infos.Error:{0}", e);
                        }
                    }
                }
            });
            return checkoutItemInfoList;
        }

        #endregion API Discover extension

        #region FB

        #region FB private methods

        /// <summary>
        /// 获取某Folder下的Items和Versions信息，包括Attachement.
        /// </summary>
        /// <param name="parentFolderObject"></param>
        /// <param name="attachmentUrl"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        private void GetAllItemsUnderFolderForFB(AveItemObject parentFolderObject, string attachmentUrl, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin,bool includeVersion)
        {
            ExceptionHandlingScope(() =>
            {
                if (parentFolderObject.DocID == Guid.Empty)
                {
                    logger.Log(AveLogLevel.WARN, "parentId should not be null.ParentFolder Url:{0}", parentFolderObject.FullUrl);
                    return;
                }

                mQueryWorker.AddParameter("@ParentId", parentFolderObject.DocID);
                Dictionary<int, AveItemObject> attachments = null;
                if (listObject == null) //System Folder
                {
                    //SiteId,ParentId
                    var queryDocString = GetAllItemsByParentId_Select_AllDocs(includeRecycleBin, true, discoverReader);
                    QueryDocsForFB(queryDocString, parentFolderObject, attachments, null, discoverReader);
                    if (includeVersion)
                    {
                        var queryCommandText = GetWebItemVersionsByParentId_Select_AllUserDataORAllVersions(includeRecycleBin, discoverReader);
                        QueryItemVersions(queryCommandText, parentFolderObject.SubItemObjs.ToDictionary(key => key.DocID, value => value), discoverReader, null);
                        QueryItemVersions(queryCommandText, parentFolderObject.SubFolderObjs.ToDictionary(key => key.DocID, value => value), discoverReader, null);
                    }
                }
                else
                {
                    var enableAttachment = listObject.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
                    if (enableAttachment)
                    {
                        attachments = new Dictionary<int, AveItemObject>();
                    }
                    //查alldoc 表中记录，当前version，checkout，publish 
                    var queryDocString = GetAllItemsByParentId_Select_AllDocs(includeRecycleBin, false, discoverReader);
                    QueryDocsForFB(queryDocString, parentFolderObject, attachments, listObject, discoverReader);
                    if (includeVersion)
                    {
                        var queryCommandText = GetListItemVersionsByParentId_Select_AllUserDataORAllVersions(includeRecycleBin, discoverReader);
                        QueryItemVersions(queryCommandText, parentFolderObject.SubItemObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject);
                        QueryItemVersions(queryCommandText, parentFolderObject.SubFolderObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject);
                    }
                    if (!string.IsNullOrEmpty(attachmentUrl) && enableAttachment && attachments.Count > 0)
                    {
                        mQueryWorker.AddParameter("@AttachmentUrl", attachmentUrl);
                        //@SiteId,@AttachmentUrl
                        var queryAttachmentString = GetAllItemAttachments_Select_AllDocs(includeRecycleBin, discoverReader);
                        QueryAttachmentForFB(queryAttachmentString, attachments, discoverReader);
                    }
                }
            });
        }

        /// <summary>
        /// 查询特定parentId下的所有Items and Versions（包括Attachments)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="commText">parameter 在调用处添加</param>
        /// <param name="parentFolderObject"></param>
        /// <param name="attachmentItems"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/12/11", "Austin Han", false, "在调用方法中Review")]
        private void QueryDocsForFB(string commText, AveItemObject parentFolderObject, IDictionary<int, AveItemObject> attachmentItems, AveListObject listObject, IAveDiscoverReader discoverReader)
        {
            ExceptionHandlingScope(() =>
            {
                AveItemObject previousItem = null;
                var lastItemId = Guid.Empty;
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var leafName = (string)sr["LeafName"];
                            var docLibRowId = sr["DoclibRowId"] is DBNull ? null : (int?)sr["DoclibRowId"];
                            var docId = (Guid)sr["Id"];
                            var type = (byte)sr["Type"];
                            var deleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId");
                            if (!docLibRowId.HasValue && string.Compare(leafName, "Attachments", StringComparison.OrdinalIgnoreCase) == 0
                                || listObject == null && discoverReader.IsUnusedFolder(leafName, true))
                            {
                                continue;
                            }
                            if (!docId.Equals(lastItemId) || previousItem == null)
                            {
                                var queryData = new AveItemObject { DirName = parentFolderObject.FullUrl };
                                discoverReader.ReadItemContent(queryData, sr);
                                //Folder
                                if (type == 1)
                                {
                                    parentFolderObject.SubFolderObjs.Add(queryData);
                                    queryData.ObjType = ItemType.Folder;
                                }
                                else
                                {
                                    if (listObject != null && listObject.Type != DocList && docLibRowId.HasValue)
                                    {
                                        queryData.ObjType = ItemType.Item;
                                    }
                                    else
                                    {
                                        queryData.ObjType = ItemType.Document;
                                    }
                                    parentFolderObject.SubItemObjs.Add(queryData);
                                }
                                //需要在之前reader 中赋值属性后再进行处理

                                //对于root site collection,一些parentFolder的FullUrl为empty. DirName为empty,调用CombineUrl方法会抛异常
                                queryData.FullUrl = queryData.DirName.Length > 0 ? AveUrlUtility.CombineUrl(queryData.DirName, queryData.LeafName) : queryData.LeafName;
                                //此处先将Item 的对象加入集合，之后会将attachment 放入item 对象的属性上
                                if (docLibRowId.HasValue)
                                {
                                    var subId = docLibRowId.Value;
                                    attachmentItems?.Add(subId, queryData);
                                }
                                queryData.DeleteTransactionId = deleteTransactionId;
                                previousItem = queryData;
                                lastItemId = docId;
                            }
                            //Item 本身也要在version 集合中存在,之前外围需要。此处可以商议是否去掉，暂时不影响效率，不做修改。
                            var version = new AveVersionObject();
                            discoverReader.ReadVersionContent(version, sr);
                            AddVersion(version, previousItem, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occurred while getting data from QueryDocsForFB. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 查询folder下stub 文件数量(包括item上的attachment)
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="parentFolder"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The Wrong words are the part of sql statement. ")]
        [QueryReview("2012/05/22", "Oliver Luo")]
        private void GetStubItem(Guid parentId, AveItemObject parentFolder, AveListObject listObject, IAveDiscoverReader discoverReader, bool includeRecycleBin)
        {
            if (parentId == Guid.Empty)
            {
                logger.Log(AveLogLevel.WARN, "GetStubItem parentId should not be null.ParentFolder Url:{0}", parentFolder.FullUrl);
                return;
            }
            mQueryWorker.AddParameter("@ParentId", parentId);

            Dictionary<int, AveItemObject> itemEntities = null;
            if (listObject == null) //System Folder
            {
                var queryDocString = GetStubAllItemAndVersions_Select_AllDocs_DocsToStreams_DocStreams(includeRecycleBin, true);
                QueryDocsForFB(queryDocString, parentFolder, itemEntities, null, discoverReader);
            }
            else
            {
                var enableAttachment = DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
                if (enableAttachment)
                {
                    itemEntities = new Dictionary<int, AveItemObject>();
                }
                var queryDocString = GetStubAllItemAndVersions_Select_AllDocs_DocsToStreams_DocStreams(includeRecycleBin, false);
                QueryDocsForFB(queryDocString, parentFolder, itemEntities, listObject, discoverReader);
            }
        }

        /// <summary>
        /// 查询folder中stub file数量
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <returns></returns>
        private int GetStubAttachmentCountByParentId(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, bool includeRecycleBin)
        {
            var stubAttachmentNum = 0;
            if (listObject == null)
            {
                return stubAttachmentNum;
            }
            var enableAttachment = listObject.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
            if (enableAttachment)
            {
                ExceptionHandlingScope(() =>
                {
                    var attachmentDir = listObject.RootFolderUrl + '/' + "Attachments/";
                    mQueryWorker.ResetCommand(CommandType.Text);
                    mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
                    mQueryWorker.AddParameter("@WebId", folderCache.WebId);
                    mQueryWorker.AddParameter("@ListId", folderCache.ListId);
                    mQueryWorker.AddParameter("@ParentId", folderObject.DocID);
                    mQueryWorker.AddParameter("@AttachmentDir", attachmentDir);
                    var command = includeRecycleBin ? GetItemStubAttachmentsCountInFolderWithRecycleBin_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams
                                                    : GetItemStubAttachmentsCountInFolder_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams;
                    stubAttachmentNum = (int)mQueryWorker.ExecuteScalar(command);
                });
            }
            return stubAttachmentNum;
        }

        /// <summary>
        /// 查询folder中item的stub attachment的数量
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <returns></returns>
        private int GetStubFileCountByParentId(AveFolderCache folderCache, AveItemObject folderObject, bool includeRecycleBin)
        {
            var stubFileNum = 0;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
                mQueryWorker.AddParameter("@WebId", folderCache.WebId);
                mQueryWorker.AddParameter("@ListId", folderCache.ListId);
                mQueryWorker.AddParameter("@ParentId", folderObject.DocID);
                var command = includeRecycleBin ? GetStubFilesCountInFolderWithRecycleBin_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams
                                         : GetStubFilesCountInFolder_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams;
                stubFileNum = (int)mQueryWorker.ExecuteScalar(command);
            });
            return stubFileNum;
        }

        private string GetWebUrlById(Guid siteId, Guid webId)
        {
            var webUrl = "";
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                using (var sr = mQueryWorker.ExecuteReader(GetWebUrlById_Select_AllWebs))
                {
                    if (sr.Read())
                    {
                        webUrl = sr.IsDBNull(0) ? string.Empty : sr.GetString(0);
                    }
                }
            });
            return webUrl;
        }

        /// <summary>
        /// 根据web url查询web下的所有ContentType
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webUrl"></param>
        /// <returns></returns>
        private Dictionary<byte[], AveContentTypeObject> GetWebContentTypesByUrl(Guid siteId, string webUrl)
        {
            var contentTypes = new Dictionary<byte[], AveContentTypeObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebFullUrl", webUrl);
                using (var sr = mQueryWorker.ExecuteReader(GetWebContentTypesByWebUrl_Select_ContentTypes))
                {
                    while (sr.Read())
                    {
                        var contentType = new AveContentTypeObject
                        {
                            ContentTypeId = sr.GetValue(0) as byte[],
                            SchemaXml = sr.IsDBNull(1) ? string.Empty : sr.GetString(1),
                            Name = sr.IsDBNull(1) ? string.Empty : sr.GetString(2),
                            Scope = sr.GetString(3)
                        };
                        contentTypes.Add((byte[])sr["ContentTypeId"], contentType);
                    }
                }
            });
            return contentTypes;
        }

        #endregion

        /// <summary>
        /// 获取Site下的所有web信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId)
        {
            var webObjs = new Dictionary<Guid, AveWebObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);

                var rootWebUrlLength = -1;
                using (var sr = mQueryWorker.ExecuteReader(GetAllWebsBySiteId_Select_AllWebs))
                {
                    while (sr.Read())
                    {
                        AveWebObject web = null;
                        try
                        {
                            web = AveDiscoverSqlUtility.GetWebInfoByDataReader(sr, rootWebUrlLength);
                            if (web != null)
                            {
                                webObjs.Add(web.WebID, web);
                                if (rootWebUrlLength < 0)
                                {
                                    //only the first web will init len(root web)
                                    rootWebUrlLength = web.FullUrl.Length;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Error occur while access data from method QuerySiteWebForFB. SiteId:{0}. WebId:{1}. ErrorMessage:{2}", siteId, web?.WebID ?? Guid.Empty, e);
                        }

                    }
                }
            });
            return webObjs;
        }

        /// <summary>
        /// 获取Site的RootWeb信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public AveWebObject QueryRootWeb(Guid siteId)
        {

            AveWebObject rootWebObj = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                using (var sr = mQueryWorker.ExecuteReader(GetRootWebBySiteId_Select_AllWebs))
                {
                    if (sr.Read())
                    {
                        var webId = sr.GetGuid(0);
                        try
                        {
                            rootWebObj = new AveWebObject
                            {
                                WebID = webId,
                                Name = ".",
                                FullUrl = sr.GetString(1),
                                Title = sr.IsDBNull(2) ? string.Empty : sr.GetString(2),
                                DeleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId")
                            };

                        }
                        catch (Exception e)
                        {
                            logger.Warn("Error occur while access data from method QueryRootWeb.SiteId:{0}. WebId:{1}. ErrorMessage:{2}", siteId, webId, e);
                        }
                    }
                }
            });
            return rootWebObj;
        }

        /// <summary>
        /// 根据ParentWebId获取sub webs
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentWebId"></param>
        /// <param name="includeRecycleBin">FOR SO</param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin)
        {
            var subWebObjs = new Dictionary<Guid, AveWebObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentWebId);
                var rootWebUrlLength = -1;
                var command = GetSubWebsByParentWebId_Select_AllWebs(includeRecycleBin);
                using (var sr = mQueryWorker.ExecuteReader(command))
                {
                    while (sr.Read())
                    {
                        AveWebObject web = null;
                        try
                        {
                            web = AveDiscoverSqlUtility.GetWebInfoByDataReader(sr, rootWebUrlLength);
                            if (web != null)
                            {
                                if (!string.Equals(web.Name, ".", StringComparison.OrdinalIgnoreCase))
                                {
                                    subWebObjs.Add(web.WebID, web);
                                }
                                else
                                {
                                    //only the first web will init len(root web)
                                    rootWebUrlLength = web.FullUrl.Length;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Exception occur while access data from QueryWebRootFolder. SiteId:{0}. ParentWebId:{1}. CurrentWebId:{2}. ErrorMessage:{3}", siteId, parentWebId, web?.WebID ?? Guid.Empty, e);
                        }
                    }
                }
            });
            return subWebObjs;
        }

        /// <summary>
        /// 获取Web下的所有Lists信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="includeRecycleBin"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han", true, "Add SiteId to improve performance")]
        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool includeRecycleBin)
        {
            var listObjs = new Dictionary<Guid, AveListObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);

                var command = GetAllListsInWeb_Select_AllLists(includeRecycleBin);
                using (var sr = mQueryWorker.ExecuteReader(command))
                {
                    while (sr.Read())
                    {
                        var listId = sr.GetGuid(0);
                        try
                        {
                            if (!listObjs.ContainsKey(listId))
                            {
                                var listObj = AveDiscoverSqlUtility.GetListInfoByDataReader(sr);
                                listObjs.Add(listId, listObj);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from query QueryWebListForFB.SiteId:{0}. WebId:{1}. ListId:{2} ErrorMessage:{3}", siteId, webId, listId, e);
                        }
                    }
                }
            });
            return listObjs;
        }

        /// <summary>
        /// 获取List下的所有Views信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            var views = new Dictionary<Guid, AveViewObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@listId", listId);
                using (var sr = mQueryWorker.ExecuteReader(GetViewsByListId_Select_AllWebParts))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var view = new AveViewObject();
                            DiscoverUtility.FillWebPartDicFromAllWebParts(view, sr);
                            views.Add(view.ViewID, view);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListViewForFB.SiteId:{0}. WebId:{1}. ListId:{2} ErrorMessage:{3}", siteId, webId, listId, e);
                        }
                    }
                }
            });
            return views;
        }
        
        /// <summary>
        /// 获取某Folder下的Items和Versions信息，包括Attachement.
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        [QueryReview("2012/12/10", "Austin Han", false, "在GetListItem中Review")]
        public void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin, bool includeVersion)
        {
            var attachmentUrl = string.Empty;
            if (listObject != null)
            {
                if (!string.IsNullOrEmpty(listObject.RootFolderUrl))
                {
                    attachmentUrl = listObject.RootFolderUrl + '/' + "Attachments";
                }
                else
                {
                    logger.Log(AveLogLevel.WARN, "Current List should have RootFolderUrl. ListId:{0}", folderCache.ListId);
                }
            }
            mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
            mQueryWorker.AddParameter("@ListId", folderCache.ListId);
            GetAllItemsUnderFolderForFB(folderObject, attachmentUrl, listObject, discoverReader, includeRecycleBin, includeVersion);
        }

        /// <summary>
        /// 查询某folder下的stub Item信息
        /// 无API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        [QueryReview("2012/05/10", "Oliver Luo", false, "在GetStubItem中Review")]
        public void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin)
        {
            if (listObject != null)
            {
                if (string.IsNullOrEmpty(listObject.RootFolderUrl))
                {
                    logger.Log(AveLogLevel.WARN, "Current List should have RootFolderUrl.Current folder DocId:{0}. Url:{1}", folderObject.DocID, folderObject.FullUrl);
                }
            }
            mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
            mQueryWorker.AddParameter("@ListId", folderCache.ListId);
            GetStubItem(folderObject.DocID, folderObject, listObject, discoverReader, includeRecycleBin);
        }

        /// <summary>
        /// 获取ParentId下所有Stub Item数量(包括AllDocs表中和AllDocVersions表中)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HistVersion is the parameter of the sql statement. ")]
        [QueryReview("2012/05/10", "Oliver Luo")]
        public int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, bool includeRecycleBin = false)
        {
            var stubFileNum = GetStubFileCountByParentId(folderCache, folderObject,includeRecycleBin);
            var stubAttachmentNum = GetStubAttachmentCountByParentId(folderCache, folderObject, listObject,includeRecycleBin);
            return stubFileNum + stubAttachmentNum;
        }

        /// <summary>
        /// 查询Web下的所有ContentTypes信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han", false, "Add an overload method to improve the performance.")]
        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            var webUrl = GetWebUrlById(siteId, webId);
            return GetWebContentTypesByUrl(siteId, webUrl);
        }

        /// <summary>
        /// 查询Web下的所有ContentTypes信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="serverRelativeUrl"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl)
        {
            return GetWebContentTypesByUrl(siteId, serverRelativeUrl);
        }

        #endregion FB

        #region IB Private methods

        /// <summary>
        /// 根据Query语句条件批量查询EventCache表change 记录，batch size 2000 as default
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="isSystemFolder"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private List<EventObject> GetAllEventsObject(AveFolderCache folderCache, bool isSystemFolder, DateTime startTime, DateTime endTime)
        {
            var command = GetListItemsForIB_Select_EventCache(isSystemFolder);
            var allEvents = new List<EventObject>();
            List<EventObject> tempEvents;
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
            mQueryWorker.AddParameter("@WebId", folderCache.WebId);
            if (!isSystemFolder)
            {
                mQueryWorker.AddParameter("@ListId", folderCache.ListId);
            }
            mQueryWorker.AddParameter("@endTime", endTime);
            mQueryWorker.AddParameter("@startTime", startTime);
            do
            {
                tempEvents = AveQueryUtility.GetDBRows<EventObject>(mQueryWorker, command, string.Empty);
                if (tempEvents == null)
                {
                    break;
                }
                if (tempEvents.Count > 0)
                {
                    mQueryWorker.AddParameter("@startTime", tempEvents[tempEvents.Count - 1].EventTime);
                    allEvents.AddRange(tempEvents);
                }
            } while (tempEvents.Count == AveWrapperConstants.MaxRows);
            return allEvents;
        }

        /// <summary>
        /// @SiteId 根据event cache查询的结果，查询对应的doc表数据
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="allEvents"></param>
        /// <param name="IsSystemDocs">True system list,会过滤掉listId不为空的数据</param>
        /// <returns></returns>
        private Dictionary<Guid, DocObject> GetAllDocsInfos(Guid siteId, List<EventObject> allEvents, AveDiscoverReader discoverReader,bool isSystemFolder)
        {
            var index = 0;
            var tempDocs = new List<DocObject>();
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            while (index < allEvents.Count)
            {
                var sb = new StringBuilder();
                var tempids = new List<Guid>();
                do
                {
                    var eventObj = allEvents[index++];
                    if (eventObj.DocId == Guid.Empty)
                    {
                        continue;
                    }
                    if (!tempids.Contains(eventObj.DocId))
                    {
                        tempids.Add(eventObj.DocId);
                    }
                } while (index < allEvents.Count && tempids.Count < 800);
                if (tempids.Count > 0) //有需要在Alldoc 表中查询的数据, view,webpart 等不需要再alldoc 中查询
                {
                    var command = GetDocInfoByIdsBatch_Select_AllDocs(tempids, discoverReader,isSystemFolder);
                    var queryResults = AveQueryUtility.GetDBRows<DocObject>(mQueryWorker, command, string.Empty);
                    if (queryResults != null)
                    {
                        tempDocs.AddRange(queryResults);
                    }
                }
            }
            return tempDocs.Distinct().ToDictionary(k => k.Id, v => v);
        }

        /// <summary>
        /// 为ExtraItems补全Attachment信息
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="extraItems"></param>
        private void AddAttachmentsGuidToExtraItems(AveFolderCache folderCache, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            List<Guid> attachmentCollection = new List<Guid>();
            for (int i = 0; i < extraItems.Count; i++)
            {
                var attachments = GetItemAttachments(folderCache.SiteId, folderCache.ListId, extraItems[i].Id);
                attachmentCollection.AddRange(attachments);
            }
            attachmentCollection.ForEach(delegate (Guid attachmentGuid)
            {
                if (!extraItems.Exists(itemBaseInfo => itemBaseInfo.Id == attachmentGuid))
                {
                    extraItems.Add(new AveDiscoverExtraItemBaseInfo() { Id = attachmentGuid, ObjectType = ChangeObjectType.File });
                }
            });
        }

        private List<Guid> GetItemAttachments(Guid siteId, Guid listId, Guid itemDocId)
        {
            List<Guid> attachments = new List<Guid>();
            ExceptionHandlingScope(() =>
            {
                string attachmentFolderUrl = GetAttachmentFolderUrl(itemDocId, listId, siteId);
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@AttachmentDirName", attachmentFolderUrl);
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(GetAttachmentsByParentFolderUrl_Select_AllDocs))
                {
                    while (sr.Read())
                    {
                        attachments.Add(sr.GetGuid(0));
                    }
                }
            });
            return attachments;
        }

        private Tuple<string, string> GetItemDirNameAndRowIdByDocId(Guid siteId, Guid extraItemId)
        {
            Tuple<string, string> dirNameAndRowId = null;
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ItemId", extraItemId);
            //@SiteId @ItemId
            using (var sr = mQueryWorker.ExecuteReader(GetItemDirNameAndRowIdByDocId_Select_AllDocs))
            {
                while (sr.Read())
                {
                    var itemDirName = sr[0].ToString();
                    var itemDoclibRowId = sr[1].ToString();
                    dirNameAndRowId = new Tuple<string, string>(itemDirName, itemDoclibRowId);
                }
            }
            return dirNameAndRowId;
        }

        private string GetAttachmentFolderUrl(Guid itemDocId, Guid listId, Guid siteId)
        {
            var dirNameAndRowId = GetItemDirNameAndRowIdByDocId(siteId, itemDocId);
            string listRootFolderUrl = GetListRootFolderUrl(listId, siteId);
            string itemDirName = string.IsNullOrEmpty(listRootFolderUrl) ? dirNameAndRowId.Item1 : listRootFolderUrl;
            return string.Format("{0}/Attachments/{1}", itemDirName, dirNameAndRowId.Item2);
        }

        private string GetListRootFolderUrl(Guid listId, Guid siteId)
        {
            string listRootFolderUrl = "";
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            using (SqlDataReader sr = mQueryWorker.ExecuteReader(GetListUrlById_Select_AllDocs))
            {
                while (sr.Read())
                {
                    listRootFolderUrl = sr.GetString(0) + "/" + sr.GetString(1);
                }
            }
            return listRootFolderUrl;
        }

        private void QueryListItemsForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems, bool isSystemFolder)
        {
            var allEvents = GetAllEventsObject(folderCache, isSystemFolder, startTime, endTime);
            var allDocs = GetAllDocsInfos(folderCache.SiteId, allEvents, discoverReader, isSystemFolder);
            var enableAttachment = listObject?.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
            //此处逻辑为：当extraItems的Count大于0，同时enableAttachment的时候进行添加attachment操作。但是在list flag ==null的情况下不考虑enableAttachment，都进行添加操作以防丢失数据。
            if (extraItems != null && extraItems.Count > 0 && enableAttachment)
            {
                AddAttachmentsGuidToExtraItems(folderCache, extraItems);
            }
            ItemChanged(allEvents, allDocs, folderObject, folderCache, listObject, discoverReader, noPropertyFolders, extraItems);
        }

        #region item change

        /// <summary>
        /// 补全传入的item的doc信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        [AveQueryServiceBase.QueryReview("2012/12/11", "Austin Han")]
        private List<Dictionary<string, object>> GetItemsDocsInfo(Guid siteId, List<AveDiscoverExtraItemBaseInfo> items)
        {
            var results = new List<Dictionary<string, object>>();
            if (items == null || items.Count == 0)
            {
                return results;
            }
            var cmdText = new StringBuilder(AveDiscoverQueryString.ItemChangedByCustomItems13);
            for (var i = 0; i < items.Count; i++)
            {
                cmdText.Append("'");
                cmdText.Append(items[i].Id);
                cmdText.Append("' ,");
                if (cmdText.Length > 40960 || i == items.Count - 1)
                {
                    --cmdText.Length;
                    cmdText.Append(')'); //去除最后的逗号
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    using (var sr = mQueryWorker.ExecuteReader(cmdText.ToString()))
                    {
                        var result = AveSqlUtility.GetDBRows(sr, true);
                        if (result != null)
                        {
                            results.AddRange(result);
                        }
                    }
                    cmdText.Length = AveDiscoverQueryString.ItemChangedByCustomItems13.Length;
                }
            }
            return results;
        }

        /// <summary>
        /// 查询view信息
        /// </summary>
        /// <param name="systemItems"></param>
        /// <param name="views"></param>
        /// <param name="rootFolder"></param>
        /// <param name="siteId"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        [QueryReview("2012/12/11", "Austin Han", true, "Add SiteId in the where condition to improve the performance")]
        private void QueryViewItemsForIB(Dictionary<Guid, AveItemObject> systemItems, Dictionary<Guid, EventObject> views, AveItemObject rootFolder, Guid siteId, IAveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            var docIds = new Dictionary<Guid, EventObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                var index = 0;
                var keys = new List<Guid>(views.Keys);
                var viewDocIds = new List<Guid>();
                while (index < keys.Count)
                {
                    for (var idCount = 0; index < keys.Count && idCount < 800; ++idCount)
                    {
                        var key = keys[index++];
                        var changeType = DiscoverUtility.GetChangeType((NativeChangeType)views[key].EventType);
                        if (changeType == ChangeType.Edit)
                        {
                            viewDocIds.Add(key);
                        }
                    }
                    if (viewDocIds.Count > 0)
                    {
                        var command = GetViewInfosByIds_Select_AllWebParts(viewDocIds);
                        using (var sr = mQueryWorker.ExecuteReader(command))
                        {
                            while (sr.Read())
                            {
                                var id = (Guid)sr["tp_PageUrlID"];
                                var viewId = (Guid)sr["tp_ID"];
                                if (!systemItems.ContainsKey(id))
                                {
                                    docIds[id] = views[viewId];
                                }
                                else
                                {
                                    if (systemItems[id].EventTime < views[viewId].EventTime)
                                    {
                                        systemItems[id].EventTime = views[viewId].EventTime;
                                    }
                                }
                            }
                        }
                    }
                }
            });
            QueryViewInDocsWithDocId(docIds, rootFolder, discoverReader, noPropertyFolders);
        }

        /// <summary>
        /// 查询view的docInfo
        /// </summary>
        /// <param name="docIds"></param>
        /// <param name="rootFolder"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        private void QueryViewInDocsWithDocId(Dictionary<Guid, EventObject> docIds, AveItemObject rootFolder, IAveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            ExceptionHandlingScope(() =>
            {
                var index = 0;
                var ids = docIds.Keys.ToList();
                while (index < ids.Count)
                {
                    var queryDocIds = new List<Guid>();
                    for (var idCount = 0; index < ids.Count && idCount < 800; ++idCount)
                    {
                        queryDocIds.Add(ids[index++]);
                    }
                    if (queryDocIds.Count > 0)
                    {
                        var commText = GetViewDocInfoByIds_Select_AllDocs(queryDocIds, discoverReader);
                        using (var sr = mQueryWorker.ExecuteReader(commText))
                        {
                            while (sr.Read())
                            {
                                var dirName = (string)sr["DirName"];
                                var leafName = (string)sr["LeafName"];
                                var docId = (Guid)sr["Id"];
                                var parentFolder = discoverCommon.GetParentFolder(dirName, rootFolder, noPropertyFolders);
                                if (parentFolder == null)
                                {
                                    continue;
                                }
                                var item = new AveItemObject
                                {
                                    DirName = dirName,
                                    FullUrl = (dirName + '/' + leafName).Trim('/'),
                                    EventTime = docIds[docId].EventTime,
                                    ChangeType = ChangeType.Edit,
                                    ObjType = ItemType.Document
                                };
                                discoverReader.ReadItemContent(item, sr);
                                parentFolder.SubItemObjs.Add(item);
                            }
                        }
                    }
                }
            });
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryItemsAlerts(Dictionary<int, AveItemObject> itemAlerts, AveItemObject rootFolder, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            if (itemAlerts.Count <= 0)
            {
                return;
            }
            var items = QueryItemsAlertsInDocs(itemAlerts, rootFolder, listObject, discoverReader, noPropertyFolders);
            if (items != null && items.Count > 0)
            {
                var itemDocIds = items.ConvertAll(item => item.DocID);
                var allUserDataQueryCommandText = GetItemVersionsInUDByDocId_Select_AllUserData(discoverReader, itemDocIds);
                QueryItemVersions(allUserDataQueryCommandText, items.ToDictionary(key => key.DocID, value => value), discoverReader, listObject);
            }
        }

        private List<AveItemObject> QueryItemsAlertsInDocs(Dictionary<int, AveItemObject> itemAlerts, AveItemObject rootFolder, AveListObject listObject, IAveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            //记录查出来的Item，用来查找对应的Version和UserData信息。
            var items = new List<AveItemObject>();
            ExceptionHandlingScope(() =>
            {
                AveItemObject previousItem = null;
                var lastItemId = Guid.Empty;
                var queryString = GetItemDocInfosByItemIdInAlert_Select_AllDocs(itemAlerts.Keys.ToList(), rootFolder.FullUrl, discoverReader);
                using (var sr = mQueryWorker.ExecuteReader(queryString))
                {
                    while (sr.Read())
                    {
                        var dirName = (string)sr["DirName"];
                        var leafName = (string)sr["LeafName"];
                        var fullName = (dirName + '/' + leafName).Trim('/');
                        var docLibRowId = sr["DoclibRowId"] is DBNull ? null : (int?)sr["DoclibRowId"];
                        var docId = (Guid)sr["Id"];
                        try
                        {
                            if (!docId.Equals(lastItemId) || previousItem == null)
                            {
                                AveItemObject parentFolder;
                                if ((parentFolder = discoverCommon.GetParentFolder(dirName, rootFolder, noPropertyFolders)) == null)
                                {
                                    continue;
                                }

                                if ((byte)sr["Type"] == 1)
                                {
                                    logger.Debug("It is not a item alert. Url: {0}", fullName);
                                    continue;
                                }

                                var item = new AveItemObject
                                {
                                    ParentID = (Guid)sr["ParentId"],
                                    DirName = dirName
                                };
                                discoverReader.ReadItemContent(item, sr);
                                item.FullUrl = (item.DirName + "/" + item.LeafName).Trim('/');
                                if (listObject.Type != DocList && docLibRowId.HasValue)
                                {
                                    item.ObjType = ItemType.Item;
                                }
                                else
                                {
                                    item.ObjType = ItemType.Document;
                                }
                                AveItemObject alertItem;
                                if (itemAlerts.TryGetValue(docLibRowId.Value, out alertItem))
                                {
                                    item.AlertObjs = alertItem.AlertObjs;
                                }
                                item.ChangeType = ChangeType.Edit;
                                parentFolder.SubItemObjs.Add(item);
                                items.Add(item);

                                previousItem = item;
                                lastItemId = item.DocID;
                            }
                            var version = new AveVersionObject();
                            discoverReader.ReadVersionContent(version, sr);
                            AddVersion(version, previousItem, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemsAlertsInDocs. DirName:{0}. LeafName:{1}. Id:{2}. ErrorMessage:{3}",
                                dirName, leafName, docId, e);
                        }
                    }
                }
            });
            return items;
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryFoldersAlerts(Dictionary<Guid, AveAlertObject> folderAlerts, AveItemObject rootFolder, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            if (folderAlerts.Count <= 0)
            {
                return;
            }

            ExceptionHandlingScope(() =>
            {
                var commandText = GetFolderAlertsByIds_Select_ImmedSubscriptions_SchedSubscriptions(folderAlerts.Keys.ToList());
                using (var sr = mQueryWorker.ExecuteReader(commandText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var alertId = sr.GetGuid(0);
                            var properties = sr.IsDBNull(1) ? string.Empty : sr.GetString(1);
                            var folderFullUrl = discoverCommon.GetFileFilter(properties);
                            if (string.IsNullOrEmpty(folderFullUrl))
                            {
                                continue;
                            }
                            var dir = folderFullUrl.Substring(0, folderFullUrl.LastIndexOf('/'));

                            AveItemObject parent;
                            if ((parent = discoverCommon.GetParentFolder(dir, rootFolder, noPropertyFolders)) == null)
                            {
                                return;
                            }

                            var folder = discoverCommon.GetCurrentFolder(parent, folderFullUrl, true, noPropertyFolders);

                            folder.AlertObjs = new Dictionary<Guid, AveAlertObject>();
                            folder.FullUrl = folderFullUrl;
                            folder.LeafName = folderFullUrl.Substring(folderFullUrl.LastIndexOf('/') + 1);
                            if (!noPropertyFolders.ContainsKey(folderFullUrl))
                            {
                                noPropertyFolders.Add(folderFullUrl, folder);
                            }

                            if (!folder.AlertObjs.ContainsKey(alertId))
                            {
                                var changeType = folderAlerts[alertId].ChangeType;
                                var alertObject = new AveAlertObject
                                {
                                    Id = alertId,
                                    ChangeType = changeType
                                };
                                folder.AlertObjs.Add(alertId, alertObject);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryFoldersAlerts. ErrorMessage:{0}", e);
                        }
                    }
                }
            });
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DocId is a part of Keys")]
        private void AddVersionToItems(Dictionary<int, AveItemObject> items, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            if (items.Count <= 0)
            {
                return;
            }

            bool isSpecialLibrary = listObject != null && listObject.MaxMajorwithMinorVersionCount.HasValue && listObject.Type == 1;

            var allItems = items.Select(item => item.Value.DocID).ToArray();
            int index = 0;
            while (index < allItems.Length)
            {
                List<Guid> queryItemDocIds = new List<Guid>();
                //SQL command text limited 64k
                for (var idCount = 0; idCount < 800; ++idCount)
                {
                    queryItemDocIds.Add(allItems[index++]);
                    if (index >= allItems.Length)
                    {
                        break;
                    }
                }
                if (isSpecialLibrary && !(discoverReader is AveExtenderDiscoverReader))
                {
                    AddVersionToItemsForSpecialLibrary(items, discoverReader, queryItemDocIds);
                }
                else
                {
                    AddVersionToItemsForNormal(items, discoverReader, queryItemDocIds);
                }
            }
        }

        private void AddVersionToItemsForNormal(Dictionary<int, AveItemObject> items, AveDiscoverReader discoverReader, List<Guid> queryItemDocIds)
        {
            AveItemObject item = null;
            var tempCommand = AveQueryUtility.GetAllDocVersionsUserData_Select_AllUserData_AllDocs_AllDocVersions(queryItemDocIds, discoverReader);
            using (var sr = mQueryWorker.ExecuteReader(tempCommand))
            {
                while (sr.Read())
                {
                    try
                    {
                        var docId = (Guid)sr["tp_DocId"];
                        var id = (int)sr["DoclibRowId"];
                        if (item == null || item.DocID != docId)
                        {
                            item = items[id];
                        }
                        var version = new AveVersionObject();
                        discoverReader.ReadVersionContentWithDeleteState(version, sr);
                        AddVersion(version, item, sr, discoverReader);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "Error occur while access data from AddVersionToItems. ErrorMessage:{0}", e);
                    }
                }
            }
        }

        private void AddVersionToItemsForSpecialLibrary(Dictionary<int, AveItemObject> items, AveDiscoverReader discoverReader, List<Guid> queryItemDocIds)
        {
            AveItemObject item = null;
            var allDocVersionsCache = new Dictionary<Guid, List<int>>();
            var tempAllDocCommand = AveQueryUtility.GetAllDocVersionsForSpecialLibrary_Select_AllDocVersions(queryItemDocIds);
            using (var sr = mQueryWorker.ExecuteReader(tempAllDocCommand))
            {
                while (sr.Read())
                {
                    var allDocVersionId = (Guid)sr["Id"];
                    var uiVersion = (int)sr["UIVersion"];
                    if (allDocVersionsCache.ContainsKey(allDocVersionId))
                    {
                        allDocVersionsCache[allDocVersionId].Add(uiVersion);
                    }
                    else
                    {
                        var uiList = new List<int> { uiVersion };
                        allDocVersionsCache.Add(allDocVersionId, uiList);
                    }
                }
            }
            var tempAllUserDataCommand = AveQueryUtility.GetAllDocVersionsUserData_Select_AllUserData_AllDocs_AllDocVersions(queryItemDocIds, discoverReader);
            using (var sr = mQueryWorker.ExecuteReader(tempAllUserDataCommand))
            {
                while (sr.Read())
                {
                    var audDocId = (Guid)sr["tp_DocId"];
                    var audCalculatedVersion = Convert.ToInt32(sr["tp_CalculatedVersion"]);
                    var audTPCurrentVersion = Convert.ToInt32(sr["tp_IsCurrentVersion"]);
                    if (audTPCurrentVersion == 1 || (allDocVersionsCache.ContainsKey(audDocId) && allDocVersionsCache[audDocId].Exists(uiversion => uiversion == audCalculatedVersion)))
                    {
                        try
                        {
                            var docId = (Guid)sr["tp_DocId"];
                            var id = (int)sr["DoclibRowId"];
                            if (item == null || item.DocID != docId)
                            {
                                item = items[id];
                            }
                            var version = new AveVersionObject();
                            discoverReader.ReadVersionContentWithDeleteState(version, sr);
                            AddVersion(version, item, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from AddVersionToItems. ErrorMessage:{0}", e);
                        }
                    }
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void DoLastCache(Dictionary<Guid, AveItemObject> systemItems, Dictionary<Guid, EventObject> views, Dictionary<int, AveItemObject> itemAlerts, Dictionary<Guid, AveAlertObject> folderAlerts, Dictionary<int, AveItemObject> items, Dictionary<int, List<AveItemObject>> attachments, AveItemObject rootFolder, AveListObject listObject, AveFolderCache folderCache, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            try
            {
                QueryViewItemsForIB(systemItems, views, rootFolder, folderCache.SiteId, discoverReader, noPropertyFolders);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryViewItemsForIB. ErrorMessage:{0}", e);
            }

            try
            {
                QueryItemsAlerts(itemAlerts, rootFolder, listObject, discoverReader, noPropertyFolders);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryItemsAlerts. ErrorMessage:{0}", e);
            }

            try
            {
                QueryFoldersAlerts(folderAlerts, rootFolder, noPropertyFolders);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryFoldersAlerts. ErrorMessage:{0}", e);
            }

            try
            {
                QueryFolderProperty(folderCache, noPropertyFolders, discoverReader, listObject);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryFolderProperty. ErrorMessage:{0}", e);
            }

            discoverCommon.SetDeleteFolders(rootFolder, noPropertyFolders);

            try
            {
                AddVersionToItems(items, listObject, discoverReader);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing AddVersionToItems. ErrorMessage:{0}", e);
            }

            discoverCommon.AddAttachmentToItem(attachments, items);

        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void ItemChanged(List<EventObject> allEvents, Dictionary<Guid, DocObject> allDocs, AveItemObject rootFolder, AveFolderCache folderCache, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            try
            {
                string attachmentUrl = null;
                if (listObject != null)
                {
                    attachmentUrl = listObject.RootFolderUrl + "/Attachments/";
                }
                var extraItemInfos = GetItemsDocsInfo(folderCache.SiteId, extraItems);
                var result = discoverCommon.HandleItemChanged(allEvents, allDocs, rootFolder, folderCache, listObject, discoverReader, noPropertyFolders, attachmentUrl, extraItemInfos);
                discoverCommon.HandleExtraItems(rootFolder, listObject, noPropertyFolders, extraItemInfos, result.Items, result.SystemItems, attachmentUrl, result.Attachments);
                DoLastCache(result.SystemItems, result.SystemItemViews, result.ItemAlerts, result.FolderAlerts, result.Items, result.Attachments, rootFolder, listObject, folderCache, discoverReader, noPropertyFolders);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        public void QueryItemFromExtraItemList(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader,
            Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            try
            {
                string attachmentUrl = null;
                if (listObject != null)
                {
                    attachmentUrl = listObject.RootFolderUrl + "/Attachments/";
                }
                var extraItemInfos = GetItemsDocsInfo(folderCache.SiteId, extraItems);
                var result = new BusinessLayerForDiscover.AveItemChangedResultCollection();
                discoverCommon.HandleExtraItems(folderObject, listObject, noPropertyFolders, extraItemInfos, result.Items, result.SystemItems, attachmentUrl, result.Attachments);
                DoLastCache(result.SystemItems, result.SystemItemViews, result.ItemAlerts, result.FolderAlerts, result.Items, result.Attachments, folderObject, listObject, folderCache, discoverReader, noPropertyFolders);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        #endregion item change


        /// <summary>
        /// 查询删除的List的ModifiedBy user title
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="deleteListObjs"></param>
        /// <param name="listObjs"></param>
        private void InitUserTitleOfListDeletedByUser(Guid siteId, Guid webId, Dictionary<Guid, AveListObject> deleteListObjs, Dictionary<Guid, AveListObject> listObjs)
        {
            if (deleteListObjs.Keys.Count > 0)
            {
                ExceptionHandlingScope(() =>
                {
                    mQueryWorker.ResetCommand(CommandType.Text);
                    mQueryWorker.AddParameter("@siteId", siteId);
                    mQueryWorker.AddParameter("@webId", webId);
                    var commandText = GetDeletedByUserTitleOfList_Select_UserInfo_AllLists_RecycleBin;
                    foreach (var listId in deleteListObjs.Keys)
                    {
                        var deleteList = listObjs[listId];
                        try
                        {
                            mQueryWorker.AddParameter("@ListId", listId);
                            using (var reader = mQueryWorker.ExecuteReader(commandText))
                            {
                                while (reader.Read())
                                {
                                    deleteList.ModifiedBy = reader.GetString(0);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while get the modifiedBy user of delete list:{0} Error Message{1}", deleteList.Title, ex);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 获取changed list object； 
        /// </summary>
        /// <param name="listObjs"></param>
        /// <param name="deleteListObjs"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        private void CacheChangeListObject(IDictionary<Guid, AveListObject> listObjs, IDictionary<Guid, AveListObject> deleteListObjs, DateTime startTime, DateTime endTime, Guid siteId, Guid webId)
        {
            bool hasSystemList = false;
            try
            {
                ExceptionHandlingScope(() =>
                {
                    mQueryWorker.ResetCommand(CommandType.Text);
                    mQueryWorker.AddParameter("@endTime", endTime);
                    mQueryWorker.AddParameter("@startTime", startTime);
                    mQueryWorker.AddParameter("@siteId", siteId);
                    mQueryWorker.AddParameter("@webId", webId);
                    using (var sr = mQueryWorker.ExecuteReader(GetChangeEventsInWeb_Select_EventCache))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                var isSystemList = sr.IsDBNull(2);

                                #region System List

                                if (isSystemList)
                                {
                                    if (hasSystemList)
                                    {
                                        continue;
                                    }
                                    var systemList = new AveListObject
                                    {
                                        ListId = Guid.Empty,
                                        Name = "{System Folder}",
                                        Title = "{System Folder}"
                                    };
                                    listObjs.Add(Guid.Empty, systemList);
                                    hasSystemList = true;
                                    continue;
                                }

                                #endregion System List

                                var ObjType = (ChangeObjectType)sr.GetValue(1);
                                var nativeChangeType = (NativeChangeType)sr[0];
                                var changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                                var listId = sr.GetGuid(2);
                                var itemUrl = !sr.IsDBNull(5) ? sr.GetString(5) : null;
                                var modifiedBy = !sr.IsDBNull(3) ? sr.GetString(3) : null;
                                var int0 = sr.IsDBNull(6) ? (int?)null : sr.GetInt32(6);
                                var int1 = sr.IsDBNull(7) ? (int?)null : sr.GetInt32(7);
                                var itemId = sr.IsDBNull(9) ? (int?)null : sr.GetInt32(9);
                                var modifiedTime = sr.GetDateTime(4);

                                discoverCommon.HandleSingleChangedListForIB(listObjs, deleteListObjs, listId, ObjType, itemUrl, modifiedTime, changeType, modifiedBy, nativeChangeType, int0, int1, itemId, this);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while Get Change List From EventCache Table. ErrorMessage:{0}", ex);
                            }
                        }
                    }
                });
                HandleViewWebpartChangeForLists(listObjs, startTime, endTime, siteId, webId);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "Error occur while access data from method CacheChangeListObject. ErrorMessage:{0}", e);
            }
        }

        /// <summary>
        /// 添加通过DocId回查对应listId的方法，解决ADO-17242，或者page页上删除webpart时eventlog只有docId没有listId的情况；
        /// </summary>
        /// <param name="listObjs"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        private void HandleViewWebpartChangeForLists(IDictionary<Guid, AveListObject> listObjs, DateTime startTime, DateTime endTime, Guid siteId, Guid webId)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                using (var sr = mQueryWorker.ExecuteReader(GetViewWebPartChangedListIdsForLists_Select_AllDocs_EventCache))
                {
                    while (sr.Read())
                    {
                        if (!sr.IsDBNull(0))
                        {
                            var listid = sr.GetGuid(0);
                            if (!listObjs.ContainsKey(listid))
                            {
                                var docListObj = new AveListObject
                                {
                                    ListId = listid
                                };
                                listObjs.Add(listid, docListObj);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 从AllLists和AllDocs表获取属性初始化AveListObject对象；
        /// </summary>
        /// <param name="listObjs"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        private void InitChangeListObeject(Dictionary<Guid, AveListObject> listObjs, Guid siteId, Guid webId)
        {
            try
            {
                var ids = listObjs.Keys.ToList();
                if (ids.Count == 0)
                {
                    return;
                }
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                var commandTex = GetListByIds_Select_AllLists_AllDocs(ids);
                using (var sr = mQueryWorker.ExecuteReader(commandTex))
                {
                    while (sr.Read())
                    {
                        var listId = sr.GetGuid(0);
                        if (!listObjs.ContainsKey(listId))
                        {
                            continue;
                        }
                        var listObj = listObjs[listId];
                        AveDiscoverSqlUtility.InitListInfoByDataReader(sr, listObj);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Error occurred while Init Change List Object. Error Message {0}", ex);
            }
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryUserProperty(Dictionary<int, AveSiteMemberObject> users, Guid siteId)
        {
            if (!ArgumentCheck(users, siteId))
            {
                return;
            }
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@siteId", siteId);
                var commText = GetUserInfoByIds_Select_UserInfo(users.Keys.ToList());
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        int userId = sr.GetInt32(0);
                        users[userId].IsDomainGroup = sr.GetBoolean(1);
                        users[userId].Title = sr.GetString(2);
                        users[userId].Login = sr.GetString(3);
                    }
                }
            });
        }

        private static bool ArgumentCheck(Dictionary<int, AveSiteMemberObject> users, Guid siteId)
        {
            if (users == null || users.Count <= 0)
            {
                return false;
            }
            if (siteId == Guid.Empty)
            {
                throw new ArgumentException("siteId is null");
            }
            return true;
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryGroupProperty(Dictionary<int, AveSiteMemberObject> groups, Guid siteId)
        {
            if (!ArgumentCheck(groups, siteId))
            {
                return;
            }
            foreach (var item in groups)
            {
                QueryUserProperty(item.Value.AddedMemberIds, siteId);
                QueryUserProperty(item.Value.DeletedMemberIds, siteId);
            }
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@siteId", siteId);
                var commText = GetGroupInfoByIds_Select_Groups(groups.Keys.ToList());
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    while (sr.Read())
                    {
                        var groupId = sr.GetInt32(0);
                        groups[groupId].Title = sr.GetString(1);
                    }
                }
            });
        }

        #endregion

        #region Item Level Security

        /// <summary>
        /// 从EventCache表中查询Item的Security改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/05/10", "Oliver Luo")]
        public Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid siteId, Guid webId, Guid listId, int itemId, DateTime startTime, DateTime endTime)
        {
            var securityChanges = new Dictionary<int, List<AveSecurityObject>>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@listId", listId);
                mQueryWorker.AddParameter("@itemId", itemId);
                mQueryWorker.AddParameter("@SiteId", siteId);
                using (var sr = mQueryWorker.ExecuteReader(GetChangedItemSecurity_Select_EventCache))
                {
                    while (sr.Read())
                    {
                        var securityObject = AveDiscoverSqlUtility.GetSecurityObjectInfoByReader(sr);
                        discoverCommon.AddSecurityChangeObjectToCollection(securityObject, securityChanges);
                    }
                }
            });
            return securityChanges;
        }

        #endregion Security

        #region List Level

        /// <summary>
        /// 获取List下的RootFolder信息
        /// 效率考虑，有API实现 
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="itemColumns"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "list or rootFolder properties.")]
        [QueryReview("2012/12/11", "Austin Han", true, "Add SiteId to improve the performance.")]
        public void QueryListRootFolder(AveListCache listCache, AveDiscoverReader discoverReader, AveListObject listObject, AveItemObject rootFolderObject)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", listCache.SiteId);
                mQueryWorker.AddParameter("@WebId", listCache.WebId);
                mQueryWorker.AddParameter("@ListId", listCache.ListId);
                var getListRootFolderCommandText = GetListRootFolder_Select_AllLists_AllDocs(discoverReader.GetItemColumns());
                using (var sr = new AveQueryDataReader(mQueryWorker.ExecuteReader(getListRootFolderCommandText)))
                {
                    if (sr.Read())
                    {
                        try
                        {
                            discoverReader.ReadItemContent(rootFolderObject, sr);
                            rootFolderObject.ObjType = ItemType.Folder;
                            rootFolderObject.DirName = (string)sr["DirName"];
                            if (!Convert.IsDBNull(sr["tp_MaxMajorwithMinorVersionCount"]))
                            {
                                listObject.MaxMajorwithMinorVersionCount = (int)sr["tp_MaxMajorwithMinorVersionCount"];
                            }
                            rootFolderObject.FullUrl = string.Format("{0}/{1}", rootFolderObject.DirName, rootFolderObject.LeafName).Trim('/');
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "An exception occurred while access data from QueryListRootFolder. Error Message: {0}", e);
                        }
                    }
                }
            });
        }


        /// <summary>
        /// 从EventCache表中获取List下Alert的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            var changeAlerts = new Dictionary<Guid, AveAlertObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@listId", listId);
                using (var sr = mQueryWorker.ExecuteReader(GetChangedListAlerts_Select_EventCache_ImmedSubscriptions_SchedSubscriptions))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                            var changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                            var alertId = sr.GetGuid(2);
                            var isAlertDeleted = sr.IsDBNull(6) && sr.IsDBNull(7)
                                                 || sr.GetString(4).ToLower(CultureInfo.InvariantCulture).Contains("filterpath")
                                                 || sr.GetString(5).ToLower(CultureInfo.InvariantCulture).Contains("filterpath");

                            discoverCommon.AddAlertChangeInfoToCollection(changeAlerts, alertId, changeType, isAlertDeleted);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Exception occur while access data from method QueryListAlertForIB.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(3), e);
                        }
                    }
                }
            });
            return changeAlerts;
        }

        /// <summary>
        /// 获取List下的View信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            Dictionary<Guid, AveViewObject> changeViews = new Dictionary<Guid, AveViewObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                using (var sr = mQueryWorker.ExecuteReader(GetChangedListViews_Select_EventCache_AllWebParts))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var changeType = DiscoverUtility.GetChangeType((NativeChangeType)sr.GetValue(6));
                            var viewId = (Guid)sr.GetValue(7);

                            var viewChange = new AveViewObject();
                            if (!sr.IsDBNull(ViewColumn.Id)) //tp_ID is not null
                            {
                                DiscoverUtility.FillWebPartDicFromAllWebParts(viewChange, sr);
                            }
                            discoverCommon.AddChangedViewToCollection(changeViews, viewId, viewChange, changeType);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListViewForIB. EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(29), e);
                        }
                    }
                }
            });
            return changeViews;
        }


        /// <summary>
        /// 获取List下的Security信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            //add siteId and scopeId
            var securityChanges = new Dictionary<int, List<AveSecurityObject>>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                //add parameter
                using (var sr = mQueryWorker.ExecuteReader(GetChangedListSecurity_Select_EventCache))
                {
                    while (sr.Read())
                    {
                        var securityObject = AveDiscoverSqlUtility.GetSecurityObjectInfoByReader(sr);
                        discoverCommon.AddSecurityChangeObjectToCollection(securityObject, securityChanges);
                    }
                }
            });
            return securityChanges;
        }


        /// <summary>
        /// 获取List下ContentType信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            var contentTypeChanges = new Dictionary<byte[], AveContentTypeObject>();
            ExceptionHandlingScope(() =>
            {
                //can't get a content modify from list  just can get add and delete
                //we create a culumn to a list it belongs to modify view,list 级别没有column的概念
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                using (var sr = mQueryWorker.ExecuteReader(GetChangedListContentTypes_Select_EventCache))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                            var objType = (ChangeObjectType)sr.GetValue(1);
                            ChangeType changeType;
                            switch (nativeChangeType)
                            {
                                case NativeChangeType.ListContenTypeAdd:
                                    changeType = ChangeType.Add;
                                    break;
                                case NativeChangeType.ListContenTypeDelete:
                                    changeType = ChangeType.Delete;
                                    break;
                                default:
                                    changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                                    break;
                            }
                            var contentTypeId = (byte[])sr.GetValue(3);
                            discoverCommon.AddChangedContentTypeToCollection(contentTypeChanges, contentTypeId, changeType, objType);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListContentTypeForIB.EventTime:{0}. ErrorMessage:{1}", sr.GetDateTime(4), e);
                        }
                    }
                }
            });
            return contentTypeChanges;
        }


        /// <summary>
        /// 获取Web下系统文件的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        [QueryReview("2012/12/11", "Austin Han")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of sql statement. ")]
        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        {
            QueryListItemsForIB(folderCache, folderObject, startTime, endTime, listObject, discoverReader, noPropertyFolders, extraItems, true);
        }

        /// <summary>
        /// 获取List下Item信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ec:EventCache As ec.")]
        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        {
            QueryListItemsForIB(folderCache, folderObject, startTime, endTime, listObject, discoverReader, noPropertyFolders, extraItems, false);
        }


        #endregion List Level

        #region web level

        /// <summary>
        /// 获取Web的RootFolder信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="rootFolderObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        public void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            noPropertyFolders.Clear();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteId", listCache.SiteId);
                mQueryWorker.AddParameter("@webId", listCache.WebId);

                var result = mQueryWorker.ExecuteScalar(GetWebUrlById_Select_AllWebs);
                if (result == null)
                {
                    //当web被放入回收站时，web full url在数据库中是查不到的,为null。在之后的调用中会出现空引用。[ADO-149249]
                    //当web为rootSC的rootWeb时，webFullUrl为empty,正常查询。
                    logger.Warn("Web full Url can not find. SiteId: {0}, WebId: {1}", listCache.SiteId, listCache.WebId);
                    return;
                }
                var webFullUrl = Convert.ToString(result);
                string dirName;
                string leafName;
                AveUrlUtility.SplitUrl(webFullUrl, out dirName, out leafName);
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteId", listCache.SiteId);
                mQueryWorker.AddParameter("@DirName", dirName);
                mQueryWorker.AddParameter("@LeafName", leafName);
                var commText = GetWebRootFolderInDocs_Select_AllDocs(discoverReader);
                using (var sr = mQueryWorker.ExecuteReader(commText))
                {
                    if (sr.Read())
                    {
                        try
                        {
                            discoverReader.ReadItemContent(rootFolderObject, sr);
                            rootFolderObject.ObjType = ItemType.Folder;
                            rootFolderObject.DirName = (string)sr["DirName"];
                            rootFolderObject.FullUrl = (rootFolderObject.DirName + "/" + rootFolderObject.LeafName).Trim('/');
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Exception occur while access data from QueryWebRootFolder. SiteId:{0}. WebId:{1}. ErrorMessage:{2}", listCache.SiteId, listCache.WebId, e);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 获取Web下Security信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime)
        {
            var webSecurityChanges = new Dictionary<int, List<AveSecurityObject>>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                using (var sr = mQueryWorker.ExecuteReader(GetChangedWebSecurity_Select_EventCache))
                {
                    while (sr.Read())
                    {
                        var securityObject = AveDiscoverSqlUtility.GetSecurityObjectInfoByReader(sr);
                        discoverCommon.AddSecurityChangeObjectToCollection(securityObject, webSecurityChanges);
                    }
                }
            });
            return webSecurityChanges;
        }

        /// <summary>
        /// 获取Web下所有List的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime)
        {
            var listObjs = new Dictionary<Guid, AveListObject>();
            var deleteListObjs = new Dictionary<Guid, AveListObject>();
            CacheChangeListObject(listObjs, deleteListObjs, startTime, endTime, siteId, webId);
            InitChangeListObeject(listObjs, siteId, webId);
            InitUserTitleOfListDeletedByUser(siteId, webId, deleteListObjs, listObjs);
            return listObjs;
        }
        #endregion web level

        #region Site Level

        /// <summary>
        /// 获取Site下的改变信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/05/21", "Oliver Luo")]
        public int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            var type = ChangeType.None;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);

                using (var sr = mQueryWorker.ExecuteReader(GetChangedSite_Select_EventCache))
                {
                    while (sr.Read())
                    {
                        var changeType = DiscoverUtility.GetChangeType((NativeChangeType)sr[1]);
                        if (changeType == ChangeType.Delete)
                        {
                            type = changeType;
                            break;
                        }
                        if (type != ChangeType.Add)
                        {
                            type = changeType;
                        }
                    }
                }
            });
            return (int)type;
        }

        /// <summary>
        /// 获取Site本身的改变信息，还有User 以及Group的改变
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="siteCollectionChangeType"></param>
        /// <param name="userChangeType"></param>
        /// <param name="groupChangeType"></param>
        /// <returns></returns>
        [QueryReview("2013/01/29", "Long Liang")]
        public bool GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime,
            ref ChangeType siteCollectionChangeType, ref ChangeType userChangeType, ref ChangeType groupChangeType)
        {
            var changed = false;
            var tempSiteChangeType = siteCollectionChangeType;
            var tempUserChangeType = userChangeType;
            var tempGroupChangeType = groupChangeType;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                using (var sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.SiteCollectionChangedWithUserAndGroup))
                {
                    while (sr.Read())
                    {
                        changed = true;
                        var changeType = DiscoverUtility.GetChangeType((NativeChangeType)sr[0]);
                        var objectType = (ChangeObjectType)sr[1];

                        switch (objectType)
                        {
                            case ChangeObjectType.Site:
                                if (tempSiteChangeType == ChangeType.Add)
                                {
                                    if (changeType == ChangeType.Delete)
                                    {
                                        tempSiteChangeType = changeType;
                                    }
                                }
                                else
                                {
                                    tempSiteChangeType = changeType;
                                }
                                break;
                            case ChangeObjectType.Group:
                                tempGroupChangeType |= changeType;
                                break;
                            case ChangeObjectType.User:
                                tempUserChangeType |= changeType;
                                break;
                        }
                    }
                }
            });
            siteCollectionChangeType = tempSiteChangeType;
            userChangeType = tempUserChangeType;
            groupChangeType = tempGroupChangeType;
            return changed;
        }

        /// <summary>
        /// 查询event cache里特定时间段内
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            var changeWebObjs = new Dictionary<Guid, AveWebObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                using (var sr = mQueryWorker.ExecuteReader(GetChangedWebsInSite_Select_EventCache_AllWebs))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(1);
                            var objType = (ChangeObjectType)sr.GetValue(2);
                            var webId = sr.GetGuid(3);

                            var isWebRootFolder = sr.GetInt32(13) == 1;
                            var eventTime = sr.GetDateTime(0);
                            var itemFullUrl = sr.IsDBNull(8) ? null : sr.GetString(8);
                            var principleId = sr.IsDBNull(9) ? (int?)null : sr.GetInt32(9);
                            var roleId = sr.IsDBNull(10) ? (int?)null : sr.GetInt32(10);
                            var roleName = sr.IsDBNull(11) ? null : sr.GetString(11);

                            AveWebObject webObj = null;
                            if (!changeWebObjs.ContainsKey(webId))
                            {
                                var title = string.Empty;
                                var fullUrl = string.Empty;
                                var name = string.Empty;
                                if (!sr.IsDBNull(7))
                                {
                                    fullUrl = sr.GetString(4);
                                    title = sr.GetString(5);

                                    name = sr.IsDBNull(6) ? "." : fullUrl.Substring(fullUrl.LastIndexOf('/') + 1).TrimStart('/');
                                }

                                var appInstanceId = sr.IsDBNull(12) ? Guid.Empty : sr.GetGuid(12);

                                webObj = new AveWebObject
                                {
                                    WebID = webId,
                                    Name = name,
                                    FullUrl = fullUrl,
                                    Title = title,
                                    IsAppWeb = !appInstanceId.Equals(Guid.Empty),
                                    AppInstanceId = appInstanceId
                                };
                                changeWebObjs.Add(webId, webObj);
                            }
                            else
                            {
                                webObj = changeWebObjs[webId];
                            }
                            discoverCommon.HandleChangeEventInWebForIB(objType, webObj, changeWebObjs, webId, nativeChangeType, eventTime, itemFullUrl, principleId, roleId, roleName, isWebRootFolder);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebForIB. EventTime:{0}.  ErrorMessage:{1}. SiteId:{2}", sr.GetDateTime(0), e, siteId);
                        }
                    }
                }
            });
            return changeWebObjs;
        }

        /// <summary>
        /// 获取Site下Security信息的改变(User/Group）
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            var memberChanges = new Dictionary<int, AveSiteMemberObject>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);

                var groups = new Dictionary<int, AveSiteMemberObject>();
                var users = new Dictionary<int, AveSiteMemberObject>();

                using (var sr = mQueryWorker.ExecuteReader(GetChangedSiteSecurity_Select_EventCache))
                {
                    while (sr.Read())
                    {
                        var eventTime = sr.GetDateTime(0);
                        try
                        {
                            var principalId = sr.GetInt32(2);
                            var eventType = (NativeChangeType)sr.GetValue(4);
                            var changeObjectType = (ChangeObjectType)sr.GetValue(5);
                            var title = sr.IsDBNull(3) ? string.Empty : sr.GetString(3);
                            var userId = sr.IsDBNull(1) ? (int?)null : sr.GetInt32(1);

                            discoverCommon.HandleSecurityChangedForSite(memberChanges, principalId, changeObjectType, groups, users, eventTime, title, eventType, userId);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Exception occur while access data from method QuerySiteSecurityForIB. EventTime:{0}.  Exception:{1}.  SiteId:{2}", eventTime, e, siteId);
                        }
                    }
                }
                QueryUserProperty(users, siteId);
                QueryGroupProperty(groups, siteId);
            });
            return memberChanges;
        }

        #endregion Site Level

        /// <summary>
        /// 查询delete site的event信息
        /// </summary>
        /// <param name="deletedSites"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public void GetDeleteSites(Dictionary<Guid, AveSiteObject> deletedSites, DateTime startTime, DateTime endTime)
        {
            if (deletedSites == null)
            {
                return;
            }
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@endTime", endTime);
                using (var sr = mQueryWorker.ExecuteReader(GetDeletedSiteChangeEvent_Select_EventCache))
                {
                    while (sr.Read())
                    {
                        var site = new AveSiteObject
                        {
                            ChangeType = ChangeType.Delete,
                            Id = (Guid)sr["SiteId"],
                            EventTime = (DateTime)sr["EventTime"],
                            Url = string.Empty
                        };
                        deletedSites[site.Id] = site;
                    }
                }
            });
        }

        #region TO DO:need test
        public long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime)
        {
            using (new AvePerformanceScope("AveCLReader.GetObjectChangedSize"))
            {
                List<string> parentIdList;
                long changeSizeInAllDocs = 0;
                long changeSizeInAllDocVersions = 0;
                long changeSizeInAllUserData = 0;
                try
                {
                    //不确定要获取Size的对象类型，所以根据参数是否为空来确定查询条件。
                    changeSizeInAllDocs = GetObjectChangeSizeInAllDocs(siteId, webId, listId, folderPath, beginTime, out parentIdList);
                    changeSizeInAllDocVersions = GetObjectChangeSizeInAllDocVersions(siteId, webId, listId, folderPath, beginTime);
                    changeSizeInAllUserData = GetObjectChangeSizeInAllUserData(siteId, webId, listId, folderPath, beginTime, parentIdList);
                }
                catch (Exception ex)
                {
                    logger.Warn(@"An error occurred while getting object changed size, 
                              site id: {0}, web id: {1}, list id: {2}, folder path: {3}, start time: {4}, error: {5}"
                                , siteId, webId, listId, folderPath, beginTime, ex);
                }
                return changeSizeInAllDocs + changeSizeInAllDocVersions + changeSizeInAllUserData;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sql table abbreviation. ")]
        private long GetObjectChangeSizeInAllDocs(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime, out List<string> parentIdList)
        {
            long allDocsChangeSize = 0;
            mQueryWorker.ClearParameters();
            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.AppendLine("SELECT SUM(isnull(cast(doc.Size as bigint) ,doc.SizeWrite)) ,ParentId FROM AllDocs doc with(nolock) WHERE Id IN (");
            cmdBuilder.AppendLine("SELECT DISTINCT dc.Id FROM AllDocs dc with(nolock) INNER JOIN EventCache et with(nolock) ON et.SiteId=dc.SiteId AND et.WebId=dc.WebId AND et.DocId=dc.Id ");
            cmdBuilder.AppendLine("WHERE et.EventTime>@StartTime AND dc.DeleteTransactionId=0x ");
            if (siteId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                cmdBuilder.AppendLine("AND dc.SiteId=@SiteId");
            }
            if (webId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@WebId", webId);
                cmdBuilder.AppendLine("AND dc.WebId=@WebId");
            }
            if (listId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@ListId", listId);
                cmdBuilder.AppendLine("AND dc.ListId=@ListId");
            }
            if (!string.IsNullOrEmpty(folderPath))
            {
                mQueryWorker.AddParameter("@DirName", folderPath + "%");
                cmdBuilder.AppendLine("AND dc.DirName like @DirName ");
            }

            mQueryWorker.AddParameter("@StartTime", beginTime);
            cmdBuilder.Append(" AND dc.Size is not null)");
            cmdBuilder.Append(" Group By ParentId");
            object obj;
            parentIdList = new List<string>();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdBuilder.ToString()))
            {
                while (reader.Read())
                {
                    obj = reader.GetValue(0);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                    {
                        allDocsChangeSize += long.Parse(obj.ToString());
                    }
                    parentIdList.Add(reader.GetValue(1).ToString());
                }
            }
            return allDocsChangeSize;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sql table abbreviation. ")]
        private long GetObjectChangeSizeInAllDocVersions(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime)
        {
            long changeSizeInAllDocVersions = 0;
            mQueryWorker.ClearParameters();
            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.AppendLine("SELECT  SUM(isnull(cast(Size as bigint), SizeWrite)) FROM AllDocVersions with(nolock) WHERE Id IN (");
            cmdBuilder.AppendLine("SELECT DISTINCT dc.Id FROM AllDocs dc with(nolock) INNER JOIN EventCache et with(nolock) ON et.SiteId=dc.SiteId AND et.WebId=dc.WebId AND et.DocId=dc.Id ");
            cmdBuilder.AppendLine("WHERE et.EventTime>@StartTime AND dc.DeleteTransactionId=0x ");

            if (siteId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                cmdBuilder.AppendLine("AND dc.SiteId=@SiteId");
            }
            if (webId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@WebId", webId);
                cmdBuilder.AppendLine("AND dc.WebId=@WebId");
            }
            if (listId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@ListId", listId);
                cmdBuilder.AppendLine("AND dc.ListId=@ListId");
            }
            if (!string.IsNullOrEmpty(folderPath))
            {
                mQueryWorker.AddParameter("@DirName", folderPath + "%");
                cmdBuilder.AppendLine("AND dc.DirName like @DirName");
            }
            mQueryWorker.AddParameter("@StartTime", beginTime);
            cmdBuilder.Append(") AND DeleteTransactionId=0x");
            object obj = mQueryWorker.ExecuteScalar(cmdBuilder.ToString());
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                changeSizeInAllDocVersions = long.Parse(obj.ToString());
            }
            return changeSizeInAllDocVersions;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sql table abbreviation. ")]
        private long GetObjectChangeSizeInAllUserData(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime, List<string> parentIdList)
        {
            long changeSizeInAllUserData = 0;
            mQueryWorker.ClearParameters();
            StringBuilder cmdBuilder = new StringBuilder();
            string parentIdStr = string.Empty;
            List<string> parentIdUnit = new List<string>();
            for (int i = 0; i < parentIdList.Count; i++)
            {
                mQueryWorker.AddParameter("@ParentId" + i, parentIdList[i]);
                parentIdStr += ("@ParentId" + i + ",");
                if ((i % 5000) == 0 && (i != 0))
                {
                    parentIdUnit.Add(parentIdStr);
                    parentIdStr = string.Empty;
                }
            }
            parentIdUnit.Add(parentIdStr);

            foreach (string pIdUnit in parentIdUnit)
            {
                cmdBuilder.AppendLine("SELECT SUM(cast(tp_Size as bigint)) FROM AllUserData with(nolock) WHERE tp_GUID IN (");
                cmdBuilder.AppendLine("SELECT DISTINCT ud.tp_GUID FROM AllUserData ud with(nolock) INNER JOIN EventCache et with(nolock) ON et.SiteId=ud.tp_SiteId AND et.ListId=ud.tp_ListId AND et.ItemId=ud.tp_ID ");
                cmdBuilder.AppendLine("WHERE et.EventTime>@StartTime AND ud.tp_DeleteTransactionId=0x ");
                if (siteId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    cmdBuilder.AppendLine("AND ud.tp_SiteId=@SiteId");
                }
                if (webId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@WebId", webId);
                    cmdBuilder.AppendLine("AND et.WebId=@WebId");
                }
                if (listId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@ListId", listId);
                    cmdBuilder.AppendLine("AND ud.tp_ListId=@ListId");
                }
                mQueryWorker.AddParameter("@StartTime", beginTime);
                if (string.IsNullOrEmpty(folderPath))
                {
                    cmdBuilder.AppendLine("AND ud.tp_ParentId in (");
                    string parentIDString = pIdUnit.TrimEnd(',') + ")";
                    cmdBuilder.Append(parentIDString);
                }

                cmdBuilder.Append(")");
                Object obj = mQueryWorker.ExecuteScalar(cmdBuilder.ToString());
                if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                {
                    changeSizeInAllUserData = long.Parse(obj.ToString());
                }
            }
            return changeSizeInAllUserData;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ud is the abbreviation fo alluserdata. ")]
        public long GetListSize(Guid siteId, Guid webId, Guid listId)
        {
            using (new AvePerformanceScope("AveCLReader.GetListSize"))
            {
                long result = 0;
                try
                {
                    #region Calculate size in AllDocs table
                    string sCmdTxt = @"SELECT SUM(isnull(cast(Size as bigint) ,SizeWrite)) FROM AllDocs with(nolock) 
                                   WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DeleteTransactionId=0x";
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@WebId", webId);
                    mQueryWorker.AddParameter("@ListId", listId);

                    object obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllDocVersions table
                    sCmdTxt = @"SELECT SUM(isnull(cast(Size as bigint), SizeWrite)) FROM AllDocVersions with(nolock) 
                            WHERE Id IN (SELECT Id FROM AllDocs with(nolock) 
                            WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DeleteTransactionId=0x) 
                            AND DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllUserData table //jisuan chongfu duohang
                    sCmdTxt = @"SELECT SUM(cast(tp_Size as bigint)) FROM AllUserData with(nolock) 
                            WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting list size, site id: {0}, web id: {1}, list id: {2}, error: {3}", siteId, webId, listId, ex);
                }
                return result;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ud is the abbreviation fo alluserdata. ")]
        public long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl)
        {
            using (new AvePerformanceScope("AveCLReader.GetFolderSize"))
            {
                long result = 0;
                try
                {
                    #region Calculate size in AllDocs table
                    string sCmdTxt = @"SELECT SUM(isnull(cast(Size as bigint) ,SizeWrite)),ParentId FROM AllDocs with(nolock) 
                                   WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DirName like @DirName AND DeleteTransactionId=0x Group By ParentId ";
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@WebId", webId);
                    mQueryWorker.AddParameter("@ListId", listId);
                    mQueryWorker.AddParameter("@DirName", folderUrl + "%");

                    object obj;
                    List<string> parentIdCollection = new List<string>();
                    using (SqlDataReader reader = mQueryWorker.ExecuteReader(sCmdTxt))
                    {
                        while (reader.Read())
                        {
                            obj = reader.GetValue(0);
                            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                            {
                                result += long.Parse(obj.ToString());
                            }
                            //record parentid, use it query AllUserData later
                            parentIdCollection.Add(reader.GetValue(1).ToString());
                        }
                    }
                    #endregion

                    #region Calculate size in AllDocVersions table
                    sCmdTxt = @"SELECT SUM(isnull(cast(Size as bigint), SizeWrite)) FROM AllDocVersions with(nolock) 
                            WHERE SiteId=@SiteId AND Id IN (SELECT Id FROM AllDocs with(nolock) 
                            WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DirName like @DirName AND DeleteTransactionId=0x) 
                            AND DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllUserData table

                    int queryBatchSize = 1000;
                    int startIndex = 0;
                    int parentIdCount = parentIdCollection.Count;
                    while (startIndex < parentIdCount)
                    {
                        var command = string.Empty;
                        try
                        {
                            command = GetUserDataSizeByParentId_Select_AllUserData(parentIdCollection, startIndex, queryBatchSize);
                            obj = mQueryWorker.ExecuteScalar(command);
                            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                                result += long.Parse(obj.ToString());
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while getting folder size in calculation, folder url: {0}, error:{1} ", folderUrl, e);
                        }
                        startIndex += queryBatchSize;
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting folder size, site id: {0}, web id: {1}, list id: {2}, folder url:{3}, error: {4}", siteId, webId, listId, folderUrl, ex);
                }
                return result;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ud is the abbreviation fo alluserdata. ")]
        public long GetWebSize(Guid siteId, Guid webId)
        {
            using (new AvePerformanceScope("AveCLReader.GetWebSize"))
            {
                long result = 0;
                try
                {
                    #region Calculate size in AllDocs table
                    string sCmdTxt = @"SELECT SUM(isnull(cast(Size as bigint) ,SizeWrite)) FROM AllDocs with(nolock) 
                                   WHERE SiteId=@SiteId AND WebId=@WebId AND DeleteTransactionId=0x";

                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@WebId", webId);


                    Object obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllDocVersion table
                    sCmdTxt = @"SELECT SUM(isnull(cast(Size as bigint), SizeWrite)) FROM AllDocVersions with(nolock) 
                            WHERE SiteId=@SiteId AND Id IN (SELECT Id FROM AllDocs with(nolock) 
                            WHERE SiteId=@SiteId AND WebId=@WebId AND DeleteTransactionId=0x) 
                            AND DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllUserData table
                    sCmdTxt = @"SELECT SUM (cast(tp_Size as bigint)) FROM AllUserData U with(nolock) INNER JOIN AllLists L with(nolock) ON U.tp_ListId = L.tp_ID 
                            INNER JOIN Webs W with(nolock) ON W.SiteId = U.tp_SiteId AND W.ID = L.tp_WebId 
                            WHERE U.tp_SiteId = @SiteId AND W.ID = @WebId AND U.tp_DeleteTransactionId = 0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting web size, site id: {0}, web id: {1}, error: {2}", siteId, webId, ex);
                }
                return result;
            }
        }
        #endregion

        /// <summary>
        /// 初始化AveItemObject的stub信息
        /// </summary>
        /// <param name="allitems"></param>
        /// <param name="siteId"></param>
        public void SetItemStubInfo(List<AveItemObject> allitems, Guid siteId)
        {
            SetItemStubInfo(allitems, siteId, false);
        }

        /// <summary>
        /// 初始化AveItemObject的stub信息
        /// </summary>
        /// <param name="allitems"></param>
        /// <param name="siteId"></param>
        /// <param name="includeRecycleBin">是否查询回收站的文件</param>
        public void SetItemStubInfo(List<AveItemObject> allitems, Guid siteId, bool includeRecycleBin)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                var index = 0;
                var allItemsKeyValues = allitems.Distinct(new ItemObjectDistinc()).ToDictionary(key => key.DocID, value => value);
                while (index < allitems.Count)
                {
                    var tempids = new List<Guid>();
                    do
                    {
                        var item = allitems[index++];
                        if (item.DocID == Guid.Empty)
                        {
                            continue;
                        }
                        if (!tempids.Contains(item.DocID))
                        {
                            tempids.Add(item.DocID);
                        }
                    } while (index < allitems.Count && tempids.Count < 800);
                    if (tempids.Count > 0) //有需要在Alldoc 表中查询的数据, view,webpart 等不需要再alldoc 中查询
                    {
                        var command = GetItemStubInfo_Select_AllDocs_DocsToStreams_DocStreams(tempids, includeRecycleBin);
                        using (var reader = mQueryWorker.ExecuteReader(command))
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var id = (Guid)reader["Id"];
                                    var currentItem = allItemsKeyValues[id];
                                    if (!(reader["DocFlags"] is DBNull))
                                    {
                                        currentItem.DocFlags = (int)reader["DocFlags"];
                                    }
                                    if (!(reader["RbsId"] is DBNull))
                                    {
                                        currentItem.RbsId = (byte[])reader["RbsId"];
                                    }
                                    if (!(reader["Content"] is DBNull))
                                    {
                                        currentItem.Content = (byte[])reader["Content"];
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("An error occurred while getting stub infos.Error:{0}", e);
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 查询site中的user，group信息
        /// </summary>
        /// <param name="siteMember"></param>
        /// <param name="siteId"></param>
        /// <param name="changeObjType"></param>
        public void QueryUserOrGroupProperty(Dictionary<int, AveSiteMemberObject> siteMember, Guid siteId, ChangeObjectType changeObjType)
        {
            switch (changeObjType)
            {
                case ChangeObjectType.User:
                    QueryUserProperty(siteMember, siteId);
                    break;
                case ChangeObjectType.Group:
                    QueryGroupProperty(siteMember, siteId);
                    break;
            }
        }
    }

    internal static class DictionaryExtension
    {
        public static bool TryGetValueByByteArray<T>(this Dictionary<byte[], T> dic, byte[] key, out T value)
        {
            var realKey = GetKeyByByteArray(dic, key);
            var findKey = realKey != null;
            value = findKey ? dic[realKey] : default(T);
            return findKey;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "dic is argument name.")]
        private static byte[] GetKeyByByteArray<T>(Dictionary<byte[], T> dic, byte[] key)
        {
            if (dic == null)
            {
                throw new ArgumentNullException(nameof(dic));
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            byte[] resultkey = null;
            foreach (var tempKey in dic.Keys.Where(k => k.Length == key.Length))
            {
                var i = 0;
                for (; i < key.Length; i++)
                {
                    if (key[i] != tempKey[i])
                    {
                        break;
                    }
                }
                if (i == tempKey.Length)
                {
                    resultkey = tempKey;
                    break;
                }
            }
            return resultkey;
        }

        public static void RemoveByByteArray<T>(this Dictionary<byte[], T> dic, byte[] key)
        {
            var realKey = GetKeyByByteArray(dic, key);
            var findKey = realKey != null;
            if (findKey)
            {
                dic.Remove(realKey);
            }
        }
    }
}


