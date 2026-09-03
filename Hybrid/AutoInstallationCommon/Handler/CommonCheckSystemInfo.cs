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
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Windows.Forms;
using AutoInstallationCommon.Utility.Handler;
using Microsoft.Win32;

namespace AutoInstallationCommon.Utility
{
    public class CommonCheckSystemInfo
    {
        private readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool IsShowHasServerManagerMessageBox;
        private string os = string.Empty;

        public string GetGivenItemInfo(InfoType infoType, string disk, params string[] projeckKey)
        {
            var result = string.Empty;

            switch (infoType)
            {
                case InfoType.DotNetVersion:
                {
                    result = GetDotNetVersion();
                    break;
                }
                case InfoType.DotNetFeature:
                {
                    if (projeckKey.Length != 0)
                        result = GetDotNetFeature(projeckKey[0]);
                    else
                        result = GetDotNetFeature();
                    break;
                }
                case InfoType.OS:
                {
                    result = GetOSVersion();
                    os = result;
                    break;
                }
                case InfoType.Memory:
                {
                    result = GetAvailableMemory().ToString();
                    break;
                }
                case InfoType.Disk:
                {
                    result = GetLeftSpaceDisk(disk).ToString();
                    break;
                }
                case InfoType.NetTcpPortSharingService:
                {
                    result = GetNetTcpPortSharingService().ToString();
                    break;
                }
                case InfoType.WorldWideWebPublishingService:
                {
                    result = GetWorldWideWebPublishingService().ToString();
                    break;
                }
                case InfoType.WindowsProcessActivationService:
                {
                    result = GetWindowsProcessActivationService();
                    break;
                }
                case InfoType.WebServiceRole:
                {
                    result = GetWebServiceRole().ToString();
                    break;
                }
                case InfoType.RemoteServerAdministrationTools:
                {
                    result = GetRemoteServerAdministrationTools().ToString();
                    break;
                }
                case InfoType.ASPDotNet:
                {
                    result = GetASPDotNetVersion();
                    break;
                }
                case InfoType.ApplicationServer:
                {
                    if (os == OSVersion.WindowsXP.ToString() ||
                        os.Equals(OSVersion.WindowsXPProfessionalX64Edition.ToString()))
                        result = GetApplicationServer(os);
                    else
                        result = GetApplicationServer();
                    break;
                }
                case InfoType.IISService:
                {
                    result = GetIISServiceInfo();
                    break;
                }
                case InfoType.HTTPSSL:
                {
                    result = GetHTTPSSLService();
                    break;
                }
                case InfoType.SharePoint:
                {
                    result = GetSharePointVersion();
                    break;
                }
                case InfoType.PowerShell:
                {
                    result = GetPowerShellVersion();
                    break;
                }
                case InfoType.CPUCount:
                {
                    result = GetCPUCount();
                    break;
                }
            }

            return result;
        }

        public static void VerifySingleInstance(string message, string summary)
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            if (!AveMutex.CheckMutex(processName))
            {
                MessageBox.Show(message,
                    summary,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        public static void VerifyRegistryKey(string message, string summary,string registryKeyPath)
        {
            var subKey = Registry.LocalMachine.OpenSubKey(registryKeyPath);
            if (subKey == null)
            {
                MessageBox.Show(message,
                    summary,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }


        #region --- Rrivate Function for Every Item ---

        /// <summary>
        ///     获取DotNet Version
        /// </summary>
        /// <returns></returns>
        private string GetDotNetVersion()
        {
            var rw = CommonRegistryWrapper.GetInstance();
            string version;

            #region 4.0

            var install = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client",
                "Install");
            if (install == "1")
            {
                version = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client",
                    "Version");
                if (string.IsNullOrEmpty(version))
                    return "4.0";
                return version;
            }

            #endregion

            install = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5",
                "Install");
            if (install == "1")
            {
                version = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5",
                    "Version");
                var sp = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", "SP");

                if (sp != "0")
                    return "3.6";
                return "3.5";
            }

            install = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0\Setup",
                "InstallSuccess");
            if (install == "1")
            {
                version = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0\Setup",
                    "Version");
                if (string.IsNullOrEmpty(version))
                    return "3.0";
                return version;
            }

            install = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727",
                "Install");
            if (install == "1")
            {
                version = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727",
                    "Version");
                if (string.IsNullOrEmpty(version))
                    return "2.0";
                return version;
            }

            install = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v1.1.4322",
                "Install");
            if (install == "1")
            {
                version = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v1.1.4322",
                    "Version");
                if (string.IsNullOrEmpty(version))
                    return "1.1";
                return version;
            }

            return "none";
        }

        /// <summary>
        ///     获取DotNet Feature
        /// </summary>
        /// <returns></returns>
        private string GetDotNetFeature(params string[] projectKey)
        {
            var dotNetFeatureList = new List<string>();
            var result = string.Empty;
            dotNetFeatureList.Add("NET-Framework");
            dotNetFeatureList.Add("NET-Framework-Core");
            if (projectKey.Length == 0)
            {
                dotNetFeatureList.Add("NET-Win-CFAC");
                dotNetFeatureList.Add("NET-HTTP-Activation");
                dotNetFeatureList.Add("NET-Non-HTTP-Activ");
            }

            result = VerifyFeaturesConfig(dotNetFeatureList, "D6FeatureDotNetFeatureList").ToString();

            return result;
        }

        /// <summary>
        ///     获取OS版本号
        /// </summary>
        /// <returns></returns>
        public string GetOSVersion()
        {
            return new CommonOSVersionWrapper().GetOSVersionHandler().ToString();
        }

        /// <summary>
        ///     获取可用物理内存（MB）
        /// </summary>
        /// <returns></returns>
        private long GetAvailableMemory()
        {
            var info = new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_PerfFormattedData_PerfOS_Memory");
            long fpm = 0;
            foreach (ManagementObject mo in info.Get())
                fpm = Convert.ToInt64(mo["AvailableMBytes"]); //剩余物理内存量FreePhysicalMemory
            return fpm;
        }

        /// <summary>
        ///     检查磁盘剩余空间
        /// </summary>
        /// <param name="disk">盘符</param>
        /// <returns>剩余空间（M）</returns>
        private int GetLeftSpaceDisk(string disk)
        {
            var maxLeftSpace = 0;

            var driveInfos = DriveInfo.GetDrives();
            foreach (var di in driveInfos)
                if (di.DriveType == DriveType.Fixed &&
                    di.Name.Split('\\')[0].Trim().Equals(disk.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var freespaceGB = di.TotalFreeSpace / (1024 * 1024);
                    maxLeftSpace = (int) freespaceGB;
                }

            return maxLeftSpace;
        }

        /// <summary>
        ///     Net.Tcp Port Sharing Service
        /// </summary>
        /// <returns></returns>
        private bool GetNetTcpPortSharingService()
        {
            return VerifySystemService("NetTcpPortSharing");
        }

        /// <summary>
        ///     World Wide Web Publishing Service
        /// </summary>
        /// <returns></returns>
        private bool GetWorldWideWebPublishingService()
        {
            return VerifySystemService("W3SVC");
        }

        /// <summary>
        ///     Windows Process Activation Service
        /// </summary>
        /// <returns></returns>
        private string GetWindowsProcessActivationService()
        {
            var windowsProcessActivationServiceList = new List<string>
            {
                "WAS",
                "WAS-Process-Model",
                "WAS-NET-Environment",
                "WAS-Config-APIs"
            };

            var feature =
                VerifyFeaturesConfig(windowsProcessActivationServiceList,
                        "D6FeatureWindowsProcessActivationServiceList")
                    .ToString();

            var state = VerifySystemService("WAS").ToString();

            var result = state + "#" + feature;
            return result;
        }

        /// <summary>
        ///     Web Server (IIS) Role
        /// </summary>
        /// <returns></returns>
        private bool GetWebServiceRole()
        {
            var osv = new CommonOSVersionWrapper().GetOSVersionHandler();
            var webServiceRoleList = new List<string>
            {
                "Web-Server",
                "Web-Common-Http",
                "Web-Static-Content",
                "Web-Default-Doc",
                "Web-App-Dev",
                "Web-Asp-Net",
                "Web-Net-Ext",
                "Web-ISAPI-Ext",
                "Web-ISAPI-Filter",
                "Web-Security",
                "Web-Filtering",
                "Web-Mgmt-Tools",
                "Web-Mgmt-Compat",
                "Web-Metabase"
            };

            if (!osv.Equals(OSVersion.WindowsServer2008R2ServerCore)) webServiceRoleList.Add("Web-Mgmt-Console");

            return VerifyFeaturesConfig(webServiceRoleList, "D6FeatureWebServiceRole");
        }

        /// <summary>
        ///     Remote Server Administration Tools
        /// </summary>
        /// <returns></returns>
        private bool GetRemoteServerAdministrationTools()
        {
            var remoteServerAdministrationToolsList = new List<string> {"RSAT", "RSAT-Role-Tools", "RSAT-Web-Server"};

            return VerifyFeaturesConfig(remoteServerAdministrationToolsList,
                "D6FeatureRemoteServerAdministrationTools");
        }

        /// <summary>
        ///     ASP DotNet Version(03 OS)
        /// </summary>
        /// <returns></returns>
        private string GetASPDotNetVersion()
        {
            var wrapper = CommonRegistryWrapper.GetInstance();

            if (VerifyASPDotNETRootVer(wrapper))
                return "2.0.50727";
            return string.Empty;
        }


        public bool VerifyASPDotNETRootVer(CommonRegistryWrapper mRegistryWrapper)
        {
            var RootVerValue = string.Empty;
            RootVerValue = mRegistryWrapper.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET", "RootVer");
            var RootVerValueString = RootVerValue.Split('.');
            if (int.Parse(RootVerValueString[0]) > 1)
            {
                if (RootVerValueString[0].Equals("2"))
                    if (RootVerValueString[1].Equals("0"))
                        return int.Parse(RootVerValueString[2]) >= 20727;
                return true;
            }

            return false;
        }

        private bool VerifyASPDotNET2()
        {
            var mRegistryWrapper = CommonRegistryWrapper.GetInstance();
            return mRegistryWrapper.Exists(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET\2.0.50727.0");
        }

        private bool VerifyASPDotNET4()
        {
            var mRegistryWrapper = CommonRegistryWrapper.GetInstance();
            return mRegistryWrapper.Exists(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET\4.0.30319.0");
        }

        /// <summary>
        ///     Application Server(03 OS)
        /// </summary>
        /// <returns></returns>
        private string GetApplicationServer()
        {
            var complus = GetCOMPlus().ToString();
            var commonfiles = GetCommonFiles().ToString();
            var iismanager = GetIISManager().ToString();
            var iiswwwservice = GetIISWWWService().ToString();

            var result = complus + "#" + commonfiles + "#" + iismanager + "#" + iiswwwservice;

            return result;
        }

        private string GetApplicationServer(string os)
        {
            //string complus = GetCOMPlus().ToString();
            var commonfiles = GetCommonFiles().ToString();
            var iismanager = GetIISManager().ToString();
            var iiswwwservice = GetIISWWWService().ToString();

            var result = commonfiles + "#" + iismanager + "#" + iiswwwservice;

            return result;
        }

        private bool GetCOMPlus()
        {
            var result = VerifyComponent("complusnetwork");
            return result;
        }

        private bool GetCommonFiles()
        {
            var result = VerifyComponent("iis_common");
            return result;
        }

        private bool GetIISManager()
        {
            var result = VerifyComponent("iis_inetmgr");
            return result;
        }

        private bool GetIISWWWService()
        {
            var result = VerifyComponent("iis_www");
            return result;
        }

        /// <summary>
        ///     IIS Service Info(03 OS)
        /// </summary>
        /// <returns></returns>
        private string GetIISServiceInfo()
        {
            var version = GetIISServiceVersion();
            var service = "true"; //GetIISAdminService().ToString();

            var result = version + "#" + service;
            return result;
        }

        private string GetIISServiceVersion()
        {
            var rw = CommonRegistryWrapper.GetInstance();
            var majorVersion = "none";
            try
            {
                majorVersion = rw.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\W3SVC\Parameters",
                    "MajorVersion");
                logger.Info("IIS Version in check is {0}.", majorVersion);
            }
            catch (Exception ex)
            {
                logger.Error("Check IIS Version Exception:{0}", ex);
            }

            return majorVersion;
        }

        private bool GetIISAdminService()
        {
            return VerifySystemService("IISADMIN"); //IIS Admin Service
        }

        /// <summary>
        ///     HTTP SSL Service(03 OS)
        /// </summary>
        /// <returns></returns>
        private string GetHTTPSSLService()
        {
            var httpfilter = GetHTTPFilter().ToString();

            var result = httpfilter;
            return result;
        }

        private bool GetHTTPFilter()
        {
            return VerifySystemService("HTTPFilter"); //HTTP SSL
        }

        /// <summary>
        ///     Judge Feature existing
        /// </summary>
        /// <param name="features"></param>
        /// <param name="debugFilename"></param>
        /// <returns></returns>
        private bool VerifyFeaturesConfig(List<string> features, string debugFilename)
        {
            try
            {
                var result = true;

                var feature = new CommonFeaturesWrapper();
                List<string> fails = null;
                var osv = new CommonOSVersionWrapper().GetOSVersionHandler();
                if (osv.Equals(OSVersion.WindowsServer2012) || osv.Equals(OSVersion.Windows8) ||
                    osv.Equals(OSVersion.Windows7) || osv.Equals(OSVersion.WindowsServer2008R2ServerCore) ||
                    osv.Equals(OSVersion.WindowsVista))
                {
                    features = ChangeFeatureNames(features, OSVersion.Windows7);
                    fails = feature.VerifyFeatureWin7(features);
                }
                else
                {
                    fails = feature.VerifyFeature(features);
                }

                if (fails.Count <= 0)
                {
                    result = true;
                }
                else
                {
                    for (var i = 0; i < fails.Count; i++) logger.Info("The Fail Feature:", fails[i]);
                    var featuresDebugFilePath = "C:\\" + debugFilename;
                    if (File.Exists(featuresDebugFilePath))
                    {
                        File.Delete(featuresDebugFilePath);
                        using (
                            var fs = new FileStream(featuresDebugFilePath,
                                FileMode.CreateNew,
                                FileAccess.ReadWrite,
                                FileShare.Write))
                        {
                            using (var sw = new StreamWriter(fs))
                            {
                                for (var i = 0; i < fails.Count; i++) sw.WriteLine(fails[i] + "\r\n");
                            }
                        }
                    }

                    result = false;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Verify Features Config Exception:", ex);
                return false;
            }
        }

        /// <summary>
        ///     Change Feature Names By OS Version
        /// </summary>
        /// <param name="features"></param>
        /// <returns></returns>
        private List<string> ChangeFeatureNames(List<string> features, OSVersion osv)
        {
            var returnFeatureNames = new List<string>();
            var featureNames = InitializationFeaturesList();
            foreach (var featurename in features)
            foreach (var onefeatureNames in featureNames)
                if (featurename.Equals(onefeatureNames.Win08FeatureName))
                {
                    if (osv.Equals(OSVersion.Windows7)) returnFeatureNames.Add(onefeatureNames.Win7FeatureName);
                    //else if (osv.Equals(OSVersion.WindowsVista))//for vista
                    //{

                    //}
                    break;
                }

            return returnFeatureNames;
        }

        /// <summary>
        ///     Initialization featrues
        /// </summary>
        private List<FeatureNames> InitializationFeaturesList()
        {
            var returnlist = new List<FeatureNames>
            {
                InitializationFeature("NET-HTTP-Activation", "WCF-HTTP-Activation", ""),
                InitializationFeature("NET-Non-HTTP-Activ", "WCF-NonHTTP-Activation", ""),
                InitializationFeature("Web-Server", "IIS-WebServer", ""),
                InitializationFeature("Web-Common-Http", "IIS-CommonHttpFeatures", ""),
                InitializationFeature("Web-Static-Content", "IIS-StaticContent", ""),
                InitializationFeature("Web-Default-Doc", "IIS-DefaultDocument", ""),
                InitializationFeature("Web-App-Dev", "IIS-ApplicationDevelopment", ""),
                InitializationFeature("Web-Asp-Net", "IIS-ASPNET", ""),
                InitializationFeature("Web-Net-Ext", "IIS-NetFxExtensibility", ""),
                InitializationFeature("Web-ISAPI-Ext", "IIS-ISAPIExtensions", ""),
                InitializationFeature("Web-ISAPI-Filter", "IIS-ISAPIFilter", ""),
                InitializationFeature("Web-Security", "IIS-Security", ""),
                InitializationFeature("Web-Filtering", "IIS-RequestFiltering", ""),
                InitializationFeature("Web-Mgmt-Tools", "IIS-WebServerManagementTools", ""),
                InitializationFeature("Web-Mgmt-Console", "IIS-ManagementConsole", ""),
                InitializationFeature("Web-Mgmt-Compat", "IIS-IIS6ManagementCompatibility", ""),
                InitializationFeature("Web-Metabase", "IIS-Metabase", ""),
                InitializationFeature("WAS", "WAS-WindowsActivationService", ""),
                InitializationFeature("WAS-Process-Model", "WAS-ProcessModel", ""),
                InitializationFeature("WAS-NET-Environment", "WAS-NetFxEnvironment", ""),
                InitializationFeature("WAS-Config-APIs", "WAS-ConfigurationAPI", "")
            };
            return returnlist;
        }

        /// <summary>
        ///     Initialization featrues
        /// </summary>
        /// <param name="win08Featurename"></param>
        /// <param name="win7Featurename"></param>
        /// <param name="vistaFeaturename"></param>
        /// <returns></returns>
        private FeatureNames InitializationFeature(string win08Featurename,
            string win7Featurename,
            string vistaFeaturename)
        {
            FeatureNames featureNames;
            featureNames.Win08FeatureName = win08Featurename;
            featureNames.Win7FeatureName = win7Featurename;
            featureNames.VistaFeatureName = vistaFeaturename;
            return featureNames;
        }

        /// <summary>
        ///     Judge Service existing and running
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private bool VerifySystemService(string serviceName)
        {
            var feature = new CommonServiceWrapper();
            var result = feature.VerifyServiceExist(serviceName);
            if (result) result = feature.VerifyServiceRun(serviceName);
            return result;
        }


        /// <summary>
        ///     Judge Component existing
        /// </summary>
        /// <param name="valueName">键值项名称</param>
        /// <returns>true：存在 false：不存在</returns>
        private bool VerifyComponent(string valueName)
        {
            var mRegistryWrapper = CommonRegistryWrapper.GetInstance();
            var result =
                mRegistryWrapper.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Setup\OC Manager\Subcomponents",
                    valueName);
            if ("1".Equals(result)) return true;
            return false;
        }


        /// <summary>
        ///     SharePoint Version
        /// </summary>
        /// <returns></returns>
        private string GetSharePointVersion()
        {
            var result = string.Empty;
            var sp = CommonCheckSharePointWrapper.GetInstance();
            var tmpString = sp.GetMOSSOrWSS();
            if (tmpString.StartsWith("Microsoft SharePoint Server 2010", StringComparison.OrdinalIgnoreCase))
                result = "SP2010";
            if (tmpString.StartsWith("Microsoft SharePoint Foundation 2010", StringComparison.OrdinalIgnoreCase))
                result = "SP2010Foundation";
            return result;
        }

        private string GetFastSearchServerVersion()
        {
            var hasFast = false;
            var fastPath = Environment.GetEnvironmentVariable("FASTSEARCH");
            if (string.IsNullOrEmpty(fastPath))
            {
                var rs = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\FAST Search Server\Setup");
                if (rs != null) fastPath = rs.GetValue("Path").ToString();
            }

            if (!string.IsNullOrEmpty(fastPath))
                if (Directory.Exists(fastPath))
                    hasFast = true;

            if (hasFast)
                return "FSS2010";
            return hasFast.ToString();
        }


        private string GetPowerShellVersion()
        {
            var rs = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\PowerShell\3\PowerShellEngine");
            if (rs != null) return rs.GetValue("PowerShellVersion").ToString();

            rs = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\PowerShell\1\PowerShellEngine");
            if (rs != null)
                return rs.GetValue("PowerShellVersion").ToString();
            return "0.0"; //for check the value after point
        }

        private string GetSliverLightVersion()
        {
            var sliverLightVersion = string.Empty;
            var rw = CommonRegistryWrapper.GetInstance();
            var version = rw.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Silverlight", "Version");
            if (string.IsNullOrEmpty(version)) version = "none";
            return version;
        }

        /// <summary>
        ///     检查CPU核数
        /// </summary>
        /// <returns>核数</returns>
        private string GetCPUCount()
        {
            var cpuCount = 0;
            var mc = new ManagementClass("Win32_Processor");
            var moc = mc.GetInstances();
            foreach (ManagementObject mo in moc) cpuCount += Convert.ToInt32(mo.Properties["NumberOfCores"].Value);
            return cpuCount.ToString();
        }

        #endregion
    }

    /// <summary>
    ///     Save FeatureNames(win08,win7,vista)
    /// </summary>
    internal struct FeatureNames
    {
        public string Win08FeatureName;
        public string Win7FeatureName;
        public string VistaFeatureName;
    }

    public enum InfoType
    {
        OS,
        Memory,
        DotNetVersion,
        DotNetFeature,
        Disk,
        NetTcpPortSharingService,
        WorldWideWebPublishingService,
        WindowsProcessActivationService,
        WebServiceRole,
        RemoteServerAdministrationTools,
        ASPDotNet,
        ApplicationServer,
        IISService,
        HTTPSSL,
        SharePoint,
        PRRequirement,
        PowerShell,
        CPUCount,

        AppManagementService,
        AppManagementServiceApplication,
        AppManagementServiceApplicationAssociation,
        SharePointFoundationService,
        SharePointFoundationServiceApplication,
        UserProfileService,
        UserProfileServiceApplication,
        UserProfileServiceApplicationAssociation,
        AppDomian,
        AppPreFix,
        ConfigRecordsApp,
        EditRegisty,


        CatalogLink
    }
}