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
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.HttpMode;
using AvePoint.GCommon.Transfer.HttpMode.Common;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace AvePoint.GCommon.Transfer.Data.HttpMode
{
    public class HttpModeSenderUtility : IDisposable
    {
        private IStreamRelay mChannel;
        private FileCycleStream mFileCycleStream;
        private AutoResetEvent mSingleEvent;
        public delegate void TransferExceptionCallBack();
        public event TransferExceptionCallBack mExceptionCallBack;
        private AveThreadWrapper mTransferBackgroudThread;
        private AutoResetEvent mTransferFinishEvent;
        private bool mIsSender;
        private PerformanceMonitor performance;
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(HttpModeSenderUtility));

        public bool IsSender
        {
            set { this.mIsSender = value; }
        }

        public string SessionId
        {
            get;
            set;
        }

        public HttpModeSenderUtility(FileCycleStream fileCycleStream, TransferExceptionCallBack exceptionCallBack)
        {
            mFileCycleStream = fileCycleStream;
            mSingleEvent = new AutoResetEvent(false);
            mTransferFinishEvent = new AutoResetEvent(false);
            mExceptionCallBack += exceptionCallBack;
            performance = new PerformanceMonitor();
        }

        public void StartTransferThread()
        {
            mTransferBackgroudThread = AveThreadUtility.StartThread(TransferLoop, "HttpModeSenderThread", string.Empty);
        }

        private void TransferLoop()
        {
            mSingleEvent.Reset();
            if (mIsSender)
            {
                SenderThread();
            }
            else
            {
                ReceiverThread();
            }
            mTransferFinishEvent.Set();
        }

        public void WaitTransferFinish()
        {
            mTransferFinishEvent.WaitOne();
        }

        private void SenderThread()
        {
            while (mFileCycleStream.CanReadBuffer == 0)//wait until data is write to 
            {
                Thread.Sleep(1000);
                continue;
            }
            while (!mFileCycleStream.IsWriteFinish || (mFileCycleStream.CanReadBuffer != 0))
            {
                int writeLength = 0;
                try
                {
                    writeLength = mChannel.NextTransferDataSize(SessionId, true).Length;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get Transfer Basic information failed, exception:{0}", e.ToString());
                    mExceptionCallBack();
                    mSingleEvent.WaitOne();
                    continue;
                }
                int filecycleReadBuffer = mFileCycleStream.CanReadBuffer;
                writeLength = writeLength > filecycleReadBuffer ? filecycleReadBuffer : writeLength;
                if (writeLength == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    if (performance != null) performance.StartSingleWatch();
                    using (HttpSenderUnit unit = new HttpSenderUnit(mChannel, mFileCycleStream, writeLength))
                    {
                        while (true)
                        {
                            try
                            {
                                unit.DoTransfer(SessionId);
                            }
                            catch (Exception e)
                            {
                                mLogger.Info("Transfer specific length of data failed, exception:{0}", e.ToString());
                                mExceptionCallBack();
                                //reset stream and channel
                                mSingleEvent.WaitOne();
                                unit.ResetStreamChannel(mChannel);
                                continue;
                            }
                            break;
                        }
                    }
                }
                finally
                {
                    if (performance != null) performance.StopSingleWatch(writeLength);
                }
            }

            FinishTransfer();
        }

        private void ReceiverThread()
        {
            while (true)
            {
                int readLength = 0;
                try
                {
                    StreamHader header = mChannel.NextTransferDataSize(SessionId, false);
                    if (header.Finish)
                    {
                        break;
                    }
                    readLength = header.Length;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get Transfer Basic information failed, exception:{0}", e.ToString());
                    mExceptionCallBack();
                    mSingleEvent.WaitOne();
                    continue;
                }
                int filecycleWriteLenght = mFileCycleStream.CanWriteBuffer;
                readLength = readLength > filecycleWriteLenght ? filecycleWriteLenght : readLength;

                if (readLength == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    if (performance != null) performance.StartSingleWatch();
                    using (HttpReceiverUnit unit = new HttpReceiverUnit(mChannel, mFileCycleStream, readLength))
                    {
                        while (true)
                        {
                            try
                            {
                                unit.DoTransfer(SessionId);
                            }
                            catch (Exception e)
                            {
                                mLogger.Info("Transfer specific length of data failed, exception:{0}", e.ToString());
                                mExceptionCallBack();
                                //reset stream and channel
                                mSingleEvent.WaitOne();
                                unit.ResetStreamChannel(mChannel);
                                continue;
                            }
                            break;
                        }
                    }
                }
                finally
                {
                    if (performance != null) performance.StopSingleWatch(readLength);
                }
            }

            mFileCycleStream.FinishWrite();
        }

        private void FinishTransfer()
        {
            while (true)
            {
                try
                {
                    mChannel.TransferFinish(SessionId);
                    break;
                }
                catch (Exception e)
                {
                    mLogger.Warn("Tell Receiver Transfer Finish Exception, Error message:{0}", e.ToString());
                    mExceptionCallBack();
                    mSingleEvent.WaitOne();
                    continue;
                }
            }
        }

        public void ResetChannel(IStreamRelay channel, string sessionId)
        {
            mChannel = channel;
            SessionId = sessionId;
            performance.SessionId = sessionId;
            mSingleEvent.Set();
        }

        public void Dispose()
        {
            if (mSingleEvent != null)
            {
                mSingleEvent.Set();
                mSingleEvent.Close();
                mSingleEvent = null;
            }
            if (mTransferFinishEvent != null)
            {
                mTransferFinishEvent.Close();
                mTransferFinishEvent = null;
            }
            if (performance != null)
            {
                performance.Dispose();
            }
        }

        public void FinishWriteFailedTransfer()
        {
            mTransferFinishEvent.Set();
        }

    }

    public class HttpBasic : IDisposable
    {
        protected IStreamRelay mChannel;
        protected Stream mStream;
        protected FileCycleStream mFileCycleStream;
        protected string mSessionId;
        protected IAsyncResult mCallbackAsyncResult;
        protected Exception mEndCallbackException;
        protected ManualResetEvent mGetStreamEvent;
        protected Exception mKeepAliveException;
        protected DateTime mLastTransferDataTime;
        protected AveThreadWrapper mMonitorTransferThread;
        protected bool mMonitorFinish;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(HttpBasic));

        public HttpBasic(IStreamRelay channel, FileCycleStream fileCycleStream)
        {
            this.mChannel = channel;
            this.mFileCycleStream = fileCycleStream;
            this.mStream = new HttpModeStream();
            this.mSessionId = Guid.NewGuid().ToString();
            this.mGetStreamEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// wcf sender---inprocess receiver
        /// receiver端断网，sender需要定期在几分钟无法发送数据之后主动关闭channel
        /// </summary>
        private void MonitorInnerDataTransferThread()
        {
            while (!mMonitorFinish)
            {
                if (mLastTransferDataTime != DateTime.MinValue && mLastTransferDataTime.AddMinutes(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.ConnectionTimeout) < DateTime.Now)
                {
                    mKeepAliveException = new Exception("No data Transfer through network for 30 minutes, so abort the connection automatically.");
                    mStream.Dispose();
                    mStream = null;
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        public void DoTransfer(string sessionId)
        {
            RestartMonitorThread();
            PreConnection();
            InitStream(sessionId);
            try
            {
                TranferSignelLenghtData();
            }
            finally
            {
                CheckCallback();
            }
        }

        private void RestartMonitorThread()
        {
            if (mMonitorTransferThread != null && mMonitorTransferThread.IsAlive)
            {
                mMonitorFinish = true;
                mMonitorTransferThread.SafeStop(2000, "");
            }
            mMonitorFinish = false;
            mLastTransferDataTime = DateTime.MinValue;
            this.mMonitorTransferThread = AveThreadUtility.StartThread(MonitorInnerDataTransferThread, "MonitorTransferThread", string.Empty);
        }

        public virtual void InitStream(string sessionId) { }

        /// <summary>
        /// 无论是sender还是receiver都有循环等待逻辑，这是由于
        /// wcf receiver---inprocess sender
        /// sender端断网，重连后receiver端需要等待2分钟无法接受数据之后主动退出，才可以继续发送数据
        /// </summary>
        public virtual void PreConnection() { }

        public virtual void TranferSignelLenghtData() { }

        public void CheckCallback()
        {
            if (mCallbackAsyncResult != null)
            {
                mCallbackAsyncResult.AsyncWaitHandle.WaitOne();
                mCallbackAsyncResult = null;
                mGetStreamEvent.WaitOne();
                mGetStreamEvent.Reset();
            }
            if (mEndCallbackException != null)
            {
                throw mEndCallbackException;
            }
            if (mKeepAliveException != null)
            {
                throw mKeepAliveException;
            }
        }

        public virtual void ResetStreamChannel(IStreamRelay channel) { }

        public void DisposeStream()
        {
            if (mStream != null)
            {
                mStream.Dispose();
            }
        }

        public virtual void Dispose()
        {
            this.mMonitorFinish = true;
            if (mStream != null)
            {
                mStream.Dispose();
                mStream = null;
            }
        }
    }

    public class HttpSenderUnit : HttpBasic
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(HttpSenderUnit), false);
        private int mNeedWriteLength;
        private int mWriteLength;
        private int mReopenNumber;
        private CacheBuffer mCacheSendBuffer;//cache data for sender


        public HttpSenderUnit(IStreamRelay channel, FileCycleStream fileCycleStream, int writeLength)
            : base(channel, fileCycleStream)
        {
            this.mNeedWriteLength = writeLength;
            this.mCacheSendBuffer = new CacheBuffer(fileCycleStream.CacheFilePath, this.mNeedWriteLength);
        }


        public override void InitStream(string sessionId)
        {
            var modeStream = new HttpModeServiceStream();
            modeStream.SessionId = sessionId;
            modeStream.SubSessionId = mSessionId;
            modeStream.HttpStream = mStream;
            mEndCallbackException = null;
            mKeepAliveException = null;
            mCallbackAsyncResult = mChannel.BeginPutTransferStream(modeStream, PutTransferStreamCallback, mChannel);
        }

        public override void TranferSignelLenghtData()
        {
            int writeNumber = mCacheSendBuffer.CopyDateToStream(mStream, mReopenNumber);
            mWriteLength = mReopenNumber + writeNumber;
            byte[] buffer = new byte[64 * 1024];
            while (mWriteLength < mNeedWriteLength)
            {
                int read = (64 * 1024 >= mNeedWriteLength - mWriteLength) ? mNeedWriteLength - mWriteLength : 64 * 1024;
                read = mFileCycleStream.Read(buffer, 0, read);
                mCacheSendBuffer.CopyDataToCache(buffer, 0, read);
                mStream.Write(buffer, 0, read);
                mLastTransferDataTime = DateTime.Now;
                mWriteLength += read;
            }
            mStream.Flush();
            buffer = null;
        }


        public override void PreConnection()
        {
            while (true)
            {
                ReconnectionInfo reconnectionInfo = new ReconnectionInfo();
                reconnectionInfo.SerialNum = 0;
                reconnectionInfo = mChannel.ReopenConnection(mSessionId, DataTransferConstants.SenderIdentifier, reconnectionInfo);
                if (reconnectionInfo.Status == SessionStatus.IsInUse)
                {
                    Thread.Sleep(2000);
                    continue;
                }
                this.mReopenNumber = reconnectionInfo.SerialNum;
                break;
            }

        }

        private void PutTransferStreamCallback(IAsyncResult result)
        {
            try
            {
                //SendAsyncResult
                //mLog.Info("Send stream is completed, Completed Synchronously:{0}, IsCompleted:{1}, AsyncState:{2}", result.CompletedSynchronously, result.IsCompleted, result.AsyncState);
                ((IStreamRelay)result.AsyncState).EndPutTransferStream(result);
                mStream.Flush();
            }
            catch (Exception e)
            {
                mLog.Warn("Exception in WCFHttpModeChannel End. detail:{0}", e.ToString());
                mEndCallbackException = e;
                //avoid stop add try catch logic
                //mLog.Error("Call EndPutTransferStream method failed:{0}", e);
                if (mStream is HttpModeStream)
                {
                    ((HttpModeStream)mStream).Stop(string.Format("{0}, put transfer stream callback failed:{1}", DateTime.Now, e));
                }
                else
                {
                    mStream.Flush();
                }
            }
            mGetStreamEvent.Set();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (mCacheSendBuffer != null)
            {
                mCacheSendBuffer.Dispose();
            }
        }

        public override void ResetStreamChannel(IStreamRelay channel)
        {
            DisposeStream();
            this.mChannel = channel;
            mStream = new HttpModeStream();
        }
    }

    public class HttpReceiverUnit : HttpBasic
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(HttpReceiverUnit), false);
        private int mNeedReadLenght;
        private int mReadLength;
        public HttpReceiverUnit(IStreamRelay channel, FileCycleStream fileCycleStream, int readLength)
            : base(channel, fileCycleStream)
        {
            this.mNeedReadLenght = readLength;

        }

        public override void InitStream(string sessionId)
        {
            var downloadInfo = new HttpModeDownLoadStream();
            mKeepAliveException = null;
            downloadInfo.SessionId = sessionId;
            downloadInfo.SubSessionId = mSessionId;
            downloadInfo.DownloadStreamLength = mNeedReadLenght;
            mCallbackAsyncResult = mChannel.BeginGetTransferStream(downloadInfo, GetTransferStreamCalBack, mChannel);
        }

        public override void TranferSignelLenghtData()
        {
            mGetStreamEvent.WaitOne();
            byte[] buffer = new byte[64 * 1024];
            int read;
            while ((read = mStream.Read(buffer, 0, 64 * 1024)) != 0)
            {
                mLastTransferDataTime = DateTime.Now;
                mFileCycleStream.SafeWrite(buffer, 0, read);
                mReadLength += read;
            }
        }


        public override void PreConnection()
        {
            while (true)
            {
                ReconnectionInfo reconnectionInfo = new ReconnectionInfo();
                reconnectionInfo.SerialNum = mReadLength;
                reconnectionInfo = mChannel.ReopenConnection(mSessionId, DataTransferConstants.ReceiverIdentifier, reconnectionInfo);
                if (reconnectionInfo.Status == SessionStatus.IsInUse)
                {
                    Thread.Sleep(2000);
                    continue;
                }
                break;
            }
        }

        private void GetTransferStreamCalBack(IAsyncResult result)
        {
            //mLog.Info("Get transfer stream is completed, Completed Synchronously:{0}, IsCompleted:{1}, AsyncState:{2}", result.CompletedSynchronously, result.IsCompleted, result.AsyncState);
            //mStream = Channel.GetTransferStream(downloadInfo).HttpStream;
            try
            {
                mStream = ((IStreamRelay)result.AsyncState).EndGetTransferStream(result).HttpStream;
            }
            catch (Exception ex)
            {
                mLog.Warn("Get transfer stream failed:{0}", ex);

                mEndCallbackException = ex;
            }
            mGetStreamEvent.Set();
        }

        public void Dispose()
        {
            base.Dispose();
        }

        public override void ResetStreamChannel(IStreamRelay channel)
        {
            DisposeStream();
            this.mChannel = channel;
            //mStream = new HttpModeStream();
        }
    }

    public class CacheBuffer : IDisposable
    {
        //private byte[] mBuffer;
        //private int mLength;
        //private int mOffSet;

        //public CacheBuffer(int length)
        //{
        //    this.mLength = length;
        //    this.mBuffer = new byte[this.mLength];
        //}

        //public void CopyDataToCache(byte[] buffer, int offset, int length)
        //{
        //    Array.Copy(buffer, offset, mBuffer, mOffSet, length);
        //    mOffSet += length;
        //}

        //public int CopyDateToStream(Stream stream, int offset)
        //{
        //    stream.Write(mBuffer, offset, mOffSet - offset);
        //    return mOffSet - offset;
        //}

        private FileCycleUnit mUnit;
        public CacheBuffer(string filePath, int length)
        {
            string fileLocation = Path.Combine(filePath, "Cache_" + Guid.NewGuid().ToString());
            mUnit = new FileCycleUnit(fileLocation, length);
        }

        public void CopyDataToCache(byte[] buffer, int offset, int length)
        {
            mUnit.WriteByte(buffer, offset, length);
        }

        public int CopyDateToStream(Stream stream, int offset)
        {
            byte[] buffer = new byte[64 * 1024];
            int readLength = mUnit.WritePos - offset; int returnValue = readLength;
            while (readLength > 0)
            {
                int readSignle = (readLength > 64 * 1024) ? 64 * 1024 : readLength;
                mUnit.ReadByteOneCycle(buffer, 0, offset, readSignle);
                stream.Write(buffer, 0, readSignle);
                readLength -= readSignle;
                offset += readSignle;
            }
            buffer = null;
            return returnValue;
        }

        public void Dispose()
        {
            if (mUnit != null)
            {
                mUnit.Dispose();
                mUnit = null;
            }
        }
    }

    internal class PerformanceMonitor : IDisposable
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(PerformanceMonitor));
        private Stopwatch watch = new Stopwatch();
        private long totalSize = 0;
        private DateTime logTime = DateTime.Now;
        private Stopwatch singleWatch = new Stopwatch();

        public PerformanceMonitor()
        {
            watch.Reset();
            watch.Start();
        }

        public void StartSingleWatch()
        {
            //singleWatch.Reset();
            singleWatch.Start();
        }

        public void StopSingleWatch(long size)
        {
            singleWatch.Stop();
            totalSize += size;
            if (logTime.AddMinutes(5) < DateTime.Now)
            {
                logger.Info("StreamMode transfer total size :{0} KB, actual time :{1} S, speed :{2} KB/S, SessionId: {3}", totalSize / 1024, singleWatch.Elapsed.TotalSeconds, totalSize / 1024 / singleWatch.Elapsed.TotalSeconds, SessionId);
                logTime = DateTime.Now;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            watch.Stop();
            logger.Info("StreamMode transfer total size :{0} KB, actual time :{1} S, speed :{2}  KB/S, total time :{3} S, SessionId: {4}", totalSize / 1024, singleWatch.Elapsed.TotalSeconds, totalSize / 1024 / singleWatch.Elapsed.TotalSeconds, watch.Elapsed.TotalSeconds, SessionId);
            //watch.Elapsed
        }

        #endregion

        public string SessionId { get; set; }
    }
}
