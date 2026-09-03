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
using AvePoint.RA.Contract.Salesforce.Model;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Inactive
{
    public class RMDiscoverySalesforceInactiveAggregateStatisticQuerier(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        : RMDiscoverySalesforceInactiveDataQuerier<RMDiscoverySalesforceAggregateStatisticDataInfo>(salesforceQueryParameter)
    {
        public override async Task<RMDiscoverySalesforceAggregateStatisticDataInfo> QueryAsync()
        {
            var condition = AppendAllSqlConditions("", out var sqlParams, out _);
            var sql = $"SELECT ISNULL((SELECT SUM(TotalCount) FROM [{_schemaName}].[RMSalesforceRecordBasicInactiveData]  AS data {condition}),0) + ISNULL((SELECT SUM(TotalFileCount) FROM [{_schemaName}].[RMSalesforceBasicInactiveData]  AS data {condition}),0) AS RecordsTotalCount," +
                $"ISNULL((SELECT SUM(TotalSize) FROM [{_schemaName}].[RMSalesforceRecordBasicInactiveData]  AS data {condition}),0) AS DataTotalSize," +
                $"ISNULL((SELECT SUM(TotalFileSize) FROM [{_schemaName}].[RMSalesforceBasicInactiveData] AS data {condition}),0) AS FileTotalSize";
            var res = await _dataQueryDao.GetDataAsync<RMDiscoverySalesforceAggregateStatisticDataInfo>(sql, sqlParams);
            return res;
        }
    }
}
