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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core
{
    public class DatabaseContext: IDatabaseContext
    {
        private SqlConnection conn;

        public DatabaseContext(SqlConnection conn)
        {
            this.conn = conn;
        }

        public DatabaseContext()
        {
            this.conn = new SqlConnection(DatabaseUtility.GetSystemDbConnectionString());
        }

        public void Dispose()
        {
            conn.Dispose();
        }

        public int ExecuteNonQuery(string query, params DbParameter[] parameters)
        {
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
            cmd.CommandText = query;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            int result = cmd.ExecuteNonQuery();
            return result;
        }

        public DbDataReader ExecuteQuery(string query, params DbParameter[] parameters)
        {
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
            cmd.CommandText = query;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            var result = cmd.ExecuteReader();
            return result;
        }

        public T ExecuteScalar<T>(string query, params DbParameter[] parameters)
        {
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
            cmd.CommandText = query;
            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            var result = cmd.ExecuteScalar();
            if (result == null)
            {
                return default(T);
            }
            else
            {
                if (Convert.IsDBNull(result))
                {
                    return default(T);
                }
                else
                {
                    return (T)result;
                }
            }
        }

        public void BatchCommit(DataTable dataTable)
        {
            using (dataTable)
            {
                using (var bulkCopy = new SqlBulkCopy(conn))
                {
                    bulkCopy.DestinationTableName = dataTable.TableName;
                    bulkCopy.BulkCopyTimeout = 300;
                    bulkCopy.BatchSize = 1000;
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

       
    }
}
