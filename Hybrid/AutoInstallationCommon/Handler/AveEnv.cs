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
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace AutoInstallationCommon.Utility
{
    #region using directives

    #endregion


    public class AveEnv
    {
        private static readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static FileSystemWatcher configurationFileWatcher = null;

        static AveEnv()
        {
        }


        public class PersistOptions
        {
            public bool PersistAgentFarmName { get; set; }
            public bool PersistAgentFarmId { get; set; }
            public bool PersistAgentName { get; set; }
            public bool PersistAgentAddress { get; set; }
            public bool PersistAgentPort { get; set; }
            public bool PersistAgentLogLevel { get; set; }
            public bool PersistAgentLogMaxFileSize { get; set; }
            public bool PersistAgentLogMaxRollBackups { get; set; }
            public bool PersistAgentType { get; set; }
            public bool PersistAgentWcfThumbprint { get; set; }
            public bool PersistAgentCIID { get; set; }
            public bool PersistAgentLazyStartProcess { get; set; }
            public bool PersistAgentProductType { get; set; }
            public bool PersistManagerAddress { get; set; }
            public bool PersistManagerPort { get; set; }
            public bool PersistPassphraseHash { get; set; }

            public string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("PersistAgentFarmName:" + PersistAgentFarmName);
                sb.Append(" PersistAgentFarmId:" + PersistAgentFarmId);
                sb.Append(" PersistAgentName:" + PersistAgentName);
                sb.Append(" PersistAgentAddress:" + PersistAgentAddress);
                sb.Append(" PersistAgentPort:" + PersistAgentPort);
                sb.Append(" PersistAgentAddress:" + PersistAgentAddress);
                sb.Append(" PersistAgentLogLevel:" + PersistAgentLogLevel);
                sb.Append(" PersistAgentType:" + PersistAgentType);
                sb.Append(" PersistAgentWcfThumbprint:" + PersistAgentWcfThumbprint);
                sb.Append(" PersistAgentCIID:" + PersistAgentCIID);
                sb.Append(" PersistAgentProductType:" + PersistAgentProductType);
                sb.Append(" PersistManagerAddress:" + PersistManagerAddress);
                sb.Append(" PersistManagerPort:" + PersistManagerPort);
                sb.Append(" PersistPassphraseHash:" + PersistPassphraseHash);

                return sb.ToString();
            }
        }

        #region -- public Static Properties --

        public static string LocalSPServerName { get; set; }
        public static string AgentSchema { get; set; }
        public static string AgentName { get; set; }
        public static string AgentAddress { get; set; }
        public static int AgentPort { get; set; }
        private static int agentLogLevel;

        public static int AgentLogLevel
        {
            get { return (int) logger.CurrentLogLevel; }
            set { agentLogLevel = value; }
        }

        public static int AgentLogMaxFileSize { get; set; }
        public static int AgentLogMaxRollBackups { get; set; }
        public static string AgentType { get; set; }
        public static string AgentRootFolder { get; set; }
        public static string AgentBinFolder { get; set; }
        public static string AgentJobFolder { get; set; }
        public static string AgentDataFolder { get; set; }
        public static string AgentLogFolder { get; set; }
        public static int AgentLogRetentionDays { get; set; }
        public static long AgentLogRetentionTriggerSize { get; set; }
        public static long AgentLogRetentionKeepSize { get; set; }
        public static int AgentArchivedLogRetetionDays { get; set; }
        public static int AgentJobsRetentionDays { get; set; }
        public static int AgentServiceUpdataInterval { get; set; }
        public static int AgentTempFileRetentionDays { get; set; }
        public static string AgentLazyStartProcess { get; set; }
        public static string AgentTempFolder { get; set; }
        public static string AgentFarmName { get; set; }
        public static string AgentFarmId { get; set; }
        public static string AgentWcfThumbprint { get; set; }
        public static bool AgentEnableSSL { get; set; }
        public static string AgentSSLThumbprint { get; set; }
        public static string AgentCIID { get; set; }
        public static bool AgentSkipRemoveAgentType { get; set; }
        public static int AgentRegisterRetryInterval { get; set; }
        public static int AgentRegisterMaxRetries { get; set; }
        public static bool AgentRegisterSkipFailed { get; set; }
        public static int AgentCheckingRoleInFarmTimeout { get; set; }
        public static string AgentVersion { get; set; }
        public static string AgentDisplayVersion { get; set; }
        public static OEMProductType AgentProductType { get; set; }
        public static bool IsDocAve => (AgentProductType & OEMProductType.DocAve) == OEMProductType.DocAve;
        public static bool IsNetApp => (AgentProductType & OEMProductType.NetApp) == OEMProductType.NetApp;
        public static string ManagerSchema { get; set; }
        public static string ManagerAddress { get; set; }
        public static int ManagerPort { get; set; }
        public static byte[] PassphraseHash { get; set; }

        public static int SPVersion => (int) AveSPEnv.SPVersion;
        public static int MossOrWss => (int) AveSPEnv.SPMOSSOrWSS;
        public static string SharePointDisplayVersion => AveSPEnv.DisplayVersion;
        public static string SharePointDLLVersion { get; set; }

        public static bool IsPublishing
        {
            get
            {
                if (AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2007 ||
                    AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2010 ||
                    AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2013)
                    if (AveSPEnv.SPMOSSOrWSS == AveSPEnv.AveSPMOSSOrWSSInternal.MOSS)
                        return true;
                return false;
            }
        }

        public static bool IsSharePoint2003 => AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2003;
        public static bool IsSharePoint2007 => AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2007;
        public static bool IsSharePoint2010 => AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2010;

        public static bool IsSharePoint2010OrLower => AveSPEnv.SPVersion <= AveSPEnv.AveSPVersionInternal.SharePoint2010;
        public static bool IsSharePoint2013 => AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2013;
        public static bool IsSharePoint2016 => AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2016;
        public static bool IsSharePoint2019 { get { return AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.SharePoint2019; } }
        public static bool IsNonSPInstalled => AveSPEnv.SPVersion == AveSPEnv.AveSPVersionInternal.None;
        public static bool IsMoss => AveSPEnv.SPMOSSOrWSS == AveSPEnv.AveSPMOSSOrWSSInternal.MOSS;
        public static bool IsWss => AveSPEnv.SPMOSSOrWSS == AveSPEnv.AveSPMOSSOrWSSInternal.WSS;

        #endregion

        #region -- Static Methods --

        private static void EnsureSharePointDLLVersion()
        {
            if (!string.IsNullOrEmpty(SharePointDLLVersion)) return;

            SharePointDLLVersion = GetSharePointDLLFileVersion();
        }

        private static void EnsureWcfThumbprint()
        {
            try
            {
                if (!string.IsNullOrEmpty(AgentWcfThumbprint)) return;

                var xDoc = new XmlDocument();
                xDoc.Load(Path.Combine(AgentBinFolder, "AgentCommonWCFBehaviors.config"));
                var clientCertificateNode =
                    xDoc.SelectSingleNode(@"/behaviors/endpointBehaviors/behavior/clientCredentials/clientCertificate");
                var serviceCertificateNode =
                    xDoc.SelectSingleNode(
                        @"/behaviors/serviceBehaviors/behavior/serviceCredentials/serviceCertificate");
                var wcfThumbprint1 = clientCertificateNode.Attributes["findValue"].Value;
                var wcfThumbprint2 = serviceCertificateNode.Attributes["findValue"].Value;
                AgentWcfThumbprint = wcfThumbprint1;
            }
            catch (Exception ex)
            {
                logger.Error("Load WCF thumbprint failed:{0}", ex.ToString());
            }
        }

        private static void PersistWcfThumbprint()
        {
            if (string.IsNullOrEmpty(AgentWcfThumbprint)) return;

            var xDoc = new XmlDocument();
            xDoc.Load(Path.Combine(AgentBinFolder, "AgentCommonWCFBehaviors.config"));
            var clientCertificateNode =
                xDoc.SelectSingleNode(@"/behaviors/endpointBehaviors/behavior/clientCredentials/clientCertificate");
            var serviceCertificateNode =
                xDoc.SelectSingleNode(@"/behaviors/serviceBehaviors/behavior/serviceCredentials/serviceCertificate");
            clientCertificateNode.Attributes["findValue"].Value = AgentWcfThumbprint;
            serviceCertificateNode.Attributes["findValue"].Value = AgentWcfThumbprint;
            xDoc.Save(Path.Combine(AgentBinFolder, "AgentCommonWCFBehaviors.config"));
        }

        private static string CombinePath(string parentFolder, string currentFolderName)
        {
            var path = Path.Combine(parentFolder, currentFolderName);
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                logger.Warn("Create Directory:{0} failed:{1}", path, ex.ToString());
            }

            return path;
        }

        private static string GetAttributeFromSetting(KeyValueConfigurationCollection settings, string name,
            object defaultValue, ref bool configurationFileChanged)
        {
            var keyValue = settings[name];
            if (keyValue != null) return keyValue.Value;

            settings.Add(name, defaultValue.ToString());
            configurationFileChanged = true;

            return defaultValue.ToString();
        }

        private static string GetSharePointDLLFileVersion()
        {
            string fileVersion = string.Empty;
            try
            {
                logger.Info("Begin to get SharePoint file version.");
                string hiveLocation = string.Empty;
                if (IsSharePoint2019)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2016)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("16.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2013)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("15.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2010)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("14.0").GetValue("Location").ToString();
                }
                else if (IsSharePoint2007)
                {
                    hiveLocation = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft").OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("12.0").GetValue("Location").ToString();
                }
                string spDLL = Path.Combine(Path.Combine(hiveLocation, "ISAPI"), "Microsoft.SharePoint.dll");
                fileVersion = FileVersionInfo.GetVersionInfo(spDLL).FileVersion;
                logger.Info("SharePoint file version: " + fileVersion);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting SharePoint file version. Exception: {0}", ex.ToString());
            }
            if (string.IsNullOrEmpty(fileVersion))
            {
                logger.Info("Cannot get SharePoint file version. it's non-SharePoint box.");
                return string.Empty;
            }
            return fileVersion;
        }

        private static string GetFarmId()
        {
            var farmId = string.Empty;

            try
            {
                logger.Info("Begin to get farm id.");
                if (IsSharePoint2013)
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft")
                        .OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("15.0")
                        .OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                else if (IsSharePoint2010)
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft")
                        .OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("14.0")
                        .OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                else if (IsSharePoint2007)
                    farmId = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft")
                        .OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("12.0")
                        .OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("id").ToString();
                logger.Info("SharePoint Configuration id: {0}", farmId);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting SharePoint configuration id. Exception: {0}",
                    ex.ToString());
            }

            if (string.IsNullOrEmpty(farmId))
            {
                logger.Info("Cannot get SharePoint configuration id. it's non-SharePoint box.");
                return string.Empty;
            }

            return farmId;
        }


        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToUpper")]
        private static string GetFarmName()
        {
            var farmName = string.Empty;
            var dsn = string.Empty;

            try
            {
                logger.Info("Begin to get farm name.");
                try
                {
                    if (IsSharePoint2013)
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft")
                            .OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("15.0")
                            .OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                    else if (IsSharePoint2010)
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft")
                            .OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("14.0")
                            .OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                    else if (IsSharePoint2007)
                        dsn = Registry.LocalMachine.OpenSubKey("SoftWare").OpenSubKey("Microsoft")
                            .OpenSubKey("Shared tools").OpenSubKey("Web Server Extensions").OpenSubKey("12.0")
                            .OpenSubKey("Secure").OpenSubKey("ConfigDB").GetValue("dsn").ToString();
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while getting SharePoint configuration dsn. Exception: {0}",
                        ex.ToString());
                }

                if (string.IsNullOrEmpty(dsn))
                {
                    logger.Info("Cannot get SharePoint configuration dsn. it's non-SharePoint box.");
                    return string.Empty;
                }

                logger.Info("SharePoint Configuration DSN: {0}", dsn);
                var contents = dsn.Split(';');
                var dbName = string.Empty;
                var instance = string.Empty;
                foreach (var temp in contents)
                {
                    if (temp.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase))
                    {
                        instance = temp.Substring(12).ToUpperInvariant();
                        continue;
                    }

                    if (temp.StartsWith("Initial Catalog", StringComparison.OrdinalIgnoreCase))
                        dbName = temp.Substring(16);
                }

                farmName = "Farm(" + instance.ToUpper() + ":" + dbName.ToUpper() + ")";
                string[] specialChars = {"/", "*", "?", "\"", "<", ">", "|"};
                foreach (var s in specialChars) farmName = farmName.Replace(s, "");
            }
            catch (Exception ex)
            {
                logger.Error("Get Farm Name Failed:{0}", ex.ToString());
            }

            return farmName;
        }

        #endregion
    }

    internal class AveSPEnv
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(AveSPEnv));

        static AveSPEnv()
        {
            try
            {
                GetMossOrWssVersion();
            }
            catch (Exception ex)
            {
                logger.Error("Get SPVersion Failed:{0}", ex.ToString());
            }
        }

        static AveSPVersionInternal spVersion;
        static AveSPMOSSOrWSSInternal spMoss;

        internal static AveSPVersionInternal SPVersion { get { return spVersion; } }
        internal static AveSPMOSSOrWSSInternal SPMOSSOrWSS { get { return spMoss; } }
        internal static string DisplayVersion { get; set; }

        private static void GetMossOrWssVersion()
        {
            String wss30 = "Microsoft Windows SharePoint Services 3.0";
            String wss30ID = "{90120000-1014-0000-0000-0000000FF1CE}";
            String wss30IDx64 = "{90120000-1014-0000-1000-0000000FF1CE}";
            String mossDisplay = "Microsoft Office SharePoint Server 2007";
            String moss2007ID = "{90120000-110D-0000-0000-0000000FF1CE}";
            String moss2007IDx64 = "{90120000-110D-0000-1000-0000000FF1CE}";
            String sps2003 = "Microsoft Office SharePoint Portal Server 2003";
            String sps2003ID = "{610F491D-BE5F-4ED1-A0F7-759D40C7622E}";

            String wss20 = "Microsoft Windows SharePoint Services 2.0";
            String wss20ID = "{91140409-7000-11D3-8CFE-0150048383C9}";
            String moss2010 = "Microsoft SharePoint Server 2010";
            String moss2010ID = "{20140000-110D-0000-1000-0000000FF1CE}";
            String moss2010IDNew = "{90140000-110D-0000-1000-0000000FF1CE}";
            String wss2010 = "Microsoft SharePoint Foundation 2010";
            String wss2010ID = "{90140000-1110-0000-1000-0000000FF1CE}";
            String wss2010New = "Microsoft SharePoint Foundation 2010 Core";
            String wss2010IDNew = "{90140000-1014-0000-1000-0000000FF1CE}";

            String wss2013 = "Microsoft SharePoint Foundation 2013 Core";
            String wss2013ID = "{20150000-1014-0000-1000-0000000FF1CE}";
            String wss2013IDNew = "{90150000-1014-0000-1000-0000000FF1CE}";
            String moss2013 = "Microsoft SharePoint Server 2013";
            String moss2013ID = "{20150000-110D-0000-1000-0000000FF1CE}";
            String moss2013IDNew = "{90150000-110D-0000-1000-0000000FF1CE}";

            String wss2016 = "Microsoft SharePoint Foundation 2016 Core";
            String wss2016ID = "{20160000-1014-0000-1000-0000000FF1CE}";
            String wss2016IDNew = "{90160000-1014-0000-1000-0000000FF1CE}";
            String moss2016 = "Microsoft SharePoint Server 2016";
            String moss2016ID = "{20160000-110D-0000-1000-0000000FF1CE}";
            String moss2016IDNew = "{90160000-110D-0000-1000-0000000FF1CE}";

            String moss2019 = "Microsoft SharePoint Server 2019";
            String moss2019Id = "{10160000-110D-0000-1000-0000000FF1CE}";
            String moss2019IdNew = "{90160000-1167-0000-1000-0000000FF1CE}";

            if (KeyNameExists(moss2019IdNew, moss2019))
            {
                spVersion = AveSPVersionInternal.SharePoint2019;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2019IdNew);
            }
            else if (KeyNameExists(moss2019Id, moss2019))
            {
                spVersion = AveSPVersionInternal.SharePoint2019;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2019Id);
            }
            else if (KeyNameExists(moss2016ID, moss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2016ID);
            }
            else if (KeyNameExists(moss2016IDNew, moss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2016IDNew);
            }
            else if (KeyNameExists(wss2016ID, wss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2016ID);
            }
            else if (KeyNameExists(wss2016IDNew, wss2016))
            {
                spVersion = AveSPVersionInternal.SharePoint2016;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2016IDNew);
            }
            else if (KeyNameExists(moss2013ID, moss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2013ID);
            }
            else if (KeyNameExists(moss2013IDNew, moss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                DisplayVersion = GetDisplayVersionUnderKey(moss2013IDNew);
            }
            else if (KeyNameExists(wss2013ID, wss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2013ID);
            }
            else if (KeyNameExists(wss2013IDNew, wss2013))
            {
                spVersion = AveSPVersionInternal.SharePoint2013;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2013IDNew);
            }
            else if (KeyNameExists(moss2010ID, moss2010) || KeyNameExists(moss2010IDNew, moss2010))
            {
                spVersion = AveSPVersionInternal.SharePoint2010;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                if (KeyNameExists(moss2010ID, moss2010))
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2010ID);
                }
                else
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2010IDNew);
                }
            }
            else if (KeyNameExists(wss2010ID, wss2010))
            {
                spVersion = AveSPVersionInternal.SharePoint2010;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2010ID);
            }
            else if (KeyNameExists(wss2010IDNew, wss2010New))
            {
                spVersion = AveSPVersionInternal.SharePoint2010;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                DisplayVersion = GetDisplayVersionUnderKey(wss2010New);
            }
            else if (KeyNameExists(moss2007ID, mossDisplay) || KeyNameExists(moss2007IDx64, mossDisplay))
            {
                spVersion = AveSPVersionInternal.SharePoint2007;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
                if (KeyNameExists(moss2007ID, mossDisplay))
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2007ID);
                }
                else
                {
                    DisplayVersion = GetDisplayVersionUnderKey(moss2007IDx64);
                }
            }
            else if (KeyNameExists(wss30ID, wss30) || KeyNameExists(wss30IDx64, wss30))
            {
                spVersion = AveSPVersionInternal.SharePoint2007;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
                if (KeyNameExists(wss30ID, wss30))
                {
                    DisplayVersion = GetDisplayVersionUnderKey(wss30ID);
                }
                else
                {
                    DisplayVersion = GetDisplayVersionUnderKey(wss30IDx64);
                }
            }
            else if (KeyNameExists(sps2003ID, sps2003))
            {
                spVersion = AveSPVersionInternal.SharePoint2003;
                spMoss = AveSPMOSSOrWSSInternal.MOSS;
            }
            else if (KeyNameExists(wss20ID, wss20))
            {
                spVersion = AveSPVersionInternal.SharePoint2003;
                spMoss = AveSPMOSSOrWSSInternal.WSS;
            }
            else
            {
                spVersion = AveSPVersionInternal.None;
                spMoss = AveSPMOSSOrWSSInternal.None;
            }

            //temporary method
            //if (spVersion == AveSPVersionInternal.SharePoint2016)
            //{
            //    var sysRoot = Path.GetPathRoot(Environment.SystemDirectory);
            //    if (File.Exists(Path.Combine(sysRoot, "IAmSP2019.debug")))
            //    {
            //        spVersion = AveSPVersionInternal.SharePoint2019;
            //    }
            //}
            //end
        }

        private static string GetDisplayVersionUnderKey(string winKeyPath)
        {
            var win32UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            var win32KeyPath = win32UninstallKeyPath + winKeyPath;
            var rk = Registry.LocalMachine.OpenSubKey(win32KeyPath, false);
            try
            {
                if (rk != null)
                {
                    var displayVersionValue = rk.GetValue("DisplayVersion");
                    if (displayVersionValue != null) return displayVersionValue.ToString();
                }
                else
                {
                    var win64UninstallKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
                    var win64KeyPath = win64UninstallKeyPath + winKeyPath;
                    rk = Registry.LocalMachine.OpenSubKey(win64KeyPath, false);
                    if (rk != null)
                    {
                        var displayVersionValue = rk.GetValue("DisplayVersion");
                        if (displayVersionValue != null) return displayVersionValue.ToString();
                    }
                }
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }

            return string.Empty;
        }

        private static bool KeyNameExists(string winKeyPath, string displayName)
        {
            var win32UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            var win32KeyPath = win32UninstallKeyPath + winKeyPath;
            var rk = Registry.LocalMachine.OpenSubKey(win32KeyPath, false);
            try
            {
                if (rk != null)
                {
                    var displayNameValue = rk.GetValue("DisplayName");
                    if (displayNameValue != null && displayNameValue.ToString()
                            .StartsWith(displayName, StringComparison.OrdinalIgnoreCase)) return true;
                }
                else
                {
                    var win64UninstallKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
                    var win64KeyPath = win64UninstallKeyPath + winKeyPath;
                    rk = Registry.LocalMachine.OpenSubKey(win64KeyPath, false);
                    if (rk != null)
                    {
                        var displayNameValue = rk.GetValue("DisplayName");
                        if (displayNameValue != null && displayNameValue.ToString()
                                .StartsWith(displayName, StringComparison.OrdinalIgnoreCase)) return true;
                    }
                }
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }

            return false;
        }

        /// <summary>
        ///     这个枚举只表示SharePoint版本，但是不区分MOSS Or WSS
        /// </summary>
        internal enum AveSPVersionInternal
        {
            None = 0,
            SharePoint2003 = 1,
            SharePoint2007 = 2,
            SharePoint2010 = 4,
            SharePoint2013 = 8,
            SharePoint2016 = 16,
            SharePoint2019 = 32,
        }

        /// <summary>
        ///     这个区分是WSS还是MOSS
        /// </summary>
        internal enum AveSPMOSSOrWSSInternal
        {
            None = 0,
            WSS = 1,
            MOSS = 2
        }
    }

    public enum OEMProductType
    {
        DocAve = 1,
        NetApp = 2,
        NetAppToIBM = 6
    }
}