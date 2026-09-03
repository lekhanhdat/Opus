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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{
    public class FolderLevel : IndividualBase
    {
        public FolderLevel(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
            : base(objectModel, sqlConnString, siteUrl)
        {

        }

        /// <summary>
        /// 任何list都要给root folder
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<SPTreeNodeDto> GetRootFolder(IAveList list, int siteLockStatus)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> subFolders = new List<SPTreeNodeDto>();
            SPTreeNodeDto dto = ConvertToDto(list.RootFolder, true);
            dto.Name = AvePoint.GCommon.Utility.GConstants.SPNodeName.RootFolder;
            dto.DisplayName = AvePoint.GCommon.Utility.GConstants.SPNodeName.RootFolder;
            dto.ParentId = list.ID.ToString();
            dto.SiteLockStatus = siteLockStatus;
            subFolders.Add(dto);
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower RootFolder Elasped Time: {0}, RootFolderCount: {1}, ParentWeb: {2}, ListTitle: {2}", sw.Elapsed.ToString(), subFolders.Count, list.ParentWebUrl, list.Title);
#endif
            return subFolders;
        }

        public List<SPTreeNodeDto> GetRootFolder(Guid parentWebId, Guid parentListId, int siteLockStatus)
        {
            List<SPTreeNodeDto> subFolders = new List<SPTreeNodeDto>();
            AveFolderBrowserInfo rootFolder = new AveFolderBrowserInfo();
            if (parentListId != Guid.Empty)
            {
                rootFolder = Query.GetBrowserRootFolder(parentWebId, parentListId);
            }
            else
            {
                rootFolder = Query.GetBrowserWebRootFolder(parentWebId);
            }
            SPTreeNodeDto dto = ConvertToRootFolderDto(rootFolder);
            dto.Name = AvePoint.GCommon.Utility.GConstants.SPNodeName.RootFolder;
            dto.DisplayName = AvePoint.GCommon.Utility.GConstants.SPNodeName.RootFolder;
            dto.ParentId = parentListId.ToString();
            dto.SiteLockStatus = siteLockStatus;
            subFolders.Add(dto);
            return subFolders;
        }

        public List<SPTreeNodeDto> GetRootFolder(IAveWeb web, int siteLockStatus)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> subFolders = new List<SPTreeNodeDto>();
            SPTreeNodeDto dto = ConvertToDto(web.GetFolder(""), true);
            dto.Name = AvePoint.GCommon.Utility.GConstants.SPNodeName.RootFolder;
            dto.DisplayName = AvePoint.GCommon.Utility.GConstants.SPNodeName.RootFolder;
            dto.ParentId = Guid.Empty.ToString();
            dto.SiteLockStatus = siteLockStatus;
            subFolders.Add(dto);
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower RootFolder Elasped Time: {0}, RootFolderCount: {1}, ParentWeb: {2}", sw.Elapsed.ToString(), subFolders.Count, web.Url);
#endif
            return subFolders;
        }

        public List<SPTreeNodeDto> GetItems(IAveFolder parentFolder) // for system folder
        {
            List<SPTreeNodeDto> items = new List<SPTreeNodeDto>();
            IAveFileCollection fileCollection = parentFolder.Files;
            foreach (IAveFile file in fileCollection)
            {
                items.Add(ConvertToDto(file, parentFolder));
            }
            return items;
        }

        public List<SPTreeNodeDto> GetItems(IAveFolder parentFolder, ref string pageInfo, uint perPage)
        {
            List<SPTreeNodeDto> items = new List<SPTreeNodeDto>();
            AveCamlQuery query = new AveCamlQuery();
            query.ViewXml = "<View Scope=\"\">\r\n<Query>\r\n<Where>\r\n<Eq>\r\n<FieldRef Name=\"FSObjType\" />\r\n<Value Type=\"Integer\">0</Value>\r\n</Eq>\r\n</Where>\r\n</Query>\r\n<RowLimit>" + perPage + "</RowLimit></View>";
            query.FolderServerRelativeUrl = parentFolder.ServerRelativeUrl;
            if (!string.IsNullOrEmpty(pageInfo))
            {
                query.ListItemCollectionPosition = new AveItemCollectionPosition() { PagingInfo = pageInfo };
            }
            IAveListItemCollection itemCollection = parentFolder.ParentList.GetItems(query);

            if (itemCollection.ListItemCollectionPosition != null)
            {
                pageInfo = itemCollection.ListItemCollectionPosition.PagingInfo;
            }
            else
            {
                pageInfo = string.Empty;
            }

            foreach (IAveListItem item in itemCollection)
            {
                if (item.Folder == null)// 将folder过滤掉
                {
                    items.Add(ConvertToDto(item, parentFolder.ParentList, parentFolder));
                }
            }
            return items;
        }

        public List<SPTreeNodeDto> GetItems(Guid parentWebId, Guid parentFolderUniqueId, string parentFolderServerRelatedUrl, ref string pageInfo, uint perPage, int siteLockStatus)
        {
            List<SPTreeNodeDto> items = new List<SPTreeNodeDto>();
            List<AveItemBrowserInfo> itemsInfo = Query.GetBrowserItems(parentWebId, parentFolderUniqueId, parentFolderServerRelatedUrl, ref pageInfo, perPage);
            foreach (AveItemBrowserInfo item in itemsInfo)
            {
                items.Add(ConvertToDto(item, siteLockStatus));
            }
            return items;
        }

        public List<SPTreeNodeDto> GetSubFolders(IAveFolder parentFolder, int siteLockStatus)
        {
            List<SPTreeNodeDto> subFolders = new List<SPTreeNodeDto>();
            List<AveFolderBrowserInfo> subFolderInfos = Query.GetBrowserSubFolders(parentFolder.ParentWeb.ID, parentFolder.ParentListId, parentFolder.UniqueId, parentFolder.ServerRelativeUrl, false);
            foreach (AveFolderBrowserInfo folder in subFolderInfos)
            {
                if (parentFolder.ParentListId == Guid.Empty)
                {
                    if (folder.ParentListId != Guid.Empty)
                    {
                        continue;
                    }
                }
                else if (folder.Hidden)
                {
                    continue;
                }
                subFolders.Add(ConvertToDto(folder, siteLockStatus));
            }
            return subFolders;
        }

        public List<SPTreeNodeDto> GetSubFolders(Guid parentWebId, Guid parentListId, Guid parentFolderId, string parentFolderServerRelativeUrl, int siteLockStatus)
        {
            List<SPTreeNodeDto> subFolders = new List<SPTreeNodeDto>();
            List<AveFolderBrowserInfo> subFolderInfos = Query.GetBrowserSubFolders(parentWebId, parentListId, parentFolderId, parentFolderServerRelativeUrl, false);
            foreach (AveFolderBrowserInfo folder in subFolderInfos)
            {
                if (parentListId == Guid.Empty)
                {
                    if (folder.ParentListId != Guid.Empty)
                    {
                        continue;
                    }
                }
                else if (folder.Hidden)
                {
                    continue;
                }
                subFolders.Add(ConvertToDto(folder, siteLockStatus));
            }
            return subFolders;
        }

        protected SPTreeNodeDto ConvertToDto(AveItemBrowserInfo item, int siteLockStatus)
        {
            SPTreeNodeDto itemNode = new SPTreeNodeDto();
            itemNode.Level = NodeLevel.Item;
            itemNode.FullPath = item.Url;
            itemNode.Name = item.Name;
            if (item.ParentListID != Guid.Empty)
            {
                if (string.IsNullOrEmpty(itemNode.Name))
                {
                    itemNode.Name = item.ID.ToString();
                }
                // SAAS-12414 ListBaseType为DocumentLibrary时，页面显示需要带后缀
                itemNode.DisplayName = (NodeType)item.ListBaseType == NodeType.DocumentLibrary ? item.Name : item.DisplayName;

                itemNode.Type = ((NodeType)item.ListBaseType) == NodeType.DocumentLibrary ? NodeType.Document : NodeType.ListItem;
            }
            else
            {
                itemNode.DisplayName = item.Name;
                itemNode.Type = NodeType.Document;

            }
            itemNode.ParentId = item.ParentFolderUniqueID.ToString();
            itemNode.SPObjectId = item.UniqueId.ToString();
            itemNode.ItemRowId = item.ID;
            itemNode.InheritingPermissions = !item.HasUniqueRoleAssignments;
            itemNode.FarmID = FarmId;
            itemNode.SiteLockStatus = siteLockStatus;
            itemNode.NodeExtension = FillNodeExtension(itemNode.NodeExtension, item);
            return itemNode;
        }

        protected SPTreeNodeDto ConvertToDto(IAveListItem Item, IAveList parentList)
        {
            return ConvertToDto(Item, parentList, null);
        }

        protected SPTreeNodeDto ConvertToDto(IAveListItem Item, IAveList parentList, IAveFolder parentFolder)
        {
            SPTreeNodeDto itemNode = new SPTreeNodeDto();
            itemNode.Level = NodeLevel.Item;
            itemNode.FullPath = Item.Url;
            itemNode.Name = Item.Name;
            if (string.IsNullOrEmpty(itemNode.Name))
            {
                itemNode.Name = Item.ID.ToString();
            }
            itemNode.DisplayName = Item.DisplayName;
            if (parentFolder != null)
            {
                itemNode.ParentId = parentFolder.UniqueId.ToString();
            }
            itemNode.SPObjectId = Item.UniqueId.ToString();
            itemNode.FarmID = FarmId;
            itemNode.InheritingPermissions = !Item.HasUniqueRoleAssignments;
            itemNode.Type = ((NodeType)parentList.BaseType) == NodeType.DocumentLibrary ? NodeType.Document : NodeType.ListItem;
            itemNode.NodeExtension = FillNodeExtension(itemNode.NodeExtension, Item);
            return itemNode;
        }

        protected SPTreeNodeDto ConvertToDto(IAveFile file, IAveFolder parentFolder)
        {
            SPTreeNodeDto fileNode = new SPTreeNodeDto();
            fileNode.Level = NodeLevel.Item;
            fileNode.FullPath = file.Url;
            fileNode.Name = file.Name;
            fileNode.DisplayName = file.Name;
            fileNode.ParentId = parentFolder.UniqueId.ToString();
            fileNode.SPObjectId = file.UniqueId.ToString();
            fileNode.InheritingPermissions = file.Item != null ? !file.Item.HasUniqueRoleAssignments : false;
            fileNode.FarmID = FarmId;
            fileNode.Type = NodeType.Document;
            fileNode.NodeExtension = FillNodeExtension(fileNode.NodeExtension, file);
            return fileNode;
        }

        protected SPTreeNodeDto ConvertToDto(AveFolderBrowserInfo folderInfo, int siteLockStatus)
        {
            SPTreeNodeDto folderNode = new SPTreeNodeDto();

            folderNode.FullPath = folderInfo.ServerRelativeUrl;
            folderNode.Name = folderInfo.Name;
            folderNode.DisplayName = folderInfo.Name;
            folderNode.Url = folderInfo.Url;//new Uri(new Uri(folderInfo.ParentWebUrl), folderInfo.ServerRelativeUrl).ToString();

            folderNode.ParentId = folderInfo.UniqueId.ToString();
            if (folderInfo.ParentListId == Guid.Empty)//Web Root Folder
            {
                folderNode.InheritingPermissions = true;
            }
            else
            {
                folderNode.InheritingPermissions = !folderInfo.HasUniqueRoleAssignments;
            }
            folderNode.Level = NodeLevel.Folder;

            folderNode.SPObjectId = folderInfo.UniqueId.ToString();
            folderNode.FarmID = FarmId;
            folderNode.CMFlag = GetFolderFlag(folderInfo, folderNode);
            folderNode.HasSubFolder = true;
            folderNode.SiteLockStatus = siteLockStatus;
            folderNode.NodeExtension = FillNodeExtension(folderNode.NodeExtension, folderInfo);
            return folderNode;
        }

        protected SPTreeNodeDto ConvertToDto(IAveFolder folder, bool isRootFolder)
        {
            SPTreeNodeDto folderNode = new SPTreeNodeDto();

            folderNode.FullPath = folder.ServerRelativeUrl;
            folderNode.Name = folder.Name;
            folderNode.DisplayName = folder.Name;
            folderNode.Url = new Uri(new Uri(folder.ParentWeb.Url), folder.ServerRelativeUrl).ToString();
            if (isRootFolder)
            {
                folderNode.ParentId = folder.ParentListId.ToString();
                if (folder.ParentListId == Guid.Empty)//Web Root Folder
                {
                    folderNode.InheritingPermissions = true;
                }
                else
                {
                    folderNode.InheritingPermissions = !folder.ParentList.HasUniqueRoleAssignments;
                }
                folderNode.Level = NodeLevel.RootFolder;
            }
            else
            {
                folderNode.ParentId = folder.ParentFolder.UniqueId.ToString();
                if (folder.ParentListId == Guid.Empty)//Web Root Folder
                {
                    folderNode.InheritingPermissions = true;
                }
                else
                {
                    folderNode.InheritingPermissions = !folder.Item.HasUniqueRoleAssignments;
                }
                folderNode.Level = NodeLevel.Folder;
            }
            folderNode.SPObjectId = folder.UniqueId.ToString();
            folderNode.FarmID = FarmId;
            folderNode.CMFlag = GetFolderFlag(folder, folderNode);
            folderNode.HasSubFolder = true;
            folderNode.NodeExtension = FillNodeExtension(folderNode.NodeExtension, folder);
            return folderNode;
        }

        protected SPTreeNodeDto ConvertToRootFolderDto(AveFolderBrowserInfo folder)
        {
            SPTreeNodeDto folderNode = new SPTreeNodeDto();
            folderNode.FullPath = folder.ServerRelativeUrl;
            folderNode.Name = folder.Name;
            folderNode.DisplayName = folder.Name;
            folderNode.Url = folder.Url;
            folderNode.ParentId = folder.ParentListId.ToString();
            if (folder.ParentListId == Guid.Empty)//Web Root Folder
            {
                folderNode.InheritingPermissions = true;
            }
            else
            {
                folderNode.InheritingPermissions = !folder.HasUniqueRoleAssignments;
            }
            folderNode.Level = NodeLevel.RootFolder;
            folderNode.SPObjectId = folder.UniqueId.ToString();
            folderNode.FarmID = FarmId;
            folderNode.CMFlag = GetFolderFlag(folder, folderNode);
            folderNode.HasSubFolder = true;
            folderNode.NodeExtension = FillNodeExtension(folderNode.NodeExtension, folder);
            return folderNode;
        }

        /// <summary>
        /// This method is only used for Content Manager
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="folderDto"></param>
        /// <returns></returns>
        private int GetFolderFlag(IAveFolder folder, SPTreeNodeDto folderDto)
        {
            if (folder.ParentListId == Guid.Empty)
            {
                folderDto.CMFlag |= (int)FolderFlag.ListIdIsNull;//1
            }
            else
            {
                folderDto.CMFlag = ~((~folderDto.CMFlag) | (int)FolderFlag.ListIdIsNull);//0
            }
            if (folder.Item == null)
            {
                folderDto.CMFlag |= (int)FolderFlag.DoclibRowIdIsNull;//1
            }
            else
            {
                folderDto.CMFlag = ~((~folderDto.CMFlag) | (int)FolderFlag.DoclibRowIdIsNull);//0
            }
            return folderDto.CMFlag;
        }

        private int GetFolderFlag(AveFolderBrowserInfo folder, SPTreeNodeDto folderDto)
        {
            if (folder.ParentListId == Guid.Empty)
            {
                folderDto.CMFlag |= (int)FolderFlag.ListIdIsNull;//1
            }
            else
            {
                folderDto.CMFlag = ~((~folderDto.CMFlag) | (int)FolderFlag.ListIdIsNull);//0
            }
            if (folder.Hidden)
            {
                folderDto.CMFlag |= (int)FolderFlag.DoclibRowIdIsNull;//1
            }
            else
            {
                folderDto.CMFlag = ~((~folderDto.CMFlag) | (int)FolderFlag.DoclibRowIdIsNull);//0
            }
            return folderDto.CMFlag;
        }

        public SPTreeNodeDto ConvertToFolderDto(IAveFolder folder, bool isRootFolder)
        {
            return ConvertToDto(folder, isRootFolder);
        }
    }
    public enum FolderFlag
    {
        DoclibRowIdIsNull = 1,
        ListIdIsNull = 2
    }
}
