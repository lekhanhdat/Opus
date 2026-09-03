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
    using RAFileSystem.FileSystem.Common;
    using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;
    #region using directives
    using System;
    using System.Collections.Generic;
    #endregion

    public class ArchiverRetentionIndexService
        : ArchiverIndexServiceBase
        , IArchiverRetentionIndexService
    {
        private Dictionary<string, ArchiveIndexInfo> masterIndexDic = new Dictionary<string, ArchiveIndexInfo>();
        public void DeleteDataFromMainIndex(String storagePolicyId, String jobId)
        {
            this.HeadAndBodyService.DeleteDataFromMainIndex(storagePolicyId, jobId);
            this.SiteMasterService.DeleteSiteMasterByJobId(jobId);
        }
        public void DeleteDataFromMainIndexByDateTime(String storagePolicyId, String jobId,long dateTime ,bool isFilterSoftDelete)
        {
            this.HeadAndBodyService.DeleteDataFromMainIndexByTime(storagePolicyId, jobId, dateTime, isFilterSoftDelete);
            var indexDatas = this.HeadAndBodyService.GetDeletingDataFromMainIndex(storagePolicyId, jobId);
            if (indexDatas.Count == 0)
            {
                this.SiteMasterService.DeleteSiteMasterByJobId(jobId);
            }
        }
        private ArchiveIndexInfo GetMasterIndexInternal(string jobId)
        {
            if (masterIndexDic.ContainsKey(jobId))
            {
                return masterIndexDic[jobId];
            }
            else
            {
                var masterIndexTemp = this.SiteMasterService.GetSiteMasterByJobId(jobId);
                masterIndexDic.Add(jobId, masterIndexTemp);
                return masterIndexTemp;
            }
        }
        public void UpdateAsSoftDelete(String storagePolicyId, String jobId)
        {
            this.HeadAndBodyService.UpdateAsSoftDelete(storagePolicyId, jobId);
        }
        public void UpdateAsSoftDeleteByDateTime(String storagePolicyId, String jobId, long dateTime)
        {
            this.HeadAndBodyService.UpdateAsSoftDeleteByTime(storagePolicyId, jobId, dateTime);
        }
        public void InitIndexProcesser(ArchiverIndexService indexService)
        {
            this.HeadAndBodyService.InitIndexProcesser(indexService);
            this.SiteMasterService.InitIndexProcesser(indexService);
        }
        public List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId)
        {
            var result = this.HeadAndBodyService.GetDeletingDataFromMainIndex(storagePolicyId, jobId);
            var masterIndexInfo = GetMasterIndexInternal(jobId);
            foreach (var temp in result)
            {
                DtoConverter.SetMasterIndexValue(masterIndexInfo, temp);
            }
            return result;
        }
        public List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(String storagePolicyId, String jobId, String siteURL,long dateTime = 0)
        {
            return this.HeadAndBodyService.GetDeleteDataFromMainIndex(storagePolicyId, jobId, siteURL, dateTime);
        }

        public Dictionary<string, List<string>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId,ref String stubType)
        {
            return this.HeadAndBodyService.FilterDocumentUrlFromMainIndex(storagePolicyId, jobId,ref stubType);
        }
        public Dictionary<string, List<string>> FilterDocumentUrlFromMainIndexByModifiedTime(String storagePolicyId, String jobId, ref String stubType,long modifiedTime,bool isFilterSoftDelete)
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
            var result = this.HeadAndBodyService.GetDeletingIndexesByModifiedTime(storagePolicyId, jobId, time, filterSoftDeleteDatas);
            var masterIndexInfo = GetMasterIndexInternal(jobId);
            foreach (var temp in result)
            {
                DtoConverter.SetMasterIndexValue(masterIndexInfo, temp);
            }
            return result;
        }
        #endregion
    }
}