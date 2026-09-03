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
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem
{
    public class RMDiscoveryFSDataDao : IRMDiscoveryFSDataDao
    {
        #region Inactive

        public async Task AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicInactiveData[] dataList)
        {
            if (dataList == null || dataList.Length == 0)
            {
                return;
            }
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSBasicInactiveData]";
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(params RMDiscoveryFSContainerInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSContainerInactiveData] WHERE ContainerId = @ContainerId";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async Task<List<RMDiscoveryFSBasicInactiveData>> GetBasicInactiveDataListAsync(List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSBasicInactiveData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryFSBasicInactiveData>();
        }

        //Need change
        public async IAsyncEnumerable<RMDiscoveryFSConnectionInactiveData> GetConnectionInactiveDataByContainerIdAsync(int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            yield return new RMDiscoveryFSConnectionInactiveData();
        }

        public async IAsyncEnumerable<RMDiscoveryFSContainerInactiveData> GetContainerInactiveDataListAsync(int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSContainerInactiveData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
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

                var dataList = dataCollection.ToTableList<RMDiscoveryFSContainerInactiveData>();
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

        #region Rot

        public async Task AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSBasicRuleLevelRotData]";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async Task<List<RMDiscoveryFSBasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSBasicRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryFSBasicRuleLevelRotData>();
        }

        public async IAsyncEnumerable<RMDiscoveryFSContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSContainerRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)}
                        FROM [{SecurityUtils.SanitizeSQLSchemaName(schemaName)}].[{SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name)}] 
                        WHERE ContainerId = @ContainerId 
                        ORDER BY Id 
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROW ONLY";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@ContainerId", containerId),
                    new SqlParameter("@Offset", i),
                    new SqlParameter("@PageSize", 1000));

                var dataList = dataCollection.ToTableList<RMDiscoveryFSContainerRuleLevelRotData>();
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

        public async Task AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSBasicCategoryLevelRotData]";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async Task<List<RMDiscoveryFSBasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSBasicRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryFSBasicRootLevelRotData>();
        }

        public async Task<List<RMDiscoveryFSBasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSBasicCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryFSBasicCategoryLevelRotData>();
        }

        public async IAsyncEnumerable<RMDiscoveryFSContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSContainerCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
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

                var dataList = dataCollection.ToTableList<RMDiscoveryFSContainerCategoryLevelRotData>();
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

        public async Task AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSBasicRootLevelRotData]";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async IAsyncEnumerable<RMDiscoveryFSContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryFSContainerRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
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

                var dataList = dataCollection.ToTableList<RMDiscoveryFSContainerRootLevelRotData>();
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

        public async Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(params RMDiscoveryFSContainerRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSContainerRuleLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(params RMDiscoveryFSContainerCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSContainerCategoryLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(params RMDiscoveryFSContainerRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMFSContainerRootLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteFSInsertAsync(dataList.ToList());
        }

        public async Task<RMDiscoveryFSAggregateTotalData> GetAggregateTotalDataAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            var res = await efContext.FSAggregateTotalDataList.FirstOrDefaultAsync();
            if (res == null)
            {
                return new RMDiscoveryFSAggregateTotalData();
            }
            return res;
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryFSAggregateTotalData data)
        {
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            await AddOrUpdateAggregateTotalDataAsync(efContext, data);
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryFSAggregateTotalData data)
        {
            efContext.FSAggregateTotalDataList.AddOrUpdate(data);
            await efContext.SaveChangesAsync();
        }

        #endregion
    }
}
