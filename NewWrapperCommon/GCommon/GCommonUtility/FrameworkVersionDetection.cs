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
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace AvePoint.GCommon
{
    /// <summary>
    /// Provides support for determining if a specific version of the .NET Framework runtime is installed and the service pack level for the runtime version.
    /// </summary>
    public static class FrameworkVersionDetection
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<string> GetAllInstalledDotNetVersions()
        {
            List<string> versions = new List<string>();
            try
            {
                using (RegistryKey installedVersions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP", false))
                {
                    foreach (string versionName in installedVersions.GetSubKeyNames())
                    {
                        try
                        {
                            if (versionName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                            {
                                using (RegistryKey versionNameKey = installedVersions.OpenSubKey(versionName))
                                {
                                    if (versionName.StartsWith("v4", StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (string versionTypeName in versionNameKey.GetSubKeyNames())
                                        {
                                            string version = versionNameKey.OpenSubKey(versionTypeName).GetValue("Version").ToString() + ":" + versionTypeName;
                                            versions.Add(version);
                                        }
                                    }
                                    else
                                    {
                                        string version = versionNameKey.GetValue("Version").ToString();
                                        versions.Add(version);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("An error occurred while getting framework version. {0},{1}", versionName, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting all framework versions. {0}", e.ToString());
            }

            return versions;
        }

        public static bool IsInstalled(FrameworkVersion frameworkVersion)
        {
            bool ret = false;
            switch (frameworkVersion)
            {
                case FrameworkVersion.Fx35:
                    ret = IsNetfx35Installed();
                    break;
                case FrameworkVersion.Fx35SP1:
                    ret = IsNetfx35SP1Installed();
                    break;
                case FrameworkVersion.Fx40:
                    ret = IsNetfx40Installed();
                    break;
                case FrameworkVersion.Fx45:
                    ret = IsNetfx45Installed();
                    break;

                default:
                    break;
            }
            return ret;
        }

        private static bool IsNetfx35Installed()
        {
            using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\NET Framework Setup\\NDP\\v3.5", false))
            {
                if (baseKey != null)
                {
                    object install = baseKey.GetValue("Install");
                    if (install != null && string.Compare(install.ToString(), "1", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private static bool IsNetfx35SP1Installed()
        {
            if (IsNetfx35Installed())
            {
                using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\NET Framework Setup\\NDP\\v3.5", false))
                {
                    if (baseKey != null)
                    {
                        object sp = baseKey.GetValue("SP");
                        if (sp != null && int.Parse(sp.ToString()) >= 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsNetfx40Installed()
        {
            using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full", false))
            {
                if (baseKey != null)
                {
                    object install = baseKey.GetValue("Install");
                    if (install != null && string.Compare(install.ToString(), "1", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsNetfx45Installed()
        {
            if (IsNetfx40Installed())
            {
                using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full", false))
                {
                    if (baseKey != null)
                    {
                        object version = baseKey.GetValue("Version");
                        if (version != null && version.ToString().StartsWith("4.5", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 该方法确认是否安装了4.5(包含4.5)以上，5.0(不包含5.0)以下version的Framework。
        /// </summary>
        /// <returns></returns>
        public static bool IsNetfx45AndAboveInstalled()
        {
            if (IsNetfx40Installed())
            {
                using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full", false))
                {
                    if (baseKey != null)
                    {
                        object versionObject = baseKey.GetValue("Version");
                        string versionString = versionObject != null ? versionObject.ToString() : string.Empty;
                        if (!string.IsNullOrEmpty(versionString))
                        {
                            Version version = new Version(versionString);
                            if (version.Major == 4 && version.Minor >= 5)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }

    public enum FrameworkVersion
    {
        Fx35,
        Fx35SP1,
        Fx40,
        Fx45,
    }

}
