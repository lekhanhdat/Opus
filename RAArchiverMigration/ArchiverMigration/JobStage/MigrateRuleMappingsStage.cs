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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RuleManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using DocAveOnline.WebApi.Contracts;
using PnP.Framework;
using SPTreeNodeDto = AvePoint.RA.Contract.Object.RMSPTreeNode;
using NodeLevel = AvePoint.GCommon.Contract.Tree.Object.NodeLevel;
using SPType = AvePoint.GCommon.Contract.Tree.Object.SPType;
using NodeType = AvePoint.GCommon.Contract.Tree.Object.NodeType;
using RemoveNodeType = AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType;
using AvePoint.Common.RemoteNode.Impl;
using Microsoft.Graph;
using AvePoint.RA.Contract.Tenant;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Common.Util;
using AvePoint.Wrapper.Common;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateRuleMappingsStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Migrate ArchiverRules Mapping";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(9, 11);

        public override string JobDetailType => "RM_JS_ArchiverMigration_DataType_ArchiverRuleMapping";

        private List<ArchiverMigrationRuleSetting> archiverRuleSettings;

        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();


        protected async override Task PreExecuteAsync()
        {
            archiverRuleSettings = await GetAllRuleSettingsAsync();
        }

        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return Task.FromResult(archiverRuleSettings.Count);
        }

        protected override async Task InnerExecuteAsync()
        {
            logger.Info($"Start migrating rule settings.");

            var addedScheduleIDs = new HashSet<string>();
            foreach (var ruleSetting in archiverRuleSettings)
            {
                logger.Info($"Mgirate archiver setting : {ruleSetting.Url}");
                ConvertNodeName(ruleSetting);
                if (ruleSetting.Level == NodeLevel.RootFolder)
                {
                    // root folder apply rule is not support in opus. skip it
                    AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, ruleSetting.Url, "RM_ArchiverMigration_Comment_UnsupportRootFolderApplyRule");
                    JobProgressUpdater.Increase(1);
                    continue;
                }

                var detailStatus = Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful;
                string detailComment = null;

                ResetSPObjectId(ruleSetting);

                var ruleMappings = GetRuleMappings(ruleSetting, out var hasUnsupportedRule);
                bool disableArchiverManagement = false;
                if (ruleMappings.Count == 0)
                {
                    disableArchiverManagement = true;
                    logger.Warn($"No supported rules on the node, will disable archiver management on: {ruleSetting.Url}");
                    AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful, ruleSetting.Url, hasUnsupportedRule ? "RM_ArchiverMigration_Comment_NoSupportedRules" : null);
                    JobProgressUpdater.Increase(1);
                }

                try
                {
                    if (ruleSetting.Level == NodeLevel.List || ruleSetting.Level == NodeLevel.Folder)
                    {
                        var checkListNode = await JobExecutor.SPNodeService.GetListNodeWithParentsAsync(ruleSetting.SiteUrl, new Guid(ruleSetting.WebId), new Guid(ruleSetting.ListId));
                        if (SPCommonUtility.CheckIsDesignList(checkListNode.Title + checkListNode.TemplateId.ToString()) || checkListNode.Hidden)
                        {
                            logger.Info($"Skip Design List or Hidden or System Folder or not in BaseTemplate: {checkListNode.Name}");
                            AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, ruleSetting.Url, "RM_ArchiverMigration_Comment_UnsupportSystemList");
                            JobProgressUpdater.Increase(1);
                            continue;
                        }
                        if (ruleSetting.Level == NodeLevel.Folder && checkListNode.NodeType != (int)AveBaseType.DocumentLibrary)
                        {
                            logger.Info($"Skip folder in list,{ruleSetting.Url}");
                            AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, ruleSetting.Url, "RM_ArchiverMigration_Comment_UnsupportSystemList");
                            JobProgressUpdater.Increase(1);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"Check node error {ex}");
                }
                try
                {
                    if (ruleSetting.IsScan)
                    {
                        ruleSetting.Schedule = null;
                        logger.Info($"Because this schedule is for scan job, skip it, setting url: {ruleSetting.Url}");
                    }
                    await AssemblyArchiverScheduleAsync(ruleSetting, ruleMappings);
                    if (ruleSetting.Schedule != null && addedScheduleIDs.Contains(ruleSetting.Schedule.Id.ToLower()))
                    {
                        logger.Info($"This repeat schedule maybe from rules profile.");
                        ruleSetting.Schedule.Id = Guid.NewGuid().ToString();
                    }
                }
                catch (Exception ex)
                {
                    ruleSetting.Schedule = null;
                    detailStatus = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                    detailComment = "RM_ArchiverMigration_Comment_ConvertRuleScheduleFailed";
                    logger.Error($"Migrate rule schedule failed, Node: {ruleSetting.Url}. {ex}");
                }
                if (!disableArchiverManagement && ruleSetting.Schedule != null)
                {
                    logger.Info($"Schedule:{ruleSetting.Schedule.Id}, StartTime:{new DateTime(ruleSetting.Schedule.StartTime)}, EndTime:{new DateTime(ruleSetting.Schedule.EndTime)} {ruleSetting.Schedule.TimeZoneId}");
                    if (ruleSetting.Schedule.EndTime > 0)
                    {
                        ruleSetting.Schedule.EndTime = DateTimeUtil.ConvertTimeToUtcDate(new DateTime(ruleSetting.Schedule.EndTime, DateTimeKind.Unspecified), ruleSetting.Schedule.TimeZoneId, true).Ticks;
                        logger.Info($"Convert EndTime:{new DateTime(ruleSetting.Schedule.EndTime)} {ruleSetting.Schedule.TimeZoneId}");
                    }
                }

                if(disableArchiverManagement)
                {
                    await ArchiverSettingDao.SaveMigratedDisabledArchiverSettingAsync(ruleSetting);
                }
                else
                {
                    await ArchiverSettingDao.SaveMigratedArchiverSettingAsync(ruleSetting, ruleMappings);
                }
                if (!disableArchiverManagement && ruleSetting.Schedule != null)
                {
                    addedScheduleIDs.Add(ruleSetting.Schedule.Id.ToLower());
                }

                JobProgressUpdater.Increase(1);
                AddJobDetail(detailStatus, ruleSetting.Url, detailComment);
            }

            logger.Info($"Finish migrate rule settings.");
        }

        private static void ConvertNodeName(ArchiverMigrationRuleSetting ruleSetting)
        {
            if (ruleSetting.Url == "Default Office 365 Group Sites Group")
            {
                ruleSetting.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
            }
            if (ruleSetting.Url == "Default_ SharePoint Sites_ Group")
            {
                ruleSetting.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
            }
            if (ruleSetting.Url == "Default OneDrive for Business Group")
            {
                ruleSetting.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultOneDriveforBusinessGroup");
            }
            if (ruleSetting.Url == "Default Private Channel Sites Container")
            {
                ruleSetting.Url = I18N.Core.I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
            }
        }

        private List<RMExchangeOnlineSettingRuleMapping> GetRuleMappings(ArchiverMigrationRuleSetting ruleSetting, out bool hasUnsupportedRule)
        {
            hasUnsupportedRule = false;
            var sourceFlag = JobExecutor.SPNodeService.GetSourceFlagBySiteGroupId(ruleSetting.SiteGroupId.ToString());
            // key: rule id,  value: (string, string) => (rule order string, rule name)
            var ruleOrderInfoes = new Dictionary<Guid, (string, string)>();
            for (int i = 0; i < ruleSetting.RuleIdList.Count; i++)
            {
                var rid = new Guid(ruleSetting.RuleIdList[i]);
                if (!JobExecutor.RuleIdAndRuleInfoMappings.TryGetValue(rid, out (int ruleLevel, string ruleName) ruleInfo))
                {
                    logger.Error($"Can't find the rule level. the rule not exists. rule id: {rid}");
                    continue;
                }

                if(sourceFlag == Contract.Explorer.SourceFlag.OneDrive && ruleInfo.ruleLevel <= (int)PolicyLevel.Site)
                {
                    hasUnsupportedRule = true;
                    logger.Warn($"Don't support site and sitecollection level rule on onedrive. {rid} - {ruleInfo.ruleLevel.ToString()}");
                    continue;
                }
                
                ruleOrderInfoes[rid] = ($"{ruleInfo.ruleLevel.ToString().PadLeft(10, '0')}{i.ToString().PadLeft(5, '0')}", ruleInfo.ruleName);
            }

            var finalMappings = new List<RMExchangeOnlineSettingRuleMapping>();
            var orderedList = ruleOrderInfoes.OrderBy(i => i.Value.Item1).ToList();
            for (int i = 0; i < orderedList.Count(); i++)
            {
                var orderedItem = orderedList[i];

                finalMappings.Add(new RMExchangeOnlineSettingRuleMapping()
                {
                    DAOMigrated = true,
                    ScopeId = new Guid(ruleSetting.Id),
                    RuleId = orderedItem.Key,
                    RuleName = orderedItem.Value.Item2,
                    RuleOrder = i + 1,
                    Type = (int)DB.Dao.Impl.RuleType.Archiver
                });
            }

            return finalMappings;
        }

        private void ResetSPObjectId(ArchiverMigrationRuleSetting ruleSetting)
        {
            var isGroupNode = IsSiteGroupNode(ruleSetting.Level);
            if (isGroupNode)
            {
                ruleSetting.SiteGroupId = JobExecutor.SPNodeService.GetTargetSiteGroupId(ruleSetting.SiteGroupName, (int)ruleSetting.Level);
                ruleSetting.SiteId = Guid.Empty;
                ruleSetting.NodeId = ruleSetting.SiteGroupId.ToString();
            }
            else
            {
                ruleSetting.SiteGroupId = JobExecutor.SPNodeService.GetGroupNodeId4Site(ruleSetting.SiteUrl);
                ruleSetting.SiteId = JobExecutor.SPNodeService.GetSiteNodeId(ruleSetting.SiteUrl);

                if (ruleSetting.Level == NodeLevel.SiteCollection)
                {
                    ruleSetting.NodeId = ruleSetting.SiteId.ToString();
                }
            }
        }

        private bool IsSiteGroupNode(NodeLevel nodeLevel)
        {
            return nodeLevel == NodeLevel.WebApplication || nodeLevel == NodeLevel.O365GroupSitesGroup
                || nodeLevel == NodeLevel.PrivateChannelGroup || nodeLevel == NodeLevel.SkyDriveProGroup;
        }

        private async Task AssemblyArchiverScheduleAsync(ArchiverMigrationRuleSetting ruleSetting, List<RMExchangeOnlineSettingRuleMapping> ruleMapping)
        {
            if(ruleSetting.Schedule == null)
            {
                return;
            }

            SPTreeNodeDto treeNode = null;
            if(IsSiteGroupNode(ruleSetting.Level))
            {
                treeNode = JobExecutor.SPNodeService.GetSiteGroupNodeWithParents(ruleSetting.SiteGroupId.ToString());
            }
            else if(ruleSetting.Level == NodeLevel.SiteCollection)
            {
                treeNode = JobExecutor.SPNodeService.GetSiteCollectionNodeWithParents(ruleSetting.SiteUrl);
            }
            else if (ruleSetting.Level == NodeLevel.Site)
            {
                treeNode = await JobExecutor.SPNodeService.GetSiteNodeWithParentsAsync(ruleSetting.SiteUrl, new Guid(ruleSetting.WebId));
            }
            else if (ruleSetting.Level == NodeLevel.List)
            {
                treeNode = await JobExecutor.SPNodeService.GetListNodeWithParentsAsync(ruleSetting.SiteUrl, new Guid(ruleSetting.WebId), new Guid(ruleSetting.ListId));
            }
            else if (ruleSetting.Level == NodeLevel.Folder)
            {
                treeNode = await JobExecutor.SPNodeService.GetFolderNodeWithParentsAsync(ruleSetting.SiteUrl, new Guid(ruleSetting.WebId), new Guid(ruleSetting.ListId), ruleSetting.Url);
            }

            if(treeNode != null)
            {
                treeNode.IconStatus = IconStatus.Break;
                if (treeNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                {
                    treeNode.IsCustomSetting = true;
                }
                treeNode.EnableArchiverManagement = ruleSetting.EnableArchiverManagement;
                treeNode.Rules = ruleMapping.Select(m => new Contract.RMRuleManageMent.RMSimpleRule()
                {
                    RuleId = m.RuleId,
                    RuleName = m.RuleName,
                    IntRuleLevel = JobExecutor.RuleIdAndRuleInfoMappings.TryGetValue(m.RuleId, out (int ruleLevel, string ruleName) ruleInfo) ? ruleInfo.ruleLevel : 0,
                    RuleOrder = m.RuleOrder,
                }).ToList();
                treeNode.IsEnableSuperUserDecrypt = ruleSetting.IsEnableSuperUserDecrypt;
                treeNode.IsEnableRemoveRetentionLabel = ruleSetting.IsEnableRemoveRetentionLabel;
                treeNode.IsManagedMetadataService = ruleSetting.IsIncludeManagedMetadataService;
                treeNode.IsWorkflowDefinition = ruleSetting.IsIncludeWorkflowDefinition;
                treeNode.EnableArchiverManagement = ruleSetting.EnableArchiverManagement;

                ruleSetting.Schedule.Extentions = JsonConvert.SerializeObject(treeNode);
                ruleSetting.Schedule.ProfileId = ScheduleService.GetProfileId(treeNode);
            }
            else
            {
                throw new Exception($"Schedule related tree node chould be built. {ruleSetting.Schedule.Id}");
            }
        }


        private async Task<List<ArchiverMigrationRuleSetting>> GetAllRuleSettingsAsync()
        {
            return await GetArchiverMigrationDataAsync<List<ArchiverMigrationRuleSetting>>((service) =>
            {
                return service.GetAllArchiverRuleSettings();
            });
        }


    }
}
