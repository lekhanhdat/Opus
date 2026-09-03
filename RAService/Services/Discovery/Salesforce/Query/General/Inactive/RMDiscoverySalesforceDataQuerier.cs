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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Inactive;

public abstract class RMDiscoverySalesforceDataQuerier<T>
{
    protected readonly RMDiscoverySalesforceQueryParameter SalesforceQueryParameter;

    protected readonly IRMDiscoverySalesforceDataQueryDao _dataQueryDao = new RMDiscoverySalesforceDataQueryDao();

    protected readonly RALogger _logger;

    protected readonly string _schemaName;

    protected readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();

    protected string OrganizationId;

    protected virtual string DataTableAlias => "data";

    protected RMDiscoverySalesforceDataQuerier(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
    {
        _logger = RALogger.GetInstance(GetType());
        SalesforceQueryParameter = salesforceQueryParameter;
        var organizationId = salesforceQueryParameter.OrganizationId;
        if (string.IsNullOrEmpty(salesforceQueryParameter.OrganizationId))
        {
            organizationId = _dataQueryDao.GetOrginazationId().GetAwaiter().GetResult();
        }
        _schemaName = RMDiscoveryDBManager.GetSalesforceSchemaName(organizationId);
        OrganizationId = organizationId;
    }

    public abstract Task<T> QueryAsync();

    protected abstract string GetDataTable(RMSFDiscoveryNodeViewMode dataType);

    protected virtual List<RMDiscoverySqlDefinition> GetAllConditionSqlDefinitions(RMSFDiscoveryNodeViewMode dataType)
    {
        var res = new List<RMDiscoverySqlDefinition>();

        if (SalesforceQueryParameter.WithoutDateQueryParameter != null &&
            SalesforceQueryParameter.WithoutDateQueryParameter.TryGetSFSqlDefinition(DataTableAlias, out var sqlDefinition))
        {
            res.Add(sqlDefinition);
        }

        if (SalesforceQueryParameter.SelectedObjectIds.IsNotNullOrEmpty() &&
            SalesforceQueryParameter.SelectedObjectIds.TryGetSeletedObjectSqlDefinition(DataTableAlias, out sqlDefinition))
        {
            res.Add(sqlDefinition);
        }

        if (SalesforceQueryParameter.NodeQueryParameter != null &&
            SalesforceQueryParameter.NodeQueryParameter.TryGetSqlDefinition(_schemaName, DataTableAlias, out sqlDefinition))
        {
            res.Add(sqlDefinition);
        }

        if (dataType == RMSFDiscoveryNodeViewMode.File) ConditionRMSFBasicInactiveDataTable(res);

        return res;
    }
    protected virtual List<RMDiscoverySqlDefinition> GetSelectedObjectConditionSqlDefinitions()
    {
        var res = new List<RMDiscoverySqlDefinition>();

        if (SalesforceQueryParameter.SelectedObjectIds.IsNotNullOrEmpty() &&
            SalesforceQueryParameter.SelectedObjectIds.TryGetObjectIdSqlDefinition(DataTableAlias, out var sqlDefinition))
        {
            res.Add(sqlDefinition);
        }

        return res;
    }
    public void ConditionRMSFBasicInactiveDataTable(List<RMDiscoverySqlDefinition> condition)
    {
        if (SalesforceQueryParameter.FileExtensionQueryParameter != null &&
            SalesforceQueryParameter.FileExtensionQueryParameter.TryGetSqlDefinition(DataTableAlias, out var sqlDefinition))
        {
            condition.Add(sqlDefinition);
        }

        if (SalesforceQueryParameter.SizeRangeQueryParameter != null &&
            SalesforceQueryParameter.SizeRangeQueryParameter.TryGetSqlDefinition(DataTableAlias, out sqlDefinition))
        {
            condition.Add(sqlDefinition);
        }
    }

    protected string AppendAllSqlConditions(string sql, out SqlParameter[] sqlParams, out List<RMDiscoverySqlDefinition> conditionSqlDefinitions, RMSFDiscoveryNodeViewMode dataType = RMSFDiscoveryNodeViewMode.Data)
    {
        if (dataType == RMSFDiscoveryNodeViewMode.None)
        {
            conditionSqlDefinitions = GetSelectedObjectConditionSqlDefinitions();
        }
        else
        {
            conditionSqlDefinitions = GetAllConditionSqlDefinitions(dataType);
        }
        var noRepeatJoinSqls = new SortedSet<string>();
        var conditionSqls = new List<string>();
        foreach (var condition in conditionSqlDefinitions)
        {
            if (condition.JoinOnSqls?.Any() == true)
            {
                condition.JoinOnSqls.ForEach(i => noRepeatJoinSqls.Add(i.FullSql));
            }

            conditionSqls.Add(condition.ConditionSql);
        }

        if (noRepeatJoinSqls.Any())
        {
            sql += $" {string.Join(" ", noRepeatJoinSqls)} ";
        }

        if (conditionSqls.Any())
        {
            sql = sql.Contains("Where", StringComparison.InvariantCultureIgnoreCase) ? sql + " And " : sql + " WHERE ";
            sql += $"{string.Join(" AND ", conditionSqls)} ";
        }

        sqlParams = conditionSqlDefinitions.SelectMany(item => item.Parameters).ToArray();

        return sql;
    }

    protected bool TryGetJoinTableAlias(List<RMDiscoverySqlDefinition> conditionSqlDefinitions, string tableName, out string tableAlias)
    {
        tableAlias = null;
        foreach (var condition in conditionSqlDefinitions)
        {
            if (condition.JoinOnSqls?.Any() == true)
            {
                foreach (var joinOnSqlInfo in condition.JoinOnSqls)
                {
                    if (joinOnSqlInfo.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                    {
                        tableAlias = joinOnSqlInfo.TableAlias;
                        return true;
                    }
                }
            }

        }

        return false;
    }
}