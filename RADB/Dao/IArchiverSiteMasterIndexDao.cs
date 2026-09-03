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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IArchiverSiteMasterIndexDao : IBaseDao<ArchiverSiteMasterIndex>
    {
        Task<long> GetMaxArchiverTimeAsync();
        IAsyncEnumerable<ArchiverSiteMasterIndexContract> GetSiteMasterIndexesAsync(long minArchiverTime, long maxArchiverTime);
        IAsyncEnumerable<ArchiverSiteMasterIndexContract> GetSiteMasterIndexesAsync(string siteUrl, long minArchiverTime, long maxArchiverTime);
        IAsyncEnumerable<ArchiverSiteMasterIndexContract> GetSiteMasterIndexesBySiteUrlsAsync(IEnumerable<string> siteUrls, long minArchiverTime, long maxArchiverTime);
        Task<int> CountSiteMasterIndexesAsync(long minArchiverTime, long maxArchiverTime);
        public void UpdateStateByJobId(int status, string jobId);
        ArchiverSiteMasterIndexContract GetSiteCollectionInfo(ArchiverSiteMasterIndexContract site);
        ArchiverSiteMasterIndexContract GetGoogleDriveInfo(ArchiverSiteMasterIndexContract site);
        Task<(bool Has, ArchiverSiteMasterIndexContract indexContract)> TryGetSiteMasterIndexAsync(string jobId);
        List<ArchiverSiteMasterIndex> GetAllSiteCollectionNodsInfo(List<int> flagIgnores = null);
        List<ArchiverSiteMasterIndex> GetAllGoogleNodesInfo();
        List<ArchiverSiteMasterIndex> GetAllSiteCollectionNodsInfoByUrl(string url);
        Task<bool> ExistsArchivedDataAsync(string siteURL);
        bool ExistsRestoringSiteCollectionByUrl(string url);
        ArchiverSiteMasterIndex GetRestoringSiteCollectionInfoByUrl(string url);
        string GetSiteIdByUrl(string url);
        string InsertIntoArchiverSiteMasterIndex(ArchiverSiteMasterIndexContract indexDto);
        List<ArchiverSiteMasterIndexContract> GetSiteCollectionStorageInfo(ArchiverSiteMasterIndexContract site);
        List<ArchiverSiteMasterIndexContract> GetIndexByJobId(string jobId);

        bool IsFileLevelBlockBackup(string jobId);
        List<string> GetAllBackupSiteCollectionDistinctUrl();
        List<ArchiverSiteMasterIndexContract> GetAllBackupGoogleDriveIndexs();
        Dictionary<string, List<string>> GetAllBackupSiteCollectionDistinctJobIdMappings(List<string> siteUrls);

        Dictionary<string, List<string>> GetAllBackupSiteCollectionDistinctJobIdMappings(List<string> siteUrls, long startTime, long endTime);
        Dictionary<string, List<string>> GetAllBackupGDriveDistinctJobIdMappings(List<string> driveIds);
        Dictionary<string, List<string>> GetAllBackupGDriveCollectionDistinctJobIdMappings(List<string> driveIds, long startTime, long endTime);
        Dictionary<string, (double archivedSizeInGB, string groupMailboxAddress)> GetAllSiteArchivedSizeInGBAndGroupMailBox(long startTime, long endTime);

        Dictionary<string, double> GetSiteArchivedSizeInGB();

        List<ArchiverSiteMasterIndex> GetGetAllWithMoveDataTierFlagAndArchiverTime(int interval);
        void SetMoveDateTierFlag(string jobId);
        Task CreateByBulkCopyAsync(IEnumerable<ArchiverSiteMasterIndexContract> items);
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedSiteMasterIndexesAsync();
        List<ArchiverSiteMasterIndex> GetAllDisposalJobNodsInfo(long timeOlder);

        void UpdateArchiverMasterIndexDeduplicatedState(IEnumerable<string> idList);

        Task<Dictionary<string, List<string>>> GetAllUnDedupArchiverSiteMasterIndexesAsync();
        Dictionary<string, List<string>> GetAllUnDedupArchiverSiteMasterIndexes(IEnumerable<string> siteURLs);

        string GetSiteId(string masterIndexId);

        List<ArchiverSiteMasterIndex> GetAllSiteMastersInfo();
        List<ArchiverSiteMasterIndex> GetSiteMastersInfoByMainJobId(string mainJobId);
        List<ArchiverSiteMasterIndex> GetSiteMastersInfoByJobIds(List<string> jobIds);
        void UpdateGroupMailboxAddressBySiteURL(IEnumerable<string> siteURLs, string groupMailboxAddress);
        Dictionary<string, List<string>> GetAllBackSiteCollectionGroupMailboxMapping();
        IAsyncEnumerable<IEnumerable<string>> GetAllSiteDistinctUrlAsync();

        List<ArchiverSiteMasterIndexContract> GetGDriveStorageInfo(ArchiverSiteMasterIndexContract site);

        Task<(List<ArchiverSiteMasterIndex> Items, int TotalCount)> GetSiteCollectionNodesByFilterAsync(
            IEnumerable<Guid> containerIds,
            string filterKeyword,
            int pageIndex,
            int pageSize,
            bool filterByContainers);

        Task<ArchiverSiteMasterIndex?> GetLatestSiteCollectionNodeInfoByUrlAsync(string url);

        List<string> GetExistingSiteCollectionUrls(IEnumerable<string> siteUrls);

        List<string> LoadSiteCollectionUrlsByJobIdOrTeamsGroup(string jobId, string teamsGroupAddress);

        List<ArchiverSiteMasterIndexContract> LoadSiteMasterIndexByJobIdOrTeamsGroup(string jobId, string teamsGroupAddress);

        Task<(string, string, string)> GetArchivedChannelSiteInfoAsync(string siteCollectionUrl);
        Task<string> GetO365TenantIdBySiteCollectionAsync(string siteCollectionUrl);
    }
}
