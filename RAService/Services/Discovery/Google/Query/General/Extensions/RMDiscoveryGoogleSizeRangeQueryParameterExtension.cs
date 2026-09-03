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
using System.Data.SqlClient;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions
{
    public static class RMDiscoveryGoogleSizeRangeQueryParameterExtension
    {
        public static bool TryGetSqlDefinition(this RMDiscoveryGoogleSizeRangeQueryParameter parameter, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;

            var sql = parameter.QueryMode switch
            {
                RMDiscoveryGoogleSizeRangeQueryMode.Range => "SizeRange = @SizeRange",
                RMDiscoveryGoogleSizeRangeQueryMode.LessThanEqual => "SizeRange <= @SizeRange",
                RMDiscoveryGoogleSizeRangeQueryMode.GenerateThanEqual => "SizeRange >= @SizeRange",
                _ => string.Empty,
            };

            if (string.IsNullOrWhiteSpace(sql))
            {
                return false;
            }

            sqlDefinition = new RMDiscoverySqlDefinition
            {
                ConditionSql = $"{dataTableAlias}.{sql}",
                Parameters = new()
                {
                    new SqlParameter("@SizeRange", parameter.SizeRange)
                }
            };

            return true;
        }
    }

}
