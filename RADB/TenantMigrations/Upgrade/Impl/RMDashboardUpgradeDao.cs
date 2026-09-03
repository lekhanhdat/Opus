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
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMDashboardUpgradeDao: IDbUpgradeDao
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMDashboardUpgradeDao));

       

        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                if (context.Schedule.Count(item => item.JobCategory == (int)ScheduleType.Dashboard) > 0)
                {
                    Logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has dashboard job schedule.");
                    return;
                }

                var timeZone = GetTimeZone();
                var startTime = GetRandomStartTime(timeZone);
                startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
                startTime = ConvertTimeToUtcDate(startTime, GeneralSettingConfig.FindSystemTimeZoneById(timeZone), !GetDayLight());
                startTime = DateTime.Parse(startTime.ToString(APIDateTimeFormat.DATETYPEForAPI003));
                var scheduleInfo = new RMSchedule
                {
                    Id = Guid.NewGuid().ToString(),
                    JobCategory = (int)ScheduleType.Dashboard,
                    StartTime = startTime.Ticks,
                    EndTime = DateTime.MaxValue.Ticks,
                    NextTime = startTime.Ticks,
                    EndType = (int)EndType.NoEnd,
                    Interval = 1,
                    IntervalType = (int)IntervalType.Daily,
                    OccurrencesTotal = 1,
                    Occurrences = 0,
                    IsDaylightSaving = GetDayLight(),
                    TimeZoneId = timeZone
                };

                context.Schedule.Add(scheduleInfo);
                context.SaveChanges();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while dashboard upgrade logic. Error: {e}");
            }
        }

        private DateTime ConvertTimeToUtcDate(DateTime datetime, TimeZoneInfo sourceTimezone, bool useDst)// = true)
        {
            datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Unspecified);
            // 时间为夏令时时间 且指定不使用夏令时 加一小时
            if (useDst && sourceTimezone.SupportsDaylightSavingTime && sourceTimezone.IsDaylightSavingTime(datetime))
            {
                datetime = datetime.AddHours(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(datetime, sourceTimezone);
        }

        private string GetTimeZone()
        {
            using (var sysContext = RMDBContextManager.GetSystemDBContext())
            {
                var generalSetting = sysContext.RMCPGeneralSetting.Where(item => item.TenantId == TenantLocalValue.LogonGroupId).FirstOrDefault();
                return generalSetting?.TimeZone ?? TimeZoneInfo.Local.Id;
            }
        }

        private bool GetDayLight()
        {
            using (var sysContext = RMDBContextManager.GetSystemDBContext())
            {
                var generalSetting = sysContext.RMCPGeneralSetting.Where(item => item.TenantId == TenantLocalValue.LogonGroupId).FirstOrDefault();
                return (generalSetting != null && generalSetting.DayLight) || TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now);
            }
        }

        private DateTime GetRandomStartTime(string timeZoneId)
        {
            /* Fortify Issue Type: Insecure Randomness 
            * Sink Details:  this method
            * Ignore Reason: random用于生成时间，不涉及安全问题 
            */
            Random random = new Random((int)DateTime.Now.Ticks);
            var hour = random.Next(-2, 3);
            hour = hour < 0 ? hour + 24 : hour;
            var min = random.Next(0, 59);
            var second = random.Next(0, 59);

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId));

            var startTime = localNow;
            if(hour <= localNow.Hour)
            {
                startTime = localNow.AddDays(1);
            }

            return new DateTime(startTime.Year, startTime.Month, startTime.Day, hour, min, second);
        }
    }
}
