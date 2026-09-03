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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMWorkflowDefinitionDao : BaseDao<RMWorkflowDefinition>, IRMWorkflowDefinitionDao
    {

        private static readonly IRMWorkflowStepDao WorkflowStepDao = new RMWorkflowStepDao();
        private RALogger Logger = RALogger.GetInstance(typeof(RMWorkflowDefinitionDao));
        public void DeleteWorkflow(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                using (var tran = ctx.Database.BeginTransaction())
                {
                    var workflowReferenceId = ctx.WorkflowDefinition.Where(t => t.Id == id).Select(o => o.ReferenceId).FirstOrDefault();
                    var multipleVersionWorkflows = ctx.WorkflowDefinition.Where(t => t.ReferenceId == workflowReferenceId).ToList();
                    foreach (var workflow in multipleVersionWorkflows)
                    {
                        var steps = ctx.WorkflowStep.Where(t => t.DefinitionId == workflow.Id).ToList();
                        foreach (var step in steps)
                        {
                            //删除workflow节点设置的setting(eg:reviewer信息)
                            var configs = ctx.WorkflowStepConfiguration.Where(t => t.StepId == step.Id).ToList();
                            if (configs.Count > 0)
                            {
                                ctx.WorkflowStepConfiguration.RemoveRange(configs);
                            }
                        }
                        //删除workflow节点信息
                        ctx.WorkflowStep.RemoveRange(steps);
                        //最后删除整个workflow
                        ctx.WorkflowDefinition.Remove(workflow);
                        ctx.SaveChanges();
                    }
                    tran.Commit();
                }
            }
        }

        public async Task<RMWorkflowDefinition> LoadAsync(Guid id)
        {
            using var context = GetNewContext();
            var res = await context.WorkflowDefinition.FirstOrDefaultAsync(item => item.Id == id);
            return res;
        }

        public RMWorkflowDefinition LoadWorkflow(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.WorkflowDefinition.First(t => t.Id == id);
            }
        }

        /// <summary>
        /// get the max verison workflow 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RMWorkflowDefinition GetWorkflowByReferenceId(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.WorkflowDefinition.Where(w => w.ReferenceId == id).OrderByDescending(x => x.Version).FirstOrDefault();
            }
        }

        public RMWorkflowDefinition LoadWorkflow(string name)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.WorkflowDefinition.Where(t => t.Name == name).FirstOrDefault();
            }
        }

        public List<RMWorkflowDefinition> QueryWorkflows(ProcessQueryDto queryDto, out int totalCount)
        {
            using (var ctx = GetNewContext())
            {
                var pageIndex = queryDto.PageIndex;
                var pageSize = queryDto.PageSize;
                var searchValue = queryDto.SearchValue;
                var templateIds = new List<Guid>();

                Expression<Func<RMWorkflowDefinition, bool>> predicate = null;
                if (!string.IsNullOrEmpty(searchValue))
                {
                    predicate = s => s.Name.Contains(searchValue);
                }
                else
                {
                    predicate = s => !string.IsNullOrEmpty(s.Name);
                }

                //totalCount = ctx.WorkflowDefinition.Where(predicate).Count();
                //return ctx.WorkflowDefinition.Where(predicate).OrderBy(s => s.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                totalCount = ctx.WorkflowDefinition.Where(predicate).GroupBy(w => w.ReferenceId).ToDictionary(z => z.Key, g => g.OrderByDescending(x => x.Version).FirstOrDefault()).Count();
                return ctx.WorkflowDefinition.Where(predicate)
                    .GroupBy(w => w.ReferenceId)
                    .ToDictionary(z => z.Key, g => g.OrderByDescending(x => x.Version).FirstOrDefault())
                    .Select(o => o.Value)
                    .OrderByDescending(s => s.LastUpdatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public async Task SaveWorkflowAsync(WorkflowDefinitionDto dto)
        {
            using (var ctx = GetNewContext())
            {
                using (var tran = ctx.Database.BeginTransaction())
                {
                    var isNew = true;
                    var id = dto.Id;
                    var uiWorkflowNodes = dto.Content.WorkflowNodes;
                    var needUpdateVersion = dto.UpgradeVersion;

                    if (id != Guid.Empty)
                    {
                        isNew = false;
                        var oldWf = await ctx.WorkflowDefinition.Where(t => t.Id == id).FirstAsync();
                        ArgumentNullException.ThrowIfNull(oldWf);
                        if (needUpdateVersion)
                        {
                            var newVersion = GenerateWFVersion(oldWf?.Version);
                            var wf = new RMWorkflowDefinition();
                            UpdateRMWorkflowDefinitionAttr(dto, wf);
                            wf.Id = dto.UpgradedVersionId == Guid.Empty ? Guid.NewGuid() : dto.UpgradedVersionId;
                            wf.Version = newVersion;
                            wf.ReferenceId = oldWf.ReferenceId;
                            wf.CreationTime = DateTime.UtcNow;
                            wf.LastUpdatedTime = DateTime.UtcNow;
                            ctx.WorkflowDefinition.Add(wf);
                            ctx.SaveChanges();
                            id = wf.Id;
                        }
                        else
                        {
                            UpdateRMWorkflowDefinitionAttr(dto, oldWf);
                            oldWf.LastUpdatedTime = DateTime.UtcNow;
                            await this.UpdateAsync(oldWf);
                        }
                    }
                    else
                    {
                        var wf = new RMWorkflowDefinition();
                        UpdateRMWorkflowDefinitionAttr(dto, wf);
                        wf.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
                        wf.Version = GenerateWFVersion(); //默认版本号1.0.0.0
                        wf.CreationTime = DateTime.UtcNow;
                        wf.LastUpdatedTime = DateTime.UtcNow;
                        wf.ReferenceId = dto.ReferenceId == Guid.Empty ? Guid.NewGuid() : dto.ReferenceId;
                        ctx.WorkflowDefinition.Add(wf);
                        ctx.SaveChanges();
                        id = wf.Id;
                    }

                    var dbSteps = new List<Model.RMWorkflowStep>();
                    if (isNew || needUpdateVersion)
                    {
                        foreach (var node in uiWorkflowNodes)
                        {
                            AddWorkflowNodeAndSettings(node, id);
                        }
                    }
                    else
                    {
                        var dbWfNodes = await ctx.WorkflowStep.Where(t => t.DefinitionId == id).ToListAsync();
                        var uiWorkflowNodeIds = uiWorkflowNodes.Select(t => t.Id).ToHashSet();
                        foreach (var dbNode in dbWfNodes)
                        {
                            if (!uiWorkflowNodeIds.Contains(dbNode.Id))
                            {
                                RemoveWorkflowNodeSettings(dbNode);
                                ctx.WorkflowStep.Remove(dbNode);
                                ctx.SaveChanges();
                            }
                        }

                        foreach (var node in uiWorkflowNodes)
                        {
                            //保存ui中的新增node以及修改的node相关信息
                            var dbNode = await ctx.WorkflowStep.Where(t => t.Id == node.Id).FirstOrDefaultAsync();
                            if (dbNode != null)
                            {
                                UpdateWorkflowNodeAndSettings(dbNode, node);
                            }
                            else
                            {
                                AddWorkflowNodeAndSettings(node, id);
                            }
                        }
                    }
                    tran.Commit();
                }
            }
        }

        public async Task UpsertReplicaWorkflowAsync(WorkflowDefinitionDto dto)
        {
            using (var ctx = GetNewContext())
            {
                using (var tran = ctx.Database.BeginTransaction())
                {
                    var isNewWorkflow = false;
                    var workflow = await ctx.WorkflowDefinition.FirstOrDefaultAsync(item => item.Id == dto.Id);
                    if (workflow == null)
                    {
                        isNewWorkflow = true;
                        workflow = new RMWorkflowDefinition
                        {
                            Id = dto.Id,
                        };
                    }

                    UpdateReplicaWorkflowDefinition(dto, workflow);
                    if (isNewWorkflow)
                    {
                        await InsertWorkflowDefinitionAsync(ctx, workflow);
                    }
                    else
                    {
                        await ctx.SaveChangesAsync();
                    }

                    var steps = await ctx.WorkflowStep.Where(item => item.DefinitionId == dto.Id).ToListAsync();
                    if (steps.Count > 0)
                    {
                        var stepIds = steps.Select(item => item.Id).ToList();
                        var settings = await ctx.WorkflowStepConfiguration.Where(item => stepIds.Contains(item.StepId)).ToListAsync();
                        if (settings.Count > 0)
                        {
                            ctx.WorkflowStepConfiguration.RemoveRange(settings);
                        }

                        ctx.WorkflowStep.RemoveRange(steps);
                        await ctx.SaveChangesAsync();
                    }

                    foreach (var node in dto.Content?.WorkflowNodes ?? new List<Contract.RMWeb.CP.RMWorkflowStepNode>())
                    {
                        AddReplicaWorkflowNode(ctx, dto.Id, node);
                    }

                    await ctx.SaveChangesAsync();
                    tran.Commit();
                }
            }
        }

        public bool NeedUpgradeVersion(WorkflowDefinitionDto dto)
        {
            using (var ctx = GetNewContext())
            {
                var oldWf = ctx.WorkflowDefinition.Where(t => t.Id == dto.Id).FirstOrDefault();
                var hasRunningInstance = ctx.WorkflowInstance.Where(d => d.DefinitionId == dto.Id && d.Status == RMWorkflowStatus.Running).FirstOrDefault();
                return hasRunningInstance != null;
            }
        }

        public string GenerateWFVersion(string currentVersion = "")
        {
            var version = 1000;
            if (!string.IsNullOrEmpty(currentVersion))
            {
                var aa = currentVersion.Replace(".", "");
                var newVersion = Convert.ToInt32(aa);
                ++newVersion;
                version = newVersion;
            }
            return string.Join(".", version.ToString().ToArray());
        }

        public void UpdateRMWorkflowDefinitionAttr(WorkflowDefinitionDto dto, RMWorkflowDefinition dbo)
        {
            var dbItem = new RMWorkflowDefinition();
            dbo.Name = dto.Name;
            dbo.Description = dto.Description;
            dbo.Type = dto.Type;
            dbo.ContentStr = dto.ContentStr;
            dbo.XamlStr = dto.XamlStr;
            dbo.HashCode = dto.HashCode;
            dbo.CreatedBy = dto.CreatedBy;
            dbo.Level = dto.LevelCount;
        }

        public void AddWorkflowNodeAndSettings(Contract.RMWeb.CP.RMWorkflowStepNode node, Guid workflowId)
        {
            using var ctx = GetNewContext();
            //保存workflow node节点信息
            var wfNode = new Model.RMWorkflowStep
            {
                Id = node.Id,
                DefinitionId = workflowId,
                DisplayName = node.DisplayName,
                Name = node.Name,
                ReviewerType = node.ReviewerType,
                UsedEmailTemplateMode = node.UsedEmailTemplateMode,
                UsedEmailTemplateId = node.UsedEmailTemplateId,
                CustomIntervalSetting = JsonConvert.SerializeObject(node.CustomIntervalSetting),
            };
            ctx.WorkflowStep.Add(wfNode);
            ctx.SaveChanges();
            //保存workflow node setting信息
            if (node.Reviewers != null && node.Reviewers.Count > 0)
            {
                foreach (var reviewer in node.Reviewers)
                {
                    var wfNodeSetting = new RMWorkflowStepConfiguration
                    {
                        StepId = wfNode.Id,
                        OwnerType = reviewer.InviteType,
                        OwnerId = reviewer.UserId
                    };
                    ctx.WorkflowStepConfiguration.Add(wfNodeSetting);
                }
                ctx.SaveChanges();
            }
        }

        public void UpdateWorkflowNodeAndSettings(Model.RMWorkflowStep node, Contract.RMWeb.CP.RMWorkflowStepNode nodeDto)
        {
            using var ctx = GetNewContext();
            node.Name = nodeDto.Name;
            node.DisplayName = nodeDto.DisplayName;
            node.ReviewerType = nodeDto.ReviewerType;
            node.UsedEmailTemplateMode = nodeDto.UsedEmailTemplateMode;
            node.UsedEmailTemplateId = nodeDto.UsedEmailTemplateId;
            node.CustomIntervalSetting = JsonConvert.SerializeObject(nodeDto.CustomIntervalSetting);
            WorkflowStepDao.UpdateStep(node);

            var dbNodeSettings = ctx.WorkflowStepConfiguration.Where(t => t.StepId == node.Id).ToList();
            if (nodeDto.Reviewers == null || nodeDto.Reviewers.Count == 0)
            {
                //删除所有reviewer setting
                if (dbNodeSettings != null && dbNodeSettings.Count > 0)
                {
                    ctx.WorkflowStepConfiguration.RemoveRange(dbNodeSettings);
                    ctx.SaveChanges();
                }
            }
            else
            {
                //删除不存在的reviewer setting
                var delReviewerIds = new List<string>();
                var reviewerIds = nodeDto.Reviewers.Select(t => t.UserId).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (dbNodeSettings != null && dbNodeSettings.Count > 0)
                {
                    foreach (var dbNodeSetting in dbNodeSettings)
                    {
                        if (!reviewerIds.Contains(dbNodeSetting.OwnerId))
                        {
                            delReviewerIds.Add(dbNodeSetting.OwnerId);
                            ctx.WorkflowStepConfiguration.Remove(dbNodeSetting);
                        }
                    }
                    ctx.SaveChanges();
                }

                //增加新设置的reviewer setting
                var otherReviewers = nodeDto.Reviewers.Where(t => !delReviewerIds.Contains(t.UserId)).ToList();
                var existingOwnerIds = dbNodeSettings.Select(t => t.OwnerId).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var reviewer in otherReviewers)
                {
                    if (!existingOwnerIds.Contains(reviewer.UserId))
                    {
                        var wfNodeSetting = new RMWorkflowStepConfiguration
                        {
                            StepId = node.Id,
                            OwnerType = reviewer.InviteType,
                            OwnerId = reviewer.UserId
                        };
                        ctx.WorkflowStepConfiguration.Add(wfNodeSetting);
                    }
                }
                ctx.SaveChanges();
            }
        }

        public void RemoveWorkflowNodeSettings(Model.RMWorkflowStep node)
        {
            using var ctx = GetNewContext();
            //删除之前node设置的settings记录
            var oldNodeSettings = ctx.WorkflowStepConfiguration.Where(t => t.StepId == node.Id).ToList();
            if (oldNodeSettings != null && oldNodeSettings.Count > 0)
            {
                ctx.WorkflowStepConfiguration.RemoveRange(oldNodeSettings);
                ctx.SaveChanges();
            }
        }

        public List<string> GetReviewerIds(Guid workflowId)
        {
            var userIds = new List<string>();
            using (var ctx = GetNewContext())
            {
                var stepIds = ctx.WorkflowStep.Where(t => t.DefinitionId == workflowId).Select(t => t.Id).ToList();
                userIds = ctx.WorkflowStepConfiguration.Where(t => stepIds.Contains(t.StepId)).Select(t => t.OwnerId).Distinct().ToList();
            }
            return userIds;
        }

        public bool IsRunningWorkflow(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                var workflowReferenceId = ctx.WorkflowDefinition.Where(w => w.Id == id).Select(a => a.ReferenceId).FirstOrDefault();
                var MultipleVerisonWorkflowIds = ctx.WorkflowDefinition.Where(w => w.ReferenceId == workflowReferenceId).Select(w => w.Id).ToList();
                return ctx.WorkflowInstance.Any(w => MultipleVerisonWorkflowIds.Contains(w.DefinitionId) && w.Status == RMWorkflowStatus.Running);
            }
        }

        public void CheckSameWorkflow(WorkflowDefinitionDto dto)
        {
            using var ctx = GetNewContext();
            if (ctx.WorkflowDefinition.Any(t => t.Name == dto.Name && t.Id != dto.Id && t.ReferenceId != dto.ReferenceId))
            {
                throw new WorkflowNameConflictException();
            }
        }

        public List<RMWorkflowDefinition> GetAllWorkflows()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.WorkflowDefinition.GroupBy(w => w.ReferenceId)
                    .ToDictionary(z => z.Key, g => g.OrderByDescending(x => x.Version).FirstOrDefault())
                    .Select(o => o.Value)
                    .OrderByDescending(s => s.CreationTime)
                    .ToList();
            }
        }

        public List<RMWorkflowInstance> GetInstancesByHasSiteOwnersReviewerTypeDefinition(List<string> definitionIds, List<string> userAndGroupIds)
        {
            using(var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                var stepIds = context.WorkflowStep.Where(item => definitionIds.Contains(item.DefinitionId.ToString()) && item.ReviewerType == WorkflowReviewerType.SiteOwners).Select(item => item.Id.ToString()).ToList();
                var query = from instance in context.WorkflowInstance
                            where stepIds.Any(item => item == instance.CurStepId)
                            && !context.WorkflowExcludeInstanceOwner.Any(item => item.StepId.Equals(instance.CurStepId, StringComparison.OrdinalIgnoreCase) && item.InstanceId == instance.Id && userAndGroupIds.Contains(item.OwnerId))
                            select instance;
                return query.ToList();
            }
        }

        public List<RMWorkflowInstance> GetInstances(List<string> userIds)
        {

            var instances = new List<RMWorkflowInstance>();

            using (var ctx = GetNewContext())
            {

                ctx.Database.CommandTimeout = 600;

                var stepIds = ctx.WorkflowStepConfiguration.Where(t => userIds.Contains(t.OwnerId)).Select(t => t.StepId).Distinct().ToList();

                if (stepIds != null && stepIds.Count > 0)
                {
                    var strStepIds = stepIds.Select(s => s.ToString()).ToList();
                    var excludedInstanceIds = ctx.WorkflowExcludeInstanceOwner
                        .Where(w => strStepIds.Contains(w.StepId) && userIds.Contains(w.OwnerId))
                        .Select(w => w.InstanceId).ToList();

                    Expression<Func<RMWorkflowInstance, bool>> predicate = null;
                    if (excludedInstanceIds.Count > 0)
                    {
                        predicate = w => strStepIds.Contains(w.CurStepId) && !excludedInstanceIds.Contains(w.Id);
                    }
                    else
                    {
                        predicate = w => strStepIds.Contains(w.CurStepId);
                    }
                    instances = ctx.WorkflowInstance.Where(predicate).ToList();
                }
                return instances;
            }
        }

        public List<RMWorkflowInstance> GetAllInstances()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.WorkflowInstance.ToList();
            }
        }

        public List<string> GetReviewerIdsByStepId(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                var userIds = ctx.WorkflowStepConfiguration.Where(t => t.StepId == id).Select(t => t.OwnerId).Distinct().ToList();
                return userIds;
            }
        }

        public List<string> GetReviewersByStepIdAndSiteId(Guid workflowInstanceId, Guid stepId, Guid siteId)
        {
            using (var ctx = GetNewContext())
            {
                var step = ctx.WorkflowStep.First(item => item.Id == stepId);
                var excludeUsers = ctx.WorkflowExcludeInstanceOwner
                    .Where(item => item.InstanceId == workflowInstanceId && item.StepId == stepId.ToString())
                    .Select(item => item.OwnerId).ToHashSet();
                if (step.ReviewerType == WorkflowReviewerType.SiteOwners)
                {
                    var reviewers = ctx.WorkflowSiteOwner.Where(item => item.DefinitionId.Equals(step.DefinitionId.ToString(), StringComparison.OrdinalIgnoreCase) && item.SiteId == siteId).Select(item => item.OwnerId).ToList();
                    return reviewers.Except(excludeUsers).ToList();
                }
                var recordOwnerReviewers = GetReviewerIdsByStepId(stepId);
                return recordOwnerReviewers.Except(excludeUsers).ToList();
            }
        }

        public async Task<RMWorkflowInstance> GetWorkflowInstanceAsync(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.WorkflowInstance.FirstAsync(t => t.Id == id);
            }
        }

        public List<Guid> GetCompleteInstanceIds()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.WorkflowInstance.Where(w => w.Status == RMWorkflowStatus.Completed).Select(w => w.Id).ToList();
            }
        }

        public void AddExcludedOwnerForInstance(Guid instanceId, string ownerId, string stepId)
        {
            using (var ctx = GetNewContext())
            {
                if (!ctx.WorkflowExcludeInstanceOwner.Any(w => w.InstanceId == instanceId && w.StepId.Equals(stepId, StringComparison.OrdinalIgnoreCase) && w.OwnerId.Equals(ownerId, StringComparison.OrdinalIgnoreCase)))
                {
                    var model = new RMWorkflowExcludeInstanceOwner();
                    model.InstanceId = instanceId;
                    model.OwnerId = ownerId;
                    model.StepId = stepId;
                    ctx.WorkflowExcludeInstanceOwner.Add(model);
                    ctx.SaveChanges();
                }
            }
        }

        public List<RMWorkflowInstance> GetInstances(List<Guid> instanceIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.WorkflowInstance.Where(w => instanceIds.Contains(w.Id)).ToList();
            }
        }

        public bool ValidateHasCompleteWorkflows(List<Guid> instanceIds)
        {
            using(var context = GetNewContext())
            {
                return context.WorkflowInstance.Any(item => instanceIds.Contains(item.Id) && item.Status != RMWorkflowStatus.Running);
            }
        }

        public RMWorkflowDefinition GetWorkflowByName(string name)
        {
            using (var context = GetNewContext())
            {
                return context.WorkflowDefinition.Where(w => w.Name == name).ToList()
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault();
            }
        }

        public async Task<List<RMWorkflowDefinition>> GetCustomNotificationWorkflowAsync()
        {
            using var context = GetNewContext();
            var customStep = await context.WorkflowStep.Where(step => step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom).Select(step => step.DefinitionId).Distinct().ToListAsync();
            return await context.WorkflowDefinition.Where(w => customStep.Contains(w.Id)).ToListAsync();
        }

        public async Task<IEnumerable<RMWorkflowDefinition>> LoadWorkflowDefinitionsByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.WorkflowDefinition.AsNoTracking().OrderByDescending(w => w.CreationTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<IEnumerable<RMWorkflowStepConfiguration>> LoadWorkflowStepConfigurationByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.WorkflowStepConfiguration.AsNoTracking().OrderByDescending(w => w.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertWorkflowDefinitionTableAsync(IEnumerable<RMWorkflowDefinition> workflowDefinitions)
        {
            using var context = GetNewContext();
            string tableName = "RMWorkflowDefinitions";
            try
            {
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);


                var sqlBuilder = new StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, ReferenceId, Type, Name, Description, Version, ContentStr, XamlStr, HashCode, CreationTime, LastUpdatedTime, CreatedBy, Level) VALUES ");
                int i = 0;
                foreach (var item in workflowDefinitions)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8}, @p{paramIndex + 9}, @p{paramIndex + 10}, @p{paramIndex + 11}, @p{paramIndex + 12})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.ReferenceId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", (int)item.Type));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.Name));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", (object)item.Description ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", item.Version));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 6}", item.ContentStr));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 7}", (object)item.XamlStr ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 8}", (object)item.HashCode ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 9}", item.CreationTime));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 10}", item.LastUpdatedTime));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 11}", item.CreatedBy));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 12}", item.Level));
                    paramIndex += 13;
                    i++;
                }


                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMWorkflowDefinitions data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllWorkflowDefinitionAsync()
        {
            return await TruncateAllDataInTableAsync("RMWorkflowDefinitions");
        }

        public async Task<long> MultiGeoInsertWorkflowStepConfigurationTableAsync(IEnumerable<RMWorkflowStepConfiguration> workflowStepConfigurations)
        {
            using var context = GetNewContext();
            string tableName = "RMWorkflowStepConfigurations";
            try
            {
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, StepId, OwnerType, OwnerId) VALUES ");
                int i = 0;
                foreach(var item in workflowStepConfigurations)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.StepId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", (int)item.OwnerType));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.OwnerId));
                    paramIndex += 4;
                    i++;
                }

                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMWorkflowStepConfigurations data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllWorkflowStepConfigurationAsync()
        {
            return await TruncateAllDataInTableAsync("RMWorkflowStepConfigurations");
        }

        private static void UpdateReplicaWorkflowDefinition(WorkflowDefinitionDto dto, RMWorkflowDefinition workflow)
        {
            workflow.ReferenceId = dto.ReferenceId == Guid.Empty ? dto.Id : dto.ReferenceId;
            workflow.Type = dto.Type;
            workflow.Name = dto.Name;
            workflow.Description = dto.Description;
            workflow.Version = string.IsNullOrWhiteSpace(dto.Version) ? "1.0.0.0" : dto.Version;
            workflow.ContentStr = !string.IsNullOrEmpty(dto.ContentStr)
                ? dto.ContentStr
                : JsonConvert.SerializeObject(dto.Content);
            workflow.XamlStr = dto.XamlStr;
            workflow.HashCode = dto.HashCode;
            workflow.CreationTime = dto.CreatedOn == default ? DateTime.UtcNow : dto.CreatedOn;
            workflow.LastUpdatedTime = dto.LastUpdatedTime == default ? DateTime.UtcNow : dto.LastUpdatedTime;
            workflow.CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? string.Empty : dto.CreatedBy;
            workflow.Level = dto.LevelCount;
        }

        private static void AddReplicaWorkflowNode(RMDbContext ctx, Guid workflowId, Contract.RMWeb.CP.RMWorkflowStepNode node)
        {
            var workflowStep = new Model.RMWorkflowStep
            {
                Id = node.Id,
                DefinitionId = workflowId,
                DisplayName = node.DisplayName,
                Name = node.Name,
                ReviewerType = node.ReviewerType,
                UsedEmailTemplateMode = node.UsedEmailTemplateMode,
                UsedEmailTemplateId = node.UsedEmailTemplateId,
                CustomIntervalSetting = JsonConvert.SerializeObject(node.CustomIntervalSetting),
            };
            ctx.WorkflowStep.Add(workflowStep);

            foreach (var reviewer in node.Reviewers ?? new List<ReviewerUser>())
            {
                var setting = new RMWorkflowStepConfiguration
                {
                    StepId = workflowStep.Id,
                    OwnerType = reviewer.InviteType,
                    OwnerId = reviewer.UserId,
                };
                ctx.WorkflowStepConfiguration.Add(setting);
            }
        }

        private static Task<int> InsertWorkflowDefinitionAsync(RMDbContext ctx, RMWorkflowDefinition workflow)
        {
            string schemaName = SecurityUtils.SanitizeSQLSchemaName(ctx.SchemaName);
            string sql = $@"
INSERT INTO {schemaName}.RMWorkflowDefinitions
    (Id, ReferenceId, Type, Name, Description, Version, ContentStr, XamlStr, HashCode, CreationTime, LastUpdatedTime, CreatedBy, Level)
VALUES
    (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)";

            return ctx.Database.ExecuteSqlCommandAsync(
                sql,
                new System.Data.SqlClient.SqlParameter("@p0", workflow.Id),
                new System.Data.SqlClient.SqlParameter("@p1", workflow.ReferenceId),
                new System.Data.SqlClient.SqlParameter("@p2", (int)workflow.Type),
                new System.Data.SqlClient.SqlParameter("@p3", workflow.Name),
                new System.Data.SqlClient.SqlParameter("@p4", (object)workflow.Description ?? DBNull.Value),
                new System.Data.SqlClient.SqlParameter("@p5", workflow.Version),
                new System.Data.SqlClient.SqlParameter("@p6", workflow.ContentStr),
                new System.Data.SqlClient.SqlParameter("@p7", (object)workflow.XamlStr ?? DBNull.Value),
                new System.Data.SqlClient.SqlParameter("@p8", (object)workflow.HashCode ?? DBNull.Value),
                new System.Data.SqlClient.SqlParameter("@p9", workflow.CreationTime),
                new System.Data.SqlClient.SqlParameter("@p10", workflow.LastUpdatedTime),
                new System.Data.SqlClient.SqlParameter("@p11", workflow.CreatedBy),
                new System.Data.SqlClient.SqlParameter("@p12", workflow.Level));
        }
    }
}
