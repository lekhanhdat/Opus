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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMRoleAssignmentUpgradeDao
    {
        public void Upgrade(Core.RMDbContext context)
        {
            #region init permission
            var permission = context.Permission.AsQueryable().FirstOrDefault();

            if (null == permission)
            {
                var permissionList = new List<RMPermission>()
                {
                    new RMPermission()
                    {
                        Type = Contract.RoleAssignments.RMPermissionType.Reviewer,
                        Description = "RDM-ManualApprovalReview",
                        Modified = DateTime.UtcNow
                    },new RMPermission()
                    {
                        Type = Contract.RoleAssignments.RMPermissionType.All,
                        Description = "All",
                        Modified = DateTime.UtcNow
                    },
                    new RMPermission()
                    {
                        Type = Contract.RoleAssignments.RMPermissionType.Common,
                        Description = "Account-CheckSession",
                        Modified = DateTime.UtcNow
                    },
                    new RMPermission()
                    {
                        Type = Contract.RoleAssignments.RMPermissionType.Common,
                        Description = "Account-Logout",
                        Modified = DateTime.UtcNow
                    },
                };
                context.Permission.AddRange(permissionList);
                context.SaveChanges();
            }
            else
            {
                //Upgrade
            }

            #endregion

            #region init role
            var role = context.Role.AsQueryable().FirstOrDefault();

            if (null == role)
            {
                var roleList = new List<RMRole>()
                {
                    new RMRole()
                    {
                        RoleName = "Application Admin",
                        RoleType = Contract.RoleAssignments.RMRoleType.ApplicationAdmin,
                        Modified = DateTime.UtcNow
                    },new RMRole()
                    {
                        RoleName = "Standard User",
                        RoleType = Contract.RoleAssignments.RMRoleType.StandardUser,
                        Modified = DateTime.UtcNow
                    }
                    ,new RMRole()
                    {
                        RoleName = RecordsConstants.BuiltIn_ReviewRole_Name,
                        RoleType = Contract.RoleAssignments.RMRoleType.ReviewUser,
                        Modified = DateTime.UtcNow,
						PermissionMasks = (long)PermissionWrappers.ReviewUser,
                    } ,
                    new RMRole()
                    {
                        RoleName = RecordsConstants.BuiltIn_HoldRole_Name,
                        RoleType = RMRoleType.ManageHoldUser,
                        Modified = DateTime.UtcNow,
                        PermissionMasks = (long)PermissionWrappers.HoldUser,
                    }
                };
                context.Role.AddRange(roleList);
                context.SaveChanges();
            }
            else
            {
                //Upgrade
            }
            #endregion

            #region init role link permission

            var lnkRP = context.LnkRolePermission.AsQueryable().ToList();
            if (lnkRP.Count == 0)
            {
                var permissions = context.Permission.AsQueryable().ToList();
                var roles = context.Role.AsQueryable().ToList();
                var lnkRolePermission = new List<RMLnkRolePermission>();
                foreach (var r in roles)
                {
                    switch (r.RoleType)
                    {
                        case Contract.RoleAssignments.RMRoleType.ApplicationAdmin:
                            var allP = permissions.Where(p => p.Type == Contract.RoleAssignments.RMPermissionType.All).FirstOrDefault();
                            if (allP != null)
                            {
                                lnkRolePermission.Add(new RMLnkRolePermission() { PermissionId = allP.PermissionId, RoleId = r.RoleId });
                            }
                            continue;
                        case Contract.RoleAssignments.RMRoleType.StandardUser:
                            var reviewP = permissions.Where(p => p.Type == Contract.RoleAssignments.RMPermissionType.Reviewer).FirstOrDefault();
                            if (reviewP != null)
                            {
                                lnkRolePermission.Add(new RMLnkRolePermission() { PermissionId = reviewP.PermissionId, RoleId = r.RoleId });
                            }
                            var cps = permissions.Where(p => p.Type == Contract.RoleAssignments.RMPermissionType.Common);
                            foreach (var p in cps)
                            {
                                lnkRolePermission.Add(new RMLnkRolePermission() { PermissionId = p.PermissionId, RoleId = r.RoleId });
                            }
                            continue;
                    }

                }
                context.LnkRolePermission.AddRange(lnkRolePermission);
                context.SaveChanges();

            }
            

            #endregion
        }

        public async Task UpgradeManagerRole(RMDbContext context)
        {
            var role = context.Role.AsQueryable().FirstOrDefault(r => r.RoleType == RMRoleType.ManageHoldUser);
            if (role == null)
            {
                role = new RMRole()
                {
                    RoleName = RecordsConstants.BuiltIn_HoldRole_Name,
                    RoleType = RMRoleType.ManageHoldUser,
                    Modified = DateTime.UtcNow,
                    PermissionMasks = (long)PermissionWrappers.HoldUser,
                };
                context.Role.Add(role);
                context.SaveChanges();
            }
        }

        #region 升级db for Phycisal
        /// <summary>
        /// 升级db for Phycisal:返回值true-升级,false-未升级
        /// </summary>
        /// <returns>是否升级,true-升级,false-未升级,已经升级过了</returns>
        public static bool UpgradeDBForPhysical()
        {
            bool isUpgrade = false;
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                List<string> lstPermission = new List<string>() { "Root-Home", "PRM-RecordsExplorer", "PRM-MyRequest", "PRM-GlobalSearch" };
                var needAddPermissions = new List<string>();
                var existPermissions = context.Permission.Where(o => lstPermission.Contains(o.Description)).ToList();
                if (existPermissions.Count > 0)
                {
                    var existPermissionDescs = existPermissions.Select(o => o.Description).ToList();
                    needAddPermissions = lstPermission.Except(existPermissionDescs).ToList();
                }
                else {
                    needAddPermissions = lstPermission;
                }
                if (needAddPermissions.Count > 0)
                {
                    using (var tran = new TransactionScope())
                    {
                        needAddPermissions.ForEach(p =>
                        {
                            RMPermission per = new RMPermission()
                            {
                                Type = Contract.RoleAssignments.RMPermissionType.Reviewer,
                                Description = p,
                                Modified = DateTime.UtcNow
                            };
                            RMPermission perAdd = context.Permission.Add(per);
                            context.SaveChanges();

                            RMLnkRolePermission rolePermission = new RMLnkRolePermission()
                            {
                                RoleId = 2,
                                PermissionId = per.PermissionId
                            };
                            context.LnkRolePermission.Add(rolePermission);
                            context.SaveChanges();
                        });

                        tran.Complete();
                        isUpgrade = true;
                    }
                }
            }
            return isUpgrade;
        }
        #endregion

    }
}
