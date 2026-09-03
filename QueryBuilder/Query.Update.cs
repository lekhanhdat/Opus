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
using System.Linq;
using System.Reflection;

namespace SqlKata
{
    public partial class Query
    {
        public Query AsUpdate(object data)
        {
            var dictionary = BuildKeyValuePairsFromObject(data, considerKeys: true);

            return AsUpdate(dictionary);
        }

        public Query AsUpdate(IEnumerable<string> columns, IEnumerable<object> values)
        {
            if ((columns?.Any() ?? false) == false || (values?.Any() ?? false) == false)
            {
                throw new InvalidOperationException($"{columns} and {values} cannot be null or empty");
            }

            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException($"{columns} count should be equal to {values} count");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = columns.ToList(),
                Values = values.ToList()
            });

            return this;
        }

        public Query AsUpdate(IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values == null || values.Any() == false)
            {
                throw new InvalidOperationException($"{values} cannot be null or empty");
            }

            Method = "update";

            ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = values.Select(x => x.Key).ToList(),
                Values = values.Select(x => x.Value).ToList(),
            });

            return this;
        }

        public Query AsIncrement(string column, int value = 1)
        {
            Method = "update";
            AddOrReplaceComponent("update", new IncrementClause
            {
                Column = column,
                Value = value
            });

            return this;
        }

        public Query AsDecrement(string column, int value = 1)
        {
            return AsIncrement(column, -value);
        }
    }
}
