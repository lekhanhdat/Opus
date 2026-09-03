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
using AvePoint.GCommon.Contract.PlatformRecovery;
using AvePoint.Hybrid.ClientLibrary.SDK.Services;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.CustomizeConnector.Model.Api;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager.BuildIn;
using AvePoint.RA.Service.Services.CustomizeConnector.Converters;
using AvePoint.RA.Service.Services.CustomizeConnector.RuleManagement;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using Newtonsoft.Json;
using RazorEngine.Compilation.ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.Api
{
    public class RMCustomizeConnectorApiService : RMServiceBase, IRMCustomizeConnectorApiService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMCustomizeConnectorApiService));

        private static IRMCustomizeConnectorService CustomizeConnectorService => PlatformWindsorManager.GetService<IRMCustomizeConnectorService>();

        private static IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao => PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly ExplorerDao ExplorerDao = new(true);

        public async Task<BaseConnectorApiResponse> InsertData(object connectorDataObj)
        {
            using var performance = new PerformanceScope("RMCustomizeConnector.InsertData");
            try
            {
                if (connectorDataObj == null)
                {
                    return CustomizeConnectorApiResponse.BadRequest(I18NEntity.GetString("RM_Connector_ConnectorObjIllegal"));
                }
                var dataJson = JsonConvert.SerializeObject(connectorDataObj);
                var dataExpandoObj = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
                var connValidateRes = await ValidateConnectorInfo(dataExpandoObj);
                if (!connValidateRes.IsValidated)
                {
                    Logger.Error($"Sumbit record infoes validate failed. Message: {connValidateRes.Message}.");
                    return CustomizeConnectorApiResponse.BadRequest(connValidateRes.Message);
                }

                var conflict = dataExpandoObj.FirstOrDefault(item => item.Key == "confilictOption" || item.Key == "conflictOption");
                if (conflict.Value == null || !Enum.TryParse(conflict.Value.ToString(), true, out CustomizeConnectorConflictOption option))
                {
                    return CustomizeConnectorApiResponse.BadRequest(I18NEntity.GetString("RM_Connector_ConflictOptionObjIllegal"));
                }

                var dataObjList = dataExpandoObj.FirstOrDefault(item => item.Key == "data");
                if (dataObjList.Value == null || dataObjList.Value is not List<object> dataList)
                {
                    return CustomizeConnectorApiResponse.BadRequest(I18NEntity.GetString("RM_Connector_DataListObjIllegal"));
                }

                if (dataList.Count > 15)
                {
                    return CustomizeConnectorApiResponse.BadRequest(string.Format(I18NEntity.GetString("RM_Connector_InsertDataMaxCount"), 15));
                }

                var columnManager = new ConnectorColumnManager(connValidateRes.ConnectorInfo.ColumnInfoes);

                var failedItems = await InsertDataListToExplorer(connValidateRes.ConnectorInfo, columnManager, dataList, option);
                if (failedItems.Any())
                {
                    return CustomizeConnectorApiResponse.SomeDataOperationFailed(failedItems);
                }

                return CustomizeConnectorApiResponse.OK();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while insert customize connector data. Error: {e}");
                return CustomizeConnectorApiResponse.InternalServerError(I18NEntity.GetString("RM_Connector_InsertDatasFailed"));
            }
        }

        public async Task<BaseConnectorApiResponse> GetData(object queryInfo)
        {
            using var performance = new PerformanceScope("RMCustomizeConnector.GetData");
            try
            {
                if (queryInfo == null)
                {
                    return BaseConnectorApiResponse.BadRequest(I18NEntity.GetString("RM_Connector_ConnectorObjIllegal"));
                }
                var dataJson = JsonConvert.SerializeObject(queryInfo);
                var dataExpandoObj = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);

                var format = await GeneralSettingService.GetDateTimeFormatAsync();
                var formats = new[] { format, format.Replace('-', '/') };

                var connValidateRes = ValidateQueryInfo(dataExpandoObj, formats);
                if (!connValidateRes.IsValidated)
                {
                    return BaseConnectorApiResponse.BadRequest(connValidateRes.Message);
                }

                var disposalDueDate = dataExpandoObj.FirstOrDefault(item => item.Key == "disposalDueDate");
                var dueDate = DateTime.ParseExact(disposalDueDate.Value.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None).Ticks;

                var pageSize = dataExpandoObj.FirstOrDefault(item => item.Key == "itemsPerPage");
                _ = int.TryParse(pageSize.Value?.ToString(), out int size);

                var pageIndex = dataExpandoObj.FirstOrDefault(item => item.Key == "startIndex");

                var connectorIdKeyValue = dataExpandoObj.FirstOrDefault(item => item.Key == "id");
                _ = Guid.TryParse(connectorIdKeyValue.Value?.ToString(), out var connectorId);
                var source = await CustomizeConnectorContentSourceDao.Get(connectorId);
                var connectorInfo = await CustomizeConnectorService.GetAsync(connectorId);
                var result = ExplorerDao.QueryByPage(e => 
                e.RecordStatus == (int)RMRecordStatus.Active
                && ((!e.IsManualSynced && e.DisposalStatus == (int)SOApproveDBStatus.None) || (e.IsManualSynced && e.ManualApprovedStatus == (int)SOApproveDBStatus.Approved))
                && e.SourceFlag == source.Flag 
                && e.DisposalDueDate <= dueDate 
                && e.DisposalDueDate != AvePoint.RA.Contract.Common.DueDateUtil.None 
                && e.DisposalDueDate != AvePoint.RA.Contract.Common.DueDateUtil.Pending, size, pageIndex.Value?.ToString());
                if (result != null)
                {
                    var queriedItems = new List<ExpandoObject>();
                    if (result.Item1 != null)
                    {
                        foreach (var record in result.Item1)
                        {
                            queriedItems.Add(await ConnectorRecordCoverter.ConvertRecord2QueryResultAsync(record, connectorInfo));
                        }
                    }
                    return ConnectorApiQueryResponse.QueryResult(queriedItems, result.Item2);
                }
                else
                {
                    return BaseConnectorApiResponse.OK();
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while insert customize connector data. Error: {e}");
                return BaseConnectorApiResponse.InternalServerError(I18NEntity.GetString("RM_Connector_InsertDatasFailed"));
            }
        }

        public async Task<BaseConnectorApiResponse> DisposeRecords(object recordsInfo)
        {
            using var performance = new PerformanceScope("RMCustomizeConnector.DisposeRecords");
            try
            {
                if (recordsInfo == null)
                {
                    return CustomizeConnectorApiResponse.BadRequest(I18NEntity.GetString("RM_Connector_ConnectorDisposalIllegal"));
                }
                var dataJson = JsonConvert.SerializeObject(recordsInfo);
                var dataExpandoObj = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
                var dataObjList = dataExpandoObj.FirstOrDefault(item => item.Key == "disposalIds");
                if (dataObjList.Value == null || dataObjList.Value is not List<object> dataList)
                {
                    return CustomizeConnectorApiResponse.BadRequest(I18NEntity.GetString("RM_Connector_DisposalIdsObjIllegal"));
                }

                if (dataList.Count > 15)
                {
                    return CustomizeConnectorApiResponse.BadRequest(string.Format(I18NEntity.GetString("RM_Connector_InsertDataMaxCount"), 15));
                }

                var failedItems = await DisposeConnectorRecords(dataList.ConvertAll(item => item.ToString()));
                if (failedItems.Any())
                {
                    return CustomizeConnectorApiResponse.SomeDataOperationFailed(failedItems);
                }
                return CustomizeConnectorApiResponse.OK();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while disposing customize connector data. Error: {e}");
                return CustomizeConnectorApiResponse.InternalServerError(I18NEntity.GetString("RM_Connector_InsertDatasFailed"));
            }
        }

        public async Task<BaseConnectorApiV1Response> InsertDataV1(object connectorDataObj)
        {
            using var performance = new PerformanceScope("RMCustomizeConnector.InsertData");
            try
            {
                if (connectorDataObj == null)
                {
                    return ErrorResponse.BadRequest(I18NEntity.GetString("RM_Connector_ConnectorObjIllegal"));
                }
                var dataJson = JsonConvert.SerializeObject(connectorDataObj);
                var dataExpandoObj = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
                var connValidateRes = await ValidateConnectorInfo(dataExpandoObj);
                if (!connValidateRes.IsValidated)
                {
                    Logger.Error($"Sumbit record infoes validate failed. Message: {connValidateRes.Message}.");
                    return ErrorResponse.BadRequest(connValidateRes.Message);
                }

                var conflict = dataExpandoObj.FirstOrDefault(item => item.Key == "confilictOption" || item.Key == "conflictOption");
                if (conflict.Value == null || !Enum.TryParse(conflict.Value.ToString(), true, out CustomizeConnectorConflictOption option))
                {
                    return ErrorResponse.BadRequest(I18NEntity.GetString("RM_Connector_ConflictOptionObjIllegal"));
                }

                var dataObjList = dataExpandoObj.FirstOrDefault(item => item.Key == "data");
                if (dataObjList.Value == null || dataObjList.Value is not List<object> dataList)
                {
                    return ErrorResponse.BadRequest(I18NEntity.GetString("RM_Connector_DataListObjIllegal"));
                }

                if (dataList.Count > 15)
                {
                    return ErrorResponse.BadRequest(string.Format(I18NEntity.GetString("RM_Connector_InsertDataMaxCount"), 15));
                }

                var columnManager = new ConnectorColumnManager(connValidateRes.ConnectorInfo.ColumnInfoes);

                var failedItems = await InsertDataListToExplorerV1(connValidateRes.ConnectorInfo, columnManager, dataList, option);
                if (failedItems.Any())
                {
                    return ExceptionResponse.ExistOperationFailedData(failedItems);
                }

                return SuccessResponse.Created();

            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while insert customize connector data. Error: {e}");
                return ErrorResponse.InternalServerError(I18NEntity.GetString("RM_Connector_InsertDatasFailed"));
            }
        }

        public async Task<BaseConnectorApiV1Response> GetDataV1(string id, string disposalDueDate, int itemsPerPage, string startIndex)
        {
            using var performance = new PerformanceScope("RMCustomizeConnector.GetData");
            try
            {
                var formats = new[] { "MM/dd/yyyy h:mm:ss tt" };
                var connValidateRes = ValidateQueryInfoV1(id, disposalDueDate, itemsPerPage);
                if (!connValidateRes.IsValidated)
                {
                    return ErrorResponse.BadRequest(connValidateRes.Message);
                }

                var dueDate = DateTime.Parse(disposalDueDate).Ticks;

                _ = Guid.TryParse(id, out var connectorId);
                var source = await CustomizeConnectorContentSourceDao.Get(connectorId);
                var connectorInfo = await CustomizeConnectorService.GetAsync(connectorId);
                var result = ExplorerDao.QueryByPage(e =>
                e.RecordStatus == (int)RMRecordStatus.Active
                && ((!e.IsManualSynced && e.DisposalStatus == (int)SOApproveDBStatus.None) || (e.IsManualSynced && e.ManualApprovedStatus == (int)SOApproveDBStatus.Approved))
                && e.SourceFlag == source.Flag
                && e.DisposalDueDate <= dueDate
                && e.DisposalDueDate != AvePoint.RA.Contract.Common.DueDateUtil.None
                && e.DisposalDueDate != AvePoint.RA.Contract.Common.DueDateUtil.Pending, itemsPerPage, startIndex);
                if (result != null)
                {
                    var queriedItems = new List<ExpandoObject>();
                    if (result.Item1 != null)
                    {
                        foreach (var record in result.Item1)
                        {
                            queriedItems.Add(await ConnectorRecordCoverter.ConvertRecord2QueryResultAsync(record, connectorInfo));
                        }
                    }
                    return QueryResponse.QueryResult(queriedItems, result.Item2);
                }
                else
                {
                    return SuccessResponse.NoContent();
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while insert customize connector data. Error: {e}");
                return ErrorResponse.InternalServerError(I18NEntity.GetString("RM_Connector_InsertDatasFailed"));
            }
        }

        public async Task<BaseConnectorApiV1Response> DisposeRecordsV1(object recordsInfo)
        {
            using var performance = new PerformanceScope("RMCustomizeConnector.DisposeRecords");
            try
            {
                if (recordsInfo == null)
                {
                    return ErrorResponse.BadRequest(I18NEntity.GetString("RM_Connector_ConnectorDisposalIllegal"));
                }
                var dataJson = JsonConvert.SerializeObject(recordsInfo);
                var dataExpandoObj = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
                var dataObjList = dataExpandoObj.FirstOrDefault(item => item.Key == "disposalIds");
                if (dataObjList.Value == null || dataObjList.Value is not List<object> dataList)
                {
                    return ErrorResponse.BadRequest(I18NEntity.GetString("RM_Connector_DisposalIdsObjIllegal"));
                }

                if (dataList.Count > 15)
                {
                    return ErrorResponse.BadRequest(string.Format(I18NEntity.GetString("RM_Connector_InsertDataMaxCount"), 15));
                }

                var failedItems = await DisposeConnectorRecords(dataList.ConvertAll(item => item.ToString()));
                if (failedItems.Any())
                {
                    return ExceptionResponse.ExistOperationFailedData(failedItems);
                }

                return SuccessResponse.OK();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while disposing customize connector data. Error: {e}");
                return ErrorResponse.InternalServerError(I18NEntity.GetString("RM_Connector_InsertDatasFailed"));
            }
        }

        private static async Task<CustomizeConnectorValidateResult> ValidateConnectorInfo(ExpandoObject dataExpandoObj)
        {
            var connectorIdKeyValue = dataExpandoObj.FirstOrDefault(item => item.Key == "id");
            if (connectorIdKeyValue.Value == null || !Guid.TryParse(connectorIdKeyValue.Value?.ToString(), out var connectorId))
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_ConnectorIdIllegal"));
            }

            if (!await CustomizeConnectorContentSourceDao.Exist(connectorId))
            {
                return CustomizeConnectorValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connecor_Validate_ConnectorExist"), connectorId));
            }

            var connectorInfo = await CustomizeConnectorService.GetAsync(connectorId);
            return CustomizeConnectorValidateResult.Validated(connectorInfo);
        }


        private CustomizeConnectorValidateResult ValidateQueryInfo(ExpandoObject dataExpandoObj, string[] formats)
        {
            var currentDateTime = DateTime.UtcNow;
            var currentDateTimeTicks = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day).Ticks;
            var disposalDueDate = dataExpandoObj.FirstOrDefault(item => item.Key == "disposalDueDate");

            if (disposalDueDate.Value == null || !DateTime.TryParseExact(disposalDueDate.Value?.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_IllegalQueryParameter", "DisposalDueDate"));
            }

            if (dueDate.Ticks < currentDateTimeTicks)
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connector_Validate_DueDate_LessThan_CurrentDate"));
            }

            var pageSize = dataExpandoObj.FirstOrDefault(item => item.Key == "itemsPerPage");
            if (pageSize.Value == null || !int.TryParse(pageSize.Value?.ToString(), out int size) || (size > 100 || size <= 0))
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_IllegalQueryParameter", "PageSize"));
            }

            var pageIndex = dataExpandoObj.FirstOrDefault(item => item.Key == "startIndex");
            if (pageIndex.Value == null)
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_IllegalQueryParameter", "PageIndex"));
            }

            var connectorIdKeyValue = dataExpandoObj.FirstOrDefault(item => item.Key == "id");
            if (connectorIdKeyValue.Value == null || !Guid.TryParse(connectorIdKeyValue.Value?.ToString(), out var connectorId))
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_IllegalQueryParameter", "ConnectorId"));
            }
            return CustomizeConnectorValidateResult.Validated(null);
        }

        private CustomizeConnectorValidateResult ValidateQueryInfoV1(string id, string disposalDueDate, int itemsPerPage)
        {
            var currentDateTime = DateTime.UtcNow;
            var currentDateTimeTicks = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day).Ticks;

            if (string.IsNullOrEmpty(disposalDueDate) || !DateTime.TryParse(disposalDueDate, out var dueDate))
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_IllegalQueryParameter", "DisposalDueDate"));
            }

            if (dueDate.Ticks < currentDateTimeTicks)
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connector_Validate_DueDate_LessThan_CurrentDate"));
            }

            if (itemsPerPage > 100 || itemsPerPage <= 0)
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_IllegalQueryParameter", "PageSize"));
            }

            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var connectorId))
            {
                return CustomizeConnectorValidateResult.UnValidated(I18NEntity.GetString("RM_Connecor_Validate_IllegalQueryParameter", "ConnectorId"));
            }
            return CustomizeConnectorValidateResult.Validated(null);
        }

        private async Task<List<CustomizeConnectorApiFailedItem>> InsertDataListToExplorer(CustomizeConnectorInfo connectorInfo,
            ConnectorColumnManager columnManager,
            List<object> dataObjList, CustomizeConnectorConflictOption option)
        {
            var res = new List<CustomizeConnectorApiFailedItem>();
            var idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, dataObjList.Count);
            var termRuleInfoManager = new ConnectorTermRuleInfoManagement();
            foreach (var dataObj in dataObjList)
            {
                try
                {
                    var columnDataValidateRes = await columnManager.ColumnDataValidateAsync(dataObj);
                    if (!columnDataValidateRes.IsValidated)
                    {
                        res.Add(new CustomizeConnectorApiFailedItem
                        {
                            Item = dataObj,
                            Message = columnDataValidateRes.Message
                        });
                        continue;
                    }

                    var customColumnDic = await columnManager.ConvertToExplorerColumnDicAsync(dataObj);
                    var rowKey = customColumnDic[CustomizeConnectorBuildColumnIds.RowKey.ToString()].Value;

                    var record = ExplorerDao.GetFirstOrDefault(r => r.RowKey.Equals(rowKey, StringComparison.OrdinalIgnoreCase));

                    if (option == CustomizeConnectorConflictOption.Skip && record != null)
                    {
                        Logger.Error($"Failed to insert data to explorer, has duplicate item id: [{rowKey}]");
                        res.Add(new CustomizeConnectorApiFailedItem
                        {
                            Item = dataObj,
                            Message = string.Format(I18NEntity.GetString("RM_Connecor_Validate_RecordExist"), rowKey),
                        });
                        continue;
                    }

                    if (option == CustomizeConnectorConflictOption.Overwrite && record != null && record.ContainerId != connectorInfo.Id.ToString()) 
                    {
                        Logger.Error($"Failed to insert data to explorer, has duplicate item id: [{rowKey}] in different connector.");
                        res.Add(new CustomizeConnectorApiFailedItem
                        {
                            Item = dataObj,
                            Message = string.Format(I18NEntity.GetString("RM_Connecor_Validate_RecordExistInOtherConnector"), rowKey),
                        });
                        continue;
                    }

                    record ??= new Record
                    {
                        Id = Guid.NewGuid(),
                        RecordsId = idUtil.GenerateUniqueId(),
                        ContainerId = connectorInfo.Id.ToString(),
                        SourceFlag = connectorInfo.Flag,
                        NodeType = (int)RMNodeLevel.CustomizeConnectorItem,
                        ExtensionForFile = "RM_Connector_ItemLevel_Item",
                    };

                    record.CollectTime = DateTime.UtcNow.Ticks;
                    record.RecordStatus = 1;

                    ConnectorBuildInColumnManager.ApplyRecordValues(record, customColumnDic);
                    var rulePolicyValues = columnManager.ConvertToRulePolicy(customColumnDic);
                    termRuleInfoManager.ApplyRule(record, rulePolicyValues);
                    record.CustomColumnDic = customColumnDic;
                    await ExplorerDao.UpsertAsync(record);
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while insert item to explorer. Item: [{JsonConvert.SerializeObject(dataObj)}] Error: {e}");
                    res.Add(new CustomizeConnectorApiFailedItem
                    {
                        Item = dataObj,
                        Message = I18NEntity.GetString("RM_Connector_InsertToExplorerFailed")
                    });
                }
            }
            return res;
        }        
        
        private async Task<List<CustomizeConnectorApiFailedItem>> InsertDataListToExplorerV1(CustomizeConnectorInfo connectorInfo,
            ConnectorColumnManager columnManager,
            List<object> dataObjList, CustomizeConnectorConflictOption option)
        {
            var res = new List<CustomizeConnectorApiFailedItem>();
            var idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, dataObjList.Count);
            var termRuleInfoManager = new ConnectorTermRuleInfoManagement();
            foreach (var dataObj in dataObjList)
            {
                try
                {
                    var columnDataValidateRes = await columnManager.ColumnDataValidateV1Async(dataObj);
                    if (!columnDataValidateRes.IsValidated)
                    {
                        res.Add(new CustomizeConnectorApiFailedItem
                        {
                            Item = dataObj,
                            Message = columnDataValidateRes.Message
                        });
                        continue;
                    }

                    var customColumnDic = await columnManager.ConvertToExplorerColumnDicV1Async(dataObj);
                    var rowKey = customColumnDic[CustomizeConnectorBuildColumnIds.RowKey.ToString()].Value;

                    var record = ExplorerDao.GetFirstOrDefault(r => r.RowKey.Equals(rowKey, StringComparison.OrdinalIgnoreCase));

                    if (option == CustomizeConnectorConflictOption.Skip && record != null)
                    {
                        Logger.Error($"Failed to insert data to explorer, has duplicate item id: [{rowKey}]");
                        res.Add(new CustomizeConnectorApiFailedItem
                        {
                            Item = dataObj,
                            Message = string.Format(I18NEntity.GetString("RM_Connecor_Validate_RecordExist"), rowKey),
                        });
                        continue;
                    }

                    if (option == CustomizeConnectorConflictOption.Overwrite && record != null && record.ContainerId != connectorInfo.Id.ToString()) 
                    {
                        Logger.Error($"Failed to insert data to explorer, has duplicate item id: [{rowKey}] in different connector.");
                        res.Add(new CustomizeConnectorApiFailedItem
                        {
                            Item = dataObj,
                            Message = string.Format(I18NEntity.GetString("RM_Connecor_Validate_RecordExistInOtherConnector"), rowKey),
                        });
                        continue;
                    }

                    record ??= new Record
                    {
                        Id = Guid.NewGuid(),
                        RecordsId = idUtil.GenerateUniqueId(),
                        ContainerId = connectorInfo.Id.ToString(),
                        SourceFlag = connectorInfo.Flag,
                        NodeType = (int)RMNodeLevel.CustomizeConnectorItem,
                        ExtensionForFile = "RM_Connector_ItemLevel_Item",
                    };

                    record.CollectTime = DateTime.UtcNow.Ticks;
                    record.RecordStatus = 1;

                    ConnectorBuildInColumnManager.ApplyRecordValues(record, customColumnDic);
                    var rulePolicyValues = columnManager.ConvertToRulePolicy(customColumnDic);
                    termRuleInfoManager.ApplyRule(record, rulePolicyValues);
                    record.CustomColumnDic = customColumnDic;
                    await ExplorerDao.UpsertAsync(record);
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while insert item to explorer. Item: [{JsonConvert.SerializeObject(dataObj)}] Error: {e}");
                    res.Add(new CustomizeConnectorApiFailedItem
                    {
                        Item = dataObj,
                        Message = I18NEntity.GetString("RM_Connector_InsertToExplorerFailed")
                    });
                }
            }
            return res;
        }

        private async Task<List<CustomizeConnectorApiFailedItem>> DisposeConnectorRecords(List<string> connectorUniqueIds)
        {
            var failedItems = new List<CustomizeConnectorApiFailedItem>();
            var items = ExplorerDao.QueryAll(item => connectorUniqueIds.Contains(item.RowKey));
            var existItemsRowKey = items.Select(item => item.RowKey).ToList();
            var notExistItemsRowKey = connectorUniqueIds.Except(existItemsRowKey);

            failedItems.AddRange(notExistItemsRowKey.ToList().ConvertAll(item => new CustomizeConnectorApiFailedItem
            {
                Item = connectorUniqueIds,
                Message = I18NEntity.GetString("RM_Connector_ItemNotFound")
            }));

            var now = DateTime.UtcNow.Ticks;

            foreach(var item in items)
            {
                try
                {
                    if(item.IsManualSynced)
                    {
                        item.ManualArchiveStatus = (int)ActionStatus.Archiverd;
                        item.ManualArchivedTime = now;

                        if (item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                        {
                            failedItems.Add(new CustomizeConnectorApiFailedItem
                            {
                                Item = item.RowKey,
                                Message = I18NEntity.GetString("RM_Connector_ItemIsWaitingApprove"),
                            });
                            continue;
                        }

                        if (item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                        {
                            failedItems.Add(new CustomizeConnectorApiFailedItem
                            {
                                Item = item.RowKey,
                                Message = I18NEntity.GetString("RM_Connector_ItemIsRejected"),
                            });
                            continue;
                        }
                    }

                    if(!item.IsManualSynced && item.DisposalStatus == (int)SOApproveDBStatus.WaitingApprove)
                    {
                        failedItems.Add(new CustomizeConnectorApiFailedItem
                        {
                            Item = item.RowKey,
                            Message = I18NEntity.GetString("RM_Connector_ItemIsWaitingApproveForDisposal"),
                        });
                        continue;
                    }

                    if(item.RuleId != Guid.Empty)
                    {
                        item.DisposalStatus = (int)SOApproveDBStatus.Archived;
                    }

                    item.RecordStatus = (int)RMRecordStatus.Destroyed;
                    item.DestroyedTime = now;
                    await ExplorerDao.UpsertAsync(item);
                }
                catch(Exception e)
                {
                    Logger.Error($"Error occurred while disposing connector record. Id:{item.RowKey} Error:{e}");
                    failedItems.Add(new CustomizeConnectorApiFailedItem
                    {
                        Item = item.RowKey,
                        Message = I18NEntity.GetString("RM_Connector_DisposalItemFailed")
                    });
                }
            }

            return failedItems;
        }
    }
}
