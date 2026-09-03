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
using System.Text;
using Microsoft.Win32;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    internal class AveEnvironment
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);     		
        internal static bool IsSPInstalled(string siteUrl)
        {
            bool isSPInstalled = false;
            try
            {
                if (IsSP2019Installed(siteUrl))
                {
                    return true;
                }
                else if (IsSP2016Installed(siteUrl))
                {
                    return true;
                }
                else if (IsSP2013Installed(siteUrl))
                {
                    return true;
                }
                else if (IsSP2010Installed(siteUrl))
                {
                    return true;
                }
                else if (IsSP2007Installed(siteUrl))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallEnviromentError, e.ToString());
                isSPInstalled = false;
            }
            return isSPInstalled;           
        }

        internal static bool IsSP2019Installed(string siteUrl)
        {
            var isSPInstalled = AvePoint.Common.AveEnv.IsSharePoint2019;
            //bool isSPInstalled = SPSVersionDetector.IsMoss2016Installed();
            if (isSPInstalled)
            {
                if (string.IsNullOrEmpty(siteUrl))
                {
                    return true;
                }
                IAveWebApplication lookupWebApplication = (AveAssemblyUtility.CreateInstance(AveObjectModelFactory.Server19AssemblyName, AveObjectModelFactory.Server19NameSpace + "AveWebApplication", new Type[] { }, new object[] { }) as IAveWebApplication).Lookup(new Uri(siteUrl));
                if (lookupWebApplication == null)
                {
                    log.Log(AveLogLevel.INFO, string.Format("Can not find site {0} or its Web Application", siteUrl));
                    isSPInstalled = false;
                }
            }
            return isSPInstalled;
        }

        internal static bool IsSP2016Installed(string siteUrl)
        {
            bool isSPInstalled =SPSVersionDetector.IsMoss2016Installed();
            if (isSPInstalled)
            {
                if (string.IsNullOrEmpty(siteUrl))
                {
                    return true;
                }
                IAveWebApplication lookupWebApplication = (AveAssemblyUtility.CreateInstance(AveObjectModelFactory.Server16AssemblyName, AveObjectModelFactory.Server16NameSpace + "AveWebApplication", new Type[] { }, new object[] { }) as IAveWebApplication).Lookup(new Uri(siteUrl));
                if (lookupWebApplication == null)
                {
                    log.Log(AveLogLevel.INFO, string.Format("Can not find site {0} or its Web Application", siteUrl));
                    isSPInstalled = false;
                }
            }
            return isSPInstalled;
        }
        internal static bool IsSP2013Installed(string siteUrl)
        {
            bool isSPInstalled = SPSVersionDetector.IsMoss2013Installed();
            if (isSPInstalled)
            {
                if (string.IsNullOrEmpty(siteUrl))
                {
                    return true;
                }
                IAveWebApplication lookupWebApplication = (AveAssemblyUtility.CreateInstance(AveObjectModelFactory.Server13AssemblyName, AveObjectModelFactory.Server13NameSpace + "AveWebApplication", new Type[] { }, new object[] { }) as IAveWebApplication).Lookup(new Uri(siteUrl));
                if (lookupWebApplication == null)
                {
                    log.Log(AveLogLevel.INFO, string.Format("Can not find site {0} or its Web Application", siteUrl));
                    isSPInstalled = false;
                }
            }
            return isSPInstalled;
        }

        internal static bool IsSP2010Installed(string siteUrl)
        {
            bool isSPInstalled = SPSVersionDetector.IsMOSS2010Installed();
            if (isSPInstalled)
            {
                if (string.IsNullOrEmpty(siteUrl))
                {
                    return true;
                }
                IAveWebApplication lookupWebApplication = (AveAssemblyUtility.CreateInstance(AveObjectModelFactory.ServerAssemblyName, AveObjectModelFactory.ServerNameSpace + "AveWebApplication", new Type[] { }, new object[] { }) as IAveWebApplication).Lookup(new Uri(siteUrl));
                if (lookupWebApplication == null)
                {
                    log.Log(AveLogLevel.INFO, string.Format("Can not find site {0} or its Web Application", siteUrl));
                    isSPInstalled = false;
                }
            }
            return isSPInstalled;
        }

        internal static bool IsSP2007Installed(string siteUrl)
        {
            bool isSPInstalled = SPSVersionDetector.IsMOSS2007Installed();
            if (isSPInstalled)
            {
                if (string.IsNullOrEmpty(siteUrl))
                {
                    return true;
                }
                IAveWebApplication lookupWebApplication = (AveAssemblyUtility.CreateInstance(AveObjectModelFactory.Server07AssmeblyName, AveObjectModelFactory.Server07NameSpace + "AveWebApplication", new Type[] { }, new object[] { }) as IAveWebApplication).Lookup(new Uri(siteUrl));
                if (lookupWebApplication == null)
                {
                    log.Log(AveLogLevel.INFO, string.Format("Can not find site {0} or its Web Application", siteUrl));
                    isSPInstalled = false;
                }
            }
            return isSPInstalled;
        }
    }

    internal class SPSVersionDetector
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool? isSP2016Installed = null;
        private static bool? isSP2013Installed = null;
        private static bool? isSP2010Installed = null;
        private static bool? isSP2007Installed = null;
        private static bool? isSP2003Installed = null;
        private static bool CheckSPVersion(RegistryKey key)
        {
            if (key == null)
            {
                return false;
            }
            else
            {
                var installedValue = key.GetValue("SharePoint");

                if (installedValue != null)
                {
                    return string.Equals(installedValue.ToString(), "Installed", StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        public static bool IsMossSEInstalled()
        {
            return AvePoint.Common.AveEnv.IsSharePointSE;
        }
        public static bool IsMoss2019Installed()
        {
            return AvePoint.Common.AveEnv.IsSharePoint2019;
            //try
            //{
            //    if (isSP2016Installed == null)
            //    {
            //        RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\16.0");
            //        isSP2016Installed = CheckSPVersion(key);
            //    }
            //    return (bool)isSP2016Installed;
            //}
            //catch (Exception e)
            //{
            //    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2010Error, e.ToString());
            //    isSP2016Installed = false;
            //    return false;
            //}
        }
        public static bool IsMoss2016Installed()
        {
            try
            {
                if (isSP2016Installed == null)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\16.0");
                    isSP2016Installed = CheckSPVersion(key);
                }
                return (bool)isSP2016Installed;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2010Error, e.ToString());
                isSP2016Installed = false;
                return false;
            }
        }

        public static bool IsMoss2013Installed()
        {
            try
            {
                if (isSP2013Installed == null)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\15.0");
                    isSP2013Installed = CheckSPVersion(key);
                }
                return (bool)isSP2013Installed;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2010Error, e.ToString());
                isSP2013Installed = false;
                return false;
            }
        }

        public static bool IsMOSS2010Installed()
        {
            try
            {
                if (isSP2010Installed == null)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\14.0");
                    isSP2010Installed = CheckSPVersion(key);
                }
                return (bool)isSP2010Installed;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2010Error, e.ToString());
                isSP2010Installed = false;
                return false;
            }
        }

        public static bool IsMOSS2007Installed()
        {
            try
            {
                if (isSP2007Installed == null)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\12.0");
                    isSP2007Installed = CheckSPVersion(key);
                }
                return (bool)isSP2007Installed;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2007Error, e.ToString());
                isSP2007Installed = false;
                return false;
            }
        }

        public static bool IsMOSS2003Installed()
        {
            try
            {
                if (isSP2003Installed == null)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\6.0");
                    isSP2003Installed = CheckSPVersion(key);
                }
                return (bool)isSP2003Installed;
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2003Error, e.ToString());
                isSP2003Installed = false;
                return false;
            }
        }
    }

    public class AveWrapperContext
    {
        private static bool m_bInitialized;
        private static bool m_bDBServiceEnabled = true;

        public static bool DBServiceEnabled
        {
            get
            {
                if (!m_bInitialized)
                {
                    m_bInitialized = true;
                }

                return m_bDBServiceEnabled;
            }
        }
    }
}
