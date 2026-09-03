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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public interface ICommonSiteMasterIndexService
    {
        void InsertSiteMaster(CommonSiteMasterIndex siteMasterIndex);
        Int32 GetSPVersionBySiteCollection(String siteCollection);
        List<CommonSiteMasterIndex> GetAllSiteMasterIndex();
        List<CommonSiteMasterIndex> GetAllSiteMasterIndex(int dataType);
        Int64 GetRetentionTimeSpanByJobId(String jobId);
        ArchiverSiteMasterIndexContract GetTeamsInfo(ArchiverSiteMasterIndexContract site);
        ArchiverSiteMasterIndexContract GetSiteCollectionStorageInfo(ArchiverSiteMasterIndexContract site);
        List<ArchiverSiteMasterIndexContract> GetSiteCollectionWithSubInfos(ArchiverSiteMasterIndexContract index);

        List<ArchiverSiteMasterIndexContract> LoadSiteMasterIndexByJobIdOrTeamsGroup(string teamsGroupAddress, string jobId);
        Task BulkCopySiteMasterIndexesAsync(IEnumerable<ArchiverSiteMasterIndexContract> items);
        Task BulkCopyIndexSubInfoesAsync(IEnumerable<ArchiverIndexSubInfoContract> items);

        /// <summary>
        /// This method can only be called in the Cloud Archiver Migration Job to clean up historical data
        /// </summary>
        Task<int> DeleteMigratedSiteMasterIndexesAsync();

        /// <summary>
        /// This method can only be called in the Cloud Archiver Migration Job to clean up historical data
        /// </summary>
        Task<int> DeleteMigratedIndexSubInfoesAsync();

        Task<string> GetFailedJobsDataAsync(JMPager pager);
    }
}
