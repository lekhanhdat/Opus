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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using Merged18NResources.MediaServiceArchiverBackup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntention
{
    public interface IGDriveArchiverHeadAndBodyIndexService
    {
        void InsertArchiveIndexes(List<GoogleBasicIndex> indexes);
        void DeleteDataFromMainIndex(String storagePolicyId, String jobId);
        void DeleteDataFromMainIndexByTime(String storagePolicyId, String jobId, long dateTime, bool isFilterSoftDelete);
        void UpdateAsSoftDelete(String storagePolicyId, String jobId);
        void UpdateAsSoftDeleteByTime(String storagePolicyId, String jobId, long dateTime);
        List<GoogleBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId);
        List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(String storagePolicyId, String jobId, String siteURL, long dateTime = 0);

        Int64 GetJobDataMode(String jobId);

        String GetItemName(Int64 contentFileNumber, String jobId);

        List<String> GetStorageInfosByJobId(String jobId);

        List<string> GetUniqueRetentions();
        void DeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5);
        List<KeyValuePair<string, long>> GetDeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5, string siteURL);
        void DeletedDataFromMainIndexByNodeGuid(string jobId, List<string> nodeGuid);
        List<GoogleBasicIndex> GetRetentionData(string retentionId, long orphanTicks);
        long GetFileCount();
        long GetFileVersionCount();
        List<GoogleBasicIndex> GetDeletingIndexesByModifiedTime(String storagePolicyId, String jobId, long dateTime, bool filterSoftDeleteDatas);
        Dictionary<string, List<string>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId, ref String stubType, long modifiedTime = 0, bool isFilterSoftDelete = false);
        Dictionary<string, List<GoogleBasicIndex>> FilterDocumentsByJobId(String jobId, ref String stubType);

        string GetSiteUrlFromMainIndex(String storagePolicyId, String jobId);

        Dictionary<string, List<string>> FilterDocumentUrlForLifecycle(List<GoogleBasicIndex> item, String jobId, ref String stubType);
        List<GoogleBasicIndex> GetAllGoogleDatasFromItemTableByType(StringBuilder sql, ArchiverRestoreFilter filter, GDriveBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy);


        #region Restor
        GoogleBasicIndex GetOneDataFromHeadOrBodyTable(String path, Int64 endTime);
        List<GoogleBasicIndex> GetDatasFromBodyTable(ArchiverIndexInfo indexInfo);
        List<GoogleBasicIndex> GetDatasFromHeadTable(ArchiverIndexInfo indexInfo);

        List<GoogleBasicIndex> GetVersionsByItemIdFromBodyTable(int topCount, string ItemId, long endTime);
        #endregion
        GoogleBasicIndex GetParentDataFromHeadTable(GoogleBasicIndex childIndex);
        List<GoogleBasicIndex> GetAllBodyIndex();
        List<GoogleBasicIndex> GetAllHeadIndex();
        List<GoogleBasicIndex> GetAllBodyIndexOnSpecificTimeRange(GDriveBrowseInfo info);
        List<GoogleBasicIndex> GetAllHeadIndexOnSpecificTimeRange(GDriveBrowseInfo info);

    }
}
