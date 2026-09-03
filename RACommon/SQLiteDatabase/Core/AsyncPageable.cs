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

namespace RACommon.SQLiteDatabase.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AsyncPageable<T> : IAsyncEnumerable<T> where T : notnull
{
    private readonly Func<String, Task<Page<T>>> pageFucn;
    private String sql;

    internal AsyncPageable(Func<String, Task<Page<T>>> pageFucn, String sql)
    {
        ArgumentNullException.ThrowIfNull(pageFucn);
        sql.ThrowIfNullOrEmpty();

        this.pageFucn = pageFucn;
        this.sql = sql;
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await foreach (var page in AsPages().ConfigureAwait(false).WithCancellation(cancellationToken))
        {
            foreach (var value in page.Values)
            {
                yield return value;
            }
        }
    }

    private async IAsyncEnumerable<Page<T>> AsPages()
    {
        do
        {
            var result = await pageFucn(sql!);
            yield return result;
            sql = result.NextSql!;
        } while (sql.IsNotNullOrEmpty());
    }
}
