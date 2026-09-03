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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMSecurityGroupUpgradeDao : BaseDao<RMSecurityGroup>, IDbUpgradeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMSecurityGroupUpgradeDao));
        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                //TO DO Confirm only have 2 role??
                if (context.RMSecurityGroup.Count() == 0)
                {
                    #region  create build-in group
                    logger.Info("start init security group settings");
                    var adminGroup = context.RMSecurityGroup.Add(new RMSecurityGroup()
                    {
                        Name = "RM_CP_AM_DefaultGroup_Admin_Title",
                        Description = "RM_CP_AM_DefaultGroup_Admin_Desc",
                        RoleId = 1,
                        ModifiedTime = DateTime.MaxValue.Ticks
                    });
                    var standardGroup = context.RMSecurityGroup.Add(new RMSecurityGroup()
                    {
                        Name = "RM_CP_AM_DefaultGroup_EndUser_Title",
                        Description = "RM_CP_AM_DefaultGroup_EndUser_Desc",
                        RoleId = 2,
                        ModifiedTime = DateTime.MaxValue.Ticks
                    });
                    var reviewerGroup = context.RMSecurityGroup.Add(new RMSecurityGroup()
                    {
                        Name = "RM_CP_AM_DefaultGroup_ReviewUser_Title",
                        Description = "RM_CP_AM_DefaultGroup_ReviewUser_Desc",
                        RoleId = 3,
                        ModifiedTime = DateTime.MaxValue.Ticks
                    });
                    var holdGroup = context.RMSecurityGroup.Add(new RMSecurityGroup()
                    {
                        Name = "RM_CP_AM_DefaultGroup_Hold_Title",
                        Description = "RM_CP_AM_DefaultGroup_Hold_Desc",
                        RoleId = 4,
                        ModifiedTime = DateTime.MaxValue.Ticks
                    });
                    context.SaveChanges();
                    #endregion
                    #region create build-in group memberships
                    var standardUsers = context.LnkUserRole.Where(u => u.RoleId == 2).Select(u => u.UserId).ToList();
                    var adminUsers = context.LnkUserRole.Where(u => u.RoleId == 1).Select(u => u.UserId).ToList();
                    List<RMSecurityGroupMembership> memberships = new List<RMSecurityGroupMembership>();
                    foreach (var standardUser in standardUsers)
                    {
                        memberships.Add(new RMSecurityGroupMembership()
                        {
                            GroupId = standardGroup.Id,
                            UserId = standardUser
                        });
                    }
                    foreach (var adminUser in adminUsers)
                    {
                        memberships.Add(new RMSecurityGroupMembership()
                        {
                            GroupId = adminGroup.Id,
                            UserId = adminUser
                        });
                    }
                    var holdManagerId = context.RMSecurityGroup.Where(u => u.RoleId == 4).Select(u => u.Id).FirstOrDefault();
                    var holdUsers = context.LnkUserRole.Where(u => u.RoleId == 4).Select(u => u.UserId).ToList();
                    foreach (var holdUser in holdUsers)
                    {
                        memberships.Add(new RMSecurityGroupMembership()
                        {
                            GroupId = holdManagerId,
                            UserId = holdUser
                        });
                    }
                    context.RMSecurityGroupMembership.AddRange(memberships);
                    context.SaveChanges();
                    #endregion
                }
                else
                {
                    logger.Info("Default Group exist ");
                }
            }
            catch (Exception e)
            {
                logger.Error($"*** upgrade security group failed ,{e.ToString()}");
            }
        }

        public async Task UpgradeManagerHoldData(RMDbContext context)
        {
            try
            {
                var role = context.Role.FirstOrDefault(r => r.RoleType == Contract.RoleAssignments.RMRoleType.ManageHoldUser);
                if (role != null)
                {
                    logger.Info("Start upgrade ManagerHold Security group");
                    await UpgradeManagerHoldSecurityGroup(context, role.RoleId);
                    logger.Info("Start upgrade ManagerHold User role Security group");
                    await UpgradeManagerHoldUserRoleRelatedData(context, role.RoleId);
                }
                else
                {
                    logger.Info("Role Manage Hold User does not exist");
                }
                
            }
            catch (Exception e)
            {
                logger.Error($"*** upgrade hold security group failed ,{e.ToString()}");
            }
        }
        private Task UpgradeManagerHoldSecurityGroup(RMDbContext context, int roleId)
        {
            var holdGroup = context.RMSecurityGroup.FirstOrDefault(g => g.RoleId == roleId);
            if (holdGroup == null)
            {
                logger.Info("start init hold security group settings");
                var holdGroupEntity = context.RMSecurityGroup.Add(new RMSecurityGroup()
                {
                    Name = "RM_CP_AM_DefaultGroup_Hold_Title",
                    Description = "RM_CP_AM_DefaultGroup_Hold_Desc",
                    RoleId = roleId,
                    ModifiedTime = DateTime.MaxValue.Ticks
                });
                context.SaveChanges();
            }
            else
            {
                logger.Info("Hold Group exist ");
            }

            return Task.CompletedTask;
        }

        private Task UpgradeManagerHoldUserRoleRelatedData(RMDbContext context, int roleId)
        {
            var holdGroup = context.RMSecurityGroup.FirstOrDefault(g => g.RoleId == roleId);
            if (holdGroup != null)
            {
                logger.Info("start add membership security group");

                var holdUsers = context.LnkUserRole.Where(u => u.RoleId == roleId).Select(u => u.UserId).ToList();
                List<RMSecurityGroupMembership> memberships = new List<RMSecurityGroupMembership>();
                foreach (var holdUser in holdUsers)
                {
                    memberships.Add(new RMSecurityGroupMembership()
                    {
                        GroupId = holdGroup.Id,
                        UserId = holdUser
                    });
                }
                context.RMSecurityGroupMembership.AddRange(memberships);
                context.SaveChanges();
            }
            else
            {
                logger.Info("Hold Group does not exist ");

            }
            return Task.CompletedTask;
        }
    }
}
