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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IArchiverIndexSubInfoDao:IBaseDao<ArchiverIndexSubInfo>
    {
        void UpdateSubInfoes(params ArchiverIndexSubInfo[] subInfoes);
        List<ArchiverIndexSubInfoContract> GetSubInfoesBySubJobId(string subJobId);
        List<string> GetAllArchiverIndexSubSubJobIDs(string subJobId);
        List<string> GetAllBackupOrMergeIndexFailedSubJobIds();
        Task<List<(string, string)>> GetAllArchiverIndexSubInfoThatNoSubJobIdAsync();

        Task<List<string>> GetAllDeviceIDsAsync();

        Task<(bool, ArchiverIndexSubInfoContract)> TryGetSubInfoByJobIdAsync(string subSubJobId);

        Task<(bool, ArchiverIndexSubInfo)> TryGetByJobIdAsync(string subSubJobId);

        List<ArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByStorageId(string storageId);
        List<ArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByStorageIdAndSourceFlag(string storageId, List<int> sourceFlag);
        Task<bool> CheckIfExistArchiverIndexSubInfoByStorageIdAndSourceFlag(string storageId, List<int> sourceFlag);
        List<ArchiverIndexSubInfo> GetAllArchiverIndexSubInfoByMainJobId(string mainJobId);
        Task UpdateArchiverIndexSubInfoMergeIndexStatusAsync(string jobId, int status);
        Task UpdateGDriveArchiverIndexSubInfoMergeIndexStatusAsync(string jobId, int status);
        long GetArchiverStorageGBSize();
        long GetAOSPArchiverStorageGBSize();
        double GetArchiverStorageDoubleGBSize();

        Task<double> GetAllArchiverStorageGBSizeAsync(string storageId, IEnumerable<string> excludedJobPrefixes = null, CancellationToken cancellationToken = default);

        Dictionary<string, double> GetAllArchiverIndexSubInfoBySiteUrls(Dictionary<string, List<string>> SiteUrlJobIds);
        Dictionary<string, double> GetAllGoogleArchiverIndexSubInfoByDriveIds(Dictionary<string, List<string>> siteIdsJobIds);

        double GetAllArchivedSizeBySubJobIdInGB(string subJobId);

        Task CreateByBulkCopyAsync(IEnumerable<ArchiverIndexSubInfoContract> items);
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedIndexSubInfoesAsync();
        Task UpdateArchiverIndexSubInfoMediaSizeAsync(string jobId, long size);
        Task UpdateArchiverIndexSubInfoMediaSizeForAdjustAsync(string jobId, long size);
        int BatchUpdateSubJobId(List<ArchiverIndexSubInfo> items);
        List<ArchiverIndexSubInfo> GetAllDisposalArchiverIndexSubInfo(long timeOlder);

        List<ArchiverIndexSubInfo> GetAllSubInfos();
        bool CheckExistSoftInfoAndUpdateThem(List<string> jobIds);
        Task<ArchiverIndexSubInfo> GetSubInfoBySubsubJobIdAsync(string subsubjobId);
        Task<ArchiverIndexSubInfo> GetSubInfoByJobIdAsync(string jobId);
        Task<bool> ExistsSubInfoAsync(string subJobId);

        Task<int> GetSubInfoCountAsync(string subJobId);
    }
}
