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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions
{
    public static class RMDiscoveryNodeQueryParameterExtension
    {
        public static bool TryGetSearchKeySqlDefinition(this RMDiscoveryOffice365NodeQueryParameter parameter, string tableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = new();
            if (string.IsNullOrWhiteSpace(parameter.SearchKey))
            {
                return false;
            }

            var sql = parameter.ViewMode switch
            {
                RMDiscoveryNodeViewMode.Container => $"{tableAlias}.Name LIKE '%'+@SearchKey+'%'",
                RMDiscoveryNodeViewMode.Site => $"{tableAlias}.Url LIKE '%'+@SearchKey+'%'",
                RMDiscoveryNodeViewMode.SiteInContainer => $"{tableAlias}.Url LIKE '%'+@SearchKey+'%'",
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

        public static bool TryGetSqlDefinition(this RMDiscoveryOffice365NodeQueryParameter parameter, string dbSchemaName, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;

            if (parameter.TryGetDiscoverySqlDefinitionBySelectedNodeIDs(dataTableAlias, out var selectedDefinition))
            {
                if (parameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer)
                {
                    selectedDefinition.ConditionSql = $"{dataTableAlias}.ContainerId = @ContainerId AND {selectedDefinition.ConditionSql}";
                    selectedDefinition.Parameters.Add(new SqlParameter("@ContainerId", parameter.JoinedContainerId));
                }
                sqlDefinition = selectedDefinition;
                return true;
            }
            //else if (!string.IsNullOrWhiteSpace(parameter.SearchKey))
            //{
            //    var searchKeyDefinition = parameter.GetDiscoverySqlDefinitionBySearchKey(dbSchemaName, dataTableAlias);
            //    if (parameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer)
            //    {
            //        searchKeyDefinition.ConditionSql = $"{dataTableAlias}.ContainerId = @ContainerId AND {searchKeyDefinition.ConditionSql}";
            //        searchKeyDefinition.Parameters.Add(new SqlParameter("@ContainerId", parameter.JoinedContainerId));
            //    }
            //    sqlDefinition = searchKeyDefinition;
            //    return true;
            //}

            if (parameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer)
            {
                sqlDefinition = new()
                {
                    ConditionSql = $"{dataTableAlias}.ContainerId = @ContainerId",
                    Parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@ContainerId", parameter.JoinedContainerId)
                    }
                };

                return true;
            }

            return false;
        }

        public static (bool need, string tableName, string joinColumn, string alias) NeedJoin(this RMDiscoveryOffice365NodeQueryParameter parameter)
        {
            var needJoin = !string.IsNullOrWhiteSpace(parameter.SearchKey);

            needJoin &= parameter.ViewMode switch
            {
                RMDiscoveryNodeViewMode.Container => !parameter.ContainerIds.Any(),
                RMDiscoveryNodeViewMode.Site => !parameter.SiteIds.Any(),
                RMDiscoveryNodeViewMode.SiteInContainer => !parameter.SiteIds.Any(),
                _ => true,
            };

            var (tableName, joinColumn, alias) = parameter.ViewMode == RMDiscoveryNodeViewMode.Container ? ("RMContainerInfoes", "ContainerId", "container") : ("RMSiteInfoes", "SiteId", "site");
            return (needJoin, tableName, joinColumn, alias);
        }

        public static string GetNodeTableName(this RMDiscoveryOffice365NodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().nodeTableName;
        }

        public static string GetNodeIdColumnInDataTable(this RMDiscoveryOffice365NodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().nodeIdColumnInDataTable;
        }

        public static string GetNodeUrlOrNameColumn(this RMDiscoveryOffice365NodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().searchColumnInNodeTable;
        }

        private static (string nodeTableName, string nodeTableAlias, string searchColumnInNodeTable, string nodeIdColumnInDataTable, List<int> nodeIDs) GetNodeTableRelatedInfo(this RMDiscoveryOffice365NodeQueryParameter parameter)
        {
            return parameter.ViewMode == RMDiscoveryNodeViewMode.Container
                ? ("RMContainerInfoes", "container", "Name", "ContainerId", parameter.ContainerIds)
                : ("RMSiteInfoes", "site", "Url", "SiteId", parameter.SiteIds);
        }

        private static bool TryGetDiscoverySqlDefinitionBySelectedNodeIDs(this RMDiscoveryOffice365NodeQueryParameter parameter, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;
            var (_, _, _, nodeIdColumnInDataTable, nodeIDs) = parameter.GetNodeTableRelatedInfo();
            if (nodeIDs == null || nodeIDs.Count == 0)
            {
                return false;
            }

            var sql = $"{dataTableAlias}.{nodeIdColumnInDataTable} IN {DatabaseUtility.BuildInClause(nodeIDs)}";
            var parameters = new List<SqlParameter>();

            if (parameter.ViewMode == RMDiscoveryNodeViewMode.Container && parameter.ContainerIds.Any())
            {
                var sqls = new List<string>();
                for (var i = 0; i < parameter.ContainerIds.Count; i++)
                {
                    var placeholder = $"@{nodeIdColumnInDataTable}{i}";
                    sqls.Add($"{dataTableAlias}.{nodeIdColumnInDataTable} = {placeholder}");
                    parameters.Add(new SqlParameter(placeholder, parameter.ContainerIds[i]));
                }
                sql = $"({string.Join(" OR ", sqls)})";
            }
            else
            {
                if (parameter.SiteIds.Any())
                {
                    var sqls = new List<string>();
                    for (var i = 0; i < parameter.SiteIds.Count; i++)
                    {
                        var placeholder = $"@{nodeIdColumnInDataTable}{i}";
                        sqls.Add($"{dataTableAlias}.{nodeIdColumnInDataTable} = {placeholder}");
                        parameters.Add(new SqlParameter(placeholder, parameter.SiteIds[i]));
                    }
                    sql = $"({string.Join(" OR ", sqls)})";
                }
            }

            sqlDefinition = new()
            {
                ConditionSql = sql,
                Parameters = parameters
            };

            return true;
        }

        private static string GetSearchKeySqlValue(this RMDiscoveryOffice365NodeQueryParameter parameter)
        {
            var searchKey = parameter.SearchKey;
            searchKey = searchKey.Replace("[", "[[]");
            searchKey = searchKey.Replace("%", "[%]");
            searchKey = searchKey.Replace("_", "[_]");
            return searchKey;
        }
    }
}
