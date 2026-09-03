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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMCustomMetadataColumnDao : IRMCustomMetadataColumnDao
    {
        public async Task AddOrUpdateCustomMetadataColumnsAsync(params RMCustomMetadataColumn[] customMetadataColumns)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.RMCustomMetadataColumns.AddOrUpdate(customMetadataColumns);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RMCustomMetadataColumn>> GetAllCustomMetadataColumnsAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMCustomMetadataColumns.ToListAsync();
        }


        public async Task<IEnumerable<RMCustomMetadataColumn>> GetInUsedCustomMetadataColumnsAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var inUsedColumnIds = await context.RMCustomIndexMetadatas.Select(item => item.TargetColumnId).ToListAsync();
            return await context.RMCustomMetadataColumns.Where(item => inUsedColumnIds.Contains(item.UniqueId)).ToListAsync();
        }

        public async Task<IEnumerable<RMCustomMetadataColumn>> GetCustomMetadataColumnsAsync(params Guid[] customMetadataColumnIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var columnIds = customMetadataColumnIds.ToArray();
            return await context.RMCustomMetadataColumns.Where(item => Enumerable.Contains(columnIds, item.UniqueId)).ToListAsync();
        }

        public async Task DeleteCustomMetadataColumnAsync(params RMCustomMetadataColumn[] customMetadataColumns)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            foreach (var data in customMetadataColumns)
            {
                context.RMCustomMetadataColumns.Attach(data);
                context.RMCustomMetadataColumns.Remove(data);
            }
            await context.SaveChangesAsync();
        }

        public async Task DeleteAllCustomMetadataColumnsAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.RMCustomMetadataColumns.RemoveRange(context.RMCustomMetadataColumns.ToArray());
            await context.SaveChangesAsync();
        }
    }
}
