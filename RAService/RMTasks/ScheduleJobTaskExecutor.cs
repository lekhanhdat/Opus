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
using Aspose.Words.XAttr;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.SharePoint.MoveDataTier;
using Newtonsoft.Json;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.RMTasks
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    /// 
    public class ScheduleJobTaskExecutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(ScheduleJobTaskExecutor));

        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        private IRMSharePointTaxonomyService RMSharePointTaxonomyService => PlatformWindsorManager.GetService<IRMSharePointTaxonomyService>();

        private IRMSharePointSettingsService RMSharePointSettingsService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();

        private IRMOneDriveSettingsService RMOneDriveSettingsService => PlatformWindsorManager.GetService<IRMOneDriveSettingsService>();
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService<IUniqueIdSettingService>();

        private IRMArchivedFullTextIndexService ArchivedFullTextIndexService => PlatformWindsorManager.GetService<IRMArchivedFullTextIndexService>();



        private IManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IManualApprovalService>();

        private IEnforceRetentionService EnforceRetentionService => PlatformWindsorManager.GetService<IEnforceRetentionService>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMJobService mRMJobService => PlatformWindsorManager.GetService<IRMJobService>();

        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();

        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IRMSharePointOnPremSettingsService RMSharePointOnPremSettingsService => PlatformWindsorManager.GetService<IRMSharePointOnPremSettingsService>();
        private IRMSharePointOnPremScanNodeService RMSharePointOnPremScanNodeService => PlatformWindsorManager.GetService<IRMSharePointOnPremScanNodeService>();

        private IDashboardService DashboardService => PlatformWindsorManager.GetService<IDashboardService>();

        private IRMManualApprovalService RMManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();

        private IRMAzureFileSettingsService AzureFileSettingsService => PlatformWindsorManager.GetService<IRMAzureFileSettingsService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        private IRMBoxSettingsService RMBoxSettingsService => PlatformWindsorManager.GetService<IRMBoxSettingsService>();
        private IRMBoxConnectionService RMBoxConnectionService => PlatformWindsorManager.GetService<IRMBoxConnectionService>();
        private IRMBoxConnectionGroupService RMBoxConnectionGroupService => PlatformWindsorManager.GetService<IRMBoxConnectionGroupService>();
        private IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();

        private readonly IRMRestoreSiteMappingDao _restoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        private IRMGoogleJobService RMGoogleJobService => PlatformWindsorManager.GetService<IRMGoogleJobService>();

        private IRMTeamsSettingsService RMTeamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
        private IRMSecurityTrimmingHelper trimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private ILicenseHelperService _licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IStubSettingService StubSettingService => PlatformWindsorManager.GetService<IStubSettingService>();
        private IRMFunctionSettingDao _functionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IMultiGeoDataCenterService _multiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IRMReportService RMReportService => PlatformWindsorManager.GetService<IRMReportService>();
        private IProfileDao ProfileDao => PlatformWindsorManager.GetService<IProfileDao>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                try
                {
                    var tInfos = TenantService.GetAllAvailableTenantInfo();
                    foreach (var tInfo in tInfos)
                    {
                        if (tInfo.ExplorerUpgradeStatus == 2)
                        {
                            logger.Info("Upgrading explorer data, skip tasks for tenant {0}", tInfo.TenantId);
                            continue;
                        }
                        await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, ExcuteScheduleTaskAsync);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred while run schedule job:{0}", ex.ToString());
                }

                logger.Debug("Check schedule job.");
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while excute schedule job,ERROR:{0}", ex.ToString());
            }
            //return System.Threading.Tasks.Task.CompletedTask;
        }

        private async System.Threading.Tasks.Task ExcuteScheduleTaskAsync()
        {
            ScheduleInfo scheduleInfo = null;
            try
            {
                List<ScheduleInfo> runableSchedules = null;
                if (await _functionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao) && !_multiGeoDataCenterService.IsMainDC())
                {
                    logger.Info($"Multi-geo feature is enabled and this is not the main DC, skipping schedule job tenant {TenantLocalValue.LogonGroupId}.");
                    List<ScheduleType> allowedOtherDCJobTypes = new List<ScheduleType>
                    {
                        ScheduleType.Dashboard,
                        ScheduleType.ManualApprovalEmailSchedule,
                        ScheduleType.FSDisposalSchedule,
                        ScheduleType.FSColletionDataSchedule
                    };
                    runableSchedules = await ScheduleService.GetRunableScheduleByTypeAsync(allowedOtherDCJobTypes);
                }
                else
                {
                    runableSchedules = await ScheduleService.GetRunableScheduleAsync();
                }
                foreach (ScheduleInfo schedule in runableSchedules)
                {
                    scheduleInfo = schedule;
                    if (schedule.JobCategory == ScheduleType.SyncSchedule)
                    {
                        logger.Info("Run sync schedule job.");
                        await RMSharePointTaxonomyService.RunSyncRMTermTreeToSharePointAsync(JobRunBy.Schedule, true);
                        logger.Info("Run sync schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.SharePointSettingSchedule)
                    {
                        logger.Info("Begin run sharePoint setting Schedule job.");
                        RMSharePointSettingsService.RunSharepointSettingsScheduleJob(JobRunBy.Schedule);//TO DO ylgu
                        logger.Info("Run sharePoint setting Schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.TeamsSettingSchedule)
                    {
                        logger.Info("Begin run teams setting Schedule job.");
                        RMTeamsSettingsService.RunTeamsSettingsScheduleJob(JobRunBy.Schedule);
                        logger.Info("Run teams setting Schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.DisposalSchedule)
                    {
                        logger.Info("Begin run Disposal Schedule job.");
                        var treeNode = JsonConvert.DeserializeObject<Contract.Object.RMSPTreeNode>(schedule.Extentions);
                        if (mRMJobService.CheckIsOneDriveNode(treeNode))
                        {
                            logger.Info($"Skip run disposal schedule for onedrive node. Full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else if (RMKeyValueDao.HasUpgradeTeams() && mRMJobService.CheckIsTeamsNode(treeNode))
                        {
                            logger.Info($"Skip run disposal schedule for teams node. Full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else
                        {
                            if (mRMJobService.CheckIsRemoteSite(treeNode))
                            {
                                logger.Info($"Skip run disposal schedule node full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                            }
                            else
                            {
                                RAReturnMessage msg;
                                if (TenantService.IsCSDTenant())
                                {
                                    msg = mRMJobService.RunDeclaredOnly(treeNode, JobRunBy.Schedule);
                                }
                                else if (TenantService.IsNewOpusTenant())
                                {
                                    msg = RMSharePointSettingsService.RunRecordsDisposalJob(treeNode, JobRunBy.Schedule);
                                }
                                else
                                {
                                    msg = await mRMJobService.RunNowAsync(treeNode, JobRunBy.Schedule);
                                }
                                logger.Info("run disposal schedule job,id:{0}, result:{1}", schedule.Id, msg.MessageType == Contract.Object.RAMessageType.Failed ? msg.ErrorMessage : "success");
                                logger.Info("Run Disposal Schedule job finish.");
                            }
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.RebuildStubSchedule)
                    {
                        logger.Info("Begin run Rebuild Stub Schedule job.");
                        //RebuildStubInfo rebuildStubInfo = new RebuildStubInfo();
                        //rebuildStubInfo.RebuildJobId = "SO20240227154731023625";
                        //rebuildStubInfo.StubTemplateName = "NewStub";
                        //schedule.Extentions = JsonConvert.SerializeObject(rebuildStubInfo);
                        var stubInfo = JsonConvert.DeserializeObject<RebuildStubInfo>(schedule.Extentions);
                        RAReturnMessage msg;
                        msg = RMSharePointSettingsService.RunRebuildStubJob(stubInfo, JobRunBy.Schedule);
                        logger.Info("run Rebuild Stub schedule job,id:{0}, result:{1}", schedule.Id, msg.MessageType == Contract.Object.RAMessageType.Failed ? msg.ErrorMessage : "success");
                        logger.Info("Run Rebuild Stub Schedule job finish.");
                        //Disable Current Schedule.
                    }
                    else if (schedule.JobCategory == ScheduleType.RebuildIndexSchedule)
                    {
                        logger.Info($"Begin run Rebuild Index Schedule job. Extentions: {schedule.Extentions}");
                        var msg = RMSharePointSettingsService.RunRebuildIndexJob(schedule.Extentions, JobRunBy.Schedule);
                        logger.Info($"run Rebuild Index schedule job, id:{schedule.Id}, result:{(msg.MessageType == RAMessageType.Failed ? msg.ErrorMessage : "success")}");
                    }
                    else if (schedule.JobCategory == ScheduleType.UniqueIDSettingSchedule)
                    {
                        logger.Info("Begin run UniqueID setting Incremental schedule job.");
                        UniqueIdSettingService.RunUniqueIDSettingScheduleJob(JobRunBy.Schedule, JobType.UniqueIDSettingIncrementalSchedule/*, true*/);
                        logger.Info("Run UniqueID setting Incremental schedule job finish.");
                    }

                    else if (schedule.JobCategory == ScheduleType.ManualApprovalScheduleTimer)
                    {
                        logger.Info("Begin run manual approval timer Schedule job.");
                        ManualApprovalService.RunManualApprovalTimerJob(JobRunBy.Schedule);
                        logger.Info("Run manual approval timer Schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.EnforceRetention)
                    {
                        logger.Info("run enforce retention job schedule job.");

                        EnforceRetentionService.RunScheduleJob(JobRunBy.Schedule, JobType.EnforceRetention);

                        logger.Info("run enforce retention job schedule job success.");

                    }
                    else if (schedule.JobCategory == ScheduleType.EXOEnforceRetention)
                    {
                        logger.Info("run exo enforce retention job schedule job.");

                        EnforceRetentionService.RunEXOScheduleJob(JobRunBy.Schedule, JobType.EXOEnforceRetention);

                        logger.Info("run exo enforce retention job schedule job success.");

                    }
                    else if (schedule.JobCategory == ScheduleType.TeamsEnforceRetention)
                    {
                        if(RMKeyValueDao.HasUpgradeTeams())
                        {
                        logger.Info("run teams enforce retention job schedule job.");

                        EnforceRetentionService.RunTeamsScheduleJob(JobRunBy.Schedule, JobType.TeamsEnforceRetention);

                        logger.Info("run teams enforce retention job schedule job success.");
                        }
                        else
                        {
                            logger.Info("This account does not upgrade teams so skip");
                        }
                    }
                    //TODO 95 xwwang
                    //else if (schedule.JobCategory == ScheduleType.ColletionDataSchedule)
                    //{
                    //    logger.Info("Run collection job schedule job.");
                    //    var nodeInfo = RMSharePointSettingsService.GetTreeNodeInfoByScheduleId(ScheduleType.ColletionDataSchedule, schedule.Id);
                    //    var selectedTree = SerializerHelper.DeserializeByDataContractSerializer<Contract.Object.RMSPTreeNode>(nodeInfo);
                    //    RMSharePointSettingsService.RunDataSyncJob(selectedTree, JobRunBy.Schedule);

                    //    logger.Info("Run collection job schedule job success.");
                    //}
                    else if (schedule.JobCategory == ScheduleType.EXODisposalSchedule)
                    {
                        logger.Info("Begin run EXO Disposal Schedule job.");

                        var treeNode = JsonConvert.DeserializeObject<Contract.Object.RMEXOTreeNode>(schedule.Extentions);
                        if (mRMJobService.CheckEXONodeMoved(treeNode))
                        {
                            logger.Info($"Skip run exo disposal schedule node full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else
                        {
                            RAReturnMessage msg;
                            if (TenantService.IsNewOpusTenant())
                            {
                                var tempSetting = await RMSharePointSettingsService.LoadExchangeNodeSettingAsync(RMDtoConverter.ConvertTreeNodeDto2RMSampleExchangeTree(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(treeNode)));
                                treeNode.IsNullClassificationSetting = tempSetting.IsNullClassificationSetting;
                                msg = RMSharePointSettingsService.RunEXORecordsDisposalJob(treeNode, JobRunBy.Schedule);
                            }
                            else
                            {
                                msg = await mRMJobService.RunEXONowAsync(treeNode, JobRunBy.Schedule);
                            }
                            logger.Info("run exo disposal schedule job,id:{0}, result:{1}", schedule.Id, msg.MessageType == Contract.Object.RAMessageType.Failed ? msg.ErrorMessage : "success");
                            logger.Info("Run EXO Disposal Schedule job finish.");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.SPSyncDataSchedule)
                    {
                        logger.Info("Begin run SP Data Sync Schedule job.");
                        RMSharePointSettingsService.RunSPDataSyncScheduleJob(JobRunBy.Schedule);
                        logger.Info("Begin run SP Data Sync Schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.EXOSyncDataSchedule)
                    {
                        logger.Info("Begin run EXO Data Sync Schedule job.");
                        RMSharePointSettingsService.RunEXODataSyncScheduleJob(JobRunBy.Schedule);
                        logger.Info("Begin run EXO Data Sync Schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.EXOApplypSchedule)
                    {
                        logger.Info("Begin run EXO apply setting Schedule job.");
                        RMSharePointSettingsService.RunEXOSettingsScheduleJob(JobRunBy.Schedule);
                        logger.Info("Begin run EXO apply setting Schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.PRDisposalSchedule)
                    {
                        logger.Info("Begin run physical disposal schedule job.");
                        //RMPhysicalRecordSettingsService.RunPhysicalDisposalScheduleJob(schedule.ProfileId, JobRunBy.Schedule);
                        bool skipRemoveContentAndDestroyAction = bool.Parse(schedule.Extentions);
                        var guids = schedule.ProfileId.Split('|').ToList();
                        Guid locationUniqueId = new Guid(guids.Last());
                        var locationIntId = RMLocationDao.GetLocationByUniqueId(locationUniqueId);
                        if (TenantService.IsNewOpusTenant())
                        {
                            await mRMJobService.NewOpusTenantRunPhysicalJobNowAsync(locationIntId.Id, JobRunBy.Schedule, skipRemoveContentAndDestroyAction);
                        }
                        else
                        {
                            await mRMJobService.OldOpusTenantRunPhysicalJobNowAsync(locationIntId.Id, JobRunBy.Schedule, skipRemoveContentAndDestroyAction);
                        }
                        //RMPhysicalRecordSettingsService.RunPhysicalDisposalScheduleJob(schedule.ProfileId);
                        logger.Info("End run physical disposal schedule job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.PRExplorerTimer)
                    {
                        logger.Info("Begin run physical explorer timer job.");
                        ExplorerService.RunPhysicalTimerJob(JobRunBy.Schedule);
                        logger.Info("End run physical explorer timer job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.ConnectorExplorerTimer)
                    {
                        logger.Info("Begin run connector explorer timer job.");
                        ExplorerService.RunConnectorTimerJob(JobRunBy.Schedule);
                        logger.Info("End run connector explorer timer job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.FSColletionDataSchedule)
                    {
                        logger.Info("Begin run FS Data Sync Schedule job.");
                        RMFileSystemSettingsService.RunFSDataSyncScheduleJob(JobRunBy.Schedule);
                        logger.Info("End run FS Data Sync Schedule job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.ContentDueForAction ||
                             schedule.JobCategory == ScheduleType.SPOActionAuditReport ||
                             schedule.JobCategory == ScheduleType.RestoreReport ||
                             schedule.JobCategory == ScheduleType.ArchivedSiteReport)
                    {
                        logger.Info("Begin run scheduled report job. ScheduleId={0}, Type={1}", schedule.Id, schedule.JobCategory);
                        await RMReportService.GenarateReportSchedule(schedule.Id);
                        logger.Info("End run scheduled report job. ScheduleId={0}, Type={1}", schedule.Id, schedule.JobCategory);
                    }                                   
                    else if (schedule.JobCategory == ScheduleType.FSDisposalSchedule)
                    {
                        logger.Info("Begin run FS Disposal Schedule job.");
                        var treeNode = JsonConvert.DeserializeObject<Contract.Object.RMFSTreeNode>(schedule.Extentions);
                        if (mRMJobService.IsFSConnectionDeleted(treeNode))
                        {
                            logger.Warn($"Connection has been deleted, will not run disposal job. Id:{treeNode.FullPath}");
                        }
                        else
                        {
                            var isEnabledJPMC = await RMKeyValueDao.GetValueByKeyAsync(KeyNameCollection.EnableJPMCFileSystemFeature, false);
                            if (isEnabledJPMC)
                            {
                                var isEnabledRecordManagement = await RMFileSystemSettingsService.LoadFSNodeEnableRecordManagement(treeNode.Id);
                                if (!isEnabledRecordManagement)
                                {
                                    logger.Info($"Skip run FS Disposal Schedule job for node {treeNode.FullPath} since record management is not enabled.");
                                    return;
                                }
                            }
                            await RMFileSystemSettingsService.RunFSDisposalScheduleJobAsync(treeNode, JobRunBy.Schedule);
                        }
                        logger.Info("End run FS Disposal Schedule job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.SyncSecurityContainerSchedule)
                    {
                        logger.Info("Begin run Sync Security Container schedule job");
                        //SecurityContainerService.RunScheduleJob(JobRunBy.Schedule);
                        logger.Info("End run Sync Security Container schedule job");
                    }
                    else if (schedule.JobCategory == ScheduleType.SPOnPremScanNodesSchedule)
                    {
                        logger.Info("Begin run Scan Local Nodes schedule job");
                        RMSharePointOnPremScanNodeService.RunScheduleJob(JobRunBy.Schedule);
                        logger.Info("End run Scan Local Nodes  schedule job");
                    }
                    else if (schedule.JobCategory == ScheduleType.SPOnPremApplySettingSchedule)
                    {
                        logger.Info("Begin run sp on premise apply setting schedule job");
                        RMSharePointOnPremSettingsService.RunSharepointSettingsScheduleJob(JobRunBy.Schedule);
                        logger.Info("End run sp on premise apply setting schedule job");
                    }
                    else if (schedule.JobCategory == ScheduleType.SPOnPremDataSyncSchedule)
                    {
                        logger.Info("Begin run sp on premise data sync schedule job");
                        RMSharePointOnPremSettingsService.RunSPDataSyncScheduleJob(JobRunBy.Schedule);
                        logger.Info("End run sp on premise data sync schedule job");
                    }
                    else if (schedule.JobCategory == ScheduleType.AzureFileShareDataSyncSchedule)
                    {
                        logger.Info("Begin run azure file share data sync schedule job");
                        AzureFileSettingsService.RunDataSyncScheduleJob();
                        logger.Info("End run azure file share data sync schedule job");
                    }
                    else if (schedule.JobCategory == ScheduleType.SPOnPremDisposalSchedule)
                    {
                        logger.Info("Begin run OnPrem Disposal schedule job");
                        var treeNode = JsonConvert.DeserializeObject<Contract.Object.RMSPTreeNode>(schedule.Extentions);
                        RMSharePointOnPremSettingsService.RunOnPremiseEnforceRuleActionScheduleJob(treeNode, JobRunBy.Schedule);
                        logger.Info("End run Scan Local Nodes  schedule job");
                    }
                    else if (schedule.JobCategory == ScheduleType.SPOnPremUniqueIDSettingSchedule)
                    {

                        logger.Info("Begin run OnPrem UniqueID setting Incremental schedule job.");
                        UniqueIdSettingService.RunUniqueIDSettingScheduleJob(JobRunBy.Schedule, JobType.SPOnPremUniqueIDSettingIncrementalSchedule);
                        logger.Info("Run OnPrem UniqueID setting Incremental schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.OneDriveSyncDataSchedule)
                    {
                        logger.Info("Begin run onedrive data sync schedule job.");
                        RMOneDriveSettingsService.RunOneDriveDataSyncScheduleJob(JobRunBy.Schedule);
                        logger.Info("Run onedrive data sync schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.OneDriveDisposalSchedule)
                    {
                        logger.Info("Begin run OneDrive Disposal Schedule job.");
                        var treeNode = JsonConvert.DeserializeObject<Contract.Object.RMSPTreeNode>(schedule.Extentions);
                        if (mRMJobService.CheckIsRemoteSite(treeNode))
                        {
                            logger.Info($"Skip run onedrive disposal schedule node full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else
                        {
                            RAReturnMessage msg;
                            if (TenantService.IsNewOpusTenant())
                            {
                                msg = RMOneDriveSettingsService.RunRecordsDisposalJob(treeNode, JobRunBy.Schedule);
                            }
                            else
                            {
                                msg = await mRMJobService.RunOneDriveNowAsync(treeNode, JobRunBy.Schedule);
                            }
                            logger.Info("run OneDrive disposal schedule job,id:{0}, result:{1}", schedule.Id, msg.MessageType == Contract.Object.RAMessageType.Failed ? msg.ErrorMessage : "success");
                            logger.Info("Run OneDrive Disposal Schedule job finish.");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.OneDriveEnforceRetention)
                    {
                        logger.Info("run OneDrive enforce retention job schedule job.");
                        EnforceRetentionService.RunOneDriveScheduleJob(JobRunBy.Schedule, JobType.OneDriveEnforceRetention);
                        logger.Info("run OneDrive enforce retention job schedule job success.");

                    }
                    else if (schedule.JobCategory == ScheduleType.ArchiveFullTextIndex)
                    {
                        var enable = RestoreSearchService.CanSendFullTextIndexJobMessage();
                        if (enable)
                        {
                            logger.Info($"Current tenant is enable full text index search. Run archive full text index job.");
                            ArchivedFullTextIndexService.SendJobMessage();
                            logger.Info($"End run archive full text index job.");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.Dashboard)
                    {
                        DashboardService.SchduleRunDashboardJob(JobRunBy.Schedule);
                    }
                    else if (schedule.JobCategory == ScheduleType.ManualApprovalEmailSchedule)
                    {
                        RMManualApprovalService.SchduleRunEmailScheduleJob(JobRunBy.Schedule);
                    }
                    else if (schedule.JobCategory == ScheduleType.SPArchiveJobSchedule || schedule.JobCategory == ScheduleType.OneDriveArchiveJobSchedule)
                    {
                        var treeNode = JsonConvert.DeserializeObject<Contract.Object.RMSPTreeNode>(schedule.Extentions);
                        if (mRMJobService.CheckIsRemoteSite(treeNode))
                        {
                            logger.Info($"Skip run onedrive disposal schedule node full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else if (RMKeyValueDao.HasUpgradeTeams() && mRMJobService.CheckIsTeamsNode(treeNode))
                        {
                            logger.Info($"Skip run disposal schedule for teams node. Full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else
                        {
                            logger.Info("run Archive schedule job.");
                            RMArchiverSettingsService.RunArchiverJob(treeNode, JobRunBy.Schedule);
                            logger.Info("run Archive schedule job success.");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.ArchiveDataRetentionSchedule)
                    {
                        bool hasGControlLicense = TenantService.HasInitGControlPlatForm().Result;

                        if (_licenseHelperService.HasOpusSPILOrSOLicense)
                        {
                            logger.Info("run Archive DataRetention schedule job.");
                            var spJobId = await RMArchiverSettingsService.RealRunArchiverRetentionJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info("run Archive DataRetention schedule job success.");
                        }
                        
                        var user = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                        TenantLocalValue.LogonUserId = user?.UserId;
                        if (await trimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSEnduser))
                        {
                            logger.Info("run FS DataRetention schedule job.");
                            var fsJobId = await RMArchiverSettingsService.RealRunFSRetentionJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info("run FS DataRetention schedule job success.");
                        }
                        
                        if (trimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin).GetAwaiter().GetResult() || hasGControlLicense)
                        {
                            logger.Info("run Google DataRetention schedule job.");
                            var gDriveMessage = RMArchiverSettingsService.RealRunGDriveArchiverRetentionJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info("run Google DataRetention schedule job success.");
                        }

                        //logger.Info("run FS DataRetention simulate schedule job.");
                        //await RMArchiverSettingsService.RealRunFSRetentionJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule", true, fsJobId);
                        //logger.Info("run FS DataRetention simulate schedule job success.");

                        if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
                        {
                            logger.Info("run teams Archive DataRetention schedule job.");
                            await RMArchiverSettingsService.RealRunTeamsArchiverRetentionJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info("run teams Archive DataRetention schedule job success.");

                            logger.Info("run EXO Archive DataRetention schedule job.");
                            await RMArchiverSettingsService.RealRunEXOArchiverRetentionJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info("run EXO Archive DataRetention schedule job success.");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.ArchiverDeleteRestoredDataSchedule)
                    {
                        logger.Info("run Archive DeleteRestoredData schedule job.");
                        RMArchiverSettingsService.RealRunArchiverDeleteRestoredDataJob(JobRunBy.Schedule, "RM_TS_RunSchedule");
                        logger.Info("run Archive DeleteRestoredData schedule job success.");
                    }
                    else if (schedule.JobCategory == ScheduleType.BoxDisposalSchedule)
                    {
                        logger.Info("Begin run Box Disposal schedule job");
                        var treeNode = JsonConvert.DeserializeObject<BoxSettingDto>(schedule.Extentions);
                        if (treeNode.SelectedNode == null)
                        {
                            logger.Info("selected node is null");
                            return;
                        }
                        var selectedNodeId = Guid.TryParse(treeNode.SelectedNode.Id, out var nodeId) ? nodeId : Guid.Empty;
                        var connectionId = Guid.TryParse(treeNode.SelectedNode.ConnectionId, out var connId) ? connId : Guid.Empty;
                        if ((treeNode.SelectedNode.Level == RMNodeLevel.BoxConnectionGroup && RMBoxConnectionGroupService.Exists(selectedNodeId)) ||
                            RMBoxConnectionService.Exists(connectionId))
                        {
                            RMBoxSettingsService.RunBoxEnforceRuleActionScheduleJob(treeNode, JobRunBy.Schedule);
                            logger.Info("End run Box Disposal schedule job");
                        }
                        else
                        {
                            logger.Info($"The Box connection or connection group has been deleted, will not run disposal job. ScopeId: {selectedNodeId}, Level: {treeNode.SelectedNode.Level}, ProfileId: {schedule.ProfileId}");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.BoxDataSyncSchedule)
                    {
                        logger.Info("Begin run box data sync schedule job");
                        RMBoxSettingsService.EnqueueDataSyncScheduleJob(false);
                        logger.Info("End run box data sync schedule job");
                    }
                    else if (schedule.JobCategory == ScheduleType.ApprovalProcessJob)
                    {
                        var approvalOption = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.EnableAutoApprovedProcess);
                        if (!string.IsNullOrEmpty(approvalOption))
                        {
                            bool needRunApproval = Convert.ToBoolean(approvalOption);
                            if (needRunApproval)
                            {
                                var enableDeleteInvalidRecordsOption = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.EnableDeleteInvalidRecords);
                                if (!string.IsNullOrEmpty(enableDeleteInvalidRecordsOption) &&
                                    Convert.ToBoolean(enableDeleteInvalidRecordsOption))
                                {
                                    logger.Info("run Delete Invalid Records schedule job.");
                                    RMManualApprovalService.RunDeleteInvalidRecordsJob(JobRunBy.Schedule, "RM_TS_RunSchedule");
                                    logger.Info("run Delete Invalid Records schedule job success.");
                                }
                                logger.Info("run Approval process schedule job.");
                                RMArchiverSettingsService.RunApprovalProcessJob(JobRunBy.Schedule, "RM_TS_RunSchedule");
                                logger.Info("run Approval process schedule job success.");
                            }
                            else
                            {
                                logger.Info("Approval process is disabled, skip run Approval process schedule job.");
                            }
                        }
                        else
                        {
                            logger.Info("Approval process is not exsit, skip run Approval process schedule job.");
                        }
                    }
                    //else if (schedule.JobCategory == ScheduleType.MoveDataTierSchedule)
                    //{
                    //    if (RMJobService.GetRunningJobsCount(JobType.MoveDataTier) == 0)
                    //    {
                    //        var jobidMapping = GetJobIdsForMoveDataTier();
                    //        if (jobidMapping != null && jobidMapping.Count > 0)
                    //        {
                    //            logger.Info("run Move DataTier schedule job.");
                    //            await RMArchiverSettingsService.RealRunMoveDataTierJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule", jobidMapping);
                    //            logger.Info("run Move DataTier schedule job. success.");
                    //        }
                    //        else
                    //        {
                    //            logger.Info("jobidList count less than zero when run Move Data Tier Job");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        logger.Info("there has move data tier job is running ,skip this move data tier job");
                    //    }
                    //}
                    else if (schedule.JobCategory == ScheduleType.AdjustSizeSchedule)
                    {
                        if (RMJobService.GetRunningJobsCount(JobType.AdjustStorageSize) == 0)
                        {
                            logger.Info("run adjust storage size schedule job.");
                            var key = RMKeyValueDao.GetValueByKey("RunAdjustStorageSize");
                            if (key == null)
                            {
                                RMKeyValueDao.Save(new RMKeyValue() { Key = "RunAdjustStorageSize", Value = "false" });
                                logger.Info("not exist RunAdjustStorageSize,run adjust storage size schedule job. no.");
                            }
                            else
                            {
                                bool.TryParse(key?.Value, out bool result);
                                if (result)
                                {
                                    await RMArchiverSettingsService.RealRunAdjustStorageSizeJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                                    await RMKeyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = "RunAdjustStorageSize", Value = "false" });
                                    logger.Info("run adjust storage size schedule job. success.");
                                }
                                else
                                {
                                    logger.Info("run adjust storage size schedule job. failed,RunAdjustStorageSize is false.");
                                }
                            }
                        }
                        else
                        {
                            logger.Info("there has adjust storage size job is running ,skip this adjust storage size job");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.JobNotificationSchedule)
                    {
                        logger.Info("Run job notification schedule.");
                        await new ProcessJobEmailNotificationExecutor().ExecutorAsync();
                        logger.Info("Run job notification schedule success.");
                    }
                    else if (schedule.JobCategory == ScheduleType.ArchiverDedupJobSchedule)
                    {
                        logger.Info("run Archiver Dedup schedule job.");
                        if (SettingProfileService.IsEnableArchiverDeduplication())
                        {
                            await RMArchiverSettingsService.RealRunArchiverDedupJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info("run Archiver Dedup schedule job success.");
                        }
                        else
                        {
                            logger.Warn("Not enable deduplication, so skip the schedule job.");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.GoogleDataSyncSchedule)
                    {
                        logger.Info("Begin run Google Data Sync Schedule job.");
                        RMGoogleJobService.RunDataSyncJob(JobRunBy.Schedule, "RM_TS_RunSchedule");
                        logger.Info("End run Google Data Sync Schedule job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.GoogleSettingSchedule)
                    {
                        logger.Info("Begin run Google Setting Schedule job.");
                        RMGoogleJobService.ApplySettings(JobRunBy.Schedule, true, RunApplySettingMethod.Auto);
                        logger.Info("End run Google Setting Schedule job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.GoogleDisposalSchedule)
                    {
                        logger.Info("Begin run Google Disposal Schedule job.");
                        var treeNode = JsonConvert.DeserializeObject<RMGoogleTreeNode>(schedule.Extentions);
                        await RMGoogleJobService.RunEnforceRuleActionScheduleJobAsync(treeNode, JobRunBy.Schedule);
                        logger.Info("End run Google Disposal Schedule job.");
                    }
                    else if (schedule.JobCategory == ScheduleType.TeamsSyncDataSchedule)
                    {
                        logger.Info("Begin run Teams Data Sync Schedule job.");
                        RMTeamsSettingsService.RunDataSyncScheduleJob(JobRunBy.Schedule);
                        logger.Info("Teams Data Sync Schedule job has finished.");
                    }
                    else if (schedule.JobCategory == ScheduleType.TeamsDisposalSchedule)
                    {
                        logger.Info("Begin run Teams disposal schedule job.");
                        var treeNode = JsonConvert.DeserializeObject<RMSPTreeNode>(schedule.Extentions);
                        if (mRMJobService.CheckIsRemoteTeamsExisting(treeNode))
                        {
                            logger.Info($"Skip run disposal schedule node full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else
                        {
                            RAReturnMessage msg = new();
                            if (TenantService.IsNewOpusTenant())
                            {
                                msg = RMTeamsSettingsService.RunRecordsDisposalJob(treeNode, JobRunBy.Schedule);
                            }
                            logger.Info("Teams disposal schedule job,id:{0}, result:{1}", schedule.Id, msg.MessageType == Contract.Object.RAMessageType.Failed ? msg.ErrorMessage : "success");
                        }
                        logger.Info("Teams disposal schedule job has finished.");
                    }
                    else if (schedule.JobCategory == ScheduleType.TeamsUniqueIDSettingSchedule)
                    {
                        logger.Info("Begin run Teams UniqueID setting Incremental schedule job.");
                        UniqueIdSettingService.RunTeamsUniqueIDSettingScheduleJob(JobRunBy.Schedule, JobType.TeamsUniqueIDSettingIncrementalSchedule);
                        logger.Info("Run Teams UniqueID setting Incremental schedule job finish.");
                    }
                    else if (schedule.JobCategory == ScheduleType.TeamsArchiveJobSchedule)
                    {
                        var treeNode = JsonConvert.DeserializeObject<RMSPTreeNode>(schedule.Extentions);
                        if (mRMJobService.CheckIsRemoteTeamsExisting(treeNode))
                        {
                            logger.Info($"Skip run onedrive disposal schedule node full path: [{(GCommon.Contract.Tree.Object.NodeLevel)treeNode.Level}]{treeNode.FullPath}.");
                        }
                        else
                        {
                            logger.Info("Begin run Teams archive schedule job.");
                            RMArchiverSettingsService.RunTeamsArchiverJob(treeNode, JobRunBy.Schedule);
                        }
                        logger.Info("Teams archive job has finished.");
                    }
                    else if (schedule.JobCategory == ScheduleType.JobMonitorArchiveSchedule)
                    {
                        logger.Info("Begin run JobMonitor archive schedule job.");
                        try
                        {
                            // Enqueue a job so RealTimeJobTaskExecutor processes it uniformly
                            var msg = RMArchiverSettingsService.RunJobMonitorArchiveJob(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info($"JobMonitor archive schedule enqueued. Result: {msg.MessageType}, Id: {msg.Extension}");
                        }
                        catch (Exception e)
                        {
                            logger.Error($"JobMonitor archive schedule enqueue failed: {e}");
                        }
                    }
                    else if (schedule.JobCategory == ScheduleType.StubDisposalSchedule)
                    {
                        logger.Info("run stub disposal job schedule job.");
                        StubSettingService.RunStubDisposalJob(JobRunBy.Schedule, "RM_TS_RunSchedule");
                        logger.Info("run stub disposal job schedule job success.");

                    }
                    else if (schedule.JobCategory == ScheduleType.HoldNotificationSchedule)
                    {
                        logger.Info("Run hold notification schedule.");
                        await new ProcessHoldNotificationExecutor().ExecutorAsync();
                        logger.Info("Run hold notification schedule success.");
                    }
                    else if (schedule.JobCategory == ScheduleType.APStorageCostEvaluationSchedule)
                    {
                        try
                        {
                            logger.Info("run AvePoint Storage Cost Evaluation schedule job.");
                            var jobId = await RMArchiverSettingsService.RealRunAPStorageCostEvaluationJobAsync(JobRunBy.Schedule, "RM_TS_RunSchedule");
                            logger.Info($"run AvePoint Storage Cost Evaluation schedule job success. JobId: {jobId}");
                        }
                        catch (Exception e)
                        {
                            logger.Error($"AvePoint Storage Cost Evaluation schedule job failed: {e}");
                        }
                    }
                    else
                    {
                        switch (schedule.JobCategory)
                        {
                            case ScheduleType.TeamsEnforceRetention:
                                {
                                    logger.Info("run Teams enforce retention job schedule job.");
                                    //EnforceRetentionService.RunTeamsScheduleJob(JobRunBy.Schedule, JobType.TeamsEnforceRetention);
                                    logger.Info("run Teams enforce retention job schedule job success.");
                                }
                                break;
                            default:
                                logger.Info($"skip run schedule job:{schedule?.JobCategory}, id:{schedule?.ProfileId}.");
                                break;
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Error("Error occurred while run schedue job, Id:{0}, jobType:{1}, ERROR:{2}", scheduleInfo?.Id, scheduleInfo?.JobCategory, ex.ToString());
            }

        }
    }
}
