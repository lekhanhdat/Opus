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

using Microsoft.SharePoint.Client;

namespace AvePoint.ObjectModel.ClientOM
{
    public static class ListExtension
    {
        public delegate void ItemsRetrieving(int currentCount, int totalCount);

        //<ViewFields><FieldRef Name=\"ID\" /><FieldRef Name=\"Title\" /></ViewFields>
        static string queryIdXml = "<View Scope=\"RecursiveAll\"><Query><OrderBy Override=\"TRUE\"><FieldRef Name=\"ID\" /></OrderBy></Query><QueryOptions><QueryThrottleMode>Override</QueryThrottleMode></QueryOptions><RowLimit>{0}</RowLimit></View>";

        /// <summary>
        /// load AllItems with particular row litmit
        /// https://support.office.com/en-us/article/Manage-large-lists-and-libraries-in-Office-365-B4038448-EC0E-49B7-B853-679D3D8FB784
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rowLimit">the recommended number is 4500 for site collection; for OD4B, the max number is 20000</param>
        /// <returns></returns>
        /*public static SPOCaFolder LoadAllItemIdsForCa(this List list, int rowLimit, ItemsRetrieving itemsRetrieving)
        {
            list.Context.Load(list, l => l.RootFolder.ServerRelativeUrl, l => l.ItemCount);

            var contentIterator = new ContentIterator(list.Context);

            var folders = new Dictionary<string, SPOCaFolder>(StringComparer.OrdinalIgnoreCase);

            int currentCount = 0;

            contentIterator.ProcessListItems(list,
                new CamlQuery() { ViewXml = string.Format(queryIdXml, rowLimit) },
                items => items.RetrieveItems().Retrieve("Id", "FileRef", "FileLeafRef", "FileSystemObjectType", "HasUniqueRoleAssignments"),
                items =>
                {
                    if (itemsRetrieving != null)
                    {
                        currentCount += items.Count;
                        itemsRetrieving(currentCount, list.ItemCount);
                    }
                    AnalyzeListItems(list, items, folders);
                },
                (items, ex) => true);

            var rootFolder = folders[list.RootFolder.ServerRelativeUrl];
            folders.Remove(list.RootFolder.ServerRelativeUrl);

            List<string> invalidItem = new List<string>();
            foreach (var keyValue in folders)
            {
                if (keyValue.Value.Id == 0)
                {
                    invalidItem.Add(keyValue.Key);
                    //throw new Office365ApiException(ApiRS.InvalidFolderFormat(keyValue.Key), Office365ApiErrorCode.InvalidFolderId);
                }
            }
            if (invalidItem.Count > 0)
            {
                //LoggerUtility.WriteMessage(Severity.WARN, "There are some invalid folders: {0}", string.Join(Environment.NewLine, invalidItem));
            }

            return rootFolder;
        }*/

        /*private static void AnalyzeListItems(List list, ListItemCollection items, Dictionary<string, SPOCaFolder> folders)
        {
            if (folders.Count == 0)
            {
                folders[list.RootFolder.ServerRelativeUrl] = new SPOCaFolder() { Name = list.RootFolder.ServerRelativeUrl };
            }

            foreach (var item in items)
            {
                var serverRelativeUrl = (string)item.FieldValues["FileRef"];
                var name = (string)item.FieldValues["FileLeafRef"];
                var id = item.Id;
                var hasUniquePermissions = item.HasUniqueRoleAssignments;
                SPOCaFolder folder;
                if (item.FileSystemObjectType == FileSystemObjectType.File)
                {
                    var spoItem = new SPOCaItem()
                    {
                        Id = id,
                        Name = name,
                        HasUniqueRoleAssignments = hasUniquePermissions,
                    };
                    var folderUrl = serverRelativeUrl.Substring(0, serverRelativeUrl.Length - name.Length - 1);

                    if (!folders.TryGetValue(folderUrl, out folder))
                    {
                        folder = new SPOCaFolder();
                        folder.Name = folderUrl.Substring(folderUrl.LastIndexOf('/') + 1);
                        PushParentFolder(folderUrl, folder, folders);
                        folders.Add(folderUrl, folder);
                    }

                    if (folder.Items == null)
                    {
                        folder.Items = new List<SPOCaItem>() { spoItem };
                    }
                    else
                    {
                        folder.Items.Add(spoItem);
                    }
                }
                else
                {
                    if (!folders.TryGetValue(serverRelativeUrl, out folder))
                    {
                        folder = new SPOCaFolder();
                        folder.Name = name;
                        folder.Id = id;
                        folder.HasUniqueRoleAssignments = hasUniquePermissions;
                        PushParentFolder(serverRelativeUrl, folder, folders);
                        folders.Add(serverRelativeUrl, folder);
                    }
                    else
                    {
                        folder.Name = name;
                        folder.Id = id;
                        folder.HasUniqueRoleAssignments = hasUniquePermissions;
                    }
                }
            }
        }*/

        /*private static void PushParentFolder(string serverRelativeUrl, SPOCaFolder folder, Dictionary<string, SPOCaFolder> folders)
        {
            var index = serverRelativeUrl.LastIndexOf('/');
            if (index > 0)
            {
                var parentFolderServerRelativeUrl = serverRelativeUrl.Substring(0, index);

                SPOCaFolder parentFolder;
                if (!folders.TryGetValue(parentFolderServerRelativeUrl, out parentFolder))
                {
                    parentFolder = new SPOCaFolder()
                    {
                        Name = serverRelativeUrl.Substring(index + 1),
                        SubFolders = new List<SPOCaFolder>() { folder }
                    };
                    PushParentFolder(parentFolderServerRelativeUrl, parentFolder, folders);
                    folders.Add(parentFolderServerRelativeUrl, parentFolder);
                }
                else
                {
                    if (parentFolder.SubFolders == null)
                    {
                        parentFolder.SubFolders = new List<SPOCaFolder>() { folder };
                    }
                    else
                    {
                        parentFolder.SubFolders.Add(folder);
                    }
                }
            }
        }
        }*/

        /// <summary>
        /// https://support.office.com/en-us/article/Manage-large-lists-and-libraries-in-Office-365-B4038448-EC0E-49B7-B853-679D3D8FB784
        /// </summary>
        /// <param name="list"></param>
        /*public static SPOCaFolder LoadAllItemIdsForCa(this List list, ItemsRetrieving itemsRetrieving)
        {
            return LoadAllItemIdsForCa(list, 4500, itemsRetrieving);
        }*/
    }
}
