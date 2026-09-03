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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.DB.Model;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using Newtonsoft.Json;
using AvePoint.GCommon.Utility;
using Microsoft.Graph;
using AvePoint.RA.Contract;
using Azure.Data.Tables;
using AvePoint.RA.Common.Util;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ArchiverTableDao : IArchiverTableDao
    {       

        private const string _SOArchiverTablePrefix = "SOArchiverDB";
        private const string _SOArchiverTablePrefixForEXO = "SOExchangeOnlineDB";
        private const string _SOArchiverTablePrefixForFS = "SOFSArchiverDB";
        private const string _SOArchiverTablePrefixForOnPremiseSP = "SOOnPremiseSPArchiverDB";
        private const string _SOStaticArchiverTablePrefix = "SOStaticArchiverDB";
        private const string _SOStaticArchiverTablePrefixForEXO = "SOStaticExchangeOnlineDB";
        private const string _SOStaticArchiverTablePrefixForFS = "SOStaticFSArchiverDB";
        private const string _SOStaticArchiverTablePrefixForOnPremiseSP = "SOStaticOnPremiseSPArchiverDB";
        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ArchiverTableDao));
        public IGeneralSettingDao GeneralSettingDao { get; set; }
        public void DeleteItemsByRowKey(AzureTableConnectContract connectionInfo, string tenantGroupId, string siteId, List<string> itemRowKeys)
        {
            string filterCondition = null;
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(siteId).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<ArchiverTableEntity> entities;
            var count = 0;
            foreach (var rowKey in itemRowKeys)
            {
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                count++;
                if (count == 100)
                {
                    filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                    entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).ToList();
                    AzureTableStorageUtility.DeleteTableEntities(connectStr, tableName, entities);
                    keyBuilder = new AzureTableQueryConditionBuilder();
                    count = 0;
                }
            }

            if (count > 0)
            {
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).ToList();
                AzureTableStorageUtility.DeleteTableEntities(connectStr, tableName, entities);
            }
        }

        public async Task UpdateItemsToApprovedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys)
        {
            await UpdateItemsAsync(connectionInfo, tenantGroupId, sitePath, itemRowKeys, e => e.Status = (int)SOApproveDBStatus.Approved);
        }
        public async Task UpdateItemsToRejectedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys)
        {
            await UpdateItemsAsync(connectionInfo, tenantGroupId, sitePath, itemRowKeys, e => e.Status = (int)SOApproveDBStatus.Rejected);
        }

        public async Task UpdateItemsToExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys, SourceFlag sourceFlag = SourceFlag.SharePoint)
        {
            switch (sourceFlag)
            {
                case SourceFlag.Exchange:
                    await UpdateItemsForEXOAsync(connectionInfo, tenantGroupId, sitePath, itemRowKeys, e => e.ExportToRECO = true);
                    break;
                case SourceFlag.SharePoint:
                default:
                    await UpdateItemsAsync(connectionInfo, tenantGroupId, sitePath, itemRowKeys, e => e.ExportToRECO = true);
                    break;
            }

        }

        public async Task UpdateItemsToExportedStatusForSPOnPremAsync(string connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys)
        {
            await UpdateItemsForSPOnPremAsync(connectionInfo, tenantGroupId, sitePath, itemRowKeys, e => e.MovedToApprovalTable = true);
        }
        
        public async Task UpdateItemsToExportedStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string sitePath, List<string> itemRowKeys)
        {
            await UpdateItemsForFSAsync(fsAzureTableConnectStr, tenantGroupId, sitePath, itemRowKeys, e => e.MovedToApprovalTable = true);
        }

        public async Task UpdateItemsToNotExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys)
        {
            await UpdateItemsAsync(connectionInfo, tenantGroupId, sitePath, itemRowKeys, e => e.ExportToRECO = false);
        }

        public List<ArchiverTableEntity> GetWaitingApprovalDatas(AzureTableConnectContract connectionInfo, string tenantGroupId, SourceFlag source)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.WaitingApprove, AzureDataType.Int);
            builder.AppendAndQuery(ArchiverTableEntityProperty.ExportToRECO, AzureQueryComparisons.NotEqual, true, AzureDataType.Bool);
            builder.AppendAndQuery(ArchiverTableEntityProperty.SourceFlag, AzureQueryComparisons.Equal, source, AzureDataType.Int);
            builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.NotEqual, 10001, AzureDataType.Int);
            builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.NotEqual, 20000, AzureDataType.Int);
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableName(tenantGroupId);
            return AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, builder.ToString()).ToList();
        }
        public List<ArchiverExchangeOnlineDto> GetWaitingApprovalDatasForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.WaitingApprove, AzureDataType.Int);
            builder.AppendAndQuery(ArchiverTableEntityProperty.ExportToRECO, AzureQueryComparisons.NotEqual, true, AzureDataType.Bool);
            builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.Equal, 700, AzureDataType.Int);
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableNameForEXO(tenantGroupId);
            return AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverExchangeOnlineDto>(connectStr, tableName, builder.ToString()).ToList();
        }

        public List<FileSystemTableEntity> GetWaitingApprovalDatasForFS(string fsAzureTableConnectStr, string tenantGroupId)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.WaitingApprove, AzureDataType.Int);
            builder.AppendAndQuery(ArchiverTableEntityProperty.MoveToApprovalTable, AzureQueryComparisons.NotEqual, true, AzureDataType.Bool);
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            return AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(fsAzureTableConnectStr, tableName, builder.ToString()).ToList();
        }

        public List<OnPremiseSPTableEntity> GetWaitingApprovalDatasForSPOnPrem(string connectString, string tenantGroupId)
        {
            AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
            builder.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.WaitingApprove, AzureDataType.Int);
            builder.AppendAndQuery(ArchiverTableEntityProperty.MoveToApprovalTable, AzureQueryComparisons.NotEqual, true, AzureDataType.Bool);
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            return AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectString, tableName, builder.ToString()).ToList();
        }
        public async Task<Dictionary<string, long>> GetDestroyedRecordsAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, Dictionary<Guid, string> physicalListIds, List<string> siteCollectionIds)
        {
            //get from sharepoint settings.....
            AzureTableQueryConditionBuilder phybuilder = new AzureTableQueryConditionBuilder();
            string otherCondition = string.Empty;
            var setting = await GeneralSettingDao.GetGeneralSettingByUserAsync(tenantGroupId);
            Dictionary<string, long> result = new Dictionary<string, long>();
            var partSiteCollectionIds = new List<string>();
            for (int i = 0; i < siteCollectionIds.Count; i++)
            {
                partSiteCollectionIds.Add(siteCollectionIds[i]);
                if (partSiteCollectionIds.Count == 200 || i == siteCollectionIds.Count - 1)//200
                {
                    logger.Info("get data from archiver table , index is {0}", i);
                    foreach (var sitecollectionId in partSiteCollectionIds)
                    {
                        AzureTableQueryConditionBuilder otherBuilder = new AzureTableQueryConditionBuilder();
                        otherBuilder.AppendAndQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, sitecollectionId);
                        if (string.IsNullOrEmpty(otherCondition))
                        {
                            otherCondition = "(" + otherBuilder.ToString();
                        }
                        else
                        {
                            //otherCondition = AzureTableQueryConditionBuilder.CombineOrQueries(otherCondition, otherBuilder.ToString());
                            otherCondition = otherCondition + " or " + otherBuilder.ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(otherCondition))
                    {
                        otherCondition = otherCondition + ")";
                        AzureTableQueryConditionBuilder otherConditionBuilder = new AzureTableQueryConditionBuilder();
                        //只查找destroyed的数据。
                        AzureTableQueryConditionBuilder oldRowkey = new AzureTableQueryConditionBuilder();
                        oldRowkey.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.GreaterThanOrEqual, "5_", AzureDataType.String);
                        oldRowkey.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.LessThan, "6_", AzureDataType.String);
                        AzureTableQueryConditionBuilder newRowkey = new AzureTableQueryConditionBuilder();
                        newRowkey.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.GreaterThanOrEqual, "New_5_1", AzureDataType.String);
                        newRowkey.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.LessThan, "New_5_2", AzureDataType.String);
                        var rowKeyCondition = string.Format(" and ({0} or {1})", oldRowkey, newRowkey);
                        otherCondition += rowKeyCondition;

                        otherConditionBuilder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.GreaterThanOrEqual, 1, AzureDataType.Int);
                        otherConditionBuilder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.LessThanOrEqual, 10000, AzureDataType.Int);
                        otherCondition = AzureTableQueryConditionBuilder.CombineAndQueries(otherCondition, otherConditionBuilder.ToString());
                    }
                    string allQuery = string.Empty;
                    allQuery = otherCondition;
                    string connectStr = GetConnectString(connectionInfo);
                    string tableName = GetStaticArchiverApprovalTableName(tenantGroupId);
                    logger.Info("allQuery length is : {0}, Text  is : {1}", allQuery?.Length, allQuery);
                    var azureData = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, allQuery);
                    var Entities = azureData.GroupBy(g => GetArchiveTime(g.RowKey, setting)).ToList();
                    foreach (var entity in Entities)
                    {
                        result.Add(entity.Key, entity.LongCount());//to do next
                    }
                    partSiteCollectionIds.Clear();
                    otherCondition = string.Empty;
                }
            }
            return result;
        }
        public string GetArchiveTime(string rowkey, RMCPGeneralSetting setting)
        {
            var timeZone = "UTC";
            var dayLight = false;
            if (setting != null)
            {
                timeZone = setting.TimeZone;
                dayLight = setting.DayLight;
            }
            string[] rowarray = rowkey.Split('_');
            DateTime time;
            long tickets = rowarray[0] == "New" ? Convert.ToInt64(rowarray[3]) : Convert.ToInt64(rowarray[1]);
            time = Common.Util.DateTimeUtil.ConvertTimeFromUtc(tickets, timeZone, !dayLight);

            return time.ToString("d");
        }
        public List<ArchiverTableEntity> GetDestroyedItemsByListId(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, Guid listId, DateTime startTime, DateTime endTime, bool isPhysicalLibrary)
        {
            List<ArchiverTableEntity> allDatas = new List<ArchiverTableEntity>();
            var newAndOldRowKey = new string[] { "5_{0}", "New_5_1_{0}", "New_5_3_{0}" };
            foreach (var rowKeyFormat in newAndOldRowKey)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
                string siteIdCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, partitionKey, AzureDataType.String);
                string listIdCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.ScopeID, AzureQueryComparisons.Equal, listId, AzureDataType.Guid);

                string archivedItemRowKeyFormat = rowKeyFormat;
                builder.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.GreaterThan, string.Format(archivedItemRowKeyFormat, startTime.Ticks.ToString()), AzureDataType.String);
                builder.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.LessThan, string.Format(archivedItemRowKeyFormat, endTime.Ticks.ToString()), AzureDataType.String);

                if (isPhysicalLibrary)
                {
                    builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.GreaterThanOrEqual, 1001, AzureDataType.Int);
                    builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.LessThanOrEqual, 10000, AzureDataType.Int);
                }
                else
                {
                    builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.Equal, 10000, AzureDataType.Int);
                }
                string condition = AzureTableQueryConditionBuilder.CombineAndQueries(listIdCondition, builder.ToString());
                condition = AzureTableQueryConditionBuilder.CombineAndQueries(condition, siteIdCondition);
                string connectStr = GetConnectString(connectionInfo);
                string tableName = GetStaticArchiverApprovalTableName(tenantGroupId);
                allDatas.AddRange(AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, condition).ToList());
                logger.Info($"GetDestroyedItemsByListId condition is {condition}, total:{allDatas.Count}");
            }
            return allDatas;
        }

        public List<ArchiverTableEntity> GetDestroyedItemsByListIdForOneDrive(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, Guid listId, DateTime startTime, DateTime endTime)
        {
            List<ArchiverTableEntity> allDatas = new List<ArchiverTableEntity>();
            var newAndOldRowKey = new string[] { "5_{0}", "New_5_1_{0}", "New_5_3_{0}" };
            foreach (var rowKeyFormat in newAndOldRowKey)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
                string siteIdCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, partitionKey, AzureDataType.String);
                string listIdCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.ScopeID, AzureQueryComparisons.Equal, listId, AzureDataType.Guid);

                string archivedItemRowKeyFormat = rowKeyFormat;
                builder.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.GreaterThan, string.Format(archivedItemRowKeyFormat, startTime.Ticks.ToString()), AzureDataType.String);
                builder.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.LessThan, string.Format(archivedItemRowKeyFormat, endTime.Ticks.ToString()), AzureDataType.String);
                builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.Equal, 10000, AzureDataType.Int);
                builder.AppendAndQuery(ArchiverTableEntityProperty.SourceFlag, AzureQueryComparisons.Equal, (int)SourceFlag.OneDrive, AzureDataType.Int);
                string condition = AzureTableQueryConditionBuilder.CombineAndQueries(listIdCondition, builder.ToString());
                condition = AzureTableQueryConditionBuilder.CombineAndQueries(condition, siteIdCondition);
                string connectStr = GetConnectString(connectionInfo);
                string tableName = GetStaticArchiverApprovalTableName(tenantGroupId);
                logger.Info("GetDestroyedItemsByListId condition is {0}", condition);
                allDatas.AddRange(AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, condition).ToList());
            }
            return allDatas;
        }

        private async Task UpdateItemsAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys, Action<ArchiverTableEntity> changeFunc)
        {
            string filterCondition = null;
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(sitePath).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<ArchiverTableEntity> entities;
            var count = 0;
            foreach (var rowKey in itemRowKeys)
            {
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                count++;
                if (count == 100)
                {
                    filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                    entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).ToList();
                    entities.ForEach(changeFunc);
                    await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
                    keyBuilder = new AzureTableQueryConditionBuilder();
                    count = 0;
                }
            }
            if (count > 0)
            {
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).ToList();
                entities.ForEach(changeFunc);
                await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
            }
        }

        private async Task UpdateItemsForSPOnPremAsync(string connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys, Action<OnPremiseSPTableEntity> changeFunc)
        {
            string filterCondition = null;
            string connectStr = connectionInfo;
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(sitePath).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<OnPremiseSPTableEntity> entities;
            var count = 0;
            foreach (var rowKey in itemRowKeys)
            {
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                count++;
                if (count == 100)
                {
                    filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                    entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition).ToList();
                    entities.ForEach(changeFunc);
                    await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
                    keyBuilder = new AzureTableQueryConditionBuilder();
                    count = 0;
                }
            }
            if (count > 0)
            {
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition).ToList();
                entities.ForEach(changeFunc);
                await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
            }
        }

        private async Task UpdateItemsForEXOAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string sitePath, List<string> itemRowKeys, Action<ExchangeOnlineTableEntity> changeFunc)
        {
            string filterCondition = null;
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableNameForEXO(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(sitePath).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<ExchangeOnlineTableEntity> entities;
            var count = 0;
            foreach (var rowKey in itemRowKeys)
            {
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                count++;
                if (count == 100)
                {
                    filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                    entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ExchangeOnlineTableEntity>(connectStr, tableName, filterCondition).ToList();
                    entities.ForEach(changeFunc);
                    await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
                    keyBuilder = new AzureTableQueryConditionBuilder();
                    count = 0;
                }
            }

            if (count > 0)
            {
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ExchangeOnlineTableEntity>(connectStr, tableName, filterCondition).ToList();
                entities.ForEach(changeFunc);
                await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
            }
        }

        private async Task UpdateItemsForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partionKey, List<string> itemRowKeys, Action<FileSystemTableEntity> changeFunc)
        {
            string filterCondition = null;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partionKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<FileSystemTableEntity> entities;
            var count = 0;
            foreach (var rowKey in itemRowKeys)
            {
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                count++;
                if (count == 100)
                {
                    filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                    entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(fsAzureTableConnectStr, tableName, filterCondition).ToList();
                    entities.ForEach(changeFunc);
                    await AzureTableStorageUtility.UpdateTableEnitiesAsync(fsAzureTableConnectStr, tableName, entities);
                    keyBuilder = new AzureTableQueryConditionBuilder();
                    count = 0;
                }
            }

            if (count > 0)
            {
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(fsAzureTableConnectStr, tableName, filterCondition).ToList();
                entities.ForEach(changeFunc);
                await AzureTableStorageUtility.UpdateTableEnitiesAsync(fsAzureTableConnectStr, tableName, entities);
            }
        }

        private string GetConnectString(AzureTableConnectContract info)
        {
            if (string.IsNullOrEmpty(info.AccountKey) || string.IsNullOrEmpty(info.AccountName))
            {
                logger.Info("Use managed identity authentication table connection string");
                return info.Endpoint;
            }

            if (!string.IsNullOrEmpty(info.Endpoint))
            {
                return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};TableEndpoint={2}", info.AccountName, info.AccountKey, info.Endpoint);
            }
            else
            {
                return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", info.AccountName, info.AccountKey);
            }
        }

        private string GetArchiverApprovalTableName(string tenantGroupId)
        {
            //return "SOArchiverDBabbd956f716944d890c48d19afbbdce5";
            return string.Concat(_SOArchiverTablePrefix, tenantGroupId.Replace("-", string.Empty));
        }

        private string GetArchiverApprovalTableNameForEXO(string tenantGroupId)
        {
            //return "SOArchiverDBabbd956f716944d890c48d19afbbdce5";
            return string.Concat(_SOArchiverTablePrefixForEXO, tenantGroupId.Replace("-", string.Empty));
        }


        private string GetArchiverApprovalTableNameForFS(string tenantGroupId)
        {
            //return "SOArchiverDBabbd956f716944d890c48d19afbbdce5";
            return string.Concat(_SOArchiverTablePrefixForFS, tenantGroupId.Replace("-", string.Empty));
        }

        private string GetArchiverApprovalTableNameForOnPremiseSP(string tenantGroupId)
        {
            //return "SOArchiverDBabbd956f716944d890c48d19afbbdce5";
            return string.Concat(_SOArchiverTablePrefixForOnPremiseSP, tenantGroupId.Replace("-", string.Empty));
        }

        private string GetStaticArchiverApprovalTableName(string tenantGroupId)
        {
            return string.Concat(_SOStaticArchiverTablePrefix, tenantGroupId.Replace("-", string.Empty));
        }

        private string GetStaticArchiverApprovalTableNameForEXO(string tenantGroupId)
        {
            //return "SOArchiverDBabbd956f716944d890c48d19afbbdce5";
            return string.Concat(_SOStaticArchiverTablePrefixForEXO, tenantGroupId.Replace("-", string.Empty));
        }

        private string GetStaticArchiverApprovalTableNameForFS(string tenantGroupId)
        {
            //return "SOArchiverDBabbd956f716944d890c48d19afbbdce5";
            return string.Concat(_SOStaticArchiverTablePrefixForFS, tenantGroupId.Replace("-", string.Empty));
        }

        private string GetStaticArchiverApprovalTableNameForOnPremiseSP(string tenantGroupId)
        {
            //return "SOArchiverDBabbd956f716944d890c48d19afbbdce5";
            return string.Concat(_SOStaticArchiverTablePrefixForOnPremiseSP, tenantGroupId.Replace("-", string.Empty));
        }

        public ArchiverTableEntity GetArchiverItem(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableName(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            ArchiverTableEntity entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, itemRowKey);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());
            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }

        #region fs

        public FileSystemTableEntity GetArchiverItemForFS(string connectString, string tenantGroupId, string partitionKey, string itemRowKey)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            FileSystemTableEntity entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, itemRowKey);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());
            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }

        public IEnumerable<FileSystemTableEntity> AddArchiverItemsForFS(string connectString, string tenantGroupId, List<FileSystemTableEntity> entities)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            string staticTableName = GetStaticArchiverApprovalTableNameForFS(tenantGroupId);

            //add archived data to static table
            var archivedEntities = entities.Where(t => t.Status == (int)SOApproveDBStatus.Archived).ToList();
            if (archivedEntities.IsNotNullOrEmpty())
            {
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.AddArchivedAzureTableEntities"))
                {
                    AzureTableStorageUtility.AddAzureTableEntities<FileSystemTableEntity>(connectStr, staticTableName, archivedEntities);
                }
                logger.Info($"Archived records count:{archivedEntities.Count}");
                //remove archived data from archiver table
                try
                {
                    using (new AvePoint.RA.Common.PerformanceScope("FSScanData.BatchDeleteTableEntities"))
                    {
                        AzureTableStorageUtility.DeleteTableEntities(connectString, tableName, archivedEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Batch delete archived data failed, error:{e.ToString()}");
                    using (new AvePoint.RA.Common.PerformanceScope("FSScanData.SingleDeleteTableEntities"))
                    {
                        foreach (var entity in archivedEntities)
                        {
                            try
                            {
                                AzureTableStorageUtility.DeleteTableEntity(connectString, tableName, entity);
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Delete archived data failed. NodeId:{entity.RowKey} Error:{ex.ToString()}");
                            }
                        }
                    }
                }
                //DeleteEntitiesByRowkeys(connectStr, tableName, archivedEntities.FirstOrDefault().PartitionKey.ToString(), archivedEntities.Select(e => e.RowKey).ToList());
            }

            var otherEntities = entities.Where(t => t.Status != (int)SOApproveDBStatus.Archived).ToList();
            IEnumerable<FileSystemTableEntity> mEntities = null;
            if (otherEntities.IsNotNullOrEmpty())
            {
                //add other data to archiver table              
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.AddAzureTableEntities"))
                {
                    mEntities = AzureTableStorageUtility.AddAzureTableEntities<FileSystemTableEntity>(connectStr, tableName, otherEntities);
                }
                logger.Info($"Normal records count:{otherEntities.Count}");
                //DeleteEntitiesByRowkeys(connectStr, staticTableName, otherEntities.FirstOrDefault().PartitionKey.ToString(), otherEntities.Select(e => e.RowKey).ToList());
            }            
            return mEntities;
        }

            

        public void RemoveArchiverItemsForFS(string connectString, string tenantGroupId, List<FileSystemTableEntity> entities)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            try
            {
                List<FileSystemTableEntity> deleteEntities;
                var count = 0;
                string partionCondition = new AzureTableQueryConditionBuilder(entities[0].PartitionKey.ToString()).ToString();
                AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
                List<Guid> tempIds = new List<Guid>();
                string filterCondition;
                var itemRowKeys = entities.Select(e => e.RowKey).ToList();
                foreach (var rowKey in itemRowKeys)
                {
                    keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                    tempIds.Add(new Guid(rowKey));
                    count++;
                    if (count == 100)
                    {
                        try
                        {
                            filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                            deleteEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).ToList();
                            if (deleteEntities.Count > 0)
                            {
                                AzureTableStorageUtility.DeleteTableEntities(connectStr, tableName, deleteEntities);
                            }
                            keyBuilder = new AzureTableQueryConditionBuilder();
                            count = 0;
                        }
                        catch (Exception e)
                        {
                            logger.Error("Failed to update rejected items for fs. Error:{0}", e.ToString());
                            tempIds.Clear();
                        }
                    }
                }
                if (count > 0)
                {
                    try
                    {
                        filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                        deleteEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).ToList();
                        if (deleteEntities.Count > 0)
                        {
                            AzureTableStorageUtility.DeleteTableEntities(connectStr, tableName, deleteEntities);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to update rejected items for fs. Error:{0}", e.ToString());
                        tempIds.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while deleting item from static table. Error:{0}", e.ToString());
            }            
        }
        public FileSystemTableEntity AddArchiverItemForFS(string connectString, string tenantGroupId, FileSystemTableEntity entity)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            string staticTableName = GetStaticArchiverApprovalTableNameForFS(tenantGroupId);
            var mEntity = AzureTableStorageUtility.AddAzureTableEntity<FileSystemTableEntity>(connectStr, tableName, entity);

            try
            {
                string partionCondition = new AzureTableQueryConditionBuilder(entity.PartitionKey.ToString()).ToString();
                AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, entity.RowKey);
                var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                var deleteEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, staticTableName, filterCondition).ToList();
                if (deleteEntities.Count > 0)
                {
                    AzureTableStorageUtility.DeleteTableEntities(connectStr, staticTableName, deleteEntities);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to remove entity from static table.Rowkey:{0} Error:{1}", entity?.RowKey, e.ToString());
            }
            return mEntity;
        }

        public IEnumerable<FileSystemTableEntity> GetArchiveItemsByPageForFS(string connectString, string tenantGroupId, string jobId, int pageIndex, int pageSize)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            AzureTableQueryConditionBuilder jobIdfilter = new AzureTableQueryConditionBuilder();
            jobIdfilter.AppendOrQuery(ArchiverTableEntityProperty.ScanJobID, AzureQueryComparisons.Equal, jobId);
            //AzureTableQueryConditionBuilder ruleIdfilter = new AzureTableQueryConditionBuilder();
            //ruleIdfilter.AppendOrQuery(ArchiverTableEntityProperty.RuleID, AzureQueryComparisons.Equal, ruleId);
            //string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(jobIdfilter.ToString(), ruleIdfilter.ToString());
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, jobIdfilter.ToString())            
                .OrderBy(e => e.SortTicks)
                .Skip(pageSize * pageIndex)
                .Take(pageSize);
            return result;
        }

        public IEnumerable<FileSystemTableEntity> GetAzureDataByFolderForFS(string connectString, string tenantGroupId, string folderId, string scopeId, long sortTicks, int pageSize)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(scopeId.ToString()).ToString();
            AzureTableQueryConditionBuilder folderIdfilter = new AzureTableQueryConditionBuilder();
            folderIdfilter.AppendOrQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, new Guid(folderId), AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, folderIdfilter.ToString());
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition.ToString())
               .OrderBy(e => e.SortTicks)
               .Where(e => e.SortTicks > sortTicks)
               .Take(pageSize);
            return result;
        }

        public int MoveRecordsToStaticForConnectionForFS(string connectString, string tenantGroupId, string connectionPath, string scopeId)
        {
            try
            {
                string connectStr = connectString;
                string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
                string staticTableName = GetStaticArchiverApprovalTableNameForFS(tenantGroupId);
                string partionCondition = new AzureTableQueryConditionBuilder(scopeId.ToString()).ToString();
                AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
                // keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, 4, AzureDataType.Int);
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.Archived, AzureDataType.Int);
                var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                var entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition.ToString()).ToList();
                //move archived data to static
                if (entities.Count() > 0)
                {
                    AzureTableStorageUtility.DeleteTableEntities(connectStr, tableName, entities);
                    //entities.ForEach(e =>
                    //{
                    //    e.FullPath = System.IO.Path.Combine(connectionPath, e.HighName, e.LowName);
                    //});
                    AzureTableStorageUtility.AddAzureTableEntities(connectStr, staticTableName, entities);
                }

                //add reject data to static
                AzureTableQueryConditionBuilder rejectKeyBuilder = new AzureTableQueryConditionBuilder();
                rejectKeyBuilder.AppendOrQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.Rejected, AzureDataType.Int);
                var rejectFilterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, rejectKeyBuilder.ToString());
                var rejectEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, rejectFilterCondition.ToString()).ToList();

                if (rejectEntities.Count() > 0)
                {
                    //rejectEntities.ForEach(e =>
                    //{
                    //    e.FullPath = System.IO.Path.Combine(connectionPath, e.HighName, e.LowName);
                    //});
                    AzureTableStorageUtility.AddAzureTableEntities(connectStr, staticTableName, rejectEntities);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while moving fs data to static table. Error:" + e.ToString());
                return 0;
            }
            return 1;
        }

        public List<Guid> AddRejectItemsForFS(string connectString, string tenantGroupId, List<FileSystemTableEntity> entities)
        {
            List<Guid> failedGuids = new List<Guid>();
            try
            {
                //string filterCondition = null;
                string connectStr = connectString;
                string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
                string staticTableName = GetStaticArchiverApprovalTableNameForFS(tenantGroupId);
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.SyncRejectDataToStatic"))
                {
                    AzureTableStorageUtility.AddAzureTableEntities<FileSystemTableEntity>(connectStr, staticTableName, entities);
                }
                entities.ForEach(e =>
                {
                    e.MovedToApprovalTable = false;
                    e.Status = (int)SOApproveDBStatus.WaitingApprove;
                });
                using (new AvePoint.RA.Common.PerformanceScope("FSScanData.UpdateRejectData"))
                {
                    AzureTableStorageUtility.AddAzureTableEntities(connectStr, tableName, entities);
                }
                #region old logic
                //string partionCondition = new AzureTableQueryConditionBuilder(scopeId.ToString()).ToString();
                //AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
                //List<FileSystemTableEntity> entities;
                //List<FileSystemTableEntity> deleteEntities;
                //var count = 0;
                //List<Guid> tempIds = new List<Guid>();
                //foreach (var rowKey in itemRowKeys)
                //{
                //    keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                //    tempIds.Add(new Guid(rowKey));
                //    count++;
                //    if (count == 100)
                //    {
                //        try
                //        {
                //            filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                //            using (new AvePoint.RA.Common.PerformanceScope("FSScanData.RetrieveRejectData"))
                //            {
                //                entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).ToList();
                //            }
                //            using (new AvePoint.RA.Common.PerformanceScope("FSScanData.SyncRejectDataToStatic"))
                //            {
                //                AzureTableStorageUtility.AddAzureTableEntities<FileSystemTableEntity>(connectStr, staticTableName, entities);
                //            }
                //            entities.ForEach(e =>
                //            {
                //                e.MovedToApprovalTable = false;
                //                e.Status = (int)SOApproveDBStatus.WaitingApprove;
                //            });
                //            using (new AvePoint.RA.Common.PerformanceScope("FSScanData.UpdateRejectData"))
                //            {
                //                AzureTableStorageUtility.UpdateTableEnities(connectStr, tableName, entities);
                //            }
                //            logger.Info($"Update reject data count:{entities.Count}");
                //            keyBuilder = new AzureTableQueryConditionBuilder();
                //            count = 0;
                //        }
                //        catch (Exception e)
                //        {
                //            logger.Error("Failed to update rejected items for fs. Error:{0}", e.ToString());
                //            failedGuids.AddRange(tempIds);
                //            tempIds.Clear();
                //        }
                //    }
                //}
                //if (count > 0)
                //{
                //    try
                //    {
                //        filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                //        using (new AvePoint.RA.Common.PerformanceScope("FSScanData.RetrieveRejectData"))
                //        {
                //            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).ToList();
                //        }
                //        using (new AvePoint.RA.Common.PerformanceScope("FSScanData.SyncRejectDataToStatic"))
                //        {
                //            AzureTableStorageUtility.AddAzureTableEntities<FileSystemTableEntity>(connectStr, staticTableName, entities);
                //        }
                //        entities.ForEach(e =>
                //        {
                //            e.MovedToApprovalTable = false;
                //            e.Status = (int)SOApproveDBStatus.WaitingApprove;
                //        });
                //        using (new AvePoint.RA.Common.PerformanceScope("FSScanData.UpdateRejectData"))
                //        {
                //            AzureTableStorageUtility.UpdateTableEnities(connectStr, tableName, entities);
                //        }
                //        logger.Info($"Update reject data count:{entities.Count}");
                //        //deleteEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, staticTableName, filterCondition).ToList();
                //        //if (deleteEntities.Count > 0)
                //        //{
                //        //    AzureTableStorageUtility.DeleteTableEntities(connectStr, staticTableName, deleteEntities);
                //        //}
                //    }
                //    catch (Exception e)
                //    {
                //        logger.Error("Failed to update rejected items for fs. Error:{0}", e.ToString());
                //        failedGuids.AddRange(tempIds);
                //        tempIds.Clear();
                //    }
                //}
                #endregion
            }
            catch (Exception e)
            {
                logger.Error("Failed to process rejected items for fs. Error:{0}", e.ToString());
                failedGuids = entities.Select(e => new Guid(e.RowKey)).ToList();
            }
            return failedGuids;
        }
        #endregion

        #region OnPremise SP
        public OnPremiseSPTableEntity GetArchiverItemForSPOnPrem(string connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey)
        {
            string connectStr = connectionInfo;
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            OnPremiseSPTableEntity entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, itemRowKey);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());
            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }
        public IEnumerable<OnPremiseSPTableEntity> AddArchiverItemsForOnPremiseSP(string connectString, string tenantGroupId, List<OnPremiseSPTableEntity> entities)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            string staticTableName = GetStaticArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            var mEntities = AzureTableStorageUtility.AddAzureTableEntities<OnPremiseSPTableEntity>(connectStr, tableName, entities);

            #region Delete static table dirty data.
            try
            {
                List<OnPremiseSPTableEntity> deleteEntities;
                var count = 0;
                string partionCondition = new AzureTableQueryConditionBuilder(entities[0].PartitionKey.ToString()).ToString();
                AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
                List<Guid> tempIds = new List<Guid>();
                string filterCondition;
                var itemRowKeys = entities.Select(e => e.RowKey).ToList();
                foreach (var rowKey in itemRowKeys)
                {
                    keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
                    tempIds.Add(new Guid(rowKey));
                    count++;
                    if (count == 100)
                    {
                        try
                        {
                            filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                            deleteEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, staticTableName, filterCondition).ToList();
                            if (deleteEntities.Count > 0)
                            {
                                AzureTableStorageUtility.DeleteTableEntities(connectStr, staticTableName, deleteEntities);
                            }
                            keyBuilder = new AzureTableQueryConditionBuilder();
                            count = 0;
                        }
                        catch (Exception e)
                        {
                            logger.Error("Failed to update rejected items for OnPremiseSP. Error:{0}", e.ToString());
                            tempIds.Clear();
                        }
                    }
                }
                if (count > 0)
                {
                    try
                    {
                        filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                        deleteEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, staticTableName, filterCondition).ToList();
                        if (deleteEntities.Count > 0)
                        {
                            AzureTableStorageUtility.DeleteTableEntities(connectStr, staticTableName, deleteEntities);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Failed to update rejected items for OnPremiseSP. Error:{0}", e.ToString());
                        tempIds.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while deleting item from OnPremiseSP static table. Error:{0}", e.ToString());
            }
            #endregion

            #region Add reject data to static table.
            List<OnPremiseSPTableEntity> rejectEntities = entities.Where(x => x.Status == (int)SOApproveDBStatus.Rejected).ToList();
            if (rejectEntities.Count() > 0)
            {
                string rowKeyFormat = "{0}_{1}_{2}_{3}_{4}";
                rejectEntities.ForEach(e =>
                {
                    string moveTime = DateTime.UtcNow.Ticks.ToString();
                    e.RowKey = string.Format(rowKeyFormat, "New", e.Status.ToString(), e.RuleAction, e.Status == (int)SOApproveDBStatus.Archived ? e.ArchivedTime.Ticks.ToString() : moveTime,
                        Guid.NewGuid().ToString("N"));
                    e.ScopeID = e.ListId;//use this for RevIM Online Query Destroy item.
                    e.PartitionKey = e.SiteId.ToString();
                });
                AzureTableStorageUtility.AddAzureTableEntities(connectStr, staticTableName, rejectEntities);
            }
            #endregion 
            return mEntities;
        }

        public int AddRejectItemsToStaticTableForOnPremiseSP(string connectString, string tenantGroupId, List<OnPremiseSPTableEntity> entities)
        {
            string connectStr = connectString;
            string staticTableName = GetStaticArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            #region Add reject data to static table.
            if (entities.Count() > 0)
            {
                string rowKeyFormat = "{0}_{1}_{2}_{3}_{4}";
                entities.ForEach(e =>
                {
                    string moveTime = DateTime.UtcNow.Ticks.ToString();
                    e.Status = (int)SOApproveDBStatus.Rejected;
                    e.RowKey = string.Format(rowKeyFormat, "New", e.Status.ToString(), e.RuleAction, e.Status == (int)SOApproveDBStatus.Archived ? e.ArchivedTime.Ticks.ToString() : moveTime,
                        Guid.NewGuid().ToString("N"));
                    e.ScopeID = e.ListId;//use this for RevIM Online Query Destroy item.
                    e.PartitionKey = e.SiteId.ToString();
                });
                  AzureTableStorageUtility.AddAzureTableEntities(connectStr, staticTableName, entities);
            }
            #endregion 
            return 0;
        }

        public IEnumerable<OnPremiseSPTableEntity> UpdateArchiverItemsForOnPremiseSP(string connectString, string tenantGroupId, List<OnPremiseSPTableEntity> entities)
        {
            return null;
        }

        public IEnumerable<OnPremiseSPTableEntity> GetAzureDataByListForOnPremiseSP(string connectString, string tenantGroupId, string listId, string scopeId, long sortTicks, int pageSize)
        {
            string connectStr = connectString;
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(scopeId.ToString()).ToString();
            AzureTableQueryConditionBuilder listIdfilter = new AzureTableQueryConditionBuilder();
            listIdfilter.AppendOrQuery(ArchiverTableEntityProperty.ListID, AzureQueryComparisons.Equal, new Guid(listId), AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, listIdfilter.ToString());
            var result = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition.ToString())
               .OrderBy(e => e.SortTicks)
               .Where(e => e.SortTicks > sortTicks)
               .Take(pageSize);
            return result;
        }

        public int MoveRecordsToStaticForOnPremiseSP(string connectString, string tenantGroupId, string scopeId)
        {
            try
            {
                string connectStr = connectString;
                string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
                string staticTableName = GetStaticArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
                string partionCondition = new AzureTableQueryConditionBuilder(scopeId).ToString();
                AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
                keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.Archived, AzureDataType.Int);
                var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
                var entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition.ToString()).ToList();
                string rowKeyFormat = "{0}_{1}_{2}_{3}_{4}";
                //move archived data to static
                if (entities.Count() > 0)
                {
                    AzureTableStorageUtility.DeleteTableEntities(connectStr, tableName, entities);
                    entities.ForEach(e =>
                    {
                        string moveTime = DateTime.UtcNow.Ticks.ToString();
                        e.RowKey = string.Format(rowKeyFormat, "New", e.Status.ToString(), e.RuleAction, e.Status == (int)SOApproveDBStatus.Archived ? e.ArchivedTime.Ticks.ToString() : moveTime,
                            Guid.NewGuid().ToString("N"));
                        e.ScopeID = e.ListId;//use this for RevIM Online Query Destroy item.
                        e.PartitionKey = e.SiteId.ToString();
                    });
                    AzureTableStorageUtility.AddAzureTableEntities(connectStr, staticTableName, entities);
                }

                //add reject data to static
                AzureTableQueryConditionBuilder rejectKeyBuilder = new AzureTableQueryConditionBuilder();
                rejectKeyBuilder.AppendOrQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.Rejected, AzureDataType.Int);
                var rejectFilterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, rejectKeyBuilder.ToString());
                var rejectEntities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, rejectFilterCondition.ToString()).ToList();

                if (rejectEntities.Count() > 0)
                {
                    rejectEntities.ForEach(e =>
                    {
                        string moveTime = DateTime.UtcNow.Ticks.ToString();
                        e.RowKey = string.Format(rowKeyFormat, "New", e.Status.ToString(), e.RuleAction, e.Status == (int)SOApproveDBStatus.Archived ? e.ArchivedTime.Ticks.ToString() : moveTime,
                            Guid.NewGuid().ToString("N"));
                        e.ScopeID = e.ListId;//use this for RevIM Online Query Destroy item.
                        e.PartitionKey = e.SiteId.ToString();
                    });
                    AzureTableStorageUtility.AddAzureTableEntities(connectStr, staticTableName, rejectEntities);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while moving fs data to static table. Error:" + e.ToString());
                return 0;
            }
            return 1;
        }
        #endregion

        private int ParseUIVersion(string version)
        {
            var uiversion = 0;
            try
            {
                if (version.Contains("."))
                {
                    var splits = version.Split('.');
                    int majorVers = int.Parse(splits[0]);
                    int minorVers = int.Parse(splits[1]);
                    uiversion = 512 * majorVers + minorVers;
                }
            }
            catch (Exception e)
            {
                logger.Warn("parse ui version: {0}, error: {1}", version, e.ToString());
            }
            return uiversion;
        }

        public ArchiverTableEntity GetDestroyItem(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version, bool isRetention = false)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetStaticArchiverApprovalTableName(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            ArchiverTableEntity entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());

            var uiversion = ParseUIVersion(version);
            if (uiversion != 0)
            {
                string uisersionFilter = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.UIVersion, AzureQueryComparisons.Equal, uiversion, AzureDataType.Int);
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(filterCondition, uisersionFilter.ToString());
            }
            if (isRetention)
            { 
                string sourceFlagFilter = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.SourceFlag, AzureQueryComparisons.Equal, 99, AzureDataType.Int);
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(filterCondition, sourceFlagFilter.ToString());
            }
            else
            {
                string sourceFlagFilter = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.SourceFlag, AzureQueryComparisons.NotEqual, 99, AzureDataType.Int);
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(filterCondition, sourceFlagFilter.ToString());
            }
            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }

        public OnPremiseSPTableEntity GetDestroyItemForSPOnPrem(string connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version)
        {
            string connectStr = connectionInfo;
            string tableName = GetStaticArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            OnPremiseSPTableEntity entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());

            var uiversion = ParseUIVersion(version);
            if (uiversion != 0)
            {
                string uisersionFilter = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.UIVersion, AzureQueryComparisons.Equal, uiversion, AzureDataType.Int);
                filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(filterCondition, uisersionFilter.ToString());
            }

            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }

        public ArchiverExchangeOnlineDto GetArchiverItemForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableNameForEXO(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            ArchiverExchangeOnlineDto entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, itemRowKey);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());
            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverExchangeOnlineDto>(connectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }

        public ArchiverExchangeOnlineDto GetDestroyItemForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, string itemRowKey)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetStaticArchiverApprovalTableNameForEXO(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            ArchiverExchangeOnlineDto entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, itemRowKey);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());
            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverExchangeOnlineDto>(connectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }

        public FileSystemTableEntity GetDestroyItemForFS(string fsAzureTableConnectStr, string tenantGroupId, string partitionKey, string itemRowKey)
        {
            string tableName = GetStaticArchiverApprovalTableNameForFS(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder rowKeyFilter = new AzureTableQueryConditionBuilder();
            FileSystemTableEntity entity;
            rowKeyFilter.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, itemRowKey);
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), rowKeyFilter.ToString());
            entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(fsAzureTableConnectStr, tableName, filterCondition).FirstOrDefault();
            return entity;
        }
        public List<FileSystemTableEntity> GetDestroyItemForFSDesctruntion(string fsAzureTableConnectStr, string tenantGroupId, string partitionKey, Guid parentId, DateTime archiverTimeStart, DateTime archiverTimeEnd)
        {
            string tableName = GetStaticArchiverApprovalTableNameForFS(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            AzureTableQueryConditionBuilder keyFilter = new AzureTableQueryConditionBuilder();
            keyFilter.AppendAndQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, parentId, AzureDataType.Guid);
            //注意FS这个AchiveTime的单词拼写是错的, 不要使用常量
            keyFilter.AppendAndQuery("AchiveTime", AzureQueryComparisons.GreaterThan, archiverTimeStart, AzureDataType.Date);
            keyFilter.AppendAndQuery("AchiveTime", AzureQueryComparisons.LessThan, archiverTimeEnd, AzureDataType.Date);
            keyFilter.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, (int)SOApproveDBStatus.Archived, AzureDataType.Int);
            keyFilter.AppendAndQuery("RuleAction", AzureQueryComparisons.Equal, 1, AzureDataType.Int);   //archiver and remove
            string filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partitionFilter.ToString(), keyFilter.ToString());
            var entity = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(fsAzureTableConnectStr, tableName, filterCondition).ToList();
            return entity;
        }

        public Tuple<IEnumerable<FileSystemTableEntity>, string> GetDestroyItemForFSDesctruntionByConnectionIdByPage(string fsAzureTableConnectStr, string tenantGroupId, string partitionKey, int pageSize, string continuationToken)
        {
            string tableName = GetStaticArchiverApprovalTableNameForFS(tenantGroupId);
            AzureTableQueryConditionBuilder partitionFilter = new AzureTableQueryConditionBuilder(partitionKey);
            string filterCondition = partitionFilter.ToString();
            TableClient tableClient = AzureUtil.GetTableClient(fsAzureTableConnectStr, tableName, true);
            var pageable = tableClient.Query<FileSystemTableEntity>(filter: filterCondition, maxPerPage: pageSize);
            var page = pageable.AsPages(continuationToken, pageSize).FirstOrDefault();
            var entities = page?.Values?.ToList() ?? new List<FileSystemTableEntity>();
            var nextToken = page?.ContinuationToken ?? string.Empty;
            return new Tuple<IEnumerable<FileSystemTableEntity>, string>(entities, nextToken);
        }

        public async Task UpdateItemStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid nodeId, bool isApproval)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<ArchiverTableEntity> entities;
            Action<ArchiverTableEntity> changeFunc;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());

            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).ToList();
            if (isApproval)
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Approved;
            }
            else
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Rejected;
            }
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
            //UpdateItems(connectionInfo, tenantGroupId, partKey, itemRowKeys, e => e.Status = (int)SOApproveDBStatus.Approved);
        }

        public async Task ResetItemStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid nodeId)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<ArchiverTableEntity> entities;
            Action<ArchiverTableEntity> changeFunc = e => e.Status = (int)SOApproveDBStatus.WaitingApprove;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());

            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).ToList();
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        public async Task UpdateItemStatusForSPOnPremAsync(string connectionInfo, string tenantGroupId, string partKey, Guid nodeId, bool isApproval)
        {
            string connectStr = connectionInfo;
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<OnPremiseSPTableEntity> entities;
            Action<OnPremiseSPTableEntity> changeFunc;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());

            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition).ToList();
            if (isApproval)
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Approved;
            }
            else
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Rejected;
            }
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        public async Task ResetItemStatusForSPOnPremAsync(string connectionInfo, string tenantGroupId, string partKey, Guid nodeId)
        {
            string connectStr = connectionInfo;
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<OnPremiseSPTableEntity> entities;
            Action<OnPremiseSPTableEntity> changeFunc = e => e.Status = (int)SOApproveDBStatus.WaitingApprove;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());

            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<OnPremiseSPTableEntity>(connectStr, tableName, filterCondition).ToList();
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        public async Task UpdateItemStatusForEXOAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, string rowKey, bool isApproval)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableNameForEXO(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<ExchangeOnlineTableEntity> entities;
            Action<ExchangeOnlineTableEntity> changeFunc;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ExchangeOnlineTableEntity>(connectStr, tableName, filterCondition).ToList();
            if (isApproval)
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Approved;
            }
            else
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Rejected;
            }
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        public async Task ResetItemStatusForEXOAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, string rowKey)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableNameForEXO(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<ExchangeOnlineTableEntity> entities;
            Action<ExchangeOnlineTableEntity> changeFunc = e => e.Status = (int)SOApproveDBStatus.WaitingApprove;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ExchangeOnlineTableEntity>(connectStr, tableName, filterCondition).ToList();
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        public async Task UpdateItemStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partKey, string rowKey, bool isApproval)
        {
            string connectStr = fsAzureTableConnectStr;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<FileSystemTableEntity> entities;
            Action<FileSystemTableEntity> changeFunc;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).ToList();
            if (isApproval)
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Approved;
            }
            else
            {
                changeFunc = e => e.Status = (int)SOApproveDBStatus.Rejected;
            }
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        public async Task ResetItemStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partKey, string rowKey)
        {
            string connectStr = fsAzureTableConnectStr;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();
            List<FileSystemTableEntity> entities;
            Action<FileSystemTableEntity> changeFunc = e => e.Status = (int)SOApproveDBStatus.WaitingApprove;
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());
            entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).ToList();
            entities.ForEach(changeFunc);
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        //public bool CheckArchiveTableExist(AzureTableConnectContract connectionInfo, string tenantGroupId)
        //{
        //    return Core.AzureTableStorageUtility.CheckAzureTableExist(GetConnectString(connectionInfo), GetArchiverApprovalTableName(tenantGroupId));
        //}

        public async Task UpdateItemDisposalActionAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, Guid nodeId, RelatedRecordOption relatedRecordOption)
        {
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetArchiverApprovalTableName(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();

            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());

            List<ArchiverTableEntity> entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectStr, tableName, filterCondition).ToList();
            entities.ForEach(e =>
            {
                //rule 中选择都删除，则archiver table中标记成 delete
                if (relatedRecordOption == RelatedRecordOption.Both)
                {
                    e.DeleteRelatedRecords = 1;
                }
                else if (relatedRecordOption == RelatedRecordOption.None)
                {
                    e.DeleteRelatedRecords = 0;
                }
            });
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }
        public async Task UpdateItemDisposalActionAsync(string connectionstring, string tenantGroupId, string partKey, Guid nodeId, RelatedRecordOption relatedRecordOption)
        {
            string tableName = GetArchiverApprovalTableNameForOnPremiseSP(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();

            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.ParentID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());

            List<ArchiverTableEntity> entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(connectionstring, tableName, filterCondition).ToList();
            entities.ForEach(e =>
            {
                //rule 中选择都删除，则archiver table中标记成 delete
                if (relatedRecordOption == RelatedRecordOption.Both)
                {
                    e.DeleteRelatedRecords = 1;
                }
                else if (relatedRecordOption == RelatedRecordOption.None)
                {
                    e.DeleteRelatedRecords = 0;
                }
            });
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectionstring, tableName, entities);
        }
        public async Task UpdateItemDisposalActionForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partKey,string rowKey, RelatedRecordOption relatedRecordOption)
        {
            string connectStr = fsAzureTableConnectStr;
            string tableName = GetArchiverApprovalTableNameForFS(tenantGroupId);
            string partionCondition = new AzureTableQueryConditionBuilder(partKey).ToString();
            AzureTableQueryConditionBuilder keyBuilder = new AzureTableQueryConditionBuilder();

            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.Equal, rowKey, AzureDataType.String);
            keyBuilder.AppendOrQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, partKey, AzureDataType.String);
            var filterCondition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, keyBuilder.ToString());

            List<FileSystemTableEntity> entities = AzureTableStorageUtility.RetrieveTableEntitiesInCondition<FileSystemTableEntity>(connectStr, tableName, filterCondition).ToList();
            entities.ForEach(e =>
            {
                //rule 中选择都删除，则archiver table中标记成 delete
                if (relatedRecordOption == RelatedRecordOption.Both)
                {
                    e.DisposalAction = true;
                }
                else if (relatedRecordOption == RelatedRecordOption.None)
                {
                    e.DisposalAction = false;
                }
            });
            await AzureTableStorageUtility.UpdateTableEnitiesAsync(connectStr, tableName, entities);
        }

        #region ForExchangeOnline
        public List<ArchiverExchangeOnlineDto> GetDeletedItemsByMailBoxId(AzureTableConnectContract connectionInfo, string tenantGroupId, string partKey, string mailBoxId, DateTime startTime, DateTime endTime)
        {
            List<ArchiverExchangeOnlineDto> allDatas = new List<ArchiverExchangeOnlineDto>();
            string connectStr = GetConnectString(connectionInfo);
            string tableName = GetStaticArchiverApprovalTableNameForEXO(tenantGroupId);
            var partKeyFormats = new string[] { "{0}", "{0}Manual" };
            foreach (var partKeyFormat in partKeyFormats)
            {
                string partionCondition = new AzureTableQueryConditionBuilder(string.Format(partKeyFormat, partKey)).ToString();
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
                builder.AppendAndQuery(ArchiverTableEntityProperty.ArchivedTime, AzureQueryComparisons.GreaterThan, startTime.Ticks, AzureDataType.Long);
                builder.AppendAndQuery(ArchiverTableEntityProperty.ArchivedTime, AzureQueryComparisons.LessThan, endTime.Ticks, AzureDataType.Long);
                builder.AppendAndQuery(ArchiverTableEntityProperty.CacheNodeType, AzureQueryComparisons.Equal, 700, AzureDataType.Int);
                //PartitionKey 就能够定位某个Mailbox数据，目前ArchiverStatic表中存储的是AOS真实MailboxID，但是DestroyReport获取不到真实MailboxID,
                //是通过DAO TreeNodeID查询，添加此条件后由于和ArchiverStatic表中ID不一致，造成数据查询不出来。因此去掉Mailbox查询条件
                //builder.AppendAndQuery(ArchiverTableEntityProperty.MailBoxId, AzureQueryComparisons.Equal, mailBoxId, AzureDataType.String);
                builder.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, 5, AzureDataType.Int);

                string condition = AzureTableQueryConditionBuilder.CombineAndQueries(partionCondition, builder.ToString());
                logger.Info("GetDestroyedItemsByListId condition is {0}", condition);
                allDatas.AddRange(AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverExchangeOnlineDto>(connectStr, tableName, condition).ToList());
            }
            return allDatas;
        }
        #endregion
    }
}
