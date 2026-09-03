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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using SqlKata;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension
{
    public static class QueryExtension
    {
        private static string GreaterThanOP = ">";
        private static string GreaterOrEqualsOP = ">=";
        private static string LessOrEqualsOP = "<=";
        private static string NotEquals = "<>";
        /// <summary>
        /// search columns
        /// </summary>
        /// <param name="query"></param>
        /// <param name="builderList"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static Query BuildSearch(this Query query, List<ISearchBuilder> builderList, ExplorerSearchOptionV2 searchOption)
        {
            if (builderList != null && builderList.Count > 0)
            {
                query.Where(q =>
                {
                    foreach (var builder in builderList)
                    {
                        //builder.Search(q, searchOption);
                        q.OrWhere(q1 => builder.Search(q1, searchOption));
                    }
                    return q;
                });
            }

            return query;
        }

        /// <summary>
        /// filter columns
        /// </summary>
        /// <param name="query"></param>
        /// <param name="builderList"></param>
        /// <param name="filterOption"></param>
        /// <returns></returns>
        public static Query BuildFilter(this Query query, List<IFilterBuilder> builderList, ExplorerFilterOptionV2 filterOption)
        {
            if (filterOption == null) return query;
            if (builderList != null && builderList.Count > 0)
            {
                foreach (var builder in builderList)
                {
                    builder.Filter(query, filterOption);
                };
            }

            return query;
        }

        public static Query BuildAdvancedSearch(this Query query, List<IAdvancedQueryBuilder> builderList, ExplorerQueryOptionV3 queryOption)
        {
            var groups = queryOption.Split();
            query.Where(q =>
            {
                foreach (var g in groups)
                {
                    q.OrWhere(q1 => BuildAdvancedSearch(q1, builderList, g.Values));  //不同group之间做or运算
                }
                return q;
            });

            return query;
        }

        private static Query BuildAdvancedSearch(this Query query, List<IAdvancedQueryBuilder> builderList, List<ExplorerSearchOptionV3> values)
        {
            foreach(var v in values)
            {
                builderList.ForEach(builder => builder.Build(query, v.Column, v.Value, v.ColumnOperationLogic, ExplorerSearchKeyOperationLogic.AND)); //同一个group内，各个成员做and运算
            }
            return query;
        }


        /// <summary>
        /// build order by clause
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderColumn"></param>
        /// <returns></returns>
        public static Query BuildOrder(this Query query, ExplorerQueryOrderColumn orderColumn = null)
        {
            if (orderColumn == null)
            {
                orderColumn = GetDefaultOrderColumn();
            }
            var columnName = orderColumn.Column.GetColumnName();
            if (orderColumn.OrderAsc)
            {
                query.OrderBy(columnName);
            }
            else
            {
                query.OrderByDesc(columnName);
            }

            return query;
        }

        /// <summary>
        /// query by DateTime object
        /// </summary>
        /// <param name="query"></param>
        /// <param name="timeInfo"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Query BuildDateInfo(this Query query, DateInfo timeInfo, string columnName)
        {
            switch(timeInfo.Condition)
                {
                case DateCondition.Pending:
                    return query.Where(columnName, DueDateUtil.Pending);
                case DateCondition.NextJob:
                    return query.Where(columnName, DueDateUtil.NextJob);
                case DateCondition.None:
                        break;
                    case DateCondition.Before:
                        var dtBefore = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                return query.Where(columnName, GreaterThanOP, DateTime.MinValue)
                    .Where(columnName, LessOrEqualsOP, dtBefore);
                    case DateCondition.After:
                        var dtAfter = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                return query.Where(columnName, GreaterThanOP, dtAfter);
                    case DateCondition.FromTo:
                        var startDt = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                var endDt = DateTimeUtil.ConvertTimeToUtcDate(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                return query.Where(columnName, GreaterOrEqualsOP, startDt)
                    .Where(columnName, LessOrEqualsOP, endDt);
                default:
                        break;
            }

            return query;
        }

        /// <summary>
        /// Query by Date time ticks
        /// </summary>
        /// <param name="query"></param>
        /// <param name="timeInfo"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Query BuildDateInfoTicks(this Query query, DateInfo timeInfo, string columnName)
        {
            switch (timeInfo.Condition)
            {
                case DateCondition.Pending:
                    return query.Where(columnName, DueDateUtil.Pending);
                case DateCondition.NextJob:
                    return query.Where(columnName, DueDateUtil.NextJob);
                case DateCondition.None:
                    break;
                case DateCondition.Before:
                    var dtBefore = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                    return query.Where(columnName, GreaterThanOP, DateTime.MinValue.Ticks)
                        .Where(columnName, LessOrEqualsOP, dtBefore);
                case DateCondition.After:
                    var dtAfter = DateTimeUtil.GetTicks(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                    return query.Where(columnName, GreaterThanOP, dtAfter);
                case DateCondition.FromTo:
                    var startDt = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                    var endDt = DateTimeUtil.GetTicks(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                    return query.Where(columnName, GreaterOrEqualsOP, startDt)
                        .Where(columnName, LessOrEqualsOP, endDt);
                case DateCondition.NextJobOrOverDue:  //用于查询DisposalDueDate到期数据1.DueDate为NextJob 2.DueDate早于当前时间
                    var dueDate = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                    return query.Where(columnName, NotEquals, DueDateUtil.Pending).Where(columnName, NotEquals, DueDateUtil.None).Where(columnName, LessOrEqualsOP, dueDate); ;
                default:
                    break;
            }

            return query;
        }

        public static Query BuildExists(this Query query, string formattedColumnName, string subColumnName, string key, Dictionary<string, List<string>> stringTermsDic)
        {
            //foreach (string subKey in splitedKeys)
            //{
            var terms = stringTermsDic.ContainsKey(key) ? stringTermsDic[key] : new List<string> { key };
            query.Where(subqu =>
            {
                return subqu.WhereExists(q =>
                {
                    return q.FromParent(subColumnName, formattedColumnName)
                    .WhereArrayContainV2(terms, string.Empty);
                });
            }
            );
            //}
            return query;
        }

        private static ExplorerQueryOrderColumn GetDefaultOrderColumn()
        {
            return new ExplorerQueryOrderColumn {
                Column = new ExplorerQueryColumn { Name = CosmosConst.C_LeafName},
                OrderAsc = true
            };
        }

    }
}
