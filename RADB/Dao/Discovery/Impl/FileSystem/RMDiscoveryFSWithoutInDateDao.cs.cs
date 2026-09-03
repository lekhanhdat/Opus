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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem
{
    public class RMDiscoveryFSWithoutInDateDao : IRMDiscoveryFSWithoutInDateDao
    {
        public async Task InitBuildInDataAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var has = await efContext.FSWithoutInDateList.AnyAsync();
            if (has)
            {
                return;
            }

            var dataList = new List<RMDiscoveryFSWithoutInDate>
            {
                new()
                {
                    Unit = 12,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 1,
                },
                new()
                {
                    Unit = 24,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 2,
                },
                new()
                {
                    Unit = 36,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 3,
                },
                new()
                {
                    Unit = 60,
                    UnitType = RMDiscoveryWithoutInUnitType.Year,
                    Order = 4,
                },
            };
            efContext.FSWithoutInDateList.AddRange(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryFSWithoutInDate>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.FSWithoutInDateList.ToListAsync();
        }

        public async Task<RMDiscoveryFSWithoutInDate> GetAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.FSWithoutInDateList.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<int> DeleteAllInfoAsync(RMDiscoveryDBEFContext efContext)
        {
            await efContext.Database.ExecuteSqlCommandAsync($"EXEC('DBCC CHECKIDENT (''RMFSWithoutInDate'', RESEED, 0)')");
            efContext.FSWithoutInDateList.RemoveRange(efContext.FSWithoutInDateList.ToArray());
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, List<RMDiscoveryFSWithoutInDate> updateWithoutInDate)
        {
            efContext.FSWithoutInDateList.AddOrUpdate(updateWithoutInDate.ToArray());
            return await efContext.SaveChangesAsync();
        }
    }

}
