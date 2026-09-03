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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;
    using System.Linq;
    #endregion

    /// <summary>
    /// This class is to provide some OS information
    /// </summary>
    public static class OSInformation
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static PerformanceCounter cpuPerformanceCounter;
        static List<NetworkInterface> opeationalNetworkAdapterList;
        static List<NetworkMonitorInfo> monitorNetworkAdapterList;

        private static readonly object _locker = new object();
        private static IOperatingSystem mOperatingSubSystem { get; set; }
        internal static IOperatingSystem OperatingSubSystem
        {
            get
            {
                lock (_locker)
                {
                    if (mOperatingSubSystem == null)
                    {
                        mOperatingSubSystem = GetOperatingSubSystem();
                    }
                    return mOperatingSubSystem;
                }
            }
        }
        private static OperatingSystemInfo osInfo;

        static OSInformation()
        {
            try
            {
                GetSystemInformation();
            }
            catch (Exception e)
            {
                logger.Warn("GetSystemInformation Exception:{0}", e.ToString());
            }
            try
            {
                EnumerateMonitorNetworkAdapter();
            }
            catch (Exception e)
            {
                logger.Warn("EnumerateMonitorNetworkAdapter Exception:{0}", e.ToString());
            }
            try
            {
                InitCpuPerformanceCounter();
            }
            catch (Exception e)
            {
                logger.Warn("InitCpuPerformanceCounter Exception:{0}", e.ToString());
            }
        }

        private static IOperatingSystem GetOperatingSubSystem()
        {
            if (System.OperatingSystem.IsLinux())
            {
                return new OperatingSystemLinux();
            }
            else if (System.OperatingSystem.IsWindows())
            {
                return new OperatingSystemWindows();
            }
            else
            {
                throw new PlatformNotSupportedException("Current platform is not windows or linux,don't support now.");
            }
        }
        private static void GetOSInformation()
        {
            try
            {
                osInfo = OperatingSubSystem.GetOSInfo();
            }
            catch (Exception e)
            {
                logger.Warn("GetOSInformations Exception:{0}", e.ToString());
            }
        }

        /// <summary>
        /// UAC enabled after Vista
        /// </summary>
        public static Boolean UACEnabled { get { return OSVersionNumber >= 60; } }

        /// <summary>
        /// this property return OS version number, 6.1 return 61
        /// </summary>
        public static Int32 OSVersionNumber { get { return Environment.OSVersion.Version.Major * 10 + Environment.OSVersion.Version.Minor; } }

        /// <summary>
        /// this property return OS name
        /// </summary>
        public static String OSName { get { return osInfo.Name; } }

        /// <summary>
        /// this property return OS short name
        /// </summary>
        public static String OSShortName { get { return osInfo.ShortName; } }

        /// <summary>
        /// this property return CPU Hz
        /// </summary>
        public static UInt32 CPUHz { get { return osInfo.CpuHz; } }

        /// <summary>
        /// this property return current CPU usage.
        /// <remarks>
        /// NOTE: if we initialize the cpu performance counter, this instance will not
        /// be released until the process is terminated. The reason for this is the
        /// NextValue method, you should invoke the NextValue twice before you get
        /// the cpu usage
        /// </remarks>
        /// </summary>
        public static Int32 CPUUsage { get { return OperatingSubSystem.GetCPUUsage(); } }

        /// <summary>
        /// Gets the number of processors on the current machine.
        /// The 32-bit signed integer that specifies the number of processors on the
        /// current machine. There is no default.
        /// </summary>
        public static Int32 CPUCount { get { return Environment.ProcessorCount; } }

        /// <summary>
        /// The function determines whether the current operating system is a
        /// 64-bit operating system.
        /// </summary>
        /// <returns>
        /// The function returns true if the operating system is 64-bit;
        /// otherwise, it returns false.
        /// </returns>
        public static Boolean Is64BitOperatingSystem { get { return IntPtr.Size == 8 ? 1 < 2 : IsWow64Process; } }

        /// <summary>
        /// To judge if the current process is a wow64 process.
        /// </summary>
        /// <returns>the check result</returns>
        public static Boolean IsWow64Process
        {
            get
            {
                // 32-bit programs run on both 32-bit and 64-bit Windows
                // Detect whether the current process is a 32-bit process
                // running on a 64-bit system.
                var result = default(Boolean);
                var currentProcessPtr = Win32Native.GetCurrentProcess();
                result = (DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && Win32Native.IsWow64Process(currentProcessPtr, out result)) && result;
                Win32Native.CloseHandle(currentProcessPtr);
                return result;
            }
        }

        /// <summary>
        /// To judge if the current process is 64 bit process.
        /// </summary>
        public static Boolean Is64BitProcess { get { return IntPtr.Size == 8; } }

        /// <summary>
        /// To get the total disk size
        /// </summary>
        public static UInt64 TotalDiskSize { get { return GetTotalDiskSize(); } }

        /// <summary>
        /// To get the free disk size
        /// </summary>
        public static UInt64 FreeDiskSize { get { return GetFreeDiskSize(); } }

        /// <summary>
        /// To get network information list
        /// </summary>
        public static List<NetworkInfo> NetworkInfos { get { return GetNetworkInfoList(); } }

        /// <summary>
        /// the bytes sent per second
        /// </summary>
        public static Int64 BytesSentPerSecond
        {
            get
            {
                var resultSpeed = default(Int64);
                var networkInfoList = GetNetworkInfoList();
                if (networkInfoList != null)
                    networkInfoList.ForEach(info => resultSpeed += info.SentSpeed);
                return resultSpeed;
            }
        }

        /// <summary>
        /// the bytes received per second
        /// </summary>
        public static Int64 BytesReceivedPerSecond
        {
            get
            {
                var resultSpeed = default(Int64);
                var networkInfoList = GetNetworkInfoList();
                if (networkInfoList != null)
                    networkInfoList.ForEach(info => resultSpeed += info.ReceivedSpeed);
                return resultSpeed;
            }
        }

        /// <summary>
        /// the bytes total per second
        /// </summary>
        public static Int64 BytesTotalPerSecond
        {
            get
            {
                var resultSpeed = default(Int64);
                var networkInfoList = GetNetworkInfoList();
                if (networkInfoList != null)
                    networkInfoList.ForEach(info => resultSpeed += info.TotalSpeed);
                return resultSpeed;
            }
        }

        /// <summary>
        /// the network total link speed
        /// </summary>
        public static Int64 BytesTotalLinkSpeed
        {
            get
            {
                var resultSpeed = default(Int64);
                var networkInfoList = GetNetworkInfoList();
                if (networkInfoList != null)
                    networkInfoList.ForEach(info => resultSpeed += info.Speed);
                return resultSpeed;
            }
        }

        /// <summary>
        /// Get current machine IP Host entry
        /// </summary>
        public static IPHostEntry HostEntry { get { return GetHostEntry(Dns.GetHostName()); } }

        /// <summary>
        /// Get one available tcp port of current system
        /// </summary>
        public static Int32 AvailableTcpPortInLocalhost { get { return GetOneAvailableTcpPortInLocalhost(); } }

        /// <summary>
        /// get current host name
        /// </summary>
        public static String HostName { get { return HostEntry.HostName; } }

        /// <summary>
        /// Get all aliases name of current system
        /// </summary>
        public static String[] HostNameAliases { get { return HostEntry.Aliases; } }

        public static Boolean IsDebuggerAttached { get { return Debugger.IsAttached; } }
        public static Boolean IsConsoleAttached { get { return Console.In != StreamReader.Null; } }

        /// <summary>
        /// Evaluate current system tcp connections. This is the same information provided
        /// by the netstat command line application, just in .Net strongly-typed object
        /// form.  We will look through the list, and if our port we would like to use
        /// in our TcpClient is occupied, we will set isAvailable to false.</summary>
        /// <param name="tcpPort">tcp port
        /// <remarks>
        /// Tcp port should be greater than 1024 and lower than 65536
        /// </remarks>
        /// </param>
        /// <returns></returns>
        public static Boolean IsTcpPortAvailableTcpPort(Int32 tcpPort)
        {
            return !Array.Exists(IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners(), endpoint => endpoint.Port == tcpPort);
        }

        /// <summary>
        /// Provide the ability of getting specific host or address's host entry
        /// </summary>
        /// <param name="hostNameOrIPAddress">host or ip address</param>
        /// <returns>the result ip host entry object</returns>
        public static IPHostEntry GetHostEntry(String hostNameOrIPAddress)
        {
            return Dns.GetHostEntry(hostNameOrIPAddress);
        }

        public static Int32 GetOneAvailableTcpPortInLocalhost()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                tempSocket.Bind(endPoint);
                var ipEndPoint = tempSocket.LocalEndPoint as IPEndPoint;
                return ipEndPoint.Port;
            }
        }

        public static Boolean HideConsole()
        {
            return Win32Native.FreeConsole();
        }

        public static Boolean ShowConsole()
        {
            return Win32Native.AllocConsole();
        }

        /// <summary>
        /// get all the network information
        /// </summary>
        /// <returns>the network information list</returns>
        static List<NetworkInfo> GetNetworkInfoList()
        {
            var resultNetworkInfoList = new List<NetworkInfo>();
            if (monitorNetworkAdapterList != null)
            {
                monitorNetworkAdapterList.ForEach(adapter => resultNetworkInfoList.Add(new NetworkInfo
                {
                    AdapterName = adapter.AdapterName,
                    AdapterDescription = adapter.AdapterName,
                    Speed = adapter.Speed,
                    SentSpeed = Convert.ToInt64(adapter.SentPerformanceCounter.NextValue()),
                    ReceivedSpeed = Convert.ToInt64(adapter.ReceivedPerformanceCounter.NextValue()),
                    TotalSpeed = Convert.ToInt64(adapter.TotalPerformanceCounter.NextValue()),
                }));
            }
            return resultNetworkInfoList;
        }

        /// <summary>
        /// Initialize the cpu performance counter
        /// </summary>
        static void InitCpuPerformanceCounter()
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

        /// <summary>
        /// The function determines whether a method exists in the export
        /// table of a certain module.
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        /// <param name="methodName">The name of the method</param>
        /// <returns>
        /// The function returns true if the method specified by methodName
        /// exists in the export table of the module specified by moduleName.
        /// </returns>
        static Boolean DoesWin32MethodExist(String moduleName, String methodName)
        {
            var result = default(Boolean);
            var moduleHandle = Win32Native.GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
                result = false;
            else
            {
                var processHandle = Win32Native.GetProcAddress(moduleHandle, methodName);
                result = processHandle != IntPtr.Zero;
                Win32Native.CloseHandle(moduleHandle);
                Win32Native.CloseHandle(processHandle);
            }
            return result;
        }

        /// <summary>
        /// use WMI class win32_operationSystem to get the name and CSDVersion of the system,
        /// get total memory size. Get cpu name and HZ number
        /// </summary>
        /// <remarks>
        /// Notes, we use WMI namespace root\cimv2 here because in some Windows 2003 systems,
        /// the default WMI namespace is the root\default, not the cimv2. So be careful for
        /// this. Also, on windows 2003 server, the win32_operating system does not has the
        /// OSArchitecture attribute. so we use the IS64BitsOperatingSystem instead.
        /// </remarks>
        static void GetSystemInformation()
        {
            GetOSInformation();
            opeationalNetworkAdapterList = GetAllOperationalAdapters();
        }

        static List<NetworkInterface> GetAllOperationalAdapters()
        {
            var result = new List<NetworkInterface>();
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up
                    && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback
                    && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    result.Add(networkInterface);
            }
            return result;
        }

        static UInt64 GetTotalDiskSize()
        {
            var resultTotalDiskSize = default(Int64);
            foreach (var diskDrive in DriveInfo.GetDrives())
            {
                if (diskDrive.DriveType == DriveType.Fixed)
                    resultTotalDiskSize += diskDrive.TotalSize;
            }
            return Convert.ToUInt64(resultTotalDiskSize, NumberFormatInfo.InvariantInfo);
        }

        public static UInt64 GetLeftMemory()
        {
            return OperatingSubSystem.GetLeftMemory();
        }

        /// <summary>
        /// travel all the logical disk to get the
        /// </summary>
        /// <returns></returns>
        static UInt64 GetFreeDiskSize()
        {
            var resultFreeSize = default(Int64);
            foreach (var diskDrive in DriveInfo.GetDrives())
            {
                if (diskDrive.DriveType == DriveType.Fixed)
                    resultFreeSize += diskDrive.TotalFreeSpace;
            }
            return Convert.ToUInt64(resultFreeSize, NumberFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Enumerate the network adapter.
        /// </summary>
        static void EnumerateMonitorNetworkAdapter()
        {
            if (monitorNetworkAdapterList == null)
            {
                monitorNetworkAdapterList = new List<NetworkMonitorInfo>();
                var networkPerformanceCounterCategory = new PerformanceCounterCategory("Network Interface");
                string[] instanceNames;
                try
                {
                    instanceNames = networkPerformanceCounterCategory.GetInstanceNames();
                }
                catch (Exception e)
                {
                    logger.Warn(e.ToString());
                    Thread.Sleep(500);
                    try
                    {
                        instanceNames = networkPerformanceCounterCategory.GetInstanceNames();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.ToString());
                        Thread.Sleep(500);
                        instanceNames = networkPerformanceCounterCategory.GetInstanceNames();
                    }
                }
                foreach (string instanceName in instanceNames)
                {
                    var operationalNetworkAdapter = opeationalNetworkAdapterList.Find(adapter => CheckAdapter(adapter, instanceName));
                    if (operationalNetworkAdapter != null)
                    {
                        monitorNetworkAdapterList.Add(new NetworkMonitorInfo
                        {
                            AdapterName = instanceName,
                            AdapterDescription = operationalNetworkAdapter.Description,
                            Speed = operationalNetworkAdapter.Speed,
                            ReceivedPerformanceCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName),
                            SentPerformanceCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName),
                            TotalPerformanceCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", instanceName)
                        });
                    }
                };
            }
        }

        /// <summary>
        /// Check the adapter is the proper of performance counter instance name
        /// </summary>
        /// <param name="adapter">a adapter to be checked</param>
        /// <param name="instanceName">the performance counter instance name</param>
        /// <returns>the check result</returns>
        static Boolean CheckAdapter(NetworkInterface adapter, String instanceName)
        {
            var checkResult = default(Boolean);
            var filtedAdapterName = adapter.Description.Trim().Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace("-", "").Replace("_", "").Replace("/", "");
            var filtedInstanceName = instanceName.Trim().Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace("-", "").Replace("_", "").Replace("/", "");
            if (filtedAdapterName.Equals(filtedInstanceName, StringComparison.OrdinalIgnoreCase)) checkResult = true;
            return checkResult;
        }
    }
}