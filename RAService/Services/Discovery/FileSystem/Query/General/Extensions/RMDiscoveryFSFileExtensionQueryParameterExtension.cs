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
using System.Data.SqlClient;
using System.Linq;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Query.General.Extensions
{
    public static class RMDiscoveryFSFileExtensionQueryParameterExtension
    {
        public static bool TryGetSqlDefinition(this RMDiscoveryFSFileExtensionQueryParameter parameter, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;
            if (parameter.FileExtensions == null || !parameter.FileExtensions.Any())
            {
                return false;
            }

            if (parameter.FileExtensions.Count <= 3)
            {
                var parameters = new List<SqlParameter>();
                var conditions = new List<string>();
                for (var i = 0; i < parameter.FileExtensions.Count; i++)
                {
                    var placeholder = "@FileType" + i;
                    conditions.Add($"{dataTableAlias}.FileExtension = {placeholder}");
                    parameters.Add(new SqlParameter(placeholder, parameter.FileExtensions[i]));
                }

                sqlDefinition = new RMDiscoverySqlDefinition
                {
                    ConditionSql = $"({string.Join(" OR ", conditions)})",
                    Parameters = parameters,
                };

                return true;
            }

            var sql = $"{dataTableAlias}.FileExtension IN {DatabaseUtility.BuildInClause(parameter.FileExtensions)}";
            sqlDefinition = new RMDiscoverySqlDefinition { ConditionSql = sql };
            return true;
        }
    }
}
