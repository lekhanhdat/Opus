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
    using Microsoft.Online.SharePoint.TenantAdministration;
    using Microsoft.SharePoint.Client;
    using Microsoft365.Authentication.TokenProvider;
    using Microsoft365.Common.Extension;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;
    using Microsoft365.SharePoint.CSOM;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class BrowserQuery : IDisposable
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(BrowserQuery));
        private const int RETRYCOUNT_DEFAULT = 3;
        private const int RETRYINTERVAL_DEFAULT = 5000;
        protected string USERAGENT { get; set; }= Microsoft365Configuration.CommonConfiguration.UserAgent;
        protected IATokenProvider TokenProvider { get; set; }
        protected RetryableClientContextFactory ClientContextFactory { get; set; }
        public BrowserQuery(IATokenProvider tokenProvider)
        {
            TokenProvider = tokenProvider;
            ClientContextFactory = new RetryableClientContextFactory(USERAGENT, tokenProvider, RETRYCOUNT_DEFAULT, RETRYINTERVAL_DEFAULT);
        }

        #region common
        private RetryableClientContext CreateContext(string siteUrl)
        {
            return ClientContextFactory.GetClientContext(siteUrl);
        }

        private RetryableProjectClientContext CreateProjectContext(string siteUrl)
        {
            return ClientContextFactory.GetProjectClientContext(siteUrl);
        }

        #endregion

        #region GetSiteCollectionInfos
        public List<SPSiteInfo> GetSiteCollections(string adminSiteUrl, SPSiteType type)
        {
            SPOSitePropertiesEnumerableFilter filter = new SPOSitePropertiesEnumerableFilter
            {
                IncludePersonalSite = PersonalSiteFilter.Exclude
            };
            switch (type)
            {
                case SPSiteType.GroupTeamSite:
                    filter.GroupIdDefined = 1;
                    break;
                case SPSiteType.SharePointSite:
                    filter.GroupIdDefined = 2;
                    break;
                case SPSiteType.PrivateChannelSite:
                    filter.Template = "TEAMCHANNEL";
                    filter.GroupIdDefined = 2;
                    break;
                case SPSiteType.OneDrive:
                    filter.IncludePersonalSite = PersonalSiteFilter.Include;
                    filter.Template = "SPSPERS";
                    break;
            }
            Stopwatch watch = Stopwatch.StartNew();
            var sites = GetSiteCollectionInfoInternal(adminSiteUrl, filter);
            OutputGetSiteCollectionDetail(filter, watch, sites);
            return sites;
        }

        private List<SPSiteInfo> GetSiteCollectionInfoInternal(string adminSiteUrl, SPOSitePropertiesEnumerableFilter filter)
        {
            var results = new List<SPSiteInfo>();
            try
            {
                using (var context = CreateContext(adminSiteUrl))
                {
                    Tenant tenant = new Tenant(context);
                    SPOSitePropertiesEnumerable sites = null;
                    string startIndex = "";
                    do
                    {
                        if (!string.IsNullOrEmpty(startIndex))
                        {
                            filter.StartIndex = startIndex;
                        }
                        sites = tenant.GetSitePropertiesFromSharePointByFilters(filter);
                        context.Load(sites, s => s.NextStartIndexFromSharePoint, s => s.Include(t => t.Url, t => t.LastContentModifiedDate, t => t.LockState));
                        context.ExecuteQueryWithRetry();
                        foreach (var siteProp in sites)
                        {
                            try
                            {
                                var info = new SPSiteInfo()
                                {
                                    Url = siteProp.Url.TrimEnd('/'),
                                    LastContentModifiedDate = siteProp.LastContentModifiedDate.Ticks,
                                    LockState = siteProp.LockState,
                                };
                                results.Add(info);
                            }
                            catch (Exception ex)
                            {
                                logger.Warn($"Get site last content modified time error.{ex}");
                            }
                        }
                        startIndex = sites.NextStartIndexFromSharePoint;

                    } while (sites.NextStartIndexFromSharePoint != null);
                };
            }
            catch (Exception e)
            {
                logger.Warn($"Get sites last content modified time error.{e}");
            }
            logger.Info($"Total {results.Count} sites scanned for last conent modified time.");
            return results;
        }

        private static void OutputGetSiteCollectionDetail(SPOSitePropertiesEnumerableFilter filter, Stopwatch watch, List<SPSiteInfo> sites)
        {
            logger.Info($@"Check no change site collection with following parameters
GroupIdDefined:{filter.GroupIdDefined};
Template:{filter.Template};
IncludePersonalSite:{filter.IncludePersonalSite};
SiteCount:{sites?.Count};
TimeCost:{watch.Elapsed}");
        }
        #endregion GetSiteCollectionInfos

        #region TestProjectLicense
        public CheckProjectAccountResult TestProjectLicense(string siteUrl)
        {
            var result = new CheckProjectAccountResult();
            try
            {
                using (var context = CreateProjectContext(siteUrl))
                {
                    context.Load(context.Projects, ps => ps.Include(p => p.Id));
                    context.ExecuteQueryWithRetry();
                }
                logger.Warn($"TestProjectLicense success.SiteUrl:{siteUrl}");
                return new CheckProjectAccountResult
                {
                    Success = true,
                    Error = null
                };
            }
            catch (Exception ex)
            {
                logger.Warn($"TestProjectLicense failed.SiteUrl:{siteUrl},Error:{ex}.");
                return new CheckProjectAccountResult
                {
                    Success = false,
                    Error = ex
                };
            }
        }
        #endregion

        #region browse children

        public WebBrowserInfo GetBrowserRootWeb(string siteUrl)
        {
            using (var context = CreateContext(siteUrl))
            {
                var webBrowserInfo = new WebBrowserInfo();

                Web rootWeb = context.Site.RootWeb;
                context.Load(rootWeb, w => w.ServerRelativeUrl,
                                             w => w.Id,
                                             w => w.Title,
                                             w => w.Language,
                                             w => w.HasUniqueRoleAssignments,
                                             w => w.WebTemplate,
                                             w => w.ParentWeb,
                                             w => w.Url);
                context.ExecuteQueryWithRetry();
                return rootWeb.ConvertToWebBrowserInfo(true);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="parentWebId"></param>
        /// <param name="startIndex">must be great than 0</param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public PagedBrowserResult<WebBrowserInfo> GetBrowserWebs(string siteUrl, Guid parentWebId, int startIndex, int perPage)
        {
            if (string.IsNullOrEmpty(siteUrl))
            {
                throw new ArgumentException($"siteUrl is null or empty");
            }
            if (parentWebId == Guid.Empty)
            {
                throw new ArgumentException($"parentWebId is not a valid Guid.");
            }
            if (startIndex < 0)
            {
                throw new ArgumentException($"startIndex is less than 0,Value:{startIndex}");
            }
            if (perPage <= 0)
            {
                throw new ArgumentException($"perPage is less than or equal 0,Value:{perPage}");
            }
            using (var context = CreateContext(siteUrl))
            {
                WebCollection subWebs = null;
                Web parentWeb = context.Site.OpenWebById(parentWebId);
                context.Load(parentWeb.Webs, webs => webs.IncludeWithDefaultProperties(w => w.HasUniqueRoleAssignments).
                                                          Where(tempWeb => tempWeb.AppInstanceId == Guid.Empty));
                context.ExecuteQueryWithRetry();
                subWebs = parentWeb.Webs;

                var pagedWebs = parentWeb.Webs.GetItemRange(startIndex, perPage).ToList().ConvertAll(t => t.ConvertToWebBrowserInfo(false));
                return new PagedBrowserResult<WebBrowserInfo>
                {
                    TotalCount = subWebs.Count,
                    Children = pagedWebs
                };
            }
        }

        public PagedBrowserResult<AppBrowserInfo> GetBrowserApps(string siteUrl, Guid parentWebId, int startIndex, int perPage)
        {
            using (var context = CreateContext(siteUrl))
            {
                Web web = context.Site.OpenWebById(parentWebId);
                ClientObjectList<AppInstance> apps = AppCatalog.GetAppInstances(context, web);
                context.Load(web, w => w.ServerRelativeUrl);
                context.Load(apps, s => s.IncludeWithDefaultProperties(app => app.Title, app => app.ProductId, app => app.AppWebFullUrl, app => app.Id, app => app.Status));
                context.ExecuteQueryWithRetry();
                return new PagedBrowserResult<AppBrowserInfo>
                {
                    TotalCount = apps.Count,
                    Children = apps.GetItemRange(startIndex, perPage).ToList().ConvertAll(t => t.ConvertToAppBrowserInfo())
                };
            }
        }

        public PagedBrowserResult<ListBrowserInfo> GetBrowserLists(string siteUrl, Guid parentWebId, int startIndex, int perPage)
        {
            using (var context = CreateContext(siteUrl))
            {
                var listInfoList = new List<ListBrowserInfo>();
                Web web = context.Site.OpenWebById(parentWebId);
                context.Load(context.Site, s => s.Url);
                context.Load(web.Lists, ls => ls.Include(l => l.Id,
                                                         l => l.ParentWebUrl,
                                                         l => l.Title,
                                                         l => l.BaseType,
                                                         l => l.BaseTemplate,
                                                         l => l.Hidden,
                                                         l => l.RootFolder.ServerRelativeUrl,
                                                         l => l.RootFolder.Name,
                                                         l => l.HasUniqueRoleAssignments,
                                                         l => l.EnableFolderCreation,
                                                         l => l.ParentWeb.Url));
                context.ExecuteQueryWithRetry();

                var pagedLists = web.Lists.GetItemRange(startIndex, perPage).ToList().ConvertAll(t => t.ConvertToListBrowserInfo());
                return new PagedBrowserResult<ListBrowserInfo>
                {
                    Children = pagedLists,
                    TotalCount = web.Lists.Count
                };
            }
        }

        public FolderBrowserInfo GetBrowserListRootFolder(string siteUrl, Guid parentWebId, Guid parentListId)
        {
            using (var context = CreateContext(siteUrl))
            {

                Web web = context.Site.OpenWebById(parentWebId);
                List list = web.Lists.GetById(parentListId);
                Folder folder = list.RootFolder;
                context.Load(list,
                    l => l.BaseType,
                    l => l.HasUniqueRoleAssignments,
                    l => l.Id,
                    l => l.RootFolder.ServerRelativeUrl,
                    l => l.RootFolder.Name,
                    l => l.RootFolder.UniqueId,
                    l => l.ParentWeb.Url);
                context.ExecuteQueryWithRetry();
                return list.ConvertToListRootFolderBrowserInfo();
            }
        }

        public FolderBrowserInfo GetBrowserWebRootFolder(string siteUrl, Guid parentWebId)
        {

            using (var context = CreateContext(siteUrl))
            {
                Web web = context.Site.OpenWebById(parentWebId);
                Folder webRootFolder = web.RootFolder;
                context.Load(web, w => w.Url);
                context.Load(webRootFolder, f => f.Name, f => f.ServerRelativeUrl, f => f.UniqueId);
                context.ExecuteQueryWithRetry();
                return web.ConvertToWebRootFolderBrowserInfo();
            }
        }

        /// <summary>
        /// |Name|parentlistId    |systemFolder| Result                  |
        /// |    |Not Guid.Empty  |true        |Load System Folders      |
        /// |    |Not Guid.Empty  |false       |Load List/Library Folders|
        /// |    |Guid.Empty      |true        |Load System Folders      |
        /// |    |Guid.Empty      |false       |Load System Folders      |
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="parentWebId"></param>
        /// <param name="parentlistId">if this is Guid.Empty, will query system folders</param>
        /// <param name="parentFolderUniqueId"></param>
        /// <param name="startIndex"></param>
        /// <param name="perPage"></param>
        /// <param name="systemFolder">load System folders or list/library folders if true.</param>
        /// <returns></returns>
        public PagedBrowserResult<FolderBrowserInfo> GetBrowserSubFolders(string siteUrl, Guid parentWebId, Guid parentlistId, Guid parentFolderUniqueId, int startIndex = 0, int perPage = int.MaxValue, bool systemFolder = false)
        {
            using (var context = CreateContext(siteUrl))
            {
                Web web = context.Site.OpenWebById(parentWebId);
                context.Load(context.Site, s => s.MaxItemsPerThrottledOperation);
                context.Load(web, w => w.Url);
                List list = null;
                if (parentlistId != Guid.Empty)
                {
                    list = web.Lists.GetById(parentlistId);
                    context.Load(list);
                }
                Folder folder = web.GetFolderById(parentFolderUniqueId);
                context.Load(folder, f => f.ItemCount, f => f.Properties, f => f.ServerRelativeUrl);
                context.ExecuteQuery();
                var maxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation;
                int folderCount;
                if (list == null || systemFolder)
                {
                    if (folder.ItemCount < maxItemsPerThrottledOperation)
                    {
                        return GetSystemFolderSubFolders(parentFolderUniqueId, startIndex, perPage, context, web.Url, folder);
                    }
                    throw new ArgumentOutOfRangeException("There are too many items in this folder. Fetch system folders in this folder is not supported due to api limitaion.");
                }

                folderCount = folder.Properties.FieldValues.ContainsKey("vti_foldersubfolderitemcount") ? Convert.ToInt32(folder.Properties.FieldValues["vti_foldersubfolderitemcount"]) : 0;
                if (folderCount == 0)
                {
                    return new PagedBrowserResult<FolderBrowserInfo>
                    {
                        Children = new List<FolderBrowserInfo> { },
                        TotalCount = 0
                    };
                }

                if ((folder.ItemCount < maxItemsPerThrottledOperation))
                {
                    return GetSmallListSubFolders(parentFolderUniqueId, startIndex, perPage, context, web, list, folder, folderCount);
                }
                else
                {
                    return GetLargeListSubFolders(parentFolderUniqueId, folder.ServerRelativeUrl, startIndex, perPage, context, web, list, folderCount);
                }
            }
        }

        /// <summary>
        /// this method have performance issue, if list is too large, item id range is very huge, this method may query very slow.
        /// </summary>
        /// <param name="parentFolderUniqueId"></param>
        /// <param name="parentFolderServerRelativeUrl"></param>
        /// <param name="startIndex"></param>
        /// <param name="perPage"></param>
        /// <param name="context"></param>
        /// <param name="web"></param>
        /// <param name="list"></param>
        /// <param name="folderCount"></param>
        /// <returns></returns>
        private static PagedBrowserResult<FolderBrowserInfo> GetLargeListSubFolders(Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, int startIndex, int perPage, RetryableClientContext context, Web web, List list, int folderCount)
        {
            var subFolders = GetLargeListSubFoldersInternal(parentFolderUniqueId, parentFolderServerRelativeUrl, context, web, list, folderCount);
            return new PagedBrowserResult<FolderBrowserInfo>
            {
                Children = subFolders.Skip(startIndex).Take(perPage).ToList(),
                TotalCount = folderCount
            };
        }

        private static IEnumerable<FolderBrowserInfo> GetLargeListSubFoldersInternal(Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, RetryableClientContext context, Web web, List list, int folderCount)
        {
            int processedFolders = 0;
            ListItemCollection listItems = null;
            int index = 0;
            do
            {
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = string.Format(
                            "<View Scope='Default'>" +
                            "<Query><Where><And><And><Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And><Eq><FieldRef Name='FSObjType' /><Value Type='Integer'>1</Value></Eq></And></Where></Query>" +
                            "<RowLimit>{2}</RowLimit>" +
                            "</View>", index, index + 5000, 2000);
                logger.Info("browser subfolders between {0} and {1}", index, index + 5000);
                camlQuery.FolderServerRelativePath = ResourcePath.FromDecodedUrl(parentFolderServerRelativeUrl);
                listItems = list.GetItems(camlQuery);
                context.Load(listItems, items => items.Include(item => item.FieldValues));
                int lastIndex = index;
                context.ExecuteQuery();
                if (listItems.Count > 0)
                {
                    for (int i = 0; i < listItems.Count; i++)
                    {
                        processedFolders++;
                        yield return listItems[i].ConvertToFolderBrowserInfo(list, web.Url, parentFolderUniqueId);
                        index = index < listItems[i].Id ? listItems[i].Id : index;
                    }
                }
                index = lastIndex + 2000 < index ? index : lastIndex + 2000;
            }
            while (processedFolders < folderCount);
        }

        private static PagedBrowserResult<FolderBrowserInfo> GetSystemFolderSubFolders(Guid parentFolderUniqueId, int startIndex, int perPage, RetryableClientContext context, string parentWebFullUrl, Folder folder)
        {
            var folders = new List<FolderBrowserInfo>();
            context.Load(folder.Folders, fs => fs.IncludeWithDefaultProperties(f => f.Properties));
            context.ExecuteQuery();
            var items = folder.Folders.Where(t=> IsSystemFolder(t)).GetItemRange(startIndex, perPage).ToList().ConvertAll(t => t.ConvertToBrowserInfo(parentFolderUniqueId, parentWebFullUrl, true));
            return new PagedBrowserResult<FolderBrowserInfo>
            {
                Children = items,
                TotalCount = folder.Folders.Count
            };
        }

        private static bool IsSystemFolder(Folder folder)
        {
            if (folder.Properties.FieldValues.ContainsKey("vti_listname") &&
                     folder.Properties["vti_listname"] != null &&
                     Guid.TryParse(folder.Properties["vti_listname"].ToString(), out Guid listId))
            {
                return true;
            }
            return false;
        }

        private static PagedBrowserResult<FolderBrowserInfo> GetSmallListSubFolders(Guid parentFolderUniqueId, int startIndex, int perPage, RetryableClientContext context, Web web, List list, Folder folder, int subFolderCount)
        {
            var folders = new List<FolderBrowserInfo>();
            context.Load(folder.Folders,
                fs =>
                    fs.IncludeWithDefaultProperties(f => f.ListItemAllFields)
                        .Where(
                            f =>
                                f.ListItemAllFields.ServerObjectIsNull != null
                                && f.ListItemAllFields.ServerObjectIsNull.Value == false));

            context.ExecuteQuery();
            var items = folder.Folders.GetItemRange(startIndex, perPage).ToList().ConvertAll(t => t.ListItemAllFields.ConvertToFolderBrowserInfo(list, web.Url, parentFolderUniqueId));
            return new PagedBrowserResult<FolderBrowserInfo>
            {
                Children = items,
                TotalCount = subFolderCount
            };
        }

        public PagedBrowserResult<ProjectBrowserInfo> GetBrowserProjects(string siteUrl, int startIndex, int perPage)
        {
            using (var context = CreateProjectContext(siteUrl))
            {
                var projInfoList = new List<ProjectBrowserInfo>();
                var projects = context.Projects;
                context.Load(projects, ps => ps.Include(
                    p => p.Name,
                    p => p.Id,
                    p => p.IsEnterpriseProject,
                    p => p.EnterpriseProjectType.Id,
                    p => p.ProjectSiteUrl,
                    p => p.IsCheckedOut));
                context.ExecuteQuery();
                return new PagedBrowserResult<ProjectBrowserInfo>
                {
                    Children = projects.GetItemRange(startIndex, perPage).ToList().ConvertAll(t => t.ConvertToProjectBrowserInfo()),
                    TotalCount = projects.Count
                };
            }
        }

        public void Dispose()
        {
            ClientContextFactory?.Dispose();
            ClientContextFactory = null;
            TokenProvider = null;
        }


        #endregion

        public Web GetWeb(string siteUrl)
        {
            using (var context = CreateContext(siteUrl))
            {
                var web = context.Web;
                context.Load(web, w => w.Title);
                context.Load(web.AssociatedOwnerGroup);
                context.Load(web.AssociatedOwnerGroup.Users);
                context.Load(web.AssociatedOwnerGroup.Users, we => we.IncludeWithDefaultProperties(e => e.AadObjectId));
                context.ExecuteQuery();
                return web;
            }
        }

        public string GetSiteLockState(string siteUrl, string adminUrl)
        {
            using (var context = CreateContext(adminUrl))
            {
                try
                {
                    var tenant = new Tenant(context);
                    var siteProp = tenant.GetSitePropertiesByUrl(siteUrl, false);
                    context.Load(siteProp, p => p.Url, p => p.LockState, p => p.Status);
                    context.ExecuteQuery();
                    logger.Info($"Site collection information: Url: {siteProp.Url}, Status: {siteProp.Status}, LockStatus: {siteProp.LockState}.");
                    return siteProp.LockState;
                }
                catch (ServerException ex)
                {
                    if (!string.IsNullOrEmpty(ex.ServerErrorTypeName) && ex.ServerErrorTypeName.Contains("SpoNoSiteException"))
                    {
                        return "NotExist";
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Exception occurred while get site status, site url: {siteUrl}, ex: {ex}");
                    return null;
                }
            }
        }

    }
}