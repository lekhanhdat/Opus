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
namespace Microsoft.SharePoint.Client
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ListExtension
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(ListExtension));

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
        public static SPOFolder LoadAllItemIdsV1(this List list, int rowLimit, ItemsRetrieving itemsRetrieving)
        {
            list.Context.Load(list, l => l.RootFolder.ServerRelativeUrl, l => l.ItemCount);
            list.Context.ExecuteQuery();
            var contentIterator = new ContentIterator(list.Context);

            //add all item ids to root folder
            var rootFolder = SPOFolder.BuildRootFolder(new (), new (), list.RootFolder.ServerRelativeUrl);
            //var folders = new Dictionary<string, SPOFolder>(StringComparer.OrdinalIgnoreCase);

            int currentCount = 0;

            contentIterator.ProcessListItems(list,
                new CamlQuery() { ViewXml = string.Format(queryIdXml, rowLimit) },
                items => items.RetrieveItems().Retrieve("Id", "FileRef", "FileLeafRef", "FileSystemObjectType"),
                items => 
                {
                    if (itemsRetrieving != null)
                    {
                        currentCount += items.Count;
                        itemsRetrieving(currentCount, list.ItemCount);
                    }
                    AnalyzeListItems(items, rootFolder);
                },
                (items, ex) => true);

            return rootFolder;
        }

        /// <summary>
        /// 拼接Folder/Item结构
        /// </summary>
        /// <param name="items"></param>
        /// <param name="rootFolder"></param>
        private static void AnalyzeListItems(ListItemCollection items, SPOFolder rootFolder)
        {
            foreach (var item in items)
            {
                var serverRelativeUrl = (string)item.FieldValues["FileRef"];
                var name = (string)item.FieldValues["FileLeafRef"];

                var parentFolder = rootFolder;
                var frUrl = serverRelativeUrl.Substring(rootFolder.Name.Length, serverRelativeUrl.Length - rootFolder.Name.Length - name.Length - 1);
                mLogger.Info($"AnalyzeListItems. ObjectId:{item.Id}.ObjectServerRelativeUrl:{frUrl}.");
                var parentFoldersName = frUrl.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parentFoldersName.Length; i++)
                {
                    var folderName = parentFoldersName[i];
                    SPOFolder tempFolder = parentFolder.SubFolders.GetByName(folderName);

                    if (tempFolder == null)
                    {
                        tempFolder = SPOFolder.BuildUnRootFolder(parentFolder, folderName, -1);
                        parentFolder.SubFolders.Add(tempFolder);
                    }
                    parentFolder = tempFolder;
                }

                var id = item.Id;
                if (item.FileSystemObjectType == FileSystemObjectType.File)
                {
                    var spoItem = new SPOItem()
                    {
                        Id = id,
                        Name = name,
                    };
                    parentFolder.Items.Add(spoItem);
                }
                else
                {
                    var spoFolder = parentFolder.SubFolders.GetByName(name);
                    if (spoFolder == null)
                    {
                        spoFolder = SPOFolder.BuildUnRootFolder(parentFolder, name, id);
                        parentFolder.SubFolders.Add(spoFolder);
                    }
                    else
                    {
                        spoFolder.Id = id;
                    }
                }
            }
        }

        /*private static void PushParentFolder(string serverRelativeUrl, SPOFolder folder, Dictionary<string, SPOFolder> folders)
        {
            var index = serverRelativeUrl.LastIndexOf('/');
            if (index > 0)
            {
                var parentFolderServerRelativeUrl = serverRelativeUrl.Substring(0, index);

                SPOFolder parentFolder;
                if (!folders.TryGetValue(parentFolderServerRelativeUrl, out parentFolder))
                {
                    parentFolder = new SPOFolder()
                    {
                        Name = serverRelativeUrl.Substring(index + 1),
                        SubFolders = new List<SPOFolder>() { folder }
                    };
                    PushParentFolder(parentFolderServerRelativeUrl, parentFolder, folders);
                    folders.Add(parentFolderServerRelativeUrl, parentFolder);
                }
                else
                {
                    if (parentFolder.SubFolders == null)
                    {
                        parentFolder.SubFolders = new List<SPOFolder>() { folder };
                    }
                    else
                    {
                        parentFolder.SubFolders.Add(folder);
                    }
                }
            }
        }*/

        /// <summary>
        /// https://support.office.com/en-us/article/Manage-large-lists-and-libraries-in-Office-365-B4038448-EC0E-49B7-B853-679D3D8FB784
        /// </summary>
        /// <param name="list"></param>
        public static SPOFolder LoadAllItemIds(this List list, ItemsRetrieving itemsRetrieving)
        {
            return LoadAllItemIdsV1(list, 4500, itemsRetrieving);
        }
    }
}
