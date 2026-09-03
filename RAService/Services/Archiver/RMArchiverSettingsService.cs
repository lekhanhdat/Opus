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
using AutoMapper.Internal;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.MediaManagement.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Connector;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.Event;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.GraphApi.GroupSite;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.DiscoveryPlan;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Retention;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.PlanProfile;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Archiver.AuditHandler;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using AvePoint.RA.Service.Services.Dashboard.AuditHandler;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Audit;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.MachineLearningManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.Service.Services.Schedule;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using AvePoint.RA.Service.Services.SignalR;
using AvePoint.RA.Service.SharePointSetting;
using AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.MoveDataTier;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using DocumentFormat.OpenXml.Presentation;
using Google.Apis.Storage.v1;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.News.DataModel;
using Newtonsoft.Json;
using PnP.Framework.Modernization.Transform;
using RAArchiverCommon;
using RAExportCommon;
using RATeams;
using Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using RetentionRule = AvePoint.GCommon.Contract.Storage.Entity.RetentionRule;
using Rule = AvePoint.GCommon.Contract.StorageOptimization.Object.Rule;
using RuleType = AvePoint.RA.DB.Dao.Impl.RuleType;
using TreeMode = AvePoint.RA.Contract.Object.TreeMode;

namespace AvePoint.RA.Service.Services.Archiver
{
    [Audit]
    public class RMArchiverSettingsService : BaseContentRepositorySettingsService, IRMArchiverSettingsService
    {
        private IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRestoredSitesInfoDao RestoredSitesInfoDao => PlatformWindsorManager.GetService<IRestoredSitesInfoDao>();
        private IHybridFileSystemWorkerService HybridFileSystemWorkerService => PlatformWindsorManager.GetService<IHybridFileSystemWorkerService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMJobQueueDao RMJobQueueDao => PlatformWindsorManager.GetService<IRMJobQueueDao>();
        private IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IFSIndexSubInfoDao FSIndexSubInfoDao => PlatformWindsorManager.GetService<IFSIndexSubInfoDao>();
        private IEXOArchiverIndexSubInfoDao EXOArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
        private IFSMasterIndexDao FSMasterIndexDao => PlatformWindsorManager.GetService<IFSMasterIndexDao>();
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService<IRMFileSystemRegisterService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMMiscProfileDao MiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private ICommonSiteMasterIndexDao ArchiverTeamsMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
        private IRMSharePointSettingsService RMSharePointSettingsService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IRMGoogleSettingDao GoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();
        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IRMOptimizationSettingInfoDao RMOptimizationSettingInfoDao => PlatformWindsorManager.GetService<IRMOptimizationSettingInfoDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private ITeamsSettingTreeService TeamsSettingTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private ITeamsSettingDao TeamsSettingsDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private IRMRetentionSimulateInfosDao RententionInfosDao => PlatformWindsorManager.GetService<IRMRetentionSimulateInfosDao>();
        private IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private readonly IRMDiscoveryOffice365OptimizationSettingsInfoDao _optimizationSettingsInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();
        private readonly IRMDiscoveryOffice365SiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryOffice365SiteOptimizationMappingTableDao();
        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();
        private readonly IRMDiscoveryOffice365JobDao _jobDao = new RMDiscoveryOffice365JobDao();
        private readonly IRMDiscoveryOffice365ProgressDao _optimizationDao = new RMDiscoveryOffice365ProgeressDao();
        private readonly IRMDiscoveryPlanProfileDao _planProfileDao = new RMDiscoveryPlanProfileDao();
        private readonly IRMDiscoveryPlanSiteMappingDao _planSiteMappingDao = new RMDiscoveryPlanSiteMappingDao();

        private readonly IRMDiscoveryAOSPOptimizationSettingsInfoDao _optimizationAOSPSettingsInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();
        private readonly IRMDiscoveryAOSPSiteOptimizationMappingTableDao _siteAOSPOptimizationMappingTableDao = new RMDiscoveryAOSPSiteOptimizationMappingTableDao();
        private readonly IRMDiscoveryAOSPNodeDao _nodeAOSPDao = new RMDiscoveryAOSPNodeDao();
        private readonly IRMDiscoveryAOSPProgressDao _optimizationAOSPDao = new RMDiscoveryAOSPProgressDao();
        private readonly IRMDiscoveryAOSPJobDao _jobAOSPDao = new RMDiscoveryAOSPJobDao();
        private readonly IRMArchiveSiteInfoDao _archiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private readonly IJobProgressDao _jobProgressDao = PlatformWindsorManager.GetService<IJobProgressDao>();
        private IEXOArchiverIndexSubInfoDao EXOArhciverSubInfo = PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
        private RALogger logger = RALogger.GetInstance(typeof(RMArchiverSettingsService));
        private DB.Explorer.Dao.IExplorerDao explorerDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        private const int OptimizationQueryBatchSize = 1000;
        private const int OptimizationInsertBatchSize = 1000;

        public void LoadArchiverSettingIcon(List<RMSPSampleTreeNode> nodes, ScheduleType scheduleType)
        {
            try
            {
                if (nodes.Count > 0)
                {
                    RMSPSampleTreeNode groupNode = nodes[0];
                    if (groupNode.Level != (int)NodeLevel.WebApplication)
                    {
                        while (groupNode != null && groupNode.Level != (int)NodeLevel.WebApplication)
                        {
                            groupNode = groupNode.Parent;
                        }

                        Guid groupId = Guid.Empty;
                        if (groupNode != null)
                        {
                            groupId = new Guid(groupNode.SPObjectId);
                        }

                        var gsSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(groupId, Guid.Empty, Guid.Empty
                            , (SourceFlag)groupNode?.SourceType == SourceFlag.Teams ? ContentSourceType.Teams : ContentSourceType.SharePoint);

                        if (gsSetting == null)
                        {
                            RMSPSampleTreeNode tempSiteNode = nodes[0];
                            if (tempSiteNode.SourceType == (int)SourceFlag.Teams)
                            {
                                if (tempSiteNode.Level != (int)NodeLevel.Office365GroupEntire)
                                {
                                    while (tempSiteNode != null && tempSiteNode.Level != (int)NodeLevel.SiteCollection)
                                    {
                                        tempSiteNode = tempSiteNode.Parent;
                                    }
                                    var tempSiteId = tempSiteNode != null ? new Guid(tempSiteNode?.SPObjectId) : Guid.Empty;

                                    var tempTeamsNode = nodes[0];

                                    while (tempTeamsNode != null && tempTeamsNode.Level != (int)NodeLevel.Office365GroupEntire)
                                    {
                                        tempTeamsNode = tempTeamsNode.Parent;
                                    }
                                    var tempTeamsId = tempTeamsNode != null ? new Guid(tempTeamsNode?.TeamsId) : Guid.Empty;

                                    gsSetting = LoadSampleNodeParentSeting(nodes[0]?.Parent, tempSiteId, tempTeamsId);
                                }
                            }
                            else
                            {
                                if (tempSiteNode.Level != (int)NodeLevel.SiteCollection)
                                {
                                    while (tempSiteNode != null && tempSiteNode.Level != (int)NodeLevel.SiteCollection)
                                    {
                                        tempSiteNode = tempSiteNode.Parent;
                                    }

                                    var siteId = tempSiteNode != null ? new Guid(tempSiteNode?.SPObjectId) : Guid.Empty;
                                    gsSetting = LoadSampleNodeParentSeting(nodes[0]?.Parent, siteId, Guid.Empty);
                                }
                            }
                        }

                        var allSchedules = ScheduleService.GetScheduleByTypeServiceAsync(scheduleType).GetAwaiter().GetResult();
                        List<string> allSchedulesProfilesId = new List<string>();
                        if (allSchedules != null && allSchedules.Count != 0)
                        {
                            allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                        }

                        var allSettings = new Dictionary<string, RMArchiverSetting>();
                        var settings = ArchiverSettingDao.LoadArchiverSettings()
                            .Where(st => (SourceFlag)groupNode?.SourceType == SourceFlag.Teams
                                ? st.ContentSourceType == (int)ContentSourceType.Teams
                                : st.ContentSourceType != (int)ContentSourceType.Teams)
                            .OrderBy(item => item.SPObjectId);
                        foreach (var setting in settings)
                        {
                            var key = setting.SPObjectId.ToString() + setting.SiteId.ToString() + setting.TeamsId.ToString() + groupNode?.SPObjectId.ToString();
                            if (!allSettings.ContainsKey(key))
                            {
                                allSettings.Add(key, setting);
                            }
                        }
                        foreach (var node in nodes)
                        {
                            ArgumentCheck.NotNull(node, nameof(node));
                            var siteNode = node;
                            while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                            {
                                siteNode = siteNode.Parent;
                            }
                            var siteObjId = siteNode == null ? Guid.Empty.ToString() : siteNode.SPObjectId;

                            var teamsNode = node;
                            while (teamsNode != null && teamsNode.Level != (int)NodeLevel.Office365GroupEntire)
                            {
                                teamsNode = teamsNode.Parent;
                            }

                            var teamsId = teamsNode == null ? Guid.Empty.ToString() : teamsNode.TeamsId;

                            RMArchiverSetting csSetting = null;
                            var settingKey = node?.SPObjectId + siteObjId + teamsId + groupNode?.SPObjectId;
                            if (allSettings.TryGetValue(settingKey, out csSetting))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            var profileId = ScheduleService.GetProfileId(node);
                            if (!string.IsNullOrEmpty(profileId) && allSchedulesProfilesId.Contains(profileId))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            if (gsSetting != null)
                            {
                                node.IconStatus = IconStatus.Inhert;
                                continue;
                            }
                            node.IconStatus = IconStatus.NoSet;
                        }
                    }
                    else
                    {
                        foreach (var selfGroupNode in nodes)
                        {
                            var profileId = ScheduleService.GetProfileId(selfGroupNode);
                            var disposeSchedule = ScheduleService.GetScheduleAsync(profileId, scheduleType).GetAwaiter().GetResult();
                            var selfGSSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(selfGroupNode.SPObjectId), Guid.Empty, Guid.Empty
                            , (SourceFlag)groupNode?.SourceType == SourceFlag.Teams ? ContentSourceType.Teams : ContentSourceType.SharePoint);
                            if (selfGSSetting == null && disposeSchedule == null)
                            {
                                selfGroupNode.IconStatus = IconStatus.NoSet;
                            }
                            else
                            {
                                selfGroupNode.IconStatus = IconStatus.Break;
                            }

                            if (selfGroupNode.Children != null && selfGroupNode.Children.Any())
                            {
                                LoadArchiverSettingIcon(selfGroupNode.Children, scheduleType);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load ArchiverSetting Icon.Error:{0}", e.ToString());
                throw;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditArchiverInheritSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Inherit Parent Settings");
                var siteCollectionNode = GetSiteCollectionNode(node);
                var siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                var teamsNode = GetTeamsNode(node);
                var teamsId = teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty;
                ArchiverSettingDao.DeleteArchiverSettingByContentSourceType(new Guid(node.SPObjectId), siteId, teamsId, node.Type);
                if (node.DisposeScheduleInfo != null)
                {
                    ScheduleService.DeleteScheduleService(node.DisposeScheduleInfo.Id);
                }
                await CleanParentNodeSettingAsync(node);
                await ArchiverSettingDao.ForceSupportLockedSiteForBrokenChildNodesAsync(node);
                //Update the parent node setting to inherit settings. to do next.
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Inherit Parent Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.InheritSubNodeToCurrent, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> InheritSubNodeToCurrentAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Inherit SubNodetoCurrent Settings");

                bool isNodeExist = node.Level == (int)NodeLevel.Office365GroupEntire
                    ? RMRemoteNodeDao.CheckTeamsExistByTeamsId(node.TeamsId)
                    : RMRemoteNodeDao.CheckSiteExistBySiteId(node.Id);

                if (!isNodeExist)
                {
                    logger.Warn("Inherit SubNodetoCurrent Setting is not sp or one drive c or sc or teams");
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = I18NEntity.GetString("RM_SP_SO_NotcORscNodeId");
                    return result;
                }

                List<RMArchiverSetting> archiverSettings = null;

                var scheduleType = node.Type switch
                {
                    ContentSourceType.SharePoint => ScheduleType.SPArchiveJobSchedule,
                    ContentSourceType.Teams => ScheduleType.TeamsArchiveJobSchedule,
                    _ => ScheduleType.OneDriveArchiveJobSchedule
                };

                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    archiverSettings = ArchiverSettingDao.LoadArchiverSettingsUnderGroup(new Guid(node.Id), node.Type);
                    if (node.Type == ContentSourceType.Teams)
                    {
                        foreach (var archiverContainerSetting in archiverSettings)
                        {
                            if (archiverContainerSetting.TeamsId == Guid.Empty)
                            {
                                continue;
                            }
                            logger.Info($"DeleteArchiverSettin  {archiverContainerSetting.SPObjectId} => {archiverContainerSetting.TeamsId} => {archiverContainerSetting.SiteId}");
                            DeleteArchiverSetting(archiverContainerSetting.SPObjectId, archiverContainerSetting.TeamsId, archiverContainerSetting.SiteId);
                        }
                    }
                    else
                    {
                        foreach (var archiverContainerSetting in archiverSettings)
                        {
                            if (archiverContainerSetting.SiteId == Guid.Empty)
                            {
                                continue;
                            }
                            logger.Info($"DeleteArchiverSettin  {archiverContainerSetting.SPObjectId} => {archiverContainerSetting.SiteId}");
                            DeleteArchiverSetting(archiverContainerSetting.SPObjectId, archiverContainerSetting.SiteId);
                        }
                    }
                    var breakTreeNodes = RMScheduleDao.GetScheduleBreakNodes(node.Id.ToString());
                    foreach (var breakTreeNode in breakTreeNodes)
                    {
                        if (breakTreeNode == node.Id.ToString())
                        {
                            continue;
                        }
                        logger.Info($"breakTreeNode = node id  :  {breakTreeNode}  {node.Id}");
                        var schedulesInfo = await ScheduleService.GetAncestryScheduleAsync(breakTreeNode, scheduleType);
                        ScheduleService.DeleteScheduleService(schedulesInfo.Id);
                    }
                }
                else if (node.Type == ContentSourceType.Teams && node.Level == (int)NodeLevel.Office365GroupEntire)
                {
                    archiverSettings = ArchiverSettingDao.LoadArchiverSettingsUnderTeams(new Guid(node.TeamsId));
                    foreach (var archiverSiteSetting in archiverSettings)
                    {
                        if (archiverSiteSetting.SiteId == Guid.Empty && archiverSiteSetting.TeamsId == new Guid(node.TeamsId))
                        {
                            continue;
                        }
                        logger.Info($"DeleteArchiverSettin  {archiverSiteSetting.SPObjectId} => {archiverSiteSetting.TeamsId} => {archiverSiteSetting.SiteId}");
                        DeleteArchiverSetting(archiverSiteSetting.SPObjectId, archiverSiteSetting.TeamsId, archiverSiteSetting.SiteId);
                    }
                    var breakTreeNodes = RMScheduleDao.GetScheduleBreakNodes(node.TeamsId.ToString());
                    foreach (var breakTreeNode in breakTreeNodes)
                    {
                        if (breakTreeNode.Split('|').Length == 2 && breakTreeNode.Contains(node.TeamsId.ToString()))
                        {
                            continue;
                        }
                        logger.Info($"breakTreeNode = node id  :  {breakTreeNode}  {node.TeamsId}");
                        var schedulesInfo = await ScheduleService.GetAncestryScheduleAsync(breakTreeNode, scheduleType);
                        ScheduleService.DeleteScheduleService(schedulesInfo.Id);
                    }
                }
                else if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    archiverSettings = ArchiverSettingDao.LoadArchiverSettingsUnderSite(new Guid(node.Id), node.Type);
                    var breakTreeNodeLength = 2;
                    if (node.Type == ContentSourceType.Teams)
                    {
                        breakTreeNodeLength = 3;
                        foreach (var archiverSiteSetting in archiverSettings)
                        {
                            if (archiverSiteSetting.SPObjectId == archiverSiteSetting.SiteId)
                            {
                                continue;
                            }
                            logger.Info($"DeleteArchiverSettin  {archiverSiteSetting.SPObjectId} => {archiverSiteSetting.TeamsId} => {archiverSiteSetting.SiteId}");
                            DeleteArchiverSetting(archiverSiteSetting.SPObjectId, archiverSiteSetting.TeamsId, archiverSiteSetting.SiteId);
                        }
                    }
                    else
                    {
                        foreach (var archiverSiteSetting in archiverSettings)
                        {
                            if (archiverSiteSetting.SPObjectId == archiverSiteSetting.SiteId)
                            {
                                continue;
                            }
                            logger.Info($"DeleteArchiverSettin  {archiverSiteSetting.SPObjectId} => {archiverSiteSetting.SiteId}");
                            DeleteArchiverSetting(archiverSiteSetting.SPObjectId, archiverSiteSetting.SiteId);
                        }
                    }
                    var breakTreeNodes = RMScheduleDao.GetScheduleBreakNodes(node.Id.ToString());
                    foreach (var breakTreeNode in breakTreeNodes)
                    {
                        if (breakTreeNode.Split('|').Length == breakTreeNodeLength && breakTreeNode.Contains(node.Id.ToString()))
                        {
                            continue;
                        }
                        logger.Info($"breakTreeNode = node id  :  {breakTreeNode}  {node.Id}");
                        var schedulesInfo = await ScheduleService.GetAncestryScheduleAsync(breakTreeNode, scheduleType);
                        ScheduleService.DeleteScheduleService(schedulesInfo.Id);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Inherit SubNodetoCurrent Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }



        public void DeleteArchiverSetting(Guid ObjectId, Guid siteId)
        {
            ArchiverSettingDao.DeleteArchiverSetting(ObjectId, siteId);
        }

        public void DeleteArchiverSetting(Guid ObjectId, Guid teamsId, Guid siteId)
        {
            ArchiverSettingDao.DeleteArchiverSetting(ObjectId, teamsId, siteId);
        }
        private async System.Threading.Tasks.Task CleanParentNodeSettingAsync(RMSPTreeNode node)
        {
            do
            {
                if (await ArchiverSettingDao.CleanSettingJobTimeAsync(node))
                {
                    break;
                }
                node = node.Parent;
            }
            while (node != null);
        }
        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        public RMSPTreeNode GetTeamsNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.Office365GroupEntire)
            {
                node = node.Parent;
            }
            return node;
        }

        public RMArchiverSetting LoadSampleNodeParentSeting(RMSPSampleTreeNode node, Guid siteId, Guid teamsId)
        {
            RMArchiverSetting SPSetting = null;
            if (node == null)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.Office365GroupEntire)
            {
                siteId = Guid.Empty; // clear siteId for teams node
            }

            if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder || node.Level == (int)NodeLevel.Office365GroupEntire)
            {
                SPSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(node.SPObjectId), siteId, teamsId
                    , (SourceFlag)node.SourceType == SourceFlag.Teams ? ContentSourceType.Teams : ContentSourceType.SharePoint);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadSampleNodeParentSeting(node.Parent, siteId, teamsId);
            }

            return SPSetting;
        }
        public async Task<List<RMRuleInfos>> GetArchiverRuleListAsync(string containerId, SourceFlag sourceFlag)
        {
            List<RMRuleInfos> listRuleFromDA = new List<RMRuleInfos>();
            List<RMRuleInfos> availableRules = new List<RMRuleInfos>();
            try
            {
                logger.Info("Get Rules from DA ");
                using (PerformanceScope scope = new PerformanceScope("setting rules"))
                {
                    var securityGroupIds = SecurityTrimmingHelper.GetSecurityGroupsByContentScope(new List<string> { containerId }, sourceFlag);
                    var ruleContainerIds = SecurityTrimmingHelper.GetRuleScopeBySecurityGroupIds(securityGroupIds);
                    listRuleFromDA = await RuleManagerService.GetArchiverRulesByDataSourceAsync((int)sourceFlag, ruleContainerIds);
                    var associateAvailableRule = RuleManagerService.GetSimpleRulesFromDBAsync(ruleContainerIds).GetAwaiter().GetResult(); ;
                    var availableRuleIds = associateAvailableRule.Select(r => r.RuleId).ToList();
                    availableRules = listRuleFromDA.Where(r => availableRuleIds.Contains(r.RuleId)).ToList();
                }
                logger.Info("Rule count {0}", listRuleFromDA.Count);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return availableRules;
        }

        public RMSPTreeNode LoadSampleNodeSettings(RMSPSampleTreeNode sNode, ScheduleType scheduleType)
        {
            var configNode = new RMSPTreeNode();
            configNode.IconStatus = IconStatus.NoSet;
            #region copy node properties
            configNode.Id = sNode.Id;
            configNode.Name = sNode.Name;
            configNode.Title = sNode.Title;
            configNode.FullPath = sNode.FullPath;
            configNode.Level = sNode.Level;
            configNode.NodeType = sNode.NodeType;
            configNode.SPObjectId = sNode.SPObjectId;
            configNode.Expanded = sNode.Expanded;
            configNode.ChildrenCount = sNode.ChildrenCount;
            configNode.CheckNumber = sNode.CheckNumber;
            configNode.Hidden = sNode.Hidden;
            configNode.TeamsId = sNode.TeamsId;
            configNode.Type = (SourceFlag)sNode.SourceType switch
            {
                SourceFlag.Teams => ContentSourceType.Teams,
                SourceFlag.SharePoint => ContentSourceType.SharePoint,
                SourceFlag.OneDrive => ContentSourceType.OneDrive,
                _ => ContentSourceType.None,
            };
            #endregion

            try
            {
                RMSPSampleTreeNode groupNode = sNode;
                //TODO
                while (groupNode != null && groupNode.Level != (int)NodeLevel.WebApplication)
                {
                    groupNode = groupNode.Parent;
                }
                if (groupNode == null)
                {
                    return configNode;
                }
                //var groupNode = GetGroupNode(configNode);
                Guid groupId = Guid.Empty;
                string GlobalColumnName = string.Empty;
                string GlobalColumnNameDesc = string.Empty;
                if (groupNode != null && !string.IsNullOrEmpty(groupNode.SPObjectId))
                {
                    groupId = new Guid(groupNode.SPObjectId);
                }
                var GSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(groupId, Guid.Empty, Guid.Empty
                    , (SourceFlag)sNode?.SourceType == SourceFlag.Teams ? ContentSourceType.Teams : ContentSourceType.SharePoint);
                if (GSetting != null)
                {
                    configNode.IconStatus = IconStatus.Inhert;
                    //return configNode;
                    //if (sNode.Level == (int)NodeLevel.SiteCollection || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Folder)
                    //{
                    //    if (GSetting.EnableArchiverManagement != (int)EnableRecordManagementSetting.Enable)
                    //    {
                    //        configNode.EnableArchiverManagement = (int)EnableRecordManagementSetting.ParentDisable;
                    //    }
                    //    else
                    //    {
                    //        configNode.EnableArchiverManagement = (int)EnableRecordManagementSetting.Enable;
                    //    }
                    //}
                }

                RMSPSampleTreeNode teamsNode = sNode;
                while (teamsNode != null && teamsNode.Level != (int)NodeLevel.Office365GroupEntire)
                {
                    teamsNode = teamsNode.Parent;
                }

                Guid teamsId = Guid.Empty;
                if (teamsNode != null)
                {
                    teamsId = new Guid(teamsNode.TeamsId);
                }

                RMSPSampleTreeNode siteNode = sNode;
                while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                {
                    siteNode = siteNode.Parent;
                }

                Guid siteId = Guid.Empty;
                if (siteNode != null)
                {
                    siteId = new Guid(siteNode.SPObjectId);
                }
                RMArchiverSetting spSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(sNode.SPObjectId), siteId, teamsId
                    , (SourceFlag)sNode.SourceType == SourceFlag.Teams ? ContentSourceType.Teams : ContentSourceType.SharePoint);
                //if (configNode.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                //{
                //    var pNode = LoadFolderParentSeting(sNode, siteId);
                //    if (pNode != null && pNode.EnableArchiverManagement == (int)EnableRecordManagementSetting.ParentDisable)
                //    {
                //        if (spSetting != null)
                //        {
                //            spSetting.EnableArchiverManagement = (int)EnableRecordManagementSetting.ParentDisable;
                //        }
                //        folderDisable = true;
                //    }
                //}

                if (spSetting == null || spSetting.SPObjectId == Guid.Empty)
                {
                    if (spSetting != null)
                    {
                        if (spSetting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Disable)
                        {
                            configNode.EnableArchiverManagement = spSetting.EnableArchiverManagement;
                            configNode.DisposeScheduleInfo = null;
                            var tempCleanRestoredOption = string.IsNullOrEmpty(spSetting.CleanRestoredOption) ? null : SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(spSetting.CleanRestoredOption);
                            if (tempCleanRestoredOption != null)
                            {
                                configNode.CleanupAndDelRestoredType = tempCleanRestoredOption.CleanupAndDelRestoredType;
                                configNode.DayNum = tempCleanRestoredOption.DayNum;
                                configNode.EnableDelArchivedData = tempCleanRestoredOption.EnableDelArchivedData;
                                configNode.EnableCleanStubs = tempCleanRestoredOption.EnableCleanStubs;
                            }

                            configNode.Rules = null;
                            configNode.IconStatus = IconStatus.Break;
                            if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                            {
                                configNode.IsCustomSetting = true;
                            }
                            return configNode;
                        }
                    }
                    if ((sNode.Level == (int)NodeLevel.Office365GroupEntire || sNode.Level == (int)NodeLevel.SiteCollection || sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.Folder) && spSetting == null)
                    {
                        if (sNode.Level == (int)NodeLevel.SiteCollection)
                        {
                            siteId = Guid.Empty;
                        }
                        spSetting = LoadSampleNodeParentSeting(sNode.Parent, siteId, teamsId);
                        //if (spSetting != null && configNode.Level != (int)NodeLevel.WebApplication)
                        //{
                        //    if (spSetting.EnableArchiverManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                        //    {
                        //        spSetting.EnableArchiverManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        //    }
                        //}
                        if (spSetting != null && spSetting.SPObjectId != Guid.Empty)
                        {
                            configNode.Rules = EXOSettingRuleDao.GetArchiverMappingRules(spSetting.Id, (int)RuleType.Archiver);
                            configNode.IsEnableSuperUserDecrypt = spSetting.isEnableSuperUserDecrypt;
                            configNode.IsEnableRemoveRetentionLabel = spSetting.isEnableRemoveRetentionLabel;
                            configNode.SupportLockedSite = spSetting.SupportLockedSite;
                            configNode.SupportArchivedTeams = spSetting.SupportArchivedTeams;
                            configNode.IsManagedMetadataService = spSetting.isIncludeManagedMetadataService;
                            configNode.IsWorkflowDefinition = spSetting.isIncludeWorkflowDefinition;
                            configNode.EnableArchiverManagement = spSetting.EnableArchiverManagement;
                            var tempCleanRestoredOption = string.IsNullOrEmpty(spSetting.CleanRestoredOption) ? null : SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(spSetting.CleanRestoredOption);
                            if (tempCleanRestoredOption != null)
                            {
                                configNode.CleanupAndDelRestoredType = tempCleanRestoredOption.CleanupAndDelRestoredType;
                                configNode.DayNum = tempCleanRestoredOption.DayNum;
                                configNode.EnableDelArchivedData = tempCleanRestoredOption.EnableDelArchivedData;
                                configNode.EnableCleanStubs = tempCleanRestoredOption.EnableCleanStubs;
                            }
                            configNode.IconStatus = IconStatus.Inhert;
                        }
                        else
                        {
                            if (spSetting != null)
                            {
                                configNode.EnableArchiverManagement = spSetting.EnableArchiverManagement;
                            }
                            else
                            {
                                configNode.EnableArchiverManagement = (int)EnableRecordManagementSetting.Enable;
                            }
                            configNode.Rules = null;
                        }
                    }
                    else
                    {
                        if (spSetting != null)
                        {
                            configNode.EnableArchiverManagement = spSetting.EnableArchiverManagement;
                            var tempCleanRestoredOption = string.IsNullOrEmpty(spSetting.CleanRestoredOption) ? null : SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(spSetting.CleanRestoredOption);
                            if (tempCleanRestoredOption != null)
                            {
                                configNode.CleanupAndDelRestoredType = tempCleanRestoredOption.CleanupAndDelRestoredType;
                                configNode.DayNum = tempCleanRestoredOption.DayNum;
                                configNode.EnableDelArchivedData = tempCleanRestoredOption.EnableDelArchivedData;
                                configNode.EnableCleanStubs = tempCleanRestoredOption.EnableCleanStubs;
                            }
                            configNode.IconStatus = IconStatus.Break;
                            if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                            {
                                configNode.IsCustomSetting = true;
                            }
                        }
                        else
                        {
                            configNode.EnableArchiverManagement = (int)EnableRecordManagementSetting.Enable;
                        }
                        configNode.Rules = null;
                    }
                }
                else
                {
                    configNode.IconStatus = IconStatus.Break;
                    if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                    {
                        configNode.IsCustomSetting = true;
                    }
                    configNode.EnableArchiverManagement = spSetting.EnableArchiverManagement;
                    var tempCleanRestoredOption = string.IsNullOrEmpty(spSetting.CleanRestoredOption) ? null : SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(spSetting.CleanRestoredOption);
                    if (tempCleanRestoredOption != null)
                    {
                        configNode.CleanupAndDelRestoredType = tempCleanRestoredOption.CleanupAndDelRestoredType;
                        configNode.DayNum = tempCleanRestoredOption.DayNum;
                        configNode.EnableDelArchivedData = tempCleanRestoredOption.EnableDelArchivedData;
                        configNode.EnableCleanStubs = tempCleanRestoredOption.EnableCleanStubs;
                    }
                    configNode.Rules = EXOSettingRuleDao.GetArchiverMappingRules(spSetting.Id, (int)RuleType.Archiver);
                    configNode.IsEnableRemoveRetentionLabel = spSetting.isEnableRemoveRetentionLabel;
                    configNode.SupportLockedSite = spSetting.SupportLockedSite;
                    configNode.SupportArchivedTeams = spSetting.SupportArchivedTeams;
                    configNode.IsEnableSuperUserDecrypt = spSetting.isEnableSuperUserDecrypt;
                    configNode.IsManagedMetadataService = spSetting.isIncludeManagedMetadataService;
                    configNode.IsWorkflowDefinition = spSetting.isIncludeWorkflowDefinition;
                    configNode.EnableArchiverManagement = spSetting.EnableArchiverManagement;
                }

                var profileId = ScheduleService.GetProfileId(sNode);
                var disposeSchedule = ScheduleService.GetScheduleAsync(profileId, scheduleType).GetAwaiter().GetResult(); ;
                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                    //configNode.IsCustomSetting = true;
                    configNode.IconStatus = IconStatus.Break;
                    if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                    {
                        configNode.IsCustomSetting = true;
                    }
                    configNode.DisposeScheduleInfo = disposeSchedule;
                }
                else
                {
                    if (configNode.IsCustomSetting && configNode.Rules != null && configNode.Rules.Count > 0)
                    {
                        configNode.DisposeScheduleInfo = null;
                    }
                    else
                    {
                        var ancestryDisposeSchedule = ScheduleService.GetAncestryScheduleAsync(profileId, scheduleType).GetAwaiter().GetResult(); ;
                        if (ancestryDisposeSchedule != null)
                        {
                            var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                            ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                            ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                            configNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                            configNode.DisposeScheduleInfo.Id = "1";//Indicates that the schedule of the current node is inherited
                        }
                        else
                        {
                            configNode.DisposeScheduleInfo = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load ArchiverSetting.Error:{0}", e.ToString());
                throw;
            }
            return configNode;
        }

        public ArchiverSettingInfo LoadChannelSampleNodeSettings(Guid scopeId, string id)
        {
            ArchiverSettingInfo archiverSettingInfo = null;
            var spSetting = ArchiverSettingDao.LoadChannelArchiverSetting(scopeId, id);
            if (spSetting != null)
            {
                var archiverRuleInfos = EXOSettingRuleDao.GetArchiverMappingRules(spSetting.Id, (int)RuleType.Archiver);
                archiverSettingInfo = new ArchiverSettingInfo
                {
                    EnableArchiverManagement = spSetting.EnableArchiverManagement,
                    isIncludeManagedMetadataService = spSetting.isIncludeManagedMetadataService,
                    isEnableSuperUserDecrypt = spSetting.isEnableSuperUserDecrypt,
                    isEnableRemoveRetentionLabel = spSetting.isEnableRemoveRetentionLabel,
                    ArchiverRuleInfos = archiverRuleInfos.ConvertAll(item =>
                    {
                        return new ArchiverRuleInfo()
                        {
                            RuleId = item.RuleId,
                            RuleName = item.RuleName,
                            RuleOrder = item.RuleOrder,
                        };
                    })
                };
            }

            return archiverSettingInfo;
        }

        public void DisableSCArchiverManageMent(Guid siteId)
        {
            try
            {
                RemoteSiteCollection site = RMRemoteNodeDao.GetRemoteSiteCollectionByObjectId(siteId.ToString());
                if (site == null)
                {
                    throw new Exception($@"Unable found site by site id :{siteId}");
                }

                RMSPSampleTreeNode sampleNode = BuildSampleTreeNodeBySiteInfo(site);
                RMSPTreeNode nodeSetting = null;
                if (site.NodeType == RemoveNodeType.SkyDrivePro)
                {
                    nodeSetting = LoadSampleNodeSettings(sampleNode, ScheduleType.OneDriveArchiveJobSchedule);
                }
                else
                {
                    nodeSetting = LoadSampleNodeSettings(sampleNode, ScheduleType.SPArchiveJobSchedule);
                }
                if (nodeSetting == null)
                {
                    logger.Info($@"Unable found node setting by site id:{siteId}");
                    nodeSetting = BuildTreeNodeBySiteInfo(site);
                }

                nodeSetting.Parent = new RMSPTreeNode()
                {
                    SPObjectId = site.parentId,
                    Level = (int)NodeLevel.WebApplication
                };
                nodeSetting.EnableArchiverManagement = (int)EnableRecordManagementSetting.Disable;
                SaveGeneralSettingAsync(nodeSetting);
            }
            catch (Exception e)
            {
                logger.Error(@$"Fail disable SC archvie management for site:{siteId},ex :{e}");
                throw;
            }
        }

        private RMSPTreeNode BuildTreeNodeBySiteInfo(RemoteSiteCollection site)
        {
            return new RMSPTreeNode()
            {
                Type = site.NodeType == RemoveNodeType.SkyDrivePro ? ContentSourceType.OneDrive : ContentSourceType.SharePoint,
                SiteGroupId = new Guid(site.parentId),
                SPObjectId = site.id,
                FullPath = site.url,
                Level = (int)NodeLevel.SiteCollection,
                Id = site.id
            };
        }

        private RMSPSampleTreeNode BuildSampleTreeNodeBySiteInfo(RemoteSiteCollection site)
        {
            RMSPSampleTreeNode sampleNode = new RMSPSampleTreeNode();
            sampleNode.Name = site.url;
            sampleNode.Title = site.TemplateTitle;
            sampleNode.FullPath = site.url;
            sampleNode.Level = (int)NodeLevel.SiteCollection;
            sampleNode.NodeType = (int)site.NodeType;
            sampleNode.SPObjectId = site.id;
            sampleNode.Parent = new RMSPSampleTreeNode
            {
                Level = (int)NodeLevel.WebApplication,
                SPObjectId = site.parentId
            };
            return sampleNode;
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditArchiverSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveArchiverSettingAsync(RMSPTreeNode node)
        {
            //SiteCollection:100, Site:200, List:300, Folder:400
            var invalidRuleLevelMapping = new Dictionary<int, List<PolicyLevel>>
            {
                [100] = [PolicyLevel.Teams],
                [200] = [PolicyLevel.Teams, PolicyLevel.SiteCollection],
                [300] = [PolicyLevel.Teams, PolicyLevel.SiteCollection, PolicyLevel.Site],
                [400] = [PolicyLevel.Teams, PolicyLevel.SiteCollection, PolicyLevel.Site, PolicyLevel.List],
            };

            if (invalidRuleLevelMapping.TryGetValue(node.Level, out var inValidLevels))
            {
                var invalidRuleIds = RMRuleDao.GetTeamsArchiverRuleIdsByLevels(inValidLevels);
                logger.Info($"Start validate rule level. Node level :{node.Level}, invalidRuleIds count: {invalidRuleIds.Count}");
                if (node.Rules.Any(r => invalidRuleIds.Contains(r.RuleId)))
                {
                    var failResult = new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.RuleLevelNotMatchNodeLevel,
                        ErrorMessage = I18NEntity.GetString("RM_AR_SPS_ArchiverSetting_SaveError")
                    };
                    return failResult;
                }
            }
            return await ArchiverSettingDao.SaveArchiverSettingAsync(node);
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ArchiverGeneralSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<RAReturnMessage> SaveGeneralSettingAsync(RMSPTreeNode node)
        {
            if (node.DisposeScheduleInfo != null && node.EnableArchiverManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteScheduleService(node.DisposeScheduleInfo.Id);
            }
            return ArchiverSettingDao.SaveOrUpdateGeneralSettingAsync(node);
        }
        public HSMArchiverResult RunHSMArchiverJob(HSMArchiverDto hsmDto, JobRunBy jobRunBy)
        {
            logger.Debug("start HSM archiver backup job");
            string id = string.Empty;
            HSMArchiverResult result = new HSMArchiverResult() { IsSuccessStartJob = true };
            try
            {
                var indexDevice = StorageDeviceService.GetIndexDevice();
                if (indexDevice == null)
                {
                    result.IsSuccessStartJob = false;
                    return result;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                //var loginName = TenantLocalValue.LogonUserEmail;
                var jobId = this.RMJobService.GenerateJobId(JobType.ArchiverByHSMXml);
                hsmDto.MainJobId = jobId;
                result.MainJobId = jobId;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ArchiverByHSMXml,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(hsmDto)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    result.IsSuccessStartJob = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunHSMArchiverJob,ERROR:{0}", ex.ToString());
            }

            return result;
        }
        public RAReturnMessage RunArchiverJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start archiver backup job");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                try
                {

                    var selectedTreeNodeId = new Guid(selectedTree.SPObjectId);
                    var selectedTreeSiteNode = selectedTree.GetSiteCollectionNode();
                    var selectedTreeSiteNodeId = selectedTreeSiteNode != null ? new Guid(selectedTreeSiteNode.SPObjectId) : Guid.Empty;
                    RMArchiverSetting spSetting = ArchiverSettingDao.LoadArchiverSetting(selectedTreeNodeId, selectedTreeSiteNodeId);
                    bool ruleExist = CheckRuleExist(selectedTree);
                    if (!ruleExist)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_SO_RunJobFailed_NoRule");
                        return msg;
                    }
                    if (spSetting == null || spSetting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Disable)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        logger.Info($"current node is disabled. id:{selectedTree.SPObjectId}");
                        return msg;
                    }
                    if (selectedTree.UserArchiverImportFile && selectedTree.ArchiverImportSitesUrl == null)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError");
                        logger.Info("run job by archiver import sites url failed.");
                        return msg;
                    }
                }
                catch (System.Exception e)
                {
                    logger.Warn($"Get settings error: {e}");
                }

                var indexDevice = StorageDeviceService.GetIndexDevice();
                if (indexDevice == null)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_AR_RunJob_Failed_NoIndexDeviceSetting");
                    return msg;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                //var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.RMArchiverBackup,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public RAReturnMessage RunTeamsArchiverJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start teams archiver backup job");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                try
                {
                    var selectedTreeNodeId = new Guid(selectedTree.SPObjectId);
                    var selectedTreeTeamsNode = selectedTree.GetTeamsNode();
                    var selectedTreeTeamsNodeId = selectedTreeTeamsNode != null ? new Guid(selectedTreeTeamsNode.TeamsId) : Guid.Empty;
                    var selectedTreeSiteNode = selectedTree.GetSiteCollectionNode();
                    var selectedTreeSiteNodeId = selectedTreeSiteNode != null ? new Guid(selectedTreeSiteNode.SPObjectId) : Guid.Empty;
                    RMArchiverSetting spSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(selectedTreeNodeId, selectedTreeSiteNodeId, selectedTreeTeamsNodeId, ContentSourceType.Teams);
                    bool ruleExist = CheckRuleExist(selectedTree);
                    if (!ruleExist)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_SO_RunJobFailed_NoRule");
                        return msg;
                    }
                    if (spSetting == null || spSetting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Disable)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        logger.Info($"current node is disabled. id:{selectedTree.SPObjectId}");
                        return msg;
                    }
                    if (selectedTree.UserArchiverImportFile && selectedTree.ArchiverImportSitesUrl == null)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError");
                        logger.Info("run job by archiver import sites url failed.");
                        return msg;
                    }
                }
                catch (System.Exception e)
                {
                    logger.Warn($"Get settings error: {e}");
                }

                var indexDevice = StorageDeviceService.GetIndexDevice();
                if (indexDevice == null)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_AR_RunJob_Failed_NoIndexDeviceSetting");
                    return msg;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsArchiverBackup,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        private bool CheckRuleExist(RMSPTreeNode node)
        {
            try
            {
                if (node.Rules == null || node.Rules.Count == 0)
                {
                    logger.Warn($"rules is null or count is 0,so cannot run job");
                    return false;
                }
                List<Guid> ruleIds = node.Rules.Select(r => r.RuleId).ToList();
                var rule = RMRuleDao.GetRulesByIds(ruleIds);
                if (rule == null || rule.Count == 0)
                {
                    logger.Warn($"the rule has remove from rule setting");
                    return false;
                }
                else
                {
                    List<RMSimpleRule> tempNodeRules = new List<RMSimpleRule>();
                    List<Guid> dbRuleIds = rule.Select(r => r.RuleId).ToList();
                    foreach (var nodeRule in node.Rules)
                    {
                        if (dbRuleIds.Contains(nodeRule.RuleId))
                        {
                            tempNodeRules.Add(nodeRule);
                        }
                    }
                    node.Rules = tempNodeRules;
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Error($"check rule exsit failed,error:{e}");
                return true;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunODPreScanJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public RAReturnMessage RunODPreScanJobWrapper(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            return RunSOPreScanJob(selectedTree, jobRunBy);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunSOPreScanJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public RAReturnMessage RunSOPreScanJobWrapper(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            return RunSOPreScanJob(selectedTree, jobRunBy);
        }

        private RAReturnMessage RunSOPreScanJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start SO Pre-Scan job");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                try
                {
                    var selectedTreeNodeId = new Guid(selectedTree.SPObjectId);
                    var selectedTreeSiteNode = selectedTree.GetSiteCollectionNode();
                    var selectedTreeSiteNodeId = selectedTreeSiteNode != null ? new Guid(selectedTreeSiteNode.SPObjectId) : Guid.Empty;
                    RMArchiverSetting spSetting = ArchiverSettingDao.LoadArchiverSetting(selectedTreeNodeId, selectedTreeSiteNodeId);
                    bool ruleExist = CheckRuleExist(selectedTree);
                    if (!ruleExist)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_SO_RunJobFailed_NoRule");
                        return msg;
                    }
                    if (spSetting == null || spSetting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Disable)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        logger.Info($"current node is disabled. id:{selectedTree.SPObjectId}");
                        return msg;
                    }
                }
                catch (System.Exception e)
                {
                    logger.Warn($"Get settings error: {e}");
                }

                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                var jobId = RMJobService.GenerateJobId(JobType.SOPreScan);
                msg.Extension = jobId;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SOPreScan,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public RAReturnMessage RunTeamsPreScanJobWrapper(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            return RunTeamsPreScanJob(selectedTree, jobRunBy);
        }

        private RAReturnMessage RunTeamsPreScanJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("Start Teams Pre-Scan job");
            RAReturnMessage msg = new();
            try
            {
                if (selectedTree == null) throw new Exception("Selected tree is null.");
                try
                {
                    var selectedTreeNodeId = new Guid(selectedTree.SPObjectId);
                    var selectedTreeTeamsNode = selectedTree.GetTeamsNode();
                    var selectedTreeTeamsNodeId = selectedTreeTeamsNode != null ? new Guid(selectedTreeTeamsNode.TeamsId) : Guid.Empty;
                    var selectedTreeSiteNode = selectedTree.GetSiteCollectionNode();
                    var selectedTreeSiteNodeId = selectedTreeSiteNode != null ? new Guid(selectedTreeSiteNode.SPObjectId) : Guid.Empty;
                    var teamsSetting = ArchiverSettingDao.LoadTeamsArchiverSetting(selectedTreeNodeId, selectedTreeSiteNodeId, selectedTreeTeamsNodeId);

                    bool ruleExist = CheckRuleExist(selectedTree);
                    if (!ruleExist)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_SO_RunJobFailed_NoRule");
                        return msg;
                    }
                    if (teamsSetting == null || teamsSetting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Disable)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        logger.Info($"current node is disabled. id:{selectedTree.SPObjectId}");
                        return msg;
                    }
                }
                catch (Exception e)
                {
                    msg.MessageType = RAMessageType.Failed;
                    logger.Warn($"Get settings error: {e}");
                    throw;
                }

                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new()
                {
                    JobType = JobType.TeamsPreScan,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                var id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                msg.MessageType = RAMessageType.Failed;
                logger.Error("error occurred while RunTeamsPreScanJob,ERROR:{0}", ex);
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunTeamsPreScanJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunTeamsPreScanJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.TeamsPreScan;
            RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
            var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            return RealRunTeamsPreScanOnSelectedNode(loginName, jobType, selectedNode);
        }

        public string RealRunTeamsPreScanOnSelectedNode(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            try
            {
                List<JobType> types = new List<JobType> { JobType.TeamsPreScan };
                string nodeUrl = selectedNode.FullPath;
                if (selectedNode.Level == (int)NodeLevel.Folder && !nodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var siteNode = selectedNode.GetSiteCollectionNode();
                    if (siteNode != null)
                    {
                        nodeUrl = WebUtil.MakeFullUrl(siteNode.FullPath, selectedNode.FullPath);
                    }
                }
                logger.Info($"Start Creating subjobs of TeamsPreScan");

                if (RMJobService.HasRunningArchiverJobOnScope(types, nodeUrl) || RMJobService.HasStoppingArchiverJobOnScope(types, nodeUrl))
                {
                    logger.Warn($"Current has job running on same scope.{nodeUrl}");
                    jobId = RMJobService.CreateJobWithScopeId(JobType.TeamsPreScan, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                string containerId = GetSPContainerId(selectedNode);
                jobId = RMJobService.CreateJobWithScopeId(JobType.TeamsPreScan, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                int estimatedSiteCount = GetEstimatedSiteCount(selectedNode, containerId);
                if (estimatedSiteCount > 0)
                {
                    SubJobDao.UpdateSubJobCount(jobId, estimatedSiteCount);
                    RMJobService.SetSumSCCountOfJobExtension(estimatedSiteCount, jobId);
                    logger.Info("Initialize main job {0} sub job count by selected node level {1}, estimated site count {2}.", jobId, selectedNode.Level, estimatedSiteCount);
                }
                else
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                    logger.Info($"No available sc to run ,jobId:{jobId}");
                    return jobId;
                }

                try
                {
                    RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetTeamsArchiverJobRuleIds(selectedNode));
                }
                catch (Exception ex)
                {
                    logger.Error($"AddJobRuleMappings failed ,jobId:{jobId},error:{ex}");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                    return jobId;
                }

                if (IsTrailLicenceAndExceedSizeLimit())
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
                    return jobId;
                }

                CreateTeamsPreScanSubJobsByStream(
                    jobId,
                    jobType,
                    selectedNode,
                    estimatedSiteCount
                );
                logger.Info($"Finish Creating subjobs of TeamsPreScan, JobId is {jobId}");

                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("RealRunTeamsPreScanOnSelectedNode failed, jobId:{0}, error:{1}", jobId, ex.ToString());
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                }

                throw;
            }
        }
        /// <summary>
        /// Streaming, paged, DAO-filter-pushdown subjob creation for TeamsPreScan (analogous to TeamsArchiverBackup)
        /// </summary>
        private void CreateTeamsPreScanSubJobsByStream(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            int estimatedSiteCount)
        {
            int totalCount = 0;
            int conflictFilterPassedCount = 0;
            int currentSubjobIndex = 0;
            int subJobIndexDigits = GetSubJobIndexDigits(estimatedSiteCount);
            var runningScopes = new HashSet<string>(RMJobService.GetRunningArchiverJobsScopes([JobType.TeamsPreScan]) ?? new List<string>());
            var conflictFilterBatch = new List<RMSPTreeNode>(DisposalBrowsePageSize);
            var pendingSubJobs = new List<RMSubJob>(SubJobBulkInsertBatchSize);

            foreach (var node in EnumerateTeamsPreScanRunnableNodeStream(selectedNode))
            {
                totalCount++;
                conflictFilterBatch.Add(node);
                if (conflictFilterBatch.Count < DisposalBrowsePageSize)
                {
                    continue;
                }
                else if (CheckWhetherJobShouldStop(jobId))
                {
                    return;
                }

                conflictFilterPassedCount += AppendTeamsPreScanSubJobsFromBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    estimatedSiteCount,
                    subJobIndexDigits,
                    runningScopes,
                    conflictFilterBatch,
                    ref currentSubjobIndex,
                    pendingSubJobs
                );
                conflictFilterBatch.Clear();
            }

            if (conflictFilterBatch.Count > 0)
            {
                conflictFilterPassedCount += AppendTeamsPreScanSubJobsFromBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    estimatedSiteCount,
                    subJobIndexDigits,
                    runningScopes,
                    conflictFilterBatch,
                    ref currentSubjobIndex,
                    pendingSubJobs
                );
                conflictFilterBatch.Clear();
            }

            if (pendingSubJobs.Count > 0)
            {
                SubJobDao.BulkCreateJobs(pendingSubJobs, SubJobBulkInsertBatchSize);
                pendingSubJobs.Clear();
            }

            if (CheckWhetherJobShouldStop(jobId))
            {
                return;
            }
            if (totalCount == 0)
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoTeams");
                return;
            }

            if (conflictFilterPassedCount == 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return;
            }

            SubJobDao.UpdateSubJobCount(jobId, conflictFilterPassedCount);
            RMJobService.SetSumSCCountOfJobExtension(conflictFilterPassedCount, jobId);
            logger.Info("all teams pre-scan sub jobs were created correctlly, jobId is {0}, total count is {1}", jobId, conflictFilterPassedCount);
            var subJobWeight = 100d / conflictFilterPassedCount;
            if (!SubJobDao.UpdateSubJobWeightByParentId(jobId, subJobWeight))
            {
                logger.Warn("Failed to update teams pre-scan sub job weights in batch, jobId:{0}, targetWeight:{1}", jobId, subJobWeight);
            }
        }

        private IEnumerable<RMSPTreeNode> EnumerateTeamsPreScanRunnableNodeStream(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level != (int)NodeLevel.Office365GroupEntire && selectedNode.Level != (int)NodeLevel.WebApplication)
            {
                foreach (var node in EnumerateTeamsDisposalRunnableNodeStream(selectedNode))
                {
                    yield return node;
                }

                yield break;
            }

            var groupNode = selectedNode.GetGroupNode();
            if (groupNode == null || !Guid.TryParse(groupNode.SPObjectId, out var groupId))
            {
                logger.Warn("Skip teams pre-scan subjob creation because the container id is invalid. Node:{0}", selectedNode.FullPath);
                yield break;
            }

            var settings = ArchiverSettingDao.LoadArchiverSettingsUnderGroup(groupId, ContentSourceType.Teams);
            var teamsLevelSettingIds = GetTeamsLevelArchiverSettingIds(settings);
            var teamSettingsByTeamsId = BuildTeamsLevelSettingsByTeamId(settings);
            var uniqueSiteSettingIdsByTeamId = BuildUniqueSiteSettingIdsByTeamId(settings);
            var hasContainerTeamsLevelRule = HasContainerTeamsLevelRule(selectedNode, teamsLevelSettingIds, settings);
            var teamNodes = selectedNode.Level == (int)NodeLevel.WebApplication
                ? GetPagedTeamsSiteCollections(selectedNode)
                : EnumerateSelectedTeamsNode(selectedNode);
            var pendingSiteLevelTeams = new List<RMSPTreeNode>(TeamsSiteLookupBatchSize);

            foreach (var teamNode in teamNodes)
            {
                if (!Guid.TryParse(teamNode.TeamsId, out var teamsId))
                {
                    logger.Warn("Skip teams due to invalid teams id, name:{0}, teamsId:{1}", teamNode.Name, teamNode.TeamsId);
                    continue;
                }

                teamNode.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                teamNode.SupportLockedSite = selectedNode.SupportLockedSite;
                teamNode.SupportArchivedTeams = selectedNode.SupportArchivedTeams;

                if (HasTeamsLevelArchiverRule(teamsId, teamSettingsByTeamsId, teamsLevelSettingIds, hasContainerTeamsLevelRule))
                {
                    yield return teamNode;
                    continue;
                }
                
                pendingSiteLevelTeams.Add(teamNode);
                if (pendingSiteLevelTeams.Count < TeamsSiteLookupBatchSize)
                {
                    continue;
                }

                foreach (var siteNode in EnumerateTeamsArchiverSiteNodesByBatch(
                                pendingSiteLevelTeams,
                                uniqueSiteSettingIdsByTeamId,
                                selectedNode.UserArchiverImportFile))
                {
                    yield return siteNode;
                }

                pendingSiteLevelTeams.Clear();
            }

            if (pendingSiteLevelTeams.Count > 0)
            {
                foreach (var siteNode in EnumerateTeamsArchiverSiteNodesByBatch(
                             pendingSiteLevelTeams,
                             uniqueSiteSettingIdsByTeamId,
                             selectedNode.UserArchiverImportFile))
                {
                    yield return siteNode;
                }
            }
        }

        /// <summary>
        /// Append subjobs for TeamsPreScan from a batch (analogous to TeamsArchiverBackup)
        /// </summary>
        private int AppendTeamsPreScanSubJobsFromBatch(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            int subJobCount,
            int subJobIndexDigits,
            HashSet<string> runningScopes,
            List<RMSPTreeNode> batch,
            ref int currentSubjobIndex,
            List<RMSubJob> pendingSubJobs)
        {
            var filteredNodes = FilterTeamsPreScanConflictBatch(batch, selectedNode, runningScopes);
            int count = 0;
            foreach (var filteredNode in filteredNodes)
            {
                var subJobNodes = new List<RMSPTreeNode>(1) { filteredNode };
                var subJob = BuildSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, subJobIndexDigits, subJobNodes, false, filteredNode.FullPath, filteredNode.O365TenantId);
                pendingSubJobs.Add(subJob);
                if (pendingSubJobs.Count >= SubJobBulkInsertBatchSize)
                {
                    SubJobDao.BulkCreateJobs(pendingSubJobs, SubJobBulkInsertBatchSize);
                    pendingSubJobs.Clear();
                }

                currentSubjobIndex++;
                count++;
            }

            return count;
        }

        private List<RMSPTreeNode> FilterTeamsPreScanConflictBatch(
            List<RMSPTreeNode> nodes,
            RMSPTreeNode selectedNode,
            HashSet<string> runningScopes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new List<RMSPTreeNode>();
            }

            if (runningScopes == null || runningScopes.Count == 0)
            {
                return nodes.ToList();
            }

            if (selectedNode.Level != (int)NodeLevel.WebApplication
                && nodes.Count == 1
                && string.Equals(nodes[0].FullPath, selectedNode.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                return nodes.ToList();
            }

            return nodes.Where(node => !runningScopes.Contains(node.Name)).ToList();
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunArchiverBackupJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            bool hasSoLicense = LicenseHelperService.HasOpusSOLicense;
            if (!hasSoLicense)
            {
                logger.Error("this user has no so license,cannot run job");
                return "HasNoSoLicense";
            }
            JobType jobType = JobType.RMArchiverBackup;
            RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
            var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            return RealRunArchiverBackupJobOnSelectedNode(loginName, jobType, selectedNode);

        }

        public string RealRunArchiverBackupJobOnSelectedNode(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            try
            {
                List<string> archiverImportSitesUrl = selectedNode.ArchiverImportSitesUrl;
                bool useArchiverImportFile = selectedNode.UserArchiverImportFile;
                selectedNode.ArchiverImportSitesUrl = new List<string>();
                string nodeUrl = selectedNode.FullPath;
                string folderFullPath = string.Empty;
                if (selectedNode.Level == (int)NodeLevel.Folder && !nodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var siteNode = selectedNode.GetSiteCollectionNode();
                    if (siteNode != null)
                    {
                        nodeUrl = WebUtil.MakeFullUrl(siteNode.FullPath, selectedNode.FullPath);
                        folderFullPath = nodeUrl;
                    }
                    selectedNode.FullUrl = nodeUrl;
                }

                bool onlyHasVersionDeletionRule = OnlyHasVersionDeletionRule(nodeUrl, selectedNode);
                if (onlyHasVersionDeletionRule)
                {
                    logger.Warn($"Current Node is version deletion rule .");
                }
                else
                {
                    List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
                    var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
                    if (mIndexJobs.Count > 0)
                    {
                        //has move index job, need skip.
                        logger.Warn("so Current has move index or retention job running.");
                        jobId = RMJobService.CreateJobWithScopeId(JobType.RMArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }

                var shouldCheckConflictTypes = (onlyHasVersionDeletionRule
                    ? JobTypeConstants.ArchiveSiteConflictType.Where(t => t != JobType.ArchiverRetention)
                    : JobTypeConstants.ArchiveSiteConflictType).ToList();

                if (useArchiverImportFile && archiverImportSitesUrl != null && archiverImportSitesUrl.Count > 0)
                {
                    archiverImportSitesUrl = RMJobService.FilterRunnableSOJobSitesInContainerForImportedSites(GetSPContainerId(selectedNode), archiverImportSitesUrl);
                    if (archiverImportSitesUrl.Count == 0)
                    {
                        logger.Warn("All import site urls conflict with running or stopping jobs.");
                        jobId = RMJobService.CreateJobWithScopeId(JobType.RMArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
                else
                {
                    bool hasRunningHierarchyScopeConflict = false;
                    if (selectedNode.Level >= (int)NodeLevel.SiteCollection)
                    {
                        string siteCollectionScope = selectedNode.GetSiteCollectionNode()?.FullPath;
                        if (string.IsNullOrWhiteSpace(siteCollectionScope))
                        {
                            siteCollectionScope = nodeUrl;
                        }

                        var runningScopes = RMJobService.GetRunningArchiverJobSiteUrl(shouldCheckConflictTypes, new List<string> { siteCollectionScope });
                        hasRunningHierarchyScopeConflict = runningScopes.Any(scope =>
                            !string.IsNullOrWhiteSpace(scope)
                            && (RuleSPTreeUtil.IsPrefixWithSlash(nodeUrl, scope)
                                || RuleSPTreeUtil.IsPrefixWithSlash(scope, nodeUrl)));
                    }

                    if (hasRunningHierarchyScopeConflict || RMJobService.HasRunningArchiverJobOnScope(shouldCheckConflictTypes, nodeUrl))
                    {
                        logger.Warn($"Current has job running or stopping on same scope.{nodeUrl}");
                        jobId = RMJobService.CreateJobWithScopeId(JobType.RMArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }

                var tempExtension = string.Empty;
                if (useArchiverImportFile && archiverImportSitesUrl != null && archiverImportSitesUrl.Count > 0)
                {
                    tempExtension = RuleSPTreeUtil.GenerateArchiveJobMonitorExtension(selectedNode, TreeMode.SO, archiverImportSitesUrl, useArchiverImportFile);
                }
                else
                {
                    tempExtension = RuleSPTreeUtil.GenerateArchiveJobMonitorExtension(selectedNode, TreeMode.SO);
                }
                string containerId = GetSPContainerId(selectedNode);
                jobId = RMJobService.CreateJobWithScopeId(JobType.RMArchiverBackup, jobRunByUser, nodeUrl, containerId, null, tempExtension);
                int estimatedSiteCount = GetEstimatedSiteCount(selectedNode, containerId, useArchiverImportFile, archiverImportSitesUrl);
                if (estimatedSiteCount > 0)
                {
                    SubJobDao.UpdateSubJobCount(jobId, estimatedSiteCount);
                    RMJobService.SetSumSCCountOfJobExtension(estimatedSiteCount, jobId);
                    logger.Info("Initialize main job {0} sub job count by selected node level {1}, estimated site count {2}.", jobId, selectedNode.Level, estimatedSiteCount);
                }
                else
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                    logger.Info($"No available sc to run ,jobId:{jobId}");
                    return jobId;
                }

                try
                {
                    RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetAppliedRuleIds(selectedNode));
                }
                catch (Exception ex)
                {
                    logger.Error($"AddJobRuleMappings failed ,jobId:{jobId},error:{ex}");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                    return jobId;
                }

                if (IsTrailLicenceAndExceedSizeLimit())
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
                    return jobId;
                }
                logger.Info($"Start Creating subjobs by stream, JobId is {jobId}");

                UpdateJobVersion(jobId, jobType);
                CreateSubJobsByStream(
                    jobId,
                    jobType,
                    selectedNode,
                    shouldCheckConflictTypes,
                    archiverImportSitesUrl,
                    useArchiverImportFile,
                    folderFullPath,
                    estimatedSiteCount);
                logger.Info($"Finish Creating subjobs by stream, JobId is {jobId}");
                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("RealRunArchiverBackupJobOnSelectedNode failed, jobId:{0}, error:{1}", jobId, ex.ToString());
                if (!string.IsNullOrWhiteSpace(jobId) && !string.Equals(jobId, "HasNoSoLicense", StringComparison.OrdinalIgnoreCase))
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                }

                throw;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunArchiverBackupJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunTeamsArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            bool hasSoLicense = LicenseHelperService.HasOpusSOLicense;
            if (!hasSoLicense)
            {
                logger.Error("this user has no so license,cannot run job");
                return "HasNoSoLicense";
            }
            JobType jobType = JobType.TeamsArchiverBackup;
            RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
            var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            return RealRunTeamsArchiverBackupJobOnSelectedNode(loginName, jobType, selectedNode);
        }

        public string RealRunTeamsArchiverBackupJobOnSelectedNode(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            try
            {
                List<JobType> types = JobTypeConstants.ArchiveTeamsConflictType;
                string teamsUrl = selectedNode.GetTeamsNode()?.DisplayName ?? (RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.GetTeamsNode()?.SPObjectId).Item1?.url ?? string.Empty);
                string nodeFullPath = selectedNode.Level == (int)NodeLevel.Office365GroupEntire ? selectedNode.DisplayName ?? teamsUrl : selectedNode.FullPath;
                string nodeUrl = selectedNode.FullPath;
                bool useArchiverImportFile = selectedNode.UserArchiverImportFile;
                List<string> archiverImportSitesUrl = selectedNode.ArchiverImportSitesUrl;
                if (selectedNode.Level == (int)NodeLevel.Folder && !nodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var siteNode = selectedNode.GetSiteCollectionNode();
                    if (siteNode != null)
                    {
                        nodeUrl = WebUtil.MakeFullUrl(siteNode.FullPath, selectedNode.FullPath);
                        nodeFullPath = nodeUrl;
                    }
                }
                logger.Info($"Start Creating subjobs of TeamsArchiverBackup");

                bool onlyHasVersionDeletionRule = TeamsOnlyHasVersionDeletionRule(nodeUrl, selectedNode);
                if (onlyHasVersionDeletionRule)
                {
                    logger.Warn($"teams Current Node is version deletion rule and can run SO job normally.");
                }
                else
                {
                    List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
                    var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
                    if (mIndexJobs.Count > 0)
                    {
                        logger.Warn("teams Current has move index or retention job running.");
                        jobId = RMJobService.CreateJobWithScopeId(JobType.TeamsArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
                var shouldCheckConflictJobTypes = onlyHasVersionDeletionRule ?
                    JobTypeConstants.ArchiveTeamsConflictType.Where(type => type != JobType.TeamsArchiverRetention && type != JobType.ArchiverRetention).ToList() :
                    JobTypeConstants.ArchiveTeamsConflictType;

                if (useArchiverImportFile && archiverImportSitesUrl != null && archiverImportSitesUrl.Count > 0)
                {
                    var importTeams = archiverImportSitesUrl
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var runningTeams = RMJobService.GetRunningTeamsArchiverJobSiteUrl(
                        shouldCheckConflictJobTypes,
                        RuleSPTreeUtil.CheckNeedLoadRuningSCUrlBySelectNode(selectedNode, useArchiverImportFile),
                        importTeams.ToDictionary(team => team, team => new List<string>(), StringComparer.OrdinalIgnoreCase));

                    var runningTeamSet = new HashSet<string>(runningTeams.Keys, StringComparer.OrdinalIgnoreCase);
                    archiverImportSitesUrl = importTeams.Where(team => !runningTeamSet.Contains(team)).ToList();

                    if (archiverImportSitesUrl.Count == 0)
                    {
                        logger.Warn("All import teams conflict with running or stopping jobs.");
                        jobId = RMJobService.CreateJobWithScopeId(JobType.TeamsArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
                else
                {
                    bool hasRunningHierarchyScopeConflict = false;
                    bool hasRunningScopeConflict = false;
                    if (selectedNode.Level  != (int)NodeLevel.WebApplication)
                    {
                        var filterTeamAncSCDic = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                        var teamNode = selectedNode.GetTeamsNode();
                        var teamName = teamNode?.Name;
                        var siteCollectionUrl = "";
                        if (!string.IsNullOrWhiteSpace(teamName))
                        {
                            siteCollectionUrl = selectedNode.GetSiteCollectionNode()?.FullPath;
                            filterTeamAncSCDic[teamName] = string.IsNullOrWhiteSpace(siteCollectionUrl)
                                ? new List<string>()
                                : new List<string> { siteCollectionUrl };
                        }

                        var runningTeams = RMJobService.GetRunningTeamsArchiverJobSiteUrl(
                            shouldCheckConflictJobTypes,
                            RuleSPTreeUtil.CheckNeedLoadRuningSCUrlBySelectNode(selectedNode, useArchiverImportFile),
                            filterTeamAncSCDic);

                        var runningNames = (runningTeams ?? new Dictionary<string, List<string>>())
                            .SelectMany(kv => new[] { kv.Key }.Concat(kv.Value ?? new List<string>()))
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        var currentScopes = new List<string> { nodeUrl, nodeFullPath }
                            .Where(scope => !string.IsNullOrWhiteSpace(scope))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        hasRunningHierarchyScopeConflict = runningNames.Any(runningScope =>
                        {
                            bool isHierarchyRelated = RuleSPTreeUtil.IsPrefixWithSlash(runningScope, nodeFullPath)
                                                || RuleSPTreeUtil.IsPrefixWithSlash(nodeFullPath, runningScope);
                            bool isExactMatch = currentScopes.Contains(runningScope, StringComparer.OrdinalIgnoreCase);

                            return isHierarchyRelated || isExactMatch;
                        });
                        // Add conflict scenarios for Teams‑job splitting by site.
                        if(selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
                        {
                            var parentNode = selectedNode.Parent;
                            var containerFullPath = parentNode?.FullPath;
                            if (!string.IsNullOrWhiteSpace(containerFullPath))
                            {
                                var siteCollectionNode = selectedNode.GetSiteCollectionNode();
                                var teamsNode = selectedNode.GetTeamsNode();
                                var currentSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(
                                    new Guid(selectedNode.SPObjectId),
                                    siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty,
                                    teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty,
                                    ContentSourceType.Teams);

                                if (currentSetting != null && currentSetting.CreateTime > 0)
                                {
                                    var runningContainerJobs = JMDao.HasRunningArchiverJob(shouldCheckConflictJobTypes)
                                        .Where(job => job != null
                                            && job.ScopeId != null
                                            && string.Equals(job.ScopeId, containerFullPath, StringComparison.OrdinalIgnoreCase))
                                        .ToList();

                                    hasRunningScopeConflict = runningContainerJobs.Any(job =>
                                        job.StartTime > 0 && job.StartTime < currentSetting.CreateTime);
                                }
                            }
                        }
                        else
                        {
                            string siteCollectionScope = selectedNode.GetSiteCollectionNode()?.FullPath;
                            if (string.IsNullOrWhiteSpace(siteCollectionScope))
                            {
                                siteCollectionScope = nodeUrl;
                            }

                            var runningScopes = RMJobService.GetRunningTeamsArchiverJobSiteUrl(shouldCheckConflictJobTypes, new List<string> { siteCollectionScope });
                            hasRunningScopeConflict = runningScopes.Any(scope =>
                                !string.IsNullOrWhiteSpace(scope)
                                && (RuleSPTreeUtil.IsPrefixWithSlash(nodeUrl, scope)
                                    || RuleSPTreeUtil.IsPrefixWithSlash(scope, nodeUrl)));
                        }
                    }

                    if (hasRunningHierarchyScopeConflict || hasRunningScopeConflict || RMJobService.HasRunningArchiverJobOnScope(shouldCheckConflictJobTypes, nodeUrl))
                    {
                        logger.Warn($"Current has job running or stopping on same scope.{nodeUrl}");
                        jobId = RMJobService.CreateJobWithScopeId(JobType.TeamsArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }

                string jobExtension = string.Empty;
                if (useArchiverImportFile && archiverImportSitesUrl != null && archiverImportSitesUrl.Count > 0)
                {
                    jobExtension = RuleSPTreeUtil.GenerateTeamsArchiveJobMonitorExtension(selectedNode, TreeMode.SO, archiverImportSitesUrl, useArchiverImportFile, teamsUrl: teamsUrl);
                }
                else
                {
                    jobExtension = RuleSPTreeUtil.GenerateTeamsArchiveJobMonitorExtension(selectedNode, TreeMode.SO, teamsUrl: teamsUrl);
                }
                string containerId = GetSPContainerId(selectedNode);
                jobId = RMJobService.CreateJobWithScopeIdForTeams(JobType.TeamsArchiverBackup, jobRunByUser, nodeUrl, nodeFullPath, GetSPContainerId(selectedNode), null, jobExtension);
                int estimatedSiteCount = GetEstimatedSiteCount(selectedNode, containerId);
                if (estimatedSiteCount > 0)
                {
                    SubJobDao.UpdateSubJobCount(jobId, estimatedSiteCount);
                    RMJobService.SetSumSCCountOfJobExtension(estimatedSiteCount, jobId);
                    logger.Info("Initialize main job {0} sub job count by selected node level {1}, estimated site count {2}.", jobId, selectedNode.Level, estimatedSiteCount);
                }
                else
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                    logger.Info($"No available sc to run ,jobId:{jobId}");
                    return jobId;
                }

                try
                {
                    RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetAppliedRuleIds(selectedNode, ContentSourceType.Teams));
                }
                catch (Exception ex)
                {
                    logger.Error($"AddJobRuleMappings failed ,jobId:{jobId},error:{ex}");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                    return jobId;
                }

                if (IsTrailLicenceAndExceedSizeLimit())
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
                    return jobId;
                }

                UpdateJobVersion(jobId, jobType);
                CreateTeamsSubJobsByStream(
                    jobId,
                    jobType,
                    selectedNode,
                    shouldCheckConflictJobTypes,
                    archiverImportSitesUrl,
                    useArchiverImportFile,
                    estimatedSiteCount);
                logger.Info($"Finish Creating subjobs of TeamsArchiverBackup, JobId is {jobId}");

                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("RealRunTeamsArchiverBackupJobOnSelectedNode failed, jobId:{0}, error:{1}", jobId, ex.ToString());
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                }

                throw;
            }
        }

        private string GenerateArchiveJobMonitorExtensionForDSO(List<string> siteUrls, TreeMode isSoMode)
        {
            ArchiveJobMonitorExtension extension = new ArchiveJobMonitorExtension();
            extension.treeMode = isSoMode;
            extension.ConflictNodeLevel = ConflictNodeLevel.SiteCollection;
            extension.IsGroupLevelArchive = false;
            extension.SiteUrls = siteUrls;
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }

        /// <summary>
        /// 1.通过Tree ID，找到节点Setting ID.
        /// 2.通过Setting ID，找到当前Scope对应的Rule ID.
        /// 3.通过Rule ID，找到对应的Rule.
        /// 4.通过Rule查看对应Rule的Action.
        /// </summary>
        private bool OnlyHasVersionDeletionRule(string nodeUrl, RMSPTreeNode selectedNode)
        {
            logger.Info($"Begin check only has version deletion rule for retention job in progress.nodeUrl:{nodeUrl}.");
            bool onlyHasVersionDeletionRule = true;
            try
            {
                Guid groupId = Guid.Empty;
                Guid siteId = Guid.Empty;
                if (selectedNode.Level == (int)NodeLevel.WebApplication)
                {
                    groupId = new Guid(selectedNode.SPObjectId);
                }
                else if (selectedNode.Level == (int)NodeLevel.SiteCollection)
                {
                    siteId = new Guid(selectedNode.SPObjectId);
                    groupId = new Guid(selectedNode.Parent.SPObjectId);
                }
                else if (selectedNode.Level > (int)NodeLevel.SiteCollection)
                {
                    RMSPTreeNode siteNode = selectedNode;
                    while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                    {
                        siteNode = siteNode.Parent;
                    }
                    siteId = new Guid(siteNode?.SPObjectId);

                    RMSPTreeNode groupNode = selectedNode;
                    while (groupNode != null && groupNode.Level != (int)NodeLevel.WebApplication)
                    {
                        groupNode = groupNode.Parent;
                    }
                    groupId = new Guid(groupNode?.SPObjectId);
                }
                var mSetting = ArchiverSettingDao.LoadArchiverSettingBySPObjectId(new Guid(selectedNode.SPObjectId), siteId, groupId);
                logger.Info($"Check only has version deletion rule for retention job in progress.siteId:{siteId}.groupId:{groupId}.SettingId:{mSetting.Id}.ContentSourceType:{mSetting.ContentSourceType}.");
                List<RMSimpleRule> scopeApplyRules = EXOSettingRuleDao.GetMappingRules(mSetting.Id);
                if (scopeApplyRules.Count > 0)
                {
                    foreach (RMSimpleRule simpleRule in scopeApplyRules)
                    {
                        try
                        {
                            RMMiscProfile rMMiscProfile = MiscProfileDao.Load(simpleRule.RuleId.ToString());
                            Rule rule = AvePoint.GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<Rule>(rMMiscProfile.Extension);
                            if (mSetting.ContentSourceType == (int)ContentSourceType.None || mSetting.ContentSourceType == (int)ContentSourceType.SharePoint)
                            {
                                var isDeleteOnlyRule = rule.KeepDataOption == (int)KeepDataOption.DeleteOnly
                                    || rule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly);
                                var isTriggerMicrosoft365ArchivingRule = rule.KeepDataOption == (int)KeepDataOption.TriggerMicrosoft365Archiving
                                    || rule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.TriggerMicrosoft365Archiving);

                                if (isDeleteOnlyRule || isTriggerMicrosoft365ArchivingRule)
                                {
                                    logger.Info($"SharePoint Check only has version deletion rule for retention job in progress.Current Rule is version deletion or microsoft 365 archiving trigger rule.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                }
                                else
                                {
                                    logger.Info($"SharePoint Check only has version deletion rule for retention job in progress.Current Rule is not version deletion or microsoft 365 archiving trigger rule.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                    onlyHasVersionDeletionRule = false;
                                }
                            }
                            else if (mSetting.ContentSourceType == (int)ContentSourceType.OneDrive && rule.OneDriveRule != null)
                            {
                                if (rule.OneDriveRule.KeepDataOption == (int)KeepDataOption.DeleteOnly || rule.OneDriveRule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly))
                                {
                                    logger.Info($"OneDrive Check only has version deletion rule for retention job in progress.Current Rule is version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                                }
                                else
                                {
                                    logger.Info($"OneDrive Check only has version deletion rule for retention job in progress.Current Rule is not version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                                    onlyHasVersionDeletionRule = false;
                                }
                            }
                            else
                            {
                                logger.Info($"Check only has version deletion rule for retention job in progress.Current Rule is not SPO or OneDrive rule.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                onlyHasVersionDeletionRule = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            onlyHasVersionDeletionRule = false;
                            logger.Warn($"Error check only has version deletion rule for retention job in progress.No rules apply for current scope:{nodeUrl}.Message:{ex}.");
                        }
                    }
                }
                else
                {
                    logger.Info($"Check only has version deletion rule for retention job in progress.No rules apply for current scope:{nodeUrl}.");
                    onlyHasVersionDeletionRule = false;
                }
            }
            catch (Exception ex)
            {
                onlyHasVersionDeletionRule = false;
                logger.Warn($"OnlyHasVersionDeletionRule Error check only has version deletion rule for retention job in progress.No rules apply for current scope:{nodeUrl}.Message:{ex}.");
            }

            logger.Info($"Finished check only has version deletion rule for retention job in progress.nodeUrl:{nodeUrl}.onlyHasVersionDeletionRule:{onlyHasVersionDeletionRule}.");
            return onlyHasVersionDeletionRule;
        }

        private bool TeamsOnlyHasVersionDeletionRule(string nodeUrl, RMSPTreeNode selectedNode)
        {
            logger.Info($"teams Begin check only has version deletion rule for retention job in progress.nodeUrl:{nodeUrl}.");
            bool onlyHasVersionDeletionRule = true;
            try
            {
                Guid groupId = Guid.Empty;
                Guid siteId = Guid.Empty;
                if (selectedNode.Level == (int)NodeLevel.WebApplication)
                {
                    groupId = new Guid(selectedNode.SPObjectId);
                }
                else if (selectedNode.Level >= (int)NodeLevel.SiteCollection)
                {
                    // siteId = Guid.Empty;
                    siteId = new Guid(selectedNode.SPObjectId);
                    RMSPTreeNode groupNode = selectedNode;
                    while (groupNode != null && groupNode.Level != (int)NodeLevel.Office365GroupEntire)
                    {
                        groupNode = groupNode.Parent;
                    }
                    groupId = new Guid(groupNode?.SPObjectId);
                }

                var mSetting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(selectedNode.SPObjectId), siteId, groupId, ContentSourceType.Teams);
                logger.Info($"teams Check only has version deletion rule for retention job in progress.siteId:{siteId}.groupId:{groupId}.SettingId:{mSetting.Id}.ContentSourceType:{mSetting.ContentSourceType}.");
                List<RMSimpleRule> scopeApplyRules = EXOSettingRuleDao.GetMappingRules(mSetting.Id);
                if (scopeApplyRules.Count > 0)
                {
                    foreach (RMSimpleRule simpleRule in scopeApplyRules)
                    {
                        try
                        {
                            RMMiscProfile rMMiscProfile = MiscProfileDao.Load(simpleRule.RuleId.ToString());
                            Rule rule = AvePoint.GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<Rule>(rMMiscProfile.Extension);
                            if (mSetting.ContentSourceType == (int)ContentSourceType.None || mSetting.ContentSourceType == (int)ContentSourceType.SharePoint)
                            {
                                if (rule.KeepDataOption == (int)KeepDataOption.DeleteOnly || rule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly))
                                {
                                    logger.Info($"teams Check only has version deletion rule for retention job in progress.Current Rule is version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                }
                                else
                                {
                                    logger.Info($"teams Check only has version deletion rule for retention job in progress.Current Rule is not version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                    onlyHasVersionDeletionRule = false;
                                }
                            }
                            else if (mSetting.ContentSourceType == (int)ContentSourceType.OneDrive && rule.OneDriveRule != null)
                            {
                                if (rule.OneDriveRule.KeepDataOption == (int)KeepDataOption.DeleteOnly || rule.OneDriveRule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly))
                                {
                                    logger.Info($"teams OneDrive Check only has version deletion rule for retention job in progress.Current Rule is version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                                }
                                else
                                {
                                    logger.Info($"teams OneDrive Check only has version deletion rule for retention job in progress.Current Rule is not version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                                    onlyHasVersionDeletionRule = false;
                                }
                            }
                            else if (mSetting.ContentSourceType == (int)ContentSourceType.Teams)
                            {
                                if (rule.TeamsRule != null)
                                {
                                    if (rule.TeamsRule.KeepDataOption == (int)KeepDataOption.DeleteOnly || rule.TeamsRule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly))
                                    {
                                        logger.Info($"teams teams rule Check only has version deletion rule for retention job in progress.Current Rule is version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                                    }
                                    else
                                    {
                                        logger.Info($"teams teams rule Check only has version deletion rule for retention job in progress.Current Rule is not version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.OneDriveRule.KeepDataOption}.");
                                        onlyHasVersionDeletionRule = false;
                                    }
                                }
                                else
                                {
                                    if (rule.KeepDataOption == (int)KeepDataOption.DeleteOnly || rule.KeepDataOption == ((int)KeepDataOption.KeepLatestVersion + (int)KeepDataOption.DeleteOnly))
                                    {
                                        logger.Info($"teams level Check only has version deletion rule for retention job in progress.Current Rule is version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                    }
                                    else
                                    {
                                        logger.Info($"teams level Check only has version deletion rule for retention job in progress.Current Rule is not version deletion.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                        onlyHasVersionDeletionRule = false;
                                    }
                                }
                            }
                            else
                            {
                                logger.Info($"teams Check only has version deletion rule for retention job in progress.Current Rule is not SPO or OneDrive rule.RuleName:{rule.Name}.KeepDataOption:{rule.KeepDataOption}.");
                                onlyHasVersionDeletionRule = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            onlyHasVersionDeletionRule = false;
                            logger.Warn($"teams Error check only has version deletion rule for retention job in progress.No rules apply for current scope:{nodeUrl}.Message:{ex}.");
                        }
                    }
                }
                else
                {
                    logger.Info($"teams Check only has version deletion rule for retention job in progress.No rules apply for current scope:{nodeUrl}.");
                    onlyHasVersionDeletionRule = false;
                }
            }
            catch (Exception ex)
            {
                onlyHasVersionDeletionRule = false;
                logger.Warn($"teams OnlyHasVersionDeletionRule Error check only has version deletion rule for retention job in progress.No rules apply for current scope:{nodeUrl}.Message:{ex}.");
            }

            logger.Info($"teams Finished check only has version deletion rule for retention job in progress.nodeUrl:{nodeUrl}.onlyHasVersionDeletionRule:{onlyHasVersionDeletionRule}.");
            return onlyHasVersionDeletionRule;
        }

        private List<RMSPTreeNode> FilterByArchiverImportFile(List<RMSPTreeNode> availableNode, List<string> archiverImportSitesUrl, bool useArchiverImportFile)
        {
            if (useArchiverImportFile)
            {
                if (archiverImportSitesUrl.IsNullOrEmpty())
                {
                    return new List<RMSPTreeNode>();
                }

                var importUrlSet = new HashSet<string>(archiverImportSitesUrl, StringComparer.OrdinalIgnoreCase);
                return availableNode.Where(a => importUrlSet.Contains(a.FullPath)).ToList();
            }
            else
            {
                return availableNode;
            }
        }
        private bool IsTrailLicenceAndExceedSizeLimit()
        {
            try
            {
                using var pc = new PerformanceScope("IsTrailLicenceAndExceedSizeLimit");
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Type == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                {
                    logger.Info("this is Trial licence");
                    var size = StorageDeviceService.GetArchiverStorageGBSize();
                    var resultSize = size;
                    if (resultSize >= 5)
                    {
                        logger.Info($"current trial licence user has run out of size {resultSize}gb is bigger than 5gb");
                        //RMKeyValueDao.SaveAsync(new DB.Model.RMKeyValue() { Key= keyString ,Value="true"}).GetAwaiter().GetResult();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error($"some thing went wrong when check Trail Licence And Exceed Size,error{e.ToString()}");
                return false;
            }
        }
        public string RealRunRebuildStubJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.RebuildStub;
            //RebuildStubInfo rebuildStubInfo = SerializerHelper.DeserializeByDataContractSerializer<RebuildStubInfo>(param);
            //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            List<JobType> types = new List<JobType>() { JobType.RebuildStub };
            string jobId = jobId = RMJobService.CreateJob(JobType.RebuildStub, jobRunByUser);
            SubJobDao.UpdateSubJobCount(jobId, 1);
            string subJobId = string.Format(jobId + "_{0:D3}", 0);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait };
            subJob.Runable = RecordsConstants.SubJob_Runnable_Runing;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = param };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}.", subJob.Id, subJob.JobType);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            });
            return jobId;
        }

        public string RealRunRebuildIndexJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.RebuildIndex;
            string jobId = RMJobService.CreateJob(jobType, jobRunByUser);
            SubJobDao.UpdateSubJobCount(jobId, 1);
            string subJobId = string.Format(jobId + "_{0:D3}", 0);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait };
            subJob.Runable = RecordsConstants.SubJob_Runnable_Runing;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = param };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}.", subJob.Id, subJob.JobType);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            });

            return jobId;
        }

        public string RealRunRebuildSOJobReportJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.RebuildSOJobReport;
            string rebuildJobId = RMJobService.GenerateJobId(JobType.RebuildSOJobReport);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = rebuildJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1} {2}", jobType, rebuildJobId, param),
            });
            logger.Info($"Create virtual sub job {rebuildJobId} sucessfull, type RebuildSOJobReport.RebuildSOJobID:{param}.");
            return rebuildJobId;
        }

        public string RealRunRebuildEncryptKeyValueJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.RebuildEncryptKeyValue;
            string rebuildJobId = RMJobService.GenerateJobId(JobType.RebuildEncryptKeyValue);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = rebuildJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1} {2}", jobType, rebuildJobId, param),
            });
            logger.Info($"Create virtual sub job {rebuildJobId} sucessfull, type RebuildEncryptKeyValue.RebuildEncryptKeyValueJobID:{param}.");
            return rebuildJobId;
        }

        public string RealRunDispatchedJob(
            JobRunBy jobRunBy,
            string jobRunByUser,
            JobType targetJobType,
            string param,
            string originalMessageId,
            string originalTenantId)
        {
            JobType jobType = JobType.DispatchedJob;
            string jobId = RMJobService.GenerateJobId(jobType);
            string payload = Newtonsoft.Json.JsonConvert.SerializeObject(new JobDispatchPayload()
            {
                TargetJobType = targetJobType,
                Parameters = param,
                OriginalMessageId = originalMessageId,
                OriginalTenantId = originalTenantId,
            });
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                RunBy = jobRunBy,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, jobId),
                Extension = payload,
            });
            logger.Info($"Create DispatchedJob {jobId} sucessfull, type DispatchedJob.DispatchedJobID, targetJobType:{targetJobType}, param:{param}.");
            return jobId;
        }

        public string RealRunBuildRunningJobReportJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.BuildRunningJobReport;
            string rebuildJobId = RMJobService.GenerateJobId(JobType.BuildRunningJobReport);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = rebuildJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1} {2}", jobType, rebuildJobId, param),
            });
            logger.Info($"Create virtual sub job {rebuildJobId} sucessfull, type BuildRunningJobReport.BuildRunningJobID:{param}.");
            return rebuildJobId;
        }

        public string RealRunExportDecryptIndexDBJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.ExportDecryptIndexDB;
            string rebuildJobId = RMJobService.GenerateJobId(JobType.ExportDecryptIndexDB);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = rebuildJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1} {2}", jobType, rebuildJobId, param),
            });
            logger.Info($"Create virtual sub job {rebuildJobId} sucessfull, type ExportDecryptIndexDB. ExportDecryptIndexDBJobID:{param}.");
            return rebuildJobId;
        }

        public string RealRunBaseArchiveJobIdMultiRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.BaseArchiveJobIdMultiRestore;
            string rebuildJobId = RMJobService.GenerateJobId(JobType.BaseArchiveJobIdMultiRestore);
            SubJobDao.AddJobContext(rebuildJobId, param);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = rebuildJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, rebuildJobId),
            });
            logger.Info($"Create virtual sub job {rebuildJobId} sucessfull, type BaseArchiveJobIdMultiRestore. param:{param}.");
            return rebuildJobId;
        }

        public string RealRunRebuildDeDupForWPPMigrationJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.RebuildDeDupForWPPMigration;
            string rebuildJobId = RMJobService.GenerateJobId(JobType.RebuildDeDupForWPPMigration);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = rebuildJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1} {2}", jobType, rebuildJobId, param),
            });
            logger.Info($"Create virtual sub job {rebuildJobId} sucessfull, type RebuildSOJobReport.RebuildSOJobID:{param}.");
            return rebuildJobId;
        }

        public string RealRunSOPreScanJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.SOPreScan;
            RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
            var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            return RealRunSOPreScanOnSelectedNode(loginName, jobType, selectedNode);
        }

        public string RealRunSOPreScanOnSelectedNode(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            try
            {
                List<JobType> types = new List<JobType>() { JobType.SOPreScan };

                string containerId = GetSPContainerId(selectedNode);

                string nodeUrl = selectedNode.FullPath;
                if (selectedNode.Level == (int)NodeLevel.Folder && !nodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var siteNode = selectedNode.GetSiteCollectionNode();
                    if (siteNode != null)
                    {
                        nodeUrl = WebUtil.MakeFullUrl(selectedNode.GetSiteCollectionNode().FullPath, selectedNode.FullPath);
                    }
                }
                if (RMJobService.HasRunningArchiverJobOnScope(types, nodeUrl) || RMJobService.HasStoppingArchiverJobOnScope(types, nodeUrl))
                {
                    logger.Warn($"Current has job running on same scope.{nodeUrl}");
                    jobId = RMJobService.CreateJobWithScopeId(JobType.SOPreScan, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                else
                {
                    jobId = RMJobService.CreateJobWithScopeId(JobType.SOPreScan, jobRunByUser, nodeUrl, containerId);
                }
                int estimatedSiteCount = GetEstimatedSiteCount(selectedNode, containerId, false, null);

                if (estimatedSiteCount <= 0)
                {
                    logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                    return jobId;
                }

                SubJobDao.UpdateSubJobCount(jobId, estimatedSiteCount);
                RMJobService.SetSumSCCountOfJobExtension(estimatedSiteCount, jobId);
                logger.Info("Initialize main job {0} sub job count by selected node level {1}, estimated site count {2}.", jobId, selectedNode.Level, estimatedSiteCount);

                try
                {
                    RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetAppliedRuleIds(selectedNode));
                }
                catch (Exception e)
                {
                    logger.Error($"AddJobRuleMappings failed for job {jobId}, error:{e.ToString()}");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                    return jobId;
                }

                CreateSOPreScanSubJobsByStream(jobId, jobType, selectedNode, types, estimatedSiteCount);
                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("RealRunSOPreScanOnSelectedNode failed, jobId:{0}, error:{1}", jobId, ex.ToString());
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                }

                throw;
            }
        }

        public RAReturnMessage RunArchiverMoveIndexJob(JobRunBy jobRunBy, string jobRunByUser, string sourceDeviceId, string destinationDeviceId)
        {
            logger.Info($"Start archiver MoveIndex job, SourceDeviceId {sourceDeviceId} , DestinationDeviceId {destinationDeviceId}");
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var indexDevice = StorageDeviceService.GetIndexDevice();
                if (indexDevice == null)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_AR_RunJob_Failed_NoIndexDeviceSetting");
                    return msg;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                RMArchiverMoveIndexInfo rMArchiverMoveIndexInfo = new RMArchiverMoveIndexInfo()
                {
                    SrcIndexDeviceId = sourceDeviceId,
                    DestIndexDeviceId = destinationDeviceId
                };
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ArchiverMoveIndex,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(rMArchiverMoveIndexInfo)
                };

                string id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                else
                {
                    msg.Extension = id;
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while MoveIndex,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.StorageDeviceSettings, Action = AuditAction.RunMoveIndexJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunArchiverMoveIndexJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info($"Start archiver MoveIndex job.");
            List<JobType> otherJobTypes = new List<JobType>() { JobType.RMArchiverBackup, JobType.RMEndUserArchiverBackup, JobType.SpecifySitesArchiverBackup, JobType.RecordsDisposal, JobType.OneDriveRecordsDisposal, JobType.ArchiverRestore, JobType.ArchiverToSpoRestore, JobType.StubArchiverRestore, JobType.M365InPlaceArchiverRestore, JobType.ArchiverRetention, JobType.ArchiverOutPlaceRestore, JobType.ArchiverByHSMXml, JobType.DiscoverOptimization, JobType.DiscoveryAOSPOptimization, JobType.ArchiverFullTextIndex, JobType.StubOopRestore, JobType.AOSPRestore, JobType.DeleteRestoredData, JobType.DeleteOrphanDatas, JobType.FSRetain, JobType.FSArchiverRestore, JobType.FSDisposal, JobType.ConvertStub, JobType.TeamsArchiverRestore, JobType.TeamsOutPlaceRestore, JobType.MailBoxArchiverRestore, JobType.TeamsArchiverBackup, JobType.TeamsRecordsDisposal, JobType.TeamsArchiverRetention, JobType.GoogleRecordsDisposal, JobType.GoogleArchiverRestore, JobType.GoogleArchiverRetention, JobType.SpecifyTeamsArchiverBackup, JobType.CleanUpDuplicateDatas, JobType.DeleteArchivedSiteCollection };
            List<JobType> moveIndexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
            string jobId = string.Empty;
            JobType jobType = JobType.ArchiverMoveIndex;
            RMArchiverMoveIndexInfo info = SerializerHelper.DeserializeByDataContractSerializer<RMArchiverMoveIndexInfo>(param);
            logger.Info($"SourceDeviceId {info.SrcIndexDeviceId} , DestinationDeviceId {info.DestIndexDeviceId}");

            //int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            jobId = RMJobService.CreateJobWithScopeId(JobType.ArchiverMoveIndex, jobRunByUser, info.DestIndexDeviceId);

            var mIndexJobs = RMJobService.GetRunningJobs(moveIndexJobTypes);
            foreach (var i in mIndexJobs)
            {
                if (!i.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                {
                    //RM_Job_MoveIndexJobAlreadyScheduledConflict
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_MoveIndexJobAlreadyScheduledConflict");
                    return jobId;
                }
            }

            var mOtherJobs = RMJobService.GetRunningJobs(otherJobTypes);
            foreach (var i in mOtherJobs)
            {
                if (!i.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                {
                    //RM_Job_MoveIndexJobAlreadyScheduledConflict
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
            }

            SubJobDao.UpdateSubJobCount(jobId, 1);
            string subJobId = CreateSubJobForMoveIndex(jobId, 0, jobType, 1, info, true);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            });
            return jobId;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunVeoMergeJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunVeoMergeJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info($"Start archiver Veo Merge job.");

            string jobId = string.Empty;
            JobType jobType = JobType.VeoMerge;
            jobId = RMJobService.CreateJob(JobType.VeoMerge, jobRunByUser);
            SubJobDao.UpdateSubJobCount(jobId, 1);
            List<string> info = SerializerHelper.DeserializeByDataContractSerializer<List<string>>(param);
            string subJobId = CreateSubJobForVEOMerge(jobId, 0, jobType, 1, info, true);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            });
            return jobId;
        }
        public async Task<string> RealRunMoveDataTierJobAsync(JobRunBy jobRunBy, string jobRunByUser, Dictionary<string, List<string>> jobidMapping)
        {
            logger.Info($"Start move data tier job.");

            string jobId = string.Empty;
            JobType jobType = JobType.MoveDataTier;
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            jobId = RMJobService.CreateJob(JobType.MoveDataTier, jobRunByUser, account.UserId);
            SubJobDao.UpdateSubJobCount(jobId, 1);
            int currentIndex = 0;
            foreach (var temp in jobidMapping)
            {
                MoveDataTierContent content = new MoveDataTierContent()
                {
                    SiteUrl = temp.Key,
                    JobIds = temp.Value
                };
                var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);

                string subJobId = CreateSubJobForMoveDataTier(jobId, currentIndex, jobType, 1, content, currentIndex < subJobCountInConfigFile);
                if (currentIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Schedule,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                currentIndex++;
            }

            return jobId;
        }
        public RAReturnMessage RunArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start archiver Retention job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ArchiverRetention,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while Retention,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }
        public RAReturnMessage RunFSRetentionJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start archiver Retention job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var HasGoogleLicense = TenantService.CheckLicenseWithAdditionalProduct(groupId, PaidForProduct.OpusGoogle);
                var HasArchiverLicense = TenantService.CheckLicenseWithAdditionalProduct(groupId, PaidForProduct.OpusSO);
                var HasRecordsLicense = TenantService.CheckLicenseWithAdditionalProduct(groupId, PaidForProduct.OpusIL);
                var licenseType = (RMAosApiClient.GetLicenseInfo(groupId).GetAwaiter().GetResult()).Type;
                logger.Info($"this user so only license or trail license info,HasArchiverLicense:{HasArchiverLicense},HasRecordsLicense:{HasRecordsLicense},HasGoogleLicense:{HasGoogleLicense},licenseType:{licenseType}");
                if ((HasArchiverLicense && !HasRecordsLicense && !HasGoogleLicense) || licenseType == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                {
                    //so only license or trail license
                    //logger.Info($"this user has so only license or trail license,HasArchiverLicense:{HasArchiverLicense},HasRecordsLicense:{HasRecordsLicense},HasGoogleLicense:{HasGoogleLicense},licenseType:{licenseType}");
                }
                else
                {
                    var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                    JobQueueDto jqDto = new JobQueueDto()
                    {
                        JobType = JobType.FSRetain,
                        JobRunType = JobRunBy.Control,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName
                    };

                    id = JobQueueService.AddToDBJobQueue(jqDto);
                    if (string.IsNullOrEmpty(id))
                    {
                        msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while fs Retention,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }
        public RAReturnMessage RunTeamsArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start run Teams archiver retention job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsArchiverRetention,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while Retention,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }

        public RAReturnMessage RunEXOArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start run EXO archiver retention job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.EXOArchiverRetention,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while run EXO retention job,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }
        public RAReturnMessage RunGDriveArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start run google drive archiver retention job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.GoogleArchiverRetention,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while run google drive retention job,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }

        // JobMonitor archive: enqueue a job into JobQueue for RealTime executor
        public RAReturnMessage RunJobMonitorArchiveJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info("Start enqueue JobMonitor archive job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.JobMonitorArchive,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                else
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Successful, Extension = id };
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while enqueuing JobMonitor archive job: {ex}");
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.JobMonitor, Action = AuditAction.RunJobMonitorArchiveJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunJobMonitorArchiveJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            // Always create a JobMonitor entry to give visibility even if skipped
            var jobId = JobMonitorService.CreateJob(JobType.JobMonitorArchive, jobRunByUser);
            try
            {
                // We only allow a single instance of this job type at a time.
                var runningSameTypeJobs = JobMonitorService.GetRunningJobs(JobType.JobMonitorArchive) ?? new List<string>();
                runningSameTypeJobs = runningSameTypeJobs.Where(id => !string.Equals(id, jobId, StringComparison.OrdinalIgnoreCase)).ToList();
                if (runningSameTypeJobs.Any())
                {
                    logger.Warn($"Skip starting JobMonitorArchive because another instance is running. RunningIds=[{string.Join(",", runningSameTypeJobs)}]");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "Another JobMonitorArchive job is already running.");
                    return await System.Threading.Tasks.Task.FromResult(jobId);
                }

                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = JobRunBy.Schedule,
                    JobType = JobType.JobMonitorArchive,
                    CommandLine = string.Format("{0} {1}", JobType.JobMonitorArchive, jobId),
                });
            }
            catch (Exception e)
            {
                logger.Error($"RealRunJobMonitorArchiveJobAsync outer failure: {e}");
                // Create a failed job to surface the error path if creation possible

                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
            }
            return jobId;
        }

        public RAReturnMessage RunDeleteOrphanDatasJob(JobRunBy jobRunBy, string jobRunByUser, List<string> needDeleteJobIds)
        {
            logger.Info($"Start archiver Retention job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            if (!IsEnableDeleteOrphanDataSetting())
            {
                msg.MessageType = RAMessageType.Failed;
                return msg;
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.DeleteOrphanDatas,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(needDeleteJobIds)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while run delete orphan datas job,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }
        public RAReturnMessage RunArchiverDeduplicationJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start archiver Dedup job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ArchiverDeduplication,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while Retention,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }

        public RAReturnMessage RunApprovalProcessJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start approval process job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ApprovalProcessArchive,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while approval process,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }
        public async Task<List<RMSPTreeNode>> GetApprovalProcessJobSites()
        {
            var needRunJobNodes = GetNeedRunJobNodes();
            logger.Info($"this job has need run job nodes count is {needRunJobNodes?.Count}");
            var odRealNodes = await GetAvailableSites(needRunJobNodes, RMBrowseTreeNodeSourceType.SkyDrivePro);
            var spRealNodes = await GetAvailableSites(needRunJobNodes, RMBrowseTreeNodeSourceType.SharepointOnline);
            var result = new List<RMSPTreeNode>();
            if (spRealNodes != null && spRealNodes.Count > 0)
            {
                result.AddRange(spRealNodes);
            }
            if (odRealNodes != null && odRealNodes.Count > 0)
            {
                result.AddRange(odRealNodes);
            }
            return result;
        }

        public async Task<string> RealRunApprovalProcessJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            try
            {
                string jobId = string.Empty;

                var isRecheckRuleSetting = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.IsRecheckRule);
                if (!bool.TryParse(isRecheckRuleSetting, out bool isRecheckRule))
                {
                    isRecheckRule = true; //the old setting need to check rule
                }
                logger.Info($"current is recheck rule status is :{isRecheckRule}");

                #region sp od disposal
                var needRunJobNodes = GetNeedRunJobNodes(isRecheckRule);
                logger.Info($"this job has need run job nodes count is {needRunJobNodes?.Count}");
                var odRealNodes = await GetAvailableSites(needRunJobNodes, RMBrowseTreeNodeSourceType.SkyDrivePro);
                var spRealNodes = await GetAvailableSites(needRunJobNodes, RMBrowseTreeNodeSourceType.SharepointOnline);
                if (spRealNodes != null && spRealNodes.Count > 0)
                {
                    logger.Info($"this job has sp need run job nodes count is {spRealNodes?.Count}");
                    await RMSharePointSettingsService.RealRunApprovalProcessJobAsync(jobRunBy, jobRunByUser, spRealNodes, JobType.RecordsDisposal);
                }
                else
                {
                    logger.Info($"this job has no sp need run job nodes.");
                }
                if (odRealNodes != null && odRealNodes.Count > 0)
                {
                    logger.Info($"this job has od need run job nodes count is {odRealNodes?.Count}");
                    await RMSharePointSettingsService.RealRunApprovalProcessJobAsync(jobRunBy, jobRunByUser, odRealNodes, JobType.OneDriveRecordsDisposal);
                }
                else
                {
                    logger.Info($"this job has no od need run job nodes.");
                }

                if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
                {
                    var teamsNodes = await GetTeamsAvailableNodes(isRecheckRule);
                    if (teamsNodes != null && teamsNodes.Any())
                    {
                        logger.Info($"Start run approval process job, {teamsNodes?.Count}");
                        await RMSharePointSettingsService.RealRunApprovalProcessJobAsync(jobRunBy, jobRunByUser, teamsNodes, JobType.TeamsRecordsDisposal);
                    }
                    else
                    {
                        logger.Warn($"Skipped Teams approval process job since don't have any available nodes.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while ApprovalProcess sp or od,ERROR:{e}");
            }
            #endregion

            return string.Empty;
        }
        private async Task<List<RMSPTreeNode>> GetAvailableSites(List<RMSharePointSetting> needRunJobNodes, RMBrowseTreeNodeSourceType sourceType)
        {
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            List<string> hasApprovedUrls = new List<string>();
            var hasApprovedRecods = explorerDao.QueryAll(r => r.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved && r.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.None && r.RecordStatus != (int)RMRecordStatus.Destroyed && r.RecordStatus != (int)RMRecordStatus.RMDeleted && (r.SourceFlag == (sourceType == RMBrowseTreeNodeSourceType.SharepointOnline ? (int)SourceFlag.SharePoint : (int)SourceFlag.OneDrive)));
            if (hasApprovedRecods != null && hasApprovedRecods.Count() > 0)
            {
                hasApprovedUrls = hasApprovedRecods.Select(a => a.ManualSiteUrl).Distinct().ToList();
                logger.Info($"this job has approved urls count is {hasApprovedUrls?.Count}");
            }
            foreach (var nodeInfo in needRunJobNodes)
            {
                var sett = CloneSetting(nodeInfo);
                if (sett.NodeInfo == null)
                {
                    logger.Info("no change, nodeinfo is null.Id:{0}", sett.ScopeId);
                    continue;
                }
                var group = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(sett.NodeInfo);
                var browseTreeSourceType = group.NodeType == (int)GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup ? RMBrowseTreeNodeSourceType.SkyDrivePro : RMBrowseTreeNodeSourceType.SharepointOnline;
                if (browseTreeSourceType == sourceType)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    List<RMSPTreeNode> childNodes = await RMSPTreeService.BrowseAsync(group, false, browseTreeSourceType);
                    if (childNodes == null || childNodes.Count == 0)
                    {
                        logger.Info("No sites in  the gourp {0}", group.Name);
                        continue;
                    }
                    foreach (RMSPTreeNode site in childNodes)
                    {
                        try
                        {
                            bool exsitApprovalData = hasApprovedUrls.Contains(site.Name);
                            if (exsitApprovalData)
                            {
                                site.IsProcessApprovalDatasOnly = true;
                                availableSites.Add(site);
                            }
                            else
                            {
                                logger.Info("The site {0} has no approval data to process", site.Name);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"check approval data exsit failed,error:{e}");
                        }
                    }
                    sw.Stop();
                    logger.Info($"approve disposal GetAvailableSites cost time:{sw.ElapsedMilliseconds},node info:{nodeInfo.FullPath}");
                }
                else
                {
                    logger.Info("The group {0} is not the source type {1}", group.Name, sourceType);
                }
            }
            return availableSites;
        }

        private async Task<List<RMSPTreeNode>> GetTeamsAvailableNodes(bool isRecheckRule = true)
        {
            logger.Info("Start get teams avaiable sites.");
            var groupSettings = TeamsSettingsDao.LoadGroupSetting(isRecheckRule);
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            List<string> hasApprovedUrls = new List<string>();
            var hasApprovedRecods = explorerDao.QueryAll(r => r.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved && r.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.None && r.RecordStatus != (int)RMRecordStatus.Destroyed && r.RecordStatus != (int)RMRecordStatus.RMDeleted && (r.SourceFlag == (int)SourceFlag.Teams || r.SourceFlag == (int)SourceFlag.SharePoint));
            if (hasApprovedRecods != null && hasApprovedRecods.Count() > 0)
            {
                hasApprovedUrls = hasApprovedRecods.Select(a => a.ManualSiteUrl).Distinct().ToList();
                logger.Info($"this job has approved urls count is {hasApprovedUrls?.Count}");
            }
            foreach (var sett in groupSettings)
            {
                var group = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(sett.NodeInfo);
                List<RMSPTreeNode> childNodes = await TeamsSettingTreeService.BrowseAsync(group, needChannel: true);
                if (childNodes == null || childNodes.Count == 0)
                {
                    logger.Info("No sites in the group {0}", group.Name);
                    continue;
                }
                foreach (RMSPTreeNode site in childNodes)
                {
                    try
                    {
                        bool exsitApprovalData = hasApprovedUrls.Contains(site.DisplayName);
                        if (exsitApprovalData)
                        {
                            site.IsProcessApprovalDatasOnly = true;
                            availableSites.Add(site);
                        }
                        else
                        {
                            logger.Info("The site {0} has no approval data to process", site.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Check approval data exist failed, error:{e}");
                    }
                }
            }
            logger.Info($"End to get teams avaiable sites, count [{availableSites.Count}].");
            return availableSites;
        }
        private RMSharePointSetting CloneSetting(RMSharePointSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMSharePointSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMSharePointSetting>(xml);
            return result;
        }
        private List<RMSharePointSetting> GetNeedRunJobNodes(bool isRecheckRule = true)
        {
            var spNodes = SharePointSettingDao.LoadGroupSetting(isRecheckRule);
            var oneDriveNodes = OneDriveSettingDao.LoadGroupSetting(isRecheckRule);
            if (oneDriveNodes.Count > 0)
            {
                oneDriveNodes.ForEach((o) =>
                {
                    var odNode = ConvertToRMSharePointSetting(o);
                    if (odNode != null)
                    {
                        spNodes.Add(odNode);
                    }
                });
            }
            return spNodes;
        }
        private RMSharePointSetting ConvertToRMSharePointSetting(RMOneDriveSetting oneDriveSetting)
        {
            if (oneDriveSetting != null)
            {
                return new RMSharePointSetting
                {
                    ScopeId = oneDriveSetting.ScopeId,
                    NodeInfo = oneDriveSetting.NodeInfo
                };
            }
            return null;
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.JobMonitor, Action = AuditAction.RunDeleteOrphanDatasJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunDeleteOrphanDatasJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info($"Start Delete Orphan Datas job.");

            string jobId = string.Empty;
            JobType jobType = JobType.DeleteOrphanDatas;

            var mJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            if (mJobs.Count > 0)
            {
                foreach (var job in mJobs)
                {
                    if (!job.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        jobId = RMJobService.CreateJob(JobType.DeleteOrphanDatas, jobRunByUser);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
            }
            int currentIndex = 0;
            Dictionary<string, List<ArchiverPruningJob>> needRetentionSitesJob = new Dictionary<string, List<ArchiverPruningJob>>();
            var indeDevice = StorageDeviceService.GetIndexDevice();
            if (indeDevice == null)
            {
                logger.Error("Cannot find inde Device.");
                jobId = RMJobService.CreateJob(JobType.DeleteOrphanDatas, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped);
                return jobId;
            }
            try
            {
                List<string> mainJobIds = SerializerHelper.DeserializeByDataContractSerializer<List<string>>(param);
                GenerateSiteDeleteOrphanDatasInfo(needRetentionSitesJob, indeDevice, mainJobIds);
            }
            catch (LicenseMismatchOfAvePointStorageException e)
            {
                jobId = RMJobService.CreateJob(JobType.DeleteOrphanDatas, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);

            if (needRetentionSitesJob.Count == 0)
            {
                logger.Info("no job need to delete orphaned blob.");
                jobId = RMJobService.CreateJob(JobType.DeleteOrphanDatas, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_NoJobNeedToDeleteOrphanedBlob");
                return jobId;
            }
            var runningSiteUrls = RMJobService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, needRetentionSitesJob.Select(job => job.Key), true);
            needRetentionSitesJob = FilterRetentionCanrunUrl(needRetentionSitesJob, runningSiteUrls);
            if (needRetentionSitesJob.Count == 0)
            {
                logger.Info("All sites are running other job.");
                jobId = RMJobService.CreateJob(JobType.DeleteOrphanDatas, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            else
            {
                logger.Info($"The sites count is {needRetentionSitesJob.Count}");
                jobId = RMJobService.CreateJobWithScopeId(JobType.DeleteOrphanDatas, jobRunByUser, null, null, null, GenerateRetentionExtensionInfo(needRetentionSitesJob.Keys.ToList()));
                SubJobDao.UpdateSubJobCount(jobId, needRetentionSitesJob.Count);
            }

            foreach (var siteRetain in needRetentionSitesJob)
            {
                var orderedRetentionList = siteRetain.Value.OrderBy(j => j.ArchiverBackupTime).ToList();
                string siteRetentionInfo = SerializerHelper.SerializeByDataContractSerializer(orderedRetentionList);
                string subJobId = CreateSubJobForRetention(jobId, currentIndex, jobType, needRetentionSitesJob.Count, siteRetentionInfo, currentIndex < subJobCountInConfigFile);
                logger.Info($"Start delete orphan datas job ,jobid:{subJobId},site url:{siteRetain.Key}");
                if (currentIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                currentIndex++;
            }
            return jobId;
        }




        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunArchiverRetentionJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool isSimulateJob = false, string previousJobId = "")
        {
            logger.Info($"Start archiver Retention job.");

            string jobId = string.Empty;
            JobType jobType = isSimulateJob ? JobType.ArchiverRetentionSimulate : JobType.ArchiverRetention;
            JobStatus jobStatus = JobStatus.Wait;

            long nextRunJobDate = 0;
            string mainRetentionJobId = "";
            try
            {
                if (isSimulateJob)
                {
                    nextRunJobDate = GetNextRetentionRunTime();
                    if (nextRunJobDate == 0)
                    {
                        logger.Info($"Skipped run archiver retention silumate job due to next run time is 0");
                        return null;
                    }
                    mainRetentionJobId = CreateOrUpdateMainRetentionSimulateJob(nextRunJobDate);
                    if (mainRetentionJobId.IsNullOrEmpty())
                    {
                        logger.Info($"Skipped run archiver retention silumate job due to main rentention jobid is null");
                        return null;
                    }

                    RententionInfosDao.AddOrUpdateRetentionInfo(new RMRetentionSimulateInfos()
                    {
                        SourceFlag = (int)SourceFlag.SharePoint,
                        FileNumber = 0,
                        DataSize = 0,
                        LastRunJobDate = DateTime.UtcNow.Ticks,
                        NextRunJobDate = nextRunJobDate,
                        LastRetentionJobId = previousJobId,
                        RetentionJobId = jobId,
                        MainRetentionJobId = mainRetentionJobId,
                        JobStatus = (int)jobStatus,
                        MergeReportState = jobStatus == JobStatus.Wait ? (int)MergeIndexState.None : (int)MergeIndexState.Succeed,
                    });

                    if (previousJobId.IsNotNullOrEmpty())
                    {
                        var previousJob = await RMJobService.GetJobAsync(previousJobId);
                        while (!RA.Common.JobService.JobServiceUtility.IsFinalState((int)previousJob.Status))
                        {
                            logger.Info($"awaiting real job finish before run retention simualte job. JobId:{previousJobId}, JobStatus:{previousJob.Status}");
                            await Task.Delay(10000);

                            previousJob = await RMJobService.GetJobAsync(previousJobId);
                        }
                    }
                }

                var mJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
                if (mJobs.Count > 0)
                {
                    foreach (var job in mJobs)
                    {
                        if (!job.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                        {
                            jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                            RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                            jobStatus = JobStatus.Finished;
                            return jobId;
                        }
                    }
                }
                var phDtos = await StorageDeviceService.GetAllStorageDeviceNotPagedAsync();
                var deviceList = new List<string>();
                var needCheckRulePolicyDeviceList = new List<string>();
                foreach (var pro in phDtos)
                {
                    if (pro.SetupDataRetention && !deviceList.Contains(pro.Id))
                    {
                        deviceList.Add(pro.Id);
                    }
                    else if (!needCheckRulePolicyDeviceList.Contains(pro.Id))
                    {
                        needCheckRulePolicyDeviceList.Add(pro.Id);
                    }
                }

                if (needCheckRulePolicyDeviceList.Count > 0)
                {
                    var allRules = GetRulesForRetentionDeviceCheck();
                    var archiverRule = allRules.Where(r =>
                        (r.SOFilters != null && r.SOFilters.Count > 0) ||
                        (r.ProfileType == ProfileType.AOSPArchiverRuleForRevIM && !string.IsNullOrWhiteSpace(r.StoragePolicyId)))
                        .ToList();
                    var oneDriveRule = allRules.Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count > 0).ToList();
                    var physicalRule = allRules.Where(r => r.PhysicalRule != null && r.PhysicalRule.SOFilters != null && r.PhysicalRule.SOFilters.Count > 0).ToList();
                    var teamsRule = allRules.Where(r => r.TeamsRule != null && r.TeamsRule.SOFilters != null && r.TeamsRule.SOFilters.Count > 0).ToList();
                    logger.Info($"SONeed check rule level retention:[{string.Join(",", needCheckRulePolicyDeviceList)}]");
                    foreach (var storageId in needCheckRulePolicyDeviceList)
                    {
                        logger.Info($"SOStorage [{storageId}] process start.");
                        var hasStorageRules = new List<Rule>();


                        var archiverStorageRules = archiverRule.Where(r => r.StoragePolicyId == storageId).ToList();
                        if (archiverStorageRules.Any())
                        {
                            logger.Info($"SOStorage [{storageId}] SPO rules:[{string.Join(",", archiverStorageRules.Select(r => r.Id))}]");
                            hasStorageRules.AddRange(archiverStorageRules);
                        }

                        var oneDriveStorageRules = oneDriveRule.Where(r => r.OneDriveRule.StoragePolicyId == storageId).Select(r => { r.OneDriveRule.Id = r.Id; return r.OneDriveRule; }).ToList();
                        if (oneDriveStorageRules.Any())
                        {
                            logger.Info($"SOStorage [{storageId}] ODFB rules:[{string.Join(",", oneDriveStorageRules.Select(r => r.Id))}]");
                            hasStorageRules.AddRange(oneDriveStorageRules);
                        }

                        var physicalStorageRules = physicalRule.Where(r => r.PhysicalRule.StoragePolicyId == storageId).Select(r => { r.PhysicalRule.Id = r.Id; return r.PhysicalRule; }).ToList();
                        if (physicalStorageRules.Any())
                        {
                            logger.Info($"SOStorage [{storageId}] Physical rules:[{string.Join(",", physicalStorageRules.Select(r => r.Id))}]");
                            hasStorageRules.AddRange(physicalStorageRules);
                        }

                        var teamsStorageRule = teamsRule.Where(r => r.TeamsRule.StoragePolicyId == storageId).Select(r => { r.TeamsRule.Id = r.Id; return r.TeamsRule; }).ToList();
                        if (teamsStorageRule.Any())
                        {
                            logger.Info($"SOStorage [{storageId}] teams rules:[{string.Join(",", teamsStorageRule.Select(r => r.Id))}]");
                            hasStorageRules.AddRange(teamsStorageRule);
                        }

                        logger.Info($"SOStorage [{storageId}] associated rules(by source) count is:{hasStorageRules.Count}");
                        var ruleEnableRetention = false;
                        foreach (var ruleInfo in hasStorageRules)
                        {
                            if (ruleInfo.IsEnableStoreContentRetention)
                            {
                                ruleEnableRetention = true;
                                logger.Info($"SOStorage [{storageId}], Rule [{ruleInfo.Id}] is enable retention. ");
                                if (!deviceList.Contains(storageId))
                                {
                                    deviceList.Add(storageId);
                                    break;
                                }
                            }

                        }
                        if (!ruleEnableRetention)
                        {
                            logger.Info($"SOStorage [{storageId}], No rule is enable retention.");
                        }
                    }
                }


                if (deviceList.Count == 0)
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, "RM_Job_PruneArchivedData_NoRule");
                    jobStatus = JobStatus.Finished;
                    return jobId;
                }
                int currentIndex = 0;
                Dictionary<string, List<ArchiverPruningJob>> needRetentionSitesJob = new Dictionary<string, List<ArchiverPruningJob>>();
                var indeDevice = StorageDeviceService.GetIndexDevice(false);
                if (indeDevice == null)
                {
                    logger.Error("SOCannot find inde Device.");
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped);
                    jobStatus = JobStatus.Finished;
                    return jobId;
                }
                try
                {
                    foreach (var l in deviceList)
                    {
                        GenerateSiteRetentionInfo(l, needRetentionSitesJob, indeDevice, isSimulateJob);
                    }
                }
                catch (LicenseMismatchOfAvePointStorageException e)
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                    jobStatus = JobStatus.Failed;
                    return jobId;
                }
                var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
                if (needRetentionSitesJob.Count > 0)
                {
                    var runningSiteUrls = RMJobService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, needRetentionSitesJob.Select(job => job.Key), true);
                    needRetentionSitesJob = FilterRetentionCanrunUrl(needRetentionSitesJob, runningSiteUrls);

                    if (needRetentionSitesJob.Count == 0)
                    {
                        logger.Info("SOAll sites are running other job.");
                        jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        jobStatus = JobStatus.Finished;
                        return jobId;
                    }
                    else
                    {
                        logger.Info($"SOThe sites count is {needRetentionSitesJob.Count}");
                        jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, null, null, null, GenerateRetentionExtensionInfo(needRetentionSitesJob.Keys.ToList()));
                        SubJobDao.UpdateSubJobCount(jobId, needRetentionSitesJob.Count);
                    }

                    var EnqueueSubJob = StartEnqueueSubJobForRetention(jobId);

                    foreach (var siteRetain in needRetentionSitesJob)
                    {
                        var orderedRetentionList = siteRetain.Value.OrderBy(j => j.ArchiverBackupTime).ToList();
                        string siteRetentionInfo = SerializerHelper.SerializeByDataContractSerializer(orderedRetentionList);
                        string subJobId = CreateSubJobForRetentionV2(jobId, currentIndex, jobType, needRetentionSitesJob.Count, siteRetentionInfo, currentIndex < subJobCountInConfigFile, siteRetain.Key);
                        logger.Info($"SOStart retention job ,jobid:{subJobId},site url:{siteRetain.Key}");
                        if (currentIndex < subJobCountInConfigFile)
                        {
                            EnqueueSubJob(new JobQueueMessage()
                            {
                                JobId = subJobId,
                                RunBy = JobRunBy.Control,
                                JobType = jobType,
                                CommandLine = string.Format("{0} {1}", jobType, subJobId),
                            });
                        }
                        currentIndex++;
                    }
                }
                else
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_SkipNoDataRetention");
                    jobStatus = JobStatus.Finished;
                }


                return jobId;
            }
            finally
            {
                if (!isSimulateJob)
                {
                    if (jobStatus == JobStatus.Failed)
                    {
                        logger.Debug("skipped simulate RealRunArchiverRetentionJobAsync due to real job failed.");
                    }
                    else
                    {
                        logger.Debug("start simulate RealRunArchiverRetentionJobAsync");
                        await RealRunArchiverRetentionJobAsync(jobRunBy, jobRunByUser, true, jobId);
                        logger.Debug("start simulate RealRunArchiverRetentionJobAsync finished");
                    }
                }
                else
                {
                    logger.Debug($"Update RMRetentionSimulateInfos, SourceFlag:{SourceFlag.SharePoint}");


                    var simulateInfo = RententionInfosDao.GetAll().FirstOrDefault(r => r.SourceFlag == (int)SourceFlag.SharePoint);
                    if (simulateInfo != null)
                    {
                        simulateInfo.RetentionJobId = jobId;
                        simulateInfo.MainRetentionJobId = mainRetentionJobId;
                        simulateInfo.JobStatus = (int)jobStatus;
                        simulateInfo.MergeReportState = jobStatus == JobStatus.Wait ? (int)MergeIndexState.None : (int)MergeIndexState.Succeed;
                        RententionInfosDao.AddOrUpdateRetentionInfo(simulateInfo);
                    }
                }
            }
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunArchiverRetentionJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunGDriveArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool isSimulateJob = false, string previousJobId = "")
        {
            logger.Info($"Start google drive archiver Retention job.");

            string jobId = string.Empty;
            JobType jobType = JobType.GoogleArchiverRetention;

            var mJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            if (mJobs.Count > 0)
            {
                foreach (var job in mJobs)
                {
                    if (!job.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
            }

            var phDtos = await StorageDeviceService.GetAllStorageDeviceNotPagedAsync();
            var deviceList = new List<string>();
            var needCheckRulePolicyDeviceList = new List<string>();
            foreach (var pro in phDtos)
            {
                if (pro.SetupDataRetention && !deviceList.Contains(pro.Id))
                {
                    deviceList.Add(pro.Id);
                }
                else if (!needCheckRulePolicyDeviceList.Contains(pro.Id))
                {
                    needCheckRulePolicyDeviceList.Add(pro.Id);
                }
            }

            if (needCheckRulePolicyDeviceList.Count > 0)
            {
                GetAvailableDeviceList(ref deviceList, needCheckRulePolicyDeviceList);
            }

            if (deviceList.Count == 0)
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, "RM_Job_PruneArchivedData_NoRule");
                return jobId;
            }

            Dictionary<string, List<ArchiverPruningJob>> needRetentionDrivesJob = new();
            var indexDevice = StorageDeviceService.GetIndexDevice(false);
            if (indexDevice == null)
            {
                logger.Error("Cannot find index Device.");
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped);
                return jobId;
            }

            try
            {
                foreach (var device in deviceList)
                {
                    GenerateGDriveRetentionInfo(device, needRetentionDrivesJob, indexDevice, isSimulateJob);
                }
            }
            catch (LicenseMismatchOfAvePointStorageException e)
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            jobId = await SeparateJobForGDriveRetention(jobType, needRetentionDrivesJob, jobRunByUser);

            return jobId;
        }

        private void GetAvailableDeviceList(ref List<string> deviceList, List<string> needCheckRulePolicyDeviceList)
        {
            var allRules = RuleManagerService.GetRulesFromRecords();
            var googleRule = allRules.Where(r => r.GoogleDriveRule is { SOFilters.Count: > 0 }).ToList();
            logger.Info($"Need check rule level retention:[{string.Join(",", needCheckRulePolicyDeviceList)}]");
            foreach (var storageId in needCheckRulePolicyDeviceList)
            {
                logger.Info($"Storage [{storageId}] process start.");
                var hasStorageRules = new List<Rule>();

                var googleStorageRule = googleRule.Where(r => r.GoogleDriveRule.StoragePolicyId == storageId)
                    .Select(r =>
                    {
                        r.GoogleDriveRule.Id = r.Id;
                        return r.GoogleDriveRule;
                    }).ToList();
                if (googleStorageRule.Any())
                {
                    logger.Info(
                        $"Storage [{storageId}] Google Drive rules:[{string.Join(",", googleStorageRule.Select(r => r.Id))}]");
                    hasStorageRules.AddRange(googleStorageRule);
                }

                logger.Info($"Storage [{storageId}] associated rules(by source) count is:{hasStorageRules.Count}");
                var ruleEnableRetention = false;
                foreach (var ruleInfo in hasStorageRules.Where(ruleInfo => ruleInfo.IsEnableStoreContentRetention))
                {
                    ruleEnableRetention = true;
                    logger.Info($"Storage [{storageId}], Rule [{ruleInfo.Id}] is enable retention. ");
                    if (!deviceList.Contains(storageId))
                    {
                        deviceList.Add(storageId);
                        break;
                    }
                }

                if (!ruleEnableRetention)
                {
                    logger.Info($"Storage [{storageId}], No rule is enable retention.");
                }
            }
        }

        private List<string> GetGoogleRetentionNode()
        {
            List<string> res = new List<string>();
            var runningJobs = JMDao.GetRunningJobs([JobType.GoogleArchiverRetention]);
            foreach (var job in runningJobs)
            {
                try
                {
                    var jobExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiveJobMonitorExtension>(job.JobConflictExtension);
                    res.AddRange(jobExtension.SiteUrls ?? new());
                }
                catch (Exception e)
                {
                    logger.Error($"Check google drive retention running job failed, error:{e}");
                }
            }
            return res;
        }

        private async Task<string> SeparateJobForGDriveRetention(JobType jobType,
            Dictionary<string, List<ArchiverPruningJob>> needRetentionDrivesJob, string jobRunByUser)
        {
            int currentIndex = 0;
            string jobId = string.Empty;
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            if (needRetentionDrivesJob.Count > 0)
            {
                var runningGDrives = GetGoogleRetentionNode();
                var runningGGDisposalJob = await RMJobService.GetRunningDriveNodeIds([JobType.GoogleRecordsDisposal]);
                needRetentionDrivesJob = FilterRetentionCanrunUrl(needRetentionDrivesJob, runningGDrives.Concat(runningGGDisposalJob).ToList());

                if (needRetentionDrivesJob.Count == 0)
                {
                    logger.Info("All google drive are running other job.");
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                else
                {
                    logger.Info($"The google drive count is {needRetentionDrivesJob.Count}");
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, null, null, null,
                        GenerateGDriveRetentionExtensionInfo(needRetentionDrivesJob.Keys.ToList()));
                    SubJobDao.UpdateSubJobCount(jobId, needRetentionDrivesJob.Count);
                }

                foreach (var driveRetain in needRetentionDrivesJob)
                {
                    var orderedRetentionList = driveRetain.Value.OrderBy(j => j.ArchiverBackupTime).ToList();
                    string driveRetentionInfo = SerializerHelper.SerializeByDataContractSerializer(orderedRetentionList);
                    string subJobId = CreateSubJobForRetention(jobId, currentIndex, jobType,
                        needRetentionDrivesJob.Count, driveRetentionInfo, currentIndex < subJobCountInConfigFile);
                    logger.Info($"Google drive retention job ,jobid:{subJobId}, drive id:{driveRetain.Key}");
                    if (currentIndex < subJobCountInConfigFile)
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }

                    currentIndex++;
                }
            }
            else
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_SkipNoDataRetention");
            }

            return jobId;
        }

        private string CreateOrUpdateMainRetentionSimulateJob(long nextRunJobDate)
        {
            try
            {
                var mainSimulateInfo = RententionInfosDao.GetAll().FirstOrDefault(r => r.SourceFlag == (int)SourceFlag.All);
                if (mainSimulateInfo == null || mainSimulateInfo.JobStatus != (int)JobStatus.Wait)
                {
                    var mainJobId = RMJobService.GenerateJobId(JobType.ArchiverRetentionSimulateMain);
                    var lastRetentionJobId = mainSimulateInfo != null ? mainSimulateInfo.RetentionJobId : string.Empty;
                    mainSimulateInfo = new RMRetentionSimulateInfos()
                    {
                        SourceFlag = (int)SourceFlag.All,
                        FileNumber = 0,
                        DataSize = 0,
                        LastRunJobDate = DateTime.UtcNow.Ticks,
                        NextRunJobDate = nextRunJobDate,
                        RetentionJobId = mainJobId,
                        LastRetentionJobId = lastRetentionJobId,
                        JobStatus = (int)JobStatus.Wait,
                        MergeReportState = (int)MergeIndexState.None,
                    };
                    RententionInfosDao.AddOrUpdateRetentionInfo(mainSimulateInfo);
                }
                return mainSimulateInfo.RetentionJobId;
            }
            catch (Exception e)
            {
                logger.Warn($"Failed to CreateOrUpdateMainRetentionSimulateJob. error:{e}");
                return null;
            }
        }

        private List<string> GetRunningArchiverJobSiteUrlOfTeamsJob(Dictionary<string, List<ArchiverPruningJob>> needRetentionNodes)
        {
            Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> sites = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionBySiteUrls(needRetentionNodes.Keys.ToList());
            Dictionary<string, List<string>> searchDic = new Dictionary<string, List<string>>();
            foreach (var item in sites)
            {
                searchDic[item.Key.Name] = item.Value.Select(site => site.url).ToList();
            }
            var runningJobs = RMJobService.GetRunningTeamsArchiverJobSiteUrl(JobTypeConstants.ArchiveTeamsConflictType, true, searchDic);
            return runningJobs.Values.SelectMany(url => url).Distinct().ToList();
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunArchiverRetentionJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunTeamsArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start Teams archiver retention job, run by [{jobRunBy}].");

            string jobId = string.Empty;
            JobType jobType = JobType.TeamsArchiverRetention;

            var mJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            if (mJobs.Count > 0)
            {
                foreach (var job in mJobs)
                {
                    if (!job.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
            }
            var phDtos = await StorageDeviceService.GetAllStorageDeviceNotPagedAsync();
            var deviceList = new List<string>();
            var needCheckRulePolicyDeviceList = new List<string>();
            foreach (var pro in phDtos)
            {
                if (pro.SetupDataRetention && !deviceList.Contains(pro.Id))
                {
                    deviceList.Add(pro.Id);
                }
                else if (!needCheckRulePolicyDeviceList.Contains(pro.Id))
                {
                    needCheckRulePolicyDeviceList.Add(pro.Id);
                }
            }

            if (needCheckRulePolicyDeviceList.Count > 0)
            {
                var allRules = RuleManagerService.GetRulesFromRecords();
                var teamsRule = allRules.Where(r => r.TeamsRule != null && r.TeamsRule.SOFilters != null && r.TeamsRule.SOFilters.Count > 0).ToList();

                logger.Info($"Need check rule level retention:[{string.Join(",", needCheckRulePolicyDeviceList)}]");
                foreach (var storageId in needCheckRulePolicyDeviceList)
                {
                    logger.Info($"Storage [{storageId}] process start.");
                    var hasStorageRules = new List<Rule>();

                    var teamsStorageRules = teamsRule.Where(r => r.TeamsRule.StoragePolicyId == storageId).Select(r => { r.TeamsRule.Id = r.Id; return r.TeamsRule; }).ToList();
                    if (teamsStorageRules.Any())
                    {
                        logger.Info($"Storage [{storageId}] Teams rules:[{string.Join(",", teamsStorageRules.Select(r => r.Id))}]");
                        hasStorageRules.AddRange(teamsStorageRules);
                    }

                    logger.Info($"Storage [{storageId}] associated rules(by source) count is:{hasStorageRules.Count}");
                    var ruleEnableRetention = false;
                    foreach (var ruleInfo in hasStorageRules)
                    {
                        if (ruleInfo.IsEnableStoreContentRetention)
                        {
                            ruleEnableRetention = true;
                            logger.Info($"Storage [{storageId}], Rule [{ruleInfo.Id}] is enable retention. ");
                            if (!deviceList.Contains(storageId))
                            {
                                deviceList.Add(storageId);
                                break;
                            }
                        }
                    }
                    if (!ruleEnableRetention)
                    {
                        logger.Info($"Storage [{storageId}], No rule is enable retention.");
                    }
                }
            }

            if (deviceList.Count == 0)
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, "RM_Job_PruneArchivedData_NoRule");
                return jobId;
            }

            int currentIndex = 0;
            Dictionary<string, List<ArchiverPruningJob>> retentionTeamsJobMapping = new Dictionary<string, List<ArchiverPruningJob>>();
            var indeDevice = StorageDeviceService.GetIndexDevice(false);
            if (indeDevice == null)
            {
                logger.Error("Cannot find inde Device.");
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped);
                return jobId;
            }
            try
            {
                foreach (var l in deviceList)
                {
                    GenerateTeamsRetentionInfo(l, retentionTeamsJobMapping, indeDevice);
                }
            }
            catch (LicenseMismatchOfAvePointStorageException e)
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            if (retentionTeamsJobMapping.Count > 0)
            {
                var runningTeamsGroup = RMJobService.GetRunningTeamsArchiverJobSiteUrl(JobTypeConstants.ArchiveTeamsConflictType,
                    false, retentionTeamsJobMapping.Keys.ToDictionary(key => key, key => new List<string>()));
                retentionTeamsJobMapping = FilterRetentionCanrunUrl(retentionTeamsJobMapping, runningTeamsGroup.Keys.ToList());
                if (retentionTeamsJobMapping.Count == 0)
                {
                    logger.Info("All teams group are running other job.");
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                else
                {
                    logger.Info($"The teams group count is {retentionTeamsJobMapping.Count}");
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, null, null, null, GenerateTeamsRetentionExtensionInfo(retentionTeamsJobMapping.Keys.ToList()));
                    SubJobDao.UpdateSubJobCount(jobId, retentionTeamsJobMapping.Count);
                }

                var EnqueueSubJob = StartEnqueueSubJobForRetention(jobId);

                foreach (var retentionInfo in retentionTeamsJobMapping)
                {
                    var orderedRetentionList = retentionInfo.Value.OrderBy(j => j.ArchiverBackupTime).ToList();
                    string siteRetentionInfo = SerializerHelper.SerializeByDataContractSerializer(orderedRetentionList);
                    string subJobId = CreateSubJobForRetention(jobId, currentIndex, jobType, retentionTeamsJobMapping.Count, siteRetentionInfo, currentIndex < subJobCountInConfigFile);
                    logger.Info($"Start retention job ,jobid:{subJobId},site url:{retentionInfo.Key}");
                    if (currentIndex < subJobCountInConfigFile)
                    {
                        EnqueueSubJob(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    currentIndex++;
                }
            }
            else
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_SkipNoDataRetention");
            }
            return jobId;
        }
        private Action<JobQueueMessage> StartEnqueueSubJobForRetention(string mainJobId)
        {
            var enableSuperPriorityJobQueue = bool.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.ENABLE_SUPER_PRIORITY_JOB_QUEUE)?.Value, out var enableSuperQueue) && enableSuperQueue;
            string superJobQueueName = null;
            if (enableSuperPriorityJobQueue)
            {
                superJobQueueName = RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.SUPER_PRIORITY_JOB_QUEUE_NAME)?.Value;
                if (string.IsNullOrEmpty(superJobQueueName))
                {
                    superJobQueueName = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.HIGHEST_PRIORITY_JOB_QUEUE_NAME];
                    if (string.IsNullOrEmpty(superJobQueueName))
                    {
                        logger.Error("Enable highest job queue, but not config for it");
                        RMJobService.UpdateJobStatus(mainJobId, JobStatus.Failed, "RM_JS_DAM_RunJob_Failed");
                        return null;
                    }
                }
                else
                {
                    logger.Info($"Custom highest job queue name: {superJobQueueName}");
                }
            }

            var funcEnqueue = (JobQueueMessage message) =>
            {
                if (enableSuperPriorityJobQueue)
                {
                    logger.Info($"Start to send {message.JobId} to highest job queue {superJobQueueName}");
                    JobQueueService.HandleCustomerMessage(message, superJobQueueName);
                }
                else
                {
                    JobQueueService.HandleMessage(message);
                }
            };
            return funcEnqueue;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunArchiverRetentionJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunEXOArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start EXO archiver retention job, run by [{jobRunBy}].");

            string jobId = string.Empty;
            JobType jobType = JobType.EXOArchiverRetention;

            var mJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            if (mJobs.Count > 0)
            {
                foreach (var job in mJobs)
                {
                    if (!job.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
            }
            var phDtos = await StorageDeviceService.GetAllStorageDeviceNotPagedAsync();
            var deviceList = new List<string>();
            var needCheckRulePolicyDeviceList = new List<string>();
            foreach (var pro in phDtos)
            {
                if (pro.SetupDataRetention && !deviceList.Contains(pro.Id))
                {
                    deviceList.Add(pro.Id);
                }
                else if (!needCheckRulePolicyDeviceList.Contains(pro.Id))
                {
                    needCheckRulePolicyDeviceList.Add(pro.Id);
                }
            }

            if (needCheckRulePolicyDeviceList.Count > 0)
            {
                var allRules = RuleManagerService.GetRulesFromRecords();
                var teamsRule = allRules.Where(r => r.TeamsRule != null && r.TeamsRule.SOFilters != null && r.TeamsRule.SOFilters.Count > 0).ToList();

                logger.Info($"Need check rule level retention:[{string.Join(",", needCheckRulePolicyDeviceList)}]");
                foreach (var storageId in needCheckRulePolicyDeviceList)
                {
                    logger.Info($"Storage [{storageId}] process start.");
                    var hasStorageRules = new List<Rule>();

                    var teamsStorageRules = teamsRule.Where(r => r.TeamsRule.StoragePolicyId == storageId).Select(r => { r.TeamsRule.Id = r.Id; return r.TeamsRule; }).ToList();
                    if (teamsStorageRules.Any())
                    {
                        logger.Info($"Storage [{storageId}] Teams rules:[{string.Join(",", teamsStorageRules.Select(r => r.Id))}]");
                        hasStorageRules.AddRange(teamsStorageRules);
                    }

                    logger.Info($"Storage [{storageId}] associated rules(by source) count is:{hasStorageRules.Count}");
                    var ruleEnableRetention = false;
                    foreach (var ruleInfo in hasStorageRules)
                    {
                        if (ruleInfo.IsEnableStoreContentRetention)
                        {
                            ruleEnableRetention = true;
                            logger.Info($"Storage [{storageId}], Rule [{ruleInfo.Id}] is enable retention. ");
                            if (!deviceList.Contains(storageId))
                            {
                                deviceList.Add(storageId);
                                break;
                            }
                        }
                    }
                    if (!ruleEnableRetention)
                    {
                        logger.Info($"Storage [{storageId}], No rule is enable retention.");
                    }
                }
            }

            if (deviceList.Count == 0)
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, "RM_Job_PruneArchivedData_NoRule");
                return jobId;
            }

            int currentIndex = 0;
            Dictionary<string, List<ArchiverPruningJob>> retentionTeamsJobMapping = new Dictionary<string, List<ArchiverPruningJob>>();
            var indeDevice = StorageDeviceService.GetIndexDevice(false);
            if (indeDevice == null)
            {
                logger.Error("Cannot find inde Device.");
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped);
                return jobId;
            }
            try
            {
                foreach (var l in deviceList)
                {
                    GenerateExchangeRetentionInfo(l, retentionTeamsJobMapping, indeDevice);
                }
            }
            catch (LicenseMismatchOfAvePointStorageException e)
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            if (retentionTeamsJobMapping.Count > 0)
            {
                var runningTeamsGroup = RMJobService.GetRunningTeamsArchiverJobSiteUrl(new() { JobType.EXOArchiverRetention, JobType.TeamsArchiverBackup, JobType.SpecifyTeamsArchiverBackup },
                false, retentionTeamsJobMapping.Select(map => map.Key).ToDictionary(key => key, key => new List<string>()));
                retentionTeamsJobMapping = FilterRetentionCanrunUrl(retentionTeamsJobMapping, runningTeamsGroup.Keys.ToList());
                if (retentionTeamsJobMapping.Count == 0)
                {
                    logger.Info("All teams group are running other job.");
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                else
                {
                    logger.Info($"The teams group count is {retentionTeamsJobMapping.Count}");
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, null, null, null, GenerateTeamsRetentionExtensionInfo(retentionTeamsJobMapping.Keys.ToList()));
                    SubJobDao.UpdateSubJobCount(jobId, retentionTeamsJobMapping.Count);
                }

                var EnqueueSubJob = StartEnqueueSubJobForRetention(jobId);

                foreach (var retentionInfo in retentionTeamsJobMapping)
                {
                    var orderedRetentionList = retentionInfo.Value.OrderBy(j => j.ArchiverBackupTime).ToList();
                    string siteRetentionInfo = SerializerHelper.SerializeByDataContractSerializer(orderedRetentionList);
                    string subJobId = CreateSubJobForRetention(jobId, currentIndex, jobType, retentionTeamsJobMapping.Count, siteRetentionInfo, currentIndex < subJobCountInConfigFile);
                    logger.Info($"Start retention job ,jobid:{subJobId},site url:{retentionInfo.Key}");
                    if (currentIndex < subJobCountInConfigFile)
                    {
                        EnqueueSubJob(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    currentIndex++;
                }
            }
            else
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_SkipNoDataRetention");
            }
            return jobId;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunArchiverRetentionJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunFSRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool isSimulateJob, string previousJobId)
        {
            logger.Info($"Start fs Retention job. isSimulate:{isSimulateJob}");
            string jobId = string.Empty;
            JobType jobType = isSimulateJob ? JobType.FSRetainSimulate : JobType.FSRetain;
            JobStatus jobStatus = JobStatus.Wait;

            long nextRunJobDate = 0;
            string mainRetentionJobId = "";
            try
            {
                if (isSimulateJob)
                {
                    nextRunJobDate = GetNextRetentionRunTime();
                    if (nextRunJobDate == 0)
                    {
                        logger.Info($"Skipped run archiver retention silumate job due to next run time is 0");
                        return null;
                    }
                    mainRetentionJobId = CreateOrUpdateMainRetentionSimulateJob(nextRunJobDate);
                    if (mainRetentionJobId.IsNullOrEmpty())
                    {
                        logger.Info($"Skipped run archiver retention silumate job due to main rentention jobid is null");
                        return null;
                    }
                    RententionInfosDao.AddOrUpdateRetentionInfo(new RMRetentionSimulateInfos()
                    {
                        SourceFlag = (int)SourceFlag.FileSystem,
                        FileNumber = 0,
                        DataSize = 0,
                        LastRunJobDate = DateTime.UtcNow.Ticks,
                        NextRunJobDate = nextRunJobDate,
                        LastRetentionJobId = previousJobId,
                        RetentionJobId = jobId,
                        MainRetentionJobId = mainRetentionJobId,
                        JobStatus = (int)jobStatus,
                        MergeReportState = jobStatus == JobStatus.Wait ? (int)MergeIndexState.None : (int)MergeIndexState.Succeed,
                    });

                    if (previousJobId.IsNotNullOrEmpty())
                    {
                        var previousJob = await RMJobService.GetJobAsync(previousJobId);
                        while (!RA.Common.JobService.JobServiceUtility.IsFinalState((int)previousJob.Status))
                        {
                            logger.Info($"awaiting real job finish before run retention simualte job. JobId:{previousJobId}, JobStatus:{previousJob.Status}");
                            await Task.Delay(10000);

                            previousJob = await RMJobService.GetJobAsync(previousJobId);
                        }
                        logger.Info($"awaiting real job finished");
                    }

                    logger.Info($"RealRunFSRetentionJobAsync, nextRunJobDate:{nextRunJobDate}");
                }
                var phDtos = await StorageDeviceService.GetAllStorageDeviceNotPagedAsync();
                var deviceList = new List<string>();
                var needCheckRulePolicyDeviceList = new List<string>();
                foreach (var pro in phDtos)
                {
                    if (pro.SetupDataRetention && !deviceList.Contains(pro.Id))
                    {
                        deviceList.Add(pro.Id);
                    }
                }
                if (deviceList.Count == 0)
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, "RM_Job_PruneArchivedData_NoRule");
                    jobStatus = JobStatus.Finished;
                    return jobId;
                }
                int currentIndex = 0;
                Dictionary<string, List<ArchiverPruningJob>> needRetentionJob = new Dictionary<string, List<ArchiverPruningJob>>();
                var indeDevice = StorageDeviceService.GetIndexDevice(false);
                if (indeDevice == null)
                {
                    logger.Error("Cannot find inde Device.");
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped);
                    jobStatus = JobStatus.Finished;
                    return jobId;
                }
                try
                {
                    foreach (var l in deviceList)
                    {
                        GenerateFSRetentionInfo(l, needRetentionJob, indeDevice, isSimulateJob);
                    }
                }
                catch (LicenseMismatchOfAvePointStorageException e)
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                    jobStatus = JobStatus.Failed;
                    return jobId;
                }
                catch (FSNotSurpportAvePointStorageException e)
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                    jobStatus = JobStatus.Failed;
                    return jobId;
                }
                var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
                var mJobs = RMJobService.GetRunningJobs(JobTypeConstants.FSArchiveConflictType);
                if (mJobs.Count > 0)
                {
                    foreach (var job in mJobs)
                    {
                        if (job.JobType == (int)JobType.FSRetain)
                        {
                            jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                            RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_FSDisposal_JobSkip");
                            return jobId;
                        }
                        if (!job.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                        {
                            jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                            RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                            return jobId;
                        }
                    }
                }
                if (needRetentionJob.Count > 0)
                {
                    logger.Info($"The sites count is {needRetentionJob.Count}");
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, null, null, null, GenerateRetentionExtensionInfo(needRetentionJob.Keys.ToList()));
                    SubJobDao.UpdateSubJobCount(jobId, needRetentionJob.Count);
                    foreach (var fsRetain in needRetentionJob)
                    {
                        ConnectionDto connection = new ConnectionDto();
                        try
                        {
                            connection = await FSRegisterService.GetConnectionByIdAsync(new Guid(fsRetain.Key));
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Get connection failed in retain,error:{e}");
                        }
                        var orderedRetentionList = fsRetain.Value.OrderBy(j => j.ArchiverBackupTime).ToList();
                        var isEnableMoveToAnotherLocation = RMKeyValueDao.IsEnableMoveToAnotherLocation();
                        var isEnableCopyToAnotherLocation = RMKeyValueDao.IsEnableCopyToAnotherLocation();
                        orderedRetentionList.ForEach(a =>
                        {
                            a.UNCPath = connection.UNCPath;
                            a.IsEnableMoveToAnotherLocation = isEnableMoveToAnotherLocation;
                            a.IsEnableCopyToAnotherLocation = isEnableCopyToAnotherLocation;
                        });
                        string siteRetentionInfo = SerializerHelper.SerializeByJsonSerializer(orderedRetentionList);
                        string subJobId = CreateSubJobForRetention(jobId, currentIndex, jobType, needRetentionJob.Count, siteRetentionInfo, currentIndex < subJobCountInConfigFile);
                        logger.Info($"Start fs retention job ,jobid:{subJobId},site url:{fsRetain.Key}");
                        if (currentIndex < subJobCountInConfigFile)
                        {
                            var masterIndex = FSMasterIndexDao.GetConnectionInfos(fsRetain.Key);
                            string agentId = string.Empty;
                            if (masterIndex != null && masterIndex.Count > 0)
                            {
                                agentId = masterIndex[0].AgentId ?? "";
                            }

                            var hybridJobType = !isSimulateJob
                                     ? AvePoint.Hybrid.Contract.JobType.FSRetain
                                     : AvePoint.Hybrid.Contract.JobType.FSRetainSimulate;

                            HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                            {
                                JobId = subJobId,
                                JobType = hybridJobType,
                                TenantId = TenantLocalValue.LogonGroupId,
                                AgentId = agentId,
                            }, connection == null ? new Guid() : connection.GroupId);
                        }
                        currentIndex++;
                    }
                }
                else
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_SkipNoDataRetention");
                    jobStatus = JobStatus.Finished;
                }
                return jobId;
            }
            finally
            {
                if (!isSimulateJob)
                {
                    if (jobStatus == JobStatus.Failed)
                    {
                        logger.Info($"Skip running simulate FSRetentionJobAsync due to the real job failed.");
                    }
                    else
                    {
                        logger.Info($"running simulate FSRetentionJobAsync");
                        await RealRunFSRetentionJobAsync(jobRunBy, jobRunByUser, true, jobId);
                        logger.Info($"running simulate FSRetentionJobAsync finished");
                    }
                }
                else
                {
                    var simulateInfo = RententionInfosDao.GetAll().FirstOrDefault(r => r.SourceFlag == (int)SourceFlag.FileSystem);
                    if (simulateInfo != null)
                    {
                        simulateInfo.RetentionJobId = jobId;
                        simulateInfo.MainRetentionJobId = mainRetentionJobId;
                        simulateInfo.JobStatus = (int)jobStatus;
                        simulateInfo.MergeReportState = jobStatus == JobStatus.Wait ? (int)MergeIndexState.None : (int)MergeIndexState.Succeed;
                        RententionInfosDao.AddOrUpdateRetentionInfo(simulateInfo);
                    }
                }
            }
        }
        private string GenerateRetentionExtensionInfo(List<string> canRunUrls)
        {
            ArchiveJobMonitorExtension extension = new ArchiveJobMonitorExtension();
            extension.IsGroupLevelArchive = false;
            extension.SiteUrls = canRunUrls;
            extension.ConflictNodeLevel = ConflictNodeLevel.SiteCollection;
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }

        private string GenerateGDriveRetentionExtensionInfo(List<string> canRunUrls)
        {
            ArchiveJobMonitorExtension extension = new ArchiveJobMonitorExtension
            {
                IsGroupLevelArchive = false,
                SiteUrls = canRunUrls,
                ConflictNodeLevel = ConflictNodeLevel.GDrive
            };
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }
        private string GenerateTeamsRetentionExtensionInfo(List<string> canRunUrls)
        {
            ArchiveJobMonitorExtension extension = new ArchiveJobMonitorExtension();
            extension.IsGroupLevelArchive = false;
            extension.ConflictNodeLevel = ConflictNodeLevel.ArchiverTeamsRetention;
            extension.teamsUrls = canRunUrls;
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }
        private Dictionary<string, List<ArchiverPruningJob>> FilterRetentionCanrunUrl(Dictionary<string, List<ArchiverPruningJob>> needRetentionNodes, List<string> runningUrl)
        {
            Dictionary<string, List<ArchiverPruningJob>> result = needRetentionNodes.ToDictionary();

            foreach (var url in runningUrl)
            {
                foreach (var node in needRetentionNodes.Keys.OrderByDescending(key => key.Length).ToList())
                {
                    if (RuleSPTreeUtil.IsPrefixWithSlash(url, node) || RuleSPTreeUtil.IsPrefixWithSlash(node, url))
                    {
                        result.Remove(node);
                    }
                    else
                    {
                        logger.Info($"SOThe site {node} is not running job.");
                    }
                }
            }

            return result;
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunArchiverDedupJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunArchiverDedupJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start archiver dedup job.");

            string jobId = string.Empty;
            JobType jobType = JobType.ArchiverDeduplication;

            jobId = RMJobService.CreateJob(jobType, jobRunByUser);

            var mJobs = RMJobService.GetRunningJobs(JobTypeConstants.ArchiverIndexConflictJobTypes);
            if (mJobs.Count > 0)
            {
                foreach (var job in mJobs)
                {
                    if (!job.Id.Equals(jobId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
            }

            Dictionary<string, List<string>> undedupSiteInfoes = null;
            var siteURLs = GetDedupSiteCollections();
            if (siteURLs == null)
            {
                undedupSiteInfoes = await ArchiverSiteMasterIndexDao.GetAllUnDedupArchiverSiteMasterIndexesAsync();
            }
            else if (siteURLs.Count > 0)
            {
                undedupSiteInfoes = ArchiverSiteMasterIndexDao.GetAllUnDedupArchiverSiteMasterIndexes(siteURLs);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_DedupSkip_NoSitesInConfigFile");
                return jobId;
            }

            if (undedupSiteInfoes.Count > 0)
            {
                CreateSubJobsForDeduplication(jobId, undedupSiteInfoes);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, I18NEntity.GetString("RM_JM_DedupSkip_NoDataNeedDedup"));
            }
            return jobId;
        }
        private void CreateSubJobsForDeduplication(string mainJobId, Dictionary<string, List<string>> undedupSiteInfoes)
        {
            JobType jobType = JobType.ArchiverDeduplication;
            double subJobWeight = 100d / undedupSiteInfoes.Count;

            var currentIndex = 0;
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            SubJobDao.UpdateSubJobCount(mainJobId, undedupSiteInfoes.Count);

            foreach (var siteInfo in undedupSiteInfoes)
            {
                bool sendNow = currentIndex < subJobCountInConfigFile;
                DeduplicationSiteData deduplicationSiteData = new DeduplicationSiteData()
                {
                    SiteCollectionURL = siteInfo.Key,
                    ArchiverSiteMasterIndexIds = siteInfo.Value,
                };
                string jobParams = SerializerHelper.SerializeByJsonConvert(deduplicationSiteData);
                string subJobId = string.Format(mainJobId + "_{0:D3}", currentIndex);

                var subJob = new RMSubJob()
                {
                    Id = subJobId,
                    ParentId = mainJobId,
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)jobType,
                    Progress = 0,
                    Status = (int)JobStatus.Wait,
                    Weight = subJobWeight,
                    Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
                    JobContext = new RMJobContext() { JobId = subJobId, Settings = jobParams }
                };
                SubJobDao.CreateJob(subJob);
                logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJobWeight);

                if (sendNow)
                {
                    logger.Info($"Start the dedup sub job: {subJobId}, site url:{siteInfo.Key}");
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                currentIndex++;
            }
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.ConfigureDedupScheduleJob, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<bool> UpdateDedupSettingFile(string fileName, Stream fileStream)
        {
            var uploadedFileName = GetUploadedDedupSettingsFileName();

            if (string.IsNullOrEmpty(fileName))
            {
                if (string.IsNullOrEmpty(uploadedFileName))
                {
                    logger.Warn($"No dedup job config before.");
                }
                else
                {
                    logger.Info($"Upload dedup setting file.");
                    RMKeyValueDao.DeleteByKey(KeyName_UploadedDedupSettingsFileName);
                    RAStorageUtil.DeleteReportBlob(GetDedupSettingsFileBlobPath());
                }
            }
            else
            {
                await RMKeyValueDao.UpsertAsync(KeyName_UploadedDedupSettingsFileName, fileName);

                var buffer = new byte[1024];
                using var validationStream = new MemoryStream();
                int read;
                while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    validationStream.Write(buffer, 0, read);
                }
                validationStream.Position = 0;
                logger.Info($"update dedup setting file.");
                RAStorageUtil.UploadReportBlob(GetDedupSettingsFileBlobPath(), validationStream);
            }
            return true;
        }

        private List<string> GetDedupSiteCollections()
        {
            List<string> siteURLs = null;
            var configFileStream = DownloadDedupSettingsFileToStream(out _);
            if (configFileStream != null)
            {
                var hsList = new HashSet<string>();
                string siteUrl = null;
                var fileContent = ExcelUtil.ReadExcelWithHeader(configFileStream, 0);
                if (fileContent.Count > 0)
                {
                    var rows = fileContent.First().Value;
                    foreach (var row in rows)
                    {
                        siteUrl = row.FirstOrDefault()?.Trim();
                        if (!string.IsNullOrEmpty(siteUrl))
                        {
                            hsList.Add(siteUrl);
                        }
                    }
                }
                siteURLs = hsList.ToList();
            }
            return siteURLs;
        }

        public Dictionary<string, string> GetSavedDedupFileInfo()
        {
            long fileSize = 0;
            var fileName = GetUploadedDedupSettingsFileName();
            var savedFileBlobPath = GetDedupSettingsFileBlobPath();
            if (!string.IsNullOrEmpty(fileName) && !RAStorageUtil.TryGetReportBlobLength(savedFileBlobPath, out fileSize))
            {
                fileSize = 0;
            }
            return new Dictionary<string, string>()
            {
                { "FileName", fileName },
                { "FileSize", (fileSize / 1024.0).ToString("f2") }
            };
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.DownloadTemplate, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public string DownloadDedupTemplate()
        {
            return Path.Combine(WebUtil.GetInstallPath(), "Config", "Deduplicate Template.xlsx");
        }

        public Stream DownloadDedupSettingsFileToStream(out string filename)
        {
            var blobPath = GetDedupSettingsFileBlobPath();
            filename = GetUploadedDedupSettingsFileName();
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }
            return RAStorageUtil.DownloadReportBlobToStream(blobPath);
        }

        private const string KeyName_UploadedDedupSettingsFileName = "DedupJobConfigFile";
        private string GetUploadedDedupSettingsFileName()
        {
            return RMKeyValueDao.GetValueByKey(KeyName_UploadedDedupSettingsFileName)?.Value;
        }

        private string GetDedupSettingsFileBlobPath()
        {
            return $"{JobReportUtility.GetTenantIdentity()}/Deduplication/Deduplicate Template.xlsx";
        }
        private void GenerateSiteDeleteOrphanDatasInfo(Dictionary<string, List<ArchiverPruningJob>> needRetentionSitesJobs, StorageDeviceDto indexDevice, List<string> mainJobIds)
        {
            var indexLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);
            Dictionary<string, StorageDeviceDto> mStorageDirectory = new Dictionary<string, StorageDeviceDto>();
            foreach (string mainJobId in mainJobIds)
            {
                logger.Info($"Process delete orphan datas main job id {mainJobId}");
                var jobSubInfos = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoByMainJobId(mainJobId);
                if (jobSubInfos != null && jobSubInfos.Count > 0)
                {
                    foreach (var info in jobSubInfos)
                    {
                        if (info.MergeIndexState != (int)MergeIndexState.Succeed && info.MergeIndexState != (int)MergeIndexState.DAOMigrated)
                        {
                            logger.Info($"The job {info.SubSubJobId} is not merged index, need to process it.");
                        }
                        else
                        {
                            logger.Info($"The job {info.SubSubJobId} is merged index, skip it.");
                            continue;
                        }
                        if (!mStorageDirectory.ContainsKey(info.CurrentStorageId))
                        {
                            logger.Info($"Process delete orphan datas storage id {info.CurrentStorageId}");
                            var storage = StorageDeviceService.GetStorageDeviceById(info.CurrentStorageId, needDecryptSecert: true);
                            mStorageDirectory.Add(info.CurrentStorageId, storage);
                        }
                        var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(mStorageDirectory[info.CurrentStorageId]);
                        ArchiverRetentionMessage message = null;
                        if (info.DataFlag == (int)SourceFlag.Teams || info.DataFlag == (int)SourceFlag.Groups)
                        {
                            message = AssembleDeleteOrphanDatasMessage(info, srcLogical, indexLogical, true);
                            message.dataSourceForOrphanBlob = DataSourceForOrphanBlob.Teams;
                        }
                        else
                        {
                            message = AssembleDeleteOrphanDatasMessage(info, srcLogical, indexLogical, false);
                            message.dataSourceForOrphanBlob = DataSourceForOrphanBlob.SharePoint;
                        }
                        ArchiverPruningJob archiverPruningJob = InitPruningJob(message);
                        archiverPruningJob.RetainType = RetainType.DeleteOrphanDatas;
                        if (needRetentionSitesJobs.ContainsKey(archiverPruningJob.SiteUrl))
                        {
                            needRetentionSitesJobs[archiverPruningJob.SiteUrl].Add(archiverPruningJob);
                        }
                        else
                        {
                            needRetentionSitesJobs.Add(archiverPruningJob.SiteUrl, new List<ArchiverPruningJob>() { archiverPruningJob });
                        }
                    }
                }
                else
                {
                    var exoJobInfo = EXOArhciverSubInfo.GetAllEXOArchiverIndexSubInfoByMainJobId(mainJobId);
                    if (exoJobInfo != null && exoJobInfo.Count > 0)
                    {
                        foreach (var info in exoJobInfo)
                        {
                            if (info.MergeIndexState != (int)MergeIndexState.Succeed && info.MergeIndexState != (int)MergeIndexState.DAOMigrated)
                            {
                                logger.Info($"The exo job {info.SubSubJobId} is not merged index, need to process it.");
                            }
                            else
                            {
                                logger.Info($"The exo job {info.SubSubJobId} is merged index, skip it.");
                                continue;
                            }
                            if (!mStorageDirectory.ContainsKey(info.CurrentStorageId))
                            {
                                logger.Info($"Process delete exo orphan datas storage id {info.CurrentStorageId}");
                                var storage = StorageDeviceService.GetStorageDeviceById(info.CurrentStorageId, needDecryptSecert: true);
                                mStorageDirectory.Add(info.CurrentStorageId, storage);
                            }
                            var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(mStorageDirectory[info.CurrentStorageId]);
                            ArchiverRetentionMessage message = AssembleEXODeleteOrphanDatasMessage(info, srcLogical, indexLogical);
                            ArchiverPruningJob archiverPruningJob = InitPruningJob(message);
                            archiverPruningJob.RetainType = RetainType.DeleteOrphanDatas;
                            if (needRetentionSitesJobs.ContainsKey(archiverPruningJob.SiteUrl))
                            {
                                needRetentionSitesJobs[archiverPruningJob.SiteUrl].Add(archiverPruningJob);
                            }
                            else
                            {
                                needRetentionSitesJobs.Add(archiverPruningJob.SiteUrl, new List<ArchiverPruningJob>() { archiverPruningJob });
                            }
                        }
                    }
                }
            }
            logger.Info($"finish generate delete orphan datas job info");
        }
        private void GenerateSiteRetentionInfo(string storageId, Dictionary<string, List<ArchiverPruningJob>> needRetentionSitesJobs, StorageDeviceDto indexDevice, bool isSimulateJob)
        {
            Dictionary<string, StorageDeviceDto> mStorageDirectory = new Dictionary<string, StorageDeviceDto>();
            Dictionary<string, AvePoint.RA.DB.Model.ArchiverSiteMasterIndex> siteInfoCache = new Dictionary<string, AvePoint.RA.DB.Model.ArchiverSiteMasterIndex>();
            logger.Info($"SOProcess storage id {storageId}");
            var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId);
            mStorageDirectory.Add(storageId, storageDevice);
            var hasRetentionRule = false;
            var indexLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);

            var infos = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoByStorageIdAndSourceFlag(storageId, new List<int> { (int)SourceFlag.SharePoint, (int)SourceFlag.SharePointOnPrem, (int)SourceFlag.OneDrive, 0 });
            //var infos = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoByStorageId(storageId);
            var inProgressStoreJobs = JobMonitorService.GetRunningJobs(new List<JobType> { JobType.RecordsDisposal, JobType.OneDriveRecordsDisposal, JobType.PhysicalRecordsDisposal, JobType.ArchiverByHSMXml, JobType.RMArchiverBackup, JobType.RMEndUserArchiverBackup, JobType.SpecifySitesArchiverBackup, JobType.DiscoverOptimization, JobType.DiscoveryAOSPOptimization, JobType.TeamsArchiverBackup, JobType.TeamsRecordsDisposal, JobType.SpecifyTeamsArchiverBackup, JobType.CleanUpDuplicateDatas });
            var infoGrpupsByRule = infos.GroupBy(f => f.RuleId);

            HashSet<string> includedSiteUrls = [];
            bool isEnableCustomRetentionSettings = RMKeyValueDao.IsEnableCustomRetentionSettings();
            if (isEnableCustomRetentionSettings)
            {
                try
                {
                    // Need to copy to memory stream due to Azure Stream doesn't support seek
                    // Max file size is 5MB, so it won't cause memory issue
                    using var memStream = new MemoryStream();
                    using (var fileStream = GetCurrentRetentionSettingsFileStream().ExecuteAsyncTask())
                    {
                        fileStream.CopyToAsync(memStream).ExecuteAsyncTask();
                    }
                    try
                    {
                        memStream.Position = 0;
                        var fileContent = ExcelUtil.ReadExcel(memStream);
                        foreach (var sheet in fileContent.Values)
                        {
                            foreach (var row in sheet)
                            {
                                if (row.Length > 0 && !string.IsNullOrEmpty(row[0]))
                                    includedSiteUrls.Add(row[0].Trim().ToLower());
                            }
                        }
                    }
                    catch
                    {
                        logger.Warn("Failed to read excel file, try to read as csv.");
                        memStream.Position = 0;
                        using StreamReader sr = new(memStream, Encoding.UTF8, leaveOpen: true);
                        while (!sr.EndOfStream)
                        {
                            string csvLine = sr.ReadLine();
                            if (csvLine != null)
                            {
                                var siteUrl = CSVHelper.AnalyseCSVRow2Array(csvLine).FirstOrDefault();
                                if (!string.IsNullOrEmpty(siteUrl))
                                {
                                    includedSiteUrls.Add(siteUrl.Trim().ToLower());
                                }
                            }
                        }
                    }
                    if (includedSiteUrls.Count == 0)
                    {
                        logger.Warn("No site url found in retention settings file, skip retention job.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to read retention settings file, error: {ex}");
                }
            }

            foreach (var infoGroup in infoGrpupsByRule)
            {
                Rule? rule = null;
                var currentRuleId = infoGroup.Key;
                logger.Info($"SOProcess data by rule: {currentRuleId}");
                if (!string.IsNullOrEmpty(currentRuleId))
                {
                    var profile = MiscProfileDao.Load(currentRuleId);
                    if (profile != null)
                    {
                        try
                        {
                            rule = AvePoint.GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<Rule>(profile.Extension);
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"SODeserialize rule [{currentRuleId}] error:{e}");
                        }
                    }
                    else
                    {
                        logger.Warn($"SOThe rule maybe deleted, rule id {currentRuleId}");
                    }
                }

                foreach (var info in infoGroup)
                {
                    try
                    {
                        logger.Info($"SOProcess job id :{info.SubSubJobId}, rule id: {currentRuleId}");
                        RetentionSourceFlag retentionSource = RetentionSourceFlag.Storage;
                        var tempJobId = info.SubSubJobId?.Split("_");
                        if (tempJobId?.Length > 0)
                        {
                            var mainJobId = tempJobId.FirstOrDefault();
                            if (inProgressStoreJobs.Any(j => j.Id == mainJobId))
                            {
                                logger.Warn($"SO:{mainJobId} is running, skip this info");
                                continue;
                            }
                        }
                        try
                        {
                            RetentionRule? matchedRule = null;
                            List<RetentionRule>? ruleRetentionInfos = null;
                            bool ruleHasModified = false;
                            if (!string.IsNullOrEmpty(currentRuleId))
                            {
                                if (rule != null)
                                {
                                    Rule? configurateRetentionRule = GetRuleBySource(rule, (SourceFlag)info.SourceFlag);

                                    if (configurateRetentionRule != null)
                                    {
                                        if (configurateRetentionRule.IsEnableRetention)
                                        {
                                            logger.Warn($"SORule [{currentRuleId}], is enable archiver content retention, skip it in retention job.");
                                            continue;
                                        }
                                        else if (configurateRetentionRule.IsEnableStoreContentRetention)
                                        {
                                            var ruleModifyTime = GetRuleModifyTime(currentRuleId, MiscProfileDao.Load(currentRuleId));
                                            if (ruleModifyTime > 0 && info.RetentionTime < ruleModifyTime)
                                            {
                                                logger.Warn($"SOthe rule has been modified,job id:{info.SubSubJobId},info Retention time is:{info.RetentionTime},rule modifytime:{ruleModifyTime}");
                                                ruleHasModified = true;
                                            }
                                            ruleRetentionInfos = configurateRetentionRule.StoreContentRetentionInfos;
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"SOCan't get rule by source[{info.SourceFlag}]");
                                    }
                                }
                            }
                            else
                            {
                                logger.Info($"SOIndex info is not has rule, job id:{info.SubSubJobId}");
                            }
                            if (ruleRetentionInfos?.Count == 1 && ruleRetentionInfos.FirstOrDefault().RetentionDataTimeType == KeepDateType.ModifiedTime)
                            {
                                retentionSource = RetentionSourceFlag.Rule;
                                logger.Info("SOthis rule retention job is retention by modified time");
                                matchedRule = ruleRetentionInfos.FirstOrDefault();
                            }
                            else if (ruleRetentionInfos?.Count > 0)
                            {
                                retentionSource = RetentionSourceFlag.Rule;
                                if (ruleHasModified)
                                {
                                    logger.Info($"SOthe rule has been modified,need reset Retention count,job id:{info.SubSubJobId}");
                                    info.RetentionCount = 1;
                                    info.RetentionSource = (int)RetentionSourceFlag.Rule;
                                    if (!isSimulateJob)
                                    {
                                        ArchiverIndexSubInfoDao.UpdateAsync(info).GetAwaiter().GetResult();
                                    }
                                }
                                logger.Info($"SORule id:{currentRuleId}, rule source:{info.SourceFlag}, rule retention info:{JsonConvert.SerializeObject(ruleRetentionInfos.Select(r => new { Unit = r.ArchiveDateUnit.ToString(), r.KeepValue }))}");
                                matchedRule = GetMatchedRetentionRule(ruleRetentionInfos, info, RetentionSourceFlag.Rule, isSimulateJob);
                                if (matchedRule != null)
                                {
                                    matchedRule.RemoveOrphanedStub = matchedRule.RemoveOrphanedStub || !matchedRule.KeepOrphanedStub4CompatibilityExistingRule;
                                    logger.Info($"SOMatch retention rule by rule level, RuleId:{currentRuleId}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                }
                                else
                                {
                                    logger.Info($"SONot match retention rule by rule level, RuleId:{currentRuleId}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                }
                            }
                            else if (storageDevice.ArchiveRetentionRules?.Count == 1 && storageDevice?.ArchiveRetentionRules?.FirstOrDefault()?.RetentionDataTimeType == KeepDateType.ModifiedTime)
                            {
                                logger.Info("SOthis storage retention job is retention by modified time");
                                matchedRule = storageDevice.ArchiveRetentionRules.FirstOrDefault();
                            }
                            else if (storageDevice.SetupDataRetention)
                            {
                                if (info.RetentionTime < storageDevice.ModifyTime)
                                {
                                    logger.Info($"SOthe storage device has been modified,need reset Retention count,job id:{info.SubSubJobId}");
                                    info.RetentionCount = 1;
                                    info.RetentionSource = (int)RetentionSourceFlag.Storage;
                                    if (!isSimulateJob)
                                    {
                                        ArchiverIndexSubInfoDao.UpdateAsync(info).GetAwaiter().GetResult();
                                    }
                                }
                                logger.Info($"SONone retention infos by rule level, RuleId:{currentRuleId}, SourceFlag:{info.SourceFlag}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                matchedRule = GetMatchedRetentionRule(storageDevice.ArchiveRetentionRules, info, RetentionSourceFlag.Storage, isSimulateJob);
                            }
                            else
                            {
                                logger.Info("SONeither storage nor rule has a retention");
                            }

                            if (matchedRule != null)
                            {
                                hasRetentionRule = true;
                                ArchiverRetentionMessage? message = null;
                                logger.Info($"SOStorageId {storageId} CurrentStorageId {info.CurrentStorageId}");
                                if (!mStorageDirectory.ContainsKey(info.CurrentStorageId))
                                {
                                    var storage = StorageDeviceService.GetStorageDeviceById(info.CurrentStorageId, needDecryptSecert: true);
                                    mStorageDirectory.Add(info.CurrentStorageId, storage);
                                }
                                var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(mStorageDirectory[info.CurrentStorageId]);
                                message = AssembleRetentionMessage(info.CurrentStorageId, matchedRule, info, srcLogical, indexLogical);

                                if (isEnableCustomRetentionSettings && includedSiteUrls.Count > 0 && !includedSiteUrls.Contains(message.SiteUrl.Trim().ToLower()))
                                {
                                    logger.Info($"The site {message.SiteUrl} for jobId {message.JobId} is not in custom retention settings file, skip it.");
                                    continue;
                                }

                                // JobId 如果 AR 开头的，表示是从DAO migration 过来的数据
                                if (message.JobId.StartsWith("AR") || (message.JobId.StartsWith("EA") && info.DAOMigrated.GetValueOrDefault()))
                                {
                                    // DAO migrated 的备份数据，需要用DAO 的StoragePolicyId去级联删除，所以在这替换成 DAOStoragePolicyId
                                    if (string.IsNullOrEmpty(storageDevice.DAOStoragePolicyId))
                                    {
                                        logger.Error($"SOCan't find DAOStoragePolicyId from the archiver sub index's storage. SubInfoId: {info.Id}, storage id: {storageId}, JobId: {message.JobId}");
                                    }
                                    else
                                    {
                                        message.StoragePolicyId = storageDevice.DAOStoragePolicyId;
                                    }
                                }

                                ArchiverPruningJob archiverPruningJob = InitPruningJob(message);
                                if (isSimulateJob)
                                {
                                    if (retentionSource == RetentionSourceFlag.Rule)
                                    {
                                        archiverPruningJob.RetentionSourceName = rule?.Name;
                                    }
                                    else
                                    {
                                        archiverPruningJob.RetentionSourceName = storageDevice.Name;
                                    }
                                    archiverPruningJob.SourceFlag = info.DataFlag != 0 ? info.DataFlag : info.SourceFlag;
                                    archiverPruningJob.IsSimulateJob = isSimulateJob;
                                    archiverPruningJob.SimulateJobRunTime = GetNextRetentionRunTime();
                                }
                                if (needRetentionSitesJobs.ContainsKey(archiverPruningJob.SiteUrl))
                                {
                                    needRetentionSitesJobs[archiverPruningJob.SiteUrl].Add(archiverPruningJob);
                                }
                                else
                                {
                                    needRetentionSitesJobs.Add(archiverPruningJob.SiteUrl, new List<ArchiverPruningJob>() { archiverPruningJob });
                                }
                            }
                            else
                            {
                                logger.Info($"SONot match retention rule.");
                            }
                        }
                        catch (LicenseMismatchOfAvePointStorageException lme)
                        {
                            logger.Error($"SOLicenseMismatchOfAvePointStorageException error : {lme}");
                            throw;
                        }
                        catch (Exception e)
                        {
                            logger.Error("SOretention Error :{0}", e.ToString());
                        }

                    }
                    catch (LicenseMismatchOfAvePointStorageException lme)
                    {
                        logger.Error($"SOLicenseMismatchOfAvePointStorageException error : {lme}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"SOrun retention Error :{e}");
                    }
                }
            }
            if (!hasRetentionRule)
            {
                logger.Error("SONot match retention rule info.");
            }
        }

        private void GenerateFSRetentionInfo(string storageId, Dictionary<string, List<ArchiverPruningJob>> needRetentionJobs, StorageDeviceDto indexDevice, bool isSimulateJob)
        {
            Dictionary<string, StorageDeviceDto> mStorageDirectory = new Dictionary<string, StorageDeviceDto>();
            Dictionary<string, AvePoint.RA.DB.Model.ArchiverSiteMasterIndex> siteInfoCache = new Dictionary<string, AvePoint.RA.DB.Model.ArchiverSiteMasterIndex>();
            logger.Info($"Process storage id {storageId}");
            var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId);
            mStorageDirectory.Add(storageId, storageDevice);
            var hasRetentionRule = false;
            var indexLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);
            var subInfos = FSIndexSubInfoDao.GetAllFSArchiverIndexSubInfoByStorageId(storageId);
            var inProgressStoreJobs = JobMonitorService.GetRunningJobs(new List<JobType> { JobType.FSDisposal, JobType.FSDisposalSchedule });

            foreach (var info in subInfos)
            {
                try
                {
                    logger.Info($"Process fs job id :{info.SubSubJobId}");
                    var tempJobId = info.SubSubJobId?.Split("_");
                    if (tempJobId?.Length > 0)
                    {
                        var mainJobId = tempJobId.FirstOrDefault();
                        if (inProgressStoreJobs.Any(j => j.Id == mainJobId))
                        {
                            logger.Warn($"{mainJobId} is running, skip this info");
                            continue;
                        }
                    }
                    try
                    {
                        RetentionRule? matchedRule = null;
                        if (storageDevice.ArchiveRetentionRules?.Count == 1 && storageDevice?.ArchiveRetentionRules?.FirstOrDefault()?.RetentionDataTimeType == KeepDateType.ModifiedTime)
                        {
                            logger.Info("this storage retention job is retention by modified time");
                            matchedRule = storageDevice.ArchiveRetentionRules.FirstOrDefault();
                        }
                        else if (storageDevice.SetupDataRetention)
                        {
                            if (info.RetentionTime < storageDevice.ModifyTime)
                            {
                                logger.Info($"the storage device has been modified,need reset Retention count,job id:{info.SubSubJobId}");
                                info.RetentionCount = 1;
                                info.RetentionSource = (int)RetentionSourceFlag.Storage;
                                if (!isSimulateJob)
                                {
                                    FSIndexSubInfoDao.UpdateAsync(info).GetAwaiter().GetResult();
                                }
                            }
                            logger.Info($"None retention infos by rule level, , JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                            matchedRule = GetMatchedFSRetentionRule(storageDevice.ArchiveRetentionRules, info, RetentionSourceFlag.Storage, isSimulateJob);
                        }
                        else
                        {
                            logger.Info("Neither storage nor rule has a retention");
                        }

                        if (matchedRule != null)
                        {
                            hasRetentionRule = true;
                            ArchiverRetentionMessage? message = null;
                            logger.Info($"StorageId {storageId} CurrentStorageId {info.CurrentStorageId}");
                            if (!mStorageDirectory.ContainsKey(info.CurrentStorageId))
                            {
                                var storage = StorageDeviceService.GetStorageDeviceById(info.CurrentStorageId, needDecryptSecert: true);
                                mStorageDirectory.Add(info.CurrentStorageId, storage);
                            }
                            var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(mStorageDirectory[info.CurrentStorageId]);

                            message = AssembleFSRetentionMessage(info.CurrentStorageId, matchedRule, info, srcLogical, indexLogical);

                            ArchiverPruningJob archiverPruningJob = InitPruningJob(message);
                            if (isSimulateJob)
                            {
                                archiverPruningJob.RetentionSourceName = storageDevice.Name;
                                archiverPruningJob.SourceFlag = (int)SourceFlag.FileSystem;
                                archiverPruningJob.IsSimulateJob = isSimulateJob;
                                archiverPruningJob.SimulateJobRunTime = GetNextRetentionRunTime();
                            }
                            if (needRetentionJobs.ContainsKey(archiverPruningJob.SiteUrl))
                            {
                                needRetentionJobs[archiverPruningJob.SiteUrl].Add(archiverPruningJob);
                            }
                            else
                            {
                                needRetentionJobs.Add(archiverPruningJob.SiteUrl, new List<ArchiverPruningJob>() { archiverPruningJob });
                            }
                        }
                        else
                        {
                            logger.Info($"Not match retention rule.");
                        }
                    }
                    catch (LicenseMismatchOfAvePointStorageException lme)
                    {
                        logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                        throw;
                    }
                    catch (FSNotSurpportAvePointStorageException e)
                    {
                        logger.Error($"FSNotSurpportAvePointStorageException error : {e}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.Error("retention Error :{0}", e.ToString());
                    }

                }
                catch (LicenseMismatchOfAvePointStorageException lme)
                {
                    logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                    throw;
                }
                catch (FSNotSurpportAvePointStorageException e)
                {
                    logger.Error($"FSNotSurpportAvePointStorageException error : {e}");
                    throw;
                }
                catch (Exception e)
                {
                    logger.Error($"run retention Error :{e}");
                }
            }
            if (!hasRetentionRule)
            {
                logger.Error("Not match retention rule info.");
            }
        }

        private void GenerateTeamsRetentionInfo(string storageId, Dictionary<string, List<ArchiverPruningJob>> needRetentionTeamsGroupJobs, StorageDeviceDto indexDevice, bool isSimulateJob = false)
        {
            Dictionary<string, StorageDeviceDto> mStorageDirectory = new Dictionary<string, StorageDeviceDto>();
            logger.Info($"Process storage id {storageId}");
            var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId);
            mStorageDirectory.Add(storageId, storageDevice);
            var hasRetentionRule = false;
            var indexLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);

            var infos = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoByStorageIdAndSourceFlag(storageId, new List<int> { (int)SourceFlag.Teams, (int)SourceFlag.Groups });
            var inProgressStoreJobs = JobMonitorService.GetRunningJobs(new List<JobType> { JobType.TeamsRecordsDisposal, JobType.TeamsArchiverBackup, JobType.SpecifyTeamsArchiverBackup });
            var infoMappingByRule = infos.GroupBy(g => g.RuleId);

            foreach (var infoGroup in infoMappingByRule)
            {
                Rule? rule = null;
                var currentRuleId = infoGroup.Key;
                var currentRuleName = "";
                logger.Info($"Process data by rule: {currentRuleId}");
                if (!string.IsNullOrEmpty(currentRuleId))
                {
                    var profile = MiscProfileDao.Load(currentRuleId);
                    if (profile != null)
                    {
                        try
                        {
                            rule = SerializerHelper.DeserializeByDataContractSerializer<Rule>(profile.Extension);
                            currentRuleName = rule.Name;
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"Deserialize rule [{currentRuleId}] error:{e}");
                        }
                    }
                    else
                    {
                        logger.Warn($"The rule maybe deleted, rule id {currentRuleId}");
                    }
                }

                foreach (var info in infoGroup)
                {
                    try
                    {
                        logger.Info($"Process job id :{info.SubSubJobId}, rule id: {currentRuleId}");
                        var tempJobId = info.SubSubJobId?.Split("_");
                        if (tempJobId?.Length > 0)
                        {
                            var mainJobId = tempJobId.FirstOrDefault();
                            if (inProgressStoreJobs.Any(j => j.Id == mainJobId))
                            {
                                logger.Warn($"{mainJobId} is running, skip this info");
                                continue;
                            }
                        }
                        try
                        {
                            RetentionRule? matchedRule = null;
                            List<RetentionRule>? ruleRetentionInfos = null;
                            bool ruleHasModified = false;
                            if (!string.IsNullOrEmpty(currentRuleId))
                            {
                                if (rule != null)
                                {
                                    Rule? configurateRetentionRule = GetRuleBySource(rule, (SourceFlag)info.DataFlag);

                                    if (configurateRetentionRule != null)
                                    {
                                        if (configurateRetentionRule.IsEnableRetention)
                                        {
                                            logger.Warn($"Rule [{currentRuleId}], is enable archiver content retention, skip it in retention job.");
                                            continue;
                                        }
                                        else if (configurateRetentionRule.IsEnableStoreContentRetention)
                                        {
                                            var ruleModifyTime = GetRuleModifyTime(currentRuleId, MiscProfileDao.Load(currentRuleId));
                                            if (ruleModifyTime > 0 && info.RetentionTime < ruleModifyTime)
                                            {
                                                logger.Warn($"the rule has been modified,job id:{info.SubSubJobId},info Retention time is:{info.RetentionTime},rule modifytime:{ruleModifyTime}");
                                                ruleHasModified = true;
                                            }
                                            ruleRetentionInfos = configurateRetentionRule.StoreContentRetentionInfos;
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"Can't get rule by source[{info.SourceFlag}]");
                                    }
                                }
                            }
                            else
                            {
                                logger.Info($"Index info is not has rule, job id:{info.SubSubJobId}");
                            }

                            if (ruleRetentionInfos?.Count == 1 && ruleRetentionInfos.FirstOrDefault().RetentionDataTimeType == KeepDateType.ModifiedTime)
                            {
                                logger.Info("this rule retention job is retention by modified time");
                                matchedRule = ruleRetentionInfos.FirstOrDefault();
                            }
                            else if (ruleRetentionInfos?.Count > 0)
                            {
                                if (ruleHasModified)
                                {
                                    logger.Info($"the rule has been modified,need reset Retention count,job id:{info.SubSubJobId}");
                                    info.RetentionCount = 1;
                                    info.RetentionSource = (int)RetentionSourceFlag.Rule;
                                    ArchiverIndexSubInfoDao.UpdateAsync(info).GetAwaiter().GetResult();
                                }
                                logger.Info($"Rule id:{currentRuleId}, rule source:{info.SourceFlag}, rule retention info:{JsonConvert.SerializeObject(ruleRetentionInfos.Select(r => new { Unit = r.ArchiveDateUnit.ToString(), r.KeepValue }))}");
                                matchedRule = GetMatchedRetentionRule(ruleRetentionInfos, info, RetentionSourceFlag.Rule, isSimulateJob);
                                if (matchedRule != null)
                                {
                                    matchedRule.RemoveOrphanedStub = matchedRule.RemoveOrphanedStub || !matchedRule.KeepOrphanedStub4CompatibilityExistingRule;
                                    logger.Info($"Match retention rule by rule level, RuleId:{currentRuleId}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                }
                                else
                                {
                                    logger.Info($"Not match retention rule by rule level, RuleId:{currentRuleId}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                }
                            }
                            else if (storageDevice.ArchiveRetentionRules?.Count == 1 && storageDevice?.ArchiveRetentionRules?.FirstOrDefault()?.RetentionDataTimeType == KeepDateType.ModifiedTime)
                            {
                                logger.Info("this storage retention job is retention by modified time, not supported");
                                //matchedRule = storageDevice.ArchiveRetentionRules.FirstOrDefault();
                            }
                            else if (storageDevice.SetupDataRetention)
                            {
                                if (info.RetentionTime < storageDevice.ModifyTime)
                                {
                                    logger.Info($"the storage device has been modified,need reset Retention count,job id:{info.SubSubJobId}");
                                    info.RetentionCount = 1;
                                    info.RetentionSource = (int)RetentionSourceFlag.Storage;
                                    ArchiverIndexSubInfoDao.UpdateAsync(info).GetAwaiter().GetResult();
                                }
                                logger.Info($"None retention infos by rule level, RuleId:{currentRuleId}, SourceFlag:{info.SourceFlag}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");

                                matchedRule = GetMatchedRetentionRule(storageDevice.ArchiveRetentionRules, info, RetentionSourceFlag.Storage, isSimulateJob);
                            }
                            else
                            {
                                logger.Info("Neither storage nor rule has a retention");
                            }

                            if (matchedRule != null)
                            {
                                hasRetentionRule = true;
                                ArchiverRetentionMessage? message = null;
                                logger.Info($"StorageId {storageId} CurrentStorageId {info.CurrentStorageId}");
                                if (!mStorageDirectory.ContainsKey(info.CurrentStorageId))
                                {
                                    var storage = StorageDeviceService.GetStorageDeviceById(info.CurrentStorageId, needDecryptSecert: true);
                                    mStorageDirectory.Add(info.CurrentStorageId, storage);
                                }
                                var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(mStorageDirectory[info.CurrentStorageId]);
                                message = AssembleRetentionMessageForTeams(info.CurrentStorageId, matchedRule, info, srcLogical, indexLogical);
                                if (message == null)
                                {
                                    logger.Warn($"Could not assemble retension message for Teams, index's subsub job: [{info.SubSubJobId}].");
                                    continue;
                                }
                                ;
                                ArchiverPruningJob archiverPruningJob = InitPruningJob(message);
                                if (needRetentionTeamsGroupJobs.ContainsKey(archiverPruningJob.SiteUrl))
                                {
                                    needRetentionTeamsGroupJobs[archiverPruningJob.SiteUrl].Add(archiverPruningJob);
                                }
                                else
                                {
                                    needRetentionTeamsGroupJobs.Add(archiverPruningJob.SiteUrl, new List<ArchiverPruningJob>() { archiverPruningJob });
                                }
                            }
                            else
                            {
                                logger.Info($"Not match retention rule.");
                            }
                        }
                        catch (LicenseMismatchOfAvePointStorageException lme)
                        {
                            logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                            throw;
                        }
                        catch (Exception e)
                        {
                            logger.Error("retention Error :{0}", e.ToString());
                        }

                    }
                    catch (LicenseMismatchOfAvePointStorageException lme)
                    {
                        logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"run retention Error :{e}");
                    }
                }
            }
            if (!hasRetentionRule)
            {
                logger.Error("Not match retention rule info.");
            }
        }

        private void GenerateExchangeRetentionInfo(string storageId, Dictionary<string, List<ArchiverPruningJob>> needRetentionTeamsGroupJobs, StorageDeviceDto indexDevice)
        {
            Dictionary<string, StorageDeviceDto> mStorageDirectory = new Dictionary<string, StorageDeviceDto>();
            logger.Info($"Process storage id {storageId}");
            var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId);
            mStorageDirectory.Add(storageId, storageDevice);
            var hasRetentionRule = false;
            var indexLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);

            var infos = EXOArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoByStorageId(storageId);
            var inProgressStoreJobs = JobMonitorService.GetRunningJobs(new List<JobType> { JobType.TeamsRecordsDisposal, JobType.TeamsArchiverBackup, JobType.SpecifyTeamsArchiverBackup });
            var infoMappingByRule = infos.GroupBy(g => g.RuleId);

            foreach (var infoGroup in infoMappingByRule)
            {
                Rule? rule = null;
                var currentRuleId = infoGroup.Key;
                var currentRuleName = "";
                logger.Info($"Process data by rule: {currentRuleId}");
                if (!string.IsNullOrEmpty(currentRuleId))
                {
                    var profile = MiscProfileDao.Load(currentRuleId);
                    if (profile != null)
                    {
                        try
                        {
                            rule = SerializerHelper.DeserializeByDataContractSerializer<Rule>(profile.Extension);
                            currentRuleName = rule.Name;
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"Deserialize rule [{currentRuleId}] error:{e}");
                        }
                    }
                    else
                    {
                        logger.Warn($"The rule maybe deleted, rule id {currentRuleId}");
                    }
                }

                foreach (var info in infoGroup)
                {
                    try
                    {
                        logger.Info($"Process job id :{info.SubSubJobId}, rule id: {currentRuleId}");
                        var tempJobId = info.SubSubJobId?.Split("_");
                        if (tempJobId?.Length > 0)
                        {
                            var mainJobId = tempJobId.FirstOrDefault();
                            if (inProgressStoreJobs.Any(j => j.Id == mainJobId))
                            {
                                logger.Warn($"{mainJobId} is running, skip this info");
                                continue;
                            }
                        }
                        try
                        {
                            RetentionRule? matchedRule = null;
                            List<RetentionRule>? ruleRetentionInfos = null;
                            bool ruleHasModified = false;
                            if (!string.IsNullOrEmpty(currentRuleId))
                            {
                                if (rule != null)
                                {
                                    Rule? configurateRetentionRule = GetRuleBySource(rule, SourceFlag.Exchange);

                                    if (configurateRetentionRule != null)
                                    {
                                        if (configurateRetentionRule.IsEnableRetention)
                                        {
                                            logger.Warn($"Rule [{currentRuleId}], is enable archiver content retention, skip it in retention job.");
                                            continue;
                                        }
                                        else if (configurateRetentionRule.IsEnableStoreContentRetention)
                                        {
                                            var ruleModifyTime = GetRuleModifyTime(currentRuleId, MiscProfileDao.Load(currentRuleId));
                                            if (ruleModifyTime > 0 && info.RetentionTime < ruleModifyTime)
                                            {
                                                logger.Warn($"the rule has been modified,job id:{info.SubSubJobId},info Retention time is:{info.RetentionTime},rule modifytime:{ruleModifyTime}");
                                                ruleHasModified = true;
                                            }
                                            ruleRetentionInfos = configurateRetentionRule.StoreContentRetentionInfos;
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"Can't get rule by source[{SourceFlag.Exchange}]");
                                    }
                                }
                            }
                            else
                            {
                                logger.Info($"Index info is not has rule, job id:{info.SubSubJobId}");
                            }

                            if (ruleRetentionInfos?.Count == 1 && ruleRetentionInfos.FirstOrDefault().RetentionDataTimeType == KeepDateType.ModifiedTime)
                            {
                                logger.Info("this rule retention job is retention by modified time");
                                matchedRule = ruleRetentionInfos.FirstOrDefault();
                            }
                            else if (ruleRetentionInfos?.Count > 0)
                            {
                                if (ruleHasModified)
                                {
                                    logger.Info($"the rule has been modified,need reset Retention count,job id:{info.SubSubJobId}");
                                    info.RetentionCount = 1;
                                    info.RetentionSource = (int)RetentionSourceFlag.Rule;
                                    EXOArchiverIndexSubInfoDao.UpdateAsync(info).GetAwaiter().GetResult();
                                }
                                logger.Info($"Rule id:{currentRuleId}, rule source:{SourceFlag.Exchange}, rule retention info:{JsonConvert.SerializeObject(ruleRetentionInfos.Select(r => new { Unit = r.ArchiveDateUnit.ToString(), r.KeepValue }))}");
                                matchedRule = GetMatchedEXORetentionRule(ruleRetentionInfos, info, RetentionSourceFlag.Rule);
                                if (matchedRule != null)
                                {
                                    matchedRule.RemoveOrphanedStub = matchedRule.RemoveOrphanedStub || !matchedRule.KeepOrphanedStub4CompatibilityExistingRule;
                                    logger.Info($"Match retention rule by rule level, RuleId:{currentRuleId}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                }
                                else
                                {
                                    logger.Info($"Not match retention rule by rule level, RuleId:{currentRuleId}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                }
                            }
                            else if (storageDevice.ArchiveRetentionRules?.Count == 1 && storageDevice?.ArchiveRetentionRules?.FirstOrDefault()?.RetentionDataTimeType == KeepDateType.ModifiedTime)
                            {
                                logger.Info("this storage retention job is retention by modified time, not supported.");
                                //matchedRule = storageDevice.ArchiveRetentionRules.FirstOrDefault();
                            }
                            else if (storageDevice.SetupDataRetention)
                            {
                                if (info.RetentionTime < storageDevice.ModifyTime)
                                {
                                    logger.Info($"the storage device has been modified,need reset Retention count,job id:{info.SubSubJobId}");
                                    info.RetentionCount = 1;
                                    info.RetentionSource = (int)RetentionSourceFlag.Storage;
                                    EXOArchiverIndexSubInfoDao.UpdateAsync(info).GetAwaiter().GetResult();
                                }
                                logger.Info($"None retention infos by rule level, RuleId:{currentRuleId}, SourceFlag:{SourceFlag.Exchange}, JobId {info.SubSubJobId}, StorageId {storageId}, CurrentStorageId {info.CurrentStorageId}");
                                matchedRule = GetMatchedEXORetentionRule(storageDevice.ArchiveRetentionRules, info, RetentionSourceFlag.Storage);
                            }
                            else
                            {
                                logger.Info("Neither storage nor rule has a retention");
                            }

                            if (matchedRule != null)
                            {
                                hasRetentionRule = true;
                                ArchiverRetentionMessage? message = null;
                                logger.Info($"StorageId {storageId} CurrentStorageId {info.CurrentStorageId}");
                                if (!mStorageDirectory.ContainsKey(info.CurrentStorageId))
                                {
                                    var storage = StorageDeviceService.GetStorageDeviceById(info.CurrentStorageId, needDecryptSecert: true);
                                    mStorageDirectory.Add(info.CurrentStorageId, storage);
                                }
                                var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(mStorageDirectory[info.CurrentStorageId]);
                                message = AssembleRetentionMessageForEXO(info.CurrentStorageId, matchedRule, info, srcLogical, indexLogical);
                                ArchiverPruningJob archiverPruningJob = InitPruningJob(message);
                                if (needRetentionTeamsGroupJobs.ContainsKey(archiverPruningJob.SiteUrl))
                                {
                                    needRetentionTeamsGroupJobs[archiverPruningJob.SiteUrl].Add(archiverPruningJob);
                                }
                                else
                                {
                                    needRetentionTeamsGroupJobs.Add(archiverPruningJob.SiteUrl, new List<ArchiverPruningJob>() { archiverPruningJob });
                                }
                            }
                            else
                            {
                                logger.Info($"Not match retention rule.");
                            }
                        }
                        catch (LicenseMismatchOfAvePointStorageException lme)
                        {
                            logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                            throw;
                        }
                        catch (Exception e)
                        {
                            logger.Error("retention Error :{0}", e.ToString());
                        }

                    }
                    catch (LicenseMismatchOfAvePointStorageException lme)
                    {
                        logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"run retention Error :{e}");
                    }
                }
            }
            if (!hasRetentionRule)
            {
                logger.Error("Not match retention rule info.");
            }
        }

        private void GenerateGDriveRetentionInfo(string storageId, Dictionary<string, List<ArchiverPruningJob>> needRetentionGDriveJobs, StorageDeviceDto indexDevice, bool isSimulateJob)
        {
            logger.Info($"Process storage id {storageId} for GDrive retention job.");

            var hasRetentionRule = false;

            var archiverIndexSubJobInfos = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoByStorageIdAndSourceFlag(storageId, [(int)SourceFlag.Google]);
            var inProgressStoreJobs = JobMonitorService.GetRunningJobs([JobType.GoogleRecordsDisposal, JobType.GoogleArchiverBackup]);
            var archiverIndexSubJobInfosMappingByRule = archiverIndexSubJobInfos.GroupBy(g => g.RuleId);

            foreach (var archiverIndexSubJobInfoGroup in archiverIndexSubJobInfosMappingByRule)
            {
                var currentRuleId = archiverIndexSubJobInfoGroup.Key;
                logger.Info($"Process data by rule: {currentRuleId} for GDrive retention job.");

                var rule = GetRuleById(currentRuleId);

                foreach (var archiverIndexSubJobInfo in archiverIndexSubJobInfoGroup)
                {
                    try
                    {
                        logger.Info($"Process job id :{archiverIndexSubJobInfo.SubSubJobId}, rule id: {currentRuleId} for GDrive retention job.");
                        var tempJobId = archiverIndexSubJobInfo.SubSubJobId?.Split("_");
                        if (tempJobId?.Length > 0)
                        {
                            var mainJobId = tempJobId[0];
                            if (inProgressStoreJobs.Any(j => j.Id == mainJobId))
                            {
                                logger.Warn($"{mainJobId} is running, skip this info");
                                continue;
                            }
                        }

                        var retentionRule = ProcessGDriveArchiverIndexSubJobInfo(rule, currentRuleId, archiverIndexSubJobInfo
                            , storageId, indexDevice, needRetentionGDriveJobs);
                        if (retentionRule != null)
                        {
                            hasRetentionRule = true;
                        }

                    }
                    catch (LicenseMismatchOfAvePointStorageException lme)
                    {
                        logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"run retention Error :{e}");
                    }
                }
            }

            if (!hasRetentionRule)
            {
                logger.Error("Not match retention rule info.");
            }
        }

        private RetentionRule ProcessGDriveArchiverIndexSubJobInfo(Rule rule, string currentRuleId,
            ArchiverIndexSubInfo archiverIndexSubJobInfo, string storageId, StorageDeviceDto indexDevice
            , Dictionary<string, List<ArchiverPruningJob>> needRetentionGDriveJobs)
        {
            Dictionary<string, StorageDeviceDto> mStorageDirectory = new();
            var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId);
            mStorageDirectory.Add(storageId, storageDevice);

            var indexLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDevice);

            RetentionRule? matchedRule = null;

            try
            {
                List<RetentionRule>? ruleRetentionInfos = null;
                bool ruleHasModified = false;

                CheckExistRetentionRules(ref ruleRetentionInfos, ref ruleHasModified, rule, currentRuleId,
                        archiverIndexSubJobInfo);

                matchedRule = GetMatchedGDriveRetentionRule(ruleRetentionInfos, archiverIndexSubJobInfo, currentRuleId,
                    storageDevice, ruleHasModified, storageId);

                if (matchedRule != null)
                {
                    ArchiverRetentionMessage? message;
                    logger.Info($"StorageId {storageId} CurrentStorageId {archiverIndexSubJobInfo.CurrentStorageId} for GDrive retention job.");
                    if (!mStorageDirectory.ContainsKey(archiverIndexSubJobInfo.CurrentStorageId))
                    {
                        var storage =
                            StorageDeviceService.GetStorageDeviceById(archiverIndexSubJobInfo.CurrentStorageId,
                                needDecryptSecert: true);
                        mStorageDirectory.Add(archiverIndexSubJobInfo.CurrentStorageId, storage);
                    }

                    var srcLogical =
                        ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(
                            mStorageDirectory[archiverIndexSubJobInfo.CurrentStorageId]);
                    message = AssembleRetentionMessageForGDrive(archiverIndexSubJobInfo.CurrentStorageId, matchedRule,
                        archiverIndexSubJobInfo, srcLogical, indexLogical);
                    if (message == null)
                    {
                        logger.Warn(
                            $"Could not assemble retention message for Google Drive, index's sub job: [{archiverIndexSubJobInfo.SubSubJobId}].");
                        return matchedRule;
                    }

                    ArchiverPruningJob archiverPruningJob = InitGooglePruningJob(message);
                    if (needRetentionGDriveJobs.ContainsKey(archiverPruningJob.SiteUrl))
                    {
                        needRetentionGDriveJobs[archiverPruningJob.SiteUrl].Add(archiverPruningJob);
                    }
                    else
                    {
                        needRetentionGDriveJobs.Add(archiverPruningJob.SiteUrl, [archiverPruningJob]);
                    }
                }
                else
                {
                    logger.Info($"Not match retention rule for GDrive retention job..");
                }
            }
            catch (LicenseMismatchOfAvePointStorageException lme)
            {
                logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                throw;
            }
            catch (Exception e)
            {
                logger.Error("retention Error :{0}", e.ToString());
            }
            return matchedRule;
        }

        private RetentionRule GetMatchedGDriveRetentionRule(List<RetentionRule>? ruleRetentionInfos,
            ArchiverIndexSubInfo archiverIndexSubJobInfo, string currentRuleId,
            StorageDeviceDto storageDevice, bool ruleHasModified, string storageId)
        {
            RetentionRule matchedRule = null;
            if (ruleRetentionInfos?.Count == 1 && ruleRetentionInfos[0].RetentionDataTimeType == KeepDateType.ModifiedTime)
            {
                logger.Info("this rule retention job is retention by modified time for GDrive retention job.");
                matchedRule = ruleRetentionInfos[0];
            }
            if (ruleRetentionInfos?.Count > 0)
            {
                if (ruleHasModified)
                {
                    logger.Info(
                        $"the rule has been modified,need reset Retention count,job id:{archiverIndexSubJobInfo.SubSubJobId} for GDrive retention job.");
                    archiverIndexSubJobInfo.RetentionCount = 1;
                    archiverIndexSubJobInfo.RetentionSource = (int)RetentionSourceFlag.Rule;
                    ArchiverIndexSubInfoDao.UpdateAsync(archiverIndexSubJobInfo).GetAwaiter().GetResult();
                }

                logger.Info(
                    $"Rule id:{currentRuleId}, rule source:{archiverIndexSubJobInfo.SourceFlag}, rule retention info:{JsonConvert.SerializeObject(ruleRetentionInfos.Select(r => new { Unit = r.ArchiveDateUnit.ToString(), r.KeepValue }))} for GDrive retention job.");
                matchedRule = GetMatchedRetentionRule(ruleRetentionInfos, archiverIndexSubJobInfo,
                    RetentionSourceFlag.Rule, false);
                if (matchedRule != null)
                {
                    matchedRule.RemoveOrphanedStub = matchedRule.RemoveOrphanedStub ||
                                                     !matchedRule.KeepOrphanedStub4CompatibilityExistingRule;
                    logger.Info(
                        $"Match retention rule by rule level, RuleId:{currentRuleId}, JobId {archiverIndexSubJobInfo.SubSubJobId}, StorageId {storageId}, CurrentStorageId {archiverIndexSubJobInfo.CurrentStorageId} for GDrive retention job.");
                }
                else
                {
                    logger.Info(
                        $"Not match retention rule by rule level, RuleId:{currentRuleId}, JobId {archiverIndexSubJobInfo.SubSubJobId}, StorageId {storageId}, CurrentStorageId {archiverIndexSubJobInfo.CurrentStorageId} for GDrive retention job.");
                }
            }
            else if (storageDevice.ArchiveRetentionRules?.Count == 1 &&
                     storageDevice?.ArchiveRetentionRules[0].RetentionDataTimeType == KeepDateType.ModifiedTime)
            {
                logger.Info("This Google rule retention job is retention by modified time");
                matchedRule = storageDevice.ArchiveRetentionRules.FirstOrDefault();
            }
            else if (storageDevice.SetupDataRetention)
            {
                if (archiverIndexSubJobInfo.RetentionTime < storageDevice.ModifyTime)
                {
                    logger.Info(
                        $"the storage device has been modified,need reset Retention count,job id:{archiverIndexSubJobInfo.SubSubJobId} for GDrive retention job.");
                    archiverIndexSubJobInfo.RetentionCount = 1;
                    archiverIndexSubJobInfo.RetentionSource = (int)RetentionSourceFlag.Storage;
                    ArchiverIndexSubInfoDao.UpdateAsync(archiverIndexSubJobInfo).GetAwaiter().GetResult();
                }

                logger.Info(
                    $"None retention infos by rule level, RuleId:{currentRuleId}, SourceFlag:{archiverIndexSubJobInfo.SourceFlag}, JobId {archiverIndexSubJobInfo.SubSubJobId}, StorageId {storageId}, CurrentStorageId {archiverIndexSubJobInfo.CurrentStorageId} for GDrive retention job.");

                matchedRule = GetMatchedRetentionRule(storageDevice.ArchiveRetentionRules, archiverIndexSubJobInfo,
                    RetentionSourceFlag.Storage, false);
            }
            else
            {
                logger.Info("Neither storage nor rule has a retention for GDrive retention job.");
            }

            return matchedRule;
        }

        private void CheckExistRetentionRules(ref List<RetentionRule> ruleRetentionInfos, ref bool ruleHasModified, Rule rule, string currentRuleId, ArchiverIndexSubInfo archiverIndexSubJobInfo)
        {
            if (!string.IsNullOrEmpty(currentRuleId))
            {
                if (rule != null)
                {
                    Rule? configRetentionRule = GetRuleBySource(rule, (SourceFlag)archiverIndexSubJobInfo.DataFlag);

                    if (configRetentionRule != null)
                    {
                        if (configRetentionRule.IsEnableRetention)
                        {
                            logger.Warn(
                                $"Rule [{currentRuleId}], is enable archiver content retention, skip it in Google retention job.");
                        }

                        if (configRetentionRule.IsEnableStoreContentRetention)
                        {
                            var profile = MiscProfileDao.Load(currentRuleId);
                            var ruleModifyTime = GetRuleModifyTime(currentRuleId, profile);
                            if (ruleModifyTime > 0 && archiverIndexSubJobInfo.RetentionTime < ruleModifyTime)
                            {
                                logger.Warn(
                                    $"the rule has been modified,job id:{archiverIndexSubJobInfo.SubSubJobId},info Retention time is:{archiverIndexSubJobInfo.RetentionTime},rule modified time:{ruleModifyTime} for GDrive retention job.");
                                ruleHasModified = true;
                            }

                            ruleRetentionInfos = configRetentionRule.StoreContentRetentionInfos;
                        }
                    }
                    else
                    {
                        logger.Info($"Can't get rule by source[{archiverIndexSubJobInfo.SourceFlag}]");
                    }
                }
            }
            else
            {
                logger.Info($"Index info is not has rule, job id:{archiverIndexSubJobInfo.SubSubJobId} for GDrive retention job.");
            }
        }

        private List<RetentionRule> ConvertStoreContentRetentionSetting(List<RetentionSettings> setting)
        {
            List<RetentionRule> infos = new();
            if (setting == null || setting.Count == 0)
            {
                return null;
            }
            foreach (RetentionSettings tempSetting in setting)
            {
                if (tempSetting == null)
                {
                    continue;
                }
                RetentionRule info = new();
                infos.Add(info);
                info.SetupDataRetention = tempSetting.IsEnableRetention;
                info.RetentionDataTimeType = tempSetting.RetentionDataTimeType == KeepDateType.None ? KeepDateType.ArchiveTime : tempSetting.RetentionDataTimeType;
                info.KeepValue = tempSetting.KeepDateNumber;
                info.ArchiveDateUnit = tempSetting.KeepDateUnite switch
                {
                    TimeUnit.Day => DateUnit.Day,
                    TimeUnit.Week => DateUnit.Week,
                    TimeUnit.Month => DateUnit.Month,
                    TimeUnit.Year => DateUnit.Year,
                    _ => throw new NotImplementedException(),
                };
                //save rule
                if (tempSetting.OperateDataType == (int)OperateDateTypeEnum.Delete)
                {
                    info.DeleteTheData = true;
                }
                else if (tempSetting.OperateDataType == (int)OperateDateTypeEnum.MarkTier)
                {
                    info.IsMarkDataTier = true;
                    info.TierType = tempSetting.TierType ?? 0;
                }
                info.RemoveOrphanedStub = tempSetting.RemoveOrphanedStub;
                info.KeepOrphanedStub4CompatibilityExistingRule = !tempSetting.RemoveOrphanedStub;
                info.SoftDeleteDateUnit = tempSetting.SoftKeepDateUnite switch
                {
                    TimeUnit.Day => DateUnit.Day,
                    TimeUnit.Week => DateUnit.Week,
                    TimeUnit.Month => DateUnit.Month,
                    TimeUnit.Year => DateUnit.Year,
                    _ => throw new NotImplementedException(),
                };
                info.SoftDeleteKeepValue = tempSetting.SoftKeepDateNumber;
                info.IsSoftDelete = tempSetting.IsSoftDelete;
            }
            return infos;
        }

        private Rule GetRuleById(string currentRuleId)
        {
            if (!string.IsNullOrEmpty(currentRuleId))
            {
                var profile = MiscProfileDao.Load(currentRuleId);
                if (profile != null)
                {
                    try
                    {
                        return SerializerHelper.DeserializeByDataContractSerializer<Rule>(profile.Extension);
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Deserialize rule [{currentRuleId}] error:{e}");
                    }
                }
                else
                {
                    logger.Warn($"The rule maybe deleted, rule id {currentRuleId}");
                }
            }

            return null;
        }

        private long GetRuleModifyTime(string currentRuleId, RMMiscProfile profile)
        {
            if (string.IsNullOrWhiteSpace(currentRuleId))
            {
                return 0;
            }

            if (Guid.TryParse(currentRuleId, out var ruleGuid))
            {
                var internalRule = RMRuleDao.GetRuleById(ruleGuid);
                if (internalRule != null)
                {
                    return internalRule.ModifyTime;
                }
            }

            return profile?.ModifiedTime ?? 0;
        }

        private ArchiverPruningJob InitPruningJob(ArchiverRetentionMessage message)
        {
            ArchiverPruningJob archiverPruningJob = new ArchiverPruningJob();
            archiverPruningJob.FarmName = message.FarmName;
            archiverPruningJob.SiteUrl = message.SiteUrl;
            archiverPruningJob.WebApp = message.WebApp;
            archiverPruningJob.JobId = message.JobId;
            archiverPruningJob.StoragePolicyId = message.StoragePolicyId;
            archiverPruningJob.ArchiverBackupTime = message.ArchiverBackupTime;
            archiverPruningJob.IndexLogicalDevice = message.IndexLogicalDevice;
            archiverPruningJob.DataLogicalDevice = message.LogicalDevice;
            archiverPruningJob.DestinationDevice = message.DestinationDevice;
            archiverPruningJob.RemoveOrphanedStub = message.RemoveOrphanedStub;
            archiverPruningJob.SiteId = message.SiteId;
            archiverPruningJob.RetentionAction = message.RetentionAction;
            archiverPruningJob.DestinationPhysicalDeviceId = message.DestinationPhysicalDeviceId;
            archiverPruningJob.MediaService = message.MediaService;
            archiverPruningJob.IsDeleteJob = message.IsDeleteJob;
            archiverPruningJob.State = message.State;
            archiverPruningJob.RetentionJob = message.RetentionJob;
            archiverPruningJob.RetentionTimeSpanSeconds = message.RetentionTimeSpanSeconds;
            archiverPruningJob.TenantGroupId = message.TenantGroupId;
            archiverPruningJob.TenantGroupOwner = message.TenantGroupOwner;
            archiverPruningJob.MainIndexStorageInfo = message.MainIndexStorageInfo;
            archiverPruningJob.SubIndexStorageInfo = message.SubIndexStorageInfo;
            archiverPruningJob.NeedStoreInArchiverTier = message.IsArchivedTier;
            archiverPruningJob.AccessTierType = message.AccessTierType;
            archiverPruningJob.ArchiveDateUnit = message.ArchiveDateUnit;
            archiverPruningJob.KeepValue = message.KeepValue;
            archiverPruningJob.RetentionDataTimeType = message.RetentionDataTimeType;
            archiverPruningJob.IsFitSoftDelete = message.IsFitSoftDelete;
            archiverPruningJob.IsSoftDelete = message.IsSoftDelete;
            archiverPruningJob.CurrentStoragePolicyId = message.CurrentStoragePolicyId;
            archiverPruningJob.SoftDeleteKeepValue = message.SoftDeleteKeepValue;
            archiverPruningJob.SoftDeleteDateUnit = message.SoftDeleteDateUnit;
            archiverPruningJob.SoftDeleteTime = message.SoftDeleteTime;
            archiverPruningJob.AgentId = message.AgentId;
            archiverPruningJob.dataSourceForOrphanBlob = message.dataSourceForOrphanBlob;
            archiverPruningJob.IsSystemStorage = message.IsSystemStorage;
            archiverPruningJob.DeleteStatus = message.DeleteStatus;
            archiverPruningJob.HasMoveActionInPreviousRules = message.HasMoveActionInPreviousRules;
            return archiverPruningJob;
        }

        private ArchiverPruningJob InitGooglePruningJob(ArchiverRetentionMessage message)
        {
            ArchiverPruningJob archiverPruningJob = new ArchiverPruningJob();
            archiverPruningJob.FarmName = message.FarmName;
            archiverPruningJob.SiteUrl = message.SiteUrl;
            archiverPruningJob.WebApp = message.WebApp;
            archiverPruningJob.JobId = message.JobId;
            archiverPruningJob.StoragePolicyId = message.StoragePolicyId;
            archiverPruningJob.ArchiverBackupTime = message.ArchiverBackupTime;
            archiverPruningJob.IndexLogicalDevice = message.IndexLogicalDevice;
            archiverPruningJob.DataLogicalDevice = message.LogicalDevice;
            archiverPruningJob.DestinationDevice = message.DestinationDevice;
            archiverPruningJob.RemoveOrphanedStub = message.RemoveOrphanedStub;
            archiverPruningJob.SiteId = message.SiteId;
            archiverPruningJob.RetentionAction = message.RetentionAction;
            archiverPruningJob.DestinationPhysicalDeviceId = message.DestinationPhysicalDeviceId;
            archiverPruningJob.MediaService = message.MediaService;
            archiverPruningJob.IsDeleteJob = message.IsDeleteJob;
            archiverPruningJob.State = message.State;
            archiverPruningJob.RetentionJob = message.RetentionJob;
            archiverPruningJob.RetentionTimeSpanSeconds = message.RetentionTimeSpanSeconds;
            archiverPruningJob.TenantGroupId = message.TenantGroupId;
            archiverPruningJob.TenantGroupOwner = message.TenantGroupOwner;
            archiverPruningJob.MainIndexStorageInfo = message.MainIndexStorageInfo;
            archiverPruningJob.SubIndexStorageInfo = message.SubIndexStorageInfo;
            archiverPruningJob.NeedStoreInArchiverTier = message.IsArchivedTier;
            archiverPruningJob.AccessTierType = message.AccessTierType;
            archiverPruningJob.ArchiveDateUnit = message.ArchiveDateUnit;
            archiverPruningJob.KeepValue = message.KeepValue;
            archiverPruningJob.RetentionDataTimeType = message.RetentionDataTimeType;
            archiverPruningJob.IsFitSoftDelete = message.IsFitSoftDelete;
            archiverPruningJob.IsSoftDelete = message.IsSoftDelete;
            archiverPruningJob.CurrentStoragePolicyId = message.CurrentStoragePolicyId;
            archiverPruningJob.SoftDeleteKeepValue = message.SoftDeleteKeepValue;
            archiverPruningJob.SoftDeleteDateUnit = message.SoftDeleteDateUnit;
            archiverPruningJob.SoftDeleteTime = message.SoftDeleteTime;
            archiverPruningJob.AgentId = message.AgentId;
            archiverPruningJob.dataSourceForOrphanBlob = message.dataSourceForOrphanBlob;
            archiverPruningJob.IsSystemStorage = message.IsSystemStorage;
            archiverPruningJob.HasMoveActionInPreviousRules = message.HasMoveActionInPreviousRules;
            return archiverPruningJob;
        }

        private Rule? GetRuleBySource(Rule rule, SourceFlag sourceFlag)
        {
            var storageRule = sourceFlag switch
            {
                SourceFlag.SharePoint => rule,
                SourceFlag.OneDrive => rule.OneDriveRule ?? rule,
                SourceFlag.Physical => rule.PhysicalRule,
                SourceFlag.Teams or SourceFlag.Groups or SourceFlag.Exchange => rule.TeamsRule ?? rule,
                SourceFlag.Google => rule.GoogleDriveRule,
                _ => null
            };
            return storageRule;
        }

        private List<Rule> GetRulesForRetentionDeviceCheck()
        {
            var allRules = RuleManagerService.GetRulesFromRecords();
            try
            {
                var aospRules = MiscProfileDao.FindListAsync(p => p.Type == (int)ProfileType.AOSPArchiverRuleForRevIM && !p.IsRemoved)
                    .GetAwaiter()
                    .GetResult();
                if (aospRules != null && aospRules.Count > 0)
                {
                    foreach (var profile in aospRules)
                    {
                        if (string.IsNullOrWhiteSpace(profile.Extension))
                        {
                            continue;
                        }

                        try
                        {
                            var rule = SerializerHelper.DeserializeByDataContractSerializer<Rule>(profile.Extension);
                            if (rule != null)
                            {
                                allRules.Add(rule);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"Skip invalid AOSP retention profile [{profile.Id}] in device check. error:{ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to load AOSP retention profiles for device check. error:{ex}");
            }

            return allRules;
        }
        private ArchiverRetentionMessage AssembleDeleteOrphanDatasMessage(ArchiverIndexSubInfo subIndexInfo, LogicalDeviceDto storageLogical, LogicalDeviceDto indexLogical, bool isTeams)
        {
            string subJobId = GetSubJobId(subIndexInfo.SubSubJobId);
            if (string.IsNullOrEmpty(subJobId))
            {
                logger.Error($"Can't find sub job id by job id {subIndexInfo.SubSubJobId}");
                return null;
            }
            List<ArchiverSiteMasterIndexContract> masterIndexs = null;
            logger.Info($"Assemble delete orphan datas message for job id {subJobId}, isTeams: {isTeams}");
            if (isTeams)
            {
                masterIndexs = ArchiverTeamsMasterIndexDao.GetIndexByJobId(subJobId);
            }
            else
            {
                masterIndexs = ArchiverSiteMasterIndexDao.GetIndexByJobId(subJobId);
            }
            ArchiverSiteMasterIndexContract siteInfo = masterIndexs[0];
            ArchiverRetentionMessage message = new ArchiverRetentionMessage();
            message.FarmName = string.Empty;
            message.WebApp = siteInfo.WebURL;
            message.SiteUrl = siteInfo.SiteURL;
            message.JobId = subIndexInfo.SubSubJobId;
            message.StoragePolicyId = subIndexInfo.StorageId;
            message.IndexLogicalDevice = indexLogical;
            message.LogicalDevice = storageLogical;
            message.RetentionAction = MediaArchiverRetentionAction.DeleteData;
            message.IsDeleteJob = false;
            message.MainIndexStorageInfo = siteInfo.StorageInfo;
            message.SubIndexStorageInfo = subIndexInfo.StorageInfo;
            message.RemoveOrphanedStub = true;
            message.SiteId = siteInfo.SiteId;
            message.RetentionDataTimeType = KeepDateType.ArchiveTime;
            message.RetentionJob = new SOJob();
            return message;
        }
        private ArchiverRetentionMessage AssembleEXODeleteOrphanDatasMessage(EXOArchiverIndexSubInfo subIndexInfo, LogicalDeviceDto storageLogical, LogicalDeviceDto indexLogical)
        {
            string subJobId = GetSubJobId(subIndexInfo.SubSubJobId);
            if (string.IsNullOrEmpty(subJobId))
            {
                logger.Error($"Can't find exo sub job id by job id {subIndexInfo.SubSubJobId}");
                return null;
            }
            ArchiverRetentionMessage message = new ArchiverRetentionMessage();
            message.FarmName = string.Empty;
            message.SiteUrl = subIndexInfo.MailBoxAddress;
            message.JobId = subIndexInfo.SubSubJobId;
            message.StoragePolicyId = subIndexInfo.StorageId;
            message.IndexLogicalDevice = indexLogical;
            message.LogicalDevice = storageLogical;
            message.RetentionAction = MediaArchiverRetentionAction.DeleteData;
            message.IsDeleteJob = false;
            message.MainIndexStorageInfo = subIndexInfo.StorageInfo;
            message.SubIndexStorageInfo = subIndexInfo.StorageInfo;
            message.RemoveOrphanedStub = true;
            message.RetentionDataTimeType = KeepDateType.ArchiveTime;
            message.RetentionJob = new SOJob();
            message.dataSourceForOrphanBlob = DataSourceForOrphanBlob.Mailbox;
            return message;
        }
        private ArchiverRetentionMessage AssembleRetentionMessageForTeams(string currentStorageId, RetentionRule rule, ArchiverIndexSubInfo subIndexInfo, LogicalDeviceDto storageLogical, LogicalDeviceDto indexLogical)
        {
            string subJobId = GetSubJobId(subIndexInfo.SubSubJobId);
            if (subIndexInfo.DataFlag == (int)SourceFlag.Exchange)
            {
                int lastUnderscore = subIndexInfo.SubSubJobId.LastIndexOf('_');
                subJobId = subIndexInfo.SubSubJobId.Substring(0, lastUnderscore - 3); //SO..._XXX
            }
            if (string.IsNullOrEmpty(subJobId))
            {
                return null;
            }
            List<ArchiverSiteMasterIndexContract> masterIndexs = ArchiverTeamsMasterIndexDao.GetIndexByJobId(subJobId);
            if (masterIndexs == null) return null;
            ArchiverSiteMasterIndexContract siteInfo = masterIndexs[0];
            ArchiverRetentionMessage message = new ArchiverRetentionMessage();
            message.FarmName = string.Empty;
            message.WebApp = siteInfo.WebURL;
            message.SiteUrl = siteInfo.SiteURL;
            message.JobId = subIndexInfo.SubSubJobId;
            message.StoragePolicyId = subIndexInfo.StorageId;
            message.ArchiverBackupTime = siteInfo.ArchiverTime;
            message.IndexLogicalDevice = indexLogical;
            message.LogicalDevice = storageLogical;
            message.RetentionAction = rule.IsMove ? MediaArchiverRetentionAction.MoveData : rule.IsMarkDataTier ? MediaArchiverRetentionAction.MarkTier : MediaArchiverRetentionAction.DeleteData;
            message.IsDeleteJob = rule.RemoveTheJob;
            message.MainIndexStorageInfo = siteInfo.StorageInfo;
            message.SubIndexStorageInfo = subIndexInfo.StorageInfo;
            message.IsArchivedTier = rule.IsArchivedTier;
            message.RemoveOrphanedStub = rule.RemoveOrphanedStub;
            message.AccessTierType = rule.TierType;
            message.SiteId = siteInfo.TeamsId;
            message.RetentionJob = new SOJob();
            message.RetentionDataTimeType = rule.RetentionDataTimeType;
            message.KeepValue = rule.KeepValue;
            message.ArchiveDateUnit = rule.ArchiveDateUnit;
            message.IsFitSoftDelete = rule.IsFitSoftDelete;
            message.IsSoftDelete = rule.IsSoftDelete;
            message.CurrentStoragePolicyId = currentStorageId;
            message.SoftDeleteDateUnit = rule.SoftDeleteDateUnit;
            message.SoftDeleteKeepValue = rule.SoftDeleteKeepValue;
            message.SoftDeleteTime = subIndexInfo.SoftDeleteTime;
            if (rule.IsMove)
            {
                //移动数据时的目的端logical device
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.MoveDeviceId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(storageDevice);
                message.DestinationDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                message.DestinationPhysicalDeviceId = rule.MoveDeviceId;
            }
            message.HasMoveActionInPreviousRules = rule.HasMoveActionInPreviousRules;
            return message;
        }

        private ArchiverRetentionMessage AssembleRetentionMessageForGDrive(string currentStorageId, RetentionRule rule, ArchiverIndexSubInfo subIndexInfo, LogicalDeviceDto storageLogical, LogicalDeviceDto indexLogical)
        {
            string subJobId = GetSubJobId(subIndexInfo.SubSubJobId);
            if (string.IsNullOrEmpty(subJobId))
            {
                return null;
            }
            List<ArchiverSiteMasterIndexContract> masterIndexs = ArchiverSiteMasterIndexDao.GetIndexByJobId(subJobId);
            if (masterIndexs == null)
            {
                return null;
            }
            ArchiverSiteMasterIndexContract driveInfo = masterIndexs[0];
            ArchiverRetentionMessage message = new ArchiverRetentionMessage
            {
                FarmName = driveInfo.SiteURL,
                WebApp = driveInfo.WebId,
                SiteUrl = driveInfo.SiteId,
                JobId = subIndexInfo.SubSubJobId,
                StoragePolicyId = subIndexInfo.StorageId,
                ArchiverBackupTime = driveInfo.ArchiverTime,
                IndexLogicalDevice = indexLogical,
                LogicalDevice = storageLogical,
                RetentionAction = rule.IsMove ? MediaArchiverRetentionAction.MoveData : rule.IsMarkDataTier ? MediaArchiverRetentionAction.MarkTier : MediaArchiverRetentionAction.DeleteData,
                IsDeleteJob = rule.RemoveTheJob,
                MainIndexStorageInfo = driveInfo.StorageInfo,
                SubIndexStorageInfo = subIndexInfo.StorageInfo,
                IsArchivedTier = rule.IsArchivedTier,
                RemoveOrphanedStub = rule.RemoveOrphanedStub,
                AccessTierType = rule.TierType,
                SiteId = driveInfo.SiteId,
                RetentionJob = new SOJob(),
                RetentionDataTimeType = rule.RetentionDataTimeType,
                KeepValue = rule.KeepValue,
                ArchiveDateUnit = rule.ArchiveDateUnit,
                IsFitSoftDelete = rule.IsFitSoftDelete,
                IsSoftDelete = rule.IsSoftDelete,
                CurrentStoragePolicyId = currentStorageId,
                SoftDeleteDateUnit = rule.SoftDeleteDateUnit,
                SoftDeleteKeepValue = rule.SoftDeleteKeepValue,
                SoftDeleteTime = subIndexInfo.SoftDeleteTime,
            };
            if (rule.IsMove)
            {
                //移动数据时的目的端logical device
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.MoveDeviceId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(storageDevice);
                message.DestinationDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                message.DestinationPhysicalDeviceId = rule.MoveDeviceId;
            }
            message.HasMoveActionInPreviousRules = rule.HasMoveActionInPreviousRules;
            return message;
        }

        private ArchiverRetentionMessage AssembleRetentionMessageForEXO(string currentStorageId, RetentionRule rule, EXOArchiverIndexSubInfo subIndexInfo, LogicalDeviceDto storageLogical, LogicalDeviceDto indexLogical)
        {
            ArchiverRetentionMessage message = new ArchiverRetentionMessage();
            message.FarmName = string.Empty;
            //message.WebApp = siteInfo.WebURL;
            message.SiteUrl = subIndexInfo.MailBoxAddress;
            message.JobId = subIndexInfo.SubSubJobId;
            message.StoragePolicyId = subIndexInfo.StorageId;
            message.ArchiverBackupTime = subIndexInfo.ArchiverTime;
            message.IndexLogicalDevice = indexLogical;
            message.LogicalDevice = storageLogical;
            message.RetentionAction = rule.IsMove ? MediaArchiverRetentionAction.MoveData : rule.IsMarkDataTier ? MediaArchiverRetentionAction.MarkTier : MediaArchiverRetentionAction.DeleteData;
            message.IsDeleteJob = rule.RemoveTheJob;
            message.MainIndexStorageInfo = subIndexInfo.StorageInfo;
            message.SubIndexStorageInfo = subIndexInfo.StorageInfo;
            message.IsArchivedTier = rule.IsArchivedTier;
            message.RemoveOrphanedStub = rule.RemoveOrphanedStub;
            message.AccessTierType = rule.TierType;
            //message.SiteId = siteInfo.TeamsId;
            message.RetentionJob = new SOJob();
            message.RetentionDataTimeType = rule.RetentionDataTimeType;
            message.KeepValue = rule.KeepValue;
            message.ArchiveDateUnit = rule.ArchiveDateUnit;
            message.IsFitSoftDelete = rule.IsFitSoftDelete;
            message.IsSoftDelete = rule.IsSoftDelete;
            message.CurrentStoragePolicyId = currentStorageId;
            message.SoftDeleteDateUnit = rule.SoftDeleteDateUnit;
            message.SoftDeleteKeepValue = rule.SoftDeleteKeepValue;
            message.SoftDeleteTime = subIndexInfo.SoftDeleteTime;
            if (rule.IsMove)
            {
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.MoveDeviceId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(storageDevice);
                message.DestinationDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                message.DestinationPhysicalDeviceId = rule.MoveDeviceId;
            }
            message.HasMoveActionInPreviousRules = rule.HasMoveActionInPreviousRules;
            return message;
        }
        private ArchiverRetentionMessage AssembleRetentionMessage(string currentStorageId, RetentionRule rule, ArchiverIndexSubInfo subIndexInfo, LogicalDeviceDto storageLogical, LogicalDeviceDto indexLogical)
        {
            string subJobId = GetSubJobId(subIndexInfo.SubSubJobId);
            if (string.IsNullOrEmpty(subJobId))
            {
                return null;
            }
            List<ArchiverSiteMasterIndexContract> masterIndexs = ArchiverSiteMasterIndexDao.GetIndexByJobId(subJobId);
            ArchiverSiteMasterIndexContract siteInfo = masterIndexs[0];
            ArchiverRetentionMessage message = new ArchiverRetentionMessage();
            message.FarmName = string.Empty;
            message.WebApp = siteInfo.WebURL;
            message.SiteUrl = siteInfo.SiteURL;
            message.JobId = subIndexInfo.SubSubJobId;
            message.StoragePolicyId = subIndexInfo.StorageId;
            message.ArchiverBackupTime = siteInfo.ArchiverTime;
            message.IndexLogicalDevice = indexLogical;
            message.LogicalDevice = storageLogical;
            message.RetentionAction = rule.IsMove ? MediaArchiverRetentionAction.MoveData : rule.IsMarkDataTier ? MediaArchiverRetentionAction.MarkTier : MediaArchiverRetentionAction.DeleteData;
            message.IsDeleteJob = rule.RemoveTheJob;
            message.MainIndexStorageInfo = siteInfo.StorageInfo;
            message.SubIndexStorageInfo = subIndexInfo.StorageInfo;
            message.IsArchivedTier = rule.IsArchivedTier;
            message.RemoveOrphanedStub = rule.RemoveOrphanedStub;
            message.AccessTierType = rule.TierType;
            message.SiteId = siteInfo.SiteId;
            message.RetentionJob = new SOJob();
            message.RetentionDataTimeType = rule.RetentionDataTimeType;
            message.KeepValue = rule.KeepValue;
            message.ArchiveDateUnit = rule.ArchiveDateUnit;
            message.IsFitSoftDelete = rule.IsFitSoftDelete;
            message.IsSoftDelete = rule.IsSoftDelete;
            message.CurrentStoragePolicyId = currentStorageId;
            message.SoftDeleteDateUnit = rule.SoftDeleteDateUnit;
            message.SoftDeleteKeepValue = rule.SoftDeleteKeepValue;
            message.SoftDeleteTime = subIndexInfo.SoftDeleteTime;
            message.IsSystemStorage = storageLogical.PhysicalDrives?.FirstOrDefault().IsSystemStorage ?? false;
            message.DeleteStatus = subIndexInfo.DeletedStatus;
            if (rule.IsMove)
            {
                //移动数据时的目的端logical device
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.MoveDeviceId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(storageDevice);
                message.DestinationDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                message.DestinationPhysicalDeviceId = rule.MoveDeviceId;
            }
            message.HasMoveActionInPreviousRules = rule.HasMoveActionInPreviousRules;
            return message;
        }
        private ArchiverRetentionMessage AssembleFSRetentionMessage(string currentStorageId, RetentionRule rule, FSIndexSubInfo subIndexInfo, LogicalDeviceDto storageLogical, LogicalDeviceDto indexLogical)
        {
            string subJobId = GetSubJobId(subIndexInfo.SubSubJobId);
            if (string.IsNullOrEmpty(subJobId))
            {
                return null;
            }
            List<FSMasterIndexContract> masterIndexs = FSMasterIndexDao.GetIndexByJobId(subJobId);
            FSMasterIndexContract siteInfo = masterIndexs[0];
            ArchiverRetentionMessage message = new ArchiverRetentionMessage();
            message.FarmName = string.Empty;
            //message.WebApp = siteInfo.WebURL;
            message.SiteUrl = siteInfo.ConnectionId;
            message.AgentId = siteInfo.AgentId;
            message.JobId = subIndexInfo.SubSubJobId;
            message.StoragePolicyId = subIndexInfo.StorageId;
            message.ArchiverBackupTime = siteInfo.ArchiverTime;
            message.IndexLogicalDevice = indexLogical;
            message.LogicalDevice = storageLogical;
            message.RetentionAction = rule.IsMove ? MediaArchiverRetentionAction.MoveData : rule.IsMarkDataTier ? MediaArchiverRetentionAction.MarkTier : MediaArchiverRetentionAction.DeleteData;
            message.IsDeleteJob = rule.RemoveTheJob;
            message.MainIndexStorageInfo = siteInfo.StorageInfo;
            message.SubIndexStorageInfo = subIndexInfo.StorageInfo;
            message.IsArchivedTier = rule.IsArchivedTier;
            message.RemoveOrphanedStub = rule.RemoveOrphanedStub;
            message.AccessTierType = rule.TierType;
            message.SiteId = siteInfo.ConnectionId;
            message.RetentionJob = new SOJob();
            message.RetentionDataTimeType = rule.RetentionDataTimeType;
            message.KeepValue = rule.KeepValue;
            message.ArchiveDateUnit = rule.ArchiveDateUnit;
            //message.IsFitSoftDelete = rule.IsFitSoftDelete;
            //message.IsSoftDelete = rule.IsSoftDelete;
            message.CurrentStoragePolicyId = currentStorageId;
            //message.SoftDeleteDateUnit = rule.SoftDeleteDateUnit;
            //message.SoftDeleteKeepValue = rule.SoftDeleteKeepValue;
            if (rule.IsMove)
            {
                //移动数据时的目的端logical device
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.MoveDeviceId, needDecryptSecert: true);
                //ArchiverCommonStaticMethod.VerifyFSRetainAvePoint(storageDevice);
                message.DestinationDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                message.DestinationPhysicalDeviceId = rule.MoveDeviceId;
            }
            message.HasMoveActionInPreviousRules = rule.HasMoveActionInPreviousRules;
            return message;
        }
        private string GetSubJobId(string subSubJobId)
        {
            if (!string.IsNullOrEmpty(subSubJobId))
            {
                return subSubJobId.Substring(0, subSubJobId.LastIndexOf("_", StringComparison.CurrentCulture));
            }
            return null;
        }
        private RetentionRule GetMatchedRetentionRule(List<RetentionRule> retentionRules, ArchiverIndexSubInfo subIndexInfo, RetentionSourceFlag retentionSource, bool isSimulateJob)
        {
            RetentionRule retentionRule = null;
            if (string.IsNullOrEmpty(subIndexInfo.CurrentStorageId))
            {
                subIndexInfo.CurrentStorageId = subIndexInfo.StorageId;
            }

            bool hasMoveActionInPreviousRules = false;
            if (subIndexInfo.RetentionCount == null || subIndexInfo.RetentionCount == 0)
            {
                logger.Info($"this subinfo is old data,job id :{subIndexInfo.SubSubJobId}");
                subIndexInfo.RetentionCount = 1;

                //如果Data在备份时使用的Storage中，则直接取第一个rule
                if (subIndexInfo.StorageId.Equals(subIndexInfo.CurrentStorageId, StringComparison.OrdinalIgnoreCase))
                {
                    RetentionRule rule = retentionRules[0];
                    if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                    {
                        retentionRule = rule;
                    }
                }
                else
                {
                    bool flag = false;
                    int tier = 0;
                    foreach (RetentionRule rule in retentionRules)
                    {
                        logger.Info($"Validate retention rule logical, Move logical id {rule.MoveDeviceId}, tier {tier}");
                        tier++;
                        if (flag && rule.SetupDataRetention)
                        {
                            if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                            {
                                retentionRule = rule;
                            }
                            break;
                        }
                        //如果找到了当前存储的deviceid， 则使用下个rule 校验时间并执行
                        if (subIndexInfo.CurrentStorageId.Equals(rule.MoveDeviceId, StringComparison.CurrentCulture))
                        {
                            flag = true;
                        }
                    }
                    //如果当前所在的device 不在设置的retention devices 中，则使用第一个rule 来校验
                    if (!flag)
                    {
                        RetentionRule rule = retentionRules[0];
                        if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                        {
                            retentionRule = rule;
                        }
                    }
                }
            }
            else
            {
                logger.Info($"archive job subindexInfo retention source is {subIndexInfo.RetentionSource},current retention job source is {retentionSource.ToString()}");
                if (subIndexInfo.RetentionSource != null && subIndexInfo.RetentionSource != 0 && subIndexInfo.RetentionSource != (int)retentionSource)
                {
                    subIndexInfo.RetentionCount = 1;
                    subIndexInfo.RetentionSource = (int)retentionSource;
                }
                int index = 1;
                foreach (RetentionRule rule in retentionRules)
                {
                    logger.Info($"Validate retention rule logical, Move logical id {rule.MoveDeviceId}, tier {(AccessTierType)rule.TierType}");
                    //如果找到了当前存储的deviceid， 则使用下个rule 校验时间并执行
                    if (subIndexInfo.CurrentStorageId.Equals(rule.MoveDeviceId, StringComparison.CurrentCulture))
                    {
                        logger.Info($"currentstorage id is matched with rule.MoveDeviceId,id:{subIndexInfo.CurrentStorageId}");
                        index++;
                        hasMoveActionInPreviousRules = true;
                        continue;
                    }
                    if (rule.SetupDataRetention || rule.KeepValue > 0)
                    {
                        logger.Info($"current rule info:markTier:{rule.IsMarkDataTier},IsMove:{rule.IsMove},isDelete:{rule.DeleteTheData},index:{index},RetentionCount:{subIndexInfo.RetentionCount}");
                        if (index > subIndexInfo.RetentionCount)
                        {
                            logger.Info($"current storage retention rule has reset and the move target is same,will set retention count :{index},original:{subIndexInfo.RetentionCount}");
                            subIndexInfo.RetentionCount = index;
                        }
                        if (index == subIndexInfo.RetentionCount)
                        {
                            if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                            {
                                retentionRule = rule;
                                if (rule.IsSoftDelete || subIndexInfo.DeletedStatus == (int)DeletedStatus.SoftDelete)
                                {
                                    logger.Info($"this retention rule is soft delete rule,do not increase retention count,{subIndexInfo.RetentionTime},status:{subIndexInfo.DeletedStatus}");
                                }
                                else
                                {
                                    logger.Info($"this retention rule is not soft delete rule,{subIndexInfo.RetentionTime}");
                                }
                            }
                            if (rule.IsSoftDelete)
                            {
                                var dateTime = GenerateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit);
                                logger.Info($"will check retention soft delete,retention time:{subIndexInfo.RetentionTime},soft delete time:{dateTime.Ticks}");
                                if (ValidateRetentionTime(dateTime.Ticks, rule.SoftDeleteKeepValue, rule.SoftDeleteDateUnit, isSimulateJob))
                                {
                                    logger.Info($"check retention soft delete success,retention time:{dateTime.Ticks}");
                                    if (!isSimulateJob)
                                    {
                                        ArchiverIndexSubInfoDao.UpdateAsync(subIndexInfo).GetAwaiter().GetResult();
                                    }
                                }
                            }
                        }
                        else
                        {
                            logger.Info($"index is :{index},RetentionCount is {subIndexInfo.RetentionCount}");
                            index++;
                            continue;
                        }
                        break;
                    }
                }
            }
            if (hasMoveActionInPreviousRules && retentionRule != null)
            {
                retentionRule.HasMoveActionInPreviousRules = true;
            }
            return retentionRule;
        }
        private RetentionRule GetMatchedFSRetentionRule(List<RetentionRule> retentionRules, FSIndexSubInfo subIndexInfo, RetentionSourceFlag retentionSource, bool isSimulateJob)
        {
            RetentionRule retentionRule = null;
            if (string.IsNullOrEmpty(subIndexInfo.CurrentStorageId))
            {
                subIndexInfo.CurrentStorageId = subIndexInfo.StorageId;
            }

            logger.Info($"archive job subindexInfo retention source is {subIndexInfo.RetentionSource},current retention job source is {retentionSource.ToString()}");
            if (subIndexInfo.RetentionSource != null && subIndexInfo.RetentionSource != 0 && subIndexInfo.RetentionSource != (int)retentionSource)
            {
                subIndexInfo.RetentionCount = 1;
                subIndexInfo.RetentionSource = (int)retentionSource;
            }
            int index = 1;
            foreach (RetentionRule rule in retentionRules)
            {
                logger.Info($"Validate retention rule logical, Move logical id {rule.MoveDeviceId}, tier {(AccessTierType)rule.TierType}");
                //如果找到了当前存储的deviceid， 则使用下个rule 校验时间并执行
                if (subIndexInfo.CurrentStorageId.Equals(rule.MoveDeviceId, StringComparison.CurrentCulture))
                {
                    logger.Info($"currentstorage id is matched with rule.MoveDeviceId,id:{subIndexInfo.CurrentStorageId}");
                    index++;
                    continue;
                }
                if (rule.SetupDataRetention || rule.KeepValue > 0)
                {
                    logger.Info($"current rule info:markTier:{rule.IsMarkDataTier},IsMove:{rule.IsMove},isDelete:{rule.DeleteTheData},index:{index},RetentionCount:{subIndexInfo.RetentionCount}");
                    if (index > subIndexInfo.RetentionCount)
                    {
                        logger.Info($"current storage retention rule has reset and the move target is same,will set retention count :{index},original:{subIndexInfo.RetentionCount}");
                        subIndexInfo.RetentionCount = index;
                    }
                    if (index == subIndexInfo.RetentionCount)
                    {
                        if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                        {
                            retentionRule = rule;
                            logger.Info($"this retention rule is not soft delete rule,{subIndexInfo.RetentionTime}");
                        }
                    }
                    else
                    {
                        logger.Info($"index is :{index},RetentionCount is {subIndexInfo.RetentionCount}");
                        index++;
                        continue;
                    }
                    break;
                }
            }
            return retentionRule;
        }
        private RetentionRule GetMatchedEXORetentionRule(List<RetentionRule> retentionRules, EXOArchiverIndexSubInfo subIndexInfo, RetentionSourceFlag retentionSource, bool isSimulateJob = false)
        {
            RetentionRule retentionRule = null;
            if (string.IsNullOrEmpty(subIndexInfo.CurrentStorageId))
            {
                subIndexInfo.CurrentStorageId = subIndexInfo.StorageId;
            }

            bool hasMoveActionInPreviousRules = false;
            if (subIndexInfo.RetentionCount == null || subIndexInfo.RetentionCount == 0)
            {
                logger.Info($"this subinfo is old data,job id :{subIndexInfo.SubSubJobId}");
                subIndexInfo.RetentionCount = 1;
                if (subIndexInfo.StorageId.Equals(subIndexInfo.CurrentStorageId, StringComparison.OrdinalIgnoreCase))
                {
                    RetentionRule rule = retentionRules[0];
                    if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                    {
                        retentionRule = rule;
                    }
                }
                else
                {
                    bool flag = false;
                    int tier = 0;
                    foreach (RetentionRule rule in retentionRules)
                    {
                        logger.Info($"Validate retention rule logical, Move logical id {rule.MoveDeviceId}, tier {tier}");
                        tier++;
                        if (flag && rule.SetupDataRetention)
                        {
                            if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                            {
                                retentionRule = rule;
                            }
                            break;
                        }
                        if (subIndexInfo.CurrentStorageId.Equals(rule.MoveDeviceId, StringComparison.CurrentCulture))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        RetentionRule rule = retentionRules[0];
                        if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, isSimulateJob))
                        {
                            retentionRule = rule;
                        }
                    }
                }
            }
            else
            {
                logger.Info($"archive job subindexInfo retention source is {subIndexInfo.RetentionSource},current retention job source is {retentionSource.ToString()}");
                if (subIndexInfo.RetentionSource != null && subIndexInfo.RetentionSource != 0 && subIndexInfo.RetentionSource != (int)retentionSource)
                {
                    subIndexInfo.RetentionCount = 1;
                    subIndexInfo.RetentionSource = (int)retentionSource;
                }
                int index = 1;
                foreach (RetentionRule rule in retentionRules)
                {
                    logger.Info($"Validate retention rule logical, Move logical id {rule.MoveDeviceId}, tier {(AccessTierType)rule.TierType}");
                    if (subIndexInfo.CurrentStorageId.Equals(rule.MoveDeviceId, StringComparison.CurrentCulture))
                    {
                        logger.Info($"currentstorage id is matched with rule.MoveDeviceId,id:{subIndexInfo.CurrentStorageId}");
                        index++;
                        hasMoveActionInPreviousRules = true;
                        continue;
                    }
                    if (rule.SetupDataRetention || rule.KeepValue > 0)
                    {
                        logger.Info($"current rule info:markTier:{rule.IsMarkDataTier},IsMove:{rule.IsMove},isDelete:{rule.DeleteTheData},index:{index},RetentionCount:{subIndexInfo.RetentionCount}");
                        if (index > subIndexInfo.RetentionCount)
                        {
                            logger.Info($"current storage retention rule has reset and the move target is same,will set retention count :{index},original:{subIndexInfo.RetentionCount}");
                            subIndexInfo.RetentionCount = index;
                        }
                        if (index == subIndexInfo.RetentionCount)
                        {
                            if (ValidateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit, false))
                            {
                                retentionRule = rule;
                                if (rule.IsSoftDelete || subIndexInfo.DeletedStatus == (int)DeletedStatus.SoftDelete)
                                {
                                    logger.Info($"this retention rule is soft delete rule,do not increase retention count,{subIndexInfo.RetentionTime},status:{subIndexInfo.DeletedStatus}");
                                }
                                else
                                {
                                    logger.Info($"this retention rule is not soft delete rule,{subIndexInfo.RetentionTime}");
                                    subIndexInfo.RetentionCount++;
                                    EXOArchiverIndexSubInfoDao.UpdateAsync(subIndexInfo).GetAwaiter().GetResult();
                                }
                            }
                            if (rule.IsSoftDelete)
                            {
                                var dateTime = GenerateRetentionTime(subIndexInfo.RetentionTime, rule.KeepValue, rule.ArchiveDateUnit);
                                logger.Info($"will check retention soft delete,retention time:{subIndexInfo.RetentionTime},soft delete time:{dateTime.Ticks}");
                                if (ValidateRetentionTime(dateTime.Ticks, rule.SoftDeleteKeepValue, rule.SoftDeleteDateUnit, isSimulateJob))
                                {
                                    logger.Info($"check retention soft delete success,retention time:{dateTime.Ticks}");
                                    EXOArchiverIndexSubInfoDao.UpdateAsync(subIndexInfo).GetAwaiter().GetResult();
                                }
                            }
                        }
                        else
                        {
                            logger.Info($"index is :{index},RetentionCount is {subIndexInfo.RetentionCount}");
                            index++;
                            continue;
                        }
                        break;
                    }
                }
            }
            if (hasMoveActionInPreviousRules && retentionRule != null)
            {
                retentionRule.HasMoveActionInPreviousRules = true;
            }
            return retentionRule;
        }


        private long GetNextRetentionRunTime()
        {
            var schedule = RMScheduleDao.GetScheduleByType(ScheduleType.ArchiveDataRetentionSchedule);
            long nextRunTime = 0;

            if (schedule != null && schedule.Count > 0)
            {
                var defaultSchedule = schedule.FirstOrDefault();
                nextRunTime = defaultSchedule.NextTime;
                logger.Info($"Retention simulate job next run time:{defaultSchedule.NextTime},EndType:{defaultSchedule.EndType},Occurrences:{defaultSchedule.Occurrences},OccurrencesTotal:{defaultSchedule.OccurrencesTotal},NextTime:{defaultSchedule.NextTime},EndTime:{defaultSchedule.EndTime}");
                if (defaultSchedule.EndType == (int)EndType.EndByOccurrences && defaultSchedule.Occurrences >= defaultSchedule.OccurrencesTotal)
                {
                    logger.Info($"Will not run retention simulate job due to occurences is same with OccurrencesTotal:{defaultSchedule.Occurrences},OccurrencesTotal:{defaultSchedule.OccurrencesTotal}");
                    nextRunTime = 0;
                }
                else if (defaultSchedule.EndType == (int)EndType.EndByTime && defaultSchedule.NextTime > defaultSchedule.EndTime)
                {
                    logger.Info($"Will not run retention simulate job due to NextTime is greater than EndTime, NextTime:{defaultSchedule.NextTime},EndTime:{defaultSchedule.EndTime}");
                    nextRunTime = 0;
                }
            }
            else
            {
                logger.Info("Will not run retention simulate job due to there's no ArchiveDataRetentionSchedule.");
            }
            if (nextRunTime == 3155063652000000000)
            {
                logger.Info("Retention simulate job next run time is invalid, reset to 0.");
                nextRunTime = 0;
            }

            return nextRunTime;
        }

        private bool ValidateRetentionTime(long retentionTimeTicks, int keepValue, DateUnit dateUnit, bool isSimulateJob)
        {
            if (keepValue < 0)
            {
                logger.Info($"keep value is zero,return false");
                return false;
            }
            DateTime retentionTime = new DateTime(retentionTimeTicks);
            switch (dateUnit)
            {
                case DateUnit.Year:
                    retentionTime = retentionTime.AddYears(keepValue);
                    break;
                case DateUnit.Month:
                    retentionTime = retentionTime.AddMonths(keepValue);
                    break;
                case DateUnit.Week:
                    retentionTime = retentionTime.AddDays(keepValue * 7);
                    break;
                case DateUnit.Day:
                    retentionTime = retentionTime.AddDays(keepValue);
                    break;
            }
            logger.Info($"ValidateRetentionTime.RetentionTime {retentionTime.Ticks}");
            if (isSimulateJob)
            {
                return retentionTime.Ticks <= GetNextRetentionRunTime(); // Simulate job 10 days buffer
            }
            else
            {
                return retentionTime.Ticks <= DateTime.UtcNow.Ticks;
            }
        }

        private DateTime GenerateRetentionTime(long retentionTimeTicks, int keepValue, DateUnit dateUnit)
        {
            DateTime retentionTime = new DateTime(retentionTimeTicks);
            switch (dateUnit)
            {
                case DateUnit.Year:
                    retentionTime = retentionTime.AddYears(keepValue);
                    break;
                case DateUnit.Month:
                    retentionTime = retentionTime.AddMonths(keepValue);
                    break;
                case DateUnit.Week:
                    retentionTime = retentionTime.AddDays(keepValue * 7);
                    break;
                case DateUnit.Day:
                    retentionTime = retentionTime.AddDays(keepValue);
                    break;
            }
            logger.Info($"GenerateRetentionTime.RetentionTime {retentionTime.Ticks}");
            return retentionTime;
        }
        public RAReturnMessage RunArchiverDeleteRestoredDataJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            if (!LicenseHelperService.IsEnableDeleteRestoreDataFeature())
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            logger.Info($"Start archiver delete restored job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.DeleteRestoredData,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while Retention,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.ConfigureArchiverDeleteRestoredData, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunArchiverDeleteRestoredDataJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start run delete restored data job.");

            var mJobs = RMJobService.GetRunningJobs(new List<JobType>() { JobType.ArchiverMoveIndex, JobType.ConvertStub });

            var jobId = RMJobService.CreateJob(JobType.DeleteRestoredData, jobRunByUser);
            if (mJobs.Count > 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            List<RestoredSitesInfo> restoredSites = GetRestoredSites();
            if (restoredSites != null && restoredSites.Count > 0)
            {
                UpdateDeleteRestoredDataJobConflict(restoredSites, jobId);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_JobConflictOrNotExistData");
                return jobId;
            }

            JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.DeleteRestoredData,
                CommandLine = $"{JobType.DeleteRestoredData} {jobId}",
                RunBy = jobRunBy,
            });

            return jobId;
        }

        private List<RestoredSitesInfo> GetRestoredSites()
        {
            List<RestoredSitesInfo> restoredSites = RestoredSitesInfoDao
                .GetAll()
                .Where(item => !string.IsNullOrEmpty(item.SiteUrl))
                .GroupBy(item => item.SiteUrl)
                .ToDictionary(item => item.Key, item => item.ToList().First())
                .Values.ToList();
            var runningSites = RMJobService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, restoredSites.Select(site => site.SiteUrl), true);
            logger.Info($"The sites that are executing the job are [{string.Join("; ", runningSites)}]");

            List<RestoredSitesInfo> orderSites = restoredSites.OrderByDescending(site => site.SiteUrl.Length).ToList();
            foreach (var runningSite in runningSites)
            {
                foreach (RestoredSitesInfo site in orderSites)
                {
                    if (RuleSPTreeUtil.IsPrefixWithSlash(runningSite, site.SiteUrl)
                        || RuleSPTreeUtil.IsPrefixWithSlash(site.SiteUrl, runningSite))
                    {
                        restoredSites.Remove(site);
                    }
                }
            }

            return restoredSites;
        }

        private void UpdateDeleteRestoredDataJobConflict(List<RestoredSitesInfo> restoredSites, string jobId)
        {
            if (restoredSites != null && restoredSites.Count > 0)
            {
                RMJobService.UpdateJobExtension(jobId, new ArchiveJobMonitorExtension
                {
                    IsGroupLevelArchive = false,
                    treeMode = TreeMode.SO,
                    SiteUrls = restoredSites.Select(site => site.SiteUrl).ToList()
                });
            }
        }

        private List<Guid> GetAppliedRuleIds(RMSPTreeNode node, ContentSourceType type = ContentSourceType.SharePoint)
        {
            List<Guid> ruleIds = new();
            var settingId = type == ContentSourceType.Teams ? GetTeamsArchiverSettingId(node) : GetArchiverSettingId(node);
            var rules = EXOSettingRuleDao.GetArchiverMappingRules(settingId, (int)AvePoint.RA.DB.Dao.Impl.RuleType.Archiver);
            if (rules != null && rules.Count > 0)
            {
                ruleIds = rules.Select(r => r.RuleId).ToList();
            }
            return ruleIds;
        }

        public Guid GetTeamsArchiverSettingId(RMSPTreeNode node)
        {
            var siteId = Guid.Empty;
            var teamsId = Guid.Empty;
            if (node.Level != (int)NodeLevel.WebApplication)
            {
                var siteCollectionNode = node.GetSiteCollectionNode();
                siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                teamsId = new Guid(node.GetTeamsNode().TeamsId);
            }
            var setting = ArchiverSettingDao.LoadArchiverSettingByContentSource(new Guid(node.SPObjectId), siteId, teamsId, ContentSourceType.Teams);
            return setting != null ? setting.Id : Guid.Empty;
        }

        public Guid GetArchiverSettingId(RMSPTreeNode node)
        {
            var siteId = Guid.Empty;
            if (node.Level != (int)NodeLevel.WebApplication)
            {
                siteId = new Guid(node.GetSiteCollectionNode().SPObjectId);
            }
            var setting = ArchiverSettingDao.LoadArchiverSetting(new Guid(node.SPObjectId), siteId);
            return setting != null ? setting.Id : Guid.Empty;
        }

        public Guid GetTeamsArchiverSettingId(Guid id, Guid siteId, Guid teamsId)
        {
            var setting = ArchiverSettingDao.LoadTeamsArchiverSetting(id, siteId, teamsId);
            return setting?.Id ?? Guid.Empty;
        }

        private string CreateSubJobForDisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, string scope, string o365TenantId)
        {
            var subJobIndexDigits = GetSubJobIndexDigits(subJobCount);
            var subJob = BuildSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, subJobIndexDigits, tempList, sendNow, scope, o365TenantId);
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} , Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            if (JobServiceUtility.NewJobDetailsJobs.Contains((int)subJob.JobType))
            {
                using (var progresExecutor = AvePoint.RA.SharePoint.Common.JobExecutionProgress.JobExecutionProgressStatisticExecutor.Instance)
                {
                    logger.Info("Init progress for sub job {0}, type {1}", subJob.Id, subJob.JobType);
                    progresExecutor.InitializeJobExecutionProgressStatictics(subJob.String1, subJob.Id, subJob.ParentId, subJob.JobType);
                }
            }
            return subJob.Id;
        }

        private static int GetSubJobIndexDigits(int subJobCount)
        {
            return Math.Max(3, subJobCount.ToString(CultureInfo.InvariantCulture).Length);
        }

        private static string BuildPaddedSubJobId(string jobId, int currentSubjobIndex, int subJobIndexDigits)
        {
            var indexText = currentSubjobIndex.ToString("D" + subJobIndexDigits, CultureInfo.InvariantCulture);
            return string.Concat(jobId, "_", indexText);
        }

        private RMSubJob BuildSubJobForDisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, int subJobIndexDigits, List<RMSPTreeNode> tempList, bool sendNow, string scope, string o365TenantId)
        {
            string subJobId = BuildPaddedSubJobId(jobId, currentSubjobIndex, subJobIndexDigits);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            subJob.String1 = scope;
            return subJob;
        }

        private string CreateSubJobForMoveIndex(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, RMArchiverMoveIndexInfo indexMoveInfo, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(indexMoveInfo) };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        private string CreateSubJobForVEOMerge(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<string> tempList, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        private string CreateSubJobForMoveDataTier(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, MoveDataTierContent dataTierContent, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(dataTierContent) };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        private string CreateSubJobForAdjustStorageSize(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        private string CreateSubJobForRetention(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, string siteRetentionInfo, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = siteRetentionInfo };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        private string CreateSubJobForRetentionV2(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, string siteRetentionInfo, bool sendNow, string objectPath)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, String1 = objectPath };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = siteRetentionInfo };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        public List<RMSPTreeNode> AssembleDisposalRunnableNodeForImport(RMSPTreeNode selectedNode, List<string> importSiteUrls)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                if (importSiteUrls.IsNullOrEmpty())
                {
                    return availableNode;
                }
                var settingIdSet = new HashSet<Guid>(ArchiverSettingDao
                    .LoadArchiverSettingsUnderGroup(new Guid(selectedNode.SPObjectId))
                    .Select(s => s.SPObjectId));
                foreach (var site in importSiteUrls)
                {
                    var existingSite = RMRemoteNodeDao.GetRemoteSiteCollectionByExactUrl(site);
                    if (existingSite == null)
                    {
                        logger.Info("Site collection not exist, site:{0}", site);
                        continue;
                    }

                    var siteNode = RMDtoConverter.ConvertRemoteSite2RMTree(existingSite);
                    siteNode.Parent = selectedNode;
                    siteNode.O365TenantId = existingSite.TenantId;

                    //skip site collections has unique setting
                    if (!Guid.TryParse(siteNode.SPObjectId, out var siteObjectId))
                    {
                        logger.Warn("Skip site due to invalid site object id, site:{0}, objectId:{1}", site, siteNode.SPObjectId);
                        continue;
                    }

                    if (selectedNode.UserArchiverImportFile || !settingIdSet.Contains(siteObjectId))
                    {
                        siteNode.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                        siteNode.SupportLockedSite = selectedNode.SupportLockedSite;
                        siteNode.SupportArchivedTeams = selectedNode.SupportArchivedTeams;
                        availableNode.Add(siteNode);
                    }
                }
            }
            else
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                if (ValidateSiteExist(siteNode))
                {
                    selectedNode.O365TenantId = siteNode.O365TenantId;
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        public List<RMSPTreeNode> AssembleDisposalRunnableNode(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                var settingIdSet = new HashSet<Guid>(ArchiverSettingDao
                    .LoadArchiverSettingsUnderGroup(new Guid(selectedNode.SPObjectId))
                    .Select(s => s.SPObjectId));
                foreach (RMSPTreeNode site in GetPagedDisposalSiteCollections(selectedNode))
                {
                    //skip site collections has unique setting
                    if (!Guid.TryParse(site.SPObjectId, out var siteObjectId))
                    {
                        logger.Warn("Skip site due to invalid site object id, site:{0}, objectId:{1}", site.FullPath, site.SPObjectId);
                        continue;
                    }

                    if (selectedNode.UserArchiverImportFile || !settingIdSet.Contains(siteObjectId))
                    {
                        site.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                        site.SupportLockedSite = selectedNode.SupportLockedSite;
                        site.SupportArchivedTeams = selectedNode.SupportArchivedTeams;
                        availableNode.Add(site);
                    }
                }
            }
            else
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                if (ValidateSiteExist(siteNode))
                {
                    selectedNode.O365TenantId = siteNode.O365TenantId;
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }


        private void CreateSubJobsByStream(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            List<JobType> shouldCheckConflictTypes,
            List<string> archiverImportSitesUrl,
            bool useArchiverImportFile,
            string folderFullPath,
            int estimatedSiteCount)
        {
            int totalCount = 0;
            int oneDriveRuleCheckPassedCount = 0;
            int importFilterPassedCount = 0;
            int conflictFilterPassedCount = 0;
            int currentSubjobIndex = 0;
            int subJobIndexDigits = GetSubJobIndexDigits(estimatedSiteCount);

            var importUrlSet = useArchiverImportFile
                ? new HashSet<string>(archiverImportSitesUrl ?? new List<string>(), StringComparer.OrdinalIgnoreCase)
                : null;
            var conflictFilterBatch = new List<RMSPTreeNode>(DisposalConflictFilterBatchSize);
            var pendingSubJobs = new List<RMSubJob>(SubJobBulkInsertBatchSize);
            var nodeStream = useArchiverImportFile
                ? EnumerateDisposalRunnableNodeStreamForImport(selectedNode, archiverImportSitesUrl)
                : EnumerateDisposalRunnableNodeStream(selectedNode);

            foreach (var node in nodeStream)
            {
                totalCount++;
                if (!CheckOneDriveForSiteCollectionLevelRule(node))
                {
                    continue;
                }

                oneDriveRuleCheckPassedCount++;
                if (useArchiverImportFile && (importUrlSet == null || !importUrlSet.Contains(node.FullPath)))
                {
                    continue;
                }

                importFilterPassedCount++;
                conflictFilterBatch.Add(node);
                if (conflictFilterBatch.Count < DisposalConflictFilterBatchSize)
                {
                    continue;
                }
                else if (CheckWhetherJobShouldStop(jobId))
                {
                    return;
                }

                conflictFilterPassedCount += AppendSubJobsFromConflictFilteredBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    shouldCheckConflictTypes,
                    folderFullPath,
                    subJobIndexDigits,
                    ref currentSubjobIndex,
                    conflictFilterBatch,
                    pendingSubJobs,
                    estimatedSiteCount);
            }

            if (conflictFilterBatch.Count > 0)
            {
                conflictFilterPassedCount += AppendSubJobsFromConflictFilteredBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    shouldCheckConflictTypes,
                    folderFullPath,
                    subJobIndexDigits,
                    ref currentSubjobIndex,
                    conflictFilterBatch,
                    pendingSubJobs,
                    estimatedSiteCount);
            }

            if (pendingSubJobs.Count > 0)
            {
                //SubJobDao.BulkCreateJobs(pendingSubJobs, SubJobBulkInsertBatchSize);
                //pendingSubJobs.Clear();
                CreateSubJobsAndInitProgress(jobType, currentSubjobIndex, pendingSubJobs);
            }

            if (CheckWhetherJobShouldStop(jobId))
            {
                return;
            }

            if (totalCount == 0)
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                return;
            }

            if (oneDriveRuleCheckPassedCount == 0)
            {
                logger.Warn("all is onedrive node, and all is un orphaned node and only config site collection level rule");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_JMD_AllODIsUnOrphanedAndOnlyConfigSiteCollectionLevelRule");
                return;
            }

            if (importFilterPassedCount == 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ArchiverImportSkip");
                return;
            }

            if (conflictFilterPassedCount == 0)
            {
                logger.Warn("not exsite can run job,will skip current job");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return;
            }

            var subJobCount = conflictFilterPassedCount;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);
            logger.Info("all sub jobs were created correctlly, jobId is {0}, total count is {1}", jobId, subJobCount);
            var subJobWeight = 100d / subJobCount;
            if (!SubJobDao.UpdateSubJobWeightByParentId(jobId, subJobWeight))
            {
                logger.Warn("Failed to update sub job weights in batch, jobId:{0}, targetWeight:{1}", jobId, subJobWeight);
            }
        }

        private IEnumerable<RMSPTreeNode> EnumerateDisposalRunnableNodeStreamForImport(RMSPTreeNode selectedNode, List<string> importSiteUrls)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                if (importSiteUrls.IsNullOrEmpty())
                {
                    yield break;
                }

                var settingIdSet = new HashSet<Guid>(ArchiverSettingDao
                    .LoadArchiverSettingsUnderGroup(new Guid(selectedNode.SPObjectId))
                    .Select(s => s.SPObjectId));

                foreach (var siteUrl in importSiteUrls)
                {
                    var existingSite = RMRemoteNodeDao.GetRemoteSiteCollectionByExactUrl(siteUrl);
                    if (existingSite == null)
                    {
                        logger.Info("Site collection not exist, site:{0}", siteUrl);
                        continue;
                    }

                    var siteNode = RMDtoConverter.ConvertRemoteSite2RMTree(existingSite);
                    siteNode.Parent = selectedNode;
                    siteNode.O365TenantId = existingSite.TenantId;

                    if (!Guid.TryParse(siteNode.SPObjectId, out var siteObjectId))
                    {
                        logger.Warn("Skip site due to invalid site object id, site:{0}, objectId:{1}", siteUrl, siteNode.SPObjectId);
                        continue;
                    }

                    if (selectedNode.UserArchiverImportFile || !settingIdSet.Contains(siteObjectId))
                    {
                        siteNode.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                        siteNode.SupportLockedSite = selectedNode.SupportLockedSite;
                        siteNode.SupportArchivedTeams = selectedNode.SupportArchivedTeams;
                        yield return siteNode;
                    }
                }

                yield break;
            }

            var siteCollectionNode = selectedNode.GetSiteCollectionNode();
            if (ValidateSiteExist(siteCollectionNode))
            {
                selectedNode.O365TenantId = siteCollectionNode.O365TenantId;
                yield return selectedNode;
            }
            else
            {
                logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
            }
        }

        private void CreateSOPreScanSubJobsByStream(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            List<JobType> types,
            int estimatedSiteCount)
        {
            int totalCount = 0;
            int oneDriveRuleCheckPassedCount = 0;
            int conflictFilterPassedCount = 0;
            int currentSubjobIndex = 0;
            int subJobIndexDigits = GetSubJobIndexDigits(estimatedSiteCount);

            var runningScopes = new HashSet<string>(RMJobService.GetRunningArchiverJobsScopes(types) ?? new List<string>());
            var conflictFilterBatch = new List<RMSPTreeNode>(DisposalConflictFilterBatchSize);
            var pendingSubJobs = new List<RMSubJob>(SubJobBulkInsertBatchSize);

            foreach (var node in EnumerateDisposalRunnableNodeStream(selectedNode))
            {
                totalCount++;

                if (!CheckOneDriveForSiteCollectionLevelRule(node))
                {
                    continue;
                }

                oneDriveRuleCheckPassedCount++;
                conflictFilterBatch.Add(node);
                if (conflictFilterBatch.Count < DisposalConflictFilterBatchSize)
                {
                    continue;
                }
                else if (CheckWhetherJobShouldStop(jobId))
                {
                    return;
                }

                conflictFilterPassedCount += AppendSOPreScanSubJobsFromConflictFilteredBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    runningScopes,
                    subJobIndexDigits,
                    ref currentSubjobIndex,
                    conflictFilterBatch,
                    pendingSubJobs,
                    estimatedSiteCount);
            }

            if (conflictFilterBatch.Count > 0)
            {
                conflictFilterPassedCount += AppendSOPreScanSubJobsFromConflictFilteredBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    runningScopes,
                    subJobIndexDigits,
                    ref currentSubjobIndex,
                    conflictFilterBatch,
                    pendingSubJobs,
                    estimatedSiteCount);
            }

            if (pendingSubJobs.Count > 0)
            {
                SubJobDao.BulkCreateJobs(pendingSubJobs, SubJobBulkInsertBatchSize);
                pendingSubJobs.Clear();
            }

            if (CheckWhetherJobShouldStop(jobId))
            {
                return;
            }

            if (totalCount == 0)
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                return;
            }

            if (oneDriveRuleCheckPassedCount == 0)
            {
                logger.Warn("all is onedrive node, and all is un orphaned node and only config site collection level rule");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_JMD_AllODIsUnOrphanedAndOnlyConfigSiteCollectionLevelRule");
                return;
            }

            if (conflictFilterPassedCount == 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return;
            }

            SubJobDao.UpdateSubJobCount(jobId, conflictFilterPassedCount);
            var subJobWeight = 100d / conflictFilterPassedCount;
            if (!SubJobDao.UpdateSubJobWeightByParentId(jobId, subJobWeight))
            {
                logger.Warn("Failed to update sub job weights in batch, jobId:{0}, targetWeight:{1}", jobId, subJobWeight);
            }
        }

        private int AppendSOPreScanSubJobsFromConflictFilteredBatch(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            HashSet<string> runningScopes,
            int subJobIndexDigits,
            ref int currentSubjobIndex,
            List<RMSPTreeNode> conflictFilterBatch,
            List<RMSubJob> pendingSubJobs,
            int estimatedSiteCount)
        {
            var filteredNodes = FilterSOPreScanConflictBatch(conflictFilterBatch, selectedNode, runningScopes);
            conflictFilterBatch.Clear();
            int addedCount = 0;

            foreach (var filteredNode in filteredNodes)
            {
                var subJobNodes = new List<RMSPTreeNode>(1) { filteredNode };
                var subJob = BuildSubJobForDisposal(jobId, currentSubjobIndex, jobType, estimatedSiteCount, subJobIndexDigits, subJobNodes, false, filteredNode.FullPath, filteredNode.O365TenantId);
                pendingSubJobs.Add(subJob);
                if (pendingSubJobs.Count >= SubJobBulkInsertBatchSize)
                {
                    SubJobDao.BulkCreateJobs(pendingSubJobs, SubJobBulkInsertBatchSize);
                    pendingSubJobs.Clear();
                }
                currentSubjobIndex++;
                addedCount++;
            }

            return addedCount;
        }

        private List<RMSPTreeNode> FilterSOPreScanConflictBatch(
            List<RMSPTreeNode> nodes,
            RMSPTreeNode selectedNode,
            HashSet<string> runningScopes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new List<RMSPTreeNode>();
            }

            if (runningScopes == null || runningScopes.Count == 0)
            {
                return nodes.ToList();
            }

            if (selectedNode.Level != (int)NodeLevel.WebApplication
                && nodes.Count == 1
                && string.Equals(nodes[0].FullPath, selectedNode.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                return nodes.ToList();
            }

            return nodes.Where(node => !runningScopes.Contains(node.Name)).ToList();
        }

        private int AppendSubJobsFromConflictFilteredBatch(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            List<JobType> shouldCheckConflictTypes,
            string folderFullPath,
            int subJobIndexDigits,
            ref int currentSubjobIndex,
            List<RMSPTreeNode> conflictFilterBatch,
            List<RMSubJob> pendingSubJobs,
            int estimatedSiteCount)
        {
            var filteredNodes = FilterDisposalConflictBatch(conflictFilterBatch, selectedNode, shouldCheckConflictTypes, folderFullPath);
            conflictFilterBatch.Clear();
            int addedCount = 0;

            foreach (var filteredNode in filteredNodes)
            {
                var subJobNodes = new List<RMSPTreeNode>(1) { filteredNode };
                var subJob = BuildSubJobForDisposal(jobId, currentSubjobIndex, jobType, estimatedSiteCount, subJobIndexDigits, subJobNodes, false, filteredNode.Level == (int)NodeLevel.Folder ? filteredNode.FullUrl : filteredNode.FullPath, filteredNode.O365TenantId);
                pendingSubJobs.Add(subJob);
                if (pendingSubJobs.Count >= SubJobBulkInsertBatchSize)
                {
                    CreateSubJobsAndInitProgress(jobType, currentSubjobIndex, pendingSubJobs);
                }
                currentSubjobIndex++;
                addedCount++;
            }

            return addedCount;
        }

        private void CreateSubJobsAndInitProgress(JobType jobType, int currentSubjobIndex, List<RMSubJob> pendingSubJobs)
        {
            using PerformanceScope scope = new PerformanceScope($"BulkCreateJobs in SO job function RealRunArchiverBackupJobOnSelectedNode , and currentSubjobIndex is {currentSubjobIndex}");
            SubJobDao.BulkCreateJobs(pendingSubJobs, SubJobBulkInsertBatchSize);
            if (JobServiceUtility.NewJobDetailsJobs.Contains((int)jobType))
            {
                logger.Info($"Init progress {jobType} for {pendingSubJobs.Count} sub jobs.");
                _jobProgressDao.BatchAddJobProgressesBySubJobsAsync(pendingSubJobs).ExecuteAsyncTask();
            }
            pendingSubJobs.Clear();
        }

        private List<RMSPTreeNode> FilterDisposalConflictBatch(
            List<RMSPTreeNode> nodes,
            RMSPTreeNode selectedNode,
            List<JobType> shouldCheckConflictTypes,
            string folderFullPath)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new List<RMSPTreeNode>();
            }

            if (shouldCheckConflictTypes == null || shouldCheckConflictTypes.Count == 0)
            {
                return nodes.ToList();
            }

            // Avoid self-conflict only for SiteCollection single selection.
            // Other node types (Site/List/Folder) should still use the normal conflict filter.
            if ((selectedNode.Level != (int)NodeLevel.WebApplication
                && nodes.Count == 1
                && string.Equals(nodes[0].FullPath, selectedNode.FullPath, StringComparison.OrdinalIgnoreCase))
                || (selectedNode.UserArchiverImportFile && selectedNode.Level == (int)NodeLevel.WebApplication))
            {
                return nodes.ToList();
            }

            var siteCollectionUrls = nodes
                .Select(node => node.GetSiteCollectionNode()?.FullPath)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (siteCollectionUrls.Count == 0)
            {
                return new List<RMSPTreeNode>();
            }

            var runningSiteUrls = RMJobService.GetRunningArchiverJobSiteUrl(shouldCheckConflictTypes, siteCollectionUrls);
            if (runningSiteUrls == null || runningSiteUrls.Count == 0)
            {
                return nodes.ToList();
            }

            return RuleSPTreeUtil.FilterSCAvailableNodeByRunningUrl(nodes, runningSiteUrls, selectedNode, folderFullPath);
        }

        private IEnumerable<RMSPTreeNode> EnumerateDisposalRunnableNodeStream(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                var settingIdSet = new HashSet<Guid>(ArchiverSettingDao
                    .LoadArchiverSettingsUnderGroup(new Guid(selectedNode.SPObjectId))
                    .Select(s => s.SPObjectId));

                foreach (RMSPTreeNode site in GetPagedDisposalSiteCollections(selectedNode))
                {
                    if (!Guid.TryParse(site.SPObjectId, out var siteObjectId))
                    {
                        logger.Warn("Skip site due to invalid site object id, site:{0}, objectId:{1}", site.FullPath, site.SPObjectId);
                        continue;
                    }

                    if (!selectedNode.UserArchiverImportFile && settingIdSet.Contains(siteObjectId))
                    {
                        continue;
                    }

                    site.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                    site.SupportLockedSite = selectedNode.SupportLockedSite;
                    site.SupportArchivedTeams = selectedNode.SupportArchivedTeams;
                    yield return site;
                }
                yield break;
            }

            var siteNode = selectedNode.GetSiteCollectionNode();
            if (ValidateSiteExist(siteNode))
            {
                selectedNode.O365TenantId = siteNode.O365TenantId;
                yield return selectedNode;
            }
            else
            {
                logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
            }
        }

        private IEnumerable<RMSPTreeNode> EnumerateTeamsDisposalRunnableNodeStream(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                var settingIdSet = new HashSet<Guid>(ArchiverSettingDao
                    .LoadArchiverSettingsUnderGroup(new Guid(selectedNode.SPObjectId), ContentSourceType.Teams)
                    .Select(s => s.SPObjectId));

                foreach (var teamsNode in GetPagedTeamsSiteCollections(selectedNode))
                {
                    if (!Guid.TryParse(teamsNode.SPObjectId, out var teamsObjectId))
                    {
                        logger.Warn("Skip teams due to invalid object id, name:{0}, objectId:{1}", teamsNode.Name, teamsNode.SPObjectId);
                        continue;
                    }

                    if (!settingIdSet.Contains(teamsObjectId) || selectedNode.UserArchiverImportFile)
                    {
                        teamsNode.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                        teamsNode.SupportLockedSite = selectedNode.SupportLockedSite;
                        teamsNode.SupportArchivedTeams = selectedNode.SupportArchivedTeams;
                        yield return teamsNode;
                    }
                }

                yield break;
            }

            var teamsNodeInSelection = selectedNode.GetTeamsNode();
            if (ValidateTeamsExist(teamsNodeInSelection))
            {
                selectedNode.O365TenantId = teamsNodeInSelection.O365TenantId;
                yield return selectedNode;
            }
            else
            {
                logger.Info("Teams not exist, site:{0}", selectedNode.Name);
            }
        }

        private IEnumerable<RMSPTreeNode> EnumerateTeamsArchiverRunnableNodeStream(
            RMSPTreeNode selectedNode,
            HashSet<string> importTeamSet)
        {
            if (selectedNode.Level != (int)NodeLevel.Office365GroupEntire && selectedNode.Level != (int)NodeLevel.WebApplication)
            {
                foreach (var node in EnumerateTeamsDisposalRunnableNodeStream(selectedNode))
                {
                    yield return node;
                }
                yield break;
            }

            var groupNode = selectedNode.GetGroupNode();
            if (groupNode == null || !Guid.TryParse(groupNode.SPObjectId, out var groupId))
            {
                logger.Warn("Skip teams archiver subjob creation because the container id is invalid. Node:{0}", selectedNode.FullPath);
                yield break;
            }

            var settings = ArchiverSettingDao.LoadArchiverSettingsUnderGroup(groupId, ContentSourceType.Teams);
            var teamsLevelSettingIds = GetTeamsLevelArchiverSettingIds(settings);
            var teamSettingsByTeamsId = BuildTeamsLevelSettingsByTeamId(settings);
            var uniqueSiteSettingIdsByTeamId = BuildUniqueSiteSettingIdsByTeamId(settings);
            var hasContainerTeamsLevelRule = HasContainerTeamsLevelRule(selectedNode, teamsLevelSettingIds, settings);
            var teamNodes = selectedNode.Level == (int)NodeLevel.WebApplication
                ? GetPagedTeamsSiteCollections(selectedNode)
                : EnumerateSelectedTeamsNode(selectedNode);
            var pendingSiteLevelTeams = new List<RMSPTreeNode>(TeamsSiteLookupBatchSize);

            foreach (var teamNode in teamNodes)
            {
                if (!Guid.TryParse(teamNode.TeamsId, out var teamsId))
                {
                    logger.Debug("Skip teams due to invalid teams id, name:{0}, teamsId:{1}", teamNode.Name, teamNode.TeamsId);
                    continue;
                }

                if (importTeamSet != null && !importTeamSet.Contains(teamNode.FullPath))
                {
                    continue;
                }

                teamNode.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                teamNode.SupportLockedSite = selectedNode.SupportLockedSite;
                teamNode.SupportArchivedTeams = selectedNode.SupportArchivedTeams;

                if (teamSettingsByTeamsId.TryGetValue(teamsId, out var teamSettings)
                    && teamSettings.FirstOrDefault() is { } teamSetting
                    && teamSetting.EnableArchiverManagement != (int)EnableRecordManagementSetting.Enable)
                {
                    logger.Debug("Skip team with disabled archiving setting. Team:{0}, TeamsId:{1}", teamNode.FullPath, teamsId);
                    continue;
                }

                if (HasTeamsLevelArchiverRule(teamsId, teamSettingsByTeamsId, teamsLevelSettingIds, hasContainerTeamsLevelRule))
                {
                    yield return teamNode;
                    continue;
                }

                pendingSiteLevelTeams.Add(teamNode);
                if (pendingSiteLevelTeams.Count < TeamsSiteLookupBatchSize)
                {
                    continue;
                }

                foreach (var siteNode in EnumerateTeamsArchiverSiteNodesByBatch(
                             pendingSiteLevelTeams,
                             uniqueSiteSettingIdsByTeamId,
                             selectedNode.UserArchiverImportFile))
                {
                    yield return siteNode;
                }

                pendingSiteLevelTeams.Clear();
            }

            if (pendingSiteLevelTeams.Count > 0)
            {
                foreach (var siteNode in EnumerateTeamsArchiverSiteNodesByBatch(
                             pendingSiteLevelTeams,
                             uniqueSiteSettingIdsByTeamId,
                             selectedNode.UserArchiverImportFile))
                {
                    yield return siteNode;
                }
            }
        }

        private IEnumerable<RMSPTreeNode> EnumerateSelectedTeamsNode(RMSPTreeNode selectedNode)
        {
            var teamsNode = selectedNode.GetTeamsNode();
            if (ValidateTeamsExist(teamsNode))
            {
                selectedNode.O365TenantId = teamsNode.O365TenantId;
                yield return selectedNode;
            }
            else
            {
                logger.Info("Teams not exist, site:{0}", selectedNode.Name);
            }
        }

        private HashSet<Guid> GetTeamsLevelArchiverSettingIds(List<RMArchiverSetting> settings)
        {
            var settingIds = settings.Select(setting => setting.Id).Distinct().ToList();
            if (settingIds.Count == 0)
            {
                return new HashSet<Guid>();
            }

            var mappings = EXOSettingRuleDao.GetAllTeamsNodeRuleMappings(settingIds)
                .Where(mapping => mapping.Type == (int)AvePoint.RA.DB.Dao.Impl.RuleType.Archiver)
                .ToList();
            var mappedRuleIds = mappings.Select(mapping => mapping.RuleId).Distinct().ToList();
            var teamsLevelRuleIds = new HashSet<Guid>(RMRuleDao.GetRulesByIds(mappedRuleIds)
                .Where(rule => rule.RuleLevel == (int)PolicyLevel.Teams)
                .Select(rule => rule.RuleId));

            return mappings
                .Where(mapping => teamsLevelRuleIds.Contains(mapping.RuleId))
                .Select(mapping => mapping.ScopeId)
                .ToHashSet();
        }

        private Dictionary<Guid, List<RMArchiverSetting>> BuildTeamsLevelSettingsByTeamId(List<RMArchiverSetting> settings)
        {
            return settings
                .Where(setting => setting.TeamsId != Guid.Empty
                    && setting.SiteId == Guid.Empty
                    && setting.SPObjectId == setting.TeamsId)
                .GroupBy(setting => setting.TeamsId)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        private Dictionary<Guid, HashSet<Guid>> BuildUniqueSiteSettingIdsByTeamId(List<RMArchiverSetting> settings)
        {
            return settings
                .Where(setting => setting.TeamsId != Guid.Empty
                    && setting.SiteId != Guid.Empty
                    && setting.SPObjectId == setting.SiteId)
                .GroupBy(setting => setting.TeamsId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(setting => setting.SPObjectId).ToHashSet());
        }

        private bool HasContainerTeamsLevelRule(
            RMSPTreeNode selectedNode,
            HashSet<Guid> teamsLevelSettingIds,
            List<RMArchiverSetting> settings)
        {
            if (selectedNode.Level != (int)NodeLevel.WebApplication
                || !Guid.TryParse(selectedNode.SPObjectId, out var containerId))
            {
                return false;
            }

            return settings.Any(setting =>
                setting.TeamsId == Guid.Empty
                && setting.SiteId == Guid.Empty
                && setting.SPObjectId == containerId
                && teamsLevelSettingIds.Contains(setting.Id));
        }

        private bool HasTeamsLevelArchiverRule(
            Guid teamsId,
            Dictionary<Guid, List<RMArchiverSetting>> teamSettingsByTeamsId,
            HashSet<Guid> teamsLevelSettingIds,
            bool hasContainerTeamsLevelRule)
        {
            if (teamSettingsByTeamsId.TryGetValue(teamsId, out var teamSettings))
            {
                return teamSettings.Any(setting => teamsLevelSettingIds.Contains(setting.Id));
            }

            return hasContainerTeamsLevelRule;
        }

        private IEnumerable<RMSPTreeNode> EnumerateTeamsArchiverSiteNodesByBatch(
            List<RMSPTreeNode> teamNodes,
            Dictionary<Guid, HashSet<Guid>> uniqueSiteSettingIdsByTeamId,
            bool useArchiverImportFile)
        {
            if (teamNodes == null || teamNodes.Count == 0)
            {
                yield break;
            }

            var teamIds = teamNodes
                .Select(node => node.TeamsId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (teamIds.Count == 0)
            {
                yield break;
            }

            var allowedStates = new[]
            {
                SiteCollectionState.AccessAll,
                SiteCollectionState.AccessSome
            };

            var queryStopwatch = Stopwatch.StartNew();
            var siteCollections = RMRemoteNodeDao.GetRemoteSiteCollectionsByTeamsIds(teamIds, allowedStates);
            queryStopwatch.Stop();
            logger.Info(
                "Loaded Teams site-level archiver candidates. TeamCount:{0}, SiteCount:{1}, ElapsedMilliseconds:{2}",
                teamIds.Count,
                siteCollections.Count,
                queryStopwatch.ElapsedMilliseconds);

            var groupedSites = siteCollections
                .Where(site => site != null
                    && !string.IsNullOrWhiteSpace(site.TeamId))
                .GroupBy(site => site.TeamId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(site => site.SiteCollectionType)
                        .ThenBy(site => site.url)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var teamNode in teamNodes)
            {
                if (string.IsNullOrWhiteSpace(teamNode.TeamsId)
                    || !groupedSites.TryGetValue(teamNode.TeamsId, out var sites)
                    || sites.Count == 0)
                {
                    continue;
                }

                var uniqueSiteSettingIds = new HashSet<Guid>();
                if (Guid.TryParse(teamNode.TeamsId, out var teamsId)
                    && uniqueSiteSettingIdsByTeamId.TryGetValue(teamsId, out var cachedUniqueSiteSettingIds))
                {
                    uniqueSiteSettingIds = cachedUniqueSiteSettingIds;
                }

                foreach (var site in sites)
                {
                    if (!useArchiverImportFile && Guid.TryParse(site.id, out var siteId) && uniqueSiteSettingIds.Contains(siteId))
                    {
                        logger.Debug("Skip site with a unique archiving setting. Team:{0}, Site:{1}", teamNode.FullPath, site.url);
                        continue;
                    }

                    var siteNode = new RMSPTreeNode
                    {
                        Id = site.id,
                        Name = site.url,
                        DisplayName = string.IsNullOrWhiteSpace(site.Name) ? site.url : site.Name,
                        FullPath = site.url,
                        FullUrl = site.url,
                        Level = (int)NodeLevel.SiteCollection,
                        NodeType = site.NodeType == RemoveNodeType.PrivateChannel
                            ? (int)GCommon.Contract.Tree.Object.NodeType.TeamPrivateChannel
                            : (int)GCommon.Contract.Tree.Object.NodeType.TeamChannel,
                        SPObjectId = site.id,
                        Type = ContentSourceType.Teams,
                        Parent = teamNode,
                        ParentId = teamNode.Id,
                        TeamsId = teamNode.TeamsId,
                        TeamName = teamNode.Name,
                        O365TenantId = site.TenantId,
                        UserArchiverImportFile = teamNode.UserArchiverImportFile,
                        SupportLockedSite = teamNode.SupportLockedSite,
                        SupportArchivedTeams = teamNode.SupportArchivedTeams
                    };
                    yield return siteNode;
                }
            }
        }

        private List<Guid> GetTeamsArchiverJobRuleIds(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level > (int)NodeLevel.Office365GroupEntire)
            {
                return GetAppliedRuleIds(selectedNode, ContentSourceType.Teams);
            }

            var groupNode = selectedNode.GetGroupNode();
            if (groupNode == null || !Guid.TryParse(groupNode.SPObjectId, out var groupId))
            {
                return new List<Guid>();
            }

            var settings = ArchiverSettingDao.LoadArchiverSettingsUnderGroup(groupId, ContentSourceType.Teams);
            IEnumerable<RMArchiverSetting> associatedSettings;
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                associatedSettings = settings.Where(setting =>
                    setting.SiteId == Guid.Empty
                    && ((setting.TeamsId == Guid.Empty && setting.SPObjectId == groupId)
                        || (setting.TeamsId != Guid.Empty && setting.SPObjectId == setting.TeamsId)));
            }
            else if (Guid.TryParse(selectedNode.GetTeamsNode()?.TeamsId, out var teamsId))
            {
                associatedSettings = settings.Where(setting =>
                    setting.TeamsId == teamsId
                    && setting.SiteId == Guid.Empty
                    && setting.SPObjectId == teamsId);
            }
            else
            {
                return new List<Guid>();
            }

            var settingIds = associatedSettings.Select(setting => setting.Id).Distinct().ToList();
            return EXOSettingRuleDao.GetAllTeamsNodeRuleMappings(settingIds)
                .Where(mapping => mapping.Type == (int)AvePoint.RA.DB.Dao.Impl.RuleType.Archiver)
                .Select(mapping => mapping.RuleId)
                .Distinct()
                .ToList();
        }

        private void CreateTeamsSubJobsByStream(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            List<JobType> shouldCheckConflictJobTypes,
            List<string> archiverImportSitesUrl,
            bool useArchiverImportFile,
            int estimatedSiteCount)
        {
            int totalCount = 0;
            int importFilterPassedCount = 0;
            int conflictFilterPassedCount = 0;
            int currentSubjobIndex = 0;
            int subJobIndexDigits = GetSubJobIndexDigits(estimatedSiteCount);
            var importUrlSet = useArchiverImportFile
                ? new HashSet<string>(archiverImportSitesUrl ?? new List<string>(), StringComparer.OrdinalIgnoreCase)
                : null;
            var pendingSubJobs = new List<RMSubJob>(SubJobBulkInsertBatchSize);
            var runningUrlsSnapshot = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int conflictFilterBatchProcessedCount = 0;
            DateTime lastRunningUrlsRefreshTimeUtc = DateTime.MinValue;
            var conflictFilterBatch = new List<RMSPTreeNode>(DisposalBrowsePageSize);

            foreach (var node in EnumerateTeamsArchiverRunnableNodeStream(selectedNode, importUrlSet))
            {
                totalCount++;
                importFilterPassedCount++;
                conflictFilterBatch.Add(node);
                if (conflictFilterBatch.Count < DisposalBrowsePageSize)
                {
                    continue;
                }
                else if (CheckWhetherJobShouldStop(jobId))
                {
                    return;
                }

                conflictFilterPassedCount += AppendTeamsSubJobsFromConflictFilteredBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    estimatedSiteCount,
                    subJobIndexDigits,
                    shouldCheckConflictJobTypes,
                    useArchiverImportFile,
                    ref currentSubjobIndex,
                    conflictFilterBatch,
                    pendingSubJobs,
                    runningUrlsSnapshot,
                    ref conflictFilterBatchProcessedCount,
                    ref lastRunningUrlsRefreshTimeUtc);
            }

            if (conflictFilterBatch.Count > 0)
            {
                conflictFilterPassedCount += AppendTeamsSubJobsFromConflictFilteredBatch(
                    jobId,
                    jobType,
                    selectedNode,
                    estimatedSiteCount,
                    subJobIndexDigits,
                    shouldCheckConflictJobTypes,
                    useArchiverImportFile,
                    ref currentSubjobIndex,
                    conflictFilterBatch,
                    pendingSubJobs,
                    runningUrlsSnapshot,
                    ref conflictFilterBatchProcessedCount,
                    ref lastRunningUrlsRefreshTimeUtc);
            }

            if (pendingSubJobs.Count > 0)
            {
                FlushTeamsArchiverSubJobs(jobType, pendingSubJobs);
            }

            if (CheckWhetherJobShouldStop(jobId))
            {
                return;
            }

            if (totalCount == 0 && useArchiverImportFile)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_Teams_ArchiverImportSkip");
                return;
            }

            if (totalCount == 0)
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoTeams");
                return;
            }

            if (importFilterPassedCount == 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_Teams_ArchiverImportSkip");
                return;
            }

            if (conflictFilterPassedCount == 0)
            {
                logger.Warn("not exsite can run job,will skip current job");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return;
            }

            SubJobDao.UpdateSubJobCount(jobId, conflictFilterPassedCount);
            RMJobService.SetSumSCCountOfJobExtension(conflictFilterPassedCount, jobId);
            logger.Info("all teams sub jobs were created correctlly, jobId is {0}, total count is {1}", jobId, conflictFilterPassedCount);
            var subJobWeight = 100d / conflictFilterPassedCount;
            if (!SubJobDao.UpdateSubJobWeightByParentId(jobId, subJobWeight))
            {
                logger.Warn("Failed to update teams sub job weights in batch, jobId:{0}, targetWeight:{1}", jobId, subJobWeight);
            }
        }

        private int AppendTeamsSubJobsFromConflictFilteredBatch(
            string jobId,
            JobType jobType,
            RMSPTreeNode selectedNode,
            int subJobCount,
            int subJobIndexDigits,
            List<JobType> shouldCheckConflictJobTypes,
            bool useArchiverImportFile,
            ref int currentSubjobIndex,
            List<RMSPTreeNode> conflictFilterBatch,
            List<RMSubJob> pendingSubJobs,
            Dictionary<string, List<string>> runningUrlsSnapshot,
            ref int conflictFilterBatchProcessedCount,
            ref DateTime lastRunningUrlsRefreshTimeUtc)
        {
            var searchFilter = RuleSPTreeUtil.BuildSearchFilter(selectedNode, conflictFilterBatch);
            EnsureRunningTeamsConflictSnapshot(
                selectedNode,
                shouldCheckConflictJobTypes,
                useArchiverImportFile,
                conflictFilterBatch,
                searchFilter,
                runningUrlsSnapshot,
                ref conflictFilterBatchProcessedCount,
                ref lastRunningUrlsRefreshTimeUtc);

            var filteredNodes = FilterTeamsDisposalConflictBatch(
                conflictFilterBatch,
                selectedNode,
                shouldCheckConflictJobTypes,
                runningUrlsSnapshot);
            conflictFilterBatch.Clear();
            int addedCount = filteredNodes.Count;
            if (addedCount == 0)
            {
                return 0;
            }

            int startSubJobIndex = currentSubjobIndex;
            currentSubjobIndex += addedCount;

            var builtSubJobs = new RMSubJob[addedCount];

            if (addedCount >= TeamsParallelBuildSubJobThreshold)
            {
                var maxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, TeamsParallelBuildMaxDegreeOfParallelism);
                Parallel.For(0, addedCount, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, i =>
                {
                    var filteredNode = filteredNodes[i];
                    var subJobNodes = new List<RMSPTreeNode>(1) { filteredNode };
                    builtSubJobs[i] = BuildSubJobForDisposal(
                        jobId,
                        startSubJobIndex + i,
                        jobType,
                        subJobCount,
                        subJobIndexDigits,
                        subJobNodes,
                        false,
                        GetTeamsSubJobScope(filteredNode),
                        filteredNode.O365TenantId);
                });
            }
            else
            {
                for (int i = 0; i < addedCount; i++)
                {
                    var filteredNode = filteredNodes[i];
                    var subJobNodes = new List<RMSPTreeNode>(1) { filteredNode };
                    builtSubJobs[i] = BuildSubJobForDisposal(
                        jobId,
                        startSubJobIndex + i,
                        jobType,
                        subJobCount,
                        subJobIndexDigits,
                        subJobNodes,
                        false,
                        GetTeamsSubJobScope(filteredNode),
                        filteredNode.O365TenantId);
                }
            }

            for (int i = 0; i < addedCount; i++)
            {
                var subJob = builtSubJobs[i];
                pendingSubJobs.Add(subJob);

                if (pendingSubJobs.Count >= SubJobBulkInsertBatchSize)
                {
                    FlushTeamsArchiverSubJobs(jobType, pendingSubJobs);
                }
            }

            return addedCount;
        }

        private static string GetTeamsSubJobScope(RMSPTreeNode node)
        {
            return node.Level == (int)NodeLevel.Folder
                ? WebUtil.MakeFullUrl(node.GetSiteCollectionNode()?.FullPath ?? node.FullPath, node.FullPath)
                : node.FullPath;
        }

        private void EnsureRunningTeamsConflictSnapshot(
            RMSPTreeNode selectedNode,
            List<JobType> shouldCheckConflictJobTypes,
            bool useArchiverImportFile,
            List<RMSPTreeNode> nodes,
            Dictionary<string, List<string>> searchFilter,
            Dictionary<string, List<string>> runningUrlsSnapshot,
            ref int conflictFilterBatchProcessedCount,
            ref DateTime lastRunningUrlsRefreshTimeUtc)
        {
            conflictFilterBatchProcessedCount++;

            var hasUnknownTeams = searchFilter.Keys.Any(teamName => !runningUrlsSnapshot.ContainsKey(teamName));
            var shouldRefreshByBatch = conflictFilterBatchProcessedCount % TeamsRunningUrlsRefreshBatchInterval == 0;
            var shouldRefreshByTime = lastRunningUrlsRefreshTimeUtc == DateTime.MinValue
                || DateTime.UtcNow - lastRunningUrlsRefreshTimeUtc >= TeamsRunningUrlsRefreshInterval;

            if (runningUrlsSnapshot.Count > 0 && !hasUnknownTeams && !shouldRefreshByBatch && !shouldRefreshByTime)
            {
                return;
            }

            var needLoadSiteUrl = RuleSPTreeUtil.CheckNeedLoadRuningSCUrlBySelectNode(selectedNode, useArchiverImportFile)
                || nodes.Any(node => node.Level >= (int)NodeLevel.SiteCollection);
            var runningUrls = RMJobService.GetRunningTeamsArchiverJobSiteUrl(
                shouldCheckConflictJobTypes,
                needLoadSiteUrl,
                searchFilter);

            runningUrlsSnapshot.Clear();
            foreach (var runningUrl in runningUrls)
            {
                runningUrlsSnapshot[runningUrl.Key] = runningUrl.Value ?? new List<string>();
            }

            lastRunningUrlsRefreshTimeUtc = DateTime.UtcNow;
        }


        private void FlushTeamsArchiverSubJobs(JobType jobType, List<RMSubJob> pendingSubJobs)
        {
            SubJobDao.BulkCreateJobs(pendingSubJobs, SubJobBulkInsertBatchSize);
            if (JobServiceUtility.NewJobDetailsJobs.Contains((int)jobType))
            {
                logger.Info($"Batch initialize progress for {pendingSubJobs.Count} sub jobs");
                _jobProgressDao.BatchAddJobProgressesBySubJobsAsync(pendingSubJobs).ExecuteAsyncTask();
            }
            pendingSubJobs.Clear();
        }

        private List<RMSPTreeNode> FilterTeamsDisposalConflictBatch(
            List<RMSPTreeNode> nodes,
            RMSPTreeNode selectedNode,
            List<JobType> shouldCheckConflictJobTypes,
            Dictionary<string, List<string>> runningUrls)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new List<RMSPTreeNode>();
            }

            if (shouldCheckConflictJobTypes == null || shouldCheckConflictJobTypes.Count == 0)
            {
                return nodes.ToList();
            }

            if ((selectedNode.Level != (int)NodeLevel.WebApplication
                && nodes.Count == 1
                && nodes[0].Level == selectedNode.Level
                && string.Equals(nodes[0].FullPath, selectedNode.FullPath, StringComparison.OrdinalIgnoreCase))
                || (selectedNode.UserArchiverImportFile && selectedNode.Level == (int)NodeLevel.WebApplication))
            {
                return nodes.ToList();
            }

            return RuleSPTreeUtil.FilterTeamsAvailableNodeByRunningUrl(nodes, runningUrls ?? new Dictionary<string, List<string>>(), selectedNode);
        }

        private const int DisposalBrowsePageSize = 5000;
        private const int DisposalConflictFilterBatchSize = 10000;
        private const int SubJobBulkInsertBatchSize = 2000;
        private const int TeamsSiteLookupBatchSize = 1000;
        private const int TeamsParallelBuildSubJobThreshold = 256;
        private const int TeamsParallelBuildMaxDegreeOfParallelism = 8;
        private const int TeamsRunningUrlsRefreshBatchInterval = 5;
        private static readonly TimeSpan TeamsRunningUrlsRefreshInterval = TimeSpan.FromSeconds(30);

        private bool CheckWhetherJobShouldStop(string jobId)
        {
            var mainJob = JMDao.Find(j => j.Id == jobId);

            if (mainJob.Status == (int)JobStatus.Stopped || mainJob.Status == (int)JobStatus.Stopping)
            {
                CleanupWaitingSubJobsWhenMainJobStopped(jobId);
            }
            return (mainJob.Status == (int)JobStatus.Stopped || mainJob.Status == (int)JobStatus.Stopping);
        }

        private void CleanupWaitingSubJobsWhenMainJobStopped(string jobId)
        {
            SubJobDao.DeleteSubJob(jobId, (int)JobStatus.Wait);
            SubJobDao.DeleteJobContext(jobId);
            logger.Warn("Main job was stopped during streaming sub job creation, stop processing and delete waiting sub jobs. jobId:{0}", jobId);
        }

        private IEnumerable<RMSPTreeNode> GetPagedTeamsSiteCollections(RMSPTreeNode node)
        {
            var states = new[] { SiteCollectionState.AccessAll, SiteCollectionState.AccessSome };
            var teamsTypes = new[]
            {
                AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Teams,
                AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Group
            };
            string lastId = null;
            int totalCount = 0;
            try
            {
                logger.Info("Begin browse teams site collections by container id: {0}, name: {1}.", node?.SPObjectId, node?.Name);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while browse teams site collections. Error: {0}", e);
                yield break;
            }

            while (true)
            {
                var previousLastId = lastId;
                var siteCollections = RMRemoteNodeDao.GetRemoteSiteCollectionsByParentIdByCursor(
                    node.SPObjectId,
                    states,
                    ref lastId,
                    DisposalBrowsePageSize,
                    includeOrphenNode: false,
                    types: teamsTypes);

                if (siteCollections.Count == 0)
                {
                    if (string.Equals(lastId, previousLastId, StringComparison.Ordinal))
                    {
                        break;
                    }

                    continue;
                }

                foreach (var site in siteCollections)
                {
                    var displayName = site.NodeType switch
                    {
                        RemoveNodeType.PrivateChannel => site.url,
                        RemoveNodeType.SkyDrivePro => site.Name,
                        RemoveNodeType.O365GroupSites => site.Name,
                        _ => site.url
                    };

                    var containerNodeType = site.SiteCollectionType == AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Teams
                        ? GCommon.Contract.Tree.Object.NodeType.O365TeamSites
                        : site.NodeType switch
                        {
                            RemoveNodeType.PrivateChannel => GCommon.Contract.Tree.Object.NodeType.PrivateChannelSitesGroup,
                            RemoveNodeType.O365GroupSites => GCommon.Contract.Tree.Object.NodeType.O365GroupSitesGroup,
                            RemoveNodeType.SkyDrivePro => GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup,
                            _ => GCommon.Contract.Tree.Object.NodeType.SharePointSitesGroup
                        };

                    var siteNode = new RMSPTreeNode
                    {
                        Id = site.TeamId,
                        SPObjectId = site.TeamId,
                        Name = site.Name,
                        DisplayName = displayName,
                        FullPath = site.Name,
                        FullUrl = site.url,
                        NodeType = (int)containerNodeType,
                        SPType = (int)SPType.BPOS,
                        FarmId = node.FarmId,
                        Level = (int)NodeLevel.Office365GroupEntire,
                        Type = node.Type,
                        Parent = node,
                        ParentId = node.Id,
                        O365TenantId = site.TenantId,
                        TeamsId = site.TeamId,
                        TeamName = site.Name,
                        BposInfo = new BposInfo
                        {
                            SiteUrl = string.Empty,
                            AppType = site.AppType,
                            ConnectionType = site.AuthType,
                            UserAccountInfo = new BposUserAccountInfo
                            {
                                Domain = site.domain,
                                Username = site.username,
                                Password = string.Empty,
                                AdminUrl = site.AdminUrl,
                                TenantId = site.TenantId
                            },
                            Mode = new DateTime(site.CreateTime).AddDays(1) <= DateTime.UtcNow ? BPOSMode.Office365 : BPOSMode.Undetermined
                        }
                    };

                    yield return siteNode;
                    totalCount++;
                }

                if (siteCollections.Count < DisposalBrowsePageSize || string.Equals(lastId, previousLastId, StringComparison.Ordinal))
                {
                    break;
                }
            }

            logger.Info("Finish browse teams site collections by container id: {0}. Yield count: {1}.", node?.SPObjectId, totalCount);
        }

        private IEnumerable<RMSPTreeNode> GetPagedDisposalSiteCollections(RMSPTreeNode node)
        {
            var states = new[] { SiteCollectionState.AccessAll, SiteCollectionState.AccessSome };
            string lastId = null;
            int totalCount = 0;
            try
            {
                logger.Info("Begin browse disposal site collections by container id: {0}, name: {1}.", node?.SPObjectId, node?.Name);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while browse disposal site collections. Error: {0}", e);
                yield break;
            }

            while (true)
            {
                var previousLastId = lastId;
                var siteCollections = RMRemoteNodeDao.GetRemoteSiteCollectionsByParentIdByCursor(
                    node.SPObjectId,
                    states,
                    ref lastId,
                    DisposalBrowsePageSize);
                if (siteCollections.IsNullOrEmpty())
                {
                    break;
                }

                if (string.Equals(previousLastId, lastId, StringComparison.Ordinal))
                {
                    logger.Warn("WebApplicationBrowse stopped because keyset cursor did not advance. ParentId:{0}, LastId:{1}, PageSize:{2}, ReturnedCount:{3}",
                        node?.SPObjectId,
                        lastId,
                        DisposalBrowsePageSize,
                        siteCollections.Count);
                    break;
                }

                totalCount += siteCollections.Count;
                // RMAosApiClient.SetPassWordBySiteCollectionuserName(siteCollections);
                // scUrlToAppProfileDict = RMAosApiClient.GetRemoteNodeUrlToAppProfileDict(siteCollections, TenantLocalValue.LogonGroupId);

                foreach (var siteCollection in siteCollections)
                {
                    var displayName = siteCollection.NodeType == RemoveNodeType.PrivateChannel
                        ? siteCollection.url
                        : siteCollection.Name;

                    var containerNodeType = siteCollection.NodeType switch
                    {
                        RemoveNodeType.PrivateChannel => GCommon.Contract.Tree.Object.NodeType.PrivateChannelSitesGroup,
                        RemoveNodeType.O365GroupSites => GCommon.Contract.Tree.Object.NodeType.O365GroupSitesGroup,
                        RemoveNodeType.SkyDrivePro => GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup,
                        _ => GCommon.Contract.Tree.Object.NodeType.SharePointSitesGroup
                    };

                    var nodeDto = new RMSPTreeNode
                    {
                        Id = siteCollection.id,
                        SPObjectId = siteCollection.id,
                        Name = siteCollection.url,
                        DisplayName = displayName,
                        FullPath = siteCollection.url,
                        NodeType = (int)containerNodeType,
                        SPType = (int)SPType.BPOS,
                        FarmId = node.FarmId,
                        Level = (int)NodeLevel.SiteCollection,
                        Type = node.Type,
                        Parent = node,
                        ParentId = node.Id,
                        O365TenantId = siteCollection.TenantId,
                        IsOrphenOneDrive = siteCollection.NodeType == RemoveNodeType.SkyDrivePro && string.IsNullOrEmpty(siteCollection.Name),
                        BposInfo = new BposInfo
                        {
                            SiteUrl = string.Empty,
                            AppType = siteCollection.AppType,
                            ConnectionType = siteCollection.AuthType,
                            UserAccountInfo = new BposUserAccountInfo
                            {
                                Domain = siteCollection.domain,
                                Username = siteCollection.username,
                                Password = string.Empty,
                                AdminUrl = siteCollection.AdminUrl,
                                TenantId = siteCollection.TenantId
                            },
                            Mode = new DateTime(siteCollection.CreateTime).AddDays(1) <= DateTime.UtcNow ? BPOSMode.Office365 : BPOSMode.Undetermined
                        }
                    };

                    // nodeDto.BposInfo.AddCertInfo(siteCollection, scUrlToAppProfileDict);
                    yield return nodeDto;
                }

                if (siteCollections.Count < DisposalBrowsePageSize)
                {
                    break;
                }
            }

            logger.Info("Success browse disposal site collections, count: {0}.", totalCount);
            logger.Info("End browse disposal site collections.");
        }

        private static int GetDisposalBrowseSourceType(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Type == ContentSourceType.OneDrive || selectedNode.NodeType == (int)GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup)
            {
                return (int)SourceFlag.OneDrive;
            }

            return (int)SourceFlag.SharePoint;
        }

        private static RMSPTreeNode ConvertSampleSiteToTreeNode(RMSPSampleTreeNode sampleSite, RMSPTreeNode parentNode)
        {
            return new RMSPTreeNode
            {
                Id = sampleSite.Id,
                SPObjectId = sampleSite.SPObjectId,
                Name = sampleSite.Name,
                DisplayName = sampleSite.DisplayName,
                Title = sampleSite.Title,
                FullPath = sampleSite.FullPath,
                OrphanNameSuffix = sampleSite.OrphanNameSuffix,
                Level = sampleSite.Level,
                NodeType = sampleSite.NodeType,
                SPType = sampleSite.SPType,
                SPVersion = sampleSite.SPVersion,
                TeamName = sampleSite.TeamName,
                TeamsId = sampleSite.TeamsId,
                O365TenantId = sampleSite.O365TenantId,
                Type = parentNode.Type,
                Parent = parentNode,
                ParentId = parentNode.Id
            };
        }

        public List<RMSPTreeNode> AssembleTeamsDisposalRunnableNode(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> teamsNodes = TeamsSettingTreeService.BrowseAsync(selectedNode, false).GetAwaiter().GetResult();
                if (teamsNodes.IsNullOrEmpty())
                {
                    return availableNode;
                }
                var settingIds = ArchiverSettingDao.LoadArchiverSettingsUnderGroup(new Guid(selectedNode.SPObjectId), ContentSourceType.Teams).Select(s => s.SPObjectId);
                foreach (RMSPTreeNode teamsNode in teamsNodes)
                {
                    //skip Teams has unique setting
                    if (!settingIds.Contains(new Guid(teamsNode.SPObjectId)) || selectedNode.UserArchiverImportFile)
                    {
                        teamsNode.UserArchiverImportFile = selectedNode.UserArchiverImportFile;
                        teamsNode.SupportLockedSite = selectedNode.SupportLockedSite;
                        teamsNode.SupportArchivedTeams = selectedNode.SupportArchivedTeams;
                        availableNode.Add(teamsNode);
                    }
                }
            }
            else
            {
                var teamsNode = selectedNode.GetTeamsNode();
                if (ValidateTeamsExist(teamsNode))
                {
                    selectedNode.O365TenantId = teamsNode.O365TenantId;
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Teams not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        private bool CheckOneDriveForSiteCollectionLevelRule(RMSPTreeNode siteCollectionNode)
        {
            try
            {
                if (siteCollectionNode.NodeType != (int)AvePoint.GCommon.Contract.Tree.Object.NodeType.SkyDriveProSitesGroup
                || siteCollectionNode.Level != (int)AvePoint.GCommon.Contract.Tree.Object.NodeLevel.SiteCollection)
                {
                    return true;
                }
                RemoteSiteCollection remoteNode = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteCollectionNode.Id);
                if (remoteNode != null && remoteNode.Name == null && remoteNode.NodeType == RemoveNodeType.SkyDrivePro)
                {
                    return true;
                }
                if (siteCollectionNode.Rules != null && siteCollectionNode.Rules.Any())
                {
                    return !siteCollectionNode.Rules.All(rule => rule.IntRuleLevel == (int)DocAveOnline.WebApi.Contracts.PolicyLevel.SiteCollection);
                }
                else if (siteCollectionNode.Parent.Rules != null && siteCollectionNode.Parent.Rules.Any())
                {
                    return !siteCollectionNode.Parent.Rules.All(rule => rule.IntRuleLevel == (int)DocAveOnline.WebApi.Contracts.PolicyLevel.SiteCollection);
                }
                else
                {
                    logger.Error(@$"Current node not contains rule, node : {siteCollectionNode.FullPath}");
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error(@$"have exception when CheckOneDriveForSiteCollectionLevelRule,ex:{e}");
                return false;
            }
        }

        private bool ValidateTeamsExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                site = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.Id).Item1;
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }

        private bool ValidateSiteExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                if (selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
                {
                    (site, _) = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.Id);
                }
                else
                {
                    site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                }
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }

        private string GetSPContainerId(RMSPTreeNode selectedNode)
        {
            return TreeNodeUtil.GetSPContainderId(selectedNode);
        }

        private int GetEstimatedSiteCount(RMSPTreeNode selectedNode, string containerId, bool useArchiverImportFile = false, List<string> importSiteUrls = default)
        {
            if (useArchiverImportFile)
            {
                return importSiteUrls?.Count ?? 0;
            }

            if (selectedNode == null)
            {
                return 0;
            }

            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return RMRemoteNodeDao.GetRemoteSiteCollectionCountByParentId(containerId);
            }

            return 1;
        }

        public async Task<string> RealRunOptimizationJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info("Begin RealRunOptimizationJobAsync.");
            RMDiscoverOptimizationJobInfo jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationJobInfo>(param);
            List<RMDiscoverOptimizationNode> result = new List<RMDiscoverOptimizationNode>();
            List<RMDiscoverOptimizationNode> nodeInfo = new List<RMDiscoverOptimizationNode>();
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationJobCancel, jobParaInfo.o365Info.UniqueId.ToString(), TimeSpan.FromMinutes(10)))
            {
                List<JobType> types = JobTypeConstants.ArchiveSiteConflictType;
                var settingIds = new List<Guid>() { jobParaInfo.settingInfo.SettingId };
                var mappings = new List<RMDiscoveryOffice365SiteOptimizationMappingInfo>();
                var statusUpdated = false;
                var skip = 0;
                var batchIndex = 0;
                while (true)
                {
                    var mappingBatch = await _siteOptimizationMappingTableDao.GetAllMappingInfoBySettingIdsAsync(jobParaInfo.o365Info.UniqueId, settingIds, skip, OptimizationQueryBatchSize);
                    logger.Info($"RealRunOptimizationJobAsync fetched mapping batch {batchIndex} with {mappingBatch?.Count ?? 0} records (skip {skip}).");
                    if (mappingBatch == null || mappingBatch.Count == 0)
                    {
                        logger.Info("RealRunOptimizationJobAsync no more mappings to process.");
                        break;
                    }

                    mappings.AddRange(mappingBatch);
                    var siteIds = mappingBatch.Select(item => item.NodeId).Distinct().ToList();
                    var siteInfos = await _nodeDao.GetSiteInfosBySiteIds(jobParaInfo.o365Info.UniqueId, siteIds);
                    var siteInfoMap = siteInfos.ToDictionary(item => item.Id);

                    foreach (var mappingInfo in mappingBatch)
                    {
                        if (!siteInfoMap.TryGetValue((int)mappingInfo.NodeId, out var siteInfo))
                        {
                            throw new InvalidOperationException($"Cannot find site information for node {mappingInfo.NodeId}.");
                        }

                        RMDiscoverOptimizationNode tempNode = new RMDiscoverOptimizationNode
                        {
                            SiteUrl = siteInfo.Url,
                            SiteInfoId = siteInfo.Id,
                            SiteId = siteInfo.SiteId,
                            sourceFlag = siteInfo.ContentSource,
                            SettingId = jobParaInfo.settingInfo.SettingId,
                            O365TenantId = jobParaInfo.o365Info.UniqueId
                        };

                        if (!statusUpdated)
                        {
                            var updateCount = await _optimizationSettingsInfoDao.UpdateStatusAsync(tempNode.SettingId, Contract.Discovery.Model.Configuration.Office365.DiscoverOptimizationScheduleStatus.Finish, jobParaInfo.o365Info.UniqueId);
                            statusUpdated = updateCount > 0;
                            if (!statusUpdated)
                            {
                                continue;
                            }
                        }

                        result.Add(tempNode);
                        var siteOptimizedInfo = await _optimizationDao.GetSiteOptimizedInfoAsync(jobParaInfo.o365Info.UniqueId, siteInfo.Id);
                        logger.Info($"RealRunOptimizationJobAsync the jobParaInfo.o365Info.UniqueId:{jobParaInfo?.o365Info?.UniqueId},siteInfo.Id:{siteInfo?.Id}.siteOptimizedInfo is null?{siteOptimizedInfo == null}");
                        logger.Info($"RealRunOptimizationJobAsync the jobParaInfo.settingInfo.SettingId:{jobParaInfo?.settingInfo?.SettingId}");
                        if (siteOptimizedInfo != null)
                        {
                            siteOptimizedInfo.LastOptimizedTime = siteOptimizedInfo.NextOptimizationTime;
                            siteOptimizedInfo.NextOptimizableFileTotalSize = 0;
                            siteOptimizedInfo.NextOptimizableVersionTotalSize = 0;
                            await _optimizationDao.AddOrUpdateSiteOptimizedInfoAsync(jobParaInfo.o365Info.UniqueId, siteOptimizedInfo);
                        }
                        else
                        {
                            RMDiscoveryOffice365SiteOptimizedInfo siteInsertOptimizedInfo = new RMDiscoveryOffice365SiteOptimizedInfo()
                            {  //兜底逻辑，防止caculate job出现某些意外问题导致没有插入记录
                                SettingId = jobParaInfo.settingInfo.SettingId,
                                NextOptimizationTime = 0,
                                NextOptimizableFileTotalSize = 0,
                                NextOptimizableVersionTotalSize = 0,
                                Archived = 0,
                                Deleted = 0,
                                LastOptimizedTime = 0L
                            };
                            await _optimizationDao.AddOrUpdateSiteOptimizedInfoAsync(jobParaInfo.o365Info.UniqueId, siteInsertOptimizedInfo);
                        }
                    }

                    skip += mappingBatch.Count;
                    batchIndex++;
                    if (mappingBatch.Count < OptimizationQueryBatchSize)
                    {
                        logger.Info($"RealRunOptimizationJobAsync reached final mapping batch {batchIndex - 1} with {mappingBatch.Count} records.");
                        break;
                    }
                }
                logger.Info($"RealRunOptimizationJobAsync processed total {mappings.Count} mapping records.");
                if (mappings == null || mappings.Count == 0)
                {
                    var jobid = RMJobService.CreateJobWithScopeId(JobType.DiscoverOptimization, "RM_TS_RunSchedule", "", null, null);
                    RMJobService.UpdateJobStatus(jobid, JobStatus.Skipped, "RM_Job_ArchiverImportSkip");
                    logger.Info($"the import file not contain any site url,will skip,id:{jobid}");
                    await _optimizationSettingsInfoDao.UpdateStatusAsync(jobParaInfo.settingInfo.SettingId, Contract.Discovery.Model.Configuration.Office365.DiscoverOptimizationScheduleStatus.Finish, jobParaInfo.o365Info.UniqueId);
                    return jobid;
                }
                //foreach settings
                //get nodes by settings
                nodeInfo = result;
                if (nodeInfo.IsNullOrEmpty() || nodeInfo.Count == 0)
                {
                    logger.Warn("No available sc to run.");
                    return string.Empty;
                }
                var runningUrl = RMJobService.GetRunningArchiverJobSiteUrl(types, nodeInfo.Select(node => node.SiteUrl), true);
                string jobId = string.Empty;
                nodeInfo = FilterDiscoverOptimizationUrl(nodeInfo, runningUrl);
                string nodeUrl = "DiscoverOptimizationScope";
                if (nodeInfo.Count == 0)
                {
                    jobId = RMJobService.CreateJobWithScopeId(JobType.DiscoverOptimization, "RM_TS_RunSchedule", nodeUrl);
                    logger.Warn("all site has running job,skip.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                jobId = RMJobService.CreateJobWithScopeId(JobType.DiscoverOptimization, "RM_TS_RunSchedule", nodeUrl, null, null, GenerateArchiveJobMonitorExtensionForDSO(nodeInfo.Select(n => n.SiteUrl).ToList(), TreeMode.SO));
                mappings.ForEach(map => map.SubJobId = jobId);
                var totalMappings = mappings.Count;
                var inserted = 0;
                var batchIndex1 = 0;
                logger.Info("start insert _siteOptimizationMappingTableDao");
                while (inserted < totalMappings)
                {
                    var batch = mappings.Skip(inserted).Take(OptimizationInsertBatchSize).ToList();
                    logger.Info($"RealRunOptimizationJobAsync writing mapping batch {batchIndex1} with {batch.Count} records (tenant {jobParaInfo.o365Info.UniqueId}).");
                    await _siteOptimizationMappingTableDao.AddOrUpdateAsync(batch, jobParaInfo.o365Info.UniqueId);
                    inserted += batch.Count;
                    batchIndex1++;
                }
                logger.Info($"RealRunOptimizationJobAsync finished writing {totalMappings} mapping records (tenant {jobParaInfo.o365Info.UniqueId}).");
                await AddSettingHistoryToAzureTableAsync(nodeInfo, jobParaInfo.o365Info, jobId);
                logger.Info("finish add setting history to azure table");
                if (nodeInfo.IsNullOrEmpty())
                {
                    logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                    return jobId;
                }
                List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
                var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
                var (has, mainJobInfo) = await _jobDao.TryGetProcessingMainJobAsync();
                if (mIndexJobs.Count > 0 || has)
                {
                    //has move index job or discovery job, need skip.
                    logger.Warn("Current has move index job or discovery job or retention job running.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                int subJobCount = nodeInfo.Count;
                SubJobDao.UpdateSubJobCount(jobId, subJobCount);
                RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);

                //RMRunningJobRuleMappingDao.AddJobRuleMappings(TenantLocalValue.LogonGroupId, jobId, GetAppliedRuleIds(selectedNode));
                int currentSubjobIndex = 0;
                foreach (var node in nodeInfo)
                {
                    AddOptimizationSubjob(jobId, currentSubjobIndex, JobType.DiscoverOptimization, subJobCount, node, false, node.SiteUrl, node.O365TenantId.ToString());
                    currentSubjobIndex++;
                }
                logger.Info("End RealRunOptimizationJobAsync.");
                return jobId;
            }
        }

        public async Task<string> RealRunDiscoveryPlanProOptimizationJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info("Begin RealRunDiscoveryPlanProOptimizationJobAsync.");
            string mainJobRunBy = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            string nodeUrl = "DiscoverOptimizationScope";
            string jobId = RMJobService.CreateJobWithScopeId(JobType.DiscoveryPlanProOptimization, mainJobRunBy, nodeUrl);

            var profileIds = string.IsNullOrWhiteSpace(param)
                ? new List<string>()
                : SerializerHelper.DeserializeByDataContractSerializer<List<string>>(param) ?? new List<string>();
            var validProfileIds = profileIds
                .Where(id => !string.IsNullOrWhiteSpace(id) && int.TryParse(id, out var parsed) && parsed > 0)
                .Select(int.Parse)
                .Distinct()
                .ToList();

            if (!validProfileIds.Any())
            {
                SubJobDao.UpdateSubJobCount(jobId, 0);
                RMJobService.SetSumSCCountOfJobExtension(0, jobId);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                logger.Warn("RealRunDiscoveryPlanProOptimizationJobAsync skipped because no valid profile id exists.");
                return jobId;
            }

            await RMDiscoveryDBManager.InitPlanTablesAsync();
            var candidateNodes = new List<RMDiscoverOptimizationNode>();
            foreach (var profileId in validProfileIds)
            {
                var profile = await _planProfileDao.GetByIdAsync(profileId);
                if (profile == null)
                {
                    logger.Warn($"RealRunDiscoveryPlanProOptimizationJobAsync cannot find profile: {profileId}.");
                    continue;
                }

                var mappedNodeIds = await _planSiteMappingDao.GetNodeIdsByPlanProfileIdAsync(profileId);
                if (mappedNodeIds == null || mappedNodeIds.Count == 0)
                {
                    logger.Warn($"RealRunDiscoveryPlanProOptimizationJobAsync profile has no mapped sites: {profileId}.");
                    continue;
                }

                var planRuleDefinition = BuildPlanProRuleDefinition(profileId, profile.Name, profile.Rules);
                foreach (var mappedNodeId in mappedNodeIds)
                {
                    var remoteNode = RMRemoteNodeDao.GetRemoteSiteCollectionById(mappedNodeId);

                    if (remoteNode == null)
                    {
                        logger.Warn($"Skip mapped site because remote node not found. mappedNodeId:{mappedNodeId}, profile:{profileId}.");
                        continue;
                    }
                    if (!Guid.TryParse(remoteNode.ObjectId, out var siteId))
                    {
                        logger.Warn($"Skip mapped site because site id is invalid. objectId:{remoteNode.ObjectId}, profile:{profileId}.");
                        continue;
                    }
                    if (!Guid.TryParse(remoteNode.TenantId, out var tenantId))
                    {
                        logger.Warn($"Skip mapped site because tenant id is invalid. tenantId:{remoteNode.TenantId}, profile:{profileId}.");
                        continue;
                    }

                    var siteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(tenantId, siteId);
                    if (siteInfo == null)
                    {
                        logger.Warn($"Skip mapped site because discovery site info not found. tenantId:{tenantId}, siteId:{siteId}, profile:{profileId}.");
                        continue;
                    }
                    var siteInfoId = siteInfo.Id;

                    candidateNodes.Add(new RMDiscoverOptimizationNode
                    {
                        SiteUrl = remoteNode.url,
                        SiteInfoId = siteInfoId,
                        SiteId = siteId,
                        sourceFlag = (SourceFlag)remoteNode.NodeType,
                        SettingId = Guid.NewGuid(),
                        O365TenantId = tenantId,
                        PlanProOptimizationSetting = BuildPlanProOptimizationSetting(profile, tenantId.ToString(), siteInfoId),
                        PlanProRuleDefinitions = planRuleDefinition == null ? new List<RMDiscoveryRuleDefinition>() : new List<RMDiscoveryRuleDefinition> { planRuleDefinition },
                        UseDalDataOptimizationService = true,
                    });
                }
            }

            var nodeInfo = candidateNodes
                .GroupBy(node => node.SiteUrl, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var first = group.First();
                    first.PlanProRuleDefinitions = group
                        .SelectMany(item => item.PlanProRuleDefinitions ?? new List<RMDiscoveryRuleDefinition>())
                        .Where(item => item != null)
                        .GroupBy(item => item.UniqueId)
                        .Select(item => item.First())
                        .ToList();
                    return first;
                })
                .ToList();

            if (!nodeInfo.Any())
            {
                SubJobDao.UpdateSubJobCount(jobId, 0);
                RMJobService.SetSumSCCountOfJobExtension(0, jobId);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                logger.Warn("RealRunDiscoveryPlanProOptimizationJobAsync skipped because no executable site exists.");
                return jobId;
            }

            var runningUrl = RMJobService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, nodeInfo.Select(node => node.SiteUrl), true);
            nodeInfo = FilterDiscoverOptimizationUrl(nodeInfo, runningUrl);
            if (nodeInfo.Count == 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                logger.Warn("RealRunDiscoveryPlanProOptimizationJobAsync all sites are in running-job conflict.");
                return jobId;
            }

            var mIndexJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            var (has, mainJobInfo) = await _jobDao.TryGetProcessingMainJobAsync();
            if (mIndexJobs.Count > 0 || has)
            {
                logger.Warn("Current has move index job or discovery job or retention job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            int subJobCount = nodeInfo.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);

            int currentSubjobIndex = 0;
            foreach (var node in nodeInfo)
            {
                AddOptimizationSubjob(jobId, currentSubjobIndex, JobType.DiscoveryPlanProOptimization, subJobCount, node, false, node.SiteUrl, node.O365TenantId.ToString());
                currentSubjobIndex++;
            }

            logger.Info($"End RealRunDiscoveryPlanProOptimizationJobAsync. subjob count:{subJobCount}.");
            return jobId;
        }

        private RMDiscoveryRuleDefinition BuildPlanProRuleDefinition(int profileId, string profileName, string rulesJson)
        {
            try
            {
                var criteriaInfoes = string.IsNullOrWhiteSpace(rulesJson)
                    ? new List<RMDiscoveryRuleCriteriaInfo>()
                    : JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(rulesJson) ?? new List<RMDiscoveryRuleCriteriaInfo>();

                return new RMDiscoveryRuleDefinition
                {
                    Id = profileId,
                    Name = string.IsNullOrWhiteSpace(profileName) ? $"PlanProfile_{profileId}" : profileName,
                    UniqueId = Guid.NewGuid(),
                    Description = string.Empty,
                    IsEnable = true,
                    Order = 1,
                    Kind = RMDiscoveryRuleDefinitionKind.ROT,
                    AnalyseMethod = RMDiscoveryRuleAnalyseMethod.Document,
                    CriteriaInfoes = criteriaInfoes,
                };
            }
            catch (Exception ex)
            {
                logger.Warn($"BuildPlanProRuleDefinition failed for profile:{profileId}. error:{ex}");
                return null;
            }
        }

        private Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting BuildPlanProOptimizationSetting(DB.Model.Discovery.Plan.RMDiscoveryPlanProfile profile, string tenantId, int siteInfoId)
        {
            return new Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting
            {
                ArchiveDataType = (int)Contract.Discovery.Model.Configuration.Office365.ArchiverDataType.Special,
                MS365DataType = (int)Contract.Discovery.Model.Configuration.Office365.MS365DataType.Default,
                DataType = (int)ArchiverDataType.All,
                O365TenantId = tenantId,
                SelectedStorage = new StorageDeviceUIDto
                {
                    Id = profile.StorageLocationId,
                    Name = string.Empty,
                },
                NodeQueryParameter = new RMDiscoveryOffice365NodeQueryParameter
                {
                    ViewMode = RMDiscoveryNodeViewMode.Site,
                    SiteIds = new List<int> { siteInfoId },
                },
                SizeRangeQueryParameter = new RMDiscoveryOffice365SizeRangeQueryParameter
                {
                    QueryMode = RMDiscoverySizeRangeQueryMode.None,
                    SizeRange = 0,
                },
                WithoutDateQueryParameter = new RMDiscoveryOffice365WithoutDateQueryParameter
                {
                    From = -1,
                    To = 999,
                },
                FileExtensionQueryParameter = new RMDiscoveryOffice365FileExtensionQueryParameter
                {
                    FileExtensions = new List<int>(),
                },
                ROTRuleQueryParameter = new Contract.Discovery.Model.Configuration.Office365.ROTRuleQueryParameter
                {
                    Enable = true,
                    RuleCategories = new List<RMDiscoveryROTRuleCategoryQueryParameter>(),
                },
                InactiveRuleQueryParameter = new Contract.Discovery.Model.Configuration.Office365.InactiveRuleQueryParameter
                {
                    Enable = false,
                    RuleIds = new List<int>(),
                },
                ProcessActionParameter = BuildPlanProProcessActionParameter(profile.Action, profile.ActionOptions, profile.PreviousVersion),
                MoveToAnotherTierType = 0,
            };
        }

        private Contract.Discovery.Model.Configuration.Office365.ProcessActionParameter BuildPlanProProcessActionParameter(RMDiscoveryPlanAction action, RMDiscoveryPlanActionOptions options, int previousVersion)
        {
            var processAction = new Contract.Discovery.Model.Configuration.Office365.ProcessActionParameter
            {
                FileAction = action == RMDiscoveryPlanAction.DestroyFile ? Contract.Discovery.Model.Configuration.Office365.FileAction.Remove : Contract.Discovery.Model.Configuration.Office365.FileAction.ArchiveAndRemove,
                VersionAction = Contract.Discovery.Model.Configuration.Office365.VersionAction.None,
                ArchivedLatestVersion = 0,
                EnableArchivedLatestVersion = false,
                ArchivedOnlyLatestVersion = 0,
                EnableArchivedOnlyLatestVersion = false,
                IsEnableLeaveStub = (options & RMDiscoveryPlanActionOptions.LeaveStub) == RMDiscoveryPlanActionOptions.LeaveStub,
                DeleteRecords = (options & RMDiscoveryPlanActionOptions.IncludeDeclaredRecords) != RMDiscoveryPlanActionOptions.IncludeDeclaredRecords,
                DeleteRecordToRecycleBin = (options & RMDiscoveryPlanActionOptions.DeleteToRecycleBin) == RMDiscoveryPlanActionOptions.DeleteToRecycleBin,
                DeleteVersionToRecycleBin = (options & RMDiscoveryPlanActionOptions.DeleteToRecycleBin) == RMDiscoveryPlanActionOptions.DeleteToRecycleBin,
            };

            if ((options & RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest) == RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest
                || (options & RMDiscoveryPlanActionOptions.ArchiveCurrentAndPrevious) == RMDiscoveryPlanActionOptions.ArchiveCurrentAndPrevious)
            {
                processAction.VersionAction = Contract.Discovery.Model.Configuration.Office365.VersionAction.ArchiveAndRemoveVerison;
                processAction.EnableArchivedLatestVersion = previousVersion > 0;
                processAction.ArchivedLatestVersion = previousVersion;
            }
            else if ((options & RMDiscoveryPlanActionOptions.KeepCurrentAndPrevious) == RMDiscoveryPlanActionOptions.KeepCurrentAndPrevious)
            {
                processAction.VersionAction = Contract.Discovery.Model.Configuration.Office365.VersionAction.RemoveVersion;
                processAction.EnableArchivedOnlyLatestVersion = previousVersion > 0;
                processAction.ArchivedOnlyLatestVersion = previousVersion;
            }

            return processAction;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ImportExternalArchivedData, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunOptimizationJobFromManifestAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info("Begin RealRunOptimizationJobFromManifestAsync.");
            HSMArchiverDto jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<HSMArchiverDto>(param);
            List<RMHSMBackupNode> nodeInfo = new List<RMHSMBackupNode>();

            List<JobType> types = JobTypeConstants.ArchiveSiteConflictType;
            logger.Info($"RealRunOptimizationJobFromManifestAsync processed total site urls {jobParaInfo.SiteUrls?.Count}.");
            if (jobParaInfo.SiteUrls.IsNullOrEmpty() || jobParaInfo.SiteUrls.Count == 0)
            {
                logger.Warn("No available sc to run.");
                return string.Empty;
            }
            var runningUrl = RMJobService.GetRunningArchiverJobSiteUrl(types, jobParaInfo.SiteUrls);
            string jobId = string.Empty;
            var canRunJobUrls = FilterHSMOptimizationUrl(jobParaInfo.SiteUrls, runningUrl);
            string nodeUrl = jobParaInfo.Location ?? "HSMArchiverScope";
            if (canRunJobUrls.Count == 0)
            {
                jobId = RMJobService.CreateJobWithScopeIdAndJobId(jobParaInfo.MainJobId, JobType.ArchiverByHSMXml, "RM_TS_RunSchedule", nodeUrl);
                logger.Warn("all site has running job,skip run HSM archiver.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            jobId = RMJobService.CreateJobWithScopeIdAndJobId(jobParaInfo.MainJobId, JobType.ArchiverByHSMXml, "RM_TS_RunSchedule", nodeUrl, jobConflictExtension: GenerateArchiveJobMonitorExtensionForDSO(canRunJobUrls, TreeMode.SO));
            List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
            if (mIndexJobs.Count > 0)
            {
                //has move index job or discovery job, need skip.
                logger.Warn("Current has move index job or discovery job or retention job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            int subJobCount = canRunJobUrls.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);
            int currentSubjobIndex = 0;
            nodeInfo = GenerateHSMBackupNode(canRunJobUrls, jobParaInfo);
            if (nodeInfo == null || nodeInfo.Count == 0)
            {
                logger.Warn("Current not exist nodeinfo to run for HSMArchiver.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_RDM_SCNotFound");
                return jobId;
            }
            foreach (var node in nodeInfo)
            {
                AddHSMSubjob(jobId, currentSubjobIndex, JobType.ArchiverByHSMXml, subJobCount, node, false, node.SiteUrl, node.O365TenantId.ToString());
                currentSubjobIndex++;
            }
            logger.Info("End RealRunOptimizationJobFromManifestAsync.");
            return jobId;
        }
        private List<RMHSMBackupNode> GenerateHSMBackupNode(List<string> siteUrls, HSMArchiverDto hSMArchiverDto)
        {
            List<RMHSMBackupNode> result = new List<RMHSMBackupNode>();
            var remoteNodes = RMRemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(siteUrls);
            if (!siteUrls.IsNullOrEmpty())
            {
                var remoteUrlSet = new HashSet<string>((remoteNodes ?? new List<RemoteSiteCollection>()).Select(node => node.url), StringComparer.OrdinalIgnoreCase);
                var missingUrls = siteUrls.Where(url => !remoteUrlSet.Contains(url)).ToList();
                if (missingUrls.Count > 0)
                {
                    logger.Warn($"GenerateHSMBackupNode missing remote node definitions for: {string.Join(", ", missingUrls)}");
                }
            }
            foreach (var node in remoteNodes)
            {
                RMHSMBackupNode tempNode = new RMHSMBackupNode
                {
                    SiteUrl = node.url,
                    SiteId = new Guid(node.ObjectId),
                    sourceFlag = node.NodeType,
                    O365TenantId = new Guid(node.TenantId),
                    SelectedStorage = hSMArchiverDto.SelectedStorage,
                    SourceDataStorageId = hSMArchiverDto.SourceDataStorageId,
                    StubTemplateId = hSMArchiverDto.StubTemplateId,
                    DataContentStorageId = hSMArchiverDto.DataContentStorageId,
                    SkipCheckFileExtension = hSMArchiverDto.SkipCheckFileExtension,
                    TraceId = hSMArchiverDto.TraceId
                };
                var urlList = result.Select(a => a.SiteUrl).ToList();
                if (urlList != null && urlList.Contains(tempNode.SiteUrl))
                {
                    logger.Warn($"this node has been added in the resilt,skip add it,url:{tempNode.SiteUrl}");
                }
                else
                {
                    logger.Info($"this node will run the hsm job,url:{tempNode.SiteUrl}");
                    result.Add(tempNode);
                }
            }
            return result;
        }
        public async Task<string> RealRunAOSPOptimizationJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info("Begin RealRunAOSPOptimizationJobAsync.");
            RMDiscoverAOSPOptimizationJobInfo jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverAOSPOptimizationJobInfo>(param);
            List<RMDiscoverOptimizationNode> result = new List<RMDiscoverOptimizationNode>();
            List<RMDiscoverOptimizationNode> nodeInfo = new List<RMDiscoverOptimizationNode>();
            RMDiscoveryAOSPOptimizationSetting setting = new RMDiscoveryAOSPOptimizationSetting();
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationJobCancel, jobParaInfo.o365Info.UniqueId.ToString(), TimeSpan.FromMinutes(10)))
            {
                List<JobType> types = JobTypeConstants.ArchiveSiteConflictType;
                List<RMDiscoveryAOSPSiteOptimizationMappingInfo> mappings = new List<RMDiscoveryAOSPSiteOptimizationMappingInfo>();
                bool isArchiverProFile = false;
                if (!string.IsNullOrEmpty(jobParaInfo?.settingInfo?.Setting))
                {
                    try
                    {
                        logger.Info("try check current job is archiver profile");
                        setting = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryAOSPOptimizationSetting>(RMDiscoveryAOSPOptimizationSetting.XMLCompatibleConvert(jobParaInfo.settingInfo.Setting));
                        isArchiverProFile = setting.UseArchiverProfile;
                        logger.Info($"success check current job, is archiver profile:{isArchiverProFile}, SupportLockedSite:{setting.SupportLockedSite}");
                    }
                    catch (Exception e)
                    {
                        logger.Info($"failed check current job, is archiver profile:{isArchiverProFile},error:{e}");
                    }
                }
                if (!isArchiverProFile)
                {
                    mappings = await _siteAOSPOptimizationMappingTableDao.GetAllMappingInfoBySettingIdsAsync(jobParaInfo.o365Info.UniqueId, new List<Guid>() { jobParaInfo.settingInfo.SettingId });
                    var siteInfos = await _nodeAOSPDao.GetSiteInfosBySiteIds(jobParaInfo.o365Info.UniqueId, mappings.Select(item => item.NodeId).ToList());
                    foreach (var mappingInfo in mappings)
                    {
                        RMDiscoverOptimizationNode tempNode = new RMDiscoverOptimizationNode();
                        var siteInfo = siteInfos.Where(item => item.Id == mappingInfo.NodeId).First();

                        tempNode.SiteUrl = siteInfo.Url;
                        tempNode.SiteInfoId = siteInfo.Id;
                        tempNode.SiteId = siteInfo.SiteId;
                        tempNode.sourceFlag = siteInfo.ContentSource;

                        tempNode.SettingId = jobParaInfo.settingInfo.SettingId;
                        tempNode.O365TenantId = jobParaInfo.o365Info.UniqueId;
                        tempNode.SupportLockedSite = setting.SupportLockedSite;
                        logger.Info($"AOSP optimization node created. SettingId:{tempNode.SettingId}, SiteId:{tempNode.SiteId}, SupportLockedSite:{tempNode.SupportLockedSite}");
                        var count = await _optimizationAOSPSettingsInfoDao.UpdateStatusAsync(tempNode.SettingId, Contract.Discovery.Model.Configuration.Office365.DiscoverOptimizationScheduleStatus.Finish, jobParaInfo.o365Info.UniqueId);
                        if (count > 0)
                        {
                            result.Add(tempNode);
                            var siteOptimizedInfo = await _optimizationAOSPDao.GetSiteOptimizedInfoAsync(jobParaInfo.o365Info.UniqueId, siteInfo.Id);
                            if (siteOptimizedInfo != null)
                            {
                                siteOptimizedInfo.LastOptimizedTime = siteOptimizedInfo.NextOptimizationTime;
                                await _optimizationAOSPDao.AddOrUpdateSiteOptimizedInfoAsync(jobParaInfo.o365Info.UniqueId, siteOptimizedInfo);
                            }
                        }
                    }
                }
                else
                {
                    jobParaInfo.settingInfo.JobId = setting.JobId;
                    var appProfile = await RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, setting.O365TenantId);
                    var siteManager = new RMGraphGroupManager(appProfile);
                    foreach (var siteId in setting.NodeIds)
                    {
                        try
                        {
                            var siteInfo = setting.SiteInfos?.FirstOrDefault(info => string.Equals(info.SiteId, siteId, StringComparison.OrdinalIgnoreCase));
                            logger.Info($"This site info is from setting. SiteId:{siteId}, SiteInfoId:{siteInfo?.SiteId}, SiteInfoUrl:{siteInfo?.SiteUrl}");
                            var siteUrl = siteInfo?.SiteUrl;
                            if (string.IsNullOrWhiteSpace(siteUrl))
                            {
                                var siteResult = await siteManager.GetSiteById(siteId);
                                siteUrl = siteResult?.Url;
                            }
                            logger.Info($"this site siteUrl,siteid:{siteId},siteurl:{siteUrl}");
                            if (string.IsNullOrWhiteSpace(siteUrl))
                            {
                                throw new InvalidOperationException($"Cannot find site URL for site ID: {siteId}");
                            }

                            logger.Info($"this site will run archiver profile job,siteid:{siteId}");
                            RMDiscoverOptimizationNode tempNode = new RMDiscoverOptimizationNode();
                            tempNode.SiteUrl = siteUrl;
                            tempNode.SiteId = new Guid(siteId);
                            tempNode.SettingId = jobParaInfo.settingInfo.SettingId;
                            tempNode.O365TenantId = jobParaInfo.o365Info.UniqueId;
                            tempNode.ArchiverProfileSetting = setting;
                            tempNode.SupportLockedSite = setting.SupportLockedSite;
                            logger.Info($"AOSP archiver optimization node created. SettingId:{tempNode.SettingId}, SiteId:{tempNode.SiteId}, SupportLockedSite:{tempNode.SupportLockedSite}");
                            result.Add(tempNode);
                        }
                        catch (Exception ex)
                        {
                            logger.Info($"RealRunAOSPOptimizationJobAsync.failed GetSiteById.SiteID:{siteId},error:{ex}");
                        }
                    }
                }
                //foreach settings
                //get nodes by settings
                nodeInfo = result;
                if (nodeInfo.IsNullOrEmpty() || nodeInfo.Count == 0)
                {
                    logger.Warn("No available sc to run.");
                    return string.Empty;
                }
                var runningUrl = RMJobService.GetRunningArchiverJobSiteUrl(types, nodeInfo.Select(node => node.SiteUrl), true);
                string jobId = jobParaInfo.settingInfo.JobId;
                nodeInfo = FilterDiscoverOptimizationUrl(nodeInfo, runningUrl);
                string nodeUrl = "DiscoverOptimizationScope";
                if (nodeInfo.Count == 0)
                {
                    RMJobService.CreateJobWithScopeId(jobId, JobType.DiscoveryAOSPOptimization, "RM_TS_RunSchedule", nodeUrl);
                    logger.Warn("all site has running job,skip.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                RMJobService.CreateJobWithScopeId(jobId, JobType.DiscoveryAOSPOptimization, "RM_TS_RunSchedule", nodeUrl, null, null, GenerateArchiveJobMonitorExtensionForDSO(nodeInfo.Select(n => n.SiteUrl).ToList(), TreeMode.SO));
                if (!isArchiverProFile)
                {
                    mappings.ForEach(map => map.SubJobId = jobId);
                    await _siteAOSPOptimizationMappingTableDao.AddOrUpdateAsync(mappings, jobParaInfo.o365Info.UniqueId);
                    await AddSettingHistoryToAzureTableAsync(nodeInfo, jobParaInfo.o365Info, jobId);
                    logger.Info("finish add setting history to azure table");
                }
                if (nodeInfo.IsNullOrEmpty())
                {
                    logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                    return jobId;
                }
                List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
                var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
                if (!isArchiverProFile)
                {
                    var (has, mainJobInfo) = await _jobAOSPDao.TryGetProcessingMainJobAsync(jobParaInfo.o365Info.UniqueId.ToString());
                    if (mIndexJobs.Count > 0 || has)
                    {
                        //has move index job or discovery job, need skip.
                        logger.Warn("Current has move index job or discovery job or retention job running.");
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
                else
                {
                    if (mIndexJobs.Count > 0)
                    {
                        //has move index job or discovery job, need skip.
                        logger.Warn("Current has move index job or retention job running.");
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
                int subJobCount = nodeInfo.Count;
                SubJobDao.UpdateSubJobCount(jobId, subJobCount);
                RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);

                //RMRunningJobRuleMappingDao.AddJobRuleMappings(TenantLocalValue.LogonGroupId, jobId, GetAppliedRuleIds(selectedNode));
                int currentSubjobIndex = 0;
                foreach (var node in nodeInfo)
                {
                    AddAOSPOptimizationSubjob(jobId, currentSubjobIndex, JobType.DiscoveryAOSPOptimization, subJobCount, node, false, node.SiteUrl, node.O365TenantId.ToString(), node.SiteId.ToString());
                    currentSubjobIndex++;
                }
                logger.Info("End RealRunOptimizationJobAsync.");
                return jobId;
            }
        }
        private List<RMDiscoverOptimizationNode> FilterDiscoverOptimizationUrl(List<RMDiscoverOptimizationNode> needRunNodes, List<string> runningUrls)
        {
            List<RMDiscoverOptimizationNode> result = needRunNodes.ToList();
            foreach (var runningUrl in runningUrls)
            {
                foreach (var node in needRunNodes.OrderByDescending(node => node.SiteUrl.Length))
                {
                    if (RuleSPTreeUtil.IsPrefixWithSlash(runningUrl, node.SiteUrl) || RuleSPTreeUtil.IsPrefixWithSlash(node.SiteUrl, runningUrl))
                    {
                        logger.Warn($"current scope :{node.SiteUrl} has running job,so skip run subjob");
                        result.Remove(node);
                    }
                }
            }
            return result;
        }
        private List<string> FilterHSMOptimizationUrl(List<string> siteUrls, List<string> runningUrls)
        {
            List<string> result = siteUrls.ToList();
            foreach (string runningUrl in runningUrls)
            {
                foreach (var url in siteUrls.OrderByDescending(site => site.Length))
                {
                    if (RuleSPTreeUtil.IsPrefixWithSlash(url, runningUrl) || RuleSPTreeUtil.IsPrefixWithSlash(runningUrl, url))
                    {
                        logger.Warn($"FilterHSMOptimizationUrl current scope :{url} has running job,so skip run subjob");
                        result.Remove(url);
                    }
                }
            }

            return result;
        }
        public async Task<string> RealRunOptimizationPreScanJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            RMDiscoverOptimizationPreScanJobInfo jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationPreScanJobInfo>(param);
            Guid o365TenantId = new Guid(jobParaInfo.SettingInfo.O365TenantId);
            List<RMDiscoverOptimizationPreScanNode> nodeInfo = new List<RMDiscoverOptimizationPreScanNode>();
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationJobCancel, jobParaInfo.SettingInfo.O365TenantId, TimeSpan.FromMinutes(10)))
            {
                List<JobType> types = new List<JobType>() { JobType.DiscoveryPreScan };

                var (siteIds, batchSize) = (jobParaInfo.SiteIds, 200);

                for (int i = 0; i < siteIds.Count; i += batchSize)
                {
                    var batchSiteIds = siteIds.Skip(i).Take(batchSize);
                    var sites = await _nodeDao.GetSiteInfosBySiteIds(o365TenantId, batchSiteIds);

                    foreach (RMDiscoveryOffice365SiteInfo siteInfo in sites)
                    {
                        RMDiscoverOptimizationPreScanNode tempNode = new RMDiscoverOptimizationPreScanNode();
                        tempNode.SiteUrl = siteInfo.Url;
                        tempNode.SiteInfoId = siteInfo.Id;
                        tempNode.SiteId = siteInfo.SiteId;
                        tempNode.sourceFlag = siteInfo.ContentSource;
                        tempNode.Setting = jobParaInfo.SettingInfo;
                        tempNode.O365TenantId = o365TenantId;
                        nodeInfo.Add(tempNode);
                    }
                }

                string jobId = string.Empty;
                string nodeUrl = "DiscoverOptimizationScope";
                string mainJobRunBy = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                jobId = RMJobService.CreateJobWithScopeId(JobType.DiscoveryPreScan, mainJobRunBy, nodeUrl);
                await AddSettingHistoryToAzureTableAsync(jobParaInfo.SettingInfo, o365TenantId, jobId);
                //foreach settings
                //get nodes by settings
                if (nodeInfo.IsNullOrEmpty() || nodeInfo.Count == 0)
                {
                    logger.Warn("No available sc to run.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                    return jobId;
                }


                List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex, JobType.ArchiverRetention, JobType.ArchiverDeduplication, JobType.DeleteOrphanDatas };
                List<Contract.JobMonitor.BaseJobDto> mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
                var (has, mainJobInfo) = await _jobDao.TryGetProcessingMainJobAsync();
                if (mIndexJobs.Count > 0 || has)
                {
                    //has move index job or discovery job, need skip.
                    logger.Warn("Current has move index job or discovery job or retention job running.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }

                List<string> scopes = RMJobService.GetRunningArchiverJobsScopes(types);
                foreach (var node in nodeInfo)
                {
                    if (scopes != null && scopes.Contains(node.SiteUrl))
                    {
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        logger.Warn($"current scope :{node.SiteUrl} has running job,so skip run job");
                        return jobId;
                    }
                }

                int subJobCount = nodeInfo.Count;

                SubJobDao.UpdateSubJobCount(jobId, subJobCount);
                RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);

                int currentSubjobIndex = 0;
                foreach (var node in nodeInfo)
                {
                    AddOptimizationSubjob(jobId, currentSubjobIndex, JobType.DiscoveryPreScan, subJobCount, node, false, node.SiteUrl, node.O365TenantId.ToString());
                    currentSubjobIndex++;
                }
                return jobId;
            }
        }

        public async Task<string> RealRunDiscoveryPlanProScanJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info("Begin RealRunDiscoveryPlanProScanJobAsync.");
            string mainJobRunBy = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            string nodeUrl = "DiscoverOptimizationScope";
            string jobId = RMJobService.CreateJobWithScopeId(JobType.DiscoveryPlanProScan, mainJobRunBy, nodeUrl);

            var profileIds = string.IsNullOrWhiteSpace(param)
                ? new List<string>()
                : SerializerHelper.DeserializeByDataContractSerializer<List<string>>(param) ?? new List<string>();
            var validProfileIds = profileIds
                .Where(id => !string.IsNullOrWhiteSpace(id) && int.TryParse(id, out var parsed) && parsed > 0)
                .Select(int.Parse)
                .Distinct()
                .ToList();

            if (!validProfileIds.Any())
            {
                SubJobDao.UpdateSubJobCount(jobId, 0);
                RMJobService.SetSumSCCountOfJobExtension(0, jobId);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                logger.Warn("RealRunDiscoveryPlanProScanJobAsync skipped because no valid profile id exists.");
                return jobId;
            }

            await RMDiscoveryDBManager.InitPlanTablesAsync();
            var candidateNodes = new List<RMDiscoverOptimizationPreScanNode>();
            foreach (var profileId in validProfileIds)
            {
                var profile = await _planProfileDao.GetByIdAsync(profileId);
                if (profile == null)
                {
                    logger.Warn($"RealRunDiscoveryPlanProScanJobAsync cannot find profile: {profileId}.");
                    continue;
                }

                var mappedNodeIds = await _planSiteMappingDao.GetNodeIdsByPlanProfileIdAsync(profileId);
                if (mappedNodeIds == null || mappedNodeIds.Count == 0)
                {
                    logger.Warn($"RealRunDiscoveryPlanProScanJobAsync profile has no mapped sites: {profileId}.");
                    continue;
                }

                var planRuleDefinition = BuildPlanProRuleDefinition(profileId, profile.Name, profile.Rules);
                foreach (var mappedNodeId in mappedNodeIds)
                {
                    var remoteNode = RMRemoteNodeDao.GetRemoteSiteCollectionById(mappedNodeId);

                    if (remoteNode == null)
                    {
                        logger.Warn($"Skip mapped site because remote node not found. mappedNodeId:{mappedNodeId}, profile:{profileId}.");
                        continue;
                    }
                    if (!Guid.TryParse(remoteNode.ObjectId, out var siteId))
                    {
                        logger.Warn($"Skip mapped site because site id is invalid. objectId:{remoteNode.ObjectId}, profile:{profileId}.");
                        continue;
                    }
                    if (!Guid.TryParse(remoteNode.TenantId, out var tenantId))
                    {
                        logger.Warn($"Skip mapped site because tenant id is invalid. tenantId:{remoteNode.TenantId}, profile:{profileId}.");
                        continue;
                    }

                    var siteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(tenantId, siteId);
                    if (siteInfo == null)
                    {
                        logger.Warn($"Skip mapped site because discovery site info not found. tenantId:{tenantId}, siteId:{siteId}, profile:{profileId}.");
                        continue;
                    }
                    var siteInfoId = siteInfo.Id;

                    var planSetting = BuildPlanProOptimizationSetting(profile, tenantId.ToString(), siteInfoId);
                    candidateNodes.Add(new RMDiscoverOptimizationPreScanNode
                    {
                        SiteUrl = remoteNode.url,
                        SiteInfoId = siteInfoId,
                        SiteId = siteId,
                        sourceFlag = (SourceFlag)remoteNode.NodeType,
                        SettingId = Guid.NewGuid(),
                        O365TenantId = tenantId,
                        Setting = planSetting,
                        PlanProOptimizationSetting = planSetting,
                        PlanProRuleDefinitions = planRuleDefinition == null ? new List<RMDiscoveryRuleDefinition>() : new List<RMDiscoveryRuleDefinition> { planRuleDefinition },
                        UseDalDataOptimizationService = true,
                    });
                }
            }

            var nodeInfo = candidateNodes
                .GroupBy(node => node.SiteUrl, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var first = group.First();
                    first.PlanProRuleDefinitions = group
                        .SelectMany(item => item.PlanProRuleDefinitions ?? new List<RMDiscoveryRuleDefinition>())
                        .Where(item => item != null)
                        .GroupBy(item => item.UniqueId)
                        .Select(item => item.First())
                        .ToList();
                    return first;
                })
                .ToList();

            if (!nodeInfo.Any())
            {
                SubJobDao.UpdateSubJobCount(jobId, 0);
                RMJobService.SetSumSCCountOfJobExtension(0, jobId);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                logger.Warn("RealRunDiscoveryPlanProScanJobAsync skipped because no executable site exists.");
                return jobId;
            }

            var runningUrl = RMJobService.GetRunningArchiverJobSiteUrl(
                new List<JobType> { JobType.DiscoveryPreScan, JobType.DiscoveryPlanProScan },
                nodeInfo.Select(node => node.SiteUrl),
                true);
            var runnableNodes = nodeInfo
                .Where(node => !runningUrl.Contains(node.SiteUrl, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (runnableNodes.Count == 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                logger.Warn("RealRunDiscoveryPlanProScanJobAsync all sites are in running-job conflict.");
                return jobId;
            }

            List<JobType> indexJobTypes = new List<JobType>()
            {
                JobType.ArchiverMoveIndex,
                JobType.ArchiverRetention,
                JobType.ArchiverDeduplication,
                JobType.DeleteOrphanDatas,
            };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
            var (has, mainJobInfo) = await _jobDao.TryGetProcessingMainJobAsync();
            if (mIndexJobs.Count > 0 || has)
            {
                logger.Warn("Current has move index job or discovery job or retention job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            int subJobCount = runnableNodes.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);

            int currentSubjobIndex = 0;
            foreach (var node in runnableNodes)
            {
                AddOptimizationSubjob(jobId, currentSubjobIndex, JobType.DiscoveryPlanProScan, subJobCount, node, false, node.SiteUrl, node.O365TenantId.ToString());
                currentSubjobIndex++;
            }

            logger.Info($"End RealRunDiscoveryPlanProScanJobAsync. subjob count:{subJobCount}.");
            return jobId;
        }

        public void AddOptimizationSubjob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, RMDiscoverOptimizationNode tempNode, bool sendNow, string scope, string o365TenantId)
        {
            RMSPTreeNode treeNode = new RMSPTreeNode();
            treeNode.SPObjectId = tempNode.SiteId.ToString();
            treeNode.O365TenantId = o365TenantId;
            treeNode.SiteId = tempNode.SiteId;
            treeNode.Level = 100;//siteCollection
            treeNode.FullPath = tempNode.SiteUrl;
            tempNode.TreeNode = treeNode;
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempNode) };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} , Path {3},settingId {4},365tenantId {5}", subJob.Id, subJob.JobType, subJob.Weight, scope, tempNode.SettingId, o365TenantId);
        }
        public void AddHSMSubjob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, RMHSMBackupNode tempNode, bool sendNow, string scope, string o365TenantId)
        {
            RMSPTreeNode treeNode = new RMSPTreeNode();
            treeNode.SPObjectId = tempNode.SiteId.ToString();
            treeNode.O365TenantId = o365TenantId;
            treeNode.SiteId = tempNode.SiteId;
            treeNode.Level = 100;//siteCollection
            treeNode.FullUrl = tempNode.SiteUrl;
            treeNode.FullPath = tempNode.SiteUrl;
            tempNode.TreeNode = treeNode;
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempNode) };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} , Path {3},settingId {4},365tenantId {5}", subJob.Id, subJob.JobType, subJob.Weight, scope, tempNode.SettingId, o365TenantId);
        }

        public void AddAOSPOptimizationSubjob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, RMDiscoverOptimizationNode tempNode, bool sendNow, string scope, string o365TenantId, string siteId)
        {
            RMSPTreeNode treeNode = new RMSPTreeNode();
            treeNode.SPObjectId = tempNode.SiteId.ToString();
            treeNode.O365TenantId = o365TenantId;
            treeNode.SiteId = tempNode.SiteId;
            treeNode.Level = 100;//siteCollection
            treeNode.FullPath = tempNode.SiteUrl;
            treeNode.SupportLockedSite = tempNode.SupportLockedSite;
            tempNode.TreeNode = treeNode;
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId, SiteId = siteId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempNode) };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} , Path {3},settingId {4},365tenantId {5}", subJob.Id, subJob.JobType, subJob.Weight, scope, tempNode.SettingId, o365TenantId);
        }

        private async Task AddSettingHistoryToAzureTableAsync(List<RMDiscoverOptimizationNode> nodeInfo, RMDiscoveryOffice365TenantInfo o365Info, string jobId)
        {
            try
            {
                logger.Info($"Begin AddSettingHistoryToAzureTableAsync.Job Id: {jobId}");
                var ruleSettingId = nodeInfo.FirstOrDefault()?.SettingId;
                if (ruleSettingId != null)
                {
                    logger.Info($"AddSettingHistoryToAzureTableAsync. Setting Id {ruleSettingId} Job Id {jobId}");

                    var historySettings = await ConvertSettingToJobHistorySettingsAsync((Guid)ruleSettingId, o365Info.UniqueId);
                    RMOptimizationSettingInfo settingsHistoryTableEntity = new RMOptimizationSettingInfo()
                    {
                        Id = Guid.NewGuid().ToString(),
                        PartitionKey = TenantLocalValue.LogonGroupId,
                        RowKey = jobId,
                        o365Id = o365Info.UniqueId.ToString(),
                        ArchivedTime = DateTime.UtcNow.Ticks,
                        JobId = jobId,
                        SettingID = ruleSettingId.ToString(),
                        Settings = SerializerHelper.SerializeByJsonSerializer(historySettings)
                    };
                    logger.Info($"AddSettingHistoryToAzureTableAsync begin add RMDiscoverDataOptimizationSettingsHistory.Job Id:{jobId}");
                    RMOptimizationSettingInfoDao.InsertInfo(settingsHistoryTableEntity);
                    logger.Info($"AddSettingHistoryToAzureTableAsync finished add RMDiscoverDataOptimizationSettingsHistory.Job Id:{jobId}");
                }
                else
                {
                    logger.Warn($"Cannot find setting from nodeInfo. Job Id {jobId}");
                }
                logger.Info($"Finished AddSettingHistoryToAzureTableAsync.Job Id: {jobId}");
            }
            catch (Exception e)
            {
                logger.Warn($"AddSettingHistoryToAzureTableAsync error. Job Id {jobId} error {e}");
            }
        }

        private async Task AddSettingHistoryToAzureTableAsync(List<RMDiscoverOptimizationNode> nodeInfo, RMDiscoveryAOSPTenantInfo o365Info, string jobId)
        {
            try
            {
                logger.Info($"Begin AddSettingHistoryToAzureTableAsync.Job Id: {jobId}");
                var ruleSettingId = nodeInfo.FirstOrDefault()?.SettingId;
                if (ruleSettingId != null)
                {
                    logger.Info($"AddSettingHistoryToAzureTableAsync. Setting Id {ruleSettingId} Job Id {jobId}");

                    var historySettings = await ConvertSettingToJobHistorySettingsAsync((Guid)ruleSettingId, o365Info.UniqueId);
                    RMOptimizationSettingInfo settingsHistoryTableEntity = new RMOptimizationSettingInfo()
                    {
                        Id = Guid.NewGuid().ToString(),
                        PartitionKey = TenantLocalValue.LogonGroupId,
                        RowKey = jobId,
                        o365Id = o365Info.UniqueId.ToString(),
                        ArchivedTime = DateTime.UtcNow.Ticks,
                        JobId = jobId,
                        SettingID = ruleSettingId.ToString(),
                        Settings = SerializerHelper.SerializeByJsonSerializer(historySettings)
                    };
                    logger.Info($"AddSettingHistoryToAzureTableAsync begin add RMDiscoverDataOptimizationSettingsHistory.Job Id:{jobId}");
                    RMOptimizationSettingInfoDao.InsertInfo(settingsHistoryTableEntity);
                    logger.Info($"AddSettingHistoryToAzureTableAsync finished add RMDiscoverDataOptimizationSettingsHistory.Job Id:{jobId}");
                }
                else
                {
                    logger.Warn($"Cannot find setting from nodeInfo. Job Id {jobId}");
                }
                logger.Info($"Finished AddSettingHistoryToAzureTableAsync.Job Id: {jobId}");
            }
            catch (Exception e)
            {
                logger.Warn($"AddSettingHistoryToAzureTableAsync error. Job Id {jobId} error {e}");
            }
        }

        private async Task AddSettingHistoryToAzureTableAsync(Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting, Guid o365TenantId, string jobId)   //新写一个
        {
            try
            {
                if (currentNodeSetting != null)
                {
                    logger.Info($"AddSettingHistoryToAzureTableAsync. Setting  Job Id {jobId}");

                    var historySettings = await ConvertSettingToJobHistorySettingsAsync(currentNodeSetting, o365TenantId);
                    RMOptimizationSettingInfo settingsHistoryTableEntity = new RMOptimizationSettingInfo()
                    {
                        Id = Guid.NewGuid().ToString(),
                        PartitionKey = TenantLocalValue.LogonGroupId,
                        RowKey = jobId,
                        o365Id = o365TenantId.ToString(),
                        ArchivedTime = DateTime.UtcNow.Ticks,
                        JobId = jobId,
                        SettingID = null,
                        Settings = SerializerHelper.SerializeByJsonSerializer(historySettings)
                    };
                    RMOptimizationSettingInfoDao.InsertInfo(settingsHistoryTableEntity);
                }
                else
                {
                    logger.Warn($"Cannot find setting from nodeInfo. Job Id {jobId}");
                }
            }
            catch (Exception e)
            {
                logger.Warn($"AddSettingHistoryToAzureTableAsync error. Job Id {jobId} error {e}");
            }
        }

        private async Task<DataOptimizationSettingsForJobHistory> ConvertSettingToJobHistorySettingsAsync(Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting, Guid O365Id)
        {
            IRMDiscoveryOffice365BasicInfoQueryService _basicInfoQueryService = new RMDiscoveryOffice365BaiscInfoQueryService();
            var fileExtensionsTask = _basicInfoQueryService.GetFileExtensionsAsync(O365Id);
            var withoutInDateListTask = _basicInfoQueryService.GetWithoutInDateListAsync();
            var sizeRangeListTask = _basicInfoQueryService.GetSizeRangeListAsync();

            DataOptimizationSettingsForJobHistory settingsHistory = new DataOptimizationSettingsForJobHistory();

            settingsHistory.ScopeSettings.MS365DataType = (AvePoint.RA.Contract.Discovery.Model.Configuration.Office365.MS365DataType)currentNodeSetting.MS365DataType;

            #region fileExtensions
            var fileExtensions = await fileExtensionsTask;
            PackageFileExtensionsToSettingsHistory(settingsHistory, currentNodeSetting, fileExtensions);
            #endregion

            #region withoutInDateList
            var withoutInDateList = await withoutInDateListTask;
            PackageWithoutInDateListToSettingsHistory(settingsHistory, currentNodeSetting, withoutInDateList);
            #endregion

            #region sizeRangeList
            var sizeRangeList = await sizeRangeListTask;
            PackageSizeRangeListToSettingsHistory(settingsHistory, currentNodeSetting, sizeRangeList);
            #endregion

            #region RuleList
            var rules = new List<RMDiscoveryOffice365RuleInfo>();

            if (currentNodeSetting.ArchiveDataType == (int)Contract.Discovery.Model.Configuration.Office365.ArchiverDataType.Special)
            {
                var inactiveRuleTask = DiscoverUtil.GetInactiveRuleAsync(currentNodeSetting.InactiveRuleQueryParameter, currentNodeSetting.ArchiveDataType);
                var rotRuleTask = DiscoverUtil.GetROTRuleAsync(currentNodeSetting.ROTRuleQueryParameter, currentNodeSetting.ArchiveDataType);
                var inactiveRule = await inactiveRuleTask;
                var rotRule = await rotRuleTask;
                if (inactiveRule != null && inactiveRule.Count > 0)
                {
                    rules.AddRange(inactiveRule);
                }
                if (rotRule != null && rotRule.Count > 0)
                {
                    rules.AddRange(rotRule);
                }
            }
            PackageRuleListToSettingsHistory(settingsHistory, currentNodeSetting, rules);
            PackageActionToSettingsHistory(settingsHistory, currentNodeSetting);
            #endregion

            return settingsHistory;
        }

        private List<RMDiscoverOptimizationNode> LoadNodesFromOptimizationManifest(RMHSMManifestOptimizationJobInfo jobInfo)
        {
            var nodes = new List<RMDiscoverOptimizationNode>();
            var manifestContent = ReadManifestContent(jobInfo);
            if (string.IsNullOrWhiteSpace(manifestContent))
            {
                return nodes;
            }

            XDocument document;
            try
            {
                document = XDocument.Parse(manifestContent, LoadOptions.None);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to parse optimization manifest. Path:{jobInfo.ManifestPath}.", ex);
                return nodes;
            }

            foreach (var siteElement in document.Root?.Elements("Site") ?? Enumerable.Empty<XElement>())
            {
                var siteUrl = (string)siteElement.Attribute("Url");
                var siteIdValue = (string)siteElement.Attribute("SiteId");
                var siteInfoIdValue = (string)siteElement.Attribute("SiteInfoId");
                var sourceFlagValue = (string)siteElement.Attribute("SourceFlag");

                if (string.IsNullOrWhiteSpace(siteUrl) || string.IsNullOrWhiteSpace(siteIdValue) || string.IsNullOrWhiteSpace(siteInfoIdValue))
                {
                    logger.Warn("Skip manifest entry because required attributes are missing. Url:{0}.", siteUrl ?? "<null>");
                    continue;
                }

                if (!Guid.TryParse(siteIdValue, out var siteId))
                {
                    logger.Warn("Skip manifest entry because SiteId is invalid. Value:{0}.", siteIdValue);
                    continue;
                }

                if (!long.TryParse(siteInfoIdValue, out var siteInfoId))
                {
                    logger.Warn("Skip manifest entry because SiteInfoId is invalid. Value:{0}.", siteInfoIdValue);
                    continue;
                }

                var sourceFlag = SourceFlag.SharePoint;
                if (!string.IsNullOrWhiteSpace(sourceFlagValue) && Enum.TryParse(sourceFlagValue, true, out SourceFlag parsedSource))
                {
                    sourceFlag = parsedSource;
                }

                nodes.Add(new RMDiscoverOptimizationNode
                {
                    SiteUrl = siteUrl,
                    SiteId = siteId,
                    SiteInfoId = siteInfoId,
                    SettingId = jobInfo.SettingId,
                    O365TenantId = jobInfo.o365Info.UniqueId,
                    sourceFlag = sourceFlag
                });
            }

            logger.Info("Loaded {0} manifest optimization entries.", nodes.Count);
            return nodes;
        }

        private string ReadManifestContent(RMHSMManifestOptimizationJobInfo jobInfo)
        {
            if (!string.IsNullOrWhiteSpace(jobInfo.ManifestXml))
            {
                return jobInfo.ManifestXml;
            }

            if (!string.IsNullOrWhiteSpace(jobInfo.ManifestPath))
            {
                if (System.IO.File.Exists(jobInfo.ManifestPath))
                {
                    return System.IO.File.ReadAllText(jobInfo.ManifestPath);
                }

                logger.Warn("Manifest file not found at path:{0}.", jobInfo.ManifestPath);
            }

            return string.Empty;
        }

        [DataContract]
        private sealed class RMHSMManifestOptimizationJobInfo
        {
            [DataMember]
            public RMDiscoveryOffice365TenantInfo o365Info { get; set; }

            [DataMember]
            public Guid SettingId { get; set; }

            [DataMember]
            public string ManifestPath { get; set; }

            [DataMember]
            public string ManifestXml { get; set; }
        }

        private async Task<DataOptimizationSettingsForJobHistory> ConvertSettingToJobHistorySettingsAsync(Guid ruleSettingId, Guid O365Id)
        {
            IRMDiscoveryOffice365OptimizationSettingsInfoDao optimizationSettingsInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();
            RMDiscoveryOffice365OptimizationSettingsInfo settingInfo = await optimizationSettingsInfoDao.GetSettingInfoByIdAsync(ruleSettingId, O365Id);
            Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting = SerializerHelper.DeserializeByDataContractSerializer<Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting>(AvePoint.RA.Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
            return await ConvertSettingToJobHistorySettingsAsync(currentNodeSetting, O365Id);
        }

        private void PackageFileExtensionsToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoveryFileExtensionDataInfo> fileExtensions)
        {
            if (currentNodeSetting.FileExtensionQueryParameter.FileExtensions != null && currentNodeSetting.FileExtensionQueryParameter.FileExtensions.Count == 0)
            {
                //all
                settingsHistory.ScopeSettings.FileExtensionDataInfos = fileExtensions;
            }
            else
            {
                settingsHistory.ScopeSettings.FileExtensionDataInfos = fileExtensions.Where(i => currentNodeSetting.FileExtensionQueryParameter.FileExtensions.Contains(i.Id)).ToList();
            }
            settingsHistory.ScopeSettings.FileCatagorysStr = ParseListToFormatString(settingsHistory.ScopeSettings.FileExtensionDataInfos.ConvertAll(f => f.RealName));
        }

        private void PackageWithoutInDateListToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoveryWithoutInDateDataInfo> withoutInDateList)
        {
            string modifiedTimeFrom = string.Empty;
            string modifiedTimeTo = string.Empty;
            if (currentNodeSetting.WithoutDateQueryParameter.From <= -1)
            {
                modifiedTimeFrom = $"0 {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
            }
            else
            {
                var from = withoutInDateList.FirstOrDefault(i => i.Id == currentNodeSetting.WithoutDateQueryParameter.From);
                if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (from?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeFrom = $"{from.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }

            if (currentNodeSetting.WithoutDateQueryParameter.To >= 999)
            {
                modifiedTimeTo = I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max");
            }
            else
            {
                var to = withoutInDateList.FirstOrDefault(i => i.Id == currentNodeSetting.WithoutDateQueryParameter.To);
                if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years")}";
                }
                else if (to?.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                {
                    modifiedTimeTo = $"{to.Unit} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months")}";
                }
            }
            settingsHistory.ScopeSettings.ModifiedTimeRangeStr = string.Format(I18NEntity.GetString("ExchangeOnline.Service_642972b7-1c4c-48e0-b94e-d968795edd09"), modifiedTimeFrom, modifiedTimeTo);
            settingsHistory.ScopeSettings.WithoutDateQueryParameter = currentNodeSetting.WithoutDateQueryParameter;
            settingsHistory.ScopeSettings.WithoutInDateDataInfos = withoutInDateList;
        }

        private void PackageSizeRangeListToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoverySizeRangeDataInfo> sizeRangeList)
        {
            if (currentNodeSetting.SizeRangeQueryParameter.SizeRange == 0 || currentNodeSetting.SizeRangeQueryParameter.QueryMode == RMDiscoverySizeRangeQueryMode.None)
            {
                settingsHistory.ScopeSettings.SizeRangeStr = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
            }
            else
            {
                settingsHistory.ScopeSettings.SizeRangeDataInfos = sizeRangeList.FirstOrDefault(i => i.Id == currentNodeSetting.SizeRangeQueryParameter.SizeRange);
                settingsHistory.ScopeSettings.SizeRangeStr = settingsHistory.ScopeSettings.SizeRangeDataInfos.Name;
            }
            settingsHistory.ScopeSettings.SizeRangeQueryParameter = currentNodeSetting.SizeRangeQueryParameter;
        }

        private void PackageRuleListToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting, List<RMDiscoveryOffice365RuleInfo> ruleList)
        {
            if (ruleList.Count == 0)
            {
                settingsHistory.DefinitionAndActionSettings.DefinitionsStr = "RM_FA_DataOptimize_Archive_All";
            }
            else
            {
                settingsHistory.DefinitionAndActionSettings.DefinitionsStr = ParseListToFormatString(ruleList.ConvertAll(r => r.Name));
                settingsHistory.DefinitionAndActionSettings.DefinitionsJson = SerializerHelper.SerializeByJsonSerializer(ruleList);
            }

        }

        private void PackageActionToSettingsHistory(DataOptimizationSettingsForJobHistory settingsHistory, Contract.Discovery.Model.Configuration.Office365.RMDiscoveryOffice365OptimizationSetting currentNodeSetting)
        {
            settingsHistory.DefinitionAndActionSettings.ProcessActionParameter = currentNodeSetting.ProcessActionParameter;
            settingsHistory.DefinitionAndActionSettings.ArchiveDataType = currentNodeSetting.ArchiveDataType;
            settingsHistory.DefinitionAndActionSettings.ROTRuleQueryParameter = currentNodeSetting.ROTRuleQueryParameter;
            settingsHistory.DefinitionAndActionSettings.InactiveRuleQueryParameter = currentNodeSetting.InactiveRuleQueryParameter;
            bool addFileAction = false;
            bool addFileVersionAction = false;
            if (currentNodeSetting.ArchiveDataType == (int)Contract.Discovery.Model.Configuration.Office365.ArchiverDataType.Special)
            {
                if (currentNodeSetting.ROTRuleQueryParameter.Enable)
                {
                    addFileAction = true;
                    addFileVersionAction = true;
                }
                else if (currentNodeSetting.InactiveRuleQueryParameter.Enable)
                {
                    addFileVersionAction = true;
                }
            }
            else
            {
                addFileAction = true;
            }

            if (addFileAction)
            {
                if (currentNodeSetting.ProcessActionParameter.FileAction == Contract.Discovery.Model.Configuration.Office365.FileAction.ArchiveAndRemove)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr = "RM_FA_DataOptimize_File_ArchiveAndRemove";
                    if (currentNodeSetting.ProcessActionParameter.IsEnableLeaveStub)
                    {
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "; ";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "RM_FA_DataOptimize_File_LeaveStub";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += " ";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += currentNodeSetting.ProcessActionParameter.StubSettingDto.Name;
                    }
                    if (currentNodeSetting.ProcessActionParameter.EnableArchivedLatestVersion)
                    {
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "; ";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "RM_JS_Audit_ArchiveVersionAndDestroyFile";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += " ";
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += currentNodeSetting.ProcessActionParameter.ArchivedLatestVersion;
                    }
                }
                else if (currentNodeSetting.ProcessActionParameter.FileAction == Contract.Discovery.Model.Configuration.Office365.FileAction.Archive)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr = "RM_JS_RDM_CreateRule_Options_Backup";
                    if (settingsHistory.DefinitionAndActionSettings.ProcessActionParameter != null && settingsHistory.DefinitionAndActionSettings.ProcessActionParameter.EnableArchivedOnlyLatestVersion)
                    {
                        settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "; " + "RM_JS_Rule_ArchiveVersionAndDestroyFile ";
                    }
                }
                else
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr = "RM_FA_DataOptimize_File_RemoveFile";
                }
                if (currentNodeSetting.ProcessActionParameter.DeleteRecords)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentActionStr += "; " + I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeDeclaredFile");
                }
            }
            if (addFileVersionAction)
            {
                if (currentNodeSetting.ProcessActionParameter.VersionAction == Contract.Discovery.Model.Configuration.Office365.VersionAction.ArchiveAndRemoveVerison)
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentVersionActionStr = "RM_FA_DataOptimize_Version_ArchiveAndRemove";
                }
                else
                {
                    settingsHistory.DefinitionAndActionSettings.DocumentVersionActionStr = "RM_FA_DataOptimize_Version_RemoveVersion";
                }
            }
        }

        private string ParseListToFormatString(List<string> list)
        {
            if (list.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                {
                    str.Append($"{list[i]}");
                }
                else
                {
                    str.Append($"{list[i]}, ");
                }
            }
            return str.ToString();
        }

        public async Task<string> RealRunAdjustStorageSizeJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            try
            {
                logger.Info($"Start adjust storage size job.");

                string jobId = string.Empty;
                JobType jobType = JobType.AdjustStorageSize;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = RMJobService.CreateJob(JobType.AdjustStorageSize, jobRunByUser, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                int currentIndex = 0;
                var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
                string subJobId = CreateSubJobForAdjustStorageSize(jobId, currentIndex, jobType, 1, currentIndex < subJobCountInConfigFile);
                if (currentIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Schedule,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                logger.Info($"Finish adjust storage size job.");
                return jobId;
            }
            catch (Exception e)
            {
                logger.Error($"Run adjust job failed,error:{e}");
                return string.Empty;
            }
        }

        public async Task<RAReturnMessage> RunExportIndexJob()
        {
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                if (!await LicenseHelperService.IsNewOpus() || !TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.ExportIndex)) return msg;
                string password = GeneratePassword(13, true, false, true, true);
                var dto = new JobQueueDto
                {
                    JobType = JobType.ExportIndex,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = password
                };
                msg.Extension = password;

                var jobId = JobQueueService.AddToDBJobQueue(dto);

                if (string.IsNullOrEmpty(jobId))
                {
                    logger.Error("Failed to add the export index job to the job queue. Job ID is null or empty.");
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                else
                {
                    logger.Info($"Successfully added the  export index job [{jobId}] to the job queue.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while running the export index job. Error: {ex}");
            }
            return msg;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.Globalsettings, Action = AuditAction.ExportIndex, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunExportIndexJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info($"Start run export index job.");

            List<JobType> types = new List<JobType>() { JobType.ExportIndex };
            var mJobs = RMJobService.GetRunningJobs(types);

            var jobId = RMJobService.CreateJob(JobType.ExportIndex, jobRunByUser);
            if (mJobs.Count > 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                return jobId;
            }
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            DownloadDataInfoDao.Create(new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = jobId,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = account.UserId,
                Name = jobId + ".zip",
                DownloadType = DownloadContentType.ExportIndex,
            });
            JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ExportIndex,
                CommandLine = $"{JobType.ExportIndex} {jobId} {param}",
                RunBy = jobRunBy,
            });

            return jobId;
        }



        public void FillRemoteSiteCollection(List<RemoteSiteCollection> remoteNodes, Dictionary<string, HashSet<EndUserArchiveSiteCollectionConfig>> containerAndNodesMapping, EndUserArchiveRequestParam request)
        {
            foreach (RemoteSiteCollection remoteSiteCollection in remoteNodes)
            {
                string containerId = string.IsNullOrEmpty(remoteSiteCollection.parentId) ? Guid.Empty.ToString() : remoteSiteCollection.parentId;
                if (!containerAndNodesMapping.TryGetValue(containerId, out HashSet<EndUserArchiveSiteCollectionConfig> siteCollectionConfigs))
                {
                    siteCollectionConfigs = new HashSet<EndUserArchiveSiteCollectionConfig>();
                    containerAndNodesMapping[containerId] = siteCollectionConfigs;
                }

                bool siteConfigExists = siteCollectionConfigs.Any(config => config.SiteCollectionId.ToString().EqualsIgnoreCase(remoteSiteCollection.ObjectId));
                if (siteConfigExists)
                {
                    continue;
                }

                EndUserArchiveSiteCollectionConfig config = new EndUserArchiveSiteCollectionConfig
                {
                    Office365TenantId = remoteSiteCollection.TenantId,
                    SiteCollectionId = remoteSiteCollection.ObjectId,
                    RuleAction = request.RuleAction,
                };
                siteCollectionConfigs.Add(config);
                config.FileInfoList = request.FileInfoList.Where(file => file.SiteCollectionId.ToString().EqualsIgnoreCase(remoteSiteCollection.ObjectId)).ToList();
                request.FileInfoList = request.FileInfoList.Where(file => !file.SiteCollectionId.ToString().EqualsIgnoreCase(remoteSiteCollection.ObjectId)).ToList();
            }
            FillSiteCollectionNotFoundFiles(containerAndNodesMapping, request.FileInfoList, request);
        }

        private void FillSiteCollectionNotFoundFiles(Dictionary<string, HashSet<EndUserArchiveSiteCollectionConfig>> containerAndNodesMapping, List<EndUserFileInfo> fileInfos, EndUserArchiveRequestParam request)
        {
            if (fileInfos == null || !fileInfos.Any())
            {
                return;
            }
            if (!containerAndNodesMapping.Any())
            {
                containerAndNodesMapping.Add(Guid.Empty.ToString(), new HashSet<EndUserArchiveSiteCollectionConfig>());
            }
            logger.Info($"FillSiteCollectionNotFoundFiles, fileInfos : {fileInfos.Count} | {string.Join(',', fileInfos.Select(f => $"{f.FullPath}|{f.Id}"))}");
            EndUserArchiveSiteCollectionConfig exceptionFileConfig = new EndUserArchiveSiteCollectionConfig() { RuleAction = request.RuleAction };
            containerAndNodesMapping.First().Value.Add(exceptionFileConfig);
            exceptionFileConfig.ExceptionFileInfoList.AddRange(fileInfos);
            foreach (EndUserFileInfo fileInfo in fileInfos)
            {
                fileInfo.Status = AvePoint.Api.Contract.Job.JobDetailsStatus.Exception;
                fileInfo.ErrorMessage = "RM_TS_SCNotRegisterInOpus";
            }
        }

        private void UpdateChannelSiteParentIdAsContainerId(List<RemoteSiteCollection> remoteNodes)
        {
            if (!RMKeyValueDao.HasUpgradeTeams() || remoteNodes == null)
            {
                return;
            }

            List<RemoteSiteCollection> channelSiteSet = remoteNodes.Where(node => node.NodeType == RemoveNodeType.PrivateChannel).ToList();
            if (!channelSiteSet.Any())
            {
                return;
            }

            List<string> channelTeamIds = channelSiteSet.Select(info => info.TeamId).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet().ToList();
            if (!channelTeamIds.Any())
            {
                return;
            }

            var groupSitesDict = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsIds(channelTeamIds);
            var groupSites = groupSitesDict?.Keys?.ToList();
            if (groupSites == null || groupSites.Count == 0)
            {
                return;
            }

            foreach (RemoteSiteCollection remoteSiteCollection in channelSiteSet)
            {
                var parentSite = groupSites.FirstOrDefault(site => site.TeamId.EqualsIgnoreCase(remoteSiteCollection.TeamId));
                if (parentSite != null)
                {
                    remoteSiteCollection.parentId = parentSite.parentId;
                }
            }
        }

        private void QueueEndUserArchiveJobs(Dictionary<string, HashSet<EndUserArchiveSiteCollectionConfig>> containerAndNodesMapping, RMEndUserArchiveReturnMessage response)
        {
            response.JobInfos = new Dictionary<string, List<EndUserFileInfo>>();
            var groupId = TenantLocalValue.LogonGroupId;
            foreach (var map in containerAndNodesMapping)
            {
                List<EndUserFileInfo> infoList = map.Value.SelectMany(siteConfig => siteConfig.FileInfoList.Concat(siteConfig.SkipFileInfoList).Concat(siteConfig.ExceptionFileInfoList)).ToList();
                string jobId = JobMonitorService.GenerateJobId(JobType.RMEndUserArchiverBackup);
                try
                {
                    EndUserArchiveContainerConfig config = new()
                    {
                        JobId = jobId,
                        ContainerId = map.Key,
                        SiteCollectionConfigs = map.Value
                    };

                    JobQueueDto jqDto = new JobQueueDto()
                    {
                        JobType = JobType.RMEndUserArchiverBackup,
                        JobRunType = JobRunBy.Schedule,
                        TenantGroupId = groupId,
                        JobRunByUser = "RM_TS_RunSchedule",
                        Parameters = SerializerHelper.SerializeByDataContractSerializer(config),
                        Extension = config.JobId
                    };
                    JobQueueService.AddToDBJobQueue(jqDto);
                    logger.Info($"Request Run EndUser Archiver Backup Job success, job id : {config.JobId}");
                }
                catch
                {
                    logger.Info($"Request Run EndUser Archiver Backup Job fail, job id : {jobId}");
                    foreach (var info in infoList)
                    {
                        info.Status = info.Status == AvePoint.Api.Contract.Job.JobDetailsStatus.Successful ? AvePoint.Api.Contract.Job.JobDetailsStatus.Exception : info.Status;
                    }
                }
                finally
                {
                    response.JobInfos.Add(jobId, infoList);
                }
            }
        }

        public RMEndUserArchiveReturnMessage RunEndUserArchiverBackupJob(EndUserArchiveRequestParam request)
        {
            RMEndUserArchiveReturnMessage response = new RMEndUserArchiveReturnMessage() { JobInfos = new() };
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    throw new Exception(I18NEntity.GetString("NoOperationPermission"));
                }
                PrintEndUserArchiveParam(request);
                List<RemoteSiteCollection> remoteNodes = RMRemoteNodeDao.GetRemoteSiteCollectionByObjectIds(request.FileInfoList.Select(info => info.SiteCollectionId).Select(id => id.ToString()).ToList());
                UpdateChannelSiteParentIdAsContainerId(remoteNodes);
                Dictionary<string, HashSet<EndUserArchiveSiteCollectionConfig>> containerAndNodesMapping = new();
                FillRemoteSiteCollection(remoteNodes, containerAndNodesMapping, request);
                //logger.Info($"prepare send to job qeueu for EndUser Archiver Backup Job, config : {SerializerHelper.SerializeByDataContractSerializer(containerAndNodesMapping)}");
                QueueEndUserArchiveJobs(containerAndNodesMapping, response);
                logger.Info("End request Run EndUser Archive All");
            }
            catch (Exception e)
            {
                logger.Error($"Run EndUser Archiver Backup Job failed, error : {e}");
                response.JobInfos.Add("", request?.FileInfoList ?? new());
                foreach (var info in request?.FileInfoList ?? new())
                {
                    info.Status = AvePoint.Api.Contract.Job.JobDetailsStatus.Exception;
                }
                response.MessageType = RAMessageType.Failed;
                response.ErrorMessage = e.Message;
            }
            return response;
        }

        private void PrintEndUserArchiveParam(EndUserArchiveRequestParam request)
        {
            try
            {
                EndUserArchiveRequestParam copyRequest = SerializerHelper.Copy(request);
                foreach (var fileInfo in copyRequest.FileInfoList)
                {
                    if (string.IsNullOrWhiteSpace(fileInfo.FullPath))
                    {
                        continue;
                    }
                    int parentPathLength = fileInfo.FullPath.LastIndexOf('/') >= 0 ? fileInfo.FullPath.LastIndexOf('/') + 1 : fileInfo.FullPath.Length;
                    fileInfo.FullPath = fileInfo.FullPath.Substring(0, parentPathLength);
                }
                logger.Info($"Run EndUser Archiver Backup Job, config : {SerializerHelper.SerializeByDataContractSerializer(copyRequest)}");
            }
            catch (Exception e)
            {
                logger.Warn($"Fail print end user archive param,e:{e}  ,Run EndUser Archiver Backup Job");
            }
        }

        public RAReturnMessage RunSpecifySitesArchiverBackupJob(List<string> siteUrls)
        {
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var jobId = JobMonitorService.GenerateJobId(JobType.SpecifySitesArchiverBackup);
                var paramsDto = new SpecifySitesArchiverBackupParameters() { SitesUrlList = siteUrls, PreGeneratedJobId = jobId };
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SpecifySitesArchiverBackup,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = groupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(paramsDto),
                    Extension = jobId
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = jobId };
            }
            catch (Exception e)
            {
                logger.Error($"Run specify site failed, error : {e}");
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = e.Message;
                msg.FaildType = RAFailedType.None;
            }

            return msg;
        }


        public RAReturnMessage RunSpecifyTeamsArchiverBackupJob(List<string> teamIdList)
        {
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var jobId = JobMonitorService.GenerateJobId(JobType.SpecifyTeamsArchiverBackup);
                var paramsDto = new SpecifyTeamsArchiverBackupParameters() { TeamIdList = teamIdList, PreGeneratedJobId = jobId };
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SpecifyTeamsArchiverBackup,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = groupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(paramsDto),
                    Extension = jobId
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                msg = new RAReturnMessage() { MessageType = RAMessageType.Successful, FaildType = RAFailedType.None, Extension = jobId };
            }
            catch (Exception e)
            {
                logger.Error($"Run specify team failed, error : {e}");
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = e.Message;
                msg.FaildType = RAFailedType.None;
            }

            return msg;
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunArchiverBackupJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunSpecifySitesArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            bool hasSoLicense = LicenseHelperService.HasOpusSOLicense;

            JobType jobType = JobType.SpecifySitesArchiverBackup;

            var paramsDto = SerializerHelper.DeserializeByDataContractSerializer<SpecifySitesArchiverBackupParameters>(param);
            List<string> sitesUrlList = paramsDto.SitesUrlList;
            var preGeneratedJobId = paramsDto.PreGeneratedJobId;
            logger.Info($"start process sprcial site archive job:{preGeneratedJobId}");
            if (!hasSoLicense)
            {
                string nodeUrl = sitesUrlList.FirstOrDefault();
                RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifySitesArchiverBackup, jobRunByUser, nodeUrl, "");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Failed, "RM_Job_NOSOLicense");
                logger.Error("this user has no so license,cannot run job");
                return preGeneratedJobId;
            }

            List<RMSPTreeNode> selectedNodes = new List<RMSPTreeNode>();
            try
            {
                foreach (var siteUrl in sitesUrlList)
                {
                    var selectedRemoteNode = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    if (selectedRemoteNode == null)
                    {
                        logger.Warn("Site collection not exist, site:{0}", siteUrl);
                        continue;
                    }
                    var treeNode = RMDtoConverter.ConvertRemoteSite2RMTree(selectedRemoteNode);
                    treeNode.O365TenantId = selectedRemoteNode.TenantId;

                    var group = RABrowserClient.GetWebApplicationById(selectedRemoteNode.parentId);
                    treeNode.Parent = RMDtoConverter.ConvertRemoteWebApplication2RMTree(group);
                    treeNode.Parent.O365TenantId = selectedRemoteNode.TenantId;
                    treeNode.SupportLockedSite = true;
                    treeNode.SupportArchivedTeams = true;
                    selectedNodes.Add(treeNode);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Fail build site collection info for special sites, e:{e}");
                string errorMessage = "";
                if (I18NEntity.HasKey(e.Message))
                {
                    errorMessage = e.Message;
                }
                string nodeUrl = sitesUrlList?.FirstOrDefault();
                RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifySitesArchiverBackup, jobRunByUser, nodeUrl, "");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Failed, errorMessage);
                logger.Error("this user has no so license,cannot run job");
                return preGeneratedJobId;
            }


            return InnerRealRunSpecifySitesArchiverBackupJob(jobRunByUser, jobType, selectedNodes, preGeneratedJobId);
        }




        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunArchiverBackupJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunSpecifyTeamsArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.SpecifyTeamsArchiverBackup;

            var paramsDto = SerializerHelper.DeserializeByDataContractSerializer<SpecifyTeamsArchiverBackupParameters>(param);
            List<string> teamIdList = paramsDto.TeamIdList;
            var preGeneratedJobId = paramsDto.PreGeneratedJobId;
            logger.Info($"Start process specify teams archive job:{preGeneratedJobId}");

            List<RMSPTreeNode> selectedNodes = new List<RMSPTreeNode>();

            try
            {
                foreach (var teamId in teamIdList)
                {
                    if (selectedNodes.Any())
                    {
                        break;
                    }
                    var selectedRemoteNode = RABrowserClient.GetRemoteTeamByTeamId(teamId);
                    if (selectedRemoteNode == null)
                    {
                        logger.Warn("team not exist, site:{0}", teamId);
                        continue;
                    }
                    var treeNode = RABrowserClient.GetRemoteTeamByTeamId(teamId);

                    var group = RABrowserClient.GetWebApplicationById(treeNode.ParentId);
                    treeNode.Parent = RMDtoConverter.ConvertRemoteWebApplication2RMTree(group);
                    treeNode.Parent.O365TenantId = treeNode.O365TenantId;
                    treeNode.SupportLockedSite = true;
                    treeNode.SupportArchivedTeams = true;
                    selectedNodes.Add(treeNode);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Fail run special teams archive job {preGeneratedJobId},e:{e}");
                string errorMessage = "";
                if (I18NEntity.HasKey(e.Message))
                {
                    errorMessage = e.Message;
                }
                RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifyTeamsArchiverBackup, jobRunByUser, selectedNodes?.FirstOrDefault()?.FullPath, "");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Failed, errorMessage);
                return preGeneratedJobId;
            }

            return RealRunSpecifyTeamsArchiverBackupJob(jobRunByUser, jobType, selectedNodes.FirstOrDefault(), preGeneratedJobId);
        }

        public string RealRunSpecifyTeamsArchiverBackupJob(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode, string preGeneratedJobId)
        {
            if (selectedNode == null)
            {
                RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifyTeamsArchiverBackup, jobRunByUser, "", "");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Failed, "RM_JM_Archive_TeamNotExistInOpus_ErrorMessage");
                return preGeneratedJobId;
            }
            if (!LicenseHelperService.HasOpusSOLicense || !RMKeyValueDao.HasUpgradeTeams())
            {
                RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifyTeamsArchiverBackup, jobRunByUser, selectedNode.FullPath, "");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Failed, !LicenseHelperService.HasOpusSOLicense ? "No SO License" : "Not Upgrade Teams");
                logger.Error("this user has no TEAMS so license,cannot run job");
                return preGeneratedJobId;
            }
            string teamsUrl = selectedNode.GetTeamsNode()?.DisplayName ?? (RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.GetTeamsNode()?.SPObjectId).Item1?.url ?? string.Empty);
            string nodeFullPath = selectedNode.Level == (int)NodeLevel.Office365GroupEntire ? selectedNode.DisplayName ?? teamsUrl : selectedNode.FullPath;
            string nodeUrl = selectedNode.FullPath;
            bool useArchiverImportFile = selectedNode.UserArchiverImportFile;
            List<string> archiverImportSitesUrl = selectedNode.ArchiverImportSitesUrl;
            List<RMSPTreeNode> availableNode = AssembleTeamsDisposalRunnableNode(selectedNode);

            RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifyTeamsArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoTeams");
                return preGeneratedJobId;
            }
            if (availableNode.Count == 0)
            {
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Skipped, "RM_Job_Teams_ArchiverImportSkip");
                return preGeneratedJobId;
            }

            List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
            if (mIndexJobs.Count > 0)
            {
                //has move index job, need skip.
                logger.Warn("teams Current has move index or retention job running.");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return preGeneratedJobId;
            }

            var runningUrls = RMJobService.GetRunningTeamsArchiverJobSiteUrl(JobTypeConstants.ArchiveTeamsConflictType,
                RuleSPTreeUtil.CheckNeedLoadRuningSCUrlBySelectNode(selectedNode),
                RuleSPTreeUtil.BuildSearchFilter(selectedNode, availableNode));
            availableNode = RuleSPTreeUtil.FilterTeamsAvailableNodeByRunningUrl(availableNode, runningUrls, selectedNode);

            if (availableNode.Count == 0)
            {
                logger.Warn($"not exsite can run job,will skip current job");
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return preGeneratedJobId;
            }

            string jobExtension = RuleSPTreeUtil.GenerateTeamsArchiveJobMonitorExtension(selectedNode, TreeMode.SO, teamsUrl: teamsUrl);
            JMDao.UpdateJobExtension(preGeneratedJobId, jobExtension);

            logger.Info($"real run job node count after filter is {availableNode.Count}");

            int subJobCount = availableNode.Count;

            SubJobDao.UpdateSubJobCount(preGeneratedJobId, subJobCount);
            RMJobService.SetSumSCCountOfJobExtension(subJobCount, preGeneratedJobId);

            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            if (!IsTrailLicenceAndExceedSizeLimit())
            {
                foreach (RMSPTreeNode node in availableNode)
                {
                    tempList.Add(node);
                    string subJobId = CreateSubJobForDisposal(preGeneratedJobId, currentSubjobIndex, jobType, subJobCount, tempList, false, node.FullPath, node.O365TenantId);
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            else
            {
                RMJobService.UpdateJobStatus(preGeneratedJobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
            }
            return preGeneratedJobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunEndUserArchiverBackupJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunEndUserArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.RMEndUserArchiverBackup;
            var containerConfig = SerializerHelper.DeserializeByDataContractSerializer<EndUserArchiveContainerConfig>(param);
            logger.Info($"RealRunEndUserArchiverBackupJob for job id:{containerConfig.JobId}, container id:{containerConfig.ContainerId}," +
                $" sc:{string.Join("; ", containerConfig.SiteCollectionConfigs.Select(config => config.SiteCollectionId))}");

            bool hasJobLevelConflicts = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes).Any();
            if (hasJobLevelConflicts)
            {
                logger.Warn($"has job level conflict in end user archive,job id:{containerConfig.JobId}, container id:{containerConfig.ContainerId} ");
            }

            bool isTrailLicenceAndExceedSizeLimit = IsTrailLicenceAndExceedSizeLimit();
            if (isTrailLicenceAndExceedSizeLimit)
            {
                logger.Warn($"isTrailLicenceAndExceedSizeLimit in end user archive,job id:{containerConfig.JobId}, container id:{containerConfig.ContainerId} ");
            }

            List<RMSPTreeNode> nodeList = new List<RMSPTreeNode>();
            foreach (EndUserArchiveSiteCollectionConfig siteCollectionConfig in containerConfig.SiteCollectionConfigs)
            {
                logger.Info($"start build end user sub job info for job id:{containerConfig.JobId},sc :{siteCollectionConfig.SiteCollectionId}");
                RMSPTreeNode treeNode = PrepareEndUserArchiverTreeNode(siteCollectionConfig);
                if (treeNode == null)
                {
                    logger.Warn($"job id:{containerConfig.JobId},sc :{siteCollectionConfig.SiteCollectionId}, not exist in opus");
                    treeNode = new RMSPTreeNode() { EndUserArchiveSiteCollectionConfig = siteCollectionConfig };
                    MoveEndUserFilesToExceptionList(siteCollectionConfig, AvePoint.Api.Contract.Job.JobDetailsStatus.Exception, "RM_TS_SCNotRegisterInOpus");
                }
                else if (hasJobLevelConflicts)
                {
                    logger.Warn($"job id:{containerConfig.JobId},sc :{siteCollectionConfig.SiteCollectionId}, has job level conflict, will skip current job");
                    MoveEndUserFilesToSkipList(siteCollectionConfig, AvePoint.Api.Contract.Job.JobDetailsStatus.Skipped, "RM_Job_ScheduledJobConflictForCurrentItem");
                }
                else if (isTrailLicenceAndExceedSizeLimit)
                {
                    logger.Warn($"job id:{containerConfig.JobId},sc :{siteCollectionConfig.SiteCollectionId}, is trial licence and exceed size limit");
                    MoveEndUserFilesToExceptionList(siteCollectionConfig, AvePoint.Api.Contract.Job.JobDetailsStatus.Exception, "RM_Job_TrailSizeLimit");
                }
                else
                {
                    FilterEndUserConflictingFiles(siteCollectionConfig, treeNode);
                }
                nodeList.Add(treeNode);
            }
            return CreateAndRunEndUserArchiverJob(containerConfig.JobId, jobRunByUser, jobType, nodeList, containerConfig.ContainerId);
        }

        private RMSPTreeNode PrepareEndUserArchiverTreeNode(EndUserArchiveSiteCollectionConfig paramsDto)
        {
            if (!Guid.TryParse(paramsDto.SiteCollectionId, out Guid scId))
            {
                logger.Warn("site collection not exist, site collection id:{0}", paramsDto.SiteCollectionId);
                return null;
            }
            var selectedRemoteNode = RABrowserClient.GetRemoteSiteCollectionByObjectId(paramsDto.SiteCollectionId.ToString());
            if (selectedRemoteNode == null)
            {
                logger.Warn("site collection not exist, site collection id:{0}", paramsDto.SiteCollectionId);
                return null;
            }

            var treeNode = RMDtoConverter.ConvertRemoteSite2RMTree(selectedRemoteNode);
            treeNode.O365TenantId = selectedRemoteNode.TenantId;
            var group = RABrowserClient.GetWebApplicationById(selectedRemoteNode.parentId);
            treeNode.Parent = group == null ? null : RMDtoConverter.ConvertRemoteWebApplication2RMTree(group);
            if (treeNode.Parent != null)
            {
                treeNode.Parent.O365TenantId = selectedRemoteNode.TenantId;
            }
            treeNode.EndUserArchiveSiteCollectionConfig = paramsDto;
            return treeNode;
        }

        private void FilterEndUserConflictingFiles(EndUserArchiveSiteCollectionConfig paramsDto, RMSPTreeNode treeNode)
        {
            paramsDto.FileInfoList = paramsDto.FileInfoList
                .Where(file => file != null && !string.IsNullOrWhiteSpace(file.GetDecodedFullPath()))
                .GroupBy(file => file.Id + '/' + file.GetDecodedFullPath(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First()).ToList();

            paramsDto.SkipFileInfoList = paramsDto.SkipFileInfoList ?? new();
            var runningSiteUrls = RMJobService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, [treeNode.GetSiteCollectionNode().FullPath], true);

            foreach (var fileInfo in paramsDto.FileInfoList.ToArray())
            {
                List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>() { treeNode };
                availableNode = RuleSPTreeUtil.FilterSCAvailableNodeByRunningUrl(availableNode, runningSiteUrls,
                    new RMSPTreeNode { FullPath = fileInfo.GetDecodedFullPath(), Level = (int)NodeLevel.Item });
                if (availableNode == null || !availableNode.Any())
                {
                    fileInfo.Status = AvePoint.Api.Contract.Job.JobDetailsStatus.Skipped;
                    fileInfo.ErrorMessage = "StorageOptimization_EndUserArchive_ItemProcessByOtherJob";
                    paramsDto.FileInfoList.Remove(fileInfo);
                    paramsDto.SkipFileInfoList.Add(fileInfo);
                }
            }

            if (paramsDto.SkipFileInfoList.Any())
            {
                logger.Warn($"Some files conflict with running job, will not process them in this job. items:{string.Join("; ", paramsDto.SkipFileInfoList.Select(node => node.GetDecodedFullPath()))}");
            }
        }

        private static void MoveEndUserFilesToExceptionList(EndUserArchiveSiteCollectionConfig config, AvePoint.Api.Contract.Job.JobDetailsStatus status, string message)
        {
            config.ExceptionFileInfoList = config.ExceptionFileInfoList ?? new List<EndUserFileInfo>();
            if (config.FileInfoList == null || config.FileInfoList.Count == 0)
            {
                return;
            }

            foreach (var fileInfo in config.FileInfoList.Where(file => file != null))
            {
                fileInfo.Status = status;
                fileInfo.ErrorMessage = message;
            }

            config.ExceptionFileInfoList.AddRange(config.FileInfoList.Where(file => file != null));
            config.FileInfoList.Clear();
        }

        private static void MoveEndUserFilesToSkipList(EndUserArchiveSiteCollectionConfig config, AvePoint.Api.Contract.Job.JobDetailsStatus status, string message)
        {
            if (config == null)
            {
                return;
            }

            config.SkipFileInfoList ??= new List<EndUserFileInfo>();
            if (config.FileInfoList == null || config.FileInfoList.Count == 0)
            {
                return;
            }

            foreach (var fileInfo in config.FileInfoList.Where(file => file != null))
            {
                fileInfo.Status = status;
                fileInfo.ErrorMessage = message;
            }

            config.SkipFileInfoList.AddRange(config.FileInfoList.Where(file => file != null));
            config.FileInfoList.Clear();
        }

        private string CreateAndRunEndUserArchiverJob(string jobId, string jobRunByUser, JobType jobType, List<RMSPTreeNode> nodeAndConfigDic, string containerId)
        {
            RMSPTreeNode containerNode = new RMSPTreeNode() { Level = (int)NodeLevel.SiteCollection };
            List<string> siteCollectionUrls = nodeAndConfigDic
                .Where(node => !string.IsNullOrWhiteSpace(node.FullPath))
                .Select(node => node.FullPath).ToHashSet().ToList();
            List<string> fileUrls = new List<string>();
            foreach (RMSPTreeNode node in nodeAndConfigDic)
            {
                fileUrls.AddRange(node.EndUserArchiveSiteCollectionConfig.FileInfoList.Select(file => file.GetDecodedFullPath()));
            }

            var tempExtension = RuleSPTreeUtil.GenerateArchiveJobMonitorExtension(containerNode, TreeMode.SO, siteCollectionUrls, false, fileUrls);
            RemoteWebApplication container = RMRemoteNodeDao.GetWebApplicationById(containerId);
            jobId = RMJobService.CreateJobWithScopeIdAndJobId(jobId, JobType.RMEndUserArchiverBackup, jobRunByUser, container?.url, containerId, null, tempExtension);

            if (!nodeAndConfigDic.Any())
            {
                logger.Error($"end user job for id :{jobId}, containerId:{containerId},no any sc reqeust was process");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "");
                return jobId;
            }

            SubJobDao.UpdateSubJobCount(jobId, nodeAndConfigDic.Count);
            RMJobService.SetSumSCCountOfJobExtension(nodeAndConfigDic.Count, jobId);

            int currentSubjobIndex = 0;
            foreach (RMSPTreeNode node in nodeAndConfigDic)
            {
                string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex++, jobType, nodeAndConfigDic.Count, [node], false, node.FullPath, node.O365TenantId);
                if (!node.EndUserArchiveSiteCollectionConfig.FileInfoList.Any())
                {
                    SubJobDao.UpdateRunable(subJobId, (int)RecordsConstants.SubJob_Runnable_CanRun);
                }
            }
            logger.Info($"success create end user job for id :{jobId}, containerId:{containerId}," +
                $"sc ids:{string.Join("; ", nodeAndConfigDic.Select(node => node.SPObjectId))}");
            return jobId;
        }

        private string InnerRealRunSpecifySitesArchiverBackupJob(string jobRunByUser, JobType jobType, List<RMSPTreeNode> selectedNodes, string preGeneratedJobId)
        {
            List<JobType> types = new List<JobType>() { JobType.SpecifySitesArchiverBackup, JobType.RecordsDisposal, JobType.OneDriveRecordsDisposal };
            string jobId = string.Empty;
            if (selectedNodes.FirstOrDefault() == null)
            {
                jobId = RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifySitesArchiverBackup, jobRunByUser, "", "");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JM_Archive_SiteRemoveFromAOS_ErrorMessage");
                return jobId;
            }
            string nodeUrl = selectedNodes.First().Parent.FullPath;
            List<RMSPTreeNode> availableNode = selectedNodes;
            var scopes = JobMonitorService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, availableNode.Select(n => n.GetSiteCollectionNode().FullPath), true);
            var tempExtension = RuleSPTreeUtil.GenerateArchiveJobMonitorExtension(selectedNodes.First(), TreeMode.SO, selectedNodes.Select(node => node.FullPath).ToList(), true, new List<string>());
            jobId = RMJobService.CreateJobWithScopeIdAndJobId(preGeneratedJobId, JobType.SpecifySitesArchiverBackup, jobRunByUser, nodeUrl, GetSPContainerId(selectedNodes.First()), "", tempExtension);



            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_RunJobFailed_NoSiteCollection");
                return jobId;
            }
            List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex, JobType.ArchiverDeduplication };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                //has move index job, need skip.
                logger.Warn("Current has move index or retention job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }



            availableNode = RuleSPTreeUtil.FilterSCAvailableNodeByRunningUrl(availableNode, scopes, selectedNodes.First());
            if (availableNode.Count == 0)
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            int subJobCount = availableNode.Count;

            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);

            //RMRunningJobRuleMappingDao.AddJobRuleMappings(TenantLocalValue.LogonGroupId, jobId, GetAppliedRuleIds(selectedNode));
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            if (!IsTrailLicenceAndExceedSizeLimit())
            {
                foreach (RMSPTreeNode node in availableNode)
                {
                    tempList.Add(node);
                    string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, tempList, false, node.FullPath, node.O365TenantId);
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
            }
            return jobId;
        }

        private string GeneratePassword(int intLength, bool booNumber, bool booSign, bool booSmallword, bool booBigword)
        {
            int intResultRound = 0;
            string strB = "";
            while (intResultRound < intLength)
            {
                int intA = SecurityUtils.GetRandomNumber(1, 5);
                if (intA == 1 && booNumber)
                {
                    intA = SecurityUtils.GetRandomNumber(0, 10);
                    strB = intA.ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }
                if (intA == 2 && booSign)
                {
                    intA = SecurityUtils.GetRandomNumber(1, 5);
                    if (intA == 1)
                    {
                        intA = SecurityUtils.GetRandomNumber(33, 48);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                    if (intA == 2)
                    {
                        intA = SecurityUtils.GetRandomNumber(58, 65);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                    if (intA == 3)
                    {
                        intA = SecurityUtils.GetRandomNumber(91, 97);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                    if (intA == 4)
                    {
                        intA = SecurityUtils.GetRandomNumber(123, 127);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                }
                if (intA == 3 && booSmallword)
                {
                    intA = SecurityUtils.GetRandomNumber(97, 123);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }
                if (intA == 4 && booBigword)
                {
                    intA = SecurityUtils.GetRandomNumber(65, 89);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                }
            }
            return strB;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.Globalsettings, Action = AuditAction.CopyExportIndexPassword, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string CopyPasswordAudit()
        {
            return string.Empty;
        }

        private bool IsEnableDeleteOrphanDataSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableDeleteOrphanData");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        public bool CheckRemoteNodeHaveRunningJob(RMSPTreeNode selectedTree, List<JobType> checkJobTypes)
        {
            if (checkJobTypes == null || !checkJobTypes.Any())
            {
                return false;
            }
            try
            {
                RMSPTreeNode needCheckNode = selectedTree;
                while (needCheckNode.Parent != null && needCheckNode.Level > (int)NodeLevel.SiteCollection)
                {
                    needCheckNode = needCheckNode.Parent;
                }

                #region check job queue
                List<RMJobQueue> jobqueue = RMJobQueueDao.GetMessages(TenantLocalValue.LogonGroupId, checkJobTypes.ToArray());
                foreach (RMJobQueue job in jobqueue)
                {
                    RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(job.Parameters);
                    if (needCheckNode.Level >= (int)NodeLevel.SiteCollection)
                    {
                        if (selectedNode.Level >= (int)NodeLevel.SiteCollection && BelongToObjectSiteCollection(needCheckNode.FullPath, selectedNode.FullPath))
                        {
                            return true;
                        }
                        else if (HasMatchingDisposalRunnableNode(selectedNode, needCheckNode.FullPath))
                        {
                            return true;
                        }
                    }
                    else if (needCheckNode.Level < (int)NodeLevel.SiteCollection && selectedNode.Level < (int)NodeLevel.SiteCollection && needCheckNode.FullPath == selectedNode.FullPath)
                    {
                        return true;
                    }
                }
                #endregion

                #region check job monitor
                List<RMJobMonitor> allSORunningJobs = JMDao.GetRunningJobs(checkJobTypes);
                foreach (RMJobMonitor mainJob in allSORunningJobs)
                {
                    if (needCheckNode.Level < (int)NodeLevel.SiteCollection && mainJob.ContainerId == needCheckNode.Id && mainJob.ScopeId == needCheckNode.FullPath)
                    {
                        List<string> allSubJobIds = SubJobDao.GetAllSubJobIds(mainJob.Id, null);
                        string context = SubJobDao.GetJobContextSettingByJobId(allSubJobIds.First());
                        RMSPTreeNode runningNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(context).First();
                        if (runningNode.IconStatus != IconStatus.Break && runningNode.Level == (int)NodeLevel.SiteCollection)
                        {
                            return true;
                        }
                    }
                    else if (needCheckNode.Level >= (int)NodeLevel.SiteCollection)
                    {
                        List<string> allSubJobScopes = SubJobDao.GetAllSubJobString1sByParentId(mainJob.Id);
                        foreach (string subJobScope in allSubJobScopes)
                        {
                            if (BelongToObjectSiteCollection(needCheckNode.FullPath, subJobScope))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
                return false;
            }
            catch (Exception e)
            {
                logger.Error($@"Fail check temote node have running job, remote node :{selectedTree}, ex:{e}");
                throw;
            }
        }

        private bool HasMatchingDisposalRunnableNode(RMSPTreeNode selectedNode, string targetSiteCollectionUrl)
        {
            foreach (var node in EnumerateDisposalRunnableNodeStream(selectedNode))
            {
                if (BelongToObjectSiteCollection(targetSiteCollectionUrl, node.FullPath))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasMatchingTeamsDisposalRunnableNode(RMSPTreeNode selectedNode, string targetTeamsPath)
        {
            foreach (var node in EnumerateTeamsDisposalRunnableNodeStream(selectedNode))
            {
                if (BelongToObjectTeams(targetTeamsPath, node.FullPath))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckTeamsRemoteNodeHaveRunningJob(RMSPTreeNode selectedTree)
        {
            try
            {
                RMSPTreeNode needCheckNode = selectedTree;
                string nodeFullPath = selectedTree.FullPath;
                string teamsScopePath = selectedTree.GetTeamsNode()?.FullPath ?? nodeFullPath;
                while (needCheckNode.Parent != null && (needCheckNode.Level != (int)NodeLevel.SiteCollection))
                {
                    needCheckNode = needCheckNode.Parent;
                }

                #region check job queue
                List<RMJobQueue> jobqueue = RMJobQueueDao.GetMessages(TenantLocalValue.LogonGroupId, JobType.TeamsArchiverBackup, JobType.SpecifyTeamsArchiverBackup);
                foreach (RMJobQueue job in jobqueue)
                {
                    RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(job.Parameters);
                    if (needCheckNode.Level == (int)NodeLevel.Office365GroupEntire || needCheckNode.Level == (int)NodeLevel.WebApplication)
                    {
                        if ((selectedNode.Level == (int)NodeLevel.Office365GroupEntire || selectedNode.Level == (int)NodeLevel.WebApplication) && BelongToObjectTeams(needCheckNode.FullPath, selectedNode.FullPath))
                        {
                            return true;
                        }
                        else if (HasMatchingTeamsDisposalRunnableNode(selectedNode, needCheckNode.FullPath ?? nodeFullPath))
                        {
                            return true;
                        }
                    }
                    else if (needCheckNode.Level == (int)NodeLevel.WebApplication && selectedNode.Level == (int)NodeLevel.WebApplication && needCheckNode.FullPath == selectedNode.FullPath)
                    {
                        return true;
                    }
                }
                #endregion

                #region check job monitor
                List<RMJobMonitor> allSORunningJobs = JMDao.GetRunningJobs(new List<JobType> { JobType.TeamsArchiverBackup, JobType.SpecifyTeamsArchiverBackup });
                foreach (RMJobMonitor mainJob in allSORunningJobs)
                {
                    if (needCheckNode.Level == (int)NodeLevel.WebApplication && mainJob.ContainerId == needCheckNode.Id && mainJob.ScopeId == needCheckNode.FullPath)
                    {
                        List<string> allSubJobIds = SubJobDao.GetAllSubJobIds(mainJob.Id, null);
                        var firstSubJobId = allSubJobIds.FirstOrDefault();
                        if (firstSubJobId != null)
                        {
                            string context = SubJobDao.GetJobContextSettingByJobId(firstSubJobId);
                            RMSPTreeNode runningNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(context).FirstOrDefault();
                            if (runningNode != null && runningNode.IconStatus != IconStatus.Break && runningNode.Level == (int)NodeLevel.Office365GroupEntire)
                            {
                                return true;
                            }
                        }
                    }
                    else if (needCheckNode.Level != (int)NodeLevel.WebApplication)
                    {
                        List<string> allSubJobScopes = SubJobDao.GetAllSubJobString1sByParentId(mainJob.Id);
                        foreach (string subJobScope in allSubJobScopes)
                        {
                            if (!string.IsNullOrEmpty(subJobScope))
                            {
                                string comparePath = (subJobScope.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                                    || subJobScope.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                    ? (needCheckNode.FullPath ?? nodeFullPath)
                                    : teamsScopePath;
                                if (BelongToObjectTeams(comparePath, subJobScope))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                #endregion
                return false;
            }
            catch (Exception e)
            {
                logger.Error($@"Fail check temote node have running job, remote node :{selectedTree}, ex:{e}");
                throw;
            }
        }

        private bool BelongToObjectSiteCollection(string objectSiteCollectionUrl, string needCheckUrl)
        {
            if (string.IsNullOrWhiteSpace(objectSiteCollectionUrl))
            {
                throw new Exception("source url unable be empty");
            }
            string[] objectSiteCollectionUrlArr = objectSiteCollectionUrl.Split('/');
            string[] needCheckUrlArr = needCheckUrl.Split('/');

            if (objectSiteCollectionUrlArr.Length > needCheckUrlArr.Length)
            {
                return false;
            }

            for (int index = 0; index < objectSiteCollectionUrlArr.Length; index++)
            {
                if (objectSiteCollectionUrlArr[index] != needCheckUrlArr[index])
                {
                    return false;
                }
            }
            return true;
        }

        private bool BelongToObjectTeams(string objectTeamsUrl, string needCheckUrl)
        {
            if (string.IsNullOrWhiteSpace(objectTeamsUrl))
            {
                throw new Exception("source url unable be empty");
            }
            string[] objectTeamsUrlArr = objectTeamsUrl.Split('/');
            string[] needCheckUrlArr = needCheckUrl.Split('/');

            if (objectTeamsUrlArr.Length > needCheckUrlArr.Length)
            {
                return false;
            }

            for (int index = 0; index < objectTeamsUrlArr.Length; index++)
            {
                if (objectTeamsUrlArr[index] != needCheckUrlArr[index])
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<RetentionSettingsDto> GetRetentionSettingsAsync()
        {
            var blobName = GetBlobNameForRetentionSettings();
            var connectionString = RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            var containerName = GetContainerNameForRetentionSettings();
            var result = new RetentionSettingsDto();
            try
            {
                var blobClient = AzureUtil.GetBlobContainerClient(connectionString, containerName).GetBlobClient(blobName);
                var blobProps = (await blobClient.GetPropertiesAsync()).Value;

                result.FileName = await RMKeyValueDao.GetValueByKeyAsync(KeyNameCollection.UploadedCustomRetentionSettingsFileName);
                result.FileSize = Math.Round(blobProps.ContentLength / 1024.0, 2); // Byte -> KB
            }
            catch
            {
                logger.Info(I18NEntity.GetString("RM_Retention_Settings_GetFailed"));
            }
            return result;
        }

        public async Task<Stream> GetCurrentRetentionSettingsFileStream()
        {
            var blobName = GetBlobNameForRetentionSettings();
            var connectionString = RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            var containerName = GetContainerNameForRetentionSettings();
            try
            {
                var blobClient = AzureUtil.GetBlobContainerClient(connectionString, containerName).GetBlobClient(blobName);
                var downloadStreamingResult = await blobClient.DownloadStreamingAsync();
                return downloadStreamingResult.Value.Content;
            }
            catch
            {
                throw new Exception(I18NEntity.GetString("RM_Retention_Settings_GetFailed"));
            }
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.TimerJobSettings, Action = AuditAction.SaveCustomRetentionSettings, BeforeHandler = typeof(CustomRetentionSettingsBeforeAuditHandler), AfterHandler = typeof(CustomRetentionSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveRetentionSettingsAsync(Stream fileStream, string fileName)
        {
            try
            {
                var fileContent = ExcelUtil.ReadExcel(fileStream);
                foreach (var sheet in fileContent.Values)
                {
                    foreach (var row in sheet)
                    {
                        string url = row[0];
                        if (!string.IsNullOrEmpty(url) && !IsValidUrl(url))
                        {
                            return new RAReturnMessage
                            {
                                MessageType = RAMessageType.Failed,
                                FaildType = RAFailedType.UpdateFailed,
                                Extension = string.Empty,
                                ErrorMessage = I18NEntity.GetString("RM_Retention_Settings_Invalid_SiteUrl", url),
                            };
                        }
                    }
                }
            }
            catch
            {
                logger.Warn("Failed to read excel file, try to read as csv.");
                fileStream.Position = 0;
                using (StreamReader sr = new(fileStream, Encoding.UTF8, leaveOpen: true))
                {
                    while (!sr.EndOfStream)
                    {
                        string csvLine = sr.ReadLine();
                        if (csvLine != null)
                        {
                            var siteUrl = CSVHelper.AnalyseCSVRow2Array(csvLine).FirstOrDefault();
                            if (!string.IsNullOrEmpty(siteUrl) && !IsValidUrl(siteUrl))
                            {
                                return new RAReturnMessage
                                {
                                    MessageType = RAMessageType.Failed,
                                    FaildType = RAFailedType.UpdateFailed,
                                    Extension = string.Empty,
                                    ErrorMessage = I18NEntity.GetString("RM_Retention_Settings_Invalid_SiteUrl", siteUrl),
                                };
                            }
                        }
                    }
                }
            }

            try
            {
                var blobName = GetBlobNameForRetentionSettings();
                var connectionString = RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
                var containerName = GetContainerNameForRetentionSettings();
                fileStream.Position = 0;
                await AzureUtil.GetBlobContainerClient(connectionString, containerName, true).GetBlobClient(blobName).UploadAsync(fileStream, true);

                await RMKeyValueDao.SaveOrUpdateAsync(new RMKeyValue { Key = KeyNameCollection.UploadedCustomRetentionSettingsFileName, Value = fileName });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to save retention settings, error: {0}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.UpdateFailed,
                    Extension = string.Empty,
                    ErrorMessage = I18NEntity.GetString("RM_Retention_Settings_SaveFailed"),
                };
            }

            return new RAReturnMessage { MessageType = RAMessageType.Successful };
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.TimerJobSettings, Action = AuditAction.SaveCustomRetentionSettings, BeforeHandler = typeof(CustomRetentionSettingsBeforeAuditHandler), AfterHandler = typeof(CustomRetentionSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> RemoveRetentionSettingsAsync()
        {
            try
            {
                var blobName = GetBlobNameForRetentionSettings();
                var connectionString = RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
                var containerName = GetContainerNameForRetentionSettings();
                await AzureUtil.GetBlobContainerClient(connectionString, containerName, true).GetBlobClient(blobName).DeleteIfExistsAsync();

                RMKeyValueDao.DeleteByKey(KeyNameCollection.UploadedCustomRetentionSettingsFileName);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to remove retention settings, error: {0}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.UpdateFailed,
                    Extension = string.Empty,
                    ErrorMessage = I18NEntity.GetString("RM_Retention_Settings_SaveFailed"),
                };
            }

            return new RAReturnMessage { MessageType = RAMessageType.Successful };
        }

        public async Task<string> GetUploadedCustomRetentionSettingsFileName()
        {
            return await RMKeyValueDao.GetValueByKeyAsync(KeyNameCollection.UploadedCustomRetentionSettingsFileName);
        }

        private string GetBlobNameForRetentionSettings()
        {
            return SecurityUtils.SafeCombinePath(TenantLocalValue.LogonGroupId, "RetentionJobWhiteList", "sites.csv");
        }

        private string GetContainerNameForRetentionSettings()
        {
            return "config";
        }

        /// <summary>
        /// Copy all the rest data in the 'data_archive/DataVolume' to the target storage.
        /// If the item already exists in the target, we need skip it, just move the rest items.
        /// </summary>
        /// <returns></returns>
        public async Task<string> RealRunArchiverFullMoveRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            // Check if feature enabled
            if (!RMKeyValueDao.IsEnableExtendedMoveActionForRetention())
            {
                logger.Warn("EnableExtendedMoveActionForRetention is not enabled, so skip this ArchiverFullMoveRetention job.");
                return null;
            }

            // Param format: "srcStorageId; destStorageId"
            var storageIds = param.Split(';');
            if (storageIds.Length < 2)
            {
                logger.Error($"Invalid storage ids for ArchiverFullMoveRetention job, param: {param}");
                return null;
            }
            string srcStorageId = storageIds[0].Trim();
            string destStorageId = storageIds[1].Trim();
            if (string.IsNullOrEmpty(srcStorageId) || string.IsNullOrEmpty(destStorageId)
                || !Guid.TryParse(srcStorageId, out _) || !Guid.TryParse(destStorageId, out _))
            {
                logger.Error($"Invalid storage ids for ArchiverFullMoveRetention job, param: {param}");
                return null;
            }

            // Check if source and destination storage are the same, if yes, skip this job
            if (srcStorageId.Equals(destStorageId, StringComparison.OrdinalIgnoreCase))
            {
                logger.Error($"Source and destination storage are the same for ArchiverFullMoveRetention job, storageId: {srcStorageId}, skip this job.");
                return null;
            }

            // Check if source and destination storage exist
            var srcStorage = StorageDeviceService.GetStorageDeviceById(srcStorageId, needDecryptSecert: true);
            var destStorage = StorageDeviceService.GetStorageDeviceById(destStorageId, needDecryptSecert: true);
            if (srcStorage is null || destStorage is null)
            {
                logger.Error($"Source or destination storage does not exist, srcStorageId: {srcStorageId}, destStorageId: {destStorageId}");
                return null;
            }

            // Check if there is any archiver index sub job using the source storage, if yes, cannot run full move job
            if (await ArchiverIndexSubInfoDao.CheckIfExistArchiverIndexSubInfoByStorageIdAndSourceFlag(srcStorageId, [(int)SourceFlag.SharePoint, (int)SourceFlag.SharePointOnPrem, (int)SourceFlag.OneDrive]))
            {
                logger.Error($"There are archiver index sub info using source storage {srcStorageId}, cannot run ArchiverFullMoveRetention job.");
                return null;
            }

            var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(srcStorage);
            var destLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(destStorage);

            JobType jobType = JobType.ArchiverFullMoveRetention;
            string jobId = RMJobService.CreateJob(jobType, jobRunByUser);
            SubJobDao.UpdateSubJobCount(jobId, 1);
            logger.Info($"Created {jobType} job with id {jobId} to move orphan data from storage {srcStorageId} to {destStorageId}");

            string subJobId = string.Format(jobId + "_{0:D3}", 0);
            var jobSettings = new ArchiverFullMoveRetentionJobInfo
            {
                SourceDevice = srcLogical,
                DestinationDevice = destLogical,
            };
            var subJob = new RMSubJob
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                Runable = RecordsConstants.SubJob_Runnable_Runing,
                JobContext = new RMJobContext()
                {
                    JobId = subJobId,
                    Settings = SerializerHelper.SerializeByDataContractSerializer(jobSettings)
                }
            };
            SubJobDao.CreateJob(subJob);
            logger.Info($"Created sub job with id {subJobId} for {jobType} job {jobId}");

            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            });
            return jobId;
        }

        public async Task<string> RealRunAPStorageCostEvaluationJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Starting APStorageCostEvaluation job by {jobRunByUser}");
            var keyValue = await RMKeyValueDao.GetValueByKeyAsync(RMKeyValuesConstants.EnableDeleteRestoredDataFeature);
            if (keyValue is null || !bool.TryParse(keyValue, out bool isEnabled) || !isEnabled)
            {
                logger.Info("Delete restored data feature is not enabled. Skipping APStorageCostEvaluation job.");
                return null;
            }

            JobType jobType = JobType.APStorageCostEvaluation;
            string jobId = RMJobService.CreateJob(jobType, jobRunByUser);
            logger.Info($"Created {jobType} job with id {jobId}");
            try
            {
                var storages = await StorageDeviceService.GetAllAvePointStorageAsync();
                SubJobDao.UpdateSubJobCount(jobId, storages.Count);
                const int maxConcurrentSubJobs = 5;
                for (int i = 0; i < storages.Count; i++)
                {
                    var jobSettings = new APStorageCostEvaluationJobInfo
                    {
                        SourceDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storages[i]),
                    };
                    jobSettings.SourceDevice.Id = storages[i].Id;
                    await CreateSubJobForAPStorageCostEvaluationAsync(jobId, i, jobType, storages.Count, i < maxConcurrentSubJobs, jobSettings);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to create sub jobs for APStorageCostEvaluation job {jobId}: {ex}");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_APStorageCostEvaluation_SubJobCreationFailed");
            }

            return jobId;
        }

        private async Task CreateSubJobForAPStorageCostEvaluationAsync(string jobId, int subJobIndex, JobType jobType, int subJobCount, bool runNow, APStorageCostEvaluationJobInfo jobSettings)
        {
            var subJobId = string.Format(jobId + "_{0:D3}", subJobIndex);
            var subJob = new RMSubJob
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                Runable = runNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
                Weight = 100d / subJobCount,
                JobContext = new RMJobContext()
                {
                    JobId = subJobId,
                    Settings = SerializerHelper.SerializeByDataContractSerializer(jobSettings),
                },
            };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessful, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            if (runNow)
            {
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJob.Id,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType, subJob.Id),
                });
            }
        }
    }
}
