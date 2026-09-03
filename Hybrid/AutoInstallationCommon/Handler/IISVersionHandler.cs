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
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace AutoInstallationCommon.Utility.Handler
{
    public enum WebServerTypes
    {
        /**/
        /// <summary>
        ///     IIS 4.0
        /// </summary>
        IIS4 = 4,

        /**/
        /// <summary>
        ///     IIS 5.0,5.1
        /// </summary>
        IIS5 = 5,

        /**/
        /// <summary>
        ///     IIS 6.0
        /// </summary>
        IIS6 = 6,

        /**/
        /// <summary>
        ///     IIS 7.0
        /// </summary>
        IIS7 = 7
    }

    public class IISVersionHandler
    {
        private const string IISRegKeyName = "Software\\Microsoft\\InetStp";
        private const string IISRegKeyValue = "MajorVersion";

        public static IiiSUtil FindIISUtil()
        {
            IiiSUtil iisUtil;
            if (isIIS6()) return iisUtil = new IIS6Util();

            if (isIIS7()) return iisUtil = new IIS7Util();

            var version = iiSVersion();
            if (version == 8)
                return iisUtil = new IIS7Util();
            if (version == 10)
                return iisUtil = new IIS7Util();
            throw new Exception();
        }

        public static bool isIIS6()
        {
            return iiSVersion(null) == 6;
        }


        public static bool isIIS7()
        {
            return iiSVersion(null) == 7;
        }

        private static int iiSVersion(string domainName)
        {
            domainName = "LOCALHOST";
            var path = "IIS://" + domainName + "/W3SVC/INFO";
            DirectoryEntry entry = null;

            try
            {
                entry = new DirectoryEntry(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get IIS Version. " + e);
                return (int) WebServerTypes.IIS5;
            }

            var num = 5;
            try
            {
                num = (int) entry.Properties["MajorIISVersionNumber"].Value;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get IIS Version. " + e);
                return (int) WebServerTypes.IIS5;
            }

            switch (num)
            {
                case 6:
                    return (int) WebServerTypes.IIS6;

                case 7:
                    return (int) WebServerTypes.IIS7;

                default: return (int) WebServerTypes.IIS5;
            }
        }

        private static int iiSVersion()
        {
            var regValue = 0;

            if (GetRegistryValue(RegistryHive.LocalMachine, IISRegKeyName, IISRegKeyValue, RegistryValueKind.DWord,
                out regValue))
                return regValue;
            return 5;
        }

        private static bool GetRegistryValue<T>(RegistryHive hive, string key, string value, RegistryValueKind kind,
            out T data)
        {
            var success = false;
            data = default(T);

            using (var baseKey = RegistryKey.OpenRemoteBaseKey(hive, string.Empty))
            {
                if (baseKey != null)
                    using (var registryKey = baseKey.OpenSubKey(key, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if (registryKey != null)
                            try
                            {
                                // If the key was opened, try to retrieve the value.
                                var kindFound = registryKey.GetValueKind(value);
                                if (kindFound == kind)
                                {
                                    var regValue = registryKey.GetValue(value, null);
                                    if (regValue != null)
                                    {
                                        data = (T) Convert.ChangeType(regValue, typeof(T),
                                            CultureInfo.InvariantCulture);
                                        success = true;
                                    }
                                }
                            }
                            catch (IOException)
                            {
                                // The registry value doesn't exist. Since the
                                // value doesn't exist we have to assume that
                                // the component isn't installed and return
                                // false and leave the data param as the
                                // default value.
                            }
                    }
            }

            return success;
        }
    }
}