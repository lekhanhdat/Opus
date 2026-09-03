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
using AvePoint.GCommon;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader
{
    public class RecordsAgentUpgraderConst
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static string MSI_AGENT_INSTALLER_URL { get; private set; } = "https://recordspackage.blob.core.windows.net/agentinstaller/CloudAgentInstaller.msi";
        public static string MSP_AGENT_INSTALLER_URL { get; private set; } = "https://recordspackage.blob.core.windows.net/agentinstaller/CloudAgentInstaller_Upgrade.msp";
        public static string AGENT_INSTALLER_INFO_URL { get; private set; } = "https://recordspackage.blob.core.windows.net/agentinstaller/CloudAgentInstaller_Info.json";
        public const string NetworkHost = @"https://www.avepointonlineservices.com";

        public static string INSTALL_FOLDER { get; private set; } = "AvePoint";
        public static string INSTALL_PATH { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), INSTALL_FOLDER);
        public static string INSTALL_LOG_FILE_PATH { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), INSTALL_FOLDER, @"Cloud\Install_{0}.log");
        public static string CLOUD_AGENT_SERVICE_NAME = @"AvePointCloudAgentService";
        public static string GENERAL_FILE_NAME_FORMAT = "CloudAgentUpgrader_{0}.{1}";

        private const string ConfigFileName = "config.json";
        private const string PackageUrlKey = "PackageUrl";
        private const string PatchUrlKey = "PatchUrl";
        private const string AgentInfoUrlKey = "AgentInfoUrl";
        private const string PackageConfig = "PackageConfig";

        private const string MemoryLimitKey = "MemoryLimit";
        private const string DiskFreeSpaceLimitKey = "DiskFreeSpaceLimit";
        private const string DotNetFrameworkVersionKey = "DotNetFrameworkVersion";
        public static double DiskFreeSpaceLimit { get; private set; } = 5; // GB
        public static double MemoryLimit { get; private set; } = 1; // GB
        public const double BytesPerGB = 1024 * 1024 * 1024; // 1GB
        public static DotNetFramework DotNetFrameworkVersion { get; private set; } = DotNetFramework.V4_8;

        public static void ReadConfigFile()
        {
            var exe = Assembly.GetExecutingAssembly().Location;
            var path = exe.Split(Path.DirectorySeparatorChar).ToList();
            path.RemoveAt(path.Count - 1);
            path.Add(PackageConfig);
            path.Add(ConfigFileName);
            var jsonFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), path);
            if (!File.Exists(jsonFilePath))
            {
                s_logger.Warn($"[Configuration] Config file not found: {jsonFilePath}, use default settings.");
                return;
            }
            try
            {
                var json = File.ReadAllText(jsonFilePath).Replace(@"\r\n", string.Empty);
                s_logger.Error($"[Configuration] {json}");
                var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (config == null)
                {
                    s_logger.Error("[Configuration] invalid file");
                    return;
                }

                if (config.ContainsKey(PackageUrlKey) && config[PackageUrlKey] is string url && !string.IsNullOrWhiteSpace(url))
                {
                    MSI_AGENT_INSTALLER_URL = url;
                    s_logger.Info($"[Configuration] PackageUrl: {MSI_AGENT_INSTALLER_URL}");
                }

                if (config.ContainsKey(PatchUrlKey) && config[PatchUrlKey] is string patchUrl && !string.IsNullOrWhiteSpace(patchUrl))
                {
                    MSP_AGENT_INSTALLER_URL = patchUrl;
                    s_logger.Info($"[Configuration] PatchUrl: {MSP_AGENT_INSTALLER_URL}");
                }

                if (config.ContainsKey(AgentInfoUrlKey) && config[AgentInfoUrlKey] is string agentInfoUrl && !string.IsNullOrWhiteSpace(agentInfoUrl))
                {
                    AGENT_INSTALLER_INFO_URL = agentInfoUrl;
                    s_logger.Info($"[Configuration] AgentInfoUrl: {AGENT_INSTALLER_INFO_URL}");
                }

                if (config.ContainsKey(MemoryLimitKey) && config[MemoryLimitKey] is string ms && double.TryParse(ms, out var mem) && mem > 0)
                {
                    MemoryLimit = mem;
                    s_logger.Info($"[Configuration] MemoryLimit: {MemoryLimit:F1}");
                }

                if (config.ContainsKey(DiskFreeSpaceLimitKey) && config[DiskFreeSpaceLimitKey] is string ss && double.TryParse(ss, out var space) && space > 0)
                {
                    DiskFreeSpaceLimit = space;
                    s_logger.Info($"[Configuration] DiskFreeSpaceLimit: {DiskFreeSpaceLimit:F1}");
                }

                if (config.ContainsKey(DotNetFrameworkVersionKey) && config[DotNetFrameworkVersionKey] is string v && Enum.TryParse(v, out DotNetFramework version))
                {
                    DotNetFrameworkVersion = version;
                    s_logger.Info($"[Configuration] DotNetFrameworkVersion: {DotNetFrameworkVersion} | {(int)DotNetFrameworkVersion}");
                }
            }
            catch (Exception e)
            {
                s_logger.Error("[Configuration] An error occured while read the configuration file:", e);
            }
        }
    }

    public enum DotNetFramework
    {
        V4_5 = 378389,
        V4_5_1 = 378675,
        V4_5_2 = 379893,
        V4_6 = 393295,
        V4_6_1 = 394254,
        V4_6_2 = 394802,
        V4_7 = 460798,
        V4_7_1 = 461308,
        V4_7_2 = 461808,
        V4_8 = 528040
    }
}
