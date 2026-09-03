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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMRelatedRecord.BrowserObjInfo;
using AvePoint.RA.SharePoint.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RelatedRecords.RelatedRecordsBrowser
{
    public class BrowserWorker : IDisposable
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(BrowserWorker));
        private ClientContext context { get; set; }
        private string mSiteUrl { get; set; }
        private string mAccessToken { get; set; }
        public BrowserWorker(string siteUrl, string accessToken)
        {
            if (!IsSharePointUrl(siteUrl))
            {
                throw new Exception("The current url is not a sharepoint url and cannot be accessed.");
            }
            this.mSiteUrl = siteUrl;
            this.mAccessToken = accessToken;
            context = CommonClientContext.GetClientContextWithAccessToken(siteUrl, accessToken);
        }

        public RecordsSiteBrowserInfo BrowserSiteCollection()
        {
            var siteBrowserInfo = new RecordsSiteBrowserInfo();
            var siteCollection = context.Site;
            context.Load(siteCollection);
            var rootWeb = context.Site.RootWeb;
            context.Load(rootWeb, w => w.Title, w => w.Language, w => w.WebTemplate);
            context.ExecuteQuery();
            siteBrowserInfo.name = rootWeb.Title;
            siteBrowserInfo.id = siteCollection.Id;
            siteBrowserInfo.Language = rootWeb.Language;
            siteBrowserInfo.TemplateName = rootWeb.WebTemplate;
            siteBrowserInfo.TemplateTitle = rootWeb.WebTemplate;
            siteBrowserInfo.url = siteCollection.Url;
            return siteBrowserInfo;
        }
        public RecordsWebBrowserInfo BrowserRootWeb()
        {
            RecordsWebBrowserInfo webBrowserInfo = new RecordsWebBrowserInfo();
            Web rootWeb = context.Site.RootWeb;
            context.Load(rootWeb, w => w.ServerRelativeUrl,
                                         w => w.Id,
                                         w => w.Title,
                                         w => w.Language,
                                         w => w.Url,
                                         w => w.HasUniqueRoleAssignments,
                                         w => w.WebTemplate);
            context.ExecuteQuery();
            SetWebBrowserInfo(webBrowserInfo, rootWeb);
            return webBrowserInfo;
        }
        public RecordsWebBrowserInfo BrowserCurrentWeb()
        {
            var webBrowserInfo = new RecordsWebBrowserInfo();
            var site = context.Site;
            context.Load(site);
            Web web = site.RootWeb;
            context.Load(web);
            context.ExecuteQuery();

            if (web.Url.Equals(this.mSiteUrl))
            {
                context.Load(web, w => w.ServerRelativeUrl,
                                             w => w.Id,
                                             w => w.Title,
                                             w => w.Language,
                                             w => w.Url,
                                             w => w.HasUniqueRoleAssignments,
                                             w => w.WebTemplate);
                context.ExecuteQuery();
            }
            else
            {
                web = context.Web;
                context.Load(web, w => w.ServerRelativeUrl,
                                             w => w.Id,
                                             w => w.Title,
                                             w => w.Language,
                                             w => w.Url,
                                             w => w.HasUniqueRoleAssignments,
                                             w => w.WebTemplate);
                context.ExecuteQuery();
            }
            SetWebBrowserInfo(webBrowserInfo, web);
            return webBrowserInfo;
        }
        public List<RecordsWebBrowserInfo> BrowserSites(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            List<RecordsWebBrowserInfo> webInfos = new List<RecordsWebBrowserInfo>();
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
                foreach (Web web in parentWeb.Webs.OrderBy(w => w.Title))
                {
                    RecordsWebBrowserInfo info = new RecordsWebBrowserInfo();
                    SetWebBrowserInfo(info, web);
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
                    RecordsWebBrowserInfo info = new RecordsWebBrowserInfo();
                    SetWebBrowserInfo(info, subWebs[startIndex + i]);
                    webInfos.Add(info);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", startIndex, childrenCount, ex.ToString());
            }
            return webInfos;

        }
        public List<RecordsListBrowserInfo> BrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            string siteUrl = string.Empty;
            List<RecordsListBrowserInfo> listInfoList = new List<RecordsListBrowserInfo>();
            Web web = context.Site.OpenWebById(parentWebId);
            context.Load(web, s => s.Url);
            context.Load(web.Lists, ls => ls.Include(l => l.Id,
                                                     l => l.ParentWebUrl,
                                                     l => l.Title,
                                                     l => l.BaseType,
                                                     l => l.BaseTemplate,
                                                     l => l.Hidden,
                                                     l => l.EnableVersioning,
                                                     l => l.EnableAttachments,
                                                     l => l.RootFolder.ServerRelativeUrl,
                                                     l => l.RootFolder.Name,
                                                     l => l.RootFolder.UniqueId,
                                                     l => l.HasUniqueRoleAssignments,
                                                     l => l.EnableFolderCreation));
            context.ExecuteQuery();
            List<List> listCollection = new List<List>();
            //filter the list obj
            foreach (var list in web.Lists.OrderBy(l => l.Title))
            {
                try
                {
                    if (SPCommonUtility.CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()) || list.Hidden)
                    {
                        mLogger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("Error in Browser list {0}:{1}", list.Title, e.ToString());
                }
                listCollection.Add(list);
            }

            childrenCount = listCollection.Count;
            siteUrl = web.Url;
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
                    List list = listCollection[i + startIndex];
                    listInfoList.Add(SetListBrowserInfo(list, siteUrl, parentWebId));
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("StartIndex Out of Range when getting browser lists.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", startIndex, childrenCount, ex.ToString());
            }
            return listInfoList;
        }
        public RecordsFolderBrowserInfo GetBrowserRootFolder(Guid parentWebId, Guid parentListId)
        {
            RecordsFolderBrowserInfo rootFolderInfo = new RecordsFolderBrowserInfo();
            try
            {
                Web web = context.Site.OpenWebById(parentWebId);
                List list = web.Lists.GetById(parentListId);
                Folder folder = list.RootFolder;
                context.Load(list, l => l.BaseType, l => l.HasUniqueRoleAssignments, l => l.Id);
                context.Load(folder, f => f.ServerRelativeUrl, f => f.Name, f => f.UniqueId);
                context.ExecuteQuery();
                rootFolderInfo = SetRootFolderBrowserInfo(folder, list, parentWebId);
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get browser list root folder has error:{0}", ex.ToString());
            }

            return rootFolderInfo;
        }
        public List<RecordsFolderBrowserInfo> BrowserFolders(Guid parentWebId, Guid parentlistId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, bool needLoadDesignFolders)
        {
            List<RecordsFolderBrowserInfo> folders = new List<RecordsFolderBrowserInfo>();
            try
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

                if (list == null || list != null && folder.ItemCount < 5000)
                {
                    ExceptionHandlingScope scope = new ExceptionHandlingScope(context);
                    using (scope.StartScope())
                    {
                        using (scope.StartTry())
                        {
                            context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.ParentFolder,
                                                                                       f => f.ListItemAllFields.HasUniqueRoleAssignments
                                                                                      ));//.Where(f => f.ListItemAllFields.ServerObjectIsNull == false));//SAAS-13567 uniqueId可以直接获取到，不需要从ListItemAllFields中获取，因为Forms获取不到ListItemAllFields
                        }
                        using (scope.StartCatch())
                        {
                            context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.Properties));
                        }
                    }
                    context.ExecuteQuery();

                    foreach (Folder subFolder in folder.Folders.OrderBy(f => f.Name))
                    {
                        if (subFolder.ListItemAllFields.ServerObjectIsNull == false)
                        {
                            folders.Add(SetFolderBrowserInfo(subFolder, parentWebId, parentlistId, parentFolderUniqueId));
                        }
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
                                    "<Query><OrderBy Override='TRUE'> <FieldRef Name = 'FileRef' Ascending = 'False' /></OrderBy><Where><And><Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query>" +
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
                                folders.Add(SetFolderBrowserInfo(listItems[i], parentWebId, parentlistId, parentFolderUniqueId));
                                index = index < listItems[i].Id ? listItems[i].Id : index;
                            }
                        }
                        index = lastIndex + 2000 < index ? index : lastIndex + 2000;
                        folderCount -= listItems.Count;
                    }
                    while (folderCount > 0);
                }

            }
            catch (Exception e)
            {
                mLogger.Warn(string.Format("get browser folders failed, parent folder url: {0}", parentFolderServerRelativeUrl), e);
            }
            return folders;
        }
        public void GetPageInfo(Guid webId, Guid listId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage, ref int childrenCount)
        {
            Site siteCollection = context.Site;
            context.Load(siteCollection, s => s.Id, s => s.Url);
            Web parentWeb = context.Web;
            context.Load(parentWeb, w => w.ServerRelativeUrl, w => w.Url, w => w.Id);
            List list = null;
            if (listId != Guid.Empty)
            {
                list = parentWeb.Lists.GetById(listId);
                context.Load(list);
                context.Load(list, l => l.RootFolder);
            }
            //context.Load(list, l => l.ItemCount);
            context.ExecuteQuery();

            Folder parentFolder = parentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl));
            context.Load(parentFolder);
            context.ExecuteQuery();
            context.Load(parentFolder, p => p.Properties, p => p.ItemCount);
            context.ExecuteQuery();
            Dictionary<string, object> folderProperties = parentFolder.Properties.FieldValues;
            var folderCount = folderProperties.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folderProperties["vti_foldersubfolderitemcount"]) : 0;
            childrenCount = parentFolder.ItemCount - folderCount;
            //List list = GetParentList(parentWeb, parentFolderServerRelativeUrl);

            if (list == null)
            {

            }
            else
            {
                //context.Load(parentFolder, f => f.ListItemAllFields["UniqueId"]);
                if (parentFolder.ItemCount > 5000)
                {
                    GetItemsPageInfoFromLargeList(parentWeb, list, parentFolder, parentFolderServerRelativeUrl, context, ref pageInfo, perPage);
                }
                else
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = "<View><Query><OrderBy Override='TRUE'> <FieldRef Name = 'FileRef' Ascending = 'True' /></OrderBy><Where><Eq><FieldRef Name=\"FSObjType\"/><Value Type=\"Integer\">0</Value></Eq></Where></Query><RowLimit>" + perPage + "</RowLimit></View>";
                    //                 camlQuery.ViewXml = @"<View><Query><OrderBy Override='TRUE'> 
                    //  <FieldRef Name = 'FSObjType' Ascending = 'False' />
                    //</OrderBy></Query><RowLimit>" + perPage + "</RowLimit></View>";
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

                }
            }
        }
        public List<RecordsItemBrowserInfo> BrowserItems(Guid webId, Guid listId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage, ref int childrenCount)
        {
            List<RecordsItemBrowserInfo> itemBrowserInfos = new List<RecordsItemBrowserInfo>();
            Site siteCollection = context.Site;
            context.Load(siteCollection, s => s.Id, s => s.Url);
            Web parentWeb = context.Web;
            context.Load(parentWeb, w => w.ServerRelativeUrl, w => w.Url, w => w.Id);
            List list = null;
            if (listId != Guid.Empty)
            {
                list = parentWeb.Lists.GetById(listId);
                context.Load(list);
                context.Load(list, l => l.RootFolder);
            }
            //context.Load(list, l => l.ItemCount);
            context.ExecuteQuery();

            Folder parentFolder = parentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl));
            context.Load(parentFolder);
            context.ExecuteQuery();
            context.Load(parentFolder, p => p.Properties, p => p.ItemCount);
            context.ExecuteQuery();
            Dictionary<string, object> folderProperties = parentFolder.Properties.FieldValues;
            var folderCount = folderProperties.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folderProperties["vti_foldersubfolderitemcount"]) : 0;
            childrenCount = parentFolder.ItemCount - folderCount;
            //List list = GetParentList(parentWeb, parentFolderServerRelativeUrl);

            if (list == null)
            {
                context.Load(parentFolder.Files, fs => fs.Include(f => f.Name));
                foreach (Microsoft.SharePoint.Client.File file in parentFolder.Files)
                {
                    RecordsItemBrowserInfo itemInfo = new RecordsItemBrowserInfo();
                    SetFileBrowserInfos(itemInfo, file, parentWeb.ServerRelativeUrl);
                    itemBrowserInfos.Add(itemInfo);
                }
            }
            else
            {
                //context.Load(parentFolder, f => f.ListItemAllFields["UniqueId"]);
                if (parentFolder.ItemCount > 5000)
                {
                    return GetBrowserItemsFromLargeList(parentWeb, list, parentFolder, parentFolderServerRelativeUrl, context, ref pageInfo, perPage);
                }
                else
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = "<View><Query><OrderBy Override='TRUE'> <FieldRef Name = 'FileRef' Ascending = 'True' /></OrderBy><Where><Eq><FieldRef Name=\"FSObjType\"/><Value Type=\"Integer\">0</Value></Eq></Where></Query><RowLimit>" + perPage + "</RowLimit></View>";
                    //                 camlQuery.ViewXml = @"<View><Query><OrderBy Override='TRUE'> 
                    //  <FieldRef Name = 'FSObjType' Ascending = 'False' />
                    //</OrderBy></Query><RowLimit>" + perPage + "</RowLimit></View>";
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
                            context.Load(items, its => its.Include(tm => tm.DisplayName, tm => tm.ParentList.BaseType, tm => tm.ParentList.BaseTemplate, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments));
                        }
                        using (exceptionScope.StartCatch())
                        {
                            context.Load(items);
                            context.Load(items, its => its.Include(tm => tm.ParentList.BaseType, tm => tm.ParentList.BaseTemplate, tm => tm.ParentList.Id, tm => tm.HasUniqueRoleAssignments));
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
                        RecordsItemBrowserInfo itemInfo = new RecordsItemBrowserInfo();
                        SetItemBrowserInfo(parentWeb.Url, parentWeb.ServerRelativeUrl, itemInfo, item, !exceptionScope.HasException);
                        itemBrowserInfos.Add(itemInfo);
                        #region init for end user job
                        itemInfo.WebUrl = parentWeb.Url;
                        itemInfo.ListId = listId;
                        itemInfo.FolderId = parentFolder.UniqueId;
                        itemInfo.WebId = parentWeb.Id;
                        itemInfo.ParentFolderIsRootFolder = list.RootFolder.UniqueId.Equals(parentFolder.UniqueId);
                        itemInfo.SiteId = siteCollection.Id;
                        itemInfo.SiteUrl = siteCollection.Url;
                        itemInfo.WebServerRelativeUrl = parentWeb.ServerRelativeUrl;
                        itemInfo.ListUrl = list.RootFolder.ServerRelativeUrl;
                        itemInfo.FolderUrl = parentFolder.ServerRelativeUrl;
                        itemInfo.ItemUrl = item["FileRef"].ToString();
                        #endregion
                    }
                }
            }
            return itemBrowserInfos;
        }
        private void GetItemsPageInfoFromLargeList(Web parentWeb, List list, Folder parentFolder, string parentFolderServerRelativeUrl, ClientContext context, ref string pageInfo, uint perPage)
        {

            ListItemCollectionPosition lastPos = null;
            int index = 0;
            int folderSubItemsCount = 0;
            bool flag = true;
            int itemCount = 0;
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
                        "<Query><OrderBy Override='TRUE'> <FieldRef Name = 'FileRef' Ascending = 'True' /></OrderBy><Where><And>" +
                        "<Gt><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{0}</Value>" +
                        "</Gt>" +
                        "<Leq><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{1}</Value>" +
                        "</Leq>" +
                        "</And></Where>" +
                       "</Query>" +
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
                    itemCount++;
                    index = index < item.Id ? item.Id : index;
                }
                index += 4999;//lastIndex + perPage < index ? index : (int)perPage;
            }
            while (folderSubItemsCount > 0 && (lastPos != null || (flag && itemCount < folderSubItemsCount)));//[SAAS-10406]确保当folder下的非subfolderItems的数量不为零，且items的起始ID大于5000时，能够检索出结果。

        }
        private List<RecordsItemBrowserInfo> GetBrowserItemsFromLargeList(Web parentWeb, List list, Folder parentFolder, string parentFolderServerRelativeUrl, ClientContext context, ref string pageInfo, uint perPage)
        {
            List<RecordsItemBrowserInfo> itemBrowserInfos = new List<RecordsItemBrowserInfo>();
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
                catch(Exception e)
                {
                    mLogger.Warn($"analyse pageinfo index failed.error:{e}");
                }
            }
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                        "<View>" +
                        "<Query><OrderBy Override='TRUE'> <FieldRef Name = 'FileRef' Ascending = 'True' /></OrderBy><Where><And>" +
                        "<Gt><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{0}</Value>" +
                        "</Gt>" +
                        "<Leq><FieldRef Name=\"ID\"/>" +
                        "<Value Type=\"Integer\">{1}</Value>" +
                        "</Leq>" +
                        "</And></Where>" +
                       "</Query>" +
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
                    RecordsItemBrowserInfo itemInfo = new RecordsItemBrowserInfo();
                    SetItemBrowserInfo(parentWeb.Url, parentWeb.ServerRelativeUrl, itemInfo, item, !exceptionScope.HasException);
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

        public RecordsItemBrowserInfo BrowserItemInfo(Guid listId, int itemId)
        {
            RecordsItemBrowserInfo info = new RecordsItemBrowserInfo();
            Site siteCollection = context.Site;
            context.Load(siteCollection);
            var parentWeb = context.Web;
            context.Load(parentWeb);
            context.ExecuteQuery();
            List list = null;
            if (listId != Guid.Empty)
            {
                list = parentWeb.Lists.GetById(listId);
                context.Load(list);
            }
            context.ExecuteQuery();
            ArgumentCheck.NotNull(list, nameof(list));
            var item = list.GetItemById(itemId);
            context.Load(item);
            context.ExecuteQuery();
            SetItemId(info, item);
            info.SiteId = siteCollection.Id;
            return info;

        }
        public string GetLoginName()
        {
            var web = context.Web;
            context.Load(web);
            context.ExecuteQuery();
            var user = web.CurrentUser;
            context.Load(user, u => u.LoginName,u=>u.Email,u=>u.UserId);
            
            context.ExecuteQuery();
            return user.LoginName.Split('|').Last();
        }
        public string GetTenantId()
        {
            Func<string> getObj = () =>
            {
                string result = null;
                var parentWeb = context.Web;
                context.Load(parentWeb);
                context.ExecuteQuery();
                var p = parentWeb.AllProperties;
                context.Load(p);
                context.ExecuteQuery();
                if (p.FieldValues.ContainsKey(RcordsBuiltInColumn.RELATED_TENANTID))
                {
                    result = p[RcordsBuiltInColumn.RELATED_TENANTID]?.ToString();
                    mLogger.Info($"get related id from property success: {result}");
                }
                return result;
            };
            return getObj();

        }
        private void SetWebBrowserInfo(RecordsWebBrowserInfo info, Web web)
        {
            //string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
            //string Url = string.Empty;
            //if (web.ServerRelativeUrl.Equals("/"))
            //{
            //    Url = this.WebAppName;
            //}
            //else
            //{
            //    if (!siteServerRelativeUrl.Equals("/"))
            //    {
            //        Url = context.Url.Replace(siteServerRelativeUrl, web.ServerRelativeUrl);
            //    }
            //    else//host header类型的sitecollection走一下逻辑；
            //    {
            //        Url = string.Format("{0}/{1}", context.Url.TrimEnd('/'), web.ServerRelativeUrl.TrimStart('/'));
            //    }
            //}
            context.Load(context.Site, s => s.ServerRelativeUrl);
            context.ExecuteQuery();
            info.id = web.Id;
            // info.Title = web.Title;
            info.url = web.Url;
            info.WebUrl = web.Url;
            info.IsRootWeb = false;
            string name = string.Empty;
            if (web.ServerRelativeUrl.Equals(context.Site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                info.IsRootWeb = true;
            }
            else
            {
                int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                name = web.ServerRelativeUrl.Substring(lastSlashIndex + 1);
            }
            info.name = name;
            info.TemplateName = web.WebTemplate;
            info.Language = web.Language;
            info.ServerRelativeUrl = web.ServerRelativeUrl;
            info.NodeLevel = (int)NodeLevel.Site;
        }
        private RecordsListBrowserInfo SetListBrowserInfo(List list, string siteUrl, Guid parentWebId)
        {
            RecordsListBrowserInfo listInfo = new RecordsListBrowserInfo();
            listInfo.BaseTemplate = list.BaseTemplate;
            listInfo.BaseType = (int)list.BaseType;
            listInfo.Hidden = list.Hidden;
            listInfo.id = list.Id;
            listInfo.name = list.Title;
            listInfo.rootFolderName = list.RootFolder.Name;
            listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
            listInfo.Title = list.Title;
            listInfo.url = new Uri(new Uri(siteUrl), list.RootFolder.ServerRelativeUrl).ToString();
            listInfo.parentWebId = parentWebId;
            listInfo.FolderId = list.RootFolder.UniqueId;
            listInfo.WebUrl = siteUrl;
            listInfo.NodeLevel = (int)NodeLevel.List;
            listInfo.ListId = list.Id;
            listInfo.WebId = parentWebId;
            return listInfo;
        }
        private RecordsFolderBrowserInfo SetRootFolderBrowserInfo(Folder folder, List list, Guid parentWebId)
        {
            RecordsFolderBrowserInfo folderInfo = new RecordsFolderBrowserInfo();
            folderInfo.parentWebId = parentWebId;
            folderInfo.name = folder.Name;
            folderInfo.id = folder.UniqueId;
            folderInfo.url = new Uri(new Uri(this.mSiteUrl), folder.ServerRelativeUrl).ToString();
            folderInfo.ServerRelativeUrl = folder.ServerRelativeUrl;
            if (list != null)
            {
                folderInfo.ParentListId = list.Id;
                folderInfo.ParentListBaseType = (int)list.BaseType;
            }
            return folderInfo;
        }
        private RecordsFolderBrowserInfo SetFolderBrowserInfo(Folder folder, Guid parentWebId, Guid parentListId, Guid parentFolderUniqueId)
        {
            RecordsFolderBrowserInfo folderInfo = new RecordsFolderBrowserInfo();
            folderInfo.id = folder.UniqueId;  //SAAS-13567 Fomrs的UniqueId在ListItemAllFields中是不存在的，但可以直接获取，普通的folder也可以直接获取。
            if (folder.Properties.FieldValues.ContainsKey("vti_etag") &&
                     folder.Properties["vti_etag"] != null)
            {
                string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                folderInfo.id = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
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
            folderInfo.name = folder.Name;
            folderInfo.ServerRelativeUrl = folder.ServerRelativeUrl;
            folderInfo.url = new Uri(new Uri(this.mSiteUrl), folder.ServerRelativeUrl).ToString();//return absolute url instead of relative url.
            //folderInfo.ParentListId = parentlistId;
            folderInfo.ParentId = parentFolderUniqueId;
            folderInfo.Hidden = folder.ListItemAllFields.ServerObjectIsNull.HasValue ? folder.ListItemAllFields.ServerObjectIsNull.Value : true;  //subFolder.ListItemAllFields.FieldValues.Count <= 0; SAAS-14037 通过这个来判断是否是需要隐藏的folder
            folderInfo.WebUrl = this.mSiteUrl;
            folderInfo.ListId = parentListId;
            folderInfo.WebId = parentWebId;
            folderInfo.NodeLevel = (int)NodeLevel.Folder;
            folderInfo.FolderId = folder.UniqueId;
            return folderInfo;
        }
        private RecordsFolderBrowserInfo SetFolderBrowserInfo(ListItem listItem, Guid parentWebId, Guid parentListId, Guid parentFolderUniqueId)
        {
            RecordsFolderBrowserInfo folderInfo = new RecordsFolderBrowserInfo();
            folderInfo.id = (Guid)listItem["UniqueId"];
            folderInfo.name = listItem["FileLeafRef"].ToString();
            folderInfo.ServerRelativeUrl = listItem["FileRef"].ToString();
            folderInfo.url = new Uri(new Uri(this.mSiteUrl), folderInfo.ServerRelativeUrl).ToString();//return absolute url instead of relative url.
            folderInfo.ParentListId = parentListId;
            folderInfo.ParentId = parentFolderUniqueId;
            //folderInfo.HasUniqueRoleAssignments = listItem.HasUniqueRoleAssignments;
            folderInfo.WebUrl = this.mSiteUrl;
            folderInfo.ListId = parentListId;
            folderInfo.WebId = parentWebId;
            folderInfo.NodeLevel = (int)NodeLevel.Folder;
            folderInfo.FolderId = folderInfo.id;
            return folderInfo;
        }
        private void SetItemBrowserInfo(string webUrl, string webServerRelativeUrl, RecordsItemBrowserInfo itemInfo, ListItem item, bool canLoadDisplayName)
        {
            if (item.FieldValues.ContainsKey("FileRef"))
            {
                string str = item.FieldValues["FileRef"].ToString();
                string relatedUrl = str.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                itemInfo.url = webUrl + "/" + relatedUrl;
                itemInfo.WebUrl = webUrl;
            }
            //itemInfo.id = item.;
            if ((item.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
            {
                if ((item.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    itemInfo.name = item.FieldValues["Title"] as string;
                    if (string.IsNullOrEmpty(itemInfo.name))
                    {
                        itemInfo.name = GetSpecialListItemName(item);
                    }
                }
                else
                {
                    itemInfo.name = item.FieldValues["FileLeafRef"].ToString();
                }
            }
            else
            {
                itemInfo.name = item.FieldValues["FileLeafRef"].ToString();
            }
            if (item.FieldValues.ContainsKey("Modified"))
            {
                itemInfo.LastModifyTime = (DateTime)item.FieldValues["Modified"];
            }
            if (item.FieldValues.ContainsKey("Editor"))
            {
                FieldUserValue fieldUserValue = item.FieldValues["Editor"] as FieldUserValue;
                itemInfo.LastModifier = fieldUserValue.LookupId;
                itemInfo.LastModifierName = fieldUserValue.LookupValue;
            }
            if (item.FieldValues.ContainsKey("_Level") && item.FieldValues["_Level"] != null)
            {
                itemInfo.Level = byte.Parse(item.FieldValues["_Level"].ToString());
            }
            if (item.FieldValues.ContainsKey("_UIVersionString") && item.FieldValues["_UIVersionString"] != null)//SAAS-9653
            {
                itemInfo.CurrentUIVersionString = item.FieldValues["_UIVersionString"].ToString();
            }
            itemInfo.Extension = item.FieldValues["File_x0020_Type"] != null ? item.FieldValues["File_x0020_Type"].ToString() : string.Empty;
            itemInfo.DisplayName = itemInfo.name;//canLoadDisplayName ? item.DisplayName : itemInfo.name;
            itemInfo.id = new Guid(item["UniqueId"].ToString());
            itemInfo.ListBaseType = (int)item.ParentList.BaseType;
            itemInfo.DocLibRowId = item.Id;
        }

        private void SetItemId(RecordsItemBrowserInfo itemInfo, ListItem item)
        {
            itemInfo.id = new Guid(item["UniqueId"].ToString());
            itemInfo.DocLibRowId = item.Id;
        }

        private void SetFileBrowserInfos(RecordsItemBrowserInfo itemInfo, Microsoft.SharePoint.Client.File file, string webServerRelativeUrl)
        {
            itemInfo.url = file.ServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            itemInfo.name = file.Name;
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
        }
        private static string[] GetIdsFromEtag(string etagStr)
        {
            string UniqueId = string.Empty;
            string DocLibRowId = string.Empty;
            int startIndex1 = etagStr.IndexOf("{", StringComparison.OrdinalIgnoreCase);
            int startIndex2 = etagStr.IndexOf(",", StringComparison.OrdinalIgnoreCase) + 1;
            if (startIndex1 >= 0)
            {
                int endIndex1 = etagStr.IndexOf('}', startIndex1);
                if (endIndex1 > startIndex1)
                {
                    UniqueId = etagStr.Substring(startIndex1, endIndex1 - startIndex1 + 1);
                }
            }
            if (startIndex2 >= 0)
            {
                int endIndex2 = etagStr.IndexOf('"', startIndex2);
                if (endIndex2 > startIndex2)
                {
                    DocLibRowId = etagStr.Substring(startIndex2, endIndex2 - startIndex2);
                }
            }
            return new string[] { UniqueId, DocLibRowId };
        }

        private string GetSpecialListItemName(ListItem item)
        {
            var itemName = "";
            if ((int)ListTemplateType.Links == item.ParentList.BaseTemplate)
            {
                FieldUrlValue filedUrlValue = item.FieldValues["URL"] as FieldUrlValue;
                itemName = filedUrlValue.Url;
            }
            return itemName;
        }

        private bool IsSharePointUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                if (uriResult.Host.EndsWith("sharepoint.com", StringComparison.OrdinalIgnoreCase))
                {
                    string path = uriResult.AbsolutePath.ToLower();
                    if (path.Contains("/sites/") || path.Contains("/teams/"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Dispose()
        {
            try
            {
                using (context)
                { }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Dispose context error {0}", ex.ToString());
            }

        }
    }


}
