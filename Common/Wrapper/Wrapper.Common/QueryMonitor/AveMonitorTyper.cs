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




namespace AvePoint.Wrapper.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Wrapper.Resource;
    #endregion

    public class AveMonitorTyper
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string monitorLogPath;
        private readonly static object locker = new object();

        static AveMonitorTyper()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                monitorLogPath = Path.Combine(dirInfo.Parent.FullName, "logs\\SP2010WrapperQueryLog");
                DirectoryInfo monitorLogDir = new DirectoryInfo(monitorLogPath);
                if (!monitorLogDir.Exists)
                {
                    monitorLogDir.Create();
                }
            }
            catch(Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCConstrctMonitorTyperError, ex);
                monitorLogPath = string.Empty;
            }
        }

        public static void WriteToFile(string processName, string logContent)
        {
            lock (locker)
            {
                if (!string.IsNullOrEmpty(monitorLogPath))
                {
                    string fileName = processName + "Performance.dat";
                    string logPath = Path.Combine(monitorLogPath, fileName);
                    try
                    {
                        if (File.Exists(logPath))
                        {
                            FileInfo fs = new FileInfo(logPath);
                            if (fs.Length > WrapperConfiguration.MonitorLogFileSize * 1024 * 1000)
                            {
                                TransferFiles(logPath);
                                fs.MoveTo(logPath + ".1");
                            }
                        }
                        if (!File.Exists(logPath))
                        {
                            using (FileStream fs = new FileStream(logPath, FileMode.Create))
                            {
                                fs.Close();
                            }
                        }
                        if (File.Exists(logPath))
                        {
                            StreamWriter sw = new StreamWriter(logPath, true, Encoding.Default);
                            sw.WriteLine(CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(CryptoUtil.ConvertStringToBytes(AveDateTimeUtility.ConvertToType006(DateTime.Now) + "    " + logContent)));
                            sw.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCWriteToFileError, processName, ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// keep at most WrapperConfiguration.MonitorLogFileCount num of logFiles.
        /// </summary>
        /// <param name="logPath"></param>
        private static void TransferFiles(string logPath)
        {
            for (int i = WrapperConfiguration.MonitorLogFileCount; i > 0; i--)
            {
                FileInfo fs = new FileInfo(logPath + "." + i);
                if (fs.Exists)
                {
                    if (i == WrapperConfiguration.MonitorLogFileCount)
                    {
                        fs.Delete();
                    }
                    else
                    {
                        fs.MoveTo(logPath + "." + (i + 1));
                    }
                }
            }
        }
    }
}
