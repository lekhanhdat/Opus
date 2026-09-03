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

namespace SqlKata
{
    public partial class Query
    {

        private Query Join(Func<Join, Join> callback)
        {
            var join = callback.Invoke(new Join().AsInner());

            return AddComponent("join", new BaseJoin
            {
                Join = join
            });
        }

        public Query Join(
            string table,
            string first,
            string second,
            string op = "=",
            string type = "inner join"
        )
        {
            return Join(j => j.JoinWith(table).WhereColumns(first, op, second).AsType(type));
        }

        public Query Join(string table, Func<Join, Join> callback, string type = "inner join")
        {
            return Join(j => j.JoinWith(table).Where(callback).AsType(type));
        }

        public Query Join(Query query, Func<Join, Join> onCallback, string type = "inner join")
        {
            return Join(j => j.JoinWith(query).Where(onCallback).AsType(type));
        }

        public Query Join(Func<Query, Query> callback)
        {
            if (!this.HasComponent("from"))
            {
                throw new ArgumentException($"'{nameof(FromClause)}' of outer query cannot be empty used for including sub queries.");
            }

            var table = this.GetOneComponent<FromClause>("from").Table;

            if (this.HasComponent("as"))
            {
                table = this.GetOneComponent<AsClause>("as").Alias;
            }

            this.HasJoin = true;

            var childQuery = new Query().SetParent(this);
            childQuery.IsSubQuery = true;
            childQuery.ParentTable = table;

            return Join(j => j.JoinWith(callback.Invoke(childQuery)).AsType("JOIN"));
        }

       
        public Query LeftJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "left join");
        }

        public Query LeftJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "left join");
        }

        public Query LeftJoin(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "left join");
        }

        public Query RightJoin(string table, string first, string second, string op = "=")
        {
            return Join(table, first, second, op, "right join");
        }

        public Query RightJoin(string table, Func<Join, Join> callback)
        {
            return Join(table, callback, "right join");
        }

        public Query RightJoin(Query query, Func<Join, Join> onCallback)
        {
            return Join(query, onCallback, "right join");
        }

        public Query CrossJoin(string table)
        {
            return Join(j => j.JoinWith(table).AsCross());
        }

    }
}