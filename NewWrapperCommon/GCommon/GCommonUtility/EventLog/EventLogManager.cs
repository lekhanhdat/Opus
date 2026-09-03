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
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    #endregion

    /// <summary>
    /// this class is used for operating event log
    /// </summary>
    public static class EventLogManager
    {
        /// <summary>
        /// call native method backup event log
        /// </summary>
        /// <param name="eventLogName">the event log name, eg: "Application"</param>
        /// <param name="backupFilePath">the target backup file path</param>
        public static void BackupEventLog(string eventLogName, string backupFilePath)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = Win32Native.OpenEventLog(".", eventLogName);
                if (handle != IntPtr.Zero)
                {
                    Win32Native.BackupEventLog(handle, backupFilePath);
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    Win32Native.CloseEventLog(handle);
                    handle = IntPtr.Zero;
                }
            }
        }
    }
}
