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
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem
{
    public class RMDiscoveryFSSizeRangeDao : IRMDiscoveryFSSizeRangeDao
    {
        public async Task InitBuildInDataAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var has = await efContext.FSSizeRanges.AnyAsync();
            if (has)
            {
                return;
            }

            var sizeRanges = new List<RMDiscoveryFSSizeRange>
            {
                new()
                {
                    GenerateEqual = 0,
                    LessThan = 1,
                    Order = 0,
                    DisplayName = "RM_FA_SizeRange_LessThan1MB",
                },
                new()
                {
                    GenerateEqual = 1,
                    LessThan = 50,
                    Order = 1,
                    DisplayName = "RM_FA_SizeRange_GreaterThan1MB",
                },
                new()
                {
                    GenerateEqual = 50,
                    LessThan = 100,
                    Order = 2,
                    DisplayName = "RM_FA_SizeRange_GreaterThan50MB",
                },
                new()
                {
                    GenerateEqual = 100,
                    LessThan = int.MaxValue,
                    Order = 3,
                    DisplayName = "RM_FA_SizeRange_GreaterThan100MB",
                },
            };
            efContext.FSSizeRanges.AddRange(sizeRanges);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryFSSizeRange>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.FSSizeRanges.ToListAsync();
        }

        public async Task<RMDiscoveryFSSizeRange> GetAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.FSSizeRanges.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<int> DeleteAllDataAsync(RMDiscoveryDBEFContext efContext)
        {
            await efContext.Database.ExecuteSqlCommandAsync($"EXEC('DBCC CHECKIDENT (''RMFSSizeRange'', RESEED, 0)')");
            efContext.FSSizeRanges.RemoveRange(efContext.FSSizeRanges.ToArray());
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, List<RMDiscoveryFSSizeRange> updateSizeRange)
        {
            efContext.FSSizeRanges.AddOrUpdate(updateSizeRange.ToArray());
            return await efContext.SaveChangesAsync();
        }
    }
}
