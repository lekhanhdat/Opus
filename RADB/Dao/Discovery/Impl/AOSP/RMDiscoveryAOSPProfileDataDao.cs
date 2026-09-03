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
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Profile;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.AOSP
{
    public class RMDiscoveryAOSPProfileDataDao : IRMDiscoveryAOSPProfileDataDao
    {
        #region Container

        public async Task DeleteContainerInactiveDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId)
        {
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMProfileContainerInactiveData] WHERE ContainerId = @ContainerId";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();

            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
        }

        public async Task AddContainerInactiveDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileContainerInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId, profileId);
        }

        public async IAsyncEnumerable<RMDiscoveryProfileContainerInactiveData> GetContainerInactiveDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryProfileContainerInactiveData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
WHERE ContentSource = @ContentSource ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@ContentSource", contentSource),
                    new SqlParameter("@PageSize", 1000),
                    new SqlParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryProfileContainerInactiveData>();
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

        public async Task DeleteBasicInactiveDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource)
        {
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMProfileBasicInactiveData] WHERE ContentSource = @ContentSource";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();

            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
        }

        public async Task AddBasicInactiveDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileBasicInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId, profileId);
        }

        #endregion

        #region Site

        public async Task DeleteSiteInactiveDataBySiteIdAsync(Guid o365TenantId, Guid profileId, int siteId)
        {
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);

            var sql = $"DELETE FROM [{schemaName}].[RMProfileSiteInactiveData] WHERE SiteId = @SiteId";
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();

            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@SiteId", siteId));
        }

        public async Task AddSiteInactiveDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileSiteInactiveData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId, profileId);
        }

        public async IAsyncEnumerable<RMDiscoveryProfileSiteInactiveData> GetSiteInactiveDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryProfileSiteInactiveData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryProfileSiteInactiveData>();
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

        public async Task DeleteBasicRotDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource)
        {
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMProfileBasicRotData] WHERE ContentSource = @ContentSource";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();

            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContentSource", contentSource));
        }

        public async Task AddBasicRotDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileBasicRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId, profileId);
        }

        #endregion

        #region Container

        public async Task DeleteContainerRotDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId)
        {
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMProfileContainerRotData] WHERE ContainerId = @ContainerId";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();

            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ContainerId", containerId));
        }

        public async Task AddContainerRotDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileContainerRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId, profileId);
        }

        public async IAsyncEnumerable<RMDiscoveryProfileContainerRotData> GetContainerRotDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryProfileContainerRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
            var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).Concat(customColumns.Select(item => item.Name)).ToList();
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = $@"SELECT {string.Join(", ", needSelectedColumns)} FROM [{schemaName}].[{tableInfo.Name}]
WHERE ContentSource = @ContentSource ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            for (var i = 0; ; i += 1000)
            {
                var dataCollection = await context.ExecuteQueryAsync(sql,
                    new SqlParameter("@ContentSource", contentSource),
                    new SqlParameter("@PageSize", 1000),
                    new SqlParameter("@Offset", i));

                var dataList = dataCollection.ToTableList<RMDiscoveryProfileContainerRotData>();
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

        #region Site
        public async Task DeleteSiteRotDataBySiteIdAsync(Guid o365TenantId, Guid profileId, int siteId)
        {
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMProfileSiteRotData] WHERE SiteId = @SiteId";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();

            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@SiteId", siteId));
        }

        public async Task AddSiteRotDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileSiteRotData[] dataList)
        {
            if (dataList.Length == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteAOSPInsertAsync(dataList.ToList(), o365TenantId, profileId);
        }

        public async IAsyncEnumerable<RMDiscoveryProfileSiteRotData> GetSiteRotDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(typeof(RMDiscoveryProfileSiteRotData));
            var schemaName = RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId);
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

                var dataList = dataCollection.ToTableList<RMDiscoveryProfileSiteRotData>();
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
    }
}
