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
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleDataDao : IRMDiscoveryGoogleDataDao
    {

        #region INACTIVE

        #region Drive
        public async Task AddDriveInactiveDataListAsync(string googleOrganizationId, params RMDiscoveryGoogleDriveInactiveData[] dataList)
        {
            if (dataList.Length == 0) return;
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async Task DeleteDriveInactiveDataListAsync(string googleOrganizationId, int driveId)
        {
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $"DELETE FROM {schemaName}$RMGoogleDriveInactiveData WHERE DriveId = @DriveId";
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@DriveId", driveId));
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveInactiveData> GetDriveInactiveDataByContainerIdAsync(string googleOrganizationId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveInactiveData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();

            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {SecurityUtils.SanitizeSQLSchemaName(schemaName)}${SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name)} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveInactiveData>();
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

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveInactiveData> GetDriveInactiveDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveInactiveData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE DriveId = @DriveId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@DriveId", driveId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveInactiveData>();
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
        public async Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }
            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleContainerInactiveData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleContainerInactiveData> GetContainerInactiveDataListAsync(string googleOrganizationId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleContainerInactiveData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleContainerInactiveData>();
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

        public async Task AddOrUpdateBasicInactiveDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleBasicInactiveData]";
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async Task<List<RMDiscoveryGoogleBasicInactiveData>> GetBasicInactiveDataListAsync(string googleOrganizationId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleBasicInactiveData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryGoogleBasicInactiveData>();
        }

        public async Task DeleteBasicInactiveDataAsync(string googleOrganizationId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleBasicInactiveData]";
            await context.ExecuteNonQueryAsync(sql);
        }

        #endregion

        #endregion

        #region ROT

        #region Drive

        public async Task DeleteDriveRuleLevelRotDataListAsync(string googleOrganizationId, int driveId)
        {
            var schema = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var sql = $"DELETE FROM {schema}$RMGoogleDriveRuleLevelRotData WHERE DriveId = @DriveId";
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@DriveId", driveId));
        }

        public async Task AddDriveRuleLevelRotDataListAsync(string googleOrganizationId, List<RMDiscoveryGoogleDriveRuleLevelRotData> dataList)
        {
            var schema = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, schema);
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveRuleLevelRotData> GetDriveRuleLevelRotDataByContainerIdAsync(string googleOrganizationId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveRuleLevelRotData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveRuleLevelRotData>();
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

        public async Task DeleteDriveCategoryLevelRotDataListAsync(string googleOrganizationId, int driveId)
        {
            var schema = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var sql = $"DELETE FROM {schema}$RMGoogleDriveCategoryLevelRotData WHERE DriveId = @DriveId";
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@DriveId", driveId));
        }

        public async Task AddDriveCategoryLevelRotDataListAsync(string googleOrganizationId, List<RMDiscoveryGoogleDriveCategoryLevelRotData> dataList)
        {
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, schemaName);
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveCategoryLevelRotData> GetDriveCategoryLevelRotDataByContainerIdAsync(string googleOrganizationId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveCategoryLevelRotData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveCategoryLevelRotData>();
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

        public async Task DeleteDriveRootLevelRotDataListAsync(string googleOrganizationId, int driveId)
        {
            var schema = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var sql = $"DELETE FROM {schema}$RMGoogleDriveRootLevelRotData WHERE DriveId = @DriveId";
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            await context.ExecuteNonQueryAsync(sql, new SQLiteParameter("@DriveId", driveId));
        }

        public async Task AddDriveRootLevelRotDataListAsync(string googleOrganizationId, List<RMDiscoveryGoogleDriveRootLevelRotData> dataList)
        {
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            await context.ExecuteInsertAsync(dataList, schemaName);
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveRootLevelRotData> GetDriveRootLevelRotDataByContainerIdAsync(string googleOrganizationId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveRootLevelRotData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();

            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE ContainerId = @ContainerId LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SQLiteParameter("@ContainerId", containerId),
                    new SQLiteParameter("@PageSize", 1000),
                    new SQLiteParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveRootLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveRuleLevelRotData> GetDriveRuleLevelRotDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveRuleLevelRotData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);

            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE DriveId = @DriveId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@DriveId", driveId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveRuleLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveRootLevelRotData> GetDriveRootLevelRotDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveRootLevelRotData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE DriveId = @DriveId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@DriveId", driveId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveRootLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveCategoryLevelRotData> GetDriveCategoryLevelRotDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleDriveCategoryLevelRotData));
            var schemaName = RMDiscoveryGoogleSQLiteDBManager.GetSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            using var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM {schemaName}${tableInfo.Name} 
WHERE DriveId = @DriveId {(string.IsNullOrWhiteSpace(sqlConditionalExpression) ? " " : $" AND {sqlConditionalExpression}")} LIMIT @PageSize OFFSET @Offset";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    [
                        .. parameters,
                        new SQLiteParameter("@DriveId", driveId),
                        new SQLiteParameter("@PageSize", 1000),
                        new SQLiteParameter("@Offset", i),
                    ]);

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleDriveCategoryLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryGoogleContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(string googleOrganizationId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleContainerCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleContainerCategoryLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryGoogleContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(string googleOrganizationId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleContainerRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleContainerRootLevelRotData>();
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

        public async IAsyncEnumerable<RMDiscoveryGoogleContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(string googleOrganizationId, int containerId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleContainerRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryGoogleContainerRuleLevelRotData>();
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

        public async Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0) return;
            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleContainerCategoryLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0) return;
            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleContainerRootLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0) return;
            var containerId = dataList.First().ContainerId;
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleContainerRuleLevelRotData] WHERE ContainerId = @ContainerId";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        #endregion

        #region Basic
        public async Task AddOrUpdateBasicCategoryLevelRotDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicCategoryLevelRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleBasicCategoryLevelRotData]";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async Task AddOrUpdateBasicRootLevelRotDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicRootLevelRotData[] dataList)
        {
            if (dataList.Length == 0) return;
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleBasicRootLevelRotData]";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async Task AddOrUpdateBasicRuleLevelRotDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicRuleLevelRotData[] dataList)
        {
            if (dataList.Length == 0) return;
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleBasicRootLevelRotData]";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql);
            await context.ExecuteInsertAsync(dataList.ToList(), schemaName);
        }

        public async Task<List<RMDiscoveryGoogleBasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync(string googleOrganizationId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleBasicCategoryLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryGoogleBasicCategoryLevelRotData>();
        }

        public async Task<List<RMDiscoveryGoogleBasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync(string googleOrganizationId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleBasicRootLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryGoogleBasicRootLevelRotData>();
        }

        public async Task<List<RMDiscoveryGoogleBasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync(string googleOrganizationId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleBasicRuleLevelRotData));
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
            needSelectedColumns = needSelectedColumns.ConvertAll(item => item.Equals("Rule", StringComparison.OrdinalIgnoreCase) ? "[Rule]" : item);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            return dataCollection.ToTableList<RMDiscoveryGoogleBasicRuleLevelRotData>();
        }

        #endregion

        #endregion

        public async Task<RMDiscoveryGoogleAggregateTotalData> GetAggregateTotalDataAsync(string organizationId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(organizationId);
            var res = await efContext.GoogleAggregateTotalDataList.FirstOrDefaultAsync();
            return res ?? new RMDiscoveryGoogleAggregateTotalData();
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(string organizationId, RMDiscoveryGoogleAggregateTotalData data)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(organizationId);
            await AddOrUpdateAggregateTotalDataAsync(efContext, data);
        }

        public async Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryGoogleAggregateTotalData data)
        {
            efContext.GoogleAggregateTotalDataList.AddOrUpdate(data);
            await efContext.SaveChangesAsync();
        }
    }
}
