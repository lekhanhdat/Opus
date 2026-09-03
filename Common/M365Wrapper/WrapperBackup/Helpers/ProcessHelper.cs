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

namespace ExchangeUtility.Graph
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using AvePoint.Common;

    using AvePoint.RA.CommonUtil;

    public static class ProcessHelper
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(ProcessHelper));

        public static void EndCurrentProcess(string jobDir)
        {
            if (string.IsNullOrEmpty(jobDir) || !Directory.Exists(jobDir))
            {
                //studo:CloudBackupLogManager.Shutdown();
                Process.GetCurrentProcess().Kill();
                return;
            }
            try
            {
                var filePaths = Directory.GetFiles(jobDir);
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while delete job files. Reason: {0}.", ex);
                    }
                }
                filePaths = Directory.GetFiles(jobDir);
                if (filePaths.Length == 0)
                {
                    try
                    {
                        Directory.Delete(jobDir, true);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while delete job directory. Reason: {0}.", ex);
                    }
                }
                //studo:AveEnv.RemoveAgentTempFolder(ContextLevel.Process);
            }
            catch (Exception e)
            {
                logger.Warn("clean log file failed. error message:{0}", e.ToString());
            }
            finally
            {
                //studo:CloudBackupLogManager.Shutdown();
                Process.GetCurrentProcess().Kill();
            }
        }

        public static long GetCurrentProcessMemory()
        {
            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    return process.WorkingSet64;
                }
            }
            catch
            {
                return 0L;
            }
        }

    }
}