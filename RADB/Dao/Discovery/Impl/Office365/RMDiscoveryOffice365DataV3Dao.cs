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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using NVelocity.Runtime.Resource;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365DataV3Dao : IRMDiscoveryOffice365DataV3Dao
    {
        #region Inactive

        #region Site

        public async Task DeleteSiteInactiveDataListAsync(Guid o365TenantId, int siteId)
        {
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);

            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $"DELETE FROM {schemaName}$RMSiteInactiveData WHERE SiteId = @SiteId";
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddSiteInactiveDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteInactiveData> GetSiteInactiveDataByContainerIdAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteInactiveData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {SecurityUtils.SanitizeSQLSchemaName(schemaName)}${SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name)} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteInactiveData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteInactiveData> GetSiteInactiveDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteInactiveData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE SiteId = @SiteId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@SiteId", siteId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteInactiveData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        #endregion

        #region Container

        public async Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMContainerInactiveData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertContainerInactiveDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerInactiveData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365ContainerInactiveData));
            var customColumns = data.CustomColumns.Select(item => item.Name).ToList();
            var selectedColumns = GetSafeSelectedColumns(tableInfo.Columns.Select(item => item.Name).Concat(customColumns));
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 {string.Join(", ", selectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
WHERE ContainerId = @ContainerId AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContainerId", data.ContainerId),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365ContainerInactiveData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                foreach (var customColumn in data.CustomColumns)
                {
                    var existingCustomColumn = existingData.CustomColumns.FirstOrDefault(item => item.Name == customColumn.Name);
                    if (existingCustomColumn != null)
                    {
                        customColumn.Value = Convert.ToInt64(customColumn.Value) + Convert.ToInt64(existingCustomColumn.Value);
                    }
                }

                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[{tableInfo.Name}] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365ContainerInactiveData> GetContainerInactiveDataListAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365ContainerInactiveData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContainerId = @ContainerId ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROW ONLY";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@ContainerId", containerId),
                    new SqlParameter("@Offset", i),
                    new SqlParameter("@PageSize", 1000));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365ContainerInactiveData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        #endregion

        #region Baisc

        public async Task AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMBasicInactiveData] WHERE ContentSource = @ContentSource";
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertBasicInactiveDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicInactiveData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365BasicInactiveData));
            var customColumns = data.CustomColumns.Select(item => item.Name).ToList();
            var selectedColumns = GetSafeSelectedColumns(tableInfo.Columns.Select(item => item.Name).Concat(customColumns));
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 {string.Join(", ", selectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
WHERE ContentSource = @ContentSource AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContentSource", data.ContentSource),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365BasicInactiveData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                foreach (var customColumn in data.CustomColumns)
                {
                    var existingCustomColumn = existingData.CustomColumns.FirstOrDefault(item => item.Name == customColumn.Name);
                    if (existingCustomColumn != null)
                    {
                        customColumn.Value = Convert.ToInt64(customColumn.Value) + Convert.ToInt64(existingCustomColumn.Value);
                    }
                }

                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[{tableInfo.Name}] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async Task<List<RMDiscoveryOffice365BasicInactiveData>> GetBasicInactiveDataListAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365BasicInactiveData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryOffice365BasicInactiveData>();
        }

        public async Task DeleteBasicInactiveDataAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMBasicInactiveData] WHERE ContentSource = @ContentSource";
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
        }

        #endregion

        #endregion

        #region ROT
        #region Site

        public async Task DeleteSiteRuleLevelRotDataListAsync(Guid o365TenantId, int siteId)
        {
            var schema = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var sql = $"DELETE FROM {schema}$RMSiteRuleLevelRotData WHERE SiteId = @SiteId";
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddSiteRuleLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteRuleLevelRotData> dataList)
        {
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteRuleLevelRotData> GetSiteRuleLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteRuleLevelRotData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteRuleLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async Task DeleteSiteCategoryLevelRotDataListAsync(Guid o365TenantId, int siteId)
        {
            var schema = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var sql = $"DELETE FROM {schema}$RMSiteCategoryLevelRotData WHERE SiteId = @SiteId";
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddSiteCategoryLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteCategoryLevelRotData> dataList)
        {
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteCategoryLevelRotData> GetSiteCategoryLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteCategoryLevelRotData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteCategoryLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async Task DeleteSiteRootLevelRotDataListAsync(Guid o365TenantId, int siteId)
        {
            var schema = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var sql = $"DELETE FROM {schema}$RMSiteRootLevelRotData WHERE SiteId = @SiteId";
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddSiteRootLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteRootLevelRotData> dataList)
        {
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteRootLevelRotData> GetSiteRootLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteRootLevelRotData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteRootLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteRuleLevelRotData> GetSiteRuleLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteRuleLevelRotData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE SiteId = @SiteId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@SiteId", siteId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteRuleLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteCategoryLevelRotData> GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteCategoryLevelRotData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE SiteId = @SiteId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@SiteId", siteId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteCategoryLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteRootLevelRotData> GetSiteRootLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365SiteRootLevelRotData));
            var schemaName = RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryOffice365SQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE SiteId = @SiteId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@SiteId", siteId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365SiteRootLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        #endregion

        #region Container

        public async Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMContainerRuleLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertContainerRuleLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerRuleLevelRotData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 Id, ContainerId, WithoutInDate, FileExtension, SizeRange, [Rule], FileTotalSize, FileSumCount
FROM [{schemaName}].[RMContainerRuleLevelRotData]
WHERE ContainerId = @ContainerId AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange AND [Rule] = @Rule";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContainerId", data.ContainerId),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange),
                new SqlParameter("@Rule", data.Rule));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365ContainerRuleLevelRotData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[RMContainerRuleLevelRotData] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365ContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365ContainerRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContainerId = @ContainerId ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROW ONLY";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@ContainerId", containerId),
                    new SqlParameter("@Offset", i),
                    new SqlParameter("@PageSize", 1000));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365ContainerRuleLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMContainerCategoryLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertContainerCategoryLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerCategoryLevelRotData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 Id, ContainerId, WithoutInDate, FileExtension, SizeRange, Category, FileTotalSize, FileSumCount
FROM [{schemaName}].[RMContainerCategoryLevelRotData]
WHERE ContainerId = @ContainerId AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange AND Category = @Category";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContainerId", data.ContainerId),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange),
                new SqlParameter("@Category", data.Category));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365ContainerCategoryLevelRotData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[RMContainerCategoryLevelRotData] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365ContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365ContainerCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContainerId = @ContainerId ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROW ONLY";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@ContainerId", containerId),
                    new SqlParameter("@Offset", i),
                    new SqlParameter("@PageSize", 1000));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365ContainerCategoryLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        public async Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMContainerRootLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertContainerRootLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerRootLevelRotData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 Id, ContainerId, WithoutInDate, FileExtension, SizeRange, FileTotalSize, FileSumCount
FROM [{schemaName}].[RMContainerRootLevelRotData]
WHERE ContainerId = @ContainerId AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContainerId", data.ContainerId),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365ContainerRootLevelRotData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[RMContainerRootLevelRotData] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365ContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365ContainerRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContainerId = @ContainerId ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROW ONLY";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@ContainerId", containerId),
                    new SqlParameter("@Offset", i),
                    new SqlParameter("@PageSize", 1000));

                var dataList = dataCollection.ToTableList<RMDiscoveryOffice365ContainerRootLevelRotData>();
                foreach (var data in dataList)
                {
                    yield return data;
                }

                if (dataList.Count < 1000)
                {
                    break;
                }
            }
        }

        #endregion

        #region Basic

        public async Task AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMBasicRuleLevelRotData] WHERE ContentSource = @ContentSource";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertBasicRuleLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicRuleLevelRotData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 Id, WithoutInDate, FileExtension, SizeRange, [Rule], ContentSource, FileTotalSize, FileSumCount
FROM [{schemaName}].[RMBasicRuleLevelRotData]
WHERE ContentSource = @ContentSource AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange AND [Rule] = @Rule";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContentSource", data.ContentSource),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange),
                new SqlParameter("@Rule", data.Rule));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365BasicRuleLevelRotData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[RMBasicRuleLevelRotData] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async Task<List<RMDiscoveryOffice365BasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365BasicRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryOffice365BasicRuleLevelRotData>();
        }

        public async Task AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMBasicCategoryLevelRotData] WHERE ContentSource = @ContentSource";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertBasicCategoryLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicCategoryLevelRotData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 Id, WithoutInDate, FileExtension, SizeRange, Category, ContentSource, FileTotalSize, FileSumCount
FROM [{schemaName}].[RMBasicCategoryLevelRotData]
WHERE ContentSource = @ContentSource AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange AND Category = @Category";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContentSource", data.ContentSource),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange),
                new SqlParameter("@Category", data.Category));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365BasicCategoryLevelRotData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[RMBasicCategoryLevelRotData] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async Task<List<RMDiscoveryOffice365BasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365BasicCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryOffice365BasicCategoryLevelRotData>();
        }

        public async Task AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMBasicRootLevelRotData] WHERE ContentSource = @ContentSource";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task UpsertBasicRootLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicRootLevelRotData data)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT TOP 1 Id, WithoutInDate, FileExtension, SizeRange, ContentSource, FileTotalSize, FileSumCount
FROM [{schemaName}].[RMBasicRootLevelRotData]
WHERE ContentSource = @ContentSource AND WithoutInDate = @WithoutInDate AND FileExtension = @FileExtension AND SizeRange = @SizeRange";
            var existingCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContentSource", data.ContentSource),
                new SqlParameter("@WithoutInDate", data.WithoutInDate),
                new SqlParameter("@FileExtension", data.FileExtension),
                new SqlParameter("@SizeRange", data.SizeRange));
            var existingData = existingCollection.ToTableList<RMDiscoveryOffice365BasicRootLevelRotData>().FirstOrDefault();
            if (existingData != null)
            {
                data.FileTotalSize += existingData.FileTotalSize;
                data.FileSumCount += existingData.FileSumCount;
                await context.ExecuteNonQueryAsync($"DELETE FROM [{schemaName}].[RMBasicRootLevelRotData] WHERE Id = @Id", new SqlParameter("@Id", existingData.Id));
            }

            await context.ExecuteInsertAsync([data], o365TenantId);
        }

        public async Task<List<RMDiscoveryOffice365BasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryOffice365BasicRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryOffice365BasicRootLevelRotData>();
        }

        #endregion

        #endregion

        private static List<string> GetSafeSelectedColumns(IEnumerable<string> columnNames)
        {
            return columnNames.Select(columnName => SecurityUtils.QuoteSQLIdentifier(columnName)).ToList();
        }
    }
}
