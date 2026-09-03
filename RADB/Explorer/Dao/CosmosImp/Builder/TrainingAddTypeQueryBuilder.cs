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
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using Newtonsoft.Json;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class TrainingAddTypeQueryBuilder : IFilterBuilder
    {
        #region BaseArrayFilterBuilder
        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.TrainingAddTypes != null && filterOption.TrainingAddTypes.Count > 0;
        }

        private string GetFilterColumnName()
        {
            return CosmosConst.C_TrainingAddType;
        }

        private List<TrainingAddType> GetFilterValue(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption.TrainingAddTypes;
        }
        #endregion

        #region IObjectArrayFilterBuilder

        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (CanFilter(filterOption))
            {
                var values = GetFilterValue(filterOption);
                if (values?.Count == 0 || values.Contains(TrainingAddType.None))
                {
                    query.Where(q =>
                    {
                        q.WhereArrayContainV2(values, GetFilterColumnName().FormatColumnName())
                        .OrWhereNotDefined(GetFilterColumnName().FormatColumnName());
                        return q;
                    });
                }
                else
                {
                    query.WhereArrayContainV2(values, GetFilterColumnName().FormatColumnName());
                }
            }
            return query;
        }
        
        #endregion
    }
}
