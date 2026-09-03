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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleExecutionInfoDao : IRMDiscoveryGoogleExecutionInfoDao
    {
        public async Task<(long fileTotalSize, int executedCount)> CalculateAllAsync(LicenseType licenseType)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var fileTotalSize = 0L;

            var fileTotalSizeQuery = efContext.GoogleExecutionInfoList.Where(item => item.LicenseType == licenseType);
            if (await fileTotalSizeQuery.AnyAsync())
            {
                fileTotalSize = await fileTotalSizeQuery.SumAsync(item => item.FileTotalSize);
            }

            var executedCount = await efContext.GoogleExecutionInfoList.Where(item => item.LicenseType == licenseType).CountAsync();
            return (fileTotalSize, executedCount);
        }

        public async Task<(long fileTotalSize, int executedCount, int currentMonthCount)> CalculateAsync(LicenseType licenseType)
        {
            var startMonth = Convert.ToInt32(DateTime.UtcNow.AddMonths(-11).ToString("yyyyMM"));

            var EndMonth = Convert.ToInt32(DateTime.UtcNow.ToString("yyyyMM"));

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var fileTotalSize = (await efContext.GoogleExecutionInfoList.Where(item => item.LicenseType == licenseType).ToListAsync()).Sum(item => item.FileTotalSize);
            var executedCount = await efContext.GoogleExecutionInfoList.Where(item => item.LicenseType == licenseType).CountAsync();
            var currentYearCount = (await efContext.GoogleExecutionInfoList.Where(item => item.LicenseType == licenseType && (item.Month >= startMonth && item.Month <= EndMonth)).ToListAsync()).Count();

            return (fileTotalSize, executedCount, currentYearCount);
        }

        public async Task DeleteAllRecordsAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.GoogleExecutionInfoList.RemoveRange(efContext.GoogleExecutionInfoList);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteByMainJobIdAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var info = await efContext.GoogleExecutionInfoList.FirstOrDefaultAsync(item => item.MainJobId == mainJobId);
            if (info != null)
            {
                efContext.GoogleExecutionInfoList.Remove(info);
                await efContext.SaveChangesAsync();
            }
        }

        public async Task GenerateByMainJobAsync(Guid mainJobId, LicenseType licenseType)
        {
            var month = Convert.ToInt32(DateTime.UtcNow.ToString("yyyyMM"));
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.GoogleExecutionInfoList.Add(new RMDiscoveryGoogleExecutionInfo
            {
                Month = month,
                FileTotalSize = 0,
                MainJobId = mainJobId,
                LicenseType = licenseType
            });
            await efContext.SaveChangesAsync();
        }

        public async Task<int> GetCurrentMonthExecuteCountAsync()
        {
            var month = Convert.ToInt32(DateTime.UtcNow.ToString("yyyyMM"));
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleExecutionInfoList.CountAsync(item => item.Month == month);
        }

        public async Task UpdateFileSizeByMainJobAsync(Guid mainJobId, long fileTotalSize)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var info = await efContext.GoogleExecutionInfoList.FirstOrDefaultAsync(item => item.MainJobId == mainJobId);
            if (info != null)
            {
                info.FileTotalSize = fileTotalSize;
                await efContext.SaveChangesAsync();
            }
        }
    }
}
