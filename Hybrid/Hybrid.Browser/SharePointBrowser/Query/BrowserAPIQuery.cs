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
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.Query
{
    public class BrowserAPIQuery : BaseBrowserQuery
    {
        private AveObjectModelFactory mObjectModel = null;

        protected static AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ConverTo方法，用于组装BrowserInfo
        private AveWebBrowserInfo ConverToBrowserInfo(IAveWeb web, IAveWebTemplateCollection templates, bool isRootWeb)
        {
            var template = TryGetWebTemplate(templates, web.Template);
            AveWebBrowserInfo WebInfo = new AveWebBrowserInfo()
            {
                ID = web.ID,
                Name = web.Name,
                Url = web.Url,
                Title = web.Title,
                Language = web.Language,
                IsRootWeb = isRootWeb,
                TemplateName = template == null ? web.Template : template.Name,
                TemplateTitle = template == null ? string.Empty : template.Title,
                HasUniqueRoleAssignments = web.HasUniqueRoleAssignments,
                IsAppWeb = web.IsAppWeb
            };
            return WebInfo;
        }

        private AveListBrowserInfo ConverToBrowserInfo(IAveList list)
        {
            AveListBrowserInfo listInfo = new AveListBrowserInfo()
            {
                ID = list.ID,
                Title = list.TitleResource == null ? list.Title : list.TitleResource.GetValueForUICulture(list.ParentWeb.UICulture),
                Name = list.TitleResource == null ? list.Title : list.TitleResource.GetValueForUICulture(list.ParentWeb.UICulture),
                HasUniqueRoleAssignments = list.HasUniqueRoleAssignments,
                ServerRelativeUrl = list.RootFolder.ServerRelativeUrl,
                Url = new Uri(new Uri(list.ParentWeb.Url), list.RootFolder.ServerRelativeUrl).ToString(),
                BaseTemplate = (int)list.BaseTemplate,
                BaseType = (int)list.BaseType,
                Hidden = list.Hidden,
                EnableFolderCreation = list.EnableFolderCreation,
                rootFolderName = list.RootFolder.Name
            };
            return listInfo;
        }

        private AveFolderBrowserInfo ConverToBrowserInfo(IAveFolder folder, string siteUrl, bool isRootFolder)
        {
            AveFolderBrowserInfo folderInfo = new AveFolderBrowserInfo()
            {
                UniqueId = folder.UniqueId,
                Name = folder.Name,
                ServerRelativeUrl = folder.ServerRelativeUrl,
                Url = new Uri(new Uri(siteUrl), folder.ServerRelativeUrl).ToString(),
                ParentListId = folder.ParentListId,
                ParentId = isRootFolder ? folder.ParentListId : folder.ParentFolder.UniqueId,
                Hidden = folder.Item == null,
            };
            if (isRootFolder)
            {
                folderInfo.HasUniqueRoleAssignments = folder.ParentList == null ? true : folder.ParentList.HasUniqueRoleAssignments;
            }
            else if (folder.Item != null)
            {
                folderInfo.HasUniqueRoleAssignments = folder.Item.HasUniqueRoleAssignments;
            }
            return folderInfo;
        }

        private AveItemBrowserInfo ConverToBrowserInfo(IAveListItem item, Guid ParentFolderUniqueID, string parentFolderUrl)
        {
            Dictionary<string, byte> versions = new Dictionary<string, byte>();
            foreach (IAveListItemVersion version in item.Versions)
            {
                if (!versions.ContainsKey(version.VersionLabel))
                {
                    versions.Add(version.VersionLabel, (byte)version.Level);
                }
            }
            //versions 中包含了当前版本，需要去掉
            versions.Remove(item.Versions[0].VersionLabel);

            AveItemBrowserInfo browserInfo = new AveItemBrowserInfo()
            {
                UniqueId = item.UniqueId,
                Versions = versions,
                Name = item.Name,
                DisplayName = item.DisplayName,
                ID = item.ID,
                ListBaseType = (int)item.ParentList.BaseType,
                Url = parentFolderUrl,
                ParentFolderUniqueID = ParentFolderUniqueID,
                HasUniqueRoleAssignments = item.HasUniqueRoleAssignments,
                ParentListID = item.ParentList.ID,
                //由于通过display name获取field信息，而display name是语言相关的，所以需要先通过internal name获取到display name
                CurrentUIVersionString = item.Fields.ContainsFieldWithStaticName("_UIVersionString") ? item[item.Fields.GetFieldByInternalName("_UIVersionString").Title].ToString() : string.Empty,
                LastModifier = item.Fields.ContainsFieldWithStaticName("Editor") ? Convert.ToInt32(item[item.Fields.GetFieldByInternalName("Editor").Title].ToString().Split(';')[0]) : 0,
                LastModifyTime = item.Fields.ContainsFieldWithStaticName("Modified") ? Convert.ToDateTime(item[item.Fields.GetFieldByInternalName("Modified").Title]).ToUniversalTime() : DateTime.MinValue,
                Level = item.Fields.ContainsFieldWithStaticName("_Level") ? Convert.ToByte(item[item.Fields.GetFieldByInternalName("_Level").Title]) : byte.MinValue
            };

            return browserInfo;
        }

        private AveItemBrowserInfo ConverToBrowserInfo(IAveFile file, Guid parentFolderUniqueID, string parentWebUrl)
        {
            AveItemBrowserInfo browserInfo = new AveItemBrowserInfo()
            {
                UniqueId = file.UniqueId,
                Name = file.Name,
                Url = new Uri(new Uri(parentWebUrl), file.ServerRelativeUrl).ToString(),
                ParentFolderUniqueID = parentFolderUniqueID,
                ParentListID = Guid.Empty,
                HasUniqueRoleAssignments = file.Item != null ? file.Item.HasUniqueRoleAssignments : false
            };
            return browserInfo;
        }

        private AveItemVersionBrowserInfo ConverToBrowserInfo(string versionLabel)
        {
            AveItemVersionBrowserInfo itemVersionInfo = new AveItemVersionBrowserInfo
            {
                VersionLabel = versionLabel,
            };
            return itemVersionInfo;
        }

        private AveItemVersionBrowserInfo ConverToBrowserInfo(IAveFileVersion fileVersion)
        {
            AveItemVersionBrowserInfo fileVersionInfo = new AveItemVersionBrowserInfo
            {
                VersionLabel = fileVersion.VersionLabel,
            };
            return fileVersionInfo;
        }
        #endregion

        public BrowserAPIQuery(AveObjectModelFactory objectModel)
        {
            mObjectModel = objectModel;
        }

        private int GetPagedCount(string pageInfo)
        {
            int pagedCount = 0;
            if (string.IsNullOrEmpty(pageInfo))
            {
                return pagedCount;
            }
            else
            {
                pagedCount = Convert.ToInt32(pageInfo.Substring(pageInfo.LastIndexOf("=", StringComparison.OrdinalIgnoreCase) + 1));
            }
            return pagedCount;
        }

        /// <summary>
        /// 通过ConverTo方法组装AveWebBrowserInfo对象，添加到list中
        /// </summary>
        /// <param name="site"></param>
        /// <param name="parentweb"></param>
        /// <param name="webs"></param>
        private void GetBrowserWebs(IAveSite site, IAveWeb parentweb, List<AveWebBrowserInfo> webs, bool filterAppWeb)
        {
            foreach (var web in parentweb.Webs)
            {
                try
                {
                    if (web.IsAppWeb && filterAppWeb)
                    {
                        continue;
                    }
                    var templates = site.GetWebTemplates(web.Language);
                    webs.Add(ConverToBrowserInfo(web, templates, false));
                }
                catch (Exception ex)
                {
                    Logger.Error("An error occurred while browsing sub web,parent web url is {0}, sub web url is {1}, error message:{2} ", parentweb.Url, web.Url, ex.ToString());
                }
                finally
                {
                    web.Dispose();
                }
            }
        }

        /// <summary>
        /// 组装 AveSiteBrowserInfo对象
        /// </summary>
        /// <param name="webApp"></param>
        /// <param name="sites"></param>
        /// <param name="hasError"></param>
        private void GetBrowserSites(IAveWebApplication webApp, List<AveSiteBrowserInfo> sites, ref bool hasError)
        {
            foreach (var site in webApp.Sites)
            {
                if (site != null && !site.IsSiteMaster)//由于manage path被删除，导致load出的应用了该path的site 是null，需要过滤 && FasterCreation Site need be filtered out
                {
                    try
                    {
                        var rootWeb = site.RootWeb;
                        AveSiteBrowserInfo siteBrowserInfo = new AveSiteBrowserInfo()
                        {
                            Url = site.Url,
                            ID = site.ID,
                            DisplayName = site.HostHeaderIsSiteName ? site.Url.Substring(site.Url.IndexOf(site.HostName, StringComparison.OrdinalIgnoreCase)) : rootWeb.ServerRelativeUrl,//ADO-92765 对于ADO-92765的case，需要通过截取url的方式才能正常显示。
                            Language = rootWeb.Language,
                            Title = rootWeb.Title,
                            AuditActions = mObjectModel.ContextKind == AveContextKind.ClientObjectModel ? 0 : (int)rootWeb.Audit.AuditFlags,
                            BitFlags = (uint)site.Flags,
                            ContentDBID = site.ContentDatabase.ID.ToString(),
                            ContentDBName = site.ContentDatabase.Name,
                            PlatformVersion = site.CompatibilityLevel.ToString("0.0"),
                        };
                        var templates = site.GetWebTemplates(rootWeb.Language);
                        var rootWebTemplate = TryGetWebTemplate(templates, rootWeb.Template);
                        //siteBrowserInfo.TemplateName = GetWebTemplateIdName(rootWeb.WebTemplateId, templates, ref siteBrowserInfo.TemplateTitle);
                        siteBrowserInfo.TemplateName = rootWebTemplate == null ? rootWeb.Template : rootWebTemplate.Name;
                        siteBrowserInfo.TemplateTitle = rootWebTemplate == null ? string.Empty : rootWebTemplate.Title;
                        sites.Add(siteBrowserInfo);
                        site.Dispose();
                    }
                    catch (Exception e)
                    {
                        hasError = true;
                        Logger.Warn("An error occurred while browsing site collection,site collection url is {0}, error message:{1},", site.Url, e.ToString());
                    }
                }
            }
        }

        private IAveWebTemplate TryGetWebTemplate(IAveWebTemplateCollection collection, string name)
        {
            try
            {
                return collection[name];
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private IAveFolder GetFolder(IAveWeb web, Guid folderId, string parentFolderServerRelativeUrl)
        {
            IAveFolder fodler;
            if (mObjectModel != null && mObjectModel.ContextKind == AveContextKind.ClientObjectModel)
            {
                fodler = web.GetFolder(parentFolderServerRelativeUrl);
            }
            else
            {
                fodler = web.GetFolder(folderId);
            }
            return fodler;
        }
        /// <summary>
        /// 区分client和server，cilent通过url获取site collection，server通过id获取site collection
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        private IAveSite GetSite(Guid siteId, string siteUrl)
        {
            IAveSite site;
            if (mObjectModel.ContextKind == AveContextKind.ClientObjectModel)
            {
                site = mObjectModel.CreateSite(siteUrl);
            }
            else
            {
                site = mObjectModel.CreateSite(siteId);
            }
            return site;
        }

        /// API获取WebApp下的Site Collection
        /// </summary>
        /// <param name="webAppUrl"></param>
        /// <param name="username"></param>
        /// <param name="startIndex"></param>
        /// <param name="perPage"></param>
        /// <param name="childrenCount"></param>
        /// <param name="hasError"></param>
        /// <param name="needFilterInfo"></param>
        /// <returns></returns>
        public override List<AveSiteBrowserInfo> GetBrowserSites(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, bool needFilterInfo = false)
        {
            List<AveSiteBrowserInfo> sites = new List<AveSiteBrowserInfo>();

            GetBrowserSites(webApp, sites, ref hasError);
            //由于manage path被删除的话，通过api取到的site是root site,需要过滤。+
            sites = sites.Distinct(new SiteInfoComparer()).ToList();
            childrenCount = sites.Count;
            var pageCount = perPage > childrenCount ? childrenCount : (int)perPage;
            sites.Sort(new AveSiteBrowserInfoComparer());
            return sites.Skip<AveSiteBrowserInfo>(startIndex).Take<AveSiteBrowserInfo>(pageCount).ToList<AveSiteBrowserInfo>();
        }

        /// <summary>
        /// 尝试获取带有checkout vrsion的item version
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileId"></param>
        /// <param name="itemVersionInfos"></param>
        /// <param name="checkOutUser"></param>
        private void GetItemCheckOutVersions(Guid siteId, string webServerRelativeUrl, Guid fileId, List<AveItemVersionBrowserInfo> itemVersionInfos, IAveUser checkOutUser)
        {
            try
            {

                using (IAveSite site = mObjectModel.CreateSite(siteId, checkOutUser.UserToken))
                {
                    using (IAveWeb web = site.OpenWeb(webServerRelativeUrl))
                    {
                        IAveFile file = web.GetFile(fileId, string.Empty);
                        itemVersionInfos.Add(ConverToBrowserInfo(file.UIVersionLabel));//获取check out version
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn("An error occurred while getting item check out version. Error message:{0}", e.ToString());
            }
        }

        public override string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId)
        {
            using (var site = mObjectModel.CreateSite(siteUrl))
            {
                siteId = site.ID;
            }
            return string.Empty;
        }
        public override void Dispose()
        {
        }

        public override List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            List<AveWebBrowserInfo> webs = new List<AveWebBrowserInfo>();
            using (var site = GetSite(option.ParentSiteId, option.SiteUrl))
            {
                using (var parentWeb = site.OpenWeb(option.ParentWebId))
                {
                    GetBrowserWebs(site, parentWeb, webs, option.FilterAppWeb);
                }
            }
            option.ChildrenTotalCount = webs.Count;
            int count = option.PerPage > option.ChildrenTotalCount ? option.ChildrenTotalCount : (int)option.PerPage;
            int index = option.StartIndex > option.ChildrenTotalCount ? 0 : option.StartIndex;
            webs.Sort(new AveWebBrowserInfoComparer());
            return webs.Skip<AveWebBrowserInfo>(index).Take<AveWebBrowserInfo>(count).ToList<AveWebBrowserInfo>();
        }

        public override List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            List<AveFolderBrowserInfo> subFolders = new List<AveFolderBrowserInfo>();
            using (var site = GetSite(option.ParentSiteId, option.SiteUrl))
            {
                using (var web = site.OpenWeb(option.ParentWebId))
                {
                    var parentFolder = GetFolder(web, option.ParentFolderId, option.ParentFolderServerRelativeUrl);
                    bool isWebFolder = parentFolder.ParentListId == Guid.Empty;
                    foreach (var subFolder in parentFolder.SubFolders)
                    {
                        try
                        {
                            if (option.NeedFilter && option.FilterSystemFolder)
                            {
                                if (isWebFolder && subFolder.ParentListId != Guid.Empty)
                                { continue; }
                                if (!isWebFolder && subFolder.Item == null)
                                { continue; }
                            }
                            subFolders.Add(ConverToBrowserInfo(subFolder, site.Url, false));
                        }
                        catch (Exception e)
                        {
                            Logger.Error("An error occurred while browsing folder, parent folder name is {0}, sub folder name is  {1}, error message:{2} ", parentFolder.Name, subFolder.Name, e.ToString());
                        }
                    }
                }
            }
            option.ChildrenTotalCount = subFolders.Count;
            var pageCount = option.PerPage > option.ChildrenTotalCount ? option.ChildrenTotalCount : (int)option.PerPage;
            subFolders.Sort(new AveFolderBrowserInfoComparer());
            return subFolders.Skip<AveFolderBrowserInfo>(option.StartIndex).Take<AveFolderBrowserInfo>(pageCount).ToList<AveFolderBrowserInfo>();
        }

        public override List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option)
        {
            List<AveItemBrowserInfo> listItemInfos = new List<AveItemBrowserInfo>();
            bool isDocumentLibrary;
            using (var site = GetSite(option.ParentSiteId, option.SiteUrl))
            {
                using (var web = site.OpenWeb(option.ParentWebId))
                {
                    var parentFolder = GetFolder(web, option.ParentFolderId, option.ParentFolderServerRelativeUrl);
                    //先判断是不是system folder，再判断list 的base type，system folder 认为是一个library
                    isDocumentLibrary = parentFolder.ParentList == null ? true : (int)parentFolder.ParentList.BaseType == 1 ? true : false;
                    string parentFolderUrl = new Uri(new Uri(site.Url), parentFolder.ServerRelativeUrl).ToString();
                    if (parentFolder.ParentList == null)//system folder list
                    {
                        foreach (var file in parentFolder.Files)
                        {
                            try
                            {
                                listItemInfos.Add(ConverToBrowserInfo(file, parentFolder.UniqueId, parentFolderUrl));
                            }
                            catch (Exception e)
                            {
                                Logger.Error("An error occurred while browsing file from system folder. parent folder url is {0}, file url is {1}, error message: {2}", parentFolder.Url, file.ServerRelativeUrl, e.ToString());
                            }
                        }
                    }
                    else
                    {
                        string pageInfo = string.Empty;
                        AveCamlQuery query = new AveCamlQuery();
                        query.FolderServerRelativeUrl = parentFolder.ServerRelativeUrl;
                        if (!string.IsNullOrEmpty(option.PageInfo))
                        {
                            query.ListItemCollectionPosition = new AveItemCollectionPosition() { PagingInfo = option.PageInfo.Substring(0, option.PageInfo.LastIndexOf("&", StringComparison.OrdinalIgnoreCase)) };
                        }
                        query.ViewXml = isDocumentLibrary ? string.Format("<View><Query><OrderBy><FieldRef Name='FileLeafRef'/></OrderBy><Where><Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>0</Value></Eq></Where></Query><RowLimit>{0}</RowLimit></View>", option.PerPage.ToString())
                                                          : string.Format("<View><Query><OrderBy><FieldRef Name='ID'/></OrderBy><Where><Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>0</Value></Eq></Where></Query><RowLimit>{0}</RowLimit></View>", option.PerPage.ToString());
                        var items = parentFolder.ParentList.GetItems(query);
                        if (items.ListItemCollectionPosition != null)
                        {
                            int pageCount = GetPagedCount(option.PageInfo);
                            pageInfo = items.ListItemCollectionPosition.PagingInfo;
                            pageInfo = pageInfo + string.Format("&StartIndex={0}", pageCount + items.Count);
                        }
                        else
                        {
                            pageInfo = string.Empty;
                        }
                        option.PageInfo = pageInfo;

                        foreach (var item in items)
                        {
                            try
                            {
                                if (item.Folder == null)
                                {
                                    listItemInfos.Add(ConverToBrowserInfo(item, parentFolder.UniqueId, parentFolderUrl));
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error("An error occurred while browsing item. parent folder url is {0},item url is {1}, error message: {2}", parentFolder.Url, item.Url, e.ToString());
                            }
                        }
                    }
                }
            }
            return listItemInfos.Take<AveItemBrowserInfo>((int)option.PerPage).ToList<AveItemBrowserInfo>();
        }

        public override List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            List<AveItemVersionBrowserInfo> itemVersionInfos = new List<AveItemVersionBrowserInfo>();
            using (var site = GetSite(option.ParentSiteId, option.SiteUrl))
            {
                using (IAveWeb web = site.OpenWeb(option.ParentWebServerRelativeUrl))
                {
                    IAveFolder folder = GetFolder(web, option.ParentFolderId, option.ParentFolderServerRelativeUrl);
                    try
                    {
                        if (folder.ParentList == null)//system folder
                        {
                            folder.Files.Select(file => file.Versions.Select(fileVersion => ConverToBrowserInfo(fileVersion))).ToList();
                        }
                        else
                        {
                            IAveListItem item = folder.ParentList.GetItemByUniqueId(option.ParentItemUniqueId);
                            if (item != null)
                            {
                                IAveUser checkOutUser = item.File != null ? item.File.CheckedOutByUser : null;
                                foreach (IAveListItemVersion itemVersion in item.Versions)
                                {
                                    itemVersionInfos.Add(ConverToBrowserInfo(itemVersion.VersionLabel));
                                }
                                if (mObjectModel.ContextKind != AveContextKind.ClientObjectModel && checkOutUser != null && checkOutUser.ID != web.CurrentUser.ID)//office365不支持
                                {
                                    GetItemCheckOutVersions(option.ParentSiteId, option.ParentWebServerRelativeUrl, item.File.UniqueId, itemVersionInfos, checkOutUser);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Warn("An error occurred while browsing item version, parent folder url is {0}, error message is {1}", folder.ServerRelativeUrl, e.ToString());
                    }
                }
            }
            option.ChildrenTotalCount = itemVersionInfos.Count;
            var pageCount = option.PerPage > option.ChildrenTotalCount ? option.ChildrenTotalCount : (int)option.PerPage;
            int index = option.StartIndex > option.ChildrenTotalCount ? 0 : option.StartIndex;
            itemVersionInfos.Sort(new AveItemVersionBrowserInfoComparer());
            return itemVersionInfos.Skip<AveItemVersionBrowserInfo>(index).Take<AveItemVersionBrowserInfo>(pageCount).ToList<AveItemVersionBrowserInfo>();
        }

        public override AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            using (var site = GetSite(option.ParentSiteId, option.SiteUrl))
            {
                var rootWeb = site.RootWeb;
                var templates = site.GetWebTemplates(rootWeb.Language);
                return ConverToBrowserInfo(rootWeb, templates, true);
            }
        }

        public override AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {
            AveFolderBrowserInfo rootFolderInfo;
            IAveFolder rootFolder;
            using (var site = GetSite(option.ParentSiteId, option.SiteUrl))
            {
                using (var web = site.OpenWeb(option.ParentWebId))
                {
                    if (option.ParentListId == Guid.Empty)//web folder
                    {
                        rootFolder = web.GetFolder("");
                    }
                    else
                    {
                        rootFolder = web.GetList(option.ParentListId).RootFolder;
                    }
                    //组装browserInfo
                    rootFolderInfo = ConverToBrowserInfo(rootFolder, option.SiteUrl, true);
                }
            }
            return rootFolderInfo;
        }

        public override List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option)
        {
            List<AveListBrowserInfo> lists = new List<AveListBrowserInfo>();
            using (var site = GetSite(option.ParentSiteId, option.SiteUrl))
            {
                using (var web = site.OpenWeb(option.ParentWebId))
                {
                    foreach (IAveList list in web.Lists)
                    {
                        try
                        {
                            //过滤掉Shared Packages
                            //ADO-77738
                            if (list.Title.Equals("Shared Packages") && list.Hidden)
                            {
                                continue;
                            }
                            lists.Add(ConverToBrowserInfo(list));
                        }
                        catch (Exception e)
                        {
                            Logger.Warn("An error occurred while getting list, parent web url is {0}, list title is {1}. Error message:{2}", web.Url, list.Title, e.ToString());
                        }
                    }
                }
            }

            lists.Sort(new AveListBrowserInfoComparer());
            lists.Insert(0, new AveListBrowserInfo()
            {
                ID = Guid.Empty,
                Name = "{System Folder}",
                Title = "{System Folder}",
                rootFolderName = "Root Folder",
            });
            option.ChildrenTotalCount = lists.Count;

            var pageCount = option.PerPage > option.ChildrenTotalCount ? option.ChildrenTotalCount : (int)option.PerPage;
            lists = lists.Skip<AveListBrowserInfo>(option.StartIndex).Take<AveListBrowserInfo>(pageCount).ToList<AveListBrowserInfo>();

            return lists;
        }
    }
}
