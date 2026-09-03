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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using SqlKata;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    /// <summary>
    /// physical shallow query
    /// </summary>
    public class PhysicalShallowQueryBuilder : IFilterBuilder
    {
        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;
            var filterBuilderList = new List<IFilterBuilder> {new NodeTypeQueryBuilder(), new PhysicalLocationQueryBuilder(), new PhysicalBoxQueryBuilder(), new PhysicalFileQueryBuilder()};
            var ancestorBuilder = new ParentIdQueryBuilder();
            query.Where(q1 =>
            {
                q1.Where(q2 =>
                {
                    if (!filterOption.PhysicalSearchNodeLevel.HasValue || filterOption.PhysicalSearchNodeLevel.Value != Contract.RMWeb.Tree.Base.RMNodeLevel.PhysicalCustom)
                    {
                        filterBuilderList.ForEach(builder => builder.Filter(q2, filterOption));
                        q2.WhereNotDefined(CosmosConst.C_AncestorArray.FormatColumnName());
                    }

                    return q2;
                }).OrWhere(q3 =>
                {
                    ancestorBuilder.Filter(q3, filterOption);
                    return q3;
                });

                return q1;
            });
            return query;
        }

        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.PhycialModel.HasValue && filterOption.PhycialModel.Value == PhysicalSearchModel.Shallow;
        }

    }
}
