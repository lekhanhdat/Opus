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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot
{
    public class RMDiscoveryRotOptimizationNodeTotalAggregateQuerier : RMDiscoveryRotDataQuerier<Dictionary<string, object>>
    {
        public RMDiscoveryRotOptimizationNodeTotalAggregateQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var sql = $@"SELECT 
  data.ContentSource AS contentSource,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Redundant} THEN data.FileTotalsize ELSE 0 END) AS redundant,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Obsolete} THEN data.FileTotalsize ELSE 0 END) AS obsolete,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Trivial} THEN data.FileTotalsize ELSE 0 END) AS trivial
FROM [{_schemaName}].[RMBasicRotData] AS data
";

            sql = AppendAllSqlConditions(sql, out var sqlParams, out _);
            sql += " GROUP BY data.ContentSource";

            var list = await _queryDao.GetDataDictionaryListAsync(sql, sqlParams);

            var res = new Dictionary<string, object>
            {
                {$"redundant_{(int)SourceFlag.SharePoint}", 0L},
                {$"obsolete_{(int)SourceFlag.SharePoint}", 0L},
                {$"trivial_{(int)SourceFlag.SharePoint}", 0L},
                {$"redundant_{(int)SourceFlag.OneDrive}", 0L},
                {$"obsolete_{(int)SourceFlag.OneDrive}", 0L},
                {$"trivial_{(int)SourceFlag.OneDrive}", 0L},
                {$"fileTotalSize_{(int)SourceFlag.SharePoint}", 0L },
                {$"fileTotalSize_{(int)SourceFlag.OneDrive}", 0L },
                {$"fileSumCount_{(int)SourceFlag.SharePoint}", 0L },
                {$"fileSumCount_{(int)SourceFlag.OneDrive}", 0L },
            };

            foreach (var contentSourceList in list)
            {
                res[$"redundant_{contentSourceList["contentSource"]}"] = Convert.ToInt64(contentSourceList["redundant"]);
                res[$"obsolete_{contentSourceList["contentSource"]}"] = Convert.ToInt64(contentSourceList["obsolete"]);
                res[$"trivial_{contentSourceList["contentSource"]}"] = Convert.ToInt64(contentSourceList["trivial"]);
            }

            var aggregateDataList = await _queryDao.GetAggregateTotalDataListAsync(_queryParameter.O365TenantId);
            foreach (var aggregateData in aggregateDataList)
            {
                res[$"fileTotalSize_{(int)aggregateData.ContentSource}"] = aggregateData.FileTotalSize;
                res[$"fileSumCount_{(int)aggregateData.ContentSource}"] = aggregateData.FileSumCount;
            }

            return res;
        }
    }
}
