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
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao
{
    public interface ICommonSiteMasterIndexDao : IBaseDao<CommonSiteMasterIndex>
    {
        Task<long> GetMaxArchiverTimeAsync();
        ArchiverSiteMasterIndexContract GetTeamsInfo(ArchiverSiteMasterIndexContract site);
        Task<(bool Has, ArchiverSiteMasterIndexContract indexContract)> TryGetSiteMasterIndexAsync(string jobId);
        List<CommonSiteMasterIndex> GetAllSiteCollectionNodsInfoByUrl(string url);
        List<CommonSiteMasterIndex> GetAllTeamIndexInfoes();
        bool ExistsTeamsGroupIndex();
        string InsertIntoCommonSiteMasterIndex(ArchiverSiteMasterIndexContract indexDto);
        List<ArchiverSiteMasterIndexContract> GetSiteCollectionStorageInfo(ArchiverSiteMasterIndexContract site);
        List<ArchiverSiteMasterIndexContract> GetIndexByJobId(string jobId);
        Task CreateByBulkCopyAsync(IEnumerable<ArchiverSiteMasterIndexContract> items);
        Task<int> DeleteMigratedSiteMasterIndexesAsync();
        Task<(string, string)> GetExtensionAsync(string jobId);
        Task<bool> UpdateExtensionAsync(string indexId, string extension);
        Task UpdateMergeIndexStateAsync(string jobId);
        Dictionary<string, List<string>> GetAllBackupTeamsDistinctJobIdMappings(List<string> mailboxAddress);
        Dictionary<string, List<string>> GetTeamsGroupWithRelatedSitesUrlMappings(List<string> mailboxAddress);
        Dictionary<string, List<string>> GetAllBackupTeamsDistinctJobIdMappings(List<string> mailboxAddress, long startTime, long endTime);
        List<CommonSiteMasterIndex> GetTeamIndexInfoesByTimeRange(long startTime, long endTime);
        Task<List<string>> GetAllRelatedSPSiteUrls(List<string> teamsIds);
        (Dictionary<string, double>, Dictionary<string, string>) GetAllTeamsArchivedSizeAndSiteURLs((long, long)? archivedTimeRange = null);
        List<CommonSiteMasterIndex> GetAllCommonSiteMasterIndexes();
    }
}
