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
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365TenantDao : IRMDiscoveryOffice365TenantDao
    {

        public async Task<List<RMDiscoveryOffice365TenantInfo>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365TenantInfoes.ToListAsync();
        }

        public async Task AddOrUpdateAsync(params RMDiscoveryOffice365TenantInfo[] infoes)
        {
            if (!infoes.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.Office365TenantInfoes.AddOrUpdate(infoes);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(params RMDiscoveryOffice365TenantInfo[] infoes)
        {
            if (!infoes.Any())
            {
                return;
            }

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            foreach (var info in infoes)
            {
                efContext.Office365TenantInfoes.Attach(info);
                efContext.Office365TenantInfoes.Remove(info);
            }
            await efContext.SaveChangesAsync();
        }

        public async Task<bool> HasAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365TenantInfoes.AnyAsync(tenant => tenant.UniqueId == o365TenantId);
        }
    }
}
