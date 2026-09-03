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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public enum SortDirectionEnum
    {
        /// <summary>
        /// Do not Sort By this Property
        /// </summary>
        None = 0,
        Ascending = 1,
        Descending = 2,
    }

    public static class QueryExtensions
    {
        /// <summary>
        /// Linq Sort Extensions Method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sortPropertyName"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection)
        {
            string OrderBy = "OrderBy";
            string OrderByDescending = "OrderByDescending";
            return BaseSort(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending);
        }

        /// <summary>
        /// Linq Then Sort Extensions Method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sortPropertyName"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenSortBy<T>(this IOrderedQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection)
        {
            string OrderBy = "ThenBy";
            string OrderByDescending = "ThenByDescending";
            var iQuery = BaseSort(source, sortPropertyName, sortDirection, OrderBy, OrderByDescending);
            return iQuery;
        }
        public static IOrderedQueryable<T> BaseSort<T>(IQueryable<T> source, string sortPropertyName, SortDirectionEnum sortDirection, string OrderBy, string OrderByDescending)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (String.IsNullOrEmpty(sortPropertyName) || sortPropertyName.Trim().Length == 0)
            {
                return (IOrderedQueryable<T>)source;
            }

            ParameterExpression parameter = Expression.Parameter(source.ElementType, String.Empty);
            MemberExpression property = Expression.Property(parameter, sortPropertyName);
            LambdaExpression lambda = Expression.Lambda(property, parameter);

            string methodName = (sortDirection == SortDirectionEnum.Ascending) ? OrderBy : OrderByDescending;

            Expression methodCallExpression = Expression.Call(typeof(Queryable), methodName,
                                                new Type[] { source.ElementType, property.Type },
                                                source.Expression, Expression.Quote(lambda));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(methodCallExpression);
        }


    }
}
