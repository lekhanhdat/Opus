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
using System.Net;
using System.Text;
using System.IO;
using System.Security;
using Microsoft.SharePoint;
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.ObjectModel.ServerSE.Office;
using AvePoint.ObjectModel.ServerSE.Search;
using AvePoint.Wrapper.QueryService;
using AvePoint.Wrapper.Common.Extension;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveServerObjectModelFactory :AveObjectModelFactoryExtension
    {
        internal const string SOIntegrationAssemblyName = "SPSEStorageOptimizationIntegration"; 
        internal const string SOIntegrationNameSpace = "AvePoint.StorageOptimization.Integration.";
        internal const string SOConnectorIntegrationAssemblyName = "SPSEConnectorBusinessLogic";
        internal const string SOConnectorIntegrationNameSpace = "StorageOptimization.Connector.BusinessLogic.WrapperImpl.";
        private static readonly string Microsoft_SharePoint_ApplicationPagesAssembly_Path = System.Environment.GetEnvironmentVariable("CommonProgramFiles") + @"\Microsoft Shared\Web Server Extensions\16\CONFIG\BIN\Microsoft.SharePoint.ApplicationPages.dll";

        private static AveUtility mUtility;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveServerObjectModelFactory));

        private string mSiteUrl;

        public override AveContextKind ContextKind
        {
            get { return AveContextKind.ServerSEObjectModel; }
        }

        public override bool IsSPInstalled
        {
            get { return true; }
        }

        public override IAveUtility Utility
        {
            get { return mUtility; }
        }

        public override AveAPIType APIType
        {
            get
            {
                return AveAPIType.Server;
            }
        }

        public override AveBPOSAccountInfo AccountInfo
        {
            get
            {
                return null;
            }
        }

        static AveServerObjectModelFactory()
        {
            logger.Debug("Start SE server object model factory");
            mUtility = new AveUtility();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AveServerAssemblyInit.LoadAssembly();
            AveAssemblyUtility.SetStaticFieldValue(typeof(SPSite).Assembly, "Microsoft.SharePoint.SPCertificateValidator", "s_ServicePointManagerCertificatePolicyInitialized", true);
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => 1 < 2;
            DisableIriParsing();
        }
        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c引用了Microsoft.SharePoint.ApplicationPages, 并且不在GAC中，反射获取ApplicationPages相关的Type的时候会错
            if (string.Equals(args.Name, "Microsoft.SharePoint.ApplicationPages, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", StringComparison.OrdinalIgnoreCase))
            {
                return Assembly.LoadFile(Microsoft_SharePoint_ApplicationPagesAssembly_Path);
            }
            return null;
        }

        /// <summary>
        ///Disable Iri parsing, otherwise API will encode some special characters.
        ///The section in app.config about Iri parsing setting will overwrite this part
        /// </summary>       
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues",Justification = "System.Uri.s_IriParsing")]
        private static void DisableIriParsing()
        {
            try
            {
                AveAssemblyUtility.SetFieldValue(null, typeof(UriParser), "s_QuirksVersion", 2);
                AveAssemblyUtility.SetFieldValue(null, typeof(Uri), "s_IriParsing", false);
                //re get the section in app.config
                AveAssemblyUtility.SetFieldValue(null, typeof(Uri), "s_ConfigInitialized", false);
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to disable IRI parsing setting, error: {0}", ex);
            }
        }

        public AveServerObjectModelFactory(string siteUrl)
        {
            mSiteUrl = siteUrl;
        }

        public override AveDiscoverReader CreateDiscoverReader(DiscoverModule module)
        {
            return AveDiscoverReaderFactory.GetAveDiscoverReader(module);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="InitializedByFactory">false option is used for static methods,default is true</param>
        /// <returns></returns>
        public override IAveSite CreateSite(bool InitializedByFactory = true)
        {
            if (InitializedByFactory)
            {
                return new AveSite(mSiteUrl);
            }
            return new AveSite();
        }

        public override IAveSite CreateSite(string url)
        {
            mSiteUrl = url;
            return new AveSite(url);
        }

        public override IAveSite CreateSite(Guid siteId)
        {
            return new AveSite(siteId);
        }

        public override IAveSite CreateSite(string url, IAveUserToken token)
        {
            return new AveSite(url, token);
        }

        public override IAveSite CreateElevatedSite(string url)
        {
            return new AveSite(url, new AveUserToken(SPUserToken.SystemAccount));
        }

        public override IAveSite CreateSite(Guid id, AveUrlZone zone)
        {
            return new AveSite(id, zone);
        }

        public override IAveSite CreateAdminCenterSite(string url)
        {
            throw new NotImplementedException();
        }

        public override IAveWebApplication CreateWebApplication()
        {
            return new AveWebApplication();
        }

        public override IAveRoleDefinitionBindingCollection CreateRoleDefinitionBindingCollection()
        {
            return new AveRoleDefinitionBindingCollection();
        }

        public override IAveRoleAssignment CreateRoleAssignment(IAvePrincipal principal)
        {
            return new AveRoleAssignment(principal as AvePrincipal);
        }

        public override IAveQuery CreateQuery()
        {
            return new AveQuery();
        }

        public override IAveContentTypeIdPub CreateContentTypePub()
        {
            return new AveContentTypeIdPub();
        }

        public override IAvePublishingWeb CreatePublishingWeb(IAveWeb web)
        {
            return new AvePublishingWeb(web);
        }

        public override IAvePublishingWeb CreatePublishingWeb()
        {
            return new AvePublishingWeb();
        }

        public override IAvePublishingSite CreatePublishingSite()
        {
            return new AvePublishingSite();
        }

        public override IAveFieldMultiChoiceValue CreateFieldMultiChoiceValue()
        {
            return new AveFieldMultiChoiceValue();
        }

        public override IAveFieldMultiChoiceValue CreateFieldMultiChoiceValue(string fieldValue)
        {
            return new AveFieldMultiChoiceValue(fieldValue);
        }

        public override IAveFieldUrlValue CreateFieldUrlValue()
        {
            return new AveFieldUrlValue();
        }

        public override IAveFieldUrlValue CreateFieldUrlValue(string fieldValue)
        {
            return new AveFieldUrlValue(fieldValue);
        }

        public override IAveFieldLink CreateFieldLink(IAveField field)
        {
            return new AveFieldLink(field as AveField);
        }

        public override IAveContentTypeId CreateContentTypeId(string id)
        {
            return new AveContentTypeId(id);
        }

        public override IAveContentTypeId CreateContentTypeId()
        {
            return new AveContentTypeId();
        }

        public override IAveContentType CreateContentType()
        {
            return new AveContentType();
        }

        public override IAveContentType CreateContentType(IAveContentTypeId contentTypeId)
        {
            return new AveContentType(contentTypeId);
        }

        public override IAveContentType CreateContentType(IAveContentTypeId parentContentType, IAveContentTypeCollection collection, string name)
        {
            return new AveContentType(parentContentType as AveContentTypeId, collection as AveContentTypeCollection, name);
        }

        public override IAveContentType CreateContentType(IAveContentType parentContentType, IAveContentTypeCollection collection, string name)
        {
            return new AveContentType(parentContentType, collection, name);
        }

        public override IAveRegionalSettings CreateRegionalSettings(IAveWeb web, bool bIsUserRegionalSetting)
        {
            return new AveRegionalSettings(web as AveWeb, bIsUserRegionalSetting);
        }

        public override IAveRegionalSettings CreateRegionalSettings()
        {
            return new AveRegionalSettings();
        }

        public override IAveRoleDefinition CreateRoleDefinition()
        {
            return new AveRoleDefinition();
        }

        public override IAveNavigationNode CreateNavigationNode(string title, string url, bool isExternal)
        {
            return new AveNavigationNode(title, url, isExternal);
        }

        public override IAveNavigationNode CreateNavigationNode(string title, string url)
        {
            return new AveNavigationNode(title, url);
        }

        public override IAveTaxonomyFieldValue CreateTaxonomyFieldValue(IAveField field)
        {
            return new AveTaxonomyFieldValue(field);
        }

        public override IAveTaxonomyFieldValue CreateTaxonomyFieldValue(string value)
        {
            return new AveTaxonomyFieldValue(value);
        }
        public override IAveTaxonomyFieldValueCollection CreateTaxonomyFieldValueCollection(IAveField field)
        {
            return new AveTaxonomyFieldValueCollection(field);
        }

        public override IAveFieldUserValueCollection CreateFieldUserValueCollection()
        {
            return new AveFieldUserValueCollection();
        }

        public override IAveFieldUserValue CreateFieldUserValue(IAveWeb web, int lookupId, string lookupValue)
        {
            return new AveFieldUserValue(web, lookupId, lookupValue);
        }

        public override IAveFieldLookupValueCollection CreateFieldLookupValueCollection()
        {
            return new AveFieldLookupValueCollection();
        }

        public override IAveFieldLookupValue CreateFieldLookupValue(int lookupId, string lookupValue)
        {
            return new AveFieldLookupValue(lookupId, lookupValue);
        }

        public override IAveUserToken CreateUserToken(byte[] token)
        {
            return new AveUserToken(token);
        }

        public override IAveWebApplication CreateWebApplication(string url)
        {
            return new AveWebApplication(url);
        }

        public override IAveItem CreateAveItem(IAveWeb web, IAveList list)
        {
            return new AveItem(web, list);
        }

        public override IAveFarm CreateFarm()
        {
            return new AveFarm();
        }
        public override IAveSecurity CreateSecurity()
        {
            return new AveSecurity();
        }

        public override IAveWebService CreateWebService()
        {
            return new AveWebService();
        }

        public override IAveWebService CreateWebService(string name, IAveFarm farm)
        {
            return new AveWebService(name, farm);
        }

        public override IAveUserCodeService CreateUserCodeService()
        {
            return new AveUserCodeService();
        }

        public override IAveUserCodeService CreateUserCodeService(IAveFarm farm)
        {
            return new AveUserCodeService(farm);
        }

        public override IAveRoleAssignment CreateRoleAssignment(IAveUser user)
        {
            return new AveRoleAssignment(user as AvePrincipal);
        }

        public override IAveAdministrationWebApplication CreateAdministrationWebApplication()
        {
            return new AveAdministrationWebApplication();
        }

        public override IAveItemEventReceiver CreateItemEventReceiver()
        {
            return new AveItemEventReceiver();
        }

        public override IAveAlternateUrlCollection CreatedAlternateUrlCollection(string name, IAveFarm local)
        {
            return new AveAlternateUrlCollection(name, local);
        }

        public override IAveAlternateUrl CreateAlternateUrl(string incomingUrl, AveUrlZone urlZone)
        {
            return new AveAlternateUrl(incomingUrl, urlZone);
        }

        public override IAveAlternateUrl CreateAlternateUrl(Uri requestUri, AveUrlZone urlZone)
        {
            return new AveAlternateUrl(requestUri, urlZone);
        }

        public override IAveMetadataListFieldSettings CreateMetadataListFieldSettings(IAveList list)
        {
            return new AveMetadataListFieldSettings(list);
        }

        public override IAveTaxonomySession CreateTaxonomySession(IAveSite site)
        {
            return new AveTaxonomySession(site);
        }

        public override IAveTaxonomySession CreateTaxonomySession()
        {
            return new AveTaxonomySession();
        }

        public override IAveTaxonomySession CreateTaxonomySession(IAveServiceContext context)
        {
            return new AveTaxonomySession(context);
        }

        public override IAveOMetadataNavigationSettings CreateMetadataNavigationSettings()
        {
            return new AveOMetadataNavigationSettings();
        }

        public override IAveOMetadataNavigationSettings CreateMetadataNavigationSettings(string xmlMetadataNavigationSettings)
        {
            return new AveOMetadataNavigationSettings(xmlMetadataNavigationSettings);
        }

        public override IAveOMappingCollection CreateMappingCollection()
        {
            return new AveOMappingCollection();
        }

        public override IAveOFieldIndexDictionary CreateFieldIndexDictionary()
        {
            return new AveOFieldIndexDictionary();
        }

        public override IAveDateOptions CreateDateOptions(string localeId, AveCalendarType calendar, string workWeek, string firstDayOfWeek, string hijriAdjustment, string timeZoneSpan, string selectedDate)
        {
            return new AveDateOptions(localeId, calendar, workWeek, firstDayOfWeek, hijriAdjustment, timeZoneSpan, selectedDate);
        }

        public override IAveOPolicyCatalog CreatePolicyCatalog(IAveSite site)
        {
            return new AveOPolicyCatalog(site);
        }

        public override IAveOSearchService CreateOSearchService()
        {
            return new AveOSearchService();
        }

        public override IAveOConfiguredView CreateConfiguredView(IAveView view, int index)
        {
            return new AveOConfiguredView(view, index);
        }

        public override IAveONodeViewSettings CreateNodeViewSettings(IAveOViewSettingsCollection viewSettingsCollection, string uniqueNodeId, int folderId)
        {
            return new AveONodeViewSettings(viewSettingsCollection, uniqueNodeId, folderId);
        }

        public override IAveRatingsSettingsPage CreateRatingsSettingsPage()
        {
            return new AveRatingsSettingsPage();
        }

        public override IAveMobileUtility CreateMobileUtility()
        {
            return new AveMobileUtility();
        }

        public override IAveAlertTemplateCollection CreateAlertTemplateCollection(IAveWebService wssService)
        {
            return new AveAlertTemplateCollection(wssService);
        }

        public override IAveResource CreateResource()
        {
            return new AveResource();
        }

        public override IAveOmsMobileFacade CreateOmsMobileFacade()
        {
            return new AveOmsMobileFacade();
        }

        public override IAveCredentialDeployment CreateCredentialDeployment()
        {
            return new AveCredentialDeployment();
        }

        public override IAveGlobalAdmin CreateGlobalAdmin()
        {
            return new AveGlobalAdmin();
        }

        public override IAveDatabaseService CreateDatabaseService()
        {
            return new AveDatabaseService();
        }

        public override IAveOMetadataNavigationHierarchy CreateMetadataNavigationHierarchy()
        {
            return new AveOMetadataNavigationHierarchy();
        }

        public override IAveOMetadataNavigationHierarchy CreateMetadataNavigationHierarchy(IAveField hierarchyField)
        {
            return new AveOMetadataNavigationHierarchy(hierarchyField);
        }

        public override IAveOMetadataNavigationKeyFilter CreateMetadataNavigationKeyFilter()
        {
            return new AveOMetadataNavigationKeyFilter();
        }

        public override IAveOMetadataNavigationKeyFilter CreateMetadataNavigationKeyFilter(IAveField hierarchyFiled)
        {
            return new AveOMetadataNavigationKeyFilter(hierarchyFiled);
        }

        public override IAveOMetadataHierarchyNodeTaxonomy CreateMetadataHierarchyNodeTaxonomy()
        {
            return new AveOMetadataHierarchyNodeTaxonomy();
        }

        public override IAveOMetadataDefaults CreateMetadataDefaults(IAveList aveList)
        {
            return new AveOMetadataDefaults(aveList);
        }

        public override IAveClaimProviderOperations CreateClaimProviderOperations()
        {
            return new AveClaimProviderOperations();
        }

        public override IAveServer CreateServer(string address)
        {
            return new AveServer(address);
        }

        public override IAveServer CreateServer(string address, IAveFarm farm)
        {
            return new AveServer(address, farm);
        }

        public override IAveDatabaseServiceInstance CreateDatabaseServiceInstance(string name, IAveServer server, IAveDatabaseService service)
        {
            return new AveDatabaseServiceInstance(name, server, service);
        }

        public override IAveAlternateUrlCollection CreateAlternateUrlCollection(string resourceName, IAveFarm farm)
        {
            return new AveAlternateUrlCollection(resourceName, farm);
        }

        public override IAvePortalService CreatePortalService(string name, IAveFarm farm)
        {
            return new AvePortalService(name, farm);
        }

        public override IAveFarmManagedAccountCollection CreateFarmManagedAccountCollection(IAveFarm farm)
        {
            return new AveFarmManagedAccountCollection(farm);
        }

        public override IAveOfficialFileHost CreateOfficialFileHost(bool bCreateUniqueId)
        {
            return new AveOfficialFileHost(bCreateUniqueId);
        }

        public override IAveSecurityTokenServiceManager CreateSecurityTokenServiceManager()
        {
            return new AveSecurityTokenServiceManager();
        }

        /// <summary>
        /// Constructor Method for Webs in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override IAveWebCollection CreateWebs(ICredentials crendeantial, string url)
        {
            return new AveWebCollection(crendeantial, url);
        }

        /// <summary>
        /// Constructor Method for Lists in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override IAveListCollection CreateLists(ICredentials crendeantial, string url)
        {
            return new AveListCollection(crendeantial, url);
        }

        /// <summary>
        /// Constructor Method for Views in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override IAveViewCollection CreateViews(ICredentials crendeantial, string url)
        {
            return new AveViewCollection(crendeantial, url);
        }

        public override IAveUpgradeSessionCollection CreateUpgradeSessionCollection(IAveFarm farm)
        {
            return new AveUpgradeSessionCollection(farm);
        }

        public override IAvePersistedDependencyCollection<IAveIisWebServiceApplication> CreatePersistedDependencyCollection(IAveIisWebServiceApplicationPool iisWebServiceApplicationPool)
        {
            return new AvePersistedDependencyCollection<IAveIisWebServiceApplication>(iisWebServiceApplicationPool);
        }

        private string MakeGenericTypeName(Type objectType)
        {
            return "`1[[" + objectType.AssemblyQualifiedName + "]]";
        }

        public override IAveServiceApplicationProxyGroup CreateServiceApplicationProxyGroup()
        {
            return new AveServiceApplicationProxyGroup();
        }

        public override IAveWebApplicationProvisioningJobDefinition CreateWebApplicationProvisioningJobDefinition(IAveWebApplication app)
        {
            return new AveWebApplicationProvisioningJobDefinition(app);
        }

        public override IAveIisWebsiteUnprovisioningJobDefinition CreateIisWebsiteUnprovisioningJobDefinition(bool deleteWebSites, string[] serverComments, string applicationPoolId, string[] vdirs, Guid webAppId, bool webAppUnprovisioning)
        {
            return new AveIisWebsiteUnprovisioningJobDefinition(deleteWebSites, serverComments, applicationPoolId, vdirs, webAppId, webAppUnprovisioning);
        }

        public override IAveWebConfigModification CreateAveWebConfigModification()
        {
            return new AveWebConfigModification();
        }

        public override IAveWebConfigModification CreateAveWebConfigModification(string name, string xpath)
        {
            return new AveWebConfigModification(name, xpath);
        }

        public override IAveWebServiceCollection CreateWebServiceCollection(IAveFarm farm)
        {
            return new AveWebServiceCollection(farm);
        }

        public override IAveWebApplicationBuilder CreateWebApplicationBuilder(IAveFarm farm)
        {
            return new AveWebApplicationBuilder(farm);
        }

        public override IAveIisSettings CreateIisSettings(string serverComment, bool allowAnonymous, bool disableKerberos, IAveServerBinding serverBinding, IAveSecureBinding secureBinding, DirectoryInfo path)
        {
            return new AveIisSettings(serverComment, allowAnonymous, disableKerberos, serverBinding, secureBinding, path);
        }

        /// <summary>
        /// Invoke object for calling static member;
        /// </summary>
        /// <returns></returns>
        public override IAveWebTemplate CreateWebTemplate()
        {
            return new AveWebTemplate();
        }

        public override IAveIisSettings CreateIisSettings()
        {
            return new AveIisSettings();
        }

        public override IAveIisWebSite CreateIisWebSite(int instanceId)
        {
            return new AveIisWebSite(instanceId);
        }

        public override IAveIisWebSite CreateIisWebSite()
        {
            return new AveIisWebSite();
        }

        public override IAveIisApplicationPool CreateIisApplicationPool(string name)
        {
            return new AveIisApplicationPool(name);
        }

        public override IAveServiceProxy CreateServiceProxy(string name, IAveFarm farm)
        {
            return new AveServiceProxy(name, farm);
        }

        public override IAveMobileMessagingAccount CreateMobileMessagingAccount()
        {
            return new AveMobileMessagingAccount();
        }

        public override IAveMobileMessagingAccount CreateMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password)
        {
            return new AveMobileMessagingAccount(serviceName, serviceUrl, userId, password);
        }

        public override IAveMobileMessagingAccount CreateMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password, IAveMobileMessageServiceProvider serviceProvider, IAveMobileMessageUserInfo userInfo)
        {
            return new AveMobileMessagingAccount(serviceName, serviceUrl, userId, password, serviceProvider, userInfo);
        }

        public override IAveServiceApplicationProxyGroupCollection CreateServiceApplicationProxyGroupCollection(IAveFarm farm)
        {
            return new AveServiceApplicationProxyGroupCollection(farm);
        }

        public override IAveDiagnosticsService CreateDiagnosticsService()
        {
            return new AveDiagnosticsService();
        }

        public override IAveSiteCollectionCopier CreateSiteCollectionCopier(IAveContentDatabase dbFrom, IAveContentDatabase dbTo, List<IAveSite> colSites)
        {
            return new AveSiteCollectionCopier(dbFrom, dbTo, colSites);
        }

        public override IAveQueryProvider CreateQueryProvider(Uri helpList, uint lcid)
        {
            return new AveQueryProvider(helpList, lcid);
        }

        public override IAveHelpContextManager CreateHelpContextManager()
        {
            return new AveHelpContextManager();
        }

        public override IAveDeliveryChannelSettings CreateDeliveryChannelSettings()
        {
            return new AveDeliveryChannelSettings();
        }

        public override IAveSQM CreateSQM()
        {
            return new AveSQM();
        }

        public override IAveThmxTheme CreateThmxTheme(IAveSite site)
        {
            return new AveThmxTheme(site);
        }

        public override IAveClaimEncodingManager CreateClaimEncodingManager()
        {
            return new AveClaimEncodingManager();
        }

        public override IAveClaimEntityTypes CreateClaimEntityTypes()
        {
            return new AveClaimEntityTypes();
        }

        public override IAveClaimProviderManager CreateClaimProviderManager()
        {
            return new AveClaimProviderManager();
        }

        public override IAveOneTimeSchedule CreateOneTimeSchedule(DateTime dt)
        {
            return new AveOneTimeSchedule(dt);
        }

        public override IAveSchedule CreateSchedule()
        {
            return new AveSchedule();
        }

        public override IAveOfficialFileSoap CreateOfficialFileSoap(Uri uri)
        {
            return new AveOfficialFileSoap(uri);
        }

        public override IAveOfficialFileSoap CreateOfficialFileSoap(string url)
        {
            return new AveOfficialFileSoap(url);
        }

        public override IAveSecureBinding CreateSecureBinding()
        {
            return new AveSecureBinding();
        }

        public override IAveServerBinding CreateServerBinding()
        {
            return new AveServerBinding();
        }

        public override IAveWindowsAuthenticationProvider CreateWindowsAuthenticationProvider()
        {
            return new AveWindowsAuthenticationProvider();
        }

        public override IAveFormsAuthenticationProvider CreateFormsAuthenticationProvider(string membershipProvider, string roleProvider)
        {
            return new AveFormsAuthenticationProvider(membershipProvider, roleProvider);
        }

        public override IAveTrustedAuthenticationProvider CreateTrustedAuthenticationProvider(string providerName)
        {
            return new AveTrustedAuthenticationProvider(providerName);
        }

        public override IAveQuotaTemplate CreateQuotaTemplate()
        {
            return new AveQuotaTemplate();
        }

        public override IAveWorkflowManager CreateWorkflowManager()
        {
            return new AveWorkflowManager();
        }

        public override IAveListViewWebPart CreateListViewWebPart()
        {
            return new AveListViewWebPart();
        }

        public override IAveServiceContext CreateServiceContext()
        {
            return new AveServiceContext();
        }

        public override IAveSiteSubscriptionIdentifier CreateSiteSubscriptionIdentifier()
        {
            return new AveSiteSubscriptionIdentifier();
        }

        public override IAveOUserProfileManager CreateUserProfileManager(IAveServiceContext context)
        {
            return new AveOUserProfileManager(context);
        }

        public override IAveOSocialTagManager CreateSocialTagManager(IAveServiceContext context)
        {
            return new AveOSocialTagManager(context);
        }

        public override IAveOSocialRatingManager CreateSocialRatingManager(IAveServiceContext context)
        {
            return new AveOSocialRatingManager(context);
        }

        public override IAveOAlternateAccessMapping CreateOAlternateAccessMapping()
        {
            return new AveOAlternateAccessMapping();
        }

        public override IAveOUserProfileApplicationProxy CreateOUserProfileApplicationProxy()
        {
            return new AveOUserProfileApplicationProxy();
        }

        public override IAveOULS CreateOULF()
        {
            return new AveOULS();
        }

        public override IAveOSocialCommentManager CreateSocialCommentManager(IAveServiceContext context)
        {
            return new AveOSocialCommentManager(context);
        }

        public override IAveSecureString CreateSecurityString()
        {
            return new AveSecureString();
        }

        public override IAveSecurityContext CreateSecurityContext(IntPtr priorToken)
        {
            return new AveSecurityContext(priorToken);
        }

        public override IAveORecords CreateRecords()
        {
            return new AveORecords();
        }

        public override IAveOFormsService CreateFormsService()
        {
            return new AveOFormsService();
        }

        public override IAveBlockedSolution CreateBlockedSolution(string fileName, string signature, string message)
        {
            return new AveBlockedSolution(fileName, signature, message);
        }

        public override IAveDatabaseSequence CreateDatabaseSequence()
        {
            return new AveDatabaseSequence();
        }

        public override IAveOFormTemplateCollection CreateFormTemplateCollection()
        {
            return new AveOFormTemplateCollection();
        }

        public override IAveIisSmtpServer CreateIisSmtpServer()
        {
            return new AveIisSmtpServer();
        }

        public override IAveMonthlySchedule CreateMonthlySchedule()
        {
            return new AveMonthlySchedule();
        }

        public override IAveMonthlyByDaySchedule CreateMonthlyByDaySchedule()
        {
            return new AveMonthlyByDaySchedule();
        }

        public override IAveProductVersions CreateProductVersions()
        {
            return new AveProductVersions();
        }

        public override IAveServiceInstanceJobDefinition CreateServiceInstanceJobDefinition(IAveServiceInstance serviceInstance, bool provision)
        {
            return new AveServiceInstanceJobDefinition(serviceInstance, provision);
        }

        public override IAveProcessAccount CreateProcessAccount()
        {
            return new AveProcessAccount();
        }

        public override IAveSkuUpgradeJob CreateSkuUpgradeJob()
        {
            return new AveSkuUpgradeJob();
        }

        public override IAveSkuUpgradeJob CreateSkuUpgradeJob(string name, IAveService service)
        {
            return new AveSkuUpgradeJob(name, service);
        }

        public override IAveSkuUpgradePage CreateSkuUpgradePage()
        {
            return new AveSkuUpgradePage();
        }

        public override IAveOSetupLicensing CreateSetupLicensing()
        {
            return new AveOSetupLicensing();
        }

        public override IAveSecurityContext CreateSecurityContext()
        {
            return new AveSecurityContext();
        }

        public override IAveServer CreateServer()
        {
            return new AveServer();
        }

        public override IAveSmtpSettingsPushJobDefinition CreateSmtpSettingsPushJobDefinition(string name, IAveService service)
        {
            return new AveSmtpSettingsPushJobDefinition(name, service);
        }

        public override IAveTrustedRootAuthorityManager CreateTrustedRootAuthorityManager()
        {
            return new AveTrustedRootAuthorityManager();
        }

        public override IAveWeeklySchedule CreateWeeklySchedule()
        {
            return new AveWeeklySchedule();
        }

        public override IAveStringResourceManager CreateStringResourceManager()
        {
            return new AveStringResourceManager();
        }

        public override IAveOPolicyCatalog CreatePolicyCatalog()
        {
            return new AveOPolicyCatalog();
        }

        public override IAveSolutionDeploymentJobDefinition CreateSolutionDeploymentJobDefinition()
        {
            return new AveSolutionDeploymentJobDefinition();
        }

        public override IAveWorkflowAssociation CreateWorkflowAssociation()
        {
            return new AveWorkflowAssociation();
        }

        public override IAveWorkflowDefinition CreateWorkflowDefinition() 
        {
            return new AveWorkflowDefinition();
        }

        public override IAveWorkflowSubscription CreateWorkflowSubscription() 
        {
            return new AveWorkflowSubscription();
        }

        public override IAveSolution CreateSolution()
        {
            return new AveSolution();
        }

        public override IAveFieldId CreateFieldId()
        {
            return new AveFieldId();
        }

        public override IAveOULS CreateULS()
        {
            return new AveOULS();
        }

        public override IAveOKeywords CreateKeywords(IAveOSearchServiceApplicationProxy searchAdminProxy, Uri url)
        {
            return new AveOKeywords(searchAdminProxy, url);
        }

        public override IAveScheduledItem CreateScheduledItem()
        {
            return new AveScheduledItem();
        }

        public override IAveLegacyListTemplate CreateLegacyListTemplate()
        {
            return new AveLegacyListTemplate();
        }

        public override IAveNavigationSiteMapNode CreateNavigationSiteMapNode()
        {
            return new AveNavigationSiteMapNode();
        }

        public override IAveSite CreateSite(Guid siteId, IAveUserToken userToken)
        {
            return new AveSite(siteId, userToken);
        }

        public override IAveOAudienceManager CreateAudienceManager(IAveServiceContext serviceContext)
        {
            return new AveOAudienceManager(serviceContext);
        }

        public override IAveSiteAdministration CreateSiteAdministration(string url)
        {
            return new AveSiteAdministration(url);
        }

        public override IAveORemoteScopes CreateRemoteScopes(IAveServiceContext context)
        {
            return new AveORemoteScopes(context);
        }

        public override IAveOScopeInfo CreateScopeInfo()
        {
            return new AveOScopeInfo();
        }

        public override IAveOManagedPropertyInfo CreateManagedPropertyInfo()
        {
            return new AveOManagedPropertyInfo();
        }

        public override IAveORuleInfo CreateRuleInfo()
        {
            return new AveORuleInfo();
        }

        public override IAveODisplayGroupInfo CreateDisplayGroupInfo()
        {
            return new AveODisplayGroupInfo();
        }

        public override IAvePeopleEditor CreatePeopleEditor()
        {
            return new AvePeopleEditor();
        }

        public override IAveOScopesUtilities CreateScopesUtilities()
        {
            return new AveOScopesUtilities();
        }

        public override IAveOKeywordHelper CreateKeywordHelper(string siteId)
        {
            return new AveOKeywordHelper(siteId);
        }

        public override IAveOKeywordHelper CreateKeywordHelper(string siteId, IAveServiceContext serviceContext)
        {
            return new AveOKeywordHelper(siteId, serviceContext);
        }

        public override IAveOUserContextHelper CreateUserContextHelper(string siteID)
        {
            return new AveOUserContextHelper(siteID);
        }

        public override IAveOUserProfilePropertyHelper CreateUserProfilePropertyHelper()
        {
            return new AveOUserProfilePropertyHelper();
        }

        public override IAveListItemCollectionPosition CreateListItemCollectionPosition(string pageInfo)
        {
            return new AveListItemCollectionPosition(pageInfo);
        }

        public override IAveOBestBetHelper CreateBestBetHelper(string siteID, IAveServiceContext serviceContext)
        {
            return new AveOBestBetHelper(siteID, serviceContext);
        }

        public override IAveOFeaturedContentHelper CreateFeaturedContentHelper(string siteID, IAveServiceContext serviceContext)
        {
            return new AveOFeaturedContentHelper(siteID, serviceContext);
        }

        public override IAveFeatureProperty CreateFeatureProperty(string propName, string propValue)
        {
            return new AveFeatureProperty(propName, propValue);
        }

        public override IAveORankPromotionHelper CreateRankPromotionHelper(string siteID, IAveServiceContext serviceContext)
        {
            return new AveORankPromotionHelper(siteID, serviceContext);
        }

        public override IAveContentDatabase CreateContentDatabase()
        {
            return new AveContentDatabase();
        }

        public override IAveODocIdUiSettings CreateDocIdUiSettings()
        {
            return new AveODocIdUiSettings();
        }

        public override IAveODocIdUiSettings CreateDocIdUiSettings(bool assignmentEnabled, string prefix)
        {
            return new AveODocIdUiSettings(assignmentEnabled, prefix);
        }

        public override IAveODocIdLookup CreateDocIdLookup()
        {
            return new AveODocIdLookup();
        }

        public override IAveOOobProvider CreateOobProvider()
        {
            return new AveOOobProvider();
        }

        public override IAveCommonUtilities CreateCommonUtilities()
        {
            return new AveCommonUtilities();
        }

        public override IAveODocumentId CreateDocumentId()
        {
            return new AveODocumentId();
        }

        public override IAveItem CreateAveItem(AveBaseItemInfo info, IAveFolder folder, IAveWeb web, IAveList list)
        {
            return new AveItem(info, folder, web, list);
        }

        public override IAveAttachment CreateAttachment(AveAttachmentInfo info, IAveListItem item)
        {
            return new AveAttachment(info, item);
        }

        public override IAveRegister CreateAveRegister()
        {
            return new AveRegister();
        }
        public override IAveConfigurationDatabase CreateConfigurationDatabase()
        {
            return new AveConfigurationDatabase();
        }

        public override IAveItem CreateAveItem(IAveSite iAveSite)
        {
            return new AveItem(iAveSite);
        }

        public override IAveWebTemplateCollection CreateWebTemplateCollection(string xmlWebTemplates, uint LCID)
        {
            return new AveWebTemplateCollection(xmlWebTemplates, LCID);
        }

        public override IAveCertificateValidator CreateCertificateValidator()
        {
            return new AveCertificateValidator();
        }

        public override IAveLimitedWebPartManager CreateLimitedWebPartManager(IAveSite site, IAveWeb web, IAveFile file)
        {
            return new AveLimitedWebPartManager(site, web, file);
        }

        public override IAveLimitedWebPartManager CreateLimitedWebPartManager(IAveSite site, IAveWeb web, string fileServerRelativeUrl)
        {
            return new AveLimitedWebPartManager(site, web, fileServerRelativeUrl);
        }

        public override IAveSiteSubscriptionSettings CreateSiteSubscriptionSettings()
        {
            return new AveSiteSubscriptionSettings();
        }

        public override IAveExportSettings CreateExportSettings(Uri url, string tempFileFolder, string tempFileName)
        {
            return new AveExportSettings(url, tempFileFolder, tempFileName);
        }

        public override IAveExportObject CreateExportObject(Guid objId, AveDeploymentObjectType objType, Guid parentObjId, bool excludeChildren)
        {
            return new AveExportObject(objId, objType, parentObjId, excludeChildren);
        }

        public override IAveExport CreateExport(IAveExportSettings exportSettings)
        {
            return new AveExport(exportSettings);
        }

        public override IAvePublishing CreatePublishing(IAveSite site)
        {
            return new AvePublishing(site);
        }

        //public override IAvePublishing CreatePublishing(string siteUrl)
        //{
        //    return new AvePublishing(new AveSite(siteUrl));
        //}

        public override IAveFieldLookupValue CreateFieldLookupValue()
        {
            return new AveFieldLookupValue();
        }

        public override IEcmDocumentRouting EcmDocumentRouting()
        {
            return new AveEcmDocumentRouting();
        }

        public override IAvePersistedTypeCollection<T> CreatePersistedTypeCollection<T>(IAveFarm farm)
        {
            return new AvePersistedTypeCollection<T>(farm);
        }

        public override IAveSolutionLanguagePack CreateSoluctionLanguagePack()
        {
            return new AveSolutionLanguagePack();
        }

        public override IAveMetaDataServiceSerializer CreateMetadataServiceSerilizer(Guid serviceAppId)
        {
            return new AveMetaDataServiceSerializer(serviceAppId);
        }

        public override IAveMetadataServiceRestorer CreateMetadataServiceRestorer(Guid serviceAppId)
        {
            return new AveMetadataServiceRestorer(serviceAppId);
        }

        public override IAveMetadataServiceApplication CreateMetadataServiceApplication(string name)
        {
            return new AveMetadataServiceApplication(name);
        }
        public override IAveMetadataServiceApplication CreateMetadataServiceApplication(string name, Guid defaultParititionId)
        {
            return new AveMetadataServiceApplication(name, defaultParititionId);
        }
        public override IAveMetadataServiceApplication CreateMetadataServiceApplication(IAveSite site)
        {
            throw new NotImplementedException();
        }

        public override IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount)
        {
            return new AveDiscoverQuery(siteUrl, conn, bposAccount);
        }

        public override IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount, DateTime startTime, DateTime endTime)
        {
            return new AveDiscoverQuery(siteUrl, conn, bposAccount, startTime, endTime);
        }

        [Obsolete("Use it with date time")]
        public override IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DiscoverModule module)
        {
            return new AveDiscoverQuery(site, module);
        }

        public override IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DateTime startTime, DateTime endTime, DiscoverModule module)
        {
            return new AveDiscoverQuery(site, startTime, endTime, module);
        }

        public override IAveContentTypePublisher CreateContentTypePublisher()
        {
            return new AveContentTypePublisher();
        }

        public override IAveContentTypePublisher CreateContentTypePublisher(IAveSite site)
        {
            return new AveContentTypePublisher(site);
        }

        public override IAveContentTypePublisher CreateContentTypePublisher(IAveTermStore store)
        {
            return new AveContentTypePublisher(store);
        }

        public override IAveMetadataServiceApplication CreateMetadataServiceApplication(Guid applicationId)
        {
            return new AveMetadataServiceApplication(applicationId);
        }
        public override IAveMetadataServiceApplication CreateMetadataServiceApplication(Guid applicationId, Guid defaultPartitionId)
        {
            return new AveMetadataServiceApplication(applicationId, defaultPartitionId);
        }
        public override IAveSOIntegrationUtility CreateSOIntegrationUtility()
        {
            return new AveSOIntegrationUtility();
        }
        public override IAveSOIntegrationUtility CreateSOIntegrationUtility(IAveSite site, IAveList list)
        {
            return new AveSOIntegrationUtility(site, list);
        }
        //public override IAveSOIntegrationUtility CreateSOIntegrationUtility(AveStorageInfo storageInfo, IAveSite site)
        //{
        //    return new AveSOIntegrationUtility();
        //}

        //public override IAveSOIntegrationUtility CreateSOIntegrationUtility(AveStorageInfo13 storageInfo, IAveSite site)
        //{
        //    return new AveSOIntegrationUtility(storageInfo, site);
        //}

        public override IAveElementProvider CreateElementProvider()
        {
            return new AveElementProvider();
        }

        public override object CreateSOIntegrationAPI()
        {
            return AveAssemblyUtility.CreateInstance(SOIntegrationAssemblyName, SOIntegrationNameSpace + "StorageOptimizationIntegration", new Type[] { }, new object[] { });
        }

        public override IAveEventManager CreateEventManager()
        {
            return new AveEventManager();
        }

        public override IAveWebPartPagesWebService CreateWebPartPagesWebService(IAveWeb web)
        {
            return new AveWebPartPagesWebService(web);
        }

        public override IAveFormsServicesWebService CreateFormsServicesWebService(IAveWeb web) 
        {
            throw new NotImplementedException();
        }

        public override IAveWrapperWorkflowService CreateWorkflowService()
        {
            return new AveWrapperWorkflowService();
        }

        public override IAveWorkflowServicesManager CreateWorkflowServicesManager(IAveWeb web)
        {
            return new AveWorkflowServicesManager(web);
        }

        [Obsolete]
        public override IAveBrowserQuery CreateBrowserQuery(string siteUrl, AveSqlConnection sqlConn)
        {
            return CreateBrowserQuery(siteUrl, sqlConn.ConnectionString);
        }

        public override IAveBrowserQuery CreateBrowserQuery(string siteUrl, string connectString)
        {
            return new AveBrowserQuery(siteUrl, connectString);
        }

        //plug in, have to use reflection
        public override Object CreateConnectorInegration()
        {
            return AveAssemblyUtility.CreateInstance(SOConnectorIntegrationAssemblyName, SOConnectorIntegrationNameSpace + "ConnectorItemRestore", new Type[] { }, new object[] { });
        }

        public override IAveOContent CreateContent(IAveOSearchServiceApplication searchApp)
        {
            return new AveOContent(searchApp);
        }

        public override IAveORanking CreateRanking(IAveOSearchServiceApplication searchApp)
        {
            return new AveORanking(searchApp);
        }
        public override IAveORanking CreateRanking(IAveOSearchServiceApplication searchApp,IAveOSearchObjectOwner searchOwner)
        {
            return new AveORanking(searchApp, searchOwner);
        }
        public override IAveOSearchObjectOwner CreateSearchOwner(AveOSearchObjectLevel objectLevel,IAveWeb aveWeb)
        {
            return new AveOSearchObjectOwner(objectLevel,aveWeb);
        }
        public override IAveOCrawlLogFilters CreateCrawlLogFilters()
        {
            return new AveOCrawlLogFilters();
        }

        public override IAveOLogViewer CreateLogViewer(IAveOSearchServiceApplication searchApp)
        {
            return new AveOLogViewer(searchApp);
        }

        public override IAveODailySchedule CreateODailySchedule(IAveOSearchServiceApplication searchApp)
        {
            return new AveODailySchedule(searchApp);
        }

        public override IAveOMonthlyDateSchedule CreateMonthlyDateSchedule(IAveOSearchServiceApplication searchApp)
        {
            return new AveOMonthlyDateSchedule(searchApp);
        }

        public override IAveOWeeklySchedule CreateOWeeklySchedule(IAveOSearchServiceApplication searchApp)
        {
            return new AveOWeeklySchedule(searchApp);
        }

        public override IAveOApplicationRegistry CreateApplicationRegistry()
        {
            return new AveOApplicationRegistry();
        }

        public override IAveLinksCheckerJob CreateLinksCheckerJob(IAveService service)
        {
            return new AveLinksCheckerJob(service);
        }

        public override IAveListItemSerializer CreateListItemSerializer(IAveSite site, IAveWeb web, IAveList list)
        {
            return new AveListItemSerializer(site as AveSite, web as AveWeb, list as AveList);
        }

        public override IAveUserProfileSerializer CreateUserProfileSerializer(IAveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo)
        {
            return new AveUserProfileSerializer(site as AveSite, login, needInit, sourceSiteInfo, this);
        }

        public override T CreateQueryService<T>(object arg)
        {
            return AveQueryServiceProvider.Instance<T>(arg);
        }

        public override IAveOHold CreateHold()
        {
            return new AveOHold();
        }

        public override IAveOUserProfileManager CreateUserProfileManager(IAveOServerContext context)
        {
            throw new NotImplementedException();
        }

        public override IAveOServerContext CreateServerContext()
        {
            throw new NotImplementedException();
        }
        public override IAveServiceContext CreateServerContext(AveServiceContextInfo contextInfo)
        {
            return new AveServiceContext().GetContext(contextInfo.WebApplication.ServiceApplicationProxyGroup, contextInfo.SiteSubscriptionIdentifier.Default);
        }

        public override IAveServiceContext CreateServiceContext(IAveServiceApplicationProxyGroup proxyGroup, IAveSiteSubscriptionIdentifier identifier)
        {
            return new AveServiceContext().GetContext(proxyGroup, identifier);
        }

        public override IAveOServerFarm CreateServerFarm()
        {
            return new AveOServerFarm();
        }

        public override IAveOLocation CreateLocation(string name, IAveOSearchServiceApplicationProxy searchProxy)
        {
            return new AveOLocation(name, searchProxy);
        }

        public override IAveOLocationList CreateLocationList()
        {
            return new AveOLocationList();
        }

        public override IAveOQueryManager CreateQueryManager()
        {
            return new AveOQueryManager();
        }

        public override IAveOSearchServiceApplication CreateSearchServiceApplication()
        {
            return new AveOSearchServiceApplication();
        }

        public override IAveSearchService CreateSearchService(string name, IAveFarm farm)
        {
            return new AveSearchService(name, farm);
        }


        public override IAveOScopes CreateScopes(IAveOSearchContext searchContext)
        {
            return new AveOScopes(searchContext);
        }

        public override IAveOScopes CreateScopes(IAveOSearchServiceApplication searchServiceApplication)
        {
            return new AveOScopes(searchServiceApplication);
        }

        public override IAveOUserProfileService CreateUserProfileService()
        {
            return new AveOUserProfileService();
        }

        public override IAveOSchema CreateSchema(IAveOSearchServiceApplication aveOSearchServiceApplication)
        {
            return new AveOSchema(aveOSearchServiceApplication);
        }

        public override IAveOSchema CreateSchema(IAveOSearchContext aveOSearchContext)
        {
            return new AveOSchema(aveOSearchContext);
        }

        public override IAveWorkflowCollection CreateWorkflowCollection(IAveList list, Guid associationId)
        {
            return new AveWorkflowCollection(list, associationId);
        }

        public override IAveWorkflowCollection CreateWorkflowCollection(IAveWeb web)
        {
            return new AveWorkflowCollection(web);
        }

        public override IAveMeeting CreateMeeting()
        {
            return new AveMeeting();
        }

        public override IAveOSetFormsServiceCmdlet CreateSetFormsServiceCmdlet()
        {
            return new AveOSetFormsServiceCmdlet();
        }
        public override IAveOContentIterator CreateContentIterator()
        {
            return new AveOContentIterator();
        }

        public override IAveContentTypePageUtil CreateContentTypePageUtil()
        {
            return new AveContentTypePageUtil();
        }

        public override IAveOPolicyItemCollection CreateOPolicyItemCollection(IAveOPolicy policy)
        {
            return new AveOPolicyItemCollection(policy);
        }

        public override IAveOExpiration CreateExpiration()
        {
            return new AveOExpiration();
        }

        public override IAveOPolicyAudit CreatePolicyAudit()
        {
            return new AveOPolicyAudit();
        }

        public override IAveExecutionTimeCounter CreateExecutionTimeCounter()
        {
            return new AveExecutionTimeCounter();
        }

        public override IAveExecutionTimeCounter CreateExecutionTimeCounter(uint maxValue)
        {
            return new AveExecutionTimeCounter(maxValue);
        }

        public override IAveOPolicy CreateOPolicy()
        {
            return new AveOPolicy();
        }
        public override IAveONewCustomConnector CreateNewCustomConnector()
        {
            return new AveONewCustomConnector();
        }

        public override IAveORemoveCustomConnector CreateRemoveCustomConnector()
        {
            return new AveORemoveCustomConnector();
        }

        public override IAveOSearchAdminUtils CreateSearchAdminUtils()
        {
            return new AveOSearchAdminUtils();
        }

        public override IAveOUserProfileManager CreateUserProfileManager(IAveServiceApplication application)
        {
            return new AveOUserProfileManager(application, null);
        }

        public override IAveOMapping CreateMapping()
        {
            return new AveOMapping();
        }

        public override IAveOMapping CreateMapping(Guid crawledPropset, string crawledPropertyName, int crawledPropertyVariantType, int managedPid)
        {
            return new AveOMapping(crawledPropset, crawledPropertyName, crawledPropertyVariantType, managedPid);
        }

        public override IAveOPropagation CreatePropagation(IAveOSearchServiceApplication searchServiceApplication)
        {
            // add by adrian
            throw new NotImplementedException();
        }

        public override IAveOTopologySettings CreateTopologySettings()
        {
            return new AveOTopologySettings();
        }
        public override IAveOTopologySettings CreateTopologySettings(IAveOSearchServiceApplication searchApplication)
        {
            return new AveOTopologySettings(searchApplication);
        }
        public override IAveOCrawlComponentSettings CreateCrawlComponentSettings()
        {
            return new AveOCrawlComponentSettings();
        }

        public override IAveOSearchApplicationSystemStatus CreateSearchApplicationSystemStatus()
        {
            // add by adrian
            return new AveOSearchApplicationSystemStatus();
        }

        public override IAveOCrawlLogData CreateCrawlLogData(IAveOSearchServiceApplication searchApp)
        {
            return new AveOCrawlLogData(searchApp);
        }

        public override IAveUsageApplicationProxy CreateUsageApplicationProxy()
        {           
            return new AveUsageApplicationProxy();            
        }


        public override IAveProjectPolicyItemListUtility CreatePolicyItemListUtility()
        {
            return new AveProjectPolicyItemListUtility();
        }

        public override IAveVariationSettingsFactory CreateVariationSettingsFactory(IAveSite site, AveVariationSettings settings)
        {
            return new AveVariationSettingsFactory(site, settings);
        }

        public override IAveSEOSetting CreateAveSEOSetting(IAveSite mAveSite)
        {
            return new AveSEOSetting(mAveSite);
        }

        public override IAveOStringResourceManager CreateOStringResourceManager()
        {
            return new AveOStringResourceManager();
        }

        public override IAveEventReceiverBase CreateEventReceiverBase()
        {
            return new AveEventReceiverBase();
        }

        public override IAveContentType AddSameParentContentType(IAveContentTypeCollection collection, IAveContentType contentType)
        {
            SPContentTypeCollection spCollection = (collection as AveContentTypeCollection).ContentTypeCollection;
            SPContentType spContentType = (contentType as AveContentType).ContentType;
            AveAssemblyUtility.InvokeMethod(spCollection, "UpdateContentType", new object[] { spContentType, true });
            return new AveContentType(collection as AveContentTypeCollection, spContentType);
        }

        #region add for SP2013
        public override IAveReputationHelper CreateReputationHelper()
        {
            return new AveReputationHelper();
        }

        public override IAveUserSettingsProviderManager CreateUserSettingsProviderManager()
        {
            return new AveUserSettingsProviderManager();
        }

        public override IAveTheme CreateTheme()
        {
            return new AveTheme();
        }

        public override IAveColor CreateColor()
        {
            return new AveColor();
        }

        public override IAveFont CreateFont()
        {
            return new AveFont();
        }

        public override IAveOSearchObjectFilter CreateSearchObjectFilter(IAveOSearchObjectOwner searchObjectOwner)
        {
            return new AveOSearchObjectFilter(searchObjectOwner);
        }


        public override IAveOSearchObjectOwner CreateSearchOwner(AveOSearchObjectLevel objectLevel)
        {
            return new AveOSearchObjectOwner(objectLevel);
        }

        public override IAveFederationManager CreateFederationManager(IAveOSearchServiceApplication searchServiceApplication)
        {
            return new AveFederationManager(searchServiceApplication);
        }

        public override IAveOSearchProvider CreateSearchProvider()
        {
            return new AveOSearchProvider();
        }

        public override IAveOSharedSearchBoxSettings CreateSharedSearchBoxSettings()
        {
            return new AveOSharedSearchBoxSettings();
        }

        public override IAveOSocialFeedManager CreateSocialFeedManager()
        {
            return new AveOSocialFeedManager();
        }

        public override IAveOSocialFeedManager CreateSocialFeedManager(IAveOUserProfile up, IAveServiceContext ctx)
        {
            return new AveOSocialFeedManager(up, ctx);
        }

        public override IAveOSocialFeedOptions CreateSocialFeedOptions()
        {
            return new AveOSocialFeedOptions();
        }

        public override IAveServiceContextScope CreateServiceContextScope(IAveServiceContext serverContextScop)
        {
            return new AveServiceContextScope(serverContextScop);
        }

        public override IAveOSocialPostCreationData CreateSocialPostCreationData()
        {
            return new AveOSocialPostCreationData();
        }

        public override IAveOSocialAttachment CreateSocialAttachment()
        {
            return new AveOSocialAttachment();
        }
        #endregion


        public override IAveOSocialFollowingManager CreateSPSocialFollowingManager(IAveOUserProfile profile, IAveServiceContext context)
        {
            return new AveOSocialFollowingManager(profile, context);
        }

        public override IAveOSocialActorInfo CreateSPSocialActorInfo(AveSocialActorInfo actor)
        {
            return new AveOSocialActorInfo(actor);
        }

        public override IAveSiteDataQuery CreateSiteDataQuery()
        {
            return new AveSiteDataQuery();
        }

        public override IAveAuditQuery CreateAuditQuery(IAveSite site)
        {
            return new AveAuditQuery(site);
        }

        public override IAveImageRenditionCollection CreateImageRenditionCollection()
        {
            return new AveImageRenditionCollection();
        }

        public override IAveWorkflowInventoryUpgrade CreateWorkflowInventoryUpgrade()
        {
            return new AveWorkflowInventoryUpgrade();
        }

        #region  Add for SP App
        
        public override IAveAppCatalog CreateAppCatalog()
        {
            return new AveAppCatalog();
        }
        #endregion

        public override IAveChangeQuery CreateChangeQuery(bool allChangeObjectTypes, bool allChangeTypes)
        {
            return new AveChangeQuery(allChangeObjectTypes,allChangeTypes);
        }

        public override IAveChangeToken CreateChangeToken(AveCollectionScope scope, Guid scopeId, DateTime changeTime)
        {
            return new AveChangeToken(scope,scopeId,changeTime);
        }

        public override IAveChangeToken CreateChangeToken(string strChangeToken)
        {
            return new AveChangeToken(strChangeToken);
        }

        public override IAvePublishingPage CreatePublishingPage(IAveListItem item)
        {
            return new AvePublishingPage(item);
        }

        public override IAveRecycleBinQuery CreateRecycleBinQuery()
        {
            return new AveRecycleBinQuery();
        }

        public override IAveTenant CreateTenant(IAveSite site)
        {
            throw new NotImplementedException();
        }

        public override IAveTenant CreateTenant(string url, bool isOnline = true, bool needLoadProperties = false)
        {
            throw new NotImplementedException();
        }

        public override IAveOSocialDataItem CreateSocialDataItem()
        {
            return new AveOSocialDataItem();
        }

        public override IAveOSocialDataItem[] CreateSocialDataItemCollection(int count)
        {
            return new AveOSocialDataItem[count];
        }

        public override IAveOUserProfileChangeToken CreateUserProfileChangeToken(DateTime date)
        {
            return new AveOUserProfileChangeToken(date);
        }

        public override IAveOUserProfileChangeToken CreateUserProfileChangeToken(string changeToken)
        {
            return new AveOUserProfileChangeToken(changeToken);
        }

        public override IAveOUserProfileChangeToken CreateUserProfileChangeToken(long eventId, DateTime changeTime)
        {
            return new AveOUserProfileChangeToken(eventId, changeTime);
        }

        public override IAveOUserProfileChangeQuery CreateUserProfileChangeQuery()
        {
            return new AveOUserProfileChangeQuery();
        }


        public override IAveOUserProfileSubTypeManager CreateProfileSubTypeManager(IAveServiceContext serviceContext)
        {
            return new AveOUserProfileSubTypeManager().Get(serviceContext);
        }
        public override IAveOriginalIssuers CreateOriginalIssuers()
        {
            return new AveOriginalIssuers();
        }

        public override IAveClaimTypes CreateClaimTypes()
        {
            return new AveClaimTypes();
        }

        public override IAveClaim CreateClaim(string type, string value, string valueType, string originalIssuer)
        {
            return new AveClaim(type, value, valueType, originalIssuer);
        }

        public override IAveOUserProfilePropertyManager CreateUserProfilePropertyManager(IAveServiceContext serviceContext)
        {
            return new AveOUserProfilePropertyManager(serviceContext);
        }

        public override IAveUserProfileSerializer CreateUserProfileSerializer(IAveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo, Func<string, string> userMapping)
        {
            return new AveUserProfileSerializer(site as AveSite, login, needInit, sourceSiteInfo, this,userMapping);
        }
        public override IAveSPCommentStorage CreateSPCommentStorage(IAveSite stie)
        {
            return new AveSPCommentStorage(stie);
        }

        public override IAveExportObject CreateExportObject()
        {
            return new AveExportObject();
        }
        
        public override ISharePointDataProcessor CreateSharepointDataProcessor(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo SourceSiteInfo, Func<string, string> GetUserFromMapping)
        {
            return new SharePointDocumentDataProcessor(site, mapping, SourceSiteInfo);
        }

        public override IAveAzurePowerShellRequest CreateAzurePowerShellRequest(AveBPOSAccountInfo accountInfo)
        {
            throw new NotImplementedException();
        }

        public override string GetAdminUrl(AveBPOSAccountInfo accountInfo)
        {
            throw new NotImplementedException();
        }

        public override IAveProfileLoader CreateProfileLoader(string adminUrl)
        {
            throw new NotImplementedException();
        }

        public override IAveClientRequest CreateClientRequest(string url, AveBPOSAccountInfo userAccountInfo, AuthenticationModeOption[] authenticationModeOptions)
        {
            return null;
        }
        public override IAveOMetadataDefaults CreateMetadataDefaults(IAveSite aveSite, string columnName)
        {
            throw new NotImplementedException();
        }

        public override IAveSiteServiceHelper CreateSiteServiceHelper()
        {
            throw new NotImplementedException();
        }
    }
}
