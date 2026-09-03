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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.SecurityTrimming
{
    /// <summary>
    /// 使用这个类需要注意
    /// CustomerId依赖于TenantLocalValue.LogonGroupId
    /// UserId依赖于TenantLocalValue.LogonUserId
    /// 需要保证这两个值的正确性.
    /// </summary>
    public interface IRMSecurityTrimmingHelper
    {
        bool EnableCache { get; }
        string CustomerId { get; }
        string UserId { get; }
        /// <summary>
        /// 在当前实例中生效, 不使用Cache
        /// </summary>
        /// <param name="value"></param>
        void DisableCache();
        Task RemovePermissionCacheAsync(List<string> fields = null);
        Task<RMSecurityTrimmingCheckResult> CheckAsync(IList<SourceFlag> flags, bool isGlobalSecurityTrimmingEnabled = true);

        Task<List<SourceFlag>> GetAllAvailableSourceFlagsFromDbAsync();

        Task<List<RMCustomizeConnectorContentSource>> GetAllAvailableDataSourceFromDbAsync();

        Task<IList<SourceFlag>> GetAvailableDataSourceAsync();
        Task<T> GetUserPermissionAsync<T>(bool checkLicense = true) where T : struct;
        Task<bool> DoesUserHasThisPermissionAsync<T>(T permissionMask, PermissionJoinType joinType = PermissionJoinType.And) where T : struct;
        Task<bool> EqualsThisPermission<T>(T mask) where T : struct;
        RMSecurityTrimmingCheckResult GetContentScope(IList<SourceFlag> flags);
        RMSecurityTrimmingCheckResult GetTermScope();
        RMSecurityTrimmingCheckResult GetTermScopeByContentScope(string contentScopeId, DataScope scope);
        List<int> GetSecurityGroupsByContentScope(List<string> containerIds, SourceFlag sourceFlag);
        List<RMSecurityGroup> GetSecurityGroupsByContentScope(List<RMSecurityGroup> securityGroups, SourceFlag sourceFlag, bool excludeBuiltIn);
        List<RMSecurityGroup> TrimEndUserAndFunctionSecurityGroups(List<RMSecurityGroup> securityGroups);
        Task<List<Guid>> GetRuleScopeAsync();
        List<Guid> GetRuleScopeByTermId(string customerId, string userId, string termId);
        List<Guid> GetRuleScopeByRuleId(string customerId, string userId, Guid rule);
        Task<SecurityTermPermissionDto> GetSecurityTermDtoAsync();
        List<Guid> GetRuleScopeBySecurityGroupIds(List<int> securityGroupIds);
        Task<FunctionSubPermission> GetUserRestoreCenterFunctionPermissionAsync();
        Task<List<bool>> GetUserGroupsIsNewGroups();
        Task<(List<Guid> physicalLocationPermission, bool isAdmin)> GetPhysicalLocationPermissionAsync();
    }
}
