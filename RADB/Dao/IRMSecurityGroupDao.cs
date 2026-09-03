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
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMSecurityGroupDao : IBaseDao<RMSecurityGroup>
    {
        RMSecurityGroup CreateSecurityGroup(SecurityGroupDto group);
        List<SimpleSecurityGroupDto> LoadAllGroup();
        Task<RMSecurityGroup> EditSecurityGroupAsync(SecurityGroupDto group);
        Task<RMSecurityGroup> EditBuiltInEndUserGroupAsync(SecurityGroupDto dto);
        RMSecurityGroup EditBuiltInReviewUserGroup(SecurityGroupDto dto);
        RMSecurityGroup EditBuiltInHoldManagerGroup(SecurityGroupDto dto);
        void DeleteSecurityGroup(int groupId);
        SecurityGroupDto GetGroup(int id);
        List<RMSecurityGroup> GetAllGroup();
        List<RMSecurityGroup> GetAllGroupById(List<int> groupIds);
        List<string> GetGroupNames(List<int> ids);

        /// <summary>
        /// 获取User所在的Security Group Name集合
        /// </summary>
        /// <param name="userAndGroupIds">User和所在365Group的Id集合</param>
        /// <returns></returns>
        List<string> GetGroupNames(List<string> userAndGroupIds);
        List<RMSecurityGroup> GetSecurityGroups(List<string> userAndGroupIds);
        SecurityUserPermissionsDto GetUserScopePermissions(List<string> userAndGroupIds);
        List<Guid> GetScopeLocationPermission(SourceFlag sourceFlag);
        bool IsSupperAdminUser(List<string> userAndGroupIds);
        (TermPermissionMethod, Dictionary<Guid, List<Guid>>) GetTermGroupIdUserScopePermission(string userOrGroupId);

        SecurityTermPermissionDto GetSecurityTermObjInfo(QuerySecurityTermObjDto dto);
        Dictionary<SecurityTermLevel, List<Guid>> GetSecurityTermObjIds(int securityGroupId);
        bool DoesUserHasPermisionToTerm(List<Guid> termObjIds, QuerySecurityTermObjDto dto);
        SecurityTermPermissionDto GetAllSecurityTerm(List<string> userAndGroupIds);

        List<RMSecurityGroupTermMapping> GetMappedTermByOtherGroups(int securityGroupId = 0);
        List<RMSecurityGroupRuleMapping> GetMappedRuleByOtherGroups(int securityGroupId = 0);
        List<RMSecurityGroupTermMapping> GetMappedTermByGroup(int securityGroupId);
        void RemoveTermMappings(List<RMSecurityGroupTermMapping> mappings);
        void RemoveRuleMappings(List<RMSecurityGroupRuleMapping> mappings);
        List<Guid> GetSecurityGroupRuleContainers(int termId, out int securityGroupId);
        List<Guid> GetSecurityGroupRuleContainers(Guid ruleId);
        List<Guid> GetSecurityGroupRuleContainers(List<string> userAndGroupIds);
        List<RMSecurityGroup> GetSecurityGroupsBySource(List<RMSecurityGroup> securityGroups, SourceFlag sourceFlag);
        List<RMSecurityGroup> TrimEndUserAndFunctionSecurityGroups(List<RMSecurityGroup> securityGroups);
        List<Guid> GetSecurityGroupRuleContainerIds(List<int> securityGroupIds);
        Task UpdateSecurityGroupPermissionAsync(int groupId, long permissionMasks, long subPermissionMasks, long permissionExtensionMasks, long soPermissionMasks, long reportPermissionMasks);
        bool IsBuiltInReviewUserGroup(int groupId);
        bool IsBuiltInHoldManagerGroup(int groupId);
        int GetBuitInReviewUserGroupId();

        int LoadGroupIdHavePhysicalRecordManagerPermission();
    }
}
