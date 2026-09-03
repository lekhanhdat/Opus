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
using System.Net;
using System.Runtime.Remoting.Messaging;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.GCommon.Utility.Storage;
using log4net;
using log4net.Repository.Hierarchy;

namespace AvePoint.GCommon
{
    public class AveLogUploader
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(AveLogUploader));
        /// <summary>
        /// 上传log文件，上传后删除log文件
        /// </summary>
        /// <param name="fileName"></param>
        public void UploadLog(string fileName)
        {
            string xriString = GCommonRoleConfiguration.JobLogStorageXri;
            string tenantFolderName = GetTenantFolderName(fileName);
            FileInfo file = new FileInfo(fileName);
            string lowName = string.Format("{0}", GetLogFileName(fileName, tenantFolderName));
            string jobId = CallContext.LogicalGetData("ThreadJobId") as string;
            //jobId为null或者TenantFolder为CommonFolder需要在文件名后加一个时间戳标识防止重名覆盖丢失之前上传的log
            //因为只使用第一个判断条件就足够筛选故删去对TenantFolderName的判断
            //if(tenantFolderName.Equals(MultiTenantFileLocker.CommonFolder, StringComparison.OrdinalIgnoreCase))
            if (string.IsNullOrEmpty(jobId))
            {
                lowName = string.Format("{0}({1})", lowName, file.LastWriteTime.ToString("yyyy-MM-dd HHmmss"));
            }
            //if (StorageUtil.UploadFileToStorage(fileName, lowName, xriString))
            //{
            //    DeleteFile(fileName);
            //}

            if (DoUpload(fileName, lowName, xriString))
            {
                DeleteFile(fileName);
            }
        }

        /// <summary>
        /// job结束后上传最后一个log文件
        /// </summary>
        public void FinallyUpload(string tenantAccountName, string jobId)
        {
            string fileName = string.Empty;
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = hierarchy.Root;
            AveSeparativeLogAppender appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveSeparativeLogAppender;

            if (appender == null)
            {

                var bufferingForwardingAppender = rootLogger.GetAppender("BufferingForwarder") as log4net.Appender.BufferingForwardingAppender;

                if (bufferingForwardingAppender != null)
                {
                    bufferingForwardingAppender.Flush();
                    (bufferingForwardingAppender.Appenders[0] as AveAzureBlobStorageLogAppender).FinishAndSwitch2CommonLog();
                    WaitForUploadCompleted();
                }
                else
                {
                    logger.Warn("upload last log file failed.get the log appender failed.");
                }
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
            LogUploadService.GetInstance().UploadLog(fileName);
            WaitForUploadCompleted();
            //将appender.NeedWriteFooter设为false，防止在上传结束后创建空log文件
            appender.NeedWriteFooter = false;
        }

        private void WaitForUploadCompleted()
        {
            var uploadService = LogUploadService.GetInstance();
            DateTime endTime = DateTime.Now.AddMinutes(30);
            while (uploadService.HasLogNeedUpload)
            {
                System.Threading.Thread.Sleep(10 * 1000);
                if (DateTime.Now > endTime)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// job结束后上传最后一个log文件
        /// </summary>
        public void UploadLog()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = hierarchy.Root;
            AveRollingFileAppender appender = rootLogger.GetAppender("LogFileAppender") as AveRollingFileAppender;
            if (appender == null)
            {
                logger.Warn("upload last log file failed.get the log rolling appender failed.");
                return;
            }
            logger.Warn("upload file name: {0}.", appender.File);
            var uploadService = LogUploadService.GetInstance();
            uploadService.UploadLog(appender.File);
            DateTime endTime = DateTime.Now.AddMinutes(30);
            while (uploadService.HasLogNeedUpload)
            {
                System.Threading.Thread.Sleep(10 * 1000);
                if (DateTime.Now > endTime)
                {
                    break;
                }
            }
        }

        private bool DoUpload(string fileName, string lowName, string xriString)
        {
            try
            {
                if (!string.IsNullOrEmpty(xriString))
                {
                    logger.Info("upload file to blob stroage. File Name: {0}", fileName);
                    FileInfo file = new FileInfo(fileName);

                    IXSystem xSystem = XFactory.InstanceSystem(xriString);
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        StorageInfo storageInfo = new StorageInfo()
                        {
                            LowName = lowName,
                            Length = fs.Length
                        };
                        StorageResult sr = xSystem.CommitStream(fs, storageInfo);
                    }
                    logger.Info("upload file successfully.");
                    return true;
                }
                else
                {
                    logger.Warn("upload file failed,file name:{0}.it may caused by incorrect StorageXri,please check your configuration", fileName);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while uploading file.{0}", ex);
                return false;
            }
        }

        private string GetLogFileName(string filePath, string tenantFoldername)
        {
            if (string.IsNullOrEmpty(tenantFoldername))
            {
                filePath = filePath.Substring(filePath.LastIndexOf("\\"));
            }
            else
            {
                filePath = filePath.Substring(filePath.IndexOf(tenantFoldername));
            }
            filePath = filePath.Insert(filePath.LastIndexOf('.'), string.Format("_{0}", Dns.GetHostName()));  //trim parent folder path
            return filePath;
        }


        private string GetTenantFolderName(string filePath)
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            if (filePath.Contains(processName))
            {
                filePath = filePath.Substring(0, filePath.IndexOf(processName)).TrimEnd('\\');
                return filePath.Substring(filePath.LastIndexOf('\\') + 1);
            }
            else
            {
                return string.Empty;
            }
        }

        private void DeleteFile(string fileName)
        {
            try
            {
                System.IO.File.Delete(fileName);
            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while delete file.{0}", ex);
            }
        }
    }
}
