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

    using AvePoint.Common.Portal;
    using AvePoint.ObjectModel.WebService;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.Client;
    using Microsoft.SharePoint.Client;
    using System;
    using Microsoft.SharePoint.Client.Publishing.Navigation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AveChangeType = AvePoint.Wrapper.Common.ChangeType;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using ClientFolder = Microsoft.SharePoint.Client.Folder;
    using SPChangeType = Microsoft.SharePoint.Client.ChangeType;
    using Microsoft.SharePoint.Client.Taxonomy;
    using Microsoft365.Authentication;
    using Microsoft.SharePoint.Client.Search.Query;
   
    public partial class AveClientOM2013Request
    {

        #region RecycleBin

        public virtual Dictionary<string, object> GetRecycleBin(string webServerRelativeUrl = null)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> recycleBin = new Dictionary<string, object>();
                try
                {
                    RecycleBinItemCollection binItems = null;
                    if (string.IsNullOrEmpty(webServerRelativeUrl))
                    {
                        binItems = context.Site.RecycleBin;
                    }
                    else
                    {
                        binItems = context.Site.OpenWeb(webServerRelativeUrl).RecycleBin;
                    }

                    ExceptionHandlingScope han = new ExceptionHandlingScope(context);
                    using (han.StartScope())
                    {
                        using (han.StartTry())
                        {
                            context.Load(context.Site.RecycleBin, bin => bin.IncludeWithDefaultProperties(temp => temp.Author, temp => temp.DeletedBy));
                        }
                        using (han.StartCatch())
                        {
                            context.Load(context.Site.RecycleBin, bin => bin.IncludeWithDefaultProperties());
                        }
                    }
                    context.ExecuteQuery();
                    AssembleRecycleBinProperties(binItems, recycleBin);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.GetRecycleBinError, context.Url, e.ToString());
                    throw;
                }
                return recycleBin;
            }
        }

        protected virtual void AssembleRecycleBinProperties(RecycleBinItemCollection recycleBinCollection, Dictionary<string, object> recycleBin)
        {
            var recycleBinList = new List<IDictionary<string, object>>();
            foreach (RecycleBinItem recycleBinItem in recycleBinCollection)
            {
                Dictionary<string, object> dicRecycleBin = new Dictionary<string, object>();
                CopyProperty(dicRecycleBin, recycleBinItem);
                if (recycleBinItem.Author.ServerObjectIsNull.HasValue && !recycleBinItem.Author.ServerObjectIsNull.Value)
                {
                    dicRecycleBin["Author" + AveObjectModelConstant.ObjectPropertySuffix] = recycleBinItem.Author.LoginName;
                }
                if (recycleBinItem.DeletedBy.ServerObjectIsNull.HasValue && !recycleBinItem.DeletedBy.ServerObjectIsNull.Value)
                {
                    dicRecycleBin["DeletedBy" + AveObjectModelConstant.ObjectPropertySuffix] = recycleBinItem.DeletedBy.LoginName;
                }
                recycleBinList.Add(dicRecycleBin);
            }
            recycleBin.AddChildren(recycleBinList);
        }

        #endregion RecycleBin

        #region Get Web

        public Dictionary<string, object> GetFirstUniqueNavigationWeb(string webServerRelativeUrl)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl.TrimEnd('/'));
                    string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                    context.Load(web, w => w.Navigation.UseShared, w => w.ServerRelativeUrl);
                    context.ExecuteQuery();
                    bool isUsedShared = web.Navigation.UseShared;
                    while (isUsedShared)
                    {
                        int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                        string parentWebServerRelativeUrl = web.ServerRelativeUrl.Substring(0, lastSlashIndex);
                        web = context.Site.OpenWeb(parentWebServerRelativeUrl);
                        context.Load(web, w => w.Navigation.UseShared, w => w.ServerRelativeUrl);
                        context.ExecuteQuery();
                        isUsedShared = web.Navigation.UseShared;
                    }
                    webProperties = GetWebProperties(context, web, context.Url, siteServerRelativeUrl, false);
                }
                catch (Exception e)
                {
                    webProperties["Exists"] = false;
                    mLogger.Debug(e.ToString());
                }
                return webProperties;
            }
        }

        public Dictionary<string, object> GetQuickLaunchFromInheritWeb(string webServerRelativeUrl)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                try
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl.TrimEnd('/'));
                    string siteServerRelativeUrl = AveUrlUtility.GetSiteServerRelativeUrl(context.Url);
                    context.Load(web, w => w.ServerRelativeUrl);
                    //context.ExecuteQuery();
                    //bool isUsedShared = web.Navigation.UseShared;
                    var properties = web.AllProperties;
                    context.Load(properties);
                    context.ExecuteQuery();
                    string isInheritCurrentNavigation = "False";
                    if (web.ServerRelativeUrl.Equals("/") || web.ServerRelativeUrl.Equals(siteServerRelativeUrl) || !properties.FieldValues.ContainsKey("__InheritCurrentNavigation"))
                    {
                    }
                    else
                    {
                        isInheritCurrentNavigation = (string)properties.FieldValues["__InheritCurrentNavigation"];
                    }
                    while (isInheritCurrentNavigation.Equals("True", StringComparison.OrdinalIgnoreCase))
                    {
                        int lastSlashIndex = web.ServerRelativeUrl.LastIndexOf('/');
                        string parentWebServerRelativeUrl = web.ServerRelativeUrl.Substring(0, lastSlashIndex);
                        web = context.Site.OpenWeb(parentWebServerRelativeUrl);
                        context.Load(web, w => w.ServerRelativeUrl);
                        var propertiesOfSub = web.AllProperties;
                        context.Load(propertiesOfSub);
                        context.ExecuteQuery();
                        if (web.ServerRelativeUrl.Equals("/") || web.ServerRelativeUrl.Equals(siteServerRelativeUrl, StringComparison.OrdinalIgnoreCase) || !propertiesOfSub.FieldValues.ContainsKey("__InheritCurrentNavigation"))
                        {
                            isInheritCurrentNavigation = "False";
                        }
                        else
                        {
                            isInheritCurrentNavigation = (string)propertiesOfSub.FieldValues["__InheritCurrentNavigation"];
                        }
                    }
                    webProperties = GetWebProperties(context, web, context.Url, siteServerRelativeUrl, false);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveClientOMRequestResource.CannotGetFirstUniqueNavigationWeb, webServerRelativeUrl, e.ToString());
                    webProperties["Exists"] = false;
                }
                return webProperties;
            }
        }

        public Dictionary<string, object> GetAllWebs()
        {
            using (var context = CreateRetryContext())
            {
                var webList = new List<IDictionary<string, object>>();
                Dictionary<string, object> allWebs = new Dictionary<string, object>();

                Web rootWeb = context.Site.RootWeb;
                context.Load(context.Site, s => s.Url, s => s.ServerRelativeUrl);
                LoadWeb(rootWeb, context);
                LoadSubSites(context, rootWeb);
                context.ExecuteQuery();
                webList.Add(GetWebProperties(context, rootWeb, context.Site.Url, context.Site.ServerRelativeUrl, true));
                foreach (Web web in rootWeb.Webs)
                {
                    //if (IsApplicationWeb(web))
                    //{
                    //    continue;
                    //}
                    Dictionary<string, object> dicWeb = new Dictionary<string, object>();
                    dicWeb = GetWebProperties(context, web, context.Site.Url, context.Site.ServerRelativeUrl, true);
                    webList.Add(dicWeb);
                    WebGetSubwebs(context, web, webList, context.Site.Url, context.Site.ServerRelativeUrl);
                }
                allWebs.AddChildren(webList);

                return allWebs;
            }
        }

        public Dictionary<string, object> GetWeb(Guid webId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> webProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWebById(webId);
                webProperties = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);

                return webProperties;
            }
        }

        public Dictionary<string, object> GetWeb(string webServerRelativeUrl)
        {
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            try
            {
                using (var context = CreateRetryContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl.TrimEnd('/'));
                    webProperties = GetWebProperties(context, web, context.Url, mSiteRelativeUrl, false);
                }
            }
            catch (Exception e)
            {
                mLogger.Debug(AveClientOMRequestResource.CannotGetWeb, webServerRelativeUrl, e.ToString());
                webProperties["Exists"] = false;
            }
            return webProperties;
        }

        #endregion Get Web

        #region Web Regional Setting

        public Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> regionalSettingProperties = new Dictionary<string, object>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web);
                RegionalSettings regionalSettings = web.RegionalSettings;
                context.Load(regionalSettings);
                context.Load(regionalSettings.TimeZone);
                context.Load(regionalSettings.InstalledLanguages);
                context.ExecuteQuery();
                CopyProperty(regionalSettingProperties, regionalSettings);
                regionalSettingProperties["InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix] = AssembleInstalledLanguages(regionalSettings.InstalledLanguages);
                Dictionary<string, object> timeZoneProperties = new Dictionary<string, object>();
                CopyProperty(timeZoneProperties, regionalSettings.TimeZone);
                timeZoneProperties["ID"] = Convert.ToUInt16(regionalSettings.TimeZone.Id);
                if (timeZoneProperties.ContainsKey("Id"))
                {
                    timeZoneProperties.Remove("Id");
                }
                regionalSettingProperties["TimeZone" + AveObjectModelConstant.ObjectPropertySuffix] = timeZoneProperties;
                return regionalSettingProperties;
            }
        }

        private Dictionary<string, object> AssembleInstalledLanguages(LanguageCollection languages)
        {
            Dictionary<string, object> container = new Dictionary<string, object>();

           var list = new List<IDictionary<string, object>>();
            container.AddChildren(list);

            foreach (Language language in languages)
            {
                Dictionary<string, object> languageDict = new Dictionary<string, object>();
                languageDict["DisplayName"] = language.DisplayName;
                languageDict["LCID"] = language.Lcid;
                list.Add(languageDict);
            }

            return container;
        }

        #endregion Web Regional Setting

        #region Get Site Property

        public int GetSiteOwnerId() //check addmin permission when add single sitecollection or save scan results
        {
            using (var context = CreateRetryContext())
            {
                Site site = context.Site;
                context.Load(site.Owner, o => o.Id);
                context.ExecuteQuery();
                return site.Owner.Id;
            }
        }

        public int GetSiteCompatibility()
        {
            using (var context = CreateRetryContext())
            {
                Site site = context.Site;
                context.Load(site, s => s.CompatibilityLevel);
                context.ExecuteQuery();
                return site.CompatibilityLevel;
            }
        }

        public int GetAuditFlags()
        {
            using (var context = CreateRetryContext())
            {
                context.Load(context.Site.Audit, a => a.AuditFlags);
                context.ExecuteQuery();
                return (int)context.Site.Audit.AuditFlags;
            }
        }

        #endregion 

        #region Get Site

        public Dictionary<string, object> GetSiteBasicProperties()
        {
            using (var context = CreateRetryContext())
            {
                Site site = context.Site;
                Web web = context.Web;
                context.Load(site);
                context.Load(web);
                context.ExecuteQuery();
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                siteProperties.Add("CompatibilityLevel", site.CompatibilityLevel);
                siteProperties.Add("RootWebWebTemplate", web.WebTemplate);
                siteProperties.Add("RootWebServerRelativeUrl", web.ServerRelativeUrl);
                siteProperties.Add("Configuration", web.Configuration);
                return siteProperties;
            }
        }

        public Dictionary<string, object> GetAdminCenterSite()
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> siteProperties = new Dictionary<string, object>();
                var site = context.Site;
                site.RetrieveSite();
                context.Load(site.RootWeb);
                context.ExecuteQuery();
                CopyProperty(siteProperties, context.Site);

                CompatibilityLevel = context.Site.CompatibilityLevel;
                InitHttpWebRequestCommon(context);
                Dictionary<string, object> rootWebProperties = new Dictionary<string, object>();
                CopyProperty(rootWebProperties, context.Site.RootWeb);
                rootWebProperties["IsRootWeb"] = true;
                siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                mSiteRelativeUrl = context.Site.ServerRelativeUrl;
                return siteProperties;
            }
        }

        public Dictionary<string, object> GetSite()
        {
            try
            {
                using (var context = CreateRetryContext())
                {
                    Dictionary<string, object> siteProperties = new Dictionary<string, object>();

                    ExceptionHandlingScope ehScope = new ExceptionHandlingScope(context);
                    //failed to load owner in admin sitecollection
                    using (ehScope.StartScope())
                    {
                        using (ehScope.StartTry())
                        {
                            //context.Load(context.Site);
                            var site = context.Site;
                            site.RetrieveSite();
                            context.Load(site, s => s.Usage);
                            context.Load(site.Owner, o => o.Id);
                            LoadWeb(site.RootWeb, context);
                        }
                        using (ehScope.StartCatch())
                        {
                            //context.Load(context.Site);
                            var site = context.Site;
                            site.RetrieveSite();
                            context.Load(site.RootWeb);
                        }
                    }
                    context.ExecuteQuery();
                    this.maxItemsPerThrottledOperation = context.Site.MaxItemsPerThrottledOperation;
                    mLogger.Info("current site max items throttle limit is :{0}", this.maxItemsPerThrottledOperation);
                    CopyProperty(siteProperties, context.Site);
                    if (!ehScope.HasException)
                    {
                        AveUsageInfo usage = new AveUsageInfo();
                        AssembleUsageProperties(usage, context.Site.Usage);
                        siteProperties["Usage"] = usage;
                    }

                    CompatibilityLevel = context.Site.CompatibilityLevel;
                    InitHttpWebRequestCommon(context);
                    if (!ehScope.HasException)
                    {
                        //siteProperties.Add("SyndicationEnabled", context.Site.RootWeb.SyndicationEnabled);
                        siteProperties["IsMoss"] = false;
                        siteProperties["ExternalSharingTipsEnabled"] = context.Site.ExternalSharingTipsEnabled;
                        siteProperties["Owner" + AveObjectModelConstant.ObjectPropertySuffix] = context.Site.Owner.Id;
                        Dictionary<string, object> rootWebProperties = GetWebProperties(context, context.Site.RootWeb, mWebUrl, context.Site.ServerRelativeUrl, true);
                        siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                    }
                    else
                    {
                        mLogger.Error($"fail get site, error code :{ehScope.ServerErrorCode}, error message:{ehScope.ErrorMessage}" +
                            $",server error type name: {ehScope.ServerErrorTypeName}, server error detail:{ehScope.ServerErrorDetails}" +
                            $",server error value:{ehScope.ServerErrorValue}");
                        Dictionary<string, object> rootWebProperties = new Dictionary<string, object>();
                        CopyProperty(rootWebProperties, context.Site.RootWeb);
                        CopyUserResourceProperty(rootWebProperties, context.Site.RootWeb);
                        if (ehScope.ErrorMessage.Contains("Attempted to perform an unauthorized operation"))
                        {
                            rootWebProperties.Add("LoadRootWebErrorMsg", "RM_GS_Email_App_Permission_Message");
                        }
                        else
                        {
                            rootWebProperties.Add("LoadRootWebErrorMsg", ehScope.ErrorMessage);
                        }
                        rootWebProperties["IsRootWeb"] = true;
                        siteProperties["RootWeb" + AveObjectModelConstant.ObjectPropertySuffix] = rootWebProperties;
                    }
                    mSiteRelativeUrl = context.Site.ServerRelativeUrl;
                    //siteProperties.Add("IsPublish", false);
                    OuputsiteProperties(siteProperties);
                    return siteProperties;
                }
            }
            catch (Microsoft.SharePoint.Client.ServerException mse)
            {
                mLogger.Info($"GetSite failed with ServerException.Message:{mse.Message}." +
                    $"ServerErrorCode:{mse.ServerErrorCode}." +
                    $"ServerErrorDetails:{mse.ServerErrorDetails}." +
                    $"ServerErrorTraceCorrelationId:{mse.ServerErrorTraceCorrelationId}." +
                    $"ServerErrorTypeName:{mse.ServerErrorTypeName}." +
                    $"ServerErrorValue:{mse.ServerErrorValue}." +
                    $"ServerStackTrace:{mse.ServerStackTrace}." +
                    $"Source:{mse.Source}." +
                    $"StackTrace:{mse.StackTrace}.");
                throw;
            }
        }



        private void OuputsiteProperties(Dictionary<string, object> siteProperties)
        {
            try
            {
                mLogger.Info($"[SAAS-38254]Site properties output:{FormatOutput.Process(siteProperties)}");
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when out put site properties , due to {0}", e);
            }
        }
        private void AssembleUsageProperties(AveUsageInfo aveUsage, UsageInfo usage)
        {
            aveUsage.Bandwidth = usage.Bandwidth;
            aveUsage.DiscussionStorage = usage.DiscussionStorage;
            aveUsage.Hits = usage.Hits;
            aveUsage.Storage = usage.Storage;
            aveUsage.StoragePercentageUsed = usage.StoragePercentageUsed;
            aveUsage.Visits = usage.Visits;
        }

        private void InitHttpWebRequestCommon(ClientContext context)
        {
            if (tokenProvider.TokenType != TokenType.Bearer)
            {
                //if (context.Site.CompatibilityLevel == 15)
                //{
                    mRequestCommon = new AveHttpWebRequestCommon2013(mWebUrl, tokenProvider, mInternalServerVersion);
                //}
                //else
                //{
                //    mRequestCommon = new AveHttpWebRequestCommon2010(mWebUrl, tokenProvider, mServerVersion, mInternalServerVersion);
                //}
            }
            else
            {
                mRequestCommon = new AveHttpWebRequestCommonEmpty();
            }
        }

        #endregion Get Site

        #region Get Web Property

        public string GetWebTemplateConfiguration(string webRelativeUrl)
        {
            using (var context = CreateRetryContext())
            {
                var web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web, w => w.Configuration, w => w.WebTemplate);
                context.ExecuteQuery();
                return string.Format("{0}#{1}", web.WebTemplate, web.Configuration);
            }
        }

        public string GetTenantAppCatalogSite(string webRelativeUrl)
        {
            using (var context = CreateRetryContext())
            {
                var web = context.Site.OpenWeb(webRelativeUrl);
                KeywordQuery keywordQuery = new KeywordQuery(web.Context)
                {
                    TrimDuplicates = false
                };
                keywordQuery.QueryText = "contentclass:STS_Site AND SiteTemplate:APPCATALOG";
                keywordQuery.SelectProperties.Add("SPSiteUrl");
                SearchExecutor searchExec = new SearchExecutor(web.Context);
                ClientResult<ResultTableCollection> results = searchExec.ExecuteQuery(keywordQuery);
                context.ExecuteQuery();
                if (results != null)
                {
                    if (results.Value[0].RowCount > 0)
                    {
                        var row = results.Value[0].ResultRows.First();
                        if(row == null)
                        {
                            return "";
                        }
                        return row["SPSiteUrl"] != null ? row["SPSiteUrl"].ToString() : "";
                    }
                }
            }
            return null;
        }

        public bool DoesUserHavePermissions(string webServerRelativeUrl, int permissionMask)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                BasePermissions permissions = new BasePermissions();
                permissions.Set((PermissionKind)permissionMask);
                ClientResult<bool> doesUserHavePermissions = web.DoesUserHavePermissions(permissions);
                context.ExecuteQuery();
                return doesUserHavePermissions.Value;
            }
        }

        public int GetWebWorkingLanguage(string url)
        {
            using (var context = CreateRetryContext(url))
            {
                var serverRelativeUrl = AveUrlUtility.GetServerRelativeUrl(url);
                var web = context.Site.OpenWebUsingPath(ResourcePath.FromDecodedUrl(serverRelativeUrl));
                var processor = new WorkingLanguageProcessor();
                int language = processor.GetWorkingLanguage(context, web, mUserAccountInfo);
                return language;
            }
        }
        public string GetAuthor(string webServerRelativeUrl)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.Author);
                context.ExecuteQuery();
                return web.Author.LoginName;
            }
        }
        #endregion Get Web Property

        #region Navigation 

        public Ave2013NavigationInfo Get2013Navigation(string webServerRelativeUrl, bool isPublishFeatureEnable)
        {
            Ave2013NavigationInfo navigationInfo = new Ave2013NavigationInfo();

            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                WebNavigationSettings webNavigationSettings = TaxonomyNavigation.GetWebNavigationSettings(context, web);
                context.Load(webNavigationSettings);
                context.Load(webNavigationSettings, w => w.GlobalNavigation, w => w.CurrentNavigation);
                context.ExecuteQuery();
                if (webNavigationSettings.ServerObjectIsNull.HasValue && webNavigationSettings.ServerObjectIsNull.Value)
                {
                    return navigationInfo;
                }
                navigationInfo.AddNewPagesToNavigation = webNavigationSettings.AddNewPagesToNavigation;
                navigationInfo.CreateFriendlyUrlsForNewPages = webNavigationSettings.CreateFriendlyUrlsForNewPages;

                navigationInfo.CurrentNavigation = new AveStandardNavigationSettings();
                navigationInfo.CurrentNavigation.Source = (AveStandardNavigationSource)webNavigationSettings.CurrentNavigation.Source;
                navigationInfo.CurrentNavigation.TermSetId = webNavigationSettings.CurrentNavigation.TermSetId;
                navigationInfo.CurrentNavigation.TermStoreId = webNavigationSettings.CurrentNavigation.TermStoreId;

                navigationInfo.GlobalNavigation = new AveStandardNavigationSettings();
                navigationInfo.GlobalNavigation.Source = (AveStandardNavigationSource)webNavigationSettings.GlobalNavigation.Source;
                navigationInfo.GlobalNavigation.TermSetId = webNavigationSettings.GlobalNavigation.TermSetId;
                navigationInfo.GlobalNavigation.TermStoreId = webNavigationSettings.GlobalNavigation.TermStoreId;

                StandardNavigationSettings navigationSettings = null;
                if (webNavigationSettings.CurrentNavigation.Source == StandardNavigationSource.TaxonomyProvider)
                {
                    navigationSettings = webNavigationSettings.CurrentNavigation;
                }
                else if (webNavigationSettings.GlobalNavigation.Source == StandardNavigationSource.TaxonomyProvider)
                {
                    navigationSettings = webNavigationSettings.GlobalNavigation;
                }

                if (navigationSettings != null)
                {
                    try
                    {
                        TaxonomySession taxSession = TaxonomySession.GetTaxonomySession(context);
                        TermStore termStore = taxSession.TermStores.GetById(navigationSettings.TermStoreId);
                        TermSet termSet = termStore.GetTermSet(navigationSettings.TermSetId);
                        //termset had been delete
                        ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(context);
                        using (exceptionScope.StartScope())
                        {
                            using (exceptionScope.StartTry())
                            {
                                context.Load(termSet.Group);
                            }
                            using (exceptionScope.StartCatch())
                            {
                                context.Load(termSet, tS => tS.Id);
                            }
                        }
                        context.ExecuteQuery();
                        if (exceptionScope.HasException && !termSet.IsPropertyAvailable("Id"))
                        {
                            navigationInfo.CurrentNavigation.TermGroupId = Guid.Empty;
                            navigationInfo.GlobalNavigation.TermGroupId = Guid.Empty;
                        }
                        else
                        {
                            navigationInfo.CurrentNavigation.TermGroupId = termSet.Group.Id;
                            navigationInfo.GlobalNavigation.TermGroupId = termSet.Group.Id;
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("An error accured when get the term set witch relatived to the navigation, term store id:{0}, term set id:{1}, error message:{2}", navigationSettings.TermStoreId, navigationSettings.TermSetId, e);
                    }
                }
            }

            return navigationInfo;
        }

        public Dictionary<string, object> GetNavigation(string webServerRelativeUrl, bool isPublishFeatureEnable)
        {
            Dictionary<string, object> nodesProp = null;
            try
            {
                if (tokenProvider.TokenType != TokenType.Bearer)
                {
                    nodesProp = new Dictionary<string, object>();
                    //需要支持SearchNavigation的还原，在这里加了一项SearchNavigation的页面路径。
                    string[] pageUrls = { "/_layouts/15/AreaNavigationSettings.aspx", "/_layouts/15/EnhancedSearch.aspx?level=site" };
                    foreach (string pageUrl in pageUrls)
                    {
                        string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + pageUrl;
                        string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);
                        string searchContent = "newNode = new NavigationNode(";
                        AveHttpWebRequestUtility.GetNodesProperties(html, searchContent, nodesProp);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get Web:{0} Navigation failed.Error Message:{1}", webServerRelativeUrl, ex.ToString());
            }
            return this.GetNavigation(webServerRelativeUrl, nodesProp, isPublishFeatureEnable);
        }

        protected Dictionary<string, object> GetNavigation(string webServerRelativeUrl, Dictionary<string, object> nodesProp, bool isPublishFeatureEnable)
        {
            string tempWebUrl = WebAppName.TrimEnd('/') + webServerRelativeUrl;
            using (var context = CreateRetryContext(tempWebUrl))
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.Navigation);
                context.Load(web, w => w.Navigation.QuickLaunch, w => w.Navigation.TopNavigationBar);
                NavigationNode searchNode = web.Navigation.GetNodeById(0x410);
                context.Load(searchNode, node => node.Children);
                context.ExecuteQuery();
                Dictionary<string, object> navigationProperties = new Dictionary<string, object>();
                CopyProperty(navigationProperties, web.Navigation);
                navigationProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = web.Navigation.Path;

                NavigationNodeCollection quickLaunchCollection = web.Navigation.QuickLaunch;
                NavigationNodeCollection topLinkBarCollection = web.Navigation.TopNavigationBar;
                #region  QuickLaunch
                if (!(quickLaunchCollection.ServerObjectIsNull.HasValue && quickLaunchCollection.ServerObjectIsNull.Value))
                {
                    List<IDictionary<string, object>> quickLaunchList = NavigationNodeCollectionToList(web.Navigation.QuickLaunch, nodesProp);
                    Dictionary<string, object> quickLaunchProperties = new Dictionary<string, object>();
                    quickLaunchProperties.AddChildren(quickLaunchList);
                    navigationProperties["QuickLaunch" + AveObjectModelConstant.ObjectPropertySuffix] = quickLaunchProperties;

                    Dictionary<string, object> quickLaunchParentProperties = new Dictionary<string, object>();
                    quickLaunchParentProperties["Title"] = "Quick launch";
                    quickLaunchParentProperties["Id"] = 1025;
                    quickLaunchParentProperties["Children" + AveObjectModelConstant.ObjectPropertySuffix] = quickLaunchProperties;
                    quickLaunchParentProperties["ClientContext"] = context;
                    navigationProperties["QuickLaunchParent" + AveObjectModelConstant.ObjectPropertySuffix] = quickLaunchParentProperties;
                }
                #endregion
                #region  TopLinkBar
                if (!(topLinkBarCollection.ServerObjectIsNull.HasValue && topLinkBarCollection.ServerObjectIsNull.Value))
                {
                    var topNavigationBarList = NavigationNodeCollectionToList(web.Navigation.TopNavigationBar, nodesProp);
                    if (web.Navigation.TopNavigationBar.Count == 1 && isPublishFeatureEnable && nodesProp != null) //SAAS-1540
                    {
                        bool needRemoveDefaultNav = false;
                        foreach (KeyValuePair<string, object> pair in nodesProp)
                        {
                            if (pair.Key.Contains("," + web.Navigation.TopNavigationBar[0].Id.ToString()))
                            {
                                needRemoveDefaultNav = true;
                                break;
                            }
                        }
                        if (!needRemoveDefaultNav)
                        {
                            topNavigationBarList.RemoveAt(0);
                        }
                    }
                    Dictionary<string, object> topNavigationBarProperties = new Dictionary<string, object>();
                    topNavigationBarProperties.AddChildren(topNavigationBarList);
                    navigationProperties["TopNavigationBar" + AveObjectModelConstant.ObjectPropertySuffix] = topNavigationBarProperties;

                    Dictionary<string, object> topNavigationBarParentProperties = new Dictionary<string, object>();
                    topNavigationBarParentProperties["Title"] = "SharePoint Top Navigation Bar";
                    topNavigationBarParentProperties["Id"] = 1002;
                    topNavigationBarParentProperties["Children" + AveObjectModelConstant.ObjectPropertySuffix] = topNavigationBarProperties;
                    topNavigationBarParentProperties["ClientContext"] = context;
                    navigationProperties["TopNavigationBarParent" + AveObjectModelConstant.ObjectPropertySuffix] = topNavigationBarParentProperties;
                }
                #endregion
                #region  SearchNav
                if (!(searchNode.ServerObjectIsNull.HasValue && searchNode.ServerObjectIsNull.Value))
                {
                    mLogger.Info("Start get the SearchNav.");
                    var searchNavList = NavigationNodeCollectionToList(searchNode.Children, nodesProp);
                    Dictionary<string, object> searchNavProperties = new Dictionary<string, object>();
                    searchNavProperties.AddChildren(searchNavList);
                    navigationProperties["SearchNav" + AveObjectModelConstant.ObjectPropertySuffix] = searchNavProperties;

                    Dictionary<string, object> searchNavParentProperties = new Dictionary<string, object>();
                    searchNavParentProperties["Title"] = "Search";
                    searchNavParentProperties["Id"] = 1040;
                    searchNavParentProperties["Children" + AveObjectModelConstant.ObjectPropertySuffix] = searchNavProperties;
                    searchNavParentProperties["ClientContext"] = context;
                    navigationProperties["SearchNavParent" + AveObjectModelConstant.ObjectPropertySuffix] = searchNavParentProperties;
                }
                #endregion
                return navigationProperties;
            }
        }

        #endregion Navigation

        #region Field

        //有些field在XML里会有RelatedField这项，还原这个field会一并把RelatedField也还回去。
        //这个方法就是单独取一个RelatedField的属性。
        public Dictionary<string, object> GetRelatedFieldProperties(string webServerRelativeUrl, string fieldName, string fieldSource, string listName, Guid listId)
        {
            using (var context = CreateRetryContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                Dictionary<string, object> fieldProperties = null;
                FieldCollection fields = null;
                switch (fieldSource)
                {
                    case "list.fields":
                        List list = web.Lists.GetById(listId);
                        fields = list.Fields;
                        break;
                    case "web.fields":
                        fields = web.Fields;
                        break;
                    default:
                        break;
                }
                if (fields != null)
                {
                    context.Load(fields, fs => fs.IncludeWithDefaultProperties().Where(f => f.InternalName == fieldName));
                    context.ExecuteQuery();
                    if (fields.Count != 0)
                    {
                        fieldProperties = new Dictionary<string, object>();
                        AssembleSingleFieldProperties(fieldProperties, fields[0]);
                    }
                }
                return fieldProperties;
            }
        }

        #endregion Field
    }
}
