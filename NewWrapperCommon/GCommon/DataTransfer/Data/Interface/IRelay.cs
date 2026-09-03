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
    public interface IRelay
    {
        /// <summary>
        /// 用于测试服务可用状态，并在服务端注册当前的服务实例到全局队列
        /// </summary>
        /// <returns>是否正常</returns>
        [OperationContract]
        int CheckStatus(string sessionId, string identifier);
        /// <summary>
        /// 初始化Session或者Wait
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="isInited"></param>
        /// <returns></returns>
        [OperationContract]
        SessionStatus InitSession(string sessionId, string identifier, bool isInited);
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
        /// <summary>
        /// 将buffer数组上传到服务端
        /// </summary>
        /// <param name="sessionId">当前数据块的分类session</param>
        /// <param name="serialNo">序列号</param>
        /// <param name="buffer">数据块</param>
        /// <returns></returns>
        [OperationContract]
        BufferStatus PutBuffer(string sessionId, string identifier, long serialNo, byte[] buffer);
        /// <summary>
        /// 检查Buffer是否还可以继续发送或者接收
        /// </summary>
        /// <param name="sessionid"></param>
        /// <param name="identifier"></param>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        [OperationContract]
        BufferStatus CheckBuffer(string sessionId, string identifier, long serialNo, bool isSender);
        /// <summary>
        /// 从服务端获取数据块。
        /// </summary>
        /// <param name="sessionId">指定Session的ID</param>
        /// <param name="serialNo">序列号</param>
        /// <param name="buffer">获得数据块</param>
        /// <returns></returns>
        [OperationContract]
        BufferStatus GetBuffer(string sessionId, string identifier, long serialNo, out byte[] buffer);
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
        ///  sender结束之后需要等待reciever的结束状态，添加check方法用于检测目的端receiver是否结束接收
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="isSender"></param>
        /// <returns></returns>
        [OperationContract]
        bool CheckSessionInUse(string sessionId, string identifier, bool isSender);
        
    }

}
