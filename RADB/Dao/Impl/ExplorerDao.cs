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
using System.Linq.Expressions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Model;
using Z.EntityFramework.Plus;
using System.Data;
using AvePoint.RA.DB.FullTextIndex;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.DB.Dao.Impl
{

    public class ExplorerDao : IExplorerDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExplorerDao));

        #region test code
        //internal void LogSql(string sql)
        //{
        //    logger.Debug(sql);
        //}
        #endregion
        public void AddDataToDestroyed(RMArchivedRecord rec)
        {
            using (var ctx = GetExplorerContext())
            {
                var exist = ctx.ArchivedRecord.Any(d => d.ScopeId == rec.ScopeId && d.NodeId == rec.NodeId);
                if (!exist)
                {
                    ctx.ArchivedRecord.Add(rec);
                    ctx.SaveChanges();
                }

            }
        }

        #region For Test
        /// <summary>
        /// 批量插入记录, N条commit一次
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(List<RMManagedRecord> list)
        {
            using (var ctx = GetExplorerContext())
            {
                ctx.ManagedRecord.AddRange(list);
                ctx.SaveChanges();
            }
        }
        /// <summary>
        /// 测试SQL直连REcords, Search
        /// </summary>
        /// <returns></returns>
        //public List<RMManagedRecord> TestSearchSQL()
        //{
        //    List<RMManagedRecord> result = new List<RMManagedRecord>();
        //    SqlConnection conn = null;
        //    SqlDataReader reader = null;
        //    int total = 0;
        //    DateTime begin = DateTime.Now;
        //    try
        //    {
        //        //conn = new SqlConnection(RMDBSetting.ConnectionDatabaseString);
        //        conn.Open();
        //        //SqlCommand cmd = new SqlCommand("select count(id) as num from RMManagedRecords", conn);
        //        //reader = cmd.ExecuteReader();
        //        //if (reader.Read())
        //        //{
        //        //    total = (int)reader["num"];
        //        //}
        //        //reader.Close();
        //        //SqlCommand cmd = new SqlCommand("select * from (select Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName, ROW_NUMBER() OVER(Order by Id) AS RowId  from RMManagedRecords as r where r.HOldStatus = 0 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0') as b where RowId between 900 and 909", conn);
        //        // cmd = new SqlCommand("select top 10 * from RMManagedRecords as r where r.HOldStatus = 1 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0'", conn);

        //        SqlCommand cmd = new SqlCommand("select * from (select *, ROW_NUMBER() OVER(Order by Id) AS RowId  from RMManagedRecords as r where r.HOldStatus = 1 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0' and contains(FullPath, 'Em2XaVXYGUvC')) as b where RowId between 0 and 10", conn);
        //        reader = cmd.ExecuteReader();
        //        while (reader.NextResult())
        //        {
        //            RMManagedRecord rd = new RMManagedRecord();
        //            rd.Id = (int)reader["Id"];
        //            rd.ScopeId = (Guid)reader["ScopeId"];
        //            rd.RecordsId = (string)reader["RecordsId"];
        //            rd.HoldStatus = (bool)reader["HoldStatus"];
        //            rd.LeafName = (string)reader["LeafName"];
        //            rd.MetaInfo = (string)reader["MetaInfo"];
        //            rd.NodeType = (int)reader["NodeType"];
        //            rd.RecordsId = (string)reader["RecordsId"];
        //            //rd.RuleName = (string)reader["RuleName"];
        //            result.Add(rd);
        //        }
        //        DateTime end = DateTime.Now;
        //        logger.Info("SQL time begin {0}, end {1}, total:{2}, spend ms: {3}", begin, end, total, (end.Ticks - begin.Ticks) / 10000);
        //        //SqlDataAdapter ada = new SqlDataAdapter("select top 15 * from RMManagedRecords where HOldStatus = 1 and TermId = '1c26d02c-f102-44da-9cac-96996acf98e0'", conn);
        //        //DataSet ds = new DataSet();
        //        //ada.Fill(ds); 
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn(e.Message, e);
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            reader.Close();
        //        }
        //        if (conn != null)
        //        {
        //            conn.Close();
        //        }
        //    }
        //    return result;
        //}
        /// <summary>
        /// 测试EF Code First Search, 包含了FullTextIndex 关键字, Select部分字段方案.
        /// </summary>
        /// <returns></returns>
        public List<RMManagedRecord> TestSearchEF()
        {
            using (var ctx = GetExplorerContext())
            {
                logger.Info("Test full text search partition match and index volumn.");
                DateTime begin = DateTime.Now;
                int total = 0; //Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName
                //var text = FullTextSearchModelUtil.Contains("Em2XaVXYGUvC");   //精确匹配单词
                var text = FullTextSearchModelUtil.Contains("isabout(\"Em2XaV*\")");  //模糊匹配部分words or phrase, *只能加在尾部
                                                                                      // List<RMPartitionRecord> result1 = ctx.Database.SqlQuery<RMPartitionRecord>("select * from (select Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName, ROW_NUMBER() OVER(Order by Id) AS RowId  from RMManagedRecords as r where r.HOldStatus = 1 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0') as b where RowId between 33330 and 33340").ToList();
                List<RMManagedRecord> result = ctx.ManagedRecord.Where(a => a.HoldStatus == true && a.TermId == new Guid("1c26d02c-f102-44da-9cac-96996acf98e0")).OrderBy(m => m.Id).Paging(3330, 10, out total).ToList();
                logger.Debug("REsult count {0}", result.Count);
                DateTime end = DateTime.Now;
                logger.Info("EF time begin {0}, end {1}, total:{2}, spend ms: {3}", begin, end, total, (end.Ticks - begin.Ticks) / 10000);
                return result;
            }
            //TestSearchEF1();
            //TestSearchEF2();
            //TestSearchEF3();
            //TestSearchEF4();
            //return null;
        }
        public List<RMManagedRecord> TestSearchEF1()
        {
            using (var ctx = GetExplorerContext())
            {
                logger.Info("Test  partition columns records");
                DateTime begin = DateTime.Now;
                int total = 0;
                List<RMPartitionRecord> result = ctx.Database.SqlQuery<RMPartitionRecord>("select * from (select Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName, ROW_NUMBER() OVER(Order by Id) AS RowId  from RMManagedRecords as r where r.HOldStatus = 1 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0') as b where RowId between 33330 and 33340").ToList();
                // List<RMManagedRecord> result = ctx.ManagedRecord.Where(a => a.HoldStatus == true && a.TermId == new Guid("1c26d02c-f102-44da-9cac-96996acf98e0") && (a.LeafName + a.FullPath).Contains(text)).OrderBy(m => m.Id).Paging(1, 10, out total).ToList();
                logger.Debug("REsult count {0}", result.Count);
                DateTime end = DateTime.Now;
                logger.Info("EF time begin {0}, end {1}, total:{2}, spend ms: {3}", begin, end, total, (end.Ticks - begin.Ticks) / 10000);
                return null;
            }
        }
        public List<RMManagedRecord> TestSearchEF2()
        {
            using (var ctx = GetExplorerContext())
            {
                logger.Info("Test one index column and one none-index column");
                DateTime begin = DateTime.Now;
                int total = 0; //Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName
                //var text = FullTextSearchModelUtil.Contains("Em2XaVXYGUvC");   //精确匹配单词
                //var text = FullTextSearchModelUtil.Contains("isabout(\"Em2XaV*\")");  //模糊匹配部分words or phrase, *只能加在尾部
                //List<RMPartitionRecord> result1 = ctx.Database.SqlQuery<RMPartitionRecord>("select * from (select Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName, ROW_NUMBER() OVER(Order by Id) AS RowId  from RMManagedRecords as r where r.HOldStatus = 1 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0') as b where RowId between 33330 and 33340").ToList();
                List<RMManagedRecord> result = ctx.ManagedRecord.Where(a => a.HoldStatus == true).OrderBy(m => m.Id).Paging(1, 10, out total).ToList();
                logger.Debug("REsult count {0}", result.Count);
                DateTime end = DateTime.Now;
                logger.Info("EF time begin {0}, end {1}, total:{2}, spend ms: {3}", begin, end, total, (end.Ticks - begin.Ticks) / 10000);
                return null;
            }
        }
        public List<RMManagedRecord> TestSearchEF3()
        {
            using (var ctx = GetExplorerContext())
            {
                logger.Info("Test one index column and one <> column");
                DateTime begin = DateTime.Now;
                int total = 0; //Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName
                               //var text = FullTextSearchModelUtil.Contains("Em2XaVXYGUvC");   //精确匹配单词
                               //var text = FullTextSearchModelUtil.Contains("isabout(\"Em2XaV*\")");  //模糊匹配部分words or phrase, *只能加在尾部
                               //List<RMPartitionRecord> result1 = ctx.Database.SqlQuery<RMPartitionRecord>("select * from (select Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName, ROW_NUMBER() OVER(Order by Id) AS RowId  from RMManagedRecords as r where r.HOldStatus = 1 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0') as b where RowId between 33330 and 33340").ToList();
                               // List<RMManagedRecord> result = ctx.ManagedRecord.Where(a => a.HoldStatus == true && a.TermId == new Guid("1c26d02c-f102-44da-9cac-96996acf98e0") && (a.LeafName + a.FullPath).Contains(text)).OrderBy(m => m.Id).Paging(1, 10, out total).ToList();
                List<RMManagedRecord> result = ctx.ManagedRecord.Where(a => a.TermId == new Guid("1c26d02c-f102-44da-9cac-96996acf98e0")).OrderBy(m => m.Id).Paging(1, 10, out total).ToList();
                logger.Debug("REsult count {0}", result.Count);
                DateTime end = DateTime.Now;
                logger.Info("EF time begin {0}, end {1}, total:{2}, spend ms: {3}", begin, end, total, (end.Ticks - begin.Ticks) / 10000);
                return null;
            }
        }
        public List<RMManagedRecord> TestSearchEF4()
        {
            using (var ctx = GetExplorerContext())
            {
                logger.Info("Test one none-index column column");
                DateTime begin = DateTime.Now;
                int total = 0; //Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName
                //var text = FullTextSearchModelUtil.Contains("Em2XaVXYGUvC");   //精确匹配单词
                //var text = FullTextSearchModelUtil.Contains("isabout(\"Em2XaV*\")");  //模糊匹配部分words or phrase, *只能加在尾部
                //List<RMPartitionRecord> result1 = ctx.Database.SqlQuery<RMPartitionRecord>("select * from (select Id, ScopeId,RecordsId, HoldStatus, LeafName, MetaInfo, NodeType, RuleName, ROW_NUMBER() OVER(Order by Id) AS RowId  from RMManagedRecords as r where r.HOldStatus = 1 and r.TermId = '1c26d02c-f102-44da-9cac-96996acf98e0') as b where RowId between 33330 and 33340").ToList();
                List<RMManagedRecord> result = ctx.ManagedRecord.Where(a => a.TermName == "term08").OrderBy(m => m.Id).Paging(1, 10, out total).ToList();
                logger.Debug("REsult count {0}", result.Count);
                DateTime end = DateTime.Now;
                logger.Info("EF time begin {0}, end {1}, total:{2}, spend ms: {3}", begin, end, total, (end.Ticks - begin.Ticks) / 10000);
                return null;
            }
        }
        #endregion


        class NameAndId
        {
            internal string Name;
            internal Guid Id;
        }

        public void ChangeTerm(List<int> ids, Guid termId)
        {
           
            using (var context = GetExplorerContext())
            {
                context.ManagedRecord.Where(m => ids.Any(i => m.Id == i))
                    .Update(m => new RMManagedRecord()
                    {
                        TermId = term.Id,
                        TermName = term.Name,
                        RuleId = Guid.Empty,
                        DisposalDueDate = I18NEntity.GetString("RM_JS_JM_EndTimePending"),
                        RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending"),
                    });
            }
        }

      
        private string BuildInClause(int[] status)
        {
            if (status == null || status.Length == 0)
            {
                throw new InvalidOperationException();
            }
            string[] value = (from u in status select ((int)u).ToString()).ToArray();

            return string.Join(",", value);
        }
        public void DeleteRecord(bool destroyed, int id)
        {
            using (var ctx = GetExplorerContext())
            {
                string sql = null;
                if (destroyed)
                {
                    //sql = "delete from RMManagedRecords where DestroyedAction = 1 and Id = @id";
                    ctx.ArchivedRecord.Where(m => m.Id == id).Delete();
                }
                else
                {
                    //sql = "delete from RMManagedRecords where Id = @id";
                    ctx.ManagedRecord.Where(m => m.Id == id).Delete();
                }
                //ctx.Database.ExecuteSqlCommand(sql, new SqlParameter("id", id));
            }
        }



        public int QueryDataGetTotal(bool isArchived, string keyWord, Expression<Func<RMBaseRecord, bool>> whereLambda = null)
        {
            IQueryable<RMBaseRecord> query = null;
            using (var ctx = GetExplorerContext())
            {
                ctx.Database.Log = SQLLog;//TEST
                //query destroyed data
                //if (whereLambda != null)
                //{
                //    logger.Info("explorer query data lamnda is:{0}", whereLambda.ToString());
                //}
                if (isArchived)
                {
                    if (whereLambda != null)
                    {
                        query = ctx.ArchivedRecord.AsNoTracking().Where(d => (d.NodeType == (int)NodeLevel.Item || d.NodeType == (int)NodeLevel.FSFile || d.NodeType == (int)NodeLevel.FSFolder)).Where(whereLambda).OrderByDescending(d => d.Id);
                    }
                    else
                    {
                        query = ctx.ArchivedRecord.AsNoTracking().Where(d => (d.NodeType == (int)NodeLevel.Item || d.NodeType == (int)NodeLevel.FSFile || d.NodeType == (int)NodeLevel.FSFolder)).OrderByDescending(d => d.Id);
                    }
                }
                else
                {
                    string containsKey = DB.FullTextIndex.FullTextSearchModelUtil.GetContainsKeySplitBySpace(keyWord);
                    logger.Debug("Search key words after analyze is {0}", containsKey);
                    query = GetQueryExpression(ctx, isArchived, containsKey, keyWord, whereLambda);
                }

                int totalRecord = query.Count();
                //var ids = query.Select(s => s.Id).Paging(pageIndex, pageSize, out totalRecord);
                //return (from m in ctx.ManagedRecord.AsNoTracking() where ids.Contains(m.Id) select m).ToList<RMBaseRecord>();
                return totalRecord;
            }
        }
        public List<RMBaseRecord> QueryDataWithoutTotal(bool isArchived, string keyWord, int pageIndex, int pageSize, out bool hasNext, Expression<Func<RMBaseRecord, bool>> whereLambda = null)
        {
            IQueryable<RMBaseRecord> query = null;
            using (var ctx = GetExplorerContext())
            {
                ctx.Database.Log = SQLLog;//TEST
                //query destroyed data
                //if (whereLambda != null)
                //{
                //    logger.Info("explorer query data lamnda is:{0}", whereLambda.ToString());
                //}
                if (isArchived)
                {
                    if (whereLambda != null)
                    {
                        query = ctx.ArchivedRecord.AsNoTracking().Where(d => d.NodeType == (int)NodeLevel.Item).Where(whereLambda).OrderByDescending(d => d.Id);
                    }
                    else
                    {
                        query = ctx.ArchivedRecord.AsNoTracking().Where(d => d.NodeType == (int)NodeLevel.Item).OrderByDescending(d => d.Id);
                    }
                }
                else
                {
                    string containsKey = DB.FullTextIndex.FullTextSearchModelUtil.GetContainsKeySplitBySpace(keyWord);
                    logger.Debug("Search key words after analyze is {0}", containsKey);
                    query = GetQueryExpression(ctx, isArchived, containsKey, keyWord, whereLambda);
                }

                var ids = query.Select(s => s.Id).PagingWithNextFirst(pageIndex, pageSize);
                var result = (from m in ctx.ManagedRecord.AsNoTracking() where ids.Contains(m.Id) orderby m.Id descending select m).ToList<RMBaseRecord>();
                hasNext = result.Count > pageSize;
                return result.Take(pageSize).ToList();
            }
        }
        private IQueryable<RMBaseRecord> GetQueryExpression(ExplorerDbContext ctx, bool isArchiver, string fulltextkeyWord, string keyWord, Expression<Func<RMBaseRecord, bool>> whereLambda)
        {
            IQueryable<RMBaseRecord> query = null;
            if (whereLambda != null)    //有Filter
            {
                if (fulltextkeyWord.IsNullOrEmpty())
                {
                    query = ctx.ManagedRecord.AsNoTracking().Where(whereLambda).Where(m => (m.NodeType == (int)NodeLevel.Item || m.NodeType == (int)NodeLevel.FSFile || m.NodeType == (int)NodeLevel.FSFolder)).OrderByDescending(d => d.Id);
                }
                else  //有Keywords模糊搜索
                {
                    if (DB.FullTextIndex.FullTextIndexInitializer.IndexExists)  //开启了Fulltextindex
                    {
                        query = ctx.ManagedRecord.AsNoTracking().Where(whereLambda).Where(m => (m.NodeType == (int)NodeLevel.Item || m.NodeType == (int)NodeLevel.FSFile || m.NodeType == (int)NodeLevel.FSFolder) && (m.LeafName + m.RecordsId).Contains(fulltextkeyWord)).OrderByDescending(d => d.Id);
                    }
                    else
                    {
                        query = ctx.ManagedRecord.AsNoTracking().Where(whereLambda).Where(m => (m.NodeType == (int)NodeLevel.Item || m.NodeType == (int)NodeLevel.FSFile || m.NodeType == (int)NodeLevel.FSFolder) && (m.LeafName.Contains(keyWord) || m.RecordsId.Contains(keyWord))).OrderByDescending(d => d.Id);
                    }
                }
            }
            else   //没有Filter
            {
                if (fulltextkeyWord.IsNullOrEmpty())
                {
                    query = ctx.ManagedRecord.AsNoTracking().Where(d => (d.NodeType == (int)NodeLevel.Item || d.NodeType == (int)NodeLevel.FSFile || d.NodeType == (int)NodeLevel.FSFolder)).OrderByDescending(d => d.Id);
                }
                else
                {   //有Keywords模糊搜索
                    if (DB.FullTextIndex.FullTextIndexInitializer.IndexExists)//开启了Fulltextindex
                    {
                        query = ctx.ManagedRecord.AsNoTracking().Where(m => (m.NodeType == (int)NodeLevel.Item || m.NodeType == (int)NodeLevel.FSFile || m.NodeType == (int)NodeLevel.FSFolder) && (m.LeafName + m.RecordsId).Contains(fulltextkeyWord)).OrderByDescending(d => d.Id);
                    }
                    else
                    {
                        query = ctx.ManagedRecord.AsNoTracking().Where(m => (m.NodeType == (int)NodeLevel.Item || m.NodeType == (int)NodeLevel.FSFile || m.NodeType == (int)NodeLevel.FSFolder) && (m.LeafName.Contains(keyWord) || m.RecordsId.Contains(keyWord))).OrderByDescending(d => d.Id);
                    }
                }
            }
            return query;
        }

        public List<RMBaseRecord> GetRecordByAlliance(List<int> recordIds)
        {
            //IQueryable<RMBaseRecord> query = null;
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(m => recordIds.Any(id => id == m.Id)).AsQueryable().ToList<RMBaseRecord>();
            }

        }

        public List<RMRecordAlliance> GetRecordAllianceById(List<int> recordIds)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.Alliance.Where(m => recordIds.Any(id => id == m.RecordsId)).ToList();
            }
        }

        public Dictionary<Guid, int> TestUnion()
        {
            using (var ctx = GetExplorerContext())
            {

                var query = (from a in ctx.ManagedRecord
                             where a.LeafName.Contains("PNG")
                             select new RMBaseRecord() { LeafName = a.LeafName, TermId = a.TermId })
                       .Union
                       (from b in ctx.ArchivedRecord
                        where b.LifecycleStatus == 2
                        select new RMBaseRecord() { LeafName = b.LeafName, TermId = b.TermId })
                        .GroupBy(d => d.TermId).ToDictionary(z => z.Key, z => z.Count());
                return query;
            }

        }

        public void UpdateHoldSetting(List<int> ids, bool holdStatus, HoldSettingDto holdDto)
        {

            using (var ctx = GetExplorerContext())
            {
                if (holdDto != null)
                {
                    List<int> updateIds = new List<int>();
                    foreach (var id in ids)
                    {
                        if (ctx.Alliance.Any(a => a.RecordsId == id))
                        {
                            updateIds.Add(id);
                        }
                        else
                        {
                            ctx.Alliance.Add(new RMRecordAlliance()
                            {
                                RecordsId = id,
                                AllianceType = holdDto.AllianceType,
                                HoldId = holdDto.HoldId,
                                HoldReleaseTime = holdDto.ReleaseTime,
                                HoldBy = holdDto.HoldBy
                            });
                        }

                    }
                    ctx.SaveChanges();
                    if (updateIds.Count() > 0)
                    {
                        ctx.Alliance.Where(a => updateIds.Contains(a.RecordsId)).Update(o => new RMRecordAlliance() { HoldBy = holdDto.HoldBy, HoldReleaseTime = holdDto.ReleaseTime, HoldId = holdDto.HoldId });
                    }
                }
                ctx.ManagedRecord.Where(m => ids.Any(id => id == m.Id)).Update(m => new RMManagedRecord() { HoldStatus = holdStatus });
            }
        }

        public void UpdateHoldSetting(List<int> recordIds, List<RMRecordAlliance> Records)
        {
            using (var ctx = GetExplorerContext())
            {
                foreach (var re in Records)
                {
                    ctx.Alliance.Where(a => a.RecordsId == re.RecordsId).Update(a => new RMRecordAlliance() { HoldBy = re.HoldBy, HoldReleaseTime = re.HoldReleaseTime });
                }
            }
        }
        public void CancelHoldByRecords(List<int> recordsIds)
        {
            using (var ctx = GetExplorerContext())
            {
                ctx.Alliance.Where(a => recordsIds.Any(id => a.RecordsId == id)).Delete();
                ctx.ManagedRecord.Where(m => recordsIds.Any(id => id == m.Id)).Update(m => new RMManagedRecord() { HoldStatus = false });

            }
        }


        private ExplorerDbContext GetExplorerContext()
        {
            return new ExplorerDbContext();
        }

        private Core.RMDbContext GetRMDbContext()
        {
            return Core.RMDBContextManager.GetNewDBContext();
        }

        public void AddReocrdHistory(List<int> id, RecordHistoryXml xmlDto)
        {
            using (var ctx = GetExplorerContext())
            {

                var history = ctx.ManagedRecord.Where(s => id.Contains(s.Id)).Select(m => new { m.Id, m.RecordHistory }).ToList();
                foreach (var his in history)
                {
                    if (!string.IsNullOrEmpty(his.RecordHistory))
                    {
                        var old = XmlUtil.GetXmlObject<RecordHistoryXml>(his.RecordHistory);
                        old.HistoryList.AddRange(xmlDto.HistoryList);
                        var str = XmlUtil.GetXmlString(old);
                        ctx.ManagedRecord.Where(s => s.Id == his.Id).Update(m => new RMManagedRecord() { RecordHistory = str });
                    }
                    else
                    {
                        var str = XmlUtil.GetXmlString(xmlDto);
                        ctx.ManagedRecord.Where(s => s.Id == his.Id).Update(m => new RMManagedRecord() { RecordHistory = str });
                    }
                }
            }
        }


        public void UpdateCollectionTime(Guid id, long timeTicks)
        {
            using (var ctx = GetRMDbContext())
            {
                var scopeNode = ctx.Scope.Where(s => s.ScopeId == id).Update(m => new RMScope() { CollectionTime = timeTicks });
            }
        }

        public List<RMBaseRecord> GetRelatedRecords(Expression<Func<RMBaseRecord, bool>> whereLambda = null)
        {
            IQueryable<RMBaseRecord> query = null;
            using (var ctx = GetExplorerContext())
            {
                query = ctx.ManagedRecord.Where(whereLambda).OrderByDescending(d => d.Id);
                return query.ToList();
            }
        }
        public List<RMRecordAlliance> GetSettingHoldByRecordIds(List<int> ids)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.Alliance.Where(m => ids.Any(id => m.RecordsId == id)).ToList();
            }
        }

        public List<int> GetExpiredHold()
        {
            using (var ctx = GetExplorerContext())
            {
                var now = DateTime.UtcNow.Ticks;
                return ctx.Alliance.Where(m => m.HoldReleaseTime != 0 && m.HoldReleaseTime < now).Select(a => a.RecordsId).ToList();
            }
        }

        public int UpdateExpiredHold(List<int> ids)
        {
            logger.Info("Begin expire hold");
            using (var ctx = GetExplorerContext())
            {
                ctx.Alliance.Where(a => ids.Any(id => id == a.RecordsId)).Delete();
                return ctx.ManagedRecord.Where(m => ids.Any(id => m.Id == id)).Update(m => new RMManagedRecord() { HoldStatus = false });

            }
        }

        public RMBaseRecord GetRecordByUniqueId(Guid scopeId, Guid itemUniqueId)
        {
            using (var ctx = GetExplorerContext())
            {
                return ctx.ManagedRecord.Where(r => r.ScopeId == scopeId && r.ItemId == itemUniqueId).FirstOrDefault();
            }
        }


        private void SQLLog(string sql)
        {
            //logger.Debug(sql);
        }
       

        //public RMBaseRecord GetFSRootNode()
        //{
        //    using (var ctx = GetExplorerContext())
        //    {
        //        var rootNode = ctx.ManagedRecord.Where(r => r.NodeType == (int)FSTreeType.Root).FirstOrDefault();
        //        if (rootNode != null)
        //        {
        //            rootNode.subNodesCount = SubChildrenCount(rootNode.NodeId);
        //        }
        //        return rootNode;
        //    }
        //}

        //public List<RMBaseRecord> GetFSChildNodes(Guid parentId, int fsType)
        //{
        //    IQueryable<RMBaseRecord> query = null;
        //    using (var ctx = GetExplorerContext())
        //    {
        //        if (fsType != (int)FSTreeType.Folder)
        //        {
        //            query = ctx.ManagedRecord.Where(r => r.ParentId.Equals(parentId)).OrderBy(a => a.LeafName);
        //        }
        //        else
        //        {
        //            query = ctx.ManagedRecord.Where(r => r.ParentId.Equals(parentId) && r.NodeType != (int)FSTreeType.File).OrderBy(a => a.LeafName);
        //        }
        //        List< RMBaseRecord> list =  query.ToList();
        //        foreach (var item in list)
        //        {
        //            //if (item.NodeType != (int)FSTreeType.Folder)
        //            //{
        //                item.subNodesCount = SubChildrenCount(item.NodeId);
        //            //}
        //        }
        //        return list;
        //    }
        //}
        //public int SubChildrenCount(Guid nodeId)
        //{
        //    using (var ctx = GetExplorerContext())
        //    {
        //        return ctx.ManagedRecord.Where(r => r.ParentId.Equals(nodeId) && r.NodeType != (int)FSTreeType.File).Count();
        //    }
        //}
        //public RMManagedRecord GetFSConnGroupNode(Guid nodeId)
        //{
        //    using (var ctx = GetExplorerContext())
        //    {
        //        var node = ctx.ManagedRecord.Where(r => r.NodeId == nodeId).FirstOrDefault();
        //        if (node != null && node.NodeType != (int)FSTreeType.ConnGroup)
        //        {
        //            node =  GetFSConnGroupNode(node.ParentId);
        //        }
        //        return node;
        //    }
        //}
    }
}
