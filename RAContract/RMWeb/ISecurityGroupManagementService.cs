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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface ISecurityGroupManagementService
    {
        //load all group(group name, scope, Description)
        //get group info(all info)
        //get containers info by groupId and scope （Container id, name）
        ////以下需要添加Audit,并适当添加后台验证
        //create group
        //edit group
        //delete group(不要删除build in group)
        //config physical permission(Records Manager or End User)
        Task<List<SimpleSecurityGroupDto>> GetGroupsAsync();
        Task<SecurityGroupDto> GetGroupAsync(int id);
        SecurityGroupDto GetSimpleGroup(int id);
        Task<RAReturnMessage> CreateGroupAsync(SecurityGroupDto group);
        Task<RAReturnMessage> EditGroupAsync(SecurityGroupDto group);
        Task<RAReturnMessage> ValidateGroupTermAndRuleAsync(ValidateSecurityGroupDto vGroup);
        Task<bool> DeleteGroupAsync(int id);
        List<SecurityContainerDto> GetContianers(int id);
        Task<RAReturnMessage> SyncADUsersAsync(List<AOSUserDto> users);
        Task<List<SecurityContainerDto>> GetContainersAsync(SourceFlag source, bool isExcludeAssigned = false);
        Task<SecurityUserPermissionsDto> GetUserScopePermissionsAsync(string userId, bool isFromGControl = false);
        Task<SecurityTermPermissionDto> GetSecurityTermObjInfoAsync(QuerySecurityTermObjDto dto);
        Task<bool> DoesUserHasPermisionToTermAsync(string userId, SecurityTermLevel level, List<Guid > termObjIds);
        Task<bool> DoesUserHasPermisionToTermAsync(string userId, SecurityTermLevel level, List<Guid > termObjIds, FilterTermObjOption filterOption);
        SecurityTermInfo GetSecurityTermRootNode();
        SecurityRuleInfo GetSecurityRuleRootNode();
        List<Guid> GetAllAssignContainerIds();
        Task<List<AOSUserDto>> SearchUsersByPermissionScopeAsync(string keyword);
        bool HasManageHoldsPermission(SecurityUserPermissionsDto permissions);
    }
}
