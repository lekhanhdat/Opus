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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Inactive
{
    internal class RMDiscoveryGoogleInactiveNodeTotalAggregateQuerier : RMDiscoveryGoogleInactiveDataQuerier<Dictionary<string, object>>
    {

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao = new RMDiscoveryGoogleNodeDao();

        public RMDiscoveryGoogleInactiveNodeTotalAggregateQuerier(RMDiscoveryGoogleQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var sql = $@"SELECT
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}
FROM [{_schemaName}].[RMGoogleBasicInactiveData] as data";

            sql = AppendAllSqlConditions(sql, out var sqlParams, out _);

            var list = await _queryDao.GetDataDictionaryListAsync(sql, sqlParams);

            var res = new Dictionary<string, object>
            {
                {$"inactiveFileTotalSize", 0L},
                {$"inactiveFileSumCount", 0L},
            };

            foreach (var needSumColumn in needSumColumns)
            {
                res.Add(needSumColumn, 0L);
            }


            foreach (var data in list)
            {
                res[$"inactiveFileTotalSize"] = long.TryParse(Convert.ToString(data?["inactiveFileTotalSize"]), out var totalSize) ? totalSize : 0L;
                res[$"inactiveFileSumCount"] = long.TryParse(Convert.ToString(data?["inactiveFileSumCount"]), out var sumCount) ? sumCount : 0L;
            }

            var aggregateDataList = await _queryDao.GetAggregateTotalDataListAsync(_queryParameter.OrganizationId);
            foreach (var aggregateData in aggregateDataList)
            {
                res[$"fileTotalSize"] = aggregateData.FileTotalSize;
                res[$"fileSumCount"] = aggregateData.FileSumCount;
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Container)
            {
                var driveCount = await _nodeDao.CountDiscoveryGoogleDriveAsync(_queryParameter.OrganizationId);
                res["driveCount"] = driveCount;
            }

            return res;
        }
    }
}
