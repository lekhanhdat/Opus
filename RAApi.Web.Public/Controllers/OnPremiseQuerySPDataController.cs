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
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.RACommonUtility.UniqueId;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AvePoint.RA.Common.Global.Utils;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Api.Web.Public.Filters;
using System.Threading.Tasks;
using LiteDB;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.RAEnduserArchive.Imps;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [RMAgentApiPerformanceLogger]
    public class OnPremiseQuerySPDataController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(OnPremiseQuerySPDataController));

        private IArchiverTableDao _ArchiverTableDao;

        private IArchiverTableDao ArchiverTableDao => PlatformWindsorManager.GetService(ref _ArchiverTableDao);

        private IExplorerService _ExplorerService;

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

  



        private IRMManualApproveDao _RMManualApproveDao;

        private IRMManualApproveDao RMManualApproveDao => PlatformWindsorManager.GetService(ref _RMManualApproveDao);







        private IRMScopeDao _RMScopeDao;

        private IRMScopeDao RMScopeDao => PlatformWindsorManager.GetService(ref _RMScopeDao);

        private ISharePointOnPremiseSettingDao _SharePointOnPremiseSettingDao;

        private ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao => PlatformWindsorManager.GetService(ref _SharePointOnPremiseSettingDao);

        private IRMSubJobDao _SubJobDao;

        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService(ref _SubJobDao);

        private IExplorerQueryService _ExplorerQueryService;

        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService(ref _ExplorerQueryService);

        private IRecordsHistoryService _RecordsHistoryService;

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService(ref _RecordsHistoryService);

        private IRMClassificationHistoryDao _ClassificationHistoryDao;

        private IRMClassificationHistoryDao ClassificationHistoryDao => PlatformWindsorManager.GetService(ref _ClassificationHistoryDao);

        private IRMRecordsUpdateTempDao _RMRecordsUpdateTempDao;

        private IRMRecordsUpdateTempDao RMRecordsUpdateTempDao => PlatformWindsorManager.GetService(ref _RMRecordsUpdateTempDao);

        private IRMNodeFlagDao _RMNodeFlagDao;

        private IRMNodeFlagDao RMNodeFlagDao => PlatformWindsorManager.GetService(ref _RMNodeFlagDao);

        private IRMRuleDao _RuleDao;

        private IRMRuleDao RuleDao => PlatformWindsorManager.GetService(ref _RuleDao);

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        #region Disposal Job
        [HttpPost]
        public List<Guid> AddOnpremiseSPManualDataToAzureTable([FromBody] List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            List<Guid> failedGuids = new List<Guid>();
            try
            {
                string mTenantGroupId = TenantLocalValue.LogonGroupId;
                List<OnPremiseSPTableEntity> entities = new List<OnPremiseSPTableEntity>();
                foreach (var dto in dtos)
                {
                    entities.Add(ConvertUtil.ConvertOnPremiseSPDto2ArchiverTableEntity(dto));
                }
                ArchiverTableDao.AddArchiverItemsForOnPremiseSP(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, entities);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while add onpremise archiver data. Error:" + e.ToString());
                failedGuids = dtos.Select(d => d.NodeID).ToList();
            }
            return failedGuids;
        }

        [HttpPost]
        public int AddRejectItemsToStaticTableForOnPremiseSP([FromBody] List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            try
            {
                string mTenantGroupId = TenantLocalValue.LogonGroupId;
                List<OnPremiseSPTableEntity> entities = new List<OnPremiseSPTableEntity>();
                foreach (var dto in dtos)
                {
                    entities.Add(ConvertUtil.ConvertOnPremiseSPDto2ArchiverTableEntity(dto));
                }
                ArchiverTableDao.AddRejectItemsToStaticTableForOnPremiseSP(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, entities);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while AddRejectItemsToStaticTableForOnPremiseSP. Error:" + e.ToString());
            }
            return 0;
        }

        [HttpPost]
        public List<Guid> UpdateAzureTableOnpremiseSPManualItem([FromBody] List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            List<Guid> failedGuids = new List<Guid>();
            try
            {
                string mTenantGroupId = TenantLocalValue.LogonGroupId;
                List<OnPremiseSPTableEntity> entities = new List<OnPremiseSPTableEntity>();
                foreach (var dto in dtos)
                {
                    entities.Add(ConvertUtil.ConvertOnPremiseSPDto2ArchiverTableEntity(dto));
                }
                ArchiverTableDao.UpdateArchiverItemsForOnPremiseSP(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, entities);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while update onpremise archiver data. Error:" + e.ToString());
                failedGuids = dtos.Select(d => d.NodeID).ToList();
            }
            return failedGuids;
        }
        [HttpPost]
        public List<OnPremRelatedResult> DeleteRelatedPhysicalRecord([FromBody] OnPremRelatedDto dto)
        {
            try
            {
                IRelativeDataArchiverService EnduserArchiverAction = new RelativeDataArchiverService();
                return EnduserArchiverAction.DeleteSPOnpremRelatedPhysicalData(ConverRelatedRecordsRule(dto.CurrentRule), dto.RecordRelatedValue, dto.Jobid);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while DeleteRelatedPhysicalRecord. Error:" + e.ToString());
            }
            return new List<OnPremRelatedResult>();
        }
        private AvePoint.GCommon.Contract.StorageOptimization.Object.Rule ConverRelatedRecordsRule(AvePoint.RA.Contract.Global.Object.Rule tempRule)
        {
            return new GCommon.Contract.StorageOptimization.Object.Rule()
            {
                Id = tempRule?.Id,
                Name = tempRule?.Name,
            };
        }

        [HttpGet]
        public List<OnPremiseSPListCacheDto> GetOnPremiseSPAzureDataByListId(string listId, string scopeId, long sortTicks, int pageSize)
        {
            List<OnPremiseSPListCacheDto> dtos = new List<OnPremiseSPListCacheDto>();
            try
            {
                string mTenantGroupId = TenantLocalValue.LogonGroupId;
                ArchiverTableDao.GetAzureDataByListForOnPremiseSP(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, listId, scopeId, sortTicks, pageSize).ToList().ForEach(r =>
                {
                    dtos.Add(ConvertUtil.ConvertAzureData2OnPremiseSPListCacheDto(r));
                });
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting onpremise data from azure table. listId Id:{0} Error:{1}", listId, e.ToString());
            }
            return dtos;
        }

        [HttpGet]
        public List<OnPremiseSPListCacheDto> GetOnPremiseSPExplorerDataByListId(string listId, string scopeId, long sortTicks, int pageSize)
        {
            try
            {
                return ExplorerService.GetOnPremiseSPExplorerDataByListId(listId, scopeId, sortTicks, pageSize);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting explorer data. listId:{0} Error:{1}.", listId, e.ToString());
                return null;
            }
        }

        [HttpPost]
        public int MoveOnPremiseSPItemsToStatic([FromBody] string scopeId)
        {
            string mTenantGroupId = TenantLocalValue.LogonGroupId;
            return ArchiverTableDao.MoveRecordsToStaticForOnPremiseSP(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], mTenantGroupId, scopeId);
        }

        [HttpPost]
        public List<Guid> OnPremiseSPUpdateRecordsInExplorer([FromBody] List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            return ExplorerService.OnPremiseSPUpdateRecordsInExplorer(dtos);
        }
        [HttpPost]
        public bool CheckIsHoldRecord([FromBody] string id)
        {
            return ExplorerService.CheckIsHoldRecord(new Guid(id));
        }
        #endregion

        #region data sync
        [HttpPost]
        public async Task<AgentSyncDataResultDto> AddSPDataToExplorer([FromBody] List<RecordDto> records)
        {
            List<Guid> failedIds = new List<Guid>();
            List<Guid> skipIds = new List<Guid>();
            using (new PerformanceScope("OnPremiseQuerySPData.AddSPDataToExplorer"))
            {
                try
                {
                    bool isCosmosBulkOperationEnabled = records[0].BulkImportEnabled;
                    RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                    var needCreateIdCount = records.Where(d => string.IsNullOrWhiteSpace(d.RecordsId) && d.NodeType == (int)RMNodeLevel.Item).ToList().Count;
                    UniqueIdUtil idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, needCreateIdCount);
                    var recordIds = records.Select(n => n.NodeId).ToList();
                    //List<RMManualApprove> mas = RMManualApproveDao.GetManualApproveByNodes(records.FirstOrDefault().ScopeId, recordIds);
                    //Dictionary<Guid, string> tempMapping = AssembleItemAndOwnerMappingNew(mas, recordIds);
                    if (isCosmosBulkOperationEnabled)
                    {
                        Dictionary<Guid, RMRule> ruleCache;
                        using (new PerformanceScope("OnPremiseQuerySPData.GetRulesWithoutRemoved"))
                        {
                            ruleCache = (await RuleDao.GetRulesWithoutRemovedAsync()).ToDictionary(r => r.RuleId);
                        }

                        var bulkSize = records[0].BulkSize;
                        if (bulkSize == default(int))
                        {
                            bulkSize = CosmosBulkOperator.DefualtBufferSize;
                        }
                        var ids = records.Select(r => r.Id).ToList();
                        List<Record> dbRecords;
                        using (new PerformanceScope("OnPremiseQuerySPData.GetDBRecords"))
                        {
                            dbRecords = _explorerDao.QueryAll(r => ids.Contains(r.Id)).ToList();
                        }
                        logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                        List<Record> syncRecords = new List<Record>();
                        foreach (var dto in records)
                        {
                            var dbRecord = dbRecords.Where(r => r.Id == dto.Id).FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(dto.RecordsId) && dto.NodeType == (int)RMNodeLevel.Item)
                            {
                                if (dbRecord == null || string.IsNullOrWhiteSpace(dbRecord.RecordsId))
                                {
                                    dto.RecordsId = idUtil.GenerateUniqueId();
                                }
                                else
                                {
                                    dto.RecordsId = dbRecord?.RecordsId;
                                }
                            }
                            //AssembleRecordOwner(dto, tempMapping);
                            var record = ConvertUtil.ConvertRecordDto2Record(dto);
                            RMRule tempRule = null;
                            if (record.RuleId != Guid.Empty && ruleCache != null && ruleCache.ContainsKey(record.RuleId))
                            {
                                tempRule = ruleCache[record.RuleId];
                            }
                            if (_explorerDao.NeedUpdateRecord(record, false, dbRecord, tempRule))
                            {
                                syncRecords.Add(record);
                            }
                        }
                        failedIds = _explorerDao.BatchUpdate(syncRecords, bulkSize);
                        var duplicateRecords = syncRecords.Where(r => !failedIds.Contains(r.Id) && r.hasDuplicate);
                        foreach (var item in duplicateRecords)
                        {
                            ProcessManualDuplicateData(_explorerDao, item.Id);
                        }
                    }
                    else
                    {
                        foreach (var dto in records)
                        {
                            try
                            {
                                if (dto != null)
                                {
                                    Record dbRecord = _explorerDao.ReadById(dto.ScopeId, dto.Id); ;
                                    if (string.IsNullOrWhiteSpace(dto.RecordsId) && dto.NodeType == (int)RMNodeLevel.Item)
                                    {
                                        if (dbRecord == null || string.IsNullOrWhiteSpace(dbRecord.RecordsId))
                                        {
                                            dto.RecordsId = idUtil.GenerateUniqueId();
                                        }
                                        else
                                        {
                                            dto.RecordsId = dbRecord?.RecordsId;
                                        }
                                    }
                                    //AssembleRecordOwner(dto, tempMapping);
                                    var message = ExplorerService.AddOrUpdateSPOnPremObject(dto);
                                    if (message.MessageType == RAMessageType.Failed)
                                    {
                                        failedIds.Add(dto.Id);
                                    }
                                    else if (message.MessageType == RAMessageType.Exception)
                                    {
                                        skipIds.Add(dto.Id);
                                    }
                                    else if (dbRecord != null && dbRecord.CheckExistAndTagDuplicateManual())
                                    {
                                        ProcessManualDuplicateData(_explorerDao, dto.Id);
                                    }
                                    logger.Info($"Finish adding record : {dto.Id} to db.");
                                }
                            }
                            catch (Exception e)
                            {
                                GCommon.Utility.ArgumentCheck.NotNull(dto, nameof(dto));
                                failedIds.Add(dto.Id);
                                logger.Error("An error occurred while add sp data to explorer. Error:{0}", e.ToString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    failedIds = records.Select(r => r.Id).ToList();
                    logger.Error("An error occurred while adding sp data to explorer, error:{0}", e.ToString());
                }
            }
            
            return new AgentSyncDataResultDto() { FailedGuids = failedIds, SkippedGuids = skipIds };
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
        public Dictionary<Guid, string> RetrieveItemOwnerMapping([FromBody] ItemOwnerMappingDto dto)
        {
            Dictionary<Guid, string> tempMapping = new Dictionary<Guid, string>();
            try
            {
                ThrowUtil.ThrowIfNull(dto.ScopeId, "ScopeId is null");
                ThrowUtil.ThrowIfNull(dto.NodeIds, "NodeIds is null");
                List<RMManualApprove> mas = RMManualApproveDao.GetManualApproveByNodes(dto.ScopeId, dto.NodeIds);
                tempMapping = AssembleItemAndOwnerMappingNew(mas, dto.NodeIds);
                //List<RMManualApprove> esclates = mas.Where(a => a.WorkflowInstanceId == Guid.Empty && a.EscalateTo != null).ToList();
                //foreach (RMManualApprove ra in esclates)
                //{
                //    List<string> userIds = ra.EscalateTo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //    logger.Debug("Owner for node {0}, {1}, is {2}", ra.NodeId, ra.Url, string.Join("|", userIds));
                //    if (!tempMapping.ContainsKey(ra.NodeId))
                //    {
                //        tempMapping.Add(ra.NodeId, string.Join("|", userIds));
                //    }
                //}
                //List<RMManualApprove> workflow = mas.Where(a => a.WorkflowInstanceId != Guid.Empty).ToList();

                //if (workflow.Count > 0)
                //{
                //    Dictionary<Guid, List<string>> dictionary = RMManualApproveDao.GetManualNodeAndApproverMapping(dto.ScopeId, workflow.Select(a => a.NodeId).ToList());
                //    if (dictionary.Count > 0)
                //    {
                //        List<string> tempUserIds = new List<string>();
                //        foreach (var id in dictionary.Values)
                //        {
                //            tempUserIds.AddRange(id);
                //        }
                //        List<string> uniqueUserIds = tempUserIds.Where(a => a != null).Distinct().ToList();
                //        List<RMAccount> accounts = AccountDao.GetUserByUserIds(uniqueUserIds);
                //        foreach (KeyValuePair<Guid, List<string>> pa in dictionary)
                //        {
                //            if (!tempMapping.ContainsKey(pa.Key))
                //            {
                //                List<int> userKey = accounts.Where(a => pa.Value.Contains(a.UserId)).Select(s => s.Id).ToList();
                //                logger.Debug("Owner for workflow node {0}, is {1}", pa.Key, string.Join("|", userKey));
                //                if (userKey.Count > 0)
                //                {
                //                    string owner = AddBeforeAndAfterSeparator(string.Join("|", userKey));
                //                    tempMapping.Add(pa.Key, owner);
                //                }
                //            }
                //            else
                //            {
                //                logger.Warn("Node {0} has multi manual approve records", pa.Key);
                //            }
                //        }
                //    }
                //}
                //logger.Info("Node with manual info, count {0}", tempMapping.Count);
                //foreach (Guid nodeId in dto.NodeIds)
                //{
                //    if (!tempMapping.ContainsKey(nodeId))
                //    {
                //        tempMapping.Add(nodeId, string.Empty);
                //    }
                //}
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting item owner mapping, error{0}", e.ToString());
            }
            return tempMapping;
        }

        private Dictionary<Guid, string> AssembleItemAndOwnerMappingNew(List<RMManualApprove> mas, List<Guid> allNodeIds)
        {
            Dictionary<Guid, string> tempMapping = new Dictionary<Guid, string>();
            foreach (var ma in mas)
            {
                var ownerIds = RMManualApproveDao.GetManualApproveOwnerIds(ma);
                if (!tempMapping.ContainsKey(ma.NodeId))
                {
                    tempMapping.Add(ma.NodeId, string.Join("|", ownerIds) + "|");
                }
            }
            logger.Info("Node with manual info, count {0}", tempMapping.Count);
            foreach (Guid nodeId in allNodeIds)
            {
                if (!tempMapping.ContainsKey(nodeId))
                {
                    tempMapping.Add(nodeId, string.Empty);
                }
            }
            return tempMapping;
        }

        [HttpPost]
        public Dictionary<Guid, string> RetrieveIncrementalItemOwnerMapping([FromBody] IncrementalItemOwnerMappingDto mappingDto)
        {
            Dictionary<Guid, string> tempMapping = new Dictionary<Guid, string>();
            try
            {
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                ThrowUtil.ThrowIfNull(mappingDto.ScopeId, "ScopeId is null");
                ThrowUtil.ThrowIfNull(mappingDto.ItemId, "ItemId is null");
                var nodeIds = _explorerDao.QueryAll(r => r.ScopeId == mappingDto.ScopeId && r.ListId == mappingDto.ListId && mappingDto.ItemId.Contains(r.ItemRowId))?.Select(r => r.ItemId)?.ToList();
                if (nodeIds == null || nodeIds.Count == 0)
                {
                    logger.Warn("No record found for IncrementalItemOwnerMapping.");
                    return new Dictionary<Guid, string>();
                }
                List<RMManualApprove> mas = RMManualApproveDao.GetManualApproveByNodes(mappingDto.ScopeId, nodeIds);
                foreach (var ma in mas)
                {
                    var ownerIds = RMManualApproveDao.GetManualApproveOwnerIds(ma);
                    if (!tempMapping.ContainsKey(ma.NodeId))
                    {
                        tempMapping.Add(ma.NodeId, string.Join("|", ownerIds) + "|");
                    }
                }
                logger.Info("Node with manual info, count {0}", tempMapping.Count);
                //foreach (Guid nodeId in mappingDto.ItemId)
                //{
                //    if (!tempMapping.ContainsKey(nodeId))
                //    {
                //        tempMapping.Add(nodeId, string.Empty);
                //    }
                //}
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting inremental item owner mapping, error{0}", e.ToString());
            }
            return tempMapping;
        }

        [HttpPost]
        public List<RecordDto> RetrieveRecordsByTerms([FromBody] QueryChangedTermItemsDto queryDto)
        {
            List<RecordDto> dtos = new List<RecordDto>();
            try
            {
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                var allRecords = _explorerDao.GetRecordsByTermsByPage(queryDto.ScopeId, queryDto.TermIds, queryDto.Ticks, queryDto.SortTicks, queryDto.PageSize);
                foreach (var record in allRecords)
                {
                    dtos.Add(ConvertUtil.ConvertRecord2RecordDto(record));
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting records by terms, error:{0}", e.ToString());
            }
            return dtos;
        }

        [HttpPost]
        public List<Guid> UpdateRecordsInExplorer([FromBody] List<RecordDto> records)
        {
            List<Guid> failedIds = new List<Guid>();
            try
            {
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                foreach (var record in records)
                {
                    try
                    {
                        _explorerDao.Upsert(ConvertUtil.ConvertRecordDto2Record(record));
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while updating record in explorer, error:{0}", e.ToString());
                        failedIds.Add(record.Id);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while updating records. Error:{0}", e.ToString());
            }
            return failedIds;
        }

        [HttpPost]
        public bool AddSiteFlagInfos([FromBody] List<RA.Contract.Global.Object.NodeFlag> nodeFlags)
        {
            bool result = true;
            foreach (var node in nodeFlags)
            {
                try
                {
                    RMNodeFlagDao.AddSiteFlagInfo(new RMNodeFlag()
                    {
                        CollectionTime = node.CollectionTime,
                        FolderId = node.FolderId,
                        FullPath = node.FullPath,
                        GroupId = node.GroupId,
                        IsRemoved = node.IsRemoved,
                        ListId = node.ListId,
                        NodeFlagType = node.NodeFlagType,
                        NodeId = node.NodeId,
                        RowId = node.RowId,
                        Title = node.Title
                    });
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while adding site flag info. Id:{0} Error:{1}", node.NodeId, e.ToString());
                    result = false;
                }
            }
            return result;
        }

        [HttpPost]
        public bool RemoveSPObjInExplorer([FromBody] RemoveSPObjDto removeDto)
        {
            bool result = true;
            try
            {
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                var siteId = removeDto.SiteId;
                var recId = IDGenerator.GetRecordId(siteId, removeDto.ObjectId);
                Record removeRecordInDB = _explorerDao.ReadById(siteId, recId);
                if (removeRecordInDB != null)
                {
                    List<Guid> subFolderIds = new List<Guid>();
                    List<Record> tempRecords = GetAssociatedRecords(removeRecordInDB, _explorerDao, ref subFolderIds);
                    logger.Debug($"get {removeRecordInDB.DirPath} removed items related count:{tempRecords.Count}");

                    if (removeRecordInDB.RecordStatus == (int)RMRecordStatus.Active)
                    {
                        _explorerDao.UpdateRecordState(removeRecordInDB, (int)RMRecordStatus.RMDeleted, subFolderIds);
                        logger.Info("update record state to 3,siteId: {0}, objId: {1}, itemId: {2}", siteId, removeDto.ObjectId, removeDto.ItemRowId);

                    }
                    else
                    {
                        logger.Warn("sp object already archived,siteId:{0}, objId:{1}, itemId:{2}", siteId, removeDto.ObjectId, removeDto.ItemRowId);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while removing sp data in explorer, error:{0}", e.ToString());
                result = false;
            }
            return result;
        }

        private List<Record> GetAssociatedRecords(Record rec, RA.DB.Explorer.Dao.IExplorerDao _explorerDao, ref List<Guid> folderIds)
        {
            List<Record> results = new List<Record>();
            if (rec != null)
            {
                Expression<Func<Record, bool>> lambda = null;
                switch (rec.NodeType)
                {
                    case (int)NodeLevel.SiteCollection:
                        lambda = s => s.ScopeId == rec.ScopeId;
                        break;
                    case (int)NodeLevel.Site:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.NodeType == (int)NodeLevel.Item;
                        break;
                    case (int)NodeLevel.List:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.NodeType == (int)NodeLevel.Item;
                        break;
                    case (int)NodeLevel.Folder:
                        //Get all folder id list under current folder...

                        var tempFolderIds = _explorerDao.GetAllSubFolderUnderFolder(rec);
                        logger.Debug($"get removed folder count:{tempFolderIds.Count}");
                        folderIds = tempFolderIds;
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.NodeType == (int)NodeLevel.Item && tempFolderIds.Contains(s.FolderId);
                        break;
                    case (int)NodeLevel.Item:
                        results.Add(rec);
                        return results;
                    default:
                        logger.Warn($"node type not supported:{rec.NodeType}, {rec.DirPath}");
                        break;
                }
                if (lambda != null)
                {
                    results = _explorerDao.GetFilterList(a => new Record { Id = a.Id, TermId = a.TermId, ScopeId = a.ScopeId, RecordStatus = a.RecordStatus, DestroyedTime = a.DestroyedTime }, lambda).ToList();
                }

            }
            return results;
        }

        [HttpPost]
        public bool AddSiteScope([FromBody] AvePoint.RA.Contract.Global.Object.RMScope site)
        {
            bool result = true;
            try
            {
                RMScope siteScope = new RMScope()
                {
                    FullPath = site.FullPath,
                    ScopeId = site.ScopeId,
                    IsRemoved = false,
                    ScopeName = site.ScopeName
                };
                RMScopeDao.AddOrUpateSiteScope(siteScope);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while adding site scope, error:{0}", e.ToString());
                result = false;
            }
            return result;
        }


        [HttpPost]
        public bool UpdateDeletedItemsInExplorer([FromBody] List<AvePoint.RA.Contract.Global.Object.DeleteItemDto> dtos)
        {
            bool result = true;
            try
            {
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                foreach (var dto in dtos)
                {
                    try
                    {
                        var siteId = dto.SiteId;
                        var itemId = dto.ItemId;
                        var recId = IDGenerator.GetRecordId(siteId, itemId);
                        Record removeRecordInDB = null;
                        removeRecordInDB = _explorerDao.ReadById(siteId, recId);
                        if (removeRecordInDB != null)
                        {
                            if (removeRecordInDB.RecordStatus == (int)RMRecordStatus.Active)
                            {
                                _explorerDao.UpdateRecordState(removeRecordInDB, (int)RMRecordStatus.RMDeleted);
                                logger.Info("update record state to 3,siteId: {0}, List id: {1}, ItemRowId: {2}", dto.SiteId, dto.ListId, dto.ItemRowId);
                            }
                            else
                            {
                                logger.Warn("sp object already archived,siteId:{0}, List id:{1}, ItemRowId:{2}", dto.SiteId, dto.ListId, dto.ItemRowId);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while updating deleted items. SiteId: {0}, List id: {1}, ItemRowId: {2} Error:{3}", dto.SiteId, dto.ListId, dto.ItemRowId, e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while updating deleted items. Error:{0}", e.ToString());
                result = false;
            }
            return result;
        }
        #endregion

        #region explorer action
        [HttpPost]
        public Dictionary<string, AvePoint.RA.Contract.Global.JobMessage.SiteInfo> FindOnPremiseSiteInfos([FromBody] List<string> siteIds)
        {
            Dictionary<string, AvePoint.RA.Contract.Global.JobMessage.SiteInfo> infos = new Dictionary<string, RA.Contract.Global.JobMessage.SiteInfo>();
            try
            {
                foreach (var id in siteIds)
                {
                    try
                    {
                        AvePoint.RA.Contract.Global.JobMessage.SiteInfo info = new RA.Contract.Global.JobMessage.SiteInfo();
                        var site = SharePointOnPremClient.GetLocalSiteCollectionById(id);
                        info.SiteUrl = site.Url;
                        var webApp = SharePointOnPremClient.GetLocalWebApplicationById(site.ParentId);
                        var groupLevelSetting = SharePointOnPremiseSettingDao.GetGroupLevelSetting(webApp.Url, new Guid(webApp.Id));
                        var columnName = groupLevelSetting.IsUsingExistColumnName ? groupLevelSetting.ExistColumnName : groupLevelSetting.ColumnName;
                        info.BCSColumnName = columnName;
                        infos.Add(id, info);
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while get site info. Site id:{0} Error:{1}", id, e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while get site info. Error:{0}", e.ToString());
            }
            return infos;
        }

        [HttpPost]
        public bool AddRecordHistory([FromBody] AvePoint.RA.Contract.Global.Explorer.RecordHistoryDto recordHistoryDto)
        {
            bool result = true;
            try
            {
                RecordsHistoryService.AddRecordsHistoryWithUser(recordHistoryDto.CurrentIds, recordHistoryDto.historyAction, recordHistoryDto.LogonUser, recordHistoryDto.Comment);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while AddRecordsHistory. Error:{0}", e.ToString());
                result = false;
            }
            return result;
        }

        [HttpPost]
        public bool UpdateTermChangeItems([FromBody] AvePoint.RA.Contract.Global.Explorer.TermChangeItemDto termChangeItemDto)
        {
            bool result = true;
            try
            {
                var isNewLogicAccount = TenantService.IsNewOpusTenant();
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                var previousTermId = Guid.Empty;
                _explorerDao.UpdateAll(r => termChangeItemDto.Ids.Contains(r.Id), rec =>
                {
                    previousTermId = rec.TermId;
                    rec.TermId = termChangeItemDto.TermId;
                    rec.TermName = termChangeItemDto.TermName;
                    rec.RuleId = Guid.Empty;
                    rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                    rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                    rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                    rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
                    if(isNewLogicAccount && previousTermId != rec.TermId) rec.RemoveManualFields();
                });
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while UpdateTermChangeItems. Error:{0}", e.ToString());
                result = false;
            }
            return result;
        }

        [HttpPost]
        public bool UpdateDeclaredItems([FromBody] AvePoint.RA.Contract.Global.Explorer.DeclareItemDto declareItemDto)
        {
            bool result = true;
            try
            {
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                _explorerDao.UpdateAll(r => declareItemDto.Ids.Contains(r.Id), rec => { rec.DeclareAsRecord = declareItemDto.IsDeclare; rec.DeclaredBy = declareItemDto.DeclaredBy; });
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while UpdateDeclaredItems. Error:{0}", e.ToString());
                result = false;
            }
            return result;
        }


        [HttpPost]
        public bool AddClassificationHistory([FromBody] List<AvePoint.RA.Contract.Global.Object.RMClassificationHistory> classificationHistories)
        {
            bool result = true;
            try
            {
                foreach (var history in classificationHistories)
                {
                    ClassificationHistoryDao.Create(new RMClassificationHistory()
                    {
                        RecordId = history.RecordId,
                        PreviousTermId = history.PreviousTermId,
                        NewTermId = history.NewTermId,
                        OperationTime = history.OperationTime
                    });
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while AddClassificationHistory. Error:{0}", e.ToString());
                result = false;
            }
            return result;
        }

        [HttpPost]
        public List<RecordDto> FindRecordsByIds([FromBody] List<Guid> ids)
        {
            List<RecordDto> records = new List<RecordDto>();
            try
            {
                RA.DB.Explorer.Dao.IExplorerDao _explorerDao = new ExplorerDao(true);
                var tempRecords = _explorerDao.GetRecordByIds(ids);
                if (tempRecords != null && tempRecords.Count > 0)
                {
                    records = tempRecords.ConvertAll(r => ConvertUtil.ConvertRecord2RecordDto(r));
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting records by ids. Error:{0}", e.ToString());
            }
            return records;
        }

        [HttpPost]
        public bool UpdateRealtimeJobState([FromBody] AvePoint.RA.Contract.Global.Object.RealtimeJobState realtimeJobState)
        {
            bool result = true;
            try
            {
                RMRecordsUpdateTempDao.InsertUpdateTemp(realtimeJobState.Jobid, "", realtimeJobState.Status, realtimeJobState.StartItems);
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        [HttpPost]
        public async Task<AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryResult> QueryDataForGlobalSearch([FromBody] AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryDto dto)
        {
            AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryResult result = new AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryResult();
            try
            {
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(dto.JobId, true);
                var jobContext = SerializerHelper.DeserializeByDataContractSerializer<GlobalSearchActionDto>(subJobWithContext.JobContext.Content);
                TenantLocalValue.LogonUserId = jobContext.UserId;
                ExplorerQueryV3Dto explorerQueryV2Dto = jobContext.FilterInfo;
                explorerQueryV2Dto.PagingInfo = new RA.Contract.RMWeb.ExplorerPagingInfo()
                {
                    PageIndex = dto.PageInfo.PageIndex,
                    PageSize = dto.PageInfo.PageSize
                };
                var queryResult = await ExplorerQueryService.QueryDataListWithoutTotalAsync(explorerQueryV2Dto);
                result.Data = queryResult.Datas != null && queryResult.Datas.Count > 0 ?
                    queryResult.Datas.ConvertAll(r => ConvertUtil.ConvertBaseRecordDto2RecordDto(r))
                    : new List<RecordDto>();

                result.PageInfo = queryResult.PagingInfo != null ? new RA.Contract.Global.Explorer.ExplorerPagingInfo()
                {
                    HasNextPage = queryResult.PagingInfo.HasNextPage,
                    PageIndex = queryResult.PagingInfo.PageIndex,
                    PageSize = queryResult.PagingInfo.PageSize,
                    Total = queryResult.PagingInfo.Total
                } : null;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while querying data for global search. JobId:{0} Error:{1}", dto?.JobId, e.ToString());
            }
            return result;
        }
        #endregion
    }
}
