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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMFunctionSettingDao : BaseDao<RMFunctionSetting>, IRMFunctionSettingDao
    {
        private RALogger Logger = RALogger.GetInstance(typeof(RMFunctionSettingDao));
        public bool TryGet(FunctionSettingType type, out RMFunctionSetting setting)
        {
            using var context = GetNewContext();
            setting = context.FunctionSettings.FirstOrDefault(item => item.Type == type);
            return setting != null;
        }

        public async Task<string> GetSettingInfo(FunctionSettingType type)
        {
            using var context = GetNewContext();
            var setting = await context.FunctionSettings.FirstOrDefaultAsync(item => item.Type == type);
            return setting?.SettingInfo;
        }

        public async Task<bool> AddOrUpdateSettingInfoAsync(FunctionSettingType type, string settingInfo)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            using var context = GetNewContext();
            var setting = await context.FunctionSettings.FirstOrDefaultAsync(item => item.Type == type);
            if (setting == null)
            {
                setting = new RMFunctionSetting
                {
                    Id = Guid.NewGuid(),
                    Type = type,
                    SettingInfo = settingInfo,
                    CreatedTime = nowTicks,
                    ModifiedTime = nowTicks
                };

                context.FunctionSettings.Add(setting);
                return await context.SaveChangesAsync() > 0;
            }

            setting.SettingInfo = settingInfo;
            setting.ModifiedTime = nowTicks;

            context.FunctionSettings.AddOrUpdate(setting);

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> NotExistCreateIt(FunctionSettingType type, string settingInfo)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            using var context = GetNewContext();
            var exist = await context.FunctionSettings.AnyAsync(item => item.Type == type);
            if (exist)
            {
                return true;
            }

            var setting = new RMFunctionSetting
            {
                Id = Guid.NewGuid(),
                Type = type,
                SettingInfo = settingInfo,
                CreatedTime = nowTicks,
                ModifiedTime = nowTicks
            };

            context.FunctionSettings.Add(setting);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<long> MultiGeoInsertFunctionSettingTableAsync(IEnumerable<RMFunctionSetting> settingInfoes)
        {
            try
            {
                using var context = GetNewContext();
                context.FunctionSettings.AddRange(settingInfoes);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while inserting multi-geo setting info.", ex);
                return 0;
            }
        }
        public async Task<long> MultiGeoDeleteAllFunctionSettingAsync()
        {
            return await TruncateAllDataInTableAsync("RMFunctionSettings");
        }
        public async Task<IEnumerable<object>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.FunctionSettings.AsNoTracking()
                .OrderBy(item => item.CreatedTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
