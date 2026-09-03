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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMPhysicalPushColumnDao : BaseDao<RMPhysicalPushColumn>, IRMPhysicalPushColumnDao
    {
        public void AddOrUpdate(RMPhysicalPushColumn pushColumn)
        {
            using (var ctx = GetNewContext())
            {
                if (!ctx.RMPhysicalPushColumn.Any(s => s.ColumnUniqueId == pushColumn.ColumnUniqueId && s.PhysicalObjectId == pushColumn.PhysicalObjectId))
                {
                    ctx.RMPhysicalPushColumn.Add(pushColumn);
                    ctx.SaveChanges();
                }
                else
                {
                    var entities = ctx.RMPhysicalPushColumn.Where(s => s.ColumnUniqueId == pushColumn.ColumnUniqueId && s.PhysicalObjectId == pushColumn.PhysicalObjectId).ToList();
                    foreach (var entity in entities)
                    {
                        entity.ColumnValue = pushColumn.ColumnValue;
                    }
                    BatchUpdate(entities);
                }
            }
        }

        public Task DeletePushColumnAsync(Guid columnId, Guid physicalObjectId)
        {
            return BatchDeleteAsync(c => c.ColumnUniqueId == columnId && c.PhysicalObjectId == physicalObjectId);
        }

        public List<RMPhysicalPushColumn> GetColumnValues(Guid phyObjUniqueId, IEnumerable<Guid> columnUniqueIDs)
        {
            using (var ctx = GetNewContext())
            {
                var columnValues = ctx.RMPhysicalPushColumn.Where(s => columnUniqueIDs.Contains(s.ColumnUniqueId) && s.PhysicalObjectId == phyObjUniqueId);
                return columnValues.ToList();
            }
        }

        public List<RMPhysicalPushColumn> GetPushColumns(Guid columnUniqueId, List<Guid> physicObjectIds)
        {
            using (var ctx = GetNewContext())
            {
                var columnValues = ctx.RMPhysicalPushColumn.Where(s => columnUniqueId == s.ColumnUniqueId && physicObjectIds.Contains(s.PhysicalObjectId));
                return columnValues.ToList();
            }
        }

        public List<RMPhysicalPushColumn> GetPushColumnsByUniqueId(Guid columnUniqueId)
        {
            using (var ctx = GetNewContext())
            {
                var columnValues = ctx.RMPhysicalPushColumn.Where(s => columnUniqueId == s.ColumnUniqueId);
                return columnValues.ToList();
            }
        }

    }
}
