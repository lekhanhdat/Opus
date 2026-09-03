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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.Common
{
    public static class PerformanceMonitor
    {
        private static RALogger mLog = RALogger.GetInstance(typeof(PerformanceMonitor));

        private static SortedDictionary<string, RecPerformanceStatistics> sStatistics = new SortedDictionary<string, RecPerformanceStatistics>();
        private readonly static object sSyncLock = new object();
        private static bool sIsDisabled = false;

        public static bool IsDisabled { get { return sIsDisabled; } }

        public static PerformanceTimer Start(string name, string detailname = "", bool addToStatistics = false)
        {
            PerformanceTimer timer = new PerformanceTimer(name, detailname, addToStatistics);
            timer.Start();
            return timer;
        }
        public static void Stop(PerformanceTimer timer)
        {
            timer.Stop();
            WriteToLog(timer);
            UpdateStatistics(timer);
        }
        public static void WriteToLog(PerformanceTimer timer)
        {
            mLog.Debug(
                "---Performance---Module: {0} ,time(s):{1:F6}. {2}",
                !string.IsNullOrEmpty(timer.DisplayName) ? timer.DisplayName : timer.Name, 
                timer.GetTimerSecond(),
                timer.AppendedMessage);
        }
        public static void SetDisable(bool isDisable)
        {
            sIsDisabled = isDisable;
        }
        public static void InitsStatistics()
        {
            sStatistics.Clear();
        }
        internal static void UpdateStatistics(PerformanceTimer timer)
        {
            if (timer.AddToStatistics)
            {
                lock (sSyncLock)
                {
                    RecPerformanceStatistics statistics = null;
                    var statisticKey = timer.Name + ":" + Thread.CurrentThread.ManagedThreadId.ToString();
                    if (!sStatistics.TryGetValue(statisticKey, out statistics))
                    {
                        statistics = new RecPerformanceStatistics();
                        sStatistics[statisticKey] = statistics;
                    }
                    long elapsed = timer.Stopwatch.ElapsedMilliseconds;
                    statistics.Count++;
                    statistics.Duration += elapsed;
                    statistics.MaxTime = elapsed > statistics.MaxTime ? elapsed : statistics.MaxTime;
                    statistics.MinTime = elapsed < statistics.MinTime ? elapsed : statistics.MinTime;
                }
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
                foreach (KeyValuePair<string, RecPerformanceStatistics> keyValue in sStatistics)
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
                root.SetAttribute("TimeStamp", DateTime.Now.ToString());
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
            }
            catch (Exception ex)
            {
                mLog.Debug("An error occurred while write performance to file.ErrorMessage:{0}.", ex.ToString());
            }
        }

        #region decompress string

        public static string GZipCompressString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = System.Text.Encoding.UTF8.GetBytes(rawString.ToString());
                byte[] zippedData = Compress(rawData);
                return (string)(Convert.ToBase64String(zippedData));
            }
        }


        static byte[] Compress(byte[] rawData)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.GZipStream compressedzipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }

        public static string GetStringByString(string Value)
        {
            //DataSet ds = new DataSet();
            string CC = GZipDecompressString(Value);
            //System.IO.StringReader Sr = new System.IO.StringReader(CC);
            //ds.ReadXml(Sr);
            return CC;
        }



        /// <summary>
        /// 将传入的二进制字符串资料以GZip算法解压缩
        /// </summary>
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>
        /// <returns>原始未压缩字符串</returns>
        public static string GZipDecompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
                return (string)(System.Text.Encoding.UTF8.GetString(Decompress(zippedData)));
            }
        }


        public static byte[] Decompress(byte[] zippedData)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(zippedData);
            System.IO.Compression.GZipStream compressedzipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress);
            System.IO.MemoryStream outBuffer = new System.IO.MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();

        }
        #endregion
    }

    internal class RecPerformanceStatistics
    {
        public RecPerformanceStatistics()
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
    public class PerformanceTimer
    {
        private Stopwatch mStopwatch;
        private string name;
        private string displayname;
        private bool addToStatistics;
        public string Name { get { return name; } }
        public string DisplayName { get { return displayname; } }
        public bool AddToStatistics { get { return addToStatistics; } }
        public string AppendedMessage { get; set; }
        public Stopwatch Stopwatch
        {
            get
            {
                return mStopwatch;
            }
        }
        public PerformanceTimer(string name, string displayName = "", bool addToStatistics = false)
        {
            this.name = name;
            this.displayname = displayName;
            this.addToStatistics = addToStatistics;
            mStopwatch = new Stopwatch();
        }
        public double GetTimerSecond()
        {
            TimeSpan tspan = mStopwatch.Elapsed;
            return tspan.TotalSeconds;
        }
        public void Start()
        {
            mStopwatch.Start();
        }
        public void Stop()
        {
            mStopwatch.Stop();
        }
    }
    public class PerformanceScope : IDisposable
    {
        private PerformanceTimer timer;
        /// <summary>
        /// Module Name used for the Method Name  (for WEB,Timer,Or api web ,It's OK to only set module name)
        /// Detail info used for the log to show the detail time span
        /// Add TO statistics for summary the time span info when schedule job finished.(important only used for JOB!)
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="detailInfo"></param>
        /// <param name="addToStatistics"></param>
        public PerformanceScope(string moduleName, string detailInfo = "", bool addToStatistics = false)
        {
            timer = PerformanceMonitor.Start(moduleName, detailInfo, addToStatistics);
        }
        public void Dispose()
        {
            PerformanceMonitor.Stop(timer);
        }

        public void AppendMessage(string message)
        {
            timer.AppendedMessage += message;
        }
    }
}
