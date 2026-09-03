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
using AvePoint.RA.Common.Extension;
using AvePoint.RA.Contract.Discovery.ExportDiscoveryProfile;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.ExportInactiveDataQuerier;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.ExportDataQuerier
{
    public class RMDiscoveryExportDataQuerier : RMExportDataQuerier<RMExportItem>
    {
        public RMDiscoveryExportDataQuerier(ExportDiscoveryProfileParam queryParameter) : base(queryParameter){ }

        public override async Task<RMExportItem> QueryExportDataAsync(List<string> needSumColumns)
        {
            var datas = new List<Dictionary<string, object>>();
            RMExportItem exportItem = new();
            var profileType = RA.Common.Extension.EnumExtension.ToEnum<RMDiscoveryProfileType>(_queryParameter.DiscoveryType);
            switch (profileType)
            {
                case RMDiscoveryProfileType.Inactive:                 
                        datas = await QueryExportInactiveDataAsync(needSumColumns);
                    break;

                case RMDiscoveryProfileType.ROT:                  
                        datas = await QueryExportRotDataAsync(needSumColumns);          
                    break;
            }
        
            exportItem.Items.AddRange(datas);
            return exportItem;
        }

        public async Task<List<Dictionary<string, object>>> QueryExportInactiveDataAsync(List<string> needSumColumns)
        {
            var sqlParameters = new List<SqlParameter>();
            var sql = $@"
                        SELECT 
                            container.Name AS Container,
                            site.Id AS Id,
                            site.Url AS SiteCollection,
                            site.ContentSource,
                            site.PHLTotalSize,
                            siteData.FileTotalSize,
                            siteData.FileSumCount,
                            siteData.InactiveFileTotalSize,
                            siteData.InactiveFileSumCount,                          
                            containerData.FileTotalSize AS ContainerTotalSize,
                            containerData.FileSumCount AS ContainerFileCount
                            {(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"siteData.{item} AS {item}")) : "")}
                        FROM [{_o365TenantSchemaName}].[RMContainerInfoes] AS container
                        LEFT JOIN [{_profileSchemaName}].[RMProfileContainerInactiveData] AS containerData
                            ON container.Id = containerData.ContainerId
                        LEFT JOIN [{_o365TenantSchemaName}].[RMSiteInfoes] AS site
                            ON container.Id = site.ContainerId
                        LEFT JOIN [{_profileSchemaName}].[RMProfileSiteInactiveData] AS siteData
                            ON site.Id = siteData.SiteId";

            sql += $@" ORDER BY siteData.{SecurityUtils.SanitizeSQLParameterName(_queryParameter.SortBy)} {(_queryParameter.IsDescending ? "DESC" : "ASC")} 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            sqlParameters.Add(new("@Offset", _queryParameter.PageIndex * _queryParameter.PageSize));
            sqlParameters.Add(new("@PageSize", _queryParameter.PageSize));

            var rawResult = await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray());
            return rawResult.Where(row => row.ContainsKey("Id") && row["Id"] != null && row["Id"] != DBNull.Value).ToList();
        }

        public async Task<List<Dictionary<string, object>>> QueryExportRotDataAsync(List<string> needSumColumns)
        {
            var sql = $@"
                SELECT 
                    container.Name AS Container,
                    site.Id AS Id,
                    site.Url AS SiteCollection,
                    site.ContentSource AS ContentSource,
                    data.FileTotalSize AS FileTotalSize,
                    data.RotFileTotalSize AS RotFileTotalSize,
                    data.RCategoryFileTotalSize AS RCategoryFileTotalSize,
                    data.OCategoryFileTotalSize AS OCategoryFileTotalSize,
                    data.TCategoryFileTotalSize AS TCategoryFileTotalSize
                    {(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
                FROM [{_o365TenantSchemaName}].[RMContainerInfoes] AS container
                LEFT JOIN [{_o365TenantSchemaName}].[RMSiteInfoes] AS site ON container.Id = site.ContainerId
                LEFT JOIN [{_profileSchemaName}].[RMProfileSiteRotData] AS data ON site.Id = data.SiteId ";

            var sqlParameters = new List<SqlParameter>();
            sql += $@" ORDER BY data.{SecurityUtils.SanitizeSQLParameterName(_queryParameter.SortBy)} {(_queryParameter.IsDescending ? "DESC" : "ASC")}
                   OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            sqlParameters.Add(new("@Offset", _queryParameter.PageIndex * _queryParameter.PageSize));
            sqlParameters.Add(new("@PageSize", _queryParameter.PageSize));

            var rawResult = await _queryDao.GetDataDictionaryListAsync(sql, sqlParameters.ToArray());
            return rawResult.Where(row => row.ContainsKey("Id") && row["Id"] != null && row["Id"] != DBNull.Value).ToList(); ;
        }
    }
}
