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
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    public partial class AveClientOM2013Request
    {
        private const string FORMAT_CAML_QUERY_VALUE_INT = "<Value Type=\"Integer\">{0}</Value>";
        private const string FORMAT_CAML_QUERY_ITEM =
            "<View Scope=\"RecursiveAll\"><QueryOptions><QueryThrottleMode>Override</QueryThrottleMode></QueryOptions>" +
                "<Query><Where><In>" +
                    "<FieldRef Name=\"ID\"/><Values>{0}</Values>" +
                "</In></Where><OrderBy Override=\"TRUE\"><FieldRef Name=\"ID\" /></OrderBy></Query>" +
            "</View>";

        private bool IsListEnableVersions(List list)
        {
            return list.BaseTemplate != (int)AveListTemplateType.UserInformation && 
                (list.EnableMinorVersions || list.EnableVersioning);
        }

        public IEnumerable<Dictionary<string, object>> QueryFolderWithStructureForFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, IDictionary<string, string> fieldsNeedLoadOfVersion, bool includeSystemFolder = false)
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
                bool needGetVersion=false;
                using (var context = CreateRetryContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    folderServerRelativeUrl = "/" + folderServerRelativeUrl.TrimStart('/');
                    Folder folder = null;
                    try
                    {
                        folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));

                        context.Load(list, l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate,
                            l => l.EnableVersioning, l => l.EnableMinorVersions);
                        context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                        context.Load(folder, f => f.ListItemAllFields, f => f.ItemCount);
                        context.Load(context.Site,s=>s.MaxItemsPerThrottledOperation);
                        context.ExecuteQuery();

                        //Get normal folder
                        //needGetVersion = WrapperConfiguration.BPOS_S.IncludeVersionForPerformance &&
                        //    list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard && IsListEnableVersions(list);
                    }
                    catch (ServerException e)
                    {
                        mLogger.Error("Failed to get folder: {0} error code: {1} message: {2}", folderServerRelativeUrl, e.ServerErrorCode, e.Message);
                        if (e.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                        {
                            yield break;
                        }
                        throw;
                    }

                    SwitchListContext(list);
                    parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                    try
                    {
                        //Get system file firstly
                        GetSystemFoldersAndFiles(context, subFolderProp, subItemsProp, list, folder, web.ServerRelativeUrl, folderServerRelativeUrl, folder.ItemCount > context.Site.MaxItemsPerThrottledOperation);
                        //Add to Query View Item by Client API
                        AddViewItems(context, list, folderServerRelativeUrl, subItemsProp, subFolderProp, folder.ItemCount > context.Site.MaxItemsPerThrottledOperation);
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query system folders with listId:{0}, folderUrl:{1} Error: {2}", listId, folderServerRelativeUrl, e);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        //subFolderProp.Add(error);
                    }
                }
                foreach (var foldersProp in GetFolders(webId, listId, folderServerRelativeUrl, foldersId, needGetVersion, fieldsNeedLoadOfVersion))
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
                using (var context = CreateRetryContext())
                {
                    try
                    {
                        Web web = context.Site.OpenWebById(webId);
                        context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                        context.ExecuteQuery();
                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                        Dictionary<string, object> folders = GetFolders(web.ServerRelativeUrl, null, Guid.Empty, folderServerRelativeUrl != "/" ? "/" + folderServerRelativeUrl.TrimStart('/') : "/", includeSystemFolder);
                        foreach (Dictionary<string,object> folder in folders.GetChildren())
                        {
                            //This is for AveQuery GetFolderStructureFromParent in FillFolderObject
                            //need improve
                            folder["IsSystemFile"] = true;
                            subFolderProp.Add(folder);
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query web folders with webId:{0}, folderUrl:{1}", webId, folderServerRelativeUrl);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        //subFolderProp.Add(error);
                    }
                }
                yield return parentFolder;
            }

            mIsLoadDFolderId = false;
        }

        private IEnumerable<List<Dictionary<string, object>>> GetFolders(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, bool needGetVersion, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            int versionCount = WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount == -1 ? int.MaxValue : WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount + 1;
            foreach (var foldersIdRange in GetCamlQueryIdRange(foldersId, false))
            {
                using (var context = CreateRetryContext())
                {
                    List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                    try
                    {
                        CamlQuery query = new CamlQuery();
                        StringBuilder values = new StringBuilder();
                        if(foldersIdRange != null)
                        {
                            foreach (var id in foldersIdRange)
                            {
                                values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
                            }
                        }
                        query.ViewXml = string.Format(FORMAT_CAML_QUERY_ITEM, values);

                        Stopwatch watch = Stopwatch.StartNew();
                        ListItemCollection listItems = null;
                        bool onebyOne = false;
                        try
                        {
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            listItems = list.GetItems(query);
                            context.Load(list, l => l.BaseTemplate, l => l.RootFolder.ServerRelativeUrl);
                            context.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                            if (needGetVersion)
                            {
                                context.Load(listItems, items => items.Include(item => item.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                    v => v.CreatedBy, v => v.FileVersion.CheckInComment, v => v.FileVersion.Created)));
                            }
                            context.ExecuteQuery();
                            foreach (var item in listItems)
                            {
                                results.Add(AssembleFolderProperties(list, item, fieldsNeedLoadOfVersion));
                            }
                        }
                        catch (Exception e)
                        {
                            string foldersIdStr = string.Join(",", foldersIdRange);
                            var exception = e as ServerException;
                            if (exception == null)
                            {
                                mLogger.Error("Failed to query folders with id range: {0} error: {1}", foldersIdStr, e);
                            }
                            else
                            {
                                mLogger.Error("Failed to query folders with id range: {0} error code: {1} message: {2}",
                                    foldersIdStr, exception.ServerErrorCode, exception.Message);
                                if (exception.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                {
                                    throw;
                                }
                            }
                            onebyOne = true;
                        }

                        if (onebyOne)
                        {
                            results.Clear();
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            context.Load(list, l=>l.BaseTemplate, l=>l.RootFolder.ServerRelativeUrl);
                            context.ExecuteQuery();

                            foreach (var folderId in foldersId)
                            {
                                ListItem item = null;
                                try
                                {
                                    item = list.GetItemById(folderId);
                                    context.Load(item);
                                    context.Load(item.RoleAssignments);
                                    if (needGetVersion)
                                    {
                                        context.Load(item, i => i.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                            v => v.CreatedBy, v => v.FileVersion.CheckInComment, v => v.FileVersion.Created));
                                    }
                                    context.ExecuteQuery();
                                    results.Add(AssembleFolderProperties(list, item, fieldsNeedLoadOfVersion));
                                }
                                catch (Exception e)
                                {
                                    var serverException = e as ServerException;
                                    if (serverException == null)
                                    {
                                        mLogger.Warn("Load folder failed by id: {0} parent folder: {1} error: {2}",
                                            folderId, folderServerRelativeUrl, e);
                                    }
                                    else
                                    {
                                        mLogger.Warn("Load folder failed by id: {0} parent folder: {1} error code: {2} message: {3}",
                                            folderId, folderServerRelativeUrl, serverException.ServerErrorCode, serverException.Message);

                                        if (serverException.ServerErrorCode == AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST)
                                        {
                                            //Item may have been deleted by another user.
                                            continue;
                                        }
                                        //else if (serverException.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                        //{
                                        //    throw;
                                        //}
                                    }
                                    throw;
                                }
                            }
                        }

                        watch.Stop();
                        mLogger.Info("Load folders with structure under folder: {0} costs: {1} count: {2}",
                            folderServerRelativeUrl, watch.Elapsed, results.Count);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Failed to query folders under folder:{0} with id range:{1} Error: {2}",
                            folderServerRelativeUrl, string.Join(", ", foldersIdRange), ex);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = ex;
                        results.Add(error);
                    }
                    yield return results; 
                }
            }
        }

        private Dictionary<string, object> AssembleFolderProperties(List list, ListItem folder, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, folder);
            itemProperty["ObjType"] = ItemType.Folder;
            itemProperty["ItemId"] = itemProperty["Id"];
            itemProperty["Hidden"] = (itemProperty["Id"] == null) ? true : false;
            itemProperty["Items"] = new List<Dictionary<string, object>>();
            itemProperty["Folders"] = new List<Dictionary<string, object>>();

            if (list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard)
            {
                AssembleVersionsProperties(itemProperty, folder, fieldsNeedLoadOfVersion);
            }

            //TODO
            //1.what kind of folders have attachment exectly?
            //2.Is there an other efficiency way to get attachment?
            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = folder.FieldValues.ContainsKey("Attachments") ?
                folder.FieldValues["Attachments"] : false;
            itemProperty["Attachments"] = new List<Dictionary<string, object>>();
            GetAttachmentsFromItem(list.Context as ClientContext, list, itemProperty, list.RootFolder.ServerRelativeUrl);
            return itemProperty;
        }

        private Dictionary<string, object> AssembleFolderPropertiesForArchiver(List list, ListItem folder, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, folder);
            itemProperty["ObjType"] = ItemType.Folder;
            itemProperty["ItemId"] = itemProperty["Id"];
            itemProperty["Hidden"] = (itemProperty["Id"] == null) ? true : false;
            //itemProperty["Items"] = new List<Dictionary<string, object>>();
            itemProperty["Folders"] = new List<Dictionary<string, object>>();

            if (list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard)
            {
                AssembleVersionsProperties(itemProperty, folder, fieldsNeedLoadOfVersion);
            }
            itemProperty["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProperty;
            //TODO
            //1.what kind of folders have attachment exectly?
            //2.Is there an other efficiency way to get attachment?
            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = folder.FieldValues.ContainsKey("Attachments") ?
                folder.FieldValues["Attachments"] : false;
            itemProperty["Attachments"] = new List<Dictionary<string, object>>();
            GetAttachmentsFromItem(list.Context as ClientContext, list, itemProperty, list.RootFolder.ServerRelativeUrl);

            this.mCurrentList.Folders[Convert.ToInt32(itemProperty["Id"])] = itemProperty;
            return itemProperty;
        }

        public IEnumerable<Dictionary<string, object>> QueryItemWithStructureForFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> itemsId, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            mIsLoadDFolderId = true;
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            var subitemsProp = new List<Dictionary<string, object>>();
            parentFolder["Items"] = subitemsProp;
            if (listId != Guid.Empty)
            {
                bool needGetVersion;
                bool isDocumentLib;
                using (var context = CreateRetryContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(context.Site,s=>s.MaxItemsPerThrottledOperation);
                    context.Load(web, w => w.ServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    folderServerRelativeUrl = "/" + folderServerRelativeUrl.TrimStart('/');
                    Folder folder = null;
                    try
                    {
                        folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                        context.Load(list, l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate,
                            l => l.EnableVersioning, l => l.EnableMinorVersions);
                        context.Load(list.RootFolder, f => f.ServerRelativeUrl);
                        context.Load(folder, f => f.ListItemAllFields, f => f.ItemCount);
                        context.ExecuteQuery();
                        needGetVersion = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance; 
                            //&& IsListEnableVersions(list);
                        isDocumentLib = list.BaseType == BaseType.DocumentLibrary;
                    }
                    catch (ServerException e)
                    {
                        mLogger.Error("Failed to get folder: {0} error code: {1} message: {2}", folderServerRelativeUrl, e.ServerErrorCode, e.Message);
                        if (e.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                        {
                            yield break;
                        }
                        throw;
                    }
                    var mMaxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation;
                    SwitchListContext(list);
                    parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                    //get system file firstly
                    try
                    {
                        GetSystemFiles(context, subitemsProp, list, folder, folderServerRelativeUrl, mMaxItemsPerThrottledOperation);
                        AddViewItems(context, list, folderServerRelativeUrl, subitemsProp, mMaxItemsPerThrottledOperation);
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query system files with listId:{0}, folderUrl:{1} Error: {2}", listId, folderServerRelativeUrl, e);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        subitemsProp.Add(error);
                    }
                }
                foreach (var itemsProp in GetItems(webId, listId, folderServerRelativeUrl, itemsId, needGetVersion, isDocumentLib, fieldsNeedLoadOfVersion))
                {
                    subitemsProp.AddRange(itemsProp);
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
                using (var context = CreateRetryContext())
                {
                    try
                    {
                        Web web = context.Site.OpenWebById(webId);
                        context.Load(web, w => w.ServerRelativeUrl);
                        context.ExecuteQuery();

                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                        Dictionary<string, object> files = GetFiles(web.ServerRelativeUrl, null, folderServerRelativeUrl != "/" ? "/" + folderServerRelativeUrl.TrimStart('/') : "/");
                        foreach (Dictionary<string,object> item in files.GetChildren())
                        {
                            List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                            AssembleWebItemVersionProperty(item, versions);
                            item["HasVersion"] = false;
                            webItems.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query web files with webId:{0}, folderUrl:{1}", webId, folderServerRelativeUrl);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        //webItems.Add(error);
                    }
                    yield return parentFolder;
                }
            }

            mIsLoadDFolderId = false;
        }

        public IEnumerable<Dictionary<string, object>> QueryFolderWithStructureForArchiverFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, IDictionary<string, string> fieldsNeedLoadOfVersion, bool includeSystemFolder = false)
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
                using (var context = CreateRetryContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    folderServerRelativeUrl = "/" + folderServerRelativeUrl.TrimStart('/');
                    Folder folder = null;
                    try
                    {
                        folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));

                        context.Load(list, l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate,
                            l => l.EnableVersioning, l => l.EnableMinorVersions);
                        context.Load(list.RootFolder, r => r.ServerRelativeUrl);
                        context.Load(folder, f => f.ListItemAllFields, f => f.ItemCount);
                        context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                        context.ExecuteQuery();

                        //Get normal folder
                        //needGetVersion = WrapperConfiguration.BPOS_S.IncludeVersionForPerformance &&
                        //    list.BaseTemplate == (int)AveListTemplateType.DiscussionBoard && IsListEnableVersions(list);
                    }
                    catch (ServerException e)
                    {
                        mLogger.Error("Failed to get folder: {0} error code: {1} message: {2}", folderServerRelativeUrl, e.ServerErrorCode, e.Message);
                        if (e.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                        {
                            yield break;
                        }
                        throw;
                    }

                    SwitchListContext(list);
                    parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                    try
                    {
                        //Get system file firstly
                        GetSystemFoldersAndFiles(context, subFolderProp, subItemsProp, list, folder, web.ServerRelativeUrl, folderServerRelativeUrl, folder.ItemCount > context.Site.MaxItemsPerThrottledOperation);
                        //Add to Query View Item by Client API
                        AddViewItems(context, list, folderServerRelativeUrl, subItemsProp, subFolderProp, folder.ItemCount > context.Site.MaxItemsPerThrottledOperation);
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query system folders with listId:{0}, folderUrl:{1} Error: {2}", listId, folderServerRelativeUrl, e);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        //subFolderProp.Add(error);
                    }
                }
                foreach (var foldersProp in GetFoldersForArchiver(webId, listId, folderServerRelativeUrl, foldersId, needGetVersion, fieldsNeedLoadOfVersion))
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
                using (var context = CreateRetryContext())
                {
                    try
                    {
                        Web web = context.Site.OpenWebById(webId);
                        context.Load(web, tmpWeb => tmpWeb.ServerRelativeUrl);
                        context.ExecuteQuery();
                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;
                        Dictionary<string, object> folders = GetFolders(web.ServerRelativeUrl, null, Guid.Empty, folderServerRelativeUrl != "/" ? "/" + folderServerRelativeUrl.TrimStart('/') : "/", includeSystemFolder);
                        foreach (Dictionary<string, object> folder in folders.GetChildren())
                        {
                            //This is for AveQuery GetFolderStructureFromParent in FillFolderObject
                            //need improve
                            folder["IsSystemFile"] = true;
                            subFolderProp.Add(folder);
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query web folders with webId:{0}, folderUrl:{1}", webId, folderServerRelativeUrl);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        //subFolderProp.Add(error);
                    }
                }
                yield return parentFolder;
            }

            mIsLoadDFolderId = false;
        }

        private IEnumerable<List<Dictionary<string, object>>> GetFoldersForArchiver(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, bool needGetVersion, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            int versionCount = WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount == -1 ? int.MaxValue : WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount + 1;
            foreach (var foldersIdRange in GetCamlQueryIdRangeForArchiver(foldersId, false))
            {
                using (var context = CreateRetryContext())
                {
                    List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                    try
                    {
                        CamlQuery query = new CamlQuery();
                        StringBuilder values = new StringBuilder();
                        if (foldersIdRange != null)
                        {
                            foreach (var id in foldersIdRange)
                            {
                                values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
                            }
                        }
                        ArgumentCheck.CheckNotNull(foldersIdRange);
                        foreach (var id in foldersIdRange)
                        {
                            values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
                        }
                        query.ViewXml = string.Format(FORMAT_CAML_QUERY_ITEM, values);

                        Stopwatch watch = Stopwatch.StartNew();
                        ListItemCollection listItems = null;
                        bool onebyOne = false;
                        try
                        {
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            listItems = list.GetItems(query);
                            context.Load(list, l => l.BaseTemplate, l => l.RootFolder.ServerRelativeUrl);
                            context.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                            if (needGetVersion)
                            {
                                context.Load(listItems, items => items.Include(item => item.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                    v => v.CreatedBy, v => v.FileVersion.CheckInComment, v => v.FileVersion.Created)));
                            }
                            context.ExecuteQuery();
                            foreach (var item in listItems)
                            {
                                results.Add(AssembleFolderPropertiesForArchiver(list, item, fieldsNeedLoadOfVersion));
                            }
                        }
                        catch (Exception e)
                        {
                            string foldersIdStr = string.Join(",", foldersIdRange);
                            var exception = e as ServerException;
                            if (exception == null)
                            {
                                mLogger.Error("Failed to query folders with id range: {0} error: {1}", foldersIdStr, e);
                            }
                            else
                            {
                                mLogger.Error("Failed to query folders with id range: {0} error code: {1} message: {2}",
                                    foldersIdStr, exception.ServerErrorCode, exception.Message);
                                if (exception.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                {
                                    throw;
                                }
                            }
                            onebyOne = true;
                        }

                        if (onebyOne)
                        {
                            results.Clear();
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            context.Load(list, l => l.BaseTemplate, l => l.RootFolder.ServerRelativeUrl);
                            context.ExecuteQuery();

                            foreach (var folderId in foldersId)
                            {
                                ListItem item = null;
                                try
                                {
                                    item = list.GetItemById(folderId);
                                    context.Load(item);
                                    context.Load(item.RoleAssignments);
                                    if (needGetVersion)
                                    {
                                        context.Load(item, i => i.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                            v => v.CreatedBy, v => v.FileVersion.CheckInComment, v => v.FileVersion.Created));
                                    }
                                    context.ExecuteQuery();
                                    results.Add(AssembleFolderPropertiesForArchiver(list, item, fieldsNeedLoadOfVersion));
                                }
                                catch (Exception e)
                                {
                                    var serverException = e as ServerException;
                                    if (serverException == null)
                                    {
                                        mLogger.Warn("Load folder failed by id: {0} parent folder: {1} error: {2}",
                                            folderId, folderServerRelativeUrl, e);
                                    }
                                    else
                                    {
                                        mLogger.Warn("Load folder failed by id: {0} parent folder: {1} error code: {2} message: {3}",
                                            folderId, folderServerRelativeUrl, serverException.ServerErrorCode, serverException.Message);

                                        if (serverException.ServerErrorCode == AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST)
                                        {
                                            //Item may have been deleted by another user.
                                            continue;
                                        }
                                        //else if (serverException.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                        //{
                                        //    throw;
                                        //}
                                    }
                                    throw;
                                }
                            }
                        }

                        watch.Stop();
                        mLogger.Info("Load folders with structure under folder: {0} costs: {1} count: {2}",
                            folderServerRelativeUrl, watch.Elapsed, results.Count);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Failed to query folders under folder:{0} with id range:{1} Error: {2}",
                            folderServerRelativeUrl, string.Join(", ", foldersIdRange), ex);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = ex;
                        results.Add(error);
                    }
                    yield return results;
                }
            }
        }

        public IEnumerable<Dictionary<string, object>> QueryItemWithStructureForArchiverFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> itemsId, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            mIsLoadDFolderId = true;
            Dictionary<string, object> parentFolder = new Dictionary<string, object>();
            var subitemsProp = new List<Dictionary<string, object>>();
            parentFolder["Items"] = subitemsProp;
            if (listId != Guid.Empty)
            {
                bool needGetVersion;
                bool isDocumentLib;
                using (var context = CreateRetryContext())
                {
                    Web web = context.Site.OpenWebById(webId);
                    context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                    context.Load(web, w => w.ServerRelativeUrl);
                    List list = web.Lists.GetById(listId);
                    folderServerRelativeUrl = "/" + folderServerRelativeUrl.TrimStart('/');
                    Folder folder = null;
                    try
                    {
                        folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                        context.Load(list, l => l.Id, l => l.Title, l => l.BaseType, l => l.BaseTemplate,
                            l => l.EnableVersioning, l => l.EnableMinorVersions);
                        context.Load(list.RootFolder, f => f.ServerRelativeUrl);
                        context.Load(folder, f => f.ListItemAllFields, f => f.ItemCount);
                        context.ExecuteQuery();
                        needGetVersion = WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance;
                            //&& IsListEnableVersions(list);
                        isDocumentLib = list.BaseType == BaseType.DocumentLibrary;
                    }
                    catch (ServerException e)
                    {
                        mLogger.Error("Failed to get folder: {0} error code: {1} message: {2}", folderServerRelativeUrl, e.ServerErrorCode, e.Message);
                        if (e.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                        {
                            yield break;
                        }
                        throw;
                    }
                    var mMaxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation;
                    //Archiver外围实例化DiscoverList，DiscoverFolder时不会初始化mCurrentList，需要在QueryItems/QueryFolders时初始化
                    SwitchListContext(list);
                    parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                    //get system file firstly
                    try
                    {
                        GetSystemFiles(context, subitemsProp, list, folder, folderServerRelativeUrl, mMaxItemsPerThrottledOperation);
                        AddViewItems(context, list, folderServerRelativeUrl, subitemsProp, mMaxItemsPerThrottledOperation);
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query system files with listId:{0}, folderUrl:{1} Error: {2}", listId, folderServerRelativeUrl, e);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        subitemsProp.Add(error);
                    }
                }
                foreach (var itemsProp in GetItemsForArchiver(webId, listId, folderServerRelativeUrl, itemsId, needGetVersion, isDocumentLib, fieldsNeedLoadOfVersion))
                {
                    subitemsProp.AddRange(itemsProp);
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
                using (var context = CreateRetryContext())
                {
                    try
                    {
                        Web web = context.Site.OpenWebById(webId);
                        context.Load(web, w => w.ServerRelativeUrl);
                        context.ExecuteQuery();

                        parentFolder["WebServerRelativeUrl"] = web.ServerRelativeUrl;

                        Dictionary<string, object> files = GetFiles(web.ServerRelativeUrl, null, folderServerRelativeUrl != "/" ? "/" + folderServerRelativeUrl.TrimStart('/') : "/");
                        foreach (Dictionary<string, object> item in files.GetChildren())
                        {
                            List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                            AssembleWebItemVersionProperty(item, versions);
                            item["HasVersion"] = false;
                            webItems.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("Failed to query web files with webId:{0}, folderUrl:{1}", webId, folderServerRelativeUrl);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = e;
                        //webItems.Add(error);
                    }
                    yield return parentFolder;
                }
            }

            mIsLoadDFolderId = false;
        }

        private IEnumerable<List<Dictionary<string, object>>> GetItemsForArchiver(Guid webId, Guid listId, string folderServerRelativeUrl,
    IEnumerable<int> itemsId, bool needGetVersion, bool isDocumentLib, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            int versionCount = WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount == -1 ? int.MaxValue : WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount + 1;
            foreach (var itemsIdRange in GetCamlQueryIdRangeForArchiver(itemsId, isDocumentLib))
            {
                using (var context = CreateRetryContext())
                {
                    List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        CamlQuery query = new CamlQuery();
                        StringBuilder values = new StringBuilder();
                        if(itemsIdRange != null)
                        {
                            foreach (var id in itemsIdRange)
                            {
                                values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
                            }
                        }
                        query.ViewXml = string.Format(FORMAT_CAML_QUERY_ITEM, values);

                        ListItemCollection listItems = null;
                        bool oneByOne = false;
                        try
                        {
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            listItems = list.GetItems(query);
                            context.Load(list, l => l.BaseType, l => l.RootFolder.ServerRelativeUrl);
                            if (isDocumentLib)
                            {
                                context.Load(listItems, items => items.IncludeWithDefaultProperties(
                                    item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus, item => item.File.TimeLastModified));
                            }
                            else
                            {
                                context.Load(listItems, items => items.IncludeWithDefaultProperties(
                                    item => item.HasUniqueRoleAssignments));
                            }

                            if (needGetVersion)
                            {
                                context.Load(listItems, items => items.Include(item => item.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                    v => v.FileVersion.CheckInComment, v => v.FileVersion.Created, v => v.FileVersion.Length)));
                            }
                            context.ExecuteQuery();

                            foreach (var item in listItems)
                            {
                                if (!string.IsNullOrEmpty(WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName)
                                    && item != null
                                    && item.FieldValues != null
                                    )
                                {
                                    try
                                    {
                                        if (item.FieldValues.ContainsKey(WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName))
                                        {
                                            var fieldValue = item[WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName] as Microsoft.SharePoint.Client.Taxonomy.TaxonomyFieldValue;
                                            if (string.IsNullOrEmpty(fieldValue.Label) || string.IsNullOrEmpty(fieldValue.TermGuid))
                                            {
                                                mLogger.Error("IsNullOrEmpty to get bcsColumnValue when GetItemsForArchiver: {0}.", item.Id);
                                                throw new Exception("GetItemsForArchiverException");
                                            }
                                            else
                                            {
                                                mLogger.Info($"GetItemsForArchiver RecordsBCSColumnInternalName not null:ItemId{item.Id}.BCSColumnValue:{fieldValue.Label + fieldValue.TermGuid}.");
                                            }
                                        }
                                        else
                                        {
                                            mLogger.Error("item.FieldValues does not contains RecordsBCSColumnInternalName when GetItemsForArchiver: {0}.RecordsBCSColumnInternalName:{1}.", item.Id, WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName);
                                            throw new Exception("GetItemsForArchiverException");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        mLogger.Error("Failed to get bcsColumnValue when GetItemsForArchiver: {0} error: {1}", item.Id, ex.ToString());
                                        throw new Exception("GetItemsForArchiverException");
                                    }
                                }
                                results.Add(AssembleItemPropertiesForArchiver(list, item, fieldsNeedLoadOfVersion));
                            }
                            BuildLATInfoToItems(listItems, results);
                        }
                        catch (Exception e)
                        {
                            string itemsIdStr = string.Join(",", itemsIdRange);
                            var exception = e as ServerException;
                            if (exception == null)
                            {
                                mLogger.Error("Failed to query items with id range: {0} error: {1}", itemsIdStr, e);
                            }
                            else
                            {
                                mLogger.Error("Failed to query items with id range: {0} error code: {1} message: {2}",
                                    itemsIdStr, exception.ServerErrorCode, exception.Message);
                                if (exception.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                {
                                    throw;
                                }
                            }
                            oneByOne = true;
                        }

                        if (oneByOne)
                        {
                            results.Clear();
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            context.Load(list, l => l.BaseType, l => l.BaseTemplate, l => l.RootFolder.ServerRelativeUrl);
                            context.ExecuteQuery();

                            foreach (var id in itemsIdRange)
                            {
                                ListItem item = null;
                                try
                                {
                                    item = list.GetItemById(id);
                                    context.Load(item);
                                    if (needGetVersion)
                                    {
                                        context.Load(item, itm => itm.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                             v => v.FileVersion.CheckInComment, v => v.FileVersion.Created));
                                    }

                                    ExceptionHandlingScope handlingScope = new ExceptionHandlingScope(context);
                                    using (handlingScope.StartScope())
                                    {
                                        using (handlingScope.StartTry())
                                        {
                                            if (list.BaseType == BaseType.DocumentLibrary)
                                            {
                                                context.Load(item, it => it.HasUniqueRoleAssignments, it => it.File.CustomizedPageStatus, it => it.File.TimeLastModified);
                                            }
                                            else
                                            {
                                                context.Load(item, it => it.HasUniqueRoleAssignments);
                                            }
                                        }
                                        using (handlingScope.StartCatch())
                                        {
                                            if (list.BaseType == BaseType.DocumentLibrary)
                                            {
                                                context.Load(item, it => it.File.CustomizedPageStatus, it => it.File.TimeLastModified);
                                            }
                                        }
                                    }
                                    context.ExecuteQuery();
                                    if (handlingScope.HasException)
                                    {
                                        mLogger.Warn("Failed retrieve HasUniqueRoleAssignments property of {0}, ErrorTypeName:{1}, ErrorCode:{2}, ErrorMessage:{3}, StackTrace:{4}",
                                            id, handlingScope.ServerErrorTypeName, handlingScope.ServerErrorCode, handlingScope.ErrorMessage, handlingScope.ServerStackTrace);
                                    }

                                    results.Add(AssembleItemPropertiesForArchiver(list, item, fieldsNeedLoadOfVersion));
                                    if (!string.IsNullOrEmpty(WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName)
                                    && item != null
                                    && item.FieldValues != null
                                    )
                                    {
                                        try
                                        {
                                            if (item.FieldValues.ContainsKey(WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName))
                                            {
                                                var fieldValue = item[WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName] as Microsoft.SharePoint.Client.Taxonomy.TaxonomyFieldValue;
                                                if (string.IsNullOrEmpty(fieldValue.Label) || string.IsNullOrEmpty(fieldValue.TermGuid))
                                                {
                                                    mLogger.Error("IsNullOrEmpty to get bcsColumnValue when GetItemsForArchiver one by one: {0}.", item.Id);
                                                }
                                                else
                                                {
                                                    mLogger.Info($"GetItemsForArchiver RecordsBCSColumnInternalName not null one by one:ItemId{item.Id}.BCSColumnValue:{fieldValue.Label + fieldValue.TermGuid}.");
                                                }
                                            }
                                            else
                                            {
                                                mLogger.Error("item.FieldValues does not contains RecordsBCSColumnInternalName when GetItemsForArchiver one by one: {0}.RecordsBCSColumnInternalName:{1}.", item.Id, WrapperConfiguration.WrapperConfigurationForBPOS.RecordsBCSColumnInternalName);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            mLogger.Error("Failed to get bcsColumnValue when GetItemsForArchiver one by one: {0} error: {1}", item.Id, ex.ToString());
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    var serverException = e as ServerException;
                                    if (serverException == null)
                                    {
                                        mLogger.Warn("Load item failed by id: {0} parent folder: {1} error: {2}",
                                            id, folderServerRelativeUrl, e);
                                    }
                                    else
                                    {
                                        mLogger.Warn("Load item failed by id: {0} parent folder: {1} error code: {2} message: {3}",
                                            id, folderServerRelativeUrl, serverException.ServerErrorCode, serverException.Message);

                                        if (serverException.ServerErrorCode == AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST)
                                        {
                                            //Item does not exist. It may have been deleted by another user.
                                            continue;
                                        }
                                        else if (list.BaseTemplate == (int)AveListTemplateType.UserInformation && serverException.ServerErrorCode == AveSPErrorCode.ACCESS_DENIED)
                                        {
                                            mLogger.Warn("Load user information failed: {0}", id);
                                            continue;
                                        }
                                        //else if (serverException.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                        //{
                                        //    throw;
                                        //}
                                    }
                                    throw;
                                }
                            }
                        }

                        watch.Stop();
                        mLogger.Info("Load items with structure under folder: {0} costs: {1} count: {2}",
                            folderServerRelativeUrl, watch.Elapsed, results.Count);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Failed to query items under folder:{0} with id range:{1} Error: {2}",
                            folderServerRelativeUrl, string.Join(", ", itemsIdRange), ex);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = ex;
                        results.Add(error);
                    }
                    yield return results;
                }
            }
        }

        private Dictionary<string,DateTime> GetItemsLAT(ListItemCollection listItems)
        {
            Dictionary<string, DateTime> itemLATs = new Dictionary<string, DateTime>();
            try
            {
                List<string> itemIds = listItems.Select(x => x.FieldValues["UniqueId"].ToString()).ToList();
                string siteUrl = WebAppName.TrimEnd('/') + mSiteRelativeUrl;
                DateTime itemLastAccessTime = DateTime.MinValue;
                string cloudInsightsApiUrl = GCommon.Utility.Cloud.GCommonRoleConfiguration.PortalCloudInsightsApiURL;
                if (reportService == null)
                {
#if DEBUG
                    cloudInsightsApiUrl = "https://graph.sharepointguild.com/cloudinsights";
#endif
                    reportService = AvePoint.GCommon.Utility.AosApiUtility.CloudInsightsClientFactory.CreateCloudInsightsClient(cloudInsightsApiUrl, mTenantGroupId).ReportService;
                }
                lock (mlatUtilityLockObj)
                {
                    if (!string.Equals(lastSiteUrl, siteUrl))
                    {
                        latMgtApiEnableTime = new DateTime(AvePoint.Common.Portal.PortalUtil.Execute(() => reportService.GetMgtApiEnableTimeByTenantId(mTenantId.ToString())));
                        WrapperConfiguration.WrapperConfigurationForBPOS.LATMgtApiEnableTime = latMgtApiEnableTime;
                        mLogger.Info($"reportService CheckPrerequisites documentLatCheckResult:{latMgtApiEnableTime}.o365TenantId:{mTenantId}.tenantGroupId:{mTenantGroupId}.");
                        if (WrapperConfiguration.EnableDownloadLATData)
                        {
                            mLogger.Info("enable download lat is true");
                            //new interfacce
                            var scLAT = (AvePoint.Common.Portal.PortalUtil.Execute(() => reportService.GetDocumentLastAccessTime(new List<string>() { siteUrl }))).FirstOrDefault();
                            mLogger.Info($"reportService CheckPrerequisites SASUrlIsEmpty:{string.IsNullOrEmpty(scLAT?.StorageSasUrl)}.TableName:{scLAT?.TableName}.SASFileName:{scLAT?.FileName}.");
                            LastAccessTimeSqliteDBUtility.ClearInstance();
                            lastAccessTimeSqliteDBUtility = LastAccessTimeSqliteDBUtility.GetInstance(mTenantGroupId, scLAT?.StorageSasUrl, scLAT?.TableName);
                        }
                        lastSiteUrl = siteUrl;
                    }
                }
                if (latMgtApiEnableTime != DateTime.MinValue)
                {
                    if (WrapperConfiguration.EnableDownloadLATData)
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        lastAccessTimeSqliteDBUtility.ExecuteQueryWithAction(connection =>
                        {
                            using (var command = connection.CreateCommand())
                            {
                                itemLATs = lastAccessTimeSqliteDBUtility.SelectItemsLastAccessedTimeFromSqliteDB(command, itemIds);
                            }
                        });
                        watch.Stop();
                        mLogger.Info($"GetItemsLAT:ItemCount:{itemIds.Count}.itemLATCount:{itemLATs.Count}.QueryTime:{watch.Elapsed}.");
                    }
                }
                else
                {
                    //modify for SAAS-23181,由于DateTime这种声明方式不允许将time赋值为空，所以将itemLastAccessTime赋一个默认值。
                    WrapperConfiguration.WrapperConfigurationForBPOS.SiteMgtApiEnable = false;
                    mLogger.Warn($"The site do not meet the conditions. no lastAccessTime.");
                    throw new Exception("The site do not meet the conditions.");
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn($"GetItemsLAT failed.Message:{ex}.");
                throw;
            }
            return itemLATs;
        }

        private void BuildLATInfoToItems(ListItemCollection listItems, List<Dictionary<string, object>> results)
        {
            if (WrapperConfiguration.WrapperConfigurationForBPOS.HasLATRule && WrapperConfiguration.WrapperConfigurationForBPOS.SiteMgtApiEnable)
            {
                try
                {
                    var itemLATs = GetItemsLAT(listItems);
                    foreach (var result in results)
                    {
                        if (itemLATs.ContainsKey(result["UniqueId"].ToString()))
                        {
                            result.Add("LastAccessTime", itemLATs[result["UniqueId"].ToString()]);
                        }
                        result.Add("HasGetLAT", true);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn($"BuildLATInfoToItems failed.Message:{ex}.");
                }
            }
        }

        private Dictionary<string, object> AssembleItemPropertiesForArchiver(List list, ListItem item, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, item);

            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
            if (list.BaseType == BaseType.DocumentLibrary)
            {
                itemProperty["ObjType"] = ItemType.Document;
                try
                {
                    itemProperty["CustomizedPageStatus"] = (int)item.File.CustomizedPageStatus;
                }
                catch (Exception ex)
                {
                    mLogger.Info("Can not get CustomizedPageStatus with file {1}.Error:{0}", ex, item.Id);
                }
            }
            else
            {
                itemProperty["ObjType"] = ItemType.Item;
                itemProperty["Attachments"] = new List<Dictionary<string, object>>();
                GetAttachmentsFromItem(list.Context as ClientContext, list, itemProperty, list.RootFolder.ServerRelativeUrl);
            }
            AssembleVersionsPropertiesForArchiver(itemProperty, item, fieldsNeedLoadOfVersion);

            this.mCurrentList.Items[item.Id] = itemProperty;
            //subItemIds.Add(currentItemId);
            return itemProperty;
        }

        private void AssembleVersionsPropertiesForArchiver(Dictionary<string, object> itemProperty, ListItem item, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            var versionsObject = new Dictionary<string, object>();
            itemProperty["Versions" + AveObjectModelConstant.ObjectPropertySuffix] = versionsObject;
            if (item.IsObjectPropertyInstantiated("Versions") && item.Versions.Count > 0)
            {
                var versions = new List<IDictionary<string, object>>();
                foreach (var version in item.Versions)
                {
                    Dictionary<string, object> listItemVersionData = new Dictionary<string, object>();
                    Dictionary<string, object> listItemVersionFieldValue = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> fieldValue in version.FieldValues)
                    {
                        if (fieldsNeedLoadOfVersion.ContainsKey(fieldValue.Key) ||
                                fieldValue.Key.Equals("Editor", StringComparison.OrdinalIgnoreCase) ||
                                fieldValue.Key.Equals("Modified", StringComparison.OrdinalIgnoreCase) ||
                                fieldValue.Key.Equals("_CheckinComment", StringComparison.OrdinalIgnoreCase))
                        {
                            AssembleItemProperties(listItemVersionFieldValue, fieldValue.Value, fieldValue.Key);
                        }
                    }
                    ///这么做的原因是因为List Item Version的Field Values取出来的check in comment是所有version都有，
                    ///可能是通过current version赋值的。
                    string checkinComment = null;
                    if (version.IsObjectPropertyInstantiated("FileVersion") && version.FileVersion.IsPropertyAvailable("CheckInComment"))
                    {
                        checkinComment = version.FileVersion.CheckInComment;
                    }
                    else
                    {
                        object checkinCommentObj;
                        if (version.FieldValues.TryGetValue("_CheckinComment", out checkinCommentObj))
                        {
                            checkinComment = checkinCommentObj as string;
                        }
                    }

                    if (checkinComment != null)
                    {
                        listItemVersionFieldValue["_CheckinComment"] = checkinComment;
                        listItemVersionData["_CheckinComment"] = checkinComment;
                    }

                    //Created 在创建item的时候能顺带更新，页面上显示的item Created是第一个version的Created
                    //Created_x0020_Date 不可更新，SharePoint记录的系统时间。
                    listItemVersionFieldValue["Created"] = version.Created;

                    listItemVersionData.Add("FieldValues", listItemVersionFieldValue);

                    //listItemVersionData["Created"] = version.Created;
                    listItemVersionData["Modified"] = version.FieldValues["Modified"];
                    listItemVersionData["Editor"] = listItemVersionFieldValue["Editor"];
                    listItemVersionData["VersionId"] = version.VersionId;
                    listItemVersionData["VersionLabel"] = version.VersionLabel;

                    listItemVersionData["Level"] = byte.Parse(version.FieldValues["_Level"].ToString());
                    listItemVersionData["IsCurrentVersion"] = version.IsCurrentVersion;
                    listItemVersionData["Url"] = version.FieldValues["FileRef"];

                    object length;
                    if (version.IsObjectPropertyInstantiated("FileVersion") && version.FileVersion.IsPropertyAvailable("Length")&& version.FileVersion.Length > 0)
                    {
                        listItemVersionData["Length"] = version.FileVersion.Length;
                    }
                    else if (version.FieldValues.TryGetValue("File_x0020_Size", out length))
                    {
                        listItemVersionData["Length"] = length;
                    }
                    listItemVersionData["ModerationStatus"] = version.FieldValues["_ModerationStatus"];
                    object author;
                    if (version.FieldValues.TryGetValue("Author", out author))
                    {
                        listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = author;
                    }
                    else
                    {
                        listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = null;
                    }
                    //if (version.CreatedBy.ServerObjectIsNull == true)
                    //{
                    //    listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = null;
                    //}
                    //else
                    //{
                    //    listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = version.CreatedBy.LoginName;
                    //}
                    versions.Add(listItemVersionData);
                }
                versionsObject[AveObjectModelConstant.ChildrenProperties] = versions;
            }
            else
            {
                // add current version to versions if list disable version or item do not have versions
                //AssembleItemVersionProperty(itemProperty, versions);
                versionsObject["HasVersion"] = false;
            }
        }

        private IEnumerable<List<Dictionary<string, object>>> GetItems(Guid webId, Guid listId, string folderServerRelativeUrl, 
            IEnumerable<int> itemsId, bool needGetVersion, bool isDocumentLib, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            int versionCount = WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount == -1 ? int.MaxValue : WrapperConfiguration.WrapperConfigurationForBPOS.VersionCount + 1;
            foreach (var itemsIdRange in GetCamlQueryIdRange(itemsId, isDocumentLib))
            {
                using (var context = CreateRetryContext())
                {
                    List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        CamlQuery query = new CamlQuery();
                        StringBuilder values = new StringBuilder();
                        if (itemsIdRange != null)
                        {
                            foreach (var id in itemsIdRange)
                            {
                                values.AppendFormat(FORMAT_CAML_QUERY_VALUE_INT, id);
                            }
                        }
                        query.ViewXml = string.Format(FORMAT_CAML_QUERY_ITEM, values);

                        ListItemCollection listItems = null;
                        bool oneByOne = false;
                        try
                        {
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            listItems = list.GetItems(query);
                            context.Load(list, l => l.BaseType, l => l.RootFolder.ServerRelativeUrl);
                            if (isDocumentLib)
                            {
                                context.Load(listItems, items => items.IncludeWithDefaultProperties(
                                    item => item.HasUniqueRoleAssignments, item => item.File.CustomizedPageStatus, item => item.File.TimeLastModified));
                            }
                            else
                            {
                                context.Load(listItems, items => items.IncludeWithDefaultProperties(
                                    item => item.HasUniqueRoleAssignments));
                            }

                            if (needGetVersion)
                            {
                                context.Load(listItems, items => items.Include(item => item.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                    v => v.CreatedBy, v => v.FileVersion.CheckInComment, v => v.FileVersion.Created)));
                            }
                            context.ExecuteQuery();

                            foreach (var item in listItems)
                            {
                                results.Add(AssembleItemProperties(list, item, fieldsNeedLoadOfVersion));
                            }
                        }
                        catch (Exception e)
                        {
                            string itemsIdStr = string.Join(",", itemsIdRange);
                            var exception = e as ServerException;
                            if (exception == null)
                            {
                                mLogger.Error("Failed to query items with id range: {0} error: {1}", itemsIdStr, e);
                            }
                            else
                            {
                                mLogger.Error("Failed to query items with id range: {0} error code: {1} message: {2}",
                                    itemsIdStr, exception.ServerErrorCode, exception.Message);
                                if (exception.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                {
                                    throw;
                                }
                            }
                            oneByOne = true;
                        }

                        if (oneByOne)
                        {
                            results.Clear();
                            var web = context.Site.OpenWebById(webId);
                            var list = web.Lists.GetById(listId);
                            context.Load(list, l => l.BaseType, l => l.RootFolder.ServerRelativeUrl);
                            context.ExecuteQuery();

                            foreach (var id in itemsIdRange)
                            {
                                ListItem item = null;
                                try
                                {
                                    item = list.GetItemById(id);
                                    context.Load(item);
                                    if (needGetVersion)
                                    {
                                        context.Load(item, itm => itm.Versions.Take(versionCount).IncludeWithDefaultProperties(
                                             v => v.CreatedBy, v => v.FileVersion.CheckInComment, v => v.FileVersion.Created));
                                    }

                                    ExceptionHandlingScope handlingScope = new ExceptionHandlingScope(context);
                                    using (handlingScope.StartScope())
                                    {
                                        using (handlingScope.StartTry())
                                        {
                                            if (list.BaseType == BaseType.DocumentLibrary)
                                            {
                                                context.Load(item, it => it.HasUniqueRoleAssignments, it => it.File.CustomizedPageStatus, it => it.File.TimeLastModified);
                                            }
                                            else
                                            {
                                                context.Load(item, it => it.HasUniqueRoleAssignments);
                                            }
                                        }
                                        using (handlingScope.StartCatch())
                                        {
                                            if (list.BaseType == BaseType.DocumentLibrary)
                                            {
                                                context.Load(item, it => it.File.CustomizedPageStatus, it => it.File.TimeLastModified);
                                            }
                                        }
                                    }
                                    context.ExecuteQuery();
                                    if (handlingScope.HasException)
                                    {
                                        mLogger.Warn("Failed retrieve HasUniqueRoleAssignments property of {0}, ErrorTypeName:{1}, ErrorCode:{2}, ErrorMessage:{3}, StackTrace:{4}",
                                            id, handlingScope.ServerErrorTypeName, handlingScope.ServerErrorCode, handlingScope.ErrorMessage, handlingScope.ServerStackTrace);
                                    }

                                    results.Add(AssembleItemProperties(list, item, fieldsNeedLoadOfVersion));
                                }
                                catch (Exception e)
                                {
                                    var serverException = e as ServerException;
                                    if (serverException == null)
                                    {
                                        mLogger.Warn("Load item failed by id: {0} parent folder: {1} error: {2}",
                                            id, folderServerRelativeUrl, e);
                                    }
                                    else
                                    {
                                        mLogger.Warn("Load item failed by id: {0} parent folder: {1} error code: {2} message: {3}",
                                            id, folderServerRelativeUrl, serverException.ServerErrorCode, serverException.Message);

                                        if (serverException.ServerErrorCode == AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST)
                                        {
                                            //Item does not exist. It may have been deleted by another user.
                                            continue;
                                        }
                                        else if (list.BaseTemplate == (int)AveListTemplateType.UserInformation && serverException.ServerErrorCode == AveSPErrorCode.ACCESS_DENIED)
                                        {
                                            mLogger.Warn("Load user information failed: {0}", id);
                                            continue;
                                        }
                                        //else if (serverException.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                        //{
                                        //    throw;
                                        //}
                                    }
                                    throw;
                                }
                            }
                        }

                        watch.Stop();
                        mLogger.Info("Load items with structure under folder: {0} costs: {1} count: {2}",
                            folderServerRelativeUrl, watch.Elapsed, results.Count);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Failed to query items under folder:{0} with id range:{1} Error: {2}",
                            folderServerRelativeUrl, string.Join(", ", itemsIdRange), ex);
                        Dictionary<string, object> error = new Dictionary<string, object>();
                        error["Error"] = ex;
                        results.Add(error);
                    }
                    yield return results;
                }
            }
        }

        private Dictionary<string, object> AssembleItemProperties(List list, ListItem item, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            Dictionary<string, object> itemProperty = new Dictionary<string, object>();
            GetItemDic(itemProperty, item);

            itemProperty["Attachments" + AveObjectModelConstant.ObjectPropertySuffix] = item.FieldValues.ContainsKey("Attachments") ? item.FieldValues["Attachments"] : false;
            if (list.BaseType == BaseType.DocumentLibrary)
            {
                itemProperty["ObjType"] = ItemType.Document;
                try
                {
                    itemProperty["CustomizedPageStatus"] = (int)item.File.CustomizedPageStatus;
                }
                catch (Exception ex)
                {
                    mLogger.Info("Can not get CustomizedPageStatus with file {1}.Error:{0}", ex, item.Id);
                }
            }
            else
            {
                itemProperty["ObjType"] = ItemType.Item;
                itemProperty["Attachments"] = new List<Dictionary<string, object>>();
                GetAttachmentsFromItem(list.Context as ClientContext, list, itemProperty, list.RootFolder.ServerRelativeUrl);
            }
            AssembleVersionsProperties(itemProperty, item, fieldsNeedLoadOfVersion);

            this.mCurrentList.Items[item.Id] = itemProperty;
            //subItemIds.Add(currentItemId);
            return itemProperty;
        }

        private void AssembleVersionsProperties(Dictionary<string, object> itemProperty, ListItem item, IDictionary<string, string> fieldsNeedLoadOfVersion)
        {
            var versionsObject = new Dictionary<string, object>();
            itemProperty["Versions" + AveObjectModelConstant.ObjectPropertySuffix] = versionsObject;
            if (item.IsObjectPropertyInstantiated("Versions") && item.Versions.Count > 0)
            {
                var versions = new List<IDictionary<string, object>>();
                foreach (var version in item.Versions)
                {
                    Dictionary<string, object> listItemVersionData = new Dictionary<string, object>();
                    Dictionary<string, object> listItemVersionFieldValue = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> fieldValue in version.FieldValues)
                    {
                        if (fieldsNeedLoadOfVersion.ContainsKey(fieldValue.Key) ||
                                fieldValue.Key.Equals("Editor", StringComparison.OrdinalIgnoreCase) ||
                                fieldValue.Key.Equals("Modified", StringComparison.OrdinalIgnoreCase) ||
                                fieldValue.Key.Equals("_CheckinComment", StringComparison.OrdinalIgnoreCase))
                        {
                            AssembleItemProperties(listItemVersionFieldValue, fieldValue.Value, fieldValue.Key);
                        }
                    }
                    ///这么做的原因是因为List Item Version的Field Values取出来的check in comment是所有version都有，
                    ///可能是通过current version赋值的。
                    string checkinComment = null;
                    if (version.IsObjectPropertyInstantiated("FileVersion") && version.FileVersion.IsPropertyAvailable("CheckInComment"))
                    {
                        checkinComment = version.FileVersion.CheckInComment;
                    }
                    else
                    {
                        object checkinCommentObj;
                        if (version.FieldValues.TryGetValue("_CheckinComment", out checkinCommentObj))
                        {
                            checkinComment = checkinCommentObj as string;
                        }
                    }

                    if (checkinComment != null)
                    {
                        listItemVersionFieldValue["_CheckinComment"] = checkinComment;
                        listItemVersionData["_CheckinComment"] = checkinComment;
                    }

                    //Created 在创建item的时候能顺带更新，页面上显示的item Created是第一个version的Created
                    //Created_x0020_Date 不可更新，SharePoint记录的系统时间。
                    listItemVersionFieldValue["Created"] = version.Created;

                    listItemVersionData.Add("FieldValues", listItemVersionFieldValue);

                    //listItemVersionData["Created"] = version.Created;
                    listItemVersionData["Modified"] = version.FieldValues["Modified"];
                    listItemVersionData["Editor"] = listItemVersionFieldValue["Editor"];
                    listItemVersionData["VersionId"] = version.VersionId;
                    listItemVersionData["VersionLabel"] = version.VersionLabel;

                    listItemVersionData["Level"] = byte.Parse(version.FieldValues["_Level"].ToString());
                    listItemVersionData["IsCurrentVersion"] = version.IsCurrentVersion;
                    listItemVersionData["Url"] = version.FieldValues["FileRef"];

                    object length;
                    if (version.FieldValues.TryGetValue("File_x0020_Size", out length))
                    {
                        listItemVersionData["Length"] = length;
                    }
                    listItemVersionData["ModerationStatus"] = version.FieldValues["_ModerationStatus"];
                    if (version.CreatedBy.ServerObjectIsNull == true)
                    {
                        listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = null;
                    }
                    else
                    {
                        listItemVersionData["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = version.CreatedBy.LoginName;
                    }
                    versions.Add(listItemVersionData);
                }
                versionsObject[AveObjectModelConstant.ChildrenProperties] = versions;
            }
            else
            {
                // add current version to versions if list disable version or item do not have versions
                //AssembleItemVersionProperty(itemProperty, versions);
                versionsObject["HasVersion"] = false;
            }
        }

        public Dictionary<string, object> QueryListRootFolderWithStructureCache(Guid siteId, Guid webId, Guid mlistId)
        {
            var rootFolderProps = QueryListRootFolder(siteId, webId, mlistId);
            rootFolderProps["ListStructure"] = GetListFolderStructure(webId, mlistId);
            return rootFolderProps;
        }

        public Dictionary<string, object> QueryListRootFolderForFullDiscover(Guid siteId, Guid webId, Guid mlistId)
        {
            var rootFolderProps = QueryListRootFolder(siteId, webId, mlistId);
            rootFolderProps["ListStructure"] = GetListRootFolderWithAllItems(webId, mlistId);
            return rootFolderProps;
        }

        /// <summary>
        /// 获取整个list的folder结构
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        private SPOFolder GetListFolderStructure(Guid webId, Guid listId)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                var folder = list.RootFolder;
                int rowLimit = Convert.ToInt32(maxItemsPerThrottledOperation);
                Stopwatch watch = Stopwatch.StartNew();
                SPOFolder structure = list.LoadAllItemIds(rowLimit <= 0 ? 4000 : rowLimit, null);
                watch.Stop();

                try
                {
                    mLogger.Info("Cache folder structure with list {0} costs: {1}, Item Count: {2}", folder.ServerRelativeUrl, watch.Elapsed, list.ItemCount);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Log error during caching folder structure: {0}", e);
                }

                return structure;
            }
        }

        private SPOFolder GetListRootFolderWithAllItems(Guid webId, Guid listId)
        {
            using (var context = CreateContext())
            {
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                var folder = list.RootFolder;
                int rowLimit = Convert.ToInt32(maxItemsPerThrottledOperation);
                Stopwatch watch = Stopwatch.StartNew();
                SPOFolder structure = list.LoadAllItemIdsV1(rowLimit <= 0 ? 4000 : rowLimit, null);
                watch.Stop();

                try
                {
                    mLogger.Info("Cache folder structure with list {0} costs: {1}, Item Count: {2}", folder.ServerRelativeUrl, watch.Elapsed, list.ItemCount);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Log error during caching folder structure: {0}", e);
                }

                return structure;
            }
        }

        /*
         * need ensure itemsId is already ordered
         */
        public IEnumerable<IEnumerable<int>> GetCamlQueryIdRange(IEnumerable<int> itemsId, bool queryFile)
        {
            if (itemsId == null || !itemsId.Any())
            {
                yield break;
            }

            //itemsId = itemsId.OrderBy((id) => id); will break lazy get value of yield return
            int limited = queryFile ? AveCamlQuery.QUERY_VALUES_LIMITE_FILE : 
                AveCamlQuery.QUERY_VALUES_LIMITE_ITEM;
            List<int> idRange = new List<int>();
            int firstId = itemsId.First();
            foreach (var id in itemsId)
            {
                if (id - firstId > maxItemsPerThrottledOperation || idRange.Count >= limited)
                {
                    yield return idRange;
                    firstId = id;
                    idRange.Clear();
                }
                idRange.Add(id);
            }
            if (idRange.Count > 0)
            {
                yield return idRange;
                idRange.Clear();
            }
        }
        /// <summary>
        /// File and ListItem limit 60  need ensure itemsId is already ordered
        /// </summary>
        public IEnumerable<IEnumerable<int>> GetCamlQueryIdRangeForArchiver(IEnumerable<int> itemsId, bool queryFile)
        {
            if (itemsId == null || !itemsId.Any())
            {
                yield break;
            }

            //itemsId = itemsId.OrderBy((id) => id);  need ensure itemsId is already ordered
            int limited = WrapperConfiguration.WrapperConfigurationForBPOS.QUERY_VALUES_LIMITE_FILE;
            List<int> idRange = new List<int>();
            int firstId = itemsId.First();
            foreach (var id in itemsId)
            {
                if (id - firstId > maxItemsPerThrottledOperation || idRange.Count >= limited)
                {
                    yield return idRange;
                    firstId = id;
                    idRange.Clear();
                }
                idRange.Add(id);
            }
            if (idRange.Count > 0)
            {
                yield return idRange;
                idRange.Clear();
            }
        }
    }
}
