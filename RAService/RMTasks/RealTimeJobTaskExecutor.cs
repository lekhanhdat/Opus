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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Import;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.RMEmail;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Service.Services.Discovery.AOSP;
using AvePoint.RA.Service.Services.Discovery.FileSystem;
using AvePoint.RA.Service.Services.Discovery.Google;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Service.Services.Google;
using AvePoint.RA.Service.Services.RMMachineLearning;
using AvePoint.RA.Service.Services.TemplateManagement;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using RAGoogle.Restore.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.Service.Services.FileSystem;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Service.Services.Discovery.Common;
using AvePoint.RA.Contract.RMPublicAPI.OpusReport.SharePoint;
using Microsoft.Azure.Cosmos.Linq;

namespace AvePoint.RA.Service.RMTasks
{
    public class RealTimeJobTaskExecutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(RealTimeJobTaskExecutor));

        private IRMSharePointTaxonomyService RMSharePointTaxonomyService => PlatformWindsorManager.GetService<IRMSharePointTaxonomyService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMSharePointSettingsService RMSharePointSettingsService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();

        private IRMEmailManagementService RMEmailManagementService => PlatformWindsorManager.GetService<IRMEmailManagementService>();
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService<IUniqueIdSettingService>();
        private ILocationSynchronizationService LocationSynchronizationService => PlatformWindsorManager.GetService<ILocationSynchronizationService>();
        public IUpdateRecordLocationService UpdateRecordLocationService => PlatformWindsorManager.GetService<IUpdateRecordLocationService>();
        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();

        public IManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IManualApprovalService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        public IRMJobService RMJobService => PlatformWindsorManager.GetService<IRMJobService>();
        public ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        public ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        public IImportTRIMService ImportTRIMService => PlatformWindsorManager.GetService<IImportTRIMService>();
        public IRMReportService RMReportService => PlatformWindsorManager.GetService<IRMReportService>();
        public ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();

        public IRMCollectionDataService ReportCollectionService => PlatformWindsorManager.GetService<IRMCollectionDataService>();
        public IEnforceRetentionService EnforceRetentionService => PlatformWindsorManager.GetService<IEnforceRetentionService>();
        public IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        private IRMPhysicalRecordSettingsService RMPhysicalRecordSettingsService => PlatformWindsorManager.GetService<IRMPhysicalRecordSettingsService>();
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IWorkspaceHoldService WorkspaceHoldService => PlatformWindsorManager.GetService<IWorkspaceHoldService>();

        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        public IRMSecurityContainerService RMSecurityContainerService => PlatformWindsorManager.GetService<IRMSecurityContainerService>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        public IRMLocalNodeService LocalNodeService => PlatformWindsorManager.GetService<IRMLocalNodeService>();
        public IRMAOSNotificationService AOSNotificationService => PlatformWindsorManager.GetService<IRMAOSNotificationService>();
        public IRMSharePointOnPremSettingsService RMSharePointOnPremSettingsService => PlatformWindsorManager.GetService<IRMSharePointOnPremSettingsService>();
        public IRMSharePointOnPremScanNodeService SharePointOnPremScanNodeService => PlatformWindsorManager.GetService<IRMSharePointOnPremScanNodeService>();
        public IRMOneDriveSettingsService RMOneDriveSettingsService => PlatformWindsorManager.GetService<IRMOneDriveSettingsService>();
        private IPhysicalReqeustService PhysicalRequestService => PlatformWindsorManager.GetService<IPhysicalReqeustService>();

        private IRMArchivedFullTextIndexService ArchivedFullTextIndexService => PlatformWindsorManager.GetService<IRMArchivedFullTextIndexService>();

        private IDashboardService DashboardService => PlatformWindsorManager.GetService<IDashboardService>();

        public ITenantUpgradeService TenantUpgradeService => PlatformWindsorManager.GetService<ITenantUpgradeService>();
        private IRMManualApprovalService RMManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();

        public IDisposalReportService DisposalReportService => PlatformWindsorManager.GetService<IDisposalReportService>();

        public ICreateAndDestryoedReportService CreateAndDestryoedReportService => PlatformWindsorManager.GetService<ICreateAndDestryoedReportService>();

        public ITermUsageReportService TermUsageReportService => PlatformWindsorManager.GetService<ITermUsageReportService>();

        public IRMAzureFileSettingsService AzureFileSettingService => PlatformWindsorManager.GetService<IRMAzureFileSettingsService>();
        public IPickListService PickListService => PlatformWindsorManager.GetService<IPickListService>();
        public IRMMLTermService MLTermService => PlatformWindsorManager.GetService<IRMMLTermService>();
        public IRMMLManualApprovalService MLManualApprovalService => PlatformWindsorManager.GetService<IRMMLManualApprovalService>();
        public IArchiverRuleService ArchiverRuleService => PlatformWindsorManager.GetService<IArchiverRuleService>();

        public IRMDiscoveryOffice365OptimizationService OptimizationService => new RMDiscoveryOffice365OptimizationService();

        public IRMDiscoveryAOSPOptimizationService AOSPOptimizationService => new RMDiscoveryAOSPOptimizationService();

        public IRMDiscoveryOffice365ConfigurationService ConfigurationService = new RMDiscoveryOffice365ConfigurationService();

        public IRMDiscoverySpecificSiteService SpecificSiteService = new RMDiscoverySpecificSiteService();

        public IRMDiscoveryOffice365ExportJobService DiscoveryExportService => PlatformWindsorManager.GetService<IRMDiscoveryOffice365ExportJobService>();

        public IRMDiscoveryGoogleConfigurationService GoogleConfigurationService = new RMDiscoveryGoogleConfigurationService();

        public IRMDiscoveryAOSPConfigurationService AOSPConfigurationService = new RMDiscoveryAOSPConfigurationService();

        public IRMDiscoveryFSConfigurationService FileSystemConfigurationService = new RMDiscoveryFSConfigurationService();

        public IRMDiscoveryOffice365ProfileService ProfileService = new RMDiscoveryOffice365ProfileService();
        
        public IRMDiscoveryGoogleProfileService DiscoveryProfileService = new RMDiscoveryGoogleProfileService();

        public ITrainingReportService TrainingReportService => PlatformWindsorManager.GetService<ITrainingReportService>();

        public IRMBoxSettingsService BoxSettingsService => PlatformWindsorManager.GetService<IRMBoxSettingsService>();

        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private IRMMailboxDao MailBoxDao => PlatformWindsorManager.GetService<IRMMailboxDao>();

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        public IRMGoogleJobService GoogleJobService => PlatformWindsorManager.GetService<IRMGoogleJobService>();

        public IStubSettingService StubSettingService => PlatformWindsorManager.GetService<IStubSettingService>();
        private IRMTeamsSettingsService RMTeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();

        public IRMTeamsSettingsService TeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
       
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();

        public IDeclaredRecordsMigrationService DeclaredRecordsMigrationService => PlatformWindsorManager.GetService<IDeclaredRecordsMigrationService>();

        private IRMSharePointSiteMetricsReportService SharePointReportExportService => PlatformWindsorManager.GetService<IRMSharePointSiteMetricsReportService>();

        public IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        public IRMDiscoveryPlanProfileService DiscoveryPlanProfileService => PlatformWindsorManager.GetService<IRMDiscoveryPlanProfileService>();
        public RMFSUpgradeDataService FileSystemSettingService = new();

        private const string JpmcUpgradeStatusKey = "JPMC_UPGRADE_STATUS";

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                #region init 
                //UpdateRecordLocationService = (IUpdateRecordLocationService)PlatformWindsorManager.GetService(typeof(IUpdateRecordLocationService));
                //ManualApprovalService = (IManualApprovalService)PlatformWindsorManager.GetService(typeof(IManualApprovalService));
                //LocationManagementService = (ILocationManagementService)PlatformWindsorManager.GetService(typeof(ILocationManagementService));
                //RMReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                //TaxonomyService = (ITaxonomyService)PlatformWindsorManager.GetService(typeof(ITaxonomyService));
                //ReportCollectionService = (IRMCollectionDataService)PlatformWindsorManager.GetService(typeof(IRMCollectionDataService));
                //EnforceRetentionService = (IEnforceRetentionService)PlatformWindsorManager.GetService(typeof(IEnforceRetentionService));
                //ExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                //RMPhysicalRecordSettingsService = (IRMPhysicalRecordSettingsService)PlatformWindsorManager.GetService(typeof(IRMPhysicalRecordSettingsService));
                //CommonService = (ICommonService)PlatformWindsorManager.GetService(typeof(ICommonService));
                //ImportTRIMService = PlatformWindsorManager.GetService<IImportTRIMService>();
                //RMJobService= PlatformWindsorManager.GetService<IRMJobService>();
                //PermissionManagementService = (IPermissionManagementService)PlatformWindsorManager.GetService(typeof(IPermissionManagementService));
                //RMSecurityContainerService = (IRMSecurityContainerService)PlatformWindsorManager.GetService(typeof(IRMSecurityContainerService));
                //RemoteNodeService = (IRMRemoteNodeService)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeService));
                //LocalNodeService = (IRMLocalNodeService)PlatformWindsorManager.GetService(typeof(IRMLocalNodeService));
                //AOSNotificationService = (IRMAOSNotificationService)PlatformWindsorManager.GetService(typeof(IRMAOSNotificationService));
                //RMSharePointOnPremSettingsService=(IRMSharePointOnPremSettingsService)PlatformWindsorManager.GetService(typeof(IRMSharePointOnPremSettingsService));
                //SharePointOnPremScanNodeService = (IRMSharePointOnPremScanNodeService)PlatformWindsorManager.GetService(typeof(IRMSharePointOnPremScanNodeService));
                //RMOneDriveSettingsService = (IRMOneDriveSettingsService)PlatformWindsorManager.GetService(typeof(IRMOneDriveSettingsService));
                //PhysicalRequestService = (IPhysicalReqeustService)PlatformWindsorManager.GetService(typeof(IPhysicalReqeustService));
                //TenantUpgradeService = PlatformWindsorManager.GetService<ITenantUpgradeService>();
                //DisposalReportService = PlatformWindsorManager.GetService<IDisposalReportService>();
                //CreateAndDestryoedReportService = PlatformWindsorManager.GetService<ICreateAndDestryoedReportService>();
                //TermUsageReportService = PlatformWindsorManager.GetService<ITermUsageReportService>();
                //AzureFileSettingService = PlatformWindsorManager.GetService<IRMAzureFileSettingsService>();
                #endregion
                logger.Info("begin to get job queue job.");

                try
                {
                    var jobMessages = JobQueueService.GetDBJobMessageGroupByTenant(15);
                    logger.Info($"get job message:{jobMessages?.Count}");
                    OutPutJobQueueMessageDetail(jobMessages);
                    if (jobMessages?.Count > 0)
                    {
                        var parallelMsgs = jobMessages.Where(t=>t.JobType!= JobType.SyncNodesFromAOS).GroupBy(m => m.TenantGroupId);
                        if(parallelMsgs.Count() > 0)
                        {
                            AveTenantTasks.RunParallel(parallelMsgs, 1, new System.Threading.CancellationTokenSource(), async msgGroup =>
                            {
                                try
                                {
                                    foreach (var msg in msgGroup)
                                    {
                                        try
                                        {
                                            if (CheckIfAllowRunJob(msg))
                                            {
                                                var pars = new List<JobQueueDto>() { msg };
                                                logger.Info("process to start job queue job, tenantId:{0}.", msg.TenantGroupId);
                                                await TenantUtil.RunUnderTenantAsync(msg.TenantGroupId, msg.JobRunByUser, msg.ClientIP, msg.PartnerUser, RunJobAsync, pars);
                                            }
                                            else
                                            {
                                                logger.Info($"skip run job message:{msg.TenantGroupId}, {msg.JobType}, {msg.Parameters}");
                                                ResetQueueJob(msg);
                                            }
                                        }
                                        catch(Exception e)
                                        {
                                            logger.Error($"Check if allow run job failed, reset job queue message status, error : {e}");
                                            ResetQueueJob(msg);
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                    logger.Warn(e.ToString());
                                }
                            });
                        }
                        
                        var syncNodeJobMsgs = jobMessages.Where(t => t.JobType == JobType.SyncNodesFromAOS);
                        foreach (var msg in syncNodeJobMsgs)
                        {
                            try
                            {
                                if (CheckIfAllowRunJob(msg))
                                {
                                    var pars = new List<JobQueueDto>() { msg };
                                    logger.Info("process to start sync node from aos job, tenantId:{0}.", msg.TenantGroupId);
                                    await TenantUtil.RunUnderTenantAsync(msg.TenantGroupId, msg.JobRunByUser, msg.ClientIP, msg.PartnerUser, RunJobAsync, pars);
                                }
                                else
                                {
                                    logger.Info($"skip sync node from AOS run job message:{msg.TenantGroupId}, {msg.JobType}, {msg.Parameters}");
                                    ResetQueueJob(msg);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error($"Check sync node from AOS if allow run job failed, reset job queue message status, error : {e}");
                                ResetQueueJob(msg);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while excute job task, ERROR:{0}", ex.ToString());
            }

        }

        private void OutPutJobQueueMessageDetail(List<JobQueueDto> jobQueueDtos)
        {
            try
            {
                if (jobQueueDtos != null && jobQueueDtos.Count > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (JobQueueDto jobQueueDto in jobQueueDtos)
                    {
                        stringBuilder.AppendLine($"JobQueueDto.JobType:{jobQueueDto.JobType}.MessageId:{jobQueueDto.MessageId}.TenantGroupId:{jobQueueDto.TenantGroupId}.JobRunByUser:{jobQueueDto.JobRunByUser}.");
                    }
                    logger.Info($"OutPutJobQueueMessageDetail:{stringBuilder.ToString()}.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Error occurred while OutPutJobQueueMessageDetail, ERROR:{ex}.");
            }
        }

        private bool IsNotLimitTenantJob(JobQueueDto jqDto) 
        {
            var whiteJobList = new List<JobType> { JobType.SyncNodesFromAOS, JobType.RMArchiverBackup, JobType.RMEndUserArchiverBackup, JobType.ExportAdvanceSeachResult,
             JobType.RebuildSOJobReport, JobType.RebuildEncryptKeyValue, JobType.BuildRunningJobReport, JobType.ExportRestoreCenterSeachResult, JobType.ExportDecryptIndexDB,
              JobType.BaseArchiveJobIdMultiRestore, JobType.MultiSiteCollectionRestore, JobType.ApplyClassCode, JobType.DispatchedJob , JobType.PreviewRestore };  //白名单List 不受限制。
            whiteJobList.AddRange(RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToArray());

            if (whiteJobList.Contains(jqDto.JobType)) 
            {
                return true;
            }
            if (jqDto.JobType == JobType.ArchiverRestore || jqDto.JobType == JobType.ArchiverToSpoRestore || jqDto.JobType == JobType.ArchiverOutPlaceRestore || jqDto.JobType == JobType.StubOopRestore || jqDto.JobType == JobType.AOSPRestore
                || jqDto.JobType == JobType.TeamsArchiverRestore || jqDto.JobType == JobType.TeamsOutPlaceRestore || jqDto.JobType == JobType.MailBoxArchiverRestore
                || jqDto.JobType == JobType.StubArchiverRestore || jqDto.JobType == JobType.M365InPlaceArchiverRestore
                )
            {
                try
                {
                    var restoreSetting = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(jqDto.Parameters);
                    if (restoreSetting != null && restoreSetting.IsEndUserJob)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("error occurred while check job if is EndUserRestoreJob, ERROR:{0}", e.ToString());
                }
            }
            return false;

        }

        private bool CheckIfAllowRunJob(JobQueueDto jqDto)
        {
            return TenantUtil.RunUnderTenant(
                jqDto.TenantGroupId, 
                jqDto.JobRunByUser, 
                () =>
                {
                    if (!AllowRunJobForJpmcUpgrade(jqDto))
                    {
                        return false;
                    }
                    if (!IsNotLimitTenantJob(jqDto) && TenantJobReachedLimit())
                    {
                        return false;
                    }
                    if (!CheckLicenseAvailable(jqDto.JobType) && jqDto.ProductType == ProductType.None)
                    {
                        logger.Warn($"tenant: {jqDto.TenantGroupId}, {jqDto.JobType} license is expired.");
                        if (jqDto.JobType != JobType.CloudArchiverMigration)
                        {
                            JobQueueService.DeleteDBJobQueueMessage(jqDto.MessageId, jqDto.TenantGroupId);
                            logger.Info("delete job message success, jobId:{0}, TenantId:{1},", jqDto.TenantGroupId);
                        }
                        return false;
                    }
                    if (RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.Contains(jqDto.JobType))
                    {
                        if ((jqDto.JobType == JobType.ArchiverRestore || jqDto.JobType == JobType.TeamsArchiverRestore || jqDto.JobType == JobType.TeamsOutPlaceRestore || jqDto.JobType == JobType.AOSPRestore || jqDto.JobType == JobType.ArchiverToSpoRestore || jqDto.JobType == JobType.StubArchiverRestore || jqDto.JobType == JobType.M365InPlaceArchiverRestore)
                            && !CheckIsAllowAdminRestore(jqDto.Parameters, jqDto.JobType))
                        {
                            return false;
                        }
                        if (jqDto.JobType == JobType.ArchiverByHSMXml)
                        {
                            HSMArchiverDto jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<HSMArchiverDto>(jqDto.Parameters);
                            var runningUrl = JobMonitorService.GetRunningArchiverJobSiteUrl(new List<JobType>() { JobType.ArchiverByHSMXml }, jobParaInfo.SiteUrls);
                            if (runningUrl != null && runningUrl.Count > 0)
                            {
                                logger.Info($"ArchiverByHSMXml runningUrl count: {runningUrl?.Count ?? 0}, urls: [{string.Join(",", runningUrl ?? new List<string>())}]");
                                return false;
                            }
                        }
                        return CheckMainJobCount(jqDto.Parameters, jqDto.JobType).GetAwaiter().GetResult();
                    }
                    switch (jqDto.JobType)
                    {
                        case JobType.SyncNodesFromAOS:
                            return CheckSyncNodesFromAOSJob();
                        //case JobType.ArchiverRestore:
                        case JobType.ArchiverOutPlaceRestore:
                            return CheckIsAllowAdminRestore(jqDto.Parameters, jqDto.JobType);
                        case JobType.GoogleArchiverRestore:
                            return CheckGoogleIsAllowAdminRestore(jqDto.Parameters, jqDto.JobType);
                        case JobType.SpecifySitesArchiverBackup:
                            return CheckMainJobCount4SpecifySites(jqDto.Parameters).GetAwaiter().GetResult();
                        case JobType.EXOApplySetting:
                        case JobType.ApplySharePointSettings:
                        case JobType.ApplyTeamsSettings:
                            return !CheckHasRunningJobApplySetting(jqDto.JobType);
                        case JobType.MultiGeoMainDCSyncCommonData:
                        case JobType.MultiGeoOtherDCSyncCommonData:
                            return !HasRunningSyncCommonDataJob(jqDto.JobType);
                        default:
                            return true;
                    }
                });
        }

        private bool HasRunningSyncCommonDataJob(JobType jobType)
        {
            return JobMonitorService.GetRunningJobs(jobType).Count > 0;
        }

        private bool AllowRunJobForJpmcUpgrade(JobQueueDto jqDto)
        {
            if (!RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return true;
            }

            if (jqDto.JobType == JobType.MigrateDataCosmosDbForJPMC 
                || jqDto.JobType == JobType.MultiGeoMainDCSyncCommonData
                || jqDto.JobType == JobType.MultiGeoOtherDCSyncCommonData)
            {
                return true;
            }

            var status = GetJpmcUpgradeStatus();
            return status == 3;
        }

        private int? GetJpmcUpgradeStatus()
        {
            var keyValue = KeyValueDao.GetValueByKey(JpmcUpgradeStatusKey);
            if (keyValue == null || string.IsNullOrWhiteSpace(keyValue.Value))
            {
                return null;
            }

            return int.TryParse(keyValue.Value, out var status) ? status : null;
        }

        private async Task<bool> CheckMainJobCount(string paremeters, JobType jobType)
        {
            try
            {
                var currentTenant = GetCurrentO365Tenant(paremeters, jobType);
                logger.Debug($"Get current o365 tenant id: {currentTenant} for job type: {jobType}.");
                if(string.IsNullOrEmpty(currentTenant))
                {
                    logger.Warn($"unable get currentTeant id, jobType:{jobType}, check current aos tenant sum job count");
                    return !TenantJobReachedLimit();
                }
                var controller = new RMO365TenantSubJobController();
                var tenantSubscribedInfoes = await controller.GetTenantSubscribedInfoToCache();
                logger.Debug($"Get tenant subscribed info from cache, tenants: {string.Join("; ", tenantSubscribedInfoes.Select(info => info.Id))}.");
                var tenantSubJobControlDefinitions = await controller.GetTenantSubJobControlDefinitions(tenantSubscribedInfoes);
                logger.Debug($"Get tenant sub job control definitions from cache, tenants: {string.Join("; ", tenantSubJobControlDefinitions.Keys)}.");
                var runningJobCount = GetCurrentTenantMainJobCount(currentTenant);
                var maxRunJobCount = 5;
                var tenantSubscribedInfo = tenantSubscribedInfoes.Where(info => info.Id == currentTenant).FirstOrDefault();
                if (tenantSubscribedInfo != null)
                {
                    if (!tenantSubJobControlDefinitions.ContainsKey(currentTenant))
                    {
                        logger.Info($"Tenant [{tenantSubscribedInfo.Id}] does not have sub job control definition, can not run so job.");
                        return false;
                    }
                    maxRunJobCount = controller.CalculateSubJobCount(tenantSubscribedInfo.UserSeats, tenantSubJobControlDefinitions[currentTenant]);
                    logger.Info($"Tenant [{tenantSubscribedInfo.Id}] has user seats [{tenantSubscribedInfo.UserSeats}], " +
                        $"max can run sub job count: [{maxRunJobCount}], " +
                        $", running main job count: [{runningJobCount}]" +
                        $"can run sub job count: [{maxRunJobCount - runningJobCount}].");
                }
                else
                {
                    logger.Warn($"Teannt [{currentTenant}], jobType:{jobType}, use hard code userSeat 5, " +
                        $"running main job count: [{runningJobCount}],can run sub job count: [{maxRunJobCount - runningJobCount}].");
                }
                return runningJobCount < maxRunJobCount;
            }
            catch(Exception e) 
            {
                logger.Warn($"Some thing went wrong when check so main job count ,error:{e}");
                throw;
            }
        }

        private string GetCurrentO365Tenant(string parameter, JobType jobType)
        {
            try
            {
                switch (jobType)
                {
                    case JobType.RMEndUserArchiverBackup:
                        var endUserTreeNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<EndUserArchiveContainerConfig>(parameter);
                        return endUserTreeNodeInfo?.SiteCollectionConfigs?
                            .FirstOrDefault(config => Guid.TryParse(config?.Office365TenantId, out Guid res) && res != Guid.Empty)?
                            .Office365TenantId;
                    case JobType.RMArchiverBackup:
                    case JobType.RecordsDisposal:
                    case JobType.OneDriveRecordsDisposal:
                    case JobType.SOPreScan:
                    case JobType.ApprovalProcessArchive:
                    case JobType.ConvertStub:
                        var currentTenant = string.Empty;
                        RMSPTreeNode treeNodeInfo = null;
                        if (jobType == JobType.ApprovalProcessArchive)
                        {
                            var siteTreeNodes = RMArchiverSettingsService.GetApprovalProcessJobSites().GetAwaiter().GetResult();
                            treeNodeInfo = siteTreeNodes.FirstOrDefault();
                        }
                        else if(jobType == JobType.ConvertStub)
                        {
                            ConvertStubDto convertStubDto = SerializerHelper.DeserializeByDataContractSerializer<ConvertStubDto>(parameter);
                            treeNodeInfo = convertStubDto.NodeSetting;
                        }
                        else
                        {
                            treeNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(parameter);
                        }
                        var selectedTreeNodeId = new Guid(treeNodeInfo?.SPObjectId);
                        var selectedTreeSiteNode = treeNodeInfo?.GetSiteCollectionNode();
                        var selectedTreeSiteNodeId = selectedTreeSiteNode != null ? new Guid(selectedTreeSiteNode.SPObjectId) : Guid.Empty;
                        if (selectedTreeSiteNodeId == Guid.Empty)
                        {
                            var remoteNode = RMRemoteNodeDao.GetRemoteNodeByParentId(selectedTreeNodeId);
                            if (remoteNode != null)
                            {
                                currentTenant = remoteNode.TenantId;
                            }
                        }
                        else
                        {
                            var remoteNode = RMRemoteNodeDao.GetRemoteNodeById(selectedTreeSiteNodeId);
                            if (remoteNode != null)
                            {
                                currentTenant = remoteNode.TenantId;
                            }
                        }
                        return currentTenant;
                    case JobType.DiscoverOptimization:
                        var dsoParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationJobInfo>(parameter);
                        return dsoParaInfo.o365Info.UniqueId.ToString();
                    case JobType.DiscoveryPlanProOptimization:
                        var dppOptimizationParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationJobInfo>(parameter);
                        return dppOptimizationParaInfo.o365Info.UniqueId.ToString();
                    case JobType.DiscoveryAOSPOptimization:
                        var aospParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverAOSPOptimizationJobInfo>(parameter);
                        return aospParaInfo.o365Info.UniqueId.ToString();
                    case JobType.DiscoveryPreScan:
                        var dopParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationPreScanJobInfo>(parameter);
                        return dopParaInfo.SettingInfo.O365TenantId;
                    case JobType.DiscoveryPlanProScan:
                        var dppScanParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationPreScanJobInfo>(parameter);
                        return dppScanParaInfo.SettingInfo.O365TenantId;
                    case JobType.EXORecordsDisposal:
                        var mailTenant = string.Empty;
                        var selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(parameter);
                        if(selectedNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                        {
                            var remoteMail = MailBoxDao.GetEmailByEmailGroupId(selectedNode.Id);
                            if(remoteMail == null)
                            {
                                return mailTenant;
                            }
                            mailTenant = remoteMail.TenantId;
                        }
                        else
                        {
                            var remoteMail = MailBoxDao.GetEmailById(selectedNode.Id);
                            if (remoteMail == null)
                            {
                                return mailTenant;
                            }
                            mailTenant = remoteMail.TenantId;
                        }
                        return mailTenant;
                    case JobType.ExportSiteMetrics:
                        return GetJpmcTenant();
                    case JobType.TeamsRecordsDisposal:
                    case JobType.TeamsArchiverBackup:
                    case JobType.TeamsPreScan:
                        currentTenant = string.Empty;
                        treeNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(parameter);
                        if (treeNodeInfo == null)
                        {
                            logger.Warn("The tree node info is null, cannot get the tenant id.");
                            return currentTenant;
                        }

                        selectedTreeSiteNode = treeNodeInfo.GetSiteCollectionNode();
                        selectedTreeSiteNodeId = selectedTreeSiteNode != null ? new Guid(selectedTreeSiteNode.SPObjectId) : Guid.Empty;
                        if (selectedTreeSiteNodeId != Guid.Empty)
                        {
                            var remoteNode = RMRemoteNodeDao.GetRemoteNodeById(selectedTreeSiteNodeId);
                            if (remoteNode != null && !string.IsNullOrEmpty(remoteNode.TenantId)) return remoteNode.TenantId;
                        }

                        selectedTreeNodeId = new Guid(treeNodeInfo.SPObjectId);

                        var selectedTreeTeamsNode = treeNodeInfo.GetTeamsNode();
                        var selectedTreeTeamsNodeId = selectedTreeTeamsNode != null ? new Guid(selectedTreeTeamsNode.TeamsId) : Guid.Empty;

                        if (selectedTreeTeamsNodeId == Guid.Empty)
                        {
                            var remoteNode = RMRemoteNodeDao.GetRemoteNodeByParentId(selectedTreeNodeId);
                            if (remoteNode == null)
                            {
                                return currentTenant;
                            }
                            currentTenant = remoteNode.TenantId;
                        }
                        else
                        {
                            var remoteNode = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedTreeTeamsNodeId.ToString()).Item1;
                            if (remoteNode == null)
                            {
                                return currentTenant;
                            }
                            currentTenant = remoteNode.TenantId;
                        }
                        return currentTenant;
                    case JobType.SpecifySitesArchiverBackup:
                        SpecifySitesArchiverBackupParameters sitesParameters = SerializerHelper.DeserializeByDataContractSerializer<SpecifySitesArchiverBackupParameters>(parameter);
                        return RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(sitesParameters.SitesUrlList.First())?.TenantId ?? string.Empty;
                    case JobType.SpecifyTeamsArchiverBackup:
                        SpecifyTeamsArchiverBackupParameters parameters = SerializerHelper.DeserializeByDataContractSerializer<SpecifyTeamsArchiverBackupParameters>(parameter);
                        return RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(parameters.TeamIdList.First()).Item1.TenantId;
                    case JobType.AOSPRestore:
                    case JobType.ArchiverRestore:
                    case JobType.ArchiverToSpoRestore:
                    case JobType.StubArchiverRestore:
                    case JobType.M365InPlaceArchiverRestore:
                        RestoreSettingAndTree restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(parameter);
                        string tempSitePath;
                        if (restoreSettingAndTree?.Setting?.NodeObjects == null || restoreSettingAndTree?.Setting?.NodeObjects.Count==0)
                        {
                            logger.Warn($"tempSitePath is empty,maybe this is rerun restore job.");
                            tempSitePath = restoreSettingAndTree?.Setting?.SiteUrl;
                            logger.Warn($"tempSitePath is empty,maybe this is rerun restore job.site url:{tempSitePath}");
                        }
                        else
                        {
                            tempSitePath = restoreSettingAndTree?.Setting?.NodeObjects[0]?.SitePath;
                        }                        
                        return RestoreSearchService.GetO365TenantId(tempSitePath);
                    case JobType.TeamsArchiverRestore:
                    case JobType.TeamsOutPlaceRestore:
                        RestoreSettingAndTree restoreSetting = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(parameter);
                        string mailAddress = restoreSetting?.Setting?.NodeObjects[0]?.SitePath;
                        return RestoreSearchService.GetO365TeamsTenantId(mailAddress);
                    case JobType.ArchiverByHSMXml:
                        HSMArchiverDto hsmDto = SerializerHelper.DeserializeByDataContractSerializer<HSMArchiverDto>(parameter);
                        return hsmDto.O365TenantId;
                    default:
                        return string.Empty;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Some thing went wrong when check so main job count ,error:{e}");
                return string.Empty;
            }
        }

        private async Task<bool> CheckMainJobCount4SpecifySites(string paremeters)
        {
            try
            {
                var paramsDto = SerializerHelper.DeserializeByDataContractSerializer<SpecifySitesArchiverBackupParameters>(paremeters);
                List<string> sitesUrlList = paramsDto.SitesUrlList;
                var selectedRemoteNode = RABrowserClient.GetRemoteSiteCollectionByListUrlV1(sitesUrlList.First());
                var currentTenant = selectedRemoteNode.TenantId;
                var controller = new RMO365TenantSubJobController();
                var tenantSubscribedInfoes = await controller.GetTenantSubscribedInfoToCache();
                var tenantSubJobControlDefinitions = await controller.GetTenantSubJobControlDefinitions(tenantSubscribedInfoes);
                var runningJobCount = GetCurrentTenantMainJobCount(currentTenant);
                var tenantSubscribedInfo = tenantSubscribedInfoes.Where(info => info.Id == currentTenant).FirstOrDefault();
                if (tenantSubscribedInfo != null)
                {
                    var maxRunJobCount = controller.CalculateSubJobCount(tenantSubscribedInfo.UserSeats, tenantSubJobControlDefinitions[currentTenant]);
                    var runningSubJobCount = await SubJobDao.GetRunningAndRunnableSubJobCountAsync(tenantSubscribedInfo.Id, RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToArray());
                    return runningJobCount + runningSubJobCount < maxRunJobCount;
                }

                return true;
            }
            catch (Exception e)
            {
                logger.Warn($"Some thing went wrong when check so main job count ,error:{e}");
                throw;
            }
        }

        private int GetCurrentTenantMainJobCount(string o365TenantId)
        {
            var runningMainJobs = JobMonitorService.GetRunningJobs([.. RMO365TenantSubJobControlConstants.CONTROLLED_JOBS]);
            var subJobs = SubJobDao.GetOneSubJobByParentIds(runningMainJobs.Select(job => job.Id).ToList());
            return subJobs.Count(job => job != null && job.O365TenantId == o365TenantId);
        }

        private string GetJpmcTenant()
        {
            var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization");
            List<JPMCTenantConfig> configs = null;
            var tenant = string.Empty;
            if (jsonConfig != null)
            {
                configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig.Value);
                var configSiteUrls = configs.Select(c => c.ConfigSiteUrl).ToList();
                var remoteNodes = RMRemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(configSiteUrls);
                if (remoteNodes.Count != 0)
                {
                    var firstNode = remoteNodes.Where(sc => sc.NodeType != RemoveNodeType.SkyDrivePro).FirstOrDefault();
                    if(firstNode != null)
                    {
                        tenant = firstNode.TenantId;
                    }
                }
            }
            return tenant;
        }

        /// <summary>
        /// 由于从job queue中取出job时，修改了status状态=1，需要重新设置为0，否则job无法在下次被调度
        /// </summary>
        /// <param name="jqDto"></param>
        private void ResetQueueJob(JobQueueDto jqDto)
        {
            TenantUtil.RunUnderTenant(
                jqDto.TenantGroupId,
                jqDto.JobRunByUser,
                () =>
                {
                    JobQueueService.ResetDBJobQueue(jqDto.MessageId, jqDto.TenantGroupId);
                });
        }

        //Check the restore job to be started to see if there is a restore with the same scope running. If there is, wait in the Job queue instead of skipping it directly
        private bool CheckIsAllowAdminRestore(string param, JobType jobType)
        {
            try
            {
                List<JobType> types = new List<JobType>() { jobType };
                logger.Info($"start get restore job from job queue,type:{jobType}");
                var restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(param);
                bool isEndUserRestore = restoreSettingAndTree.IsEndUserJob;
                bool skipCheckRunningRestoreJob = KeyValueDao.HasSkipCheckRunningRestoreJob();
                if (isEndUserRestore)
                {
                    logger.Info($"this job is end user restore,return true");
                    return true;
                }
                if (skipCheckRunningRestoreJob)
                {
                    logger.Info($"this tenant has enable the function skipCheckRunningRestoreJob and the value is true");
                    return true;
                }
                var scope = restoreSettingAndTree.Tree.First().FullPath;
                bool hasRunningJob = JobMonitorService.HasRunningArchiverJobOnScope(types, scope) || JobMonitorService.HasStoppingArchiverJobOnScope(types, scope);
                logger.Info($"isEndUserRestore:{isEndUserRestore},hasRunning job:{hasRunningJob},scope:{scope}");
                if (jobType == JobType.TeamsArchiverRestore || jobType == JobType.MailBoxArchiverRestore || jobType == JobType.TeamsOutPlaceRestore)
                {
                    var runningJobIds = JobMonitorService.GetRunningArchiverJobOnScope(types, scope);
                    if (runningJobIds != null && runningJobIds.Count > 0)
                    {
                        foreach (var jobId in runningJobIds)
                        {
                            var jobSetting = SubJobDao.GetJobContextSettingByMainJobId(jobId);
                            if (!string.IsNullOrEmpty(jobSetting))
                            {
                                var setting = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(jobSetting);
                                if (setting.Setting.RestoreTypeSelect == restoreSettingAndTree.Setting.RestoreTypeSelect)
                                {
                                    hasRunningJob = true;
                                    break;
                                }
                                else
                                {
                                    hasRunningJob = false;
                                }
                            }
                        }
                    }
                }
                return !hasRunningJob;
            }
            catch (Exception e)
            {
                logger.Warn($"some thing went wrong when run restore job ,error:{e}");
                return true;
            }
        }
        private bool CheckGoogleIsAllowAdminRestore(string param, JobType jobType)
        {
            try
            {
                List<JobType> types = new List<JobType>() { jobType };
                logger.Info($"start get restore job from job queue,type:{jobType}");
                var restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<GDriveRestoreSettingAndTree>(param);
                bool isEndUserRestore = restoreSettingAndTree.IsEndUserJob;
                bool skipCheckRunningRestoreJob = KeyValueDao.HasSkipCheckRunningRestoreJob();
                if (isEndUserRestore)
                {
                    logger.Info($"this job is end user restore,return true");
                    return true;
                }
                if (skipCheckRunningRestoreJob)
                {
                    logger.Info($"this tenant has enable the function skipCheckRunningRestoreJob and the value is true");
                    return true;
                }
                var scope = restoreSettingAndTree.Tree.First().FullPath;
                bool hasRunningJob = JobMonitorService.HasRunningArchiverJobOnScope(types, scope) || JobMonitorService.HasStoppingArchiverJobOnScope(types, scope);
                logger.Info($"isEndUserRestore:{isEndUserRestore},hasRunning job:{hasRunningJob},scope:{scope}");
                return !hasRunningJob;
            }
            catch (Exception e)
            {
                logger.Warn($"some thing went wrong when run restore job ,error:{e}");
                return true;
            }
        }

        private bool TenantJobReachedLimit()
        {
            int maxJobCount = RMJobService.GetTenantMainJobCount();
            var jobCount = JobMonitorService.GetRunningJobsCount(JobType.All);
            var discoveryJobCount = JobMonitorService.GetRunningJobsCount(JobType.DiscoveryJob);
            var highJobCount = JobMonitorService.GetRunningJobsCount([.. RMO365TenantSubJobControlConstants.CONTROLLED_JOBS]);
            if ((jobCount - discoveryJobCount - highJobCount) >= maxJobCount)
            {
                logger.Info("current job count reach limited, tenantId:{0}.", TenantLocalValue.LogonGroupId);
                return true;
            }
            return false;
        }

        private bool CheckSyncNodesFromAOSJob()
        {
            //var allRunningJobs = AOSNotificationService.GetRunningSRNJobCount();
            
            //if (allRunningJobs >= MaxParallelSRNJobs)
            //{
            //    logger.Info($"Reach MaxParallelSRNJobs: {MaxParallelSRNJobs}, RunningSRNJobs: {allRunningJobs}");
            //    return false;
            //}
            //else
            //{
            //    logger.Info($"MaxParallelSRNJobs: {MaxParallelSRNJobs}, RunningSRNJobs: {allRunningJobs}");
            //}
            var jobCount = JobMonitorService.GetRunningJobsCount(JobType.SyncNodesFromAOS);
            return jobCount == 0;
        }

        private async System.Threading.Tasks.Task RunJobAsync(List<JobQueueDto> jqMessage)
        {
            string jobId = string.Empty;
            JobQueueDto jqDto = null;
            bool checkRunningJobExceedLimit = false;
            try
            {

                jqDto = jqMessage[0];
                logger.Info("begin to run job:{0}", jqDto.JobType);
                string[] paras;
                if(AgentMgmtService.TryCreateSkippedJobIfAgentUpgrading(jqDto.JobType, jqDto.JobRunByUser, out string _jobId))
                {
                    jobId = _jobId;
                    return;
                }
                if (AgentMgmtService.TryCreateSkippedJobIfRunningJobExceedLimit(jqDto.JobType, jqDto.JobRunByUser, out string joId))
                {
                    if(!string.IsNullOrEmpty(joId))
                    {
                        jobId = joId;
                        logger.Info($"No agent availaile, job failed {jobId}");
                        return;
                    }
                    ResetQueueJob(jqDto);
                    checkRunningJobExceedLimit = true;
                    return;
                }
                switch (jqDto.JobType)
                {
                    case Contract.JobMonitor.JobType.JobMonitorArchive:
                        jobId = await RMArchiverSettingsService.RealRunJobMonitorArchiveJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.TermSynchronization:
                        var parameters = JsonConvert.DeserializeObject<Dictionary<string, bool>>(jqDto.Parameters);
                        jobId = RMSharePointTaxonomyService.RealRunSyncJob(jqDto.JobRunType, jqDto.JobRunByUser, parameters["fromTimerJobPage"], parameters["fromGoogleOne"]);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremTermSynchronization:
                        jobId = await RMSharePointTaxonomyService.RealRunSyncJobForSPOnpremAsync(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(jqDto.Parameters));
                        break;
                    case Contract.JobMonitor.JobType.SharePointScheduleSetting:
                        jobId = await RMSharePointSettingsService.RealSharepointSettingsScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunType == JobRunBy.Control ? jqDto.JobRunByUser : "", true, jqDto.JobPriority);
                        break;
                    case Contract.JobMonitor.JobType.PhysicalTermSynchronization:
                        break;
                    case Contract.JobMonitor.JobType.PhysicalFolderSynchronization:
                        jobId = LocationSynchronizationService.RealRunSyncLocationTreeToSharePoint(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(jqDto.Parameters));
                        break;
                    case Contract.JobMonitor.JobType.TermDeletion:
                        break;
                    case Contract.JobMonitor.JobType.UpdateLocation:
                        jobId = UpdateRecordLocationService.RealRunUpdateRecordLocation(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(jqDto.Parameters));
                        break;
                    case Contract.JobMonitor.JobType.ImportPhysicalRecords:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await LocationManagementService.RealRunImportPhysicalFilesAndRecordsAsync(jqDto.JobRunType, jqDto.JobRunByUser, paras[0], int.Parse(paras[1]), int.Parse(paras[2]));
                        break;
                    // 新增加导入Zip
                    case Contract.JobMonitor.JobType.PhysicalBulkInsertExport:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await LocationManagementService.RealRunImportPhysicalZipFilesAndRecordsAsync(jqDto.JobRunType, jqDto.JobRunByUser, paras[0], int.Parse(paras[1]));
                        break;
                    case Contract.JobMonitor.JobType.PhysicalBulkEditExport:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await LocationManagementService.RealRunExportPhysicalZipFilesAndRecordsAsync(jqDto.JobRunType, jqDto.JobRunByUser, paras[0]);
                        break;
                    case Contract.JobMonitor.JobType.PhysicalTemplateImport:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = TemplateManagementService.RealRunPhysicalTemplateImportJob(jqDto.JobRunType, jqDto.JobRunByUser, paras[0]);
                        break;
                    case Contract.JobMonitor.JobType.TrimRecordsDeletion:
                        jobId = await ImportTRIMService.RealRunImportRecordsDeletionAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.ImportRecordsRelated:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await ImportTRIMService.RealRunImportRecordsRelatedAsync(jqDto.JobRunType, jqDto.JobRunByUser, paras[0], int.Parse(paras[1]));
                        break;
                    case Contract.JobMonitor.JobType.RetiredTermReport:
                    case Contract.JobMonitor.JobType.ItemsFilesDueDisposal:
                    case Contract.JobMonitor.JobType.OrphanedTermReport:
                    case Contract.JobMonitor.JobType.CreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.BCSTermUsageReport:
                    case Contract.JobMonitor.JobType.AvailableSpaceReport:
                    case Contract.JobMonitor.JobType.RestoreReport:
                    case Contract.JobMonitor.JobType.EXOTermUsageReport:
                    case Contract.JobMonitor.JobType.EXORetiredTermUsageReport:
                    case Contract.JobMonitor.JobType.EXOOrphanedTermUsageReport:
                    case Contract.JobMonitor.JobType.EXOItemsFilesDueDisposalReport:
                    case Contract.JobMonitor.JobType.EXOCreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.PhysicalCreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.PhysicalTermUsageReport:
                    case Contract.JobMonitor.JobType.PhysicalOrphanedTermUsageReport:
                    case Contract.JobMonitor.JobType.PhysicalRetiredTermUsageReport:
                    case Contract.JobMonitor.JobType.PhysicalItemsFilesDueDisposalReport:
                    case Contract.JobMonitor.JobType.FSItemsFilesDueDisposal:
                    case Contract.JobMonitor.JobType.FSCreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.FSBCSTermUsageReport:
                    case Contract.JobMonitor.JobType.FSOrphanedTermReport:
                    case Contract.JobMonitor.JobType.FSRetiredTermReport:
                    case Contract.JobMonitor.JobType.OneDriveTermUsageReport:
                    case Contract.JobMonitor.JobType.OneDriveItemsFilesDueDisposalReport:
                    case Contract.JobMonitor.JobType.OneDriveCreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.OneDriverRestoreReport:
                    case Contract.JobMonitor.JobType.SPOnPremItemsFilesDueDisposal:
                    case Contract.JobMonitor.JobType.SPOnPremCreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.SPOnPremBCSTermUsageReport:
                    case Contract.JobMonitor.JobType.SPOnPremRetiredTermReport:
                    case Contract.JobMonitor.JobType.SPOnPremOrphanedTermReport:
                    case Contract.JobMonitor.JobType.SPOActionAuditReport:
                    case Contract.JobMonitor.JobType.OneDriveActionAuditReport:
                    case Contract.JobMonitor.JobType.BoxItemsFilesDueDisposalReport:
                    case Contract.JobMonitor.JobType.BoxCreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.BoxBCSTermUsageReport:
                    case Contract.JobMonitor.JobType.BoxOrphanedTermUsageReport:
                    case Contract.JobMonitor.JobType.BoxRetiredTermUsageReport:
                    case Contract.JobMonitor.JobType.GoogleCreateAndDestroyedFileReport:
                    case Contract.JobMonitor.JobType.GoogleItemsFilesDueDisposalReport:
                    case Contract.JobMonitor.JobType.GoogleBCSTermUsageReport:
                    case Contract.JobMonitor.JobType.GoogleOrphanedTermUsageReport:
                    case Contract.JobMonitor.JobType.GoogleRetiredTermUsageReport:
                    case Contract.JobMonitor.JobType.GoogleRestoreReport:
                    case Contract.JobMonitor.JobType.TeamsRestoreReport:
                    case Contract.JobMonitor.JobType.TeamsBCSTermUsageReport:
                    case Contract.JobMonitor.JobType.TeamsOrphanedTermUsageReport:
                    case Contract.JobMonitor.JobType.TeamsRetiredTermUsageReport:
                    case Contract.JobMonitor.JobType.TeamsItemsFilesDueDisposalReport:
                    case Contract.JobMonitor.JobType.TeamsCreateAndDestroyedFileReport:
                     case Contract.JobMonitor.JobType.ArchivedSiteReport:
                     case Contract.JobMonitor.JobType.OneDriveArchivedSiteReport:
                     case Contract.JobMonitor.JobType.TeamsArchivedSiteReport:
                     case Contract.JobMonitor.JobType.GoogleArchivedSiteReport:
                    case Contract.JobMonitor.JobType.TeamsActionAuditReport:
                        paras = jqDto.Parameters.Split(' ');
                        var profileId = int.Parse(paras[0]);
                        var IsOrphanedTermReport = bool.Parse(paras[1]);
                        var isRetiredTermReport = paras.Length > 4 ? bool.Parse(paras[4]) : false;
                        jobId = await RMReportService.RealRunReportJobAsync(jqDto.JobType, jqDto.JobRunByUser, profileId, IsOrphanedTermReport, isRetiredTermReport);
                        break;
                    case Contract.JobMonitor.JobType.ManualApproval:
                        jobId = ManualApprovalService.RealRunManualApprovalJob(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.ManualApprovalLocationTest:
                        break;
                    case Contract.JobMonitor.JobType.ApplySharePointSettings:
                        paras = jqDto.Parameters.Split(',');
                        string scopeId = string.Empty;
                        string siteId = string.Empty;
                        string folderPath = string.Empty;
                        if (paras.Length > 3)
                        {
                            scopeId = paras[2];
                            siteId = paras[3];
                            folderPath = paras[4];
                        }
                        jobId = await RMSharePointSettingsService.RealRunApplySettingJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(paras[0]), (RunApplySettingMethod)int.Parse(paras[1]), scopeId, siteId, folderPath, jqDto.JobPriority);
                        break;
                    case Contract.JobMonitor.JobType.ImportTermStructure:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = TaxonomyService.RealRunImportTermStructureJob(jqDto.JobRunType, jqDto.JobRunByUser, paras[0], paras[1], isControlPlus: bool.Parse(paras[2]));
                        break;
                    case JobType.ExportTermStructure:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await TaxonomyService.RealRunExportTermStructureJob(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.ImportSCMapping:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = RestoreSearchService.RealRunImportSCMappingJob(jqDto.JobRunByUser, paras[0]);
                        break;
                    case Contract.JobMonitor.JobType.ExportSCMapping:
                        jobId = RestoreSearchService.RealRunExportSCMappingJob(jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.ImportSCWhitelist:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = RestoreSearchService.RealRunImportSCWhitelistJob(jqDto.JobRunByUser, paras[0]);
                        break;
                    case Contract.JobMonitor.JobType.ImportSCBlacklist:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = RestoreSearchService.RealRunImportSCBlacklistJob(jqDto.JobRunByUser, paras[0]);
                        break;
                    case Contract.JobMonitor.JobType.ExportSCBlacklist:
                        jobId = RestoreSearchService.RealRunExportSCBlacklistJob(jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.ExportSCWhitelist:
                        jobId = RestoreSearchService.RealRunExportSCWhitelistJob(jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.DiscoveryImportExcludeSCList:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = SpecificSiteService.RealRunImportSCExcludeList(jqDto.JobRunByUser, paras[0]);
                        break;
                    case Contract.JobMonitor.JobType.DiscoveryExportExcludeSCList:
                        jobId = SpecificSiteService.RealRunExportSCExcludeList(jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.ImportSPSetting:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await RMSharePointSettingsService.RealRunImportSPSettingJob(jqDto.JobRunType, jqDto.JobRunByUser, paras[0], paras[1]);
                        break;
                    case Contract.JobMonitor.JobType.ExportSPSetting:
                        jobId = await RMSharePointSettingsService.RealRunExportSPSettingJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.ExportSPSOSetting:
                        jobId = await RMSharePointSettingsService.RealRunExportSPSOSettingJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.UniqueIDSettingIncrementalSchedule:
                        jobId = await UniqueIdSettingService.RealUnIDSettingScheduleJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.UniqueIDSettingIncrementalSchedule);
                        break;
                    case Contract.JobMonitor.JobType.ExportToLocation:
                        jobId = await JobMonitorService.RunExportDisposalJobAsync(jqDto.Parameters, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.All:
                        break;
                    case Contract.JobMonitor.JobType.UniqueIDSettingFullSchedule:
                        jobId = await UniqueIdSettingService.RealUnIDSettingScheduleJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.UniqueIDSettingFullSchedule, jqDto.JobRunType == JobRunBy.Control ? jqDto.JobRunByUser : "");
                        break;
                    case Contract.JobMonitor.JobType.ManualApprovalTimer:
                        jobId = ManualApprovalService.RealRunManualApprovalTimerJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.ManualApprovalOrRejectJob:
                        jobId = await RMManualApprovalService.RealRunBulkActionJobAsync(jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.EnforceRetention:
                        jobId = await EnforceRetentionService.RealRunJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.EnforceRetention);
                        break;
                    case Contract.JobMonitor.JobType.OldEnforceRetention:
                        jobId = await EnforceRetentionService.RealRunJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.OldEnforceRetention);
                        break;
                    case Contract.JobMonitor.JobType.DataSynchronisation:
                        jobId = await RMSharePointSettingsService.RealRunDataSyncJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.DiscoveryDalJob:
                        jobId = await DiscoveryPlanProfileService.RealRunTriggerDalJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.RecordsExplorerMove:
                        jobId = ExplorerService.RunMoveToJob(jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.EXOApplySetting:
                        jobId = await RMSharePointSettingsService.RealRunApplyEXOSettingJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(jqDto.Parameters), jqDto.JobPriority);
                        break;
                    case Contract.JobMonitor.JobType.EXODataSynchronisation:
                        jobId = await RMSharePointSettingsService.RealRunEXODataSyncJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.SPDataSynchronisationSchedule:
                        jobId = await RMSharePointSettingsService.RealRunSPDataSyncScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.EXODataSynchronisationSchedule:
                        jobId = await RMSharePointSettingsService.RealRunEXODataSyncScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.EXOApplySettingSchedule:
                        jobId = await RMSharePointSettingsService.RealRunApplyEXOSettingsScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.JobPriority);
                        break;
                    case Contract.JobMonitor.JobType.EXOEnforceRetention:
                        jobId = EnforceRetentionService.RealRunEXOJob(jqDto.JobRunType, Contract.JobMonitor.JobType.EXOEnforceRetention);
                        break;
                    //case Contract.JobMonitor.JobType.PhysicalDisposal:
                    //    RAReturnMessage msg = RMPhysicalRecordSettingsService.RealRunPhysicallDisposalScheduleJob(jqDto.Parameters, jqDto.JobRunType);
                    //    jobId = msg.Extension;
                    //    break;
                    case Contract.JobMonitor.JobType.PhysicalExplorerTimer:
                        jobId = ExplorerService.RealRunPhysicalTimerJob(jqDto.Parameters, jqDto.JobRunType);
                        break;
                    case Contract.JobMonitor.JobType.ConnectorTimer:
                        jobId = ExplorerService.RealRunConnectorTimerJob(jqDto.Parameters, jqDto.JobRunType);
                        break;
                    case Contract.JobMonitor.JobType.PhysicalExportBarcode:
                        paras = jqDto.Parameters.Split(' ');
                        string exportLocationId = paras[0];
                        string nodeId = paras[1];
                        string nodeType = paras[2];
                        string exportLocationName = paras[3];
                        string suiteId = paras[4];
                        jobId = ExplorerService.RealExportBarcode(jqDto.JobRunType, exportLocationId, nodeId, nodeType, exportLocationName, suiteId);
                        break;
                    case Contract.JobMonitor.JobType.CollectionDataFull:
                        jobId = ReportCollectionService.RealRunJob(jqDto.JobRunType, jqDto.JobType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.ActionOnly:
                        RMSPTreeNode runJobNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(jqDto.Parameters);
                        jobId = await RMJobService.RealRunDeclareOnlyJobAsync(jqDto.JobRunType, jqDto.JobRunType == JobRunBy.Control ? jqDto.JobRunByUser : "", runJobNode);
                        break;
                    case Contract.JobMonitor.JobType.PhysicalSetPermission:
                        jobId = PermissionManagementService.RealRunSetPermissionJob(jqDto.JobRunType, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.FSDataSynchronization:
                        jobId = await RMFileSystemSettingsService.RealRunDataSyncJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.ImportFSSetting:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await RMFileSystemSettingsService.RealRunImportFSSettingJob(jqDto.JobRunType, jqDto.JobRunByUser, paras[0], paras[1]);
                        break;
                    case Contract.JobMonitor.JobType.ExportFSSetting:
                        jobId = await RMFileSystemSettingsService.RealRunExportFSSettingJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.DownloadRCCReport:
                        jobId = await RMFileSystemSettingsService.RealRunDownloadRCCReportJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.SharePointSiteMetricsReport:
                        jobId = await SharePointReportExportService.RealRunSPReportExportJobAsync(jqDto);
                        break;
                    case Contract.JobMonitor.JobType.FSDataSynchronizationSchedule:
                        jobId = await RMFileSystemSettingsService.RealRunFSDataSyncScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.FSDisposal:
                        jobId = await RMFileSystemSettingsService.RealRunDisposalJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.FSDisposalByClassCode:
                        jobId = await RMFileSystemSettingsService.RealRunDisposalByClassCodeJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.ApplyClassCode:
                        jobId = await RMFileSystemSettingsService.RealRunApplyClassCodeJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.FSArchiverRestore:
                        jobId = await RMFileSystemSettingsService.RealRunFSRestoreJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.FSDashBoard:
                        jobId = ExplorerService.RealStartFSDashBoard(jqDto.JobRunType);
                        break;
                    case Contract.JobMonitor.JobType.FSFolderChangeTerm:
                        jobId = ExplorerService.RealRunFSFolderReclassifyJob(jqDto.JobRunType, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.FSFolderManageHold:
                        jobId = ExplorerService.RealRunFSFolderHoldJob(jqDto.JobRunType, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.SyncSecurityContainer:
                        jobId = RMSecurityContainerService.RealScheduleJob(jqDto.JobRunType, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.GlobalSearchAction:
                        jobId = await ExplorerService.RealRunGlobalSearchActionJobAsync(jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.ExplorerOfflineSearch:
                        paras = jqDto.Parameters.Split(' ');
                        Contract.PersonalSetting.IPersonalSettingService personalSettingService = PlatformWindsorManager.GetService<Contract.PersonalSetting.IPersonalSettingService>();
                        jobId = await personalSettingService.RealRunSearchOfflineAsync(jqDto.JobRunType, jqDto.JobRunByUser, int.Parse(paras[0]), paras[1]);
                        break;
                    case Contract.JobMonitor.JobType.SyncNodesFromAOS:
                        jobId = RemoteNodeService.RealRunSyncNodesJob(jqDto);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremScanLocalNodes:
                        //jobId = LocalNodeService.RealRunScanNodesJob(jqDto);
                        jobId = await SharePointOnPremScanNodeService.RunRealTimeJobAsync(jqDto.JobRunType);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremApplySetting:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await RMSharePointOnPremSettingsService.RealRunApplySettingJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(paras[0]), (RunApplySettingMethod)int.Parse(paras[1]));
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremApplySettingSchedule:
                        jobId = await RMSharePointOnPremSettingsService.RealSharepointSettingsScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunType == JobRunBy.Control ? jqDto.JobRunByUser : "", true);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremEnforceRuleAction:
                    case Contract.JobMonitor.JobType.SPOnPremEnforceRuleActionSchedule:
                        jobId = await RMSharePointOnPremSettingsService.RealRunOnpremiseEnforceRuleActionJobAsync(jqDto.JobRunByUser, jqDto.JobRunType, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremDataSync:
                        jobId = await RMSharePointOnPremSettingsService.RealRunDataSyncJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremDataSyncSchedule:
                        jobId = await RMSharePointOnPremSettingsService.RealRunSPDataSyncScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingIncrementalSchedule:
                        jobId = await UniqueIdSettingService.RealSPOnPremUnIDSettingScheduleJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingIncrementalSchedule);
                        break;
                    case Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingFullSchedule:
                        jobId = await UniqueIdSettingService.RealSPOnPremUnIDSettingScheduleJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingFullSchedule, jqDto.JobRunType == JobRunBy.Control ? jqDto.JobRunByUser : "");
                        break;
                    case Contract.JobMonitor.JobType.OneDriveDataSynchronisation:
                        jobId = await RMOneDriveSettingsService.RealRunDataSyncJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case Contract.JobMonitor.JobType.OneDriveDataSynchronisationSchedule:
                        jobId = await RMOneDriveSettingsService.RealRunOneDriveDataSyncScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case Contract.JobMonitor.JobType.OneDriveEnforceRetention:
                        jobId = await EnforceRetentionService.RealRunOneDriveJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.OneDriveEnforceRetention);
                        break;
                    case JobType.Dashboard:
                        jobId = DashboardService.RealRunDashboardJob(jqDto.JobRunType);
                        break;
                    case JobType.FSMyHubDashboard:
                        jobId = ExplorerService.RealStartFSMyHubDashBoard(jqDto.JobRunType, jqDto.Parameters);
                        break;
                    case JobType.ArchiverFullTextIndex:
                        jobId = ArchivedFullTextIndexService.RealRunJob(jqDto.Parameters);
                        break;
                    case JobType.TenantUpgrade:
                        jobId = TenantUpgradeService.RealRunUpgradeJob();
                        break;
                    case JobType.DisposalReport:
                        jobId = DisposalReportService.RealRunReportJob(Convert.ToInt32(jqDto.Parameters));
                        break;
                    case JobType.CreateAndDestroyedReport:
                        jobId = CreateAndDestryoedReportService.RealRunReportJob(Convert.ToInt32(jqDto.Parameters));
                        break;
                    case JobType.TermUsageReport:
                        jobId = TermUsageReportService.RealRunReportJob(Convert.ToInt32(jqDto.Parameters));
                        break;
                    case Contract.JobMonitor.JobType.PhysicalLoanBox:
                    case Contract.JobMonitor.JobType.PhysicalReturnBox:
                        jobId = await PhysicalRequestService.RealRunStartLoanOrReturnBoxJobAsync(jqDto.JobType, jqDto.Parameters);
                        break;
                    case JobType.ExportSearchResult:
                        jobId = await ExplorerService.RealRunExportSearchResultJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ExportHoldRecords:
                        jobId = await ExplorerService.RealRunExportHoldRecordsJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ImportHoldRecords:
                        jobId = await ExplorerService.RealRunImportHoldRecordsJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ImportWorkspaceHold:
                        jobId = await WorkspaceHoldService.RealRunImportWorkspaceHoldJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ManualApprovalEmailSchedule:
                        jobId = await RMManualApprovalService.RealRunEmailScheduleJobAsync(jqDto.JobRunType);
                        break;
                    case JobType.AzureFileShareDataSynchronisation:
                        jobId = await AzureFileSettingService.RealRunDataSyncJobAsync(jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.AzureFileShareDataSynchronisationSchedule:
                        jobId = await AzureFileSettingService.RealRunDataSyncScheduleJobAsync();
                        break;
                    case JobType.BoxDataSynchronisation:
                        jobId = await BoxSettingsService.RealRunDataSyncJobAsync(jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.BoxDataSynchronisationSchedule:
                        jobId = await BoxSettingsService.RealRunDataSyncScheduleJobAsync(jqDto.JobRunByUser);
                        break;
                    case JobType.BoxRecordsDisposal:
                        jobId = await BoxSettingsService.RealRunBoxRecordsDisposalJobAsync(jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.OneDriveRecordsDisposal:
                        jobId = await RMOneDriveSettingsService.RealRunRecordsDisposalJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.EXORecordsDisposal:
                        jobId = await RMSharePointSettingsService.RealRunEXORecordsDisposalJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.PhysicalRecordsDisposal:
                        jobId = RMPhysicalRecordSettingsService.RealRunPhysicalRecordsDisposalJob(jqDto.JobRunByUser, jqDto.JobRunType, jqDto.Parameters);
                        break;
                    case JobType.ManualHistoriesUpgrade:
                        jobId = RMManualApprovalService.RealRunUpgradeJob();
                        break;
                    case JobType.SendEmailJob:
                        jobId = await RMEmailManagementService.RealRunSendEmailJob(jqDto.Parameters);
                        break;
                    case JobType.SharePointOnlineDeletionSyncUpgrade:
                        jobId = RMSharePointSettingsService.RealRunDeletionSyncUpgradeJob();
                        break;
                    case JobType.CosmosDBDirtyDataDeleteUpgrade:
                        jobId = RMSharePointSettingsService.RealRunDirtyDataDeleteUpgradeJob();
                        break;
                    case JobType.ManualFileSystemUpgrade:
                        jobId = RMManualApprovalService.RealRunFileSystemManualDataUpgradeJob();
                        break;
                    case JobType.PhysicalDestructionPick:
                    case JobType.PhysicalLoanPick:
                    case JobType.PhysicalDestructionPickExportJob:
                    case JobType.PhysicalLoanPickExportJob:
                    case JobType.PhysicalReturnHistoryExport:
                    case JobType.PhysicalMovePickExportJob:
                        jobId = await PickListService.RealRunStartPickCompleteJobAsync(jqDto.JobType, jqDto.Parameters, jqDto.Extension);
                        break;
                    case JobType.PhysicalMoveDataJob:
                        jobId = await PhysicalRequestService.RealRunStartMoveDataJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ManualExportHistoryDatasJob:
                        jobId = await RMManualApprovalService.RealRunExportHistoryDatasJobAsync(jqDto.Parameters);
                        break;
                    case JobType.MachineLearningTraining:
                        jobId = MLTermService.RealRunTrainingJob(jqDto.JobType);
                        break;
                    case JobType.MachineLearningAnalyse:
                        jobId = MLTermService.RealRunAnalyseJob(jqDto.JobType);
                        break;
                    case JobType.MachineLearningReviewApprove:
                        jobId = await MLManualApprovalService.RealRunApproveJobAsync(jqDto.Parameters, jqDto.JobType);
                        break;
                    case JobType.MachineLearningReviewReclassify:
                        jobId = await MLManualApprovalService.RealRunChangeTermJobAsync(jqDto.Parameters, jqDto.JobType);
                        break;
                    case JobType.MachineLearningExportReportJob:
                        jobId = TrainingReportService.RealRunExportJob(jqDto.Parameters);
                        break;
                    case JobType.ManualExportRecordsForReviewDatasJob:
                        jobId = await RMManualApprovalService.RealRunExportRecordsForReviewDatasJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ManualImportUnderReviewDatasJob:
                        jobId = await RMManualApprovalService.RealRunImportUnderReviewDatasJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ManualFolderViewActions:
                        jobId = await RMManualApprovalService.RealRunFolderViewActionJobAsync(jqDto.Parameters);
                        break;
                    case JobType.ExportReportDetails:
                        jobId = await RMReportService.RealRunExportReportJobAsync(jqDto.Parameters);
                        break;
                    case JobType.CleanUpDuplicateDatas:
                        jobId = await DiscoveryExportService.RealRunCleanDuplicateDatasJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.TeamsRecordsDisposal:
                    case JobType.TeamsPreScan:
                    case JobType.TeamsArchiverBackup:
                    case JobType.RecordsDisposal:
                    case JobType.SOPreScan:
                    case JobType.RMArchiverBackup:
                        jobId = RMArchiverSettingsService.RealRunDispatchedJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.JobType, jqDto.Parameters, jqDto?.MessageId, jqDto?.TenantGroupId);
                        break;
                    case JobType.RebuildStub:
                        jobId = RMArchiverSettingsService.RealRunRebuildStubJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.RebuildIndex:
                        jobId = RMArchiverSettingsService.RealRunRebuildIndexJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.RebuildSOJobReport:
                        jobId = RMArchiverSettingsService.RealRunRebuildSOJobReportJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.RebuildEncryptKeyValue:
                        jobId = RMArchiverSettingsService.RealRunRebuildEncryptKeyValueJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.PreviewRestore:
                        jobId = RestoreSearchService.RealRunPreviewRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters, jqDto.MessageId);
                        break;
                    case JobType.BuildRunningJobReport:
                        jobId = RMArchiverSettingsService.RealRunBuildRunningJobReportJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ExportDecryptIndexDB:
                        jobId = RMArchiverSettingsService.RealRunExportDecryptIndexDBJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;

                    case JobType.BaseArchiveJobIdMultiRestore:
                        jobId = RMArchiverSettingsService.RealRunBaseArchiveJobIdMultiRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.MultiSiteCollectionRestore:
                        jobId = RestoreSearchService.RealRunMultiSiteCollectionRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.RebuildDeDupForWPPMigration:
                        jobId = RMArchiverSettingsService.RealRunRebuildDeDupForWPPMigrationJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ArchiverRestore:
                    case JobType.ArchiverOutPlaceRestore:
                    case JobType.StubOopRestore:
                    case JobType.AOSPRestore:
                    case JobType.ArchiverToSpoRestore:
                    case JobType.StubArchiverRestore:
                    case JobType.M365InPlaceArchiverRestore:
                        if (RestoreSearchService.ShouldQueryInJobForEndUserRestore(jqDto.Parameters))
                        {
                            jobId = RestoreSearchService.RealRunEndUserArchiverRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters, jqDto.JobType);
                        }
                        else
                        {
                            jobId = RestoreSearchService.RealRunArchiverRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters, jqDto.JobType);
                        }
                        break;
                    case JobType.GoogleArchiverRestore:
                        jobId = RestoreSearchService.RealRunDriveArchiverRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters, jqDto.JobType);
                        break;
                    case JobType.SimulateRestore:
                        jobId = RestoreSearchService.RealRunSimulateArchiverRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters,jqDto.TenantGroupId);
                        break;
                    case JobType.ArchiverMoveIndex:
                        jobId = RMArchiverSettingsService.RealRunArchiverMoveIndexJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ArchiverRetention:
                        jobId = await RMArchiverSettingsService.RealRunArchiverRetentionJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.FSRetain:
                        jobId = await RMArchiverSettingsService.RealRunFSRetentionJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.ArchiverFullMoveRetention:
                        jobId = await RMArchiverSettingsService.RealRunArchiverFullMoveRetentionJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ArchiverDeduplication:
                        jobId = await RMArchiverSettingsService.RealRunArchiverDedupJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.ArchiverDeduplicationReport:
                        jobId = await DashboardService.RealRunExportArchiverDedupSiteInfoJobAsync(jqDto.Parameters);
                        break;
                    case JobType.DeleteRestoredData:
                        jobId = RMArchiverSettingsService.RealRunArchiverDeleteRestoredDataJob(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.VeoMerge:
                        jobId = await RMArchiverSettingsService.RealRunVeoMergeJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ArchiverExport:
                        jobId = await DashboardService.RealRunExportArchiverSiteInfoJobAsync(jqDto.Parameters);
                        break;
                    case JobType.CloudArchiverMigration:
                        var migrationMessage = RA.Common.Global.Utils.SerializerHelper.DeserializeByJsonConvert<ArchiverMigratedJobMessage>(jqDto.Parameters);
                        var jobSettings = RA.Common.Global.Utils.SerializerHelper.SerializeByJsonConvert(migrationMessage.ArchiverMigrationJobSettings);
                        jobId = ArchiverRuleService.RealRunCloudArchiverMigrationJob(jobSettings, migrationMessage.JobId);
                        break;
                    case JobType.DiscoverOptimization:
                        jobId = await RMArchiverSettingsService.RealRunOptimizationJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.DiscoveryPlanProOptimization:
                        jobId = await RMArchiverSettingsService.RealRunDiscoveryPlanProOptimizationJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ArchiverByHSMXml:
                        jobId = await RMArchiverSettingsService.RealRunOptimizationJobFromManifestAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.DiscoveryAOSPOptimization:
                        jobId = await RMArchiverSettingsService.RealRunAOSPOptimizationJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.DiscoveryOptimizationCalculate:
                        jobId = OptimizationService.RealRunOptimizationCalculateJob(jqDto.Parameters);
                        break;
                    case JobType.DiscoveryAOSPOptimizationCalculate:
                        jobId = AOSPOptimizationService.RealRunOptimizationCalculateJob(jqDto.Parameters);
                        break;
                    case JobType.DiscoveryPreScan:
                        jobId = await RMArchiverSettingsService.RealRunOptimizationPreScanJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.DiscoveryPlanProScan:
                        jobId = await RMArchiverSettingsService.RealRunDiscoveryPlanProScanJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.DiscoveryReCalculate:
                        jobId = ConfigurationService.RealRunCalculateJob(jqDto.Parameters);
                        break;
                    case JobType.DiscoveryJobV2:
                    case JobType.DiscoveryJobV3:
                    case JobType.DiscoveryJobV4:
                    case JobType.DiscoveryJobV5:
                        jobId = ConfigurationService.RealRunNextVersionDiscoveryAnalysisJob(jqDto.Parameters);
                        break;
                    case JobType.DiscoveryGoogleJobV1:
                        jobId = GoogleConfigurationService.RealRunDiscoveryAnalysisJob(jqDto.Parameters);
                        break;
                    case JobType.DiscoveryAOSPJob:
                        jobId = AOSPConfigurationService.RealRunDiscoveryAnalysisJob(jqDto.Parameters);
                        break;
                    case JobType.DiscoveryProfileJob:
                        jobId = ProfileService.RealRunProfileJob(jqDto);
                        break;
                    case JobType.DiscoveryGoogleProfileJob:
                        jobId = DiscoveryProfileService.RealRunProfileJob(jqDto);
                        break;
                    case JobType.DiscoveryExportRowDataJob:
                        jobId = await DiscoveryExportService.RealExportRowDataJobAsync(jqDto);
                        break;
                    case JobType.DiscoveryExportDuplicationReport:
                        jobId = await DiscoveryExportService.RealRunExportDuplicationReportAsync(jqDto);
                        break;
                    case JobType.DiscoveryAnalysisFileSystemV1:
                        jobId = FileSystemConfigurationService.RealRunDiscoveryAnalysisJob(jqDto.Parameters);
                        break;
                    case JobType.DownloadJobReports:
                        jobId = await JobMonitorService.RealRunDownloadJobReportJob(jqDto.Parameters);
                        break;
                    case JobType.ExportSiteMetrics:
                        jobId = await CreateAndDestryoedReportService.RealRunGenerateSiteMetricsReportJobAsync(jqDto.JobType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ApprovalProcessArchive:
                        await RMArchiverSettingsService.RealRunApprovalProcessJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        await RMSharePointSettingsService.RealRunEXORecordsDisposalJobForApprovalAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        RMPhysicalRecordSettingsService.RealRunPhysicalRecordsForApprovalDisposalJob(jqDto.JobRunByUser, jqDto.JobRunType);
                        await BoxSettingsService.RealRunBoxRecordsDisposalJobForApprovalAsync(jqDto.JobRunByUser);
                        //await RMFileSystemSettingsService.RealRunDisposalJobForApprovalAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        //await RMSharePointOnPremSettingsService.RealRunOnpremiseEnforceRuleActionJobForApprovalAsync(jqDto.JobRunByUser, jqDto.JobRunType);
                        //RMManualApprovalService.RealRunDeleteInvalidRecordsJob();
                        break;
                    case JobType.DeleteInvalidRecords:
                        RMManualApprovalService.RealRunDeleteInvalidRecordsJob();
                        break;
                    case JobType.ExportIndex:
                        jobId = await RMArchiverSettingsService.RealRunExportIndexJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.GoogleApplySettings:
                        paras = jqDto.Parameters.Split(',');
                        string gScopeId = Guid.Empty.ToString();
                        string gDriveId = Guid.Empty.ToString();
                        string gFolderPath = string.Empty;
                        if (paras.Length > 3)
                        {
                            gScopeId = paras[2];
                            gDriveId = paras[3];
                            gFolderPath = paras[4];
                        }
                        jobId = await GoogleJobService.RealRunApplySettingJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(paras[0]), (RunApplySettingMethod)int.Parse(paras[1]), gScopeId, gDriveId, gFolderPath);
                        break;
                    case JobType.ImportGoogleTermStructure:
                        jobId = GoogleJobService.RealRunImportGoogleTermJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.GoogleDataSynchronization:
                        jobId = await GoogleJobService.RealRunDataSyncJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.GoogleRecordsDisposal:
                        jobId = await GoogleJobService.RealRunRecordsDisposalJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.GoogleArchiverRetention:
                        jobId = await RMArchiverSettingsService.RealRunGDriveArchiverRetentionJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.ExportAdvanceSeachResult:
                        jobId = RestoreSearchService.RealRunExportSearchResultJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters, jqDto.JobType);
                        break;
                    case JobType.ExportRestoreCenterSeachResult:
                        jobId = RestoreSearchService.RealRunRestoreCenterExportSearchResultJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters, jqDto.JobType);
                        break;
                    case JobType.DeleteOrphanDatas:
                        jobId = await RMArchiverSettingsService.RealRunDeleteOrphanDatasJobAsync(jqDto.JobRunType, jqDto.JobRunByUser,jqDto.Parameters);
                        break;
                    case JobType.SpecifySitesArchiverBackup:
                        jobId = RMArchiverSettingsService.RealRunSpecifySitesArchiverBackupJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.RMEndUserArchiverBackup:
                        jobId = RMArchiverSettingsService.RealRunEndUserArchiverBackupJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.SpecifyTeamsArchiverBackup:
                        jobId = RMArchiverSettingsService.RealRunSpecifyTeamsArchiverBackupJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ConvertStub:
                        jobId = await StubSettingService.RealRunConvertStubJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.DeclaredRecordsMigration:
                        jobId = await DeclaredRecordsMigrationService.RealRunDeclaredRecordsMigrationJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.DiscoveryExportO365Profile:
                        jobId = await ProfileService.RealRunExportProfileDiscoveryDataAnalysisForOffice365Job(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.ApplyTeamsSettings:
                        paras = jqDto.Parameters.Split(',');
                        string teamsScopeId = string.Empty;
                        string teamsId = string.Empty;
                        string teamsSiteId = string.Empty;
                        string teamsFolderPath = string.Empty;
                        if (paras.Length > 3)
                        {
                            teamsScopeId = paras[2];
                            teamsId = paras[3];
                            teamsSiteId = paras[4];
                            teamsFolderPath = paras[5];
                        }
                        jobId = await TeamsSettingsService.RealRunApplySettingJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, bool.Parse(paras[0]), (RunApplySettingMethod)int.Parse(paras[1]), teamsScopeId, teamsId, teamsSiteId, teamsFolderPath, jqDto.JobPriority);
                        break;
                    case JobType.TeamsUniqueIDSettingIncrementalSchedule:
                        jobId = await UniqueIdSettingService.RealRunTeamsIDSettingScheduleJobAsync(jqDto.JobRunType, JobType.TeamsUniqueIDSettingIncrementalSchedule);
                        break;
                    case JobType.TeamsUniqueIDSettingFullSchedule:
                        jobId = await UniqueIdSettingService.RealRunTeamsIDSettingScheduleJobAsync(jqDto.JobRunType, JobType.TeamsUniqueIDSettingFullSchedule, jqDto.JobRunType == JobRunBy.Control ? jqDto.JobRunByUser : "");
                        break;
                    case JobType.TeamsDataSynchronisation:
                        jobId = await TeamsSettingsService.RealRunDataSyncJobAsync(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.TeamsDataSynchronisationSchedule:
                        jobId = await TeamsSettingsService.RealRunDataSyncScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.TeamsScheduleSetting:
                        jobId = await RMTeamsSettingsService.RealTeamsSettingsScheduleJobAsync(jqDto.JobRunType, jqDto.JobRunType == JobRunBy.Control ? jqDto.JobRunByUser : "", true, jqDto.JobPriority);
                        break;
                    case JobType.TeamsArchiverRestore:
                    case JobType.MailBoxArchiverRestore:
                    case JobType.TeamsOutPlaceRestore:
                        jobId = RestoreSearchService.RealRunTeamsArchiverRestoreJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters, jqDto.JobType, jqDto.JobPriority);
                        break;
                    case Contract.JobMonitor.JobType.TeamsEnforceRetention:
                        jobId = await EnforceRetentionService.RealTeamsRunJobAsync(jqDto.JobRunType, Contract.JobMonitor.JobType.TeamsEnforceRetention);
                        break;
                    case JobType.ExportTeamsSetting:
                        jobId = await RMTeamsSettingsService.RealRunExportTeamsSettingJobAsync(jqDto.JobRunType, jqDto.Parameters, jqDto.JobRunByUser);
                        break;
                    case JobType.ExportTeamsSOSetting:
                        jobId = await RMTeamsSettingsService.RealRunExportTeamsSOSettingJobAsync(jqDto.JobRunType, jqDto.Parameters, jqDto.JobRunByUser);
                        break;
                    case JobType.ImportTeamsSetting:
                        paras = jqDto.Parameters.Split(' ');
                        jobId = await RMTeamsSettingsService.RealRunImportTeamsSettingJob(jqDto.JobRunType, jqDto.JobRunByUser, paras[0], paras[1]);
                        break;
                    case JobType.TeamsArchiverRetention:
                        jobId = await RMArchiverSettingsService.RealRunTeamsArchiverRetentionJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.EXOArchiverRetention:
                        jobId = await RMArchiverSettingsService.RealRunEXOArchiverRetentionJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.TeamsChannelSettingConflictCheck:
                        jobId = RMTeamsSettingsService.RealRunTeamsChannelSettingConflictCheckJob(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.ConflictSettingDetailExport:
                        jobId = RMTeamsSettingsService.RealRunConflictSettingDetailExportJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.TeamsNodeSettingUpgrade:
                        jobId = RMTeamsSettingsService.RealRunTeamsNodeSettingUpgradeJob(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.TeamsDataUpgrade:
                        jobId = RMTeamsSettingsService.RealRunTeamsDataUpgradeJob(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.StubDisposal:
                        jobId = await StubSettingService.RealRunStubDisposalJobAsync(jqDto.JobRunType, jqDto.JobRunByUser);
                        break;
                    case JobType.DeleteArchivedSiteCollection:
                        jobId = RestoreSearchService.RealRunDeleteArchivedSiteCollectionJob(jqDto.JobRunType, jqDto.JobRunByUser, jqDto.Parameters);
                        break;
                    case JobType.MultiGeoMainDCSyncCommonData:
                        jobId = await MultiGeoDataCenterService.RealRunMainDCSyncCommonDataJob(jqDto.JobRunType);
                        break;
                    case JobType.MultiGeoOtherDCSyncCommonData:
                        jobId = await MultiGeoDataCenterService.RealRunOtherDCSyncCommonDataJob(jqDto.Parameters);
                        break;
                    default:
                        break;
                }
                logger.Info("run job success, jobId:{0},TenantId:{1}, JobType:{2},JobRunType:{3}", jobId, jqDto.TenantGroupId, jqDto.JobType, jqDto.JobRunType);
                var jobPriority = jqDto.JobPriority;
                if (jobPriority != JobPriority.Normal)
                {
                    await JobMonitorDao.UpdateJobPriorityAsync([jobId], jobPriority);
                }
                if (AvePoint.RA.Common.JobService.JobServiceUtility.SkipMergeDetailsJobs.Contains((int)jqDto.JobType) && !string.IsNullOrEmpty(jobId))
                {
                    try
                    {
                        await JobMonitorDao.UpdateJobVersion(jobId, JobVersion.UnMerged);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error occurred when update job version, jobId:{0}, error:{1}", jobId, ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run job task, messageId:{0},TenantId:{1}, JobType:{2},JobRunType:{3},JobId:{4},ERROR:{5}", jqDto?.MessageId, jqDto?.TenantGroupId, jqDto?.JobType, jqDto?.JobRunType, jobId, ex.ToString());
                if (!string.IsNullOrEmpty(jobId) && jobId != RecordsConstants.UniqueId_NoNeedRunJob)
                {
                    JobMonitorService.UpdateJobStatus(jobId, Contract.RMWeb.JobMonitor.JobStatus.Failed, string.Format("Job has some error.JobID {0}, error message {1}", jobId, ex.Message));
                }
            }
            finally
            {
                if (!checkRunningJobExceedLimit)
                {
                    if (!ShouldDeferQueueCleanup(jqDto?.JobType ?? JobType.All))
                    {
                        JobQueueService.DeleteDBJobQueueMessage(jqDto?.MessageId, jqDto?.TenantGroupId);
                        logger.Info("delete job message success, jobId:{0}, TenantId:{1},", jobId, jqDto?.TenantGroupId);
                    }
                }
            }
        }

        private static readonly HashSet<JobType> DeferredQueueCleanupTypes = new()
        {
            JobType.TeamsRecordsDisposal,
            JobType.TeamsPreScan,
            JobType.TeamsArchiverBackup,
            JobType.RecordsDisposal,
            JobType.SOPreScan,
            JobType.RMArchiverBackup
        };

        private static bool ShouldDeferQueueCleanup(JobType jobType)
        {
            return DeferredQueueCleanupTypes.Contains(jobType);
        }

        private bool CheckHasRunningJobApplySetting(JobType jobType)
        {
            if (!TenantService.IsNewOpusTenant())
            {
                return false;
            }
            return jobType switch
            {
                JobType.ApplySharePointSettings => JobMonitorService.GetRunningSharePointSettingJob().Count > 0,
                JobType.ApplyTeamsSettings => JobMonitorService.GetRunningTeamsSettingJob().Count > 0,
                JobType.EXOApplySetting => JobMonitorService.GetRunningEXOApplySettingJob().Count > 0,
                _ => false
            };
        }

        private bool CheckLicenseAvailable(JobType jobType)
        {
            bool result = false;
            try
            {
                if (TenantService.CheckTenantIsAvailable(TenantLocalValue.LogonGroupId))
                {
                    result = true;
                    var requiredModule = FindSourceByJobType(jobType);
                    if (requiredModule != PaidForModule.None)
                    {
                        //need check additional data source
                        result = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, requiredModule);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"tenant: {TenantLocalValue.LogonGroupId} check license error:{ex.ToString()}");
            }
            
            return result;
        }

        public PaidForModule FindSourceByJobType(JobType jobType)
        {
            PaidForModule source = PaidForModule.None;
            foreach (var jobs in sourceJobMapping)
            {
                if (jobs.Value.Any(v => v.Equals(jobType)))
                {
                    source |= jobs.Key;
                }
            }
            return source;
        }

        private static readonly Dictionary<PaidForModule, List<JobType>> sourceJobMapping = new Dictionary<PaidForModule, List<JobType>>()
        {
            { PaidForModule.FileSystem, new List<JobType>() {
                    JobType.FSBCSTermUsageReport,
                    JobType.FSDisposalSchedule,
                    JobType.FSCreateAndDestroyedFileReport,
                    JobType.FSDashBoard,
                    JobType.FSDataSynchronization,
                    JobType.FSDataSynchronizationSchedule,
                    JobType.FSDisposal,
                    JobType.FSDisposalByClassCode,
                    JobType.FSDisposalSchedule,
                    JobType.FSItemsFilesDueDisposal,
                    JobType.FSOrphanedTermReport,
                    JobType.FSRetiredTermReport,
                    JobType.ImportFSSetting,
                    JobType.ExportFSSetting,
                    JobType.DownloadRCCReport
            }  },
            { PaidForModule.SharePointOnPrem, new List<JobType>() {
                    JobType.SPOnPremApplySettingSchedule,
                    JobType.SPOnPremDataSyncSchedule,
                    JobType.SPOnPremDataSync,
                    JobType.SPOnPremDashBoard,
                    JobType.SPOnPremApplySetting,
                    JobType.SPOnPremBCSTermUsageReport,
                    JobType.SPOnPremCreateAndDestroyedFileReport,
                    JobType.SPOnPremEnforceRuleAction,
                    JobType.SPOnPremEnforceRuleActionSchedule,
                    JobType.SPOnPremItemsFilesDueDisposal,
                    JobType.SPOnPremOrphanedTermReport,
                    JobType.SPOnPremRetiredTermReport,
                    JobType.SPOnPremScanLocalNodes,
                    JobType.SPOnPremTermSynchronization,
                    JobType.SPOnPremTermSynchronizationSchedule,
                    JobType.SPOnPremUniqueIDSettingFullSchedule,
                    JobType.SPOnPremUniqueIDSettingIncrementalSchedule,
            }  },
        };
    }
}
