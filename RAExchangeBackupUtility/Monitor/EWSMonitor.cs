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

namespace ExchangeUtility
{
    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class EWSMonitor : ISingleton
    {
        private const int DefalutIntervalInSecond = 1 * 60;
        private static RALogger logger = RALogger.GetInstance(typeof(EWSMonitor));

        public static int IntervalInSecond { get; set; }
        public static EWSMonitorMode Mode { get; set; }
        private readonly object outputLocker = new object();
        private volatile bool monitoring = true;

        static EWSMonitor()
        {
            Mode = EWSMonitorMode.RequesRate;
            IntervalInSecond = DefalutIntervalInSecond;
        }


        private int requestNumber;
        private int lastOutputRequestNumber;
        private int errorResponseNumber;
        private int lastErrorResponseNumber;
        private DateTime startTime;
        private DateTime lastOutputTime;
        private EWSGroupMonitor gMonitor = new EWSGroupMonitor();
        private EWSGroupMonitor eMonitor = new EWSGroupMonitor();
        private EWSMonitorBlock lastBlock = new EWSMonitorBlock();

        public EWSMonitorBlock DiagnosticsInfo
        {
            get { return this.lastBlock; }
        }

        private EWSMonitor()
        {
            if (Mode == EWSMonitorMode.None) return;
            var now = DateTime.Now;
            this.startTime = now;
            this.lastOutputTime = now;
            this.requestNumber = 0;
            this.lastOutputRequestNumber = 0;
            this.errorResponseNumber = 0;
            this.lastErrorResponseNumber = 0;
            Thread t = new Thread(OutputResults);
            t.Name = "EWSMonitor";
            t.IsBackground = true;
            t.Start();
        }

        public static EWSMonitor Instance { get { return Singleton<EWSMonitor>.SingletonInstance; } }

        private void OutputResults()
        {
            try
            {
                logger.Info("EWSMonitor thread start.");
                WriteToLog("Performance Monitor: {0}", EWSMonitorBlock.HEADER);
                while (this.monitoring)
                {
                    Thread.Sleep(IntervalInSecond * 1000);
                    OutputOneRecord();
                }
            }
            catch (Exception ex)
            {
                logger.Error("EWSMonitor thread throw an exception. Error: {0}", ex);
            }
        }

        private void OutputOneRecord(bool isMonitorStopped = false)
        {
            if (this.monitoring)
            {
                lock (this.outputLocker)
                {
                    if (this.monitoring)
                    {
                        if (IsMonitorEnabledFor(EWSMonitorMode.RequesRate))
                        {
                            var block = GenerateResult();
                            this.lastBlock = block;
                            WriteToLog("Performance Monitor: {0}", block);
                        }
                        if (!isMonitorStopped) return;
                        if (IsMonitorEnabledFor(EWSMonitorMode.RequestDetails))
                        {
                            LogDetails(this.gMonitor.GetRequestInfo(), "Performance Monitor Details");
                        }
                        if (IsMonitorEnabledFor(EWSMonitorMode.RequestErrorDetails))
                        {
                            LogDetails(this.eMonitor.GetRequestInfo(), "Performance Monitor Error Details");
                        }
                    }
                }
            }
        }

        public void StopMonitor()
        {
            try
            {
                if (Mode == EWSMonitorMode.None) return;
                OutputOneRecord(true);
            }
            catch (Exception ex)
            {
                logger.Error("EWSMonitor StopMonitor throw an exception. Error: {0}", ex);
            }
            finally
            {
                this.monitoring = false;
            }
        }

        private void LogDetails(IDictionary<string, int> dictionary, string prefix)
        {
            foreach (var kv in dictionary)
            {
                WriteToLog("{0}: {1}, {2}", prefix, kv.Key, kv.Value);
            }
        }

        internal void IncreaseRequestNumber(string webServiceMethod)
        {
            if (IsMonitorEnabledFor(EWSMonitorMode.RequesRate))
            {
                Interlocked.Increment(ref this.requestNumber);
            }
            if (IsMonitorEnabledFor(EWSMonitorMode.RequestDetails))
            {
                //log request group by web service method name in the future
                this.gMonitor.IncreaseRequestGroupByKey(webServiceMethod);
            }
        }

        internal void IncreaseErrorResponseNumber(string errorCode)
        {
            if (IsMonitorEnabledFor(EWSMonitorMode.RequesRate))
            {
                Interlocked.Increment(ref this.errorResponseNumber);
            }
            if (IsMonitorEnabledFor(EWSMonitorMode.RequestErrorDetails))
            {
                this.eMonitor.IncreaseRequestGroupByKey(errorCode);
            }
        }

        private bool IsMonitorEnabledFor(EWSMonitorMode flags)
        {
            return (Mode & flags) != 0;
        }

        private EWSMonitorBlock GenerateResult()
        {
            int currentRequestNumber = this.requestNumber;
            int currentErrorResponseNumber = this.errorResponseNumber;
            DateTime currentTime = DateTime.Now;
            try
            {
                return new EWSMonitorBlock
                {
                    Full = new EWSRequestMonitorBlock()
                    {
                        TotalRequest = currentRequestNumber,
                        TotalDuration = currentTime - this.startTime,
                        ErrorResponse = currentErrorResponseNumber,
                    },
                    Incremental = new EWSRequestMonitorBlock()
                    {
                        TotalRequest = currentRequestNumber - this.lastOutputRequestNumber,
                        TotalDuration = currentTime - this.lastOutputTime,
                        ErrorResponse = currentErrorResponseNumber - this.lastErrorResponseNumber,
                    },
                };
            }
            finally
            {
                this.lastOutputRequestNumber = currentRequestNumber;
                this.lastErrorResponseNumber = currentErrorResponseNumber;
                this.lastOutputTime = currentTime;
            }
        }

        private void WriteToLog(string format, params object[] args)
        {
            logger.Info(format, args);
        }

        public class EWSRequestMonitorBlock
        {
            internal const string TO_STRING_FORMAT = "{0,10},{1,10},{2,10},{3,10},{4,10},{5,10}";
            internal static string HEADER = string.Format(TO_STRING_FORMAT, "Req", "ErrRsq", "Dur(s)", "Req/s", "ErrRsq/s", "ErrRsq%");


            public double RequestPerSecond
            {
                get
                {
                    return this.TotalRequest / this.TotalDuration.TotalSeconds;
                }
            }

            public double ErrorResponsePerSecond
            {
                get
                {
                    return this.ErrorResponse / this.TotalDuration.TotalSeconds;
                }
            }

            public double ErrorResponsePercentage
            {
                get
                {
                    return this.ErrorResponse / (double)this.TotalRequest;
                }
            }

            public int ErrorResponse { get; internal set; }

            public int TotalRequest { get; internal set; }

            public TimeSpan TotalDuration { get; internal set; }

            public override string ToString()
            {
                return string.Format(TO_STRING_FORMAT,
                    this.TotalRequest,
                    this.ErrorResponse,
                    (int)this.TotalDuration.TotalSeconds,
                    this.RequestPerSecond.ToString("0.##"),
                    this.ErrorResponsePerSecond.ToString("0.##"),
                    this.ErrorResponsePercentage.ToString("P1"));
            }
        }

        public class EWSMonitorBlock
        {
            internal const string TO_STRING_FORMAT = "{0},{1}";
            internal static string HEADER = string.Format(TO_STRING_FORMAT, EWSRequestMonitorBlock.HEADER, EWSRequestMonitorBlock.HEADER);
            public EWSRequestMonitorBlock Full { get; internal set; }
            public EWSRequestMonitorBlock Incremental { get; internal set; }

            public override string ToString()
            {
                return string.Format(TO_STRING_FORMAT, this.Full, this.Incremental);
            }
        }


    }

    class EWSGroupMonitor
    {
        private ConcurrentDictionary<string, int> internalCache = new ConcurrentDictionary<string, int>();
        public void IncreaseRequestGroupByKey(string key) 
        {
            if (string.IsNullOrEmpty(key)) key = "Unknown";
            this.internalCache.AddOrUpdate(key, 1, (keyArg, valueArg) => { return ++valueArg; });
        }
 
        public IDictionary<string,int> GetRequestInfo()
        {
            return new Dictionary<string, int>(internalCache);
        }
    }

    [Flags]
    public enum EWSMonitorMode
    {
        None = 0x0,
        RequesRate = 0x01,
        RequestDetails = 0x02,
        RequestErrorDetails = 0x04,
        All = RequesRate | RequestDetails | RequestErrorDetails,
    }
}
