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

namespace Microsoft365.SharePoint
{
    using Microsoft.ProjectServer.Client;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Linq;
    internal static class BrowserInfoConvert
    {
        internal static WebBrowserInfo ConvertToWebBrowserInfo(this Web web, bool isRoot)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web is null");
            }
            return new WebBrowserInfo
            {
                ID = web.Id,
                Title = web.Title,
                Url = web.Url,
                IsRootWeb = isRoot,
                Name = isRoot ? string.Empty : web.ServerRelativeUrl.Split('/').LastOrDefault() ?? string.Empty,
                TemplateName = web.WebTemplate,
                Language = web.Language,
                HasUniqueRoleAssignments = web.HasUniqueRoleAssignments,
                ServerRelativeUrl = web.ServerRelativeUrl
            };
        }

        internal static AppBrowserInfo ConvertToAppBrowserInfo(this AppInstance appInstance)
        {
            if (appInstance == null)
            {
                throw new ArgumentNullException("appInstance is null");
            }
            return new AppBrowserInfo
            {
                Name = appInstance.Title,
                DisplayName = appInstance.Title,
                SPObjectId = appInstance.ProductId,
                Id = appInstance.Id,
                Url = !string.IsNullOrEmpty(appInstance.AppWebFullUrl) ? new Uri(appInstance.AppWebFullUrl) : null,
                Status = (int)appInstance.Status
            };
        }

        /// <summary>
        /// need have list.properties, include list.ParentWeb.Url
        /// </summary>
        /// <param name="list"></param>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        internal static ListBrowserInfo ConvertToListBrowserInfo(this List list)
        {
            return new ListBrowserInfo
            {
                BaseTemplate = list.BaseTemplate,
                BaseType = (int)list.BaseType,
                EnableFolderCreation = list.EnableFolderCreation,
                HasUniqueRoleAssignments = list.HasUniqueRoleAssignments,
                Hidden = list.Hidden,
                ID = list.Id,
                Name = list.Title,
                rootFolderName = list.RootFolder.Name,
                ServerRelativeUrl = list.RootFolder.ServerRelativeUrl,
                Title = list.Title,
                Url = new Uri(new Uri(list.ParentWeb.Url), list.RootFolder.ServerRelativeUrl).ToString()
            };
        }

        #region List/Web Root Folder
        /// <summary>
        /// requires 
        /// List.ParentWeb.Url
        /// list.id
        /// list.BaseType
        /// list.HasUniqueRoleAssignments
        /// list.RootFolder.Name
        /// list.RootFolder.UniqueId
        /// list.RootFolder.ServerRelativeUrl
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static FolderBrowserInfo ConvertToListRootFolderBrowserInfo(this List list)
        {
            var folder= ConvertToRootFolderBrowserInfoInternal(list.RootFolder,list.ParentWeb.Url);
            folder.ParentListId = list.Id;
            folder.ParentListBaseType = (int)list.BaseType;
            folder.ParentId = list.Id;
            return folder;
        }

        /// <summary>
        /// requires 
        /// web.Url
        /// web.RootFolder.Name
        /// web.RootFolder.UniqueId
        /// web.RootFolder.ServerRelativeUrl
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        internal static FolderBrowserInfo ConvertToWebRootFolderBrowserInfo(this Web web)
        {
            var folder = ConvertToRootFolderBrowserInfoInternal(web.RootFolder, web.Url);
            folder.ParentId = Guid.Empty;
            return folder;
        }

        private static FolderBrowserInfo ConvertToRootFolderBrowserInfoInternal(this Folder folder, string parentWebUrl)
        {
            var folderInfo = new FolderBrowserInfo
            {
                Name = folder.Name,
                UniqueId = folder.UniqueId,
                Url = new Uri(new Uri(parentWebUrl), folder.ServerRelativeUrl).ToString(),
                ServerRelativeUrl = folder.ServerRelativeUrl,
            };
            return folderInfo;
        }
        #endregion List/Web Root Folder

        #region System Folder
        internal static FolderBrowserInfo ConvertToBrowserInfo(this Folder folder, Guid parentFolderUniqueId,string parentWebUrl,bool systemFolder)
        {
            var folderInfo = new FolderBrowserInfo();
            folderInfo.UniqueId = folder.UniqueId;  //SAAS-13567 Fomrs的UniqueId在ListItemAllFields中是不存在的，但可以直接获取，普通的folder也可以直接获取。
            folderInfo.Name = folder.Name;
            folderInfo.ServerRelativeUrl = folder.ServerRelativeUrl;
            folderInfo.Url = new Uri(new Uri(parentWebUrl), folder.ServerRelativeUrl).ToString();//return absolute url instead of relative url.
            folderInfo.ParentId = parentFolderUniqueId;
            folderInfo.IsSystemFolder = systemFolder;  
            return folderInfo;
        }
        #endregion

        #region Items/Documents in List/Library under a folder
        internal static FolderBrowserInfo ConvertToFolderBrowserInfo(this ListItem listItem, List list, string parentWebUrl, Guid parentFolderUniqueId)
        {
            var folderInfo = new FolderBrowserInfo();
            folderInfo.UniqueId = (Guid)listItem["UniqueId"];
            folderInfo.Name = listItem["FileLeafRef"].ToString();
            folderInfo.ServerRelativeUrl = listItem["FileRef"].ToString();
            folderInfo.Url = new Uri(new Uri(parentWebUrl), folderInfo.ServerRelativeUrl).ToString();//return absolute url instead of relative url.

            folderInfo.ParentListId = list.Id;
            folderInfo.ParentListBaseType = (int)list.BaseType;
            folderInfo.ParentId = parentFolderUniqueId;
            return folderInfo;
        }

        #endregion Items/Documents in List/Library under a folder

        internal static ProjectBrowserInfo ConvertToProjectBrowserInfo(this PublishedProject project)
        {
            var projInfo = new ProjectBrowserInfo
            {
                Name = project.Name,
                ID = project.Id,
                IsEnterpriseProject = project.IsEnterpriseProject,
                EnterpriseProjectTypeId = project.EnterpriseProjectType.Id,
                IsCheckedOut = project.IsCheckedOut
            };
            if (!string.IsNullOrEmpty(project.ProjectSiteUrl))
            {
                projInfo.Url = project.ProjectSiteUrl;
            }
            return projInfo;
        }
    }
}