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
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Profile;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google;

public class RMDiscoveryGoogleProfileDataDao : IRMDiscoveryGoogleProfileDataDao
{
    public async Task DeleteDriveInactiveDataByDriveIdAsync(string googleOrganizationId, Guid profileId, int driveInfoId)
    {
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);

        var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileDriveInactiveData] WHERE DriveId = @DriveId";
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        await using var context = await RMDiscoveryDBManager.GetContextAsync();

        await context.ExecuteNonQueryAsync(sql, new SqlParameter("@DriveId", driveInfoId));
    }

    public async Task AddDriveInactiveDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileDriveInactiveData[] dataList)
    {
        if (dataList.Length == 0)
        {
            return;
        }

        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        await context.ExecuteInsertAsync(dataList.ToList(), googleOrganizationId, profileId);
    }

    public async Task DeleteContainerInactiveDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId)
    {
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileContainerInactiveData] WHERE ContainerId = @ContainerId";
        await using var context = await RMDiscoveryDBManager.GetContextAsync();

        await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));    }

    public async IAsyncEnumerable<RMDiscoveryGoogleProfileDriveInactiveData> GetDriveInactiveDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
    {
        var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleProfileDriveInactiveData));
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
WHERE ContainerId = @ContainerId ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        for (var i = 0; ; i += 1000)
        {
            var dataCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContainerId", containerId),
                new SqlParameter("@PageSize", 1000),
                new SqlParameter("@Offset", i));

            var dataList = dataCollection.ToTableList<RMDiscoveryGoogleProfileDriveInactiveData>();
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

    public async Task AddContainerInactiveDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileContainerInactiveData[] dataList)
    {
        if (dataList.Length == 0)
        {
            return;
        }

        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        await context.ExecuteInsertAsync(dataList.ToList(), googleOrganizationId, profileId);
    }

    public async Task DeleteBasicInactiveDataAsync(string googleOrganizationId, Guid profileId)
    {
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileBasicInactiveData]";
        await using var context = await RMDiscoveryDBManager.GetContextAsync();

        await context.ExecuteNonQueryAsync(sql);
    }

    public async IAsyncEnumerable<RMDiscoveryGoogleProfileContainerInactiveData> GetContainerInactiveDataAsync(string googleOrganizationId, Guid profileId, List<RMDiscoveryCustomColumn> customColumns)
    {
        var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleProfileContainerInactiveData));
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
                                     ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        for (var i = 0; ; i += 1000)
        {
            var dataCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@PageSize", 1000),
                new SqlParameter("@Offset", i));

            var dataList = dataCollection.ToTableList<RMDiscoveryGoogleProfileContainerInactiveData>();
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

    public async Task AddBasicInactiveDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileBasicInactiveData[] dataList)
    {
        if (dataList.Length == 0)
        {
            return;
        }

        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        await context.ExecuteInsertAsync(dataList.ToList(), googleOrganizationId, profileId);
    }

    public async Task DeleteDriveRotDataByDriveIdAsync(string googleOrganizationId, Guid profileId, int driveId)
    {
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileDriveRotData] WHERE DriveId = @DriveId";
        await using var context = await RMDiscoveryDBManager.GetContextAsync();

        await context.ExecuteNonQueryAsync(sql, new SqlParameter("@DriveId", driveId));
    }

    public async Task AddDriveRotDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileDriveRotData[] dataList)
    {
        if (dataList.Length == 0)
        {
            return;
        }

        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        await context.ExecuteInsertAsync(dataList.ToList(), googleOrganizationId, profileId);
    }

    public async Task DeleteContainerRotDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId)
    {
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileContainerRotData] WHERE ContainerId = @ContainerId";
        await using var context = await RMDiscoveryDBManager.GetContextAsync();

        await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
    }

    public async IAsyncEnumerable<RMDiscoveryGoogleProfileDriveRotData> GetDriveRotDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
    {
        var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleProfileDriveRotData));
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
WHERE ContainerId = @ContainerId ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        for (var i = 0; ; i += 1000)
        {
            var dataCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@ContainerId", containerId),
                new SqlParameter("@PageSize", 1000),
                new SqlParameter("@Offset", i));

            var dataList = dataCollection.ToTableList<RMDiscoveryGoogleProfileDriveRotData>();
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

    public async Task AddContainerRotDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileContainerRotData[] dataList)
    {
        if (dataList.Length == 0)
        {
            return;
        }

        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        await context.ExecuteInsertAsync(dataList.ToList(), googleOrganizationId, profileId);
    }

    public async Task DeleteBasicRotDataAsync(string googleOrganizationId, Guid profileId)
    {
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileBasicRotData]";
        await using var context = await RMDiscoveryDBManager.GetContextAsync();
    }

    public async Task AddBasicRotDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileBasicRotData[] dataList)
    {
        if (dataList.Length == 0)
        {
            return;
        }

        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        await context.ExecuteInsertAsync(dataList.ToList(), googleOrganizationId, profileId);
    }

    public async IAsyncEnumerable<RMDiscoveryGoogleProfileContainerRotData> GetContainerRotDataAsync(string googleOrganizationId, Guid profileId, List<RMDiscoveryCustomColumn> customColumns)
    {
        var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryGoogleProfileContainerRotData));
        var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId);
        var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
        SecurityUtils.SanitizeSQLSchemaName(schemaName);
        SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
        await using var context = await RMDiscoveryDBManager.GetContextAsync();
        var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        for (var i = 0; ; i += 1000)
        {
            var dataCollection = await context.ExecuteQueryAsync(sql,
                new SqlParameter("@PageSize", 1000),
                new SqlParameter("@Offset", i));

            var dataList = dataCollection.ToTableList<RMDiscoveryGoogleProfileContainerRotData>();
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
}