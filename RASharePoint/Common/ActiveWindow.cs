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
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.GCommon;
using AvePoint.RA.SharePoint.ActionOnly.Base;

namespace AvePoint.RA.SharePoint.Common
{
    public class ActiveWindowConfiguration
    {
        public string TimeZoneId { get; set; }
        public bool EnableDaylightSaving { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class ActiveWindow
    {
        public TimeSpan StartTimeSpan { get; private set; }
        public TimeSpan EndTimeSpan { get; private set; }
        public TimeZoneInfo TimeZone { get; private set; }
        private bool EnableDaylightSaving { get; set; }
        public bool IsEnabled { get; private set; }

        private const string Key_ActiveWindow = "ActiveWindow";
        //private ICSDDictionaryDao CSDDictionaryDao = new CSDDictionaryDao();
        private IRMKeyValueDao mRMKeyValueDao;
        protected IRMKeyValueDao RMKeyValueDao
        {
            get
            {
                if (mRMKeyValueDao == null)
                {
                    mRMKeyValueDao = new RMKeyValueDao();
                }
                return mRMKeyValueDao;
            }
        }
        private readonly IAveLogger Logger = AveLogger.GetInstance(typeof(ActiveWindow));

        public void Init()
        {
            var activeWindowStr = RMKeyValueDao.GetValueByKey(Key_ActiveWindow)?.Value;
            if (string.IsNullOrEmpty(activeWindowStr))
            { 
                this.IsEnabled = false;
                Logger.Info($"ActiveWindow is not enabled.");
                return;
            }
            else
            {
                this.IsEnabled = true;
                Logger.Info($"ActiveWindowConfiguration Json is: {activeWindowStr}");
            }

            var activeWindowConfig = JsonConvert.DeserializeObject<ActiveWindowConfiguration>(activeWindowStr);
            //var timeZone = GeneralSettingConfig.GetTimeZoneInforById(activeWindowConfig.TimeZoneId);
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(activeWindowConfig.TimeZoneId);
            if (timeZone == null)
            {
                throw new Exception($"TimeZoneId in ActiveWindowConfiguration is wrong. TimeZoneId:[{activeWindowConfig.TimeZoneId}]");
            }
            else
            {
                this.TimeZone = timeZone;
            }

            if (!TimeSpan.TryParse(activeWindowConfig.StartTime, out TimeSpan startTimeSpan))
            {
                throw new Exception($"StartTime in ActiveWindowConfiguration is wrong. StartTime:[{activeWindowConfig.StartTime}]");
            }
            else
            {
                this.StartTimeSpan = startTimeSpan;
            }

            if (!TimeSpan.TryParse(activeWindowConfig.EndTime, out TimeSpan endTimeSpan))
            {
                throw new Exception($"EndTime in ActiveWindowConfiguration is wrong. EndTime:[{activeWindowConfig.EndTime}]");
            }
            else
            {
                this.EndTimeSpan = endTimeSpan;
            }

            this.EnableDaylightSaving = activeWindowConfig.EnableDaylightSaving;
        }

        public bool IsCurrentTimeInActiveWindow()
        {
            bool isCurrentTimeActive = false;
            try
            {
                DateTime currentTime;
                if (this.EnableDaylightSaving)
                {
                    currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.TimeZone);
                }
                else 
                {
                    currentTime = DateTime.UtcNow + this.TimeZone.BaseUtcOffset;
                }
                TimeSpan currentTimeSpan = currentTime.TimeOfDay;
                //跨越一天，例如开始时间是20：00，结束时间是6:00
                if (this.StartTimeSpan > this.EndTimeSpan)
                {
                    isCurrentTimeActive = currentTimeSpan >= this.StartTimeSpan || currentTimeSpan < this.EndTimeSpan;
                }
                else
                {
                    isCurrentTimeActive = currentTimeSpan >= this.StartTimeSpan && currentTimeSpan < this.EndTimeSpan;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Checking current time is in active window error. Detail:{e}");
            }
            return isCurrentTimeActive;
        }

    }

}
