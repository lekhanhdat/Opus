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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot
{
    public class RMDiscoveryRotAggregateStatisticQuerier : RMDiscoveryRotDataQuerier<RMDiscoveryOffice365AggregateStatisticDataInfo>
    {
        public RMDiscoveryRotAggregateStatisticQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryAsync()
        {
            var sql = $@"SELECT SUM(data.FileTotalSize) AS FileTotalSize, SUM(data.FileSumCount) AS FileSumCount
FROM [{_schemaName}].[{GetDataTable()}] AS data ";

            sql = AppendAllSqlConditions(sql, out var sqlParams, out _);

            return await _queryDao.GetDataAsync<RMDiscoveryOffice365AggregateStatisticDataInfo>(sql, sqlParams);
        }
    }
}
