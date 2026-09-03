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
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 这个类就是从AveSqlConnection拷过来的
    /// </summary>
    public class AveDiscoverConnection : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveDiscoverConnection));

        public const int DEFAULT_TIMEOUT = 180;
        public const int DEFAULT_COMMAND_TIMEOUT = 300;
        public const int DEAD_LOCK = 1205;

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
        public AveDiscoverConnection()
        {
        }

        public AveDiscoverConnection(string connString)
        {
            Open(connString);
        }

        public AveDiscoverConnection(string connString, int timeout)
        {
            Open(connString, timeout);
        }
        #endregion

        #region Open & Close

        private void InitConnectionString(ref string connString, ref string server, ref string database)
        {
            if (string.IsNullOrEmpty(connString))
            {
                return;
            }
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
            mConnection.Open();
            mCommand = mConnection.CreateCommand();

            SetCommandTimeout(DEFAULT_COMMAND_TIMEOUT);
        }
        public void SetCommandTimeout(int seconds)
        {
            Command.CommandTimeout = seconds;
        }

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
                    mConnection.Dispose();
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
            if (mCommand != null)
            {
                mCommand.Dispose();
            }
            if (mConnection != null)
            {
                mConnection.Dispose();
            }
        }
        #endregion
        #region Methods from Command
        public void ClearParameters()
        {
            mCommand.Parameters.Clear();
        }

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
                catch (SqlException exception)
                {
                    switch (exception.Number)
                    {
                        case DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw;
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\nDead lock exception:{1}", retryCount, exception.ToString()));
                            Thread.Sleep(1000);
                            break;
                        //TODO need to handler other case like primary key
                        default:
                            mLog.Error(string.Format("An error occurred in execute reader:{0}", exception.ToString()));
                            throw;
                    }
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
        public SqlDataReader ExecuteReader(string cmdText, CommandBehavior bevavior)
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
                catch (SqlException exception)
                {
                    switch (exception.Number)
                    {
                        case DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw;
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\nDead lock exception:{1}", retryCount, exception.ToString()));
                            Thread.Sleep(1000);
                            break;
                        //TODO need to handler other case like primary key
                        default:
                            mLog.Error(string.Format("An error occurred in execute reader:{0}", exception.ToString()));
                            throw;
                    }
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
        #endregion

        public object ExecuteScalar(string cmdText)
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
                catch (SqlException exception)
                {
                    switch (exception.Number)
                    {
                        case DEAD_LOCK:
                            retryCount--;
                            if (retryCount <= 0)
                            {
                                throw;
                            }
                            mLog.Warn(string.Format("Retry Times:{0}\r\nDead lock exception:{1}", retryCount, exception.ToString()));
                            Thread.Sleep(1000);
                            break;
                        //TODO need to handler other case like primary key
                        default:
                            mLog.Error(string.Format("An error occurred in execute reader:{0}", exception.ToString()));
                            throw;
                    }
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
        }
    }
}
