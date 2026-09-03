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
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class DatabaseUtility
    {
        public static readonly AveRetryPolicy ConnectionRetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5)));
        public static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5)));
        public static readonly int DefaultCommandTimeout = 600;
        private static RALogger logger = RALogger.GetInstance(typeof(DatabaseUtility));

        public static DateTime LastTimeCreated { get; set; } = DateTime.MinValue;

        #region Get Connection

        private static SqlConnection GetConnection(string connString)
        {
            SqlConnection con = null;
            ConnectionRetryPolicy.ExecuteAction(() =>
            {
                con = new SqlConnection(connString);
                con.Open();
            });
            return con;
        }
        #endregion

        public static string EscapeSqlParam(string value)
        {
            if (value != null)
            {
                return value.Replace("'", "''");
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<string> EscapeSqlParam(IEnumerable<string> values)
        {
            return values.Select(EscapeSqlParam);
        }

        public static string BuildInClause<T>(IEnumerable<T> values)
        {
            if (values == null || !values.Any())
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            if (typeof(int) == typeof(T) || typeof(long) == typeof(T))
            {
                foreach (T value in values)
                {
                    builder.Append(value).Append(',');
                }
            }
            else
            {
                foreach (T value in values)
                {
                    builder.Append('\'').Append(EscapeSqlParam(value?.ToString())).Append('\'').Append(',');
                }
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append(')');
            return builder.ToString();
        }

        public static string BuildInClause<T>(IEnumerable<T> array, out List<SqlParameter> objParams, int seedNum = 0)
        {
            var guidStr = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            int seed = seedNum;
            if (array == null || array.Count() == 0)
            {
                throw new ArgumentException("array");
            }
            int count = array.Count();
            objParams = new List<SqlParameter>();
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            int index = 0;
            foreach (T item in array)
            {
                string name = string.Concat("arg", guidStr, seed++.ToString());
                var tmpParam = new SqlParameter(name, item);
                objParams.Add(tmpParam);
                sb.Append("@").Append(name).Append(",");
                index++;
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");
            return sb.ToString();
        }

        public static string SafeReadString(DbDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : reader.GetString(columnIndex);
        }

        public static int SafeReadInt32(DbDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? 0 : reader.GetInt32(columnIndex);
        }

        public static long SafeReadInt64(DbDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? 0L : reader.GetInt64(columnIndex);
        }

        public static void BatchOperation<TSource>(
            IEnumerable<TSource> source,
            Action<IEnumerable<TSource>> action,
            int batchCount = 200)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            int count = source.Count();
            if (count == 0) { return; }
            int iterateCount = (count - 1) / batchCount + 1;
            for (int i = 0; i < iterateCount; i++)
            {
                var per = source.Skip(i * batchCount).Take(batchCount);
                action(per);
            }
        }

        public static async Task BatchOperationAsync<TSource>(
            IEnumerable<TSource> source,
            Func<IEnumerable<TSource>,Task> func,
            int batchCount = 200)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            int count = source.Count();
            if (count == 0) { return; }
            int iterateCount = (count - 1) / batchCount + 1;
            for (int i = 0; i < iterateCount; i++)
            {
                var per = source.Skip(i * batchCount).Take(batchCount);
                await func(per);
            }
        }

        public static void BatchOperation<TSource>(
            List<TSource> source,
            Action<IEnumerable<TSource>> action,
            int batchCount = 200)
        {
            BatchOperation((IEnumerable<TSource>)source, action, batchCount);
        }

        public static Dictionary<T, V> ExecuteOnBatch<T, V>(IEnumerable<string> collection, Func<List<string>, Dictionary<T, V>> func, int batch = 200)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("source");
            }
            var result = new Dictionary<T, V>();
            var total = collection.Count();
            var iteration = (total - 1) / batch + 1;
            for (int i = 0; i < iteration; i++)
            {
                var source = collection.Skip(i * batch).Take(batch).ToList();
                var r = func(source);
                foreach (var pair in r)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            return result;
        }

        public static List<V> ExecuteOnBatch<T, V>(IEnumerable<T> collection, Func<List<T>, List<V>> func, int batch = 200)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("source");
            }
            var result = new List<V>();
            var total = collection.Count();
            var iteration = (total - 1) / batch + 1;
            for (int i = 0; i < iteration; i++)
            {
                var source = collection.Skip(i * batch).Take(batch).ToList();
                result.AddRange(func(source));
            }
            return result;
        }

        /// <summary>
        /// Builds a paginated/limited query from a SELECT SQL.
        /// </summary>
        /// <param name="startRow">Start row</param>
        /// <param name="numberOfRows">Number/quatity of rows to be expected</param>
        /// <param name="sql">Original SQL (without its ordering clause)</param>
        /// <param name="orderingClause">MANDATORY: ordering clause (including ORDER BY keywords)</param>
        /// <returns>Paginated SQL ready to be executed.</returns>
        /// <remarks>SELECT keyword of original SQL must be placed exactly at the beginning of the SQL.</remarks>
        public static string GetPaginatedSQL(int startRow, int numberOfRows, string sql, string orderingClause)
        {
            // Ordering clause is mandatory!
            if (String.IsNullOrEmpty(orderingClause))
                throw new ArgumentNullException("orderingClause");

            // numberOfRows here is checked of disable building paginated/limited query
            // in case is not greater than 0. In this case we simply return the
            // query with its ordering clause appended to it. 
            // If ordering is not spe
            if (numberOfRows <= 0)
            {
                return String.Format("{0} {1}", sql, orderingClause);
            }
            // Extract the SELECT from the beginning.
            String partialSQL = sql.Remove(0, "SELECT ".Length);

            // Build the limited query...
            return String.Format(
                "SELECT * FROM ( SELECT ROW_NUMBER() OVER ({0}) AS ROWNUMBER, {1} ) AS SUB WHERE ROWNUMBER > {2} AND ROWNUMBER <= {3}",
                orderingClause,
                partialSQL,
                startRow.ToString(),
                (startRow + numberOfRows).ToString()
            );
        }
    }
}
