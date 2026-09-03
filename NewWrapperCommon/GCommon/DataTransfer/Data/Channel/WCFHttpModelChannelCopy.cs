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
using AvePoint.GCommon.Transfer.Factory;
using AvePoint.GCommon.Transfer.HttpMode;
using AvePoint.GCommon.Transfer.HttpMode.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using AvePoint.GCommon.Transfer.Data.HttpMode;

namespace AvePoint.GCommon.Transfer.Data.Channel
{
    public class WCFHttpModeChannel : BaseWcfTransferChannel<IStreamRelay>
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(WCFHttpModeChannel), false);
        private static bool mNetWorkExceptionHappened;
        private long mTransferedNumber = 0;//number has been transfered
        private long mTotalSend = 0;
        private FileCycleStream mFileCycleStream = new FileCycleStream(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.FileCycleStreamSize * 1024 * 1024, (DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.FileCycleStreamSize * 1024 * 1024) / 10);
        private HttpModeSenderUtility mSendUtility;
        private ReadRetryInformation mLastReadInformation = new ReadRetryInformation();
        public WCFHttpModeChannel(WcfChannelFactory<IStreamRelay> channelFactory)
            : base(channelFactory)
        {
            mFileCycleStream.CheckLogicDelegateEvent = CheckStatus;
            mSendUtility = new HttpModeSenderUtility(mFileCycleStream, TransferCallBack);
        }

        public override bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage)//Open(string sessionId,out string errorMessage, params object[] parameters)
        {
            bool initSuccessfully = false;
            errorMessage = string.Empty;
            this.SessionId = sessionId;
            this.Identifier = identifier;
            this.RemoteIdentifier = remoteIdentifier;
            mSendUtility.IsSender = (this.Identifier == DataTransferConstants.SenderIdentifier);
            try
            {
                base.Open(sessionId, identifier, remoteIdentifier, settings, out errorMessage);//, parameters);
                ((ICommunicationObject)Channel).Open();
                Channel.CheckStatus(sessionId, identifier);
                mSendUtility.ResetChannel(Channel, this.SessionId);
                mNetWorkExceptionHappened = false;
                initSuccessfully = true;
            }
            catch (Exception ex)
            {
                initSuccessfully = false;
                errorMessage = "Create Channel Failed:" + ex.ToString();
                mLog.Error(errorMessage);
            }
            return initSuccessfully;
        }

        public override SessionStatus InitSession(string sessionId, string identifier, bool isInited, int timeout)
        {
            try
            {
                SessionStatus status = Channel.InitSession(sessionId, identifier, isInited);
                if (status == SessionStatus.IsReady || status == SessionStatus.InitedOK)
                {
                    //Channel.SetTimeout(sessionId, identifier, timeout, identifier == DataTransferConstants.SenderIdentifier);
                    mSendUtility.StartTransferThread();
                }
                return status;

            }
            catch (Exception e)
            {
                mLog.Error("Initialize session {0} failed:{1}", sessionId, e);
            }
            return SessionStatus.NonExist;
        }

        /// <summary>
        /// use data block to put data into stream
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        public override BufferStatus SendBinary(long serialNo, byte[] buf)
        {
            try
            {
                CheckStatus(false);
                var result = SendData(serialNo, buf);

                if (result == BufferStatus.OK)
                {
                    //没有错误，更新当前的传输状态
                    CurrentWorkStatus.RecordTransferData(true, buf.LongLength);
                }

                //DataTransferLogger.Logger(AveLogLevel.INFO, "end to send binary:{0}", serialNo);

                return result;
            }
            catch (Exception e)
            {
                mLog.Error("send data with number:{0}, details:{1}", serialNo, e.ToString());
                //Reset connection paramter
                //bool waitClose = (e is LargeDataInterruptException) ? true : false;
                //if (!ResetReconnectionInfo(waitClose))
                //{
                //    return BufferStatus.WriteTimeout;//can not reset reconnnection and can not do again
                //}
                throw;
            }
        }

        public override BufferStatus CheckBinary(long serialNo, bool isSender)
        {
            return BufferStatus.OK;
        }

        public override BufferStatus ReceiveBinary(long serialNo, out byte[] buf)
        {
            try
            {
                CheckStatus(false);
                if (mLastReadInformation.IsFinish)
                {
                    return ReceiveOneBinary(serialNo, out buf);
                }
                else
                {
                    return ReceiveBinaryContinue(serialNo, out buf);
                }
            }
            catch (Exception e)
            {
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

        private BufferStatus ReceiveOneBinary(long serialNo, out byte[] buf)
        {
            try
            {
                mLastReadInformation.SetIsHeader();
                byte[] headerInforamtion = new byte[AveDataBlock.DATA_BLOCK_HEADER_LEN];
                SafeRead(headerInforamtion, 0, headerInforamtion.Length);
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
                    this.Channel.ResetReconnectionStatus(this.SessionId, false);//clear memory
                    return BufferStatus.NoDataFromSender;
                }
                else
                {
                    mLastReadInformation.SetIsData(serialNo, length);
                    buf = new byte[length];
                    SafeRead(buf, 0, buf.Length);
                    mTransferedNumber = serialNo;//remember data for reconnection.
                    CurrentWorkStatus.RecordTransferData(false, buf.LongLength);
                    mLastReadInformation.SetIsFinish();
                    return BufferStatus.OK;
                }
            }
            catch (Exception e)
            {
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

        private BufferStatus ReceiveBinaryContinue(long serialNu, out byte[] buf)
        {
            SafeRead(mLastReadInformation.Buffer, mLastReadInformation.BufferOffSet, mLastReadInformation.TotalLength - mLastReadInformation.ReadLength);
            if (mLastReadInformation.IsHeader)
            {

                AveDataBlock tempDataBlock = new AveDataBlock(mLastReadInformation.Buffer);
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
                    this.Channel.ResetReconnectionStatus(this.SessionId, false);//clear memory
                    return BufferStatus.NoDataFromSender;
                }
                else
                {
                    mLastReadInformation.SetIsData(serialNu, length);
                    buf = new byte[length];
                    SafeRead(buf, 0, buf.Length);
                    mTransferedNumber = serialNu;//remember data for reconnection.
                    CurrentWorkStatus.RecordTransferData(false, buf.LongLength);
                }
            }
            buf = new byte[mLastReadInformation.TotalLength];
            Array.Copy(mLastReadInformation.Buffer, 0, buf, 0, mLastReadInformation.TotalLength);
            mLastReadInformation.SetIsFinish();
            return BufferStatus.OK;
        }

        public override void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            Channel.SetTimeout(sessionId, identifier, timeout, isSender);
        }

        public override bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            //if (isSender)
            //{
            //    if (mTotalSend > DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.StreamModeMaxSendSize)
            //    {
            //        var reopenDataBlock = new AveDataBlock(AveDataBlock.DATA_BLOCK_HEADER_LEN);
            //        reopenDataBlock.Type = AveDataBlockType.RECV_REOPEN_CONNECTION_TYPE;
            //        mStream.Write(reopenDataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
            //        mStream.Flush();

            //        CheckCallback();

            //        throw new LargeDataInterruptException(string.Format("need to reconnect because the total send data is {0}", mTotalSend));
            //    }
            //    else
            //    {
            //        var keepAliveDataBlock = new AveDataBlock(AveDataBlock.DATA_BLOCK_HEADER_LEN);
            //        keepAliveDataBlock.Type = AveDataBlockType.ALIVE_TYPE;
            //        mStream.Write(keepAliveDataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
            //        mTotalSend += AveDataBlock.DATA_BLOCK_HEADER_LEN;
            //    }
            //}

            //return Channel.KeepAlive(sessionId, identifier, isSender);
            return true;
        }

        public override string ClearBufferInSession(bool clearAll)
        {
            if (clearAll)
            {
                Channel.ClearSessionManagement(SessionId);
            }
            else
            {
                Channel.ClearSession(SessionId, Identifier);
            }
            return string.Empty;
        }

        public override bool BufferSessionInUse(bool isSender)
        {
            return this.Channel.CheckStreamInUse(this.SessionId, this.Identifier, isSender);
        }

        public override string Close()
        {
            return string.Empty;
        }

        public override void Dispose()
        {
            if (mFileCycleStream != null)
            {
                mFileCycleStream.Dispose();
                mFileCycleStream = null;
            }
            if (mSendUtility != null)
            {
                mSendUtility.Dispose();
                mSendUtility = null;
            }
            base.Dispose();
        }

        #region common method

        void SafeRead(byte[] buffer, int offset, int length)
        {
            int readLength = 0;
            int readTotalLength = 0;
            int leftLengthToRead = length;
            while (readTotalLength < length)
            {
                readLength = mFileCycleStream.Read(buffer, offset, leftLengthToRead);
                if (readLength == 0)
                {
                    //can not get data stop
                    break;
                }
                readTotalLength += readLength;
                mLastReadInformation.CacheBuffer(buffer, offset, readLength);
                leftLengthToRead -= readLength;
                offset += readLength;
            }
        }

        /// <summary>
        /// 由于HTTP Mode不是在Service端存储block，
        /// 所以远端发送-1完毕的时候需要主动检查目的端是否能够正常接收完，
        /// 如果不能，则需要进行retry，如果能，则源端再退出，否则会出现源端发送完，
        /// 但是目的端出现https异常的问题。
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="buf"></param>
        /// <param name="cacheData"></param>
        /// <returns></returns>
        BufferStatus SendData(long serialNo, byte[] buf)
        {
            if (serialNo == -1)
            {
                mFileCycleStream.FinishWrite();//
                mSendUtility.WaitTransferFinish();
                CheckStatus(false);
            }
            else if (serialNo != -1)
            {
                var tempDataBlock = new AveDataBlock(buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                tempDataBlock.PutBinary(buf);
                tempDataBlock.SerialNumber = (uint)serialNo;
                tempDataBlock.DataSize = buf.Length;
                mFileCycleStream.SafeWrite(tempDataBlock.Buffer, 0, buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                mTotalSend += buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN;
            }

            return BufferStatus.OK;
        }


        public override double KeepAliveTimeout
        {
            get
            {
                return 1;
            }
        }

        private void TransferCallBack()
        {
            mNetWorkExceptionHappened = true;
            if (mFileCycleStream.IsWriteFinish)
            {
                mSendUtility.FinishWriteFailedTransfer();
            }
        }

        private void CheckStatus(bool isInitial)
        {
            if (mNetWorkExceptionHappened)
            {
                throw new Exception("Stream Mode Transfer Exception happened.");
            }
        }
        #endregion
    }

    public class ReadRetryInformation
    {
        public long SerialNumber;
        public int TotalLength;
        public bool IsHeader;
        public bool IsFinish = true;
        public byte[] Buffer;
        public int BufferOffSet;
        public int ReadLength;

        public void SetIsHeader()
        {
            IsFinish = false;
            IsHeader = true;
            SerialNumber = 0;
            TotalLength = AveDataBlock.DATA_BLOCK_HEADER_LEN;
            Buffer = new byte[TotalLength];
            BufferOffSet = 0;
            ReadLength = 0;
        }

        public void SetIsData(long serialNumber, int length)
        {
            IsFinish = false;
            IsHeader = false;
            SerialNumber = serialNumber;
            TotalLength = length;
            Buffer = new byte[TotalLength];
            BufferOffSet = 0;
            ReadLength = 0;
        }

        public void CacheBuffer(byte[] data, int offset, int length)
        {
            Array.Copy(data, offset, Buffer, BufferOffSet, length);
            ReadLength += length;
            BufferOffSet += length;
        }

        public void SetIsFinish()
        {
            IsFinish = true;
            BufferOffSet = 0;
            ReadLength = 0;
            Buffer = null;
        }
    }

}
