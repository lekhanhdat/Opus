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
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMManagedRecordRelatedDao : IRMManagedRecordRelatedDao
    {
        public List<Guid> GetRelatedRecords(Guid id)
        {
            using (var ctx = GetRMDbContext())
            {
                var relatedIds = ctx.ManagedRecordRelated.Where(r => r.CurrentRecordId1 == id).Select(r => r.RelatedRecordId1);
                //var records = ctx.ManagedRecord.Where(r => relatedIds.Contains(r.Id)).ToList();
                return relatedIds.ToList();
            }
        }
        //public List<RMBaseRecord> FSSearchRecords(int pageIndex, int pageSize, string value, List<int> exceptIds, out int totalRecord)
        //{
        //    using (var ctx = GetRMDbContext())
        //    {
        //        //ctx.Database.Log = SQLLog;
        //        Expression<Func<RMBaseRecord, bool>> lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord;
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            //ctx.Database.Log = LogSql;//TEST
        //            if (FullTextIndex.FullTextIndexInitializer.IndexExists)
        //            {
        //                string contains = FullTextIndex.FullTextSearchModelUtil.GetContainsKeySplitBySpace(value);
        //                lambda = s => (s.LeafName + s.RecordsId).Contains(contains) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.FSFile && !s.DeclareAsRecord;
        //            }
        //            else
        //            {
        //                lambda = s => (s.LeafName.Contains(value) || s.RecordsId.Contains(value)) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.FSFile && !s.DeclareAsRecord;
        //            }
        //        }
        //        var ids = ctx.ManagedRecord.Where(d => d.NodeType == (int)NodeLevel.FSFile).Where(lambda).OrderByDescending(d => d.Id).Paging(pageIndex, pageSize, out totalRecord).Select(s => s.Id);
        //        //!d.DeclareAsRecord此处在search的之后，直接过滤，在update的时候，仍需要添加后台验证
        //        return (from m in ctx.ManagedRecord where ids.Contains(m.Id) select m).ToList<RMBaseRecord>();
        //    }
        //}

        public void AddRelated(Guid currentId, Guid relatedId)
        {
            using (var ctx = GetRMDbContext())
            {
                var entity1 = ctx.ManagedRecordRelated.FirstOrDefault(r => r.CurrentRecordId1 == currentId && r.RelatedRecordId1 == relatedId);
                if (entity1 == null)
                {
                    ctx.ManagedRecordRelated.Add(new RMManagedRecordRelated() { CurrentRecordId1 = currentId, RelatedRecordId1 = relatedId});
                }

                var entity2 = ctx.ManagedRecordRelated.FirstOrDefault(r => r.CurrentRecordId1 == relatedId && r.RelatedRecordId1 == currentId);
                if (entity2 == null)
                {
                    ctx.ManagedRecordRelated.Add(new RMManagedRecordRelated() { CurrentRecordId1 = relatedId, RelatedRecordId1 = currentId });
                }
                ctx.SaveChanges();
            }
        }

        public void DeleteRelated(Guid currentId, Guid relatedId)
        {
            using (var ctx = GetRMDbContext())
            {
                var entity1 = ctx.ManagedRecordRelated.FirstOrDefault(r => r.CurrentRecordId1 == currentId && r.RelatedRecordId1 == relatedId);
                if (entity1 != null)
                {
                    ctx.ManagedRecordRelated.Remove(entity1);
                }

                var entity2 = ctx.ManagedRecordRelated.FirstOrDefault(r => r.CurrentRecordId1 == relatedId && r.RelatedRecordId1 == currentId);
                if (entity2 != null)
                {
                    ctx.ManagedRecordRelated.Remove(entity2);
                }
                ctx.SaveChanges();
            }
        }
        
        //public int FSSearchRecordsGetTotal(string value, List<int> exceptIds)
        //{
        //    int totalRecord = 0;
        //    using (var ctx = GetExplorerContext())
        //    {
        //        Expression<Func<RMBaseRecord, bool>> lambda = s => !exceptIds.Contains(s.Id) && !s.DeclareAsRecord;
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            //ctx.Database.Log = LogSql;//TEST
        //            if (FullTextIndex.FullTextIndexInitializer.IndexExists)
        //            {
        //                string contains = FullTextIndex.FullTextSearchModelUtil.GetContainsKeySplitBySpace(value);
        //                lambda = s => (s.LeafName + s.RecordsId).Contains(contains) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.FSFile && !s.DeclareAsRecord;
        //            }
        //            else
        //            {
        //                lambda = s => (s.LeafName.Contains(value) || s.RecordsId.Contains(value)) && !exceptIds.Contains(s.Id) && s.NodeType == (int)NodeLevel.FSFile && !s.DeclareAsRecord;
        //            }
        //        }
        //        totalRecord = ctx.ManagedRecord.Where(d => d.NodeType == (int)NodeLevel.FSFile).Where(lambda).Count();
        //    }
        //    return totalRecord;
        //}
        private Core.RMDbContext GetRMDbContext()
        {
            return Core.RMDBContextManager.GetNewDBContext();
        }


        /// <summary>
        /// 用于Import physical record from trim, 临时存储Realate关系
        /// </summary>
        /// <param name="relate"></param>
        public void AddImportTRIMRelate(RMManagedRecordRelated relate)
        {
            using(var context = GetRMDbContext())
            {
                context.ManagedRecordRelated.Add(relate);
                context.SaveChanges();
            }
        }

        public bool IsRelatedExist(string srcUniqueId, string relatedUniqueId)
        {
            using (var context = GetRMDbContext())
            {
                bool exist = context.ManagedRecordRelated.Any(a => a.SrcUniqueId == srcUniqueId && a.RelatedUniqueId == relatedUniqueId);
                return exist;
            }
        }

        public List<RMManagedRecordRelated> GetAll()
        {
            using (var context = GetRMDbContext())
            {
                List<RMManagedRecordRelated> all = context.ManagedRecordRelated.Where(a=>a.Type == 1).ToList();
                return all;
            }
        }
    }
}
