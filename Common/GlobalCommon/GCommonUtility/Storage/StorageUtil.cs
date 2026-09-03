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

namespace AvePoint.GCommon.Utility.Storage
{
    public class StorageUtil
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(StorageUtil));

        /// <summary>
        /// 将文件上出到azure storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lowName"></param>
        /// <returns></returns>
        public static bool UploadFileToStorage(string fileName, string lowName, string xriString)
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
                            HighName = Dns.GetHostName(),
                            LowName = lowName,
                            Length = fs.Length
                        };
                        StorageResult sr = xSystem.CommitStream(fs, storageInfo);
                        logger.Info("upload file successfully.{0}/{1}", storageInfo.HighName,storageInfo.LowName);
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
    }
}
