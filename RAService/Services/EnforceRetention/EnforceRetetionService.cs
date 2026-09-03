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
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Service.Services.EnforceRetention.AuditHandler;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Common.Cache;
using AvePoint.GCommon.Utility;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.DB.Dao.Extension;

namespace AvePoint.RA.Service.Services.EnforceRetention
{
    [Audit]
    public class EnforceRetentionService: RMServiceBase, IEnforceRetentionService
    {
        protected RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region public properties
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        public ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        public IRMChangeClassificationDao TermChangeDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();
        public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        public ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        public ITeamsSettingTreeService RMTeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        public IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        public ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        #endregion

        public string RunScheduleJob(JobRunBy jobRunBy, JobType jobType)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
            };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run enforce schedule,ERROR:{0}", ex.ToString());
            }
            return id;
        }

        public string RunEXOScheduleJob(JobRunBy jobRunBy, JobType jobType)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run enforce schedule,ERROR:{0}", ex.ToString());
            }
            return id;
        }

        public string RunOneDriveScheduleJob(JobRunBy jobRunBy, JobType jobType)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run enforce schedule,ERROR:{0}", ex.ToString());
            }
            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.RunEnforceRetentionJob, BeforeHandler = typeof(EnforceRetetionBeforeAuditHandler), AfterHandler = typeof(EnforceRetetionAfterAuditHandler))]
        public async Task<string> RealRunJobAsync(JobRunBy jobRunBy, JobType jobType)
        {
            string jobId = string.Empty;
            var jobRunByUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);

            List<string> runningJobs = RMJobService.GetRunningJobs(jobType);

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                await StartJobAsync(jobId, jobRunBy, jobType);
                logger.Info("run enforce retention job success, JobId:{0}", jobId);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                logger.Info("enforce retention job has job running,so shedule job is skip");
            }
            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.RunEnforceRetentionJob, BeforeHandler = typeof(EnforceRetetionBeforeAuditHandler), AfterHandler = typeof(EnforceRetetionAfterAuditHandler))]
        public string RealRunEXOJob(JobRunBy jobRunBy, JobType jobType)
        {
            string jobId = string.Empty;
            var jobRunByUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);

            List<string> runningJobs = RMJobService.GetRunningJobs(JobType.EXOEnforceRetention);

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                StartEXOJob(jobId, jobRunBy);
            }
            else
            {
                logger.Info(I18NEntity.GetString("RM_EF_JobSkip"));
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
            }
            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.RunEnforceRetentionJob, BeforeHandler = typeof(EnforceRetetionBeforeAuditHandler), AfterHandler = typeof(EnforceRetetionAfterAuditHandler))]
        public async Task<string> RealRunOneDriveJobAsync(JobRunBy jobRunBy, JobType jobType)
        {
            string jobId = string.Empty;
            var jobRunByUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);

            List<string> runningJobs = RMJobService.GetRunningJobs(jobType);

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                await StartOneDriveJobAsync(jobId, jobRunBy, jobType);
                logger.Info("run enforce retention job success, JobId:{0}", jobId);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                logger.Info("enforce retention job has job running,so shedule job is skip");
            }
            return jobId;
        }

        public async System.Threading.Tasks.Task StartJobAsync(string jobId, JobRunBy runBy, JobType jobType)
        {
            List<RMSharePointSetting> allSettings = null;
            allSettings = SharePointSettingDao.GetAllGroupSettings();
            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No SharePoint online group setting node found.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_TM_EnforceRetention_NoAvailableSPSetting");
                return;
            }
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            foreach (RMSharePointSetting setting in allSettings)
            {
                try
                {
                    RMSPTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                    if (dbNodeInfo == null)
                    {
                        logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                        continue;
                    }

                    if (dbNodeInfo.Level == (int)NodeLevel.WebApplication)
                    {
                        var webApp = RABrowserClient.GetWebApplicationById(setting.SiteGroupId.ToString());
                        if (webApp == null)
                        {
                            logger.Warn($"Can't find the group: [{setting.SiteGroupId}] in database.");
                            continue;
                        }
                        else
                        {
                            if (RMKeyValueDao.HasUpgradeTeams() && (webApp.NodeType == RemoveNodeType.O365GroupSites || webApp.NodeType == RemoveNodeType.PrivateChannel))
                            {
                                logger.Warn($"Current node is teams, will be skipped. Scope id: [{setting.SiteGroupId}]");
                                continue;
                            }
                            if (webApp.NodeType == RemoveNodeType.SkyDrivePro)
                            {
                                logger.Warn($"Current node is onedrive, will be skipped. Scope id: [{setting.SiteGroupId}]");
                                continue;
                            }
                        }
                        List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(dbNodeInfo);
                        var totalSiteCount = sites.Count;
                        logger.Info("Group:{0} site collection count is {1}", dbNodeInfo.Name, sites.Count);
                        foreach (RMSPTreeNode siteNode in sites)
                        {
                            if (!availableSites.Any(site => site.Id.Equals(siteNode.Id)))
                            {
                                availableSites.Add(siteNode);
                            }
                        }
                    }
                    else
                    {
                        logger.Warn($"not support node found:{ dbNodeInfo.Level }");
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }
            //run top level site first
            //availableSites = availableSites.OrderBy(a => a.NodeType).ToList();
            if (availableSites.Count == 0)
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
                return;
            }
            SeperateSubJobForEnforceRetention(availableSites, jobId, runBy, jobType);
        }

        private void SeperateSubJobForEnforceRetention(List<RMSPTreeNode> availableSites, string jobId, JobRunBy runBy, JobType jobType)
        {
            //int subJobCountInConfig = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfig = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List <RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (RMSPTreeNode site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfig);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfig)
                    {
                        mJobQueueService.HandleMessage(new JobQueueMessage()
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
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfig);
                logger.Debug("Create and queue sub job {0}", subJobId);
                if (currentSubjobIndex < subJobCountInConfig)
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
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

        public async System.Threading.Tasks.Task StartOneDriveJobAsync(string jobId, JobRunBy runBy, JobType jobType)
        {
            List<RMOneDriveSetting> allSettings = null;
            allSettings = OneDriveSettingDao.GetAllGroupSettings();
            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No SharePoint online group setting node found.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_TM_EnforceRetention_NoAvailableSPSetting");
                return;
            }
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            var enableNullClassificationGroupIds = OneDriveSettingDao.LoadAllSetting().Where(s => s.SiteGroupId == s.ScopeId && s.IsNullClassificationSetting).Select(s => s.SiteGroupId.ToString()).ToList();
            Dictionary<string, string> OneDriveRuleSettingContainers = new Dictionary<string, string>();
            foreach (RMOneDriveSetting setting in allSettings)
            {
                try
                {
                    RMSPTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                    if (dbNodeInfo == null)
                    {
                        logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                        continue;
                    }
                    if (enableNullClassificationGroupIds != null && enableNullClassificationGroupIds.Count > 0)
                    {
                        var groupNode = dbNodeInfo.GetGroupNode();
                        if (enableNullClassificationGroupIds.Contains(groupNode.SPObjectId))
                        {
                            logger.Info("Onedrive group enable null classification, site:{0}", dbNodeInfo.Name);
                            if (!OneDriveRuleSettingContainers.ContainsKey(groupNode.SPObjectId))
                            {
                                OneDriveRuleSettingContainers.Add(groupNode.SPObjectId, GetSPContainerName(groupNode));
                            }
                            continue;
                        }
                    }
                    if (dbNodeInfo.Level == (int)NodeLevel.WebApplication)
                    {
                        List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(dbNodeInfo);
                        var totalSiteCount = sites.Count;
                        logger.Info("Group:{0} site collection count is {1}", dbNodeInfo.Name, sites.Count);
                        foreach (RMSPTreeNode siteNode in sites)
                        {
                            if (!availableSites.Any(site => site.Id.Equals(siteNode.Id)))
                            {
                                availableSites.Add(siteNode);
                            }
                        }
                    }
                    else
                    {
                        logger.Warn($"not support node found:{ dbNodeInfo.Level }");
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }
           
            //run top level site first
            //availableSites = availableSites.OrderBy(a => a.NodeType).ToList();
            if (availableSites.Count == 0)
            {
                if (OneDriveRuleSettingContainers != null && OneDriveRuleSettingContainers.Count > 0)
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{string.Join(',', OneDriveRuleSettingContainers.Values)}");
                    logger.Warn($"Onedrive group enable null classification. Skip run job. Group name:{string.Join(',', OneDriveRuleSettingContainers.Values)}");
                }
                else
                {
                    logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
                }
                return;
            }
            SeperateSubJobForEnforceRetention(availableSites, jobId, runBy, jobType);
        }

        private string GetSPContainerName(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Name;
            }
            else
            {
                return GetSPContainerName(selectedNode.Parent);
            }
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, Dictionary<Guid, RMSharePointSetting> gruopSetingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
           
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        private void StartEXOJob(string jobId, JobRunBy runBy)
        {
            List<RMExchangeOnlineSetting> allSettings = null;
            allSettings = EXOSettingDao.LoadAllGroupSettings();
            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No exchange online setting node found.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_TM_EnforceRetention_NoAvailableEXOSetting");
                return;
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
                        List<RMEXOTreeNode> mailboxs = RMSPTreeService.BrowseExchangeTree(dbNodeInfo);
                        var totalMailBoxCount = mailboxs.Count;
                        logger.Info("Group:{0} mailbox count is {1}", dbNodeInfo.Name, mailboxs.Count);
                        foreach (RMEXOTreeNode mailbox in mailboxs)
                        {
                           availableMailbox.Add(mailbox);
                        }
                    }
                    else
                    {
                        logger.Warn("exo enforce retention job not supported type");
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }
            //run top level site first
            //availableSites = availableSites.OrderBy(a => a.NodeType).ToList();
            if (availableMailbox.Count == 0)
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoMailboxUnderGroup");
                return;
            }
            SeperateSubJobForEXOEnforceRetention(availableMailbox, jobId, runBy, JobType.EXOEnforceRetention);
        }

        private void SeperateSubJobForEXOEnforceRetention(List<RMEXOTreeNode> availableSites, string jobId, JobRunBy runBy, JobType jobType)
        {
            List<RMEXOTreeNode> tempList = new List<RMEXOTreeNode>();
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (RMEXOTreeNode site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJobForEXO(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        mJobQueueService.HandleMessage(new JobQueueMessage()
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
                string subJobId = CreateSubJobForEXO(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                logger.Debug("Create and queue sub job {0}", subJobId);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
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

        private string CreateSubJobForEXO(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMEXOTreeNode> tempList, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        #region Teams enforce retention job

        public string RunTeamsScheduleJob(JobRunBy jobRunBy, JobType jobType)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run teams enforce schedule,ERROR:{0}", ex.ToString());
            }
            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.RunEnforceRetentionJob, BeforeHandler = typeof(EnforceRetetionBeforeAuditHandler), AfterHandler = typeof(EnforceRetetionAfterAuditHandler))]
        public async Task<string> RealTeamsRunJobAsync(JobRunBy jobRunBy, JobType jobType)
        {
            string jobId = string.Empty;
            var jobRunByUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);

            List<string> runningJobs = RMJobService.GetRunningJobs(jobType);

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                await StartTeamsJobAsync(jobId, jobRunBy, jobType);
                logger.Info("run teams enforce retention job success, JobId:{0}", jobId);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                logger.Info("teams enforce retention job has job running,so shedule job is skip");
            }
            return jobId;
        }

        public async System.Threading.Tasks.Task StartTeamsJobAsync(string jobId, JobRunBy runBy, JobType jobType)
        {
            List<RMTeamsSetting> allSettings = null;
            allSettings = TeamsSettingDao.GetAllGroupSettings();
            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No Teams group setting node found.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_TM_EnforceRetention_NoAvailableSPSetting");
                return;
            }
            List<RMSPTreeNode> availableTeams = new List<RMSPTreeNode>();
            foreach (RMTeamsSetting setting in allSettings)
            {
                try
                {
                    RMSPTreeNode dbNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                    if (dbNodeInfo == null)
                    {
                        logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                        continue;
                    }

                    if (dbNodeInfo.Level == (int)NodeLevel.WebApplication)
                    {
                        var webApp = RABrowserClient.GetWebApplicationById(setting.TeamsGroupId.ToString());
                        if (webApp == null)
                        {
                            logger.Warn($"Can't find the group: [{setting.TeamsGroupId}] in database.");
                            continue;
                        }
                        List<RMSPTreeNode> teams = await RMTeamsTreeService.BrowseAsync(dbNodeInfo);
                        var totalSiteCount = teams.Count;
                        logger.Info("Group:{0} teams count is {1}", dbNodeInfo.Name, teams.Count);
                        foreach (RMSPTreeNode teamsNode in teams)
                        {
                            if (!availableTeams.Any(site => site.Id.Equals(teamsNode.Id)))
                            {
                                availableTeams.Add(teamsNode);
                            }
                        }
                    }
                    else
                    {
                        logger.Warn($"not support node found:{dbNodeInfo.Level}");
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }
            if (availableTeams.Count == 0)
            {
                logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoTeamsUnderGroup");
                return;
            }
            SeperateSubJobForEnforceRetention(availableTeams, jobId, runBy, jobType);
        }
        #endregion
        /*private async Task<List<SPTreeNodeDto>> GetSPTreeNodeAsync()
        {
            List<SPTreeNodeDto> returnList = new List<SPTreeNodeDto>();
            List<RMSPTreeNode> registeredFarms = SPSettingTreeService.LoadFarm();
            var groups = await SPSettingTreeService.BrowseAsync(registeredFarms[0]);
            foreach (var gp in groups)
            {
                var spSeting = SharePointSettingDao.LoadSharePointSetting(new Guid(gp.SPObjectId), Guid.Empty);
                if (spSeting != null)
                {
                    SPTreeNodeDto sptree = RMDtoConverter.ConvertRMTree2SPTree(gp);
                    returnList.Add(sptree);
                }
            }
            return returnList;
        }*/

        /*private bool CheckTermRetentionStatus()
        {
            bool result = false;
            try
            {
                var changeList = TermChangeDao.GetAllChangeByType((int)TermChangeType.Retention);
                result = changeList.Count > 0;
                //TermDao.HasRetentionSetting(termId);
            }
            catch (Exception ex)
            {
                logger.Error("error while change term retention status,ERROR:{0}", ex.ToString());
            }
            return result;
        }*/

    }
}
