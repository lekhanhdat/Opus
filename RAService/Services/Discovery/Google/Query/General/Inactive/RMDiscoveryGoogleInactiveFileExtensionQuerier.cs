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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Inactive
{
    public class RMDiscoveryGoogleInactiveFileExtensionQuerier : RMDiscoveryGoogleInactiveDataQuerier<List<RMDiscoveryFileExtensionDataInfo>>
    {
        public RMDiscoveryGoogleInactiveFileExtensionQuerier(RMDiscoveryGoogleQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryAsync()
        {
            var dataTable = GetDataTable();
            var sql = $@"SELECT fileType.Id AS Id, fileType.Name AS Name, SUM(data.FileTotalSize) AS FileTotalSize FROM [{_schemaName}].[{dataTable}] AS data
JOIN [{_schemaName}].[RMGoogleFileExtensions] AS fileType ON data.FileExtension = fileType.Id ";

            sql = AppendAllSqlConditions(sql, out var sqlParams, out _);

            sql += $" GROUP BY fileType.Id, fileType.Name";
            var dataList = await _queryDao.GetDataListAsync<RMDiscoveryFileExtensionDataInfo>(sql, sqlParams);
            dataList.ForEach(item => item.Name = I18NEntity.GetString(item.Name));
            return dataList.OrderByDescending(item => item.FileTotalSize)
                           .Take(20)
                           .ToList();
        }
    }
}
