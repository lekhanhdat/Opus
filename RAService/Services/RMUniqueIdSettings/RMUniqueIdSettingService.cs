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
using AvePoint.Common.RemoteNode.Impl;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler;
using AvePoint.RA.Service.Services.RMUniqueIdSettings.AuditHandler;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Restore;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMUniqueIdSettings
{
    [Audit]
    public class RMUniqueIdSettingService : RMServiceBase, IUniqueIdSettingService
    {
        private int digit = 10;
        private string currentFormat = "{0}-{1}";
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IUniqueIdSettingDao UniqueIdSettingDao => PlatformWindsorManager.GetService<IUniqueIdSettingDao>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ITeamsSettingTreeService RMTeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao => PlatformWindsorManager.GetService<ISharePointOnPremiseSettingDao>();
        private IRMSharePointOnPremBrowseService RMSharePointOnPremBrowseService => PlatformWindsorManager.GetService<IRMSharePointOnPremBrowseService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        protected IRMNodeFlagDao RMNodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();
        public IHybridSharePointOnPremWorkerService HybridSharePointWorkerService => PlatformWindsorManager.GetService<IHybridSharePointOnPremWorkerService>();
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.UniqueIDSetting, BeforeHandler = typeof(RMUniqueIdSettingBeforeAuditHandler), AfterHandler = typeof(RMUniqueIdSettingAfterAuditHandler))]
        public async System.Threading.Tasks.Task UpdateUniqueIdSettingAsync(UniqueIdSetting setting)
        {
            try
            {
                if (setting.SourceFlag == SourceFlag.FileSystem)
                {
                    await UpdateFileSystemUniqueIdSettingAsync(setting);
                    return;
                }

                var uniqueIdSettting = new RMUniqueIdSetting();

                Logger.Info(string.Format("Update UniqueIdSettings, Name : {0} , Prefix{1} , IsActived{2}, overridSPPrefix:{3}", setting.Name, setting.Prefix, setting.IsActived, setting.OverrrideSPPrefix));
                RMUniqueIdSetting oldSetting = null;
                if (setting.SourceFlag == Contract.Explorer.SourceFlag.Teams)
                {
                    oldSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.Teams);
                }
                else if(setting.SourceFlag == Contract.Explorer.SourceFlag.FileSystem)
                {
                    oldSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.FileSystem);
                    var enableUniqueIdsetting = await AgentMgmtService.CheckIfEnableFSUniqueIdSetting();
                    //if (!enableUniqueIdsetting)
                    //{
                    //    throw new Exception("RM_JS_FS_UniqueSetting_SaveFailed");
                    //}
                }
                else
                {
                    oldSetting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                }
                if (setting.IsActived)
                { 
                    //validation
                    if (string.IsNullOrEmpty(setting.Name))
                    {
                        Logger.Info("Validation failed : Column is empty");
                        throw new Exception("Column is empty");
                    }

                    if (oldSetting != null && !string.IsNullOrEmpty(oldSetting.Name))
                    {
                        if (oldSetting.Name.Trim() != setting.Name.Trim())
                        {
                            //Logger.Info("Validation failed : Column is not change");
                            //throw new Exception("Column is not change");
                            if (setting.SourceFlag == Contract.Explorer.SourceFlag.Teams)
                            {
                                RMNodeFlagDao.ClearDataByType((int)NodeFlagType.TeamsUniqueId);
                            }
                            else
                            {
                                RMNodeFlagDao.ClearDataByType((int)NodeFlagType.UniqueId);
                            }
                        }
                    }
                    else
                    {
                        if (setting.SourceFlag == Contract.Explorer.SourceFlag.Teams)
                        {
                            var spOldSetting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                            if (spOldSetting != null && !string.IsNullOrEmpty(spOldSetting.Name))
                            {
                                if (spOldSetting.Name.Trim() != setting.Name.Trim())
                                { 
                                    RMNodeFlagDao.ClearDataByType((int)NodeFlagType.TeamsUniqueId);
                                }
                            }
                        }
                    }
                    uniqueIdSettting.Name = setting.Name;
                    uniqueIdSettting.Prefix = setting.Prefix;
                    uniqueIdSettting.OverrideSPPrefix = setting.OverrrideSPPrefix;
                }
                else
                {
                    if (oldSetting != null && !string.IsNullOrEmpty(oldSetting.Name))
                    {
                        uniqueIdSettting.Name = oldSetting.Name;
                        uniqueIdSettting.Prefix = oldSetting.Prefix;
                        uniqueIdSettting.OverrideSPPrefix = setting.OverrrideSPPrefix;
                    }
                }
                uniqueIdSettting.IsActived = setting.IsActived;
                uniqueIdSettting.UniqueIdType = setting.SourceFlag switch
                {
                    SourceFlag.Teams => UniqueIdType.Teams,
                    SourceFlag.FileSystem => UniqueIdType.FileSystem,
                    _ => UniqueIdType.Default
                };
                await UniqueIdSettingDao.UpdateUniqueIdSettingAsync(uniqueIdSettting);
            }
            catch (Exception e)
            {
                Logger.Info("Failed to Update UniqueIdSetting : " + e.ToString());
                throw;
            }
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.UniqueIDSetting, BeforeHandler = typeof(RMUniqueIdSettingBeforeAuditHandler), AfterHandler = typeof(RMUniqueIdSettingAfterAuditHandler))]
        public async Task UpdateFileSystemUniqueIdSettingAsync(UniqueIdSetting setting)
        {
            try
            {
                var uniqueIdSettting = new RMUniqueIdSetting();

                Logger.Info(string.Format("Update file system UniqueIdSettings, Name : {0} , Prefix{1} , IsActived{2}, overridSPPrefix:{3}", setting.Name, setting.Prefix, setting.IsActived, setting.OverrrideSPPrefix));
                var oldSetting = await LoadFileSystemUniqueIdSettingAsync();

                if (setting.IsActived)
                {
                    if (string.IsNullOrEmpty(setting.Name))
                    {
                        Logger.Info("Validation failed : Column is empty");
                        throw new Exception("Column is empty");
                    }

                    if (oldSetting != null && !string.IsNullOrEmpty(oldSetting.Name) && oldSetting.Name.Trim() != setting.Name.Trim())
                    {
                        RMNodeFlagDao.ClearDataByType((int)NodeFlagType.UniqueId);
                    }

                    uniqueIdSettting.Name = setting.Name;
                    uniqueIdSettting.Prefix = setting.Prefix;
                    uniqueIdSettting.OverrideSPPrefix = setting.OverrrideSPPrefix;
                }
                else if (oldSetting != null && !string.IsNullOrEmpty(oldSetting.Name))
                {
                    uniqueIdSettting.Name = oldSetting.Name;
                    uniqueIdSettting.Prefix = oldSetting.Prefix;
                    uniqueIdSettting.OverrideSPPrefix = setting.OverrrideSPPrefix;
                }

                uniqueIdSettting.IsActived = setting.IsActived;
                uniqueIdSettting.UniqueIdType = UniqueIdType.FileSystem;
                await UniqueIdSettingDao.UpdateUniqueIdSettingAsync(uniqueIdSettting);
            }
            catch (Exception e)
            {
                Logger.Info("Failed to Update file system UniqueIdSetting : " + e.ToString());
                throw;
            }
        }

        public UniqueIdSetting LoadingUniqueIdSetting()
        {
            var result = new UniqueIdSetting();
            try
            {
                var setting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                if (setting != null)
                {
                    result.Prefix = setting.Prefix;
                    result.IsActived = setting.IsActived; 
                    result.Name = setting.Name;
                    result.OverrrideSPPrefix = setting.OverrideSPPrefix;
                }
            }
            catch (Exception e)
            {
                Logger.Info("Failed to Load UniqueIdSetting : " + e.ToString());
                throw;
            }
            return result;
        }

        public UniqueIdSetting LoadingFSUniqueIdSetting()
        {
            var result = new UniqueIdSetting();
            try
            {
                var setting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.FileSystem);
                if (setting == null)
                {
                    setting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                    setting.OverrideSPPrefix = false;
                }
                if (setting != null)
                {
                    result.Prefix = setting.Prefix;
                    result.IsActived = setting.IsActived;
                    result.Name = setting.Name;
                    result.OverrrideSPPrefix = setting.OverrideSPPrefix;
                }
            }
            catch (Exception e)
            {
                Logger.Info("Failed to Load UniqueIdSetting : " + e.ToString());
                throw;
            }
            return result;
        }

        public UniqueIdSetting LoadingTeamsUniqueIdSetting()
        {
            var result = new UniqueIdSetting();
            try
            {
                var setting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.Teams);
                if (setting == null)
                {
                    setting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                }
                if (setting != null)
                {
                    result.Prefix = setting.Prefix;
                    result.IsActived = setting.IsActived;
                    result.Name = setting.Name;
                    result.OverrrideSPPrefix = setting.OverrideSPPrefix;
                }
            }
            catch (Exception e)
            {
                Logger.Info("Failed to Load UniqueIdSetting : " + e.ToString());
                throw;
            }
            return result;
        }
        public async Task<string> LoadingCurrentIdAsync()
        {
            var result = string.Empty;
            try
            {
                var setting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                result = await FormateCurrentIdAsync(setting);
            }
            catch (Exception e)
            {
                Logger.Info("Failed to Load CurrentId : " + e.ToString());
                throw;
            }
            return result;
        }

        private string FormatNumber(long number)
        {
            var result = string.Empty;
            try
            {
                if (number < (Math.Pow(10, digit - 1)))
                {
                    result = number.ToString().PadLeft(10, '0');
                }
                else
                {
                    result = number.ToString();
                }
            }
            catch (Exception e)
            {
                Logger.Info(string.Format("Failed to formate number {0} : {1}", number, e.ToString()));
                throw;
            }
            return result;
        }

        private async Task<string> FormateCurrentIdAsync(RMUniqueIdSetting setting)
        {
            var result = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                if (setting != null)
                {
                    if (string.IsNullOrEmpty(setting.Prefix))
                    {
                        result = FormatNumber(await RMGlobalLocker.GetIdAsync(groupId));
                    }
                    else
                    {
                        result = string.Format(currentFormat, setting.Prefix, FormatNumber(await RMGlobalLocker.GetIdAsync(groupId)));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Info("Failed to formate currentId : " + e.ToString());
                throw;
            }
            return result;
        }

        /*private string PadLeft(string text, int digit, char fillChar)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            Char[] array = new Char[digit];
            int length = text.Length;

            for (int i = 0; i < digit - length; i++)
            {
                array[i] = fillChar;
            }

            text.ToCharArray().CopyTo(array, digit - length);
            return new string(array);
        }*/
        public string RunUniqueIDSettingScheduleJob(JobRunBy jobRunBy,JobType jobType)
        {
            string id = string.Empty;
            var needRunJob = true;
            try
            {
                if (jobType == JobType.UniqueIDSettingIncrementalSchedule && !ValidUniqueIdSetting())
                {
                    needRunJob = false;
                    Logger.Warn("Run spo uniqued id job is not required.");
                }
                if (jobType == JobType.SPOnPremUniqueIDSettingIncrementalSchedule && !ValidSPOnPremUniqueIdSetting())
                {
                    needRunJob = false;
                    Logger.Warn("Run sp-onprem uniqued id job is not required.");
                }
                if (needRunJob)
                {
                    var groupId = TenantLocalValue.LogonGroupId;
                    var loginName = TenantLocalValue.LogonUserEmail;
                    JobQueueDto jqDto = new JobQueueDto()
                    {
                        JobType = jobType,
                        JobRunType = jobRunBy,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName
                    };
                    id = mJobQueueService.AddToDBJobQueue(jqDto);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while RunUniqueIDSettingsScheduleJob,ERROR:{0}", ex.ToString());
            }
            return id;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunUniqueIDSettingJob, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealUnIDSettingScheduleJobAsync(JobRunBy jobRunBy,JobType jobType, string jobRunByUser = "")
        {
            string jobId = string.Empty;
            if (!ValidUniqueIdSetting())
            {
                return RecordsConstants.UniqueId_NoNeedRunJob;
            }
            jobId = RMJobService.CreateJob(jobType, string.IsNullOrEmpty(jobRunByUser)? "RM_TS_RunSchedule": jobRunByUser);

            List<string> runningJobs = RMJobService.GetRunningUniqueIDSettingJob();

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                await StartUniqueIDSettingsJobAsync(jobType, jobId, jobRunBy);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_UID_JobSkip");
                Logger.Info("unidsetting job has job running,so shedule job is skip");
            }
            return jobId;
        }
        public async System.Threading.Tasks.Task StartUniqueIDSettingsJobAsync(JobType jobType, string jobId, JobRunBy runBy)
        {
            var needRunJobNodes = GetNeedRunJobNodes();
            //int subJobCountInConfig = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfig = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            var emptyGroupNames = new List<string>();
            foreach (var nodeInfo in needRunJobNodes)
            {
                var sett = CloneSetting(nodeInfo);
                if (sett.NodeInfo == null)
                {
                    Logger.Info("no change, nodeinfo null.Id:{0}", sett.ScopeId);
                    continue;
                }
               
                var group = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(sett.NodeInfo);
                var browseTreeSourceType = group.NodeType == (int)NodeType.SkyDriveProSitesGroup ? RMBrowseTreeNodeSourceType.SkyDrivePro : RMBrowseTreeNodeSourceType.SharepointOnline;
                List <RMSPTreeNode> childNodes = await RMSPTreeService.BrowseAsync(group, false, browseTreeSourceType);
                if (childNodes == null || childNodes.Count == 0)
                {
                    Logger.Info("No sites in gourp {0}", group.Name);
                    emptyGroupNames.Add(group.Name);
                    continue;
                }
                foreach (RMSPTreeNode site in childNodes)
                {
                    if (RemoteNodeService.ValidOrphenSiteCollection(site))
                    {
                        Logger.Info("Skip orphen OneDrive site: {0}", site.FullPath);
                    }
                    else
                    {
                        availableSites.Add(site);
                    }
                }
            }

            if (availableSites.Count == 0)
            {
                Logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_NoSiteUnderContainers_Message{I18NEntity.Separator}{string.Join(";", emptyGroupNames)}");
                return;
            }

            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (RMSPTreeNode site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                { 
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfig);
                    if (currentSubjobIndex < subJobCountInConfig)  //一次只发X个子job, 后续在JobInfoUpdater中触发
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
                if (currentSubjobIndex < subJobCountInConfig)  //一次只发X个子job, 后续在JobInfoUpdater中触发
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

        private List<RMSharePointSetting> GetNeedRunJobNodes()
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
            if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
            {
                var teamContainerId = RMRemoteNodeDao.GetAllTeamsContainerIds();
                var filteredSpNodes = spNodes.Where(spNode => !teamContainerId.Contains(spNode.ScopeId.ToString())).ToList();
                return filteredSpNodes;
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

        private async System.Threading.Tasks.Task StartSPOnPremUniqueIDSettingsJobAsync(JobRunBy jobRunBy, string jobId, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            var needRunJobNodes = SharePointOnPremiseSettingDao.LoadShowUniqueIdSetting();
            var emptyGroupNames = new List<string>();
            foreach (var nodeInfo in needRunJobNodes)
            {
                var sett = CloneSetting(nodeInfo);
                if (sett.NodeInfo == null)
                {
                    Logger.Info("no change, nodeinfo null.Id:{0}", sett.ScopeId);
                    continue;
                }
                var group = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(sett.NodeInfo);
                List<RMSPTreeNode> sites = (await RMSharePointOnPremBrowseService.BrowseAsync(RMDtoConverter.ConvertRMTree2SPTree(group))).ConvertAll(n => RMDtoConverter.ConvertSPTree2RMTree(n));
                if (sites == null || sites.Count == 0)
                {
                    Logger.Info("No sites in gourp {0}", group.Name);
                    emptyGroupNames.Add(group.Name);
                    continue;
                }
                foreach (RMSPTreeNode site in sites)
                {
                    availableSites.Add(site);
                }
            }

            if (availableSites.Count == 0)
            {
                Logger.Warn("No available sc to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_NoSiteUnderContainers_Message{I18NEntity.Separator}{string.Join(";", emptyGroupNames)}");
                return;
            }

            Dictionary<string, List<RMSPTreeNode>> farmNodeGroup = new Dictionary<string, List<RMSPTreeNode>>();
            foreach (var node in availableSites)
            {
                if (string.IsNullOrWhiteSpace(node.FarmId))
                {
                    Logger.Warn("Node farm id is null, node id:{0}", node.Id);
                    continue;
                }

                if (farmNodeGroup.ContainsKey(node.FarmId))
                {
                    farmNodeGroup[node.FarmId].Add(node);
                }
                else
                {
                    farmNodeGroup.Add(node.FarmId, new List<RMSPTreeNode>() { node });
                }
            }

            int totalSubJobCount = 0;
            foreach (var nodes in farmNodeGroup.Values)
            {
                int tempSubJobCount = nodes.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? nodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : nodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
                totalSubJobCount += tempSubJobCount;
            }
            //int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB : availableNode.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB + 1;
            SubJobDao.UpdateSubJobCount(jobId, totalSubJobCount);

            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            foreach (var group in farmNodeGroup)
            {
                foreach (var site in group.Value)
                {
                    tempList.Add(site);
                    if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                    {
                        string farmId = tempList[0].FarmId;
                        string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, totalSubJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, farmId);
                        if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                        {
                            HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                            {
                                JobId = subJobId,
                                JobType = AvePoint.Hybrid.Contract.JobType.SPOnPremUniqueIDSetting,
                                TenantId = TenantLocalValue.LogonGroupId,
                                FarmId = farmId
                            });
                        }
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                if (tempList.Count > 0)
                {
                    string farmId = tempList[0].FarmId;
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, totalSubJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, farmId);
                    if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                        {
                            JobId = subJobId,
                            JobType = AvePoint.Hybrid.Contract.JobType.SPOnPremUniqueIDSetting,
                            TenantId = TenantLocalValue.LogonGroupId,
                            FarmId = farmId
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunSPOnPremUniqueIDSettingJob, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealSPOnPremUnIDSettingScheduleJobAsync(JobRunBy jobRunBy, JobType jobType, string jobRunByUser = "")
        {
            string jobId = string.Empty;
            if (!ValidSPOnPremUniqueIdSetting())
            {
                return RecordsConstants.UniqueId_NoNeedRunJob;
            }
            jobId = RMJobService.CreateJob(jobType, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);
            List<string> runningJobs = RMJobService.GetRunningSPOnPremUniqueIDSettingJob();
            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                await StartSPOnPremUniqueIDSettingsJobAsync(jobRunBy, jobId, jobType);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_UID_JobSkip");
                Logger.Info("unidsetting job has job running,so shedule job is skip");
            }
            return jobId;
        }

        public bool ValidUniqueIdSetting()
        {
            bool need = true;
            UniqueIdSetting setting = LoadingUniqueIdSetting();
            if (setting == null || !setting.IsActived || (!SharePointSettingDao.ExistShowUniqueIdSetting() && !OneDriveSettingDao.ExistShowUniqueIdSetting()))
            {
                Logger.Warn("Setting for unique id is empty or deactived.");
                need = false;
            }
            return need;
        }

        public bool ValidTeamsUniqueIdSetting()
        {
            bool need = true;
            UniqueIdSetting setting = LoadingTeamsUniqueIdSetting();
            if (setting == null || !setting.IsActived || !TeamsSettingDao.ExistShowUniqueIdSetting())
            {
                Logger.Warn("Setting for unique id is empty or deactived.");
                need = false;
            }
            return need;
        }

        public bool ValidSPOnPremUniqueIdSetting()
        {
            bool need = true;
            UniqueIdSetting setting = LoadingUniqueIdSetting();
            if (setting == null || !setting.IsActived || !SharePointOnPremiseSettingDao.ExistShowUniqueIdSetting())
            {
                Logger.Warn("Setting for unique id is empty or deactived.");
                need = false;
            }
            return need;
        }

        public string RunTeamsUniqueIDSettingScheduleJob(JobRunBy jobRunBy, JobType jobType)
        {
            string id = string.Empty;
            var needRunJob = true;
            try
            {
                if (jobType == JobType.TeamsUniqueIDSettingIncrementalSchedule && !ValidTeamsUniqueIdSetting())
                {
                    needRunJob = false;
                    Logger.Warn("Run teams uniqued id job is not required.");
                }
                if (needRunJob)
                {
                    var groupId = TenantLocalValue.LogonGroupId;
                    var loginName = TenantLocalValue.LogonUserEmail;
                    JobQueueDto jqDto = new JobQueueDto()
                    {
                        JobType = jobType,
                        JobRunType = jobRunBy,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName
                    };
                    id = mJobQueueService.AddToDBJobQueue(jqDto);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while RunUniqueIDSettingsScheduleJob,ERROR:{0}", ex.ToString());
            }
            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunTeamsUniqueIDSettingJob, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunTeamsIDSettingScheduleJobAsync(JobRunBy jobRunBy, JobType jobType, string jobRunByUser = "")
        {
            string jobId = string.Empty;
            if (!ValidTeamsUniqueIdSetting())
            {
                return RecordsConstants.UniqueId_NoNeedRunJob;
            }
            jobId = RMJobService.CreateJob(jobType, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);

            List<string> runningJobs = RMJobService.GetTeamsRunningUniqueIDSettingJob();

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                await StartTeamsUniqueIDSettingsJobAsync(jobType, jobId, jobRunBy);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_UID_JobSkip");
                Logger.Info("unidsetting job has job running,so shedule job is skip");
            }
            return jobId;
        }
        public async Task StartTeamsUniqueIDSettingsJobAsync(JobType jobType, string jobId, JobRunBy runBy)
        {
            var needRunJobNodes = GetTeamsNeedRunJobNodes();
            int subJobCountInConfig = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMSPTreeNode> availableTeamsNodes = new List<RMSPTreeNode>();
            var emptyGroupNames = new List<string>();
            foreach (var nodeInfo in needRunJobNodes)
            {
                var sett = CloneSetting(nodeInfo);
                if (sett.NodeInfo == null)
                {
                    Logger.Info("no change, nodeinfo null.Id:{0}", sett.ScopeId);
                    continue;
                }

                var group = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(sett.NodeInfo);
                List<RMSPTreeNode> teamsNodes = await RMTeamsTreeService.BrowseAsync(group);
                if (teamsNodes == null || teamsNodes.Count == 0)
                {
                    Logger.Info("No sites in gourp {0}", group.Name);
                    emptyGroupNames.Add(group.Name);
                    continue;
                }
                foreach (RMSPTreeNode teamsNode in teamsNodes)
                {
                    availableTeamsNodes.Add(teamsNode);
                }
            }

            if (availableTeamsNodes.Count == 0)
            {
                Logger.Warn("No available teams to run");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_NoSiteUnderContainers_Message{I18NEntity.Separator}{string.Join(";", emptyGroupNames)}");
                return;
            }

            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            int subJobCount = availableTeamsNodes.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableTeamsNodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableTeamsNodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (RMSPTreeNode teamsNode in availableTeamsNodes)
            {
                tempList.Add(teamsNode);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfig);
                    if (currentSubjobIndex < subJobCountInConfig)  //一次只发X个子job, 后续在JobInfoUpdater中触发
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
                if (currentSubjobIndex < subJobCountInConfig)  //一次只发X个子job, 后续在JobInfoUpdater中触发
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

        private List<RMTeamsSetting> GetTeamsNeedRunJobNodes()
        {
            var teamsNodes = TeamsSettingDao.LoadShowUniqueIdSetting();
            
            return teamsNodes;
        }


        private RMSharePointSetting CloneSetting(RMSharePointSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMSharePointSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMSharePointSetting>(xml);
            return result;
        }

        private RMSharePointOnPremiseSetting CloneSetting(RMSharePointOnPremiseSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMSharePointOnPremiseSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMSharePointOnPremiseSetting>(xml);
            return result;
        }
        
        private RMTeamsSetting CloneSetting(RMTeamsSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMTeamsSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMTeamsSetting>(xml);
            return result;
        }

        private async Task<RMUniqueIdSetting> LoadFileSystemUniqueIdSettingAsync()
        {
            var oldSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.FileSystem);
            var enableUniqueIdsetting = await AgentMgmtService.CheckIfEnableFSUniqueIdSetting();
            if (!enableUniqueIdsetting)
            {
                throw new Exception("RM_JS_FS_UniqueSetting_SaveFailed");
            }

            return oldSetting;
        }

        // UniqueIdSettingDao.LoadingUniqueIdSetting
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, string farmId = "")
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, FarmId= farmId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            SubJobDao.CreateJob(subJob);
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
    }
}
