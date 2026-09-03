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
using System;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class RecordStatusQueryBuilder : BaseArrayFilterBuilder, IObjectArrayFilterBuilder<RMRecordStatus>
    {

        #region BaseArrayFilterBuilder
        protected override bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.Status != null && filterOption.Status.Count > 0;
        }

        protected override string GetFilterColumnName()
        {
            return CosmosConst.C_RecordStatus;
        }

        protected override object GetFilterValue(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption.Status;
        }
        #endregion

        #region IObjectArrayFilterBuilder

        public Query Filter(Query query, RMRecordStatus[] values)
        {
            return query.WhereArrayContainV2(values, GetFilterColumnName().FormatColumnName());

        }

        #endregion

        #region Advanced search
        protected override string GetColumnId()
        {
            throw new NotImplementedException();
        }

        protected override ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
