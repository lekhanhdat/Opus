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
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMTemplateRelationshipDao : BaseDao<RMTemplateRelationship>, IRMTemplateRelationshipDao
    {
        //public List<RMSuiteMembership> GetAllRMSuiteMemberships()
        //{
        //    return SharedDbContext.SuiteMembership.AsQueryable().ToList();
        //}
        public List<Guid> GetByParent(Guid parent, List<string> idPathList, int pageIndex, int pageCount, out int total)
        {
            //var query = SharedDbContext.TemplateRelationship.Where(o => o.Ancestor == parent && o.Distance == 1).Select(o => o.Descendant);
            //var parents = GetParents(parent); //由于存在add exsting的情况，导致一个节点可能有多个parent记录
            //if (parents.Count > 1 && ancestorList?.Count > 0) 
            //{
            //    var maxDistance = ancestorList.Count + 1;
            //    //只能load一层节点，不需要load exsting更底层的节点，所以这里做了处理，避免把多余的节点给load出来
            //    foreach (var ancestor in ancestorList) 
            //    {
            //        var distance = maxDistance;
            //        query = query.Intersect(SharedDbContext.TemplateRelationship.Where(o => ancestor == o.Ancestor && o.Distance == distance).Select(o => o.Descendant));
            //        maxDistance--;
            //    }
            //}
            using var context = GetNewContext();
            var query = BuildQuery(context, parent, idPathList);
            total = query.Count();
            var result = context.Template.Where(o => query.Contains(o.UniqueId)).OrderBy(o => o.Name).Select(o => o.UniqueId);
            return result.Skip(pageCount * (pageIndex - 1)).Take(pageCount).ToList();
        }

        public List<Guid> GetAllByParent(Guid parent, List<string> idPathList, List<TemplateType> subTypes = null)
        {
            var idPath = TemplateUtil.Convert2Path(idPathList);
            using var context = GetNewContext();
            var query = context.TemplateRelationship.Where(o => o.IdPath.StartsWith(idPath) && o.Distance == 1 && o.Ancestor == parent);
            if (subTypes != null && subTypes.Count > 0) query = query.Where(o => subTypes.Contains(o.TemplateType));
            return query.Select(o => o.Descendant).ToList();
        }

        private IQueryable<Guid> BuildQuery(RMDbContext context, Guid parent, List<string> idPathList)
        {
            var idPath = TemplateUtil.Convert2Path(idPathList);
            var query = context.TemplateRelationship.Where(o => o.IdPath.StartsWith(idPath) && o.Distance == 1 && o.Ancestor == parent).Select(o => o.Descendant);
            ////由于存在add exsting的情况，导致一个节点可能有多个parent记录
            //if (ancestorList?.Count > 0 && GetParents(parent).Count > 1)
            //{
            //    var maxDistance = ancestorList.Count + 1;
            //    //只能load一层节点，不需要load exsting更底层的节点，所以这里做了处理，避免把多余的节点给load出来
            //    foreach (var ancestor in ancestorList)
            //    {
            //        var distance = maxDistance;
            //        query = query.Intersect(SharedDbContext.TemplateRelationship.Where(o => ancestor == o.Ancestor && o.Distance == distance).Select(o => o.Descendant));
            //        maxDistance--;
            //    }
            //}

            return query;
        }

        public int GetAncesstorCount(List<string> idPathList, TemplateType templateType)
        {
            var idPath = TemplateUtil.Convert2Path(idPathList);
            using var context = GetNewContext();
            return context.TemplateRelationship.Count(o => o.IdPath == idPath && o.TemplateType == templateType);
        }
        /// <summary>
        /// return the total children count
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ancestorList">ancestor list of parent, doesn't include parent itself</param>
        /// <returns></returns>
        public int GetChildrenCount(Guid parent, List<string> idPathList)
        {
            using var context = GetNewContext();
            var query = BuildQuery(context, parent, idPathList);
            return query.Count();
        }

        public bool AddRelationships(List<RMTemplateRelationship> relationships)
        {
            using (var ctx = GetNewContext())
            {
                using (DbContextTransaction tran = ctx.Database.BeginTransaction())
                {
                    foreach(var entity in relationships)
                    {
                        if (!ctx.TemplateRelationship.Any(o => o.IdPath == entity.IdPath && o.Distance == entity.Distance))
                        {
                            ctx.TemplateRelationship.Add(entity);
                        }
                    }
                    tran.Commit();
                    return ctx.SaveChanges() > 0;
                }
            }
        }

        public Guid GetSuiteUniqueId(Guid rootTemplateUniqueId)
        {
            using var context = GetNewContext();
            var parentIds = context.TemplateRelationship.Where(o => o.Descendant == rootTemplateUniqueId && o.Distance == 1).Select(o => o.Ancestor);
            var suite = context.TemplateRelationship.FirstOrDefault(o => parentIds.Contains(o.Descendant) && o.Distance == 0 && o.TemplateType == TemplateType.Suite);
            return suite != null? suite.Ancestor :Guid.Empty;
        }

        public Guid GetStartTemplateUniqueId(Guid suiteUniqueId)
        {
            using var context = GetNewContext();
            var template = context.TemplateRelationship.FirstOrDefault(o => o.Ancestor == suiteUniqueId && o.Distance == 1);
            return template != null ? template.Descendant : Guid.Empty;
        }

        public bool UsedAsStartTemplate(Guid templateUniqueId)
        {
            return GetSuiteUniqueId(templateUniqueId) != Guid.Empty;
        }

        public bool HasStartTemplate(Guid suiteUniqueId, TemplateType templateType)
        {
            using var context = GetNewContext();
            var template = context.TemplateRelationship.FirstOrDefault(o => o.Ancestor == suiteUniqueId && o.Distance == 1 && o.TemplateType == templateType);
            return template != null;
        }

        public bool Exists(string ancestorIdPath, int templateId)
        {
            var path = ancestorIdPath + templateId.ToString() + TemplateUtil.IdPathSeprator;
            return Exists(path);
        }

        public bool Exists(string idPath)
        {
            using var context = GetNewContext();
            return context.TemplateRelationship.FirstOrDefault(o => o.IdPath == idPath) != null;
        }

        public List<string> GetAllPathBySuite(Guid suiteId)
        {

            using (var ctx = GetNewContext())
            {
                return ctx.TemplateRelationship.Where(a => a.IdPath.StartsWith(suiteId.ToString())).Select(a=>a.IdPath).ToList();
            }
        }

        //private List<Guid> GetParents(Guid descendant)
        //{
        //    return SharedDbContext.TemplateRelationship.Where(o => o.Descendant == descendant && o.Distance == 1).Select(o => o.Ancestor).ToList();
        //}
    }
}
