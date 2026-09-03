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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Google;
using Microsoft.Azure.Cosmos.Linq;
using PnP.Core.QueryModel;

namespace AvePoint.RA.DB.Dao.GoogleSyncNodeDao;

public class RMGoogleRemoteNodeDao : BaseDao<RMRemoteNode>, IRMGoogleRemoteNodeDao
{
    private const int NodeLevel_GoogleMyDriveGroup = (int)NodeLevel.GoogleMyDriveContainer;
    private const int NodeLevel_GoogleSharedDriveGroup = (int)NodeLevel.GoogleSharedDriveContainer;
    private const int NodeLevel_GoogleMyDrive = (int)NodeLevel.GoogleMyDrive;
    private const int NodeLevel_GoogleSharedDrive = (int)NodeLevel.GoogleSharedDrive;
    private readonly Dictionary<NodeLevelExpressionType, Expression<Func<RMRemoteNode, bool>>> _expressionDict = new()
    {
        {
            NodeLevelExpressionType.ExpressionContainers,
            rmRemoteNode => rmRemoteNode.NodeLevel == (int)NodeLevel.GoogleMyDriveContainer ||
                            rmRemoteNode.NodeLevel == (int)NodeLevel.GoogleSharedDriveContainer
        },
        {
            NodeLevelExpressionType.ExpressionGoogleDrive,
            rmRemoteNode => rmRemoteNode.NodeLevel == (int)NodeLevel.GoogleMyDrive ||
                            rmRemoteNode.NodeLevel == (int)NodeLevel.GoogleSharedDrive
        }
    };

    private readonly RALogger logger = RALogger.GetInstance(typeof(RMRemoteNodeDao));

    private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

    private IRMScopeRoleAssignmentDao RmScopeRoleAssignmentDao =>
        PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();

    public async Task<RMSampleGoogleTreeNode> GetContainersWithoutCheckPermissionAsync(RMSampleGoogleTreeNode node)
    {
        return await ExecuteWithRetry(context =>
            GetChildrenNodesPaged(node, context, _expressionDict[NodeLevelExpressionType.ExpressionContainers]));
    }

    public Task<RMSampleGoogleTreeNode> GetContainersWithoutCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node, params NodeLevel[] nodeLevels)
    {
        return ExecuteWithRetry(context =>
            GetAllChildrenNodes(node, context, rmRemoteNode => nodeLevels.Contains((NodeLevel)rmRemoteNode.NodeLevel)));
    }

    public Task<RMSampleGoogleTreeNode> GetContainersWithCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node, params NodeLevel[] nodeLevels)
    {
        var userId = TenantLocalValue.LogonUserId;


        return GetAllAuthorizedChildrenNodes(node, "ORDER BY Name ASC", context =>
        {
            var sqlParameterList = new List<SqlParameter>
            {
                new SqlParameter("@userId", TenantLocalValue.LogonUserId),
                new SqlParameter("@dataSourceType", (int)SourceFlag.Google),
            };
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var queryAllSql =
                $@"SELECT * FROM [{context.SchemaName}].RMRemoteNodes WHERE Id IN ( 
  SELECT ScopeId FROM [{context.SchemaName}].RMScopeRoleAssignments AS p 
    JOIN [{context.SchemaName}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{context.SchemaName}].RMAccounts WHERE IsRemoved=0 AND (
          UserId=@userId OR UserId IN (
            SELECT GroupId FROM [{context.SchemaName}].RMLnkUserGroups WHERE UserId=@userId
          )
	    )
      )
    WHERE DataSourceType = @dataSourceType)";
            if(nodeLevels != null && nodeLevels.Any())
            {
                var levels = string.Join(",", nodeLevels.Select(l => (int)l));
                queryAllSql += $"AND NodeLevel IN (@nodeLevels)";
                sqlParameterList.Add(new SqlParameter("@nodeLevels", levels));
            }
            return new Tuple<string, SqlParameter[]>(queryAllSql, sqlParameterList.ToArray());
        });
    }

    public async Task<List<RMRemoteNode>> GetAllGoogleRemoteNodes()
    {
        using (var ctx = GetNewContext())
        {
            var query = ctx.RMRemoteNodes.Where(x => x.NodeLevel == (int)NodeLevel.GoogleMyDrive || x.NodeLevel == (int)NodeLevel.GoogleSharedDrive);
            return await query.ToListAsync();
        }
    }

    public async Task<RMSampleGoogleTreeNode> GetContainersWithCheckPermissionAsync(RMSampleGoogleTreeNode node)
    {
        var userId = TenantLocalValue.LogonUserId;

        return await GetAuthorizedChildrenNodesPaged(node, "ORDER BY Name ASC", context =>
        {
            var queryAllSql =
                $@"SELECT * FROM [{context.SchemaName}].RMRemoteNodes WHERE Id IN ( 
  SELECT ScopeId FROM [{context.SchemaName}].RMScopeRoleAssignments AS p 
    JOIN [{context.SchemaName}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{context.SchemaName}].RMAccounts WHERE IsRemoved=0 AND (
          UserId= @UserId OR UserId IN (
            SELECT GroupId FROM [{context.SchemaName}].RMLnkUserGroups WHERE UserId= @UserId
          )
	    )
      )
    WHERE DataSourceType={(int)SourceFlag.Google}
)";
            return new Tuple<string, SqlParameter[]>(queryAllSql, new SqlParameter[] {new SqlParameter("@UserId", userId)});
        });
    }

    public async Task<RMSampleGoogleTreeNode> GetDrivesWithoutCheckPermissionAsync(RMSampleGoogleTreeNode node)
    {
        Expression<Func<RMRemoteNode, bool>> expressionParentId = rmRemoteNode => rmRemoteNode.ParentId == node.Id;
        return await ExecuteWithRetry(context => GetChildrenNodesPaged(node, context, expressionParentId));
    }

    public async Task<RMSampleGoogleTreeNode> GetDrivesWithoutCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node)
    {
        Expression<Func<RMRemoteNode, bool>> expressionParentId = rmRemoteNode => rmRemoteNode.ParentId == node.Id;
        return await ExecuteWithRetry(context => GetAllChildrenNodes(node, context, expressionParentId));
    }

    public async Task<RMSampleGoogleTreeNode> GetDrivesWithCheckPermissionAsync(RMSampleGoogleTreeNode node)
    {
        var userId = TenantLocalValue.LogonUserId;

        return await GetAuthorizedChildrenNodesPaged(node, "ORDER BY Name ASC", context =>
        {
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var queryAllSql =
                $@"SELECT * FROM [{context.SchemaName}].RMRemoteNodes WHERE EXISTS (
  SELECT ScopeId FROM [{context.SchemaName}].RMScopeRoleAssignments AS p 
    JOIN [{context.SchemaName}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{context.SchemaName}].RMAccounts WHERE IsRemoved=0 AND (
          UserId= @UserId OR UserId IN (
            SELECT GroupId FROM [{context.SchemaName}].RMLnkUserGroups WHERE UserId= @UserId
          )
	    )
      )
    WHERE p.ScopeId=@ParendId
) AND ParentId=@ParendId ";
            return Tuple.Create(queryAllSql, new[] { new SqlParameter("@ParendId", node.Id) , new SqlParameter("@UserId", userId) });
        });
    }

    public async Task<RMSampleGoogleTreeNode> GetDrivesWithCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node)
    {
        var userId = TenantLocalValue.LogonUserId;

        return await GetAllAuthorizedChildrenNodes(node, "ORDER BY Name ASC", context =>
        {
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var queryAllSql =
                $@"SELECT * FROM [{context.SchemaName}].RMRemoteNodes WHERE EXISTS (
  SELECT ScopeId FROM [{context.SchemaName}].RMScopeRoleAssignments AS p 
    JOIN [{context.SchemaName}].RMSecurityGroupMemberships m ON p.GroupId=m.GroupId 
	  AND m.UserId IN (
        SELECT UserId FROM [{context.SchemaName}].RMAccounts WHERE IsRemoved=0 AND (
          UserId= @userId OR UserId IN (
            SELECT GroupId FROM [{context.SchemaName}].RMLnkUserGroups WHERE UserId= @userId
          )
	    )
      )
    WHERE p.ScopeId=@ParendId
) AND ParentId=@ParendId ";
            return Tuple.Create(queryAllSql, new[] { new SqlParameter("@ParendId", node.Id), new SqlParameter("@userId", userId) });
        });
    }

    public async Task<RMSampleGoogleTreeNode> GetContainersForSearchAsync(RMSampleGoogleTreeNode node,
        bool checkPermission)
    {
        using var context = GetNewContext();
        context.Database.CommandTimeout = 900;
        List<string> parentPermissionIds;

        if (!checkPermission)
            parentPermissionIds = context.RMRemoteNodes
                .Where(r => r.Name.Contains(node.SearchKey) && !string.IsNullOrEmpty(r.ParentId))
                .Where(_expressionDict[NodeLevelExpressionType.ExpressionGoogleDrive])
                .Select(r => r.ParentId).Distinct().ToList();
        else
            parentPermissionIds = await GetPermissionContainerIdsAsync();

        var remoteNodes = context.RMRemoteNodes.Where(r => parentPermissionIds.Contains(r.Id))
            .Where(_expressionDict[NodeLevelExpressionType.ExpressionContainers]).ToList();
        node.Children = remoteNodes.OrderBy(n => n.Name).Skip(node.PageIndex * node.PageSize).Take(node.PageSize)
            .ToList().Select(childNode => Convert2GoogleTreeNode(childNode, node)).ToList();
        node.ChildrenCount = parentPermissionIds.Count;
        return node;
    }

    public RMGoogleSetting LoadGoogleSetting(Guid containerId, Guid driveId)
    {
        using var context = GetNewContext(); 
        if (!driveId.Equals(Guid.Empty))
        {
            return context.RMGoogleSettings.FirstOrDefault(s =>  s.DriveId.Equals(driveId) && s.ContainerId == containerId && !s.IsRemoved);
        }

        return context.RMGoogleSettings.FirstOrDefault(setting =>
            setting.ContainerId == containerId && setting.DriveId == Guid.Empty && !setting.IsRemoved);
    }
    
   

    public List<RMSimpleRule> GetMappingRules(Guid containerId, Guid driveId)
    {
        using (var context = GetNewContext())
        {
            List<RMGoogleSettingRuleMapping> settings = null;
            if (driveId != Guid.Empty)
            {
                settings = context.RMGoogleSettingRuleMapping.Where(o => o.ScopeId == driveId && o.Type != (int)RuleType.Archiver).ToList();
            }
            if (settings == null || settings.Count == 0)
            {
                settings = context.RMGoogleSettingRuleMapping.Where(o => o.ScopeId == containerId && o.Type != (int)RuleType.Archiver).ToList();
            }
            return settings.Select(o => new RMSimpleRule { RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = o.RuleOrder }).OrderBy(o => o.RuleOrder).ToList();
        }
    }

    public async Task<List<string>> GetPermissionContainerIdsAsync()
    {
        List<string> containerIds = [];
        try
        {
            var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            var allContainers =
                (await RmScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x =>
                    (int)SourceFlag.Google == x.Key);
            foreach (var item in allContainers)
                item.Value.ForEach(o =>
                {
                    if (!containerIds.Contains(o.ToString())) containerIds.Add(o.ToString());
                });
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to get container ids, error:{ex}");
        }

        return containerIds;
    }

    private async Task<RMSampleGoogleTreeNode> GetChildrenNodesPaged(RMSampleGoogleTreeNode node, RMDbContext context,
        Expression<Func<RMRemoteNode, bool>> expression)
    {
        try
        {
            context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var queryResults = context.RMRemoteNodes.Where(expression);
            if (node.SearchKey.IsNotNullOrEmpty())
            {
                Expression<Func<RMRemoteNode, bool>> expressionName = rmRemoteNode =>
                    rmRemoteNode.Name.Contains(node.SearchKey);
                queryResults = queryResults.Where(expressionName);
            }

            var count = await queryResults.CountAsync();
            ResetPagerInfo(node, count);

            if (node.ChildrenCount > 0)
            {
                queryResults = queryResults.OrderBy(n => n.Name).Skip(node.PageIndex * node.PageSize)
                    .Take(node.PageSize);
                var rmRemoteNodes = queryResults.ToList();
                node.Children = rmRemoteNodes.Select(childNode => Convert2GoogleTreeNode(childNode, node)).ToList();
            }
            else
            {
                node.Children = [];
            }

            return node;
        }
        catch (Exception ex)
        {
            logger.Error($"GetGoogleChildrenNodesPaged error : {ex}");
            if (ex.InnerException != null)
                logger.Error($"GetGoogleChildrenNodesPaged InnerException : {ex.InnerException}");
            throw;
        }
    }
    
    private async Task<RMSampleGoogleTreeNode> GetAllChildrenNodes(RMSampleGoogleTreeNode node, RMDbContext context,
        Expression<Func<RMRemoteNode, bool>> expression)
    {
        try
        {
            context = GetNewContext();
            context.Database.CommandTimeout = 900;
            var queryResults = context.RMRemoteNodes.Where(expression);
            
            node.ChildrenCount = await queryResults.CountAsync();

            if (node.ChildrenCount > 0)
            {
                queryResults = queryResults.OrderBy(n => n.Name);
                var rmRemoteNodes = await queryResults.ToListAsync();
                node.Children = rmRemoteNodes.Select(childNode => Convert2GoogleTreeNode(childNode, node)).ToList();
            }
            else
            {
                node.Children = [];
            }

            return node;
        }
        catch (Exception ex)
        {
            logger.Error($"Get all children nodes error : {ex}");
            if (ex.InnerException != null)
                logger.Error($"Get all children nodes InnerException : {ex.InnerException}");
            throw;
        }
    }

    public RMSampleGoogleTreeNode GetGoogleContainerById(string id)
    {
        return ExecuteWithRetry(context =>
            {
                var node = context.RMRemoteNodes
                    .Where(m => m.Id == id && (m.NodeLevel == NodeLevel_GoogleMyDriveGroup || m.NodeLevel == NodeLevel_GoogleSharedDriveGroup))
                    .FirstOrDefault();
                if (node != null)
                {
                    return Convert2GoogleTreeNode(node);
                }
                return null;
            });
    }

    public RMSampleGoogleTreeNode GetGoogleDriveById(string id)
    {
        return ExecuteWithRetry(context =>
        {
            var node = context.RMRemoteNodes
                .Where(m => m.Id == id && (m.NodeLevel == NodeLevel_GoogleMyDrive
                    || m.NodeLevel == NodeLevel_GoogleSharedDrive
                ))
                .FirstOrDefault();
            if (node != null)
            {
                return Convert2GoogleTreeNode(node);
            }
            return null;
        });
    }

    public List<RMSampleGoogleTreeNode> GetGoogleDrives(IEnumerable<string> ids)
    {
        return ExecuteWithRetry(context =>
        {
            var nodes = context.RMRemoteNodes
                .Where(remoteNode => ids.Contains(remoteNode.Id) && (remoteNode.NodeLevel == NodeLevel_GoogleMyDrive
                                                                     || remoteNode.NodeLevel == NodeLevel_GoogleSharedDrive
                    )).ToList();
            return nodes.Select(node => Convert2GoogleTreeNode(node)).ToList();
        });
    }

    public List<RMSampleGoogleTreeNode> GetGoogleContainers(IEnumerable<string> ids)
    {
        return ExecuteWithRetry(context =>
        {
            var nodes = context.RMRemoteNodes
                .Where(remoteNode => ids.Contains(remoteNode.Id) && (remoteNode.NodeLevel == NodeLevel_GoogleMyDriveGroup
                                                                     || remoteNode.NodeLevel == NodeLevel_GoogleSharedDriveGroup
                    )).ToList();
            return nodes.Select(node => Convert2GoogleTreeNode(node)).ToList();
        });
    }

    public List<RMSampleGoogleTreeNode> GetAllGoogleContainers()
    {
        return ExecuteWithRetry(context =>
        {
            var nodes = context.RMRemoteNodes
                .Where(m => (m.NodeLevel == NodeLevel_GoogleMyDriveGroup
                    || m.NodeLevel == NodeLevel_GoogleSharedDriveGroup)).ToList();
            if (nodes != null && nodes.Count > 0)
            {
                return nodes.ConvertAll(x => Convert2GoogleTreeNode(x));
            }

            return null;
        });
    }

    public List<RMSampleGoogleTreeNode> GetGoogleDrivesByParentId(string parentId)
    {
        return ExecuteWithRetry(context =>
        {
            var nodes = context.RMRemoteNodes
                .Where(m => m.ParentId == parentId && (m.NodeLevel == NodeLevel_GoogleMyDrive
                    || m.NodeLevel == NodeLevel_GoogleSharedDrive)).ToList();
            if(nodes != null && nodes.Count > 0)
            {
                return nodes.ConvertAll(x => Convert2GoogleTreeNode(x));
            }

            return null;
        });
    }

    private RMSampleGoogleTreeNode Convert2GoogleTreeNode(RMRemoteNode childNode, RMSampleGoogleTreeNode parentNode = null)
    {
        var sample = new RMSampleGoogleTreeNode
        {
            Id = childNode.Id,
            Name = RMDatabaseDefaultEncryptor.DecryptToString(childNode.UserName),
            DisplayName = childNode.Name,
            NodeType = childNode.NodeLevel,
            Level = childNode.NodeLevel,
            ParentId = childNode.ParentId,
            ObjectId = childNode.ObjectId, //GetObjectIdByType(childNode),
            Parent = parentNode,
            FullPath = childNode.Url,
            GoogleTenantId = childNode.TenantId
        };
        AssignSpecificGoogleProps(sample);
        return sample;
    }



    private void AssignSpecificGoogleProps(RMSampleGoogleTreeNode node)
    {
        switch (node.NodeType)
        {
            case ((int)NodeLevel.GoogleMyDrive):
            case ((int)NodeLevel.GoogleSharedDrive):
                node.NodeId = node.Id;
                node.ContainerId = node.ParentId;
                break;
            case ((int)NodeLevel.GoogleMyDriveContainer):
            case ((int)NodeLevel.GoogleSharedDriveContainer):
                node.ContainerId = node.Id;
                break;
        }
    }

    private void ResetPagerInfo(RMSampleGoogleTreeNode node, int childrenCount)
    {
        node.ChildrenCount = childrenCount;
        if (node.PageIndex * node.PageSize >= node.ChildrenCount)
            node.PageIndex = (node.ChildrenCount - 1) / node.PageSize;
    }

    private async Task<RMSampleGoogleTreeNode> GetAuthorizedChildrenNodesPaged(RMSampleGoogleTreeNode node,
        string orderingClause, Func<RMDbContext, Tuple<string, SqlParameter[]>> getQueryAllSql)
    {
        ExecuteWithRetry(context =>
        {
            var queryAllSqlInfo = getQueryAllSql(context);
            var queryParameters = queryAllSqlInfo.Item2.ToList() ?? new List<SqlParameter>();
            queryParameters.Add(new SqlParameter("@SearchKey",$"%{node.SearchKey}%"));
            var queryAllSql = queryAllSqlInfo.Item1;
            if (node.SearchKey.IsNotNullOrEmpty()) queryAllSql += $" And [Name] like @SearchKey";
            var queryCountSql =
                $"SELECT COUNT(1) {queryAllSql.Substring(queryAllSql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase))}";
            var pagedQuerySql = DatabaseUtility.GetPaginatedSQL(node.PageIndex * node.PageSize, node.PageSize,
                queryAllSql, orderingClause);
            var total = context.Database.SqlQuery<int>(queryCountSql, queryParameters).FirstOrDefault();
            ResetPagerInfo(node, total);
            if (total > 0)
            {
                var results = context.Database.SqlQuery<RMRemoteNode>(
                    pagedQuerySql, queryParameters.Select(p => (p as ICloneable).Clone()).ToArray()
                ).ToList();
                node.Children = results.ConvertAll(remoteNode =>
                {
                    var child = Convert2GoogleTreeNode(remoteNode, node);
                    return child;
                });
            }
            else
            {
                node.Children = [];
            }
        });

        return node;
    }
    
    private async Task<RMSampleGoogleTreeNode> GetAllAuthorizedChildrenNodes(RMSampleGoogleTreeNode node,
        string orderingClause, Func<RMDbContext, Tuple<string, SqlParameter[]>> getQueryAllSql)
    {
        ExecuteWithRetry(context =>
        {
            var queryAllSqlInfo = getQueryAllSql(context);
            var queryParameters = queryAllSqlInfo.Item2.ToList() ?? [];
            var queryAllSql = queryAllSqlInfo.Item1;
            var queryCountSql =
                $"SELECT COUNT(1) {queryAllSql.Substring(queryAllSql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase))}";

            var total = context.Database.SqlQuery<int>(queryCountSql, queryParameters).FirstOrDefault();
            node.ChildrenCount = total;
            if (total > 0)
            {
                var results = context.Database.SqlQuery<RMRemoteNode>(
                    queryAllSql, queryParameters.Select(p => (p as ICloneable).Clone()).ToArray()
                ).ToList();
                node.Children = results.ConvertAll(remoteNode =>
                {
                    var child = Convert2GoogleTreeNode(remoteNode, node);
                    return child;
                });
            }
            else
            {
                node.Children = [];
            }
        });

        return node;
    }

    public List<string> GetContainerNames(List<string> nodeIds)
    {
        using var ctx = GetNewContext();
        return ctx.RMRemoteNodes.Where(x => nodeIds.Contains(x.Id)).Select(x => x.Url).ToList();
    }

    public List<string> GetGoogleTenantIdsUnderContainer(string containerId)
    {
        using var ctx = GetNewContext();
        return ctx.RMRemoteNodes.Where(m => m.ParentId == containerId && (m.NodeLevel == NodeLevel_GoogleMyDrive|| m.NodeLevel == NodeLevel_GoogleSharedDrive))
            .Select(x => x.TenantId)
            .Distinct()
            .ToList();
    }
    public async Task<List<string>> GetGoogleTenantIdsUnderContainers(List<string> containerIds)
    {
        using var ctx = GetNewContext();
        return await ctx.RMRemoteNodes
                  .Where(m => containerIds.Contains(m.ParentId) &&
                             (m.NodeLevel == NodeLevel_GoogleMyDrive || m.NodeLevel == NodeLevel_GoogleSharedDrive))
                  .Select(x => x.TenantId)
                  .Distinct()
                  .ToListAsync();
    }

    public List<string> GetGoogleTenantIdsUnderNodes(List<string> nodeIds, NodeLevelExpressionType expType)
    {
        if (nodeIds == null || nodeIds.Count == 0)
            return new List<string>();

        Expression<Func<RMRemoteNode, bool>> queryIdsExpression = expType switch
        {
            NodeLevelExpressionType.ExpressionContainers => n => nodeIds.Contains(n.ParentId),
            NodeLevelExpressionType.ExpressionGoogleDrive => n => nodeIds.Contains(n.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(expType), expType, null)
        };

        using var ctx = GetNewContext();
        return ctx.RMRemoteNodes
                  .Where(_expressionDict[NodeLevelExpressionType.ExpressionGoogleDrive])
                  .Where(queryIdsExpression)
                  .Select(n => n.TenantId)
                  .Distinct()
                  .ToList();
    }

    public enum NodeLevelExpressionType
    {
        ExpressionContainers,
        ExpressionGoogleDrive
    }
    
    #region Support GoogleOne
    public async Task<List<RMRemoteNode>> QueryGoogleNodesForSearchAsync(RMSampleGoogleTreeNode node, (string sql, List<SqlParameter> parameters) queryTuple)
    {
        try
        {
            return await ExecuteWithRetry(async context =>
            {
                var schema = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var finalSql = queryTuple.sql.Replace("@SCHEMA", schema);
                context.Database.CommandTimeout = 900;
                return context.Database.SqlQuery<RMRemoteNode>(finalSql, queryTuple.parameters.ToArray()).ToList();
            });
        }
        catch (Exception ex)
        {
            logger.Error("An error occurred while querying Google nodes for search.", ex);
            return new List<RMRemoteNode>();
        }
    }
    #endregion
}