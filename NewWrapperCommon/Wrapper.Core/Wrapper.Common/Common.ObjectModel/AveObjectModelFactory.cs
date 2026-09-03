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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Runtime.Serialization.Formatters.Binary;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    //make it abstract factory to avoid performance issue caused by reflection
    public abstract class AveObjectModelFactory
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static string copySiteUrl;
        private static AveBPOSAccountInfo copyAccountInfo;
        private static AveContextKind copyContextKind;

        internal const string ServerAssemblyName = "SP2010WrapperServer";
        internal const string ServerNameSpace = "AvePoint.ObjectModel.Server.";
        internal const string ClientAssemblyName = "AgentCommonObjectModelCommon";
        internal const string ClientNameSpace = "AvePoint.ObjectModel.Common.";
        internal const string Server07AssmeblyName = "SP2007WrapperServer";
        internal const string Server07NameSpace = "AvePoint.ObjectModel.Server07.";
        internal const string Server13AssemblyName = "SP2013WrapperServer";
        internal const string Server13NameSpace = "AvePoint.ObjectModel.Server13.";

        internal const string Server16AssemblyName = "SP2016WrapperServer";
        internal const string Server16NameSpace = "AvePoint.ObjectModel.Server16.";

        internal const string Server19AssemblyName = "SP2019WrapperServer";
        internal const string Server19NameSpace = "AvePoint.ObjectModel.Server19.";

        internal const string ServerSEAssemblyName = "SPSEWrapperServer";
        internal const string ServerSENameSpace = "AvePoint.ObjectModel.ServerSE.";
        protected AveObjectModelFactory()
        {
        }

        public static AveObjectModelFactory CreateObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo, AveContextKind contextKind)
        {
            copySiteUrl = siteUrl;
            copyAccountInfo = accountInfo;
            copyContextKind = contextKind;
            try
            {
                AveContextKind lContextKind = DecideUseWhichModel(contextKind, siteUrl);
                AveObjectModelFactory omFactory = null;
                switch (lContextKind)
                {
                    case AveContextKind.Server10ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(ServerAssemblyName, ServerNameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.ClientObjectModel:
                        InitProxy();
                        omFactory = AveAssemblyUtility.CreateInstance(ClientAssemblyName, ClientNameSpace + "AveClientObjectModelFactory", new Type[] { typeof(string), typeof(AveBPOSAccountInfo) }, new object[] { siteUrl, accountInfo }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server07ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server07AssmeblyName, Server07NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server13ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server13AssemblyName, Server13NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server16ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server16AssemblyName, Server16NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server19ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server19AssemblyName, Server19NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.ServerSEObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(ServerSEAssemblyName, ServerSENameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    default:
                        break;
                }
                WrapperRuntime.SetGlobalContextModelFactorySetting(omFactory);
                WrapperRuntime.CurrentContext.ModelFactory = omFactory;
                return omFactory;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.ERROR, WrapperCommonResource.AWCGetObjectError, e);
                return null;
            }
        }
        public static AveObjectModelFactory CopyAveObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo, AveContextKind contextKind)
        {
            copySiteUrl = siteUrl;
            copyAccountInfo = accountInfo;
            copyContextKind = contextKind;
            try
            {
                AveContextKind lContextKind = DecideUseWhichModel(contextKind, siteUrl);
                AveObjectModelFactory omFactory = null;
                switch (lContextKind)
                {
                    case AveContextKind.Server10ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(ServerAssemblyName, ServerNameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.ClientObjectModel:
                        InitProxy();
                        omFactory = AveAssemblyUtility.CreateInstance(ClientAssemblyName, ClientNameSpace + "AveClientObjectModelFactory", new Type[] { typeof(string), typeof(AveBPOSAccountInfo) }, new object[] { siteUrl, accountInfo }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server07ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server07AssmeblyName, Server07NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server13ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server13AssemblyName, Server13NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server16ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server16AssemblyName, Server16NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.Server19ObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(Server19AssemblyName, Server19NameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    case AveContextKind.ServerSEObjectModel:
                        omFactory = AveAssemblyUtility.CreateInstance(ServerSEAssemblyName, ServerSENameSpace + "AveServerObjectModelFactory", new Type[] { typeof(string) }, new object[] { siteUrl }) as AveObjectModelFactory;
                        break;
                    default:
                        break;
                }
                WrapperRuntime.SetGlobalContextModelFactorySetting(omFactory);
                return omFactory;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.ERROR, WrapperCommonResource.AWCGetObjectError, e);
                return null;
            }
        }
        public static AveObjectModelFactory CreateObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo)
        {
            return CreateObjectModelFactory(siteUrl, accountInfo, AveContextKind.Auto);
        }

        private static void InitProxy()
        {
            if (WrapperConfiguration.IsProxyEnabled)
            {
                ProxyInfo info = WrapperConfiguration.ProxyInfo;
                var reqProxy = new WebProxy(info.Address, info.BypassProxyOnLocal,info.BypassList) { Credentials = new NetworkCredential(info.Username, info.Password) };
                WebRequest.DefaultWebProxy = reqProxy;
            }
            else
            {
                WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
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
            if ((contextKind == AveContextKind.ClientObjectModel && AveEnvironment.IsSPInstalled(siteUrl)) || (contextKind == AveContextKind.ServerObjectModel))
            {
                AveSPEnv.IsServerMode = true;
                if (SPSVersionDetector.IsMossSEInstalled())
                {
                    return AveContextKind.ServerSEObjectModel;
                }
                else if (SPSVersionDetector.IsMoss2019Installed())
                {
                    return AveContextKind.Server19ObjectModel;
                }
                else if (SPSVersionDetector.IsMoss2016Installed())
                {
                    return AveContextKind.Server16ObjectModel;
                }
                else if (SPSVersionDetector.IsMoss2013Installed())
                {
                    return AveContextKind.Server13ObjectModel;
                }
                else if (SPSVersionDetector.IsMOSS2010Installed())
                {
                    return AveContextKind.Server10ObjectModel;
                }
                else
                {
                    return AveContextKind.Server07ObjectModel;
                }
            }
            return contextKind;
        }

        public abstract AveDiscoverReader CreateDiscoverReader(DiscoverModule module);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="InitializedByFactory">false option is used for static methods,default is true</param>
        /// <returns></returns>
        public abstract IAveSite CreateSite(bool InitializedByFactory = true);

        public abstract IAveSite CreateSite(string url);

        public abstract IAveSite CreateSite(Guid siteId);

        public abstract IAveSite CreateSite(string url, IAveUserToken token);

        public abstract IAveSite CreateAdminCenterSite(string url);

        /// <summary>
        /// 创建一个使用System Account登录的Site Collection, 目前只有Local 13有实现
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public abstract IAveSite CreateElevatedSite(string url);

        public abstract IAveSite CreateSite(Guid id, AveUrlZone zone);

        public abstract IAveTenant CreateTenant(IAveSite site);

        public abstract IAveTenant CreateTenant(string url, bool isOnline = true, bool needLoadProperties = false);

        public abstract IAveAzurePowerShellRequest CreateAzurePowerShellRequest(AveBPOSAccountInfo accountInfo);

        public abstract string GetAdminUrl(AveBPOSAccountInfo accountInfo);

        public abstract IAveWebApplication CreateWebApplication();

        public abstract IAveRoleDefinitionBindingCollection CreateRoleDefinitionBindingCollection();

        public abstract IAveRoleAssignment CreateRoleAssignment(IAvePrincipal principal);

        public abstract IAveQuery CreateQuery();

        public abstract IAveContentTypeIdPub CreateContentTypePub();

        public abstract IAvePublishingWeb CreatePublishingWeb(IAveWeb web);

        public abstract IAvePublishingWeb CreatePublishingWeb();

        public abstract IAvePublishingSite CreatePublishingSite();

        public abstract IAveFieldMultiChoiceValue CreateFieldMultiChoiceValue();

        public abstract IAveFieldMultiChoiceValue CreateFieldMultiChoiceValue(string fieldValue);

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

        public abstract IAveTaxonomySession CreateTaxonomySession(IAveServiceContext context);

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

        public abstract IAveClaimProviderOperations CreateClaimProviderOperations();

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

        public abstract IAveWebConfigModification CreateAveWebConfigModification();

        public abstract IAveWebConfigModification CreateAveWebConfigModification(string name, string xpath);

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

        public abstract IAveOSocialRatingManager CreateSocialRatingManager(IAveServiceContext context);

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

        public abstract IAveFeatureProperty CreateFeatureProperty(string propName, string propValue);

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

        /// <summary>
        /// ADO-162688，该接口实现主要是为了支持local往云端上传数据，365不需要实现
        /// </summary>
        /// <returns></returns>
        public abstract IAveExportObject CreateExportObject();

        public abstract IAveExport CreateExport(IAveExportSettings exportSettings);

        public abstract IAvePublishing CreatePublishing(IAveSite site);

        public abstract IAveFieldLookupValue CreateFieldLookupValue();

        public abstract IEcmDocumentRouting EcmDocumentRouting();

        public abstract IAvePersistedTypeCollection<T> CreatePersistedTypeCollection<T>(IAveFarm farm) where T : IAvePersistedObject;

        public abstract IAveSolutionLanguagePack CreateSoluctionLanguagePack();

        public abstract IAveMetaDataServiceSerializer CreateMetadataServiceSerilizer(Guid serviceAppId);

        public abstract IAveMetadataServiceRestorer CreateMetadataServiceRestorer(Guid serviceAppId);

        public abstract IAveMetadataServiceApplication CreateMetadataServiceApplication(string name);
        public abstract IAveMetadataServiceApplication CreateMetadataServiceApplication(string name, Guid defaultPartitionId);
        public abstract IAveMetadataServiceApplication CreateMetadataServiceApplication(IAveSite site);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount, DateTime startTime, DateTime endTime);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DiscoverModule module);

        public abstract IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DateTime startTime, DateTime endTime, DiscoverModule module);

        public abstract IAveContentTypePublisher CreateContentTypePublisher();

        public abstract IAveContentTypePublisher CreateContentTypePublisher(IAveSite site);

        public abstract IAveContentTypePublisher CreateContentTypePublisher(IAveTermStore store);

        public abstract IAveMetadataServiceApplication CreateMetadataServiceApplication(Guid applicationId);
        public abstract IAveMetadataServiceApplication CreateMetadataServiceApplication(Guid applicationId, Guid defaultPartitionId);

        public abstract IAveSOIntegrationUtility CreateSOIntegrationUtility();
        public abstract IAveSOIntegrationUtility CreateSOIntegrationUtility(IAveSite site,IAveList list);

        //public abstract IAveSOIntegrationUtility CreateSOIntegrationUtility(AveStorageInfo storageInfo, IAveSite site);

        //public abstract IAveSOIntegrationUtility CreateSOIntegrationUtility(AveStorageInfo13 storageInfo, IAveSite site);

        public abstract IAveElementProvider CreateElementProvider();

        public abstract object CreateSOIntegrationAPI();

        public abstract IAveEventManager CreateEventManager();

        public abstract IAveWebPartPagesWebService CreateWebPartPagesWebService(IAveWeb web);

        public abstract IAveFormsServicesWebService CreateFormsServicesWebService(IAveWeb web);

        public abstract IAveWrapperWorkflowService CreateWorkflowService();

        public abstract IAveWorkflowServicesManager CreateWorkflowServicesManager(IAveWeb web);

        public abstract IAveBrowserQuery CreateBrowserQuery(string siteUrl, AvePoint.Common.AveSqlConnection sqlConn);

        public abstract IAveBrowserQuery CreateBrowserQuery(string siteUrl, string connectString);

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

        public abstract IAveUserProfileSerializer CreateUserProfileSerializer(IAveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo);

        public abstract IAveUserProfileSerializer CreateUserProfileSerializer(IAveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo, Func<string, string> userMapping);

        public abstract T CreateQueryService<T>(object arg) where T : IAveQueryService;

        public abstract IAveOHold CreateHold();

        public abstract IAveOUserProfileManager CreateUserProfileManager(IAveOServerContext context);

        public abstract IAveOSearchServiceApplication CreateSearchServiceApplication();

        public abstract IAveOServerContext CreateServerContext();

        public abstract IAveServiceContext CreateServerContext(AveServiceContextInfo contextInfo);

        public abstract IAveServiceContext CreateServiceContext(IAveServiceApplicationProxyGroup proxyGroup, IAveSiteSubscriptionIdentifier identifier);

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

        public abstract IAveOMapping CreateMapping();

        public abstract IAveOMapping CreateMapping(Guid crawledPropset, string crawledPropertyName, int crawledPropertyVariantType, int managedPid);

        public abstract IAveOMappingCollection CreateMappingCollection();

        public abstract IAveOPropagation CreatePropagation(IAveOSearchServiceApplication searchServiceApplication);

        public abstract IAveOTopologySettings CreateTopologySettings();

        public abstract IAveOCrawlComponentSettings CreateCrawlComponentSettings();

        public abstract IAveOSearchApplicationSystemStatus CreateSearchApplicationSystemStatus();

        public abstract IAveOCrawlLogData CreateCrawlLogData(IAveOSearchServiceApplication searchApp);

        public abstract IAveUsageApplicationProxy CreateUsageApplicationProxy();
        //add by adrian
        public abstract IAveORanking CreateRanking(IAveOSearchServiceApplication searchApp, IAveOSearchObjectOwner searchOwner);

        public abstract IAveOSearchObjectOwner CreateSearchOwner(AveOSearchObjectLevel objectLevel, IAveWeb aveWeb);

        public abstract IAveOTopologySettings CreateTopologySettings(IAveOSearchServiceApplication searchApplication);

        public abstract IAveOStringResourceManager CreateOStringResourceManager();

        public abstract IAveEventReceiverBase CreateEventReceiverBase();

        public abstract IAveSiteDataQuery CreateSiteDataQuery();

        public abstract IAveContentType AddSameParentContentType(IAveContentTypeCollection collection, IAveContentType contentType);

        public abstract IAveImageRenditionCollection CreateImageRenditionCollection();

        public abstract IAvePublishingPage CreatePublishingPage(IAveListItem item);

        public abstract IAveOUserProfileChangeToken CreateUserProfileChangeToken(DateTime date);

        public abstract IAveOUserProfileChangeToken CreateUserProfileChangeToken(string changeToken);

        public abstract IAveOUserProfileChangeToken CreateUserProfileChangeToken(long eventId, DateTime changeTime);

        public abstract IAveOUserProfileChangeQuery CreateUserProfileChangeQuery();

        public abstract IAveOUserProfileSubTypeManager CreateProfileSubTypeManager(IAveServiceContext serviceContext);


        public abstract IAveOriginalIssuers CreateOriginalIssuers();

        public abstract IAveClaimTypes CreateClaimTypes();

        public abstract IAveClaim CreateClaim(string type, string value, string valueType, string originalIssuer);

        public abstract IAveRecycleBinQuery CreateRecycleBinQuery();

        #region add for SP2013
        public abstract IAveReputationHelper CreateReputationHelper();

        public abstract IAveUserSettingsProviderManager CreateUserSettingsProviderManager();

        public abstract IAveTheme CreateTheme();

        public abstract IAveColor CreateColor();

        public abstract IAveFont CreateFont();

        public abstract IAveOSearchObjectFilter CreateSearchObjectFilter(IAveOSearchObjectOwner searchObjectOwner);

        public abstract IAveOSearchObjectOwner CreateSearchOwner(AveOSearchObjectLevel objectLevel);

        public abstract IAveFederationManager CreateFederationManager(IAveOSearchServiceApplication searchServiceApplication);

        public abstract IAveOSearchProvider CreateSearchProvider();

        public abstract IAveOSharedSearchBoxSettings CreateSharedSearchBoxSettings();

        public abstract IAveOSocialFollowingManager CreateSPSocialFollowingManager(IAveOUserProfile profile, IAveServiceContext context);

        public abstract IAveOSocialActorInfo CreateSPSocialActorInfo(AveSocialActorInfo actor);

        public abstract IAveOSocialFeedManager CreateSocialFeedManager();

        public abstract IAveOSocialFeedManager CreateSocialFeedManager(IAveOUserProfile up, IAveServiceContext ctx);

        public abstract IAveOSocialFeedOptions CreateSocialFeedOptions();

        public abstract IAveServiceContextScope CreateServiceContextScope(IAveServiceContext serverContextScop);

        public abstract IAveOSocialPostCreationData CreateSocialPostCreationData();

        public abstract IAveOSocialAttachment CreateSocialAttachment();

        public abstract IAveOSocialDataItem CreateSocialDataItem();

        public abstract IAveOSocialDataItem[] CreateSocialDataItemCollection(int count);
        public abstract IAveOUserProfilePropertyManager CreateUserProfilePropertyManager(IAveServiceContext serviceContext);

        public abstract IAveWorkflowInventoryUpgrade CreateWorkflowInventoryUpgrade();


        #endregion

        #region Add for SP App
        public abstract IAveAppCatalog CreateAppCatalog();

        public static AveObjectModelFactory CloneAveObjectModelFactory()
        {
            return CopyAveObjectModelFactory(copySiteUrl, copyAccountInfo, copyContextKind);
        }
        #endregion

        public abstract IAveProfileLoader CreateProfileLoader(string adminUrl);
        
        public abstract ISharePointDataProcessor CreateSharepointDataProcessor(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo SourceSiteInfo, Func<string, string> GetUserFromMapping);

        public abstract IAveSPCommentStorage CreateSPCommentStorage(IAveSite stie);

        public abstract IAveClientRequest CreateClientRequest(string url, AveBPOSAccountInfo userAccountInfo, AuthenticationModeOption[] authenticationModeOptions);

        public abstract IAveSiteServiceHelper CreateSiteServiceHelper();
    }
}