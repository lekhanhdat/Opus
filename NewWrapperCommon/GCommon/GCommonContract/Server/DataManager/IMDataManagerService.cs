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
using System.Linq;
using System.Text;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;
using System.IO;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.AveLicense;

namespace AvePoint.GCommon.Contract.Server.DataManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMDataManagerService
    {
        /// <summary>
        /// 根据功能模块load相应的storage policy
        /// </summary>
        /// <param name="ModuleType"></param>
        /// <returns></returns>
        [OperationContract]
        List<StoragePolicyDto> GetStoragePolicyByModule(int moduleType, ProductVersion productVersion, ProductType productType);

        /// <summary>
        /// 修改retention 时间
        /// </summary>
        /// <param name="dataManagerContactDto"></param>
        /// <returns></returns>
        [OperationContract]
        DataManagerExcuteResult UpdateRetention(DataManagerContactDto dataManagerContactDto);
        
        /// <summary>
        /// 修改压缩时间
        /// </summary>
        /// <param name="dataManagerContactDto"></param>
        /// <returns></returns>
        [OperationContract]
        DataManagerExcuteResult UpdateCompression(DataManagerContactDto dataManagerContactDto);

        /// <summary>
        /// 修改data 属性
        /// </summary>
        /// <param name="dataManagerContactDto"></param>
        /// <returns></returns>
        [OperationContract]
        DataManagerExcuteResult UpdateMetaData(DataManagerContactDto dataManagerContactDto);

        /// <summary>
        /// 获取device上的压缩信息
        /// </summary>
        /// <param name="dataManagerContactDto"></param>
        /// <returns></returns>
        [OperationContract]
        List<DataManagerTreeNodeDto> GetInfoFormDevice(DataManagerContactDto dataManagerContactDto);
        
        /// <summary>
        /// hold住相应的cycle
        /// </summary>
        /// <param name="dataManagerContactDto"></param>
        /// <returns></returns>
        [OperationContract]
        DataManagerExcuteResult DoHold(DataManagerContactDto dataManagerContactDto);

        /// <summary>
        /// delete 
        /// </summary>
        /// <param name="dataManagerContactDto"></param>
        /// <returns></returns>
        [OperationContract]
        DataManagerExcuteResult DoDeletion(DataManagerContactDto dataManagerContactDto);
    }
}
