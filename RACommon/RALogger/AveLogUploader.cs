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
using log4net;
using log4net.Repository.Hierarchy;

namespace AvePoint.RA.CommonUtil
{
    internal class AveLogUploader: IRALogUploader
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AveLogUploader));

        /// <summary>
        /// job结束后上传最后一个log文件
        /// </summary>
        public void FinallyUpload(string tenantAccountName, string jobId)
        {
            RALogger.WaitForAllLogsFlush();

            string fileName = string.Empty;
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = hierarchy.Root;
            AveSeparativeLogAppender appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveSeparativeLogAppender;

            if (appender == null)
            {
                logger.Warn("upload last log file failed.get the log appender failed.");
                return;
            }

            //tenantAccountName,jobId同时为null直接上传当前的输出log
            if (string.IsNullOrEmpty(tenantAccountName) && string.IsNullOrEmpty(jobId))
            {
                fileName = appender.File;
            }
            else
            {
                fileName = string.Format(appender.BaseFileName, tenantAccountName, jobId);
            }
            var uploadService = LogUploadService.GetInstance();
            uploadService.UploadLog(fileName + "_Finally");
            DateTime endTime = DateTime.Now.AddMinutes(30);
            while (uploadService.HasLogNeedUpload)
            {
                System.Threading.Thread.Sleep(10 * 1000);
                if (DateTime.Now > endTime)
                {
                    break;
                }
            }
            //将appender.NeedWriteFooter设为false，防止在上传结束后创建空log文件
            appender.NeedWriteFooter = false;
        }

        /// <summary>
        /// upload current log file
        /// </summary>
        public void UploadCurrentLog()
        {
            RALogger.WaitForAllLogsFlush();

            string fileName = string.Empty;
            try
            {
                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
                Logger rootLogger = hierarchy.Root;
                AveSeparativeLogAppender appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveSeparativeLogAppender;

                if (appender == null)
                {
                    logger.Warn("upload last log file failed.get the log appender failed.");
                    return;
                }
                fileName = appender.File;
               
                var uploadService = LogUploadService.GetInstance();
                uploadService.UploadLog(fileName);
                DateTime endTime = DateTime.Now.AddMinutes(30);
                while (uploadService.HasLogNeedUpload)
                {
                    System.Threading.Thread.Sleep(10 * 1000);
                    if (DateTime.Now > endTime)
                    {
                        break;
                    }
                }
                //将appender.NeedWriteFooter设为false，防止在上传结束后创建空log文件
                appender.NeedWriteFooter = false;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upload file:{0}", ex.ToString());
            }
           
        }

        public void UploadLog(string fileName)
        {
            var uploadService = LogUploadService.GetInstance();
            uploadService.UploadLog(fileName);
        }
    }
}
