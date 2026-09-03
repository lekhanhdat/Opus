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
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions
{
    public static class RMDiscoveryGoogleNodeQueryParameterExtension
    {
        public static bool TryGetSearchKeySqlDefinition(this RMDiscoveryGoogleNodeQueryParameter parameter, string tableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = new();
            if (string.IsNullOrWhiteSpace(parameter.SearchKey))
            {
                return false;
            }

            var sql = parameter.ViewMode switch
            {
                RMDiscoveryGoogleNodeViewMode.Container => $"{tableAlias}.Name LIKE '%'+@SearchKey+'%'",
                RMDiscoveryGoogleNodeViewMode.Drive => $"{tableAlias}.Url LIKE '%'+@SearchKey+'%'",
                RMDiscoveryGoogleNodeViewMode.DriveInContainer => $"{tableAlias}.Url LIKE '%'+@SearchKey+'%'",
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

        public static bool TryGetSqlDefinition(this RMDiscoveryGoogleNodeQueryParameter parameter, string dbSchemaName, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;

            if (parameter.TryGetDiscoverySqlDefinitionBySelectedNodeIDs(dataTableAlias, out var selectedDefinition))
            {
                if (parameter.ViewMode == RMDiscoveryGoogleNodeViewMode.DriveInContainer)
                {
                    selectedDefinition.ConditionSql = $"{dataTableAlias}.ContainerId = @ContainerId AND {selectedDefinition.ConditionSql}";
                    selectedDefinition.Parameters.Add(new SqlParameter("@ContainerId", parameter.JoinedContainerId));
                }
                sqlDefinition = selectedDefinition;
                return true;
            }

            if (parameter.ViewMode == RMDiscoveryGoogleNodeViewMode.DriveInContainer)
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

        public static (bool need, string tableName, string joinColumn, string alias) NeedJoin(this RMDiscoveryGoogleNodeQueryParameter parameter)
        {
            var needJoin = !string.IsNullOrWhiteSpace(parameter.SearchKey);

            needJoin &= parameter.ViewMode switch
            {
                RMDiscoveryGoogleNodeViewMode.Container => !parameter.ContainerIds.Any(),
                RMDiscoveryGoogleNodeViewMode.Drive => !parameter.DriveIds.Any(),
                RMDiscoveryGoogleNodeViewMode.DriveInContainer => !parameter.DriveIds.Any(),
                _ => true,
            };

            var (tableName, joinColumn, alias) = parameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Container ? ("RMGoogleContainerInfoes", "ContainerId", "container") : ("RMGoogleDriveInfoes", "DriveId", "drive");
            return (needJoin, tableName, joinColumn, alias);
        }

        public static string GetNodeTableName(this RMDiscoveryGoogleNodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().nodeTableName;
        }

        public static string GetNodeIdColumnInDataTable(this RMDiscoveryGoogleNodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().nodeIdColumnInDataTable;
        }

        public static string GetNodeUrlOrNameColumn(this RMDiscoveryGoogleNodeQueryParameter parameter)
        {
            return parameter.GetNodeTableRelatedInfo().searchColumnInNodeTable;
        }


        private static (string nodeTableName, string nodeTableAlias, string searchColumnInNodeTable, string nodeIdColumnInDataTable, List<string> nodeIDs) GetNodeTableRelatedInfo(this RMDiscoveryGoogleNodeQueryParameter parameter)
        {
            return parameter.ViewMode switch
            {
                RMDiscoveryGoogleNodeViewMode.Container => ("RMGoogleContainerInfoes", "container", "Name", "ContainerId", parameter.ContainerIds.Select(i => i.ToString()).ToList()),
                RMDiscoveryGoogleNodeViewMode.Drive => ("RMGoogleDriveInfoes", "drive", "Url", "DriveId", parameter.DriveIds),
                _ => ("", "", "", "", new()),
            };
        }

        private static bool TryGetDiscoverySqlDefinitionBySelectedNodeIDs(this RMDiscoveryGoogleNodeQueryParameter parameter, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;
            var (_, _, _, nodeIdColumnInDataTable, nodeIDs) = parameter.GetNodeTableRelatedInfo();
            if (nodeIDs == null || nodeIDs.Count == 0)
            {
                return false;
            }

            var sql = $"{dataTableAlias}.{nodeIdColumnInDataTable} IN {DatabaseUtility.BuildInClause(nodeIDs)}";
            var parameters = new List<SqlParameter>();

            if (parameter.ViewMode == RMDiscoveryGoogleNodeViewMode.Container && parameter.ContainerIds.Any())
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
                if (parameter.DriveIds.Any())
                {
                    var sqls = new List<string>();
                    for (var i = 0; i < parameter.DriveIds.Count; i++)
                    {
                        var placeholder = $"@{nodeIdColumnInDataTable}{i}";
                        sqls.Add($"{dataTableAlias}.{nodeIdColumnInDataTable} = {placeholder}");
                        parameters.Add(new SqlParameter(placeholder, parameter.DriveIds[i]));
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

        private static string GetSearchKeySqlValue(this RMDiscoveryGoogleNodeQueryParameter parameter)
        {
            var searchKey = parameter.SearchKey;
            searchKey = searchKey.Replace("[", "[[]");
            searchKey = searchKey.Replace("%", "[%]");
            searchKey = searchKey.Replace("_", "[_]");
            return searchKey;
        }
    }
}
