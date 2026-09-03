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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using AvePoint.RA.DB.Dao.Impl;
    #endregion

    public interface IArchiverSiteMasterIndexService
    {
        void InsertSiteMaster(ArchiverSiteMasterIndex siteMasterIndex);
        Int32 GetSPVersionBySiteCollection(String siteCollection);
        List<ArchiverSiteMasterIndex> GetAllSiteMasterIndex();
        Int64 GetRetentionTimeSpanByJobId(String jobId);
        ArchiverSiteMasterIndexContract GetSiteCollectionInfo(ArchiverSiteMasterIndexContract site);
        ArchiverSiteMasterIndexContract GetGoogleDriveInfo(ArchiverSiteMasterIndexContract site);
        ArchiverSiteMasterIndexContract GetSiteCollectionStorageInfo(ArchiverSiteMasterIndexContract site);
        List<string> GetExistingSiteCollectionUrls(IEnumerable<string> siteUrls);
        List<ArchiverSiteMasterIndexContract> GetSiteCollectionWithSubInfos(ArchiverSiteMasterIndexContract index);
        List<ArchiverSiteMasterIndexContract> GetGoogleDriveWithSubInfos(ArchiverSiteMasterIndexContract index);
        Task BulkCopySiteMasterIndexesAsync(IEnumerable<ArchiverSiteMasterIndexContract> items);
        Task BulkCopyIndexSubInfoesAsync(IEnumerable<ArchiverIndexSubInfoContract> items);

        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedSiteMasterIndexesAsync();
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedIndexSubInfoesAsync();
        Task<string> GetFailedJobsDataAsync(JMPager pager);

        Task<(string, string, string)> GetArchivedChannelSiteInfoAsync(string siteCollectionUrl);
    }
}