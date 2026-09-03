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



using System.Collections.Generic;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.LogManager.Object;


namespace AvePoint.GCommon.Contract.Server.ControlPanel.LogManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMLogManagerService
    {
        /// <summary>
        /// 开始执行job
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="notificationDto"></param>
        /// <returns>job的id</returns>
        [OperationContract]
        string RunNow(List<string> serviceId, NotificationDto notificationDto);
        /// <summary>
        /// 根据serviceIds生成logManager的一个job
        /// 会在job完成后才会返回结果
        /// </summary>
        /// <param name="services">需要收集的service id</param>
        /// <returns>logManager job id</returns>
        [OperationContract]
        string GetLogJobIdByService(List<string> services);
    }
}
