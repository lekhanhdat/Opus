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
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request
    {
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool isDiscover, bool includeSystemFolder = false)
        {
            return base.QueryListItemForFB(siteId, webId, listId, folderId, folderUrl, isDiscover, includeSystemFolder);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changeItemsCache)
        {
            return base.QueryListItemForIB(siteId, webId, listId, folderId, folderUrl, changeItemsCache);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<byte[], object> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            return base.QueryWebContentTypeForFB(siteId, webId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> QueryWebRootFolder(Guid webId)
        {
            return base.QueryWebRootFolder(webId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> QueryCurrentFolder(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, string listUrl)
        {
            return base.QueryCurrentFolder(siteId, webId, listId, folderId, folderUrl, listUrl);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> QueryListRootFolder(Guid siteId, Guid webId, Guid mListID)
        {
            return base.QueryListRootFolder(siteId, webId, mListID);
        }
        [NoAPI]
        public override Dictionary<Guid, object> QueryListAlertForIB(Guid siteId, Guid webId, Guid mListID)
        {
            return base.QueryListAlertForIB(siteId, webId, mListID);
        }
        [NoAPI]
        public override Dictionary<Guid, object> QueryListViewForIB(Guid siteId, Guid webId, Guid mListID)
        {
            return base.QueryListViewForIB(siteId, webId, mListID);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<Guid, object> QueryWebListForFB(Guid siteId, Guid webId)
        {
            return base.QueryWebListForFB(siteId, webId);
        }
        [KeepOriginalWithAPI]
        public override int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache)
        {
            return base.GetSiteChangedForIB(siteId, startTime, endTime, changeCache);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<Guid, object> QueryWebForIB(Dictionary<Guid, object> changedWebsInfo)
        {
            return base.QueryWebForIB(changedWebsInfo);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<int, object> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            return base.QuerySiteSecurityForIB(siteId, startTime, endTime);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> QueryRootWeb(Guid siteId)
        {
            return base.QueryRootWeb(siteId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId)
        {
            return base.GetSubWebs(siteId, parentWebId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<Guid, Dictionary<string, object>> GetSubWebsBasicInfo(string siteUrl, Guid parentWebId)
        {
            return base.GetSubWebsBasicInfo(siteUrl, parentWebId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<Guid, object> changedListCache)
        {
            return base.QueryListForIB(webId, changedListCache);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<Guid, object> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            return base.QueryListViewForFB(siteId, webId, listId);
        }
        [ReplaceByAPI]
        public override Dictionary<string, object> DiscoverAllListContent(Guid siteId, Guid webId, Guid listId, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWebById(webId);
                if (listId == Guid.Empty)
                {
                    return DiscoverSystemListAllContent(context, web, includeSystemFolder);
                }
                else
                {
                    var list = web.Lists.GetById(listId);
                    return DiscoverNormalListAllContent(context, web, list, maxItemCount);
                }
            }
        }
        private Dictionary<string, object> DiscoverSystemListAllContent(AveClientContext context, Web web, bool includeSystemFolder)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["Items"] = new List<Dictionary<string, object>>();
            result["Folders"] = new List<Dictionary<string, object>>();
            context.Load(web, tempWeb => tempWeb.ServerRelativeUrl);
            context.ExecuteQuery();
            DiscoverAllListContentForSystemList(result, web.ServerRelativeUrl, web.ServerRelativeUrl, includeSystemFolder);
            (result["Items"] as List<Dictionary<string, object>>).ForEach((item) =>
            {
                List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                AssembleWebItemVersionProperty(item, versions);
                item["HasVersion"] = false;
            });
            return result;
        }
        private void DiscoverAllListContentForSystemList(Dictionary<string, object> result,string webServerRelativeUrl, string paretFolderUrl, bool includeSystemFolder)
        {
            var items = result["Items"] as List<Dictionary<string, object>>;
            var folders = result["Folders"] as List<Dictionary<string, object>>;
            var subFiles = GetFiles(paretFolderUrl, null, paretFolderUrl);
            var subFolders = GetFolders(webServerRelativeUrl, null, Guid.Empty, paretFolderUrl != "/" ? "/" + paretFolderUrl.TrimStart('/') : "/", includeSystemFolder);
            items.AddRange(subFiles[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>);
            folders.AddRange(subFolders[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>);
            foreach (var folder in subFolders[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>)
            {
                DiscoverAllListContentForSystemList(result, webServerRelativeUrl, "", includeSystemFolder);
            }
        }
        private Dictionary<string, object> DiscoverNormalListAllContent(AveClientContext context, Web web, List list, int maxItemCount)
        {
            context.Load(web, w => w.ServerRelativeUrl);
            context.Load(list, l => l.BaseType, l => l.BaseTemplate, l => l.ItemCount, l => l.Views, l => l.RootFolder.ServerRelativeUrl, l => l.Id);
            context.ExecuteQuery();

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["Items"] = new List<Dictionary<string, object>>();
            result["Folders"] = new List<Dictionary<string, object>>();

            List<string> viewFields = new List<string> { "FileDirRef", "FileLeafRef", "Title", "GUID", "_UIVersion", "Modified_x0020_By", "Created_x0020_By" };
            if (list.BaseType != BaseType.DocumentLibrary) viewFields.Add("Attachments");
            if (list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard)
            {
                viewFields.Add("ThreadIndex");
                viewFields.Add("ParentFolderId");
            }
            if (list.ItemCount > maxItemCount)
            {
                throw new Exception(string.Format("Current items count: {0}, max discover item count: {1}, list: {2}", list.ItemCount, maxItemCount, list.RootFolder.ServerRelativeUrl));
            }
            if (!IsThrottled(list.ItemCount))
            {
                QueryListAllContentForSmallList(context, list, viewFields, result);
            }
            else
            {
                QueryListAllContentForLargeList(context, list, viewFields, result);
            }
            EnsureParentThreadId(list, result["Items"] as List<Dictionary<string, object>>);
            QueryListViewItems(context, web, list, list.RootFolder, result);
            QueryItemExtentionInfo( web, list, result);
            return result;
        }
        private void QueryListViewItems(ClientContext context, Web web, List list, Folder parentFolder, Dictionary<string, object> result)
        {
            var items = result["Items"] as List<Dictionary<string, object>>;
            var folders = new List<Dictionary<string, object>>();
            AddViewItems(context, list, parentFolder, parentFolder.ServerRelativeUrl, items, folders, web.ServerRelativeUrl);
            (result["Folders"] as List<Dictionary<string, object>>).AddRange(folders);
            foreach(var subFolder in folders)
            {
                var clientSubFolder = GetFolderByAPI(web, subFolder["ServerRelativeUrl"] as string);
                context.Load(clientSubFolder, folder => folder.ServerRelativeUrl, Folder => Folder.ListItemAllFields);
                context.ExecuteQuery();
                QueryListViewItems(context, web, list, clientSubFolder, result);
            }
        }
        private void QueryListAllContentForSmallList(ClientContext context, List list, List<string> viewFields, Dictionary<string, object> result)
        {
            var items = result["Items"] as List<Dictionary<string, object>>;
            var folders = result["Folders"] as List<Dictionary<string, object>>;
            ListItemCollectionPosition pos = null;
            do
            {
                var camlQuery = new CamlQuery()
                {
                    ViewXml = AveCamlQueryString.GetAllItemsString(viewFields, (int)this.MaxItemsPerThrottledOperation, QueryFindOption.RecursiveAll),
                    ListItemCollectionPosition = pos,
                };
                var listItems = list.GetItems(camlQuery);
                context.Load(listItems, collection => collection.ListItemCollectionPosition,
                                        collection => collection.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                context.ExecuteQuery();
                foreach (ListItem item in listItems)
                {
                    if ((string)item["FSObjType"] == "0")
                    {
                        items.Add(AssemblyItemInfoWIthoutAddToCache(context, list.BaseType == BaseType.DocumentLibrary, item));
                    }
                    else
                    {
                        folders.Add(AssemblyFolderInfoWithoutAddToCache(context, item));
                    }
                }
                pos = listItems.ListItemCollectionPosition;
            }
            while (pos != null);
        }
        private void QueryListAllContentForLargeList(ClientContext context, List list, List<string> viewFields, Dictionary<string, object> result)
        {
            var items = result["Items"] as List<Dictionary<string, object>>;
            var folders = result["Folders"] as List<Dictionary<string, object>>;

            var worker = new LargeListQueryWorker(context, list, list.RootFolder.ServerRelativeUrl, this.MaxItemsPerThrottledOperation, GetFolderByAPI, null, viewFields, QueryFindOption.RecursiveAll);
            worker.BeforeQueryAction += (contextArg, listItemsArg) =>
            {
                contextArg.Load(listItemsArg, collection => collection.ListItemCollectionPosition,
                              collection => collection.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
            };
            worker.AfterQueryAction += (contextArg, itemArg, isLibraryArg) =>
            {
                if ((string)itemArg["FSObjType"] == "0")
                {
                    items.Add(AssemblyItemInfoWIthoutAddToCache(contextArg, isLibraryArg, itemArg));
                }
                else
                {
                    folders.Add(AssemblyFolderInfoWithoutAddToCache(contextArg, itemArg));
                }
            };
            worker.ExceptionWhenQueryAction = () =>
            {
                result.Clear();
            };
            worker.SetCamlQueryUrl = SetCamlQueryFolderUrl;
            mLogger.Debug("Begin discover folders and items in large list, list.ItemCount:{0}, folder URL:{1}.", list.ItemCount, list.RootFolder.ServerRelativeUrl);
            worker.Run();
            context.ExecuteQuery();
            mLogger.Debug("Finish discover folders and items in large list, ItemCount: {0}, FolderCount: {1}", items.Count, folders.Count);
        }
        /// <summary>
        /// 获取list item的其他信息，目前有Version和Attachment。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="web"></param>
        /// <param name="list"></param>
        /// <param name="objects"></param>
        private void QueryItemExtentionInfo( Web web, List list, Dictionary<string, object> objects)
        {
            bool isLibrary = list.BaseType == BaseType.DocumentLibrary;
            var items = objects["Items"] as List<Dictionary<string, object>>;
            var folders = objects["Folders"] as List<Dictionary<string, object>>;
            List<Dictionary<string, object>> itemsAndFolders = new List<Dictionary<string, object>>();
            itemsAndFolders.AddRange(items);
            itemsAndFolders.AddRange(folders);

            List<Task> getItemExtentionInfoTasks = new List<Task>();
            mLogger.Debug("Start discover item versions and attachments.");
            itemsAndFolders.ForEach((item) =>
            {
                getItemExtentionInfoTasks.Add(() =>
                {
                    item["Attachments"] = new List<Dictionary<string, object>>();
                    Folder attachmentFolder;
                    var clientItem = GetAndLoadListItemForDiscover(web.ServerRelativeUrl, list, item, out attachmentFolder);
                    if (attachmentFolder != null)
                    {
                        AssemblyAttachmentInfo(web, attachmentFolder, item);
                    }
                    if (clientItem != null && clientItem.IsObjectPropertyInstantiated("Versions"))
                    {
                        item["Versions"] = new List<Dictionary<string, object>>();
                        AssemblyVersionInfo(clientItem.Versions, item);
                    }
                    else
                    {
                        List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                        AssembleWebItemVersionProperty(item, versions);
                        item["HasVersion"] = false;
                    }
                });
            }
            );
            if (getItemExtentionInfoTasks.Count > 0)
            {
                using (AveTaskExecutor taskExecutor = new AveTaskExecutor(WrapperConfiguration.BPOS_S.MaximumThreadsGettingVersions))
                {
                    taskExecutor.Execute(getItemExtentionInfoTasks);
                }
            }
            mLogger.Debug("Finish discover item versions and attachments.");
        }
        private ListItem GetAndLoadListItemForDiscover(string webUrl, List list, Dictionary<string, object> item, out Folder attachmentFolder)
        {
            ListItem clientItem = null;
            attachmentFolder = null;
            if (!item.ContainsKey("Id") || !(item["Id"] is int) || (int)item["Id"] <= 0)
            {
                return clientItem;
            }
            using (ClientContext ct = CreateContext())//不支持多线程，重新new context.
            {
                //目前不获取Folder的version信息。
                Web web = null;
                if (WrapperConfiguration.BPOS_S.IncludeVersionForPerformance && (ItemType)item["ObjType"] != ItemType.Folder && ItemHasVersion(list, item))
                {
                    web = ct.Site.OpenWeb(webUrl);
                    var newList = web.Lists.GetById(list.Id);
                    clientItem = newList.GetItemById((int)item["Id"]);
                    ct.Load(clientItem, i => i.Versions.Include(v => v["Modified"], v => v["Editor"], v => v["_Level"], v => v["_UIVersion"]));
                }
                if (list.BaseType != BaseType.DocumentLibrary && item.ContainsKey("Attachments" + AveObjectModelConstant.ObjectPropertySuffix)
                        && Convert.ToBoolean(item["Attachments" + AveObjectModelConstant.ObjectPropertySuffix]))
                {
                    web = web == null ? ct.Site.OpenWeb(webUrl) : web;
                    int id = (int)item["Id"];
                    string attachmentFolderUrl = list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/Attachments/" + id;
                    attachmentFolder = GetFolderByAPI(web, attachmentFolderUrl);
                    ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope((ct));
                    using (exceptionScope.StartScope())
                    {
                        using (exceptionScope.StartTry())
                        {
                            ct.Load(attachmentFolder,
                                    a => a.ServerRelativeUrl,
                                    a => a.Files,
                                    a => a.Files.Include(file => file.Author, file => file.ModifiedBy, file => file.CheckedOutByUser));
                        }
                        using (exceptionScope.StartCatch())
                        {
                            ct.Load(attachmentFolder,
                                    a => a.ServerRelativeUrl,
                                    a => a.Files);
                        }
                    }
                }
                if (ct.HasPendingRequest) ct.ExecuteQuery();
                return clientItem;
            }
        }
        private Dictionary<string, object> AssemblyItemInfoWIthoutAddToCache(ClientContext context, bool isFile, ListItem item)
        {
            //Do not need load Author and Editor during discover.
            //if (!item.FieldValues.ContainsKey("Author") && !item.FieldValues.ContainsKey("Editor")) //for community site discussion list
            //{
            //    context.Load(item);
            //    context.ExecuteQuery();
            //}
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, item);
            itemProperty["ObjType"] = ItemType.Item;
            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
            if (isFile)
            {
                itemProperty["CheckoutUserId"] = null;
                if (item.FieldValues.ContainsKey("FileRef") && !string.IsNullOrEmpty(item["FileRef"] as string))
                {
                    itemProperty["ServerRelativeUrl"] = item["FileRef"];
                }
                if (item.FieldValues.ContainsKey("CheckoutUser") && item["CheckoutUser"] as FieldUserValue != null)
                {
                    var checkoutuser = item["CheckoutUser"] as FieldUserValue;
                    itemProperty["CheckoutUserId"] = checkoutuser.LookupId;
                }

                itemProperty["ObjType"] = ItemType.Document;
            }
            return itemProperty;
        }
        private void AssemblyAttachmentInfo(Web web, Folder attachmentFolder, Dictionary<string, object> item)
        {
            var attachments = item["Attachments"] as List<Dictionary<string, object>>;
            foreach (File attachment in attachmentFolder.Files)
            {
                Dictionary<string, object> attachmentPro = new Dictionary<string, object>();
                string eTag = attachment.ETag.Trim('"');
                string[] pros = eTag.Split(',');
                attachmentPro["DocID"] = new Guid(pros[0]);
                attachmentPro["DirName"] = attachmentFolder.ServerRelativeUrl;
                attachmentPro["Name"] = attachmentPro["LeafName"] = attachment.Name;
                //attachmentPro["UIVersion"] = attachment.UIVersion;//统一为UIVersion
                attachmentPro["DocFlags"] = (int?)null;//cannot get this property
                                                       //attachmentPro["TimeLastModified"] = attachment.TimeLastModified;
                attachmentPro["Level"] = (byte)attachment.Level;
                attachmentPro["Type"] = (byte)FileSystemObjectType.File;
                //attachmentPro["Size"] = 0; //cannot get this property
                attachmentPro["ParentID"] = Guid.Empty;
                attachmentPro["FullUrl"] = attachmentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + attachmentPro["LeafName"];
                attachmentPro["CheckoutUserId"] = (int?)null;
                attachmentPro["HasStream"] = true;
                attachmentPro["RbsId"] = null;
                //attachmentPro["ServerRelativeUrl"] = attachment.ServerRelativeUrl;
                attachmentPro["ID"] = (int)item["Id"];
                AssembleFileProperties(attachmentPro, attachment, web.ServerRelativeUrl, attachment.ListItemAllFields);
                attachmentPro["Size"] = attachmentPro.ContainsKey("Length") ? int.Parse(attachmentPro["Length"].ToString()) : 0;
                attachments.Add(attachmentPro);
            }
        }
        private void AssemblyVersionInfo(ListItemVersionCollection clientVersions, Dictionary<string, object> item)
        {
            Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
            needLoadFields.Add("_Level", "Integer");
            needLoadFields.Add("_UIVersion", "Integer");

            List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)item["Versions"];
            foreach (var clientVersion in clientVersions)
            {
                try
                {
                    Dictionary<string, object> versionFieldValues = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> fieldValue in clientVersion.FieldValues)
                    {
                        var value = fieldValue.Value;
                        if (string.Equals(fieldValue.Key, "Created_x0020_Date", StringComparison.Ordinal))
                        {
                            value = DateTime.Parse(value.ToString(), null, DateTimeStyles.AdjustToUniversal);
                        }
                        AssembleItemProperties(versionFieldValues, value, fieldValue.Key);
                    }
                    var version = GetNeedLoadFields(versionFieldValues, needLoadFields);


                    version["ID"] = (int)item["Id"];
                    version["GUID"] = new Guid(item["GUID"].ToString());
                    version["Size"] = 0;
                    version["ObjType"] = item["ObjType"];
                    version["TimeLastModified"] = version["Modified"];
                    int versionId = (int)version["VersionId"];
                    if (!version.ContainsKey("Level"))
                    {
                        version["Level"] = (byte)1;
                    }
                    version["UIVersion"] = version["VersionId"];
                    version["UserDataGuid"] = version["GUID"];
                    object fieldValues;
                    if (item.TryGetValue("FieldValues", out fieldValues) && fieldValues != null)
                    {
                        version["IsCurrentVersion"] = versionId == (int)((Dictionary<string, object>)fieldValues)["_UIVersion"];
                    }
                    else
                    {
                        version["IsCurrentVersion"] = versionId == (int)item["UIVersion"];
                    }
                    versions.Add(version);
                }
                catch(Exception )
                {

                }
            }

        }
    }
}
