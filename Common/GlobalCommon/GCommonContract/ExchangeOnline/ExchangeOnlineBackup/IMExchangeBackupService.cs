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



namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup
{
    #region  == using dirictives ==
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Media.Object.Exchange;
    #endregion

    public interface IMExchangeBackupService
    {
        /// <summary> 用来创建SiteMasterIndex(主记录).</summary>
        /// <param name="siteMasterIndex"></param>
        /// <param name="subInfo"></param>
        EOIndexOperationResult CreateSiteMasterIndexInfo(EOSiteMasterIndexDto siteMasterIndex);

        /// <summary> 用来创建SiteMasterIndexSubDto(子记录). </summary>
        /// <param name="subInfo"></param>
        void CreateSiteMasterIndexSubInfo(EOSiteMasterIndexSubDto subInfo);

        /// <summary> 更新SiteMasterIndexSubDto(子记录)记录. </summary>
        /// <param name="siteMasterIndex"></param>
        void UpdateSiteMasterIndexSubInfo(EOSiteMasterIndexSubDto siteMasterIndex);

        /// <summary>删除SiteMasterIndex(主记录)或者SiteMasterIndexSub(子记录)记录. </summary>
        /// <param name="id"></param>
        void DeleteSiteMasterIndexInfo(string id);

        /// <summary> 更新SiteMasterIndex(主记录)的FullTextState值和主Job表的IndexStatus值. </summary>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        void UpdateFullTextState(string jobId, EOFullTextState status);

        ///// <summary> According to the subJob Id, update the subJob performance value. </summary>
        ///// <param name="subJobId"></param>
        ///// <param name="jobPerformance"></param>
        //void UpdateSubJobPerformance(string subJobId, string jobPerformance);

        void UpdateRestoreCacheTree(EORestoreCacheTreeContract treeContent);

        ExchangeOnlineBackupJobDto GetExchangeOnlineBackupJobByJobId(string backupJobId);

        void RunBackupJobById(string jobId);

        void PruningBackupDataByJobIds(List<string> jobIds, bool deleteJob);

        void HandleRetentionJob(ExchangePruningResult result, ExchangeOnlineBackupDataPruningMsg pruningMsg);
    }
}
