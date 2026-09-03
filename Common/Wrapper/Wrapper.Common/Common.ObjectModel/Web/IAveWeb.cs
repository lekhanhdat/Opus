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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWeb : IAveSecurableObject, IDisposable
    {
        int GetWorkingLanguage();
        string AlternateCssUrl { get; set; }
        Guid TaxonomyListId { get; }
        Queue<IAveList> CachingFieldsList { get;}
        IAveAlertCollection Alerts { get; }
        IAveAlertCollection AlertsV2 { get; }
        bool AllowRssFeeds { get; }
        bool AllowUnsafeUpdates { get; set; }
        bool AllowAutomaticASPXPageIndexing { get; set; }
        Hashtable AllProperties { get; }
        IAveUserCollection AllUsers { get; }
        AveWebASPXPageIndexMode ASPXPageIndexMode { get; set; }
        IAveGroup AssociatedMemberGroup { get; set; }
        IAveGroup AssociatedOwnerGroup { get; set; }
        IAveUser Author { get; }//
        IAveAudit Audit { get; }
        IAveFieldCollection AvailableFields { get; }
        IAveContentTypeCollection AvailableContentTypes { get; }
        DateTime Created { get; set; }
        IAveContentTypeCollection ContentTypes { get; }
        string Description { get; set; }
        IAveFeatureCollection Features { get; }
        IAveFieldCollection Fields { get; }
        IAveWeb FirstUniqueRoleDefinitionWeb { get; }
        IAveGroupCollection Groups { get; }
        IAveGroupCollection SiteGroups { get; }
        IAveGroupCollection SiteGroupsWithUsers { get; }
        IAveUserCustomActionCollection UserCustomActions { get; }
        bool IsRootWeb { get; }
        uint Language { get; }
        DateTime LastItemModifiedDate { get; set; }
        DateTime LastItemUserModifiedDate { get; }
        IAveListCollection Lists { get; }
        IAveListCollection BrowserLists { get; }
        CultureInfo Locale { get; }
        string MasterUrl { get; set; }
        string CustomMasterUrl { get; set; }
        string Name { get; set; }
        IAveNavigation Navigation { get; }
        bool NoCrawl { get; set; }
        IAveUserCollection SiteUsers { get; }
        bool QuickLaunchEnabled { get; set; }

        [Obsolete("This is not actually a web property,has some logic inside,will be removed later")]
        bool RequestAccessEnable { get; set; }

        #region Access Request Setting
        bool MembersCanShare { get; set; }
        string AccessRequestSiteDescription { get; set; }
        bool UseAccessRequestDefault { get; set; }
        string RequestAccessEmail { get; set; }
        #endregion Access Request Setting

        IAveRegionalSettings RegionalSettings { get; }
        IAveRoleDefinitionCollection RoleDefinitions { get; }
        IAveFolder RootFolder { get; }
        string ServerRelativeUrl { get; }
        IAveSite Site { get; }
        bool SyndicationEnabled { get; set; }
        string Title { get; set; }
        bool TreeViewEnabled { get; set; }
        IAveWebCollection Webs { get; }
        string Url { get; }
        string WebTemplate { get; }
        int WebTemplateId { get; }//
        IAveWeb ParentWeb { get; }
        Guid ParentWebId { get; }
        bool ParserEnabled { get; set; }
        bool PresenceEnabled { get; set; }
        short Configuration { get; }
        bool HasUniqueRoleDefinitions { get; }
        string Theme { get; }
        bool UIVersionConfigurationEnabled { get; set; }
        string SiteLogoUrl { get; set; }
        string SiteLogoDescription { get; set; }
        int UIVersion { get; set; }
        CultureInfo UICulture { get; }
        IAveUserCollection Users { get; }
        bool IsMultilingual { get; set; }
        bool OverwriteTranslationsOnChange { get; set; }
        IAveUserCollection SiteAdministrators { get; }
        string ThemedCssFolderUrl { get; set; }
        #region Modern Look and Feel
        AveSPVariantThemeType HeaderEmphasis { get; set; }
        AveHeaderLayoutType HeaderLayout { get; set; }
        bool MegaMenuEnabled { get; set; }
        bool FooterEnabled { get; set; }
        #endregion
        AveWebAnonymousState AnonymousState { get; set; }
        bool ExcludeFromOfflineClient { get; set; }
        int SearchVersion { get; set; }
        IAveList SiteUserInfoList { get; }
        IAveListTemplateCollection ListTemplates { get; }
        IAveUser CurrentUser { get; }
        IAvePropertyBag Properties { get; }
        bool Exists { get; }
        IAveEventReceiverDefinitionCollection EventReceivers { get; }
        IAveCommonRequest Request { get; }
        bool IsPublish { get; }
        IAveGroup AssociatedVisitorGroup { get; set; }
        IEnumerable<int> SupportedUICultures { get; }
        CultureInfo LanguageCulture { get; }
        long Size { get; }
        IAveDocTemplateCollection DocTemplates { get; }
        IAveFileCollection Files { get; }
        IAveWorkflowAssociationCollection WorkflowAssociations { get; }
        string WebTemplateName { get; }
        int Count { get; }//Gets the count of sub sites beneath the website, including children of those websites.
        IAveWorkflowCollection Workflows { get; }
        Guid AppInstanceId { get; }
        IAveUserResource TitleResource { get; }
        IAveUserResource DescriptionResource { get; }
        Dictionary<int, AveBaseItemInfo> DiscussionTopicCache { get; }
        Dictionary<int, AveBaseItemInfo> DiscussionReplyCache { get; }

        string GetServerRelativeUrlFromUrl(string fullOrRelativeUrl, bool includeQueryString, bool canonicalizeUrl);
        void ApplyTheme(string theme);
        void ApplyTheme(string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated);
        void ApplyWebTemplate(string webTemplate, uint lcid);//webTeamplate  means  webTemplate.Title
        /// <summary>
        /// apply the web template with the template name.
        /// </summary>
        /// <param name="template"></param>
        void ApplyWebTemplate(IAveWebTemplate template);
        void Close();
        IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid, bool doIncludeCrossLanguage);
        IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid);
        IAveFolder GetFolder(string serverRelativeUrl);
        IAveFolder GetFolderFromCache(int rowId, string serverRelativeUrl);
        [Obsolete("Use IAveWeb.GetFolder(Guid uniqueId, int rowId, string serverRelativeUrl) Instead.")]
        IAveFolder GetFolder(Guid uniqueId);
        IAveFolder GetFolder(Guid uniqueId, int rowId, string serverRelativeUrl);//Add to support client API to get folder by uniqueId
        IAveFile GetFileByFullPath(string fullPath);
        IAveFile GetFile(string serverRelativeUrl);
        IAveFile GetFile(string serverRelativeUrl, bool needProperties);
        [Obsolete("Use IAveWeb.GetFile(Guid fileId, string serverRelativeUrl) Instead.")]
        IAveFile GetFile(Guid fileId);
        IAveFile GetFile(Guid fileId, string serverRelativeUrl);
        string GetFileAsString(string url);
        [Obsolete("Use IAveWeb.GetListItem(string itemFullUrl, Guid listId, Guid docId) Instead.")]
        IAveListItem GetListItem(string url);
        IAveListItem GetListItem(string itemFullUrl, Guid listId, Guid docId);
        IAveListItem GetListItem(string itemFullUrl, Guid listId, int rowId);
        IAveList GetList(string strUrl);
        IAveList GetList(Guid listId);
        IAveList GetListByTitle(string title);
        IAveList GetListFromUrl(string pageUrl);
        IAveLimitedWebPartManager GetLimitedWebPartManager(string fullOrRelativeUrl, AvePersonalizationScope scope);
        object GetObject(string strUrl);
        IAveWebPartCollection GetWebPartCollection(string fullOrRelativeUrl, AveStorage storage);
        Dictionary<Guid, Dictionary<Guid, Guid>> GetWebAlerts();
        IAveUser EnsureUser(string loginName);
        string GetDomainGroupLoginName(string groupName);
        void Update();
        void Delete();
        IAveList GetCatalog(AveListTemplateType typeCatalog);
        IAveList GetListByName(string strListName, bool bThrowException);

        IAveNavigationSerializer NavigationSerializer { get; }
        IAveRolesSerializer RolesSerializer { get; }
        IAveWebSerializer WebSerializer { get; }
        IAveWebSettingSerializer WebSettingSerializer { get; }
        IAveUsersSerializer WebUsersSerializer { get; }
        IAveGroupsSerializer GroupsSerializer { get; }
        IAveRoleAssignmentsSerializer RoleAssignmentsSerializer { get; }
        IAveFeatureSerializer FeatureSerializer { get; }

        void RevertAllDocumentContentStreams();
        IAveView GetViewFromUrl(string listUrl);
        IAveFieldTypeDefinitionCollection FieldTypeDefinitionCollection { get; }

        Guid ID { get; }
        void FakeSPContext();
        void SetSPContextNull();
        IAveViewStyleCollection ViewStyles { get; }
        void InvalidateRequest();
        void InitializeSPRequest();
        IAveFile GetCheckoutFile(string url);
        void RestoreTheme(AveWebSettingInfo webSettingInfo, string themedCssFolderUrl);
        void AddSupportedUICulture(List<int> lcids);
        IAveListCollection GetListsOfType(AveBaseType baseType);
        IAveWorkflowTemplateCollection WorkflowTemplates { get; }
        void CreateDefaultAssociatedGroups(string userLogin, string userLogin2, string groupNameSeed);
        /// <summary>
        /// 调用该reload方法时，如果当前进程中有通过该web取出来的对象，比如list,需要list该对象也进行reload，否则可能导致对象不一致问题。
        /// </summary>
        void ReloadWeb();
        List<Guid> StopListAlerts(IAveList list);
        DateTime GetLastAccessedDayOfWeb(Guid siteId, Guid webId);
        IAveFeatureDefinitionCollection GetAllFeatureDefinitions();
        void RestoreMasterPage(AveWebMasterPageInfo pageInfo, string alternateCssUrl);
        string ProcessBatchData(string strBatchData);
        void AddProperty(object key, object value);
        void SetFormForList(int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId);
        /// <summary>
        /// 获取web下所有List Content Type Id
        /// </summary>
        /// <returns></returns>
        List<IAveContentTypeId> GetAllListContentTypeIds();
        IList<IAveAppInstance> GetAppInstancesByProductId(Guid productId);
        IAveAppInstance GetAppInstanceById(Guid appInstanceId);
        IAveAppInstance LoadAndInstallApp(string webServerRelativeUrl, Stream stream);
        /// <summary>
        /// 只给Records使用。
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="restoreMode"></param>
        /// <returns></returns>
        IAveAppInstance DeployApp(Guid productId, AveRestoreMode restoreMode);

        /// <summary>
        /// 调用该方法时，请判断SharePoint版本在2010 sp1及其以上，否则会抛异常。
        /// </summary>
        void Recycle();
        /// <summary>
        /// Include configuration, e.g. STS#0
        /// </summary>
        string Template { get; }
        bool IsAppWeb { get; }
        IAveFeatureDefinitionCollection FeatureDefinitions { get; }
        //To operate Change Log
        IAveChangeCollection GetChanges();
        IAveChangeCollection GetChanges(IAveChangeQuery query);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd);

        void UpgradeAppByProductId(Guid productId);

        void RecalculateForCommunitySite(IAveList discussionsList, Dictionary<int, int> itemIdmapping);

        /// <summary>
        /// add this method to load the title resource of list with special culture to improve the performance
        /// </summary>
        /// <param name="cultureName"></param>
        void LoadListTitleResource(string cultureName);

        void PublishNintexWorkflow(string workflowId, string workflowRestrictToScope);

        List<Guid> GetListsIdContainItemsWithUniquePermissions();

        bool GetAccessRequestApprover();

        void SetAccessRequestApprover(bool defaultApprover, string email);

        object GetClientContext();

        Dictionary<string, object> GetListItemSharingInformation(Guid listid, int itemID, bool excludeCurrentUser = true, bool excludeSiteAdmin = false, bool excludeSecurityGroups = true, bool retrieveAnonymousLinks = true, bool retrieveUserInfoDetails = false, bool checkForAccessRequests = false);

        string GetTenantAppCatalogSite();
        void DisableAlert(string webServerRelativeUrl, List<Guid> disableAlertIds);
        void EnableAlert(string webServerRelativeUrl, List<Guid> disableAlertIds);

        public Dictionary<string, (Guid UniqueId, Guid ListId)> GetStubNodesByBatchPath(List<string> serverRelativeUrls);
    }
}
