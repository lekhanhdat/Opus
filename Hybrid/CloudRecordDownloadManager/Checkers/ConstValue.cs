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
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NLog;

namespace CloudRecordDownloadManager.Checkers {

    public static class ConstValue {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        public enum DotNetFramework {

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

        private const string ConfigFileName = "config.json";
        private const string PackageUrlKey = "PackageUrl";
        private const string PatchUrlKey = "PatchUrl";
        private const string AgentInfoUrlKey = "AgentInfoUrl";

        private const string MemoryLimitKey = "MemoryLimit";
        private const string DiskFreeSpaceLimitKey = "DiskFreeSpaceLimit";
        private const string DotNetFrameworkVersionKey = "DotNetFrameworkVersion";

        public static void ReadConfigFile() {
            var exe = Assembly.GetExecutingAssembly().Location;
            var path = exe.Split(Path.DirectorySeparatorChar).ToList();
            path.RemoveAt(path.Count - 1);
            path.Add(ConfigFileName);
            var jsonFile = string.Join(Path.DirectorySeparatorChar.ToString(), path);
            if (File.Exists(jsonFile)) {
                try {
                    var json = File.ReadAllText(jsonFile).Replace(@"\r\n", string.Empty);
                    Log.Error($"[config] {json}");
                    var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (config == null) {
                        Log.Error("[config] invalid file");
                        return;
                    }

                    if (config.ContainsKey(PackageUrlKey) && config[PackageUrlKey] is string url && !string.IsNullOrWhiteSpace(url)) {
                        PackageUrl = url;
                        Log.Info($"[config] PackageUrl: {PackageUrl}");
                    }

                    if (config.ContainsKey(PatchUrlKey) && config[PatchUrlKey] is string patchUrl && !string.IsNullOrWhiteSpace(patchUrl))
                    {
                        PatchUrl = patchUrl;
                        Log.Info($"[config] PatchUrl: {PatchUrl}");
                    }

                    if (config.ContainsKey(AgentInfoUrlKey) && config[AgentInfoUrlKey] is string agentInfoUrl && !string.IsNullOrWhiteSpace(agentInfoUrl))
                    {
                        AgentInfoUrl = agentInfoUrl;
                        Log.Info($"[config] AgentInfoUrl: {AgentInfoUrl}");
                    }

                    if (config.ContainsKey(MemoryLimitKey) && config[MemoryLimitKey] is string ms && double.TryParse(ms, out var mem) && mem > 0) {
                        MemoryLimit = mem; // GB
                        Log.Info($"[config] MemoryLimit: {MemoryLimit:F1}");
                    }


                    if (config.ContainsKey(DiskFreeSpaceLimitKey) && config[DiskFreeSpaceLimitKey] is string ss && double.TryParse(ss, out var space) && space > 0) {
                        DiskFreeSpaceLimit = space; // GB
                        Log.Info($"[config] DiskFreeSpaceLimit: {DiskFreeSpaceLimit:F1}");
                    }

                    if (config.ContainsKey(DotNetFrameworkVersionKey) && config[DotNetFrameworkVersionKey] is string v && Enum.TryParse(v, out DotNetFramework version)) {
                        DotNetFrameworkVersion = version;
                        Log.Info($"[config] DotNetFrameworkVersion: {DotNetFrameworkVersion} | {(int) DotNetFrameworkVersion}");
                    }
                } catch (Exception e) {
                    Log.Error(e, "[config] invalid file");
                }
            } else {
                Log.Warn("[config] no file");
            }
        }

        // public const string Host = @"ftp://10.1.87.90"; //FTP
        // public static readonly Uri MsiPackageUri = new(Host + "/setup.msi");
        
        public const string NetworkHost = @"https://www.avepointonlineservices.com";
        public static double DiskFreeSpaceLimit { get; private set; } = 5; // GB
        public static double MemoryLimit { get; private set; } = 1; // GB
        public const double BytesPerGB = 1024 * 1024 * 1024; // 1GB
        public static DotNetFramework DotNetFrameworkVersion { get; private set; } = DotNetFramework.V4_8;
        public static string PackageUrl { get; private set; } = "https://recordspackage.blob.core.windows.net/agentinstaller/CloudAgentInstaller.msi";
        public static string PatchUrl { get; private set; } = "https://recordspackage.blob.core.windows.net/agentinstaller/CloudAgentInstaller_Upgrade.msp";
        public static string AgentInfoUrl { get; private set; } = "https://recordspackage.blob.core.windows.net/agentinstaller/CloudAgentInstaller_Info.json";

        /// <summary>
        /// Everytime the product id is changed, it should be added to this list.
        /// </summary>
        public static List<string> PackageIds => new List<string>() 
        { 
            "{C349C41A-87AE-4155-9431-FD2B559FF23E}", 
            "{EE780EAB-9923-49B0-A080-181A1DBD0E6C}", 
            "{0C16D78A-20C8-4F5A-A558-7CB6E49CA3B6}", 
            "{2F470578-5AD8-420C-85D3-5C3424756C2A}", 
            "{83E9AA50-2A25-4A9F-BAC0-36D75961E46F}", 
            "{AFCD33F0-D6C1-45EC-912C-0A32C954B0F9}", 
            "{DF5D64B0-C0C8-99C3-8650-031A7B9ADE3A}",
            "{C03C109A-B679-4CFB-9E0C-C849DEB8A9CA}",
            "{EB827043-A51B-475B-8AE6-C0E7A0A46520}",
            "{6FA0CE2E-51D0-4EAA-B09E-A11802E38F94}",
            "{5D58EC0E-8F86-40E3-91CB-DCBEE8B3249A}",
            "{A3F4E8B1-2C5D-4E6F-9A0B-1C2D3E4F5A6B}",
            "{BB30AFE3-5DA2-4AA0-B401-A983E4DD18BF}",
            "{C079F427-130D-451F-9148-B0B59965FF47}",
            "{3F6A9C2E-8D41-4B5F-9A72-1E6C0D8F4B91}",
            "{3F2504E0-4F89-11D3-9A0C-0305E82C3301}",
            "{A3F7C9D2-8B41-4E6A-9F0D-1C2E7B5A8D93}",
            "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"
        };

        //public const string PackageId = "{AFCD33F0-D6C1-45EC-912C-0A32C954B0F9}";
        //public const string OldPackageId = "{DF5D64B0-C0C8-99C3-8650-031A7B9ADE3A}";
        public const string RegistryUninstall = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        public const string RegistryDisplayVersion = @"DisplayVersion";


        public const string AgentServiceName = @"AvePointCloudAgentService";

    }

}
