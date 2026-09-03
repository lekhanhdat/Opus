using AvePoint.GCommon;
using AvePoint.RA.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public class AveRequestStatisticMonitor
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static SortedDictionary<string, AveRequestStatistics> sRequestStatistics = new SortedDictionary<string, AveRequestStatistics>();

        private readonly static object sSyncLock = new object();
        private static bool sIsDisabled = true;

        internal static AveRequestTimer Start(string fullName)
        {
            if (sIsDisabled)
            {
                return null;
            }
            AveRequestTimer timer = new AveRequestTimer(fullName);
            timer.Start();
            return timer;
        }

        internal static void Stop(AveRequestTimer timer)
        {
            if (sIsDisabled)
            {
                return;
            }
            timer.Stop();
            UpdateStatistics(timer);
        }

        internal static void UpdateStatistics(AveRequestTimer timer)
        {
            lock (sSyncLock)
            {
                AveRequestStatistics statistics = null;
                if (!sRequestStatistics.TryGetValue(timer.FullName, out statistics))
                {
                    statistics = new AveRequestStatistics();
                    sRequestStatistics[timer.FullName] = statistics;
                }
                long elapsed = timer.Stopwatch.ElapsedMilliseconds;
                statistics.Count++;
                statistics.Duration += elapsed;
                statistics.MaxTime = elapsed > statistics.MaxTime ? elapsed : statistics.MaxTime;
                statistics.MinTime = elapsed < statistics.MinTime ? elapsed : statistics.MinTime;
            }
        }

        // use provided duration instead of timer. Ex: request exception, use retry interval instead of elapsed time
        internal static void Record(string fullName, long duration)
        {
            if (sIsDisabled)
            {
                return;
            }
            lock (sSyncLock)
            {
                AveRequestStatistics statistics = null;
                if (!sRequestStatistics.TryGetValue(fullName, out statistics))
                {
                    statistics = new AveRequestStatistics();
                    sRequestStatistics[fullName] = statistics;
                }
                statistics.Count++;
                statistics.Duration += duration;
                statistics.MaxTime = duration > statistics.MaxTime ? duration : statistics.MaxTime;
                statistics.MinTime = duration < statistics.MinTime ? duration : statistics.MinTime;
                //mLog.Debug("Record performance statistics for:{0},value:{1},count:{2}.", fullName, value, statistics.Count);
            }
        }

        public static void SetDisable(bool isDisable)
        {
            sIsDisabled = isDisable;
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
                foreach (KeyValuePair<string, AveRequestStatistics> keyValue in sRequestStatistics)
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
                string jobDir = path.Substring(0, path.LastIndexOf("\\"));
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
            catch (Exception ex)
            {
                mLog.Debug("An error occurred while write performance to file.ErrorMessage:{0}.", ex.ToString());
            }
        }

        public static void WriteRequestStatisticsResult()
        {
            if (sIsDisabled)
            {
                return;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("RequestStatisticsResults");
                long maxCount = 0;
                long maxTotal = 0;
                long maxEach = 0;
                long each;
                XmlElement maxCountEle = null;
                XmlElement maxTotalEle = null;
                XmlElement maxEachEle = null;
                foreach (KeyValuePair<string, AveRequestStatistics> keyValue in sRequestStatistics)
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
                        maxCount = keyValue.Value.Count;
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
                SetMaxAttribute(root, "maxTotal", maxTotalEle);
                SetMaxAttribute(root, "maxEach", maxEachEle);
                SetMaxAttribute(root, "maxCount", maxCountEle);

                doc.AppendChild(root);
                //mLog.Info(doc.OuterXml);
                RACustomLogger.WriteToolManyRequestLog(doc.OuterXml);
            }
            catch (Exception ex)
            {
                mLog.Debug("An error occurred while write performance to file.ErrorMessage:{0}.", ex.ToString());
            }
        }

        private static void SetMaxAttribute(XmlElement parent, string attrName, XmlElement targetEle)
        {
            if (targetEle != null)
            {
                parent.SetAttribute(attrName, string.Format("{0},{1},{2},{3},{4},{5}",
                    targetEle.Attributes["method"].Value,
                    targetEle.Attributes["total"].Value,
                    targetEle.Attributes["count"].Value,
                    targetEle.Attributes["each"].Value,
                    targetEle.Attributes["max"].Value,
                    targetEle.Attributes["min"].Value));
            }
        }
    }

    internal class AveRequestStatistics : AvePerformanceStatistics
    {
    }

    internal class AveRequestTimer : AvePerformanceTimer
    {
        public AveRequestTimer(string fullname) : base(fullname) { }
    }
}
