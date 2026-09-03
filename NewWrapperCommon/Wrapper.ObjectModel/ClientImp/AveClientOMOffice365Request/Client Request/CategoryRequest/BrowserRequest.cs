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
using System.Linq;
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Online.SharePoint.TenantAdministration;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request
    {
        [ReplaceByAPI]
        public override List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl)
        {
            try
            {
                using (AveClientContext context = InitClientObject(tenantAdminSiteUrl))     //mObj should be the cookieContainer we get from tenant admin site
                {
                    Tenant tenant = new Tenant(context);
                    SPOSitePropertiesEnumerable sitePropertyEnum = null;
                    List<Dictionary<string, object>> managedSiteCollections = new List<Dictionary<string, object>>();
                    int tempIndex = 0;
                    do
                    {
                        sitePropertyEnum = tenant.GetSitePropertiesFromSharePoint(tempIndex.ToString(), true);
                        context.Load(sitePropertyEnum);
                        context.ExecuteQuery();
                        foreach (SiteProperties siteProperty in sitePropertyEnum)
                        {
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add("SiteCollectionUrl", siteProperty.Url.TrimEnd('/'));
                            properties.Add("CompatibilityLevel", siteProperty.CompatibilityLevel);
                            properties.Add("WebTemplateName", siteProperty.Template);
                            properties.Add("Lcid", siteProperty.Lcid);
                            managedSiteCollections.Add(properties);
                        }
                        tempIndex += sitePropertyEnum.Count;

                    }
                    while (sitePropertyEnum != null && sitePropertyEnum.Count >= 300);
                    return managedSiteCollections;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to load site collections, admin site collection url : {0}, error information : {1}", tenantAdminSiteUrl, e.ToString());
                return null;
            }
        }


        [ReplaceByAPI]
        public override List<Dictionary<string, object>> GetOneDriveSiteCollectionsList(string tenantAdminSiteUrl)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter
            {
                IncludeDetail = true,
                IncludePersonalSite = PersonalSiteFilter.Include,
                Template = "SPSPERS"
            };
            var collection = GetSiteCollectionsList(tenantAdminSiteUrl, speFilter);
            return collection;
        }

        [ReplaceByAPI]
        public override List<Dictionary<string, object>> GetGroupSiteCollectionsList(string tenantAdminSiteUrl)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter
            {
                IncludeDetail = true,
                Template = "GROUP#0",
                IncludePersonalSite = PersonalSiteFilter.Exclude
            };
            return GetSiteCollectionsList(tenantAdminSiteUrl, speFilter);
        }

        [ReplaceByAPI]
        public override List<Dictionary<string, object>> GetAllSiteCollectionsList(string tenantAdminSiteUrl, bool inlcudeOneDriveSite, List<string> excludeTempaltes)
        {
            SPOSitePropertiesEnumerableFilter speFilter = new SPOSitePropertiesEnumerableFilter
            {
                IncludeDetail = true,
                IncludePersonalSite = inlcudeOneDriveSite ? PersonalSiteFilter.Include : PersonalSiteFilter.Exclude
            };
            var collection = GetSiteCollectionsList(tenantAdminSiteUrl, speFilter);
            if (collection != null && excludeTempaltes != null)
            {
                return collection.Where(
                    element => excludeTempaltes.FirstOrDefault(
                        tempalte => tempalte.Equals(element["WebTemplateName"].ToString(), StringComparison.OrdinalIgnoreCase)) == null)
                    .ToList();
            }
            return collection;
        }


        private List<Dictionary<string, object>> GetSiteCollectionsList(string tenantAdminSiteUrl, SPOSitePropertiesEnumerableFilter filter)
        {
            try
            {
                using (AveClientContext context = InitClientObject(mWebUrl)) //the admin url is saved in request at constructor    //mObj should be the cookieContainer we get from tenant admin site
                {
                    Tenant tenant = new Tenant(context);
                    SPOSitePropertiesEnumerable sitePropertyEnum = null;
                    List<Dictionary<string, object>> managedSiteCollections = new List<Dictionary<string, object>>();
                    string tempIndex = null;
                    do
                    {
                        filter.StartIndex = tempIndex;
                        sitePropertyEnum = tenant.GetSitePropertiesFromSharePointByFilters(filter);
                        context.Load(sitePropertyEnum);
                        context.ExecuteQuery();
                        foreach (SiteProperties siteProperty in sitePropertyEnum)
                        {
                            //REDIRECTSITE#0 is just a placeholder site. Please contact wrapper team if you need this site.  Please refer to https://docs.microsoft.com/en-us/sharepoint/manage-site-redirects
                            if (string.Equals(siteProperty.Template, "REDIRECTSITE#0", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            if (string.Equals(siteProperty.LockState, "NoAccess", StringComparison.OrdinalIgnoreCase))
                            {
                                mLogger.Warn("The site collection {0} lock state is {1}", siteProperty.Url, siteProperty.LockState);
                                continue;
                            }
                            if (string.Equals(siteProperty.LockState, "ReadOnly", StringComparison.OrdinalIgnoreCase))
                            {
                                if (tokenProviders.MainTokenProvider.TokenType != Office365.Api.TokenType.Bearer)//App profile does not add site administrator.
                                {
                                    mLogger.Warn("The site collection {0} lock state is {1}", siteProperty.Url, siteProperty.LockState);
                                    continue;
                                }
                            }
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            CopyProperty(properties, siteProperty);
                            properties.Add("SiteCollectionUrl", siteProperty.Url.TrimEnd('/'));
                            properties.Add("WebTemplateName", siteProperty.Template);
                            //properties.Add("CompatibilityLevel", siteProperty.CompatibilityLevel);
                            //properties.Add("Lcid", siteProperty.Lcid);
                            //properties.Add("LockState", siteProperty.LockState);

                            managedSiteCollections.Add(properties);
                        }
                        tempIndex = sitePropertyEnum.NextStartIndexFromSharePoint;
                    }
                    while (tempIndex != null);

                    return managedSiteCollections;
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to load site collections, admin site collection url : {0}, error information : {1}", tenantAdminSiteUrl, e.ToString());
                return null;
            }
        }


        [NoAPIAttribute]
        public override void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            mWebServiceRequest.BrowserEnableUserFormTemplate(formTemplateUrl);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> GetBrowserSiteInfo()
        {
            using (var context = CreateContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                try
                {
                    context.Load(context.Site, site => site.Id, site => site.ReadOnly, site => site.CompatibilityLevel);
                    context.Load(context.Site.RootWeb, web => web.WebTemplate, web => web.Configuration, web => web.Language);
                    context.ExecuteQuery();
                    CopyProperty(siteProperties, context.Site);
                    Dictionary<string, object> rootWebProperties = new Dictionary<string, object>();
                    CopyProperty(rootWebProperties, context.Site.RootWeb);
                    rootWebProperties["IsRootWeb"] = true;
                    siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                }
                catch (Exception e)
                {
                    mLogger.Debug("An error occurred while get browser site info, url: {0}, error: {1}", context.Url, e);
                    throw;
                }
                return siteProperties;
            }
        }

        [KeepOriginalWithAPI]
        public override AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            return base.GetBrowserRootWeb(option);
    }
        [KeepOriginalWithAPI]
        public override List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            return base.GetBrowserWebs(option);
        }
        [KeepOriginalWithAPI]
        public override AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWebById(option.ParentWebId);
                List list = web.Lists.GetById(option.ParentListId);
                Folder folder = list.RootFolder;
                context.Load(context.Site, s => s.Url);
                context.Load(folder, f => f.UniqueId, f => f.ServerRelativeUrl, f => f.Name);
                context.Load(folder.ListItemAllFields, item => item.HasUniqueRoleAssignments);
                context.ExecuteQuery();
                return new AveFolderBrowserInfo
                {
                    HasUniqueRoleAssignments = folder.ListItemAllFields.IsPropertyAvailable("HasUniqueRoleAssignments") ? folder.ListItemAllFields.HasUniqueRoleAssignments : false,
                    UniqueId = folder.UniqueId,
                    Name = folder.Name,
                    ParentId = option.ParentListId,
                    Url = new Uri(new Uri(context.Site.Url), folder.ServerRelativeUrl).ToString(),
                    ServerRelativeUrl = folder.ServerRelativeUrl,
                };
            }
        }
        [KeepOriginalWithAPI]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "etag is property name")]
        public override List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            int childrenCount = 0;
            string Ids = string.Empty;
            List<AveFolderBrowserInfo> folders = new List<AveFolderBrowserInfo>();
            var queryItem = new CamlQuery
            {
                ViewXml = string.Format("<View Scope=\"\"><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">1</Value></Eq></Where></Query><RowLimit>{0}</RowLimit></View>", option.PerPage),
                FolderServerRelativePath = ResourcePath.FromDecodedUrl(option.ParentFolderServerRelativeUrl),
                ListItemCollectionPosition = new ListItemCollectionPosition { PagingInfo = GetPageInfo(option) },
            };

            var queryCount = new CamlQuery()
            {
                ViewXml = "<View><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">1</Value></Eq></Where></Query></View>",
                FolderServerRelativePath = ResourcePath.FromDecodedUrl(option.ParentFolderServerRelativeUrl),
            };

            using (AveClientContext context = CreateContext())
            {
                List<Folder> tempFolders = null;
                Web web = context.Site.OpenWebById(option.ParentWebId);
                List list = web.Lists.GetById(option.ParentListId);
                context.Load(list, l => l.BaseType, l => l.ItemCount);
                context.Load(list.RootFolder, folder => folder.ServerRelativeUrl);
                context.ExecuteQuery();
                if (IsThrottled(list.ItemCount))
                {
                    childrenCount = QuerySubFoldersCountForLargeList(context, list, option.ParentFolderServerRelativeUrl, queryCount, ref Ids);
                    tempFolders = QueryFoldersForLargeList(context, list, option.ParentFolderServerRelativeUrl, queryItem);
                    option.PageInfo = Ids;
                }
                else
                {

                    var itemCount = list.GetItems(queryCount);
                    var items = list.GetItems(queryItem);
                    context.Load(itemCount, count => count.Include(i => i.Id));
                    LoadBrowserFolderProperty(context, items);
                    context.ExecuteQuery();

                    tempFolders = items.Select(item => item.Folder).ToList();
                    childrenCount = itemCount.Count;
                    //每10个item 记录一次Id,对应browser界面 一页10个item,用于分页逻辑
                    for (int i = 1; i <= itemCount.Count; i++)
                    {
                        if (i % 10 == 0)
                        {
                            Ids = string.Format("{0},{1}", Ids, itemCount[i - 1].Id);
                        }
                    }
                }

                foreach (var temp in tempFolders)
                {
                    folders.Add(new AveFolderBrowserInfo
                    {
                        UniqueId = temp.UniqueId,
                        Name = temp.Name,
                        ServerRelativeUrl = temp.ServerRelativeUrl,
                        Url = new Uri(new Uri(this.mWebUrl), temp.ServerRelativeUrl).ToString(),
                        ParentListId = option.ParentListId,
                        ParentId = option.ParentFolderId,
                        Hidden = temp.ListItemAllFields.IsPropertyAvailable("Id"),
                        HasUniqueRoleAssignments = temp.ListItemAllFields.IsPropertyAvailable("HasUniqueRoleAssignments") ? temp.ListItemAllFields.HasUniqueRoleAssignments : false,

                    });
                }
                option.ChildrenTotalCount = childrenCount;
                option.PageInfo = Ids;
                return folders;
            }
        }
        [KeepOriginalWithAPI]
        public override List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option)
        {
            return base.GetBrowserItems(option);
        }
        [KeepOriginalWithAPI]
        public override List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option)
        {
            return base.GetBrowserLists(option);
        }
        [KeepOriginalWithAPI]
        public override List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            return base.GetBrowserItemVersions(option);
        }
        [KeepOriginalWithAPI]
        public override string GetServerVersion()
        {
            using (AveClientContext context = CreateContext())
            {
                context.ExecuteQuery();
                return context.ServerVersion.ToString();
            }
        }
    }
}
