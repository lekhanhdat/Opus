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
using System.Threading;
using System.IO;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

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
        private AveConnectionOptions connOptions;

        byte[] mReSendBuffer = null;

        public AveReconnectableChannel(IAveNetworkChannel innerChannel, IReConnectable reconnectHandler, AveConnectionOptions connOptions)
        {
            this.mInnerChannel = innerChannel;
            this.mReconnectHandler = reconnectHandler;
            this.connOptions = connOptions;
            AveNetworkTrace.TraceVerbose("network cache buffer size is " + connOptions.SentCacheBufferSize);
            mSentDataCache = new CircularBuffer(connOptions.SentCacheBufferSize);
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
                if (this.connOptions.EnableReconnect)
                {
                    AveNetworkTrace.TraceError("An error occurred while writing data in AveReconnectableChannel. Exception:{0} ", e);
                    lock (this)
                    {
                        if (this.mInternalSocketSequenceNO > internalSocketSeqNO)
                        {
                            //内部socket被其他线程（读线程）reset过，自己不需要再重连
                            AveNetworkTrace.TraceWarning("The inner socket already reset by other thread, retry write. ");
                        }
                        else
                        {
                            mInnerChannel.Close();// 关闭当前的channel之后进行重连
                            try
                            {
                                if (!isReconnectedFailed)
                                {
                                    Thread.Sleep(5000);
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
                else
                    throw;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AveReconnectableChannel is unmodifiable as the cause of being referenced.")]
        public int Read(byte[] data, int offset, int len, bool mustGet)
        {
            while (true)
            {
                int internalSocketSeqNO = this.mInternalSocketSequenceNO;
                try
                {
                    if (!isReconnectedFailed)
                    {
                        return mInnerChannel.Read(data, offset, len, mustGet);
                    }
                    else
                    {
                        throw reconnectedFailedException;
                    }
                }
                catch (Exception e)
                {
                    if (this.connOptions.EnableReconnect)
                    {
                        AveNetworkTrace.TraceError("An error occurred while reading data in AveReconnectableChannel. Exception:{0} ", e);
                        lock (this)
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
                                        Thread.Sleep(5000);
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
                    else
                        throw;
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
                AveNetworkTrace.TraceInformation("Resend completed.  {0} bytes", mReSendBuffer.Length);
            }
            else
            {
                AveNetworkTrace.TraceInformation("No data was resent after replacing socket.");
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "AveReconnectableChannel is unmodifiable as the cause of being referenced.")]
        public bool BackToByte(long offset)
        {
            //在cache中能否找到需要重发的数据
            AveNetworkTrace.TraceInformation("AveReconnectableChannel BackToByte Size :{0} Capacity:{1} Offset:{2}", mSentDataCache.Size, mSentDataCache.Capacity, offset);
            if (mSentDataCache.Size - offset <= mSentDataCache.Capacity)
            {
                //get data need resend
                mReSendBuffer = mSentDataCache.GetLatest((int)(mSentDataCache.Size - offset));
                AveNetworkTrace.TraceInformation("{0} bytes need resend.", mReSendBuffer.Length);
                return true;
            }
            else
            {
                //需要重发的数据已经丢失，无法恢复连接，上层必须失败 
                AveNetworkTrace.TraceInformation("AveReconnectableChannel can not find enough data from cache.");
                return false;
            }
        }

        #endregion
    }
}
