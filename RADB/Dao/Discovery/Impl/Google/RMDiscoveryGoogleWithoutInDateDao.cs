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
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleWithoutInDateDao : IRMDiscoveryGoogleWithoutInDateDao
    {
        public async Task InitBuildInDataAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var has = await efContext.GoogleWithoutInDateList.AnyAsync();
            if (has)
            {
                return;
            }

            var dataList = new List<RMDiscoveryGoogleWithoutInDate>
            {
                new()
                {
                    Unit = 1,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 1,
                },
                new()
                {
                    Unit = 3,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 2,
                },
                new()
                {
                    Unit = 5,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 3,
                },
                new()
                {
                    Unit = 10,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 4,
                },
            };
            efContext.GoogleWithoutInDateList.AddRange(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryGoogleWithoutInDate>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleWithoutInDateList.ToListAsync();
        }

        public async Task<RMDiscoveryGoogleWithoutInDate> GetAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleWithoutInDateList.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<int> DeleteAllInfoAsync(RMDiscoveryDBEFContext efContext)
        {
            await efContext.Database.ExecuteSqlCommandAsync($"EXEC('DBCC CHECKIDENT (''RMGoogleWithoutInDate'', RESEED, 0)')");
            efContext.GoogleWithoutInDateList.RemoveRange(efContext.GoogleWithoutInDateList.ToArray());
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, List<RMDiscoveryGoogleWithoutInDate> updateWithoutInDate)
        {
            efContext.GoogleWithoutInDateList.AddOrUpdate(updateWithoutInDate.ToArray());
            return await efContext.SaveChangesAsync();
        }
    }
}
