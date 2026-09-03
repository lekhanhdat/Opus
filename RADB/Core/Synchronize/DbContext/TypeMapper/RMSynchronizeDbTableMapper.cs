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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Synchronize;

namespace AvePoint.RA.DB.Core.Synchronize.DbContext.TypeMapper;

public class RMSynchronizeDbTableMapper
{
    private static readonly Dictionary<string, RMSynchronizeTableInfo> s_tableColumns = new();

    static RMSynchronizeDbTableMapper()
    {
        var tableType = typeof(IRMSynchronizeDbTable);
        var assembly = Assembly.GetAssembly(tableType);
        var synchronizeTables = assembly.GetTypes()
            .Where(x => !x.IsAbstract && x.IsClass
                                      && tableType.IsAssignableFrom(x));
        foreach (var type in synchronizeTables)
        {
            var tableInfo = new RMSynchronizeTableInfo();
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            tableInfo.Name = tableAttr is null ? type.Name : tableAttr.Name;

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (property.GetAttribute<NotMappedAttribute>() != null)
                {
                    continue;
                }

                var columnInfo = new RMDiscoveryColumnInfo();
                var keyAttr = property.GetCustomAttribute<KeyAttribute>();
                columnInfo.IsKey = keyAttr != null;
                columnInfo.NeedIndex = property.GetCustomAttribute<IndexAttribute>() != null;

                var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValueAttr != null)
                {
                    columnInfo.HasDefaultValue = true;
                    columnInfo.DefaultValue = defaultValueAttr.Value;
                }

                var generatedAttr = property.GetCustomAttribute<DatabaseGeneratedAttribute>();
                columnInfo.NeedAutoIncremental = generatedAttr != null &&
                                                 generatedAttr.DatabaseGeneratedOption ==
                                                 DatabaseGeneratedOption.Identity;

                var typeName = property.GetCustomAttribute<ColumnAttribute>();
                columnInfo.TypeName = typeName.TypeName.Equals("nvarchar(max)", StringComparison.OrdinalIgnoreCase) ? "Text" : typeName.TypeName;

                columnInfo.Name = property.Name;

                if (typeName.TypeName.Equals("nvarchar", StringComparison.OrdinalIgnoreCase))
                {
                    var maxLengthAttr = property.GetCustomAttribute<MaxLengthAttribute>();

                    if (maxLengthAttr != null)
                    {
                        columnInfo.MaxLength = $"({maxLengthAttr.Length})";
                    }
                    else
                    {
                        columnInfo.MaxLength = $"(MAX)";
                    }
                }

                tableInfo.Columns.Add(columnInfo);
            }

            s_tableColumns.Add(type.Name, tableInfo);
        }
    }

    public static RMSynchronizeTableInfo Get(Type type)
    {
        return s_tableColumns[type.Name];
    }
}

public class RMSynchronizeTableInfo
{
    public string Name { get; set; }

    public List<RMDiscoveryColumnInfo> Columns { get; set; } = [];
}

public class RMSynchronizeTableDataFieldInfo
{
    public Type FieldType { get; private set; }

    public string FieldName { get; private set; }

    public object FieldValue { get; private set; }

    internal RMSynchronizeTableDataFieldInfo(Type fieldType, string fieldName, object fieldValue)
    {
        FieldType = fieldType;
        FieldName = fieldName;
        FieldValue = fieldValue;
    }
}

public class RMSynchronizeDataCollection
{
    public List<Dictionary<string, RMSynchronizeTableDataFieldInfo>> DataList { get; private set; }

    internal RMSynchronizeDataCollection(List<Dictionary<string, RMSynchronizeTableDataFieldInfo>> dataList)
    {
        DataList = dataList;
    }

    public List<Dictionary<string, object>> ToDictionary()
    {
        return DataList.Select(
            data => 
            data.ToDictionary(item => 
                item.Key, item => item.Value.FieldValue)
            ).ToList();
    }

    public List<T> ToList<T>()
    {
        var res = new List<T>();
        var type = typeof(T);
        if (type.IsInterface || type.IsAbstract)
        {
            throw new NotSupportedException(typeof(T).ToString());
        }

        foreach (var data in DataList)
        {
            if (type.IsClass)
            {
                var properties = type.GetProperties();
                var obj = ConvertToClassObject<T>(properties, data);
                res.Add(obj);
            }
            else
            {
                var obj = ConvertToBuildInType<T>(data);
                res.Add(obj);
            }
        }

        return res;
    }

    public List<T> ToTableList<T>()
    {
        var res = new List<T>();
        var type = typeof(T);
        if (type.IsInterface || type.IsAbstract)
        {
            throw new NotSupportedException(typeof(T).ToString());
        }

        foreach (var data in DataList)
        {
            var properties = type.GetProperties();
            var obj = Activator.CreateInstance<T>();
            foreach (var (fieldName, fieldValue) in data)
            {
                var property = properties.FirstOrDefault(item => item.Name == fieldName);
                if (property != null)
                {
                    property.SetValue(obj, Convert.ChangeType(fieldValue.FieldValue, fieldValue.FieldType));
                }
            }

            res.Add(obj);
        }

        return res;
    }

    private static T ConvertToClassObject<T>(PropertyInfo[] properties,
        Dictionary<string, RMSynchronizeTableDataFieldInfo> data)
    {
        var obj = Activator.CreateInstance<T>();
        foreach (var property in properties)
        {
            var name = property.Name;
            if (data.TryGetValue(name, out var fieldInfo))
            {
                property.SetValue(obj, Convert.ChangeType(fieldInfo.FieldValue, fieldInfo.FieldType));
            }
        }

        return obj;
    }

    private static T ConvertToBuildInType<T>(Dictionary<string, RMSynchronizeTableDataFieldInfo> data)
    {
        var fieldInfo = data.First().Value;
        var value = Convert.ChangeType(fieldInfo.FieldValue, fieldInfo.FieldType);
        return (T)value;
    }
}