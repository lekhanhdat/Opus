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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.AI
{
    public static class TokenUsageCache
    {
        private static readonly ConcurrentDictionary<Guid, long> _totals = new();
        private static readonly AsyncLocal<Guid?> _currentId = new();

        public sealed class Scope : IDisposable
        {
            private readonly Guid _id;
            private bool _ended;

            internal Scope(Guid id) { _id = id; }

            public long End()
            {
                if (_ended) return 0;
                _ended = true;
                _currentId.Value = null;
                return _totals.TryRemove(_id, out var total) ? total : 0;
            }

            public void Dispose() => End();
        }

        public static Scope Begin()
        {
            var id = Guid.NewGuid();
            _currentId.Value = id;
            _totals[id] = 0;
            return new Scope(id);
        }

        public static void Add(long usage)
        {
            var id = _currentId.Value;
            if (id is null) return;
            _totals.AddOrUpdate(id.Value, usage, (_, old) => checked(old + usage));
        }
    }
}
