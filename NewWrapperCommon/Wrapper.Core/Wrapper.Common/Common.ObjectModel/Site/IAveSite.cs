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
using AvePoint.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Core.Common;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSite : IDisposable
    {
        /// <summary>
        /// 用来判断当前site是使用什么方式获取的。
        /// </summary>
        WrapperSPMode SPMode { get; }
        /// <summary>
        /// 获取当前site的用户名以及密码
        /// </summary>
        AveBPOSAccountInfo UserAccountInfo { get; }

        /// <summary>
        /// Api Permission，目前只有13会check permission，其他平台都是full control，
        /// 如果以后需要检查什么权限，再添加。
        /// </summary>
        WrapperNativeApiPermission NativeApiPermission { get; }

        void Close();
        void Delete();
        void Delete(bool deleteADAccounts, bool gradualDelete);
        bool InvalidateCacheEntry(Uri uri, Guid siteId);
        IAveListTemplateCollection GetCustomListTemplates(IAveWeb web);
        Dictionary<Guid, IAveTerm> TermIdCache { set; get; }
        Guid GetListId(Guid webId, string listTitle);
        //AveSiteInfo GetSiteInfo();
        //AveSiteSettingInfo GetSiteSettingInfo();
        //List<AveUserInfo> GetSiteUsers(bool allAvailableUser);
        Dictionary<Guid, long> GetAllWebSize();
        IAveWebTemplateCollection GetWebTemplates(uint licd);// 
        IAveWebTemplateCollection GetWebTemplates(uint licd, int overrideCompatLevel);
        string MakeFullUrl(string strUrl);
        string MakeFullUrl(string strUrl, string realWebAppUrl);
        IAveWeb OpenWeb(Guid webId);
        IAveWeb OpenWeb(string webUrl);
        IAveWeb OpenWeb();
        IAveWeb OpenWeb(string strUrl, bool requireExactUrl);
        void Update();
        void VisualUpgradeWebs();

        AveBitField Flags { get; }
        long Size { get; }
        IAveTaxonomySession AveSPTaxonomySession { get; }
        bool AllowRssFeeds { get; }
        bool AllowDesigner { get; set; }
        bool AllowMasterPageEditing { get; set; }
        bool AllowRevertFromTemplate { get; set; }

        /// <summary>
        /// 尽量使用OpenWeb，使用此方式会产生很多Web对象，并且都需要释放
        /// </summary>
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
        bool IsReadLocked { get; }
        bool WriteLocked { get; set; }
        bool ReadOnly { get; set; }
        string LockIssue { get; set; }
        IAveQuota Quota { get; set; }
        AveUrlZone Zone { get; }
        DateTime CertificationDate { get; }
        IAveUser SystemAccount { get; }
        string SPVersion { get; }
        bool AdministratorOperationMode { get; set; }

        //add by Guoxi sun, for metadata service backup.
        //List<AveTermStoreInfo> GetMetadataServiceData();
        Guid ID { get; }
        Guid GetWeb(IAveBackupRestoreQueryService queryService, string url);
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
        IAveFeatureDefinitionCollection FeatureDefinitions { get; }
        bool IsMoss { get; }
        bool IsPublish { get; }
        object DataProvider { get; }
        DateTime LastReloadTimeUTC { get; }

        [Obsolete("replace with GetCheckoutWeb(Guid siteId, IAveWeb web, IAveList list, IAveUser user, Guid fileId, bool isBackupJob)")]
        IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId);
        [Obsolete("replace with GetCheckoutWeb(Guid siteId, IAveWeb web, IAveList list, IAveUser user, Guid fileId, bool isBackupJob)")]
        IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveUser user, Guid fileId, bool isBackupJob);
        IAveWeb GetCheckoutWeb(Guid siteId, IAveWeb web, IAveList list, IAveUser user, Guid fileId, bool isBackupJob, bool throwIfNotHaveEnoughPermission = false);
        void RestoreSettings(AveSiteSettingInfo settingInfo);
        void ApplyCustomWebTemplateInSolution(String solutionPath, String solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId);


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
        AveAPIType GetAPIType();
        void EnableAlerts(Dictionary<Guid, List<Guid>> alerts);
        DateTime GetLastAccessedDayOfSite();
        void GetRecycleBinStatistics(out int itemCount, out long size);
        AveAPIType APIType { get; }
        List<Dictionary<string, object>> GetPublishedContentTypes();
        IAveFeatureDefinitionCollection GetAllFeatureDefinitions();
        void UpdateSpecialProperty();
        IAveOUserProfileManager GetUserProfileManager();
        string GetUserLoginBySystemId(byte[] systemId);
        bool ActiveDeletedUserBySystemId(byte[] systemId);
        bool IsClassicWindowsModeAuthentication { get; }
        bool IsOnlineSite { get; }
        int CompatibilityLevel { get; }
        /// <summary>
        /// This method will clean up some static cache used internal in SharePoint API object.
        /// There will be performance issue if this method is used frequently and unexpected issue in muti-thread env.
        /// DO NOT use this method unless there is heavy memory leak. Contact Oliver.Luo before use this method if needed.
        /// </summary>
        void InternalCleanup();

        Dictionary<string, string> GetLookupItemIdAndDisplayValue(AveLookupFieldInfo fieldInfo);

        bool Exists(Uri uri);
        IAveEventReceiverDefinitionCollection EventReceivers { get; }
        #region add for SP2013
        bool Archived { get; set; }
        bool ReadOnlyMode { get; set; }
        AveBasePermissions DenyPermissionsMask { get; set; }
        #endregion

        #region add for SP2016
        bool IsSiteMaster { get; }
        #endregion

        //To operate Change Log
        IAveChangeCollection GetChanges();
        IAveChangeCollection GetChanges(IAveChangeQuery query);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd);

        IAveRecycleBinItemCollection GetRecycleBinItems(IAveRecycleBinQuery query);

        //for apps PRItem need to use reflector to get app package info
        IAveQuerySession SqlSession { get; }
        string AppSiteDomainPrefix { get; set; }

        void CustomizeReport(Dictionary<string, object> parameters);
        bool MigrateUser(string oldLogin, byte[] oldSid, string newLogin, byte[] newSid);

        IAveUserCustomActionCollection UserCustomActions { get; }
        Guid GetVariationLabelId(string labelName);

        #region Add For Office365

        bool DeleteMigrationJob(Guid id);

        AveMigrationJobState GetMigrationJobStatus(Guid id);

        Guid CreateMigrationJob(Guid gWebId,string azureContainerSourceUri,string azureContainerManifestUri,string azureQueueReportUri);

        Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options);

        AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers();

        AveProvisionedMigrationQueueInfo ProvisionMigrationQueue();

        bool DenyAddAndCustomizePagesStatus { get; set; }

        List<AveComplianceTagInfo> GetAvailableTagsForSite();

        #region Project
        IAveProjectServer ProjectServer { get; }
        IAveProjectCollection Projects { get; }
        IAveProjectCalendarCollection ProjectCalendars { get; }
        IAveProjectCustomFieldCollection ProjectCustomFields { get; }
        IAveProjectLookupTableCollection ProjectLookupTables { get; }
        IAveProjectEnterpriseProjectTypeCollection ProjectEnterpriseProjectTypes { get; }
        IAveProjectEnterpriseResourceCollection ProjectEnterpriseResources { get; }
        IAveProjectPhaseCollection ProjectPhases { get; }
        IAveProjectStageCollection ProjectStages { get; }
        #endregion

        #endregion
    }
}
