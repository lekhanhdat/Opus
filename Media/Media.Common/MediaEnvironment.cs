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




namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Linq;
    using System.Net;
    using AvePoint.GCommon.Contract;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility;
    using Merged18NResources.MediaCommon;
    #endregion

    #region CodeReview

    [AveCodeReview(
    "2011/12/23",
    "yhzhang@avepoint.com",
    "yhzhang@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion

    public static class MediaEnvironment
    {
        public static Boolean Is64BitOperatingSystem { get { return OSInformation.Is64BitOperatingSystem; } }
        public static Boolean IsWow64Process { get { return OSInformation.IsWow64Process; } }
        public static Boolean Is64BitProcess { get { return OSInformation.Is64BitProcess; } }
        public static Boolean IsDebuggerAttached { get { return OSInformation.IsDebuggerAttached; } }
        public static Boolean IsConsoleAttached { get { return OSInformation.IsConsoleAttached; } }

        public static String OperationSystemDescription { get { return OSInformation.OSName; } }
        public static String OperationSystemName { get { return OSInformation.OSShortName; } }
        public static UInt32 CpuHz { get { return OSInformation.CPUHz; } }
        public static Int32 CpuCount { get { return OSInformation.CPUCount; } }

        public static MediaServer MediaServer { get; set; }
        public static String AuthorizationKey { get; set; }

        public static Boolean CheckMediaDataPortIsAvaliable(Int32 dataPort)
        {
            return MediaServer.MediaServerDataPort == dataPort || CheckTcpPortIsAvaliable(dataPort);
        }

        public static Boolean CheckMediaControlPortIsAvaliable(Int32 controlPort)
        {
            return MediaServer.MediaServerControlPort == controlPort || CheckTcpPortIsAvaliable(controlPort);
        }

        public static Boolean CheckTcpPortIsAvaliable(Int32 tcpPort)
        {
            if (tcpPort >= 65535 || tcpPort <= 1024)
                throw new ArgumentOutOfRangeException(MediaCommonResource.MediaEnvironmentCheckTcpPortIsAvaliableArgumentOutOfRangeExceptionTcpPort);
            return OSInformation.IsTcpPortAvailableTcpPort(tcpPort);
        }

        public static Boolean CheckHostNameOrIPAddressValid(String ipOrHostName)
        {
            var result = default(Boolean);
            if (OSInformation.HostName.Equals(ipOrHostName, StringComparison.OrdinalIgnoreCase)
                || Environment.MachineName.Equals(ipOrHostName, StringComparison.OrdinalIgnoreCase))
                result = true;
            else
            {
                var ipAddress = IPAddress.Parse(ipOrHostName);
                result = OSInformation.HostEntry.AddressList.Contains(ipAddress);
            }
            return result;
        }

        public static void ShowConsole()
        {
            if (!IsConsoleAttached)
            {
                OSInformation.ShowConsole();
            }
        }

        public static void HideConsole()
        {
            if (IsConsoleAttached)
            {
                OSInformation.HideConsole();
            }
        }
    }
}