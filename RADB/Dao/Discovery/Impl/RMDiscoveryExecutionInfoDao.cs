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
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl
{
    public class RMDiscoveryExecutionInfoDao : IRMDiscoveryExecutionInfoDao
    {

        public async Task<(long fileTotalSize, int executedCount, int currentMonthCount)> CalculateAsync(LicenseType licenseType)
        {
            //以年为计算单位，取当前时间减去12个月的时间,包含本月
            var startMonth = Convert.ToInt32(DateTime.UtcNow.AddMonths(-11).ToString("yyyyMM"));
            //当前月份时间
            var EndMonth = Convert.ToInt32(DateTime.UtcNow.ToString("yyyyMM"));

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var fileTotalSize = (await efContext.ExecutionInfoList.Where(item => item.LicenseType == licenseType).ToListAsync()).Sum(item => item.FileTotalSize);
            var executedCount = await efContext.ExecutionInfoList.Where(item => item.LicenseType == licenseType).CountAsync();
            var currentYearCount = (await efContext.ExecutionInfoList.Where(item => item.LicenseType == licenseType && (item.Month >= startMonth && item.Month <= EndMonth)).ToListAsync()).Count();

            return (fileTotalSize, executedCount, currentYearCount);
        }

        public async Task<int> GetCurrentMonthExecuteCountAsync()
        {
            var month = Convert.ToInt32(DateTime.UtcNow.ToString("yyyyMM"));
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.ExecutionInfoList.CountAsync(item => item.Month == month);
        }

        public async Task<(long fileTotalSize, int executedCount)> CalculateAllAsync(LicenseType licenseType)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var fileTotalSize = 0L;

            var fileTotalSizeQuery = efContext.ExecutionInfoList.Where(item => item.LicenseType == licenseType);
            if (await fileTotalSizeQuery.AnyAsync())
            {
                fileTotalSize = await fileTotalSizeQuery.SumAsync(item => item.FileTotalSize);
            }

            var executedCount = await efContext.ExecutionInfoList.Where(item => item.LicenseType == licenseType).CountAsync();
            return (fileTotalSize, executedCount);
        }

        public async Task GenerateByMainJobAsync(Guid mainJobId, LicenseType licenseType)
        {
            var month = Convert.ToInt32(DateTime.UtcNow.ToString("yyyyMM"));
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.ExecutionInfoList.Add(new Model.Discovery.Office365.RMDiscoveryOffice365ExecutionInfo
            {
                Month = month,
                FileTotalSize = 0,
                MainJobId = mainJobId,
                LicenseType = licenseType
            });
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteByMainJobIdAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var info = await efContext.ExecutionInfoList.FirstOrDefaultAsync(item => item.MainJobId == mainJobId);
            if(info != null)
            {
                efContext.ExecutionInfoList.Remove(info);
                await efContext.SaveChangesAsync();
            }
        }

        public async Task UpdateFileSizeByMainJobAsync(Guid mainJobId, long fileTotalSize)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var info = await efContext.ExecutionInfoList.FirstOrDefaultAsync(item => item.MainJobId == mainJobId);
            if(info != null)
            {
                info.FileTotalSize = fileTotalSize;
                await efContext.SaveChangesAsync();
            }
        }

        public async Task DeleteAllRecordsAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.ExecutionInfoList.RemoveRange(efContext.ExecutionInfoList);
            await efContext.SaveChangesAsync();
        }
    }
}
