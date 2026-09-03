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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Google;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class GoogleSettingDao : BaseDao<RMGoogleSetting>, IRMGoogleSettingDao
    {
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        
        public IScheduleService ScheduleService { get; set; }
        private RALogger logger = RALogger.GetInstance(typeof(TermDao));

        public List<RMGoogleSetting> GetAllSettings()
        {
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                return context.RMGoogleSettings.AsNoTracking().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }

        public List<RMGoogleSetting> GetDriveNodeLevelSettings()
        {
            using (var context = GetNewContext())
            {
                return context.RMGoogleSettings.AsNoTracking().Where(s => s.NodeInfo != null && s.ScopeId.Equals(s.DriveId) && !s.IsRemoved).ToList();
            }
        }

        public List<RMGoogleSetting> GetRunJobSetting()
        {
            using (var context = GetNewContext())
            {
                return context.RMGoogleSettings.AsQueryable().Where(s => s.SettingTime.Equals(0) && s.NodeInfo != null && s.IsActive).ToList();
            }
        }

        public RMGoogleSetting GetSettingInfoByScope(Guid containerId, Guid scopeId, Guid driveId)
        {
            using (var context = GetNewContext())
            {
                return context.RMGoogleSettings.FirstOrDefault(g => g.DriveId == driveId && g.ScopeId == scopeId && g.ContainerId == containerId && !g.IsRemoved);
            }
        }

        public async Task SetSettingJobTimeWithContainerIdAsync(Guid containerId, Guid scopeId)
        {
            using (var context = GetNewContext())
            {
                var setting = context.RMGoogleSettings.AsQueryable().FirstOrDefault(s => s.ContainerId == containerId && s.ScopeId.Equals(scopeId));
                if (setting != null)
                {
                    setting.SettingTime = DateTime.UtcNow.Ticks;
                    setting.NeedCheckDefaultValue = false;
                    setting.RunAutoFullJob = false;
                }
                await UpdateAsync(setting);
            }
        }

        public async Task<RMGoogleSetting> GetSettingInfo(Guid containerId, Guid driveId)
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.RMGoogleSettings
                                        .Where(x => x.ContainerId == containerId
                                                && x.DriveId == driveId).FirstOrDefaultAsync();
            }
        }

        public RMGoogleSetting GetParentNode(Expression<Func<RMGoogleSetting, bool>> whereLambda)
        {
            using var context = GetNewContext();
            return context.RMGoogleSettings.Where(whereLambda).FirstOrDefault(s => !s.IsRemoved);
        }

        public async Task AddOrUpdateCustomSettingAsync(RMGoogleTreeNode node, Guid driveId)
        {
            EnsureLabelName(node);
            using var context = GetNewContext();
            var containerId = Guid.Empty;
            
            if (node.Level == (int)NodeLevel.GoogleMyDriveContainer || node.Level == (int)NodeLevel.GoogleSharedDriveContainer)
            {
                containerId = new Guid(node.Id);
            }
            else
            {
                var drive = context.RMRemoteNodes.FirstOrDefault(nodeDb => nodeDb.Id == node.Id);
                containerId = drive != null ? new Guid(drive.ParentId) : Guid.Empty;
                driveId = new Guid(node.Id);
            }
            
            var driveSetting = context.RMGoogleSettings.FirstOrDefault(setting =>
                                   setting.ScopeId == new Guid(node.Id) &&
                                   !setting.IsRemoved);
            if (driveSetting != null)
            {
                driveSetting.CopyProperties(node, containerId, driveId);
                 ApplyCurrentValues(context, driveSetting);
                /*await RecordOwnerDao.UpdateRecordOwnersAsync(driveSetting.Id, node.RecordOwner,
                    RecordOwnerSettingType.GoogleDrive);*/

                if (node.AIReviewers != null)
                {
                    await RecordOwnerDao.UpdateRecordOwnersAsync(driveSetting.Id, node.AIReviewers, RecordOwnerSettingType.AIGoogleDrive);
                }
            }
            else
            {
                var settings = new RMGoogleSetting().CopyProperties(node, containerId, driveId);
                context.RMGoogleSettings.Add(settings);
                await context.SaveChangesAsync();
                await RemoveDeletedSettingAsync(context, settings);
                driveSetting = context.RMGoogleSettings.FirstOrDefault(s =>
                    s.ContainerId == containerId && s.ScopeId == settings.ScopeId && !s.IsRemoved);
                ArgumentNullException.ThrowIfNull(driveSetting);
                /*await RecordOwnerDao.AddRecordOwnersAsync(driveSetting.Id, node.RecordOwner,
                    RecordOwnerSettingType.GoogleDrive);*/

                if (node.AIReviewers != null)
                {
                    await RecordOwnerDao.UpdateRecordOwnersAsync(driveSetting.Id, node.AIReviewers, RecordOwnerSettingType.AIGoogleDrive);
                }
            }

            await SaveGoogleSettingMappingRule(node);
            if (node.Level is (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer )
            {
                if (node.IsNullClassificationSetting)
                {
                    await MarkAllSettingUnderContainerDeleted(containerId);
                }
                else
                {
                    await DeleteNullClassificationDriveSettingAsync(containerId);
                }
            }
        }

        private async Task MarkAllSettingUnderContainerDeleted(Guid containerId)
        {
            using var context = GetNewContext();
            var settings = await context.RMGoogleSettings.Where(s => s.ContainerId == containerId && s.ScopeId != containerId && !s.IsRemoved && s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable).ToListAsync();
            var needProcessSettings = settings.Where(s => !(s.DriveId == s.ScopeId && s.IsNullClassificationSetting)).ToList();
            if (needProcessSettings.IsNotNullOrEmpty())
            {
                needProcessSettings.ForEach(s =>
                {
                    s.IsRemoved = true;
                });
                BatchUpdate(context, needProcessSettings);
            }
        }

        private async Task DeleteNullClassificationDriveSettingAsync(Guid containerId)
        {
            using var context = GetNewContext();
            var driveSettings = await context.RMGoogleSettings.Where(s => s.ContainerId == containerId && s.ScopeId != containerId && s.IsNullClassificationSetting).ToListAsync();
            if (driveSettings.IsNotNullOrEmpty())
            {
                var enableRMNodes = driveSettings.Where(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable).ToList();
                if (enableRMNodes.IsNotNullOrEmpty())
                {
                    context.RMGoogleSettings.RemoveRange(enableRMNodes);
                }
                var disableRMNodes = driveSettings.Where(s => s.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable).ToList();
                if (disableRMNodes.IsNotNullOrEmpty())
                {
                    disableRMNodes.ForEach(s =>
                    {
                        s.IsNullClassificationSetting = false;
                    });
                    BatchUpdate(context, disableRMNodes);
                }
                context.SaveChanges();
            }
        }



        public RMGoogleSetting GetSettingInfoByAgentId(string id)
        {
            using var context = GetNewContext();
            RMGoogleSetting googleSetting = context.RMGoogleSettings.FirstOrDefault(s => s.ScopeId.Equals(new Guid(id)) && !s.IsRemoved);
            return googleSetting;
        }

        public async Task<List<RMGoogleSetting>> GetSettingsByExpression(Expression<Func<RMGoogleSetting, bool>> whereLambda)
        {
            using var context = GetNewContext();

            return await (from setting in context.RMGoogleSettings.Where(whereLambda)
                          join node in context.RMRemoteNodes
                          on setting.ScopeId.ToString() equals node.Id
                          where setting.ContainerId.ToString() == node.ParentId || node.ParentId == null
                          select setting)
                  .ToListAsync();
        }

        public async Task DeleteGoogleSettingAsync(Guid id)
        {
            using var context = GetNewContext();
            var parentSetting = context.RMRemoteNodes.FirstOrDefault(s => s.Id == id.ToString());
            var parentId = parentSetting != null && parentSetting.ParentId.IsNotNullOrEmpty() ? new Guid(parentSetting.ParentId) : Guid.Empty;
            var googleSetting = context.RMGoogleSettings.FirstOrDefault(setting => setting.ScopeId == id && setting.ContainerId == parentId && !setting.IsRemoved);

            if (googleSetting != null)
            {
                context.RMGoogleSettings.Remove(googleSetting);
                context.RMGoogleSettingRuleMapping.RemoveRange(context.RMGoogleSettingRuleMapping.Where(s => s.ScopeId == googleSetting.ScopeId));
                await context.SaveChangesAsync();
            }
            
        }

        public async Task CheckNeedRemoveDescendantsSetting(RMGoogleTreeNode settingNode, string nodeProfileIdPath)
        {
            if (settingNode.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteSchedules(ScheduleType.GoogleDisposalSchedule, nodeProfileIdPath);
                var deleteDescendantsSql =
                    "Delete From {0}.[RMGoogleSettings] Where {1} = @scopeId And ScopeId <> @scopeId";
                var IdLevel = "";
                switch ((NodeLevel)settingNode.Level)
                {
                    case NodeLevel.GoogleMyDriveContainer:
                    case NodeLevel.GoogleSharedDriveContainer:
                        IdLevel = "ContainerId";
                        break;
                    case NodeLevel.GoogleMyDrive:
                    case NodeLevel.GoogleSharedDrive:
                        IdLevel = "DriveId";
                        break;
                }

                int result = 0;
                using var context = RMDBContextManager.GetNewDBContext();
                var sql = string.Format(deleteDescendantsSql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), IdLevel);
                using (var tran = context.Database.BeginTransaction())
                {
                    result = await context.Database.ExecuteSqlCommandAsync(sql, new SqlParameter("@scopeId", settingNode.Id));
                    tran.Commit();
                }
            }
        }

        private async Task RemoveDeletedSettingAsync(RMDbContext context, RMGoogleSetting setting)
        {
            var deletedSetting = context.RMGoogleSettings.FirstOrDefault(s => s.ContainerId == setting.ContainerId && s.ScopeId == setting.ScopeId && s.IsRemoved);
            if (deletedSetting != null)
            {
                context.RMGoogleSettings.Remove(deletedSetting);
                await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == deletedSetting.Id);
                await context.SaveChangesAsync();
            }
        }
        
        private void EnsureLabelName(RMGoogleTreeNode node)
        {
            if (!string.IsNullOrEmpty(node.LabelName) && node.LabelName.Contains(":"))
            {
                node.LabelName = node.LabelName.Substring(node.LabelName.LastIndexOf(":") + 1);
            }
            if (!string.IsNullOrEmpty(node.DefaultLabelName) && node.DefaultLabelName.Contains(":"))
            {
                node.DefaultLabelName = node.DefaultLabelName.Substring(node.DefaultLabelName.LastIndexOf(":") + 1);
            }
        }

        public async Task UpdateLabelNameSettingAsync(RMDbContext ctx, string uniqueLabelId, string newNameLabel)
        {
            try
            {
                if (ctx != null)
                {
                    var searchPattern = $"<TermId>{uniqueLabelId}</TermId>";
                    var googleSettings = await ctx.RMGoogleSettings
                                 .Where(s => s.AutoClassificationRules.Contains(searchPattern) && !s.IsRemoved)
                                 .ToListAsync();

                    if (googleSettings.Any())
                    {
                        foreach (var setting in googleSettings)
                        {
                            setting.AutoClassificationRules = setting.UpdateLabelNameInRules(setting.AutoClassificationRules, uniqueLabelId, newNameLabel);
                            ApplyCurrentValues(ctx, setting);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Having trouble updating name for google setting with error : {ex.Message}");
            }
        }

        public List<string> GetUnSyncableNodeIdsByContainerId(Guid containerId)
        {
            using var ctx = GetNewContext();
            var query = from setting in ctx.RMGoogleSettings
                        where setting.ContainerId == containerId && setting.ScopeId != containerId && (!setting.IsSyncData || setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        select setting.ScopeId.ToString();

            return query.ToList();
        }

        public List<RMGoogleSetting> GetSettingInforDrive(Guid containerId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMGoogleSettings.Where(x => x.ContainerId == containerId && x.DriveId != Guid.Empty).ToList();;
            }
        }

        public async Task<List<RMSimpleRule>> GetGoogleDriveMappingRules(Guid driveId)
        {
            using var context = GetNewContext();
            List<RMGoogleSettingRuleMapping> settings = null;
            if (driveId != Guid.Empty)
            {
                settings = await context.RMGoogleSettingRuleMapping.Where(o => o.ScopeId == driveId && o.Type != (int)RuleType.Archiver).ToListAsync();
            }
            if (settings.IsNullOrEmpty())
            {
                settings = await context.RMGoogleSettingRuleMapping.Where(o => o.ScopeId == driveId && o.Type != (int)RuleType.Archiver).ToListAsync();
            }
            return settings.Select(o => new RMSimpleRule { RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = o.RuleOrder }).OrderBy(o => o.RuleOrder).ToList();
        }
        public async Task SaveGoogleSettingMappingRule(RMGoogleTreeNode node)
        {
            List<int> googleLevel = [(int)NodeLevel.GoogleMyDriveContainer, (int)NodeLevel.GoogleSharedDriveContainer, (int)NodeLevel.GoogleMyDrive, (int)NodeLevel.GoogleSharedDrive];
            var needSaveRules = googleLevel.Contains(node.Level) && node.IsNullClassificationSetting;
            if (needSaveRules)
            {
                using var context = GetNewContext();
                using var tran = context.Database.BeginTransaction();
                var scopeId = new Guid(node.Id);
                var existsRules = await context.RMGoogleSettingRuleMapping.Where(o => o.ScopeId == scopeId && o.Type != (int)RuleType.Archiver).ToListAsync();
                context.RMGoogleSettingRuleMapping.RemoveRange(existsRules);
                await context.SaveChangesAsync();
                var entityRules = node?.Rules?.Select(o => new RMGoogleSettingRuleMapping { ScopeId = scopeId, RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = (int)o.RuleOrder }).ToList();
                if (entityRules?.Count > 0)
                {
                    context.RMGoogleSettingRuleMapping.AddRange(entityRules);
                    await context.SaveChangesAsync();
                }
                tran.Commit();
            }

            if (!node.IsNullClassificationSetting)
            {
              
                if (googleLevel.Contains(node.Level))
                {
                    using var context = GetNewContext();
                    var containerId = new Guid(node.Id);
                    List<Guid> scopeIds = [ containerId ];
                    var driveSettingIds = await context.RMGoogleSettings.Where(s => s.ContainerId == containerId && s.DriveId == s.ScopeId && s.IsNullClassificationSetting).Select(s => s.ScopeId).ToListAsync();
                    if (driveSettingIds.IsNotNullOrEmpty())
                    {
                        scopeIds.AddRange(driveSettingIds);
                    }
                    var existsRules = context.RMGoogleSettingRuleMapping.Where(o => scopeIds.Contains(o.ScopeId)).ToList();
                    context.RMGoogleSettingRuleMapping.RemoveRange(existsRules);
                    await context.SaveChangesAsync();
                }
                else
                {
                    using var context = GetNewContext();
                    var scopeId = new Guid(node.Id);
                    var existsRules = await context.RMGoogleSettingRuleMapping.Where(o => o.ScopeId == scopeId).ToListAsync();
                    context.RMGoogleSettingRuleMapping.RemoveRange(existsRules);
                    await context.SaveChangesAsync();
                }
            }
        }


        #region Google one
        public async Task UpdateEnableRecordManagement(RMGoogleTreeNode node)
        {
            using (var ctx = GetNewContext())
            {
                var existing = await ctx.RMGoogleSettings
                    .Where(x => x.ContainerId == new Guid(node.ContainerId)
                    && x.ScopeId == new Guid(node.ContainerId)
                    && x.DriveId == Guid.Empty
                    && !x.IsRemoved).FirstOrDefaultAsync();
                bool needUpdate = node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable 
                    && existing?.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable;
                if (existing != null && needUpdate)
                {
                    existing.EnableRecordManagement = node.EnableRecordManagement;
                    existing.IsSyncData = node.IsSyncData;
                    ApplyCurrentValues(ctx, existing);
                }
            }
        }
        #endregion
    }
}