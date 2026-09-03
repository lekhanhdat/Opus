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
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class ListExtension
    {
        public async static Task<List<TOutput>> ConvertAllAsync<T,TOutput>(this List<T> list, Converter<T, Task<TOutput>> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            List<TOutput> olist = new List<TOutput>(list.Count);

            foreach(var item in list)
            {
                olist.Add(await converter(item));
            }

            //for (int i = 0; i < list.Count; i++)
            //{
            //    olist[i] = await converter(list[i]);
            //}

            return olist;
        }

        public async static Task ForEachAsync<T>(this List<T> list, Func<T,Task> func)
        {
                if (func == null)
                {
                    throw new ArgumentNullException(nameof(func));
                }
                foreach (var item in list)
                {
                    await func(item);
                }
           
        }
    }
}
