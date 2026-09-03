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
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.StorageOptimization
{
    /// <summary>
    /// 此接口为Storage Manager获取指定Device中的BLOB数据大小提供了方法
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IACalculateBlobSizeService
    {
        /// <summary>
        /// 计算指定Physical Device下的所有BLOB的大小
        /// </summary>
        /// <param name="physicalDevices">指定Physical Device集合</param>
        /// <returns>BLOB size单位是byte</returns>
        [OperationContract]
        long CalculateBlobSizeinDevice(Guid[] physicalDevices);
        /// <summary>
        /// 计算指定Physical Device下的指定某种类型的BLOB的大小
        /// </summary>
        /// <param name="physicalDevices">指定Physical Device集合</param>
        /// <param name="moduleType">1(001) --- RealTime Extender, 2(010) --- Schedule Extender, 4(100) --- Connector, 7(111) --- All</param>
        /// <param name="providerType">1(01) --- EBS, 2(10) --- RBS, 3(11) --- All</param>
        /// <returns>BLOB size单位是byte</returns>
        [OperationContract]
        long CalculateBlobSizeinDevice(Guid[] physicalDevices, int moduleType, int providerType);
    }
}
