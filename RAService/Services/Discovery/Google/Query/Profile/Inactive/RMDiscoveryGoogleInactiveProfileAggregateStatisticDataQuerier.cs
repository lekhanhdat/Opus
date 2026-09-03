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
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.Profile.Inactive
{
    public class RMDiscoveryGoogleInactiveProfileAggregateStatisticDataQuerier : RMDiscoveryGoogleInactiveProfileDataQuerier<Dictionary<string, object>>
    {
        public RMDiscoveryGoogleInactiveProfileAggregateStatisticDataQuerier(RMDiscoveryGoogleProfileQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();
            var sql = $@"SELECT SUM(data.InactiveFileTotalSize) AS optimizableFileTotalSize,
SUM(data.InactiveFileSumCount) AS optimizableFileSumCount FROM ";

            if (nodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Container
                && nodeQueryParameter.ContainerIds != null && nodeQueryParameter.ContainerIds.Count > 0)
            {
                var inSql = DatabaseUtility.BuildInClause(nodeQueryParameter.ContainerIds, out var parameters);
                sql += $@"[{_profileSchemaName}].[RMGoogleProfileContainerInactiveData] AS data 
WHERE data.ContainerId IN {inSql}";
                sqlParameters.AddRange(parameters);
            }
            else if (nodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Drive
                && nodeQueryParameter.DriveIds != null && nodeQueryParameter.DriveIds.Count > 0)
            {
                var inSql = DatabaseUtility.BuildInClause(nodeQueryParameter.DriveIds, out var parameters);
                sql += $@"[{_profileSchemaName}].[RMGoogleProfileDriveInactiveData] AS data 
WHERE data.DriveId IN {inSql}";
                sqlParameters.AddRange(parameters);
            }
            else if (nodeQueryParameter.ViewMode == RMDiscoveryGoogleNodeViewMode.DriveInContainer)
            {
                if (nodeQueryParameter.DriveIds != null && nodeQueryParameter.DriveIds.Count > 0)
                {
                    var inSql = DatabaseUtility.BuildInClause(nodeQueryParameter.DriveIds, out var parameters);
                    sql += $@"[{_profileSchemaName}].[RMGoogleProfileDriveInactiveData] AS data 
WHERE data.DriveId IN {inSql}";
                    sqlParameters.AddRange(parameters);
                }
                else
                {
                    sql += $@"[{_profileSchemaName}].[RMGoogleProfileContainerInactiveData] AS data 
WHERE data.ContainerId = @ContainerId";
                    sqlParameters.Add(new("@ContainerId", nodeQueryParameter.JoinedContainerId));
                }
            }
            else
            {
                sql += $"[{_profileSchemaName}].[RMGoogleProfileBasicInactiveData] AS data";
            }

            var res = (await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray())).First();

            var totalSql = $"SELECT SUM(data.FileTotalSize) AS fileTotalSize FROM [{_googleOrganizationSchemaName}].[RMGoogleAggregateTotalData] AS data";
            var totalSize = await _queryDao.GetDataAsync<long>(totalSql);
            res.Add("fileTotalSize", totalSize);

            return res;
        }
    }
}
