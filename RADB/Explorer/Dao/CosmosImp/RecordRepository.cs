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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    //public class RecordRepository : IDisposable
    //{
    //    private CommonUtil.RALogger logger = CommonUtil.RALogger.GetInstance(typeof(RecordRepository));
    //    private readonly static object locker = new object();
    //    private DocumentClient Client
    //    {
    //        get
    //        {
    //            if (_client == null)
    //            {
    //                lock (locker)
    //                {
    //                    if (_client == null)
    //                    {
    //                        _client = new DocumentClient(new Uri(CosmosConnectionInfo.Endpoint),
    //                        CosmosConnectionInfo.Key,
    //                        new ConnectionPolicy
    //                        {
    //                            ConnectionMode = CosmosConnectionInfo.ConnectionMode,
    //                            ConnectionProtocol = CosmosConnectionInfo.Protocol,
    //                        });
    //                        _client.OpenAsync().Wait();
    //                    }
    //                }
    //            }
    //            return _client;
    //        }
    //    }
    //    public CosmosConnectionInfo CosmosConnectionInfo { get; set; }

    //    private static DocumentClient _client;
    //    public RecordRepository(CosmosConnectionInfo connectionInfo)
    //    {
    //        CodeContract.NullThrowing(connectionInfo, "CosmosConnectionInfo");
    //        CodeContract.NullOrEmptyStringThrowing(connectionInfo.Endpoint, "connectionInfo.Endpoint");
    //        CodeContract.NullOrEmptyStringThrowing(connectionInfo.DatabaseId, "connectionInfo.DatabaseId");
    //        CodeContract.NullOrEmptyStringThrowing(connectionInfo.Key, "connectionInfo.Key");
    //        CodeContract.NullOrEmptyStringThrowing(connectionInfo.CollectionId, "connectionInfo.CollectionId");
    //        CosmosConnectionInfo = connectionInfo;

    //    }
    //    /// <summary>
    //    /// database level操作使用
    //    /// </summary>
    //    /// <param name="specifiedDatabaseId"></param>
    //    public RecordRepository(string specifiedDatabaseId = "")
    //    {
    //        var connection = new CosmosConnectionInfo()
    //        {
    //            Key = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.CosmosDBSecret],
    //            Endpoint = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.CosmosDBURI],
    //            DatabaseId = specifiedDatabaseId,
    //        };
    //        CosmosConnectionInfo = connection;

    //    }

    //    //public void Setup()
    //    //{
    //    //    //RECO-3992
    //    //    //多线程，或者多进程 同时调用CreateCollectionIfNotExists 方法时候，如果collection 不存在，则可能出现部分线程/进程中的方法异常，
    //    //    //为了防止方法异常，添加retry 机制，防止由于并发引起不必要的问题
    //    //    int retry = 5;
    //    //    while (retry > 0)
    //    //    {
    //    //        retry--;
    //    //        try
    //    //        {
    //    //            CreateCollectionIfNotExists();
    //    //            break;
    //    //        }
    //    //        catch
    //    //        {
    //    //            if (retry <= 0)
    //    //            {
    //    //                throw;
    //    //            }
    //    //        }
    //    //    }
    //    //}


    //    public void Add(Record record)
    //    {
    //        record.AppendCustomColumns();
    //        var result = Client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId), record).Result;
    //    }
    //    [Obsolete]
    //    public void Delete(Guid scopeId, Guid id)
    //    {
    //        RequestOptions options = new RequestOptions()
    //        {
    //            PartitionKey = new PartitionKey(scopeId.ToString()),
    //        };

    //        var result = Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId, id.ToString()), options).Result;
    //    }

    //    public void Delete(int createDate, Guid id)
    //    {
    //        RequestOptions options = new RequestOptions()
    //        {
    //            PartitionKey = new PartitionKey(createDate),
    //        };

    //        var result = Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId, id.ToString()), options).Result;
    //    }
    //    public Record ReadById(Guid scopeId, Guid id)
    //    {
    //        try
    //        {
    //            //RequestOptions options = new RequestOptions()
    //            //{
    //            //    PartitionKey = new PartitionKey(scopeId.ToString()),
    //            //};
    //            //var result = Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId, id.ToString()), options).Result;
    //            //var result = Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId, id.ToString())).Result;
    //            //return (Record)(dynamic)result.Resource;
    //            return this.GetFirstOrDefault(a => a.Id == id && a.ScopeId == scopeId);

    //        }
    //        catch (AggregateException e)
    //        {
    //            foreach (var ex in e.InnerExceptions)
    //            {
    //                if ((ex as DocumentClientException)?.StatusCode == System.Net.HttpStatusCode.NotFound)
    //                {
    //                    return null;
    //                }
    //            }
    //            throw;
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }
    //    }

    //    public void Replace(Record record)
    //    {
    //        //RequestOptions options = new RequestOptions()
    //        //{
    //        //    PartitionKey = new PartitionKey(record.CreateDate),
    //        //};
    //        //var doc = Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId, record.Id.ToString()), record, options).Result;
    //        var doc = Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId, record.Id.ToString()), record).Result;
    //    }

    //    public void Upsert(Record record)
    //    {
    //        record.AppendCustomColumns();
    //        var doc = Client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId), record).Result;
    //    }
    //    public int UpdateAll(Expression<Func<Record, bool>> predicate, Action<Record> operation)
    //    {
    //        int count = 0;
    //        IDocumentQuery<Record> query = Client.CreateDocumentQuery<Record>(
    //             UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId),
    //             new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
    //             .Where(predicate)
    //             .AsDocumentQuery();
    //        while (query.HasMoreResults)
    //        {
    //            List<Record> results = new List<Record>();
    //            results.AddRange(query.ExecuteNextAsync<Record>().Result);
    //            results.ForEach(r =>
    //            {
    //                operation(r);
    //                r.AppendCustomColumns();
    //                Replace(r);
    //                count++;
    //            });
    //        }
    //        return count;
    //    }

    //    /// <summary>
    //    /// 取出满足条件的第一个记录
    //    /// </summary>
    //    /// <param name="whereLambda"></param>
    //    /// <returns></returns>
    //    public Record GetFirstOrDefault(Expression<Func<Record, bool>> whereLambda)
    //    {
    //        var query = Client.CreateDocumentQuery<Record>(
    //             GetCollectionUri(),
    //             new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
    //             .Where(whereLambda)
    //            .Take(1).AsEnumerable().FirstOrDefault();

    //        return query;
    //    }

    //    /// <summary>
    //    /// 判断是否存在满足条件的记录
    //    /// </summary>
    //    /// <param name="whereLambda"></param>
    //    /// <returns></returns>
    //    public bool Exist(Expression<Func<Record, bool>> whereLambda)
    //    {
    //        var query = Client.CreateDocumentQuery<Record>(
    //             GetCollectionUri(),
    //             new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
    //             .Where(whereLambda)
    //             .Select(c => c.Id)
    //            .Take(1).AsEnumerable();

    //        return query.Count() == 1;
    //    }

    //    public List<T> GetFilterList<T>(Expression<Func<Record, T>> selectLambda, Expression<Func<Record, bool>> whereLambda)
    //    {
    //        List<T> results = new List<T>();
    //        IDocumentQuery<T> query;
    //        if (selectLambda == null)
    //        {
    //            return results;
    //        }
    //        if (whereLambda != null)
    //        {
    //            query = Client.CreateDocumentQuery<Record>(
    //            UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId),
    //            new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
    //            .Where(whereLambda)
    //            .Select(selectLambda)
    //            .AsDocumentQuery();
    //        }
    //        else
    //        {
    //            query = Client.CreateDocumentQuery<Record>(
    //            UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId),
    //            new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
    //            .Select(selectLambda)
    //            .AsDocumentQuery();
    //        }
    //        while (query.HasMoreResults)
    //        {
    //            results.AddRange(query.ExecuteNextAsync<T>().Result);
    //        }
    //        return results;
    //    }

    //    public IEnumerable<Record> QueryAll(Expression<Func<Record, bool>> predicate)
    //    {
    //        IDocumentQuery<Record> query = Client.CreateDocumentQuery<Record>(
    //             UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId),
    //             new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
    //             .Where(predicate)
    //             .AsDocumentQuery();
    //        List<Record> results = new List<Record>();
    //        while (query.HasMoreResults)
    //        {
    //            results.AddRange(query.ExecuteNextAsync<Record>().Result);
    //        }
    //        return results;
    //    }

    //    public IEnumerable<Record> QueryAllByDescending(Expression<Func<Record, bool>> predicate)
    //    {
    //        IDocumentQuery<Record> query = Client.CreateDocumentQuery<Record>(
    //             UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId),
    //             new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 })
    //             .Where(predicate)
    //             .OrderByDescending(d => d.CollectTime)
    //             .AsDocumentQuery();
    //        List<Record> results = new List<Record>();
    //        while (query.HasMoreResults)
    //        {
    //            results.AddRange(query.ExecuteNextAsync<Record>().Result);
    //        }
    //        return results;
    //    }
    //    /// <summary>
    //    /// Query count
    //    /// </summary>
    //    /// <param name="sql">SELECT VALUE COUNT(1) FROM c where c.xxx</param>
    //    /// <returns></returns>
    //    public int QueryCount(string sql)
    //    {
    //        IQueryable<dynamic> familyQueryInSql = Client.CreateDocumentQuery<dynamic>(
    //        UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId),
    //        sql,
    //        new FeedOptions { MaxItemCount = -1 });
    //        foreach (dynamic family in familyQueryInSql)
    //        {
    //            return (int)family;
    //        }
    //        return 0;
    //    }


    //    public Dictionary<string, int> QueryRelatedTermCount(string sql)
    //    {
    //        Dictionary<string, int> termIdAndRelatedCount = new Dictionary<string, int>();
    //        IQueryable<dynamic> familyQueryInSql = Client.CreateDocumentQuery<dynamic>(
    //        UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId),
    //        sql,
    //        new FeedOptions { MaxItemCount = -1 });
    //        foreach (dynamic family in familyQueryInSql)
    //        {
    //            var dic = (IDictionary<string, object>)family;
    //            string termId = dic["termId"].ToString();
    //            int termCount = Convert.ToInt32(dic["termcount"]);
    //            if (termIdAndRelatedCount.ContainsKey(termId))
    //            {
    //                termIdAndRelatedCount[termId] = termCount;
    //            }
    //            else
    //            {
    //                termIdAndRelatedCount.Add(termId, termCount);
    //            }
    //        }
    //        return termIdAndRelatedCount;
    //    }

    //    /// <summary>
    //    /// Will log the query metrics if CommonRoleConfiguration.LogCosmosQueryMetrics is true
    //    /// </summary>
    //    /// <param name="queryMetrics"></param>
    //    private void LogQueryMetrics(IReadOnlyDictionary<string, QueryMetrics> queryMetrics)
    //    {
    //        if (queryMetrics == null) return;

    //        var enableLog = false;
    //        if (!bool.TryParse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.LogCosmosQueryMetrics], out enableLog)) return;
    //        if (!enableLog) return;

    //        foreach (var metrics in queryMetrics.Values)
    //        {
    //            logger.Warn(metrics.ToString());
    //        }
    //    }
    //    public List<Record> QueryRecordsByPermission(SqlQuerySpec sqlQuerySpec)
    //    {
    //        Exception ex = null;
    //        var maxRetryTimes = 3;
    //        while (maxRetryTimes > 0)
    //        {
    //            try
    //            {
    //                var option = new FeedOptions { EnableCrossPartitionQuery = true };
    //                //var uri = UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId);
    //                var query = Client.CreateDocumentQuery<Record>(GetCollectionUri(), sqlQuerySpec, option)
    //                  .AsDocumentQuery();

    //                var result = query.ExecuteNextAsync<Record>().Result;

    //                LogQueryMetrics(result.QueryMetrics);

    //                var records = new List<Record>(result);
    //                return records;
    //            }
    //            catch (Exception aex)
    //            {
    //                ///will retry if error of too large rate with statuscode 429 occurs
    //                Exception baseException = aex.GetBaseException();
    //                if (!(baseException is DocumentClientException)) throw;

    //                DocumentClientException de = (DocumentClientException)baseException;
    //                if (--maxRetryTimes > 0 && Convert.ToInt32(de.StatusCode) == 429)
    //                {
    //                    logger.Warn("An error with status code 429 occurs while retrieve data, will retry in 200 ms");
    //                    Thread.Sleep(200);
    //                }
    //                else
    //                {
    //                    ex = aex;
    //                    break;
    //                }
    //            }
    //        }

    //        throw ex;
    //    }

    //    public Tuple<IEnumerable<Record>, string> QueryPageBySql(SqlQuerySpec sqlQuerySpec, int countPerPage = 15, string continuation = "")
    //    {
    //        Exception ex = null;
    //        var maxRetryTimes = 3;
    //        while (maxRetryTimes > 0)
    //        {
    //            try
    //            {
    //                var option = new FeedOptions { MaxItemCount = countPerPage, EnableCrossPartitionQuery = true, PopulateQueryMetrics = true, MaxDegreeOfParallelism = -1 };
    //                if (!string.IsNullOrEmpty(continuation))
    //                {
    //                    option.RequestContinuation = continuation;
    //                }

    //                //var uri = UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId);
    //                var query = Client.CreateDocumentQuery<Record>(GetCollectionUri(), sqlQuerySpec, option)
    //                  .AsDocumentQuery();

    //                var result = query.ExecuteNextAsync<Record>().Result;

    //                LogQueryMetrics(result.QueryMetrics);

    //                var records = new List<Record>(result);
    //                return new Tuple<IEnumerable<Record>, string>(records, result.ResponseContinuation);
    //            }
    //            catch (Exception aex)
    //            {
    //                ///will retry if error of too large rate with statuscode 429 occurs
    //                Exception baseException = aex.GetBaseException();
    //                if (!(baseException is DocumentClientException)) throw;

    //                DocumentClientException de = (DocumentClientException)baseException;
    //                if (--maxRetryTimes > 0 && Convert.ToInt32(de.StatusCode) == 429)
    //                {
    //                    logger.Warn("An error with status code 429 occurs while retrieve data, will retry in 200 ms");
    //                    Thread.Sleep(200);
    //                }
    //                else
    //                {
    //                    ex = aex;
    //                    break;
    //                }
    //            }
    //        }

    //        throw ex;
    //    }


    //    public Tuple<IEnumerable<TOut>, string> QueryByPage<TSource, TOut, TOrder>(Expression<Func<TSource, bool>> predicate, Expression<Func<TSource, TOut>> selector, Expression<Func<TSource, TOrder>> orderByLambda, bool orderAscending = true, int countPerPage = 15, string continuation = "")
    //    {
    //        var option = new FeedOptions { MaxItemCount = countPerPage, EnableCrossPartitionQuery = true, /*PopulateQueryMetrics = true,*/ MaxDegreeOfParallelism = -1 };
    //        if (!string.IsNullOrEmpty(continuation))
    //        {
    //            option.RequestContinuation = continuation;
    //        }

    //        var rec = Client.CreateDocumentQuery<TSource>(GetCollectionUri(), option)
    //          .Where(predicate);

    //        rec = orderAscending == true ? rec.OrderBy(orderByLambda) : rec.OrderByDescending(orderByLambda);

    //        var query = rec.Select(selector).AsDocumentQuery();

    //        var result = query.ExecuteNextAsync<TOut>().Result;

    //        LogQueryMetrics(result.QueryMetrics);

    //        var records = new List<TOut>(result);
    //        return new Tuple<IEnumerable<TOut>, string>(records, result.ResponseContinuation);
    //    }

    //    public Tuple<IEnumerable<Record>, string> QueryByPage(Expression<Func<Record, bool>> predicate, int countPerPage = 15, string continuation = "")
    //    {
    //        Exception ex = null;
    //        var maxRetryTimes = 3;
    //        while (maxRetryTimes > 0)
    //        {
    //            try
    //            {
    //                var option = new FeedOptions { RequestContinuation = continuation, MaxItemCount = countPerPage, EnableCrossPartitionQuery = true, /*PopulateQueryMetrics = true,*/ MaxDegreeOfParallelism = -1 };
    //                if (string.IsNullOrEmpty(continuation))
    //                {
    //                    option = new FeedOptions { MaxItemCount = countPerPage, EnableCrossPartitionQuery = true, /*PopulateQueryMetrics = true,*/ MaxDegreeOfParallelism = -1 };
    //                }
    //                IDocumentQuery<Record> query = null;
    //                var uri = UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId);
    //                if (predicate == null)
    //                {

    //                    query = Client.CreateDocumentQuery<Record>(uri, option)
    //                   .OrderByDescending(d => d.CollectTime)
    //                   .AsDocumentQuery();
    //                }
    //                else
    //                {
    //                    query = Client.CreateDocumentQuery<Record>(uri, option)
    //                  .Where(predicate)
    //                  .OrderByDescending(d => d.CollectTime)
    //                  .AsDocumentQuery();
    //                }

    //                var result = query.ExecuteNextAsync<Record>().Result;

    //                //LogQueryMetrics(result.QueryMetrics);

    //                var records = new List<Record>(result);
    //                return new Tuple<IEnumerable<Record>, string>(records, result.ResponseContinuation);
    //            }
    //            catch (Exception aex)
    //            {
    //                ///will retry if error of too large rate with statuscode 429 occurs
    //                Exception baseException = aex.GetBaseException();
    //                if (!(baseException is DocumentClientException)) throw;

    //                DocumentClientException de = (DocumentClientException)baseException;
    //                if (--maxRetryTimes > 0 && Convert.ToInt32(de.StatusCode) == 429)
    //                {
    //                    logger.Warn("An error with status code 429 occurs while retrieve data, will retry in 200 ms");
    //                    Thread.Sleep(200);
    //                }
    //                else
    //                {
    //                    ex = aex;
    //                    break;
    //                }
    //            }
    //        }

    //        throw ex;
    //    }

    //    private Uri GetCollectionUri()
    //    {
    //        return UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId);
    //    }

    //    public ResourceResponse<Database> CreateDatabaseIfNotExists(string dbName)
    //    {
    //        try
    //        {

    //            return Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(dbName)).Result;
    //        }
    //        catch
    //        {
    //            try
    //            {
    //                return Client.CreateDatabaseIfNotExistsAsync(
    //                   new Database { Id = dbName },
    //                   new RequestOptions
    //                   {
    //                       OfferThroughput = CosmosConnectionInfo.ThroughputType != ThroughputType.Dedicated
    //                       ? CosmosConnectionInfo.Throughput
    //                       : default(int?)
    //                   }
    //                   ).Result;
    //            }
    //            catch
    //            {
    //                throw;
    //            }
    //        }
    //    }
    //    public bool CreateCollectionIfNotExists()
    //    {
    //        bool result = false;
    //        try
    //        {
    //            var doc = Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId)
    //                , new RequestOptions
    //                {

    //                    OfferThroughput = CosmosConnectionInfo.ThroughputType != ThroughputType.Shared
    //                     ? CosmosConnectionInfo.Throughput
    //                     : default(int?)
    //                }
    //                ).Result;
    //        }
    //        catch
    //        {
    //            result = CreateCollection();
    //        }
    //        return result;
    //    }

    //    private bool CreateCollection()
    //    {
    //        bool result = false;
    //        try
    //        {
    //            logger.Info($"begin to create collection:{CosmosConnectionInfo?.CollectionId}");
    //            DocumentCollection collection = new DocumentCollection()
    //            {
    //                Id = CosmosConnectionInfo.CollectionId,
    //            };
    //            collection.IndexingPolicy.Automatic = true;
    //            collection.IndexingPolicy.IndexingMode = IndexingMode.Consistent;  //Change from lazy to consistent in May release.
    //            collection.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });
    //            collection.IndexingPolicy.IncludedPaths = new Collection<IncludedPath>() {
    //            new IncludedPath()
    //            {
    //                Path = "/*",
    //                Indexes = new Collection<Index>() {
    //                    new RangeIndex(DataType.Number) { Precision = -1},
    //                    new RangeIndex(DataType.String) { Precision = -1 }
    //                }
    //            }
    //        };

    //            collection.IndexingPolicy.ExcludedPaths = new Collection<ExcludedPath>()
    //        {
    //            new ExcludedPath() { Path = "/recordHistory/*" },
    //            new ExcludedPath() { Path = "/metaInfo/*" },
    //            new ExcludedPath() { Path = "/relatedRecordsCount/*" },
    //            new ExcludedPath() { Path = "/relatedRecords/*" },
    //            new ExcludedPath() { Path = "/extsion1/*" },
    //        };

    //            collection.PartitionKey = new PartitionKeyDefinition { Paths = new System.Collections.ObjectModel.Collection<string> { "/createDate" } };


    //            RequestOptions options = null;
    //            if (CosmosConnectionInfo.ThroughputType == ThroughputType.Dedicated)
    //            {
    //                options = new RequestOptions() { OfferThroughput = 400 };
    //            }

    //            var connection = Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(CosmosConnectionInfo.DatabaseId), collection, options).Result;
    //            result = true;
    //            logger.Info($"success to create collection:{CosmosConnectionInfo?.CollectionId}");
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Error($"error to create collection:{CosmosConnectionInfo?.DatabaseId}, {CosmosConnectionInfo?.CollectionId}, ERROR:{ex.ToString()}");
    //            throw;
    //        }
    //        return result;
    //    }

    //    public void DeleteConnection(string connectionId)
    //    {
    //        Client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, connectionId));
    //    }

    //    /// <summary>
    //    /// Check the index transformation progress using a ReadDocumentCollectionAsync. 
    //    /// The service returns a 0-100 value based on the progress.
    //    /// </summary>
    //    /// <param name="collection">the collection to monitor progress</param>
    //    public async Task WaitForIndexTransformationToComplete()
    //    {
    //        long smallWaitTimeMilliseconds = 1000;
    //        long progress = 0;

    //        while (progress >= 0 && progress < 100)
    //        {
    //            ResourceResponse<DocumentCollection> collectionReadResponse = await Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId));
    //            progress = collectionReadResponse.IndexTransformationProgress;

    //            //Console.WriteLine("Waiting...");
    //            await Task.Delay(TimeSpan.FromMilliseconds(smallWaitTimeMilliseconds));
    //        }

    //        //Console.WriteLine("Done!");
    //    }

    //    public bool CheckHasData()
    //    {
    //        try
    //        {
    //            //跳过判断是否存在逻辑直接查询
    //            //var option1 = new RequestOptions { OfferThroughput = CosmosConnectionInfo.ThroughputType != ThroughputType.Shared ? CosmosConnectionInfo.Throughput : default(int?) };
    //            //var result = Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId), option1).Result;
    //            try
    //            {
    //                var option2 = new FeedOptions { RequestContinuation = "", MaxItemCount = 1, EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 };
    //                IDocumentQuery<Record> query = null;
    //                query = Client.CreateDocumentQuery<Record>(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, CosmosConnectionInfo.CollectionId), option2)
    //               .AsDocumentQuery();
    //                var queryOneResult = query.ExecuteNextAsync<Record>().Result;
    //                return queryOneResult.Count > 0;
    //            }
    //            catch (Exception)
    //            {
    //                throw;
    //            }
    //        }
    //        catch
    //        {
    //            return false;
    //        }

    //    }

    //    public List<string> GetContainersInDB(string dbName)
    //    {

    //        Database database = Client.CreateDatabaseQuery($"SELECT * FROM d WHERE d.id = \"{dbName}\"").AsEnumerable().First();

    //        return Client.CreateDocumentCollectionQuery((String)database.SelfLink).Select(m => m.Id).ToList();
    //    }

    //    public bool ConnectionExist(string connectionId)
    //    {
    //        try
    //        {
    //            logger.Info($"connection Info:{CosmosConnectionInfo.DatabaseId}, {CosmosConnectionInfo.CollectionId}");
    //            var cnn = Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(CosmosConnectionInfo.DatabaseId, connectionId)
    //              , new RequestOptions
    //              {
    //                  OfferThroughput = CosmosConnectionInfo.ThroughputType != ThroughputType.Shared
    //                   ? CosmosConnectionInfo.Throughput
    //                   : default(int?)
    //              }
    //              ).Result;
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Warn($"connection not exist:{ex.ToString()}");
    //            return false;
    //        }
    //        return true;
    //    }

    //    #region IDisposable Support
    //    private bool disposedValue = false; // To detect redundant calls

    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!disposedValue)
    //        {
    //            if (disposing)
    //            {
    //                try
    //                {
    //                    Client?.Dispose();
    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.Warn("dispose error {0}", ex.ToString());
    //                }
    //            }

    //            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
    //            // TODO: set large fields to null.

    //            disposedValue = true;
    //        }
    //    }

    //    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    //    // ~RecordRepository() {
    //    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //    //   Dispose(false);
    //    // }

    //    // This code added to correctly implement the disposable pattern.
    //    public void Dispose()
    //    {
    //        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //        //Dispose(true);
    //        // TODO: uncomment the following line if the finalizer is overridden above.
    //        // GC.SuppressFinalize(this);
    //    }
    //    #endregion
    //}
}
