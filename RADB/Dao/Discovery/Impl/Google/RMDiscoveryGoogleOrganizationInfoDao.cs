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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleOrganizationInfoDao : IRMDiscoveryGoogleOrganizationInfoDao
    {
        public async Task<List<RMDiscoveryGoogleOrganizationInfo>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleOrganizationInfoes.ToListAsync();
        }

        public async Task AddOrUpdateAsync(params RMDiscoveryGoogleOrganizationInfo[] infoes)
        {
            if (!infoes.Any()) return;
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.GoogleOrganizationInfoes.AddOrUpdate(infoes);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(params RMDiscoveryGoogleOrganizationInfo[] infoes)
        {
            if (!infoes.Any()) return;
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            foreach (var info in infoes)
            {
                efContext.GoogleOrganizationInfoes.Attach(info);
                efContext.GoogleOrganizationInfoes.Remove(info);
            }
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryGoogleOrganizationInfo>> GetAllDiscoveryContainersAsync(string organizeId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(organizeId);
            return await efContext.GoogleOrganizationInfoes.ToListAsync();
        }
    }
}
