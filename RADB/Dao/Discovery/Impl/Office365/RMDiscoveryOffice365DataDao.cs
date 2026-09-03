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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365DataDao : IRMDiscoveryOffice365DataDao
    {
        public async Task AddSiteInactiveDataAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInactiveData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task AddOrUpdateContainerInactiveDataAsync(Guid o365TenantId, int containerId, params RMDiscoveryOffice365ContainerInactiveData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMContainerInactiveData] WHERE ContainerId = @ContainerId";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));

            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task<List<RMDiscoveryOffice365ContainerInactiveData>> GetContainerInactiveDataListAsync(Guid o365Tenant, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365ContainerInactiveData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365Tenant);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name));
            var sql = $"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] WHERE ContainerId = @ContainerId";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            return dataCollection.ToTableList<RMDiscoveryOffice365ContainerInactiveData>();
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365ContainerInactiveData> GetContainerInactiveDataListAsync(Guid o365Tenant, List<RMDiscoveryCustomColumn> customColumns)
        {
            const int pageSize = 10000;
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365ContainerInactiveData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365Tenant);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name));
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            for (var i = 0; ; i++)
            {
                var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
ORDER BY Id
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
                var dataCollection = await context.ExecuteQueryAsync(sql
                    , new SqlParameter("@offset", i * pageSize)
                    , new SqlParameter("@pageSize", pageSize));
                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365ContainerInactiveData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < pageSize)
                {
                    yield break;
                }
            }

        }

        public async Task AddOrUpdateBasicInactiveDataAsync(Guid o365TenantId, SourceFlag contentSource, params RMDiscoveryOffice365BasicInactiveData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMBasicInactiveData] WHERE ContentSource = @contentSource";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteQueryAsync(sql, new SqlParameter("@contentSource", (int)contentSource));

            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task<List<RMDiscoveryOffice365BasicInactiveData>> GetBasicInactiveDataListAsync(Guid o365Tenant, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365BasicInactiveData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365Tenant);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name));
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            var sql = $"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] WHERE ContentSource = @contentSource";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@contentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryOffice365BasicInactiveData>();
        }

        public async Task AddSiteRotDataAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteRotData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task AddOrUpdateContainerRotDataAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerRotData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            using var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            context.Office365ContainerRotDataList.AddOrUpdate(dataList);
            await context.SaveChangesAsync();
        }

        public async Task AddOrUpdateContainerRotDataAsync(Guid o365TenantId, int containerId, params RMDiscoveryOffice365ContainerRotData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            await using (var context = await RMDiscoveryDBManager.GetContextAsync())
            {
                var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
                SecurityUtils.SanitizeSQLSchemaName(schemaName);
                var sql = $"DELETE FROM [{schemaName}].[RMContainerRotData] WHERE ContainerId = @ContainerId";
                await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            }

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365ContainerRotDataList.AddOrUpdate(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365ContainerRotData>> GetContainerRotDataListAsync(Guid o365TenantId, int containerId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ContainerRotDataList.Where(item => item.ContainerId == containerId).ToListAsync();
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365ContainerRotData> GetContainerRotDataListAsync(Guid o365TenantId)
        {
            const int pageSize = 10000;
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            for (var i = 0; ; i++)
            {
                var dataList = await efContext.Office365ContainerRotDataList
                    .OrderBy(item => item.Id)
                    .Skip(i * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < pageSize)
                {
                    yield break;
                }
            }
        }

        public async Task AddOrUpdateBasicRotDataAsync(Guid o365TenantId, SourceFlag contentSource, params RMDiscoveryOffice365BasicRotData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            await using (var context = await RMDiscoveryDBManager.GetContextAsync())
            {
                var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
                SecurityUtils.SanitizeSQLSchemaName(schemaName);
                var sql = $"DELETE FROM [{schemaName}].[RMBasicRotData] WHERE ContentSource = @contentSource";
                await context.ExecuteNonQueryAsync(sql, new SqlParameter("@contentSource", contentSource));
            }

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365BasicRotDataList.AddOrUpdate(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task AddBasicRotDataAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicRotData[] dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365BasicRotDataList.AddOrUpdate(dataList);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365BasicRotData>> GetBasicRotDataListAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365BasicRotDataList.Where(item => item.ContentSource == contentSource).ToListAsync();
        }

        public async Task<RMDiscoveryOffice365AggregateTotalData> GetAggregateTotalDataAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var res = await efContext.Office365AggregateTotalDataList.FirstOrDefaultAsync(item => item.ContentSource == contentSource);
            if (res == null)
            {
                return new RMDiscoveryOffice365AggregateTotalData()
                {
                    ContentSource = contentSource
                };
            }

            return res;
        }

        public async Task<List<RMDiscoveryOffice365AggregateTotalData>> GetAggregateTotalDataListAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365AggregateTotalDataList.ToListAsync();
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(Guid o365TenantId, RMDiscoveryOffice365AggregateTotalData data)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            await AddOrUpdateAggregateTotalDataAsync(efContext, data);
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryOffice365AggregateTotalData data)
        {
            efContext.Office365AggregateTotalDataList.AddOrUpdate(data);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365SiteRotData>> GetSiteRotDataListAsync(Guid o365TenantId, int siteId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365SiteRotDataList.Where(item => item.SiteId == siteId).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365SiteInactiveData>> GetSiteInactiveDataListAsync(Guid o365TenantId, int siteId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteInactiveData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name));
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            var sql = $"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryOffice365SiteInactiveData>();
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteRotData> GetSiteRotDataListByContainerAsync(Guid o365TenantId, int containerId)
        {
            const int pageSize = 10000;
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            for (var i = 0; ; i++)
            {
                var dataList = await efContext.Office365SiteRotDataList.Where(item => item.ContainerId == containerId)
                    .OrderBy(item => item.Id)
                    .Skip(i * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < pageSize)
                {
                    yield break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteInactiveData> GetSiteInactiveDataListByContainerAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            const int pageSize = 10000;
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteInactiveData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name));
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            for (var i = 0; ; i++)
            {
                var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
WHERE ContainerId = @containerId
ORDER BY Id
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
                var dataCollection = await context.ExecuteQueryAsync(sql
                    , new SqlParameter("@containerId", containerId)
                    , new SqlParameter("@offset", i * pageSize)
                    , new SqlParameter("@pageSize", pageSize));
                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteInactiveData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < pageSize)
                {
                    yield break;
                }
            }
        }

        public async Task DeleteSiteRotDataListAsync(Guid o365TenantId, int siteId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMSiteRotData] WHERE SiteId = @siteId";
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@siteId", siteId));
        }

        public async Task DeleteSiteInactiveDataListAsync(Guid o365TenantId, int siteId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMSiteInactiveData] WHERE SiteId = @siteId";
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@siteId", siteId));
        }

        public async Task DeleteSitesDuplicateDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365RuleInfo[] duplicateRules)
        {

            var duplicateRuleIds = duplicateRules.Select(item => item.Id).ToList();
            const int deleteBatchCount = 10000;

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var effectCount = 0;
            do
            {
                var inClauseParamName = DatabaseUtility.BuildInClause(duplicateRuleIds, out var paramList);
                var sql = $"DELETE top({deleteBatchCount}) FROM [{schemaName}].[RMSiteRotData] WHERE [Rule] IN {inClauseParamName}";
                effectCount = await context.ExecuteNonQueryAsync(sql, paramList.ToArray());
            } while (effectCount >= deleteBatchCount);
        }

        public async Task DeleteContainersDuplicateDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365RuleInfo[] duplicateRules)
        {

            var duplicateRuleIds = duplicateRules.Select(item => item.Id).ToList();
            const int deleteBatchCount = 10000;

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var effectCount = 0;
            do
            {
                var inClauseParamName = DatabaseUtility.BuildInClause(duplicateRuleIds, out var paramList);
                var sql = $"DELETE top({deleteBatchCount}) FROM [{schemaName}].[RMContainerRotData] WHERE [Rule] IN {inClauseParamName}";
                effectCount = await context.ExecuteNonQueryAsync(sql, paramList.ToArray());
            } while (effectCount >= deleteBatchCount);
        }

        public async Task DeleteBasicDuplicateDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365RuleInfo[] duplicateRules)
        {

            var duplicateRuleIds = duplicateRules.Select(item => item.Id).ToList();
            const int deleteBatchCount = 10000;

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var effectCount = 0;
            do
            {
                var inClauseParamName = DatabaseUtility.BuildInClause(duplicateRuleIds, out var paramList);
                var sql = $"DELETE top({deleteBatchCount}) FROM [{schemaName}].[RMBasicRotData] WHERE [Rule] IN {inClauseParamName}";
                effectCount = await context.ExecuteNonQueryAsync(sql, paramList.ToArray());
            } while (effectCount >= deleteBatchCount);
        }
    }
}
