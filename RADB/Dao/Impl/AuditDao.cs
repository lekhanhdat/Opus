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
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class AuditDao : BaseDao<RMAudit>, IAuditDao
    {
        public List<RMAudit> FindAuditInfoByTimeInterval(int pageIndex, int pageSize, ref int dataCount, Expression<Func<RMAudit, bool>> whereLamdba)
        {
            using (var context = GetNewContext())
            {
                var list = context.Set<RMAudit>().AsNoTracking().Where<RMAudit>(whereLamdba);
                dataCount = list.Count();
                List<RMAudit> result = list.OrderByDescending(a => a.ExecuteOn).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList<RMAudit>();
                return result;
            }
        }
        public List<RMAudit> FindAuditInfoByFilterAndSort(int pageIndex, int pageSize, ref int dataCount, Expression<Func<RMAudit, bool>> whereLamdba, DisplayColumn orderColumn, bool? IsAscending)
        {
            using (var context = GetNewContext())
            {
                List<RMAudit> result = null;
                var query = context.Set<RMAudit>().AsNoTracking().Where<RMAudit>(whereLamdba);
                dataCount = query.Count();
                IOrderedQueryable<RMAudit> ordered;
                if (IsAscending != null)
                {
                    switch (orderColumn)
                    {
                        case DisplayColumn.Time:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.ExecuteOn) : query.OrderByDescending(item => item.ExecuteOn);
                            break;
                        case DisplayColumn.User:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.UserName) : query.OrderByDescending(item => item.UserName);
                            break;
                        case DisplayColumn.Role:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.Role) : query.OrderByDescending(item => item.Role);
                            break;
                        case DisplayColumn.Object:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.Object) : query.OrderByDescending(item => item.Object);
                            break;
                        case DisplayColumn.DocAveModule:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.Category) : query.OrderByDescending(item => item.Category);
                            break;
                        case DisplayColumn.Action:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.Action) : query.OrderByDescending(item => item.Action);
                            break;
                        case DisplayColumn.Status:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.Status) : query.OrderByDescending(item => item.Status);
                            break;
                        default:
                            ordered = IsAscending.Equals(true) ? query.OrderBy(item => item.ExecuteOn) : query.OrderByDescending(item => item.ExecuteOn);
                            break;
                    }
                }
                else
                {
                    ordered = query.OrderByDescending(a => a.ExecuteOn);
                }
                result = ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                return result;
            }
        }
        public List<RMAudit> FindAllAuditInfos()
        {
            using var context = GetNewContext();
            return context.Set<RMAudit>().AsNoTracking().ToList();
        }

        public List<long> FindAuditInfoByTimeIntervalAndGroupByTime(DateTime startTime, DateTime endTime)
        {
            using (var context = GetNewContext())
            {
                return context.Set<RMAudit>().Where(item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks).Select(item => item.ExecuteOn).ToList<long>();
            }
        }

        public Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByUser(DateTime startTime, DateTime endTime)
        {
            using var context = GetNewContext();
            return context.Set<RMAudit>()
                .Where(item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                .GroupBy(item => item.UserName)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByRole(DateTime startTime, DateTime endTime)
        {
            using var context = GetNewContext();
            return context.Set<RMAudit>()
                .Where(item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                .GroupBy(item => item.Role)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByModule(DateTime startTime, DateTime endTime)
        {
            using var context = GetNewContext();
            return context.Set<RMAudit>()
                .Where(item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                .GroupBy(item => item.Category)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByObject(DateTime startTime, DateTime endTime)
        {
            using var context = GetNewContext();
            return context.Set<RMAudit>()
                .Where(item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                .GroupBy(item => item.Object)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByAction(DateTime startTime, DateTime endTime)
        {
            using var context = GetNewContext();
            return context.Set<RMAudit>()
                .Where(item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                .GroupBy(item => item.Action)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByStatus(DateTime startTime, DateTime endTime)
        {
            using var context = GetNewContext();
            return context.Set<RMAudit>()
                .Where(item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                .GroupBy(item => item.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public List<AuditAction> GetAuditActionFromDB()
        {
            using var context = GetNewContext();
            return context.Audit
                .AsNoTracking()
                .Select(item => item.Action)
                .Distinct()
                .Select(item => (AuditAction)item)
                .ToList();
        }

        public List<AuditCategory> GetAuditModuleFromDB()
        {
            using var context = GetNewContext();
            return context.Audit
                .AsNoTracking()
                .Select(item => item.Category)
                .Distinct()
                .Select(item => (AuditCategory)item)
                .ToList();
        }

        public List<string> GetAuditUserFromDb()
        {
            using var context = GetNewContext();
            return context.Audit
                .AsNoTracking()
                .Select(item => item.UserName)
                .Distinct()
                .ToList();
        }

        public async Task Add(RMAudit audit)
        {
            using var context = GetNewContext();
            context.Audit.Add(audit);
            await context.SaveChangesAsync();
        }
    }
}
