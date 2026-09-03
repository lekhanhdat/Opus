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
    public class Join : BaseQuery<Join>
    {
        protected string _type = "inner join";

        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value.ToUpperInvariant();
            }
        }

        public Join() : base()
        {
        }

        public override Join Clone()
        {
            var clone = base.Clone();
            clone._type = _type;
            return clone;
        }

        public Join AsType(string type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Alias for "from" operator.
        /// Since "from" does not sound well with join clauses
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Join JoinWith(string table) => From(table);
        public Join JoinWith(Query query) => From(query);
        public Join JoinWith(Func<Query, Query> callback) => From(callback);

        public Join AsInner() => AsType("inner join");
        public Join AsOuter() => AsType("outer join");
        public Join AsLeft() => AsType("left join");
        public Join AsRight() => AsType("right join");
        public Join AsCross() => AsType("cross join");

        public Join On(string first, string second, string op = "=")
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });

        }

        public Join OrOn(string first, string second, string op = "=")
        {
            return Or().On(first, second, op);
        }

        public override Join NewQuery()
        {
            return new Join();
        }
    }
}