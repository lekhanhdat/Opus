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
    using AvePoint.Common;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using AvePoint.GCommon.Contract.Server.Common.Performance.Object;
    using AvePoint.GCommon.Contract.Server.Common.Performance;
    using AvePoint.Adonis.ReportCenter.Object;
    using System.Diagnostics.CodeAnalysis;
    #endregion

    /// <summary>
    /// Provide the ability of monitor the performance of the local computer
    /// </summary>
    public class PerformanceMonitorService : IPerformanceService
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Get the cpu and memory usage
        /// </summary>
        /// <returns>current cpu and memory usage</returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Cpu is ok.")]
        public CpuMemoryUsage GetCpuAndMemoryDetail()
        {
            CpuMemoryUsage result = new CpuMemoryUsage();
            try
            {
                result = new CpuMemoryUsage
                {
                    CpuUsage = OSInformation.CPUUsage,
                    HostName = Environment.MachineName,
                    TotalMemorySize = OSInformation.TotalVisibleMemorySize,
                    FreeMemorySize = OSInformation.FreePhysicalMemory
                };
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while getting CPUMemoryUsage. Details: {0}", ex);
            }
            return result;
        }

        /// <summary>
        /// Get a process detail list of the current process details
        /// </summary>
        /// <returns>the detail list of current process</returns>
        public List<ProcessDetails> GetProcessDetails()
        {
            return Singleton<ProcessController>.SingletonInstance.ProcessInformation.ConvertAll(processItem => new ProcessDetails
            {
                BasePri = processItem.BasePri,
                CPU = processItem.CPU,
                CPUTime = processItem.CPUTime,
                GDIObjects = processItem.GDIObjects,
                Handles = processItem.Handles,
                ImageName = processItem.ImageName,
                IOOther = processItem.IOOther,
                IOOtherBytes = processItem.IOOtherBytes,
                IOReadBytes = processItem.IOReadBytes,
                IOReads = processItem.IOReads,
                IOWriteBytes = processItem.IOWriteBytes,
                IOWrites = processItem.IOWrites,
                MemDelta = processItem.MemDelta,
                MemUsage = processItem.MemUsage,
                NPPool = processItem.NPPool,
                PagedPool = processItem.PagedPool,
                PageFaults = processItem.PageFaults,
                PeakMemUsage = processItem.PeakMemUsage,
                PFDelta = processItem.PFDelta,
                PID = processItem.PID,
                SessionId = processItem.SessionId,
                Threads = processItem.Threads,
                UserName = processItem.UserName,
                UserObjects = processItem.UserObjects,
                VMSize = processItem.VMSize,
            }) ?? new List<ProcessDetails>();
        }


        public CommonTopologyServer GetSystemInfo()
        {
            return new CommonTopologyServer()
            {
                CpuUsage = OSInformation.CPUUsage + "%",
                Memery = Math.Round(((double)OSInformation.TotalVisibleMemorySize / 1024 / 1024), 2) + "G",
                OS = OSInformation.OSShortName,
                TotalLocalStorage = Math.Round(((double)OSInformation.TotalDiskSize / 1024 / 1024 / 1024), 2) + "G",
                SystemType = OSInformation.Is64BitOperatingSystem ? "64 bit" : "32 bit",
                MemeryUsage = Math.Round((((double)OSInformation.TotalVisibleMemorySize - (double)OSInformation.FreePhysicalMemory) / (double)OSInformation.TotalVisibleMemorySize * 100), 2) + "%",
                LocalStorageUsage = Math.Round((((double)OSInformation.TotalDiskSize - (double)OSInformation.FreeDiskSize) / (double)OSInformation.TotalDiskSize * 100), 2) + "%",
                Processor = (Double)OSInformation.CPUHz / 1000 + "GHz",
                BytesReceivedPerSecond = OSInformation.BytesReceivedPerSecond,
                BytesSentPerSecond = OSInformation.BytesSentPerSecond,
                NetworkUsage = Math.Round(((double)OSInformation.BytesTotalPerSecond / (double)OSInformation.BytesTotalLinkSpeed), 2) + "%"
            };
        }


        public CommonNetworkConnection GetNetworkInfo(CommonNetworkConnection networkConnection)
        {
            networkConnection.InBytes = OSInformation.BytesReceivedPerSecond;
            networkConnection.OutBytes = OSInformation.BytesSentPerSecond;
            string ipOrHost = networkConnection.DestinationIP;
            Ping ping = new Ping();
            PingReply reply = null;
            try
            {
                reply = ping.Send(networkConnection.DestinationIP, 5000);
                if (reply.Status == IPStatus.Success)
                {
                    networkConnection.NetworkLatency = reply.RoundtripTime;
                }
                else
                {
                    networkConnection.NetworkLatency = -1;
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
                networkConnection.NetworkLatency = -1;
            }
            return networkConnection;
        }

        public double GetNetworkUsage()
        {
            double Utilization = 0;
            AvePoint.GCommon.Utility.PerformanceMonitor.NetWorkDetail networkDetail = new PerformanceMonitor.NetWorkDetail();
            List<NetworkAdapterDetail> details = networkDetail.GetDetails();
            foreach (NetworkAdapterDetail detail in details)
            {
                double result;
                if (Double.TryParse(detail.NetworkUtilization, System.Globalization.NumberStyles.Number, System.Globalization.NumberFormatInfo.InvariantInfo, out result))
                {
                    Utilization += result;
                }
            }
            return Utilization;
        }
    }
}
