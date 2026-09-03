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
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Model.Discovery;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office.Word;
using Newtonsoft.Json;
using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.Context
{
    public class RMDiscoveryDBContext : IAsyncDisposable
    {
        private readonly SqlConnection _connection;

        public int Timeout { get; set; } = 60 * 10; //10min

        internal RMDiscoveryDBContext(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<RMDiscoveryDataCollection> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            sql = sql.Replace(" Rule ", " [Rule] ");
            using var command = GetSqlCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            await using var reader = await command.ExecuteReaderAsync();
            var dataList = new List<Dictionary<string, RMDiscoveryTableDataFieldInfo>>();
            while (await reader.ReadAsync())
            {
                var fieldCount = reader.FieldCount;
                var row = new Dictionary<string, RMDiscoveryTableDataFieldInfo>(fieldCount);
                for (var i = 0; i < fieldCount; i++)
                {
                    var fieldType = reader.GetFieldType(i);
                    var fieldName = reader.GetName(i);
                    var fieldValue = reader.GetValue(i);
                    row.Add(fieldName, new(fieldType, fieldName, fieldValue));
                }
                dataList.Add(row);
            }

            return new(dataList);
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            sql = sql.Replace(" Rule ", " [Rule] ");
            using var command = GetSqlCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            using var command = GetSqlCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            var res = await command.ExecuteScalarAsync();
            if (res == null)
            {
                return default;
            }
            return (T)res;
        }

        public Task<int> ExecuteInsertAsync<T>(T item) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(new List<T> { item }, "dbo");
        }

        public Task<int> ExecuteInsertAsync<T>(List<T> items) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, "dbo");
        }

        public Task<int> ExecuteInsertAsync<T>(List<T> items, Guid o365TenantId) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId));
        }

        public Task<int> ExecuteAOSPInsertAsync<T>(List<T> items, Guid o365TenantId) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId));
        }

        public Task<int> ExecuteFSInsertAsync<T>(List<T> items) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, RMDiscoveryDBManager.GetFileSystemSchemaName());
        }

        public Task<int> ExecuteInsertAsync<T>(List<T> items, Guid o365TenantId, Guid profileId) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId, profileId));
        }
        
        public Task<int> ExecuteInsertAsync<T>(List<T> items, string googleOrganizationId, Guid profileId) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId, profileId));
        }

        public Task<int> ExecuteAOSPInsertAsync<T>(List<T> items, Guid o365TenantId, Guid profileId) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, RMDiscoveryDBManager.GetAOSPSchemaName(o365TenantId, profileId));
        }

        public Task<int> ExecuteInsertAsync<T>(T item, string schema) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(new List<T> { item }, schema);
        }

        public async Task<int> ExecuteInsertAsync<T>(List<T> items, string schema) where T : RMDiscoveryDBTable
        {

            if (items == null || !items.Any())
            {
                return 0;
            }

            var type = typeof(T);
            var tableInfo = RMDiscoveryDBTableManager.Get(type);
            var dataTable = new DataTable();
            var autoIncrementalKeyColumn = tableInfo.Columns.FirstOrDefault(item => item.IsKey && item.NeedAutoIncremental);

            var properties = type.GetProperties().Where(item => item.GetAttribute<NotMappedAttribute>() == null).ToList();

            foreach (var property in properties)
            {
                dataTable.Columns.Add(property.Name, property.PropertyType);
            }

            foreach (var customColumn in items.First().CustomColumns)
            {
                dataTable.Columns.Add(customColumn.Name, customColumn.ValueType);
            }

            foreach (var item in items)
            {
                var row = dataTable.NewRow();
                foreach (var property in properties)
                {
                    if (autoIncrementalKeyColumn != null && autoIncrementalKeyColumn.Name == property.Name)
                    {
                        continue;
                    }
                    row[property.Name] = property.GetValue(item);
                }

                foreach (var customColumn in item.CustomColumns)
                {
                    row[customColumn.Name] = Convert.ChangeType(customColumn.Value, customColumn.ValueType);
                }

                dataTable.Rows.Add(row);
            }

            using var bulkCopy = new SqlBulkCopy(_connection);
            bulkCopy.DestinationTableName = $"[{schema}].[{tableInfo.Name}]";
            bulkCopy.BulkCopyTimeout = Timeout;
            bulkCopy.BatchSize = 1000;

            foreach (DataColumn col in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            }

            try
            {
                await bulkCopy.WriteToServerAsync(dataTable);
            }
            catch (Exception e)
            {
                throw;
            }

            return items.Count;
        }

        private SqlCommand GetSqlCommand()
        {
            var command = _connection.CreateCommand();
            command.CommandTimeout = Timeout;
            return command;
        }

        public ValueTask DisposeAsync()
        {
            if (_connection == null)
            {
                return ValueTask.CompletedTask;
            }
            return _connection.DisposeAsync();
        }
    }

    public class RMDiscoveryTableDataFieldInfo
    {
        public Type FieldType { get; private set; }

        public string FieldName { get; private set; }

        public object FieldValue { get; private set; }

        internal RMDiscoveryTableDataFieldInfo(Type fieldType, string fieldName, object fieldValue)
        {
            FieldType = fieldType;
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
    }

    public class RMDiscoveryDataCollection
    {
        public List<Dictionary<string, RMDiscoveryTableDataFieldInfo>> DataList { get; private set; }

        internal RMDiscoveryDataCollection(List<Dictionary<string, RMDiscoveryTableDataFieldInfo>> dataList)
        {
            DataList = dataList;
        }

        public List<Dictionary<string, object>> ToDictionary()
        {
            var res = new List<Dictionary<string, object>>();
            foreach (var data in DataList)
            {
                res.Add(data.ToDictionary(item => item.Key, item => item.Value.FieldValue));
            }

            return res;
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

        public List<T> ToTableList<T>() where T : RMDiscoveryDBTable
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
                foreach (var fieldInfo in data)
                {
                    var fieldName = fieldInfo.Key;
                    var property = properties.FirstOrDefault(item => item.Name == fieldName);
                    if (property != null)
                    {
                        property.SetValue(obj, Convert.ChangeType(fieldInfo.Value.FieldValue, fieldInfo.Value.FieldType));
                    }
                    else
                    {
                        obj.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(fieldName, fieldInfo.Value.FieldValue, fieldInfo.Value.FieldType));
                    }
                }
                res.Add(obj);
            }

            return res;
        }

        private static T ConvertToClassObject<T>(PropertyInfo[] properties, Dictionary<string, RMDiscoveryTableDataFieldInfo> data)
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var property in properties)
            {
                var name = property.Name;
                if (data.ContainsKey(name))
                {
                    var fieldInfo = data[name];
                    property.SetValue(obj, Convert.ChangeType(fieldInfo.FieldValue, fieldInfo.FieldType));
                }
            }
            return obj;
        }

        private static T ConvertToBuildInType<T>(Dictionary<string, RMDiscoveryTableDataFieldInfo> data)
        {
            var fieldInfo = data.First().Value;
            var value = Convert.ChangeType(fieldInfo.FieldValue, fieldInfo.FieldType);
            return (T)value;
        }
    }
}
