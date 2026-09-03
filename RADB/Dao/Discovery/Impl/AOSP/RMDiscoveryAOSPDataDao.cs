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
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.AOSP
{
    public class RMDiscoveryAOSPDataDao : IRMDiscoveryAOSPDataDao
    {

        public async Task DeleteSiteRuleLevelRotDataListAsync(Guid o365TenantId, int siteId)
        {
            var schema = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var sql = $"DELETE FROM {schema}$RMAOSPSiteRuleLevelRotData WHERE SiteId = @SiteId";
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddSiteRuleLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryAOSPSiteRuleLevelRotData> dataList)
        {
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, o365TenantId);
        }

        public async Task DeleteSiteCategoryLevelRotDataListAsync(Guid o365TenantId, int siteId)
        {
            var schema = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var sql = $"DELETE FROM {schema}$RMAOSPSiteCategoryLevelRotData WHERE SiteId = @SiteId";
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddSiteCategoryLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryAOSPSiteCategoryLevelRotData> dataList)
        {
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, o365TenantId);
        }

        public async Task DeleteSiteRootLevelRotDataListAsync(Guid o365TenantId, int siteId)
        {
            var schema = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var sql = $"DELETE FROM {schema}$RMAOSPSiteRootLevelRotData WHERE SiteId = @SiteId";
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddSiteRootLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryAOSPSiteRootLevelRotData> dataList)
        {
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, o365TenantId);
        }

        public async Task AddSiteInactiveDataListAsync(Guid o365TenantId, params RMDiscoveryAOSPSiteInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task DeleteSiteInactiveDataListAsync(Guid o365TenantId, int siteId)
        {
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);

            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $"DELETE FROM {schemaName}$RMAOSPSiteInactiveData WHERE SiteId = @SiteId";
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@SiteId", siteId));
        }

        public async Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPContainerInactiveData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteInactiveData> GetSiteInactiveDataByContainerIdAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteInactiveData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {SecurityUtils.SanitizeSQLSchemaName(schemaName)}${SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name)} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteInactiveData>();
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


        public async Task AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPBasicInactiveData] WHERE ContentSource = @ContentSource";
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task<List<RMDiscoveryAOSPBasicInactiveData>> GetBasicInactiveDataListAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPBasicInactiveData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryAOSPBasicInactiveData>();
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPContainerInactiveData> GetContainerInactiveDataListAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPContainerInactiveData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPContainerInactiveData>();
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

        public async Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPContainerRuleLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteRuleLevelRotData> GetSiteRuleLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteRuleLevelRotData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteRuleLevelRotData>();
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

        public async Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPContainerCategoryLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteCategoryLevelRotData> GetSiteCategoryLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteCategoryLevelRotData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteCategoryLevelRotData>();
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

        public async Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPContainerRootLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteRootLevelRotData> GetSiteRootLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteRootLevelRotData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteRootLevelRotData>();
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

        public async Task AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPBasicRuleLevelRotData] WHERE ContentSource = @ContentSource";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task<List<RMDiscoveryAOSPBasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPBasicRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryAOSPBasicRuleLevelRotData>();
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPContainerRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPContainerRuleLevelRotData>();
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

        public async Task AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPBasicCategoryLevelRotData] WHERE ContentSource = @ContentSource";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task<List<RMDiscoveryAOSPBasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPBasicCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryAOSPBasicCategoryLevelRotData>();
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPContainerCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPContainerCategoryLevelRotData>();
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

        public async Task AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var contentSource = dataList.First().ContentSource;
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMAOSPBasicRootLevelRotData] WHERE ContentSource = @ContentSource";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId);
        }

        public async Task<List<RMDiscoveryAOSPBasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPBasicRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}] 
WHERE ContentSource = @ContentSource";
            var dataCollection = await context.ExecuteQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
            return dataCollection.ToTableList<RMDiscoveryAOSPBasicRootLevelRotData>();
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(Guid o365TenantId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPContainerRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPContainerRootLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteInactiveData> GetSiteInactiveDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteInactiveData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
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

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteInactiveData>();
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

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteInactiveData> GetSiteInactiveDataBySqlConditionalExpressionAsync(Guid o365TenantId, List<int> siteIds, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteInactiveData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await RMDiscoveryAOSPSQLiteDBManager.DownloadDatabaseAsync();
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE SiteId IN {DatabaseUtility.BuildInClause(siteIds)} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteInactiveData>();
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

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteRuleLevelRotData> GetSiteRuleLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteRuleLevelRotData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
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

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteRuleLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteRootLevelRotData> GetSiteRootLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteRootLevelRotData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
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

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteRootLevelRotData>();
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
        
        public async IAsyncEnumerable<RMDiscoveryAOSPSiteRootLevelRotData> GetSiteRootLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, List<int> siteIds)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteRootLevelRotData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE SiteId IN {DatabaseUtility.BuildInClause(siteIds)} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteRootLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteCategoryLevelRotData> GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, List<int> siteIds)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryAOSPSiteCategoryLevelRotData));
            var schemaName = RMDiscoveryAOSPSQLiteDBManager.GetSchemaName(o365TenantId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryAOSPSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE SiteId IN {DatabaseUtility.BuildInClause(siteIds)} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryAOSPSiteCategoryLevelRotData>();
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

        public async Task<RMDiscoveryAOSPAggregateTotalData> GetAggregateTotalDataAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            var res = await efContext.AOSPAggregateTotalDataList.FirstOrDefaultAsync(item => item.ContentSource == contentSource);
            if (res == null)
            {
                return new RMDiscoveryAOSPAggregateTotalData()
                {
                    ContentSource = contentSource
                };
            }

            return res;
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(Guid o365TenantId, RMDiscoveryAOSPAggregateTotalData data)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            await AddOrUpdateAggregateTotalDataAsync(efContext, data);
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryAOSPAggregateTotalData data)
        {
            efContext.AOSPAggregateTotalDataList.AddOrUpdate(data);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryAOSPAggregateTotalData>> GetAggregateTotalDataListAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            return await efContext.AOSPAggregateTotalDataList.ToListAsync();
        }
    }
}
