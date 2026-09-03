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
    using AvePoint.Media.Service.DomainModel;
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    public interface IArchiverRetentionIndexService
    {
        void DeleteDataFromMainIndex(String storagePolicyId, String jobId);
        void DeleteDataFromMainIndexByDateTime(String storagePolicyId, String jobId, long dateTime,bool isSoftDelete);
        void UpdateAsSoftDelete(String storagePolicyId, String jobId);
        void UpdateAsSoftDeleteByDateTime(String storagePolicyId, String jobId, long dateTime);
        List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId);
        List<ArchivedFileIndexInfo> GetArchivedFileIndexes(String storagePolicyId, String jobId);
        List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(String storagePolicyId, String jobId, String siteURL, long dateTime = 0);
        long GetFileNumber();
        long GetFileVersionNumber();
        Dictionary<string, List<(string, string)>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId,ref String stubType);
        Dictionary<string, List<(string, string)>> FilterDocumentUrlFromMainIndexByModifiedTime(String storagePolicyId, String jobId, ref String stubType,long modifiedTime, bool isFilterSoftDelete);
        Dictionary<string, List<ArchiverBasicIndex>> FilterDocumentsByJobId(String jobId, ref String stubType);
        Dictionary<string, List<string>> FilterDocumentUrlForLifecycle(List<ArchiverBasicIndex> item, String jobId, ref String stubType);
        string GetSiteUrlFromMainIndex(String storagePolicyId, String jobId);
        List<String> GetStorageInfosByJobId(String jobId);
        Int64 GetJobDataMode(String jobId);
        
        /// <summary>
        /// Batch retrieves DataMode for multiple job IDs (eliminates N+1 query problem)
        /// </summary>
        /// <param name="jobIds">Collection of job IDs to query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary mapping JobId to DataMode value</returns>
        Task<Dictionary<string, Int64>> GetJobDataModesBatchAsync(IEnumerable<string> jobIds, CancellationToken cancellationToken = default);
        
        String GetItemName(Int64 contentFileNumber, String jobId);

        #region Archiver/Records Lifecycle
        List<string> GetUniqueRetentions();
        void DeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5);
        List<KeyValuePair<string, long>> GetDeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5, string siteURL);
        void DeletedDataFromMainIndexByNodeGuid(string jobId, List<string> nodeGuid);
        List<ArchiverBasicIndex> GetRetentionData(string retentionId, long orphanTicks);
        List<ArchiverBasicIndex> GetDeletingIndexesByModifiedTime(String storagePolicyId, String jobId,long time,bool filterSoftDeleteDatas);
        #endregion
    }
}