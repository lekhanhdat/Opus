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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Salesforce.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Extensions;

public static class RMDiscoverySalesforceNodeQueryParameterExtension
{
    public static bool TryGetSearchKeySqlDefinition(this RMDiscoverySalesforceNodeQueryParameter parameter, string tableAlias, out RMDiscoverySqlDefinition sqlDefinition)
    {
        sqlDefinition = new();
        if (string.IsNullOrWhiteSpace(parameter.SearchKey))
        {
            return false;
        }

        var sql = parameter.ViewMode switch
        {
            RMSFDiscoveryNodeViewMode.Data => $"{tableAlias}.DisplayName LIKE '%'+@SearchKey+'%'",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(sql))
        {
            return false;
        }

        sqlDefinition = new()
        {
            ConditionSql = sql,
            Parameters = new()
            {
                new SqlParameter("@SearchKey", parameter.GetSearchKeySqlValue())
            }
        };

        return true;
    }
    public static bool TryGetSqlDefinition(this RMDiscoverySalesforceNodeQueryParameter parameter, string dbSchemaName, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
    {
        sqlDefinition = null;

        if (parameter.ObjectIds.IsNullOrEmpty())
        {
            return false;
        }
        var sql = $"{dataTableAlias}.ObjectId IN {DatabaseUtility.BuildInClause(parameter.ObjectIds)}";
        sqlDefinition = new()
        {
            ConditionSql = sql,
            Parameters = []
        };
        return true;
    }
    private static string GetSearchKeySqlValue(this RMDiscoverySalesforceNodeQueryParameter parameter)
    {
        var searchKey = parameter.SearchKey;
        searchKey = searchKey.Replace("[", "[[]");
        searchKey = searchKey.Replace("%", "[%]");
        searchKey = searchKey.Replace("_", "[_]");
        return searchKey;
    }
    public static bool TryGetSeletedObjectSqlDefinition(this List<Guid> parameter, string tableAlias, out RMDiscoverySqlDefinition sqlDefinition)
    {
        sqlDefinition = new();
        if (parameter.IsNullOrEmpty())
        {
            return false;
        }

        var sql = $"{tableAlias}.ObjectId IN {DatabaseUtility.BuildInClause(parameter)}";

        sqlDefinition = new()
        {
            ConditionSql = sql,
            Parameters = []
        };

        return true;
    }
    public static bool TryGetObjectIdSqlDefinition(this List<Guid> parameter, string tableAlias, out RMDiscoverySqlDefinition sqlDefinition)
    {
        sqlDefinition = new();
        if (parameter.IsNullOrEmpty())
        {
            return false;
        }

        var sql = $"{tableAlias}.Id IN {DatabaseUtility.BuildInClause(parameter)}";

        sqlDefinition = new()
        {
            ConditionSql = sql,
            Parameters = []
        };

        return true;
    }
}