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

    public class ArchiverRetentionIndexService
        : ArchiverIndexServiceBase
        , IArchiverRetentionIndexService
    {
        public void DeleteDataFromMainIndex(String storagePolicyId, String jobId)
        {
            this.HeadAndBodyService.DeleteDataFromMainIndex(storagePolicyId, jobId);
        }
        public void DeleteDataFromMainIndexByDateTime(String storagePolicyId, String jobId,long dateTime ,bool isFilterSoftDelete)
        {
            this.HeadAndBodyService.DeleteDataFromMainIndexByTime(storagePolicyId, jobId, dateTime, isFilterSoftDelete);
        }
        public void UpdateAsSoftDelete(String storagePolicyId, String jobId)
        {
            this.HeadAndBodyService.UpdateAsSoftDelete(storagePolicyId, jobId);
        }
        public void UpdateAsSoftDeleteByDateTime(String storagePolicyId, String jobId, long dateTime)
        {
            this.HeadAndBodyService.UpdateAsSoftDeleteByTime(storagePolicyId, jobId, dateTime);
        }
        public List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId)
        {
            return this.HeadAndBodyService.GetDeletingDataFromMainIndex(storagePolicyId, jobId);
        }
        public List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(String storagePolicyId, String jobId, String siteURL,long dateTime = 0)
        {
            return this.HeadAndBodyService.GetDeleteDataFromMainIndex(storagePolicyId, jobId, siteURL, dateTime);
        }

        public List<ArchivedFileIndexInfo> GetArchivedFileIndexes(String storagePolicyId, String jobId)
        {
            return this.HeadAndBodyService.GetArchivedFileIndexes(storagePolicyId, jobId);
        }

        public Dictionary<string, List<(string, string)>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId,ref String stubType)
        {
            return this.HeadAndBodyService.FilterDocumentUrlFromMainIndex(storagePolicyId, jobId,ref stubType);
        }
        public Dictionary<string, List<(string, string)>> FilterDocumentUrlFromMainIndexByModifiedTime(String storagePolicyId, String jobId, ref String stubType,long modifiedTime,bool isFilterSoftDelete)
        {
            return this.HeadAndBodyService.FilterDocumentUrlFromMainIndex(storagePolicyId, jobId, ref stubType, modifiedTime, isFilterSoftDelete);
        }
        public Dictionary<string, List<ArchiverBasicIndex>> FilterDocumentsByJobId(String jobId, ref String stubType)
        {
            return this.HeadAndBodyService.FilterDocumentsByJobId(jobId, ref stubType);
        }

        public Dictionary<string, List<string>> FilterDocumentUrlForLifecycle(List<ArchiverBasicIndex> item, String jobId, ref String stubType)
        {
            return this.HeadAndBodyService.FilterDocumentUrlForLifecycle(item, jobId, ref stubType);
        }

        public string GetSiteUrlFromMainIndex(String storagePolicyId, String jobId)
        {
            return this.HeadAndBodyService.GetSiteUrlFromMainIndex(storagePolicyId, jobId);
        }
        public List<String> GetStorageInfosByJobId(String jobId)
        {
            return this.HeadAndBodyService.GetStorageInfosByJobId(jobId);
        }

        public Int64 GetJobDataMode(String jobId)
        {
            return this.HeadAndBodyService.GetJobDataMode(jobId);
        }

        /// <summary>
        /// Batch retrieves DataMode for multiple job IDs (eliminates N+1 query problem)
        /// </summary>
        public async Task<Dictionary<string, Int64>> GetJobDataModesBatchAsync(IEnumerable<string> jobIds, CancellationToken cancellationToken = default)
        {
            return await this.HeadAndBodyService.GetJobDataModesBatchAsync(jobIds, cancellationToken);
        }

        public String GetItemName(Int64 contentFileNumber, String jobId)
        {
            return this.HeadAndBodyService.GetItemName(contentFileNumber, jobId);
        }


        #region Archiver/Records Lifecycle
        public List<string> GetUniqueRetentions()
        {
            return this.HeadAndBodyService.GetUniqueRetentions();
        }

        public List<ArchiverBasicIndex> GetRetentionData(string retentionId, long orphanTicks)
        {
            return this.HeadAndBodyService.GetRetentionData(retentionId, orphanTicks);
        }

        public void DeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5)
        {
            this.HeadAndBodyService.DeletedDataFromMainIndexByPathMD5(jobId, pathMD5);
        }
        
        public List<KeyValuePair<string, long>> GetDeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5, string siteURL)
        {
            return this.HeadAndBodyService.GetDeletedDataFromMainIndexByPathMD5(jobId, pathMD5, siteURL);
        }

        public void DeletedDataFromMainIndexByNodeGuid(string jobId, List<string> nodeGuid)
        {
            this.HeadAndBodyService.DeletedDataFromMainIndexByNodeGuid(jobId, nodeGuid);
        }

        public long GetFileNumber()
        {
            return this.HeadAndBodyService.GetFileCount();
        }

        public long GetFileVersionNumber()
        {
            return this.HeadAndBodyService.GetFileVersionCount();
        }

        public List<ArchiverBasicIndex> GetDeletingIndexesByModifiedTime(string storagePolicyId, string jobId, long time,bool filterSoftDeleteDatas)
        {
            return this.HeadAndBodyService.GetDeletingIndexesByModifiedTime(storagePolicyId, jobId, time,filterSoftDeleteDatas);
        }
        #endregion
    }
}