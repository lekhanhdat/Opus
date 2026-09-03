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
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.MockV2;
using Microsoft.Azure.Documents;
using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    public class SqlQuerySpecBuilder
    {
        public List<ISearchBuilder> SearchBuilder { get; set; }
        public List<IFilterBuilder> FilterBuilder { get; set; }

        public List<IAdvancedQueryBuilder> AdvancedQueryBuilders { get; set; }
        public bool EnableOrderBy { get; set; } = true;

        public SqlQuerySpec Example()
        {
            var nameOrId = "leafname";
            int[] nodeTypes = new int[] { 1, 2, 3 };
            Guid[] exceptIds = new Guid[] { Guid.NewGuid() };
            int[] recordStatus = new int[] { 0, 1 };

            var query = new Query()
           .From(CosmosConst.T_Records)
           .As(CosmosConst.A_Records)
           .Where(q => q.WhereContains(CosmosConst.C_LeafName, nameOrId, true)
           .OrWhereContains(CosmosConst.C_RecordId, nameOrId, true))
           .WhereArrayContain(CosmosConst.C_NodeType, nodeTypes, false)
           .WhereNotArrayContain(CosmosConst.C_RecordId, exceptIds)
           .WhereFalse(CosmosConst.C_DeclareAsRecord)
           .WhereArrayContain(CosmosConst.C_RecordStatus, recordStatus)
           .Select(CosmosConst.C_LeafName, CosmosConst.C_RecordId);
            //.SecurityTirming();

            SqlQuerySpec SqlQueryResult = ComplileQuery(query);

            return SqlQueryResult;
        }

        public SqlQuerySpec Build(ExplorerQueryOptionV2 queryOption, bool queryTotalCount = false)
        {
            var query = new Query()
           .From(CosmosConst.T_Records)
           .As(CosmosConst.A_Records)
           .BuildSearch(SearchBuilder, queryOption.SearchOption)
           .BuildFilter(FilterBuilder, queryOption.FilterOption);

            if (EnableOrderBy)
            {
                query.BuildOrder(queryOption.OrderColumn);
            }

            if (queryTotalCount) query.AsCount();
            //.FilterSourceFlags(filterOption)
            //.Where(q => q.SearchUniqueId(filterOption)
            //     .OrWhere(q1 => q1.QueryName(filterOption)))
            //.SearchCustomColumn(filterOption);

            SqlQuerySpec SqlQueryResult = ComplileQuery(query);

            return SqlQueryResult;
        }

        public SqlQuerySpec BuildAdvancedSearch(ExplorerQueryOptionV3 queryOption, ExplorerFilterOptionV2 builtinFilterOption, bool queryTotalCount = false)
        {
            var query = new Query()
           .From(CosmosConst.T_Records)
           .As(CosmosConst.A_Records)
           .BuildFilter(FilterBuilder, builtinFilterOption)
           .BuildAdvancedSearch(AdvancedQueryBuilders, queryOption);

            if (EnableOrderBy)
            {
                query.BuildOrder(queryOption.OrderColumn);
            }

            if (queryTotalCount) query.AsCount();

            SqlQuerySpec SqlQueryResult = ComplileQuery(query);

            return SqlQueryResult;
        }

        #region private

        private static SqlQuerySpec ComplileQuery(Query query)
        {
            CosmosDBCompiler compiler = new CosmosDBCompiler();
            var result = compiler.Compile(query);

            var sqlParaCollection = new SqlParameterCollection();

            foreach (var binding in result.NamedBindings)
            {
                sqlParaCollection.Add(new SqlParameter(binding.Key, binding.Value));
            }

            var SqlQueryResult = new SqlQuerySpec()
            {
                QueryText = result.Sql,
                Parameters = sqlParaCollection
            };
            return SqlQueryResult;
        }

        #endregion
    }
}
