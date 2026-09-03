using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Myhub.Permission;
using AvePoint.RA.Contract.MyHub.Items.Views;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FSConnectionOwnerType = AvePoint.RA.DB.Model.FSConnectionOwnerType;

namespace AvePoint.RA.Api.Web.Public.Filters
{
    public class ValidMyhubPermissionParameterFilter : ActionFilterAttribute
    {
        private static readonly IRALogger _logger = RALogger.GetInstance(typeof(ValidMyhubPermissionParameterFilter));
        public IAccountDao AccountDao => PlatformWindsorManager.GetService(typeof(IAccountDao)) as IAccountDao;

        public IRMFSConnectionAndOwnerRelationshipDao RMFSConnectionAndOwnerRelationshipDao =>
            PlatformWindsorManager.GetService(typeof(IRMFSConnectionAndOwnerRelationshipDao))
                as IRMFSConnectionAndOwnerRelationshipDao;
        public ValidMyhubPermissionParameterType ActionType { get; set; }

        public ILnkUserGroupDao LnkUserGroupDao => PlatformWindsorManager.GetService(typeof(ILnkUserGroupDao)) as ILnkUserGroupDao;
        private IUserService UserService = PlatformWindsorManager.GetService<IUserService>();

        public ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterType actionType)
        {
            ActionType = actionType;
        }

        public ValidMyhubPermissionParameterFilter() { }

        public override async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            var validateRes = ActionType switch
            {
                ValidMyhubPermissionParameterType.QueryTreeFolders =>
                    await ValidatePermissionAsync<RMMyhubTreeChildFolderQueryInfo>(
                        actionContext,
                        p => p.PartitionKeyId),

                ValidMyhubPermissionParameterType.QueryDetailTable =>
                    await ValidatePermissionAsync<RMMyhubFolderDetailTableQueryInfo>(
                        actionContext,
                        p => p.PartitionKeyId),

                ValidMyhubPermissionParameterType.QueryFolderAndItems =>
                    await ValidatePermissionAsync<RMMyhubFolderItemQueryInfo>(
                        actionContext,
                        p => p.PartitionKeyId),
                ValidMyhubPermissionParameterType.GetNodeIdByConnectionId =>
                     await ValidatePermissionAsync<RMMyhubDriveDirectionQueryInfo>(
                        actionContext,
                        p => p.PartitionKeyId),
                ValidMyhubPermissionParameterType.RunFSDashboardSyncJob =>
                    await ValidatePermissionAsync<FileSystemMyhubSelectedNodeDto>(
                        actionContext,
                        p => p.PartitionKeyId),

                ValidMyhubPermissionParameterType.GetParameterBeforeUnderReviewQuery =>
                    await ValidatePermissionAsync<RMMyhubPendingDisposalQueryInfo>(
                        actionContext,
                        p => p.PartitionKeyId,
                        new List<FSConnectionOwnerType> { FSConnectionOwnerType.InformationOwner }),

                ValidMyhubPermissionParameterType.UpdateConnectionRecordOwners =>
                    await ValidatePermissionAsync<RMConnectionRecordOwnerUpdateModel>(
                        actionContext,
                        p => p.ConnectionId.ToString(),
                        new List<FSConnectionOwnerType> { FSConnectionOwnerType.InformationOwner }),

                ValidMyhubPermissionParameterType.PauseOrResume =>
                    await ValidatePermissionAsync<PauseOrResumeReq>(
                        actionContext,
                        p => p.NodeIds,
                        new List<FSConnectionOwnerType> { FSConnectionOwnerType.InformationOwner }),
                ValidMyhubPermissionParameterType.ClassifyUpdate =>
                    await ValidatePermissionAsync<RMMyhubClassifyQueryInfo>(
                        actionContext,
                        p => p.PartitionKeyId),
                ValidMyhubPermissionParameterType.GetPermissionByConnectionId =>
                    await ValidateConnectionPermissionAsync(actionContext),
                ValidMyhubPermissionParameterType.GetFSDashboardData =>
                    await ValidatePermissionAsync<RMMyHubFolderDashboard>(
                        actionContext,
                        p => p.PartitionKeyId),
                ValidMyhubPermissionParameterType.QueryAuditTrial =>
                    await ValidatePermissionAsync<RMMyhubAuditTrialQueryInfo>(
                        actionContext,
                        p => p.QueryParam.PartitionKeyId),
                _ => true
            };

            if (validateRes)
            {
                await next();
            }
        }
        private async Task<bool> ValidatePermissionAsync<T>(ActionExecutingContext actionContext, Func<T, string> partitionKeySelector, List<FSConnectionOwnerType>? userRoles = null) where T : class
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault() as T;
            if (parameter == null)
            {
                return SetNoPermissionResult(actionContext);
            }

            var partitionKeyId = partitionKeySelector(parameter);
            if (!Guid.TryParse(partitionKeyId, out var connectionId))
            {
                return SetNoPermissionResult(actionContext);
            }
            var userIds = new List<int>();
            try
            {
                userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to get removed/group ids for user [{TenantLocalValue.LogonUserId}]. Fallback to active user only. Error: {ex}");
            }
            var connectionIds = (await RMFSConnectionAndOwnerRelationshipDao.GetConnectionsByUserIdsAndRoles(userIds.Distinct().ToList(), userRoles))
                .Select(c => c.Id)
                .ToHashSet();

            return connectionIds.Contains(connectionId)
                || SetNoPermissionResult(actionContext);
        }

        private bool SetNoPermissionResult(ActionExecutingContext actionContext)
        {
            actionContext.Result = new JsonResult(new
            {
                hasPermission = false
            })
            {
                StatusCode = StatusCodes.Status200OK
            };

            return false;
        }

        private async Task<bool> ValidatePermissionAsync<T>(ActionExecutingContext actionContext, Func<T, IEnumerable<string>> partitionKeySelector, List<FSConnectionOwnerType>? userRoles = null) where T : class
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault() as T;
            if (parameter == null)
            {
                return SetNoPermissionResult(actionContext);
            }
            var partitionKeyIds = partitionKeySelector(parameter);
            var connectionIds = new HashSet<Guid>();
            foreach (var partitionKeyId in partitionKeyIds)
            {
                if (Guid.TryParse(partitionKeyId, out var connectionId))
                {
                    connectionIds.Add(connectionId);
                }
                else
                {
                    return SetNoPermissionResult(actionContext);
                }
            }
            var userIds = new List<int>();
            try
            {
                userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to get removed/group ids for user [{TenantLocalValue.LogonUserId}]. Fallback to active user only. Error: {ex}");
            }
            var allowedConnectionIds = (await RMFSConnectionAndOwnerRelationshipDao.GetConnectionsByUserIdsAndRoles(userIds.Distinct().ToList(), userRoles))
                .Select(c => c.Id)
                .ToHashSet();
            return connectionIds.All(id => allowedConnectionIds.Contains(id))
                || SetNoPermissionResult(actionContext);
        }
        private async Task<bool> ValidateConnectionPermissionAsync(ActionExecutingContext actionContext, List<FSConnectionOwnerType>? userRoles = null)
        {
            if (!actionContext.ActionArguments.TryGetValue("connectionId", out var value) ||
                value is not Guid connectionId)
            {
                return SetNoPermissionResult(actionContext);
            }

            var userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);

            var connectionIds = (await RMFSConnectionAndOwnerRelationshipDao
                .GetConnectionsByUserIdsAndRoles(userIds.Distinct().ToList(), userRoles))
                .Select(c => c.Id)
                .ToHashSet();

            return connectionIds.Contains(connectionId)
                || SetNoPermissionResult(actionContext);
        }
        public enum ValidMyhubPermissionParameterType
        {
            None = 0,
            QueryDrives = 1,
            QueryTreeFolders = 2,
            QueryDetailTable = 3,
            QueryFolderAndItems = 4,
            ClassifyUpdate = 5,
            RunFSDashboardSyncJob = 6,
            GetParameterBeforeUnderReviewQuery = 7,
            GetNodeIdByConnectionId = 8,
            UpdateConnectionRecordOwners = 9,
            PauseOrResume = 10,
            GetPermissionByConnectionId = 11,
            GetFSDashboardData = 12,
            QueryAuditTrial = 13,
        }
    }
}
