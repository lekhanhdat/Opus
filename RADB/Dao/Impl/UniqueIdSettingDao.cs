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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class UniqueIdSettingDao : BaseDao<RMUniqueIdSetting>, IUniqueIdSettingDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(UniqueIdSettingDao));
        public RMUniqueIdSetting LoadingUniqueIdSetting()
        {
            using var context = GetNewContext();
            return context.UniqueIdSetting.FirstOrDefault(setting => setting.UniqueIdType == UniqueIdType.Default);
        }

        public RMUniqueIdSetting LoadingUniqueIdSetting(UniqueIdType uniqueIdType)
        {
            using var context = GetNewContext();
            return context.UniqueIdSetting.FirstOrDefault(setting => setting.UniqueIdType == uniqueIdType);
        }

        public async Task<IEnumerable<RMUniqueIdSetting>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.UniqueIdSetting.AsNoTracking().OrderBy(u => u.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertUniqueIdSettingTableAsync(IEnumerable<RMUniqueIdSetting> uniqueIdSettings)
        {
            using var context = GetNewContext();
            string tableName = "RMUniqueIdSettings";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, IsActived, Name, Prefix, OverrideSPPrefix, UniqueIdType) VALUES ");
                int i = 0;
                foreach (var item in uniqueIdSettings)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.IsActived));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", (object)item.Name ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", (object)item.Prefix ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", item.OverrideSPPrefix));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", (int)item.UniqueIdType));
                    paramIndex += 6;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMUniqueIdSettings data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
        public async Task<long> MultiGeoDeleteAllUniqueIdSettingAsync()
        {
            return await TruncateAllDataInTableAsync("RMUniqueIdSettings");
        }

        public async Task UpdateUniqueIdSettingAsync(RMUniqueIdSetting setting)
        {
            using var content = GetNewContext();
            var settings = content.UniqueIdSetting.AsQueryable().ToList();
            if (settings.Count == 0)
            {
                content.UniqueIdSetting.Add(setting);
                content.SaveChanges();
                return;
            }

            var oldSetting = settings.FirstOrDefault(oSetting => oSetting.UniqueIdType == setting.UniqueIdType);
            if (oldSetting != null)
            {
                oldSetting.IsActived = setting.IsActived;
                oldSetting.Prefix = setting.Prefix;
                oldSetting.Name = setting.Name;
                oldSetting.OverrideSPPrefix = setting.OverrideSPPrefix;
                await this.UpdateAsync(oldSetting);
            }
            else
            {
                content.UniqueIdSetting.Add(setting);
                content.SaveChanges();
            }
        }

    }
}
