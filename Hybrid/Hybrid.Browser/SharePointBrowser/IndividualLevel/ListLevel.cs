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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{

    public enum ListFlag
    {
        IsDesignList = 1
    }
    public class ListLevel : IndividualBase
    {
        public ListLevel(AveObjectModelFactory objectModel)
            : base(objectModel, string.Empty, string.Empty)
        {

        }

        public ListLevel(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
            : base(objectModel, sqlConnString, siteUrl)
        {

        }

        public IAveList GetList(IAveWeb web, string listUrl)
        {
            return web.GetList(listUrl);
        }

        public List<SPTreeNodeDto> Getlists(Guid siteId, Guid parentWebId, uint siteLockStatus, int startIndex, uint perPage, ref int childrenCount)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> lists = new List<SPTreeNodeDto>();
            AveBrowserOption option = new AveBrowserOption
            {
                ParentSiteId = siteId,
                ParentWebId = parentWebId,
                NeedPaging = true,
                StartIndex = startIndex,
                PerPage = perPage,
                SiteUrl = siteUrl
            };
            List<AveListBrowserInfo> listsInfo = Query.GetBrowserLists(option); //Query.GetBrowserLists(siteId, parentWebId, startIndex, perPage, ref childrenCount, siteUrl);
            listsInfo.ForEach(l => lists.Add(ConvertToDto(l, siteLockStatus)));
            childrenCount = option.ChildrenTotalCount;
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower Webs Elapsed Time: {0}, WebCount: {1}, SiteId: {2}, ParentWebId: {3}", sw.Elapsed.ToString(), lists.Count, siteId, parentWebId);
#endif
            return lists;
        }

        protected SPTreeNodeDto ConvertToDto(AveListBrowserInfo list, uint siteLockStatus)
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
            listDto.Hidden = list.Hidden;
            if (listDto.NodeExtension == null)
            {
                listDto.NodeExtension = new NodeExtensionDto();
            }
            listDto.FolderCreation = list.EnableFolderCreation;
            listDto.CMFlag |= IsDesignList(list);
            listDto.FarmID = FarmId;
            listDto.SiteLockStatusValue = siteLockStatus;
            listDto.NodeExtension = FillNodeExtension(listDto.NodeExtension, list);
            return listDto;
        }

        private int IsDesignList(AveListBrowserInfo list)
        {
            if (list.Hidden == true || BrowserTreeUtility.DesignLists.Contains(list.rootFolderName.ToUpper(CultureInfo.InvariantCulture) + "," + ((int)list.BaseTemplate).ToString()))
            {
                return (int)ListFlag.IsDesignList;
            }
            return 0;
        }




        #region 原来的API实现
        private int IsDesignList(IAveList list)
        {
            if (list.Hidden == true || BrowserTreeUtility.DesignLists.Contains(list.RootFolder.Name.ToUpper(CultureInfo.InvariantCulture) + "," + ((int)list.BaseTemplate).ToString()))
            {
                return (int)ListFlag.IsDesignList;
            }
            return 0;
        }

        public SPTreeNodeDto ConvertToListDto(IAveList list, uint siteLockStatus)
        {
            return ConvertToDto(list, siteLockStatus);
        }

        private SPTreeNodeDto ConvertToDto(IAveList list, uint siteLockStatus)
        {
            SPTreeNodeDto listDto = new SPTreeNodeDto();

            listDto.HasSubFolder = true;
            listDto.SPObjectId = list.ID.ToString();
            listDto.Name = list.Title;
            listDto.DisplayName = list.Title;
            listDto.Url = new Uri(new Uri(list.ParentWeb.Url), list.RootFolder.ServerRelativeUrl).ToString();
            listDto.Level = NodeLevel.List;
            listDto.Template = (int)list.BaseTemplate;
            listDto.Type = (NodeType)list.BaseType;
            listDto.FullPath = list.RootFolder.ServerRelativeUrl;
            listDto.Hidden = list.Hidden;
            //TO DO:listDto.ImageUrl=list.ImageUrl
            listDto.InheritingPermissions = !list.HasUniqueRoleAssignments;
            listDto.FarmID = FarmId;
            //listDto.ChildrenCount = list.ItemCount;
            listDto.FolderCreation = list.EnableFolderCreation;
            listDto.CMFlag |= IsDesignList(list);
            listDto.SiteLockStatusValue = siteLockStatus;
            listDto.NodeExtension = FillNodeExtension(listDto.NodeExtension, list);
            return listDto;

        }

        public List<SPTreeNodeDto> Getlists(IAveWeb web, uint siteLockStatus)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> lists = new List<SPTreeNodeDto>();
            foreach (IAveList list in web.Lists)
            {
                lists.Add(ConvertToDto(list, siteLockStatus));
            }
            lists.Sort(new SPTreeNodeDtoComparer());
            SPTreeNodeDto systemFolder = new SPTreeNodeDto();
            systemFolder.HasSubFolder = true;
            systemFolder.SPObjectId = Guid.Empty.ToString();
            systemFolder.Name = "{System Folder}";
            systemFolder.Level = NodeLevel.List;
            systemFolder.FullPath = web.ServerRelativeUrl + "/Lists/{System Folder}";
            systemFolder.FarmID = FarmId;
            systemFolder.SiteLockStatusValue = siteLockStatus;
            systemFolder.NodeExtension = FillNodeExtension(systemFolder.NodeExtension, systemFolder);
            lists.Insert(0, systemFolder);
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower Lists Elapsed Time: {0}, ListCount: {1}, WebUrl: {2}", sw.Elapsed.ToString(), lists.Count, web.Url);
#endif
            return lists;
        }
        #endregion

    }
}
