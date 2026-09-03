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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Inactive
{
    internal class RMDiscoveryGoogleInactiveNodeQuerier : RMDiscoveryGoogleInactiveDataQuerier<RMDiscoveryNodeDataInfo>
    {
        public RMDiscoveryGoogleInactiveNodeQuerier(RMDiscoveryGoogleQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
        {
            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();

            var items = await (_queryParameter.NodeQueryParameter.ViewMode switch
            {
                RMDiscoveryGoogleNodeViewMode.Container => QueryContainerViewItems(needSumColumns),
                RMDiscoveryGoogleNodeViewMode.Drive => QueryDriveViewItems(needSumColumns),
                RMDiscoveryGoogleNodeViewMode.DriveInContainer => QueryDriveInContainerViewItems(needSumColumns),
                _ => throw new Exception()
            });

            var count = 0;
            if (_queryParameter.NodeQueryParameter.PageIndex == 0)
            {
                count = await (_queryParameter.NodeQueryParameter.ViewMode switch
                {
                    RMDiscoveryGoogleNodeViewMode.Container => QueryContainerViewCount(),
                    RMDiscoveryGoogleNodeViewMode.Drive => QueryDriveViewCount(),
                    RMDiscoveryGoogleNodeViewMode.DriveInContainer => QueryDriveInContainerViewCount(),
                    _ => throw new Exception()
                });
            }

            return new RMDiscoveryNodeDataInfo
            {
                Count = count,
                Items = items,
            };
        }

        private async Task<List<Dictionary<string, object>>> QueryContainerViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var containersSql = $@"SELECT Id, Name, FileTotalSize, FileSumCount, DriveCount
FROM [{_schemaName}].[RMGoogleContainerInfoes] AS container ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("container", out var searchKeySqlDefinition))
            {
                containersSql += " WHERE " + searchKeySqlDefinition.ConditionSql;
            }

            containersSql += $@"order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var containers = await _queryDao.GetDataListAsync<RMDiscoveryGoogleContainerInfo>(containersSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                }).ToArray());

            var sql = $@"SELECT 
container.Id AS id,
container.Name AS name,
container.DriveCount AS driveCount,
container.FileTotalSize AS fileTotalSize,
container.FileSumCount AS fileSumCount,
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}";

            var inClauseParamName = DatabaseUtility.BuildInClause(containers.Select(item => item.Id), out var paramList);

            sql += $@" FROM [{_schemaName}].[RMGoogleContainerInfoes] AS container
LEFT JOIN [{_schemaName}].[RMGoogleContainerInactiveData] AS data
ON container.Id = data.ContainerId
WHERE container.Id IN {inClauseParamName}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");
            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            sql += " GROUP BY container.Id, container.Name, container.DriveCount, container.FileTotalSize, container.FileSumCount";

            paramList.AddRange(conditionSqlDefinitions.SelectMany(item => item.Parameters));

            var items = await _queryDao.GetDataDictionaryListAsync(sql, paramList.ToArray());

            foreach (var container in containers)
            {
                if (!items.Any(item => container.Id.ToString() == item["id"].ToString()))
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = container.Id,
                        ["name"] = container.Name,
                        ["driveCount"] = container.DriveCount,
                        ["fileTotalSize"] = container.FileTotalSize,
                        ["fileSumCount"] = container.FileSumCount,
                        ["inactiveFileTotalSize"] = 0,
                        ["inactiveFileSumCount"] = 0,
                    };

                    needSumColumns.ForEach(column => itemDic[column] = 0);

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<List<Dictionary<string, object>>> QueryDriveViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var drivesSql = $@"SELECT Id, Url, ContentSource, FileTotalSize, FileSumCount
FROM [{_schemaName}].[RMGoogleDriveInfoes] AS drive WHERE drive.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("drive", out var searchKeySqlDefinition))
            {
                drivesSql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            drivesSql += $@"order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var drives = await _queryDao.GetDataListAsync<RMDiscoveryGoogleDriveInfo>(drivesSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                }).ToArray());

            var sql = $@"SELECT 
drive.Id AS id,
drive.DriveName AS name,
drive.FileTotalSize AS fileTotalSize,
drive.FileSumCount AS fileSumCount,
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}";

            sql += $@" FROM [{_schemaName}].[RMGoogleDriveInfoes] AS drive
LEFT JOIN [{_schemaName}].[RMGoogleDriveInactiveData] AS data
ON drive.Id = data.DriveId
WHERE drive.Id IN {DatabaseUtility.BuildInClause(drives.Select(item => item.Id))}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");
            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            sql += " GROUP BY drive.Id, drive.DriveName, drive.ContentSource, drive.FileTotalSize, drive.FileSumCount";
            var items = await _queryDao.GetDataDictionaryListAsync(sql, conditionSqlDefinitions.SelectMany(item => item.Parameters).ToArray());

            foreach (var drive in drives)
            {
                if (!items.Any(item => drive.Id.ToString() == item["id"].ToString()))
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = drive.Id,
                        ["name"] = drive.DriveName,
                        //["contentSource"] = drive.ContentSource,
                        ["fileTotalSize"] = drive.FileTotalSize,
                        ["fileSumCount"] = drive.FileSumCount,
                        ["inactiveFileTotalSize"] = 0,
                        ["inactiveFileSumCount"] = 0,
                    };

                    needSumColumns.ForEach(column => itemDic[column] = 0);

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<List<Dictionary<string, object>>> QueryDriveInContainerViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var drivesSql = $@"SELECT Id, DriveName, FileTotalSize, FileSumCount
FROM [{_schemaName}].[RMGoogleDriveInfoes] AS drive WHERE drive.ContainerId = @ContainerId AND drive.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("drive", out var searchKeySqlDefinition))
            {
                drivesSql += " And " + searchKeySqlDefinition.ConditionSql;
            }

            drivesSql += $@"order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var drives = await _queryDao.GetDataListAsync<RMDiscoveryGoogleDriveInfo>(drivesSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize),
                    new SqlParameter("@ContainerId", nodeQueryParameter.JoinedContainerId)
                }).ToArray());

            var sql = $@"SELECT 
drive.Id AS id,
drive.DriveName AS name,
drive.ContentSource AS contentSource,
drive.FileTotalSize AS fileTotalSize,
drive.FileSumCount AS fileSumCount,
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}";

            sql += $@" FROM [{_schemaName}].[RMGoogleDriveInfoes] AS drive
LEFT JOIN [{_schemaName}].[RMGoogleDriveInactiveData] AS data
ON drive.Id = data.DriveId
WHERE drive.Id IN {DatabaseUtility.BuildInClause(drives.Select(item => item.Id))}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");
            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            sql += " GROUP BY drive.Id, drive.DriveName, drive.ContentSource, drive.FileTotalSize, drive.FileSumCount";
            var items = await _queryDao.GetDataDictionaryListAsync(sql, conditionSqlDefinitions.SelectMany(item => item.Parameters).ToArray());

            foreach (var drive in drives)
            {
                if (!items.Any(item => drive.Id.ToString() == item["id"].ToString()))
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = drive.Id,
                        ["driveName"] = drive.DriveName,
                        ["fileTotalSize"] = drive.FileTotalSize,
                        ["fileSumCount"] = drive.FileSumCount,
                        ["inactiveFileTotalSize"] = 0,
                        ["inactiveFileSumCount"] = 0,
                    };

                    needSumColumns.ForEach(column => itemDic[column] = 0);

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<int> QueryContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMGoogleContainerInfoes] AS container";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("container", out var searchKeySqlDefinition))
            {
                sql += " WHERE " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
        }

        private async Task<int> QueryDriveViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMGoogleDriveInfoes] AS drive WHERE drive.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("drive", out var searchKeySqlDefinition))
            {
                sql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
        }

        private async Task<int> QueryDriveInContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMGoogleDriveInfoes] AS drive
WHERE drive.ContainerId = @ContainerId AND drive.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("drive", out var searchKeySqlDefinition))
            {
                sql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters
                .Concat(new List<SqlParameter> { new SqlParameter("@ContainerId", nodeQueryParameter.JoinedContainerId) }).ToArray());
        }

        protected List<RMDiscoverySqlDefinition> GetConditionSqlDefinitionWithoutNode(string tableAlias)
        {
            var res = new List<RMDiscoverySqlDefinition>();

            if (_queryParameter.FileExtensionQueryParameter != null &&
                _queryParameter.FileExtensionQueryParameter.TryGetSqlDefinition(tableAlias, out var sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.SizeRangeQueryParameter != null &&
                _queryParameter.SizeRangeQueryParameter.TryGetSqlDefinition(tableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.WithoutDateQueryParameter != null &&
                _queryParameter.WithoutDateQueryParameter.TryGetSqlDefinition(tableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            return res;
        }
    }
}
