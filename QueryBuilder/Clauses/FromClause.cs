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
    public abstract class AbstractFrom : AbstractClause
    {
        protected string _alias;

        /// <summary>
        /// Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public virtual string Alias { get => _alias; set => _alias = value; }
    }

    /// <summary>
    /// Represents a "from" clause.
    /// </summary>
    public class FromClause : AbstractFrom
    {
        public string Table { get; set; }

        public override string Alias
        {
            get
            {
                if (Table.IndexOf(" as ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var segments = Table.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    return segments[2];
                }

                return Table;
            }
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new FromClause
            {
                Engine = Engine,
                Alias = Alias,
                Table = Table,
                Component = Component,
            };
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FromParentClause : AbstractFrom
    {
        /// <summary>
        /// from Alias
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// in parentTable.Path
        /// </summary>
        public string Path { get; set; }
        public override AbstractClause Clone()
        {
            return new FromParentClause
            {
               Table = Table,
               Path = Path,
               Component = Component,
               
            };
        }
    }

    /// <summary>
    /// Represents a "from subquery" clause.
    /// </summary>
    public class QueryFromClause : AbstractFrom
    {
        public Query Query { get; set; }

        public override string Alias
        {
            get
            {
                return string.IsNullOrEmpty(_alias) ? Query.QueryAlias : _alias;
            }
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawFromClause : AbstractFrom
    {
        public string Expression { get; set; }
        public object[] Bindings { set; get; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }
    }
}