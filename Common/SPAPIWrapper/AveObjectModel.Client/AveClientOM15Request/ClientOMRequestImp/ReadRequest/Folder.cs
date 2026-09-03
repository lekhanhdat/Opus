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
    using AvePoint.Common.Portal;
    using AvePoint.ObjectModel.WebService;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using ClientFolder = Microsoft.SharePoint.Client.Folder;
    using SPChangeType = Microsoft.SharePoint.Client.ChangeType;

    public partial class AveClientOM2013Request
    {
        public Dictionary<string, object> GetFolder(string webServerRelativeUrl, string listName, string folderServerRelativeUrl)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> folderProp = GetFolder(context, webServerRelativeUrl, listName, folderServerRelativeUrl);
                return folderProp;
            }
        }

        private Dictionary<string, object> GetFolder(ClientContext context, string webServerRelativeUrl, string listName, string folderServerRelativeUrl)
        {
            bool folderServerRelativeUrlVaild = true;
            Web web = context.Site.OpenWeb(webServerRelativeUrl);

            Folder folder = null;
            ListItem item = null;
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
            context.Load(folder);
            context.Load(folder.Properties);
            context.Load(folder, f => f.ParentFolder);
            try
            {
                if (listName != null)
                {
                    context.Load(folder.ListItemAllFields);
                    item = folder.ListItemAllFields;
                    context.Load(item, it => it.HasUniqueRoleAssignments);//SAAS-5240
                }
                else
                {
                    context.Load(folder.Files, fs => fs.Include(f => f.Name));
                    context.Load(folder.Folders, ff => ff.Include(f => f.Name));
                }
                context.ExecuteQuery();
                if (string.IsNullOrEmpty(listName))
                {
                    folderProperties["FilesCount"] = folder.Files.Count;
                    folderProperties["FoldersCount"] = folder.Folders.Count;
                }
            }
            catch (ServerException se)
            {
                if (se.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    folderProperties["Exists"] = false;
                    folderServerRelativeUrlVaild = false;
                }
                if (se.ServerErrorCode == -2147023080) ///exceed storage limited
                {
                    throw;
                }
                else
                {
                    mLogger.Warn("Folder:{0} not exists.Error Message:{1}", folderServerRelativeUrl, se.ToString());
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Folder:{0} not exists.Error Message:{1}", folderServerRelativeUrl, ex.ToString());
                folderProperties["Exists"] = false;
                folderServerRelativeUrlVaild = false;
            }
            if (item != null && item.IsPropertyAvailable("Id"))
            {
                Dictionary<string, object> itmProp = new Dictionary<string, object>();
                GetItemDic(itmProp, item);
                if (itmProp.ContainsKey("UniqueId"))
                {
                    folderProperties["UniqueId"] = itmProp["UniqueId"];
                }
                folderProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itmProp;
            }
            if (folderServerRelativeUrlVaild)
            {
                AssembleFolderProperties(webServerRelativeUrl, folder, folderServerRelativeUrl, folderProperties);
                folderProperties["Exists"] = true;
            }
            return folderProperties;
        }
        public Dictionary<string, object> GetFolderFromCache(string webServerRelativeUrl, string listName, string folderServerRelativeUrl, Guid listId, int folderId)
        {
            if (this.mCurrentList.ListTitle.Equals(listName, StringComparison.OrdinalIgnoreCase) && mCurrentList.Folders.ContainsKey(folderId)
               && mCurrentList.ListId == listId)
            {
                mLogger.Info("Current folder exist in folder cache, folder url: {0}.folder Id:{1}.", folderServerRelativeUrl, folderId);
                return this.mCurrentList.Folders[folderId];
            }
            else
            {
                using (var context = CreateRetryContext())
                {
                    Dictionary<string, object> folderProp = GetFolder(context, webServerRelativeUrl, listName, folderServerRelativeUrl);
                    return folderProp;
                }
            }
        }
        public Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        {
            return GetFolders(webServerRelativeUrl, listName, listId, folderServerRelativeUrl, false);
        }

        private Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl, bool includeSystemFolder)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> subFolders = new Dictionary<string, object>();
                var subFolderList = new List<IDictionary<string, object>>();

                try
                {
                    var folderProperties = new List<IDictionary<string, object>>();
                    Hashtable _hashTable = null;

                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    List list = null;
                    Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    context.Load(folder);
                    context.Load(folder, f => f.Properties);
                    context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder, f => f.Properties));
                    if (listName != null)
                    {
                        list = web.Lists.GetById(listId);
                        try
                        {
                            context.ExecuteQuery();
                            int subfolderCount = folder.Properties.FieldValues.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folder.Properties.FieldValues["vti_foldersubfolderitemcount"]) : 0;
                            var temp = GetFoldersWithRequestedProperties(context, list, webServerRelativeUrl, folderServerRelativeUrl, subfolderCount);
                            folderProperties.AddRange(temp);
                        }
                        catch (Exception e)
                        {
                            mLogger.Error("failed to load folder items, error detail : {0}", e.ToString());
                        }
                    }
                    _hashTable = new Hashtable(folderProperties.Count, StringComparer.OrdinalIgnoreCase);
                    foreach (var folderProperty in folderProperties)
                    {
                        _hashTable[folderProperty["ServerRelativeUrl"].ToString()] = folderProperty;
                    }
                    GetFolderProperties(context, web, list, folder, webServerRelativeUrl, folderServerRelativeUrl, _hashTable, subFolderList, includeSystemFolder);

                    if (_hashTable != null)
                    {
                        _hashTable.Clear();
                        _hashTable = null;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn(string.Format("get folders failed, parent folder url: {0}", folderServerRelativeUrl), e);
                }
                subFolders.AddChildren(subFolderList);
                return subFolders;
            }
        }

        private void GetFolderProperties(ClientContext context, Web web, List list, Folder folder, string webServerRelativeUrl, string folderServerRelativeUrl, Hashtable folderProperties, List<IDictionary<string, object>> subFolderList, bool includeSystemFolder)
        {
            List<string> excludeFolders = null;
            if (!includeSystemFolder && folderServerRelativeUrl.Trim('/').Equals(webServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
            {
                context.Load(web, w => w.Lists.Include(l => l.RootFolder.ServerRelativeUrl));
                context.ExecuteQuery();
                excludeFolders = new List<string>();
                foreach (List l in web.Lists)
                {
                    excludeFolders.Add(l.RootFolder.ServerRelativeUrl.ToLower());
                }
                excludeFolders.AddRange(new string[] { "_catalogs",
                        "_vti_pvt", "_cts", "_private",
                        "_themes", "lists" , "m"});
            }
            else if (folderServerRelativeUrl.Trim('/').Equals(webServerRelativeUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
            {
                excludeFolders = new List<string> { "_cts" };
            }

            List<Folder> folders = new List<Folder>();
            if (list != null && list.ItemCount > 5000 && folder.Folders.Count == 0)     //large list which contains over 5000 items/files/folders
            {
                int pendingRequestCount = 0;
                foreach (Dictionary<string, object> fProp in folderProperties.Values)
                {
                    Folder subFolder = list.ParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(fProp["ServerRelativeUrl"].ToString()));
                    context.Load(subFolder);
                    context.Load(subFolder, f => f.ParentFolder.ServerRelativeUrl);
                    pendingRequestCount++;
                    folders.Add(subFolder);
                    if (pendingRequestCount > 100)
                    {
                        context.ExecuteQuery();
                    }
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }
            }
            else
            {
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }
                foreach (Folder subFolder in folder.Folders)
                {
                    if (excludeFolders != null && excludeFolders.Count > 0)
                    {
                        if (excludeFolders.Contains(subFolder.ServerRelativeUrl.ToLowerInvariant()) || excludeFolders.Contains(subFolder.Name.ToLowerInvariant()))
                        {
                            continue;
                        }
                    }
                    //For DPM discover
                    if (includeSystemFolder)
                    {
                        if (subFolder.Properties.FieldValues.ContainsKey("vti_listname") && subFolder.Properties.FieldValues["vti_listname"] != null &&
                            AveTypeHelper.IsGuid(subFolder.Properties.FieldValues["vti_listname"].ToString()) && new Guid(subFolder.Properties.FieldValues["vti_listname"].ToString()) != Guid.Empty)
                        {
                            continue;
                        }
                    }
                    folders.Add(subFolder);
                }
            }

            foreach (Folder f in folders)
            {
                Dictionary<string, object> subFolderProperties = new Dictionary<string, object>();
                subFolderProperties["Exists"] = true;
                AssembleFolderProperties(webServerRelativeUrl, f, f.ServerRelativeUrl, subFolderProperties);
                if (folderProperties != null)
                {
                    if (folderProperties.ContainsKey(f.ServerRelativeUrl))
                    {
                        Dictionary<string, object> fProp = folderProperties[f.ServerRelativeUrl] as Dictionary<string, object>;
                        subFolderProperties["UniqueId"] = fProp["UniqueId"];
                        string item = "Item" + AveObjectModelConstant.ObjectPropertySuffix;
                        subFolderProperties[item] = fProp;
                    }
                }
                subFolderList.Add(subFolderProperties);
            }
        }
    }
}
