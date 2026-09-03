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
using Amazon.Runtime.Internal.Transform;
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.Service.LocationManagement;
using AvePoint.RA.Service.Services.PermissionManagement;
using AvePoint.RA.Service.Services.TemplateManagement;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.RecordsRepository;
using Newtonsoft.Json;
using SforceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColumnType = AvePoint.RA.Contract.TemplateManagement.ColumnType;

namespace AvePoint.RA.Service.Services.Explorer
{
    public class RecordsHistoryService : RMServiceBase, IRecordsHistoryService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RecordsHistoryService));
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        public IRecordsHistoryTableDao RecordsHistoryTableDao => PlatformWindsorManager.GetService<IRecordsHistoryTableDao>();

        public IPhysicalRecordsActionAuditTableDao PhysicalRecordsActionAuditTableDao => PlatformWindsorManager.GetService<IPhysicalRecordsActionAuditTableDao>();
        public IPhysicalRecordsMoveDataTableDao PhysicalRecordsMoveDataHistoryTableDao => PlatformWindsorManager.GetService<IPhysicalRecordsMoveDataTableDao>();
        public IRecordReturnLoanDataHistoryTableDao RecordReturnLoanDataHistoryDao => PlatformWindsorManager.GetService<IRecordReturnLoanDataHistoryTableDao>();
        public ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public IHoldDao HoldDao => PlatformWindsorManager.GetService<IHoldDao>();

        public void AddRecordsHistory(List<Guid> currentIds, string historyAction, string comment = "")
        {
            logger.Info($"start add records history: {string.Join(",", currentIds)}");
            List<RecordHistoryTableEntity> entities = new List<RecordHistoryTableEntity>();
            //Move History From CosmosDB to AzureTable
            var historys = ExplorerDao.QueryAll(s => currentIds.Contains(s.Id), false).ToList();
            logger.Info($"get history count: {historys.Count}");
            foreach (var his in historys)
            {
                string str = string.Empty;
                if (!string.IsNullOrEmpty(his.RecordHistory))
                {
                    logger.Info($"Add history to AzureTable, RecordsId: {his.Id}, RecordHistory: {his.RecordHistory}");
                    var exist = XmlUtil.GetXmlObject<RecordHistoryXml>(his.RecordHistory);
                    foreach (var existHistory in exist.HistoryList)
                    {
                        LogRecordHistory(existHistory);
                        entities.Add(new RecordHistoryTableEntity()
                        {
                            PartitionKey = his.Id.ToString(),
                            RowKey = Guid.NewGuid().ToString(),
                            Action = existHistory.Action,
                            ExecuteOn = existHistory.TimeUTC,
                            User = existHistory.User,
                            Comment = comment,
                        });
                    }
                }
                else
                {
                    logger.Info($"CosmosDB Records RecordHistory is Empty, Just insert history data directly into azure table, recordsId: {his.Id}");
                }
            }
            //New History Insert
            foreach (var currentId in currentIds)
            {
                entities.Add(new RecordHistoryTableEntity()
                {
                    PartitionKey = currentId.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Action = historyAction,
                    ExecuteOn = DateTime.UtcNow.Ticks,
                    User = WebUtil.LogOnUserName,
                    Comment = comment
                });
            }
            RecordsHistoryTableDao.AddRecordsHistory(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], TenantLocalValue.LogonGroupId, entities);
            ExplorerDao.UpdateAll(r => currentIds.Contains(r.Id), r => r.RecordHistory = "");
            logger.Info($"Insert AzureTable Successfully, RecordsIds: {string.Join(",", currentIds)} , Entities Count: {entities.Count}");
        }

        public void AddRecordsHistoryWithUser(List<Guid> currentIds, string historyAction, string logonUser, string comment)
        {
            logger.Info($"start add records history: {string.Join(",", currentIds)}");
            List<RecordHistoryTableEntity> entities = new List<RecordHistoryTableEntity>();
            //Move History From CosmosDB to AzureTable
            var historys = ExplorerDao.QueryAll(s => currentIds.Contains(s.Id), false).ToList();
            logger.Info($"get history count: {historys.Count}");
            if (historys.Count == 0)
            {
                historys = ExplorerDao.GetRecordsBySql(currentIds);
                logger.Info($"get history count (retry 1): {historys.Count}");
            }

            if (historys.Count == 0)
            {
                historys = ExplorerDao.GetRecordByIds(currentIds);
                logger.Info($"get history count (retry 2): {historys.Count}");
            }

            if (historys.Count == 0)
            {
                foreach (var currId in currentIds)
                {
                    var currRec = ExplorerDao.GetFirstOrDefault((s) => s.Id == currId);
                    if (currRec != null)
                    {
                        historys.Add(currRec);
                    }
                }
                logger.Info($"get history count (retry 3): {historys.Count}");
            }

            foreach (var his in historys)
            {
                string str = string.Empty;
                if (!string.IsNullOrEmpty(his.RecordHistory))
                {
                    logger.Info($"Add history to AzureTable, RecordsId: {his.Id}, RecordHistory: {his.RecordHistory}");
                    var exist = XmlUtil.GetXmlObject<RecordHistoryXml>(his.RecordHistory);
                    foreach (var existHistory in exist.HistoryList)
                    {
                        LogRecordHistory(existHistory);
                        entities.Add(new RecordHistoryTableEntity()
                        {
                            PartitionKey = his.Id.ToString(),
                            RowKey = Guid.NewGuid().ToString(),
                            Action = existHistory.Action,
                            ExecuteOn = existHistory.TimeUTC,
                            User = existHistory.User,
                        });
                    }
                }
                else
                {
                    logger.Info($"CosmosDB Records RecordHistory is Empty, Just insert history data directly into azure table, recordsId: {his.Id}");
                }
            }
            //New History Insert
            foreach (var currentId in currentIds)
            {
                entities.Add(new RecordHistoryTableEntity()
                {
                    PartitionKey = currentId.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Action = historyAction,
                    ExecuteOn = DateTime.UtcNow.Ticks,
                    User = logonUser,
                    Comment = comment
                });
            }
            RecordsHistoryTableDao.AddRecordsHistory(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], TenantLocalValue.LogonGroupId, entities);
            ExplorerDao.UpdateAll(r => currentIds.Contains(r.Id), r => r.RecordHistory = "");
            logger.Info($"Insert AzureTable Successfully, RecordsIds: {string.Join(",", currentIds)} , Entities Count: {entities.Count}");
        }

        public async Task<List<RecordHistory>> GetRecordsHistoryAsync(string historyInfo, Guid recordsId, bool isControlPlus = false)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            if (isControlPlus) gls.TimeZoneId = TenantLocalValue.TimezoneId;
            List<RecordHistory> historyList = new List<RecordHistory>();
            if (!string.IsNullOrEmpty(historyInfo))
            {
                historyList = XmlUtil.GetXmlObject<RecordHistoryXml>(historyInfo).HistoryList;
                historyList = historyList.OrderByDescending(o => o.TimeUTC).ToList();
            }
            else
            {
                try
                {
                    var historys = RecordsHistoryTableDao.GetRecordsHistory(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], TenantLocalValue.LogonGroupId, recordsId.ToString());
                    if (historys != null && historys.Count() != 0)
                    {
                        historyList = historys.Select(h => new RecordHistory() { Action = h.Action, TimeUTC = h.ExecuteOn, User = h.User, Comment = h.Comment }).ToList();
                    }
                }
                catch (AzureTableNotExistException ae)
                {
                    logger.Warn($"Get RecordsHistory Azure Table Error: {ae}");
                }
            }

            foreach (var item in historyList)
            {
                if (item.TimeUTC != 0)
                {
                    item.DisplayTime = GeneralSettingService.ConvertTiksToDateTime(gls, item.TimeUTC, true).SimplifyFormatTime;
                }
                item.Action = I18NEntity.GetStringWithSeparator(item.Action);
            }
            return historyList;
        }

        public void CloneMoveHistoryRecords(Guid sourceId, Guid destId)
        {
            List<RecordHistoryTableEntity> entities = new List<RecordHistoryTableEntity>();
            RecordsHistoryTableDao.CloneMoveHistoryRecords(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], TenantLocalValue.LogonGroupId, sourceId, destId);
        }

        private void LogRecordHistory(RecordHistory recordHistory)
        {
            logger.Info($"Record History, Action: {recordHistory.Action} DisplayTime: {recordHistory.DisplayTime} TimeUTC: {recordHistory.TimeUTC} User: {recordHistory.User}");
        }

        public async Task AddPhysicalRecordActionAuditAsync(PhysicalActionType actionType, Guid recordId, PhysicalObjectDto newObject, bool isNew, PhysicalObjectDto oldObject = null)
        {
            var audit = new PhysicalRecordActionAudit()
            {
                PartitionKey = recordId.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Action = (int)actionType,
                User = TenantLocalValue.LogonUserEmail,
                ExecuteOn = DateTime.UtcNow.Ticks,
            };

            if (!isNew)
            {
                if (oldObject == null)
                {
                    oldObject = await ExplorerService.GetPhysicalObjectByIdAsync(newObject.Id);
                }

                if (oldObject.Id == newObject.Id)
                {
                    var template = await TemplateManagementService.LoadTemplateDtoAsync(oldObject.TemplateId, oldObject);
                    var allColumns = GetColumnsInfo(template);
                    var modifyContent = await AddPhyColumnsAuditAsync(allColumns, oldObject, newObject);
                    audit.ModifyContent = modifyContent;
                }
            }

            PhysicalRecordsActionAuditTableDao.AddPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, new List<PhysicalRecordActionAudit> { audit });
        }

        public void AddPhysicalHoldActionAudit(Dictionary<Guid, string> records, HoldSettingDto holdDto, string holdName, AuditAction actionType)
        {
            var auditList = new List<PhysicalRecordActionAudit>();
            foreach (var record in records)
            {
                var audit = new PhysicalRecordActionAudit()
                {
                    PartitionKey = record.Key.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Action = (int)PhysicalActionType.PlaceHold,
                    User = TenantLocalValue.LogonUserEmail,
                    ExecuteOn = DateTime.UtcNow.Ticks,
                };

                if (holdDto.HoldAction == RecordsConstants.HOLD_ACTION_APPEND)
                {
                    audit.Action = (int)PhysicalActionType.AddHold;
                    if (actionType == AuditAction.CreateHoldTypeWithRecord)
                    {
                        actionType = AuditAction.CreateAppendHoldTypeWithRecord;
                    }
                    if (actionType == AuditAction.ReuseHoldTypeWithRecord)
                    {
                        actionType = AuditAction.ReuseAppendHoldTypeWithRecord;
                    }
                }
                else if (holdDto.HoldAction == RecordsConstants.HOLD_ACTION_CHANGE)
                {
                    audit.Action = (int)PhysicalActionType.ChangeHold;
                }
                var targetSetting = "";
                var auditItemList = new List<PhysicalAuditItem>();
                switch (actionType)
                {
                    case AuditAction.CreateHoldTypeWithRecord:
                        targetSetting = "RM_BCM_Audit_Action_CreateHoldTypeWithRecord";
                        break;
                    case AuditAction.ReuseHoldTypeWithRecord:
                        targetSetting = "RM_BCM_Audit_Action_ReuseHoldTypeWithRecord";
                        break;
                    case AuditAction.CreateAppendHoldTypeWithRecord:
                        targetSetting = "RM_BCM_Audit_Action_CreateAppendHoldTypeWithRecord";
                        break;
                    case AuditAction.ReuseAppendHoldTypeWithRecord:
                        targetSetting = "RM_BCM_Audit_Action_ReuseAppendHoldTypeWithRecord";
                        break;
                }

                var auditItem = new PhysicalAuditItem();
                if (actionType == AuditAction.CreateHoldTypeWithRecord
    || actionType == AuditAction.ReuseHoldTypeWithRecord
    || actionType == AuditAction.CreateAppendHoldTypeWithRecord
    || actionType == AuditAction.ReuseAppendHoldTypeWithRecord)
                {
                    var originalHoldNames = new List<string>();
                    if (!string.IsNullOrEmpty(record.Value))
                    {
                        List<string> holdIds = record.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                        List<RMHold> holds = HoldDao.GetHoldByIds(holdIds);
                        if (!holds.IsNullOrEmpty())
                        {
                            originalHoldNames = holds.Select(h => h.Name).ToList();
                        }
                    }
                    auditItem = new()
                    {
                        TargetSetting = targetSetting,
                        OldValue = string.Join(", ", originalHoldNames),
                        NewValue = holdName
                    };
                    auditItemList.Add(auditItem);
                }
                else if (actionType == AuditAction.ChangeHoldCreate || actionType == AuditAction.ChangeHoldReuse)
                {
                    var originalHoldNames = new List<string>();
                    if (!string.IsNullOrEmpty(record.Value))
                    {
                        List<string> holdIds = record.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                        List<RMHold> holds = HoldDao.GetHoldByIds(holdIds);
                        if (!holds.IsNullOrEmpty())
                        {
                            originalHoldNames = holds.Select(h => h.Name).ToList();
                        }
                    }
                    auditItem = new()
                    {
                        TargetSetting = "RM_JS_RDM_Hold_HoldName",
                        OldValue = string.Join(", ", originalHoldNames),
                        NewValue = holdName
                    };
                    auditItemList.Add(auditItem);
                }
                if (auditItemList.Count != 0)
                {
                    audit.ModifyContent = JsonConvert.SerializeObject(auditItemList);
                }
                auditList.Add(audit);
            }

            PhysicalRecordsActionAuditTableDao.AddPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, auditList);
        }

        public void AddPhysicalRelatedActionAudit(Guid id, string relateRecords, List<string> addRecords)
        {
            var oldRelated = string.Empty;
            var audit = new PhysicalRecordActionAudit()
            {
                PartitionKey = id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Action = (int)PhysicalActionType.ManageRelated,
                User = TenantLocalValue.LogonUserEmail,
                ExecuteOn = DateTime.UtcNow.Ticks,
            };

            if (!string.IsNullOrEmpty(relateRecords))
            {
                List<RMRelatedItemInfo> infos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(relateRecords);
                var oldRelatedIds = infos.Select(r => r.id).ToList();
                oldRelated = string.Join(",", ExplorerDao.QueryAll(r => oldRelatedIds.Contains(r.NodeId)).Select(s => s.LeafName));
            }

            var modifyContent = new List<PhysicalAuditItem>() { 
                new()
                {
                    TargetSetting = "RM_JS_MA_Grid_RelatedRecords",
                    OldValue = oldRelated,
                    NewValue = string.Join(", ", addRecords)
                }
            };

            audit.ModifyContent = JsonConvert.SerializeObject(modifyContent);
            PhysicalRecordsActionAuditTableDao.AddPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, new List<PhysicalRecordActionAudit> { audit });

        }

        public void AddPhysicalCommonHoldActionAudit(Guid id, PhysicalActionType actionType)
        {
            var audit = new PhysicalRecordActionAudit()
            {
                PartitionKey = id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Action = (int)actionType,
                User = TenantLocalValue.LogonUserEmail,
                ExecuteOn = DateTime.UtcNow.Ticks,
            };

            PhysicalRecordsActionAuditTableDao.AddPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, new List<PhysicalRecordActionAudit> { audit });
        }

        public async Task AddPhysicalPermissionAudtisAsync(ScopePermissionDto dto)
        {
            var newAccountIds = dto.AccountIds;
            var auditList = new List<PhysicalRecordActionAudit>();

            foreach (var scopeInfo in dto.ScopeInfos)
            { 
                var audit = new PhysicalRecordActionAudit()
                {
                    PartitionKey = scopeInfo.ScopeId,
                    RowKey = Guid.NewGuid().ToString(),
                    Action = (int)PhysicalActionType.AccessControl,
                    User = TenantLocalValue.LogonUserEmail,
                    ExecuteOn = DateTime.UtcNow.Ticks,
                };

                var modifyContents = new List<PhysicalAuditItem>();
                var item = new PhysicalAuditItem
                {
                    TargetSetting = I18NEntity.GetString(scopeInfo.ScopeNameFullPath).TrimEnd('/')
                };
                var oldAccountIds = PermissionManagementService.GetUserIdsWithPermission(scopeInfo.ScopeId);
                item.OldValue = await GetUserNamesAsync(oldAccountIds);
                item.NewValue = await GetUserNamesAsync(newAccountIds);
                modifyContents.Add(item);
                audit.ModifyContent = JsonConvert.SerializeObject(modifyContents);
                auditList.Add(audit);
            }

            PhysicalRecordsActionAuditTableDao.AddPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, auditList);
        }

        public PhysicalRecordActionAudit BuildPhysicalActionAuditForJob(Guid id, PhysicalActionType actionType, bool isNew, JobRunBy jobRunBy = JobRunBy.Control, string originalPath = "", string destinationPath = "")
        {
            var audit = new PhysicalRecordActionAudit()
            {
                PartitionKey = id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Action = (int)actionType,
                User = TenantLocalValue.LogonUserEmail,
                ExecuteOn = DateTime.UtcNow.Ticks,
            };

            if (actionType == PhysicalActionType.Move)
            {
                var modifyContent = new List<PhysicalAuditItem>();
                modifyContent =
                [
                    new()
                    {
                        TargetSetting = "RM_JS_JMD_Grid_HomeLocation",
                        OldValue = originalPath,
                        NewValue = destinationPath,
                    }
                ];
                audit.ModifyContent = JsonConvert.SerializeObject(modifyContent);
            }

            return audit;
        }

        public PhysicalRecordActionAudit BuildPhysicalLoanAudit(Guid id, Dictionary<string, CustomColumn> customColumnDic, string currHeldBy)
        {
            var audit = new PhysicalRecordActionAudit()
            {
                PartitionKey = id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Action = (int)PhysicalActionType.Loan,
                User = TenantLocalValue.LogonUserEmail,
                ExecuteOn = DateTime.UtcNow.Ticks,
            };

            var oldValue = string.Empty;
            if (customColumnDic != null && customColumnDic.ContainsKey(DefaultColumnIDs.LoanedBy))
            {
                oldValue = customColumnDic[DefaultColumnIDs.LoanedBy]?.Users.FirstOrDefault()?.DisplayName;
            }

            var auditItem = new PhysicalAuditItem();
            auditItem.TargetSetting = "RM_PRM_PRE_TargetSetting_HeldBy";
            auditItem.OldValue = oldValue;
            auditItem.NewValue = currHeldBy;

            audit.ModifyContent =JsonConvert.SerializeObject(new List<PhysicalAuditItem>() { auditItem });
            return audit;
        }

        public PhysicalRecordActionAudit BuildPhysicalReturnLoanAudit(Guid id, Dictionary<string, CustomColumn> customColumnDic)
        {
            var audit = new PhysicalRecordActionAudit()
            {
                PartitionKey = id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Action = (int)PhysicalActionType.ReturnLoan,
                User = TenantLocalValue.LogonUserEmail,
                ExecuteOn = DateTime.UtcNow.Ticks,
            };

            var oldValue = string.Empty;
            if (customColumnDic != null && customColumnDic.ContainsKey(DefaultColumnIDs.LoanedBy))
            {
                oldValue = customColumnDic[DefaultColumnIDs.LoanedBy]?.Users.FirstOrDefault()?.DisplayName;
            }

            var auditItem = new PhysicalAuditItem();
            auditItem.TargetSetting = "RM_PRM_PRE_TargetSetting_HeldBy";
            auditItem.OldValue = oldValue;
            auditItem.NewValue = "";

            audit.ModifyContent = JsonConvert.SerializeObject(new List<PhysicalAuditItem>() { auditItem });
            return audit;
        }

        public PhysicalRecordActionAudit BuildPhysicalReclassifyAudit(Guid id, string orignalTermPath, string currentTermPath)
        {
            var audit = new PhysicalRecordActionAudit()
            {
                PartitionKey = id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                Action = (int)PhysicalActionType.Reclassify,
                User = TenantLocalValue.LogonUserEmail,
                ExecuteOn = DateTime.UtcNow.Ticks,
            };

            var modifyContents = new List<PhysicalAuditItem>();
            modifyContents.Add(new()
            {
                TargetSetting = "RM_PRM_PRE_TargetSetting_TermPath",
                OldValue = orignalTermPath,
                NewValue = currentTermPath,
            });

            audit.ModifyContent = JsonConvert.SerializeObject(modifyContents);
            return audit;
        }

        public void AddPhysicalAudit(List<PhysicalRecordActionAudit> physicalAudits)
        {
            PhysicalRecordsActionAuditTableDao.AddPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, physicalAudits);
        }
        public void AddMoveData(List<PhysicalRecordMoveData> moveData)
        {
            PhysicalRecordsMoveDataHistoryTableDao.Add(TenantLocalValue.LogonGroupId, moveData);
        }

        public async Task<List<PhysicalAudit>> GetPhysicalRecordActionAuditsAsync(Guid recordId)
        {
            var auditList = new List<PhysicalAudit>();
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            try
            {
                var tableEntities = PhysicalRecordsActionAuditTableDao.GetPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, recordId.ToString());
                if (tableEntities != null && tableEntities.Any())
                {
                    return ConvertAzureTableEntityToAudit(tableEntities.ToList(), gls);
                }

                var dbRecord = ExplorerDao.QueryAll(s => s.Id == recordId, false).FirstOrDefault();

                if (dbRecord == null || dbRecord.PhysicalActionAudit == null)
                {
                    return auditList;
                }

                var dbAudits = JsonConvert.DeserializeObject<List<PhysicalAudit>>(dbRecord.PhysicalActionAudit);
                var entities = await CovertDBAuditToAzureTableEntityAsync(recordId, dbAudits);
                PhysicalRecordsActionAuditTableDao.AddPhysicalRecordsAudits(TenantLocalValue.LogonGroupId, entities);
                return ConvertAzureTableEntityToAudit(entities, gls);
            }
            catch (AzureTableNotExistException ae)
            {
                logger.Warn($"Get RecordsHistory Azure Table Error: {ae}");
            }
            return auditList;
        }

        private List<PhysicalAudit> ConvertAzureTableEntityToAudit(List<PhysicalRecordActionAudit> entities, GeneralSettingModel gls)
        {
            var auditList = new List<PhysicalAudit>();
            foreach (var entity in entities)
            {
                try
                {
                    var audit = new PhysicalAudit();
                    audit.ActionType = (PhysicalActionType)entity.Action;
                    audit.ActionTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, entity.ExecuteOn, true).SimplifyFormatTime;
                    audit.ActionUser = I18NEntity.GetString(entity.User);
                    if (!string.IsNullOrEmpty(entity.ModifyContent) && entity.ModifyContent != "null")
                    {
                        var modifyContent = JsonConvert.DeserializeObject<List<PhysicalAuditItem>>(entity.ModifyContent);
                        modifyContent.ForEach(content => content.TargetSetting = I18NEntity.GetString(content.TargetSetting));
                        if (entity.Action == (int)PhysicalActionType.Move)
                        {
                            var tempPath = I18NEntity.GetString("RM_SPS_Location_RootNode");
                            modifyContent[0].OldValue = tempPath + modifyContent[0].OldValue[modifyContent[0].OldValue.IndexOf('/')..];
                            modifyContent[0].NewValue = tempPath + modifyContent[0].NewValue[modifyContent[0].NewValue.IndexOf('/')..];
                        }
                        audit.ModifyContent = modifyContent;
                    }
                    auditList.Add(audit);
                }
                catch (Exception e)
                {
                    logger.Error($"Convert record [{entity.PartitionKey}] action audit failed, error : {e}");
                }
            }

            return auditList;
        }

        private async Task<List<PhysicalRecordActionAudit>> CovertDBAuditToAzureTableEntityAsync(Guid recordId, List<PhysicalAudit> dbAudits)
        {
            var userNameEmailMapping = new Dictionary<string, string>()
            {
                {"RM_TS_RunSchedule", "RM_TS_RunSchedule" }
            };

            var entities = new List<PhysicalRecordActionAudit>();
            foreach (var dbAudit in dbAudits)
            {
                if (!userNameEmailMapping.TryGetValue(dbAudit.ActionUser, out var userEmail))
                {
                    var userInfo = await AccountDao.GetActiveUserByNameAsync(dbAudit.ActionUser);
                    userEmail = userInfo?.UserPrincipalName;
                    userNameEmailMapping[dbAudit.ActionUser] = userEmail;
                }

                var entity = new PhysicalRecordActionAudit()
                {
                    PartitionKey = recordId.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Action = (int)dbAudit.ActionType,
                    ExecuteOn = dbAudit.ActionTime,
                    User = userEmail,
                    ModifyContent = JsonConvert.SerializeObject(dbAudit.ModifyContent),
                    Comment = "",
                };
                entities.Add(entity);
            }

            return entities;
        }

        private List<TemplateColumnDto> GetColumnsInfo(TemplateDto template)
        {
            var allColumns = new List<TemplateColumnDto>();
            template.categories.ForEach(c => allColumns.AddRange(c.columns));
            return allColumns;
        }

        private async Task<string> AddPhyColumnsAuditAsync(List<TemplateColumnDto> allColumns, PhysicalObjectDto oldObject, PhysicalObjectDto newObject)
        {
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            var dtFormat = GeneralSettingService.GetDateTimeFormat(gls);
            var auditItemList = new List<PhysicalAuditItem>();

            foreach (var key in newObject.MetaInfo.Keys)
            {
                if (!oldObject.MetaInfo.ContainsKey(key))
                {
                    _ = oldObject.MetaInfo.TryAdd(key, string.Empty);
                }
            }

            foreach (KeyValuePair<string, string> oldMetaItem in oldObject.MetaInfo)
            {
                if (allColumns.Any(c => c.uniqueId.ToString() == oldMetaItem.Key))
                {
                    var columnInfo = allColumns.Where(c => c.uniqueId.ToString() == oldMetaItem.Key).First();
                    if (newObject.MetaInfo.ContainsKey(oldMetaItem.Key))
                    {
                        var oldValue = oldMetaItem.Value;
                        var newValue = newObject.MetaInfo[oldMetaItem.Key];
                        if (newValue != oldValue)
                        {
                            try
                            {
                                var auditItem = new PhysicalAuditItem
                                {
                                    TargetSetting = I18NEntity.GetString(columnInfo.columnName)
                                };
                                switch ((ColumnType)columnInfo.typeId)
                                {
                                    case ColumnType.SingleText:
                                    case ColumnType.MultipleText:
                                    case ColumnType.Number:
                                        auditItem.OldValue = oldValue;
                                        auditItem.NewValue = newValue;
                                        break;
                                    case ColumnType.DateTime:
                                        auditItem.OldValue = string.IsNullOrEmpty(oldValue) ?
                                            string.Empty :
                                            GeneralSettingService.ConvertTiksToDateTime(gls, JsonConvert.DeserializeObject<DateTimeColumnValue>(oldValue).GetUtcDate().Ticks, true).SimplifyFormatTime;
                                        auditItem.NewValue = string.IsNullOrEmpty(newValue) ?
                                            string.Empty :
                                            GeneralSettingService.ConvertTiksToDateTime(gls, JsonConvert.DeserializeObject<DateTimeColumnValue>(newValue).GetUtcDate().Ticks, true).SimplifyFormatTime;
                                        break;
                                    case ColumnType.SingleChoice:
                                        Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(columnInfo.optionsJSON);
                                        var oldSelectedOption = JsonConvert.DeserializeObject<ChoiceColumnValue>(oldValue);
                                        auditItem.OldValue = GetSelectedChoiceColumnNames(options, new List<string> { oldSelectedOption.Value });

                                        var newSelectedOption = JsonConvert.DeserializeObject<ChoiceColumnValue>(newValue);
                                        auditItem.NewValue = GetSelectedChoiceColumnNames(options, new List<string> { newSelectedOption.Value });
                                        break;
                                    case ColumnType.PeopleOrGroup:
                                        if (!string.IsNullOrEmpty(oldValue))
                                        {
                                            List<UIPeopleColumnValue> oldP = JsonConvert.DeserializeObject<List<UIPeopleColumnValue>>(oldValue);
                                            if (oldP != null && oldP.Count > 0)
                                            {
                                                auditItem.OldValue = string.Join(",", oldP.Select(a => a.DisplayName).ToArray());
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(newValue))
                                        {
                                            List<UIPeopleColumnValue> newP = JsonConvert.DeserializeObject<List<UIPeopleColumnValue>>(newValue);
                                            if (newP != null && newP.Count > 0)
                                            {
                                                auditItem.NewValue = string.Join(",", newP.Select(a => a.DisplayName).ToArray());
                                            }
                                        }
                                        break;
                                    case ColumnType.MultipleChoice:
                                        Dictionary<int, string> mulOptions = JsonConvert.DeserializeObject<Dictionary<int, string>>(columnInfo.optionsJSON);
                                        var oldCheckedOptions = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(oldValue);
                                        var oldOptionValues = oldCheckedOptions.Select(s => s.Value).ToList();
                                        auditItem.OldValue = GetSelectedChoiceColumnNames(mulOptions, oldOptionValues);

                                        var newCheckedOptions = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(newValue);
                                        var newOptionValues = newCheckedOptions.Select(s => s.Value).ToList();
                                        auditItem.NewValue = GetSelectedChoiceColumnNames(mulOptions, newOptionValues);
                                        break;
                                    case ColumnType.Taxonomy:
                                        auditItem.OldValue = JsonConvert.DeserializeObject<TaxonomyColumnValue>(oldValue).Name;
                                        auditItem.NewValue = JsonConvert.DeserializeObject<TaxonomyColumnValue>(newValue).Name;
                                        break;
                                }
                                auditItemList.Add(auditItem);
                            }
                            catch (Exception ex)
                            {
                                logger.Info($"Physical Object Audit failed {ex}");
                            }
                        }
                    }
                }
            }

            return JsonConvert.SerializeObject(auditItemList);
        }

        private string GetSelectedChoiceColumnNames(Dictionary<int, string> options, List<string> optionValues)
        {
            var selectedOptionNames = options.Where(s => optionValues.Contains(s.Key.ToString())).Select(s => s.Value).ToList();
            if (selectedOptionNames.Count > 0)
            {
                return string.Join(",", selectedOptionNames).TrimEnd(',');
            }
            return string.Empty;
        }

        private async Task<string> GetUserNamesAsync(List<int> userIds)
        {
            var userNames = "";
            if (userIds.Count > 0)
            {
                var users = await AccountDao.GetUserByIdsAsync(userIds);
                var userNameList = users.Select(o => o.DisplayName).Distinct().ToList();
                userNames = string.Join(";", userNameList);
            }
            return userNames;
        }

        public void AddRecordReturnLoanHistory(List<RecordReturnLoanDataHistory> entities)
        {
            RecordReturnLoanDataHistoryDao.AddRecordReturnLoanDataHistory(TenantLocalValue.LogonGroupId, entities);
        }

        public async Task<PhysicalReturnHistoryResponse> GetReturnLoanHistory(ReturnLoanHistoryParam param, int limit = -1)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            (var returnHistoryData , var totalCount) = await RecordReturnLoanDataHistoryDao.GetRecordReturnLoanDataHistoryPaginationWithLimit(TenantLocalValue.LogonGroupId, param, limit);
            return new PhysicalReturnHistoryResponse(){
                Datas = returnHistoryData.Select(item => new PhysicalReturnHistory
                {
                    ItemName = item.ItemName,
                    UniqueId = item.UniqueId,
                    RequestBy = item.RequestBy,
                    ReturnTime = GeneralSettingService.ConvertTiksToDateTime(gls, item.ReturnTime, true).SimplifyFormatTime,
                    HomeLocation = item.HomeLocation,
                }).ToList(),
                TotalCount = totalCount};
        }

        public async Task<PhysicalRecordMoveData> BuildPhysicalMoveDataAsync(dynamic item, int status, string comment, string destinationPath, Guid destinationLocationId, string homeLocation)
        {
            var audit = new PhysicalRecordMoveData()
            {
                PartitionKey = item.Id.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                ApproveBy = TenantLocalValue.DisplayName ?? AccountDao.GetUserWithRemovedByPrincipalNames([TenantLocalValue.LogonUserEmail]).FirstOrDefault()?.DisplayName,
                ItemName = item.Name,
                UniqueId = item.RecordId,
                DestinationPath = destinationPath,
                HomeLocationId = item.LocationId,
                DestinationLocationId = destinationLocationId,
                HomeLocation = homeLocation,
                Status = status,
                ExecuteOn = DateTime.UtcNow.Ticks,
                Comment = comment
            };

            return audit;
        }

        public async Task<PickListMoveResultDto> GetMoveData(PickListMoveParam param, int limit = -1)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            (var returnHistoryData, var totalCount) = await PhysicalRecordsMoveDataHistoryTableDao.GetMoveDatasPaginationWithLimit(TenantLocalValue.LogonGroupId, param, limit);
            return new PickListMoveResultDto()
            {
                Datas = returnHistoryData.Select(item => new PickListMoveDto
                {
                    ItemName = item.ItemName,
                    UniqueId = item.UniqueId,
                    ApproveBy = item.ApproveBy,
                    HomeLocation = item.HomeLocation,
                    DestinationLocation = item.DestinationPath,
                    Status = item.Status,
                    Comment = I18NEntity.GetString(item.Comment),
                }).ToList(),
                TotalCount = totalCount
            };
        }
    }
}
