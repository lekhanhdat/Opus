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
using System.Configuration;
using System.IO;

namespace AvePoint.GCommon.Network
{
    internal class AveReconnectableChannel : IAveNetworkChannel
    {
        private IAveNetworkChannel mInnerChannel;
        private IReConnectable mReconnectHandler;

        private int mInternalSocketSequenceNO = 0;

        private Exception reconnectedFailedException;
        private Boolean isReconnectedFailed = false;

        private CircularBuffer mSentDataCache;

        private readonly object locker = new object();

        byte[] mReSendBuffer = null;

        public AveReconnectableChannel(IAveNetworkChannel innerChannel, IReConnectable reconnectHandler)
        {
            this.mInnerChannel = innerChannel;
            this.mReconnectHandler = reconnectHandler;
            
            int cacheBufferSize = 1024 * 1024;
            AveNetworkTrace.TraceVerbose("network cache buffer size is " + cacheBufferSize);
            mSentDataCache = new CircularBuffer(cacheBufferSize);
        }

        #region IAveNetworkChannel Members
        public void Write(byte[] data, int offset, int len)
        {
            Write(data, offset, len, false);
        }

        private void Write(byte[] data, int offset, int len, bool resend)
        {
            int internalSocketSeqNO = this.mInternalSocketSequenceNO;
            try
            {
                if (!resend)
                {
                    this.mSentDataCache.Put(data, offset, len);
                }
                if (!isReconnectedFailed)
                {
                    this.mInnerChannel.Write(data, offset, len);
                }
                else
                {
                    throw reconnectedFailedException;
                }
                return;
            }
            catch (Exception e)
            {
                AveNetworkTrace.TraceError("An error occurred while writing data in AveReconnectableChannel. Exception:{0} ", e);
                lock (locker)
                {
                    if (this.mInternalSocketSequenceNO > internalSocketSeqNO)
                    {
                        //内部socket被其他线程（读线程）reset过，自己不需要再重连
                        AveNetworkTrace.TraceWarning("The inner socket already reset by other thread, skip reconnect. ");
                    }
                    else
                    {
                        mInnerChannel.Close();// 关闭当前的channel之后进行重连
                        try
                        {
                            if (!isReconnectedFailed)
                            {
                                mReconnectHandler.ReConnect();
                            }
                            else
                            {
                                throw reconnectedFailedException;
                            }
                        }
                        catch (Exception exception)
                        {
                            reconnectedFailedException = exception;
                            isReconnectedFailed = true;
                            throw;
                        }
                    }
                    //重连之后已经发送到cache但是对方没有收到的数据会被重连逻辑续传，这里不再需要retry了
                }
            }
        }

        public int Read(byte[] data, int offset, int len)
        {
            while (true)
            {
                int internalSocketSeqNO = this.mInternalSocketSequenceNO;
                try
                {
                    if (!isReconnectedFailed)
                    {
                        return mInnerChannel.Read(data, offset, len);
                    }
                    else
                    {
                        throw reconnectedFailedException;
                    }
                }
                catch (Exception e)
                {
                    AveNetworkTrace.TraceError("An error occurred while reading data in AveReconnectableChannel. Exception:{0} ", e);
                    lock (locker)
                    {
                        if (this.mInternalSocketSequenceNO > internalSocketSeqNO)
                        {
                            //内部socket被其他线程reset过，自己不需要重连，只需要重试即可
                            AveNetworkTrace.TraceWarning("The inner socket already reset by other thread, retry read. ");
                            continue;
                        }
                        else
                        {
                            mInnerChannel.Close();// 关闭当前的channel之后进行重连
                            try
                            {
                                if (!isReconnectedFailed)
                                {
                                    mReconnectHandler.ReConnect();
                                }
                                else 
                                {
                                    throw reconnectedFailedException;
                                }
                            }
                            catch (Exception exception)
                            {
                                reconnectedFailedException = exception;
                                isReconnectedFailed = true;
                                throw;
                            }
                        }
                    }
                }
            }
        }

        public long TotalBytesReceived { get { return mInnerChannel.TotalBytesReceived; } }

        public long TotalReadTime { get { return mInnerChannel.TotalReadTime; } }

        public long TotalBytesSent { get { return mInnerChannel.TotalBytesSent; } }

        public long TotalWriteTime { get { return mInnerChannel.TotalWriteTime; } }

        public int Available { get { return mInnerChannel.Available; } }

        public void Shutdown(ShutDownOptions shutDownOption)
        {
            mInnerChannel.Shutdown(shutDownOption);
        }

        public void Close()
        {
            mInnerChannel.Close();
        }

        public void ReplaceSocket(System.Net.Sockets.Socket socket, Stream socketStream)
        {
            this.mInternalSocketSequenceNO++;
            this.mInnerChannel.ReplaceSocket(socket, socketStream);

            if (this.mReSendBuffer != null && this.mReSendBuffer.Length > 0)
            {
                this.Write(mReSendBuffer, 0, mReSendBuffer.Length, true);
            }
        }

        public bool BackToByte(long offset)
        {
            if (mInnerChannel.BackToByte(offset) == true)
            {
                return true;
            }
            else
            {
                //在cache中能否找到需要重发的数据
                if (mSentDataCache.Size - offset <= mSentDataCache.Capacity)
                {
                    //get data need resend
                    mReSendBuffer = mSentDataCache.GetLatest((int)(mSentDataCache.Size - offset));
                    return true;
                }
                else
                {
                    //需要重发的数据已经丢失，无法恢复连接，上层必须失败
                    return false;
                }

            }
        }

        #endregion
    }
}
