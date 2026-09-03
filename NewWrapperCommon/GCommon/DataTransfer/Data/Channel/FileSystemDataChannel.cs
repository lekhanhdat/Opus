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
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;
using AvePoint.Media.Storage;

namespace AvePoint.GCommon.Transfer.Data.Channel
{
    /// <summary>
    /// 将处理后的数据直接写入文件系统媒体里面。
    /// 该种模式通讯信道可以支持各种服务接口(IRelay,IFileTransfer)
    /// </summary>
    public class FileSystemDataChannel : ITransferChannel
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(FileSystemDataChannel), false);

        /// <summary>
        /// 内部用于读写的文件流
        /// </summary>
        //private FileStream mInternalStream = null;
        private string filePath = string.Empty;
        //private AveImpersonator impersonator = null;
        private DataTransferResultStatus mCurrWorkStatus = new DataTransferResultStatus();
        private IXSystem instanceSystem = null;
        private XStream instanceStream = null;

        #region ITransferChannel Members

        public DataTransferResultStatus CurrentWorkStatus
        {
            get { return mCurrWorkStatus; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId">要读写的文件名字</param>
        /// <param name="errorMessage"></param>
        /// <param name="parameters">
        /// parameters[0]:表示文件的打开方式
        /// </param>
        /// <returns>文件流是否创建成功</returns>
        public bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage)//Open(string sessionId, out string errorMessage, params object[] parameters)
        {
            errorMessage = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(settings.MediaStorageXri))
                {
                    if (string.IsNullOrEmpty(settings.DataFileDir))
                    {
                        if (!string.IsNullOrEmpty(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.FileTransferServiceTempFolder))
                        {
                            settings.DataFileDir = DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.FileTransferServiceTempFolder;
                        }
                        else
                        {
                            throw new ArgumentNullException("DataFileDirectory");
                        }
                    }
                    settings.MediaStorageXri = XFactory.CreateXri(0, settings.DataFileDir, settings.NetShareDomain, settings.NetShareUsername, settings.NetSharePassword, 0);
                    settings.DataFileDir = string.Empty;
                    //if (settings.DataFileDir.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    settings.MediaStorageXri = XFactory.CreateXri(0, settings.DataFileDir, settings.NetShareUsername, settings.NetSharePassword, 0);
                    //    settings.DataFileDir = string.Empty;
                    //}
                }

                instanceSystem = XFactory.InstanceSystem(settings.MediaStorageXri);
                var verifyResult = instanceSystem.Open();
                if (instanceSystem.SystemHealth == Media.Storage.Util.XSystemHealth.Unknown ||
                    instanceSystem.SystemHealth == Media.Storage.Util.XSystemHealth.Unaccessable)
                {
                    throw new Exception(string.Format("Access physical device:{0} failed:{1}, status:{2}", settings.MediaStorageXri, verifyResult.Message, instanceSystem.SystemHealth));
                }
                var fileInfo = new StorageInfo();
                fileInfo.HighName = settings.DataFileDir;

                if (!instanceSystem.DirectoryExists(fileInfo))
                {
                    var directoryInstance = instanceSystem.OpenDirectory(fileInfo, FileMode.OpenOrCreate);
                }
                fileInfo.LowName = settings.DataFileName;
                if (settings.DataFileMode == OfflineFileMode.OverWrite)
                {
                    instanceStream = instanceSystem.OpenStream(fileInfo, FileMode.OpenOrCreate);
                }
                else if (settings.DataFileMode == OfflineFileMode.Append)
                {
                    instanceStream = instanceSystem.OpenStream(fileInfo, FileMode.OpenOrCreate);
                    instanceStream.Seek(0L, SeekOrigin.End);
                }
                else if (settings.DataFileMode == OfflineFileMode.Open)
                {
                    instanceStream = instanceSystem.OpenStream(fileInfo, FileMode.Open);
                }
                else
                {
                    instanceStream = instanceSystem.OpenStream(fileInfo, FileMode.CreateNew);
                }
                filePath = string.Format("Device:{0}, DirName:{1}, LeafName:{2}", settings.MediaStorageXri, settings.DataFileDir, settings.DataFileName);

                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
            //finally
            //{
            //    if (impersonator != null)
            //    {
            //        impersonator.Undo();
            //    }
            //}
        }

        public SessionStatus InitSession(string sessionId, string identifier, bool isInited, int timeout)
        {
            if (isInited)
            {
                return SessionStatus.InitedOK;
            }
            return SessionStatus.IsReady;
        }

        public BufferStatus SendBinary(long serialNo, byte[] buf)
        {
            try
            {
                instanceStream.Write(buf, 0, buf.Length);
                //没有错误，更新当前的传输状态
                mCurrWorkStatus.RecordTransferData(true, buf.LongLength);
            }
            catch (Exception ex)
            {
                logger.Error("Writer file:{0} failed:{1}", filePath, ex.ToString());
                return BufferStatus.WriteFileError;
            }
            return BufferStatus.OK;
        }

        public BufferStatus CheckBinary(long serialNo, bool isSender)
        {
            return BufferStatus.OK;
        }

        public BufferStatus ReceiveBinary(long serialNo, out byte[] buf)
        {
            /// 返回1，session中没有缓冲区
            /// 返回2,发送端已经不再发送数据
            /// 返回0,表示成功取得一个缓冲区
            /// 返回3，缓冲区顺序状态出错了，不可恢复
            try
            {
                if (instanceStream.Position == instanceStream.Length)
                {
                    buf = null;
                    return BufferStatus.NoDataFromSender;
                }
                buf = new byte[4 * 1024];
                int readLen = instanceStream.Read(buf, 0, buf.Length);
                mCurrWorkStatus.RecordTransferData(false, readLen);
                return BufferStatus.OK;
            }
            catch (Exception ex)
            {
                logger.Error("Read file:{0} failed:{1}", filePath, ex.ToString());
                buf = null;
                return BufferStatus.BufferSerialNoError;
            }
        }

        public void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            //
        }

        public bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            return true;
        }

        public bool BufferSessionInUse(bool isSender)
        {
            return false;
        }

        public string Close()
        {
            try
            {
                if (instanceStream != null)
                {
                    using (instanceStream)
                    {
                        instanceStream.Flush();
                        instanceStream.Close();
                    }
                }
                if (instanceSystem != null)
                {
                    using (instanceSystem)
                    {
                        instanceSystem.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return string.Empty;
        }

        public string ClearBufferInSession(bool clearAll)
        {
            return string.Empty;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        public double KeepAliveTimeout { get { return int.MaxValue; } }
    }
}
