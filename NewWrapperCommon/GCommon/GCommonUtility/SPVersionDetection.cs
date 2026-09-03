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



namespace AvePoint.GCommon
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class SPVersionDetection
    {
        public static int GetSPVersion()
        {
            String wss2010 = "Microsoft SharePoint Foundation 2010";
            String wss2010ID = "{90140000-1110-0000-1000-0000000FF1CE}";
            String wss2010New = "Microsoft SharePoint Foundation 2010 Core";
            String wss2010IDNew = "{90140000-1014-0000-1000-0000000FF1CE}";

            String moss2010 = "Microsoft SharePoint Server 2010";
            String moss2010ID = "{20140000-110D-0000-1000-0000000FF1CE}";
            String moss2010IDNew = "{90140000-110D-0000-1000-0000000FF1CE}";

            String wss2013 = "Microsoft SharePoint Foundation 2013 Core";
            String wss2013ID = "{20150000-1014-0000-1000-0000000FF1CE}";
            String wss2013IDNew = "{90150000-1014-0000-1000-0000000FF1CE}";

            String moss2013 = "Microsoft SharePoint Server 2013";
            String moss2013ID = "{20150000-110D-0000-1000-0000000FF1CE}";
            String moss2013IDNew = "{90150000-110D-0000-1000-0000000FF1CE}";

            if (KeyNameExists(moss2013ID, moss2013)
                || KeyNameExists(moss2013IDNew, moss2013)
                || KeyNameExists(wss2013ID, wss2013)
                || KeyNameExists(wss2013IDNew, wss2013))
            {
                return 2013;
            }
            else if (KeyNameExists(moss2010ID, moss2010)
                || KeyNameExists(moss2010IDNew, moss2010)
                || KeyNameExists(wss2010ID, wss2010)
                || KeyNameExists(wss2010IDNew, wss2010New))
            {
                return 2010;
            }
            return -1;
        }

        private static bool KeyNameExists(string winKeyPath, string displayName)
        {
            string win32UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
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
                    string win64UninstallKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
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
