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
using System.Data;
using Microsoft.Data.SqlClient;
using AvePoint.GCommon;
using System.Threading;
using AvePoint.GCommon.Utility.TransientFault;

namespace AvePoint.Common
{
    public class AveSqlConnection : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveSqlConnection));

        static AveRetryPolicy ConnectionRetryPolicy = new AveRetryPolicy<AveTransientErrorCatchAllStrategy>(new FixedIntervalRetryStrategy(5, TimeSpan.FromSeconds(20)));

        public const int DEFAULT_TIMEOUT = 180;
        public const int DEFAULT_COMMAND_TIMEOUT = 300;//300 seconds

        #region --Sql Exception Number--
        public const int SQL_FOREIGN_KEY_VIOLATION = 547;
        public const int SQL_DEAD_LOCK = 1205;
        public const int SQL_UNIQUE_CONSTRAINT_VIOLATION = 2601;
        public const int SQL_UNIQUE_CONSTRAINT_VIOLATION_1 = 2627;
        #endregion

        #region Propertis
        private int mTimeout;
        private string mServer;
        private string mDatabase;
        /// <summary>
        /// only for reset sql connection
        /// </summary>
        private string mConnectionString;
        public string ConnectionString
        {
            get
            {
                return mConnectionString;
            }
        }
        private SqlConnection mConnection;

        public SqlConnection Connection
        {
            get
            {
                return mConnection;
            }
        }

        // We DO NOT expose this field to user. If you need to call some methods from SqlCommand,
        // please add them to this class.
        private SqlCommand mCommand;

        public int Timeout
        {
            get { return mTimeout; }
        }

        public string Server
        {
            get { return mServer; }
        }

        public string Database
        {
            get { return mDatabase; }
        }

        public SqlCommand Command
        {
            get
            {
                if (mConnection == null || mConnection.State == ConnectionState.Broken || mConnection.State == ConnectionState.Closed)
                {
                    ResetConnection(false);
                }
                return mCommand;
            }
            set { mCommand = value; }
        }
        #endregion

        #region Constructor
        public AveSqlConnection()
        {
        }

        public AveSqlConnection(string connString)
        {
            Open(connString);
        }

        public AveSqlConnection(string connString, int timeout)
        {
            Open(connString, timeout);
        }
        #endregion

        #region Open & Close

        /// <summary>
        /// Open connection to sql database.
        /// If you are tring to open mulit connection,
        /// do not close this connection before you open
        /// another connection. You can call Open mulit time,
        /// and this function will test if two connctions are
        /// in same server and database. If this situation happens,
        /// It will reuse previous connction. Otherwise, it will close
        /// previous connection and setup a new connction.
        /// This function is equals Open(connstring, DEFAULT_TIMEOUT)
        /// </summary>
        /// <param name="connString">Connction string</param>
        public void Open(string connString)
        {
            Open(connString, DEFAULT_TIMEOUT);
        }

        public void Open(string connString, int timeout)
        {
            mTimeout = timeout;
            string server = string.Empty;
            string database = string.Empty;
            InitConnectionString(ref connString, ref server, ref database);
            if (mConnection != null)
            {
                if (mConnection.State == System.Data.ConnectionState.Open
                    && server.Equals(mServer, StringComparison.OrdinalIgnoreCase)
                    && database.Equals(mDatabase, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                Dispose();
            }
            mConnection = new SqlConnection(connString);
            mServer = server;
            mDatabase = database;
            mConnectionString = connString;
            ConnectionRetryPolicy.ExecuteAction(() =>
            {
                mConnection.Open();
            });
            mCommand = mConnection.CreateCommand();

            SetCommandTimeout(DEFAULT_COMMAND_TIMEOUT);
        }

        public void SetCommandTimeout(int seconds)
        {
            Command.CommandTimeout = seconds;
        }

        public SqlTransaction BeginTransaction()
        {
            return mConnection.BeginTransaction();
        }

        /// <summary>
        /// only for dynamic to get SqlCmd in case connection is not online
        /// </summary>
        /// <param name="resetCommand">
        /// true: reset command.
        /// false: no need to reset command, command is only use SqlConnection, just reset this will be ok
        /// default value is false.
        /// </param>
        private void ResetConnection(bool resetCommand, bool sleepBeforeOpen=false)
        {
            if (resetCommand || mCommand == null)
            {
                Dispose();
                mConnection = new SqlConnection(mConnectionString);
                mConnection.Open();
                mCommand = mConnection.CreateCommand();
            }
            else
            {
                if (mConnection != null)
                {
                    try
                    {
                        mConnection.Dispose();
                    }
                    catch (Exception e) { mLog.Warn(e.ToString()); }
                    mConnection = new SqlConnection(mConnectionString);
                    if (sleepBeforeOpen)
                    {
                        Thread.Sleep(5000);
                    }
                    mConnection.Open();
                    mCommand.Connection = mConnection;
                }
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (mCommand != null)
            {
                try
                {
                    mCommand.Dispose();
                }
                catch (Exception e) { mLog.Warn(e.ToString()); }
                mCommand = null;
            }
            if (mConnection != null)
            {
                try
                {
                    mConnection.Dispose();
                }
                catch (Exception e) { mLog.Warn(e.ToString()); }
                mConnection = null;
            }
        }
        #endregion

        #region Methods from SqlCommand
        public void ClearParameters()
        {
            mCommand.Parameters.Clear();
        }

        /// <summary>
        /// 增加返回值，为了更好的切换值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlParameter AddParameter(string key, object value)
        {
            SqlParameter currentParameter = null;
            if (mCommand.Parameters.Contains(key))
            {
                currentParameter = mCommand.Parameters[key];
                currentParameter.Value = value;
            }
            else
            {
                currentParameter = mCommand.Parameters.AddWithValue(key, value);
            }

            return currentParameter;
        }

        /// <summary>
        /// 增加返回值，为了更好的切换值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public SqlParameter AddParameterWithType(string key, SqlDbType type)
        {
            SqlParameter currentParameter = null;
            if (mCommand.Parameters.Contains(key))
            {
                currentParameter = mCommand.Parameters[key];
                currentParameter.SqlDbType = type;
            }
            else
            {
                currentParameter = mCommand.Parameters.Add(new SqlParameter(key, type));
            }
            return currentParameter;
        }

        public void SetParameterValue(string key, object value)
        {
            mCommand.Parameters[key].Value = value;
        }

        public SqlDataReader ExecuteReader(string cmdText)
        {
            return ExecuteReader(cmdText, 3);
        }

        public SqlDataReader ExecuteReader(string cmdText, int retryCount)
        {
            SqlDataReader dataReader = null;
            //    int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    mCommand.CommandText = cmdText;
                    dataReader = mCommand.ExecuteReader();
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    retryCount--;
                    if (retryCount <= 0 || !NeedRetry(sqlException))
                    {
                        throw;
                    }
                    mLog.Warn(string.Format("Retry Times:{0}\r\n Error:{1}, sql error code:{2}", retryCount, sqlException.ToString(), sqlException.Number));
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw;
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\nInvalidException:{1}", retryCount, invalidException.ToString()));
                    }
                    else
                    {
                        mLog.Error(string.Format("InvalidOperationException:{0}", invalidException.ToString()));
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(string.Format("{0}", ex.ToString()));
                    throw;
                }
            }
            return dataReader;
        }

        /*public SqlDataReader ExecuteReader(string cmdText, CommandBehavior bevavior)
        {
            return ExecuteReader(cmdText, bevavior, 3);
        }

        public SqlDataReader ExecuteReader(string cmdText, CommandBehavior behavior, int retryCount)
        {
            SqlDataReader dataReader = null;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    mCommand.CommandText = cmdText;
                    dataReader = mCommand.ExecuteReader(behavior);
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    retryCount--;
                    if (retryCount <= 0 || !NeedRetry(sqlException))
                    {
                        throw;
                    }
                    mLog.Warn(string.Format("Retry Times:{0}\r\n Error:{1}, sql error code:{2}", retryCount, sqlException.ToString(), sqlException.Number));
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw;
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\nInvalidException:{1}", retryCount, invalidException.ToString()));
                    }
                    else
                    {
                        mLog.Error(string.Format("InvalidOperationException:{0}", invalidException.ToString()));
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(string.Format("{0}", ex.ToString()));
                    throw;
                }
            }
            return dataReader;
        }*/

        /*public int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(cmdText, 5);
        }

        public int ExecuteNonQuery(string cmdText, int retryCount)
        {
            int queryResult = 0;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    mCommand.CommandText = cmdText;
                    queryResult = mCommand.ExecuteNonQuery();
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    retryCount--;
                    if (retryCount <= 0 || !NeedRetry(sqlException))
                    {
                        throw;
                    }
                    mLog.Warn(string.Format("Retry Times:{0}\r\n Error:{1}, sql error code:{2}", retryCount, sqlException.ToString(), sqlException.Number));
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw;
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\nInvalidException:{1}", retryCount, invalidException.ToString()));
                    }
                    else
                    {
                        mLog.Error(string.Format("InvalidOperationException:{0}", invalidException.ToString()));
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(string.Format("{0}", ex.ToString()));
                    throw;
                }
            }
            return queryResult;
        }*/

        /*public object ExecuteScalar(string cmdText)
        {
            return ExecuteScalar(cmdText, 3);
        }

        public object ExecuteScalar(string cmdText, int retryCount)
        {
            object queryResult = null;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    mCommand.CommandText = cmdText;
                    queryResult = mCommand.ExecuteScalar();
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    retryCount--;
                    if (retryCount <= 0 || !NeedRetry(sqlException))
                    {
                        throw;
                    }
                    mLog.Warn(string.Format("Retry Times:{0}\r\n Error:{1}, sql error code:{2}", retryCount, sqlException.ToString(), sqlException.Number));
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw;
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\nInvalidException:{1}", retryCount, invalidException.ToString()));
                    }
                    else
                    {
                        mLog.Error(string.Format("InvalidOperationException:{0}", invalidException.ToString()));
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(string.Format("{0}", ex.ToString()));
                    throw;
                }
            }
            return queryResult;
        }*/

        /*public IAsyncResult BeginExecuteNonQuery(string cmdText)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteNonQuery();
        }

        public IAsyncResult BeginExecuteNonQuery(string cmdText, AsyncCallback callback, object stateObject)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteNonQuery(callback, stateObject);
        }

        public int EndExecuteNonQuery(IAsyncResult asyncResule)
        {
            return mCommand.EndExecuteNonQuery(asyncResule);
        }

        public IAsyncResult BeginExecuteReader(string cmdText)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader();
        }

        public IAsyncResult BeginExecuteReader(string cmdText, CommandBehavior behavior)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader(behavior);
        }

        public IAsyncResult BeginExecuteReader(string cmdText, AsyncCallback callback, object stateObject)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader(callback, stateObject);
        }*/

        /*public IAsyncResult BeginExecuteReader(string cmdText, AsyncCallback callback, object stateObject, CommandBehavior behavior)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader(callback, stateObject, behavior);
        }*/

        public SqlDataReader EndExecuteReader(IAsyncResult asyncResule)
        {
            return mCommand.EndExecuteReader(asyncResule);
        }

        /*public void InsertTableRow(Dictionary<string, object> dic, string tableName)
        {
            string cmdText = BuildInsertCmdText(dic, tableName);

            ExecuteCmdText(cmdText);
        }*/

        private string BuildInsertCmdText(Dictionary<string, object> dic, string tableName)
        {
            StringBuilder cmdText = new StringBuilder();

            cmdText.Append("INSERT INTO ");
            cmdText.Append(tableName);
            cmdText.Append(" (");

            bool isFirstItem = true;

            string key;
            foreach (string tempKey in dic.Keys)
            {
                if (tempKey.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    key = tempKey.Substring(1);

                    if (isFirstItem)
                    {
                        isFirstItem = false;
                        cmdText.Append(key);
                    }
                    else
                    {
                        cmdText.Append(", ");
                        cmdText.Append(key);
                    }
                }
            }

            cmdText.Append(") VALUES (");

            isFirstItem = true;

            foreach (KeyValuePair<string, object> entry in dic)
            {
                string tempKey = (string)entry.Key;

                if (tempKey.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    key = tempKey.Substring(1);

                    if (isFirstItem)
                    {
                        isFirstItem = false;
                        if (entry.Value == null)
                        {
                            cmdText.Append("NULL");
                        }
                        else
                        {
                            key = "@" + key;
                            while (mCommand.Parameters.Contains(key))
                            {
                                key += "_";
                            }
                            mCommand.Parameters.AddWithValue(key, entry.Value);
                            cmdText.Append(key);
                        }
                    }
                    else
                    {
                        if (entry.Value == null)
                        {
                            cmdText.Append(", NULL");
                        }
                        else
                        {
                            cmdText.Append(", ");
                            key = "@" + key;
                            while (mCommand.Parameters.Contains(key))
                            {
                                key += "_";
                            }
                            mCommand.Parameters.AddWithValue(key, entry.Value);
                            cmdText.Append(key);
                        }
                    }
                }
            }

            cmdText.Append(")");
            return cmdText.ToString();
        }

        /*/// <summary>
        /// not updateColumns format is :  null, "", ",Id," or ",Id,Name,Age,"
        /// </summary>
        /// <param name="ht"></param>
        /// <param name="tableName"></param>
        /// <param name="notUpdateColumns"></param>
        /// <param name="whereCmdText"></param>
        public void UpdateTableRow(Dictionary<string, object> dic, string tableName, string notUpdateColumns, string whereCmdText)
        {
            string cmdText = BuildUpdateCmdText(dic, tableName, notUpdateColumns, whereCmdText);

            ExecuteCmdText(cmdText);
        }

        private void ExecuteCmdText(string cmdText)
        {
            int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    mCommand.CommandText = cmdText;
                    mCommand.ExecuteNonQuery();
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    retryCount--;
                    if (retryCount <= 0 && !NeedRetry(sqlException))
                    {
                        throw;
                    }
                    mLog.Warn(string.Format("Retry Times:{0}\r\n Error:{1}, sql error code:{2}", retryCount, sqlException.ToString(), sqlException.Number));
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw;
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\nInvalidException:{1}", retryCount, invalidException.ToString()));
                    }
                    else
                    {
                        mLog.Error(string.Format("InvalidOperationException:{0}", invalidException.ToString()));
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(string.Format("{0}", ex.ToString()));
                    throw;
                }
            }
        }*/

        private string BuildUpdateCmdText(Dictionary<string, object> dic, string tableName, string notUpdateColumns, string whereCmdText)
        {
            if (notUpdateColumns == null)
                notUpdateColumns = "";

            StringBuilder cmdText = new StringBuilder();

            cmdText.Append("UPDATE ");
            cmdText.Append(tableName);
            cmdText.Append(" SET ");

            bool isFirst = true;

            foreach (KeyValuePair<string, object> entry in dic)
            {
                string tempKey = (string)(entry.Key);

                if (tempKey[0] == '#')
                {
                    string key = tempKey.Substring(1);
                    if (notUpdateColumns.IndexOf("," + key + ",", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            cmdText.Append(", ");
                        }
                        if (entry.Value == null)
                        {
                            cmdText.Append(key + "=NULL");
                        }
                        else
                        {
                            cmdText.Append(key);
                            key = "@" + key;
                            while (mCommand.Parameters.Contains(key))
                            {
                                key += "_";
                            }
                            cmdText.Append("=");
                            cmdText.Append(key);
                            mCommand.Parameters.AddWithValue(key, entry.Value);
                        }
                    }
                }
            }
            cmdText.Append(" ");
            cmdText.Append(whereCmdText);

            return cmdText.ToString();
        }
        #endregion

        private void InitConnectionString(ref string connString, ref string server, ref string database)
        {
            if (string.IsNullOrEmpty(connString))
            {
                return;
            }
            try
            {
                string[] quotSplits = connString.Split(';');
                StringBuilder sb = new StringBuilder();
                foreach (string s in quotSplits)
                {
                    if (s.StartsWith("timeout=", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("Timeout=");
                        sb.Append(mTimeout);
                        sb.Append(";");
                        continue;
                    }
                    sb.Append(s);
                    sb.Append(";");
                    if (s.IndexOf('=') < 0)
                    {
                        continue;
                    }
                    string[] equalsSplits = s.Split('=');
                    string key = equalsSplits[0];
                    string value = equalsSplits[1];
                    if (key.Equals("server", StringComparison.OrdinalIgnoreCase)
                        || key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                    {
                        server = value;
                    }
                    if (key.Equals("database", StringComparison.OrdinalIgnoreCase)
                        || key.Equals("Initial Catalog", StringComparison.OrdinalIgnoreCase))
                    {
                        database = value;
                    }
                }
                connString = sb.ToString().TrimEnd(';');
            }
            catch (Exception e)
            { mLog.Warn(e.ToString()); }
        }
        private bool NeedRetry(SqlException sqlException)
        {
            if (sqlException.IsConnectionError())
            {
                ResetConnection(false, true);
            }
            else if (sqlException.IsOperationTimedOut())
            {
                mCommand.CommandTimeout += 60;
            }
            else
            {
                switch (sqlException.Number)
                {
                    case SQL_DEAD_LOCK:
                        Thread.Sleep(1000);
                        break;
                    //TODO need to handler other case like primary key
                    default:
                        mLog.Error(string.Format("An error occurred in execute reader:{0}", sqlException.ToString()));
                        return false;
                }
            }
            return true;
        }
    }
}
