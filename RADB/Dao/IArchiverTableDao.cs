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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IArchiverTableDao
    {
        void DeleteItemsByRowKey(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, List<string> itemRowKeys);

        Task UpdateItemsToApprovedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, List<string> itemRowKeys);
        Task UpdateItemStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid nodeId, bool isApproval);

        Task ResetItemStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid nodeId);

        Task UpdateItemStatusForSPOnPremAsync(string connectionInfo, string tenantGroupId, string partKey, Guid nodeId, bool isApproval);

        Task ResetItemStatusForSPOnPremAsync(string connectionInfo, string tenantGroupId, string partKey, Guid nodeId);

        Task UpdateItemStatusForEXOAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, string rowKey, bool isApproval);

        Task ResetItemStatusForEXOAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, string rowKey);

        Task UpdateItemStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partKey, string rowKey, bool isApproval);

        Task ResetItemStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partKey, string rowKey);

        Task UpdateItemDisposalActionAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid nodeId, RelatedRecordOption relatedRecordOption);
        Task UpdateItemDisposalActionAsync(string connectionString, string tenantGroupId, string partKey, Guid nodeId, RelatedRecordOption relatedRecordOption);
        Task UpdateItemDisposalActionForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partKey, string rowKey, RelatedRecordOption relatedRecordOption);
        Task UpdateItemsToRejectedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, List<string> itemRowKeys);

        Task UpdateItemsToExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, List<string> itemRowKeys, SourceFlag sourceFlag = SourceFlag.SharePoint);
        Task UpdateItemsToExportedStatusForSPOnPremAsync(string connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys);
        Task UpdateItemsToNotExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, List<string> itemRowKeys);
        ArchiverTableEntity GetArchiverItem(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, string itemRowKey);
        #region fs
        FileSystemTableEntity GetArchiverItemForFS(string connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey);
        IEnumerable<FileSystemTableEntity> AddArchiverItemsForFS(string connectionInfo, string tenantGroupId, List<FileSystemTableEntity> entities);
        void RemoveArchiverItemsForFS(string connectionInfo, string tenantGroupId, List<FileSystemTableEntity> entities);
        FileSystemTableEntity AddArchiverItemForFS(string connectionInfo, string tenantGroupId, FileSystemTableEntity entity);
        IEnumerable<FileSystemTableEntity> GetArchiveItemsByPageForFS(string connectionInfo, string tenantGroupId, string jobId, int pageIndex, int pageSize);
        IEnumerable<FileSystemTableEntity> GetAzureDataByFolderForFS(string connectionInfo, string tenantGroupId, string folderId, string scopeId, long sortTicks, int pageSize);
        int MoveRecordsToStaticForConnectionForFS(string connectionInfo, string tenantGroupId, string connectionPath, string scopeId);
        List<Guid> AddRejectItemsForFS(string connectionInfo, string tenantGroupId, List<FileSystemTableEntity> fileSystemTableEntities);
        #endregion
        ArchiverTableEntity GetDestroyItem(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version, bool isRetention = false);
        OnPremiseSPTableEntity GetDestroyItemForSPOnPrem(string connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version);
        ArchiverExchangeOnlineDto GetArchiverItemForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey);
        ArchiverExchangeOnlineDto GetDestroyItemForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey);
        FileSystemTableEntity GetDestroyItemForFS(string sAzureTableConnectStr, string tenantGroupId, string partitionKey, string itemRowKey);
        List<FileSystemTableEntity> GetDestroyItemForFSDesctruntion(string fsAzureTableConnectStr, string tenantGroupId, string partitionKey, Guid parentId, DateTime archiverTimeStart, DateTime archiverTimeEnd);
        Tuple<IEnumerable<FileSystemTableEntity>, string> GetDestroyItemForFSDesctruntionByConnectionIdByPage(string fsAzureTableConnectStr, string tenantGroupId, string partitionKey, int pageSize, string continuationToken);
        List<ArchiverTableEntity> GetWaitingApprovalDatas(AzureTableConnectContract connectionInfo, string tenantGroupId, SourceFlag source);
        List<ArchiverExchangeOnlineDto> GetWaitingApprovalDatasForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId);
        List<FileSystemTableEntity> GetWaitingApprovalDatasForFS(string fsAzureTableConnectStr, string tenantGroupId);
        List<OnPremiseSPTableEntity> GetWaitingApprovalDatasForSPOnPrem(string connectString, string tenantGroupId);
        List<ArchiverTableEntity> GetDestroyedItemsByListId(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid listId, DateTime startTime, DateTime endTime, bool isPhysicalLibrary);
        List<ArchiverTableEntity> GetDestroyedItemsByListIdForOneDrive(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid listId, DateTime startTime, DateTime endTime);
        Task<Dictionary<string, long>> GetDestroyedRecordsAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, Dictionary<Guid, string> physicalListIds, List<string> siteCollectionIds);
        //bool CheckArchiveTableExist(AzureTableConnectContract connectionInfo, string tenantGroupId);
        List<ArchiverExchangeOnlineDto> GetDeletedItemsByMailBoxId(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, string mailBoxId, DateTime startTime, DateTime endTime);
        Task UpdateItemsToExportedStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string sitePath, List<string> itemRowKeys);
        #region OnPremise SP
        OnPremiseSPTableEntity GetArchiverItemForSPOnPrem(string connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey);
        IEnumerable<OnPremiseSPTableEntity> AddArchiverItemsForOnPremiseSP(string connectionInfo, string tenantGroupId, List<OnPremiseSPTableEntity> entities);
        int AddRejectItemsToStaticTableForOnPremiseSP(string connectString, string tenantGroupId, List<OnPremiseSPTableEntity> entities);
        IEnumerable<OnPremiseSPTableEntity> UpdateArchiverItemsForOnPremiseSP(string connectionInfo, string tenantGroupId, List<OnPremiseSPTableEntity> entities);
        IEnumerable<OnPremiseSPTableEntity> GetAzureDataByListForOnPremiseSP(string connectionInfo, string tenantGroupId, string listId, string scopeId, long sortTicks, int pageSize);
        int MoveRecordsToStaticForOnPremiseSP(string connectString, string tenantGroupId, string scopeId);
        #endregion
    }
}
