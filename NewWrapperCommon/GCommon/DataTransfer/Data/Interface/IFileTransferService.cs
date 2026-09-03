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
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Transfer.Data.Interface
{
    /// <summary>
    /// FileTransfer的服务契约定义
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IFileTransferService
    {
        /// <summary>
        /// 两个作用，1、确认服务可用，2、将传输文件名发送到服务段，让服务实例建立相应的filestream，
        /// 服务采用PerSession 实例模型。
        /// </summary>
        /// <param name="fileName">传输的文件名</param>
        /// <param name="overwriteFile">服务器短打开文件的操作模式：true：新建；false：打开</param>
        /// <param name="fileDir">文件存放目录</param>
        /// <returns>是否调用成功</returns>
        [OperationContract]
        Boolean CheckStatus(string fileName, string fileDir, Boolean overwriteFile);
        /// <summary>
        /// 从服务器端获得文件的内存块
        /// </summary>
        /// <param name="buffer">文件部分内容</param>
        /// <returns>是否成功
        /// 返回1，session中没有缓冲区
        /// 返回2,发送端已经不再发送数据
        /// 返回0,表示成功取得一个缓冲区
        /// 返回3，缓冲区顺序状态出错了，不可恢复
        /// </returns>
        [OperationContract]
        BufferStatus GetFileBuffer(out byte[] buffer);
        /// <summary>
        /// 将文件分块上传到服务器
        /// </summary>
        /// <param name="buffer">文件内容</param>
        /// <returns>
        /// 返回1，客户端应该过一会再尝试重新放入
        /// 返回0,成功把缓冲区放到session里
        /// </returns>
        [OperationContract]
        BufferStatus WriteFileBuffer(byte[] buffer);
    }

}
