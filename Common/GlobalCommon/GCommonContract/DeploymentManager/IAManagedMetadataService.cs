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



using System.ServiceModel;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.DeploymentManager.Message;

namespace AvePoint.GCommon.Contract.DeploymentManager
{
    /// <summary>
    /// Agent提供给APVCserviceHost的一个服务接口，用于接受Server或Agent的请求消息
    /// </summary>
    [ServiceContract]
    public interface IAManagedMetadataService
    {
        /// <summary>
        /// 用于处理Manager发送的请求，并返回处理结果
        /// </summary>
        /// <param name="message">Manager发送的请求消息对象</param>
        [OperationContract]
        ReturnResult HandleMessageFromServer(MMSMessage message);

        /// <summary>
        /// 用于处理Agent之间发送的请求，并返回结果，当前的方法目前为了启动目的端进程
        /// </summary>
        /// <param name="message">启动进程需要的请求消息</param>
        /// <returns>操作的结果返回值</returns>
        [OperationContract]
        ReturnResult HandleMessageFromPrimaryAgent(MMSMessage message);

        /// <summary>
        /// 用于处理Manager发送的Stop/Pause/Resume请求，并返回处理结果
        /// </summary>
        /// <param name="message">Manager发送的请求消息对象</param>
        /// <returns>操作的结果返回值</returns>
        [OperationContract]
        ReturnResult HandleMessageFromServerForMonitorJob(MMSMessage message);

        /// <summary>
        /// 用于处理Agent发送的Stop/Pause/Resume请求，并返回处理结果
        /// </summary>
        /// <param name="message">Agent发送的请求消息对象</param>
        /// <returns>操作的结果返回值</returns>
        [OperationContract]
        ReturnResult HandleMessageFromAgentForMonitorJob(MMSMessage message);
    }
}
