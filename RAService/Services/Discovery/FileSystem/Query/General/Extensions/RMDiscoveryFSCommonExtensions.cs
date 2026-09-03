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
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AvePoint.RA.Common.Util;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Query.General.Extensions
{
    public static class RMDiscoveryFSCommonExtensions
    {
        public static string BuildFindInSqlCondition(this IEnumerable<string> items, string columnShortName, string columnFullName, int splitSize, out List<SqlParameter> sqlParameters)
        {
            if (splitSize <= 0)
            {
                splitSize = 3;
            }

            string sql = string.Empty;
            sqlParameters = null;
            if (items == null || !items.Any())
            {
                return sql;
            }

            if (items.Count() <= splitSize)
            {
                sqlParameters = new List<SqlParameter>();
                var sqls = new List<string>();
                int idx = 0;
                foreach (var item in items)
                {
                    var placeholder = $"@{columnShortName}{idx}";
                    sqls.Add($"{columnFullName} = {placeholder}");
                    sqlParameters.Add(new SqlParameter(placeholder, item));
                    idx++;
                }

                sql = $"({string.Join(" OR ", sqls)})";
            }
            else
            {
                sql = $@"{columnFullName} IN {DatabaseUtility.BuildInClause(items, out sqlParameters)}";
            }
            return sql;
        }
    }
}
