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



namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using AvePoint.Common;
    #endregion

    internal class ProcessController : ISingleton
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Dictionary<Process, PerfRawData_PerfProc_Process> processDictionary = new Dictionary<Process, PerfRawData_PerfProc_Process>();
        Dictionary<ProcessIdentity, ProcessOldDataState> cpuDataDictionary = new Dictionary<ProcessIdentity, ProcessOldDataState>();
        Dictionary<ProcessIdentity, Int64> memDeltaDictionary = new Dictionary<ProcessIdentity, Int64>();

        private ProcessController() { }

        public List<ProcessInfo> ProcessInformation { get { return this.GetDetailsInfo(); } }

        List<ProcessInfo> GetDetailsInfo()
        {
            lock (processDictionary)
            {
                var result = new List<ProcessInfo>();
                var processCollection = Process.GetInstances();
                var performanceCollection = PerfRawData_PerfProc_Process.GetInstances();
                this.MatchProcess(processCollection, performanceCollection);
                foreach (KeyValuePair<Process, PerfRawData_PerfProc_Process> kv in this.processDictionary)
                {
                    if (kv.Value.IDProcess == 0)
                        continue;
                    var processItem = new ProcessInfo();
                    var process = default(System.Diagnostics.Process);
                    try
                    {
                        process = System.Diagnostics.Process.GetProcessById((Int32)kv.Key.ProcessId);
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                        continue;
                    }
                    try
                    {
                        System.Diagnostics.Process.EnterDebugMode();
                        processItem = this.RetrieveProcessInfo(kv, process);
                        System.Diagnostics.Process.LeaveDebugMode();
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                        continue;
                    }
                    process.Dispose();
                    result.Add(processItem);
                }
                return result;
            }
        }

        ProcessInfo RetrieveProcessInfo(KeyValuePair<Process, PerfRawData_PerfProc_Process> kv, System.Diagnostics.Process process)
        {
            var resultProcessInfo = new ProcessInfo();

            #region basepriority
            resultProcessInfo.BasePri = this.GetBasePriority(process.BasePriority);
            #endregion

            #region cpu
            ProcessIdentity processIdentity = new ProcessIdentity(kv.Key.ProcessId, kv.Key.Name);
            ProcessOldDataState processOldDataState = new ProcessOldDataState(kv.Value.PercentProcessorTime, kv.Value.Timestamp_Sys100NS);
            if (this.cpuDataDictionary.ContainsKey(processIdentity))
            {
                resultProcessInfo.CPU = GetCpuUsage(processOldDataState.OldCpuPercentProcessorTime, processOldDataState.OldTimeStampSys100Ns, this.cpuDataDictionary[processIdentity]);
                this.cpuDataDictionary[processIdentity] = processOldDataState;
            }
            else
            {
                resultProcessInfo.CPU = GetCpuUsage(processOldDataState.OldCpuPercentProcessorTime, processOldDataState.OldTimeStampSys100Ns, new ProcessOldDataState());
                this.cpuDataDictionary.Add(processIdentity, processOldDataState);
            }
            #endregion

            #region cputime
            resultProcessInfo.CPUTime = GetTimeSpan(process.TotalProcessorTime.TotalSeconds);
            #endregion

            #region  gdi object and user object
            resultProcessInfo.GDIObjects = Win32Native.GetGuiResources(process.Handle, 0).ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.UserObjects = Win32Native.GetGuiResources(process.Handle, 1).ToString(NumberFormatInfo.InvariantInfo);
            #endregion

            resultProcessInfo.Handles = process.HandleCount.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.ImageName = process.ProcessName;
            resultProcessInfo.IOOther = kv.Value.IOOtherOperationsPersec.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.IOOtherBytes = kv.Value.IOOtherBytesPersec.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.IOReadBytes = kv.Value.IOReadBytesPersec.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.IOReads = kv.Value.IOReadOperationsPersec.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.IOWriteBytes = kv.Value.IOWriteBytesPersec.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.IOWrites = kv.Value.IOWriteOperationsPersec.ToString(NumberFormatInfo.InvariantInfo);

            #region memdelta
            if (this.memDeltaDictionary.ContainsKey(processIdentity))
            {
                resultProcessInfo.MemDelta = (process.WorkingSet64 - this.memDeltaDictionary[processIdentity] / 1024).ToString(NumberFormatInfo.InvariantInfo);
                this.memDeltaDictionary[processIdentity] = process.WorkingSet64;
            }
            else
            {
                resultProcessInfo.MemDelta = "0";
                this.memDeltaDictionary.Add(processIdentity, process.WorkingSet64);
            }
            #endregion

            resultProcessInfo.MemUsage = (process.WorkingSet64 / 1024).ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.NPPool = kv.Value.PoolNonpagedBytes.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.PagedPool = kv.Value.PoolPagedBytes.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.PageFaults = kv.Key.PageFaults.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.PeakMemUsage = (process.PeakWorkingSet64 / 1024).ToString(NumberFormatInfo.InvariantInfo); ;
            resultProcessInfo.PFDelta = kv.Value.PageFaultsPersec.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.PID = process.Id.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.SessionId = process.SessionId.ToString(NumberFormatInfo.InvariantInfo);
            resultProcessInfo.Threads = kv.Key.ThreadCount.ToString(NumberFormatInfo.InvariantInfo);

            #region username
            var domainName = String.Empty;
            var userName = String.Empty;
            kv.Key.GetOwner(out domainName, out userName);
            resultProcessInfo.UserName = userName;
            #endregion
            resultProcessInfo.VMSize = (process.VirtualMemorySize64 / 1024).ToString(NumberFormatInfo.InvariantInfo);
            return resultProcessInfo;
        }

        String GetCpuUsage(UInt64 newPercentProcessorTime, UInt64 newTimeStampSys100Ns, ProcessOldDataState oldDataState)
        {
            var percentProcessorTime = default(decimal);
            var newCPU = Convert.ToDecimal(newPercentProcessorTime);
            var newNano = Convert.ToDecimal(newTimeStampSys100Ns);
            var oldCPU = Convert.ToDecimal(oldDataState.OldCpuPercentProcessorTime);
            var oldNano = Convert.ToDecimal(oldDataState.OldTimeStampSys100Ns);
            if (newNano - oldNano == 0) percentProcessorTime = 0;
            else percentProcessorTime = (((newCPU - oldCPU) / (newNano - oldNano))) * 100m;
            int count = Environment.ProcessorCount;
            decimal result = Math.Round(percentProcessorTime / count, 2);

            if (result > (decimal)100)
            {
                result = (decimal)100;
            }

            return result.ToString("N");
        }

        void MatchProcess(Process.ProcessCollection processCollection, PerfRawData_PerfProc_Process.PerfRawData_PerfProc_ProcessCollection performanceProcessCollection)
        {
            this.processDictionary.Clear();
            if (processCollection != null
                && processCollection.Count != 0
                && performanceProcessCollection != null
                && performanceProcessCollection.Count != 0)
            {
                foreach (Process item in processCollection)
                {
                    foreach (PerfRawData_PerfProc_Process perfItem in performanceProcessCollection)
                    {
                        if (item.ProcessId == perfItem.IDProcess)
                        {
                            this.processDictionary.Add(item, perfItem);
                            break;
                        }
                    }
                }
            }
        }

        String GetTimeSpan(Double totalSecond)
        {
            var seconds = 0;
            var minutes = 0;
            var hours = 0;
            if (totalSecond >= 3600)
            {
                hours = (Int32)(totalSecond / 3600);
                totalSecond = totalSecond % 3600;
                if (totalSecond >= 60)
                {
                    minutes = (Int32)(totalSecond / 60);
                    seconds = (Int32)(totalSecond % 60);
                }
                else seconds = (Int32)totalSecond;
            }
            else if (totalSecond >= 60 && totalSecond < 3600)
            {
                minutes = (Int32)(totalSecond / 60);
                seconds = (Int32)(totalSecond % 60);
            }
            else seconds = (Int32)totalSecond;
            return this.GetTimeString(hours) + ":" + this.GetTimeString(minutes) + ":" + this.GetTimeString(seconds);
        }

        String GetTimeString(Int32 value)
        {
            var result = String.Empty;
            if (value == 0) result = "00";
            else if (value > 0 && value < 10) result = "0" + value.ToString(NumberFormatInfo.InvariantInfo);
            else result = value.ToString(NumberFormatInfo.InvariantInfo);
            return result;
        }

        /// <summary>
        /// I am sure in some conditions, the base Priority is not the value that MSDN shows
        /// </summary>
        /// <param name="basePriority">the integer priority value</param>
        /// <returns>the description of the base priority</returns>
        String GetBasePriority(Int32 basePriority)
        {
            var result = String.Empty;
            if (basePriority == 0) result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.NotAvailable);
            else if (basePriority >= 1 && basePriority <= 4) result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.Low);
            else if (basePriority >= 5 && basePriority <= 7) result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.BelowNormal);
            else if (basePriority >= 8 && basePriority <= 9) result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.Normal);
            else if (basePriority >= 10 && basePriority <= 12) result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.AboveNormal);
            else if (basePriority >= 13 && basePriority <= 23) result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.High);
            else if (basePriority >= 24 && basePriority <= 31) result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.RealTime);
            else result = Enum.GetName(typeof(ProcessBasePrority), ProcessBasePrority.NotAvailable);
            return result;
        }

        struct ProcessIdentity
        {
            public UInt32 m_ProcessId;
            public String m_ProcessFriendlyName;
            public ProcessIdentity(UInt32 processId, String processFriendlyName)
            {
                this.m_ProcessId = processId;
                this.m_ProcessFriendlyName = processFriendlyName;
            }
        }

        enum ProcessBasePrority
        {
            NotAvailable = 0,
            Low = 4,
            BelowNormal = 6,
            Normal = 8,
            AboveNormal = 10,
            High = 13,
            RealTime = 24
        }

        struct ProcessOldDataState
        {
            public UInt64 OldCpuPercentProcessorTime;
            public UInt64 OldTimeStampSys100Ns;

            public ProcessOldDataState(UInt64 processorTime, UInt64 timeStampSys100Ns)
            {
                this.OldCpuPercentProcessorTime = processorTime;
                this.OldTimeStampSys100Ns = timeStampSys100Ns;
            }
        }
    }
}
