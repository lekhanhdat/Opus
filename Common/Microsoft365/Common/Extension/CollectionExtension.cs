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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft365.Common.Extension
{
    public static class CollectionExtension
    {
        /// <summary>
        /// get specific page items from a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="perPage"></param>
        /// <param name="page">index start with 1,</param>
        /// <returns></returns>
        public static IEnumerable<T> GetPagedItems<T>(this IEnumerable<T> items, int perPage, int page)
        {
            if (page <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }
            if (perPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(perPage));
            }
            return items.Skip(perPage * (page - 1)).Take(perPage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="startIndex">first item start at 0</param>
        /// <param name="range">items count that will be pick up in one time</param>
        /// <returns></returns>
        public static IEnumerable<T> GetItemRange<T>(this IEnumerable<T> items, int startIndex, int range)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (range < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }
            return items.Skip(startIndex).Take(range);
        }
    }
}