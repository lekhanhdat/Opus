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
namespace AvePoint.Wrapper.Common
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class AveQueryStringCommonUtility
    {
        /// <summary>
        /// 将集合拼接成string条件,example{a,b,c}  'a','b','c'
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string GetCondByCommaSeparatedList<T>(IEnumerable<T> collection)
        {
            var text = collection.Aggregate(string.Empty, (current, name) => string.Format("{0}'{1}',", current, name));
            return text.Trim(',');
        }

        /// <summary>
        /// 将集合拼接成string条件,example{a,b,c}  a,b,c
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string GetCondByCommaSeparatedWithoutQuoteList<T>(IEnumerable<T> collection)
        {
            var text = collection.Aggregate(string.Empty, (current, name) => string.Format("{0}{1},", current, name));
            return text.Trim(',');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="format"></param>
        /// <param name="conditionLength"></param>
        /// <returns></returns>
        public static string GetCondByCommaSeparatedWithoutQuoteList<T>(IEnumerable<T> collection, string format, int conditionLength)
        {
            var text = collection.Aggregate(string.Empty, (current, name) => current + string.Format(format, name));
            text = text.Substring(0, text.Length - conditionLength);
            return text;
        }
    }
}
