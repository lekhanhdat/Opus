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

namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using AveChangeType = Wrapper.Common.ChangeType;

    public partial class AveClientOM2013Request
    {
        private readonly int mDefaultBatchQueryItemCount = 480;//AveCamlQuery.QUERY_VALUES_LIMITE_ITEM;
        private int mBatchQueryItemCount;

        //private bool IsListEnableVersions(List list)
        //{
        //    return list.BaseTemplate != (int)AveListTemplateType.UserInformation && 
        //        (list.EnableMinorVersions || list.EnableVersioning);
        //}

        private List<int> GetQueryItemIdRange(List<int> idList, int index, int count)
        {
            var remainingCount = idList.Count - index;
            count = remainingCount < count ? remainingCount : count;
            if (count <= 0)
            {
                return null;
            }

            var ids = idList.GetRange(index, count);
            var firstId = ids.First();
            while (ids.Last() - firstId >= maxItemsPerThrottledOperation)
            {
                ids.RemoveAt(ids.Count - 1);
            }
            return ids;
        }

        private IEnumerable<List<Dictionary<string, object>>> QueryListItems(string webFullUrl,Guid webId, Guid listId, List<int> itemsId, Action<List, ListItemCollection> itemsRetriever,
            Func<List, ListItemCollection, List<Dictionary<string, object>>> itemProcessor)
        {
            var index = 0;
            var count = mBatchQueryItemCount;
            Stopwatch watch = Stopwatch.StartNew();
            do
            {
                var itemsIdRange = GetQueryItemIdRange(itemsId, index, count);
                if (itemsIdRange == null || itemsIdRange.Count <= 0)
                {
                    break;
                }

                var result = new List<Dictionary<string, object>>();
                using (var context = CreateDiscoverContext(webFullUrl))
                {
                    var web = context.Web;
                    var list = web.Lists.GetById(listId);

                    CamlQuery query = new CamlQuery();
                    StringBuilder values = new StringBuilder();
                    foreach (var id in itemsIdRange)
                    {
                        values.AppendFormat(AveCamlQuery.FORMAT_CAML_QUERY_VALUE_INT, id);
                    }
                    query.ViewXml = string.Format(AveCamlQuery.FORMAT_CAML_QUERY_ITEM, values);

                    ListItemCollection items;
                    try
                    {
                        items = list.GetItems(query);
                        itemsRetriever(list, items);
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        count /= 2;
                        string itemsIdStr = string.Join(",", itemsIdRange);
                        var se = e as ServerException;
                        if (se == null)
                        {
                            mLogger.Error($"Failed to query items with id collection: {itemsIdStr} error: {e}");
                        }
                        else
                        {
                            mLogger.Error($"Failed to query items with id collection: {itemsIdStr} server error code: {se.ServerErrorCode} error: {se}");
                            if (ExceptionHandleUtil.HandleBatchExecuteException(se, ref mBatchQueryItemCount, ref count))
                            {
                                index += 1;
                            }
                        }
                        if (count == 0)
                        {
                            Dictionary<string, object> error = new Dictionary<string, object>();
                            error["Error"] = e;
                            result.Add(error);
                            break;
                        }
                        continue;
                    }

                    watch.Stop();
                    mLogger.Info($"Query items with id collection costs: {watch.Elapsed} count: {itemsIdRange.Count()}");

                    try
                    {
                        result = itemProcessor(list, items);
                    }
                    catch (Exception e)
                    {
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        result.Add(error);
                        break;
                    }
                }
                //return here make sure context dispose to avoid memory issue.
                yield return result;

                index += itemsIdRange.Count;
                count = mBatchQueryItemCount;
                watch.Restart();
            }
            while (true);
            watch.Stop();
        }

        private IEnumerable<List<Dictionary<string, object>>> QueryListItemsV2(string webFullUrl, Guid webId, Guid listId, List<int> itemsId, Action<List, ListItemCollection> itemsRetriever,
            Func<List, ListItemCollection, List<Dictionary<string, object>>> itemProcessor, bool isStructureChangeTree = false)
        {
            Dictionary<int, Exception> queryFailedItems = new Dictionary<int, Exception>();
            Action<List<int>, Exception> appendFailedItemsInfo = (itemsIdRange, exception) =>
            {
                foreach(var id in itemsIdRange)
                {
                    queryFailedItems.Add(id, exception);
                }
            };
            foreach (var item in QueryListItemsInternal(webFullUrl, webId, listId, itemsId, mBatchQueryItemCount, appendFailedItemsInfo, itemsRetriever, itemProcessor, isStructureChangeTree))
            {
                yield return item;
            }
            if (queryFailedItems.Count > 0)
            {
                yield return new List<Dictionary<string, object>>
                            {
                                new Dictionary<string, object>
                                {
                                    { "Error", QueryDiscoverFailedItems(webFullUrl, listId, queryFailedItems, isStructureChangeTree) }
                                }
                            };
            }
        }

        private IEnumerable<List<Dictionary<string, object>>> QueryListItemsInternal(string webFullUrl, Guid webId, Guid listId, List<int> itemsId, int count,
            Action<List<int>, Exception> appendFailedItemsInfo, Action<List, ListItemCollection> itemsRetriever, Func<List, ListItemCollection, List<Dictionary<string, object>>> itemProcessor, bool isStructureChangeTree)
        {
            var index = 0;
            CamlQuery query = new CamlQuery();
            Stopwatch watch = Stopwatch.StartNew();
            do
            {
                count = count > mBatchQueryItemCount ? mBatchQueryItemCount : count;
                var itemsIdRange = GetQueryItemIdRange(itemsId, index, count + 1);//Set idrange more than batch query count could return ListItemCollectionPosition for performance optimize
                if (itemsIdRange == null || itemsIdRange.Count <= 0)
                {
                    break;
                }

                string itemsIdStr = string.Join(",", itemsIdRange);
                using (var context = CreateDiscoverContext(webFullUrl))
                {
                    var web = context.Web;
                    var list = web.Lists.GetById(listId);

                    StringBuilder values = new StringBuilder();
                    foreach (var id in itemsIdRange)
                    {
                        values.AppendFormat(AveCamlQuery.FORMAT_CAML_QUERY_VALUE_INT, id);
                    }
                    query.ViewXml = string.Format(AveCamlQuery.FORMAT_CAML_QUERY_ITEM_With_RowLimit, values, count);
                    if (query.ListItemCollectionPosition == null)
                    {
                        query.ListItemCollectionPosition = new ListItemCollectionPosition() { PagingInfo = $"Paged=TRUE&p_ID={itemsIdRange.First() - 1}" };
                    }

                    ListItemCollection items = null;
                    Exception exception = null;
                    try
                    {
                        items = list.GetItems(query);
                        itemsRetriever(list, items);
                        context.Load(items, itms => itms.ListItemCollectionPosition);
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        if (!(e is ServerException se))
                        {
                            mLogger.Error($"Failed to query items with id collection({itemsIdRange.Count()} items): {itemsIdStr} error: {e}");
                        }
                        else
                        {
                            mLogger.Error($"Failed to query items with id collection({itemsIdRange.Count()} items): {itemsIdStr} server error code: {se.ServerErrorCode} error: {se}");
                            switch (se.ServerErrorCode)
                            {
                                case AveSPErrorCode.TP_E_LISTDELETED:
                                case AveSPErrorCode.TP_E_FIELDNOTFOUND:
                                    throw se;
                                case AveSPErrorCode.ERROR_SHARING_BUFFER_EXCEEDED:
                                case AveSPErrorCode.V_OWSSVR_CLICK_MENU:
                                    mBatchQueryItemCount = mBatchQueryItemCount > 60 ? 60 : 1;
                                    break;
                            }
                        }
                        exception = e;
                        items = null;
                    }
                    List<int> shouldRetrievedIdRange = itemsIdRange.Count > count ? itemsIdRange.GetRange(0, count) : itemsIdRange;
                    if (exception != null && shouldRetrievedIdRange.Count > 1)
                    {
                        exception = null;
                        int queryItemsCount = shouldRetrievedIdRange.Count > 60 ? 60 : 1;
                        foreach (var item in QueryListItemsInternal(webFullUrl, webId, listId, shouldRetrievedIdRange, queryItemsCount,
                            appendFailedItemsInfo, itemsRetriever, itemProcessor, isStructureChangeTree))
                        {
                            yield return item;
                        }
                        index += shouldRetrievedIdRange.Count;
                    }
                    watch.Stop();
                    mLogger.Info($"Query items with id collection costs: {watch.Elapsed} count: {itemsIdRange.Count()}, retrieved items count: {items?.Count}");

                    if (items != null && exception == null)
                    {
                        List<Dictionary<string, object>> result = null;
                        try
                        {
                            result = itemProcessor(list, items);
                        }
                        catch (Exception e)
                        {
                            exception = e;
                            mLogger.Warn($"Failed to process items, webFullUrl: {webFullUrl}, webId: {webId}, listId: {listId}, itemIds: {itemsIdStr}{Environment.NewLine}{e}");
                        }
                        if (result != null && result.Count > 0)
                        {
                            yield return result;
                        }
                    }

                    if (exception != null)
                    {
                        appendFailedItemsInfo(shouldRetrievedIdRange, exception);
                        index += shouldRetrievedIdRange.Count;
                    }
                    if (items != null)
                    {
                        //Api will not throw exception if queried items not exist, need control index in this case to avoid query nonexist items repeatledly
                        List<int> retrievedItemIds = items.Select(i => i.Id).ToList();
                        if (items.Count == shouldRetrievedIdRange.Count && retrievedItemIds.All(shouldRetrievedIdRange.Contains))
                        {
                            //shouldRetrievedIdRange里包含retrievedItemIds里的所有元素, query结果正确
                            index = itemsId.IndexOf(items.Last().Id) + 1;
                        }
                        else
                        {
                            mLogger.Info($"Cannot retrieved all items, itemsIdRange: {string.Join(",", itemsIdRange)}, retrievedItemIds: {string.Join(",", retrievedItemIds)}");
                            #region Validate queried out items
                            foreach (var rangeId in itemsIdRange)
                            {
                                if (items.FirstOrDefault(itm => itm.Id == rangeId) == null)
                                {
                                    try
                                    {
                                        var item = GetItemById(webId, listId, rangeId, false);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex is System.IO.FileNotFoundException || ex is AveNotFoundException)
                                        {
                                            mLogger.Info($"Item '{rangeId}' not exist. ex: {ex}");
                                            continue;
                                        }
                                        throw;
                                    }
                                    mLogger.Error($"Find the item that exist but not be retrieved, item id: {rangeId}");
                                    throw new IncorrectBatchQueryException("Incorrect batch discover items result, discover failed.");
                                }
                            }
                            #endregion
                            index += itemsIdRange.Count;
                        }
                    }

                    #region Re-set query.ListItemCollectionPosition for performance optimize
                    //query.ListItemCollectionPosition = null;
                    var position = items?.ListItemCollectionPosition;
                    if (position != null)
                    {
                        var pagingInfo = position.PagingInfo;
                        string[] parameters = pagingInfo.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> requiredParameters = new List<string>();
                        foreach (string str in parameters)
                        {
                            if (str.Contains("Paged=") || str.Contains("p_ID="))
                                requiredParameters.Add(str);
                        }

                        pagingInfo = string.Join("&", requiredParameters.ToArray());
                        position.PagingInfo = pagingInfo;
                        query.ListItemCollectionPosition = position;
                    }
                    #endregion
                }
                //return here make sure context dispose to avoid memory issue.

                //index += itemsIdRange.Count;
                //count = mBatchQueryItemCount;
                watch.Restart();
            }
            while (true);
            watch.Stop();
        }

        private void LoadListItems(string webFullUrl, Guid listId, List<int> itemsIdRange, Action<List, ListItemCollection> itemsRetriever)
        {
            using (var context = CreateDiscoverContext(webFullUrl))
            {
                var web = context.Web;
                var list = web.Lists.GetById(listId);

                CamlQuery query = new CamlQuery();
                StringBuilder values = new StringBuilder();
                foreach (var id in itemsIdRange)
                {
                    values.AppendFormat(AveCamlQuery.FORMAT_CAML_QUERY_VALUE_INT, id);
                }
                query.ViewXml = string.Format(AveCamlQuery.FORMAT_CAML_QUERY_ITEM, values);

                ListItemCollection items = list.GetItems(query);
                itemsRetriever(list, items);
                context.ExecuteQuery();
            }
        }

        private List<DiscoverFailedObj> QueryDiscoverFailedItems(string webFullUrl, Guid listId, Dictionary<int, Exception> discoverFailedItems, bool isStructureChangeTree)
        {
            List<DiscoverFailedObj> result = new List<DiscoverFailedObj>();
            using (var context = CreateDiscoverContext(webFullUrl))
            {
                bool queryListProp = true;
                var web = context.Web;
                var list = web.Lists.GetById(listId);
                try
                {
                    context.Load(list, l => l.DefaultDisplayFormUrl,
                                       l => l.Title,
                                       l => l.RootFolder.ServerRelativeUrl);
                    context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    mLogger.Warn($"Failed to query list basic info, webFullUrl: {webFullUrl}, listId: {listId}, ex: {ex}");
                    queryListProp = false;
                }

                foreach (var failedItm in discoverFailedItems)
                {
                    var obj = new DiscoverFailedObj
                    {
                        ID = failedItm.Key,
                        WebFullUrl = webFullUrl,
                        ListId = listId,
                        ListTitle = queryListProp ? list.Title : string.Empty,
                        ListSRUrl = queryListProp ? list.RootFolder.ServerRelativeUrl : string.Empty,
                        ListDisplayFormUrl = queryListProp ? list.DefaultDisplayFormUrl : string.Empty,
                    };
                    
                    ListItem item = null;
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                    bool hasException = false;
                    try
                    {
                        using (scope.StartScope())
                        {
                            using (scope.StartTry())
                            {
                                item = list.GetItemById(failedItm.Key);
                                context.Load(item, itm => itm["FileRef"],
                                                   itm => itm["UniqueId"],
                                                   itm => itm.FileSystemObjectType);
                            }
                            using (scope.StartCatch())
                            {
                            }
                        }
                        context.ExecuteQuery();
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn($"Failed to query discover failed item basic prop, ex: {ex}");
                        hasException = true;
                    }
                    if (hasException || scope.HasException || item == null)
                    {
                        if (scope.HasException)
                        {
                            mLogger.Warn($"Failed to query discover failed item basic prop, itemId: {failedItm.Key}, listId: {listId}, webFullUrl: {webFullUrl}, ServerErrorTypeName: {scope.ServerErrorTypeName}, ServerErrorCode: {scope.ServerErrorCode}, ErrorMessage: {scope.ErrorMessage}");
                        }
                        obj.FileSystemObjectType = FileSystemObjectType.File;
                        obj.NeedAddFailedIndex = false;
                        obj.UniqueID = Guid.Empty;
                        obj.FileRef = string.IsNullOrEmpty(obj.ListDisplayFormUrl) ? string.Empty : $"{obj.ListDisplayFormUrl}?ID={obj.ID}";
                        obj.LeafName = string.Empty;
                        obj.Error = new AveDiscoverFailedNotAddIdxException(failedItm.Value.Message, obj);
                    }
                    else
                    {
                        obj.UniqueID = new Guid(item["UniqueId"].ToString());
                        obj.FileRef = item["FileRef"].ToString();
                        obj.LeafName = obj.FileRef.Substring(obj.FileRef.LastIndexOf('/') + 1);
                        obj.FileSystemObjectType = item.FileSystemObjectType;
                        obj.NeedAddFailedIndex = true;
                        obj.Error = new AveDiscoverFailedAddIdxException(failedItm.Value.Message, obj);
                    }
                    result.Add(obj);
                }
            }
            return result;
        }

        private IEnumerable<List<Dictionary<string, object>>> ProcessListItems(List list, ListItemCollection items, Func<List, ListItemCollection, List<Dictionary<string, object>>> itemProcessor)
        {
            var result = new List<Dictionary<string, object>>();
            try
            {
                result = itemProcessor(list, items);
            }
            catch (Exception e)
            {
                Dictionary<string, object> error = new Dictionary<string, object>();
                error["Error"] = e;
                result.Add(error);
            }

            //return here make sure context dispose to avoid memory issue.
            yield return result;
        }

        private Func<FileCollection, List<Dictionary<string, object>>, List<File>> GetQueryFileExProcessorFunc(ClientContext context)
        {
            List<File> queryFileExProcessor(FileCollection fileCollection, List<Dictionary<string, object>> objectsProp)
            {
                List<File> queriedOutObjects = null;
                bool querySRUrlFailed = false;
                try
                {
                    context.Load(fileCollection, fs => fs.Include(f => f.ServerRelativeUrl));
                    context.ExecuteQuery();
                }
                catch (Exception srex)
                {
                    mLogger.Warn($"exception occurred when get files' server relative url, ex: {srex}");
                    querySRUrlFailed = true;
                }
                if (querySRUrlFailed)
                {
                    return queriedOutObjects;
                }
                else
                {
                    mLogger.Info($"Need query items count: {fileCollection.Count}.");
                    queriedOutObjects = new List<File>();
                    foreach (var subFile in fileCollection)
                    {
                        try
                        {
                            context.Load(subFile);
                            context.ExecuteQuery();
                            queriedOutObjects.Add(subFile);
                        }
                        catch (Exception oex)
                        {
                            string errorMsg = $"exception occurred when query one file, fileSRUrl: {subFile.ServerRelativeUrl}";
                            mLogger.Warn($"{errorMsg}, ex: {oex}");
                            Dictionary<string, object> errorInfo = new Dictionary<string, object>
                            {
                                ["Error"] = new Exception(errorMsg, oex)
                            };
                            objectsProp.Add(errorInfo);
                        }
                    }
                }
                return queriedOutObjects;
            }
            return queryFileExProcessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="folderSRUrl"></param>
        /// <param name="foldersId"></param>
        /// <param name="systemFolders">system folder that has list item under it</param>
        /// <param name="fieldsNeedLoadOfVersion"></param>
        /// <param name="includeSystemFolder"></param>
        /// <returns></returns>
        public IEnumerable<Dictionary<string, object>> QueryFolderWithStructureForFB(Guid webId, string webUrl, Guid listId, string folderSRUrl, IEnumerable<int> foldersId, List<string> systemFolders, IDictionary<string, string> fieldsNeedLoadOfVersion,bool includeSystemFolder = false)
        {
            // Any issues to set vaule with yield return?
            mIsLoadDFolderId = true;
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            var subFolderProp = new List<Dictionary<string, object>>();
            var subItemsProp = new List<Dictionary<string, object>>();
            parentFolder["Folders"] = subFolderProp;
            parentFolder["Items"] = subItemsProp;
            if (listId != Guid.Empty)
            {
                bool needGetVersion = false;
                using (var context = CreateDiscoverContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    folderSRUrl = "/" + folderSRUrl.TrimStart('/');
                    Folder folder = null;
                    Exception error = null;
                    try
                    {
                        folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderSRUrl));

                        context.Load(list, l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate,
                            l => l.EnableVersioning, l => l.EnableMinorVersions);
                        context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                        context.Load(folder, f => f.ListItemAllFields, f => f.ItemCount);
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        mLogger.Error($"Failed to get folder: {folderSRUrl} Error: {e}");
                        var se = e as ServerException;
                        if (se != null && se.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                        {
                            mIsLoadDFolderId = false;
                            yield break;
                        }
                        error = e;
                    }

                    if (error == null)
                    {
                        needGetVersion = WrapperConfiguration.BPOS_S.IncludeVersionForPerformance &&
                            list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard && IsListEnableVersions(list);

                        SwitchListContext(list);
                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                        try
                        {
                            //Get system file firstly
                            GetSystemFoldersAndFiles(context, subFolderProp, subItemsProp, list, folder, folderSRUrl, maxItemsPerThrottledOperation, systemFolders);
                            //Add to Query View Item by Client API
                            AddViewItems(context, list, folderSRUrl, subItemsProp, subFolderProp, maxItemsPerThrottledOperation);
                        }
                        catch (Exception e)
                        {
                            mLogger.Error("Failed to query system folders with listId:{0}, folderUrl:{1} Error: {2}", listId, folderSRUrl, e);
                            error = e;
                        }
                    }
                    if (error != null)
                    {
                        var obj = new DiscoverFailedObj
                        {
                            //ID = item.Id,
                            //WebFullUrl = webFullUrl,
                            ListId = listId,
                            //ListTitle = list.Title,
                            //ListSRUrl = list.RootFolder.ServerRelativeUrl,
                            //UniqueID = new Guid(item["UniqueId"].ToString()),
                            FileRef = folderSRUrl,
                            FileSystemObjectType = FileSystemObjectType.Folder,
                            NeedAddFailedIndex = true,
                            LeafName = folderSRUrl.Substring(folderSRUrl.LastIndexOf('/') + 1)
                        };
                        obj.Error = new AveDiscoverFailedAddIdxException(error.Message, obj);
                        FillDiscoverFailedObject(subFolderProp, obj);
                        
                        //Dictionary<string, object> errorInfo = new Dictionary<string, object>();
                        //errorInfo["Error"] = error;
                        //subFolderProp.Add(errorInfo);
                        yield return parentFolder;

                        mIsLoadDFolderId = false;
                        yield break;
                    }
                }
                //string webFullUrl = GetCurrentWebFullUrl(webId);
                foreach (var foldersProp in GetFolders(webId, webUrl, listId, folderSRUrl, foldersId, needGetVersion, fieldsNeedLoadOfVersion))
                {
                    subFolderProp.AddRange(foldersProp);
                    yield return parentFolder;
                    subFolderProp.Clear();
                }

                if (subFolderProp.Count > 0)
                {
                    yield return parentFolder;
                    subFolderProp.Clear();
                }
            }
            else
            {
                using (var context = CreateDiscoverContext())
                {
                    try
                    {
                        Web web = context.Site.OpenWebById(webId);
                        context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                        context.ExecuteQuery();
                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                        Dictionary<string, object> folders = GetFolders(web.ServerRelativeUrl, null, Guid.Empty, folderSRUrl != "/" ? "/" + folderSRUrl.TrimStart('/') : "/", includeSystemFolder);
                        foreach (var folder in folders[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>)
                        {
                            //This is for AveQuery GetFolderStructureFromParent in FillFolderObject
                            //need improve
                            folder["IsSystemFile"] = true;
                            subFolderProp.Add(folder);
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query web folders with webId:{0}, folderUrl:{1}", webId, folderSRUrl);
                        var obj = new DiscoverFailedObj
                        {
                            //ID = item.Id,
                            //WebFullUrl = webFullUrl,
                            ListId = listId,
                            //ListTitle = list.Title,
                            //ListSRUrl = list.RootFolder.ServerRelativeUrl,
                            //UniqueID = new Guid(item["UniqueId"].ToString()),
                            FileRef = folderSRUrl,
                            FileSystemObjectType = FileSystemObjectType.Folder,
                            NeedAddFailedIndex = true,
                        };
                        obj.LeafName = obj.FileRef.Substring(obj.FileRef.LastIndexOf('/') + 1);
                        obj.Error = new AveDiscoverFailedAddIdxException(e.Message, obj);
                        FillDiscoverFailedObject(subFolderProp, obj);
                        //Dictionary<string, object> error = new Dictionary<string, object>();
                        //error["Error"] = e;
                        //subFolderProp.Add(error);
                    }
                }
                yield return parentFolder;
            }

            mIsLoadDFolderId = false;
        }
        public IEnumerable<Dictionary<string, object>> QueryChangeFolders(Guid webId, string webUrl, Guid listId, string folderSRUrl, 
            IList<SPOChangeFolder> changeFolders, IDictionary<string, string> fieldsNeedLoadOfVersion, IList<SPOChangeDesignFolder> changeDesignFolders = null)
        {
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            var subFolderProp = new List<Dictionary<string, object>>();
            parentFolder["Folders"] = subFolderProp;

            folderSRUrl = "/" + folderSRUrl.TrimStart('/');
            using (var context = CreateDiscoverContext())
            {
                Web web = context.Site.OpenWebById(webId);
                context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                context.ExecuteQuery();
                parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                #region DesignFolder
                if (changeDesignFolders?.Count > 0)
                {
                    Action<IList<Guid>> batchRetrieveFolders = (batchFolderId) => 
                    {
                        if (batchFolderId?.Count <= 0)
                        {
                            return;
                        }
                        var folders = new List<Folder>();
                        try
                        {
                            foreach (var folderId in batchFolderId)
                            {
                                var folder = web.GetFolderById(folderId);
                                context.Load(folder);
                                context.Load(folder.Properties);
                                folders.Add(folder);
                            }
                            context.ExecuteQuery();
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn($"Failure to retrieve change folder with batch: {string.Join(",", batchFolderId)} Error: {e}");
                            folders.Clear();
                            foreach (var folderId in batchFolderId)
                            {
                                try
                                {
                                    var folder = web.GetFolderById(folderId);
                                    context.Load(folder);
                                    context.Load(folder.Properties);
                                    context.ExecuteQuery();
                                }
                                catch (Exception ex)
                                {
                                    mLogger.Error($"Failure to retrieve change folder, WebId: {webId} UniqueId: {folderId}, Error: {ex}");
                                    var se = ex as ServerException;
                                    if (se != null && se.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                                    {
                                        continue;
                                    }
                                    throw;
                                }
                            }

                        }
                        foreach (var temp in folders)
                        {
                            var cf = changeDesignFolders.FirstOrDefault(f => f.UniqueId == temp.UniqueId);
                            if (cf == null)
                            {
                                mLogger.Warn($"Change design folder unique id does not match: {temp.UniqueId}");
                                continue;
                            }
                            var prop = AssembleFolderProperties(temp);
                            prop["ItemId"] = prop["Id"] = 0;
                            prop["Hidden"] = true;
                            prop["IsSystemFile"] = true;
                            prop["ChangeType"] = cf.ChangeType;
                            prop["ChangeTime"] = new DateTime(cf.ChangeTicks, DateTimeKind.Utc);
                            subFolderProp.Add(prop);
                        }
                    };

                    var foldersBatch = new List<Folder>();
                    var foldersIdToQuery = new List<Guid>();
                    for (int i = 0; i < changeDesignFolders.Count; i++)
                    {
                        var designFolder = changeDesignFolders[i];
                        if (designFolder.ChangeType == AveChangeType.None)
                        {
                            var noneChangeFolder = new Dictionary<string, object>();
                            noneChangeFolder["ObjType"] = ItemType.Folder;
                            noneChangeFolder["FullUrl"] = noneChangeFolder["ServerRelativeUrl"] = $"{folderSRUrl}/{designFolder.Name}";
                            noneChangeFolder["LeafName"] = designFolder.Name;
                            noneChangeFolder["Items"] = new List<Dictionary<string, object>>();
                            noneChangeFolder["Folders"] = new List<Dictionary<string, object>>();
                            noneChangeFolder["IsSystemFile"] = true;
                            subFolderProp.Add(noneChangeFolder);
                            continue;
                        }

                        foldersIdToQuery.Add(designFolder.UniqueId);
                        if (foldersIdToQuery.Count == OBJECT_NUMBER_PER_REQUEST)
                        {
                            batchRetrieveFolders(foldersIdToQuery);
                            foldersIdToQuery.Clear();
                        }
                    }

                    batchRetrieveFolders(foldersIdToQuery);
                }
                #endregion                
            }
            if (listId != Guid.Empty && changeFolders?.Count > 0)
            {
                bool needGetVersion;
                using (var context = CreateDiscoverContext())
                {
                    var web = context.Site.OpenWebById(webId);
                    List list = web.Lists.GetById(listId);
                    context.Load(list, l => l.BaseTemplate, l => l.EnableVersioning, l => l.EnableMinorVersions);
                    context.ExecuteQuery();

                    needGetVersion = WrapperConfiguration.BPOS_S.IncludeVersionForPerformance &&
                        list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard && IsListEnableVersions(list);
                }

                var foldersId = new List<int>();
                foreach (var folder in changeFolders)
                {
                    if (folder.ChangeType == AveChangeType.None)
                    {
                        var changeFolder = new Dictionary<string, object>();
                        changeFolder["ObjType"] = ItemType.Folder;
                        changeFolder["FullUrl"] = changeFolder["ServerRelativeUrl"] = $"{folderSRUrl}/{folder.Name}";
                        changeFolder["LeafName"] = folder.Name;
                        changeFolder["Items"] = new List<Dictionary<string, object>>();
                        changeFolder["Folders"] = new List<Dictionary<string, object>>();
                        subFolderProp.Add(changeFolder);
                    }
                    else
                    {
                        foldersId.Add(folder.Id);
                    }
                }
                //string webFullUrl = GetCurrentWebFullUrl(webId);
                foreach (var foldersProp in GetFolders(webId, webUrl, listId, folderSRUrl, foldersId, needGetVersion, fieldsNeedLoadOfVersion))
                {
                    foreach (var folderProp in foldersProp)
                    {
                        if (folderProp.Count == 1 && folderProp.ContainsKey("Error"))
                        {
                            subFolderProp.Add(folderProp);
                            continue;
                        }
                        var itemProp = folderProp[$"Item{AveObjectModelConstant.ObjectPropertySuffix}"] as Dictionary<string, object>;
                        if (itemProp.TryGetValue("Id", out object idObj))
                        {
                            var folderId = Convert.ToInt64(idObj);
                            var changeFolder = changeFolders.FirstOrDefault(f => f.Id == folderId);
                            if (changeFolder == null)
                            {
                                mLogger.Warn($"Change folder id does not match: {folderId}");
                                continue;
                            }
                            itemProp["ChangeType"] = changeFolder.ChangeType;
                            itemProp["ChangeTime"] = new DateTime(changeFolder.ChangeTicks, DateTimeKind.Utc);
                            subFolderProp.Add(folderProp);
                        }
                    }
                    yield return parentFolder;
                    subFolderProp.Clear();
                }
            }

            if (subFolderProp.Count > 0)
            {
                yield return parentFolder;
                subFolderProp.Clear();
            }
        }
        public IEnumerable<Dictionary<string, object>> QueryItemWithStructureForFB(Guid webId, string webUrl, Guid listId, string folderSRUrl, IEnumerable<int> itemsId, IList<SPOChangeItem> changeItems, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            mIsLoadDFolderId = true;
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            var subitemsProp = new List<Dictionary<string, object>>();
            var subfoldersProp = new List<Dictionary<string, object>>();
            parentFolder["Items"] = subitemsProp;
            parentFolder["Folders"] = subfoldersProp;
            if (listId != Guid.Empty)
            {
                bool needGetVersion = false;
                bool isDocumentLib = false;

                using (var context = CreateDiscoverContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(web, w => w.ServerRelativeUrl, w => w.WebTemplate,w=>w.Url);
                    List list = web.Lists.GetById(listId);
                    folderSRUrl = "/" + folderSRUrl.TrimStart('/');
                    Folder folder = null;
                    Exception error = null;
                    try
                    {
                        folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderSRUrl));
                        context.Load(list, l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate,
                            l => l.EnableVersioning, l => l.EnableMinorVersions);
                        context.Load(list.RootFolder, f => f.ServerRelativeUrl);
                        context.Load(folder, f => f.ListItemAllFields, f => f.ItemCount);
                        context.ExecuteQuery();
                        needGetVersion = WrapperConfiguration.BPOS_S.IncludeVersionForPerformance
                            && IsListEnableVersions(list)
                            && !string.Equals(web.WebTemplate, "APP", StringComparison.OrdinalIgnoreCase);
                        isDocumentLib = list.BaseType == BaseType.DocumentLibrary;
                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                        //get system file firstly
                        GetSystemFiles(context, subitemsProp, list, folder, folderSRUrl);
                        AddViewItems(context, list, folderSRUrl, subitemsProp, maxItemsPerThrottledOperation);
                    }
                    catch (Exception e)
                    {
                        mLogger.Error($"Failed to get folder: {folderSRUrl} Error: {e}");
                        var se = e as ServerException;
                        if (se != null && se.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                        {
                            mIsLoadDFolderId = false;
                            yield break;
                        }
                        error = e;
                    }
                    if (error != null)
                    {
                        var obj = new DiscoverFailedObj
                        {
                            //ID = item.Id,
                            //WebFullUrl = webFullUrl,
                            ListId = listId,
                            //ListTitle = list.Title,
                            //ListSRUrl = list.RootFolder.ServerRelativeUrl,
                            //UniqueID = new Guid(item["UniqueId"].ToString()),
                            FileRef = folderSRUrl,
                            FileSystemObjectType = FileSystemObjectType.Folder,
                            NeedAddFailedIndex = true,
                            LeafName = folderSRUrl.Substring(folderSRUrl.LastIndexOf('/') + 1)
                        };
                        obj.Error = new AveDiscoverFailedAddIdxException(error.Message, obj);
                        FillDiscoverFailedObject(subfoldersProp, obj);

                        //Dictionary<string, object> errorInfo = new Dictionary<string, object>();
                        //errorInfo["Error"] = error;
                        //subitemsProp.Add(errorInfo);
                        yield return parentFolder;

                        mIsLoadDFolderId = false;
                        yield break;
                    }
                }
                //string webFullUrl = GetCurrentWebFullUrl(webId);
                foreach (var itemsProp in GetItems(webId, webUrl, listId, folderSRUrl, itemsId, needGetVersion, isDocumentLib, fieldsNeedLoadOfVersion))
                {
                    foreach (var prop in itemsProp)
                    {
                        if (prop.Count == 1 && prop.ContainsKey("Error"))
                        {
                            subitemsProp.Add(prop);
                            continue;
                        }
                        int id = Convert.ToInt32(prop["Id"]);
                        var ci = changeItems?.FirstOrDefault(i => i.Id == id);
                        if (ci != null)
                        {
                            prop["ChangeType"] = ci.ChangeType;
                            prop["ChangeTime"] = new DateTime(ci.ChangeTicks, DateTimeKind.Utc);
                            prop["ChangeTicks"] = ci.ChangeTicks;
                            prop["ChangeBehaviorDetails"] = ci.ChangeBehaviorDetails;
                        }
                        subitemsProp.Add(prop);
                    }
                    yield return parentFolder;
                    subitemsProp.Clear();
                }

                if (subitemsProp.Count > 0)
                {
                    yield return parentFolder;
                    subitemsProp.Clear();
                }
            }
            else
            {
                List<Dictionary<string, object>> webItems = parentFolder["Items"] as List<Dictionary<string, object>>;
                using (var context = CreateDiscoverContext())
                {
                    try
                    {
                        Web web = context.Site.OpenWebById(webId);
                        context.Load(web, w => w.ServerRelativeUrl);
                        context.ExecuteQuery();

                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                        Dictionary<string, object> files = GetFiles(web.ServerRelativeUrl, null, folderSRUrl != "/" ? "/" + folderSRUrl.TrimStart('/') : "/");
                        foreach (var item in files[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>)
                        {
                            List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                            AssembleWebItemVersionProperty(item, versions);
                            item["HasVersion"] = false;
                            webItems.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query web files with webId:{0}, folderUrl:{1}", webId, folderSRUrl);
                        var se = e as ServerException;
                        if (se != null && se.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                        {
                            mIsLoadDFolderId = false;
                            yield break;
                        }
                        var obj = new DiscoverFailedObj
                        {
                            //ID = item.Id,
                            //WebFullUrl = webFullUrl,
                            ListId = listId,
                            //ListTitle = list.Title,
                            //ListSRUrl = list.RootFolder.ServerRelativeUrl,
                            //UniqueID = new Guid(item["UniqueId"].ToString()),
                            FileRef = folderSRUrl,
                            FileSystemObjectType = FileSystemObjectType.Folder,
                            NeedAddFailedIndex = true,
                        };
                        obj.LeafName = obj.FileRef.Substring(obj.FileRef.LastIndexOf('/') + 1);
                        obj.Error = new AveDiscoverFailedAddIdxException(e.Message, obj);
                        FillDiscoverFailedObject(subfoldersProp, obj);
                        //Dictionary<string, object> error = new Dictionary<string, object>();
                        //error["Error"] = e;
                        //webItems.Add(error);
                    }
                    yield return parentFolder;
                }
            }

            mIsLoadDFolderId = false;
        }
        public IEnumerable<Dictionary<string, object>> QueryChangeItems(Guid webId, string webUrl, Guid listId, string folderSRUrl,
            IList<SPOChangeItem> changeItems, IDictionary<string, string> fieldsNeedLoadOfVersion, IList<SPOChangeDesignFile> changeDesignFiles = null)
        {
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            var subItemsProp = new List<Dictionary<string, object>>();
            parentFolder["Items"] = subItemsProp;

            folderSRUrl = "/" + folderSRUrl.TrimStart('/');
            using (var context = CreateDiscoverContext())
            {
                Web web = context.Site.OpenWebById(webId);
                #region ChangeFile
                if (changeDesignFiles?.Count > 0)
                {
                    List<File> filesBatch = new List<File>();
                    int pageCount = changeDesignFiles.Count / OBJECT_NUMBER_PER_REQUEST + 1;
                    for (int pageNum = 0; pageNum < pageCount; pageNum++)
                    {
                        var page = changeDesignFiles.Skip(pageNum * OBJECT_NUMBER_PER_REQUEST).Take(OBJECT_NUMBER_PER_REQUEST);
                        if (!page.Any())
                        {
                            break;
                        }
                        var cfsInPage = page.ToList();
                        try
                        {
                            foreach (var cf in cfsInPage)
                            {
                                var filePath = ResourcePath.FromDecodedUrl($"{folderSRUrl.TrimEnd('/')}/{cf.Name}");
                                var temp = web.GetFileByServerRelativePath(filePath);
                                context.Load(temp);
                                filesBatch.Add(temp);
                            }
                            context.ExecuteQuery();
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn($"Failure to retrieve change file property with batch, Error: {e}");

                            filesBatch.Clear();
                            foreach (var cf in cfsInPage)
                            {
                                var fileSRUrl = $"{folderSRUrl.TrimEnd('/')}/{cf.Name}";
                                try
                                {
                                    var filePath = ResourcePath.FromDecodedUrl(fileSRUrl);
                                    var temp = web.GetFileByServerRelativePath(filePath);
                                    context.Load(temp);
                                    context.ExecuteQuery();
                                    filesBatch.Add(temp);
                                }
                                catch (Exception ex)
                                {
                                    mLogger.Error($"Failure to retrieve change file property, WebId: {webId}, fileSRUrl: {fileSRUrl}, Error: {e}");
                                    var se = ex as ServerException;
                                    if (se != null && se.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                                    {
                                        continue;
                                    }
                                    break;
                                }
                            }
                        }
                        foreach (var file in filesBatch)
                        {
                            var cf = changeDesignFiles.FirstOrDefault(f => string.Equals(file.Name, f.Name, StringComparison.OrdinalIgnoreCase));
                            if (cf == null)
                            {
                                mLogger.Warn($"Change design file name does not match with which in change log: {file.Name}");
                                continue;
                            }

                            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
                            AssembleViewFileProperties(itemProperty, file);
                            itemProperty["ChangeType"] = cf.ChangeType;
                            itemProperty["ChangeTime"] = new DateTime(cf.ChangeTicks, DateTimeKind.Utc);

                            itemProperty["ObjType"] = ItemType.Item;
                            itemProperty["IsSystemFile"] = true;
                            subItemsProp.Add(itemProperty);
                        }
                        filesBatch.Clear();
                    }
                }
                #endregion
            }
            if (listId != Guid.Empty && changeItems?.Count > 0)
            {
                bool needGetVersion = false;
                bool isDocumentLib = false;
                try
                {
                    using (var context = CreateDiscoverContext())
                    {
                        var web = context.Site.OpenWebById(webId);
                        context.Load(web, w => w.WebTemplate);
                        List list = web.Lists.GetById(listId);
                        context.Load(list, l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate,
                            l => l.EnableVersioning, l => l.EnableMinorVersions);
                        context.Load(list.RootFolder, f => f.ServerRelativeUrl);
                        context.ExecuteQuery();
                        needGetVersion = WrapperConfiguration.BPOS_S.IncludeVersionForPerformance
                            && IsListEnableVersions(list)
                            && !string.Equals(web.WebTemplate, "APP", StringComparison.OrdinalIgnoreCase);
                        isDocumentLib = list.BaseType == BaseType.DocumentLibrary; 
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn($"Failed to get list {listId}, Error: {e}");
                    var se = e as ServerException;
                    if (se != null && se.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                    {
                        yield break;
                    }
                    throw;
                }

                var changeItemsId = changeItems.Select(i => i.Id).ToList();
                //var webFullurl = GetCurrentWebFullUrl(webId);
                foreach (var itemsProp in GetItems(webId, webUrl, listId, folderSRUrl, changeItemsId, needGetVersion, isDocumentLib, fieldsNeedLoadOfVersion))
                {
                    foreach (var prop in itemsProp)
                    {
                        if (prop.Count == 1 && prop.ContainsKey("Error"))
                        {
                            subItemsProp.Add(prop);
                            continue;
                        }
                        int id = Convert.ToInt32(prop["Id"]);
                        var ci = changeItems.FirstOrDefault(i=>i.Id == id);
                        if (ci == null)
                        {
                            mLogger.Warn($"Change item id does not match: {id}");
                            continue;
                        }

                        prop["ChangeType"] = ci.ChangeType;
                        prop["ChangeTime"] = new DateTime(ci.ChangeTicks, DateTimeKind.Utc);
                        prop["ChangeTicks"] = ci.ChangeTicks;
                        prop["ChangeBehaviorDetails"] = ci.ChangeBehaviorDetails;
                        subItemsProp.Add(prop);
                    }                    
                    yield return parentFolder;
                    subItemsProp.Clear();
                } 
            }

            if (subItemsProp.Count > 0)
            {
                yield return parentFolder;
                subItemsProp.Clear();
            }
        }

        private string GetCurrentWebFullUrl(Guid webid)
        {
            using (var context = CreateDiscoverContext())
            {
                var web = context.Site.OpenWebById(webid);
                context.Load(web,w=>w.Url);
                context.ExecuteQuery();
                return web.Url;
            }
        }

        private IEnumerable<List<Dictionary<string, object>>> GetFolders(Guid webId,string webFullUrl, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, bool needGetVersion, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            if (foldersId == null || foldersId.Count() <= 0)
            {
                yield break;
            }

            int versionCount = WrapperConfiguration.BPOS_S.VersionCount == -1 ? int.MaxValue : WrapperConfiguration.BPOS_S.VersionCount + 1;
            Action<List, ListItemCollection> itemsRetriever = (list, listItems) =>
            {
                var context = list.Context;
                context.Load(list, l => l.BaseTemplate, l => l.RootFolder.ServerRelativeUrl, l => l.BaseType, l => l.Title);
                context.Load(listItems, items => items.IncludeWithDefaultProperties(
                    item => item.HasUniqueRoleAssignments,
                    item => item.Folder,
                    item => item.Folder.Properties));

                if (needGetVersion)
                {
                    context.Load(listItems, items => items.Include(item => item.Versions.Take(versionCount).IncludeWithDefaultProperties(
                        v => v.CreatedBy.LoginName, v => v.FileVersion.CheckInComment)));
                }
            };

            Func<List, ListItemCollection, List<Dictionary<string, object>>> itemsProcessor = (list, listItems) =>
            {
                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                List<DiscoverFailedObj> failedObjs = new List<DiscoverFailedObj>();
                foreach (var item in listItems)
                {
                    try
                    {
                        if (item.FieldValues.TryGetValue("FileRef", out var fileRef) && NeedSkip(list, fileRef.ToString()))
                        {
                            continue;
                        }
                        results.Add(AssembleFolderProperties(list, item, fieldsNeedLoadOfVersion));
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn($"Error occurred when AssembleFolderProperties, item id: {item.Id}, ex: {ex}");
                        var obj = new DiscoverFailedObj
                        {
                            ID = item.Id,
                            WebFullUrl = webFullUrl,
                            ListId = listId,
                            ListTitle = list.Title,
                            ListSRUrl = list.RootFolder.ServerRelativeUrl,
                            UniqueID = new Guid(item["UniqueId"].ToString()),
                            FileRef = item["FileRef"].ToString(),
                            FileSystemObjectType = item.FileSystemObjectType,
                            NeedAddFailedIndex = true,
                        };
                        obj.LeafName = obj.FileRef.Substring(obj.FileRef.LastIndexOf('/') + 1);
                        obj.Error = new AveDiscoverFailedAddIdxException(ex.Message, obj);
                        failedObjs.Add(obj);
                    }
                }
                if (failedObjs.Count > 0)
                {
                    results.Add(new Dictionary<string, object> { { "Error", failedObjs } });
                }
                return results;
            };

            var idList = foldersId.OrderBy(i => i).ToList();

            foreach (var itemsProp in QueryListItemsV2(webFullUrl, webId, listId, idList, itemsRetriever, itemsProcessor))
            {
                yield return itemsProp;
            }
        }

        private IEnumerable<List<Dictionary<string, object>>> GetItems(Guid webId,string webfullurl, Guid listId, string folderServerRelativeUrl, 
            IEnumerable<int> itemsId, bool needGetVersion, bool isDocumentLib, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            if (itemsId == null || itemsId.Count() <= 0)
            {
                yield break;
            }

            int versionCount = WrapperConfiguration.BPOS_S.VersionCount == -1 ? int.MaxValue : WrapperConfiguration.BPOS_S.VersionCount + 1;
            Action<List, ListItemCollection> itemsRetriever = (list, listItems) =>
            {
                var context = list.Context;
                context.Load(list, l => l.BaseType, l => l.RootFolder.ServerRelativeUrl, l => l.Title);
                if (isDocumentLib)
                {
                    context.Load(listItems, items => items.IncludeWithDefaultProperties(
                        item => item.HasUniqueRoleAssignments, item => item.ComplianceInfo
                        ,item => item.File.CustomizedPageStatus, item => item.File.TimeLastModified));
                }
                else
                {
                    context.Load(listItems, items => items.IncludeWithDefaultProperties(
                        item => item.HasUniqueRoleAssignments, item => item.ComplianceInfo));
                }

                if (needGetVersion)
                {
                    context.Load(listItems, items => items.Include(item => item.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                v => v.CreatedBy.LoginName, v => v.FileVersion.CheckInComment, v => v.FileVersion.Length)));
                }
            };

            Func<List, ListItemCollection, List<Dictionary<string, object>>> itemsProcessor = (list, listItems) => 
            {
                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                Dictionary<string, object> complianceTagInfoProperties = null;
                List<DiscoverFailedObj> failedObjs = new List<DiscoverFailedObj>();

                var shortCuts = new List<int>();
                foreach (var item in listItems)
                {
                    try
                    {
                        if (item.FieldValues.TryGetValue(AveConstants.COLUMN_NAME_SHORTCUT_REMOTE_ITEM_UNIQUEID, out object remoteItemUniqueIdObj)
                            && remoteItemUniqueIdObj != null)
                        {
                            shortCuts.Add(item.Id);
                            continue;
                        }

                        var properties = AssembleItemProperties(list, item, fieldsNeedLoadOfVersion);
                        if (properties.ContainsKey("ComplianceInfo"))
                        {
                            properties.Remove("ComplianceInfo");
                        }
                        complianceTagInfoProperties = AssembleComplianceTagInfo(item);
                        properties = properties.Concat(complianceTagInfoProperties).ToDictionary(k => k.Key, k => k.Value);
                        results.Add(properties);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn($"Error occurred when AssembleItemProperties, item id: {item.Id}, ex: {ex}");
                        var obj = new DiscoverFailedObj
                        {
                            ID = item.Id,
                            WebFullUrl = string.Empty,
                            ListId = listId,
                            ListTitle = list.Title,
                            ListSRUrl = list.RootFolder.ServerRelativeUrl,
                            UniqueID = new Guid(item["UniqueId"].ToString()),
                            FileRef = item["FileRef"].ToString(),
                            FileSystemObjectType = item.FileSystemObjectType,
                            NeedAddFailedIndex = true,
                        };
                        obj.LeafName = obj.FileRef.Substring(obj.FileRef.LastIndexOf('/') + 1);
                        obj.Error = new AveDiscoverFailedAddIdxException(ex.Message, obj);
                        failedObjs.Add(obj);
                    }
                }

                if (shortCuts.Any())
                {
                    mLogger.Warn($"Skip the shortcuts: {string.Join(",", shortCuts)}");
                }

                if (failedObjs.Count > 0)
                {
                    results.Add(new Dictionary<string, object> { { "Error", failedObjs } });
                }

                return results;
            };

            var idList = itemsId.OrderBy(i => i).ToList();
            foreach (var itemsProp in QueryListItemsV2(webfullurl,webId, listId, idList, itemsRetriever, itemsProcessor))
            {
                yield return itemsProp;
            }
        }

        private Dictionary<string, object> AssembleFolderProperties(Folder folder)
        {
            var folderProp = new Dictionary<string, object>();
            folderProp["ObjType"] = ItemType.Folder;
            folderProp["Items"] = new List<Dictionary<string, object>>();
            folderProp["Folders"] = new List<Dictionary<string, object>>();
            folderProp[$"Attachments{AveObjectModelConstant.ObjectPropertySuffix}"] = false;
            folderProp["FullUrl"] = folder.ServerRelativeUrl;
            folderProp["LeafName"] = folder.Name;

            CopyProperty(folderProp, folder);
            if (folder.IsPropertyAvailable("Properties") && folder.Properties.FieldValues.Count > 0)
            {
                Hashtable hashtable = new Hashtable();
                foreach (KeyValuePair<string, object> pair in folder.Properties.FieldValues)
                {
                    hashtable[pair.Key] = pair.Value;
                }
                folderProp[$"Properties{AveObjectModelConstant.ObjectPropertySuffix}"] = hashtable;
            }

            return folderProp;
        }
        //private Dictionary<string, object> AssembleFolderProperties(List list, ListItem folder, IDictionary<string, string> fieldsNeedLoadOfVersion)
        //{
        //    var folderProp = AssembleFolderProperties(folder.Folder);
        //    var itemProp = new Dictionary<string, object>();
        //    GetItemDic(itemProp, folder);
        //    itemProp["ItemId"] = itemProp["Id"];
        //    itemProp["Hidden"] = (itemProp["Id"] == null) ? true : false;

        //    itemProp["ObjType"] = ItemType.Folder;
        //    itemProp["Items"] = new List<Dictionary<string, object>>();
        //    itemProp["Folders"] = new List<Dictionary<string, object>>();
        //    itemProp[$"Attachments{AveObjectModelConstant.ObjectPropertySuffix}"] = false;
        //    itemProp["FullUrl"] = folderProp["FullUrl"];
        //    itemProp["LeafName"] = folderProp["LeafName"];

        //    if (list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard)
        //    {
        //        AssembleVersionsProperties(itemProp, folder, fieldsNeedLoadOfVersion);
        //    }
        //    folderProp[$"Item{AveObjectModelConstant.ObjectPropertySuffix}"] = itemProp;

        //    return folderProp;
        //}
        //private Dictionary<string, object> AssembleItemProperties(List list, ListItem item, IDictionary<string, string> fieldsNeedLoadOfVersion)
        //{
        //    Dictionary<string, object> itemProperty = new Dictionary<string, object>();
        //    GetItemDic(itemProperty, item);

        //    itemProperty[$"Attachments{AveObjectModelConstant.ObjectPropertySuffix}"] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
        //    if (list.BaseType == BaseType.DocumentLibrary)
        //    {
        //        itemProperty["ObjType"] = ItemType.Document;
        //        try
        //        {
        //            itemProperty["CustomizedPageStatus"] = (int)item.File.CustomizedPageStatus;
        //        }
        //        catch (Exception ex)
        //        {
        //            mLogger.Info("Can not get CustomizedPageStatus with file {1}.Error:{0}", ex, item.Id);
        //        }
        //    }
        //    else
        //    {
        //        itemProperty["ObjType"] = ItemType.Item;
        //        itemProperty["Attachments"] = new List<Dictionary<string, object>>();
        //        GetAttachmentsFromItem(list.Context, list, itemProperty, list.RootFolder.ServerRelativeUrl);
        //    }
        //    AssembleVersionsProperties(itemProperty, item, fieldsNeedLoadOfVersion);

        //    return itemProperty;
        //}

        //private void AssembleVersionsProperties(Dictionary<string, object> itemProperty, ListItem item, IDictionary<string, string> fieldsNeedLoadOfVersion)
        //{
        //    if (item.IsObjectPropertyInstantiated("Versions") && item.Versions.Count > 0)
        //    {
        //        var versionsObject = new Dictionary<string, object>();
        //        itemProperty["Versions" + AveObjectModelConstant.ObjectPropertySuffix] = versionsObject;
        //        var versions = new List<Dictionary<string, object>>();
        //        foreach (var version in item.Versions)
        //        {
        //            bool needSkipVersion = false;
        //            bool cpoyPropFailed = false;
        //            Dictionary<string, object> listItemVersionData = new Dictionary<string, object>();
        //            Dictionary<string, object> listItemVersionFieldValue = new Dictionary<string, object>();
        //            foreach (KeyValuePair<string, object> fieldValue in version.FieldValues)
        //            {
        //                if (fieldsNeedLoadOfVersion.ContainsKey(fieldValue.Key) ||
        //                        fieldValue.Key.Equals("Editor", StringComparison.OrdinalIgnoreCase) ||
        //                        fieldValue.Key.Equals("Modified", StringComparison.OrdinalIgnoreCase) ||
        //                        fieldValue.Key.Equals("_CheckinComment", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    AssembleItemProperties(listItemVersionFieldValue, fieldValue.Value, fieldValue.Key);
        //                }
        //            }
        //            ///这么做的原因是因为List Item Version的Field Values取出来的check in comment是所有version都有，
        //            ///可能是通过current version赋值的。
        //            string checkinComment = null;
        //            if (version.IsObjectPropertyInstantiated("FileVersion") && version.FileVersion.IsPropertyAvailable("CheckInComment"))
        //            {
        //                checkinComment = version.FileVersion.CheckInComment;
        //            }
        //            else
        //            {
        //                object checkinCommentObj;
        //                if (version.FieldValues.TryGetValue("_CheckinComment", out checkinCommentObj))
        //                {
        //                    checkinComment = checkinCommentObj as string;
        //                }
        //            }

        //            if (checkinComment != null)
        //            {
        //                listItemVersionFieldValue["_CheckinComment"] = checkinComment;
        //                listItemVersionData["_CheckinComment"] = checkinComment;
        //            }

        //            //Created 在创建item的时候能顺带更新，页面上显示的item Created是第一个version的Created
        //            //Created_x0020_Date 不可更新，SharePoint记录的系统时间。
        //            listItemVersionFieldValue["Created"] = version.Created;

        //            listItemVersionData.Add("FieldValues", listItemVersionFieldValue);

        //            //listItemVersionData["Created"] = version.Created;
        //            cpoyPropFailed = cpoyPropFailed || !CopyVersionFieldValues<object>("Modified", "Modified", version.FieldValues, listItemVersionData);
        //            cpoyPropFailed = cpoyPropFailed || !CopyVersionFieldValues<object>("Editor", "Editor", listItemVersionFieldValue, listItemVersionData);
        //            listItemVersionData["VersionId"] = version.VersionId;
        //            listItemVersionData["VersionLabel"] = version.VersionLabel;

        //            needSkipVersion = needSkipVersion || !CopyVersionFieldValues<byte>("_Level", "Level", version.FieldValues, listItemVersionData);
        //            listItemVersionData["IsCurrentVersion"] = version.IsCurrentVersion;
        //            needSkipVersion = needSkipVersion || !CopyVersionFieldValues<object>("FileRef", "Url", version.FieldValues, listItemVersionData);

        //            if (version.IsObjectPropertyInstantiated("FileVersion") && version.FileVersion.IsPropertyAvailable("Length"))
        //            {
        //                listItemVersionData["Length"] = version.FileVersion.Length;
        //            }
        //            //object length;
        //            //File_x0020_Size only represent size for current version,for none current version, this is an incorrect value. Need to get the real version content length from file version.
        //            //if (version.FieldValues.TryGetValue("File_x0020_Size", out length))
        //            //{
        //            //    listItemVersionData["Length"] = length;
        //            //}
        //            needSkipVersion = needSkipVersion || !CopyVersionFieldValues<object>("_ModerationStatus", "ModerationStatus", version.FieldValues, listItemVersionData);
        //            if (version.CreatedBy.ServerObjectIsNull == true)
        //            {
        //                listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = null;
        //            }
        //            else
        //            {
        //                listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = version.CreatedBy.LoginName;
        //            }
        //            if (cpoyPropFailed || needSkipVersion)
        //            {
        //                StringBuilder fieldValuesSb = new StringBuilder();
        //                foreach (var fieldValue in version.FieldValues)
        //                {
        //                    fieldValuesSb.AppendLine($"{fieldValue.Key}: {fieldValue.Value}");
        //                }
        //                mLogger.Warn($"Failed to copy item version's props, need skip this version: {needSkipVersion}. item id: {item.Id}, version id: {version.VersionId}, version fieldValues: {Environment.NewLine}{fieldValuesSb.ToString()}");
        //                if (needSkipVersion)
        //                {
        //                    continue;
        //                }
        //            }
        //            versions.Add(listItemVersionData);
        //        }
        //        versionsObject[AveObjectModelConstant.ChildrenProperties] = versions;
        //    }
        //    else
        //    {
        //        // add current version to versions if list disable version or item do not have versions
        //        //AssembleItemVersionProperty(itemProperty, versions);
        //        itemProperty["HasVersion"] = false;
        //    }
        //}

        private bool CopyVersionFieldValues<T>(string sourcePropName, string destPropName, Dictionary<string, object> sourceFieldValues, Dictionary<string, object> destDic)
        {
            bool IsCopySuccess = false;
            if (sourceFieldValues.TryGetValue(sourcePropName, out object value))
            {
                destDic[destPropName] = (T)Convert.ChangeType(value, typeof(T));
                IsCopySuccess = true;
            }
            else
            {
                mLogger.Warn($"Cannot find prop from field values, prop name: {sourcePropName}");
            }
            return IsCopySuccess;
        }

        /// <summary>
        /// 获取整个list的folder结构
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        private SPOFolder GetListFolderStructure(string webFullUrl, Guid listId)
        {
            using (var context = CreateDiscoverContext(webFullUrl))
            {
                Stopwatch watch = Stopwatch.StartNew();
                var web = context.Web;
                var list = web.Lists.GetById(listId);
                context.Load(list, l=>l.RootFolder.ServerRelativeUrl, l=>l.ItemCount);
                context.ExecuteQuery();

                var fullTree = new SPOFolder() { Name = list.RootFolder.ServerRelativeUrl };
                int currentCount = 0;

                ProcessListItems(webFullUrl, listId,
                    items => items.RetrieveItems().Retrieve("Id", "FileRef", "FileLeafRef", "FileSystemObjectType"),
                    items =>
                    {
                        currentCount += items.Count;
                        AnalyzeListItems(items, fullTree);
                    });

                watch.Stop();
                mLogger.Info("Cache folder structure with list {0} costs: {1}, Item Count: {2}, Expected Item Count: {3}", 
                        list.RootFolder.ServerRelativeUrl, watch.Elapsed, currentCount, list.ItemCount);
                return fullTree;
            }
        }

        void ProcessListItems(string webFullUrl, Guid listId, Action<ListItemCollection> itemsRetriever, Action<ListItemCollection> itemsProcessor)
        {
            var retryHelper = new AveTaskRetryHelper(3, true);
            retryHelper.SetRetryInterval(WrapperConfiguration.BPOS_S.RetryInterval * 3);
            
            int defaultRowLimit = maxItemsPerThrottledOperation > 0 ? Convert.ToInt32(maxItemsPerThrottledOperation) : 4500;
            int rowlimit = defaultRowLimit;
            CamlQuery query = new CamlQuery() { ViewXml= string.Format(AveCamlQuery.FORMAT_CAML_QUERY_ID, rowlimit) };
            ListItemCollectionPosition queryPosition = null;
            var pagingInfo = string.Empty;
            bool firsttime = true;
            try
            {
                do
                {
                    try
                    {                        
                        mLogger.Info($"ProcessListItems with row limit {rowlimit},pagingInfo:{pagingInfo}");
                        retryHelper.ExecuteWithRetryMechanism(() =>
                        {
                            using (var context = CreateDiscoverContext(webFullUrl))
                            {
                                var web = context.Web;
                                var list = web.Lists.GetById(listId);
                                var listItems = list.GetItems(query);
                                context.Load(listItems,
                                    items => items.ListItemCollectionPosition,
                                    items => items.Include(item => item.Id));
                                if (itemsRetriever != null)
                                {
                                    itemsRetriever(listItems);
                                }
                                context.ExecuteQuery();
                                itemsProcessor(listItems);

                                queryPosition = listItems.ListItemCollectionPosition;
                                if (queryPosition != null)
                                {
                                    /*if query contains lookup column filter last batch returns null 
                                     by removing the lookup column in paginginfo query will return next records
                                     */
                                    pagingInfo = queryPosition.PagingInfo;
                                    string[] parameters = pagingInfo.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                    List<string> requiredParameters = new List<string>();
                                    foreach (string str in parameters)
                                    {
                                        if (str.Contains("Paged=") || str.Contains("p_ID="))
                                            requiredParameters.Add(str);
                                    }

                                    pagingInfo = string.Join("&", requiredParameters.ToArray());
                                    queryPosition.PagingInfo = pagingInfo;
                                    query.ListItemCollectionPosition = listItems.ListItemCollectionPosition;
                                }
                            }
                        });
                        firsttime = false;
                    }
                    catch (ServerException ex)
                    {
                        rowlimit /= 2;
                        if (ExceptionHandleUtil.HandleBatchExecuteException(ex, ref defaultRowLimit, ref rowlimit) || rowlimit == 0)
                        {
                            throw;
                        }
                        query.ViewXml = string.Format(AveCamlQuery.FORMAT_CAML_QUERY_ID, rowlimit);
                    }
                   
                }
                while (queryPosition != null || firsttime);
            }
            catch (Exception e)
            {
                mLogger.Error($"Failed query list items with page: {pagingInfo}, error: {e}");
                throw;
            }
        }

        void AnalyzeListItems(ListItemCollection items, SPOFolder rootFolder)
        {
            foreach (var item in items)
            {
                var serverRelativeUrl = (string)item.FieldValues["FileRef"];
                var name = (string)item.FieldValues["FileLeafRef"];

                var parentFolder = rootFolder;
                var frUrl = serverRelativeUrl.Substring(rootFolder.Name.Length, serverRelativeUrl.Length - rootFolder.Name.Length -name.Length - 1);
                var parentFoldersName = frUrl.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < parentFoldersName.Length; i++)
                {
                    var folderName = parentFoldersName[i];
                    SPOFolder tempFolder = null;
                    if (parentFolder.SubFolders == null)
                    {
                        parentFolder.SubFolders = new List<SPOFolder>();
                    }
                    else
                    {
                        tempFolder = parentFolder.SubFolders.FirstOrDefault(f =>
                                string.Equals(f.Name, folderName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (tempFolder == null)
                    {
                        tempFolder = new SPOFolder() { Name = folderName };
                        parentFolder.SubFolders.Add(tempFolder);
                    }
                    parentFolder = tempFolder;
                }

                var id = item.Id;
                if (item.FileSystemObjectType == FileSystemObjectType.File)
                {
                    if (parentFolder.Items == null)
                    {
                        parentFolder.Items = new List<SPOItem>();
                    }

                    var spoItem = new SPOItem()
                    {
                        Id = id,
                        Name = name
                    };
                    parentFolder.Items.Add(spoItem);
                }
                else
                {
                    if (parentFolder.SubFolders == null)
                    {
                        parentFolder.SubFolders = new List<SPOFolder>();
                    }

                    var spoFolder = parentFolder.SubFolders.FirstOrDefault(f =>
                        string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (spoFolder == null)
                    {
                        spoFolder = new SPOFolder()
                        {
                            Id = id,
                            Name = name,
                        };
                        parentFolder.SubFolders.Add(spoFolder);
                    }
                    else
                    {
                        spoFolder.Id = id;
                    }
                }
            }
        }

        int AnalyzeListItems(ListItemCollection items, SPOChangeFolder changeFolder, Func<ListItem, bool> filterOut = null, AveChangeType changeType = AveChangeType.Edit)
        {
            var count = 0;
            foreach (var item in items)
            {
                if (filterOut != null && filterOut(item)) continue;
                count++;
                var serverRelativeUrl = (string)item.FieldValues["FileRef"];
                var name = (string)item.FieldValues["FileLeafRef"];

                var parentFolder = changeFolder;
                var frUrl = serverRelativeUrl.Substring(changeFolder.Name.Length, serverRelativeUrl.Length - changeFolder.Name.Length - name.Length - 1);
                var parentFoldersName = frUrl.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parentFoldersName.Length; i++)
                {
                    var folderName = parentFoldersName[i];
                    SPOChangeFolder tempFolder = null;
                    if (parentFolder.Folders == null)
                    {
                        parentFolder.Folders = new List<SPOChangeFolder>();
                    }
                    else
                    {
                        tempFolder = parentFolder.Folders.FirstOrDefault(f =>
                                string.Equals(f.Name, folderName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (tempFolder == null)
                    {
                        tempFolder = new SPOChangeFolder() { Name = folderName, ChangeType = changeType };
                        parentFolder.Folders.Add(tempFolder);
                    }
                    parentFolder = tempFolder;
                }

                var id = item.Id;
                if (item.FileSystemObjectType == FileSystemObjectType.File)
                {
                    if (parentFolder.Items == null)
                    {
                        parentFolder.Items = new List<SPOChangeItem>();
                    }

                    var spoItem = new SPOChangeItem()
                    {
                        Id = id,
                        Name = name,
                        ChangeType = changeType,
                    };
                    parentFolder.Items.Add(spoItem);
                }
                else
                {
                    if (parentFolder.Folders == null)
                    {
                        parentFolder.Folders = new List<SPOChangeFolder>();
                    }

                    var spoFolder = parentFolder.Folders.FirstOrDefault(f =>
                        string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (spoFolder == null)
                    {
                        spoFolder = new SPOChangeFolder()
                        {
                            Id = id,
                            Name = name,
                            ChangeType = changeType,
                        };
                        parentFolder.Folders.Add(spoFolder);
                    }
                    else
                    {
                        spoFolder.Id = id;
                    }
                }
            }
            return count;
        }
    }
}