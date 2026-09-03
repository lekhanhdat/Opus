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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{

    public enum ListFlag
    {
        IsDesignList = 1
    }

    public class ListLevel : IndividualBase
    {
        private string mSiteUrl = string.Empty;

        public ListLevel(AveObjectModelFactory objectModel)
            : base(objectModel, string.Empty, string.Empty)
        {

        }

        public ListLevel(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
            : base(objectModel, sqlConnString, siteUrl)
        {
            mSiteUrl = siteUrl;
        }

        public IAveList GetList(IAveWeb web, string listUrl)
        {
            return web.GetList(listUrl);
        }

        public virtual List<AveListBrowserInfo> GetListInfo(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            return Query.GetBrowserLists(parentWebId, startIndex, perPage, ref childrenCount);
        }

        public List<SPTreeNodeDto> Getlists(Guid parentWebId, int siteLockStatus, int startIndex, uint perPage, ref int childrenCount)
        {
            startIndex = startIndex == 0 ? 0 : --startIndex; //由于计算了system folder需要减去1
            List<SPTreeNodeDto> lists = new List<SPTreeNodeDto>();
            List<AveListBrowserInfo> listsInfo = GetListInfo(parentWebId, startIndex, perPage, ref childrenCount);

            listsInfo?.ForEach(l => lists?.Add(ConvertToDto(l, siteLockStatus)));
            lists?.Sort(new SPTreeNodeDtoComparer());
            if (startIndex == 0)
            {
                SPTreeNodeDto systemFolder = new SPTreeNodeDto();
                systemFolder.HasSubFolder = true;
                systemFolder.SPObjectId = Guid.Empty.ToString();
                systemFolder.Name = "{System Folder}";
                systemFolder.Level = NodeLevel.List;
                systemFolder.FullPath = AveUrlUtility.GetSiteServerRelativeUrl(mSiteUrl).TrimEnd('/') + "/Lists/{System Folder}";
                systemFolder.FarmID = FarmId;
                systemFolder.SiteLockStatus = siteLockStatus;
                systemFolder.NodeExtension = FillNodeExtension(systemFolder.NodeExtension, systemFolder);
                lists.Insert(0, systemFolder);
            }
            childrenCount++;
            return lists;
        }

        protected SPTreeNodeDto ConvertToDto(AveListBrowserInfo list, int siteLockStatus)
        {
            SPTreeNodeDto listDto = new SPTreeNodeDto();
            listDto.HasSubFolder = true;
            listDto.Name = list.Title;
            listDto.InheritingPermissions = !list.HasUniqueRoleAssignments;
            listDto.FullPath = list.Url;
            listDto.SPObjectId = list.ID.ToString();
            listDto.DisplayName = listDto.Name;
            listDto.Url = list.Url;
            listDto.Level = NodeLevel.List;
            listDto.Template = (int)list.BaseTemplate;
            listDto.Type = (NodeType)list.BaseType;
            listDto.Hidden = list.BaseTemplate != (int)AveListTemplateType.SolutionCatalog && list.Hidden;
            if (listDto.NodeExtension == null)
            {
                listDto.NodeExtension = new NodeExtensionDto();
            }
            listDto.FolderCreation = list.EnableFolderCreation;
            listDto.CMFlag |= IsDesignList(list);
            listDto.FarmID = FarmId;
            listDto.SiteLockStatus = siteLockStatus;
            listDto.NodeExtension = FillNodeExtension(listDto.NodeExtension, list);
            return listDto;
        }

        public List<SPTreeNodeDto> Getlists(IAveWeb web, int siteLockStatus)
        {
            List<SPTreeNodeDto> lists = new List<SPTreeNodeDto>();
            Uri webUri = new Uri(web.Url);//代码中应该尽量避免相同代码的重复处理，所以添加webUri属性；
            foreach (IAveList list in web.BrowserLists)
            {
                lists.Add(ConvertToDto(list, siteLockStatus, webUri));
            }
            lists.Sort(new SPTreeNodeDtoComparer());
            SPTreeNodeDto systemFolder = new SPTreeNodeDto();
            systemFolder.HasSubFolder = true;
            systemFolder.SPObjectId = Guid.Empty.ToString();
            systemFolder.Name = "{System Folder}";
            systemFolder.Level = NodeLevel.List;
            systemFolder.FullPath = web.ServerRelativeUrl + "/Lists/{System Folder}";
            systemFolder.FarmID = FarmId;
            systemFolder.SiteLockStatus = siteLockStatus;
            systemFolder.NodeExtension = FillNodeExtension(systemFolder.NodeExtension, systemFolder);
            lists.Insert(0, systemFolder);
            return lists;
        }

        private SPTreeNodeDto ConvertToDto(IAveList list, int siteLockStatus)
        {
            return ConvertToDto(list, siteLockStatus, null);
        }

        private SPTreeNodeDto ConvertToDto(IAveList list, int siteLockStatus, Uri webUri)
        {
            SPTreeNodeDto listDto = new SPTreeNodeDto();

            listDto.HasSubFolder = true;
            listDto.SPObjectId = list.ID.ToString();
            listDto.Name = list.Title;
            listDto.DisplayName = list.Title;
            listDto.Url = new Uri(webUri == null ? new Uri(list.ParentWeb.Url) : webUri, list.RootFolder.ServerRelativeUrl).ToString();
            listDto.Level = NodeLevel.List;
            listDto.Template = (int)list.BaseTemplate;
            listDto.Type = (NodeType)list.BaseType;
            listDto.FullPath = list.RootFolder.ServerRelativeUrl;
            listDto.Hidden = list.BaseTemplate != AveListTemplateType.SolutionCatalog && list.Hidden;
            //TO DO:listDto.ImageUrl=list.ImageUrl
            listDto.InheritingPermissions = !list.HasUniqueRoleAssignments;
            listDto.FarmID = FarmId;
            //listDto.ChildrenCount = list.ItemCount;
            listDto.FolderCreation = list.EnableFolderCreation;
            listDto.CMFlag |= IsDesignList(list);
            listDto.SiteLockStatus = siteLockStatus;
            listDto.NodeExtension = FillNodeExtension(listDto.NodeExtension, list);
            return listDto;

        }

        private int IsDesignList(IAveList list)
        {
            if ((list.Hidden == true || BrowserTreeUtility.DesignLists.Contains(list.RootFolder.Name.ToUpper() + "," + ((int)list.BaseTemplate).ToString())))
            {
                return (int)ListFlag.IsDesignList;
            }
            return 0;
        }

        private int IsDesignList(AveListBrowserInfo list)
        {
            if ((list.Hidden == true || BrowserTreeUtility.DesignLists.Contains(list.rootFolderName.ToUpper() + "," + ((int)list.BaseTemplate).ToString())))
            {
                return (int)ListFlag.IsDesignList;
            }
            return 0;
        }

        public SPTreeNodeDto ConvertToListDto(IAveList list, int siteLockStatus)
        {
            return ConvertToDto(list, siteLockStatus);
        }
    }
}
