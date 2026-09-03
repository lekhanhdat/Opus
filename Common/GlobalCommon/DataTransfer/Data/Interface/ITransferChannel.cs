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
using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.Data.Interface
{
    /// <summary>
    /// 数据和传输介质之间的操作接口，用于定义传输层的处理方法
    /// 实现它的类用于实现数据最终发送到的介质的各种操作。
    /// 可以采用WCF，socket，SSL，文件系统等
    /// </summary>
    public interface ITransferChannel : IDisposable
    {
        /// <summary>
        /// 获得当前数据处理的各种状态信息
        /// </summary>
        DataTransferResultStatus CurrentWorkStatus
        {
            get;
        }
        /// <summary>
        /// 开启一个可用的处理对象，使其处于工作状态
        /// </summary>
        /// <param name="sessionId">当前数据报的分类号，同一批次的数举报sessionId应该一样</param>
        /// <param name="errorMessage">启动时候的错误消息</param>
        /// <param name="parameters">其他自定义参数
        /// parameters[0]:boolean，true是发送状态，false是接受状态
        /// parameters[1]:string，数据传输过程中的临时文件路径
        /// </param>
        /// <returns>是否启动成功</returns>
        bool Open(string sessionId, string identifier, string remoteIdentifier, DataTransferSetting settings, out string errorMessage);//, params object[] parameters);
        /// <summary>
        /// 主要用于检查缓冲区是否可用，并且初始化缓冲区。
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="isInited">是初始化还是等待</param>
        /// <returns></returns>
        SessionStatus InitSession(string sessionId, string identifier, bool isInited);
        /// <summary>
        /// 设置Timeout
        /// </summary>
        /// <param name="timeout"></param>
        void SetTimeout(string sessionId, string identifier, int timeout, bool isSender);
        /// <summary>
        /// 保持更新，更新BufferStorage里面的时间
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="isSender"></param>
        /// <returns></returns>
        bool KeepAlive(string sessionId, string identifier, bool isSender);
        /// <summary>
        /// 发送数据到对应的介质中
        /// </summary>
        /// <param name="buf">数据块</param>
        /// <param name="index">当前数据块读取的起始索引</param>
        /// <param name="len">需要发送的数据块的长度</param>
        /// <returns>当前操作是否成功的状态标志
        /// 返回1，客户端应该过一会再尝试重新放入
        /// 返回0,成功把缓冲区放到session里
        /// </returns>
        BufferStatus SendBinary(long serialNo, byte[] buf);
        /// <summary>
        /// 检查是否可以发送数据或者接收数据
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        BufferStatus CheckBinary(long serialNo, bool isSender);
        /// <summary>
        /// 从对应的介质中接受数据
        /// </summary>
        /// <param name="buf">接受的数举块</param>
        /// <param name="index">数据写入的起始位置</param>
        /// <param name="len">数据读取的长度</param>
        /// <returns>当前操作是否成功的状态标志
        /// 返回1，session中没有缓冲区
        /// 返回2,发送端已经不再发送数据
        /// 返回0,表示成功取得一个缓冲区
        /// 返回3，缓冲区顺序状态出错了，不可恢复
        /// </returns>
        BufferStatus ReceiveBinary(long serialNo, out byte[] buf);
        /// <summary>
        /// 关闭当前的处理对象的工作状态
        /// </summary>
        /// <returns>如果有错误返回错误值，否则是string.Empty</returns>
        string Close();
        /// <summary>
        /// 是否要关闭Session中的Buffer
        /// </summary>
        /// <param name="clearAll"></param>
        /// <returns></returns>
        string ClearBufferInSession(bool clearAll);

        bool BufferSessinInUse(bool isSender);
    }
}
