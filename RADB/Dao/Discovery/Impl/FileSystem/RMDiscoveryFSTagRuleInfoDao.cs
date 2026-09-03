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
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem
{
    public class RMDiscoveryFSTagRuleInfoDao : IRMDiscoveryFSTagRuleInfoDao
    {
        public async Task<int> AddOrUpdateAsync(List<RMDiscoveryFSTagRuleInfo> entities)
        {
            if (!entities.Any()) return 0;
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.FSTagRuleInfoes.AddOrUpdate(entities.ToArray());
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(RMDiscoveryFSTagRuleInfo entity)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.FSTagRuleInfoes.Add(entity);
            return await efContext.SaveChangesAsync();
        }

        public async Task<int> BatchUpdateAsync(List<RMDiscoveryFSTagRuleInfo> entities)
        {
            if (!entities.Any()) return 0;
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            foreach (var entity in entities)
            {
                var existingEntity = await efContext.FSTagRuleInfoes.FirstOrDefaultAsync(e => e.TagId == entity.TagId);
                if (existingEntity != null)
                {
                    var entry = efContext.Entry(existingEntity);
                    entity.Id = existingEntity.Id;
                    entry.CurrentValues.SetValues(entity);
                }
                else
                {
                    efContext.FSTagRuleInfoes.Add(entity);
                }
            }
            return await efContext.SaveChangesAsync();

        }

        public async Task<int> DeleteAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any()) return 0;
            await using var efContext = await RMDiscoveryDBManager.GetContextAsync();
            var paramNames = ids.Select((_, i) => $"@Id{i}").ToList();
            var sql = $"DELETE FROM [RMFSTagRuleInfo] WHERE TagId IN ({string.Join(",", paramNames)})";
            var parameters = new List<SqlParameter> {};
            parameters.AddRange(ids.Select((id, i) => new SqlParameter(paramNames[i], id)));
            return await efContext.ExecuteNonQueryAsync(sql, parameters.ToArray());
        }

        public async Task<int> DeleteAsync(Guid tagId)
        {
            var sql = $"DELETE FROM [RMFSTagRuleInfo] WHERE TagId = @Id AND Type = @Type";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            return await context.ExecuteNonQueryAsync(sql, new SqlParameter("@Id", tagId));
        }

        public async Task<int> DeleteAsync(List<RMDiscoveryFSTagRuleInfo> entities)
        {
            if (!entities.Any()) return 0;
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            foreach (var info in entities)
            {
                efContext.FSTagRuleInfoes.Attach(info);
                efContext.FSTagRuleInfoes.Remove(info);
            }
            return await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryFSTagRuleInfo>> GetAllAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.FSTagRuleInfoes.ToListAsync();
        }

        public async Task<RMDiscoveryFSTagRuleInfo> GetAsync(Guid tagId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.FSTagRuleInfoes.FirstOrDefaultAsync(s => s.TagId == tagId);
        }
    }
}
