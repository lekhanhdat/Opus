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

//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics.CodeAnalysis;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace System.Linq;

//public static class IEnumerableExtension
//{
//    public static void ForEach<T>(this IEnumerable<T> source, Action<T> func)
//    {
//        foreach (T item in source)
//        {
//            func(item);
//        }
//    }

//    public static IEnumerable<TOutput> ConvertAll<TInput, TOutput>(this IEnumerable<TInput> source, Func<TInput, TOutput> func)
//    {
//        return source.Select(func);
//    }

//    public static bool IsNullOrEmpty<TInput>([NotNullWhen(false)] this IEnumerable<TInput> source)
//    {
//        if (source != null)
//        {
//            return !source.Any();
//        }

//        return true;
//    }

//    public static bool IsNotNullOrEmpty<TInput>([NotNullWhen(true)] this IEnumerable<TInput> source)
//    {
//        return source?.Any() ?? false;
//    }

//    public static IEnumerable<TInput> EnsureNotNullOrEmpty<TInput>([NotNull] this IEnumerable<TInput> source)
//    {
//        if (source.IsNullOrEmpty())
//        {
//            throw new ArgumentNullException();
//        }

//        return source;
//    }

//    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
//    {
//        TSource[]? array = null;
//        int num = 0;
//        foreach (TSource item in source)
//        {
//            if (array == null)
//            {
//                array = new TSource[size];
//            }

//            array[num++] = item;
//            if (num == size)
//            {
//                yield return array;
//                array = null;
//                num = 0;
//            }
//        }

//        if (array != null && num > 0)
//        {
//            yield return array.Take(num);
//        }
//    }

//    public static void Batch<T>(this IEnumerable<T> source, Action<IEnumerable<T>> action, int size)
//    {
//        foreach (IEnumerable<T> item in source.Batch(size))
//        {
//            action(item);
//        }
//    }

//    public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, Func<IEnumerable<TSource>, TResult> func, int size)
//    {
//        foreach (IEnumerable<TSource> item in source.Batch(size))
//        {
//            yield return func(item);
//        }
//    }

//    public static async IAsyncEnumerable<TResult> BatchAsync<TSource, TResult>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, ValueTask<TResult>> func)
//    {
//        foreach (IEnumerable<TSource> item in source.Batch(size))
//        {
//            yield return await func(item);
//        }
//    }

//    public static async ValueTask BatchAsync<TSource>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, ValueTask> func)
//    {
//        foreach (IEnumerable<TSource> item in source.Batch(size))
//        {
//            await func(item);
//        }
//    }

//    public static bool HasValue<TSource>([NotNullWhen(true)] this IEnumerable<TSource> collection)
//    {
//        return !collection.IsNullOrEmpty();
//    }

//    //public static DataTable ToDataTable<T>(this IEnumerable<T> items)
//    //{
//    //    DataTable dataTable = new DataTable(typeof(T).Name);
//    //    PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
//    //    PropertyInfo[] array = properties;
//    //    foreach (PropertyInfo propertyInfo in array)
//    //    {
//    //        Type coreType = GetCoreType(propertyInfo.PropertyType);
//    //        dataTable.Columns.Add(propertyInfo.Name, coreType);
//    //    }

//    //    foreach (T item in items)
//    //    {
//    //        object[] array2 = new object[properties.Length];
//    //        for (int j = 0; j < properties.Length; j++)
//    //        {
//    //            array2[j] = properties[j].GetValue(item, null);
//    //        }

//    //        dataTable.Rows.Add(array2);
//    //    }

//    //    return dataTable;
//    //}

//    //private static Type GetCoreType(Type t)
//    //{
//    //    if (t != null && IsNullable(t))
//    //    {
//    //        if (!t.IsValueType)
//    //        {
//    //            return t;
//    //        }

//    //        return Nullable.GetUnderlyingType(t);
//    //    }

//    //    return t;
//    //}

//    private static bool IsNullable(Type t)
//    {
//        if (t.IsValueType)
//        {
//            if (t.IsGenericType)
//            {
//                return t.GetGenericTypeDefinition() == typeof(Nullable<>);
//            }

//            return false;
//        }

//        return true;
//    }
//}