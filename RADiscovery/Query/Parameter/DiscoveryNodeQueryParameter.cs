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
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.Parameter
{
    public class DiscoveryNodeQueryParameter
    {
        [JsonProperty("viewMode")]
        public DiscoveryNodeViewMode ViewMode { get; set; }

        [JsonProperty("searchKey")]
        public string SearchKey { get; set; }

        [JsonProperty("joinedContainerId")]
        public int JoinedContainerId { get; set; }

        [JsonProperty("containerIds")]
        public List<int> ContainerIds { get; set; } = new();

        [JsonProperty("siteIds")]
        public List<int> SiteIds { get; set; } = new();

        [JsonProperty("orderBy")]
        public string OrderBy { get; set; }

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("needCalculateInfo")]
        public DiscoveryNodeNeedCalculateInfo NeedCalculateInfo { get; set; }

        public (bool has, DiscoverySqlDefinition sqlDefinition) TryGetSearchKeySqlDefinition(string tableAlias)
        {
            if(string.IsNullOrWhiteSpace(SearchKey))
            {
                return (false, new());
            }

            var sql = ViewMode switch
            {
                DiscoveryNodeViewMode.Container => $"{tableAlias}.Name LIKE '%'+@SearchKey+'%'",
                DiscoveryNodeViewMode.Site => $"{tableAlias}.Url LIKE '%'+@SearchKey+'%'",
                DiscoveryNodeViewMode.SiteInContainer => $"{tableAlias}.Url LIKE '%'+@SearchKey+'%'",
                _ => string.Empty
            };

            if(string.IsNullOrWhiteSpace(sql))
            {
                return (false, new());
            }

            return (true, new()
            {
                Sql = sql,
                Parameters = new()
                    {
                        new SqlParameter("@SearchKey", GetSearchKeyValue())
                    }
            });
        }

        public (bool has, DiscoverySqlDefinition sqlDefinition) TryGetSqlDefinition(string tableAlias)
        {
            if (!HasCalculationConditions())
            {
                return (false, new());
            }

            return ViewMode switch
            {
                DiscoveryNodeViewMode.Container => TryGetContainerSqlDefinition(tableAlias),
                DiscoveryNodeViewMode.Site => TryGetSiteSqlDefinition(tableAlias),
                DiscoveryNodeViewMode.SiteInContainer => TryGetSiteInContainerSqlDefinition(tableAlias),
                _ => (false, new())
            };
        }

        private (bool has, DiscoverySqlDefinition sqlDefinition) TryGetContainerSqlDefinition(string tableAlias)
        {
            if (!ContainerIds.Any())
            {
                return (true, new()
                {
                    Sql = $"{tableAlias}.Name LIKE '%'+@SearchKey+'%'",
                    Parameters = new()
                    {
                        new SqlParameter("@SearchKey", GetSearchKeyValue())
                    }
                });
            }

            var sql = $"{tableAlias}.ContainerId IN {DatabaseUtility.BuildInClause(ContainerIds)}";
            var parameters = new List<SqlParameter>();

            if (ContainerIds.Count <= 3)
            {
                var sqls = new List<string>();
                for (var i = 0; i < ContainerIds.Count; i++)
                {
                    var placeholder = "@ContainerId" + i;
                    sqls.Add($"{tableAlias}.ContainerId = {placeholder}");
                    parameters.Add(new SqlParameter(placeholder, ContainerIds[i]));
                }
                sql = string.Join(" AND ", sqls);
            }

            return (true, new()
            {
                Sql = sql,
                Parameters = parameters
            });
        }

        private (bool has, DiscoverySqlDefinition sqlDefinition) TryGetSiteSqlDefinition(string tableAlias)
        {
            if (!ContainerIds.Any())
            {
                return (true, new()
                {
                    Sql = $"{tableAlias}.Url LIKE '%'+@SearchKey+'%'",
                    Parameters = new()
                    {
                        new SqlParameter("@SearchKey", GetSearchKeyValue())
                    }
                });
            }

            var sql = $"{tableAlias}.SiteId IN {DatabaseUtility.BuildInClause(ContainerIds)}";
            var parameters = new List<SqlParameter>();

            if (ContainerIds.Count <= 3)
            {
                var sqls = new List<string>();
                for (var i = 0; i < ContainerIds.Count; i++)
                {
                    var placeholder = "@SiteId" + i;
                    sqls.Add($"{tableAlias}.SiteId = {placeholder}");
                    parameters.Add(new SqlParameter(placeholder, ContainerIds[i]));
                }
                sql = string.Join(" AND ", sqls);
            }

            return (true, new()
            {
                Sql = sql,
                Parameters = parameters
            });
        }

        private (bool has, DiscoverySqlDefinition sqlDefinition) TryGetSiteInContainerSqlDefinition(string tableAlias)
        {
            if (!ContainerIds.Any())
            {
                return (true, new()
                {
                    Sql = $"{tableAlias}.ContainerId = @ContainerId AND {tableAlias}.Url LIKE '%'+@SearchKey+'%'",
                    Parameters = new()
                    {
                        new SqlParameter("@ContainerId", ContainerIds.First()),
                        new SqlParameter("@SearchKey", GetSearchKeyValue())
                    }
                });
            }

            var sql = $"{tableAlias}.SiteId IN {DatabaseUtility.BuildInClause(ContainerIds)}";
            var parameters = new List<SqlParameter>();

            if (ContainerIds.Count <= 3)
            {
                var sqls = new List<string>();
                for (var i = 0; i < ContainerIds.Count; i++)
                {
                    var placeholder = "@SiteId" + i;
                    sqls.Add($"{tableAlias}.SiteId = {placeholder}");
                    parameters.Add(new SqlParameter(placeholder, ContainerIds[i]));
                }
                sql = string.Join(" AND ", sqls);
            }

            return (true, new()
            {
                Sql = sql,
                Parameters = parameters
            });
        }

        public (bool need, string tableName, string joinColumn, string alias) NeedJoin()
        {
            var needJoin = !string.IsNullOrWhiteSpace(SearchKey);

            needJoin &= ViewMode switch
            {
                DiscoveryNodeViewMode.Container => !ContainerIds.Any(),
                DiscoveryNodeViewMode.Site => !SiteIds.Any(),
                DiscoveryNodeViewMode.SiteInContainer => !SiteIds.Any(),
                _ => true,
            };

            var (tableName, joinColumn, alias) = ViewMode == DiscoveryNodeViewMode.Container ? ("RMContainerInfoes", "ContainerId", "container") : ("RMSiteInfoes", "SiteId", "site");
            return (needJoin, tableName, joinColumn, alias);
        }

        public bool HasCalculationConditions()
        {
            if (!string.IsNullOrWhiteSpace(SearchKey))
            {
                return true;
            }

            return ViewMode switch
            {
                DiscoveryNodeViewMode.Container => ContainerIds.Any(),
                DiscoveryNodeViewMode.Site => SiteIds.Any(),
                DiscoveryNodeViewMode.SiteInContainer => SiteIds.Any(),
                _ => false,
            };
        }

        public string GetInactiveDataTableName()
        {
            if(ViewMode == DiscoveryNodeViewMode.Container &&  ContainerIds.Any())
            {
                return "RMContainerInactiveData";
            }

            if(ViewMode == DiscoveryNodeViewMode.SiteInContainer || ( ViewMode == DiscoveryNodeViewMode.Site && SiteIds.Any()))
            {
                return "RMSiteInactiveData";
            }

            return "RMBasicInactiveData";
        }

        private string GetSearchKeyValue()
        {
            var searchKey = SearchKey;
            searchKey = searchKey.Replace("[", "[[]");
            searchKey = searchKey.Replace("%", "[%]");
            searchKey = searchKey.Replace("_", "[_]");
            return searchKey;
        }
    }

    public class DiscoveryNodeNeedCalculateInfo
    {
        [JsonProperty("needCalculateCount")]
        public bool NeedCalculateCount { get; set; }
    }

    public enum DiscoveryNodeViewMode
    {
        None = 0,
        Container = 1,
        Site = 2,
        SiteInContainer = 3,
    }
}
