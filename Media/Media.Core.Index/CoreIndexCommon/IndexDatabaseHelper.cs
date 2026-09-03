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




namespace AvePoint.Media.Core.Index
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Reflection;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility;
    using Merged18NResources.MediaCoreIndex;
    using AvePoint.Media.Service.DomainModel;
    using System.Collections.Concurrent;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Azure.Amqp.Framing;
    using System.Runtime.InteropServices;
    using Microsoft.InformationProtection.Exceptions;

    #endregion

    public class IndexDatabaseHelper
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static IDictionary<Type, String> cachedInsertSqlCommandTextDictionary = new ConcurrentDictionary<Type, String>();
        static IDictionary<Type, Dictionary<String, String>> cachedIndexSqlColumnNamesDictionary = new ConcurrentDictionary<Type, Dictionary<String, String>>();
        public static bool isNoNeedUploadIndex;
        //DbProviderFactory providerFactory = DbProviderFactories.GetFactory("System.Data.SQLite");
        SQLiteConnection dbConnection;

        String connectionString;
        Guid connectionID;

        String domainName;
        String userName;
        String password;

        public Boolean IsOpen
        {
            get { return this.dbConnection != null && this.dbConnection.State == ConnectionState.Open; }
        }

        public void Open(String connectionString)
        {
            this.Open(connectionString, string.Empty, string.Empty, string.Empty);
        }
        public void Open(String connectionString, String domain, String username, String password)
        {
            this.connectionString = connectionString;
            this.connectionID = Guid.NewGuid();

            this.domainName = domain;
            this.userName = username;
            this.password = password;

            {
                ConnectionLockManager.GetConnectionLock(this.connectionString, this.connectionID, ConnectionLockType.ReadWrite);
                this.dbConnection = new SQLiteConnection(connectionString);
                this.dbConnection.Open();
            }
        }
        public void Open(String connectionString, string seePassWord)
        {
            this.connectionString = connectionString;
            this.connectionID = Guid.NewGuid();

            {

                AppDomain.CurrentDomain.SetData(string.Format("b6aae9db-c854-4fcc-96c3-67c1f404afe4_{0}", Process.GetCurrentProcess().Id), "AvePoint 525 Washington Blvd Ste 1400 Jersey City, NJ 07310");
                SQLiteCommand.Execute("PRAGMA activate_extensions='see-7bb07b8d471d642e';", SQLiteExecuteType.NonQuery, "Data Source=:memory:;");
                System.Environment.SetEnvironmentVariable("Override_SEE_Certificate", "SDS-SEE.exml");

                //                System.Environment.SetEnvironmentVariable("ConfigurationDirectory",
                //                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                //                    "lib\\Harpy1.0\\Configurations"));
                //                System.Environment.SetEnvironmentVariable("LicenseOtherAppDomain", "1");
                //                System.Environment.SetEnvironmentVariable("LicenseAssemblyPath", System.AppDomain.CurrentDomain.BaseDirectory);

                //                System.Environment.SetEnvironmentVariable("StubPath", System.AppDomain.CurrentDomain.BaseDirectory);
                //                //System.Environment.SetEnvironmentVariable("Override_SEE_Certificate", System.AppDomain.CurrentDomain.BaseDirectory + "SDS-SEE.exml");
                //#if DEBUG
                //                System.Environment.SetEnvironmentVariable("ForceEnableTrace", "1");
                //                System.Environment.SetEnvironmentVariable("ForceEnableTraceLogFile", "1");
                //                System.Environment.SetEnvironmentVariable("TracePriorities", "AnyMask");
                //#endif
                ConnectionLockManager.GetConnectionLock(string.Format("Data Source = {0}", this.connectionString), this.connectionID, ConnectionLockType.ReadWrite);
                SQLiteConnectionStringBuilder connStringBuilder = new SQLiteConnectionStringBuilder();
                connStringBuilder.DataSource = connectionString;
                string dbHeader = string.Empty;
                try
                {
                    int i = 0;
                    Exception exception = null;
                    while (i < 3)
                    {
                        try
                        {
                            i++;
                            if (File.Exists(connStringBuilder.DataSource))
                            {
                                using (FileStream file = new FileStream(connStringBuilder.DataSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    byte[] bys = new byte[16];
                                    int bRead = 0;
                                    while (bRead < 16)
                                    {
                                        int rd = file.Read(bys, bRead, 16 - bRead);
                                        if (rd == -1)
                                        {
                                            throw new FileIOException("file is unusually small");
                                        }
                                        bRead += rd;
                                    }
                                    dbHeader = Encoding.ASCII.GetString(bys);
                                }
                                if (dbHeader.Equals("SQLite format 3\0"))
                                {
                                    logger.Info("Start open indexdb,this indexdb is not encryption");
                                    this.dbConnection = new SQLiteConnection(connStringBuilder.ToString());
                                    this.dbConnection.Open();
                                    if (!isNoNeedUploadIndex)
                                    {
                                        this.dbConnection.ChangePassword(seePassWord);
                                        logger.Info("DB has not encryption ,and ChangePassword success.");
                                    }
                                    logger.Info($"Finish open indexdb,isNoNeedUploadIndex:{isNoNeedUploadIndex}");
                                }
                                else
                                {
                                    logger.Info("Start open indexdb,this indexdb is encryption");
                                    connStringBuilder.Password = seePassWord;
                                    this.dbConnection = new SQLiteConnection(connStringBuilder.ToString());
                                    this.dbConnection.Open();
                                    logger.Info("DB with SEE encryption open success.");
                                }
                            }
                            else
                            {
                                this.dbConnection = new SQLiteConnection(connStringBuilder.ToString());
                                this.dbConnection.Open();
                                this.dbConnection.ChangePassword(seePassWord);
                                logger.Info("DB not exist ,create and ChangePassword success.");
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.Error("something went wrong when open DB or check is DB has encrypted,message:{0}", ex.ToString());
                            exception = ex;
                            if (i == 3)
                            {
                                throw;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("something went wrong when open DB or check is DB has encrypted,message:{0}", e.ToString());
                    string ConnectionLockString = connectionString;
                    if (!connectionString.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase))
                    {
                        ConnectionLockString = "Data Source = {0}".FormatWith(this.connectionString);
                    }
                    ConnectionLockManager.RemoveConnectionLock(ConnectionLockString, connectionID);
                    throw;
                }
            }
        }

        public void ChangePassword(string newPassword)
        {
            try
            {
                this.dbConnection.ChangePassword(newPassword);
            }
            catch(Exception e)
            {
                logger.Error($"Fail change password,ex: {e}");
                throw;
            }
        }


        /// <summary>
        /// In order to make sure that AveImpersonator which in using statement never throw exception,
        /// I change the AveImpernator dispose method
        /// </summary>
        public void Close()
        {
            {
                try
                {
                    if (this.dbConnection != null)
                    {
                        this.dbConnection.Close();
                        string ConnectionLockString = connectionString;
                        if (!connectionString.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase))
                        {
                            ConnectionLockString = "Data Source = {0}".FormatWith(this.connectionString);
                        }
                        ConnectionLockManager.RemoveConnectionLock(ConnectionLockString, connectionID);
                        this.dbConnection = null;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(MediaCoreIndexResource.IndexDatabaseHelperCloseException, e.ToString());
                }
            }
        }

        public void ExecuteNonQuery(String commandText, Dictionary<String, Object> parameters)
        {
            ExecuteWithTransaction(commandText, parameters, (command, sqlCommandText, sqlParameters) =>
            {
                command.CommandText = commandText;
                this.BindingParametersToDbCommand(parameters, command);
                command.ExecuteNonQuery();
            });
        }
        public void ExecuteNonQuery(String tableName, DataTable dataTable)
        {
            this.ExecuteWithTransaction(tableName, dataTable, (command, sqlTableName, sqlDataTable) =>
            {
                var param = new Dictionary<String, Object>();
                var columnParam = new StringBuilder();

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    foreach (DataColumn dataColumn in dataTable.Columns)
                    {
                        columnParam.Append("@" + dataColumn.ColumnName + ",");
                        param["@" + dataColumn.ColumnName] = dataRow[dataColumn.ColumnName];
                    }
                    command.CommandText = String.Format("INSERT INTO [{0}] VALUES({1})", tableName, columnParam.ToString().Remove(columnParam.ToString().LastIndexOf(",", StringComparison.OrdinalIgnoreCase)));
                    this.BindingParametersToDbCommand(param, command);
                    command.ExecuteNonQuery();
                    columnParam.Length = 0;
                }
            });
        }
        public void ExecuteNonQuery<TIndex>(List<TIndex> indexList)
            where TIndex : IIndexable
        {
            this.ExecuteWithTransaction(indexList, Missing.Value, (command, objListType, missingType) =>
            {
                indexList.ForEach(index =>
                {
                    var insertCommandText = this.GenerateInsertCommandText(index);
                    var dbParameters = index.GenerateInsertDatabaseParameters();
                    command.CommandText = insertCommandText;
                    this.BindingParametersToDbCommand(dbParameters, command);
                    command.ExecuteNonQuery();
                });
            });
        }

        public DataTable ExecuteReader(String commandText, Dictionary<String, Object> parameters)
        {
            var dataTable = new DataTable();
            //Never remove the following three lines
            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            dataSet.EnforceConstraints = false;

            this.ExecuteWithImpersonator(commandText, parameters, (commandTextString, databaseParameters) =>
            {
                using (var command = this.dbConnection.CreateCommand())
                {
                    command.CommandText = commandText;
                    this.BindingParametersToDbCommand(parameters, command);
                    using (var dataReader = command.ExecuteReader())
                    {
                        dataTable.Load(dataReader);
                    }
                }
            });
            return dataTable;
        }

        public List<TIndex> ExecuteReader<TIndex>(String commandText, Dictionary<String, Object> parameters)
             where TIndex : IIndexable
        {
            var result = new List<TIndex>();
            this.ExecuteWithImpersonator(commandText, parameters, (commandTextString, databaseParameters) =>
            {
                using (var command = this.dbConnection.CreateCommand())
                {
                    command.CommandText = commandText;
                    BindingParametersToDbCommand(parameters, command);
                    using (var dbDataReader = command.ExecuteReader())
                    {
                        while (dbDataReader.Read())
                        { result.Add(ConstructObjectFromDbReader<TIndex>(dbDataReader)); }
                    }
                }
            });
            return result;
        }

        public List<T> ExecuteQueryForAllClass<T>(String commandText, Dictionary<String, Object> parameters)
            where T : class
        {
            var result = new List<T>();
            this.ExecuteWithImpersonator(commandText, parameters, (commandTextString, databaseParameters) =>
            {
                using (var command = this.dbConnection.CreateCommand())
                {
                    command.CommandText = commandText;
                    BindingParametersToDbCommand(parameters, command);
                    using (var dbDataReader = command.ExecuteReader())
                    {
                        while (dbDataReader.Read())
                        { result.Add(ConstructObjectFromDbReaderForClass<T>(dbDataReader)); }
                    }
                }
            });
            return result;
        }
        public Object ExecuteScalar(String commandText, Dictionary<String, Object> parameters = null)
        {
            var result = default(Object);
            this.ExecuteWithImpersonator(commandText, parameters, (commandTextString, databaseParameters) =>
            {
                using (var command = this.dbConnection.CreateCommand())
                {
                    command.CommandText = commandText;
                    BindingParametersToDbCommand(parameters, command);
                    result = command.ExecuteScalar();
                }
            });
            return result;
        }

        public List<T> ExecuteQueryForOneColume<T>(String commandText, Dictionary<String, Object> parameters = null)
            where T : class
        {
            var result = new List<T>();
            this.ExecuteWithImpersonator(commandText, parameters, (commandTextString, databaseParameters) =>
            {
                using (var command = this.dbConnection.CreateCommand())
                {
                    command.CommandText = commandText;
                    BindingParametersToDbCommand(parameters, command);
                    using (var dbDataReader = command.ExecuteReader())
                    {
                        while (dbDataReader.Read())
                        {
                            result.Add(dbDataReader.GetValue(0) as T);
                        }
                    }
                }
            });
            return result;
        }
        public List<Int64> ExecuteQueryForOneColumeInt64(String commandText, Dictionary<String, Object> parameters = null)
        {
            var result = new List<Int64>();
            this.ExecuteWithImpersonator(commandText, parameters, (commandTextString, databaseParameters) =>
            {
                using (var command = this.dbConnection.CreateCommand())
                {
                    command.CommandText = commandText;
                    BindingParametersToDbCommand(parameters, command);
                    using (var dbDataReader = command.ExecuteReader())
                    {
                        while (dbDataReader.Read())
                        {
                            result.Add(dbDataReader.GetInt64(0));
                        }
                    }
                }
            });
            return result;
        }

        void ExecuteWithTransaction<TCommandText, TParameters>(
            TCommandText commandText,
            TParameters parameters,
            Action<DbCommand, TCommandText, TParameters> action)
        {
            this.ExecuteWithImpersonator(commandText, parameters, (commandTextString, databaseParameters) =>
            {
                using (var transaction = this.dbConnection.BeginTransaction())
                {
                    using (var command = this.dbConnection.CreateCommand())
                    {
                        try
                        {
                            command.Transaction = transaction;
                            action(command, commandText, parameters);
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            });
        }

        void ExecuteWithImpersonator<TCommandText, TParameters>(
            TCommandText commandText,
            TParameters parameters,
            Action<TCommandText, TParameters> action)
        {
            using (var sqliteLock = new IndexDatabaseOperationLock(this.connectionString))
            {
                {
                    action(commandText, parameters);
                }
            }
        }

        void BindingParametersToDbCommand(Dictionary<String, Object> parameters, DbCommand command)
        {
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    var par = command.CreateParameter();
                    par.ParameterName = item.Key;
                    par.Value = item.Value ?? DBNull.Value;
                    command.Parameters.Add(par);
                }
            }
        }

        T ConstructObjectFromDbReaderForClass<T>(DbDataReader dataReader)
             where T : class
        {
            var instance = Activator.CreateInstance<T>();
            Array.ForEach<PropertyInfo>(typeof(T).GetProperties(), item =>
            {
                var mappedColumnOrdinal = dataReader.GetOrdinal(item.Name);
                if (mappedColumnOrdinal != -1)
                {
                    var colValue = default(Object);
                    if (!dataReader.IsDBNull(mappedColumnOrdinal))
                        colValue = dataReader.GetValue(mappedColumnOrdinal);
                    if (colValue != null && !colValue.IsTypeInternal(item.PropertyType))
                        colValue = colValue.ChangeToType(item.PropertyType);
                    else if (colValue == null && typeof(ValueType).IsAssignableFrom(item.PropertyType))
                        colValue = Activator.CreateInstance(item.PropertyType);
                    item.FastSetValue(instance, colValue);
                }
            });
            return instance;
        }

        TIndex ConstructObjectFromDbReader<TIndex>(DbDataReader dataReader)
             where TIndex : IIndexable
        {
            var instance = Activator.CreateInstance<TIndex>();
            var propertyColumnNameDic = this.GetMappedPropertyColumnNameMappedDictionary(typeof(TIndex));
            Array.ForEach<PropertyInfo>(typeof(TIndex).GetProperties(), item =>
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
                        if (colValue != null && !colValue.IsTypeInternal(item.PropertyType))
                            colValue = colValue.ChangeToType(item.PropertyType);
                        else if (colValue == null && typeof(ValueType).IsAssignableFrom(item.PropertyType))
                            colValue = Activator.CreateInstance(item.PropertyType);
                        item.FastSetValue(instance, colValue);
                    }
                }
            });
            return instance;
        }

        Dictionary<String, String> GetMappedPropertyColumnNameMappedDictionary(Type indexType)
        {
            if (!cachedIndexSqlColumnNamesDictionary.ContainsKey(indexType))
            {
                var mappedDictionary = new Dictionary<String, String>();
                Array.ForEach<PropertyInfo>(indexType.GetProperties(), item =>
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
                cachedIndexSqlColumnNamesDictionary.AddOrReplaceInternal(indexType, mappedDictionary);
            }
            return cachedIndexSqlColumnNamesDictionary[indexType];
        }

        String GenerateInsertCommandText(Object index)
        {
            var indexType = index.GetType();
            if (!cachedInsertSqlCommandTextDictionary.ContainsKey(indexType))
            {
                var tableName = default(String);
                var mapTableAttributeArray = indexType.GetCustomAttributes(typeof(TableAttribute), false);
                if (mapTableAttributeArray.Length > 0)
                    tableName = ((TableAttribute)mapTableAttributeArray[0]).TableName;
                else throw new Exception(String.Format(MediaCoreIndexResource.IndexDatabaseHelperGenerateInsertCmdExceptionMapTableAttributeNotSpecified, index.GetType().FullName));
                var columns = new StringBuilder();
                var dbParams = new StringBuilder();
                Array.ForEach<PropertyInfo>(indexType.GetProperties(), item =>
                {
                    var itemValue = item.FastGetValue(index);
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
                cachedInsertSqlCommandTextDictionary.AddOrReplaceInternal(indexType, indexTypeInsertCommandText);
            }
            return cachedInsertSqlCommandTextDictionary[indexType];
        }
    }
}