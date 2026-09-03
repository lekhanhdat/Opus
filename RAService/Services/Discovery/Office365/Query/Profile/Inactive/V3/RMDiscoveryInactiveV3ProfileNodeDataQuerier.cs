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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.Service.Services.Discovery.Office365.Common;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.Inactive.V3
{
    public class RMDiscoveryInactiveV3ProfileNodeDataQuerier : RMDiscoveryInactiveProfileDataQuerier<RMDiscoveryNodeDataInfo>
    {
        public RMDiscoveryInactiveV3ProfileNodeDataQuerier(RMDiscoveryOffice365ProfileQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
        {
            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();

            var items = await (_queryParameter.NodeQueryParameter.ViewMode switch
            {
                RMDiscoveryNodeViewMode.Container => QueryContainerViewItems(needSumColumns),
                RMDiscoveryNodeViewMode.Site => QuerySiteViewItems(needSumColumns),
                RMDiscoveryNodeViewMode.SiteInContainer => QuerySiteInContainerViewItems(needSumColumns),
                _ => throw new Exception()
            });

            var count = 0;
            if (_queryParameter.NodeQueryParameter.PageIndex == 0)
            {
                count = await (_queryParameter.NodeQueryParameter.ViewMode switch
                {
                    RMDiscoveryNodeViewMode.Container => QueryContainerViewCount(),
                    RMDiscoveryNodeViewMode.Site => QuerySiteViewCount(),
                    RMDiscoveryNodeViewMode.SiteInContainer => QuerySiteInContainerViewCount(),
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
            try
            {

            
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();

            var sql = $@"SELECT 
container.Id AS id,
container.Name AS name,
container.ContentSource AS contentSource,
container.SiteCount AS siteCount,
container.PHLTotalSize AS {DiscoveryConstants.PHL_TOTAL_SIZE_NAME},
data.FileTotalSize AS fileTotalSize,
data.FileSumCount AS fileSumCount,
data.InactiveFileTotalSize AS inactiveFileTotalSize,
data.InactiveFileSumCount AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
FROM [{_o365TenantSchemaName}].[RMContainerInfoes] AS container LEFT JOIN [{_profileSchemaName}].[RMProfileContainerInactiveData] AS data 
ON container.Id = data.ContainerId ";
            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " WHERE container.Name LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            sql += $@" ORDER BY data.{SecurityUtils.SanitizeSQLParameterName(nodeQueryParameter.SortBy)} {(nodeQueryParameter.IsDesc ? "DESC" : "ASC")} 
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            sqlParameters.Add(new("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize));
            sqlParameters.Add(new("@PageSize", nodeQueryParameter.PageSize));

            return await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray());
            }catch(Exception e)
            {
                throw;
            }
        }

        private async Task<List<Dictionary<string, object>>> QuerySiteViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();

            var sql = $@"SELECT 
site.Id AS id,
site.Url AS url,
site.ContentSource AS contentSource,
site.PHLTotalSize AS {DiscoveryConstants.PHL_TOTAL_SIZE_NAME},
data.FileTotalSize AS fileTotalSize,
data.FileSumCount AS fileSumCount,
data.InactiveFileTotalSize AS inactiveFileTotalSize,
data.InactiveFileSumCount AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
FROM [{_o365TenantSchemaName}].[RMSiteInfoes] AS site LEFT JOIN [{_profileSchemaName}].[RMProfileSiteInactiveData] AS data 
ON site.Id = data.SiteId ";
            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " WHERE site.Url LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            sql += $@" ORDER BY data.{SecurityUtils.SanitizeSQLParameterName(nodeQueryParameter.SortBy)} {(nodeQueryParameter.IsDesc ? "DESC" : "ASC")} 
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            sqlParameters.Add(new("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize));
            sqlParameters.Add(new("@PageSize", nodeQueryParameter.PageSize));

            return await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray());
        }

        private async Task<List<Dictionary<string, object>>> QuerySiteInContainerViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();

            var sql = $@"SELECT 
site.Id AS id,
site.Url AS url,
site.ContentSource AS contentSource,
site.PHLTotalSize AS {DiscoveryConstants.PHL_TOTAL_SIZE_NAME},
data.FileTotalSize AS fileTotalSize,
data.FileSumCount AS fileSumCount,
data.InactiveFileTotalSize AS inactiveFileTotalSize,
data.InactiveFileSumCount AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
FROM [{_o365TenantSchemaName}].[RMSiteInfoes] AS site LEFT JOIN [{_profileSchemaName}].[RMProfileSiteInactiveData] AS data 
ON site.Id = data.SiteId 
WHERE site.ContainerId = @ContainerId";

            sqlParameters.Add(new("@ContainerId", nodeQueryParameter.JoinedContainerId));

            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " AND site.Url LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            sql += $@" ORDER BY data.{SecurityUtils.SanitizeSQLParameterName(nodeQueryParameter.SortBy)} {(nodeQueryParameter.IsDesc ? "DESC" : "ASC")} 
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            sqlParameters.Add(new("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize));
            sqlParameters.Add(new("@PageSize", nodeQueryParameter.PageSize));

            return await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray());
        }

        private async Task<int> QueryContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();
            var sql = $@"SELECT COUNT(1) FROM [{_o365TenantSchemaName}].RMContainerInfoes AS container";
            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " WHERE container.Name LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            return await _queryDao.GetDataAsync<int>(sql, sqlParameters.ToArray());
        }

        private async Task<int> QuerySiteViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();
            var sql = $@"SELECT COUNT(1) FROM [{_o365TenantSchemaName}].RMSiteInfoes AS site";
            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " WHERE site.Url LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            return await _queryDao.GetDataAsync<int>(sql, sqlParameters.ToArray());
        }

        private async Task<int> QuerySiteInContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();
            var sql = $@"SELECT COUNT(1) FROM [{_o365TenantSchemaName}].RMSiteInfoes AS site
 WHERE site.ContainerId = @ContainerId";

            sqlParameters.Add(new("@ContainerId", nodeQueryParameter.JoinedContainerId));

            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " AND site.Url LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            return await _queryDao.GetDataAsync<int>(sql, sqlParameters.ToArray());
        }
    }
}
