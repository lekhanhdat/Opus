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
using System.Text;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Common;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Common
{
    public class AveSqlUtility
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> mFieldMaps = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static Dictionary<string, FieldInfo> GetFieldMap(Type type, string prefix)
        {
            if (!mFieldMaps.ContainsKey(type))
            {
                Dictionary<string, FieldInfo> fieldMap = new Dictionary<string, FieldInfo>();
                foreach (FieldInfo fieldInfo in type.GetFields())
                {
                    if (string.IsNullOrEmpty(prefix))
                    {
                        fieldMap[fieldInfo.Name] = fieldInfo;
                    }
                    else
                    {
                        fieldMap[prefix + fieldInfo.Name] = fieldInfo;
                    }
                }
                lock (mFieldMaps)
                {
                    if (!mFieldMaps.ContainsKey(type))
                    {
                        mFieldMaps[type] = fieldMap;
                    }
                }
            }
            return mFieldMaps[type];
        }

        public static void GetDBRow(IDictionary<string, object> data, AveSqlConnection sqlConn, string cmdText)
        {
            GetDBRow(data, sqlConn, cmdText, 0);
        }

        public static void GetDBRow(IDictionary<string, object> data, AveSqlConnection sqlConn, string cmdText, int startIndex)
        {
            using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
            {
                if (!dr.Read())
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindData);
                }
                GetDBRow(data, dr, startIndex);
            }
        }

        public static bool TryGetDBRow(IDictionary<string, object> data, AveSqlConnection sqlConn, string cmdText)
        {
            return TryGetDBRow(data, sqlConn, cmdText, 0);
        }

        public static bool TryGetDBRow(IDictionary<string, object> data, AveSqlConnection sqlConn, string cmdText, int startIndex)
        {
            using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
            {
                if (!dr.Read())
                {
                    return false;
                }
                GetDBRow(data, dr, startIndex);
                return true;
            }
        }

        public static void GetDBRow(IDictionary<string, object> data, SqlDataReader sqlReader, bool includeDBNull = false)
        {
            GetDBRow(data, sqlReader, 0, includeDBNull);
        }

        public static void GetDBRow(IDictionary<string, object> data, SqlDataReader sqlReader, int startIndex, bool includeDBNull = false)
        {
            int fieldCount = sqlReader.FieldCount;
            for (int i = startIndex; i < fieldCount; i++)
            {
                if (!includeDBNull && sqlReader.IsDBNull(i))
                {
                    continue;
                }
                string name = sqlReader.GetName(i);
                object value = sqlReader.GetValue(i);
                data[name] = value;
            }
        }

        public static List<T> GetDBRows<T>(AveSqlConnection sqlConn, string cmdText)
        {
            return GetDBRows<T>(sqlConn, cmdText, null);
        }

        public static List<T> GetDBRows<T>(AveSqlConnection sqlConn, string cmdText, string prefix)
        {
            List<T> values = null;
            GetDBRows<T>(ref values, sqlConn, cmdText, prefix);
            return values;
        }

        public static void GetDBRows<T>(ref List<T> values, AveSqlConnection sqlConn, string cmdText)
        {
            GetDBRows<T>(ref values, sqlConn, cmdText, null);
        }

        public static void GetDBRows<T>(ref List<T> values, AveSqlConnection sqlConn, string cmdText, string prefix)
        {
            Type type = typeof(T);
            Dictionary<string, FieldInfo> fieldMap = GetFieldMap(type, prefix);
            using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    T value = (T)AveAssemblyUtility.CreateInstanceByType(type);
                    GetDBRow(value, dr, fieldMap, 0);
                    if (values == null)
                    {
                        values = new List<T>();
                    }
                    values.Add(value);
                }
            }
        }

        public static List<Dictionary<string, object>> GetDBRows(AveSqlConnection sqlConn, string cmdText, bool includeDBNull = false)
        {
            using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
            {
                return GetDBRows(dr, includeDBNull);
            }
        }

        public static List<Dictionary<string, object>> GetDBRows(SqlDataReader dr, bool includeDBNull)
        {
            List<Dictionary<string, object>> rows = null;
            while (dr.Read())
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                GetDBRow(dic, dr, includeDBNull);
                if (rows == null)
                {
                    rows = new List<Dictionary<string, object>>();
                }
                rows.Add(dic);
            }
            return rows;
        }

        public static void GetDBRow(object data, AveSqlConnection sqlConn, string cmdText)
        {
            GetDBRow(data, sqlConn, cmdText, null, 0);
        }

        public static void GetDBRow(object data, AveSqlConnection sqlConn, string cmdText, string prefix)
        {
            GetDBRow(data, sqlConn, cmdText, prefix, 0);
        }

        public static void GetDBRow(object data, AveSqlConnection sqlConn, string cmdText, string prefix, int startIndex)
        {
            Dictionary<string, FieldInfo> fieldMap = GetFieldMap(data.GetType(), prefix);
            using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
            {
                if (!dr.Read())
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindData);
                }
                GetDBRow(data, dr, fieldMap, startIndex);
            }
        }

        public static void GetDBRow(object data, SqlDataReader sqlReader, Dictionary<string, FieldInfo> fieldMap, int startIndex)
        {
            if (data == null)
            {
                return;
            }
            int fieldCount = sqlReader.FieldCount;

            for (int i = startIndex; i < fieldCount; i++)
            {
                if (sqlReader.IsDBNull(i))
                {
                    continue;
                }
                string name = sqlReader.GetName(i);
                object value = sqlReader.GetValue(i);
                if (fieldMap.ContainsKey(name))
                {
                    Type fieldType = fieldMap[name].FieldType;
                    if (sqlReader.GetFieldType(i).IsAssignableFrom(fieldType))
                    {
                        fieldMap[name].SetValue(data, value);
                    }
                    else
                    {
                        fieldMap[name].SetValue(data, AveConvert.ChangeType(value, fieldType));
                    }
                }
            }
        }
    }
}
