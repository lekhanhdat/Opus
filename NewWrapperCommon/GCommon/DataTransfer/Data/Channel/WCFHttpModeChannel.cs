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
    public class WCFHttpModeChannel_ : BaseWcfTransferChannel<IStreamRelay>
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(WCFHttpModeChannel_), false);

        private Stream mStream = null;
        private bool mCreateConnection = false;//whether connection has been set up
        private long mTransferedNumber = 0;//number has been transfered
        private AveDataTransferQueue mCacheQueue = new AveDataTransferQueue();//cache data for sender transfer;
        private bool mComplete = false;
        private long mTotalSend = 0;
        private ManualResetEvent getStreamEvent;
        private IAsyncResult callbackAsyncResult;
        private Exception endCallbackException;

        public WCFHttpModeChannel_(WcfChannelFactory<IStreamRelay> channelFactory)
            : base(channelFactory)
        {
            getStreamEvent = new ManualResetEvent(false);
        }

        public override bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage)//Open(string sessionId,out string errorMessage, params object[] parameters)
        {
            bool openSuccessfully = false;
            errorMessage = string.Empty;
            this.SessionId = sessionId;
            this.Identifier = identifier;
            this.RemoteIdentifier = remoteIdentifier;
            try
            {
                base.Open(sessionId, identifier, remoteIdentifier, settings, out errorMessage);//, parameters);
                ((ICommunicationObject)Channel).Open();
                if (!mCreateConnection)
                {
                    SessionStatus status = Channel.OpenConnection(sessionId, identifier, false);
                    mLog.Info("open connection with session id:{0} and identifier:{1}, status:{2}", sessionId, identifier, status);
                    openSuccessfully = (status == SessionStatus.IsReady ? true : false);
                    if(!openSuccessfully)
                    {
                        errorMessage = string.Format("The status:{0} is not ready, the system will retry it later.", status);
                    }
                    mCreateConnection = openSuccessfully;
                }
                else
                {
                    //reconnection logic
                    openSuccessfully = Reconnection(sessionId, identifier);
                }
            }
            catch (NoneDataFoundException ex)
            {
                mLog.Error(ex.ToString());
                openSuccessfully = false;
                errorMessage = DataTransferConstants.StreamModeReconnectionError;
            }
            catch (Exception ex)
            {
                openSuccessfully = false;
                errorMessage = "Create Channel Failed:" + ex.ToString();
                mLog.Error(errorMessage);
            }
            return openSuccessfully;
        }

        public override SessionStatus InitSession(string sessionId, string identifier, bool isInited, int timeout)
        {
            try
            {
                endCallbackException = null;
                //mSendFinish = false;
                if (!isInited)
                {
                    mStream = new HttpModeStream();
                    var modeStream = new HttpModeServiceStream();
                    modeStream.HttpStream = mStream;
                    modeStream.SessionId = sessionId;
                    callbackAsyncResult = Channel.BeginPutTransferStream(modeStream, PutTransferStreamCallback, Channel);
                    return SessionStatus.IsReady;
                }
                else
                {
                    var downloadInfo = new HttpModeDownLoadStream();
                    downloadInfo.SessionId = sessionId;
                    callbackAsyncResult = Channel.BeginGetTransferStream(downloadInfo, GetTransferStreamCalBack, Channel);
                    return SessionStatus.InitedOK;
                }
            }
            catch (Exception e)
            {
                mLog.Error("Initialize session {0} failed:{1}", sessionId, e);
            }
            return SessionStatus.NonExist;
        }

        private void PutTransferStreamCallback(IAsyncResult result)
        {
            try
            {
                //SendAsyncResult
                mLog.Info("Send stream is completed, Completed Synchronously:{0}, IsCompleted:{1}, AsyncState:{2}", result.CompletedSynchronously, result.IsCompleted, result.AsyncState);
                ((IStreamRelay)result.AsyncState).EndPutTransferStream(result);
                mStream.Flush();
            }
            catch (Exception e)
            {
                mLog.Warn("Exception in WCFHttpModeChannel End. detail:{0}", e.ToString());
                endCallbackException = e;
                //avoid stop add try catch logic
                //mLog.Error("Call EndPutTransferStream method failed:{0}", e);
                if(mStream is HttpModeStream)
                {
                    ((HttpModeStream)mStream).Stop(string.Format("{0}, put transfer stream callback failed:{1}", DateTime.Now, e));
                }
                else
                {
                    mStream.Flush();
                }
            }
            //CheckFinishStatus();
            //finish write
            //check status and so on
            getStreamEvent.Set();
        }

        private void GetTransferStreamCalBack(IAsyncResult result)
        {
            mLog.Info("Get transfer stream is completed, Completed Synchronously:{0}, IsCompleted:{1}, AsyncState:{2}", result.CompletedSynchronously, result.IsCompleted, result.AsyncState);
            //mStream = Channel.GetTransferStream(downloadInfo).HttpStream;
            try
            {
                mStream = ((IStreamRelay)result.AsyncState).EndGetTransferStream(result).HttpStream;
            }
            catch(Exception ex)
            {
                mLog.Warn("Get transfer stream failed:{0}", ex);

                endCallbackException = ex;
            }
            getStreamEvent.Set();
        } 

        private void CheckCallback()
        {
            if (callbackAsyncResult != null)
            {
                callbackAsyncResult.AsyncWaitHandle.WaitOne();
                callbackAsyncResult = null;
                getStreamEvent.WaitOne();
                getStreamEvent.Reset();
            }
            if (endCallbackException != null)
            {
                throw endCallbackException;
            }
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
                //DataTransferLogger.Logger(AveLogLevel.INFO, "start to send binary:{0}", serialNo);
                var result = SendData(serialNo, buf, true);

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
                CheckCallback();
                
                if(mStream == null)
                {
                    throw new DataTransferNetworkException("Logic Error, the receiver stream is null.");
                }

                byte[] headerInforamtion = new byte[AveDataBlock.DATA_BLOCK_HEADER_LEN];
                SafeRead(mStream, headerInforamtion, 0, headerInforamtion.Length);
                AveDataBlock tempDataBlock = new AveDataBlock(headerInforamtion);
                if (tempDataBlock.Type == AveDataBlockType.RECV_REOPEN_CONNECTION_TYPE)
                {
                    throw new LargeDataInterruptException("Reopen Connection");
                }
                else if(tempDataBlock.Type == AveDataBlockType.ALIVE_TYPE)
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
                    buf = new byte[length];
                    SafeRead(mStream, buf, 0, buf.Length);
                    mTransferedNumber = serialNo;//remember data for reconnection.
                    CurrentWorkStatus.RecordTransferData(false, buf.LongLength);
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

        public override void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            Channel.SetTimeout(sessionId, identifier, timeout, isSender);
        }

        public override bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            if (isSender)
            {
                if (mTotalSend > DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.StreamModeMaxSendSize)
                {
                    var reopenDataBlock = new AveDataBlock(AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    reopenDataBlock.Type = AveDataBlockType.RECV_REOPEN_CONNECTION_TYPE;
                    mStream.Write(reopenDataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    mStream.Flush();

                    CheckCallback();

                    throw new LargeDataInterruptException(string.Format("need to reconnect because the total send data is {0}", mTotalSend));
                }
                else
                {
                    var keepAliveDataBlock = new AveDataBlock(AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    keepAliveDataBlock.Type = AveDataBlockType.ALIVE_TYPE;
                    mStream.Write(keepAliveDataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    mTotalSend += AveDataBlock.DATA_BLOCK_HEADER_LEN;
                }
            }

            return Channel.KeepAlive(sessionId, identifier, isSender);
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
            this.mStream.Dispose();
            //this.getStreamEvent.Close();
            return string.Empty;
        }

        public override void Dispose()
        {
            base.Dispose();
            if(this.getStreamEvent != null)
            {
                this.getStreamEvent.Close();
                this.getStreamEvent = null;
            }
        }

        #region common method

        void SafeRead(Stream stream, byte[] buffer, int offset, int length)
        {
            int readLength = 0;
            int readTotalLength = 0;
            int leftLengthToRead = length;
            while (readTotalLength < length)
            {
                readLength = stream.Read(buffer, offset, leftLengthToRead);
                if (readLength == 0)
                {
                    //can not get data stop
                    break;
                }
                readTotalLength += readLength;
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
        BufferStatus SendData(long serialNo, byte[] buf, bool cacheData)
        {
            if (serialNo == -1)
            {
                mStream.Flush();//

                CheckCallback();

                return CheckReceiverStatusBeforeClose();

                //return BufferStatus.OK;
            }
            else if (serialNo != -1)
            {
                if (mTotalSend + buf.Length > DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.StreamModeMaxSendSize)
                {
                    var reopenDataBlock = new AveDataBlock(AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    reopenDataBlock.Type = AveDataBlockType.RECV_REOPEN_CONNECTION_TYPE;
                    mStream.Write(reopenDataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    mStream.Flush();

                    CheckCallback();

                    throw new LargeDataInterruptException(string.Format("need to reconnect because the total send data is {0}", mTotalSend));
                }
                else
                {
                    var tempDataBlock = new AveDataBlock(buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    tempDataBlock.PutBinary(buf);
                    tempDataBlock.SerialNumber = (uint)serialNo;
                    tempDataBlock.DataSize = buf.Length;
                    mStream.Write(tempDataBlock.Buffer, 0, buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    mTotalSend += buf.Length + AveDataBlock.DATA_BLOCK_HEADER_LEN;
                    if (cacheData)
                    {
                        mCacheQueue.Enque(new DataUnit(serialNo, buf, buf.Length));//cache data for resent logic
                    }
                }
            }

            return BufferStatus.OK;
        }

        /// <summary>
        /// 检查源端发送完毕，目的端还在查询的问题。
        /// </summary>
        /// <returns></returns>
        public BufferStatus CheckReceiverStatusBeforeClose()
        {
            var status = BufferStatus.NotInited;

            mLog.Info("start to verify receiver status before close channel.");

            while(true)
            {
                status = Channel.CheckPeerFinishStatus(this.SessionId, this.Identifier, false);

                CheckCallback();

                if(status == BufferStatus.OK || status == BufferStatus.ReadTimeout || status == BufferStatus.NotInited)
                {
                    break;
                }

                Thread.Sleep(2000);
            }

            if(status == BufferStatus.NotInited)
            {
                status = BufferStatus.OK;
            }

            return status;
        }

        /// <summary>
        /// reconnection, sending resent data for sender 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool Reconnection(string sessionId, string identifier)
        {
            //mSendFinish = false;
            mComplete = false;
            mTotalSend = 0;
            ReconnectionInfo reconnectionInfo = new ReconnectionInfo();
            reconnectionInfo.SerialNum = (int)(identifier == DataTransferConstants.ReceiverIdentifier ? mTransferedNumber : 0);
            reconnectionInfo = Channel.ReopenConnection(sessionId, identifier, reconnectionInfo);
            if (reconnectionInfo.Status == SessionStatus.IsReady)
            {
                InitSession(sessionId, identifier, (identifier == DataTransferConstants.ReceiverIdentifier ? true : false), 0);
                if (identifier == DataTransferConstants.SenderIdentifier)
                {
                    //resent data
                    AveDataTransferQueue dataResent = mCacheQueue.QueryReSentData(reconnectionInfo.SerialNum + 1);
                    while (dataResent.Length > 0)
                    {
                        DataUnit data = dataResent.Deque();
                        SendData(data.SerialNumber, data.Buffer, false);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        //private bool ResetReconnectionInfo(bool waitClose)
        //{
        //    DateTime now = DateTime.Now;
        //    while (DateTime.Now < now.AddMinutes(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DefaultReconnectTimeout))
        //    {
        //        try
        //        {
        //            //Channel.CheckPeerFinishStatus(this.SessionId);
        //            Channel.ResetReconnectionStatus(this.SessionId, waitClose);
        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            mLog.Warn("Exception in reset reconnection information, error:{0}", e.ToString());
        //        }
        //        Thread.Sleep(500);
        //    }
        //    return false;
        //}

        //private void CheckFinishStatus()
        //{
        //    DateTime now = DateTime.Now;
        //    while (DateTime.Now < now.AddMinutes(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DefaultReconnectTimeout))
        //    {
        //        try
        //        {
        //            mComplete = Channel.CheckPeerFinishStatus(this.SessionId);
        //            break;
        //        }
        //        catch (Exception e)
        //        {
        //            mLog.Warn("Exception in check finish status information, error:{0}", e.ToString());
        //        }  
        //        Thread.Sleep(500);
        //    }
        //}

        public override double KeepAliveTimeout
        {
            get
            {
                return 1;
            }
        }
        #endregion
    }
}
