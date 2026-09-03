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
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Common
{
    public enum WorkDays
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64,
    }

    public class ThrottleControlInfo
    {
        private const long RESET_INTERVAL = 1 * 60 * 1000;

        private bool mEnable = false;
        private int mWorkdays = 0;
        private int mStartHour = -1;
        private int mEndHour = -1;
        private long mWorkHoursRate_bytesPerSecond = -1;
        private long mNonWorkHoursRate_bytesPerSecond = -1;

        public bool IsEnable
        {
            get { return mEnable; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Throttle Control Details:");
            sb.AppendFormat("\tEnable={0}", mEnable);
            sb.AppendFormat("\tWorkDays={0}", mWorkdays);
            sb.AppendFormat("\tStartHour={0}", mStartHour);
            sb.AppendFormat("\tEndHour={0}", mEndHour);
            sb.AppendFormat("\tWorkHoursRate_bytesPerSecond={0}", mWorkHoursRate_bytesPerSecond);
            sb.AppendFormat("\tNonWorkHoursRate_bytesPerSecond={0}", mNonWorkHoursRate_bytesPerSecond);
            return sb.ToString();
        }

        public ThrottleControlInfo()
        {
            this.mEnable = false;
        }

        #region --init--

        public void Enable(bool enable)
        {
            this.mEnable = enable;
        }

        public void SetWorkDay(WorkDays day)
        {
            mWorkdays |= (int)day;
        }

        public void SetWorkDay(int days)
        {
            mWorkdays = days;
        }

        public void SetStartHour(int hour)
        {
            CheckHourValue(hour);
            mStartHour = hour;
        }

        public void SetEndHour(int hour)
        {
            CheckHourValue(hour);
            mEndHour = hour;
        }

        public void SetWorkHoursRate(long bytesPerSecond)
        {
            this.mWorkHoursRate_bytesPerSecond = bytesPerSecond;
        }

        public void SetNonWorkHoursRate(long bytesPerSecond)
        {
            this.mNonWorkHoursRate_bytesPerSecond = bytesPerSecond;
        }

        private void CheckHourValue(int hour)
        {
            if (hour < 1 || hour > 24)
            {
                throw new Exception("hour must >=1,<=24");
            }
        }

        private bool IsWorkDay()
        {
            WorkDays day = (WorkDays)Enum.Parse(typeof(WorkDays), Enum.GetName(typeof(DayOfWeek), DateTime.Now.DayOfWeek), true);
            return ((mWorkdays & (int)day) != 0);
        }

        private bool IsWorkHour()
        {
            int hour = DateTime.Now.Hour;
            return ((hour >= mStartHour) && (hour <= mEndHour));
        }

        public bool ValidThrottleControlInfo()
        {
            if (!mEnable)
            {
                return false;
            }
            else
            {
                if (mStartHour < 1 || mStartHour > 24)
                {
                    return false;
                }
                if (mEndHour < 1 || mEndHour > 24)
                {
                    return false;
                }
                //如果这两个值都是负数或零，证明不需要控制
                if (mWorkHoursRate_bytesPerSecond <= 0 && mNonWorkHoursRate_bytesPerSecond <= 0)
                {
                    return false;
                }
                //if (mNonWorkHoursRate_bytesPerSecond <= 0)
                //{
                //    return false;
                //}
                return true;
            }
        }

        #endregion

        #region -- --

        const int DEFAULT_SLEEP_INTERVAL = 300;

        private DateTime mLastResetTime = DateTime.MinValue;
        private long mTotalSendBytesFromLastReset = 0;
        private int mSleepInterval = DEFAULT_SLEEP_INTERVAL;
        private bool mIsWorkingTime = false;

        public void WriteBytesCount(long bytesWritten)
        {
            if (!ValidThrottleControlInfo())
            {
                return;
            }
            if (mLastResetTime.AddMilliseconds(RESET_INTERVAL) < DateTime.Now)
            {
                mLastResetTime = DateTime.Now;
                mTotalSendBytesFromLastReset = 0;
                if (IsWorkDay() && IsWorkHour())
                {
                    mIsWorkingTime = true;
                }
                else
                {
                    mIsWorkingTime = false;
                }
            }
            mTotalSendBytesFromLastReset += bytesWritten;
            long secondsFromLastReset = ((DateTime.Now.Ticks - mLastResetTime.Ticks) / 10000) / 1000;
            if (secondsFromLastReset <= 0)
            {
                Thread.Sleep(300);
                return;
            }
            long useRate = -1;
            if (mIsWorkingTime)
            {
                useRate = mWorkHoursRate_bytesPerSecond;
            }
            else
            {
                useRate = mNonWorkHoursRate_bytesPerSecond;
            }
            if (useRate <= 0)
            {
                mSleepInterval = DEFAULT_SLEEP_INTERVAL;
            }
            else
            {
                if (mTotalSendBytesFromLastReset / secondsFromLastReset > useRate)
                {
                    Thread.Sleep(mSleepInterval);
                    mSleepInterval += DEFAULT_SLEEP_INTERVAL;
                }
                else
                {
                    mSleepInterval = DEFAULT_SLEEP_INTERVAL;//reset;
                }
            }
        }

        #endregion
    }
}
