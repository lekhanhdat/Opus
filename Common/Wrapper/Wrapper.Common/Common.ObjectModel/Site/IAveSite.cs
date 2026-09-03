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




using AvePoint.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Apps;
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSite : IDisposable
    {
        void InitializeDBService(string connectionString);
        void Close();
        void Delete();
        void Delete(bool deleteADAccounts, bool gradualDelete);
        bool InvalidateCacheEntry(Uri uri, Guid siteId);
        IAveListTemplateCollection GetCustomListTemplates(IAveWeb web);
        Guid GetListId(Guid webId, string listTitle);
        //AveSiteInfo GetSiteInfo();
        //AveSiteSettingInfo GetSiteSettingInfo();
        //List<AveUserInfo> GetSiteUsers(bool allAvailableUser);
        IAveWebTemplateCollection GetWebTemplates(uint licd);// 
        string MakeFullUrl(string strUrl);
        string MakeFullUrl(string strUrl, string realWebAppUrl);
        List<AveAppMetadata> AvaliableSiteApp { get; }
        bool EnableSiteAppCatalog { get; }
        List<AveAppMetadata> AvaliableTenantApp { get; }
        IAveWeb OpenWeb(Guid webId);
        IAveWeb OpenWeb(string webUrl);
        IAveWeb OpenWeb();
        IAveWeb OpenWeb(string strUrl, bool requireExactUrl);
        void Update();
        void VisualUpgradeWebs();
        IAveWeb AddWeb(string strWebUrl, string strTitle, string strDescription, uint nLCID, string strWebTemplate, bool useUniquePermissions, bool bConvertIfThere);
        void ApplyCustomWebTemplateInSolution(String solutionPath, String solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId);

        long Size { get; }
        IAveTaxonomySession AveSPTaxonomySession { get; }
        bool AllowRssFeeds { get; }
        bool AllowDesigner { get; set; }
        bool AllowMasterPageEditing { get; set; }
        bool AllowRevertFromTemplate { get; set; }
        IAveWebCollection AllWebs { get; }
        IAveAudit Audit { get; }
        string AuditLogTrimmingCallout { get; set; }
        int AuditLogTrimmingRetention { get; set; }
        double AverageResourceUsage { get; }
        bool BrowserDocumentsEnabled { get; }
        IAveContentDatabase ContentDatabase { get; }
        double CurrentResourceUsage { get; }
        bool HostHeaderIsSiteName { get; }//
        bool IISAllowsAnonymous { get; }
        DateTime LastContentModifiedDate { get; }
        DateTime LastItemUserModifiedDate { get; }
        IAveUser Owner { get; set; }
        IAveUser SecondaryContact { get; set; }
        string PortalName { get; set; }
        string PortalUrl { get; set; }
        string Url { get; }
        string ServerRelativeUrl { get; }
        bool ShowURLStructure { get; set; }
        IAveUserSolutionCollection Solutions { get; }
        IAveRecycleBinItemCollection RecycleBin { get; }
        IAveWeb RootWeb { get; }
        IAveFeatureCollection Features { get; }
        bool SyndicationEnabled { get; set; }
        bool TrimAuditLog { get; set; }
        bool UIVersionConfigurationEnabled { get; set; }
        AveUsageInfo Usage { get; }
        IAveWebApplication WebApplication { get; }
        IAveWorkflowManager WorkflowManager { get; }
        bool ReadLocked { get; set; }
        bool WriteLocked { get; set; }
        bool ReadOnly { get; set; }
        string LockIssue { get; set; }
        bool DenyAddAndCustomizePagesStatus { get; set; }
        bool AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled { get; }
        bool HasHolds { get; }
        IAveQuota Quota { get; set; }
        AveUrlZone Zone { get; }
        DateTime CertificationDate { get; }
        IAveUser SystemAccount { get; }
        string SPVersion { get; }

        //add by Guoxi sun, for metadata service backup.
        //List<AveTermStoreInfo> GetMetadataServiceData();
        Guid ID { get; }
        Guid GetWeb(IAveBackupRestoreQueryService queryService, string p);
        bool AllowUnsafeUpdates { get; set; }
        string GetWebCTNameById(string contentTypeId);
        IAveList GetCatalog(AveListTemplateType typeCatalog);
        void UpdateUserInfo(string listName, int userId, AveUserInfo old);
        //AveUserInfo GetUserInfo(int principalId);
        //AveGroupInfo GetGroupInfo(int principalId);
        bool CheckUserIfAvailable(int userId);
        string Protocol { get; }
        int Port { get; }
        string HostName { get; }
        IAveUserToken UserToken { get; }
        IAveRequest Request { get; }
        IAveFeatureDefinitionCollection FeatureDefinitions { get; }
        bool IsMoss { get; }
        bool IsPublish { get; }
        object DataProvider { get; }
        DateTime LastReloadTimeUTC { get; }
        IAveUserCustomActionCollection UserCustomActions { get; }
        string GeoLocation { get; }

        #region publishing 

        IAveDesignPackage DesignPackageSerializer { get; }
        #endregion

        IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId);
        void RestoreSettings(AveSiteSettingInfo settingInfo);


        Guid CheckOutFileId { get; set; }
        DateTime LastSecurityModifiedDate { get; }
        int CheckOutUser { get; set; }
        IAveSiteSerializer SiteSerializer { get; }
        IAveSiteSettingSerializer SiteSettingSerializer { get; }
        IAveMetaDataServiceSerializer MetaDataServiceSerializer { get; }
        IAveUserSerializer UserSerializer { get; }
        IAveGroupSerializer GroupSerializer { get; }
        IAveUsersSerializer SiteUsersSerializer { get; }
        IAveFeatureSerializer FeatureSerializer { get; }
        void ReloadSite();
        void ReloadTaxonomySession();

        AveAPIType GetAPIType();
        void EnableAlerts(Dictionary<Guid, List<Guid>> alerts);
        DateTime GetLastAccessedDayOfSite();
        void GetRecycleBinStatistics(out int itemCount, out long size);
        AveAPIType APIType { get; }
        IAveFeatureDefinitionCollection GetAllFeatureDefinitions();
        List<Dictionary<string, object>> GetPublishedContentTypes();
        void UpdateSpecialProperty();
        IAveOUserProfileManager GetUserProfileManager();
        string GetUserLoginBySystemId(byte[] systemId);
        bool IsClassicWindowsModeAuthentication { get; }
        int CompatibilityLevel { get; }
        //string CustomizeReport(Dictionary<string, object> parameters);
        void SetAuditLogTrimming(Dictionary<string, object> parameters);
        DateTime GetLastAccessTime(string sitecollectionURL, DateTime? modifiedTime = null, bool isCompatibleByModifiedTime = false);

        List<AveComplianceTagInfo> GetAvailableTagsForSite();

        #region add for SP2013
        bool Archived { get; set; }
        bool ReadOnlyMode { get; set; }
        bool ExternalSharingTipsEnabled { get; }
        #endregion

        void AddChangePropertiesToDataCache(Dictionary<string, object> changeProperties);

        #region
        IAveProjectServer ProjectServer{get;}
        IAveProjectCollection Projects { get; }
        IAveProjectCalendarCollection ProjectCalendars { get; }
        IAveProjectCustomFieldCollection ProjectCustomFields { get; }
        IAveProjectLookupTableCollection ProjectLookupTables { get; }
        IAveProjectEnterpriseProjectTypeCollection ProjectEnterpriseProjectTypes { get; }
        IAveProjectEnterpriseResourceCollection ProjectEnterpriseResources { get; }
        IAveProjectPhaseCollection ProjectPhases { get; }
        IAveProjectStageCollection ProjectStages { get; }
        #endregion

        #region Add For Office365

        bool DeleteMigrationJob(Guid id);

        AveMigrationJobState GetMigrationJobStatus(Guid id);

        MigrationJobProgress GetMigrationJobProgress(Guid id, string nextToken = "0");

        bool NeedDeleteMigrationJob(Guid id);

        Dictionary<Guid, AveMigrationJobState> GetMigrationStatus();

        Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri);

        Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options);

        AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers();

        AveProvisionedMigrationQueueInfo ProvisionMigrationQueue();

        #endregion

        bool CheckSiteIsLocked();

        void RemoveSiteLockedState();

        bool DeleteSCTermGroup();

        bool ExistSCTermGroup();

        bool UpdateSCTermGroupName(string name);
    }
}
