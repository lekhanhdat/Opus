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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Contract.RMWeb.Explorer;
using SqlKata;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class CustomColumnQueryBuilder : ISearchBuilder, IFilterBuilder
    {
        #region Filter
        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (CanFilter(filterOption))
            {
                foreach (var customColumn in filterOption.CustomColumns.Where(o => !string.IsNullOrEmpty(o.Column.Id)))
                {
                    if (customColumn.Column.Type.HasValue)
                    {
                        CustomColumnFilterBuilderFactory.Create(customColumn.Column.Type.Value).Filter(query, customColumn.Column, customColumn.Value);
                    }
                }
            }
            return query;
        }

        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.CustomColumns != null && filterOption.CustomColumns.Count > 0;
        }
        #endregion

        #region search
        public Query Search(Query query, ExplorerSearchOptionV2 searchOption)
        {
            if (CanSearch(searchOption))
            {
                query.Where(q =>
                {
                    foreach (var column in searchOption.Columns.Where(o => !string.IsNullOrEmpty(o.Id)))
                    {
                        if (column.Type.HasValue)
                        {
                            q.OrWhere(q1 => CustomColumnSearchBuilderFactory.Create(column.Type.Value).Search(q1, column, searchOption.Key));
                        }
                    }

                    return q;
                });

                //foreach (var column in GetCustomSearchColumns(searchOption))
                //{
                //    //query.OrWhere(q1 => CustomColumnSearchBuilderFactory.Create(column.Type.Value).Search(q1, column, searchOption.Key));
                //    CustomColumnSearchBuilderFactory.Create(column.Type.Value).Search(query, column, searchOption.Key, searchOption.OperationLogic);
                //}
            }

            return query;
        }

        private bool CanSearch(ExplorerSearchOptionV2 searchOption)
        {
            return searchOption != null && searchOption.Columns != null && searchOption.Columns.Count > 0 && GetCustomSearchColumns(searchOption).Count > 0;
        }

        private List<ExplorerQueryColumn> GetCustomSearchColumns(ExplorerSearchOptionV2 searchOption)
        {
            return searchOption.Columns.Where(c => !string.IsNullOrEmpty(c.Id) && c.Type.HasValue).ToList();
        }


        #endregion
    }
}
