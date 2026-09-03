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
using System.Text;

namespace AutoInstallationCommon.Utility
{
    public class CommonCheckSharePointWrapper
    {
        private static CommonCheckSharePointWrapper sharePointWrapper;

        public static CommonCheckSharePointWrapper GetInstance()
        {
            if (sharePointWrapper == null) sharePointWrapper = new CommonCheckSharePointWrapper();
            return sharePointWrapper;
        }

        private bool VerifyMOSS2010()
        {
            if (GetRegistryKey("14.0"))
                return true;
            return false;
        }

        private bool VerifyMOSS2007()
        {
            if (GetRegistryKey("12.0"))
                return true;
            return false;
        }

        private bool VerifyMOSS2003()
        {
            if (GetRegistryKey("6.0"))
                return true;
            return false;
        }

        private bool GetRegistryKey(string keyversion)
        {
            var rw = CommonRegistryWrapper.GetInstance();
            var checkSubkey32 =
                "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\" + keyversion;
            var checkSubkey64 =
                "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Shared Tools\\Web Server Extensions\\" +
                keyversion;

            if (
                string.Compare(rw.GetValue(checkSubkey32, "SharePoint"),
                    "Installed",
                    StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            if (
                string.Compare(rw.GetValue(checkSubkey64, "SharePoint"),
                    "Installed",
                    StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            return false;
        }

        public string GetMOSSOrWSS()
        {
            var sBuilderSPFinal = new StringBuilder();

            const string wss30 = "Microsoft Windows SharePoint Services 3.0";
            const string wss30ID = "{90120000-1014-0000-0000-0000000FF1CE}";
            const string wss30IDx64 = "{90120000-1014-0000-1000-0000000FF1CE}";
            const string mossDisplay = "Microsoft Office SharePoint Server 2007";
            const string moss2007ID = "{90120000-110D-0000-0000-0000000FF1CE}";
            const string moss2007IDx64 = "{90120000-110D-0000-1000-0000000FF1CE}";
            const string sps2003 = "Microsoft Office SharePoint Portal Server 2003";
            const string sps2003ID = "{610F491D-BE5F-4ED1-A0F7-759D40C7622E}";
            const string wss20 = "Microsoft Windows SharePoint Services 2.0";
            const string wss20ID = "{91140409-7000-11D3-8CFE-0150048383C9}";
            const string moss2010 = "Microsoft SharePoint Server 2010";
            const string moss2010ID = "{20140000-110D-0000-1000-0000000FF1CE}";
            const string moss2010IDNew = "{90140000-110D-0000-1000-0000000FF1CE}";
            const string wss2010 = "Microsoft SharePoint Foundation 2010";
            const string wss2010ID = "{90140000-1110-0000-1000-0000000FF1CE}";

            if (IsMoss2010(moss2010, moss2010ID, moss2010IDNew)) sBuilderSPFinal.Append(moss2010 + "\r\n"); //Moss 2010
            if (IsWSS40(wss2010, wss2010ID)) sBuilderSPFinal.Append(wss2010 + "\r\n"); //WSS4.0
            if (IsMoss2007(mossDisplay, moss2007ID, moss2007IDx64))
                sBuilderSPFinal.Append(mossDisplay + "\r\n"); //Moss 2007
            if (IsWSS30(wss30, wss30ID, wss30IDx64))
                sBuilderSPFinal.Append(wss30 + "\r\n"); //WSS3.0,be careful moss 2007 has wss30ID; 
            if (VerifyKeyNameExist(sps2003ID, sps2003)) sBuilderSPFinal.Append(sps2003 + "\r\n");
            if (VerifyKeyNameExist(wss20ID, wss20))
                sBuilderSPFinal.Append(wss20 + "\r\n"); //WSS2.0,be careful sps2003 has wss20ID.
            if (sBuilderSPFinal.Length == 0) sBuilderSPFinal.Append("None");

            return sBuilderSPFinal.ToString();
        }

        private bool IsWSS30(string wss30, string wss30ID, string wss30IDx64)
        {
            return VerifyKeyNameExist(wss30ID, wss30) || VerifyKeyNameExist(wss30IDx64, wss30);
        }

        private bool IsMoss2007(string mossDisplay, string moss2007ID, string moss2007IDx64)
        {
            return VerifyKeyNameExist(moss2007ID, mossDisplay) || VerifyKeyNameExist(moss2007IDx64, mossDisplay);
        }

        private bool IsWSS40(string wss2010, string wss2010ID)
        {
            return VerifyKeyNameExist(wss2010ID, wss2010);
        }

        private bool IsMoss2010(string moss2010, string moss2010ID, string moss2010IDNew)
        {
            return VerifyKeyNameExist(moss2010ID, moss2010) || VerifyKeyNameExist(moss2010IDNew, moss2010);
        }

        private bool VerifyKeyNameExist(string winKeyPath, string displayName)
        {
            var rw = CommonRegistryWrapper.GetInstance();
            const string win32UninstallKeyPath =
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            var win32KeyPath = win32UninstallKeyPath + winKeyPath;
            var spVersion = string.Empty;
            spVersion = rw.GetValue(win32KeyPath, "DisplayName");
            if (spVersion != null)
            {
                if (spVersion.StartsWith(displayName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            else
            {
                const string win64UninstallKeyPath =
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
                var win64KeyPath = win64UninstallKeyPath + winKeyPath;
                spVersion = rw.GetValue(win64KeyPath, "DisplayName");
                if (spVersion != null)
                    if (spVersion.StartsWith(displayName, StringComparison.OrdinalIgnoreCase))
                        return true;
            }

            return false;
        }

        public string GetSharePointVersion()
        {
            var result = GetMOSSOrWSS();
            if (result.StartsWith("Microsoft SharePoint Server 2010", StringComparison.OrdinalIgnoreCase))
                result = "Microsoft SharePoint Server 2010";
            if (result.StartsWith("Microsoft SharePoint Foundation 2010", StringComparison.OrdinalIgnoreCase))
                result = "Microsoft SharePoint Foundation 2010";
            return result;
        }
    }
}