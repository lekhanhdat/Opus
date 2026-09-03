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
using System.Threading.Tasks;
using AvePoint.RA.Contract.Salesforce.Model;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Inactive;

public class RMDiscoverySalesforceInactiveNodeTotalAggregateQuerier(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
    : RMDiscoverySalesforceInactiveDataQuerier<Dictionary<string, object>>(salesforceQueryParameter)
{
    public override async Task<Dictionary<string, object>> QueryAsync()
    {
        if (salesforceQueryParameter.NodeQueryParameter.ViewMode is RMSFDiscoveryNodeViewMode.Data)
        {
            return await GetRecordTotalAggrgate();
        }
        return await GetFileTotalAggrgate();
    }

    private async Task<Dictionary<string, object>> GetFileTotalAggrgate()
    {
        var sql = $@"SELECT
ISNULL(SUM(data.TotalFileSize),0) AS inactiveTotalSize,
ISNULL(SUM(data.TotalFileCount),0) AS inactiveFileSumCount
FROM [{_schemaName}].[RMSalesforceBasicInactiveData] as data";
        sql = AppendAllSqlConditions(sql, out var sqlParams, out _, RMSFDiscoveryNodeViewMode.File);
        var list = await _dataQueryDao.GetDataDictionaryListAsync(sql, sqlParams);

        var res = new Dictionary<string, object>
        {
            {"inactiveSumCount", 0L},
            {"totalItemCount", 0L },
            {"inactiveCountOfTotal", 0L },
            {"inactiveTotalSize", 0L},
            {"totalSize", 0L },
            {"inactiveSizeOfTotal", 0L },
        };
        foreach (var dataObjerct in list)
        {
            res["inactiveSumCount"] = Convert.ToInt64(dataObjerct["inactiveFileSumCount"]);
            res["inactiveTotalSize"] = Convert.ToInt64(dataObjerct["inactiveTotalSize"]);
        }
        var sqlInObjectTable = $@"SELECT
SUM(data.TotalSize) AS DataTotalSize,
SUM(data.TotalItemCount) AS FilesSumCount
FROM [{_schemaName}].[RMSalesforceObjectInfoData] as data Where ObjectType in (2,3)";
        var listObjectTable = await _dataQueryDao.GetDataDictionaryListAsync(sqlInObjectTable);
        foreach (var dataObject in listObjectTable)
        {
            res["totalItemCount"] = Convert.ToInt64(dataObject["FilesSumCount"]);
            res["totalSize"] = Convert.ToInt64(dataObject["DataTotalSize"]);
        }
        res["inactiveCountOfTotal"] = (long)res["totalItemCount"] == 0 ? 0 : Math.Round((long)res["inactiveSumCount"] / (float)(long)res["totalItemCount"] * 100, MidpointRounding.AwayFromZero);
        res["inactiveSizeOfTotal"] = (long)res["totalSize"] == 0 ? 0 : Math.Round((long)res["inactiveTotalSize"] / (float)(long)res["totalSize"] * 100, MidpointRounding.AwayFromZero);
        return res;
    }

    private async Task<Dictionary<string, object>> GetRecordTotalAggrgate()
    {
        var sql = $@"SELECT
ISNULL(SUM(data.TotalSize),0) AS inactiveTotalSize,
ISNULL(SUM(data.TotalCount),0) AS inactiveRecordsSumCount
FROM [{_schemaName}].[RMSalesforceRecordBasicInactiveData] as data";
        sql = AppendAllSqlConditions(sql, out var sqlParams, out _, RMSFDiscoveryNodeViewMode.Data);
        var list = await _dataQueryDao.GetDataDictionaryListAsync(sql, sqlParams);

        var res = new Dictionary<string, object>
        {
            {"inactiveSumCount", 0L},
            {"totalItemCount", 0L },
            {"inactiveCountOfTotal", 0L },
            {"inactiveTotalSize", 0L},
            {"totalSize", 0L },
            {"inactiveSizeOfTotal", 0L },
        };
        foreach (var dataObjerct in list)
        {
            res["inactiveSumCount"] = Convert.ToInt64(dataObjerct["inactiveRecordsSumCount"]);
            res["inactiveTotalSize"] = Convert.ToInt64(dataObjerct["inactiveTotalSize"]);
        }
        var sqlInObjectTable = $@"SELECT
SUM(data.TotalSize) AS DataTotalSize,
SUM(data.TotalItemCount) AS RecordsSumCount
FROM [{_schemaName}].[RMSalesforceObjectInfoData] as data Where ObjectType in (0,1) ";
        sqlInObjectTable = AppendAllSqlConditions(sqlInObjectTable, out var sqlObjectTableParams, out _, RMSFDiscoveryNodeViewMode.None);
        var listObjectTable = await _dataQueryDao.GetDataDictionaryListAsync(sqlInObjectTable, sqlObjectTableParams);
        foreach (var dataObject in listObjectTable)
        {
            res["totalItemCount"] = Convert.ToInt64(dataObject["RecordsSumCount"]);
            res["totalSize"] = Convert.ToInt64(dataObject["DataTotalSize"]);
        }

        res["inactiveCountOfTotal"] = (long)res["totalItemCount"] == 0 ? 0 : Math.Round((long)res["inactiveSumCount"] / (float)(long)res["totalItemCount"] * 100, MidpointRounding.AwayFromZero);
        res["inactiveSizeOfTotal"] = (long)res["totalSize"] == 0 ? 0 : Math.Round((long)res["inactiveTotalSize"] / (float)(long)res["totalSize"] * 100, MidpointRounding.AwayFromZero);
        return res;
    }
}