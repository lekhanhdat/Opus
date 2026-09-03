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
using Newtonsoft.Json;
using SqlKata;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class PhysicalPermissionQueryBuilder : IFilterBuilder
    {
        private IFilterBuilder includePermissionBuilder;
        private IFilterBuilder excludePermissionBuilder;
        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;
            includePermissionBuilder = new PermissionIdQueryBuilder();
            excludePermissionBuilder = new PermissionIdNotContainQueryBuilder();

            var hasInclude = filterOption.PersmissionScopes?.Count > 0;
            var hasExclude = filterOption.ExcludePersmissionScopes?.Count > 0;

            if (hasInclude && hasExclude)
            {
                query.Where(q1 =>
                {
                    q1.Where(q2 =>
                    {
                        includePermissionBuilder.Filter(q2, filterOption);
                        return q2;
                    }).OrWhere(q3 =>
                    {
                        excludePermissionBuilder.Filter(q3, filterOption);
                        return q3;
                    });

                    return q1;
                });
            }
            else if (hasInclude)
            {
                includePermissionBuilder.Filter(query, filterOption);
            }
            else
            {
                excludePermissionBuilder.Filter(query, filterOption);
            }


            return query;
        }
        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && (filterOption.PersmissionScopes != null || filterOption.ExcludePersmissionScopes != null);
        }
    }
}
