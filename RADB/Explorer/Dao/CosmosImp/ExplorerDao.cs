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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AvePoint.RA.Contract.Services;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{

    //public class ExplorerDao : IExplorerDao, IDisposable
    //{
    //    RecordRepository _repository = null;
    //    private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));

    //    public ExplorerDao(bool createConnectionIfNotExist = false) : this(RMDBContextManager.GetCosmosDBConnection(), createConnectionIfNotExist)
    //    {
    //    }
    //    private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExplorerDao));

    //    public ExplorerDao(CosmosConnectionInfo connectionInfo, bool createConnectionIfNotExist = false)
    //    {
    //        _repository = new RecordRepository(connectionInfo);
    //        if (createConnectionIfNotExist)
    //        {
    //            CreateExplorerContainerIfNotExist(connectionInfo);
    //        }
    //    }

    //    private void CreateExplorerContainerIfNotExist(CosmosConnectionInfo connectionInfo)
    //    {
    //        try
    //        {
    //            if (_repository.CreateCollectionIfNotExists())
    //            {
    //                IDBInfoDao dao = new DB.Dao.Impl.DBInfoDao();
    //                dao.AddExplorerDBMappingInfo(new Contract.Tenant.RMDBInfoDto() { DBName = connectionInfo.DatabaseId, ContainerName = connectionInfo.CollectionId });
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Error($"Error occurred while create explorer containers, {connectionInfo?.DatabaseId}, {connectionInfo?.CollectionId},ERROR:{ex.ToString()}");
    //            throw;
    //        }

    //    }
    //    //public ExplorerDao(bool isInit)
    //    //{
    //    //    _repository = new RecordRepository(RMDBContextManager.GetCosmosDBConnection());
    //    //}

    //    private IRMRuleDao mRMRuleDao;
    //    protected IRMRuleDao RMRuleDao
    //    {
    //        get
    //        {
    //            if (mRMRuleDao == null)
    //            {
    //                mRMRuleDao = (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao));
    //            }
    //            return mRMRuleDao;
    //        }
    //    }

    //    /// <summary>
    //    /// todo try batch lib
    //    /// </summary>
    //    /// <param name="records"></param>
    //    public void BatchAddRecords(List<Record> records, bool forceUpdate = false)
    //    {
    //        foreach (var record in records)
    //        {
    //            AddOrUpdateRecord(record, forceUpdate);
    //        }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<Guid> UpdateExpiredHeldRecords()
    //    {
    //        var records = QueryAll(r => r.HoldStatus && r.HoldReleaseTime < DateTime.UtcNow.Ticks);
    //        List<Guid> ids = records.Select(a => a.Id).ToList();
    //        foreach (var record in records)
    //        {
    //            record.HoldStatus = false;
    //            record.HoldReleaseTime = 0;
    //            record.HoldBy = string.Empty;
    //            record.HoldId = string.Empty;
    //            Replace(record);
    //        }
    //        return ids;
    //    }

    //    public List<Record> GetRecordsByTerms(Guid scopeId, List<Guid> termIds, long ticks)
    //    {
    //        return QueryAll(m => termIds.Contains(m.TermId) && m.ScopeId == scopeId && m.RecordStatus == (int)RMRecordStatus.Active && m.NodeType == 500 && m.CollectTime < ticks).ToList();
    //    }

    //    public List<Record> GetEXORecordsByTerms(Guid scopeId, List<Guid> termIds, long ticks, string emailAddress)
    //    {
    //        return QueryAll(m => termIds.Contains(m.TermId) && m.ScopeId == scopeId && m.RecordStatus == (int)RMRecordStatus.Active && m.NodeType == 5110 && m.CollectTime < ticks && m.EmailAddress == emailAddress).ToList();
    //    }

    //    public Record ReadById(Guid scopeId, Guid id)
    //    {
    //        Record rec = _repository.ReadById(scopeId, id);
    //        rec.AppendMetaInfoForOldLogic();
    //        return rec;
    //    }

    //    public void Upsert(Record record)
    //    {
    //        record.AppendCustomColumns();
    //        _repository.Upsert(record);
    //    }

    //    public void Replace(Record record)
    //    {
    //        record.AppendCustomColumns();
    //        _repository.Replace(record);
    //    }

    //    public void Add(Record record)
    //    {
    //        record.AppendCustomColumns();
    //        _repository.Add(record);
    //    }
    //    [Obsolete("not work from Sep release")]
    //    public void Delete(Guid scopeId, Guid id)
    //    {
    //        _repository.Delete(scopeId, id);
    //    }
    //    public void Delete(int createDate, Guid id)
    //    {
    //        _repository.Delete(createDate, id);
    //    }
    //    public int UpdateAll(Expression<Func<Record, bool>> predicate, Action<Record> operation)
    //    {
    //        return _repository.UpdateAll(predicate, operation);
    //    }

    //    public List<T> GetFilterList<T>(Expression<Func<Record, T>> selectLambda, Expression<Func<Record, bool>> whereLambda)
    //    {
    //        return _repository.GetFilterList(selectLambda, whereLambda);
    //    }

    //    public IEnumerable<Record> QueryAll(Expression<Func<Record, bool>> predicate, bool convertCustomColumn2Metainfo = true)
    //    {
    //        IEnumerable<Record> recs = _repository.QueryAll(predicate);
    //        if (convertCustomColumn2Metainfo)
    //        {
    //            foreach (Record rec in recs)
    //            {
    //                rec.AppendMetaInfoForOldLogic();
    //            }
    //        }
    //        return recs;
    //    }

    //    public IEnumerable<Record> QueryAllByDescending(Expression<Func<Record, bool>> predicate)
    //    { 
    //        IEnumerable<Record> recs = _repository.QueryAllByDescending(predicate);
    //        foreach (Record rec in recs)
    //        {
    //            rec.AppendMetaInfoForOldLogic();
    //        }
    //        return recs;
    //    }
    //    public int QueryCount(string sql)
    //    {
    //        return _repository.QueryCount(sql);
    //    }

    //    public Dictionary<string, int> QueryRelatedTermCount(string sql)
    //    {
    //        return _repository.QueryRelatedTermCount(sql);
    //    }
    //    public Record GetFirstOrDefault(Expression<Func<Record, bool>> whereLambda)
    //    {
    //        Record rec = _repository.GetFirstOrDefault(whereLambda);
    //        rec.AppendMetaInfoForOldLogic();
    //        return rec;
    //    }

    //    public bool Exist(Expression<Func<Record, bool>> whereLambda)
    //    {
    //        return _repository.Exist(whereLambda);
    //    }

    //    //public Tuple<IEnumerable<TOut>, string> QueryByPage<TOut, TOrder>(Expression<Func<Record, bool>> predicate, Expression<Func<Record, TOut>> selector, Expression<Func<Record, TOrder>> orderByLambda, bool orderAscending = true, int pageCount = 15, string continuation = "")
    //    //{
    //    //    return _repository.QueryByPage<Record, TOut, TOrder>(predicate, selector, orderByLambda, orderAscending, pageCount, continuation);
    //    //}

    //    public Tuple<IEnumerable<Record>, string> QueryByPage(Expression<Func<Record, bool>> predicate, int pageCount = 15, string continuation = "")
    //    {
    //        Tuple<IEnumerable<Record>, string> result = _repository.QueryByPage(predicate, pageCount, continuation);
    //        foreach(Record rec in result.Item1)
    //        {
    //            rec.AppendMetaInfoForOldLogic();
    //        }
    //        return result;
    //    }

    //    public Tuple<IEnumerable<Record>, string> QueryPageBySql(SqlQuerySpec sqlQuerySpec, int pageCount = 15, string continuation = "")
    //    {
    //        Tuple<IEnumerable<Record>, string> result = _repository.QueryPageBySql(sqlQuerySpec, pageCount, continuation);
    //        foreach (Record rec in result.Item1)
    //        {
    //            rec.AppendMetaInfoForOldLogic();
    //        }
    //        return result;
    //    }


    //    //public async void WaitForIndexTransformationToComplete()
    //    //{
    //    //    await _repository.WaitForIndexTransformationToComplete();
    //    //}


    //    private bool disposedValue = false;
    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!disposedValue)
    //        {
    //            if (disposing)
    //            {
    //                _repository?.Dispose();
    //            }
    //            disposedValue = true;
    //        }
    //    }
    //    public void Dispose()
    //    {
    //        Dispose(true);
    //    }

    //    public int QueryDataGetTotal(int status, string keyWord, Expression<Func<Record, bool>> whereLambda = null)
    //    {
    //        int cnt = 0;
    //        Expression<Func<Record, bool>> searchLambda = null;
    //        if (string.IsNullOrEmpty(keyWord))
    //        {
    //            searchLambda = m => m.LeafName.Contains(keyWord) || m.RecordsId.Contains(keyWord);
    //            cnt = QueryAll(whereLambda).AsQueryable().Where(searchLambda).Where(r => r.RecordStatus == status && r.NodeType == (int)NodeLevel.Item).Count();
    //        }
    //        else
    //        {
    //            cnt = QueryAll(whereLambda).AsQueryable().Where(r => r.RecordStatus == status && r.NodeType == (int)NodeLevel.Item).Count();
    //        }

    //        return cnt;
    //    }

    //    private List<SourceFlag> ReAssembleFourceFlags(SourceFlag sourceFlag)
    //    {
    //        return sourceFlag == SourceFlag.All ? new List<SourceFlag>()
    //            { SourceFlag.Exchange, SourceFlag.FileSystem, SourceFlag.SharePoint, SourceFlag.Physical }
    //        : new List<SourceFlag>() { sourceFlag };
    //    }

    //    //private List<SourceFlag> RemoveNoPermissionFourceFlags(List<SourceFlag> sourceFlags, RMPermissionMasks userPermission)
    //    //{
    //    //    //if (!RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled()) return sourceFlags;

    //    //    //var userPermission = SecurityTrimmingHelper.GetCurrentUserPermission();
    //    //    if (!userPermission.HasPermission(RMPermissionMasks.SPOEnduser) && sourceFlags.Contains(SourceFlag.SharePoint))
    //    //    {
    //    //        sourceFlags.Remove(SourceFlag.SharePoint);
    //    //    }

    //    //    if (!userPermission.HasPermission(RMPermissionMasks.EXOEnduser) && sourceFlags.Contains(SourceFlag.Exchange))
    //    //    {
    //    //        sourceFlags.Remove(SourceFlag.Exchange);
    //    //    }

    //    //    if (!userPermission.HasPermission(RMPermissionMasks.FSEnduser) && sourceFlags.Contains(SourceFlag.FileSystem))
    //    //    {
    //    //        sourceFlags.Remove(SourceFlag.FileSystem);
    //    //    }

    //    //    if (!userPermission.HasPermission(RMPermissionMasks.PhysicalEndUser) && sourceFlags.Contains(SourceFlag.Physical))
    //    //    {
    //    //        sourceFlags.Remove(SourceFlag.Physical);
    //    //    }

    //    //    return sourceFlags;
    //    //}


    //    private Expression<Func<Record, bool>> GetNewLambda(List<int> nodeTypes, List<Guid> exceptIds, List<string> containerIds, List<int> otherSourceFlags)
    //    {
    //        Expression<Func<Record, bool>>  lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord
    //    && (nodeTypes.Contains(s.NodeType))
    //    && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed)
    //    && (containerIds.Contains(s.ContainerId) || otherSourceFlags.Contains(s.SourceFlag));
    //        return lambda;
    //    }


    //    /// <summary>
    //    /// serach records for related records.
    //    /// </summary>
    //    /// <param name="pageIndex"></param>
    //    /// <param name="pageSize"></param>
    //    /// <param name="value"></param>
    //    /// <param name="exceptIds"></param>
    //    /// <param name="hasNext"></param>
    //    /// <returns></returns>
    //    public Tuple<IEnumerable<Record>, string> SearchRecords(string pageIndex, int pageSize, string value, List<Guid> exceptIds, List<int> permissions, SourceFlag sourceFlag, out bool hasNext, bool isEnduser = false)
    //    {

    //        var nodeTypes = new List<int> { (int)NodeLevel.Item, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
    //        var sourceFlags = ReAssembleFourceFlags(sourceFlag);
    //        if (RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled())
    //        {
    //            var userPermission = SecurityTrimmingHelper.GetCurrentUserPermission();
    //            userPermission.RemoveNoPermissionFourceFlags(sourceFlags);
    //            userPermission.RemoveNoPermissionNodeTypes(nodeTypes);
    //        }
    //        var intSourceFlags = sourceFlags.Select(o => (int)o);
    //        //由于Phy和SP都不查询Status是2的数据，暂不需要区分数据源
    //        Expression<Func<Record, bool>> lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord
    //        && (nodeTypes.Contains(s.NodeType))
    //        && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed)
    //        && (intSourceFlags.Contains(s.SourceFlag));

    //        var useSqlQuery = !string.IsNullOrEmpty(value) || isEnduser;


    //        if (sourceFlag == SourceFlag.FileSystem)
    //        {
    //            //var intSourceFlags = sourceFlags.Select(o => (int)o).ToList();
    //            useSqlQuery = false;
    //            lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord
    //            && (s.NodeType == (int)NodeLevel.FSFile)
    //            && (s.RecordStatus == (int)RMRecordStatus.Active)
    //            && (intSourceFlags.Contains(s.SourceFlag));
    //        }                

    //        Tuple<IEnumerable<Record>, string> result = null;
    //        if (useSqlQuery)
    //        {

    //            //value = value.ToLower();
    //            //lambda = s => (s.LeafName.Contains(value) || s.RecordsId.Contains(value)) && !exceptIds.Contains(s.Id) && !s.DeclareAsRecord
    //            //&& (s.NodeType == (int)NodeLevel.Item || s.NodeType == (int)RMNodeLevel.PhysicalFile || s.NodeType == (int)RMNodeLevel.PhysicalRecord)
    //            //&& (s.RecordStatus == (int)PhysicalRecordStatus.Open || s.RecordStatus == (int)PhysicalRecordStatus.Closed);

    //            //由于要支持根据name或者id的search，lambda表达式中无法支持大小写无关的contains查询，
    //            //因此改为拼装sql语句，在语句中调用cosmos db的内置函数，能够达到要求
    //            //var nodeTypes = new int[] { (int)NodeLevel.Item, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
    //            var recordStatus = new int[] { (int)RMRecordStatus.Active, (int)RMRecordStatus.Closed };
    //            SqlQuerySpec sqlQuery = null;
    //            if (isEnduser)
    //            {
    //                sqlQuery = CosmosSqlQueryHelper.BuildSearchForEnduser(value, exceptIds.ToArray(), nodeTypes.ToArray(), recordStatus, permissions);
    //            }
    //            else
    //            {
    //                sqlQuery = CosmosSqlQueryHelper.BuildSearch(value, exceptIds.ToArray(), nodeTypes.ToArray(), recordStatus, sourceFlags);
    //            }
    //            result = QueryPageBySql(sqlQuery, pageSize, pageIndex);

    //        }
    //        else
    //        {
    //            if (RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled())
    //            {
    //                //RemoveNoPermissionFourceFlags(sourceFlags);
    //                //if (sourceFlags.Contains(SourceFlag.SharePoint) || sourceFlags.Contains(SourceFlag.Exchange))
    //                //{
    //                var permissionCheckResult = SecurityTrimmingHelper.Check(sourceFlags);
    //                if (permissionCheckResult.NeedCheck)
    //                {
    //                    permissionCheckResult.RemoveSourceFlags(sourceFlags);
    //                    var containerIds = permissionCheckResult.GetContainerIds();
    //                    var otherSourceFlags = sourceFlags.Except(new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.Exchange })
    //                        .Select(o => (int)o).ToList();

    //                    lambda = GetNewLambda(nodeTypes, exceptIds, containerIds, otherSourceFlags);
    //                }
    //                //}
    //            }

    //            result = QueryByPage(lambda, pageSize, pageIndex);
    //        }

    //        hasNext = !string.IsNullOrEmpty(result.Item2);
    //        return result;
    //    }

    //    public Tuple<IEnumerable<Record>, string> QueryDataWithoutTotal(string continuation, int pageSize, out bool hasNext, Expression<Func<Record, bool>> whereLambda = null)
    //    {
    //        var list = QueryByPage(whereLambda, pageSize, continuation);
    //        hasNext = !string.IsNullOrEmpty(list.Item2);
    //        return list;
    //    }

    //    public List<Record> GetPhysicalRecordByRecordIds(List<string> uniqueIds)
    //    {
    //        return QueryAll(r => r.ScopeId == Guid.Empty && uniqueIds.Contains(r.RecordsId)).ToList();
    //    }
    //    public Tuple<IEnumerable<Record>, string> QueryDataBySqlWithoutTotal(ExplorerQueryDto dto, bool isGlobalSearch, string continuation, int pageSize, out bool hasNext, SecurityTermPermissionDto termPermDto = null)
    //    {
    //        var sqlQuerySpec = CosmosSqlQueryHelper.BuildSearch(dto, true, true, isGlobalSearch, true, termPermDto);
    //        logger.Info($"SQL query statement: {sqlQuerySpec.QueryText}");
    //        logSqlParam(sqlQuerySpec.Parameters);
    //        var list = QueryPageBySql(sqlQuerySpec, pageSize, continuation);
    //        hasNext = !string.IsNullOrEmpty(list.Item2);
    //        return list;
    //    }

    //    public List<Record> GetRecordsByIdPermssions(List<int> scopePermissions, List<Guid> recordIds)
    //    {
    //        var sqlQuery = CosmosSqlQueryHelper.GetRecordsByPermission(scopePermissions, recordIds);
    //        return _repository.QueryRecordsByPermission(sqlQuery);

    //    }
    //    public Tuple<IEnumerable<Record>, string> GetRecordsByContainer(Guid scopeId, string containerId, string continuation, int pageSize)
    //    {
    //        var sqlQuery = CosmosSqlQueryHelper.GenerateContianerIdQueyExpression(scopeId, containerId);
    //        var list = QueryPageBySql(sqlQuery, pageSize, continuation);
    //        return list;
    //    }
    //    public Tuple<IEnumerable<Record>, string> QueryDataBySqlWithoutTotal(PhysicalExplorerQueryDto dto, string continuation, int pageSize, out bool hasNext, SecurityTermPermissionDto termPermDto, bool withoutPhysicalRecord = false)
    //    {
    //        var sqlQuerySpec = CosmosSqlQueryHelper.BuildSerch(dto, termPermDto, withoutPhysicalRecord);
    //        logger.Info($"SQL query statement: {sqlQuerySpec.QueryText}");
    //        logSqlParam(sqlQuerySpec.Parameters);
    //        var list = QueryPageBySql(sqlQuerySpec, pageSize, continuation);
    //        hasNext = !string.IsNullOrEmpty(list.Item2);
    //        return list;
    //    }
    //    private void logSqlParam(SqlParameterCollection sqlParameter)
    //    {
    //        StringBuilder builder = new StringBuilder();
    //        foreach (SqlParameter p in sqlParameter)
    //        {
    //            builder.Append($"{p.Name} : {p.Value}").Append("; "); ;
    //        }
    //        logger.Info(builder.ToString());
    //    }
    //    public Tuple<IEnumerable<Record>, string> QueryPageBySqlForBrowse(RMPhysicalExplorerNode currentRecord, List<int> permissionIds, bool hasScopePermission, int pageCount = 15, string continuation = "", SecurityTermPermissionDto termPermDto = null)
    //    {
    //        var sqlQuerySpec = CosmosSqlQueryHelper.BuildSqlForBrowseTree(currentRecord, permissionIds, hasScopePermission, termPermDto);
    //        return QueryPageBySql(sqlQuerySpec, pageCount, continuation);
    //    }

    //    public List<Record> GetRecordByIds(List<Guid> ids)
    //    {
    //        return QueryAll(r => ids.Contains(r.Id)).ToList();
    //    }

    //    public void AddReocrdHistory(List<Guid> id, RecordHistoryXml xmlDto)
    //    {

    //        var history = QueryAll(s => id.Contains(s.Id)).Select(m => new { m.Id, m.RecordHistory }).ToList();
    //        foreach (var his in history)
    //        {
    //            string str = string.Empty;
    //            if (!string.IsNullOrEmpty(his.RecordHistory))
    //            {
    //                var old = XmlUtil.GetXmlObject<RecordHistoryXml>(his.RecordHistory);
    //                old.HistoryList.AddRange(xmlDto.HistoryList);
    //                str = XmlUtil.GetXmlString(old);
    //            }
    //            else
    //            {
    //                str = XmlUtil.GetXmlString(xmlDto);
    //            }
    //            UpdateAll(s => s.Id == his.Id, rec => { rec.RecordHistory = str; });
    //        }
    //    }

    //    public bool AddOrUpdateRecord(Record rec, bool forceUpdate)
    //    {
    //        bool result = false;
    //        rec.AppendCustomColumns();
    //        var dbRec = ReadById(rec.ScopeId, rec.Id);
    //        if (dbRec != null && dbRec.RecordStatus == (int)RMRecordStatus.Active)
    //        {
    //            //Hold状态Record重新计算Due Date;
    //            if (dbRec.HoldStatus)
    //            {
    //                if (rec.RuleId != null && rec.RuleId != Guid.Empty)
    //                {
    //                    var tempRule = RMRuleDao.GetRuleById(rec.RuleId);
    //                    if (tempRule != null && IsRemoveRule(tempRule, dbRec.SourceFlag))
    //                    {
    //                        long newDisposalDueDate = 0;
    //                        //Remove Rule需要计算Due Date
    //                        if (rec.DisposalDueDate == DueDateUtil.NextJob)
    //                        {
    //                            newDisposalDueDate = dbRec.HoldReleaseTime;
    //                        }
    //                        if (rec.DisposalDueDate > 0)
    //                        {
    //                            if (rec.DisposalDueDate > dbRec.HoldReleaseTime)
    //                            {
    //                                newDisposalDueDate = rec.DisposalDueDate;
    //                            }
    //                            else
    //                            {
    //                                newDisposalDueDate = dbRec.HoldReleaseTime;
    //                            }
    //                        }
    //                        rec.DisposalDueDate = newDisposalDueDate;
    //                    }
    //                }
    //            }
    //            if (forceUpdate)
    //            {
    //                UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
    //                {
    //                    r.TimeModified = rec.TimeModified;
    //                    r.TermId = rec.TermId;
    //                    r.TermName = rec.TermName;
    //                    r.DisposalDueDate = rec.DisposalDueDate;
    //                    r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
    //                    r.LeafName = rec.LeafName;
    //                    r.RuleId = rec.RuleId;
    //                    r.FolderId = rec.FolderId;
    //                    r.RecordOwner = rec.RecordOwner;
    //                    r.DirPath = rec.DirPath;
    //                    r.MetaInfo = rec.MetaInfo;
    //                    r.CustomColumnDic = rec.CustomColumnDic;
    //                    r.RuleLevel = rec.RuleLevel;
    //                    r.RelatedRecords = rec.RelatedRecords;
    //                    r.RelatedRecordsCount = rec.RelatedRecordsCount;
    //                    r.CollectTime = rec.CollectTime;
    //                    r.RecordsId = rec.RecordsId;
    //                    r.DeclareAsRecord = rec.DeclareAsRecord;
    //                    r.ModifiedBy = rec.ModifiedBy;
    //                    r.CreatedBy = rec.CreatedBy;
    //                    r.ExtensionForFile = rec.ExtensionForFile;
    //                    r.ExternalId = rec.ExternalId;
    //                    r.EmailAddress = rec.EmailAddress;
    //                    r.SendTo = rec.SendTo;
    //                    r.ContainerId = rec.ContainerId;
    //                    r.LeafName_Array = rec.LeafName_Array; 
    //                    r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
    //                    r.CreatedBy_Lower = rec.CreatedBy_Lower;
    //                    r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
    //                    r.ModifiedBy_Array = rec.ModifiedBy_Array;
    //                    r.CreatedBy_Array = rec.CreatedBy_Array;
    //                    r.DeclaredBy_Array = rec.DeclaredBy_Array;
    //                    r.RecordOwner_Array = rec.RecordOwner_Array;
    //                });
    //                result = true;
    //            }
    //            else
    //            {
    //                // compare uniqueid, add for document ID feature
    //                if (rec.TimeModified > dbRec.TimeModified || rec.TermId != dbRec.TermId || rec.DeclareAsRecord != dbRec.DeclareAsRecord
    //                    || dbRec.DisposalDueDate == DueDateUtil.Pending
    //                    || rec.RuleId != dbRec.RuleId || rec.ModifiedBy != dbRec.ModifiedBy || rec.CreatedBy != dbRec.CreatedBy
    //                    || rec.RecordsId != dbRec.RecordsId || rec.RecordOwner != dbRec.RecordOwner)  //add comparing record owner
    //                {
    //                    result = true;
    //                    UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
    //                    {
    //                        r.TimeModified = rec.TimeModified;
    //                        r.TermId = rec.TermId;
    //                        r.TermName = rec.TermName;
    //                        r.DisposalDueDate = rec.DisposalDueDate;
    //                        r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
    //                        r.LeafName = rec.LeafName;
    //                        r.RuleId = rec.RuleId;
    //                        r.FolderId = rec.FolderId;
    //                        r.RecordOwner = rec.RecordOwner;
    //                        r.DirPath = rec.DirPath;
    //                        r.MetaInfo = rec.MetaInfo;
    //                        r.CustomColumnDic = rec.CustomColumnDic;
    //                        r.RuleLevel = rec.RuleLevel;
    //                        r.RelatedRecords = rec.RelatedRecords;
    //                        r.RelatedRecordsCount = rec.RelatedRecordsCount;
    //                        r.CollectTime = rec.CollectTime;
    //                        r.RecordsId = rec.RecordsId;
    //                        r.DeclareAsRecord = rec.DeclareAsRecord;
    //                        r.ModifiedBy = rec.ModifiedBy;
    //                        r.CreatedBy = rec.CreatedBy;
    //                        r.ExtensionForFile = rec.ExtensionForFile;
    //                        r.ExternalId = rec.ExternalId;
    //                        r.EmailAddress = rec.EmailAddress;
    //                        r.SendTo = rec.SendTo;
    //                        r.ContainerId = rec.ContainerId;
    //                        r.LeafName_Array = rec.LeafName_Array;
    //                        r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
    //                        r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
    //                        r.ModifiedBy_Array = rec.ModifiedBy_Array;
    //                        r.DeclaredBy_Array = rec.DeclaredBy_Array;
    //                        r.RecordOwner_Array = rec.RecordOwner_Array;
    //                    });
    //                }
    //            }
    //        }
    //        else if (dbRec == null)
    //        {
    //            _repository.Add(rec);
    //            result = true;
    //        }
    //        return result;
    //    }

    //    private bool IsRemoveRule(RMRule tempRule, int sourceFlag)
    //    {
    //        var result = false;
    //        if ((int)SourceFlag.SharePoint == sourceFlag)
    //        {
    //            if (tempRule.DisposalAction == 0 || tempRule.DisposalAction == 2 || tempRule.DisposalAction == 5 || tempRule.DisposalAction == 7 || tempRule.DisposalAction == 8
    //            || tempRule.DisposalAction == 10 || tempRule.DisposalAction == 13 || tempRule.DisposalAction == 15 || tempRule.DisposalAction == 16 || tempRule.DisposalAction == 18
    //            || tempRule.DisposalAction == 21 || tempRule.DisposalAction == 23 || tempRule.DisposalAction == 24 || tempRule.DisposalAction == 26 || tempRule.DisposalAction == 29
    //            || tempRule.DisposalAction == 31)
    //            {
    //                result = true;
    //            }
    //        }
    //        if ((int)SourceFlag.Exchange == sourceFlag)
    //        {
    //            if (tempRule.ExchangeDisposalAction == 0)
    //            {
    //                result = true;
    //            }
    //        }
    //        if ((int)SourceFlag.FileSystem == sourceFlag)
    //        {
    //            switch (tempRule.FSDisposalAction)
    //            {
    //                case (int)RMContentDisposalAction.Remove:
    //                case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub:
    //                case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.RelatedRecords:
    //                case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords:
    //                    return true;
    //                default:
    //                    break;
    //            }
    //        }
    //        return result;
    //    }

    //    //public bool IsActiveRecords(Guid scopeId, Guid id)
    //    //{
    //    //    var rec = ReadById(scopeId, id);
    //    //    return rec != null && rec.RecordStatus == (int)RMRecordStatus.Active;
    //    //}

    //    public int UpdateRecordOwner(Guid scopeId, Guid nodeId, string owners)
    //    {
    //        owners = AddBeforeAndAfterSeparator(owners);
    //        return UpdateAll(s => s.ScopeId == scopeId && s.NodeId == nodeId, r => { r.RecordOwner = owners; r.RecordOwner_Array = owners.ExplorerSearchSplit(); });
    //    }

    //    public void UpdateRecordOwnerForPhysical(Guid id, string owners)
    //    {
    //        owners = AddBeforeAndAfterSeparator(owners);
    //        UpdateAll(s => s.Id == id, r => { r.RecordOwner = owners; r.RecordOwner_Array = owners.ExplorerSearchSplit(); });
    //    }

    //    public int UpdateRecordOwnerForFS(Guid nodeId, string owners)
    //    {
    //        owners = AddBeforeAndAfterSeparator(owners);
    //        return UpdateAll(s => s.NodeId == nodeId, r => { r.RecordOwner = owners; r.RecordOwner_Array = owners.ExplorerSearchSplit(); });
    //    }

    //    private string AddBeforeAndAfterSeparator(string source, string separator = "|")
    //    {
    //        if (!string.IsNullOrEmpty(source))
    //        {
    //            if (!source.StartsWith(separator))
    //            {
    //                source = separator + source;
    //            }
    //            if (!source.EndsWith(separator))
    //            {
    //                source = source + separator;
    //            }
    //            return source;
    //        }
    //        return string.Empty;
    //    }

    //    public void UpdateRecordState(Record rec, int status, List<Guid> subFolderIds = null)
    //    {
    //        if (rec != null)
    //        {
    //            Expression<Func<Record, bool>> lambda = s => s.ScopeId == rec.ScopeId;
    //            switch (rec.NodeType)
    //            {
    //                case (int)NodeLevel.Site:
    //                    lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId;
    //                    break;
    //                case (int)NodeLevel.List:
    //                    lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId;
    //                    break;
    //                case (int)NodeLevel.Folder:
    //                    //Get all folder id list under current folder...
    //                    var folderids = subFolderIds != null ? subFolderIds : GetAllSubFolderUnderFolder(rec);

    //                    lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && (folderids.Contains(s.FolderId) || folderids.Contains(s.NodeId));
    //                    break;
    //                case (int)NodeLevel.Item:
    //                    lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.ItemRowId == rec.ItemRowId;
    //                    break;
    //                default:
    //                    break;
    //            }
    //            UpdateAll(lambda, r => { r.RecordStatus = status; });
    //        }
    //    }

    //    public List<Guid> GetAllSubFolderUnderFolder(Record rec)
    //    {
    //        var result = new List<Guid>();
    //        if (rec.NodeType == (int)NodeLevel.Folder)
    //        {
    //            var currentFolderId = rec.NodeId;
    //            if (currentFolderId != Guid.Empty)
    //            {
    //                //当前Folder需要记录
    //                result.Add(currentFolderId);
    //                var tempList = GetAllSubFolderIds(rec);
    //                result.AddRange(tempList);
    //            }
    //        }
    //        return result;
    //    }
    //    private List<Guid> GetAllSubFolderIds(Record rec)
    //    {
    //        List<Guid> folderIds = new List<Guid>();
    //        if (rec.NodeType == (int)NodeLevel.Folder)
    //        {
    //            string curFolderDirPath = rec.DirPath + "/";
    //            Expression<Func<Record, bool>> lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.DirPath.Contains(curFolderDirPath) && s.NodeType == (int)NodeLevel.Folder;
    //            folderIds = GetFilterList(a => a.NodeId, lambda);

    //        }
    //        return folderIds;
    //    }

    //    //private void GetAllSubFolderIds(Guid scopeId, Guid webId, Guid listId, Guid currentFolderId, List<Guid> result)
    //    //{
    //    //    Expression<Func<Record, bool>> lambda = s => s.ScopeId == scopeId && s.WebId == webId && s.ListId == listId && s.FolderId == currentFolderId && s.NodeType == (int)NodeLevel.Folder;
    //    //    var tempFolderList = GetFilterList(a => new { currentFolderId = a.NodeId }, lambda);
    //    //    foreach (var tempFolder in tempFolderList)
    //    //    {
    //    //        if (result == null)
    //    //        {
    //    //            result = new List<Guid>();
    //    //        }
    //    //        result.Add(tempFolder.currentFolderId);
    //    //        GetAllSubFolderIds(scopeId, webId, listId, tempFolder.currentFolderId, result);
    //    //    }
    //    //}

    //    public void UpdateRecordState(Guid scopeId, Guid id, int status)
    //    {
    //        var rec = QueryAll(s => s.ScopeId == scopeId && s.Id == id).FirstOrDefault();
    //        if (rec != null)
    //        {
    //            UpdateAll(s => s.ScopeId == scopeId && s.Id == id && s.RecordStatus == (int)RMRecordStatus.Active, r => { r.RecordStatus = status; });
    //        }
    //    }

    //    public Record ReadSPRecordById(Guid scopeId, Guid webId, Guid listId, int itemRowId)
    //    {
    //        return QueryAll(s => s.ScopeId == scopeId && s.WebId == webId && s.ListId == listId && s.ItemRowId == itemRowId).FirstOrDefault();
    //    }

    //    public bool CheckHasData()
    //    {
    //        return _repository.CheckHasData();

    //    }
    //    public void DeleteExplorerData(string tenantId)
    //    {
    //        if (_repository.ConnectionExist(tenantId))
    //        {
    //            _repository.DeleteConnection(tenantId);
    //        }
    //        else
    //        {
    //            logger.Info($"connection not exist:{tenantId}");
    //        }
    //    }

    //    #region Physical Associated
    //    public List<Record> GetWaitingApproveItemForPhysical()
    //    {
    //        return QueryAll(m => m.DisposalStatus == (int)SOApproveDBStatus.WaitingApprove && m.ExportToRECO == false
    //                && m.SourceFlag == (int)SourceFlag.Physical
    //                && (m.NodeType == (int)RMNodeType.PhyFile || m.NodeType == (int)RMNodeType.PhyBox)).ToList();
    //    }

    //    public void UpdateItemToExportStatus(Guid id)
    //    {
    //        var rec = QueryAll(s => s.Id == id).FirstOrDefault();
    //        if (rec != null)
    //        {
    //            UpdateAll(s => s.Id == id, r => { r.ExportToRECO = true; });
    //        }
    //    }

    //    public void UpdateApproveStatus(Guid id, SOApproveDBStatus status)
    //    {
    //        var rec = QueryAll(s => s.Id == id).FirstOrDefault();
    //        if (rec != null)
    //        {
    //            UpdateAll(s => s.Id == id, r => { r.DisposalStatus = (int)status; });
    //        }
    //    }

    //    public Record GetPhysicalRawDataById(Guid id)
    //    {
    //        return _repository.QueryAll(s => s.ScopeId == Guid.Empty && s.Id == id).FirstOrDefault();
    //    }

    //    public bool AddPhysicalRecord(Record rec)
    //    {
    //        bool result = true;
    //        try
    //        {
    //            rec.AppendCustomColumns();
    //            _repository.Add(rec);
    //        }
    //        catch (Exception e)
    //        {
    //            result = false;
    //        }
    //        return result;
    //    }
    //    /// <summary>
    //    /// Physical Record Import Check
    //    /// </summary>
    //    /// <param name="uniqueId"></param>
    //    /// <returns></returns>
    //    public Record GetPhysicalRecordByRecordsId(string uniqueId)
    //    {
    //        return QueryAll(s => s.ScopeId == Guid.Empty && s.RecordsId == uniqueId).FirstOrDefault();
    //    }
    //    public Record GetPhysicalRecordById(Guid id)
    //    {
    //        return QueryAll(s => s.SourceFlag == (int)SourceFlag.Physical && s.Id == id && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
    //    }

    //    public bool UpdatePhysicalRecord(Record rec, bool forceUpdate, bool isModifyPermissionId = false)
    //    {
    //        bool result = false;
    //        rec.AppendCustomColumns();
    //        if (forceUpdate)
    //        {
    //            UpdateAll(r => r.NodeId == rec.NodeId, r =>
    //            {
    //                r.LeafName = rec.LeafName;
    //                r.NodeType = rec.NodeType;
    //                r.RecordsId = rec.RecordsId;
    //                r.TermId = rec.TermId;
    //                r.TermName = rec.TermName;
    //                r.LocationId = rec.LocationId;
    //                r.BoxId = rec.BoxId;
    //                r.FileId = rec.FileId;
    //                r.TemplateId = rec.TemplateId;
    //                r.IsLocked = rec.IsLocked;
    //                r.MetaInfo = rec.MetaInfo;
    //                r.CustomColumnDic = rec.CustomColumnDic;
    //                r.TimeCreated = rec.TimeCreated;
    //                r.TimeModified = rec.TimeModified;
    //                r.CreatedBy = rec.CreatedBy;
    //                r.ModifiedBy = rec.ModifiedBy;
    //                r.DisposalDueDate = rec.DisposalDueDate;
    //                r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
    //                r.RuleId = rec.RuleId;
    //                r.RuleLevel = rec.RuleLevel;
    //                r.RecordStatus = rec.RecordStatus;
    //                r.DisposalStatus = rec.DisposalStatus;
    //                r.ExportToRECO = rec.ExportToRECO;
    //                r.DestroyedTime = rec.DestroyedTime;
    //                r.HoldType = rec.HoldType;
    //                r.HoldBy = rec.HoldBy;
    //                r.HoldReleaseTime = rec.HoldReleaseTime;
    //                r.HoldId = rec.HoldId;
    //                r.HoldStatus = rec.HoldStatus;
    //                r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
    //                r.RelatedRecords = rec.RelatedRecords;
    //                r.RelatedRecordsCount = rec.RelatedRecordsCount;
    //                r.ScopePermissionId = isModifyPermissionId ? rec.ScopePermissionId : r.ScopePermissionId;
    //                r.LeafName_Array = rec.LeafName_Array; 
    //                r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
    //                r.CreatedBy_Lower = rec.CreatedBy_Lower;
    //                r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
    //                r.ModifiedBy_Array = rec.ModifiedBy_Array;
    //                r.CreatedBy_Array = rec.CreatedBy_Array;
    //                r.DeclaredBy_Array = rec.DeclaredBy_Array;
    //            });
    //            result = true;
    //        }
    //        else
    //        {
    //            UpdateAll(r => r.NodeId == rec.NodeId, r =>
    //            {
    //                r.LeafName = rec.LeafName;
    //                r.TermId = rec.TermId;
    //                r.TermName = rec.TermName;
    //                r.IsLocked = rec.IsLocked;
    //                r.MetaInfo = rec.MetaInfo;
    //                r.CustomColumnDic = rec.CustomColumnDic;
    //                r.TimeModified = rec.TimeModified;
    //                r.ModifiedBy = rec.ModifiedBy;
    //                r.DisposalDueDate = rec.DisposalDueDate;
    //                r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
    //                r.RuleId = rec.RuleId;
    //                r.RuleLevel = rec.RuleLevel;
    //                r.RecordStatus = rec.RecordStatus;
    //                r.DisposalStatus = rec.DisposalStatus;
    //                r.ExportToRECO = rec.ExportToRECO;
    //                r.DestroyedTime = rec.DestroyedTime;
    //                r.HoldType = rec.HoldType;
    //                r.HoldBy = rec.HoldBy;
    //                r.HoldReleaseTime = rec.HoldReleaseTime;
    //                r.HoldId = rec.HoldId;
    //                r.HoldStatus = rec.HoldStatus;
    //                r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
    //                r.ScopePermissionId = isModifyPermissionId ? rec.ScopePermissionId : r.ScopePermissionId;
    //                r.LeafName_Array = rec.LeafName_Array; 
    //                r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
    //                r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
    //                r.ModifiedBy_Array = rec.ModifiedBy_Array;
    //                r.DeclaredBy_Array = rec.DeclaredBy_Array;
    //            });
    //            result = true;
    //        }
    //        return result;
    //    }
    //    #endregion

    //    #region fs
    //    public Record GetFSRecordById(Guid id)
    //    {
    //        return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.Id == id && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
    //    }
    //    public Record GetFSRecord(Guid Scopeid, Guid id)
    //    {
    //        return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ScopeId == Scopeid && s.Id == id && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
    //    }

    //    public List<Record> GetFSChildNodes(Guid parentId, int fsType)
    //    {
    //        if (fsType != (int)NodeLevel.FSFolder)
    //        {
    //            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ParentId.Equals(parentId)).OrderBy(a => a.LeafName).ToList();
    //        }
    //        else
    //        {
    //            return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ParentId.Equals(parentId) && s.NodeType != (int)NodeLevel.FSFile).OrderBy(a => a.LeafName).ToList();
    //        }
    //    }

    //    public List<Record> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
    //    {
    //        return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem
    //         && (s.NodeType == (int)NodeLevel.FSFile
    //         && s.RecordStatus == (int)RMRecordStatus.Active
    //         && s.FolderId == (new Guid(folderId))
    //         && s.ScopeId == (new Guid(scopeId))
    //         || s.NodeType == (int)NodeLevel.FSFolder
    //         && s.Id == new Guid(folderId)
    //         && s.ScopeId == new Guid(scopeId)))
    //             .OrderBy(s => s.SortTicks)
    //             .Where(s => s.SortTicks > sortTicks)
    //             .Take(pageSize).ToList();
    //    }

    //    public int UpdateFSDeleteRecord(Guid id, Guid scopeId, int status)
    //    {
    //        return UpdateAll(r => r.ScopeId == scopeId && r.NodeId == id, rec => { rec.RecordStatus = status; rec.DestroyedTime = DateTime.UtcNow.Ticks; });
    //    }

    //    public Record GetFSRootNode()
    //    {
    //        return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.NodeType == (int)NodeLevel.FSConnectionGroups).FirstOrDefault();
    //    }

    //    public bool AddFileSystemRecord(Record rec)
    //    {
    //        bool result = true;
    //        try
    //        {
    //            _repository.Add(rec);
    //        }
    //        catch (Exception e)
    //        {
    //            throw;
    //        }
    //        return result;
    //    }

    //    //public int SubChildrenCount(Guid nodeId)
    //    //{
    //    //    return QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem && s.ParentId.Equals(nodeId) && s.NodeType != (int)NodeLevel.FSFile).Count();
    //    //}
    //    public bool UpdateFileSystemRecord(Record rec, bool forceUpdate)
    //    {
    //        bool result = false;
    //        rec.AppendCustomColumns();
    //        if (forceUpdate)
    //        {
    //            UpdateAll(r=>r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
    //            {
    //                r.LeafName = rec.LeafName;
    //                r.NodeType = rec.NodeType;
    //                r.RecordsId = rec.RecordsId;
    //                r.TermId = rec.TermId;
    //                r.TermName = rec.TermName;
    //                //r.LocationId = rec.LocationId;
    //                //r.BoxId = rec.BoxId;
    //                //r.FileId = rec.FileId;
    //                //r.TemplateId = rec.TemplateId;
    //                r.IsLocked = rec.IsLocked;
    //                r.MetaInfo = rec.MetaInfo;
    //                //r.TimeCreated = rec.TimeCreated;
    //                r.TimeModified = rec.TimeModified;
    //                r.CreatedBy = rec.CreatedBy;
    //                r.ModifiedBy = rec.ModifiedBy;
    //                r.DisposalDueDate = rec.DisposalDueDate;
    //                r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
    //                r.RuleId = rec.RuleId;
    //                r.RuleLevel = rec.RuleLevel;
    //                r.RecordStatus = rec.RecordStatus;
    //                r.DisposalStatus = rec.DisposalStatus;
    //                r.ExportToRECO = rec.ExportToRECO;
    //                r.DestroyedTime = rec.DestroyedTime;
    //                r.HoldType = rec.HoldType;
    //                r.HoldBy = rec.HoldBy;
    //                r.HoldReleaseTime = rec.HoldReleaseTime;
    //                r.HoldId = rec.HoldId;
    //                r.HoldStatus = rec.HoldStatus;
    //                r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
    //                r.RelatedRecords = rec.RelatedRecords;
    //                r.RelatedRecordsCount = rec.RelatedRecordsCount;
    //                r.ScopePermissionId = rec.ScopePermissionId;
    //                r.RecordOwner = rec.RecordOwner;
    //                r.SortTicks = rec.SortTicks;
    //                r.LeafName_Array = rec.LeafName_Array; 
    //                r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
    //                r.CreatedBy_Lower = rec.CreatedBy_Lower;
    //                r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
    //                r.ModifiedBy_Array = rec.ModifiedBy_Array;
    //                r.CreatedBy_Array = rec.CreatedBy_Array;
    //                r.DeclaredBy_Array = rec.DeclaredBy_Array;
    //                r.RecordOwner_Array = rec.RecordOwner_Array;
    //            });
    //            result = true;
    //        }
    //        else
    //        {
    //            UpdateAll(r => r.ScopeId == rec.ScopeId && r.NodeId == rec.NodeId, r =>
    //            {
    //                r.LeafName = rec.LeafName;
    //                r.TermId = rec.TermId;
    //                r.TermName = rec.TermName;
    //                r.IsLocked = rec.IsLocked;
    //                r.MetaInfo = rec.MetaInfo;
    //                r.TimeModified = rec.TimeModified;
    //                r.ModifiedBy = rec.ModifiedBy;
    //                r.DisposalDueDate = rec.DisposalDueDate;
    //                r.PreviosDisposalDueDate = rec.PreviosDisposalDueDate;
    //                r.RuleId = rec.RuleId;
    //                r.RuleLevel = rec.RuleLevel;
    //                r.RecordStatus = rec.RecordStatus;
    //                r.DisposalStatus = rec.DisposalStatus;
    //                r.ExportToRECO = rec.ExportToRECO;
    //                r.DestroyedTime = rec.DestroyedTime;
    //                r.HoldType = rec.HoldType;
    //                r.HoldBy = rec.HoldBy;
    //                r.HoldReleaseTime = rec.HoldReleaseTime;
    //                r.HoldId = rec.HoldId;
    //                r.HoldStatus = rec.HoldStatus;
    //                r.DeleteRelatedRecords = rec.DeleteRelatedRecords;
    //                r.ScopePermissionId = rec.ScopePermissionId;
    //                r.RecordOwner = rec.RecordOwner;
    //                r.LeafName_Array = rec.LeafName_Array; 
    //                r.ModifiedBy_Lower = rec.ModifiedBy_Lower;
    //                r.DeclaredBy_Lower = rec.DeclaredBy_Lower;
    //                r.ModifiedBy_Array = rec.ModifiedBy_Array;
    //                r.DeclaredBy_Array = rec.DeclaredBy_Array;
    //                r.RecordOwner_Array = rec.RecordOwner_Array;
    //            });
    //            result = true;
    //        }
    //        return result;
    //    }
    //    #endregion

    //    public Tuple<IEnumerable<Record>, string> SearchRecordsV2(ExplorerQueryV2Dto dto, SqlQuerySpecBuilder sqlQuerySpecBuilder = null)
    //    {
    //        sqlQuerySpecBuilder = sqlQuerySpecBuilder ?? SqlQuerySpecBuilderFactory.Create(); //if not has a builder, use the default builder

    //        var sqlQuery = sqlQuerySpecBuilder.Build(dto.QueryOption);
    //        logger.Info($"SQL query V2 : {sqlQuery.QueryText}");
    //        var result = _repository.QueryPageBySql(sqlQuery, dto.PagingInfo.PageSize, dto.PagingInfo.PageIndex);

    //        return result;
    //    }

    //    public Tuple<IEnumerable<Record>, string> QueryPageBySqlForTermBrowse(RMNodeType nodeType, Guid termId, List<int> permissionIds, bool hasScopePermission, int pageCount = 15, string continuation = "")
    //    {
    //        var sqlQuerySpec = CosmosSqlQueryHelper.BuildSqlForBrowseTermTree(nodeType, termId, permissionIds, hasScopePermission);
    //        return _repository.QueryPageBySql(sqlQuerySpec, pageCount, continuation);
    //    }
    //}
}
