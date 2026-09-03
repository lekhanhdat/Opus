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
using System.Runtime.Serialization;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Contract.Common;
using System.IO;

namespace AvePoint.GCommon.Transfer.Data.Interface
{
    /// <summary>
    /// 底层数据传输的处理WCF服务接口
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IStreamRelay
    {
        /// <summary>
        /// 初始化Session或者Wait
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="isInited"></param>
        /// <returns></returns>
        [OperationContract]
        SessionStatus InitSession(string sessionId, string identifier, bool isInited);

        [OperationContract]
        int CheckStatus(string sessionId, string identifier);

        /// <summary></summary>
        /// set stream object and used for job
        /// </summary
        /// <returns></returns>
        [OperationContract]
        void PutTransferStream(HttpModeServiceStream stream);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginPutTransferStream(HttpModeServiceStream stream, AsyncCallback callBack, object asyncState);

        void EndPutTransferStream(IAsyncResult result);

        [OperationContract]
        HttpModeServiceStream GetTransferStream(HttpModeDownLoadStream downLoadInfo);

        [OperationContract(AsyncPattern=true)]
        IAsyncResult BeginGetTransferStream(HttpModeDownLoadStream downLoadInfo, AsyncCallback callBack, object asyncState);

        HttpModeServiceStream EndGetTransferStream(IAsyncResult result);

        [OperationContract]
        SessionStatus OpenConnection(string sessionId, string identifier, bool isInited);

        [OperationContract]
        ReconnectionInfo ReopenConnection(string sessionId, string identifier, ReconnectionInfo reconnectionInfo);

        [OperationContract]
        BufferStatus CheckPeerFinishStatus(string sessionId, string identifier, bool isSender);
        [OperationContract]
        void ResetReconnectionStatus(string sessionId, bool waitClose);

        [OperationContract]
        bool CheckStreamInUse(string sessionId, string identifier, bool isSender);

        /// <summary>
        /// 设置Session的Timeout时间
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="timeout"></param>
        /// <param name="isSender"></param>
        [OperationContract]
        void SetTimeout(string sessionId, string identifier, int timeout, bool isSender);

        /// <summary>
        /// 保持更新
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="isSender"></param>
        /// <returns></returns>
        [OperationContract]
        bool KeepAlive(string sessionId, string identifier, bool isSender);

        /// <summary>
        /// 清空服务端当前Session的数据队列
        /// </summary>
        /// <param name="sessionId">数据报的Session</param>
        /// <returns></returns>
        [OperationContract]
        int ClearSession(string sessionId, string identifier);
        /// <summary>
        /// 清空服务端当前Session的数据队列
        /// </summary>
        /// <param name="sessionId">数据报的Session</param>
        /// <returns></returns>
        [OperationContract]
        int ClearSessionManagement(string sessionId);

        [OperationContract]
        StreamHader NextTransferDataSize(string sessionId, bool isSender);

        [OperationContract]
        void TransferFinish(string sessionId);
    }

    [MessageContract]
    public class HttpModeServiceStream : IDisposable
    {
        private AveLogger mLog = AveLogger.GetInstance(typeof(HttpModeServiceStream));
        [MessageHeader(MustUnderstand = true)]
        public string SessionId;

        [MessageHeader(MustUnderstand = true)]
        public string SubSessionId;

        [MessageBodyMember]
        public Stream HttpStream;

        public void Dispose()
        {
            if (HttpStream != null)
            {
                try
                {
                    HttpStream.Dispose();
                    HttpStream = null;
                }
                catch (Exception e)
                {
                    mLog.Warn("Dispose stream exception:{0}", e.ToString());
                }
            }
        }
    }

    [MessageContract]
    public class HttpModeDownLoadStream
    {
        [MessageHeader(MustUnderstand = true)]
        public string SessionId;

        [MessageHeader(MustUnderstand = true)]
        public string SubSessionId;

        [MessageHeader(MustUnderstand = true)]
        public int DownloadStreamLength;
    }

    [DataContract]
    public class StreamHader
    {
        [DataMember]
        public bool Finish { get; set; }

        [DataMember]
        public int Length { get; set; }
    }
}
