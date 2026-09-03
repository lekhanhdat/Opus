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
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RoleDao : BaseDao<RMRole>, IRoleDao
    {
        public List<RMRole> GetUserRoles(string userId)
        {
            using (var ctx = this.GetNewContext())
            {
                var roleIds = ctx.LnkUserRole.AsQueryable().Where(t => t.UserId.Equals(userId)).Select(r => r.RoleId).ToList();

                return ctx.Role.Where(p => roleIds.Contains(p.RoleId)).ToList();
            }
        }

        public RMRole GetRoleByAccountType(Contract.RoleAssignments.RMRoleType type)
        {
            using (var ctx = this.GetNewContext())
            {
                var role = ctx.Role.AsQueryable().Where(t => t.RoleType == type).FirstOrDefault();

                return role;
            }
        }

        public async Task UpdateRoleAsync(int roleId, long permissionMasks, long subPermissionMasks, long permissionExtensionMasks, long soPermissionMasks, long reportPermissionMasks)
        {
            using (var context = GetNewContext())
            {
                var roleObj = context.Role.Where(t => t.RoleId == roleId).FirstOrDefault();
                roleObj.PermissionMasks = permissionMasks;
                roleObj.SubPermission1 = subPermissionMasks;
                roleObj.PermissionExtensionMasks = permissionExtensionMasks;
                roleObj.SOPermissionMasks = soPermissionMasks;
                roleObj.UpgradeType = RMRoleUpgradeType.UpgradePhysicalAction;
                roleObj.ReportingPermission = reportPermissionMasks;
                roleObj.IsNewGroup = true;
                await this.UpdateAsync(roleObj);
            }
        }

        public void UpdateRoleSubPermission(int roleId, long subPermissionMasks)
        {
            using (var context = GetNewContext())
            {
                var roleObj = context.Role.Where(t => t.RoleId == roleId).FirstOrDefault();
                roleObj.SubPermission1 = subPermissionMasks;
                roleObj.UpgradeType = RMRoleUpgradeType.UpgradePhysicalAction;
                this.ApplyCurrentValues(context, roleObj);
            }
        }
    }
}
