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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Schedule;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Global.Utils;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Tenant;

namespace RATeams.Upgrade
{
    public class TeamsNodeSettingUpgradeProcessor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(TeamsNodeSettingUpgradeProcessor));

        private static readonly ISharePointSettingDao s_sharePointSettingDao = PlatformWindsorManager.GetService<ISharePointSettingDao>();

        private static readonly IRMArchiverSettingDao s_archiverSettingDao = PlatformWindsorManager.GetService<IRMArchiverSettingDao>();

        private static readonly ITeamsChannelConflictSettingDao s_teamsChannelConflictSettingDao = PlatformWindsorManager.GetService<ITeamsChannelConflictSettingDao>();

        private static readonly IEXOSettingRuleDao s_exoSettingRuleDao = PlatformWindsorManager.GetService<IEXOSettingRuleDao>();

        private static readonly IRMRemoteNodeDao s_remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static readonly ITeamsSettingDao s_teamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();

        private static readonly IScheduleService s_scheduleService = PlatformWindsorManager.GetService<IScheduleService>();

        private static readonly ITeamsSettingTreeService s_teamsSettingTreeService = PlatformWindsorManager.GetService<ITeamsSettingTreeService>();

        private static readonly IBrowseTreeService s_browseTreeService = PlatformWindsorManager.GetService<IBrowseTreeService>();

        private static readonly IRMTeamsSettingsService s_teamsSettingsService = PlatformWindsorManager.GetService<IRMTeamsSettingsService>();

        private static readonly IUniqueIdSettingService s_uniqueIdSettingService = PlatformWindsorManager.GetService<IUniqueIdSettingService>();

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly IRecordOwnerDao s_recordOwnerDao = PlatformWindsorManager.GetService<IRecordOwnerDao>();

        private const string ChannelContianerId = "41cfe969-e07b-45cb-a7d0-b022f967e929";
        private const int NodeLevel_O365GroupSitesEntire = (int)NodeLevel.Office365GroupEntire;

        private readonly List<string> _teamsGroupIds;

        private static readonly TeamsUpgradeJobManager s_reportManager = new();

        public TeamsNodeSettingUpgradeProcessor(string jobId)
        {
            _teamsGroupIds = s_remoteNodeDao.GetAllTeamsContainerIds().Where(item => !item.Equals(ChannelContianerId, StringComparison.CurrentCultureIgnoreCase)).ToList();
            var total = s_sharePointSettingDao.GetSettingsCountBySiteGroupIds([.. _teamsGroupIds.Select(item => new Guid(item))]);
            total += s_archiverSettingDao.LoadArchiverSettingCountBySiteGroupIds([.. _teamsGroupIds.Select(item => new Guid(item))]);
            s_reportManager.Init(jobId, JobType.TeamsNodeSettingUpgrade, total);
        }

        public void Run()
        {
            var TaskForIL = ProcessILSettings();
            var TaskForSO = ProcessSOSettings();

            Task.WaitAll(TaskForIL, TaskForSO);

            var status = s_reportManager.SetJobFinished();
            if(status != JobStatus.Failed)
            {
                var keyValueEntity = new RMKeyValue() { Key = KeyNameCollection.HasUpgradeTeamsSettings, Value = "True" };
                s_keyValueDao.SaveOrUpdateAsync(keyValueEntity).GetAwaiter().GetResult();
                ApplySetting();
            }
        }

        public async Task ProcessILSettings()
        {
            try
            {
                s_logger.Info($"Start to upgrade teams IL settings");
                var teamsNodeILSettings = s_sharePointSettingDao.GetAllSettingsBySiteGroupIds([.. _teamsGroupIds.Select(item => new Guid(item))]);
                var teamsGroupILSettings = teamsNodeILSettings.Where(item => item.ScopeId == item.SiteGroupId).ToList();
                s_logger.Info($"Teams group IL settings count is [{teamsGroupILSettings.Count}]");

                teamsGroupILSettings.ForEach(item => item.SettingTime = 0);
                var addContainerResult = await s_teamsSettingDao.AddTeamsSettingAsync(teamsGroupILSettings, Guid.Empty);
                UpdateRecordOwner(teamsGroupILSettings, addContainerResult);
                s_reportManager.Increase(teamsGroupILSettings.Count);
                s_reportManager.AddRecordReport(teamsGroupILSettings.ConvertAll(item =>
                {
                    return new JMConvertStubJobDetails()
                    {
                        Action = (int)TeamsUpgradeAction.ILUpgrade,
                        FullPath = GetTeamsGroupUrl(item.FullPath),
                        FinishTime = DateTime.UtcNow.Ticks,
                        Status = JobDetailsStatus.Successful
                    };
                }));
                s_reportManager.HasSucceedDetail = true;

                var conflictSettings = s_teamsChannelConflictSettingDao.GetAllTeamsConflictChannelSettings(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.Teams.ModuleType.LifeCycle);
                var conflcitTeamsIds = conflictSettings.Select(item => item.PartitionKey).Distinct().ToList();

                foreach (var teamsGroupSetting in teamsGroupILSettings)
                {
                    s_logger.Info($"Start to upgrade teams group [{teamsGroupSetting.FullPath}] IL settings");
                    var teamsSettings = teamsNodeILSettings.Where(item => item.SiteGroupId == teamsGroupSetting.ScopeId && item.SiteId != Guid.Empty).ToList();

                    s_logger.Info($"Teams group [{teamsGroupSetting.FullPath}] teams node IL setting count is: [{teamsSettings.Count}]");
                    var teamsNodes = s_remoteNodeDao.GetRemoteSiteCollectionByIds(teamsSettings.Select(item => item.SiteId.ToString()).Distinct().ToList());

                    var currentGroupSchedules = s_scheduleService.GetScheduleByTypeAndGroupIdService(teamsGroupSetting.ScopeId.ToString().ToLowerInvariant(), ScheduleType.DisposalSchedule);
                    s_logger.Info($"Teams group [{teamsGroupSetting.FullPath}] schedule setting count is: [{currentGroupSchedules.Count}]");

                    var groupSchedule = currentGroupSchedules.Where(item => item.ProfileId == teamsGroupSetting.ScopeId.ToString().ToLowerInvariant()).FirstOrDefault();
                    if(groupSchedule != null)
                    {
                        groupSchedule.JobCategory = ScheduleType.TeamsDisposalSchedule;
                        s_scheduleService.UpdateTeamsScheduleService(new List<ScheduleInfo> { groupSchedule });
                    }
                    var onlyScheduleSettings = currentGroupSchedules.Where(item => teamsNodes.All(teams => !item.ProfileId.Contains(teams.id.ToString(), StringComparison.InvariantCultureIgnoreCase))).ToList();
                    await UpdateOnlySchedulesUnderGroup(teamsGroupSetting.ScopeId.ToString(), onlyScheduleSettings, ScheduleType.TeamsDisposalSchedule, teamsGroupSetting);
                    foreach (var teamsNode in teamsNodes)
                    {
                        try
                        {
                            var settingList = new List<RMSharePointSetting>();
                            var teamId = teamsNode.TeamId;

                            var teamsSchedules = currentGroupSchedules.Where(item => item.ProfileId.Contains(teamsNode.id.ToString(), StringComparison.InvariantCultureIgnoreCase)).ToList();
                            s_logger.Info($"Current container [{teamsGroupSetting.FullPath}] teams node [{teamsNode.url}] and children schedule count is: [{teamsSchedules.Count}].");

                            var teamsSetting = teamsSettings.Where(item => item.ScopeId == new Guid(teamsNode.id) && item.SiteGroupId == teamsGroupSetting.ScopeId).FirstOrDefault();

                            var teamsTreeNode = await BuildTeamsSPTreeNode(teamsNode, teamsSchedules, teamsSetting, teamsGroupSetting, null);
                            var teamsChildSettings = teamsNodeILSettings.Where(item => item.SiteGroupId == teamsGroupSetting.ScopeId && item.SiteId == new Guid(teamsNode.id) && item.ScopeId != item.SiteId).ToList();
                            if (teamsChildSettings.Count > 0)
                            {
                                teamsChildSettings = BuildTeamsChildrenSPTreeNode(teamsTreeNode, teamsChildSettings, teamsSchedules);
                                s_logger.Info($"Current container [{teamsGroupSetting.FullPath}] teams node [{teamsNode.url}] children setting count is: [{teamsChildSettings.Count}].");
                                settingList.AddRange(teamsChildSettings);
                            }

                            if (teamsSetting != null)
                            {
                                if (conflcitTeamsIds.Contains(teamsSetting.ScopeId.ToString()))
                                {
                                    teamsSetting.SettingTime = 0;
                                }
                                teamsSetting.ScopeId = new Guid(teamId);
                                teamsSetting.SiteId = Guid.Empty;
                                teamsSetting.FullPath = teamsNode.Name;
                                teamsSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(teamsTreeNode);
                                settingList.Add(teamsSetting);
                            }
                            UpdateILSchedulesUnderGroup(teamsGroupSetting.ScopeId.ToString(), teamsSchedules, ScheduleType.TeamsDisposalSchedule, teamId, teamsTreeNode);

                            var addResult = await s_teamsSettingDao.AddTeamsSettingAsync(settingList, new Guid(teamId));
                            UpdateRecordOwner(settingList, addResult);
                            s_reportManager.Increase(settingList.Count);

                            s_reportManager.AddRecordReport(settingList.ConvertAll(item =>
                            {
                                return new JMConvertStubJobDetails()
                                {
                                    Action = (int)TeamsUpgradeAction.ILUpgrade,
                                    FullPath = item.FullPath,
                                    FinishTime = DateTime.UtcNow.Ticks,
                                    Status = JobDetailsStatus.Successful
                                };
                            }));
                        }
                        catch (Exception e)
                        {
                            s_logger.Error($"Current container [{teamsGroupSetting.FullPath}] teams node [{teamsNode.url}] upgrade failed, error: {e}.");
                            s_reportManager.AddRecordReport(new List<JMConvertStubJobDetails>()
                            {
                                new()
                                {
                                    Action = (int)TeamsUpgradeAction.ILUpgrade,
                                    FullPath = teamsNode.url,
                                    FinishTime = DateTime.UtcNow.Ticks,
                                    Status = JobDetailsStatus.Failed
                                },
                            });
                            s_reportManager.HasFailedDetail = true;
                        }
                    }  
                }
            }
            catch(Exception e)
            {
                s_logger.Error($"Upgrade IL settings failed, error: {e}.");
                s_reportManager.HasFailedDetail = true;
            }
        }

        public async Task ProcessSOSettings()
        {
            try
            {
                s_logger.Info($"Start to upgrade teams SO settings");
                var teamsNodeSOSettings = s_archiverSettingDao.LoadArchiverSettingBySiteGroupIds([.. _teamsGroupIds.Select(item => new Guid(item))]);
                var teamsGroupNodes = s_remoteNodeDao.GetWebApplicationByIds(_teamsGroupIds).Distinct().ToList();
                s_logger.Info($"Teams group IL settings count is [{teamsNodeSOSettings.Count}]");

                foreach (var teamsGroupId in _teamsGroupIds)
                {
                    var teamsSettings = teamsNodeSOSettings.Where(item => item.SiteGroupId == new Guid(teamsGroupId) && item.SiteId != Guid.Empty).ToList();
                    s_logger.Info($"Teams group [{teamsGroupId}] teams node SO setting count is: [{teamsSettings.Count}]");
                    var teamsGroupNode = teamsGroupNodes.FirstOrDefault(t => t.id == teamsGroupId);
                    var teamsNodes = s_remoteNodeDao.GetRemoteSiteCollectionByIds(teamsSettings.Select(item => item.SiteId.ToString()).Distinct().ToList());
                    
                    var currentGroupSchedules = s_scheduleService.GetScheduleByTypeAndGroupIdService(teamsGroupId, ScheduleType.SPArchiveJobSchedule);
                    s_logger.Info($"Teams group [{teamsGroupId}] schedule setting count is: [{currentGroupSchedules.Count}]");

                    var groupSchedule = currentGroupSchedules.Where(item => item.ProfileId.Equals(teamsGroupId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (groupSchedule != null)
                    {
                        groupSchedule.JobCategory = ScheduleType.TeamsArchiveJobSchedule;
                        s_scheduleService.UpdateTeamsScheduleService(new List<ScheduleInfo> { groupSchedule });
                    }

                    var teamsSchedules = currentGroupSchedules.Where(item => !item.ProfileId.Equals(teamsGroupId, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    s_logger.Info($"Current container [{teamsGroupId}] teams node schedule count is: [{teamsSchedules.Count}].");
                    await UpdateSOSchedulesUnderGroup(teamsGroupId, teamsSchedules, ScheduleType.TeamsArchiveJobSchedule, groupSchedule, teamsGroupNode);
                    foreach (var teamsNode in teamsNodes)
                    {
                        try
                        {
                            var settingList = new List<RMArchiverSetting>();
                            var teamsId = teamsNode.TeamId;

                            //var teamsSchedules = currentGroupSchedules.Where(item => item.ProfileId.Contains(teamsNode.id.ToString(), StringComparison.InvariantCultureIgnoreCase)).ToList();
                            var teamsChildSettings = teamsNodeSOSettings.Where(item => item.SiteGroupId == new Guid(teamsGroupId) && item.SiteId == new Guid(teamsNode.id) && item.SPObjectId != item.SiteId).ToList();
                            s_logger.Info($"Current container [{teamsGroupId}] teams node [{teamsNode.url}] children setting count is: [{teamsChildSettings.Count}].");
                            if (teamsChildSettings.Count > 0)
                            {
                                s_logger.Info($"Current container [{teamsGroupId}] teams node [{teamsNode.url}] children setting count is: [{teamsChildSettings.Count}].");
                                settingList.AddRange(teamsChildSettings);
                            }

                            var teamsSetting = teamsSettings.Where(item => item.SPObjectId == new Guid(teamsNode.id) && item.SiteGroupId == new Guid(teamsGroupId)).FirstOrDefault();
                            if (teamsSetting != null)
                            {
                                teamsSetting.SPObjectId = new Guid(teamsNode.TeamId);
                                teamsSetting.Url = teamsNode.Name;
                                teamsSetting.SiteId = Guid.Empty;
                                settingList.Add(teamsSetting);
                            }
                            
                            settingList.ForEach(item =>
                            {
                                //var currentSchedule = teamsSchedules.FirstOrDefault(s => s.ProfileId.Split('|')[^1].Equals(item.SPObjectId.ToString(), StringComparison.InvariantCultureIgnoreCase));
                                item.ContentSourceType = (int)ContentSourceType.Teams;
                                item.TeamsId = new Guid(teamsNode.TeamId);
                            });
                            s_archiverSettingDao.UpgradeTeamsSettings(settingList);
                            s_reportManager.Increase(settingList.Count);

                            s_reportManager.AddRecordReport(settingList.ConvertAll(item =>
                            {
                                return new JMConvertStubJobDetails()
                                {
                                    Action = (int)TeamsUpgradeAction.SOUpgrade,
                                    FullPath = item.Url,
                                    FinishTime = DateTime.UtcNow.Ticks,
                                    Status = JobDetailsStatus.Successful
                                };
                            }));

                            s_reportManager.HasSucceedDetail = true;
                        }
                        catch(Exception e)
                        {
                            s_logger.Error($"Update container [{teamsGroupId}] teams node [{teamsNode.url}] failed, error: {e}");
                            s_reportManager.AddRecordReport(new List<JMConvertStubJobDetails>()
                            {
                                new()
                                {
                                    Action = (int)TeamsUpgradeAction.SOUpgrade,
                                    FullPath = teamsNode.url,
                                    FinishTime = DateTime.UtcNow.Ticks,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = e.Message,
                                },
                            });
                            s_reportManager.HasFailedDetail = true;
                        }
                    }
                }
                var teamsGroupSOSettings = teamsNodeSOSettings.Where(item => item.SPObjectId == item.SiteGroupId).ToList();
                teamsGroupSOSettings.ForEach(item => item.ContentSourceType = (int)ContentSourceType.Teams);
                s_archiverSettingDao.UpgradeTeamsSettings(teamsGroupSOSettings);
                s_reportManager.Increase(teamsGroupSOSettings.Count);
                s_reportManager.AddRecordReport(teamsGroupSOSettings.ConvertAll(item =>
                {
                    return new JMConvertStubJobDetails()
                    {
                        Action = (int)TeamsUpgradeAction.SOUpgrade,
                        FullPath = GetTeamsGroupUrl(item.Url),
                        FinishTime = DateTime.UtcNow.Ticks,
                        Status = JobDetailsStatus.Successful
                    };
                }));
            }
            catch (Exception e)
            {
                s_logger.Error($"Update SO setting failed, error: {e}");
                s_reportManager.HasFailedDetail = true;
            }
        }

        private List<ScheduleInfo> UpdateILSchedulesUnderGroup(string groupId, List<ScheduleInfo> ScheduleInfoes, ScheduleType currentScheduleType, string teamId, RMSPTreeNode teamsTreeNode)
        {
            try
            {
                foreach (var teamsNodeSchedule in ScheduleInfoes)
                {
                    try
                    {
                        if (teamsNodeSchedule == null)
                        {
                            continue;
                        }

                        var profileId = BuildScheduleProfileId(groupId, teamId, teamsNodeSchedule.ProfileId);
                        var treeNode = JsonConvert.DeserializeObject<RMSPTreeNode>(teamsNodeSchedule.Extentions);
                        RMSPTreeNode newTreeNode = null;
                        if (treeNode != null && treeNode.Level == (int)NodeLevel.SiteCollection)
                        {
                            newTreeNode = teamsTreeNode;
                            newTreeNode.SkipRemoveContentAndDestroyAction = treeNode.SkipRemoveContentAndDestroyAction;
                            newTreeNode.IsEnableSuperUserDecrypt = treeNode.IsEnableSuperUserDecrypt;
                        }
                        else
                        {
                            newTreeNode = BuildTeamsTreeNode(treeNode, teamsTreeNode);
                        }
                        teamsNodeSchedule.JobCategory = currentScheduleType;
                        teamsNodeSchedule.ProfileId = profileId;
                        teamsNodeSchedule.Extentions = JsonConvert.SerializeObject(newTreeNode);
                    }
                    catch(Exception ex)
                    {
                        s_logger.Error($"Build schedule [{teamsNodeSchedule.Id}] failed, error: {ex}");
                    }
                }
                s_scheduleService.UpdateTeamsScheduleService(ScheduleInfoes);
            }
            catch(Exception e)
            {
                s_logger.Error($"Update schedules failed, error: {e}");
            }
            
            return ScheduleInfoes;
        }

        private async Task<List<ScheduleInfo>> UpdateSOSchedulesUnderGroup(string groupId, List<ScheduleInfo> ScheduleInfoes, ScheduleType currentScheduleType, ScheduleInfo groupSchedule, RemoteWebApplication teamsGroupNode)
        {
            try
            {
                var siteIdList = ScheduleInfoes.Select(s => s.ProfileId.Split('|')[1]).ToList();
                var teamsNodes = s_remoteNodeDao.GetRemoteSiteCollectionByIds(siteIdList).Distinct().ToList();
                foreach (var teamsNodeSchedule in ScheduleInfoes)
                {
                    try
                    {
                        if (teamsNodeSchedule == null)
                        {
                            continue;
                        }
                        var teamsNode = teamsNodes.Where(t => teamsNodeSchedule.ProfileId.Contains(t.id)).FirstOrDefault();
                        if (teamsNode == null)
                        {
                            continue;
                        }
                        var teamsTreeNode = await BuildSOTeamsSPTreeNode(teamsNode, ScheduleInfoes, groupSchedule, teamsGroupNode);
                        var profileId = BuildScheduleProfileId(groupId, teamsNode.TeamId, teamsNodeSchedule.ProfileId);
                        var treeNode = JsonConvert.DeserializeObject<RMSPTreeNode>(teamsNodeSchedule.Extentions);
                        RMSPTreeNode newTreeNode = null;
                        if (treeNode != null && treeNode.Level == (int)NodeLevel.SiteCollection)
                        {
                            newTreeNode = teamsTreeNode;
                            newTreeNode.Rules = treeNode.Rules;
                            newTreeNode.IsManagedMetadataService = treeNode.IsManagedMetadataService;
                            newTreeNode.IsEnableSuperUserDecrypt = treeNode.IsEnableSuperUserDecrypt;
                            newTreeNode.EnableArchiverManagement = treeNode.EnableArchiverManagement;
                            newTreeNode.SPType = (int)SPType.Moss;
                            newTreeNode.BposInfo = null;
                        }
                        else
                        {
                            newTreeNode = BuildTeamsTreeNode(treeNode, teamsTreeNode);
                        }
                        teamsNodeSchedule.JobCategory = currentScheduleType;
                        teamsNodeSchedule.ProfileId = profileId;
                        teamsNodeSchedule.Extentions = JsonConvert.SerializeObject(newTreeNode);
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error($"Build schedule [{teamsNodeSchedule.Id}] failed, error: {ex}");
                    }
                }
                s_scheduleService.UpdateTeamsScheduleService(ScheduleInfoes);
            }
            catch (Exception e)
            {
                s_logger.Error($"Update schedules failed, error: {e}");
            }

            return ScheduleInfoes;
        }

        private async Task<List<ScheduleInfo>> UpdateOnlySchedulesUnderGroup(string groupId, List<ScheduleInfo> ScheduleInfoes, ScheduleType currentScheduleType, RMSharePointSetting teamGroupSetting)
        {
            try
            {
                var siteIdList = ScheduleInfoes.Select(s => s.ProfileId.Split('|')[1]).ToList();
                var teamsNodes = s_remoteNodeDao.GetRemoteSiteCollectionByIds(siteIdList).Distinct().ToList();
                var addedTeamsNodeSchedules = new List<ScheduleInfo>();
                foreach (var teamsNodeSchedule in ScheduleInfoes)
                {
                    var treeNode = JsonConvert.DeserializeObject<RMSPTreeNode>(teamsNodeSchedule.Extentions);
                    try
                    {
                        var teamsNode = teamsNodes.Where(t => teamsNodeSchedule.ProfileId.Contains(t.id)).FirstOrDefault();
                        if(teamsNode == null)
                        {
                            continue;
                        }
                        var teamsTreeNode = await BuildTeamsSPTreeNode(teamsNode, ScheduleInfoes, null, teamGroupSetting, teamsNodeSchedule.ProfileId);
                        var profileId = BuildScheduleProfileId(groupId, teamsNode.TeamId, teamsNodeSchedule.ProfileId);
                        
                        RMSPTreeNode newTreeNode = null;
                        if (treeNode != null && treeNode.Level == (int)NodeLevel.SiteCollection)
                        {
                            newTreeNode = teamsTreeNode;
                            newTreeNode.SkipRemoveContentAndDestroyAction = treeNode.SkipRemoveContentAndDestroyAction;
                            newTreeNode.IsEnableSuperUserDecrypt = treeNode.IsEnableSuperUserDecrypt;
                        }
                        else
                        {
                            newTreeNode = BuildTeamsTreeNode(treeNode, teamsTreeNode);
                        }
                        teamsNodeSchedule.JobCategory = currentScheduleType;
                        teamsNodeSchedule.ProfileId = profileId;
                        teamsNodeSchedule.Extentions = JsonConvert.SerializeObject(newTreeNode);
                        addedTeamsNodeSchedules.Add(teamsNodeSchedule);
                        s_reportManager.AddRecordReport((int)TeamsUpgradeAction.ILUpgrade, treeNode.FullPath, JobDetailsStatus.Successful);
                        s_reportManager.HasSucceedDetail = true;
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error($"Build schedule [{teamsNodeSchedule.Id}] failed, error: {ex}");
                        s_reportManager.AddRecordReport((int)TeamsUpgradeAction.ILUpgrade, treeNode.FullPath, JobDetailsStatus.Failed, ex.Message);
                        s_reportManager.HasFailedDetail = true;
                    }
                }
                s_scheduleService.UpdateTeamsScheduleService(addedTeamsNodeSchedules);
            }
            catch (Exception e)
            {
                s_logger.Error($"Update schedules failed, error: {e}");
            }

            return ScheduleInfoes;
        }

        private static void UpdateRecordOwner(List<RMSharePointSetting> sharePointSettings, List<RMTeamsSetting> teamsSettings)
        {
            try
            {
                var mapping = sharePointSettings.ToDictionary(item => item.Id, item => teamsSettings.FirstOrDefault(teams => teams.ScopeId == item.ScopeId)?.Id);
                s_recordOwnerDao.UpdateRecordOwnerToTeams(mapping, RecordOwnerSettingType.AISharePointOnline);
            }
            catch(Exception e)
            {
                s_logger.Error($"Update records owner failed, error: {e}");
            }
        }

        private static string BuildScheduleProfileId(string groupId, string teamId, string profileId)
        {
            var pathLength = profileId.Split('|').Length;
            if (pathLength == 2)
            {
                return $"{groupId.ToLowerInvariant()}|{teamId.ToLowerInvariant()}";
            }
            else if (pathLength > 2)
            {
                return profileId.Replace(groupId.ToLowerInvariant(), $"{groupId.ToLowerInvariant()}|{teamId.ToLowerInvariant()}");
            }

            return profileId;
        }

        private async Task<RMSPTreeNode> BuildTeamsSPTreeNode(RemoteSiteCollection teamsNode, List<ScheduleInfo> ScheduleInfoes, RMSharePointSetting teamSetting, RMSharePointSetting teamGroupSetting, string profileId)
        {
            var spTreeNode = new RMSPTreeNode();
            if (!string.IsNullOrEmpty(profileId))
            {
                spTreeNode.DisposeScheduleInfo = new ScheduleInfo
                {
                    ProfileId = profileId
                };
            }
            var farmNode = s_teamsSettingTreeService.LoadFarm()[0];
            if (farmNode == null)
            {
                return spTreeNode;
            }

            var farmTreeNode = await GetFormTreeNode(farmNode);
            if (teamSetting != null) 
            {
                spTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(teamSetting.NodeInfo);
                spTreeNode.Parent.Parent = farmTreeNode;
                spTreeNode.Parent.ParentId = farmTreeNode.Id;
            }
            else
            {
                try
                {
                    spTreeNode = RMDtoConverter.ConvertRemoteSite2RMTeamsTree(teamsNode);
                    var groupTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(teamGroupSetting.NodeInfo);
                    spTreeNode.Parent = groupTreeNode;
                    spTreeNode.Parent.Id = groupTreeNode.Id;
                    spTreeNode.Parent.ParentId = groupTreeNode.Id;
                    spTreeNode.Parent.Parent = farmTreeNode;
                    spTreeNode.Parent.ParentId = farmTreeNode.Id;
                }
                catch(Exception ex)
                {
                    s_logger.Error($"Build teams [{teamsNode.Name}] tree node failed, error: {ex}");
                    throw new Exception("RM_JM_JD_TeamsUpgrade_SiteExpired");
                }
            }
                
            try
            {
                spTreeNode.Id = teamsNode.TeamId;
                spTreeNode.TeamsId = teamsNode.TeamId;
                spTreeNode.Level = NodeLevel_O365GroupSitesEntire;
                spTreeNode.Name = teamsNode.Name;
                spTreeNode.NodeType = 1014;
                spTreeNode.FullPath = teamsNode.Name;
                spTreeNode.Type = ContentSourceType.Teams;
                spTreeNode.SPObjectId = teamsNode.TeamId;
                spTreeNode.DisplayName = teamsNode.url;

                if (spTreeNode.DisposeScheduleInfo != null)
                {
                    var scheduleInfo = ScheduleInfoes.Where(s => s.ProfileId == spTreeNode.DisposeScheduleInfo.ProfileId).FirstOrDefault();
                    if (scheduleInfo != null)
                    {
                        spTreeNode.DisposeScheduleInfo.ProfileId = BuildScheduleProfileId(spTreeNode.Parent.Id, teamsNode.TeamId, spTreeNode.DisposeScheduleInfo.ProfileId);
                        spTreeNode.DisposeScheduleInfo.JobCategory = ScheduleType.TeamsDisposalSchedule;
                    }
                }
                return spTreeNode;
            }
            catch(Exception e)
            {
                s_logger.Error($"Build teams [{teamsNode.Name}] tree node failed, error: {e}");
                throw;
            }
        }

        private async Task<RMSPTreeNode> BuildSOTeamsSPTreeNode(RemoteSiteCollection teamsNode, List<ScheduleInfo> ScheduleInfoes, ScheduleInfo teamGroupSchedule, RemoteWebApplication teamsGroupNode)
        {
            var spTreeNode = new RMSPTreeNode();
            var farmNode = s_teamsSettingTreeService.LoadFarm()[0];
            if (farmNode == null)
            {
                return spTreeNode;
            }

            var farmTreeNode = await GetFormTreeNode(farmNode);
            try
            {
                spTreeNode = RMDtoConverter.ConvertRemoteSite2RMTeamsTree(teamsNode);
                var groupTreeNode = RMDtoConverter.ConvertRemoteWebApplication2RMTeamsTree(teamsGroupNode);
                spTreeNode.Parent = groupTreeNode;
                spTreeNode.ParentId = groupTreeNode.Id;
                spTreeNode.Parent.Parent = farmTreeNode;
                spTreeNode.Parent.ParentId = farmTreeNode.Id;
            }
            catch (Exception ex)
            {
                s_logger.Error($"Build teams [{teamsNode.Name}] tree node failed, error: {ex}");
                throw new Exception("RM_JM_JD_TeamsUpgrade_SiteExpired");
            }
            
            try
            {
                spTreeNode.Id = teamsNode.TeamId;
                spTreeNode.TeamsId = teamsNode.TeamId;
                spTreeNode.Level = NodeLevel_O365GroupSitesEntire;
                spTreeNode.Name = teamsNode.Name;
                spTreeNode.NodeType = 1014;
                spTreeNode.FullPath = teamsNode.Name;
                spTreeNode.Type = ContentSourceType.Teams;
                spTreeNode.SPObjectId = teamsNode.TeamId;
                spTreeNode.DisplayName = teamsNode.url;


                if (spTreeNode.DisposeScheduleInfo != null)
                {
                    var scheduleInfo = ScheduleInfoes.Where(s => s.ProfileId == spTreeNode.DisposeScheduleInfo.ProfileId).FirstOrDefault();
                    if (scheduleInfo != null)
                    {
                        spTreeNode.DisposeScheduleInfo.ProfileId = BuildScheduleProfileId(spTreeNode.Parent.Id, teamsNode.TeamId, spTreeNode.DisposeScheduleInfo.ProfileId);
                        spTreeNode.DisposeScheduleInfo.JobCategory = ScheduleType.TeamsDisposalSchedule;
                    }
                }
                return spTreeNode;
            }
            catch (Exception e)
            {
                s_logger.Error($"Build teams [{teamsNode.Name}] tree node failed, error: {e}");
                throw;
            }
        }

        private List<RMSharePointSetting> BuildTeamsChildrenSPTreeNode(RMSPTreeNode teamsTreeNode, List<RMSharePointSetting> settings, List<ScheduleInfo> ScheduleInfoes)
        {
            foreach(var setting in settings)
            {
                try
                {
                    s_logger.Info($"Build teams children node [{setting.FullPath}] tree node.");
                    var spTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                    var profileId = s_scheduleService.GetProfileId(spTreeNode);
                    spTreeNode = BuildTeamsTreeNode(spTreeNode, teamsTreeNode);

                    if (spTreeNode.DisposeScheduleInfo != null)
                    {
                        var scheduleInfo = ScheduleInfoes.Where(s => s.ProfileId == spTreeNode.DisposeScheduleInfo.ProfileId).FirstOrDefault();
                        if (scheduleInfo != null)
                        {
                            spTreeNode.DisposeScheduleInfo.ProfileId = BuildScheduleProfileId(spTreeNode.Parent.Id, teamsTreeNode.Id, spTreeNode.DisposeScheduleInfo.ProfileId);
                            spTreeNode.DisposeScheduleInfo.JobCategory = ScheduleType.TeamsDisposalSchedule;
                        }
                    }
                    setting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(spTreeNode);
                }
                catch(Exception e)
                {
                    s_logger.Error($"Build teams children node [{setting.FullPath}] tree node failed, error: {e}.");
                }
            }

            return settings;
        }

        private static RMSPTreeNode BuildTeamsTreeNode(RMSPTreeNode spTreeNode, RMSPTreeNode teamsTreeNode)
        {
            if (spTreeNode.Parent.Level != (int)NodeLevel.WebApplication)
            {
                BuildTeamsTreeNode(spTreeNode.Parent, teamsTreeNode);
            }
            else
            {
                spTreeNode.Parent = teamsTreeNode;
            }

            return spTreeNode;
        }

        private static async Task<RMSPTreeNode> GetFormTreeNode(RMSPTreeNode node)
        {
            var spTreeNode = RMDtoConverter.ConvertRMTree2SPTree(node);
            var simpleNode = RMDtoConverter.ConvertSPTree2RMSampleTree(spTreeNode);
            simpleNode.SourceType = (int)SourceFlag.Teams;
            var returnNode = await s_browseTreeService.BrowseSPOTreeAsync(simpleNode, RMBrowseTreeNodeSourceType.Teams, true);
            s_teamsSettingTreeService.TransChildrenNodeName(returnNode);
            await s_teamsSettingsService.LoadTeamsSettingIconAsync(returnNode.Children);
            returnNode.Children?.ForEach(n => n.Parent = null);
            var resulteSampleNode = RMDtoConverter.ConvertRMSampleTree2SPTree(returnNode);
            var resulteTreeNode = RMDtoConverter.ConvertSPTree2RMTree(resulteSampleNode);
            return resulteTreeNode;
        }

        private static string GetTeamsGroupUrl(string url)
        {
            if (url == "Default Office 365 Group Sites Group")
            {
                return I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
            }

            return url;
        }

        private static void ApplySetting()
        {
            try
            {
                if (!s_teamsSettingsService.ExistConfiguredSettings(JobType.ApplyTeamsSettings))
                {
                    s_logger.Debug(I18NEntity.GetString("RM_ApplySetting_NoSettings"));
                    return;
                }
                var needRunNodes = new List<RMSPTreeNode>();
                if (s_uniqueIdSettingService.ValidTeamsUniqueIdSetting() && s_teamsSettingsService.NeedRunUniqueIdJob(needRunNodes))
                {
                    s_logger.Debug("need run unique id job.");
                    var jobId = s_uniqueIdSettingService.RunUniqueIDSettingScheduleJob(
                        JobRunBy.Control,
                        JobType.TeamsUniqueIDSettingFullSchedule
                        );
                    s_logger.Debug("Run unique id job[{0}].", jobId);
                }
            }
            catch (Exception ex)
            {
                s_logger.Debug("Run unique id job error{0}.", ex.ToString());
            }
            s_teamsSettingsService.ApplySettings(JobRunBy.Control, false, RunApplySettingMethod.UpdatedScope);
        }
    }
}
