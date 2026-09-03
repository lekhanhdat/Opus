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
using System.Collections.Generic;
using System.Text;
using System.Net;
using AvePoint.GCommon;
using System.Reflection;
using System.IO;
using System.Security;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Common
{
    //make it abstract factory to avoid performance issue caused by reflection
    public abstract class AveObjectModelFactory
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal const string ServerAssemblyName = "SP2010WrapperServer";
        internal const string ServerNameSpace = "AvePoint.ObjectModel.Server.";
        internal const string ClientAssemblyName = "AgentCommonObjectModelCommon";
        internal const string ClientNameSpace = "AvePoint.ObjectModel.Common.";
        internal const string Server07AssmeblyName = "SP2007WrapperServer";
        internal const string Server07NameSpace = "AvePoint.ObjectModel.Server07.";

        protected AveObjectModelFactory()
        {
        }

        public static AveObjectModelFactory CreateObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo, AveContextKind contextKind)
        {
            try
            {
                AveContextKind lContextKind = DecideUseWhichModel(contextKind, siteUrl);
                AveObjectModelFactory omFactory = null;
                switch (lContextKind)
                {
                    case AveContextKind.ServerObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(ServerAssemblyName, ServerNameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.ClientObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(ClientAssemblyName, ClientNameSpace + "AveClientObjectModelFactory", new Type[] { typeof(string), typeof(AveBPOSAccountInfo) }, new object[] { siteUrl, accountInfo }) as AveObjectModelFactory;
                        WrapperRuntime.CurrentContext.ModelFactory = omFactory;
                        break;
                    case AveContextKind.Server07ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server07AssmeblyName, Server07NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    default:
                        break;
                }
                return omFactory;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCGetObjectError, e.ToString());
                return null;
            }
        }

        public static AveObjectModelFactory CreateObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo)
        {
            return CreateObjectModelFactory(siteUrl, accountInfo, AveContextKind.Auto);
        }

        public static AveObjectModelFactory CreateObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo, AveDiscoveryKind kind)
        {
            try
            {
                AveObjectModelFactory omFactory = null;
                switch (kind)
                {
                    case AveDiscoveryKind.Database:
                        omFactory = CreateObjectModelFactory(siteUrl, accountInfo, AveContextKind.Auto);
                        break;
                    case AveDiscoveryKind.API:
                        omFactory = AveAssemblyUtility.CreateInstance(ClientAssemblyName, ClientNameSpace + "AveClientObjectModelFactory", new Type[] { typeof(string), typeof(AveBPOSAccountInfo) }, new object[] { siteUrl, accountInfo }) as AveObjectModelFactory;
                        break;
                    default:
                        break;
                }
                return omFactory;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetObjectError, e.ToString());
                return null;
            }
        }

        public abstract AveContextKind ContextKind
        {
            get;
        }

        public abstract bool IsSPInstalled
        {
            get;
        }

        //don't have to create mutiple instances
        public abstract IAveUtility Utility
        {
            get;
        }

        public abstract AveBPOSAccountInfo AccountInfo
        {
            get;
        }

        public abstract AveAPIType APIType
        {
            get;
        }

        private static AveContextKind DecideUseWhichModel(AveContextKind contextKind, string siteUrl)
        {
            if (contextKind == AveContextKind.ClientObjectModel || (!string.IsNullOrEmpty(siteUrl) && !AveUrlUtility.IsUrlFull(siteUrl)))
            {
                return AveContextKind.ClientObjectModel;
            }
            contextKind = contextKind == AveContextKind.Auto ? AveContextKind.ClientObjectModel : contextKind;
            if (contextKind == AveContextKind.ClientObjectModel && AveEnvironment.IsSPInstalled(siteUrl))
            {
                AveSPEnv.IsServerMode = true;
                if (SPSVersionDetector.IsMOSS2010Installed())
                {
                    return AveContextKind.ServerObjectModel;
                }
                else
                {
                    return AveContextKind.Server07ObjectModel;
                }
            }
            return contextKind;
        }

        public abstract IAveSite CreateSite();

        public abstract IAveSite CreateSite(string url);

        public abstract IAveSite CreateSite(Guid siteId);

        public abstract IAveSite CreateSite(string url, IAveUserToken token);

        public abstract IAveSite CreateSite(Guid id, AveUrlZone zone);

        public abstract IAveSite CreateAdminCenterSite(string url);

        public abstract IAveTenant CreateTenant(IAveSite site);

        public abstract IAveTenant CreateTenant(string url);

        public abstract IAveTenant GetTenant(string url);

        public abstract IAveWebApplication CreateWebApplication();

        public abstract IAveRoleDefinitionBindingCollection CreateRoleDefinitionBindingCollection();

        public abstract IAveRoleAssignment CreateRoleAssignment(IAvePrincipal principal);

        public abstract IAveQuery CreateQuery();

        public abstract IAveContentTypeIdPub CreateContentTypePub();

        public abstract IAvePublishingWeb CreatePublishingWeb(IAveWeb web);

        public abstract IAvePublishingWeb CreatePublishingWeb();

        public abstract IAvePublishingSite CreatePublishingSite();

        public abstract IAveFieldUrlValue CreateFieldUrlValue();

        public abstract IAveFieldUrlValue CreateFieldUrlValue(string fieldValue);

        public abstract IAveFieldLink CreateFieldLink(IAveField field);

        public abstract IAveContentTypeId CreateContentTypeId(string id);

        public abstract IAveContentTypeId CreateContentTypeId();

        public abstract IAveContentType CreateContentType();

        public abstract IAveContentType CreateContentType(IAveContentTypeId contentTypeId);

        public abstract IAveContentType CreateContentType(IAveContentTypeId parentContentType, IAveContentTypeCollection collection, string name);

        public abstract IAveContentType CreateContentType(IAveContentType parentContentType, IAveContentTypeCollection collection, string name);

        public abstract IAveRegionalSettings CreateRegionalSettings(IAveWeb web, bool bIsUserRegionalSetting);

        public abstract IAveRegionalSettings CreateRegionalSettings();

        public abstract IAveRoleDefinition CreateRoleDefinition();

        public abstract IAveNavigationNode CreateNavigationNode(string title, string url, bool isExternal);

        public abstract IAveNavigationNode CreateNavigationNode(string title, string url);

        public abstract IAveTaxonomyFieldValue CreateTaxonomyFieldValue(IAveField field);

        public abstract IAveTaxonomyFieldValue CreateTaxonomyFieldValue(string value);

        public abstract IAveTaxonomyFieldValueCollection CreateTaxonomyFieldValueCollection(IAveField field);

        public abstract IAveFieldUserValueCollection CreateFieldUserValueCollection();

        public abstract IAveFieldUserValue CreateFieldUserValue(IAveWeb web, int lookupId, string lookupValue);

        public abstract IAveFieldLookupValueCollection CreateFieldLookupValueCollection();

        public abstract IAveFieldLookupValue CreateFieldLookupValue(int lookupId, string lookupValue);

        public abstract IAveUserToken CreateUserToken(byte[] token);

        public abstract IAveWebApplication CreateWebApplication(string url);

        public abstract IAveItem CreateAveItem(IAveWeb web, IAveList list);

        public abstract IAveFarm CreateFarm();

        public abstract IAveSecurity CreateSecurity();

        public abstract IAveWebService CreateWebService();

        public abstract IAveWebService CreateWebService(string name, IAveFarm farm);

        public abstract IAveUserCodeService CreateUserCodeService();

        public abstract IAveUserCodeService CreateUserCodeService(IAveFarm farm);

        public abstract IAveRoleAssignment CreateRoleAssignment(IAveUser user);

        public abstract IAveAdministrationWebApplication CreateAdministrationWebApplication();

        public abstract IAveItemEventReceiver CreateItemEventReceiver();

        public abstract IAveAlternateUrlCollection CreatedAlternateUrlCollection(string name, IAveFarm local);

        public abstract IAveAlternateUrl CreateAlternateUrl(string incomingUrl, AveUrlZone urlZone);

        public abstract IAveAlternateUrl CreateAlternateUrl(Uri requestUri, AveUrlZone urlZone);

        public abstract IAveMetadataListFieldSettings CreateMetadataListFieldSettings(IAveList list);

        public abstract IAveTaxonomySession CreateTaxonomySession(IAveSite site);

        public abstract IAveTaxonomySession CreateTaxonomySession();

        public abstract IAveOMetadataNavigationSettings CreateMetadataNavigationSettings();

        public abstract IAveOMetadataNavigationSettings CreateMetadataNavigationSettings(string xmlMetadataNavigationSettings);

        public abstract IAveOFieldIndexDictionary CreateFieldIndexDictionary();

        public abstract IAveDateOptions CreateDateOptions(string localeId, AveCalendarType calendar, string workWeek, string firstDayOfWeek, string hijriAdjustment, string timeZoneSpan, string selectedDate);

        public abstract IAveOPolicyCatalog CreatePolicyCatalog(IAveSite site);

        public abstract IAveOSearchService CreateOSearchService();

        public abstract IAveOConfiguredView CreateConfiguredView(IAveView view, int index);

        public abstract IAveONodeViewSettings CreateNodeViewSettings(IAveOViewSettingsCollection viewSettingsCollection, string uniqueNodeId, int folderId);

        public abstract IAveRatingsSettingsPage CreateRatingsSettingsPage();

        public abstract IAveMobileUtility CreateMobileUtility();

        public abstract IAveAlertTemplateCollection CreateAlertTemplateCollection(IAveWebService wssService);

        public abstract IAveResource CreateResource();

        public abstract IAveOmsMobileFacade CreateOmsMobileFacade();

        public abstract IAveCredentialDeployment CreateCredentialDeployment();

        public abstract IAveGlobalAdmin CreateGlobalAdmin();

        public abstract IAveDatabaseService CreateDatabaseService();

        public abstract IAveOMetadataNavigationHierarchy CreateMetadataNavigationHierarchy();

        public abstract IAveOMetadataNavigationHierarchy CreateMetadataNavigationHierarchy(IAveField hierarchyField);

        public abstract IAveOMetadataNavigationKeyFilter CreateMetadataNavigationKeyFilter();

        public abstract IAveOMetadataNavigationKeyFilter CreateMetadataNavigationKeyFilter(IAveField hierarchyFiled);

        public abstract IAveOMetadataHierarchyNodeTaxonomy CreateMetadataHierarchyNodeTaxonomy();

        public abstract IAveOMetadataDefaults CreateMetadataDefaults(IAveList aveList);
        public abstract IAveOMetadataDefaults CreateMetadataDefaults(IAveSite aveSite, string columnName);

        //public abstract IAveClaimProviderOperations CreateClaimProviderOperations();

        public abstract IAveServer CreateServer(string address);

        public abstract IAveServer CreateServer(string address, IAveFarm farm);

        public abstract IAveDatabaseServiceInstance CreateDatabaseServiceInstance(string name, IAveServer server, IAveDatabaseService service);

        public abstract IAveAlternateUrlCollection CreateAlternateUrlCollection(string resourceName, IAveFarm farm);

        public abstract IAvePortalService CreatePortalService(string name, IAveFarm farm);

        public abstract IAveFarmManagedAccountCollection CreateFarmManagedAccountCollection(IAveFarm farm);

        public abstract IAveOfficialFileHost CreateOfficialFileHost(bool bCreateUniqueId);

        public abstract IAveSecurityTokenServiceManager CreateSecurityTokenServiceManager();

        /// <summary>
        /// Constructor Method for Webs in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public abstract IAveWebCollection CreateWebs(ICredentials crendeantial, string url);

        /// <summary>
        /// Constructor Method for Lists in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public abstract IAveListCollection CreateLists(ICredentials crendeantial, string url);

        /// <summary>
        /// Constructor Method for Views in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public abstract IAveViewCollection CreateViews(ICredentials crendeantial, string url);

        public abstract IAveUpgradeSessionCollection CreateUpgradeSessionCollection(IAveFarm farm);

        public abstract IAvePersistedDependencyCollection<IAveIisWebServiceApplication> CreatePersistedDependencyCollection(IAveIisWebServiceApplicationPool iisWebServiceApplicationPool);

        public abstract IAveServiceApplicationProxyGroup CreateServiceApplicationProxyGroup();

        public abstract IAveWebApplicationProvisioningJobDefinition CreateWebApplicationProvisioningJobDefinition(IAveWebApplication app);

        public abstract IAveIisWebsiteUnprovisioningJobDefinition CreateIisWebsiteUnprovisioningJobDefinition(bool deleteWebSites, string[] serverComments, string applicationPoolId, string[] vdirs, Guid webAppId, bool webAppUnprovisioning);

        public abstract IAveWebServiceCollection CreateWebServiceCollection(IAveFarm farm);

        public abstract IAveWebApplicationBuilder CreateWebApplicationBuilder(IAveFarm farm);

        public abstract IAveIisSettings CreateIisSettings(string serverComment, bool allowAnonymous, bool disableKerberos, IAveServerBinding serverBinding, IAveSecureBinding secureBinding, DirectoryInfo path);

        /// <summary>
        /// Invoke object for calling static member;
        /// </summary>
        /// <returns></returns>
        public abstract IAveWebTemplate CreateWebTemplate();

        public abstract IAveIisSettings CreateIisSettings();

        public abstract IAveIisWebSite CreateIisWebSite(int instanceId);

        public abstract IAveIisWebSite CreateIisWebSite();

        public abstract IAveIisApplicationPool CreateIisApplicationPool(string name);

        public abstract IAveServiceProxy CreateServiceProxy(string name, IAveFarm farm);

        public abstract IAveMobileMessagingAccount CreateMobileMessagingAccount();

        public abstract IAveMobileMessagingAccount CreateMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password);

        public abstract IAveMobileMessagingAccount CreateMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password, IAveMobileMessageServiceProvider serviceProvider, IAveMobileMessageUserInfo userInfo);

        public abstract IAveServiceApplicationProxyGroupCollection CreateServiceApplicationProxyGroupCollection(IAveFarm farm);

        public abstract IAveDiagnosticsService CreateDiagnosticsService();

        public abstract IAveSiteCollectionCopier CreateSiteCollectionCopier(IAveContentDatabase dbFrom, IAveContentDatabase dbTo, List<IAveSite> colSites);

        public abstract IAveQueryProvider CreateQueryProvider(Uri helpList, uint lcid);

        public abstract IAveHelpContextManager CreateHelpContextManager();

        public abstract IAveDeliveryChannelSettings CreateDeliveryChannelSettings();

        public abstract IAveSQM CreateSQM();

        public abstract IAveThmxTheme CreateThmxTheme(IAveSite site);

        public abstract IAveClaimEncodingManager CreateClaimEncodingManager();

        public abstract IAveClaimEntityTypes CreateClaimEntityTypes();

        public abstract IAveClaimProviderManager CreateClaimProviderManager();

        public abstract IAveOneTimeSchedule CreateOneTimeSchedule(DateTime dt);

        public abstract IAveSchedule CreateSchedule();

        public abstract IAveOfficialFileSoap CreateOfficialFileSoap(Uri uri);

        public abstract IAveOfficialFileSoap CreateOfficialFileSoap(string url);

        public abstract IAveSecureBinding CreateSecureBinding();

        public abstract IAveServerBinding CreateServerBinding();

        public abstract IAveWindowsAuthenticationProvider CreateWindowsAuthenticationProvider();

        public abstract IAveFormsAuthenticationProvider CreateFormsAuthenticationProvider(string membershipProvider, string roleProvider);

        public abstract IAveTrustedAuthenticationProvider CreateTrustedAuthenticationProvider(string providerName);

        public abstract IAveQuotaTemplate CreateQuotaTemplate();

        public abstract IAveWorkflowManager CreateWorkflowManager();

        public abstract IAveListViewWebPart CreateListViewWebPart();

        public abstract IAveServiceContext CreateServiceContext();

        public abstract IAveChangeQuery CreateChangeQuery(bool allChangeObjectTypes, bool allChangeTypes);

        public abstract IAveChangeToken CreateChangeToken(AveCollectionScope scope, Guid scopeId, DateTime changeTime);

        public abstract IAveChangeToken CreateChangeToken(string strChangeToken);

        public abstract IAveSiteSubscriptionIdentifier CreateSiteSubscriptionIdentifier();

        public abstract IAveOUserProfileManager CreateUserProfileManager(IAveServiceContext context);

        public abstract IAveOUserProfileManager CreateUserProfileManager(IAveServiceApplication application);

        public abstract IAveOSocialTagManager CreateSocialTagManager(IAveServiceContext context);

        public abstract IAveOAlternateAccessMapping CreateOAlternateAccessMapping();

        public abstract IAveOUserProfileApplicationProxy CreateOUserProfileApplicationProxy();

        public abstract IAveOULS CreateOULF();

        public abstract IAveOSocialCommentManager CreateSocialCommentManager(IAveServiceContext context);

        public abstract IAveSecureString CreateSecurityString();

        public abstract IAveSecurityContext CreateSecurityContext(IntPtr priorToken);

        public abstract IAveORecords CreateRecords();

        public abstract IAveOFormsService CreateFormsService();

        public abstract IAveBlockedSolution CreateBlockedSolution(string fileName, string signature, string message);

        public abstract IAveDatabaseSequence CreateDatabaseSequence();

        public abstract IAveOFormTemplateCollection CreateFormTemplateCollection();

        public abstract IAveIisSmtpServer CreateIisSmtpServer();

        public abstract IAveMonthlySchedule CreateMonthlySchedule();

        public abstract IAveMonthlyByDaySchedule CreateMonthlyByDaySchedule();

        public abstract IAveProductVersions CreateProductVersions();

        public abstract IAveServiceInstanceJobDefinition CreateServiceInstanceJobDefinition(IAveServiceInstance serviceInstance, bool provision);

        public abstract IAveProcessAccount CreateProcessAccount();

        public abstract IAveSkuUpgradeJob CreateSkuUpgradeJob();

        public abstract IAveSkuUpgradeJob CreateSkuUpgradeJob(string name, IAveService service);

        public abstract IAveSkuUpgradePage CreateSkuUpgradePage();

        public abstract IAveOSetupLicensing CreateSetupLicensing();

        public abstract IAveSecurityContext CreateSecurityContext();

        public abstract IAveServer CreateServer();

        public abstract IAveSmtpSettingsPushJobDefinition CreateSmtpSettingsPushJobDefinition(string name, IAveService service);

        public abstract IAveTrustedRootAuthorityManager CreateTrustedRootAuthorityManager();

        public abstract IAveWeeklySchedule CreateWeeklySchedule();

        public abstract IAveStringResourceManager CreateStringResourceManager();

        public abstract IAveOPolicyCatalog CreatePolicyCatalog();

        public abstract IAveSolutionDeploymentJobDefinition CreateSolutionDeploymentJobDefinition();

        public abstract IAveWorkflowAssociation CreateWorkflowAssociation();

        public abstract IAveWorkflowDefinition CreateWorkflowDefinition();

        public abstract IAveWorkflowSubscription CreateWorkflowSubscription();

        public abstract IAveSolution CreateSolution();

        public abstract IAveFieldId CreateFieldId();

        public abstract IAveOULS CreateULS();

        public abstract IAveOKeywords CreateKeywords(IAveOSearchServiceApplicationProxy searchAdminProxy, Uri url);

        public abstract IAveScheduledItem CreateScheduledItem();

        public abstract IAveLegacyListTemplate CreateLegacyListTemplate();

        public abstract IAveNavigationSiteMapNode CreateNavigationSiteMapNode();

        public abstract IAveSite CreateSite(Guid siteId, IAveUserToken userToken);

        public abstract IAveOAudienceManager CreateAudienceManager(IAveServiceContext serviceContext);

        public abstract IAveAuditQuery CreateAuditQuery(IAveSite site);

        public abstract IAveSiteAdministration CreateSiteAdministration(string url);

        public abstract IAveORemoteScopes CreateRemoteScopes(IAveServiceContext context);

        public abstract IAveOScopeInfo CreateScopeInfo();

        public abstract IAveOManagedPropertyInfo CreateManagedPropertyInfo();

        public abstract IAveORuleInfo CreateRuleInfo();

        public abstract IAveODisplayGroupInfo CreateDisplayGroupInfo();

        public abstract IAvePeopleEditor CreatePeopleEditor();

        public abstract IAveOScopesUtilities CreateScopesUtilities();

        public abstract IAveOKeywordHelper CreateKeywordHelper(string siteId);

        public abstract IAveOKeywordHelper CreateKeywordHelper(string siteId, IAveServiceContext serviceContext);

        public abstract IAveOUserContextHelper CreateUserContextHelper(string siteID);

        public abstract IAveOUserProfilePropertyHelper CreateUserProfilePropertyHelper();

        public abstract IAveListItemCollectionPosition CreateListItemCollectionPosition(string pageInfo);

        public abstract IAveOBestBetHelper CreateBestBetHelper(string siteID, IAveServiceContext serviceContext);

        public abstract IAveOFeaturedContentHelper CreateFeaturedContentHelper(string siteID, IAveServiceContext serviceContext);

        public abstract IAveORankPromotionHelper CreateRankPromotionHelper(string siteID, IAveServiceContext serviceContext);

        public abstract IAveContentDatabase CreateContentDatabase();

        public abstract IAveODocIdUiSettings CreateDocIdUiSettings();

        public abstract IAveODocIdUiSettings CreateDocIdUiSettings(bool assignmentEnabled, string prefix);

        public abstract IAveODocIdLookup CreateDocIdLookup();

        public abstract IAveOOobProvider CreateOobProvider();

        public abstract IAveCommonUtilities CreateCommonUtilities();

        public abstract IAveODocumentId CreateDocumentId();

        public abstract IAveItem CreateAveItem(AveBaseItemInfo info, IAveFolder folder, IAveWeb web, IAveList list);

        public abstract IAveAttachment CreateAttachment(AveAttachmentInfo info, IAveListItem item);

        public abstract IAveConfigurationDatabase CreateConfigurationDatabase();

        public abstract IAveItem CreateAveItem(IAveSite iAveSite);

        public abstract IAveWebTemplateCollection CreateWebTemplateCollection(string xmlWebTemplates, uint LCID);

        public abstract IAveCertificateValidator CreateCertificateValidator();

        public abstract IAveLimitedWebPartManager CreateLimitedWebPartManager(IAveSite site, IAveWeb web, IAveFile file);

        public abstract IAveLimitedWebPartManager CreateLimitedWebPartManager(IAveSite site, IAveWeb web, string fileServerRelativeUrl);

        public abstract IAveSiteSubscriptionSettings CreateSiteSubscriptionSettings();

        public abstract IAveExportSettings CreateExportSettings(Uri url, string tempFileFolder, string tempFileName);

        public abstract IAveExportObject CreateExportObject(Guid objId, AveDeploymentObjectType objType, Guid parentObjId, bool excludeChildren);

        public abstract IAveExport CreateExport(IAveExportSettings exportSettings);

        public abstract IAvePublishing CreatePublishing(IAveSite site);

        public abstract IAveFieldLookupValue CreateFieldLookupValue();

        public abstract IEcmDocumentRouting EcmDocumentRouting();

        public abstract IAvePersistedTypeCollection<T> CreatePersistedTypeCollection<T>(IAveFarm farm) where T : IAvePersistedObject;

        public abstract IAveSolutionLanguagePack CreateSoluctionLanguagePack();

        public abstract IAveMetaDataServiceSerializer CreateMetadataServiceSerilizer(Guid serviceAppId);

        public abstract IAveMetadataServiceRestorer CreateMetadataServiceRestorer(Guid serviceAppId);

        public abstract IAveMetadataServiceApplication CreateMetadataServiceApplication(string name);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount, DateTime startTime, DateTime endTime);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DiscoverModule module);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DateTime startTime, DateTime endTime, DiscoverModule module);

        public abstract IAveContentTypePublisher CreateContentTypePublisher();

        public abstract IAveContentTypePublisher CreateContentTypePublisher(IAveSite site);

        public abstract IAveContentTypePublisher CreateContentTypePublisher(IAveTermStore store);

        public abstract IAveMetadataServiceApplication CreateMetadataServiceApplication(Guid applicationId);

        public abstract IAveSOIntegrationUtility CreateSOIntegrationUtility();

        public abstract IAveSOIntegrationUtility CreateSOIntegrationUtility(AveStorageInfo storageInfo, IAveSite site);

        public abstract IAveElementProvider CreateElementProvider();

        public abstract object CreateSOIntegrationAPI();

        public abstract IAveEventManager CreateEventManager();

        public abstract IAveWebPartPagesWebService CreateWebPartPagesWebService(IAveWeb web);

        public abstract IAveWorkflowService CreateWorkflowService();

        public abstract IAveWorkflowServicesManager CreateWorkflowServicesManager(IAveWeb web);

        public abstract IAveFormsServicesWebService CreateFormsServicesWebService(IAveWeb web);

        public abstract IAveBrowserQuery CreateBrowserQuery(string siteUrl);

        public abstract object CreateConnectorInegration();

        public abstract IAveOContent CreateContent(IAveOSearchServiceApplication searchApp);

        public abstract IAveORanking CreateRanking(IAveOSearchServiceApplication searchApp);

        public abstract IAveOCrawlLogFilters CreateCrawlLogFilters();

        public abstract IAveOLogViewer CreateLogViewer(IAveOSearchServiceApplication searchApp);

        public abstract IAveODailySchedule CreateODailySchedule(IAveOSearchServiceApplication searchApp);

        public abstract IAveOMonthlyDateSchedule CreateMonthlyDateSchedule(IAveOSearchServiceApplication searchApp);

        public abstract IAveOWeeklySchedule CreateOWeeklySchedule(IAveOSearchServiceApplication searchApp);

        public abstract IAveOApplicationRegistry CreateApplicationRegistry();

        public abstract IAveLinksCheckerJob CreateLinksCheckerJob(IAveService service);

        public abstract IAveListItemSerializer CreateListItemSerializer(IAveSite site, IAveWeb web, IAveList list);

        public abstract IAveListItem WrapperListItem(IAveList list, Dictionary<string, object> itemProperties);

        public abstract IAveUserProfileSerializer CreateUserProfileSerializer(IAveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo);

        public abstract T CreateQueryService<T>(object arg) where T : IAveQueryService;

        public abstract IAveOHold CreateHold();

        public abstract IAveOUserProfileManager CreateUserProfileManager(IAveOServerContext context);

        public abstract IAveOSearchServiceApplication CreateSearchServiceApplication();

        public abstract IAveOServerContext CreateServerContext();

        public abstract IAveServiceContext CreateServerContext(AveServiceContextInfo contextInfo);

        public abstract IAveOServerFarm CreateServerFarm();

        public abstract IAveOLocation CreateLocation(string name, IAveOSearchServiceApplicationProxy searchProxy);

        public abstract IAveOLocationList CreateLocationList();

        public abstract IAveOQueryManager CreateQueryManager();

        public abstract IAveSearchService CreateSearchService(string name, IAveFarm farm);

        public abstract IAveOScopes CreateScopes(IAveOSearchContext searchContext);

        public abstract IAveOScopes CreateScopes(IAveOSearchServiceApplication searchServiceApplication);

        public abstract IAveOUserProfileService CreateUserProfileService();

        public abstract IAveOSchema CreateSchema(IAveOSearchServiceApplication aveOSearchServiceApplication);

        public abstract IAveOSchema CreateSchema(IAveOSearchContext aveOSearchContext);

        public abstract IAveWorkflowCollection CreateWorkflowCollection(IAveList list, Guid associationId);

        public abstract IAveWorkflowCollection CreateWorkflowCollection(IAveWeb web);

        public abstract IAveMeeting CreateMeeting();

        public abstract IAveRegister CreateAveRegister();

        public abstract IAveOSetFormsServiceCmdlet CreateSetFormsServiceCmdlet();

        public abstract IAveOContentIterator CreateContentIterator();

        public abstract IAveContentTypePageUtil CreateContentTypePageUtil();

        public abstract IAveOPolicyItemCollection CreateOPolicyItemCollection(IAveOPolicy policy);

        public abstract IAveOExpiration CreateExpiration();

        public abstract IAveOPolicyAudit CreatePolicyAudit();

        public abstract IAveExecutionTimeCounter CreateExecutionTimeCounter();

        public abstract IAveExecutionTimeCounter CreateExecutionTimeCounter(uint maxValue);

        public abstract IAveOPolicy CreateOPolicy();

        public abstract IAveONewCustomConnector CreateNewCustomConnector();

        public abstract IAveORemoveCustomConnector CreateRemoveCustomConnector();

        public abstract IAveOSearchAdminUtils CreateSearchAdminUtils();

        public abstract IAveOMapping CreateMapping(Guid crawledPropset, string crawledPropertyName, int crawledPropertyVariantType, int managedPid);

        public abstract IAveAppCatalog CreateAppCatalog();

        public abstract IAveAppSerializer CreateAppSerializer(IAveWeb web, int restoreMode);

        public abstract IAveAttachmentSerializer CreateAttachmentSerializer(IAveList list, int restoreOption);

        public abstract IAveTheme CreateTheme();

        public abstract IAveProfileLoader CreateOLProfileLoader(string url);

        public abstract IAveSiteServiceHelper CreateSiteServiceHelper();
    }
}