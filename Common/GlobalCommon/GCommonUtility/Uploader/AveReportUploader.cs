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
using System.Runtime.Remoting.Messaging;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.GCommon.Utility.Storage;

namespace AvePoint.GCommon
{
    public class AveReportUploader
    {

        static AveLogger logger = AveLogger.GetInstance(typeof(AveReportUploader));
        /// <summary>
        /// 将report文件夹打包上传，并在上传结束后删除report文件夹
        /// </summary>
        /// <param name="jobDir"></param>
        /// <param name="tenantAccountName"></param>
        public static void UploadReport(string jobDir, string tenantAccountName = null)
        {
            string xriString = GCommonRoleConfiguration.ReportStorageXri;
            if (!Directory.Exists(jobDir))
            {
                logger.Warn("upload report failed,folder:{0} does not exist.", jobDir);
                return;
            }
            try
            {
                IXSystem xSystem = XFactory.InstanceSystem(xriString);
            }
            catch (Exception ex)
            {
                logger.Warn("report storage connection string is incorrect.{0}", ex);
                return;
            }
            string[] result = GetZipFile(jobDir, tenantAccountName);
            if (StorageUtil.UploadFileToStorage(result[0], result[1], xriString))
            {
                //上传失败不删除压缩文件
                DeleteFile(result[0]);
            }
        }

        public static void Upload429Report(string jobDir, string tenantAccountName)
        {
            string xriString = GCommonRoleConfiguration.JobLogStorageXri;
            try
            {
                if (!Directory.Exists(jobDir))
                {
                    logger.Warn("upload report failed,folder:{0} does not exist.", jobDir);
                    return;
                }
                var jobId = jobDir.Substring(jobDir.LastIndexOf("\\") + 1);
                var fileFullName = $"{jobDir.TrimEnd(@"\".ToCharArray())}\\{jobId}_TooManyRequestErrors.txt";
                if (!File.Exists(fileFullName))
                {
                    logger.Warn("upload report failed,File:{0} does not exist.", fileFullName);
                    return;
                }
                IXSystem xSystem = XFactory.InstanceSystem(xriString);
            }
            catch (Exception ex)
            {
                logger.Warn("report storage connection string is incorrect.{0}", ex);
                return;
            }
            string[] result = Get429ZipFile(jobDir, tenantAccountName);
            if (UploadFileToStorage(result[0], result[1], xriString))
            {
                DeleteFile(result[0]);
            }
        }

        private static bool UploadFileToStorage(string fileName, string lowName, string xriString)
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
                        logger.Info("upload file successfully.{0}/{1}", storageInfo.HighName, storageInfo.LowName);
                    }
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

        /// <summary>
        /// 压缩文件并删除job的report文件夹
        /// </summary>
        /// <param name="jobDir">job的report文件夹路径</param>
        /// <param name="tenantAccountName"></param>
        /// <returns>数组第一个元素为压缩后保存文件路径，第二个为上传后的文件名</returns>
        private static string[] GetZipFile(string jobDir, string tenantAccountName = null)
        {
            string[] result = new string[2]; 
            if (string.IsNullOrEmpty(tenantAccountName))
            {
                tenantAccountName = CallContext.LogicalGetData("TenantIdentity") as string;
            }
            string lowName = Path.GetFileName(jobDir) +".zip";
            string zipFilePath = jobDir + ".zip";
            lowName = string.IsNullOrEmpty(tenantAccountName) ? lowName : tenantAccountName + '/' + lowName;
            try
            {
                ZipUtil.ZipFolder(jobDir, zipFilePath);
                logger.Info($"Delete Directory [{jobDir}].Location:AveReportUploader.GetZipFile");
                Directory.Delete(jobDir, true);
            }
            catch (Exception ex)
            {
                logger.Error("compress report folder failed.{0}",ex);
            }
            result[0] = zipFilePath;
            result[1] = lowName;
            return result;
        }

        private static string[] Get429ZipFile(string jobDir, string tenantAccountName)
        {
            var jobId = jobDir.Substring(jobDir.LastIndexOf("\\") + 1);
            var fileFullName = $"{jobDir.TrimEnd(@"\".ToCharArray())}\\{jobId}_TooManyRequestErrors";
            string[] result = new string[2];
            string lowName = tenantAccountName + '/' + GetMainJobId(jobId) + $"/{jobId}_TooManyRequestErrors.zip";
            string zipFilePath = fileFullName + ".zip";
            try
            {
                ZipUtil.ZipFolder(jobDir, zipFilePath);
                logger.Info($"Delete Directory [{jobDir}].Location:AveReportUploader.GetZipFile");
                DeleteFile(fileFullName + ".txt");
            }
            catch (Exception ex)
            {
                logger.Error("compress report folder failed.{0}", ex);
            }
            result[0] = zipFilePath;
            result[1] = lowName;
            return result;
        }

        private static string GetMainJobId(string subjobId)
        {
            string mainJobId = subjobId;
            if (subjobId.LastIndexOf("_") != 0)
            {
                mainJobId = subjobId.Substring(0, subjobId.LastIndexOf("_"));
            }
            return mainJobId;
        }

        private static void DeleteFile(string fileName)
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
