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
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Core;
using AvePoint.RA.Contract.RMWeb;
using System.Data.Entity;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class LnkUserRoleDao : BaseDao<RMLnkUserRole>, ILnkUserRoleDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(LnkUserRoleDao));
        public RMLnkUserRole GetAccountRole(string accountId)
        {
            return base.Find(o => o.UserId == accountId);
        }

        public List<RMPermission> GetUserPermissions(string userId)
        {
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                List<string> userGroupIds = new List<string>();
                userGroupIds = ctx.LnkUserGroup.Where(g => g.UserId == userId).Select(g => g.GroupId).ToList();
                userGroupIds.Add(userId);
                var roleIds = ctx.LnkUserRole.AsQueryable().Where(t => userGroupIds.Contains(t.UserId)).Select(r => r.RoleId).ToList();
                var pIds = ctx.LnkRolePermission.Where(p => roleIds.Contains(p.RoleId)).Select(p => p.PermissionId).ToList();
                return ctx.Permission.Where(p => pIds.Contains(p.PermissionId)).ToList();
            }
        }

        public async Task<IEnumerable<RMLnkUserRole>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.LnkUserRole.AsNoTracking().OrderBy(o => o.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoDeleteAllLnkUserRoleAsync()
        {
            return await TruncateAllDataInTableAsync("RMLnkUserRoles");
        }
        public async Task<long> MultiGeoInsertLnkUserRoleTableAsync(IEnumerable<RMLnkUserRole> lnkUserRoles)
        {
            using var context = GetNewContext();
            string tableName = "RMLnkUserRoles";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, UserId, RoleId) VALUES ");
                int i = 0;
                foreach (var item in lnkUserRoles)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", (object)item.UserId ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.RoleId));
                    paramIndex += 3;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMLnkUserRoles data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
    }
}
