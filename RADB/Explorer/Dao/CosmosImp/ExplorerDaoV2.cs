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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.MockV2;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.Records.Core.Utilities.Extensions;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    public class ExplorerDao : IExplorerDao, IDisposable
    {

        RecordRepositoryV2 _repository = null;
        private IRALogger logger = RALogger.GetInstance(typeof(ExplorerDao));


        private static IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public ExplorerDao(bool createConnectionIfNotExist = false) : this(RMDBContextManager.GetCosmosDBConnectionAsync().Result, createConnectionIfNotExist)
        {
        }

        public ExplorerDao(bool useJpmcNoramlDB, bool createConnectionIfNotExist = false) : this(RMDBContextManager.GetCosmosDBConnectionAsync(useJpmcNoramlDB).Result, createConnectionIfNotExist)
        {
        }

        public ExplorerDao UsingNormalDatabase()
        {
            logger.Info("Using Normal Database");
            return new ExplorerDao(RMDBContextManager.GetCosmosDBConnectionAsync(true).Result);
        }

        public ExplorerDao(CosmosConnectionInfo connectionInfo, bool createConnectionIfNotExist = false)
        {
            _repository = new RecordRepositoryV2(connectionInfo);
            if (createConnectionIfNotExist)
            {
                CreateExplorerContainerIfNotExist(connectionInfo);
            }
        }

        private void CreateExplorerContainerIfNotExist(CosmosConnectionInfo connectionInfo)
        {
            try
            {
                var createRes = RMCosmosDBIndependentController.IsEnabledIndependent() ? _repository.CreateIndependentContainerIfNotExistsAsync(connectionInfo.CollectionId).Result : _repository.CreateNormalContainerIfNotExistsAsync(connectionInfo.CollectionId).Result;
                if (createRes)
                {
                    try
                    {
                        var l_CurrentStack = new System.Diagnostics.StackTrace(false);
                        logger.Info($"Create new Cosmos DB {connectionInfo?.DatabaseId}, {connectionInfo?.CollectionId}");
                        logger.Info(l_CurrentStack.ToString());
                    }
                    catch (Exception e)
                    {
                        logger.Info($"Get StackTrace failed {e}");
                    }
                    var extensionConnectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION];
                    var resource = 0;
                    if (!string.IsNullOrEmpty(extensionConnectionStr))
                    {
                        resource = JsonConvert.DeserializeObject<List<string>>(extensionConnectionStr).Count;
                    }
                    logger.Info($"Create container if not exists, tenant : {connectionInfo.CollectionId}, current resource is : {resource}");
                    IDBInfoDao dao = new DB.Dao.Impl.DBInfoDao();
                    if (RMCosmosDBIndependentController.IsEnabledIndependent())
                    {
                        dao.AddIndependentExplorerDBMappingInfo(new Contract.Tenant.RMDBInfoDto() { DBName = connectionInfo.DatabaseId, ContainerName = connectionInfo.CollectionId, Resource = resource });
                    }
                    else
                    {
                        dao.AddExplorerDBMappingInfo(new Contract.Tenant.RMDBInfoDto() { DBName = connectionInfo.DatabaseId, ContainerName = connectionInfo.CollectionId, Resource = resource });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while create explorer containers, {connectionInfo?.DatabaseId}, {connectionInfo?.CollectionId},ERROR:{ex.ToString()}");
                throw;
            }

        }

        public void Dispose()
        {
            _repository?.Dispose();
        }

        public Record FindRecordBySiteAndPath(string aveSiteId, string dirPath, string leafName, bool isFileSystem)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                return null;
            }

            if (isFileSystem)
            {
                if (string.IsNullOrWhiteSpace(leafName))
                {
                    return null;
                }

                return GetFirstOrDefault(r => r.DirPath == dirPath && r.LeafName == leafName && r.SourceFlag == (int)SourceFlag.FileSystem);
            }

            if (string.IsNullOrWhiteSpace(aveSiteId))
            {
                return null;
            }

            return GetFirstOrDefault(r => r.AveSiteId == aveSiteId && r.DirPath == dirPath);
        }

        public void AddPath2IndexPolicy(List<string> pathList, bool includedPath = true)
        {
            try
            {
                if (_repository.CanUpdateIndexPolicy())
                {
                    logger.Info($"Will update the index policy for tenant '{Contract.Tenant.TenantLocalValue.LogonGroupId}' if needed.");
                    if (includedPath)
                        _repository.AddIndexPolicyIncludedPaths(pathList).GetAwaiter().GetResult();
                }
                else
                {
                    logger.Warn($"Ignore updating index policy because the previous update isn't completed yet.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to update the index policy for tenant '{Contract.Tenant.TenantLocalValue.LogonGroupId}', error {ex.ToString()}");
            }
        }

        public bool Contains(Guid id)
        {
            var record = _repository.FirstOrDefault(a => a.Id == id);
            return record != null;
        }

        public bool ContainsConnectorRecords(string connectorUniqueId)
        {
            var record = _repository.FirstOrDefault(a => a.RowKey.Equals(connectorUniqueId, StringComparison.OrdinalIgnoreCase));
            return record != null;
        }

        public Record ReadById(Guid scopeId, Guid id)
        {
            //Record rec = _repository.ReadItemAsync(id.ToString(), new Microsoft.Azure.Cosmos.PartitionKey(scopeId.ToString())).Result;
            Record rec = _repository.FirstOrDefault(a => a.Id == id && a.ScopeId == scopeId);
            rec.AppendMetaInfoForOldLogic();
            return rec;
        }

        public Record GetByItemRowId(Guid scopeId, Guid listId, int itemRowId)
        {
            Record rec = _repository.FirstOrDefault(a => a.ScopeId == scopeId && a.ListId == listId && a.ItemRowId == itemRowId);
            rec.AppendMetaInfoForOldLogic();
            return rec;
        }

        public void Upsert(Record record)
        {
            record.AppendCustomColumns();
            var result = _repository.UpsertItemAsync(record).Result;
        }

        public Task UpsertAsync(Record record)
        {
            record.AppendCustomColumns();
            return _repository.UpsertItemAsync(record);
        }

        public List<(Record, Exception)> BulkUpsert(List<Record> recordList)
        {
            var result = new List<(Record, Exception)>();
            recordList.ForEach(r => r.AppendCustomColumns());
            var failedItems = _repository.RetryUpsertRecordsConcurrentlyAsync(recordList).Result;
            if (failedItems?.Count > 0)
            {
                failedItems.ForEach(failedItem =>
                {
                    var r = recordList.FirstOrDefault(o => o.Id == failedItem.Item1);
                    if (r != null)
                    {
                        result.Add((r, failedItem.Item2));
                    }
                });
            }
            return result;
        }

        public List<(Record, Exception)> BulkUpsertDirectly(List<Record> recordList)
        {
            var result = new List<(Record, Exception)>();
            recordList.ForEach(r => r.AppendCustomColumns());
            var task = Task.Run(() => { return _repository.RetryUpsertRecordsConcurrentlyAsync(recordList); });
            var failedItems = task.GetAwaiter().GetResult();
            if (failedItems?.Count > 0)
            {
                failedItems.ForEach(failedItem =>
                {
                    var r = recordList.FirstOrDefault(o => o.Id == failedItem.Item1);
                    if (r != null)
                    {
                        result.Add((r, failedItem.Item2));
                    }
                });
            }
            return result;
        }

        public void Replace(Record record)
        {
            record.AppendCustomColumns();
            var result = _repository.ReplaceItemAsync(record).Result;
        }

        public void Add(Record record)
        {
            record.AppendCustomColumns();
            var result = _repository.AddAsync(record).Result;
        }

        public bool DeleteExplorerData(string tenantId)
        {
            return _repository.DeleteContainerAsync(tenantId).Result;
        }

        public void BatchAddRecords(List<Record> records, bool forceUpdate = false)
        {
            foreach (var record in records)
            {
                AddOrUpdateRecord(record, forceUpdate);
            }
        }

        public int UpdateAll(Expression<Func<Record, bool>> predicate, Action<Record> operation)
        {
            return _repository.UpdateAllAsync(predicate, operation).Result;
        }

        public IEnumerable<Record> QueryAll(Expression<Func<Record, bool>> predicate, bool convertCustomColumn2Metainfo = true)
        {
            IEnumerable<Record> recs = _repository.QueryAllAysnc(predicate).Result;
            if (convertCustomColumn2Metainfo)
            {
                foreach (Record rec in recs)
                {
                    rec.AppendMetaInfoForOldLogic();
                }
            }
            return recs;
        }

        public IEnumerable<Record> QueryAllByDescending(Expression<Func<Record, bool>> predicate)
        {
            IEnumerable<Record> recs = _repository.QueryAllAysnc(predicate, null, true).Result;
            foreach (Record rec in recs)
            {
                rec.AppendMetaInfoForOldLogic();
            }
            return recs;
        }

        public Record GetFirstOrDefault(Expression<Func<Record, bool>> whereLambda)
        {
            var rec = _repository.FirstOrDefault(whereLambda);
            rec.AppendMetaInfoForOldLogic();
            return rec;
        }

        public Record GetFirstOrDefault(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderLambda)
        {
            return _repository.FirstOrDefault(predicate, orderLambda);
        }

        public Record GetFirstOrDefaultByOrderDesc(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderLambda)
        {
            return _repository.FirstOrDefaultByOrderDesc(predicate, orderLambda);
        }

        public int QueryCount(Expression<Func<Record, bool>> whereLambda)
        {
            var rec = _repository.CountAsync(whereLambda).Result;
            return rec;
        }

        /// <summary>
        /// To Test
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int QueryCount(string sql, Dictionary<string, object> parameters = null)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return _repository.QueryAllBySqlAsync<int>(new Microsoft.Azure.Cosmos.QueryDefinition(sql)).Result.First();
            }
            else
            {
                var query = new Microsoft.Azure.Cosmos.QueryDefinition(sql);
                foreach (var para in parameters)
                {
                    query.WithParameter(para.Key, para.Value);
                }
                return _repository.QueryAllBySqlAsync<int>(query).Result.First();
            }
        }
        /// <summary>
        /// To Test
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public Dictionary<string, int> QueryRelatedTermCount(string sql)
        {
            Dictionary<string, int> termIdAndRelatedCount = new Dictionary<string, int>();
            var count = _repository.QueryAllBySqlAsync<dynamic>(new Microsoft.Azure.Cosmos.QueryDefinition(sql)).Result;
            foreach (dynamic family in count)
            {
                //var dic = (IDictionary<string, object>)family;
                string termId = family["termId"].ToString();
                int termCount = Convert.ToInt32(family["termcount"]);
                if (termIdAndRelatedCount.ContainsKey(termId))
                {
                    termIdAndRelatedCount[termId] = termCount;
                }
                else
                {
                    termIdAndRelatedCount.Add(termId, termCount);
                }
            }
            return termIdAndRelatedCount;
        }

        public List<(string date, int count)> QueryDashboardDataUsageOfDate(string sql)
        {
            var res = new List<(string date, int count)>();

            var queryResult = _repository.QueryAllBySqlAsync<dynamic>(new QueryDefinition(sql)).Result;

            foreach (dynamic result in queryResult)
            {
                var date = result["date"].ToString();
                var count = Convert.ToInt32(result["count"]);
                res.Add((date, count));
            }

            return res;
        }

        public List<(List<int> Reviewers, int Count)> QueryReviewerWaitingApprovalItemCount(string sql)
        {
            var res = new List<(List<int> Reviewers, int Count)>();

            var queryResult = _repository.QueryAllBySqlAsync<dynamic>(new QueryDefinition(sql)).Result;

            foreach (dynamic result in queryResult)
            {
                var reviewersJson = JsonConvert.SerializeObject(result["reviewers"]);
                var reviewers = JsonConvert.DeserializeObject<List<int>>(reviewersJson);
                var count = Convert.ToInt32(result["count"]);
                res.Add((reviewers, count));
            }

            return res;
        }

        public Dictionary<string, int> QuerySiteCollectionUsageCount(string sql)
        {
            var siteCollectionUsageCount = new Dictionary<string, int>();
            var queryResult = _repository.QueryAllBySqlAsync<dynamic>(new QueryDefinition(sql)).Result;
            foreach (dynamic result in queryResult)
            {
                var siteId = result["aveSiteId"].ToString();
                var count = Convert.ToInt32(result["siteUsageCount"]);
                siteCollectionUsageCount.Add(siteId, count);
            }
            return siteCollectionUsageCount;
        }

        public bool Exist(Expression<Func<Record, bool>> whereLambda)
        {
            try
            {
                return _repository.ExistAsync(whereLambda).Result;
            }
            catch (AggregateException e)
            {
                logger.Error($"Failed to connect cosmosdb, method: Exist, AggregateException error: {e.InnerException}");
                return false;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to connect cosmosdb, method: Exist, error: {e}");
                return false;
            }
        }

        public List<T> GetFilterList<T>(Expression<Func<Record, T>> selectLambda, Expression<Func<Record, bool>> whereLambda)
        {
            return _repository.QueryAllAysnc<T>(whereLambda, selectLambda).Result.ToList();
        }

        public Tuple<IEnumerable<Record>, string> QueryByPage(Expression<Func<Record, bool>> predicate, int pageCount = 15, string continuation = "", bool convertCustomColumn2Metainfo = true)
        {
            var result = _repository.QueryByPageAsync(predicate, null, false, pageCount, continuation).Result;
            if (convertCustomColumn2Metainfo)
            {
                foreach (Record rec in result.Item1)
                {
                    rec.AppendMetaInfoForOldLogic();
                }
            }

            var temp = new Tuple<IEnumerable<Record>, string>(result.records, result.continuationToken);
            return temp;
        }

        public async Task<List<string>> DistinctQueryAsync(Expression<Func<Record, string>> selectLambda, Expression<Func<Record, bool>> whereLambda)
        {
            return await _repository.DistinctQueryAsync(selectLambda, whereLambda);
        }

        public Tuple<IEnumerable<Record>, string> QueryByPage(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderBy = null, bool descending = false, int pageCount = 15, string continuation = "", bool convertCustomColumn2Metainfo = true)
        {
            var result = _repository.QueryByPageAsync(predicate, orderBy, descending, pageCount, continuation).Result;
            if (convertCustomColumn2Metainfo)
            {
                foreach (Record rec in result.Item1)
                {
                    rec.AppendMetaInfoForOldLogic();
                }
            }

            var temp = new Tuple<IEnumerable<Record>, string>(result.records, result.continuationToken);
            return temp;
        }

        //public Tuple<IEnumerable<TOut>, string> QueryByPage<TOut, TOrder>(Expression<Func<Record, bool>> predicate, Expression<Func<Record, TOut>> selector, Expression<Func<Record, TOrder>> orderByLambda, bool orderAscending = true, int pageCount = 15, string continuation = "")
        //{
        //    return _repository.QueryByPageAsync<TOut>(predicate, selector, orderByLambda, orderAscending, pageCount, continuation);
        //}

        public List<Guid> UpdateExpiredHeldRecords()
        {
            var records = QueryAll(r => r.HoldStatus && r.HoldReleaseTime < DateTime.UtcNow.Ticks);
            List<Guid> ids = records.Select(a => a.Id).ToList();
            foreach (var record in records)
            {
                record.HoldStatus = false;
                record.HoldReleaseTime = 0;
                record.HoldBy = string.Empty;
                record.HoldId = string.Empty;
                record.AppendHolds_Array = new string[0];
                record.HoldByUsers = null;
                record.HoldUntilTimes = null;
                record.DisposalDueDate = record.PreviosDisposalDueDate;
                Replace(record);
            }
            return ids;
        }

        public List<Record> GetRecordsByTerms(Guid scopeId, List<Guid> termIds, long ticks)
        {
            return QueryAll(m => termIds.Contains(m.TermId) && m.ScopeId == scopeId && m.RecordStatus == (int)RMRecordStatus.Active && (m.NodeType == 500 || m.NodeType == 400) && m.CollectTime < ticks).ToList();
        }

        public List<Record> GetConnectorRecordsByTerms(int sourceFlag, List<Guid> termIds)
        {
            return QueryAll(m => termIds.Contains(m.TermId) && m.SourceFlag == sourceFlag && m.RecordStatus == (int)RMRecordStatus.Active).ToList();
        }

        public IEnumerable<Record> GetDoesNotMatchRuleConnectorItems(int sourceFlag)
        {
            return QueryAll(m => m.TermId != Guid.Empty && m.SourceFlag == sourceFlag && m.RecordStatus == (int)RMRecordStatus.Active && ((m.DisposalDueDate < DateTime.UtcNow.Ticks && m.DisposalDueDate > DateTime.MinValue.Ticks) || m.RuleId == Guid.Empty)).ToList();
        }

        public List<Record> GetRecordsByTermsByPage(Guid scopeId, List<Guid> termIds, long ticks, long sortTicks, int pageSize)
        {
            return QueryAll(m => termIds.Contains(m.TermId) && m.ScopeId == scopeId && m.RecordStatus == (int)RMRecordStatus.Active && m.NodeType == 500 && m.CollectTime < ticks).ToList()
                .OrderBy(s => s.SortTicks)
                .Where(s => s.SortTicks > sortTicks)
                .Take(pageSize).ToList(); ;
        }

        public List<Record> GetEXORecordsByTerms(Guid scopeId, List<Guid> termIds, long ticks, string emailAddress)
        {
            return QueryAll(m => termIds.Contains(m.TermId) && m.ScopeId == scopeId && m.RecordStatus == (int)RMRecordStatus.Active && m.NodeType == 5110 && m.CollectTime < ticks && m.EmailAddress == emailAddress).ToList();
        }

        public int QueryDataGetTotal(int status, string keyWord, Expression<Func<Record, bool>> whereLambda = null)
        {
            int cnt = 0;
            Expression<Func<Record, bool>> searchLambda = null;
            if (string.IsNullOrEmpty(keyWord))
            {
                searchLambda = m => m.LeafName.Contains(keyWord) || m.RecordsId.Contains(keyWord);
                cnt = QueryAll(whereLambda).AsQueryable().Where(searchLambda).Where(r => r.RecordStatus == status && r.NodeType == (int)NodeLevel.Item).Count();
            }
            else
            {
                cnt = QueryAll(whereLambda).AsQueryable().Where(r => r.RecordStatus == status && r.NodeType == (int)NodeLevel.Item).Count();
            }

            return cnt;
        }

        public Tuple<IEnumerable<Record>, string> QueryDataWithoutTotal(string continuation, int pageSize, out bool hasNext, Expression<Func<Record, bool>> whereLambda = null)
        {
            var list = QueryByPage(whereLambda, pageSize, continuation);
            hasNext = !string.IsNullOrEmpty(list.Item2);
            return list;
        }

        public async Task<(Tuple<IEnumerable<Record>, string>, bool)> QueryDataBySqlWithoutTotalAsync(ExplorerQueryDto dto, bool isGlobalSearch, string continuation, int pageSize, SecurityTermPermissionDto termPermDto = null)
        {
            var sqlQuerySpec = await CosmosSqlQueryHelper.BuildSearchAsync(dto, true, true, isGlobalSearch, true, termPermDto);
            LogSqlQuerySpec(sqlQuerySpec);
            var list = QueryPageBySql(From(sqlQuerySpec), pageSize, continuation);
            bool hasNext = !string.IsNullOrEmpty(list.Item2);
            return (list, hasNext);
        }

        public Tuple<IEnumerable<Record>, string> QueryDataBySqlWithoutTotal(PhysicalExplorerQueryDto dto, string continuation, int pageSize, out bool hasNext, SecurityTermPermissionDto termPermDto, bool withoutPhysicalRecord = false)
        {
            var sqlQuerySpec = CosmosSqlQueryHelper.BuildSerch(dto, termPermDto, withoutPhysicalRecord);
            LogSqlQuerySpec(sqlQuerySpec);
            var list = QueryPageBySql(From(sqlQuerySpec), pageSize, continuation);
            hasNext = !string.IsNullOrEmpty(list.Item2);
            return list;
        }

        //public int QueryCountBySql(PhysicalExplorerQueryDto dto, SecurityTermPermissionDto termPermDto, bool withoutPhysicalRecord = false)
        //{
        //    var sqlQuerySpec = CosmosSqlQueryHelper.BuildSerch(dto, termPermDto, withoutPhysicalRecord);
        //    sqlQuerySpec.QueryText = sqlQuerySpec.QueryText.Replace(CosmosSqlQueryHelper.SELECT_ALL_CLAUSE_WHERE, CosmosSqlQueryHelper.SELECT_COUNT_CLAUSE_WHERE);
        //    LogSqlQuerySpec(sqlQuerySpec);
        //    var r = _repository.QueryAllBySqlAsync<dynamic>(From(sqlQuerySpec)).Result.First();
        //    return Convert.ToInt32(r);
        //}

        public Tuple<IEnumerable<Record>, string> QueryPageBySqlForBrowse(RMPhysicalExplorerNode currentRecord, List<int> permissionIds, bool hasScopePermission, int pageCount = 15, string continuation = "", SecurityTermPermissionDto termPermDto = null)
        {
            var sqlQuerySpec = CosmosSqlQueryHelper.BuildSqlForBrowseTree(currentRecord, permissionIds, hasScopePermission, termPermDto);
            LogSqlQuerySpec(sqlQuerySpec);
            return QueryPageBySql(From(sqlQuerySpec), pageCount, continuation);
        }

        public Tuple<IEnumerable<Record>, string> QueryPageBySql(QueryDefinition queryDefinition, int pageCount = 15, string continuation = "")
        {
            var temp = _repository.QueryPageBySqlAsync(queryDefinition, pageCount, continuation).Result;
            foreach (Record rec in temp.results)
            {
                rec.AppendMetaInfoForOldLogic();
            }

            Tuple<IEnumerable<Record>, string> result = new Tuple<IEnumerable<Record>, string>(temp.results, temp.continuationToken);
            return result;
        }

        //public Tuple<IEnumerable<Record>, string> SearchRecords(string pageIndex, int pageSize, string value, List<Guid> exceptIds, List<int> permissions, SourceFlag sourceFlag, out bool hasNext, bool isEnduser = false)
        //{
        //    var nodeTypes = new List<int> { (int)NodeLevel.Item, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
        //    var sourceFlags = ReAssembleFourceFlags(sourceFlag);
        //    if (RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled())
        //    {
        //        var userPermission = SecurityTrimmingHelper.GetCurrentUserPermission();
        //        userPermission.RemoveNoPermissionFourceFlags(sourceFlags);
        //        userPermission.RemoveNoPermissionNodeTypes(nodeTypes);
        //    }
        //    var intSourceFlags = sourceFlags.Select(o => (int)o);
        //    //由于Phy和SP都不查询Status是2的数据，暂不需要区分数据源
        //    Expression<Func<Record, bool>> lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord
        //    && (nodeTypes.Contains(s.NodeType))
        //    && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed)
        //    && (intSourceFlags.Contains(s.SourceFlag));

        //    var useSqlQuery = !string.IsNullOrEmpty(value) || isEnduser;


        //    if (sourceFlag == SourceFlag.FileSystem)
        //    {
        //        //var intSourceFlags = sourceFlags.Select(o => (int)o).ToList();
        //        useSqlQuery = false;
        //        lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord
        //        && (s.NodeType == (int)NodeLevel.FSFile)
        //        && (s.RecordStatus == (int)RMRecordStatus.Active)
        //        && (intSourceFlags.Contains(s.SourceFlag));
        //    }

        //    Tuple<IEnumerable<Record>, string> result = null;
        //    if (useSqlQuery)
        //    {

        //        //value = value.ToLower();
        //        //lambda = s => (s.LeafName.Contains(value) || s.RecordsId.Contains(value)) && !exceptIds.Contains(s.Id) && !s.DeclareAsRecord
        //        //&& (s.NodeType == (int)NodeLevel.Item || s.NodeType == (int)RMNodeLevel.PhysicalFile || s.NodeType == (int)RMNodeLevel.PhysicalRecord)
        //        //&& (s.RecordStatus == (int)PhysicalRecordStatus.Open || s.RecordStatus == (int)PhysicalRecordStatus.Closed);

        //        //由于要支持根据name或者id的search，lambda表达式中无法支持大小写无关的contains查询，
        //        //因此改为拼装sql语句，在语句中调用cosmos db的内置函数，能够达到要求
        //        //var nodeTypes = new int[] { (int)NodeLevel.Item, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
        //        var recordStatus = new int[] { (int)RMRecordStatus.Active, (int)RMRecordStatus.Closed };
        //        SqlQuerySpec sqlQuery = null;
        //        if (isEnduser)
        //        {
        //            sqlQuery = CosmosSqlQueryHelper.BuildSearchForEnduser(value, exceptIds.ToArray(), nodeTypes.ToArray(), recordStatus, permissions);
        //        }
        //        else
        //        {
        //            sqlQuery = CosmosSqlQueryHelper.BuildSearch(value, exceptIds.ToArray(), nodeTypes.ToArray(), recordStatus, sourceFlags);
        //        }

        //        var sqlDefinition = From(sqlQuery);

        //        result = QueryPageBySql(sqlDefinition, pageSize, pageIndex);

        //    }
        //    else
        //    {
        //        if (RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled())
        //        {
        //            //RemoveNoPermissionFourceFlags(sourceFlags);
        //            //if (sourceFlags.Contains(SourceFlag.SharePoint) || sourceFlags.Contains(SourceFlag.Exchange))
        //            //{
        //            var permissionCheckResult = SecurityTrimmingHelper.Check(sourceFlags);
        //            if (permissionCheckResult.NeedCheck)
        //            {
        //                permissionCheckResult.RemoveSourceFlags(sourceFlags);
        //                var containerIds = permissionCheckResult.GetContainerIds();
        //                var otherSourceFlags = sourceFlags.Except(new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.Exchange })
        //                    .Select(o => (int)o).ToList();

        //                lambda = GetNewLambda(nodeTypes, exceptIds, containerIds, otherSourceFlags);
        //            }
        //            //}
        //        }

        //        result = QueryByPage(lambda, pageSize, pageIndex);
        //    }

        //    hasNext = !string.IsNullOrEmpty(result.Item2);
        //    return result;
        //}

        public Tuple<IEnumerable<Record>, string> SearchRecordsV2(ExplorerQueryV2Dto dto, SqlQuerySpecBuilder sqlQuerySpecBuilder = null)
        {
            //sqlQuerySpecBuilder = sqlQuerySpecBuilder ?? SqlQuerySpecBuilderFactory.Create(); //if not has a builder, use the default builder

            //var sqlQuery = sqlQuerySpecBuilder.Build(dto.QueryOption);
            //logger.Info($"SQL query V2 : {sqlQuery.QueryText}");
            var sqlQuery = GetSqlQuerySpec(dto, sqlQuerySpecBuilder);
            logger.Info($"SQL query V2 : {sqlQuery.QueryText}");
            logSqlParam(sqlQuery.Parameters);

            var result = _repository.QueryPageBySqlAsync(From(sqlQuery), dto.PagingInfo.PageSize, dto.PagingInfo.PageIndex).Result;

            return result.ToTuple();
        }

        public int QueryCount(ExplorerQueryV2Dto dto, SqlQuerySpecBuilder sqlQuerySpecBuilder = null)
        {
            //sqlQuerySpecBuilder = sqlQuerySpecBuilder ?? SqlQuerySpecBuilderFactory.Create(); //if not has a builder, use the default builder
            //var sqlQuery = sqlQuerySpecBuilder.Build(dto.QueryOption, true);
            //logger.Info($"SQL query total count V2 : {sqlQuery.QueryText}");

            var sqlQuery = GetSqlQuerySpec(dto, sqlQuerySpecBuilder, true);
            var r = _repository.QueryAllBySqlAsync<dynamic>(From(sqlQuery)).Result.First();
            return Convert.ToInt32(r);
        }

        #region advanced search
        public Tuple<IEnumerable<Record>, string> SearchRecordsV3(ExplorerQueryV3Dto dto, ExplorerFilterOptionV2 builtinFilterOption, SqlQuerySpecBuilder sqlQuerySpecBuilder = null, bool suggestSearch = false)
        {
            using (var performance = new PerformanceScope("ExplorerDAOV2.SearchRecordsV3"))
            {
                var sqlQueryBuilder = suggestSearch ? SqlQuerySpecBuilderFactory.CreateDirPathSuggestionSearchBuilder() : sqlQuerySpecBuilder;
                var sqlQuery = GetSqlQuerySpecV3(dto.QueryOption, builtinFilterOption, sqlQueryBuilder);
                logSqlParam(sqlQuery.Parameters);
                var result = _repository.QueryPageBySqlAsync(From(sqlQuery), dto.PagingInfo.PageSize, dto.PagingInfo.PageIndex).Result;

                return result.ToTuple();
            }
        }

        public int QueryCountV3(ExplorerQueryV3Dto dto, ExplorerFilterOptionV2 builtinFilterOption, SqlQuerySpecBuilder sqlQuerySpecBuilder = null, bool suggestSearch = false)
        {
            var sqlQueryBuilder = suggestSearch ? SqlQuerySpecBuilderFactory.CreateDirPathSuggestionSearchBuilder() : sqlQuerySpecBuilder;
            var sqlQuery = GetSqlQuerySpecV3(dto.QueryOption, builtinFilterOption, sqlQueryBuilder, true);
            var r = _repository.QueryAllBySqlAsync<dynamic>(From(sqlQuery)).Result.First();
            return Convert.ToInt32(r);
        }

        private SqlQuerySpec GetSqlQuerySpecV3(ExplorerQueryOptionV3 queryOptionV3, ExplorerFilterOptionV2 builtinFilterOption, SqlQuerySpecBuilder sqlQuerySpecBuilder, bool queryTotalCount = false)
        {
            queryOptionV3.Values = NormalizeSpoLocationEqualsToDirPath(queryOptionV3.Values);
            sqlQuerySpecBuilder = sqlQuerySpecBuilder ?? SqlQuerySpecBuilderFactory.CreateDefaultAdvancedSearchBuilder(); //if not has a builder, use the default builder
            var sqlQuery = sqlQuerySpecBuilder.BuildAdvancedSearch(queryOptionV3, builtinFilterOption, queryTotalCount);
            logger.Info($"SQL query V3 :");
            return sqlQuery;
        }
        #endregion

        private SqlQuerySpec GetSqlQuerySpec(ExplorerQueryV2Dto dto, SqlQuerySpecBuilder sqlQuerySpecBuilder, bool queryTotalCount = false)
        {
            sqlQuerySpecBuilder = sqlQuerySpecBuilder ?? SqlQuerySpecBuilderFactory.Create(); //if not has a builder, use the default builder
            var sqlQuery = sqlQuerySpecBuilder.Build(dto.QueryOption, queryTotalCount);
            logger.Info($"SQL query V2 :");
            return sqlQuery;
        }

        private List<ExplorerSearchOptionV3> NormalizeSpoLocationEqualsToDirPath(List<ExplorerSearchOptionV3> values)
        {
            return [.. values
                .Select(x=>
                {
                    if (x.Column.Id == QueryCloumnIds.SPOLocation && x.ColumnOperationLogic == ExplorerSearchColumnOperationLogic.Equals)
                    {
                        x.Column.Id = QueryCloumnIds.DirPath;
                    }

                    return x;
                })];
        }

        //public void AddReocrdHistory(List<Guid> id, RecordHistoryXml xmlDto)
        //{
        //    var history = QueryAll(s => id.Contains(s.Id)).Select(m => new { m.Id, m.RecordHistory }).ToList();
        //    foreach (var his in history)
        //    {
        //        string str = string.Empty;
        //        if (!string.IsNullOrEmpty(his.RecordHistory))
        //        {
        //            var old = XmlUtil.GetXmlObject<RecordHistoryXml>(his.RecordHistory);
        //            old.HistoryList.AddRange(xmlDto.HistoryList);
        //            str = XmlUtil.GetXmlString(old);
        //        }
        //        else
        //        {
        //            str = XmlUtil.GetXmlString(xmlDto);
        //        }
        //        UpdateAll(s => s.Id == his.Id, rec => { rec.RecordHistory = str; });
        //    }
        //}

        public List<Record> GetRecordByIds(List<Guid> ids)
        {
            return QueryAll(r => ids.Contains(r.Id)).ToList();
        }

        public List<Record> GetRecordByRecordsIds(List<string> recordsId)
        {
            return QueryAll(r => recordsId.Contains(r.RecordsId)).ToList();
        }

        public List<Record> GetActiveRecordsByIds(List<Guid> ids)
        {
            return QueryAll(r => ids.Contains(r.Id) && r.RecordStatus != (int)RMRecordStatus.RMDeleted
                                                    && r.RecordStatus != (int)RMRecordStatus.Destroyed).ToList();
        }
        public List<Record> GetRecordsByNodeIds(List<Guid> ids)
        {
            return QueryAll(r => ids.Contains(r.NodeId)).ToList();
        }
        public List<Record> GetHoldRecordsByIds(List<Guid> ids)
        {
            return QueryAll(r => ids.Contains(r.Id) && r.HoldStatus).ToList();
        }

        public List<Record> GetPhysicalRecordByRecordIds(List<string> uniqueIds)
        {
            return QueryAll(r => r.ScopeId == Guid.Empty && uniqueIds.Contains(r.RecordsId)).ToList();
        }

        public int UpdateRecordOwner(Guid scopeId, Guid nodeId, string owners)
        {
            owners = AddBeforeAndAfterSeparator(owners);
            return UpdateAll(s => s.ScopeId == scopeId && s.NodeId == nodeId, r => { r.RecordOwner = owners; r.RecordOwner_Array = owners.ExplorerSearchSplit(); });
        }

        public int UpdateRecordOwnerForFS(Guid nodeId, string owners)
        {
            owners = AddBeforeAndAfterSeparator(owners);
            return UpdateAll(s => s.NodeId == nodeId, r => { r.RecordOwner = owners; r.RecordOwner_Array = owners.ExplorerSearchSplit(); });
        }

        private void UpdateDisposalDueDate(Record rec, Record dbRec, RMRule tempRule = null)
        {
            //Hold状态Record重新计算Due Date;
            if (dbRec.HoldStatus)
            {
                if (rec.RuleId != null && rec.RuleId != Guid.Empty)
                {
                    if (tempRule == null)
                    {
                        tempRule = RMRuleDao.GetRuleById(rec.RuleId);
                    }
                    if (tempRule != null && IsRemoveRule(tempRule, dbRec.SourceFlag))
                    {
                        long newDisposalDueDate = 0;
                        //Remove Rule需要计算Due Date
                        if (rec.DisposalDueDate == DueDateUtil.NextJob)
                        {
                            newDisposalDueDate = dbRec.HoldReleaseTime;
                        }
                        if (rec.DisposalDueDate > 0)
                        {
                            if (rec.DisposalDueDate > dbRec.HoldReleaseTime)
                            {
                                newDisposalDueDate = rec.DisposalDueDate;
                            }
                            else
                            {
                                newDisposalDueDate = dbRec.HoldReleaseTime;
                            }
                        }
                        rec.DisposalDueDate = newDisposalDueDate;
                    }
                }
            }
        }

        public void UpdateRecordDisposalDueDate(Guid scopeId, Guid recordId, long disposalDueDate, string ruleId, int ruleLevel)
        {
            UpdateAll(s => s.ScopeId == scopeId && s.Id == recordId, r => { r.DisposalDueDate = disposalDueDate; r.RuleId = new Guid(ruleId); r.RuleLevel = ruleLevel; });
        }

        public bool AddOrUpdateRecordWithKeepManual(Record rec, bool forceUpdate, RMRule tempRule = null, bool isKeepManualColumn = true)
        {
            bool result = false;
            rec.AppendCustomColumns();
            var dbRec = ReadById(rec.ScopeId, rec.Id);
            if (dbRec != null && (dbRec.RecordStatus == (int)RMRecordStatus.Active || dbRec.RecordStatus == (int)RMRecordStatus.RMDeleted || dbRec.RecordStatus == (int)RMRecordStatus.TrainingManualSync))
            {
                if(isKeepManualColumn)
                    rec.KeepOldManualColumn(dbRec);
                rec.KeepMachineLearningPredictInfo(dbRec);
                UpdateDisposalDueDate(rec, dbRec, tempRule);
                if (forceUpdate)
                {
                    _repository.UpsertItemAsync(rec).Wait();
                    result = true;
                }
                else
                {
                    if (rec.HaveFieldsValueChanged(dbRec))
                    {
                        result = true;
                        _repository.UpsertItemAsync(rec).Wait();
                    }
                }
            }
            else if (dbRec != null && dbRec.RecordStatus == (int)RMRecordStatus.ManualPreSync)
            {
                if (isKeepManualColumn && dbRec != null && dbRec.CheckExistAndTagDuplicateManual())
                {
                    rec.KeepOldManualColumn(dbRec);
                }
                _repository.UpsertItemAsync(rec).Wait();
                result = true;
            }
            else if (dbRec == null)
            {
                _repository.AddAsync(rec).Wait();
                result = true;
            }
            return result;
        }

        public bool AddOrUpdateRecord(Record rec, bool forceUpdate, RMRule tempRule = null)
        {
            bool result = false;
            rec.AppendCustomColumns();
            var dbRec = ReadById(rec.ScopeId, rec.Id);
            if (dbRec != null && (dbRec.RecordStatus == (int)RMRecordStatus.Active || dbRec.RecordStatus == (int)RMRecordStatus.RMDeleted || dbRec.RecordStatus == (int)RMRecordStatus.TrainingManualSync))
            {
                //Hold状态Record重新计算Due Date;
                rec.KeepOldManualColumn(dbRec);
                rec.KeepMachineLearningPredictInfo(dbRec);
                UpdateDisposalDueDate(rec, dbRec, tempRule);
                if (forceUpdate)
                {
                    UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
                    {
                        r.CopyFrom(rec);
                    });
                    result = true;
                }
                else
                {
                    // compare uniqueid, add for document ID feature
                    if (rec.HaveFieldsValueChanged(dbRec))  //add comparing record owner
                    {
                        result = true;
                        UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
                        {
                            r.CopyFrom(rec);
                        });
                    }
                }
            }
            else if (dbRec != null && dbRec.RecordStatus == (int)RMRecordStatus.ManualPreSync)
            {
                if (dbRec != null && dbRec.CheckExistAndTagDuplicateManual())
                {
                    rec.KeepOldManualColumn(dbRec);
                }
                _repository.UpsertItemAsync(rec).Wait();
                result = true;
            }
            else if (dbRec == null)
            {
                _repository.AddAsync(rec).Wait();
                result = true;
            }
            return result;
        }

        public bool NeedUpdateRecord(Record rec, bool forceUpdate, RMRule tempRule = null)
        {
            bool result = false;
            rec.AppendCustomColumns();
            var dbRec = ReadById(rec.ScopeId, rec.Id);
            if (dbRec.CheckExistAndTagDuplicateManual())
            {
                //Hold状态Record重新计算Due Date;
                rec.KeepOldManualColumn(dbRec);
                rec.KeepMachineLearningPredictInfo(dbRec);
                UpdateDisposalDueDate(rec, dbRec, tempRule);
                if (forceUpdate || rec.HaveFieldsValueChanged(dbRec))
                {
                    result = true;
                }
            }
            else if (dbRec == null)
            {
                result = true;
            }
            return result;
        }

        public bool NeedUpdateRecord(Record rec, bool forceUpdate, Record dbRec, RMRule tempRule = null)
        {
            bool result = false;
            rec.AppendCustomColumns();
            if (dbRec.CheckExistAndTagDuplicateManual())
            {
                rec.KeepOldManualColumn(dbRec);
                rec.KeepMachineLearningPredictInfo(dbRec);
                //Hold状态Record重新计算Due Date;
                UpdateDisposalDueDate(rec, dbRec, tempRule);
                if (forceUpdate || rec.HaveFieldsValueChanged(dbRec))
                {
                    result = true;
                }
            }
            else if (dbRec == null)
            {
                result = true;
            }
            return result;
        }

        public bool NeedUpdateMLManualRecord(Record rec, bool forceUpdate, Record dbRec, RMRule tempRule = null)
        {
            bool result = false;
            //rec.AppendCustomColumns();
            if (dbRec.CheckExistAndTagDuplicateManual())
            {
                //Hold状态Record重新计算Due Date;
                //rec.KeepOldManualColumn(dbRec);
                //UpdateDisposalDueDate(rec, dbRec, tempRule);
                if (forceUpdate || rec.HaveFieldsValueChangedMLManual(dbRec))
                {
                    result = true;
                }
            }
            else if (dbRec == null)
            {
                result = true;
            }
            return result;
        }

        public void UpdateRecordState(Record rec, int status, List<Guid> subFolderIds = null)
        {
            if (rec != null)
            {
                Expression<Func<Record, bool>> lambda = s => s.ScopeId == rec.ScopeId;
                switch (rec.NodeType)
                {
                    case (int)NodeLevel.Site:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId;
                        break;
                    case (int)NodeLevel.List:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId;
                        break;
                    case (int)NodeLevel.Folder:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.DirPath.StartsWith(rec.DirPath);
                        break;
                    case (int)NodeLevel.Item:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.ItemRowId == rec.ItemRowId;
                        break;
                    default:
                        break;
                }
                int result = UpdateAll(lambda, r => { r.RecordStatus = status; });
                logger.Info($"Update record: {rec.Id} success. Count:{result}");
            }
        }

        public List<Guid> GetAllSubFolderUnderFolder(Record rec)
        {
            var result = new List<Guid>();
            if (rec.NodeType == (int)NodeLevel.Folder)
            {
                var currentFolderId = rec.NodeId;
                if (currentFolderId != Guid.Empty)
                {
                    //当前Folder需要记录
                    result.Add(currentFolderId);
                    var tempList = GetAllSubFolderIds(rec);
                    result.AddRange(tempList);
                }
            }
            return result;
        }

        public void UpdateRecordState(Guid scopeId, Guid id, int status)
        {
            var rec = QueryAll(s => s.ScopeId == scopeId && s.Id == id).FirstOrDefault();
            if (rec != null)
            {
                UpdateAll(s => s.ScopeId == scopeId && s.Id == id && s.RecordStatus == (int)RMRecordStatus.Active, r => { r.RecordStatus = status; });
            }
        }

        public Record ReadSPRecordById(Guid scopeId, Guid webId, Guid listId, int itemRowId)
        {
            return QueryAll(s => s.ScopeId == scopeId && s.WebId == webId && s.ListId == listId && s.ItemRowId == itemRowId).FirstOrDefault();
        }

        public bool CheckHasData()
        {
            return _repository.ExistAsync(r => true).Result;
        }

        /// <summary>
        /// To Test
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public List<string> GetContainersInDB(string dbName)
        {
            return _repository.GetContainersInDBAsync(dbName).Result;
        }

        //public void WaitForIndexTransformationToComplete()
        //{
        //    throw new NotImplementedException();
        //}

        public List<Record> GetWaitingApproveItemForPhysical()
        {
            return QueryAll(m => m.DisposalStatus == (int)SOApproveDBStatus.WaitingApprove && m.ExportToRECO == false
                   && m.SourceFlag == (int)SourceFlag.Physical
                   && (m.NodeType == (int)RMNodeType.PhyFile || m.NodeType == (int)RMNodeType.PhyBox)).ToList();
        }

        public void UpdateItemToExportStatus(Guid id)
        {
            var rec = QueryAll(s => s.Id == id).FirstOrDefault();
            if (rec != null)
            {
                UpdateAll(s => s.Id == id, r => { r.ExportToRECO = true; });
            }
        }

        public void UpdateRecordOwnerForPhysical(Guid id, string owners)
        {
            owners = AddBeforeAndAfterSeparator(owners);
            UpdateAll(s => s.Id == id, r => { r.RecordOwner = owners; r.RecordOwner_Array = owners.ExplorerSearchSplit(); });
        }

        public Record GetPhysicalRecordById(Guid id)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.Physical 
            && s.Id == id 
            && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed))
                .FirstOrDefault();
        }

        public Record GetPhysicalRecordByRecordsId(string uniqueId)
        {
            return QueryAll(s => s.ScopeId == Guid.Empty && s.RecordsId == uniqueId).FirstOrDefault();
        }

        public Record GetPhysicalRecordByRecordsIdAndTemId(string uniqueId, int temId)
        {
            return QueryAll(s => s.ScopeId == Guid.Empty && s.RecordsId == uniqueId && s.TemplateId == temId).FirstOrDefault();
        }

        public void UpdateApproveStatus(Guid id, SOApproveDBStatus status)
        {
            var rec = QueryAll(s => s.Id == id).FirstOrDefault();
            if (rec != null)
            {
                UpdateAll(s => s.Id == id, r => { r.DisposalStatus = (int)status; });
            }
        }

        public bool AddPhysicalRecord(Record rec)
        {
            bool result = true;
            try
            {
                rec.AppendCustomColumns();
                _repository.AddAsync(rec).Wait();
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        public bool UpdatePhysicalRecord(Record rec, bool forceUpdate, bool isModifyPermissionId = false, bool isUpdateManualProperties = false)
        {
            bool result = false;
            rec.AppendCustomColumns();
            if (forceUpdate)
            {
                UpdateAll(r => r.NodeId == rec.NodeId, r =>
                {
                    r.LeafName = rec.LeafName;
                    r.NodeType = rec.NodeType;
                    r.RecordsId = rec.RecordsId;
                    r.TermId = rec.TermId;
                    r.TermName = rec.TermName;
                    r.LocationId = rec.LocationId;
                    r.BoxId = rec.BoxId;
                    r.FileId = rec.FileId;
                    r.TemplateId = rec.TemplateId;
                    r.IsLocked = rec.IsLocked;
                    r.MetaInfo = rec.MetaInfo;
                    r.CustomColumnDic = rec.CustomColumnDic;
                    r.TimeCreated = rec.TimeCreated;
                    r.TimeModified = rec.TimeModified;
                    r.CreatedBy = rec.CreatedBy;
                    r.ModifiedBy = rec.ModifiedBy;
                    r.DisposalDueDate = rec.DisposalDueDate;
                    r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
                    r.RuleId = rec.RuleId;
                    r.RuleLevel = rec.RuleLevel;
                    r.RecordStatus = rec.RecordStatus;
                    r.DisposalStatus = rec.DisposalStatus;
                    r.ExportToRECO = rec.ExportToRECO;
                    r.DestroyedTime = rec.DestroyedTime;
                    r.HoldType = rec.HoldType;
                    r.HoldBy = rec.HoldBy;
                    r.HoldReleaseTime = rec.HoldReleaseTime;
                    r.HoldId = rec.HoldId;
                    r.HoldStatus = rec.HoldStatus;
                    r.HoldByUsers = rec.HoldByUsers;
                    r.HoldUntilTimes = rec.HoldUntilTimes;
                    r.AppendHolds_Array = rec.AppendHolds_Array;
                    r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
                    r.RelatedRecords = rec.RelatedRecords;
                    r.RelatedRecordsCount = rec.RelatedRecordsCount;
                    r.ScopePermissionId = isModifyPermissionId ? rec.ScopePermissionId : r.ScopePermissionId;
                    r.LeafName_Array = rec.LeafName_Array;
                    r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
                    r.CreatedBy_Lower = rec.CreatedBy_Lower;
                    r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
                    r.ModifiedBy_Array = rec.ModifiedBy_Array;
                    r.CreatedBy_Array = rec.CreatedBy_Array;
                    r.DeclaredBy_Array = rec.DeclaredBy_Array;
                    r.ParentId = rec.ParentId;
                    r.Ancestors = rec.Ancestors;
                    r.ManualArchiveStatus = isUpdateManualProperties ? rec.ManualArchiveStatus : r.ManualArchiveStatus;
                    r.PhysicalActionAudit = rec.PhysicalActionAudit;
                });
                result = true;
            }
            else
            {
                UpdateAll(r => r.NodeId == rec.NodeId, r =>
                {
                    r.LeafName = rec.LeafName;
                    r.TermId = rec.TermId;
                    r.TermName = rec.TermName;
                    r.IsLocked = rec.IsLocked;
                    r.MetaInfo = rec.MetaInfo;
                    r.CustomColumnDic = rec.CustomColumnDic;
                    r.TimeModified = rec.TimeModified;
                    r.ModifiedBy = rec.ModifiedBy;
                    r.DisposalDueDate = rec.DisposalDueDate;
                    r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
                    r.RuleId = rec.RuleId;
                    r.RuleLevel = rec.RuleLevel;
                    r.RecordStatus = rec.RecordStatus;
                    r.DisposalStatus = rec.DisposalStatus;
                    r.ExportToRECO = rec.ExportToRECO;
                    r.DestroyedTime = rec.DestroyedTime;
                    r.HoldType = rec.HoldType;
                    r.HoldBy = rec.HoldBy;
                    r.HoldReleaseTime = rec.HoldReleaseTime;
                    r.HoldId = rec.HoldId;
                    r.HoldStatus = rec.HoldStatus;
                    r.HoldByUsers = rec.HoldByUsers;
                    r.HoldUntilTimes = rec.HoldUntilTimes;
                    r.AppendHolds_Array = rec.AppendHolds_Array;
                    r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
                    r.ScopePermissionId = isModifyPermissionId ? rec.ScopePermissionId : r.ScopePermissionId;
                    r.LeafName_Array = rec.LeafName_Array;
                    r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
                    r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
                    r.ModifiedBy_Array = rec.ModifiedBy_Array;
                    r.DeclaredBy_Array = rec.DeclaredBy_Array;
                    r.ParentId = rec.ParentId;
                    r.Ancestors = rec.Ancestors;
                    r.ManualArchiveStatus = isUpdateManualProperties ? rec.ManualArchiveStatus : r.ManualArchiveStatus;
                    r.PhysicalActionAudit = rec.PhysicalActionAudit;
                });
                result = true;
            }
            return result;
        }

        public List<Record> GetRecordsBySql(List<Guid> recordIds)
        {
            var sqlText = "SELECT * FROM c Where ARRAY_CONTAINS(@idArrays,c.id, false)";
            var parameters = new SqlParameterCollection();
            parameters.Add(new SqlParameter("@idArrays", recordIds.ToArray()));
            SqlQuerySpec sqlQuery = new SqlQuerySpec()
            {
                QueryText = sqlText,
                Parameters = parameters
            };
            return _repository.QueryAllBySqlAsync(From(sqlQuery)).Result.ToList();
        }

        public List<Record> GetRecordbyHoldId(string holdId)
        {
            var sqlText = "SELECT * FROM c Where (ARRAY_CONTAINS(c.appendHolds_Array, @holdId, false) or c.holdId = @holdId) AND ARRAY_CONTAINS([1,6,2,7],c.recordStatus, false) ";
            var parameters = new SqlParameterCollection
            {
                new SqlParameter("@holdId", holdId)
            };
            SqlQuerySpec sqlQuery = new SqlQuerySpec()
            {
                QueryText = sqlText,
                Parameters = parameters
            };
            return _repository.QueryAllBySqlAsync(From(sqlQuery)).Result.ToList();
        }

        public List<Record> GetRecordbyHoldIds(List<string> holdIds)
        {
            return _repository.QueryAllAysnc(r => (r.AppendHolds_Array != null && r.AppendHolds_Array.Any(c => holdIds.Contains(c))) || holdIds.Contains(r.HoldId)).Result.ToList();
        }

        public bool CanPhysicalFileMove(Guid fileId, Guid srcParentId, Guid destParentId)
        {
            //先检查源端的Hold状态
            var srcHolds = GetHoldRecordsByIds(new List<Guid>() { fileId, srcParentId });
            var destHold = GetHoldRecordsByIds(new List<Guid>() { destParentId }).FirstOrDefault();
            if (srcHolds.Any(a => a.Id == fileId))
            {
                var srcHold = srcHolds.First(a => a.Id == fileId);
                //File本身是Hold的
                if (destHold != null)
                {
                    //目的端的Container 有Hold, 比较HOld Id或者ReleaseTime是否相同
                    if (srcHold.HoldId != destHold.HoldId)
                    {
                        //异常失败, 不允许Move
                        //throw new GCommon.Utility.AveException("Dest container has a different hold time.");
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsRecordsHold(List<Guid> ids, long ticks)
        {
            int disposalCount = _repository.CountAsync((a => a.HoldReleaseTime > ticks && ids.Contains(a.Id))).Result;
            return disposalCount > 0;
        }


        public List<Record> GetRecordsByIdPermssions(List<int> scopePermissions, List<Guid> recordIds)
        {
            var sqlQuery = CosmosSqlQueryHelper.GetRecordsByPermission(scopePermissions, recordIds);
            return _repository.QueryAllBySqlAsync(From(sqlQuery)).Result.ToList();
        }

        public Tuple<IEnumerable<Record>, string> GetRecordsByContainer(Guid scopeId, string containerId, string continuation, int pageSize)
        {
            var sqlQuery = CosmosSqlQueryHelper.GenerateContianerIdQueyExpression(scopeId, containerId);
            var list = QueryPageBySql(From(sqlQuery), pageSize, continuation);
            return list;
        }

        public Tuple<IEnumerable<Record>, string> GetRecordsByContainerAndNodeType(Guid scopeId, string containerId,List<int> nodeTypes,string url,string continuation, int pageSize)
        {
            var sqlQuery = CosmosSqlQueryHelper.GenerateQueryByContainerAndNodeTypeExpression(scopeId, containerId, nodeTypes, url);
            var list = QueryPageBySql(From(sqlQuery), pageSize, continuation);
            return list;
        }
        public List<Record> GetRecordBoxsByBoxIds(List<Guid> boxIds)
        {
            return QueryAll(r => boxIds.Contains(r.Id)).ToList();
        }

        public Tuple<IEnumerable<Record>, string> SearchPhysicalBoxOrFolderByName(string searchKey, string continuation, int pageSize, bool isGlobalSearch, bool isSearchFolder, string locationId)
        {
            var sqlQuery = CosmosSqlQueryHelper.GenerateSearchPhysicalBoxOrFolderBySearchKey(searchKey, isGlobalSearch,  isSearchFolder, locationId);
            var list = QueryPageBySql(From(sqlQuery), pageSize, continuation);
            return list;
        }

        public long GetTotalSizeByNodeTypeAndDirPaths(int nodeType, int sourceFlag, int status, List<string> dirPaths)
        {
            if (dirPaths == null || !dirPaths.Any())
                return 0;

            var conditions = new List<string>();
            var parameters = new SqlParameterCollection
            {
                new SqlParameter("@nodeType", nodeType),
                new SqlParameter("@sourceFlag", sourceFlag),
                new SqlParameter("@recordStatus", status)
            };

            var pathConditions = new List<string>();
            var idConditions = new List<string>();
            for (int i = 0; i < dirPaths.Count; i++)
            {
                var paramName = $"@dirPath{i}";

                var path = dirPaths[i];
                var id = path.ToLowerInvariant().ToMd5();


                parameters.Add(new SqlParameter(paramName, path));
                pathConditions.Add($"c.dirPath = {paramName} OR STARTSWITH(c.dirPath, CONCAT({paramName}, '\\\\'))");
                //var paramName = $"@id{i}";
                //var id = dirPaths[i].ToLowerInvariant().ToMd5();

                //parameters.Add(new SqlParameter(paramName, id));
                //idConditions.Add($"c.id = {paramName}");
            }
            conditions.Add("IS_DEFINED(c.jpmcFileSize)");
            conditions.Add($"({string.Join(" OR ", pathConditions)})");
            conditions.Add("c.nodeType = @nodeType");
            conditions.Add("c.sourceFlag = @sourceFlag");
            conditions.Add("c.recordStatus = @recordStatus");

            var sqlText = $@"
                SELECT VALUE SUM(c.jpmcFileSize)
                FROM c
                WHERE {string.Join(" AND ", conditions)}
            ";

            var sqlQuery = new SqlQuerySpec
            {
                QueryText = sqlText,
                Parameters = parameters
            };
            return _repository.QueryAllBySqlAsync<long?>(From(sqlQuery))
                              .Result
                              .FirstOrDefault() ?? 0;
        }

        public Tuple<IEnumerable<Record>, string> SearchFileSystemBySearchKey(string searchKey, string continuation, int pageSize)
        {
            var sqlQuery = CosmosSqlQueryHelper.GenerateSearchFileSystemBySearchKey(searchKey);
            var list = QueryPageBySql(From(sqlQuery), pageSize, continuation);
            return list;
        }

        public Tuple<IEnumerable<Record>, string> SearchFileSystemBySearchKeyAndConnectionIds(string searchKey, IEnumerable<Guid> connectionIds, string continuation, int pageSize)
        {
            var sqlQuery = CosmosSqlQueryHelper.GenerateSearchFileSystemBySearchKeyAndConnectionIds(searchKey, connectionIds);
            var list = QueryPageBySql(From(sqlQuery), pageSize, continuation);
            return list;
        }

        public Tuple<IEnumerable<Record>, string> SearchByFullPath(List<string> fullPaths, int nodeType, int sourceFlag, string continuation, int pageSize)
        {
            var sqlQuery = CosmosSqlQueryHelper.GenerateSearchByFullPaths(fullPaths, nodeType, sourceFlag);
            var list = QueryPageBySql(From(sqlQuery), pageSize, continuation);
            return list;
        }

        public bool AddFileSystemRecord(Record rec)
        {
            bool result = true;
            try
            {
                rec.AppendCustomColumns();
                _repository.AddAsync(rec).Wait();
            }
            catch (Exception e)
            {
                throw;
            }
            return result;
        }

        public Record GetFSRecordById(Guid id)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.Id == id && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
        }

        public Record GetGoogleDriveRecordById(Guid id)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.Google && s.Id == id &&
                                 (s.RecordStatus == (int)RMRecordStatus.Active ||
                                  s.RecordStatus == (int)RMRecordStatus.Closed ||
                                  s.RecordStatus == (int)RMRecordStatus.Missing ||
                                  s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
        }

        public Record GetFSRecord(Guid ScopeId, Guid Id)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ScopeId == ScopeId && s.Id == Id && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
        }

        public List<Record> GetFSChildNodes(Guid parentId, int fsType)
        {
            if (fsType != (int)NodeLevel.FSFolder)
            {
                return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ParentId.Equals(parentId)).OrderBy(a => a.LeafName).ToList();
            }
            else
            {
                return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ParentId.Equals(parentId) && s.NodeType != (int)NodeLevel.FSFile).OrderBy(a => a.LeafName).ToList();
            }
        }

        public List<Record> GetFSConnectionUnderGroup(Guid connectionGroupId, int level)
        {
            var connections = new List<Record>();
            if (level == (int)NodeLevel.WebApplication)
                connections = QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ParentId == connectionGroupId && s.RecordStatus == 1).OrderBy(a => a.LeafName).ToList();
            return connections;
        }

        public string GetFSConnectionIdByItemId(Guid itemId)
        {
            return QueryAll(r => r.Id == itemId).FirstOrDefault().L2PartitionKey;
        }

        public Record GetFSRootNode()
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.NodeType == (int)NodeLevel.FSConnectionGroups).FirstOrDefault();
        }

        /// <summary>
        /// 只更新Sync job可以获取的数据
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public void UpdateFileSystemFolderForSync(Record rec)
        {
            UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
            {
                r.AveSiteId = rec.AveSiteId;
                r.DirPath = rec.DirPath;
                r.NodeId = rec.NodeId;
                r.NodeType = rec.NodeType;
                r.ParentId = rec.ParentId;
                r.LeafName = rec.LeafName;
                r.TermId = rec.TermId;
                r.TermName = rec.TermName;
                r.MetaInfo = rec.MetaInfo;
                r.TimeModified = rec.TimeModified;
                r.ModifiedBy = rec.ModifiedBy;
                r.RecordStatus = rec.RecordStatus;
                r.LeafName_Array = rec.LeafName_Array;
                r.ModifiedBy_Array = rec.ModifiedBy_Array;
            });
        }

        public bool UpdateFileSystemRecord(Record rec, bool forceUpdate)
        {
            bool result = false;
            rec.AppendCustomColumns();
            if (forceUpdate)
            {
                UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
                {
                    r.LeafName = rec.LeafName;
                    r.NodeType = rec.NodeType;
                    r.RecordsId = rec.RecordsId;
                    r.TermId = rec.TermId;
                    r.TermName = rec.TermName;
                    //r.LocationId = rec.LocationId;
                    //r.BoxId = rec.BoxId;
                    //r.FileId = rec.FileId;
                    //r.TemplateId = rec.TemplateId;
                    r.IsLocked = rec.IsLocked;
                    r.MetaInfo = rec.MetaInfo;
                    //r.TimeCreated = rec.TimeCreated;
                    r.TimeModified = rec.TimeModified;
                    r.CreatedBy = rec.CreatedBy;
                    r.ModifiedBy = rec.ModifiedBy;
                    r.DisposalDueDate = rec.DisposalDueDate;
                    r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
                    r.RuleId = rec.RuleId;
                    r.RuleLevel = rec.RuleLevel;
                    r.RecordStatus = rec.RecordStatus;
                    r.DisposalStatus = rec.DisposalStatus;
                    r.ExportToRECO = rec.ExportToRECO;
                    r.DestroyedTime = rec.DestroyedTime;
                    r.HoldType = rec.HoldType;
                    r.HoldBy = rec.HoldBy;
                    r.HoldReleaseTime = rec.HoldReleaseTime;
                    r.HoldId = rec.HoldId;
                    r.HoldStatus = rec.HoldStatus;
                    r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
                    r.RelatedRecords = rec.RelatedRecords;
                    r.RelatedRecordsCount = rec.RelatedRecordsCount;
                    r.ScopePermissionId = rec.ScopePermissionId;
                    r.RecordOwner = rec.RecordOwner;
                    r.SortTicks = rec.SortTicks;
                    r.LeafName_Array = rec.LeafName_Array;
                    r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
                    r.CreatedBy_Lower = rec.CreatedBy_Lower;
                    r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
                    r.ModifiedBy_Array = rec.ModifiedBy_Array;
                    r.CreatedBy_Array = rec.CreatedBy_Array;
                    r.DeclaredBy_Array = rec.DeclaredBy_Array;
                    r.RecordOwner_Array = rec.RecordOwner_Array;
                });
                result = true;
            }
            else
            {
                UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
                {
                    r.LeafName = rec.LeafName;
                    r.TermId = rec.TermId;
                    r.TermName = rec.TermName;
                    r.IsLocked = rec.IsLocked;
                    r.MetaInfo = rec.MetaInfo;
                    r.TimeModified = rec.TimeModified;
                    r.ModifiedBy = rec.ModifiedBy;
                    r.DisposalDueDate = rec.DisposalDueDate;
                    r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
                    r.RuleId = rec.RuleId;
                    r.RuleLevel = rec.RuleLevel;
                    r.RecordStatus = rec.RecordStatus;
                    r.DisposalStatus = rec.DisposalStatus;
                    r.ExportToRECO = rec.ExportToRECO;
                    r.DestroyedTime = rec.DestroyedTime;
                    r.HoldType = rec.HoldType;
                    r.HoldBy = rec.HoldBy;
                    r.HoldReleaseTime = rec.HoldReleaseTime;
                    r.HoldId = rec.HoldId;
                    r.HoldStatus = rec.HoldStatus;
                    r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
                    r.ScopePermissionId = rec.ScopePermissionId;
                    r.RecordOwner = rec.RecordOwner;
                    r.LeafName_Array = rec.LeafName_Array;
                    r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
                    r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
                    r.ModifiedBy_Array = rec.ModifiedBy_Array;
                    r.DeclaredBy_Array = rec.DeclaredBy_Array;
                    r.RecordOwner_Array = rec.RecordOwner_Array;
                });
                result = true;
            }
            return result;
        }

        public List<Record> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem
            && (s.NodeType == (int)NodeLevel.FSFile
            && s.RecordStatus == (int)RMRecordStatus.Active
            && s.FolderId == (new Guid(folderId))
            && s.ScopeId == (new Guid(scopeId))
            || s.NodeType == (int)NodeLevel.FSFolder
            && s.Id == new Guid(folderId)
            && s.ScopeId == new Guid(scopeId)))
                .OrderBy(s => s.SortTicks)
                .Where(s => s.SortTicks > sortTicks)
                .Take(pageSize).ToList();
        }

        public bool HasFileMatchTerm(string path, string scopeId, List<Guid> classCodeIds)
        {
            if (classCodeIds == null || classCodeIds.Count == 0 || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var pathWithSep = path.EndsWith("\\") ? path : path + "\\";

            return QueryAll(s =>
                    s.SourceFlag == (int)SourceFlag.FileSystem
                    && s.NodeType == (int)NodeLevel.FSFile
                    && s.RecordStatus == (int)RMRecordStatus.Active
                    && s.L2PartitionKey == scopeId
                    && (
                        s.DirPath.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                        s.DirPath.StartsWith(pathWithSep, StringComparison.OrdinalIgnoreCase)
                       )
                    && classCodeIds.Contains(s.TermId))
                .Any();
        }

        public List<Record> GetExplorerDataByFolderAndEndTime(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            long timeNow = DateTime.UtcNow.Ticks;
            logger.Info($"GetExplorerDataByFolderAndEndTime the time now is:{timeNow}");
            var folderGuid = new Guid(folderId);
            var scopeGuid = new Guid(scopeId);
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem
            && s.EndTime != 0
            && (s.NodeType == (int)NodeLevel.FSFile
            && s.RecordStatus == (int)RMRecordStatus.Active
            && s.FolderId == folderGuid
            && s.ScopeId == scopeGuid
            || s.NodeType == (int)NodeLevel.FSFolder
            && s.Id == folderGuid
            && s.ScopeId == scopeGuid))
                .OrderBy(s => s.SortTicks)
                .Where(s => s.SortTicks > sortTicks)
                .Where(s =>  s.EndTime > 0 && s.EndTime < timeNow)
                .Take(pageSize).ToList();
        }
        public List<Record> GetExplorerDataByNodeIds(IEnumerable<Guid> nodeIds, string scopeId, long sortTicks)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem
                                 && (s.NodeType == (int)NodeLevel.FSFile
                                     && s.RecordStatus == (int)RMRecordStatus.Active
                                     && nodeIds.Contains(s.NodeId)
                                     && s.ScopeId == (new Guid(scopeId))
                                     || s.NodeType == (int)NodeLevel.FSFolder
                                     && nodeIds.Contains(s.Id)
                                     && s.ScopeId == new Guid(scopeId)))
                .OrderBy(s => s.SortTicks)
                .Where(s => s.SortTicks > sortTicks)
                .ToList();
        }
        public List<Record> GetExplorerDataByNodeIdsAndEndTime(IEnumerable<Guid> nodeIds, string scopeId, long sortTicks)
        {
            long timeNow = DateTime.UtcNow.Ticks;
            var scopeGuid = new Guid(scopeId);
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem
                        && s.EndTime!=0
                                 && (s.NodeType == (int)NodeLevel.FSFile
                                     && s.RecordStatus == (int)RMRecordStatus.Active
                                     && nodeIds.Contains(s.NodeId)
                                     && s.ScopeId == scopeGuid
                                     || s.NodeType == (int)NodeLevel.FSFolder
                                     && nodeIds.Contains(s.Id)
                                     && s.ScopeId == scopeGuid))
                .OrderBy(s => s.SortTicks)
                .Where(s => s.SortTicks > sortTicks)
                .Where(s =>  s.EndTime > 0 && s.EndTime < timeNow)
                .ToList();
        }
        public List<Record> GetDBRecordsByClassCodeAndFilterByEndTime(IEnumerable<Guid> nodeIds, IEnumerable<Guid> classCodeIds, string scopeId, long sortTicks)
        {
            long timeNow = DateTime.UtcNow.Ticks;
            var scopeGuid = new Guid(scopeId);

            var dbQuery = QueryAll(s =>
                s.SourceFlag == (int)SourceFlag.FileSystem
                && s.ScopeId == scopeGuid
                && classCodeIds.Contains(s.TermId)
                && s.SortTicks > sortTicks
                && (
                    (s.NodeType == (int)NodeLevel.FSFile
                        && s.RecordStatus == (int)RMRecordStatus.Active
                        && nodeIds.Contains(s.NodeId))
                    ||
                    (s.NodeType == (int)NodeLevel.FSFolder
                        && nodeIds.Contains(s.Id))
                )
            );

            return dbQuery
                .AsEnumerable()
                .Where(s => s.EndTime != 0
                    && s.EndTime > 0
                    && s.EndTime < timeNow)
                .OrderBy(s => s.SortTicks)
                .ToList();
        }
        public List<Record> GetOnPremiseSPExplorerDataByListId(string listId, string scopeId, long sortTicks, int pageSize)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.SharePointOnPrem
            && (s.NodeType == (int)NodeLevel.Item
            && s.RecordStatus == (int)RMRecordStatus.Active
            && s.ListId == (new Guid(listId))
            && s.ScopeId == (new Guid(scopeId))))
                .OrderBy(s => s.SortTicks)
                .Where(s => s.SortTicks > sortTicks)
                .Take(pageSize).ToList();
        }

        public int UpdateFSDeleteRecord(Guid id, Guid scopeId, int recordStatus)
        {
            return UpdateAll(r => r.ScopeId == scopeId && r.Id == id, rec => { rec.RecordStatus = recordStatus; rec.DestroyedTime = DateTime.UtcNow.Ticks; });
        }

        [Obsolete("Need to modify reference method assocication logic and remove internal query logic.")]
        public void Delete(int createDate, Guid id)
        {
            if(RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                var item = _repository.FirstOrDefault(r => r.Id == id);
                if(_repository != null && item != null)
                {
                var result = _repository.DeleteAsync(id.ToString(), item.BuildPartitionKey()).Result;
            }
            }
            else
            {
                var result = _repository.DeleteAsync(id.ToString(), new Microsoft.Azure.Cosmos.PartitionKey(createDate)).Result;
            }
        }

        public void Delete(Record record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

             _repository.DeleteAsync(record.Id.ToString(), record.BuildPartitionKey()).GetAwaiter().GetResult();
        }

        public async Task DeleteAsync(IEnumerable<Record> records)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            var recordList = records.ToList();
            if (!recordList.Any())
                return; 

            await _repository.DeleteRangeAsync(recordList).ConfigureAwait(false);
        }

        public List<Guid> BatchUpdate(List<Record> records, int bufferSize)
        {
            List<Guid> failedIds = new List<Guid>();
            using (new RA.Common.PerformanceScope("ExplorerDao.BatchUpdate"))
            {
                try
                {
                    if (records.Count == 0)
                    {
                        return new List<Guid>();
                    }
                    for (int i = 0; i < records.Count; i += bufferSize)
                    {
                        var tempRecords = records.Skip(i).Take(bufferSize).ToList();

                        var failedRecords = BulkUpsertDirectly(tempRecords);
                        foreach (var failedRecord in failedRecords)
                        {
                            if (!failedIds.Contains(failedRecord.Item1.NodeId))
                            {
                                failedIds.Add(failedRecord.Item1.NodeId);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while BatchUpdate records. error: {e.ToString()}");
                }
            }
            return failedIds;
        }

        //public Tuple<IEnumerable<Record>, string> QueryDataBySqlWithoutTotal(PhysicalExplorerQueryDto dto, string continuation, int pageSize, out bool hasNext, SecurityTermPermissionDto termPermDto, bool withoutPhysicalRecord = false)
        //{
        //    var sqlQuerySpec = CosmosSqlQueryHelper.BuildSerch(dto, termPermDto, withoutPhysicalRecord);
        //    logger.Info($"SQL query statement: {sqlQuerySpec.QueryText}");
        //    logSqlParam(sqlQuerySpec.Parameters);
        //    var list = QueryPageBySql(From(sqlQuerySpec), pageSize, continuation);
        //    hasNext = !string.IsNullOrEmpty(list.Item2);
        //    return list;
        //}

        public Tuple<IEnumerable<Record>, string> QueryPageBySqlForTermBrowse(RMNodeType nodeType, Guid termId, List<int> permissionIds, bool hasScopePermission, int pageCount = 15, string continuation = "")
        {
            var sqlQuerySpec = CosmosSqlQueryHelper.BuildSqlForBrowseTermTree(nodeType, termId, permissionIds, hasScopePermission);
            var result = _repository.QueryPageBySqlAsync(From(sqlQuerySpec), pageCount, continuation).Result.ToTuple();
            foreach (Record rec in result.Item1)
            {
                rec.AppendMetaInfoForOldLogic();
            }
            return result;
        }
        public Tuple<IEnumerable<Record>, string> QueryPageBySqlForTermBrowse(RMNodeType nodeType, Guid termId, List<int> permissionIds, bool hasScopePermission, List<Guid> bottomLocationIds, int pageCount = 15, string continuation = "")
        {
            var sqlQuerySpec = CosmosSqlQueryHelper.BuildSqlForBrowseTermTree(nodeType, termId, permissionIds, hasScopePermission, bottomLocationIds);
            var result = _repository.QueryPageBySqlAsync(From(sqlQuerySpec), pageCount, continuation).Result.ToTuple();
            foreach (Record rec in result.Item1)
            {
                rec.AppendMetaInfoForOldLogic();
            }
            return result;
        }
        public Dictionary<string, CustomColumn> GetUpdateColumns(Dictionary<string, string> metaInfoDic)
        {
            Dictionary<string, CustomColumn> customColumnDicResult = new Dictionary<string, CustomColumn>();
            //Convert metainfo string to List<CustomColumn>
            try
            {
                if (metaInfoDic.Count > 0)
                {
                    foreach (string key in metaInfoDic.Keys)
                    {
                        CustomColumn customColumn = new CustomColumn();
                        string content = metaInfoDic[key];
                        if (string.IsNullOrEmpty(content))
                        {
                            continue;
                        }
                        if (content.Contains("\":\""))
                        {
                            if (content.StartsWith("["))
                            {
                                if (content.Contains("\"UserPrincipalName\":\"") && content.Contains("\"InviteType\":"))
                                {
                                    try
                                    {
                                        List<AOSUserDto> users = JsonConvert.DeserializeObject<List<AOSUserDto>>(content);
                                        users.ForEach(a => a.UserPrincipalName = a.UserPrincipalName?.ToLower());
                                        customColumn.Users = users;
                                    }
                                    catch(Exception e)
                                    {
                                        logger.Error($"Deserialize Object failed, error : {e}");
                                    }
                                }
                                else if (content.Contains("\"Name\":") && content.Contains("\"Value\":"))
                                {
                                    try
                                    {
                                        List<ChoiceColumnValue> choices = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(content);
                                        customColumn.MultiChoice = choices;
                                    }
                                    catch(Exception e)
                                    {
                                        logger.Error($"Deserialize Object failed, error : {e}");
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    customColumn = JsonConvert.DeserializeObject<CustomColumn>(content);
                                }
                                catch
                                {
                                    customColumn.Value = content;
                                    customColumn.Number = GetNumber(content);
                                    customColumn.Value_Array = content.ExplorerAnalyzeBuiltInColumn();
                                }
                            }
                        }
                        else
                        {
                            customColumn.Value = content;
                            customColumn.Number = GetNumber(content);
                            customColumn.Value_Array = content.ExplorerAnalyzeBuiltInColumn();
                        }

                        if (customColumn.Date != default && customColumn.TimeZoneId != null)
                        {
                            try
                            {
                                customColumn.Date = Common.Util.DateTimeUtil.ConvertTimeToUtcDate(customColumn.Date, customColumn.TimeZoneId, customColumn.IsSetDayLight);
                            }
                            catch(Exception e)
                            {
                                logger.Error($"Convert time failed, error : {e}");
                            }
                        }
                        customColumnDicResult.Add(key, customColumn);
                    }
                }
                return customColumnDicResult;
            }
            catch (Exception e)
            {
                logger.Error($"GetBulkUpdateColumns error:{e}");
                return customColumnDicResult;
            }
        }

        public void AddArchivedRelatedColumn(Guid scopeId, Guid id, string pathMd5, string jobId, string index)
        {
            Record rec = QueryAll(r => r.ScopeId == scopeId && r.Id == id).FirstOrDefault();
            if (rec != null)
            {
                if (!string.IsNullOrWhiteSpace(rec.MetaInfo))
                {
                    var metaInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordMetaInfo>(rec.MetaInfo);
                    metaInfo.BackUpJobId = jobId;
                    metaInfo.PathMD5 = pathMd5;
                    metaInfo.ArchiverIndex = index;
                    rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                }
                else
                {
                    RecordMetaInfo metaInfo = new RecordMetaInfo()
                    {
                        BackUpJobId = jobId,
                        PathMD5 = pathMd5,
                        ArchiverIndex = index
                    };
                    rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                }
                Upsert(rec);
            }
        }

        public void UpdateRecordStatusAndDestroyedTime(Guid scopeId, Guid pathMD5, int recordStatus)
        {
            // recordStatus 1:active 2, archived, 3 delete
            UpdateAll(s => s.ScopeId == scopeId && s.Id == pathMD5, r => { r.RecordStatus = recordStatus; r.DestroyedTime = DateTime.UtcNow.Ticks; });
        }

        public void UpdateRecordStatusAndDestroyedTime4Manual(Guid scopeId, Guid pathMD5, int recordStatus)
        {
            // recordStatus 1:active 2, archived, 3 delete
            UpdateAll(s => s.ScopeId == scopeId && s.Id == pathMD5, r => { r.RecordStatus = recordStatus; r.DestroyedTime = DateTime.UtcNow.Ticks; r.ManualArchiveStatus = (int)ActionStatus.Archiverd; });
        }

        public void BatchUpdateRecordStatusAndDestroyedTime4Manual(List<Tuple<Guid, Guid>> recordIdentities, int recordStatus)
        {
            if (recordIdentities == null || recordIdentities.Count == 0)
            {
                return;
            }

            List<Record> records = new List<Record>();
            var destroyedTime = DateTime.UtcNow.Ticks;
            foreach (var recordIdentity in recordIdentities.Distinct())
            {
                var record = _repository.FirstOrDefault(record => record.AveSiteId == recordIdentity.Item1.ToString() && record.NodeId == recordIdentity.Item2);
                if (record == null)
                {
                    continue;
                }

                record.RecordStatus = recordStatus;
                record.DestroyedTime = destroyedTime;
                record.ManualArchiveStatus = (int)ActionStatus.Archiverd;
                records.Add(record);
            }

            BatchUpdate(records, 500);
        }

        public void UpdateRecordStatusToCancel(Guid scopeId, Guid pathMD5, int recordStatus)
        {
            // recordStatus 1:active 2, archived, 3 delete
            UpdateAll(s => s.ScopeId == scopeId && s.Id == pathMD5, 
                r => 
                { 
                    r.RecordStatus = recordStatus; 
                    r.DestroyedTime = DateTime.UtcNow.Ticks; 
                    r.ManualArchiveStatus = (int)ActionStatus.Archiverd; 
                    r.ManualApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                    r.ManualInternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                }
                );
        }

        private static double GetNumber(string content)
        {
            double result = default(double);
            if (content != null && content.Length < 255)
            {
                if (double.TryParse(content, out result))
                {
                    return result;
                }
            }
            return result;
        }

        private IRMRuleDao mRMRuleDao;
        protected IRMRuleDao RMRuleDao
        {
            get
            {
                if (mRMRuleDao == null)
                {
                    mRMRuleDao = (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao));
                }
                return mRMRuleDao;
            }
        }

        #region private method
        private bool IsRemoveRule(RMRule tempRule, int sourceFlag)
        {
            var result = false;
            int disposalAction = -1;
            if ((int)SourceFlag.SharePoint == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.DisposalAction);
                if (disposalAction == 0 || disposalAction == 2 || disposalAction == 5 || disposalAction == 7 || disposalAction == 8
                || disposalAction == 10 || disposalAction == 13 || disposalAction == 15 || disposalAction == 16 || disposalAction == 18
                || disposalAction == 21 || disposalAction == 23 || disposalAction == 24 || disposalAction == 25 || disposalAction == 26
                || disposalAction == 28 || disposalAction == 29 || disposalAction == 31 || disposalAction == 130 || disposalAction == 135
                || disposalAction == 138 || disposalAction == 143 || disposalAction == 146 || disposalAction == 151 || disposalAction == 154
                || disposalAction == 156 || disposalAction == 159)
                {
                    result = true;
                }
            }
            if ((int)SourceFlag.OneDrive == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.OneDriveDisposalAction);
                if (disposalAction == 0 || disposalAction == 2 || disposalAction == 5 || disposalAction == 7 || disposalAction == 8
                || disposalAction == 10 || disposalAction == 13 || disposalAction == 15 || disposalAction == 16 || disposalAction == 18
                || disposalAction == 21 || disposalAction == 23 || disposalAction == 24 || disposalAction == 25 || disposalAction == 26
                || disposalAction == 28 || disposalAction == 29 || disposalAction == 31 || disposalAction == 130 || disposalAction == 135
                || disposalAction == 138 || disposalAction == 143 || disposalAction == 146 || disposalAction == 151 || disposalAction == 154
                || disposalAction == 156 || disposalAction == 159)
                {
                    result = true;
                }
            }
            if ((int)SourceFlag.SharePointOnPrem == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.SPLocalDisposalAction);
                if (disposalAction == 0 || disposalAction == 2 || disposalAction == 5 || disposalAction == 7 || disposalAction == 8
                || disposalAction == 10 || disposalAction == 13 || disposalAction == 15 || disposalAction == 16 || disposalAction == 18
                || disposalAction == 21 || disposalAction == 23 || disposalAction == 24 || disposalAction == 26 || disposalAction == 29
                || disposalAction == 31 || disposalAction == 130 || disposalAction == 135 || disposalAction == 138 || disposalAction == 143
                || disposalAction == 146 || disposalAction == 151 || disposalAction == 154 || disposalAction == 159)
                {
                    result = true;
                }
            }
            if ((int)SourceFlag.Exchange == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.ExchangeDisposalAction);
                if (disposalAction == 0)
                {
                    result = true;
                }
            }
            if ((int)SourceFlag.FileSystem == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.FSDisposalAction);
                switch (disposalAction)
                {
                    case (int)RMContentDisposalAction.Remove:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.RelatedRecords:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords:
                        return true;
                    default:
                        break;
                }
            }
            return result;
        }

        private QueryDefinition From(SqlQuerySpec sqlQuerySpec)
        {
            QueryDefinition result = new QueryDefinition(sqlQuerySpec.QueryText);

            foreach (var param in sqlQuerySpec.Parameters)
            {
                result.WithParameter(param.Name, param.Value);
            }

            return result;
        }

        private void LogSqlQuerySpec(SqlQuerySpec sqlQuerySpec)
        {
            logger.Info($"SQL query statement:");
            logSqlParam(sqlQuerySpec.Parameters);
        }

        private void logSqlParam(SqlParameterCollection sqlParameter)
        {
            StringBuilder builder = new StringBuilder();
            foreach (SqlParameter p in sqlParameter)
            {
                var type = p.Value.GetType();
                if (type.FullName.StartsWith("System.Collections.Generic.List`1", StringComparison.OrdinalIgnoreCase))
                {
                    AssembleLogStringBuilder(builder, p.Name, p.Value as System.Collections.IList);
                }
                else if (type == typeof(int[]))
                {
                    AssembleLogStringBuilder(builder, p.Name, (p.Value as int[]));
                }
                else if (type == typeof(Guid[]))
                {
                    AssembleLogStringBuilder(builder, p.Name, (p.Value as Guid[]));
                }
                else if (type == typeof(string[]))
                {
                    AssembleLogStringBuilder(builder, p.Name, (p.Value as string[]));
                }
                else if (type == typeof(SourceFlag[]))
                {
                    AssembleLogStringBuilder(builder, p.Name, (p.Value as SourceFlag[]));
                }
                else if (type == typeof(RMNodeLevel[]))
                {
                    AssembleLogStringBuilder(builder, p.Name, (p.Value as RMNodeLevel[]));
                }
                else if (type == typeof(RMRecordStatus[]))
                {
                    AssembleLogStringBuilder(builder, p.Name, (p.Value as RMRecordStatus[]));
                }
                else
                {
                    builder.Append($"{p.Name} : {p.Value}").Append("; ");
                }
            }
            logger.Info(builder.ToString());
        }

        private void AssembleLogStringBuilder<T>(StringBuilder builder, string paramName, List<T> objList)
        {
            if (objList != null)
            {
                AssembleLogStringBuilder(builder, paramName, objList as System.Collections.IList);
            }
        }

        private void AssembleLogStringBuilder<T>(StringBuilder builder, string paramName, T[] objList)
        {
            if (objList != null)
            {
                AssembleLogStringBuilder(builder, paramName, objList.ToList());
            }
        }

        private void AssembleLogStringBuilder(StringBuilder builder, string paramName, System.Collections.IList objList)
        {
            if (objList == null) return;
            builder.Append($"{paramName} : [");
            foreach (var v in objList)
            {
                builder.Append($"'{v}', ");
            }
            builder.Append($"]; ");

        }

        private string AddBeforeAndAfterSeparator(string source, string separator = "|")
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (!source.StartsWith(separator))
                {
                    source = separator + source;
                }
                if (!source.EndsWith(separator))
                {
                    source = source + separator;
                }
                return source;
            }
            return string.Empty;
        }

        private List<Guid> GetAllSubFolderIds(Record rec)
        {
            List<Guid> folderIds = new List<Guid>();
            if (rec.NodeType == (int)NodeLevel.Folder)
            {
                string curFolderDirPath = rec.DirPath + "/";
                Expression<Func<Record, bool>> lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.DirPath.StartsWith(curFolderDirPath) && s.NodeType == (int)NodeLevel.Folder;
                folderIds = GetFilterList(a => a.NodeId, lambda);

            }
            return folderIds;
        }

        public Record GetPhysicalRawDataById(Guid id)
        {
            return _repository.QueryAllAysnc(s => s.ScopeId == Guid.Empty && s.Id == id).Result.FirstOrDefault();
        }

        public Tuple<IEnumerable<Record>, string> QueryDueRecordsByPage(SearchFilterParam param)
        {
            if (param == null)
            {
                throw new ArgumentNullException("SearchFilterParam is null");
            }
            //var sqlQuery = CosmosSqlQueryHelper.BuildSqlForGetDueRecords(param);
            //LogSqlQuerySpec(sqlQuery);

            int pageSize = param.PageInfo != null && param.PageInfo.PageSize > 0 ? param.PageInfo.PageSize : 100;

            string pageIndex = !string.IsNullOrWhiteSpace(param?.PageInfo?.PageIndex) ? param.PageInfo.PageIndex : "";

            //var result = _repository.QueryPageBySqlAsync(From(sqlQuery), pageSize, pageIndex).Result;

            var sqlQuery = GetSqlQuerySpec(BuildQueryDueRecordsQueryDto(param), null);
            logSqlParam(sqlQuery.Parameters);
            var result = _repository.QueryPageBySqlAsync(From(sqlQuery), pageSize, pageIndex).Result;
            return result.ToTuple();
        }

        //private ExplorerQueryV3Dto BuildQueryDueRecordsQueryV3Dto(SearchFilterParam param)
        //{
        //    //ExplorerQueryOptionV3
        //    ExplorerQueryV3Dto explorerQueryV3Dto = new ExplorerQueryV3Dto()
        //    { };
        //    return explorerQueryV3Dto;
        //}

        //private ExplorerQueryOptionV3 BuildQueryDueRecordsQueryOptionV3(SearchFilterParam param)
        //{
        //    ExplorerQueryOptionV3 explorerQueryOptionV3 = new ExplorerQueryOptionV3();
        //    explorerQueryOptionV3.Values = new List<ExplorerSearchOptionV3>();
        //    DisposalDateInfo
        //    explorerQueryOptionV3.Values.Add(new ExplorerSearchOptionV3()
        //    {
        //        Value = JsonConvert.SerializeObject(),
        //        ColumnsLogic = ExplorerSearchKeyOperationLogic.OR,
        //        Column = new ExplorerQueryColumn { Id = QueryCloumnIds.DisposalDueDate },
        //    });
        //    return explorerQueryOptionV3;
        //}

        private ExplorerQueryV2Dto BuildQueryDueRecordsQueryDto(SearchFilterParam param)
        {
            ExplorerQueryV2Dto queryDto = new ExplorerQueryV2Dto()
            {
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = AssembleFilterOptionForDueRecords(param),
                }
            };

            return queryDto;
        }

        private ExplorerFilterOptionV2 AssembleFilterOptionForDueRecords(SearchFilterParam param)
        {
            ExplorerFilterOptionV2 explorerFilterOptionV2 = new ExplorerFilterOptionV2();
            explorerFilterOptionV2.DisposalDateInfo = new DateInfo()
            {
                TimeZoneId = "UTC",
                Value1 = (new DateTime(param.DueDate, DateTimeKind.Utc)).ToString(),
                Condition = DateCondition.NextJobOrOverDue
            };
            explorerFilterOptionV2.ExceptRuleIds = new List<Guid>() { Guid.Empty };
            if (param.DataSource != default(int))
            {
                explorerFilterOptionV2.SourceFlags = new List<SourceFlag>() { (SourceFlag)param.DataSource };
            }

            if (!string.IsNullOrWhiteSpace(param.ScopeId))
            {
                explorerFilterOptionV2.ScopeId = param.ScopeId;
            }

            if (param.SkipHold)
            {
                explorerFilterOptionV2.HoldStatus = false;
            }

            //var status = param?.Filter?.RecordStatus;
            //if (status != null && status.Count > 0)
            //{
            //    explorerFilterOptionV2.Status = status.Select(s => (RMRecordStatus)s).ToList();
            //}
            //else
            //{
            //    explorerFilterOptionV2.Status = new List<RMRecordStatus>() { RMRecordStatus.Active };
            //}

            var dataLevel = param?.Filter?.NodeTypes;
            if (dataLevel != null && dataLevel.Count > 0)
            {
                explorerFilterOptionV2.NodeTypes = dataLevel.Select(t => (RMNodeLevel)t).ToList();
            }

            var searchScope = param?.Filter?.SearchScope;
            if (!string.IsNullOrWhiteSpace(searchScope) && param.DataSource != default(int))
            {
                switch (param.DataSource)
                {
                    case (int)SourceFlag.FileSystem:
                        explorerFilterOptionV2.DirPath = searchScope;
                        break;
                    default:
                        break;
                }
            }
            return explorerFilterOptionV2;
        }
        #endregion

        #region Box
        public Record GetBoxRecordById(Guid id)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.Box && s.Id == id && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
        }
        #endregion

        public Record GetPhysicalRecordByBarcode(string barcode)
        {
            return QueryAll(r => r.CustomColumnDic[DefaultColumnIDs.Barcode].Value.Equals(barcode, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        // temp, todo -> will change when already defined an id field for google
        public void DeleteGoogleItem(int createDate, string itemId)
        {
            var recordToDelete = GetFirstOrDefault(r => r.RecordsId.Equals(itemId));
            if (recordToDelete == null)
            {
                return;
            }
            Delete(recordToDelete.CreateDate, recordToDelete.Id);
        }

        public async Task<List<Record>> GetGoogleRecordsByFolderIdAsync(Guid scopeId, List<Guid> folderIds)
        {
            var files = await _repository.QueryAllAysnc(
               r => r.ScopeId == scopeId &&
                    r.SourceFlag == (int)SourceFlag.Google &&
                    r.RecordStatus != (int)RMRecordStatus.RMDeleted &&
                    r.RecordStatus != (int)RMRecordStatus.Destroyed &&
                    folderIds.Contains(r.ParentId)
            );

            return files.ToList();
        }

        public async Task<List<Record>> GetAllGoogleFilesByBatchBFSAsync(Guid scopeId, Guid rootFolderId)
        {
            List<Record> allFiles = new();
            List<Guid> currentLevelFolderIds = new() { rootFolderId };

            while (currentLevelFolderIds.Any())
            {
                var records = await GetGoogleRecordsByFolderIdAsync(scopeId, currentLevelFolderIds);

                var files = records.Where(r => r.NodeType == (int)RMNodeLevel.GoogleFile).ToList();
                var subFolders = records.Where(r => r.NodeType == (int)RMNodeLevel.GoogleFolder).ToList();

                allFiles.AddRange(files);

                currentLevelFolderIds = subFolders.Select(f => f.Id).ToList();
            }

            return allFiles;
        }

        public List<(string GControlCurrentApproverId, int Count)> QueryReviewerWaitingApprovalItemCountForGControl(string sql)
        {
            var res = new List<(string GControlCurrentApproverId, int Count)>();

            var queryResult = _repository.QueryAllBySqlAsync<dynamic>(new QueryDefinition(sql)).Result;

            foreach (dynamic result in queryResult)
            {
                var reviewersJson = JsonConvert.SerializeObject(result["reviewers"]);
                var reviewers = JsonConvert.DeserializeObject<string>(reviewersJson);
                var count = Convert.ToInt32(result["count"]);
                res.Add((reviewers, count));
            }

            return res;
        }
        public List<Record> GetChildRecordsByBoxIds(List<Guid> boxIds)
        {
            return QueryAll(s => s.SourceFlag == (int)SourceFlag.Physical && s.NodeType == (int)RMNodeLevel.PhysicalFile && boxIds.Contains(s.ParentId)).ToList();
        }

        #region support JPMC FS features
        /// <summary>
        /// Query FileSystem records by Ids for JPMC connection
        /// </summary>
        public List<Record> QueryJPMCRecords(int sourceFlag, string aveSiteId, List<Guid> ids)
        {
            return QueryAll(r => r.L1PartitionKey == $"{sourceFlag}" && r.L2PartitionKey == aveSiteId && ids.Contains(r.Id)).ToList();
        }

        /// <summary>
        /// Query FileSystem records by recordIds for JPMC connection
        /// </summary>
        public List<Record> QueryJPMCRecords(int sourceFlag, string aveSiteId, List<string> recordIds)
        {
            return QueryAll(r => r.L1PartitionKey == $"{sourceFlag}" && r.L2PartitionKey == aveSiteId && recordIds.Contains(r.RecordsId)).ToList();
        }

        public List<Record> QueryJPMCRecords(int sourceFlag, List<string> recordIds)
        {
            return QueryAll(r => r.L1PartitionKey == $"{sourceFlag}" && recordIds.Contains(r.RecordsId)).ToList();
        }

        public bool HasJPMCConnectionRecord(int sourceFlag, string aveSiteId)
        {
            return Exist(r => r.L1PartitionKey == $"{sourceFlag}" && r.L2PartitionKey == aveSiteId);
        }
        #endregion

        #region Maestro AI

        public int ResetMARecordsForRemovedMLTerms(List<Guid> predictTermIds)
        {
           return UpdateAll(r => predictTermIds.Contains(r.PredictTermId), r =>
            {
                r.PredictTermId = Guid.Empty;
                r.PredictTermScore = default;
                r.PredictTime = 0;
                r.TrainingModelId = Guid.Empty;
                r.MLApprovalStatus = (int)RMMLApprovalStatus.None;
                r.MLClassificationType = (int)RMMLClassificationType.None;
                r.MLUnderReview = (int)RMMLUnderReview.None;
                r.MLReviewer = [];
                r.MLEscalateFrom = 0;
                r.MLEscalatedComment = string.Empty;
            });
    }

        #endregion

        public async Task<Dictionary<string, int>> GetRecordCountByHoldIdAndHoldReleaseAsync(List<RMHold> holds)
        {
            if (holds == null || holds.Count == 0) return [];

            const int BatchSize = 100;

            var result = new Dictionary<string, int>(holds.Count);

            for (var i = 0; i < holds.Count; i += BatchSize)
            {
                var batch = holds.GetRange(i, Math.Min(BatchSize, holds.Count - i));

                foreach (var item in await QueryRecordCountBatchAsync(batch))
                {
                    result[item.Key] = item.Value;
                }
            }

            return result;
        }

        private async Task<Dictionary<string, int>> QueryRecordCountBatchAsync(List<RMHold> holds)
        {
            var whereClauses = new List<string>(holds.Count);

            var query = new StringBuilder();
            query.AppendLine("SELECT");
            query.AppendLine("    c.holdId,");
            query.AppendLine("    COUNT(1) AS recordCount");
            query.AppendLine("FROM c");
            query.Append("WHERE ");

            for (var i = 0; i < holds.Count; i++)
            {
                whereClauses.Add($"(c.holdId = @holdId{i} AND c.holdReleaseTime = @holdReleaseTime{i})");
            }

            query.Append(string.Join(" OR ", whereClauses));
            query.AppendLine();
            query.Append("GROUP BY c.holdId");

            var queryDefinition = new QueryDefinition(query.ToString());

            for (var i = 0; i < holds.Count; i++)
            {
                queryDefinition
                    .WithParameter($"@holdId{i}", holds[i].Id)
                    .WithParameter($"@holdReleaseTime{i}", holds[i].CalendarTime);
            }

            var rows = await _repository.QueryAllBySqlAsync<dynamic>(queryDefinition);

            var result = new Dictionary<string, int>();

            foreach (dynamic row in rows)
            {
                result[(string)row.holdId] = Convert.ToInt32(row.recordCount);
            }

            return result;
        }
    }
}
