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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.Service.SharePointSetting;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Util.Outlook;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.Service.Services.RMSharePointSettings
{
    [Audit]
    public class RMSharePointSettingsService : BaseContentRepositorySettingsService, IRMSharePointSettingsService
    {
        #region All Dao
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();
        private IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private IRMNodeFlagDao RMNodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();

        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private IJobMonitorService RMJobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService<IUniqueIdSettingService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private IRMChangeClassificationDao RMChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();

        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IRMMailboxService MailBoxService => PlatformWindsorManager.GetService<IRMMailboxService>();

        private IRMMailboxDao MailBoxDao => PlatformWindsorManager.GetService<IRMMailboxDao>();

        private IArchiverRuleService ArchiverRuleService => PlatformWindsorManager.GetService<IArchiverRuleService>();

        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ILicenseHelperService licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private DB.Explorer.Dao.IExplorerDao explorerDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        private IRMSettingJobDao RMSettingJobDao = PlatformWindsorManager.GetService<IRMSettingJobDao>();

        private IRMCustomIndexMetadataDao RMCustomIndexMetadataDao = PlatformWindsorManager.GetService<IRMCustomIndexMetadataDao>();

        private IRMCustomMetadataColumnDao RMCustomMetadataColumnDao = PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();
        private static IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private static IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        #endregion

        public Dictionary<string, RMSPTreeNode> cacheNodes = new Dictionary<string, RMSPTreeNode>();

        private const string PROFILEIDFORMAT = "{0}|{1}|{2}";

        private RALogger logger = RALogger.GetInstance(typeof(RMSharePointSettingsService));

        #region Save SharePoint Settings Action to RMDB, Apply Action to run job.
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ConfigureGroupGlobalsetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async System.Threading.Tasks.Task AddGlobalColumnAsync(SaveTreePage setting)
        {
            try
            {
                logger.Info("Set Global SharePoint Setting");
                foreach (var groupNode in setting.allRMSPTreeNode)
                {
                    if (!groupNode.IsUsingExistColumnName)
                    {
                        groupNode.NeedCheckDefaultValue = setting.NeedCheckDefaultVaule;
                        groupNode.ApplyExistType = (int)setting.applyType;
                        groupNode.EnableRelatedRecords = setting.EnableRelatedRecords;
                        //stodo
                        SharePointSettingDao.UpdateBCSColumnName(groupNode.SiteGroupId, groupNode.ColumnName, groupNode.Description, true, false);
                        await SharePointSettingDao.AddOrUpdateGlobalSettingAsync(groupNode);
                    }
                    else
                    {
                        groupNode.EnableRelatedRecords = setting.EnableRelatedRecords;
                        await  SharePointSettingDao.AddOrUpdateGlobalSettingUsingExistColumnAsync(groupNode);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ConfigureCustomSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async System.Threading.Tasks.Task AddCustomColumnAsync(List<RMSPTreeNode> nodes, bool isWebAPI = false, string siteGroupId = null, bool needCheckDefaultVaule = false, int applyType = 0, bool enableRelatedRecords = false)
        {
            try
            {
                logger.Info("Set Custom SharePoint Setting");
                foreach (var customNode in nodes)
                {
                    var settingNode = customNode;
                    RMSPTreeNode siteCollectionNode = null;
                    if (isWebAPI)
                    {
                        settingNode = new RMSharePointColumn().GetCustomSettingsNode(customNode, customNode.FullPath, customNode.BposInfo.UserAccountInfo.Username, customNode.BposInfo.UserAccountInfo.Password);
                    }
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (settingNode.Level == (int)NodeLevel.Folder)
                    {
                        settingNode.FolderId = new Guid(settingNode.SPObjectId);
                        if (isWebAPI)
                        {
                            //settingNode.WebId = settingNode.WebId; //Quality Issue

                            //settingNode.ListId = settingNode.ListId;
                        }
                        else
                        {
                            settingNode.WebId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                            settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id
                        }
                        //RECO-1881
                        settingNode.isEnableClassification = false;
                        settingNode.DescriptionOfContainer = null;
                        settingNode.IsInheritParentTerm = false;
                        settingNode.TermIdOfContainer = Guid.Empty;
                        settingNode.TermNameOfContainer = null;
                    }
                    if (settingNode.Level == (int)NodeLevel.List || settingNode.Level == (int)NodeLevel.Library)
                    {
                        settingNode.ListId = new Guid(settingNode.SPObjectId);
                        if (isWebAPI)
                        {
                            //settingNode.WebId = settingNode.WebId;  //Quality Issue
                        }
                        else
                        {
                            settingNode.WebId = new Guid(settingNode.Parent.Parent.SPObjectId);//set Web Id
                        }
                    }
                    else if (customNode.Level == (int)NodeLevel.Site)
                    {
                        settingNode.WebId = new Guid(settingNode.SPObjectId);
                    }
                    settingNode.NeedCheckDefaultValue = needCheckDefaultVaule;
                    settingNode.ApplyExistType = applyType;
                    settingNode.EnableRelatedRecords = enableRelatedRecords;
                    await SharePointSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
            }
        }

        #region DAta Sync Job

        //[Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public RAReturnMessage RunDataSyncJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();


            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            if (selectedTree != null)
            {
                if (!IsExistCanRunJobNodes(selectedTree))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_EXO_SyncData_NoSC");
                    return msg;
                }
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.DataSynchronisation,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
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

        public RAReturnMessage RunRecordsDisposalJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("Run records disposal Job");

            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            var indexDevice = StorageDeviceService.GetIndexDevice();
            if (indexDevice == null)
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_AR_RunEnforceRuleActionJob_Failed_NoIndexDeviceSetting");
                return msg;
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                //var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.RecordsDisposal,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
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

        public RAReturnMessage RunRebuildStubJob(RebuildStubInfo rebuildStubInfo, JobRunBy jobRunBy)
        {
            logger.Debug("Run Rebuild Stub Job");

            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.RebuildStub,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = rebuildStubInfo == null ? null : SerializerHelper.SerializeByDataContractSerializer(rebuildStubInfo)
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

        public RAReturnMessage RunRebuildIndexJob(string rebuildIndexData, JobRunBy jobRunBy)
        {
            logger.Debug("Run Rebuild Index Job");

            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.RebuildIndex,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = rebuildIndexData
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while sending Rebuild Index job, {ex}");
            }

            return msg;
        }

        /// <summary>
        /// 验证:是否存在可以运行Job的节点
        /// </summary>
        /// <param name="selectedTree"></param>
        /// <returns></returns>
        private bool IsExistCanRunJobNodes(RMSPTreeNode selectedTree)
        {
            if (selectedTree != null)
            {
                if (IsEnableRecordManagement(selectedTree) /*&& IsHaveAvailableNodes(selectedTree)*/)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsEnableRecordManagement(RMSPTreeNode selectedTree)
        {
            Guid siteId = Guid.NewGuid();
            Guid siteGroupId = Guid.NewGuid();
            RMSharePointSetting setting = null;

            //当前只有两个类型的结点可以启动Sync Job: 一类是Group,一类是SiteCollection
            int cnt = 6;
            do
            {
                switch ((NodeLevel)selectedTree.Level)
                {
                    case NodeLevel.WebApplication:
                        {
                            siteId = Guid.Empty;
                            siteGroupId = Guid.Parse(selectedTree.SPObjectId);
                            break;
                        }
                    case NodeLevel.SiteCollection:
                        {
                            siteId = Guid.Parse(selectedTree.SPObjectId);
                            siteGroupId = selectedTree.SiteGroupId;
                            break;
                        }
                }
                setting = SharePointSettingDao.GetSettingInfoByScope(siteGroupId, siteId, Guid.Parse(selectedTree.SPObjectId));
                selectedTree = selectedTree.Parent;
            }
            while (setting == null && selectedTree != null && cnt-- > 0);

            if (setting == null || setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                logger.Info($"IsEnableRecordManagement:setting==null:{setting == null}");
                return false;
            }
            logger.Info($"IsEnableRecordManagement:{true}");
            return true;
        }

       /* private async Task<bool> IsHaveAvailableNodesAsync(RMSPTreeNode selectedTree)
        {
            List<RMSPTreeNode> lstAvailableNodes = await AssembleSyncDataRunnableNodeAsync(selectedTree);
            if (lstAvailableNodes == null || lstAvailableNodes.Count() <= 0)
            {
                return false;
            }
            return true;
        }*/

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunDisposalJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.RecordsDisposal;
            RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
            return RunRecordsDisposalJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);

        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.ApprovalProcessConfig, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public Task<string> RealRunApprovalProcessJobAsync(JobRunBy jobRunBy, string jobRunByUser, List<RMSPTreeNode> nodes, JobType jobType)
        {
            return RunApprovalProcessJobByUrlsAsync(jobRunByUser, jobType, nodes);

        }
        private async Task<string> RunApprovalProcessJobByUrlsAsync(string jobRunByUser, JobType jobType, List<RMSPTreeNode> nodes)
        {
            string jobId = string.Empty;
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            //List<JobType> types = new List<JobType>() { JobType.RMArchiverBackup, JobType.RecordsDisposal, JobType.OneDriveRecordsDisposal };
            var scope = jobType switch
            {
                JobType.RecordsDisposal => "RM_SP_Virtual_Container",
                JobType.OneDriveRecordsDisposal => "RM_OD_Virtual_Container",
                JobType.TeamsRecordsDisposal => "RM_Teams_Virtual_Container",
                _ => throw new Exception($"Not support approval process job with Jobtype is [{jobType}].")
            };
            jobId = RMJobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, scope);
            List<JobType> types = JobTypeConstants.ArchiveSiteConflictType;
            List<RMSPTreeNode> avalibleNodes = nodes.ToList();
            List<string> runningUrls = new List<string>();
            if (jobType == JobType.TeamsRecordsDisposal)
            {
                runningUrls = RMJobMonitorService.GetRunningArchiverJobSiteUrl(types, nodes.Select(node => node.GetTeamsNode().FullPath));
            }
            else
            {
                runningUrls = RMJobMonitorService.GetRunningArchiverJobSiteUrl(types, nodes.Select(node => node.GetSiteCollectionNode().FullPath));
            }
            foreach (var runningUrl in runningUrls)
            {
                foreach (var tempNode in nodes.OrderByDescending(node => node.FullPath.Length))
                {
                    if (RuleSPTreeUtil.IsPrefixWithSlash(runningUrl, tempNode.FullPath) || RuleSPTreeUtil.IsPrefixWithSlash(tempNode.FullPath, runningUrl))
                    {
                        logger.Warn($"not create sub job, current has job running on same scope.{tempNode.FullPath}");
                        avalibleNodes.Remove(tempNode);
                    }
                }
            }


            if (avalibleNodes.Count == 0)
            {
                logger.Warn("No available sc to run for approval process");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
            var mIndexJobs = RMJobMonitorService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                //has move index job, need skip.
                logger.Warn("Current has move index job running.");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            //var scopes = RMJobMonitorService.GetRunningArchiverJobsScopes(types);

            //nodes = nodes.Where(n => !scopes.Contains(n.Name)).ToList();
            //if (nodes.Count == 0)
            //{
            //    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
            //    return jobId;
            //}
            ArchiveJobMonitorExtension jobMonitorExtension = null;
            if (jobType != JobType.TeamsRecordsDisposal)
            {
                jobMonitorExtension = new ArchiveJobMonitorExtension()
                {
                    treeMode = TreeMode.SO,
                    IsGroupLevelArchive = false,
                    SiteUrls = avalibleNodes.Select(n => n.FullPath).ToList()
                };
            }
            else
            {
                jobMonitorExtension = new ArchiveJobMonitorExtension()
                {
                    IsGroupLevelArchive = false,
                    ConflictNodeLevel = ConflictNodeLevel.TeamsApprovalProcessJob,
                    teamsUrls = avalibleNodes.Select(n => n.FullPath).ToList()
                };
            }

            RMJobMonitorService.UpdateJobExtension(jobId, jobMonitorExtension);
            int subJobCount = avalibleNodes.Count;

            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            if (subJobCount > 0 && (jobType == JobType.RecordsDisposal || jobType == JobType.TeamsRecordsDisposal || jobType == JobType.OneDriveRecordsDisposal))
            {
                RMJobMonitorService.SetSumSCCountOfJobExtension(subJobCount, jobId);
                logger.Info("Initialize extension for main job {0} ,estimated site count {1}.", jobId, subJobCount);
            }
            RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetSPORulesForApprovalProcess());
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            if (!IsTrailLicenceAndExceedSizeLimit())
            {
                if (licenseHelperService.HasOpusSOLicense)
                {
                    foreach (RMSPTreeNode site in avalibleNodes)
                    {
                        tempList.Add(site);
                        string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, tempList, false, site.FullPath, site.O365TenantId);
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                else
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_NOSOLicense");
                }
            }
            else
            {
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
            }
            if (JobServiceUtility.SkipMergeDetailsJobs.Contains((int)jobType) && !string.IsNullOrEmpty(jobId))
            {
                try
                {
                    await RMJobMonitorService.UpdateJobVersionAsync(jobId, JobVersion.UnMerged);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while update job version to UnMerged for jobId: {jobId}, error: {ex}");
                }
            }
            return jobId;
        }
        public async Task<string> RunRecordsDisposalJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            List<JobType> types = JobTypeConstants.ArchiveSiteConflictType;

            string nodeUrl = selectedNode.FullPath;
            string folderFullPath = "";
            if (selectedNode.Level == (int)NodeLevel.Folder && !nodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                if (siteNode != null)
                {
                    nodeUrl = WebUtil.MakeFullUrl(selectedNode.GetSiteCollectionNode().FullPath, selectedNode.FullPath);
                    folderFullPath = nodeUrl;
                }
            }
            List<RMSPTreeNode> availableNode = await AssembleDisposalRunnableNodeAsync(selectedNode);
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                jobId = RMJobMonitorService.CreateJobWithScopeId(JobType.RecordsDisposal, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                if (jobType == JobType.RecordsDisposal)
                {
                    RMJobMonitorService.SetSumSCCountOfJobExtension(0, jobId);
                    logger.Info("Initialize extension for main job {0} ,support job run failed.", jobId);
                }
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed,
                    $"RM_SP_NoSiteCollectionUnderGroup{I18NEntity.Separator}{GetSPContainerName(selectedNode)}");
                return jobId;
            }

            var runningUrls = RMJobMonitorService.GetRunningArchiverJobSiteUrl(types, availableNode.Select(n => n.GetSiteCollectionNode().FullPath));
            availableNode = RuleSPTreeUtil.FilterSCAvailableNodeByRunningUrl(availableNode, runningUrls, selectedNode, folderFullPath);
            if (availableNode.Count == 0)
            {
                logger.Warn($"Current has job running on same scope.will skip job");
                jobId = RMJobMonitorService.CreateJobWithScopeId(JobType.RecordsDisposal, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            jobId = RMJobMonitorService.CreateJobWithScopeId(JobType.RecordsDisposal, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode), null, RuleSPTreeUtil.GenerateArchiveJobMonitorExtension(selectedNode, TreeMode.LifeSP));
            try
            {
                List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
                var mIndexJobs = RMJobMonitorService.GetRunningJobs(indexJobTypes);

                if (mIndexJobs.Count > 0)
                {
                    //has move index job, need skip.
                    logger.Warn("Current has move index job running.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }

                RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetSPORules(selectedNode));
            }
            catch(Exception ex)
            {
                logger.Error($"error occurred while check job conflict and add job rule mapping for disposal job, error:{ex}");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }

            UpdateJobVersion(jobId, jobType);
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            if (subJobCount > 0)
            {
                RMJobMonitorService.SetSumSCCountOfJobExtension(subJobCount, jobId);
                logger.Info("Initialize extension for main job {0}, sub job count by selected node level {1}, estimated site count {2}.", jobId, selectedNode.Level, subJobCount);
            }
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            if (!IsTrailLicenceAndExceedSizeLimit())
            {
                if (licenseHelperService.HasOpusSOLicense)
                {
                    foreach (RMSPTreeNode site in availableNode)
                    {
                        tempList.Add(site);
                        string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, tempList, false, site.FullPath, site.O365TenantId);
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                else
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_NOSOLicense");
                }
            }
            else
            {
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
            }
            return jobId;
        }
        
        private bool IsTrailLicenceAndExceedSizeLimit()
        {
            try
            {
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
        private List<Guid> GetSPORules(RMSPTreeNode tree)
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = RuleManagerService.GetRulesFromRecords();
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> spRules = rules.AsQueryable().Where(r => r.SOFilters != null && r.SOFilters.Count != 0).ToList();
            return TermRuleAssociationDao.GetTermWithRuleLevel(tree.Level, spRules).Select(t => t.RuleId).Distinct().ToList();
        }
        private List<Guid> GetSPORulesForApprovalProcess()
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = RuleManagerService.GetRulesFromRecords();
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> spRules = rules.AsQueryable().Where(r => r.SOFilters != null && r.SOFilters.Count != 0).ToList();
            return TermRuleAssociationDao.GetTermWithRuleLevel((int)NodeLevel.SiteCollection, spRules).Select(t => t.RuleId).Distinct().ToList();
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.DataSynchronisation;
            if (string.IsNullOrEmpty(param))
            {
                return RunSPDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
            else
            {
                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
                return RunDataSyncJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }

        private async Task<string> RunDataSyncJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            jobId = RMJobMonitorService.CreateJob(JobType.DataSynchronisation, jobRunByUser, GetSPContainerId(selectedNode));
            List<RMSPTreeNode> availableNode = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
            //remove sites that not changed since last job
            try
            {
            bool noContentModified = false;
            if (availableNode.Count > 1)
            {
                using (var performance = new PerformanceScope("RMSharePointSettingsService.FilterNoContentModifiedSites"))
                {
                    Dictionary<Guid, List<Guid>> termScopeCache = new Dictionary<Guid, List<Guid>>();
                    var modifiedDateCache = GetSiteModifiedDateCache(availableNode);
                    List<string> notIncludeSiteIds = new List<string>();
                    var IsChangedInheritOption = IsHasContainerLevelInheritChanged(selectedNode);
                        if (!IsChangedInheritOption)
                    {
                        logger.Info("Inherit option has not changed, will check content modified date to filter sites.");
                        foreach (var node in availableNode)
                        {
                            if (!NeedCollectSPSite(modifiedDateCache, node, termScopeCache))
                            {
                                notIncludeSiteIds.Add(node.SPObjectId);
                            }
                        }
                        availableNode = availableNode.Where(n => !notIncludeSiteIds.Contains(n.SPObjectId)).ToList();
                        if (availableNode.Count == 0)
                        {
                            noContentModified = true;
                        }
                    }
                }
            }
            if (availableNode.IsNullOrEmpty())
            {
                if (noContentModified)
                {
                    logger.Warn("No content modified under sites.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished);
                }
                else
                {
                    logger.Warn("No available sc to run");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
                }
                return jobId;
            }
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred when run sp data sync job. Error:{ex}");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
           
            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            foreach (RMSPTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }
            return jobId;
        }

        private bool IsHasContainerLevelInheritChanged(RMSPTreeNode selectedNode)
        {
            Guid groupId = Guid.Empty;
            var result = false;
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                if (!string.IsNullOrEmpty(selectedNode.SPObjectId))
                {
                    groupId = new Guid(selectedNode.SPObjectId);
                }
                result = SharePointSettingDao.CheckHasInheritChanged(groupId);
                if (result)
                {
                    SharePointSettingDao.UpdateChangedInheritOptionFlag(groupId);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(selectedNode.SPObjectId))
                {
                    groupId = new Guid(selectedNode.ParentId);
                }
                result = SharePointSettingDao.CheckHasInheritChanged(groupId, new Guid(selectedNode.SPObjectId));
                if (result)
                {
                    SharePointSettingDao.UpdateChangedInheritOptionFlag(groupId, new Guid(selectedNode.SPObjectId));
                }
            }

            return result;
        }

        public void SendDeletionSyncUpgradeJobMessage()
        {
            try
            {
                var count = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.SharePointOnlineDeletionSyncUpgrade);
                if(count > 0)
                {
                    logger.Warn("Deletion sync upgrade job already exists.");
                }

                var queue = new JobQueueDto
                {
                    JobType = JobType.SharePointOnlineDeletionSyncUpgrade,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = null,
                };

                JobQueueService.AddToDBJobQueue(queue);
            }
            catch(Exception e)
            {
                logger.Error($"An error occurrd while run deletion sync upgrade job. Error: {e}");
            }
        }

        public void SendDirtyDataDeleteJobMessage()
        {
            try
            {
                var count = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.CosmosDBDirtyDataDeleteUpgrade);
                if (count > 0)
                {
                    logger.Warn("Deletion sync upgrade job already exists.");
                }

                var queue = new JobQueueDto
                {
                    JobType = JobType.CosmosDBDirtyDataDeleteUpgrade,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = null,
                };

                JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurrd while run deletion sync upgrade job. Error: {e}");
            }
        }

        public string RealRunDeletionSyncUpgradeJob()
        {
            var jobId = string.Empty;
            try
            {
                var username = "RM_TS_RunSchedule";
                var hasRunningJob = RMJobMonitorService.GetRunningJobsCount(JobType.SharePointOnlineDeletionSyncUpgrade) > 0;
                jobId = RMJobMonitorService.CreateJob(JobType.SharePointOnlineDeletionSyncUpgrade, username);
                if (hasRunningJob)
                {
                    logger.Warn("A running upgrade job already exists.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return jobId;
                }

                logger.Info($"Real run upgrade job: [{jobId}]");
                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.SharePointOnlineDeletionSyncUpgrade,
                    CommandLine = $"{JobType.SharePointOnlineDeletionSyncUpgrade} {jobId}",
                });
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while real run upgrade job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        public string RealRunDirtyDataDeleteUpgradeJob()
        {
            var jobId = string.Empty;
            try
            {
                var username = "RM_TS_RunSchedule";
                var hasRunningJob = RMJobMonitorService.GetRunningJobsCount(JobType.CosmosDBDirtyDataDeleteUpgrade) > 0;
                jobId = RMJobMonitorService.CreateJob(JobType.CosmosDBDirtyDataDeleteUpgrade, username);
                if (hasRunningJob)
                {
                    logger.Warn("A running upgrade job already exists.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return jobId;
                }

                logger.Info($"Real run upgrade job: [{jobId}]");
                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.CosmosDBDirtyDataDeleteUpgrade,
                    CommandLine = $"{JobType.CosmosDBDirtyDataDeleteUpgrade} {jobId}",
                });
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while real run upgrade job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        private string GetSPContainerId(RMSPTreeNode selectedNode)
        {
            return TreeNodeUtil.GetSPContainderId(selectedNode);
        }

        private string GetSPContainerName(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return DefaultSecurityContainerNameHelper.GetI18NName(selectedNode.Name);
            }
            else
            {
                return GetSPContainerName(selectedNode.Parent);
            }
        }

        private async Task<List<RMSPTreeNode>> AssembleSyncDataRunnableNodeAsync(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(selectedNode);
                if (sites.IsNullOrEmpty())
                {
                    return availableNode;
                }
                await LoadSPSettingUnderGroupAsync(sites, selectedNode);
                //this.LoadSPSetting(sites);
                foreach (RMSPTreeNode site in sites)
                {
                    if (site.IsSyncData && site.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)//RECO-3282  RECO-3268
                    //if (!site.IsCustomSetting && site.IsSyncData)   //去掉CustomSetting的节点
                    {
                        availableNode.Add(site);
                    }
                }
            }
            else
            {
                if (ValidateSiteExist(selectedNode))
                {
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        private async Task<List<RMSPTreeNode>> AssembleDisposalRunnableNodeAsync(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(selectedNode);
                List<string> mBreakTreeNode = new List<string>();
                if (sites.IsNullOrEmpty())
                {
                    return availableNode;
                }
                var parentId = ScheduleService.GetProfileId(selectedNode) + "|";

                var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                foreach (var item in treeNodes)
                {

                    var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        continue;
                    }
                    mBreakTreeNode.Add(node.FullPath);
                }
                await LoadSPSettingUnderGroupAsync(sites, selectedNode);
                //this.LoadSPSetting(sites);
                foreach (RMSPTreeNode site in sites)
                {
                    if (site.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && !mBreakTreeNode.Contains(site.FullPath))//RECO-3282  RECO-3268
                    //if (!site.IsCustomSetting && site.IsSyncData)   //去掉CustomSetting的节点
                    {
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

        private bool ValidateSiteExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                //DAOAPIClientV1 client = new DAOAPIClientV1();
                //testMailbox = client.GetExchangeNodeById(dbNodeInfo.Id);
                site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, Dictionary<Guid, RMSharePointSetting> gruopSetingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            if (gruopSetingMap != null)
            {
                subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(gruopSetingMap);
            }
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        private string CreateSubJobForDisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, string scope, string o365TenantId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList)};
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            if (JobServiceUtility.NewJobDetailsJobs.Contains((int)jobType))
            {
                using (var progresExecutor = AvePoint.RA.SharePoint.Common.JobExecutionProgress.JobExecutionProgressStatisticExecutor.Instance)
                {
                    logger.Info("Init progress for sub job {0}, type {1}", subJob.Id, subJob.JobType);
                    progresExecutor.InitializeJobExecutionProgressStatictics(subJob.String1, subJob.Id, subJob.ParentId, subJob.JobType);
                }
            }
            return subJobId;
        }
        #endregion


        public RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod)
        {
            logger.Debug("start ApplySettings");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            if (runJobMethod == RunApplySettingMethod.UpdatedScope)
            {
                var updatedScopeCount = 0;
                var settings = SharePointSettingDao.LoadRunJobSetting();
                updatedScopeCount = settings.Count;
                msg.Extension = updatedScopeCount.ToString();
                if (updatedScopeCount == 0)
                {
                    //选择updated scope run job，如果settings count为0直接返回，不起job
                    msg.Extsion1 = I18NEntity.GetString("RM_JS_SPS_NoUpdatedScope");
                    return msg;
                }
                msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobNodes"), updatedScopeCount);
				if (updatedScopeCount == 1)
				{
					msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobSingleNode"), updatedScopeCount);
            }
			}

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ApplySharePointSettings,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0},{1}", fromTimerJobPage, Convert.ToInt32(runJobMethod))
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

        public RAReturnMessage ApplySettingsOnSelectedNode(RMSPTreeNode node)
        {
            logger.Debug("start ApplySettings on selected node, path:{0}", node.FullPath);
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ApplySharePointSettings,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0},{1},{2},{3},{4}", false, Convert.ToInt32(RunApplySettingMethod.SelectedNode), GetTreeNodeScopeId(node), GetTreeNodeSiteId(node), node.FullPath)
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettingsOnSelectedNode,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public bool CheckRunningSharePointSettingJob()
        {
            var runningJobs = RMJobMonitorService.GetRunningSharePointSettingJob();
            return runningJobs.Count > 0;
        }

        private string GetTreeNodeScopeId(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return node.Id;
            }
            else
            {
                return node.SPObjectId.ToString();
            }
        }

        private string GetTreeNodeSiteId(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return Guid.Empty.ToString();
            }
            else
            {
                var siteNode = node.GetSiteCollectionNode();
                return siteNode.SPObjectId;
            }
        }

        public bool NeedRunUniqueIdJob(List<RMSPTreeNode> needRunNodes = null)
        {
            bool result = false;
            try
            {
                var needRunJobNodes = GetNeedRunJobNodes();
                //DAOAPIClientV1 client = new DAOAPIClientV1();
                foreach (var nodeInfo in needRunJobNodes)
                {
                    var setting = CloneSetting(nodeInfo);
                    if (setting.NodeInfo == null)
                    {
                        logger.Info("no change, nodeinfo null.Id:{0}", setting.ScopeId);
                        continue;
                    }
                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);

                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        //var group = client.GetWebApplicationById(node.SPObjectId);
                        var group = RABrowserClient.GetWebApplicationById(node.SPObjectId);
                        if (group == null)
                        {
                            logger.Info($"can not find the group:{node?.FullPath}.");
                            continue;
                        }

                        Guid groupId = Guid.Empty;
                        Guid.TryParse(node.SPObjectId, out groupId);

                        if (ExistsSiteNode(node.SPObjectId) && !RMNodeFlagDao.IsNodeFlagExist(groupId, Guid.Empty, (int)NodeFlagType.UniqueId))
                        {
                            //group存在site节点，并且没有任何一个site节点成功跑过UniqueId job
                            if (needRunNodes != null)
                            {
                                needRunNodes.Add(node);
                            }
                            else
                            {
                                needRunNodes = new List<RMSPTreeNode>();
                                needRunNodes.Add(node);
                            }
                            logger.Info("need run unique id node:{0}", node.FullPath);
                            result = true;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while check unique id,ERROR:{0}", ex.ToString());
            }
            return result;
        }
        private RMSharePointSetting CloneSetting(RMSharePointSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMSharePointSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMSharePointSetting>(xml);
            return result;
        }

        private bool ExistsSiteNode(string groupId)
        {
            try
            {
                var states = new SiteCollectionState[] { SiteCollectionState.AccessAll, SiteCollectionState.AccessSome };
                var siteCollections = RemoteNodeService.GetRemoteSiteCollectionsByParentId(groupId, states);
                return siteCollections.Count > 0;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while ExistsSiteNode, {ex}");
            }
            return false;
        }

        /// <summary>
        /// 包含两部分：1.勾选IsShowUniqueIdSharePoint Online Group的节点, 2: 设置Setting的OneDrive Group节点
        /// </summary>
        /// <returns></returns>
        public List<RMSharePointSetting> GetNeedRunJobNodes()
        {
            var spNodes = SharePointSettingDao.LoadShowUniqueIdSetting();
            var oneDriveNodes = OneDriveSettingDao.LoadShowUniqueIdSetting();
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

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ApplySharePointSetting, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod, string scopeId = null, string siteId = null, string fullPath = null, JobPriority jobPriority = JobPriority.Normal)
        {
            string jobId = string.Empty;
            //起Job，判断是前台起Job还是Schedule起的Job
            List<string> runningJobs = RMJobMonitorService.GetRunningSharePointSettingJob();

            //bool isSkip = runningJobs.Any(j => j != jobId);
            try
            {
                if (runningJobs.Count == 0)
                {
                    jobId = await StartApplySettingJobAsync(jobRunBy, jobRunByUser, JobType.ApplySharePointSettings, runJobMethod, scopeId, siteId, fullPath, jobPriority);
                }
                else
                {
                    //TO DO for skipped jobs, how to set container id?
                    var settings = GetSPSettings(jobRunBy, runJobMethod, scopeId, siteId);
                    if (settings.IsNullOrEmpty())
                    {
                        logger.Warn("No sharepoint setting node found.");
                        throw new Exception("No sharepoint setting node found.");
                    }
                    bool hasAvailableNode = false;
                    foreach (var setting in settings)
                    {
                        RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                        if (node == null)
                        {
                            logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                            continue;
                        }
                        var containerId = GetSPContainerId(node);
                        var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                        if (!(await IsSPAdminAsync(account.UserId)))
                        {
                            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                            {
                                logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                                continue;
                            }
                        }
                        jobId = CreateApplySettingJob(jobRunBy, jobRunByUser, containerId, scopeId, fullPath, jobPriority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        logger.Info(I18NEntity.GetString("RM_SS_JobSkip"));
                        hasAvailableNode = true;
                        break;
                    }
                    if (!hasAvailableNode)
                    {
                        jobId = CreateApplySettingJob(jobRunBy, jobRunByUser, siteId, scopeId, fullPath, jobPriority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SP_NoAvailableNodeError");
                        logger.Warn($"Has no available node for current user. JobId:{jobId}");
                    }
                }
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = CreateApplySettingJob(jobRunBy, jobRunByUser, siteId, scopeId, fullPath, jobPriority);
                }
                if (e.Message == I18NEntity.GetString("RM_SP_NoAvailableSettingError"))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_NoAvailableSettingError");
                }
                else if (e.Message == I18NEntity.GetString("RM_SP_NoInhertSiteError"))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SP_NoInhertSiteError");
                }
                else
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_CreateJobError");
                }


                logger.Error("real run apply sp setting job error: {0}", e.ToString());
            }

            return jobId;
        }

        private List<RMSharePointSetting> GetSPSettings(JobRunBy runBy, RunApplySettingMethod runJobMethod, string scopeId = null, string siteId = null)
        {
            List<RMSharePointSetting> allSettings = null;
            if (runBy == JobRunBy.Control)
            {
                switch (runJobMethod)
                {
                    case RunApplySettingMethod.UpdatedScope:
                        allSettings = SharePointSettingDao.LoadRunJobSetting();
                        break;
                    case RunApplySettingMethod.AllScope:
                        logger.Info("apply full sharepoint setting job");
                        allSettings = SharePointSettingDao.LoadAllSetting();
                        break;
                    case RunApplySettingMethod.Auto:
                        //Part job by node.
                        allSettings = SharePointSettingDao.LoadRunJobSetting();
                        if (allSettings.Count == 0)
                        {
                            logger.Info("apply full sharepoint setting job");
                            allSettings = SharePointSettingDao.LoadAllSetting();
                        }
                        break;
                    case RunApplySettingMethod.SelectedNode:
                        if (string.IsNullOrWhiteSpace(scopeId) || string.IsNullOrWhiteSpace(siteId))
                        {
                            throw new Exception("Scope id or site id is null.");
                        }
                        logger.Info("Apply setting on seleceted node, ScopeId:{0} SiteId:{1}", scopeId, siteId);
                        var webApp = RMRemoteNodeDao.GetWebApplicationById(scopeId);
                        string groupId = string.Empty;
                        if (webApp != null)
                        {
                            groupId = scopeId;
                        }
                        else
                        {
                            var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId);
                            groupId = site?.parentId;
                        }
                        var setting = SharePointSettingDao.GetSettingInfoByScope(new Guid(groupId), new Guid(siteId), new Guid(scopeId));
                        logger.Info("Get setting of seleceted node successfully, exist:{0}", setting != null);
                        if (setting != null)
                        {
                            allSettings = new List<RMSharePointSetting>() { setting };
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Full job
                allSettings = SharePointSettingDao.LoadAllSetting();
            }
            if (allSettings != null)
            {
                logger.Info("Load sp setting finished. Count:{0}", allSettings.Count);
            }
            return allSettings;
        }

        private string CreateApplySettingJob(JobRunBy runBy, string jobRunByUser, string containerId = null, string scopedId = null, string fullPath = null, JobPriority jobPriority = JobPriority.Normal)
        {
            if (!string.IsNullOrEmpty(scopedId)) 
            {
                var node = SharePointSettingDao.LoadSharePointSettingForImportSetting(Guid.Empty, new Guid(scopedId));
                if (node != null && fullPath.StartsWith("/")) 
                {
                    fullPath = node.FullPath;
                }
            }
            string jobId = string.Empty;
            if (runBy == JobRunBy.Control)
            {
                jobId = RMJobMonitorService.CreateJob(JobType.ApplySharePointSettings, jobRunByUser, containerId, scopedId, fullPath);
                logger.Info("Begin control Apply Job {0}", jobId);
            }
            else if (runBy == JobRunBy.Schedule)
            {
                jobId = RMJobMonitorService.CreateJob(JobType.ApplySharePointSettings, "RM_TS_RunSchedule", containerId, scopedId, fullPath);
                logger.Info("Begin schedule Apply Job {0}", jobId);
            }
            else
            {
                jobId = RMJobMonitorService.CreateJob(JobType.ApplySharePointSettings, jobRunByUser, containerId, scopedId, fullPath);
                logger.Info("Begin default Sync Job {0}", jobId);
            }
            if(jobPriority != JobPriority.Normal) JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, jobPriority).GetAwaiter().GetResult();
            return jobId;
        }
        //start setting job by subjob...
        /// <summary>
        /// to go groupby setting by site id
        /// </summary>
        /// <param name="runBy"></param>
        /// <param name="jobRunByUser"></param>
        /// <param name="jobType"></param>
        /// <param name="runJobMethod"></param>
        /// <returns></returns>
        private async Task<string> StartApplySettingJobAsync(JobRunBy runBy, string jobRunByUser, JobType jobType, RunApplySettingMethod runJobMethod, string scopeId = null, string siteId = null,string fullPath = null, JobPriority jobPriority = JobPriority.Normal)
        {
            //Get settings jobs
            //browser tree start sub job..
            //Create sub job detail..
            List<RMSharePointSetting> allSettings = new List<RMSharePointSetting>();
            using (var performance = new PerformanceScope("SPOApplySetting.GetSPSettings"))
            {
                allSettings = GetSPSettings(runBy, runJobMethod, scopeId, siteId);
            }
            string jobId = string.Empty;

            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No sharepoint setting node found.");
                throw new Exception(I18NEntity.GetString("RM_SP_NoAvailableSettingError"));
            }
            Dictionary<Guid, RMSharePointSetting> gruopSetingMap = new Dictionary<Guid, RMSharePointSetting>();
            Dictionary<Guid, int> nodeSettingMap = new Dictionary<Guid, int>();
            List<RMSharePointSetting> excludeSiteNodes = new List<RMSharePointSetting>();
            using (var performance = new PerformanceScope("SPOApplySetting.LoadExcludeSiteCollectionSetting"))
            {
                excludeSiteNodes = SharePointSettingDao.LoadExcludeSiteCollectionSetting();
            }
            List<Guid> ExcludeSiteIds = new List<Guid>();
            List<ValidateNodeInfo> siteStatusCache = new List<ValidateNodeInfo>();
            foreach (var setting in excludeSiteNodes)
            {
                if (setting.SiteId != Guid.Empty)
                {
                    if (!ValidateSiteAvailability(siteStatusCache, setting.SiteId, setting.SiteGroupId))
                    {
                        continue;
                    }
                }
                ExcludeSiteIds.Add(setting.ScopeId);
            }
            Dictionary<Guid, int> applyExistScopes = new Dictionary<Guid, int>();

            //List<SPTreeNodeDto> subJobNodes = new List<SPTreeNodeDto>();
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            Dictionary<Guid, List<RMSPTreeNode>> settingGroup = new Dictionary<Guid, List<RMSPTreeNode>>();
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            Dictionary<string, string> emptyContainers = new Dictionary<string, string>();
            foreach (RMSharePointSetting setting in allSettings)
            {
                if (!ValidateGroupAvailability(siteStatusCache, setting.SiteGroupId))
                {
                    await SharePointSettingDao.SetSettingJobTimeWithGroupIdAsync(setting.SiteGroupId, setting.ScopeId, false, false);
                    continue;
                }
                using (var getRemoteSite = new PerformanceScope("GetRemote", $"GetRemoteSite{setting.SiteId}"))
                {
                    if (setting.SiteId != Guid.Empty)
                    {
                        if (!ValidateSiteAvailability(siteStatusCache, setting.SiteId, setting.SiteGroupId))
                        {
                            await SharePointSettingDao.SetSettingJobTimeWithGroupIdAsync(setting.SiteGroupId, setting.ScopeId, false, false);
                            continue;
                        }
                    }
                }
                RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                if (node == null)
                {
                    logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                    continue;
                }
                //will use common method later
                var containerId = GetSPContainerId(node);
                var isAdmin = await IsSPAdminAsync(account.UserId);
                if (!isAdmin)
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                        continue;
                    }
                }
                List<RMSPTreeNode> nodes = new List<RMSPTreeNode>();
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    using (var initWebApp = new PerformanceScope("InitWebAppSettings", $"InitWebAppSettings{node.Name}"))
                    {
                        List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(node);
                        var totalSiteCount = sites.Count;
                        var hasCustomSiteCount = 0;

                        logger.Info("Group:{0} site collection count is {1}", node.Name, sites.Count);
                        if (sites.Count > 0)
                        {
                            foreach (RMSPTreeNode siteNode in sites)
                            {
                                siteNode.SiteId = new Guid(siteNode.Id);
                                siteNode.EnableLifecycleManagementForSharePointLists = node.EnableLifecycleManagementForSharePointLists;
                                if (ExcludeSiteIds.Contains(new Guid(siteNode.SPObjectId)))
                                {
                                    logger.Info("Exclude SiteId {0}", siteNode.SPObjectId);
                                    hasCustomSiteCount++;
                                }
                                else
                                {
                                    nodes.Add(siteNode);
                                }
                                if (!gruopSetingMap.ContainsKey(new Guid(node.Id)))
                                {
                                    gruopSetingMap.Add(new Guid(node.Id), setting);
                                }
                            }
                        }
                        else
                        {
                            if (!emptyContainers.ContainsKey(containerId))
                            {
                                emptyContainers.Add(containerId, GetSPContainerName(node));
                            }
                        }
                        if (totalSiteCount == hasCustomSiteCount)
                        {
                            //update group node setting
                            //SharePointSettingDao.SetSettingJobTime(new Guid(node.Id), false, false);
                            await SharePointSettingDao.SetSettingJobTimeWithGroupIdAsync(setting.SiteGroupId, setting.ScopeId, false, false);
                        }
                    }
                }
                else
                {
                    node.SiteId = setting.SiteId;
                    nodes.Add(node);
                }
                var isZeroShotMode = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                foreach (var n in nodes)
                {
                    n.PredictionModeType = isZeroShotMode ? PredictionModeType.ZeroShot : PredictionModeType.MLTraining;
                }
                if (nodes.Count > 0)
                {
                    if (settingGroup.ContainsKey(setting.SiteGroupId))
                    {
                        settingGroup[setting.SiteGroupId].AddRange(nodes);
                    }
                    else
                    {
                        settingGroup.Add(setting.SiteGroupId, nodes);
                    }
                }
            }
            if (settingGroup.Count > 0)
            {
                foreach (var group in settingGroup)
                {
                    jobId = CreateApplySettingJob(runBy, jobRunByUser, group.Key.ToString(), scopeId, fullPath, jobPriority);
                    SeperateSubJobForApplySetting(group.Value, gruopSetingMap, jobId, runBy, jobType);

                    #region Store job settings to db.
                    var settingsPerContainer = allSettings.Where(s => s.SiteGroupId == group.Key).ToList();
                    logger.Info("Begin store job setting, JobId: {0}, Site Container: {1} Setting Count: {2}.", jobId, group.Key, settingsPerContainer.Count);
                    var isExist = RMSettingJobDao.GetRMSettingJob(item => item.Id == jobId && item.JobType == (int)jobType) != null;
                    if (!isExist)
                    {
                        RMSettingJobInfo settingJobInfo = new RMSettingJobInfo
                        {
                            Id = jobId,
                            JobType = (int)JobType.ApplySharePointSettings,
                            JobInfos = SerializerHelper.SerializeByDataContractSerializer(settingsPerContainer),
                        };

                        RMSettingJobDao.AddRMSettingJob(settingJobInfo);
                    }
                    logger.Info("Finishing stored job setting, JobId: {0}, Site Container: {1} Setting Count: {2}.", jobId, group.Key ,settingsPerContainer.Count);
                    #endregion
                }
            }
            else
            {
                if (emptyContainers.Count > 0)
                {
                    foreach (var container in emptyContainers)
                    {
                        jobId = CreateApplySettingJob(runBy, jobRunByUser, container.Key,null ,fullPath, jobPriority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, $"RM_SP_NoSiteCollectionUnderGroup{I18NEntity.Separator}{container.Value}");
                    }
                }
                else
                {
                    logger.Warn("No sharepoint setting node group found.");
                    throw new Exception(I18NEntity.GetString("RM_SP_NoInhertSiteError"));
                }
            }
            return jobId;
        }

        private bool ValidateSiteAvailability(List<ValidateNodeInfo> siteStatusCache, Guid siteId, Guid groupId)
        {
            bool isAvailable = true;
            ValidateNodeInfo nodeInfo = new ValidateNodeInfo()
            {
                ScopeId = siteId,
                GroupId = groupId
            };

            if (nodeInfo.NodeExistingInCache(siteStatusCache))
            {
                if (!nodeInfo.NodeIsValid(siteStatusCache))
                {
                    logger.Warn($"Site is null or has been move to other group [{siteId}]. Will not add to exclude list.");
                    isAvailable = false;
                }
            }
            else
            {
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
                if (site == null || !site.parentId.Equals(groupId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (!nodeInfo.NodeExistingInCache(siteStatusCache))
                    {
                        nodeInfo.AddNode2Cache(siteStatusCache);
                    }
                    logger.Warn($"Site is null or has been move to other group [{siteId}]. Will not add to exclude list.");
                    isAvailable = false;
                }
                if (!nodeInfo.NodeExistingInCache(siteStatusCache))
                {
                    nodeInfo.IsValid = true;
                    nodeInfo.AddNode2Cache(siteStatusCache);
                }

            }
            return isAvailable;
        }

        private bool ValidateGroupAvailability(List<ValidateNodeInfo> groupStatusCache, Guid groupId)
        {
            bool isAvailable = true;
            ValidateNodeInfo nodeInfo = new ValidateNodeInfo()
            {
                ScopeId = groupId,
                GroupId = groupId
            };
            if (nodeInfo.NodeExistingInCache(groupStatusCache))
            {
                if (!nodeInfo.NodeIsValid(groupStatusCache))
                {
                    logger.Warn($"Can't find the group: [{groupId}] in database");
                    isAvailable = false;
                }
            }
            else
            {
                var webApp = RMRemoteNodeDao.GetWebApplicationById(groupId.ToString());
                if (webApp == null)
                {
                    if (!nodeInfo.NodeExistingInCache(groupStatusCache))
                    {
                        nodeInfo.AddNode2Cache(groupStatusCache);
                    }
                    logger.Warn($"Can't find the group: [{groupId}] in database.");
                    isAvailable = false;
                }
                else
                {
                    if (webApp.NodeType == RemoveNodeType.SkyDrivePro)
                    {
                        if (!nodeInfo.NodeExistingInCache(groupStatusCache))
                        {
                            nodeInfo.AddNode2Cache(groupStatusCache);
                        }
                        logger.Warn($"Current node is onedrive, will be skipped. Scope id: [{groupId}]");
                        isAvailable = false;
                    }
                    if (RMKeyValueDao.HasUpgradeTeams() && (webApp.NodeType == RemoveNodeType.PrivateChannel || webApp.NodeType == RemoveNodeType.O365GroupSites))
                    {
                        if (!nodeInfo.NodeExistingInCache(groupStatusCache))
                        {
                            nodeInfo.AddNode2Cache(groupStatusCache);
                        }
                        logger.Info($"The account has upgrade teams, Web application is {webApp.NodeType}");
                        isAvailable = false;
                    }
                }
                if (!nodeInfo.NodeExistingInCache(groupStatusCache))
                {
                    nodeInfo.IsValid = true;
                    nodeInfo.AddNode2Cache(groupStatusCache);
                }

            }
            return isAvailable;
        }

        private void SeperateSubJobForApplySetting(List<RMSPTreeNode> availableSites, Dictionary<Guid, RMSharePointSetting> gruopSetingMap, string jobId, JobRunBy runBy, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            Dictionary<string, List<RMSPTreeNode>> dic = this.GroupNodeForSubJob(availableSites);
            var orderDic = dic.OrderBy(a => a.Value.Count);
            Dictionary<int, List<RMSPTreeNode>> subJobNodeDic = new Dictionary<int, List<RMSPTreeNode>>();
            int count = 0;
            foreach (KeyValuePair<string, List<RMSPTreeNode>> pa in orderDic)
            {
                tempList.AddRange(pa.Value);
                if (tempList.Count >= RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    count++;
                    var temp = new List<RMSPTreeNode>();
                    temp.AddRange(tempList);
                    subJobNodeDic.Add(count, temp);
                    tempList.Clear();
                }
            }
            if (tempList.Count > 0)
            {
                count++;
                subJobNodeDic.Add(count, tempList);
            }
            SubJobDao.UpdateSubJobCount(jobId, count);
            logger.Info("Sub job count for [{0}] is [{1}]", jobId, count);
            //int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB : availableSites.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB + 1;
            //SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            using (var subJob = new PerformanceScope("AddSubJob", $"AddSubJob{jobId}:{count}"))
            {
                var isZeroShotMode = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                var extension = new Dictionary<string, string>
                {
                    { "IsZeroShotMode", isZeroShotMode.ToString() }
                };
                foreach (KeyValuePair<int, List<RMSPTreeNode>> pa in subJobNodeDic)
                {

                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, count, pa.Value, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        logger.Debug("Start sub job {0}", subJobId);
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = runBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                            Extension = JsonConvert.SerializeObject(extension)
                        });
                    }
                    currentSubjobIndex++;
                }
            }

            //foreach (RMSPTreeNode site in availableSites)
            //{
            //    tempList.Add(site);
            //    if (tempList.Count == RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB)
            //    {
            //        string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
            //        logger.Debug("Create and queue sub job {0}", subJobId);
            //        if (currentSubjobIndex < subJobCountInConfigFile)
            //        {
            //            mJobQueueService.HandleMessage(new JobQueueMessage()
            //            {
            //                JobId = subJobId,
            //                RunBy = runBy,
            //                JobType = jobType,
            //                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            //            });
            //        }
            //        tempList.Clear();
            //        currentSubjobIndex++;
            //    }
            //}
            //if (tempList.Count > 0)
            //{
            //    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
            //    logger.Debug("Create and queue sub job {0}", subJobId);
            //    if (currentSubjobIndex < subJobCountInConfigFile)
            //    {
            //        mJobQueueService.HandleMessage(new JobQueueMessage()
            //        {
            //            JobId = subJobId,
            //            RunBy = runBy,
            //            JobType = jobType,
            //            CommandLine = string.Format("{0} {1}", jobType, subJobId),
            //        });
            //    }
            //    tempList.Clear();
            //}
        }
        /// <summary>
        /// 分组，保证同一个List一的节点在一个子job里(before Jan2022)
        /// Jan 2022 change to group by site collection.for list schema or term id confiction
        /// </summary>
        /// <param name="treeNodes"></param>
        /// <returns></returns>
        private Dictionary<string, List<RMSPTreeNode>> GroupNodeForSubJob(List<RMSPTreeNode> treeNodes)
        {
            Dictionary<string, List<RMSPTreeNode>> result = new Dictionary<string, List<RMSPTreeNode>>();
            result = treeNodes.GroupBy(t => t.SiteId.ToString()).ToDictionary(group => group.Key, group => group.ToList());
            return result;

        }

        public async System.Threading.Tasks.Task CleanParentNodeSettingAsync(RMSPTreeNode node)
        {
            do
            {
                if (await SharePointSettingDao.CleanSettingJobTimeAsync(node))
                {
                    break;
                }
                node = node.Parent;
            }
            while (node != null);
        }
        #endregion

        #region == For sharepoint settings schedule job ==

        public string RunSharepointSettingsScheduleJob(JobRunBy jobRunBy)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SharePointScheduleSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunSharepointSettingsScheduleJob,ERROR:{0}", ex.ToString());
            }

            return id;
        }





        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ApplySharePointSetting, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealSharepointSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, JobPriority jobPriority = JobPriority.Normal)
        {
            string jobId = string.Empty;

            #region old logic
            //获取节点上正在运行的job 如果有其他运行的job job Skip
            //List<string> runningJobs = RMJobService.GetRunningJobs(JobType.SharePointScheduleSetting, schedule.ProfileId);
            //string workJobId = RMJobService.GetJobIdByJobTypeExceptCurrent(Contract.JobMonitor.JobType.SharePointCustomSetting, jobId);
            //string workGlobalJobId = RMJobService.GetJobIdByJobTypeExceptCurrent(Contract.JobMonitor.JobType.SharePointGlobalSetting, jobId);
            //string workCustomJobId = RMJobService.GetJobIdByJobTypeExceptCurrent(Contract.JobMonitor.JobType.SharePointInheritSetting, jobId);
            //string scheduleJobId = RMJobService.GetJobIdByJobTypeExceptCurrent(Contract.JobMonitor.JobType.SharePointScheduleSetting, jobId);
            #endregion
            List<string> runningJobs = RMJobMonitorService.GetRunningSharePointSettingJob();

            //bool isSkip = runningJobs.Any(j => j != jobId);
            if (runningJobs.Count == 0)
            {
                //StartSettingsJob(JobType.SharePointScheduleSetting, jobId, jobRunBy);
                jobId = await StartApplySettingJobAsync(jobRunBy, jobRunByUser, JobType.SharePointScheduleSetting, RunApplySettingMethod.Auto, null, null, null, jobPriority);
            }
            else
            {
                jobId = RMJobMonitorService.CreateJob(Contract.JobMonitor.JobType.SharePointScheduleSetting, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);
                if(jobPriority != JobPriority.Normal) await JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, jobPriority);
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                //StartSettingsJob(JobType.SharePointScheduleSetting, jobId);
                logger.Info("CustomSetting job or GlobalSetting job or InheritSetting job has job running,so shedule job is skip");
            }

            return jobId;
        }
        /*private void StartSettingsJob(JobType jobType, string jobId, JobRunBy runBy)
        {

            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = jobType,
                RunBy = runBy,
                CommandLine = string.Format("{0} {1}", jobType, jobId),
            });
        }*/
        //private void StartSettingsJob(JobType jobType, string jobId, JobSettings jobSettings)
        //{
        //    string jobInfo = SerializerHelper.SerializeByDataContractSerializer(jobSettings);
        //    string jobInfoInBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jobInfo), Base64FormattingOptions.None);
        //    var info = new RMSettingJobInfo();
        //    info.Id = jobId;
        //    info.JobInfos = jobInfoInBase64;
        //    info.JobType = (int)jobType;
        //    //JobSettings ss = SerializerHelper.DeserializeByDataContractSerializer<JobSettings>(jobInfo);
        //    if (RMSettingJobDao.AddRMSettingJob(info))
        //    {
        //        mJobQueueService.HandleMessage(new JobQueueMessage()
        //        {
        //            JobId = jobId,
        //            JobType = jobType,
        //            CommandLine = string.Format("{0} {1}", jobType, jobId),
        //        });
        //    }
        //}
        #endregion

        #region ==========Records Web  method ============================
        public bool IsUseExistingColumn(List<Guid> groupSpObjectIds)
        {
            return SharePointSettingDao.IsUsingExistingColumnByGroupIds(groupSpObjectIds);
        }
        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetSPListNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
            }
            return node;
        }
        /// <summary>
        /// 判断是否为后代Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ancestorId">祖先节点的ID</param>
        /// <returns></returns>
        public bool CheckIsDescendantsNode(RMSPTreeNode node, string ancestorId)
        {
            while (node != null)
            {
                if (node.ParentId == ancestorId)
                {
                    return true;
                }
                else
                {
                    node = node.Parent;//TODO
                }
            }
            return false;
        }
        public RMSPTreeNode GetGroupNode(RMSPTreeNode node)
        {
            if (node.Level != (int)NodeLevel.WebApplication)
            {
                while (node.Level != (int)NodeLevel.SiteCollection)
                {
                    node = node.Parent;
                }
                return node.Parent;
            }
            else
            {
                return node;
            }
        }
        public RMSPTreeNode GetListNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetWebNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.Site)
            {
                node = node.Parent;
            }
            return node;
        }
        

        public async System.Threading.Tasks.Task LoadSPSettingAsync(List<RMSPTreeNode> nodes)
        {
            try
            {
                foreach (var node in nodes)
                {
                    bool ownSetting = true;
                    var groupNode = GetGroupNode(node);
                    Guid groupId = Guid.Empty;
                    string GlobalColumnName = string.Empty;
                    bool folderDisable = false;
                    if (groupNode != null)
                    {
                        groupId = new Guid(groupNode.SPObjectId);
                    }
                    var GSetting = SharePointSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                    if (GSetting != null)
                    {
                        GlobalColumnName = GSetting.ColumnName;
                        var termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);

                        node.ColumnName = GlobalColumnName;
                        node.ExistColumnName = GSetting.ExistColumnName;
                        node.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                        node.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                        node.TermSetName = GSetting.TermSetName;
                        node.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                        node.DefaultTermNameFullPath = termScope == null ? GSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                        node.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                        node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePoint);
                        node.IsDefaultTermRemoved = termScope == null ? false : termScope.IsRemoved;
                        node.IsDefaultTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                        node.isFailedConfigClassification = GSetting.isFailedConfigClassification;
                        node.isFailedConfigMetaDataColumn = GSetting.isFailedConfigMetaDataColumn;
                        node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                        node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                        node.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                        node.EnableRecordManagement = GSetting.EnableRecordManagement;
                        node.isEnableClassification = GSetting.isEnableClassification;
                        node.IsSyncData = GSetting.IsSyncData;
                    }
                    var siteNode = GetSiteCollectionNode(node);
                    Guid siteId = Guid.Empty;
                    if (siteNode != null)
                    {
                        siteId = new Guid(siteNode.SPObjectId);
                    }
                    var SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
                    if (SPSetting != null && (SPSetting.TermIdOfContainer != Guid.Empty || SPSetting.TermId != Guid.Empty || SPSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable))
                    {
                        node.HasCustomSetting = true;
                    }
                    else
                    {
                        node.HasCustomSetting = false;
                    }

                    if (SPSetting != null)
                    {
                        node.IsCustomSetting = true;
                    }
                    if (node.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                    {
                        var pNode = LoadFolderParentSeting(node, siteId);
                        if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                        {
                            if (SPSetting != null)
                            {
                                SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                            folderDisable = true;
                        }
                    }

                    if (SPSetting == null)
                    {
                        if (node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.Folder)
                        {
                            SPSetting = LoadParentSeting(node.Parent, siteId);
                            if (SPSetting != null && node.Level != (int)NodeLevel.WebApplication)
                            {
                                if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                                {
                                    SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                                }

                            }
                        }
                    }
                    //else
                    //{
                    //    node.IsCustomSetting = true;
                    //}



                    if (SPSetting != null)
                    {
                        var termScope = TermDao.GetRMTermByGuId(SPSetting.TermId);
                        var defaultTerm = TermDao.GetRMTermByGuId(SPSetting.DefaultTermId);
                        var containerTerm = TermDao.GetRMTermByGuId(SPSetting.TermIdOfContainer);

                        node.ColumnName = GlobalColumnName;
                        node.Description = SPSetting.Description;
                        node.DefaultTermId = SPSetting.DefaultTermId;
                        node.DefaultTermName = defaultTerm == null ? SPSetting.DefaultTermName : defaultTerm.Name;
                        node.DefaultTermNameFullPath = defaultTerm == null ? SPSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(SPSetting.DefaultTermId);
                        node.TermId = SPSetting.TermId;
                        node.TermName = termScope == null ? SPSetting.TermName : termScope.Name;
                        node.TermNameFullPath = termScope == null ? SPSetting.TermName : TermDao.GetTermFullPathByTermId(SPSetting.TermId);
                        node.TermSetId = SPSetting.TermSetId;
                        node.TermSetName = SPSetting.TermSetName;
                        node.IsTermRemoved = termScope == null ? false : termScope.IsRemoved;
                        node.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                        node.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                        node.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                        node.DescriptionOfContainer = SPSetting.DescriptionOfContainer;
                        node.IsInheritParentTerm = SPSetting.IsInheritParentTerm;
                        node.TermIdOfContainer = SPSetting.TermIdOfContainer;
                        node.TermNameOfContainer = containerTerm == null ? SPSetting.TermNameOfContainer : containerTerm.Name;
                        node.isEnableClassification = SPSetting.isEnableClassification;
                        node.EnableRecordManagement = SPSetting.EnableRecordManagement;
                        node.IsEnableHoldPhyical = SPSetting.IsEnableHoldPhyical;
                        node.isFailedConfigClassification = SPSetting.isFailedConfigClassification;
                        node.isFailedConfigMetaDataColumn = SPSetting.isFailedConfigMetaDataColumn;
                        node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                        node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                        node.ExistColumnName = SPSetting.ExistColumnName;
                        node.IsUsingExistColumnName = SPSetting.IsUsingExistColumnName;
                        node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(SPSetting.Id, RecordOwnerSettingType.SharePoint);
                        node.EMailToRecordOwner = SPSetting.EMailToRecordOwner;
                        node.IsDisplyaTermPath = SPSetting.IsDisplyaTermPath;
                        node.EnableRelatedRecords = SPSetting.EnableRelatedRecords;
                        node.IsSyncData = SPSetting.IsSyncData;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
        }

        public async System.Threading.Tasks.Task LoadSPSettingUnderGroupAsync(List<RMSPTreeNode> nodes, RMSPTreeNode groupNode)
        {
            try
            {
                logger.Info($"Begin to load sp settings for group:{groupNode.FullPath} Site collection count:{nodes.Count}");
                using (var performance = new PerformanceScope("RMSharePointSettingsService.LoadSPSettingUnderGroup"))
                {
                    Guid groupId = Guid.Empty;
                    if (groupNode != null)
                    {
                        groupId = new Guid(groupNode.SPObjectId);
                    }
                    var GSetting = SharePointSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                    string GlobalColumnName = string.Empty;
                    RMTerm termScope = null;
                    RMTerm containerTerm = null;
                    string groupTermFullPath = string.Empty;
                    bool groupTermExpired = false;
                    bool groupContainerTermExpired = false;
                    List<ToUserInfo> groupRecordOwner = null;
                    if (GSetting != null)
                    {
                        termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);
                        GlobalColumnName = GSetting.ColumnName;
                        if (termScope != null)
                        {
                            groupTermFullPath = TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                            groupTermExpired = TermDao.IsExpiredTerm(termScope.Id);
                        }
                        if (containerTerm != null)
                        {
                            groupContainerTermExpired = TermDao.IsExpiredTerm(containerTerm.Id);
                        }
                        groupRecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePoint);
                    }
                    List<RMSharePointSetting> siteSettings;
                    using (var performance0 = new PerformanceScope("RMSharePointSettingsService.LoadSharePointSettings"))
                    {
                        siteSettings = SharePointSettingDao.LoadSharePointSettings(groupId, true);
                    }
                    foreach (var node in nodes)
                    {
                        ArgumentCheck.NotNull(node, nameof(node));
                        var siteNode = node;
                        Guid siteId = Guid.Empty;
                        if (siteNode != null)
                        {
                            siteId = new Guid(siteNode.SPObjectId);
                        }
                        var SPSetting = siteSettings.Where(s => s.ScopeId == siteId && s.SiteId == siteId).FirstOrDefault();
                        //SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
                        if (SPSetting == null)
                        {
                            if (GSetting != null)
                            {
                                node.ColumnName = GlobalColumnName;
                                node.ExistColumnName = GSetting.ExistColumnName;
                                node.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                                node.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                                node.TermSetName = GSetting.TermSetName;
                                node.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                                node.DefaultTermNameFullPath = termScope == null ? GSetting.DefaultTermName : groupTermFullPath;
                                node.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                                node.RecordOwner = groupRecordOwner;
                                node.IsDefaultTermRemoved = termScope == null ? false : termScope.IsRemoved;
                                node.IsDefaultTermDeprecated = termScope == null ? false : termScope.IsDeprecated || groupTermExpired;
                                node.isFailedConfigClassification = GSetting.isFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = GSetting.isFailedConfigMetaDataColumn;
                                node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                                node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || groupContainerTermExpired;
                                node.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                                node.EnableRecordManagement = GSetting.EnableRecordManagement;
                                node.isEnableClassification = GSetting.isEnableClassification;
                                node.IsSyncData = GSetting.IsSyncData;
                                node.ApprovalType = (int)GSetting.ApprovalType;
                                RMSPTreeNode rMSPTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(GSetting.NodeInfo);
                                node.SupportLockedSite = rMSPTreeNode.SupportLockedSite;
                                node.EnableLifecycleManagementForSharePointLists = rMSPTreeNode.EnableLifecycleManagementForSharePointLists;
                            }
                        }
                        else
                        {
                            if (SPSetting != null && (SPSetting.TermIdOfContainer != Guid.Empty || SPSetting.TermId != Guid.Empty || SPSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable))
                            {
                                node.HasCustomSetting = true;
                            }
                            else
                            {
                                node.HasCustomSetting = false;
                            }

                            if (SPSetting != null)
                            {
                                node.IsCustomSetting = true;
                            }
                            //if (node.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                            //{
                            //    var pNode = LoadFolderParentSeting(node, siteId);
                            //    if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                            //    {
                            //        if (SPSetting != null)
                            //        {
                            //            SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            //        }
                            //        folderDisable = true;
                            //    }
                            //}

                            //if (SPSetting == null)
                            //{
                            //    if (node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.Folder)
                            //    {
                            //        SPSetting = LoadParentSeting(node.Parent, siteId);
                            //        if (SPSetting != null && node.Level != (int)NodeLevel.WebApplication)
                            //        {
                            //            if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                            //            {
                            //                SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            //            }

                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    node.IsCustomSetting = true;
                            //}



                            if (SPSetting != null)
                            {
                                var siteTermScope = TermDao.GetRMTermByGuId(SPSetting.TermId);
                                var siteDefaultTerm = TermDao.GetRMTermByGuId(SPSetting.DefaultTermId);
                                var siteContainerTerm = TermDao.GetRMTermByGuId(SPSetting.TermIdOfContainer);

                                RMSPTreeNode rMSPTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(SPSetting.NodeInfo);
                                node.SupportLockedSite = rMSPTreeNode.SupportLockedSite;
                                node.EnableLifecycleManagementForSharePointLists = rMSPTreeNode.EnableLifecycleManagementForSharePointLists;

                                node.ColumnName = GlobalColumnName;
                                node.Description = SPSetting.Description;
                                node.DefaultTermId = SPSetting.DefaultTermId;
                                node.DefaultTermName = siteDefaultTerm == null ? SPSetting.DefaultTermName : siteDefaultTerm.Name;
                                node.DefaultTermNameFullPath = siteDefaultTerm == null ? SPSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(SPSetting.DefaultTermId);
                                node.TermId = SPSetting.TermId;
                                node.TermName = siteTermScope == null ? SPSetting.TermName : siteTermScope.Name;
                                node.TermNameFullPath = siteTermScope == null ? SPSetting.TermName : TermDao.GetTermFullPathByTermId(SPSetting.TermId);
                                node.TermSetId = SPSetting.TermSetId;
                                node.TermSetName = SPSetting.TermSetName;
                                node.IsTermRemoved = siteTermScope == null ? false : siteTermScope.IsRemoved;
                                node.IsDefaultTermRemoved = siteDefaultTerm == null ? false : siteDefaultTerm.IsRemoved;
                                node.IsTermDeprecated = siteTermScope == null ? false : siteTermScope.IsDeprecated || TermDao.IsExpiredTerm(siteTermScope.Id);
                                node.IsDefaultTermDeprecated = siteDefaultTerm == null ? false : siteDefaultTerm.IsDeprecated || TermDao.IsExpiredTerm(siteDefaultTerm.Id);
                                node.DescriptionOfContainer = SPSetting.DescriptionOfContainer;
                                node.IsInheritParentTerm = SPSetting.IsInheritParentTerm;
                                node.TermIdOfContainer = SPSetting.TermIdOfContainer;
                                node.TermNameOfContainer = siteContainerTerm == null ? SPSetting.TermNameOfContainer : siteContainerTerm.Name;
                                node.isEnableClassification = SPSetting.isEnableClassification;
                                node.EnableRecordManagement = SPSetting.EnableRecordManagement;
                                node.IsEnableHoldPhyical = SPSetting.IsEnableHoldPhyical;
                                node.isFailedConfigClassification = SPSetting.isFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = SPSetting.isFailedConfigMetaDataColumn;
                                node.IsClassificationTermRemoved = siteContainerTerm == null ? false : siteContainerTerm.IsRemoved;
                                node.IsClassificationTermDeprecated = siteContainerTerm == null ? false : siteContainerTerm.IsDeprecated || TermDao.IsExpiredTerm(siteContainerTerm.Id);
                                node.ExistColumnName = SPSetting.ExistColumnName;
                                node.IsUsingExistColumnName = SPSetting.IsUsingExistColumnName;
                                node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(SPSetting.Id, RecordOwnerSettingType.SharePoint);
                                node.EMailToRecordOwner = SPSetting.EMailToRecordOwner;
                                node.IsDisplyaTermPath = SPSetting.IsDisplyaTermPath;
                                node.EnableRelatedRecords = SPSetting.EnableRelatedRecords;
                                node.IsSyncData = SPSetting.IsSyncData;
                                node.ApprovalType = (int)SPSetting.ApprovalType;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
        }

        public RMSharePointSetting LoadParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMSharePointSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }

        public RMSharePointSetting LoadSampleNodeParentSeting(RMSPSampleTreeNode node, Guid siteId)
        {
            RMSharePointSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadSampleNodeParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }
        //public RMSharePointSetting LoadFolderParentSeting(SPTreeNodeDto node, Guid siteId)
        //{
        //    RMSharePointSetting SPSetting = null;

        //    if (node.Level == NodeLevel.WebApplication)
        //    {
        //        SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), Guid.Empty, true);
        //        return SPSetting;
        //    }

        //    if (node.Level == NodeLevel.SiteCollection || node.Level == NodeLevel.Site || node.Level == NodeLevel.List)
        //    {
        //        SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
        //    }

        //    if (SPSetting == null)
        //    {
        //        SPSetting = LoadFolderParentSeting(node.Parent, siteId);
        //        if (SPSetting != null)
        //        {
        //            if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
        //            {
        //                SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
        //            }

        //        }
        //    }

        //    return SPSetting;
        //}

        public RMSharePointSetting LoadFolderParentSeting(RMSPSampleTreeNode node, Guid siteId)
        {
            RMSharePointSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), Guid.Empty, true);
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadFolderParentSeting(node.Parent, siteId);
                if (SPSetting != null)
                {
                    if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                    }

                }
            }

            return SPSetting;
        }
        public RMSharePointSetting LoadFolderParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMSharePointSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), Guid.Empty, true);
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, true);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadFolderParentSeting(node.Parent, siteId);
                if (SPSetting != null)
                {
                    if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                    }

                }
            }

            return SPSetting;
        }
        public async System.Threading.Tasks.Task LoadScheduleAsync(List<RMSPTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                var groupNode = GetGroupNode(node);
                Guid groupId = groupNode != null ? Guid.Parse(groupNode.SPObjectId) : Guid.Empty;

                var siteNode = GetSiteCollectionNode(node);
                Guid siteId = siteNode != null ? Guid.Parse(siteNode.SPObjectId) : Guid.Empty;

                var idStr = GetParentWebIds(node);
                string parentId = !string.IsNullOrEmpty(idStr) ? idStr + "|" : string.Empty;

                var profileId = string.Format(PROFILEIDFORMAT, groupId, siteId, parentId + node.SPObjectId);
                var schedule = await ScheduleService.GetScheduleByProfileIdAsync(profileId);
                node.ScheduleInfo = schedule;

            }
        }

        public void CheckIsContainScheduleForOwnAndChildNodes(List<RMSPTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                var groupNode = GetGroupNode(node);
                var groupId = groupNode != null ? groupNode.SPObjectId : string.Empty;
                node.IsContainScheduleForOwnAndChildNodes = ScheduleService.CheckIsContainScheduleForOwnAndChildNodes(node.SPObjectId, groupId);
            }
        }

        private string GetParentWebIds(RMSPTreeNode node)
        {
            string result = string.Empty;

            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
                if (node != null && node.Level == (int)NodeLevel.Site)
                {
                    result = result == "" ? node.SPObjectId : node.SPObjectId + "|" + result;
                }
            }
            return result;
        }

        public string GetMetadataColumn(Guid nodeId)
        {
            return SharePointSettingDao.GetMedataColumn(nodeId);
        }
        public List<string> GetDesignLists()
        {
            bool isCSDTenant = TenantService.IsCSDTenant();
            return WebUtil.GetDesignLists(isCSDTenant);
        }
        private bool CheckIsDesignList(RMSPTreeNode list)
        {
            var listInfo = list.FullPath.Substring(list.FullPath.LastIndexOf('/') + 1) + ((int)list.TemplateId).ToString();
            bool isDesignList = false;
            try
            {
                if (this.designLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Check is DesignList error {0}", ex.ToString());
            }
            return isDesignList;
        }
        #endregion

        #region SPS Dirty Data
        private List<string> designLists = null;

        private Dictionary<string, bool> underSiteCollSettingsNeedRemoveForSPS = new Dictionary<string, bool>();
        public async System.Threading.Tasks.Task CheckDirtyDataAsync()
        {
            logger.Info("Check Dirty Data start, current user is :{0}", TenantLocalValue.LogonUserEmail);
            //CheckDirtyDisposalData();//xwwang todo RECO-2467
            await CheckDirtySPSDataAsync();
            await CheckDirtyEXOSDataAsync();
            logger.Info("Check Dirty Data end, current user is :{0}", TenantLocalValue.LogonUserEmail);
        }
        public async System.Threading.Tasks.Task CheckDirtySPSDataAsync()
        {
            designLists = GetDesignLists();
            var upperSiteCollSettings = new List<RMSPTreeNode>();

            var allSettings = SharePointSettingDao.LoadAllSetting();
            foreach (var setting in allSettings)
            {
                var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                if (node.Level > (int)NodeLevel.SiteCollection)
                {
                    var scNode = GetSiteCollectionNode(node);
                    if (!upperSiteCollSettings.Any(s => s.SPObjectId == scNode.SPObjectId))
                    {
                        upperSiteCollSettings.Add(scNode);
                    }
                    underSiteCollSettingsNeedRemoveForSPS[node.FullPath + "|" + node.SPObjectId] = true;
                }
                else
                {
                    upperSiteCollSettings.Add(node);
                }
            }
            logger.Debug("SPSetting upper site coll level node: {0}", string.Join(",", upperSiteCollSettings.Select(s => s.FullPath).ToList()));
            foreach (var node in upperSiteCollSettings)
            {
                switch (node.Level)
                {
                    case (int)NodeLevel.WebApplication:
                        #region global
                        try
                        {
                            //DAOAPIClientV1 client = new DAOAPIClientV1();
                            //RemoteWebApplication webApp = client.GetWebApplicationById(node.Id);
                            RemoteWebApplication webApp = RABrowserClient.GetWebApplicationById(node.Id);
                            if (webApp == null)
                            {
                                logger.Info("Group was removed in DAO Register Groups {0}", node.FullPath);
                                SharePointSettingDao.MarkRemovedSharePointSetting(new Guid(node.SPObjectId));
                                var siteGroupId = node.SiteGroupId;
                                await SharePointSettingDao.MarkRemovedSharePointSettingUnderCurrentAsync(s => s.SiteGroupId == siteGroupId && !s.IsRemoved);
                            }
                            else
                            {
                                await CheckSPSSubNodeSettingsAsync(node);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                        break;
                    #endregion
                    case (int)NodeLevel.SiteCollection:
                        #region site collection
                        try
                        {
                            //var client = new DAOAPIClientV1();
                            //var testSite = client.GetRemoteSiteCollectionByUrl(node.FullPath);
                            var testSite = RABrowserClient.GetRemoteSiteCollectionByUrl(node.FullPath);
                            if (testSite == null || !testSite.parentId.Equals(GetGroupNode(node).SPObjectId, StringComparison.OrdinalIgnoreCase))
                            {
                                SharePointSettingDao.MarkRemovedSharePointSetting(new Guid(node.SPObjectId));
                                var fullPath = node.FullPath + "/";
                                await SharePointSettingDao.MarkRemovedSharePointSettingUnderCurrentAsync(s => s.FullPath.StartsWith(fullPath) && !s.IsRemoved);
                            }
                            else
                            {
                                await CheckSPSSubNodeSettingsAsync(node);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                        break;
                    #endregion
                    default:
                        logger.Warn("node is {0},node level is{1}", node.FullPath, ((NodeLevel)node.Level).ToString());
                        break;
                }
            }
            logger.Info("SPSetting node need remove without browse {0}", string.Join(",", underSiteCollSettingsNeedRemoveForSPS.Where(s => s.Value).Select(s => s.Key)));
            foreach (var item in underSiteCollSettingsNeedRemoveForSPS.Where(s => s.Value))
            {
                try
                {
                    SharePointSettingDao.MarkRemovedSharePointSetting(allSettings.Where(s => s.FullPath + "|" + s.ScopeId == item.Key).Select(s => s.ScopeId).First());
                }
                catch (Exception)
                {
                    logger.Error("delete node error {0}", item.Key);
                }
            }
        }

        public async System.Threading.Tasks.Task CheckSPSSubNodeSettingsAsync(RMSPTreeNode node)//node level is Site or List.
        {
            //node是upperSiteCollSettings中的 WebApplication 属于终止级别，只进行children的比对
            if (node.Level == (int)NodeLevel.WebApplication)
            {
                logger.Debug("current node is {0}", !string.IsNullOrEmpty(node.FullPath) ? node.FullPath : node.Name);
                await DeleteDirtySPSDataAsync(node, await RMSPTreeService.BrowseAsync(node));
            }
            else if (node.Level == (int)NodeLevel.SiteCollection
                || node.Level == (int)NodeLevel.Site
                || node.Level == (int)NodeLevel.Lists
                  || node.Level == (int)NodeLevel.Sites
                  || node.Level == (int)NodeLevel.List
                  || node.Level == (int)NodeLevel.RootFolder
                  || node.Level == (int)NodeLevel.Folders
                  || node.Level == (int)NodeLevel.Folder)
            {
                if (node.Level == (int)NodeLevel.List)
                {
                    if (node.Hidden)
                    {
                        return;
                    }
                    if (CheckIsDesignList(node))
                    {
                        return;
                    }
                    //check design list
                }
                logger.Debug("current node is {0}", !string.IsNullOrEmpty(node.FullPath) ? node.FullPath : node.Name);
                var children = await RMSPTreeService.BrowseAsync(node);

                await DeleteDirtySPSDataAsync(node, children);
                //for vir node.
                foreach (var subNode in children)
                {
                    await CheckSPSSubNodeSettingsAsync(subNode);
                }
            }
        }

        public async System.Threading.Tasks.Task DeleteDirtySPSDataAsync(RMSPTreeNode current, List<RMSPTreeNode> children)
        {
            foreach (var child in children)
            {
                if (child.Level == (int)NodeLevel.Folder)
                {
                    child.FullPath = WebUtil.MakeFullUrl(GetSiteCollectionNode(child).FullPath, child.FullPath);
                }

                if (underSiteCollSettingsNeedRemoveForSPS.ContainsKey(child.FullPath + "|" + child.SPObjectId))
                {
                    underSiteCollSettingsNeedRemoveForSPS[child.FullPath + "|" + child.SPObjectId] = false;
                }
            }
            NodeLevel findLevel = NodeLevel.Undefined;
            try
            {
                switch ((NodeLevel)current.Level)
                {
                    case NodeLevel.Farm:
                        findLevel = NodeLevel.WebApplication;
                        break;
                    case NodeLevel.WebApplication:
                        findLevel = NodeLevel.SiteCollection;
                        break;
                    case NodeLevel.SiteCollection:
                    case NodeLevel.Site:
                        break;
                    case NodeLevel.Lists:
                        findLevel = NodeLevel.List;
                        break;
                    case NodeLevel.Sites:
                        findLevel = NodeLevel.Site;
                        break;
                    case NodeLevel.List:
                    case NodeLevel.Library:
                        break;
                    case NodeLevel.Folders:
                        findLevel = NodeLevel.Folder;
                        break;
                }
                if (findLevel == NodeLevel.Folder)
                {
                    var parent = current.Parent;
                    if (parent.FullPath.StartsWith("/"))
                    {
                        parent.FullPath = WebUtil.MakeFullUrl(GetSiteCollectionNode(parent).FullPath, parent.FullPath);
                    }
                }
                var settings = SharePointSettingDao.GetAllSettingsForLevel(current, findLevel);
                if (settings == null)
                {
                    return;
                }
                var findFlags = new bool[settings.Count];
                for (var i = 0; i < settings.Count; i++)
                {
                    if (children.Any(s => new Guid(s.SPObjectId) == settings[i].ScopeId))
                    {
                        findFlags[i] = true;
                    }
                }
                var needRemoveIndex = new List<int>();
                for (int i = 0; i < findFlags.Length; i++)
                {
                    if (!findFlags[i])
                    {
                        needRemoveIndex.Add(i);
                    }
                }
                foreach (var i in needRemoveIndex)
                {
                    SharePointSettingDao.MarkRemovedSharePointSetting(settings[i].ScopeId);
                    if (findLevel == NodeLevel.WebApplication)//Group level 无法按照full path查找其下面的节点。
                    {
                        var siteGroupId = settings[i].ScopeId;
                        await SharePointSettingDao.MarkRemovedSharePointSettingUnderCurrentAsync(s => s.SiteGroupId == siteGroupId && !s.IsRemoved);
                    }
                    else
                    {
                        var fullPath = settings[i].FullPath + "/";
                        await SharePointSettingDao.MarkRemovedSharePointSettingUnderCurrentAsync(s => s.FullPath.StartsWith(fullPath) && !s.IsRemoved);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Delete Dirty SPSData error:{0}", e.ToString());
            }
        }
        #endregion

        #region Disposal Dirty Data 
        private Dictionary<string, bool> underSiteCollSettingsNeedRemoveForDisposal = new Dictionary<string, bool>();
        public async System.Threading.Tasks.Task CheckDirtyDisposalDataAsync()
        {
            Dictionary<string, RMSPTreeNode> allNodes = new Dictionary<string, RMSPTreeNode>();
            var disposalSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.DisposalSchedule);
            foreach (var schedule in disposalSchedules)
            {
                var treeNode = JsonConvert.DeserializeObject<Contract.Object.RMSPTreeNode>(schedule.Extentions);
                allNodes.Add(schedule.Id, treeNode);
            }

            var upperSiteCollSettings = new List<RMSPTreeNode>();
            foreach (var kv in allNodes)
            {
                var node = kv.Value;
                if (node.Level > (int)NodeLevel.SiteCollection)
                {
                    var scNode = GetSiteCollectionNode(node);
                    if (!upperSiteCollSettings.Any(s => s.SPObjectId == scNode.SPObjectId))
                    {
                        upperSiteCollSettings.Add(scNode);
                    }
                    underSiteCollSettingsNeedRemoveForDisposal[node.FullPath + "|" + node.SPObjectId] = true;
                }
                else
                {
                    //SiteCollection WebApplication
                    upperSiteCollSettings.Add(node);
                }
            }
            logger.Debug("SPSetting upper site coll level node: {0}", string.Join(",", upperSiteCollSettings.Select(s => s.FullPath).ToList()));
            foreach (var node in upperSiteCollSettings)
            {
                switch (node.Level)
                {
                    case (int)NodeLevel.WebApplication:
                        #region global
                        try
                        {
                            //DAOAPIClientV1 client = new DAOAPIClientV1();
                            //RemoteWebApplication webApp = client.GetWebApplicationById(node.Id);
                            RemoteWebApplication webApp = RABrowserClient.GetWebApplicationById(node.Id);
                            if (webApp == null)
                            {
                                logger.Info("Group was removed in DAO Register Groups {0}", node.FullPath);
                                foreach (var n in allNodes)
                                {
                                    if (CheckIsDescendantsNode(n.Value, node.SPObjectId) || n.Value.SPObjectId == node.Id)
                                    {
                                        var deleteDescendantsScheduleId = n.Key;
                                        RMSchedule deleteDescendantsSchedule = RMScheduleDao.GetSchedule(deleteDescendantsScheduleId);
                                        await RMScheduleDao.MarkScheduleRemovedAsync(deleteDescendantsScheduleId);
                                        logger.Info("mark removed disposal schedule dirty data:{0}", ForeachClassProperties(deleteDescendantsSchedule));
                                    }
                                }
                            }
                            else
                            {
                                await CheckDisposalSubNodeSettingsAsync(node, allNodes);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                        break;
                    #endregion
                    case (int)NodeLevel.SiteCollection:
                        #region site collection
                        try
                        {
                            //var client = new DAOAPIClientV1();
                            //var testSite = client.GetRemoteSiteCollectionByUrl(node.FullPath);
                            var testSite = RABrowserClient.GetRemoteSiteCollectionByUrl(node.FullPath);
                            if (testSite == null || !testSite.parentId.Equals(GetGroupNode(node).SPObjectId, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var n in allNodes)
                                {
                                    if (CheckIsDescendantsNode(n.Value, node.SPObjectId) || n.Value.SPObjectId == node.Id)
                                    {
                                        var deleteDescendantsScheduleId = n.Key;
                                        var deleteDescendantsSchedule = RMScheduleDao.GetSchedule(deleteDescendantsScheduleId);
                                        await RMScheduleDao.MarkScheduleRemovedAsync(deleteDescendantsScheduleId);
                                        logger.Info("mark removed disposal schedule dirty data:{0}", ForeachClassProperties(deleteDescendantsSchedule));
                                    }
                                }
                            }
                            else
                            {
                                await CheckDisposalSubNodeSettingsAsync(node, allNodes);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                        break;
                    #endregion
                    default:
                        logger.Warn("node is {0},node level is{1}", node.FullPath, ((NodeLevel)node.Level).ToString());
                        break;
                }
            }
            logger.Info("disposal node need remove without browse {0}", string.Join(",", underSiteCollSettingsNeedRemoveForDisposal.Where(s => s.Value).Select(s => s.Key)));
            foreach (var item in underSiteCollSettingsNeedRemoveForDisposal.Where(s => s.Value))
            {
                try
                {
                    //SharePointSettingDao.MarkRemovedSharePointSetting();
                    await RMScheduleDao.MarkScheduleRemovedAsync(allNodes.Where(s => s.Value.FullPath + "|" + s.Value.SPObjectId == item.Key).Select(s => s.Key).First());
                }
                catch (Exception e)
                {
                    logger.Error("delete node {0} error {1}", item.Key, e.ToString());
                }
            }
        }

        public async System.Threading.Tasks.Task CheckDisposalSubNodeSettingsAsync(RMSPTreeNode node, Dictionary<string, RMSPTreeNode> allNodes)//node level is Site or List.
        {
            if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Lists)
            {
                await DeleteDirtyDisposalDataAsync(node, await RMSPTreeService.BrowseAsync(node), allNodes);
            }
            else if (node.Level == (int)NodeLevel.SiteCollection
                || node.Level == (int)NodeLevel.Site
                || node.Level == (int)NodeLevel.Lists
                || node.Level == (int)NodeLevel.Sites)
            {
                var children = await RMSPTreeService.BrowseAsync(node);
                await DeleteDirtyDisposalDataAsync(node, children, allNodes);
                //for vir node.
                foreach (var subNode in children)
                {
                    await CheckDisposalSubNodeSettingsAsync(subNode, allNodes);
                }
            }
        }

        public async System.Threading.Tasks.Task DeleteDirtyDisposalDataAsync(RMSPTreeNode current, List<RMSPTreeNode> children, Dictionary<string, RMSPTreeNode> allNodes)
        {
            foreach (var child in children)
            {
                if (underSiteCollSettingsNeedRemoveForDisposal.ContainsKey(child.FullPath + "|" + child.SPObjectId))
                {
                    underSiteCollSettingsNeedRemoveForDisposal[child.FullPath + "|" + child.SPObjectId] = false;
                }
            }
            Dictionary<string, string> urlScheduleMappings = new Dictionary<string, string>();
            foreach (var node in allNodes)
            {
                urlScheduleMappings.Add(node.Value.SPObjectId, node.Key);
            }
            List<RMSPTreeNode> findSettings = null;
            try
            {
                if (current.Level == (int)NodeLevel.Sites)
                {
                    findSettings = allNodes.Values.Where(s => s.Level == (int)NodeLevel.Site && s.Parent.Parent.SPObjectId == current.Parent.SPObjectId).ToList();
                }
                else if (current.Level == (int)NodeLevel.Lists)
                {
                    findSettings = allNodes.Values.Where(s => s.Level == (int)NodeLevel.List && s.Parent.Parent.SPObjectId == current.Parent.SPObjectId).ToList();
                }
                else
                {
                    findSettings = allNodes.Values.Where(s => s.Parent.SPObjectId == current.SPObjectId).ToList();
                }
                if (findSettings == null)
                {
                    return;
                }
                var findFlags = new bool[findSettings.Count];
                for (var i = 0; i < findSettings.Count; i++)
                {
                    if (children.Any(s => s.SPObjectId == findSettings[i].SPObjectId))
                    {
                        findFlags[i] = true;
                    }
                }
                var needRemoveIndex = new List<int>();
                for (int i = 0; i < findFlags.Length; i++)
                {
                    if (!findFlags[i])
                    {
                        needRemoveIndex.Add(i);
                    }
                }
                foreach (var i in needRemoveIndex)
                {
                    var deleteScheduleId = urlScheduleMappings[findSettings[i].SPObjectId];
                    var deleteSchedule = RMScheduleDao.GetSchedule(deleteScheduleId);
                    await RMScheduleDao.MarkScheduleRemovedAsync(deleteScheduleId);
                    logger.Info("remove disposal schedule dirty data:{0}", ForeachClassProperties(deleteSchedule));
                    foreach (var node in allNodes)
                    {
                        if (CheckIsDescendantsNode(node.Value, findSettings[i].SPObjectId))
                        {
                            var deleteDescendantsScheduleId = node.Key;
                            var deleteDescendantsSchedule = RMScheduleDao.GetSchedule(deleteDescendantsScheduleId);
                            await RMScheduleDao.MarkScheduleRemovedAsync(deleteDescendantsScheduleId);
                            logger.Info("remove disposal schedule dirty data:{0}", ForeachClassProperties(deleteDescendantsSchedule));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Delete Dirty SPSData error:{0}", e.ToString());
            }
        }
        #endregion
        public static string ForeachClassProperties<T>(T model)
        {
            var builder = new StringBuilder();
            builder.Append("{");
            Type t = model.GetType();
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                object value = item.GetValue(model, null);
                builder.AppendFormat(@"""{0}"":""{1}"", ", name, value?.ToString().Replace("\"", "\\\""));
            }
            builder.Remove(builder.Length - 2, 2);//remove , and space
            builder.Append("}");
            return builder.ToString();
        }

        #region EXO Dirty Data
        public async System.Threading.Tasks.Task CheckDirtyEXOSDataAsync()
        {
            var allSettings = EXOSettingDao.LoadAllSetting();
            foreach (var setting in allSettings)
            {
                var node = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(setting.NodeInfo);
                switch (node.Level)
                {
                    case (int)NodeLevel.ExchangeOnlineMailboxGroup:
                        #region global
                        try
                        {
                            //DAOAPIClientV1 client = new DAOAPIClientV1();
                            ExchangeOnlineTreeNodeDto group = null;
                            try
                            {
                                //group = client.GetExchangeNodeById(node.Id);
                                group = RABrowserClient.GetExchangeNodeById(node.Id);
                            }
                            catch (Exception e)
                            {
                                logger.Error("get exo node error:{0}", e.ToString());
                            }
                            if (group == null)
                            {
                                logger.Info("Group was removed in DAO Register Groups {0}", node.Name);
                                EXOSettingDao.MarkRemovedSharePointSetting(new Guid(node.Id));
                                var siteGroupId = node.GroupId;
                                await EXOSettingDao.MarkRemovedSharePointSettingUnderCurrentAsync(s => s.GroupId == siteGroupId && !s.IsRemoved);
                            }
                            else
                            {
                                CheckSPSSubNodeSettings(node);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                        break;
                    #endregion
                    case (int)NodeLevel.ExchangeOnlineMailbox:
                        #region site collection
                        try
                        {
                            //var client = new DAOAPIClientV1();
                            ExchangeOnlineTreeNodeDto mailbox = null;
                            try
                            {
                                //mailbox = client.GetExchangeNodeById(node.Id);
                                mailbox = RABrowserClient.GetExchangeNodeById(node.Id);
                            }
                            catch (Exception e)
                            {
                                logger.Error("get exo node error:{0}", e.ToString());
                            }
                            if (mailbox == null)
                            {
                                EXOSettingDao.MarkRemovedSharePointSetting(new Guid(node.Id));
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                        break;
                    #endregion
                    default:
                        logger.Warn("node is {0},node level is{1}", node.FullPath, ((NodeLevel)node.Level).ToString());
                        break;
                }
            }
        }

        public void CheckSPSSubNodeSettings(RMEXOTreeNode node)//node level is Site or List.
        {
            logger.Debug("current node is {0}", !string.IsNullOrEmpty(node.FullPath) ? node.FullPath : node.Name);
            if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                DeleteDirtySPSData(node, RMSPTreeService.BrowseExchangeTree(node));
            }
        }

        public void DeleteDirtySPSData(RMEXOTreeNode current, List<RMEXOTreeNode> children)
        {
            try
            {
                var settings = EXOSettingDao.GetAllSettingsForGroup(current);
                if (settings == null)
                {
                    return;
                }
                var findFlags = new bool[settings.Count];
                for (var i = 0; i < settings.Count; i++)
                {
                    if (children.Any(s => new Guid(s.Id) == settings[i].ScopeId))
                    {
                        findFlags[i] = true;
                    }
                }
                var needRemoveIndex = new List<int>();
                for (int i = 0; i < findFlags.Length; i++)
                {
                    if (!findFlags[i])
                    {
                        needRemoveIndex.Add(i);
                    }
                }
                foreach (var i in needRemoveIndex)
                {
                    EXOSettingDao.MarkRemovedSharePointSetting(settings[i].ScopeId);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Delete Dirty EXOData error:{0}", e.ToString());
            }
        }
        #endregion

        #region re sps
        public async Task<RMSPTreeNode> LoadSampleNodeSettingsAsync(RMSPSampleTreeNode sNode)
        {
            var configNode = new RMSPTreeNode();
            configNode.IconStatus = IconStatus.NoSet;
            #region copy node properties
            configNode.Id = sNode.Id;
            configNode.FarmId = sNode.FarmId;
            configNode.FarmName = sNode.FarmName;
            configNode.Name = sNode.Name;
            configNode.Title = sNode.Title;
            configNode.FullPath = sNode.FullPath;
            configNode.Level = sNode.Level;
            configNode.NodeType = sNode.NodeType;
            configNode.SPType = sNode.SPType;
            configNode.SPObjectId = sNode.SPObjectId;
            configNode.SPVersion = sNode.SPVersion;
            configNode.Expanded = sNode.Expanded;
            configNode.ChildrenCount = sNode.ChildrenCount;
            configNode.CheckNumber = sNode.CheckNumber;
            configNode.Hidden = sNode.Hidden;
            configNode.TemplateId = sNode.TemplateId;
            configNode.BposInfo = sNode.BposInfo;
            #endregion

            try
            {
                RMSPSampleTreeNode groupNode = sNode;
                while (groupNode.Level != (int)NodeLevel.WebApplication && groupNode != null)
                {
                    groupNode = groupNode.Parent;
                }
                if (groupNode == null)
                {
                    return configNode;
                }
                //var groupNode = GetGroupNode(configNode);
                Guid groupId = Guid.Empty;
                bool ownSetting = true;
                bool folderDisable = false;
                string GlobalColumnName = string.Empty;
                string GlobalColumnNameDesc = string.Empty;
                if (groupNode != null && !string.IsNullOrEmpty(groupNode.SPObjectId))
                {
                    groupId = new Guid(groupNode.SPObjectId);
                }
                var GSetting = SharePointSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                if (GSetting != null)
                {
                    configNode.IconStatus = IconStatus.Inhert;
                    GlobalColumnName = GSetting.ColumnName;
                    GlobalColumnNameDesc = GSetting.Description;
                    var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                    var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);
                    var termScope = TermDao.GetRMTermByGuId(GSetting.TermId);
                    RMTermSet termSet = null;
                    if (GSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(GSetting.TermSetId);
                    }


                    configNode.ColumnName = GlobalColumnName;
                    configNode.ColumnRequired = GSetting.ColumnRequired == null ? true : (bool)GSetting.ColumnRequired;
                    configNode.ColumnHidden = GSetting.ColumnHidden == null ? false : (bool)GSetting.ColumnHidden;
                    configNode.Description = GlobalColumnNameDesc;
                    configNode.ExistColumnName = GSetting.ExistColumnName;
                    configNode.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                    configNode.SetDocLevelTermForExistColumn = GSetting.SetDocLevelTermForExistColumn;
                    configNode.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                    configNode.TermIdOfContainer = GSetting.TermIdOfContainer;
                    configNode.ContainerTermFullPath = GSetting.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermIdOfContainer) : "";
                    configNode.isEnableClassification = GSetting.isEnableClassification;
                    configNode.DescriptionOfContainer = GSetting.DescriptionOfContainer;
                    configNode.IsInheritParentTerm = GSetting.IsInheritParentTerm;
                    configNode.TermSetId = GSetting.TermSetId;
                    configNode.TermSetName = GSetting.TermSetName;
                    configNode.TermId = GSetting.TermId;
                    configNode.TermName = GSetting.TermName;
                    configNode.DefaultTermId = GSetting.DefaultTermId;
                    configNode.DefaultTermName = termDefaultValue == null ? GSetting.DefaultTermName : termDefaultValue.Name;
                    configNode.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                    configNode.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";

                    //configNode.DefaultTermNameFullPath = termDefaultValue == null ? GSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                    configNode.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                    configNode.IsShowUniqueId = GSetting.IsShowUniqueId == null ? true : (bool)GSetting.IsShowUniqueId;
                    configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                    configNode.isFailedConfigClassification = GSetting.isFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = GSetting.isFailedConfigMetaDataColumn;
                    configNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                    configNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                    configNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = GSetting.ApplyExistType;
                    if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.None;
                    }
                    configNode.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                    configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                    //configNode.RecordOwner = GetSettingRecordOnwers(GSetting.Id, SourceType.SharePoint);
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePoint);
                    configNode.SiteGroupId = GSetting.SiteGroupId;
                    //configNode.ProfileId = GSetting.IdPath;
                    configNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                    if (GSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && GSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                    //configNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                    configNode.IncludeDeclaredRecords = GSetting.IncludeDeclaredRecords;
                    configNode.ApplyTermIncludeFolder = GSetting.IsApplyTermIncludeFolder();
                    configNode.AlwaysScanAllExistDocuments = GSetting.AlwaysScanAllExistDocuments;
                    configNode.IsKeepSharePointDefaultValue = GSetting.IsKeepSharePointDefaultValue;
                    configNode.SetTermForEmptyDefaultValue = GSetting.SetTermForEmptyDefaultValue;
                    if (sNode.Level == (int)NodeLevel.SiteCollection || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Folder)
                    {
                        if (GSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        else
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
                        }
                    }
                    configNode.isEnableClassification = GSetting.isEnableClassification;
                    configNode.IsSyncData = GSetting.IsSyncData;
                    if (!string.IsNullOrEmpty(GSetting.NodeInfo))
                    {
                        RMSPTreeNode GSettingSPTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(GSetting.NodeInfo);
                        configNode.SupportLockedSite = GSettingSPTreeNode.SupportLockedSite;
                        configNode.EnableLifecycleManagementForSharePointLists = GSettingSPTreeNode.EnableLifecycleManagementForSharePointLists;
                    }
                    configNode.ApprovalType = (int)GSetting.ApprovalType;
                    configNode.WorkflowReferenceId = GSetting.WorkflowReferenceId;

                    configNode.AITermUseType = GSetting.AITermUseType;
                    configNode.AIApprovalType = (int)GSetting.AIApprovalType;
                    configNode.AISendEMail = GSetting.AISendEMail;
                    configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.AISharePointOnline);
                    configNode.AIThenIsDefaultTermMethod = GSetting.AIThenIsDefaultTermMethod;
                    configNode.AIThenDefaultTermId = GSetting.AIThenDefaultTermId;
                    configNode.AIThenDefaultTermName = GSetting.AIThenDefaultTermName;

                    //SetDisposeJob(configNode, GSetting.DisposalJobId1);
                    //SetCollectionJob(configNode, GSetting.CollectionJobId1);
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
                var spSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(sNode.SPObjectId), siteId, true);//TODO 暂时不考虑 only mark physical
                if (configNode.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                {
                    var pNode = LoadFolderParentSeting(sNode, siteId);
                    if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                    {
                        if (spSetting != null)
                        {
                            spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        folderDisable = true;
                    }
                }

                if (spSetting == null)
                {
                    if (sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.Folder)
                    {
                        spSetting = LoadSampleNodeParentSeting(sNode.Parent, siteId);
                        if (spSetting != null && configNode.Level != (int)NodeLevel.WebApplication)
                        {
                            if (spSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                            {
                                spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                        }
                        configNode.IsCustomSetting = false;
                    }
                }
                else
                {
                    configNode.IconStatus = IconStatus.Break;
                    if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                    {
                        configNode.IsCustomSetting = true;
                    }
                }

                if (spSetting != null)
                {
                    var termScope = TermDao.GetRMTermByGuId(spSetting.TermId);
                    var defaultTerm = TermDao.GetRMTermByGuId(spSetting.DefaultTermId);
                    var containerTerm = TermDao.GetRMTermByGuId(spSetting.TermIdOfContainer);
                    RMTermSet termSet = null;
                    if (spSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(spSetting.TermSetId);
                    }

                    RMSPTreeNode rMSPTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(spSetting.NodeInfo);
                    configNode.SupportLockedSite = rMSPTreeNode.SupportLockedSite;
                    configNode.EnableLifecycleManagementForSharePointLists = rMSPTreeNode.EnableLifecycleManagementForSharePointLists;
                    configNode.ColumnName = GlobalColumnName;
                    configNode.Description = GlobalColumnNameDesc;
                    configNode.ColumnRequired = spSetting.ColumnRequired == null ? true : (bool)spSetting.ColumnRequired;
                    configNode.ColumnHidden = spSetting.ColumnHidden == null ? false : (bool)spSetting.ColumnHidden;
                    configNode.DefaultTermId = spSetting.DefaultTermId;
                    configNode.DefaultTermName = defaultTerm == null ? spSetting.DefaultTermName : defaultTerm.Name;
                    configNode.TermScopeFullPath = spSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(spSetting.TermSetId);
                    configNode.DefaultTermFullPath = spSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.DefaultTermId) : "";
                    //configNode.DefaultTermNameFullPath = defaultTerm == null ? spSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(spSetting.DefaultTermId);
                    configNode.TermId = spSetting.TermId;
                    configNode.TermName = termScope == null ? spSetting.TermName : termScope.Name;
                    //configNode.TermNameFullPath = termScope == null ? spSetting.TermName : TermDao.GetTermFullPathByTermId(spSetting.TermId);
                    configNode.TermSetId = spSetting.TermSetId;
                    configNode.TermSetName = spSetting.TermSetName;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                    configNode.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                    configNode.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                    configNode.DescriptionOfContainer = spSetting.DescriptionOfContainer;
                    configNode.IsInheritParentTerm = spSetting.IsInheritParentTerm;
                    configNode.TermIdOfContainer = spSetting.TermIdOfContainer;
                    configNode.TermNameOfContainer = containerTerm == null ? spSetting.TermNameOfContainer : containerTerm.Name;
                    configNode.ContainerTermFullPath = spSetting.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermIdOfContainer) : "";
                    configNode.isEnableClassification = spSetting.isEnableClassification;
                    configNode.IsEnableHoldPhyical = spSetting.IsEnableHoldPhyical;
                    configNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                    configNode.isFailedConfigClassification = spSetting.isFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = spSetting.isFailedConfigMetaDataColumn;
                    configNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                    configNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                    //configNode.ExistColumnName = spSetting.ExistColumnName;
                    //configNode.IsUsingExistColumnName = spSetting.IsUsingExistColumnName;
                    configNode.IsDisplyaTermPath = spSetting.IsDisplyaTermPath;
                    //configNode.IsShowUniqueId = spSetting.IsShowUniqueId;
                    configNode.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = spSetting.ApplyExistType;
                    if (spSetting.NeedCheckDefaultValue && spSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                    }

                    configNode.EnableRelatedRecords = spSetting.EnableRelatedRecords;
                    //configNode.RecordOwner = GetSettingRecordOnwers(spSetting.Id, SourceType.SharePoint);
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.SharePoint);
                    configNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
                    configNode.IsSyncData = spSetting.IsSyncData;
                    //configNode.ProfileId = spSetting.IdPath;
                    configNode.DeployTermMethod = spSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)spSetting.DeployTermMethod;
                    if (spSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && spSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = spSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = spSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)spSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)spSetting.AutoJobOption;
                    configNode.IncludeDeclaredRecords = spSetting.IncludeDeclaredRecords;
                    configNode.ApprovalType = (int)spSetting.ApprovalType;
                    configNode.WorkflowReferenceId = spSetting.WorkflowReferenceId;
                    configNode.ApplyTermIncludeFolder = spSetting.IsApplyTermIncludeFolder();
                    configNode.AlwaysScanAllExistDocuments = spSetting.AlwaysScanAllExistDocuments;

                    configNode.AITermUseType = spSetting.AITermUseType;
                    configNode.AIApprovalType = (int)spSetting.AIApprovalType;
                    configNode.AISendEMail = spSetting.AISendEMail;
                    configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.AISharePointOnline);
                    configNode.AIThenIsDefaultTermMethod = spSetting.AIThenIsDefaultTermMethod;
                    configNode.AIThenDefaultTermId = spSetting.AIThenDefaultTermId;
                    configNode.AIThenDefaultTermName = spSetting.AIThenDefaultTermName;
                    //SetDisposeJob(configNode, spSetting.DisposalJobId1);
                    //if (sNode.Level == (int)NodeLevel.WebApplication || sNode.Level == (int)NodeLevel.SiteCollection)
                    //{
                    //    SetCollectionJob(configNode, spSetting.CollectionJobId1);
                    //}
                    //else
                    //{
                    //    var tempSetting = SharePointSettingDao.LoadSharePointSetting(siteId, siteId, true);//TODO 暂时不考虑 only mark physical
                    //    if (tempSetting != null)
                    //    {
                    //        SetCollectionJob(configNode, tempSetting.CollectionJobId1);
                    //    }
                    //}
                }

                if (string.IsNullOrEmpty(configNode.ColumnName))
                {
                    configNode.ColumnRequired = true;
                }
                //if (!configNode.IsCustomSetting && configNode.Level != (int)NodeLevel.WebApplication)
                //{
                //    configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                //    configNode.DefaultTermId = Guid.Empty;
                //    configNode.DefaultTermName = string.Empty;
                //    configNode.TermId = Guid.Empty;
                //    configNode.TermName = string.Empty;
                //    configNode.AutoClassificationRules = null;
                //}

                var profileId = ScheduleService.GetProfileId(sNode);
                var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.DisposalSchedule);
                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                    configNode.IsEnableSuperUserDecrypt = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableSuperUserDecrypt).GetValueOrDefault();
                    configNode.IsEnableRemoveRetentionLabel = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableRemoveRetentionLabel).GetValueOrDefault();
                    configNode.DisposeScheduleInfo = disposeSchedule;
                    configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    //configNode.IsCustomSetting = true;
                    configNode.IconStatus = IconStatus.Break;
                    //if (!configNode.IsCustomSetting && configNode.Level != (int)NodeLevel.WebApplication)
                    //{
                    //    configNode.DisposeScheduleInfo.Id = "1";
                    //}
                }
                else
                {
                    var ancestryDisposeSchedule = await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.DisposalSchedule);
                    if (ancestryDisposeSchedule != null)
                    {
                        var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                        ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                        ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                        configNode.IsEnableSuperUserDecrypt = (JsonConvert.DeserializeObject<RMSPTreeNode>(ancestryDisposeSchedule.Extentions)?.IsEnableSuperUserDecrypt).GetValueOrDefault();
                        configNode.IsEnableRemoveRetentionLabel = (JsonConvert.DeserializeObject<RMSPTreeNode>(ancestryDisposeSchedule.Extentions)?.IsEnableRemoveRetentionLabel).GetValueOrDefault();
                        configNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                        configNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                        configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    }
                    else
                    {
                        configNode.DisposeScheduleInfo = null;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
            return configNode;
        }

        public async Task<RMSPTreeNode> LoadSampleNodeSettingsByScopeId(Guid scopeId, int id)
        {
            var configNode = new RMSPTreeNode();
            var GSetting = SharePointSettingDao.LoadChannelSetting(scopeId, id);
            var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(GSetting.NodeInfo);
            if (GSetting != null)
            {
                configNode.IconStatus = IconStatus.Inhert;
                var globalColumnName = GSetting.ColumnName;
                var globalColumnNameDesc = GSetting.Description;
                var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);

                configNode.ColumnName = globalColumnName;
                configNode.ColumnRequired = GSetting.ColumnRequired == null ? true : (bool)GSetting.ColumnRequired;
                configNode.ColumnHidden = GSetting.ColumnHidden == null ? false : (bool)GSetting.ColumnHidden;
                configNode.Description = globalColumnNameDesc;
                configNode.ExistColumnName = GSetting.ExistColumnName;
                configNode.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                configNode.SetDocLevelTermForExistColumn = GSetting.SetDocLevelTermForExistColumn;
                configNode.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                configNode.TermIdOfContainer = GSetting.TermIdOfContainer;
                configNode.ContainerTermFullPath = GSetting.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermIdOfContainer) : "";
                configNode.isEnableClassification = GSetting.isEnableClassification;
                configNode.DescriptionOfContainer = GSetting.DescriptionOfContainer;
                configNode.IsInheritParentTerm = GSetting.IsInheritParentTerm;
                configNode.TermSetId = GSetting.TermSetId;
                configNode.TermSetName = GSetting.TermSetName;
                configNode.TermId = GSetting.TermId;
                configNode.TermName = GSetting.TermName;
                configNode.DefaultTermId = GSetting.DefaultTermId;
                configNode.DefaultTermName = termDefaultValue == null ? GSetting.DefaultTermName : termDefaultValue.Name;
                configNode.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                configNode.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";
                configNode.Level = node.Level;

                //configNode.DefaultTermNameFullPath = termDefaultValue == null ? GSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                configNode.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                configNode.IsShowUniqueId = GSetting.IsShowUniqueId == null ? true : (bool)GSetting.IsShowUniqueId;
                configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                configNode.isFailedConfigClassification = GSetting.isFailedConfigClassification;
                configNode.isFailedConfigMetaDataColumn = GSetting.isFailedConfigMetaDataColumn;
                configNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                configNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                configNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                configNode.ApplyExistType = GSetting.ApplyExistType;
                if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                {
                    configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.None;
                }
                configNode.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                //configNode.RecordOwner = GetSettingRecordOnwers(GSetting.Id, SourceType.SharePoint);
                configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePoint);
                configNode.SiteGroupId = GSetting.SiteGroupId;
                //configNode.ProfileId = GSetting.IdPath;
                configNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                if (GSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && GSetting.DefaultTermId == Guid.Empty)
                {
                    configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                }
                configNode.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                    null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                SetAutoTermStatus(configNode.AutoClassificationRules);
                await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                configNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                configNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                configNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                configNode.IncludeDeclaredRecords = GSetting.IncludeDeclaredRecords;
                configNode.ApplyTermIncludeFolder = GSetting.IsApplyTermIncludeFolder();
                configNode.AlwaysScanAllExistDocuments = GSetting.AlwaysScanAllExistDocuments;
                configNode.IsKeepSharePointDefaultValue = GSetting.IsKeepSharePointDefaultValue;
                configNode.SetTermForEmptyDefaultValue = GSetting.SetTermForEmptyDefaultValue;
                configNode.isEnableClassification = GSetting.isEnableClassification;
                configNode.IsSyncData = GSetting.IsSyncData;
                configNode.ApprovalType = (int)GSetting.ApprovalType;
                configNode.WorkflowReferenceId = GSetting.WorkflowReferenceId;

                configNode.AITermUseType = GSetting.AITermUseType;
                configNode.AIApprovalType = (int)GSetting.AIApprovalType;
                configNode.AISendEMail = GSetting.AISendEMail;
                configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.AISharePointOnline);
                configNode.AIThenIsDefaultTermMethod = GSetting.AIThenIsDefaultTermMethod;
                configNode.AIThenDefaultTermId = GSetting.AIThenDefaultTermId;
                configNode.AIThenDefaultTermName = GSetting.AIThenDefaultTermName;

                var profileId = ScheduleService.GetProfileId(node);
                var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.DisposalSchedule);
                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                    configNode.IsEnableSuperUserDecrypt = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableSuperUserDecrypt).GetValueOrDefault();
                    configNode.IsEnableRemoveRetentionLabel = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableRemoveRetentionLabel).GetValueOrDefault();
                    configNode.DisposeScheduleInfo = disposeSchedule;
                    configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    //configNode.IsCustomSetting = true;
                    configNode.IconStatus = IconStatus.Break;
                    //if (!configNode.IsCustomSetting && configNode.Level != (int)NodeLevel.WebApplication)
                    //{
                    //    configNode.DisposeScheduleInfo.Id = "1";
                    //}
                }
                else
                {
                    var ancestryDisposeSchedule = await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.DisposalSchedule);
                    if (ancestryDisposeSchedule != null)
                    {
                        var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                        ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                        ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                        configNode.IsEnableSuperUserDecrypt = (JsonConvert.DeserializeObject<RMSPTreeNode>(ancestryDisposeSchedule.Extentions)?.IsEnableSuperUserDecrypt).GetValueOrDefault();
                        configNode.IsEnableRemoveRetentionLabel = (JsonConvert.DeserializeObject<RMSPTreeNode>(ancestryDisposeSchedule.Extentions)?.IsEnableRemoveRetentionLabel).GetValueOrDefault();
                        configNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                        configNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                        configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    }
                    else
                    {
                        configNode.DisposeScheduleInfo = null;
                    }
                }

            }
            return configNode;
        }


        public RMSharePointSetting LoadEnableRecordManagementParentAllSeting(SPTreeNodeDto node, Guid siteId)
        {
            RMSharePointSetting SPSetting = null;

            if (node.Level == NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == NodeLevel.SiteCollection || node.Level == NodeLevel.Site || node.Level == NodeLevel.List || node.Level == NodeLevel.Folder)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, false);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadEnableRecordManagementParentAllSeting(node.Parent, siteId);
            }

            return SPSetting;
        }

        public async System.Threading.Tasks.Task LoadSPSettingIconAsync(List<RMSPSampleTreeNode> nodes)
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
                        var gsSetting = SharePointSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                        var allSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.DisposalSchedule);
                        List<string> allSchedulesProfilesId = new List<string>();
                        if (allSchedules != null && allSchedules.Count != 0)
                        {
                            allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                        }

                        var allSettings = new Dictionary<string, RMSharePointSetting>();
                        var settings = SharePointSettingDao.LoadSharePointSettings(groupId, true).OrderBy(item => item.Id);
                        foreach (var setting in settings)
                        {
                            var key = setting.ScopeId.ToString() + setting.SiteId.ToString();
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
                            RMSharePointSetting csSetting = null;
                            var settingKey = node?.SPObjectId + siteNode?.SPObjectId;
                            if (allSettings.TryGetValue(settingKey, out csSetting))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            var profileId = ScheduleService.GetProfileId(node);
                            if (allSchedulesProfilesId.Contains(profileId))
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
                            var selfGSSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(selfGroupNode.SPObjectId), Guid.Empty);
                            if (selfGSSetting == null)
                            {
                                selfGroupNode.IconStatus = IconStatus.NoSet;
                            }
                            else
                            {
                                selfGroupNode.IconStatus = IconStatus.Break;
                            }

                            if (selfGroupNode.Children != null && selfGroupNode.Children.Any())
                            {
                                await LoadSPSettingIconAsync(selfGroupNode.Children);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting Icon.Error:{0}", e.ToString());
                throw;
            }
        }

        public RMSharePointSetting LoadParentAllSeting(RMSPSampleTreeNode node, Guid siteId, bool includeOnlySetPhysicalNode = false)
        {
            RMSharePointSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId, includeOnlySetPhysicalNode);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadParentAllSeting(node.Parent, siteId, includeOnlySetPhysicalNode);
            }

            return SPSetting;
        }

        public async Task<RAReturnMessage> AddEnableColumnSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (settingNode.Level == (int)NodeLevel.WebApplication)
                {
                    SharePointSettingDao.UpdateBCSColumnName(settingNode.SiteGroupId, settingNode.ColumnName, settingNode.Description, settingNode.ColumnRequired, settingNode.ColumnHidden);
                    await SharePointSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                    SharePointSettingDao.FlagCustomSettingNewColumn(settingNode.SiteGroupId);
                }
                else
                {
                    RMSPTreeNode siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId, false))
                    {

                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.SharePoint);
                        await SharePointSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                string nodeProfileIdPath = ScheduleService.GetProfileId(settingNode);
                SharePointSettingDao.RemoveDescendantsSetting(settingNode, nodeProfileIdPath);
                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditColumnSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddColumnSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Set SharePoint Column Setting");
                result.MessageType = RAMessageType.Successful;
                if (groupNode.IsShowUniqueId)
                {
                    UniqueIdSetting curUniqueIdSetting = UniqueIdSettingService.LoadingUniqueIdSetting();
                    if (curUniqueIdSetting == null || !curUniqueIdSetting.IsActived)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.UniqueIdSettingIsEmpty;
                        return result;
                    }
                }
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString()))
                {
                    if (!groupNode.IsUsingExistColumnName)
                    {
                        SharePointSettingDao.UpdateBCSColumnName(groupNode.SiteGroupId, groupNode.ColumnName, groupNode.Description, groupNode.ColumnRequired, groupNode.ColumnHidden);
                        await SharePointSettingDao.AddOrUpdateGlobalSettingAsync(groupNode);
                        SharePointSettingDao.FlagCustomSettingNewColumn(groupNode.SiteGroupId);
                    }
                    //else
                    //{
                    //    SharePointSettingDao.AddOrUpdateGlobalSettingUsingExistColumn(groupNode);
                    //}
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditDocLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddGlobalColumnAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Global SharePoint Setting");
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString()))
                {
                    if (!groupNode.IsUsingExistColumnName || (groupNode.IsUsingExistColumnName && groupNode.SetDocLevelTermForExistColumn))
                    {
                        AddFilterCretiaProperty(groupNode.AutoClassificationRules, SourceFlag.SharePoint);
                        //SharePointSettingDao.UpdateBCSColumnName(groupNode.SiteGroupId, groupNode.ColumnName);
                        await SharePointSettingDao.AddOrUpdateGlobalSettingAsync(groupNode);
                    }
                    //else
                    //{
                    //    SharePointSettingDao.AddOrUpdateGlobalSettingUsingExistColumn(groupNode);
                    //}
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditColumnSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddUsingExistColumnSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Begin save global using column name settings {0}:{1}", groupNode.FullPath, groupNode.ExistColumnName);
                result.MessageType = RAMessageType.Successful;
                if (groupNode.IsShowUniqueId)
                {
                    var curUniqueIdSetting = UniqueIdSettingService.LoadingUniqueIdSetting();
                    if (curUniqueIdSetting == null || !curUniqueIdSetting.IsActived)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.UniqueIdSettingIsEmpty;
                        return result;
                    }
                }
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString()))
                {
                   await  SharePointSettingDao.AddOrUpdateGlobalSettingUsingExistColumnAsync(groupNode, true);
                    logger.Info("using column name add or update global serring succes,group node:{0}", groupNode.Name);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Warn("using column name add or update global serring occur error,group node:{0},info:{1}", groupNode.Name, e.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditDocLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddCustomColumnAsync(RMSPTreeNode customNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Custom SharePoint Setting");
                var settingNode = customNode;
                RMSPTreeNode siteCollectionNode = null;

                siteCollectionNode = GetSiteCollectionNode(settingNode);
                if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                {
                    SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.SharePoint);

                    #region remove code
                    //if (customNode.Level == (int)NodeLevel.List && customNode.NodeType == (int)NodeType.DocumentLibrary)//Document Library 
                    //{
                    //    var spstting = SharePointSettingDao.LoadSharePointSetting(new Guid(settingNode.SPObjectId), new Guid(siteCollectionNode.SPObjectId));
                    //    if (spstting == null || spstting.TermId != settingNode.TermId)
                    //    {
                    //        var folderBreaks = SharePointSettingDao.GetDescendantsFolderBreakNodes(customNode);
                    //        if (folderBreaks != null && folderBreaks.Count > 0)
                    //        {
                    //            result.MessageType = RAMessageType.Failed;
                    //            result.FaildType = RAFailedType.BreakFolderNode;
                    //            var folderFullPaths = folderBreaks.Select(s => s.FullPath).ToList();
                    //            for (int i = 0; i < folderFullPaths.Count; i++)
                    //            {
                    //                folderFullPaths[i] = folderFullPaths[i].Replace(customNode.FullPath + "/", "");
                    //            }
                    //            result.ErrorMessage = I18NEntity.GetString("RM_SPS_HasBreakFolderNodes_Save_Failed", string.Join("; ", folderFullPaths));
                    //            return result;
                    //        }
                    //    }
                    //}
                    //if (customNode.Level == (int)NodeLevel.Folder)
                    //{
                    //    var libNode = GetSPListNode(settingNode);
                    //    var libNodeSetting = SharePointSettingDao.GetParentLibraryCustomSetting(new Guid(libNode.SPObjectId));
                    //    if (libNodeSetting == null)
                    //    {
                    //        result.MessageType = RAMessageType.Failed;
                    //        result.FaildType = RAFailedType.BreakFolderNode;
                    //        result.ErrorMessage = I18NEntity.GetString("RM_SPS_HasBreakFolderNodes_Dependent_Failed");
                    //        return result;
                    //    }
                    //}
                    #endregion
                    await SharePointSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }

                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditConLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddContainerTermAsync(RMSPTreeNode containerNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Container SharePoint Setting");
                var settingNode = containerNode;
                if (containerNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString()))
                    {
                        await SharePointSettingDao.AddOrUpdateGlobalSettingAsync(containerNode);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    logger.Info("Set Container SharePoint Setting, current node save term as group : {0}", containerNode.FullPath);
                    RMSPTreeNode siteCollectionNode = null;
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                    {
                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        await SharePointSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditLocationOwnersSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode locationNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Container SharePoint Setting");
                var settingNode = locationNode;
                if (locationNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString()))
                    {
                        await SharePointSettingDao.AddOrUpdateGlobalSettingAsync(locationNode);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    logger.Info("Set Container SharePoint Setting, current node save term as group : {0}", locationNode.FullPath);
                    RMSPTreeNode siteCollectionNode = null;
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                    {
                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        await SharePointSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditInheritSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Inherit Parent Settings");
                var siteCollectionNode = GetSiteCollectionNode(node);

                //if (node.Level == (int)NodeLevel.List && node.NodeType == (int)NodeType.DocumentLibrary)//Document Library 
                //{
                //    var folderBreaks = SharePointSettingDao.GetDescendantsFolderBreakNodes(node);
                //    if (folderBreaks != null && folderBreaks.Count > 0)
                //    {
                //        result.MessageType = RAMessageType.Failed;
                //        result.FaildType = RAFailedType.BreakFolderNode;
                //        var folderFullPaths = folderBreaks.Select(s => s.FullPath).ToList();
                //        for (int i = 0; i < folderFullPaths.Count; i++)
                //        {
                //            folderFullPaths[i] = folderFullPaths[i].Replace(node.FullPath + "/", "");
                //        }
                //        result.ErrorMessage = I18NEntity.GetString("RM_SPS_HasBreakFolderNodes_Inherit_Failed", string.Join("; ", folderFullPaths));
                //        return result;
                //    }
                //}

                await SharePointSettingDao.DeleteSharePointSettingAsync(new Guid(node.SPObjectId), new Guid(siteCollectionNode.SPObjectId));
                await CleanParentNodeSettingAsync(node);
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

        public async System.Threading.Tasks.Task UpgradeScheduleProfileId4SharePointSettingQueryAsync()
        {
            //for sps query by like profileId
            try
            {
                logger.Info("begin to run upgrade sp setting.");
                //TODO GetScheduleByTypeService
                //List<ScheduleInfo> scheduleInfo = ScheduleService.GetScheduleByTypeService(ScheduleType.DisposalSchedule).Where(s => !string.IsNullOrEmpty(s.Extentions) && !string.IsNullOrEmpty(s.ProfileId)).ToList();
                var scheduleInfo = (await RMScheduleDao.FindListAsync(s => s.JobCategory == (int)ScheduleType.DisposalSchedule && !string.IsNullOrEmpty(s.Extentions) && !string.IsNullOrEmpty(s.ProfileId))).ToList();

                foreach (var sdu in scheduleInfo)
                {
                    var pathIds = sdu.ProfileId.Split('|');
                    Guid groupId0 = Guid.Empty;
                    Guid siteId1 = Guid.Empty;
                    Guid scopeId2 = Guid.Empty;
                    int length = pathIds.Length;
                    if (length == 3)
                    {
                        var node = JsonConvert.DeserializeObject<RMSPTreeNode>(sdu.Extentions);
                        logger.Info("upgrade schedule profileId, node{0}", node.FullPath);
                        Guid.TryParse(pathIds[0], out groupId0);
                        Guid.TryParse(pathIds[1], out siteId1);
                        Guid.TryParse(pathIds.Last(), out scopeId2);

                        if (groupId0 == scopeId2 && Guid.Empty == siteId1)
                        {
                            //group
                            logger.Info("old schedule profileId: {0}", sdu.ProfileId);
                            sdu.ProfileId = groupId0.ToString();
                            logger.Info("new schedule profileId: {0}", sdu.ProfileId);
                            await RMScheduleDao.UpdateAsync(sdu);
                        }
                        if (siteId1 == scopeId2)
                        {
                            //site collection
                            logger.Info("old schedule profileId: {0}", sdu.ProfileId);
                            sdu.ProfileId = groupId0.ToString() + "|" + siteId1.ToString();
                            logger.Info("new schedule profileId: {0}", sdu.ProfileId);
                            await RMScheduleDao.UpdateAsync(sdu);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade sharepoint setting,ERROR:{0}", ex.ToString());
            }
        }


      



       
        public async System.Threading.Tasks.Task LoadExchangeSettingIconAsync(List<RMSampleEXOTreeNode> nodes)
        {
            try
            {
                if (nodes.Count > 0)
                {
                    RMSampleEXOTreeNode groupNode = nodes[0];
                    if (groupNode.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        while (groupNode.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup && groupNode != null)
                        {
                            groupNode = groupNode.Parent;
                        }

                        Guid groupId = Guid.Empty;
                        if (groupNode != null)
                        {
                            groupId = new Guid(groupNode.Id);
                        }
                        var gsSetting = EXOSettingDao.LoadSharePointSetting(groupId, Guid.Empty);

                        var allSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.EXODisposalSchedule);
                        List<string> allSchedulesProfilesId = new List<string>();
                        if (allSchedules != null && allSchedules.Count != 0)
                        {
                            allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                        }

                        foreach (var node in nodes)
                        {
                            RMSampleEXOTreeNode siteNode = node;
                            while (siteNode != null && siteNode.Level != (int)NodeLevel.ExchangeOnlineMailbox)
                            {
                                siteNode = siteNode.Parent;
                            }

                            Guid siteId = Guid.Empty;
                            if (siteNode != null)
                            {
                                siteId = new Guid(siteNode.Id);
                            }
                            ArgumentCheck.NotNull(node, nameof(node));
                            var csSetting = EXOSettingDao.LoadSharePointSetting(new Guid(node.Id), siteId, true);
                            if (csSetting != null)
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            var profileId = ScheduleService.GetProfileId(node);
                            if (allSchedulesProfilesId.Contains(profileId))
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
                            var selfGSSetting = EXOSettingDao.LoadSharePointSetting(new Guid(selfGroupNode.Id), Guid.Empty);
                            if (selfGSSetting == null)
                            {
                                selfGroupNode.IconStatus = IconStatus.NoSet;
                            }
                            else
                            {
                                selfGroupNode.IconStatus = IconStatus.Break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting Icon.Error:{0}", e.ToString());
                throw;
            }
        }


        public async Task<RMEXOTreeNode> LoadExchangeNodeSettingAsync(RMSampleEXOTreeNode sNode)
        {
            var configNode = new RMEXOTreeNode();
            configNode.IconStatus = IconStatus.NoSet;
            #region copy node properties
            configNode.Id = sNode.Id;
            configNode.Name = sNode.Name;
            configNode.Title = sNode.Title;
            configNode.FullPath = sNode.FullPath;
            configNode.Level = sNode.Level;
            configNode.NodeType = sNode.NodeType;
            configNode.Expanded = sNode.Expanded;
            configNode.ChildrenCount = sNode.ChildrenCount;
            configNode.CheckNumber = sNode.CheckNumber;
            configNode.Hidden = sNode.Hidden;

            //exo new 
            configNode.GroupName = sNode.GroupName;
            configNode.MailboxType = sNode.MailboxType;
            configNode.InternalFolderPath = sNode.InternalFolderPath;
            configNode.SiteCollectionUrl = sNode.SiteCollectionUrl;
            configNode.Sender = sNode.Sender;
            configNode.SendDate = sNode.SendDate;
            configNode.DisplayTo = sNode.DisplayTo;
            configNode.Email = sNode.Email;
            configNode.Category = sNode.Category;
            configNode.HasAttachment = sNode.HasAttachment;
            configNode.OffSet = sNode.OffSet;
            configNode.SubFolderCount = sNode.SubFolderCount;

            //configNode.Parent = sNode.Parent;

            #endregion
            //TODO for load setting group / custom

            try
            {
                RMSampleEXOTreeNode groupNode = sNode;
                while (groupNode != null && groupNode.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    groupNode = groupNode.Parent;
                }
                if (groupNode == null)
                {
                    return configNode;
                }
                Guid groupId = Guid.Empty;
                bool folderDisable = false;
                if (groupNode != null)
                {
                    groupId = new Guid(groupNode.Id);
                }
                var GSetting = EXOSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                if (GSetting != null)
                {
                    configNode.IconStatus = IconStatus.Inhert;
                    var termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                    var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                    RMTermSet termSet = null;
                    if (GSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(GSetting.TermSetId);
                    }
                    configNode.TermSetId = GSetting.TermSetId;
                    configNode.TermSetName = GSetting.TermSetName;
                    configNode.TermId = GSetting.TermId;
                    configNode.TermName = GSetting.TermName;
                    configNode.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";
                    configNode.DefaultTermId = GSetting.DefaultTermId;
                    configNode.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                    configNode.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                    configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                    //configNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = GSetting.ApplyExistType;
                    //if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    //{
                    //    configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.None;
                    //}
                    configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.ExchangeOnline);
                    //configNode.ProfileId = GSetting.IdPath;
                    configNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                    if (GSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && GSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                    configNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                    configNode.IsSyncData = GSetting.IsSyncData;
                    configNode.ApprovalType = (int)GSetting.ApprovalType;
                    configNode.WorkflowReferenceId = GSetting.WorkflowReferenceId;
                    configNode.IsNullClassificationSetting = GSetting.IsNullClassificationSetting;
                    configNode.Rules = EXOSettingRuleDao.GetMappingRules(groupId);
                    if (sNode.Level == (int)NodeLevel.ExchangeOnlineMailbox || sNode.Level == (int)NodeLevel.ExchangeOnlineFolders || sNode.Level == (int)NodeLevel.ExchangeOnlineFolder || sNode.Level == (int)NodeLevel.ExchangeOnlineItems || sNode.Level == (int)NodeLevel.ExchangeOnlineItem)
                    {
                        if (GSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        else
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
                        }
                    }
                    //SetDisposeJob(configNode, GSetting.DisposalJobId);
                    //SetCollectionJob(configNode, GSetting.CollectionJobId);
                }
                RMSampleEXOTreeNode siteNode = sNode;
                while (siteNode != null && siteNode.Level != (int)NodeLevel.ExchangeOnlineMailbox)
                {
                    siteNode = siteNode.Parent;
                }

                Guid siteId = Guid.Empty;
                if (siteNode != null)
                {
                    siteId = new Guid(siteNode.Id);
                }
                var spSetting = EXOSettingDao.LoadSharePointSetting(new Guid(sNode.Id), siteId, true);
                //if (configNode.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                //{
                //    var pNode = LoadFolderParentSeting(sNode, siteId);
                //    if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                //    {
                //        if (spSetting != null)
                //        {
                //            spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                //        }
                //        folderDisable = true;
                //    }
                //}

                if (spSetting == null)
                {
                    if (sNode.Level == (int)NodeLevel.ExchangeOnlineFolders || sNode.Level == (int)NodeLevel.ExchangeOnlineFolder || sNode.Level == (int)NodeLevel.ExchangeOnlineItems || sNode.Level == (int)NodeLevel.ExchangeOnlineItem)
                    {
                        spSetting = LoadSampleNodeParentSeting(sNode.Parent, siteId);
                        if (spSetting != null && configNode.Level != (int)NodeLevel.WebApplication)
                        {
                            if (spSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                            {
                                spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                        }
                        configNode.IsCustomSetting = false;
                    }
                }
                else
                {
                    configNode.IconStatus = IconStatus.Break;
                    if (sNode.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)//Group Level 不能有CustomSetting，
                    {
                        configNode.IsCustomSetting = true;
                        configNode.IsCustomTermSetting = spSetting.TermSetId != Guid.Empty;
                    }
                }

                if (spSetting != null)
                {
                    var termScope = TermDao.GetRMTermByGuId(spSetting.TermId);
                    var defaultTerm = TermDao.GetRMTermByGuId(spSetting.DefaultTermId);
                    RMTermSet termSet = null;
                    if (spSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(spSetting.TermSetId);
                    }

                    configNode.DefaultTermId = spSetting.DefaultTermId;
                    configNode.DefaultTermName = defaultTerm == null ? spSetting.DefaultTermName : defaultTerm.Name;
                    configNode.TermScopeFullPath = spSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(spSetting.TermSetId);
                    configNode.DefaultTermFullPath = spSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.DefaultTermId) : "";
                    configNode.TermId = spSetting.TermId;
                    configNode.TermName = termScope == null ? spSetting.TermName : termScope.Name;
                    configNode.TermSetId = spSetting.TermSetId;
                    configNode.TermSetName = spSetting.TermSetName;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                    configNode.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                    configNode.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                    //configNode.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = spSetting.ApplyExistType;
                    //if (spSetting.NeedCheckDefaultValue && spSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    //{
                    //    configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                    //}
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.ExchangeOnline);
                    configNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
                    //configNode.ProfileId = spSetting.IdPath;
                    configNode.DeployTermMethod = spSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)spSetting.DeployTermMethod;
                    if (spSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && spSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = spSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = spSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)spSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)spSetting.AutoJobOption;
                    configNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                    configNode.IsSyncData = spSetting.IsSyncData;
                    configNode.ApprovalType = (int)spSetting.ApprovalType;
                    configNode.WorkflowReferenceId = spSetting.WorkflowReferenceId;
                    //SetDisposeJob(configNode, spSetting.DisposalJobId);
                    //if (sNode.Level == (int)NodeLevel.WebApplication || sNode.Level == (int)NodeLevel.SiteCollection)
                    //{
                    //    SetCollectionJob(configNode, spSetting.CollectionJobId);
                    //}
                    //else
                    //{
                    //    var tempSetting = EXOSettingDao.LoadSharePointSetting(siteId, siteId, true);
                    //    if (tempSetting != null)
                    //    {
                    //        SetCollectionJob(configNode, tempSetting.CollectionJobId);
                    //    }
                    //}
                }
                var profileId = ScheduleService.GetProfileId(sNode);
                var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.EXODisposalSchedule);

                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");

                    configNode.DisposeScheduleInfo = disposeSchedule;
                    configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMEXOTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    //configNode.IsCustomSetting = true;
                    configNode.IconStatus = IconStatus.Break;
                    //if (!configNode.IsCustomSetting && configNode.Level != (int)NodeLevel.WebApplication)
                    //{
                    //    configNode.DisposeScheduleInfo.Id = "1";
                    //}
                }
                else
                {
                    var ancestryDisposeSchedule = await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.EXODisposalSchedule);
                    if (ancestryDisposeSchedule != null)
                    {
                        var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                        ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                        ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                        configNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                        configNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                        configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMEXOTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    }
                    else
                    {
                        configNode.DisposeScheduleInfo = null;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
            return configNode;
        }

        public async System.Threading.Tasks.Task LoadExchangeNodeSettingAsync(List<RMEXOTreeNode> nodes)
        {
            try
            {
                foreach (var configNode in nodes)
                {
                    try
                    {
                        RMEXOTreeNode groupNode = configNode;
                        while (groupNode.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup && groupNode != null)
                        {
                            groupNode = groupNode.Parent;
                        }
                        if (groupNode == null)
                        {
                            continue;
                        }
                        Guid groupId = Guid.Empty;
                        if (groupNode != null)
                        {
                            groupId = new Guid(groupNode.Id);
                        }
                        var GSetting = EXOSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                        if (GSetting != null)
                        {
                            configNode.IconStatus = IconStatus.Inhert;
                            var termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                            var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);

                            configNode.TermSetId = GSetting.TermSetId;
                            configNode.TermSetName = GSetting.TermSetName;
                            configNode.TermId = GSetting.TermId;
                            configNode.TermName = GSetting.TermName;
                            configNode.DefaultTermId = GSetting.DefaultTermId;
                            configNode.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                            configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                            configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                            //configNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                            configNode.ApplyExistType = GSetting.ApplyExistType;
                            //if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                            //{
                            //    configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.None;
                            //}
                            configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                            configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.ExchangeOnline);
                            //configNode.ProfileId = GSetting.IdPath;
                            configNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                            if (GSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && GSetting.DefaultTermId == Guid.Empty)
                            {
                                configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                            }
                            configNode.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                                null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                            SetAutoTermStatus(configNode.AutoClassificationRules);
                            await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                            ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                            configNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                            configNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                            configNode.IsSyncData = GSetting.IsSyncData;
                            configNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                            if (configNode.Level == (int)NodeLevel.ExchangeOnlineMailbox || configNode.Level == (int)NodeLevel.ExchangeOnlineFolders || configNode.Level == (int)NodeLevel.ExchangeOnlineFolder || configNode.Level == (int)NodeLevel.ExchangeOnlineItems || configNode.Level == (int)NodeLevel.ExchangeOnlineItem)
                            {
                                if (GSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                                {
                                    configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                                }
                                else
                                {
                                    configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
                                }
                            }
                            //SetDisposeJob(configNode, GSetting.DisposalJobId);
                            //SetCollectionJob(configNode, GSetting.CollectionJobId);
                        }
                        RMEXOTreeNode siteNode = configNode;
                        while (siteNode != null && siteNode.Level != (int)NodeLevel.ExchangeOnlineMailbox)
                        {
                            siteNode = siteNode.Parent;
                        }

                        Guid siteId = Guid.Empty;
                        if (siteNode != null)
                        {
                            siteId = new Guid(siteNode.Id);
                        }
                        var spSetting = EXOSettingDao.LoadSharePointSetting(new Guid(configNode.Id), siteId, true);
                        //if (configNode.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                        //{
                        //    var pNode = LoadFolderParentSeting(sNode, siteId);
                        //    if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                        //    {
                        //        if (spSetting != null)
                        //        {
                        //            spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        //        }
                        //        folderDisable = true;
                        //    }
                        //}

                        if (spSetting == null)
                        {
                            configNode.IsCustomSetting = false;
                            configNode.IsCustomTermSetting = false;
                            //if (configNode.Level == (int)NodeLevel.ExchangeOnlineFolders || configNode.Level == (int)NodeLevel.ExchangeOnlineFolder || configNode.Level == (int)NodeLevel.ExchangeOnlineItems || configNode.Level == (int)NodeLevel.ExchangeOnlineItem)
                            //{
                            //    spSetting = LoadSampleNodeParentSeting(configNode.Parent, siteId);
                            //    if (spSetting != null && configNode.Level != (int)NodeLevel.WebApplication)
                            //    {
                            //        if (spSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                            //        {
                            //            spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            //        }
                            //    }
                            //    configNode.IsCustomSetting = false;
                            //}
                        }
                        else
                        {
                            configNode.IconStatus = IconStatus.Break;
                            if (configNode.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)//Group Level 不能有CustomSetting，
                            {
                                configNode.IsCustomSetting = true;
                                configNode.IsCustomTermSetting = spSetting.TermSetId != Guid.Empty;
                            }
                        }

                        if (spSetting != null)
                        {
                            var termScope = TermDao.GetRMTermByGuId(spSetting.TermId);
                            var defaultTerm = TermDao.GetRMTermByGuId(spSetting.DefaultTermId);

                            configNode.DefaultTermId = spSetting.DefaultTermId;
                            configNode.TermId = spSetting.TermId;
                            configNode.TermSetId = spSetting.TermSetId;
                            configNode.TermSetName = spSetting.TermSetName;
                            configNode.IsTermRemoved = termScope == null ? false : termScope.IsRemoved;
                            configNode.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                            configNode.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                            configNode.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                            //configNode.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                            configNode.ApplyExistType = spSetting.ApplyExistType;
                            //if (spSetting.NeedCheckDefaultValue && spSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                            //{
                            //    configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                            //}
                            configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.ExchangeOnline);
                            configNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
                            //configNode.ProfileId = spSetting.IdPath;
                            configNode.DeployTermMethod = spSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)spSetting.DeployTermMethod;
                            if (spSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && spSetting.DefaultTermId == Guid.Empty)
                            {
                                configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                            }
                            configNode.AutoClassificationRules = spSetting.AutoClassificationRules == null ?
                                null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                            SetAutoTermStatus(configNode.AutoClassificationRules);
                            await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                            ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                            configNode.RunAutoFullJob = spSetting.RunAutoFullJob;
                            configNode.AutoJobOption = (AutoJobOption)spSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)spSetting.AutoJobOption;
                            configNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                            configNode.IsSyncData = spSetting.IsSyncData;
                            //SetDisposeJob(configNode, spSetting.DisposalJobId);
                            //if (configNode.Level == (int)NodeLevel.WebApplication || configNode.Level == (int)NodeLevel.SiteCollection)
                            //{
                            //    SetCollectionJob(configNode, spSetting.CollectionJobId);
                            //}
                            //else
                            //{
                            //    var tempSetting = EXOSettingDao.LoadSharePointSetting(siteId, siteId, true);
                            //    if (tempSetting != null)
                            //    {
                            //        SetCollectionJob(configNode, tempSetting.CollectionJobId);
                            //    }
                            //}
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
        }

        public async System.Threading.Tasks.Task LoadExchangeNodeSettingUnderGroupAsync(List<RMEXOTreeNode> nodes, RMEXOTreeNode groupNode)
        {
            try
            {
                logger.Info($"Begin to load exo settings for group:{groupNode.Name} Mailbox count:{nodes.Count}");
                using (var performance = new PerformanceScope("RMSharePointSettingsService.LoadExchangeNodeSettingUnderGroup"))
                {
                    Guid groupId = Guid.Empty;
                    if (groupNode != null)
                    {
                        groupId = new Guid(groupNode.Id);
                    }
                    var GSetting = EXOSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                    RMTerm termScope = null;
                    RMTerm termDefaultValue = null;
                    bool groupTermExpired = false;
                    List<ToUserInfo> groupRecordOwner = null;
                    List<ClassificationRule> autoRules = null;
                    if (GSetting != null)
                    {
                        termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        groupRecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.ExchangeOnline);
                        if (termDefaultValue != null)
                        {
                            groupTermExpired = TermDao.IsExpiredTerm(termDefaultValue.Id);
                        }
                        if (GSetting.AutoClassificationRules != null)
                        {
                            autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                            SetAutoTermStatus(autoRules);
                            await ConvertClassificationRuleTimeZoneAsync(autoRules);
                            ConvertClassificationRuleAndOrExpression(autoRules);
                        }
                    }
                    List<RMExchangeOnlineSetting> settings;
                    using (var performance0 = new PerformanceScope("RMSharePointSettingsService.GetAllSettingsForGroup"))
                    {
                        settings = EXOSettingDao.GetAllSettingsForGroup(groupNode);
                    }
                    foreach (var configNode in nodes)
                    {
                        try
                        {
                            ArgumentCheck.NotNull(configNode, nameof(configNode));
                            RMEXOTreeNode siteNode = configNode;
                            Guid siteId = Guid.Empty;
                            if (siteNode != null)
                            {
                                siteId = new Guid(siteNode.Id);
                            }
                            var spSetting = settings.Where(s => s.ScopeId == siteId).FirstOrDefault();
                            if (spSetting == null)
                            {
                                configNode.IsCustomSetting = false;
                                if (GSetting != null)
                                {
                                    configNode.IconStatus = IconStatus.Inhert;
                                    configNode.TermSetId = GSetting.TermSetId;
                                    configNode.TermSetName = GSetting.TermSetName;
                                    configNode.TermId = GSetting.TermId;
                                    configNode.TermName = GSetting.TermName;
                                    configNode.DefaultTermId = GSetting.DefaultTermId;
                                    configNode.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                                    configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                                    configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || groupTermExpired;
                                    configNode.ApplyExistType = GSetting.ApplyExistType;
                                    configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                                    configNode.RecordOwner = groupRecordOwner;
                                    configNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                                    if (GSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && GSetting.DefaultTermId == Guid.Empty)
                                    {
                                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                                    }
                                    configNode.AutoClassificationRules = autoRules;
                                    //GSetting.AutoClassificationRules == null ?
                                    //null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                                    //SetAutoTermStatus(configNode.AutoClassificationRules);
                                    //ConvertClassificationRuleTimeZone(configNode.AutoClassificationRules);
                                    //ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                                    configNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                                    configNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                                    configNode.IsSyncData = GSetting.IsSyncData;
                                    configNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                                    if (configNode.Level == (int)NodeLevel.ExchangeOnlineMailbox || configNode.Level == (int)NodeLevel.ExchangeOnlineFolders || configNode.Level == (int)NodeLevel.ExchangeOnlineFolder || configNode.Level == (int)NodeLevel.ExchangeOnlineItems || configNode.Level == (int)NodeLevel.ExchangeOnlineItem)
                                    {
                                        if (GSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                                        {
                                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                                        }
                                        else
                                        {
                                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                configNode.IconStatus = IconStatus.Break;
                                if (configNode.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)//Group Level 不能有CustomSetting，
                                {
                                    configNode.IsCustomSetting = true;
                                    configNode.IsCustomTermSetting = spSetting.TermSetId != Guid.Empty;
                                }

                                if (spSetting != null)
                                {
                                    var mailBoxTermScope = TermDao.GetRMTermByGuId(spSetting.TermId);
                                    var mailBoxDefaultTerm = TermDao.GetRMTermByGuId(spSetting.DefaultTermId);

                                    configNode.DefaultTermId = spSetting.DefaultTermId;
                                    configNode.TermId = spSetting.TermId;
                                    configNode.TermSetId = spSetting.TermSetId;
                                    configNode.TermSetName = spSetting.TermSetName;
                                    configNode.IsTermRemoved = mailBoxTermScope == null ? false : mailBoxTermScope.IsRemoved;
                                    configNode.IsDefaultTermRemoved = mailBoxDefaultTerm == null ? false : mailBoxDefaultTerm.IsRemoved;
                                    configNode.IsTermDeprecated = mailBoxTermScope == null ? false : mailBoxTermScope.IsDeprecated || TermDao.IsExpiredTerm(mailBoxTermScope.Id);
                                    configNode.IsDefaultTermDeprecated = mailBoxDefaultTerm == null ? false : mailBoxDefaultTerm.IsDeprecated || TermDao.IsExpiredTerm(mailBoxDefaultTerm.Id);
                                    configNode.ApplyExistType = spSetting.ApplyExistType;
                                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.ExchangeOnline);
                                    configNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
                                    configNode.DeployTermMethod = spSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)spSetting.DeployTermMethod;
                                    if (spSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && spSetting.DefaultTermId == Guid.Empty)
                                    {
                                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                                    }
                                    configNode.AutoClassificationRules = spSetting.AutoClassificationRules == null ?
                                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                                    SetAutoTermStatus(configNode.AutoClassificationRules);
                                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                                    configNode.RunAutoFullJob = spSetting.RunAutoFullJob;
                                    configNode.AutoJobOption = (AutoJobOption)spSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)spSetting.AutoJobOption;
                                    configNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                                    configNode.IsSyncData = spSetting.IsSyncData;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                            throw;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
        }

        //public List<ToUserInfo> GetSettingRecordOnwers(int settingId, SourceType sourceType)
        //{
        //    var resultLilst = new List<ToUserInfo>();
        //    List<RMAccount> rmAccounts = new List<RMAccount>();
        //    if (sourceType == SourceType.SharePoint)
        //    {
        //        rmAccounts = RecordOwnerDao.GetRecordOwnerAccounts(settingId).ToList();

        //    }
        //    else if (sourceType == SourceType.FileSystem)
        //    {
        //        rmAccounts = RecordOwnerDao.GetFSRecordOwnerAccounts(settingId).ToList();

        //    }
        //    foreach (var rmAccount in rmAccounts)
        //    {
        //        var adAcc = RMSecurityUtil.ConvertToAccountDto(rmAccount);
        //        var userInfo = new ToUserInfo()
        //        {
        //            UserId = rmAccount.Id.ToString(),
        //            DisplayName = rmAccount.DisplayName,
        //        };
        //        try
        //        {
        //            ADAuthentication ADProvider = new ADAuthentication();
        //            if (ADProvider.GetADAccountInDomain(ref adAcc))
        //            {
        //                if (!string.IsNullOrEmpty(adAcc.Mail))
        //                {
        //                    userInfo.UserPrincipalName = adAcc.Mail;
        //                }
        //                else
        //                {
        //                    userInfo.UserPrincipalName = string.Empty;
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Warn("get user from ad error: {0}", e.ToString());
        //        }
        //        resultLilst.Add(userInfo);
        //    }
        //    return resultLilst;
        //}
        #endregion

        #region auto

        public bool CheckParentNodeDisable(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true)
        {
            string scopeIdString = string.Empty;
            var isDisableRecordsManagement = false;
            if (settingNode.DisposeScheduleInfo!= null && (settingNode.DisposeScheduleInfo.JobCategory == ScheduleType.SPArchiveJobSchedule || settingNode.DisposeScheduleInfo.JobCategory == ScheduleType.OneDriveArchiveJobSchedule))
            {
                isDisableRecordsManagement = false;
                //目前考虑到SO 可以在上层disable RecordManagementSetting的情况下，操作下层节点，所以注释掉这部分逻辑，如果后期逻辑变化，可以再次使用这部分逻辑
                //var spSetting = ArchiverSettingDao.LoadArchiverSettingsUnderGroup(new Guid(settingNode.GetGroupNode().Id));
                //if (settingNode.Level == (int)NodeLevel.WebApplication)
                //{
                //    return false;
                //}
                //foreach (var setting in spSetting)
                //{
                //    if (Guid.Empty == setting.SiteId)
                //    {
                //        if (setting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Disable)
                //        {
                //            isDisableRecordsManagement = true;
                //            break;
                //        }
                //    }
                //    else if (setting.SiteId.Equals(new Guid(settingNode.GetSiteCollectionNode().Id)))
                //    {
                //        if (setting.EnableArchiverManagement == (int)EnableRecordManagementSetting.Disable && !string.IsNullOrEmpty(setting.Url) && settingNode.FullPath.StartsWith(setting.Url.TrimEnd('/') + '/'))
                //        {
                //            isDisableRecordsManagement = true;
                //            break;
                //        }
                //    }
                //}
            }
            else
            {
                try
                {
                    Expression<Func<RMSharePointSetting, bool>> whereLambda = GetFilterLambda(settingNode, SPObjectId, isCheckSelfNode);
                    if (SharePointSettingDao.GetParentNode(whereLambda) != null)
                    {
                        isDisableRecordsManagement = true;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Check Parent Node Records Management error:{0}", ex.ToString());
                }
            }
            return isDisableRecordsManagement;
        }

        private Expression<Func<RMSharePointSetting, bool>> GetFilterLambda(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMSharePointSetting), "c");
            List<Expression> nodeIdExpressionList = new List<Expression>();
            var scopeIds = GetParentScopeId(settingNode, isCheckSelfNode);

            if (scopeIds != null && scopeIds.Count() > 0)
            {
                foreach (var scopeId in scopeIds)
                {
                    nodeIdExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMSharePointSetting), param, "ScopeId", scopeId));
                }
            }
            allExpressionList.Add(nodeIdExpressionList.Aggregate(Expression.OrElse));

            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMSharePointSetting), param, "EnableRecordManagement", (int)EnableRecordManagementSetting.Disable));

            if (SPObjectId == null || SPObjectId == "")
            {
                SPObjectId = Guid.Empty.ToString();
            }
            List<Expression> nodeSiteIdExpressionList = new List<Expression>();
            nodeSiteIdExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMSharePointSetting), param, "SiteId", new Guid(SPObjectId)));
            nodeSiteIdExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMSharePointSetting), param, "SiteId", Guid.Empty));
            allExpressionList.Add(nodeSiteIdExpressionList.Aggregate(Expression.OrElse));
            var groupNode = settingNode.GetGroupNode();
            if (groupNode != null)
            {
                allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMSharePointSetting), param, "SiteGroupId", new Guid(groupNode.SPObjectId)));
            }

            try
            {
                queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                return Expression.Lambda<Func<RMSharePointSetting, bool>>(queryExpr, param);
            }
            catch (Exception ex)
            {
                logger.Error("allExpressionList error:{0}", ex.ToString());
                return null;
            }
        }

        private List<Guid> GetParentScopeId(RMSPTreeNode settingNode, bool isCheckSelfNode)
        {
            List<Guid> scopeIds = new List<Guid>();
            if (isCheckSelfNode)
            {
                scopeIds.Add(new Guid(settingNode.SPObjectId));
            }
            while (settingNode.Parent != null && settingNode.Parent.SPObjectId != null)
            {
                scopeIds.Add(new Guid(settingNode.Parent.SPObjectId));
                settingNode = settingNode.Parent;
            }
            return scopeIds;
        }

        public bool CheckEXONodeDisable(RMEXOTreeNode settingNode, bool isCheckSelfNode = true)
        {
            bool checkRecordsManagement = true;
            if (isCheckSelfNode)
            {
                RMExchangeOnlineSetting exchangeOnlineSetting = EXOSettingDao.GetSettingInfoByAgentGroupId(settingNode.Id);
                if (exchangeOnlineSetting != null)
                {
                    if (exchangeOnlineSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable)
                    {
                        checkRecordsManagement = false;
                    }
                }
            }
            if (settingNode.Level == (int)NodeLevel.ExchangeOnlineMailbox)
            {
                RMExchangeOnlineSetting exchangeOnlineSetting = EXOSettingDao.GetSettingInfoByAgentGroupId(settingNode.Parent.Id);
                if (exchangeOnlineSetting != null)
                {
                    if (exchangeOnlineSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable)
                    {
                        checkRecordsManagement = false;
                    }
                }
            }
            return checkRecordsManagement;
        }

        //public string ApplySettings(JobRunBy jobRunBy)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
        private void SetPropertiesByNodeLevel(RMSPTreeNode settingNode, RMSPTreeNode siteCollectionNode)
        {
            if (settingNode.Level == (int)NodeLevel.Folder)
            {
                settingNode.FolderId = new Guid(settingNode.SPObjectId);
                settingNode.WebId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id
                                                                                   //RECO-1881
                settingNode.isEnableClassification = false;
                settingNode.DescriptionOfContainer = null;
                settingNode.IsInheritParentTerm = false;
                settingNode.TermIdOfContainer = Guid.Empty;
                settingNode.TermNameOfContainer = null;

                settingNode.FullPath = WebUtil.MakeFullUrl(siteCollectionNode.FullPath, settingNode.FullPath);
            }
            if (settingNode.Level == (int)NodeLevel.List || settingNode.Level == (int)NodeLevel.Library)
            {
                settingNode.ListId = new Guid(settingNode.SPObjectId);
                settingNode.WebId = new Guid(settingNode.Parent.Parent.SPObjectId);//set Web Id
            }
            else if (settingNode.Level == (int)NodeLevel.Site)
            {
                settingNode.WebId = new Guid(settingNode.SPObjectId);
            }
            var groupNode = GetGroupNode(settingNode);
            Guid groupId = Guid.Empty;
            if (groupNode != null)
            {
                groupId = new Guid(groupNode.SPObjectId);
                settingNode.SiteGroupId = groupId;
            }
            var GSetting = SharePointSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
            if (GSetting != null)
            {
                settingNode.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
            }
        }

        #region EXO
        public bool CheckRunningEXOSettingJob()
        {
            List<string> runningJobs = RMJobMonitorService.GetRunningEXOApplySettingJob();
            return runningJobs.Count > 0;
        }

        public RAReturnMessage ApplyEXOSettings(JobRunBy jobRunBy, bool fromTimerJobPage)
        {
            logger.Debug("start Apply EXO Settings");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.EXOApplySetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = fromTimerJobPage.ToString()
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

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditEXOTermSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveEXONodeSettingAsync(RMEXOTreeNode sNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (CheckEXONodeDisable(sNode))
                {
                    AddFilterCretiaProperty(sNode.AutoClassificationRules);
                    SetPropertiesByNodeLevel(sNode);
                    if ((NodeLevel)sNode.Level == NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        await EXOSettingDao.AddOrUpdateGlobalSettingAsync(sNode);
                    }
                    else
                    {
                        await EXOSettingDao.AddOrUpdateCustomSettingAsync(sNode, Guid.Empty);
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save EXO Node Setting DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        private string CreateEXOApplySettingJob(JobRunBy jobRunBy, string jobRunByUser, string containerId = null, JobPriority priority = JobPriority.Normal)
        {
            string jobId = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                jobId = RMJobMonitorService.CreateJob(JobType.EXOApplySetting, jobRunByUser, containerId);
                logger.Info("Begin control Apply Job {0}", jobId);
            }
            else if (jobRunBy == JobRunBy.Schedule)
            {
                jobId = RMJobMonitorService.CreateJob(JobType.EXOApplySetting, "RM_TS_RunSchedule", containerId);
                logger.Info("Begin schedule Apply Job {0}", jobId);
            }
            else
            {
                jobId = RMJobMonitorService.CreateJob(JobType.EXOApplySetting, jobRunByUser, containerId);
                logger.Info("Begin default Sync Job {0}", jobId);
            }
            if(priority != JobPriority.Normal) JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, priority).GetAwaiter().GetResult();
            return jobId;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ApplyEXOSetting, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunApplyEXOSettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, JobPriority priority = JobPriority.Normal)
        {
            string jobId = string.Empty;
            //起Job，判断是前台起Job还是Schedule起的Job      

            List<string> runningJobs = RMJobMonitorService.GetRunningEXOApplySettingJob();
            try
            {
                if (runningJobs.Count == 0)
                {
                    jobId = await StartApplyEXOSettingJobAsync(jobRunBy, jobRunByUser, priority);
                }
                else
                {
                    logger.Info(I18NEntity.GetString("RM_SS_JobSkip"));
                    var settings = GetEXOSettingCollection(jobRunBy);
                    bool hasAvailableNode = false;
                    foreach (var setting in settings)
                    {
                        RMEXOTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(setting.NodeInfo);
                        if (dbNodeInfo == null)
                        {
                            logger.Warn("Node info in {0} is null or empty", setting.Name);
                            continue;
                        }
                        var containerId = GetEXOContainerId(dbNodeInfo);
                        var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                        if (!(await IsEXOAdminAsync(account.UserId)))
                        {
                            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                            {
                                logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                                continue;
                            }
                        }
                        jobId = CreateEXOApplySettingJob(jobRunBy, jobRunByUser, containerId, priority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        hasAvailableNode = true;
                        break;
                    }
                    if (!hasAvailableNode)
                    {
                        jobId = CreateEXOApplySettingJob(jobRunBy, jobRunByUser, null, priority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        logger.Warn($"Has no available node for current user. JobId:{jobId}");
                    }
                }
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = CreateEXOApplySettingJob(jobRunBy, jobRunByUser, null, priority);
                }
                if (e.Message == I18NEntity.GetString("RM_EXO_NoAvailableSettingError"))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_EXO_NoAvailableSettingError");
                }
                else
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed);
                }

                logger.Error("real run apply exo setting job error: {0}", e.ToString());
            }
            return jobId;
        }

        private Task<bool> IsSPAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                        () =>
                        {
                            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
                        });
        }

        private Task<bool> IsEXOAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                        () =>
                        {
                            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOAdmin);
                        });
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ImportSPSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunImportSPSettingJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes)
        {
            string jobId = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = RMJobMonitorService.CreateJob(JobType.ImportSPSetting, jobRunByUser, account?.UserId);
                logger.Info("Begin control Import Term Job {0}", jobId);
            }

            logger.Info("create import SPSetting job in job monitor.Id:{0}", jobId);
            //查询当前还没有结束的Term Sync Job
            List<string> importJobs = RMJobMonitorService.GetRunningJobs(JobType.ImportSPSetting);
            bool isSkip = importJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                StartImportSPSettingJob(jobId, extension, strBytes);
            }
            else
            {
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_ImportSPSetting_JobSkip");
                logger.Info(I18NEntity.GetString("RM_ImportSPSetting_JobSkip"));
            }

            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ExportSPSOSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunExportSPSOSettingJob(JobRunBy jobRunBy, string jobRunByUser, string exportSettingType)
        {
            return await RealRunExportSettingJob(jobRunBy, jobRunByUser, exportSettingType, JobType.ExportSPSOSetting);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ExportSPSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunExportSPSettingJob(JobRunBy jobRunBy, string jobRunByUser, string exportSettingType)
        {
            return await RealRunExportSettingJob(jobRunBy, jobRunByUser, exportSettingType, JobType.ExportSPSetting);
        }

        private async Task<string> RealRunExportSettingJob(JobRunBy jobRunBy, string jobRunByUser, string exportSettingType, JobType jobType)
        {
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            string jobId = RMJobMonitorService.CreateJob(jobType, jobRunByUser, account?.UserId);
            List<string> runningJobIds = RMJobMonitorService.GetRunningJobs(jobType);
            var skip = runningJobIds.Any(j => j != jobId);
            if (!Enum.TryParse<ExportSettingType>(exportSettingType, out ExportSettingType type))
            {
                type = ExportSettingType.OnlyExportCustomSettingNodes;
            }
            if (!skip)
            {
                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = jobType == JobType.ExportSPSetting ? DownloadContentType.ExportSettings : DownloadContentType.ExportSPSOSetting,
                });
                logger.Info("Start to export share point setting");
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = jobType,
                    RunBy = jobRunBy,
                    CommandLine = string.Format("{0} {1} {2}", jobType, jobId, type),
                });
                return jobId;
            }
            else
            {
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_ExportSPSetting_SkipJob");
                return "";
            }
        }

        private void StartImportSPSettingJob(string jobId, string extension, string strBytes)
        {
            string content = "\"" + strBytes + "\"";
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ImportSPSetting,
                CommandLine = string.Format("{0} {1} {2} {3}", JobType.ImportSPSetting, jobId, extension, content),
            });
        }
        private List<RMExchangeOnlineSetting> GetEXOSettingCollection(JobRunBy runBy)
        {
            List<RMExchangeOnlineSetting> allSettings = null;
            if (runBy == JobRunBy.Control)
            {
                //Part job by node.
                allSettings = EXOSettingDao.LoadRunJobSetting();
                logger.Info("Start Job Settings.");
                if (allSettings.Count == 0)
                {
                    logger.Info("apply full exchange online setting job");
                    allSettings = EXOSettingDao.LoadAllSettingForAS();
                }
            }
            else
            {
                //Full job
                allSettings = EXOSettingDao.LoadAllSettingForAS();
            }
            return allSettings;
        }

        private async Task<string> StartApplyEXOSettingJobAsync(JobRunBy runBy, string jobRunByUser, JobPriority priority = JobPriority.Normal)
        {
            //Get settings jobs
            //browser tree start sub job..
            //Create sub job detail..
            string jobId = string.Empty;
            List<RMExchangeOnlineSetting> allSettings = GetEXOSettingCollection(runBy);
            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No exchange online setting node found.");
                throw new Exception(I18NEntity.GetString("RM_EXO_NoAvailableSettingError"));
            }
            Dictionary<Guid, RMExchangeOnlineSetting> gruopSettingMap = new Dictionary<Guid, RMExchangeOnlineSetting>();
            //Dictionary<Guid, int> nodeSettingMap = new Dictionary<Guid, int>();
            var excludeSiteNodes = EXOSettingDao.LoadExcludeSiteCollectionSetting();
            List<Guid> excludeMailboxIds = new List<Guid>();
            foreach (var setting in excludeSiteNodes)
            {
                RMEXOTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(setting.NodeInfo);
                if (dbNodeInfo == null)
                {
                    logger.Warn("Node info in {0} is null or empty", setting.Name);
                    continue;
                }
                if (dbNodeInfo.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    var mailBox = MailBoxService.GetMailboxById(dbNodeInfo.Id);
                    if (mailBox == null || !mailBox.ParentId.Equals(setting.GroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Mailbox is null or has been moved to other group, id:{0}. Will not add to exclude list", dbNodeInfo.Id);
                        continue;
                    }
                }

                excludeMailboxIds.Add(setting.ScopeId);
            }
            List<RMEXOTreeNode> availableMailbox = new List<RMEXOTreeNode>();
            Dictionary<string, List<RMEXOTreeNode>> groupNodes = new Dictionary<string, List<RMEXOTreeNode>>();
            Dictionary<string, string> emptyContainers = new Dictionary<string, string>();
            Dictionary<string, string> EXORuleSettingContainers = new Dictionary<string, string>();
            //TODO? 
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            foreach (RMExchangeOnlineSetting setting in allSettings)
            {
                RMEXOTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(setting.NodeInfo);
                if (dbNodeInfo == null)
                {
                    logger.Warn("Node info in {0} is null or empty", setting.Name);
                    continue;
                }
                var group = MailBoxDao.GetEmailGroupById(setting.GroupId.ToString());
                if (group == null)
                {
                    await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                    logger.Warn("Mailbox group is null, name:{0}", dbNodeInfo.Name);
                    continue;
                }
                var containerId = GetEXOContainerId(dbNodeInfo);
                //Group设置Null Classification Setting，不处理当前Group以及Group下的Mailbox，即使Mailbox有打破继承的Term Setting
                var groupSetting = allSettings.Where(x => x.ScopeId == setting.GroupId);
                if (setting.IsNullClassificationSetting || groupSetting != null && groupSetting.Count() > 0 && groupSetting.First().IsNullClassificationSetting)
                {
                    logger.Warn("Apply Setting IsNullClassificationSetting or groupSetting IsNullClassificationSetting, name:{0}.IsNullClassificationSetting:{1}.", dbNodeInfo.Name, setting.IsNullClassificationSetting);
                    if (dbNodeInfo.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup && !EXORuleSettingContainers.ContainsKey(containerId))
                    {
                        EXORuleSettingContainers.Add(containerId, GetEXOContainerName(dbNodeInfo));
                        //EXO Rule Setting需要更新SettingTime，避免影响Full Job.
                        await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                    }
                    continue;
                }
                var isAdmin = await IsEXOAdminAsync(account.UserId);
                if (!isAdmin)
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                        continue;
                    }
                }
                List<RMEXOTreeNode> tempMailbox = new List<RMEXOTreeNode>();
                if (dbNodeInfo.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    List<RMEXOTreeNode> mailboxs = RMSPTreeService.BrowseExchangeTree(dbNodeInfo);
                    var totalMailBoxCount = mailboxs.Count;
                    var hasCustomMailboxCount = 0;
                    logger.Info("Group:{0} mailbox count is {1}", dbNodeInfo.Name, mailboxs.Count);
                    if (mailboxs.Count > 0)
                    {
                        foreach (RMEXOTreeNode mailbox in mailboxs)
                        {
                            if (excludeMailboxIds.Contains(new Guid(mailbox.Id)))
                            {
                                logger.Info("Exclude mailbox Id{0}", mailbox.Id);
                                hasCustomMailboxCount++;
                            }
                            else
                            {
                                tempMailbox.Add(mailbox);
                                //if (!nodeSettingMap.ContainsKey(dbNodeInfo.SettingScopeId))//TODO debug  mailbox.SettingScopeId
                                //{
                                //    nodeSettingMap.Add(mailbox.SettingScopeId, setting.Id);
                                //}
                            }

                            if (!gruopSettingMap.ContainsKey(new Guid(dbNodeInfo.Id)))
                            {
                                gruopSettingMap.Add(new Guid(dbNodeInfo.Id), setting);
                            }
                        }
                    }
                    else
                    {
                        if (!emptyContainers.ContainsKey(containerId))
                        {
                            emptyContainers.Add(containerId, GetEXOContainerName(dbNodeInfo));
                        }
                    }
                    if (totalMailBoxCount == hasCustomMailboxCount)
                    {
                        //update group node setting
                        await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                        //EXOSettingDao.SetSettingInfo(new Guid(dbNodeInfo.Id), DateTime.UtcNow.Ticks, false);
                    }
                }
                else
                {
                    GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object.EmailAccountDto testMailbox = null;
                    try
                    {
                        //DAOAPIClientV1 client = new DAOAPIClientV1();
                        //testMailbox = client.GetExchangeNodeById(dbNodeInfo.Id);
                        testMailbox = MailBoxService.GetMailboxById(dbNodeInfo.Id);
                    }
                    catch (Exception e)
                    {
                        logger.Error("get exo node error:{0}", e.ToString());
                    }
                    if (testMailbox != null)
                    {
                        if (!testMailbox.ParentId.Equals(setting.GroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            logger.Info("Mailbox has been moved to other group, name:{0}", dbNodeInfo.Name);
                            await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                            continue;
                        }
                        tempMailbox.Add(dbNodeInfo);
                        if (!gruopSettingMap.ContainsKey(new Guid(dbNodeInfo.Id)))
                        {
                            gruopSettingMap.Add(new Guid(dbNodeInfo.Id), setting);
                        }
                    }
                    else
                    {
                        logger.Warn("Mailbox is null, name:{0}", dbNodeInfo.Name);
                        await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                    }
                }
                if (tempMailbox.Count > 0)
                {
                    if (groupNodes.ContainsKey(containerId))
                    {
                        groupNodes[containerId].AddRange(tempMailbox);
                    }
                    else
                    {
                        groupNodes.Add(containerId, tempMailbox);
                    }
                }
            }
            if (EXORuleSettingContainers.Count > 0)
            {
                foreach (var container in EXORuleSettingContainers)
                {
                    jobId = CreateEXOApplySettingJob(runBy, jobRunByUser, container.Key, priority);
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{container.Value}");
                }
            }
            if (groupNodes.Count > 0)
            {
                var allGroupSettings = EXOSettingDao.LoadAllGroupSettings();
                foreach (var group in groupNodes)
                {
                    jobId = CreateEXOApplySettingJob(runBy, jobRunByUser, group.Key, priority);
                    var parentGroupSetting = allGroupSettings.FirstOrDefault(g => group.Key?.ToLowerInvariant() == g.GroupId.ToString()?.ToLowerInvariant());
                    if (parentGroupSetting.IsNullClassificationSetting)
                    {
                        logger.Warn($"Mail Box is skip, becuase container is null classification setting. mail box ids: {string.Join(",", group.Value.Select(m => m.Id))}");
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{parentGroupSetting.Name}");
                    }
                    else
                    {
                        SeperateSubJobForApplyEXOSetting(group.Value, gruopSettingMap, jobId, runBy, JobType.EXOApplySetting);
                    }
                }
            }
            else
            {
                if (emptyContainers.Count > 0)
                {
                    foreach (var container in emptyContainers)
                    {
                        jobId = CreateEXOApplySettingJob(runBy, jobRunByUser, container.Key, priority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, $"RM_EXO_NoMailboxUnderGroup{I18NEntity.Separator}{container.Value}");
                    }
                }
                else
                {
                    logger.Warn("No exchange online setting node group found.");
                    throw new Exception(I18NEntity.GetString("RM_EXO_NoAvailableSettingError"));
                }
            }
            return jobId;
        }

        private void SeperateSubJobForApplyEXOSetting(List<RMEXOTreeNode> availableSites, Dictionary<Guid, RMExchangeOnlineSetting> gruopSetingMap, string jobId, JobRunBy runBy, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMEXOTreeNode> tempList = new List<RMEXOTreeNode>();
            int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (RMEXOTreeNode site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = runBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                logger.Debug("Create and queue sub job {0}", subJobId);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = runBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMEXOTreeNode> tempList, bool sendNow, Dictionary<Guid, RMExchangeOnlineSetting> gruopSetingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            if (gruopSetingMap != null)
            {
                subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(gruopSetingMap);
            }
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        private string CreateSubJobForEXODisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMEXOTreeNode> tempList, bool sendNow, string runJobScope, string o365TenantId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList)};
            subJob.String1 = runJobScope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        private void SetPropertiesByNodeLevel(RMEXOTreeNode settingNode)
        {
            Guid groupId = Guid.Empty;
            var groupNode = GetGroupNode(settingNode);
            if (groupNode != null)
            {
                groupId = new Guid(groupNode.Id);
            }
            settingNode.GroupId = groupId;

            if (settingNode.Level == (int)NodeLevel.ExchangeOnlineMailbox)
            {
                settingNode.MailBoxId = new Guid(settingNode.Id);
                //settingNode.SettingScopeId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                //settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id

            }
            else if (settingNode.Level == (int)NodeLevel.ExchangeOnlineFolder)
            {
                //settingNode.Id = new Guid(settingNode.Id);
                //settingNode.SettingScopeId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                //settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id

            }
            else if (settingNode.Level == (int)NodeLevel.ExchangeItem)
            {
                //settingNode.Id = new Guid(settingNode.Id);
                //settingNode.SettingScopeId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                //settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id

            }
            //RECO-1881
            settingNode.isEnableClassification = false;
            settingNode.DescriptionOfContainer = null;
            settingNode.TermIdOfContainer = Guid.Empty;
            settingNode.TermNameOfContainer = null;
        }

        public RMEXOTreeNode GetGroupNode(RMEXOTreeNode node)
        {
            while (node.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMEXOTreeNode GetMailboxNode(RMEXOTreeNode node)
        {
            while (node.Level != (int)NodeLevel.ExchangeOnlineMailbox)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMEXOTreeNode GetFolderNode(RMEXOTreeNode node)
        {
            while (node.Level != (int)NodeLevel.ExchangeOnlineFolder)
            {
                node = node.Parent;
            }
            return node;
        }

        public RMExchangeOnlineSetting LoadSampleNodeParentSeting(RMSampleEXOTreeNode node, Guid siteId)
        {
            RMExchangeOnlineSetting exoSetting = null;
            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return exoSetting;
            }

            //TODO xwwang
            if (node.Level == (int)NodeLevel.ExchangeOnlineMailbox || node.Level == (int)NodeLevel.ExchangeOnlineFolders || node.Level == (int)NodeLevel.ExchangeOnlineFolder || node.Level == (int)NodeLevel.ExchangeOnlineItems || node.Level == (int)NodeLevel.ExchangeOnlineItem)
            //if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                exoSetting = EXOSettingDao.LoadSharePointSetting(new Guid(node.Id), siteId, true);
            }
            if (exoSetting == null)
            {
                exoSetting = LoadSampleNodeParentSeting(node.Parent, siteId);
            }
            return exoSetting;
        }

        public async Task<RAReturnMessage> AddEnableColumnSettingAsync(RMEXOTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if ((NodeLevel)settingNode.Level == NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                    SetPropertiesByNodeLevel(settingNode);
                    await EXOSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                }
                else
                {
                    if (CheckEXONodeDisable(settingNode, false))
                    {
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                        SetPropertiesByNodeLevel(settingNode);
                        await EXOSettingDao.AddOrUpdateCustomSettingAsync(settingNode, Guid.Empty);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                string nodeProfileIdPath = ScheduleService.GetProfileId(settingNode);
                EXOSettingDao.RemoveDescendantsSetting(settingNode, nodeProfileIdPath);
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditEXOLocationOwnersSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddEXOLocationOwnersAsync(RMEXOTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            result.MessageType = RAMessageType.Successful;
            try
            {
                if (CheckEXONodeDisable(settingNode))
                {
                    logger.Info("Set location owners EXO Setting");
                    SetPropertiesByNodeLevel(settingNode);
                    if (settingNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        await EXOSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                    }
                    else
                    {
                        logger.Info("Set location owners EXO Setting current node save term as group : {0}", settingNode.FullPath);
                        await EXOSettingDao.AddOrUpdateCustomSettingAsync(settingNode, Guid.Empty);
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Set location owners EXO Setting DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        //public RAReturnMessage AddEXONodeSettingDisposeSchedule(RMEXOTreeNode settingNode, bool isRemove = false)
        //{
        //    RAReturnMessage result = new RAReturnMessage();
        //    result.MessageType = RAMessageType.Successful;
        //    try
        //    {
        //        if (CheckEXONodeDisable(settingNode))
        //        {
        //            logger.Info("Set Schedule EXO Setting");
        //            SetPropertiesByNodeLevel(settingNode);
        //            if (isRemove)
        //            {
        //                EXOSettingDao.AddOrUpdateScheduleSetting(settingNode, settingNode.DisposeScheduleInfo.ProfileId, string.Empty, SharePointSettingScheduleType.Dispose, isRemove);
        //            }
        //            else
        //            {
        //                EXOSettingDao.AddOrUpdateScheduleSetting(settingNode, settingNode.DisposeScheduleInfo.ProfileId, settingNode.DisposeScheduleInfo.Id, SharePointSettingScheduleType.Dispose);
        //            }
        //        }
        //        else
        //        {
        //            result.MessageType = RAMessageType.Failed;
        //            result.FaildType = RAFailedType.DisableRecordsManagement;
        //            result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
        //            return result;
        //        }
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("Save Schedule Setting to DB Error {0}", ex.ToString());
        //        result.MessageType = RAMessageType.Failed;
        //        result.ErrorMessage = ex.ToString();
        //        return result;
        //    }
        //}

        //public void AddEXONodeSettingCollectionSchedule(RMEXOTreeNode node, bool isRemove = false)
        //{
        //    throw new NotImplementedException();
        //}

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditEXOInheritSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async System.Threading.Tasks.Task InheritParentEXOSettingAsync(RMEXOTreeNode node)
        {
            try
            {
                logger.Info("Inherit Parent Settings");
                var mailboxNode = GetMailboxNode(node);
                await EXOSettingDao.DeleteSharePointSettingAsync(new Guid(node.Id), new Guid(mailboxNode.Id));

                //if (node.DisposeScheduleInfo != null)
                //{
                //    RMScheduleDao.DeleteSchedule(node.DisposeScheduleInfo.Id);
                //}
                //if (node.CollectionScheduleInfo != null)
                //{
                //    RMScheduleDao.DeleteSchedule(node.CollectionScheduleInfo.Id);
                //}
                await CleanParentNodeSettingAsync(node);
                //Update the parent node setting to inherit settings. to do next.
            }
            catch (Exception ex)
            {
                logger.Warn("Inherit Parent Setting to DB Error {0}", ex.ToString());
            }
        }
        public async System.Threading.Tasks.Task CleanParentNodeSettingAsync(RMEXOTreeNode node)
        {
            do
            {
                if (await EXOSettingDao.CleanSettingJobTimeAsync(node))
                {
                    break;
                }
                node = node.Parent;
            }
            while (node != null);
        }

        public RAReturnMessage RunEXODataSyncJob(RMEXOTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start all data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                //selectedTree is null start by Timer Page run now;
                //selectedTree is not null start by Content Repository Management;
                if (selectedTree != null)
                {
                    if (!IsExistCanRunJobNodes(selectedTree))
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.FaildType = RAFailedType.None;
                        msg.ErrorMessage = I18NEntity.GetString("RM_JM_EXO_SyncData_NoSC");
                        logger.Warn("no exo node is available to sync data.");
                        return msg;
                    }
                }

                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.EXODataSynchronisation,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while EXO DataSync,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public RAReturnMessage RunEXORecordsDisposalJob(RMEXOTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start exo disposal.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                //selectedTree is null start by Timer Page run now;
                //selectedTree is not null start by Content Repository Management;
                if (selectedTree != null)
                {
                    List<JobType> types = new List<JobType>() { JobType.EXORecordsDisposal };
                    if (RMJobMonitorService.HasRunningArchiverJobOnScope(types, selectedTree.Name))
                    {
                        msg.MessageType = RAMessageType.Failed;
                        //此处的提示信息与EXO使用同一个
                        msg.ErrorMessage = I18NEntity.GetString("RM_Job_ScheduledJobConflict");
                        logger.Warn($"Already has a job running on current node:{selectedTree.Name}");
                        return msg;
                    }
                }

                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.EXORecordsDisposal,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while EXO DataSync,ERROR:{0}", ex.ToString());
            }

            return msg;
        }


        public async Task<RAReturnMessage> AddIsSyncSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (settingNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString()))
                    {
                        SharePointSettingDao.UpdateBCSColumnName(settingNode.SiteGroupId, settingNode.ColumnName, settingNode.Description, settingNode.ColumnRequired, settingNode.ColumnHidden);
                        await SharePointSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                        SharePointSettingDao.FlagCustomSettingNewColumn(settingNode.SiteGroupId);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    RMSPTreeNode siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                    {

                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.SharePoint);
                        await SharePointSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                //SharePointSettingDao.RemoveDescendantsSetting(settingNode);
                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.GeneralSetting4SPO, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddGeneralSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage enableResult = await AddEnableColumnSettingAsync(settingNode);
            RAReturnMessage isSyncResult = new RAReturnMessage();
            if (settingNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                isSyncResult = await AddIsSyncSettingAsync(settingNode);
            }
            RAReturnMessage result = new RAReturnMessage();
            if (enableResult.MessageType == RAMessageType.Failed)
            {
                result = enableResult;
            }
            else if (isSyncResult.MessageType == RAMessageType.Failed)
            {
                result = isSyncResult;
            }
            else
            {
                result.MessageType = RAMessageType.Successful;
            }
            return result;
        }


        public async Task<RAReturnMessage> AddIsSyncEXOSettingAsync(RMEXOTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            result.MessageType = RAMessageType.Successful;
            try
            {
                if (CheckEXONodeDisable(settingNode))
                {
                    SetPropertiesByNodeLevel(settingNode);
                    if ((NodeLevel)settingNode.Level == NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                        await EXOSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                    }
                    else
                    {
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                        await EXOSettingDao.AddOrUpdateCustomSettingAsync(settingNode, Guid.Empty);
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.GeneralSetting4EXO, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddEXOGeneralSettingAsync(RMEXOTreeNode settingNode)
        {
            RAReturnMessage enableResult = await AddEnableColumnSettingAsync(settingNode);
            RAReturnMessage isSyncResult = new RAReturnMessage();
            if (settingNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                isSyncResult = await AddIsSyncEXOSettingAsync(settingNode);
            }
            RAReturnMessage result = new RAReturnMessage();
            if (enableResult.MessageType == RAMessageType.Failed)
            {
                result = enableResult;
            }
            else if (isSyncResult.MessageType == RAMessageType.Failed)
            {
                result = isSyncResult;
            }
            else
            {
                result.MessageType = RAMessageType.Successful;
            }
            return result;
        }



        /// <summary>
        /// 验证:是否存在可以运行Job的节点
        /// </summary>
        /// <param name="selectedNode"></param>
        /// <returns></returns>
        private bool IsExistCanRunJobNodes(RMEXOTreeNode selectedNode)
        {
            if (selectedNode != null)
            {
                if (IsEnableRecoredManagement(selectedNode) /*&& IsHaveAvailableNodes(selectedNode)*/)
                {
                    return true;
                }
            }
            return false;
        }

        /*private async Task<bool> IsHaveAvailableNodesAsync(RMEXOTreeNode selectedNode)
        {
            List<RMEXOTreeNode> lstAvailableNodes = await AssembleSyncDataRunnableNodeAsync(selectedNode);
            if (lstAvailableNodes != null && lstAvailableNodes.Count() > 0)
            {
                logger.Info("IsHaveAvailableNodes:true");
                return true;
            }
            logger.Info("IsHaveAvailableodes:false");
            return false;
        }*/
        private bool IsEnableRecoredManagement(RMEXOTreeNode selectedNode)
        {
            RMExchangeOnlineSetting setting = null;
            var treeNode = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(selectedNode);

            //var mailboxId = Guid.Empty;
            var scopeId = Guid.Parse(treeNode.ID);
            var groupId = Guid.Parse(TreeManagement.GetGroupNode(treeNode).ID);

            setting = EXOSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, scopeId);
            if (setting == null && selectedNode.Level == (int)NodeLevel.ExchangeOnlineMailbox)
            {
                //如果SiteCollection结点没有Setting,则找父结点Group.
                setting = EXOSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
            }

            if (setting != null && setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                logger.Info("EnableRecoredManagement:true");
                return true;
            }
            logger.Info("EnableRecoredManagement:false");
            return false;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunEXODisposalJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<string> RealRunEXORecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.EXORecordsDisposal;
            RMEXOTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(param);
            return RunEXORecordDisposalJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.ApprovalProcessConfig, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunEXORecordsDisposalJobForApprovalAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            string jobid = string.Empty;
            try
            {
                JobType jobType = JobType.EXORecordsDisposal;
                List<RMEXOTreeNode> selectedNode = new List<RMEXOTreeNode>();
                var exoSettings = EXOSettingDao.LoadExchangeOnlineGroupSetting();
                if (exoSettings != null && exoSettings.Count > 0)
                {
                    foreach (var temp in exoSettings)
                    {
                        selectedNode.Add(SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(temp.NodeInfo));
                    }
                    jobid = await RunEXORecordDisposalJobBySelectdNodeForApprovalAsync(jobRunByUser, jobType, selectedNode);
                }
            }
            catch (Exception e)
            {
                logger.Error($"RealRunEXORecordsDisposalJobForApprovalAsync error:{e}");
            }
            return jobid;
        }
        public async Task<string> RunEXORecordDisposalJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMEXOTreeNode selectedNode)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            jobId = RMJobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, selectedNode.Name, GetEXOContainerId(selectedNode));
            List<RMEXOTreeNode> availableNode = new List<RMEXOTreeNode>();
            try
            {
                availableNode = await AssembleDisposalRunnableNodeAsync(selectedNode);
                if (availableNode.IsNullOrEmpty())
                {
                    logger.Warn("No available sc to run");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoMailboxUnderGroup");
                    return jobId;
                }
                List<JobType> types = new List<JobType>() { JobType.EXORecordsDisposal };
                var scopes = RMJobMonitorService.GetRunningArchiverJobsScopes(types);
                if (availableNode.Count == 1)
                {
                    if (scopes.Contains(selectedNode.Name))
                    {
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        return jobId;
                    }
                    if (scopes.Contains(availableNode.First().Name))
                    {
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        return jobId;
                    }
                }
                else
                {
                    //TODO
                    availableNode = availableNode.Where(n => !scopes.Contains(n.Name)).ToList();
                    if (availableNode.Count == 0)
                    {
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "^All sub nodes under the specified node has job running, and this job is skipped.");
                        return jobId;
                    }
                }

                var groupId = new Guid(availableNode.FirstOrDefault().GetMailboxGroupNode().Id);
                var rules = ArchiverRuleService.GetEXORuleCollection(groupId, CheckIsNullClassificationSetting(selectedNode, groupId)).Values;
                RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, rules.Select(r => new Guid(r.Id)).ToList());
            }
            catch (Exception ex)
            {
                logger.Error("AssembleDisposalRunnableNodeAsync error:{0}", ex.ToString());
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }

            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            RMJobMonitorService.SetSumSCCountOfJobExtension(subJobCount, jobId);

            int currentSubjobIndex = 0;
            List<RMEXOTreeNode> tempList = new List<RMEXOTreeNode>();            
            foreach (RMEXOTreeNode site in availableNode)
            {
                tempList.Add(site);               
                string subJobId = CreateSubJobForEXODisposal(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, site.Name, site.O365TenantId);
                if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    logger.Info("Start to send o365 {0} to high level queue", subJobId);
                    JobQueueService.HandleO365Message(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", JobType.EXORecordsDisposal, subJobId),
                    });
                }
                tempList.Clear();
                currentSubjobIndex++;
            }
            return jobId;
        }
        
        private async Task<string> RunEXORecordDisposalJobBySelectdNodeForApprovalAsync(string jobRunByUser, JobType jobType, List<RMEXOTreeNode> selectedNode)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            
            List<RMEXOTreeNode> availableNode = new List<RMEXOTreeNode>();
            foreach (var node in selectedNode)
            {
                var tempNode = await AssembleDisposalRunnableNodeForApprovalAsync(node);
                if (tempNode != null && tempNode.Count > 0)
                {
                    availableNode.AddRange(tempNode);
                }
            }
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available exo to run");
                return jobId;
            }
            jobId = RMJobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, "RM_EXO_Virtual_Container", JobType.EXORecordsDisposal.ToString());
            List<JobType> types = new List<JobType>() { JobType.EXORecordsDisposal };
            var scopes = RMJobMonitorService.GetRunningArchiverJobsScopes(types);
            if (availableNode.Count == 1)
            {
                if (scopes.Contains(availableNode.FirstOrDefault().Name))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                    return jobId;
                }
            }
            else
            {
                //TODO
                availableNode = availableNode.Where(n => !scopes.Contains(n.Name)).ToList();
                if (availableNode.Count == 0)
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "^All sub nodes under the specified node has job running, and this job is skipped.");
                    return jobId;
                }
            }

            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            RMJobMonitorService.SetSumSCCountOfJobExtension(subJobCount, jobId);
            List<GCommon.Contract.StorageOptimization.Object.Rule> rules = new List<GCommon.Contract.StorageOptimization.Object.Rule>();
            foreach (var tempNode in selectedNode)
            {
                var groupId = new Guid(tempNode.Id);
                rules.AddRange(ArchiverRuleService.GetEXORuleCollection(groupId, CheckIsNullClassificationSetting(tempNode, groupId)).Values);
            }
            RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, rules.Select(r => new Guid(r.Id)).ToList());
            int currentSubjobIndex = 0;
            List<RMEXOTreeNode> tempList = new List<RMEXOTreeNode>();
            foreach (RMEXOTreeNode site in availableNode)
            {
                tempList.Add(site);
                string subJobId = CreateSubJobForEXODisposal(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, site.Name, site.O365TenantId);
                if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    logger.Info("Start to send o365 {0} to high level queue", subJobId);
                    JobQueueService.HandleO365Message(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", JobType.EXORecordsDisposal, subJobId),
                    });
                }
                tempList.Clear();
                currentSubjobIndex++;
            }
            return jobId;
        }
        private bool CheckIsNullClassificationSetting(RMEXOTreeNode treeNode, Guid groupId)
        {
            bool isNullClassificationSetting = false;
            RMExchangeOnlineSetting currentNodeTermSetting = null;
            if (treeNode.Level == (int)NodeLevel.ExchangeOnlineMailbox)
            {
                currentNodeTermSetting = EXOSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, new Guid(treeNode.Id));
            }
            if (treeNode.IsNullClassificationSetting)
            {
                if (currentNodeTermSetting == null)
                {
                    isNullClassificationSetting = true;
                }
                else if (currentNodeTermSetting != null && currentNodeTermSetting.TermSetId == Guid.Empty)
                {
                    isNullClassificationSetting = true;
                }
            }
            return isNullClassificationSetting;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4EXO, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunEXODataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.EXODataSynchronisation;
            if (string.IsNullOrEmpty(param))
            {
                return await RunEXODataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
            else
            {
                RMEXOTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(param);
                return await RunEXODataSyncJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }

        private async Task<string> RunEXODataSyncJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMEXOTreeNode selectedNode)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            jobId = RMJobMonitorService.CreateJob(jobType, jobRunByUser, GetEXOContainerId(selectedNode));
            List<RMEXOTreeNode> availableNode = new List<RMEXOTreeNode>();
            try
            {
                availableNode = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoMailboxUnderGroup");
                return jobId;
            }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while assembling runnable node. ERROR:{0}", ex.ToString());
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
            
            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            List<RMEXOTreeNode> tempList = new List<RMEXOTreeNode>();
            foreach (RMEXOTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }
            return jobId;
        }

        private string GetEXOContainerId(RMEXOTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                return selectedNode.Id;
            }
            else
            {
                return GetEXOContainerId(selectedNode.Parent);
            }
        }

        private string GetEXOContainerName(RMEXOTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                return DefaultSecurityContainerNameHelper.GetI18NName(selectedNode.Name);
            }
            else
            {
                return GetEXOContainerName(selectedNode.Parent);
            }
        }

        private async Task<List<RMEXOTreeNode>> AssembleSyncDataRunnableNodeAsync(RMEXOTreeNode selectedNode)
        {
            List<RMEXOTreeNode> availableNode = new List<RMEXOTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                List<RMEXOTreeNode> mailboxs = RMSPTreeService.BrowseExchangeTree(selectedNode);
                if (mailboxs.IsNullOrEmpty())
                {
                    return availableNode;
                }
                await LoadExchangeNodeSettingUnderGroupAsync(mailboxs, selectedNode);
                //this.LoadExchangeNodeSetting(mailboxs);
                foreach (RMEXOTreeNode mailbox in mailboxs)
                {
                    if (mailbox.IsSyncData && mailbox.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable) //RECO-3282                   
                    {
                        availableNode.Add(mailbox);
                    }
                }
            }
            else
            {
                if (ValidateMailboxExist(selectedNode))
                {
                    if (selectedNode.IsSyncData && selectedNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        availableNode.Add(selectedNode);
                    }
                }
                else
                {
                    logger.Info("Mailbox not exist, name:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        private async Task<List<RMEXOTreeNode>> AssembleDisposalRunnableNodeAsync(RMEXOTreeNode selectedNode)
        {
            List<RMEXOTreeNode> availableNode = new List<RMEXOTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                List<RMEXOTreeNode> mailboxs = RMSPTreeService.BrowseExchangeTree(selectedNode);
                if (mailboxs.IsNullOrEmpty())
                {
                    return availableNode;
                }
                var breakNodes = ArchiverRuleService.BuildBreakTreeNode(selectedNode);
                await LoadExchangeNodeSettingUnderGroupAsync(mailboxs, selectedNode);
                //this.LoadExchangeNodeSetting(mailboxs);
                foreach (RMEXOTreeNode mailbox in mailboxs)
                {
                    mailbox.IsNullClassificationSetting = selectedNode.IsNullClassificationSetting;
                    if (breakNodes != null && breakNodes.Count > 0 && breakNodes.Any(m => m.NodeId == mailbox.Id || m.NodeName == mailbox.Name))
                    {
                        logger.Info("Mailbox is break inheriting node, name:{0}", mailbox.Id);
                        continue;
                    }
                    if (mailbox.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable) //RECO-3282                   
                    {
                        availableNode.Add(mailbox);
                    }
                }
            }
            else
            {
                if (ValidateMailboxExist(selectedNode))
                {
                    if (selectedNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        availableNode.Add(selectedNode);
                    }
                }
                else
                {
                    logger.Info("Mailbox not exist, name:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }
        private async Task<List<RMEXOTreeNode>> AssembleDisposalRunnableNodeForApprovalAsync(RMEXOTreeNode selectedNode)
        {
            List<RMEXOTreeNode> availableNode = new List<RMEXOTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                List<RMEXOTreeNode> mailboxs = RMSPTreeService.BrowseExchangeTree(selectedNode);
                if (mailboxs.IsNullOrEmpty())
                {
                    return availableNode;
                }
                await LoadExchangeNodeSettingUnderGroupAsync(mailboxs, selectedNode);
                foreach (RMEXOTreeNode mailbox in mailboxs)
                {
                    try
                    {
                        var exsitApproval = explorerDao.Exist(e => e.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved && e.RecordStatus != (int)RMRecordStatus.RMDeleted && e.ManualArchiveStatus == (int)ActionStatus.None && e.EmailAddress.Equals(mailbox.Name));
                        if (exsitApproval)
                        {
                            logger.Info($"Get mailbox manual approved success,email:{mailbox.Name}");
                            mailbox.IsNullClassificationSetting = selectedNode.IsNullClassificationSetting;
                            if (mailbox.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable) //RECO-3282                   
                            {
                                mailbox.IsProcessApprovalDatasOnly = true;
                                availableNode.Add(mailbox);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Get mailbox manual approved status error:{e}");
                    }
                }
            }
            return availableNode;
        }
        private bool ValidateMailboxExist(RMEXOTreeNode selectedNode)
        {
            GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object.EmailAccountDto testMailbox = null;
            try
            {
                //DAOAPIClientV1 client = new DAOAPIClientV1();
                //testMailbox = client.GetExchangeNodeById(dbNodeInfo.Id);
                testMailbox = MailBoxService.GetMailboxById(selectedNode.Id);
                selectedNode.O365TenantId = testMailbox.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get exo node error:{0}", e.ToString());
            }
            return testMailbox != null ? true : false;
        }

        #region remove code
        //private void DealWithExcludeNodeList(RMEXOTreeNode selectedTree, List<ExcludeNode> excludeNodeList)
        //{
        //    if (selectedTree != null)
        //    {
        //        int nodeCount = 0;
        //        var siteMap = this.GetTotalRMEXOTreeNode(new List<RMEXOTreeNode>() { selectedTree }, ref nodeCount);
        //        if (nodeCount > 0)
        //        {
        //            this.LoadExchangeNodeSetting(siteMap[selectedTree.Id]);
        //            foreach (var sitecollection in siteMap[selectedTree.Id])
        //            {
        //                if (sitecollection.IsCustomSetting)
        //                {
        //                    var tempNode = new ExcludeNode();
        //                    //tempNode.AveSiteId = sitecollection.SPObjectId;//TODO xwwang
        //                    tempNode.Level = NodeLevel.SiteCollection;
        //                    //tempNode.Url = sitecollection.FullPath;//TODO xwwang
        //                    excludeNodeList.Add(tempNode);
        //                }
        //            }
        //        }
        //    }
        //}
        //private Dictionary<string, List<RMEXOTreeNode>> GetTotalRMEXOTreeNode(List<RMEXOTreeNode> rootNodes, ref int nodeCount)
        //{
        //    Dictionary<string, List<RMEXOTreeNode>> returnMap = new Dictionary<string, List<RMEXOTreeNode>>();
        //    foreach (RMEXOTreeNode rootNode in rootNodes)
        //    {
        //        List<RMEXOTreeNode> childNodes = RMSPTreeService.BrowseExchangeTree(rootNode);
        //        if (childNodes != null && childNodes.Count > 0)
        //        {
        //            returnMap.Add(rootNode.Id, childNodes);
        //            nodeCount = nodeCount + childNodes.Count;
        //        }
        //        else
        //        {
        //            returnMap.Add(rootNode.Id, new List<RMEXOTreeNode>());
        //            nodeCount = nodeCount + 0;
        //        }
        //    }
        //    return returnMap;
        //}
        #endregion

        public RAReturnMessage RunSPDataSyncScheduleJob(JobRunBy jobRunBy)
        {
            logger.Debug("start all data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SPDataSynchronisationSchedule,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while SP DataSync,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        public RAReturnMessage RunEXODataSyncScheduleJob(JobRunBy jobRunBy)
        {
            logger.Debug("start all data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.EXODataSynchronisationSchedule,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while EXO DataSync,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunSPDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            JobType jobType = jobRunBy == JobRunBy.Control ? JobType.DataSynchronisation : JobType.SPDataSynchronisationSchedule;
            jobRunByUser = GetJobRunByUser(jobRunBy, jobRunByUser);
            //Skip if a schedule job is running
            List<string> runningJobIds = RMJobMonitorService.GetRunningJobs(JobType.SPDataSynchronisationSchedule);
            if (!runningJobIds.IsNullOrEmpty())
            {
                logger.Info("Current running scheduled data sync job:{0}", string.Join(", ", runningJobIds.ToArray()));

                string jobId = RMJobMonitorService.CreateJob(jobType, jobRunByUser);
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "Skipped this job. A SharePoint Data Synchronization job is already running.");
                return jobId;
            }
            else
            {
                return await RunSPDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
        }

        private async Task<string> RunSPDataSyncJobAllSettingNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            jobId = RMJobMonitorService.CreateJob(jobType, jobRunByUser);
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            var foundSyncSettings = false;

            try
            {
            var syncSettingCount = 0;
            foreach (var setting in SharePointSettingDao.LoadSyncDataSettings(2))
            {
                foundSyncSettings = true;
                var webApp = RMRemoteNodeDao.GetWebApplicationById(setting.SiteGroupId.ToString());
                if (webApp == null)
                {
                    logger.Warn($"Can't find the group: [{setting.SiteGroupId}] in database.");
                    continue;
                }
                else
                {
                    if (RMKeyValueDao.HasUpgradeTeams() && (webApp.NodeType == RemoveNodeType.PrivateChannel || webApp.NodeType == RemoveNodeType.O365GroupSites))
                    {
                        logger.Info($"The account has upgrade teams, Web application is {webApp.NodeType}");
                        continue;
                    }
                    if (webApp.NodeType == RemoveNodeType.SkyDrivePro)
                    {
                        logger.Warn($"Current node is onedrive, will be skipped. Scope id: [{setting.SiteGroupId}]");
                        continue;
                    }
                }
                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                if (selectedNode.Level == (int)NodeLevel.SiteCollection)
                {
                    var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                    if (site == null)
                    {
                        logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                        continue;
                    }

                    if (!site.parentId.Equals(setting.SiteGroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Site collection has been moved to other container, site:{0}", selectedNode.Name);
                        continue;
                    }
                }

                if (selectedNode.Level == (int)NodeLevel.WebApplication || selectedNode.Level == (int)NodeLevel.SiteCollection)
                {
                    var tempNodes = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
                    foreach (var node in tempNodes)
                    {
                        if (!availableNode.Select(n => n.Id).ToList().Contains(node.Id))
                        {
                            availableNode.Add(node);
                        }
                    }
                }

                syncSettingCount++;
            }

            logger.Info($"The available sync settings count: [{syncSettingCount}]");
            //remove sites that not changed since last job
            bool noContentModified = false;
            if (availableNode.Count > 1)
            {
                using (var performance = new PerformanceScope("RMSharePointSettingsService.FilterNoContentModifiedSites"))
                {
                    Dictionary<Guid, List<Guid>> termScopeCache = new Dictionary<Guid, List<Guid>>();
                    var modifiedDateCache = GetSiteModifiedDateCache(availableNode);
                    List<string> notIncludeSiteIds = new List<string>();
                    foreach (var node in availableNode)
                    {
                        if (!NeedCollectSPSite(modifiedDateCache, node, termScopeCache))
                        {
                            notIncludeSiteIds.Add(node.SPObjectId);
                        }
                    }
                    availableNode = availableNode.Where(n => !notIncludeSiteIds.Contains(n.SPObjectId)).ToList();
                    if (availableNode.Count == 0)
                    {
                        noContentModified = true;
                    }
                }
            }
            if (availableNode.IsNullOrEmpty())
            {
                if (noContentModified)
                {
                    logger.Warn("No content modified under sites.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished);
                }
                else
                {
                    logger.Warn("No available sc to run");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroupBySchedule");
                }
                return jobId;
            }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while assembling runnable node. ERROR:{0}", ex.ToString());
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
            
            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            jobType = JobType.DataSynchronisation;
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            foreach (RMSPTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = jobRunBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = jobRunBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }

            if (!foundSyncSettings)
            {
                logger.Warn("There is no site collection setting enable sync data into Explorer.");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoIsSyncSCUnderGroup");
            }

            return jobId;
        }

        private Dictionary<string, DateTime> GetSiteModifiedDateCache(List<RMSPTreeNode> availableNode)
        {
            Dictionary<string, DateTime> siteModifiedDateCache = new Dictionary<string, DateTime>();
            try
            {
                using (var performance = new PerformanceScope("RMSharePointSettingsService.GetSiteModifiedDateCache"))
                {
                    List<string> siteUrls = availableNode.Select(s => s.FullPath).ToList();
                    var remoteSites = RMRemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(siteUrls);
                    var tenantIds = remoteSites.Select(s => s.TenantId).Distinct().ToList();
                    AvePoint.RA.RACommonUtility.CommonClientContext clientContext = new AvePoint.RA.RACommonUtility.CommonClientContext();
                    foreach (var tenantId in tenantIds)
                    {
                        try
                        {
                            var site = remoteSites.Where(s => s.TenantId == tenantId).FirstOrDefault();
                            var remoteSite = RACommonUtility.Browser.RABrowserClient.GetRemoteSiteCollectionById(site?.id);
                            var cache = clientContext.GetSiteModifiedDateCache(remoteSite);
                            if (cache != null && cache.Count > 0)
                            {
                                cache.ToList().ForEach(x => siteModifiedDateCache.Add(x.Key, x.Value));
                            }
                            #region useless
                            //Microsoft.Online.SharePoint.TenantAdministration.Tenant tenant = null;
                            //Microsoft.SharePoint.Client.ClientContext currentContext = null;
                            //using (var performance0 = new PerformanceScope("RMSharePointSettingsService.GetTenant"))
                            //{
                            //    var site = remoteSites.Where(s => s.TenantId == tenantId).FirstOrDefault();
                            //    var remoteSite = RACommonUtility.Browser.RABrowserClient.GetRemoteSiteCollectionById(site.id);
                            //    var bposInfo = RA.SharePoint.Common.PoolUserUtil.GetBPOSInfo(remoteSite);
                            //    //var factory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, Wrapper.Common.AveContextKind.ClientObjectModel);

                            //    AvePoint.RA.RACommonUtility.CommonClientContext clientContext = new AvePoint.RA.RACommonUtility.CommonClientContext();
                            //    currentContext = clientContext.InitClientContext(bposInfo, site.AdminUrl);
                            //    //SPOSitePropertiesEnumerableFilter filter = new SPOSitePropertiesEnumerableFilter();
                            //    tenant = new Microsoft.Online.SharePoint.TenantAdministration.Tenant(currentContext);
                            //    currentContext.Load(tenant);
                            //    currentContext.ExecuteQuery();
                            //}
                            //Microsoft.Online.SharePoint.TenantAdministration.SPOSitePropertiesEnumerable siteProperties = null;
                            //Microsoft.Online.SharePoint.TenantAdministration.SPOSitePropertiesEnumerableFilter sspFilter = new Microsoft.Online.SharePoint.TenantAdministration.SPOSitePropertiesEnumerableFilter()
                            //{
                            //    // get personal sites 
                            //    //IncludePersonalSite = PersonalSiteFilter.Include, // needed to for personal sites 
                            //    //IncludeDetail = true,
                            //    //Template = "SPSPERS"

                            //    // get classic team sites 
                            //    //IncludeDetail = true, 
                            //    //Template = "STS"

                            //    // get modern sites 
                            //    //IncludeDetail = true, 
                            //    //Template = "GROUP" 

                            //    // get communication sites 
                            //    //IncludeDetail = true, 
                            //    //Template = "SITEPAGEPUBLISHING" 
                            //};
                            //if (isOneDrive)
                            //{
                            //    sspFilter.IncludePersonalSite = Microsoft.Online.SharePoint.TenantAdministration.PersonalSiteFilter.Include;
                            //    sspFilter.Template = "SPSPERS";
                            //}
                            ////string filter = isOneDrive ? "Template -eq \"SPSPERS\"" : "Template -ne \"SPSPERS\"";
                            //using (var performance0 = new PerformanceScope("RMSharePointSettingsService.GetSiteProperties"))
                            //{
                            //    //int nextStartIndex = 0;
                            //    //do
                            //    //{
                            //    //    siteProperties = tenant.GetSitePropertiesByFilter(filter, nextStartIndex, false);
                            //    //    currentContext.Load(siteProperties);
                            //    //    currentContext.ExecuteQuery();
                            //    //    nextStartIndex = siteProperties != null ? siteProperties.NextStartIndex : 0;
                            //    //    using (var performance2 = new PerformanceScope("RMSharePointSettingsService.AddSiteProperties"))
                            //    //    {
                            //    //        foreach (var p in siteProperties)
                            //    //        {
                            //    //            siteModifiedDateCache.Add(p.Url.ToLower(), p.LastContentModifiedDate);
                            //    //        }
                            //    //    }
                            //    //}
                            //    //while (siteProperties != null && siteProperties.NextStartIndex > 0);

                            //    string nextIndex = null;
                            //    do
                            //    {
                            //        sspFilter.StartIndex = nextIndex;
                            //        siteProperties = tenant.GetSitePropertiesFromSharePointByFilters(sspFilter);
                            //        currentContext.Load(siteProperties);
                            //        currentContext.ExecuteQuery();
                            //        nextIndex = siteProperties.NextStartIndexFromSharePoint;
                            //        using (var performance2 = new PerformanceScope("RMSharePointSettingsService.AddSiteProperties"))
                            //        {
                            //            foreach (var p in siteProperties)
                            //            {
                            //                siteModifiedDateCache.Add(p.Url.ToLower(), p.LastContentModifiedDate);
                            //            }
                            //        }
                            //    }
                            //    while (nextIndex != null);
                            //}
                            #endregion

                        }
                        catch (Exception e)
                        {
                            logger.Error($"An error occurred while getting site modified date cache,tenant id:{tenantId} error:{e.ToString()}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while getting site modified date cache, error:{e.ToString()}");
            }
            return siteModifiedDateCache;
        }

        private bool NeedCollectSPSite(Dictionary<string, DateTime> modifiedDateCache, RMSPTreeNode site, Dictionary<Guid, List<Guid>> termScopeCache)
        {
            if (modifiedDateCache.ContainsKey(site.FullPath.ToLower()))
            {
                var collectionTime = RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.ExplorerSync, new Guid(site.Parent.SPObjectId), new Guid(site.SPObjectId));
                if (collectionTime != DateTime.MinValue.Ticks
                    && collectionTime >= modifiedDateCache[site.FullPath.ToLower()].Ticks
                    && !HasChangedTermIds(collectionTime, site, termScopeCache))
                {
                    logger.Info($"Site:{site.FullPath} content modified date:{modifiedDateCache[site.FullPath.ToLower()].Ticks} last collection time:{collectionTime}, no need run data sync job.");
                    return false;
                }
            }
            return true;
        }

        private bool HasChangedTermIds(long ticks, RMSPTreeNode site, Dictionary<Guid, List<Guid>> termScopeCache)
        {
            List<Guid> allTerms = new List<Guid>();
            try
            {
                List<Guid> subTerms = new List<Guid>();
                allTerms = RMChangeClassificationDao.GetAllChange(ticks, (int)Contract.Object.TermChangeType.TermRule);
                foreach (var id in allTerms)
                {
                    subTerms.AddRange(TermDao.GetAllSubTermUniqueIds(id));
                }
                allTerms.AddRange(subTerms);

                if (allTerms.Count > 0)
                {
                    var settings = SharePointSettingDao.LoadSPSettingsUnderSite(new Guid(site.SPObjectId));
                    var spSetting = SharePointSettingDao.GetSettingInfoByScope(new Guid(site.Parent.SPObjectId), new Guid(site.SPObjectId), new Guid(site.SPObjectId));
                    if (spSetting == null)
                    {
                        spSetting = SharePointSettingDao.GetSettingInfoByScope(new Guid(site.Parent.SPObjectId), Guid.Empty, new Guid(site.Parent.SPObjectId));
                    }
                    if (spSetting != null)
                    {
                        settings.Add(spSetting);
                    }
                    foreach (var setting in settings)
                    {
                        List<Guid> termIdsUnderScope = new List<Guid>();
                        if (setting.TermId != Guid.Empty)
                        {
                            if (termScopeCache.ContainsKey(setting.TermId))
                            {
                                termIdsUnderScope = termScopeCache[setting.TermId];
                            }
                            else
                            {
                                termIdsUnderScope.Add(setting.TermId);
                                termIdsUnderScope.AddRange(TermDao.GetAllSubTermUniqueIdsByTermId(setting.TermId));
                                termScopeCache.Add(setting.TermId, termIdsUnderScope);
                            }
                        }
                        else if (setting.TermSetId != Guid.Empty)
                        {
                            if (termScopeCache.ContainsKey(setting.TermSetId))
                            {
                                termIdsUnderScope = termScopeCache[setting.TermSetId];
                            }
                            else
                            {
                                var termIds = TermDao.GetAllSubTermUniqueIdsByTermSetId(setting.TermSetId);
                                termIdsUnderScope.AddRange(termIds);
                                termScopeCache.Add(setting.TermSetId, termIdsUnderScope);
                            }
                        }

                        if (termIdsUnderScope.Any(t => allTerms.Contains(t)))
                        {
                            Guid termScopeId = setting.TermId != Guid.Empty ? setting.TermId : setting.TermSetId;
                            logger.Info($"Site: {site.FullPath} has changed term ids. Setting scope id:{setting.ScopeId} Setting group id:{setting.SiteGroupId} Term scope id:{termScopeId}");
                            return true;
                        }
                    }
                    //if (spSetting != null)
                    //{

                    //    if (spSetting.TermId != Guid.Empty)
                    //    {
                    //        if (termScopeCache.ContainsKey(spSetting.TermId))
                    //        {
                    //            termIdsUnderScope = termScopeCache[spSetting.TermId];
                    //        }
                    //        else
                    //        {
                    //            termIdsUnderScope.Add(spSetting.TermId);
                    //            termIdsUnderScope.AddRange(TermDao.GetAllSubTermUniqueIds(spSetting.TermId));
                    //            termScopeCache.Add(spSetting.TermId, termIdsUnderScope);
                    //        }
                    //    }
                    //    else if (spSetting.TermSetId != Guid.Empty)
                    //    {
                    //        if (termScopeCache.ContainsKey(spSetting.TermSetId))
                    //        {
                    //            termIdsUnderScope = termScopeCache[spSetting.TermSetId];
                    //        }
                    //        else
                    //        {
                    //            var termIds = TermDao.GetAllSubTermUniqueIdsByTermSetId(spSetting.TermSetId);
                    //            termIdsUnderScope.AddRange(termIds);
                    //            termScopeCache.Add(spSetting.TermSetId, termIdsUnderScope);
                    //        }
                    //    }

                    //    if (termIdsUnderScope.Any(t => allTerms.Contains(t)))
                    //    {
                    //        logger.Info($"Site: {site.FullPath} has changed term ids.");
                    //        return true;
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                logger.Error("get change terms error {0}", e.ToString());
                return false;
            }
            return false;
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4EXO, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunEXODataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            JobType jobType = jobRunBy == JobRunBy.Control ? JobType.EXODataSynchronisation : JobType.EXODataSynchronisationSchedule;
            jobRunByUser = GetJobRunByUser(jobRunBy, jobRunByUser);
            return await RunEXODataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
        }

        private async Task<string> RunEXODataSyncJobAllSettingNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            jobId = RMJobMonitorService.CreateJob(jobType, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);
            List<RMEXOTreeNode> availableNode = new List<RMEXOTreeNode>();

            try
            {
            var allSetting = EXOSettingDao.LoadAllSettingForDS();

            if (allSetting.IsNullOrEmpty())
            {
                logger.Warn("There is no mailbox setting enable sync data into Explorer.");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoIsSyncMailboxUnderGroup");
                return jobId;
            }

            foreach (var setting in allSetting)
            {
                RMEXOTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(setting.NodeInfo);
                if (setting.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    var testMailbox = MailBoxService.GetMailboxById(selectedNode.Id);
                    if (testMailbox == null)
                    {
                        logger.Info("Mailbox not exist, name:{0}", selectedNode.Name);
                        continue;
                    }

                    if (!testMailbox.ParentId.Equals(setting.GroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Mailbox has been moved to other group, name:{0}", selectedNode.Name);
                        continue;
                    }

                }
                //Group设置Null Classification Setting，不处理当前Group以及Group下的Mailbox，即使Mailbox有打破继承的Term Setting
                var groupSetting = allSetting.Where(x => x.ScopeId == setting.GroupId);
                if (setting.IsNullClassificationSetting || groupSetting != null && groupSetting.Count() > 0 && groupSetting.First().IsNullClassificationSetting)
                {
                    logger.Warn("EXO Data Sync IsNullClassificationSetting or groupSetting IsNullClassificationSetting, name:{0}.IsNullClassificationSetting:{1}.", selectedNode.Name, setting.IsNullClassificationSetting);
                    continue;
                }
                var tempNodes = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
                foreach (var node in tempNodes)
                {
                    if (!availableNode.Select(n => n.Id).ToList().Contains(node.Id))
                    {
                        availableNode.Add(node);
                    }
                }
            }
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                //RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoMailboxUnderGroupBySchedule");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished);//RECO-3309
                return jobId;
            }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while assembling runnable node. ERROR:{0}", ex.ToString());
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
            
            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            jobType = JobType.EXODataSynchronisation;
            int currentSubjobIndex = 0;
            List<RMEXOTreeNode> tempList = new List<RMEXOTreeNode>();
            foreach (RMEXOTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = jobRunBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = jobRunBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }
            return jobId;
        }

        public bool ExistConfiguredSettings(JobType jobType)
        {
            bool exist = false;
            switch (jobType)
            {
                case JobType.SharePointScheduleSetting:
                case JobType.ApplySharePointSettings:
                    exist = SharePointSettingDao.Exist(s => !s.IsRemoved);
                    break;
                case JobType.EXOApplySetting:
                case JobType.EXOApplySettingSchedule:
                    exist = EXOSettingDao.Exist(s => !s.IsRemoved);
                    break;

            }
            return exist;
        }

        public RAReturnMessage RunEXOSettingsScheduleJob(JobRunBy jobRunBy)
        {
            logger.Debug("start all exo apply setting");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.EXOApplySettingSchedule,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while exo apply setting, ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ApplyEXOSetting, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunApplyEXOSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, JobPriority jobPriority = JobPriority.Normal)
        {
            string jobId = string.Empty;
            //if (jobRunBy == JobRunBy.Schedule)
            //{
            //    jobId = RMJobService.CreateJob(JobType.EXOApplySettingSchedule, "RM_TS_RunSchedule");
            //    logger.Info("Begin schedule Apply Job {0}", jobId);
            //}
            //else
            //{
            //    jobId = RMJobService.CreateJob(JobType.EXOApplySettingSchedule, jobRunByUser);
            //    logger.Info("Begin control Apply Job {0}", jobId);
            //}
            List<string> runningJobs = RMJobMonitorService.GetRunningEXOApplySettingJob();

            //bool isSkip = runningJobs.Any(j => j != jobId);

            try
            {
                if (runningJobs.Count == 0)
                {
                    jobId = await StartApplyEXOSettingJobAsync(jobRunBy, jobRunByUser, jobPriority);
                }
                else
                {
                    logger.Info(I18NEntity.GetString("RM_SS_JobSkip"));
                    if (string.IsNullOrWhiteSpace(jobId))
                    {
                        jobId = CreateEXOApplySettingJob(jobRunBy, jobRunByUser, null, jobPriority);
                    }
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                }
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = CreateEXOApplySettingJob(jobRunBy, jobRunByUser, null, jobPriority);
                }
                if (e.Message == I18NEntity.GetString("RM_EXO_NoAvailableSettingError"))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_EXO_NoAvailableSettingError");
                }
                else
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed);
                }
                logger.Error("real run apply exo setting job error: {0}", e.ToString());
            }
            return jobId;
        }

        [Obsolete("Not in use")]
        /*private async System.Threading.Tasks.Task StartApplyEXOSettingScheduleJobAsync(string jobId, JobRunBy runBy)
        {
            List<RMExchangeOnlineSetting> allSettings = null;
            allSettings = EXOSettingDao.LoadAllSettingForAS();
            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No exchange online setting node found.");
                throw new Exception(I18NEntity.GetString("RM_EXO_NoAvailableSettingError"));
            }
            Dictionary<Guid, RMExchangeOnlineSetting> nodeSettingMap = new Dictionary<Guid, RMExchangeOnlineSetting>();
            var excludeSiteNodes = EXOSettingDao.LoadExcludeSiteCollectionSetting();
            List<Guid> excludeMailboxIds = new List<Guid>();
            foreach (var setting in excludeSiteNodes)
            {
                RMEXOTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(setting.NodeInfo);
                if (dbNodeInfo == null)
                {
                    logger.Warn("Node info in {0} is null or empty", setting.Name);
                    continue;
                }
                if (dbNodeInfo.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    var mailBox = MailBoxService.GetMailboxById(dbNodeInfo.Id);
                    if (mailBox == null || !mailBox.ParentId.Equals(setting.GroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Mailbox is null or has been moved to other group, name:{0}. Will not add to exclude list", dbNodeInfo.Name);
                        continue;
                    }
                }

                excludeMailboxIds.Add(setting.ScopeId);
            }
            List<RMEXOTreeNode> availableMailbox = new List<RMEXOTreeNode>();
            foreach (RMExchangeOnlineSetting setting in allSettings)
            {
                try
                {
                    RMEXOTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(setting.NodeInfo);
                    if (dbNodeInfo == null)
                    {
                        logger.Warn("Node info in {0} is null or empty", setting.Name);
                        continue;
                    }
                    if (dbNodeInfo.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        var group = MailBoxDao.GetO365GroupById(setting.GroupId.ToString());
                        if (group == null)
                        {
                            await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                            logger.Warn("Mailbox group is null, name:{0}", dbNodeInfo.Name);
                            continue;
                        }
                        List<RMEXOTreeNode> mailboxs = RMSPTreeService.BrowseExchangeTree(dbNodeInfo);
                        var totalMailBoxCount = mailboxs.Count;
                        var hasCustomMailboxCount = 0;
                        logger.Info("Group:{0} mailbox count is {1}", dbNodeInfo.Name, mailboxs.Count);
                        foreach (RMEXOTreeNode mailbox in mailboxs)
                        {
                            if (excludeMailboxIds.Contains(new Guid(mailbox.Id)))
                            {
                                logger.Info("Exclude mailbox Id{0}", mailbox.Id);
                                hasCustomMailboxCount++;
                            }
                            else
                            {
                                availableMailbox.Add(mailbox);
                            }

                            if (!nodeSettingMap.ContainsKey(new Guid(dbNodeInfo.Id)))
                            {
                                nodeSettingMap.Add(new Guid(dbNodeInfo.Id), setting);
                            }
                        }
                        if (totalMailBoxCount == hasCustomMailboxCount)
                        {
                            await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                            //EXOSettingDao.SetSettingInfo(new Guid(dbNodeInfo.Id), DateTime.UtcNow.Ticks, false);
                        }
                    }
                    else
                    {
                        GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object.EmailAccountDto testMailbox = null;
                        try
                        {
                            //DAOAPIClientV1 client = new DAOAPIClientV1();
                            //testMailbox = client.GetExchangeNodeById(dbNodeInfo.Id);
                            testMailbox = MailBoxService.GetMailboxById(dbNodeInfo.Id);
                        }
                        catch (Exception e)
                        {
                            logger.Error("get exo node error:{0}", e.ToString());
                        }
                        if (testMailbox != null)
                        {
                            if (!testMailbox.ParentId.Equals(setting.GroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                logger.Warn("Mailbox has been moved to other group:{0}", testMailbox.FullPath);
                                await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                                continue;
                            }
                            availableMailbox.Add(dbNodeInfo);
                            if (!nodeSettingMap.ContainsKey(new Guid(dbNodeInfo.Id)))
                            {
                                nodeSettingMap.Add(new Guid(dbNodeInfo.Id), setting);
                            }
                        }
                        else
                        {
                            logger.Warn("Mailbox is null, name:{0}", dbNodeInfo.Name);
                            await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }
            int subJobCount = availableMailbox.Count;
            //run top level site first
            //availableSites = availableSites.OrderBy(a => a.NodeType).ToList();
            if (subJobCount == 0)
            {
                logger.Warn("No available sc to run");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoMailboxUnderGroup");
                foreach (RMExchangeOnlineSetting setting in allSettings)
                {
                    await EXOSettingDao.SetSettingInfoAsync(setting.GroupId, setting.ScopeId, DateTime.UtcNow.Ticks, false);
                }
                return;
            }
            SeperateSubJobForApplyEXOSetting(availableMailbox, nodeSettingMap, jobId, runBy, JobType.EXOApplySetting);
        }*/
        #endregion
        #region physical disposal 
        //[Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunDisposalJob,
        //    AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public RAReturnMessage RunPhysicalDisposalJob(int locationId, JobRunBy jobRunBy)
        {
            logger.Debug("start physical disposal");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                if (TermRuleAssociationDao.GetTermWithRule().Count == 0)
                {
                    logger.Error(I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules"));
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules");
                    return msg;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalDisposal,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = GetJobRunByUser(jobRunBy, null),
                    Parameters = locationId.ToString(),
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while start physical disposal,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        #endregion
        private static string GetJobRunByUser(JobRunBy jobRunBy, string jobRunByUser)
        {
            if (jobRunBy == JobRunBy.Control)
            {
                jobRunByUser = string.IsNullOrEmpty(jobRunByUser) ? TenantLocalValue.LogonUserEmail : jobRunByUser;
            }
            else
            {
                jobRunByUser = "RM_TS_RunSchedule";
            }

            return jobRunByUser;
        }

        public string RunImportSPSetting(JobRunBy jobRunBy, string extension, string strBytes)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportSPSetting,
                    Parameters = string.Format("{0} {1}", extension, strBytes),
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunSyncLocationTreeToSharePoint,ERROR:{0}", ex.ToString());
            }

            return id;
        }


        public RAReturnMessage RunExportSPSOSetting(ExportSettingType type, JobRunBy jobRunBy)
        {
            RAReturnMessage message = new();
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var currentSPSettings = ArchiverSettingDao.LoadAllArchiverSettingWithType(ContentSourceType.SharePoint);
                if (type == ExportSettingType.OnlyExportCustomSettingNodes && (currentSPSettings == null || currentSPSettings.Count == 0))
                {
                    message.MessageType = RAMessageType.Failed;
                    message.ErrorMessage = I18NEntity.GetString("RM_JS_BCM_ExportSPSetting_NotSetting");
                    return message;
                }
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportSPSOSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = type.ToString()
                };
                message.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunExportSPSOSetting,ERROR:{0}", ex.ToString());
            }

            return message;
        }

        public RAReturnMessage RunExportSPSetting(ExportSettingType type ,JobRunBy jobRunBy)
        {
            RAReturnMessage message = new();
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var currentSPSettings = SharePointSettingDao.GetAllGroupSettings();
                if (type == ExportSettingType.OnlyExportCustomSettingNodes && (currentSPSettings == null || currentSPSettings.Count == 0))
                {
                    message.MessageType = RAMessageType.Failed;
                    message.ErrorMessage = I18NEntity.GetString("RM_JS_BCM_ExportSPSetting_NotSetting");
                    return message;
                }
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportSPSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = type.ToString()
                };
                message.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunExportSPSetting,ERROR:{0}", ex.ToString());
            }

            return message;
        }

        public async Task<RAReturnMessage> SyncADUsersAsync(List<ToUserInfo> users)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users);
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        #region Custom index metadata

        public async Task<List<CustomMetadataColumnInfo>> GetAllCustomMetadataColumnInfoAsync()
        {
            var dbItems = await RMCustomMetadataColumnDao.GetAllCustomMetadataColumnsAsync();
            return dbItems.ToList().ConvertAll(ConvertDBItemToDto);
        }        
        
        public async Task<List<CustomMetadataColumnInfo>> GetInUsedCustomMetadataColumnInfoAsync()
        {
            var dbItems = await RMCustomMetadataColumnDao.GetInUsedCustomMetadataColumnsAsync();
            return dbItems.ToList().ConvertAll(ConvertDBItemToDto);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.SaveCustomMetadataColumn, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateCustomMetadataColumnAsync(List<CustomMetadataColumnInfo> customMetadataColumnInfo)
        {
            var returnMessage = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful,
            };
            try
            {
                var dbItems = customMetadataColumnInfo.ConvertAll(ConvertDtoToDBItem);
                await RMCustomMetadataColumnDao.DeleteAllCustomMetadataColumnsAsync();
                await RMCustomMetadataColumnDao.AddOrUpdateCustomMetadataColumnsAsync(dbItems.ToArray());
            }
            catch(Exception e)
            {
                returnMessage.MessageType = RAMessageType.Failed;
            }

            return returnMessage;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.SaveCustomIndexMetadata, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateCustomIndexMetadataAsync(CustomIndexMetadataInfo customIndexMetadataInfo, SourceFlag sourceFlag)
        {
            var returnMessage = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful,
            };
            try
            {
                await RMCustomIndexMetadataDao.DeleteCustomIndexMetadataAsync(sourceFlag);
                await RMKeyValueDao.SaveOrUpdateAsync(new() { Key = KeyNameCollection.IsEnableCustomIndexMetadata, Value = customIndexMetadataInfo.IsEnableCustomIndexMetadata.ToString() });
                if (customIndexMetadataInfo.IsEnableCustomIndexMetadata)
                {
                    var dbItems = customIndexMetadataInfo.CustomIndexMetadataDtos.ConvertAll(ConvertDtoToDBItem);
                    await RMCustomIndexMetadataDao.AddOrUpdateCustomIndexMetadatasAsync(dbItems.ToArray());
                    return returnMessage;
                }
            }
            catch (Exception e)
            {
                returnMessage.MessageType = RAMessageType.Failed;
            }

            return returnMessage;
        }

        public async Task<CustomIndexMetadataInfo> GetAllCustomIndexMetadataAsync()
        {
            var result = new CustomIndexMetadataInfo();
            _ = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnable);
            if (isEnable)
            {
                try
                {
                    result.IsEnableCustomIndexMetadata = true;
                    var dbItems = await RMCustomIndexMetadataDao.GetAllCustomIndexMetadatasAsync();
                    var dbColumns = await RMCustomMetadataColumnDao.GetCustomMetadataColumnsAsync(dbItems.Select(item => item.TargetColumnId).ToArray());
                    result.CustomIndexMetadataDtos = dbItems.ToList().ConvertAll(item => ConvertDBItemToDto(item, dbColumns.ToList()));
                }
                catch(Exception e)
                {

                }
            }
            return result;
        }

        public async Task<CustomIndexMetadataInfo> GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag sourceFlag)
        {
            var result = new CustomIndexMetadataInfo();
            _ = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnable);
            if (isEnable)
            {
                try
                {
                    result.IsEnableCustomIndexMetadata = true;
                    var dbItems = await RMCustomIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(sourceFlag);
                    var dbColumns = await RMCustomMetadataColumnDao.GetCustomMetadataColumnsAsync(dbItems.Select(item => item.TargetColumnId).ToArray());
                    result.CustomIndexMetadataDtos = dbItems.ToList().ConvertAll(item => ConvertDBItemToDto(item, dbColumns.ToList()));
                }
                catch (Exception e)
                {
                }
            }
            return result;
        }

        private RMCustomIndexMetadata ConvertDtoToDBItem(CustomIndexMetadataDto metadataDto)
        {
            return new RMCustomIndexMetadata()
            {
                Id = metadataDto.Id,
                UniqueId = metadataDto.UniqueId == Guid.Empty ? Guid.NewGuid() : metadataDto.UniqueId,
                ContentSource = metadataDto.ContentSource,
                ModifiedTime = DateTime.UtcNow.Ticks,
                SourceColumnName = metadataDto.SourceColumnName,
                TargetColumnId = metadataDto.TargetColumnId,
            };
        }

        private RMCustomMetadataColumn ConvertDtoToDBItem(CustomMetadataColumnInfo metadataDto)
        {
            return new()
            {
                UniqueId = metadataDto.UniqueId == Guid.Empty ? Guid.NewGuid() : metadataDto.UniqueId,
                ColumnName = metadataDto.ColumnName,
                ColumnType = metadataDto.ColumnType,
                EnableSort = metadataDto.EnableSort,
            };
        }

        private CustomIndexMetadataDto ConvertDBItemToDto(RMCustomIndexMetadata metadata, List<RMCustomMetadataColumn> columns)
        {
            var column = columns.FirstOrDefault(c => metadata.TargetColumnId == c.UniqueId);
            return new CustomIndexMetadataDto()
            {
                Id = metadata.Id,
                UniqueId = metadata.UniqueId,
                ContentSource = metadata.ContentSource,
                ModifiedTime = metadata.ModifiedTime,
                SourceColumnName = metadata.SourceColumnName,
                TargetColumnName = column.ColumnName,
                ColumnType = column.ColumnType,
                TargetColumnId = column.UniqueId,
            };
        }

        private CustomMetadataColumnInfo ConvertDBItemToDto(RMCustomMetadataColumn metadata)
        {
            return new()
            {
                UniqueId = metadata.UniqueId,
                ColumnName = metadata.ColumnName,
                ColumnType = metadata.ColumnType,
                EnableSort = metadata.EnableSort,
            };
        }

        #endregion
    }
}