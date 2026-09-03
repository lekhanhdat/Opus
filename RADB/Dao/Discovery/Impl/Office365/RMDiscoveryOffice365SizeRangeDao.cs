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
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Microsoft.InformationProtection.Policy.Actions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365SizeRangeDao : IRMDiscoveryOffice365SizeRangeDao
    {
        public async Task InitBuildInDataAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var has = await efContext.Office365SizeRanges.AnyAsync();
            if (has)
            {
                return;
            }

            var sizeRanges = new List<RMDiscoveryOffice365SizeRange>
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
            efContext.Office365SizeRanges.AddRange(sizeRanges);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365SizeRange>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365SizeRanges.ToListAsync();
        }

        public async Task<RMDiscoveryOffice365SizeRange> GetAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365SizeRanges.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<int> DeleteAllDataAsync(RMDiscoveryDBEFContext efContext)
        {
            await efContext.Database.ExecuteSqlCommandAsync($"EXEC('DBCC CHECKIDENT (''RMSizeRange'', RESEED, 0)')");
            efContext.Office365SizeRanges.RemoveRange(efContext.Office365SizeRanges.ToArray());
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, List<RMDiscoveryOffice365SizeRange> updateSizeRange)
        {
            efContext.Office365SizeRanges.AddOrUpdate(updateSizeRange.ToArray());
            return await efContext.SaveChangesAsync();
        }
    }
}
