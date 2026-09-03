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
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General
{
    public abstract class RMDiscoveryGoogleDataQuerier<T>
    {
        protected readonly IRMDiscoveryGoogleDataQueryDao _queryDao = new RMDiscoveryGoogleDataQueryDao();

        protected readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();

        protected readonly RALogger _logger;

        protected readonly RMDiscoveryGoogleQueryParameter _queryParameter;

        protected readonly string _schemaName;

        protected virtual string DataTableAlias => "data";

        public RMDiscoveryGoogleDataQuerier(RMDiscoveryGoogleQueryParameter queryParameter)
        {
            if (string.IsNullOrWhiteSpace(queryParameter.OrganizationId))
            {
                throw new ArgumentException("queryParameter.GoogleOrganiztionId");
            }

            _logger = RALogger.GetInstance(GetType());
            _queryParameter = queryParameter;
            _schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(_queryParameter.OrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
        }

        public abstract Task<T> QueryAsync();

        protected abstract string GetDataTable(bool queryNodeInfo = false);

        protected virtual List<RMDiscoverySqlDefinition> GetAllConditionSqlDefinitions()
        {
            var res = new List<RMDiscoverySqlDefinition>();

            if (_queryParameter.FileExtensionQueryParameter != null &&
                _queryParameter.FileExtensionQueryParameter.TryGetSqlDefinition(DataTableAlias, out var sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.SizeRangeQueryParameter != null &&
                _queryParameter.SizeRangeQueryParameter.TryGetSqlDefinition(DataTableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.WithoutDateQueryParameter != null &&
                _queryParameter.WithoutDateQueryParameter.TryGetSqlDefinition(DataTableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.NodeQueryParameter != null &&
                _queryParameter.NodeQueryParameter.TryGetSqlDefinition(_schemaName, DataTableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            return res;
        }

        protected string AppendAllSqlConditions(string sql, out SqlParameter[] sqlParams, out List<RMDiscoverySqlDefinition> conditionSqlDefinitions)
        {
            conditionSqlDefinitions = GetAllConditionSqlDefinitions();

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
                sql += $" WHERE {string.Join(" AND ", conditionSqls)} ";
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

}
