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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.User
{
    public class UserService
    {
        private RALogger logger = RALogger.GetInstance(typeof(UserService));

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private ILnkUserGroupDao LnkUserGroupDao => PlatformWindsorManager.GetService<ILnkUserGroupDao>();

        private IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        public async Task<List<string>> GetUserAndGroupUserIdsAsync(string userId)
        {
            try
            {
                var userAndGroupIds = new List<string>
                {
                    userId
                };
                var groupUniqueIds = await LnkUserGroupDao.GetAllGroupIdsAsync(userId);
                if (groupUniqueIds.Count > 0)
                {
                    userAndGroupIds.AddRange(groupUniqueIds);
                }
                var accounts = await AccountDao.GetUserByUserIdsAsync(userAndGroupIds);
                return accounts.Select(o => o.UserId).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when GetAccountAndGroupIds, message:{ex.ToString()}");
                return null;
            }
        }


        public async Task<(List<Guid> physicalLocationPermission,bool isAdmin)> GetPhysicalLocationPermissionAsync() 
        {
            try
            {
                List<Guid> physicalLocationPermission = null;
                var userIds = await GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var userPermission = SecurityGroupDao.GetUserScopePermissions(userIds);
                if (!userPermission.IsAdmin)
                {
                    logger.Info("start load Physical permission location ids");
                    var phyPermission = userPermission.ScopePermissionInfo?.Where(_ => _.DataSourceType == SourceFlag.Physical).FirstOrDefault() ?? new();
                    var locationScopeIds = phyPermission?.ScopeIds ?? new List<Guid>();
                    var physicalBottomPermissionIds = LocationDao.LoadAllLocationBottomIdUnderTopLocation(locationScopeIds);
                    physicalLocationPermission = physicalBottomPermissionIds;
                }
                return (physicalLocationPermission, userPermission.IsAdmin);
            }
            catch (Exception e)
            {
                logger.Error($"InitUserPermission have error: {e}");
                return (null, false);
            }
        }
    }
}
