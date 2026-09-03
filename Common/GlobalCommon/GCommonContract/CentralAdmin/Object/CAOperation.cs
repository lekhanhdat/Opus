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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.Adonis.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter;
    using AvePoint.GCommon.Contract.CentralAdmin.Object.SharedServices.SearchService;
    using AvePoint.GCommon.Contract.CentralAdmin.Object.STSAdmin;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    #region Farm level begin
    [KnownType(typeof(SearchCondition))]
    [XmlInclude(typeof(SearchCondition))]
    [KnownType(typeof(AdminSearchCondition))]
    [XmlInclude(typeof(AdminSearchCondition))]
    [KnownType(typeof(CAFarmBlockedFileTypesOperation))]
    [KnownType(typeof(CAFarmAntivirusSettingsOperation))]
    [KnownType(typeof(CAFarmInfoPathFormServicesOperation))]
    [KnownType(typeof(ManageTrustOperation))]
    [KnownType(typeof(CAFarmAlternateAccessMappingOperation))]
    [KnownType(typeof(CAFarmConfigureMobileAccountOperation))]
    [KnownType(typeof(ConfigurePrivacyOptionsOperation))]
    [KnownType(typeof(CrossFirewallAccessZoneOperation))]
    [KnownType(typeof(IncomingEmailSettingOperation))]
    [KnownType(typeof(ServicesOfAllServersOperation))]
    [KnownType(typeof(OutgoingEmailSettingOperation))]
    [KnownType(typeof(DefaultDatabaseServerOperation))]
    [KnownType(typeof(DataRetrivalOperation))]
    [KnownType(typeof(CAFarmManageFarmSolutionsOperation))]
    [KnownType(typeof(ManageUserSolutionsOperation))]
    [KnownType(typeof(CACreateWebApplicationOperation))]
    [XmlInclude(typeof(CACreateWebApplicationOperation))]
    [KnownType(typeof(ManageServiceApplicationsOperation))]
    [KnownType(typeof(ManageDataConnectionFilesOperatoin))]
    [KnownType(typeof(CAFarmEnableEnterpriseFeaturesOperation))]
    [KnownType(typeof(ConvertFarmLicenseTypeOperation))]
    [KnownType(typeof(CAFarmWebpartSecurityOperation))]
    [KnownType(typeof(CAFarmServiceAccountOperation))]
    [KnownType(typeof(CAFarmSendToConnectionsOperation))]
    [KnownType(typeof(CAFarmChangePasswordSettingOperation))]
    [KnownType(typeof(CAFarmScanSiteDirectoryOperation))]
    [KnownType(typeof(CAFarmPolicyFeaturesOperation))]
    [KnownType(typeof(ReviewDatabaseStatusOperation))]
    [KnownType(typeof(CAFarmManagePatchStatusOperation))]
    [KnownType(typeof(CAFarmConfigureManagedAccountsOperation))]
    [KnownType(typeof(CAFarmLevelSearchSettingOperation))]
    [KnownType(typeof(CAFarmIrmSettingsOperation))]
    [KnownType(typeof(CAFarmSiteDirectorySettingsOperation))]
    [KnownType(typeof(CAFarmCrawlerImpactRulesOperation))]
    [KnownType(typeof(CAFarmHTMLViewerOperation))]
    [KnownType(typeof(CAFarmRecordsCenterOperation))]
    [KnownType(typeof(CAFarmTimerJobDefinitionsOperation))]
    [KnownType(typeof(CAFarmTimerJobStatusOperation))]
    [KnownType(typeof(CAFarmQuiesceFarmOperation))]
    #endregion

    #region WebApplication level begin
    [KnownType(typeof(CAWebApplicationQuotaTemplatesOperation))]
    [KnownType(typeof(CAWebApplicationDeleteOrphanSitesOperation))]
    [KnownType(typeof(CAManageWebApplicationGeneralSettingsOperation))]
    [KnownType(typeof(CAWebApplicationAuthenticationProvidersOperation))]
    [KnownType(typeof(CAWebApplicationAddContentDatabaseOperation))]
    [KnownType(typeof(CAWebApplicationExtendOperation))]
    [KnownType(typeof(CAWebApplicationSiteUseConfirmationDeletionOperation))]
    [KnownType(typeof(CAWebApplicationDeleteSharePonitFromIISWebSiteOperation))]
    [KnownType(typeof(CAWebApplicationOutgoingEmailSettingsOperation))]
    [KnownType(typeof(CAManageWebApplicationSelfServiceSiteManagementOperation))]
    [KnownType(typeof(CAManageWebApplicationDeleteWebApplicationOperation))]
    [KnownType(typeof(CAManageWebApplicationDefineManagedPathOperation))]
    [KnownType(typeof(CAManageWebApplicationBlockedFileTypeOperation))]
    [KnownType(typeof(CAManageWebApplicationFeaturesOperation))]
    [KnownType(typeof(CACreateWebApplicationOperation))]
    [KnownType(typeof(CAWebApplicationGeneralSettingSharePointDesignerOperation))]
    [KnownType(typeof(CAManageWebApplicationMobileAccountOperation))]
    [KnownType(typeof(CAWebApplicationResourceThrottlingOperation))]
    [KnownType(typeof(CAWebApplicationGerneralSettingWorkflowOperation))]
    [KnownType(typeof(CAWebApplicationServiceConnectionOperation))]
    [KnownType(typeof(CAWebApplicationWebpartSecurityOperation))]
    [KnownType(typeof(CAWebApplicationConfigureDocumentConversionsOperation))]
    [KnownType(typeof(CAWebApplicationConfigureSendToConnectionsOperation))]
    [KnownType(typeof(CAWebApplicationSiteCollectionListOperation))]
    [KnownType(typeof(CAWebApplicationUserPermissionsOperation))]
    [KnownType(typeof(CAWebApplicationUserPolicyOperation))]
    [KnownType(typeof(CAWebApplicationAnonymousPolicyOperation))]
    [KnownType(typeof(CAWebApplicationPermissionPolicyOperation))]
    #endregion

    #region Site collection level begin
    [KnownType(typeof(CASiteCollectionQuoteAndLockOperation))]
    [KnownType(typeof(CASiteCollectionAnonymousAccessOperation))]
    [KnownType(typeof(CASiteCollectionColumnOperation))]
    [KnownType(typeof(CASiteCollectionAdministratorOperation))]
    [KnownType(typeof(CAPortalSiteConnection))]
    [KnownType(typeof(CASiteCollectionSearchSettingsOperation))]
    [KnownType(typeof(CASiteCollectionMaxDepth))]
    [KnownType(typeof(CASiteCollectionFeaturesOperation))]
    [KnownType(typeof(CARSSOPeration))]
    [XmlInclude(typeof(CACreateSiteCollectionOperation))]
    [KnownType(typeof(CACreateSiteCollectionOperation))]
    [KnownType(typeof(CADeleteSiteCollectionOPeration))]
    [KnownType(typeof(CASiteCollectionHelpSettingOperation))]
    [KnownType(typeof(CASiteCollectionChangeContentDatabaseOperation))]
    [KnownType(typeof(CASiteCollectionDesignerSettingOperation))]
    [KnownType(typeof(CASharePointDesignerSettingsOperation))]
    [KnownType(typeof(CAContentTypePublishingHubsOperation))]
    [KnownType(typeof(CAStorageQuotaOperation))]
    [KnownType(typeof(CASiteCollectionSearchScopesOperation))]
    [KnownType(typeof(CASiteCollectionVisualUpgradeOperation))]
    [KnownType(typeof(CAWebpartOperation))]
    [KnownType(typeof(CASolutionsOperation))]
    [KnownType(typeof(CASiteCollectionThemesOperation))]
    [KnownType(typeof(CASiteCollectionFastSearchUserContextOperation))]
    [KnownType(typeof(CASiteCollectionFastSearchKeywordsOperation))]
    [KnownType(typeof(CASiteCollectionFastSearchSitePromotionAndDemotionOperation))]
    [KnownType(typeof(CASiteCollectionSearchKeywordOperation))]
    [KnownType(typeof(CASiteCollectionListTemplateOperation))]
    [KnownType(typeof(CASiteCollectionSiteTemplateOperation))]
    [KnownType(typeof(CASiteCollectionDocumentIdSettingOperation))]
    [KnownType(typeof(CABrokenLinkCheckOperation))]
    [KnownType(typeof(CASiteCollectionTranslatableColumnsOperation))]
    [KnownType(typeof(CAPrimaryAndSecondaryAdministratorOperation))]
    [KnownType(typeof(CASharingSettingOperation))]
    #endregion

    #region web level begin
    [KnownType(typeof(CAMasterPageOperation))]
    [KnownType(typeof(CAWebContentTypeOperation))]
    [KnownType(typeof(CAWebDesTitleIconOperation))]
    [KnownType(typeof(CAWebDeleteSiteOperation))]
    [KnownType(typeof(CATopLinkBarOperation))]
    [KnownType(typeof(CAWebTreeViewOperation))]
    [KnownType(typeof(CAWebSearchAndOfflineOperation))]
    [KnownType(typeof(CAWebQuickLaunchOperation))]
    [KnownType(typeof(CAWebCreateSubSiteOperation))]
    [KnownType(typeof(CAWebSearchAlertsOperation))]
    [KnownType(typeof(CARelLinksScopeSettingsOperation))]
    [KnownType(typeof(CAPermissionSetupOperation))]
    #endregion web level end

    [KnownType(typeof(CADeleteObjectsOperation))]

    #region list level begin
    [KnownType(typeof(CAFolderSettingOperation))]
    [KnownType(typeof(CAListVersioningSettingOperation))]
    [KnownType(typeof(CAListAdvancedSettingOperation))]
    [KnownType(typeof(CAListAnonymousAccessOperation))]
    [KnownType(typeof(CAListAudienceTargetingSettingOperation))]
    [KnownType(typeof(CAListPreLocationSettingOperation))]
    [KnownType(typeof(CAListTitleDesNovOperation))]
    [KnownType(typeof(CAListRatingSettingOperation))]
    [KnownType(typeof(CAListRssSettingOperation))]
    [KnownType(typeof(CAListValidationSettingOperation))]
    [KnownType(typeof(CAListDeleteListOperation))]
    [KnownType(typeof(CAListAlertMeOperation))]
    [KnownType(typeof(CAListIndexedColumnsOperation))]
    [KnownType(typeof(CAListManageCheckOutFilesOperation))]
    [KnownType(typeof(CAListRecordDeclarationSettingsOperation))]
    [KnownType(typeof(CAListColumnDefaultValueOperation))]
    [KnownType(typeof(CAListMetadataNavigationSettingsOperation))]
    [KnownType(typeof(CAListWorkflowSettingsOperation))]
    [KnownType(typeof(CAListEnterpriseMetadataKeywordsSettingsOperation))]
    [KnownType(typeof(CAListInformationPolicySettingsOperation))]
    #endregion

    #region item level
    [KnownType(typeof(CAItemVersionDeleteOperation))]
    [KnownType(typeof(CAItemDeleteOperation))]
    [KnownType(typeof(CAItemAlertMeOperation))]
    #endregion

    #region stsadm begin
    [XmlInclude(typeof(CASTSAdmOperation))]
    [KnownType(typeof(CASTSAdmOperation))]
    #endregion 

    #region folder level begin
    [KnownType(typeof(CAFolderDeleteOperation))]
    [KnownType(typeof(CAFolderEditPropertiesOperation))]
    #endregion folder level end

    #region security center begin
    [KnownType(typeof(CASecurityDeleteUsersGroupsOperation))]
    [KnownType(typeof(CASecurityGroupsOperation))]
    [KnownType(typeof(CASecurityUsersInGroupOperation))]
    [KnownType(typeof(CASecurityUsersOperation))]
    [KnownType(typeof(CASecurityPermissionsOperation))]
    [KnownType(typeof(CASecurityRemoveUserOperation))]
    [KnownType(typeof(CASecurityUpdatePermissionOperation))]
    [XmlInclude(typeof(CASecurityImportPermissionsOperation))]
    [KnownType(typeof(CASecurityImportPermissionsOperation))]
    [KnownType(typeof(CASecurityEditGroupsOperation))]
    [KnownType(typeof(CASecurityInheritingPermissionsOperation))]
    [KnownType(typeof(CASecurityChangeGroupOperation))]
    [KnownType(typeof(CASecurityAddGroupOperation))]
    [KnownType(typeof(CASearchWebPartOperation))]
    [KnownType(typeof(CAEditWebPartOperation))]
    [XmlInclude(typeof(CASecurityCloneUserPermissionsOperation))]
    [KnownType(typeof(CASecurityCloneUserPermissionsOperation))]
    [XmlInclude(typeof(CASecurityDeadAccountCleanerOperation))]
    [KnownType(typeof(CASecurityDeadAccountCleanerOperation))]
    [XmlInclude(typeof(CASecurityOnlineDeadAccountCleanerOperation))]
    [KnownType(typeof(CASecurityOnlineDeadAccountCleanerOperation))]
    [XmlInclude(typeof(CASecurityCloneSitePermissionOperation))]
    [KnownType(typeof(CASecurityCloneSitePermissionOperation))]
    [KnownType(typeof(CASecurityAllPeopleOperation))]
    [KnownType(typeof(CASecurityPermissionLevelOperation))]
    [KnownType(typeof(CASecurityPushInheritPermissionOperation))]
    [KnownType(typeof(CASecurityCheckPermissionsOperation))]
    [KnownType(typeof(CASecurityEditUserPermissionsOperation))]
    #endregion security center end

    #region SharedServices
    //[KnownType(typeof(CASearchServiceAlertsOperation))]
    //[KnownType(typeof(CASearchServiceAuthoritativePagesOperation))]
    //[KnownType(typeof(CASearchServiceContentAccountOperation))]
    //[KnownType(typeof(CASearchServiceContentSourcesOperation))]
    //[KnownType(typeof(CASearchServiceFileTypesOperation))]
    //[KnownType(typeof(CASearchServiceServerNameMappingsOperation))]
    //[KnownType(typeof(CASearchServiceUsageReportingOperation))]
    [KnownType(typeof(CASearchServiceGeneralSettingsOperation))]
    [KnownType(typeof(CASearchServiceServerNameMappingsOperation))]
    [KnownType(typeof(CASearchServiceAuthoritativePagesOperation))]
    [KnownType(typeof(CASearchServiceContentSourcesOperation))]
    [KnownType(typeof(CASearchServiceFileTypesOperation))]
    #endregion

    #region Other level begin
    [KnownType(typeof(ManageFeartureOperation))]
    [KnownType(typeof(SiteRegionalOperation))]
    [KnownType(typeof(CAResetToSiteDefinitionOperation))]
    [KnownType(typeof(CAWebFeatureOperation))]
    [KnownType(typeof(AddWebPartOperation))]
    [KnownType(typeof(CAAlertUserNameOperation))]
    [KnownType(typeof(CreateListOperation))]
    [KnownType(typeof(CAWebThemeOperation))]
    [KnownType(typeof(CAWebDesTitleIconOperation))]
    [KnownType(typeof(CAWebAnonymousAccessOperation))]
    [KnownType(typeof(CAWebRssSettingOperation))]
    [KnownType(typeof(CACustomPropertiesOperation))]
    [KnownType(typeof(CASearchDuplicateFileOperation))]
    [KnownType(typeof(CAOfflineExportReportOperation))]
    #endregion

    #region AdminProfile
    [KnownType(typeof(AdministratorProfileLoadOperation))]
    [KnownType(typeof(AdministratorProfileJobOperation))]
    [KnownType(typeof(NewAdministratorProfileJobOperation))]
    [KnownType(typeof(AdministratorFixProfileOperation))]
    [KnownType(typeof(AdministratorProfileReportOperation))]
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]

    [XmlRoot]
    [KnownType(typeof(NewCASecurityInheritingPermissionsOperation))]  //SAAS-24195 
    [KnownType(typeof(NewCAWebCreateSubSiteOperation))]   //SAAS-24390
    public class CAOperation
    {
        [DataMember]
        [XmlAttribute]
        public string TreeNodeId { get; set; }

        [DataMember]
        [XmlElement]
        public ReturnResult ReturnValue { get; set; }

        /// <summary>
        /// This method implements a object clone, NOTES: this is not a deep clone
        /// </summary>
        /// <returns>a clone object</returns>
        public virtual CAOperation CloneOperation()
        {
            return (CAOperation)this.MemberwiseClone();
        }
    }
}