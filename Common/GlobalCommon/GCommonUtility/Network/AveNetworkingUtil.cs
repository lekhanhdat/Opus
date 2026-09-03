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
    using System.ComponentModel;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using DWORD = System.UInt32;
    using LPWSTR = System.String;
    using NET_API_STATUS = System.UInt32;
    #endregion

    /// <summary>
    /// Provide the function to check network configuration and do net share connection
    /// </summary>
    public static class AveNetworkingUtil
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// check if the hostOrIP is point to local machine
        /// </summary>
        /// <param name="hostOrIP"></param>
        /// <returns></returns>
        public static bool IsLocalAddress(string hostOrIP)
        {
            if (hostOrIP.Equals(Dns.GetHostName(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            string localHostName = Dns.GetHostName();
            if (string.Equals(hostOrIP, localHostName))
            {
                return true;
            }
            IPAddress[] localIps = Dns.GetHostAddresses(localHostName);
            foreach (IPAddress ip in localIps)
            {
                if (string.Compare(ip.ToString(), hostOrIP, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            IPAddress[] hostIps = Dns.GetHostAddresses(hostOrIP);
            foreach (IPAddress hostIp in hostIps)
            {
                if (IPAddress.IsLoopback(hostIp))
                {
                    return true;
                }
                if (string.Compare(hostIp.ToString(), hostOrIP, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    continue;
                }
                foreach (IPAddress localIp in localIps)
                {
                    if (hostIp.Equals(localIp))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// setup net share
        /// </summary>
        /// <param name="remotepath">UNC路径。</param>
        /// <param name="username">用户名。</param>
        /// <returns></returns>
        public static int NetUse(string remotepath, string username, string password)
        {
            var state = CancelNetUse(remotepath);
            logger.Info("Cancel net user state is {0}", state);
            Win32Native.NETRESOURCEW[] n = new Win32Native.NETRESOURCEW[1];
            n[0] = new Win32Native.NETRESOURCEW();
            n[0].dwType = 1;
            n[0].lpLocalName = null;
            n[0].lpRemoteName = remotepath;
            n[0].lpProvider = null;

            return Win32Native.WNetAddConnection2W(n, password, username, 1);
        }

        /// <summary>
        /// Cancel net share.
        /// </summary>
        /// <param name="remotepath">UNC路径。</param>
        public static int CancelNetUse(string remotepath)
        {
            return Win32Native.WNetCancelConnection2(remotepath, 0, true);
        }

        /// <summary>
        /// Retrieves the local path on the given server and share name.
        /// </summary>
        /// <remarks>If remote server, should use AveImpersonator to impersonate</remarks>
        public static string GetNetShareLocalPath(string serverName, string netShareName)
        {
            string path = null;
            IntPtr ptr = IntPtr.Zero;
            int errCode = Win32Native.NetShareGetInfo(serverName, netShareName, 2, ref ptr);
            if (errCode != 0)
            {
                throw new Win32Exception(errCode);
            }

            Win32Native.SHARE_INFO shareInfo = (Win32Native.SHARE_INFO)Marshal.PtrToStructure(ptr, typeof(Win32Native.SHARE_INFO));
            path = shareInfo.shi2_path;
            Win32Native.NetApiBufferFree(ptr);
            return path;
        }

        /// <summary>
        /// 获取网络接口信息
        /// </summary>
        /// <param name="hostOrIPAddress">host name or ip address</param>
        /// <returns></returns>
        public static AveNetworkInterfaceInformation GetNetworkInterfaceInformation(string hostOrIPAddress)
        {
            AveNetworkInterfaceInformation networkInterfaceInformation = new AveNetworkInterfaceInformation();

            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                if (interfaces != null && interfaces.Length > 0)
                {
                    NetworkInterface relatedNetworkInterface = null;
                    if (string.IsNullOrEmpty(hostOrIPAddress))
                    {
                        relatedNetworkInterface = interfaces[0];
                    }
                    else
                    {
                        IPAddress[] ipAddresses = Dns.GetHostAddresses(hostOrIPAddress);
                        if (ipAddresses.Length > 0)
                        {
                            IPAddress ipAddress = ipAddresses[0];
                            foreach (NetworkInterface networkInterface in interfaces)
                            {
                                foreach (UnicastIPAddressInformation ipAddrInfo in networkInterface.GetIPProperties().UnicastAddresses)
                                {
                                    if (ipAddrInfo.Address.ToString().Equals(ipAddress.ToString(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        relatedNetworkInterface = networkInterface;
                                        break;
                                    }
                                }
                                if (relatedNetworkInterface != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (relatedNetworkInterface == null) return null;

                    networkInterfaceInformation.NetWorkInterfaceAdapterCaption = relatedNetworkInterface.Description;
                    if (string.IsNullOrEmpty(networkInterfaceInformation.NetWorkInterfaceAdapterCaption))
                    {
                        networkInterfaceInformation.NetWorkInterfaceAdapterCaption = relatedNetworkInterface.Name;
                    }
                    networkInterfaceInformation.NetworkBandWidth = relatedNetworkInterface.Speed;

                    IPv4InterfaceStatistics interfaceStatistics = relatedNetworkInterface.GetIPv4Statistics();
                    long bytesSent = interfaceStatistics.BytesSent;
                    long bytesReceived = interfaceStatistics.BytesReceived;
                    Thread.Sleep(1000);
                    interfaceStatistics = relatedNetworkInterface.GetIPv4Statistics();
                    bytesSent = interfaceStatistics.BytesSent - bytesSent;
                    bytesReceived = interfaceStatistics.BytesReceived - bytesReceived;

                    networkInterfaceInformation.NetworkSentSpeed = bytesSent / 1024;
                    networkInterfaceInformation.NetworkReceivedSpeed = bytesReceived / 1024;
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting network interface information. {0}", ex.ToString());
            }
            return networkInterfaceInformation;
        }
        /// <summary>
        /// 用来根据ip获取机器名或根据机器名取ip
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string GetIpOrHost(string address)
        {
            logger.Debug("begin to convert host name , {0}", address);
            try
            {
                IPAddress ip = null;
                bool isIp = IPAddress.TryParse(address, out ip);
                IPHostEntry hostinf = Dns.GetHostEntry(address);
                if (isIp)
                {
                    logger.Debug("address is ip , host = {0}", hostinf.HostName);
                    return hostinf.HostName;
                }
                else if (hostinf.AddressList.Length > 0)
                {
                    logger.Debug("address is host , ip = {0}", hostinf.AddressList[0].ToString());
                    return hostinf.AddressList[0].ToString();
                }
                else
                {
                    logger.Info("address has no ip , {0}", address);
                }
            }
            catch (Exception e)
            {
                logger.Error("fail to convert this address ", e);
            }
            return address;
        }

        public static string GetV4IpAddress(string hostName)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return address.ToString();
                }
            }
            return hostName;
        }

    }
}
