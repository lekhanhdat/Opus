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
namespace SqlKata
{
    public abstract class AbstractColumn : AbstractClause
    {
    }

    /// <summary>
    /// Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class Column : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        /// The column name.
        /// </value>
        public string Name { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Column
            {
                Engine = Engine,
                Name = Name,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents column clause calculated using query.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class QueryColumn : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the query that will be used for column value calculation.
        /// </summary>
        /// <value>
        /// The query for column value calculation.
        /// </value>
        public Query Query { get; set; }
        public override AbstractClause Clone()
        {
            return new QueryColumn
            {
                Engine = Engine,
                Query = Query.Clone(),
                Component = Component,
            };
        }
    }

    public class RawColumn : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the RAW expression.
        /// </summary>
        /// <value>
        /// The RAW expression.
        /// </value>
        public string Expression { get; set; }
        public object[] Bindings { set; get; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }
    }
}