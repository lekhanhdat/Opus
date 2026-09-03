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
using SqlKata;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public abstract class BaseArrayMultiValueFilterBuilder : IFilterBuilder, IAdvancedQueryBuilder
    {
        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (CanFilter(filterOption))
            {
                var columnName = GetFilterColumnName();
                query.WhereExists(q =>
                {
                    q.FromParent($"sub{columnName}", columnName.FormatColumnName())
                    .WhereArrayContainV2(GetFilterValue(filterOption), string.Empty);
                    return q;
                });
                //return query.WhereArrayContainV2(GetFilterValue(filterOption), GetFilterColumnName().FormatColumnName());
            }

            return query;
        }

        protected abstract string GetFilterColumnName();
        protected abstract bool CanFilter(ExplorerFilterOptionV2 filterOption);
        protected abstract object GetFilterValue(ExplorerFilterOptionV2 filterOption);

        #region Advanced search        
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!string.Equals(GetColumnId(), column.Id, System.StringComparison.OrdinalIgnoreCase)) return query;

            var filterOption = Convert2SearchOptionV2(column, objJson, columnOperationLogic, keyOperationLogic);
            return Filter(query, filterOption);
        }

        /// <summary>
        /// id represents this column
        /// </summary>
        /// <returns></returns>
        protected abstract string GetColumnId();
        protected abstract ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic);

        #endregion
    }
}
