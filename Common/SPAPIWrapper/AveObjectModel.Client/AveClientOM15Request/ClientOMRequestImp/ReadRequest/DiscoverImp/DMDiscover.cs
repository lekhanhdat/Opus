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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    public partial class AveClientOM2013Request
    {
        public virtual PreDiscoverDesignListResult PreDiscoverDesignList(string siteUrl, Guid webId, Guid listId, bool includeGhostFile = false, bool includeEmptyFolder = false)
        {
            var emptyFolders = new Dictionary<int, string>();
            var ghostFiles = new Dictionary<int, string>();
            using (var context = CreateRetryContext(siteUrl))
            {
                var web = context.Site.OpenWebById(webId);
                var list = web.Lists.GetById(listId);
                LoadList(list);
                var folders = new Dictionary<string, DMPreDiscoverFolder>(StringComparer.OrdinalIgnoreCase)
                {
                    { list.RootFolder.ServerRelativeUrl, IntDMPreDiscoverRootFolder(list)}
                };
                LoadFolders(list, folders);
                LoadFileOrListItem(list, includeGhostFile, folders, ghostFiles);
                TrimEmptyFolders(folders, includeEmptyFolder,emptyFolders);
                OutputDiscoverResult(list, folders);
                return new PreDiscoverDesignListResult
                {
                    PreserveFolders = folders,
                    EmptyFolders = emptyFolders,
                    GhostFiles = ghostFiles
                };
            }
        }

        protected DMPreDiscoverFolder IntDMPreDiscoverRootFolder(List list)
        {
            return new DMPreDiscoverFolder
            {
                Id = 0,
                Name = list.RootFolder.Name,
                Items = new List<DMPreDiscoverItem>(),
                SubFolders = new List<DMPreDiscoverFolder>()
            };
        }
        protected void LoadFileOrListItem(List list, bool includeGhostFile, Dictionary<string, DMPreDiscoverFolder> folders, Dictionary<int, string> ghostFiles)
        {
            LoadItems(list, folders, GetLoadItemMethod(list), delegate (ListItem item)
            {
                if (list.BaseType==BaseType.DocumentLibrary&&!includeGhostFile && item.File.CustomizedPageStatus == CustomizedPageStatus.Uncustomized)
                {
                    ghostFiles.Add(item.Id,(string)item[ListItemConsts.FieldRef]);
                    return false;
                }
                return true;
            });
        }
        protected Action<ClientRuntimeContext, ListItemCollection> GetLoadItemMethod(List list)
        {
            Action<ClientRuntimeContext, ListItemCollection> loadFunction;
            if (list.BaseType == BaseType.DocumentLibrary)
            {
                loadFunction = LoadDocumentsMethod;
            }
            else
            {
                loadFunction = LoadListItemsMethod;
            }

            return loadFunction;
        }
        protected void LoadList(List list)
        {
            list.Context.Load(list, l => l.RootFolder.ServerRelativeUrl, l => l.RootFolder.Name, l => l.BaseType, l => l.Title, l => l.Id);
            list.Context.ExecuteQuery();
        }
      
        protected virtual void TrimEmptyFolders(Dictionary<string, DMPreDiscoverFolder> folders, bool includeEmptyFolder, Dictionary<int, string> emptyFolders)
        {
            if (includeEmptyFolder)
            {
                return;
            }
            var folderUrlList = folders.Keys.ToList();
            folderUrlList.Sort();
            for (int k = folderUrlList.Count - 1; k >= 0; k--)
            {
                string url = folderUrlList[k];
                var folder = folders[url];
                if (!folder.HasChildren())
                {
                    folders.Remove(url);                   
                    RemoveFromParentFolder(folders, url, folder);
                    emptyFolders.Add(folder.Id,url);
                }
            }
        }

        protected virtual void RemoveFromParentFolder(Dictionary<string, DMPreDiscoverFolder> folders, string url, DMPreDiscoverFolder folder)
        {
            var parentUrl = url.Substring(0, url.Length - folder.Name.Length - 1);
            DMPreDiscoverFolder parentFolder;
            if (folders.TryGetValue(parentUrl, out parentFolder))
            {
                parentFolder.SubFolders.Remove(folder);
            }
        }

        protected void OutputDiscoverResult(List list, Dictionary<string, DMPreDiscoverFolder> folders)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("OutputPreDiscoverResult:");
            builder.AppendLine(string.Format("ListTitle:{0}",list.Title));
            foreach (var folder in folders)
            {
                builder.AppendLine(folder.Key);
                var subItems = folder.Value.Items;
                if (subItems != null)
                {
                    foreach (var item in subItems)
                    {
                        builder.AppendLine(item.Name);
                    }
                }
            }
            mLogger.Info(builder.ToString());
        }
        protected void LoadListItemsMethod(ClientRuntimeContext context, ListItemCollection items)
        {
            context.Load(items, its => its.Include(i => i[ListItemConsts.FieldLeafRef], i => i[ListItemConsts.FieldRef], i => i.Id));
        }
        protected void LoadDocumentsMethod(ClientRuntimeContext context, ListItemCollection items)
        {
            context.Load(items, its => its.Include(i => i[ListItemConsts.FieldLeafRef], i => i[ListItemConsts.FieldRef], i => i.Id, i => i.File.CustomizedPageStatus));
        }
        protected virtual void LoadItems(List list, Dictionary<string, DMPreDiscoverFolder> folders, Action<ClientRuntimeContext, ListItemCollection> loadItemFunction, Func<ListItem, bool> shouldBackup)
        {

            var caml = new CamlQuery
            {
                ViewXml = ListItemConsts.GetQueryItemsString(FileSystemObjectType.File)
            };
            var items = list.GetItems(caml);
            loadItemFunction(list.Context, items);
            list.Context.ExecuteQuery();
            foreach (var item in items)
            {
                if (!shouldBackup(item))
                {
                    continue;
                }
                DMPreDiscoverFolder parentFolder;
                DMPreDiscoverItem spoItem = new DMPreDiscoverItem
                {
                    Id = item.Id,
                    Name = (string)item[ListItemConsts.FieldLeafRef],
                };
                var folderServerRelativeUrl = ((string)item[ListItemConsts.FieldRef]).Substring(0, ((string)item[ListItemConsts.FieldRef]).Length - spoItem.Name.Length - 1);

                if (!folders.TryGetValue(folderServerRelativeUrl, out parentFolder))
                {
                    parentFolder = new DMPreDiscoverFolder();
                    parentFolder.Name = folderServerRelativeUrl.Substring(folderServerRelativeUrl.LastIndexOf('/') + 1);
                    AddFolderToCache(folders, parentFolder, folderServerRelativeUrl);
                    folders.Add(folderServerRelativeUrl, parentFolder);
                }

                if (parentFolder.Items == null)
                {
                    parentFolder.Items = new List<DMPreDiscoverItem>() { spoItem };
                }
                else
                {
                    parentFolder.Items.Add(spoItem);
                }
            }

        }
        protected virtual void AddFolderToCache(Dictionary<string, DMPreDiscoverFolder> folders, DMPreDiscoverFolder folder, string folderServerRelativeUrl)
        {
            var index = folderServerRelativeUrl.LastIndexOf('/');
            if (index > 0)
            {
                var parentFolderServerRelativeUrl = folderServerRelativeUrl.Substring(0, index);

                DMPreDiscoverFolder parentFolder;
                if (!folders.TryGetValue(parentFolderServerRelativeUrl, out parentFolder))
                {
                    parentFolder = new DMPreDiscoverFolder()
                    {
                        Name = folderServerRelativeUrl.Substring(index + 1),
                        SubFolders = new List<DMPreDiscoverFolder>() { folder }
                    };
                    AddFolderToCache(folders, parentFolder, parentFolderServerRelativeUrl);
                    folders.Add(parentFolderServerRelativeUrl, parentFolder);
                }
                else
                {
                    if (parentFolder.SubFolders == null)
                    {
                        parentFolder.SubFolders = new List<DMPreDiscoverFolder>() { folder };
                    }
                    else
                    {
                        parentFolder.SubFolders.Add(folder);
                    }
                }
            }
        }
        protected virtual void LoadFolders(List list, Dictionary<string, DMPreDiscoverFolder> folders)
        {

            var caml = new CamlQuery
            {
                ViewXml = ListItemConsts.GetQueryItemsString(FileSystemObjectType.Folder)
            };
            var items = list.GetItems(caml);
            items.RetrieveItems().Retrieve(ListItemConsts.FieldLeafRef, ListItemConsts.FieldRef, ListItemConsts.Id);
            list.Context.ExecuteQuery();
            foreach (var item in items)
            {
                DMPreDiscoverFolder folder;
                string serverRelativeUrl = item[ListItemConsts.FieldRef].ToString();
                if (!folders.TryGetValue(serverRelativeUrl, out folder))
                {
                    folder = new DMPreDiscoverFolder
                    {
                        Items = new List<DMPreDiscoverItem>(),
                        SubFolders = new List<DMPreDiscoverFolder>(),
                        Id = item.Id,
                        Name = (string)item[ListItemConsts.FieldLeafRef]
                    };
                    AddFolderToCache(folders, folder, serverRelativeUrl);
                    folders.Add(serverRelativeUrl, folder);
                }
                else
                {
                    folder.Name = (string)item[ListItemConsts.FieldLeafRef];
                    folder.Id = item.Id;
                }
            }

        }
    }
}
