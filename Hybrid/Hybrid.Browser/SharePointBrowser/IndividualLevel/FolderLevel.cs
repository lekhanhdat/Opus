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

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{
    public class FolderLevel : IndividualBase
    {
        public FolderLevel(AveObjectModelFactory objectModel)
            : base(objectModel, string.Empty, string.Empty)
        { }

        public FolderLevel(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl, bool isForceNativeModel = false)
            : base(objectModel, sqlConnString, siteUrl, isForceNativeModel)
        {

        }

        public SPTreeNodeDto GetRootFolder(Guid siteId, Guid parentWebID, Guid parentListId, uint siteLockStatus)
        {
            AveFolderBrowserInfo rootFolderInfo = Query.GetBrowserRootFolder(new AveBrowserOption { ParentSiteId = siteId, ParentWebId = parentWebID, ParentListId = parentListId, SiteUrl = siteUrl }); //Query.GetBrowserRootFolder(siteId, parentWebID, parentListId, siteUrl);
            var result = ConvertToDto(rootFolderInfo, siteLockStatus, true);
            result.Name = GCommon.Utility.GConstants.SPNodeName.RootFolder;
            result.DisplayName = GCommon.Utility.GConstants.SPNodeName.RootFolder;
            return result;
        }

        /// <summary>
        /// 任何list都要给root folder
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<SPTreeNodeDto> GetRootFolder(IAveList list, uint siteLockStatus)
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
            dto.SiteLockStatusValue = siteLockStatus;
            subFolders.Add(dto);
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower RootFolder Elapsed Time: {0}, RootFolderCount: {1}, ParentWeb: {2}, ListTitle: {2}", sw.Elapsed.ToString(), subFolders.Count, list.ParentWebUrl, list.Title);
#endif
            return subFolders;
        }

        public List<SPTreeNodeDto> GetRootFolder(IAveWeb web, uint siteLockStatus)
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
            dto.SiteLockStatusValue = siteLockStatus;
            subFolders.Add(dto);
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower RootFolder Elapsed Time: {0}, RootFolderCount: {1}, ParentWeb: {2}", sw.Elapsed.ToString(), subFolders.Count, web.Url);
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
            //query.ViewXml = "<View><RowLimit>"+perPage+"</RowLimit></View>";
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

        public List<SPTreeNodeDto> GetItems(Guid siteId, Guid parentWebId, Guid parentFolderUniqueId, string parentFolderServerRelatedUrl, ref string pageInfo, uint perPage, uint siteLockStatus)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> items = new List<SPTreeNodeDto>();
            AveBrowserOption option = new AveBrowserOption
            {
                ParentSiteId = siteId,
                ParentWebId = parentWebId,
                ParentFolderId = parentFolderUniqueId,
                ParentFolderServerRelativeUrl = parentFolderServerRelatedUrl,
                NeedPaging = true,
                PageInfo = pageInfo,
                PerPage = perPage,
                SiteUrl = siteUrl
            };
            List<AveItemBrowserInfo> itemsInfo = Query.GetBrowserItems(option); //Query.GetBrowserItems(siteId, parentWebId, parentFolderUniqueId, parentFolderServerRelatedUrl, ref pageInfo, perPage, siteUrl);
            itemsInfo.ForEach(i => items.Add(ConvertToDto(i, siteLockStatus)));
            pageInfo = option.PageInfo;

#if DEBUG
            sw.Stop();
            Logger.Debug("Brower Items Elapsed Time: {0}, ItemCount: {1}, SiteId: {2}, ParentWebId: {3}, ParentFolderUrl: {4}, PageInfo: {5}, PerPage: {6}",
                sw.Elapsed.ToString(), items.Count, siteId, parentWebId, parentFolderServerRelatedUrl, pageInfo, perPage);
#endif
            return items;
        }

        public List<SPTreeNodeDto> GetSubFolders(IAveFolder parentFolder, uint siteLockStatus)
        {
            int childrenCount = 0;
            string pageInfo = "";
            return GetSubFolders(parentFolder.ParentWeb.Site.ID, parentFolder.ParentWeb.ID, parentFolder.ParentListId, parentFolder.UniqueId,
parentFolder.ServerRelativeUrl, siteLockStatus, 0, uint.MaxValue, ref childrenCount, ref pageInfo);
        }

        public List<SPTreeNodeDto> GetSubFolders(Guid siteId, Guid webId, Guid parentListId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, uint siteLockStatus, int startIndex, uint perPage, ref int childrenCount, ref string pageInfo, string siteUrl = "")
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> subFolders = new List<SPTreeNodeDto>();
            AveBrowserOption option = new AveBrowserOption
            {
                ParentSiteId = siteId,
                ParentWebId = webId,
                ParentListId = parentListId,
                ParentFolderId = parentFolderUniqueId,
                ParentFolderServerRelativeUrl = parentFolderServerRelativeUrl,
                NeedPaging = true,
                StartIndex = startIndex,
                PerPage = perPage,
                NeedFilter = true,
                FilterSystemFolder = true,
                SiteUrl = siteUrl,
                PageInfo = pageInfo,
            };
            List<AveFolderBrowserInfo> subFolderInfos = Query.GetBrowserSubFolders(option);  //Query.GetBrowserSubFolders(siteId, webId, parentListId, parentFolderUniqueId, parentFolderServerRelativeUrl, siteUrl);
            subFolderInfos.ForEach(f => subFolders.Add(ConvertToDto(f, siteLockStatus, false)));
            childrenCount = option.ChildrenTotalCount;
            pageInfo = option.PageInfo;
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower SubFolders Elapsed Time: {0}, SubFolderCount: {1}, ParentFolderUrl: {2}", sw.Elapsed.ToString(), subFolders.Count, parentFolderServerRelativeUrl);
#endif
            return subFolders;
        }

        protected SPTreeNodeDto ConvertToDto(AveItemBrowserInfo item, uint siteLockStatus)
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
                itemNode.DisplayName = item.Name;
                itemNode.Type = ((NodeType)item.ListBaseType) == NodeType.DocumentLibrary ? NodeType.Document : NodeType.ListItem;
            }
            else
            {
                itemNode.DisplayName = item.Name;
                itemNode.Type = NodeType.Document;

            }
            itemNode.ParentId = item.ParentFolderUniqueID.ToString();
            itemNode.SPObjectId = item.UniqueId.ToString();
            itemNode.InheritingPermissions = !item.HasUniqueRoleAssignments;
            itemNode.FarmID = FarmId;
            itemNode.SiteLockStatusValue = siteLockStatus;
            itemNode.NodeExtension = FillNodeExtension(itemNode.NodeExtension, item);
            return itemNode;
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





        #region 原来的Get Folder API实现
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

        public SPTreeNodeDto ConvertToFolderDto(IAveFolder folder, bool isRootFolder)
        {
            return ConvertToDto(folder, isRootFolder);
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
            itemNode.DisplayName = Item.Name;
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

        protected SPTreeNodeDto ConvertToDto(AveFolderBrowserInfo folderInfo, uint siteLockStatus, bool isRootFodler)
        {
            SPTreeNodeDto folderNode = new SPTreeNodeDto();

            folderNode.FullPath = folderInfo.ServerRelativeUrl;
            folderNode.Name = folderInfo.Name;
            folderNode.DisplayName = folderInfo.Name;
            folderNode.Url = folderInfo.Url;//new Uri(new Uri(folderInfo.ParentWebUrl), folderInfo.ServerRelativeUrl).ToString();

            folderNode.ParentId = folderInfo.ParentId.ToString();
            if (folderInfo.ParentListId == Guid.Empty)//Web Root Folder
            {
                folderNode.InheritingPermissions = true;
            }
            else
            {
                folderNode.InheritingPermissions = !folderInfo.HasUniqueRoleAssignments;
            }
            if (isRootFodler)
            {
                folderNode.Level = NodeLevel.RootFolder;
            }
            else
            {
                folderNode.Level = NodeLevel.Folder;
            }
            folderNode.SPObjectId = folderInfo.UniqueId.ToString();
            folderNode.FarmID = FarmId;
            folderNode.CMFlag = GetFolderFlag(folderInfo, folderNode);
            folderNode.HasSubFolder = true;
            folderNode.SiteLockStatusValue = siteLockStatus;
            folderNode.NodeExtension = FillNodeExtension(folderNode.NodeExtension, folderInfo);
            return folderNode;
        }
        #endregion
    }
    public enum FolderFlag
    {
        DoclibRowIdIsNull = 1,
        ListIdIsNull = 2
    }
}
