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
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.Media.Service.DomainModel;
    using RAFileSystem.FileSystem.FileSystem.Backup;

    #endregion using directives

    public interface IArchiverHeadAndBodyIndexService
    {
        void InsertArchiveIndexes(List<ArchiverBasicIndex> indexes);

        Dictionary<string, List<string>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId,ref String stubType,long modifiedTime = 0, bool isFilterSoftDelete = false);
        Dictionary<string, List<ArchiverBasicIndex>> FilterDocumentsByJobId(String jobId, ref String stubType);

        string GetSiteUrlFromMainIndex(String storagePolicyId, String jobId);

        Dictionary<string, List<string>> FilterDocumentUrlForLifecycle(List<ArchiverBasicIndex> item, String jobId, ref String stubType);
        void DeleteDataFromMainIndex(String storagePolicyId, String jobId);
        void DeleteDataFromMainIndexByTime(String storagePolicyId, String jobId,long dateTime, bool isFilterSoftDelete);
        void UpdateAsSoftDelete(String storagePolicyId, String jobId);
        void UpdateAsSoftDeleteByTime(String storagePolicyId, String jobId, long dateTime);
        List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId);
        List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(String storagePolicyId, String jobId, String siteURL,long dateTime=0);

        Int64 GetDatasCountFromBodyTable(String parentPath, Int64 endTime);

        Int64 GetJobDataMode(String jobId);

        String GetItemName(Int64 contentFileNumber, String jobId);

        List<String> GetStorageInfosByJobId(String jobId);

        ArchiverBasicIndex GetParentDataFromHeadTable(ArchiverBasicIndex childIndex);

        ArchiverBasicIndex GetOneDataFromHeadOrBodyTable(String path, Int64 endTime);
        void UpdateRetentionStatus(String path, Int64 endTime);
        ArchiverBasicIndex GetOneDataFromHeadByPathMd5(String pathMd5, Int64 endTime);
        ArchiverBasicIndex GetNextBodyIndexBySequence(String jobId, long sequence);

        List<ArchiverBasicIndex> GetDatasFromHeadTable(ArchiverIndexInfo indexInfo);

        List<ArchiverBasicIndex> GetDatasFromBodyTable(ArchiverIndexInfo indexInfo);
        List<ArchiverBasicIndex> GetVersionsByItemIdFromBodyTable(int topCount, string ItemId, long endTime);
        List<ArchiverBasicIndex> GetDatasFromHeadTable2(ArchiverIndexInfo indexInfo);

        List<ArchiverBasicIndex> GetDatasFromBodyTable2(ArchiverIndexInfo indexInfo);

        List<ArchiverBasicIndex> GetAllDatasFromHeadOrBodyTableByType(StringBuilder sql, ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam);
        List<ArchiverBasicIndex> GetAllDatasFromHeadOrBodyTableByTypeForJob(string sql, ArchiverBrowseInfo restoreParam);
        long GetSubSiteArchiveSize(string subSiteUrl, ArchiverBrowseInfo info);
        ArchiverBasicIndex GetNextIndexBySequence(String jobId, long sequence);
        List<ArchiverBasicIndex> GetAllBodyIndex();

        List<ArchiverBasicIndex> GetAllHeadIndex();
        List<ArchiverBasicIndex> GetHeadIndexPage(int pageSize, int pageOffset);
        List<ArchiverBasicIndex> GetBodyIndexPage(int pageSize, int pageOffset);
        List<ArchiverBasicIndex> GetAllSubSites(ArchiverBrowseInfo info);
        List<ArchiverBasicIndex> GetAllBodyIndexOnSpecificTimeRange(ArchiverBrowseInfo info);

        List<ArchiverBasicIndex> GetAllHeadIndexOnSpecificTimeRange(ArchiverBrowseInfo info);

        #region Archiver/Records Lifecycle
        List<string> GetUniqueRetentions();
        void DeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5);
        List<KeyValuePair<string, long>> GetDeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5, string siteURL);
        void DeletedDataFromMainIndexByNodeGuid(string jobId, List<string> nodeGuid);
        List<ArchiverBasicIndex> GetRetentionData(string retentionId, long orphanTicks);
        long GetFileCount();
        long GetFileVersionCount();
        List<ArchiverBasicIndex> GetDeletingIndexesByModifiedTime(String storagePolicyId, String jobId, long dateTime, bool filterSoftDeleteDatas);
        #endregion

        #region Full Text Index

        Int64 GetIndexTotalCount(String jobId, String isSystemFile);

        ArchiverBasicIndex GetParentFolder(ArchiverBasicIndex childIndex, String version);

        List<ArchiverBasicIndex> GetNeedFiles(String jobId, String siteUrl, Int32 offset, Int32 length, String isSystemFile);

        #endregion Full Text Index

        #region End User Archiver

        ArchiverBasicIndex GetIndex(String pathMd5);
        ArchiverBasicIndex GetBodyIndexByMD5(String pathMd5);
        ArchiverBasicIndex GetIndex(String pathMd5, String subJobId);

        ArchiverBasicIndex GetParentIndex(String pathMd5);

        List<ArchiverBasicIndex> GetChildIndexList(String pathMd5);

        Int64 GetChildCount(String pathMd5);

        Boolean CheckSiteCollection(String siteUrl);

        Boolean CheckNormalUrl(String url);

        Boolean CheckItemUrl(String url);
        void InitIndexProcesser(ArchiverIndexService indexService);

        #endregion End User Archiver

        #region EDiscovery Hold

        ArchiverBasicIndex GetNeedHoldItemFromHeadTable(String jobId, String name, String pathMD5);

        List<ArchiverBasicIndex> GetAttachments(String parentPathMD5, String name, String type);

        #endregion EDiscovery Hold
    }
}