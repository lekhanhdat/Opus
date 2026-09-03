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
using System.IO;
using AvePoint.GCommon;

namespace AvePoint.Common
{
    public class JobProcessUtility
    {
        private static AveLogger mlogger = AveLogger.GetInstance(typeof(JobProcessUtility));       

        public static void CheckIfJobCancelled(JobUpdateState jobState, string jobDir = null)
        {
            if (jobState == JobUpdateState.NeedNotUpdate)
            {
                mlogger.Info("the job is being cancelled by user.");
                try
                {
                    if (!string.IsNullOrEmpty(jobDir) && Directory.Exists(jobDir))
                    {
                        CleanJobFolder(jobDir);
                    }
                }
                catch (Exception e)
                {
                    mlogger.Warn("clean log file failed. error message:{0}", e.ToString());
                }
                finally
                {
                    AveLogger.FinallyUploadWithoutOverwrite();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
        }

        private static void CleanJobFolder(string jobDir)
        {
            try
            {
                //AveReportUploader.UploadReport(jobDir);
                mlogger.Info($"Delete Directory [{jobDir}].Location:JobProcessUtility.CleanJobFolder");
                Directory.Delete(jobDir, true);
            }
            catch (Exception ex)
            {
                mlogger.Warn("release jobs file failed. error message:{0}", ex);
                mlogger.Warn("Delete files in {0}", jobDir);
                foreach (var file in Directory.GetFiles(jobDir, "*", SearchOption.TopDirectoryOnly))
                {
                    SafeDelete(file);
                }
            }
        }

        private static void SafeDelete(string jobContextPath)
        {
            try
            {
                if (File.Exists(jobContextPath))
                {
                    File.Delete(jobContextPath);
                }
            }
            catch (Exception e)
            {
                mlogger.Info($"Safe delete file error {e.ToString()}");
            }
        }
    }
}
