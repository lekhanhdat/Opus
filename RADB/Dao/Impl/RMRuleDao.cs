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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.Contract.RMRuleManageMent;
using System.Linq.Expressions;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.I18N.Core;
using System.Data.Entity;
using AvePoint.RA.Contract.RMWeb.Rule;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using AvePoint.GCommon.Utility;
using DocAveOnline.WebApi.Contracts;
using AvePoint.RA.CommonUtil;
using System.Data.SqlClient;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMRuleDao : BaseDao<RMRule>, IRMRuleDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMRuleDao));
        public ITermRuleAssociationDao TermRuleAssociationDao { get; set; }
        public ITermDao TermDao { get; set; }
        #region Rule
        public void AddOrUpdateRMRule(RMRule rule, Guid? containerId = null)
        {
            using (var ctx = GetNewContext())
            {
                using (var tran = ctx.Database.BeginTransaction())
                {
                    var entities = ctx.RMRule.Where(r => r.RuleId == rule.RuleId).ToList();
                    if (entities.Count > 0)
                    {
                        foreach (var entity in entities)
                        {
                            entity.RuleName = rule.RuleName;
                            entity.DisposalAction = rule.DisposalAction;
                            entity.ExchangeDisposalAction = rule.ExchangeDisposalAction;
                            entity.PhysicalDisposalAction = rule.PhysicalDisposalAction;
                            entity.RuleLevel = rule.RuleLevel;
                            entity.Description = rule.Description;
                            entity.ModifyTime = rule.ModifyTime;
                            entity.DeleteRecords = rule.DeleteRecords;
                            entity.FSDisposalAction = rule.FSDisposalAction;
                            entity.SPLocalDisposalAction = rule.SPLocalDisposalAction;
                            entity.OneDriveDisposalAction = rule.OneDriveDisposalAction;
                            entity.AzureFileDisposalAction = rule.AzureFileDisposalAction;
                            entity.BoxDisposalAction = rule.BoxDisposalAction;
                            entity.ConnectorDisposalAction = rule.ConnectorDisposalAction;
                            entity.DisposalClass = rule.DisposalClass;
                            entity.Extension = rule.Extension;
                            entity.GoogleDriveDisposalAction = rule.GoogleDriveDisposalAction;
                            entity.TeamsDisposalAction = rule.TeamsDisposalAction;
                        }
                        BatchUpdate(ctx, entities);
                        var membershipEntities = ctx.RMRuleContainerMemberships.Where(r => r.RuleId == rule.RuleId).ToList();
                        foreach (var entity in membershipEntities)
                        {
                            entity.ContainerId = containerId.Value;
                        }
                        ctx.SaveChanges();
                    }
                    else
                    {
                        if (containerId == null)
                        {
                            containerId = RecordsConstants.RECORD_DEFAULT_CONTAINER_ID;
                        }
                        ctx.RMRule.Add(rule);
                        ctx.RMRuleContainerMemberships.Add(new RMRuleContainerMembership() { RuleId = rule.RuleId, ContainerId = containerId.Value, DAOMigrated = rule.DAOMigrated });
                        ctx.SaveChanges();
                    }
                    tran.Commit();
                }
            }
        }

        public RMRule GetRuleById(Guid ruleId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRule.Where(r => r.RuleId == ruleId).FirstOrDefault();
            }
        }

        public async Task<List<RMRule>> GetRuleByLevelAsync(PolicyLevel level)
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.RMRule.Where(r => r.RuleLevel == (int)level).ToListAsync();
            }
        }

        public void DeleteRule(List<Guid> ruleId)
        {
            using (var ctx = GetNewContext())
            {
                var entities = ctx.RMRule.Where(r => ruleId.Contains(r.RuleId)).ToList();
                foreach (var entity in entities)
                {
                    entity.IsRemoved = true;
                }
                BatchUpdate(entities);

                var mappingEntities = ctx.RMRuleContainerMemberships.Where(r => ruleId.Contains(r.RuleId)).ToList();
                ctx.RMRuleContainerMemberships.RemoveRange(mappingEntities);
                ctx.SaveChanges();
            }
        }

        public async Task<int> DeleteMigratedRulesAsync()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMRules WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<int> DeleteMigratedRuleContainerMembershipsAsync()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMRuleContainerMemberships WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public List<RMRule> GetSearchRules(List<RuleModel> ruleModels, string SearchValue, Guid? containerId = null)
        {
            if (containerId == null)
            {
                containerId = RecordsConstants.RECORD_DEFAULT_CONTAINER_ID;
            }
            using (var ctx = GetNewContext())
            {
                var ruleIdsInContainer = ctx.RMRuleContainerMemberships.Where(m => m.ContainerId.Equals(containerId.Value)).Select(m => m.RuleId).ToList();
                Expression<Func<RMRule, bool>> ruleModelPredicate = GetRuleModelPredicate(ruleModels);
                return ctx.RMRule.Where(ruleModelPredicate).Where(r => (r.RuleName.Contains(SearchValue) || r.Description.Contains(SearchValue) || r.DisposalClass.Contains(SearchValue))
                    && ruleIdsInContainer.Contains(r.RuleId) && !r.IsRemoved).ToList();
            }
        }

        private Expression<Func<RMRule, bool>> GetRuleModelPredicate(List<RuleModel> ruleModels)
        {
            Expression<Func<RMRule, bool>> ruleModelPredicate = null;
            if (ruleModels != null && ruleModels.Count > 0)
            {
                ruleModelPredicate = r => ruleModels.Contains((RuleModel)r.ModelType);
            }
            return ruleModelPredicate;
        }

        public List<RMRule> GetAllRules()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRule.ToList();
            }
        }

        public async Task<List<RMRule>> GetRulesWithoutRemovedAsync()
        {
            using(var ctx = GetNewContext())
            {
                return await ctx.RMRule.Where(item => !item.IsRemoved).ToListAsync();
            }
        }
        
        public async Task<List<RMRule>> GetGoogleRulesWithoutRemovedAsync()
        {
            using var ctx = GetNewContext();
            return await ctx.RMRule.Where(item => !item.IsRemoved && item.GoogleDriveDisposalAction != (int)RMContentDisposalAction.None).ToListAsync();
        }

        public List<RMRule> GetRulesByIds(List<Guid> ids)
        {
            using(var context = GetNewContext())
            {
                return context.RMRule.Where(item => ids.Contains(item.RuleId) && !item.IsRemoved).ToList();
            }
        }

        public List<RMRule> GetAvailableRules(List<Guid> containerIds = null)
        {

            using (var ctx = GetNewContext())
            {
                List<Guid> ruleIdsInContainer = null;
                if (containerIds == null)
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Select(m => m.RuleId).ToList();
                }
                else
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Where(m => containerIds.Contains(m.ContainerId)).Select(m => m.RuleId).ToList();
                }
                return ctx.RMRule.Where(r => ruleIdsInContainer.Contains(r.RuleId) && !r.IsRemoved).ToList();
            }
        }

        public async Task<List<RMRule>> GetAvailableRulesBySearch(RulePageRequestModel pageRequest)
        {
            var searchValue = pageRequest.SearchValue;
            var sortExp = BuildSortExpression(pageRequest.PageOrder);
            using var context = GetNewContext();
            var ruleIdsInContainer = await context.RMRuleContainerMemberships.Select(m => m.RuleId).ToListAsync();
            var query = context.RMRule
                .Where(r => ruleIdsInContainer.Contains(r.RuleId)
                && !r.IsRemoved
                && r.GoogleDriveDisposalAction != (int)RMContentDisposalAction.None
                && r.ModelType == 1);
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(r => 
                r.RuleName.Contains(searchValue) 
                || r.Description.Contains(searchValue) 
                || r.DisposalClass.Contains(searchValue));
            }

            return await sortExp(query)
                .Skip(pageRequest.PageIndex)
                .Take(pageRequest.PageSize)
                .ToListAsync();
        }


        public List<RMRule> GetAvailableFSRules(List<Guid> containerIds = null)
        {

            using (var ctx = GetNewContext())
            {
                List<Guid> ruleIdsInContainer = null;
                if (containerIds == null)
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Select(m => m.RuleId).ToList();
                }
                else
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Where(m => containerIds.Contains(m.ContainerId)).Select(m => m.RuleId).ToList();
                }
                return ctx.RMRule.Where(r => r.FSDisposalAction != (int)RMContentDisposalAction.None && ruleIdsInContainer.Contains(r.RuleId) && !r.IsRemoved).ToList();
            }
        }

        public List<RMRule> GetAvailableRules(List<RuleModel> ruleModels, List<Guid> containerIds = null)
        {
            using (var ctx = GetNewContext())
            {
                List<Guid> ruleIdsInContainer = null;
                if (containerIds == null)
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Select(m => m.RuleId).ToList();
                }
                else
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Where(m => containerIds.Contains(m.ContainerId)).Select(m => m.RuleId).ToList();
                }
                Expression<Func<RMRule, bool>> ruleModelPredicate = GetRuleModelPredicate(ruleModels);
                return ctx.RMRule.Where(ruleModelPredicate).Where(r => ruleIdsInContainer.Contains(r.RuleId) && !r.IsRemoved).ToList();
            }
        }

        public List<RMRule> GetRecordsAvailableRules(List<Guid> containerIds = null)
        {
            using (var ctx = GetNewContext())
            {
                List<Guid> ruleIdsInContainer = null;
                if (containerIds == null)
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Select(m => m.RuleId).ToList();
                }
                else
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Where(m => containerIds.Contains(m.ContainerId)).Select(m => m.RuleId).ToList();
                }
                return ctx.RMRule.Where(r =>(r.ModelType ==(int)AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.None || r.ModelType == (int)AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records)
                && ruleIdsInContainer.Contains(r.RuleId) && !r.IsRemoved).ToList();
            }
        }

        public List<RMRule> GetArchiverAvailableRules(List<Guid> containerIds = null)
        {
            using (var ctx = GetNewContext())
            {
                List<Guid> ruleIdsInContainer = null;
                if (containerIds == null)
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Select(m => m.RuleId).ToList();
                }
                else
                {
                    ruleIdsInContainer = ctx.RMRuleContainerMemberships.Where(m => containerIds.Contains(m.ContainerId)).Select(m => m.RuleId).ToList();
                }
                return ctx.RMRule.Where(r => r.ModelType == (int)AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver
                && ruleIdsInContainer.Contains(r.RuleId) && !r.IsRemoved).ToList();
            }
        }

        public bool IsExistRule(string name, Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRule.Any(r => r.RuleId != id && !r.IsRemoved && r.RuleName.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        public List<Guid> GetTeamsArchiverRuleIdsByLevels(List<GCommon.Contract.CommonFilter.PolicyLevel> levels)
        {
            using (var ctx = GetNewContext())
            {
                List<Guid> ruleIdsInContainer = ctx.RMRuleContainerMemberships.Select(m => m.RuleId).ToList();
                return ctx.RMRule
                    .Where(r => r.ModelType == (int)RuleModel.SOArchiver
                        && ruleIdsInContainer.Contains(r.RuleId)
                        && levels.Contains((GCommon.Contract.CommonFilter.PolicyLevel)r.RuleLevel)
                        && (r.DisposalAction != (int)RMContentDisposalAction.None || r.TeamsDisposalAction != (int)RMContentDisposalAction.None)
                        && !r.IsRemoved)
                    .Select(r => r.RuleId)
                    .ToList();
            }
        }

        public List<int> GetRuleIntIdsByRuleGuIds(List<Guid> ids)
        {
            using (var context = GetNewContext())
            {
                return context.RMRule.Where(item => ids.Contains(item.RuleId) && !item.IsRemoved).Select(r => r.Id).ToList();
            }
        }

        public async Task<IEnumerable<RMRule>> LoadRulesByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMRule.AsNoTracking().OrderByDescending(r => r.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertRuleTableAsync(IEnumerable<RMRule> rules)
        {
            using var context = GetNewContext();
            string tableName = "RMRules";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, RuleId, RuleName, RuleLevel, DisposalAction, DeleteRecords, IsRemoved, Description, ModifyTime, ExchangeDisposalAction, PhysicalDisposalAction, FSDisposalAction, SPLocalDisposalAction, OneDriveDisposalAction, AzureFileDisposalAction, BoxDisposalAction, ConnectorDisposalAction, GoogleDriveDisposalAction, TeamsDisposalAction, Extension, DisposalClass, ModelType, DAOMigrated) VALUES ");
                int i = 0;
                foreach (var item in rules)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8}, @p{paramIndex + 9}, @p{paramIndex + 10}, @p{paramIndex + 11}, @p{paramIndex + 12}, @p{paramIndex + 13}, @p{paramIndex + 14}, @p{paramIndex + 15}, @p{paramIndex + 16}, @p{paramIndex + 17}, @p{paramIndex + 18}, @p{paramIndex + 19}, @p{paramIndex + 20}, @p{paramIndex + 21}, @p{paramIndex + 22})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.RuleId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", (object)item.RuleName ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", item.RuleLevel));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", item.DisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", item.DeleteRecords));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", item.IsRemoved));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 7}", (object)item.Description ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 8}", item.ModifyTime));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 9}", item.ExchangeDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 10}", item.PhysicalDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 11}", item.FSDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 12}", item.SPLocalDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 13}", item.OneDriveDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 14}", item.AzureFileDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 15}", item.BoxDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 16}", item.ConnectorDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 17}", item.GoogleDriveDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 18}", item.TeamsDisposalAction));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 19}", (object)item.Extension ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 20}", (object)item.DisposalClass ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 21}", item.ModelType));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 22}", (object)item.DAOMigrated ?? DBNull.Value));
                    paramIndex += 23;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMRules data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllRuleAsync()
        {
            return await TruncateAllDataInTableAsync("RMRules");
        }

        #endregion

        #region Rule Container

        public async Task<IEnumerable<RMRuleContainer>> LoadRuleContainerByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMRuleContainers.AsNoTracking().OrderByDescending(c => c.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertRuleContainerTableAsync(IEnumerable<RMRuleContainer> ruleContainers)
        {
            using var context = GetNewContext();
            string tableName = "RMRuleContainers";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, ContainerId, Name, IsDefault, IsRemoved, ModifyTime) VALUES ");
                int i = 0;
                foreach (var item in ruleContainers)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.ContainerId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", (object)item.Name ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", item.IsDefault));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", item.IsRemoved));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", item.ModifyTime));
                    paramIndex += 6;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMRuleContainers data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllRuleContainerAsync()
        {
            return await TruncateAllDataInTableAsync("RMRuleContainers");
        }

        private readonly Expression<Func<RMRuleContainer, bool>> exceptRemoveLambda = c => !c.IsRemoved;
        public RMRuleContainer UpsertRuleContainer(RMRuleContainer ruleContainer)
        {
            using (var ctx = GetNewContext())
            {
                var entity = ctx.RMRuleContainers.Where(r => r.ContainerId == ruleContainer.ContainerId).FirstOrDefault();
                if (entity != null)
                {
                    entity.Name = ruleContainer.Name;
                    entity.ModifyTime = ruleContainer.ModifyTime;
                    entity.IsRemoved = ruleContainer.IsRemoved;
                    ctx.SaveChanges();
                    return entity;
                }
                else
                {
                    ctx.RMRuleContainers.Add(ruleContainer);
                    ctx.SaveChanges();
                    return ruleContainer;
                }
            }
        }

        public List<RMRuleContainer> GetRuleContainersByPager(RuleContainerQuery query, List<Guid> ruleContainers)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRuleContainers.Where(exceptRemoveLambda).Where(c => ruleContainers.Contains(c.ContainerId) && c.Name.Contains(query.SearchKey)).OrderByDescending(c => c.IsDefault).ThenBy(c => c.Name)
                    .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToList();
            }
        }

        public Dictionary<Guid, int> GetRuleContainersMapping(List<Guid> ruleContainerIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRuleContainerMemberships.Where(m => ruleContainerIds.Contains(m.ContainerId)).GroupBy(m => m.ContainerId).ToDictionary(g => g.Key, g => g.Count());
            }
        }

        public List<RMRuleContainer> GetAllRuleContainers(List<Guid> ruleContainers = null)
        {
            using (var ctx = GetNewContext())
            {
                if (ruleContainers == null)
                {
                    return ctx.RMRuleContainers.Where(exceptRemoveLambda).OrderByDescending(c => c.IsDefault).ThenBy(c => c.Name).ToList();
                }
                else
                {
                    return ctx.RMRuleContainers.Where(exceptRemoveLambda).Where(c => ruleContainers.Contains(c.ContainerId))
                        .OrderByDescending(c => c.IsDefault).ThenBy(c => c.Name).ToList();
                }
            }
        }

        public RMRuleContainer GetRuleContainersByRuleId(Guid ruleId)
        {
            using (var ctx = GetNewContext())
            {
                var allRuleContainerIds = ctx.RMRuleContainers.Where(exceptRemoveLambda).Select(r => r.ContainerId);
                var membership = ctx.RMRuleContainerMemberships.FirstOrDefault(r => allRuleContainerIds.Contains(r.ContainerId) && r.RuleId == ruleId);
                if (membership != null)
                {
                    return ctx.RMRuleContainers.FirstOrDefault(r => r.ContainerId == membership.ContainerId);
                }
                else
                {
                    return null;
                }
            }
        }

        public Dictionary<Guid, Guid> GetAllRulesContainerIDs()
        {
            using (var ctx = GetNewContext())
            {
                var ruleContainers = (
                    from c in ctx.RMRuleContainers
                    join m in ctx.RMRuleContainerMemberships
                    on c.ContainerId equals m.ContainerId
                    select new { m.RuleId, c.ContainerId }
                ).ToList();

                Dictionary<Guid, Guid> mappings = new Dictionary<Guid, Guid>();
                foreach (var item in ruleContainers)
                {
                    mappings[item.RuleId] = item.ContainerId;
                }

                return mappings;
            }
        }

        public List<RMRuleContainer> GetRuleContainersByRuleIds(IEnumerable<Guid> ruleIds)
        {
            using (var ctx = GetNewContext())
            {
                var allRuleContainerIds = ctx.RMRuleContainers.Where(exceptRemoveLambda).Select(r => r.ContainerId);
                var memberships = ctx.RMRuleContainerMemberships.Where(r => allRuleContainerIds.Contains(r.ContainerId) && ruleIds.Contains(r.RuleId)).ToList();
                if (memberships != null && memberships.Count > 0)
                {
                    var containerIds = memberships.Select(item => item.ContainerId).ToList();
                    return ctx.RMRuleContainers.Where(r => containerIds.Contains(r.ContainerId)).ToList();
                }
                return new List<RMRuleContainer>();
            }
        }

        public Dictionary<Guid, string> GetRuleContainerNameMemberships(List<Guid> ruleIds)
        {
            using (var ctx = GetNewContext())
            {
                var allRuleContainers = ctx.RMRuleContainers.Where(exceptRemoveLambda).ToList();
                var allRuleContainerIds = ctx.RMRuleContainers.Where(exceptRemoveLambda).Select(r => r.ContainerId).ToList();
                var memberships = ctx.RMRuleContainerMemberships.Where(r => ruleIds.Contains(r.RuleId) && allRuleContainerIds.Contains(r.ContainerId)).ToList();
                var containerIds = memberships.Select(m => m.ContainerId).ToList();
                var ruleContainers = allRuleContainers.Where(r => containerIds.Contains(r.ContainerId)).ToList();

                var resultDic = new Dictionary<Guid, string>();
                foreach (var ruleId in ruleIds)
                {
                    var membership = memberships.FirstOrDefault(m => m.RuleId == ruleId);
                    if (membership != null && membership.ContainerId != Guid.Empty)
                    {
                        var containerName = ruleContainers.FirstOrDefault(c => c.ContainerId == membership.ContainerId)?.Name;
                        if (containerName == "RM_RDM_DefaultRuleContainer")
                        {
                            resultDic[ruleId] = I18NEntity.GetString(containerName);
                        }
                        else
                        {
                            resultDic[ruleId] = containerName;
                        }
                    }
                }
                return resultDic;
            }
        }

        public int GetRuleContainersCount(string searchKey, List<Guid> ruleContainers)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRuleContainers.Where(exceptRemoveLambda).Where(c => ruleContainers.Contains(c.ContainerId) && c.Name.Contains(searchKey)).Count();
            }
        }

        public RMRuleContainer GetRuleContainersById(Guid guid)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMRuleContainers.Where(exceptRemoveLambda).Where(c => c.ContainerId == guid).FirstOrDefault();
            }
        }

        public bool CheckRuleContainerNameExist(string name)
        {
            using (var ctx = GetNewContext())
            {
                var loweredName = name.ToLower();
                return ctx.RMRuleContainers.Where(exceptRemoveLambda).Any(c => c.Name.ToLower() == loweredName);
            }
        }

        public bool DeleteRuleContainer(Guid containerId)
        {
            using (var ctx = GetNewContext())
            {
                var entity = ctx.RMRuleContainers.Where(exceptRemoveLambda).FirstOrDefault(c => c.ContainerId == containerId);
                if (entity != null)
                {
                    entity.IsRemoved = true;
                    return ctx.SaveChanges() > 0;
                }
                else
                {
                    return false;
                }
            }
        }

        public RAReturnMessage CheckContainerCrossSecurityGroup(Guid oldContainerId, Guid newContainerId, string ruleId)
        {
            using (var ctx = GetNewContext())
            {
                var message = new RAReturnMessage();
                var termIds = TermRuleAssociationDao.GetTermIdsByRuleId(ruleId);
                if (termIds == null || termIds.Count == 0)
                {
                    message.MessageType = RAMessageType.Successful;
                    return message;
                }

                var terms = ctx.Terms.AsQueryable().Where(tm => termIds.Contains(tm.Id) && tm.IsRemoved == false).ToList();
                foreach (var term in terms)
                {
                    term.FullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId);
                }
                message.Extsion1 = terms;

                var activeGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id).ToList();
                Expression<Func<RMSecurityGroupRuleMapping, bool>> predicate = m => activeGroupIds.Contains(m.SecurityGroupId) &&
                (m.Level == Contract.RMWeb.CP.SecurityRuleLevel.All || m.RuleObjId == newContainerId || m.RuleObjId == oldContainerId);
                var ruleContainerMappings = ctx.RMSecurityGroupRuleMapping.Where(predicate).ToList();
                var mappedAllContainers = ruleContainerMappings.FirstOrDefault(c => c.Level == Contract.RMWeb.CP.SecurityRuleLevel.All);
                var oldContainer = ruleContainerMappings.FirstOrDefault(c => c.RuleObjId == oldContainerId);
                var newContainer = ruleContainerMappings.FirstOrDefault(c => c.RuleObjId == newContainerId);
                if (mappedAllContainers != null)
                {
                    message.MessageType = RAMessageType.Successful;
                    return message;
                }
                else if (oldContainer == null && newContainer == null)
                {
                    message.MessageType = RAMessageType.Successful;
                    return message;
                }
                else if ((oldContainer == null && newContainer != null) || (oldContainer != null && newContainer == null))
                {
                    message.MessageType = RAMessageType.Failed;
                    return message;
                }
                else
                {
                    message.MessageType = oldContainer.SecurityGroupId == newContainer.SecurityGroupId ? RAMessageType.Successful : RAMessageType.Failed;
                    return message;
                }
            }
        }
        #endregion

        #region Rule container membership
        public async Task<IEnumerable<RMRuleContainerMembership>> LoadRuleContainerMembershipByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMRuleContainerMemberships.AsNoTracking().OrderByDescending(m => m.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoDeleteAllRuleContainerMembershipAsync()
        {
            return await TruncateAllDataInTableAsync("RMRuleContainerMemberships");
        }
        public async Task<long> MultiGeoInsertRuleContainerMembershipTableAsync(IEnumerable<RMRuleContainerMembership> ruleContainerMemberships)
        {
            using var context = GetNewContext();
            string tableName = "RMRuleContainerMemberships";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, ContainerId, RuleId, DAOMigrated) VALUES ");
                int i = 0;
                foreach (var item in ruleContainerMemberships)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.ContainerId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", item.RuleId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", (object)item.DAOMigrated ?? DBNull.Value));
                    paramIndex += 4;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMRuleContainerMemberships data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
        #endregion

        #region Google One
        private Func<IQueryable<RMRule>, IOrderedQueryable<RMRule>> BuildSortExpression(RulePageRequestOrder order)
        {
            if(order == null || string.IsNullOrWhiteSpace(order.OrderByKeyword))
                return query => query.OrderByDescending(n => n.ModifyTime);

            return query =>
            {
                return order.OrderByKeyword switch
                {
                    //Other columns ...
                    _ => order.OrderByDesc
                        ? query.OrderByDescending(n => n.RuleName)
                        : query.OrderBy(n => n.RuleName)
                };
            };
        }
        #endregion
    }
}
