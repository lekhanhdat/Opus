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
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.Common
{
    internal enum VersionOption
    {
        None = 0,
        OneItemOrVersion,
        AllPublishingVersions,
        AllVersions           //AllUserData,AllUserDataJunctions
    }

    internal enum RowOrdinalOption
    {
        None,   //AllDocs,AllDocVersions,DocStreams etc
        AllUserDataOneRow,     //AllUseData tp_RowOrdinal
        AllUserDataAllRows,    //AllUserData tp_RowOrdinal
    }

    internal class AveQueryWorker : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveQueryWorker));

        public static int DEFAULT_TIMEOUT = WrapperConfiguration.QueryServiceConnectTimeout;
        public static int DEFAULT_COMMAND_TIMEOUT = WrapperConfiguration.QueryServiceCommandTimeout;//300 seconds
        private static string[] timeoutProperties = { "Connect Timeout", "Connection Timeout", "Timeout" };
        private AveQueryCounter queryCounter = new AveQueryCounter();
        Guid queryGuid;

        #region --Query Exception Number--

        private const int QUERY_SERVER_NOTEXIST_ACCESS_DENIED = 17;
        private const int QUERY_FOREIGN_KEY_VIOLATION = 547;
        private const int QUERY_DEAD_LOCK = 1205;
        private const int QUERY_UNIQUE_CONSTRAINT_VIOLATION = 2601;
        private const int QUERY_UNIQUE_CONSTRAINT_VIOLATION_1 = 2627;
        private const int QUERY_INVALID_DATABASE = 4060;
        private const int QUERY_LOGIN_FAILED = 18546;
        private const int QUERY_CONNECTION_FAILED = 2;

        #endregion

        #region Propertis

        private int mTimeout;
        private string mServer;
        private string mDatabase;
        /// <summary>
        /// only for reset sql connection
        /// </summary>
        private string mConnectionString;

        internal string ConnectionString
        {
            get { return mConnectionString; }
        }

        private SqlConnection mConnection;

        internal SqlConnection Connection
        {
            get
            {
                return mConnection;
            }
        }

        private SqlCommand mCommand;

        internal SqlCommand Command
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

        private SqlTransaction mTransaction;
        #endregion

        #region Constructor

        public AveQueryWorker()
        { }

        public AveQueryWorker(string connString)
            : this(connString, DEFAULT_TIMEOUT)
        { }

        public AveQueryWorker(string connString, int timeout)
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
            queryGuid = Guid.NewGuid();
            queryCounter.AddConnectionRecord(queryGuid);
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
            try
            {
                mConnection = new SqlConnection(connString);
                mServer = server;
                mDatabase = database;
                mConnectionString = connString;
                mConnection.Open();
                mCommand = mConnection.CreateCommand();

                SetCommandTimeout(DEFAULT_COMMAND_TIMEOUT);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        private void SetCommandTimeout(int seconds)
        {
            Command.CommandTimeout = seconds;
        }

        internal void SetIsolateLevel(IsolationLevel level)
        {
            mTransaction = mConnection.BeginTransaction(level);
            mCommand.Transaction = mTransaction;
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
        private void ResetConnection(bool resetCommand)
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
                    catch (Exception ex)
                    {
                        mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                    }
                    mConnection = new SqlConnection(mConnectionString);
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
            queryCounter.RemoveConnectionRecord(queryGuid);
            if (mCommand != null)
            {
                try
                {
                    mCommand.Dispose();
                }
                catch (Exception ex)
                {
                    mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                }
                mCommand = null;
            }
            if (mConnection != null)
            {
                try
                {
                    mConnection.Dispose();
                }
                catch (Exception ex)
                {
                    mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                }
                mConnection = null;
            }
        }
        #endregion

        # region Private Methods

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
                    if (ReplaceTimeoutProperty(sb, s))
                    {
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
            catch (Exception e) // Because connection string may have password, so do not log error message
            {
                mLog.Log(AveLogLevel.DEBUG, WrapperQueryServiceResource.InitConnectionStringError, e.ToString());
            }
        }

        private bool ReplaceTimeoutProperty(StringBuilder sb, string s)
        {
            foreach (string timeoutProperty in timeoutProperties)
            {
                if (s.StartsWith(timeoutProperty, StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(timeoutProperty);
                    sb.Append("=");
                    sb.Append(mTimeout);
                    sb.Append(";");
                    return true;
                }
            }
            return false;
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
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(string.Format("Exception Error Code : {0}", sqlException.Number), sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code : {0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code : {0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
        }

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

        /// <summary>
        /// 获取期望的Update影响行数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="versionOption"></param>
        /// <param name="rowOrdinal"></param>
        /// <returns></returns>
        private int GetExpectedUpdateAffectedRows(SqlCommand command, VersionOption versionOption, RowOrdinalOption rowOrdinal)
        {
            int expectedRows = -1;
            string cmdText = command.CommandText;
            try
            {
                switch (versionOption)
                {
                    case VersionOption.OneItemOrVersion:
                        switch (rowOrdinal)
                        {
                            case RowOrdinalOption.None:
                            case RowOrdinalOption.AllUserDataOneRow:
                                expectedRows = 1;
                                break;
                            case RowOrdinalOption.AllUserDataAllRows:
                                using (SqlCommand cmd = command.Connection.CreateCommand())
                                {
                                    foreach (SqlParameter param in command.Parameters)
                                    {
                                        cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                                    }
                                    cmd.CommandText = ChangeUpdateToSelect(cmdText, AveUpdateCheckString.OneItemOrVersionSelectPatternForAllUserData, AveUpdateCheckString.OneItemOrVersionForAllUserDataAllRows);
                                    expectedRows = (int)cmd.ExecuteScalar();
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case VersionOption.AllPublishingVersions:
                        switch (rowOrdinal)
                        {
                            case RowOrdinalOption.None:
                                using (SqlCommand cmd = command.Connection.CreateCommand())
                                {
                                    foreach (SqlParameter param in command.Parameters)
                                    {
                                        cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                                    }
                                    cmd.CommandText = ChangeUpdateToSelect(cmdText, AveUpdateCheckString.AllPublishingVersionsSelectPatternForAllDocs, AveUpdateCheckString.AllPublishingVersionsForAllDocs);

                                    expectedRows = (int)cmd.ExecuteScalar();
                                }
                                break;
                            case RowOrdinalOption.AllUserDataOneRow:
                                using (SqlCommand cmd = command.Connection.CreateCommand())
                                {
                                    foreach (SqlParameter param in command.Parameters)
                                    {
                                        cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                                    }
                                    cmd.CommandText = ChangeUpdateToSelect(cmdText, AveUpdateCheckString.AllPublishingVersionsSelectPatternForAllUserData, AveUpdateCheckString.AllPublishingVersionsForAllUserDataOneRow);
                                    expectedRows = (int)cmd.ExecuteScalar();
                                }
                                break;
                            case RowOrdinalOption.AllUserDataAllRows:
                                using (SqlCommand cmd = command.Connection.CreateCommand())
                                {
                                    foreach (SqlParameter param in command.Parameters)
                                    {
                                        cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                                    }
                                    cmd.CommandText = ChangeUpdateToSelect(cmdText, AveUpdateCheckString.AllPublishingVersionsSelectPatternForAllUserData, AveUpdateCheckString.AllPublishingVersionsForAllUserDataAllRows);
                                    expectedRows = (int)cmd.ExecuteScalar();
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case VersionOption.AllVersions://AllUserData
                        switch (rowOrdinal)
                        {
                            case RowOrdinalOption.AllUserDataOneRow:
                                using (SqlCommand cmd = command.Connection.CreateCommand())
                                {
                                    foreach (SqlParameter param in command.Parameters)
                                    {
                                        cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                                    }
                                    cmd.CommandText = ChangeUpdateToSelect(cmdText, AveUpdateCheckString.AllVersionsSelectPatternForAllUserDataAllRows, AveUpdateCheckString.AllVersionsForAllUserDataOneRow);
                                    expectedRows = (int)cmd.ExecuteScalar();
                                }
                                break;
                            case RowOrdinalOption.AllUserDataAllRows:
                                using (SqlCommand cmd = command.Connection.CreateCommand())
                                {
                                    foreach (SqlParameter param in command.Parameters)
                                    {
                                        cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                                    }
                                    cmd.CommandText = ChangeUpdateToSelect(cmdText, AveUpdateCheckString.AllVersionsSelectPatternForAllUserDataAllRows, AveUpdateCheckString.AllVersionsForAllUserDataAllRows);
                                    expectedRows = (int)cmd.ExecuteScalar();
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        expectedRows = 0;
                        break;
                };
            }
            catch (Exception e)
            {
                mLog.Debug(WrapperQueryServiceResource.GetExpectedUpdateRowsError, AveQueryException.InternalCrypto.EncryptMessage(cmdText), AveQueryException.WrapperException(e));
            }
            return expectedRows;
        }

        /// <summary>
        /// 将Update语句转换成满足特定条件的Select语句
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pattern"></param>
        /// <param name="updateCheckString"></param>
        /// <returns></returns>
        private string ChangeUpdateToSelect(string cmdText, string pattern, string updateCheckString)
        {
            StringBuilder sBuilder = new StringBuilder();
            sBuilder.Append(@"DECLARE @_SiteId uniqueIdentifier, 
                              @_ParentId uniqueIdentifier, 
                              @_DocId uniqueIdentifier, 
                              @_IsCurrentVersion BIT, 
                              @_Level INT, 
                              @_CalculatedVersion INT ");//"_"防止参数重复
            cmdText = Regex.Replace(cmdText, "Update", pattern, RegexOptions.IgnoreCase);
            int indexofSet = 0;
            int indexofWhere = 0;
            indexofSet = cmdText.IndexOf("SET", StringComparison.OrdinalIgnoreCase);
            indexofWhere = cmdText.IndexOf("WHERE", indexofSet + 3, StringComparison.OrdinalIgnoreCase);
            if (indexofSet >= 0 && indexofWhere >= 0)
            {
                cmdText = cmdText.Remove(indexofSet, indexofWhere - indexofSet);
            }
            sBuilder.Append(cmdText);
            sBuilder.Append(" \r\n");
            sBuilder.Append(updateCheckString);
            return sBuilder.ToString();
        }

        # endregion

        #region Methods from SqlCommand

        public void ClearParameters()
        {
            mCommand.Parameters.Clear();
        }

        /// <summary>
        /// clear parameter, and set the command type
        /// </summary>
        /// <param name="commandType"></param>
        public void ResetCommand(CommandType commandType)
        {
            ClearParameters();
            mCommand.CommandType = commandType;
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
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public SqlParameter AddParameter(string key, object value,ParameterDirection direction)
        {
            SqlParameter currentParameter = AddParameter(key, value);
            currentParameter.Direction = direction;
            return currentParameter;
        }

        /// <summary>
        /// batch add parameters
        /// </summary>
        /// <param name="parameters"></param>
        public void AddParameters(IDictionary<string,object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return;
            }
            foreach (var key in parameters.Keys)
            {
                AddParameter(key, parameters[key]);
            }
        }

        /// <summary>
        /// batch add parameters
        /// </summary>
        /// <param name="parameters"></param>
        public void AddParameters(Hashtable parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return;
            }
            foreach (string key in parameters.Keys)
            {
                AddParameter(key, parameters[key]);
            }
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

        public SqlParameter AddParameterWithType(string key, SqlDbType type, ParameterDirection direction)
        {
            var currentParameter = AddParameterWithType(key, type);
            currentParameter.Direction = direction;
            return currentParameter;
        }

        /// <summary>
        /// 添加SqlCommand参数，指定key,value和Type
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddParameter(string key, SqlDbType type, object value)
        {
            SqlParameter currentParameter = null;
            currentParameter = AddParameterWithType(key, type);
            currentParameter.Value = value;
        }

        public void SetParameterValue(string key, object value)
        {
            mCommand.Parameters[key].Value = value;
        }

        public SqlDataReader ExecuteReader(string cmdText)
        {
            return ExecuteReader(cmdText, 3);
        }

        public SqlDataReader ExecuteReader(SqlCommand command)
        {
            return ExecuteReader(3, command);
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
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return dataReader;
        }

        public SqlDataReader ExecuteReader(int retryCount, SqlCommand command)
        {
            SqlDataReader dataReader = null;
            //    int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    dataReader = command.ExecuteReader();
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        if (mConnection != null)
                        {
                            try
                            {
                                mConnection.Dispose();
                            }
                            catch (Exception ex)
                            {
                                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                            }
                            mConnection = new SqlConnection(mConnectionString);
                            mConnection.Open();
                            command.Connection = mConnection;
                        }
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return dataReader;

        }

        public SqlDataReader ExecuteReader(string cmdText, CommandBehavior bevavior)
        {
            return ExecuteReader(cmdText, bevavior, 3);
        }

        public SqlDataReader ExecuteReader(CommandBehavior bevavior, SqlCommand command)
        {
            return ExecuteReader(bevavior, 3, command);
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
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return dataReader;
        }

        public SqlDataReader ExecuteReader(CommandBehavior behavior, int retryCount, SqlCommand command)
        {
            SqlDataReader dataReader = null;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    dataReader = command.ExecuteReader(behavior);
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        //ResetConnection(false);
                        if (mConnection != null)
                        {
                            try
                            {
                                mConnection.Dispose();
                            }
                            catch (Exception ex)
                            {
                                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                            }
                            mConnection = new SqlConnection(mConnectionString);
                            mConnection.Open();
                            command.Connection = mConnection;
                        }
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return dataReader;
        }

        public int ExecuteNonQuery()
        {
            return ExecuteNonQuery(this.Command.CommandText);
        }

        public int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(cmdText, 3);
        }

        public int ExecuteNonQuery(SqlCommand command)
        {
            return ExecuteNonQuery(3, command);
        }

        public int ExecuteNonQuery(SqlCommand command, VersionOption versionOption, RowOrdinalOption rowOrdinal)
        {
            return ExecuteNonQuery(3, command, versionOption, rowOrdinal);
        }

        /// <summary>
        /// 此方法适用于where条件长度可变。目前只支持一个参数拼接。
        /// 比如collection = {1,2,3}, command = ....tp_Id in {0}
        /// 拼接之后： ....tp_Id in {'1','2','3'}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">参数集合</param>
        /// <param name="eachCount">由于CommandText size问题，由此属性指定每次Execute条目数量</param>
        /// <param name="command"></param>
        /// <returns></returns>
        public int ExecuteNonQueryByCount<T>(IEnumerable<T> collection, int eachCount, string command)
        {
            return ExecuteNonQueryByCount(collection, eachCount, ExecuteNonQuery, command, "'{0}',", p => p.Length -= 1);
        }
        private int ExecuteNonQueryByCount<T>(IEnumerable<T> collection, int eachCount, Func<string, int> fun, string command, string parameterFormat, Action<StringBuilder> actionForParameter = null)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (fun == null)
            {
                throw new ArgumentNullException("fun");
            }
            if (eachCount <= 0)
            {
                throw new ArgumentException("Value must be greater than 0.", "count");
            }
            int result = 0;
            int count = 0;
            StringBuilder parameterString = new StringBuilder();
            foreach (T t in collection)
            {
                parameterString.AppendFormat(parameterFormat, t.ToString());
                count++;
                if (count % eachCount == 0)
                {
                    if(actionForParameter!=null)
                    {
                        actionForParameter(parameterString);
                    }
                    result += fun(string.Format(command, parameterString));
                    parameterString.Length = 0;
                }
            }
            if (parameterString.Length != 0)
            {
                if (actionForParameter != null)
                {
                    actionForParameter(parameterString);
                }
                result += fun(string.Format(command, parameterString));
            }
            return result;
        }

        public int ExecuteNonQuery(int retryCount, SqlCommand command, VersionOption versionOption, RowOrdinalOption rowOrdinal)
        {
            int actualAffectedRows = 0;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    actualAffectedRows = command.ExecuteNonQuery();
                    int expectedRows = GetExpectedUpdateAffectedRows(command, versionOption, rowOrdinal);
                    if (actualAffectedRows != expectedRows)
                    {
                        mLog.Warn(WrapperQueryServiceResource.UpdateCheckError, AveQueryException.InternalCrypto.EncryptMessage(command.CommandText), expectedRows, actualAffectedRows);
                    }
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        //ResetConnection(false);
                        if (mConnection != null)
                        {
                            try
                            {
                                mConnection.Dispose();
                            }
                            catch (Exception ex)
                            {
                                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                            }
                            mConnection = new SqlConnection(mConnectionString);
                            mConnection.Open();
                            command.Connection = mConnection;
                        }
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return actualAffectedRows;
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
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return queryResult;
        }

        public int ExecuteNonQuery(string cmdText, VersionOption versionOption, RowOrdinalOption rowOrdinal)
        {
            return ExecuteNonQuery(cmdText, 3, versionOption, rowOrdinal);
        }

        public int ExecuteNonQuery(string cmdText, int retryCount, VersionOption versionOption, RowOrdinalOption rowOrdinal)
        {
            int actualAffectedRows = 0;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    mCommand.CommandText = cmdText;
                    actualAffectedRows = mCommand.ExecuteNonQuery();
                    int expectedRows = GetExpectedUpdateAffectedRows(mCommand, versionOption, rowOrdinal);
                    if (actualAffectedRows != expectedRows)
                    {
                        mLog.Warn(WrapperQueryServiceResource.UpdateCheckError, AveQueryException.InternalCrypto.EncryptMessage(cmdText), expectedRows, actualAffectedRows);
                    }
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return actualAffectedRows;
        }

        public int ExecuteNonQuery(int retryCount, SqlCommand command)
        {
            int queryResult = 0;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    queryResult = command.ExecuteNonQuery();
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        //ResetConnection(false);
                        if (mConnection != null)
                        {
                            try
                            {
                                mConnection.Dispose();
                            }
                            catch (Exception ex)
                            {
                                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                            }
                            mConnection = new SqlConnection(mConnectionString);
                            mConnection.Open();
                            command.Connection = mConnection;
                        }
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return queryResult;
        }

        public object ExecuteScalar(string cmdText)
        {
            return ExecuteScalar(cmdText, 3);
        }

        public object ExecuteScalar(SqlCommand command)
        {
            return ExecuteScalar(3, command);
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
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        ResetConnection(false);
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return queryResult;
        }

        public object ExecuteScalar(int retryCount, SqlCommand command)
        {
            object queryResult = null;
            //int retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    queryResult = command.ExecuteScalar();
                    retryCount = 0;
                    break;
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case QUERY_DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw new AveQueryException(sqlException);
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            Thread.Sleep(1000);
                            break;
                        case QUERY_CONNECTION_FAILED:
                            if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                            {
                                retryCount--;
                                if (retryCount <= 0)
                                {
                                    throw new AveQueryException(sqlException);
                                }
                                ResetConnection(false);
                                mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString()));
                            }
                            else
                            {
                                mLog.Warn("Unknown connection status:{0}", mConnection.State.ToString());
                                mLog.Error(new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException).ToString());
                                throw new AveQueryException(sqlException);
                            }
                            break;
                        default:
                            AveQueryException queryException = new AveQueryException(string.Format("Exception Error Code----{0}", sqlException.Number), sqlException);
                            mLog.Error(queryException.ToString());
                            throw queryException;
                    }
                }
                catch (InvalidOperationException invalidException)
                {
                    if (mConnection.State == ConnectionState.Closed || mConnection.State == ConnectionState.Broken)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw new AveQueryException(invalidException.Message, invalidException);
                        }
                        //ResetConnection(false);
                        if (mConnection != null)
                        {
                            try
                            {
                                mConnection.Dispose();
                            }
                            catch (Exception ex)
                            {
                                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConnectionDisposeError, ex);
                            }
                            mConnection = new SqlConnection(mConnectionString);
                            mConnection.Open();
                            command.Connection = mConnection;
                        }
                        mLog.Warn(string.Format("Retry Times:{0}\r\n{1}", retryCount, new AveQueryException(invalidException.Message, invalidException).ToString()));
                    }
                    else
                    {
                        AveQueryException queryException = new AveQueryException(invalidException.Message, invalidException);
                        mLog.Error(queryException.ToString());
                        throw queryException;
                    }
                }
                catch (Exception ex)
                {
                    AveQueryException queryException = new AveQueryException(ex.Message, ex);
                    mLog.Error(queryException.ToString());
                    throw queryException;
                }
            }
            return queryResult;
        }

        public IAsyncResult BeginExecuteNonQuery(string cmdText)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteNonQuery();
        }

        public IAsyncResult BeginExecuteNonQuery(SqlCommand command)
        {
            return command.BeginExecuteNonQuery();
        }

        public IAsyncResult BeginExecuteNonQuery(string cmdText, AsyncCallback callback, object stateObject)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteNonQuery(callback, stateObject);
        }

        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback, object stateObject, SqlCommand command)
        {
            return command.BeginExecuteNonQuery(callback, stateObject);
        }

        public int EndExecuteNonQuery(IAsyncResult asyncResule)
        {
            return mCommand.EndExecuteNonQuery(asyncResule);
        }

        public int EndExecuteNonQuery(IAsyncResult asyncResule, SqlCommand command)
        {
            return command.EndExecuteNonQuery(asyncResule);
        }

        public IAsyncResult BeginExecuteReader(string cmdText)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader();
        }

        public IAsyncResult BeginExecuteReader(SqlCommand command)
        {
            return command.BeginExecuteReader();
        }

        public IAsyncResult BeginExecuteReader(string cmdText, CommandBehavior behavior)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader(behavior);
        }

        public IAsyncResult BeginExecuteReader(CommandBehavior behavior, SqlCommand command)
        {
            return command.BeginExecuteReader(behavior);
        }

        public IAsyncResult BeginExecuteReader(string cmdText, AsyncCallback callback, object stateObject)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader(callback, stateObject);
        }

        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject, SqlCommand command)
        {
            return command.BeginExecuteReader(callback, stateObject);
        }

        public IAsyncResult BeginExecuteReader(string cmdText, AsyncCallback callback, object stateObject, CommandBehavior behavior)
        {
            mCommand.CommandText = cmdText;
            return mCommand.BeginExecuteReader(callback, stateObject, behavior);
        }

        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject, CommandBehavior behavior, SqlCommand command)
        {
            return command.BeginExecuteReader(callback, stateObject, behavior);
        }

        public SqlDataReader EndExecuteReader(IAsyncResult asyncResule)
        {
            return mCommand.EndExecuteReader(asyncResule);
        }

        public SqlDataReader EndExecuteReader(IAsyncResult asyncResule, SqlCommand command)
        {
            return command.EndExecuteReader(asyncResule);
        }

        public void InsertTableRow(Dictionary<string, object> dic, string tableName)
        {
            string cmdText = BuildInsertCmdText(dic, tableName);

            ExecuteCmdText(cmdText);
        }

        public SqlCommand CreateCommand()
        {
            return CreateCommand(DEFAULT_COMMAND_TIMEOUT);
        }

        public SqlCommand CreateCommand(int defaultCommandTimeout)
        {
            SqlCommand cmd = mConnection.CreateCommand();
            cmd.CommandTimeout = defaultCommandTimeout;
            if (mTransaction != null)
            {
                cmd.Transaction = mTransaction;
            }
            return cmd;
        }

        /// <summary>
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

        #endregion
    }
}
