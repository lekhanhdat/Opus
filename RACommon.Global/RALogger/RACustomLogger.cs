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
using System.IO;
using System.Text.RegularExpressions;

namespace AvePoint.RA.Common
{
    public class RACustomLogger
    {

        public static void Init(string tenantId, string jobId, bool isDevEnv)
        {
            isDev = isDevEnv;
            try
            {
                string pattern = @"^[a-zA-Z0-9-_]+$";
                Regex regex = new Regex(pattern);

                if (!regex.IsMatch(jobId))
                {
                    throw new ArgumentException("Invalid args jobId.");
                }

                if (isDev)
                {
                    if (!Guid.TryParse(tenantId, out var parsedGuid))
                    {
                        throw new ArgumentException("Invalid args tenantId.");
                    }
                    jobLogFolderPath = Path.Combine(Path.DirectorySeparatorChar + "logs", tenantId, jobId);
                }
                else
                {
                    jobLogFolderPath = Path.Combine(Path.DirectorySeparatorChar + "logs", "Reports");
                }
                TooManyRequestLogFileName = $"{jobId}_Reporter.log";
                JobProgressLogFileName = $"{jobId}_Process.log";
            }
            catch (Exception e)
            {
                throw new Exception($"jon id is not job real id Exception {e}");
            }
        }
        private static bool isDev = false;

        /// <summary>
        /// Set/Get local path  which is used to upload to azure storage
        /// </summary>
        private static string jobLogFolderPath;

        private static string TooManyRequestLogFileName;

        private static string JobProgressLogFileName ;

        public static bool TryWriteJobProgressLog(String message)
        {
            try
            {
                return TryWriteLog(jobLogFolderPath, JobProgressLogFileName, message);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WriteJobProgressLog(String message)
        {
            WriteLog(jobLogFolderPath, JobProgressLogFileName, message);
        }

        public static bool TryWriteToolManyRequestLog(String message)
        {
            try
            {
                return TryWriteLog(jobLogFolderPath, TooManyRequestLogFileName, message);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WriteToolManyRequestLog(String message)
        {
            WriteLog(jobLogFolderPath, TooManyRequestLogFileName, message);
        }

        private static bool TryWriteLog(String logFolder, String logName, String message)
        {
            try
            {
                WriteLog(logFolder, logName, message);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void WriteLog(String logFolder, String logName, String message)
        {
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            String toolManyRequestLogFileFullName = Path.Combine(logFolder, logName);
            using (FileStream fs = File.Open(toolManyRequestLogFileFullName, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(message);
                }
            }
        }
    }
}
