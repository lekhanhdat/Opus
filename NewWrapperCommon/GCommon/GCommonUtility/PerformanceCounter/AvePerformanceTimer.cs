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



namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Threading;
    using System.Reflection;
    #endregion

    public class AvePerformanceTimer
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private static long freq = 0;
        private static object freqLock = new Object();

        static long GetFrequency()
        {
            lock (freqLock)
            {
                if (freq != 0)
                {
                    return freq;
                }

                if (QueryPerformanceFrequency(out freq) == false)
                {
                    // high-performance counter not supported
                    throw new Win32Exception();
                }

                if ((freq == 0) || (freq == 1000))
                {
                    // per documentation, return 1000 means not supported
                    // that requires using GetTickCount, which will not be used for now
                    freq = 0;
                    throw new Exception("System performance frequency is invalid.");
                }

                return freq;
            }
        }

        static AvePerformanceTimer()
        {
            try
            {
                GetFrequency();
            }
            catch (Exception e)
            {
                logger.Warn("GetFrequency() exception:{0}", e.ToString());
            }
        }

        private long startTime, stopTime, durationRaw, count, maxTime, minTime, oneTime;

        public AvePerformanceTimer()
        {
            Reset();
        }

        public void Start()
        {
            if (freq == 0)
            {
                return; // not supported
            }
            stopTime = 0;
            startTime = 0;
            try
            {
                QueryPerformanceCounter(out startTime);
            }
            catch (Exception e)
            {
                logger.Warn("QueryPerformanceCounter() exception:{0}", e.ToString());
                startTime = 0;
                stopTime = 0;
            }
        }

        public void Stop()
        {
            if (freq == 0)
            {
                return; // not supported
            }
            if (startTime == 0)
            {
                return;	// not started yet
            }

            try
            {
                QueryPerformanceCounter(out stopTime);
                oneTime = stopTime - startTime;
                durationRaw += oneTime;
                maxTime = oneTime > maxTime ? oneTime : maxTime;
                minTime = oneTime < minTime ? oneTime : minTime;
                ++count;
                if (count == 1)
                {
                    minTime = maxTime;
                }
            }
            finally
            {
                startTime = 0;
                stopTime = 0;
            }
        }

        public void Reset()
        {
            startTime = 0;
            stopTime = 0;
            durationRaw = 0;
            count = 0;
            maxTime = 0;
            minTime = 0;
        }

        #region --public properties--

        // Returns the duration of the timer (in miliseconds)
        public long Duration
        {
            get
            {
                if (freq == 0)
                {
                    return 0;
                }
                return (durationRaw * 1000) / freq;
            }
        }

        public long Count
        {
            get { return count; }
        }

        public long DurationRaw
        {
            get
            {
                return durationRaw;
            }
        }

        public long MaxTime
        {
            get
            {
                if (freq == 0)
                {
                    return 0;
                }
                return (maxTime * 1000) / freq;
            }
        }

        public long MinTime
        {
            get
            {
                if (freq == 0)
                {
                    return 0;
                }
                return (minTime * 1000) / freq;
            }
        }

        public long OneTime
        {
            get
            {
                return oneTime;
            }
        }

        #endregion

    }


    /// <summary>
    /// This class cannot be used in mutilthread environment.
    /// If you want used it in recursion method, then you should do following:
    /// 
    /// void foo(void)
    /// {
    ///   do something.
    ///   foo();
    /// }
    /// 
    /// void foo(void)
    /// {
    ///   AvePerformanceTimerPool.Start("foo");
    ///   do something.
    ///   AvePerformanceTimerPool.Stop("foo");
    ///   foo();
    /// }
    /// </summary>
    public class AvePerformanceTimerPool
    {
        private static AvePerformanceTimerPoolInstance globalInstance = new AvePerformanceTimerPoolInstance("Global");
        private static AveLevelPerformanceTimerPoolInstance levelInstance = new AveLevelPerformanceTimerPoolInstance("level");

        public static void SetDisable(bool value)
        {
            globalInstance.SetDisable(value);
            levelInstance.SetDisable(value);
        }

        public static void Start(string fullName)
        {
            globalInstance.Start(fullName);
        }

        public static void Start(string fullName, string parent)
        {
            levelInstance.Start(fullName, parent);
        }

        public static void Stop(string fullName)
        {
            globalInstance.Stop(fullName);
        }

        public static void Stop(string fullName, string parent)
        {
            levelInstance.Stop(fullName, parent);
        }

        public static void Clear()
        {
            globalInstance.Clear();
            levelInstance.Clear();
        }

        public static void WriteToFile(string path, bool append = false)
        {
            globalInstance.WriteToFile(path, append);
        }

        public static void WriteTreeToFile(string path, bool append = false)
        {
            levelInstance.WriteToFile(path, append);
        }

    }

    public class AvePerformanceTimerPoolInstance
    {
        private string instanceName;
        private bool isDisabled = true;

        private readonly Dictionary<string, Dictionary<string, AvePerformanceTimer>> typeTimers = new Dictionary<string, Dictionary<string, AvePerformanceTimer>>();

        public AvePerformanceTimerPoolInstance(string instanceName)
        {
            this.instanceName = instanceName;
        }

        public void SetDisable(bool value)
        {
            isDisabled = value;
        }

        public void Start(string fullName)
        {
            ChangeState(fullName, true);
        }

        public void Stop(string fullName)
        {
            ChangeState(fullName, false);
        }

        public void Clear()
        {
            typeTimers.Clear();
        }

        public void WriteToFile(string path, bool append = false)
        {
            if (isDisabled) return;
            lock (typeTimers)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("TimeResults");
                long maxCount = 0;
                long maxDuration = 0;
                long maxEach = 0;
                XmlElement maxCountEle = null;
                XmlElement maxDurationEle = null;
                XmlElement maxEachEle = null;
                foreach (string fullName in typeTimers.Keys)
                {
                    Dictionary<string, AvePerformanceTimer> threadTimers = typeTimers[fullName];
                    if (threadTimers.Count == 0) continue;

                    long count = 0;
                    long duration = 0;
                    long each = 0;
                    long max = long.MinValue;
                    long min = long.MaxValue;
                    foreach (AvePerformanceTimer timer in threadTimers.Values)
                    {
                        count += timer.Count;
                        duration += timer.Duration;
                        max = timer.MaxTime > max ? timer.MaxTime : max;
                        min = timer.MinTime < min ? timer.MinTime : min;
                    }
                    if (count == 0) continue;
                    each = duration / count;
                    XmlElement ele = doc.CreateElement("Result");
                    ele.SetAttribute("method", fullName);
                    ele.SetAttribute("duration", duration.ToString());
                    ele.SetAttribute("count", count.ToString());
                    ele.SetAttribute("each", each.ToString());
                    ele.SetAttribute("max", max.ToString());
                    ele.SetAttribute("min", min.ToString());
                    if (maxCount < count)
                    {
                        maxCount = count;
                        maxCountEle = ele;
                    }
                    if (maxDuration < duration)
                    {
                        maxDuration = duration;
                        maxDurationEle = ele;
                    }
                    if (maxEach < each)
                    {
                        maxEach = each;
                        maxEachEle = ele;
                    }
                    root.AppendChild(ele);
                }
                root.SetAttribute("Instance", instanceName);
                root.SetAttribute("TimeStamp", DateTime.Now.ToString());
                if (maxDurationEle != null)
                {
                    root.SetAttribute("maxTotal", maxDurationEle.Attributes["method"].Value
                                                        + "," + maxDurationEle.Attributes["duration"].Value
                                                        + "," + maxDurationEle.Attributes["count"].Value
                                                        + "," + maxDurationEle.Attributes["each"].Value
                                                        + "," + maxDurationEle.Attributes["max"].Value
                                                        + "," + maxDurationEle.Attributes["min"].Value);
                }
                if (maxEachEle != null)
                {
                    root.SetAttribute("maxEach", maxEachEle.Attributes["method"].Value
                                                        + "," + maxEachEle.Attributes["duration"].Value
                                                        + "," + maxEachEle.Attributes["count"].Value
                                                        + "," + maxEachEle.Attributes["each"].Value
                                                        + "," + maxEachEle.Attributes["max"].Value
                                                        + "," + maxEachEle.Attributes["min"].Value);
                }
                if (maxCountEle != null)
                {
                    root.SetAttribute("maxCount", maxCountEle.Attributes["method"].Value
                                                        + "," + maxCountEle.Attributes["duration"].Value
                                                        + "," + maxCountEle.Attributes["count"].Value
                                                        + "," + maxCountEle.Attributes["each"].Value
                                                        + "," + maxCountEle.Attributes["max"].Value
                                                        + "," + maxCountEle.Attributes["min"].Value);
                }

                doc.AppendChild(root);
                if (append)
                {
                    using (FileStream writer = new FileStream(path, FileMode.Append))
                    {
                        byte[] buf = Encoding.UTF8.GetBytes(doc.InnerXml);
                        writer.Write(buf, 0, buf.Length);
                    }
                }
                else
                {
                    using (FileStream writer = new FileStream(path, FileMode.Create))
                    {
                        byte[] buf = Encoding.UTF8.GetBytes(doc.InnerXml);
                        writer.Write(buf, 0, buf.Length);
                    }
                }
            }
        }

        public void ChangeState(string fullName, bool toStart)
        {
            if (isDisabled) return;
            Dictionary<string, AvePerformanceTimer> threadTimers;
            lock (typeTimers)
            {
                if (!typeTimers.ContainsKey(fullName))
                {
                    typeTimers.Add(fullName, new Dictionary<string, AvePerformanceTimer>());
                }
                threadTimers = typeTimers[fullName];
            }
            AvePerformanceTimer performanceTimer;
            lock (threadTimers)
            {
                string threadId = Thread.CurrentThread.ManagedThreadId.ToString();
                if (!threadTimers.ContainsKey(threadId))
                {
                    threadTimers.Add(threadId, new AvePerformanceTimer());
                }
                performanceTimer = threadTimers[threadId];
            }
            if (toStart)
            {
                performanceTimer.Start();
            }
            else
            {
                performanceTimer.Stop();
            }
        }

    }

    public class AveLevelPerformanceTimerPoolInstance
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string instanceName;
        private bool isDisabled = true;

        private unsafe static byte* charProperties = null;
        public unsafe static byte* CharProperties
        {
            get
            {
                if (charProperties == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(XmlDocument));
                    using (UnmanagedMemoryStream unmanagedMemoryStream = (UnmanagedMemoryStream)assembly.GetManifestResourceStream("XmlCharType.bin"))
                    {
                        charProperties = unmanagedMemoryStream.PositionPointer;
                    }
                }
                return charProperties;
            }
        }

        private readonly Dictionary<string, Dictionary<string, Dictionary<string, AvePerformanceTimer>>> typeTimers = new Dictionary<string, Dictionary<string, Dictionary<string, AvePerformanceTimer>>>();

        public AveLevelPerformanceTimerPoolInstance(string instanceName)
        {
            this.instanceName = instanceName;
        }

        public void SetDisable(bool value)
        {
            isDisabled = value;
        }

        public void Start(string fullName, string parent)
        {
            ChangeState(parent, fullName, true);
        }

        public void Stop(string fullName, string parent)
        {
            ChangeState(parent, fullName, false);
        }

        public void Clear()
        {
            typeTimers.Clear();
        }

        public void ChangeState(string parent, string fullName, bool toStart)
        {
            if (isDisabled) return;
            Dictionary<string, AvePerformanceTimer> threadTimers;
            lock (typeTimers)
            {
                if (!typeTimers.ContainsKey(parent))
                {
                    typeTimers.Add(parent, new Dictionary<string, Dictionary<string, AvePerformanceTimer>>());
                }
                if (!typeTimers[parent].ContainsKey(fullName))
                {
                    typeTimers[parent].Add(fullName, new Dictionary<string, AvePerformanceTimer>());
                }
                threadTimers = typeTimers[parent][fullName];
            }
            AvePerformanceTimer performanceTimer;
            lock (threadTimers)
            {
                string threadId = Thread.CurrentThread.ManagedThreadId.ToString();
                if (!threadTimers.ContainsKey(threadId))
                {
                    threadTimers.Add(threadId, new AvePerformanceTimer());
                }
                performanceTimer = threadTimers[threadId];
            }
            if (toStart)
            {
                performanceTimer.Start();
            }
            else
            {
                performanceTimer.Stop();
            }
        }

        public void WriteToFile(string path, bool append = false)
        {
            if (isDisabled) return;
            lock (typeTimers)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("TimeResults");
                doc.AppendChild(root);
                long maxCount = 0;
                long maxDuration = 0;
                long maxEach = 0;

                Dictionary<string, XmlNode> parentCache = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
                parentCache.Add(root.Name, root);

                XmlElement maxCountEle = null;
                XmlElement maxDurationEle = null;
                XmlElement maxEachEle = null;
                foreach (string parentName in typeTimers.Keys)
                {
                    Dictionary<string, Dictionary<string, AvePerformanceTimer>> fullNameThreadTimers = typeTimers[parentName];
                    foreach (string fullName in fullNameThreadTimers.Keys)
                    {
                        Dictionary<string, AvePerformanceTimer> threadTimers = fullNameThreadTimers[fullName];
                        if (threadTimers.Count == 0) continue;

                        long count = 0;
                        long duration = 0;
                        long each = 0;
                        long max = long.MinValue;
                        long min = long.MaxValue;
                        foreach (AvePerformanceTimer timer in threadTimers.Values)
                        {
                            count += timer.Count;
                            duration += timer.Duration;
                            max = timer.MaxTime > max ? timer.MaxTime : max;
                            min = timer.MinTime < min ? timer.MinTime : min;
                        }
                        if (count == 0) continue;
                        each = duration / count;                        
                        //string mathodName = fullName.Replace(' ', '_').Replace('/', '_').Replace(':', '_').Replace('#', '_').Replace('{', '_').Replace('}', '_').Replace('(', '_').Replace(')', '_');
                        string mathodName = CheckName(fullName);
                        XmlElement ele = doc.CreateElement(mathodName);
                        ele.SetAttribute("duration", duration.ToString());
                        ele.SetAttribute("count", count.ToString());
                        ele.SetAttribute("each", each.ToString());
                        ele.SetAttribute("max", max.ToString());
                        ele.SetAttribute("min", min.ToString());
                        if (maxCount < count)
                        {
                            maxCount = count;
                            maxCountEle = ele;
                            maxCountEle.SetAttribute("method", fullName);
                        }
                        if (maxDuration < duration)
                        {
                            maxDuration = duration;
                            maxDurationEle = ele;
                            maxDurationEle.SetAttribute("method", fullName);
                        }
                        if (maxEach < each)
                        {
                            maxEach = each;
                            maxEachEle = ele;
                            maxEachEle.SetAttribute("method", fullName);
                        }
                        //string replaceParentName = parentName.Replace(' ', '_').Replace('/', '_').Replace(':', '_').Replace('#', '_').Replace('{', '_').Replace('}', '_').Replace('(', '_').Replace(')', '_');
                        string replaceParentName = CheckName(parentName);
                        XmlNode parent = null;
                        if (parentCache.TryGetValue(replaceParentName, out parent))
                        {
                            parent.AppendChild(ele);
                        }
                        parentCache[ele.Name] = ele;
                    }
                }
                root.SetAttribute("Instance", instanceName);
                root.SetAttribute("TimeStamp", DateTime.Now.ToString());
                if (maxDurationEle != null)
                {
                    root.SetAttribute("maxTotal", maxDurationEle.Attributes["method"].Value
                                                        + "," + maxDurationEle.Attributes["duration"].Value
                                                        + "," + maxDurationEle.Attributes["count"].Value
                                                        + "," + maxDurationEle.Attributes["each"].Value
                                                        + "," + maxDurationEle.Attributes["max"].Value
                                                        + "," + maxDurationEle.Attributes["min"].Value);
                }
                if (maxEachEle != null)
                {
                    root.SetAttribute("maxEach", maxEachEle.Attributes["method"].Value
                                                        + "," + maxEachEle.Attributes["duration"].Value
                                                        + "," + maxEachEle.Attributes["count"].Value
                                                        + "," + maxEachEle.Attributes["each"].Value
                                                        + "," + maxEachEle.Attributes["max"].Value
                                                        + "," + maxEachEle.Attributes["min"].Value);
                }
                if (maxCountEle != null)
                {
                    root.SetAttribute("maxCount", maxCountEle.Attributes["method"].Value
                                                        + "," + maxCountEle.Attributes["duration"].Value
                                                        + "," + maxCountEle.Attributes["count"].Value
                                                        + "," + maxCountEle.Attributes["each"].Value
                                                        + "," + maxCountEle.Attributes["max"].Value
                                                        + "," + maxCountEle.Attributes["min"].Value);
                }
                if (append)
                {
                    using (FileStream writer = new FileStream(path, FileMode.Append))
                    {
                        byte[] buf = Encoding.UTF8.GetBytes(doc.InnerXml);
                        writer.Write(buf, 0, buf.Length);
                    }
                }
                else
                {
                    using (FileStream writer = new FileStream(path, FileMode.Create))
                    {
                        byte[] buf = Encoding.UTF8.GetBytes(doc.InnerXml);
                        writer.Write(buf, 0, buf.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 按照CreateElement方法内部检测规则处理一下name，将不符合规则的char字符替换成'_'
        /// </summary>
        /// <param name="fileName">需要检测的name</param>
        /// <returns></returns>
        public string CheckName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            List<char> needReplace = new List<char>();
            bool replaceSuccess = false;
            try
            {
                unsafe
                {
                    byte* datPtr = CharProperties;
                    for (int i = 0; i < fileName.Length; i++)
                    {
                        if ((datPtr[fileName[i]] & 8) == 0)
                        {
                            needReplace.Add(fileName[i]);
                        }
                    }
                    replaceSuccess = true;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Replace performance report name failed by reflector,error:{0}", e.ToString());
                replaceSuccess = false;
            }
            if (!replaceSuccess)
            {
                needReplace.Clear();
                needReplace.Add(' ');
                needReplace.Add('/');
                needReplace.Add(':');
                needReplace.Add('#');
                needReplace.Add('{');
                needReplace.Add('}');
                needReplace.Add('(');
                needReplace.Add(')');
            }
            foreach (char tempChar in needReplace)
            {
                fileName = fileName.Replace(tempChar, '_');
            }
            return fileName;
        }
    }
}
