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
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    public sealed class AvePerformanceMonitor
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static SortedDictionary<string, AvePerformanceStatistics> sStatistics = new SortedDictionary<string, AvePerformanceStatistics>();
        private readonly static object sSyncLock = new object();
        private static bool sIsDisabled = true;

        internal static AvePerformanceTimer Start(string fullName)
        {
            if (sIsDisabled)
            {
                return null;
            }
            AvePerformanceTimer timer = new AvePerformanceTimer(fullName);
            timer.Start();
            return timer;
        }

        internal static void Stop(AvePerformanceTimer timer)
        {
            if (sIsDisabled)
            {
                return;
            }
            timer.Stop();
            UpdateStatistics(timer);
        }

        internal static void UpdateStatistics(AvePerformanceTimer timer)
        {
            lock (sSyncLock)
            {
                AvePerformanceStatistics statistics = null;
                if (!sStatistics.TryGetValue(timer.FullName, out statistics))
                {
                    statistics = new AvePerformanceStatistics();
                    sStatistics[timer.FullName] = statistics;
                }
                long elapsed = timer.Stopwatch.ElapsedMilliseconds;
                statistics.Count++;
                statistics.Duration += elapsed;
                statistics.MaxTime = elapsed > statistics.MaxTime ? elapsed : statistics.MaxTime;
                statistics.MinTime = elapsed < statistics.MinTime ? elapsed : statistics.MinTime;
            }
        }

        public static void SetDisable(bool isDisable)
        {
            sIsDisabled = isDisable;
            AveRequestStatisticMonitor.SetDisable(isDisable);
        }

        public static void WriteToFile(string path)
        {
            if (sIsDisabled)
            {
                return;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("TimeResults");
                long maxCount = 0;
                long maxTotal = 0;
                long maxEach = 0;
                long each;
                XmlElement maxCountEle = null;
                XmlElement maxTotalEle = null;
                XmlElement maxEachEle = null;
                foreach (KeyValuePair<string, AvePerformanceStatistics> keyValue in sStatistics)
                {
                    if (keyValue.Value.Count == 0)
                    {
                        continue;
                    }
                    each = keyValue.Value.Duration / keyValue.Value.Count;
                    XmlElement ele = doc.CreateElement("Result");
                    ele.SetAttribute("method", keyValue.Key);
                    ele.SetAttribute("total", keyValue.Value.Duration.ToString());
                    ele.SetAttribute("count", keyValue.Value.Count.ToString());
                    ele.SetAttribute("each", each.ToString());
                    ele.SetAttribute("max", keyValue.Value.MaxTime.ToString());
                    ele.SetAttribute("min", keyValue.Value.MinTime.ToString());
                    if (maxCount < keyValue.Value.Count)
                    {
                        maxCount = keyValue.Value.Duration;
                        maxCountEle = ele;
                    }
                    if (maxTotal < keyValue.Value.Duration)
                    {
                        maxTotal = keyValue.Value.Duration;
                        maxTotalEle = ele;
                    }
                    if (maxEach < each)
                    {
                        maxEach = each;
                        maxEachEle = ele;
                    }
                    root.AppendChild(ele);
                }
                root.SetAttribute("TimeStamp", new DateTime().ToString());
                if (maxTotalEle != null)
                {
                    root.SetAttribute("maxTotal", maxTotalEle.Attributes["method"].Value
                                                        + "," + maxTotalEle.Attributes["total"].Value
                                                        + "," + maxTotalEle.Attributes["count"].Value
                                                        + "," + maxTotalEle.Attributes["each"].Value
                                                        + "," + maxTotalEle.Attributes["max"].Value
                                                        + "," + maxTotalEle.Attributes["min"].Value);
                }
                if (maxEachEle != null)
                {
                    root.SetAttribute("maxEach", maxEachEle.Attributes["method"].Value
                                                        + "," + maxEachEle.Attributes["total"].Value
                                                        + "," + maxEachEle.Attributes["count"].Value
                                                        + "," + maxEachEle.Attributes["each"].Value
                                                        + "," + maxEachEle.Attributes["max"].Value
                                                        + "," + maxEachEle.Attributes["min"].Value);
                }
                if (maxCountEle != null)
                {
                    root.SetAttribute("maxCount", maxCountEle.Attributes["method"].Value
                                                        + "," + maxCountEle.Attributes["total"].Value
                                                        + "," + maxCountEle.Attributes["count"].Value
                                                        + "," + maxCountEle.Attributes["each"].Value
                                                        + "," + maxCountEle.Attributes["max"].Value
                                                        + "," + maxCountEle.Attributes["min"].Value);
                }

                doc.AppendChild(root);
                string jobDir = path.Substring(0,path.LastIndexOf("\\"));
                if (!Directory.Exists(jobDir))
                {
                    Directory.CreateDirectory(jobDir);
                }
                using (FileStream writer = new FileStream(path, FileMode.Create))
                {
                    byte[] buf = Encoding.UTF8.GetBytes(doc.InnerXml);
                    writer.Write(buf, 0, buf.Length);
                }
            }
            catch(Exception ex)
            {
                mLog.Debug("An error occurred while write performance to file.ErrorMessage:{0}.", ex.ToString());
            }
        }

        public static void WritePerformanceResult()
        {
            if (sIsDisabled)
            {
                return;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("TimeResults");
                long maxCount = 0;
                long maxTotal = 0;
                long maxEach = 0;
                long each;
                XmlElement maxCountEle = null;
                XmlElement maxTotalEle = null;
                XmlElement maxEachEle = null;
                foreach (KeyValuePair<string, AvePerformanceStatistics> keyValue in sStatistics)
                {
                    if (keyValue.Value.Count == 0)
                    {
                        continue;
                    }
                    each = keyValue.Value.Duration / keyValue.Value.Count;
                    XmlElement ele = doc.CreateElement("Result");
                    ele.SetAttribute("method", keyValue.Key);
                    ele.SetAttribute("total", keyValue.Value.Duration.ToString());
                    ele.SetAttribute("count", keyValue.Value.Count.ToString());
                    ele.SetAttribute("each", each.ToString());
                    ele.SetAttribute("max", keyValue.Value.MaxTime.ToString());
                    ele.SetAttribute("min", keyValue.Value.MinTime.ToString());
                    if (maxCount < keyValue.Value.Count)
                    {
                        maxCount = keyValue.Value.Duration;
                        maxCountEle = ele;
                    }
                    if (maxTotal < keyValue.Value.Duration)
                    {
                        maxTotal = keyValue.Value.Duration;
                        maxTotalEle = ele;
                    }
                    if (maxEach < each)
                    {
                        maxEach = each;
                        maxEachEle = ele;
                    }
                    root.AppendChild(ele);
                }
                root.SetAttribute("TimeStamp", new DateTime().ToString());
                if (maxTotalEle != null)
                {
                    root.SetAttribute("maxTotal", maxTotalEle.Attributes["method"].Value
                                                        + "," + maxTotalEle.Attributes["total"].Value
                                                        + "," + maxTotalEle.Attributes["count"].Value
                                                        + "," + maxTotalEle.Attributes["each"].Value
                                                        + "," + maxTotalEle.Attributes["max"].Value
                                                        + "," + maxTotalEle.Attributes["min"].Value);
                }
                if (maxEachEle != null)
                {
                    root.SetAttribute("maxEach", maxEachEle.Attributes["method"].Value
                                                        + "," + maxEachEle.Attributes["total"].Value
                                                        + "," + maxEachEle.Attributes["count"].Value
                                                        + "," + maxEachEle.Attributes["each"].Value
                                                        + "," + maxEachEle.Attributes["max"].Value
                                                        + "," + maxEachEle.Attributes["min"].Value);
                }
                if (maxCountEle != null)
                {
                    root.SetAttribute("maxCount", maxCountEle.Attributes["method"].Value
                                                        + "," + maxCountEle.Attributes["total"].Value
                                                        + "," + maxCountEle.Attributes["count"].Value
                                                        + "," + maxCountEle.Attributes["each"].Value
                                                        + "," + maxCountEle.Attributes["max"].Value
                                                        + "," + maxCountEle.Attributes["min"].Value);
                }

                doc.AppendChild(root);
                mLog.Info(doc.OuterXml);

                AveRequestStatisticMonitor.WriteRequestStatisticsResult();
            }
            catch (Exception ex)
            {
                mLog.Debug("An error occurred while write performance to file.ErrorMessage:{0}.", ex.ToString());
            }
        }
    }

    internal class AvePerformanceStatistics
    {
        public AvePerformanceStatistics()
        {
            this.Duration = 0;
            this.MinTime = long.MaxValue;
            this.MaxTime = long.MinValue;
            this.Count = 0;
        }

        public long Duration
        {
            get;
            set;
        }

        public long Count
        {
            get;
            set;
        }

        public long MaxTime
        {
            get;
            set;
        }

        public long MinTime
        {
            get;
            set;
        }
    }

    internal class AvePerformanceTimer
    {
        private string mFullName;
        private Stopwatch mStopwatch;

        public AvePerformanceTimer(string fullname)
        {
            mFullName = fullname;
            mStopwatch = new Stopwatch();
        }

        public void Start()
        {
            mStopwatch.Start();
        }

        public void Stop()
        {
            mStopwatch.Stop();
        }

        public string FullName
        {
            get
            {
                return mFullName;
            }
        }

        public Stopwatch Stopwatch
        {
            get
            {
                return mStopwatch;
            }
        }
    }
}
