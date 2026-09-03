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
using AvePoint.RA.DB.Model.Discovery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery
{
    public class RMDiscoveryColumnInfo
    {
        public string Name { get; set; }

        public string TypeName { get; set; }

        public bool IsKey { get; set; }

        public bool NeedAutoIncremental { get; set; }

        public bool NeedIndex { get; set; }

        public string MaxLength { get; set; } = "";

        public bool HasDefaultValue { get; set; }

        public object DefaultValue { get; set; }

        public override bool Equals(object obj)
        {
            if(obj is not RMDiscoveryColumnInfo columnInfo) return false;
            return columnInfo.Name.Equals(Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    internal class RMDiscoveryDBTableManager
    {

        private static readonly Dictionary<string, RMDiscoveryTableInfo> s_tableColumns = new();

        static RMDiscoveryDBTableManager()
        {
            var tableType = typeof(RMDiscoveryDBTable);
            var assembly = Assembly.GetAssembly(tableType);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface || type.BaseType?.Name != tableType.Name) continue;
                var tableInfo = new RMDiscoveryTableInfo();
                var tableAttr = type.GetCustomAttribute<TableAttribute>();
                tableInfo.Name = tableAttr.Name;

                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    if(property.GetAttribute<NotMappedAttribute>() != null)
                    {
                        continue;
                    }
                    var columnInfo = new RMDiscoveryColumnInfo();
                    var keyAttr = property.GetCustomAttribute<KeyAttribute>();
                    columnInfo.IsKey = keyAttr != null;
                    columnInfo.NeedIndex = property.GetCustomAttribute<IndexAttribute>() != null;

                    var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
                    if(defaultValueAttr != null)
                    {
                        columnInfo.HasDefaultValue = true;
                        columnInfo.DefaultValue = defaultValueAttr.Value;
                    }

                    var generatedAttr = property.GetCustomAttribute<DatabaseGeneratedAttribute>();
                    columnInfo.NeedAutoIncremental = generatedAttr != null && generatedAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;

                    var typeName = property.GetCustomAttribute<ColumnAttribute>();
                    columnInfo.TypeName = typeName.TypeName;

                    columnInfo.Name = property.Name;

                    if(typeName.TypeName.Equals("nvarchar", StringComparison.OrdinalIgnoreCase))
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

        public static RMDiscoveryTableInfo Get(Type type)
        {
            return s_tableColumns[type.Name];
        }
    }
}
