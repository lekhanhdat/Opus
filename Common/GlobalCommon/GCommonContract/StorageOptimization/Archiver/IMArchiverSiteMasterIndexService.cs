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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Archiver
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMArchiverSiteMasterIndexService
    {
        [OperationContract]
        string InsertIntoArchiverSiteMasterIndex(ArchiverSiteMasterIndexContract indexDto);

        /// <summary>
        /// 更新Merge Index State, siteInfo中要包含site collection信息和Farm信息
        /// </summary>
        [OperationContract]
        void UpdateMergeIndexState(string jobId, ArchiverSiteInfoDto siteInfo, MergeIndexState mergeIndexState);

        /// <summary>
        /// retention后通知manager去修改sub master表或者delete job record
        /// </summary>
        /// <param name="retentionInfo"></param>
        [OperationContract]
        void NotificationRetentionState(ArchiverPruningJob retentionInfo);


        #region   Method for control panel and other services.
        /// <summary>
        /// 通过Physical Device Id获取siteInfo
        /// </summary>
        [OperationContract]
        List<ArchiverSiteMasterIndexContract> getSiteInfoByPhysicalId(string physicalDeviceId);

        /// <summary>
        /// 根据JobId获取siteInfo,此方法暂时只提供给Compliance使用
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [OperationContract]
        List<ArchiverSiteMasterIndexContract> GetSiteInfoByJob(string jobId);
        #endregion

        #region Method for archiver retention meta data
        /// <summary>
        /// 取得所有Farm的信息，以Tree形势返回
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        SOMessage GetRetentionMetaData(int[] deviceTypes);

        /// <summary>
        /// 更新有修改的RetentionTime
        /// </summary>
        /// <param name="jobs"></param>
        [OperationContract]
        void UpdateDellRetentionTimeSpanSeconds(Dictionary<Int64, List<string>> jobs);


        /// <summary>
        /// 切换crawl profile的后,要进行删除旧的crawl index,成功删除后media进行更新crawl信息.
        /// </summary>
        /// <param name="fullTextIndexRequest"></param>
        [OperationContract]
        void UpdateCrawlIndexInfo(FullTextIndexSearchRequest fullTextIndexRequest);
        #endregion

        #region Method for End user view
        [OperationContract]
        Dictionary<string, Int64> GetRetentionTimeByJobId(string subJobId);
        #endregion
    }
}
