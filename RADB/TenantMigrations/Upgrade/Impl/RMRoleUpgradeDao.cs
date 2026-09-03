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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMRoleUpgradeDao : BaseDao<RMRole>, IDbUpgradeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMRoleUpgradeDao));
        //upgrade permission masks for build in admin group && enduser group
        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                var adminRole = context.Role.Where(r => r.RoleName == "Application Admin" && r.RoleId == 1).FirstOrDefault();
                adminRole.PermissionMasks = (long)RMPermissionMasks.AccessAll;
                adminRole.PermissionExtensionMasks = (long)RMPermissionExtensionMasks.AccessAll;
                adminRole.SOPermissionMasks = (long)RMSOPermissionMasks.AccessAll;
                adminRole.DiscoveryPermissionMasks = (long)RMDiscoveryPermissionMasks.AccessAll;
                adminRole.SalesforceDiscoveryPermissionMasks = (long)RMDiscoveryPermissionMasks.AccessAll;
                adminRole.GoogleROTDiscoveryPermissionMasks = (long)RMDiscoveryPermissionMasks.AccessAll;
                adminRole.FSDiscoveryPermissionMasks = (long)RMDiscoveryFileSystemPermissionMask.AccessAll;
                await this.UpdateAsync(adminRole);
                var standandRole = context.Role.Where(r => r.RoleName == "Standard User" && r.RoleId == 2).FirstOrDefault();
                var holdRole = context.Role.Where(r => r.RoleName == "Hold Manager" && r.RoleType == RMRoleType.ManageHoldUser).FirstOrDefault();
                if (standandRole.PermissionMasks == 0)
                {
                    standandRole.PermissionMasks = (long)(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.CommonModuleAccess);
                    await this.UpdateAsync(standandRole);
                }
                if (holdRole.PermissionMasks == 0)
                {
                    holdRole.PermissionMasks = (long)PermissionWrappers.HoldUser;
                    await this.UpdateAsync(holdRole);
                }

                #region 升级SubPermission1
                string upgradeSubPermission1Sql = @"Update {0}.RMRoles set SubPermission1 = @subPermission1, UpgradeType = @nextUpgradeType Where (PermissionMasks & @phyEndUserMasks) = @phyEndUserMasks 
                    and (PermissionMasks & @phyAdminMasks) != @phyAdminMasks and IsRemoved = 0 and SubPermission1 = @subPermission1Condition and UpgradeType = @upgradeTypeCondition";     
                context.Database.ExecuteSqlCommand(
                     string.Format(upgradeSubPermission1Sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)),
                     new SqlParameter("subPermission1", RMSubPermissionMasks.PhysicalAccessControl | RMSubPermissionMasks.PhysicalBoxCreationRequest | RMSubPermissionMasks.PhysicalFolderCreationRequest | RMSubPermissionMasks.PhysicalFolderLoanRequest | RMSubPermissionMasks.PhysicalMoveRequest),
                     new SqlParameter("phyEndUserMasks", RMPermissionMasks.PhysicalEndUser),
                     new SqlParameter("phyAdminMasks", RMPermissionMasks.PhysicalAdmin),
                     new SqlParameter("subPermission1Condition", RMSubPermissionMasks.None),
                     new SqlParameter("upgradeTypeCondition", RMRoleUpgradeType.None),
                     new SqlParameter("nextUpgradeType", RMRoleUpgradeType.UpgradePhysicalAction));
                #endregion

                context.SaveChanges();

                #region 包含SPO和OneDrive数据源的Custom Security Group, 升级Role表中的SO Permission

                //string queryRoleIdsSql = "select distinct RoleId from {0}.RMRoles where IsRemoved = 0 and RoleType = 2 and (PermissionMasks & @spoEndUserMasks = @spoEndUserMasks or PermissionMasks & @oneDriveEndUserMasks) = @oneDriveEndUserMasks)";
                //var roleIds = context.Database.SqlQuery<int>(
                //    string.Format(queryRoleIdsSql, context.SchemaName),
                //    new SqlParameter("spoEndUserMasks", RMPermissionMasks.SPOEnduser),
                //    new SqlParameter("oneDriveEndUserMasks", RMPermissionMasks.OneDriveEnduser)
                //    );

                //if (roleIds != null && roleIds.Any())
                //{
                //    logger.Info($"There are roles that need to upgrade so permissions, count: {roleIds?.Count()}");
                //    var upgradeRoles = context.Role.Where(r => roleIds.Contains(r.RoleId));
                //    foreach (var role in upgradeRoles)
                //    {
                //        var soDelegateAdminPermission = RMSOPermissionMasks.None;
                //        var recordsPermission = (RMSOPermissionMasks)role.PermissionMasks;
                //        var containsSPOSource = recordsPermission.UserHasThisPermission(RMSOPermissionMasks.SPOEnduser);
                //        var containsOneDriveSource = recordsPermission.UserHasThisPermission(RMSOPermissionMasks.OneDriveEnduser);

                //        if (containsSPOSource)
                //        {
                //            soDelegateAdminPermission |= RMSOPermissionMasks.SPOEnduser;
                //        }

                //        if (containsOneDriveSource)
                //        {
                //            soDelegateAdminPermission |= RMSOPermissionMasks.OneDriveEnduser;
                //        }

                //        if (soDelegateAdminPermission != RMSOPermissionMasks.None)
                //        {
                //            soDelegateAdminPermission |= RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.JobMonitorEnduser 
                //                | RMSOPermissionMasks.RuleManagementEnduser | RMSOPermissionMasks.CommonModuleAccess;
                //            role.SOPermissionMasks = (long)soDelegateAdminPermission;
                //        }
                //    }
                //    context.SaveChanges();
                //    logger.Info("Finished to upgrade so permissions.");
                //}
                
                #endregion
            }
            catch (Exception e)
            {
                logger.Error($"Upgrade role failed {e.ToString()}");
            }
        }
    }
}
