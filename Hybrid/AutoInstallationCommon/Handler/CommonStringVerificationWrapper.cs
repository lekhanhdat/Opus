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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net;
using System.Text.RegularExpressions;

namespace AutoInstallationCommon.Utility.Handler
{
    public class CommonStringVerificationWrapper
    {
        private static CommonStringVerificationWrapper _thisInstance;

        private CommonStringVerificationWrapper()
        {
        }

        public static CommonStringVerificationWrapper GetInstance()
        {
            return _thisInstance ?? (_thisInstance = new CommonStringVerificationWrapper());
        }

        public static bool VerifyDirectoryByRegex(string path)
        {
            var aList = GetDiskList();
            string a, b, c, d;
            a = aList[0].ToString().ToLower(CultureInfo.CurrentCulture).Substring(0, 1);
            b = aList[aList.Count - 1].ToString().ToLower(CultureInfo.CurrentCulture).Substring(0, 1);
            c = aList[0].ToString().Substring(0, 1);
            d = aList[aList.Count - 1].ToString().Substring(0, 1);
            var replacestr = a + "-" + b + c + "-" + d;
            var pattern = string.Format(@"^([a-zA-Z]:\\)[^\/\:\*\?\""\<\>\|\,]*$", replacestr);
            var objNotPositivePattern = new Regex(pattern);
            return objNotPositivePattern.IsMatch(path);
        }

        /// <summary>
        ///     验证输入路径是否是合法路径
        /// </summary>
        /// <param name="item">路径</param>
        /// <returns>true：合法 false：不合法</returns>
        public bool VerifyDirectory(string item)
        {
            var result = true;

            //if (item.Contains("\\\\") || item.Contains("/"))
            //{
            //    item = item.Replace("\\\\", "\\");
            //    item = item.Replace("/", "\\");
            //}
            if (item.Contains(@"\\")) return false;
            if (item.ToLower(CultureInfo.CurrentCulture).Trim().Split(':').Length > 2 ||
                item.ToLower(CultureInfo.CurrentCulture).Trim().Length < 3) result = false;
            if (item.ToLower(CultureInfo.CurrentCulture).Trim().Split("/*?\"<>|".ToCharArray()).Length > 1)
                result = false;
            if (!VerifyDisk(item.ToLower(CultureInfo.CurrentCulture).Trim())) result = false;

            return result;
        }

        /// <summary>
        ///     验证盘符是否正确
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool VerifyDisk(string path)
        {
            var aList = GetDiskList();
            string a, b, c, d;
            a = aList[0].ToString().ToLower(CultureInfo.CurrentCulture).Substring(0, 1);
            b = aList[aList.Count - 1].ToString().ToLower(CultureInfo.CurrentCulture).Substring(0, 1);
            c = aList[0].ToString().Substring(0, 1);
            d = aList[aList.Count - 1].ToString().Substring(0, 1);
            //string regexpath = @"^([c-zC-Z]\:|\\)\\([^\\]+\\)*[^\/:*?<>|]";
            var regexpath = @"^[c-zC-Z]:(([c-zC-Z]*)||([c-zC-Z]*\\))*";
            var replacestr = a + "-" + b + c + "-" + d;
            regexpath = regexpath.Replace("c-zC-Z", replacestr);
            var result = Regex.IsMatch(path, regexpath);
            return result;
        }

        /// <summary>
        ///     获取本机所有盘符
        /// </summary>
        /// <returns>盘符List</returns>
        private static ArrayList GetDiskList()
        {
            var query = new ManagementObjectSearcher("SELECT * From Win32_LogicalDisk");
            var queryCollection = query.Get();
            var aList = new ArrayList();
            foreach (ManagementObject mo in queryCollection)
                if (int.Parse(mo["DriveType"].ToString()) == 3)
                    aList.Add(mo["Name"]);
            return aList;
        }

        /// <summary>
        ///     端口格式是否正确
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool ValidatePort(string port)
        {
            var result = false;
            //检查是否为空
            if (!string.IsNullOrEmpty(port))
            {
                port = port.Trim();
                if (ValidateNumber(port)) //检查是否是数字
                {
                    var portNumber = Convert.ToInt32(port);

                    result = ValidateIntegerPort(portNumber);
                }
            }

            return result;
        }

        /// <summary>
        ///     Verify a string is a number (0,1,2,3,...) or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true is a number / false: is not</returns>
        public bool ValidateNumber(string item)
        {
            if (string.IsNullOrEmpty(item)) return false;
            item = item.Trim();
            if (item.Length != 0)
            {
                if ((item[0] == (char) 48 || item[0] == (char) 65296) && item.Length != 1) return false;
                for (var i = 0; i < item.Length; i++)
                    if (!char.IsNumber(item, i))
                        return false;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     验证int是否为合法Port
        /// </summary>
        /// <param name="portNumber"></param>
        /// <returns></returns>
        private bool ValidateIntegerPort(int portNumber)
        {
            var result = false;

            if (portNumber >= 1 && portNumber <= 65535) //检查范围是否在1-65535之间
                result = true;
            return result;
        }

        /// <summary>
        ///     判断主机名是否是合法主机名
        /// </summary>
        /// <param name="item">host</param>
        /// <returns>true:合法 false：不合法</returns>
        public bool VerifyHostName(string item)
        {
            //主机名不能为纯数字
            if (!ValidateNumber(item))
                try
                {
                    var hostInfo = Dns.GetHostEntry(item);
                    return true;
                }
                catch
                {
                    return false;
                }

            return false;
        }

        /// <summary>
        ///     判断stringItem是否是本地Host
        /// </summary>
        /// <param name="item">host</param>
        /// <returns>true：是 false：不是</returns>
        public bool VerifyLocalHost(string item)
        {
            string host;

            host = Dns.GetHostName();

            if (item != null)
            {
                if (IsLocalHostName(item, host)) return true;

                if (item.EndsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase)) return true;
                var allLocalIP = Dns.GetHostAddresses(host);
                foreach (var localIP in allLocalIP)
                    if (item.Equals(localIP.ToString()))
                        return true;

                #region ==== Cluster ===

                return IsWindowsClusterHostOrIP(item);

                #endregion
            }

            return false;
        }

        private bool IsWindowsClusterHostOrIP(string item)
        {
            var regWrapper = CommonRegistryWrapper.GetInstance();
            if (!regWrapper.Exists(@"HKEY_LOCAL_MACHINE\Cluster", "ClusterName")) return false;
            var clusterName = regWrapper.GetValue(@"HKEY_LOCAL_MACHINE\Cluster", "ClusterName");

            if (item.Trim().Equals(clusterName, StringComparison.OrdinalIgnoreCase)) return true;

            var objIPHost = Dns.Resolve(Environment.MachineName);

            foreach (var ip in objIPHost.AddressList)
                if (item.Trim().Equals(ip.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;

            return true;
        }

        private bool IsLocalHostName(string item, string host)
        {
            return item.Trim().Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                   item.Equals(host, StringComparison.OrdinalIgnoreCase) ||
                   item.Equals(GetFullComputerName(), StringComparison.OrdinalIgnoreCase);
        }

        private string GetFullComputerName()
        {
            string fullComputerName = null;
            try
            {
                var query = new SelectQuery("Win32_ComputerSystem");
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject mo in searcher.Get())
                        try
                        {
                            var str1 = mo["DNSHostName"].ToString();
                            if ((bool) mo["partofdomain"] != true)
                            {
                                fullComputerName = str1;
                            }
                            else
                            {
                                var str2 = mo["domain"].ToString();
                                fullComputerName = str1 + "." + str2;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                }
            }
            catch (Exception ex)
            {
            }

            return fullComputerName;
        }

        /// <summary>
        ///     判断端口是否被占用
        /// </summary>
        /// <param name="item">端口号</param>
        /// <returns> true：未被占用  false：被占用</returns>
        public bool VerifyPort(string item)
        {
            try
            {
                if (ValidatePort(item))
                {
                    //get local ip
                    var ipList = GetLocalIPList();
                    //get ip+port string
                    var p = GetActiveConnections();
                    var result = p.StandardOutput.ReadToEnd().ToLower(CultureInfo.CurrentCulture);
                    var portList = GetPortList(ipList, result);
                    return ScanPort(item, portList);
                }

                return false;
            }
            catch (Exception ex)
            {
                //TODO LOG
                return false;
            }
        }

        private bool ScanPort(string item, List<string> portList)
        {
            if (portList.Contains(item)) return false;
            return true;
        }


        /// <summary>
        ///     获取本机IP列表
        /// </summary>
        /// <returns>IP列表</returns>
        private List<string> GetLocalIPList()
        {
            var ip1 = "127.0.0.1";
            var ip2 = "0.0.0.0";
            var ip3 = "::";
            var ip4 = "::1";
            var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            var ipList = new List<string> {ip1, ip2, ip3, ip4};
            for (var i = 0; i < addressList.Length; i++) ipList.Add(addressList[i].ToString());
            return ipList;
        }


        /// <summary>
        ///     获取本机端口
        /// </summary>
        /// <returns></returns>
        private Process GetActiveConnections()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo("NETSTAT", "-AN")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true
            };
            p.Start();
            return p;
        }

        private List<string> GetPortList(List<string> ipList, string result)
        {
            var portList = new List<string>();

            var item = result.Split(' ');
            for (var loop = 0; loop < item.Length; loop++)
                foreach (var ip in ipList)
                    if (item[loop].IndexOf(ip, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var tempPort = item[loop].Split(':');
                        if (!"0".Equals(tempPort[tempPort.Length - 1])) portList.Add(tempPort[tempPort.Length - 1]);
                    }

            return portList;
        }

        public bool ValidateWAUsernameFormat(string username)
        {
            if (username.Contains("\\"))
            {
                var items = username.Split('\\');
                if (items.Length != 2) return false;
                if (string.IsNullOrEmpty(items[0]) || string.IsNullOrEmpty(items[1])) return false;
                return true;
            }

            return false;
        }
    }
}