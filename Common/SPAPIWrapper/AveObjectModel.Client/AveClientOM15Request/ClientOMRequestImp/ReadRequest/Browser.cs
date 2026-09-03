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
    using AvePoint.GCommon.GraphAPI;
    using AvePoint.Wrapper.Common;
    using Microsoft.ProjectServer.Client;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using ClientFolder = Microsoft.SharePoint.Client.Folder;

    public partial class AveClientOM2013Request
    {
        #region IAveBrowserRequest

        #region Common Browser

        public AveWebBrowserInfo GetBrowserRootWeb()
        {
            using (var context = CreateRetryContext())
            {
                AveWebBrowserInfo webBrowserInfo = new AveWebBrowserInfo();
                try
                {
                    Web rootWeb = context.Site.RootWeb;
                    context.Load(rootWeb, w => w.ServerRelativeUrl,
                                                 w => w.Id,
                                                 w => w.Title,
                                                 w => w.Language,
                                                 w => w.HasUniqueRoleAssignments,
                                                 w => w.WebTemplate);
                    context.ExecuteQuery();
                    SetWebBrowserInfo(webBrowserInfo, rootWeb, context.Url, WebAppName);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get browser Root Web has error,details:{0}", ex.ToString());
                }
                return webBrowserInfo;
            }
        }

        public List<AveWebBrowserInfo> GetBrowserWebs(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            using (var context = CreateRetryContext())
            {
                List<AveWebBrowserInfo> webInfos = new List<AveWebBrowserInfo>();
                WebCollection subWebs = null;
                int pagingCount = 0;
                Web parentWeb = context.Site.OpenWebById(parentWebId);
                context.Load(parentWeb.Webs, webs => webs.IncludeWithDefaultProperties(w => w.HasUniqueRoleAssignments).
                                                          Where(tempWeb => tempWeb.AppInstanceId == Guid.Empty));
                context.ExecuteQuery();
                subWebs = parentWeb.Webs;
                childrenCount = subWebs.Count;
                if (startIndex > childrenCount)
                {
                    foreach (Web web in parentWeb.Webs)
                    {
                        AveWebBrowserInfo info = new AveWebBrowserInfo();
                        SetWebBrowserInfo(info, web, context.Url, WebAppName);
                        webInfos.Add(info);
                    }
                    return webInfos;
                }
                if (childrenCount - startIndex < perPage)
                {
                    pagingCount = childrenCount - startIndex;
                }
                else
                {
                    pagingCount = (int)perPage;
                }
                try
                {
                    for (int i = 0; i < pagingCount; i++)
                    {
                        AveWebBrowserInfo info = new AveWebBrowserInfo();
                        SetWebBrowserInfo(info, subWebs[startIndex + i], context.Url, WebAppName);
                        webInfos.Add(info);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", startIndex, childrenCount, ex.ToString());
                }
                return webInfos;
            }
        }

        public List<AveAppBrowserInfo> GetBrowserApps(Guid parentWebId)
        {
            using (var context = CreateRetryContext())
            {
                List<AveAppBrowserInfo> appsBrowserInfo = new List<AveAppBrowserInfo>();
                try
                {
                    Web web = context.Site.OpenWebById(parentWebId);
                    ClientObjectList<AppInstance> apps = AppCatalog.GetAppInstances(context, web);
                    context.Load(web, w => w.ServerRelativeUrl);
                    context.Load(apps, s => s.IncludeWithDefaultProperties(app => app.Title, app => app.ProductId, app => app.AppWebFullUrl, app => app.Id, app => app.Status));
                    context.ExecuteQuery();
                    //List<Dictionary<string, object>> appsMetadata = GetInstalledApps(web.ServerRelativeUrl);
                    foreach (AppInstance app in apps)
                    {
                        //if (appsMetadata == null || GetAppPropertiesById(appsMetadata, app.Id) != null)
                        {
                            appsBrowserInfo.Add(SetAppBrowserInfo(app));
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get Browser Apps has error,details:{0}", ex.ToString());
                }
                return appsBrowserInfo;
            }
        }

        public List<AveProjectBrowserInfo> GetBrowserProjects(int startIndex, uint perPage, ref int childrenCount)
        {
            using (var context = CreateProjectContext())
            {
                List<AveProjectBrowserInfo> projInfoList = new List<AveProjectBrowserInfo>();
                ProjectCollection projects = context.Projects;
                context.Load(projects, ps => ps.Include(
                    p => p.Name,
                    p => p.Id,
                    p => p.IsEnterpriseProject,
                    p => p.EnterpriseProjectType.Id,
                    p => p.ProjectSiteUrl,
                    p => p.IsCheckedOut));
                context.ExecuteQuery();

                childrenCount = projects.Count;
                int pagingCount = 0;
                if (childrenCount - startIndex < perPage)
                {
                    pagingCount = childrenCount - startIndex;
                }
                else
                {
                    pagingCount = (int)perPage;
                }
                try
                {
                    for (int i = 0; i < pagingCount; i++)
                    {
                        PublishedProject project = projects[i + startIndex];
                        projInfoList.Add(SetProjectBrowserInfo(project));
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browser projects.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", startIndex, childrenCount, ex.ToString());
                }
                return projInfoList;
            }
        }

        public List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId)
        {
            throw new NotImplementedException();
        }
        public List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            return GetBrowserLists(parentWebId, startIndex, perPage, ref childrenCount, false);
        }

        public List<AveListBrowserInfo> GetBrowserOneDriveLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            return GetBrowserLists(parentWebId, startIndex, perPage, ref childrenCount, true);
        }

        private List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount, bool browserOneDriveListOnly)
        {
            using (var context = CreateRetryContext())
            {
                var allLists = new List<List>();
                string siteUrl = string.Empty;
                List<AveListBrowserInfo> listInfoList = new List<AveListBrowserInfo>();
                Web web = context.Site.OpenWebById(parentWebId);
                context.Load(context.Site, s => s.Url);
                if (browserOneDriveListOnly)
                {
                    context.Load(
                        web.Lists, 
                        ls => ls.Include(
                            l => l.Id,
                            l => l.ParentWebUrl,
                            l => l.Title,
                            l => l.BaseType,
                            l => l.BaseTemplate,
                            l => l.Hidden,
                            l => l.EnableVersioning,
                            l => l.EnableAttachments,
                            l => l.RootFolder.ServerRelativeUrl,
                            l => l.RootFolder.Name,
                            l => l.HasUniqueRoleAssignments,
                            l => l.EnableFolderCreation).Where(l => l.BaseType == BaseType.DocumentLibrary));
                    context.ExecuteQuery();
                    allLists.AddRange(web.Lists);
                }
                else
                {
                    try
                    {
                        context.Load(
                            web.Lists, 
                            ls => ls.Include(
                                l => l.Id,
                                l => l.ParentWebUrl,
                                l => l.Title,
                                l => l.BaseType,
                                l => l.BaseTemplate,
                                l => l.Hidden,
                                l => l.EnableVersioning,
                                l => l.EnableAttachments,
                                l => l.RootFolder.ServerRelativeUrl,
                                l => l.RootFolder.Name,
                                l => l.HasUniqueRoleAssignments,
                                l => l.EnableFolderCreation).Where(l => l.BaseTemplate != (int)AveListTemplateType.Social));
                        context.ExecuteQuery();
                        allLists.AddRange(web.Lists);

                    }
                    catch (ServerUnauthorizedAccessException ex)
                    {
                        mLogger.Debug($"ServerUnauthorizedAccessException occurred, retry GetBrowserLists. {ex.Message}");
                        context.Load(context.Site, s => s.Url);
                        context.Load(
                            web.Lists,
                            ls => ls.Include(
                                l => l.Id,
                                l => l.ParentWebUrl,
                                l => l.Title,
                                l => l.BaseType,
                                l => l.BaseTemplate,
                                l => l.Hidden,
                                l => l.EnableVersioning,
                                l => l.EnableAttachments,
                                l => l.RootFolder.ServerRelativeUrl,
                                l => l.RootFolder.Name,
                                l => l.HasUniqueRoleAssignments,
                                l => l.EnableFolderCreation
                            ).Where(l => l.BaseTemplate != (int)AveListTemplateType.Social && l.BaseTemplate != (int)AveListTemplateType.UserInformation));
                        context.ExecuteQuery();
                        allLists.AddRange(web.Lists);

                        context.Load(
                            web.Lists,
                            ls => ls.Include(
                                l => l.Id,
                                l => l.ParentWebUrl,
                                l => l.Title,
                                l => l.BaseType,
                                l => l.BaseTemplate,
                                l => l.Hidden,
                                l => l.EnableVersioning,
                                l => l.EnableAttachments,
                                l => l.HasUniqueRoleAssignments,
                                l => l.EnableFolderCreation
                            ).Where(l => l.BaseTemplate == (int)AveListTemplateType.UserInformation));
                        context.ExecuteQuery();
                        allLists.AddRange(web.Lists);

                        allLists = allLists.OrderBy(l => l.Title, StringComparer.Ordinal).ToList();
                    }
                    
                }
                childrenCount = allLists.Count;
                siteUrl = context.Site.Url;
                int pagingCount = 0;
                if (childrenCount - startIndex < perPage)
                {
                    pagingCount = childrenCount - startIndex;
                }
                else
                {
                    pagingCount = (int)perPage;
                }
                try
                {
                    for (int i = 0; i < pagingCount; i++)
                    {
                        List list = allLists[i + startIndex];
                        listInfoList.Add(SetListBrowserInfo(list, siteUrl));
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browser lists.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", startIndex, childrenCount, ex.ToString());
                }
                return listInfoList;
            }
        }

        public AveFolderBrowserInfo GetBrowserRootFolder(Guid parentWebId, Guid parentListId)
        {
            AveFolderBrowserInfo rootFolderInfo = new AveFolderBrowserInfo();
            using (var context = CreateRetryContext())
            {
                try
                {
                    Web web = context.Site.OpenWebById(parentWebId);
                    List list = web.Lists.GetById(parentListId);
                    Folder folder = list.RootFolder;
                    context.Load(list, l => l.BaseType, l => l.HasUniqueRoleAssignments, l => l.Id);
                    context.Load(folder, f => f.ServerRelativeUrl, f => f.Name, f => f.UniqueId);
                    context.ExecuteQuery();
                    rootFolderInfo = SetRootFolderBrowserInfo(folder, list, mWebUrl);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get browser list root folder has error:{0}", ex.ToString());
                }
            }
            return rootFolderInfo;
        }

        public List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid parentWebId, Guid parentlistId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, bool needLoadDesignFolders)
        {
            List<AveFolderBrowserInfo> folders = new List<AveFolderBrowserInfo>();
            try
            {
                using (var context = CreateRetryContext())
                {
                    Web web = context.Site.OpenWebById(parentWebId);
                    List list = null;
                    if (parentlistId != Guid.Empty)
                    {
                        list = web.Lists.GetById(parentlistId);
                        context.Load(list);
                    }
                    Folder folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl));
                    context.Load(folder, f => f.ItemCount);
                    context.Load(folder, f => f.Properties);
                    context.ExecuteQuery();

                    int folderCount = 0;
                    if (list != null)
                    {
                        Dictionary<string, object> folderProperties = folder.Properties.FieldValues;
                        folderCount = folderProperties.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folderProperties["vti_foldersubfolderitemcount"]) : 0;
                        if (folderCount == 0 && !needLoadDesignFolders)
                        {
                            return folders;
                        }
                    }

                    if (list == null || list != null && folder.ItemCount < 5000)  //兼容DPM browser system folder
                    {
                        ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                        using (scope.StartScope())
                        {
                            using (scope.StartTry())
                            {
                                context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder,
                                                                                           f => f.ListItemAllFields.HasUniqueRoleAssignments
                                                                                          ).Where(f => f.ListItemAllFields.ServerObjectIsNull != null));//SAAS-13567 uniqueId可以直接获取到，不需要从ListItemAllFields中获取，因为Forms获取不到ListItemAllFields
                            }
                            using (scope.StartCatch())
                            {
                                context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.Properties));
                            }
                        }
                        context.ExecuteQuery();

                        foreach (Folder subFolder in folder.Folders)
                        {
                            folders.Add(SetFolderBrowserInfo(subFolder, parentlistId, parentFolderUniqueId, mWebUrl));
                        }
                    }
                    else
                    {
                        ListItemCollection listItems = null;
                        int index = 0;
                        do
                        {
                            CamlQuery camlQuery = new CamlQuery();
                            camlQuery.ViewXml = string.Format(
                                        "<View Scope='Default'>" +
                                        "<Query><Where><And><Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query>" +
                                        "<RowLimit>{2}</RowLimit>" +
                                        "</View>", index, index + 5000, 2000);
                            mLogger.Info("browser subfolders between {0} and {1}", index, index + 5000);
                            camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl);
                            listItems = list.GetItems(camlQuery);
                            context.Load(listItems, items => items.IncludeWithDefaultProperties(item => item["FSObjType"],
                                                                                                item => item.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "1"));
                            int lastIndex = index;
                            context.ExecuteQuery();
                            if (listItems.Count > 0)
                            {
                                for (int i = 0; i < listItems.Count; i++)
                                {
                                    folders.Add(SetFolderBrowserInfo(listItems[i], parentlistId, parentFolderUniqueId, mWebUrl));
                                    index = index < listItems[i].Id ? listItems[i].Id : index;
                                }
                            }
                            index = lastIndex + 2000 < index ? index : lastIndex + 2000;
                            folderCount -= listItems.Count;
                        }
                        while (folderCount > 0);
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Warn(string.Format("get browser folders failed, parent folder url: {0}", parentFolderServerRelativeUrl), e);
            }
            return folders;
        }

        public List<AveItemBrowserInfo> GetBrowserItems(Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage)
        {
            List<AveItemBrowserInfo> itemBrowserInfos = new List<AveItemBrowserInfo>();
            using (var context = CreateRetryContext())
            {
                Web parentWeb = context.Site.OpenWebById(webId);
                context.Load(parentWeb, web => web.ServerRelativeUrl);
                context.Load(parentWeb.Lists, ls => ls.Include(l => l.RootFolder, l => l.ItemCount));
                Folder parentFolder = parentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl));
                context.Load(parentFolder);
                context.ExecuteQuery();
                List list = GetParentList(parentWeb, parentFolderServerRelativeUrl);

                if (list == null)
                {
                    context.Load(parentFolder.Files, fs => fs.Include(f => f.Name));
                    foreach (Microsoft.SharePoint.Client.File file in parentFolder.Files)
                    {
                        AveItemBrowserInfo itemInfo = new AveItemBrowserInfo();
                        SetFileBrowserInfos(itemInfo, file, parentWeb.ServerRelativeUrl);
                        itemBrowserInfos.Add(itemInfo);
                    }
                }
                else
                {
                    context.Load(parentFolder, f => f.ListItemAllFields["UniqueId"]);
                    if (parentFolder.ItemCount > 5000)
                    {
                        return GetBrowserItemsFromLargeList(parentWeb, list, parentFolder, parentFolderServerRelativeUrl, context, ref pageInfo, perPage);
                    }
                    else
                    {
                        CamlQuery camlQuery = new CamlQuery();
                        camlQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">0</Value></Eq></Where></Query><RowLimit>" + perPage + "</RowLimit></View>";
                        camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl);
                        camlQuery.ListItemCollectionPosition = (!string.IsNullOrEmpty(pageInfo) ? new ListItemCollectionPosition { PagingInfo = pageInfo } : null);
                        ListItemCollection items = list.GetItems(camlQuery);
                        ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                        //部分folder无法使用 context.Load(items, its => its.Include(tm => tm.DisplayName))的方式load item 的display name
                        using (exceptionScope.StartScope())
                        {
                            using (exceptionScope.StartTry())
                            {
                                context.Load(items);
                                context.Load(items, its => its.Include(tm => tm.DisplayName, tm => tm.ParentList.BaseType, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments));
                            }
                            using (exceptionScope.StartCatch())
                            {
                                context.Load(items);
                                context.Load(items, its => its.Include(tm => tm.ParentList.BaseType, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments));
                            }
                        }
                        context.ExecuteQuery();
                        if (exceptionScope.HasException)
                        {
                            mLogger.Warn("Load item's display name failed,parent folder server relative url:{0}. {1}", parentFolderServerRelativeUrl, exceptionScope.ErrorMessage);
                        }
                        if (items.ListItemCollectionPosition != null)
                        {
                            pageInfo = items.ListItemCollectionPosition.PagingInfo;
                        }
                        else
                        {
                            pageInfo = null;
                        }
                        Guid parentFolderId = parentFolder.ListItemAllFields.FieldValues.Count > 0 ? (Guid)parentFolder.ListItemAllFields.FieldValues["UniqueId"] : Guid.Empty;
                        foreach (ListItem item in items)
                        {
                            AveItemBrowserInfo itemInfo = new AveItemBrowserInfo();
                            SetItemBrowserInfo(parentWeb.ServerRelativeUrl, itemInfo, item, !exceptionScope.HasException);
                            itemBrowserInfos.Add(itemInfo);
                            itemInfo.ParentFolderUniqueID = parentFolderId;
                        }
                    }
                }
                return itemBrowserInfos;
            }
        }

        public Dictionary<string, object> GetListsLightly(Guid webId)
        {
            using (var context = CreateRetryContext())
            {
                var allLists = new List<List>();
                bool hasServerUnauthorizedAccessException = false;
                Web web = context.Site.OpenWebById(webId);
                try
                {
                    context.Load(web, w => w.ServerRelativeUrl);
                    context.Load(
                        web.Lists,
                        ls => ls.Include(
                            l => l.Id,
                            l => l.Title,
                            l => l.BaseType,
                            l => l.BaseTemplate,
                            l => l.Hidden,
                            l => l.ItemCount,
                            l => l.EnableVersioning,
                            l => l.EnableAttachments,
                            l => l.RootFolder.ServerRelativeUrl,
                            l => l.RootFolder.Name,
                            l => l.HasUniqueRoleAssignments,
                            l => l.DefaultContentApprovalWorkflowId,
                            l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                            l => l.EnableFolderCreation));
                    context.ExecuteQuery();
                    allLists.AddRange(web.Lists);
                }
                catch (ServerUnauthorizedAccessException ex)
                {
                    mLogger.Debug($"ServerUnauthorizedAccessException occurred, retry GetListsLightly. {ex.Message}");
                    hasServerUnauthorizedAccessException = true;
                    context.Load(web, w => w.ServerRelativeUrl);
                    context.Load(
                        web.Lists,
                        ls => ls.Include(
                            l => l.Id,
                            l => l.Title,
                            l => l.BaseType,
                            l => l.BaseTemplate,
                            l => l.Hidden,
                            l => l.ItemCount,
                            l => l.EnableVersioning,
                            l => l.EnableAttachments,
                            l => l.RootFolder.ServerRelativeUrl,
                            l => l.RootFolder.Name,
                            l => l.HasUniqueRoleAssignments,
                            l => l.DefaultContentApprovalWorkflowId,
                            l => l.DefaultViewUrl,//2013 必须得重新取一下这个属性，否则是空，Itemversion DeleteItemVersion会用到此参数。SAAS-614,SAAS-10621
                            l => l.EnableFolderCreation
                        ).Where(l => l.BaseTemplate != (int)AveListTemplateType.UserInformation));
                    context.ExecuteQuery();
                    allLists.AddRange(web.Lists);

                    context.Load(
                        web.Lists,
                        ls => ls.Include(
                           l => l.Id,
                            l => l.Title,
                            l => l.BaseType,
                            l => l.BaseTemplate,
                            l => l.Hidden,
                            l => l.ItemCount,
                            l => l.EnableVersioning,
                            l => l.EnableAttachments,
                            l => l.HasUniqueRoleAssignments,
                            l => l.DefaultContentApprovalWorkflowId,
                            l => l.EnableFolderCreation
                        ).Where(l => l.BaseTemplate == (int)AveListTemplateType.UserInformation));
                    context.ExecuteQuery();
                    allLists.AddRange(web.Lists);

                    allLists = allLists.OrderBy(l => l.Title, StringComparer.Ordinal).ToList();
                }

                var lists = new List<IDictionary<string, object>>();
                foreach (List l in allLists)
                {
                    mLogger.Info($"Getting lightly list {(l.IsPropertyAvailable("Title") ? l.Title : $"{l.BaseTemplate}|{l.Id}")}");
                    Dictionary<string, object> listProperties = new Dictionary<string, object>();
                    CopyProperty(listProperties, l);
                    long flag = 0;
                    if (l.EnableVersioning)
                        flag |= 0x0000000000000080;
                    if (!l.EnableAttachments)
                        flag |= 0x0000000000000008;
                    listProperties["Flag"] = flag;    //Can not get this property.
                    Dictionary<string, object> rootFolderProp = new Dictionary<string, object>();
                    if(hasServerUnauthorizedAccessException && l.BaseTemplate == (int)AveListTemplateType.UserInformation)
                    {
                        rootFolderProp["Name"] = "users";
                        rootFolderProp["ServerRelativeUrl"] = $"{web.ServerRelativeUrl}/_catalogs/users";
                        listProperties["DefaultViewUrl"] = $"{web.ServerRelativeUrl}/_catalogs/users/detail.aspx";
                    }
                    else
                    {
                        AssemblRootFolderProperties(web.ServerRelativeUrl, rootFolderProp, l.RootFolder);
                    }
                    
                    listProperties["RootFolder" + AveObjectModelConstant.ObjectPropertySuffix] = rootFolderProp;
                    lists.Add(listProperties);
                }
                Dictionary<string, object> returnInfo = new Dictionary<string, object>();
                returnInfo.AddChildren(lists);
                return returnInfo;
            }
        }
        #endregion Common Browser

        #region DPM Browser

        public List<AveSolutionBrowserInfo> GetBrowserSolutionInfos()
        {
            using (var context = CreateRetryContext())
            {
                List<AveSolutionBrowserInfo> solutionBrowserInfos = new List<AveSolutionBrowserInfo>();
                List solutionGallery = context.Site.GetCatalog((int)AveListTemplateType.SolutionCatalog);
                ListItemCollection solutionItems = solutionGallery.GetItems(CamlQuery.CreateAllItemsQuery());
                context.Load(solutionItems);
                context.Load(solutionItems, sis => sis.Include(si => si.DisplayName));
                context.ExecuteQuery();
                List<Dictionary<string, object>> solutionList = new List<Dictionary<string, object>>();
                foreach (ListItem tempItem in solutionItems)
                {
                    try
                    {
                        var itemProperties = new Dictionary<string, object>();
                        itemProperties = AssembleSolutionProperties(tempItem);
                        if (tempItem.FieldValues.ContainsKey("MetaInfo"))
                        {
                            Hashtable MetaInfoTable = new MetaInfoHandler(tempItem.FieldValues["MetaInfo"].ToString()).ToHashtable();
                            if (MetaInfoTable.ContainsKey("SolutionHasAssemblies"))
                            {
                                itemProperties["SolutionHasAssemblies"] = MetaInfoTable["SolutionHasAssemblies"];
                            }
                            if (MetaInfoTable.ContainsKey("SolutionHash"))
                            {
                                itemProperties["SolutionHash"] = MetaInfoTable["SolutionHash"];
                            }
                        }
                        solutionBrowserInfos.Add(SetSolutionBrowserInfo(itemProperties));
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("get solution item failed. error message:{0}", e.ToString());
                    }
                }
                return solutionBrowserInfos;
            }
        }

        public List<AveAppBrowserInfo> GetBrowserAppsByProductId(Guid parentWebId, Guid productId)
        {
            using (var context = CreateRetryContext())
            {
                List<AveAppBrowserInfo> appList = new List<AveAppBrowserInfo>();
                try
                {
                    Web web = context.Site.OpenWebById(parentWebId);
                    ClientObjectList<AppInstance> apps = web.GetAppInstancesByProductId(productId);
                    context.Load(apps, s => s.Include(app => app.Title, app => app.ProductId, app => app.AppWebFullUrl, app => app.Id, app => app.Status));
                    context.ExecuteQuery();
                    foreach (AppInstance app in apps)
                    {
                        appList.Add(SetAppBrowserInfo(app));
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get browser Apps by productId has error,details:{0]", ex.ToString());
                }
                return appList;
            }
        }

        public AveFolderBrowserInfo GetBrowserWebRootFolder(Guid parentWebId)
        {
            AveFolderBrowserInfo rootFolderInfo = new AveFolderBrowserInfo();
            using (var context = CreateRetryContext())
            {
                try
                {
                    Web web = context.Site.OpenWebById(parentWebId);
                    Folder webRootFolder = web.RootFolder;
                    context.Load(webRootFolder, wrf => wrf.Name, wrf => wrf.ServerRelativeUrl, wrf => wrf.UniqueId);
                    context.ExecuteQuery();
                    rootFolderInfo = SetRootFolderBrowserInfo(webRootFolder, null, mWebUrl);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get browser Web Root Folder has error,details:{0]", ex.ToString());
                }
            }
            return rootFolderInfo;
        }

        public List<AveHiddenFileInfo> GetBrowserFolderHiddenFiles(Guid parentWebId, Guid parentListId, string folderServerRelativeUrl)
        {
            List<AveHiddenFileInfo> hiddenFileList = new List<AveHiddenFileInfo>();
            Folder folder = null;
            using (var context = CreateRetryContext())
            {
                try
                {
                    Web web = context.Site.OpenWebById(parentWebId);
                    folder = web.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(folderServerRelativeUrl));
                    context.Load(web, w => w.ServerRelativeUrl);
                    if (parentListId == Guid.Empty)
                    {
                        ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                        using (excepScope.StartScope())
                        {
                            using (excepScope.StartTry())
                            {
                                context.Load(folder, f => f.Files.Include(file => file.Name, file => file.Level, file => file.UIVersion, file => file.UniqueId));
                            }
                            using (excepScope.StartCatch())
                            {
                                context.Load(folder, f => f.Files);
                            }
                        }
                        context.ExecuteQuery();
                        if (excepScope.HasException)
                        {
                            mLogger.Warn("Get Files CheckedOutByUser Or Author Error, FolderUrl:{0} , Error Message:{1}", folderServerRelativeUrl, excepScope.ErrorMessage);
                        }
                    }
                    else
                    {
                        ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
                        using (excepScope.StartScope())
                        {
                            using (excepScope.StartTry())
                            {
                                context.Load(folder, f => f.Files.Include(file => file.ListItemAllFields, file => file.Name, file => file.Level, file => file.UIVersion, file => file.UniqueId));
                            }
                            using (excepScope.StartCatch())
                            {
                                context.Load(folder, f => f.Files.IncludeWithDefaultProperties(file => file.ListItemAllFields));
                            }
                        }
                        context.ExecuteQuery();
                        if (excepScope.HasException)
                        {
                            mLogger.Warn("Get Files CheckedOutByUser Or Author Error, FolderUrl:{0} , Error Message:{1}", folderServerRelativeUrl, excepScope.ErrorMessage);
                        }
                    }
                    foreach (ClientFile file in folder.Files)
                    {
                        if (file.ListItemAllFields != null && file.ListItemAllFields.FieldValues.Count > 0)
                        {
                            continue;
                        }

                        hiddenFileList.Add(SetHiddenFileInfo(file));
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn(string.Format("get files failed, parent folder url: {0}", folderServerRelativeUrl), e);
                }
                return hiddenFileList;
            }
        }

        public List<AveFieldBrowserInfo> GetBrowserFields(Guid webId, Guid listId, string fieldSource, out CultureInfo cultureInfo)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWebById(webId);
                FieldCollection fieldCollection = null;
                List<AveFieldBrowserInfo> fieldsList = new List<AveFieldBrowserInfo>();
                List list = null;
                string currentListTitle = string.Empty;
                switch (fieldSource)
                {
                    case "web.fields":
                        fieldCollection = web.Fields;
                        break;
                    case "list.fields":
                        list = web.Lists.GetById(listId);
                        fieldCollection = list.Fields;
                        context.Load(list, l => l.Title);
                        break;
                    default:
                        break;
                }
                context.Load(web, w => w.Language, w => w.Url);
                context.Load(fieldCollection, fc => fc.Include(f => f.Id, f => f.Group, f => f.InternalName, f => f.Title, f => f.Hidden));
                context.ExecuteQuery();
                cultureInfo = new CultureInfo((int)web.Language, false);
                string webUrl = web.Url;
                if (list != null)
                {
                    currentListTitle = list.Title;
                }
                ArgumentCheck.CheckNotNull(fieldCollection);
                foreach (Field field in fieldCollection)
                {
                    fieldsList.Add(SetFieldBrowserInfo(field, webUrl, currentListTitle));
                }
                return fieldsList;
            }
        }

        public List<AveContentTypeInfo> GetBrowserContentTypes(string webServerRelativeUrl, string listTitle, ContentTypeScope scope)
        {
            string tempWebUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl;
            using (var context = CreateRetryContext(tempWebUrl))
            {
                List<AveContentTypeInfo> ctInfoList = new List<AveContentTypeInfo>();
                ContentTypeCollection contentTypes = null;
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    switch (scope)
                    {
                        case ContentTypeScope.Web:
                            contentTypes = web.ContentTypes;
                            break;
                        case ContentTypeScope.List:
                            List list = web.Lists.GetByTitle(listTitle);
                            contentTypes = list.ContentTypes;
                            break;
                        default:
                            break;
                    }

                    context.Load(contentTypes, cts => cts.Include(ct => ct.Id, ct => ct.Name, ct => ct.Group, ct => ct.Hidden));
                    context.ExecuteQuery();
                    ArgumentCheck.CheckNotNull(contentTypes);
                    foreach (ContentType ct in contentTypes)
                    {
                        ctInfoList.Add(SetCTBrowserInfo(ct));
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get Browser Content Types has error,details:{0}", ex.ToString());
                }
                return ctInfoList;
            }
        }

        public List<AveWorkflowAssociationBrowserInfo> GetBrowserWorkflowAssociations(Guid webId, Guid listId, string contentTypeId, string workflowSource, out List<Guid> workflowTemplateIds)
        {
            using (var context = CreateRetryContext())
            {
                List<AveWorkflowAssociationBrowserInfo> returnInfo = new List<AveWorkflowAssociationBrowserInfo>();
                workflowTemplateIds = new List<Guid>();
                List<Dictionary<string, object>> workflows = new List<Dictionary<string, object>>();
                Microsoft.SharePoint.Client.Workflow.WorkflowAssociationCollection wfas = null;
                Web web = context.Site.OpenWebById(webId);
                List list = null;
                switch (workflowSource)
                {
                    case "web.workflow":
                        wfas = web.WorkflowAssociations;
                        break;
                    case "list.workflow":
                        list = web.Lists.GetById(listId);
                        wfas = list.WorkflowAssociations;
                        break;
                    case "web.contentTypes":
                        ContentTypeCollection cts = web.ContentTypes;
                        ContentType webContentType = cts.GetById(contentTypeId);
                        wfas = webContentType.WorkflowAssociations;
                        break;
                    case "list.contentTypes":
                        list = web.Lists.GetById(listId);
                        ContentType listContentType = list.ContentTypes.GetById(contentTypeId);
                        wfas = listContentType.WorkflowAssociations;
                        break;
                    default:
                        break;
                }
                context.Load(web.WorkflowTemplates, wfts => wfts.Include(wft => wft.Id));
                ArgumentCheck.CheckNotNull(wfas);
                context.Load(wfas, wfa => wfa.Include(wf => wf.Name, wf => wf.Id, wf => wf.BaseId, wf => wf.Created));
                context.ExecuteQuery();
                foreach (Microsoft.SharePoint.Client.Workflow.WorkflowAssociation workflow in wfas)
                {
                    returnInfo.Add(SetWorkflowAssociationBrowserInfo(workflow));
                }
                foreach (Microsoft.SharePoint.Client.Workflow.WorkflowTemplate workflowTemplate in web.WorkflowTemplates)
                {
                    workflowTemplateIds.Add(workflowTemplate.Id);
                }
                return returnInfo;
            }
        }


        #endregion DPM Browser

        #endregion IAveBrowserRequest


        #region Fill Object

        private static void SetWebBrowserInfo(AveWebBrowserInfo info, Web web, string contextUrl, string webAppName)
        {
            string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(contextUrl);
            string Url = string.Empty;
            if (web.ServerRelativeUrl.Equals("/"))
            {
                Url = webAppName;
            }
            else
            {
                if (!siteServerRelativeUrl.Equals("/"))
                {
                    Url = contextUrl.Replace(siteServerRelativeUrl, web.ServerRelativeUrl);
                }
                else//host header类型的sitecollection走一下逻辑；
                {
                    Url = string.Format("{0}/{1}", contextUrl.TrimEnd('/'), web.ServerRelativeUrl.TrimStart('/'));
                }
            }
            info.ID = web.Id;
            info.Title = web.Title;
            info.Url = Url;
            info.IsRootWeb = false;
            string name = string.Empty;
            if (web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                info.IsRootWeb = true;
            }
            else
            {
                int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                name = web.ServerRelativeUrl.Substring(lastSlashIndex + 1);
            }
            info.Name = name;
            info.TemplateName = web.WebTemplate;
            info.Language = web.Language;
            info.HasUniqueRoleAssignments = web.HasUniqueRoleAssignments;
            info.ServerRelativeUrl = web.ServerRelativeUrl;
        }

        private static void SetFileBrowserInfos(AveItemBrowserInfo itemInfo, Microsoft.SharePoint.Client.File file, string webServerRelativeUrl)
        {
            itemInfo.Url = file.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            itemInfo.ParentListID = Guid.Empty;
            itemInfo.Name = file.Name;
            string uniqueId = GetIdsFromEtag(file.ETag)[0];
            if (!string.IsNullOrEmpty(uniqueId))
            {
                itemInfo.UniqueId = new Guid(uniqueId);
            }
            else
            {
                itemInfo.UniqueId = Guid.Empty;
            }
            itemInfo.DisplayName = file.Name;
            itemInfo.ParentFolderUniqueID = Guid.Empty;
        }

        private static AveListBrowserInfo SetListBrowserInfo(List list, string siteUrl)
        {
            AveListBrowserInfo listInfo = new AveListBrowserInfo();
            listInfo.BaseTemplate = list.BaseTemplate;
            listInfo.BaseType = (int)list.BaseType;
            listInfo.EnableFolderCreation = list.EnableFolderCreation;
            listInfo.HasUniqueRoleAssignments = list.HasUniqueRoleAssignments;
            listInfo.Hidden = list.Hidden;
            listInfo.ID = list.Id;
            listInfo.Name = list.Title;
            try
            {
                listInfo.rootFolderName = list.RootFolder.Name;
                listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            }
            catch (PropertyOrFieldNotInitializedException)
            {
                if(list.BaseTemplate == (int)AveListTemplateType.UserInformation)
                {
                    listInfo.rootFolderName = "users";
                    listInfo.ServerRelativeUrl = $"{new Uri(siteUrl).LocalPath.TrimEnd('/')}/_catalogs/users";
                }
                else
                {
                    throw;
                }
            }
            
            listInfo.Title = list.Title;
            listInfo.Url = new Uri(new Uri(siteUrl), listInfo.ServerRelativeUrl).ToString();
            return listInfo;
        }

        private static AveFolderBrowserInfo SetRootFolderBrowserInfo(Folder folder, List list, string webUrl)
        {
            AveFolderBrowserInfo folderInfo = new AveFolderBrowserInfo();
            folderInfo.Name = folder.Name;
            folderInfo.UniqueId = folder.UniqueId;
            folderInfo.Url = new Uri(new Uri(webUrl), folder.ServerRelativeUrl).ToString();
            folderInfo.ServerRelativeUrl = folder.ServerRelativeUrl;
            if (list != null)
            {
                folderInfo.ParentListId = list.Id;
                folderInfo.ParentListBaseType = (int)list.BaseType;
                folderInfo.HasUniqueRoleAssignments = list.HasUniqueRoleAssignments;
            }
            return folderInfo;
        }

        private static AveFolderBrowserInfo SetFolderBrowserInfo(Folder folder, Guid parentListId, Guid parentFolderUniqueId, string webUrl)
        {
            AveFolderBrowserInfo folderInfo = new AveFolderBrowserInfo();
            folderInfo.UniqueId = folder.UniqueId;  //SAAS-13567 Fomrs的UniqueId在ListItemAllFields中是不存在的，但可以直接获取，普通的folder也可以直接获取。
            if (folder.ListItemAllFields.IsPropertyAvailable("HasUniqueRoleAssignments"))
            {
                folderInfo.HasUniqueRoleAssignments = folder.ListItemAllFields.HasUniqueRoleAssignments;
            }
            else if (folder.Properties.FieldValues.ContainsKey("vti_etag") &&
                     folder.Properties["vti_etag"] != null)
            {
                string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                folderInfo.UniqueId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
                folderInfo.HasUniqueRoleAssignments = false;
            }
            if (parentListId != Guid.Empty)
            {
                folderInfo.ParentListId = parentListId;
            }
            else if (folder.Properties.FieldValues.ContainsKey("vti_listname") &&
                     folder.Properties["vti_listname"] != null)
            {
                folderInfo.ParentListId = new Guid(folder.Properties["vti_listname"].ToString());
            }
            folderInfo.Name = folder.Name;
            folderInfo.ServerRelativeUrl = folder.ServerRelativeUrl;
            folderInfo.Url = new Uri(new Uri(webUrl), folder.ServerRelativeUrl).ToString();//return absolute url instead of relative url.
            //folderInfo.ParentListId = parentlistId;
            folderInfo.ParentId = parentFolderUniqueId;
            folderInfo.Hidden = folder.ListItemAllFields.ServerObjectIsNull.HasValue ? folder.ListItemAllFields.ServerObjectIsNull.Value : true;  //subFolder.ListItemAllFields.FieldValues.Count <= 0; SAAS-14037 通过这个来判断是否是需要隐藏的folder
            return folderInfo;
        }

        private static AveFolderBrowserInfo SetFolderBrowserInfo(ListItem listItem, Guid parentListId, Guid parentFolderUniqueId, string webUrl)
        {
            AveFolderBrowserInfo folderInfo = new AveFolderBrowserInfo();
            folderInfo.UniqueId = (Guid)listItem["UniqueId"];
            folderInfo.Name = listItem["FileLeafRef"].ToString();
            folderInfo.ServerRelativeUrl = listItem["FileRef"].ToString();
            folderInfo.Url = new Uri(new Uri(webUrl), folderInfo.ServerRelativeUrl).ToString();//return absolute url instead of relative url.
            folderInfo.ParentListId = parentListId;
            folderInfo.ParentId = parentFolderUniqueId;
            folderInfo.HasUniqueRoleAssignments = listItem.HasUniqueRoleAssignments;
            return folderInfo;
        }

        private static void SetItemBrowserInfo(string webServerRelativeUrl, AveItemBrowserInfo itemInfo, ListItem item, bool canLoadDisplayName)
        {
            if (item.FieldValues.ContainsKey("FileRef"))
            {
                string str = item.FieldValues["FileRef"].ToString();
                itemInfo.Url = str.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            itemInfo.ID = item.Id;
            if ((item.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
            {
                if ((item.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    itemInfo.Name = item.FieldValues["Title"] as string;
                    if (string.IsNullOrEmpty(itemInfo.Name))
                    {
                        itemInfo.Name = "";
                    }
                }
                else
                {
                    itemInfo.Name = item.FieldValues["FileLeafRef"].ToString();
                }
            }
            else
            {
                itemInfo.Name = item.FieldValues["FileLeafRef"].ToString();
            }
            // for DPM
            if (item.FieldValues.ContainsKey("GUID"))
            {
                itemInfo.TpGuid = (Guid)item.FieldValues["GUID"];
            }
            if (item.FieldValues.ContainsKey("Modified"))
            {
                itemInfo.LastModifyTime = (DateTime)item.FieldValues["Modified"];
            }
            if (item.FieldValues.ContainsKey("Editor"))
            {
                FieldUserValue fieldUserValue = item.FieldValues["Editor"] as FieldUserValue;
                itemInfo.LastModifier = fieldUserValue.LookupId;
            }
            if (item.FieldValues.ContainsKey("_Level") && item.FieldValues["_Level"] != null)
            {
                itemInfo.Level = byte.Parse(item.FieldValues["_Level"].ToString());
            }
            if (item.FieldValues.ContainsKey("_UIVersionString") && item.FieldValues["_UIVersionString"] != null)//SAAS-9653
            {
                itemInfo.CurrentUIVersionString = item.FieldValues["_UIVersionString"].ToString();
            }


            itemInfo.DisplayName = canLoadDisplayName ? item.DisplayName : itemInfo.Name;
            itemInfo.UniqueId = new Guid(item["UniqueId"].ToString());
            itemInfo.ListBaseType = (int)item.ParentList.BaseType;
            itemInfo.ParentListID = item.ParentList.Id;
            itemInfo.HasUniqueRoleAssignments = item.HasUniqueRoleAssignments;
        }

        private static AveContentTypeInfo SetCTBrowserInfo(ContentType ct)
        {
            AveContentTypeInfo info = new AveContentTypeInfo
            {
                Group = ct.Group,
                Name = ct.Name,
                Id = ct.Id.ToString(),
                Hidden = ct.Hidden
            };
            return info;
        }

        private static AveAppBrowserInfo SetAppBrowserInfo(AppInstance app)
        {
            AveAppBrowserInfo info = new AveAppBrowserInfo
            {
                Name = app.Title,
                DisplayName = app.Title,
                SPObjectId = app.ProductId,
                Id = app.Id,
                Url = !string.IsNullOrEmpty(app.AppWebFullUrl) ? new Uri(app.AppWebFullUrl) : null,
                Status = (int)app.Status
            };
            return info;
        }

        private static AveSolutionBrowserInfo SetSolutionBrowserInfo(Dictionary<string, object> itemProperties)
        {
            object tempObj = null;
            AveSolutionBrowserInfo solutionBrowserInfo = new AveSolutionBrowserInfo();
            solutionBrowserInfo.Name = itemProperties.TryGetValue("Name", out tempObj) ? tempObj.ToString() : string.Empty;
            solutionBrowserInfo.DisplayName = itemProperties.TryGetValue("DisplayName", out tempObj) ? tempObj.ToString() : string.Empty; ;
            solutionBrowserInfo.SolutionId = itemProperties.TryGetValue("SolutionId", out tempObj) ? tempObj.ToString() : string.Empty;
            solutionBrowserInfo.SolutionHasAssemblies = itemProperties.TryGetValue("SolutionHasAssemblies", out tempObj) ? tempObj.ToString() : string.Empty;
            solutionBrowserInfo.SolutionHash = itemProperties.TryGetValue("SolutionHash", out tempObj) ? tempObj.ToString() : string.Empty;
            if (itemProperties.TryGetValue("Status", out tempObj))
            {
                solutionBrowserInfo.Status = Int32.Parse(tempObj.ToString());
            }
            else
            {
                solutionBrowserInfo.Status = 0;
            }
            return solutionBrowserInfo;
        }

        private static AveFieldBrowserInfo SetFieldBrowserInfo(Field field, string webUrl, string listTitle)
        {
            AveFieldBrowserInfo fieldBrowserInfo = new AveFieldBrowserInfo
            {
                ID = field.Id.ToString(),
                Group = field.Group,
                DisplayName = field.Title,
                Name = field.InternalName,
                Hidden = field.Hidden,
                ParentWebUrl = webUrl,
                ParentListTitle = listTitle
            };
            return fieldBrowserInfo;
        }

        private static AveHiddenFileInfo SetHiddenFileInfo(ClientFile file)
        {
            AveHiddenFileInfo fileInfo = new AveHiddenFileInfo
            {
                Name = file.Name,
                Level = (byte)file.Level,
                Version = file.UIVersion,
                ID = file.UniqueId.ToString()
            };
            return fileInfo;
        }

        private static AveWorkflowAssociationBrowserInfo SetWorkflowAssociationBrowserInfo(Microsoft.SharePoint.Client.Workflow.WorkflowAssociation workflowAssociation)
        {
            AveWorkflowAssociationBrowserInfo workflowAssociationBrowserInfo = new AveWorkflowAssociationBrowserInfo
            {
                Name = workflowAssociation.Name,
                ID = workflowAssociation.Id,
                BaseId = workflowAssociation.BaseId,
                Created = workflowAssociation.Created
            };
            return workflowAssociationBrowserInfo;
        }

        #endregion

        private List<AveItemBrowserInfo> GetBrowserItemsFromLargeList(Web parentWeb, List list, ClientFolder parentFolder, string parentFolderServerRelativeUrl, ClientContext context, ref string pageInfo, uint perPage)
        {
            List<AveItemBrowserInfo> itemBrowserInfos = new List<AveItemBrowserInfo>();
            ListItemCollectionPosition lastPos = null;
            int index = 0;
            int folderSubItemsCount = 0;
            bool flag = true;
            if (!string.IsNullOrEmpty(pageInfo))
            {
                try
                {
                    flag = false;
                    string[] pageInfos = pageInfo.Split('&');
                    Dictionary<string, string> pagInfoDic = pageInfos.ToDictionary(v => v.Split('=')[0], v => v.Split('=')[1]);//SAAS-14378 将pageInfo中的信息转化成Dictionary，然后取出p_ID的值。
                    index = pagInfoDic.ContainsKey("p_ID") ? Convert.ToInt32(pagInfoDic["p_ID"]) + 1 : index;
                }
                catch
                {
                    mLogger.Warn("analyse pageinfo index failed.");
                }
            }
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                        "<View>" +
                        "<Query><Where><And>" +
                        "<Gt><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{0}</Value>" +
                        "</Gt>" +
                        "<Leq><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{1}</Value>" +
                        "</Leq>" +
                        "</And></Where></Query>" +
                        "<RowLimit>{2}</RowLimit>" +
                        "</View>", index, index + 4999, perPage);
                int lastIndex = index;
                camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl);
                camlQuery.ListItemCollectionPosition = lastPos != null ? lastPos : (!string.IsNullOrEmpty(pageInfo) ? new ListItemCollectionPosition { PagingInfo = pageInfo } : null);

                ListItemCollection items = list.GetItems(camlQuery);
                //context.Load(items, its => its.ListItemCollectionPosition,
                //                    its => its.IncludeWithDefaultProperties(tm => tm["FSObjType"], tm => tm.ParentList.BaseType,
                //                                                            tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "0"));
                ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                //部分folder无法使用 context.Load(items, its => its.Include(tm => tm.DisplayName))的方式load item 的display name
                using (exceptionScope.StartScope())
                {
                    using (exceptionScope.StartTry())
                    {
                        context.Load(items, its => its.ListItemCollectionPosition,
                                            its => its.IncludeWithDefaultProperties(tm => tm["FSObjType"], tm => tm.ParentList.BaseType, tm => tm.DisplayName,
                                                                                    tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "0"));
                    }
                    using (exceptionScope.StartCatch())
                    {
                        context.Load(items, its => its.ListItemCollectionPosition,
                                            its => its.IncludeWithDefaultProperties(tm => tm["FSObjType"], tm => tm.ParentList.BaseType,
                                                                                    tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments).Where(item => (string)item["FSObjType"] == "0"));
                    }
                }
                context.Load(parentFolder, f => f.Properties);
                context.ExecuteQuery();
                folderSubItemsCount = Convert.ToInt32(parentFolder.Properties.FieldValues["vti_folderitemcount"]) - Convert.ToInt32(parentFolder.Properties.FieldValues["vti_foldersubfolderitemcount"]);
                if (exceptionScope.HasException)
                {
                    mLogger.Warn("Load item's display name failed,parent folder server relative url:{0}. {1}", parentFolderServerRelativeUrl, exceptionScope.ErrorMessage);
                }
                if (items.ListItemCollectionPosition != null)
                {
                    pageInfo = items.ListItemCollectionPosition.PagingInfo;
                    lastPos = items.ListItemCollectionPosition;
                }
                else
                {
                    pageInfo = null;
                }
                Guid parentFolderId = parentFolder.ListItemAllFields.FieldValues.Count > 0 ? (Guid)parentFolder.ListItemAllFields.FieldValues["UniqueId"] : Guid.Empty;
                foreach (ListItem item in items)
                {
                    AveItemBrowserInfo itemInfo = new AveItemBrowserInfo();
                    SetItemBrowserInfo(parentWeb.ServerRelativeUrl, itemInfo, item, !exceptionScope.HasException);
                    itemInfo.ParentFolderUniqueID = parentFolderId;
                    itemBrowserInfos.Add(itemInfo);
                    if (itemBrowserInfos.Count == perPage)
                    {
                        return itemBrowserInfos;
                    }
                    index = index < item.Id ? item.Id : index;
                }
                index += 4999;//lastIndex + perPage < index ? index : (int)perPage;
            }
            while (folderSubItemsCount > 0 && (lastPos != null || (flag && itemBrowserInfos.Count < folderSubItemsCount)));//[SAAS-10406]确保当folder下的非subfolderItems的数量不为零，且items的起始ID大于5000时，能够检索出结果。
            return itemBrowserInfos;
        }

        private List GetParentList(Web parentWeb, string parentFolderServerRelativeUrl)
        {
            parentFolderServerRelativeUrl = parentFolderServerRelativeUrl.TrimEnd('/');
            if (!parentFolderServerRelativeUrl.StartsWith(parentWeb.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
            {
                parentFolderServerRelativeUrl = parentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + parentFolderServerRelativeUrl.TrimStart('/');
            }
            parentFolderServerRelativeUrl = parentFolderServerRelativeUrl.TrimEnd('/') + '/';
            foreach (List list in parentWeb.Lists)
            {
                if (parentFolderServerRelativeUrl.StartsWith(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))
                {
                    return list;
                }
            }
            return null;
        }

    }
}
