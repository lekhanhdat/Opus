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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

namespace AutoInstallationCommon.Utility.Handler
{
    public class VerifyIsLocalSql
    {
        public enum CheckMode
        {
            /// <summary>
            ///     如果在用Dns.GetHostEntry获取AddressList时出错, 会捕获异常并尝试使用Dns.Resolve来获取AddressList.
            ///     如果仍然出错, 则判定无法找到AddressList, 两个host所对应的机器不相同
            ///     这个模式下, 不会因无法找到AddressList而抛出异常
            /// </summary>
            LooseMode = 0,

            /// <summary>
            ///     只使用微软建议的Dns.GetHostEntry获取AddressList,
            ///     如果出现异常, 会抛出
            /// </summary>
            StrictMode = 1,

            /// <summary>
            ///     对方法的第一个参数使用LooseMode, 对第二个参数使用StrictMode
            /// </summary>
            FirstParamStrictMode = 2
        }

        private static readonly Dictionary<string, bool> LocalInstances = new Dictionary<string, bool>();
        private static bool IsIP;

        public static string GetSqlHost(string dataSource)
        {
            var instanceName = dataSource.Split(new[] {@"\"}, StringSplitOptions.RemoveEmptyEntries)[0];
            return instanceName;
        }

        public static bool IsLocalSqlInstance(string instanceName)
        {
            if (instanceName.Equals(".", StringComparison.OrdinalIgnoreCase)) return true;

            if (LocalInstances.Keys.Contains(instanceName)) return LocalInstances[instanceName];

            var isLocal = false;
            if (GetIsCheckByIp(instanceName))
            {
                var localIPs = GetLocalIPs();
                if (localIPs.Contains(instanceName))
                    isLocal = true;
                else
                    isLocal = false;
            }
            else
            {
                isLocal = IsSameMachine(Environment.MachineName, instanceName, CheckMode.LooseMode);
            }

            lock (LocalInstances)
            {
                LocalInstances.Add(instanceName, isLocal);
            }

            return isLocal;
        }

        public static bool GetIsCheckByIp(string instanceNameString)
        {
            var str = instanceNameString.Split('.');
            if (str.Length == 4)
                IsIP = true;
            else
                IsIP = false;
            return IsIP;
        }

        public static List<string> GetLocalIPs()
        {
            var localIPs = new List<string>();
            foreach (var address in Dns.GetHostAddresses(Dns.GetHostName())) localIPs.Add(address.ToString());
            if (localIPs.Count < 1) throw new Exception("Get Local IP failed.");
            return localIPs;
        }

        /// <summary>
        ///     判断两个host（或是ip）在网络上是不是属于同一台machine
        /// </summary>
        /// <param name="host1"></param>
        /// <param name="host2"></param>
        /// <returns></returns>
        public static bool IsSameMachine(string host1, string host2, CheckMode mode)
        {
            IPAddress ip1 = null;
            IPAddress ip2 = null;
            var hostName1 = string.Empty;
            var hostName2 = string.Empty;

            var isSame = false;

            if (string.IsNullOrEmpty(host1)) throw new ArgumentNullException("host1 is null.");
            if (string.IsNullOrEmpty(host2)) throw new ArgumentNullException("host2 is null.");

            if (string.Equals(host1, host2, StringComparison.OrdinalIgnoreCase))
                isSame = true;
            else
                try
                {
                    switch (mode)
                    {
                        case CheckMode.LooseMode:
                            ip1 = GetIPAddressWithHandleException(host1);
                            ip2 = GetIPAddressWithHandleException(host2);
                            break;
                        case CheckMode.StrictMode:
                            ip1 = GetIPAddress(host1);
                            ip2 = GetIPAddress(host2);
                            break;
                        case CheckMode.FirstParamStrictMode:
                            ip1 = GetIPAddress(host1);
                            ip2 = GetIPAddressWithHandleException(host2);
                            break;
                        default:
                            break;
                    }

                    hostName1 = GetHostNameWithHandleException(host1);
                    hostName2 = GetHostNameWithHandleException(host2);
                    if (ip1 == null || ip2 == null || string.IsNullOrEmpty(hostName1) ||
                        string.IsNullOrEmpty(hostName2))
                        isSame = false;
                    else if (ip1.Equals(ip2) || string.Equals(hostName1, hostName2, StringComparison.OrdinalIgnoreCase))
                        isSame = true;
                    else if (IPAddress.IsLoopback(ip1) &&
                             string.Equals(GetHostNameWithHandleException(Dns.GetHostName()), hostName2,
                                 StringComparison.OrdinalIgnoreCase))
                        isSame = true;
                    else if (IPAddress.IsLoopback(ip2) &&
                             string.Equals(GetHostNameWithHandleException(Dns.GetHostName()), hostName1,
                                 StringComparison.OrdinalIgnoreCase)) isSame = true;
                }
                catch (Exception e)
                {
                    throw;
                }

            return isSame;
        }


        private static IPAddress GetIPAddress(string host)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(host, out ip)) ip = Dns.GetHostEntry(host).AddressList[0];

            return ip;
        }

        [SuppressMessage("FxCopCustomRules", "C100013:DoNotMissExceptionHandlingInCatchBlocks")]
        private static IPAddress GetIPAddressWithHandleException(string host)
        {
            IPAddress ip = null;
            try
            {
                ip = GetIPAddress(host);
            }
            catch (Exception ex)
            {
                try
                {
                    var dnstoip = Dns.Resolve(host);
                    return dnstoip.AddressList[0];
                }
                catch (Exception)
                {
                }
            }

            return ip;
        }

        [SuppressMessage("FxCopCustomRules", "C100013:DoNotMissExceptionHandlingInCatchBlocks")]
        private static string GetHostNameWithHandleException(string host)
        {
            try
            {
                return Dns.GetHostEntry(host).HostName;
            }
            catch (Exception e)
            {
                try
                {
                    var dnshost = Dns.Resolve(host);
                    return dnshost.HostName;
                }
                catch (Exception ex)
                {
                }

                return string.Empty;
            }
        }
    }
}