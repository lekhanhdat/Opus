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
using Newtonsoft.Json;
using SqlKata;
using System;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class TermQueryBuilder : IFilterBuilder, IAdvancedQueryBuilder
    {
        private IFilterBuilder termIdQueryBuilder = new TermIdQueryBuilder();
        private IFilterBuilder withoutTermsQueryBuilder = new WithoutTermsQueryBuilder();

        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            var canFilterTermId = CanFilterTermId(filterOption);
            var canFilterWithoutTerms = CanFilterWithoutTerms(filterOption);

            if (!canFilterTermId && !canFilterWithoutTerms) return query;

            if (canFilterTermId && canFilterWithoutTerms)
            {
                query.Where(q1 =>
                {
                    q1.Where(q2 =>
                    {
                        FilterTermId(q2, filterOption);
                        return q2;
                    }).OrWhere(q3 =>
                    {
                        FilterWithoutTerms(q3, filterOption);
                        return q3;
                    });

                    return q1;
                });
            }
            else if (canFilterTermId)
            {
                FilterTermId(query, filterOption);
            }
            else if (canFilterWithoutTerms)
            {
                FilterWithoutTerms(query, filterOption);
            }

            return query;
        }


        private void FilterTermId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            termIdQueryBuilder.Filter(query, filterOption);
        }

        private void FilterWithoutTerms(Query query, ExplorerFilterOptionV2 filterOption)
        {
            withoutTermsQueryBuilder.Filter(query, filterOption);
        }

        private bool CanFilterTermId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.TermIds != null && filterOption.TermIds.Count > 0;
        }

        private bool CanFilterWithoutTerms(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.WithOutTerms.HasValue && filterOption.WithOutTerms.Value;
        }

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
        protected string GetColumnId()
        {
            return Contract.TemplateManagement.QueryCloumnIds.Term;
        }
        protected ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            //此builder用到了多个属性，所以需要将objJson解析为ExplorerFilterOptionV2对象
            return JsonConvert.DeserializeObject<ExplorerFilterOptionV2>(objJson);
        }

        #endregion

    }
}
