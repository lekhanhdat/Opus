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

namespace RACommon.SQLiteDatabase;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

public static class SQLiteResolver
{
    private static readonly IDictionary<Type, String> cachedInsertSqlCommandTextDictionary = new ConcurrentDictionary<Type, String>();
    private static readonly IDictionary<Type, Dictionary<String, String>> cachedIndexSqlColumnNamesDictionary = new ConcurrentDictionary<Type, Dictionary<String, String>>();

    public static String ToInsertCommand(this Object index)
    {
        var indexType = index.GetType();
        if (!cachedInsertSqlCommandTextDictionary.ContainsKey(indexType))
        {
            var tableName = default(String);
            var mapTableAttributeArray = indexType.GetCustomAttributes(typeof(TableAttribute), false);
            if (mapTableAttributeArray.Length > 0)
                tableName = ((TableAttribute)mapTableAttributeArray[0]).TableName;
            else throw new Exception($"MapTable attribute is not specified for the class: [{index.GetType().FullName}].");
            var columns = new StringBuilder();
            var dbParams = new StringBuilder();
            Array.ForEach(indexType.GetProperties(), item =>
            {
                //var itemValue = item.FastGetValue(index);
                var mapColumnAttributeArrary = item.GetCustomAttributes(typeof(ColumnAttribute), false);
                var columnName = default(String);
                if (mapColumnAttributeArrary.Length > 0)
                {
                    columnName = ((ColumnAttribute)mapColumnAttributeArrary[0]).Name;
                    if (columns.Length == 0)
                    {
                        columns.Append(columnName);
                        dbParams.Append("@" + columnName);
                    }
                    else
                    {
                        columns.AppendFormat(", {0}", columnName);
                        dbParams.AppendFormat(", @{0}", columnName);
                    }
                }
            });
            var indexTypeInsertCommandText = "INSERT INTO [{0}] ({1}) VALUES ({2})".FormatWith(tableName, columns, dbParams);
            cachedInsertSqlCommandTextDictionary.AddOrReplace(indexType, indexTypeInsertCommandText);
        }
        return cachedInsertSqlCommandTextDictionary[indexType];
    }

    public static T ToM<T>(this DbDataReader dataReader)
        where T : class
    {
        var instance = Activator.CreateInstance<T>();
        Array.ForEach(typeof(T).GetProperties(), item =>
        {
            var mappedColumnOrdinal = dataReader.GetOrdinal(item.Name);
            if (mappedColumnOrdinal != -1)
            {
                var colValue = default(Object);
                if (!dataReader.IsDBNull(mappedColumnOrdinal))
                    colValue = dataReader.GetValue(mappedColumnOrdinal);
                if (colValue != null && !colValue.IsType(item.PropertyType))
                    colValue = Convert.ChangeType(colValue, item.PropertyType);
                else if (colValue == null && typeof(ValueType).IsAssignableFrom(item.PropertyType))
                    colValue = Activator.CreateInstance(item.PropertyType);
                item.FastSetValue(instance, colValue!);
            }
        });
        return instance;
    }

    public static T ToEntity<T>(this DbDataReader dataReader)
        where T : IInsertable
    {
        var instance = Activator.CreateInstance<T>();
        var propertyColumnNameDic = GetMappedPropertyColumnNameMappedDictionary(typeof(T));
        Array.ForEach(typeof(T).GetProperties(), item =>
        {
            if (propertyColumnNameDic.ContainsKey(item.Name))
            {
                var mappedColumnOrdinal = -1;
                var mappedColumnName = propertyColumnNameDic[item.Name];
                mappedColumnOrdinal = dataReader.GetOrdinal(mappedColumnName);
                if (mappedColumnOrdinal != -1)
                {
                    var colValue = default(Object);
                    if (!dataReader.IsDBNull(mappedColumnOrdinal))
                        colValue = dataReader.GetValue(mappedColumnOrdinal);
                    if (colValue != null && !colValue.IsType(item.PropertyType))
                        colValue = Convert.ChangeType(colValue, item.PropertyType);
                    else if (colValue == null && typeof(ValueType).IsAssignableFrom(item.PropertyType))
                        colValue = Activator.CreateInstance(item.PropertyType);
                    item.FastSetValue(instance, colValue!);
                }
            }
        });
        return instance;
    }

    public static (string Condition, Dictionary<String, Object> Parameters) In<T>(List<T> list)
    {
        var index = 0;
        var sb = new StringBuilder();
        sb.Append('(');
        var parameters = new Dictionary<String, Object>();
        list.ForEach(l =>
        {
            var name = $"arg{index++}";
            parameters[name] = l!;
            sb.Append('@').Append(name).Append(',');
        });
        sb.Remove(sb.Length - 1, 1);
        sb.Append(')');
        return (sb.ToString(), parameters);
    }

    private static Dictionary<String, String> GetMappedPropertyColumnNameMappedDictionary(Type indexType)
    {
        if (!cachedIndexSqlColumnNamesDictionary.ContainsKey(indexType))
        {
            var mappedDictionary = new Dictionary<String, String>();
            Array.ForEach(indexType.GetProperties(), item =>
            {
                var mappedColumnName = default(String);
                var mapColumnAttributeArray = item.GetCustomAttributes(typeof(ColumnAttribute), false);
                if (mapColumnAttributeArray.Length > 0)
                {
                    var mapColumnAttribute = (ColumnAttribute)mapColumnAttributeArray[0];
                    mappedColumnName = mapColumnAttribute.Name;
                    mappedDictionary.Add(item.Name, mappedColumnName);
                }
            });
            cachedIndexSqlColumnNamesDictionary.AddOrReplace(indexType, mappedDictionary);
        }
        return cachedIndexSqlColumnNamesDictionary[indexType];
    }
}
