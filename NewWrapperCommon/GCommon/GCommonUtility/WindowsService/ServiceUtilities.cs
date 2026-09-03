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
namespace AvePoint.GCommon.Utility.WindowsService
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceProcess;

    public class ServiceUtilities
    {
        public static void ChangeServiceToDelayStart(string serviceName)
        {
            var controller = new ServiceController(serviceName);
            var hService = controller.ServiceHandle.DangerousGetHandle();
            if (hService == IntPtr.Zero)
            {
                throw new Exception("Cannot get the handle to this service.");
            }

            var serviceDelayedAutoStartInfo = new Win32Native.SERVICE_DELAYED_AUTO_START_INFO
            {
                fDelayedAutostart = true
            };
            var lpInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Native.SERVICE_DELAYED_AUTO_START_INFO)));
            Marshal.StructureToPtr((object)serviceDelayedAutoStartInfo, lpInfo, true);
            try
            {
                if (!Win32Native.ChangeServiceConfig2(hService, Win32Native.SERVICE_CONFIG_DELAYED_AUTO_START_INFO, lpInfo))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(lpInfo);
            }
        }
    }
}
