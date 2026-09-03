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

namespace AvePoint.Wrapper.Common
{
    public class AveEqualityComparer<T1, T2> : IEqualityComparer<T1>
    {
        private Func<T1, T2> keySelector;
        private IEqualityComparer<T2> comparer;
        public AveEqualityComparer(Func<T1, T2> keySelector, IEqualityComparer<T2> comparer)
        {
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public AveEqualityComparer(Func<T1, T2> keySelector)
            : this(keySelector, EqualityComparer<T2>.Default)
        { }

        public bool Equals(T1 x, T1 y)
        {
            return comparer.Equals(keySelector(x), keySelector(y));
        }

        public int GetHashCode(T1 obj)
        {
            return comparer.GetHashCode(keySelector(obj));
        }
    }

    public static class DistinctExtensions
    {
        public static IEnumerable<TKey> Distinct<TKey, TValue>(this IEnumerable<TKey> source, Func<TKey, TValue> keySelector)
        {
            return source.Distinct(new AveEqualityComparer<TKey, TValue>(keySelector));
        }

        public static IEnumerable<TKey> Distinct<TKey, TValue>(this IEnumerable<TKey> source, Func<TKey, TValue> keySelector, IEqualityComparer<TValue> comparer)
        {
            return source.Distinct(new AveEqualityComparer<TKey, TValue>(keySelector, comparer));
        }
    }

}
