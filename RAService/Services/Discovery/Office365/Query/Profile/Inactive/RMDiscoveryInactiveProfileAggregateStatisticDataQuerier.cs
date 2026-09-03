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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter.Profile;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.Inactive
{
    public class RMDiscoveryInactiveProfileAggregateStatisticDataQuerier : RMDiscoveryInactiveProfileDataQuerier<Dictionary<string, object>>
    {
        public RMDiscoveryInactiveProfileAggregateStatisticDataQuerier(RMDiscoveryOffice365ProfileQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();
            var sql = $@"SELECT SUM(data.InactiveFileTotalSize) AS optimizableFileTotalSize,
SUM(data.InactiveFileSumCount) AS optimizableFileSumCount FROM ";

            if (nodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container
                && nodeQueryParameter.ContainerIds != null && nodeQueryParameter.ContainerIds.Count > 0)
            {
                var inSql = DatabaseUtility.BuildInClause(nodeQueryParameter.ContainerIds, out var parameters);
                sql += $@"[{_profileSchemaName}].[RMProfileContainerInactiveData] AS data 
WHERE data.ContainerId IN {inSql}";
                sqlParameters.AddRange(parameters);
            }
            else if (nodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Site
                && nodeQueryParameter.SiteIds != null && nodeQueryParameter.SiteIds.Count > 0)
            {
                var inSql = DatabaseUtility.BuildInClause(nodeQueryParameter.SiteIds, out var parameters);
                sql += $@"[{_profileSchemaName}].[RMProfileSiteInactiveData] AS data 
WHERE data.SiteId IN {inSql}";
                sqlParameters.AddRange(parameters);
            }
            else if (nodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer)
            {
                if (nodeQueryParameter.SiteIds != null && nodeQueryParameter.SiteIds.Count > 0)
                {
                    var inSql = DatabaseUtility.BuildInClause(nodeQueryParameter.SiteIds, out var parameters);
                    sql += $@"[{_profileSchemaName}].[RMProfileSiteInactiveData] AS data 
WHERE data.SiteId IN {inSql}";
                    sqlParameters.AddRange(parameters);
                }
                else
                {
                    sql += $@"[{_profileSchemaName}].[RMProfileContainerInactiveData] AS data 
WHERE data.ContainerId = @ContainerId";
                    sqlParameters.Add(new("@ContainerId", nodeQueryParameter.JoinedContainerId));
                }
            }
            else
            {
                sql += $"[{_profileSchemaName}].[RMProfileBasicInactiveData] AS data";
            }

            var res = (await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray())).First();

            var totalSql = $"SELECT SUM(data.FileTotalSize) AS fileTotalSize FROM [{_o365TenantSchemaName}].[RMAggregateTotalData] AS data";
            var totalSize = await _queryDao.GetDataAsync<long>(totalSql);
            res.Add("fileTotalSize", totalSize);

            return res;
        }
    }
}
