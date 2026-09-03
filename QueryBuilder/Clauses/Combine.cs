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
using System.Linq;

namespace SqlKata
{
    public abstract class AbstractCombine : AbstractClause
    {

    }

    public class Combine : AbstractCombine
    {
        /// <summary>
        /// Gets or sets the query to be combined with.
        /// </summary>
        /// <value>
        /// The query that will be combined.
        /// </value>
        public Query Query { get; set; }

        /// <summary>
        /// Gets or sets the combine operation, e.g. "UNION", etc.
        /// </summary>
        /// <value>
        /// The combine operation.
        /// </value>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Combine"/> clause will combine all.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all; otherwise, <c>false</c>.
        /// </value>
        public bool All { get; set; } = false;

        public override AbstractClause Clone()
        {
            return new Combine
            {
                Engine = Engine,
                Operation = Operation,
                Component = Component,
                Query = Query,
                All = All,
            };
        }
    }

    public class RawCombine : AbstractCombine
    {
        public string Expression { get; set; }

        public object[] Bindings { get; set; }

        public override AbstractClause Clone()
        {
            return new RawCombine
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
                Bindings = Bindings,
            };
        }
    }
}