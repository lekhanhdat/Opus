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
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.ClientLibrary.SDK.Services
{
    public interface IRecordsJobService
    {
        [Api(Url = "api/jobwebapi/getjobmessage", HttpMethod = "POST")]
        Task<string> GetJobMessage(JobInfo jobInfo);
        [Api(Url = "api/jobwebapi/getretentionunit", HttpMethod = "POST")]
        Task<string> GetRetentionUnit(ApplyClassCodeDto jobInfo);

        [Api(Url = "api/jobwebapi/GetDisposalJobMessage", HttpMethod = "POST")]
        Task<string> GetDisposalJobMessage(JobInfo jobInfo);
        [Api(Url = "api/jobwebapi/GetDisposalByClassCodeJobMessage", HttpMethod = "POST")]
        Task<string> GetDisposalByClassCodeJobMessage(JobInfo jobInfo);
        [Api(Url = "api/jobwebapi/GetFSRestoreJobMessage", HttpMethod = "POST")]
        Task<string> GetFSRestoreJobMessage(JobInfo jobInfo);
        [Api(Url = "api/jobwebapi/GetFSRetainJobMessage", HttpMethod = "POST")]
        Task<string> GetFSRetainJobMessage(JobInfo jobInfo);
        [Api(Url = "api/jobwebapi/GetFSDiscoveryJobMessage", HttpMethod = "POST")]
        Task<string> GetFSDiscoveryJobMessage(JobInfo jobInfo);

        [Api(Url = "api/jobwebapi/LoadFSNodeEnableRecordManagement", HttpMethod = "POST")]
        Task<bool> LoadFSNodeEnableRecordManagement(Guid nodeId);
        [Api(Url = "api/jobwebapi/ValidateEnableRecordManagementNodes", HttpMethod = "POST")]
        Task<List<Guid>> ValidateEnableRecordManagementNodes(List<Guid> nodeIds);

        [Api(Url = "api/jobwebapi/updatejobtime", HttpMethod = "POST")]
        Task<bool> UpdateJobTime(RMFileSystemJobTimeReferenceDto dto);

        [Api(Url = "api/jobwebapi/resetapplyexistingoption", HttpMethod = "POST")]
        Task<bool> ResetApplyExistingOption(string scopeId);

        [Api(Url = "api/FSScanData/syncdata", HttpMethod = "POST")]
        Task<AgentSyncDataResultDto> SyncData(List<FileSystemRecordDto> dtos);

        [Api(Url = "api/FSScanData/updatefoldersizes", HttpMethod = "POST")]
        Task<bool> UpdateFolderSizes(List<FolderSizeUpdateDto> batch);

        [Api(Url = "api/FSScanData/getuniqueidsetting", HttpMethod = "GET")]
        Task<FileSystemUniqueIdDto> GetUniqueIdSetting();        
        
        [Api(Url = "api/FSScanData/getuniqueidlist", HttpMethod = "POST")]
        Task<List<long>> GetUniqueIdList(long needCreateCount);

        [Api(Url = "api/FSScanData/deletemoveditem", HttpMethod = "POST")]
        Task DeleteMovedItem(FileSystemRecordDto record);
        
        [Api(Url = "api/FSScanData/deletemoveditems", HttpMethod = "POST")]
        Task DeleteMovedItems(List<FsRecordProcessDto> records);

        [Api(Url = "api/FSScanData/SyncMoveToData", HttpMethod = "POST")]
        Task<AgentSyncDataResultDto> SyncMovedData(List<FileSystemRecordDto> dtos);

        [Api(Url = "api/FSScanData/FindRecords", HttpMethod = "POST")]
        Task<string> GetRecords(List<Guid> ids);

        [Api(Url = "api/fsscandata/FindFSDueRecords", HttpMethod = "POST")]
        Task<FSDueRecordsDto> GetFSDueRecords(SearchFilterParam searchFilterParam);

        [Api(Url = "api/fsscandata/addscandata", HttpMethod = "POST")]
        Task<List<Guid>> AddScanData(List<FSAzureTableEntityDto> dtos);
        [Api(Url = "api/fsscandata/addscandatatocosmos", HttpMethod = "POST")]
        Task<List<Guid>> AddScanDataToCosmos(FSAzureTableEntityDtoWithJobId dto);

        [Api(Url = "api/fsscandata/RemoveManualData", HttpMethod = "POST")]
        Task<List<Guid>> RemoveManualData(List<FSAzureTableEntityDto> dtos);

        [Api(Url = "api/fsscandata/SyncRejectData", HttpMethod = "POST")]
        Task<List<Guid>> AddRejectScanData(List<FSAzureTableEntityDto> entities);

        [Api(Url = "api/fsscanData/getscandatabypage", HttpMethod = "GET")]
        Task<List<FSAzureTableEntityDto>> GetScanDataByPage(string jobId, int pageIndex, int pageSize);

        [Api(Url = "api/fsscanData/getruleidsbyjob", HttpMethod = "GET")]
        Task<List<Guid>> GetRuleIdsByJob(string jobId);

        [Api(Url = "api/fsscanData/getexplorerdatabyfolder", HttpMethod = "GET")]
        Task<List<FSFolderCacheDto>> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize);

        [Api(Url = "api/fsscanData/GetDBRecordsByFolder", HttpMethod = "GET")]
        Task<List<FileSystemRecordDto>> GetDBRecordsByFolder(string folderId, string scopeId, long sortTicks, int pageSize);

        [Api(Url = "api/fsscanData/GetDBRecordsByFolderAndFilterByEndTime", HttpMethod = "GET")]
        Task<List<FileSystemRecordDto>> GetDBRecordsByFolderAndFilterByEndTime(string folderId, string scopeId, long sortTicks, int pageSize);

        [Api(Url = "api/fsscanData/GetDBRecordsByNodeIds", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> GetDBRecordsByNodeIds(RMAzureRecordParamsDto param);

        [Api(Url = "api/fsscanData/GetDBRecordsByNodeIdsAndFilterByEndTime", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> GetDBRecordsByNodeIdsAndFilterByEndTime(RMAzureRecordParamsDto param);
        [Api(Url = "api/fsscanData/GetDBRecordsByClassCodeAndFilterByEndTime", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> GetDBRecordsByClassCodeAndFilterByEndTime(RMAzureRecordByClassCodeParamsDto param);

        [Api(Url = "api/fsscanData/FindSyncFailedItems", HttpMethod = "POST")]
        Task<List<RMAgentSyncFailureItem>> FindSyncFailedItems(RMSyncFailedScopeDto dto);

        [Api(Url = "api/fsscanData/AddSyncFailedItems", HttpMethod = "POST")]
        Task<bool> AddSyncFailedItems(List<RMAgentSyncFailureItem> failedItems);

        [Api(Url = "api/fsscanData/RemoveSuccessItemsInAzure", HttpMethod = "POST")]
        Task<bool> RemoveSuccessItemsInAzure(List<RMAgentSyncFailureItem> successItems);

        [Api(Url = "api/fsscanData/LoadFSDBRecords", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> GetFSDBRecords(List<Guid> ids);

        [Api(Url = "api/fsscanData/LoadFSDBRecordsByRecordsId", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> GetFSDBRecordsByRecordsId(List<string> ids);

        [Api(Url = "api/fsscanData/LoadFSManualRecords", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> GetFSManualRecords(List<Guid> ids); 

        [Api(Url = "api/fsscanData/GetFoldersWithDifferentTermFromParent", HttpMethod = "GET")]
        Task<List<FSFolderCacheDto>> GetFSFolderWithoutInheritTerm(string folderId, string termId);

        [Api(Url = "api/fsscanData/GetCurrentConnectionAllSettings", HttpMethod = "GET")]
        Task<List<FSFolderCacheDto>> GetCurrentConnectionAllSettings(string connectionPath);

        [Api(Url = "api/fsscanData/getazuredatabyfolder", HttpMethod = "GET")]
        Task<List<FSFolderCacheDto>> GetAzureDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize);

        [Api(Url = "api/fsscanData/UpdateRecordsInExplorer", HttpMethod = "POST")]
        Task<List<Guid>> DeleteRecordsInExplorer(List<FSExplorerDeleteDto> dtos);
        [Api(Url = "api/fsscanData/RunSendEmailJob", HttpMethod = "POST")]
        Task RunSendEmailJob(string jobId);

        [Api(Url = "api/fsscanData/MoveItemsToStatic", HttpMethod = "POST")]
        Task<bool> DeleteAndMoveItemsInScope(FSAzureTableRequestInfo info);

        [Api(Url = "api/OnPremiseQuerySPData/GetOnPremiseSPAzureDataByListId", HttpMethod = "GET")]
        Task<List<OnPremiseSPListCacheDto>> GetOnPremiseSPAzureDataByListId(string listId, string scopeId, long sortTicks, int pageSize);

        [Api(Url = "api/OnPremiseQuerySPData/GetOnPremiseSPExplorerDataByListId", HttpMethod = "GET")]
        Task<List<OnPremiseSPListCacheDto>> GetOnPremiseSPExplorerDataByListId(string listId, string scopeId, long sortTicks, int pageSize);

        [Api(Url = "api/OnPremiseQuerySPData/AddOnpremiseSPManualDataToAzureTable", HttpMethod = "POST")]
        Task<List<Guid>> AddOnpremiseSPManualDataToAzureTable(List<OnPremiseSPAzureTableEntityDto> dtos);

        [Api(Url = "api/OnPremiseQuerySPData/AddRejectItemsToStaticTableForOnPremiseSP", HttpMethod = "POST")]
        Task<int> AddRejectItemsToStaticTableForOnPremiseSP(List<OnPremiseSPAzureTableEntityDto> dtos);

        [Api(Url = "api/OnPremiseQuerySPData/UpdateAzureTableOnpremiseSPManualItem", HttpMethod = "POST")]
        Task<List<Guid>> UpdateAzureTableOnpremiseSPManualItem(List<OnPremiseSPAzureTableEntityDto> dtos);

        [Api(Url = "api/OnPremiseQuerySPData/MoveOnPremiseSPItemsToStatic", HttpMethod = "POST")]
        Task<int> MoveOnPremiseSPItemsToStatic(string scopeId);

        [Api(Url = "api/OnPremiseQuerySPData/OnPremiseSPUpdateRecordsInExplorer", HttpMethod = "POST")]
        Task<List<Guid>> OnPremiseSPUpdateRecordsInExplorer(List<OnPremiseSPAzureTableEntityDto> dtos);
        [Api(Url = "api/OnPremiseQuerySPData/DeleteRelatedPhysicalRecord", HttpMethod = "POST")]
        Task<List<OnPremRelatedResult>> DeleteRelatedPhysicalRecord(OnPremRelatedDto dto);
        [Api(Url = "api/OnPremiseQuerySPData/CheckIsHoldRecord", HttpMethod = "POST")]
        Task<bool> CheckIsHoldRecord(string id);

        [Api(Url = "api/FSBatchDataUpload/StartQueueListener", HttpMethod = "POST")]
        Task<bool> StartQueueListener(JobInfo jobInfo);

        [Api(Url = "api/FSBatchDataUpload/GetBlobSasUri", HttpMethod = "GET")]
        Task<string> GetBlobSasUri(string jobId, string blobName); // SAS URI of Blob for batch data upload

        [Api(Url = "api/FSBatchDataUpload/NotifyUploadComplete", HttpMethod = "POST")]
        Task<string> NotifyUploadComplete(FSBatchUploadNotification notification); // Message ID for batch processing

        [Api(Url = "api/FSBatchDataUpload/GetBatchReportResponse", HttpMethod = "GET")]
        Task<FSBatchReportTableEntityDto> GetBatchReportResponse(string jobId, string messageId); // Batch report record

        [Api(Url = "api/FSBatchDataUpload/DisposeQueueListener", HttpMethod = "POST")]
        Task<bool> DisposeQueueListener(string jobId);

        #region JPMC
        [Api(Url = "api/fsscanData/QueryFileSystemRecords", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> QueryFileSystemRecords(FSQueryRecordRequestDto requestDto);

        [Api(Url = "api/fsscanData/QueryFileSystemRecordsByRecordsId", HttpMethod = "POST")]
        Task<List<FsRecordProcessDto>> QueryFileSystemRecordsByRecordsId(FSQueryRecordRequestDto requestDto);
        #endregion
    }
}
