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
using AvePoint.Wrapper.Resource;

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
                isSPInstalled = SPSVersionDetector.IsMOSS2010Installed();
                if (isSPInstalled)
                {
                    if (string.IsNullOrEmpty(siteUrl))
                    {
                        return true;
                    }
                    IAveWebApplication lookupWebApplication = (AveAssemblyUtility.CreateInstance(AveObjectModelFactory.ServerAssemblyName, AveObjectModelFactory.ServerNameSpace + "AveWebApplication", new Type[] { }, new object[] { }) as IAveWebApplication).Lookup(new Uri(siteUrl));
                    if (lookupWebApplication == null)
                    {
                        isSPInstalled = false;
                    }
                }
                else
                {
                    isSPInstalled = SPSVersionDetector.IsMOSS2007Installed();
                    if (isSPInstalled)
                    {
                        if (string.IsNullOrEmpty(siteUrl))
                        {
                            return true;
                        }
                        IAveWebApplication lookupWebApplication = (AveAssemblyUtility.CreateInstance(AveObjectModelFactory.Server07AssmeblyName, AveObjectModelFactory.Server07NameSpace + "AveWebApplication", new Type[] { }, new object[] { }) as IAveWebApplication).Lookup(new Uri(siteUrl));
                        if (lookupWebApplication == null)
                        {
                            isSPInstalled = false;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallEnviromentError, e.ToString());
                isSPInstalled = false;
            }
            return isSPInstalled;           
        }       
    }

    internal class SPSVersionDetector
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);    
        public static bool IsMOSS2010Installed()
        {
            try
            {
                using RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\14.0");
                return string.Equals(key?.GetValue("SharePoint")?.ToString(), "Installed", StringComparison.OrdinalIgnoreCase);                
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2010Error, e.ToString());
                return false;
            }
        }

        public static bool IsMOSS2007Installed()
        {
            try
            {
                using RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Shared Tools\\Web Server Extensions\\12.0");
                return string.Equals(key?.GetValue("SharePoint")?.ToString(), "Installed", StringComparison.OrdinalIgnoreCase);                
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCInstallMoss2007Error, e.ToString());
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
