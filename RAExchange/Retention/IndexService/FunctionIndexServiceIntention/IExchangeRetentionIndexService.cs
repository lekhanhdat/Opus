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
    #endregion

    public interface IExchangeRetentionIndexService
    {
        void UpdateAccessTier(int tier, string jobid);
        bool IsExistsIndexRelatedToJob(string jobId);
        List<String> GetEntireCycleStorageInfos();
        List<String> GetStorageInfosExceptFullBackup();
        
        void DeleteItemByJobId(String jobId);

        List<ArchiverBasicIndex> GetDeletingIndexesByModifiedTime(String storagePolicyId, String jobId, long time, bool filterSoftDeleteDatas);
        void DeleteDataFromMainIndex(String storagePolicyId, String jobId);
        void DeleteDataFromMainIndexByDateTime(String storagePolicyId, String jobId, long dateTime, bool isSoftDelete);
        void UpdateAsSoftDelete(String storagePolicyId, String jobId);
        void UpdateAsSoftDeleteByDateTime(String storagePolicyId, String jobId, long dateTime);
        List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId);
        List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(String storagePolicyId, String jobId, String siteURL, long dateTime = 0);
        long GetFileNumber();
        long GetFileVersionNumber();
        Dictionary<string, List<string>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId, ref String stubType);
        Dictionary<string, List<string>> FilterDocumentUrlFromMainIndexByModifiedTime(String storagePolicyId, String jobId, ref String stubType, long modifiedTime, bool isFilterSoftDelete);
        Dictionary<string, List<ArchiverBasicIndex>> FilterDocumentsByJobId(String jobId, ref String stubType);
        //Dictionary<string, List<string>> FilterDocumentUrlForLifecycle(List<ArchiverBasicIndex> item, String jobId, ref String stubType);
        string GetSiteUrlFromMainIndex(String storagePolicyId, String jobId);
        List<String> GetStorageInfosByJobId(String jobId);
        Int64 GetJobDataMode(String jobId);
        String GetItemName(Int64 contentFileNumber, String jobId);
    }
}