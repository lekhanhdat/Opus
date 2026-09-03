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
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using AvePoint.GCommon;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;


namespace AvePoint.Common.SQLServer
{
    [AveVersion("$Revision: 431116 $")]
    public class SQLServerInstanceCollection
    {
        public SortedList instances { get; set; }
        private int mFilterServiceStatus = 0;
        public int FilterServiceStatus
        {
            get { return mFilterServiceStatus; }
            set { mFilterServiceStatus = value; }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Collections.CaseInsensitiveComparer.#ctor")]
        public SQLServerInstanceCollection()
        {
            instances = new SortedList(new CaseInsensitiveComparer());
        }
        public bool Initialize()
        {
            GetInstanceNames();
            for (int i = 0; i < instances.Count; i++)
            {
                ((SQLServerInstance)instances[instances.GetKey(i)]).GetInstanceInfo();
            }
            return true;
        }

        public void GetInstanceNames()
        {
            if (SQLServerInstance.CheckRegedit(@"SOFTWARE\Microsoft", "Microsoft SQL Server", true))
            {
                SearchInstances(@"SOFTWARE\Microsoft\Microsoft SQL Server", FilterServiceStatus);
            }
            if (SQLServerInstance.CheckRegedit(@"SOFTWARE\Wow6432Node\Microsoft", "Microsoft SQL Server", true))
            {
                SearchInstances(@"SOFTWARE\Wow6432Node\Microsoft\Microsoft SQL Server", FilterServiceStatus);
            }
        }
        public void SearchInstances(string key, int status)
        {
            RegistryKey rk = null;
            if (SQLServerInstance.CheckRegedit(key, "InstalledInstances", false))
            {
                rk = Registry.LocalMachine.OpenSubKey(key);
                String[] subkeys = (String[])rk.GetValue("InstalledInstances");
                if (subkeys.Length > 0)
                {
                    foreach (String n in subkeys)
                    {
                        string serviceName = string.Empty;
                        if (n.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                        {
                            serviceName = "MSSQLSERVER";
                        }
                        else
                        {
                            serviceName = "MSSQL$" + n;
                        }
                        ServiceController sc = new ServiceController(serviceName);
                        //ServiceController sc = new ServiceController("MSSQL$" + n);
                        if (status == 0 || sc.Status == (ServiceControllerStatus)status)
                        {
                            if (n.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                            {
                                instances.Add(n, new SQLServerInstance(n, key));
                            }
                            else if (!n.Equals("SQLEXPRESS", StringComparison.OrdinalIgnoreCase) && !n.Equals("Microsoft##SSEE", StringComparison.OrdinalIgnoreCase))
                            {
                                instances.Add(n, new SQLServerInstance(n, key));
                            }
                        }
                        sc.Close();
                    }
                }
                rk.Close();
            }
        }
    }
    [AveVersion("$Revision: 431116 $")]
    public class SQLServerInstance
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string instance { get; set; }//if default instance,value is MSSQLSERVER
        public string edition { get; set; }//such as enterprise edition
        public string version { get; set; }//such as 9.0...
        public string location { get; set; }//such as MSSQL.1  MSSQL.2 in sql server 2005
        public string path { get; set; } //such as SOFTWARE\Microsoft\Microsoft SQL Server

        /// <summary>
        /// if SQL instance listens on all IP address
        /// </summary>
        public bool TCPListenIPAll { get; set; }

        /// <summary>
        /// the TCP/IP port which the SQL instance listens, when SQL instance listens on all IP address
        /// </summary>
        public uint TCPPort { get; set; }

        /// <summary>
        /// if 1433 would be used for default instance, and dynamic ports would be used for named instance.
        /// </summary>
        public bool IsDefaultTCPPortUsed { get; set; }

        private bool mIsCluster = false;
        public bool isCluster
        {
            get { return mIsCluster; }
            set { mIsCluster = value; }
        }
        public string cluster { get; set; }//the cluster name in cluster environment
        public string sqlDataRootFolder { get; set; }//such as C:\Program Files\Microsoft SQL Server\MSSQL.4\MSSQL\Data  where mdf and ldf locate
        public string defaultDataFolder { get; set; }//folder where sql server to locate mdf file,if it's null ,mdf will be locate in sqlDataRootFolder
        public string defaultLogFolder { get; set; }//folder where sql server to locate ldf file,if it's null ,ldf will be locate in sqlDataRootFolder
        public string virtualNode { get; set; } //cluster name
        public SQLServerInstance(string intanceName, string pathInfo)
        {
            instance = intanceName;
            path = pathInfo;
            isCluster = false;
        }
        public void GetInstanceInfo()
        {
            RegistryKey rk = null;
            if (CheckRegedit(path + "\\Instance Names\\SQL", instance, false))
            {
                rk = Registry.LocalMachine.OpenSubKey(path + "\\Instance Names\\SQL");
                if (rk != null)
                {
                    location = (string)rk.GetValue(instance);
                    rk.Close();
                }

                try
                {
                    string tcpSettingsPath = path + "\\" + location + @"\MSSQLServer\SuperSocketNetLib\Tcp";

                    RegistryKey tcpSetting = Registry.LocalMachine.OpenSubKey(tcpSettingsPath);

                    if (tcpSetting != null)
                    {
                        TCPListenIPAll = (Int32)tcpSetting.GetValue("ListenOnAllIPs") == 1;
                        if (TCPListenIPAll)
                        {
                            tcpSettingsPath = tcpSettingsPath + "\\IPAll";
                            RegistryKey ipAll = Registry.LocalMachine.OpenSubKey(tcpSettingsPath);
                            if (ipAll != null)
                            {
                                string tcpPortStr = ipAll.GetValue("TcpPort").ToString();
                                if (!string.IsNullOrEmpty(tcpPortStr))
                                {
                                    TCPPort = uint.Parse(tcpPortStr);
                                }
                                else
                                {
                                    tcpPortStr = ipAll.GetValue("TcpDynamicPorts").ToString();
                                    if (!string.IsNullOrEmpty(tcpPortStr))
                                    {
                                        TCPPort = uint.Parse(tcpPortStr);
                                        IsDefaultTCPPortUsed = true;
                                        //logger.Info("Named instance uses dynamic ports, current one is {0}.", TCPPort);
                                    }
                                }
                            }
                        }
                        //logger.Info("TCP settings, [Listen On All IPs] is {0}, and [TCP Port] is {1}.", TCPListenIPAll, TCPPort);

                        if (TCPPort == 1433 && instance.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                        {
                            IsDefaultTCPPortUsed = true;
                            //logger.Info("Default instance uses default port 1433.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Error occurred while getting the TCP settings of SQL instance {0}. details: {1}", instance, ex.ToString());
                }

                rk = Registry.LocalMachine.OpenSubKey(path + "\\" + location + "\\Setup");
                if (rk != null)
                {
                    edition = (string)rk.GetValue("Edition");
                    isCluster = CheckRegedit(path + "\\" + location, "Cluster", true);
                    rk.Close();
                }
                if (isCluster)
                {
                    if (CheckRegedit(path + "\\" + location + "\\Cluster", "ClusterName", false))
                    {
                        if (rk != null)
                        {
                            rk = Registry.LocalMachine.OpenSubKey(path + "\\" + location + "\\Cluster");
                            cluster = (string)rk.GetValue("ClusterName");
                            rk.Close();
                        }
                    }
                }
                rk = Registry.LocalMachine.OpenSubKey(path + "\\" + location + "\\MSSQLServer\\CurrentVersion");
                if (rk != null)
                {
                    version = (string)rk.GetValue("CurrentVersion");
                    rk.Close();
                }
            }
            else
            {
                //if (CheckRegedit(path, instance, true))
                if (CheckRegedit(path + "\\" + instance + "\\MSSQLServer", "CurrentVersion", true))
                {
                    rk = Registry.LocalMachine.OpenSubKey(path + "\\" + instance + "\\MSSQLServer\\CurrentVersion");
                    if (rk != null)
                    {
                        if (CheckRegedit(path + "\\" + instance + "\\MSSQLServer\\CurrentVersion", "CSDVersion", false))
                        {
                            version = (string)rk.GetValue("CSDVersion");
                        }
                        else if (CheckRegedit(path + "\\" + instance + "\\MSSQLServer\\CurrentVersion", "CurrentVersion", false))
                        {
                            version = (string)rk.GetValue("CurrentVersion");
                        }
                        else
                        {
                            version = "8.00.760";
                        }
                        rk.Close();
                    }
                    edition = "Enterprise Edition";
                    isCluster = CheckRegedit(path + "\\" + instance, "Cluster", true);
                    if (isCluster)
                    {
                        if (CheckRegedit(path + "\\" + instance + "\\Cluster", "ClusterName", false))
                        {
                            rk = Registry.LocalMachine.OpenSubKey(path + "\\" + instance + "\\Cluster");
                            if (rk != null)
                            {
                                cluster = (string)rk.GetValue("ClusterName");
                                rk.Close();
                            }
                        }
                    }
                }
                else
                {
                    if (instance.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                    {
                        string sql2000path = string.Empty;
                        if (CheckRegedit(@"SOFTWARE\Microsoft\MSSQLServer\MSSQLServer", "CurrentVersion", true))
                        {
                            sql2000path = @"SOFTWARE\Microsoft\MSSQLServer";
                        }
                        else
                        {
                            sql2000path = @"SOFTWARE\Wow6432Node\Microsoft\MSSQLServer";
                        }
                        rk = Registry.LocalMachine.OpenSubKey(sql2000path + @"\MSSQLServer\CurrentVersion");
                        //Console.WriteLine(sql2000path + @"\MSSQLServer\CurrentVersion");
                        if (rk != null)
                        {
                            if (CheckRegedit(sql2000path + @"\MSSQLServer\CurrentVersion", "CSDVersion", false))
                            {
                                version = (string)rk.GetValue("CSDVersion");
                            }
                            else if (CheckRegedit(sql2000path + @"\MSSQLServer\CurrentVersion", "CurrentVersion", false))
                            {
                                version = (string)rk.GetValue("CurrentVersion");
                            }
                            else
                            {
                                version = "8.00.760";
                            }
                            isCluster = CheckRegedit(sql2000path, "Cluster", true);
                            rk.Close();
                        }
                        if (isCluster)
                        {
                            if (CheckRegedit(sql2000path + @"\Cluster", "ClusterName", false))
                            {
                                rk = Registry.LocalMachine.OpenSubKey(sql2000path + @"\Cluster");
                                if (rk != null)
                                {
                                    cluster = (string)rk.GetValue("ClusterName");
                                    rk.Close();
                                }
                            }
                        }
                        edition = "Enterprise Edition";
                        path = sql2000path;
                    }
                    else
                    {
                        path = "unknown";
                        version = "unknown";
                        edition = "unknown";
                        location = "unknown";
                    }
                }
            }

            sqlDataRootFolder = SQLServerUtility.GetDBPath(instance);
            defaultDataFolder = SQLServerUtility.GetMdfFolderPath(instance);
            defaultLogFolder = SQLServerUtility.GetLdfFolderPath(instance);

        }
        public static bool CheckRegedit(string keyLocation, string key, bool isSubKey)
        {
            bool result = false;
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(keyLocation);
                string[] registrys = null;
                if (isSubKey)
                {
                    registrys = rk.GetSubKeyNames();
                }
                else
                {
                    registrys = rk.GetValueNames();
                }
                foreach (string value in registrys)
                {
                    if (value.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                        break;
                    }
                }
                rk.Close();
            }
            catch (Exception e)
            {
                logger.Debug("CheckRegistryKey exception:{0}", e.ToString());
                result = false;
            }
            return result;
        }
    }
    [AveVersion("$Revision: 431116 $")]
    public class SQLServerUtility
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetSQLServerInstanceVersion(string instanceName)
        {
            string version = string.Empty;
            SQLServerInstanceCollection sic = new SQLServerInstanceCollection();
            sic.GetInstanceNames();
            foreach (SQLServerInstance i in sic.instances.GetValueList())
            {
                if (i.instance.Equals(instanceName, StringComparison.OrdinalIgnoreCase))
                {
                    i.GetInstanceInfo();
                    version = i.version;
                    break;
                }
            }
            if (string.IsNullOrEmpty(version))
            {
                throw new Exception("Instance not found");
            }
            return version;
        }
        public static bool CheckSQLServerInstanceCluster(string instanceName)
        {
            bool isCluster = false;
            SQLServerInstanceCollection sic = new SQLServerInstanceCollection();
            sic.GetInstanceNames();
            int i;
            for (i = 0; i < sic.instances.Count; i++)
            {
                string key = (string)sic.instances.GetKey(i);
                if (((SQLServerInstance)sic.instances[key]).instance.Equals(instanceName, StringComparison.OrdinalIgnoreCase))
                {
                    ((SQLServerInstance)sic.instances[key]).GetInstanceInfo();
                    isCluster = ((SQLServerInstance)sic.instances[key]).isCluster;
                    break;
                }
            }
            if (i >= sic.instances.Count)
            {
                throw new Exception("Instance not found");
            }
            return isCluster;
        }
        public static string CheckSQLServerInstanceClusterName(string instanceName)
        {
            string cluster = "";
            SQLServerInstanceCollection sic = new SQLServerInstanceCollection();
            sic.GetInstanceNames();
            int i;
            for (i = 0; i < sic.instances.Count; i++)
            {
                string key = (string)sic.instances.GetKey(i);
                if (((SQLServerInstance)sic.instances[key]).instance.Equals(instanceName, StringComparison.OrdinalIgnoreCase))
                {
                    ((SQLServerInstance)sic.instances[key]).GetInstanceInfo();
                    cluster = ((SQLServerInstance)sic.instances[key]).cluster;
                    break;
                }
            }
            if (i >= sic.instances.Count)
            {
                throw new Exception("Instance not found");
            }
            return cluster;
        }
        public static string SearchSQLDataRootPath(string instanceName, string key)
        {
            RegistryKey rk = null;
            string path = "";
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(key + @"\Instance Names\SQL");//05
                if (rk != null)
                {
                    string[] instanceNames = rk.GetValueNames();
                    foreach (string instance in instanceNames)
                    {
                        if (instance.Equals(instanceName, StringComparison.OrdinalIgnoreCase))
                        {
                            string rks = (string)rk.GetValue(instance);
                            rk = Registry.LocalMachine.OpenSubKey(key + "\\" + rks + @"\Setup");
                            path = (string)rk.GetValue("SQLDataRoot");
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(path))
                {
                    if (instanceName.Equals("MSSQLSERVER"))
                    {
                        rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\MSSQLServer\Setup");
                        if (rk != null)
                        {
                            path = (string)rk.GetValue("SQLDataRoot");
                        }
                        if (string.IsNullOrEmpty(path))
                        {
                            rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Wow6432Node\Microsoft\MSSQLServer\Setup");
                            if (rk != null)
                            {
                                path = (string)rk.GetValue("SQLDataRoot");
                            }
                        }
                    }
                    else
                    {
                        rk = Registry.LocalMachine.OpenSubKey(key + "\\" + instanceName + "\\Setup");//2000
                        if (rk != null)
                        {
                            path = (string)rk.GetValue("SQLDataRoot");
                        }
                    }
                }
                if (rk != null)
                {
                    rk.Close();
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.ERROR, "Searched the database path to load the SQL server data path failed. Error: {0}", e.ToString());
            }
            return path;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        public static string GetDBPath(string instanceName)//return SQLDataRoot\Data
        {
            string path = string.Empty;
            path = SearchSQLDataRootPath(instanceName, @"SOFTWARE\Microsoft\Microsoft SQL Server");
            if (string.IsNullOrEmpty(path))
            {
                path = SearchSQLDataRootPath(instanceName, @"SOFTWARE\Wow6432Node\Microsoft\Microsoft SQL Server");
            }

            if (!path.ToLower().EndsWith("\\data", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(path))
            {
                path = Path.Combine(path, "Data");
            }
            return path;
        }
        private static string GetLocation(string instanceName, string key)//key=DefaultData,DefaultLog
        {
            string path = string.Empty;
            path = SearchDBLocation(instanceName, @"SOFTWARE\Microsoft\Microsoft SQL Server", key);
            if (string.IsNullOrEmpty(path))
            {
                path = SearchDBLocation(instanceName, @"SOFTWARE\Wow6432Node\Microsoft\Microsoft SQL Server", key);
            }
            return path;
        }
        private static string SearchDBLocation(string instanceName, string key, string searchKey)
        {
            RegistryKey rk = null;
            string path = "";
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(key + @"\Instance Names\SQL");//05
                if (rk != null)
                {
                    string[] instanceNames = rk.GetValueNames();
                    foreach (string instance in instanceNames)
                    {
                        if (instance.Equals(instanceName, StringComparison.OrdinalIgnoreCase))
                        {
                            string rks = (string)rk.GetValue(instance);
                            rk = Registry.LocalMachine.OpenSubKey(key + "\\" + rks + @"\MSSQLServer");
                            path = (string)rk.GetValue(searchKey);
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(path))
                {
                    if (instanceName.Equals("MSSQLSERVER"))
                    {
                        rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\MSSQLServer\MSSQLServer");
                        if (rk != null)
                        {
                            path = (string)rk.GetValue(searchKey);
                        }
                        if (string.IsNullOrEmpty(path))
                        {
                            rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Wow6432Node\Microsoft\MSSQLServer\MSSQLServer");
                            if (rk != null)
                            {
                                path = (string)rk.GetValue(searchKey);
                            }
                        }
                    }
                    else
                    {
                        rk = Registry.LocalMachine.OpenSubKey(key + "\\" + instanceName + "\\MSSQLServer");
                        if (rk != null)
                        {
                            path = (string)rk.GetValue(searchKey);
                        }
                    }
                }
                if (rk != null)
                {
                    rk.Close();
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.ERROR, "Searched the database path to load the SQL server data path failed. Error: {0}", e.ToString());
            }
            return path;
        }
        public static string GetSQLLocationPath(string serverName)
        {
            int k = serverName.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
            string mdfFolder = k < 0 ? SQLServerUtility.GetMdfFolderPath("MSSQLSERVER") : SQLServerUtility.GetMdfFolderPath(serverName.Substring(k + 1));
            string ldfFolder = k < 0 ? SQLServerUtility.GetLdfFolderPath("MSSQLSERVER") : SQLServerUtility.GetLdfFolderPath(serverName.Substring(k + 1));
            if (string.IsNullOrEmpty(mdfFolder) && !string.IsNullOrEmpty(ldfFolder))
            {
                mdfFolder = ldfFolder;
            }
            return mdfFolder;
        }
        public static string GetMdfFolderPath(string instanceName)
        {
            string path = GetLocation(instanceName, "DefaultData");
            if (string.IsNullOrEmpty(path))
            {
                path = GetDBPath(instanceName);
            }
            return path;
        }
        public static string GetLdfFolderPath(string instanceName)
        {
            string path = GetLocation(instanceName, "DefaultLog");
            if (string.IsNullOrEmpty(path))
            {
                path = GetDBPath(instanceName);
            }
            return path;
        }
    }
}
