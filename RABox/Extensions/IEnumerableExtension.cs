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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RABox.Extensions
{
    public static class IEnumerableExtension
    {
        public static async Task ParallelExecute<TSource>(this IEnumerable<TSource> source,
            Func<TSource, Task> action, int maxThread, CancellationToken cancellationToken = default)
        {
            using (var semaphore = new SemaphoreSlim(maxThread, maxThread))
            {
                foreach (var data in source)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await semaphore.WaitAsync();
                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            await action(data);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                }

                while (!cancellationToken.IsCancellationRequested && semaphore.CurrentCount != maxThread)
                    await Task.Delay(1000);
            }
        }

        public static IEnumerable<T> PrecacheData<T>(this IEnumerable<T> source, int cacheSize)
        {
            return new PrecacheIEnumerable<T>(source, cacheSize);
        }

        private class PrecacheIEnumerable<T> : IEnumerable<T>
        {
            private readonly int catchSize;
            private readonly IEnumerable<T> source;
            private IEnumerator<T> enumerator;
            private List<T> array;
            public PrecacheIEnumerable(IEnumerable<T> source, int cacheSize)
            {
                catchSize = cacheSize;
                array = new List<T>();
                this.source = source;
                enumerator = source.GetEnumerator();
                PreCacheData();
            }

            private IEnumerable<T> Get()
            {
                foreach (var item in array)
                    yield return item;

                var count = array.Count;
                array = null;
                if (count < catchSize)
                    yield break;

                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }

            private void PreCacheData()
            {
                var index = 0;
                while (index < catchSize && enumerator.MoveNext())
                {
                    array.Add(enumerator.Current);
                    index++;
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (array == null)
                {
                    return source.GetEnumerator();
                }
                return Get().GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
