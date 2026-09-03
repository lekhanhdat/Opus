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




namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    #endregion

    /// <summary>
    /// Provide a high level of using the Ado.Net Model, you can use this class
    /// to connect the any database if you have a data base provider
    /// </summary>
    public sealed class DbHelper : IDisposable, ISingleton
    {
        #region [====Private Field====]
        Boolean idDisposed;
        static readonly Object syncRoot = new Object();
        #endregion

        #region [=====Provider Related Object======]

        /// <summary>
        /// Used to syn the call
        /// </summary>
        public Object SyncRoot { get { return syncRoot; } }

        /// <summary>
        /// get current connection's db provider name
        /// </summary>
        public String ProviderName { get; private set; }

        /// <summary>
        /// get current connection's connection string
        /// </summary>
        public String ConnectionString
        {
            get
            {
                if (this.Connection == null)
                    return "";
                else return this.Connection.ConnectionString ?? "";
            }
            set
            {
                //if value is null, we should replace it with string.Empty
                string finalConnString = string.Empty;
                if (value != null)
                {
                    finalConnString = value;
                }
                //HACK: this may contains some error.
                if (this.Connection != null)
                {
                    if (this.Connection.State == ConnectionState.Open)
                        this.Connection.Close();
                    this.Connection.ConnectionString = finalConnString;
                }
                else
                {
                    this.Connection = ProviderFactory.CreateConnection();
                    this.Connection.ConnectionString = finalConnString;
                }
            }
        }

        /// <summary>
        /// get current connection's db provider factory object 
        /// </summary>
        public DbProviderFactory ProviderFactory { get; private set; }

        /// <summary>
        /// get current connection's DbConnection object
        /// </summary>
        public DbConnection Connection { get; private set; }

        /// <summary>
        /// get a DbCommand object and wrap it with special sql text and command
        /// </summary>
        public DbCommand Command { get { return this.Connection.CreateCommand(); } }

        /// <summary>
        /// get a DataAdapter object that used as non connection query
        /// </summary>
        public DbDataAdapter DataAdapter { get { return this.ProviderFactory.CreateDataAdapter(); } }

        /// <summary>
        /// get a DbParameter object the will be used as parameterlized query's parameters
        /// </summary>
        public DbParameter Parameter { get { return this.ProviderFactory.CreateParameter(); } }

        /// <summary>
        /// get a DbTransaction object which will be start query as a transaction
        /// </summary>
        public DbTransaction Transaction { get { return this.Connection.BeginTransaction(); } }

        #endregion

        #region [=====Constructor======]

        /// <summary>
        ///  Using private constructor that will be used as a global singleton dbhelper class instance
        /// </summary>
        private DbHelper()
        {
            InitializeDbHelper(null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        private DbHelper(String connectionString)
        {
            InitializeDbHelper(connectionString);
        }
        #endregion

        #region [=====Handle the connection state=====]
        /// <summary>
        /// Open the connection if the connection is closed
        /// </summary>
        public void OpenConnection()
        {
            if (this.Connection.State == ConnectionState.Closed)
            {
                this.Connection.Open();
            }
        }

        /// <summary>
        /// Close the connection if the connection is opened
        /// </summary>
        public void CloseConnection()
        {
            if (this.Connection.State == ConnectionState.Open)
                this.Connection.Close();
        }
        #endregion

        #region [=====Connection Based database operation

        #region [=====ExecuteReader=====]

        /// <summary>
        /// Execute reader using a specific dbcommand
        /// </summary>
        /// <param name="command">a dbcommand object used </param>
        /// <returns>the result dbdatareader object </returns>
        public DbDataReader ExecuteReader(DbCommand command)
        {
            this.BindingCommandToConnection(command);
            this.OpenConnection();
            return command.ExecuteReader();
        }

        /*/// <summary>
        /// using a sql command text to get the datareader object
        /// </summary>
        /// <param name="commandText">sql command text,default use commandtype text</param>
        /// <returns>the result dbdatareader object</returns>
        public DbDataReader ExecuteReader(String commandText)
        {
            return this.ExecuteReader(commandText, null);
        }*/

        /*/// <summary>
        /// using a sql command text to get the datareader object
        /// </summary>
        /// <param name="commandText">sql command text,default use commandtype text</param>
        /// <param name="parameters">the parameters will be used</param>
        /// <returns>the result dbdatareader object</returns>
        public DbDataReader ExecuteReader(String commandText, DbParameter[] parameters)
        {
            return this.ExecuteReader(commandText, CommandType.Text, parameters);
        }*/

        /*/// <summary>
        /// using a sql command text to get the datareader object
        /// </summary>
        /// <param name="commandText">sql command text,default use commandtype text</param>
        /// <param name="commandType">indicated which commandtype will be used</param>
        /// <param name="parameters">the parameters will be used in the query</param>
        /// <returns>he result dbdatareader object</returns>
        public DbDataReader ExecuteReader(String commandText, CommandType commandType, DbParameter[] parameters)
        {
            using (var command = this.BuildCommand(commandText, commandType, parameters))
            {
                this.OpenConnection();
                return command.ExecuteReader();
            }
        }*/

        #endregion

        #region [=====ExecuteScalar=====]

        /// <summary>
        /// Execute scalar using a specific command
        /// </summary>
        /// <param name="command">the command used to execute </param>
        /// <returns>the return result</returns>
        public Object ExecuteScalar(DbCommand command)
        {
            this.BindingCommandToConnection(command);
            this.OpenConnection();
            return command.ExecuteScalar();
        }

        /*/// <summary>
        /// using sql command text and specific command type to execute scalar
        /// </summary>
        /// <param name="commandText">sql command text using default command type text</param>
        /// <returns>result object</returns>
        public Object ExecuteScalar(String commandText)
        {
            return this.ExecuteScalar(commandText, null);
        }*/
        /*/// <summary>
        /// using sql command text and specific command type to execute scalar
        /// </summary>
        /// <param name="commandText">sql command text using default command type text</param>
        /// <param name="parameters">parameters will be pass to the query</param>
        /// <returns>result object</returns>
        public Object ExecuteScalar(String commandText, DbParameter[] parameters)
        {
            return this.ExecuteScalar(commandText, CommandType.Text, parameters);
        }*/

        /*/// <summary>
        /// using sql command text and specific command type to execute scalar
        /// </summary>
        /// <param name="commandText">sql command text </param>
        /// <param name="commandType">sql command type</param>
        /// <param name="parameters">parameters will be pass to the query</param>
        /// <returns>result object</returns>
        public Object ExecuteScalar(String commandText, CommandType commandType, DbParameter[] parameters)
        {
            using (var command = this.BuildCommand(commandText, commandType, parameters))
            {
                this.OpenConnection();
                return command.ExecuteScalar();
            }
        }*/

        /// <summary>
        /// generic execute scalar that change the return type
        /// </summary>
        /// <typeparam name="T">Which return type is</typeparam>
        /// <param name="command">the command used to execute</param>
        /// <returns>return value will strong</returns>
        public T ExecuteScalar<T>(DbCommand command)
        {
            return (T)Convert.ChangeType(this.ExecuteScalar(command), typeof(T));
        }

        /*/// <summary>
        /// using sql command text and specific command type to execute scalar
        /// </summary>
        /// <typeparam name="T">Which return type is</typeparam>
        /// <param name="commandText">sql command text using default command type text</param>
        /// <returns>result object</returns>
        public T ExecuteScalar<T>(String commandText)
        {
            return (T)this.ExecuteScalar(commandText);
        }*/
        /*/// <summary>
        /// using sql command text and specific command type or to execute scalar
        /// </summary>
        /// <typeparam name="T">Which return type is</typeparam>
        /// <param name="commandText">sql command text using default command type text</param>
        /// <param name="parameters">parameters will be pass to the query</param>
        /// <returns>result object</returns>
        public T ExecuteScalar<T>(String commandText, DbParameter[] parameters)
            where T : IConvertible
        {
            return (T)Convert.ChangeType(this.ExecuteScalar(commandText, parameters), typeof(T));
        }*/

        /*/// <summary>
        /// using sql command text and specific command type to execute scalar
        /// </summary>
        /// <typeparam name="T">Which return type is</typeparam>
        /// <param name="commandText">sql command text </param>
        /// <param name="commandType">sql command type</param>
        /// <param name="parameters">parameters will be pass to the query</param>
        /// <returns>result object</returns>
        public T ExecuteScalar<T>(String commandText, CommandType commandType, DbParameter[] parameters)
        {
            return (T)this.ExecuteScalar(commandText, commandType, parameters);
        }*/
        #endregion

        #region [=====ExecuteNonQuery=====]

        #region [====Non DbTransaction ExecuteNonQuery====]

        /// <summary>
        /// Execute non query using specific command
        /// </summary>
        /// <param name="command">a command object</param>
        public void ExecuteNonQuery(DbCommand command)
        {
            this.BindingCommandToConnection(command);
            this.OpenConnection();
            command.ExecuteNonQuery();
        }

        /*/// <summary>
        /// using sql text to execute nonquery
        /// </summary>
        /// <param name="commandText">sql command text</param>
        public void ExecuteNonQuery(String commandText)
        {
            this.ExecuteNonQuery(commandText, null);
        }*/
        /*/// <summary>
        /// using sql text to execute nonquery
        /// </summary>
        /// <param name="commandText">sql command text</param>
        /// <param name="parameters">parameters</param>
        public void ExecuteNonQuery(String commandText, DbParameter[] parameters)
        {
            this.ExecuteNonQuery(commandText, CommandType.Text, parameters);
        }*/

        /*/// <summary>
        /// using sql text to execute nonquery
        /// </summary>
        /// <param name="commandText">sql command text</param>
        /// <param name="commandType">command type</param>
        /// <param name="parameters">parameters</param>
        public void ExecuteNonQuery(String commandText, CommandType commandType, DbParameter[] parameters)
        {
            using (var command = this.BuildCommand(commandText, commandType, parameters))
            {
                this.OpenConnection();
                command.ExecuteNonQuery();
            }
        }*/

        #endregion

        #region [====DbTransaction ExecuteNonQuery====]

        /// <summary>
        /// using dbtransaction to execute non query
        /// </summary>
        /// <param name="transaction">a begin transcation</param>
        /// <param name="command">a specific command used to execute non query</param>
        public void ExecuteNonQuery(DbTransaction transaction, DbCommand command)
        {
            this.BindingCommandToConnection(command);
            command.Transaction = transaction;
            this.OpenConnection();
            command.ExecuteNonQuery();
        }


        /*/// <summary>
        /// using dbtransaction to execute non query
        /// </summary>
        /// <param name="transaction">a begin transcation</param>
        /// <param name="commandText">sql command text used to execute non query</param>
        public void ExecuteNonQuery(DbTransaction transaction, String commandText)
        {
            this.ExecuteNonQuery(transaction, commandText, null);
        }*/
        /*/// <summary>
        /// using dbtransaction to execute non query
        /// </summary>
        /// <param name="transaction">a begin transcation</param>
        /// <param name="commandText">sql command text used to execute non query</param>
        /// <param name="parameters">execute non query parameters</param>
        public void ExecuteNonQuery(DbTransaction transaction, String commandText, DbParameter[] parameters)
        {
            this.ExecuteNonQuery(transaction, commandText, CommandType.Text, parameters);
        }*/

        /*/// <summary>
        /// using dbtransaction to execute non query
        /// </summary>
        /// <param name="transaction">a begin transcation</param>
        /// <param name="commandText">sql command text used to execute non query</param>
        /// <param name="commandType">command type </param>
        /// <param name="parameters">xecute non query parameters</param>
        public void ExecuteNonQuery(DbTransaction transaction, String commandText, CommandType commandType, DbParameter[] parameters)
        {
            using (var command = BuildCommand(commandText, commandType, parameters))
            {
                command.Transaction = transaction;
                this.OpenConnection();
                command.ExecuteNonQuery();
            }
        }*/
        #endregion

        #endregion

        #endregion

        #region [=====Non Connection based database operation=====]

        #region [=====Fill the DataTable=====]

        /// <summary>
        /// Fill datatable with specific adapter and select command
        /// </summary>
        /// <param name="table">datatable will be filled by adapter</param>
        /// <param name="adapter">a data adapter object fill the data table</param>
        public void FillDataTable(DataTable table, DbDataAdapter adapter)
        {
            this.BindingCommandToConnection(adapter.SelectCommand);
            adapter.Fill(table);
        }

        /*/// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="commandText"></param>
        public void FillDataTable(DataTable table, String commandText)
        {
            this.FillDataTable(table, commandText, null);
        }*/

        /*/// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        public void FillDataTable(DataTable table, String commandText, DbParameter[] parameters)
        {
            this.FillDataTable(table, commandText, CommandType.Text, parameters);
        }*/

        /*/// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        public void FillDataTable(DataTable table, String commandText, CommandType commandType, DbParameter[] parameters)
        {
            using (var selectCommand = this.BuildCommand(commandText, commandType, parameters))
            {
                this.FillDataTable(table, selectCommand);
            }
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="selectCommand"></param>
        public void FillDataTable(DataTable table, DbCommand selectCommand)
        {
            var adapter = this.BuildDataAdapter(selectCommand, null, null, null, null);
            adapter.Fill(table);
        }

        #endregion

        #region [=====Write back the datatable =====]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="adapter"></param>
        public void UpdateDataTable(DataTable table, DbDataAdapter adapter)
        {
            BindingDataAdapterCommandsToConnection(adapter);
            adapter.Update(table);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="selectCommand"></param>
        /// <param name="insertCommand"></param>
        /// <param name="updateCommand"></param>
        /// <param name="deleteCommand"></param>
        /// <param name="mapping"></param>
        public void UpdateDataTable(DataTable table, DbCommand selectCommand,
            DbCommand insertCommand,
            DbCommand updateCommand,
            DbCommand deleteCommand,
            DataTableMapping mapping)
        {
            DbDataAdapter adapter = BuildDataAdapter(selectCommand, insertCommand,
                                                     updateCommand, deleteCommand, mapping);
            adapter.Update(table);
        }

        #endregion

        #endregion

        #region [=====Private method=====]

        void InitializeDbHelper(String connectionString)
        {
            this.InitProviderAndConnectionString(connectionString);
            this.ValidateProviderName(this.ProviderName);

            this.ProviderFactory = DbProviderFactories.GetFactory(this.ProviderName);
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// init provider and connection string
        /// </summary>
        private void InitProviderAndConnectionString(String connectionString)
        {
            this.ProviderName = "System.Data.SQLite";
        }

        /// <summary>
        /// check if the specific provider is existed or not
        /// </summary>
        /// <param name="providerName"></param>
        private void ValidateProviderName(String providerName)
        {
            var findResult = default(DataRow);
            foreach (DataRow item in DbProviderFactories.GetFactoryClasses().Rows)
            {
                if (item["InvariantName"].ToString().Equals(providerName, StringComparison.OrdinalIgnoreCase))
                {
                    findResult = item;
                    break;
                }
            }
            if (findResult == null)
                throw new ArgumentException(String.Format("Provider name {0} is not existed", providerName));
        }
        #region [=====DbCommand Wrapper and associate DbConnection Wrapper=====]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        private void BindingCommandToConnection(DbCommand command)
        {
            command.Connection = this.Connection;
        }

        /*/// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private DbCommand BuildCommand(String commandText, CommandType commandType, DbParameter[] parameters)
        {
            var result = this.ProviderFactory.CreateCommand();
            result.Connection = this.Connection;
            result.CommandType = commandType;
            result.CommandText = commandText;
            if (parameters != null && parameters.Length != 0)
                result.Parameters.AddRange(parameters);
            return result;
        }*/
        #endregion

        #region [=====Binding the dataAdapter command and build the dbdataadapter=====]

        /// <summary>
        /// building adapter 
        /// </summary>
        /// <param name="selectCommand">select command object</param>
        /// <param name="insertCommand">insert command object</param>
        /// <param name="updateCommand">update command object</param>
        /// <param name="deleteCommand">delete command object</param>
        /// <param name="mapping">data table mapping object </param>
        /// <returns>a data adapter object</returns>
        private DbDataAdapter BuildDataAdapter(
            DbCommand selectCommand,
            DbCommand insertCommand,
            DbCommand updateCommand,
            DbCommand deleteCommand,
            DataTableMapping mapping)
        {
            var result = DataAdapter;
            if (selectCommand != null)
            {
                this.BindingCommandToConnection(selectCommand);
                result.SelectCommand = selectCommand;
            }
            if (insertCommand != null)
            {
                this.BindingCommandToConnection(insertCommand);
                result.InsertCommand = insertCommand;
            }
            if (updateCommand != null)
            {
                this.BindingCommandToConnection(updateCommand);
                result.UpdateCommand = updateCommand;
            }
            if (deleteCommand != null)
            {
                this.BindingCommandToConnection(deleteCommand);
                result.DeleteCommand = deleteCommand;
            }
            if (mapping != null)
                result.TableMappings.Add(mapping);
            return result;
        }

        /// <summary>
        /// Binding dataadapter with commands
        /// </summary>
        /// <param name="adapter">a dataadapter object </param>
        private void BindingDataAdapterCommandsToConnection(DbDataAdapter adapter)
        {
            if (adapter.SelectCommand != null)
            {
                this.BindingCommandToConnection(adapter.SelectCommand);
            }
            if (adapter.InsertCommand != null)
            {
                this.BindingCommandToConnection(adapter.InsertCommand);
            }
            if (adapter.UpdateCommand != null)
            {
                this.BindingCommandToConnection(adapter.UpdateCommand);
            }
            if (adapter.DeleteCommand != null)
            {
                this.BindingCommandToConnection(adapter.DeleteCommand);
            }
        }

        #endregion
        #endregion

        #region [====IDisposable====]

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// finalize method
        /// </summary>
        ~DbHelper()
        {
            Dispose(false);
        }

        /// <summary>
        /// a delegate dispose method handle manual invoke or inner invoke
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                this.ConnectionString = null;
                this.ProviderFactory = null;
                this.ProviderName = null;
            }
            if (!idDisposed)
            {
                idDisposed = true;
                if (this.Connection != null)
                    this.Connection.Dispose();
            }
        }
        #endregion
    }
}