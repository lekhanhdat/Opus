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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Teams;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using Azure.Data.Tables;
using Azure;
using DocumentFormat.OpenXml.Wordprocessing;
using LiteDB;
using RATeams.Upgrade.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AngleSharp.Common;

namespace RATeams.Upgrade
{
    public class TeamsChannelSettingConflictsProcessor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(TeamsChannelSettingConflictsProcessor));

        private static readonly ISharePointSettingDao s_sharePointSettingDao = PlatformWindsorManager.GetService<ISharePointSettingDao>();

        private static readonly IRMArchiverSettingDao s_archiverSettingDao = PlatformWindsorManager.GetService<IRMArchiverSettingDao>();

        private static readonly ITeamsChannelConflictSettingDao s_teamsChannelConflictSettingDao = PlatformWindsorManager.GetService<ITeamsChannelConflictSettingDao>();

        private static readonly IEXOSettingRuleDao s_exoSettingRuleDao = PlatformWindsorManager.GetService<IEXOSettingRuleDao>();

        private static readonly IRMRemoteNodeDao s_remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static readonly ITenantService s_tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private static readonly IRecordOwnerDao s_recordOwnerDao = PlatformWindsorManager.GetService<IRecordOwnerDao>();

        private const string ChannelContianerId = "41cfe969-e07b-45cb-a7d0-b022f967e929";
        private const int NodeLevel_SiteCollection = (int)NodeLevel.SiteCollection;
        private const int NodeLevel_WebApplication = (int)NodeLevel.WebApplication;
        private const int NodeLevel_SkyDrivePro = (int)NodeLevel.SkyDrivePro;
        private const int NodeLevel_SkyDriveProGroup = (int)NodeLevel.SkyDriveProGroup;
        private const int NodeLevel_O365GroupSites = (int)NodeLevel.O365GroupSites;
        private const int NodeLevel_O365GroupSitesGroup = (int)NodeLevel.O365GroupSitesGroup;
        private const int NodeLevel_PrivateChannel = (int)NodeLevel.PrivateChannel;
        private const int NodeLevel_SharedChannel = (int)NodeLevel.SharedChannel;
        private const int NodeLevel_PrivateChannelSitesGroup = (int)NodeLevel.PrivateChannelGroup;

        private readonly List<string> _teamsGroupIds;

        private readonly List<RMRemoteNode> _teamsGroups;

        private static readonly TeamsUpgradeJobManager s_reportManager = new();


        public TeamsChannelSettingConflictsProcessor(string jobId)
        {
            var total = s_remoteNodeDao.GetChannnelNodeCount();
            s_reportManager.Init(jobId, JobType.TeamsChannelSettingConflictCheck, total);
            _teamsGroups = s_remoteNodeDao.GetAllTeamsContainers();
            _teamsGroupIds = _teamsGroups.Select(item => item.Id).ToList();
        }

        public async Task RunAsync()
        {

            if (await s_teamsChannelConflictSettingDao.DeleteAllTeamsChannelConflictSettings(TenantLocalValue.LogonGroupId))
            {
                s_logger.Info("Success to delete all channel conflict settings.Re-check.");
            }
            else
            {
                s_reportManager.HasFailedDetail = true;
                s_reportManager.SetJobFinished();
                return;
            }

            s_logger.Info("Begin to check channel setting conflicts");

            var taskList = new List<Task>();
            var taskForIL = Task.Run(ProcessILSettings);
            taskList.Add(taskForIL);

            var taskForSO = Task.Run(ProcessSOSettings);
            taskList.Add(taskForSO);

            Task.WaitAll([.. taskList]);

            s_reportManager.SetJobFinished();
        }

        public void ProcessILSettings()
        {
            var teamsNodeILSettings = s_sharePointSettingDao.GetAllSettingsBySiteGroupIds([.. _teamsGroupIds.Select(item => new Guid(item))]);
            var channelContainerSetting = teamsNodeILSettings.Where(s => s.ScopeId == new Guid(ChannelContianerId)).FirstOrDefault();
            if (channelContainerSetting == null)
            {
                s_logger.Info("Current channel container does't have IL the setting, no need to check.");
                return;
            }

            var teamsContainerILSettings = teamsNodeILSettings.Where(s => s.ScopeId == s.SiteGroupId && s.ScopeId != new Guid(ChannelContianerId)).Select(ConvertParentToChild<RMSharePointSetting, ILTeamsNodeSetting>);
            s_logger.Info($"Current channel container has IL the setting count is [{teamsContainerILSettings.Count()}].");
            foreach (var teamsGroup in _teamsGroups.Where(item => item.Id != ChannelContianerId))
            {
                var conflictChannelSettings = new List<TeamsChannelConflictSetting>();
                var teamsChannelNodesDic = s_remoteNodeDao.GetAllHasChannelTeamsNodes(teamsGroup.Id);
                s_logger.Info($"Current container [{teamsGroup.Url}] has channel teams count is [{teamsChannelNodesDic.Count()}].");
                foreach (var teamsChannelNodes in teamsChannelNodesDic)
                {
                    var teamsSite = teamsChannelNodes.Value.Where(item => item.NodeLevel == NodeLevel_O365GroupSites).FirstOrDefault();
                    if (teamsSite == null)
                    {
                        s_logger.Info($"Current container [{teamsGroup.Url}] can not find the team node.");
                        continue;
                    }

                    s_logger.Info($"Current container [{teamsGroup.Url}] team node is [{teamsSite.Name}].");
                    var teamsNodeILSetting = teamsContainerILSettings.Where(item => item.ScopeId == new Guid(teamsGroup.Id)).FirstOrDefault();
                    var teamsNodeSetting = teamsNodeILSettings.Where(item => item.ScopeId == new Guid(teamsSite.Id)).FirstOrDefault();
                    if (teamsNodeSetting != null)
                    {
                        s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] has setting.");
                        teamsNodeILSetting = ConvertParentToChild<RMSharePointSetting, ILTeamsNodeSetting>(teamsNodeSetting);
                    }

                    foreach (var channelNode in teamsChannelNodes.Value.Where(item => item.NodeLevel != NodeLevel_O365GroupSites))
                    {
                        s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node is [{channelNode.Url}].");
                        var channelNodeILSetting = ConvertParentToChild<RMSharePointSetting, ILTeamsNodeSetting>(channelContainerSetting);
                        var channelNodeSetting = teamsNodeILSettings.Where(item => item.ScopeId == new Guid(channelNode.Id)).FirstOrDefault();
                        if (channelNodeSetting != null)
                        {
                            s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node [{channelNode.Url}] has setting.");
                            channelNodeILSetting = ConvertParentToChild<RMSharePointSetting, ILTeamsNodeSetting>(channelNodeSetting);
                        }

                        var isConflict = CheckILSettingConflict(teamsNodeILSetting, channelNodeILSetting);
                        conflictChannelSettings.Add(new()
                        {
                            PartitionKey = teamsSite.Id.ToString(),
                            RowKey = Guid.NewGuid().ToString(),
                            Id = channelNodeILSetting.Id.ToString(),
                            ScopeId = channelNode?.Id,
                            FullPath = channelNode?.Url,
                            IsConflict = isConflict,
                            SettingString = channelNodeILSetting.ScopeId.ToString(),
                            ModuleType = AvePoint.RA.Contract.Teams.ModuleType.LifeCycle,
                        });;

                        var channelChildrenSettings = teamsNodeILSettings.Where(item => item.SiteId == new Guid(channelNode?.Id) && item.WebId != Guid.Empty);
                        s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node [{channelNode?.Url}] has children setting count is [{channelChildrenSettings.Count()}].");
                        foreach (var channelChildrenSetting in channelChildrenSettings)
                        {
                            var channelChildrenILSetting = ConvertParentToChild<RMSharePointSetting, ILTeamsNodeSetting>(channelChildrenSetting);
                            var childIsConflict = CheckILSettingConflict(teamsNodeILSetting, channelChildrenILSetting);
                            conflictChannelSettings.Add(new()
                            {
                                PartitionKey = teamsSite.Id.ToString(),
                                RowKey = Guid.NewGuid().ToString(),
                                Id = channelChildrenILSetting.Id.ToString(),
                                ScopeId = channelChildrenILSetting.ScopeId.ToString(),
                                FullPath = channelChildrenILSetting?.FullPath,
                                IsConflict = childIsConflict,
                                SettingString = channelChildrenILSetting.ScopeId.ToString(),
                                ModuleType = AvePoint.RA.Contract.Teams.ModuleType.LifeCycle,
                            });
                        }
                    }
                    s_reportManager.Increase(teamsChannelNodes.Value.Where(item => item.NodeLevel != NodeLevel_O365GroupSites).Count());
                }

                if (conflictChannelSettings.Count > 0)
                {
                    s_teamsChannelConflictSettingDao.AddTeamsChannelConflictSettings(TenantLocalValue.LogonGroupId, conflictChannelSettings);
                }
            }
        }

        private bool CheckILSettingConflict(ILTeamsNodeSetting teamsSetting, ILTeamsNodeSetting channelSetting)
        {
            var isConflict = teamsSetting == null || !teamsSetting.Equals(channelSetting);
            if (!isConflict && teamsSetting != null)
            {
                var recordOwners = s_recordOwnerDao.GetRecordOwner([teamsSetting.Id, channelSetting.Id], RecordOwnerSettingType.SharePoint, RecordOwnerSettingType.AISharePointOnline);
                if (recordOwners.Count != 0)
                {
                    var teamsOwnerIds = recordOwners.Where(r => r.SPSettingId == teamsSetting.Id).Select(r => r.ObjectId);
                    var channelOwnerIds = recordOwners.Where(r => r.SPSettingId == channelSetting.Id).Select(r => r.ObjectId);
                    if (teamsOwnerIds.Count() != channelOwnerIds.Count())
                    {
                        isConflict = true;
                    }
                    else
                    {
                        isConflict = teamsOwnerIds.Except(channelOwnerIds).Any();
                    }
                }
            }
            return isConflict;
        }

        public void ProcessSOSettings()
        {
            var teamsNodeSOSettings = s_archiverSettingDao.LoadArchiverSettingBySiteGroupIds([.. _teamsGroupIds.Select(item => new Guid(item))]);
            if (teamsNodeSOSettings.Count == 0)
            {
                s_logger.Warn("There is no teams and channel so settings, return");
                return;
            }
            var teamsNodeSORules = s_exoSettingRuleDao.GetAllTeamsNodeRuleMappings([.. teamsNodeSOSettings.Select(item => item.Id)]);
            var channelContainerSetting = teamsNodeSOSettings.Where(s => s.SPObjectId == new Guid(ChannelContianerId)).FirstOrDefault();
            if (channelContainerSetting == null)
            {
                s_logger.Info("Current channel container does't have the SO setting.");
                var channelSiteSettins = teamsNodeSOSettings.Where(s => s.SiteGroupId == new Guid(ChannelContianerId));
                if (!channelSiteSettins.Any())
                {
                    s_logger.Warn("There is no channel so settings, return");
                    return;
                }
            }

            s_logger.Info("Begin to check channel container so settings.");
            var teamsGroupSOSettings = teamsNodeSOSettings.GroupBy(item => item.SiteGroupId).Where(s => s.Key != new Guid(ChannelContianerId)).ToDictionary(item => item.Key, item => item.ToList());
            var teamsContainerSOSettings = teamsNodeSOSettings.Where(s => s.SPObjectId == s.SiteGroupId && s.SPObjectId != new Guid(ChannelContianerId)).Select(ConvertParentToChild<RMArchiverSetting, SOTeamsNodeSetting>);
            s_logger.Info($"Current channel container has SO the setting count is [{teamsContainerSOSettings.Count()}].");
            foreach (var teamsGroup in _teamsGroups.Where(item => item.Id != ChannelContianerId))
            {
                s_logger.Info($"Begin to check so settings under group [{teamsGroup.Url}].");
                var conflictChannelSettings = new List<TeamsChannelConflictSetting>();
                var teamsChannelNodesDic = s_remoteNodeDao.GetAllHasChannelTeamsNodes(teamsGroup.Id);
                s_logger.Info($"Current container [{teamsGroup.Url}] has channel teams count is [{teamsChannelNodesDic.Count()}].");
                foreach (var teamsChannelNodes in teamsChannelNodesDic)
                {
                    var teamsSite = teamsChannelNodes.Value.Where(item => item.NodeLevel == NodeLevel_O365GroupSites).FirstOrDefault();
                    if (teamsSite == null)
                    {
                        s_logger.Info($"Current container [{teamsGroup.Url}] can not find the team node.");
                        continue;
                    }

                    s_logger.Info($"Current container [{teamsGroup.Url}] team node is [{teamsSite.Name}].");
                    SOTeamsNodeSetting teamsNodeSOSetting = null;
                    var teamsLevelRules = new List<RMExchangeOnlineSettingRuleMapping>();
                    var teamsNodeSetting = teamsNodeSOSettings.Where(item => item.SPObjectId == new Guid(teamsSite.Id)).FirstOrDefault();
                    if (teamsNodeSetting == null)
                    {
                        s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] does't has setting.");
                        var teamGroupSetting = teamsNodeSOSettings.Where(item => item.SPObjectId == new Guid(teamsGroup.Id)).FirstOrDefault();
                        if (teamGroupSetting != null)
                        {
                            teamsNodeSOSetting = ConvertParentToChild<RMArchiverSetting, SOTeamsNodeSetting>(teamGroupSetting);
                            s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] does't has setting and container does't has setting.");
                        }
                    }
                    else
                    {
                        s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] has setting.");
                        teamsNodeSOSetting = ConvertParentToChild<RMArchiverSetting, SOTeamsNodeSetting>(teamsNodeSetting);
                    }

                    if(teamsNodeSOSetting != null)
                    {
                        teamsLevelRules = teamsNodeSORules.Where(t => t.ScopeId == teamsNodeSOSetting.Id).ToList();
                    }

                    foreach (var channelNode in teamsChannelNodes.Value.Where(item => item.NodeLevel != NodeLevel_O365GroupSites))
                    {
                        s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node is [{channelNode.Url}].");
                        SOTeamsNodeSetting channelNodeSOSetting = null;
                        var channelNodeSetting = teamsNodeSOSettings.Where(item => item.SPObjectId == new Guid(channelNode.Id)).FirstOrDefault();
                        if (channelNodeSetting == null)
                        {
                            s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node [{channelNode.Url}] does't has setting.");
                            if (channelContainerSetting != null)
                            {
                                s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node [{channelNode.Url}] use container setting.");
                                channelNodeSOSetting = ConvertParentToChild<RMArchiverSetting, SOTeamsNodeSetting>(channelContainerSetting);
                            }
                        }
                        else
                        {
                            s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node [{channelNode.Url}] has setting.");
                            channelNodeSOSetting = ConvertParentToChild<RMArchiverSetting, SOTeamsNodeSetting>(channelNodeSetting);
                        }

                        if(teamsNodeSOSetting == null)
                        {
                            s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}]  does't has setting.");
                            var result = ProcessChannelSOSettings(teamsNodeSORules, teamsNodeSOSettings, channelNodeSOSetting, teamsSite, channelNode);
                            conflictChannelSettings.AddRange(result);
                            continue;
                        }

                        var isEqual = true;
                        if (channelNodeSOSetting == null)
                        {
                            isEqual = false;
                        }
                        else
                        {
                            isEqual = teamsNodeSOSetting.Equals(channelNodeSOSetting);
                            var channelLevelRules = teamsNodeSORules.Where(t => t.ScopeId == channelNodeSOSetting.Id);
                            if (channelLevelRules.Any())
                            {
                                channelNodeSOSetting.Rules = channelLevelRules.ToList();
                            }
                            if (isEqual)
                            {
                                isEqual = CheckSOTeamsChannelEqual(teamsLevelRules, channelLevelRules);
                            }
                        }

                        conflictChannelSettings.Add(new()
                        {
                            PartitionKey = teamsSite.Id.ToString(),
                            RowKey = Guid.NewGuid().ToString(),
                            Id = channelNodeSOSetting?.Id.ToString() ?? string.Empty,
                            ScopeId = channelNode.Id,
                            FullPath = channelNode?.Url,
                            IsConflict = !isEqual,
                            SettingString = channelNodeSOSetting != null ? SerializerHelper.SerializeByDataContractSerializer(channelNodeSOSetting) : string.Empty,
                            ModuleType = AvePoint.RA.Contract.Teams.ModuleType.SO,
                        });

                        var channelChildrenSettings = teamsNodeSOSettings.Where(item => item.SiteId == new Guid(channelNode.Id) && item.SPObjectId != item.SiteId);
                        s_logger.Info($"Current container [{teamsGroup.Url}] team node [{teamsSite.Name}] channel node [{channelNode?.Url}] has children setting count is [{channelChildrenSettings.Count()}].");
                        foreach (var channelChildrenSetting in channelChildrenSettings)
                        {
                            var channelChildrenSOSetting = ConvertParentToChild<RMArchiverSetting, SOTeamsNodeSetting>(channelChildrenSetting);
                            var isChildrenEqual = teamsNodeSOSetting.Equals(channelChildrenSOSetting);
                            var channelChildrenRules = teamsNodeSORules.Where(t => t.ScopeId == channelChildrenSOSetting.Id);
                            if (channelChildrenRules.Any())
                            {
                                channelChildrenSOSetting.Rules = channelChildrenRules.ToList();
                            }
                            if (isChildrenEqual)
                            {
                                isChildrenEqual = CheckSOTeamsChannelEqual(teamsLevelRules, channelChildrenRules);
                            }

                            conflictChannelSettings.Add(new()
                            {
                                PartitionKey = teamsSite.Id.ToString(),
                                RowKey = Guid.NewGuid().ToString(),
                                Id = channelChildrenSOSetting.Id.ToString(),
                                ScopeId = channelChildrenSOSetting.SPObjectId.ToString(),
                                FullPath = channelChildrenSOSetting?.Url,
                                IsConflict = !isChildrenEqual,
                                SettingString = SerializerHelper.SerializeByDataContractSerializer(channelChildrenSOSetting),
                                ModuleType = AvePoint.RA.Contract.Teams.ModuleType.SO,
                            });
                        }
                    }
                    s_reportManager.Increase(teamsChannelNodes.Value.Where(item => item.NodeLevel != NodeLevel_O365GroupSites).Count());
                }

                if (conflictChannelSettings.Count > 0)
                {
                    s_teamsChannelConflictSettingDao.AddTeamsChannelConflictSettings(TenantLocalValue.LogonGroupId, conflictChannelSettings);
                }    
            }
        }

        private TChild ConvertParentToChild<TParent, TChild>(TParent parent) where TChild : class
        {
            if (EqualityComparer<TParent>.Default.Equals(parent, default(TParent)))
                return null;

            try
            {
                string parentJsonStr = SerializerHelper.SerializeByDataContractSerializer(parent);
                return SerializerHelper.DeserializeByDataContractSerializer<TChild>(parentJsonStr);
            }
            catch (Exception ex)
            {
                s_logger.Error(ex.Message);
                return null;
            }
        }

        private bool CheckSOTeamsChannelEqual(IEnumerable<RMExchangeOnlineSettingRuleMapping> teamsLevelRules, IEnumerable<RMExchangeOnlineSettingRuleMapping> channelLevelRules)
        {

            if (teamsLevelRules.Count() != channelLevelRules.Count())
            {
                return false;
            }

            var teamsRules = teamsLevelRules.ConvertAll(ConvertParentToChild<RMExchangeOnlineSettingRuleMapping, SOTeamsNodeRuleInfo>);
            var channelRules = channelLevelRules.ConvertAll(ConvertParentToChild<RMExchangeOnlineSettingRuleMapping, SOTeamsNodeRuleInfo>);
            
            return !teamsRules.Except(channelRules).Any();
        }

        private List<TeamsChannelConflictSetting> ProcessChannelSOSettings(IEnumerable<RMExchangeOnlineSettingRuleMapping> soRules, List<RMArchiverSetting> teamsNodeSOSettings, SOTeamsNodeSetting channelNodeSOSetting, RMRemoteNode teamsSite, RMRemoteNode channelNode)
        {
            var conflictChannelSettings = new List<TeamsChannelConflictSetting>();

            if (channelNodeSOSetting != null)
            {
                var channelLevelRules = soRules.Where(t => t.ScopeId == channelNodeSOSetting.Id);
                if (channelLevelRules.Any())
                {
                    channelNodeSOSetting.Rules = channelLevelRules.ToList();
                }

                conflictChannelSettings.Add(new()
                {
                    PartitionKey = teamsSite.Id.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Id = channelNodeSOSetting.Id.ToString(),
                    ScopeId = channelNode.Id,
                    FullPath = channelNode?.Url,
                    IsConflict = true,
                    SettingString = SerializerHelper.SerializeByDataContractSerializer(channelNodeSOSetting),
                    ModuleType = AvePoint.RA.Contract.Teams.ModuleType.SO,
                });
            }
            else
            {
                conflictChannelSettings.Add(new()
                {
                    PartitionKey = teamsSite.Id.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    ScopeId = channelNode.Id,
                    FullPath = channelNode?.Url,
                    IsConflict = false,
                    SettingString = string.Empty,
                    ModuleType = AvePoint.RA.Contract.Teams.ModuleType.SO,
                });
            }

            var channelChildrenSettings = teamsNodeSOSettings.Where(item => item.SiteId == new Guid(channelNode.Id) && item.SPObjectId != item.SiteId);
            foreach (var channelChildrenSetting in channelChildrenSettings)
            {
                var channelChildrenSOSetting = ConvertParentToChild<RMArchiverSetting, SOTeamsNodeSetting>(channelChildrenSetting);
                var channelChildrenRules = soRules.Where(t => t.ScopeId == channelChildrenSetting.Id);
                if (channelChildrenRules.Any())
                {
                    channelChildrenSOSetting.Rules = channelChildrenRules.ToList();
                }

                conflictChannelSettings.Add(new()
                {
                    PartitionKey = teamsSite.Id.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Id = channelChildrenSetting.Id.ToString(),
                    ScopeId = channelChildrenSetting.SPObjectId.ToString(),
                    FullPath = channelChildrenSetting?.Url,
                    IsConflict = true,
                    SettingString = SerializerHelper.SerializeByDataContractSerializer(channelChildrenSOSetting),
                    ModuleType = AvePoint.RA.Contract.Teams.ModuleType.SO,
                });
            }
            return conflictChannelSettings;
        }
    }
}
