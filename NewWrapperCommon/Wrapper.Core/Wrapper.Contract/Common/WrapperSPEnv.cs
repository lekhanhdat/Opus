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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.Common
{
    //TODO 优化下面的代码，去掉DisplayVersion，如果不使用的话。
    /// <summary>
    /// SP environment
    /// </summary>
    internal class WrapperSPEnv
    {
        static readonly IAveLogger Logger = AveLogger.GetInstance(typeof(WrapperSPEnv), false);

        /// <summary>
        /// 这个枚举只表示SharePoint版本，但是不区分MOSS Or WSS
        /// </summary>
        internal enum SPVersionInternal : int
        {
            None = 0,
            SharePoint2003 = 1,
            SharePoint2007 = 2,
            SharePoint2010 = 4,
            SharePoint2013 = 8,
        }

        /// <summary>
        /// 这个区分是WSS还是MOSS
        /// </summary>
        internal enum SPMOSSOrWSSInternal : int
        {
            None = 0,
            WSS = 1,
            MOSS = 2,
        }

        private static SPVersionInternal spVersion;
        private static SPMOSSOrWSSInternal spMossOrWss;
        private static string displayVersion;
        private static string rootFolder;

        internal static SPVersionInternal SPVersion
        {
            get { return spVersion; }
        }

        internal static SPMOSSOrWSSInternal SPMOSSOrWSS
        {
            get { return spMossOrWss; }
        }

        internal static string DisplayVersion
        {
            get { return displayVersion; }
        }

        internal static string RootFolder { get { return rootFolder; } }

        static WrapperSPEnv()
        {
            EnsureVersion();
        }

        private static void EnsureVersion()
        {
            try
            {
                rootFolder = GetRootLocation();
                GetMossOrWssVersion();
            }
            catch (Exception ex)
            {
                Logger.Error("Get SPVersion Failed:{0}", ex.ToString());
            }
        }

        private static string GetRootLocation()
        {
            for (int i = 20; i > 11; i--)
            {
                string name = string.Format("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\{0}.0", i);
                var registryKey = Registry.LocalMachine.OpenSubKey(name);
                if (registryKey != null)
                {
                    using (registryKey)
                    {
                        string text = registryKey.GetValue("Location") as string;
                        if (!string.IsNullOrEmpty(text))
                        {
                            return Environment.ExpandEnvironmentVariables(text);
                        }
                    }
                }
            }
            return null;
        }

        static void GetMossOrWssVersion()
        {
            const String wss30 = "Microsoft Windows SharePoint Services 3.0";
            const String wss30ID = "{90120000-1014-0000-0000-0000000FF1CE}";
            const String wss30IDx64 = "{90120000-1014-0000-1000-0000000FF1CE}";
            const String moss2007 = "Microsoft Office SharePoint Server 2007";
            const String moss2007ID = "{90120000-110D-0000-0000-0000000FF1CE}";
            const String moss2007IDx64 = "{90120000-110D-0000-1000-0000000FF1CE}";
            const String sps2003 = "Microsoft Office SharePoint Portal Server 2003";
            const String sps2003ID = "{610F491D-BE5F-4ED1-A0F7-759D40C7622E}";
            const String wss20 = "Microsoft Windows SharePoint Services 2.0";
            const String wss20ID = "{91140409-7000-11D3-8CFE-0150048383C9}";
            const String moss2010 = "Microsoft SharePoint Server 2010";
            const String moss2010ID = "{20140000-110D-0000-1000-0000000FF1CE}";
            const String moss2010IDNew = "{90140000-110D-0000-1000-0000000FF1CE}";
            const String wss2010 = "Microsoft SharePoint Foundation 2010";
            const String wss2010ID = "{90140000-1110-0000-1000-0000000FF1CE}";
            const String wss2010New = "Microsoft SharePoint Foundation 2010 Core";
            const String wss2010IDNew = "{90140000-1014-0000-1000-0000000FF1CE}";
            const String wss2013 = "Microsoft SharePoint Foundation 2013 Core";
            const String wss2013ID = "{20150000-1014-0000-1000-0000000FF1CE}";
            const String wss2013IDNew = "{90150000-1014-0000-1000-0000000FF1CE}";
            const String moss2013 = "Microsoft SharePoint Server 2013";
            const String moss2013ID = "{20150000-110D-0000-1000-0000000FF1CE}";
            const String moss2013IDNew = "{90150000-110D-0000-1000-0000000FF1CE}";

            if (KeyNameExists(moss2013ID, moss2013))
            {
                spVersion = SPVersionInternal.SharePoint2013;
                spMossOrWss = SPMOSSOrWSSInternal.MOSS;
                displayVersion = GetDisplayVersionUnderKey(moss2013ID);
            }
            else if (KeyNameExists(moss2013IDNew, moss2013))
            {
                spVersion = SPVersionInternal.SharePoint2013;
                spMossOrWss = SPMOSSOrWSSInternal.MOSS;
                displayVersion = GetDisplayVersionUnderKey(moss2013IDNew);
            }
            else if (KeyNameExists(wss2013ID, wss2013))
            {
                spVersion = SPVersionInternal.SharePoint2013;
                spMossOrWss = SPMOSSOrWSSInternal.WSS;
                displayVersion = GetDisplayVersionUnderKey(wss2013ID);
            }
            else if (KeyNameExists(wss2013IDNew, wss2013))
            {
                spVersion = SPVersionInternal.SharePoint2013;
                spMossOrWss = SPMOSSOrWSSInternal.WSS;
                displayVersion = GetDisplayVersionUnderKey(wss2013IDNew);
            }
            else if (KeyNameExists(moss2010ID, moss2010) || KeyNameExists(moss2010IDNew, moss2010))
            {
                spVersion = SPVersionInternal.SharePoint2010;
                spMossOrWss = SPMOSSOrWSSInternal.MOSS;
                if (KeyNameExists(moss2010ID, moss2010))
                {
                    displayVersion = GetDisplayVersionUnderKey(moss2010ID);
                }
                else
                {
                    displayVersion = GetDisplayVersionUnderKey(moss2010IDNew);
                }
            }
            else if (KeyNameExists(wss2010ID, wss2010))
            {
                spVersion = SPVersionInternal.SharePoint2010;
                spMossOrWss = SPMOSSOrWSSInternal.WSS;
                displayVersion = GetDisplayVersionUnderKey(wss2010ID);
            }
            else if (KeyNameExists(wss2010IDNew, wss2010New))
            {
                spVersion = SPVersionInternal.SharePoint2010;
                spMossOrWss = SPMOSSOrWSSInternal.WSS;
                displayVersion = GetDisplayVersionUnderKey(wss2010New);
            }
            else if (KeyNameExists(moss2007ID, moss2007) || KeyNameExists(moss2007IDx64, moss2007))
            {
                spVersion = SPVersionInternal.SharePoint2007;
                spMossOrWss = SPMOSSOrWSSInternal.MOSS;
                if (KeyNameExists(moss2007ID, moss2007))
                {
                    displayVersion = GetDisplayVersionUnderKey(moss2007ID);
                }
                else
                {
                    displayVersion = GetDisplayVersionUnderKey(moss2007IDx64);
                }
            }
            else if (KeyNameExists(wss30ID, wss30) || KeyNameExists(wss30IDx64, wss30))
            {
                spVersion = SPVersionInternal.SharePoint2007;
                spMossOrWss = SPMOSSOrWSSInternal.WSS;
                if (KeyNameExists(wss30ID, wss30))
                {
                    displayVersion = GetDisplayVersionUnderKey(wss30ID);
                }
                else
                {
                    displayVersion = GetDisplayVersionUnderKey(wss30IDx64);
                }
            }
            else if (KeyNameExists(sps2003ID, sps2003))
            {
                spVersion = SPVersionInternal.SharePoint2003;
                spMossOrWss = SPMOSSOrWSSInternal.MOSS;
            }
            else if (KeyNameExists(wss20ID, wss20))
            {
                spVersion = SPVersionInternal.SharePoint2003;
                spMossOrWss = SPMOSSOrWSSInternal.WSS;
            }
            else
            {
                spVersion = SPVersionInternal.None;
                spMossOrWss = SPMOSSOrWSSInternal.None;
            }
        }

        static string GetDisplayVersionUnderKey(string winKeyPath)
        {
            const string win32UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            string win32KeyPath = win32UninstallKeyPath + winKeyPath;
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(win32KeyPath, false);
            try
            {
                if (rk != null)
                {
                    object displayVersionValue = rk.GetValue("DisplayVersion");
                    if (displayVersionValue != null)
                    {
                        return displayVersionValue.ToString();
                    }
                }
                else
                {
                    const string win64UninstallKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
                    string win64KeyPath = win64UninstallKeyPath + winKeyPath;
                    rk = Registry.LocalMachine.OpenSubKey(win64KeyPath, false);
                    if (rk != null)
                    {
                        object displayVersionValue = rk.GetValue("DisplayVersion");
                        if (displayVersionValue != null)
                        {
                            return displayVersionValue.ToString();
                        }
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

        static bool KeyNameExists(string winKeyPath, string displayName)
        {
            const string win32UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            string win32KeyPath = win32UninstallKeyPath + winKeyPath;
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(win32KeyPath, false);
            try
            {
                if (rk != null)
                {
                    object displayNameValue = rk.GetValue("DisplayName");
                    if (displayNameValue != null && displayNameValue.ToString().StartsWith(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    const string win64UninstallKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
                    string win64KeyPath = win64UninstallKeyPath + winKeyPath;
                    rk = Registry.LocalMachine.OpenSubKey(win64KeyPath, false);
                    if (rk != null)
                    {
                        object displayNameValue = rk.GetValue("DisplayName");
                        if (displayNameValue != null && displayNameValue.ToString().StartsWith(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
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
    }

}
