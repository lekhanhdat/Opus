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
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Query.General.Extensions
{
    public static class RMDiscoveryFSNodeQueryParameterExtension
    {
        public static bool TryGetSearchKeySqlDefinition(this RMDiscoveryFSNodeQueryParameter parameter, string tableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = new();
            if (string.IsNullOrWhiteSpace(parameter.SearchKey))
            {
                return false;
            }

            var sql = parameter.ViewMode switch
            {
                RMDiscoveryFSNodeViewMode.Container => $"{tableAlias}.Name LIKE '%'+@SearchKey+'%'",
                RMDiscoveryFSNodeViewMode.Connection => $"{tableAlias}.Name LIKE '%'+@SearchKey+'%'",
                RMDiscoveryFSNodeViewMode.ConnectionInContainer => $"{tableAlias}.Name LIKE '%'+@SearchKey+'%'",
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

        public static bool TryGetSqlDefinition(this RMDiscoveryFSNodeQueryParameter parameter, string dbSchemaName, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;

            if (parameter.TryGetDiscoverySqlDefinitionBySelectedNodeIDs(dataTableAlias, out var selectedDefinition))
            {
                if (parameter.ViewMode == RMDiscoveryFSNodeViewMode.ConnectionInContainer)
                {
                    selectedDefinition.ConditionSql = $"{dataTableAlias}.ContainerId = @ContainerId AND {selectedDefinition.ConditionSql}";
                    selectedDefinition.Parameters.Add(new SqlParameter("@ContainerId", parameter.JoinedContainerId));
                }
                sqlDefinition = selectedDefinition;
                return true;
            }

            if (parameter.ViewMode == RMDiscoveryFSNodeViewMode.ConnectionInContainer)
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

        public static (bool need, string tableName, string joinColumn, string alias) NeedJoin(this RMDiscoveryFSNodeQueryParameter parameter)
        {
            var needJoin = !string.IsNullOrWhiteSpace(parameter.SearchKey);

            needJoin &= parameter.ViewMode switch
            {
                RMDiscoveryFSNodeViewMode.Container => !parameter.ContainerIds.Any(),
                RMDiscoveryFSNodeViewMode.Connection => !parameter.ConnectionIds.Any(),
                RMDiscoveryFSNodeViewMode.ConnectionInContainer => !parameter.ConnectionIds.Any(),
                _ => true,
            };

            var (tableName, joinColumn, alias) = parameter.ViewMode == RMDiscoveryFSNodeViewMode.Container ? ("RMFSContainerInfoes", "ContainerId", "container") : ("RMFSConnectionInfoes", "ConnectionId", "connection");
            return (needJoin, tableName, joinColumn, alias);
        }

        public static string GetNodeTableName(this RMDiscoveryFSNodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().nodeTableName;
        }

        public static string GetNodeIdColumnInDataTable(this RMDiscoveryFSNodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().nodeIdColumnInDataTable;
        }

        public static string GetNodeUrlOrNameColumn(this RMDiscoveryFSNodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().searchColumnInNodeTable;
        }


        private static (string nodeTableName, string nodeTableAlias, string searchColumnInNodeTable, string nodeIdColumnInDataTable, List<string> nodeIDs) GetNodeTableRelatedInfo(this RMDiscoveryFSNodeQueryParameter parameter)
        {
            return parameter.ViewMode switch
            {
                RMDiscoveryFSNodeViewMode.Container => ("RMFSContainerInfoes", "container", "Name", "ContainerId", parameter.ContainerIds.Select(i => i.ToString()).ToList()),
                RMDiscoveryFSNodeViewMode.Connection => ("RMFSConnectionInfoes", "connection", "Name", "ConnectionId", parameter.ConnectionIds),
                _ => ("", "", "", "", new()),
            };
        }

        private static bool TryGetDiscoverySqlDefinitionBySelectedNodeIDs(this RMDiscoveryFSNodeQueryParameter parameter, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;
            var (_, _, _, nodeIdColumnInDataTable, nodeIDs) = parameter.GetNodeTableRelatedInfo();
            if (nodeIDs == null || nodeIDs.Count == 0)
            {
                return false;
            }

            var sql = $"{dataTableAlias}.{nodeIdColumnInDataTable} IN {DatabaseUtility.BuildInClause(nodeIDs)}";
            var parameters = new List<SqlParameter>();

            if (parameter.ViewMode == RMDiscoveryFSNodeViewMode.Container && parameter.ContainerIds.Any())
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
                if (parameter.ConnectionIds.Any())
                {
                    var sqls = new List<string>();
                    for (var i = 0; i < parameter.ConnectionIds.Count; i++)
                    {
                        var placeholder = $"@{nodeIdColumnInDataTable}{i}";
                        sqls.Add($"{dataTableAlias}.{nodeIdColumnInDataTable} = {placeholder}");
                        parameters.Add(new SqlParameter(placeholder, parameter.ConnectionIds[i]));
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

        private static string GetSearchKeySqlValue(this RMDiscoveryFSNodeQueryParameter parameter)
        {
            var searchKey = parameter.SearchKey;
            searchKey = searchKey.Replace("[", "[[]");
            searchKey = searchKey.Replace("%", "[%]");
            searchKey = searchKey.Replace("_", "[_]");
            return searchKey;
        }
    }
}
