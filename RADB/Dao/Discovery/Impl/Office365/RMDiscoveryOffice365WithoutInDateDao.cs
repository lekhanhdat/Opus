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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365WithoutInDateDao : IRMDiscoveryOffice365WithoutInDateDao
    {
        public async Task InitBuildInDataAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var has = await efContext.Office365WithoutInDateList.AnyAsync();
            if (has)
            {
                return;
            }

            var dataList = new List<RMDiscoveryOffice365WithoutInDate>
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
            efContext.Office365WithoutInDateList.AddRange(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365WithoutInDate>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365WithoutInDateList.ToListAsync();
        }

        public async Task<RMDiscoveryOffice365WithoutInDate> GetAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365WithoutInDateList.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<int> DeleteAllInfoAsync(RMDiscoveryDBEFContext efContext)
        {
            await efContext.Database.ExecuteSqlCommandAsync($"EXEC('DBCC CHECKIDENT (''RMWithoutInDate'', RESEED, 0)')");
            efContext.Office365WithoutInDateList.RemoveRange(efContext.Office365WithoutInDateList.ToArray());
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, List<RMDiscoveryOffice365WithoutInDate> updateWithoutInDate)
        {
            efContext.Office365WithoutInDateList.AddOrUpdate(updateWithoutInDate.ToArray());
            return await efContext.SaveChangesAsync();
        }
    }
}
