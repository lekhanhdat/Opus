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
using System.IO;
using System.Security;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Common;
using AvePoint.ObjectModel.Common.Office;
using AvePoint.ObjectModel.Common.Workflow;
using AveClientRequest.Common;
using AvePoint.ObjectModel.Common;

namespace AvePoint.ObjectModel.Common
{
    public class AveClientObjectModelFactory : AveObjectModelFactory, IDisposable
    {
        private string mSiteUrl;
        private AveSite mAveSite;
        private AveBPOSAccountInfo mAccountInfo;
        private IAveUtility mUtility;
        private AveAPIType mAPIType;
        private IAveRequest mRequest;

        public override AveContextKind ContextKind
        {
            get { return AveContextKind.ClientObjectModel; }
        }

        public override bool IsSPInstalled
        {
            get { return false; }
        }

        public override IAveUtility Utility
        {
            get
            {
                if (mUtility == null)
                {
                    mUtility = new AveUtility();
                }
                return mUtility;
            }
        }

        public override AveAPIType APIType
        {
            get
            {
                //if (mAPIType != null)
                //{
                return mAPIType;
                //}
                //else
                //{
                //    return AveAPIType.Unknown;
            }
        }
    

        public override AveBPOSAccountInfo AccountInfo
        {
            get
            {
                return this.mAccountInfo;
            }
        }

        public AveClientObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo)
        {
            mSiteUrl = siteUrl;
            mAccountInfo = accountInfo;
            //if (mAccountInfo != null)
            //{
            //    log.Info("init clientOM, current user:{0}, admin url:{1}", mAccountInfo.UserName, mAccountInfo.AdminUrl);
            //}
        }

        public override IAveSite CreateSite()
        {
            mAveSite = new AveSite(mSiteUrl, mAccountInfo);
            if (mAveSite != null)
            {
                this.mAPIType = mAveSite.APIType;
                this.mRequest = mAveSite.Request;
            }            
            return mAveSite;
        }

        public override IAveSite CreateSite(string url)
        {
            // try
            //{
            mSiteUrl = url;
            mAveSite = new AveSite(url, mAccountInfo);
            if (mAveSite != null)
            {
                this.mAPIType = mAveSite.APIType;
                this.mRequest = mAveSite.Request;
            }
            return mAveSite;
        }

        public override IAveSite CreateSite(Guid siteId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSite CreateSite(string url, IAveUserToken token)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSite CreateSite(Guid id, AveUrlZone zone)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSite CreateAdminCenterSite(string url)
        {
            mSiteUrl = url;
            mAveSite = new AveSite(url, mAccountInfo, true);
            if (mAveSite != null)
            {
                this.mAPIType = mAveSite.APIType;
                this.mRequest = mAveSite.Request;
            }
            return mAveSite;
        }

        public override IAveTenant CreateTenant(IAveSite site)
        {
            return new AveTenant((site as AveSite).Request);
        }

        public override IAveTenant CreateTenant(string url)
        {
            return new AveTenant(url, mAccountInfo);
        }

        /// <summary>
        /// 获取Tenant下所有SC Properties: StorageQuotaUsage
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public override IAveTenant GetTenant(string url)
        {
            return new AveTenant(url, mAccountInfo, true);
        }

        public override IAveWebApplication CreateWebApplication()
        {
            return new AveWebApplication(mSiteUrl);
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveContentTypeIdPub CreateContentTypePub()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePublishingWeb CreatePublishingWeb(IAveWeb web)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePublishingWeb CreatePublishingWeb()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePublishingSite CreatePublishingSite()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveContentType CreateContentType(IAveContentTypeId contentTypeId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveTaxonomyFieldValue CreateTaxonomyFieldValue(IAveField field)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveTaxonomyFieldValue CreateTaxonomyFieldValue(string value)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }
        public override IAveTaxonomyFieldValueCollection CreateTaxonomyFieldValueCollection(IAveField field)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }
        public override IAveSecurity CreateSecurity()
        {
            return new AveSecurity();
        }

        public override IAveWebService CreateWebService()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWebService CreateWebService(string name, IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveUserCodeService CreateUserCodeService()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveUserCodeService CreateUserCodeService(IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveRoleAssignment CreateRoleAssignment(IAveUser user)
        {
            return new AveRoleAssignment(user as AvePrincipal);
        }

        public override IAveAdministrationWebApplication CreateAdministrationWebApplication()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveItemEventReceiver CreateItemEventReceiver()
        {
            return new AveItemEventReceiver();
        }

        public override IAveAlternateUrlCollection CreatedAlternateUrlCollection(string name, IAveFarm local)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveAlternateUrl CreateAlternateUrl(string incomingUrl, AveUrlZone urlZone)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveAlternateUrl CreateAlternateUrl(Uri requestUri, AveUrlZone urlZone)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOMetadataNavigationSettings CreateMetadataNavigationSettings()
        {
            return new AveOMetadataNavigationSettings();
        }

        public override IAveOMetadataNavigationSettings CreateMetadataNavigationSettings(string xmlMetadataNavigationSettings)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOFieldIndexDictionary CreateFieldIndexDictionary()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveDateOptions CreateDateOptions(string localeId, AveCalendarType calendar, string workWeek, string firstDayOfWeek, string hijriAdjustment, string timeZoneSpan, string selectedDate)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOPolicyCatalog CreatePolicyCatalog(IAveSite site)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOSearchService CreateOSearchService()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOConfiguredView CreateConfiguredView(IAveView view, int index)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveONodeViewSettings CreateNodeViewSettings(IAveOViewSettingsCollection viewSettingsCollection, string uniqueNodeId, int folderId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveResource CreateResource()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOmsMobileFacade CreateOmsMobileFacade()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveCredentialDeployment CreateCredentialDeployment()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveGlobalAdmin CreateGlobalAdmin()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveDatabaseService CreateDatabaseService()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOMetadataNavigationHierarchy CreateMetadataNavigationHierarchy()
        {
            return new AveOMetadataNavigationHierarchy();
        }

        public override IAveOMetadataNavigationHierarchy CreateMetadataNavigationHierarchy(IAveField hierarchyField)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOMetadataNavigationKeyFilter CreateMetadataNavigationKeyFilter()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOMetadataNavigationKeyFilter CreateMetadataNavigationKeyFilter(IAveField hierarchyFiled)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOMetadataHierarchyNodeTaxonomy CreateMetadataHierarchyNodeTaxonomy()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOMetadataDefaults CreateMetadataDefaults(IAveList aveList)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }
        public override IAveOMetadataDefaults CreateMetadataDefaults(IAveSite aveSite,string columnName)
        {
            return new AveOMetadataDefaults(aveSite as AveSite, columnName);
        }
        //public override IAveClaimProviderOperations CreateClaimProviderOperations()
        //{
        //    throw new NotSupportedException("this constructor is not supported in BPOS mode");
        //}

        public override IAveServer CreateServer(string address)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveServer CreateServer(string address, IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveDatabaseServiceInstance CreateDatabaseServiceInstance(string name, IAveServer server, IAveDatabaseService service)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveAlternateUrlCollection CreateAlternateUrlCollection(string resourceName, IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePortalService CreatePortalService(string name, IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveFarmManagedAccountCollection CreateFarmManagedAccountCollection(IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOfficialFileHost CreateOfficialFileHost(bool bCreateUniqueId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSecurityTokenServiceManager CreateSecurityTokenServiceManager()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        /// <summary>
        /// Constructor Method for Webs in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override IAveWebCollection CreateWebs(ICredentials crendeantial, string url)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        /// <summary>
        /// Constructor Method for Lists in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override IAveListCollection CreateLists(ICredentials crendeantial, string url)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        /// <summary>
        /// Constructor Method for Views in AveObjectModelFactory
        /// </summary>
        /// <param name="crendeantial"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public override IAveViewCollection CreateViews(ICredentials crendeantial, string url)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveUpgradeSessionCollection CreateUpgradeSessionCollection(IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePersistedDependencyCollection<IAveIisWebServiceApplication> CreatePersistedDependencyCollection(IAveIisWebServiceApplicationPool iisWebServiceApplicationPool)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }


        public override IAveServiceApplicationProxyGroup CreateServiceApplicationProxyGroup()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWebApplicationProvisioningJobDefinition CreateWebApplicationProvisioningJobDefinition(IAveWebApplication app)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveIisWebsiteUnprovisioningJobDefinition CreateIisWebsiteUnprovisioningJobDefinition(bool deleteWebSites, string[] serverComments, string applicationPoolId, string[] vdirs, Guid webAppId, bool webAppUnprovisioning)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWebServiceCollection CreateWebServiceCollection(IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWebApplicationBuilder CreateWebApplicationBuilder(IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveIisSettings CreateIisSettings(string serverComment, bool allowAnonymous, bool disableKerberos, IAveServerBinding serverBinding, IAveSecureBinding secureBinding, DirectoryInfo path)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        /// <summary>
        /// Invoke object for calling static member;
        /// </summary>
        /// <returns></returns>
        public override IAveWebTemplate CreateWebTemplate()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveIisSettings CreateIisSettings()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveIisWebSite CreateIisWebSite(int instanceId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveIisWebSite CreateIisWebSite()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveIisApplicationPool CreateIisApplicationPool(string name)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveServiceProxy CreateServiceProxy(string name, IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMobileMessagingAccount CreateMobileMessagingAccount()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMobileMessagingAccount CreateMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMobileMessagingAccount CreateMobileMessagingAccount(string serviceName, string serviceUrl, string userId, SecureString password, IAveMobileMessageServiceProvider serviceProvider, IAveMobileMessageUserInfo userInfo)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveServiceApplicationProxyGroupCollection CreateServiceApplicationProxyGroupCollection(IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveDiagnosticsService CreateDiagnosticsService()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSiteCollectionCopier CreateSiteCollectionCopier(IAveContentDatabase dbFrom, IAveContentDatabase dbTo, List<IAveSite> colSites)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveQueryProvider CreateQueryProvider(Uri helpList, uint lcid)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveHelpContextManager CreateHelpContextManager()
        {
            return new AveHelpContextManager();
        }

        public override IAveDeliveryChannelSettings CreateDeliveryChannelSettings()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSQM CreateSQM()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveThmxTheme CreateThmxTheme(IAveSite site)
        {
            return new AveThmxTheme(site);
        }

        public override IAveClaimEncodingManager CreateClaimEncodingManager()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveClaimEntityTypes CreateClaimEntityTypes()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveClaimProviderManager CreateClaimProviderManager()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOneTimeSchedule CreateOneTimeSchedule(DateTime dt)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSchedule CreateSchedule()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOfficialFileSoap CreateOfficialFileSoap(Uri uri)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOfficialFileSoap CreateOfficialFileSoap(string url)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSecureBinding CreateSecureBinding()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveServerBinding CreateServerBinding()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWindowsAuthenticationProvider CreateWindowsAuthenticationProvider()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveFormsAuthenticationProvider CreateFormsAuthenticationProvider(string membershipProvider, string roleProvider)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveTrustedAuthenticationProvider CreateTrustedAuthenticationProvider(string providerName)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveQuotaTemplate CreateQuotaTemplate()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWorkflowManager CreateWorkflowManager()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveListViewWebPart CreateListViewWebPart()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            //return new AveOUserProfileManager(context as AveServiceContext, mAveSite as AveSite);
            throw new NotImplementedException();
        }

        public override IAveOSocialTagManager CreateSocialTagManager(IAveServiceContext context)
        {
            //return new AveOSocialTagManager(context);
            throw new NotImplementedException();
        }

        public override IAveOAlternateAccessMapping CreateOAlternateAccessMapping()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOUserProfileApplicationProxy CreateOUserProfileApplicationProxy()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOULS CreateOULF()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOSocialCommentManager CreateSocialCommentManager(IAveServiceContext context)
        {
            //return new AveOSocialCommentManager(context);
            throw new NotImplementedException();
        }

        public override IAveSecureString CreateSecurityString()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSecurityContext CreateSecurityContext(IntPtr priorToken)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveORecords CreateRecords()
        {
            return new AveORecords();
        }

        public override IAveOFormsService CreateFormsService()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveBlockedSolution CreateBlockedSolution(string fileName, string signature, string message)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveDatabaseSequence CreateDatabaseSequence()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOFormTemplateCollection CreateFormTemplateCollection()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveIisSmtpServer CreateIisSmtpServer()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMonthlySchedule CreateMonthlySchedule()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMonthlyByDaySchedule CreateMonthlyByDaySchedule()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveProductVersions CreateProductVersions()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveServiceInstanceJobDefinition CreateServiceInstanceJobDefinition(IAveServiceInstance serviceInstance, bool provision)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveProcessAccount CreateProcessAccount()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSkuUpgradeJob CreateSkuUpgradeJob()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSkuUpgradeJob CreateSkuUpgradeJob(string name, IAveService service)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSkuUpgradePage CreateSkuUpgradePage()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOSetupLicensing CreateSetupLicensing()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSecurityContext CreateSecurityContext()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveServer CreateServer()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSmtpSettingsPushJobDefinition CreateSmtpSettingsPushJobDefinition(string name, IAveService service)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveTrustedRootAuthorityManager CreateTrustedRootAuthorityManager()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWeeklySchedule CreateWeeklySchedule()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveStringResourceManager CreateStringResourceManager()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOPolicyCatalog CreatePolicyCatalog()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSolutionDeploymentJobDefinition CreateSolutionDeploymentJobDefinition()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWorkflowAssociation CreateWorkflowAssociation()
        {
            //throw new NotSupportedException("this constructor is not supported in BPOS mode");
            return new AveWorkflowAssociation();
        }

        public override IAveWorkflowDefinition CreateWorkflowDefinition()
        {
            return new AveWorkflowDefinition();
        }

        public override IAveWorkflowServicesManager CreateWorkflowServicesManager(IAveWeb web)
        {
            return AveWorkflowServicesManager.CreateWorkflowServiceManager(web);
        }

        public override IAveWorkflowSubscription CreateWorkflowSubscription()
        {
            return new AveWorkflowSubscription();
        }

        public override IAveSolution CreateSolution()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveFieldId CreateFieldId()
        {
            return new AveFieldId();
        }

        public override IAveOULS CreateULS()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOKeywords CreateKeywords(IAveOSearchServiceApplicationProxy searchAdminProxy, Uri url)
        {
            return new AveOKeywords(searchAdminProxy, url);
        }

        public override IAveScheduledItem CreateScheduledItem()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOAudienceManager CreateAudienceManager(IAveServiceContext serviceContext)
        {
            return new AveOAudienceManager(serviceContext, mAveSite as AveSite);
        }

        public override IAveAuditQuery CreateAuditQuery(IAveSite site)
        {
            return null;
        }

        public override IAveSiteAdministration CreateSiteAdministration(string url)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveORemoteScopes CreateRemoteScopes(IAveServiceContext context)
        {
            return new AveORemoteScopes(context as AveServiceContext);
        }

        public override IAveOScopeInfo CreateScopeInfo()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOManagedPropertyInfo CreateManagedPropertyInfo()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveORuleInfo CreateRuleInfo()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveODisplayGroupInfo CreateDisplayGroupInfo()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePeopleEditor CreatePeopleEditor()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOScopesUtilities CreateScopesUtilities()
        {
            return new AveOScopesUtilities();
        }

        public override IAveOKeywordHelper CreateKeywordHelper(string siteId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOKeywordHelper CreateKeywordHelper(string siteId, IAveServiceContext serviceContext)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOUserContextHelper CreateUserContextHelper(string siteID)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOUserProfilePropertyHelper CreateUserProfilePropertyHelper()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveListItemCollectionPosition CreateListItemCollectionPosition(string pageInfo)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOBestBetHelper CreateBestBetHelper(string siteID, IAveServiceContext serviceContext)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOFeaturedContentHelper CreateFeaturedContentHelper(string siteID, IAveServiceContext serviceContext)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveORankPromotionHelper CreateRankPromotionHelper(string siteID, IAveServiceContext serviceContext)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveContentDatabase CreateContentDatabase()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveODocIdUiSettings CreateDocIdUiSettings()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveODocIdUiSettings CreateDocIdUiSettings(bool assignmentEnabled, string prefix)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveODocIdLookup CreateDocIdLookup()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOOobProvider CreateOobProvider()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveCommonUtilities CreateCommonUtilities()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveODocumentId CreateDocumentId()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveItem CreateAveItem(AveBaseItemInfo info, IAveFolder folder, IAveWeb web, IAveList list)
        {
            return new AveItem(info, folder, web, list);
        }

        public override IAveAttachment CreateAttachment(AveAttachmentInfo info, IAveListItem item)
        {
            //throw new NotSupportedException("this constructor is not supported in BPOS mode");
            return new AveAttachment(info, item);
        }

        public override IAveConfigurationDatabase CreateConfigurationDatabase()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveItem CreateAveItem(IAveSite iAveSite)
        {
            return new AveItem(iAveSite);
        }

        public override IAveWebTemplateCollection CreateWebTemplateCollection(string xmlWebTemplates, uint LCID)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveCertificateValidator CreateCertificateValidator()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
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
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveExportSettings CreateExportSettings(Uri url, string tempFileFolder, string tempFileName)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveExportObject CreateExportObject(Guid objId, AveDeploymentObjectType objType, Guid parentObjId, bool excludeChildren)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveExport CreateExport(IAveExportSettings exportSettings)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePublishing CreatePublishing(IAveSite site)
        {
            return new AvePublishing(site);
        }

        public override IAveFieldLookupValue CreateFieldLookupValue()
        {
            return new AveFieldLookupValue();
        }

        public override IEcmDocumentRouting EcmDocumentRouting()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAvePersistedTypeCollection<T> CreatePersistedTypeCollection<T>(IAveFarm farm)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSolutionLanguagePack CreateSoluctionLanguagePack()
        {
            return new AveSolutionLanguagePack();
        }

        public override IAveMetaDataServiceSerializer CreateMetadataServiceSerilizer(Guid serviceAppId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMetadataServiceRestorer CreateMetadataServiceRestorer(Guid serviceAppId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMetadataServiceApplication CreateMetadataServiceApplication(string name)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }


        public override IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount)
        {
            AveQuery query = new AveQuery(siteUrl, bposAccount);
            this.mRequest = query.Request;
            return query;
        }

        public override IAveDiscoveryQuery CreateDiscoveryQuery(string siteUrl, object conn, AveBPOSAccountInfo bposAccount, DateTime startTime, DateTime endTime)
        {
            AveQuery query = new AveQuery(siteUrl, bposAccount, startTime, endTime);
            this.mRequest = query.Request;
            return query;
        }

        public override IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DiscoverModule module)
        {
            AveQuery query = new AveQuery(site);
            this.mRequest = query.Request;
            return query;
        }

        public override IAveDiscoveryQuery CreateDiscoveryQuery(IAveSite site, DateTime startTime, DateTime endTime, DiscoverModule module)
        {
            AveQuery query = new AveQuery(site, startTime, endTime);
            mRequest = query.Request;
            return query;
        }

        public override IAveContentTypePublisher CreateContentTypePublisher()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveContentTypePublisher CreateContentTypePublisher(IAveSite site)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveContentTypePublisher CreateContentTypePublisher(IAveTermStore store)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveMetadataServiceApplication CreateMetadataServiceApplication(Guid applicationId)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveSOIntegrationUtility CreateSOIntegrationUtility()
        {
            return null;
        }

        public override IAveSOIntegrationUtility CreateSOIntegrationUtility(AveStorageInfo storageInfo, IAveSite site)
        {
            return null;
        }

        public override IAveElementProvider CreateElementProvider()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override object CreateSOIntegrationAPI()
        {
            return null;
        }

        public override IAveEventManager CreateEventManager()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveWebPartPagesWebService CreateWebPartPagesWebService(IAveWeb web)
        {
            //throw new NotSupportedException("this constructor is not supported in BPOS mode");
            return new AveWebPartPagesWebService(web);
        }

        public override IAveWorkflowService CreateWorkflowService()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveBrowserQuery CreateBrowserQuery(string siteUrl)
        {
            return new AveBrowserQuery(siteUrl,mAccountInfo);
        }

        public override object CreateConnectorInegration()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOContent CreateContent(IAveOSearchServiceApplication searchApp)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveORanking CreateRanking(IAveOSearchServiceApplication searchApp)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOCrawlLogFilters CreateCrawlLogFilters()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOLogViewer CreateLogViewer(IAveOSearchServiceApplication searchApp)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveODailySchedule CreateODailySchedule(IAveOSearchServiceApplication searchApp)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOMonthlyDateSchedule CreateMonthlyDateSchedule(IAveOSearchServiceApplication searchApp)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOWeeklySchedule CreateOWeeklySchedule(IAveOSearchServiceApplication searchApp)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOApplicationRegistry CreateApplicationRegistry()
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveLinksCheckerJob CreateLinksCheckerJob(IAveService service)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override T CreateQueryService<T>(object arg)
        {
            throw new NotSupportedException("this constructor is not supported in BPOS mode");
        }

        public override IAveOHold CreateHold()
        {
            throw new NotImplementedException();
        }

        public override IAveListItemSerializer CreateListItemSerializer(IAveSite site, IAveWeb web, IAveList list)
        {
            return new AveListItemSerializer(site as AveSite, web as AveWeb, list as AveList);
        }

        public override IAveUserProfileSerializer CreateUserProfileSerializer(IAveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo)
        {
            return new AveUserProfileSerializer(site as AveSite, login, needInit, sourceSiteInfo);
        }

        public override IAveOUserProfileManager CreateUserProfileManager(IAveOServerContext context)
        {
            throw new NotImplementedException();
        }

        public override IAveOServerContext CreateServerContext()
        {
            throw new NotImplementedException();
        }

        public override IAveOServerFarm CreateServerFarm()
        {
            throw new NotImplementedException();
        }

        public override IAveOLocation CreateLocation(string name, IAveOSearchServiceApplicationProxy searchProxy)
        {
            throw new NotImplementedException();
        }

        public override IAveOLocationList CreateLocationList()
        {
            throw new NotImplementedException();
        }

        public override IAveOQueryManager CreateQueryManager()
        {
            throw new NotImplementedException();
        }

        public override IAveOSearchServiceApplication CreateSearchServiceApplication()
        {
            throw new NotImplementedException();
        }

        public override IAveSearchService CreateSearchService(string name, IAveFarm farm)
        {
            throw new NotImplementedException();
        }

        public override IAveOScopes CreateScopes(IAveOSearchContext searchContext)
        {
            throw new NotImplementedException();
        }

        public override IAveOScopes CreateScopes(IAveOSearchServiceApplication searchServiceApplication)
        {
            throw new NotImplementedException();
        }

        public override IAveOUserProfileService CreateUserProfileService()
        {
            throw new NotImplementedException();
        }

        public override IAveOSchema CreateSchema(IAveOSearchServiceApplication aveOSearchServiceApplication)
        {
            throw new NotImplementedException();
        }

        public override IAveOSchema CreateSchema(IAveOSearchContext aveOSearchContext)
        {
            throw new NotImplementedException();
        }

        public override IAveWorkflowCollection CreateWorkflowCollection(IAveList list, Guid associationId)
        {
            throw new NotImplementedException();
        }

        public override IAveWorkflowCollection CreateWorkflowCollection(IAveWeb web)
        {
            throw new NotImplementedException();
        }

        public override IAveMeeting CreateMeeting()
        {
            throw new NotImplementedException();
        }

        public override IAveOSetFormsServiceCmdlet CreateSetFormsServiceCmdlet()
        {
            throw new NotImplementedException();
        }

        public override IAveOContentIterator CreateContentIterator()
        {
            throw new NotImplementedException();
        }

        public override IAveServiceContext CreateServerContext(AveServiceContextInfo contextInfo)
        {
            return new AveServiceContext();
        }

        public override IAveRegister CreateAveRegister()
        {
            return new AveRegister();
        }

        public override IAveContentTypePageUtil CreateContentTypePageUtil()
        {
            throw new NotImplementedException();
        }
        public override IAveExecutionTimeCounter CreateExecutionTimeCounter()
        {
            throw new NotImplementedException();
        }

        public override IAveExecutionTimeCounter CreateExecutionTimeCounter(uint maxValue)
        {
            throw new NotImplementedException();
        }

        public override IAveOPolicy CreateOPolicy()
        {
            throw new NotImplementedException();
        }

        public override IAveOPolicyItemCollection CreateOPolicyItemCollection(IAveOPolicy policy)
        {
            throw new NotImplementedException();
        }

        public override IAveOExpiration CreateExpiration()
        {
            throw new NotImplementedException();
        }

        public override IAveOPolicyAudit CreatePolicyAudit()
        {
            throw new NotImplementedException();
        }

        public override IAveONewCustomConnector CreateNewCustomConnector()
        {
            throw new NotImplementedException();
        }

        public override IAveORemoveCustomConnector CreateRemoveCustomConnector()
        {
            throw new NotImplementedException();
        }

        public override IAveOSearchAdminUtils CreateSearchAdminUtils()
        {
            throw new NotImplementedException();
        }

        public override IAveOUserProfileManager CreateUserProfileManager(IAveServiceApplication application)
        {
            throw new NotImplementedException();
        }

        public override IAveOMapping CreateMapping(Guid crawledPropset, string crawledPropertyName, int crawledPropertyVariantType, int managedPid)
        {
            throw new NotImplementedException();
        }

        public override IAveFormsServicesWebService CreateFormsServicesWebService(IAveWeb web)
        {
            return new AveFormsServicesWebService(web);
        }

        public override IAveAppCatalog CreateAppCatalog()
        {
            return new AveAppCatalog(mRequest);
        }

        public override IAveAppSerializer CreateAppSerializer(IAveWeb web, int restoreOption)
        {
            return new AveAppSerializer(mAveSite as AveSite, web as AveWeb, restoreOption);
        }

        public override IAveAttachmentSerializer CreateAttachmentSerializer(IAveList list, int restoreOption)
        {
            return new AveAttachmentSerializer(list as AveList, mRequest, (AveRestoreOption)restoreOption);
        }

        public override IAveChangeQuery CreateChangeQuery(bool allChangeObjectTypes, bool allChangeTypes)
        {
            return new AveChangeQuery(allChangeObjectTypes, allChangeTypes);
        }

        public override IAveChangeToken CreateChangeToken(AveCollectionScope scope, Guid scopeId, DateTime changeTime)
        {
            return new AveChangeToken(scope, scopeId, changeTime);
        }

        public override IAveChangeToken CreateChangeToken(string strChangeToken)
        {
            return new AveChangeToken(strChangeToken);
        }

        public override IAveTheme CreateTheme()
        {
            return null;
        }

        public override IAveProfileLoader CreateOLProfileLoader(string url)
        {
            return new AveProfileLoader(url, mAccountInfo);
        }

        public override IAveSiteServiceHelper CreateSiteServiceHelper()
        {
            return new AveSiteServiceHelper();
        }

        public override IAveListItem WrapperListItem(IAveList list, Dictionary<string, object> itemProperties)
        {
            return ((AveList)list).CreateListItemInstance(itemProperties);
        }

        public void Dispose()
        {
            if(mAveSite != null)
            {
                mAveSite.Dispose();
            }
        }
    }
}
