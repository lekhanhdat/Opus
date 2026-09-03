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
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Archiver
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMEndUserArchiverService
    {
        /// <summary>
        /// For client,end user触发archiver时调用此方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        EndUserArchiverMessage StartEndUserArchiverJob(RelativeDataArchiverContract request);

        /// <summary>
        /// 获得control端程序集的全路径
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        String GetCommonContractVersionString();

        /// <summary>
        /// 根据client传过来的int值，判断需要返回client的信息，返回给client
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="viewOption"></param>
        /// <returns></returns>
        [OperationContract]
        EndUserViewInfo GetEndUserViewInfo(RelativeDataArchiverContract contract);

        /// <summary>
        /// For client,end user触发restore时调用此方法
        /// </summary>
        /// <param name="restoreTreeNode"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        EndUserArchiverMessage StartEndUserRestoreJob(EndUserArchiverViewNodeDto restoreTreeNode, RelativeDataArchiverContract request);

        /// <summary>
        /// For client，在起end user archiver job时调用此方法
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        [OperationContract]
        List<TagInfo> GetAllTagMappingInfos(RelativeDataArchiverContract contract);
        
        /// <summary>
        /// 根据参数中的farm,web,site信息,获取end user archiver search需要的数据.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        [OperationContract]
        EndUserArchiverCrawlContract GetEndUserArchiverCrawlInfo(RelativeDataArchiverContract contract);
    }
}
