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
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleSizeRangeDao : IRMDiscoveryGoogleSizeRangeDao
    {
        public async Task InitBuildInDataAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var has = await efContext.GoogleSizeRanges.AnyAsync();
            if (has)
            {
                return;
            }

            var sizeRanges = new List<RMDiscoveryGoogleSizeRange>
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
                    LessThan = int.MaxValue,
                    Order = 2,
                    DisplayName = "RM_FA_SizeRange_GreaterThan50MB",
                },
            };
            efContext.GoogleSizeRanges.AddRange(sizeRanges);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryGoogleSizeRange>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleSizeRanges.ToListAsync();
        }

        public async Task<RMDiscoveryGoogleSizeRange> GetAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleSizeRanges.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<int> DeleteAllDataAsync(RMDiscoveryDBEFContext efContext)
        {
            await efContext.Database.ExecuteSqlCommandAsync($"EXEC('DBCC CHECKIDENT (''RMGoogleSizeRange'', RESEED, 0)')");
            efContext.GoogleSizeRanges.RemoveRange(efContext.GoogleSizeRanges.ToArray());
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, List<RMDiscoveryGoogleSizeRange> updateSizeRange)
        {
            efContext.GoogleSizeRanges.AddOrUpdate(updateSizeRange.ToArray());
            return await efContext.SaveChangesAsync();
        }
    }
}
