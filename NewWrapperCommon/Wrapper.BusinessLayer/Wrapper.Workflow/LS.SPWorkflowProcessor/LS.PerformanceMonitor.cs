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
using System.IO;
using System.Text;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS
{
    public class LSPerformanceMonitor
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static StreamWriter mLogFileWriter;

        private static bool mMonitorEnabled = false;
        public static bool MonitorEnabled
        {
            get { return mMonitorEnabled; }
            set { mMonitorEnabled = value; }
        }

        private static string mOutputFileFullPath;
        public static string OutputFile
        {
            get 
            { 
                if(string.IsNullOrEmpty(mOutputFileFullPath))
                    return string.Empty;
                else
                    return mOutputFileFullPath;
            }
        }

        private Dictionary<string, LSHighPerformanceTimer> mMonitorCollection;
        public Dictionary<string, LSHighPerformanceTimer> MonitorCollection
        {
            get
            {
                if (mMonitorCollection == null)
                {
                    mMonitorCollection = new Dictionary<string, LSHighPerformanceTimer>();
                }
                return mMonitorCollection;
            }
        }

        public LSHighPerformanceTimer this[string monitor]
        {
            get
            {
                if (MonitorCollection.ContainsKey(monitor))
                    return MonitorCollection[monitor];
                else
                    return null;
            }
        }

        public LSPerformanceMonitor()
        {
        }

        public void Dispose()
        {
            MonitorCollection.Clear();
        }

        public void StartMonitor(string monitor)
        {
            if (mMonitorEnabled)
            {
                LSHighPerformanceTimer timer;
                if (MonitorCollection.ContainsKey(monitor))
                    timer = MonitorCollection[monitor];
                else
                {
                    timer = new LSHighPerformanceTimer();
                    MonitorCollection.Add(monitor, timer);
                }
                if (timer != null)
                    timer.Start();
            }
        }

        public void StopMonitor(string monitor)
        {
            if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
                MonitorCollection[monitor].Stop();
        }

        public void RemoveMonitor(string monitor)
        {
            if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
            {
                MonitorCollection[monitor].Stop();
                MonitorCollection.Remove(monitor);
            }
        }

        public double GetCurrentDuration(string monitor)
        {
            try
            {
                if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
                    return MonitorCollection[monitor].CurrentDuration;
                else
                    return 0;
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetDurationError, e.ToString());
                return 0;
            }
        }

        public double GetDuration(string monitor)
        {
            try
            {
                if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
                    return MonitorCollection[monitor].Duration;
                else
                    return 0;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetDurationError, e.ToString());
                return 0;
            }
        }

        public void ResetCurrentDuration(string monitor)
        {
            try
            {
                if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
                {
                    double a=MonitorCollection[monitor].CurrentDuration;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ResetDurationError, e.ToString());
            }
        }

        public void WriteMonitorLog(string log)
        {
            if (mMonitorEnabled && mLogFileWriter != null)
            {
                mLogFileWriter.WriteLine(log);
                mLogFileWriter.Flush();
            }
        }

        public void WriteMonitorLog(params Object[] args)
        {
            if (mMonitorEnabled && mLogFileWriter != null)
            {
                if (args != null && args.Length > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (object o in args)
                    {
                        builder.Append(o.ToString());
                        if (o.ToString().IndexOf("Duration:", StringComparison.Ordinal) >= 0 && builder.Length < 64)
                        {
                            int i = 64 - builder.Length;
                            for (int j = 0; j < i; j++)
                                builder.Append(" ");
                        }
                    }
                    mLogFileWriter.WriteLine(builder.ToString());
                    mLogFileWriter.Flush();
                }
            }
        }


        public static void SetPerformanceMonitorOn(string outputFile)
        {
            SetPerformanceMonitorOn(outputFile, false);
        }

        public static void SetPerformanceMonitorOn(string outputFile, bool append)
        {
            mOutputFileFullPath = outputFile;

            if (mLogFileWriter != null)
                mLogFileWriter.Close();
            mLogFileWriter = new StreamWriter(mOutputFileFullPath, append);
            mMonitorEnabled = true;
        }

        public static void SetPerformanceMonitorOff()
        {
            mMonitorEnabled = false;
            if (mLogFileWriter != null)
                mLogFileWriter.Close();
            mLogFileWriter = null;
        }

    }
}
