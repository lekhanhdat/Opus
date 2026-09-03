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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl
{
    public class RMTenantDiscoveryDBInfoDao : IRMTenantDiscoveryDBInfoDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTenantDiscoveryDBInfoDao));

        public async Task<List<RMTenantDiscoveryDBInfo>> GetAllAvaliableAsync()
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            var query = from discoveryDbInfo in context.TenantDiscoveryDBInfoes
                        join tenantDBInfo in context.TenantInfo
                        on discoveryDbInfo.Id equals tenantDBInfo.Id
                        where discoveryDbInfo.IsEnabled && !discoveryDbInfo.IsRemoved
                        select discoveryDbInfo;
            return await query.ToListAsync();
        }

        public async Task<bool> IsInitTenantDiscoveryDBInfoAsync()
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            return await context.TenantDiscoveryDBInfoes.AnyAsync(item => item.Id == TenantLocalValue.LogonGroupId && item.IsEnabled && !item.IsRemoved);
        }

        public Task<(bool has, RMTenantDiscoveryDBInfo info)> TryGetTenantDiscoveryDBInfoAsync()
        {
            return TryGetTenantDiscoveryDBInfoAsync(TenantLocalValue.LogonGroupId);
        }

        public async Task<(bool has, RMTenantDiscoveryDBInfo info)> TryGetTenantDiscoveryDBInfoAsync(string tenantId)
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            var res = await context.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == tenantId && item.IsEnabled && !item.IsRemoved);
            return (res != null, res);
        }

        public async Task<bool> TryRemoveTenantDiscoveryDBInfoAsync(string tenantId)
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            try
            {
                if (context.TenantDiscoveryDBInfoes.Any(item => item.Id.Equals(tenantId, StringComparison.OrdinalIgnoreCase)))
                {
                    var entity = await context.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == tenantId);
                    await RMDiscoveryDBManager.DeleteDataBaseAsync(entity.DatabaseName);
                    context.TenantDiscoveryDBInfoes.Remove(entity);
                    logger.Info($"Success to remove tenant discovery db info {tenantId}");
                    return (await context.SaveChangesAsync()) > 0;
                }
                return false;
            }
            catch(Exception e)
            {
                logger.Error($"Remove tenant discovery db info failed, tenant id : {tenantId}, error {e}");
                throw;
            }
        }

        public async Task<(bool has, bool isUesd)> TryGetIsUseFailoverGroup(string tenantId)
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            var result = await context.TenantDiscoveryDBInfoes.FirstOrDefaultAsync(item => item.Id == tenantId && item.IsEnabled && !item.IsRemoved);
            if (result == null)
            {
                return (false, false);
            }
            return (true, result.UseFailoverGroup);
        }

        public async Task<bool> TryUpdateTenantDiscoveryDBInfoAsync(List<RMTenantDiscoveryDBInfo> allDiscoveryDBInfo)
        {
            try
            {
                using var context = RMDBContextManager.GetSystemDBContext();
                context.TenantDiscoveryDBInfoes.AddOrUpdate([.. allDiscoveryDBInfo]);
                await context.SaveChangesAsync();
                return true;
            }
            catch(Exception e)
            {
                logger.Error($"Update discovery db info failed, error {e}");
                throw;
            }
        }

        public async Task<string> GetEmailByTenantAsync(string tenantId)
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            return (await context.TenantInfo.FirstOrDefaultAsync(tenantInfo => tenantId == tenantInfo.Id))?.RegisterEmail;
        }
    }
}
