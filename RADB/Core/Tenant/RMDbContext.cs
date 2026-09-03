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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using AvePoint.RA.DB.Model.DisposalStub;
using AvePoint.RA.DB.Model.Google;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AvePoint.RA.DB.Model.DataIngestion;

namespace AvePoint.RA.DB.Core
{
    public class RMDbContext : DbContext, IDbModelCacheKeyProvider
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RMDbContext));
        private DateTime _expireTime = DateTime.Now;
        //private AveImpersonator impersonator = null;

        public string SchemaName { get; private set; }

        public RMDbContext()
        {
            Database.SetInitializer<RMDbContext>(null);
            //注意不能使用如下code初始化DB. 会导致DB自动升级的问题.
            //Database.SetInitializer<RMDbContext>(new MigrateDatabaseToLatestVersion<RMDbContext, AvePoint.RA.DB.TenantMigrations.Configuration>());
        }

        public RMDbContext(DbConnection conn, string schema) : base(conn, true)
        {
            Database.SetInitializer<RMDbContext>(null);
            //注意不能使用如下code初始化DB. 会导致DB自动升级的问题.
            //Database.SetInitializer<RMDbContext>(new MigrateDatabaseToLatestVersion<RMDbContext, AvePoint.RA.DB.TenantMigrations.Configuration>());
            SchemaName = schema;
        }



        #region properties
        public bool IsDispose { set; get; }

        /// <summary>
        /// 从创建DbContext实例开始，7天后超时
        /// </summary>
        public bool IsExpire
        {
            get
            {
                if ((DateTime.Now - _expireTime).Days < 7)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsTokenExpire
        {
            get
            {
                string accessToken = null;
                try
                {
                    var connection = this.Database.Connection as SqlConnection;
                    if (connection != null && !string.IsNullOrEmpty(connection.AccessToken))
                    {
                        var tokenParts = connection.AccessToken.Split('.');
                        if (tokenParts.Length > 1)
                        {
                            var base64str = tokenParts[1];
                            if(base64str.Length % 4 > 0)
                            {
                                base64str += "====".Substring(base64str.Length % 4);
                            }
                            base64str = base64str.Replace("_", "/").Replace("-", "+");
                            string jsonStr = Encoding.UTF8.GetString(Convert.FromBase64String(base64str));
                            var tokenInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
                            if(tokenInfo.TryGetValue("exp", out var expValue) && double.TryParse(expValue?.ToString(), out var expSeconds))
                            {
                                //util.azure中获取Token时，刷新缓存时间点是：Token Expire时间减去10分钟，所以这里用9分钟，保证重新获取Token时，能够刷新Token
                                expSeconds -= 60 * 9;   
                                var expireTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expSeconds);
                                if(expireTime < DateTime.UtcNow)
                                {
                                    return true;
                                }
                            }
                        }

                        //logger.Warn($"Incorrect token check for the DB Context: {accessToken}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while checking access token for the db context: {ex}");
                    //logger.Debug($"Token of the DB Context: {accessToken}");
                }
                
                return false;
            }
        }
        #endregion

        #region common functions
        public void DetachLocalObject<T>(T obj) where T : class
        {
            var localObj = FindLocalObject(obj);
            if (localObj != null)
            {
                Detach(localObj);
            }
        }

        public void Detach<T>(T obj) where T : class
        {
            ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext;
            oc.Detach(obj);
        }

        public T FindLocalObject<T>(T obj) where T : class
        {
            var keys = GetEntityKeys<T>();
            var func = GetFindExp<T>(obj, keys).Compile();
            return Set<T>().Local.FirstOrDefault(func);
        }

        public IEnumerable<string> GetEntityKeys<T>() where T : class
        {
            ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext;
            var keys = oc.CreateObjectSet<T>().EntitySet.ElementType.KeyProperties.Select(x => x.Name);
            return keys;
        }

        private Expression<Func<T, bool>> GetFindExp<T>(T obj, IEnumerable<string> keys) where T : class
        {
            var pe = Expression.Parameter(typeof(T), "p");

            var keyExps = keys.Select(k =>
            {
                var member = Expression.PropertyOrField(pe, k);
                var val = typeof(T).GetProperty(k).GetValue(obj);
                var eq = Expression.Equal(member, Expression.Constant(val));
                return eq;
            }).ToList();

            if (keys.Count() == 1)
            {
                return Expression.Lambda<Func<T, bool>>(keyExps[0], new[] { pe });
            }

            var combinExp = Expression.AndAlso(keyExps[0], keyExps[1]);
            for (var i = 2; i < keyExps.Count; i++)
            {
                combinExp = Expression.AndAlso(combinExp, keyExps[i]);
            }
            return Expression.Lambda<Func<T, bool>>(combinExp, new[] { pe });
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            IsDispose = true;
            //if (IsDispose && impersonator != null)
            //{
            //    impersonator.Dispose();
            //}
            base.Dispose(disposing);
        }

        #region Database Sets
        #region EXO DB Set
        public DbSet<EXONodeFlag> EXONodeFlag { get; set; }
        #endregion

        public DbSet<RMEncryptionData> EncryptionData { get; set; }

        public DbSet<RMManualApproveHistory> ManualApproveHistory { get; set; }

        public DbSet<RMFunctionSetting> FunctionSettings { get; set; }

        public DbSet<RMArchiveSiteInfo> RMArchiveSiteInfo { get; set; }
        public DbSet<RMArchiveGDriveInfo> RMArchiveGDriveInfo { get; set; }
        public DbSet<RMArchiveTeamsGroupInfo> RMArchiveTeamsGroupInfoes { get; set; }

        public DbSet<RMDashboardDataUsage> DashboardDataUsage { get; set; }
        public DbSet<RMDashboardTermUsage> DashboardTermUsage { get; set; }
        public DbSet<RMDashboardTermApplyRuleUsage> DashboardTermApplyRuleUsage { get; set; }
        public DbSet<RMDashboardUserWaitingApprovalCount> DashboardUserWaitingApprovalCount { get; set; }
        public DbSet<RMDashboardDataUsageOfDate> DashboardDataUsageOfDate { get; set; }
        public DbSet<RMTermSet> TermSets { set; get; }
        public DbSet<RMTerm> Terms { set; get; }
        public DbSet<RMTermSetMembership> TermSetMemberships { set; get; }

        public DbSet<RMTermRuleAssociation> RMTermRuleAssociations { set; get; }

        public DbSet<RMTermGroup> TermGruops { set; get; }

        public DbSet<RMTermGroupMembership> TermGroupMembership { get; set; }
        public DbSet<RMCPGlobalStorageSetting> GlobalStorageSettingInfos { get; set; }
        public DbSet<RMCPExportSetting> RMCPExportSetting { get; set; }
        public DbSet<RMExportDataEncryptionSetting> RMExportDataEncryptionSetting { get; set; }
        public DbSet<RMAppProfileInfo> RMAppProfileInfo { get; set; }

        public DbSet<RMJobMonitor> JobMonitors { set; get; }
        public DbSet<RMJobMonitorArchive> RMJobMonitorArchives { set; get; }

        public DbSet<RMJobProgress> JobProgresses { set; get; }

        public DbSet<RMArchiverJob> ArchiverJobs { set; get; }

        public DbSet<RMSharePointSetting> RMSharePointSettings { get; set; }

        public DbSet<RMTeamsSetting> RMTeamsSettings { get; set; }

        public DbSet<RMArchiverSetting> RMArchiverSettings { get; set; }

        public DbSet<MultiGeoSettingInfo> MultiGeoSettingInfos { get; set; }
        public DbSet<RMExchangeOnlineSetting> RMExchangeOnlineSettings { get; set; }
        public DbSet<RMExchangeOnlineSettingRuleMapping> RMExchangeOnlineSettingRuleMappings { get; set; }
        public DbSet<RMAudit> Audit { get; set; }
        public DbSet<RMFSAudit> RMFSAudits { get; set; }
        public DbSet<RMSchedule> Schedule { get; set; }
        public DbSet<RMBoardCache> RMBoardCache { get; set; }
        public DbSet<RMClassificationHistory> RMClassificationHistory { get; set; }
        public DbSet<RMEXOLabel> RMRetentionLabel { get; set; }
        public DbSet<RMDataOfDay> DataOfDay { get; set; }
        public DbSet<BoardTotal> BoardTotal { get; set; }
        public DbSet<RMWaitingApprovalAssignee> WaitingApprovalAssignee { get; set; }
        public DbSet<RMProfile> Profile { get; set; }
        public DbSet<RMMiscProfile> MiscProfile { get; set; }   //暂时去掉这个表, in march ci branch
        public DbSet<RMSettingJobInfo> SettingJobInfo { get; set; }
        public DbSet<RMLocationAssociation> LocationAssociation { get; set; }
        public DbSet<RMContainer> Container { get; set; }

        public DbSet<RMRecordOwner> RecordOwner { get; set; }
        public DbSet<RMLock> RMLock { get; set; }

        public DbSet<RMUniqueIdSetting> UniqueIdSetting { get; set; }

        public DbSet<RMJobExportSetting> JobExportSetting { get; set; }

        public DbSet<RMSiteCollectionSize> SiteCollectionSize { get; set; }

        public DbSet<RMTermUsage> TermUsage { get; set; }

        public DbSet<RECOSiteCollection> RECOSiteCollection { get; set; }

        public DbSet<RMManualApprove> ManualApprove { get; set; }

        public DbSet<RMAccount> Account { get; set; }

        public DbSet<RMPermission> Permission { get; set; }
        public DbSet<RMRole> Role { get; set; }
        public DbSet<RMLnkRolePermission> LnkRolePermission { get; set; }
        public DbSet<RMLnkUserRole> LnkUserRole { get; set; }
        public DbSet<RMPoolUser> PoolUser { get; set; }
        public DbSet<RMLnkUserGroup> LnkUserGroup { get; set; }
        public DbSet<RMChangeClassification> ChangeClassifications { get; set; }
        public DbSet<RMDeclaredSettingLock> RMDeclaredSettingLock { get; set; }
        public DbSet<RMNodeFlag> NodeFlag { get; set; }
        public DbSet<RMRule> RMRule { get; set; }
        public DbSet<RMBoardIndex> BoardIndex { get; set; }
        public DbSet<RMSubJob> RMSubJobs { get; set; }
        public DbSet<RMJobContext> JobContexts { get; set; }
        
        public DbSet<RMHold> Hold { get; set; }
        public DbSet<RMHoldMemberships> RMHoldMemberships { get; set; }
        public DbSet<RMWorkspaceHold> WorkspaceHold { get; set; }
        public DbSet<RMScope> Scope { get; set; }
        public DbSet<RMRecordAlliance> Alliance { get; set; }
        public DbSet<RMRecordLoanAlliance> LoanAlliance { set; get; }
        public DbSet<RMManagedRecordRelated> ManagedRecordRelated { get; set; }
        public DbSet<RMRecordsUpdateTemp> RecordsUpdateTemp { get; set; }
        public DbSet<RMBarcodeTemplate> BarcodeTemplate { get; set; }
        public DbSet<RMBarcodeTemplateColumnMembership> BarcodeTemplateColumnMembership { get; set; }
        public DbSet<RMTemplate> Template { get; set; }
        public DbSet<RMTemplateRelationship> TemplateRelationship { get; set; }
        public DbSet<RMDownloadDataInfo> DownloadDataInfo { get; set; }

        public DbSet<RMSuite> Suite { get; set; }
        [Obsolete]
        public DbSet<RMSuiteMembership> SuiteMembership { get; set; }
        public DbSet<RMTemplateCategory> TemplateCategory { get; set; }
        public DbSet<RMLocation> RMLocation { get; set; }
        public DbSet<RMLocationSuiteAssociation> RMLocationSuiteAssociation { get; set; }
        public DbSet<RMPhysicalRequest> RMPhysicalRequest { set; get; }
        public DbSet<RMPhysicalRecordSetting> RMPhysicalRecordSetting { get; set; }
        public DbSet<RMPhysicalColumnChangeLog> RMPhysicalColumnChangeLog { get; set; }
        public DbSet<RMPhysicalPushColumn> RMPhysicalPushColumn { get; set; }
        public DbSet<RMPhysicalNodeFlag> RMPhysicalNodeFlag { get; set; }

        public DbSet<RMEmailTemplate> EmailTemplate { get; set; }

        public DbSet<RMMobileHistory> RMMobileHistory { get; set; }

        public DbSet<RMPhysicalUniqueIdSetting> PhysicalUniqueIdSetting { get; set; }
        public DbSet<RMExportTermsWithRules> ExportTermsWithRules { get; set; }

        public DbSet<RMWorkflowData> WorkflowData { get; set; }
        public DbSet<RMWorkflowDefinition> WorkflowDefinition { get; set; }
        public DbSet<RMWorkflowInstance> WorkflowInstance{ get; set; }
        public DbSet<RMWorkflowSiteOwner> WorkflowSiteOwner { get; set; }
        public DbSet<RMWorkflowInformationOwner> WorkflowInformationOwner { get; set; }
        public DbSet<RMWorkflowStep> WorkflowStep { get; set; }
        public DbSet<RMWorkflowStepConfiguration> WorkflowStepConfiguration { get; set; }
        public DbSet<RMWorkflowHistory> WorkflowHistory { get; set; }
        public DbSet <RMWorkflowExcludeInstanceOwner> WorkflowExcludeInstanceOwner { get; set; }

        public DbSet<RMEmailItem> EmailItem { get; set; }
        public DbSet<RMScopePermission> ScopePermission { get; set; }
        public DbSet<RMScopeAccountMapping> ScopeAccountMapping { get; set; }
        public DbSet<RMScopePermissionJobInfo> ScopePermissionJobInfo { get; set; }
        public DbSet<RMSession> RMSession { get; set; }


        public DbSet<FSConnectionGroup> FSConnectionGroup { get; set; }

        public DbSet<FSConnectionGroupWithAgentMembership> FSConnectionGroupWithAgentMembership { get; set; }

        public DbSet<FSConnection> FSConnection{ get; set; }

        public DbSet<RMFSConnectionAndOwnerRelationship> RMFSConnectionAndOwnerRelationship { get; set; }

        public DbSet<RMMyhubReportJob> RMMyhubReportJobs { get; set; }

        public DbSet<RMFileSystemSetting> RMFileSystemSettings { get; set; }

        public DbSet<FileSystemTreeCache> FileSystemTreeCache { get; set; }
        
        public DbSet<RMFileSystemJobTimeReference> RMFileSystemJobTimeReference { get; set; }

        public DbSet<RMAgent> RMAgent { get; set; }

        public DbSet<RMCertificate> RMCertificate { get; set; }

        public DbSet<RMKeyValue> RMKeyValue { get; set; }
        public DbSet<RMJobSizeAndCountStatistics> RMJobSizeAndCountStatistics { get; set; }
        public DbSet<RMSiteDeletedSizeInfo> RMSiteDeletedSizeInfo { get; set; }
        public DbSet<RMScopeRoleAssignment> RMScopeRoleAssignment { get; set; }
        public DbSet<RMSecurityGroup> RMSecurityGroup { get; set; }
        public DbSet<RMSecurityGroupMembership> RMSecurityGroupMembership { get; set; }
        public DbSet<RMSecurityContainer> RMSecurityContainer { get; set; }
        public DbSet<RMSecurityGroupTermMapping> RMSecurityGroupTermMapping { get; set; }
        public DbSet<RMSecurityGroupRuleMapping> RMSecurityGroupRuleMapping { get; set; }

        public DbSet<RMPersonalSetting> RMPersonalSetting { get; set; }
        public DbSet<RMPersonalSettingShareMapping> RMPersonalSettingShareMapping { get; set; }
        public DbSet<RMDefaultPersonalSetting> RMDefaultPersonalSetting { get; set; }



        //public DbSet<RMServiceAccount> RMServiceAccounts { get; set; }
        public DbSet<RMMailbox> RMMailboxes { get; set; }
        public DbSet<RMRemoteNode> RMRemoteNodes { get; set; }
        public DbSet<RMLocalNode> RMLocalNodes { get; set; }

        public DbSet<RMSharePointOnPremiseSetting> RMSharePointOnPremiseSettings { get; set; }
        public DbSet<RMOneDriveSetting> RMOneDriveSettings { get; set; }
        public DbSet<RMRuleContainer> RMRuleContainers { get; set; }
        public DbSet<RMRuleContainerMembership> RMRuleContainerMemberships { get; set; }

        public DbSet<CSDApiKey> CSDApiKey { get; set; }

        public DbSet<SampleLocker> SampleLockers { get; set; }

        public DbSet<SPProvisioningContainer> SPProvisioningContainers { get; set; }

        #region Box

        public DbSet<RMBoxConnectionGroup> RMBoxConnectionGroups { get; set; }
        public DbSet<RMBoxConnection> RMBoxConnections { get; set; }
        public DbSet<RMBoxSetting> RMBoxSettings { get; set; }

        #endregion

        #region Google
        public DbSet<RMLabel> RMLabels { get; set; }
        
        public DbSet<RMGoogleLabelInfo> RMGoogleLabelInfo { get; set; }
        public DbSet<RMGoogleSetting> RMGoogleSettings { get; set; }
        public DbSet<RMGoogleSettingRuleMapping> RMGoogleSettingRuleMapping { get; set; }

        public DbSet<RMGDriveDeletedSizeInfo> RMGDriveDeletedSizeInfo { get; set; }
        
        public DbSet<GControlTaskAssigneeMapping> GControlUserTaskMapping { get; set; }
        #endregion

        #region Azure File Share

        public DbSet<RMAzureFileShareConnection> RMAzureFileShareConnections { get; set; }

        public DbSet<RMAzureFileShareConnectionGroup> RMAzureFileShareConnectionGroups { get; set; }

        public DbSet<RMAzureFileShareSetting> RMAzureFileShareSettings { get; set; }

        public DbSet<RMAzureFileShareSyncJobProcessInfo> RMAzureFileShareSyncJobProcessInfoes { get; set; }
        #endregion

        #region Customize Connector

        public DbSet<RMCustomizeConnectorContentSource> RMCustomizeConnectorContentSources { get; set; }

        public DbSet<RMCustomizeConnectorColumn> RMCustomizeConnectorColumns { get; set; }

        public DbSet<RMCustomizeConnectorTemplate> RMCustomizeConnectorTemplates { get; set; }

        public DbSet<RMCustomizeConnectorSourceAndTemplateMerge> RMCustomizeConnectorSourceAndTemplateMerges { get; set; }

        public DbSet<RMCustomizeConnectorTemplateAndColumnMerge> RMCustomizeConnectorTemplateAndColumnMerges { get; set; }

        #endregion
		
         #region Archiver
        public DbSet<RMStorageDeviceInfo> RMStorageInfos { get; set; }

        public DbSet<ArchiverIndexLock> ArchiverIndexLocks { get; set; }
        public DbSet<SettingProfiles> SettingProfile { get; set; }

        public DbSet<ArchiverIndexSubInfo> ArchiverIndexSubInfos { get; set; }
        public DbSet<EXOArchiverIndexSubInfo> EXOArchiverIndexSubInfos { get; set; }
        public DbSet<FSMasterIndex> FSMasterIndexs { get; set; }
        public DbSet<FSIndexSubInfo> FSIndexSubInfos { get; set; }
        public DbSet<MediaData> MediaDatas { get; set; }

        public DbSet<ArchiverSiteMasterIndex> ArchiverSiteMasterIndexs { get; set; }
        public DbSet<CommonSiteMasterIndex> CommonSiteMasterIndexes { get; set; }
        public DbSet<ArchiverDedupInfo> ArchiverDedupInfoes { get; set; }
        public DbSet<RetentionIndexSubInfo> RetentionIndexSubInfos { get; set; }
        public DbSet<IndexDataCacheInfo> IndexDataCacheInfos { get; set; }

        public DbSet<RMRuleMappings> RMRuleMappings { get; set; }

        public DbSet<RMRunningJobRuleMapping> RMRunningJobRuleMappings { get; set; }

        public DbSet<RMRetentionSiteInfo> RMRetentionSiteInfoes { get; set; }
        public DbSet<RMRetentionGDriveInfo> RMRetentionGDriveInfoes { get; set; }
        public DbSet<RMRetentionSimulateInfos> RMRetentionInfos { get; set; }
        public DbSet<RMRestoreSiteMapping> RMRestoreSiteMappings { get; set; }
        public DbSet<RMOptimizationSettingInfo> RMOptimizationSettingInfos { get; set; }
        public DbSet<RestoredSitesInfo> RestoredSitesInfos { get; set; }
        public DbSet<RMSODashboardMonthlySnapshot> RMSODashboardMonthlySnapshots { get; set; }
        #endregion

        #region Machine Learing
        public DbSet<RMMLTerm> RMMLTerms { get; set; }
        public DbSet<RMMLTrainingModel> RMMLTrainingModels { get; set; }
        public DbSet<RMMLTermModeMapping> RMMLTermModeMappings { get; set; }
        public DbSet<RMMLManualEmailNotification> RMMLManualEmailNotifications { get; set; }
        public DbSet<FeatureUsageLimit> FeatureUsageLimits { get; set; }
        public DbSet<AllocatedJobWeight> AllocatedJobWeights { get; set; }

        #endregion

        #region Archiver Full Text Index
        public DbSet<RMArchivedDataFullTextIndexSiteInfo> FullTextIndexSiteInfoes { get; set; }

        public DbSet<RMArchivedDataFullTextIndexJobInfo> FullTextIndexJobInfoes { get; set; }

        public DbSet<RMArchivedDataFullTextIndexEDiscoveryJobInfo> FullTextIndexEDiscoveryJobInfoes { get; set; }

        public DbSet<RMArchivedDataFullTextIndexSiteInfoesV1> FullTextIndexSiteInfoesV1 { get; set; }

        public DbSet<RMArchivedDataFullTextIndexJobInfoesV1> FullTextIndexJobInfoesV1 { get; set; }

        public DbSet<RMArchivedDataFullTextIndexEDiscoveryJobInfoesV1> FullTextIndexEDiscoveryJobInfoesV1 { get; set; }

        public DbSet<RMArchivedDataFullTextIndexCategory> FullTextIndexCategories { get; set; }
        #endregion

        public DbSet<RMEncryptKeyValue> RMEncryptKeyValues { get; set; }

        public DbSet<RMCustomIndexMetadata> RMCustomIndexMetadatas { get; set; }

        public DbSet<RMCustomMetadataColumn> RMCustomMetadataColumns { get; set; }

        public DbSet<RMCustomBarcodeTemplate> RMCustomBarcodeTemplates { get; set; }

        public DbSet<RMCustomBarcodeTemplateProperty> RMCustomBarcodeTemplateProperties { get; set; }

        public DbSet<RMCustomBarcodeTemplateSuite> RMCustomBarcodeTemplateSuites { get; set; }

        public DbSet<RMSiteStubSettingMapping> RMSiteStubSettingMappings { get; set; }

        public DbSet<RMStubDisposalSiteInfo> RMStubDisposalSiteInfoes { get; set; }

        public DbSet<RMDataIngestionJob> DataIngestionJobs { get; set; }

        public DbSet<RMDataIngestionMessage> DataIngestionMessages { get; set; }

        public DbSet<FSConnectionRelatedJobInfo> FSConnectionRelatedJobInfoes { get; set; }
        public DbSet<RMFSMyHubDashboard> RMFSMyHubDashboards { get; set; }

        public DbSet<RMDiscoverySpecificSite> RMDiscoverySpecificSites { get; set; }

        #endregion
        public string CacheKey
        {
            get
            {
                return this.SchemaName;
            }
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            if (!string.IsNullOrEmpty(this.SchemaName))
            {
                modelBuilder.HasDefaultSchema(this.SchemaName);
            }
            
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<DefaultValueAttribute,string>("SqlDefaultValue", (p, attributes) => attributes.SingleOrDefault().Value.ToString()));
            
            base.OnModelCreating(modelBuilder);
        }

    }   
}

