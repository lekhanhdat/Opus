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
using AngleSharp.Common;
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Inactive
{
    public class RMDiscoveryInactiveNodeTotalAggregateQuerier : RMDiscoveryInactiveDataQuerier<Dictionary<string, object>>
    {

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        public RMDiscoveryInactiveNodeTotalAggregateQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();

            var sql = $@"SELECT
data.ContentSource AS contentSource,
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}
FROM [{_schemaName}].[RMBasicInactiveData] as data";

            sql = AppendAllSqlConditions(sql, out var sqlParams, out _);

            sql += " GROUP BY data.ContentSource";

            var list = await _queryDao.GetDataDictionaryListAsync(sql, sqlParams);

            var res = new Dictionary<string, object>
            {
                {$"inactiveFileTotalSize_{(int)SourceFlag.SharePoint}", 0L},
                {$"inactiveFileSumCount_{(int)SourceFlag.SharePoint}", 0L},
                {$"inactiveFileTotalSize_{(int)SourceFlag.OneDrive}", 0L},
                {$"inactiveFileSumCount_{(int)SourceFlag.OneDrive}", 0L},
                {$"fileTotalSize_{(int)SourceFlag.SharePoint}", 0L },
                {$"fileTotalSize_{(int)SourceFlag.OneDrive}", 0L },
                {$"fileSumCount_{(int)SourceFlag.SharePoint}", 0L },
                {$"fileSumCount_{(int)SourceFlag.OneDrive}", 0L },
            };

            foreach (var needSumColumn in needSumColumns)
            {
                res.Add(needSumColumn, 0L);
            }

            foreach (var contentSourceList in list)
            {
                res[$"inactiveFileTotalSize_{contentSourceList["contentSource"]}"] = Convert.ToInt64(contentSourceList["inactiveFileTotalSize"]);
                res[$"inactiveFileSumCount_{contentSourceList["contentSource"]}"] = Convert.ToInt64(contentSourceList["inactiveFileSumCount"]);
                foreach (var needSumColumn in needSumColumns)
                {
                    res[needSumColumn] = Convert.ToInt64(res[needSumColumn]) + Convert.ToInt64(contentSourceList[needSumColumn]);
                }
            }

            var aggregateDataList = await _queryDao.GetAggregateTotalDataListAsync(_queryParameter.O365TenantId);
            foreach (var aggregateData in aggregateDataList)
            {
                res[$"fileTotalSize_{(int)aggregateData.ContentSource}"] = aggregateData.FileTotalSize;
                res[$"fileSumCount_{(int)aggregateData.ContentSource}"] = aggregateData.FileSumCount;
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
            {
                var siteCount = await _nodeDao.CountDiscoverySiteAsync(_queryParameter.O365TenantId);
                res["siteCount"] = siteCount;
            }

            return res;
        }
    }
}
