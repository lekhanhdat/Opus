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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMKeyValueDao : BaseDao<RMKeyValue>, IRMKeyValueDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMKeyValueDao));
        private const string DASHBOARD_SYNC_CHANGE_INFO = "DASHBOARD_SYNC_CHANGE_INFO";

        public async Task<List<RMKeyValue>> GetAllAsync()
        {
            using var context = GetNewContext();
            return await context.RMKeyValue.ToListAsync();
        }

        public async Task<bool> UpdateAsync(Dictionary<string, string> entities, IEnumerable<string> willDeleteEntityKeys)
        {
            using var context = GetNewContext();
            var existsEntities = (await context.RMKeyValue.ToListAsync()).ToDictionary(item => item.Key, item => item);
            foreach(var entity in entities)
            {
                if(existsEntities.TryGetValue(entity.Key, out var existsEntity))
                {
                    existsEntity.Value = entity.Value;
                    context.RMKeyValue.AddOrUpdate(existsEntity);
                    existsEntities.Remove(entity.Key);
                }
                else
                {
                    context.RMKeyValue.Add(new RMKeyValue
                    {
                        Key = entity.Key,
                        Value = entity.Value
                    });
                }
            }
            try
            {
                
                if (willDeleteEntityKeys.Count() != 0)
                {
                    var willDeleteEntities = context.RMKeyValue.Where(keyValue => willDeleteEntityKeys.Contains(keyValue.Key));
                    context.RMKeyValue.RemoveRange(willDeleteEntities);
                }
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
            //var willDeleteEntities = existsEntities.Values;
            //if(willDeleteEntities.Any())
            //{
            //    context.RMKeyValue.RemoveRange(willDeleteEntities);
            //}

            
        }

        public async Task<bool> SaveOrUpdateAsync(RMKeyValue entity)
        {
            using (var ctx = GetNewContext())
            {
                var exist = base.Exist(o => o.Key.Equals(entity.Key));
                if (exist)
                {
                    return await base.UpdateAsync(entity);
                }
                else
                {
                    base.Create(entity);
                    return true;
                }
            }
        }

        public async Task<bool> UpsertAsync(string key, string value)
        {
            using (var context = GetNewContext())
            {
                var exist = context.RMKeyValue.Any(o => o.Key.Equals(key));
                if (exist)
                {
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    return await context.Database.ExecuteSqlCommandAsync(
                        $"Update [{context.SchemaName}].RMKeyValues SET [Value]=@Value WHERE [Key]=@Key",
                        new SqlParameter[] { new SqlParameter("@Key", key), new SqlParameter("@Value", value) }
                    ) > 0;
                }

                base.Create(new RMKeyValue() { Key = key, Value = value });
                return true;
            }
        }

        public bool Save(RMKeyValue entity)
        {
            using (var ctx = GetNewContext())
            {
                var exist = ctx.RMKeyValue.Any(o => o.Key.Equals(entity.Key));
                if (!exist)
                {
                    base.Create(entity);
                    return true;
                }
            }
            return false;
        }

        public RMKeyValue GetValueByKey(string key)
        {
            using (var ctx = GetNewContext())
            {
                var setting = ctx.RMKeyValue.AsNoTracking().FirstOrDefault(k => k.Key.Equals(key));
                return setting;
            }
        }

        public async Task<T> GetValueByKeyAsync<T>(string key)
        {
            using (var ctx = GetNewContext())
            {
                var setting = await ctx.RMKeyValue.FirstOrDefaultAsync(k => k.Key.Equals(key));
                if (setting == null)
                {
                    return default;
                }
                return JsonConvert.DeserializeObject<T>(setting.Value);
            }
        }

        public async Task<T> GetValueByKeyAsync<T>(string key, T defaultValue)
        {
            using (var ctx = GetNewContext())
            {
                var setting = await ctx.RMKeyValue.FirstOrDefaultAsync(k => k.Key.Equals(key));
                if (setting == null)
                {
                    return defaultValue;
                }
                return JsonConvert.DeserializeObject<T>(setting.Value);
            }
        }

        public async Task<IEnumerable<RMKeyValue>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMKeyValue.AsNoTracking().Where(k => k.Key != DASHBOARD_SYNC_CHANGE_INFO).OrderBy(k => k.Key).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertKeyValueTableAsync(IEnumerable<RMKeyValue> keyValues)
        {
            using var context = GetNewContext();
            try
            {
                context.RMKeyValue.AddRange(keyValues);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMKeyValues data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllKeyValueAsync()
        {
            return await TruncateAllDataInTableAsync("RMKeyValues");
        }

        public async Task<string> GetValueByKeyAsync(string key)
        {
            using var ctx = GetNewContext();
            return await ctx.RMKeyValue
                .Where(k => k.Key == key)
                .Select(k => k.Value)
                .FirstOrDefaultAsync();
        }

        public bool TryGetBoolValue(string key, out bool value)
        {
            value = false;
            using var context = GetNewContext();
            var keyValue = context.RMKeyValue.FirstOrDefault(item => item.Key == key);
            if(keyValue == null || string.IsNullOrWhiteSpace(keyValue.Value))
            {
                return false;
            }

            if(!bool.TryParse(keyValue.Value, out value))
            {
                return false;
            }

            return true;
        }

        public bool AtomicityUpdate(string key, string oldValue, string newValue)
        {
            using (var context = GetNewContext())
            {
                SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                return context.Database.ExecuteSqlCommand(
                        $"Update [{context.SchemaName}].RMKeyValues SET [Value]=@Value WHERE [Key]=@Key AND [Value] = @OldValue",
                        new SqlParameter[] { new SqlParameter("@Key", key), new SqlParameter("@Value", newValue), new SqlParameter("@OldValue", oldValue)}
                    ) > 0;
            }
        }
    }
}
