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
        string AlternateCssUrl { get; set; }
        string TaxonomyList { get; }
        IAveAlertCollection Alerts { get; }
        bool AllowRssFeeds { get; }
        bool AllowUnsafeUpdates { get; set; }
        bool AllowAutomaticASPXPageIndexing { get; set; }
        Hashtable AllProperties { get; }
        IAveUserCollection AllUsers { get; }
        AveWebASPXPageIndexMode ASPXPageIndexMode { get; set; }
        IList<IAveGroup> AssociatedGroups { get; }
        IAveGroup AssociatedMemberGroup { get; set; }
        IAveGroup AssociatedOwnerGroup { get; set; }
        IAveUser Author { get; set; }
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
        bool IsRootWeb { get; }
        uint Language { get; }
        int WorkingLanguage { get; }
        DateTime LastItemModifiedDate { get; set; }
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
        #region Access Requset setting, most for o365
        bool MembersCanShare { get; set; }
        string AccessRequestSiteDescription { get; set; }
        bool UseAccessRequestDefault { get; set; }
        string RequestAccessEmail { get; set; }
		#endregion
        IAveRegionalSettings RegionalSettings { get; }
        IAveRoleDefinitionCollection RoleDefinitions { get; }
        IAveFolder RootFolder { get; }
        string ServerRelativeUrl { get; set; }
        IAveSite Site { get; }
        bool SyndicationEnabled { get; set; }
        string Title { get; set; }
        bool TreeViewEnabled { get; set; }
        IAveWebCollection Webs { get; }
        string Url { get; }
        string WebTemplate { get; }
        int WebTemplateId { get; }//
        /// <summary>
        /// Include configuration, e.g. STS#0
        /// </summary>
        string Template { get; }
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
        AveWebAnonymousState AnonymousState { get; set; }
        bool ExcludeFromOfflineClient { get; set; }
        IAveList SiteUserInfoList { get; }
        IAveListTemplateCollection ListTemplates { get; }
        IAveUser CurrentUser { get; }
        IAvePropertyBag Properties { get; }
        bool Exists { get; }
        IAveEventReceiverDefinitionCollection EventReceivers { get; }
        IAveCommonRequest Request { get; }
        bool IsPublish { get; }
        IAveGroup AssociatedVisitorGroup { get; set; }
        IEnumerable<CultureInfo> SupportedUICultures { get; }
        CultureInfo LanguageCulture { get; }
        long Size { get; }
        IAveDocTemplateCollection DocTemplates { get; }
        IAveFileCollection Files { get; }
        IAveWorkflowAssociationCollection WorkflowAssociations { get; }
        string WebTemplateName { get; }
        int Count { get; }//Gets the count of sub sites beneath the website, including children of those websites.
        IAveWorkflowCollection Workflows { get; }
        IAveRecycleBinItemCollection RecycleBin { get; }

        // only use in online 365
        bool HaveAddAndCustomizePagesPermission { get; }

        string GetServerRelativeUrlFromUrl(string fullOrRelativeUrl, bool includeQueryString, bool canonicalizeUrl);
        void ApplyTheme(string theme);
        void ApplyWebTemplate(IAveWebTemplate webTemplate);
        bool Provisioned { get; }
        void Close();
        IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid, bool doIncludeCrossLanguage);
        IAveWebTemplateCollection GetAvailableWebTemplates(uint lcid);
        IAveFolder GetFolder(string serverRelativeUrl);

        /// <summary>
        /// 只有真实365 site与local支持该方法，模拟365会throw NotSupportException 需要注意
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        IAveFolder GetFolder(Guid uniqueId);
        IAveFolder GetFolder(Guid uniqueId, int rowId, string serverRelativeUrl);//Add to support client API to get folder by uniqueId
        IAveFile GetFile(string serverRelativeUrl);
        IAveFile GetFile(string serverRelativeUrl, bool needProperties);
        [Obsolete("Use IAveWeb.GetFile(Guid fileId, string serverRelativeUrl) Instead.")]
        IAveFile GetFile(Guid uniqueId);
        IAveFile GetFile(Guid fileId, string serverRelativeUrl);
        string GetFileAsString(string url);
        [Obsolete("Use IAveWeb.GetListItem(string itemFullUrl, Guid listId, Guid docId) Instead.")]
        IAveListItem GetListItem(string url);
        IAveListItem GetListItem(string itemFullUrl, Guid listId, Guid docId);
        IAveList GetList(string strUrl);
        IAveList GetList(Guid listId);
        IAveList GetListFromUrl(string pageUrl);
        IAveLimitedWebPartManager GetLimitedWebPartManager(string fullOrRelativeUrl, AvePersonalizationScope scope);
        object GetObject(string strUrl);
        IAveWebPartCollection GetWebPartCollection(string fullOrRelativeUrl, AveStorage storage);
        Dictionary<Guid, Dictionary<Guid, Guid>> GetWebAlerts();
        IAveUser EnsureUser(string logonName);
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
        [Obsolete("Use IAveWeb. HandleSPContext(Action code) Instead.")]
        void FakeSPContext();
        [Obsolete("Use IAveWeb.HandleSPContext(Action code, bool isPost) Instead.")]
        void FakeSPContext(bool isPost);
        /// <summary>
        /// Fake SPContext, then empty SPContext
        /// </summary>
        void HandleSPContext(Action code);
        /// <summary>
        /// Fake SPContext, then empty SPContext
        /// </summary>
        void HandleSPContext(Action code, bool isPost);
        [Obsolete("The method has been included in IAveWeb. HandleSPContext(Action code) and IAveWeb.HandleSPContext(Action code, bool isPost).")]
        void SetSPContextNull();
        IAveViewStyleCollection ViewStyles { get; }
        Guid VariationLabelListId { get; }
        Guid RelationshipsListId { get; }
        void InvalidateRequest();
        void InitializeSPRequest();
        IAveFile GetCheckoutFile(string url);
        void RestoreTheme(AveWebSettingInfo webSettingInfo, string themedCssFolderUrl);
        void AddSupportedUICulture(CultureInfo cultureInfo);
        IAveListCollection GetListsOfType(AveBaseType baseType);
        IAveWorkflowTemplateCollection WorkflowTemplates { get; }
        void CreateDefaultAssociatedGroups(string userLogin, string userLogin2, string groupNameSeed);
        /// <summary>
        /// 调用该reload方法时，如果当前进程中有通过该web取出来的对象，比如list,需要list该对象也进行reload，否则可能导致对象不一致问题。
        /// </summary>
        void ReloadWeb();
        void ReloadFeatures();
        /// <summary>
        /// 调用该方法时，请判断SharePoint版本在2010 sp1及其以上，否则会抛异常。
        /// </summary>
        void Recycle();
        List<Guid> StopListAlerts(IAveList list);
        DateTime GetLastAccessedDayOfWeb(Guid siteId, Guid webId);
        IAveFeatureDefinitionCollection GetAllFeatureDefinitions();
        void RestoreMasterPage(AveWebMasterPageInfo pageInfo, string alternateCssUrl);
        string ProcessBatchData(string strBatchData);
        string GetFormula(string webUrl, string listId, string newFormula, string oldFormula);
        void AddProperty(object key, object value);
        string GetWebRelativeUrlFromUrl(string strUrl);
        #region add for SP2013
        int SearchVersion { get; set; }
        IAveFeatureDefinitionCollection FeatureDefinitions { get; }
        void ApplyTheme(string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated);
        void EnableDisableAbuseReports(bool bEnable);
        bool HideSiteContentsLink { get; set; }
        #endregion

        #region add for sp app
        bool IsAppWeb { get; }
        Guid AppInstanceId { get; }
        IAveAppInstance LoadAndInstallApp(Stream appPackageStream);
        IAveAppInstance LoadAndInstallApp(Stream appPackageStream, int appSource, string assetId, string contentMarket);
        IAveAppInstance GetAppInstanceById(Guid appInstanceId);
        IList<IAveAppInstance> GetAppInstancesByProductId(Guid productId);
        void UpgradeAppByProductId(Guid productId);
        IAveAppSerializer AppSerializer { get; }
        #endregion

        System.Data.DataTable GetSiteData(IAveSiteDataQuery siteDataQuery);

        //To operate Change Log
        IAveChangeCollection GetChanges();
        IAveChangeCollection GetChanges(IAveChangeQuery query);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd);

        #region User Resource
        /// <summary>
        /// only support server 10,13 mode. server 07, client mode will return null.
        /// </summary>
        IAveUserResource DescriptionResource { get; }
        /// <summary>
        /// only support server 10,13 mode. server 07, client mode will return null.
        /// </summary>
        IAveUserResource TitleResource { get; }
        #endregion

        IAvePublishingWeb GetPublishingWeb { get; }

        Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string listName, Guid listId);

        Guid PublishNintexWorkflow(Guid workflowDefinitionId);

        string ImportNintexWorkflow(System.IO.Stream stream, string publishName, string listTitle, Guid listId, bool overWrite);

        string ConvertNintexFormJsonObjectToXml(string formJsonData, string fileName);

        IAveUserCustomActionCollection UserCustomActions { get; }
        /// <summary>
        /// 只给Records使用。
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="restoreMode"></param>
        /// <returns></returns>
        IAveAppInstance DeployApp(Guid productId, Wrapper.Restore.AveRestoreMode restoreMode);
    }
}
