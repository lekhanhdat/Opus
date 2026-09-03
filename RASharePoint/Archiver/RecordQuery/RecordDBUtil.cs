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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.PhysicalCore.SQL
{
    public class RecordDBUtil
    {

        //private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        /*public static object ExecuteCommandScalar(string connectionString, string cmdString, SqlParameter[] parameters = null, CommandType cmdType = CommandType.Text)
        {
 
            using (var sqlConn = new SqlConnection(connectionString))
            {
                using (var sqlComm = sqlConn.CreateCommand())
                {
                    sqlComm.CommandTimeout = RecordDBConstString.DEFAULT_COMMAND_TIMEOUT;
                    sqlComm.Parameters.Clear();
                    if (parameters != null)
                    {
                        sqlComm.Parameters.AddRange(parameters);
                    }
                    sqlComm.CommandText = cmdString;
                    sqlComm.CommandType = cmdType;

                    sqlConn.Open();
                    return sqlComm.ExecuteScalar();
                }
            }
            
        }*/

        /*public static int ExecuteNonQuery(string connectionString, string cmdString, SqlParameter[] parameters = null, CommandType cmdType = CommandType.Text)
        {
           
            using (var sqlConn = new SqlConnection(connectionString))
            {
                using (var sqlComm = sqlConn.CreateCommand())
                {
                    sqlComm.CommandTimeout = RecordDBConstString.DEFAULT_COMMAND_TIMEOUT;

                    if (parameters != null)
                    {
                        sqlComm.Parameters.AddRange(parameters);
                    }
                    sqlComm.CommandText = cmdString;
                    sqlComm.CommandType = cmdType;

                    sqlConn.Open();
                    int s = sqlComm.ExecuteNonQuery();
                    sqlComm.Parameters.Clear();
                    return s;
                }
            }
            
        }*/
        /*public static int ExecuteNonQuery(string connectionString, string cmdString, Dictionary<string, object> parameters = null, CommandType cmdType = CommandType.Text)
        {
          
            using (var sqlConn = new SqlConnection(connectionString))
            {
                using (var sqlComm = sqlConn.CreateCommand())
                {
                    sqlComm.CommandTimeout = RecordDBConstString.DEFAULT_COMMAND_TIMEOUT;

                    if (parameters != null)
                    {
                        foreach (var para in parameters)
                        {
                            sqlComm.Parameters.AddWithValue(para.Key, para.Value);
                        }
                    }
                    sqlComm.CommandText = cmdString;
                    sqlComm.CommandType = cmdType;

                    sqlConn.Open();
                    int s = sqlComm.ExecuteNonQuery();
                    sqlComm.Parameters.Clear();
                    return s;
                }
            }
           
        }*/


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
    }


}
