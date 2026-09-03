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

namespace SqlKata
{
    public abstract class AbstractCondition : AbstractClause
    {
        public bool IsOr { get; set; } = false;
        public bool IsNot { get; set; } = false;

        //public bool ForJoin { get; set; } = false;
    }

    /// <summary>
    /// Represents a comparison between a column and a value.
    /// </summary>
    public class BasicCondition : AbstractCondition
    {
        public string Column { get; set; }
        public string Operator { get; set; }
        public virtual object Value { get; set; }

        public string Func { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicCondition
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Value = Value,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                Func = Func,
                //ForJoin = ForJoin
            };
        }
    }

    public class BasicStringCondition : BasicCondition
    {

        public bool CaseSensitive { get; set; } = false;

        private string escapeCharacter = null;
        public string EscapeCharacter
        {
            get => escapeCharacter;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = null;
                else if (value.Length > 1)
                    throw new ArgumentOutOfRangeException($"The {nameof(EscapeCharacter)} can only contain a single character!");
                escapeCharacter = value;
            }
        }
        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicStringCondition
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Value = Value,
                IsOr = IsOr,
                IsNot = IsNot,
                CaseSensitive = CaseSensitive,
                EscapeCharacter = EscapeCharacter,
                Component = Component,
                Func = Func,
                //ForJoin = ForJoin
            };
        }
    }

    public class BasicDateCondition : BasicCondition
    {
        public string Part { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicDateCondition
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Value = Value,
                IsOr = IsOr,
                IsNot = IsNot,
                Part = Part,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents a comparison between two columns.
    /// </summary>
    public class TwoColumnsCondition : AbstractCondition
    {
        public string First { get; set; }
        public string Operator { get; set; }
        public string Second { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new TwoColumnsCondition
            {
                Engine = Engine,
                First = First,
                Operator = Operator,
                Second = Second,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents a comparison between a column and a full "subquery".
    /// </summary>
    public class QueryCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        public string Column { get; set; }
        public string Operator { get; set; }
        public Query Query { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryCondition<T>
            {
                Engine = Engine,
                Column = Column,
                Operator = Operator,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents a comparison between a full "subquery" and a value.
    /// </summary>
    public class SubQueryCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        public object Value { get; set; }
        public string Operator { get; set; }
        public Query Query { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new SubQueryCondition<T>
            {
                Engine = Engine,
                Value = Value,
                Operator = Operator,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents a "is in" condition.
    /// </summary>
    public class InCondition<T> : AbstractCondition
    {
        public string Column { get; set; }
        public IEnumerable<T> Values { get; set; }
        public override AbstractClause Clone()
        {
            return new InCondition<T>
            {
                Engine = Engine,
                Column = Column,
                Values = new List<T>(Values),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }

    }

    /// <summary>
    /// Represents a "is in subquery" condition.
    /// </summary>
    public class InQueryCondition : AbstractCondition
    {
        public Query Query { get; set; }
        public string Column { get; set; }
        public override AbstractClause Clone()
        {
            return new InQueryCondition
            {
                Engine = Engine,
                Column = Column,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents a "is between" condition.
    /// </summary>
    public class BetweenCondition<T> : AbstractCondition
    {
        public string Column { get; set; }
        public T Higher { get; set; }
        public T Lower { get; set; }
        public override AbstractClause Clone()
        {
            return new BetweenCondition<T>
            {
                Engine = Engine,
                Column = Column,
                Higher = Higher,
                Lower = Lower,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents an "is null" condition.
    /// </summary>
    public class NullCondition : AbstractCondition
    {
        public string Column { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new NullCondition
            {
                Engine = Engine,
                Column = Column,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents a boolean (true/false) condition.
    /// </summary>
    public class BooleanCondition : AbstractCondition
    {
        public string Column { get; set; }
        public bool Value { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BooleanCondition
            {
                Engine = Engine,
                Column = Column,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                Value = Value,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents a "nested" clause condition.
    /// i.e OR (myColumn = "A")
    /// </summary>
    public class NestedCondition<T> : AbstractCondition where T : BaseQuery<T>
    {
        public T Query { get; set; }
        public override AbstractClause Clone()
        {
            return new NestedCondition<T>
            {
                Engine = Engine,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    /// <summary>
    /// Represents an "exists sub query" clause condition.
    /// </summary>
    public class ExistsCondition : AbstractCondition
    {
        public Query Query { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new ExistsCondition
            {
                Engine = Engine,
                Query = Query.Clone(),
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }

    public class RawCondition : AbstractCondition
    {
        public string Expression { get; set; }
        public object[] Bindings { set; get; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawCondition
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                IsOr = IsOr,
                IsNot = IsNot,
                Component = Component,
                //ForJoin = ForJoin
            };
        }
    }
}