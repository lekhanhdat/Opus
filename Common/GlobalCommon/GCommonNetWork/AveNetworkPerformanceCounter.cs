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
using System.Diagnostics;
using System.Runtime.InteropServices;


// <summary>
//this file is a copy of AvePerformanceCounter.cs in GCommonUtility. because this assemly can't reference GCommonUtility,
//it will be deployed in GAC.
// </summary>
namespace AvePoint.GCommon.Network
{
    internal class AveNetworkPerformanceCounterUtil
    {
        [DllImport("Kernel32.dll")]
        internal static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        internal static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private static long mFrequency = -1;

        internal static long Frequency
        {
            get
            {
                if (mFrequency == -1)
                {
                    try
                    {
                        QueryPerformanceFrequency(out mFrequency);
                    }
                    catch (Exception e)
                    {
                        AveNetworkTrace.TraceError(e.ToString());
                        mFrequency = 0;
                    }
                }
                return mFrequency;
            }
        }
    }

    internal class AveNetworkSpeedPerformanceCounterCatalogs
    {
        public static readonly string SocketReadCatalog = "DocAve Socket Read";
        public static readonly string SocketWriteCatalog = "DocAve Socket Write";

        public static readonly List<string> AllCatalogs = new List<string>() { SocketReadCatalog, SocketWriteCatalog };
    }

    internal class AveNetworkSpeedPerformanceCounter
    {
        private const long KB = 1024;

        private static bool mEnabled = false;

        private static string mTotalBytesCounterName = "Total(KB)";
        private static List<PerformanceCounter> mTotalBytesCounters = new List<PerformanceCounter>();
        private static List<long> mTotalBytes = new List<long>();

        private static string mTotalDurationCounterName = "Total Duration(seconds)";
        private static List<PerformanceCounter> mTotalDurationCounters = new List<PerformanceCounter>();
        private static List<double> mTotalDurations = new List<double>();

        private static string mSpeedInLastMinuteCounterName = "Speed(KB/sec)";
        private static List<PerformanceCounter> mSpeedInLastMinuteCounters = new List<PerformanceCounter>();
        private static List<long> mTotalBytesInLastMinutes = new List<long>();
        private static List<double> mTotalDurationInLastMinutes = new List<double>();
        private static List<DateTime> mResetDeadlines = new List<DateTime>();

        private static List<long> mStartTimes = new List<long>();
        private static List<long> mEndTimes = new List<long>();

        static AveNetworkSpeedPerformanceCounter()
        {
            try
            {
                if (AveNetworkPerformanceCounterUtil.Frequency > 0)
                {
                    SetupCategory();
                    string counterInstanceName = Process.GetCurrentProcess().ProcessName + "_" + Process.GetCurrentProcess().Id;
                    for (int i = 0; i < AveNetworkSpeedPerformanceCounterCatalogs.AllCatalogs.Count; i++)
                    {
                        string catalogName = AveNetworkSpeedPerformanceCounterCatalogs.AllCatalogs[i];
                        mTotalBytesCounters.Add(new PerformanceCounter(catalogName, mTotalBytesCounterName, counterInstanceName, false));
                        mTotalBytes.Add(0);

                        mTotalDurationCounters.Add(new PerformanceCounter(catalogName, mTotalDurationCounterName, counterInstanceName, false));
                        mTotalDurations.Add(0);

                        mSpeedInLastMinuteCounters.Add(new PerformanceCounter(catalogName, mSpeedInLastMinuteCounterName, counterInstanceName, false));
                        mTotalBytesInLastMinutes.Add(0);
                        mTotalDurationInLastMinutes.Add(0);
                        mResetDeadlines.Add(DateTime.Now.AddMinutes(1));

                        mStartTimes.Add(0);
                        mEndTimes.Add(0);
                    }
                    mEnabled = true;
                }
            }
            catch (Exception e)
            {
                AveNetworkTrace.TraceError(e.ToString());
            }
        }

        public static void Begin(string catalogName)
        {
            if (mEnabled)
            {
                int index = AveNetworkSpeedPerformanceCounterCatalogs.AllCatalogs.IndexOf(catalogName);
                if (index != -1)
                {
                    long startTime = 0;
                    AveNetworkPerformanceCounterUtil.QueryPerformanceCounter(out startTime);
                    mStartTimes[index] = startTime;
                }
            }
        }

        public static void End(string catalogName, long count)
        {
            if (mEnabled)
            {
                int index = AveNetworkSpeedPerformanceCounterCatalogs.AllCatalogs.IndexOf(catalogName);
                if (index != -1)
                {
                    long endTime = 0;
                    AveNetworkPerformanceCounterUtil.QueryPerformanceCounter(out endTime);
                    mEndTimes[index] = endTime;
                    double duration = (mEndTimes[index] - mStartTimes[index]) * 1.0 / AveNetworkPerformanceCounterUtil.Frequency;
                    mTotalDurations[index] += duration;
                    mTotalBytes[index] += count;

                    mTotalBytesCounters[index].RawValue = mTotalBytes[index] / KB;
                    mTotalDurationCounters[index].RawValue = (long)mTotalDurations[index];

                    if (DateTime.Now > mResetDeadlines[index])
                    {
                        mTotalBytesInLastMinutes[index] = 0;
                        mTotalDurationInLastMinutes[index] = 0;
                        mResetDeadlines[index] = DateTime.Now.AddMinutes(1);
                    }
                    else
                    {
                        mTotalBytesInLastMinutes[index] += count;
                        mTotalDurationInLastMinutes[index] += duration;
                        if (mTotalDurationInLastMinutes[index] > 0)
                        {
                            mSpeedInLastMinuteCounters[index].RawValue = (long)(mTotalBytesInLastMinutes[index] / KB / mTotalDurationInLastMinutes[index]);
                        }
                    }
                }
            }
        }

        private static void SetupCategory()
        {
            string[] deprecatedCatalogs = new string[] 
            {
                "DocAve Socket Channel", 
                "DocAve Socket Channel Read", 
                "DocAve Socket Channel Write" 
            };
            foreach (string deprecatedCatalog in deprecatedCatalogs)
            {
                if (PerformanceCounterCategory.Exists(deprecatedCatalog))
                {
                    PerformanceCounterCategory.Delete(deprecatedCatalog);
                }
            }

            for (int i = 0; i < AveNetworkSpeedPerformanceCounterCatalogs.AllCatalogs.Count; i++)
            {
                string catalogName = AveNetworkSpeedPerformanceCounterCatalogs.AllCatalogs[i];
                if (!PerformanceCounterCategory.Exists(catalogName))
                {
                    CounterCreationDataCollection CCDC = new CounterCreationDataCollection();

                    CounterCreationData counter64 = new CounterCreationData();
                    counter64.CounterType = PerformanceCounterType.NumberOfItems64;
                    counter64.CounterName = mTotalBytesCounterName;
                    CCDC.Add(counter64);
                    counter64 = new CounterCreationData();
                    counter64.CounterType = PerformanceCounterType.NumberOfItems64;
                    counter64.CounterName = mTotalDurationCounterName;
                    CCDC.Add(counter64); ;
                    counter64 = new CounterCreationData();
                    counter64.CounterType = PerformanceCounterType.NumberOfItems64;
                    counter64.CounterName = mSpeedInLastMinuteCounterName;
                    CCDC.Add(counter64); ;

                    PerformanceCounterCategory.Create(catalogName, string.Empty, PerformanceCounterCategoryType.MultiInstance, CCDC);
                }
            }
        }

    }

}
