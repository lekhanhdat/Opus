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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class DashboardTermUsageDao : BaseDao<RMDashboardTermUsage>, IDashboardTermUsageDao
    {
        public Task RemoveAllBySourceFlagAsync(SourceFlag sourceFlag)
        {
            return RemoveAllBySourceFlagAsync((int)sourceFlag);
        }

        public Task RemoveAllBySourceFlagAsync(int sourceFlag)
        {
            return BatchDeleteAsync(item => item.SourceFlag == sourceFlag);
        }

        public List<RMDashboardTermInfo> GetTermInfos()
        {
            using (var context = GetNewContext())
            {
                var query = from Terms in context.Terms
                            join TermSetMemberships in context.TermSetMemberships
                            on Terms.Id equals TermSetMemberships.TermId
                            join TermSets in context.TermSets
                            on TermSetMemberships.TermSetId equals TermSets.Id
                            join TermGroups in context.TermGruops
                            on TermSets.TermGroupId equals TermGroups.UniqueId
                            where !Terms.IsRemoved && !TermSetMemberships.IsRemoved
                            select new RMDashboardTermInfo
                            {
                                TermId = Terms.Id,
                                TermUniqueId = Terms.UniqueId.ToString(),
                                TermName = Terms.Name,
                                TermSetId = TermSets.UniqueId.ToString(),
                                TermSetName = TermSets.Name,
                                TermGroupId = TermGroups.UniqueId.ToString(),
                                TermGroupName = TermGroups.Name,
                                TermPath = TermSetMemberships.Path,
                                IsBreakInherit = Terms.BreakInheritFromParent,
                                IsApplyRule = !string.IsNullOrEmpty(Terms.RuleInfo)
                            };
                return query.ToList();
            }
        }
        

        public List<RMDashboardTermUsage> GetTermUsagesBySourceFlag(int sourceFlag)
        {
            using(var context = GetNewContext())
            {
                return context.DashboardTermUsage.AsNoTracking().Where(item => item.SourceFlag == sourceFlag).OrderByDescending(item => item.Active).Take(10).ToList();
            }
        }

        //public List<RMDashboardTermUsage> GetTermUsages(UsageTermQueryParam param, out int totalCount)
        //{
        //    using var context = GetNewContext();
        //    var query = context.DashboardTermUsage.Where(item => item.SourceFlag == param.SourceFlag);
        //    totalCount = query.Count();
        //    return query.OrderByDescending(item => item.Active).Skip(param.PageIndex * param.PageSize).Take(param.PageSize).ToList();
        //}

    }
}
