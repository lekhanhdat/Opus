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
using System.ServiceModel;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Common;
using System.ServiceModel.Activation;
using AvePoint.GCommon.Transfer.HttpMode.Common;

namespace AvePoint.GCommon.Transfer.Data.Service
{
    /// <summary>
    /// 实现数据传输的底层WCF服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class StreamModeService_ : IStreamRelay, IDisposable
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(StreamModeService_), false);

        private CommonPerformanceTimerPool timerPool = new CommonPerformanceTimerPool(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DisablePerformanceLogger);

        public int CheckStatus(string sessionId, string identifier)
        {
            return 0;
        }
        public SessionStatus InitSession(string sessionId, string identifier, bool isInited)
        {
            if (isInited)
            {
                return BufferStorage.InitSessionManagement(sessionId, identifier);
            }
            else
            {
                return BufferStorage.IsSessionManagementExisting(sessionId, identifier);
            }
        }

        public void Dispose()
        {
            //if (!DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DisablePerformanceLogger)
            //{
            //    logger.Debug(timerPool.ToString());
            //}
        }

        public StreamHader NextTransferDataSize(string sessionId, bool isSender)
        {
            return new StreamHader() { Finish = false, Length = 0 };
        }

        public void PutTransferStream(HttpModeServiceStream stream)
        {
            HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(stream.SessionId);
            if (peer != null)
            {
                peer.ReadDataFromStream(stream.HttpStream);
            }
            else
            {
                DataTransferLogger.Logger(AveLogLevel.WARN, "cannot find peer by session id:{0}", stream.SessionId);
                throw new Exception(string.Format("Cannot find peer according to session id:{0}", stream.SessionId));
            }
        }

        public IAsyncResult BeginPutTransferStream(HttpModeServiceStream stream, AsyncCallback callBack, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndPutTransferStream(IAsyncResult result)
        {
            throw new NotImplementedException();
        }


        public HttpModeServiceStream GetTransferStream(HttpModeDownLoadStream downLoadInfo)
        {
            HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(downLoadInfo.SessionId);
            if (peer != null)
            {
                DataTransferLogger.Logger(AveLogLevel.INFO, "start to download stream with session id:{0}", downLoadInfo.SessionId);
                var returnStream = new HttpModeServiceStream();
                returnStream.SessionId = downLoadInfo.SessionId;
                returnStream.HttpStream = peer.CacheStream;
                return returnStream;
            }
            else
            {
                DataTransferLogger.Logger(AveLogLevel.WARN, "cannot find transfer stream according to id:{0}", downLoadInfo.SessionId);
                throw new Exception(string.Format("Cannot find transfer stream according to id:{0}", downLoadInfo.SessionId));
            }
        }

        public IAsyncResult BeginGetTransferStream(HttpModeDownLoadStream downLoadInfo, AsyncCallback callBack, object asyncState)
        {
            throw new NotImplementedException();
        }

        public HttpModeServiceStream EndGetTransferStream(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 三次握手过程，并且会清除状态
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public SessionStatus OpenConnection(string sessionId, string identifier, bool isInited)
        {
            var peer = HttpModePeerCache.EnsureHttpModePee(sessionId);

            var clearStatus = false;
            var sessionStatus = SessionStatus.InitedOK;

            if (identifier.Equals(DataTransferConstants.SenderIdentifier))
            {
                if (peer.MatchReceiver)
                {
                    sessionStatus = SessionStatus.IsReady;

                    if(peer.CacheStream != null && (peer.CacheStream.IsFinishWrite || peer.CacheStream.IsReadFinish || peer.CacheStream.IsStopped))
                    {
                        DataTransferLogger.Logger(AveLogLevel.INFO, "start to reset cache stream in session:{0} with identifier:{1}", sessionId, identifier);
                        peer.CacheStream.Reset();
                    }

                    peer.MatchSender = true;
                }
            }
            else
            {
                var senderStatus = peer.MatchSender;

                peer.MatchReceiver = true;

                if (senderStatus)
                {
                    clearStatus = true;
                    sessionStatus = SessionStatus.IsReady;
                }
            }

            if(clearStatus)
            {
                peer.MatchSender = false;
                peer.MatchReceiver = false;
            }

            return sessionStatus;
        }

        public ReconnectionInfo ReopenConnection(string sessionId, string identifier, ReconnectionInfo reconnectionInfo)
        {
            HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(sessionId);
            if (peer == null)
            {
                reconnectionInfo.ErrorMessage = "Can not find peer, logic error";
                return reconnectionInfo;
                //logic
            }

            var clearStatus = false;
            var sessionStatus = SessionStatus.InitedOK;

            if (identifier.Equals(DataTransferConstants.SenderIdentifier))
            {
                //sender需要等目的端主动抛异常，所以不能破坏download的stream                
                if (peer.MatchReconnectionReceiver)
                {
                    DataTransferLogger.Logger(AveLogLevel.INFO, "start to reset cache stream in session:{0} with identifier:{1}, and get serial number:{2}", sessionId, identifier, peer.SerialNumberForReconnection);
                    peer.CacheStream.Reset();
                    reconnectionInfo.SerialNum = (int)peer.SerialNumberForReconnection;
                    sessionStatus = SessionStatus.IsReady;
                    peer.MatchReconnectionSender = true;
                }
                else
                {
                    //如果sender来重练发现目的端没有进入重练，则需要主动出发stop方法。
                    if (!peer.CacheStream.IsStopped)
                    {
                        var currentTime = DateTime.Now;
                        DataTransferLogger.Logger(AveLogLevel.INFO, "start to stop write data in session:{0} with identifier:{1}, current time:{2}", sessionId, identifier, currentTime);
                        peer.StopWriteData(string.Format("sender need to reopen connection in session:{0} with identifier:{1}, current time:{2}", sessionId, identifier, currentTime));
                    }
                }
            }
            else
            {
                var senderMatch = peer.MatchReconnectionSender;
                peer.SerialNumberForReconnection = reconnectionInfo.SerialNum;
                peer.MatchReconnectionReceiver = true;
                //receiver需要及时stop，否则写入太多会导致data的sn匹配失败。

                if (senderMatch)
                {
                    DataTransferLogger.Logger(AveLogLevel.INFO, "reconnection is successful in session:{0}, current identifier:{1}, last serial number:{2}", sessionId, identifier, reconnectionInfo.SerialNum);
                    clearStatus = true;
                    sessionStatus = SessionStatus.IsReady;
                }
                else
                {
                    if (!peer.CacheStream.IsStopped)
                    {
                        var currentTime = DateTime.Now;
                        DataTransferLogger.Logger(AveLogLevel.INFO, "start to stop write data in session:{0} with identifier:{1}, current time:{2}", sessionId, identifier, currentTime);
                        peer.StopWriteData(string.Format("receiver need to reopen connection with serial number:{0} in session:{1} with identifier:{2}, current time:{3}", reconnectionInfo.SerialNum, sessionId, identifier, currentTime));
                    }
                }
            }

            reconnectionInfo.Status = sessionStatus;

            if(clearStatus)
            {
                peer.MatchReconnectionSender = false;
                peer.MatchReconnectionReceiver = false;
            }

            return reconnectionInfo;
        }

        /// <summary>
        /// 检查两端的状态
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="isSender"></param>
        /// <returns></returns>
        public BufferStatus CheckPeerFinishStatus(string sessionId, string identifier, bool isSender)
        {
            HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(sessionId);
            if (peer != null)
            {
                if (peer.CacheStream != null)
                {
                    if (isSender)
                    {
                        if (peer.CacheStream.IsFinishWrite)
                        {
                            return BufferStatus.OK;
                        }
                        else if (peer.CacheStream.IsWriteTimeout())
                        {
                            return BufferStatus.WriteTimeout;
                        }
                        else if (peer.CacheStream.IsStopped)
                        {
                            throw new Exception(peer.CacheStream.StopMessage);
                        }
                    }
                    else
                    {
                        if (peer.CacheStream.IsReadFinish)
                        {
                            return BufferStatus.OK;
                        }
                        else if (peer.CacheStream.IsReadTimeout())
                        {
                            return BufferStatus.ReadTimeout;
                        }
                        else if (peer.CacheStream.IsStopped)
                        {
                            throw new Exception(peer.CacheStream.StopMessage);
                        }
                    }
                }
                else
                {
                    return BufferStatus.NoBuffer;
                }
            }
            
            return BufferStatus.NotInited;
        }

        public void ResetReconnectionStatus(string sessionId, bool waitClose)
        {
            //HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(sessionId);
            //peer.ClearUsedStream(waitClose);
        }

        public bool CheckStreamInUse(string sessionId, string identifier, bool isSender)
        {
            HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(sessionId);
            if (peer != null && peer.CacheStream != null)
            {
                if (isSender)
                {
                    if (peer.CacheStream.IsFinishWrite || peer.CacheStream.IsWriteTimeout())
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (peer.CacheStream.IsReadFinish || peer.CacheStream.IsReadTimeout())
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }


        public void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(sessionId);
            lock (peer)
            {
                if (isSender)
                {
                    peer.CacheStream.WriteTimeout = timeout;
                }
                else
                {
                    peer.CacheStream.ReadTimeout = timeout;
                }
            }
        }

        public bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            HttpModePeer peer = HttpModePeerCache.QueryHttpModePeer(sessionId);
            if (isSender)
            {
                peer.CacheStream.UpdateLastWriteTime();
            }
            else
            {
                peer.CacheStream.UpdateLastReadTime();
            }

            return true;
        }

        public int ClearSession(string sessionId, string identifier)
        {
            return 0;
        }
        public int ClearSessionManagement(string sessionId)
        {
            if(HttpModePeerCache.RemovePeer(sessionId))
            {
                return 0;
            }
            return 1;
        }

        public void TransferFinish(string sessionId)
        { }
    }
}
