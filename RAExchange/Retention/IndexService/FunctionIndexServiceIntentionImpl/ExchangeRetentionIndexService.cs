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




namespace RAExchangeRetention
{
    using AvePoint.Media.Service.DomainModel;
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion
    public class ExchangeRetentionIndexService
        : ExchangeFunctionIndexServiceBase
        , IExchangeRetentionIndexService
    {
        public void UpdateAccessTier(int tier, string jobid)
        {
            this.ContainerItemIndexService.UpdateAccessTier(tier, jobid);
        }
        public bool IsExistsIndexRelatedToJob(string jobId)
        {
            return this.ContainerItemIndexService.IsExistsIndexRelatedToJob(jobId);
        }
        public List<String> GetEntireCycleStorageInfos()
        {
            return this.ContainerItemIndexService.GetEntireCycleStorageInfos();
        }

        public List<String> GetStorageInfosExceptFullBackup()
        {
            return this.ContainerItemIndexService.GetStorageInfosExceptFullBackup();
        }

        public List<String> GetStorageInfosByJobId(String jobId)
        {
            return this.ContainerItemIndexService.GetStorageInfosByJobId(jobId);
        }

        public void DeleteItemByJobId(String jobId)
        {
            this.ContainerItemIndexService.DeleteItemByJobId(jobId);
        }

        public List<ArchiverBasicIndex> GetDeletingIndexesByModifiedTime(string storagePolicyId, string jobId, long time, bool filterSoftDeleteDatas)
        {
            throw new NotImplementedException();
        }

        public void DeleteDataFromMainIndex(string storagePolicyId, string jobId)
        {
            this.ContainerItemIndexService.DeleteContainerAndItemIndexByStorageAndJobId(storagePolicyId, jobId);
        }

        public void DeleteDataFromMainIndexByDateTime(string storagePolicyId, string jobId, long dateTime, bool isSoftDelete)
        {
            throw new NotImplementedException();
        }

        public void UpdateAsSoftDelete(string storagePolicyId, string jobId)
        {
            this.ContainerItemIndexService.UpdateAsSoftDelete(storagePolicyId, jobId);
        }

        public void UpdateAsSoftDeleteByDateTime(string storagePolicyId, string jobId, long dateTime)
        {
            throw new NotImplementedException();
        }

        public List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(string storagePolicyId, string jobId)
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(string storagePolicyId, string jobId, string siteURL, long dateTime = 0)
        {
            throw new NotImplementedException();
        }

        public long GetFileNumber()
        {
            throw new NotImplementedException();
        }

        public long GetFileVersionNumber()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, List<string>> FilterDocumentUrlFromMainIndex(string storagePolicyId, string jobId, ref string stubType)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, List<string>> FilterDocumentUrlFromMainIndexByModifiedTime(string storagePolicyId, string jobId, ref string stubType, long modifiedTime, bool isFilterSoftDelete)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, List<ArchiverBasicIndex>> FilterDocumentsByJobId(string jobId, ref string stubType)
        {
            throw new NotImplementedException();
        }

        public string GetSiteUrlFromMainIndex(string storagePolicyId, string jobId)
        {
            throw new NotImplementedException();
        }

        public long GetJobDataMode(string jobId)
        {
            throw new NotImplementedException();
        }

        public string GetItemName(long contentFileNumber, string jobId)
        {
            throw new NotImplementedException();
        }
    }
}