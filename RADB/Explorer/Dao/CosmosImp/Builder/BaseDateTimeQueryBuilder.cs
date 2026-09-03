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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using SqlKata;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public abstract class BaseDateTimeQueryBuilder : IFilterBuilder, IAdvancedQueryBuilder
    {
        //private const string GreaterThanOP = ">";
        //private const string GreaterOrEqualsOP = ">=";
        //private const string LessOrEqualsOP = "<=";



        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (CanFilter(filterOption))
            {
                var timeInfo = GetDateInfo(filterOption);
                var columnName = GetColumnName().FormatColumnName();
                query.BuildDateInfoTicks(timeInfo, columnName);
                //switch (timeInfo.Condition)
                //{
                //    case DateCondition.Pending:
                //        return query.Where(columnName, DueDateUtil.Pending);
                //    case DateCondition.NextJob:
                //        return query.Where(columnName, DueDateUtil.NextJob);
                //    case DateCondition.None:
                //        break;
                //    case DateCondition.Before:
                //        long ticks = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                //        return query.Where(columnName, GreaterThanOP, DateTime.MinValue.Ticks)
                //            .Where(columnName, LessOrEqualsOP, ticks);
                //    case DateCondition.After:
                //        long ticksValue = DateTimeUtil.GetTicks(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                //        return query.Where(columnName, GreaterThanOP, ticksValue);
                //    case DateCondition.FromTo:
                //        long startTicks = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                //        long endTicks = DateTimeUtil.GetTicks(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                //        return query.Where(columnName, GreaterOrEqualsOP, startTicks)
                //            .Where(columnName, LessOrEqualsOP, endTicks);
                //    default:
                //        break;
                //}
            }

            return query;
        }

        abstract protected DateInfo GetDateInfo(ExplorerFilterOptionV2 filterOption);
        abstract protected bool CanFilter(ExplorerFilterOptionV2 filterOption);

        abstract protected string GetColumnName();


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
