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
using AvePoint.GCommon.Contract.Media.Object;

namespace AvePoint.GCommon.Contract.Media
{
    /// <summary>
    /// 接收Media service的load balance信息，并保存到Control数据库
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMAcceptMediaData
    {
        /// <summary> Control端负责接收Media发送过来的数据，保存成功返回1，否则返回0 </summary>
        /// <param name="dto"></param>
        [OperationContract]
        int AcceptLoadBalanceInfo(MediaDataDto dto);

        /// <summary> 根据serviceId、key获取MediaData记录。</summary>
        /// <param name="serviceId"></param>
        /// <param name="key">不能为empty or null</param>
        [OperationContract]
        List<MediaDataDto> GetMediaDatas(string serviceId, string key);

        /// <summary> 根据serviceId、key删除MediaData记录。 </summary>
        /// <param name="serviceId"></param>
        /// <param name="key">不能为empty or null</param>
        [OperationContract]
        void DeleteMediaDatas(string serviceId, string key);

        /// <summary>根据serviceId、key更新MediaData记录，如果不存在MediaData记录，则创建一条MediaData记录。</summary>
        /// <param name="serviceId"></param>
        /// <param name="key">不能为empty or null</param>
        /// <param name="value"></param>
        [OperationContract]
        void UpdateOrInsertMediaData(string serviceId, string key, string value);

        [OperationContract]
        void InitiateMediaDataService(string groupId);
    }
}
