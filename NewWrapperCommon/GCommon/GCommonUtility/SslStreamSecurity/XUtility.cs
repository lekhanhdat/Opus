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
using Microsoft.Win32;
using System;
using System.Linq;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal static class XUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(XUtility));

        private static object locker = new object();
        private static bool init = false;
        private static bool useNetFrameworkDefaultProvider = true;
        private static bool isTls1Enabled = false;
        public static bool UseNetFrameworkDefaultProvider
        {
            get
            {
                if(init == false)
                {
                    lock(locker)
                    {
                        if(init == false)
                        {
                            Init();
                            init = true;
                        }
                    }
                }

                return useNetFrameworkDefaultProvider;
            }
        }
        public static bool IsTls1Enabled
        {
            get
            {
                if (init == false)
                {
                    lock (locker)
                    {
                        if (init == false)
                        {
                            Init();
                            init = true;
                        }
                    }
                }

                return isTls1Enabled;
            }
        }
        private static void Init()
        {
            var result = UseSysDefaultByGlobalSetting();
            if (result.HasValue)
            {
                useNetFrameworkDefaultProvider = result.Value;
            }
            else
            {
                Func<bool> useDefaultProvider = null;

                useDefaultProvider += () =>
                {
                    var winServer08r2Version = new Version(6, 1, 0, 0); //OS 6.1 is Windows Server 2008 R1 or Windows 7
                    return Environment.OSVersion.Version < winServer08r2Version;
                };

                useDefaultProvider += () =>
                {
                    var net46Version = new Version(4, 0, 30319, 42000); //.net 4.6
                    return (Environment.Version >= net46Version);
                };

                //useDefaultProvider += () =>
                //{
                //    var tls10 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0");
                //    isTls1Enabled = IsProtocolEnabled(tls10);
                //    return isTls1Enabled;
                //};

                //useDefaultProvider += () =>
                //{
                //    var ssl30 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 3.0");
                //    return IsProtocolEnabled(ssl30);
                //};

                //useDefaultProvider += () =>
                //{
                //    var ssl20 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\SSL 2.0");
                //    return IsProtocolEnabled(ssl20);
                //};

                useNetFrameworkDefaultProvider = (useDefaultProvider.GetInvocationList().Any((f) => ((Func<bool>)f)()));
            }
            logger.Info("Using .net framework default upgrade provider: {0}.", useNetFrameworkDefaultProvider);
        }

        /// <summary>
        /// If set by global, force to use sys default.
        /// </summary>
        /// <returns></returns>
        private static bool? UseSysDefaultByGlobalSetting()
        {
            var productKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\AvePoint\DocAve6");
            if (productKey == null)
            {
                productKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Network Appliance\SnapManager for SharePoint 8");
            }
            if (productKey == null)
            {
                productKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\IBM\SnapManager for SharePoint 8");
            }
            if (productKey == null)
            {
                return null;
            }
            var result = "" + productKey.GetValue("UseSystemDefaultTls");
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            if ("0".Equals(result))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// not suitable for Tls11 and Tls12
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool IsProtocolEnabled(RegistryKey key)
        {
            if (key != null)
            {
                var client = key.OpenSubKey("Client");
                if (client != null)
                {
                    if ("0".Equals("" + client.GetValue("Enabled")))
                    {
                        return false;
                    }
                }
                var server = key.OpenSubKey("Server");
                if (server != null)
                {
                    if ("0".Equals("" + server.GetValue("Enabled")))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
