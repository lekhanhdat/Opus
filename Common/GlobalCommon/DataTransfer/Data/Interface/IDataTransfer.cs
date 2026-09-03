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
    /// 数据传输Agent接口的基接口，提供基本的公用方法定义
    /// </summary>
    public interface IDataTransfer
    {
        /// <summary>
        /// 启动数据传输对象工作
        /// </summary>
        /// <param name="settings">启动传输层的配置参数，从配置文件或者GUI获得信息</param>
        /// <param name="sessionId">当前传输的数据分类标示</param>
        /// <rreturns>是否启动成功</rreturns>
        Boolean Open(DataTransferSetting setting, string sessionId);
        /// <summary>
        /// 结束数据传输对象工作
        /// </summary>
        /// <returns>如果有错误提示错误信息，否则string.Empty</returns>
        string Close();
        /// <summary>
        /// 获得当前传输层的各种状态信息,包括：
        /// 已传数据传输的大小；传输层工作工作状态
        /// </summary>
        /// <returns>返回状态集合类</returns>
        DataTransferResultStatus DataTransferStatus
        {
            get;
        }
        /// <summary>
        /// 有些情况，需要强制退出程序，所以需要有一个stop的方法，避免hang的问题。
        /// </summary>
        /// <param name="message"></param>
        void Stop(string message);
    }

    public delegate void CodeToRunReconnected();

    /// <summary>
    /// 没有变量的委托
    /// </summary>
    public delegate void DataTransferCommonDelegate();

    /// <summary>
    /// 变量的委托
    /// </summary>
    /// <param name="obj"></param>
    public delegate void DataTransferCommonParameterizedDelegate(object obj);
}
