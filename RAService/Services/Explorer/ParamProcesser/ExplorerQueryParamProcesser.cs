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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.Records.Core.Utilities.Extensions;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Explorer.ParamProcesser
{
    public class ExplorerQueryParamProcesser : IExplorerQueryParamProcesser
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IFSConnectionDao FSConnDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        private IPhysicalReqeustService PhysicalRequestService => PlatformWindsorManager.GetService<IPhysicalReqeustService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();


        #region Advanced search
        public async System.Threading.Tasks.Task ProcessV3Async(ExplorerQueryOptionV3 queryOptionV3)
        {
            using (var performance = new PerformanceScope("ExportQueryParamProcesser.ProcessV3Async"))
            {
                await AssembleDelayedLoanQueryAsync(queryOptionV3);
                foreach (var searchOption in queryOptionV3.Values)
                {
                    ProcessNodeIdV3(searchOption);
                }
            }
        }

        /// <summary>
        /// 组装过期后没有被return的search条件，
        /// 需要先从DB中查出没有return的object id，然后拿着这些id再去Cosmos DB查询
        /// </summary>
        /// <param name="queryOption"></param>
        private async System.Threading.Tasks.Task AssembleDelayedLoanQueryAsync(ExplorerQueryOptionV3 queryOption)
        {
            var searchOption = queryOption.Values.FirstOrDefault(o => o.Column.Id == QueryCloumnIds.Loan);
            if (searchOption == null) return;
            var timeInfo = JsonConvert.DeserializeObject<DateInfo>(searchOption.Value);

            var ticks = Convert2Ticks(timeInfo);

            var loanObjectIds = await PhysicalRequestService.GetLoanObjectIdsAsync(ticks.Item1, ticks.Item2);

            queryOption.AssembleRecordsId(loanObjectIds);
        }

        private (long, long) Convert2Ticks(DateInfo timeInfo)
        {
            long startDt = DateTime.MinValue.Ticks;
            long endDt = DateTime.MaxValue.Ticks;

            switch (timeInfo.Condition)
            {
                case DateCondition.None: //date not specified
                    return (-1, DateTime.MinValue.Ticks);
                case DateCondition.BeforeNow:
                    endDt = DateTime.UtcNow.Ticks;
                    break;
                case DateCondition.Before:
                    endDt = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight).Ticks;
                    break;
                case DateCondition.After:
                    startDt = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight).Ticks;
                    break;
                case DateCondition.FromTo:
                    startDt = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight).Ticks;
                    endDt = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight).Ticks;
                    break;
                case DateCondition.All:
                    startDt = -1;
                    break;
                default:
                    endDt = DateTime.UtcNow.Ticks;
                    break;
            }

            return (startDt, endDt);
        }

        /// <summary>
        /// process fs tree node id
        /// </summary>
        /// <param name="searchOption"></param>
        private void ProcessNodeIdV3(ExplorerSearchOptionV3 searchOption)
        {
            if (!string.Equals(QueryCloumnIds.NodeId, searchOption.Column.Id, StringComparison.OrdinalIgnoreCase)) return;
            searchOption.Value = GetRealNodeId(JsonConvert.DeserializeObject<string>(searchOption.Value));
        }
        #endregion

        public async System.Threading.Tasks.Task ProcessAsync(ExplorerQueryOptionV2 queryOption)
        {
            var userPermission = await  SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>(false);

            await ProcessSecurityTrimmingAsync(queryOption, userPermission);
            queryOption.ConvertSearchKey2LowerCase();
            ProcessNodeId(queryOption.FilterOption);
            await ProcessPermissionIdsAsync(queryOption.FilterOption);
            if (!await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold))
            {
                await ProcessContainerIdsAsync(queryOption.FilterOption);
            }
            ProcessOrphanedODIds(queryOption.FilterOption);
        }

        /// <summary>
        /// check source flags and node types
        /// </summary>
        /// <param name="queryOption"></param>
        /// <param name="userPermission"></param>
        private async System.Threading.Tasks.Task ProcessSecurityTrimmingAsync(ExplorerQueryOptionV2 queryOption, RMPermissionMasks userPermission)
        {
            queryOption.SecurityTrimming(userPermission);
            var holdManagerPermission = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold);
            var azureFilePermission = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSEndUser);
            var boxPermission = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxEndUser);
            var googlePermission = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser);
            var teamsPermission = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);
            var gControlPermission = await TenantService.HasInitGControlPlatForm();
            if (!azureFilePermission && !holdManagerPermission)
            {
                queryOption.FilterOption.SourceFlags.RemoveAll(s => s == SourceFlag.AzureFileShare);
            }

            if (!boxPermission && !holdManagerPermission)
            {
                queryOption.FilterOption.SourceFlags.RemoveAll(s => s == SourceFlag.Box);
            }

            if (!googlePermission && !gControlPermission && !holdManagerPermission)
            {
                queryOption.FilterOption.SourceFlags.RemoveAll(s => s == SourceFlag.Google);
            }
            if (!teamsPermission && !holdManagerPermission)
            {
                queryOption.FilterOption.SourceFlags.RemoveAll(s => s == SourceFlag.Teams);
            }
            else
            {
                if (queryOption.FilterOption.NodeTypes != null && !queryOption.FilterOption.NodeTypes.Contains(Contract.RMWeb.Tree.Base.RMNodeLevel.Item))
                {
                    queryOption.FilterOption.NodeTypes.Add(Contract.RMWeb.Tree.Base.RMNodeLevel.Item);
                }
            }
            if (!queryOption.HasAnySourceFlag())
            {
                ThrowNoPermissionException("No source flags.");
            }

            if (queryOption.HasInvalidNodeType())
            {
                ThrowNoPermissionException("No node types.");
            }
        }
        private void ProcessOrphanedODIds(ExplorerFilterOptionV2 filterOption)
        {
            filterOption.ExceptSCIds = RMRemoteNodeDao.GetOrphanedODIds();
        }

        private void ProcessNodeId(ExplorerFilterOptionV2 filterOption)
        {
            if (!string.IsNullOrEmpty(filterOption.NodeId))
            {
                filterOption.NodeId = GetRealNodeId(filterOption.NodeId);
                //var connObj = FSConnDao.GetConnectionById(new Guid(filterOption.NodeId));
                //if (connObj != null)
                //{
                //    filterOption.NodeId = connObj.UNCPath.TrimEnd('\\').ToLowerInvariant().ToMd5().ToString();
                //}
            }
        }

        private string GetRealNodeId(string treeNodeId)
        {
            var connObj = FSConnDao.GetConnectionById(new Guid(treeNodeId));
            return connObj != null ? connObj.UNCPath.TrimEnd('\\').ToLowerInvariant().ToMd5().ToString() : treeNodeId;
        }

        /// <summary>
        /// only valid for physical records
        /// </summary>
        /// <param name="filterOption"></param>
        private async System.Threading.Tasks.Task ProcessPermissionIdsAsync(ExplorerFilterOptionV2 filterOption)
        {
            if (filterOption.SourceFlags != null && filterOption.SourceFlags.Contains(SourceFlag.Physical) && await IsPhysicalEndUserAsync())
            {
                filterOption.PersmissionScopes = await GetPermissionConditionAsync();
                if (filterOption.PersmissionScopes != null) filterOption.PersmissionScopes.Add(0); // add default permission
            }
        }

        public async Task<bool> IsPhysicalEndUserAsync()
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
            return !isAdmin && (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalEndUser));
        }

        public async Task<List<int>> GetPermissionConditionAsync()
        {
            var isEnduser = await IsPhysicalEndUserAsync();
            if (!isEnduser)
            {
                return null;//管理员不做限制
            }
            else
            {
                var permissionIds = new List<int>();
                try
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                    permissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                    return permissionIds;
                }
                catch (Exception ex)
                {

                    logger.Warn($"An error occured when GetPermissonCondition, message:{ex.ToString()}");
                    return permissionIds;
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessContainerIdsAsync(ExplorerFilterOptionV2 filterOption)
        {
            var checkSourceFlags = SourceFlagHelper.GetDefaultContainerIdSource();
            if (filterOption.SourceFlags != null && filterOption.SourceFlags.Exists(o => checkSourceFlags.Contains(o)))
            {
                var permissionCheckResult = await SecurityTrimmingHelper.CheckAsync(filterOption.SourceFlags);
                if (permissionCheckResult.NeedCheck)
                {
                    var containerIds = permissionCheckResult.GetContainerIds();
                    if (containerIds.Count == 0)
                    {
                        filterOption.SourceFlags.RemoveAll(o => checkSourceFlags.Contains(o)); //if no container id, need to remove the source
                        logger.Warn("No containers found");
                        if (filterOption.SourceFlags.Count == 0) // if no other source
                        {
                            ThrowNoPermissionException("No source flags remains after checking containers");
                        }
                    }
                    else
                    {
                        filterOption.ContainerIds = containerIds;
                    }
                }
            }
        }

        private void ThrowNoPermissionException(string message)
        {
            throw new ExplorerQueryNoPermissionException(message);
        }
    }
}
