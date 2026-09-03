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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class LnkUserGroupDao : BaseDao<RMLnkUserGroup>, ILnkUserGroupDao
    {
        //private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        private readonly RALogger Logger = RALogger.GetInstance(typeof(LnkUserGroupDao));
        public Task<List<string>> GetAllGroupIdsAsync(string userId)
        {
            return RMCacheManager.Cache.TryGetAsync(IRMCache.Keys.LnkUserGroupDao_GetAllGroupIdsAsync + userId, async () =>
            {
                using (var context = GetNewContext())
                {
                    return await context.LnkUserGroup.AsNoTracking().Where(o => o.UserId == userId).Select(o => o.GroupId).Distinct().ToListAsync();
                }
            });
        }

        public async Task RemoveUserNotInGroupAsync(string userId)
        {
            using (var context = GetNewContext())
            {
                var associations = await context.LnkUserGroup.Where(a => a.UserId == userId).ToListAsync();
                context.LnkUserGroup.RemoveRange(associations);
                await context.SaveChangesAsync();
                //await Cache.RemoveAsync(IRMCache.Keys.LnkUserGroupDao_GetAllGroupIdsAsync + userId);
                await RMCacheManager.LnkUserGroupDeleted(KeyType._Default,userId);
            }
        }

        public async Task AddUsersInGroupAsync(IEnumerable<string> userIds, string groupId)
        {
            using var context = GetNewContext();
            await BatchDeleteAsync(lnkUserGroup => lnkUserGroup.GroupId == groupId);
            await BatchCreateAsync(userIds.Select(userId => new RMLnkUserGroup()
            {
                GroupId = groupId,
                UserId = userId
            }));
        }

        public async Task<List<string>> GetAllUserIdsAsync(string groupId)
        {
            using var context = GetNewContext();
            return await context.LnkUserGroup.Where(o => o.GroupId == groupId).Select(o => o.UserId).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<RMLnkUserGroup>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.LnkUserGroup.AsNoTracking().OrderBy(o => o.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        public async Task<long> MultiGeoInsertLnkUserGroupTableAsync(IEnumerable<RMLnkUserGroup> lnkUserGroups)
        {
            using var context = GetNewContext();
            string tableName = "RMLnkUserGroups";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, UserId, GroupId) VALUES ");
                int i = 0;
                foreach (var item in lnkUserGroups)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", (object)item.UserId ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", (object)item.GroupId ?? DBNull.Value));
                    paramIndex += 3;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMLnkUserGroups data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
        public async Task<long> MultiGeoDeleteAllLnkUserGroupTableAsync()
        {
            return await TruncateAllDataInTableAsync("RMLnkUserGroups");
        }
    }
}
