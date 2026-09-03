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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Z.EntityFramework.Plus;
using System.Linq.Expressions;
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class CollectionDataDao : ICollectionDataDao
    {
        protected static readonly IRALogger logger = RALogger.GetInstance(typeof(CollectionDataDao));

        #region test code
        //internal void LogSql(string sql)
        //{
        //    logger.Debug(sql);
        //}
        #endregion

        //public bool AddOrUpdateRecord(RMManagedRecord rec, bool forceUpdate)
        //{
        //    int rows = 0;
        //    using (var ctx = GetExplorerContext())
        //    {
        //        var exist = ctx.ManagedRecord.Any(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId);
        //        if (!exist)
        //        {
        //            ctx.ManagedRecord.Add(rec);
        //            rows = ctx.SaveChanges();
        //        }
        //        else
        //        {
        //            if (forceUpdate)
        //            {
        //                rows = ctx.ManagedRecord.Where(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId)
        //                       .Update(m => new RMManagedRecord()
        //                       {
        //                           TimeLastModified = rec.TimeLastModified,
        //                           TermId = rec.TermId,
        //                           TermName = rec.TermName,
        //                           DisposalDueDate = rec.DisposalDueDate,
        //                           LeafName = rec.LeafName,
        //                           FullPath = rec.FullPath,
        //                           RuleId = rec.RuleId,
        //                           FolderId = rec.FolderId,
        //                           RecordOwner = rec.RecordOwner,
        //                           DirPath = rec.DirPath,
        //                           MetaInfo = rec.MetaInfo,
        //                           RuleLevel = rec.RuleLevel,
        //                           RelatedRecords = rec.RelatedRecords,
        //                           RelatedRecordsCount = rec.RelatedRecordsCount,
        //                           CollectionTime = rec.CollectionTime,
        //                           RecordsId = rec.RecordsId,
        //                           ExtensionForFile = rec.ExtensionForFile,
        //                           DeclareAsRecord = rec.DeclareAsRecord,
        //                           ModifiedBy = rec.ModifiedBy
        //                       });
        //            }
        //            else
        //            {
        //                var dbRec = ctx.ManagedRecord.Where(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId).Select(d => new { Modified = d.TimeLastModified, TermId = d.TermId, Declare = d.DeclareAsRecord }).FirstOrDefault();
        //                if (rec.TimeLastModified > dbRec.Modified || rec.TermId != dbRec.TermId || rec.DeclareAsRecord != dbRec.Declare)
        //                {
        //                    rows = ctx.ManagedRecord.Where(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId)
        //                          .Update(m => new RMManagedRecord()
        //                          {
        //                              TimeLastModified = rec.TimeLastModified,
        //                              TermId = rec.TermId,
        //                              TermName = rec.TermName,
        //                              DisposalDueDate = rec.DisposalDueDate,
        //                              LeafName = rec.LeafName,
        //                              FullPath = rec.FullPath,
        //                              RuleId = rec.RuleId,
        //                              FolderId = rec.FolderId,
        //                              RecordOwner = rec.RecordOwner,
        //                              DirPath = rec.DirPath,
        //                              MetaInfo = rec.MetaInfo,
        //                              RuleLevel = rec.RuleLevel,
        //                              RelatedRecords = rec.RelatedRecords,
        //                              RelatedRecordsCount = rec.RelatedRecordsCount,
        //                              CollectionTime = rec.CollectionTime,
        //                              ExtensionForFile = rec.ExtensionForFile,
        //                              RecordsId = rec.RecordsId,
        //                              DeclareAsRecord = rec.DeclareAsRecord,
        //                              ModifiedBy = rec.ModifiedBy
        //                          });
        //                }
        //            }

        //        }
        //    }
        //    return rows > 0;

        //}

        //public bool BatchAddRecords(List<RMManagedRecord> recList)
        //{
        //    int rows = 0;
        //    using (var ctx = GetExplorerContext())
        //    {
        //        List<RMManagedRecord> batchAdd = new List<RMManagedRecord>();
        //        foreach (var rec in recList)
        //        {
        //            if (!ctx.ManagedRecord.Any(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId))
        //            {
        //                batchAdd.Add(rec);
        //            }
        //            else
        //            {
        //                var dbRec = ctx.ManagedRecord.Where(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId).Select(d => new { Modified = d.TimeLastModified, TermId = d.TermId }).FirstOrDefault();
        //                if (rec.TimeLastModified > dbRec.Modified || rec.TermId != dbRec.TermId)
        //                {
        //                    rows = ctx.ManagedRecord.Where(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId)
        //                          .Update(m => new RMManagedRecord()
        //                          {
        //                              TimeLastModified = rec.TimeLastModified,
        //                              TermId = rec.TermId,
        //                              TermName = rec.TermName,
        //                              DisposalDueDate = rec.DisposalDueDate,
        //                              LeafName = rec.LeafName,
        //                              FullPath = rec.FullPath,
        //                              RuleId = rec.RuleId,
        //                              FolderId = rec.FolderId,
        //                              RecordOwner = rec.RecordOwner,
        //                              DirPath = rec.DirPath,
        //                              MetaInfo = rec.MetaInfo,
        //                              RuleLevel = rec.RuleLevel,
        //                              RelatedRecords = rec.RelatedRecords,
        //                              RelatedRecordsCount = rec.RelatedRecordsCount,
        //                              CollectionTime = rec.CollectionTime,
        //                              RecordsId = rec.RecordsId,
        //                              DeclareAsRecord = rec.DeclareAsRecord,
        //                          });
        //                }
        //            }
        //        }
        //        if (batchAdd.Count() > 0)
        //        {
        //            ctx.ManagedRecord.AddRange(batchAdd);
        //            rows = ctx.SaveChanges();
        //        }

        //    }
        //    return rows > 0;
        //}

        public RMBaseRecord GetDataById(bool destroyed, int id)
        {
            RMBaseRecord rec = null;
            using (var ctx = GetExplorerContext())
            {
                if (destroyed)
                {
                    rec = ctx.ArchivedRecord.Where(m => m.Id == id).FirstOrDefault();
                }
                else
                {
                    rec = ctx.ManagedRecord.Where(m => m.Id == id).FirstOrDefault();
                }

            }
            //REC-3551
            if (rec != null)
            {
                if (rec.ExtensionForFile == "RM_RDM_RecordDetails_DataType_FileNull")
                {
                    rec.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_FileNull");
                }
                if (rec.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                {
                    rec.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_SPItem");
                }
            }
            return rec;
        }

        public void MoveToArchived(Guid scopeId, Guid nodeId)
        {
            using (var ctx = GetExplorerContext())
            {
                var rec = ctx.ManagedRecord.Where(m => m.ScopeId == nodeId && m.NodeId == nodeId).FirstOrDefault();
                if (rec != null)
                {
                    Expression<Func<RMManagedRecord, bool>> lambda = s => s.ScopeId == nodeId;
                    if (rec.NodeType == (int)NodeLevel.SiteCollection)
                    {
                        lambda = s => s.ScopeId == scopeId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.Site)
                    {
                        lambda = s => s.ScopeId == scopeId && s.WebId == nodeId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.List)
                    {
                        lambda = s => s.ScopeId == scopeId && s.ListId == nodeId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.Folder)
                    {
                        lambda = s => s.ScopeId == scopeId && s.FolderId == nodeId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.Item)
                    {
                        lambda = s => s.ScopeId == scopeId && s.ItemId == nodeId;
                    }
                    var records = ctx.ManagedRecord.Where(lambda).ToList<RMManagedRecord>();
                    List<int> recordIds = new List<int>();
                    foreach (var record in records)
                    {
                        recordIds.Add(record.Id);
                    }
                    ctx.Alliance.Where(a => recordIds.Any(id => id == a.RecordsId)).Delete();

                    var list = ctx.ManagedRecord.Where(lambda).ToList().ConvertAll(m => ConvertUtil.ConvertToRMArchivedRecord(m));
                    ctx.ArchivedRecord.AddRange(list);
                    ctx.SaveChanges();
                    ctx.ManagedRecord.Where(lambda).Delete();
                }
            }
        }

        public void MoveToDeleted(Guid scopeId, Guid webId, Guid listId, Guid folderId, int itemId)
        {
            using (var ctx = GetExplorerContext())
            {
                RMManagedRecord rec = null;
                rec = ctx.ManagedRecord.Where(m => m.ScopeId == scopeId && m.WebId == webId && m.ListId == listId && m.FolderId == folderId && m.ItemRowId == itemId).FirstOrDefault();

                if (rec != null)
                {
                    Expression<Func<RMManagedRecord, bool>> lambda = s => s.ScopeId == scopeId;
                    if (rec.NodeType == (int)NodeLevel.SiteCollection)
                    {
                        lambda = s => s.ScopeId == scopeId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.Site)
                    {
                        lambda = s => s.ScopeId == scopeId && s.WebId == webId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.List)
                    {
                        lambda = s => s.ScopeId == scopeId && s.WebId == webId && s.ListId == listId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.Folder)
                    {
                        lambda = s => s.ScopeId == scopeId && s.WebId == webId && s.ListId == listId && s.FolderId == folderId;
                    }
                    else if (rec.NodeType == (int)NodeLevel.Item)
                    {
                        lambda = s => s.ScopeId == scopeId && s.WebId == webId && s.ListId == listId && s.FolderId == folderId && s.ItemRowId == itemId;
                    }
                    var records = ctx.ManagedRecord.Where(lambda).ToList<RMManagedRecord>();
                    List<int> recordIds = new List<int>();
                    foreach (var record in records)
                    {
                        recordIds.Add(record.Id);
                    }
                    ctx.Alliance.Where(a => recordIds.Any(id => id == a.RecordsId)).Delete();

                    var list = ctx.ManagedRecord.Where(lambda).ToList().ConvertAll(m => ConvertUtil.ConvertToRMDeletedRecord(m));
                    ctx.DeletedRecord.AddRange(list);
                    ctx.SaveChanges();
                    ctx.ManagedRecord.Where(lambda).Delete();
                }
            }
        }

        public List<RMBaseRecord> GetRecordByIds(List<int> ids)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(m => ids.Any(id => m.Id == id)).ToList<RMBaseRecord>();
            }
        }

        public RMBaseRecord GetRecordByItemId(Guid siteId, Guid itemId)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(m => m.ScopeId == siteId && m.NodeId == itemId).FirstOrDefault();
            }
        }
        public RMBaseRecord GetRecordById(int id)
        {
            RMBaseRecord rec = null;
            using (var ctx = GetExplorerContext())
            {
                var dto = (from m in ctx.ManagedRecord
                           join s in ctx.Scope on m.ScopeId equals s.ScopeId
                           where m.Id == id
                           select new { m, s.FullPath }).FirstOrDefault();

                if (dto != null && dto.m != null)
                {
                    rec = dto.m;
                    rec.FullPath = WebUtil.MakeFullUrl(dto.FullPath, rec.DirPath);
                }
                return rec;

            }
        }

        public List<RMManagedRecord> GetRecordByNodeType(Guid scopeId, Guid spObjId, int nodeType)
        {
            using (var ctx = GetExplorerContext())
            {
                Expression<Func<RMManagedRecord, bool>> lambda = null;
                if (nodeType == (int)NodeLevel.SiteCollection)
                {
                    lambda = s => s.ScopeId == scopeId;
                }
                else if (nodeType == (int)NodeLevel.Site)
                {
                    lambda = s => s.ScopeId == scopeId && s.WebId == spObjId;
                }
                else if (nodeType == (int)NodeLevel.List)
                {
                    lambda = s => s.ScopeId == scopeId && s.ListId == spObjId;
                }
                else if (nodeType == (int)NodeLevel.Folder)
                {
                    lambda = s => s.ScopeId == scopeId && s.FolderId == spObjId;
                }
                else if (nodeType == (int)NodeLevel.Item)
                {
                    lambda = s => s.ScopeId == scopeId && s.ItemId == spObjId;
                }
                if (lambda == null)
                {
                    return new List<RMManagedRecord>();
                }
                return ctx.ManagedRecord.Where(lambda).ToList();

            }
        }

        public RMBaseRecord GetRecordByNodeId(Guid siteId, Guid spObjectId)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(r => r.ScopeId == siteId && r.NodeId == spObjectId).FirstOrDefault();
            }
        }
        public RMBaseRecord GetRecordByConnectioIDNodeId(Guid nodeId)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(r => r.NodeId == nodeId).FirstOrDefault();
            }
        }
        public string GetUniqueId(Guid siteId, Guid spObjectId)
        {
            using (var ctx = GetExplorerContext())
            {
                if (ctx.ManagedRecord.Any(r => r.ScopeId == siteId && r.NodeId == spObjectId))
                {
                    return ctx.ManagedRecord.Where(r => r.ScopeId == siteId && r.NodeId == spObjectId).Select(m => m.RecordsId).FirstOrDefault();
                }
                else
                {
                    return string.Empty;
                }

            }
        }
        public List<RMBaseRecord> SearchRecords(int pageIndex, int pageSize, string value, List<int> exceptIds, out bool hasNext)
        {
            using (var ctx = GetExplorerContext())
            {
                ctx.Database.Log = SQLLog;
                Expression<Func<RMBaseRecord, bool>> lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord;
                if (!string.IsNullOrEmpty(value))
                {
                    //ctx.Database.Log = LogSql;//TEST
                    if (FullTextIndex.FullTextIndexInitializer.IndexExists)
                    {
                        string contains = FullTextIndex.FullTextSearchModelUtil.GetContainsKeySplitBySpace(value);
                        lambda = s => (s.LeafName + s.RecordsId).Contains(contains) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.Item && !s.DeclareAsRecord;
                    }
                    else
                    {
                        lambda = s => (s.LeafName.Contains(value) || s.RecordsId.Contains(value)) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.Item && !s.DeclareAsRecord;
                    }
                }
                var ids = ctx.ManagedRecord.Where(d => d.NodeType == (int)NodeLevel.Item).Where(lambda).OrderByDescending(d => d.Id).Select(s => s.Id).PagingWithNextFirst(pageIndex, pageSize);
                //!d.DeclareAsRecord此处在search的之后，直接过滤，在update的时候，仍需要添加后台验证
                var result = (from m in ctx.ManagedRecord where ids.Contains(m.Id) select m).ToList<RMBaseRecord>();
                hasNext = result.Count > pageSize;
                return result.Take(pageSize).ToList();
            }
        }

        public int SearchRecordsGetTotal(string value, List<int> exceptIds)
        {
            int totalRecord = 0;
            using (var ctx = GetExplorerContext())
            {
                Expression<Func<RMBaseRecord, bool>> lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord;
                if (!string.IsNullOrEmpty(value))
                {
                    //ctx.Database.Log = LogSql;//TEST
                    if (FullTextIndex.FullTextIndexInitializer.IndexExists)
                    {
                        string contains = FullTextIndex.FullTextSearchModelUtil.GetContainsKeySplitBySpace(value);
                        lambda = s => (s.LeafName + s.RecordsId).Contains(contains) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.Item && !s.DeclareAsRecord;
                    }
                    else
                    {
                        lambda = s => (s.LeafName.Contains(value) || s.RecordsId.Contains(value)) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.Item && !s.DeclareAsRecord;
                    }
                }
                totalRecord = ctx.ManagedRecord.Where(d => d.NodeType == (int)NodeLevel.Item).Where(lambda).Count();
                //var ids = ctx.ManagedRecord.Where(d => d.NodeType == (int)NodeLevel.Item || d.NodeType == (int)NodeLevel.FSFolder || d.NodeType == (int)NodeLevel.FSFile).Where(lambda).OrderByDescending(d => d.Id).Select(s => s.Id).Paging(pageIndex, pageSize, out totalRecord);
            }
            return totalRecord;
        }
        public bool UpdateRelatedRecords(int id, string infoxXML, int relatedCount)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(m => m.Id == id).Update(m => new RMManagedRecord() { RelatedRecordsCount = relatedCount, RelatedRecords = infoxXML }) > 0;
            }
        }

        public bool UpdateDeclaredRecords(List<int> ids)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(d => ids.Contains(d.Id))
                        .Update(m => new RMManagedRecord() { DeclareAsRecord = true }) > 0;
            }
        }
        public bool UpdateUnDeclaredRecords(List<int> ids)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(d => ids.Contains(d.Id))
                        .Update(m => new RMManagedRecord() { DeclareAsRecord = false }) > 0;
            }
        }
        public bool UpdateRecordOnwer(Guid scopeId, Guid nodeId, string ownerIds)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(m => m.ScopeId == scopeId && m.NodeId == nodeId).Update(m => new RMManagedRecord() { RecordOwner = ownerIds }) > 0;
            }
        }

        //public long GetScopeCollectionTime(Guid scopeId)
        //{
        //    using (var ctx = GetExplorerContext())
        //    {
        //        var scopeNode = ctx.Scope.Where(s => s.ScopeId == scopeId).FirstOrDefault();
        //        if (scopeNode != null)
        //        {
        //            return scopeNode.CollectionTime;
        //        }
        //        else
        //        {
        //            return DateTime.MinValue.Ticks;
        //        }
        //    }
        //}
        //public void AddSiteScope(RMScope scope)
        //{
        //    using (var ctx = GetExplorerContext())
        //    {
        //        if (!ctx.Scope.Any(s => s.ScopeId == scope.ScopeId))
        //        {
        //            ctx.Scope.Add(scope);
        //            ctx.SaveChanges();
        //        }
        //        else
        //        {
        //            var scopeNode = ctx.Scope.Where(s => s.ScopeId == scope.ScopeId)
        //                .Update(m => new RMScope()
        //                {
        //                    CollectionTime = scope.CollectionTime,
        //                    FullPath = scope.FullPath,
        //                    ScopeName = scope.ScopeName
        //                });
        //        }
        //    }
        //}

        //public bool IsScopeInfoExist(Guid nodeId)
        //{
        //    using (var ctx = GetExplorerContext())
        //    {
        //        return ctx.Scope.Any(s => s.IsRemoved == false && s.NodeId == nodeId);

        //    }
        //}


        //public List<RMScope> GetExistScopeInfo()
        //{
        //    using (var ctx = GetExplorerContext())
        //    {
        //        return ctx.Scope.AsQueryable().Where(s => s.IsRemoved == false).Select(s => new { ScopeId = s.ScopeId, ScopeName = s.ScopeName }).AsEnumerable().Select(x => new RMScope() { ScopeId = x.ScopeId, ScopeName = x.ScopeName }).ToList();
        //    }
        //}
        private Core.RMDbContext GetNewContext()
        {
            return Core.RMDBContextManager.GetNewDBContext();
        }

        public List<RMSiteCollectionSize> GetBoardCollectionTop10Data(int beginIndex)
        {

            using (var ctx = GetExplorerContext())
            {
                ctx.Database.Log = SQLLog;

                var sc = ctx.Scope.AsNoTracking().Where(s => s.IsRemoved == false)
                   .Select(s => new { ScopeId = s.ScopeId, ScopeName = s.ScopeName, Url = s.FullPath }).ToList();
                var aIds = sc.Select(l => l.ScopeId);

                var list = ctx.ManagedRecord.AsNoTracking().Where(m => m.Id > beginIndex && m.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item && aIds.Contains(m.ScopeId))
                    .GroupBy(c => c.ScopeId)
                    .Select(j => new { ScopeId = j.Key, Count = j.Count() }).ToList();
                if (beginIndex > 0)
                {
                    return (from l in list
                            join s in sc on l.ScopeId equals s.ScopeId
                            select new { ScopeName = s.ScopeName, Count = l.Count, Url = s.Url, ScopeId = s.ScopeId })
                    .Select(g => new RMSiteCollectionSize() { Size = g.Count, Title = g.ScopeName, SiteUrl = g.Url, ScopeId = g.ScopeId }).OrderByDescending(g => g.Size).ToList();
                }
                else
                {
                    return (from l in list
                            join s in sc on l.ScopeId equals s.ScopeId
                            select new { ScopeName = s.ScopeName, Count = l.Count, Url = s.Url, ScopeId = s.ScopeId })
                    .Select(g => new RMSiteCollectionSize() { Size = g.Count, Title = g.ScopeName, SiteUrl = g.Url, ScopeId = g.ScopeId }).OrderByDescending(g => g.Size).Take(10).ToList();
                }



                //list.Where(l => aIds.Contains(l.ScopeId))
                //var dataBase = (from m in ctx.ManagedRecord
                //                    //join s in ctx.Scope on m.ScopeId equals s.ScopeId
                //                where m.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item //&& s.IsRemoved == false
                //                select new { m.ScopeId, m.FullPath }).GroupBy(d => d.ScopeId).Select(d => new { date = d.Count(), url = d.First().FullPath }).Select(g => new RMSiteCollectionSize() { Size = g.date, SiteUrl = g.url }).OrderByDescending(g => g.Size);
                //var temp = dataBase.ToList();
                //var temp2 = dataBase.AsEnumerable();
                //return new List<RMSiteCollectionSize>();
                //return temp2.Select(g => new RMSiteCollectionSize() { Size = g.Count(), SiteUrl = g.First().FullPath }).OrderByDescending(g => g.Size).Take(10).ToList();
            }
        }
        public void UpdateRecordUniqueId(Guid siteId, Guid spObjectId, string uniqueId)
        {
            using (var ctx = GetExplorerContext())
            {
                ctx.ManagedRecord.Where(m => m.ScopeId == siteId && m.NodeId == spObjectId).Update(o => new RMManagedRecord() { RecordsId = uniqueId });
            }
        }
        public List<RMTermUsage> GetBoardTermUsageTop10Data(int beginIndex)
        {
            using (var ctx = GetExplorerContext())
            {

                ctx.Database.Log = SQLLog;
                var sc = ctx.Scope.AsNoTracking().Where(s => s.IsRemoved == false)
                   .Select(s => new { ScopeId = s.ScopeId, ScopeName = s.ScopeName, Url = s.FullPath }).ToList();
                var scopeIds = sc.Select(l => l.ScopeId);

                var baseResult = ctx.ManagedRecord.AsNoTracking().Where(m => m.Id > beginIndex && m.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item && m.TermId != Guid.Empty && scopeIds.Contains(m.ScopeId)).Select(m => m.TermId).GroupBy(c => c).Select(j => new { TermId = j.Key, Count = j.Count() }).ToList();

                //var baseResult = (from m in ctx.ManagedRecord.AsNoTracking() where ids.Contains(m.Id) select new {  m.TermId }).GroupBy(c => c.TermId).Select(j => new { TermId = j.Key, Count = j.Count() }).ToList();
                if (beginIndex > 0)
                {
                    return baseResult.Select(g => new RMTermUsage() { Size = g.Count, TermId = g.TermId }).OrderByDescending(g => g.Size).ToList();
                }
                else
                {
                    return baseResult.Select(g => new RMTermUsage() { Size = g.Count, TermId = g.TermId }).OrderByDescending(g => g.Size).Take(10).ToList();
                }

                //return (from l in baseResult
                //        join s in sc on l.ScopeId equals s.ScopeId
                //        select new { TermName = s.ScopeName, Count = l.Count, Url = s.Url })
                //    .Select(g => new RMTermUsage() { Size = g.Count,  TermName = g.TermName, SiteUrl = g.Url }).OrderByDescending(g => g.Size).Take(10).ToList();

                //return (from m in ctx.ManagedRecord
                //        join s in ctx.Scope on m.ScopeId equals s.ScopeId
                //        where s.IsRemoved == false && m.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item && m.TermId != Guid.Empty
                //        select new { m.ScopeId, m.TermId, m.TermName }).GroupBy(d => d.TermId).AsEnumerable()
                //        .Select(g => new RMTermUsage() { Size = g.Count(), TermName = g.First().TermName, TermId = g.First().TermId }).OrderByDescending(g => g.Size).Take(10).ToList();
                //return new List<RMTermUsage>();
            }
        }

        public List<RMDataOfDay> GetBoardCreatedRecords(int mstartIndex, int dstartIndex, int archiveIndex)
        {
            using (var ctx = GetExplorerContext())
            {
                if (mstartIndex > 0 || archiveIndex > 0)
                {
                    return (from m in ctx.ManagedRecord.AsNoTracking()
                            where m.Id > mstartIndex &&
                            m.NodeType == (int)NodeLevel.Item
                            select new { m.TimeCreated1 })
                        .Concat(from a in ctx.ArchivedRecord.AsNoTracking()
                                where a.Id > archiveIndex &&
                                a.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item && a.ManualAdd == 1
                                select new { a.TimeCreated1 })
                        .AsEnumerable()
                        .GroupBy(d => d.TimeCreated1.ToString("d"))
                        .Select(g => new RMDataOfDay() { Created = g.Count(), Timestamp = g.Key, Dater = Convert.ToDateTime(g.Key).Ticks }).ToList();
                }
                else
                {
                    return (from m in ctx.ManagedRecord.AsNoTracking()
                            where m.NodeType == (int)NodeLevel.Item
                            select new { m.TimeCreated1 })
                        .Concat(from a in ctx.ArchivedRecord.AsNoTracking()
                                where a.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item
                                select new { a.TimeCreated1 })
                        .Concat(from d in ctx.DeletedRecord.AsNoTracking()
                                where d.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item
                                select new { d.TimeCreated1 })
                        .AsEnumerable()
                        .GroupBy(d => d.TimeCreated1.ToString("d"))
                        .Select(g => new RMDataOfDay() { Created = g.Count(), Timestamp = g.Key, Dater = Convert.ToDateTime(g.Key).Ticks }).ToList();
                }

            }
        }



        public List<RMDataOfDay> GetBoardDestroyedRecords(int beginIndex)
        {
            using (var ctx = GetExplorerContext())
            {

                return (from m in ctx.ArchivedRecord.AsNoTracking()
                        where m.Id > beginIndex &&
                        m.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item
                        select new { m.DestroyedTime1 }).AsEnumerable()
                        .GroupBy(d => d.DestroyedTime1.ToString("d"))
                        .Select(g => new RMDataOfDay() { Destroyed = g.Count(), Timestamp = g.Key, Dater = Convert.ToDateTime(g.Key).Ticks }).ToList();
            }
        }

        public List<RMBaseRecord> GetRemovedRecords(int aIndex, int dIndex)
        {
            using (var ctx = GetExplorerContext())
            {
                return (from a in ctx.ArchivedRecord.AsNoTracking()
                        where a.Id > aIndex &&
                        a.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item && a.ManualAdd == 0
                        select new { a.ScopeId, a.TermId, a.TimeCreated1 })
                .Concat(from d in ctx.DeletedRecord.AsNoTracking()
                        where d.Id > dIndex &&
                        d.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item
                        select new { d.ScopeId, d.TermId, d.TimeCreated1 })
                        .Select(m => new RMBaseRecord()
                        {
                            ScopeId = m.ScopeId,
                            TermId = m.TermId,
                            TimeCreated1 = m.TimeCreated1
                        }).ToList();
            }

        }

        public void UpdateArchivedManualAddStatus()
        {
            using (var ctx = GetExplorerContext())
            {
                ctx.ArchivedRecord.Where(m => m.ManualAdd == 1).Update(a => new RMArchivedRecord() { ManualAdd = 0 });
            }
        }

        public long GetRecordsCount(bool destroyed)
        {
            using (var ctx = GetExplorerContext())
            {
                if (destroyed)
                {
                    return ctx.ArchivedRecord.Where(m => m.NodeType == (int)NodeLevel.Item).Count();
                }

                return ctx.ManagedRecord.Where(m => m.NodeType == (int)NodeLevel.Item).Count();
            }
        }

        public bool UpdateRecordOwner(Guid scopeId, Guid nodeId, string owners)
        {
            using (var ctx = GetExplorerContext())
            {
                int r = ctx.ManagedRecord.Where(m => m.ScopeId == scopeId && m.NodeId == nodeId).Update(m => new RMManagedRecord() { RecordOwner = owners });
                return r >= 0;
            }
        }

        public bool UpdateRecordOwnerForFS(Guid nodeId, string owners)
        {
            using (var ctx = GetExplorerContext())
            {
                int r = ctx.ManagedRecord.Where(m => m.NodeId == nodeId).Update(m => new RMManagedRecord() { RecordOwner = owners });
                return r >= 0;
            }
        }


        private string ConvertToShortTime(long ticks)
        {
            var time = new DateTime(ticks);
            return time.ToString("d");
        }
        private long ConvertDateTimeToTicks(string timeString)
        {
            DateTime time = Convert.ToDateTime(timeString);
            return time.Ticks;
        }

        public bool IsRecordExist(Guid scopeId, Guid webId, Guid listId, int itemId)
        {
            using (var ctx = GetExplorerContext())
            {

                return ctx.ManagedRecord.AsQueryable().Any(m => m.ScopeId == scopeId && m.WebId == webId && m.ListId == listId && m.ItemRowId == itemId);
            }
        }

        //public bool IsRecordExistInArchived(Guid scopeId, Guid webId, Guid listId, int itemId)
        //{
        //    using (var ctx = GetExplorerContext())
        //    {

        //        return ctx.ArchivedRecord.AsQueryable().Any(m => m.ScopeId == scopeId && m.WebId == webId && m.ListId == listId && m.ItemRowId == itemId);
        //    }
        //}

        public bool CheckDisposalHold(Guid scopeId, Guid nodeId, long ticks)
        {
            using (var ctx = GetExplorerContext())
            {

                var currentStatus = ctx.ManagedRecord.AsQueryable().Where(m => m.ScopeId == scopeId && m.NodeId == nodeId).Select(m => m.HoldStatus).FirstOrDefault();
                //REC-4046 若当前Hold状态为True, Check下是否存在Run Report时Hold已经释放的可能;
                if (currentStatus)
                {
                    var recordId = ctx.ManagedRecord.AsQueryable().Where(m => m.ScopeId == scopeId && m.NodeId == nodeId).Select(m => m.Id).FirstOrDefault();
                    var releaseTime = ctx.Alliance.AsQueryable().Where(m => m.RecordsId == recordId).Select(m => m.HoldReleaseTime).FirstOrDefault();
                    if (ticks > releaseTime)
                    {
                        currentStatus = false;
                    }
                }
                return currentStatus;
            }
        }
        public List<RMBaseRecord> GetRecordsByTerms(Guid scopeId, List<Guid> termIds, long ticks)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(m => termIds.Any(TermId => m.TermId == TermId) && m.ScopeId == scopeId && m.ItemRowId != 0 && m.CollectionTime < ticks).ToList<RMBaseRecord>();
            }
        }

        #region BoardIndex

        public RMBoardIndex GetBoardIndex(SourceFlag sFlag)
        {
            var flag = (int)sFlag;
            using (var ctx = GetNewContext())
            {
                return ctx.BoardIndex.Where(b => b.SourceFlag == flag).FirstOrDefault();
            }
        }

        public void UpdateBoardIndex(SourceFlag sFlag)
        {
            int mIndex = 0, aIndex = 0, dIndex = 0;
            var flag = (int)sFlag;
            using (var ctx = GetExplorerContext())
            {
                bool needUpdate = false;
                if (ctx.ManagedRecord.Any(b => b.SourceFlag == flag))
                {
                    needUpdate = true;
                    mIndex = ctx.ManagedRecord.Where(b => b.SourceFlag == flag).Max(m => m.Id);
                }
                if (ctx.ArchivedRecord.Any(b => b.SourceFlag == flag))
                {
                    needUpdate = true;
                    aIndex = ctx.ArchivedRecord.Where(b => b.SourceFlag == flag).Max(m => m.Id);
                }
                if (ctx.DeletedRecord.Any(b => b.SourceFlag == flag))
                {
                    needUpdate = true;
                    dIndex = ctx.DeletedRecord.Where(b => b.SourceFlag == flag).Max(m => m.Id);
                }
                if (!needUpdate)
                {
                    return;
                }
            }
            using (var ctx = GetNewContext())
            {
                var utcNow = DateTime.UtcNow.Ticks;
                if (!ctx.BoardIndex.Any(b => b.SourceFlag == flag))
                {
                    ctx.BoardIndex.Add(new RMBoardIndex()
                    {
                        ArchivedId = aIndex,
                        DeletedId = dIndex,
                        ManagedId = mIndex,
                        SourceFlag = flag,
                        CollectionTime = utcNow

                    });
                    ctx.SaveChanges();
                }
                else
                {
                    ctx.BoardIndex.Where(b => b.SourceFlag == flag).Update(b => new RMBoardIndex() { SourceFlag = flag, ArchivedId = aIndex, ManagedId = mIndex, DeletedId = dIndex, CollectionTime = utcNow });
                }
            }
        }

        #endregion




        private ExplorerDbContext GetExplorerContext()
        {
            return new ExplorerDbContext();
        }
        private void SQLLog(string sql)
        {
            //logger.Debug(sql);
        }

        public RMBaseRecord GetRecordByConnectioIDNodeId(Guid fsGroupId, Guid nodeId)
        {
            throw new NotImplementedException();
        }
    }
}
