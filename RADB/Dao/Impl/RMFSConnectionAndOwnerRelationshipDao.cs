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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Microsoft.InformationProtection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMFSConnectionAndOwnerRelationshipDao : BaseDao<RMFSConnectionAndOwnerRelationship>, IRMFSConnectionAndOwnerRelationshipDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMFSConnectionAndOwnerRelationshipDao));
        public List<RMFSConnectionAndOwnerRelationship> GetOwnersByConnectionId(Guid connectionId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFSConnectionAndOwnerRelationship.Where(o => o.ConnectionId == connectionId).ToList();
            }
        }

        public List<RMFSConnectionAndOwnerRelationship> GetOwnersByConnectionId(Guid connectionId, FSConnectionOwnerType ownerType)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFSConnectionAndOwnerRelationship.Where(o => o.ConnectionId == connectionId && o.Type == ownerType).ToList();
            }
        }

        public async Task<List<RMFSConnectionAndOwnerRelationship>> GetOwnersByUserIntIdAsync(int userIntId)
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.RMFSConnectionAndOwnerRelationship.Where(o => o.UserIntId == userIntId).ToListAsync();
            }
        }

        public async Task<List<RMFSConnectionAndOwnerRelationship>> GetOwnersByUserIntIdsAsync(List<int> userIds)
        {
            using (var context = GetNewContext())
            {
                return await context.RMFSConnectionAndOwnerRelationship
                    .Where(x => userIds.Contains(x.UserIntId))
                    .ToListAsync();
            }
        }

        public void AddOwners(Guid connectionId, List<int> informationOwnerIntIds, List<int> recordOwnerIntIds)
        {
            var needAddInformationOwners = informationOwnerIntIds.ConvertAll(userId => new RMFSConnectionAndOwnerRelationship
            {
                ConnectionId = connectionId,
                UserIntId = userId,
                Type = FSConnectionOwnerType.InformationOwner
            });
            var needAddRecordOwnerIntIds = recordOwnerIntIds.ConvertAll(userId => new RMFSConnectionAndOwnerRelationship
            {
                ConnectionId = connectionId,
                UserIntId = userId,
                Type = FSConnectionOwnerType.RecordOwner
            });
            var owners = needAddInformationOwners.Concat(needAddRecordOwnerIntIds).ToList();
            BatchCreate(owners);
        }

        public Task RemoveAllByConnectionIdAsync(Guid connectionId)
        {
            return BatchDeleteAsync(item => item.ConnectionId == connectionId);
        }

        public Task RemoveAllRecordOwnersByConnectionIdAsync(Guid connectionId)
        {
            return BatchDeleteAsync(item => item.ConnectionId == connectionId && item.Type == FSConnectionOwnerType.RecordOwner);
        }

        public Task RemoveAllByConnectionIdsAsync(List<Guid> connectionIds)
        {
            return BatchDeleteAsync(item => connectionIds.Contains(item.ConnectionId));
        }

        public async Task<IEnumerable<RMFSConnectionAndOwnerRelationship>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMFSConnectionAndOwnerRelationship.AsNoTracking().OrderBy(item => item.ConnectionId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertFSConnectionAndOwnerRelationshipTableAsync(IEnumerable<RMFSConnectionAndOwnerRelationship> fSConnectionAndOwnerRelationships)
        {
            using var context = GetNewContext();
            string tableName = "RMFSConnectionAndOwnerRelationships";
            try
            {
                string schemaName = AvePoint.GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (ConnectionId, UserIntId, Type) VALUES ");
                int i = 0;
                foreach (var item in fSConnectionAndOwnerRelationships)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.ConnectionId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.UserIntId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", (int)item.Type));
                    paramIndex += 3;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMFSConnectionAndOwnerRelationships data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllFSConnectionAndOwnerRelationshipAsync()
        {
            return await TruncateAllDataInTableAsync("RMFSConnectionAndOwnerRelationships");
        }

        public async Task<List<FSConnection>> GetConnectionsByUserIdsAndRoles(List<int> userIds,List<FSConnectionOwnerType>? userRoles = null)
        {
            userRoles ??=
            [
                FSConnectionOwnerType.RecordOwner,
                FSConnectionOwnerType.InformationOwner
            ];

            using var context = GetNewContext();
            return await context.RMFSConnectionAndOwnerRelationship
                .Where(r => userIds.Contains(r.UserIntId) && userRoles.Contains(r.Type))
                .Join(
                    context.FSConnection,
                    r => r.ConnectionId,
                    c => c.Id,
                    (r, c) => c)
                .Distinct()
                .ToListAsync();
        }
    }
}
