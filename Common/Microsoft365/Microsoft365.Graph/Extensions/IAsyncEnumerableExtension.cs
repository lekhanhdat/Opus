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

//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;

//namespace System.Collections.Generic;

//public static class IAsyncEnumerableExtension
//{
//    public static async IAsyncEnumerable<TSource> DistinctBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
//    {
//        HashSet<TKey> seenKeys = new HashSet<TKey>();
//        await foreach (TSource item in source)
//        {
//            if (seenKeys.Add(keySelector(item)))
//            {
//                yield return item;
//            }
//        }
//    }

//    public static async IAsyncEnumerable<IEnumerable<TSource>> BatchAsync<TSource>(this IAsyncEnumerable<TSource> source, int size = 500)
//    {
//        if (size <= 0)
//        {
//            throw new ArgumentOutOfRangeException("size");
//        }

//        if (source == null)
//        {
//            throw new ArgumentNullException("source");
//        }

//        int count = 0;
//        TSource[]? bucket = null;
//        await foreach (TSource item in source)
//        {
//            if (bucket == null)
//            {
//                bucket = new TSource[size];
//            }

//            bucket[count++] = item;
//            if (count == size)
//            {
//                yield return bucket;
//                count = 0;
//                bucket = null;
//            }
//        }

//        if (bucket != null && count > 0)
//        {
//            yield return bucket.Take(count);
//        }
//    }

//    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IAsyncEnumerable<TSource> source, int size = 500)
//    {
//        if (size <= 0)
//        {
//            throw new ArgumentOutOfRangeException("size");
//        }

//        if (source == null)
//        {
//            throw new ArgumentNullException("source");
//        }

//        int num = 0;
//        TSource[]? array = null;
//        IAsyncEnumerator<TSource> enumerator = source.GetAsyncEnumerator();
//        while (Wait(enumerator.MoveNextAsync()))
//        {
//            TSource current = enumerator.Current;
//            if (array == null)
//            {
//                array = new TSource[size];
//            }

//            array[num++] = current;
//            if (num == size)
//            {
//                yield return array;
//                num = 0;
//                array = null;
//            }
//        }

//        if (array != null && num > 0)
//        {
//            yield return array.Take(num);
//        }
//    }

//    public static async IAsyncEnumerable<TResult> BatchAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, ValueTask<TResult>> func)
//    {
//        foreach (IEnumerable<TSource> item in source.Batch(size))
//        {
//            yield return await func(item);
//        }
//    }

//    public static async ValueTask BatchAsync<TSource>(this IAsyncEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, ValueTask> func)
//    {
//        foreach (IEnumerable<TSource> item in source.Batch(size))
//        {
//            await func(item);
//        }
//    }

//    private static T Wait<T>(ValueTask<T> task)
//    {
//        ValueTaskAwaiter<T> awaiter = task.GetAwaiter();
//        if (!awaiter.IsCompleted)
//        {
//            return task.AsTask().GetAwaiter().GetResult();
//        }

//        return awaiter.GetResult();
//    }

//#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
//    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerator)
//#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
//    {
//        return  asyncEnumerator.ToBlockingEnumerable().ToList();
//    }
//}