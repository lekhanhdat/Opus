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

using AvePoint.GCommon.Network;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Transfer.HttpMode;
using AvePoint.GCommon.Transfer.HttpMode.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AvePoint.GCommon.Transfer.Data.HttpMode;

namespace AvePoint.GCommon.Transfer.Data.Channel
{
    public class HttpModeInProcessChannel : ITransferChannel
    {
        private string mSessionId = string.Empty;//当前正在通讯的数据的sesstion id。
        private string mIdentifier = string.Empty;
        private string mRemoteIdentifier = string.Empty;
        private IStreamRelay mRelayService;//WCF服务端接口定义
        private DataTransferResultStatus mCurrWorkStatus = new DataTransferResultStatus();//当前的数举处理工作状态
        private FileCycleStream mStream;
        private bool mCreateConnection = false;//whether connection has been set up
        private int mTransferedNumber = 0;//number has been transfered
        //private AveDataTransferQueue mCacheQueue = new AveDataTransferQueue();//cache data for sender transfer;
        private AveLogger mLog = AveLogger.GetInstance(typeof(HttpModeInProcessChannel)); 
        private bool mSendFinish = false;
        private int mTotalSend = 0;
        private byte[] lastReadBuffer;
        private int lastReadLength;
        private int readTimeout = 0;
        private DateTime initialTime = DateTime.UtcNow;
        #region ITransferChannel Members

        public DataTransferResultStatus CurrentWorkStatus
        {
            get { return mCurrWorkStatus; }
        }

        public bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage)//Open(string sessionId, out string errorMessage, params object[] parameters)
        {
            bool openSuccessfully = false;
            errorMessage = string.Empty;
            this.Close();
            mSessionId = sessionId;
            mIdentifier = identifier;
            mRemoteIdentifier = remoteIdentifier;
            try
            {
                mRelayService = new StreamModeService();
                openSuccessfully = true;
            }
            catch (Exception ex)
            {
                openSuccessfully = false;
                errorMessage = "Create Channel Failed:" + ex.ToString();
            }

            return openSuccessfully;
        }

        public SessionStatus InitSession(string sessionId, string identifier, bool isInited, int timeout)
        {
            readTimeout = timeout;
            SessionStatus status = mRelayService.InitSession(sessionId, identifier, isInited);
            if (status == SessionStatus.InitedOK || status == SessionStatus.IsReady)
            {
                mStream = FileCycleStreamCacheUtility.GetFileCycleStream(sessionId);
                mStream.CheckLogicDelegateEvent = CheckReadTimeout;
            }
            return status;
        }

        public BufferStatus SendBinary(long serialNo, byte[] buf)
        {
            try
            {
                var result = SendData(serialNo, buf, true);

                if (result == BufferStatus.OK)
                {
                    //没有错误，更新当前的传输状态
                    CurrentWorkStatus.RecordTransferData(true, buf.LongLength);
                }

                return result;
            }
            catch (Exception e)
            {
                if (e.Message.Equals("Read or write timeout.", StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Error("Network error occurred, send binary timeout");
                    return BufferStatus.WriteTimeout;
                }
                //Reset connection paramter
                bool waitClose = (e is LargeDataInterruptException) ? true : false;
                mRelayService.ResetReconnectionStatus(this.mSessionId, waitClose);
                throw;
            }
        }

        public BufferStatus CheckBinary(long serialNo, bool isSender)
        {
            if (isSender)
            {
                if (mStream.Length == mStream.Capacity || mStream.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN > mStream.Capacity)
                {
                    return BufferStatus.BufferIsFull;
                }
            }

            return BufferStatus.OK;
        }

        public BufferStatus ReceiveBinary(long serialNo, out byte[] buf)
        {
            try
            {
                if (mStream == null)
                {
                    throw new DataTransferNetworkException("Logic Error, the receiver stream is null.");
                }

                byte[] headerInforamtion = new byte[AveDataBlock.DATA_BLOCK_HEADER_LEN];
                mStream.SafeRead(headerInforamtion, 0, headerInforamtion.Length);
                AveDataBlock tempDataBlock = new AveDataBlock(headerInforamtion);
                if (tempDataBlock.Type == AveDataBlockType.RECV_REOPEN_CONNECTION_TYPE)
                {
                    throw new LargeDataInterruptException("Reopen Connection");
                }
                else if (tempDataBlock.Type == AveDataBlockType.ALIVE_TYPE)
                {
                    buf = null;
                    return BufferStatus.NoBuffer;
                }
                int length = tempDataBlock.DataSize;
                if (length == 0)
                {
                    buf = new byte[length];
                    return BufferStatus.NoDataFromSender;
                }
                else
                {
                    buf = new byte[length];
                    mStream.SafeRead(buf, 0, buf.Length);
                    CurrentWorkStatus.RecordTransferData(false, buf.LongLength);
                    return BufferStatus.OK;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Equals("Read or write timeout.", StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Error("Network error occurred, receive binary timeout");
                    buf = null;
                    return BufferStatus.ReadTimeout;
                }
                mLog.Warn("Receive binary with serial number:{0} failed:{1}", serialNo, e);
                //bool waitClose = (e is LargeDataInterruptException) ? true : false;
                //if (!ResetReconnectionInfo(waitClose))
                //{
                //    buf = null;
                //    return BufferStatus.ReadTimeout;
                //}
                throw;
            }
        }

        public void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            mRelayService.SetTimeout(sessionId, identifier, timeout, isSender);
        }

        public bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            return mRelayService.KeepAlive(sessionId, identifier, isSender);
        }

        public string Close()
        {
            return string.Empty;
        }

        public string ClearBufferInSession(bool clearAll)
        {
            if (clearAll)
            {
                mRelayService.ClearSessionManagement(mSessionId);
            }
            else
            {
                mRelayService.ClearSession(mSessionId, mIdentifier);
            }
            return string.Empty;
        }

        public bool BufferSessionInUse(bool isSender)
        {
            return mRelayService.CheckStreamInUse(mSessionId, mIdentifier, isSender);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        private int SafeRead(Stream stream, byte[] buffer, int offset, int length)
        {
            int readTotalLength = 0;
            int leftLengthToRead = length;
            while (readTotalLength < length)
            {
                var readLength = stream.Read(buffer, offset, leftLengthToRead);
                if (readLength == 0)
                {
                    //can not get data stop
                    break;
                }
                readTotalLength += readLength;
                leftLengthToRead -= readLength;
                offset += readLength;
            }

            return readTotalLength;
        }

        BufferStatus SendData(long serialNo, byte[] buf, bool cacheData)
        {
            if(serialNo == -1)
            {
                mStream.FinishWrite();
                return BufferStatus.OK;
            }

            AveDataBlock tempDataBlock = new AveDataBlock(buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            tempDataBlock.PutBinary(buf);
            tempDataBlock.SerialNumber = (uint)serialNo;
            tempDataBlock.DataSize = buf.Length;
            mStream.SafeWrite(tempDataBlock.Buffer, 0, buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN);
            mTotalSend += buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN;
            
            return BufferStatus.OK;
        }

        private void CheckReadTimeout(bool isInitial)
        {
            if (isInitial)
            {
                initialTime = DateTime.UtcNow;
                return;
            }
            if (DateTime.UtcNow > initialTime.AddMinutes(readTimeout))
            {
                throw new Exception("Read or write timeout.");
            }
        }

        public double KeepAliveTimeout { get { return int.MaxValue; } }
    }
}
