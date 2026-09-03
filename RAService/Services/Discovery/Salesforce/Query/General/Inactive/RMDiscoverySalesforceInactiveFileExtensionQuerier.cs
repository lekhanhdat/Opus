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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Inactive;

public class RMDiscoverySalesforceInactiveFileExtensionQuerier(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
    : RMDiscoverySalesforceInactiveDataQuerier<List<RMDiscoveryFileExtensionDataInfo>>(salesforceQueryParameter)
{
    public override async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryAsync()
    {
        var dataTable = GetDataTable(RMSFDiscoveryNodeViewMode.File);
        var sql = $@"SELECT Top 20 Lower(data.FileExtension) AS Name, SUM(data.TotalFileSize) AS FileTotalSize FROM [{_schemaName}].[{dataTable}] as data";

        sql = AppendAllSqlConditions(sql, out var sqlParams, out _, RMSFDiscoveryNodeViewMode.File);

        sql += $" GROUP BY Lower(FileExtension) Order By FileTotalSize desc";
        var dataList = await _dataQueryDao.GetDataListAsync<RMDiscoveryFileExtensionDataInfo>(sql, sqlParams);
        dataList.ForEach(item =>
        {
            item.Name = I18NEntity.GetString(item.Name == RMConstants.UNKNOW ? "Empty" : item.Name);
        });
        return dataList.OrderByDescending(item => item.FileTotalSize).ToList();
    }
}