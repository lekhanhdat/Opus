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
using System.Linq;

namespace SqlKata
{
    public partial class Query
    {

        public Query Combine(string operation, bool all, Query query)
        {
            if (this.Method != "select" || query.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return AddComponent("combine", new Combine
            {
                Query = query,
                Operation = operation,
                All = all,
            });
        }

        public Query CombineRaw(string sql, params object[] bindings)
        {
            if (this.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return AddComponent("combine", new RawCombine
            {
                Expression = sql,
                Bindings = bindings,
            });
        }

        public Query Union(Query query, bool all = false)
        {
            return Combine("union", all, query);
        }

        public Query UnionAll(Query query)
        {
            return Union(query, true);
        }

        public Query Union(Func<Query, Query> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Union(query, all);
        }

        public Query UnionAll(Func<Query, Query> callback)
        {
            return Union(callback, true);
        }

        public Query UnionRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

        public Query Except(Query query, bool all = false)
        {
            return Combine("except", all, query);
        }

        public Query ExceptAll(Query query)
        {
            return Except(query, true);
        }

        public Query Except(Func<Query, Query> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Except(query, all);
        }

        public Query ExceptAll(Func<Query, Query> callback)
        {
            return Except(callback, true);
        }
        public Query ExceptRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

        public Query Intersect(Query query, bool all = false)
        {
            return Combine("intersect", all, query);
        }

        public Query IntersectAll(Query query)
        {
            return Intersect(query, true);
        }

        public Query Intersect(Func<Query, Query> callback, bool all = false)
        {
            var query = callback.Invoke(new Query());
            return Intersect(query, all);
        }

        public Query IntersectAll(Func<Query, Query> callback)
        {
            return Intersect(callback, true);
        }
        public Query IntersectRaw(string sql, params object[] bindings) => CombineRaw(sql, bindings);

    }
}