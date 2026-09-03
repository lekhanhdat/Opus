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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Service.Services.Discovery.Office365.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.Inactive.V3
{
    public class RMDiscoveryInactiveV3ProfileNodeTotalAggregateDataQuerier : RMDiscoveryInactiveProfileDataQuerier<Dictionary<string, object>>
    {
        public RMDiscoveryInactiveV3ProfileNodeTotalAggregateDataQuerier(RMDiscoveryOffice365ProfileQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();

            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sqlParameters = new List<SqlParameter>();
            string sql;

            if (nodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer
                    && nodeQueryParameter.JoinedContainerId > 0)
            {
                sql = QuerySiteInContainerLevel(needSumColumns, nodeQueryParameter.JoinedContainerId, sqlParameters);
            }
            else // Container level and Site level both use the same sql
            {
                sql = QueryContainerLevel(needSumColumns);
            }
            
            var list = await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray());
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
                res[$"fileTotalSize_{contentSourceList["contentSource"]}"] = Convert.ToInt64(contentSourceList["fileTotalSize"]);
                res[$"fileSumCount_{contentSourceList["contentSource"]}"] = Convert.ToInt64(contentSourceList["fileSumCount"]);
                foreach (var needSumColumn in needSumColumns)
                {
                    res[needSumColumn] = Convert.ToInt64(res[needSumColumn]) + Convert.ToInt64(contentSourceList[needSumColumn]);
                }
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
            {
                var siteCount = await _nodeDao.CountDiscoverySiteAsync(_queryParameter.O365TenantId);
                res["siteCount"] = siteCount;
            }
            return res;
        }

        private string QuerySiteInContainerLevel(List<string> needSumColumns, int containerId, List<SqlParameter> sqlParameters)
        {
            sqlParameters.Add(new SqlParameter("@ContainerId", containerId));

            return 
                $@"SELECT 
                    data.ContentSource AS contentSource,
                    data.FileTotalSize AS fileTotalSize, 
                    data.FileSumCount AS fileSumCount, 
                    data.InactiveFileTotalSize AS inactiveFileTotalSize,
                    data.InactiveFileSumCount AS inactiveFileSumCount
                    {(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
                FROM [{_profileSchemaName}].[RMProfileContainerInactiveData] as data
                WHERE data.ContainerId = @ContainerId";
        }

        private string QueryContainerLevel(List<string> needSumColumns)
        {
            return
                $@"SELECT 
                    data.ContentSource AS contentSource,
                    data.FileTotalSize AS fileTotalSize, 
                    data.FileSumCount AS fileSumCount, 
                    data.InactiveFileTotalSize AS inactiveFileTotalSize,
                    data.InactiveFileSumCount AS inactiveFileSumCount
                    {(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
                FROM [{_profileSchemaName}].[RMProfileBasicInactiveData] as data";
        }
    }
}
