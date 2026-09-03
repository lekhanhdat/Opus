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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Services.DeclaredRecordsMigration
{
    [Audit]
    internal class DeclaredRecordsMigrationService : RMServiceBase, IDeclaredRecordsMigrationService
    {
        private RALogger logger = RALogger.GetInstance(typeof(DeclaredRecordsMigrationService));

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ITeamsSettingTreeService TeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public async Task<RAReturnMessage> RunDeclaredRecordsMigrationJob(DeclaredRecordsMigrationDto dto)
        {
            string id = string.Empty;
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                if (dto.NodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    logger.Error($"Only web application level node is supported for Declared Records Migration job. Current level: {dto.NodeSetting.Level}");
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = "Invalid Scope";
                    return result;
                }

                if (!await GeneralSettingService.SaveOrUpdateRecordLabelAsync(dto.RecordsLabel))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = "Invalid Records Label.";
                    return result;
                }

                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.DeclaredRecordsMigration,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(dto)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    result = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run DeclaredRecordsMigration job,ERROR:{0}", ex);
                result = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return result;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunDeclaredRecordsMigrationJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunDeclaredRecordsMigrationJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.DeclaredRecordsMigration;
            var jobInfo = SerializerHelper.DeserializeByDataContractSerializer<DeclaredRecordsMigrationDto>(param);
            var loginName = TenantLocalValue.LogonUserEmail;
            if (jobInfo.NodeSetting.Type == ContentSourceType.Teams)
            {
                return await RealRunTeamsDeclaredRecordsMigrationJobOnSelectedNode(jobRunBy, loginName, jobType, jobInfo.NodeSetting, jobInfo.RecordsLabel);
            }
            return await RealRunDeclaredRecordsMigrationJobOnSelectedNode(jobRunBy, loginName, jobType, jobInfo.NodeSetting, jobInfo.RecordsLabel);
        }

        private async Task<string> RealRunDeclaredRecordsMigrationJobOnSelectedNode(JobRunBy jobRunBy, string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode, string recordsLabel)
        {
            string jobId = string.Empty;
            string nodeUrl = selectedNode.FullPath;

            logger.Info("Start RealRunDeclaredRecordsMigrationJobOnSelectedNode");

            List<RMSPTreeNode> availableNode = await AssembleDeclaredRecordsMigrationRunnableNode(selectedNode);
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                var message = selectedNode.Level == (int)NodeLevel.WebApplication
                    ? $"RM_SP_NoSiteCollectionUnderGroup{I18NEntity.Separator}{selectedNode.Name}"
                    : $"RM_JM_Report_Skip_NoAvailableSites";
                jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, nodeUrl);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, message);
                return jobId;
            }
            //var runningSiteUrls = RMJobService.GetRunningArchiverJobsScopes([JobType.DeclaredRecordsMigration]);
            //availableNode = FilterAvailableNodeByRunningUrl(availableNode, nodeUrl, runningSiteUrls);
            var hasRunningjob = RMJobService.HasRunningArchiverJobOnScope([jobType], nodeUrl);
            if (hasRunningjob)
            {
                logger.Warn($"Has running job with same scope");
                jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, nodeUrl);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, nodeUrl);
            logger.Info($"real run job node count after filter is {availableNode.Count}");
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            foreach (var node in availableNode)
            {
                var dto = new DeclaredRecordsMigrationDto()
                {
                    NodeSetting = node,
                    RecordsLabel = recordsLabel
                };

                string subJobId = CreateSubJobForDeclaredRecordsMigration(jobId, currentSubjobIndex, jobType, subJobCount, dto, false, node.FullPath, node.O365TenantId);
                currentSubjobIndex++;
            }
            return jobId;
        }

        private async Task<List<RMSPTreeNode>> AssembleDeclaredRecordsMigrationRunnableNode(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(selectedNode);
                if (sites.IsNullOrEmpty())
                {
                    return availableNode;
                }

                foreach (RMSPTreeNode site in sites)
                {
                    availableNode.Add(site);
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

        private string CreateSubJobForDeclaredRecordsMigration(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, DeclaredRecordsMigrationDto tempDto, bool sendNow, string scope, string o365TenantId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext()
            {
                JobId = subJobId,
                Settings = SerializerHelper.SerializeByDataContractSerializer(tempDto)
            };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} , Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            return subJobId;
        }

        private async Task<string> RealRunTeamsDeclaredRecordsMigrationJobOnSelectedNode(JobRunBy jobRunBy, string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode, string recordsLabel)
        {
            string jobId = string.Empty;
            string teamsUrl = selectedNode.GetTeamsNode()?.DisplayName ?? (RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.GetTeamsNode()?.SPObjectId).Item1?.url ?? string.Empty);
            string nodeFullPath = selectedNode.FullPath;
            logger.Info("Start RealRunTeamsConvertStubJobOnSelectedNode");

            List<RMSPTreeNode> availableNode = await AssembleTeamsDeclaredRecordsMigrationJobRunnableNode(selectedNode);
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                var message = selectedNode.Level == (int)NodeLevel.WebApplication
                    ? $"RM_Teams_NoTeamsGroupUnderGroup{I18NEntity.Separator}{selectedNode.Name}"
                    : $"RM_JM_Report_Skip_NoAvailableSites";
                jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, nodeFullPath);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, message);
                return jobId;
            }

            var hasRunningjob = RMJobService.HasRunningArchiverJobOnScope([jobType], nodeFullPath);
            if (hasRunningjob)
            {
                logger.Warn($"Has running job with same scope");
                jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, nodeFullPath);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            jobId = RMJobService.CreateJobWithScopeIdForTeams(jobType, jobRunByUser, nodeFullPath, nodeFullPath);
            logger.Info($"real run job node count after filter is {availableNode.Count}");
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            foreach (var node in availableNode)
            {
                var dto = new DeclaredRecordsMigrationDto()
                {
                    NodeSetting = node,
                    RecordsLabel = recordsLabel
                };

                string subJobId = CreateSubJobForDeclaredRecordsMigration(jobId, currentSubjobIndex, jobType, subJobCount, dto, false, node.FullPath, node.O365TenantId);
                logger.Debug("Start sub job {0}", subJobId);
                currentSubjobIndex++;
            }
            return jobId;
        }

        public async Task<List<RMSPTreeNode>> AssembleTeamsDeclaredRecordsMigrationJobRunnableNode(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> teamsNodes = await TeamsTreeService.BrowseAsync(selectedNode, false);
                if (teamsNodes.IsNullOrEmpty())
                {
                    return availableNode;
                }
                foreach (RMSPTreeNode teams in teamsNodes)
                {
                    var sites = await TeamsTreeService.BrowseDirectSitesByTeamNode(RMDtoConverter.ConvertRMTree2SPTree(teams));
                    availableNode.AddRange(sites);
                }
            }
            else if (selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
            {
                if (ValidateTeamsExist(selectedNode))
                {
                    var sites = await TeamsTreeService.BrowseDirectSitesByTeamNode(RMDtoConverter.ConvertRMTree2SPTree(selectedNode));
                    availableNode.AddRange(sites);
                }
                else
                {
                    logger.Info("Teams not exist, teams:{0}", selectedNode.Name);
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
                    logger.Info("Site not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
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
                site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }
    }
}
