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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.UniqueId;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/FSScanData/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [RMAgentApiPerformanceLogger]
    public class FSScanDataController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(FSScanDataController));

        private IArchiverTableDao _ArchiverTableDao;

        private IArchiverTableDao ArchiverTableDao => PlatformWindsorManager.GetService(ref _ArchiverTableDao);

        private IExplorerService _ExplorerService;

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

        private ISyncFailureItemDao _SyncFailureItemDao;

        private ISyncFailureItemDao SyncFailureItemDao => PlatformWindsorManager.GetService(ref _SyncFailureItemDao);

        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();

        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();

        [HttpPost]
        public List<Guid> AddScanData([FromBody] List<FSAzureTableEntityDto> dtos)
        {
            using (new PerformanceScope("FSScanData.AddScanData"))
            {
                List<Guid> failedGuids = new List<Guid>();
                try
                {
                    string mTenantGroupId = TenantLocalValue.LogonGroupId;
                    List<FileSystemTableEntity> entities = new List<FileSystemTableEntity>();
                    foreach (var dto in dtos)
                    {
                        entities.Add(ConvertUtil.ConvertFSDto2ArchiverTableEntity(dto));
                    }
                    ArchiverTableDao.AddArchiverItemsForFS(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, entities);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while add archiver data. Error:" + e.ToString());
                    failedGuids = dtos.Select(d => d.FilePathMd5).ToList();
                }
                return failedGuids;
            }
        }

        [HttpPost]
        public List<Guid> AddScanDataToCosmos([FromBody] FSAzureTableEntityDtoWithJobId dto)
        {
            using (new PerformanceScope("FSScanData.AddScanData"))
            {
                List<Guid> failedGuids = new List<Guid>();
                try
                {
                    string mTenantGroupId = TenantLocalValue.LogonGroupId;
                    ExplorerService.AddArchiverItemsForFSAsync(mTenantGroupId, dto.EntityDtos, dto.JobId, dto.IsFSHighPerformanceMode).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while add archiver data. Error:" + e.ToString());
                    failedGuids = dto.EntityDtos.Select(d => d.FilePathMd5).ToList();
                }
                return failedGuids;
            }
        }
        [HttpPost]
        public void RunSendEmailJob([FromBody] string jobId)
        {
            ExplorerService.RunSendEmailJobAsync(jobId);
        }
        [HttpPost]
        public List<Guid> RemoveManualData([FromBody] List<FSAzureTableEntityDto> dtos)
        {
            using (new PerformanceScope("FSScanData.RemoveManualData"))
            {
                List<Guid> failedGuids = new List<Guid>();
                try
                {
                    string mTenantGroupId = TenantLocalValue.LogonGroupId;
                    List<FileSystemTableEntity> entities = new List<FileSystemTableEntity>();
                    foreach (var dto in dtos)
                    {
                        entities.Add(ConvertUtil.ConvertFSDto2ArchiverTableEntity(dto));
                    }
                    ArchiverTableDao.RemoveArchiverItemsForFS(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, entities);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while remove manual data. Error:" + e.ToString());
                    failedGuids = dtos.Select(d => d.FilePathMd5).ToList();
                }

                //clear manully property
                try
                {
                    var nodeIds = dtos.Select(n => n.FilePathMd5).ToList();
                    var explorerDao = new ExplorerDao(true);
                    explorerDao.UpdateAll(r => nodeIds.Contains(r.NodeId), r =>
                    {
                        r.IsManualSynced = false;
                        r.ManualActionTime = 0;
                        r.ManualApprovedBy = 0;
                        r.ManualApprovedStatus = 0;
                        r.ManualArchivedTime = 0;
                        r.ManualArchiveStatus = 0;
                        r.ManualAudits = string.Empty;
                        r.ManualCollectionTime = 0;
                        r.ManualEmailNotificationCount = 0;
                        r.ManualEmailNotificationLastTime = 0;
                        r.ManualEscalatedComment = string.Empty;
                        r.ManualEscalateFrom = 0;
                        r.ManualExtendComment = string.Empty;
                        r.ManualExtendCount = 0;
                        r.ManualExtendTime = 0;
                        r.ManualFullPath = string.Empty;
                        r.ManualFolderPath = string.Empty;
                        r.ManualLastReasonForRejection = string.Empty;
                        r.ManualInternalApprovedStatus = 0;
                        r.ManualIsAutoReassigned = false;
                        r.ManualIsRelatedRecords = false;
                        r.ManualNeedEmailNotification = false;
                        r.ManualPartitionKey = string.Empty;
                        r.ManualRelatedRecords = string.Empty;
                        r.ManualRelatedRecordsAction = 0;
                        r.ManualRetentionStatus = 0;
                        r.ManualReviewer = null;
                        r.ManualRowKey = string.Empty;
                        r.ManualRuleCriteria = string.Empty;
                        r.ManualRuleDisposalClass = string.Empty;
                        r.ManualRuleName = string.Empty;
                        r.ManualVersion = string.Empty;
                        r.ManualWorkflowDefinitionId = Guid.Empty;
                        r.ManualWorkflowInstanceId = Guid.Empty;
                        r.ManualWorkflowStepId = Guid.Empty;
                        r.ManualModifiedTime = 0;
                        r.RuleId = Guid.Empty;
                    });
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while remove manual property. Error:" + e.ToString());
                }
                return failedGuids;
            }
        }

        [HttpGet]
        public List<FSFolderCacheDto> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            try
            {
                return ExplorerService.GetExplorerDataByFolder(folderId, scopeId, sortTicks, pageSize);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting explorer data. Folder Id:{0} Error:{1}", folderId, e.ToString());
                return null;
            }
        }

        [HttpGet]
        public List<FSFolderCacheDto> GetFoldersWithDifferentTermFromParent(string folderId, string termId)
        {
            try
            {
                return ExplorerService.GetDifferentTermDBRecordsByFolder(folderId, termId);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting explorer data. Folder Id:{0} Error:{1}", folderId, e.ToString());
                return null;
            }
        }

        [HttpGet]
        public List<FSFolderCacheDto> GetCurrentConnectionAllSettings(string connectionPath)
        {
            try
            {
                return FileSystemSettingDao.LoadAllSettingsUnderConnection(connectionPath).ConvertAll(item => new FSFolderCacheDto
                {
                    Id = item.ScopeId,
                    TermId = item.DefaultTermId,
                    TermName = item.DefaultTermName,
                    IsActive = item.IsActive,
                });
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while getting explorer data. {e}");
                return null;
            }
        }

        [HttpGet]
        public List<FileSystemRecordDto> GetDBRecordsByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            try
            {
                return ExplorerService.GetDBRecordsByFolder(folderId, scopeId, sortTicks, pageSize);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting explorer data. Folder Id:{0} Error:{1}", folderId, e.ToString());
                return null;
            }
        }

        [HttpGet]
        public List<FileSystemRecordDto> GetDBRecordsByFolderAndFilterByEndTime(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            try
            {
                return ExplorerService.GetDBRecordsByFolderAndEndTime(folderId, scopeId, sortTicks, pageSize);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting explorer data. Folder Id:{0} Error:{1}", folderId, e.ToString());
                return null;
            }
        }

        [HttpPost]
        public List<FileSystemRecordDto> GetDBRecordsByNodeIds([FromBody] RMAzureRecordParamsDto param)
        {
            try
            {
                return ExplorerService.GetDBRecordsByNodeIds(param.NodeIds, param.ScopeId, param.SortTicks);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting explorer data. Error:{0}", e.ToString());
                return null;
            }
        }
        [HttpPost]
        public List<FileSystemRecordDto> GetDBRecordsByNodeIdsAndFilterByEndTime([FromBody] RMAzureRecordParamsDto param)
        {
            try
            {
                return ExplorerService.GetDBRecordsByNodeIdsAndEndTime(param.NodeIds, param.ScopeId, param.SortTicks);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting explorer data. Error:{0}", e.ToString());
                return null;
            }
        }
        [HttpPost]
        public List<FileSystemRecordDto> GetDBRecordsByClassCodeAndFilterByEndTime([FromBody] RMAzureRecordByClassCodeParamsDto param)
        {
            try
            {
                return ExplorerService.GetDBRecordsByClassCodeAndFilterByEndTime(param.NodeIds, param.ClassCodeIds, param.ScopeId, param.SortTicks);
            }
            catch (Exception e)
            {
                logger.Error(
                    "An error occurred while getting records by class code and end time. Error:{0}",
                    e.ToString());
                return null;
            }
        }
        [HttpPost]
        public List<FileSystemRecordDto> LoadFSDBRecords([FromBody] List<Guid> ids)
        {
            try
            {
                return ExplorerService.GetFSDBRecords(ids);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while GetFSDBRecords. Error:{0}", e.ToString());
                return new List<FileSystemRecordDto>();
            }
        }

        [HttpPost]
        public List<FileSystemRecordDto> QueryFileSystemRecords([FromBody]FSQueryRecordRequestDto requestDto)
        {
            try
            {
                var recordGuidIds = new List<Guid>();
                foreach (var id in requestDto.RecordIds)
                {
                    if (Guid.TryParse(id, out var guid)) 
                        recordGuidIds.Add(guid);
                }
                return ExplorerService.QueryFileSystemRecords(requestDto.ConnectionId, recordGuidIds);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while QueryFileSystemRecords. ConnectionId:{0} Error:{1}", requestDto.ConnectionId, e.ToString());
                return new List<FileSystemRecordDto>();
            }
        }

        [HttpPost]
        public List<FsRecordProcessDto> QueryFileSystemRecordsByRecordsId([FromBody] FSQueryRecordRequestDto requestDto)
        {
            try
            {
                return ExplorerService.QueryFileSystemRecords(requestDto.ConnectionId, requestDto.RecordIds);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while QueryFileSystemRecords. ConnectionId:{0} Error:{1}", requestDto.ConnectionId, e.ToString());
                return new List<FsRecordProcessDto>();
            }
        }

        [HttpPost]
        public List<FileSystemRecordDto> LoadFSDBRecordsByRecordsId([FromBody] List<string> ids)
        {
            try
            {
                return ExplorerService.GetFSDBRecordsByRecordsId(ids);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while GetFSDBRecords. Error:{0}", e.ToString());
                return new List<FileSystemRecordDto>();
            }
        }


        [HttpPost]
        public List<FileSystemRecordDto> LoadFSManualRecords([FromBody] List<Guid> ids)
        {
            try
            {
                return ExplorerService.GetFSManualRecords(ids);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while LoadFSManualRecords. Error:{0}", e.ToString());
                return new List<FileSystemRecordDto>();
            }
        }

        [HttpGet]
        public List<FSFolderCacheDto> GetAzureDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            List<FSFolderCacheDto> dtos = new List<FSFolderCacheDto>();
            try
            {
                string mTenantGroupId = TenantLocalValue.LogonGroupId;
                ArchiverTableDao.GetAzureDataByFolderForFS(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, folderId, scopeId, sortTicks, pageSize).ToList().ForEach(r =>
                {
                    dtos.Add(ConvertUtil.ConvertAzureData2FSFolderCacheDto(r));
                });
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting data from azure table. Folder Id:{0} Error:{1}", folderId, e.ToString());
            }
            return dtos;
        }

        [HttpPost]
        public List<Guid> UpdateRecordsInExplorer([FromBody] List<FSExplorerDeleteDto> dtos)
        {
            using (new PerformanceScope("FSScanData--UpdateRecordsInExplorer"))
            {
                return ExplorerService.UpdateFSDeleteRecord(dtos);
            }
        }

        [HttpPost]
        public int MoveItemsToStatic([FromBody] FSAzureTableRequestInfo info)
        {
            using (new PerformanceScope("FSScanData.MoveItemsToStatic"))
            {
                //string mTenantGroupId = TenantLocalValue.LogonGroupId;
                //return ArchiverTableDao.MoveRecordsToStaticForConnectionForFS(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, info.ConnectionPath, info.ScopeId);
                return 0;
            }
        }

        [HttpGet]
        public FileSystemUniqueIdDto GetUniqueIdSetting()
        {
            UniqueIdUtil idUtil = new UniqueIdUtil();
            return idUtil.GetFileSystemUniqueSetting();
        }

        [HttpPost]
        public List<long> GetUniqueIdList([FromBody] long range)
        {
            UniqueIdUtil idUtil = new UniqueIdUtil();
            return idUtil.GetFSUniqueIdList(TenantLocalValue.LogonGroupId, range);
        }

        [HttpPost]
        public void DeleteMovedItem([FromBody]FileSystemRecordDto record)
        {
            try
            {
                IExplorerDao _explorerDao = new ExplorerDao(true);
                logger.Info($"remove moved manual data:{record.NodeId}");
                _explorerDao.Delete(record.CreateDate, record.NodeId);
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while remove old manual data, ERROR: {ex}");
            }
        }
        
        [HttpPost]
        public async Task DeleteMovedItems([FromBody] List<FsRecordProcessDto> records)
        {
            if (records == null || records.Count == 0) return;
            try
            {
                IExplorerDao explorerDao = new ExplorerDao(true);
                foreach (var record in records)
                {
                    try
                    {
                        logger.Info($"Removing moved data. NodeId: {record.NodeId}");
                        explorerDao.Delete(record.CreateDate, record.NodeId);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to remove moved data. NodeId: {record.NodeId}, ERROR: {ex}");
                    }
                }
                await FSAuditSinkService.FlushAsync(records).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error($"Unexpected error while removing moved data. ERROR: {ex}");
            }
        }

        [HttpPost]
        public AgentSyncDataResultDto SyncData([FromBody] List<FileSystemRecordDto> dtos)
        {
            var folderTermInfo = new Dictionary<Guid, (Guid termId, string termName)>();
            var folderSettingInfo = dtos.Where(a => a.FSSettingDto != null).ToDictionary(a => a.NodeId, a => a.FSSettingDto);
            List<FileSystemRecordDto> bulkDtos = dtos.Where(a => a.NodeType != (int)NodeLevel.FSFolder).ToList();
            List<FileSystemRecordDto> singularDtos = dtos.Where(a => a.NodeType == (int)NodeLevel.FSFolder).ToList();
            logger.Info($"Folders count {singularDtos.Count},  file and others count {bulkDtos.Count}, fsjobtype {dtos[0].FSJobType}");

            List<Guid> failedDataGuid = new List<Guid>();
            List<Guid> skippedGuid = new List<Guid>();
            RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
            using (new PerformanceScope("FSScanData--SyncData"))
            {
                try
                {
                    bool isCosmosBulkOperationEnabled = dtos[0].BulkImportEnabled;
                    FSJobType fsJobType = dtos[0].FSJobType;
                    if (fsJobType == FSJobType.UserFullJob)
                    {
                        bulkDtos = dtos;
                        singularDtos.Clear();
                    }
                    _explorerDao = new ExplorerDao(true);
                    var needCreateIdCount = dtos.Where(d => string.IsNullOrWhiteSpace(d.RecordsId) && (d.NodeType == (int)NodeLevel.FSFile || d.NodeType == (int)NodeLevel.FSFolder)).ToList().Count;
                    UniqueIdUtil idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, needCreateIdCount);
                    if (isCosmosBulkOperationEnabled && bulkDtos.Count > 0)
                    {
                        var bulkSize = bulkDtos[0].BulkSize;
                        if (bulkSize == default(int))
                        {
                            bulkSize = CosmosBulkOperator.DefualtBufferSize;
                        }
                        logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                        List<Record> records = new List<Record>();
                        foreach (var dto in bulkDtos)
                        {
                            if (string.IsNullOrWhiteSpace(dto.RecordsId) && (dto.NodeType == (int)NodeLevel.FSFile || dto.NodeType == (int)NodeLevel.FSFolder))
                            {
                                dto.RecordsId = idUtil.GenerateUniqueId();
                            }
                            Record record = ConvertUtil.ConvertFSDtoToRMBaseRecord(dto);
                            if (record.NodeType == (int)NodeLevel.FSConnectionGroups || record.NodeType == (int)NodeLevel.FSConnectionGroup)
                            {
                                //解决已知问题，Group and Groups level 每跑一次job就会多一条记录
                                var temp = _explorerDao.GetFSRecordById(record.Id);
                                if (temp != null)
                                {
                                    record.TimeCreated = temp.TimeCreated;
                                }
                            }

                            record.AppendCustomColumns();                
                            records.Add(record);
                        }
                        failedDataGuid = _explorerDao.BatchUpdate(records, bulkSize);
                        var duplicateRecords = bulkDtos.Where(r => !failedDataGuid.Contains(r.NodeId) && r.hasDuplicated);
                        foreach (var item in duplicateRecords)
                        {
                            ProcessManualDuplicateData(_explorerDao, item.NodeId);
                        }
                    }
                    else
                    {
                        foreach (var dto in bulkDtos)
                        {
                            try
                            {
                                if (string.IsNullOrWhiteSpace(dto.RecordsId) && (dto.NodeType == (int)NodeLevel.FSFile || dto.NodeType == (int)NodeLevel.FSFolder))
                                {
                                    dto.RecordsId = idUtil.GenerateUniqueId();
                                }
                                var returnMessage = ExplorerService.AddOrUpdateFileSystemObject(dto);
                                if (returnMessage.MessageType == RAMessageType.Failed)
                                {
                                    failedDataGuid.Add(dto.NodeId);
                                }
                                else if (returnMessage.MessageType == RAMessageType.Exception)
                                {
                                    skippedGuid.Add(dto.NodeId);
                                }

                                if (returnMessage.MessageType != RAMessageType.Failed && dto.hasDuplicated)
                                {
                                    ProcessManualDuplicateData(_explorerDao, dto.NodeId);
                                }
                            }
                            catch (Exception)
                            {
                                if (!failedDataGuid.Contains(dto.NodeId))
                                {
                                    failedDataGuid.Add(dto.NodeId);
                                }
                            }
                        }
                    }
                    ///为解决Sync覆盖数据的问题， Folder Level只能用串行模式更新 RECO-15067
                    foreach (var dto in singularDtos)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(dto.RecordsId) && (dto.NodeType == (int)NodeLevel.FSFile || dto.NodeType == (int)NodeLevel.FSFolder))
                            {
                                dto.RecordsId = idUtil.GenerateUniqueId();
                            }
                            Record record = ConvertUtil.ConvertFSDtoToRMBaseRecord(dto);
                            record.AppendCustomColumns();
                            Record dbRecord = null;
                            using (var scope = new PerformanceScope("AddOrUpdateFileSystemObject.ReadById"))
                            {
                                dbRecord = _explorerDao.ReadById(record.ScopeId, record.Id);
                            }
                            if (dbRecord == null)
                            {
                                using (var scope = new PerformanceScope("AddOrUpdateFileSystemObject.AddFileSystemRecord"))
                                {
                                    if (folderSettingInfo.TryGetValue(dto.ParentId, out var parentSetting))
                                    {
                                        if (parentSetting != null && !parentSetting.NeedCheckDefaultValue)
                                        {
                                            if (folderTermInfo.TryGetValue(dto.ParentId, out var parentTermInfo))
                                            {
                                                record.TermId = parentTermInfo.termId;
                                                record.TermName = parentTermInfo.termName;
                                            }
                                            else
                                            {
                                                var parentRecord = _explorerDao.GetFSRecordById(dto.ParentId);
                                                record.TermId = parentRecord.TermId;
                                                record.TermName = parentRecord.TermName;
                                                folderTermInfo.TryAdd(dto.ParentId, (parentRecord.TermId, parentRecord.TermName));
                                            }
                                        }
                                    }
                                    _explorerDao.AddFileSystemRecord(record);
                                }
                            }
                            else
                            {
                                using (var scope = new PerformanceScope("AddOrUpdateFileSystemObject.UpdateFileSystemRecord"))
                                {
                                    record.TermId = dbRecord.TermId;
                                    record.TermName = dbRecord.TermName;
                                    _explorerDao.UpdateFileSystemFolderForSync(record);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (!failedDataGuid.Contains(dto.NodeId))
                            {
                                failedDataGuid.Add(dto.NodeId);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while syncing data. Error:{e.ToString()}");
                    return new AgentSyncDataResultDto() { FailedGuids = bulkDtos.Select(r => r.NodeId).ToList(), SkippedGuids = skippedGuid };
                }
            }
            return new AgentSyncDataResultDto() { FailedGuids = failedDataGuid, SkippedGuids = skippedGuid };
        }

        private void ProcessManualDuplicateData(RA.DB.Explorer.Dao.IExplorerDao _explorerDao, Guid id)
        {
            //先运行manual review job scan进来的数据createdate为0, 需要remove.
            try
            {
                logger.Info($"remove old manual data:{id}");
                _explorerDao.Delete(0, id);
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while remove old manual data, ERROR: {ex.ToString()}");
            }

        }

        [HttpPost]
        public AgentSyncDataResultDto SyncMoveToData([FromBody] List<FileSystemRecordDto> dtos)
        {
            List<Guid> failedDataGuid = new List<Guid>();
            List<Guid> skippedGuid = new List<Guid>();
            RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
            using (new PerformanceScope("FSScanData--SyncMoveToData"))
            {
                try
                {
                    bool isCosmosBulkOperationEnabled = dtos[0].BulkImportEnabled;
                    _explorerDao = new ExplorerDao(true);
                    var needCreateIdCount = dtos.Where(d => string.IsNullOrWhiteSpace(d.RecordsId) && (d.NodeType == (int)NodeLevel.FSFile || d.NodeType == (int)NodeLevel.FSFolder)).ToList().Count;
                    UniqueIdUtil idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, needCreateIdCount);
                    if (isCosmosBulkOperationEnabled)
                    {
                        var bulkSize = dtos[0].BulkSize;
                        if (bulkSize == default(int))
                        {
                            bulkSize = CosmosBulkOperator.DefualtBufferSize;
                        }
                        var ids = dtos.Select(r => r.NodeId).ToList();
                        List<Record> dbRecords;
                        using (new PerformanceScope("FSScanData.GetDBRecords"))
                        {
                            dbRecords = _explorerDao.QueryAll(r => ids.Contains(r.Id)).ToList();
                        }
                        logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                        List<Record> records = new List<Record>();
                        foreach (var dto in dtos)
                        {
                            var dbRecord = dbRecords.Where(r => r.NodeId == dto.NodeId).FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(dto.RecordsId) && (dto.NodeType == (int)NodeLevel.FSFile || dto.NodeType == (int)NodeLevel.FSFolder))
                            {
                                dto.RecordsId = idUtil.GenerateUniqueId();
                            }
                            Record record = ConvertUtil.ConvertFSDtoToRMBaseRecord(dto);
                            if (dbRecord != null)
                            {
                                record.CreateDate = dbRecord.CreateDate;
                            }
                            record.AppendCustomColumns();
                            // if (_explorerDao.NeedUpdateRecord(record, false))
                            //{
                            records.Add(record);
                            //}
                        }
                        failedDataGuid = _explorerDao.BatchUpdate(records, bulkSize);
                    }
                    else
                    {
                        foreach (var dto in dtos)
                        {
                            try
                            {
                                if (string.IsNullOrWhiteSpace(dto.RecordsId) && (dto.NodeType == (int)NodeLevel.FSFile || dto.NodeType == (int)NodeLevel.FSFolder))
                                {
                                    dto.RecordsId = idUtil.GenerateUniqueId();
                                }
                                var returnMessage = ExplorerService.AddOrUpdateFileSystemObject(dto);
                                if (returnMessage.MessageType == RAMessageType.Failed)
                                {
                                    failedDataGuid.Add(dto.NodeId);
                                }
                                else if (returnMessage.MessageType == RAMessageType.Exception)
                                {
                                    skippedGuid.Add(dto.NodeId);
                                }
                            }
                            catch (Exception)
                            {
                                if (!failedDataGuid.Contains(dto.NodeId))
                                {
                                    failedDataGuid.Add(dto.NodeId);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while syncing data. Error:{e.ToString()}");
                    return new AgentSyncDataResultDto() { FailedGuids = dtos.Select(r => r.NodeId).ToList(), SkippedGuids = skippedGuid };
                }
            }
            return new AgentSyncDataResultDto() { FailedGuids = failedDataGuid, SkippedGuids = skippedGuid };
        }

        [HttpPost]
        public List<Guid> SyncRejectData([FromBody] List<FSAzureTableEntityDto> dtos)
        {
            using (new AvePoint.RA.Common.PerformanceScope("FSScanData.SyncRejectData"))
            {
                List<Guid> failedDataGuid = new List<Guid>();
                try
                {
                    string mTenantGroupId = TenantLocalValue.LogonGroupId;
                    List<FileSystemTableEntity> entities = new List<FileSystemTableEntity>();
                    foreach (var dto in dtos)
                    {
                        entities.Add(ConvertUtil.ConvertFSDto2ArchiverTableEntity(dto));
                    }
                    failedDataGuid = ArchiverTableDao.AddRejectItemsForFS(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, entities);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while sync reject items. Error:{0}", e.ToString());
                }
                return failedDataGuid;
            }
        }

        [HttpPost]
        public string FindRecords([FromBody] List<Guid> ids)
        {
            var records = ExplorerService.GetFileSystemObjectByGuids(ids);
            return records.Count > 0 ? JsonConvert.SerializeObject(records) : string.Empty;
        }

        [HttpPost]
        public bool AddSyncFailedItems([FromBody] List<RMAgentSyncFailureItem> failedItems)
        {
            bool success = true;
            using (new PerformanceScope("FSScanData.AddSyncFailedItems"))
            {
                List<SyncFailureItemEntity> failureEntities = new List<SyncFailureItemEntity>();
                try
                {
                    foreach (var item in failedItems)
                    {
                        SyncFailureItemEntity entity = new SyncFailureItemEntity(item.SiteId, item.ItemId);
                        entity.DataSource = item.SourceFlag;
                        entity.JobId = item.JobId;
                        entity.FullPath = item.URL;
                        entity.NodeId = item.NodeId;
                        entity.SortTicks = item.SortTicks;
                        entity.WebId = item.WebId;
                        entity.ListId = item.ListId;
                        entity.ItemId = item.IntemIntId;
                        entity.ParentId = item.ParentId;
                        failureEntities.Add(entity);
                    }
                    logger.Debug($"Add entity to azure, list count: {failureEntities.Count}");
                    SyncFailureItemDao.Add(TenantLocalValue.LogonGroupId, failureEntities);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while adding fs sync failed items, error:{0}", e.ToString());
                    success = false;
                }
            }
            return success;
        }

        [HttpPost]
        public bool RemoveSuccessItemsInAzure([FromBody] List<RMAgentSyncFailureItem> failedItems)
        {
            bool success = true;
            using (new PerformanceScope("FSScanData.AddSyncFailedItems"))
            {
                List<SyncFailureItemEntity> failureEntities = new List<SyncFailureItemEntity>();
                try
                {
                    foreach (var item in failedItems)
                    {
                        SyncFailureItemEntity entity = new SyncFailureItemEntity(item.SiteId, item.ItemId);
                        SyncFailureItemDao.Remove(TenantLocalValue.LogonGroupId, entity);
                    }
                    logger.Debug($"Add entity to azure, list count: {failureEntities.Count}");

                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while adding fs sync failed items, error:{0}", e.ToString());
                    success = false;
                }
            }
            return success;
        }

        [HttpPost]
        public List<RMAgentSyncFailureItem> FindSyncFailedItems([FromBody] RMSyncFailedScopeDto queryDto)
        {
            using (new PerformanceScope("FSScanData.FindSyncFailedItems"))
            {
                List<RMAgentSyncFailureItem> failureEntities = new List<RMAgentSyncFailureItem>();
                try
                {
                    var result = SyncFailureItemDao.GetDataByPage(TenantLocalValue.LogonGroupId,
                        queryDto.SiteId, queryDto.DataSource, queryDto.QueryTicks, queryDto.PageSize);

                    result.ForEach(r =>
                    {
                        RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                        {
                            SiteId = r.PartitionKey,
                            URL = r.FullPath,
                            SortTicks = r.SortTicks,
                            NodeId = r.NodeId,
                            ItemId = r.RowKey,
                            ParentId = r.ParentId,
                            IntemIntId = r.ItemId,
                            ListId = r.ListId,
                            JobId = r.JobId,
                            WebId = r.WebId
                        };
                        failureEntities.Add(item);
                    });
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while getting fs sync failed items, error:{0}", e.ToString());
                }
                return failureEntities;
            }
        }

        [HttpPost]
        public FSDueRecordsDto FindFSDueRecords([FromBody] SearchFilterParam searchFilterParam)
        {
            return ExplorerService.GetFSDueRecords(searchFilterParam);
        }

        [HttpPost]
        public async Task<bool> UpdateFolderSizes([FromBody] List<FolderSizeUpdateDto> dtos)
        {
            using (new PerformanceScope("FSScanData--UpdateFolderSizes"))
            {
                return ExplorerService.UpdateFSFolderSize(dtos);
            }
        }
    }
}
