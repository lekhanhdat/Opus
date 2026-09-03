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

namespace RACommon.SQLiteDatabase.Core;

using System;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Extensions.Caching.Memory;

using Dapper;
using System.Collections.Generic;

internal static class Mapper
{
    private static readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

    public static void SetTypeMap(Type type)
    {
        _ = cache.GetOrCreate($"{nameof(SetTypeMap)}{type.TypeHandle.Value}", cacheEntry =>
        {
            var props = GetTableProperies(type);
            var map = new CustomPropertyTypeMap(
                type,
                (type, columnName) => props.FirstOrDefault(prop => String.Equals(GetColumnName(prop, type), columnName, StringComparison.OrdinalIgnoreCase))!);
            SqlMapper.SetTypeMap(type, map);
            return true;
        });
    }

    public static String GetInsertSql(Type type)
    {
        return cache.GetOrCreate($"{nameof(GetInsertSql)}{type.TypeHandle.Value}", cacheEntry =>
        {
            cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);

            var tableName = GetTableName(type);

            var columns = new List<String>();
            var @params = new List<String>();

            GetTableProperies(type).ForEach(p =>
            {
                columns.Add(GetColumnName(p, type));
                @params.Add($"@{p.Name}");
            });

            if (columns.Count == 0)
            {
                throw new SQLiteException("Insert column is a must.");
            }

            return $"INSERT INTO {tableName} ({String.Join(',', columns)}) VALUES ({String.Join(',', @params)})";
        })!;
    }

    public static String GetUpdateSql(Type type)
    {
        return cache.GetOrCreate($"{nameof(GetUpdateSql)}{type.TypeHandle.Value}", cacheEntry =>
        {
            cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);

            var tableName = GetTableName(type);

            var updateSql = new StringBuilder();
            var whereSql = new StringBuilder();

            GetTableProperies(type).ForEach(p =>
            {
                var column = GetColumnName(p, type);

                if (IsUpdateColumn(p, type))
                {
                    updateSql.Append($"{column} = @{p.Name},");
                }

                if (IsUniqueColumn(p, type))
                {
                    whereSql.Append($"{column} = @{p.Name} AND ");
                }
            });

            if (updateSql.Length == 0)
            {
                throw new SQLiteException("Update column is a must.");
            }
            if (whereSql.Length == 0)
            {
                throw new SQLiteException("Unique column is a must.");
            }

            return $"UPDATE {tableName} SET {updateSql.ToString().TrimEnd(',')} WHERE {whereSql.Remove(whereSql.Length - 5, 5)}";
        })!;
    }

    public static String GetUpsertSql(Type type)
    {
        return cache.GetOrCreate($"{nameof(GetUpsertSql)}{type.TypeHandle.Value}", cacheEntry =>
        {
            cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);

            var tableName = GetTableName(type);

            var insertColumns = new List<String>();
            var insertParams = new List<String>();
            var uniques = new List<String>();
            var updateSql = new StringBuilder();

            GetTableProperies(type).ForEach(p =>
            {
                var column = GetColumnName(p, type);

                insertColumns.Add(column);
                insertParams.Add($"@{p.Name}");

                if (IsUniqueColumn(p, type))
                {
                    uniques.Add(column);
                }

                if (IsUpdateColumn(p, type))
                {
                    updateSql.Append($"{column} = @{p.Name},");
                }
            });

            if (insertParams.Count == 0)
            {
                throw new SQLiteException("Insert column is a must.");
            }
            if (uniques.Count == 0)
            {
                throw new SQLiteException("Unique column is a must.");
            }

            return $"INSERT INTO {tableName} ({String.Join(',', insertColumns)}) VALUES ({String.Join(',', insertParams)}) " +
            $"ON CONFLICT({String.Join(',', uniques)}) " +
            $"DO UPDATE SET {updateSql.ToString().TrimEnd(',')}";
        })!;
    }

    public static String GetDeleteSql(Type type)
    {
        return cache.GetOrCreate($"{nameof(GetDeleteSql)}{type.TypeHandle.Value}", cacheEntry =>
        {
            cacheEntry.SlidingExpiration = TimeSpan.FromHours(1);

            var tableName = GetTableName(type);

            var whereSql = new StringBuilder();

            GetTableProperies(type).ForEach(p =>
            {
                var column = GetColumnName(p, type);

                if (IsUniqueColumn(p, type))
                {
                    whereSql.Append($"{column} = @{p.Name} AND ");
                }
            });

            if (whereSql.Length == 0)
            {
                throw new SQLiteException("Unique column is a must.");
            }

            return $"DELETE FROM {tableName} WHERE {whereSql.Remove(whereSql.Length - 5, 5)}";
        })!;
    }

    public static String GetTableName(Type type) =>
        cache.GetOrCreate($"{nameof(GetTableName)}{type.TypeHandle.Value}", cacheEntry => type.GetCustomAttribute<TableAttribute>()?.TableName ?? type.Name)!;

    private static PropertyInfo[] GetTableProperies(Type type) =>
        cache.GetOrCreate($"{nameof(GetTableProperies)}{type.TypeHandle.Value}", cacheEntry => type.GetProperties().Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null).ToArray())!;

    private static String GetColumnName(PropertyInfo prop, Type type) =>
        cache.GetOrCreate($"{nameof(GetColumnName)}{type.TypeHandle.Value}{prop.Name}", cacheEntry => prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name)!;

    private static Boolean IsUniqueColumn(PropertyInfo prop, Type type) =>
        cache.GetOrCreate($"{nameof(IsUniqueColumn)}{type.TypeHandle.Value}{prop.Name}", cacheEntry => prop.GetCustomAttribute<UniqueColumnAttribute>() != null);

    private static Boolean IsUpdateColumn(PropertyInfo prop, Type type) =>
        cache.GetOrCreate($"{nameof(IsUpdateColumn)}{type.TypeHandle.Value}{prop.Name}", cacheEntry => prop.GetCustomAttribute<UpdateColumnAttribute>() != null);
}