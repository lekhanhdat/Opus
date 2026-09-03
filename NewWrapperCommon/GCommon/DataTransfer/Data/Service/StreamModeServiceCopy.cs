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
using AvePoint.GCommon.Transfer.Data.HttpMode;

namespace AvePoint.GCommon.Transfer.Data.Service
{
    /// <summary>
    /// 实现数据传输的底层WCF服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class StreamModeService : IStreamRelay, IDisposable
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(StreamModeService), false);

        private CommonPerformanceTimerPool timerPool = new CommonPerformanceTimerPool(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DisablePerformanceLogger);

        public int CheckStatus(string sessionId, string identifier)
        {
            return 0;
        }
        public SessionStatus InitSession(string sessionId, string identifier, bool isInited)
        {
            if (isInited)
            {
                if (GlobalBufferSessionUtilityCache.InitGlobalBufferSessionWithId(sessionId))
                {
                    return FileCycleStreamCacheUtility.InitCycleSessionStream(sessionId);
                }
                return SessionStatus.IsInUse;
            }
            else
            {
                if (GlobalBufferSessionUtilityCache.CheckGlobalBufferSessionIdExist(sessionId))
                {
                    return FileCycleStreamCacheUtility.CheckCycleSessionStreamExist(sessionId);
                }
                return SessionStatus.NonExist;
            }
        }

        public void Dispose()
        {
            //if (!DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DisablePerformanceLogger)
            //{
            //    logger.Debug(timerPool.ToString());
            //}
        }

        public void PutTransferStream(HttpModeServiceStream stream)
        {
            FileCycleStream fileStream = FileCycleStreamCacheUtility.GetFileCycleStream(stream.SessionId);
            BufferSessionUtility bufferUtility = GlobalBufferSessionUtilityCache.AddOrUpdateBufferUtility(stream.SessionId, stream.HttpStream, fileStream, stream.SubSessionId, true);
            if (bufferUtility != null)
            {
                bufferUtility.WaitUntilProcessFinish();
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
            FileCycleStream fileStream = FileCycleStreamCacheUtility.GetFileCycleStream(downLoadInfo.SessionId);
            var returnStream = new HttpModeServiceStream();
            returnStream.SessionId = downLoadInfo.SessionId;
            returnStream.HttpStream = new HttpModeStream();
            BufferSessionUtility bufferUtility = GlobalBufferSessionUtilityCache.AddOrUpdateBufferUtility(downLoadInfo.SessionId, returnStream.HttpStream, fileStream, downLoadInfo.SubSessionId, false, downLoadInfo.DownloadStreamLength);
            return returnStream;
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
            if (isInited)
            {
                return BufferStorage.InitSessionManagement(sessionId, identifier);
            }
            else
            {
                return BufferStorage.IsSessionManagementExisting(sessionId, identifier);
            }
        }

        public ReconnectionInfo ReopenConnection(string sessionId, string identifier, ReconnectionInfo reconnectionInfo)
        {
            if (identifier.Equals(DataTransferConstants.SenderIdentifier))
            {
                BufferSessionUtility utility = GlobalBufferSessionUtilityCache.GetBufferUtility(sessionId, true);
                if (utility != null)
                {
                    if (utility.StopTransfer())
                    {
                        int serialNumber = utility.PutSerialNumber;
                        if (serialNumber == -1)
                        {
                            serialNumber = 0;
                        }
                        reconnectionInfo = new ReconnectionInfo() { SerialNum = serialNumber, Status = SessionStatus.IsReady };
                    }
                    else
                    {
                        reconnectionInfo = new ReconnectionInfo() { SerialNum = 0, Status = SessionStatus.IsInUse };
                    }
                    return reconnectionInfo;
                }
                else
                {
                    reconnectionInfo = new ReconnectionInfo() { SerialNum = 0, Status = SessionStatus.IsReady };
                }
                return reconnectionInfo;
            }
            else
            {
                BufferSessionUtility utility = GlobalBufferSessionUtilityCache.GetBufferUtility(sessionId, false);
                if (utility != null)
                {
                    if (utility.StopTransfer())
                    {
                        utility.ResetQueueData(reconnectionInfo.SerialNum);
                        reconnectionInfo = new ReconnectionInfo() { SerialNum = 0, Status = SessionStatus.IsReady };
                    }
                    else
                    {
                        reconnectionInfo = new ReconnectionInfo() { SerialNum = 0, Status = SessionStatus.IsInUse };
                    }
                    return reconnectionInfo;
                }
                else
                {
                    reconnectionInfo = new ReconnectionInfo() { SerialNum = 0, Status = SessionStatus.IsReady };
                }
                return reconnectionInfo;
            }

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
            return FileCycleStreamCacheUtility.CheckCycleSessionStreamExist(sessionId) != SessionStatus.NonExist;
        }

        public void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            BufferStorage.SetTimeout(sessionId, identifier, timeout, isSender);
        }

        public bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            return BufferStorage.UpdateModifyTime(sessionId, identifier, isSender);
        }

        public int ClearSession(string sessionId, string identifier)
        {
            GlobalBufferSessionUtilityCache.ClearSession(sessionId);
            MutiThransferUtilityCache.Clear(sessionId);
            return FileCycleStreamCacheUtility.ClearFileCycleStringSession(sessionId);
        }
        public int ClearSessionManagement(string sessionId)
        {
            GlobalBufferSessionUtilityCache.ClearSession(sessionId);
            MutiThransferUtilityCache.Clear(sessionId);
            return FileCycleStreamCacheUtility.ClearFileCycleStringSession(sessionId);
        }

        public StreamHader NextTransferDataSize(string sessionId, bool isSender)
        {
            FileCycleStream fileStream = FileCycleStreamCacheUtility.GetFileCycleStream(sessionId);
            if (isSender)
            {
                return new StreamHader() { Finish = false, Length = fileStream.CanWriteBuffer };
            }
            else
            {
                return new StreamHader() { Finish = (fileStream.IsWriteFinish && fileStream.CanReadBuffer == 0), Length = fileStream.CanReadBuffer };
            }
        }

        public void TransferFinish(string sessionId)
        {
            FileCycleStream fileStream = FileCycleStreamCacheUtility.GetFileCycleStream(sessionId);
            fileStream.FinishWrite();
        }
    }
}
