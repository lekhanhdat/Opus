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
using System.ServiceModel;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;

namespace AvePoint.GCommon.Transfer.Data.Service
{    
    /// <summary>
    /// 提供文件传输服务，目前使用PeerSession实例模式，单并发(可以改成多并发)
    /// </summary>
    [ServiceBehavior(ConcurrencyMode=ConcurrencyMode.Single,InstanceContextMode=InstanceContextMode.PerSession)]
    public class FileTransferService:IFileTransferService,IDisposable
    {
        private FileStream mFile = null;//每一个服务实例使用的文件处理流
        #region IFileTransferService Members

        public bool CheckStatus(string fileName,string fileDir,Boolean openMode)
        {
            if (string.IsNullOrEmpty(fileDir))
            {
                fileDir = DataTransferConfiguration.FileTransferServiceTempFolder;//如果Client不赋值，则使用默认的Service Temp目录
                if (string.IsNullOrEmpty(fileDir))
                {
                    throw new ArgumentNullException("FileTransferServiceTempFolder");
                }
            }
            string realFilePath = Path.Combine(fileDir, fileName); //获得内部定义的临时文件名
            if (openMode)
            {
                if (File.Exists(realFilePath))
                {
                    File.Delete(realFilePath);
                }
                mFile = new FileStream(realFilePath, FileMode.Create);
            }
            else
            {
                mFile = new FileStream(realFilePath, FileMode.Open);
            }
            return true;
        }

        public BufferStatus GetFileBuffer(out byte[] buffer)
        {
            if (mFile.Position == mFile.Length)
            {
                buffer = null;
                return BufferStatus.NoDataFromSender;//文件已经读取到最后，那么直接返回结束状态给客户段。
            }

            var unReadLen = mFile.Length - mFile.Position;

            if (unReadLen < 4 * 1024)
            {
                buffer = new byte[unReadLen];
            }
            else
            {
                buffer = new byte[4 * 1024];                
            }
            mFile.Read(buffer, 0, buffer.Length);
            return BufferStatus.OK;
        }

        public BufferStatus WriteFileBuffer(byte[] buffer)
        {
            mFile.Write(buffer, 0, buffer.Length);
            return BufferStatus.OK;
        }

        #endregion

        #region IDisposable Members
        /// <summary>
        /// 服务实例在调用之后释放一个Session的连接时候自动被调用
        /// 意味着客户端文件发送结束
        /// </summary>
        public void Dispose()
        {
            if (mFile != null)
            {
                mFile.Flush();
                mFile.Close();
            }
        }

        #endregion
    }
}
