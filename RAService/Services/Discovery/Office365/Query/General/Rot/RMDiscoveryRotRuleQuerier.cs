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
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot
{
    public class RMDiscoveryRotRuleQuerier : RMDiscoveryRotDataQuerier<List<RMDiscoveryRotRuleDataInfo>>
    {
        public RMDiscoveryRotRuleQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {

        }

        public override async Task<List<RMDiscoveryRotRuleDataInfo>> QueryAsync()
        {
            var dataTable = GetDataTable();
            var sql =
$@"SELECT [rule].Id AS Id, [rule].Name AS Label, [rule].Category AS Category , SUM({DataTableAlias}.FileTotalSize) AS FileTotalSize 
FROM [{_schemaName}].[{dataTable}] AS {DataTableAlias} ";

            sql = AppendAllSqlConditions(sql, out var sqlParams, out _);

            sql += $" AND [rule].IsRemoved = 0 GROUP BY [rule].Id, [rule].Name, [rule].Category";
            var dataList = await _queryDao.GetDataListAsync<RMDiscoveryRotRuleDataInfo>(sql, sqlParams);

            return dataList.OrderByDescending(item => item.FileTotalSize).ToList();
        }
    }
}
