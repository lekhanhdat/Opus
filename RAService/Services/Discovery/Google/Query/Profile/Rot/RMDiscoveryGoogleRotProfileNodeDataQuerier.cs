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
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.Profile.Rot
{
    public class RMDiscoveryGoogleRotProfileNodeDataQuerier : RMDiscoveryGoogleRotProfileDataQuerier<RMDiscoveryNodeDataInfo>
    {
        public RMDiscoveryGoogleRotProfileNodeDataQuerier(RMDiscoveryGoogleProfileQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
        {
            var profileInfo = await _profileDao.GetProfileInfoByIdAsync(_queryParameter.OrganizationId, _queryParameter.ProfileId);
            var ruleIds = JsonConvert.DeserializeObject<HashSet<int>>(profileInfo.RuleIdsJson);

            var rotRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
            rotRules = rotRules.Where(item => ruleIds.Contains(item.Id)).ToList();

            var needSumColumns = rotRules.Select(item => item.ToCustomColumn().Name).ToList();

            var items = await (_queryParameter.NodeQueryParameter.ViewMode switch
            {
                RMDiscoveryGoogleNodeViewMode.Container => QueryContainerViewItems(needSumColumns),
                RMDiscoveryGoogleNodeViewMode.Drive => QueryDriveViewItems(needSumColumns),
                RMDiscoveryGoogleNodeViewMode.DriveInContainer => QueryDriveInContainerViewItems(needSumColumns),
                _ => throw new Exception()
            });

            var count = 0;
            if (_queryParameter.NodeQueryParameter.PageIndex == 0)
            {
                count = await (_queryParameter.NodeQueryParameter.ViewMode switch
                {
                    RMDiscoveryGoogleNodeViewMode.Container => QueryContainerViewCount(),
                    RMDiscoveryGoogleNodeViewMode.Drive => QueryDriveViewCount(),
                    RMDiscoveryGoogleNodeViewMode.DriveInContainer => QueryDriveInContainerViewCount(),
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
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();

            var sql = $@"SELECT 
container.Id AS id,
container.Name AS name,
container.DriveCount AS driveCount,
container.DriveType AS driveType,
data.FileTotalSize AS fileTotalSize,
data.RotFileTotalSize AS rotFileTotalSize,
data.RCategoryFileTotalSize AS rCategoryFileTotalSize,
data.OCategoryFileTotalSize AS oCategoryFileTotalSize,
data.TCategoryFileTotalSize AS tCategoryFileTotalSize
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
FROM [{_googleOrganizationSchemaName}].[RMGoogleContainerInfoes] AS container LEFT JOIN [{_profileSchemaName}].[RMGoogleProfileContainerRotData] AS data 
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
        }

        private async Task<List<Dictionary<string, object>>> QueryDriveViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();

            var sql = $@"SELECT 
drive.Id AS id,
drive.DriveName AS driveName,
drive.DriveType AS driveType,
data.FileTotalSize AS fileTotalSize,
data.RotFileTotalSize AS rotFileTotalSize,
data.RCategoryFileTotalSize AS rCategoryFileTotalSize,
data.OCategoryFileTotalSize AS oCategoryFileTotalSize,
data.TCategoryFileTotalSize AS tCategoryFileTotalSize
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
FROM [{_googleOrganizationSchemaName}].[RMGoogleDriveInfoes] AS drive LEFT JOIN [{_profileSchemaName}].[RMGoogleProfileDriveRotData] AS data 
ON drive.Id = data.DriveId ";
            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " WHERE drive.DriveName LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            sql += $@" ORDER BY data.{SecurityUtils.SanitizeSQLParameterName(nodeQueryParameter.SortBy)} {(nodeQueryParameter.IsDesc ? "DESC" : "ASC")} 
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            sqlParameters.Add(new("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize));
            sqlParameters.Add(new("@PageSize", nodeQueryParameter.PageSize));

            return await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray());
        }

        private async Task<List<Dictionary<string, object>>> QueryDriveInContainerViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();

            var sql = $@"SELECT 
drive.Id AS id,
drive.DriveName AS driveName,
drive.DriveType AS driveType,
data.FileTotalSize AS fileTotalSize,
data.RotFileTotalSize AS rotFileTotalSize,
data.RCategoryFileTotalSize AS rCategoryFileTotalSize,
data.OCategoryFileTotalSize AS oCategoryFileTotalSize,
data.TCategoryFileTotalSize AS tCategoryFileTotalSize
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
FROM [{_googleOrganizationSchemaName}].[RMGoogleDriveInfoes] AS drive LEFT JOIN [{_profileSchemaName}].[RMGoogleProfileDriveRotData] AS data 
ON drive.Id = data.DriveId 
WHERE drive.ContainerId = @ContainerId";

            sqlParameters.Add(new("@ContainerId", nodeQueryParameter.JoinedContainerId));

            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " AND drive.DriveName LIKE '%'+@SearchKey+'%'";
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
            var sql = $@"SELECT COUNT(1) FROM [{_googleOrganizationSchemaName}].RMGoogleContainerInfoes AS container";
            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " WHERE container.Name LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            return await _queryDao.GetDataAsync<int>(sql, sqlParameters.ToArray());
        }

        private async Task<int> QueryDriveViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();
            var sql = $@"SELECT COUNT(1) FROM [{_googleOrganizationSchemaName}].RMGoogleDriveInfoes AS drive";
            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " WHERE drive.DriveName LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            return await _queryDao.GetDataAsync<int>(sql, sqlParameters.ToArray());
        }

        private async Task<int> QueryDriveInContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sqlParameters = new List<SqlParameter>();
            var sql = $@"SELECT COUNT(1) FROM [{_googleOrganizationSchemaName}].RMGoogleDriveInfoes AS drive
 WHERE drive.ContainerId = @ContainerId";

            sqlParameters.Add(new("@ContainerId", nodeQueryParameter.JoinedContainerId));

            if (!string.IsNullOrWhiteSpace(nodeQueryParameter.SearchKey))
            {
                sql += " AND drive.DriveName LIKE '%'+@SearchKey+'%'";
                sqlParameters.Add(new("@SearchKey", nodeQueryParameter.SearchKey));
            }

            return await _queryDao.GetDataAsync<int>(sql, sqlParameters.ToArray());
        }
    }
}
