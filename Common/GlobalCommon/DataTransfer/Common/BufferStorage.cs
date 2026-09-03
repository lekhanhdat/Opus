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

namespace AvePoint.GCommon.Transfer.Common
{
    /// <summary>
    /// 提供基于session区分的内存块的管理服务。
    /// </summary>
    internal class BufferStorage
    {
        //private static int MAX_CACHE_BUFFER = 50;
        private static object mOpLock = new object();//同步操作的锁对象
        private static Dictionary<string, BufferSessionManagement> mSessionManagements = new Dictionary<string, BufferSessionManagement>(StringComparer.OrdinalIgnoreCase);

        private static BufferSessionManagement GetSessionManagement(string sessionId)
        {
            lock (mOpLock)
            {
                if (!mSessionManagements.ContainsKey(sessionId))
                {
                    var session = new BufferSessionManagement(sessionId);
                    mSessionManagements.Add(sessionId, session);
                }
                return mSessionManagements[sessionId];
            }
        }

        /// <summary>
        /// 0---> OK
        /// 1---> The session is InUse
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identify"></param>
        /// <returns></returns>
        public static SessionStatus InitSessionManagement(string sessionId, string identify)
        {
            SessionStatus status = SessionStatus.InitedOK;
            var sessionManagement = GetSessionManagement(sessionId);
            if (!sessionManagement.InitSession(identify))
            {
                status = SessionStatus.IsInUse;
            }

            return status;
        }

        /// <summary>
        /// 0--> exist
        /// 1--> in use
        /// 2--> 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identify"></param>
        /// <returns></returns>
        public static SessionStatus IsSessionManagementExisting(string sessionId, string identify)
        {
            SessionStatus status = SessionStatus.NonExist;

            var sessionManagement = GetSessionManagement(sessionId);
            var session = sessionManagement.GetSession(identify);
            if (session != null)
            {
                if (session.IsAvailable())
                {
                    status = SessionStatus.IsReady;
                }
                else
                {
                    status = SessionStatus.IsInUse;
                }
            }
            else
            {
                status = SessionStatus.NonExist;
            }

            return status;
        }

        /// <summary>
        /// 0--> OK
        /// 1--> Buffer is full.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="serialNo"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static BufferStatus PutBuffer(string sessionId, string identifier, long serialNo, byte[] buffer)
        {
            var session = GetSessionManagement(sessionId).GetSession(identifier);
            if (session != null)
            {
                return session.PutBuffer(serialNo, buffer);
            }
            else
            {
                return BufferStatus.NotInited;
            }
        }

        /// <summary>
        /// 检查buffer是否放入或者取出来
        /// </summary>
        /// <param name="sessionid"></param>
        /// <param name="identifier"></param>
        /// <param name="serialNo"></param>
        /// <param name="isSender"></param>
        /// <returns></returns>
        public static BufferStatus CheckBuffer(string sessionid, string identifier, long serialNo, bool isSender)
        {
            var session = GetSessionManagement(sessionid).GetSession(identifier);
            if (session != null)
            {
                return session.CheckBuffer(serialNo, isSender);
            }
            else
            {
                return BufferStatus.NotInited;
            }
        }

        /// <summary>
        /// -1---> Not inited
        /// 0---> ok
        /// 1---> No Buffer
        /// 2---> No Data from Sender
        /// 3---> Error 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="serialNo"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static BufferStatus GetBuffer(string sessionId, string identifier, long serialNo, out byte[] buffer)
        {
            buffer = new byte[0];
            var session = GetSessionManagement(sessionId).GetSession(identifier);
            if (session == null)
            {
                return BufferStatus.NotInited;
            }
            else
            {
                return session.GetBuffer(serialNo, out buffer);
            }
        }

        public static void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            var sessionManagement = GetSessionManagement(sessionId);
            var session = sessionManagement.GetSession(identifier);
            if (session != null)
            {
                session.SetTimeout(isSender, timeout);
            }
        }

        public static bool UpdateModifyTime(string sessionId, string identifier, bool isSender)
        {
            var sessionManagement = GetSessionManagement(sessionId);
            var session = sessionManagement.GetSession(identifier);
            if (session != null)
            {
                session.UpdateModifyTime(isSender);
                return true;
            }

            return false;
        }

        public static int ClearBuffer(string sessionId, string identifier)
        {
            lock (mOpLock)
            {
                if (mSessionManagements.ContainsKey(sessionId))
                {
                    mSessionManagements[sessionId].ClearSession(identifier);
                }
            }
            return 0;
        }

        public static int ClearSessionManagement(string sessionId)
        {
            lock (mOpLock)
            {
                if (mSessionManagements.ContainsKey(sessionId))
                {
                    mSessionManagements.Remove(sessionId);
                }
            }
            return 0;
        }

        public static bool BufferSessionInUse(string sessionId, string identifier, bool isSender)
        {
            bool BufferSessionInUse = false;
            var sessionManagement = GetSessionManagement(sessionId);
            var session = sessionManagement.GetSession(identifier);
            if (session != null)
            {
                BufferSessionInUse = (!session.IsTimeout(isSender));
            }
            return BufferSessionInUse;
        }
    }

    /// <summary>
    /// 内存块的封装类，提供可管理的内存块最小单元定义
    /// </summary>
    internal class BufferWrapper
    {
        public long SerialNo;
        public byte[] Buffer;
    }

    internal class BufferWrapperSession : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(BufferWrapperSession));

        public string Session = string.Empty;
        public string Identify = string.Empty;
        public bool IsEnd = false;
        public DateTime LastInputTime = DateTime.MinValue;
        public DateTime LastOutputTime = DateTime.MinValue;
        public int InputTimeout = DataTransferConfiguration.DefaultReconnectTimeout;
        public int OutputTimeout = DataTransferConfiguration.DefaultReconnectTimeout;
        public LinkedList<BufferWrapper> Buffers = new LinkedList<BufferWrapper>();
        /// <summary>
        /// ADO-24538，cache最近一次接收者接收的数据，因为有些数据是接收过程中断了，但是Server端没有存储数据
        /// </summary>
        public BufferWrapper LastBufferWrapper = null;
        public long LastSerialNo = -1;

        private DataBufferPerformanceCounter performanceCounter;

        public BufferWrapperSession(string session, string identify)
        {
            this.Session = session;
            this.Identify = identify;
            this.performanceCounter = new DataBufferPerformanceCounter();
            this.performanceCounter.Init(DataTransferConfiguration.EnablePerformanceCounter, string.Format("{0}-{1}", session, identify));
            Reset();
        }

        public bool IsExpired()
        {
            bool isExpired = IsEnd;

            if (!isExpired)
            {
                if ((LastInputTime.AddMinutes(InputTimeout) < DateTime.UtcNow) && (LastOutputTime.AddMinutes(OutputTimeout) < DateTime.UtcNow))
                {
                    isExpired = true;
                }
            }

            return isExpired;
        }

        public bool IsAvailable()
        {
            bool isAvailable = false;

            if ((!IsEnd) && Buffers.Count == 0)
            {
                isAvailable = true;
            }

            return isAvailable;
        }

        public void Reset()
        {
            this.LastBufferWrapper = null;
            this.LastSerialNo = -1;
            this.IsEnd = false;
            this.LastInputTime = DateTime.UtcNow;
            this.LastOutputTime = DateTime.UtcNow;
            this.InputTimeout = DataTransferConfiguration.DefaultReconnectTimeout;
            this.OutputTimeout = DataTransferConfiguration.DefaultReconnectTimeout;
            this.Buffers.Clear();
        }

        public bool IsTimeout(bool isInput)
        {
            if (isInput)
            {
                if (LastInputTime.AddMinutes(InputTimeout) < DateTime.UtcNow)
                {
                    return true;
                }
            }
            else
            {
                if (LastOutputTime.AddMinutes(OutputTimeout) < DateTime.UtcNow)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 0---> ok
        /// 1---> No Buffer
        /// 2---> No Data from Sender
        /// 3---> Error 
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public BufferStatus GetBuffer(long serialNo, out byte[] buffer)
        {
            buffer = new byte[0];
            LastOutputTime = DateTime.UtcNow;
            this.performanceCounter.UpdateLastSentTime(LastOutputTime);
            BufferStatus bufferStatus = BufferStatus.NoBuffer;
            if (Buffers.Count == 0)
            {
                if (IsTimeout(true))
                {
                    bufferStatus = BufferStatus.ReadTimeout;
                }
                else if (LastBufferWrapper != null && LastBufferWrapper.SerialNo == -1 && serialNo == LastSerialNo + 1)
                {
                    bufferStatus = BufferStatus.NoDataFromSender;
                }
                else
                {
                    //session中没有缓冲区，返回1，客户端应该过一会在尝试取缓冲区
                    bufferStatus = BufferStatus.NoBuffer;
                }
            }
            else
            {
                List<BufferWrapper> removedBuffer = new List<BufferWrapper>();
                //多线程读取、写入Buffers链表时，可能会出现字节数组序列号在发送端、接收端Mismatch的问题，需要加锁
                lock (Buffers)
                {
                    while (Buffers.Count > 0
                        && Buffers.First.Value.SerialNo < serialNo
                        && Buffers.First.Value.SerialNo != -1)
                    {
                        removedBuffer.Add(Buffers.First.Value);
                        Buffers.RemoveFirst();
                    }
                    if (Buffers.Count == 0)
                    {
                        //session中没有缓冲区，返回1，客户端应该过一会在尝试取缓冲区
                        bufferStatus = BufferStatus.NoBuffer;
                    }
                    else if (Buffers.First.Value.SerialNo == -1)
                    {
                        LastBufferWrapper = Buffers.First.Value;
                        //发送端已经不再发送数据，返回2
                        removedBuffer.Add(Buffers.First.Value);
                        Buffers.RemoveFirst();//防止这个数据被第二个人使用，需要Remove掉
                        bufferStatus = BufferStatus.NoDataFromSender;
                    }
                    else if (Buffers.First.Value.SerialNo == serialNo)
                    {
                        LastBufferWrapper = Buffers.First.Value;
                        LastSerialNo = serialNo;
                        buffer = LastBufferWrapper.Buffer;
                        removedBuffer.Add(Buffers.First.Value);
                        Buffers.RemoveFirst();
                        //返回0表示成功取得一个缓冲区
                        bufferStatus = BufferStatus.OK;
                    }
                    else if (LastBufferWrapper != null && serialNo == LastBufferWrapper.SerialNo)
                    {
                        buffer = LastBufferWrapper.Buffer;
                        LastSerialNo = serialNo;
                        bufferStatus = BufferStatus.OK;
                    }
                    else
                    {
                        logger.Error("The request serial number:{0} does not match the first serial number:{1}", serialNo, Buffers.First.Value.SerialNo);
                        //缓冲区顺序状态出错了，不可恢复
                        bufferStatus = BufferStatus.BufferSerialNoError;
                    }
                }
                if (this.performanceCounter.IsEnabled)
                {
                    long removedSize = 0;
                    foreach (var buf in removedBuffer)
                    {
                        if (buf.Buffer != null && buf.Buffer.Length > 0)
                        {
                            removedSize += buf.Buffer.LongLength;
                        }
                    }
                    this.performanceCounter.UpdateCurrentDataSize(-removedSize);
                    this.performanceCounter.UpdateCurrentBufferCount(Buffers.Count);
                }
            }

            return bufferStatus;
        }

        /// <summary>
        /// 0--> OK
        /// 1--> Buffer is full.
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public BufferStatus PutBuffer(long serialNo, byte[] buffer)
        {
            LastInputTime = DateTime.UtcNow;
            this.performanceCounter.UpdateLastReceivedTime(LastInputTime);
            if (Buffers.Count >= DataTransferConfiguration.MaxCacheBuffer)
            {
                if (IsTimeout(false))
                {
                    return BufferStatus.WriteTimeout;
                }
                //缓冲区已满，返回1，客户端应该过一会再尝试重新放入
                return BufferStatus.BufferIsFull;
            }
            else
            {
                BufferWrapper bw = new BufferWrapper();
                bw.SerialNo = serialNo;
                bw.Buffer = buffer;
                //多线程读取、写入Buffers链表时，可能会出现字节数组序列号在发送端、接收端Mismatch的问题，需要加锁
                lock (Buffers)
                {
                    Buffers.AddLast(bw);
                }
                if (bw.Buffer != null)
                {
                    this.performanceCounter.UpdateCurrentDataSize(bw.Buffer.LongLength);
                }
                this.performanceCounter.UpdateCurrentBufferCount(Buffers.Count);
                //成功把缓冲区放到session里，返回0
                return BufferStatus.OK;
            }
        }

        /// <summary>
        /// 检查Buffer是否可以放入或者取出
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="isSender"></param>
        /// <returns></returns>
        public BufferStatus CheckBuffer(long serialNo, bool isSender)
        {
            if (isSender)
            {
                LastInputTime = DateTime.UtcNow;
                if (Buffers.Count >= DataTransferConfiguration.MaxCacheBuffer)
                {
                    if (IsTimeout(false))
                    {
                        return BufferStatus.WriteTimeout;
                    }
                    return BufferStatus.BufferIsFull;
                }
            }
            else
            {
                LastOutputTime = DateTime.UtcNow;
                if (Buffers.Count == 0)
                {
                    if (IsTimeout(true))
                    {
                        return BufferStatus.ReadTimeout;
                    }
                    //session中没有缓冲区，返回1，客户端应该过一会在尝试取缓冲区
                    return BufferStatus.NoBuffer;
                }
            }

            return BufferStatus.OK;
        }

        public void SetTimeout(bool isInput, int timeout)
        {
            if (timeout <= 0)
            {
                timeout = DataTransferConfiguration.DefaultReconnectTimeout;
            }
            if (isInput)
            {
                InputTimeout = timeout;
            }
            else
            {
                OutputTimeout = timeout;
            }
        }

        /// <summary>
        /// 更新input和output时间
        /// </summary>
        /// <param name="isInput"></param>
        public void UpdateModifyTime(bool isInput)
        {
            if (isInput)
            {
                LastInputTime = DateTime.UtcNow;
                this.performanceCounter.UpdateLastReceivedTime(LastInputTime);
            }
            else
            {
                LastOutputTime = DateTime.UtcNow;
                this.performanceCounter.UpdateLastSentTime(LastOutputTime);
            }
        }

        public void Dispose()
        {
            Reset();
            this.performanceCounter.Dispose();
        }
    }

    internal class BufferSessionManagement
    {
        public string SessionId = string.Empty;
        public Dictionary<string, BufferWrapperSession> Buffers = new Dictionary<string, BufferWrapperSession>(StringComparer.OrdinalIgnoreCase);

        public BufferSessionManagement(string sessionId)
        {
            this.SessionId = sessionId;
        }

        public bool InitSession(string identify)
        {
            lock (Buffers)
            {
                bool initResult = false;

                if (!Buffers.ContainsKey(identify))
                {
                    Buffers[identify] = new BufferWrapperSession(this.SessionId, identify);
                    initResult = true;
                }
                else
                {
                    var session = Buffers[identify];
                    if (session.IsExpired())
                    {
                        session.Reset();
                        initResult = true;
                    }
                    else
                    {
                        initResult = false;
                    }
                }

                return initResult;
            }
        }

        public BufferWrapperSession GetSession(string identify)
        {
            lock (Buffers)
            {
                BufferWrapperSession session = null;

                if (Buffers.ContainsKey(identify))
                {
                    session = Buffers[identify];
                }

                return session;
            }
        }

        public void ClearSession(string identifier)
        {
            lock (Buffers)
            {
                if (Buffers.ContainsKey(identifier))
                {
                    Buffers[identifier].Dispose();
                    Buffers.Remove(identifier);
                }
            }
        }
    }
}
