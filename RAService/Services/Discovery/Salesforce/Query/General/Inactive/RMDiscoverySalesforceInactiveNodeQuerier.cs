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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Inactive;

public class RMDiscoverySalesforceInactiveNodeQuerier(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
    : RMDiscoverySalesforceInactiveDataQuerier<RMDiscoveryNodeDataInfo>(salesforceQueryParameter)
{
    public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
    {
        var items = await (SalesforceQueryParameter.NodeQueryParameter.ViewMode switch
        {
            RMSFDiscoveryNodeViewMode.Data => QueryDataViewItems(),
            RMSFDiscoveryNodeViewMode.File => QueryFileViewItems(),
            _ => throw new Exception()
        });
        var count = await (SalesforceQueryParameter.NodeQueryParameter.ViewMode switch
        {
            RMSFDiscoveryNodeViewMode.Data => QueryDataViewCount(),
            RMSFDiscoveryNodeViewMode.File => QueryFileViewCount(),
            _ => throw new Exception()
        });
        return new RMDiscoveryNodeDataInfo
        {
            Count = count,
            Items = items,
        };
    }

    private async Task<int> QueryFileViewCount()
    {
        var nodeQueryParameter = SalesforceQueryParameter.NodeQueryParameter;
        var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMSalesforceObjectInfoData] AS fileData  WHERE ObjectType in (2,3)";
        if (nodeQueryParameter.TryGetSearchKeySqlDefinition("fileData", out var searchKeySqlDefinition))
        {
            sql += " AND " + searchKeySqlDefinition.ConditionSql;
        }

        return await _dataQueryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
    }

    private async Task<int> QueryDataViewCount()
    {
        var selectedObjectIds = SalesforceQueryParameter.SelectedObjectIds;
        var nodeQueryParameter = SalesforceQueryParameter.NodeQueryParameter;
        var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMSalesforceObjectInfoData] AS recordData  WHERE ObjectType in (0,1)";
        if (selectedObjectIds.IsNotNullOrEmpty() && selectedObjectIds.TryGetObjectIdSqlDefinition("recordData", out var selectedObjectIdsSqlDefinition))
        {
            sql += "AND " + selectedObjectIdsSqlDefinition.ConditionSql;
        }
        if (nodeQueryParameter.TryGetSearchKeySqlDefinition("recordData", out var searchKeySqlDefinition))
        {
            sql += " AND " + searchKeySqlDefinition.ConditionSql;
        }

        return await _dataQueryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
    }

    private async Task<List<Dictionary<string, object>>> QueryDataViewItems()
    {
        var selectedObjectIds = SalesforceQueryParameter.SelectedObjectIds;
        var nodeQueryParameter = SalesforceQueryParameter.NodeQueryParameter;

        var fileSql = $@"SELECT Id, DisplayName, ObjectType, TotalItemCount, TotalSize
FROM [{_schemaName}].[RMSalesforceObjectInfoData] AS data WHERE ObjectType in (0,1) ";
        if (selectedObjectIds.IsNotNullOrEmpty() && selectedObjectIds.TryGetObjectIdSqlDefinition("data", out var selectedObjectIdsSqlDefinition))
        {
            fileSql += "AND " + selectedObjectIdsSqlDefinition.ConditionSql;
        }
        if (nodeQueryParameter.TryGetSearchKeySqlDefinition("data", out var searchKeySqlDefinition))
        {
            fileSql += "AND " + searchKeySqlDefinition.ConditionSql;
        }

        fileSql += $@"order BY DisplayName
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
        var files = await _dataQueryDao.GetDataListAsync<RMDiscoverySalesforceObjectInfo>(fileSql,
            searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
            {
                new ("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                new ("@PageSize", nodeQueryParameter.PageSize)
            }).ToArray());
        var sql = $@"  SELECT 
recordData.Id AS id,
recordData.DisplayName AS displayName,
recordData.ObjectType AS objectType,
recordData.TotalItemCount AS totalItemCount,
recordData.TotalSize AS totalSize,
ISNUll(SUM(data.TotalCount), 0) AS inactiveSumCount,
ISNULL(SUM(data.TotalSize), 0) AS inactiveTotalSize,
case WHEN recordData.TotalItemCount > 0 THEN
        ISNUll(CAST(SUM(data.TotalCount) AS FLOAT) / CAST(recordData.TotalItemCount AS FLOAT) * 100, 0)
    ELSE
        0.0 
    END as inactiveCountOfTotal,
case WHEN recordData.TotalSize > 0 THEN
        ISNULL(CAST(SUM(data.TotalSize) AS FLOAT) / CAST(recordData.TotalSize AS FLOAT) * 100,0)
    ELSE
        0.0 
    END as inactiveSizeOfTotal";

        var inClauseParamName = DatabaseUtility.BuildInClause(files.Select(item => item.Id), out var paramList);

        sql += $@" FROM [{_schemaName}].[RMSalesforceObjectInfoData] AS recordData
LEFT JOIN [{_schemaName}].[RMSalesforceRecordBasicInactiveData] AS data
ON data.ObjectId = recordData.Id
WHERE recordData.Id IN {inClauseParamName}";
        var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data", RMSFDiscoveryNodeViewMode.Data);
        if (conditionSqlDefinitions.Any())
        {
            sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
        }

        sql += " GROUP BY recordData.Id, recordData.ObjectType, recordData.DisplayName, recordData.TotalItemCount, recordData.TotalSize order BY displayName";
        paramList.AddRange(conditionSqlDefinitions.SelectMany(item => item.Parameters));
        var items = await _dataQueryDao.GetDataDictionaryListAsync(sql, paramList.ToArray());
        foreach (var file in files)
        {
            if (!items.Any(item => file.Id.ToString() == item["id"].ToString()))
            {
                var itemDic = new Dictionary<string, object>
                {
                    ["id"] = file.Id,
                    ["displayName"] = file.DisplayName,
                    ["totalItemCount"] = file.TotalItemCount,
                    ["totalSize"] = file.TotalSize,
                    ["inactiveDataTotalSize"] = 0,
                    ["inactiveDataSumCount"] = 0,
                    ["inactiveTotalSize"] = 0,
                    ["inactiveSumCount"] = 0,
                };
                items.Add(itemDic);
            }
        }

        return items.OrderBy(item => item.ContainsKey("displayName") ? item["displayName"].ToString() : string.Empty)
            .ToList();
    }

    private async Task<List<Dictionary<string, object>>> QueryFileViewItems()
    {
        var nodeQueryParameter = SalesforceQueryParameter.NodeQueryParameter;

        var fileSql = $@"SELECT Id, DisplayName, ObjectType, TotalItemCount, TotalSize
FROM [{_schemaName}].[RMSalesforceObjectInfoData] AS fileData WHERE ObjectType in (2,3) ";
        if (nodeQueryParameter.TryGetSearchKeySqlDefinition("fileData", out var searchKeySqlDefinition))
        {
            fileSql += "AND " + searchKeySqlDefinition.ConditionSql;
        }

        fileSql += $@"order BY DisplayName
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
        var files = await _dataQueryDao.GetDataListAsync<RMDiscoverySalesforceObjectInfo>(fileSql,
            searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
            {
                new ("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                new ("@PageSize", nodeQueryParameter.PageSize)
            }).ToArray());
        var sql = $@"SELECT 
fileData.Id AS id,
fileData.DisplayName AS displayName,
fileData.ObjectType AS objectType,
fileData.TotalItemCount AS totalItemCount,
fileData.TotalSize AS totalSize,
ISNULL(SUM(data.TotalFileSize),0) AS inactiveTotalSize,
ISNULL(SUM(data.TotalFileCount), 0) AS inactiveSumCount,
case WHEN fileData.TotalItemCount > 0 THEN
        ISNULL(CAST(SUM(data.TotalFileCount) AS FLOAT) / CAST(fileData.TotalItemCount AS FLOAT) * 100, 0)
    ELSE
        0.0 
    END as inactiveCountOfTotal,
case WHEN fileData.TotalSize > 0 THEN
        ISNULL(CAST(SUM(data.TotalFileSize) AS FLOAT) / CAST(fileData.TotalSize AS FLOAT) * 100, 0)
    ELSE
        0.0 
    END as inactiveSizeOfTotal";

        var inClauseParamName = DatabaseUtility.BuildInClause(files.Select(item => item.Id), out var paramList);

        sql += $@" FROM [{_schemaName}].[RMSalesforceObjectInfoData] AS fileData
LEFT JOIN [{_schemaName}].[RMSalesforceBasicInactiveData] AS data
ON fileData.Id = data.ObjectId
WHERE fileData.Id IN {inClauseParamName}";
        var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data", RMSFDiscoveryNodeViewMode.File);
        if (conditionSqlDefinitions.Any())
        {
            sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
        }

        sql += " GROUP BY fileData.Id, fileData.ObjectType, fileData.DisplayName, fileData.TotalItemCount, fileData.TotalSize order BY displayName";
        paramList.AddRange(conditionSqlDefinitions.SelectMany(item => item.Parameters));
        var items = await _dataQueryDao.GetDataDictionaryListAsync(sql, paramList.ToArray());
        foreach (var file in files)
        {
            if (!items.Any(item => file.Id.ToString() == item["id"].ToString()))
            {
                var itemDic = new Dictionary<string, object>
                {
                    ["id"] = file.Id,
                    ["displayName"] = file.DisplayName,
                    ["totalItemCount"] = file.TotalItemCount,
                    ["totalSize"] = file.TotalSize,
                    ["inactiveDataTotalSize"] = 0,
                    ["inactiveDataSumCount"] = 0,
                    ["inactiveTotalSize"] = 0,
                    ["inactiveSumCount"] = 0,
                };
                items.Add(itemDic);
            }
        }

        return items.OrderBy(item => item.ContainsKey("displayName") ? item["displayName"].ToString() : string.Empty)
            .ToList();
    }
    protected List<RMDiscoverySqlDefinition> GetConditionSqlDefinitionWithoutNode(string tableAlias, RMSFDiscoveryNodeViewMode dataType)
    {
        var res = new List<RMDiscoverySqlDefinition>();

        if (SalesforceQueryParameter.WithoutDateQueryParameter != null &&
            SalesforceQueryParameter.WithoutDateQueryParameter.TryGetSFSqlDefinition(tableAlias, out var sqlDefinition))
        {
            res.Add(sqlDefinition);
        }
        if (dataType == RMSFDiscoveryNodeViewMode.File) ConditionRMSFBasicInactiveDataTable(res);
        return res;
    }
}