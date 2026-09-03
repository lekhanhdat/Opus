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
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Management;

    public struct TcpPortSummary
    {
        public TcpPortSummary()
        {
            PortStateSummary = new Dictionary<string, int>();
            TimeWaitIpSummary = new Dictionary<string, int>();
        }

        public Dictionary<string, int> PortStateSummary { get; set; } 
        public Dictionary<string, int> TimeWaitIpSummary { get; set; } 

    }
   
    internal class TCPMonitor
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(TCPMonitor));
        private class TcpPortStateItem
        {
            public string? IP { get; set; }
            public string? State { get; set; }
        }
        internal TcpPortSummary GetTcpPortsInfo()
        {
            try
            {
                var cmd = "/c netstat -anop tcp";
                var proc = new System.Diagnostics.Process
                {
                    StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = cmd,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
                };
                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                var currentTime = DateTime.Now.ToLongTimeString();
                var lines = Regex.Split(output, "\r\n");
                List<TcpPortStateItem> ports = new();
                foreach (var tcpInfo in lines.Select(line => Regex.Split(line, "\\s+")).Where(tcpInfo => tcpInfo.Length == 6))
                {
                    var ip = tcpInfo[3].Split(':')[0];
                    var state = tcpInfo[4];
                    TcpPortStateItem item = new()
                    {
                        IP = ip,
                        State = state
                    };
                    tcpInfo[0] = currentTime;
                    ports.Add(item);
                }

                var waitPorts = ports.Where(t => t.State == "TIME_WAIT").GroupBy(t => $"{t.IP}_{t.State}").ToDictionary(t => t.Key, t => t.Count());
                var statePorts = ports.GroupBy(t => t.State).ToDictionary(t => t.Key, t => t.Count());
                return new TcpPortSummary
                {
                    PortStateSummary = statePorts,
                    TimeWaitIpSummary = waitPorts
                };
            }
            catch (Exception ex)
            {
                logger.Warn($"GetTcpPortsInfo failed,error:{ex}");
            }
            return default;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Reviewed by wbhu,This method have check and run it only on windows.")]
    internal sealed class OperatingSystemWindows : OperatingSystemBase, IOperatingSystem
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(OperatingSystemWindows));

        private static PerformanceCounter cpuPerformanceCounter;
        private static PerformanceCounter CPUPerformanceCounter
        {
            get
            {
                if (cpuPerformanceCounter == null)
                {
                    InitCpuPerformanceCounter();
                }
                return cpuPerformanceCounter;
            }
        }

        /// <summary>
        /// Initialize the cpu performance counter
        /// </summary>
        private static void InitCpuPerformanceCounter()
        {
            try
            {
                if (cpuPerformanceCounter == null)
                {
                    cpuPerformanceCounter = new PerformanceCounter();
                    cpuPerformanceCounter.CategoryName = "Processor";
                    cpuPerformanceCounter.CounterName = "% Processor Time";
                    cpuPerformanceCounter.InstanceName = "_Total";
                    cpuPerformanceCounter.NextValue();
                }
            }
            catch (Exception e)
            {
                logger.Warn("InitCpuPerformanceCounter Exception:{0}", e.ToString());
            }
        }

        public override int GetCPUUsage()
        {
            return Convert.ToInt32(CPUPerformanceCounter.NextValue(), NumberFormatInfo.InvariantInfo);
        }

        public override uint GetCpuFrequency()
        {
            using var rk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return Convert.ToUInt32(rk.GetValue("~MHz"));
        }

        public override string GetCpuName()
        {
            using var rk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return Convert.ToString(rk.GetValue("ProcessorNameString"));
        }

        public override Int64 GetTotalMemory()
        {
            var memSt = new Win32Native.MEMORYSTATUSEX();
            if (Win32Native.GlobalMemoryStatusEx(memSt))
            {
                return (long)memSt.TotalPhys;
            }
            throw new Win32Exception(Win32Native.GetErrorMessage(Marshal.GetLastWin32Error()));
        }

        public override UInt64 GetLeftMemory()
        {
            var memSt = new Win32Native.MEMORYSTATUSEX();
            if (Win32Native.GlobalMemoryStatusEx(memSt))
            {
                return memSt.AvailPhys;
            }
            throw new Win32Exception(Win32Native.GetErrorMessage(Marshal.GetLastWin32Error()));
        }

        public override string GetProcessCmdLine(int processId)
        {
            using (var searcher = new System.Management.ManagementObjectSearcher(string.Concat("SELECT CommandLine FROM Win32_Process WHERE ProcessId =", processId)))
            {
                var @object = searcher.Get().OfType<ManagementBaseObject>().FirstOrDefault();
                if (@object != null)
                {
                    using (@object)
                    {
                        var commandLine = @object["CommandLine"];

                        if (commandLine != null)
                        {
                            return commandLine.ToString();
                        }
                        return default;
                    }
                }
            }
            return default;
        }
        public override TcpPortSummary GetTcpPortSummary()
        {
           return new TCPMonitor().GetTcpPortsInfo();
        }

        internal static string[] ParseCommandLineInternal(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
            {
                return new string[0];
            }
            List<string> parameters = new List<string>();
            var array = commandLine.Split(' ').ToList();
            var index = 0;
            while (true)
            {
                if (index == array.Count)
                {
                    break;
                }
                if (array[index].StartsWith('"'))
                {
                    if (!array[index].EndsWith('"'))
                    {
                        var nextItem = string.IsNullOrEmpty(array[index + 1]) ? " " : array[index + 1];
                        array[index] = $"{array[index]} {nextItem}";
                        array.RemoveAt(index + 1);
                    }
                    else
                    {
                        parameters.Add(array[index].Trim('"'));
                        index++;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(array[index]))
                    {
                        parameters.Add(array[index]);
                    }
                    index++;
                }
            }
            return parameters.ToArray();
        }

        //private static string[] ParseCommandLine(string commandLine)
        //{
        //    var args = CommandLineToArgvW(commandLine, out var count);
        //    if (args == IntPtr.Zero)
        //    {
        //        throw new Win32Exception("convert arguements failed.");
        //    }

        //    try
        //    {
        //        var parameters = new string[count];
        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            var arg = Marshal.ReadIntPtr(args, i * IntPtr.Size);
        //            parameters[i] = Marshal.PtrToStringUni(arg);
        //        }
        //        return parameters;
        //    }
        //    catch (Exception)
        //    {
        //        return new string[] { commandLine};
        //    }
        //}

        /// <summary>
        /// NET6_0_OR_GREATER_windows,ARG: "ProcessLoader.exe"  "arg1" "arg2" "arg3"
        /// NET472,ARG: "ProcessLoader.exe"  "arg1" "arg2" "arg3"
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public override string[] GetProcessCommandLine(int processId)
        {
            return ParseCommandLineInternal(GetProcessCmdLine(processId));
        }

        //[DllImport("shell32.dll", SetLastError = true)]
        //static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
    }
}
